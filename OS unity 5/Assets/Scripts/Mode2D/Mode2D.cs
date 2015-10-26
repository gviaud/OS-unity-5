using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Pointcube.Utils;
using Pointcube.Global;
using Pointcube.InputEvents;
using System.IO;

//#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//using System.Windows.Forms;
//#endif

public class Mode2D : MonoBehaviour
{
#region attributs

    // -- RÃ©fÃ©rences scÃ¨ne & ressources --
    public  GameObject          m_mainScene;
    public  GameObject          m_mainNode;
    public  GameObject          m_avatar;
    public  GameObject          m_backgroundImg;
    public  GameObject          m_backGrid;
    public  GameObject          m_mode2DTiledGrid;
    public  GameObject          m_eraserImages;
    public  GameObject          m_grassImages;
	public string m_ctxPanelManipulatePlane;
	public string m_ctxPanelManipulateObject;
	public  M2DlineManager      m_lineMgr;
    public  GuiTextureClip      m_beforeAfter;
    public  GUISkin             m_toggleSkin;
    public  Camera              m_bgCam;
    public  Camera              m_maskCam;
    public  string              m_helpTexTouchPath; // gui/help/touch
    public  string              m_helpTexMousePath; // gui/help/mouse
    public  string              m_helpTexObjFile;     // OS_HELP_OBJ
    public  string              m_helpTexLinFile;     // OS_HELP_TRACE
    public  Texture2D           m_helpBgTex;

	public Texture 				textureBackGround;
	public Texture				textureGaucheToggle;

    private MaskCreator         m_maskCreator;
    private Camera              m_mainCam;

    // -- MÃ©caniques --
    private bool                m_mode2d;            // ActivÃ© ou non
    private bool                m_movingObj;         // translation/rotation objets active
    private bool                m_invalidateClick;   // Pour un clic commencÃ© sur une UI puis maintenu

    public  float               m_zoomObjects;       // Zoom objets pour rÃ©gler Ã  l'Ã©chelle du plan
    public Vector3             m_locationObjects;   // Translation objets (X et Z)
    public float               m_rotationObjects;

    public float               m_zoomImage;         // Zoom image plan (guiTexture) pour multires
    private Rect                m_orgBgImgPixIn;     // rect image
    private Vector2             m_oldScreenSize;     // screen
	private Rect                m_savedBgImgPixIn;   // sauvegarde pour activation/dÃ©sactivation mode2D
	private Rect                m_bkupImgPixIn;
    private bool                m_resizeEnabled;     // false = resize "cachÃ©", ie. quand mode2D dÃ©sactivÃ©
    private bool                m_modePortrait;
	private bool 				m_planManipulation;
	private bool 				m_objectsManipulation;

    // -- Chargement fichiers --
    private string              m_iPadPath;
    public Texture2D           m_loadedBg;
    private bool                m_newBgImgLoaded;

    // -- Backup du mode 3D --
    private Quaternion          m_bkupAngle;
    private Transform           m_bkupPivot;
    private Vector3             m_bkupPos;
    private Rect                m_bkupRenderZone;
    private bool                m_bkupAvatarEnabled;
    private GameObject          m_bkupSelected;
    private Color               m_bkupBgImgColor;
	private int                 m_bkupCullingMask;
	
	// -- Backup du mode 2D --
	//private Vector3             m_bkupScale = new Vector3(10.0f, 10.0f, 10.0f);
	private Rect             	m_bkupBg;
	
	
	// -- Objets qui apparaÃ®ssent en mode 2D --
    public List<Transform>     m_m2dObjs;
	private List<Vector3>       m_bkupObjScl;
	private List<Vector3>       m_bkupObjPos;
	private List<Quaternion>    m_bkupObjRot;
	private List<Transform>    m_listCopy = new List<Transform>();
	//private List<GameObject>	m_objectsToHide;

    // -- Sauvegarde --
    private SceneModel          m_sceneModel;

    // -- GUI --
    private bool                m_hideUI;
	private bool				_uiLocked = false;
    private Rect                m_toggleRect;      // Toggle 2D/3D

    // -- Load panel --
    private bool                m_showLoadPanel;
    private bool                m_loadPanelFromRightMenu;
    private Rect                m_loadPanelRect;
    private Rect                m_lpLabel1Rect;
    private Rect                m_lpBtnLoadRect;
    private Rect                m_lpTextRect;
    private Rect                m_lpLabel2Rect;
    private Rect                m_lpBtnScl1Rect;
    private Rect                m_lpBtnScl2Rect;
    private Rect                m_lpBtnScl3Rect;
    private Rect                m_lpBtnOKrect;
    private Rect                m_lpBtnCancelrect;

    // -- Right menu --
    private bool                m_showRightMenu;
    private Rect                m_rGroupRect;      // -- Menu de droite --
    private Rect                m_rBgTopRect;      //      /|   (dÃ©gradÃ© top)
    private Rect                m_rBgMidRect;      //         |-(texture stretchÃ©e sur titre + btns)
	private Rect                m_rTitleRect;      //      =| | (titre)
	private Rect                m_rObjectRect;  
	private Rect                m_rImageRect; 
    private Rect                m_rBtn1TxtRect;    //           (texte btn 1)
    private Rect                m_rBtn1Rect;       //      =| | (image btn 1)
    private Rect                m_rBtn4TxtRect;    //           (texte btn 4)
    private Rect                m_rBtn4Rect;       //      =| | (image btn 4)
    private Rect                m_rBtn2TxtRect;    //           (texte btn 2)
    private Rect                m_rBtn2Rect;       //      =| | (image btn 2)
	private Rect 				m_rBtn3Rect;
	private Rect                m_rBtn3TxtRect;
	private Rect 				m_rBtnRectPlanManipulation;
	private Rect                m_rBtnTxtRectPlanManipulation;
	private Rect 				m_rBtnRectObjectsManipulation;
	private Rect                m_rBtnTxtRectObjectsManipulation;
	private Rect 				m_rBtnRectRecenter;
	private Rect                m_rBtnTxtRectRecenter;
	private Rect 				m_rBtnRectDelete;
	private Rect 				m_rBtnTxtRectDelete;
	private Rect 				m_rBtnRectResetPlan;
	private Rect                m_rBtnTxtRectResetPlan;
	private Rect				m_snapValidRect;
	public Texture 				m_validTexture;
	public Texture 				m_notValidTexture;
    //#if UNITY_IPHONE || UNITY_ANDROID
	private Rect                m_rBtn5TxtRect;    //           (texte btn 5)
    private Rect                m_rBtn5Rect;       //      =| | (image btn 5)

    //#endif
    private Rect                m_rBgBotRect;      //      \|   (dÃ©gradÃ© bot)

    private Rect                m_rListGroup;

    private Rect[]              m_uiRects;         // Liste de rectangles pour ClickOnUI

    // -- UI liste Ã©chelles de plan --
    private int[]               m_scaleList;
    private int                 m_curScaleID;
    private float               m_rescaleFactor;

    // -- UI mesures --
    private bool                m_measuring;

    // -- UI help --
    private Texture2D           m_helpTexObj;
    private Texture2D           m_helpTexLin;
    private Texture2D           m_curHelpTex;
    private Texture2D           m_nextHelpTex;
    private Rect                m_helpRect;        // image d'aide
    private bool                m_displayHelp;
    private float               m_helpTimer;
	private GUIStyle			m_buttonStyle;

    // -- warning --
    private Rect                m_warningRect;
    private bool                m_showWarning;
    private string              m_warningTxt;
    private bool                m_warningFading;
    private float               m_warningFadeTimer;
    private Color               m_warningFadeStartCol;
    private Color               m_warningFadeEndCol;
	private const float         c_warningFadeLen = 0.2f;

    // -- Transition animÃ©e help --
    private bool                m_animHelp;        // En cours ou non
    private float               m_animHelpProg;    // Progression
    private float               m_helpGUIalpha;
    private float               m_helpGUIaStart;
    private float               m_helpGUIaEnd;
    private bool                m_helpCountdown;

    private const float         c_helpCountdown = 3f;    // en secondes
    private const float         c_scaleZoomFactor          = 1.05f;//1.325f;
    private const float         c_scaleZoomFactorPortrait  = 1.5f;
    private const string        c_pluginName = "mode2d";

    // -- Debug --
    private const string DEBUGTAG = "Mode2D : ";
    private const bool   DEBUG    = true;

    private bool m_imgLoading = false;

	public Texture iconPlan;
	public Texture iconObjet;
	public Texture iconImage;

#endregion
#region unity_func
    //-----------------------------------------------------
    void Awake()
    {
        ModulesAuthorization.moduleAuth       += AuthorizePlugin;
        UsefullEvents.HideGUIforScreenshot    += HideUI;
        UsefullEvents.HideGUIforBeforeAfter   += HideUI;
        UsefullEvents.OnResizingWindow        += ResizeRectsToScreen;
        UsefullEvents.OnResizeWindowEnd       += ResizeRectsToScreen;
		UsefullEvents.LockGuiDialogBox        += LockUI;
		
#if UNITY_IPHONE

		EtceteraManager.imagePickerChoseImageEvent += OpenFileIpad;
		EtceteraManager.imagePickerCancelledEvent  += OpenFileIpadFailed;
#endif
#if UNITY_ANDROID
        EtceteraAndroidManager.albumChooserSucceededEvent	+= OpenFileAndroid;
        EtceteraAndroidManager.albumChooserCancelledEvent 	+= OpenFileAndroidFailed;
#endif
    }

