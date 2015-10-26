using UnityEngine;
using System.Collections;
using System.IO;


public class usefullData : MonoBehaviour {
	
	public static string PhotoPath;
	public static string LogoPath;
	public static string PhotoThumbPath;
	public static string SavePath;
	public static string SaveFileExtention = ".cube";
	public static string SaveNewFileExtention = ".cub";
	
	//url's
	//public static string ActivationUrl = "http://www.pointcube.com/activationOneshot/requests/activationDev.php";
	public static string ActivationUrl = "http://www.pointcube.com/activationOneshot/requests/activation2.php";
	public static string ActivationUrlLite = "http://www.pointcube.com/activationOneshot/requests/activation2.php";
	public static string DeactivationUrl = "http://www.pointcube.com/activationOneshot/requests/deactivation.php";
	public static string UserInfosUrl = "http://www.pointcube.com/activationOneshot/requests/getUserInfos.php";
	public static string CheckNewVersion = "http://www.pointcube.com/activationOneshot/checkNewVersion.php";
	public static string CheckNewVersionMac = "http://www.pointcube.com/activationOneshot/checkNewVersionMac.php";
	public static string CheckNewVersioniOS = "http://www.pointcube.com/activationOneshot/checkNewVersioniOS.php";
	
	//Key's
	public static string k_logIn = "LOGIN";
	public static string k_password = "PASSWORD";
	public static string k_data = "DATA";
	public static string k_lld = "LLD"; //last launch date
	public static string k_lld2 = "LLD2"; //last launch date
	
	public static string k_toLoadPath = "FILE2LOAD";
	public static string k_toLoadParams = "PARAMS2LOAD";
	
	public static string k_selectedClient = "CLIENT";
	public static string k_selectedProject = "PROJECT";
	public static string k_clientLink = "LINK";
	public static string k_startBypass = " STARTACTION";
	
	// Version
	public static bool showVersion = true;
	public const string version_ID = "1.9.2"; // v 0.x.y > yeme export du mois x (0 > pas encore released)
	public const int version_forMaj = 192; 	  // This number is used to know if there is a new version to display once 
											 // the MAJ info. The format is and int wich represent the number of version.
											 // You don't need to know if the version is upper or not.

	public static string version = "v "+version_ID; // v 0.x.y > yeme export du mois x (0 > pas encore released)
	private Rect vRect;
	public static int test_mode = 0;
	private static bool hasNewVersion = false;
	private static string newVersionLink = "";
	
	// Ã‰dition
	public enum OSedition {Full, Lite};
	public static OSedition _edition = OSedition.Full;
	public const bool c_ppu = false;
	
	// Logo
	public static Texture2D _logo;
	public static Rect _logoRect;
	public static bool _loadWorking = false;
	public static event System.Action<Texture2D> logoUpdated;
		
	// Scenes
	public static string sc_activation = "activation";
	public static string sc_clientExplorer = "explorateurClients";
	public static string sc_oneShot = "mainWithLibsAndExplorer";
	
    // API graphique
    public static bool s_openGL;
    public static bool s_d3d;
    public static bool lowTechnologie =  false;
	public static bool iPad2 =  false;
    public static bool smartPhone =  false;
    public static bool retinus =  false;
	
