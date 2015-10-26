using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pointcube.Global;
using Pointcube.Utils;
using Pointcube.InputEvents;

#if UNITY_EDITOR
using UnityEditor;

#endif

//#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//using System.Windows.Forms;
//#endif

public class GUIStart : MonoBehaviour,GUIInterface
{
    // -- Références aux objets de la scène--
    public GameObject mBackGrid;
    public GameObject m_eraser;
    public GameObject m_grass;
	public Texture textureBackGround;
	public Texture textureBackGroundPhoto;

//	Rect btnTakePhoto;
//	Rect btnFolder;
//	Rect btnPlan;
//	Rect btnExample;
//	
//	float btnTPSize = 300;
//	float btnFPSize = 200;
	
//	float margeLft;
//	float margeTop;
//	float margeInterH;
//	float margeInterV;
//	float btnWSize;//width
//	float btnHSize;//height

//	float uiPos;
    bool started = false;

    public bool Started
    {
        get { return started; }
    }
    bool imgSelector = false;
    bool imgSelectorBackToStart = true;

    public bool ImgSelector
    {
        get { return imgSelector; }
        set { imgSelector = value; }
    }
	bool imgPersoSelector = false;
	bool loadstarted = true;
//	bool videoStarted = false;
	//Options Authorization
	private bool m_authClientsFile;
	private bool m_authPhotoPerso;	
	
	Texture[] phototheque;
	Texture[] persotheque;
	string[] paths;
	Texture2D tmp;
	public Texture bkupTexture;
	
	//test
//	int selected = -1;
//	string[] content = {"photo","plan","folder","examples"};
	int photoIndex = -1;
	Vector2 scrollPosition = new Vector2 (0, 0);
	Rect ScrollView;
	Rect insideWindow;
	private Rect mButtonBackRect;				// Rectangle du bouton "flèche droite" sur l'écran de sélection d'image
	int persoIndex = 0;
	public GUISkin skin;
	public Texture BgTexture_forLicenceExpiration;
	int m_selSM = -1;
	int m_selT = -1;
	int m_selM = -1;
    private int m_oldSelT = 0;
	
//	bool isPhototekRdy = false;
	private List<MonoBehaviour> m_listBckupComponentExplorer = new List<MonoBehaviour>();
	
	//New Interface
	//GUIItemV2 Title;
	GUIItemV2 Root;
//	private int selT = -1;
	Vector2 scrollpos = new Vector2 (0, 0);//vertical scroll menu trop grand
	private bool m_isSliding = false;
	
//	private Texture2D fondLoading;      // Valeur fixée automatiquement par Start()
//	private Texture2D fondStart;
	public Texture2D fondStandard;
	public Texture2D fadeSelector;
	public Texture2D photoHoldStill;


	private bool _guiReady = false;
	private bool m_bpopUpTakePhoto = false;
	
	//Confirmation box
	GUIDialogBox _cb;

    // -- Progress Bar --
	private Rect r_loadBarFond;
	private Rect r_loadBarGrpMv;
	private Rect r_loadBar;
    public GUISkin loadingSkin;

	AsyncOperation async;
//	Texture2D test;
	string byPassAction;
	public bool isBypassed = false;
	public bool bloadingTerminate = false;
	
	private Rect _logoRect;
	private Texture2D _logo;
	
	//Items
	private	GUIItemV2	_itemPhotoExample;
	private	GUIItemV2	_itemUpdate;
    private	GUIItemV2	_itemQuit;	
	//private	GUIItemV2	_btnSwitchAidePopup;
	private GUIItemV2 	_btnSwitchTouch;
	
    // -- Loading circle animé --
    private RotatingGUITexture mLoadingCircle;

    private static readonly string      DEBUGTAG               = "GUIStart : ";
	
	private PluginPhotoManagerGUI _pluginPhotoRef;	
	
	private GUIRandomHelpSentence guiRandomHelpSentence;
	private GUINotificationLicenceExpiration guiNotificationLicenceExpiration;

	//Equivalent des anciennes scnènes
	GameObject _explorer;
	GameObject _mainWithLibs;
    public GameObject m_mainCam;
	
	private static Texture2D _staticRectTexture;
	private static GUIStyle _staticRectStyle;

	Quaternion ReferenceRotation;
	// Note that this function is only meant to be called from OnGUI() functions.
	public static void GUIDrawRect( Rect position, Color color )
	{
		if (_staticRectTexture == null) {
				_staticRectTexture = new Texture2D (1, 1);
		}

		if (_staticRectStyle == null) {
				_staticRectStyle = new GUIStyle ();
		}

		_staticRectTexture.SetPixel (0, 0, color);
		_staticRectTexture.Apply ();

		_staticRectStyle.normal.background = _staticRectTexture;

		GUI.Box (position, GUIContent.none, _staticRectStyle);
		
	}
		
#region FCNs. Untity
    //-----------------------------------------------------
	void Awake()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
        //AndroidCommons.Init();
#endif
		ReferenceRotation = GameObject.Find ("SpecLight").transform.rotation;
		ModulesAuthorization.moduleAuth += AuthorizePlugin;
		usefullData.logoUpdated += LogoUpdate;
		StartCoroutine(CheckNewVersion());

        UsefullEvents.OnResizingWindow  += SetRects;
        UsefullEvents.OnResizeWindowEnd += SetRects;
		
