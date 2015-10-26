using UnityEngine;
using System;
using System.Collections;


public class image
{
	public  int[,]  r;      // TODO utiliser byte au lieu de int (ou un seul int[,] au lieu de 4)
	public  int[,]  g;
	public  int[,]  b;
	public  int[,]  a;

	public int width;
	public int height;
	public Texture2D tex;
	
	public image(int width, int height)
    {
		this.width = width;
		this.height = height;
		tex = null;
		tex = new Texture2D(width, height);
		r = new int[width, height];
		g = new int[width, height];
		b = new int[width, height];
		a = new int[width, height];
	}

    //-----------------------------------------------------
	// Copie d'une autre matrice:
	public image(image nouveau)
	{
		//image im = new image(nouveau);
		this.width = nouveau.width;
		this.height = nouveau.height;
		this.r = new int[width,height];
		this.g = new int[width,height];
		this.b = new int[width,height];
		this.a = new int[width,height];

		tex = null;
		tex = new Texture2D(width, height);

		for(int x=0; x<width; x++)
        {
			for(int y=0; y<height; y++)
            {
				r[x,y]= nouveau.r[x,y];
				g[x,y]= nouveau.g[x,y];
				b[x,y]= nouveau.b[x,y];
				a[x,y]= nouveau.a[x,y];
			}
		}
	}

    //-----------------------------------------------------
    // Copie d'une sous-matrice:
    public image(image nouveau, int xOffset, int yOffset, int w, int h)
    {
        //image im = new image(nouveau);
        this.width = w;
        this.height = h;
        this.r = new int[w,h];
        this.g = new int[w,h];
        this.b = new int[w,h];
        this.a = new int[w,h];

        tex = null;
        tex = new Texture2D(w, h);

        for(int x = xOffset ; x < xOffset+w ; x++)
        {
            for(int y = yOffset ; y < yOffset+h ; y++)
            {
                r[x-xOffset,y-yOffset] = nouveau.r[x,y];
                g[x-xOffset,y-yOffset] = nouveau.g[x,y];
                b[x-xOffset,y-yOffset] = nouveau.b[x,y];
                a[x-xOffset,y-yOffset] = nouveau.a[x,y];
            }
        }
    }

	// Matrice Carrée:
	public image(int n)
	{
		tex = null;
		tex = new Texture2D(n, n);	
		this.r = new int[n,n];
		this.g = new int[n,n];
		this.b = new int[n,n];
		this.a = new int[n,n];
		this.width = n;
		this.height = n;
	}
	
	public string Length
	{
		get
		{
			int n = r.GetLength(0);
			int p = r.GetLength(1);
			string length = "("+n+","+p+")";
		return length;
		}
	}
	
	public int Width()
	{
		return get_r().GetLength(0);
	}
	public int Height()
	{
		return get_r().GetLength(1);
	}
	
    //-----------------------------------------------------
    // ARGB Texture
    public Texture2D getTexture()
    {
        Color[] color = new Color[width*height];
        for(int x=0; x<width; x++){         
            for(int y=0; y<height; y++){
                //color[y*width + x] = new Color((float)r[x,y]/255, (float)g[x,y]/255, (float)b[x,y]/255);
                color[y*width + x] = new Color((float)r[x,y]/255, (float)g[x,y]/255, (float)b[x,y]/255,(float)a[x,y]/255);
            }
        }   
        tex.SetPixels(color);
        tex.Apply();
        
        return  tex;        
    }
    
