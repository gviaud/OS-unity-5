//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/PolygonTracer.cs - 02/2012 KS (repris de Polygon de l'inpainting)
// AttachÃ© dynamiquement Ã  Background/backgroundImage par GUISubTools

// TODO : ne pas tracer le curseur sur ipad
// TODO : tracÃ© imprÃ©cis des polygones au double clic
// TODO : image de chargement non-affichÃ©e lors du remplissage du polygone
// TODO : loupe : permettre de tracer Ã©galement sur les 16 pixels de bordure de l'image
//                  \--> CrÃ©er un espace de travail avec dÃ©filement qui joue sur le x et y des pixels insets

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pointcube.Global;

//-----------------------------------------------------------------------------
// Classe permettant de tracer des polygones pour l'outil gomme, et de les
// passer Ã  EraserMask afin de rendre effectif le gommage.
// FonctionnalitÃ©s :
//      - TracÃ© d'un polygone point par point
//      - Fermeture du polygone en revenant sur le premier point ou en double-cliquant
//      - Remplissage optimisÃ© du polygone tracÃ©, pour traitement par un outil ensuite (gomme, inpaint)
//      - Une loupe lorsqu'on maintient le clic plus d'une seconde sans bouger
public class PolygonTracer : MouseTool
{
    // -- RÃ©fÃ©rences scÃ¨ne --
    public  GameObject m_grassNode;
    public  GameObject m_eraserNode;
    public  GameObject m_mainCam;
    public  GameObject m_mainScene;
	private GUIMenuMain _GUIMenuMain;

    public  GUIStyle   m_style;

    // Variables pour le tracÃ©
//	public  bool       lineStarted    = false;         // Ligne commencÃ©e ou non
//	public  bool       polygonLocked  = false;         // Polygone terminÃ© ou non
    private  bool      maskdone       = false;         // Masque calculÃ© ou non (une fois que le polygone est terminÃ©)
//    private ArrayList  lines          = new ArrayList(); // Lignes du polygone en cours de tracÃ© TODO poly
//    private Line       lineToAdd;                      // Ligne actuellement Ã©ditÃ©e
    private Ev2_Polygon m_polygon;                     // Polygon en cours de tracÃ© // TODO poly

//    public  float      _width         = 5;      // Todo poly delete
//    private float      tmpLneW        = 2;

    // Variables d'accÃ¨s Ã  d'autres gameObjects
    private PleaseWaitUI m_pleaseWaitUI;

    // Variables liÃ©es Ã  l'outil utilisÃ©
    private GUIEditTools.EditTool m_tool;             // DÃ©finit oÃ¹ envoyer le polygone tracÃ© une fois celui-ci refermÃ©

    private bool                  m_invert;

    // Zones de l'interface pour gÃ©rer le passage de la souris sur un bouton (par exemple)
    private List<Rect> m_guiZones;                    // Rectangles de l'interface, pour ne pas crÃ©er de point Ã  ces endroits
    
    // Ressources d'affichage
	public  Texture2D  cursor         = (Texture2D) Resources.Load("tracage/OS_CURSOR_ADD");  // TODO poly
	public  Texture2D  cursorSub   = (Texture2D) Resources.Load("tracage/OS_CURSOR_SUB");  // TODO poly
//	public  Texture2D  vertex         = (Texture2D) Resources.Load("tracage/point");
//	public  Texture2D  lineTex        = (Texture2D) Resources.Load("tracage/grayTex");
//	public  Texture2D  tempLineTex    = (Texture2D) Resources.Load("tracage/redTex");

    // Double clic pour fermeture de polygone
    private bool  m_clicked;                           // clic rÃ©cent ou pas (potentiel double clic)
    private float m_lastClickTime;                     // temps depuis le dernier clic rÃ©cent (0 si pas de clic rÃ©cent)
    private int   m_clicked_x, m_clicked_y;            // position du curseur au premier clic
    private const float m_doubleClickDelay = 0.2f;     //0.50f; // temps entre les deux clics (en secondes)
    private const float m_doubleClickThresold = 30.0f; //50.0f; // tolÃ©rance de position entre l'emplacement du 1er et du 2e clic (en pixels)
    private const float m_doubleClickThresoldTactile = 50.0f; // tolÃ©rance de position entre l'emplacement du 1er et du 2e clic (en pixels)
	
