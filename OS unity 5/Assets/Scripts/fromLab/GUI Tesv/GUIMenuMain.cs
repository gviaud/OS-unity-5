using UnityEngine;
using System.Collections;

using Pointcube.Global;
using Pointcube.Utils;
using Pointcube.InputEvents;

public class GUIMenuMain : MonoBehaviour
{
	public float messError = 0;
    // -- Réf scène --
    public GameObject   m_backgroundImg;
    public GameObject   m_eraser;
    public GameObject   m_grass;
    public GameObject   m_mainCam;
    public GameObject   _camPivot;
    public GameObject   _grid;
    public GameObject   _avatar;
    public GameObject   m_ctxHelp;
	
	public Texture2D uiPoint,uiLine;

	ArrayList othersMenu;
	
	GameObject mainNode;
	
	private bool upperDisplay = false;
	private bool _lineDisplay=false;
	private bool _lineTxtDisplay=false;
	
	private string upperDisplayTxt = "";
	private string upperDisplayLbl = "";
	private string _lineTxt = "";
	
	private float off7 = -50;
	private float btnH = 50;
    private float btnW = 100;
	
	private const float _uiPtSize = 20;
	private const float _uiLnSize = 20;
	
	private Rect upperDisplayBG;
//	private Rect upperDisplayZone;
//	private Rect upperDisplayZone2;
	private Rect upperDisplayZone3;
	
	private Vector2 _uiLineStartPt;
	private Vector2 _uiLineEndPt;

    private GUITexture mBackGrid;       // Référence pour activer le mode montage (gestion redimensionnement fenêtre différent)

	public GUISkin skin;
	
	public enum Menu
	{
		Objects,Scene,Interaction,Configuration	
	}

    private static readonly string DEBUGTAG = "GUIMenuMain : ";
    private static readonly bool   DEBUG    = true;	
	
	private GameObject _iosShadows;
	
	private Camera _mainCam;
	
	private PayPerUseCtrl m_payperuseCtrl;
	private bool m_ppuSendCode;
	private const float c_ppuSendRetry = 10f; // équivaut a 5 min
	private float m_ppuSendCount = 0f;
	
	[HideInInspector]
	public bool bpresetPhoto =false;
	
//Fonctions-------------------------------------	
#region unity_func

    //-----------------------------------------------------
    void Awake()
    {
		if(usefullData.c_ppu)
		{
			m_payperuseCtrl = new PayPerUseCtrl(PayPerUseMdl.Instance);
			PayPerUseMdl.PayPerUseSendRequest += ActivateSendCode;
		}
		
        UsefullEvents.OnResizingWindow  += RelocateUpperDisplay;
        UsefullEvents.OnResizeWindowEnd += RelocateUpperDisplay;
        UsefullEvents.HideGUIforScreenshot += SetHideAll;
        UsefullEvents.HideGUIforBeforeAfter += SetHideAll;
		UsefullEvents.UpdateUIState += UIUpdate;
    }
	
    //-----------------------------------------------------
    void Start ()
	{
       // if(PC.DEBUG && DEBUG)       Debug.Log(DEBUGTAG+"Start");
        if(m_backgroundImg == null) Debug.LogError(DEBUGTAG+"Background Image"  +PC.MISSING_REF);
        if(m_eraser == null)        Debug.LogError(DEBUGTAG+"Eraser"            +PC.MISSING_REF);
        if(m_grass == null)         Debug.LogError(DEBUGTAG+"Grass"             +PC.MISSING_REF);
        if(m_mainCam == null)       Debug.LogError(DEBUGTAG+"MainCam"           +PC.MISSING_REF);
        if(m_ctxHelp == null)       Debug.LogError(DEBUGTAG+"CtxHelp"           +PC.MISSING_REF);
		
		_iosShadows = GameObject.Find("iosShadows");
		
		_mainCam = Camera.main;
		
		othersMenu = new ArrayList();
		foreach(Component cp in this.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType() != this.GetType() && cp.GetType().GetInterface("GUIInterface")!= null)
				othersMenu.Add(cp);
		}
		
		mainNode = GameObject.Find("MainNode");
		
		_camPivot = GameObject.Find("camPivot");
		_grid = GameObject.Find("grid");
		_avatar = GameObject.Find("_avatar");
		
        upperDisplayBG = new Rect(Screen.width/2-180, -50, 360, 100);
//        upperDisplayZone = new Rect(Screen.width/2-305, -50, 300, 50);
//        upperDisplayZone2 = new Rect(Screen.width/2+5, /*-200*/-50, 300, 50);
		upperDisplayZone3 = new Rect(Screen.width/2-300,-50,600,50);
		