    //-----------------------------------------------------
    void Start()
    {


        //if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log(DEBUGTAG+"Start");

        if(m_mainScene == null)         Debug.LogError(DEBUGTAG+"MainScene"+        PC.MISSING_REF);
        if(m_mainNode == null)          Debug.LogError(DEBUGTAG+"MainNode"+         PC.MISSING_REF);
        if(m_avatar == null)            Debug.LogError(DEBUGTAG+"Avatar"+           PC.MISSING_REF);
        if(m_bgCam == null)             Debug.LogError(DEBUGTAG+"Background camera"+PC.MISSING_REF);
        if(m_toggleSkin == null)        Debug.LogError(DEBUGTAG+"MainScene"+        PC.MISSING_REF);
        if(m_maskCam == null)           Debug.LogError(DEBUGTAG+"MaskCam"+          PC.MISSING_REF);
        if(m_backgroundImg == null)     Debug.LogError(DEBUGTAG+"Background image"+ PC.MISSING_REF);
        if(m_beforeAfter == null)       Debug.LogError(DEBUGTAG+"Before after"+     PC.MISSING_REF);
        if(m_backGrid == null)          Debug.LogError(DEBUGTAG+"Back Grid"+        PC.MISSING_REF);
        if(m_mode2DTiledGrid == null)   Debug.LogError(DEBUGTAG+"Mode2D Tiled grid"+PC.MISSING_REF);
        if(m_eraserImages == null)      Debug.LogError(DEBUGTAG+"EraserImages"+     PC.MISSING_REF);
        if(m_grassImages == null)       Debug.LogError(DEBUGTAG+"GrassImages"+      PC.MISSING_REF);
        if(m_lineMgr == null)           Debug.LogError(DEBUGTAG+"LineMananger"+     PC.MISSING_REF);
        if(m_helpBgTex == null)         Debug.LogError(DEBUGTAG+"Help Bg Tex"+      PC.MISSING_REF);

        // -- Chargement auto des images d'aide, selon plateforme --
        string path = "";
        if(UnityEngine.Application.platform == RuntimePlatform.Android ||
           UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer)
            path = "images_multilangue/"+PlayerPrefs.GetString("language")+"/"+m_helpTexTouchPath+"/";      // Aide Touchpad
        else
            path = "images_multilangue/"+PlayerPrefs.GetString("language")+"/"+m_helpTexMousePath+"/";      // Aide Souris

        m_helpTexObj = (Texture2D) Resources.Load(path+m_helpTexObjFile, typeof(Texture2D));
		if(m_helpTexObj == null)   Debug.LogError( DEBUGTAG+path+m_helpTexObjFile+" "+ PC.MISSING_RES);
        m_helpTexLin = (Texture2D) Resources.Load(path+m_helpTexLinFile, typeof(Texture2D));
        if(m_helpTexLin == null)   Debug.LogError(DEBUGTAG+path+m_helpTexLinFile+" "+ PC.MISSING_RES);

        // -- Initialisations --
        m_maskCreator       = (MaskCreator) m_maskCam.GetComponent<MaskCreator>();
                            
        m_mainCam           = GetComponent<Camera>();
                            
        m_movingObj         = false;
        m_invalidateClick   = false;

        m_zoomImage         = 1f;
        m_orgBgImgPixIn     = new Rect();
        m_oldScreenSize     = new Vector2();
        m_savedBgImgPixIn   = new Rect();

        // -- Scale list --
        m_scaleList         = new int[]{200, 500, 650};
        m_curScaleID        = 0;
        m_rescaleFactor     = 1f;

        m_zoomObjects       = m_scaleList[0]*m_rescaleFactor*(m_modePortrait ? c_scaleZoomFactorPortrait : c_scaleZoomFactor);
        m_rotationObjects   = 0f;
        m_locationObjects   = Vector3.zero;

        m_measuring         = false;

        m_iPadPath          = "";
        m_loadedBg          = null;
        m_newBgImgLoaded    = false;

        m_bkupAngle         = new Quaternion();
        m_bkupPivot         = transform.parent.transform;
        m_bkupPos           = new Vector3();
        m_bkupRenderZone    = new Rect();
        m_bkupAvatarEnabled = false;
        m_bkupSelected      = null;
        m_bkupBgImgColor    = new Color();
        m_bkupCullingMask   = m_mainCam.cullingMask;

        m_m2dObjs           = new List<Transform>();
		m_bkupObjScl        = new List<Vector3>();
		m_bkupObjPos        = new List<Vector3>();
        m_bkupObjRot        = new List<Quaternion>();
	//	m_objectsToHide 	= new List<GameObject>();

        m_hideUI            = false;
        GUIStyle style      = m_toggleSkin.GetStyle("toggle_2d3d");
        m_toggleRect        = new Rect(0, 0, style.fixedHeight, style.fixedWidth);
        float sw            = Screen.width;
        float sh            = Screen.height;

        // -- Load panel --
        m_loadPanelFromRightMenu = false;
        m_showLoadPanel     = false;
        m_loadPanelRect     = new Rect((sw-250f)/2f, (sh-350f)/2f, 250f, 320f);
        m_lpLabel1Rect      = new Rect( 20f,  20f, 210f,  40f);
        m_lpBtnLoadRect     = new Rect( 20f,  60f, 210f,  50f);
        m_lpTextRect        = new Rect( 20f, 110f, 210f,  20f);
        m_lpLabel2Rect      = new Rect( 20f, 140f, 210f,  40f);
        m_lpBtnScl1Rect     = new Rect( 20f, 180f,  70f,  30f);
        m_lpBtnScl2Rect     = new Rect( 90f, 180f,  70f,  30f);
        m_lpBtnScl3Rect     = new Rect(160f, 180f,  70f,  30f);
        m_lpBtnOKrect       = new Rect( 20f, 250f, 105f,  40f);
        m_lpBtnCancelrect   = new Rect(125f, 250f, 105f,  40f);

        // -- Right menu --
        int menuW           = 175;
        m_showRightMenu     = false;
        m_rBgTopRect       		= new Rect(menuW,   0f, -menuW, 150f);            // Menu de droite
		m_rTitleRect        	= new Rect(   0f, 100f,  menuW,  50f);
		
		m_rBtn1TxtRect      	= new Rect(   0f, 150f,  menuW,  50f);
		m_rBtn1Rect         	= new Rect(   0f, 150f,  menuW,  50f);
		
		m_rBtnTxtRectPlanManipulation 		= new Rect(   0f, 200f,  menuW,  50f);
		m_rBtnRectPlanManipulation 			= new Rect(   50f, 200f,  menuW,  50f);
		
		m_rBtnRectResetPlan		= new Rect(   0f, 250f,  menuW,  50f);
		m_rBtnTxtRectResetPlan	= new Rect(   0f, 250f,  menuW,  50f);
		
		m_rBtnRectDelete		= new Rect(   0f, 300f,  menuW,  50f);
		m_rBtnTxtRectDelete		= new Rect(   0f, 300f,  menuW,  50f);
		
		m_rObjectRect        	= new Rect(   0f, 350f,  menuW,  50f);
		
		m_rBtnTxtRectObjectsManipulation 	= new Rect(   0f, 400f,  menuW,  50f);
		m_rBtnRectObjectsManipulation 		= new Rect(   50f, 400f,  menuW,  50f);
		
		m_rBtnTxtRectRecenter	= new Rect(   0f, 450f,  menuW,  50f);
		m_rBtnRectRecenter		= new Rect(   0, 450f,  menuW,  50f);
		
		m_rBtn4TxtRect      	= new Rect(   0f, 500f,  menuW,  50f);
		m_rBtn4Rect         	= new Rect(   0f, 500f,  menuW,  50f);
		
		m_rImageRect        	= new Rect(   0f, 500f,  menuW,  50f);
		
		m_rBtn2TxtRect      	= new Rect(   0f, 550f,  menuW,  50f);
        m_rBtn2Rect         	= new Rect(   0f, 550f,  menuW,  50f);
        
		m_rBtn3Rect 			= new Rect(   0f, 600f,  menuW,  50f);
	    m_rBtn3TxtRect  		= new Rect(   0f, 600f,  menuW,  50f);
		
		//m_validTexture=(Texture)Resources.Load("gui/check");
        #if UNITY_IPHONE || UNITY_ANDROID
			m_rBtn5Rect 	= new Rect(		0f, 700f,  menuW,  50f);
	  	 	m_rBtn5TxtRect  = new Rect(    	0f, 700f,  menuW,  50f);
            m_rBgMidRect    = new Rect(	 menuW, 150f, -menuW, 550f);
            m_rBgBotRect    = new Rect(  menuW, 700f, -menuW, 150f);
            m_rGroupRect    = new Rect(		0f,   0f,  menuW, 800f);
        #else
            m_rBgMidRect    = new Rect( menuW, 150f, -menuW, 500f);
            m_rBgBotRect    = new Rect( menuW, 650f, -menuW, 150f);
            m_rGroupRect    = new Rect(    0f,   0f,  174, Screen.height);
        #endif

        // -- Rects UI --
        m_uiRects           = new Rect[2];
        for(int i=0; i<m_uiRects.Length; i++)
            m_uiRects[i] = new Rect(0f, 0f, 0f, 0f);

        // -- Warning --
        m_warningRect       = new Rect(0f, Screen.height*0.8f, Screen.width, 30f);
        m_showWarning       = false;
        m_warningTxt        = "";
        m_warningFading     = false;
        m_warningFadeTimer  = 0f;
        m_warningFadeStartCol = Color.clear;
        m_warningFadeEndCol = Color.clear;

        // -- Help --
        m_curHelpTex        = null;
        m_nextHelpTex       = null;
        m_helpRect          = new Rect();
        m_helpTimer         = 0f;
        m_displayHelp       = false;

        m_animHelp          = false;
        m_animHelpProg      = 0f;
        m_helpGUIalpha      = 0f;
        m_helpGUIaStart     = 0f;
        m_helpGUIaEnd       = 0f;
        m_helpCountdown     = false;

        m_resizeEnabled     = false;
        m_modePortrait      = false;
        ResizeRectsToScreen();

        m_sceneModel        = Montage.sm;
        
    }  // Start()
	
