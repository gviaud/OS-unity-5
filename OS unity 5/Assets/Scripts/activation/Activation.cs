using UnityEngine;
using System.Collections;
using Pointcube.Global;
using Pointcube.Utils;

public class Activation : MonoBehaviour
{
	EncodeToolSet encoder = new EncodeToolSet();
	
	string m_login = "";//"1028";
	string m_password = "";//"yrPBGT5vK79P";
	string m_msg = "";
	
	private const string c_tryLogin = "001426";
	private const string c_tryMdp = "2DevdUeeHxK9";
	
	bool m_showLoginBox = true;
	bool m_working = false;
	bool m_activationOk = false;
	bool m_showGUI = false;
	bool m_showRetry = false;
	bool m_showSplash = true;
	
	float angle = 0;
	float loadedProgress = 0;
	
	AsyncOperation async;
		
	EncodeToolSet m_encoder = new EncodeToolSet();
	DecodeToolSet m_decoder = new DecodeToolSet();
	ActivationData m_activationData = new ActivationData();
	
	//Texture2D test;
	
	//public bool debug;
	public float debugValue;
	
	//loading bar
	public Rect r_loadBarFond;
	public Rect r_loadBarGrpMv;
	public Rect r_loadBar;

	//rotation loading
	Vector2 rl_center;
	public Texture2D rl_pic;
	Rect rl_rect;
	float rl_angle = 0;
	
	public GUISkin skin;

    // -- Références scène --
    public GameObject m_background;

    // -- Splash Screen --
    public  Texture2D m_splashScreenBg;
    //public  string    m_logoFullPath      = "gui/backgrounds/fixed/OS3D_logo_512_glow";
   	//public  string    m_logoExpressPath   = "gui/backgrounds/fixed/OS3D_EXPRESS_LOGO";
    //private Texture2D m_logoTex;
    //private Rect      m_logoRect;
    private Rect      m_splashRect;

	// choix de la langue
	private bool m_showLanguage = false;
	//public Texture2D _imageFrLanguageChoice;
	//public Texture2D _imageEnLanguageChoice;

    private static readonly string DEBUGTAG = "Activation : ";
	
	public GUIStyle activationStyleFr;
	public GUIStyle activationStyleEn;
	public GUIStyle activationStyleDe;
	
	public Color _languageColor;
	public Color _loadingColor;
	private int  _errorId;