    //-----------------------------------------------------
    // RGB Texture
    public Texture2D getTextureRGB()
    {
        Color[] color = new Color[width*height];
        for(int x=0; x<width; x++)
        {         
            for(int y=0; y<height; y++)
                color[y*width + x] = new Color((float)r[x,y]/255, (float)g[x,y]/255, (float)b[x,y]/255);
        }
        
        Texture2D rgbTex = new Texture2D(width, height, TextureFormat.RGB24, false);
        rgbTex.SetPixels(color);
        rgbTex.Apply();
        
        return  rgbTex;
    } // getTextureRGB
//	
//	public void AppliquerFiltre3x3(float[] filtre){
//		
////		float[] tempr = r;
////		float[] tempg = g;
////		float[] tempb = b;
//		
//		for(int x=0; x<width; x++){			
//			for(int y=0; y<height; y++){
//				
//				/* cas normal */
//			if(x!=0 && y!=0 && x!=width-1 && y!=height-1)	
//			{
//
//				r[x,y]  = r[x-1,y-1]*filtre[0]+r[x,y-1]*filtre[1]+r[x+1,y-1]*filtre[2]+r[x-1,y]*filtre[3]  +r[x,y] *filtre[4]  +r[x+1,y]*filtre[5]+r[x-1,y+1]*filtre[6]+r[x,y+1]*filtre[7]+r[x+1,y+1]*filtre[8];
//				
//				g[x,y]  = g[x-1,y-1]*filtre[0]+g[x,y-1]*filtre[1]+g[x+1,y-1]*filtre[2]+g[x-1,y]*filtre[3]  +g[x,y]*filtre[4]  +g[x+1,y]*filtre[5]+g[x-1,y+1]*filtre[6]+g[x,y+1]*filtre[7]+g[x+1,y+1]*filtre[8];
//
//				b[x,y]  = b[x-1,y-1]*filtre[0]+b[x,y-1]*filtre[1]+b[x+1,y-1]*filtre[2]+b[x-1,y]*filtre[3]  +b[x,y]*filtre[4]  +b[x+1,y]*filtre[5]+b[x-1,y+1]*filtre[6]+b[x,y+1]*filtre[7]+b[x+1,y+1]*filtre[8];
//
//			}
//			//else {r[x,y]=0; g[x,y]=0; b[x,y]=0;}				
//		}
//		}	
//		
//	}
//	

	public image Transpose
	{
		get
		{
			int[,] TableauTemporaireR = new int[width,height];
			int[,] TableauTemporaireG = new int[width,height];
			int[,] TableauTemporaireB = new int[width,height];
			for ( int j = 0 ; j < height ; j++ )
			{
				for ( int i = 0 ; i < width ; i++ )
				{
					TableauTemporaireR[i,j] = r[j,i];
					TableauTemporaireG[i,j] = g[j,i];
					TableauTemporaireB[i,j] = b[j,i];
				}
			}
			image result = new image(width, height);
			result.r = TableauTemporaireR;
			result.g = TableauTemporaireG;
			result.b = TableauTemporaireB;
		return new image( result );
		}
	}
	
	public float[] Trace
	{
		get
		{
			float[] trace = new float[3];
			try
			{
				if ( this.width == this.height )
				{
					for ( int i = 0 ; i < this.height ; i++ )
					{
						trace[0] += r[i,i];
						trace[1] += g[i,i];
						trace[2] += b[i,i];
					}
					return trace;
				}
				else
				{
					throw new Exception( "Impossible de calculer la trace du matrice non carrée" );
				}
			}
			catch ( Exception e )
			{
				Console.Error.WriteLine( "" + e );
				return trace;
			}
		}
	}
	
	public static image operator*(float var1, image var2)
	{		
		
		//image tab = new image(var2.width, var2.height);
		for(int x=0; x < var2.width; x++) {
			for(int y=0; y < var2.height; y++) {
				var2.set_r ((int)(var1 * var2.get_r(x,y)),x,y);
				var2.set_g ((int)(var1 * var2.get_g(x,y)),x,y);
				var2.set_b ((int)(var1 * var2.get_b(x,y)),x,y);
			}			
		}
			
		return var2;
	}
	
	public static image operator*(image var1, image var2)
	{
	
		if(var2.Length == var1.Length){
			for(int x=0; x<var1.width; x++){
				for(int y=0; y<var1.height; y++){
					var1.r[x,y]= (var1.r[x,y] * var2.r[x,y])/255;
					var1.g[x,y]= (var1.g[x,y] * var2.g[x,y])/255;
					var1.b[x,y]= (var1.b[x,y] * var2.b[x,y])/255;
				}		
			}
		}
		else
		{
		throw new Exception( "Impossible de multiplier des matrices de dimensions différentes" );
		}
			
		return var1;
	}
	
	public static image operator+(image var1, image var2)
	{
		if(var1.Length == var2.Length){
			for(int x=0; x<var1.width; x++){
				for(int y=0; y<var1.height; y++){
						var1.r[x,y]= var1.r[x,y] + var2.r[x,y];
						var1.g[x,y]= var1.g[x,y] + var2.g[x,y];
						var1.b[x,y]= var1.b[x,y] + var2.b[x,y];
				}			
			}
		}	
		else
		    { throw new Exception( "Impossible de soustraire des matrices de dimensions différentes" ); }
		return var1;
	}
    