        mBackGrid = GameObject.Find("back_grid").GetComponent<GUITexture>();
        if(mBackGrid == null)
            Debug.LogError(DEBUGTAG+" can't find gameobject \"back_grid\"");

		
		//RESTRICTION IPAD2
		if(usefullData.lowTechnologie)
		{
			QualitySettings.antiAliasing = 0;	
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			QualitySettings.masterTextureLimit = 1;
		}
	}
	
    //-----------------------------------------------------
    void Update ()
	{
		if(usefullData.c_ppu)
		{
			if(m_ppuSendCode)
			{
				m_ppuSendCount += Time.deltaTime;
				if(m_ppuSendCount >= c_ppuSendRetry)
				{
					m_ppuSendCount = 0f;
					StartCoroutine(PayPerUseMdl.Instance.SendCode());
				}
			}
		}
		
		if(upperDisplay)
		{
			if(off7<0)
			{
				if(off7>-1)
				{
					off7 = 0;
					upperDisplayBG.y = off7;
//					upperDisplayZone.y = off7;
//					upperDisplayZone2.y = off7;//(6*off7)+50;
					upperDisplayZone3.y = off7;
				}
				else
				{
					off7 = Mathf.Lerp(off7,0,5*Time.deltaTime);
					upperDisplayBG.y = off7;
//					upperDisplayZone.y = off7;
//					upperDisplayZone2.y = off7;//(6*off7)+50;
					upperDisplayZone3.y = off7;
				}
			}
		}
		else
		{
			if(off7>-50)
			{
				if(off7<-49)
				{
					off7 = -50;
					upperDisplayBG.y = off7;
//					upperDisplayZone.y = off7;
//					upperDisplayZone2.y = off7;//(6*off7)+50;
					upperDisplayZone3.y = off7;
				}
				else
				{
					off7 = Mathf.Lerp(off7,-50,5*Time.deltaTime);
					upperDisplayBG.y = off7;
//					upperDisplayZone.y = off7;
//					upperDisplayZone2.y = off7;// (6*off7)+50;
					upperDisplayZone3.y = off7;
				}
			}
		}
		
		//fix
		if(GetComponent<GUIStart>().enabled)
		{
			GetComponent<GUIMenuLeft>().canDisplay(false);
			GetComponent<GUIMenuRight>().canDisplay(false);
		}
		//fix

	} // Update()
	
    //-----------------------------------------------------
    void OnGUI()
	{
		
			//GUI.Label(new Rect(300,300,300,300),"m_usingTouch :"+((WinTouchInput) PC.In).m_usingTouch);
//        if(GUI.Button(new Rect(50f, 50f, 50f, 50f), PC.tIn? "Touch":"Mouse"))
//        {
//            if(PC.tIn)
//                PC.In = PC.InM;
//            else
//                PC.In = PC.InT;
//            PC.tIn = !PC.tIn;
//        }


		GUISkin bkup = GUI.skin;
		GUI.skin = skin;
		if (messError > 0) 
		{
			if(GUI.Button(new Rect(0,0,Screen.width,Screen.height),""))
				messError= 0;
			Rect m_warningRect = new Rect(Screen.width*0.1f, Screen.height*0.5f, Screen.width*0.8f, 30f);
			GUI.Label(m_warningRect,TextManager.GetText("GUIMenuMain.messError"),"ErrorMess");
			
		}

		if(upperDisplay && !GetComponent<GUIMenuConfiguration>().enabled)
		{
			GUI.Box(upperDisplayBG,"","hudBG");
//			GUI.Label(upperDisplayZone,upperDisplayLbl,"hud");
//			GUI.Label(upperDisplayZone2,upperDisplayTxt,"hud2");
			GUI.Label(upperDisplayZone3,upperDisplayLbl+" "+upperDisplayTxt,"hud3");
		}
		
		if(_lineDisplay)
		{
			Drawer.DrawLineCentered(_uiLineStartPt,_uiLineEndPt,uiLine,_uiLnSize);

//			GUI.DrawTexture(new Rect(_uiLineStartPt.x - _uiPtSize/2,_uiLineStartPt.y - _uiPtSize/2,_uiPtSize,_uiPtSize),uiPoint);
			GUI.DrawTexture(new Rect(_uiLineEndPt.x - _uiPtSize/2,_uiLineEndPt.y - _uiPtSize/2,_uiPtSize,_uiPtSize),uiPoint);
			
			if(_lineTxtDisplay)
				GUI.Label(new Rect((Screen.width - 100)/2f,Screen.height - 50,100,50),_lineTxt,"linedisptxt");
			
		}
		
		GUI.skin = bkup;

		//((WinTouchInput)PC.In).drawTouches();
//        if(Event.current.type == EventType.Repaint)
//        {
//        }
	} // OnGUI()

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= RelocateUpperDisplay;
        UsefullEvents.OnResizeWindowEnd -= RelocateUpperDisplay;
        UsefullEvents.HideGUIforScreenshot   -= SetHideAll;
        UsefullEvents.HideGUIforBeforeAfter  -= SetHideAll;
		UsefullEvents.UpdateUIState -= UIUpdate;
    }

