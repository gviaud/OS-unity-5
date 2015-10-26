using UnityEngine;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
//if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//using System.Windows.Forms;
//endif

using Pointcube.Utils;
using Pointcube.Global;


public class Screenshot : MonoBehaviour
{
	
	#region variables
//    int resWidth = 4000; 
//    int resHeight = 3000;
	
//	private int           resWidth  = UnityEngine.Screen.width;
//  private int           resHeight = UnityEngine.Screen.height;

	private string        unique;
	
	private GameObject    mainScene;
	private Camera        cam;
	
	private int           inW;
	private int           inH;
	
	private byte[]        imgBytes;
	
	private RenderTexture rt;
	
	private Texture2D     m_screenShot;
	
	private string        text;                 // Texte de debug
		
//	private string path2gallery = "";
	
	//private int ql;
	
	private Texture2D w_BG;
	private Texture2D _logo;
	
	private Rect w_rect;
	
	private const int maxLogoW = 256;
	private const int maxLogoH = 256;

    // -- Références scène --
    public  GameObject      m_backgroundImage;

    // -- Rects GUI --
    private Rect            m_texRect;
    private Rect            m_label1Rect;
    private Rect            m_label2Rect;
    private Rect            m_logoTexRect;

    private bool            m_modeMontage;      // Mode montage : watermark en bas de la photo
                                                // Mode2D : watermark en bas de l'écran

	public bool w_show;
	
	public GUISkin skin;

    private static readonly string DEBUGTAG = "Screenshot : ";
    private static readonly bool   DEBUG    = false;
	#endregion

    #region unity_func
    //-----------------------------------------------------
	void Awake()
	{
		usefullData.logoUpdated += 	LogoUpdate;
	}
	
    //-----------------------------------------------------
	void Start()
	{
		mainScene = GameObject.Find("MainScene");
		cam = GameObject.Find("backgroundCamera").GetComponent<Camera>();

        // -- Rects --
        m_texRect     = new Rect();
        m_label1Rect  = new Rect();
        m_label2Rect  = new Rect();
        m_logoTexRect = new Rect();

        // -- Références scène --
        if(m_backgroundImage == null)
            Debug.LogError(DEBUGTAG+" Background Image not set. Please set it in the inspector.");

#if UNITY_IPHONE
		EtceteraManager.mailComposerFinishedEvent += endMail;
#endif
		
		w_BG = new Texture2D(1,1);
		w_BG.SetPixel(0,0,new Color(1,1,1,0.5f));
		w_BG.Apply();
		
//		if(usefullData._logo != null)
//		{
//			int logoW = usefullData._logo.width;
//			int logoH= usefullData._logo.height;
//			float ratio = (float)logoW/(float)logoH;
//			
//			if(logoW > maxLogoW || logoH > maxLogoH)
//			{
//				if(logoW>logoH)
//					w_rect = new Rect(10,10,maxLogoW,maxLogoW/ratio);
//				else
//					w_rect = new Rect(10,10,maxLogoH*ratio,maxLogoH);
//			}
//			else
//			{
//				w_rect = new Rect(10,10,logoW,logoH);
//			}
//		}
		
	}
	
    //-----------------------------------------------------
	void OnGUI()
	{
//		GUI.color = new Color(0,0,0,1);                 // Debug
//		GUI.Label(new Rect(150,150,400,400),text);
//		GUI.color = new Color(1,1,1,1);

		//WATERMARK
		if(w_show && Event.current.type == EventType.Repaint)
		{
            string madeWithText = "";
            if(usefullData._edition == usefullData.OSedition.Lite)
                madeWithText = TextManager.GetText("Screenshot.MadeWithOS3DExpress");
            else
                madeWithText = TextManager.GetText("Screenshot.MadeWithOS3D");

            SetRects(m_modeMontage);
			GUI.skin = skin;
			GUI.DrawTexture(m_texRect, w_BG);
			GUI.Label(m_label1Rect,TextManager.GetText("Screenshot.Contract"),"txtGauche");
			GUI.Label(m_label2Rect, madeWithText,"txtDroite");
			
			if(_logo != null)
				GUI.DrawTexture(m_logoTexRect, _logo);
		}
	}