	// Use this for initialization
	void Awake () 
	{
        vRect = new Rect(5,Screen.height-20,150,20);
		//Set Des Langues
		/*ConfigurationManager.LoadConfig(null);
		ConfigurationManager.LoadConfig("config");
		if(!PlayerPrefs.HasKey("language"))
			PlayerPrefs.SetString("language", ConfigurationManager.GetText("language"));
		// This will reset to default POT Language
		TextManager.LoadLanguage(null);		 
		// This will load filename-en.po
		Debug.Log("Language : "+PlayerPrefs.GetString("language"));
		TextManager.LoadLanguage("strings_"+PlayerPrefs.GetString("language"));*/
		#if UNITY_IPHONE
#if UNITY_EDITOR
		{
			lowTechnologie = true;
			iPad2 = true;
		}
#endif
		if((UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad2Gen)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPadMini1Gen)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad1Gen)
			)
		{
			lowTechnologie = true;
			iPad2 = true;
		}
		if((UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPodTouchUnknown)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPodTouch1Gen)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPodTouch2Gen)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPodTouch3Gen)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPodTouch5Gen)
			)
		{
			lowTechnologie = true;
			smartPhone = true;
		}
		if((UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneUnknown)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone3G)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone3GS)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone4)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone4S)
			||(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone5)
			)
		{
			lowTechnologie = true;
			smartPhone = true;
		}
		
		/*
		 A mettre dans la fonction OpenEAGL_UnityCallback de AppController.mm
        
        switch(UnityGetDeviceGeneration())
	    {
	        case deviceiPad1Gen:
	        case deviceiPad2Gen :
	        case deviceiPadMini1Gen :
	            _context = [[EAGLContext alloc] initWithAPI:kEAGLRenderingAPIOpenGLES1];
	            break;            
	        default:
	            _context = [[EAGLContext alloc] initWithAPI:kEAGLRenderingAPIOpenGLES2];
	    }
	    Ã  la place de 
	   	for (int openglesApi = kEAGLRenderingAPIOpenGLES2 ; openglesApi >= kEAGLRenderingAPIOpenGLES1 && !_context ; --openglesApi)
	    {
	        if (!UnityIsRenderingAPISupported(openglesApi))
	            continue;
	
	        _context = [[EAGLContext alloc] initWithAPI:openglesApi];
	    }
    */
		#endif
		#if UNITY_ANDROID
        //    if (QualitySettings.shadowDistance == 0)
            {
                lowTechnologie = true;
                Debug.Log("LOW TECHNOLOGIE !");
            }
		#endif
		
		//Creation des dossiers si n'existe pas
		PhotoPath = Application.persistentDataPath+"/photos/";
		if(!Directory.Exists(PhotoPath))
		   System.IO.Directory.CreateDirectory(PhotoPath);
		
		PhotoThumbPath = Application.persistentDataPath+"/photos/thumbs/";
		if(!Directory.Exists(PhotoThumbPath))
		   System.IO.Directory.CreateDirectory(PhotoThumbPath);
		
		SavePath = Application.persistentDataPath+"/save/";
		if(!Directory.Exists(SavePath))
		   System.IO.Directory.CreateDirectory(SavePath);
		
		//Logo
		LogoPath = Application.persistentDataPath+"/logo.png";
		
		print("Data path : " + Application.persistentDataPath);

        // API Graphique
        s_openGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
        s_d3d    = SystemInfo.graphicsDeviceVersion.Contains("Direct3D");
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void OnGUI()
	{
		//NÂ° de version
		if(showVersion && (GetComponent<GUIStart>() != null && GetComponent<GUIStart>().enabled))
        {
            vRect.y = Screen.height-20;
			//GUI.Label (vRect,version);
        }
		
	/*	//Debug Gyrocontrolv2
		if(GUI.Button (new Rect(100,100,100,100),"Gyro\n"+GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled))
		{
			GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = !GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled;
		}*/
		
	}

	//UI
	static void FireLogoUpdate(Texture2D tex)
	{
		UpdateLogoRect(tex);
		
		if(logoUpdated != null)
			logoUpdated(tex);
	}
	
	static void UpdateLogoRect(Texture2D tex)
	{
		int logoW = tex.width;
		int logoH= tex.height;
		float ratio = (float)logoW/(float)logoH;
		
		if(logoW > 118 || logoH > 118)
		{
			if(logoW>logoH)
				_logoRect = new Rect(0,0,118,118/ratio);
			else
				_logoRect = new Rect(0,0,118*ratio,118);
		}
		else
		{
			_logoRect = new Rect(0,0,logoW,logoH);
		}
	}
	
	public static void DeleteLogo()
	{
		if(File.Exists(LogoPath))
		{
			File.Delete(LogoPath);	
		}
	}
	
	public void CheckLogo(string path)
	{
		StartCoroutine(CheckLogoCR(path));
	}
	
	public IEnumerator CheckLogoCR(string path)
	{
		Debug.Log("[LOGO]> Check CR started");
		Texture2D tmpLocal = new Texture2D(1,1);
		Texture2D tmpDist = new Texture2D(1,1);
		
		bool localExists = false;
		bool distExists = false;
		
		//----LOAD LOCAL-----------------
		if(File.Exists(LogoPath))
		{
			WWW wwwLocal = new WWW("file://"+LogoPath);
			while(!wwwLocal.isDone)
			{
				yield return new WaitForEndOfFrame();
			}
			
			if (wwwLocal.error != null)
			{
	        	Debug.LogWarning(wwwLocal.error);
				yield return true;
			}
			else
			{
				wwwLocal.LoadImageIntoTexture(tmpLocal);
			}
			wwwLocal.Dispose();
			localExists = true;
		}
		
		//----LOAD DIST-----------------
		WWW wwwDist = new WWW(path);
		while(!wwwDist.isDone)
		{
			yield return new WaitForEndOfFrame();
		}
		if(wwwDist.error != null)
		{
			Debug.Log("[LOGO] dl error >" + wwwDist.error);
		}
		else
		{
			wwwDist.LoadImageIntoTexture(tmpDist);
			distExists = true;
		}
		wwwDist.Dispose();
		
		if(distExists) // si distant tÃ©lÃ©chargÃ©
		{
			if(!localExists) // si pas de fichier sauvegardÃ©, sauvegarde + update logo
			{
				//Sauvegarde
				byte[] imgBytes = tmpDist.EncodeToJPG();
	   			System.IO.File.WriteAllBytes(LogoPath, imgBytes);
				
				FireLogoUpdate(tmpDist);
			}
			else // si deja un fichier, check si diffÃ©rent
			{
				if(tmpDist != tmpLocal) //si diffÃ©rent, sauvegarde + update logo tÃ©lÃ©chargÃ©
				{
					//Sauvegarde
					byte[] imgBytes = tmpDist.EncodeToJPG();
		   			System.IO.File.WriteAllBytes(LogoPath, imgBytes);
					
					FireLogoUpdate(tmpDist);
				}
				else // si meme, utilisation du local
				{
					FireLogoUpdate(tmpLocal);
				}
			}
		}
		else
		{
			if(localExists)
			{
				FireLogoUpdate(tmpLocal);
			}
		}
		
		yield return null;
	}
	
	
	public static void SetHasNewVersion(bool state)
	{
		hasNewVersion = state;
	}
	
	public static bool HasNewVersion()
	{
		return hasNewVersion;
	}
	public static void SetNewVersionLink(string link)
	{
		newVersionLink = link;
	}
	
	public static string GetNewLink()
	{
		return newVersionLink;
	}
	
}