    //-----------------------------------------------------
    void Update()
    {
        if(!IsActive())
            return;
		//ConstantHide();
		
        if(m_movingObj) // CAMERA
		{
            if(PC.In.ClickOnUI(m_uiRects))
                m_invalidateClick = true;

            Vector2 deltaMove = Vector2.zero;
            float deltaRota = 0f;
            if(m_invalidateClick)
            {
                if(PC.In.ClickEnded())
                    m_invalidateClick = false;
            }
            else
			{
				if(m_planManipulation)
				{
					float ftempz = 0.0f;
					if(PC.In.Zoom(out ftempz))
					{
						float itemp = ftempz > 0.0f ? 1 : -1;
						GUITexture guiTexture = m_backgroundImg.GetComponent<GUITexture>();
						
						if(guiTexture)
						{
							Rect rectemp = guiTexture.pixelInset;
							float fratio = (rectemp.width / rectemp.height) * itemp * 0.02f;
							
							float fwidth = (rectemp.width * fratio);
							float fheight = (rectemp.height * fratio);
							
							rectemp.width += fwidth;
							rectemp.height += fheight;
							
							rectemp.x -= fwidth * 0.5f;
							rectemp.y -= fheight * 0.5f;
							
							guiTexture.pixelInset = rectemp;
							m_bkupBg = rectemp;

							m_lineMgr.DeleteAllLines();
						}
					}
					
					Vector2 deltaMoveBg = Vector2.zero;
					if(PC.In.Drag1(out deltaMoveBg))
					{
						GUITexture guiTexture = m_backgroundImg.GetComponent<GUITexture>();
						
						if(guiTexture)
						{
							Rect rectemp = guiTexture.pixelInset;
							
							rectemp.x += deltaMoveBg.x;
							rectemp.y += deltaMoveBg.y;
							
							guiTexture.pixelInset = rectemp;
							m_bkupBg = rectemp;
		
							
							m_lineMgr.DeleteAllLines();
						}
					}
				}
                else if(!(PC.In.Click1Down() && PC.In.CursorOnUI(m_uiRects)))
                {
                	if(!m_measuring)
                	{
                		m_objectsManipulation = true;
                	}
                	
                    // -- DÃ©placement objets --
					if(PC.In.Drag1(out deltaMove) && (deltaMove.x != 0f || deltaMove.y != 0f) && m_listCopy.Count > 0)
                    {
                        Vector3 dummy = GetObjsGravityCenter();
                        Vector3 move = _3Dutils.ScreenMoveTo3Dmove(-deltaMove, dummy, true);
                        MoveObjects(dummy.x - move.x, dummy.z - move.z);
                        SaveModel();
                    }
                    // -- Rotation objets --
                    if(PC.In.Rotate(out deltaRota) && deltaRota != 0f)
                        RotateObjects(-deltaRota);
					
					// -- Scale objets --
					float ftemp = 0.0f;
					if(PC.In.Zoom(out ftemp))
					{
						ScaleObjects((int)(ftemp * 10.0f));
					}
                }
            }
        } // moving obj

        if(Camera.main.GetComponent<ObjInteraction>().enabled) // Quick fix pour Ã©viter que objInteraction se rÃ©active
            Camera.main.GetComponent<ObjInteraction>().enabled = false;   // Quand on fait un alt-tab
        if(Camera.main.GetComponent<GuiTextureClip>().enabled) // Quick fix pour Ã©viter que avant apres se rÃ©active
            Camera.main.GetComponent<GuiTextureClip>().enabled = false;   // Apres un envoit d'email

        // -- Masquer l'aide --
        if(PC.In.Click1Down() || PC.In.Click2Down())
        {
            if(m_showWarning)
                HideWarning();
            HideHelp();
        }
    }
    //-----------------------------------------------------
    void FixedUpdate()
    {
        if(m_animHelp)
        {
            if(m_animHelpProg <= PC.T_ANIM_LEN)
            {
                m_helpGUIalpha = Mathf.Lerp(m_helpGUIaStart, m_helpGUIaEnd,
                                  ((-Mathf.Cos(m_animHelpProg/PC.T_ANIM_LEN*2*Mathf.Acos(0))+1)/2));
                m_animHelpProg++;
            }
            else
            {
                m_animHelpProg = 0f;
                m_animHelp = false;

                if(m_helpGUIalpha == 0f)
                {
                    m_displayHelp = false;    // DÃ©sactiver si invisible en fin de transition
                    if(m_nextHelpTex != null) ChangeHelpImg(m_nextHelpTex);
                }
            }
        } // anim_Help
    }
    //-----------------------------------------------------
    void OnGUI()
    {
		if (m_mode2d) 
		{
			GUI.DrawTexture (new Rect (0, 0, textureGaucheToggle.width - 25, Screen.height), textureGaucheToggle);
		}
        GUI.skin = m_toggleSkin;

        if(m_hideUI)
            return;

		if(!m_mainScene.GetComponent<GUIStart>().enabled &&
		 !m_mainScene.GetComponent<HelpPanel>()._showUpgrade &&
		 !m_mainScene.GetComponent<HelpPanel>()._showAbout)
        {
			if(usefullData._edition == usefullData.OSedition.Full)
			{

				bool tmp = false;

				GUI.depth = -1;

				bool mode2DActivate = false;
				foreach(Transform t in m_m2dObjs)
				{
					if( t != null)
					{
						OSLibObject obj = t.transform.GetComponent<ObjData>().GetObjectModel();
						//print ("obj.GetObjectType : "+ obj.GetObjectType ());
						if(obj.GetObjectType () == "pool" || obj.GetObjectType () == "dynamicShelter" || obj.IsMode2D ())
						{
							mode2DActivate = true;
							break;
						}
					}
				}

				/*
				foreach(Transform t in m_listCopy)
				{
					OSLibObject obj = t.transform.GetComponent<ObjData>().GetObjectModel();
					print ("obj.GetObjectType ()");
					if(obj.GetObjectType () == "pool" || obj.GetObjectType () == "abris")
					{
						mode2DActivate = true;
						break;
					}
				}
*/
				/*
				public List<Transform>     m_m2dObjs;
				private List<Vector3>       m_bkupObjScl;
				private List<Vector3>       m_bkupObjPos;
				private List<Quaternion>    m_bkupObjRot;
				private List<Transform>    m_listCopy = new List<Transform>();
				 * */


				if( mode2DActivate)
	           		tmp = GUI.Toggle(m_toggleRect, m_mode2d, TextManager.GetText("mode2D.mode2D"),"toggle_2d3d");
				GUI.depth = 0;

				if(m_mode2d != tmp && !_uiLocked)
	            {

	                m_mode2d = tmp;
	                StartCoroutine(ActivateMode2D_async(m_mode2d));

				}
			}

            if(m_mainScene.GetComponent<GUIMenuMain>().isOneMenuOpen())
                return;

            if(m_mode2d)
            {
				//GUI.DrawTexture(new Rect(0,0,textureGaucheToggle.width-25,Screen.height),textureGaucheToggle);
                if(m_showLoadPanel)
                {
                    GUI.BeginGroup(m_loadPanelRect, "", "LoadPanelBg");

                    GUI.Label(m_lpLabel1Rect, TextManager.GetText("Mode2D.PlanA4"), "LoadPanelLbl");

                    string txt = TextManager.GetText("Mode2D.LoadPlan");
                    if(m_loadedBg != null)
                        txt = TextManager.GetText("Mode2D.LoadNewPlan");
                    if(GUI.Button(m_lpBtnLoadRect, txt, "LoadPanelBtn"))
                        LoadImage();
                    string linkTxt = TextManager.GetText("Mode2D.CadastreTxt");
                    Vector2 centerBk = m_lpTextRect.center;
                    m_lpTextRect.width = GUI.skin.GetStyle("LoadPanelURLTxt").CalcSize(new GUIContent(linkTxt)).x;
                    m_lpTextRect.center = centerBk;
                    if(GUI.Button(m_lpTextRect, linkTxt, "LoadPanelURLTxt"))
                        Application.OpenURL("http://www.cadastre.gouv.fr/");
                    GUI.Label(m_lpLabel2Rect, TextManager.GetText("Mode2D.Scale"), "LoadPanelLbl");

                    for(int i=0; i<m_scaleList.Length; i++)
                    {
                        bool curVal = (m_curScaleID == i);
                        Rect r = new Rect(20f+70f*i, 180f,  70f,  30f);
                        bool toggleVal = GUI.Toggle(r, curVal, "1/"+m_scaleList[i], "LoadPanelToggle");
                        if(toggleVal != curVal)
                        {
                            m_curScaleID = i;
                            float zoomFactor = (m_modePortrait ? c_scaleZoomFactorPortrait : c_scaleZoomFactor);
                            SetObjZoom(m_scaleList[m_curScaleID]*zoomFactor*m_rescaleFactor);
                            CheckAndRecenterObjs();
                        }
                    }
				
					if(usefullData._edition == usefullData.OSedition.Full)
					{
						if(GUI.Button(m_lpBtnOKrect, TextManager.GetText("Mode2D.OK"), "LoadPanelBtn"))
	                    {
	                        m_showLoadPanel = false;
	                        m_showRightMenu = true;
	                        SetMovingObjects(true);
	                    }
	                    if(GUI.Button(m_lpBtnCancelrect, TextManager.GetText("Mode2D.Cancel"), "LoadPanelBtn"))
	                    {
	                        if(m_loadPanelFromRightMenu)
	                        {
	                            m_showLoadPanel = false;
	                            m_showRightMenu = true;
	                            SetMovingObjects(true);
	                        }
	                        else
	                        {
	                            m_mode2d = !m_mode2d;
	                            StartCoroutine(ActivateMode2D_async(false));
	                        }
	                    }
					}

                    GUI.EndGroup();
                }
                if(m_showRightMenu)
                {
					if(m_buttonStyle == null){
						m_buttonStyle = "helpButton";
					}
					//GUI.Button(new Rect(m_rBtnRectObjectsManipulation.x, m_rBtnRectObjectsManipulation.y, 50.0f, 50.0f), "", m_buttonStyle);

                    // -- GUI Droite --
                    GUI.BeginGroup(m_rGroupRect);
					GUI.DrawTexture(new Rect ( 0, 0,textureBackGround.width-85,Screen.height),textureBackGround);
                    GUI.Label(m_rBgTopRect, "", "bg_fadeTop");
					GUI.Label(m_rBgMidRect, "", "bg_fadeMid");

					GUI.Button(m_rTitleRect,TextManager.GetText("Mode2D.PlanA4"),"TittlePlan");
					GUI.Button(m_rObjectRect,TextManager.GetText("Mode2D.Object"),"TittleObjet");
					GUI.Button(m_rImageRect,TextManager.GetText("Mode2D.Image"),"TittleImage");
		
                    Matrix4x4 bkup = GUI.matrix;
                    //GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), m_rBtn1Rect.center);
                    if(GUI.Button(m_rBtn1Rect, "", "btnD"))
                    {
                        m_showRightMenu = false;
                        m_showLoadPanel = true;
                        m_loadPanelFromRightMenu = true;
                        HideHelp();
                        SetMeasuring(false);
                        SetMovingObjects(false);

                    }
                    GUI.matrix = bkup;
                    GUI.Label(m_rBtn1TxtRect, TextManager.GetText("Mode2D.MapParams"), "textbtnD");
					
					bool btemp = m_planManipulation;
					bkup = GUI.matrix;
					//GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), m_rBtnRectPlanManipulation.center);
					btemp = GUI.Toggle(new Rect(m_rBtnRectPlanManipulation.x, m_rBtnRectPlanManipulation.y, m_rBtnRectPlanManipulation.width - 50.0f, m_rBtnRectPlanManipulation.height), m_planManipulation, "","btnD");
					
					if(btemp != m_planManipulation)
					{
						m_planManipulation = btemp;
						m_objectsManipulation = !m_planManipulation;
						SetMeasuring(false);
						m_movingObj = true;
						
						if(m_lineMgr.getIsLineStarted())
						{
							m_lineMgr.removeStartedLine();
						}
					}
					GUI.matrix = bkup;
					GUI.Label(m_rBtnTxtRectPlanManipulation, TextManager.GetText("Mode2D.Manipulate"), m_planManipulation ? "TextBtnDselected" : "textbtnD");
					if (m_planManipulation)
					{
						m_buttonStyle = "helpButtonToggleOn";
					}
					else
					{
						m_buttonStyle = "helpButton";
						
					}
					if(GUI.Button(new Rect(0, m_rBtnRectPlanManipulation.y, 50.0f, 50.0f), "", m_buttonStyle))
					{

						PC.ctxHlp.ShowCtxHelp("ctx1_manip_plane");
						PC.ctxHlp.ShowHelp();
					}
					
					
					bkup = GUI.matrix;
					//GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), m_rBtnRectObjectsManipulation.center);
					btemp = GUI.Toggle(new Rect(m_rBtnRectObjectsManipulation.x, m_rBtnRectObjectsManipulation.y, m_rBtnRectObjectsManipulation.width - 50.0f, m_rBtnRectObjectsManipulation.height), m_objectsManipulation, "","btnD");
					
					if(btemp != m_objectsManipulation)
					{
						m_planManipulation = false;
						m_objectsManipulation = true;
						SetMeasuring(false);
						m_movingObj = true;
						
						if(m_lineMgr.getIsLineStarted())
						{
							m_lineMgr.removeStartedLine();
						}
					}
					GUI.matrix = bkup;
					GUI.Label(m_rBtnTxtRectObjectsManipulation, TextManager.GetText("Mode2D.Manipulate"), m_objectsManipulation ? "TextBtnDselected" : "textbtnD");
					if (m_objectsManipulation)
					{
						m_buttonStyle = "helpButtonToggleOn";
					}
					else
					{
						m_buttonStyle = "helpButton";

					}
					if(GUI.Button(new Rect(0, m_rBtnRectObjectsManipulation.y, 50.0f, 50.0f), "", m_buttonStyle))
					{
						PC.ctxHlp.ShowCtxHelp("ctx1_manip_objet");
						PC.ctxHlp.ShowHelp();
					}

					bkup = GUI.matrix;
					//GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), m_rBtnRectRecenter.center);
					if(GUI.Toggle(m_rBtnRectRecenter, false, "","btnD"))
					{
						CenterObjects();
						
						if(m_lineMgr.getIsLineStarted())
						{
							m_lineMgr.removeStartedLine();
						}
						
						SetMeasuring(false);
						m_movingObj = true;
					}
					GUI.matrix = bkup;
					GUI.Label(m_rBtnTxtRectRecenter, TextManager.GetText("Mode2D.Recenter"), "textbtnD");
					
					bkup = GUI.matrix;
					//GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), m_rBtnRectResetPlan.center);
					if(GUI.Toggle(m_rBtnRectResetPlan, false, "","btnD"))
					{
						resetPlan();
					}
					GUI.matrix = bkup;
					GUI.Label(m_rBtnTxtRectResetPlan, TextManager.GetText("Mode2D.Recenter"), "textbtnD");
					
					bkup = GUI.matrix;
					//GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), m_rBtnRectDelete.center);
					if(GUI.Toggle(m_rBtnRectDelete, false, "","btnD"))
					{
						deletePlan();
					}
					GUI.matrix = bkup;
					GUI.Label(m_rBtnTxtRectDelete, TextManager.GetText("Mode2D.Delete"), "textbtnD");

                    bkup = GUI.matrix;
                    GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), m_rBtn4Rect.center);
                   /* bool toggleVal = GUI.Toggle(m_rBtn4Rect, m_measuring, "","btnD");            // TOGGLE MEASURE
                    if(toggleVal != m_measuring)
                    {
						m_planManipulation = false;
						m_objectsManipulation = false;
						SetMeasuring(!m_measuring);
                        SetMovingObjects(!m_measuring);
                        /*if(m_measuring || m_m2dObjs.Count > 0)
                            LaunchHelpCountdown();*/
					//}
					
					GUI.matrix = bkup;					
					string style = (m_measuring ? "TextBtnDselected" : "textbtnD");
					//GUI.Label(m_rBtn4TxtRect, TextManager.GetText("Mode2D.Measure"), style);			
					bkup = GUI.matrix;
					//GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), m_rBtn2Rect.center);
					
					 if(GUI.Button(m_rBtn2Rect, "","btnD")) // SCREENSHOT BUTTON
                        SaveScreenshot();
                    GUI.matrix = bkup;
					GUI.Label(m_rBtn2TxtRect, TextManager.GetText("Mode2D.Save"), "textbtnD");
                    bkup = GUI.matrix;
                    #if UNITY_IPHONE || UNITY_ANDROID
	                bkup = GUI.matrix;
	                GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), m_rBtn3Rect.center);
	                if(GUI.Button(m_rBtn3Rect, "","btnD"))
							SendScreenshot();
	                    //StartCoroutine(m_mainCam.GetComponent<Screenshot>().mailShot());
	                GUI.matrix = bkup;
	                GUI.Label(m_rBtn3TxtRect, TextManager.GetText("GUIMenuRight.loadImage"), "textbtnD");
                    #endif
                    