    //-----------------------------------------------------
    void OnDestroy()
    {
        usefullData.logoUpdated -= LogoUpdate;
    }

    #endregion

    //-----------------------------------------------------
    private void SetRects(bool modeMontage)
    {
        int botStripeH = 12;
        Rect bgTexRect;
        if(modeMontage)
            bgTexRect = m_backgroundImage.GetComponent<GUITexture>().pixelInset;
        else
            bgTexRect = new Rect(0f, 0f, UnityEngine.Screen.width, UnityEngine.Screen.height);

        m_texRect.Set(bgTexRect.x, bgTexRect.y+bgTexRect.height-botStripeH, bgTexRect.width, botStripeH);
        m_label1Rect.Set(bgTexRect.x + 10, bgTexRect.y+bgTexRect.height-botStripeH, bgTexRect.width/2, botStripeH);
        m_label2Rect.Set(bgTexRect.x + bgTexRect.width/2-10, bgTexRect.y+bgTexRect.height-botStripeH, bgTexRect.width/2, botStripeH);

        m_logoTexRect.Set(bgTexRect.x+10,bgTexRect.y+10,usefullData._logoRect.width,usefullData._logoRect.height);
    }

//	void imagePickerChoseImage( string imagePath )
//	{
//		path2gallery = imagePath;
//	}
	
    //-----------------------------------------------------
    // Fonction interface
	
	//Fonctions ANNEXES
//	public void setLogo(Texture2D logo)
//	{
//		if(usefullData._logo != null)
//		{
//			usefullData._logo = logo;
//			
//			int logoW = usefullData._logo.width;
//			int logoH= usefullData._logo.height;
//			float ratio = (float)logoW/(float)logoH;
//			
//			if(logoW > maxLogoW || logoH > maxLogoH)
//			{
//				if(logoW>logoH)
//					w_rect = new Rect(10,10,maxLogoW,maxLogoW/ratio);
//				else
//					w_rect = new Rect(10,10,maxLogoH*ratio,maxLogoH);
//			}
//			else
//			{
//				w_rect = new Rect(10,10,logoW,logoH);
//			}
//		}
//	}
	
	#region Screenshot pour preview sauvegarde, enregistrement image et mail
	
	//	public void TakeShot()
//	{
//		StartCoroutine("TakeHiResShot");
//	}

    //-----------------------------------------------------
    // Routine "principale"
//    IEnumerator TakeHiResShot() // obsolete
//	{
//		unique = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
//		yield return StartCoroutine(takeScreenShot(false));
//		yield return StartCoroutine(saveImg(screenShot,false));
//		yield return StartCoroutine(takeScreenShot(true));
//		yield return StartCoroutine(saveImg(screenShot,true));
//    }
	