	public static image operator-(image var1, image var2)
	{
		if(var1.Length == var2.Length)
        {
			for(int x=0; x<var1.width; x++)
            {
				for(int y=0; y<var1.height; y++)
                {
						var1.r[x,y]= (var1.r[x,y] - var2.r[x,y]);
						var1.g[x,y]= (var1.g[x,y] - var2.g[x,y]);
						var1.b[x,y]= (var1.b[x,y] - var2.b[x,y]);
				}			
			}
		}	
		else throw new Exception( "Impossible de soustraire des matrices de dimensions différentes" );
		return var1;
	}
	
	public void setPixels(image tab)
	{
        setPixels(tab, 0, 0);
	}

    public void setPixels(image tab, int xOffset, int yOffset)
    {
        int h, w;       // Détecter les plus petites dimensions entre l'image donnée et "this", pour
//        if(tab.height >= this.height && tab.width >= this.width) // éviter les "out of array exception"
//             { h = this.height;
//               w = this.width; }
//        else if(tab.height >= this.height && tab.width < this.width)
//             { h = this.height;
//               w = tab.width;  }
//        else if(tab.height < this.height && tab.width >= this.width)
//             { h = tab.height;
//               w = this.width; }
//        else { h = tab.height;
//               w = tab.width;  }

        for(int x = 0 ; x < tab.width ; x++)
        {
            for(int y = 0 ; y < tab.height ; y++)
            {
                this.set_r(tab.get_r(x,y), x+xOffset, y+yOffset);
                this.set_g(tab.get_g(x,y), x+xOffset, y+yOffset);
                this.set_b(tab.get_b(x,y), x+xOffset, y+yOffset);
            }
        }
    }

	public void setPixels( Texture2D tex)
    {
		tex.SetPixels (tex.GetPixels());
		for (int x = 0; x < width; x++) 
		{
			for(int y=0; y < height;y++)
			{
				r[x,y] = (int) ((tex.GetPixel(x,y).r)*255);
				g[x,y] = (int) ((tex.GetPixel(x,y).g)*255);
				b[x,y] = (int) ((tex.GetPixel(x,y).b)*255);
//				a[x,y] = (int) ((tex.GetPixel(x,y).a)*255);
			}
		}
	}

    //-----------------------------------------------------
    // Fonction utilisée pour remplacer BufferedImage.getSubimage()
    // de Java, pour l'outil gazon
    public image getSubImage(int x, int y, int w, int h)
    {
        return new image(this, x, y, w, h);
    }
	
	/* Différence maximum entre toutes les composantes de tous les pixels des images T1 et T2 */
	public static int DistanceMax(image T1, image T2, image mask)
	{
		int max = 0;
		int diff;
		for (int x = 0; x < T1.width; x++) 
		{
			for(int y=0; y < T1.height;y++)
			{
				if(mask.get_r(x,y) < 0.5f)
				{
						diff = Mathf.Abs(T1.get_r(x,y) - T2.get_r(x,y));
					if(diff > max) max = diff;
						diff = Mathf.Abs(T1.get_g(x,y) - T2.get_g(x,y));
					if(diff > max) max = diff;	
						diff = Mathf.Abs(T1.get_b(x,y) - T2.get_b(x,y));
					if(diff > max) max = diff;
				}
			}
		}
	return max;
	}

    //-----------------------------------------------------
    // Fonction utilisée pour remplacer BufferedImage.getRGB()
    // de Java (pour le synthétiseur de textures du gazon)
    // ATTENTION : la longueur de output doit correspondre à width * height
    public int[] getRGB(int[] output)
    {
        return getRGB(output, 0, 0, this.width, this.height);
    }

    //-----------------------------------------------------
    // Fonction utilisée pour remplacer BufferedImage.getRGB()
    // de Java (pour le synthétiseur de textures du gazon)
    public int[] getRGB(int[] output, int xOffset, int yOffset, int w, int h)
    {
        int i = 0;
        int xMax = xOffset+w;
        int yMax = yOffset+h;
        for(int y = yOffset; y < yOffset+h; y++)
        {
            for(int x = xOffset; x < xOffset+w; x++)   // "Concaténation" des composantes argb en un seul int
            {
                output[i] = (a[x, y] << 24) | ( r[x, y] << 16 ) | ( g[x, y] << 8 ) | b[x, y];
                i++;
            }
        }
        return output;
    }

