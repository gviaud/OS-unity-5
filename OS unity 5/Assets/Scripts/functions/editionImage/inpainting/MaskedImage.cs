using UnityEngine;
using System.Collections;

public class MaskedImage
{
	// image data
	private bool[,] mask;
	private image im;
	public int width, height;
	
	// the maximum value returned by MaskedImage.distance() 
	public static int DSCALE = 65535;
	
	// array for converting distance to similarity
	public static double[] similarity;
	
	//-----------------------------------------------------
	// construct from existing BufferedImage and mask
	public MaskedImage(image im, bool[,] mask)
    {
		this.im = im;
		this.width=im.width;
		this.height=im.height;
		this.mask = mask;
		
		
				// reference array is length 100, but is truncated at first zero value
		// base[0]=1.0, base[1]=0.99, ..., base[99]=0, base[100]=0    
		double[] bas = new double[11]{1.0, 0.99, 0.96, 0.83, 0.38, 0.11, 0.02, 0.005, 0.0006, 0.0001, 0 };
		
		// stretch base array 
		similarity = new double[DSCALE+1];
		for(int i=0;i<DSCALE+1;i++) {
			double t = (double)i/(DSCALE+1);

			// interpolate from base array values
			int j = (int)(100*t), k=j+1;
			double vj = (j<11)?bas[j]:0;
			double vk = (k<11)?bas[k]:0;
			
			double v = vj + (100*t-j)*(vk-vj);
			similarity[i] = v;
		}
	}
 
    //-----------------------------------------------------
	// construct empty image
	public MaskedImage(int width, int height)
    {
		this.width=width;
		this.height=height;
		this.im = new image(width, height);
		this.mask = new bool[width,height];
	}
	
    //-----------------------------------------------------
	public MaskedImage(MaskedImage copy)
    {
		this.im = copy.im;
		width = copy.width;
		height = copy.height;
		this.mask = new bool[width,height];
		
		for(int y=0;y<height;y++)
			for(int x=0;x<width;x++)
				mask[x,y] = copy.mask[x,y];
	}
	
    //-----------------------------------------------------
	public image getBufferedImage()
    {
		return im;
	}
	
    //-----------------------------------------------------
	public int getSample(int x, int y, int band) 
	{
		if(band==0)      return (int) im.get_r(x,y);
		else if(band==1) return (int) im.get_g(x,y);
		else 			 return (int) im.get_b(x,y);
	}
	
    //-----------------------------------------------------
	public void setSample(int x, int y, int band, int val) 
	{
		if(band==0)       im.set_r( val,x,y);
		else if(band==1)  im.set_g( val,x,y);
		else              im.set_b( val,x,y);
	}
 
    //-----------------------------------------------------
	public bool isMasked(int x, int y)
    {
        return mask[x,y];
	}
	
    //-----------------------------------------------------
	public void setMask(int x, int y, bool value)
    {
//        Debug.Log("setMask x="+x+", y="+y);
		mask[x,y]=value;
	}
	
    //-----------------------------------------------------
	// return true if the patch contains one (or more) masked pixel
	public bool constainsMasked(int x, int y, int S)
    {
		for(int dy=-S;dy<=S;dy++)
        {
			for(int dx=-S;dx<=S;dx++)
            {
				int xs=x+dx, ys=y+dy;
				if (xs<0 || xs>=width) continue;
				if (ys<0 || ys>=height) continue;
				if (mask[xs,ys]) return true;
			}
		}
		return false;
	} // containsMasked()
	