					GUI.Label(m_rBgBotRect, "", "bg_fadeBot");
                  
                    GUI.EndGroup();

                } // showRightMenu

                // -- Help Panel --
                if(m_helpCountdown)
                {
                    m_helpTimer += Time.deltaTime;
                    if(m_helpTimer >= c_helpCountdown)
                    {
                        ShowHelp();
                        m_helpCountdown = false;
                    }
                } // helpCountdown

                if(m_displayHelp)
                {
                    Color bkupCol = GUI.color;
                    Color newCol  = GUI.color;
                    newCol.a = m_helpGUIalpha;
                    GUI.color = newCol;
                    GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), m_helpBgTex, ScaleMode.StretchToFill);
//                    GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), m_helpBgTex, "HelpBg");
                    GUI.DrawTexture(m_helpRect, m_curHelpTex);
                    GUI.color = bkupCol;
                } // display HelpPanel

                // -- Warning --
                if(m_showWarning)
                {
                    Color bkCol = GUI.color;
                    if(m_warningFading)
                    {
                        float progress = (Time.time - m_warningFadeTimer)/c_warningFadeLen;
                        GUI.color = Color.Lerp(m_warningFadeStartCol, m_warningFadeEndCol, progress);
                        if(progress > 1f)
                        {
                            m_warningFading = false;
                            if(GUI.color.a <= 0f)
                                m_showWarning = false;
                        }
                    }
                    //GUI.Label(m_warningRect, m_warningTxt, "WarningMsg");
                    GUI.color = bkCol;
                }
            }
        } // if GUIStart not enabled

    }  // OnGUI()

    //-----------------------------------------------------
    void OnDestroy()
    {
        ModulesAuthorization.moduleAuth       -= AuthorizePlugin;
#if UNITY_IPHONE
        EtceteraManager.imagePickerChoseImageEvent -= OpenFileIpad;
		EtceteraManager.imagePickerCancelledEvent  -= OpenFileIpadFailed;
#endif
#if UNITY_ANDROID
        EtceteraAndroidManager.albumChooserSucceededEvent	-= OpenFileAndroid;
        EtceteraAndroidManager.albumChooserCancelledEvent 	-= OpenFileAndroidFailed;
#endif
        UsefullEvents.HideGUIforScreenshot    -= HideUI;
        UsefullEvents.HideGUIforBeforeAfter   -= HideUI;
        UsefullEvents.OnResizingWindow        -= ResizeRectsToScreen;
        UsefullEvents.OnResizeWindowEnd       -= ResizeRectsToScreen;
		UsefullEvents.LockGuiDialogBox  	  -= LockUI;
    }

#endregion
#region help_panel

    //-----------------------------------------------------
    private void LaunchHelpCountdown()
    {
        m_helpCountdown = true;
        m_helpTimer     = 0f;
    }
    //-----------------------------------------------------
    private void ShowHelp()
    {
        if(!m_displayHelp)
        {
            m_displayHelp = true;
            m_helpGUIaStart = m_helpGUIalpha;
            m_helpGUIaEnd = 1f;
            m_animHelp = true;
            m_animHelpProg = 0f;
        }
    }
    //-----------------------------------------------------
    private void HideHelp()
    {
        m_helpCountdown = false;

        if(m_displayHelp)
        {
            m_helpGUIaStart = m_helpGUIalpha;
            m_helpGUIaEnd = 0f;
            m_animHelp = true;
            m_animHelpProg = 0f;
        }
    }
	public bool IsHelpDisplayed()
	{
		return m_displayHelp;
	}
    //-----------------------------------------------------
    private void ChangeHelpImg(Texture2D newHelpImg)
    {
        if(!m_displayHelp)
        {
            m_curHelpTex      = newHelpImg;
            m_helpRect.x      = (UnityEngine.Screen.width-m_curHelpTex.width)/2f;
            m_helpRect.y      = (UnityEngine.Screen.height-m_curHelpTex.height)/2f;
            m_helpRect.width  = m_curHelpTex.width;
            m_helpRect.height = m_curHelpTex.height;
        }
        else
            m_nextHelpTex = newHelpImg; // si image d'aide visible ne pas changer tout de suite
    }
