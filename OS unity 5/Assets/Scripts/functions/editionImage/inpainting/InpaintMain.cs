using UnityEngine;
using System.Collections.Generic;
using System;

public class InpaintMain : MonoBehaviour
{
	public int width;
	public int height;

	public image image;
	
	public  Texture2D tex;
    private List<MaskPix> m_inputMask; // Masque indiquant la zone à patcher, donné par PolygonTracer
    
	Inpaint inpainting;
	public int scaleLevel;
	public int step = -1;
	bool[,] mask;
	public Texture2D i_loading = (Texture2D) Resources.Load("tracage/00_Loading_pic");
//	public NNF new_nnf;
//	public NNF new_nnf_rev;
	
    //-----------------------------------------------------
	void OnGUI()
	{
			GUI.BeginGroup(new Rect((Screen.width/2)-172,(Screen.height/2)-76,342,153));
			GUI.DrawTexture(new Rect(0,0,342,153), i_loading);
	        GUI.EndGroup();
	}
	
    //-----------------------------------------------------
	void Start()
	{
		tex = new Texture2D(0,0,TextureFormat.RGB24, false);
		tex = (Texture2D) GameObject.Find("Background/backgroundImage").GetComponent<GUITexture>().texture;
		width = tex.width;
		height = tex.height;	
		image = new image(width, height);
		image.setPixels(tex);

		// Générer le tableau
		mask = new bool[width,height];  // Note : les bool sont initialisés à false par défaut
        
        for(int i=0; i<m_inputMask.Count; i++)
            mask[m_inputMask[i].GetX(), m_inputMask[i].GetY()] = true;
//      for(int y=0;y<height;y++)                           // Texture2D remplacée par une liste de MaskPix
//          for(int x=0;x<width;x++)
//              mask[x,y]=(masktex.GetPixel(x,y).r == 1);
        
//		DestroyImmediate(masktex); // TODO libérer la mémoire de masktex
		
//        for(int y=0;y<height;y++)     // KS : commenté, aucune différence apparente.
//        {
//			for(int x=0;x<width;x++)
//            {
//				if (mask[x,y])
//                {
//					image.set_r(255,x,y);
//					image.set_g(0,x,y);
//					image.set_b(0,x,y);
//				}
//			}
//		}
		Debug.Log("mask created");
		
		// display input with red area
//		GameObject.Find("/Background/backgroundImage").guiTexture.texture = image.getTexture(); // KS : utile pour debug seulement
	    
		inpainting = new Inpaint(image, mask, 2);         // initialisation inpainting

		scaleLevel = inpainting.maxlevel-1;
		Debug.Log(scaleLevel+" levels");
    
	} // Start()
	
    //-----------------------------------------------------
    void Update()
    {
        if(scaleLevel > -1)     // MAJ du niveau de l'image en cours de traitement
        {
    		if( step < 4) step ++;
    		else        { step = 0; scaleLevel --; }
        }
		
    	// Premier niveau
    	if(scaleLevel == inpainting.maxlevel - 1)
    	{
    		if(step == 0)
    		{
    			// At first, we use the same image for target and source and use random data as initial guess
    			Debug.Log("\n*** Processing -  Zoom 1:"+(1 << scaleLevel)+" ***"+scaleLevel);
    			Debug.Log(inpainting.pyramid[scaleLevel].width+"/"+inpainting.pyramid[scaleLevel].height);
    			
    			// create Nearest-Neighbor Fields (direct and reverse)
    			inpainting.source = inpainting.pyramid[scaleLevel];
    	        
    			Debug.Log("initialize NNF...");
    			
                inpainting.target = inpainting.source;
        
                // we consider that initially the target contains no masked pixels
                for(int y=0;y<inpainting.target.height;y++)
                    for(int x=0;x<inpainting.target.width;x++)
                        inpainting.target.setMask(x,y,false);   // Note : boucle sur l'image downsamplée donc beaucoup moins d'itérations
    	    }
    		else if (step == 1)               // Test uniquement entre la cible et la source
    			inpainting.nnf_TargetToSource = new NNF(inpainting.target, inpainting.source, inpainting.radius);
            
    		else if (step == 2)
    			inpainting.nnf_TargetToSource.randomize();
            
    		else if (step == 3)
    			inpainting.target = inpainting.ExpectationMaximization(scaleLevel);
            
    		else if (step == 4)
    			GameObject.Find("Background/backgroundImage").GetComponent<GUITexture>().texture = inpainting.target.getBufferedImage().getTextureRGB();
        } // fin premier niveau
    	
    	// niveaux suivants
    	if(scaleLevel>=0 && scaleLevel != inpainting.maxlevel-1)
        {
    		if(step ==0)
    		{
    			Debug.Log("\n*** Processing -  Zoom 1:"+(1<<scaleLevel)+" ***"+scaleLevel);
    			// create Nearest-Neighbor Fields (direct and reverse)
    			inpainting.source = inpainting.pyramid[scaleLevel];
    			Debug.Log("initialize NNF...");
    		}
    		else if (step ==1)
    		{
    			// then, we use the rebuilt (upscaled) target and reuse the previous NNF as initial guess
    			inpainting.nnf_TargetToSource = new NNF(inpainting.target, inpainting.source, inpainting.radius);
    		}
    		else if (step == 2){
    			if (scaleLevel >0)inpainting.nnf_TargetToSource.initialize(inpainting.nnf_TargetToSource);
    
    		}
    		else if (step ==3)
    		{
    			// Build an upscaled target by EM-like algorithm (see "PatchMatch" - page 6)
    			inpainting.target = inpainting.ExpectationMaximization(scaleLevel);
    			Debug.Log("Fin level: "+ scaleLevel);
    		}
    		else if (step == 4)
    		{
    			GameObject.Find("Background/backgroundImage").GetComponent<GUITexture>().texture = inpainting.target.getBufferedImage().getTextureRGB();
    		}
    	}
    	
    	/* finalisation */
    	if(scaleLevel == -1)
    	{
    		//finalisation / on ne fait pas le level 0=> inutile voir pire et trop long
    //			Texture2D temp = inpainting.target.getBufferedImage().getTexture();
    //			Color[] color = new Color[width*height];
    //			for(int x = 0; x< width; x++)
    //			{
    //				for(int y=0; y <height; y++)
    //				{
    //					if(mask[x,y]) color[y*width+x] = temp.GetPixel(x,y);
    //				}
    //			}
    //			mask = null;
    //			temp.Apply();
    //			GameObject.Find("/Background/backgroundImage").guiTexture.texture = temp;			
    
    		Debug.Log("DONE");
            Resources.UnloadUnusedAssets();
    		Destroy(this); //AUTO KILL
        }
    } // void Update()
    
    //-----------------------------------------------------
    public void SetMask(List<MaskPix> newMask)
    {
        m_inputMask = newMask;
    }

} // class Main