		_pluginPhotoRef = GetComponent<PluginPhotoManagerGUI>();
	}	
	
	//-----------------------------------------------------
	void Start ()
	{
        if(m_mainCam == null)  Debug.LogError(DEBUGTAG+"mainCam"+PC.MISSING_REF);
        if(mBackGrid == null) Debug.LogError(DEBUGTAG+"BackGrid"+PC.MISSING_REF);
        if(m_eraser == null)  Debug.LogError(DEBUGTAG+"Eraser"  +PC.MISSING_REF);
        if(m_grass == null)  Debug.LogError(DEBUGTAG+"Grass"  +PC.MISSING_REF);

        // -- Progress bar --
        r_loadBarFond = new Rect(162,UnityEngine.Screen.height*0.865f,UnityEngine.Screen.width-162*2,4); // new Rect(162,664,700,4);
        r_loadBarGrpMv = new Rect(162,UnityEngine.Screen.height*0.865f,0,4);                 // new Rect(162,664,0,4);
        r_loadBar = new Rect(0,0,UnityEngine.Screen.width-162*2f,4);                         // new Rect(0,0,700,4);

        // -- Loading circle animé --
        mLoadingCircle = this.GetComponent<RotatingGUITexture>();
        if(mLoadingCircle != null)
            mLoadingCircle.SetRect(new Rect(UnityEngine.Screen.width-192f, UnityEngine.Screen.height-192f, 128f, 128f));

        UnityEngine.Screen.autorotateToPortrait = false;

		//Marges
//		margeLft = (5 * UnityEngine.Screen.width) / 32;
//		margeTop = (4 * UnityEngine.Screen.height) / 24;
//		margeInterH = (2 * UnityEngine.Screen.width) / 32;
//		margeInterV = (2 * UnityEngine.Screen.height) / 24;
//		btnWSize = (10 * UnityEngine.Screen.width) / 32;
//		btnHSize = (7 * UnityEngine.Screen.height) / 24;

		//RECTS
		mButtonBackRect = new Rect();
		SetRects();
		
//		uiPos = UnityEngine.Screen.width / 4;

		ScrollView = new Rect (162, 145, 700, 485);
		
		fillPhototheque ();
		
		//Confirmation box
		_cb = GetComponent<GUIDialogBox>();
		
		//Dossier client Loading Override
		if(PlayerPrefs.HasKey(usefullData.k_toLoadPath) && PlayerPrefs.HasKey(usefullData.k_toLoadParams))
		{
			string path = PlayerPrefs.GetString(usefullData.k_toLoadPath);
			if(path != "")
			{
				started = false;
				//GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
			}
		}	
		/*if(!PlayerPrefs.HasKey("HelpPopup"))
        {
            PlayerPrefs.SetInt("HelpPopup", 1);
        }*/
        
		guiNotificationLicenceExpiration = new GUINotificationLicenceExpiration();
		guiNotificationLicenceExpiration.skin = skin;
		guiNotificationLicenceExpiration.BgTexture = BgTexture_forLicenceExpiration;

		guiRandomHelpSentence = new GUIRandomHelpSentence();
		guiRandomHelpSentence.skin = skin;
		StartCoroutine(guiRandomHelpSentence.init());

	} // Start()

    //-----------------------------------------------------
	void Update ()
	{
		//print ("STARTED : " + started);
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		//Slide des images
		if(imgSelector)
		{
			Vector2 delta = Vector2.zero;
			if(PC.In.Drag1(out delta))
			{
				scrollPosition.y += delta.y;
				m_isSliding = true;
			}
			
			if(PC.In.Drag1End())
			{
				m_isSliding = false;	
			}
		}
		#endif
		
		guiRandomHelpSentence.update();
		
		if(_itemUpdate!=null)
			_itemUpdate.SetEnableUI(usefullData.HasNewVersion());
		
	//	if((m_selM == 4)||(m_selM == 9))
		{
			ui2fcn();
		}
		if(started && isBypassed)
		{
			isBypassed = false;
//			PlayerPrefs.SetString(usefullData.k_clientLink,Mo//La
//			StartCoroutine(LoadLevel(false));
		}
		if(PlayerPrefs.HasKey(usefullData.k_startBypass))
		{
			byPassAction = PlayerPrefs.GetString(usefullData.k_startBypass);
			if(byPassAction != "")
			{
				PlayerPrefs.DeleteKey(usefullData.k_startBypass);
				started = false;
				isBypassed = true;
				
				#if UNITY_IPHONE && !UNITY_EDITOR
				switch (byPassAction)
				{
				case "photo":
					GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = true;
					GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(true);
					//bg.guiTexture.texture = photoHoldStill;
					_pluginPhotoRef.SetMode(false);
					EtceteraManager.setPrompt(true);
					EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Camera );
					break;
				case "extImage":
					_pluginPhotoRef.SetMode(true);
					EtceteraManager.setPrompt(true);
					EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Album );
					break;
				case "exemple":
					imgSelector = true;
					mBackGrid.GetComponent<BackGridManager>().SetSelectorMode();
					break;
				}
				#endif
				
				#if UNITY_ANDROID
				string name = "tmp"+System.DateTime.Now.ToString("dd-mm-yyyy_HH-mm-ss");
				switch (byPassAction)
				{
					case "photo":
						GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = true;
						GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(true);
						byPassAction = "";
						int height = (int)(UnityEngine.Screen.width *0.75f);
						_pluginPhotoRef.SetMode(false);
                        AndroidPlugin.takePicture();
                        //AndroidCommons.TakePicture("/Handler", "TakePictureCallback");
						//EtceteraAndroid.promptToTakePhoto(UnityEngine.Screen.width,/*UnityEngine.Screen.height*/height,name);
						break;
					case "extImage":
						byPassAction = "";
						_pluginPhotoRef.SetMode(true);
                        AndroidPlugin.browseGallery();
						//EtceteraAndroid.promptForPictureFromAlbum(UnityEngine.Screen.width,UnityEngine.Screen.height,name);
						break;
					case "exemple":
						imgSelector = true;
					    mBackGrid.GetComponent<BackGridManager>().SetSelectorMode();
						break;
				}
				#endif
				#if UNITY_STANDALONE_WIN
				switch (byPassAction)
				{
//				case "photo":
//					break;
				case "extImage":
					OpenFile();

					break;
				case "exemple":
					imgSelector = true;
					//mBackGrid.GetComponent<BackGridManager>().SetSelectorMode();
					break;
				}
				#endif
				
				#if UNITY_STANDALONE_OSX
				switch (byPassAction)
				{
//				case "photo":
//					break;
				case "extImage":
					OpenFile();
					break;
				case "exemple":
					imgSelector = true;
					mBackGrid.GetComponent<BackGridManager>().SetSelectorMode();
					break;
				}
				#endif
			}
		}
	}
	
	void OnGUI ()
	{

		if(GetComponent<LibraryLoader>() == null)
		{
			if( !imgSelector )
				GUI.DrawTexture (new Rect (Screen.width - textureBackGround.width+50, Screen.height - textureBackGround.height, textureBackGround.width-50, textureBackGround.height), textureBackGround);

			guiRandomHelpSentence.updateGui();
			guiNotificationLicenceExpiration.updateGui();
		}
		
		GUISkin bkup = GUI.skin;
		GUI.skin = skin;

		/*if(m_bpopUpTakePhoto)
		{
			GUI.DrawTexture(new Rect(0,0,Screen.width, Screen.height),textureBackGroundPhoto);
			if(GUI.Button(new Rect((Screen.width-519)/2, Screen.height/6.70f, 519, 502.5f), "", "okPhoto"))
			{
				started = false;
				GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = true;
				GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(true);
				//				bg.guiTexture.texture = photoHoldStill;
				_pluginPhotoRef.SetMode(false);
				EtceteraManager.setPrompt(true);
				EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Camera );

				resetMenu();
				setVisibility(false);
				m_bpopUpTakePhoto = false;
			}
			Rect[] temp = new Rect[1];
			temp[0] = new Rect((Screen.width-519)/2, Screen.height/6.70f, 519, 502.5f);
			if(!PC.In.ClickOnUI(temp) && PC.In.Click1Down())
			{
				resetMenu();
				setVisibility(false);
				m_bpopUpTakePhoto = false;
			}
			
			GUI.Label(new Rect(0, Screen.height/6.70f+0.0f, Screen.width, 50), "DETECTION DU SOL PENDANT LA PRISE DE PHOTO", "okPhotoTxt");
			GUI.Label(new Rect(0, Screen.height/6.70f+330.0f, Screen.width, 50), "Cadrez, prenez la photo puis tapez sur 'use photo'", "okPhotoTxt2");
			GUI.Label(new Rect(0.0f, Screen.height/6.70f+380.0f, Screen.width, 50), "NE PAS BOUGER L'iPAD TANT QUE LA PHOTO\nN'EST PAS VALIDEE", "okPhotoTxt");
			return;
		}*/

#if UNITY_IPHONE && !UNITY_EDITOR
		if(m_bpopUpTakePhoto)
		{
			GUI.DrawTexture(new Rect(0,0,Screen.width, Screen.height),textureBackGroundPhoto);
			if(GUI.Button(new Rect((Screen.width-519)/2, Screen.height/6.70f, 519, 502.5f), "", "okPhoto"))
			{
				started = false;
				GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = true;
				GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(true);
				//				bg.guiTexture.texture = photoHoldStill;
				_pluginPhotoRef.SetMode(false);
				EtceteraManager.setPrompt(true);
				EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Camera );
		
				resetMenu();
				setVisibility(false);
				m_bpopUpTakePhoto = false;
			}
			Rect[] temp = new Rect[1];
			temp[0] = new Rect((Screen.width-519)/2, Screen.height/6.70f, 519, 502.5f);
			if(!PC.In.ClickOnUI(temp) && PC.In.Click1Down())
			{
				resetMenu();
				setVisibility(false);
				m_bpopUpTakePhoto = false;
			}

			GUI.Label(new Rect(0, Screen.height/6.70f+0.0f, Screen.width, 50), "DETECTION DU SOL PENDANT LA PRISE DE PHOTO", "okPhotoTxt");
			GUI.Label(new Rect(0, Screen.height/6.70f+330.0f, Screen.width, 50), "Cadrez, prenez la photo puis tapez sur 'use photo'", "okPhotoTxt2");
			GUI.Label(new Rect(0.0f, Screen.height/6.70f+380.0f, Screen.width, 50), "NE PAS BOUGER L'iPAD TANT QUE LA PHOTO\nN'EST PAS VALIDEE", "okPhotoTxt");
			return;
		}
#endif

		//MENU START
		if (started)
		{

           	int guiW = 210;
			GUILayout.BeginArea (new Rect (/*590*/UnityEngine.Screen.width-210, 0, guiW, UnityEngine.Screen.height));
			GUILayout.FlexibleSpace ();
		
			scrollpos = GUILayout.BeginScrollView (scrollpos, "empty", GUILayout.Width (300));//scrollView en cas de menu trop grand
			
			GUILayout.Box ("", "bgFadeUp", GUILayout.Width (guiW), GUILayout.Height (150));//fade en haut
			GUILayout.BeginVertical ("bgFadeMid", GUILayout.Width (guiW));
			
			//Title.getUI(false);
			GetRoot().showSubItms ();//Menu
			
			GUILayout.EndVertical ();
			GUILayout.Box ("", "bgFadeDw", GUILayout.Width (guiW), GUILayout.Height (150));//fade en bas
			
			GUILayout.EndScrollView ();
			
			GUILayout.FlexibleSpace ();
			GUILayout.EndArea ();
			
			//LOGO
			if(_logo != null)
			{
//				GUI.Label(new Rect(UnityEngine.Screen.width-250,40,100,20),TextManager.GetText("GUIStart.edition"),"edition");
				GUI.DrawTexture(new Rect(/*UnityEngine.Screen.width-138*/20.0f,20.0f,usefullData._logoRect.width,usefullData._logoRect.height),_logo);	
			}
			
		}