#endregion
#region multi_res
    //-----------------------------------------------------
    // Note : Quand mode2D dÃ©sactivÃ©, resize fait quand mÃªme mais pas directement sur la guiTexture
    //          => sur le rect m_savedBgImgPixIn Ã  la place
    private void ResizeRectsToScreen()
    {
        // -- GUI --
        m_rGroupRect.x = UnityEngine.Screen.width-m_rGroupRect.width;
        m_rGroupRect.y = (UnityEngine.Screen.height-m_rGroupRect.height)/2;

        if(m_curHelpTex != null)
        {
            m_helpRect.x = (UnityEngine.Screen.width-m_curHelpTex.width)/2f;
            m_helpRect.y = (UnityEngine.Screen.height-m_curHelpTex.height)/2f;

			float sw = Screen.width;
        	float sh = Screen.height;
			m_loadPanelRect = new Rect((sw-250f)/2f, (sh-350f)/2f, m_curHelpTex.width, m_curHelpTex.height);
		}
        // -- viewport --
        m_mainCam.pixelRect = new Rect(0f, 0f, Screen.width, Screen.height);

        m_rListGroup.x = m_rGroupRect.x - m_rListGroup.width;

        if(m_loadedBg != null)
        {
            if(m_resizeEnabled)
                m_backgroundImg.GetComponent<GUITexture>().pixelInset = AdaptBgImgToScreen(m_backgroundImg.GetComponent<GUITexture>().pixelInset);
            else
                m_savedBgImgPixIn = AdaptBgImgToScreen(m_savedBgImgPixIn);


//            Debug.Log("resizing, image org rect = "+m_orgBgImgPixIn+", current = "+m_backgroundImg.guiTexture.pixelInset);

            SaveModel();
        }
        m_warningRect = new Rect(0f, Screen.height*0.8f, Screen.width, 30f);

        // -- UI rects --
        RefreshUIrects();
    }
    //-----------------------------------------------------
    private void RefreshUIrects()
    {
        int btnCount = 7;
#if UNITY_IPHONE || UNITY_ANDROID
        btnCount += 1;
#endif
        m_uiRects[0].Set(m_rBtn1Rect.x + m_rGroupRect.x, m_rBtn1Rect.y + m_rGroupRect.y,
                         m_rBtn1Rect.width, m_rBtn1Rect.height*btnCount);

//        m_uiRects[1].Set(m_lBtn1Rect.x + m_lGroupRect.x, m_lBtn1Rect.y + m_lGroupRect.y, // old left menu rect
//                         m_lBtn1Rect.width, m_lBtn1Rect.height + m_lBtn2Rect.height);

        m_uiRects[1].Set(m_toggleRect.x, m_toggleRect.y, m_toggleRect.width, m_toggleRect.height);
    }
    //-----------------------------------------------------
    private Rect AdaptBgImgToScreen(Rect bgImgRect)
    {
        // -- Recentrage de l'image --
        Vector2 centerDelta = new Vector2((Screen.width-m_oldScreenSize.x)/2,
                                          (Screen.height-m_oldScreenSize.y)/2);
//        Rect bgImgRect    = m_backgroundImage.guiTexture.pixelInset;
        bgImgRect.x      += centerDelta.x;       // le centre de l'image est dÃ©placÃ© autant que le
        bgImgRect.y      += centerDelta.y;       // centre de l'Ã©cran a Ã©tÃ© dÃ©placÃ© aussi

        // -- Rescale de l'image --
//        if(Screen.height != m_oldScreenSize.y)
        {
            float sizeDeltaRatio = Screen.height/m_oldScreenSize.y;
            m_zoomImage *= sizeDeltaRatio;
            bgImgRect = RescaleBgImg(bgImgRect, true, sizeDeltaRatio);
        }

        m_oldScreenSize.Set(Screen.width, Screen.height);

        return bgImgRect;
    }
    //-----------------------------------------------------
    // ScreenCenter : resize l'image par rapport au centre de l'Ã©cran
    //                 ie. le vecteur centreImage->centreEcran est affectÃ© par le resize (longeur multipliÃ©e par factorDelta)
    // !ScreenCenter : par rapport au centre de l'image seulement (factorDelta non utilisÃ©)
    //                 ie. la distance centreImage->centreEcran ne bouge pas
    private Rect RescaleBgImg(Rect bgImgPixIn, bool screenCenter, float factorDelta = 0f)
    {
//        Rect bgImgPixIn   = m_backgroundImage.guiTexture.pixelInset;
        if(!screenCenter)   // imgCenter
        {
            Vector2 center = bgImgPixIn.center;
            bgImgPixIn.width  = m_orgBgImgPixIn.width * m_zoomImage;
            bgImgPixIn.height = m_orgBgImgPixIn.height * m_zoomImage;
            bgImgPixIn.center = center;
        }
        else // screenCenter
        {
            // On passe en coordonnÃ©es par rapport au centre de l'Ã©cran
            Vector2 center = new Vector2(bgImgPixIn.center.x - UsefulFunctions.ScrCenter().x,
                                         bgImgPixIn.center.y - UsefulFunctions.ScrCenter().y);

            center.Scale(new Vector2(factorDelta, factorDelta)); // Scale du vecteur
            center.x += UsefulFunctions.ScrCenter().x;  // On repasse en coordonnÃ©es "normales"
            center.y += UsefulFunctions.ScrCenter().y;
            bgImgPixIn.width  = m_orgBgImgPixIn.width * m_zoomImage;
            bgImgPixIn.height = m_orgBgImgPixIn.height * m_zoomImage;
            bgImgPixIn.center = center;
        }
        return bgImgPixIn;
    } // RescaleBgImg
