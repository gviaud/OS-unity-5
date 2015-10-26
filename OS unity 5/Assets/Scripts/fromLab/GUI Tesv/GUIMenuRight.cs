using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Pointcube.Global;

public class GUIMenuRight : MonoBehaviour,GUIInterface
{
	#region attributs
	//public
	public GUISkin skin;
	public Texture textureBackGround;
	public Texture textureBackGroundRightButton;
	public Texture backgroundPanelControlEchelle;

    public Mode2D  m_mode2D;
    private ObjInteraction m_objInter;

	private Vector3 OldAvatarPostion;
	private Transform avatarTransform;

    public string  m_ctxPanelID_1; // ctx1_scale
    public string  m_ctxPanelID_2; // ctx1_height
    public string  m_ctxPanelID_3; // ctx1_tilt
    public string  m_ctxPanelID_4; // ctx1_persp
    public string  m_ctxPanelID_5; // ctx1_rota
    public string  m_ctxPanelID_6; // ctx1_roll
	public string  m_ctxPanelID_7; // ctx1_atmos
	public string  m_ctxPanelID_8; // ctx1_ppAdd
	public string  m_ctxPanelID_9; // ctx1_ppSub
	public string  m_ctxPanelID_10; // ctx1_texAdd
	public string  m_ctxPanelID_11; // ctx1_texSub

    private bool   m_gridScaling;
    private bool   m_gridHeighting;
    private bool   m_gridTilting;
    private bool   m_gridPerspecting;
    private bool   m_gridRotating;
    private bool   m_gridRolling;
    private int    m_atmosphing;
	private bool 	isNewLoad = false;
	private bool   wantToSave = false; //goto accueil check
	//private
	GUIItemV2 Root;

	//GUIItemV2 Title;
	public GUIItemV2 activeItem;
	GUIItemV2 _avatarItem;
	GUIItemV2 _avatarDisplay;
	GUIItemV2 _3DItem;
	GUIItemV2 _inclinaisonItem;
	GUIItemV2 m_gazonMaterial;
	
	GUIItemV2 _perspectiveItem;
	GUIItemV2 _roulisItem;
	GUIItemV2 _hourItem;
	GUIItemV2 _orientationItem;
	GUIItemV2 _intensityItem;
	GUIItemV2 _gommeItem;
	GUIItemV2 _gazonItem;
	GUIItemV2 _gommeAddItem;
	GUIItemV2 _gommeEraseItem;
	GUIItemV2 _gazonAddItem;
	GUIItemV2 _gazonEraseItem;
	GUIItemV2 _rotationItem;
	
	ArrayList othersMenu;
	
	//Before/After
	// ipo > isPhotoOnly
//	bool ipo = false; // true->photo only, false-> photo + 3d
//	float ipoDist = 0;
//	float ipoLimit = 500;
	int c_camCull;
//	private bool wasOpen = false;
//	private bool toggle = false;
//	enum s2hState 
//	{
//		none,halfShow,show,hide	
//	}
//	
//	s2hState s2h = s2hState.none;
//	
//	Rect s2hRect;
//	string s2hTxt = "";
//	string s2hShow = "vv Gliser pour afficher vv";
//	string s2hHide = "vv Gliser pour cacher   vv";
//	string s2hRelease = "Relacher";
//	int s2hPos = -200;
//	float s2hTmpPos = -200;
	

	//GUI
//	float tgtPos;
//	float tgtHeight;
	
	int m_selM = -1;
	int m_selSM = -1;
	int m_selT = -1;
	
	int m_bkupSelM = -1;        // Menu principal
	int m_bkupSelSM = -1;       // Selection Sous-menu
	int m_bkupSelT = -1;        // Sous-sous-menu
	
	private const int _mMenu		=	0;	
	private const int _mSubMenu		=	1;
	private const int _mTool		=	2;
	
	private const int _mConfGrnd	=	0;
	private const int _mAtmosphr	=	1;
	private const int _mUpgrade		=	2;
	private const int _mBgImg		=	4;
	private const int _mProject		=	3;
	private const int _mPlugins		=	5;
	private const int _mAssembly	=	6;
	private const int _mStartMenu	=	7;

    private int m_oldSelSM = 0;
    private int m_oldSelT = 0;
	
	private Rect menuGroup;
	
	//shortcuts
	SceneControl sc;
	LightConfiguration lc;
	ObjInteraction interacteur;
	public GameObject grassNode;
	public GameObject gommeNode;
	
	private GameObject _camPivot;
	private GameObject _lightEffects;
	private Transform _lightEffectsParentNode;
	
	//Guibutton hide/show
	GUIItemV2 _guiItemv2Night;
	GUIItemV2 _guiItemv2Day;
	
	//UI
	public bool showMenu = false;
	bool tempShow = true;
	bool canDisp = false;
	bool configScene =false;
	bool configSceneLight =false;
	bool canHideMenu = false;
	bool _firstTime=false;
    bool _canValidate = true;
	bool _isSliding = false;// Anti slide and select
	
	private bool m_authClientsFile;
	private bool m_authSaveSendImg;
	private bool m_authUpgrade;
	
	private bool m_avatarFirstDisplay = false;

	private float   m_off7;
	
	// Pour debug touchpad
	int fac = 1000;
	int fac2 = 100;
	int c_cap = 1000;
	int c_cap2 = 1000;
//	Queue<Vector2> touchOldPos;
//	int   touchCount; // Nombre d'iterations depuis la dernieÃ¨re maj de touchStartPos
//	int it = 10;
//	string dbg = "rien";
	bool lastSens = true;
	
	//slideDetect pour slide les menus vrtclmnt
	float sldDtct=0;
	private float sldDtctThreshold = 20;
	Vector2 scrollpos = new Vector2(0,0);//vertical scroll menu trop grand
	
	
	// PC detection de fin d'action (pour savoir quand creer un "point d'undo")
	float endActionTimer;
	float endActionDelta = 0.5f;
	bool detectAction = false;

	float factorInclinaison = 52;
	float factorRoulis = 100;
	float factorScale = 250;
	float factorRotation = 30;//52;
	float factorHeight = 52;
	float factorPers = 52;
	float factorLightHour = 52;
	float factorLightAzimut = 52;
	float factorLightIntensity = 6000;
	float factorShadowBlur = 90;
	float factorShadowIntensity = 6000;
	float factorReflexionIntensity = 6000;
	
	//------------------------------------
//	Dictionary<string,int> m_lastUsedTool = new Dictionary<string, int>() // a lair inutilisé ...
//	{
//		{"grille",0},
//		{"light",0}
//	};
	
	// touchPad (pour savoir quand creer un "point d'undo")
	private bool m_slideTouchEnded;
	
	//Confirmation box
	GUIDialogBox _cb;
	//AsyncOperation async;
	
	//PLUGINS (ExtraBat, ...)
	ArrayList _pluginsOS3D;
		
	string[] _listlanguage;
	
	bool _changeLanguage = false;
	
	private Avatar _avatarControl; 
	private bool m_bavatar;
	
	// Mise à jour
	private GUIItemV2 _itemUpdate;
	
	private bool _allowToQuit = false;
	private int _showWarningQuit = 0; //0 : false, 1 : back to start, 2 : real quit;
	private bool _uiLocked = false;
	
	private static readonly string DEBUGTAG = "GUIMenuRight : ";
	
	// variable pour ne pas faire des appels infinies au navigateur 
	// lors d'un clic pour mettre à jour l'application
	private bool _linkOpen = false;
	
	private bool _makeInclinaisonByDefault = false;
	
	private GameObject _grid;
	private GameObject _mainNode;
	
	private PluginPhotoManagerGUI _pluginPhotoRef;

	private	ArrayList	_childrenNodes;
	private	bool		_detachChildren = false;
	
	private	bool		m_bsubtoolChoiceMaterial = false;

	public int MaskOrGrass = -1;

	#endregion
	
//Fonctions unity---------------------------____
	#region uintyFcns
	void Awake()
	{
		// -- Enregistrement des événements à gérer --
		ModulesAuthorization.moduleAuth += AuthorizePlugin;
		UsefullEvents.OnResizingWindow  += RelocatePanel;
        UsefullEvents.OnResizeWindowEnd += RelocatePanel;
		UsefullEvents.LockGuiDialogBox  += LockUI;
		UsefullEvents.UpdateUIState 	+= PassiveControl;

        m_gridScaling     = false;
        m_gridHeighting   = false;
        m_gridTilting     = false;
        m_gridPerspecting = false;
        m_gridRotating    = false;
        m_gridRolling     = false;
        m_atmosphing      = 0;
	}
	
	//-----------------------------------------------------
	void Start () 
	{
		m_objInter=GameObject.Find("mainCam").GetComponent<ObjInteraction>();
		if (grassNode==null) Debug.LogError(DEBUGTAG+"grassNode"+PC.MISSING_REF);
		if (gommeNode==null) Debug.LogError(DEBUGTAG+"gommeNode"+PC.MISSING_REF);
        if (m_mode2D == null) Debug.LogError(DEBUGTAG+"Mode2D"+PC.MISSING_REF);

        m_off7 = Screen.width;

		m_slideTouchEnded = false;
//		touchOldPos = new Queue<Vector2>();
//		touchCount = 0;
		
		othersMenu = new ArrayList();
		foreach(Component cp in this.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType() != this.GetType() && cp.GetType().GetInterface("GUIInterface")!= null)
			{
				othersMenu.Add(cp);
			}
		}
		
		//shortcuts
		_camPivot = GameObject.Find("camPivot");
		if(_camPivot!=null)
			sc = _camPivot.GetComponent<SceneControl>();
		
		GameObject lightPivot = GameObject.Find("LightPivot");
		if(lightPivot!=null)
		{
			lc = lightPivot.GetComponent<LightConfiguration>();
		}
		
		_lightEffects = GameObject.Find("lightEffects");
		if(_lightEffects!=null)
		{
			_lightEffectsParentNode = _lightEffects.transform.parent;
		}
		interacteur = Camera.main.GetComponent<ObjInteraction>();
		_grid = GameObject.Find("grid");
		_mainNode = GameObject.Find("MainNode");
		_childrenNodes = new ArrayList();
		
//		//CREATION DE LA GUI
//		CreateGui();
		
		//Before/After
		c_camCull = Camera.main.cullingMask;
//		s2hRect = new Rect(Screen.width/2-256,-200,512,200);		
		
		//ConfirmationBox
		_cb = GetComponent<GUIDialogBox>();
		
		initLanguages();

