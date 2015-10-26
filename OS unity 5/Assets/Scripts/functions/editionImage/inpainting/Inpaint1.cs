//using UnityEngine;
//using System.Collections;
//
//public class Inpaint1 : MonoBehaviour {
//	
//	
//	
//	public float lambda = 2;
//	public float dt = 1;
//	public int width;
//	public int height;
//	public int count = 0;
//	public float distance =10;
//	
//	public int freqcount = 0;
//	public int freq = 30;
//	
//	float R = 0;
//	float laplacienr=0;
//	float laplacieng=0;
//	float laplacienb=0;
//	float psi;
//	
//	public image I;	
//	public image I1;
//	public image I2;
//	public image mask;
//
//
//	public Texture2D masktex;
//	public Texture2D tex;
//	
//	
//
//	// Use this for initialization
//	void Start () {
//		
//		
//		/* texture affichée */
//		tex =  new Texture2D(0,0,TextureFormat.RGB24, false);
//		tex = (Texture2D) GameObject.Find("/Background/backgroundImage").guiTexture.texture;
//		width = tex.width;
//		height = tex.height;		
//
//		
//		
//		/* masque: pixels à 1 si zone sinon 0 */  
//		// masktex = (Texture2D) GameObject.Find("/Background/backgroundImage").GetComponent("Polygon").;
//		 mask = new image(width, height);  
//		
//			for(int x=0; x< width;x++){
//				for(int y=0; y<height; y++){
//					if (masktex.GetPixel(x,y).r > 0.5f) masktex.SetPixel(x,y, new Color(0,0,0));
//					else 								masktex.SetPixel(x,y, new Color(1,1,1));
//			}
//		}
//				
//		mask.setPixels(masktex);
//		
//		
//		I = new image(width, height);  // image initiale 		
//		I.setPixels(tex);
//		I1 = new image(width, height); // image courante                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        Un = new image (width, height); //Un
//		I1.setPixels(tex); 
//		I2 = new image(width, height); // nouvelle image
//		I2.setPixels(tex); 
//		
//		freqcount=freq-1;
//
//
//	}
//	
//	// Update is called once per frame
//	void Update () {	
//		
//		
//		
//	if(distance>0.000001){
//			
//			count++;		
//			freqcount ++;
//	
//		for(int x=0; x< width;x++){
//				for(int y=0; y<height; y++)
//		{			
//			if(masktex.GetPixel(x,y).r<0.5f)
//			{
//				R = mask.get_r(x,y);
//				if(x!=0 && y!=0 && x!=width-1 && y!=height-1){
//				laplacienr = ((I1.get_r(x-1,y-1)*0+I1.get_r(x,y-1)*1+I1.get_r(x+1,y-1)*0+I1.get_r(x-1,y)*1  +I1.get_r(x,y) *(-4)  +I1.get_r(x+1,y)*1+I1.get_r(x-1,y+1)*0+I1.get_r(x,y+1)*1+I1.get_r(x+1,y+1)*0)/9);
//				laplacieng = ((I1.get_g(x-1,y-1)*0+I1.get_g(x,y-1)*1+I1.get_g(x+1,y-1)*0+I1.get_g(x-1,y)*1  +I1.get_g(x,y) *(-4)  +I1.get_g(x+1,y)*1+I1.get_g(x-1,y+1)*0+I1.get_g(x,y+1)*1+I1.get_g(x+1,y+1)*0)/9);
//				laplacienb = ((I1.get_b(x-1,y-1)*0+I1.get_b(x,y-1)*1+I1.get_b(x+1,y-1)*0+I1.get_b(x-1,y)*1  +I1.get_b(x,y) *(-4)  +I1.get_b(x+1,y)*1+I1.get_b(x-1,y+1)*0+I1.get_b(x,y+1)*1+I1.get_b(x+1,y+1)*0)/9);				
//
//				}else  {laplacienr=0; laplacieng=0; laplacienb=0;}
//				
//				I2.set_r((I1.get_r(x,y)+dt*(2*R*(I.get_r(x,y) - I1.get_r(x,y)) +lambda * laplacienr)),x,y);
//				I2.set_g((I1.get_g(x,y)+dt*(2*R*(I.get_g(x,y) - I1.get_g(x,y)) +lambda * laplacieng)),x,y);
//				I2.set_b((I1.get_b(x,y)+dt*(2*R*(I.get_b(x,y) - I1.get_b(x,y)) +lambda * laplacienb)),x,y);	
//			}
//		   }
//		}
//			
//		
//			/* calcul de I2 */			
//			//I2.setPixels( I1 + dt*(mask * (I- I1) + lambda * mask * new laplacien(I1, width, height)));
//			//I2.setPixels( new laplacien(I1, width, height));
//			//image temp = new image(I);
//			//I2.setPixels(I1 + (lambda*dt*new laplacien(I1,width, height)*mask) + (2*dt*mask*(I -I1))  );
//			/* affichage */
//			
//			if(freqcount == freq)
//			{
//				GameObject.Find("/Background/backgroundImage").guiTexture.texture = I1.getTexture();
//				Debug.Log("Iteration : "+count);
//				freqcount = 0;
//				/* calcul de la distance max entre les pixels de I1 et I2  */
//				//distance = image.DistanceMax(I1, I2, mask);
//				Debug.Log("convergence: "+distance);
//			}
//			
//
//			I1.setPixels(I2);
//
//			
//	}else distance = 0;
//
//
//
//	if(distance == 0) {
//			Debug.Log(count+" iterations");
//			distance = -1;
//		}
//		
//	}
//}