    //-----------------------------------------------------
    // Retourne l'image encodée façon "BufferedImage" de Java
    // format ARGB 32
    public void setRGB(int[] tab)
    {
        setRGB(tab, 0, 0, width, height, width);
    }

    //-----------------------------------------------------
    // Assigne les pixels de l'image à partir d'une image
    // encodée façon "BufferedImage" de Java
    // format ARGB 32.
    // TODO l'alpha ne marche pas bien (255<<24 est ensuite décodé en -1 au lieu de 255,
    // sûrement dû à l'encodage des int pour les négatifs)
    public void setRGB(int[] tab, int xOffset, int yOffset, int w, int h, int scanSize)
    {
        int x, y, atmp=0, rtmp=0, gtmp=0, btmp=0;
        for(int i = 0; i<tab.Length; i++)
        {
            x = i % scanSize;
            y = (i - x)/scanSize;
            x += xOffset;
            y += yOffset;
            if(x >= xOffset && x < xOffset+w && y >= yOffset && y < yOffset+h)
            {
                atmp += (tab[i] >> 24);
                rtmp += (tab[i] >> 16) & 0xff;
                gtmp += (tab[i] >>  8) & 0xff;
                btmp += (tab[i])       & 0xff;
                this.set_a(atmp, x, y);
                this.set_r(rtmp, x, y);
                this.set_g(gtmp, x, y);
                this.set_b(btmp, x, y);
                atmp = rtmp = gtmp = btmp = 0;
            }
        }
    }

    //-----------------------------------------------------
    // Exécute un miroir par rapport à l'axe X
    public void MirrorX()
    {
        int tmpr, tmpg, tmpb, tmpa;
        for(int x=0; x<this.width ; x++)
        {
            for(int y=0; y < (int)this.height/2; y++)
            {
                tmpr = r[x, y];
                tmpb = b[x, y];
                tmpg = g[x, y];
                tmpa = a[x, y];

                r[x, y] = r[x, this.height-y-1];
                b[x, y] = b[x, this.height-y-1];
                g[x, y] = g[x, this.height-y-1];
                a[x, y] = a[x, this.height-y-1];

                r[x, this.height-y-1] = tmpr;
                b[x, this.height-y-1] = tmpb;
                g[x, this.height-y-1] = tmpg;
                a[x, this.height-y-1] = tmpa;
            }
        }
    }

    //-----------------------------------------------------
    // Miroir par rapport à l'axe Y
    public void MirrorY()
    {
        int tmpr, tmpg, tmpb, tmpa;
        for(int y=0; y<this.height ; y++)
        {
            for(int x=0; x < (int)this.width/2; x++)
            {
                tmpr = r[x, y];
                tmpb = b[x, y];
                tmpg = g[x, y];
                tmpa = a[x, y];

                r[x, y] = r[this.width-x-1, y];
                b[x, y] = b[this.width-x-1, y];
                g[x, y] = g[this.width-x-1, y];
                a[x, y] = a[this.width-x-1, y];

                r[this.width-x-1, y] = tmpr;
                b[this.width-x-1, y] = tmpb;
                g[this.width-x-1, y] = tmpg;
                a[this.width-x-1, y] = tmpa;
            }
        }
    }

	public int get_r(int x, int y)
	{
		return r[x,y];
	}
	public int get_g(int x, int y)
	{
		return g[x,y];
	}
	public int get_b(int x, int y)
	{
		return b[x,y];
	}
	public int get_a(int x, int y)
	{
		return a[x,y];
	}
	
	public int[,] get_r()
	{
		return r;
	}
	public int[,] get_g()
	{
		return g;
	}
	public int[,] get_b()
	{
		return b;
	}
	public int[,] get_a()
	{
		return a;
	}
	
	public void set_r(int val, int x, int y)
	{
		 r[x,y] = val;		
	}
	public void set_g(int val, int x, int y)
	{
		 g[x,y] = val;				
	}
	public void set_b(int val, int x, int y)
	{
		 b[x,y] = val;				
	}
	public void set_a(int val, int x, int y)
	{
		 a[x,y] = val;				
	}

}
