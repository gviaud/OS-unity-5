//-----------------------------------------------------------------------------
// Assets/Scripts/fromLab/NewGui/GUISubTools.cs - 02/2012 - KS
// TODO : 

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Pointcube.Global;

//-----------------------------------------------------------------------------
// Panneau de gauche, affiche les sous-outils d'un outil de GUIEditTools
public class GUISubTools : MonoBehaviour
{
    // -- Références scène --
    public GameObject m_backgroundImg;
    public GameObject m_mainCam;
    public GameObject m_mainScene;
    public GameObject m_eraser;
    public GameObject m_grass;
    public GameObject m_grassSkybox;
    public GUIStyle   m_tracerStyle;

    // TODO : Set global Enum values so that GUIEditTools values correspond to GUISubTools values
    // eg. GUIEditTools.EditTools.Eraser est un code pour l'eraser (100), et tous les SubTools de l'eraser sont basés sur ce code (100, 101, ...)
    // Ne doivent pas figurer dans cette énumération les outils qui n'ont pas de sous-outil : ceux-ci doivent seulement figurer dans GUIEditTools.

    public  string     m_ctxPanelID_1;  // ctx1_ppAdd
    public  string     m_ctxPanelID_2;  // ctx1_ppSub
    public  string     m_ctxPanelID_3;  // ctx1_texAdd
    public  string     m_ctxPanelID_4;  // ctx1_texSub

	public GUISkin skin;
	public GUISkin skinMenuRight;
    public enum SubTool     // TODO passer en private ?
    {
        // -- Sous-outils de l'outil gomme --
        EraserPoly = 100,  EraserUndo = 102, EraserInvert = 101,
        /*EraserBrush = 101,*/ EraserRedo = 103, EraserReset = 104,
        
        // -- Sous-outils de l'outil gazon --
        GrassPoly = 110,    GrassUndo = 113, GrassInvert = 111,
        /*GrassBrush = 111*/GrassRedo = 114, GrassReset = 115,
        GrassChgMat = 112,
        
        // -- Presets de réflexion --
        ReflecP1 = 210, ReflecP2 = 211, ReflecP3 = 212, ReflecP4 = 213,
        ReflecP5 = 214, ReflecP6 = 215, ReflecReset = 216,
        
        None = 0, Validate = 10000
    }; // enum SubTool
    
    private SubTool     m_lastSubTool;   // Sous-outil de la frame précédente
    private SubTool     m_curSubTool;    // Sous-outil actuel
    private SubTool     m_curTool;       // Outil actuel (parent du sous-outil actuel) TODO utiliser le type EditTool
    
	private bool        m_defaultInitTool; // Sous-outil à activer par défaut
	private bool        m_openRightOnDisable = false;
	
