//using UnityEngine;
//using System.Collections;
//using System;
//
//public class OldPolygon : MonoBehaviour
//{
//	private ArrayList lines = new ArrayList();
//	
//	public bool lineStarted = false;
//	public bool polygonLocked = false;
//	public bool maskdone = false;
//
//	public Texture2D cursor = (Texture2D) Resources.Load("tracage/curseur");
//	public Texture2D vertex = (Texture2D) Resources.Load("tracage/point");
//	public Texture2D lineTex = (Texture2D) Resources.Load("tracage/grayTex");
//	public Texture2D tempLineTex = (Texture2D) Resources.Load("tracage/redTex");
//	
//	public float mousePosX = 0;
//	public float mousePosY = 0;
//	public float _width = 5;
//	private float tmpLneW = 2;
//	
//	public Texture2D MASK;
//	public int width;
//	public int height;
//	
//	private Line lineToAdd;
//	
//	private GUISkin skin;
//	
//	// Use this for initialization
//	void Start() 
//	{
//		skin = (GUISkin)GameObject.Find("MainScene").GetComponent<GUIMain>().GetSkin();
//		width = (int) GameObject.Find("/Background/backgroundImage").guiTexture.texture.width;
//		height = (int) GameObject.Find("/Background/backgroundImage").guiTexture.texture.height;
//	}
//	
//	// Update is called once per frame
//	void FixedUpdate()
//	{
//		if(!polygonLocked)// tracage du polygone
//		{
//			mousePosX = Input.mousePosition.x;
//			mousePosY = Screen.height-Input.mousePosition.y;
//			
//			/* click gauche */
//			if(Input.GetMouseButtonUp(0))
//			{
//				if(!lineStarted)
//				{  //premier click: créer un tracé
//					lineToAdd = new Line();
//					lineToAdd.src = new Vector2(mousePosX,mousePosY);
//					lineStarted = true;
//				}
//				else
//				{
//					//click suivant						
//					// zone du point origine, si 3 segments minimum, polygone verouillé	
//					if(lines.Count > 2 && Line.isOnOrigin(new Vector2(mousePosX,mousePosY), ((Line)lines[0]).getSrc(), 10))
//					{
//						// la destination devient le point origine	
//						lineToAdd.setDst( new Vector2( ((Line)lines[0]).src.x , ((Line)lines[0]).src.y) ,_width);
//						lines.Add(lineToAdd);
//						lineStarted = false;
//						polygonLocked =true;   // polygone verouillé
//					}
//					else{//par défaut: segment précédent validé, nouveau tracé
//						lineToAdd.setDst(new Vector2(mousePosX,mousePosY),_width);
//						lines.Add(lineToAdd);
//						lineToAdd = new Line();
//						lineToAdd.src = new Vector2(mousePosX,mousePosY);
//						lineStarted = true;
//					}
//				}
//			}
//		}// fin tracage polygone
//		else if(!maskdone)// calcul du masque
//		{
//			int wGUI = (int) GameObject.Find("/Background/backgroundImage").guiTexture.pixelInset.width;
//			int hGUI = (int) GameObject.Find("/Background/backgroundImage").guiTexture.pixelInset.height;
//			MASK = new Texture2D(width, height, TextureFormat.RGB24, false);
//
//			/* quels points de l'image sont à l'intérieur de la zone de traçage */
//			Color[] values = new Color[width * height];
//			int yoffset = (Screen.height - hGUI)/2;
//			int xoffset = (Screen.width - wGUI)/2;
//			//correspondance position masque / position écran
//			int x_corres;
//			int y_corres;
//			for(int x= 0; x < width; x++)
//			{
//				for(int y=0; y<height ; y++)
//				{
//					int j = lines.Count - 1;
//					bool inmask = false;  
//			        for (int i = 0; i < lines.Count; i++)  
//			        {  
//					  x_corres = x* wGUI /width + xoffset;	
//					  y_corres = Screen.height - y* hGUI /height - yoffset;
//			          if (((Line)lines[i]).dst.y < y_corres && ((Line)lines[j]).dst.y >= y_corres ||  
//			            ((Line)lines[j]).dst.y < y_corres && ((Line)lines[i]).dst.y >= y_corres)  
//			            {  
//			                if ((((Line)lines[i]).dst.x+ (y_corres - ((Line)lines[i]).dst.y)/(((Line)lines[j]).dst.y 
//							             - ((Line)lines[i]).dst.y)*(((Line)lines[j]).dst.x - ((Line)lines[i]).dst.x)) < x_corres)  
//			                {  
//			                    inmask = !inmask;  
//							}  
//			            }  
//			            j = i; 						
//			        }
//					/* construction du masque */
//					if(inmask)
//					{	
//						values[y*width+x] = new Color(1,1,1);
//					}
//				}
//			}
//			MASK.SetPixels(values);
//			MASK.Apply();
//			maskdone = true; // masque validé
//	
//			/* si l'inpainting n'est pas déjà actif et si il y a un background */
//			if(GameObject.Find("/Background/backgroundImage").GetComponent("InpaintMain")==null && GameObject.Find("/Background/backgroundImage").guiTexture.texture!=null)
//			{       
//				/* activation inpainting */
//				GameObject.Find("/Background/backgroundImage").AddComponent("InpaintMain");
//				Debug.Log("Ajout script Inpainting patch match ");
//				GameObject.Find("/Background/backgroundImage").GetComponent<InpaintMain>().masktex = this.MASK;
//				lines.Clear();
//			    lineStarted = false;
//				
//				Destroy(this);// AUTO KILL
//			 }
//		}// fin calcul du masque
//		
//		/* touche Esc: annule masque / inpainting */
//		if (Input.GetKeyDown(KeyCode.Escape))
//			abort();
//	}
//	
//	void OnGUI()
//	{
//		GUI.skin = skin;
//		//if(GUI.Button(new Rect ((Screen.width/2)-65,Screen.height-50, 130, 50), "Annuler le tracé", "inpainting_abort"))
//		if(GUI.Button(new Rect ((Screen.width/2)-65,Screen.height-50, 130, 50), "Annuler le tracé"))
//		{
//			abort();
//		}
//		//drawGrid();
//		drawTempLine();
//		drawLines();
//		if (!polygonLocked) drawCursor();
//		drawVertex();
//	}
//	
//	
//	void drawLines()
//	{
//		if(lines.Count > 0)
//		{
//			foreach(Line l in lines)
//			{
//				Drawer.DrawLine(l.drawSrc,l.drawDst,lineTex,l.width);
//			}
//		}
//	}
//	
//	void drawTempLine()
//	{
//		if(lineStarted == true)
//		{
//			lineToAdd.dst = new Vector2(mousePosX,mousePosY);
//			Drawer.DrawLine(lineToAdd.src,lineToAdd.dst,tempLineTex,tmpLneW);
//		}
//	}
//	
//	void drawVertex()
//	{
//		for(int i=0;i<lines.Count;i++)
//			{
//				Line l = (Line)lines[i];
//				if(i==0)
//				{
//					Drawer.DrawIcon(l.src,vertex,vertex.width/2);
//				}
//				Drawer.DrawIcon(l.dst,vertex,vertex.width/2);
//			}	
//	}
//	
//	void drawCursor()
//	{
//		Drawer.DrawIcon(new Vector2(mousePosX,mousePosY),cursor);
//	}
//	
//	void abort()
//	{
//		while(lines.Count>0)
//		{
//	       	lineStarted = false;
//			lines.RemoveAt(lines.Count-1);
//		}
//		polygonLocked = false;
//		maskdone = false;
//		MASK = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
//		//if(GameObject.Find("/Background/backgroundImage").GetComponent("Main")!=null) 
//			//GameObject.Find("/Background/backgroundImage").guiTexture.texture = GameObject.Find("/Background/backgroundImage").GetComponent<Main>().image.getTexture();
//		DestroyImmediate(GameObject.Find("/Background/backgroundImage").GetComponent("InpaintMain"));
//	    DestroyImmediate(this);
//	}
//    
//} // class Polygon