//		else
//		{
//			if(bg.guiTexture.texture == fondLoading)
//			{
//				GUI.Label(new Rect(UnityEngine.Screen.width/2-100,UnityEngine.Screen.height-80,200,40), TextManager.GetText("Activation.wait"), "waitStart");
//			}
//		}
			//MENU SELECTEUR D'IMAGE
			if (imgSelector)
			{
				// -- Appelé au clic "bouton back" dans le menu sélection d'image (à chaque fois) --
				float falphaColor = 0.8f;
				Rect r_fullPanelBG       = new Rect(162, 134, 700, 500);
			Rect r_fullPanelSV       = new Rect(0,0,624,468);
			Rect r_fullPanel         = new Rect(200,150,624,468);

			GUI.BeginGroup(new Rect((UnityEngine.Screen.width-1024)/2,(UnityEngine.Screen.height-768)/2,1024,768));
			
			Rect r_halfPanelLeft     = new Rect(162,134+50,350,500-50);
			mButtonBackRect        = new Rect(r_halfPanelLeft.x-30, r_halfPanelLeft.y+(r_halfPanelLeft.height-150)/2,30,150);



			//GUILayout.BeginArea(new Rect(0,0,UnityEngine.Screen.width,UnityEngine.Screen.height));
			//GUILayout.BeginVertical();
			//GUILayout.FlexibleSpace();
			
			//GUILayout.Space(10);
			
			//GUILayout.BeginHorizontal();
			/*GUILayout.FlexibleSpace();
			if(GUILayout.RepeatButton("","selectUp",GUILayout.Width(100),GUILayout.Height(30)))
			{
				scrollPosition.y = scrollPosition.y - 2;
			}
			GUILayout.FlexibleSpace();*/
			//GUILayout.EndHorizontal();
			
			//GUILayout.BeginHorizontal();
			//r_group1.Set(;
			

			
			Color bkupColor = GUI.color;
			GUI.color = new Color(bkupColor.a, bkupColor.g, bkupColor.b, falphaColor);
			GUI.Box(r_fullPanelBG,"","clientPanelBg");
			GUI.color = bkupColor;
			
			/*sp_fullPanel = GUI.BeginScrollView(r_fullPanel,sp_fullPanel,r_fullPanelSV);
			string style = "fullPanelSG";
			if(suppressionMode)
				style = "fullPanelSGSuppr";
			int tmpId = GUI.SelectionGrid(r_fullPanelSV,id_selectedClient,m_clients,4,style);
			
			GUI.EndScrollView();*/
			
			bkupColor = GUI.color;
			GUI.color = new Color(bkupColor.a, bkupColor.g, bkupColor.b, falphaColor);
			GUI.Box(new Rect(r_fullPanelBG.x,r_fullPanelBG.y,r_fullPanelBG.width,30),"","faderTop");
			GUI.Box(new Rect(r_fullPanelBG.x,r_fullPanelBG.yMax-100,r_fullPanelBG.width,100),"","faderBtm");
			GUI.color = bkupColor;
			
			//GUILayout.FlexibleSpace();
			scrollPosition = GUI.BeginScrollView(r_fullPanel,scrollPosition,r_fullPanelSV);/*"empty",	GUILayout.Width((700*UnityEngine.Screen.width)/1024),
																				GUILayout.Height((500*UnityEngine.Screen.height)/768));*/
			/*if(m_isSliding)
				GUILayout.SelectionGrid(photoIndex,phototheque,4,"altBtn");
			else*/
			//int tmpId = GUI.SelectionGrid(r_fullPanelSV,id_selectedClient,m_clients,4,style);
			
			//sp_fullPanel = GUI.BeginScrollView(r_fullPanel,sp_fullPanel,r_fullPanelSV);
			
			photoIndex = GUI.SelectionGrid(r_fullPanelSV, photoIndex,phototheque,4,"altBtn");
			
			GUI.EndScrollView();
			
			GUI.EndGroup();
			Rect temp = new Rect((Screen.width/2)-350, (Screen.height/2)-250, 700, 500);
			//GUIDrawRect(temp,Color.cyan);
			
			if(
				#if UNITY_IPHONE
				Input.anyKey
#else
				Input.GetMouseButtonDown(0)
				#endif		
				&& !temp.Contains(Input.mousePosition))
			{
				if (!imgSelectorBackToStart)
				{
					m_mainCam.GetComponent<GuiTextureClip>().enabled = true;
					started = false;
					mBackGrid.GetComponent<BackGridManager>().SetMontageMode();
					imgSelectorBackToStart = true;
					GetComponent<GUIMenuMain>().setStarter(true);
				}
				else
				{
					started = true;
					//mBackGrid.GetComponent<BackGridManager>().SetMenuMode();
				}
				imgSelector = false;
			}
			//GUI.DrawTexture (GUILayoutUtility.GetLastRect(), fadeSelector);
			//GUILayout.FlexibleSpace();
			//GUILayout.EndHorizontal();
			
			//GUILayout.BeginHorizontal();
			/*GUILayout.FlexibleSpace();
			if(GUILayout.RepeatButton("","selectDw",GUILayout.Width(100),GUILayout.Height(30)))
			{
				scrollPosition.y = scrollPosition.y + 2;
			}
			GUILayout.FlexibleSpace();*/
			//GUILayout.EndHorizontal();
			
			//GUILayout.FlexibleSpace();
			//GUILayout.EndVertical();
			//GUILayout.EndArea();
			
//			scrollPosition = GUI.BeginScrollView (ScrollView, scrollPosition, insideWindow, "empty", "empty");
//			photoIndex = GUI.SelectionGrid (insideWindow, photoIndex, phototheque, 3, "altBtn");
			
            //ImgSelector Validate
            if (photoIndex != -1)
			{
				imgSelector = false;
				string name = phototheque [photoIndex].name;
				Texture2D tex = (Texture2D)Resources.Load ("phototheque/" + name);
				Montage.sm.updateFond (tex, true, photoIndex.ToString ());
				// m_mainCam.GetComponent<Mode2D>().Reset();
				GetComponent<GUIMenuMain>().bpresetPhoto = true;
				GetComponent<GUIMenuMain>().setStarter(true);
                GameObject.Find("mainCam").GetComponent<MainCamManager>().FitViewportToScreen();
                
				LightConfiguration lc = GameObject.Find("LightPivot").GetComponent<LightConfiguration>();
				SceneControl sc = GameObject.Find("camPivot").GetComponent<SceneControl>();
				
				if(sc)
				{
					string fullpath = "phototheque/configs/samplepic_" + (photoIndex + 1)/* + ".txt"*/; // the file is actually ".txt" in the end
					
					TextAsset textAsset = (TextAsset) Resources.Load(fullpath, typeof(TextAsset));
					/*if (textAsset == null) 
					{
						Debug.LogWarning("[TextManager] "+ fullpath +" file not found.");
						Debug.LogWarning("[TextManager] "+ _defaultFile +" file is use by default.");
						fullpath = "Languages/" +_defaultFile;
						textAsset = (TextAsset) Resources.Load(fullpath, typeof(TextAsset));
						//	return false;
					}*/
					
					//Debug.Log("[TextManager] loading: "+ fullpath);
					
				/*	if (textTable == null) 
					{
						textTable = new Hashtable();
					}
					
					textTable.Clear();*/
					
					StringReader reader = new StringReader(textAsset.text);
					
					//TextReader reader;
					string fileName = Application.dataPath + "/Resources/phototheque/configs/samplepic_" + (photoIndex + 1) + ".txt";
					
					//reader = new StreamReader(fileName);
					
					if(reader != null)
					{
						string floatString = "";
						//string result = reader.ReadToEnd();
						do
						{
							floatString = reader.ReadLine();
							
							if(floatString != null)
							{
								string szkey = floatString.Substring(0, floatString.IndexOf("="));
								float fvalue = float.Parse(floatString.Substring(floatString.IndexOf("=") + 1));
								
								switch(szkey)
								{
								case "Inclinaison" :
									
									sc.initI(fvalue);
									break;
									
								case "Echelle" :
								
									sc.initS(fvalue * 10.0f);
									break;
									
								case "Hauteur" :
								
									sc.initH(fvalue * -10.0f);
									break;
									
								case "Rotation" :
									
									sc.initL(fvalue);
									break;
									
								case "Perspective" :
									
									sc.initP(fvalue);
									break;
									
								case "Roulis" :
									
									sc.initR(fvalue);
									break;
									
								case "Heure" :
								
									lc.initHour(fvalue);
									break;
									
								case "Orientation" :
								
									lc.initAzimut(fvalue);
									break;
									
								case "Luminosite" :
								
									lc.initIntensity(fvalue);
									break;
									
								case "Nuit" :
								
									lc.SetNightMode(fvalue == 1 ? true : false);
									break;

								case "RotationSpecLightAprem" :
									if(fvalue!=0)
									{
										Quaternion tempRotation = GameObject.Find ("SpecLight").transform.rotation;
										Quaternion tempRotation1 = GameObject.Find ("SpecLight").transform.rotation;
										Debug.Log("aaaaaaaaaaaaaaa  " + fvalue);
										//tempRotation.y =fvalue;
										tempRotation = ReferenceRotation * Quaternion.Euler(0,fvalue,0);
										tempRotation1 = tempRotation*Quaternion.Euler(0,180,0);
										
										GameObject.Find ("SpecLight").transform.rotation = tempRotation;

										GameObject.Find ("LightPivot").GetComponent<LightConfiguration>().sunSpecMatin.transform.rotation = tempRotation1;
									}
									break;
								}
							}
							
						}while(floatString != null);
						
						reader.Close();
					}
				}
				
				photoIndex = -1;
				
				sc.saveBkup();
			}
			
//			GUI.EndScrollView ();
//			
//			GUI.DrawTexture (ScrollView, fadeSelector);// fade Haut/Bas
//			
//			if (GUI.RepeatButton (new Rect (UnityEngine.Screen.width / 2 - 50, ScrollView.yMax + 4, 100, 30), "", "selectDw")) {
//				scrollPosition.y = scrollPosition.y + 2;
//			}
//			if (GUI.RepeatButton (new Rect (UnityEngine.Screen.width / 2 - 50, ScrollView.y - 32, 100, 30), "", "selectUp")) {
//				scrollPosition.y = scrollPosition.y - 2;
//			}
				
			
		}		
		
		GUI.skin = bkup;
		
		//Chargement
		if(async != null)
		{
			GUI.skin = loadingSkin;
			GUI.Box(r_loadBarFond,"","loadingBg");
			r_loadBarGrpMv.width = async.progress * r_loadBarFond.width;
			
			GUI.BeginGroup(r_loadBarGrpMv);
			GUI.Label(r_loadBar,"","loadingBar");
			GUI.EndGroup();
			GUI.Label(new Rect(r_loadBarFond.xMax-150,r_loadBarFond.yMax,150,30),TextManager.GetText("GUIStart.loading"),"loadtxt");
		}
	}
	
	public void setMenuMode(int imenu, int isousMenu)
	{
		m_selM = imenu;
		m_selSM = isousMenu;

		if(imenu >= 0)
		{
			Root.setSelected(imenu);
			
			if(isousMenu >= 0)
			{
				Root.getSelectedItem().setSelected(isousMenu);
			}
		}
		int[] indexs = new int[3];
		indexs[0] = imenu;
		indexs[1] = isousMenu;
		indexs[2] = -1;
		UsefullEvents.FireUpdateUIState(GetType().ToString(),indexs);
	}

    //-----------------------------------------------------
    void OnDestroy()
    {
        
        ModulesAuthorization.moduleAuth -= AuthorizePlugin;
        usefullData.logoUpdated -= LogoUpdate;

        UsefullEvents.OnResizingWindow  -= SetRects;
        UsefullEvents.OnResizeWindowEnd -= SetRects;
    }
	
	#endregion

    //-----------------------------------------------------
    private void SetRects()
    {
        mButtonBackRect.Set(159/1024f*UnityEngine.Screen.width-30f, UnityEngine.Screen.height / 2 - 75, 30, 150);
        // Note : 159/1024 pour garder la bonne distance(bord-cadre) en pixels sur l'image strechée
    }