	// distance par rapport au premier
	private const float m_addPointThres = 10.0f; // 25; (en pixels)
	
    // Loupe
    private bool          m_loupeActive;                // Loupe active ou non
    private float         m_loupeClickTime;             // Temps aprÃ¨s le dernier clic non relÃ¢chÃ© (pour activation de la loupe)
    private int           m_loupeClick_x, m_loupeClick_y; // Position de la souris au dernier clic non relÃ¢chÃ©
    private const float   m_loupeDelay = 0.5f;          // DÃ©lai d'activation de la loupe aprÃ¨s un clic non relÃ¢chÃ©
    private const float   m_loupeThresold = 10.0f;      // TolÃ©rance de diffÃ©rence de position entre le clic et le dÃ©lai
    private GUITexture    m_loupeGUI;                   // GUITexture de la loupe
    public Texture2D     m_loupeTex;                   // Image de la loupe
//    private Camera        m_loupeCam;                   // CamÃ©ra de la loupe
    public RenderTexture m_loupeRenderTex;             // RenderTexture de la loupe
    private int           m_stickBd;                    // Marge Ã  partir de laquelle l'aimantation vers les pixels extrÃªmes de l'image s'active
    
    // -- GUI --
    public Rect          m_cancelRect;
	private LoupeGUI     _loupeGUI;
	
    // Debug
//    private bool m_newFillAlgo = true;
    private bool s_mouseDebug = false;
    private static readonly string DEBUGTAG = "PolygonTracer : ";
	
	public bool onExtUi = false;

#region unity_func
    //-----------------------------------------------------
    void Awake()
    {
        m_polygon = new Ev2_Polygon();
        m_invert  = false;

        UsefullEvents.OnResizingWindow  += SetGUIrects;
        UsefullEvents.OnResizeWindowEnd += SetGUIrects;
    }

    //-----------------------------------------------------
	void Start() 
	{
		GameObject GUITexLoupe = GameObject.Find("Background/loupe/GUITexLoupe");
		_loupeGUI = GUITexLoupe.GetComponent<LoupeGUI>();
		if(_loupeGUI!=null)
			_loupeGUI.SetInvert(m_invert);
		_GUIMenuMain = GameObject.Find("MainScene").GetComponent<GUIMenuMain>();
        base.StartMouseTool();

        if(m_grassNode == null)   Debug.LogError(DEBUGTAG+"Grass Node" +PC.MISSING_REF);
        if(m_eraserNode == null)  Debug.LogError(DEBUGTAG+"Eraser Node"+PC.MISSING_REF);
        if(m_mainCam == null)     Debug.LogError(DEBUGTAG+"Main Camera"+PC.MISSING_REF);
        if(m_mainScene == null)   Debug.LogError(DEBUGTAG+"Main Scene" +PC.MISSING_REF);
        if(m_style == null)       Debug.LogError(DEBUGTAG+"GUI style " +PC.MISSING_REF);

        m_pleaseWaitUI = m_mainScene.GetComponent<PleaseWaitUI>();
        if(m_pleaseWaitUI == null) Debug.LogError("Script PleaseWaitUI non trouve");

        //m_guiZones = new List<Rect>(); // initialisation dans AddGuiZone() car Start est appelÃ© aprÃ¨s
        // m_cancelButtonRect =          // de mÃªme pour m_cancelButtonRect
        
        m_lastClickTime  = 0;
        m_clicked        = false;
        
        m_loupeActive    = false;
        m_loupeClickTime = 0;
        m_loupeTex       = new Texture2D(64, 64, TextureFormat.RGB24, false);
        m_loupeGUI       = GUITexLoupe.GetComponent<GUITexture>();
        m_loupeRenderTex = null;
        m_loupeGUI.texture = m_loupeTex;
//        m_loupeCam.targetTexture = m_loupeRenderTex;
        
        m_stickBd        = 16;

//        m_stickX        = -1;
//        m_stickY        = -1;
	        
        // Outil Ã  appeler aprÃ¨s le remplissage du polygone, et Ã©ventuels paramÃ¨tres
        m_tool          = (m_tool == GUIEditTools.EditTool.None)? GUIEditTools.EditTool.Eraser : m_tool; // On utilise la gomme par dÃ©faut

        // -- GUI --
        m_cancelRect = new Rect(0f, 0f, m_style.fixedWidth, m_style.fixedHeight);
        SetGUIrects();
//        AddGuiZone(m_cancelRect);

        m_mainCam.GetComponent<Mode2D>().enabled = false;
        m_mainScene.GetComponent<HelpPanel>().enabled = false;
    }