    //-----------------------------------------------------
	void Awake()
	{
	/*	ConfigurationManager.LoadConfig(null);
		ConfigurationManager.LoadConfig("config");
		if(!PlayerPrefs.HasKey("language"))
			PlayerPrefs.SetString("language", ConfigurationManager.GetText("language"));
		// This will reset to default POT Language
		TextManager.LoadLanguage(null);		 
		// This will load filename-en.po
		Debug.Log("Language : "+PlayerPrefs.GetString("language"));
		TextManager.LoadLanguage("strings_"+PlayerPrefs.GetString("language"));
	*/
#if UNITY_STANDALONE_WIN
        if (System.Environment.GetCommandLineArgs().Length > 1)
        {

            if (InterProcess.FileToOpen())
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
#endif
        UsefullEvents.OnResizingWindow += SetRects;
        UsefullEvents.OnResizeWindowEnd += SetRects;
	}
	
    //-----------------------------------------------------
	void Start ()
	{
        // -- Progress bar --
        r_loadBarFond = new Rect(162,Screen.height*0.865f,Screen.width-162*2,4); // new Rect(162,664,700,4);
        r_loadBarGrpMv = new Rect(162,Screen.height*0.865f,0,4);                 // new Rect(162,664,0,4);
        r_loadBar = new Rect(0,0,Screen.width-162*2f,4);                         // new Rect(0,0,700,4);
		_errorId = 1337;
        if(m_background == null)
            Debug.LogError(DEBUGTAG+" Background not set, please set it in the inspector.");

        // -- Chargement logo --
        /*string logoPath = (usefullData._edition == usefullData.OSedition.Lite? m_logoExpressPath: m_logoFullPath);
        m_logoTex = (Texture2D) Resources.Load(logoPath, typeof(Texture2D));
        if(m_logoTex == null)
            Debug.LogError(DEBUGTAG+logoPath+" "+PC.MISSING_RES);

        m_logoRect   = new Rect();*/
        m_splashRect = new Rect();
        if(m_splashScreenBg == null)
            Debug.LogError(DEBUGTAG+" Splash screen not set, please set it in the inspector.");
        SetRects();

		/*ConfigurationManager.LoadConfig(null);
		ConfigurationManager.LoadConfig("config");
		if(!PlayerPrefs.HasKey("language"))
			PlayerPrefs.SetString("language", ConfigurationManager.GetText("language"));
		// This will reset to default POT Language
		TextManager.LoadLanguage(null);		 
		// This will load filename-en.po
		Debug.Log("Language : "+PlayerPrefs.GetString("language"));*/
		
#if UNITY_STANDALONE_WIN		
//		Screen.SetResolution(Screen.currentResolution.width,
//		Screen.currentResolution.height,false);//Force fullscreen pleine resolution
#endif
		
#if UNITY_ANDROID
		GameObject.Find("Background").guiTexture.pixelInset = new Rect(0,0,Screen.width,Screen.height);       
#endif
		
#if UNITY_IPHONE
		
#endif
		m_showLanguage = true;
		StartCoroutine(ActivationChecks());
		
		//reset toLoad Player Prefs

		PlayerPrefs.DeleteKey(usefullData.k_toLoadPath);
		PlayerPrefs.DeleteKey(usefullData.k_toLoadParams);
		PlayerPrefs.DeleteKey(usefullData.k_startBypass);
		PlayerPrefs.DeleteKey(usefullData.k_selectedClient);
		PlayerPrefs.DeleteKey(usefullData.k_selectedProject);
		
		//rotationloading
//		rl_center = new Vector2(Screen.width/2,Screen.height/2);
//		rl_rect = new Rect(Screen.width/2-rl_pic.width/2,Screen.height/2-rl_pic.height/2,rl_pic.width,rl_pic.height);
		
		
		DbgUtils.EnableGUIdebug();
	}
	
	IEnumerator ActivationChecks()
	{
		bool activationTmp = true;
		
		yield return new WaitForSeconds(4);
		//m_showSplash = false;
		
		while(m_showLanguage)
		{
			yield return new WaitForEndOfFrame();
		}
		m_showSplash = false;
        m_background.GetComponent<GUITexture>().enabled = true;
		
		//Check du LLD (last launched date)
		if(PlayerPrefs.HasKey(usefullData.k_lld))
		{
			activationTmp = activationTmp && checkAndSaveLLD();	
		}
		else
		{
			activationTmp = false; // pas de données de LLD
		}
		
		//Check nb jours avant fin
		if(PlayerPrefs.HasKey(usefullData.k_data))
		{
			m_activationData.deSerializeInfos(m_decoder,PlayerPrefs.GetString(usefullData.k_data));
//			m_activationData.getEndDate() > format : j/m/a
			System.DateTime saved = new System.DateTime(m_activationData.getEndDate()[2],
				m_activationData.getEndDate()[1],
				m_activationData.getEndDate()[0]);
			
			PlayerPrefs.SetString("LicenceDateExpiration", m_activationData.getEndDate()[0].ToString() + "/" +
			                      m_activationData.getEndDate()[1].ToString() + "/" +
			                      m_activationData.getEndDate()[2].ToString());
			
			PlayerPrefs.SetInt("LicenceDateRestDays", (saved.DayOfYear - System.DateTime.Now.DayOfYear));
			
			Debug.Log("Now " + System.DateTime.Now.ToString());
			Debug.Log("ExpireTime " + saved.ToString());
			Debug.Log("playerPref " + PlayerPrefs.GetString(usefullData.k_lld));
			print(PlayerPrefs.GetString("LicenceDateExpiration"));
			
			if(saved.Day == 1 && saved.Month == 1 && saved.Year == 1970)
			{
				Debug.Log("Unlimited licence");
				activationTmp = activationTmp && true;

			}
			else
			{
				if(System.DateTime.Compare(System.DateTime.Now,saved)<=0)
				{
					int nbJ=0;
					if(System.DateTime.Now.Month == saved.Month && System.DateTime.Now.Year == saved.Year)
					{
						nbJ = saved.Day - System.DateTime.Now.Day;
						Debug.Log(nbJ + "jours avant Licence expirée");
					}
					activationTmp = activationTmp && true;
				}
				else
				{
					Debug.Log("Licence expirée");
					activationTmp = false;	
				}
			}
		}
		
		// du login, du mdp 
		if(!PlayerPrefs.HasKey(usefullData.k_logIn) || !PlayerPrefs.HasKey(usefullData.k_password))
		{
			activationTmp = false;	
		}
		
		//InternetCheck + online licence check si conection
		if(activationTmp)
		{
			string l = PlayerPrefs.GetString(usefullData.k_logIn);
			string p = PlayerPrefs.GetString(usefullData.k_password);
			
			string param = encoder.createSendCode(true,true,int.Parse(l),p);
			WWWForm form = new WWWForm();
			form.AddField("ac",param);
			string url=usefullData.ActivationUrl;
			if(usefullData._edition==usefullData.OSedition.Lite)
			{
				url = usefullData.ActivationUrlLite;
			}
			WWW sender = new WWW(url,form);
			yield return sender;
			
			if(sender.error != null) // pas de connection
			{
				Debug.Log("CONNECTION Error "+sender.error);
				m_msg = "ERROR :\n"+sender.error;
			}
			else
			{
				if(sender.text.Length<20)
				{
					Debug.Log("licence expirée " + sender.text);
					activationTmp = false;
				}	
			}
		}
		
		//ACTIVATION
		if(activationTmp /*&& PlayerPrefs.GetString(usefullData.k_logIn) != c_tryLogin*/)
		{
			StartCoroutine(LoadLevel(true));
			m_showGUI = false;
			m_activationOk = true;
			m_showLoginBox = false;
			m_working = false;
		}
		else
		{
			if(PlayerPrefs.HasKey(usefullData.k_logIn) || PlayerPrefs.HasKey(usefullData.k_password))
			{
				m_login = PlayerPrefs.GetString(usefullData.k_logIn);
				m_password = PlayerPrefs.GetString(usefullData.k_password);
			}
			m_showGUI = true;	
		}
	}
	
    //-----------------------------------------------------
	void Update ()
	{
	}

	private void GUILanguage()
	{
		gameObject.GetComponent<Camera>().backgroundColor = _languageColor;
		if(!PlayerPrefs.HasKey("language"))
		{
			float delta = (Screen.width-3*activationStyleEn.fixedWidth)/4;
			if(GUI.Button(new Rect(
				//(Screen.width-2*activationStyleEn.fixedWidth)/3,
				delta,
				(Screen.height-activationStyleEn.fixedHeight)/2,
				activationStyleEn.fixedWidth,
				activationStyleEn.fixedHeight),"",activationStyleEn))
			{
				PlayerPrefs.SetString("language", "en");
				TextManager.LoadLanguage(null);		 
				// This will load filename-en.po
				Debug.Log("Language : "+PlayerPrefs.GetString("language"));
				TextManager.LoadLanguage("strings_"+PlayerPrefs.GetString("language"));
				m_showLanguage=false;
			}			
			
			if(GUI.Button(new Rect(
				//2*(Screen.width-2*activationStyleFr.fixedWidth)/3 + activationStyleFr.fixedWidth,
				2*delta+activationStyleEn.fixedWidth,
				(Screen.height-activationStyleFr.fixedHeight)/2,
				activationStyleFr.fixedWidth,
				activationStyleFr.fixedHeight),"",activationStyleFr))
			{
				PlayerPrefs.SetString("language", "fr");
				TextManager.LoadLanguage(null);		 
				// This will load filename-en.po
				Debug.Log("Language : "+PlayerPrefs.GetString("language"));
				TextManager.LoadLanguage("strings_"+PlayerPrefs.GetString("language"));		
				m_showLanguage=false;
			}
			if(GUI.Button(new Rect(
				//3*(Screen.width-2*activationStyleDe.fixedWidth)/3 + 2* activationStyleDe.fixedWidth,
				3*delta+2*activationStyleEn.fixedWidth,
				(Screen.height-activationStyleDe.fixedHeight)/2,
				activationStyleDe.fixedWidth,
				activationStyleDe.fixedHeight),"",activationStyleDe))
			{
				PlayerPrefs.SetString("language", "de");
				TextManager.LoadLanguage(null);		 
				// This will load filename-en.po
				Debug.Log("Language : "+PlayerPrefs.GetString("language"));
				TextManager.LoadLanguage("strings_"+PlayerPrefs.GetString("language"));		
				m_showLanguage=false;
			}
		}
		else
		{
			TextManager.LoadLanguage(null);		 
			Debug.Log("Language : "+PlayerPrefs.GetString("language"));
			TextManager.LoadLanguage("strings_"+PlayerPrefs.GetString("language"));		
			m_showLanguage=false;
		}
	}

    //-----------------------------------------------------
	void OnGUI()
	{
		GUI.skin = skin;
		if(m_showSplash)
		{
			if(m_showLanguage)
				GUILanguage();
			else
			{
				gameObject.GetComponent<Camera>().backgroundColor = _loadingColor;
				GUI.DrawTexture(m_splashRect, m_splashScreenBg);
                //GUI.DrawTexture(m_logoRect, m_logoTex);
			}
		}
		//Login
		if(m_showGUI && async == null)
		{
			gameObject.GetComponent<Camera>().backgroundColor = _loadingColor;
			
#if UNITY_IPHONE
			GUI.Box(new Rect(Screen.width/2-595/2,Screen.height/2-700/2,595,280),"","bg");
			GUI.BeginGroup(new Rect(Screen.width/2-595/2+35,Screen.height/2-700/2+40,525,200));
#else
			GUI.Box(new Rect(Screen.width/2-595/2,Screen.height/2-230/2,595,280),"","bg");
			GUI.BeginGroup(new Rect(Screen.width/2-595/2+35,Screen.height/2-230/2+40,525,200));
#endif

			if(m_showLoginBox)
			{
				GUI.Label(new Rect(0,0,525,50),TextManager.GetText("Activation.Authentification"),"txt");
				
				GUI.Label(new Rect(0,50,150,50),TextManager.GetText("Activation.Identifiant"),"txt");
				m_login = GUI.TextField(new Rect(150,50,375,50),m_login,"input");
				
				GUI.Label(new Rect(0,100,150,50),TextManager.GetText("Activation.password"),"txt");
				m_password = GUI.TextField(new Rect(150,100,375,50),m_password,"input"); 
				
				if(m_login != "" && m_password != "")
				{
					if(GUI.Button(new Rect(0,150,525,50),TextManager.GetText("Activation.connect"),"btn"))
					{
						m_showLoginBox = false;
						StartCoroutine(Post());	
					}
				}
				else
				{
					Color bkupColor = GUI.color;
					GUI.color = Color.grey;
					GUI.Box(new Rect(0,150,525,50),TextManager.GetText("Activation.connect"),"btn");
					GUI.color = bkupColor;
				}
			}
			else if(m_working)
			{
				GUI.Label(new Rect(0,50,525,50),TextManager.GetText("Activation.wait"),"txt");
			}
			else if(m_msg != "")
			{
				GUI.Label(new Rect(0,50,525,50),m_msg,"txt");
			}
			
			if(m_showRetry)
			{
				if(GUI.Button(new Rect(0,150,262,50),TextManager.GetText("Activation.back"),"btn"))
				{
					m_showLoginBox = true;
					m_showRetry = false;
					if ( _errorId == -2){
						PlayerPrefs.DeleteAll();
						Application.LoadLevel("Activation");
					}
				}
				if(GUI.Button(new Rect(263,150,262,50),"Signaler un probleme","btn"))
				{
#if UNITY_IPHONE
					EtceteraManager.setPrompt(true);
					EtceteraBinding.showMailComposer("info@pointcube.fr",
					                                 "Licence : " + m_login,
					                                 "Veuillez remplir les champs suivant :\nNom :\nNumero de Telephone :\nNature du probleme :", false);
					
					
#else
					Application.OpenURL("http://www.pointcube.fr/contact/7-articles-pointcube");
#endif	
				}
			}
			GUI.EndGroup();
		}
		
		//Chargement
		if(async != null)
		{
			gameObject.GetComponent<Camera>().backgroundColor = _loadingColor;
			GUI.Box(r_loadBarFond,"","loadingBg");
			r_loadBarGrpMv.width = async.progress * r_loadBarFond.width;
			
			GUI.BeginGroup(r_loadBarGrpMv);
			GUI.Label(r_loadBar,"","loadingBar");
			GUI.EndGroup();
			GUI.Label(new Rect(r_loadBarFond.xMax-150,r_loadBarFond.yMax,150,30),TextManager.GetText("Activation.loading"),"loadtxt");
		}

		//debug
		/*if(debug)
		{
			//rotating loading
			Matrix4x4 bkup = GUI.matrix;
			GUIUtility.RotateAroundPivot(rl_angle,rl_center);
			rl_angle = rl_angle + 2;
			if(rl_angle > 360)
				rl_angle = 0;
			GUI.DrawTexture(rl_rect,rl_pic);
			
			GUI.matrix = bkup;
			
			GUI.Label(new Rect(rl_center.x-25,rl_center.y-25,50,50),"LOADING");
			
			//Custom Loading
			bkup = GUI.matrix;
			
			GUIUtility.ScaleAroundPivot(new Vector2(rl_angle,rl_angle),rl_center);
			rl_angle = rl_angle + 0.02f;
			if(rl_angle > 1)
				rl_angle = 0;
			GUI.DrawTexture(rl_rect,rl_pic);
			
			GUI.matrix = bkup;
			
			GUI.Label(new Rect(rl_center.x-25,rl_center.y-25,50,50),"LOADING");
		}*/
	}

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= SetRects;
        UsefullEvents.OnResizeWindowEnd -= SetRects;
    }