//	public void cleanMemory()
//	{
//		phototheque = null;
//		Resources.UnloadUnusedAssets();
//		//TODO la suite
//	}

	void fillPhototheque ()
	{
		//Load example photos
//		isPhototekRdy = false;
		Object[] textures = Resources.LoadAll ("phototheque/thumbs", typeof(Texture2D));
		phototheque = new Texture[textures.Length];
		int i = 0;
		foreach (Object o in textures) {
			phototheque [i] = (Texture)o;
			i++;
//			yield return new WaitForSeconds(Time.deltaTime);
		}
		insideWindow = new Rect (10, 0, ScrollView.width, phototheque.Length * 50);

//		isPhototekRdy = true;
//		yield return true;
	}
	
	void fillPersotheque ()
	{
		//Load personnal photos
		int total = 0;
		foreach (string s in Directory.GetFiles(usefullData.PhotoThumbPath)) {
			if (!s.Contains ("Thumbs")) {
				total++;
			}
		}
		persotheque = new Texture[total];
		paths = new string[total];
		foreach (string s in Directory.GetFiles(usefullData.PhotoThumbPath)) {
			if (!s.Contains ("Thumbs")) {
				StartCoroutine (loadPersonalImgs (s));
			}
		}
	}
	
	
	//-----------------------------------
	IEnumerator loadPersonalImgs (string s)
	{	
		WWW www = new WWW ("file://" + s);
		yield return www;
		tmp = new Texture2D (1, 1);
		www.LoadImageIntoTexture (tmp);
		persotheque [persoIndex] = (Texture)tmp;		
		paths [persoIndex] = s.Replace (usefullData.PhotoThumbPath, usefullData.PhotoPath);
		persoIndex++;
		www.Dispose ();
		yield return true;
	}

	IEnumerator loadStandAloneImg (string s)
	{	
		Debug.Log ("Path1 : " + s);
	
		

#if UNITY_STANDALONE_OSX
		s = s.Replace("\\","/");
#else
		s = s.Replace("/","\\");
#endif
	  	s = TextUtils.EncodeToHtml(s);
	//	s = WWW.EscapeURL(s);
		string path = "file://"+s;
		//path = "file://C:/Users/dthibault/Desktop/export.png";
		//path = "file://D:/export.png";
		
		Debug.Log(path);
		WWW www = new WWW( path);
		yield return www;
		/*while(!www.isDone)
		{
			Debug.Log(www.progress);
			yield return new WaitForSeconds(1);
		}*/
		if (www.error != null)
		{
        	Debug.LogWarning(www.error);
			yield return true;
		}
		else
		{
			Debug.Log ("Path2 : " + path);
			tmp = new Texture2D (1, 1);
			www.LoadImageIntoTexture (tmp);
		

		
			//persotheque [persoIndex] = (Texture)tmp;		
			//paths [persoIndex] = s.Replace (usefullData.PhotoThumbPath, usefullData.PhotoPath);
			//persoIndex++;
		
			/* www.Dispose ();*/
			if((tmp.width>1920.0f) || (tmp.height>1920.0f))
			{
				Rect resizeRect = ImgUtils.ResizeImagePreservingRatio(tmp.width,tmp.height,1920,1920);				
				ImgUtils.Point(tmp, (int)resizeRect.width, (int)resizeRect.height,true);
				tmp.Apply();
			}
			Montage.sm.updateFond (tmp, false, s);
			
			while(!_guiReady)
			{
				yield return new WaitForEndOfFrame();	
			}
			GameObject.Find("camPivot").GetComponent<SceneControl>().init ();
			GetComponent<GUIMenuMain> ().setStarter (true, false);

			GetComponent<GUIMenuLeft> ().ResetMenu();
			GetComponent<GUIMenuRight> ().showMenu = true;
			GetComponent<GUIMenuRight> ().updateGUIV2( GetComponent<GUIMenuRight> ().activeItem,0,true);

			yield return true;
		}
	}
	
	public Texture2D getPicFromPhototheque (int index)
	{
//		Object[] textures = Resources.LoadAll("phototheque", typeof(Texture2D));
//		Texture2D tmp = (Texture2D)textures[index];
//		return tmp;
		string name = phototheque [index].name;
		Texture2D tex = (Texture2D)Resources.Load ("phototheque/" + name);
		return tex;
		
		//return (Texture2D)phototheque[index];
	}

    //-----------------------------------------------------
	public void showStart (bool b) // Passage du montage au menu principal
	{
		enabled = b;
		isBypassed = false;
		started = b;
		if (b) 
		{
			GameObject.Find ("backgroundImage").GetComponent<GUITexture>().texture = null;
            mBackGrid.GetComponent<BackGridManager>().SetMenuMode();
            m_mainCam.GetComponent<GuiTextureClip>().enabled = false;
			if(!usefullData.lowTechnologie)
			{
	            m_eraser.GetComponent<EraserV2>().SetVisible(false);
	            m_grass.GetComponent<GrassV2>().SetVisible(false);
			}
		}
	}

    public void DontShowStartAnd(bool tkPhoto = false, bool importPhoto = false, bool showSelector = false)
    {
		Debug.Log("DontShowStart and ...");
        enabled = true;
        isBypassed = false;
        GameObject.Find("backgroundImage").GetComponent<GUITexture>().texture = null;
		mBackGrid.GetComponent<BackGridManager>().SetMenuMode();
        m_mainCam.GetComponent<GuiTextureClip>().enabled = false;
        if (!usefullData.lowTechnologie)
        {
            m_eraser.GetComponent<EraserV2>().SetVisible(false);
            m_grass.GetComponent<GrassV2>().SetVisible(false);
        }

        if (showSelector)
        {
            started = false;
            imgSelector = true;
            imgSelectorBackToStart = false;
            //mBackGrid.GetComponent<BackGridManager>().SetSelectorMode();
        }
        else if (tkPhoto)
        {
			m_bpopUpTakePhoto = true;
			
			GetComponent<GUIMenuMain>().setStarter(true);
        }
        else if (importPhoto)
        {
            //m_mainCam.GetComponent<GuiTextureClip>().enabled = true;
            started = false;
            //mBackGrid.GetComponent<BackGridManager>().SetMontageMode();
            GetComponent<GUIMenuMain>().setStarter(true);

            _pluginPhotoRef.SetMode(true);
#if UNITY_IPHONE && !UNITY_EDITOR
			EtceteraManager.setPrompt(true);
			EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Album );
#elif UNITY_ANDROID
            // showStart(false);
            AndroidPlugin.browseGallery();
            //EtceteraAndroid.promptForPictureFromAlbum(UnityEngine.Screen.width,UnityEngine.Screen.height,name);
#elif UNITY_STANDALONE
			OpenFile();
#endif	

        }
		
    }






	public void ShowStartAnd(bool tkPhoto = false,bool importPhoto = false,bool showSelector = false)
	{
		bkupTexture = GameObject.Find ("backgroundImage").GetComponent<GUITexture>().texture;
		
		Debug.Log("ShowStart and ...");
		enabled = true;
		isBypassed = false;
		GameObject.Find ("backgroundImage").GetComponent<GUITexture>().texture = null;
        mBackGrid.GetComponent<BackGridManager>().SetMenuMode();
        m_mainCam.GetComponent<GuiTextureClip>().enabled = false;
		if(!usefullData.lowTechnologie)
		{
	        m_eraser.GetComponent<EraserV2>().SetVisible(false);
	        m_grass.GetComponent<GrassV2>().SetVisible(false);
		}
		
		if(showSelector)
		{
			started = false;
			imgSelector = true;
			//mBackGrid.GetComponent<BackGridManager>().SetSelectorMode();
		}
		else if(tkPhoto)
		{
			m_bpopUpTakePhoto = true;
			started = true;
			imgSelector = false;
		}
		else if(importPhoto)
		{
			_pluginPhotoRef.SetMode(true);
			#if UNITY_IPHONE && !UNITY_EDITOR
			EtceteraManager.setPrompt(true);
			EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Album );
			#elif UNITY_ANDROID
           // showStart(false);
            AndroidPlugin.browseGallery();
			//EtceteraAndroid.promptForPictureFromAlbum(UnityEngine.Screen.width,UnityEngine.Screen.height,name);
			#elif UNITY_STANDALONE

			if(!OpenFile())
			{
				enabled = false;
				isBypassed = false;
				GameObject.Find ("backgroundImage").GetComponent<GUITexture>().texture = bkupTexture;
				mBackGrid.GetComponent<BackGridManager>().SetMontageMode();
				m_mainCam.GetComponent<GuiTextureClip>().enabled = true;
				
				GetComponent<GUIMenuRight>().setVisibility(true);
				GetComponent<GUIMenuRight>().canDisplay(true);
				
				GetComponent<GUIMenuLeft>().setVisibility(true);
				GetComponent<GUIMenuLeft>().canDisplay(true);
				
				if(!usefullData.lowTechnologie)
				{
					m_eraser.GetComponent<EraserV2>().SetVisible(true);
					m_grass.GetComponent<GrassV2>().SetVisible(true);
				}
			}
			#endif
			
		}
		
	}
	
	public bool isActive ()
	{
		return started || imgSelector || async != null;
	}
	