    //-----------------------------------------------------
    // Routine de prise du screenshot
//	
//	IEnumerator takeScreenShot(bool thumb)
//	{
//		//ql = QualitySettings.GetQualityLevel();
//		overrideUI(true);
//		yield return new WaitForSeconds(5);
//		if(thumb)
//		{
//			inW = 256;
//			inH = 256;
//		}
//		else
//		{
//			//QualitySettings.SetQualityLevel(5);
//		
//			inW = UnityEngine.Screen.width;//2*resWidth;
//			inH = UnityEngine.Screen.height;// 2*resHeight;
//			
//		}
////		yield return Camera.mainCamera.GetComponent<loadBackground>().ratioHD(inW,inH);
//		
//		rt = new RenderTexture(inW, inH,32,RenderTextureFormat.ARGB32);
//		rt.useMipMap = false;
//		rt.isPowerOfTwo = false;
//		
//		Camera.mainCamera.targetTexture = rt;
//        cam.targetTexture = rt;
//        screenShot = new Texture2D(inW, inH, TextureFormat.ARGB32, false);
//        
//		cam.Render();
//		yield return new WaitForEndOfFrame();
//		Camera.mainCamera.Render();
//		yield return new WaitForEndOfFrame();
//        RenderTexture.active = rt;
//        screenShot.ReadPixels(new Rect(0, 0, inW, inH), 0, 0);
//        
//		cam.targetTexture = null;
//		Camera.mainCamera.targetTexture = null;
//        RenderTexture.active = null; // JC: added to avoid errors
//        Destroy(rt);
//		
//		yield return saveImg(screenShot,thumb);
//		
//		string filename = usefullData.PhotoPath+"screen_"+unique+".png";
//		
//		if(!thumb)
//			EtceteraBinding.saveImageToPhotoAlbum(filename);
//		
//		Camera.mainCamera.GetComponent<loadBackground>().ratio();
////		if(!thumb)
////		{
////			QualitySettings.SetQualityLevel(ql);
////		}
//		overrideUI(false);
//		yield return true;
//	}
//	OBSOLETE
	
	//-----------------------------------------------------
    // Routine de prise du screenshot pour pc
	// si insideSave = true > screenshot pour preview sauvegarde
	// sinon Screenshot a sauvegarder ailleur (save file dialogBox Windows)
	public IEnumerator takeScreenShotPC(bool insideSave, bool modeMontage = true, System.Action action = null)
	{
		if((PC.DEBUG && DEBUG) || PC.DEBUGALL)
            Debug.Log(DEBUGTAG+"takeScreenShotPC");

        bool modeBkup = m_modeMontage;  // !modeMontage = mode2D
        m_modeMontage = modeMontage;

        yield return Resources.UnloadUnusedAssets();
		Camera.main.GetComponent<ObjInteraction>().enabled = false;
		Camera.main.GetComponent<GuiTextureClip>().enabled = false;
		//maskage de la GUI
		overrideUI(true);
		yield return new WaitForEndOfFrame();
		if(!insideSave)
			w_show = true;
		else
			mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);

        usefullData.showVersion = false;
		yield return new WaitForEndOfFrame();
		
        //Récuperation du path
        string fileName = "thumbnail"+System.DateTime.Now.ToString("_HH-mm-ss")+".jpg";
        string path = UnityEngine.Application.persistentDataPath+"/"+fileName;

        //SCREENSHOT HD
        //UnityEngine.Application.CaptureScreenshot(savePath,3);
        //SCREENSHOT SD
        UnityEngine.Application.CaptureScreenshot(path);   // Sauvegarde temporaire
        mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);    
        int frame = 0;
        while( !System.IO.File.Exists( path ) && frame < 300 )
        {
            yield return null;
            ++frame;
        }
        yield return new WaitForSeconds(0.1f);
        
		path = SetNonMobilePlateformSlashes(path);  // "\" pour win, "/" pour osx

		WWW www = new WWW("file://"+path);
        yield return www.isDone;

        if(m_screenShot == null)
             m_screenShot = new Texture2D(1,1);

        www.LoadImageIntoTexture(m_screenShot);

		if(insideSave)  // Sauvegarde de projet (miniature)
		{
            Montage.cdm.updateThumb(m_screenShot);
			yield return new WaitForEndOfFrame();
		}
		else            // Sauvegarde d'image (export screenshot) vs. sauvegarde projet
		{
            // -- Passer temporairement en plein √©cran (pour des tailles de screenshot homog√®nes) --
            // Note : ne marche pas sur vista ou 7 si on a plusieurs √©crans...
//            bool windowed = !Screen.fullScreen;
//            if(windowed)
//            {
//                Screen.fullScreen = true;
//                yield return new WaitForEndOfFrame();
//            }

            if(modeMontage)
                RemoveStripsIfNeeded();      // Enlever les bandes autour de la photo si besoin

            // -- Enregistrer sur le disque a l'endroit indique --
            string savePath = IOutils.AskPathDialog(TextManager.GetText("Dialog.SaveImageFile"),
                                                   "jpg", TextManager.GetText("Dialog.ImageFiles"));
            if(savePath != "")
            {
                if(PC.DEBUG && DEBUG)
                    Debug.Log(DEBUGTAG+"Saving (PC) image to "+savePath);
                IOutils.SaveImageFile(m_screenShot, savePath);

            }
            Destroy(m_screenShot);

//            if(windowed)
//                Screen.fullScreen = false;
        }

