using UnityEngine;
//using UnityEditor;
using System.Collections;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;





//-----------------------------------------------------------------------------

public class ARImage: MonoBehaviour {
	

	public Texture2D tex; //backgroundImage/GUITexture/Texture
	public Transform[] obj;
    public int width;
	public int height;
	public int channels = 3;
	public double confidenceValue = 0.4;	
	
	//public Material imagePlaneMaterial;
	//public Texture2D texSD;
	//public int widthSD;
	//public int heightSD;
	
	//public string pathHD;
	//public string pathSD;
	//public string prefix = "file://";
	//public string temp_path;



/* /////////////////////////////////////////////////////////////////////////////////////////////////
 * 
 * 			**********   START  ***********
 * 
 * /////////////////////////////////////////////////////////////////////////////////////////////////*/
	void Start () {
	
		/* attacher groupe d'objets à placer automatiquement */
		obj = new Transform[1];
		obj[0] = GameObject.Find("MainScene/camPivot/mainCam").transform;
//		obj[0] = GameObject.Find("/MainNodeTest/Camera").transform;
//		obj[0] = GameObject.Find("/MainNodeTest/objects").transform;
		
		/* Récupération de la texture background courante */		
		tex = (Texture2D) GameObject.Find("Background/backgroundImage").GetComponent<GUITexture>().texture;
		width = tex.width;
		height = tex.height;
		
		/******************************/
		/***/ launchAR();
				processAR();
		/******************************/		
		
	
	//temp_path = pathHD;
	}


/* /////////////////////////////////////////////////////////////////////////////////////////////////
 * 
 * 			**********   UPDATE  ***********
 * 
 * /////////////////////////////////////////////////////////////////////////////////////////////////*/
	void Update()
	{
		// gestion fermeture de l'appli
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            InterProcess.Stop();
#if UNITY_STANDALONE_WIN 
            System.Diagnostics.Process.GetCurrentProcess().Kill(); 
#else
            UnityEngine.Application.Quit(); 
#endif
        }
		///////////////////////////////////////////////////////////
		
		
		
//		/* gestion changement d'image background */		
//		if((Texture2D) GameObject.Find("/Background/backgroundImage").GetComponent<GUITexture>().texture != tex)
//		{
//			tex = null;
//			tex = (Texture2D) GameObject.Find("/Background/backgroundImage").GetComponent<GUITexture>().texture;
//			width = tex.width;
//			height = tex.height;
//			DllImport.releaseImage();
//			launchAR();	
//			processAR();
//		}
		
		  /* gestion changement d'image background */  

	  if((Texture2D) GameObject.Find("Background/backgroundImage").GetComponent<GUITexture>().texture != tex)
	  {
	
	   tex = null;
	
	   DllImport.releaseImage();
	
	   /* réinitialisation position camPivot */
	
	   GameObject.Find("MainScene/camPivot").transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);	
	   GameObject.Find("MainScene/camPivot").transform.localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
			
	   GameObject.Find("MainScene/camPivot/mainCam").transform.localPosition = new Vector3(0.0f, 10.0f, -40.0f);
	
	   GameObject.Find("MainScene/camPivot/mainCam").transform.localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
			
	
	   /* arret RA */
	