    //-----------------------------------------------------
    private void SetRects()
    {
        /*m_logoRect = new Rect((Screen.width-m_logoTex.width)/2f, (Screen.height-m_logoTex.height)/2f,
                                                                                    m_logoTex.width, m_logoTex.height);*/
        m_splashRect.Set(0.0f/*(Screen.width-m_splashScreenBg.width)/2*/, 0.0f/*(Screen.height-m_splashScreenBg.height)/2*/,
		                 Screen.width/*m_splashScreenBg.width*/, Screen.height/*m_splashScreenBg.height*/);
        Texture bgImg = m_background.GetComponent<GUITexture>().texture;
        Rect pixin = m_background.GetComponent<GUITexture>().pixelInset;
        pixin.Set(0, 0, Screen.width, Screen.height);
        m_background.GetComponent<GUITexture>().pixelInset = pixin;
    }

	// Privates
	IEnumerator LoadLevel(bool oneShot)
	{
		PlayerPrefs.Save();
		if(usefullData.sc_oneShot == "" || usefullData.sc_clientExplorer == ""/* || debug*/)
			Debug.Log("NO SCENE SET");
		else
		{
//			if(oneShot)
	        	async = Application.LoadLevelAsync(usefullData.sc_oneShot);
//			else
//				async = Application.LoadLevelAsync(usefullData.sc_clientExplorer);
	        yield return async;
	        Debug.Log("Loading complete");
		}
    }
	