//        File.Delete(path);
        www.Dispose();

		//rétablissement de la qualité de rendu et de la gui
		//QualitySettings.SetQualityLevel(ql,false);
        if(modeMontage)
        {
    		Camera.main.GetComponent<ObjInteraction>().enabled = true;
    		Camera.main.GetComponent<GuiTextureClip>().enabled = true;
    //		mainScene.GetComponent<GUIMenuMain>().SetHideAll(false);
        }
        mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
        w_show = false;                     // Watermark
        usefullData.showVersion = true;
		overrideUI(false);

        m_modeMontage = modeBkup;

        if(action != null)
            action();

		yield return true;
	}
	
	//-----------------------------------------------------
    // Fonction de sauvegarde de l'image
//	IEnumerator saveImg(Texture2D img,bool isThumb)
//	{
//		string filename;
//		
//		if(isThumb)
//		{
//			filename = usefullData.PhotoThumbPath+"screen_"+unique+".png";
//			Texture2D thumb = new Texture2D(256,256);
//			thumb.SetPixels(img.GetPixels());
//			thumb.Apply();
//			imgBytes = thumb.EncodeToPNG();
//		}
//		else
//		{
//			filename = usefullData.PhotoPath+"screen_"+unique+".png";
//			Debug.Log("Saved in "+filename);
//			imgBytes = img.EncodeToPNG();
//		}
//		text += "\n"+filename;
//		
//        System.IO.File.WriteAllBytes(filename, imgBytes);
//		yield return true;
//	}
// 	OBSOLETE
    
	//-----------------------------------------------------
    // Fonction de cr√©ation du thumbnail pour la sauvegarde
