using UnityEngine;
using System.Collections;
using System;


public class Inpaint
{
	//initial image
	public MaskedImage initial;         // Image donnée au constructeur (partie masquée en rouge à l'origine)
	public MaskedImage source;          // Copie de initial, image "buffer", utilisée pour créer la pyramide
	public MaskedImage newsource;
	public MaskedImage newtarget;
	public MaskedImage target;
	
	public image result;
	
	// Nearest-Neighbor Fields
	//public NNF nnf_SourceToTarget;
	public NNF nnf_TargetToSource;
	
	// patch radius
	public int radius;
	
	public int maxlevel;
	
	// Pyramid of downsampled initial images (1/2 each time)
	public MaskedImage[] pyramid;       // Remplie avec source puis des downsample successifs
	
	//-----------------------------------------------------
    public Inpaint(image input, bool[,] mask, int radius)
    {
		// initial image
		this.initial = new MaskedImage(input, mask);

		// patch radius
		this.radius = radius;
		int i = 0;
        
		// working copies
		source = new MaskedImage(initial.width, initial.height);
		source = initial;
		
		target = new MaskedImage(source.width, source.height);
		
		Debug.Log("build pyramid of images...");
		
		// build pyramid of downscaled images
		pyramid = new MaskedImage[12];
		this.pyramid[i] = source;
		
		//Debug.Log("pyramid0:"+ pyramid[0].width+"/"+pyramid[0].height);
		while(source.width>radius && source.height>radius)
        {
			source = source.downsample();
			i++;
			this.pyramid[i] = new MaskedImage(source.width, source.height);
			this.pyramid[i] = source;
		}
		maxlevel = i+1;
	}
	
    //-----------------------------------------------------
	// EM-Like algorithm (see "PatchMatch" - page 6)
	// Returns a double sized target image
	public MaskedImage ExpectationMaximization(int level)
    {
		int iterEM = 1+level/2;
		int iterNNF = Mathf.Min(7,1+level/2);
		
//		source = nnf_SourceToTarget.input;
//		target = nnf_SourceToTarget.output;
		newtarget = null;
		
//		Debug.Log("source: "+source.width+"/"+source.height);
//		Debug.Log("target: "+target.width+"/"+target.height);
		
		Debug.Log("EM loop (em="+iterEM+",nnf="+iterNNF+") : ");
		
		// EM Loop
		for(int emloop=1;emloop<=iterEM;emloop++) {

			Debug.Log((1+iterEM-emloop)+" ");
			
			// set the new target as current target
			if (newtarget!=null) {
				//nnf_SourceToTarget.output = newtarget;
				nnf_TargetToSource.input = newtarget;
				target = newtarget;
				newtarget = null;
			}
			
			// -- add constraint to the NNF

		if(level>0)
		{
			for(int y=0;y<target.height;y++)
				for(int x=0;x<target.width;x++)
					if(!source.constainsMasked(x, y, radius))
                    {
						nnf_TargetToSource.field[x,y,0] = x;
						nnf_TargetToSource.field[x,y,1] = y;
						nnf_TargetToSource.field[x,y,2] = 0;
					}
			
			// -- minimize the NNF
			nnf_TargetToSource.minimize(iterNNF);
		}
			
			// -- Now we rebuild the target using best patches from source
			bool upscaled = false;
				
			// Instead of upsizing the final target, we build the last target from the next level source image 
			// So the final target is less blurry (see "Space-Time Video Completion" - page 5) 
			if (level>=1 && (emloop==iterEM))
            {
				newsource = pyramid[level-1];
				newtarget = target.upscale(newsource.width,newsource.height);
				//Debug.Log("newsource: "+newsource.width+"/"+newsource.height);
				upscaled = true;
			}
            else
            {
				newsource = pyramid[level];
				newtarget = target;
				upscaled = false;
			}

			// --- EXPECTATION STEP ---
			if(level>0)
            {
    			// votes for best patch from NNF Source->Target (completeness) and Target->Source (coherence) 
    			double[,,] vote = new double[newtarget.width,newtarget.width,4];
    			//ExpectationStep(nnf_SourceToTarget, true, vote, newsource, upscaled);
    			ExpectationStep(nnf_TargetToSource, false, vote, newsource, upscaled);
    
    			// --- MAXIMIZATION STEP ---
    			// compile votes and update pixel values
    			MaximizationStep(newtarget, vote);
			}
			// debug : display intermediary result
			result = MaskedImage.resize(newtarget.getBufferedImage(), initial.width, initial.height);
			//Demo.display(result);
		}
	
		return newtarget;
	} // ExpectationMaximization()