    //-----------------------------------------------------
	void Update()
	{
        base.UpdateMouseTool(); // Note : ignoreClick dans cette fonction

        float xOffset   = m_backgroundImg.GetComponent<GUITexture>().pixelInset.x;
        float yOffset   = m_backgroundImg.GetComponent<GUITexture>().pixelInset.y;
        float bgImgW    = m_backgroundImg.GetComponent<GUITexture>().pixelInset.width;
        float bgImgH    = m_backgroundImg.GetComponent<GUITexture>().pixelInset.height;

        // -- TracÃ© du polygone --
		if(!m_polygon.IsClosed())
		{
            // -- Gestion de la dÃ©tection du double clic --
            if(m_clicked)       // si clic rÃ©cent
                m_lastClickTime += Time.deltaTime;
            if(m_lastClickTime > m_doubleClickDelay)
            {
                m_clicked = false;
                m_lastClickTime = 0;
            }

            bool cursorOnGUI = InGuiZone() || PC.ctxHlp.PanelBlockingInputs();
#if !UNITY_ANDROID
            // -- Loupe --
            if(PC.In.Click1Hold() && (!cursorOnGUI || m_loupeActive))
            {
                if(m_loupeClickTime == 0)            // Initialisation au dÃ©but du clic
                {
                    m_loupeClick_x = (int) m_mousePosX;
                    m_loupeClick_y = (int) m_mousePosY;
                }
                
                if(m_loupeClickTime < m_loupeDelay)  // Si le dÃ©lai n'est pas atteint
                {
                    if(m_mousePosX > (m_loupeClick_x - m_loupeThresold) && m_mousePosX < (m_loupeClick_x + m_loupeThresold) &&
                       m_mousePosY > (m_loupeClick_y - m_loupeThresold) && m_mousePosY < (m_loupeClick_y + m_loupeThresold))
                    {                                // Si le curseur n'a pas bougÃ©, incrÃ©menter le dÃ©compte "loupe"
                        m_loupeClickTime += Time.deltaTime;
                    }
                    else                             // Sinon, rÃ©initialiser le dÃ©compte
                        m_loupeClickTime = 0;
                }
                else
                {                                    // Si le dÃ©lai est atteint, activer la loupe
                    m_loupeActive = true;
                    int imgOffset = 16;              // Distance entre le curseur et le coin le plus proche de l'image loupe
                    int imgSize   = 128;             // CÃ´tÃ© de l'image loupe (= cÃ´tÃ© de l'image source * 2)

                    // -- Masquer l'interface -- TODO recabler
                    _GUIMenuMain.SetHideAll(true);


//                    if(m_loupeRenderTex == null ||
//                        m_loupeRenderTex.width  != (int)m_mainCam.camera.pixelWidth ||
//                        m_loupeRenderTex.height != (int)m_mainCam.camera.pixelHeight)
//                    {
//                        ResetLoupeRT();
//                    }

//                    m_mainCam.GetComponent<MainCamManager>().RenderSingle(m_loupeRenderTex,
//                                                                          new Rect(0f, 0f, 1f, 1f),true,16);
//					m_mainCam.GetComponent<MainCamManager>().GetBgOnly(m_loupeRenderTex);
					m_loupeRenderTex = m_mainCam.GetComponent<MainCamManager>().GetBgRenderTex(); // marche pas ca dÃ©cale la tex
		
					if(GameObject.Find("MainScene").GetComponent<GUIMenuRight>().MaskOrGrass !=2)
					{
						Camera.main.targetTexture = m_loupeRenderTex;
						RenderTexture.active = m_loupeRenderTex;
						Camera.main.Render ();
				
						//RenderTexture.active = null;
						//Camera.main.targetTexture = null;
					}
//                    m_mainCam.targetTexture = m_loupeRenderTex;
//                    m_loupeCam.Render();
//                    m_mainCam.Render();
//                    RenderTexture.active = m_loupeRenderTex;
                    
                    // -- SÃ©lection des pixels de l'image Ã  lire => DÃ©localisÃ© dans MouseTool
                    float[] data = base.GetZoneCornerFromCursor(imgSize/2);
                    float x = data[0] + xOffset;
                    float y = data[1] + yOffset;
					bool decX = (data[2]==1f);
                    bool decY = (data[3]==1f);

                    // Lecture des pixels de la renderTexture active
                    RenderTexture.active = m_loupeRenderTex;
					Rect polyDrawRect = new Rect(x, y, imgSize/2, imgSize/2);
                    m_loupeTex.ReadPixels(polyDrawRect, 0, 0);
					_loupeGUI.ContainsLastPoint(false);
					//float lastx = 0;
					//float lasty = 0;
					ArrayList lastXY = new ArrayList();
				//	Line polyLine=null;
					//Vector2 prevPoint = new Vector2();
					ArrayList prevPoints = new ArrayList();
					ArrayList screenPrevPoints = new ArrayList();
					bool findPoint=false;
					if(m_polygon.GetLineCount()>0)
					{
						if( m_polygon.GetLines().Length>0)
						{
						//	polyLine = m_polygon.GetLines()[m_polygon.GetLines().Length-1];
							bool first = true;
							foreach (Line polyLine in m_polygon.GetLines())
							{
								if(first)
								{
									Vector2 srcPoint = polyLine.src;
									prevPoints.Add(srcPoint);
									first = false;
								}
								Vector2 prevPoint = polyLine.dst;
								prevPoints.Add(prevPoint);
								findPoint=true;
							}
						}
					}
					else if (m_polygon.IsStarted())
					{
						Line polyLine = m_polygon.getLineTemp();
						if(polyLine!=null)
						{
							Vector2 prevPoint = polyLine.src;
							prevPoints.Add(prevPoint);
							findPoint=true;
						}
					}
					if(findPoint==true)
					{
#if UNITY_IPHONE
						/*Vector2 screenPrevPoint = new Vector2(
								prevPoint.x*bgImgW+xOffset,							
								Screen.height-(prevPoint.y*bgImgH+yOffset));*/
						foreach(Vector2 prevPoint in prevPoints)
						{
							Vector2 screenPrevPoint = new Vector2(
								prevPoint.x*bgImgW+xOffset,							
								Screen.height-(prevPoint.y*bgImgH+yOffset));
							screenPrevPoints.Add(screenPrevPoint);
						}
#else					
						
						/*Vector2 screenPrevPoint = new Vector2(
								prevPoint.x*bgImgW+xOffset,							
								prevPoint.y*bgImgH+yOffset);*/
						foreach(Vector2 prevPoint in prevPoints)
						{
							Vector2 screenPrevPoint = new Vector2(
								prevPoint.x*bgImgW+xOffset,							
								prevPoint.y*bgImgH+yOffset);
							screenPrevPoints.Add(screenPrevPoint);
						}
#endif
						int itPoint=0;
						_loupeGUI.ContainsLastPoint(false);
						foreach(Vector2 screenPrevPoint in screenPrevPoints)
						{							
						//	if(polyDrawRect.Contains(screenPrevPoint))
							{	
								if(prevPoints.Count>itPoint)
								{
									Vector2 prevPoint = (Vector2) prevPoints[itPoint];
									_loupeGUI.ContainsLastPoint(true);
									float lastx = m_mousePosX - screenPrevPoint.x; 
									float lasty = m_mousePosY - (prevPoint.y*bgImgH+yOffset);//screenPrevPoint.y; 
									lastXY.Add(new Vector2(lastx,lasty));
								//	Debug.Log("Find point numero : "+itPoint);
								}
							}
							itPoint++;
						}
					}
					
                    RenderTexture.active = null;
                    m_loupeTex.Apply(false);
                    
                    // -- Placement de l'image loupe --
                    bool invX = false, invY = false;
                    Rect pixInset = m_loupeGUI.pixelInset;
                    if(decX)                                                      // Si la souris est proche du bord de l'image
                    {
                        if(m_mousePosX > Screen.width/2)
                            x = Screen.width-xOffset-imgSize-imgSize/4-imgOffset; // fixe Ã  droite
                        else
                            x = xOffset - imgSize + imgOffset;                    // fixe Ã  gauche
                    }
                    else
                        x = m_mousePosX - imgSize - imgOffset;                    // Cas gÃ©nÃ©ral : au dessus du curseur
                    
                    if(decY)                                                      // De mÃªme pour y
                    {
                        if(m_mousePosY > Screen.height/2)
                            y = Screen.height - yOffset - imgSize - imgSize/4 - imgOffset;
                        else
                            y = yOffset + imgSize/4 + imgOffset;
                    }
                    else                                                          // Cas gÃ©nÃ©ral : Ã  gauche du curseur
                        y = m_mousePosY - imgOffset - imgSize;
                    
                    // Si l'image de la loupe n'a pas la place d'Ãªtre affichÃ©e de ce cÃ´tÃ© du curseur, changer de cÃ´tÃ©
                    pixInset.x = (invX = (m_mousePosX < imgSize+imgOffset+xOffset))? x+imgSize+imgOffset*2 : x;
                    pixInset.y = ((invY = (m_mousePosY < imgSize+imgOffset+yOffset)) && !decY)? y+imgSize+imgOffset*2 : y;
                    
                    m_loupeGUI.pixelInset = pixInset;
                    
                    // -- Position du curseur sur l'image de la loupe --
                    x = (invX)? (m_mousePosX + imgOffset + pixInset.width/2) : (m_mousePosX - imgSize - imgOffset + pixInset.width/2);
                    if(decX) // Si l'image est fixe proche du bord de l'Ã©cran
                    {
                        if(m_mousePosX > Screen.width - m_stickBd)
                            x = Screen.width - 1 - imgSize/4 - imgOffset;   // Aimantation sur le dernier pixel de l'image quand on est
                        else if(m_mousePosX < m_stickBd)                    // proche du bord de l'Ã©cran (pour Ipad)
                            x = 1 + imgSize/4 + imgOffset;
                        else if(m_mousePosX > Screen.width/2)               // Cas gÃ©nÃ©ral : dÃ©placement du curseur en fonction du curseur de la souris
                            x += m_mousePosX - (Screen.width - xOffset - imgSize/4);
                        else                                                // de mÃªme pour la partie gauche de l'Ã©cran
                            x -= xOffset + imgSize/4 - m_mousePosX;
                    } // Si loupe fixe en X
                    
                    y = (invY)? ((m_mousePosY + imgSize + imgOffset - pixInset.height/2)) : (m_mousePosY - imgOffset - pixInset.height/2);
                    if(decY)    // De mÃªme pour Y
                    {
                        if(m_mousePosY > Screen.height-m_stickBd)
                            y = Screen.height - 1 - imgSize/4 - imgOffset;
                        else if(m_mousePosY < m_stickBd)
                            y = 1 + imgSize/4 + imgOffset;
                        else if(m_mousePosY > Screen.height/2)
                            y += m_mousePosY - (Screen.height - yOffset - imgSize/4);
                        else
                            y -= (yOffset + imgSize/4) - m_mousePosY;
                    } // Si loupe fixe en Y
                    _loupeGUI.SetCurXY((int)x, (int)y);
				//	_loupeGUI.SetLastXY((int)(x-lastx*2), (int)(y-lasty*2));	
					_loupeGUI.SetLastXY((int)x,(int)y,lastXY);
					
                    
                    // Nettoyer
                    RenderTexture.active = null;
                    m_mainCam.GetComponent<Camera>().targetTexture = null;
//                    m_loupeRenderTex.Release(); // bug sur Ipad Ã  cause d'une bibliothÃ¨que liÃ©e Ã  OpenGL
                } // Loupe activÃ©e
            } // Loupe
            if(PC.In.Click1Up())   // Au relÃ¢chement du clic, masquer la loupe
            {
                if(m_ignoreClick)           // Ignorer un relÃ¢chement qui correspond Ã  un clic commencÃ© avant l'activation du PolygonTracer
                { Debug.Log("IgnoreClick"); m_ignoreClick = false; return; }
                
                if(m_loupeActive)
                {
                    m_loupeClickTime = 0;
                    m_loupeActive = false;
//                    m_loupeGUI.enabled = false;
                    _loupeGUI.SetCurXY(-1, -1);
                    _loupeGUI.SetLastXY(-1,-1,null);
                }
            } // Clic gauche relÃ¢chÃ©
#endif
            // -- TracÃ© de polygone --
			if(PC.In.Click1Up() && !cursorOnGUI)
			{
                // -- Aimantation sur le bord de l'Ã©cran --
                if(m_mousePosX > Screen.width - m_stickBd)
                    m_mousePosX = Screen.width;
                else if(m_mousePosX < m_stickBd)
                    m_mousePosX = 0;
                
                if(m_mousePosY > Screen.height-m_stickBd)
                    m_mousePosY = Screen.height;
                else if(m_mousePosY < m_stickBd)
                    m_mousePosY = 0;
                // -- fin aimantation --
				float m_doubleClickThresoldTemp = m_doubleClickThresold;
#if UNITY_IPHONE || UNITY_ANDROID
				m_doubleClickThresoldTemp = m_doubleClickThresoldTactile;
#endif
                if(m_clicked && m_lastClickTime <= m_doubleClickDelay && m_polygon.GetLineCount() >= 3 &&
                   m_mousePosX > (m_clicked_x - m_doubleClickThresoldTemp) && m_mousePosX < (m_clicked_x + m_doubleClickThresoldTemp) &&
                   m_mousePosY > (m_clicked_y - m_doubleClickThresoldTemp) && m_mousePosY < (m_clicked_y + m_doubleClickThresoldTemp))
                {                       // Si double-clic
                    m_clicked = false;
                    m_lastClickTime = 0;
                    m_polygon.Close();
                }
                else                    // Si clic simple
                {
                    m_clicked = true;
                    m_clicked_x = (int) m_mousePosX;
                    m_clicked_y = (int) m_mousePosY;
                    if(!m_polygon.IsStarted())        // 1er clic
                    {
                        m_polygon.AddPoint(new Vector2((m_mousePosX-xOffset)/bgImgW,
                                                       (m_mousePosY-yOffset)/bgImgH));
//                        GameObject.Find("MainScene").GetComponent<GUIMain>().SetHideHomeButton(true); // TODO recabler
//						GameObject.Find("MainScene").GetComponent<GUIMenuMain>().showHideMenu(GUIMenuMain.Menu.Scene,false);
                    }
                    else  // Clic suivant
                    {
                        Rect bgImg = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
                        if(m_polygon.GetLineCount() > 1 && m_polygon.IsOnOrigin(m_mousePosX,
                                                               m_mousePosY, bgImg, m_addPointThres))
                        {
                            m_polygon.Close(); // zone de dÃ©part : si 3+ segments, polygone fermÃ©
                        }
                        else  // segment prÃ©cÃ©dent validÃ©, nouveau segment
                        {
                            m_polygon.AddPoint(new Vector2((m_mousePosX-xOffset)/bgImgW,
                                                           (m_mousePosY-yOffset)/bgImgH));
                        }
                    } // Si ligne commencÃ©e
                } // Si clic simple
			} // Si clic
		} // fin gestion tracÃ© polygone
        
        // -- Remplissage du masque --
        else if(!maskdone && !m_pleaseWaitUI.IsDisplayingIcon())
            m_pleaseWaitUI.SetDisplayIcon(true);
		else if(!maskdone && m_pleaseWaitUI.IsDisplayingIcon())
		{
//			List<MaskPix> mask = new List<MaskPix>();

            // Masquage du message d'attente (avant l'appel Ã  l'outil qui peut le rÃ©afficher)
            m_pleaseWaitUI.SetDisplayIcon(false);

            // Passage du polygone Ã  l'outil
            if(m_tool == GUIEditTools.EditTool.Eraser)       // Gomme
            {
                m_polygon.SetMainColor(m_invert ? Color.black : Color.clear);
				if(!usefullData.lowTechnologie)
				{
                	m_eraserNode.GetComponent<EraserV2>().AddZone(m_polygon);
				}
            }
            else if(m_tool == GUIEditTools.EditTool.Grass)   // Gazon
            {
                m_polygon.SetMainColor(m_invert ? Color.clear : Color.black);
                m_grassNode.GetComponent<GrassV2>().AddZone(m_polygon);
////                GameObject.Find("/Background/grassImages").GetComponent<EraserMask>().AddEraserZone(mask, 0,0,0,0);
            }
//            else if(m_tool == GUIEditTools.EditTool.Inpaint) // Inpaint
//            {
//                // si l'inpainting n'est pas dÃ©jÃ  actif et s'il y a un background
//                if(GameObject.Find("/Background/backgroundImage").GetComponent("InpaintMain")==null &&
//                   GameObject.Find("/Background/backgroundImage").guiTexture.texture!=null)
//                {
//                    // TODO
//                    GameObject.Find("/Background/backgroundImage").AddComponent("InpaintMain"); // Activation Inpainting
////                    Debug.Log("Ajout script Inpainting patch match ");
//                    GameObject.Find("/Background/backgroundImage").GetComponent<InpaintMain>().SetMask(mask);
////                    GameObject.Find("/Background/backgroundImage").GetComponent<InpaintMain>().masktex = this.MASK;
//
////                    Destroy(this);// AUTO KILL
//                 }
//            } // Appel inpainting
            
            maskdone = false;
//            Debug.Log ("AUTOKILL");
			m_mainScene.GetComponent<GUISubTools>().Validate();
            m_mainCam.GetComponent<Mode2D>().enabled = true;
            m_mainScene.GetComponent<HelpPanel>().enabled = true;
			Destroy(this);
//            GameObject.Find("MainScene").GetComponent<GUIMain>().SetHideHomeButton(false); // TODO recabler
//			GameObject.Find("MainScene").GetComponent<GUIMenuMain>().showHideMenu(GUIMenuMain.Menu.Scene,true);

		}// fin calcul du masque
		
		if (Input.GetKeyUp(KeyCode.Escape)) // Echap pour annuler un polygone
			abort(false);

	} // Update()
	