//	public IEnumerator saveThumbnail()
//	{
//		yield return StartCoroutine(takeScreenShot(false));
//		GameObject.Find("MainScene").GetComponent<Montage>().getcdm().updateThumb(screenShot);
//		yield return true;
//	}
//	OBSOLETE

    //------------------------------------------------------
    private void RemoveStripsIfNeeded()
    {

        Rect bgTexRect = m_backgroundImage.GetComponent<GUITexture>().pixelInset;

		float newHeight = bgTexRect.height;
		float newWidth = bgTexRect.width;
		float newX = bgTexRect.x;
		float newY = bgTexRect.y;

		//Vérification si l'image est rogné en haut et en bas
		if( bgTexRect.y < 0.0f && (bgTexRect.y + bgTexRect.height) > m_screenShot.height)
		{
			newHeight = m_screenShot.height;
			newY = 0;
	
		}
		else
		{
			//Vérification si l'image est rogné par le bas
			if( bgTexRect.y < 0.0f)
			{
				newHeight += bgTexRect.y;
				newY = 0;
			
			}
			//Vérification si l'image est rogné par le haut
			else if( (bgTexRect.y + bgTexRect.height) > m_screenShot.height )
			{
				newHeight = m_screenShot.height - (m_screenShot.height - bgTexRect.y);

			}
		}
		//Vérification si l'image est rogné à gauche est droite
		if( bgTexRect.x < 0.0f && (bgTexRect.x + bgTexRect.width) > m_screenShot.width)
		{
			newWidth = m_screenShot.width;
			newX = 0;
		
		}
		else
		{
			//Vérification si l'image est rogné à gauche
			if( bgTexRect.x < 0.0f)
			{
				newWidth += bgTexRect.x;
				newX = 0;
			
			}
			//Vérification si l'image est rogné à droite
			else if( (bgTexRect.x + bgTexRect.width) > m_screenShot.width )
			{
				newWidth = m_screenShot.width - bgTexRect.x;
			
			}
		}

        if(m_screenShot.width != bgTexRect.width || m_screenShot.height != bgTexRect.height)
        {
			Color[] pixels = m_screenShot.GetPixels((int) newX,     (int) newY,
			                                        (int) newWidth, (int) newHeight);
			m_screenShot.Resize((int) newWidth, (int) newHeight);
            m_screenShot.SetPixels(pixels);
        }
    }

	// -----------------------------------------------------
	// Fonction de screenshot pour preview sauvegarde iPad Only
	public IEnumerator saveThumbnailETC()
	{
		//maskage de la GUI
		yield return Resources.UnloadUnusedAssets();
		Camera.main.GetComponent<ObjInteraction>().enabled = false;
		//mainScene.GetComponent<GUIMenuMain>().SetHideAll(true);
		overrideUI(true);
		
		Camera.main.GetComponent<GuiTextureClip>().enabled = false;
		//w_show = true;
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
		yield return new WaitForSeconds (1);
		
		//shoot
		string fileName = "thumbnail"+System.DateTime.Now.ToString("_HH-mm-ss")+".jpg";
#if UNITY_IPHONE
		yield return StartCoroutine(EtceteraBinding.takeScreenShot(fileName));
#endif
		string tmpPath = UnityEngine.Application.persistentDataPath+"/"+fileName;
		
#if UNITY_ANDROID
		UnityEngine.Application.CaptureScreenshot(fileName);
		yield return new WaitForEndOfFrame();
		int frame = 0;
		while( !System.IO.File.Exists( tmpPath ) && frame < 300)
		{
			yield return null;
			++frame;
		}
#endif		
		// affichage du please wait panel
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		yield return new WaitForEndOfFrame();
		
		//recuperation du path 
//		string tmpPath = UnityEngine.Application.persistentDataPath+"/"+fileName;
		Debug.Log(tmpPath + "Exists ? "+File.Exists(tmpPath));
		//w_show = false;
		
		// Do the job
		if(File.Exists(tmpPath))
		{
#if UNITY_IPHONE
			//EtceteraBinding.resizeImageAtPath(tmpPath,UnityEngine.Screen.width/4,UnityEngine.Screen.height/4);
#endif
#if UNITY_ANDROID
			EtceteraAndroid.scaleImageAtPath(tmpPath,0.25f);
#endif
//			Texture2D thumb = new Texture2D(256,256);
			Texture2D thumb = new Texture2D((int) m_texRect.width/4, (int) m_texRect.height/4);
			WWW www = new WWW("file://" +tmpPath);
			yield return www;
			www.LoadImageIntoTexture(thumb);
			
			GameObject.Find("MainScene").GetComponent<Montage>().getcdm().updateThumb(thumb);
			
			yield return new WaitForEndOfFrame();
			File.Delete(tmpPath);
			www.Dispose();
		}
		
		//reaffichage de la GUI
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
		//mainScene.GetComponent<GUIMenuMain>().SetHideAll(false);
		overrideUI(false);
		
		Camera.main.GetComponent<ObjInteraction>().enabled = true;
		Camera.main.GetComponent<GuiTextureClip>().enabled = true;
		yield return true;
	}
	
	// -----------------------------------------------------
	// Fonction de screenshot pour envoi par mail iPad Only
	public IEnumerator mailShot(System.Action action = null, string szobject = "Montage ONESHOT 3D")
	{
		Debug.Log("start mail shot");
		yield return Resources.UnloadUnusedAssets();
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		Camera.main.GetComponent<ObjInteraction>().enabled = false;
		Camera.main.GetComponent<GuiTextureClip>().enabled = false;
		//changement de la qualit√©
		//ql = QualitySettings.GetQualityLevel();
		//QualitySettings.SetQualityLevel(5,false);
		//maskage de la GUI
		mainScene.GetComponent<GUIMenuMain>().SetHideAll(true);
		overrideUI(true);
		
		w_show = true;
		usefullData.showVersion = false;
		
		yield return new WaitForEndOfFrame();
		
		string attachedName = mainScene.GetComponent<Montage>().getClientStr() + "_" + System.DateTime.Now.ToString("dd-mm-yyyy_HH-mm-ss");
		//ql = QualitySettings.GetQualityLevel();
		//QualitySettings.SetQualityLevel(5,false);
		w_show = true;
		usefullData.showVersion = false;
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
		yield return new WaitForSeconds (1);
		
#if UNITY_IPHONE
		EtceteraManager.setPrompt(true);
		//yield return StartCoroutine(EtceteraBinding.showMailComposerWithScreenshotNamed(this,"",szobject,"",false,attachedName));
		yield return StartCoroutine(EtceteraBinding.showMailComposerWithScreenshot("",szobject,"",false));
#endif
#if UNITY_ANDROID
		string path = UnityEngine.Application.persistentDataPath+"/"+attachedName;
		UnityEngine.Application.CaptureScreenshot(attachedName+".jpg");
		yield return new WaitForEndOfFrame();
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);	
		int frame = 0;
		while( !System.IO.File.Exists( path+".png" ) && frame < 300 )
		{
			yield return null;
			++frame;
		}
		yield return new WaitForEndOfFrame();
		EtceteraAndroid.showEmailComposer( "", "ONESHOT 3D REVOLUTION", "", true, path+".jpg");
		yield return new WaitForEndOfFrame();