    private Dictionary<int, string[]>  m_labels = new Dictionary<int, string[]>()       // Sous-outil par outil
    {
        { 100 /* Gomme   */, new string[] {"GUISubTools.erase", /*"Pinceau",*/ 
				"GUISubTools.eraseRedo", "GUISubTools.eraseCancel", 
				"GUISubTools.eraseCancelRedo", "GUISubTools.eraseReinit"}},
		
		
        { 110 /* Gazon   */, new string[] {"GUISubTools.grass", /*"Pinceau",*/ "GUISubTools.grassRedo",
				"GUISubTools.grassMaterial", "GUISubTools.grassCancel", 
				"GUISubTools.grassCancelRedo", "GUISubTools.grassReinit"}},
        { 200 /* Ombrage */, new string[] {"GUISubTools.lightMenu"}},
        { 210 /* Réflex. */, new string[] {"GUISubTools.cfaible", "GUISubTools.cmoyen",
				/*"C Forte",*/ "GUISubTools.mfaible", "GUISubTools.mforte", 
				"GUISubTools.sfaible","GUISubTools.cforte", "GUISubTools.zero"}},
        { 0   /* Rien    */, new string[] {}}
    };
	private Dictionary<int, float[]>  m_sizes = new Dictionary<int, float[]>()       // Sous-outil par outil
    {
        { 100 /* Gomme   */, new float[] {150,150, 40, 40, 40}},
        { 110 /* Gazon   */, new float[] {150, 150, 200, 40, 40, 40}},
		{ 210 /* reflex   */   , new float[] {80, 80, 80, 80, 80, 80,80}}
    };
	private Dictionary<int, string[]>  m_styles = new Dictionary<int, string[]>()       // Sous-outil par outil
    {
        { 100 /* Gomme   */, new string[] {"outilOff","outilOff", /*"undo", "redo", "reinit"*/"outilOff","outilOff","outilOff"}},
        { 110 /* Gazon   */, new string[] {"outilOff", "outilOff", "mat", /*"undo", "redo", "reinit"*/"outilOff","outilOff","outilOff"}},
		{ 210 /* reflex   */   , new string[] {"btn", "btn", "btn", "btn", "btn", "btn","btn"}}
    };
	private Dictionary<int, float>  m_maxSizes = new Dictionary<int, float>()       // Sous-outil par outil
    {
        { 100 /* Gomme   */, 460},
        { 110 /* Gazon   */, 660},
		{ 210 /*Reflex       */,680}
    };

    // -- Outil gazon --
    private bool        m_displayChoicePanel;      // Afficher le panel de choix de la texture ou non
    private Object[]    m_grassTex;                // Textures du dossier Resources/grass (chargées si besoin)
    private Texture2D   m_grassCurTex;             // Texture actuelle
    private const int   m_synthTexMaxCount = 5;    // /!\ Doit être égal au synthTexCount de GrassHandler
    private int         m_synthTexButtonCount;     // Nombre de textures synthétisées à afficher
	public 	ArrayList newTextures = new ArrayList();

    private bool		m_active = false;		//affichage des subTools (pour savoir s'il faut se soucier de mettre à jour les lerp & co)
    private bool        m_canValidate;
//	private Rect displayZone;
	private Rect sampleZone;
	private float btnH = 50;        // TODO rendre générique la hauteur de bouton?
    private float btnW = 100;
	private float off7 = -50;
	
	private bool isAdding = true;
	private SubTool activedSubTool = SubTool.None;
	
	//UI
	string[] labels = null;
	
	float[] sizes = null;
	
	string[] styles = null;
	
    int menuLength;
	
	float maxLength;
	
	private Rect dispZone;

    private static readonly string DEBUGTAG = "GUISubTools : ";
	
	public Texture2D[] icons;
	
    //-----------------------------------------------------
    void Awake()
    {
        UsefullEvents.OnResizingWindow  += SetRects;
        UsefullEvents.OnResizeWindowEnd += SetRects;
		//UsefullEvents.ShowHelpPanel 	+= Validate;
    }

    //-----------------------------------------------------
    void Start()
    {
        if(m_backgroundImg == null) Debug.LogError(DEBUGTAG+"Background image "+PC.MISSING_REF);
        if(m_mainCam == null)       Debug.LogError(DEBUGTAG+"Main camera"      +PC.MISSING_REF);
        if(m_mainScene == null)     Debug.LogError(DEBUGTAG+"Main Scene"       +PC.MISSING_REF);
        if(m_eraser == null)        Debug.LogError(DEBUGTAG+"Eraser "          +PC.MISSING_REF);
        if(m_grass == null)         Debug.LogError(DEBUGTAG+"Grass background "+PC.MISSING_REF);
        if(m_grassSkybox == null)   Debug.LogError(DEBUGTAG+"Grass skybox "    +PC.MISSING_REF);
        if(m_tracerStyle == null)   Debug.LogError(DEBUGTAG+"Tracer style "    +PC.MISSING_REF);

        m_curSubTool  = SubTool.None;   // Par défaut, aucun sous-outil d'activé
        m_curTool     = SubTool.None;
        m_lastSubTool = SubTool.None;

        m_defaultInitTool    = false;
        m_displayChoicePanel = false;
		if(!usefullData.lowTechnologie)
        m_grassCurTex        = (Texture2D) GameObject.Find("grassGround").GetComponent<Renderer>().material.GetTexture("_MainTex");

        m_synthTexButtonCount = 0;
		
        sampleZone  = new Rect();
		dispZone    = new Rect();

        SetRects();

		//enabled = false;
        m_canValidate = true;
    }
    