/*	IEnumerator loadOne (string s)
	{
		if (loadstarted) {
			loadstarted = false;
			WWW www = new WWW ("file://" + s);
			yield return www;
			tmp = new Texture2D (1, 1);
			www.LoadImageIntoTexture (tmp);
			
			imgPersoSelector = false;
			
			Montage.sm.updateFond (tmp, false, s);
			
			while(!_guiReady)
			{
				yield return new WaitForEndOfFrame();	
			}
			
			GetComponent<GUIMenuMain> ().setStarter (true);
			
			photoIndex = -1;
		
			www.Dispose ();
			loadstarted = true;
		}
		yield return true;
	}*/
	
/*	public IEnumerator LoadLevel(bool oneShot)
	{
		while(GetComponent<LibraryLoader>() != null)
		{
			yield return new WaitForEndOfFrame();
		}
		
		if(GetComponent<Montage>().getClientStr() != "")
		{
			PlayerPrefs.SetString(usefullData.k_selectedClient,GetComponent<Montage>().getClientStr());
		}
		
		if(usefullData.sc_oneShot == "" || usefullData.sc_clientExplorer == "")
			Debug.Log("NO SCENE SET");
		else
		{
			if(oneShot)
	        	async = UnityEngine.Application.LoadLevelAsync(usefullData.sc_oneShot);
			else
				async = UnityEngine.Application.LoadLevelAsync(usefullData.sc_clientExplorer);
	        yield return async;
	        Debug.Log("Loading complete");
		}
    }*/
	
	#region new interface
	
	public void updateGUI (GUIItemV2 itm, int val, bool reset)
	{		
		switch (itm.getDepth())
		{
			case 0:
				m_selM = val;
				if(reset)
				{
					m_selSM = -1;
					m_selT = -1;
					if(itm.getSelectedItem()!=null && itm.getSelectedItem().getSelectedItem()!=null)
						itm.getSelectedItem().resetSelected();
					if(itm.getSelectedItem()!=null)
						itm.resetSelected();
				}
				break;
				
			case 1:
				m_selSM = val;
				if(reset)
				{
					m_selT = -1;
					if(itm.getSelectedItem()!=null)
						itm.resetSelected();
				}
				break;
				
			case 2:
				m_selT = val;
				break;
		}	
		int[] indexs = new int[3];
		indexs[0] = m_selM;
		indexs[1] = m_selSM;
		indexs[2] = m_selT;
		
		UsefullEvents.FireUpdateUIState(GetType().ToString(),indexs);
	//	ui2fcn ();
	}
	
	public void setVisibility (bool b)
	{
		
	}
	
	public bool isVisible()
	{
		return false;
	}
	
	public void canDisplay (bool b)
	{
		
	}
	
	public bool isOnUI ()
	{
		return false;
	}
	
	private void Quit()
	{
		if(! _cb.isVisible())
		{
			_cb.showMe(true,GUI.depth);
			_cb.setText(TextManager.GetText("GUIStart.Exit")+" ?");
		}
		else
		{
			if(_cb.getConfirmation())
			{
				_cb.showMe(false,GUI.depth);
				
				//DO
				GetComponent<GUIMenuRight>().SetAllowQuit(true);
                InterProcess.Stop();
				#if UNITY_STANDALONE_WIN 
                System.Diagnostics.Process.GetCurrentProcess().Kill(); 
#else 
                UnityEngine.Application.Quit(); 
#endif
				//----------------------------
			}
			if(_cb.getCancel())
			{
				_cb.showMe(false,GUI.depth);
				
				//DO
				resetMenu();
				//----------
			}
		}
	}
	private void SwitchToExplorerV2()
	{
		_explorer = GameObject.Find("Explorer");		
		_mainWithLibs = GameObject.Find("MainWithLibs");
		if(_explorer!=null)
		{
		//	_explorer.SetActive(true);
			foreach(Transform tra in _explorer.transform)
			{
				if(tra.GetComponent<Camera>()!=null)
				{
					tra.GetComponent<Camera>().enabled=true;
					tra.gameObject.SetActive(true);
				}
				//tra.gameObject.SetActive(true);
			}
		}
		if(_mainWithLibs!=null)
		{
		//	_mainWithLibs.SetActive(false);
			foreach(Transform tra in _mainWithLibs.transform)
			{
				if(tra.name == "MainScene")
				{
					tra.gameObject.SetActive(false);
				}
			}
		}
		resetMenu ();
	}
	public void activateExplorerInGame()
	{

		_explorer = GameObject.Find("Explorer");		
		_mainWithLibs = GameObject.Find("MainWithLibs");
		if(_explorer!=null)
		{
			//	_explorer.SetActive(true);
			foreach(Transform tra in _explorer.transform)
			{
				if(tra.GetComponent<Camera>()!=null)
				{
					tra.GetComponent<Camera>().enabled=true;
					
					tra.GetComponent<ExplorerV2>().explorerInGame();
				}
					
				if(tra.name != "Background")
				{
					tra.gameObject.SetActive(true);
				}
			}
		}
		if(_mainWithLibs!=null)
		{
			foreach(Transform tra in _mainWithLibs.transform)
			{
				if(tra.name != "Background" &&
				   tra.name != "MainNode" &&
				   tra.name != "MainScene")
				   {
				   tra.gameObject.SetActive(false);
				   }
				   
				if(tra.name == "MainScene")
				{
					m_listBckupComponentExplorer.Clear();
					
					MonoBehaviour[] components = GetComponents<MonoBehaviour>();
					
					foreach(MonoBehaviour mb in components)
					{
						if(mb.enabled)
						{
							mb.enabled = false;
							m_listBckupComponentExplorer.Add(mb);
						}
					}
				}
			}
		}
		
		GetComponent<GUIMenuRight>().m_mode2D.enabled = false;
		
		resetMenu ();
	}

	public void quitExplorerInGame()
	{
		_explorer = GameObject.Find("Explorer");		
		_mainWithLibs = GameObject.Find("MainWithLibs");
		if(_explorer!=null)
		{
			//	_explorer.SetActive(true);
			foreach(Transform tra in _explorer.transform)
			{	
				if(tra.GetComponent<Camera>()!=null)
				{
					tra.GetComponent<Camera>().enabled=false;
					tra.GetComponent<ExplorerV2>().quitExplorerInGame();
				}
				
				tra.gameObject.SetActive(false);
			}
		}
		if(_mainWithLibs!=null)
		{
			foreach(Transform tra in _mainWithLibs.transform)
			{
				if(tra.name != "Background" &&
				    tra.name != "MainNode" &&
				    tra.name != "MainScene")
				{
					tra.gameObject.SetActive(true);
				}
			}
			
			foreach(MonoBehaviour mb in m_listBckupComponentExplorer)
			{
				mb.enabled = true;
			}
			
			m_listBckupComponentExplorer.Clear();
		}
		
		GetComponent<GUIMenuRight>().m_mode2D.enabled = true;
		
		resetMenu ();
	}
	private void ui2fcn ()
	{
		switch (m_selM)
		{
			
			case 1: // projet
				projectControl();
				break;
			//Général vv
			case 0: //Dossiers
				//StartCoroutine(LoadLevel(false));
				SwitchToExplorerV2();
				break;
			
			case 9 :
				Quit();
				break;
			
			
			case 10: //PARAMS
				paramsControl();
				break;		
			case 11: //Mise à jour disponible	
		#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_IPHONE
					UnityEngine.Application.OpenURL(usefullData.GetNewLink());
					resetMenu ();
                    InterProcess.Stop();
					#if UNITY_STANDALONE_WIN 
                    System.Diagnostics.Process.GetCurrentProcess().Kill(); 
#else 
                UnityEngine.Application.Quit(); 
#endif
		#endif	
			break;
		}
		//if(m_selM != 4 && m_selM != -1 && m_selM!=6 && m_selM!=9 && m_selM!=10)
		if((m_selM ==1 && m_selSM==6))
		{
			started = false;
			resetMenu ();
		}
			
	}
	
	private void projectControl()
	{
		switch (m_selSM)
		{
			//iPad vv
			#if UNITY_IPHONE && !UNITY_EDITOR
			case 1: // tk photo
				m_bpopUpTakePhoto = true;
				break;
			
			case 2: // load photo
			    _pluginPhotoRef.SetMode(true);
				EtceteraManager.setPrompt(true);
				EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Album );
				resetMenu();
				break;
			#endif
			//PC vv
			case 5: // load photo
				OpenFile();
				resetMenu();
				break;
		   case 6: // phototèque
				imgSelector = true;
				//mBackGrid.GetComponent<BackGridManager>().SetSelectorMode();
				
				break;
			//Android vv
			#if UNITY_ANDROID
			case 3: // tk photo
				string name = "tmp"+System.DateTime.Now.ToString("dd-mm-yyyy_HH-mm-ss");
				GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = true;
				GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(true);			
				//started = false;
				//resetMenu ();
				int height = (int)(UnityEngine.Screen.width *0.75f);
				//EtceteraAndroid.promptToTakePhoto(UnityEngine.Screen.width,height,name);
               // AndroidCommons.TakePicture("/Handler", "TakePictureCallback");
                _pluginPhotoRef.SetMode(false);
                resetMenu();
                setVisibility(false);
               AndroidPlugin.takePicture();	
				break;
			
			case 4: // load photo
				//started = false;
				//resetMenu ();
                	_pluginPhotoRef.SetMode(true);
                    resetMenu();
                   AndroidPlugin.browseGallery();
				//EtceteraAndroid.promptForPictureFromAlbum(UnityEngine.Screen.width,UnityEngine.Screen.height,"TempPhoto");
				break;
            #endif
		}
	}
	private void paramsControl()
	{
		switch (m_selSM)
		{
		case 0: //Désactivation licence
			if(! _cb.isVisible())
			{
				_cb.setBtns(TextManager.GetText("GUIMenuRight.Yes"),
					TextManager.GetText("GUIMenuRight.No"));
				_cb.showMe(true,GUI.depth);
				_cb.setText(TextManager.GetText("GUIMenuRight.DeactivateLic"));
			}
			else
			{
				if(_cb.getConfirmation())
				{
					_cb.showMe(false,GUI.depth);					
					//DO
					StartCoroutine(UsefulFunctions.Deactivate(_cb, false));
					resetMenu();
					//----------------------------
				}
				if(_cb.getCancel())
				{
					_cb.showMe(false,GUI.depth);
					resetMenu();
					setMenuMode(10, -1);
					//DO

					//----------
				}
			}
			break;					
		case 1:
			if(m_selT != -1)
				m_oldSelT = m_selT;
			if(! _cb.isVisible())
			{
				_cb.showMe(true,GUI.depth);
			_cb.setText(TextManager.GetText("GUIStart.ChangeLanguage"));
			_cb.setBtns(TextManager.GetText("GUIMenuRight.Yes"),
				TextManager.GetText("GUIMenuRight.cancel"));
			}
			else
			{
				if(_cb.getConfirmation())
				{
					PlayerPrefs.DeleteKey("language");
					Application.LoadLevel("activation");
					//UsefulFunctions.ChangeLanguage(m_selT);		
					//_cb.showMe(false,GUI.depth);

					//DO
			//		SetAllowQuit(true);
                    InterProcess.Stop();
					#if UNITY_STANDALONE_WIN 
                   // System.Diagnostics.Process.GetCurrentProcess().Kill(); 
#else 
                    //UnityEngine.Application.Quit(); 
#endif
					//----------------------------
				}
				if(_cb.getCancel())
				{
				//	_changeLanguage=false;
					_cb.showMe(false,GUI.depth);
					resetMenu ();
					setMenuMode(10, -1);
					//----------
				}
			}
			break;	
			
		case 2:
			//A propos
			GetComponent<HelpPanel>().switchAbout();
			//enabled = false;
			resetMenu();
			break;
		/*case 3:
			//Activer/Desactiver les aides Pop-up
			if(((GUICheckbox)_btnSwitchAidePopup).GetValue())
				PlayerPrefs.SetInt("HelpPopup", 1);
			else
				PlayerPrefs.SetInt("HelpPopup", 0);
			break;*/
		case 4:
			//Contactez nous
#if UNITY_IPHONE
			EtceteraManager.setPrompt(true);
			EtceteraBinding.showMailComposer( "info@pointcube.fr",  
			                                 "Licence : " + PlayerPrefs.GetString(usefullData.k_logIn), 
			                                 "Veuillez remplir les champs suivant :\nNom :\nNumero de Téléphone :\nNature du problème :",
			                                 false );
#endif	
			resetMenu();
			break;
		case 5:
			//Historique de version
			GetComponent<HelpPanel>().showUpgrade();
			//enabled = false;
			resetMenu();
			break;
#if UNITY_STANDALONE_WIN
		/* Activer/desactiver touch
		case 7 : 
			if(WinTouchInput.GetInstance ().TouchSupported()){
				if(((GUICheckbox)_btnSwitchTouch).GetValue()){
					PC.In=WinTouchInput.GetInstance();
					PlayerPrefs.SetInt("win8TouchEnable",1);
				}
				else{
					PC.In=MouseInput.GetInstance();	
					PlayerPrefs.SetInt("win8TouchEnable",0);
				}
			}
			break;
			*/
#endif			
		}	
	}
	
	private void resetMenu ()
	{
		m_selM = -1;
		m_selSM = -1;
		m_selT = -1;
	/*	if (GetRoot().getSelectedItem () != null) {//menu
			GetRoot().resetSelected ();
		}
		*/
		if(GetRoot().getSelectedItem()!= null)//menu
		{
			if(GetRoot().getSelectedItem().getSelectedItem()!=null)//sous menu
			{
				if(GetRoot().getSelectedItem().getSelectedItem().getSelectedItem() != null)//tool
				{
					GetRoot().getSelectedItem().getSelectedItem().resetSelected();
				}
				GetRoot().getSelectedItem().resetSelected();
			}
			GetRoot().resetSelected();
		}
		//passiveControl();
		
		int[] indexs = new int[3];
		indexs[0] = m_selM;
		indexs[1] = m_selSM;
		indexs[2] = m_selT;
		
		UsefullEvents.FireUpdateUIState(GetType().ToString(),indexs);
	}
	#endregion
	
	public bool OpenFile ()
	{
		string sOpenPath = "";

#if UNITY_EDITOR
		Debug.Log("UNITY_EDITOR : ");
    	sOpenPath = EditorUtility.OpenFilePanel("Open Session","","jpg");
    	
		if(sOpenPath != "")
		{
			Debug.Log("Path0 : "+sOpenPath);
			StartCoroutine (loadStandAloneImg(sOpenPath));

			return true;
		}
		else
		{

			resetMenu ();
			showStart(true);
			return true;
		}
#endif
#if (UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN) && !UNITY_EDITOR
string[] extensions = new string[] {"jpeg", "jpg", "png"};
		sOpenPath = NativePanels.OpenFileDialog(extensions);
	
	if(sOpenPath.Length < 4)
	{
		Debug.Log("open process has ben canceled or has failed " +sOpenPath);
		resetMenu ();
		showStart(true);
		return true;
		return false;
	}
	else
	{
		Debug.Log("file ready to open: " + sOpenPath);
		StartCoroutine (loadStandAloneImg(sOpenPath));
		return true;
	}
#endif
//#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//		StartCoroutine (WindowsExplorateur());
//#endif
		
		return false;
	}