#endif
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		Camera.main.GetComponent<ObjInteraction>().enabled = true;
		
		
		overrideUI(false);		
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
		w_show = false;
		
		if(action != null)
		{
			action();
		}
		else
		{
//#if UNITY_ANDROID
			Camera.main.GetComponent<GuiTextureClip>().enabled = true;
			usefullData.showVersion = true;	
//#endif
		}
		yield return true;
	}
	
	// -----------------------------------------------------
	// Fonction de RETOUR du screenshot pour preview sauvegarde iPad Only
	void endMail(string result)
	{
		Debug.Log("Mail Sender : "+result);
#if UNITY_IPHONE
		EtceteraManager.setPrompt(false);
#endif
		
		//QualitySettings.SetQualityLevel(ql,false);
//		mainScene.GetComponent<GUIMenuMain>().SetHideAll(false);
		overrideUI(false);
		
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
		w_show = false;
		Camera.main.GetComponent<GuiTextureClip>().enabled = true;
		usefullData.showVersion = true;
	}
	
	// -----------------------------------------------------
	// Fonction de screenshot pour sauvegarde dans la gallery iPad Only
	public IEnumerator galleryShot(System.Action endAction = null)
	{
		Debug.Log("start gallery shot");
		yield return Resources.UnloadUnusedAssets();
		Camera.main.GetComponent<ObjInteraction>().enabled = false;
		Camera.main.GetComponent<GuiTextureClip>().enabled = false;
		//changement de la qualit√©
		//ql = QualitySettings.GetQualityLevel();
		//QualitySettings.SetQualityLevel(5,false);
		
		//maskage de la GUI
		//mainScene.GetComponent<GUIMenuMain>().SetHideAll(true);
		overrideUI(true);
		
		w_show = true;
		usefullData.showVersion = false;
		yield return new WaitForSeconds (1);
		//screenshot
		string fileName = mainScene.GetComponent<Montage>().getClientStr() + "_" + System.DateTime.Now.ToString("dd-mm-yyyy_HH-mm-ss")+".jpg";
		string tmpPath = UnityEngine.Application.persistentDataPath+"/"+fileName;
		Debug.Log("fileName test "+fileName);

        // -- Capture d'√©cran vers le cache de l'application --
#if UNITY_IPHONE
		yield return StartCoroutine(EtceteraBinding.takeScreenShot(fileName));
#endif
#if UNITY_ANDROID
		UnityEngine.Application.CaptureScreenshot(fileName);
		yield return new WaitForEndOfFrame();
		int frame = 0;
		while( !System.IO.File.Exists( tmpPath ) && frame < 300 )
		{
			yield return null;
			++frame;
		}
#endif

		Debug.Log("takeScreenShot test "+fileName);
		//affichage ecran d'attente
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		yield return new WaitForSeconds(0.5f);
		//r√©cup√©ration du screenshot et enregistrement dans la gallery

//		string tmpDeletePath = "/private" + UnityEngine.Application.persistentDataPath+"/"+fileName;
		
//		Debug.Log("tmpPath test "+tmpPath);
//		Debug.Log(tmpPath + "Exists ? "+File.Exists(tmpPath));
		
		if(File.Exists(tmpPath))
		{
//		    Debug.Log("Exists test "+tmpPath);

            // -- Crop to photo rectangle (to get rid of gray bars) --
            Rect photoRect = GameObject.Find("backgroundImage").GetComponent<GUITexture>().pixelInset;
            if(!photoRect.Equals(new Rect(0f, 0f, Screen.width, Screen.height)))
            {
                Texture2D screenshot = new Texture2D(1,1);
                WWW www = new WWW("file://"+tmpPath);   // TODO tester sur Mac
                yield return www.isDone;
                www.LoadImageIntoTexture(screenshot); // Open saved screenshot

				Debug.Log("taille differente ");
				float newHeight = photoRect.height;
				float newWidth = photoRect.width;
				float newX = photoRect.x;
				float newY = photoRect.y;

				//Vérification si l'image est rogné en haut et en bas
				if( photoRect.y < 0.0f && (photoRect.y + photoRect.height) > screenshot.height)
				{
					newHeight = screenshot.height;
					newY = 0;
				}
				else
				{
					//Vérification si l'image est rogné par le bas
					if( photoRect.y < 0.0f)
					{
						newHeight += photoRect.y;
						newY = 0;
					}
					//Vérification si l'image est rogné par le haut
					else if( (photoRect.y + photoRect.height) > screenshot.height )
					{
						newHeight = screenshot.height - (screenshot.height - photoRect.y);
					}
				}
				//Vérification si l'image est rogné à gauche est droite
				if( photoRect.x < 0.0f && (photoRect.x + photoRect.width) > screenshot.width)
				{
					newWidth = screenshot.width;
					newX = 0;
				}
				else
				{
					//Vérification si l'image est rogné à gauche
					if( photoRect.x < 0.0f)
					{
						newWidth += photoRect.x;
						newX = 0;
					}
					//Vérification si l'image est rogné à droite
					else if( (photoRect.x + photoRect.width) > screenshot.width )
					{
						newWidth = screenshot.width - photoRect.x;
					}
				}
				
				if(screenshot.width != photoRect.width || screenshot.height != photoRect.height)
				{

					Color[] pixels = screenshot.GetPixels((int) newX,     (int) newY,
					                                        (int) newWidth, (int) newHeight);

					screenshot.Resize((int) newWidth, (int) newHeight);
					screenshot.SetPixels(pixels);


				}

				/*
                if(screenshot.width != photoRect.width || screenshot.height != photoRect.height)
                {
                    Color[] pixels = screenshot.GetPixels((int) photoRect.x, (int) photoRect.y,
                                                          (int) photoRect.width, (int) photoRect.height);
                    screenshot.Resize((int) photoRect.width, (int) photoRect.height);
                    screenshot.SetPixels(pixels);
                }
				*/
                // -- Overwrite saved screenshot with cropped screenshot --

                IOutils.SaveImageFile(screenshot, tmpPath);
                Object.Destroy(screenshot);
            } // crop

            // -- Sauvegarde de l'image du cache vers la gallerie / l'album --
#if UNITY_IPHONE
			EtceteraBinding.saveImageToPhotoAlbum( tmpPath );
#endif
#if UNITY_ANDROID
			EtceteraAndroid.saveImageToGallery(tmpPath,fileName);		
#endif
		    Debug.Log("saveImageToPhotoAlbum test "+tmpPath);
			yield return new WaitForSeconds(Time.deltaTime);
		    Debug.Log("saveImageToPhotoAlbum ok "+tmpPath);
		}

		//r√©tablissement de la qualit√© de rendu et de la gui
		//QualitySettings.SetQualityLevel(ql,false);
		Camera.main.GetComponent<ObjInteraction>().enabled = true;
		Camera.main.GetComponent<GuiTextureClip>().enabled = true;
		w_show = false;
		usefullData.showVersion = true;
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
//		mainScene.GetComponent<GUIMenuMain>().SetHideAll(false);
		overrideUI(false);
//		usefullData.showVersion = false;
		
		/*if(File.Exists(tmpDeletePath))
		{
			//File.Delete(tmpDeletePath);
			System.IO.File.Delete(tmpDeletePath);
		}*/

        if(endAction != null)
            endAction();
        
		yield return true;
	}

    //-----------------------------------------------------
	public void ShowHideGui(bool show)
	{
		if(show)
		{
			Camera.main.GetComponent<ObjInteraction>().enabled = true;
			Camera.main.GetComponent<GuiTextureClip>().enabled = true;
			w_show = false;
			usefullData.showVersion = true;
			overrideUI(false);
		}
		else
		{
			Camera.main.GetComponent<ObjInteraction>().enabled = false;
			Camera.main.GetComponent<GuiTextureClip>().enabled = false;
			overrideUI(true);
			usefullData.showVersion = false;
		} 
	}

    //-----------------------------------------------------
	public IEnumerator GenericShot(Texture2D tex,bool showStuff)
	{
		Debug.Log("start generic shot");
		
		if(showStuff)
			w_show = true;
		
		yield return new WaitForEndOfFrame();
		
		string fileName = "tmpScr"+System.DateTime.Now.ToString("_HH-mm-ss")+".jpg";
		string path = UnityEngine.Application.persistentDataPath+"/"+fileName;
		
		#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
		UnityEngine.Application.CaptureScreenshot(path);
		yield return new WaitForEndOfFrame();
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);	
		int frame = 0;
		while( !System.IO.File.Exists( path ) && frame < 300 )
		{
			yield return null;
			++frame;
		}
		#elif UNITY_IPHONE
		//shoot
		yield return StartCoroutine(EtceteraBinding.takeScreenShot(fileName));
		
		// affichage du please wait panel
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		yield return new WaitForEndOfFrame();
		#endif

		if(File.Exists(path))
		{
			//	#if UNITY_STANDALONE_WIN
			//	path = path.Replace("/","\\");
			//	#endif
			path = SetNonMobilePlateformSlashes(path);

			WWW www = new WWW("file://" +path);
			yield return www;
			www.LoadImageIntoTexture(tex);
			
			yield return new WaitForEndOfFrame();
			File.Delete(path);
			www.Dispose();
		}
		
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);	
		w_show = false;

		yield return tex;
	}
	#endregion
	
	// -----------------------------------------------------
	// FireEvent pour masquer/r√©afficher les √©l√©ments de GUI
	// lors du screenShot
	public void overrideUI(bool hide)
	{
        UsefullEvents.FireHideGUIforScreenshot(hide);
	}
	
	private void LogoUpdate(Texture2D t)
	{
		Debug.Log("[LOGO]>UPDATE FIRED SCREENSHOT");
		_logo = t;
	}
	
    //-----------------------------------------------------
    private string SetNonMobilePlateformSlashes(string path)
    {
#if UNITY_STANDALONE_WIN
        path = path.Replace("/","\\");
#else
        path = path.Replace("\\","/");      //if UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || ...
#endif
        return path;
    }
	
} // class Screenshot