	//-----------------------------------------------------
	// Expectation Step : vote for best estimations of each pixel
	 public void ExpectationStep(NNF nnf, bool sourceToTarget, double[,,] vote, MaskedImage source, bool upscale)
    {
		int[,,] field = nnf.getField();
		int R = nnf.S;
		for(int y=0;y<nnf.input.height;y++)
        {
			for(int x=0;x<nnf.input.width;x++)
            {
				// x,y = center pixel of patch in input 
					
				// xp,yp = center pixel of best corresponding patch in output
				int xp=field[x,y,0], yp=field[x,y,1], dp=field[x,y,2];
				
				// similarity measure between the two patches
				double w=0;
				if(dp<MaskedImage.DSCALE+1) 
				w = MaskedImage.similarity[dp];
				
				// vote for each pixel inside the input patch
				for(int dy=-R;dy<=R;dy++)
                {
					for(int dx=-R;dx<=R;dx++)
                    {

						// get corresponding pixel in output patch
						int xs,ys,xt,yt;
						if (sourceToTarget) 
							{ xs=x+dx; ys=y+dy;	xt=xp+dx; yt=yp+dy;	}
						else
							{ xs=xp+dx; ys=yp+dy; xt=x+dx; yt=y+dy; }
						
						if (xs<0 || xs>=nnf.input.width) continue;
						if (ys<0 || ys>=nnf.input.height) continue;
						if (xt<0 || xt>=nnf.output.width) continue;
						if (yt<0 || yt>=nnf.output.height) continue;
						
						// add vote for the value
						if (upscale)
                        {
							weightedCopy(source, 2*xs,   2*ys,   vote, 2*xt,   2*yt,   w);
							weightedCopy(source, 2*xs+1, 2*ys,   vote, 2*xt+1, 2*yt,   w);
							weightedCopy(source, 2*xs,   2*ys+1, vote, 2*xt,   2*yt+1, w);
							weightedCopy(source, 2*xs+1, 2*ys+1, vote, 2*xt+1, 2*yt+1, w);
						}
                        else
							weightedCopy(source, xs, ys, vote, xt, yt, w);
					}
				} // vote
			}
		}
	} // ExpectationStep()
 
    //-----------------------------------------------------
	private void weightedCopy(MaskedImage src, int xs, int ys, double[,,] vote, int xd,int yd, double w)
    {
		//Debug.Log("src:"+src.width+"/"+src.height+" xs/ys:"+xs+"/"+ys+" xd/yd:"+xd+"/"+yd+ "  w: "+w);
		
		if (src.isMasked(xs, ys)) return;
		//Debug.Log("vote: "+vote.GetLength(0)+"/"+vote.GetLength(1));
		if(xd<vote.GetLength(0) && yd<vote.GetLength(1)){
		vote[xd,yd,0] += w*src.getSample(xs, ys, 0);
		vote[xd,yd,1] += w*src.getSample(xs, ys, 1);
		vote[xd,yd,2] += w*src.getSample(xs, ys, 2);
		vote[xd,yd,3] += w;
		}
		else Debug.Log("en dehors");
	} // weightedCopy()

	//-----------------------------------------------------
	// Maximization Step : Maximum likelihood of target pixel
	public void MaximizationStep(MaskedImage target, double[,,] vote)
    {
		for(int y=0;y<target.height;y++) {
			for(int x=0;x<target.width;x++) {
				if (vote[x,y,3]>0) {
					int r = (int) ( vote[x,y,0]/vote[x,y,3] );
					int g = (int) ( vote[x,y,1]/vote[x,y,3] );
					int b = (int) ( vote[x,y,2]/vote[x,y,3] );
					
					target.setSample(x, y, 0, r );
					target.setSample(x, y, 1, g );
					target.setSample(x, y, 2, b );
					target.setMask(x,y,false);
				} else {
					// conserve the values from previous target
					//target.setMask(x,y,true);
				}
			}
		}
	} // MaximizationStep()

} // class Inpaint