	IEnumerator Post()
	{
		m_working = true;
		yield return new WaitForEndOfFrame();
		DbgUtils.EnableGUIdebug();
		DbgUtils.Log("MACOSXPB","m_login " + m_login);
		int loginInt;
		bool bparse = int.TryParse(m_login, out loginInt);
		
		if(!bparse)
		{
			loginInt = 100000;
		}
		
		DbgUtils.Log("MACOSXPB","loginInt " + loginInt);
		DbgUtils.Log("MACOSXPB","m_password " + m_password);
		string param = encoder.createSendCode(true,true,loginInt,m_password);
		WWWForm form = new WWWForm();
		form.AddField("ac",param);
		DbgUtils.Log("MACOSXPB","ac " + param);
		
		string url=usefullData.ActivationUrl;
		if(usefullData._edition==usefullData.OSedition.Lite)
		{
			url = usefullData.ActivationUrlLite;
		}
		DbgUtils.Log("MACOSXPB","ready");
		WWW sender = new WWW(url,form);
		DbgUtils.Log("MACOSXPB","en cours");
		yield return sender;
		DbgUtils.Log("MACOSXPB","sender");
		
		m_working = false;
		yield return new WaitForEndOfFrame();
		if(sender.error != null)
		{
			Debug.Log("Error "+sender.error);
			m_msg = "ERROR :\n"+sender.error;
			
			if(m_login == c_tryLogin)
			{
				m_login = "";
				m_password = "";
			}
			
			m_showRetry = true;
		}
		else
		{
			Debug.Log("CODE > "+sender.text);
			m_msg = "Authentification réussie!";
			continueActivation(sender.text);
			
		}
		
		yield return null;
	}
	