#endregion
	
//FCN. AUX. PRIVEE--------------------
	
	private void firstTimeFct(bool video)
	{
//		yield return new WaitForSeconds(0.5f);
//		rgtPanelIndex = 2;
//		while(rgtPanelIndex ==2)
//		{
//			yield return new WaitForSeconds(0.0f);
//		}
//		lpVisible = true;
		
		if(bpresetPhoto)
		{
			GetComponent<GUIMenuLeft>().setVisibility(true);
		}
		else
		{
			GetComponent<GUIMenuRight>().init(video);
		}
		
		//yield return true;
	}
	
	private void canDisplayUI(bool b)
	{
		foreach (Component cp in othersMenu)
		{
			((GUIInterface)cp).canDisplay(b);	
		}	
	}

    //-----------------------------------------------------
    private void RelocateUpperDisplay()
    {
        upperDisplayBG.x    = Screen.width/2-180;
//        upperDisplayZone.x  = Screen.width/2-305;
//        upperDisplayZone2.x = Screen.width/2+5;
		upperDisplayZone3 = new Rect(Screen.width/2-300,-50,600,50);
    }

    //-----------------------------------------------------
    private void UpdateBackgroundRatio(int wChange, int hChange)
    {
        //_mainCam.GetComponent<loadBackground>().ratio();
    }

	//-----------------------------------------------------
	private void UIUpdate(string ui, int[] indexs)
	{
		if(ui == "GUIMenuRight" && ((indexs[0] != -1 && indexs[1] != -1) || indexs[0] == 7)/*&& indexs[1] == 6*/)
			if(_lineDisplay || _lineTxtDisplay)
			{
				_lineDisplay=false;
				_lineTxtDisplay=false;
			}
	}
	
//    //-----------------------------------------------------
//    private void UpdateGridGUItexture(int wChange, int hChange)
//    {
//        Rect r = mBackGrid.pixelInset;
//        r.width  += wChange;
//        r.height += hChange;
//        mBackGrid.pixelInset = r;
//    }


	
//FCN. AUX. PUBLIC--------------------
	
	public bool isOnUi()
	{
		bool b = false;
		
		foreach(Component cp in othersMenu)
		{
			if(((MonoBehaviour)cp).enabled)
				b = b | ((GUIInterface)cp).isOnUI();	
		}
		return b;
	}
	
	public bool isOneMenuOpen() // est ce qu'au moins un des menus est ouvert
	{
		bool b = false;
		//if(othersMenu==null)
		//	return b;
		foreach(Component cp in othersMenu)
		{
			if(((MonoBehaviour)cp).enabled)
				b = b | ((GUIInterface)cp).isVisible();	
		}
		return b;
	}
	
	public void setStarter(bool firstTime, bool video)
	{
		setStarter(firstTime, video, false);
	}
	public void setStarter(bool firstTime)
	{
		setStarter(firstTime, false);
	}

    //-----------------------------------------------------
	public void setStarter(bool firstTime, bool video, bool photothequeIPad)
	{
		if(usefullData.c_ppu)
			m_payperuseCtrl.NotifyAddToday();
		
		if(_mainCam.cullingMask == 0)//réaffichage des objets si projet>changement image
			_mainCam.cullingMask = GetComponent<GUIMenuRight>().getObjCameraCull();
		
		Texture2D t = Montage.sm.getBackground();
		GetComponent<GUIStart>().showStart(false);

        m_backgroundImg.GetComponent<BgImgManager>().SetBackgroundTexture(t);

        // Mode2D
        m_mainCam.GetComponent<Mode2D>().Reset();
		
        // Aides contextuelles
        if(!video)
            m_ctxHelp.GetComponent<CtxHelpManager>().UseStartCountdown();
        m_ctxHelp.GetComponent<CtxHelpManager>().ReinitPanelsCountdown();

        // EraserV2 & GrassV2
		if(!usefullData.lowTechnologie)
		{
	        m_eraser.GetComponent<EraserV2>().Reset();
	        m_eraser.GetComponent<EraserV2>().SetVisible(true);
	
	        m_grass.GetComponent<GrassV2>().Reset();
	        m_grass.GetComponent<GrassV2>().SetVisible(true);
		}
		if(!video)
		{
			showHideMenu(Menu.Scene,true);
			canDisplayUI(true);
		}
		else
		{
			canDisplayUI(true);
			_camPivot.GetComponent<SceneControl>().SetIpadGridPreset();
			_camPivot.GetComponent<SceneControl>().setLIPH(true);
			showHideMenu(Menu.Objects,true);
			_grid.GetComponent<Renderer>().enabled = false;
			//((Avatar)_avatar.GetComponent("Avatar")).SetForceDisplay(false);
			//GetComponent<GUIMenuRight>().ResetAvatarText();
			//GetComponent<GUIMenuLeft>().SetState(0);
		}
		if(photothequeIPad)
		{
			_camPivot.GetComponent<SceneControl>().SetIpadGridPreset();
			_camPivot.GetComponent<SceneControl>().setLIPH();			
		}
		
		if(firstTime)
		{
			GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
			firstTimeFct(video);
		}
		else
		{
			_grid.GetComponent<Renderer>().enabled = false;
			/*((Avatar)_avatar.GetComponent("Avatar")).SetForceDisplay(false);
			if(!video)
				GetComponent<GUIMenuRight>().ResetAvatarText();*/
		}

		_grid.GetComponent<Renderer>().enabled = false;
		
        mBackGrid.GetComponent<BackGridManager>().SetMontageMode();
        _mainCam.GetComponent<GuiTextureClip>().enabled = true;
		
		
		_mainCam.GetComponent<ObjInteraction>().setSelected(null);
		_mainCam.GetComponent<ObjInteraction>().setActived(true);
			//StartCoroutine(firstTime());
		
		_camPivot.GetComponent<SceneControl>().saveBkup();
        // Clean autosaves
        IOutils.CleanTmpFiles();

	} // SetStarter()

    //-----------------------------------------------------
	public void flushObjects()
	{
		GameObject go = mainNode.transform.GetChild(0).gameObject;
		while (go.name != "_avatar")
		{
	//		_iosShadows.GetComponent<IosShadowManager>().DeleteShadow(go,true);
			DestroyImmediate(go);
			
			go = mainNode.transform.GetChild(0).gameObject;
		}
		
	 	while(mainNode.transform.GetChildCount()>1)
		{
			
			GameObject g = mainNode.transform.GetChild(mainNode.transform.GetChildCount()-1).gameObject;	
			if(g.name != "_avatar")
			{
				//DestroyImmediate(giosShadow);	
//				_iosShadows.GetComponent<IosShadowManager>().DeleteShadow(g,true);
				DestroyImmediate(g);				
			}
		}
	}
	
	public void hideShowUD(bool b)
	{
		upperDisplay = b;
	}
	
	public void ActivateLineDisplay(bool line,bool txt = false)
	{
		_lineDisplay = line;
		_lineTxtDisplay = txt;
	}
	
	public void setUDText(string label,string val)
	{
		upperDisplayTxt = val;
		upperDisplayLbl = label;
	}
	
	public void UpdateLineStartEndPoint(Vector2 startPos, Vector2 targetPos,string txt = "")
	{		
		_uiLineStartPt = startPos;
		_uiLineEndPt = targetPos;
		
		_lineTxt = txt;
	}
	