		if(GameObject.Find("_avatar"))
		{
			_avatarControl = GameObject.Find("_avatar").GetComponent<Avatar>();
			avatarTransform =  GameObject.Find("_avatar").transform;
		}
		_pluginPhotoRef = GetComponent<PluginPhotoManagerGUI>();


	} // Start()
	
	//-----------------------------------------------------
	void initLanguages()
	{
		_listlanguage 		= new string[2];
		_listlanguage[0]	=  "Français";
		_listlanguage[1]	=  "English";
	}
	
	//-----------------------------------------------------
	void Update()
	{

		//_3DItem.setSelected(0);
		if(Root != null && Root.getSelectedItem() == _3DItem)
		{
			if(!_makeInclinaisonByDefault)
			{
				_3DItem.setSelected(6);
				_makeInclinaisonByDefault = true;
				m_selSM = 6;
				
				//GetComponent<GUIMenuMain>().hideShowUD(true);
			}
		}
		else if(_makeInclinaisonByDefault)
		{
			_makeInclinaisonByDefault = false;
		}
		
		if((m_objInter.getSelected()==null || m_objInter.getSelected().name!="_avatar") && m_selM!=_mConfGrnd)
		{
			if(!Avatar.Locked)
			{
				avatarForceDisplay(false);
				Avatar.Locked=true;
				SetGridParametringFalse();
			}
			else
			{
				if(m_selM!=_mConfGrnd && _avatarControl)
				{
					avatarForceDisplay(false);
				}
			}
		}
		
        if(PC.ctxHlp.PanelBlockingInputs())
            return;

		if((_guiItemv2Night!=null)&&(_guiItemv2Day!=null))
		{
			if(Montage.sm.IsNightMode())
			{
				if(_guiItemv2Night.IsEnableUI())
				{
					_guiItemv2Night.SetEnableUI(false);
					_guiItemv2Day.SetEnableUI(true);
				}	
				
			}
			else
			{
				if(_guiItemv2Day.IsEnableUI())
				{
					_guiItemv2Night.SetEnableUI(true);
					_guiItemv2Day.SetEnableUI(false);
				}		
			}
		}

        if(PC.In.Click1Down())
            _canValidate = true;    // Quick-Fix (souris only) pour pas qu'un clic soit ignoré après un scroll molette
		
		if(m_selM == 0 && m_selSM == 6)
		{
			showMenu = true;
		}
		
		if(showMenu)//version pc/mac standalone
		{
			ui2fcn();
			if(!isOnUI() && !GetComponent<GUIMenuLeft>().isOnUI() && PC.In.Click1Up() && !_cb.isVisible())
			{
                if(_canValidate)
                {
				    showMenu = false;

					if( OldAvatarPostion == avatarTransform.position)
							Avatar.Locked = true;

					OldAvatarPostion = avatarTransform.position;

					//if(Avatar.Locked/* && !GetComponent<GUISubTools>().isActive() && !m_bsubtoolChoiceMaterial*/)
					if( Avatar.Locked )
					{
						_grid.GetComponent<Renderer>().enabled = false;
						SetGridParametringFalse();
	            		m_atmosphing = 0;
						resetMenu();
						avatarForceDisplay(false);
					}
                }
                else
                {
                    tempShow = true;
                    _canValidate = true;
                }
			}
		}
		else
		{
			GetComponent<GUIMenuMain>().hideShowUD(false);
		}

        // Slide menu
        float deltaScroll;
        if(PC.In.CursorOnUI(menuGroup) && PC.In.ScrollViewV(out deltaScroll))
        {
            scrollpos.y += deltaScroll;
        }
		
		if((m_selM ==0 || m_selM ==1)&& detectAction)
		{
			if(Time.time > endActionTimer+endActionDelta)
			{
				detectAction = false;
				tempShow = true;
				if(m_selM == 0)
					sc.saveToModel();
				if(m_selM == 1)
				{
					if(m_selSM == 0)
						lc.SaveToModel(true);
					if(m_selSM == 1)
						lc.SaveToModel(false);
					if(m_selSM ==2)
						lc.saveReflexToModel();
				}
			}
		}
		if(_showWarningQuit==1)
		{
			ShowWarningQuit(false);
		}
        if (_showWarningQuit == 2)
        {
            ShowWarningQuit(true);
        }
        
        if(GetComponent<GUIMatPicker>().enabled)
        {
			m_bsubtoolChoiceMaterial = false;
        }
		if(m_bsubtoolChoiceMaterial && !GetComponent<GUIMatPicker>().enabled)
		{
			GetComponent<GUISubTools>().DisplayChoicePanel();
		}

		/*if(isNewLoad && canDisp)
		{
			m_bkupSelM = 0;
			m_bkupSelSM =0;
			m_bkupSelT =-1;
			showMenu =true; 
			setMenuState();
			isNewLoad =false;
		}*/

//#endif
#if UNITY_ANDROID && !UNITY_EDITOR
			_grid.renderer.enabled=_grid.renderer.enabled;
#endif
	
	} // Update()

    //-----------------------------------------------------
    void FixedUpdate()
    {
        panelAnimation();
        menuGroup.x = m_off7;
    }

	//-----------------------------------------------------
	void OnGUI()
	{

        //back button
        #if UNITY_ANDROID
            if (Input.GetKey(KeyCode.Escape))
            {
                GUIStart mr = GameObject.Find("MainScene").GetComponent<GUIStart>();
                if (!mr.Started && !mr.ImgSelector)
                    _showWarningQuit = 1;

            }
        #endif
        

		GUISkin bkup = GUI.skin;
		GUI.skin = skin;

		if(showMenu)
			GUI.DrawTexture(new Rect ( m_off7+30 +0* textureBackGround.width+50, Screen.height - textureBackGround.height,textureBackGround.width-50,textureBackGround.height),textureBackGround);
    
//		GUI.Box(menuGroup,"MENURIGHT");
		
		//MENU
		if((showMenu || m_off7<Screen.width) && canDisp)
		{
			GUILayout.BeginArea(new Rect(m_off7+30+50,0, 210, Screen.height+100));


			scrollpos = GUILayout.BeginScrollView(scrollpos,"empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
			//GUILayout.Box("","bgFadeUp",GUILayout.Width(210),GUILayout.Height(50));//fade en haut
			GUILayout.BeginVertical("bgFadeMid",GUILayout.Width(210));

			//GUI.DrawTexture(new Rect ( Screen.width - textureBackGround.width, Screen.height - textureBackGround.height,textureBackGround.width,textureBackGround.height),textureBackGround);
			//Title.getUI(false);0
			GUILayout.FlexibleSpace();
			GetRoot().showSubItms();//Menu
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();

			//GUILayout.Box("","bgFadeDw",GUILayout.Width(210),GUILayout.Height(100));//fade en bas

			GUILayout.EndScrollView();

		

		    GUILayout.EndArea();

			//GUI.DrawTexture(new Rect ( Screen.width - textureBackGround.width, Screen.height - textureBackGround.height,textureBackGround.width,textureBackGround.height),textureBackGround);

		}
		
		//Menu Toggle
		if(showMenu)
		{

			//BTN SHOW HIDE MENU
			bool tmpShw = GUI.Toggle(new Rect(m_off7+290,Screen.height/2-350/2,30,350),showMenu,"","halfMenuToggle");
			if(showMenu != tmpShw && !_uiLocked)
			{
				showMenu = tmpShw;
				
				print ("BTN SHOW HIDE MENU");
				resetMenu();
			}
		}
		else
		{
			if(canDisp)
			{
				if (!Camera.main.GetComponent<Mode2D>().IsActive())
				{
					//BTN SHOW HIDE MENU
					GUI.DrawTexture(new Rect (Screen.width - textureBackGroundRightButton.width+25, 0,textureBackGroundRightButton.width-25, textureBackGroundRightButton.height ), textureBackGroundRightButton);
					showMenu = GUI.Toggle(new Rect(Screen.width-55,Screen.height/2-50,100,100),showMenu,"","menuToggle");
					if(showMenu)
					{
						if(Avatar.Locked)
						{
							interacteur.validateObj();
							//setMenuState();
							hideOthers();
						}
					}
				}
			}
		}	
		if(sc.fscaleChange > 0.0f && m_selM != 0)
		{
			//print (m_selM + "   " + m_selSM);
			float val = /*m_objInter.fechelle;*/ sc.GetS();
			string s = ((val*10)/10).ToString("0.00");

			GUI.DrawTexture(new Rect(Screen.width/2-180, 0, 360, 100),backgroundPanelControlEchelle);
			GUI.Label(new Rect(Screen.width/2-300,0,600,50),TextManager.GetText("GUIMenuRight.GeneralEchelle")+" "+s + " :1","hud3");
		}
		
		GUI.skin = bkup;
		
	} // OnGUI()
	
	//-----------------------------------------------------
	void OnDestroy()
	{
		ModulesAuthorization.moduleAuth -= AuthorizePlugin;
		UsefullEvents.OnResizingWindow  -= RelocatePanel;
        UsefullEvents.OnResizeWindowEnd -= RelocatePanel;
		UsefullEvents.LockGuiDialogBox  -= LockUI;
		UsefullEvents.UpdateUIState 	-= PassiveControl;
	}
	
	#endregion
//FCN. AUX. PRIVEE--------------------
#region privee
	
	//-----------------------------------------------------
	public float slidedetect(bool exponential, float cap)//Tactile
	{
        m_slideTouchEnded = PC.In.Click1Up();

        float deltaMove;
        if(!isOnUI() && PC.In.ScrollHV(out deltaMove))
        {
            _canValidate = false;

            deltaMove *= 75;

            if(exponential)
                deltaMove = (deltaMove<0? -1 : 1)*deltaMove*deltaMove;

            if(deltaMove > cap)       deltaMove = cap;
            else if(deltaMove < -cap) deltaMove = -cap;

            //tempShow = false;
            detectAction = true;
            endActionTimer = Time.time;
            return deltaMove;
        }
        else
        {
//            tempShow = true;
            return 0;
        }
        
	}
	public float slidedetectForCompass(bool exponential, float cap)//Tactile
	{
		m_slideTouchEnded = PC.In.Click1Up();
		
		float deltaMove;
		if(!isOnUI() && PC.In.ScrollHV(out deltaMove))
		{
			_canValidate = false;
			
			deltaMove *= 150;
			
			if(exponential)
				deltaMove = (deltaMove<0? -1 : 1)*deltaMove*deltaMove;
			
			if(deltaMove > cap)       deltaMove = cap;
			else if(deltaMove < -cap) deltaMove = -cap;
			
			//tempShow = false;
			detectAction = true;
			endActionTimer = Time.time;
			return deltaMove;
		}
		else
		{
			//            tempShow = true;
			return 0;
		}
		
	}
	// slidedetect()
	
	//-----------------------------------------------------
	private void hideOthers()
	{
		foreach(Component cp in othersMenu)
		{
			((GUIInterface)cp).setVisibility(false);
		}
	}
	
	//-----------------------------------------------------
	private void panelAnimation()
	{
		if(showMenu && tempShow)
		{
			float limit = Screen.width-290;
			
			if(m_off7>limit)
			{
				if(m_off7<limit-1)
				{
					m_off7 = limit;
					menuGroup.x = m_off7;
				}
				else
				{
					m_off7 = Mathf.Lerp(m_off7,limit,5*Time.deltaTime);
					menuGroup.x = m_off7;
				}
			} // if(off7 > limit)
		}
		else
		{
			if(m_off7<Screen.width)
			{
				if(m_off7>(Screen.width-1))
				{
					m_off7 = Screen.width;
					menuGroup.x = m_off7;
//					resetMenu();
				}
				else
				{
					m_off7 = Mathf.Lerp(m_off7,Screen.width,5*Time.deltaTime);
					menuGroup.x = m_off7;
				}
			} // if(off < screen width)
		}
	} // panelAnimation()
	
	//-----------------------------------------------------
	private void RelocatePanel()
	{
        menuGroup.height = Screen.height;

        if(showMenu)
            m_off7 = Screen.width - 290; // (cf. panel animation limit)
        else
            m_off7 = Screen.width;
	}
	
	//-----------------------------------------------------
	public void setMenuState()
	{
	
		m_selM = m_bkupSelM;
		m_selSM = m_bkupSelSM;
		m_selT = m_bkupSelT;
	
		if(GetRoot().getSubItemsCount()>0 && m_selM != -1)
		{
			GetRoot().setSelected(m_selM);
			if(GetRoot().getSelectedItem().getSubItemsCount()>0 && m_selSM != -1)
			{
				GetRoot().getSelectedItem().setSelected(m_selSM);
				if(GetRoot().getSelectedItem().getSelectedItem()!=null)
				{	
					if(GetRoot().getSelectedItem().getSelectedItem().getSubItemsCount()>0 && m_selT != -1)
					{
						GetRoot().getSelectedItem().getSelectedItem().setSelected(m_selT);
					}
				}
			}
		}
//		passiveControl();
		int[] indexs = new int[3];
		indexs[0] = m_selM;
		indexs[1] = m_selSM;
		indexs[2] = m_selT;
		UsefullEvents.FireUpdateUIState(GetType().ToString(),indexs);
	}
	
	private void resetMenu()
	{
		m_selM = -1;
		m_selSM = -1;
		m_selT = -1;
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
//		passiveControl();
		
		int[] indexs = new int[3];
		indexs[0] = m_selM;
		indexs[1] = m_selSM;
		indexs[2] = m_selT;
		UsefullEvents.FireUpdateUIState(GetType().ToString(),indexs);
		
		_3DItem.setSelected(0);
	}
	
	
	private void AuthorizePlugin(string name,bool auth)
	{
		if(name == "dossier_client")
		{
			m_authClientsFile = auth;
		}
		if(name == "amelioration")
		{
			m_authUpgrade = auth;
			
//#if UNITY_IPHONE		
//			if(Application.platform == RuntimePlatform.IPhonePlayer && iPhone.generation == iPhoneGeneration.iPad2Gen)
//				m_authUpgrade = false;
//#endif
/*#if UNITY_ANDROID		
			if(Application.platform == RuntimePlatform.Android)
				m_authUpgrade = false;
#endif*/
		}
		if(name == "enregistrer_envoyer_image")
		{
			m_authSaveSendImg = auth;
		}
	}
		
	private void LockUI(bool isLck)
	{
		_uiLocked = isLck;
	}
	
#region controleurs
	
	private void ui2fcn()
	{
		if(m_selM != 0)
		{
			m_avatarFirstDisplay = false;
		}
		
		switch (m_selM)
		{
		case _mConfGrnd:
			if(!Camera.main.GetComponent<Mode2D>().IsActive()/* || m_selSM == 6*/)//m_selSM == 6 > show/hide Avatar
			{
				configScene = true;
				
				/*if(m_objInter.getSelected() == null)
				{*/
					gridControl();
				//}
				
				if(!m_avatarFirstDisplay)
				{
					avatarForceDisplay(true);
					
					m_objInter.setSelected(_avatarControl.gameObject);
					
					m_avatarFirstDisplay = true;
					
					GetComponent<GUIMenuMain>().setUDText("", "");
				}
			}
			break;
		case _mAtmosphr:
			configSceneLight = true;
            SetGridParametringFalse();
			lightNreflexControl();
			break;
		case _mUpgrade:
            SetGridParametringFalse();
            m_atmosphing = 0;
			upgradeControl();
			break;
		case _mBgImg:
            SetGridParametringFalse();
            m_atmosphing = 0;
			BGImageControl();
			break;
		case _mProject:
            SetGridParametringFalse();
            m_atmosphing = 0;
			projectControl();
			break;
		case _mPlugins:
            SetGridParametringFalse();
            m_atmosphing = 0;
			pluginsControl();
			break;
		case _mAssembly:
            SetGridParametringFalse();
            m_atmosphing = 0;
			AssemblyControl();
			break;
		case _mStartMenu:
            SetGridParametringFalse();
            m_atmosphing = 0;
//			gotoAccueil();
			ShowWarningQuit(false);
			break;
		}
	}

    private void SetGridParametringFalse()
    {
		if(m_selM!=_mConfGrnd && _avatarControl)
				//_avatarControl.SetForceDisplay(false);
		if(!Avatar.Locked){
			Avatar.Locked=true;
			
			//_avatarItem.chgTxt(TextManager.GetText("GUIMenuRight.AvatarShow"));
		/*
			GetRoot().getSelectedItem().setSelected(6);
		GetRoot().getSelectedItem().getSelectedItem().chgTxt(TextManager.GetText("GUIMenuRight.AvatarHide"));
		*/
			
		}
        m_gridScaling = false;
        m_gridHeighting = false;
        m_gridRolling = false;
        m_gridPerspecting = false;
        m_gridTilting = false;
        m_gridRotating = false;

    }

	private void ShowWarningQuit(bool realQuit = true)
	{
		if(GetComponent<Montage>().saveState == Montage.states.saved)
		{
			StartCoroutine(gotoAccueil());
		}else
		{
			if(! _cb.isVisible())
			{
				_cb.Show3BtnBox(true,GUI.depth);
				_cb.setText(TextManager.GetText("GUIMenuRight.QuitWithoutSave"));
				_cb.setBtns(TextManager.GetText("GUIMenuRight.No"),
					TextManager.GetText("GUIMenuRight.cancel"),
					TextManager.GetText("GUIMenuRight.Yes"));
			}
			else
			{
				wantToSave =false;
				if(_cb.getConfirmation())
				{
					_cb.Show3BtnBox(false,GUI.depth);

	                if (!realQuit)
	                {
	                    _showWarningQuit = 0;
						StartCoroutine(gotoAccueil());
					}
					else
	                {
	                    _allowToQuit = true;
	                    _showWarningQuit = 0;
	                    resetMenu();
	                    //				yield return null;
	                    InterProcess.Stop();
	                    #if UNITY_STANDALONE_WIN 
	                    System.Diagnostics.Process.GetCurrentProcess().Kill(); 
	#else 
	                    UnityEngine.Application.Quit(); 
	#endif
	                }
				}
				if(_cb.getCancel())
				{
					_cb.Show3BtnBox(false,GUI.depth);
					resetMenu();
					_allowToQuit = false;
					_showWarningQuit = 0;
					if(realQuit)
						Application.CancelQuit();
				}
				if(_cb.get3rdChoice())
				{

					_cb.Show3BtnBox(false,GUI.depth);
					//GetComponent<Montage>().showSaveAndAccueil();
					GetComponent<GUIStart>().activateExplorerInGame();
					GetComponent<GUIMenuLeft>().canDisplay(false);
					canDisplay(false);
					StartCoroutine(gotoAccueil());


				}
			}
		}
    }
	
	public void OnApplicationQuit() 
	{
		if (!_allowToQuit && GetComponent<LibraryLoader>() == null)
		{
			if (!GetComponent<GUIStart>().isActive())
			{
	            Application.CancelQuit();
				_showWarningQuit = 2;			
			}
		}
			  
    }
	public void SetAllowQuit(bool quitOK)
	{
		_allowToQuit = quitOK;
	}
	public bool AllowQuit()
	{
		return _allowToQuit;
	}
	private void DetachMainNodeChildren()
	{
		_detachChildren = true;
		if(_mainNode!=null && _childrenNodes!=null && _camPivot!=null)
		{
			foreach (Transform transformChild in _mainNode.transform)
			{
				_childrenNodes.Add(transformChild);
			}
			foreach (Transform transformChild in _childrenNodes)
			{
				transformChild.parent = _camPivot.transform;
			}
		}
		if(_lightEffectsParentNode!=null && _lightEffects!=null)
		{
			_lightEffects.transform.parent = _camPivot.transform;
		}
	}
	private void AttachMainNodeChildren()
	{
		_detachChildren = false;
		if(_mainNode!=null && _childrenNodes!=null && _camPivot!=null)
		{
			foreach (Transform transformChild in _childrenNodes)
			{
				transformChild.parent = _mainNode.transform;
			}
			_childrenNodes.Clear();
		}		
		if(_lightEffectsParentNode!=null && _lightEffects!=null)
		{
			_lightEffects.transform.parent = _lightEffectsParentNode;
		}
	
	}
	//bc
	private void gridControl()
	{
		string s = "";
		float val = 0;
		
		if(m_selSM < 3 && m_selSM!=-1)
			m_oldSelSM = m_selSM;

		if(_inclinaisonItem.isSecondButtonClicked())
		{
			PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_3);
			PC.ctxHlp.ShowHelp();
		}
		if(_perspectiveItem.isSecondButtonClicked())
		{
			PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_4);
			PC.ctxHlp.ShowHelp();
		}
		if(_roulisItem.isSecondButtonClicked())
		{
			PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_6);
			PC.ctxHlp.ShowHelp();
		}
		if(_rotationItem.isSecondButtonClicked())
		{
			PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_5);
			PC.ctxHlp.ShowHelp();
		}
		if(_avatarDisplay.isSecondButtonClicked())
		{
			if(PlayerPrefs.GetString("language") == "fr")
			{
				PC.ctxHlp.ShowCtxHelp("ctx1_scale_fr");
			}
			else
			{
				PC.ctxHlp.ShowCtxHelp("ctx1_scale_en");
			}
				
			PC.ctxHlp.ShowHelp();
		}
		
		float deltaRota;
		float deltaScale;
		float deltaHeight;
		GameObject camPivot = GameObject.Find("camPivot");
		switch(m_selSM)
		{
		case 0: //INCLINAISON

			avatarForceDisplay(false);
            m_atmosphing = 0;
            if(!m_gridTilting)  //-- Afficher l'aide contextuelle --
            {
                SetGridParametringFalse();
                m_gridTilting = true;
            }
			
			if(_detachChildren)
				AttachMainNodeChildren();
				
            if(!isOnUI())  // Quick-fix biossun molette souris hover menu -> scroll menu, fonction active sinon
            {

				float ftemp = slidedetect(true, 1000);
				
				/*if(ftemp != 0.0f)
				{
					setVisibility(false);
					canDisplay(true);*/
					sc.setI(ftemp/factorInclinaison);
				/*}
				else
				{
					setVisibility(true);
					canDisplay(false);
				}*/
				
				//print (ftemp);
				/*val =*/ 
    			//s = val.ToString("0.00");
    			//GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Inclinaison") ,s + " "+TextManager.GetText("GUIMenuRight.degres"));
            }
            
			val = sc.GetI();
			s = val.ToString("0.00");
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Inclinaison") ,s + " "+TextManager.GetText("GUIMenuRight.degres"));
			
			/*
			if(_mainNode.transform.GetChildCount() == 1)
				_avatarControl.SetAutoDisplay(true);
				*/
			break;
			
		/*case 1://ECHELLE
            m_atmosphing = 0;
            if(!m_gridScaling)  // -- Afficher l'aide contextuelle --
            {
                SetGridParametringFalse();
                m_gridScaling = true;
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_1);
			}
			
			PC.ctxHlp.m_ftimerButton = 0.1f;
			
			if(_detachChildren)
				AttachMainNodeChildren();
            if(!isOnUI())           // Quick-fix biossun molette souris
            {
                float delta = slidedetect(false, 750);
                // -- Notify Mode2D to display warning if the user opens it back --
                if(delta != 0 && !m_mode2D.IsActive() && m_mode2D.GetObjCount() > 0 &&
                    m_mode2D.IsPlanLoaded() && !m_mode2D.ShowingMovedObjectsWarning())
                    m_mode2D.ShowMovedObjectsWarning();
                
				//m_objInter.ChangeScaleGeneral(delta/factorScale);
    			sc.setS(delta/factorScale);
    		//	s = ((val*10)/10).ToString("0.00");
    		//	GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Echelle") , s + " :1");
            }
            
            
			val = sc.GetS();
    		s = ((val*10)/10).ToString("0.00");
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.GeneralEchelle") , s + " :1");
			//if(_mainNode.transform.GetChildCount() == 1)
				//_avatarControl.SetAutoDisplay(true);
			break;*/
		/*case 8: //ROTATION
            m_atmosphing = 0;
            if(!m_gridRotating)  // -- Afficher l'aide contextuelle --
            {
                SetGridParametringFalse();
                m_gridRotating = true;
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_5);
			}
			
			PC.ctxHlp.m_ftimerButton = 0.1f;
			
			m_udGUI = false;
            if(!isOnUI())           // Quick-fix biossun molette souris
            {
				sc.setL(slidedetect(true, 4000)/factorRotation);
				//s = val.ToString("0.00");
				//GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Rotation"), s + " "+TextManager.GetText("GUIMenuRight.degres"));
            }
			val = sc.GetL();
			s = val.ToString("0.00");
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Rotation"), s + " "+TextManager.GetText("GUIMenuRight.degres"));
            break;*/
				
		/*case 2: //HAUTEUR
            m_atmosphing = 0;
            if(!m_gridHeighting)  // -- Afficher l'aide contextuelle --
            {
                SetGridParametringFalse();
                m_gridHeighting = true;
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_2);
			}
			
			PC.ctxHlp.m_ftimerButton = 0.1f;
			
			m_udGUI = false;
			if(_detachChildren)
				AttachMainNodeChildren();
			if(!isOnUI())           // Quick-fix biossun molette souris
			{
				float ftemp = -slidedetect(true, 1000);
				
				if(ftemp != 0.0f)
				{
				 sc.setH(ftemp/factorHeight);
				}
				
			//	s =((val*100)/100).ToString("0.00");
			//	GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Hauteur") , s + " m");
            }
			val = sc.GetH ();
			s =((val*100)/100).ToString("0.00");
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Hauteur") , s + " m");
            break;*/
				
		case 3: //PERS.
			avatarForceDisplay(false);
            m_atmosphing = 0;
            if(!m_gridPerspecting)  // -- Afficher l'aide contextuelle --
            {
                SetGridParametringFalse();
                m_gridPerspecting = true;
			}
			
			if(_detachChildren)
				AttachMainNodeChildren();
            if(!isOnUI())           // Quick-fix biossun molette souris
            {
				sc.setP(slidedetect(true, 1000)/factorPers);
			//	s = (sc.GetPers()).ToString("0.00");
			//	GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Perspective") ,s);
            }
			val = sc.GetP();
			s = (sc.GetPers()).ToString("0.00");
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Perspective") ,s);
            break;
		case 4: //Roulis.
		
			avatarForceDisplay(false);
            m_atmosphing = 0;
            if(!m_gridRolling)  // -- Afficher l'aide contextuelle --
            {
                SetGridParametringFalse();
                m_gridRolling = true;
			}
			
			if(_detachChildren)
				AttachMainNodeChildren();
            if(!isOnUI())           // Quick-fix biossun molette souris
            {
				/*val =*/ sc.setR(slidedetect(true, 4000)/factorRoulis);
				//s = val.ToString("0.00");
				//GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Roulis"), s + " "+TextManager.GetText("GUIMenuRight.degres"));
            }
			val = sc.GetR();
			s = val.ToString("0.00");
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Roulis"), s + " "+TextManager.GetText("GUIMenuRight.degres"));
            break;
        case 5 :
        	
			camPivot.GetComponent<SceneControl>().bkup();
			
			_avatarControl.gameObject.transform.position = Vector3.zero;
			
			interacteur.setSelected(_avatarControl.gameObject);
			
			GetComponent<GUIMenuMain>().setUDText("", "");
			
        	m_selSM = 6;
        	
        	break;
		case 6 :
			
			avatarForceDisplay(true);
			//GameObject camPivot = GameObject.Find("camPivot");
			PC.In.Zoom_n_drag2V_n_rotate2D(out deltaScale,out deltaHeight,out deltaRota); //ROTATE / SCALE / HEIGHT
			
				float retFactor = usefullData.retinus ? 0.5f: 1f;
				
				//int ichoiceScaleRotate = Mathf.Abs(deltaScale) > Mathf.Abs(deltaRota) ? 1 : 2; //1 == SCALE | 2 == ROTATE
				
				//List<Transform> tempList = new List<Transform>();
				

				
				//if(deltaScale != 0/* && ichoiceScaleRotate == 1*/)
				//{
					GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.GeneralEchelle") , ((sc.GetS()*10)/10).ToString("0.00") + " :1");
					camPivot.GetComponent<SceneControl>().setS(deltaScale/5.0f);	
				//}
				/*if(deltaHeight != 0)
				{
					GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Hauteur") , ((sc.GetH ()*100)/100).ToString("0.00") + " m");
					#if UNITY_IPHONE
					deltaHeight *= 7.5f;
					#endif
					camPivot.GetComponent<SceneControl>().setH(-deltaHeight/15.0f);
				}*/
				/*if(deltaRota != 0 && ichoiceScaleRotate == 2)
				{
					foreach(Transform child in _mainNode.transform)
					{
						tempList.Add(child);
					}
					
					foreach(Transform t in tempList)
					{
						t.parent = _camPivot.transform;
					}
					
					GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Rotation"), sc.GetL().ToString("0.00") + " "+TextManager.GetText("GUIMenuRight.degres"));
					#if UNITY_IPHONE
					deltaRota *= -1.0f;
					#endif
					camPivot.GetComponent<SceneControl>().setL(-deltaRota*1.75f);
				}
				
				foreach(Transform t in tempList)
				{
					t.parent = _mainNode.transform;
				}*/
			//}
			
			break;
		case 7 :
			
			avatarForceDisplay(false);
			/*if(PC.In.Zoom_n_drag2V_n_rotate2D(out deltaScale,out deltaHeight,out deltaRota))//ROTATE / SCALE / HEIGHT
			{
				List<Transform> tempList = new List<Transform>();
				
				GameObject camPivot = GameObject.Find("camPivot");
				
				if(deltaRota != 0)
				{
					foreach(Transform child in _mainNode.transform)
					{
						tempList.Add(child);
					}
					
					foreach(Transform t in tempList)
					{
						t.parent = _camPivot.transform;
					}
					
					GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Rotation"), sc.GetL().ToString("0.00") + " "+TextManager.GetText("GUIMenuRight.degres"));
					#if UNITY_IPHONE
					deltaRota *= -1.0f;
					#endif
					camPivot.GetComponent<SceneControl>().setL(-deltaRota*1.75f);
				}
				
				foreach(Transform t in tempList)
				{
					t.parent = _mainNode.transform;
				}
			}*/
			
			float fslide = slidedetect(true, 4000);
			
			if(!isOnUI() && fslide != 0.0f)           // Quick-fix biossun molette souris
			{
				List<Transform> tempList = new List<Transform>();
				
				//GameObject camPivot = GameObject.Find("camPivot");
				
				foreach(Transform child in _mainNode.transform)
				{
					tempList.Add(child);
				}
				
				foreach(Transform t in tempList)
				{
					t.parent = _camPivot.transform;
				}
				
				sc.setL(fslide/factorRotation);
				
				foreach(Transform t in tempList)
				{
					t.parent = _mainNode.transform;
				}
			}
			val = sc.GetL();
			s = val.ToString("0.00");
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.Rotation"), s + " "+TextManager.GetText("GUIMenuRight.degres"));
			break;
		case 9 :		// Intensité de lumière pour OS3D edition Lite
			if(Montage.sm.IsNightMode())
			{ 
				UsefullEvents.FireNightMode(false);
			}
            if(m_atmosphing != 3)
            {
                m_atmosphing = 3; // TODO vérifier si tout est OK pour les aides contextuelles
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_7);
            }
            if(!isOnUI())       // Quick-fix molette biossun
            {
    			val = ((int)(lc.changeIntensity(slidedetect(true, 1000)/factorLightIntensity)));
    			s = ((int)val).ToString();
    			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.lightIntensity"),s + " %");
            }
			break;
		}
		
		if(m_slideTouchEnded)
		{
			sc.saveToModel();
			Debug.Log("SAVETOMODEL");
			m_slideTouchEnded = false;
		}
	}

	//appel dans ExplorerV2.cs car sinon probleme de sauvegarde si on ne touche pas à la 3D
	// ctl+F -> taper "SaveSceneControl();" et rechercher pour retrouver la ligne
	public void SaveSceneControl()
	{
		sc.saveToModel();
		Debug.Log("SAVETOMODEL");
		m_slideTouchEnded = false;
	}

	private void lightNreflexControl()
	{
		if(_detachChildren)
			AttachMainNodeChildren();
		switch (m_selSM)
		{
		case 0:
			LightControl();
			break;
		case 1:
			shadowControl();
			break;
		case 2:
			reflexControl();
			break;
		case 3:
			presetsControl();
			break;
		}
	}
	
	public void avatarForceDisplay(bool _bdisplay)
	{
		_avatarControl.SetForceDisplay(_bdisplay);
		
		/*if(_avatarDisplay != null)
		{
			((GUICheckbox)_avatarDisplay).SetValue(_bdisplay);
		}*/
		if(_avatarItem != null)
		{
			_avatarItem.SetEnableUI(_bdisplay);
		}
		//GetComponent<GUIMenuMain>().setUDText("","");
		Avatar.Locked = !_bdisplay;
		
		/*if(!_bdisplay && m_selSM == 4)
		{
			m_selSM = 0;
		}*/
	}
	
	private void reflexControl()
	{
		if(m_selT == 1)
		{
            m_atmosphing = 0;
//			lc.setReflex2Zero();
			string ui = lc.reflexSwitch();
			GetRoot().getSelectedItem().getSelectedItem().getSelectedItem().chgTxt(ui);
			GetRoot().getSelectedItem().getSelectedItem().resetSelected();
			
			m_bkupSelT = -1;
			m_bkupSelM = m_selM;
			m_bkupSelSM = m_selSM;
			print ("reflex");
			setMenuState();
			if(lc.isReflexActive())
			{
				GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.reflection"),"");
				((GUIItemV2)GetRoot().getSelectedItem().getSelectedItem().getSubItems()[1]).SetEnableUI(true);
			}
			else
			{
				GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.noReflection"),"");
				((GUIItemV2)GetRoot().getSelectedItem().getSelectedItem().getSubItems()[1]).SetEnableUI(false);
			}
			return;
		}
		
		else if(m_selT ==0)
		{
            if(m_atmosphing != 6)
            {
                m_atmosphing = 6;
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_7);
			}
			
			if(lc.isReflexActive() && !isOnUI())         // Quick-fix biossun molette souris
			{
				string s = "";
				float val = 0;
				//lc.setReflexionLimits(1);
				print ("yolo");
				val = lc.changeReflex(slidedetect(true, 1000)/factorReflexionIntensity);
				s = ((int)val).ToString();
				GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.tauxReflection"),s+ " %");
			}
		}
		
		if(m_slideTouchEnded)
		{
			lc.saveReflexToModel();
			Debug.Log("Reflexion saved");
			m_slideTouchEnded = false;
		}
		
	}
	
	private void shadowControl()
	{
		string s = "";
		float val = 0;
		if(m_selT < 2 &&m_selT != -1)
            m_oldSelT = m_selT;
		
		switch (m_selT)
		{
		case 1:	//Intensité
			if(Montage.sm.IsNightMode())
			{ 
				UsefullEvents.FireNightMode(false);
			}
            if(m_atmosphing != 4)
            {
                m_atmosphing = 4;
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_7);
			}
			
            if(!isOnUI())       // Quick-fix molette biossun
            {
    			val = lc.changeShadowIntensity(slidedetect(true, 1000)/factorShadowIntensity);
    			s = ((int)val).ToString();
    			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.shadowIntensity"),s+ " %");
            }
			break;
	/*	case 0:	//flou
            if(m_atmosphing != 5)
            {
                m_atmosphing = 5;
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_7);
            }
            if(!isOnUI())       // Quick-fix molette biossun
            {
    			val = lc.changeBlur(slidedetect(false, 1000)/factorShadowBlur);
    			s = ((int)val).ToString();
    			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.shadowBlur"),s+ " %");
            }
			break;*/
		
		case 2:	//undo
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.cancel"),"");
			lc.UndoLight(false);
			GetRoot().getSelectedItem().resetSelected();
            m_bkupSelT = m_oldSelT;
			m_bkupSelM = 1;
			m_bkupSelSM = 1;
			setMenuState();
			break;
		case 3:	//redo
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.redo"),"");
			lc.RedoLight(false);
			GetRoot().getSelectedItem().resetSelected();
            m_bkupSelT = m_oldSelT;
			m_bkupSelM = 1;
			m_bkupSelSM = 1;
			setMenuState();
			break;
		case 4:	//reset
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.reset"),"");
			lc.SetLightConfig(true,false,true);
            lc.SaveToModel(false);           // Creation d'un point d'undo
			GetRoot().getSelectedItem().resetSelected();
            m_bkupSelT = m_oldSelT;
			m_bkupSelM = 1;
			m_bkupSelSM = 1;
			setMenuState();
			break;
		}
		
		if(m_slideTouchEnded)
		{
			lc.SaveToModel(false);
			m_slideTouchEnded = false;
		}
	}
	
	private void BGImageControl()
	{
		if(m_selSM != -1)
		{
			if(! _cb.isVisible())
			{
				_cb.showMe(true,GUI.depth);
				_cb.setBtns(TextManager.GetText("GUIMenuRight.Yes"),
					TextManager.GetText("GUIMenuRight.No"));
				_cb.setText(TextManager.GetText("GUIMenuRight.newPhotoAsk"));
			}
			else
			{
				if(_cb.getConfirmation())
				{
					_cb.showMe(false,GUI.depth);
					
					//DO
					Camera.main.cullingMask = 0;
					showMenu = false;
					canDisp=false;
					
					GetComponent<GUIMenuLeft>().canDisplay(false);
					GetComponent<GUIMenuLeft>().ResetMenu();
					
					interacteur.setActived(false);
					Resources.UnloadUnusedAssets();			
					switch (m_selSM)
					{
					case 0: //Take photo
#if UNITY_ANDROID
						GetComponent<GUIStart>().DontShowStartAnd(true,false,false);
#else
						GetComponent<GUIStart>().ShowStartAnd(true,false,false);
#endif
						resetMenu();
						break;
					case 1: //Import photo
#if UNITY_ANDROID
						GetComponent<GUIStart>().DontShowStartAnd(false,true,false);
#else	
						_pluginPhotoRef.SetMode(true);
						/*#if UNITY_IPHONE && !UNITY_EDITOR
						EtceteraManager.setPrompt(true);
						EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Album );
						#elif UNITY_ANDROID
						// showStart(false);
						AndroidPlugin.browseGallery();
						//EtceteraAndroid.promptForPictureFromAlbum(UnityEngine.Screen.width,UnityEngine.Screen.height,name);
						#elif UNITY_STANDALONE
						OpenFile();
						#endif*/
						GetComponent<GUIStart>().ShowStartAnd(false,true,false);
#endif
						resetMenu();
						break;
					case 2: //Example photo
#if UNITY_ANDROID
						GetComponent<GUIStart>().DontShowStartAnd(false,false,true);
#else
						GetComponent<GUIStart>().ShowStartAnd(false,false,true);
#endif
						resetMenu();
						break;
					}	
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
	}
	
	private void AssemblyControl()
	{
		switch (m_selSM)
		{
		//Mobiles
		case 0:
			StartCoroutine(Camera.main.GetComponent<Screenshot>().galleryShot());
			resetMenu();
			break;
		case 1:
			StartCoroutine(Camera.main.GetComponent<Screenshot>().mailShot());
			resetMenu();
			break;
		//PC-OSX
		case 2:
			StartCoroutine(Camera.main.GetComponent<Screenshot>().takeScreenShotPC(false));
			resetMenu();
			break;
		}
	}
	
	//-----------------------------------------------------
	private void LightControl()
	{
		string s = "";
		float val = 0;
        if(m_selT < 3 && m_selT != -1)
            m_oldSelT = m_selT;
		
		if(_hourItem.isSecondButtonClicked() ||
		 _intensityItem.isSecondButtonClicked() ||
		  _orientationItem.isSecondButtonClicked())
		{
			PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_7);
			PC.ctxHlp.ShowHelp();
		}
		
		switch (m_selT)
		{
		case 0:	//heure
            if(m_atmosphing != 1)
            {
                m_atmosphing = 1;
			}
			
           // if(!isOnUI())       // Quick-fix molette biossun
           // {
				float tmp = lc.changeHour(slidedetectForCompass(true, 1000)/factorLightHour);
    			int h = (int)tmp;
    			float m = ((tmp%1)*60)/15;
    			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.lightHour") , h + " h "+(int)m*15);
          //  }
			break;
		case 1:	//Azimut
				if(Montage.sm.IsNightMode())
				{ 
					UsefullEvents.FireNightMode(false);
				}
	            if(m_atmosphing != 2)
	            {
	                m_atmosphing = 2;
				}
			
				if(!lc.IsOrientationShown())
					lc.ShowOrientation(true);;
	          	 // if(!isOnUI())       // Quick-fix molette biossun
	         	 //  {
					/*val = */lc.changeAzimut(slidedetectForCompass(true, 3000)/factorLightAzimut);
	//    			s = ((int)val).ToString();
	    			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.lightOrientation") ,/*TextManager.GetText("GUIMenuRight.Card")*/"");
	         //   }
			break;
		case 2:	//intensite
				if(Montage.sm.IsNightMode())
				{ 
					UsefullEvents.FireNightMode(false);
				}
	            if(m_atmosphing != 3)
	            {
	                m_atmosphing = 3;
				}
			
	          //  if(!isOnUI())       // Quick-fix molette biossun
				//{
	    			val = ((int)(lc.changeIntensity(slidedetect(true, 1000)/factorLightIntensity)));
					s = ((int)val).ToString();
	    			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.lightIntensity"),s + " %");
	           // }
			break;
		case 3:	//mode nuit
            m_atmosphing = 0;
			UsefullEvents.FireNightMode(true);
			GetComponent<GUIMenuMain>().hideShowUD(false);
			break;
		case 4:	//mode jour
            m_atmosphing = 0;
			UsefullEvents.FireNightMode(false);
			GetComponent<GUIMenuMain>().hideShowUD(false);
			break;
		case 5:	//undo
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.cancel"),"");
			lc.UndoLight(true);
			GetRoot().getSelectedItem().resetSelected();
            m_bkupSelT = m_oldSelT;
			m_bkupSelM = 1;
			m_bkupSelSM = 0;
			setMenuState();
			break;
		case 6:	//redo
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.redo"),"");
			lc.RedoLight(true);
			GetRoot().getSelectedItem().resetSelected();
            m_bkupSelT = m_oldSelT;
			m_bkupSelM = 1;
			m_bkupSelSM = 0;
			setMenuState();
			break;
			// TODO Redo
		case 7:	//reset
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.reset"),"");
			lc.SetLightConfig(true,true,false);
            lc.SaveToModel(true);           // Creation d'un point d'undo
			GetRoot().getSelectedItem().resetSelected();
            m_bkupSelT = m_oldSelT;
			/*
			m_bkupSelM = 1;
			m_bkupSelSM = 0;
			setMenuState();
			*/
			break;
		default :
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.light"),"");
			break;
		}
		
		if(m_slideTouchEnded)
		{
			lc.SaveToModel(true);
			m_slideTouchEnded = false;
		}
	}
	
	private void presetsControl()
	{
		if(m_selT != -1 && m_selT < lc.getPresets().Count)
		{
			if(m_selT==3)
				UsefullEvents.FireNightMode(true);
			else
				UsefullEvents.FireNightMode(false);
			lc.applyPreset(m_selT);
			m_selT = -1;
			
			m_bkupSelM = m_selM;
			m_bkupSelSM = m_selSM;
			m_bkupSelT = m_selT;
			
			resetMenu();
			print("preset");
			setMenuState();
		}

	}
	
	private void upgradeControl()
	{
		/*if(_detachChildren)
			AttachMainNodeChildren();*/
		//-------------
		
		switch (m_selSM) 
		{
		case 0: //Premier Plan
			firstPlaneControl();
			break;
		case 1: //Matière au sol
			groundMaterialControl();
		break;
		}
	}
	
	private void firstPlaneControl()
	{
		if(_gommeAddItem.isSecondButtonClicked())
		{
			PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_8);
			PC.ctxHlp.ShowHelp();
		}
		if(_gommeEraseItem.isSecondButtonClicked())
		{
			PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_9);
			PC.ctxHlp.ShowHelp();
		}
		
		switch (m_selT)
		{
		case 0: //Mettre 1er plan
			MaskOrGrass = 1;
			GetComponent<GUISubTools>().activate();
			GetComponent<GUISubTools>().startGomme(true);
			
			GetComponent<GUIMenuLeft>().canDisplay(false);
			canDisplay(false);
			setVisibility(false);
			
			interacteur.setActived(false);
			
			m_bkupSelM = _mUpgrade;
			m_bkupSelSM = 0;
			m_selT = -1;
			
			break;
		case 1: //Remettre 2eme plan
			MaskOrGrass = 1;
			GetComponent<GUISubTools>().activate();
			GetComponent<GUISubTools>().startGomme(false);
			
			GetComponent<GUIMenuLeft>().canDisplay(false);
			canDisplay(false);
			setVisibility(false);
			
			interacteur.setActived(false);
			
			m_bkupSelM = _mUpgrade;
			m_bkupSelSM = 0;
			m_selT = -1;
			
			break;
		case 2: //Reinitialiser
		
			GetComponent<GUISubTools>().resetEraserV2();
			
			m_bkupSelM = _mUpgrade;
			m_bkupSelSM = 0;
			m_selT = -1;
			
			break;
		}
	}
	
	private void groundMaterialControl()
	{
		if(_gazonAddItem.isSecondButtonClicked())
		{
			PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_10);
			PC.ctxHlp.ShowHelp();
		}
		if(_gazonEraseItem.isSecondButtonClicked())
		{
			PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_11);
			PC.ctxHlp.ShowHelp();
		}
		
		m_bsubtoolChoiceMaterial = false;
		switch (m_selT) 
		{
		case 0: //Ajouter
			MaskOrGrass = 2;
			GetComponent<GUISubTools>().activate();
			GetComponent<GUISubTools>().startGazon(true);
			
			GetComponent<GUIMenuLeft>().canDisplay(false);
			canDisplay(false);
			setVisibility(false);
			
			interacteur.setActived(false);
			
			m_bkupSelM = _mUpgrade;
			m_bkupSelSM = 1;
			m_selT = -1;
			
			break;
		case 1: //Retirer
			MaskOrGrass = 2;
			GetComponent<GUISubTools>().activate();
			GetComponent<GUISubTools>().startGazon(false);
			
			GetComponent<GUIMenuLeft>().canDisplay(false);
			canDisplay(false);
			setVisibility(false);
			
			interacteur.setActived(false);
			
			m_bkupSelM = _mUpgrade;
			m_bkupSelSM = 1;
			m_selT = -1;
			
			break;
		case 2: //Matières
			
			GetComponent<GUISubTools>().choiceMaterial();
			
			GetComponent<GUIMenuLeft>().canDisplay(false);
			canDisplay(false);
			setVisibility(false);
			
			interacteur.setActived(false);
			
			m_bsubtoolChoiceMaterial = true;
			
			m_bkupSelM = _mUpgrade;
			m_bkupSelSM = 1;
			m_selT = -1;
			
			break;
		case 3: //Reinitialiser
			
			GetComponent<GUISubTools>().resetGrassV2();
			
			m_bkupSelM = _mUpgrade;
			m_bkupSelSM = 1;
			m_selT = -1;
			
			break;
		}
	}
	
	public void quitSubTool()
	{
		print ("quit");
		m_selM = _mUpgrade;
		m_selSM = m_bkupSelSM;
		m_selT = -1;
		
		m_gazonMaterial.setIcon(GetComponent<GUISubTools>().getCurrentTex(), "textOutilOn", "textOutilOff");
		GetComponent<GUIMatPicker>().canDisplay(true);
	}
	
	private void projectControl()
	{
		if(_detachChildren)
			AttachMainNodeChildren();
			
		switch (m_selSM)
		{
		case 0: // save/load
			GetComponent<GUIStart>().activateExplorerInGame();
			resetMenu();
			break;
		case 1: // export pc
			StartCoroutine(GetComponent<Montage>().SaveAndExport());
			resetMenu();
			break;
			
		case 2 : // sauver image pc
			StartCoroutine(Camera.main.GetComponent<Screenshot>().takeScreenShotPC(false));
			resetMenu();
			break;
			
		case 3: //envoyer image iPad
			StartCoroutine(GetComponent<Montage>().SaveAndSend());
			resetMenu();
			break;
			
		case 4: //sauver image iPad
			StartCoroutine(Camera.main.GetComponent<Screenshot>().galleryShot());
			resetMenu();
			break;
		case 5 :
			StartCoroutine(Camera.main.GetComponent<Screenshot>().mailShot());
			resetMenu();
			break;
		}
	}
	
	private void pluginsControl()
	{
		if(m_selT > -1)
		{
			((PluginsOS3D) _pluginsOS3D[m_selT]).StartPlugin();
			setVisibility(false);
		}
	}
	
	private void paramsControl()
	{
		if(_detachChildren)
			AttachMainNodeChildren();
		switch (m_selSM)
		{
		case 0: //Désactivation licence
			if(! _cb.isVisible())
			{
				_cb.setBtns(TextManager.GetText("GUIMenuRight.Yes"),
					TextManager.GetText("GUIMenuRight.No"),
					TextManager.GetText("GUIMenuRight.Save"));
				_cb.showMe(true,GUI.depth);
				_cb.setText(TextManager.GetText("GUIMenuRight.DeactivateLic"));
			}
			else
			{
				if(_cb.getConfirmation())
				{
					_cb.showMe(false,GUI.depth);
					
					//DO
					StartCoroutine(UsefulFunctions.Deactivate(_cb, true));
					resetMenu();
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
			break;					
		case 1:
			if(m_selT != -1)
				m_oldSelT = m_selT;
			switch(m_selT)
			{
			case 0: 
			case 1: 
			
			if(! _cb.isVisible())
			{
				_cb.Show3BtnBox(true,GUI.depth);
			_cb.setText(TextManager.GetText("GUIMenuRight.ChangeLanguage"));
			_cb.setBtns(TextManager.GetText("GUIMenuRight.No"),
				TextManager.GetText("GUIMenuRight.cancel"),
				TextManager.GetText("GUIMenuRight.Yes"));
			}
			else
			{
				if(_cb.getConfirmation())
				{
					UsefulFunctions.ChangeLanguage(m_selT);		
					_cb.Show3BtnBox(false,GUI.depth);					
					//DO
					SetAllowQuit(true);
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
					_changeLanguage=false;
					_cb.Show3BtnBox(false,GUI.depth);
					resetMenu();
					//----------
				}
				if(_cb.get3rdChoice())
				{
					UsefulFunctions.ChangeLanguage(m_selT);		
					_cb.Show3BtnBox(false,GUI.depth);
					GetComponent<Montage>().showSaveAndQuit();
					showMenu = false;
					GetComponent<GUIMenuLeft>().canDisplay(false);
					canDisplay(false);
					resetMenu();	
				}
			}
				break;
			}
			break;			
		case 2: //Mise à jour disponible
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
			if(_linkOpen == false)
			{
				Application.OpenURL(usefullData.GetNewLink());
				_linkOpen = true;
			}
			//SetAllowQuit(true);
			//Application.Quit();
			ShowWarningQuit();
#endif	
#if UNITY_IPHONE
//			Application.OpenURL(usefullData.GetNewLink());
//			resetMenu ();	
			if(_linkOpen == false)
			{
				Application.OpenURL(usefullData.GetNewLink());
				_linkOpen = true;
			}
			ShowWarningQuit();
#endif				
			break;
			
		case 3:
			//A propos
			GetComponent<HelpPanel>().switchAbout();
			break;
		}	
	}
	
	private void PassiveControl(string ui,int[] indexs) // Use with UpdateUI Event
	{		
//		if((m_selSM != 6 || m_selM != 0) && interacteur.getSelected() != null)
//		{
//			if(interacteur.getSelected().name == "_avatar")
//		   	{
//		    	interacteur.setSelected(null,true);
//		   	}
//		}
		
		if(ui == GetType().ToString())
		{
			if(m_selM == 0 && !Camera.main.GetComponent<Mode2D>().IsActive())//Ouverture du menu reglage grille
			{
				_grid.GetComponent<Renderer>().enabled = true;
				if(m_selSM != -1)// && m_selSM != 6)
				{
					GetComponent<GUIMenuMain>().hideShowUD(true);
					
					//if(_mainNode.transform.GetChildCount() == 1)
					//	_avatarControl.SetAutoDisplay(true);
				}
				else
				{
					m_bkupSelM = 0;
					m_bkupSelSM = m_oldSelSM;
				}
				
			}
			else if(m_selM == 1 && m_selSM != -1)//Ouverture menu eclairage/reflexion
			{
				GetComponent<GUIMenuMain>().hideShowUD(true);	
				
				//if(_mainNode.transform.GetChildCount() == 1)
				//	_avatarControl.SetAutoDisplay(true);
				
				if(m_oldSelSM != m_selSM)
				{
					m_oldSelSM = m_selSM;
					m_oldSelT = -1;
				}
				
				if(m_selSM <2 && m_selT == -1)
				{
					if(m_oldSelT == -1)
						m_oldSelT = 0;
					m_bkupSelM = m_selM;
					m_bkupSelSM = m_selSM;
					m_bkupSelT = m_oldSelT;
					setMenuState();
					print ("<2 == -1");
				}
			}
			else if(m_selM == 2 && m_selSM != -1)
			{
				//interacteur.setActived(false);
			}
			else
			{
				if(configScene)
				{
					_grid.GetComponent<Renderer>().enabled = false;
					//_avatarControl.SetForceDisplay(false);
					GetComponent<GUIMenuMain>().hideShowUD(false);
					GetComponent<GUIMenuMain>().setUDText("","");
					configScene = false;
				}
				if(configSceneLight)
				{
					GetComponent<GUIMenuMain>().hideShowUD(false);
					//_avatarControl.SetForceDisplay(false);
					GetComponent<GUIMenuMain>().setUDText("","");
					configSceneLight = false;
				}
				if(m_selM == -1)
				{
					tempShow = true;
				}
			}
		}
	}
	
	public void CreateGui()
	{
		//Title = new GUIItemV2(-1,-1,TextManager.GetText("GUIMenuRight.Parameters"),"title","title",this);

		menuGroup = new Rect(m_off7,0,320,Screen.height);
		m_off7 = Screen.width;
		
		//MENUS
		GUIItemV2 rg = null;
		GUIItemV2 upgd = null;

		if(usefullData._edition == usefullData.OSedition.Full)
		{
			//Reglage grille
			rg = _3DItem = new GUIItemV2(_mMenu,_mConfGrnd,TextManager.GetText("GUIMenuRight.GroundConfiguration"),"menuSolOn","menuSolOff",this);
			//Gomme gazon
			upgd = new GUIItemV2(_mMenu,_mUpgrade,TextManager.GetText("GUIMenuRight.improvements"),"menuUpgOn","menuUpgOff",this);
		}
		//Ambiance
		GUIItemV2 am = 	new GUIItemV2(_mMenu,_mAtmosphr,TextManager.GetText("GUIMenuRight.Atmosphere"),"menuLiteOn","menuLiteOff",this);
		//Photo de fond
		GUIItemV2 bgImg = new GUIItemV2(_mMenu,_mBgImg,TextManager.GetText("GUIMenuRight.BGImage"),"menuBgOn","menuBgOff",this);
		//Projet
		GUIItemV2 pjt = new GUIItemV2(_mMenu,_mProject,TextManager.GetText("GUIMenuRight.Project"),"menuProjOn","menuProjOff",this);
		//PhotoMontage
		//GUIItemV2 pm = new GUIItemV2(_mMenu,_mAssembly,TextManager.GetText("GUIMenuRight.Assembly"),"menuAssemblyOn","menuAssemblyOff",this);
		//Plugins
		GUIItemV2 pluginsUI = new GUIItemV2(_mMenu,_mPlugins,TextManager.GetText("GUIMenuRight.Plugins"),"menuPlugsOn","menuPlugsOff",this);
		//Accueil (BTN)
		GUIItemV2 wlcm = new GUIItemV2(_mMenu,_mStartMenu,TextManager.GetText("GUIStart.Title"),"menuQuitOn","menuQuitOff",this);
		
		//----------------------------------------------------
		
		//Filling Menus
		//Menu reglage de la grille
		if(rg != null)
		{
			//GUIItemV2 avatar = new GUIItemV2(_mSubMenu,6,TextManager.GetText("GUIMenuRight.Avatar"),"sousMenuOn","sousMenuOff",this);
			//_avatarItem = new GUIItemV2(_mTool,1,TextManager.GetText("GUIMenuRight.MoveAvatar"),"outilOn","outilOff",this);
			//_avatarItem.SetEnableUI(false);
			_avatarDisplay = new GUIItemV2(_mSubMenu,6,TextManager.GetText("GUIMenuRight.DisplayAvatar"),"outilOn","outilOff",this);//6
			_avatarDisplay.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");  
			rg.addSubItem(_avatarDisplay);
			//avatar.addSubItem(_avatarItem);
			//rg.addSubItem(avatar);
			
			_inclinaisonItem = new GUIItemV2(_mSubMenu,0,TextManager.GetText("GUIMenuRight.Inclinaison"),"outilOn","outilOff",this);
			_inclinaisonItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
			rg.addSubItem(_inclinaisonItem);//0
			//rg.addSubItem(new GUIItemV2(_mSubMenu,1,TextManager.GetText("GUIMenuRight.Echelle"),"outilOn","outilOff",this));//1
			//rg.addSubItem( new GUIItemV2(/*_mTool*/_mSubMenu,2,TextManager.GetText("GUIMenuRight.Hauteur"),"outilOn","outilOff",this));//1
			//rg.addSubItem(new GUIItemV2(_mSubMenu,8,TextManager.GetText("GUIMenuRight.Rotation"),"outilOn","outilOff",this));//0
				
		//SOUS MENU AUTRES REGLAGES
//		GUIItemV2 rgSubMenu = new GUIItemV2(_mSubMenu,2,TextManager.GetText("GUIMenuRight.MoreAdjustment"),"sousMenuOn","sousMenuOff",this);//2
		/*rgSubMenu*/
			_rotationItem = new GUIItemV2(/*_mTool*/_mSubMenu,7,TextManager.GetText("GUIMenuRight.Rotation"),"outilOn","outilOff",this);//7
			_rotationItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
			rg.addSubItem(_rotationItem);

			_perspectiveItem = new GUIItemV2(_mSubMenu,3,TextManager.GetText("GUIMenuRight.Perspective"),"outilOn","outilOff",this);//2
			_perspectiveItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
			rg.addSubItem(_perspectiveItem);

			_roulisItem = new GUIItemV2(/*_mTool*/_mSubMenu,4,TextManager.GetText("GUIMenuRight.Roulis"),"outilOn","outilOff",this);//3
			_roulisItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
			rg.addSubItem(_roulisItem);
			
			rg.addSubItem( new GUIItemV2(/*_mTool*/_mSubMenu,5,TextManager.GetText("GUIMenuRight.Reset"),"outilOff","outilOff",this));//5
			//		rg.addSubItem(rgSubMenu);
		}
		
		//Ambiance
		//SousMenu eclairage	
		GUIItemV2 lite = new GUIItemV2(_mSubMenu,0,TextManager.GetText("GUIMenuRight.light"),"sousMenuOn","sousMenuOff",this);
		
		_hourItem = new GUIItemV2(_mTool,0,TextManager.GetText("GUIMenuRight.lightHour"),"outilOn","outilOff",this);//0
		_hourItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
		lite.addSubItem(_hourItem);
		
		_orientationItem = new GUIItemV2(_mTool,1,TextManager.GetText("GUIMenuRight.lightOrientation"),"outilOn","outilOff",this);//1
		_orientationItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
		lite.addSubItem(_orientationItem);
		
		_intensityItem = new GUIItemV2(_mTool,2,TextManager.GetText("GUIMenuRight.lightIntensity"),"outilOn","outilOff",this);//2	
		_intensityItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
		lite.addSubItem(_intensityItem);	
		
		_guiItemv2Night = new GUIItemV2(_mTool,3,TextManager.GetText("LightPresets.NightMode"),"outilOn","outilOff",this);//3
		lite.addSubItem(_guiItemv2Night);//3
		
		_guiItemv2Day = new GUIItemV2(_mTool,4,TextManager.GetText("LightPresets.DayMode"),"outilOn","outilOff",this);
		lite.addSubItem(_guiItemv2Day);//4
		_guiItemv2Day.SetEnableUI(false);	
		
		//if(usefullData._edition == usefullData.OSedition.Lite)	// Fonction intensité dans menu "3D" en édition lite
		//	rg.addSubItem( new GUIItemV2(_mSubMenu,9,TextManager.GetText("GUIMenuRight.lightIntensity"),"outilOn","outilOff",this));//2		
		
		//SousMenu Ombres	
	//	GUIItemV2 shadows = new GUIItemV2(_mSubMenu,1,TextManager.GetText("GUIMenuRight.Shadow"),"sousMenuOn","sousMenuOff",this);
	//	shadows.addSubItem( new GUIItemV2(_mTool,0,TextManager.GetText("GUIMenuRight.Blur"),"outilOn","outilOff",this));//0
	//	shadows.addSubItem( new GUIItemV2(_mTool,1,TextManager.GetText("GUIMenuRight.lightIntensity"),"outilOn","outilOff",this));//1
		//SousMenu Reflexion	
		GUIItemV2 reflex = new GUIItemV2(_mSubMenu,2,TextManager.GetText("GUIMenuRight.reflection"),"sousMenuOn","sousMenuOff",this);		
		reflex.addSubItem( new GUIItemV2(_mTool,1,TextManager.GetText("GUIMenuRight.ReflectionActivate"),"outilOn","outilOff",this));//1
		reflex.addSubItem( new GUIItemV2(_mTool,0,TextManager.GetText("GUIMenuRight.reflectionIntensity"),"outilOn","outilOff",this));//0
		((GUIItemV2)reflex.getSubItems()[1]).SetEnableUI(false);
		//SousMenu Presets	
		GUIItemV2 Presets = new GUIItemV2(_mSubMenu,3,TextManager.GetText("GUIMenuRight.Preset"),"sousMenuOn","sousMenuOff",this);
		int i =0;
		foreach(LightPresets.Preset p in lc.getPresets())
		{
			Presets.addSubItem(new GUIItemV2(_mTool,i,TextManager.GetText(p.p_name),"outilOn","outilOff",this));
			i++;
		}
		
		am.addSubItem(lite);//0
	//	am.addSubItem(shadows);//1
		am.addSubItem(reflex);//2
		am.addSubItem(Presets);//3
		//}
		
		//Gomme gazon		
		_gommeItem = new GUIItemV2(_mSubMenu,0,TextManager.GetText("GUIMenuRight.Eraser"),"sousMenuOn","sousMenuOff",this);
		_gazonItem = new GUIItemV2(_mSubMenu,1,TextManager.GetText("GUIMenuRight.Grass"),"sousMenuOn","sousMenuOff",this);
				
		if(!usefullData.lowTechnologie && usefullData._edition == usefullData.OSedition.Full)
		{
			_gommeAddItem = new GUIItemV2(_mTool,0,TextManager.GetText("GUISubTools.erase"),"outilOn","outilOff",this);
			_gommeAddItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
			_gommeItem.addSubItem (_gommeAddItem);
			
			_gommeEraseItem = new GUIItemV2(_mTool,1,TextManager.GetText("GUISubTools.eraseRedo"),"outilOn","outilOff",this);
			_gommeEraseItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
			_gommeItem.addSubItem (_gommeEraseItem);
			
			_gommeItem.addSubItem(new GUIItemV2(_mTool,2,TextManager.GetText("GUISubTools.eraseReinit"),"outilOff","outilOff",this));
			
			upgd.addSubItem(_gommeItem);
			
			//---// 
			
			_gazonAddItem = new GUIItemV2(_mTool,0,TextManager.GetText("GUISubTools.grassHelp"),"outilOn","outilOff",this);
			_gazonAddItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
			_gazonItem.addSubItem (_gazonAddItem);
			
			_gazonEraseItem = new GUIItemV2(_mTool,1,TextManager.GetText("GUISubTools.grassRedoHelp"),"outilOn","outilOff",this);
			_gazonEraseItem.setSecondButton("secondButton", "secondButton", "textOutilOn", "textOutilOff");
			_gazonItem.addSubItem (_gazonEraseItem);

			m_gazonMaterial = new GUIItemV2(_mTool,2,TextManager.GetText("GUISubTools.grassMaterial"),"outilOn","outilOff",this);
			m_gazonMaterial.setIcon(GetComponent<GUISubTools>().getCurrentMaterial(), "textOutilOn", "textOutilOff");
			_gazonItem.addSubItem(m_gazonMaterial);
			
			_gazonItem.addSubItem(new GUIItemV2(_mTool,3,TextManager.GetText("GUISubTools.eraseReinit"),"outilOff","outilOff",this));
			
			upgd.addSubItem(_gazonItem);

		}
		
		//Photo de fond
		#if UNITY_IPHONE || UNITY_ANDROID
			bgImg.addSubItem(new GUIItemV2(_mSubMenu,0,TextManager.GetText("GUIMenuRight.TakePhoto"),"outilOn","outilOff",this));//PRENDRE PHOTO
		#endif
		if(usefullData._edition == usefullData.OSedition.Full)
		{
			bgImg.addSubItem(new GUIItemV2(_mSubMenu,1,TextManager.GetText("GUIMenuRight.LoadImage"),"outilOn","outilOff",this));//IMPORTER PHOTO
		}
		bgImg.addSubItem(new GUIItemV2(_mSubMenu,2,TextManager.GetText("GUIMenuRight.Phototech"),"outilOn","outilOff",this));//EXAMPLE PHOTO
		
		//Projet
		//pjt.addSubItem(new GUIItemV2(_mSubMenu,1,TextManager.GetText("GUIMenuRight.newProject"),"outilOn","outilOff",this));
		pjt.addSubItem(new GUIItemV2(_mSubMenu,0,TextManager.GetText("GUIMenuRight.saveProject"),"outilOn","outilOff",this));
		#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
		pjt.addSubItem(new GUIItemV2(_mSubMenu,1,TextManager.GetText("GUIMenuRight.ExportProject"),"outilOn","outilOff",this));
		#elif UNITY_IPHONE || UNITY_ANDROID
		pjt.addSubItem(new GUIItemV2(_mSubMenu,3,TextManager.GetText("GUIMenuRight.SendProject"),"outilOn","outilOff",this));
		#endif
		
		//Plugins
		//Récupération des plugins
		_pluginsOS3D = new ArrayList();
		GameObject plugs = GameObject.Find("Plugins");
		foreach(Component cp in plugs.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType().GetInterface("PluginsOS3D")!= null)
			{
				if(((PluginsOS3D)cp).isAuthorized())
				{
					_pluginsOS3D.Add(cp);
				}
				
			}
		}
		//---
		if(_pluginsOS3D.Count > 0)
		{
			i = 0;
			foreach(Component plug in _pluginsOS3D)
			{
				string nm = ((PluginsOS3D)plug).GetPluginName();
				pluginsUI.addSubItem( new GUIItemV2(_mTool,i,nm,"outilOn","outilOff",this));	
				i++;
			}
		}
		
		//PhotoMontage
		#if UNITY_IPHONE || UNITY_ANDROID
		pjt.addSubItem(new GUIItemV2(_mSubMenu,4,TextManager.GetText("GUIMenuRight.saveImage"),"outilOn","outilOff",this));//SAUVER IMAGE
		pjt.addSubItem(new GUIItemV2(_mSubMenu,5,TextManager.GetText("GUIMenuRight.loadImage"),"outilOn","outilOff",this));//ENVOYER IMAGE
		#else 
		//PC OSX
		pjt.addSubItem(new GUIItemV2(_mSubMenu,2,TextManager.GetText("GUIMenuRight.saveImage"),"outilOn","outilOff",this));//SAUVER IMAGE
		#endif
		
		//----------------------------------------------------
		
		//Ajout au root node
		GetRoot().addSubItem(rg);
		
		if(usefullData._edition != usefullData.OSedition.Lite)	// Pas de réglages d'ambiance en édition Lite
			GetRoot().addSubItem(am);
		
		if(m_authUpgrade)
		{
			if(!usefullData.lowTechnologie && usefullData._edition == usefullData.OSedition.Full)
			{
				GetRoot().addSubItem(upgd);
			}
		}
		GetRoot().addSubItem(bgImg);
		GetRoot().addSubItem(pjt);
		
		/*if(m_authSaveSendImg)
		{
			GetRoot().addSubItem(pm);
		}*/
		if(_pluginsOS3D.Count > 0)
		{
			GetRoot().addSubItem(pluginsUI);
		}
		GetRoot().addSubItem(wlcm);
	}
	
	private GUIItemV2 GetRoot()
	{
		if( Root==null)
		{
			Root = new GUIItemV2(-1,-1,"Root","","",this);
		}
		return Root;
	}
	
#endregion
	
#endregion
	
#region Undo Redo Reset
	void undo()
	{
		if(m_selM == 0)//grid
		{
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.cancel"),"");
			sc.UndoCam();
			GetRoot().getSelectedItem().resetSelected();
			m_bkupSelM = 0;
			m_bkupSelSM = m_oldSelSM;
			if(m_bkupSelSM == 2)
			{
				m_bkupSelT = m_oldSelT;	
			}
			setMenuState();
			print ("grid");
		}
		else if(m_selM == 1)//ambiance
		{
			if(m_selSM == 0)//eclairage
			{
				GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.cancel"),"");
				lc.UndoLight(true);
				GetRoot().getSelectedItem().resetSelected();
	            m_bkupSelT = m_oldSelT;
				m_bkupSelM = 1;
				m_bkupSelSM = 0;
				setMenuState();
				print ("ambiance");
			}
			else if(m_selSM == 1)//ombres
			{
				GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.cancel"),"");
				lc.UndoLight(false);
				GetRoot().getSelectedItem().resetSelected();
	            m_bkupSelT = m_oldSelT;
				m_bkupSelM = 1;
				m_bkupSelSM = 1;
				setMenuState();
				print ("ombres");
			}
		}
	}
	
	void redo()
	{
		if(m_selM == 0)//grid
		{
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.redo"),"");
			sc.RedoCam();
			GetRoot().getSelectedItem().resetSelected();
			m_bkupSelM = 0;
			m_bkupSelSM = m_oldSelSM;
			if(m_bkupSelSM == 2)
			{
				m_bkupSelT = m_oldSelT;	
			}
			setMenuState();
		}
		else if(m_selM == 1)//ambiance
		{
			if(m_selSM == 0)//eclairage
			{
				GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.redo"),"");
				lc.RedoLight(true);
				GetRoot().getSelectedItem().resetSelected();
	            m_bkupSelT = m_oldSelT;
				m_bkupSelM = 1;
				m_bkupSelSM = 0;
				setMenuState();
			}
			else if(m_selSM == 1)//ombres
			{
				GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.redo"),"");
				lc.RedoLight(false);
				GetRoot().getSelectedItem().resetSelected();
	            m_bkupSelT = m_oldSelT;
				m_bkupSelM = 1;
				m_bkupSelSM = 1;
				setMenuState();
			}
		}
	}
	
	void reset()
	{
		if(m_selM == 0)//grid
		{
			GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.reset"),"");
			sc.init ();     // Cree un point d'undo egalement
			GetRoot().getSelectedItem().resetSelected();
			m_bkupSelM = 0;
			m_bkupSelSM = m_oldSelSM;
			if(m_bkupSelSM == 2)
			{
				m_bkupSelT = m_oldSelT;	
			}
			setMenuState();
		}
		else if(m_selM == 1)//ambiance
		{
			if(m_selSM == 0)//eclairage
			{
				GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.reset"),"");
				lc.SetLightConfig(true,true,false);
	            lc.SaveToModel(true);           // Creation d'un point d'undo
				GetRoot().getSelectedItem().resetSelected();
	            m_bkupSelT = m_oldSelT;
				m_bkupSelM = 1;
				m_bkupSelSM = 0;
				setMenuState();
			}
			else if(m_selSM == 1)//ombres
			{
				GetComponent<GUIMenuMain>().setUDText(TextManager.GetText("GUIMenuRight.reset"),"");
				lc.SetLightConfig(true,false,true);
	            lc.SaveToModel(false);           // Creation d'un point d'undo
				GetRoot().getSelectedItem().resetSelected();
	            m_bkupSelT = m_oldSelT;
				m_bkupSelM = 1;
				m_bkupSelSM = 1;
				setMenuState();
			}
		}
	}
#endregion

//FCN. AUX. PUBLIC--------------------
#region publique	

	public void setMenuStateV2()
	{
		
		m_selM = 0;
		m_selSM = 0;
		m_selT = -1;
		
		if(GetRoot().getSubItemsCount()>0 && m_selM != -1)
		{
			GetRoot().setSelected(m_selM);
			if(GetRoot().getSelectedItem().getSubItemsCount()>0 && m_selSM != -1)
			{
				GetRoot().getSelectedItem().setSelected(m_selSM);
				if(GetRoot().getSelectedItem().getSelectedItem()!=null)
				{	
					if(GetRoot().getSelectedItem().getSelectedItem().getSubItemsCount()>0 && m_selT != -1)
					{
						GetRoot().getSelectedItem().getSelectedItem().setSelected(m_selT);
					}
				}
			}
		}
		//		passiveControl();
		int[] indexs = new int[3];
		indexs[0] = m_selM;
		indexs[1] = m_selSM;
		indexs[2] = m_selT;
		UsefullEvents.FireUpdateUIState(GetType().ToString(),indexs);
	}

	public void updateGUIV2(GUIItemV2 itm,int val,bool reset)
	{

		m_selM = 0;
		m_selSM = 6;
		m_selT = -1;
		/*
		m_bkupSelM = m_selM;
		m_bkupSelSM = m_selSM;
		m_bkupSelT = m_selT;
		setMenuState ();
		//resetMenu();
		AttachMainNodeChildren();

		int[] indexs = new int[3];
		indexs[0] = m_selM;
		indexs[1] = m_selSM;
		indexs[2] = m_selT;
		UsefullEvents.FireUpdateUIState(GetType().ToString(),indexs);

		setVisibility (true);

		//Recentrage UI
		scrollpos.y -= Screen.height / 2;
		*/
	}

	public void updateGUI(GUIItemV2 itm,int val,bool reset)
	{

		if(_uiLocked || PC.ctxHlp.PanelBlockingInputs())
		{
            Debug.Log ("1___________________________M="+m_selM+", SM="+m_selSM+", T="+m_selT);
			m_bkupSelM = m_selM;
			m_bkupSelSM = m_selSM;
			m_bkupSelT = m_selT;
			setMenuState();
			print ("updateGUI");
			return;
		}

		/*if(Camera.mainCamera.GetComponent<ObjInteraction>().getSelected() != null)
		{
			Camera.mainCamera.GetComponent<ObjInteraction>().setSelected(null);
			setVisibility(true);
		}*/
		
		/*if(activeItem == itm && val == -1)
			activeItem = null;
		else */if(reset)
			activeItem = itm;

		if(itm != null)
		{

			switch (itm.getDepth())
			{
				case 0:
					m_selM = val;
					if(reset)
					{
						print ("resetval");
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
		}
//		Debug.Log("VALUES>"+m_selM + " " + m_selSM + " " + m_selT);
//		passiveControl();
//		if((m_selSM != 6 || m_selM != 0) && interacteur.getSelected() != null)
//		{
//			if(interacteur.getSelected().name == "_avatar")
//		   	{
//		    	interacteur.setSelected(null,true);
//		   	}
//		}
		
		int[] indexs = new int[3];
		indexs[0] = m_selM;
		indexs[1] = m_selSM;
		indexs[2] = m_selT;
		UsefullEvents.FireUpdateUIState(GetType().ToString(),indexs);
		
		if((m_selSM != 6 || m_selM != 0) && interacteur.getSelected() != null)
		{
			if(interacteur.getSelected().name == "_avatar")
		   	{
		    	interacteur.setSelected(null,true);
		   	}
		}
		
		//Recentrage UI
		scrollpos.y -= Screen.height / 2;
	}
	
	public void setBkupStateMenu(int[] _stateMenu)
	{
		m_bkupSelM = _stateMenu[0];
		m_bkupSelSM = _stateMenu[1];
		m_bkupSelT = _stateMenu[2];
	}
	
	public int[] getStateMenu()
	{
		int[] stateMenu = new int[3];
		stateMenu[0] = m_selM;
		stateMenu[1] = m_selSM;
		stateMenu[2] = m_selT;
		
		return stateMenu;
	}
	
	public void setVisibility(bool b)
	{
		if(!PC.ctxHlp.blockJustShow)
		{
			showMenu = b;
			if(!b)
			{
				if(!_firstTime || m_selM == 0)
				{
					//saveMenuState();	
				}
				else
				{
					_firstTime = false;
					m_bkupSelM = -1;
					m_bkupSelSM = 0;
					m_bkupSelT = -1;
				}
				
				if(m_objInter.getSelected() == null/* && !GetComponent<GUISubTools>().isActive() && !m_bsubtoolChoiceMaterial*/)
				{
					resetMenu();
				}
				
				if(_detachChildren)
					AttachMainNodeChildren();
			}
				
			else
			{
				print ("setvisible");
				setMenuState();
				tempShow = true;
			}
		}
	}
	
	public bool isVisible()
	{
		return showMenu;
	}
	
	public void canDisplay(bool b)
	{
		canDisp = b;
	}
	
	public bool isDisplay()
	{
		return canDisp;
	}
	
	public bool isOnUI()
	{
        Rect al = new Rect(Screen.width-100,Screen.height/2-50,100,100);

		if(showMenu)
			return PC.In.CursorOnUIs(menuGroup, al); // menuGroup.Contains(cursor) || al.Contains(cursor);
		else
		{
			if(!_cb.isVisible())
				return PC.In.CursorOnUI(al); //al.Contains(cursor);
			else
				return PC.In.CursorOnUI(al) || !_cb.getRect().Contains(PC.In.GetCursorPosInvY());
		}
	}
	
	public void init(bool video)
	{
		/*if(!video)
		{
			showMenu=true;
			GetRoot().setSelected(0);
			updateGUI(GetRoot().getSelectedItem(),0, false);
			GetRoot().getSelectedItem().setSelected(0);
			updateGUI(GetRoot().getSelectedItem().getSelectedItem(),0, false);
		}*/

		_canValidate = false;
		_firstTime = true;
	}
	
	public int getObjCameraCull()
	{
		return c_camCull;	
	}
	
	public void ShowAvatarSettings()
	{
		m_bkupSelM = 0;
		m_bkupSelSM = 6;
		m_bkupSelT = -1;
		setMenuState();
		print ("showavatarsettings");
		showMenu = true;
	}
	
	public IEnumerator gotoAccueil() // equivalent a nouveau montage
	{
		GameObject _explorer = GameObject.Find("Explorer");
		Camera ui = null;
		foreach(Transform cam in _explorer.transform)
		{
			if(cam.GetComponent<Camera>())
			{
				ui = cam.GetComponent<Camera>();
			}
		}
		//Debug.Log ("Ici camera enabled " + ui.gameObject.activeSelf);
		if(ui.gameObject.activeSelf){
			yield return new WaitForSeconds(1f);
			//Debug.Log ("Par ici");
			if(GetComponent<Montage>().saveState == Montage.states.saved)
			{ 
			   wantToSave=false;
			}else{
				wantToSave = true;
			}
			//Debug.Log ("want save:  "+wantToSave);
			StartCoroutine(gotoAccueil());
		}else if(!wantToSave){
			if(_detachChildren)
				AttachMainNodeChildren();
			UsefullEvents.fireNewMontage();//TODO > tout ce qu'il y a en dessous doit passer dans le listener
			GetRoot().setSelected(0);
			GetRoot().getSelectedItem().setSelected(6);
			//GetRoot().getSelectedItem().getSelectedItem().chgTxt(TextManager.GetText("GUIMenuRight.AvatarHide"));
			
			GetComponent<GUIMenuMain>().flushObjects();
			sc.init ();
			lc.SetLightConfig(true);
	        lc.SaveToModel();
			lc.setReflex2Zero();
			lc.saveReflexToModel();
			lc.applyPreset(0);
			if(!usefullData.lowTechnologie)
			{
				grassNode.GetComponent<GrassV2>().SetVisible(false);
				gommeNode.GetComponent<EraserV2>().SetVisible(false);
			}
			
			showMenu = false;
			canDisp=false;
	     
			GetComponent<GUIMenuLeft>().canDisplay(false);
			GetComponent<GUIMenuLeft>().ResetMenu();
	        GetComponent<GUIMenuLeft>().setVisibility(false);

			interacteur.setActived(false);
			resetMenu();			
			//StartCoroutine(resetGazonGomme());//ICI
			
			Resources.UnloadUnusedAssets();
			GetComponent<GUIStart>().mBackGrid.GetComponent<BackGridManager>().allowRandomBg(true);
			GetComponent<GUIStart>().showStart(true);
		}
	}
			
	/*public void ResetAvatarText()
	{
		if(_avatarItem!=null)
			_avatarItem.chgTxt(TextManager.GetText("GUIMenuRight.Avatar"));
	}*/

    public void Quit(bool trueQuit=false){
        if (trueQuit)
        {
            _showWarningQuit = 2;
        }
        else
        {
            _showWarningQuit = 1;
        }
    }
	
	
#endregion
}