	   DestroyImmediate(this);   
	
	  }
		}
	
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnMouseDown() {
	}
	
	
	/* MENUS */
	public void OnGUI()
	{	
//		pathHD = GUI.TextField(new Rect(10,500,700,25),pathHD);
//								
//		
//		
//		if(GUI.Button(new Rect(10,40,150,25),"Retour menu principal")){
//			Debug.Log("Retour menu");
//			Application.LoadLevel(0);									
//		}
//		
//		if(GUI.Button(new Rect(10,70,150,25),"Finaliser la composition")){
//			GUI.enabled = false;
//			Application.CaptureScreenshot("/res.png");
//			GUI.enabled = true;
//		}
//				

		
//		int toolbarInt = 0;
//		String[] toolbarStrings  = {"Toolbar1", "Toolbar2", "Toolbar3"};
//		
//		int selected = GUI.Toolbar(new Rect(10,Screen.height-35,Screen.width,25),toolbarInt, toolbarStrings);
		//Debug.Log("selected: "+selected);
						

	}
	//-------------------------------------------------------------------------

	public void OnApplicationQuit(){
		Debug.Log("On quit");
		DllImport.releaseImage();
		Destroy(tex);
    }	
	
	public void launchAR()
	{


			
			//	texHD = new Texture2D(widthHD,heightHD, TextureFormat.RGB24, true);
				
				
			//	loading.LoadImageIntoTexture(texHD);									// load HD image into texture
			
				
//				Color[] texttemp = texHD.GetPixels();
//				texHD.SetPixels(texttemp);	
			
//////////////////////////////////////////////////////////////////////////////
//  IN -> Texture2D + w + h			
///////////////////////////////////////////////////////////////////////////////
				byte[] buffer = new byte[width * height * 3];
				for(int j = 0; j< height; j++){		
					for(int i = 0; i< width; i++){

						Color temp = (tex.GetPixel(i, j));
					
						buffer[((height - j -1) * width * 3) +(i * 3 + 0)] = (byte) (temp.r * 255);
						buffer[((height - j -1) * width * 3) +(i * 3 + 1)] = (byte) (temp.g * 255);
						buffer[((height - j -1) * width * 3) +(i * 3 + 2)] = (byte) (temp.b * 255);
						// ! image traitee à l'envers
					}			
				}
			
				DllImport.sendBuffertoDLL(buffer, width, height, width * height * 3);
				Debug.Log("buffer RGB24 sent into DLL");
				


	
		/* initialisation of the system, with camera intrinsec parameters, markers... */
			if(DllImport.initARforImage()) Debug.Log("AR system initialized!");
			else 						   Debug.Log("Error while initializing AR");			
///////////////////////////////////////////////////////////////////////////////////////////////////////////////

			
			////
			Debug.Log("Nr of channels: "+ DllImport.getChannels());
			////
			
			////
			if(DllImport.processImage())	Debug.Log("Image processed!");			// process image 
			else Debug.Log("Image not processed!");									// (detect ARmarkers, get AR info, transfo matrix)
			/////
	}

	
	
	
	public void processAR(){
	
		
////////////////////////////////////////////////////////////////////////////////////////////////////
			bool enough = false;
			for(int i=0; i<DllImport.nrOfDetectedMarker();i++)
			{
				Debug.Log("ID: "+DllImport.getID(i)+" area detected : "+DllImport.getAreaSize(i)+ " cf:"+DllImport.getConfidenceValue(i)+" dir: "+DllImport.getDir(i)+" X: "+DllImport.getCenterX(i)+" Y "+DllImport.getCenterY(i));			
				if(DllImport.getConfidenceValue(i)> confidenceValue) enough = true;
				//val[DllImport.getCenterY(i)*height + DllImport.getCenterX(i)] = new Color(0.0f,255.0f,0.0f);
			}
		
			double[] trans = new double[3];
			double[] rot = new double[4];
		
			/* cas ou il n'y a pas de mouvement */
			if(!DllImport.getDetectedTranslationRotation(trans, rot, 0))
			{
				Debug.Log("RIEN");
				//continue;
			}
			/* Cas où un mouvement et un marqueur est correctement detecté: calcul de la nouvelle position avec interpolation de mouvement */
			else if(DllImport.getDetectedTranslationRotation(trans, rot, 0)/* && enough ==true*/)
			{		
					//Vector3 newPos = new Vector3(-(float)trans[0] * 1.5f, -(float)trans[1] * 1.5f, (float)trans[2]);//
						/* tentative mirroring */
					//T x=+*1.5, y-*1.5, z=+*1
					//Q x=+, y=-, z=+ , w=+
					//Vector3 newPos = new Vector3((float)trans[0] , -(float)trans[1], (float)trans[2]);//OK
			
			
					//OK si on bouge l'objet
//					Vector3 newPos = new Vector3((float)trans[0]*2.0f , -(float)trans[1]*2.0f, (float)trans[2]);//OK
//			        newPos /= 40;		
//					Quaternion newRot = new Quaternion();
//					newRot = new Quaternion((float)rot[0], -(float)rot[1], (float)rot[2], (float)rot[3]);
//					Quaternion quat90 = Quaternion.AngleAxis(90.0f,  new Vector3(1f, 0f, 0f));			
//					newRot = newRot*quat90;		
//					Debug.Log("newPos 1 : "+newPos.ToString());
					//Fin si on bouge l'objet
			
					//test si on bouge la camera
					Vector3 newPos = new Vector3(-(float)trans[0]*2.0f , (float)trans[1]*2.0f, -(float)trans[2]);//OK
			        newPos /= 40;		
					Quaternion newRot = new Quaternion();
					newRot = new Quaternion((float)rot[0], -(float)rot[1], (float)rot[2], (float)rot[3]);
					Quaternion quat90 = Quaternion.AngleAxis(90.0f,  new Vector3(1f, 0f, 0f));			
					newRot = newRot*quat90;	
					newRot = Quaternion.Inverse(newRot);
//					Quaternion quat180 = Quaternion.AngleAxis(180, new Vector3(0.0f,0.0f,1.0f));
//					newRot = newRot*quat180;					
					newPos = newRot*newPos; 			
					//Fin si on bouge la camera

//					Vector3 newPos = new Vector3(-(float)trans[0]*2.0f , (float)trans[1]*2.0f, (float)trans[2]);//OK
//			        newPos /= 40;		
//					Quaternion newRot = new Quaternion();
//					newRot = new Quaternion((float)rot[0], (float)rot[1], (float)rot[2], (float)rot[3]);
//					newRot = Quaternion.Inverse(newRot);
//					Quaternion quat180 = Quaternion.AngleAxis(-180.0f,  new Vector3(1.0f, 1.0f, 1.0f));			
//					newRot = newRot*quat180;
//					Quaternion quat45 = Quaternion.AngleAxis(90.0f,  new Vector3(1.0f, 1.0f, 1.0f));			
//					newRot = newRot*quat45;	
//					Quaternion quat90 = Quaternion.AngleAxis(90.0f,  new Vector3(0.0f, 1.0f, 1.0f));			
//					newRot = newRot*quat90;				
//					newPos = newRot*newPos; 	
		
			
				for(int i=0; i<obj.Length; i++)
				{
				//	if(Vector3.Distance(obj[i].position, newPos) > 10f) 
					obj[i].position = newPos;
				//	if(Quaternion.Angle(obj[i].rotation, newRot) > 4f) 
					obj[i].rotation = newRot;
				}
			}
			/*  Cas où aucun marqueur correct est détecté: les objets sont renvoyés derrière la scène (ou trouver un moyen de désactiver leur affichage) */
			else if (enough == false)
			{
				Debug.Log("Hide");
				for(int i=0; i<obj.Length; i++)
				{
				obj[i].localPosition = Vector3.Lerp(obj[i].position, new Vector3(0, 0, -500.0f), Time.deltaTime*200);
				obj[i].localRotation = Quaternion.Slerp(obj[i].rotation, new Quaternion(0,0,0,0), Time.deltaTime*200);
			    }
			}
				

	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

}
