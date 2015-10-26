using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LibraryLoader : MonoBehaviour 
{

	bool errorLib;

	private int _lblTouchCount = 0;
	private string _lblStateTotation = "";
	
	protected const string LOGIN_ADDRESS = "http://www.pointcube.com/activationOneshot/requests/getUserInfos_v512f1.php";
	protected const string LOGIN_ADDRESS_LITE = "http://www.pointcube.com/activationOneshot/requests/getUserInfos2_v512f1Lite.php";
//  protected const string LOGIN_ADDRESS = "http://www.pointcube.com/activationOneshot/requests/getUserInfos2DEV.php";
//	protected const string LOGIN_KEY = "login";
//	protected const string PASSWORD_KEY = "password";
	protected const string LIBRARIES_KEY = "libraries";

	protected bool _logged = false;
	protected bool _loggedError = false;
	protected bool _badLog = false;
	protected bool _needToLog = false;
	protected bool _showLoginBox = false;
	
	protected GUIStart _guiStart;
	protected GUIMenuLeft _guiMenuLeft;
	
	protected string _login = "001028";
	protected string _password = "yrPBGT5vK79P";
	protected string _requestResult = "";
	protected string _loggedErrorMessage = "";
	
	protected int _libraryToLoad = 0;
	
	protected SortedList<int, OSLib> _libraryList = new SortedList<int, OSLib> ();
	
	EncodeToolSet encoder = new EncodeToolSet();
	
    // -- Progress Bar --
    private Rect r_loadBarFond;                 // Rectangles pour placement de la progress bar
    private Rect r_loadBarGrpMv;
    private Rect r_loadBar;
    private int   mLoadTotalSize;               // Taille totale des libs à télécharger, pour calculer la progression
    private int[] mLoadLibsSize;                // Tailles respectives des libs
    private WWW[] mLoadLibsProgress;            // Requêtes des libs, contenant la progression du dl

    public GUISkin mLoadingSkin;                // Skin loading bar

    private float  mOldTime;                    // -- Calcul du débit de téléchargement --
    private float  mOldLoad;
    private string mDisplayedRate;


	// Use this for initialization
	void Start () 
	{
        // -- Progress bar --
        r_loadBarFond = new Rect(162,UnityEngine.Screen.height*0.865f,UnityEngine.Screen.width-162*2,4); // new Rect(162,664,700,4);
        r_loadBarGrpMv = new Rect(162,UnityEngine.Screen.height*0.865f,0,4);                 // new Rect(162,664,0,4);
        r_loadBar = new Rect(0,0,UnityEngine.Screen.width-162*2f,4);                         // new Rect(0,0,
        mLoadTotalSize      = 0;
        mLoadLibsSize       = null;
        mLoadLibsProgress   = null;

        mDisplayedRate = "";
        mOldTime = -1f;
        mOldLoad = -1f;

		//Caching.CleanCache ();
		_guiStart = GetComponent<GUIStart> ();
		_guiMenuLeft = GetComponent<GUIMenuLeft> ();
		
		if (PlayerPrefs.HasKey (/*LOGIN_KEY*/usefullData.k_logIn) && PlayerPrefs.HasKey (/*PASSWORD_KEY*/usefullData.k_password))
		{
			_login = PlayerPrefs.GetString (/*LOGIN_KEY*/usefullData.k_logIn);
			_password = PlayerPrefs.GetString (/*PASSWORD_KEY*/usefullData.k_password);
			StartCoroutine (LogIn (_login, _password));
		}
		else
		{
			// have to log
			_needToLog = true;
			_showLoginBox = true;
		}
	}
	
	void OnGUI ()
	{
		if (_showLoginBox)
		{
			int posX = Screen.width / 2 + 200;
			int posY = Screen.height / 2 - 10;
			
			if (_badLog)
			{
				GUI.Label (new Rect (posX, posY - 30, 200, 30), "Login ou mot de passe errone");	
			}
			
			if (_loggedError)
			{
				GUI.Label (new Rect (posX, posY - 30, 200, 30), _loggedErrorMessage);	
			}
			
			GUI.Label (new Rect (posX, posY, 100, 20), "login");
			_login = GUI.TextField(new Rect(posX + 110, posY, 100, 20), _login);
			
			GUI.Label (new Rect (posX, posY + 30, 100, 20), "password");
			_password = GUI.TextField (new Rect (posX + 110, posY + 30, 100, 20), _password);
			
			if(GUI.Button(new Rect(posX, posY + 60, 100, 20), "send"))
			{
				if (_login != "" && _password != "")
				{
					_showLoginBox = false;
					StartCoroutine (LogIn (_login, _password));
				}
			}
		} // if(showLoginBox)
		else if(mLoadLibsProgress != null)   // Si téléchargement libs, afficher barre de chargement
        {
            GUI.skin = mLoadingSkin;
            GUI.Box(r_loadBarFond,"","loadingBg");
			
			Color bkupColor = GUI.color;
			GUI.color = new Color(bkupColor.r, bkupColor.g, bkupColor.b, 0.5f);
			GUI.Box(new Rect(0.0f, UnityEngine.Screen.height * 0.85f, Screen.width, Screen.height * 0.09f), "", "blackPX");
			GUI.color = bkupColor;
			
            // -- Calcul progression du téléchargement --
            float progress = 0;
            for(int i=0; i<mLoadLibsSize.Length; i++)
            {
                if(mLoadLibsProgress[i] != null)
                    progress += mLoadLibsProgress[i].progress*(mLoadLibsSize[i]/(float)mLoadTotalSize);
            }

            r_loadBarGrpMv.width = progress * r_loadBarFond.width;


            GUI.BeginGroup(r_loadBarGrpMv);  // Affichage de la barre
            GUI.Label(r_loadBar,"","loadingBar");
            GUI.EndGroup();

            // -- Affichage de la quantité de données téléchargées (Ko ou Mo) --
            float dlSize = progress * mLoadTotalSize, tmp;
            string displaySize, displayTotalSize, displayUnit, format;

            if(mLoadTotalSize >= 524288f)      // à partir de 1/2 Mo, mettre l'unité en Mo
            {
                tmp = dlSize/1048576f;
                displaySize      = tmp.ToString(tmp >= 100f? "F1" : "F2");
                displayTotalSize = (mLoadTotalSize/1048576f).ToString("F2");
                displayUnit      = TextManager.GetText("LibraryLoader.MBUnit");
            }
            else if(mLoadTotalSize >= 512f)    // idem pour Ko
            {
                tmp = dlSize/1024f;
                displaySize      = (tmp).ToString(tmp >= 100f? "F1" : "F2");
                displayTotalSize = (mLoadTotalSize/1024f).ToString("F2");
                displayUnit      = TextManager.GetText("LibraryLoader.KBUnit");
            }
            else                               // sinon en octets
            {
                displaySize      = dlSize.ToString(dlSize >= 100f? "F1" : "F2");
                displayTotalSize = mLoadTotalSize.ToString("F2");
                displayUnit      = TextManager.GetText("LibraryLoader.BUnit");
            }

            // -- Calcul du débit de téléchargement --
            if((Time.time - mOldTime) >= 1f || mOldTime == -1)
            {
                string rateUnit;
                float  rate;
                float  sampleLoad = dlSize - mOldLoad;

                if(sampleLoad >= 524288f)
                {
                    rate     = sampleLoad/1048576f;
                    rateUnit = TextManager.GetText("LibraryLoader.MBpsUnit");
                }
                else if(sampleLoad >= 512f)
                {
                    rate     = sampleLoad/1024f;
                    rateUnit = TextManager.GetText("LibraryLoader.KBpsUnit");
                }
                else
                {
                    rate     = sampleLoad;
                    rateUnit = TextManager.GetText("LibraryLoader.BpsUnit");
                }

                mDisplayedRate = "    ("+rate.ToString(rate >= 100f? "F1" : "F2")+" "+rateUnit+")";

                mOldLoad = dlSize;
                mOldTime = Time.time;
            }

            string label = displaySize + " / " + displayTotalSize +" "+ displayUnit + mDisplayedRate;
            GUI.Label(new Rect(r_loadBarFond.xMax-180,r_loadBarFond.yMax,180,30), label, "loadtxt");

            // Label bleu foncé centré
            GUI.Label(new Rect(UnityEngine.Screen.width/2-100,UnityEngine.Screen.height-80,200,40),
			          TextManager.GetText("LibraryLoader.Downloading"), "waitStart");
        }

		/*GUI.Label(new Rect(20,20,100,100),_lblTouchCount.ToString());
		GUI.Label(new Rect(140,20,100,100),_lblStateTotation);
		for(int i=0;i<_lblTouchCount;i++)
		{
			GUI.Label(new Rect(20,20+(1+i)*100,100,100),_touchPos[i].ToString());
		}*/
		
	} // OnGUI()


//    void FixedUpdate()
//    {
//        if(!_showLoginBox)
//        {
//            // -- Calcul du débit de téléchargement --
//            mLoadTimer += Time.;
//        }
//    } // FixedUpdate()
	
	IEnumerator LogIn (string login, string pwd)
	{

		Debug.Log ("PLAYER_PREFS " + _requestResult);
		WWWForm form = new WWWForm ();	
		form.AddField ("idClient", login);
		form.AddField ("password", pwd);
		form.AddField ("vos", usefullData.version);
		string player = "unity";
		
		int loginInt = int.Parse(login);
		string param = encoder.createSendCode(true,true,loginInt,pwd);
		form.AddField("ac",param);
		
#if UNITY_EDITOR
		player = "ios";
#endif
#if UNITY_STANDALONE_WIN
		player = "win";
#endif
#if UNITY_STANDALONE_OSX
		//player = "macosx";
		player = "win";
#endif
#if UNITY_WEBPLAYER
		player = "web";
#endif
#if UNITY_IPHONE
		player = "ios";
#endif
#if UNITY_ANDROID
		player = "and";
#endif
#if UNITY_FLASH
		player = "flash";
#endif	
		form.AddField ("player", player);
		Debug.Log("PLAYER : "+player);
		
		string url=LOGIN_ADDRESS;
		if(usefullData._edition==usefullData.OSedition.Lite)
		{
			print ("lite");
			url = LOGIN_ADDRESS_LITE;
		}
		WWW logRequest = new WWW (url, form);
		yield return logRequest;
		
		if(logRequest.error != null)
		{
			_loggedError = true;
			_loggedErrorMessage = logRequest.error;
			
			if (!_needToLog) // libraries are in cache
			{
				if (PlayerPrefs.HasKey (LIBRARIES_KEY))
				{
					_requestResult = PlayerPrefs.GetString (LIBRARIES_KEY);
					Debug.Log ("PLAYER_PREFS " + _requestResult);
					StartCoroutine (DownLoadAllLibraries ());
				}
				else
				{
					_showLoginBox = true;
				}
			}
			else
			{
				Debug.Log ("CONNECTION ERROR");
				_showLoginBox = true;
			}
			
			yield return true;	
		}
		else
		{
	
			_loggedError = false;
			_requestResult = logRequest.text;

			Debug.Log ("_requestResult "+_requestResult);
			if (_requestResult.StartsWith ("renew"))
			{
				_logged = true;
				_needToLog = false;
//				PlayerPrefs.SetString (LOGIN_KEY, login);
//				PlayerPrefs.SetString (PASSWORD_KEY, pwd);
				PlayerPrefs.SetString (LIBRARIES_KEY, _requestResult);
				
				StartCoroutine (DownLoadAllLibraries ());
			}
			else
			{
				Debug.Log ("WRONG LOGIN OR PASSWORD");
				_needToLog = true;
				_showLoginBox = true;
				_badLog = true;
			}
			
			yield return true;
		}
	}
	
	IEnumerator DownLoadAllLibraries ()
	{
//		GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
//		GetComponent<PleaseWaitUI>().setAdditionalLoadingTxt("Downloading");
		yield return new WaitForEndOfFrame();
		_requestResult = _requestResult.Replace("<>","@");
		print ("_requestResult : "+_requestResult);
		string [] libsData = _requestResult.Split('@');
		
//		//-------TEST--------------
//		ModulesAuthorization.FireAuthorizeModule("extrabat",bool.Parse("True"));
//		ModulesAuthorization.FireAuthorizeModule("mode2d",bool.Parse("True"));
//		ModulesAuthorization.FireAuthorizeModule("photo_perso",bool.Parse("true"));
//		ModulesAuthorization.FireAuthorizeModule("dossier_client",bool.Parse("TRUE"));
//		ModulesAuthorization.FireAuthorizeModule("enregistrer_envoyer_image",bool.Parse("TRUE"));
//		ModulesAuthorization.FireAuthorizeModule("amelioration",bool.Parse("true"));
//		//------FIN DE TEST--------
		
		foreach (string libData in libsData)
		{
			// ACTIVATION DES MODULES A ACCES RESTREINT (Extrabat, ...)
			if(libData.StartsWith("module"))
			{
				string[] splitStr = libData.Split('=');
				ModulesAuthorization.FireAuthorizeModule(splitStr[1],bool.Parse(splitStr[2]));
			}
			
			if (libData.StartsWith ("lib_"+_libraryToLoad+"_id"))
			{
				++_libraryToLoad;
			}
			
			//Chargement Logo
			if(libData.StartsWith("logo_url")/* && !System.IO.File.Exists(usefullData.LogoPath)*/)
			{
				string logoPath = libData.Split('=')[1];
				GetComponent<usefullData>().CheckLogo(logoPath);
			}
			//Chargement Logo
			if(libData.StartsWith("rev") && !libData.StartsWith("rev_name"))
			{
				string rev = libData.Split('=')[1];
				PlayerPrefs.SetString("Revendeur", rev);
			}
            // Taille totale des libs à télécharger
            if(libData.StartsWith("totalSize"))
            {
               mLoadTotalSize = int.Parse(libData.Split('=')[1]);
            }
		}
		
		if (_libraryToLoad <= 0)
		{
			_guiStart.StartGUI ();
			OSLib lib = new OSLib (-1, -1, "", "", "");		
			_guiMenuLeft.CreateMenu (lib);	
			
			yield return true;
		}
			
		string url          = "";
		string version      = "";
		string version_os3d = "";
        int    libSize      = 0;
		int    libsIndex    = 0;
		
		foreach (string libData in libsData)
		{
			if (libData.StartsWith ("lib_"+libsIndex+"_url"))
			{
				url = libData.Split('=')[1];
			}
			else if (libData.StartsWith("lib_"+libsIndex+"_version"))
			{
				version = libData.Split('=')[1];

				/*if(!numVersionInferieur(version_os3d,usefullData.version))
				{
					int v;
					int.TryParse (version, out v);
	//				Debug.Log("URL>"+url);
					StartCoroutine (DownloadLibrary (url, v, libsIndex));					
					++libsIndex;
				}*/
			}
            else if (libData.StartsWith("lib_"+libsIndex+"_size"))
            {
                libSize = int.Parse(libData.Split('=')[1]); // [V]('=')[V]

            }
            else if (libData.StartsWith("lib_"+libsIndex+"_vos"))
			{

				version_os3d = libData.Split('=')[1];

				//if(!numVersionInferieur(usefullData.version_ID,version_os3d))
				//{
					int v;
					int.TryParse (version, out v);
	//				Debug.Log("URL>"+url);

                if(mLoadLibsSize == null)    // Initialisation des tableaux pour barre de chargement
                {
                    mLoadLibsSize = new int[libsData.Length];
                    mLoadLibsProgress = new WWW[libsData.Length];
                }

                if(!numVersionInferieur(usefullData.version_ID,version_os3d))
                    mLoadLibsSize[libsIndex] = libSize;
                else
                {
					int intValue = 0;
					bool tryParse = int.TryParse(libData.Split('=')[1],out intValue);
					if(tryParse)
					{
						mLoadTotalSize -= intValue;
					}
					mLoadLibsSize[libsIndex]     = 0;
	                mLoadLibsProgress[libsIndex] = null;
                }

					do{

						StartCoroutine (DownloadLibrary (url, v, libsIndex, version_os3d));
					}while(errorLib);

				//}
				//else{
				//	Debug.LogWarning("mise a jour de OS3D necessaire");
				//}
				libsIndex++;
			}
		}
		yield return true;

    } // DownloadAllLibraries()
		
	/**
	 * Vérifie si le numéro de version 1 est strictement inférieur aum
	 * numéro 2.<br/>
	 * Le numéro est inférieur également s'il a moins de valeurs, exemple
	 * 2.5.3 < 2.5.3.2
	 * @param num1 numéro de version 1 type 2.5.3
	 * @param num2 numéro de version 2 type 2.5.3
	 * @return true si le numéro de version 1 est strictement inférieur au
	 * numéro 2, false s'il est supérieur ou egal.
	 */
	public static bool numVersionInferieur(string num1, string num2) {

		//print ("num1: "+num1);
		//print ("num2: "+num2);
		/*
		for (int i = 0; i < toVersionNumber(num1).Length; i++) {
			print ("toVersionNumber(num1) num: "+i+" ::: "+toVersionNumber(num1)[i]);
			print ("toVersionNumber(num2) num: "+i+" ::: "+toVersionNumber(num2)[i]);
		}
*/
		return numVersionInferieur(toVersionNumber(num1),
								   toVersionNumber(num2));
	}
	
	/**
	 * Vérifie si le numéro de version 1 est strictement inférieur au
	 * numéro 2.<br/>
	 * Le numéro est inférieur également s'il a moins de valeurs, exemple
	 * 2.5.3 < 2.5.3.2
	 * @param num1 numéro de version 1 type 2.5.3
	 * @param num2 numéro de version 2 type 2.5.3
	 * @return true si le numéro de version 1 est strictement inférieur au
	 * numéro 2, false s'il est supérieur ou egal.
	 */
	public static bool numVersionInferieur(int[] num1, int[] num2) {

		int size = Mathf.Min(num2.Length, num1.Length);

		for (int i = 0; i < size; i++) 
		{
			//print ("num1[i] : " + num1[i]);
			//print ("num2[i] : " + num2[i]);

			// version serveur plus récente
			if(num2[i] > num1[i]) 
			{
				return true;
			}
			else if(num2[i] < num1[i]) 
			{
				return false;
			}
			// else égal, teste les nombres suivants
		}
		// versions identiques, vérifie si la version est plus 
		// détaillée ex 2.5.3.28 > 2.5.3

		if(num2.Length > num1.Length)
			return true;
		
		return false;
	}
	
	/**
	 * Convertit la chaine du type "x.x.x" (ex 2.5.3) en un tableau d'entiers 
	 * @param version numéro en texte, ex 2.5.3
	 * @return un tableau d'entiers correspondants à la version
	 * @throws NumberFormatException Si une valeur de la version n'est pas un
	 * entier
	 */
	public static int[] toVersionNumber(string version) 
	{
		char[] splitchar = {'.'};
		string[] values = version.Split(splitchar);
		int[] result = new int[values.Length];
		for (int i = 0; i < values.Length; i++) {
			// vérification de la version donnée
			result[i] = int.Parse(values[i]);
		}

		return result;
	}
	
	IEnumerator DownloadLibrary (string url, int version, int libIndex, string version_os3d)
	{
		errorLib = false;
		//print ("usefullData.version_ID : "+usefullData.version_ID);

		if(!numVersionInferieur(usefullData.version_ID,version_os3d))
		{
			print ("version_os3d : "+version_os3d);
			//print (version_os3d);
			WWW www = WWW.LoadFromCacheOrDownload (url, version);
            mLoadLibsProgress[libIndex] = www;
			yield return www;
			Debug.Log ("url : " + url);
			//print ("_libraryToLoad : "+_libraryToLoad);
			if (www.error != null)
			{

				Debug.Log ("LIB ERROR" + www.error);
				//errorLib = true;
				//_libraryToLoad ++;
			}
			else
			{
				//Debug.Log ("LIB SUCCESS " + url);
				//Debug.Log ("LIB SIZE " + www.size);
				AssetBundle assetBundle = www.assetBundle;
				
				if (assetBundle != null)
				{
					TextAsset xmlLibrary = assetBundle.LoadAsset ("settings", typeof (TextAsset)) as TextAsset;
					
					if (xmlLibrary != null)
					{
						XMLParser parser = new XMLParser ();
						XMLNode libraryRootNode = parser.Parse (xmlLibrary.text).GetNode ("settings>0");
						
						OSLibBuilder libBuilder = new OSLibBuilder (libraryRootNode, url, version);
						OSLib library = libBuilder.GetLibModel ();
						
						LoadObjectsThumbnails (library, assetBundle);
						LoadModulesThumbnails (library, assetBundle);
						LoadBackgrounds (library, assetBundle);
						
						//GameObject.Find("back_grid").GetComponent<BackGridManager>().listBgs.Add(text2D);
						
						_libraryList.Add (libIndex, library);
					}
					else
					{
						Debug.Log ("Pas de XML");	
					}
				
					assetBundle.Unload (false);
				}
			}
		}
		
		if (--_libraryToLoad == 0)
		{
			_guiStart.StartGUI ();
           // Debug.Log("StartGUI OK");
            OSLib lib = OSLibBuilderUtils.MergeLibraries(_libraryList.Values);
           // Debug.Log("MergeLibraries OK");
            _guiMenuLeft.CreateMenu(lib);
           // Debug.Log("CreateMenu OK");
            GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
          //  Debug.Log("SetDisplayIcon OK");
            GetComponent<PleaseWaitUI>().setAdditionalLoadingTxt("");
         //   Debug.Log("setAdditionalLoadingTxt OK");
            yield return new WaitForEndOfFrame();
          //  Debug.Log("WaitForEndOfFrame OK");
			Destroy (this);
		}

	} // DownloadLibrary()
	
	protected void LoadBackgrounds (OSLib library, AssetBundle assets)
	{
		if(library.GetBgList().Count > 0)
		{
			if(PlayerPrefs.HasKey("BGStandard"))
			{
				GameObject.Find("back_grid").GetComponent<BackGridManager>().buseStandardBgs = true;
				GameObject.Find("back_grid").GetComponent<BackGridManager>().listBgs.Clear();
				
				return;
			}
			else
			{
				PlayerPrefs.SetInt("BGStandard", 0);
				GameObject.Find("back_grid").GetComponent<BackGridManager>().buseStandardBgs = false;
			}
			
			if(PlayerPrefs.HasKey("BG"))
			{
				PlayerPrefs.DeleteKey("BG");
			}
		}
		
		foreach (OSLibBackground bg in library.GetBgList())
		{
			/*if (catLvl1.GetCategoryList ().Count > 0)
			{
				foreach(OSLibCategory catLvl2 in catLvl1.GetCategoryList ())
				{
					foreach(OSLibObject obj in catLvl2.GetObjectList ())
					{					
						Texture2D text2D = assets.Load (obj.GetThumbnailPath (), typeof (Texture2D)) as Texture2D;
						obj.SetThumbnail (text2D);
					}
				}
			}*/
			
			Texture2D text2D = assets.LoadAsset (bg.GetBackgroundName(), typeof (Texture2D)) as Texture2D;
			
			//GameObject.Find("back_grid").guiTexture.texture = text2D;
			
			if(text2D != null)
			{
				GameObject.Find("back_grid").GetComponent<BackGridManager>().listBgs.Add(text2D);
			}
			
			//Camera.main.
			//obj.SetThumbnail (text2D);
		}
	}
	
	protected void LoadObjectsThumbnails (OSLib library, AssetBundle assets)
	{
		foreach (OSLibCategory catLvl1 in library.GetCategoryList ())
		{
			if (catLvl1.GetCategoryList ().Count > 0)
			{
				foreach(OSLibCategory catLvl2 in catLvl1.GetCategoryList ())
				{
					foreach(OSLibObject obj in catLvl2.GetObjectList ())
					{					
						Texture2D text2D = assets.LoadAsset (obj.GetThumbnailPath (), typeof (Texture2D)) as Texture2D;
						obj.SetThumbnail (text2D);
					}
				}
			}
			
			if (catLvl1.GetObjectList ().Count > 0)
			{
				foreach(OSLibObject obj in catLvl1.GetObjectList ())
				{
					Texture2D text2D = assets.LoadAsset (obj.GetThumbnailPath (), typeof (Texture2D)) as Texture2D;
					obj.SetThumbnail (text2D);
				}
			}
		}
	}
	
	protected void LoadModulesThumbnails (OSLib library, AssetBundle assets)
	{
		foreach (OSLibModules modules in library.GetModulesList ())
		{
			foreach (OSLibModule module in modules.GetStandardModuleList ())
			{
				foreach (OSLibTexture texture in module.GetTextureList ())
				{
					Texture2D text2D = assets.LoadAsset (texture.GetThumbnailPath (), typeof (Texture2D)) as Texture2D;
					texture.SetThumbnail (text2D);
				}
				
				foreach (OSLibColor color in module.GetColorList ())
				{
					Texture2D text2D = null;
					string temp = color.GetThumbnailPath ();
					if(temp != "")
						text2D = assets.LoadAsset (color.GetThumbnailPath (), typeof (Texture2D)) as Texture2D;
					//if(""!="")  
					if(text2D==null)
					{
						text2D = new Texture2D(86,86);
						text2D.name = color.GetDefaultText();
						
						for (int i=0;i<86;i++)
						{
							for (int j=0;j<86;j++)
							{
								text2D.SetPixel(i,j, color.GetColor());
							}
						}
						text2D.Apply();
					}
					
					color.SetThumbnail (text2D);
				}
			}
		}
		
		foreach (OSLibStairs stairs in library.GetStairsList ())
		{ 
			foreach (OSLibStair stair in stairs.GetStairList ())
			{
				Texture2D text2D = assets.LoadAsset (stair.GetThumbnailPath (), typeof (Texture2D)) as Texture2D;
				stair.SetThumbnail (text2D);
			}
		}
	}
}