	public void continueActivation(string returnedCode)
	{
		if(returnedCode != null)
		{
			if(returnedCode.Length<20)
			{
				switch(returnedCode)       
			      {         
			     	case "-1":
			            m_msg = "Erreur generale non prevue";
			            break; 
					case "0": 
			            m_msg ="Provenance inconnue";
			            break; 
			     	case "-2":
						m_msg ="Erreur identifiant ou mot de passe";
						_errorId = -2;
			            break;
					case "-3":
			            m_msg ="Aucune cle disponnible";
			            break;
					case "-4":   
			            m_msg ="Date machine incorrecte";
			            break;
					case "-6":
			            m_msg ="Erreur inattendue pendant le script";
			            break;
					case "1":
			            m_msg ="Desactivation reussie";
			            break;
			       }
				m_activationOk = false;
				m_showRetry = true;
				//Camera.mainCamera.GetComponent<GUIDisplay>().displayMessage("ERREUR","ERREUR CODE : "+ returnedCode+"\n"+errorMessage);
			}
			else
			{	
				sbyte[] data = m_decoder.decodeReturnedCode(returnedCode); // décode
				m_activationData.setInfo(data,/*login,mdp*/int.Parse(m_login),m_password); 	// enregistre temporairement les infos
				
				string serializedData = m_activationData.serializeInfos(encoder);// enregistre les infos dans les playerPrefs
				
				//Save login,mdp,data in playerPrefs
				PlayerPrefs.SetString(usefullData.k_logIn,m_login);
				PlayerPrefs.SetString(usefullData.k_password,m_password);
				PlayerPrefs.SetString(usefullData.k_data,serializedData);
				
				//saveFirstLLd in playerPrefs
				int tempDay = System.DateTime.Now.Day;
				int tempMonth = System.DateTime.Now.Month;
				int tempYear = System.DateTime.Now.Year;
				
				byte[] tempDate = new byte[4];
				tempDate[0] = (byte)tempDay;
				tempDate[1] = (byte)tempMonth;
				tempDate[2] = (byte)(tempYear >> 8);
				tempDate[3] = (byte)(tempYear);
				PlayerPrefs.SetString(usefullData.k_lld , encoder.encodeBytesUpperCase(tempDate));
				
				m_activationOk = true;
				//Debug.Log("Passe La");
				System.DateTime saved = new System.DateTime(m_activationData.getEndDate()[2],
				                                            m_activationData.getEndDate()[1],
				                                            m_activationData.getEndDate()[0]);
				
				PlayerPrefs.SetString("LicenceDateExpiration", m_activationData.getEndDate()[0].ToString() + "/" +
				                      m_activationData.getEndDate()[1].ToString() + "/" +
				                      m_activationData.getEndDate()[2].ToString());
				
				PlayerPrefs.SetInt("LicenceDateRestDays", (saved.DayOfYear - System.DateTime.Now.DayOfYear));
				
				Debug.Log("Now " + System.DateTime.Now.ToString());
				Debug.Log("ExpireTime " + saved.ToString());

				StartCoroutine(LoadLevel(true));
			}
			//return activationValidation;
		}
		else
		{
			m_msg = "Echec de l'authentification";
			//return false;
		}
	}
	