    //-----------------------------------------------------
    // Exécution des actions des boutons sur la scène
    void Update()
	{
		/*if(activedSubTool == SubTool.None)
		{
			if(m_active && !m_backgroundImg.GetComponent<PolygonTracer>())
			{
                if(!IsCursorOnUI() && m_canValidate)
                {
                	print("validate");
                    Validate();
                }
			}
			return;
		}*/
		
		/*switch(activedSubTool) // Indice du nouveau sous-outil basé sur l'indice de l'outil (parent) courant
        {
            //------------------------------------------- Gomme > Polygone
            case SubTool.EraserPoly :
            		print ("coucou");
                if(m_lastSubTool != SubTool.EraserPoly)
                {
					startGomme(true);
                }
            break;
            //------------------------------------------- Gomme > Inverser
            case SubTool.EraserInvert :
				startGomme(false);
                    
            break;
            
            //------------------------------------------- Gomme > Undo
            case SubTool.EraserUndo :
				Debug.Log("GUISubTools : Eraser Undo");
				if(!usefullData.lowTechnologie)
				{
	                m_eraser.GetComponent<EraserV2>().UndoLastZone();
				}
            break;
            //------------------------------------------- Gomme > Redo
            case SubTool.EraserRedo :
				Debug.Log("GUISubTools : Eraser Undo");
				if(!usefullData.lowTechnologie)
				{
	                m_eraser.GetComponent<EraserV2>().Redo();
				}
            break;
            
            //------------------------------------------- Gomme > Reset
            case SubTool.EraserReset :
				if(!usefullData.lowTechnologie)
				{
                   	m_eraser.GetComponent<EraserV2>().Reset();
				}
            break;
            
            //------------------------------------------- Gazon > Polygone
            case SubTool.GrassPoly :
                if(m_lastSubTool != SubTool.GrassPoly)
                {
					startGazon(true);
                    m_displayChoicePanel = false;
                }
            break;
            
            //------------------------------------------- Gazon > Inverser
            case SubTool.GrassInvert :
				startGazon(false);
                m_displayChoicePanel = false;
                    
            break;
            
            //------------------------------------------- Gazon > Changer de matière
            case SubTool.GrassChgMat :
                    m_curSubTool = activedSubTool;
                    m_displayChoicePanel = true;
            break;
            
            //------------------------------------------- Gazon > Undo
            case SubTool.GrassUndo :
                Debug.Log("GUISubTools : Undo");
                m_displayChoicePanel = false;
                m_grass.GetComponent<GrassV2>().UndoLastZone();
            break;
            
            //------------------------------------------- Gazon > Redo
            case SubTool.GrassRedo :
                Debug.Log("GUISubTools : Redo");
                m_displayChoicePanel = false;
                m_grass.GetComponent<GrassV2>().Redo();
            break;
            
            //------------------------------------------- Gazon > Reset
			case SubTool.GrassReset :
                m_displayChoicePanel = false;
                m_grass.GetComponent<GrassV2>().Reset();
            break;
            
            //------------------------------------------- Réflexion > Preset1
            case SubTool.ReflecP1 :
                SetReflectionColor(new Color(1, 1, 1, 0.082f));
            break;
            
            //------------------------------------------- Réflexion > Preset2
            case SubTool.ReflecP2 :
                SetReflectionColor(new Color(1, 0.941f, 0.843f, 0.251f));
            break;

            //------------------------------------------- Réflexion > Preset3
            case SubTool.ReflecP3:
                SetReflectionColor(new Color(1, 0.941f, 0.843f, 0.082f));
            break;

            //------------------------------------------- Réflexion > Preset4
            case SubTool.ReflecP4:
                SetReflectionColor(new Color(0.784f, 0.741f, 0.663f, 0.439f));
            break;

            //------------------------------------------- Réflexion > Preset5
            case SubTool.ReflecP5:
                SetReflectionColor(new Color(0.824f, 0.824f, 0.824f, 0.027f));
            break;
            
            //------------------------------------------- Réflexion > Preset6
            case SubTool.ReflecP6:
                SetReflectionColor(new Color(0.439f, 0.408f, 0.369f, 0.439f));
            break;

            //------------------------------------------- Réflexion > Preset7
            case SubTool.ReflecReset:
                SetReflectionColor(new Color(0, 0, 0, 0));
            break;
            
            default : // valider
            break;
            
        } // switch (sous-outil courant)
		activedSubTool = SubTool.None;*/
    }
    