//	public void SetDisplaySubTools(bool newState)
//    {
//        displaySubTools = newState;
//        if(displaySubTools)
//            folded = true;
//        else
//            folded = false;
//    }
	
//	public void SetHideHomeButton(bool newState)
//    {
//        hideHomeBtn = newState;	
//    }
	
	public void SetHideAll(bool newState)
    {
//      hideAll = newState;
		if(!newState)
		{
			foreach(Component cp in othersMenu)
			{
				((GUIInterface)cp).canDisplay(true);
			}
		}
		else
		{
			foreach(Component cp in othersMenu)
			{
				((GUIInterface)cp).setVisibility(false);
				((GUIInterface)cp).canDisplay(false);
			}
		}
    }
	
	public void CreateAllGUI()
    {
		foreach(Component cp in othersMenu)
		{
			((GUIInterface)cp).CreateGui();
		}
    }
	
//	public GUISkin GetSkin()
//	{
//		a jailler !
//		return GUI.skin;
//	}
	
	public void showHideMenu(Menu m,bool b)
	{
		System.Type t = null;
		switch (m)
		{
			case Menu.Scene:
			t = typeof(GUIMenuRight);
			break;
			case Menu.Objects:
			t = typeof(GUIMenuLeft);
			break;
			case Menu.Configuration:
			t = typeof(GUIMenuConfiguration);
			break;
			case Menu.Interaction:
			t = typeof(GUIMenuInteraction);
			break;
		}
		
		foreach(Component cp in othersMenu)
		{
			if(cp.GetType() == t)
				((GUIInterface)cp).setVisibility(b);
		}
	}
	
	private void ActivateSendCode(bool enable)
	{
		m_ppuSendCode = enable;
		if(m_ppuSendCode)
		{
			m_ppuSendCount = c_ppuSendRetry;	
		}
	}
	
	void OnApplicationQuit()
	{
		if (GetComponent<GUIMenuRight>().AllowQuit()&& GetComponent<LibraryLoader>() == null)
		{
			flushObjects();
			GetComponent<GUIStart>().showStart(true);
		}
	}
	
}