#endregion
#region mode2D_features
    //-----------------------------------------------------
    public void Reset()
    {

        m_lineMgr.DeleteAllLines();
        Debug.Log("Mode 2D reset");
        m_loadPanelFromRightMenu = false;
        m_orgBgImgPixIn.Set(0f, 0f, 0f, 0f);
        m_savedBgImgPixIn.Set(0f, 0f, 0f, 0f);
        m_oldScreenSize.Set(0f, 0f);
        m_rescaleFactor = 1f;
        m_zoomObjects = m_scaleList[0]*m_rescaleFactor*(m_modePortrait ? c_scaleZoomFactorPortrait : c_scaleZoomFactor);
        m_locationObjects = Vector3.zero;
        m_rotationObjects = 0f;

        m_m2dObjs.Clear();

        m_loadedBg = null;
//        Texture2D tex = data.m_loadedBg;
//        if(tex != null)
//        {
//            m_loadedBg = tex;
//            m_newBgImgLoaded = false;
//        }

//        m_backgroundImg.guiTexture.pixelInset = RescaleBgImg(m_backgroundImg.guiTexture.pixelInset, false);
//        m_backgroundImg.guiTexture.pixelInset = TranslateBgImg(
//                                   m_backgroundImg.guiTexture.pixelInset, deltaMove.x, deltaMove.y);
//        m_backGrid.GetComponent<BackGridManager>().RescaleGuiTexture();     // Relocate back texture

//        ZoomObjCam(m_zoomObjects);

        SaveModel();
		GameObject mainnode =  GameObject.Find("MainNode").gameObject;
		for(int i =0; i < mainnode.transform.childCount; i++)
		{
			if(mainnode.transform.GetChild(i).name != "_avatar")
				Camera.main.GetComponent<Mode2D>().AddObj(mainnode.transform.GetChild(i));
		}
    }
    //-----------------------------------------------------
    private void SetMovingObjects(bool enabled)
    {
        m_movingObj = enabled;
        HideHelp();
        if(enabled)
            ChangeHelpImg(m_helpTexObj);
    }
    //-----------------------------------------------------
    private void SetMeasuring(bool enabled)
    {
        m_measuring = enabled;
        HideHelp();
        if(enabled)
            ChangeHelpImg(m_helpTexLin);

		m_lineMgr.SetActivated(enabled);

        RefreshUIrects();
    }
    //-----------------------------------------------------
    private void SetObjZoom(float zoom)
    {
//        Vector3 vTmp = m_mainCam.transform.localPosition;
//        vTmp.y = zoom;
//        m_mainCam.transform.localPosition = vTmp;
        m_zoomObjects = zoom;
		m_mainCam.orthographicSize = zoom;
    }
    //-----------------------------------------------------
    private void MoveObjects(float deltaX, float deltaY, bool record = true)
    {
        if(record)
        {
            m_locationObjects.x += deltaX;
            m_locationObjects.y += deltaY;
        }

		for(int i=0; i<m_listCopy.Count; i++)
        {
			if(m_listCopy[i] != null)
			{
				Vector3 pos = m_listCopy[i].position;
	            pos.x += deltaX;
	            pos.z += deltaY;
				m_listCopy[i].position = pos;
        	}
        }
    }
    private void resetPlan()
    {
		GUITexture guiTexture = m_backgroundImg.GetComponent<GUITexture>();
		
		if(guiTexture && guiTexture.pixelInset != m_bkupImgPixIn)
		{
			guiTexture.pixelInset = m_bkupImgPixIn;
			
			if(m_lineMgr.getIsLineStarted())
			{
				m_lineMgr.removeStartedLine();
			}
			
			m_lineMgr.DeleteAllLines();
			
			SetMeasuring(false);
			m_movingObj = true;
		}
	}
	
	private void deletePlan()
	{

		GUITexture guiTexture = m_backgroundImg.GetComponent<GUITexture>();
		
		if(guiTexture)
		{
			//guiTexture.texture = null;
			guiTexture.enabled = false;
			m_mode2DTiledGrid.GetComponent<Renderer>().enabled = true;
			m_loadedBg = null;
			m_backGrid.GetComponent<BackGridManager>().enabled = false;
			m_backGrid.GetComponent<GUITexture>().enabled = false;
			//m_bkupBg = m_backgroundImg.guiTexture.pixelInset;
		}
	}
    
    //-----------------------------------------------------
    private Vector3 GetObjsGravityCenter()
    {
        // -- Compute gravity center of shown objects --
        Vector3 center = Vector3.zero;
		foreach(Transform obj in m_listCopy)
        {
        	if(obj)
        	{
            	center += new Vector3(obj.localPosition.x, 0f, obj.localPosition.z);
       		}
       	}
		center /= m_listCopy.Count;
        return center;
    }
    //-----------------------------------------------------
    private void RotateObjects(float angle, bool record = true)
    {
        if(record)
            m_rotationObjects += angle;

        Vector3 center = GetObjsGravityCenter();
        // -- Rotate --
		foreach(Transform obj in m_listCopy)
		{
			if(obj)
			{
            	obj.RotateAround(center, Vector3.up, angle);
            }
        }
    }
    
	private void ScaleObjects(int _isens)
	{
		List<Transform> listTransform = new List<Transform>();
		Vector3 v3center = Vector3.zero;
		int i = 0;
		foreach(Transform t in m_listCopy)
		{
			v3center += t.position;
			listTransform.Add(t);
			i++;
		}
		
		if(i != 0)
		{
			v3center /= i;
			
			GameObject tempParent = new GameObject();
			tempParent.transform.position = v3center;
			
			foreach(Transform t in listTransform)
			{
				t.parent = tempParent.transform;
			}
			
			Vector3 v3scale = tempParent.transform.localScale;
			
			v3scale.x += _isens * 0.1f;
			v3scale.y += _isens * 0.1f;
			v3scale.z += _isens * 0.1f;
			
			v3scale.x = v3scale.y = v3scale.z = Mathf.Clamp(v3scale.x, 0.1f, 10.0f);
			
			tempParent.transform.localScale = v3scale;
			
			foreach(Transform t in listTransform)
			{
				t.parent = GameObject.Find("MainWithLibs").transform;
			}
			
			Destroy(tempParent);
		}
	}
	
	private void CenterObjects()
	{	
		/*m_locationObjects.x -= GetObjsGravityCenter().x;
		m_locationObjects.y -= GetObjsGravityCenter().z;*/
		
		if(m_listCopy.Count == 1)
		{
			foreach(Transform t in m_listCopy)
			{
				t.position = Vector3.zero;
				//t.position += v3center;
			}
		}
		else if(m_listCopy.Count != 0)
		{
			Vector3 v3center = Vector3.zero;
			int i = 0;
			foreach(Transform t in m_listCopy)
			{
				v3center += t.position;
				i++;
			}
			
			v3center /= i;
			
			foreach(Transform t in m_listCopy)
			{
				t.position = Vector3.zero - (v3center - t.position);
			}
		}
		
		/*for(int i=0; i<m_listCopy.Count; i++)
		{
			if(m_listCopy[i] != null)
			{
				Vector3 pos = m_listCopy[i].position;
				//pos.x = 0.0f;
				//pos.z = 0.0f;
				m_listCopy[i].position = pos;
			}
		}*/
	}
	
    //-----------------------------------------------------
    private void CheckAndRecenterObjs()
    {
		foreach(Transform obj in m_listCopy)
        {
        	if(obj)
        	{
	            Vector2 pos = m_mainCam.WorldToViewportPoint(obj.position);
	            if(pos.x > 1.05f || pos.x < -0.05f || pos.y > 1.1f || pos.y < -0.1f) // Si un objet est hors de l'Ã©cran, recentrer
	            {
	                MoveObjects(-m_locationObjects.x, -m_locationObjects.y, false);
	                m_locationObjects = Vector3.zero;
	            }
            }
        }
    }
    //-----------------------------------------------------
    IEnumerator ActivateMode2D_async(bool active)
    {
		#if UNITY_IPHONE
		if(active)
		{
			EtceteraManager.imagePickerChoseImageEvent += OpenFileIpad;
			EtceteraManager.imagePickerCancelledEvent  += OpenFileIpadFailed;
		}
		else
		{
			EtceteraManager.imagePickerChoseImageEvent -= OpenFileIpad;
			EtceteraManager.imagePickerCancelledEvent  -= OpenFileIpadFailed;
		}
		#endif
        yield return new WaitForEndOfFrame();   // needed because of rotation attach/detach children (cf. guimenuright)
        ActivateMode2D(active);
    }
    //-----------------------------------------------------
    
    private bool listCopyContains(string _szname)
    {
		foreach(Transform t in m_listCopy)
		{
			if(t.name == _szname)
			{
				return true;
			}
		}
		
    	return false;
	}
	
	private bool m2dContains(string _szname)
	{
		foreach(Transform t in m_m2dObjs)
		{
			if(t != null && t.name == _szname)
			{
				return true;
			}
		}
		
		return false;
	}
    
    public void ActivateMode2D(bool active)
    {
		UsefullEvents.FireUpdateMode2DState(active);
        if(active)
		{
			List<Transform> deleteList = new List<Transform>();
			
			foreach(Transform t in m_m2dObjs)
			{
				if(t != null && !listCopyContains(t.name))
				{
					Transform copy = Instantiate(t) as Transform;
					Transform temp = null;
					temp = copy.Find("plage");
					if( temp != null )
						Destroy(temp.gameObject);
					m_listCopy.Add(copy);
					copy.parent = transform.root;
					copy.transform.localScale = t.localScale;
					copy.name = t.name;
					
					unactiveShadow(copy);
					
					t.gameObject.SetActive(false);
					
					FunctionConf_Dynshelter fc_ds = copy.GetComponent<FunctionConf_Dynshelter>();
					
					if(fc_ds != null)
					{
						foreach(Transform child in copy)
						{
							if(child.name.Contains("Arrow"))
							{
								Destroy (child.gameObject);
							}
						}
					}
				}
				/*else
				{
					deleteList.Add(t);
				}*/
			}
			
			foreach(Transform t in m_listCopy)
			{
				if(!m2dContains(t.name))
				{
					deleteList.Add(t);
				}
					
				t.gameObject.SetActive(true);
			}
			
			foreach(Transform dt in deleteList)
			{
				if(m_listCopy.Contains(dt))
				{
					m_listCopy.Remove(dt);
					Destroy(dt.gameObject);
				}
				/*else
				{
					m_m2dObjs.Remove(dt);
				}*/
			}
			
			deleteList.Clear();
			
			m_resizeEnabled = true;

            // -- Backup mainCam --
            m_bkupAngle = m_bkupPivot.rotation;                         // Transformations
            m_bkupPivot.rotation = Quaternion.identity;
            Vector3 tmp = new Vector3(90f, 0f, 0f);
            Quaternion mode2DAngle = new Quaternion(0f,0f,0f,0f);
            mode2DAngle.eulerAngles = tmp;

            m_mainCam.GetComponent<MainCamManager>().SetVisible(false);
            m_mainCam.GetComponent<CameraFrustum>().StopFrustum();      // DÃ©sactiver custom frustum
            m_mainCam.transform.rotation = mode2DAngle;
            m_bkupPos = m_mainCam.transform.localPosition;
            m_mainCam.transform.localPosition = new Vector3(0f, 100f, 0f);
            m_bkupRenderZone = m_mainCam.pixelRect;
            m_bkupCullingMask = m_mainCam.cullingMask;
        //    m_mainCam.cullingMask = 1<<4;
            m_mainCam.pixelRect = new Rect(0,0,UnityEngine.Screen.width,UnityEngine.Screen.height);
            m_mainCam.orthographic = true;
            m_mainCam.orthographicSize = m_zoomObjects;

            m_mainCam.GetComponent<GuiTextureClip>().enabled = false;   // Avant/aprÃ¨s

            m_bkupBgImgColor = m_backgroundImg.GetComponent<GUITexture>().color;
            m_backgroundImg.GetComponent<GUITexture>().color = Color.gray;

            if(m_loadedBg == null) // si pas de plan chargÃ©
            {
                m_backgroundImg.GetComponent<GUITexture>().enabled = false;
                m_mode2DTiledGrid.GetComponent<Renderer>().enabled = true;
                m_backGrid.GetComponent<BackGridManager>().enabled = false;
                m_backGrid.GetComponent<GUITexture>().enabled = false;
                m_showLoadPanel = false;
                m_loadPanelFromRightMenu = false;
                m_showRightMenu = true;
				m_bkupBg = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
			}
			else
            {

				Debug.Log ("loadPlan");
                SetLoadedImage(m_loadedBg);
                SetMovingObjects(true);
				m_showRightMenu = true;
				m_backgroundImg.GetComponent<GUITexture>().pixelInset = m_bkupBg;
	
				Debug.Log (m_bkupBg);

			}
			Debug.Log ("ActivateMode2D()");
			if(!usefullData.lowTechnologie)
			{
	            m_eraserImages.GetComponent<EraserV2>().SetVisible(false);
	            m_grassImages.GetComponent<GrassV2>().SetVisible(false);
			}

            // -- UI --
            m_mainScene.GetComponent<GUIMenuLeft>().enabled = false;
            m_mainScene.GetComponent<GUIMenuMain>().SetHideAll(true);
            m_mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
            Camera.main.GetComponent<ObjInteraction>().enabled = false;

            // -- objets 3D --
            m_bkupAvatarEnabled = m_avatar.GetComponent<Avatar>().IsForceDisplayed();
            m_avatar.GetComponent<Avatar>().SetForceDisplay(false);     // cacher avatar

            m_bkupSelected = m_mainCam.GetComponent<ObjInteraction>().getSelected();
            m_mainCam.GetComponent<ObjInteraction>().setSelected(null); // dÃ©sÃ©lectionner objet

            // -- Backup PosRot objets --
           /* RotateObjects(m_rotationObjects, false);
            MoveObjects(m_locationObjects.x, m_locationObjects.y, false);
            int i=0;
			foreach(Transform obj in m_listCopy)
            {
				if(obj)
				{
	                m_bkupObjScl[i] = obj.localScale;
	                m_bkupObjPos[i] = obj.transform.localPosition;
	                //obj.localScale = m_bkupScale;
	                i++;
	            }
            }*/
            CheckAndRecenterObjs();
			HideObjects();
			resetPlan();
			if(m_loadedBg != null)
			{

				m_backgroundImg.GetComponent<GUITexture>().pixelInset = m_bkupBg;
			}
        }
        else
		{

			if(m_loadedBg != null)
			{
				m_bkupBg = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
			}

			m_savedBgImgPixIn =m_backgroundImg.GetComponent<GUITexture>().GetScreenRect();
			foreach(Transform t in m_listCopy)
			{
				t.gameObject.SetActive(false);
				//Destroy(copy.gameObject);
			}
			//m_listCopy.Clear();
			
			foreach(Transform t in m_m2dObjs)
			{
				if(t != null)
				{
					t.gameObject.SetActive(true);
				}
			}
	
			m_showLoadPanel = false;

            SetMovingObjects(false);
            SetMeasuring(false);
			
			m_planManipulation = false;
			
			if(m_loadedBg != null)
                m_savedBgImgPixIn = m_backgroundImg.GetComponent<GUITexture>().pixelInset;

            // -- Main Cam --
            m_bkupPivot.rotation = m_bkupAngle;
            m_mainCam.GetComponent<MainCamManager>().SetVisible(true);
            m_mainCam.GetComponent<CameraFrustum>().StartFrustum();      // Activer custom frustum
            m_mainCam.transform.rotation = new Quaternion(0,0,0,0);
            m_mainCam.transform.localPosition = m_bkupPos;
            m_mainCam.pixelRect = m_bkupRenderZone;
            m_mainCam.cullingMask = m_bkupCullingMask;
            m_mainCam.orthographic = false;

            m_mainCam.GetComponent<GuiTextureClip>().enabled = true; // Avant/aprÃ¨s

            m_backgroundImg.GetComponent<BgImgManager>().ReinitBackgroundTex();
            m_backgroundImg.GetComponent<GUITexture>().color = m_bkupBgImgColor;
            m_backgroundImg.GetComponent<GUITexture>().enabled = true;

            m_mode2DTiledGrid.GetComponent<Renderer>().enabled = false;

            m_backGrid.GetComponent<BackGridManager>().enabled = true;
            m_backGrid.GetComponent<GUITexture>().enabled = true;
            UsefullEvents.FireResizeWindowEnd();                // Forcer resize + recharger photo

			if(!usefullData.lowTechnologie)
			{
            	m_eraserImages.GetComponent<EraserV2>().SetVisible(true);
         	   	m_grassImages.GetComponent<GrassV2>().SetVisible(true);
			}

            // -- Objets 3D --
            m_avatar.GetComponent<Avatar>().SetForceDisplay(m_bkupAvatarEnabled);
            m_mainCam.GetComponent<ObjInteraction>().setSelected(m_bkupSelected);

			// -- Backup PosRot objets --
			/*int i=0;
			foreach(Transform obj in m_m2dObjs)
			{
				if(obj)
				{
					obj.localScale = m_bkupObjScl[i];
					obj.transform.localPosition = m_bkupObjPos[i];
					i++;
				}
			}
           // MoveObjects(-m_locationObjects.x, -m_locationObjects.y, false);
            RotateObjects(-m_rotationObjects, false);*/
            

            // -- UI --
            m_mainScene.GetComponent<GUIMenuLeft>().enabled = true;
            Camera.main.GetComponent<ObjInteraction>().enabled = true;
            m_mainScene.GetComponent<GUIMenuMain>().SetHideAll(false);

            m_resizeEnabled = false;
			ShowObjects();
			//resetPlan();
        }
		
        if(m_maskCreator!=null)
            m_maskCreator.Set2DMode(active);
		
		m_lineMgr.enabled=active;
		

    } // ActivateMode2D()
	
	private void HideObjects()
	{
		//m_objectsToHide.Clear();
		if(m_mainNode!=null)
		{

			foreach (Transform transformCheck in m_mainNode.transform)
			{
				bool isInmode2D = false;
				foreach(Transform transform in m_listCopy)
				{
					if(transform && transform.gameObject.GetHashCode()==transformCheck.gameObject.GetHashCode())
						isInmode2D=true;
						
				}
				if((!isInmode2D) && (transformCheck.name!="MainNode"))
				{
		//			m_objectsToHide.Add(transformCheck.gameObject);
					transformCheck.gameObject.SetActive(false);
				}
			}
		}
	}
	
	private void ShowObjects()
	{
	//	m_objectsToHide.Clear();
		if(m_mainNode!=null)
		{
			foreach (Transform transformCheck in m_mainNode.transform)
			{
				transformCheck.gameObject.SetActive(true);
			}
		}
	}
	
	// Cache les objets qui ne doivent pas apparaitre dans le mode 2D
	/*private void ConstantHide()
	{
		foreach (GameObject objToHide in m_objectsToHide)
		{
			objToHide.SetActive(false);
		}
	}*/
	
    //-----------------------------------------------------
    private void SaveScreenshot()
    {
        #if UNITY_IPHONE || UNITY_ANDROID
            StartCoroutine(m_mainCam.GetComponent<Screenshot>().galleryShot(SetUIafterScreenshot));
        #else
            StartCoroutine(m_mainCam.GetComponent<Screenshot>().takeScreenShotPC(false, true, SetUIafterScreenshot));
        #endif
        m_mainScene.GetComponent<GUIMenuLeft>().ResetMenu();
    }
	private void SendScreenshot()
    {
        #if UNITY_IPHONE || UNITY_ANDROID
            StartCoroutine(m_mainCam.GetComponent<Screenshot>().mailShot(SetUIafterScreenshot, "Plan de masse ONESHOT 3D"));
        #endif
        m_mainScene.GetComponent<GUIMenuLeft>().ResetMenu();
    }
    //-----------------------------------------------------
    private void SetUIafterScreenshot()
    {
        // -- UI --
        m_mainScene.GetComponent<GUIMenuLeft>().enabled = false;
        m_beforeAfter.enabled = false;
        m_mainScene.GetComponent<GUIMenuMain>().SetHideAll(true);
        m_mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
        Camera.main.GetComponent<ObjInteraction>().enabled = false;
    }
    //-----------------------------------------------------
    private void LoadImage()
    {
        m_newBgImgLoaded = true;
        #if UNITY_IPHONE 
            OpenFileIphone();
        #elif  UNITY_ANDROID
            OpenFileAndroidDevice();
        #else
            OpenFileStandalone();
        #endif
    }
    //-----------------------------------------------------
    private void SetLoadedImage(Texture2D loadedImg)
    {
        m_loadedBg = loadedImg;

        m_oldScreenSize.Set(Screen.width, Screen.height);

        m_backgroundImg.GetComponent<GUITexture>().texture = loadedImg;             // assigner la texture Ã  bg img
        m_backgroundImg.GetComponent<GUITexture>().enabled = true;
        m_backGrid.GetComponent<BackGridManager>().enabled = true;  // activer la backgrid
        m_backGrid.GetComponent<GUITexture>().enabled = true;

        // -- Redimensionnement image de fond --
        m_orgBgImgPixIn = m_backgroundImg.GetComponent<BgImgManager>().RescaleBgImg(Screen.width, Screen.height);
        m_orgBgImgPixIn.x = (Screen.width-m_orgBgImgPixIn.width)/2f;
        m_backgroundImg.GetComponent<GUITexture>().pixelInset = m_orgBgImgPixIn;
        m_zoomImage = 1f;
//        Debug.Log("image org rect = "+m_orgBgImgPixIn+", current = "+m_backgroundImg.guiTexture.pixelInset);

        if(m_newBgImgLoaded)
        {
			if(!m_showLoadPanel && m_listCopy.Count > 0)
                LaunchHelpCountdown();
        }
        else
            m_backgroundImg.GetComponent<GUITexture>().pixelInset = m_savedBgImgPixIn;

        ResizeRectsToScreen();
//        Debug.Log("image new rect = "+m_backgroundImg.guiTexture.pixelInset);

        m_mainCam.GetComponent<MainCamManager>().UpdateBgCamRenderTex();

        // -- Not A4 warning --
        if(m_newBgImgLoaded && !IsA4ratio(m_loadedBg, out m_modePortrait))
            ShowNotA4Warning();
        else if(m_showWarning && m_warningTxt == TextManager.GetText("Mode2D.WarningA4"))
            HideWarning();

        float zoomFactor = (m_modePortrait ? c_scaleZoomFactorPortrait : c_scaleZoomFactor);
        SetObjZoom(m_scaleList[m_curScaleID]*zoomFactor*m_rescaleFactor);
        CheckAndRecenterObjs();

        m_backGrid.GetComponent<BackGridManager>().RescaleGuiTexture(); // Relocate back texture

        m_mode2DTiledGrid.GetComponent<Renderer>().enabled = false;                     // dÃ©sactiver grille

        // Sur iPad ObjInteraction se rÃ©active quand on charge un plan
        m_mainCam.GetComponent<ObjInteraction>().setSelected(null); // dÃ©sÃ©lectionner objet
        Camera.main.GetComponent<ObjInteraction>().enabled = false;

        SaveModel();
        m_newBgImgLoaded = false;
		
		#if UNITY_IPHONE
		Rect rpixelInset = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
		m_backgroundImg.GetComponent<GUITexture>().pixelInset = new Rect((Screen.width - rpixelInset.width) * 0.5f, (Screen.height - rpixelInset.height) * 0.5f, rpixelInset.width, rpixelInset.height); 
		#endif
		
		m_bkupImgPixIn = m_backgroundImg.GetComponent<GUITexture>().pixelInset;

		Debug.Log ("SetLoadedImage()");
    }
    //-----------------------------------------------------
    public void LoadFromSceneModel()
    {
        SceneModel.M2D data = m_sceneModel.GetMode2D();
        if(!data.m_empty)
        {
            if(data.m_oldSave)
            {
                Texture2D tex = data.m_loadedBg;
                Reset();

                if(tex != null)
                {
                    m_loadedBg = tex;
                    m_newBgImgLoaded = true;
                }
            }
            else
            {
                m_orgBgImgPixIn.Set(data.m_orgBgImgPixIn.x, data.m_orgBgImgPixIn.y,
                                    data.m_orgBgImgPixIn.width, data.m_orgBgImgPixIn.height);
                m_savedBgImgPixIn.Set(data.m_savedBgImgPixIn.x, data.m_savedBgImgPixIn.y,
                                      data.m_savedBgImgPixIn.width, data.m_savedBgImgPixIn.height);
                m_oldScreenSize.Set(data.m_oldScreenSize.x, data.m_oldScreenSize.y);
                m_zoomObjects = data.m_zoomObj != 0 ? data.m_zoomObj : 100; // Les montages prÃ© 1.3 sont Ã  0
                m_locationObjects.x = data.m_translaObj.x;
                m_locationObjects.y = data.m_translaObj.y;
                m_rotationObjects = data.m_rotaObj;

                m_lineMgr.LoadFromData(data.m_lineData);
                Texture2D tex = data.m_loadedBg;
                if(tex != null)
                {
                    m_loadedBg = tex;
                    m_newBgImgLoaded = true;
					m_bkupBg = new Rect(data.m_savedBgImgPixIn.x,data.m_savedBgImgPixIn.y,data.m_savedBgImgPixIn.width,data.m_savedBgImgPixIn.height);
                }
            }
        }
		Debug.Log ("LoadFromSceneModel()");
    }
    //-----------------------------------------------------
    public void SaveModel()
    {
        m_sceneModel.SetMode2D(m_orgBgImgPixIn, m_savedBgImgPixIn, m_oldScreenSize,
                     m_zoomObjects, m_locationObjects, m_rotationObjects, m_lineMgr.GetSaveData(), m_loadedBg);
    }
	//-----------------------------------------------------
    public bool IsActive()
    {
        return m_mode2d;
    }
    //-----------------------------------------------------
    public void HideUI(bool hide)
    {
        m_hideUI = hide;
    }
    //-----------------------------------------------------
    public bool IsCursorOnUI()
    {
        return PC.In.CursorOnUI(m_uiRects);
    }
    //-----------------------------------------------------
    public void ShowNotA4Warning()
    {
        m_showWarning = true;
        m_warningTxt = TextManager.GetText("Mode2D.WarningA4");

        m_warningFading       = true;
        m_warningFadeTimer    = Time.time;
        m_warningFadeStartCol = new Color(1f, 1f, 1f, 0f);
        m_warningFadeEndCol   = Color.white;
    }
    //-----------------------------------------------------
    public void ShowMovedObjectsWarning()
    {
        m_showWarning = true;
        m_warningTxt = TextManager.GetText("Mode2D.WarningObj");

        m_warningFading       = true;
        m_warningFadeTimer    = Time.time;
        m_warningFadeStartCol = new Color(1f, 1f, 1f, 0f);
        m_warningFadeEndCol   = Color.white;
    }
    //-----------------------------------------------------
    private void HideWarning()
    {
        m_warningFading       = true;
        m_warningFadeTimer    = Time.time;
        m_warningFadeStartCol = Color.white;
        m_warningFadeEndCol   = new Color(1f, 1f, 1f, 0f);
    }
    //-----------------------------------------------------
    public bool ShowingMovedObjectsWarning()
    {
        if(m_warningTxt == TextManager.GetText("Mode2D.WarningObj"))
            return m_showWarning;
        else
            return false;
    }
    //-----------------------------------------------------
    public bool IsPlanLoaded()
    {
        return (m_loadedBg != null);
    }