	private bool checkAndSaveLLD()
	{
		//current time
		bool activation = true;

		int tempDay = System.DateTime.Now.Day;
		int tempMonth = System.DateTime.Now.Month;
		int tempYear = System.DateTime.Now.Year;
		
		byte[] tempDate = new byte[4];
		tempDate[0] = (byte)tempDay;
		tempDate[1] = (byte)tempMonth;
		tempDate[2] = (byte)(tempYear >> 8);
		tempDate[3] = (byte)(tempYear);
		
//		Debug.Log("TEMP DATE(current date) = "+tempDay+"/"+tempMonth+"/"+tempYear);
		
		//saved time
		if(PlayerPrefs.HasKey(usefullData.k_lld))
		{
			string strSavedLLD = PlayerPrefs.GetString(usefullData.k_lld);//(string)RegisterRW.ReadValue(RegisterRW.lld);
			byte[] savedLLD = null;
			
			if (strSavedLLD != null && strSavedLLD != "")
			{
				savedLLD = new byte[4];
				savedLLD = m_decoder.decodeBytesUpperCase(strSavedLLD);
			}
			
			if(savedLLD!=null)
			{
				int savedDay = (int)savedLLD[0];
				int savedMonth = (int)savedLLD[1];
				int savedYear = (int)savedLLD[2];
				savedYear = (savedYear<<8 | (int)savedLLD[3])+4;
	//			Debug.Log("TEMP DATE(Saved date) = "+savedDay+"/"+savedMonth+"/"+savedYear);
				//TESTBUG
				if(tempYear>= savedYear && tempMonth>= savedMonth && tempDay >= savedDay)
				{
					PlayerPrefs.SetString(usefullData.k_lld , encoder.encodeBytesUpperCase(tempDate));
//					RegisterRW.WriteValue(RegisterRW.lld,encoder.encodeBytesUpperCase(tempDate));
				}
				else
				{
					Debug.Log("Cheater ! Date Changée");
					activation = false;
				}
			}
		}
		else
		{
			PlayerPrefs.SetString(usefullData.k_lld , encoder.encodeBytesUpperCase(tempDate));
			//RegisterRW.WriteValue(RegisterRW.lld,encoder.encodeBytesUpperCase(tempDate));
		}
		
		return activation;
	}
	// Public's
}
