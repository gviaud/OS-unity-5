using UnityEngine;
using System.Collections;


public class NNF {
	
	// image 
	public MaskedImage input, output;
	
	//  patch size
	public int S;

	// Nearest-Neighbor Field 1 pixel = { x_target, y_target, distance_scaled } 
	public int[,,] field;
	

	// constructor
	public NNF(MaskedImage input, MaskedImage output, int patchsize) {
		this.input = input;
		this.output= output;
		this.S = patchsize;	
	}
	
	// initialize field with random values
	public void randomize() {
		// field
		this.field = new int[input.width,input.height,3];
		
		for(int y=0;y<input.height;y++) {
			for(int x=0;x<input.width;x++) {
				field[x,y,0] = Random.Range(0, output.width);  
				field[x,y,1] = Random.Range(0,output.height);
				field[x,y,2] = MaskedImage.DSCALE;
			}
		}
		
		initialize();
	}
	
	// initialize field from an existing (possibily smaller) NNF
	public void initialize(NNF nnf) {
		// field
		this.field = new int[input.width,input.height,3];
		this.input = nnf.input;
		this.output = nnf.output;
		this.S = nnf.S;
		
		int fx = input.width/nnf.input.width;
		int fy = input.height/nnf.input.height;
		//System.out.println("nnf upscale by "+fx+"x"+fy+" : "+nnf.input.W+","+nnf.input.H+" -> "+input.W+","+input.H);
		for(int y=0;y<input.height;y++) {
			for(int x=0;x<input.width;x++) {
				int xlow = Mathf.Min(x/fx, nnf.input.width-1);
				int ylow = Mathf.Min(y/fy, nnf.input.height-1);
				field[x,y,0] = nnf.field[xlow,ylow,0]*fx;  
				field[x,y,1] = nnf.field[xlow,ylow,1]*fy;
				field[x,y,2] = MaskedImage.DSCALE;
			}
		}
		
		initialize();

	}
	
	// compute initial value of the distance term
	private void initialize() {
		for(int y=0;y<input.height;y++) {
			for(int x=0;x<input.width;x++) {
				//Debug.Log("fieldxy2 "+input.width+"/"+input.height+"= distance ("+x+"/"+y );
				field[x,y,2] = MaskedImage.distance(input, x,y, output, field[x,y,0],field[x,y,1], S);

				// if the distance is INFINITY (all pixels masked ?), try to find a better link
				int iter=0, maxretry=20;
				while( field[x,y,2] == MaskedImage.DSCALE && iter<maxretry) {
					field[x,y,0] = Random.Range(0,output.width);
					field[x,y,1] = Random.Range(0,output.height);
					field[x,y,2] = MaskedImage.distance(input, x,y, output, field[x,y,0],field[x,y,1], S);
					iter++;
				}
			}
		}
	}
	
	// multi-pass NN-field minimization (see "PatchMatch" - page 4)
	public void minimize(int pass) {
		
		int min_x=0, min_y=0, max_x=input.width-1, max_y=input.height-1;
		
		// multi-pass minimization
		for(int i=0;i<pass;i++) {
			// scanline order
			for(int y=min_y;y<max_y;y++)
				for(int x=min_x;x<=max_x;x++)
					if (field[x,y,2]>0) minimizeLink(x,y,+1);

			// reverse scanline order
			for(int y=max_y;y>=min_y;y--)
				for(int x=max_x;x>=min_x;x--)
					if (field[x,y,2]>0) minimizeLink(x,y,-1);
		}
	}

	// minimize a single link (see "PatchMatch" - page 4)
	public void minimizeLink(int x, int y, int dir) {
		int xp,yp,dp;
		
		//Propagation Left/Right
		if (x-dir>0 && x-dir<input.width) {
			xp = field[x-dir,y,0]+dir;
			yp = field[x-dir,y,1];
			dp = MaskedImage.distance(input, x,y, output, xp,yp, S);
			if (dp<field[x,y,2]) {
				field[x,y,0] = xp;
				field[x,y,1] = yp;
				field[x,y,2] = dp;
			}
		}
		
		//Propagation Up/Down
		if (y-dir>0 && y-dir<input.height) {
			xp = field[x,y-dir,0];
			yp = field[x,y-dir,1]+dir;
			dp = MaskedImage.distance(input, x,y, output, xp,yp, S);
			if (dp<field[x,y,2]) {
				field[x,y,0] = xp;
				field[x,y,1] = yp;
				field[x,y,2] = dp;
			}
		}
		
		//Random search
		int wi=output.width, xpi=field[x,y,0], ypi=field[x,y,1];
		while(wi>0) {
			xp = xpi + Random.Range(0,2*wi)-wi;
			yp = ypi + Random.Range(0,2*wi)-wi;
			xp = Mathf.Max(0, Mathf.Min(output.width-1, xp ));
			yp = Mathf.Max(0, Mathf.Min(output.height-1, yp ));
			
			dp = MaskedImage.distance(input, x,y, output, xp,yp, S);
			if (dp<field[x,y,2]) {
				field[x,y,0] = xp;
				field[x,y,1] = yp;
				field[x,y,2] = dp;
			}
			wi/=2;
		}
	}

	// compute distance between two patch 
	public int distance(int x,int y, int xp,int yp) {
		return MaskedImage.distance(input,x,y, output,xp,yp, S);
	}
	
	public int[,,] getField() {
		return field;
	}
	

	
}