#endregion

#region m2D_objects
    //----------------------------------------------------
    public void AddObj(Transform newM2dObj) // Called by ObjInstanciation, ObjInteraction, Montage
    {
        m_m2dObjs.Add(newM2dObj);
        
		m_bkupObjScl.Add(newM2dObj.localScale);
		m_bkupObjPos.Add(newM2dObj.transform.localPosition);
        m_bkupObjRot.Add(newM2dObj.localRotation);
    }
    
    public void unactiveShadow(Transform _t)
	{
		foreach(Transform child in _t)
		{
			if(child.GetComponent<Renderer>() != null)
			{
				child.GetComponent<Renderer>().castShadows = false;
				child.GetComponent<Renderer>().receiveShadows = false;
			}
			 
			unactiveShadow(child);
		}
    }
    
    //----------------------------------------------------
    public void RemoveObj(Transform objToRemove)
    {
    	if(objToRemove)
    	{
			int index = m_m2dObjs.IndexOf(objToRemove);
			if(index!=-1)
			{
				m_m2dObjs.RemoveAt(index);
	       	 	m_bkupObjScl.RemoveAt(index);
				m_bkupObjRot.RemoveAt(index);
				m_bkupObjPos.RemoveAt(index);
			}
		}
    }
    //-----------------------------------------------------
    public int GetObjCount()
    {
		return m_listCopy.Count;
    }
    //-----------------------------------------------------
    public bool HasBgImg()
    {
        return !(m_loadedBg == null);
    }