    //-----------------------------------------------------
    // Mise à jour de l'interface
    void OnGUI()
    {
        GUI.skin = skin;
      /*  if(m_displayChoicePanel)
            DisplayChoicePanel();*/
		if(m_active)
		{
			if(m_backgroundImg.GetComponent<PolygonTracer>())
			{
			}
			else if(!m_displayChoicePanel)
			{
				/*if(!GetComponent<PleaseWaitUI>().IsDisplayingIcon())
				{*/
					//m_mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
					Validate();
					//DisplaySubToolsMenu();
				//}
			}
		}
    }

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= SetRects;
        UsefullEvents.OnResizeWindowEnd -= SetRects;
		UsefullEvents.ShowHelpPanel 	-= Validate;
    }

    //-----------------------------------------------------
    private void SetRects()
    {
        sampleZone.Set(0, 50, Screen.width, 50);
        dispZone.Set(Screen.width-290, 0, 300, Screen.height);
    }

	public void startGomme(bool adding)
	{
		if(m_backgroundImg.GetComponent("PolygonTracer") == null &&
			m_backgroundImg.GetComponent<GUITexture>().texture != null)
		{
		    m_backgroundImg.AddComponent<PolygonTracer>();
            m_backgroundImg.GetComponent<PolygonTracer>().m_backgroundImg = m_backgroundImg;
            m_backgroundImg.GetComponent<PolygonTracer>().m_eraserNode = m_eraser;
            m_backgroundImg.GetComponent<PolygonTracer>().m_grassNode = m_grass;
            m_backgroundImg.GetComponent<PolygonTracer>().m_mainCam = m_mainCam;
            m_backgroundImg.GetComponent<PolygonTracer>().m_mainScene = m_mainScene;
            m_backgroundImg.GetComponent<PolygonTracer>().m_style = m_tracerStyle;
			
			isAdding = adding;
			m_backgroundImg.GetComponent<PolygonTracer>().SetInvert(!adding);
			Debug.Log("Start Gomme : " + isAdding);

           /* if(adding)
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_1);
            else
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_2);*/
		}
	}
	
	public void startGazon(bool adding)
	{
		 if(m_backgroundImg.GetComponent("PolygonTracer") == null &&
           	m_backgroundImg.GetComponent<GUITexture>().texture != null)
        {
            m_backgroundImg.AddComponent<PolygonTracer>();
            m_backgroundImg.GetComponent<PolygonTracer>().SetTool(GUIEditTools.EditTool.Grass);
            m_backgroundImg.GetComponent<PolygonTracer>().m_backgroundImg = m_backgroundImg;
            m_backgroundImg.GetComponent<PolygonTracer>().m_eraserNode = m_eraser;
            m_backgroundImg.GetComponent<PolygonTracer>().m_grassNode = m_grass;
            m_backgroundImg.GetComponent<PolygonTracer>().m_mainCam = m_mainCam;
            m_backgroundImg.GetComponent<PolygonTracer>().m_mainScene = m_mainScene;
            m_backgroundImg.GetComponent<PolygonTracer>().m_style = m_tracerStyle;
            
			isAdding = adding;
			m_backgroundImg.GetComponent<PolygonTracer>().SetInvert(!adding);
			Debug.Log("Start Gazon : " + isAdding);

           /* if(adding)
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_3);
            else
                PC.ctxHlp.ShowCtxHelp(m_ctxPanelID_4);*/

        }
	}
	
	/*public void activate(int code)
	{
		m_active = true;
		SetCurTool(code);
		Component script = GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>();
        if(script) ((ObjBehavGlobal) script).ToggleSceneObjectsTransparent();
		
		if(m_curTool == SubTool.EraserPoly && !m_eraser.GetComponent<EraserV2>().HasBeenUsedOnce())
			startGomme(true);
		
		if(m_backgroundImg.GetComponent<PolygonTracer>())
			m_backgroundImg.GetComponent<PolygonTracer>().onExtUi = false;
	}*/
	
	public void activate()
	{
		m_active = true;
		
		ObjBehavGlobal obg = GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>();
		if(obg != null)
		{
			obg.ToggleSceneObjectsTransparent();
		}
		
		if(m_backgroundImg.GetComponent<PolygonTracer>())
			m_backgroundImg.GetComponent<PolygonTracer>().onExtUi = false;
	}
	
	public void choiceMaterial()
	{
		m_active = true;
		
		m_displayChoicePanel = true;
	}
	
	public bool isActive()
	{
		return m_active || m_backgroundImg.GetComponent("PolygonTracer") != null;
	}
	
	public bool IsDisplayChoicePanelActivated()
	{
		return m_displayChoicePanel;
	}
	
    //-----------------------------------------------------
    public void DisplaySubToolsMenu()
    {
		/*GUILayout.BeginArea(dispZone);
	    GUILayout.FlexibleSpace();		
		GUILayout.Box("",skinMenuRight.GetStyle("bgFadeUp"),GUILayout.Width(260),GUILayout.Height(150));//fade en haut
		GUILayout.BeginVertical(skinMenuRight.GetStyle("bgFadeMid"),GUILayout.Width(260));
		
		bool isGazon = ((int)m_curTool >= 110);
		int intOff7 = 0;
		if(isGazon)
			intOff7 = 1;
		
		
		for(int i=0 ; i<menuLength ; i++)
        {
			bool button = false;
            if(m_curTool != SubTool.GrassPoly || i != 2)
			{				
				if(i==0) //ajout
				{
					GUILayout.BeginHorizontal(skinMenuRight.GetStyle(styles[i]),GUILayout.Height(50),GUILayout.Width(260));
					GUILayout.FlexibleSpace();
					button = GUILayout.Button(TextManager.GetText(labels[i]),"empty",GUILayout.Height(50));
					GUILayout.Space(40);
					GUILayout.EndHorizontal();
				}
				else if(i==1) //inverser
				{
					GUILayout.BeginHorizontal(skinMenuRight.GetStyle(styles[i]),GUILayout.Height(50),GUILayout.Width(260));
					GUILayout.FlexibleSpace();
					button = GUILayout.Button(TextManager.GetText(labels[i]),"empty",GUILayout.Height(50));
					GUILayout.Space(40);
					GUILayout.EndHorizontal();
				}
			}
            else
			{
				GUILayout.BeginHorizontal(GUILayout.Height(50),GUILayout.Width(260));
				button = GUILayout.Button((Texture) m_grassCurTex,"matiere") || GUILayout.Button(TextManager.GetText(labels[i]),skinMenuRight.GetStyle("outilOff"),GUILayout.Height(50),GUILayout.Width(219));
				GUILayout.EndHorizontal();
				
			}
		
			if(button || m_defaultInitTool)
            {
                // Gestion des sous-outils par défaut (à l'activation d'un outil)
                if(!m_defaultInitTool)
				{
                    activedSubTool = (SubTool) m_curTool + i; // Indice du sous-outil activé, basé sur l'indice de l'outil parent actuel
				}
                else
                {
                    activedSubTool = (SubTool) m_curTool;     // Sous-outil par défaut (même indice que l'outil parent)
                    m_defaultInitTool = false;
                }
            } // if bouton
        } // for each bouton
		
		GUILayout.BeginHorizontal(skinMenuRight.GetStyle("urr_bg"),GUILayout.Height(50),GUILayout.Width(260));
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("",skinMenuRight.GetStyle("urr_undo"),GUILayout.Height(50),GUILayout.Width(50)))
		{
			if(!m_defaultInitTool)
			{
                activedSubTool = (SubTool) m_curTool + 2 + intOff7; // Indice du sous-outil activé, basé sur l'indice de l'outil parent actuel
			}
            else
            {
                activedSubTool = (SubTool) m_curTool;     // Sous-outil par défaut (même indice que l'outil parent)
                m_defaultInitTool = false;
            }
		}
		GUILayout.Space(15);
		if(GUILayout.Button("",skinMenuRight.GetStyle("urr_redo"),GUILayout.Height(50),GUILayout.Width(50)))
		{
			if(!m_defaultInitTool)
			{
                activedSubTool = (SubTool) m_curTool + 3 + intOff7; // Indice du sous-outil activé, basé sur l'indice de l'outil parent actuel
			}
            else
            {
                activedSubTool = (SubTool) m_curTool;     // Sous-outil par défaut (même indice que l'outil parent)
                m_defaultInitTool = false;
            }
		}
		GUILayout.Space(15);
		if(GUILayout.Button("",skinMenuRight.GetStyle("urr_reset"),GUILayout.Height(50),GUILayout.Width(50)))
		{
			if(!m_defaultInitTool)
			{
				activedSubTool = (SubTool) m_curTool + 4 + intOff7; // Indice du sous-outil activé, basé sur l'indice de l'outil parent actuel
			}
			else
			{
				activedSubTool = (SubTool) m_curTool;     // Sous-outil par défaut (même indice que l'outil parent)
				m_defaultInitTool = false;
			}
		}
		GUILayout.Space(25);
		GUILayout.EndHorizontal();
		
		GUILayout.EndVertical();
		GUILayout.Box("",skinMenuRight.GetStyle("bgFadeDw"),GUILayout.Width(260),GUILayout.Height(150));//fade en bas		
	    GUILayout.FlexibleSpace();
	    GUILayout.EndArea();
		//------------------------
        
        // Gestion des changements de sous-outils (annulation d'actions en cours, etc.)
        if(m_curSubTool != m_lastSubTool)
        {
            if(m_lastSubTool == SubTool.EraserPoly || m_lastSubTool == SubTool.GrassPoly) // Outils utilisant un PolygonTracer : annulation du tracé
            {
                // TODO Si polygone pas fini, alors message de confirmation d'annulation de polygone
                Component script = m_backgroundImg.GetComponent("PolygonTracer");
                if(script)
                {
                	((PolygonTracer) script).abort(true);
                	print ("abort");
                }
            }
            if(m_lastSubTool == SubTool.GrassChgMat)
                m_displayChoicePanel = false;     // Masquer le panel de choix (de matières pour le gazon)
        } // Si l'outil actuel a changé
        
        m_lastSubTool = m_curSubTool;           // Actualisation du dernier outil activé*/
    } // DisplaySubToolsMenu
    
    //-----------------------------------------------------
    public void Validate()
    {
		m_active = false;
		
		ObjBehavGlobal obg = GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>();
		if(obg != null && obg.isObjectsTransp())
		{
			obg.ToggleSceneObjectsTransparent();
		}
		
		Camera.main.GetComponent<ObjInteraction>().setActived(true);
		m_mainScene.GetComponent<GUIMenuLeft>().canDisplay(true);
		if(m_mainScene.GetComponent<GUIMatPicker>().enabled == true)
		{
			m_mainScene.GetComponent<GUIMatPicker>().enabled = false ;
		}
		m_mainScene.GetComponent<GUIMenuRight>().setVisibility(true);
		m_mainScene.GetComponent<GUIMenuRight>().canDisplay(true);
		m_mainScene.GetComponent<GUIMenuRight>().quitSubTool();
    }
    
    //-----------------------------------------------------
    // TODO rendre générique cette fonction pour GUIGridTools, GUIEditTools, GUISubTools ?
    public int GetButtonCount()
    {
        string[] labels = null;
        m_labels.TryGetValue((int)m_curTool, out labels);
        if(labels != null)
            return labels.Length;
        else
            Debug.LogWarning("GUISubTools : Fonction non reconnue : "+(int)m_curTool+" (non implementee ?)");
        
        return 0;
    }
    
	/*public void reset()
	{
		bool isGazon = ((int)m_curTool >= 110);
		int intOff7 = 0;
		if(isGazon)
			intOff7 = 1;
		
		if(!m_defaultInitTool)
		{
			activedSubTool = (SubTool) m_curTool + 4 + intOff7; // Indice du sous-outil activé, basé sur l'indice de l'outil parent actuel
		}
		else
		{
			activedSubTool = (SubTool) m_curTool; // Sous-outil par défaut (même indice que l'outil parent)
			m_defaultInitTool = false;
		}
	}*/
	
    public void resetEraserV2()
	{
		m_eraser.GetComponent<EraserV2>().Reset();
	}
	
	public void resetGrassV2()
	{
		m_grass.GetComponent<GrassV2>().Reset();
	}
	
	//-----------------------------------------------------
	/*public void SetCurTool(int toolCode)
    {
		activedSubTool = (SubTool) toolCode;
		m_curTool = activedSubTool;
		//m_curTool = (SubTool) toolCode;
		
		labels = null;
        m_labels.TryGetValue((int)m_curTool, out labels);
		
		sizes = null;
		m_sizes.TryGetValue((int)m_curTool, out sizes);
		
		styles = null;
        m_styles.TryGetValue((int)m_curTool, out styles);
		
        if(labels == null)
		{
			Debug.LogError("GUISubTools : aucun sous-outil connu pour cet outil : "+(int)m_curTool); return;
		}

        menuLength = labels.Length;
		
		maxLength = m_maxSizes[(int)m_curTool];
    }*/
    
    //-----------------------------------------------------
    public void SetDefaultInitOn()
    {
        m_defaultInitTool = true;
    }
    
    //-----------------------------------------------------
    private void SetReflectionColor(Color newCol)
    {
        GameObject colPlane = GameObject.Find("collisionPlane");
        if(colPlane)
        {
            if (colPlane.GetComponent<Renderer>().material.HasProperty("_IlluminCol"))
                colPlane.GetComponent<Renderer>().material.SetColor("_IlluminCol", newCol);
            else Debug.LogError("GUISubTools : Propriete \"_IlluminCol\" non trouvee dans le shader du materiau de collisionPlane");
        }
        else Debug.LogError("GUISubTools : objet \"collisionPlane\" non trouve dans la scene");
    } // SetReflectionColor
    
    public void setDisplayChoicePanel(bool _bvalue)
    {
    	m_displayChoicePanel = _bvalue;
    }
    //-----------------------------------------------------
    public void DisplayChoicePanel()
    {
		if(!m_mainScene.GetComponent<GUIMatPicker>().enabled )
		{
			m_mainScene.GetComponent<GUIMatPicker>().enabled = true ;
			m_mainScene.GetComponent<GUIMatPicker>().canDisplay(false);
		}
		/*if(m_grassTex == null)
            m_grassTex = m_grassSkybox.GetComponent<GrassHandler>().GetDefaultTexs();

        int wid = 50;
        int count = m_grassTex.Length;
        Texture2D grassTex;
        int i=0;
		
		float startY = (sampleZone.width/2)-(((float)(count+m_synthTexButtonCount+1)/2)*50);
		
		if(m_backgroundImg.GetComponent<GrassSynthesizer>() == null)
		{
	        GUI.BeginGroup(sampleZone);
			for(i=0; i<count+m_synthTexButtonCount; i++)
	        {
	            if(i < count)
					grassTex = (Texture2D) m_grassTex[i]; 
	         	else         
					grassTex = m_grassSkybox.GetComponent<GrassHandler>().GetSynthTex(i-count);
	
				if(GUI.Button(new Rect(startY+i*wid, 0, wid, wid), grassTex,"samples"))
	            {
	                if(grassTex != m_grassCurTex)
	                {
	                    m_grassCurTex = grassTex;
	                    m_grassSkybox.GetComponent<GrassHandler>().SetUsedTexture(i);
	                }
					
					m_displayChoicePanel = false;
					
					m_mainScene.GetComponent<GUIMenuRight>().setVisibility(true);
					m_mainScene.GetComponent<GUIMenuRight>().canDisplay(true);
					m_mainScene.GetComponent<GUIMenuRight>().quitSubTool();
					//Validate();
	            }
	        } // pour i<nb textures
	
			if(GUI.Button(new Rect(startY+(count+m_synthTexButtonCount)*wid, 0, wid, wid), "","Add"))
	        {
	            m_canValidate = false;
	            m_backgroundImg.AddComponent("GrassSynthesizer");
	            m_backgroundImg.GetComponent<GrassSynthesizer>().m_backgroundImg = m_backgroundImg;
	            m_backgroundImg.GetComponent<GrassSynthesizer>().m_mainCam = m_mainCam;
	            //m_displayChoicePanel = false;
	        }
	
	        GUI.EndGroup();
	    }*/
    } // DisplayChoicePanel

	public Texture2D getCurrentTex()
	{
		Texture2D tex = null;
		if( m_grassCurTex)
			tex = Resources.Load("grass/thumbs/"+m_grassCurTex.name) as Texture2D;
		//ressource/GrassV2/thumb/getname
		return tex;
	}
	public Texture2D getCurrentMaterial()
	{
	
		return m_grassCurTex;
	}
	public void setCurrentMaterial(Texture2D newTexture)
	{
		m_grassCurTex= newTexture;
	}
    //-----------------------------------------------------
    public void SetSynthesizedGrassTex(Texture2D thumb)
    {
        m_canValidate = true;
        if(m_synthTexButtonCount < m_synthTexMaxCount)
            m_synthTexButtonCount++;

        m_grassCurTex = thumb;
    }

    //-----------------------------------------------------
    private bool IsCursorOnUI()
    {
        bool output = true;
        #if (UNITY_IPHONE || UNITY_ANDROID)&& !UNITY_EDITOR
            if(Input.touchCount > 0)
            {
                Touch t = Input.touches[0];
                Vector2 tpos = t.position;
                tpos.y = Screen.height - tpos.y;
                if(t.phase == TouchPhase.Began && !dispZone.Contains(tpos) &&
                                                                         !sampleZone.Contains(tpos))
                {
                    output = false;
                }
            }
        #else
            Vector3 mpos = Input.mousePosition;
            mpos.y = Screen.height - mpos.y;
            if(Input.GetMouseButtonUp(0) && !dispZone.Contains(mpos) && !sampleZone.Contains(mpos))
            {
                output = false;
            }
        #endif
        return output;
    }

} // class GUISubTools