	//-----------------------------------------------------
	void OnGUI()
	{
//		GUI.skin = skin;
        
		//drawGrid();
        bool mouseOnGUI = InGuiZone();
        Rect bgRect = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
        m_polygon.Draw(m_mousePosX, m_mousePosY, !mouseOnGUI, bgRect);

        if(!m_loupeActive)
        {
            if(GUI.Button(m_cancelRect, "", m_style))
                abort(true);
        }

        #if !UNITY_IPHONE && !UNITY_ANDROID
            if(!mouseOnGUI && !m_polygon.IsClosed()) drawCursor();
        #endif

        if(s_mouseDebug)
            GUI.Button(new Rect(50, 50, 150, 50), "X="+m_mousePosX+", Y="+m_mousePosY);
    } // OnGUI

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= SetGUIrects;
        UsefullEvents.OnResizeWindowEnd -= SetGUIrects;
    }
#endregion

    //-----------------------------------------------------
    private void SetGUIrects()
    {
        m_cancelRect.x = (Screen.width - m_cancelRect.width)/2f;
    }

    //-----------------------------------------------------
    // Retourne true si le point est dans une des zones rÃ©servÃ©es Ã  l'interface (m_guiZones)
    private bool InGuiZone()
    {
        if(!m_loupeActive && PC.In.CursorOnUI(m_cancelRect))
            return true;

        if(m_guiZones != null && !m_polygon.IsStarted())
        {
            if(PC.In.CursorOnUI(m_guiZones.ToArray()))
                return true;
//            for(int i = 0; i<m_guiZones.Count ; i++)
//            {
//                if(x > m_guiZones[i].xMin && x < m_guiZones[i].xMax &&
//                   y > m_guiZones[i].yMin && y < m_guiZones[i].yMax)
//                    return true;
//                //if(m_guiZones[i].Contains(new Vector2(x, Screen.height-y)))
//            }
        }
        return false;
    }

    //-----------------------------------------------------
    private void ResetLoupeRT()
    {
        if(m_loupeRenderTex == null)
        {
            m_loupeRenderTex = new RenderTexture((int)m_mainCam.GetComponent<Camera>().pixelWidth,
                                                 (int)m_mainCam.GetComponent<Camera>().pixelHeight, 24);
        }
        else
        {
            m_loupeRenderTex.Release();
            m_loupeRenderTex.width  = (int)m_mainCam.GetComponent<Camera>().pixelWidth;
            m_loupeRenderTex.height = (int)m_mainCam.GetComponent<Camera>().pixelHeight;
        }
    }
	
    //-----------------------------------------------------
	void drawCursor()
	{
		if(m_invert==false)
			Drawer.DrawIcon(new Vector2(m_mousePosX,m_mousePosY),cursor,cursor.width/1.5f); // #444 modification ici // soit rÃ©duire taille png soit spÃ©cifier taille
		else
			Drawer.DrawIcon(new Vector2(m_mousePosX,m_mousePosY),cursorSub,cursorSub.width/1.5f);
	}
	
	//-----------------------------------------------------
	public void abort(bool destroy)
	{
        m_polygon.Cancel();
		maskdone = false;
		GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().ToggleSceneObjectsTransparent();
//      GameObject.Find("MainScene").GetComponent<GUIMain>().SetHideHomeButton(false); // TODO Recabler
//		GameObject.Find("MainScene").GetComponent<GUIMenuMain>().showHideMenu(GUIMenuMain.Menu.Scene,true);
		if(destroy)
        {
            m_mainCam.GetComponent<Mode2D>().enabled = true;
            m_mainScene.GetComponent<HelpPanel>().enabled = true;
			Camera.main.GetComponent<ObjInteraction>().setActived(true);
			m_mainScene.GetComponent<GUISubTools>().Validate();
	        Destroy(this);
        }
	} // abort()
	
    //-----------------------------------------------------
    public void SetTool(GUIEditTools.EditTool tool)
    {
        m_tool = tool;
    }
    
    //-----------------------------------------------------
	public void SetInvert(bool invert)
	{
        m_invert = invert;
		if(m_polygon!=null)
			m_polygon.SetInvert(m_invert);
		if(_loupeGUI!=null)
			_loupeGUI.SetInvert(m_invert);
	}
    
    //-----------------------------------------------------
    public void AddGuiZone(Rect newZone)
    {
        if(m_guiZones == null)              // Initialisation si besoin de la liste d'Ã©lÃ©ments d'interface 
            m_guiZones = new List<Rect>();  //  (pas dans Start() car cette fonction est appelÃ©e avant)
        m_guiZones.Add(newZone);
//        Debug.Log("Ajout deh zone : "+newZone.x+", "+newZone.y);
    }

} // class PolygonTracer
	