#endregion


#region file_open

    //-----------------------------------------------------
    private void OpenFileIphone()
    {
#if UNITY_IPHONE
        EtceteraManager.setPrompt(true);
        EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Album );
#endif
    }
	
    //-----------------------------------------------------
    private void OpenFileAndroidDevice()
    {
#if UNITY_ANDROID
		EtceteraAndroid.promptForPictureFromAlbum(UnityEngine.Screen.width,UnityEngine.Screen.height,name);
#endif
    }



    private void importPdf(string path)
    {
        PdfToPng converter = new PdfToPng();
        try
        {
            if (!Directory.Exists(Application.persistentDataPath + "/temp"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/temp");
            path = path.Replace("/", "\\");
            converter.DoTheJob(path, Application.persistentDataPath + "\\temp\\pdfToPng.png", 1, 1, 100, 100);
        }
        catch (UnityException e)
        {
            Debug.Log("error loading pdf file :" + e.ToString());
        }
        StartCoroutine(ContinueOpenFileStandalone(Application.persistentDataPath + "\\temp\\pdfToPng.png"));
    }
    //-----------------------------------------------------
    private void OpenFileStandalone()
    {
        string sOpenPath = "";
        #if  UNITY_EDITOR
            if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log("UNITY_EDITOR : ");
            sOpenPath = UnityEditor.EditorUtility.OpenFilePanel(TextManager.GetText("Dialog.ChooseImage"),"","");
            if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log("Path0 : "+sOpenPath);
            if(sOpenPath != "")
                if (sOpenPath.ToLower().EndsWith(".pdf"))
                {
                    importPdf(sOpenPath);
                }
                else
                {
                    StartCoroutine(ContinueOpenFileStandalone(sOpenPath));
                }
        #elif UNITY_STANDALONE_WIN
            string[] extensions = new string[] {"jpeg", "jpg", "png","pdf"};
            sOpenPath = NativePanels.OpenFileDialog(extensions);
    
            if(sOpenPath.Length < 4){
                if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log("open process has ben canceled or has failed");
            }else{
                if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log("Path0 : "+sOpenPath);
                if(sOpenPath != "")
                     if (sOpenPath.ToLower().EndsWith(".pdf"))
                    {
                        importPdf(sOpenPath);
                    }
                    else
                    {
                        StartCoroutine(ContinueOpenFileStandalone(sOpenPath));
                    }
            }
        #elif UNITY_STANDALONE_OSX 
            string[] extensions = new string[] {"jpeg", "jpg", "png"};
            sOpenPath = NativePanels.OpenFileDialog(extensions);
    
            if(sOpenPath.Length < 4){
                if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log("open process has ben canceled or has failed");
            }else{
                if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log("Path0 : "+sOpenPath);
                if(sOpenPath != "")
					{
                        StartCoroutine(ContinueOpenFileStandalone(sOpenPath));
                    }
            }
#endif
    } // OpenFileStandalone()

    //-----------------------------------------------------
    IEnumerator ContinueOpenFileStandalone(string s)
    {
        #if UNITY_STANDALONE_OSX
            s = s.Replace("\\","/");
        #else
            s = s.Replace("/","\\");
        #endif

        s = TextUtils.EncodeToHtml(s);
        string path = "file://"+s;
        WWW www = new WWW (path);
        yield return www;
        if (www.error != null)
        {
            Debug.LogWarning(www.error);
            yield return true;
        }
        else
        {
            Texture2D tex = new Texture2D(1,1);
            www.LoadImageIntoTexture(tex);
            SetLoadedImage(tex);
            www.Dispose ();
            yield return true;
        }
    } // ContinueOpenFileStandalone()

    //-----------------------------------------------------
    private void OpenFileIpad(string path)
    {
#if UNITY_IPHONE
        if(m_mainScene.GetComponent<GUIStart>().enabled == true)
            return;
        if( path == null )
        {
            OpenFileIpadFailed("Path null");
            return;
        }
        m_iPadPath = path;
        StartCoroutine(EtceteraManager.textureFromFileAtPath( "file://" + path, ContinueOpenFileiPad, OpenFileIpadFailed));
#endif
	}
	
    //-----------------------------------------------------
    private void ContinueOpenFileiPad(Texture2D texture)
    {
        SetLoadedImage(texture);

        if(System.IO.File.Exists(m_iPadPath))
        {
            if((PC.DEBUG && DEBUG) || PC.DEBUGALL)
                Debug.Log("IMG STILL EXISTS: Deleting ...");
            System.IO.File.Delete(m_iPadPath);
        }
    }

    //-----------------------------------------------------
    private void OpenFileIpadFailed()
    {
        Debug.LogError(DEBUGTAG+"OPEN FILE FAILED");
    }

    //-----------------------------------------------------
    private void OpenFileIpadFailed(string error)
    {
        Debug.LogError(DEBUGTAG+"OPEN FILE FAILED >"+error);
    }
	
    //-----------------------------------------------------
    private void OpenFileAndroid(string path, Texture2D texture)
    {
#if UNITY_ANDROID
        if(m_mainScene.GetComponent<GUIStart>().enabled == true)
            return;
        if( path == null )
        {
            OpenFileAndroidFailed("Path null");
            return;
        }
		SetLoadedImage(texture);
		
#endif
	}

    //-----------------------------------------------------
    private void OpenFileAndroidFailed()
    {
        Debug.LogError(DEBUGTAG+"OPEN FILE FAILED");
    }
	
    //-----------------------------------------------------
    private void OpenFileAndroidFailed(string error)
    {
        Debug.LogError(DEBUGTAG+"OPEN FILE FAILED >"+error);
    }
#endregion

    //-----------------------------------------------------
    // Activation serveur
    private void AuthorizePlugin(string name,bool auth)
    {
        if(name == c_pluginName)
            this.enabled = auth;
    }
	
	private void LockUI(bool isLck)
	{
		_uiLocked = isLck;
	}


    //-----------------------------------------------------
    public float GetMetersPerPixels()
    {
        float pxPerCm = (GetDiag(m_backgroundImg.GetComponent<GUITexture>().pixelInset)) / c_A4diag_cm;
        return m_scaleList[m_curScaleID]/pxPerCm/100f;
    }
    //-----------------------------------------------------
    private float GetDiag(Rect r)
    {
        return Mathf.Sqrt(r.width*r.width + r.height*r.height);
    }

    #region paper_format_utils
    private const float c_A4diag_cm         = 36.37430411705f;
    private const float c_A4ratio_portrait  = 0.707070707071f;
    private const float c_A4ratio_landscape = 1.414285714286f;
    //-----------------------------------------------------
    private bool IsA4ratio(Texture2D tex, out bool portrait)
    {
        float w = tex.width;
        float h = tex.height;
        if(ApproxFormat(w/h, c_A4ratio_portrait))
        {
            portrait = true;
            return true;
        }
        else if(ApproxFormat(w/h, c_A4ratio_landscape))
        {
            portrait = false;
            return true;
        }

        portrait = (h>w);
        return false;
    }
    //-----------------------------------------------------
    private bool ApproxFormat(float f1, float f2)
    {
        return (Mathf.Abs(f1 - f2) < 0.015f);
    }

	public void DisableObjectCopy ()
	{

		for(int i = m_listCopy.Count-1; i > -1;i--)
		{
			m_listCopy[i].gameObject.SetActive(false);
		}
		m_listCopy.Clear();
		
		
	}
	public void DestroyObjectCopy ()
	{
		/*
		for(int i = m_m2dObjs.Count-1; i > -1;i--)
		{
			DestroyImmediate(m_m2dObjs[i].gameObject);
		}
		m_m2dObjs.Clear();
		*/
		for(int i = m_listCopy.Count-1; i > -1;i--)
		{
			DestroyImmediate(m_listCopy[i].gameObject);
		}
		m_listCopy.Clear();
		
		
	}

    #endregion
} // class Mode2D