//	#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//	IEnumerator WindowsExplorateur()
//	{
//		//started = true;
//		string sOpenPath = "";
//		
//		System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
//		openFileDialog.InitialDirectory = UnityEngine.Application.dataPath;
//		//openFileDialog.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
//		openFileDialog.Filter = "Image Files|*.jpg;*.png;*.jpeg|All Files|*.*";
//		openFileDialog.FilterIndex = 1;
//		openFileDialog.Title = "Open File";
//		//openFileDialog.RestoreDirectory = false;
//		if(openFileDialog.ShowDialog() == DialogResult.OK)
//		{
//			if ((openFileDialog.FileName!=""))
//	        {
//				sOpenPath = openFileDialog.FileName;
//				StartCoroutine (loadStandAloneImg(sOpenPath));
//	        }
//			else
//			{
//				resetMenu ();
//				showStart(true);
//				//GameObject.Find ("MainScene").GetComponent<GUIStart>().backToClients();
//			}
//		}
//		else
//		{
//			resetMenu ();
//			showStart(true);
//			//GameObject.Find ("MainScene").GetComponent<GUIStart>().backToClients();/
//		}
//		yield return true;
//	}
//	
//#endif

/*	public void backToClients()
	{
		if(isBypassed)
		{
			isBypassed = false;
			StartCoroutine(LoadLevel(false));
		}
		else
		{
			
		}
	}*/
	
	IEnumerator CheckNewVersion()
	{
		WWWForm checkForm = new WWWForm();		
		
		if(!PlayerPrefs.HasKey("Revendeur"))
			checkForm.AddField("config", PlayerPrefs.GetString("Revendeur"));
		else
			checkForm.AddField("config", "0");
		checkForm.AddField("version", usefullData.version_ID);
		checkForm.AddField("test", usefullData.test_mode);
#if UNITY_STANDALONE_WIN 
		WWW sender = new WWW(usefullData.CheckNewVersion,checkForm);
			
		
		yield return sender;
		
		if(sender.error != null)
		{
			Debug.Log("ERROR : "+sender.error);
		}
		else
		{
			if(sender.text.CompareTo("0")==0)
			{
				usefullData.SetHasNewVersion(false);
				Debug.Log("Aucune nouvelle version disponible");
			}
			else
			{
				usefullData.SetHasNewVersion(true);
				Debug.Log("Nouvelle Version disponible"+sender.text);
				string [] lines = sender.text.Split('\n');
				foreach (string line in lines)
				{
					if(line.StartsWith("link="))
					{
						usefullData.SetNewVersionLink(line.Substring(5));
						Debug.Log("Nouveau lien "+line.Substring(5));
					}
				}
			}
		}
#elif UNITY_STANDALONE_OSX
		WWW sender = new WWW(usefullData.CheckNewVersionMac,checkForm);
			
		yield return sender;
		
		if(sender.error != null)
		{
			Debug.Log("ERROR : "+sender.error);
		}
		else
		{
			if(sender.text.CompareTo("0")==0)
			{
				usefullData.SetHasNewVersion(false);
				Debug.Log("Aucune nouvelle version disponible");
			}
			else
			{
				usefullData.SetHasNewVersion(true);
				Debug.Log("Nouvelle Version disponible"+sender.text);
				string [] lines = sender.text.Split('\n');
				foreach (string line in lines)
				{
					if(line.StartsWith("link="))
					{
						usefullData.SetNewVersionLink(line.Substring(5));
						Debug.Log("Nouveau lien "+line.Substring(5));
					}
				}
			}
		}
#elif UNITY_IPHONE
		WWW sender = new WWW(usefullData.CheckNewVersioniOS,checkForm);
			
		yield return sender;
		
		if(sender.error != null)
		{
			Debug.Log("ERROR : "+sender.error);
		}
		else
		{
			if(sender.text.CompareTo("0")==0)
			{
				usefullData.SetHasNewVersion(false);
				Debug.Log("Aucune nouvelle version disponible");
			}
			else
			{
				usefullData.SetHasNewVersion(true);
				Debug.Log("Nouvelle Version disponible"+sender.text);
				string [] lines = sender.text.Split('\n');
				foreach (string line in lines)
				{
					if(line.StartsWith("link="))
					{
						usefullData.SetNewVersionLink(line.Substring(5));
						Debug.Log("Nouveau lien "+line.Substring(5));
					}
				}
			}
		}
#else
		yield return null;
#endif
	}
	
	public void StartGUI () // Appelé 1 fois quand l'appli démarre
	{
		Debug.Log ("Path sauvergardes : "+usefullData.SavePath);
		//StartCoroutine(CheckNewVersion());
		//CREATION DE TOUTE LA GUI
		//Fait ici afin que la création de la gui intervienne apres l'autorisation des modules
		GameObject.Find("MainScene").GetComponent<GUIMenuMain>().CreateAllGUI();
		
		_guiReady = true;

        mBackGrid.GetComponent<BackGridManager>().SetMenuMode();
        //Debug.Log("Set menu mode ok");
//		bg.guiTexture.texture = fondStart;
		started = true;
		
		//Before/After
		m_mainCam.GetComponent<GuiTextureClip>().enabled = false;		
        //Debug.Log("maincam enable ok");
		//DossierClient loading override
		if(PlayerPrefs.HasKey(usefullData.k_toLoadPath) && PlayerPrefs.HasKey(usefullData.k_toLoadParams))
		{
			string path = PlayerPrefs.GetString(usefullData.k_toLoadPath);
			if(path != "")
			{
				started = false;
			}
		}
		if(byPassAction == "exemple")
		{
			started = false;
            //mBackGrid.GetComponent<BackGridManager>().SetSelectorMode();
		}
		else
		{
			isBypassed = false;
		}

        // Désactivation du "loading circle"
        if(mLoadingCircle != null)
        {
            mLoadingCircle.SetActive(false);
            mLoadingCircle.enabled = false;
        }

#if UNITY_STANDALONE_WIN
        //Ouverture du fichier passé en argument
        if (System.Environment.GetCommandLineArgs().Length > 1)
        {
            string cmd = System.Environment.GetCommandLineArgs()[1];
            if (cmd != null)
            {
                if (File.Exists(cmd))
                {
                    GameObject.Find("MainScene").GetComponent<Montage>().LoadFromExplorer(cmd, true);
                    showStart(false);
                }
            }
        }
#endif

        //démarrage serveur de communication entre processus
        /*InterProcess ip=*/
	/*
		string filepath=usefullData.SavePath.Replace("/save/","/");
		Debug.Log("UNITY : lookin for the file "+filepath+"openFile.txt");
		if(File.Exists(filepath+"openFile.txt")){
			Debug.Log("The file exists");
			try{
				string fileToLoad=System.IO.File.ReadAllText(filepath+"openFile.txt");
				PlayerPrefs.SetString("fileToOpen",fileToLoad);
				PlayerPrefs.Save();
				Debug.Log ("playerpref(fileToOpen)= '"+fileToLoad+"'");
				showStart(false);
				File.Delete(filepath+"openFile.txt");
			}
			catch(System.Exception e){
				Debug.Log("Error with the file : "+e.Message);
			}
			
		}
		*/
		GameObject.Find("MainScene").AddComponent<InterProcess>();
        //ip.enabled = true;

	} // StartGUI()
	
	private void LogoUpdate(Texture2D t)
	{
		Debug.Log("[LOGO]>UPDATE FIRED GUISTART");
		_logo = t;
	}
	
	public void CreateGui()
	{
		//Debug.Log("Creation de la GUI START");
		//NEW INTERFACE-------------------------
		//Title = new GUIItemV2(-1,-1,TextManager.GetText("GUIStart.Title"),"title","title",this);
		//Root = new GUIItemV2 (-1,-1, "Root", "", "", this);
		
		
		GUIItemV2 pjt = new GUIItemV2(0,1,TextManager.GetText("GUIStart.NewProject"),"menuProjOn","menuProjOff",this);

#if UNITY_IPHONE && !UNITY_EDITOR
		if(m_authPhotoPerso)
		{
			pjt.addSubItem (new GUIItemV2 (1,1, TextManager.GetText("GUIStart.TakePicture"), "menuOn", "menuOff", this));

			if(usefullData._edition == usefullData.OSedition.Full)
			{
				pjt.addSubItem (new GUIItemV2 (1,2,TextManager.GetText("GUIStart.LoadPicture"), "menuOn", "menuOff", this));
			}
		}
#elif UNITY_ANDROID && !UNITY_EDITOR
		if(m_authPhotoPerso)
		{
			pjt.addSubItem (new GUIItemV2 (1,3, TextManager.GetText("GUIStart.TakePicture"), "menuOn", "menuOff", this));

			if(usefullData._edition == usefullData.OSedition.Full)
			{
				pjt.addSubItem (new GUIItemV2 (1,4,TextManager.GetText("GUIStart.LoadPicture"), "menuOn", "menuOff", this));
			}
		}
#else
		if(m_authPhotoPerso && usefullData._edition == usefullData.OSedition.Full)
		{
			pjt.addSubItem (new GUIItemV2 (1,5, TextManager.GetText("GUIStart.LoadPicture"), "menuOn", "menuOff", this));
		}
#endif
		
		
		_itemPhotoExample = new GUIItemV2 (1,6, TextManager.GetText("GUIStart.ExamplePicture"), "menuOn", "menuOff", this);
		pjt.addSubItem (_itemPhotoExample);
		GetRoot().addSubItem(pjt);

        if(m_authClientsFile)
        {
            GetRoot().addSubItem (new GUIItemV2 (0,0, TextManager.GetText("GUIStart.Dossiers"), "menuFilesOn", "menuFilesOff", this));
        }
		/*
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_IPHONE
		_itemUpdate = new GUIItemV2 (0,6, TextManager.GetText("GUIStart.UpdateDispo"), "alertOn", "alertOff", this);
		GetRoot().addSubItem (_itemUpdate);
#endif*/
		
		
		//menu Parametres
		GUIItemV2 param = new GUIItemV2(0,10,TextManager.GetText("GUIMenuRight.Software"),"menuSoftOn","menuSoftOff",this);
		
		

		
		
		
		
	//menu touch windows
		/*
#if UNITY_STANDALONE_WIN
	if(PC.In.TouchSupported()){
		int touchpref=PlayerPrefs.GetInt("win8TouchEnable",1);
		_btnSwitchTouch = new GUICheckbox(1,7,
			TextManager.GetText("GUIStart.TouchInterface"),"outilOn","outilOff",this);
		if(touchpref==1){
			((GUICheckbox)_btnSwitchTouch).SetValue (true);
			}
		else{
			((GUICheckbox)_btnSwitchTouch).SetValue (false);
			PC.In=MouseInput.GetInstance();
			}
				param.addSubItem(_btnSwitchTouch);
	}

#endif
	*/	
		//SOUS MENU langues
		param.addSubItem( new GUIItemV2(1,5,TextManager.GetText("GUIMenuRight.Historic"),"outilOn","outilOff",this));
		GUIItemV2 rgSubMenuLanguage = new GUIItemV2(1,1,TextManager.GetText("GUIMenuRight.Language"),"outilOn","outilOff",this);//2

		param.addSubItem(rgSubMenuLanguage);
		param.addSubItem( new GUIItemV2(1,2,TextManager.GetText("GUIMenuRight.apropos"),"outilOn","outilOff",this));
		
		/*_btnSwitchAidePopup = new GUICheckbox(1,3,
		                                     TextManager.GetText("GUIStart.HelpPopup"),"outilOn","outilOff",this);
		((GUICheckbox)_btnSwitchAidePopup).SetValue (true);
		if(PlayerPrefs.HasKey("HelpPopup"))
			if (PlayerPrefs.GetInt("HelpPopup")==0)
				((GUICheckbox)_btnSwitchAidePopup).SetValue (false);
		param.addSubItem(_btnSwitchAidePopup);*/
		
		#if UNITY_IPHONE
		param.addSubItem( new GUIItemV2(1,4,TextManager.GetText("GUIMenuRight.ContactUs"),"outilOn","outilOff",this));
		#endif

		
		param.addSubItem( new GUIItemV2(1,0,TextManager.GetText("GUIMenuRight.Deactivate"),"outilOn","outilOff",this));
		
		GetRoot().addSubItem (param);
		
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_IPHONE
		_itemUpdate = new GUIItemV2 (0,11, TextManager.GetText("GUIStart.UpdateDispo"), "alertOff", "alertOff", this);
		GetRoot().addSubItem (_itemUpdate);
#endif
		
        _itemQuit = new GUIItemV2(0, 9, TextManager.GetText("GUIStart.Exit"), "menuQuitOn", "menuQuitOff", this);
        GetRoot().addSubItem(_itemQuit);
	}
	
	private GUIItemV2 GetRoot()
	{
		if( Root==null)
		{
			Root = new GUIItemV2(-1,-1,"Root","","",this);
		}
		return Root;
			
		
	}
	
	private void AuthorizePlugin(string name,bool auth)
	{
		if(name == "photo_perso")
		{
			m_authPhotoPerso = auth;
		}
		if(name == "dossier_client")
		{
			m_authClientsFile = auth;
		}
	}
	
	public bool IsGUIReady()
	{
		return _guiReady;
	}
	
	public void SetState(int selection)
	{
	//	setVisibility(true);
		m_selM = selection;
		Root.setSelected(m_selM);
	}
	
	void OnActive()
	{
		print("coucou");
	}
}