    //-----------------------------------------------------
	// distance between two patches in two images
	public static int distance(MaskedImage source,int xs,int ys, MaskedImage target,int xt,int yt, int S)
    {
		long distance=0, wsum=0, ssdmax = 9*255*255;

		// for each pixel in the source patch
		for(int dy=-S;dy<=S;dy++) {
			for(int dx=-S;dx<=S;dx++) {
				wsum+=ssdmax;
				
				int xks=xs+dx, yks=ys+dy;
				if (xks<1 || xks>=source.width-1) {distance+=ssdmax; continue;}
				if (yks<1 || yks>=source.height-1) {distance+=ssdmax; continue;}
				
				// cannot use masked pixels as a valid source of information
				if (source.isMasked(xks, yks)) {distance+=ssdmax; continue;}
				
				// corresponding pixel in the target patch
				int xkt=xt+dx, ykt=yt+dy;
				//Debug.Log("target.width:"+(target.getWidth()));
				//Debug.Log("xkt:"+xkt+" ykt:"+ykt);
				if (xkt < 1 || xkt >= target.width-1) 
								{
								distance += ssdmax; continue;
								}
				if (ykt<1 || ykt>=target.height-1) 
								{distance += ssdmax; continue;}

				// cannot use masked pixels as a valid source of information
				if (target.isMasked(xkt, ykt)) {distance+=ssdmax; continue;}
				
				// SSD distance between pixels (each value is in [0,255^2])
				long ssd=0;
				for(int band=0; band<3; band++) {
					// pixel values
					int s_value = source.getSample(xks, yks, band);
					int t_value = source.getSample(xkt, ykt, band);
					
					// pixel horizontal gradients (Gx)
					float s_gx = 128+(source.getSample(xks+1, yks, band) - source.getSample(xks-1, yks, band))/2;
					float t_gx = 128+(target.getSample(xkt+1, ykt, band) - target.getSample(xkt-1, ykt, band))/2;

					// pixel vertical gradients (Gy)
					float s_gy = 128+(source.getSample(xks, yks+1, band) - source.getSample(xks, yks-1, band))/2;
					float t_gy = 128+(target.getSample(xkt, ykt+1, band) - target.getSample(xkt, ykt-1, band))/2;

					ssd += (long) Mathf.Pow(s_value-t_value , 2); // distance between values in [0,255^2]
					ssd += (long) Mathf.Pow(s_gx-t_gx , 2); // distance between Gx in [0,255^2]
					ssd += (long) Mathf.Pow(s_gy-t_gy , 2); // distance between Gy in [0,255^2]
				}

				// add pixel distance to global patch distance
				distance += ssd;
			}
		}
		
		return (int)(DSCALE*distance/wsum);
	} // static distance()
	
    //-----------------------------------------------------
	// Helper for BufferedImage resize
	public static image resize(image input, int newwidth, int newheight)
    {
		image outimage = new image(newwidth, newheight);
//		g.drawImage(scaled, 0, 0, outimage.getWidth(), outimage.getHeight(), null);
//		g.dispose();
		return outimage;
	} // resize()
 
    //-----------------------------------------------------
	// return a copy of the image
	public MaskedImage copy()
    {
		bool[,] newmask= new bool[width, height];
		image newimage = new image(im);
		//newimage.createGraphics().drawImage(image, 0, 0, null);
		for(int y=0;y<height;y++)
			for(int x=0;x<width;x++)
				newmask[x,y] = mask[x,y];
		return new MaskedImage(newimage,newmask);
	} // copy()
	
    //-----------------------------------------------------
	// return a downsampled image (factor 1/2)
	public MaskedImage downsample()
    {
		int newW=width/2, newH=height/2;
		
		// Binomial coefficient
		int[] kernel = {1,5,10,10,5,1}; 

		MaskedImage newimage = new MaskedImage(newW, newH);
		
		for(int y=0;y<height-1;y+=2) {
			for(int x=0;x<width-1;x+=2) {
				
				int r=0,g=0,b=0,m=0,ksum=0;
				
				for(int dy=-2;dy<=3;dy++) {
					int yk=y+dy;
					if (yk<0 || yk>=height) continue;
					int ky = kernel[2+dy];
					for(int dx=-2;dx<=3;dx++) {
						int xk = x+dx;
						if (xk<0 || xk>=width) continue;
						
						if (mask[xk,yk]) continue;
						int k = kernel[2+dx]*ky;
						r+= k*this.getSample(xk, yk, 0);
						g+= k*this.getSample(xk, yk, 1);
						b+= k*this.getSample(xk, yk, 2);
						ksum+=k;
						m++;
					}
				}
				if (ksum>0) {r/=ksum; g/=ksum; b/=ksum;}
	
				if (m!=0) {
					newimage.setSample(x/2, y/2, 0, r);
					newimage.setSample(x/2, y/2, 1, g);
					newimage.setSample(x/2, y/2, 2, b);
					newimage.setMask(x/2, y/2, false);
				} else {
					newimage.setMask(x/2, y/2, true);
				}
			}
		}
		
		return newimage;
	} // downsample()
	
    //-----------------------------------------------------
	// return an upscaled image
	public MaskedImage upscale(int newW,int newH)
    {
		MaskedImage newimage = new MaskedImage(newW, newH);
		
		for(int y=0;y<newH;y++) {
			for(int x=0;x<newW;x++) {
				
				// original pixel
				int xs = (x*width)/newW;
				int ys = (y*height)/newH;
				
				// copy to new image
				if (!mask[xs,ys]) {
					newimage.setSample(x, y, 0, this.getSample(xs, ys, 0));
					newimage.setSample(x, y, 1, this.getSample(xs, ys, 1));
					newimage.setSample(x, y, 2, this.getSample(xs, ys, 2));
					newimage.setMask(x, y, false);
				} else {
					newimage.setMask(x, y, true);
				}
			}
		}
		
		return newimage;
	} // upscale()
	
    //-----------------------------------------------------
	public int getWidth()
    {
	    return this.width;	
	}

} // class MaskedImage
