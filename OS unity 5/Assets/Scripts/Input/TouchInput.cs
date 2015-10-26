using UnityEngine;
using System.Collections.Generic;

using Pointcube.Utils;

namespace Pointcube.InputEvents
{

public class TouchInput : CursorInput
{
    #region attributs_&_constructeur
    // Singleton (obligé car une classe statique ne peut pas implémenter une interface)
    protected static  TouchInput s_instance = null;

    // -- Tap --
	override public bool TouchSupported(){return true;}
		
    protected float   m_tapTime1;
    protected bool    m_tapping1;
    protected Vector2 m_tapCurPos1;
    protected int     m_tapReturn1;

    protected float   m_tapTime2;
    protected bool    m_tapping2;
    protected Vector2 m_tapCurPos2;
    protected int     m_tapReturn2;

    // -- Double tap --
    protected float   m_dTapTime;
    protected bool    m_dTapping;                         // 2e tap d'un double clic en cours
    protected bool    m_simpleTapped;                     // tap simple récent ou pas (potentiel double clic)
    protected float   m_lastTapTime;                      // temps (s) depuis le dernier tap simple récent (0 si aucun)
    protected float   m_tapped_x, m_tapped_y;             // position du curseur au tap simple récent (-1 si aucun)
    protected Vector2 m_dTap1stTapPos;
    protected Vector2 m_dTapCurPos;
    protected float   m_dTapDelay;
    protected int     m_dTapReturn;

    protected const float c_dTapDelay = 0.5f;     //0.20f; // temps entre les deux taps (s)
    protected const float c_dTapThresold = 50.0f; //30.0f; // tolérance de position entre 1er et 2e tap (en pixels)

    // -- Drag --
    protected float   m_drag1Time;
    protected bool    m_drag1;
    protected Vector2 m_drag1DeltaMove;
    protected Vector2 m_drag1CursorPos;
    protected int     m_drag1Return;

    protected float   m_drag2Time;
    protected bool    m_drag2;
    protected Vector2 m_drag2DeltaMove;
    protected Vector2 m_drag2CursorPos;
    protected int     m_drag2Return;
		
	protected float   m_dragtwo1Time;
	protected Vector2 m_dragtwo1DeltaMove;
	protected Vector2 m_dragtwo1CursorPos;
	protected int     m_dragtwo1Return;
	
	protected bool    m_dragtwo1;
	protected Vector2 m_dragtwo1LastPos;
	
    // -- Rotate --
    protected float   m_rotateTime;
    protected bool    m_rotating;
    protected Vector2 m_rotateOldT0Pos;
    protected Vector2 m_rotateOldT1Pos;
    protected float   m_rotateAngle;
    protected bool    m_rotateReturn;
        
    protected float   m_drotaTime;
    protected bool    m_drotating;
    protected Vector2 m_drotaOldT0Pos;
    protected Vector2 m_drotaOldT1Pos;
    protected Vector2 m_drotaCursorPos;
    protected Queue<bool> m_recentDrotate;       // type des 5 derniers doubleRotate (true = "slide fingers", false = "rotate fingers")
    protected Queue<bool> m_recentDrotate2;   // type des 20 derniers doubleRotate (true = "slide fingers", false = "rotate fingers")
    protected bool    m_lockSlide;               // on utilise "slide fingers" jusqu'à la fin du geste
    protected bool    m_lockRotate;           // idem "rotate fingers"
    protected float   m_drotaAngle;
    protected bool    m_drotaReturn;        

    // -- Zoom --
    protected float   m_zoomTime;
    protected bool    m_zooming;
    protected float   m_lastZoomDist;
    protected float   m_zoomDelta;
    protected bool    m_zoomReturn;

    protected const float c_zoomAdaptFactor = 75f; // facteur pour adapter le zoom touch au zoom mouse (scrollwheel)

    // -- Scroll --
    protected float   m_scrollHTime;
    protected bool    m_scrollingH;
    protected float   m_scrollHoldX;
    protected float   m_scrollHDelta;
    protected bool    m_scrollHReturn;

    protected float   m_scrollVTime;
    protected bool    m_scrollingV;
    protected float   m_scrollVoldY;
    protected float   m_scrollVDelta;
    protected bool    m_scrollVReturn;

    protected float   m_scrollHVtime;
    protected bool    m_scrollingHV;
    protected Vector2 m_scrollHVoldPos;
    protected bool    m_scrollHVdir;              // Direction précédente
    protected float   m_scrollHVdelta;
    protected bool    m_scrollHVreturn;

    protected const float c_scrollAdaptFactor = 75f; // facteur pour adapter le scroll touch au scroll mouse (scrollwheel)

    // -- Zoom'n'drag --
    protected float   m_zndTime;
    protected bool    m_znding;
    protected float   m_zndOldDist;
    protected float   m_zndDeltaZoom;
    protected Vector2 m_zndDeltaMove;
    protected Vector2 m_zndCursorPos;
    protected bool    m_zndReturn;

    // -- Zoom'n'Rotate --
    protected float   m_znrTime;
    protected bool    m_znring;
    protected Vector2 m_znrOldT0Pos;
    protected Vector2 m_znrOldT1Pos;
    protected float   m_znrDeltaZoom;
    protected float   m_znrDeltaAngle;
    protected Vector2 m_znrCursorPos;
    protected bool    m_znrReturn;

    // -- Zoom'n'Drag'n'Rotate --
    protected float   m_zdrTime;
    protected bool    m_zdring;
    protected Vector2 m_zdrOldT0Pos;
    protected Vector2 m_zdrOldT1Pos;
    protected float   m_zdrDeltaZoom;
    protected float   m_zdrDeltaAngle;
    protected Vector2 m_zdrDeltaMove;
    protected Vector2 m_zdrCursorPos;
    protected bool    m_zdrReturn;
        
    // -- Zoom'n'Drag Vertical'n'Rotate --
    protected float   m_zdVrTime;
    protected bool    m_zdVring;
    protected Vector2 m_zdVrOldT0Pos;
    protected Vector2 m_zdVrOldT1Pos;
    protected float   m_zdVrDeltaZoom;
    protected float   m_zdVrDeltaAngle;
    protected float   m_zdVrDeltaMoveV;
    protected Vector2 m_zdVrCursorPos;
    protected bool    m_zdVrReturn;    
    protected Queue<int> m_recent_zdVr2;   // type des 20 derniers Zoom'n'Drag Vertical'n'Rotate (1 = "Zoom", 2 = "Drag Vertical", 3 = "Rotate")
    protected bool    m_lock_zdVr_DragV;   // on utilise "drag fingers" jusqu'à la fin du geste
    protected bool    m_lock_zdVr_Zoom;    // idem "zoom pincé"
    protected Queue<float> m_zdVrRecentAngles;   // deltaAngle des 20 dernières frames

    protected const float c_zdrRotatThres = 0.67f; // en degrés

    // -- CursorPos --
    protected float   m_cPosTime;
    protected Vector2 m_cPos;
    protected Vector2 m_oldCpos;
    protected Vector2 m_cPosInvY;

    // -- Variables poubelle --
    protected static Vector2 m_trashVect;                // Vecteur poubelle pour supprimer les arguments out
    protected static float   m_trashFloat;               // float idem

    protected const string DBGTAG = "MouseInput : ";

    //-----------------------------------------------------
    protected TouchInput()
    {
        // -- Tap --
        m_tapTime1       = 0f;
        m_tapping1       = false;
        m_tapCurPos1     = new Vector2(-1f, -1f);
        m_tapReturn1     = 0;
                         
        m_tapTime2       = 0f;
        m_tapping2       = false;
        m_tapCurPos2     = new Vector2(-1f, -1f);
        m_tapReturn2     = 0;
                         
        // -- Double tap --
        m_dTapTime       = 0f;
        m_dTapping       = false;
        m_simpleTapped   = false;
        m_tapped_x       = -1f;
        m_tapped_y       = -1f;
        m_lastTapTime    = 0f;
        m_dTap1stTapPos  = new Vector2(-1f, -1f);
        m_dTapCurPos     = new Vector2(-1f, -1f);
        m_dTapDelay      = -1f;
        m_dTapReturn     = 0;

        // -- Drag --
        m_drag1Time      = 0f;
        m_drag1          = false;
        m_drag1DeltaMove = new Vector2( 0f,  0f);
        m_drag1CursorPos = new Vector2(-1f, -1f);
        m_drag1Return    = 0;

        m_drag2Time      = 0f;
        m_drag2          = false;
        m_drag2DeltaMove = new Vector2( 0f,  0f);
        m_drag2CursorPos = new Vector2(-1f, -1f);
        m_drag2Return    = 0;

        // -- Rotate --
        m_rotateTime     = 0f;
        m_rotating       = false;
        m_rotateOldT0Pos = new Vector2(-1f, -1f);
        m_rotateOldT1Pos = new Vector2(-1f, -1f);
        m_rotateAngle    = 0f;
        m_rotateReturn   = false;
            
        m_drotaTime      = 0f;
        m_drotating      = false;
        m_drotaOldT0Pos  = new Vector2(-1f, -1);
        m_drotaOldT1Pos  = new Vector2(-1f, -1);
        m_drotaCursorPos = new Vector2(-1f, -1);
        m_recentDrotate  = new Queue<bool>();
        m_recentDrotate2 = new Queue<bool>();
        m_lockSlide      = false;
        m_lockRotate      = false;
        m_drotaAngle     = 0f;
        m_drotaReturn    = false;
            
        // -- Zoom --
        m_zoomTime       = 0f;
        m_zooming        = false;
        m_lastZoomDist   = 0f;
        m_zoomDelta      = 0f;
        m_zoomReturn     = false;

        // -- Scroll --
        m_scrollHTime    = 0f;
        m_scrollingH     = false;
        m_scrollHoldX    = 0f;
        m_scrollHDelta   = 0f;
        m_scrollHReturn  = false;

        m_scrollVTime    = 0f;
        m_scrollingV     = false;
        m_scrollVoldY    = 0f;
        m_scrollVDelta   = 0f;
        m_scrollVReturn  = false;

        m_scrollHVtime   = 0f;
        m_scrollingHV    = false;
        m_scrollHVoldPos = new Vector2(-1f, -1f);
        m_scrollHVdir    = true;
        m_scrollHVdelta  = 0f;
        m_scrollHVreturn = false;

        // -- Zoom'n'drag --
        m_zndTime        = 0f;
        m_znding         = false;
        m_zndOldDist     = -1f;
        m_zndDeltaZoom   = 0f;
        m_zndDeltaMove   = new Vector2( 0f,  0f);
        m_zndCursorPos   = new Vector2(-1f, -1f);
        m_zndReturn      = false;

        // -- Zoom'n'Rotate --
        m_znrTime        = 0f;
        m_znring         = false;
        m_znrOldT0Pos    = new Vector2(-1f, -1f);
        m_znrOldT1Pos    = new Vector2(-1f, -1f);
        m_znrDeltaZoom   = 0f;
        m_znrDeltaAngle  = 0f;
        m_znrCursorPos   = new Vector2(-1f, -1f);
        m_znrReturn      = false;

        // -- Zoom'n'Drag'n'Rotate --
        m_zdrTime        = 0f;
        m_zdring         = false;
        m_zdrOldT0Pos    = new Vector2(-1f, -1f);
        m_zdrOldT1Pos    = new Vector2(-1f, -1f);
        m_zdrDeltaZoom   = 0f;
        m_zdrDeltaAngle  = 0f;
        m_zdrDeltaMove   = new Vector2(0f, 0f);
        m_zdrCursorPos   = new Vector2(0f, 0f);
        m_zdrReturn      = false;
            
        // -- Zoom'n'Drag Vertical'n'Rotate --    
        m_zdVrTime          = 0f;
        m_zdVring           = false;
        m_zdVrOldT0Pos      = new Vector2(-1f, -1f);
        m_zdVrOldT1Pos      = new Vector2(-1f, -1f);
        m_zdVrDeltaZoom     = 0.0f;
        m_zdVrDeltaAngle    = 0.0f;
        m_zdVrDeltaMoveV    = 0f;
        m_zdVrCursorPos     = new Vector2(0f, 0f);
        m_recent_zdVr2      = new Queue<int>();
        m_lock_zdVr_DragV   = false;
        m_lock_zdVr_Zoom    = false;
		m_zdVrRecentAngles  = new Queue<float>();

        // -- CursorPos --
        m_cPosTime       = 0f;
        m_cPos           = new Vector2(-1f, -1f);
        m_oldCpos        = new Vector2(-1f, -1f);
        m_cPosInvY       = new Vector2(-1f, -1f);

        // -- variables poubelles --
        m_trashVect      = new Vector2(-1f, -1f);
        m_trashFloat     = -1f;
    }

    //-----------------------------------------------------
    public static TouchInput GetInstance()
    {
        if(s_instance == null)
            s_instance = new TouchInput();

        return s_instance;
    }
		

    #endregion attributs_&_constructeur
		
		
	#region basic Input elements
		override public PcTouch touches(int index){			
				PcTouch r=new PcTouch();
				r.position=Input.touches[index].position;
				r.phase=Input.touches[index].phase;
				r.deltaPosition=Input.touches[index].deltaPosition;
				return r;
		}
		
		public override int touchCount{
			get {
				return Input.touchCount;
			}
		}
	#endregion
		
    #region implementation_events
    //-- Clics simples --
    override public bool Click1Down()                      { return HandleTap1(out m_trashVect) == 1; }
    override public bool Click1Down(out Vector2 cursorPos) { return HandleTap1(out cursorPos  ) == 1; }
    override public bool Click1Hold()                      { return HandleTap1(out m_trashVect) == 2; }
    override public bool Click1Hold(out Vector2 cursorPos) { return HandleTap1(out cursorPos  ) == 2; }
    override public bool Click1Up()                        { return HandleTap1(out m_trashVect) == 3; }
    override public bool Click1Up(out Vector2 cursorPos)   { return HandleTap1(out cursorPos  ) == 3; }

    override public bool Click2Down()                      { return HandleTap2(out m_trashVect) == 1; }
    override public bool Click2Down(out Vector2 cursorPos) { return HandleTap2(out cursorPos  ) == 1; }
    override public bool Click2Hold()                      { return HandleTap2(out m_trashVect) == 2; }
    override public bool Click2Hold(out Vector2 cursorPos) { return HandleTap2(out cursorPos  ) == 2; }
    override public bool Click2Up()                        { return HandleTap2(out m_trashVect) == 3; }
    override public bool Click2Up(out Vector2 cursorPos)   { return HandleTap2(out cursorPos  ) == 3; }
    override public bool Click3Up()                        { return HandleTap2(out m_trashVect) == 3; }

    // Retourne true si clic gauche up et clic droit non actif, ou clic droit terminé et clic gauche non actif.
    override public bool ClickEnded()
    {
        int t1 = HandleTap1(out m_trashVect);
        int t2 = HandleTap2(out m_trashVect);
        return (t1 == 3 && t2 != 1 && t2 != 2) || (t2 == 3 && t1 != 1 && t1 != 2);
    }

    // -- Doubles clics/taps --
    override public bool DoubleClickDown()
        { return HandleDoubleTap(out m_trashVect, out m_trashVect, out m_trashFloat) == 1; }
    override public bool DoubleClickDown(out Vector2 currentPos, out Vector2 firstClick, out float clickDelay)
        { return HandleDoubleTap(out currentPos, out firstClick, out clickDelay) == 1; }

    override public bool DoubleClickHold()
        { return HandleDoubleTap(out m_trashVect, out m_trashVect, out m_trashFloat) == 2; }
    override public bool DoubleClickHold(out Vector2 currentPos, out Vector2 firstClick, out float clickDelay)
        { return HandleDoubleTap(out currentPos, out firstClick, out clickDelay) == 2; }

    override public bool DoubleClickUp()
        { return HandleDoubleTap(out m_trashVect, out m_trashVect, out m_trashFloat) == 3; }
    override public bool DoubleClickUp(out Vector2 currentPos, out Vector2 firstClick, out float clickDelay)
        { return HandleDoubleTap(out currentPos, out firstClick, out clickDelay) == 3; }

    // -- Drags --
    override public bool Drag1Start()
        { return HandleDrag1(out m_trashVect, out m_trashVect) == 1; }
    override public bool Drag1Start(out Vector2 deltaMove)
        { return HandleDrag1(out deltaMove, out m_trashVect)   == 1; }
    override public bool Drag1Start(out Vector2 deltaMove, out Vector2 cursorPos)
        { return HandleDrag1(out deltaMove, out cursorPos)     == 1; }

    override public bool Drag1()
        { return HandleDrag1(out m_trashVect, out m_trashVect) == 2; }
    override public bool Drag1(out Vector2 deltaMove)
        { return HandleDrag1(out deltaMove, out m_trashVect)   == 2; }
    override public bool Drag1(out Vector2 deltaMove, out Vector2 cursorPos)
        { return HandleDrag1(out deltaMove, out cursorPos)     == 2; }

    override public bool Drag1End()
        { return HandleDrag1(out m_trashVect, out m_trashVect) == 3; }
    override public bool Drag1End(out Vector2 deltaMove)
        { return HandleDrag1(out deltaMove, out m_trashVect)   == 3; }
    override public bool Drag1End(out Vector2 deltaMove, out Vector2 cursorPos)
        { return HandleDrag1(out deltaMove, out cursorPos)     == 3; }

    override public bool Drag2Start()
        { return HandleDrag2(out m_trashVect, out m_trashVect) == 1; }
    override public bool Drag2Start(out Vector2 deltaMove)
        { return HandleDrag2(out deltaMove, out m_trashVect)   == 1; }
    override public bool Drag2Start(out Vector2 deltaMove, out Vector2 cursorPos)
        { return HandleDrag2(out deltaMove, out cursorPos)     == 1; }

    override public bool Drag2()
        { return HandleDrag2(out m_trashVect, out m_trashVect) == 2; }
    override public bool Drag2(out Vector2 deltaMove)
        { return HandleDrag2(out deltaMove, out m_trashVect)   == 2; }
    override public bool Drag2(out Vector2 deltaMove, out Vector2 cursorPos)
        { return HandleDrag2(out deltaMove, out cursorPos)     == 2; }

	override public bool DragTwo1()
		{ return HandleDragTwo1(out m_trashVect, out m_trashVect) == 2; }
	override public bool DragTwo1(out Vector2 deltaMove)
		{ return HandleDragTwo1(out deltaMove, out m_trashVect)   == 2; }
	override public bool DragTwo1(out Vector2 deltaMove, out Vector2 cursorPos)
		{ return HandleDragTwo1(out deltaMove, out cursorPos)     == 2; }

    override public bool Drag2End()
        { return HandleDrag2(out m_trashVect, out m_trashVect) == 3; }
    override public bool Drag2End(out Vector2 deltaMove)
        { return HandleDrag2(out deltaMove, out m_trashVect)   == 3; }
    override public bool Drag2End(out Vector2 deltaMove, out Vector2 cursorPos)
        { return HandleDrag2(out deltaMove, out cursorPos)     == 3; }

    // -- Rotate --
    override public bool Rotate()
        { return Rotate2D(out m_trashFloat); }
    override public bool Rotate(out float deltaAngle)
        { return Rotate2D(out deltaAngle);   }
        
    override public bool DoubleRotate()
        { return DoubleRotate2D(out m_trashFloat); }
    override public bool DoubleRotate(out float deltaAngle)
        { return DoubleRotate2D(out deltaAngle); }
        
    // -- Zoom & scroll --
    override public bool Zoom()
        { return Zoom2(out m_trashFloat); }
    override public bool Zoom(out float deltaZoom)
        { return Zoom2(out deltaZoom); }

    override public bool ScrollH()
        { return Scroll1H(out m_trashFloat); }
    override public bool ScrollH(out float deltaScroll)
        { return Scroll1H(out deltaScroll);  }
    override public bool ScrollV()
        { return Scroll1V(out m_trashFloat); }
    override public bool ScrollV(out float deltaScroll)
        { return Scroll1V(out deltaScroll);  }

    override public bool ScrollHV()
        { return Scroll1HnV(out m_trashFloat); }
    override public bool ScrollHV(out float deltaScroll)
        { return Scroll1HnV(out deltaScroll);  }

    override public bool ScrollViewH()
        { return Scroll1H(out m_trashFloat); }
    override public bool ScrollViewH(out float deltaScroll)
        { return Scroll1H(out deltaScroll);  }
    override public bool ScrollViewV()
        { return Scroll1V(out m_trashFloat); }
    override public bool ScrollViewV(out float deltaScroll)
        { return Scroll1V(out deltaScroll);  }

    //-----------------------------------------------------
    // Événements multiples
    override public bool Zoom_n_drag1(out float deltaZoom, out Vector2 deltaMove)
        { return Zoom_n_drag(out deltaZoom, out deltaMove, out m_trashVect); }
    override public bool Zoom_n_drag1(out float deltaZoom, out Vector2 deltaMove, out Vector2 cursorPos)
        { return Zoom_n_drag(out deltaZoom, out deltaMove, out cursorPos); }

    override public bool Zoom_n_rotate2D(out float deltaZoom, out float deltaAngle)
        { return Zoom_n_rotate(out deltaZoom, out deltaAngle, out m_trashVect); }
    override public bool Zoom_n_rotate2D(out float deltaZoom, out float deltaAngle, out Vector2 cursorPos)
        { return Zoom_n_rotate(out deltaZoom, out deltaAngle, out cursorPos); }

    override public bool Zoom_n_drag1_n_rotate2D(out float deltaZoom, out Vector2 deltaMove, out float deltaAngle)
        { return Zoom_n_drag_n_rotate(out deltaZoom, out deltaMove, out deltaAngle, out m_trashVect); }
    override public bool Zoom_n_drag1_n_rotate2D(out float deltaZoom, out Vector2 deltaMove,
                                                 out float deltaAngle, out Vector2 cursorPos)
        { return Zoom_n_drag_n_rotate(out deltaZoom, out deltaMove, out deltaAngle, out cursorPos); }
        
    override public bool Zoom_n_drag2V_n_rotate2D(out float deltaZoom, out float deltaMoveV,
                                                 out float deltaAngles)
        { return Zoom_n_dragV_n_rotate(out deltaZoom, out deltaMoveV, out deltaAngles); }

    //-----------------------------------------------------
    override public Vector2 GetCursorPos()
    {
        if(m_cPosTime != Time.time)
        {
            m_cPosTime = Time.time;
            m_cPos.Set(0f, 0f);

            if(Input.touchCount > 0)
            {
                foreach(Touch t in Input.touches)
                {
                    m_cPos.x += t.position.x;
                    m_cPos.y += t.position.y;
                }

                m_cPos /= Input.touches.Length;
                m_cPos.Set( m_cPos.x < 0.0f ? Mathf.Ceil(m_cPos.x-0.5f) : Mathf.Floor(m_cPos.x+0.5f),
                            m_cPos.y < 0.0f ? Mathf.Ceil(m_cPos.y-0.5f) : Mathf.Floor(m_cPos.y+0.5f));

                m_oldCpos.Set(m_cPos.x, m_cPos.y);
                // Note : on n'utilise pas Mathf.Round car en cas de *.5, il retourne le l'entier pair le plus proche
                // au lieu de l'entier supérieur.
            }
            else
                m_cPos.Set(m_oldCpos.x, m_oldCpos.y);
        }
        return m_cPos;
    }

    //-----------------------------------------------------
    override public Vector2 GetCursorPosInvY()
    {
        GetCursorPos();
        m_cPosInvY.Set(m_cPos.x, Screen.height - m_cPos.y);
        return m_cPosInvY;
    }

    //-----------------------------------------------------
    override public bool ClickOnUI(Rect[] uiRects)
    {
        bool clickOnUI = false;

        for(int i=0; i<Input.touchCount; i++)
        {
            if(Input.touches[i].phase == TouchPhase.Began && CursorOnUI(uiRects))
                clickOnUI = true;
        }
        return clickOnUI;
    }

    #endregion

    #region fonctions_specifiques

    #region tap_&_double_tap
    //----------------------------------------------------
    protected int HandleTap1(out Vector2 cursorPos)
    {
        if(m_tapTime1 != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_tapTime1 = Time.time;

            if(Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if(t.phase == TouchPhase.Began)
                {
                    m_tapping1   = true;
                    m_tapCurPos1.Set(GetCursorPos().x, GetCursorPos().y);
                    m_tapReturn1 = 1;               // begin
                }
                else if(m_tapping1 && (t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved))
                {
                    m_tapCurPos1.Set(GetCursorPos().x, GetCursorPos().y);
                    m_tapReturn1 = 2;               // hold
                }
                else if(m_tapping1) //&& (t.phase == TouchPhase.Canceled))
                {
                    m_tapping1   = false;
                    m_tapCurPos1.Set(GetCursorPos().x, GetCursorPos().y);
                    m_tapReturn1 = 3;               // end
                }
                else
                {
                    m_tapCurPos1.Set(m_trashVect.x, m_trashVect.y);
                    m_tapReturn1 = 0;               // Aucun événement
                }
            } // if touchCount == count
            else if(m_tapping1 && Input.touchCount > 1)
            {
                m_tapCurPos1.Set(GetCursorPos().x, GetCursorPos().y);
                m_tapReturn1 = 2;               // hold
            }
            else if(m_tapping1 /*&& Input.touchCount == 0*/)
            {
                m_tapping1   = false;
                m_tapCurPos1.Set(GetCursorPos().x, GetCursorPos().y);
                m_tapReturn1 = 3;               // end
            }
            else
            {
                m_tapCurPos1.Set(m_trashVect.x, m_trashVect.y);
                m_tapReturn1 = 0;                   // Aucun événement
            }
        } // if taptime != time.time

        cursorPos = m_tapCurPos1;
        return m_tapReturn1;
    } // HandleTap1

    //----------------------------------------------------
    protected int HandleTap2(out Vector2 cursorPos)
    {
        if(m_tapTime2 != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_tapTime2 = Time.time;

            if(Input.touchCount == 2)
            {
                Touch t1 = Input.GetTouch(0);
                Touch t2 = Input.GetTouch(1);
                if(t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
                {
                    m_tapping2   = true;
                    m_tapCurPos2.Set(GetCursorPos().x, GetCursorPos().y);
                    m_tapReturn2 = 1;               // begin
                }
                else if(m_tapping2 && (t1.phase == TouchPhase.Stationary || t1.phase == TouchPhase.Moved) &&
                                      (t2.phase == TouchPhase.Stationary || t2.phase == TouchPhase.Moved))
                {
                    m_tapCurPos2.Set(GetCursorPos().x, GetCursorPos().y);
                    m_tapReturn2 = 2;               // hold
                }
                else if(m_tapping2)
                {
                    m_tapping2   = false;
                    m_tapCurPos2.Set(GetCursorPos().x, GetCursorPos().y);
                    m_tapReturn2 = 3;               // end
                }
                else
                {
                    m_tapCurPos2.Set(m_trashVect.x, m_trashVect.y);
                    m_tapReturn2 = 0;               // Aucun événement
                }
            } // if touchCount == 2
            else if(m_tapping2 && Input.touchCount > 2)
            {
                m_tapCurPos2.Set(GetCursorPos().x, GetCursorPos().y);
                m_tapReturn2 = 2;                   // hold
            }
            else if(m_tapping2)
            {
                m_tapping2   = false;
                m_tapCurPos2.Set(GetCursorPos().x, GetCursorPos().y);
                m_tapReturn2 = 3;                   // end
            }
            else
            {
                m_tapCurPos2.Set(m_trashVect.x, m_trashVect.y);
                m_tapReturn2 = 0;                   // Aucun événement
            }
        } // if taptime != time.time

        cursorPos = m_tapCurPos2;
        return m_tapReturn2;
    } // HandleTap2

    //----------------------------------------------------
    protected int HandleDoubleTap(out Vector2 cursorPos, out Vector2 firstTapPos, out float delay)
    {
        if(m_dTapTime != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_dTapTime = Time.time;

            if(Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if(t.phase == TouchPhase.Began)
                {
                    float x = GetCursorPos().x;
                    float y = GetCursorPos().y;
    
                    if(m_simpleTapped && Time.time-m_lastTapTime <= c_dTapDelay &&
                       x > (m_tapped_x - c_dTapThresold) && x < (m_tapped_x + c_dTapThresold) &&
                       y > (m_tapped_y - c_dTapThresold) && y < (m_tapped_y + c_dTapThresold))
                    {
                        m_dTapCurPos.Set(x, y);
                        m_dTap1stTapPos.Set(m_tapped_x, m_tapped_y);
                        m_dTapDelay       = Time.time - m_lastTapTime;
                        m_dTapReturn      = 1;   // Begin

                        m_simpleTapped  = false;
                        m_lastTapTime   = 0;
                        m_dTapping      = true;
                    }
                    else    // simple tap (potentiel 1er tap d'un futur double tap)
                    {
                        m_simpleTapped = true;
                        m_lastTapTime = Time.time;
                        m_tapped_x = x;
                        m_tapped_y = y;
    
                        m_dTapCurPos.Set(m_trashVect.x, m_trashVect.y);
                        m_dTap1stTapPos.Set(m_trashVect.x, m_trashVect.y);
                        m_dTapDelay = m_trashFloat;
                        m_dTapReturn = 0; // Pas d'événement
                    }
                } // TouchPhase.Began
                else if(m_dTapping && t.phase == TouchPhase.Stationary)
                {
                    m_dTapCurPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_dTap1stTapPos.Set(m_tapped_x, m_tapped_y);
                    m_dTapDelay     = Time.time - m_lastTapTime;
                    m_dTapReturn    = 2;    // Hold
                } // TouchPhase.Moved || Stationary
                else if(m_dTapping)// && t.phase == TouchPhase.Ended)
                {
                    m_dTapCurPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_dTap1stTapPos.Set(m_tapped_x, m_tapped_y);
                    m_dTapDelay     = Time.time - m_lastTapTime;
                    m_dTapReturn    = 3;    // End

                    // -- Réinitialisation --
                    m_dTapping      = false;
                    m_simpleTapped  = false;
                    m_lastTapTime   = 0f;
                    m_tapped_x      = -1f;
                    m_tapped_y      = -1f;
                }
                else
                {
                    m_dTapCurPos.Set(m_trashVect.x, m_trashVect.y);
                    m_dTap1stTapPos.Set(m_trashVect.x, m_trashVect.y);
                    m_dTapDelay  = m_trashFloat;
                    m_dTapReturn = 0;   // aucun événement
                }
            }
            else if(m_dTapping)
            {
//                m_dTapCurPos.Set(t.position.x, t.position.y); // on garde la position précédente, difficile de récupérer l'actuelle
                m_dTap1stTapPos.Set(m_tapped_x, m_tapped_y);
                m_dTapDelay     = Time.time - m_lastTapTime;
                m_dTapReturn    = 3;    // End

                // -- Réinitialisation --
                m_dTapping      = false;
                m_simpleTapped  = false;
                m_lastTapTime   = 0f;
                m_tapped_x      = -1f;
                m_tapped_y      = -1f;
            }
            else
            {
                m_dTapCurPos.Set(m_trashVect.x, m_trashVect.y);
                m_dTap1stTapPos.Set(m_trashVect.x, m_trashVect.y);
                m_dTapDelay  = m_trashFloat;
                m_dTapReturn = 0;       // aucun événement
            }
        }

        cursorPos = new Vector2(m_dTapCurPos.x, m_dTapCurPos.y);
        firstTapPos = new Vector2(m_dTap1stTapPos.x, m_dTap1stTapPos.y);
        delay = m_dTapDelay;
        return m_dTapReturn;
    }
    #endregion

    #region drag
    //-----------------------------------------------------
    protected int HandleDrag1(out Vector2 deltaMove, out Vector2 cursorPos)
    {
        if(m_drag1Time != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_drag1Time = Time.time;

            if(Input.touchCount == 1)
            {
                Touch t = Input.touches[0];
                if(!m_drag1 && (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved ||
                                t.phase == TouchPhase.Stationary))
                {
                    m_drag1DeltaMove.Set(0f, 0f);
                    m_drag1CursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_drag1Return = 1;          // Pas de drag mais début potentiel d'un drag
                    m_drag1 = true;
                }
                else if(m_drag1 && (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary))
                {
                    m_drag1DeltaMove.Set(GetCursorPos().x - m_drag1CursorPos.x,
                                         GetCursorPos().y - m_drag1CursorPos.y);
                    m_drag1CursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_drag1Return = (m_drag1DeltaMove.x != 0f || m_drag1DeltaMove.y != 0f) ? 2 : 0; // Drag potentiel
                }
                else
                {
                    m_drag1Return = m_drag1 ? 3 : 0;
                    m_drag1 = false;
                    m_drag1DeltaMove.Set(0f, 0f);
                    m_drag1CursorPos.Set(m_trashVect.x, m_trashVect.y);
                }
            } // 1 touch
            else
            {
                m_drag1Return = m_drag1 ? 3 : 0;
                m_drag1 = false;
                m_drag1DeltaMove.Set(0f, 0f);
                m_drag1CursorPos.Set(m_trashVect.x, m_trashVect.y);
            }
        }

        deltaMove = m_drag1DeltaMove;
        cursorPos = m_drag1CursorPos;
        return m_drag1Return;
    }

    //-----------------------------------------------------
    protected int HandleDrag2(out Vector2 deltaMove, out Vector2 cursorPos)
    {
        if(m_drag2Time != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_drag2Time = Time.time;

            if(Input.touchCount == 2)
            {
                Touch t0 = Input.touches[0];
                Touch t1 = Input.touches[1];
                if(!m_drag2 && !EndedOrCanceled(t0) && !EndedOrCanceled(t1))
                {
                    m_drag2DeltaMove.Set(0f, 0f);
                    m_drag2CursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_drag2Return = 1;          // Pas de drag mais début potentiel d'un drag
                    m_drag2       = true;
                }
                else if(m_drag2 && ((t0.phase == TouchPhase.Moved || t0.phase == TouchPhase.Stationary) &&
                                    (t1.phase == TouchPhase.Moved || t1.phase == TouchPhase.Stationary)))
                {
                    m_drag2DeltaMove.Set(GetCursorPos().x - m_drag2CursorPos.x,
                                         GetCursorPos().y - m_drag2CursorPos.y);
                    m_drag2CursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_drag2Return = /*(m_drag2DeltaMove.x != 0f || m_drag2DeltaMove.y != 0f) ?*/ 2;
                }
                else
                {
                    m_drag2Return = m_drag2 ? 3 : 0;
                    m_drag2 = false;
                    m_drag2DeltaMove.Set(0f, 0f);
                    m_drag2CursorPos.Set(m_trashVect.x, m_trashVect.y);
                }
            }   // 1 touch
            else
            {
                m_drag2Return = m_drag2 ? 3 : 0;
                m_drag2 = false;
                m_drag2DeltaMove.Set(0f, 0f);
                m_drag2CursorPos.Set(m_trashVect.x, m_trashVect.y);
            }
        }

        deltaMove = m_drag2DeltaMove;
        cursorPos = m_drag2CursorPos;
        return m_drag2Return;
		}
		
		private int HandleDragTwo1(out Vector2 deltaMove, out Vector2 cursorPos)
		{
			if(m_dragtwo1Time != Time.time)
			{
				m_dragtwo1Time = Time.time;
				
				if(!m_dragtwo1 && Input.GetMouseButton(1) && Input.GetMouseButton(0))
				{
					m_dragtwo1DeltaMove.Set(0f, 0f);
					m_dragtwo1CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
					m_dragtwo1Return = 1;
					
					m_dragtwo1       = true;
					m_dragtwo1LastPos.Set(Input.mousePosition.x, Input.mousePosition.y);
				}
				else if(m_dragtwo1 && Input.GetMouseButton(1) && Input.GetMouseButton(0))
				{
					m_dragtwo1DeltaMove.Set(Input.mousePosition.x - m_dragtwo1LastPos.x,
					                        Input.mousePosition.y - m_dragtwo1LastPos.y);
					m_dragtwo1CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
					m_dragtwo1Return = /*(m_drag2DeltaMove.x != 0f || m_drag2DeltaMove.y != 0f) ?*/ 2;
					
					m_dragtwo1LastPos.Set(Input.mousePosition.x, Input.mousePosition.y);
				}
				else
				{
					m_dragtwo1DeltaMove.Set(0f, 0f);
					m_dragtwo1CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
					m_dragtwo1Return = (m_drag2 ? 3 : 0);
					
					m_dragtwo1 = false;
					m_dragtwo1LastPos.Set(-1f, -1f);
				}
			}
			
			deltaMove = m_dragtwo1DeltaMove;
			cursorPos = m_dragtwo1CursorPos;
			return m_dragtwo1Return;
		}
		#endregion
		
		#region rotate
		//-----------------------------------------------------
		protected bool Rotate2D(out float deltaAngle)
    {
        if(m_rotateTime != Time.time)
        {
            m_rotateTime = Time.time;

            if(Input.touchCount == 2)
            {
                Touch t0 = Input.touches[0];
                Touch t1 = Input.touches[1];
                if(!m_rotating && !EndedOrCanceled(t0) && !EndedOrCanceled(t1))
                {
                    m_rotating = true;
                    m_rotateOldT0Pos.Set(t0.position.x, t0.position.y);
                    m_rotateOldT1Pos.Set(t1.position.x, t1.position.y);
                    m_rotateAngle = 0f;
                    m_rotateReturn = false;
                }
                else if(m_rotating && ((t0.phase == TouchPhase.Moved || t0.phase == TouchPhase.Stationary) &&
                                       (t1.phase == TouchPhase.Moved || t1.phase == TouchPhase.Stationary)))
                {
                    Vector2 oldDir = m_rotateOldT1Pos - m_rotateOldT0Pos;
                    float angle = Vector2.Angle(oldDir,(t1.position - t0.position));
                    Vector3 cross = Vector3.Cross(oldDir,(t1.position - t0.position));
                    if(cross.z < 0) angle = 360 - angle;

                    m_rotateOldT0Pos.Set(t0.position.x, t0.position.y);
                    m_rotateOldT1Pos.Set(t1.position.x, t1.position.y);
                    m_rotateAngle = angle;
                    m_rotateReturn = true;
                }
                else
                {
                    m_rotating = false;
                    m_rotateOldT0Pos.Set(-1f, -1f);
                    m_rotateOldT1Pos.Set(-1f, -1f);
                    m_rotateAngle = 0f;
                    m_rotateReturn = false;
                }
            }
            else
            {
                m_rotating = false;
                m_rotateOldT0Pos.Set(-1f, -1f);
                m_rotateOldT1Pos.Set(-1f, -1f);
                m_rotateAngle = 0f;
                m_rotateReturn = false;
            }
        }

        deltaAngle = m_rotateAngle;
        return m_rotateReturn;
    }
    
        //-----------------------------------------------------
        // NOTE : Ne marchera pas bien sur un petit écran (smartphone, ...)
        protected bool DoubleRotate2D(out float deltaAngle)
        {
            if(m_drotaTime != Time.time)
            {
                m_drotaTime = Time.time;

                if(Input.touchCount == 2)
                {
                    Touch t0 = Input.touches[0];
                    Touch t1 = Input.touches[1];
                    if(!m_drotating && !EndedOrCanceled(t1) && !EndedOrCanceled(t0))
                    {
                        m_drotating = true;
                        m_drotaOldT0Pos.Set(t0.position.x, t0.position.y);
                        m_drotaOldT1Pos.Set(t1.position.x, t1.position.y);
                        m_drotaCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                        m_recentDrotate.Clear();
                        m_recentDrotate2.Clear();
                        m_lockSlide = false;
                        m_lockRotate = false;
                        m_drotaAngle = 0f;
                        m_drotaReturn = false;
                    }
                    else if(m_drotating && ((t0.phase == TouchPhase.Moved || t0.phase == TouchPhase.Stationary) &&
                                            (t1.phase == TouchPhase.Moved || t1.phase == TouchPhase.Stationary)))
                    {
                        float angle = 0f;
                        Vector2 deltaMoveT0 = t0.position - m_drotaOldT0Pos;
                        Vector2 deltaMoveT1 = t1.position - m_drotaOldT1Pos;
                        float   deltaMoveX  = (deltaMoveT0.x + deltaMoveT1.x)/2f;
                        
                        int recentSlides = 0;
                        int recentRotates = 0;
                        foreach(bool b in m_recentDrotate2)
                        {
                            if(b) recentSlides++;
                            else  recentRotates++;
                        }
                        if(recentSlides > 20*0.8)
                            m_lockSlide = true;
                        else if(recentRotates > 20*0.8)
                            m_lockRotate = true;
                        
                        if(m_lockSlide || (!m_lockRotate && (Mathf.Abs(deltaMoveX) >= 1f &&
                                                             (TouchesGoSameWays(deltaMoveT0, deltaMoveT1) ||
                                                              (OneTouchStationary(deltaMoveT0, deltaMoveT1) &&
                                                                !TouchesAreClose(t0.position, t1.position)    ) ))))
                        {
                            angle = (GetCursorPos().x - m_drotaCursorPos.x) * 360f * 1.1f / Screen.width; 
                            
                            if(m_recentDrotate.Count == 5)
                                m_recentDrotate.Dequeue();
                            m_recentDrotate.Enqueue(true);
                            
                            if(m_recentDrotate2.Count == 20)
                                m_recentDrotate2.Dequeue();
                            m_recentDrotate2.Enqueue(true);
                        }
                        else
                        {
                            // -- Rotation des touch --
                            Vector2 oldDir = m_drotaOldT1Pos - m_drotaOldT0Pos;
                            angle = Vector2.Angle(oldDir,(t1.position - t0.position));
                            Vector3 cross = Vector3.Cross(oldDir,(t1.position - t0.position));
                            
                            int recentSlideCount = 0;
                            foreach(bool b in m_recentDrotate)
                                if(b) recentSlideCount++;
                            // Si angle tout petit et que déplacement moyen observé, alors on repasse en slide
                            // (c'est sûrement un slide pou réglage précis)
                            if( !m_lockRotate && ((Mathf.Abs(angle) < 1f && Mathf.Abs(deltaMoveX) >= 0.5f && deltaMoveT0.x != 0f && deltaMoveT1.x != 0f) ||
                                                  (recentSlideCount >= 3 && (Mathf.Abs(angle) < 2f && (deltaMoveT0.x != 0f || deltaMoveT1.x != 0f)) )))
                            {
                                angle = (GetCursorPos().x - m_drotaCursorPos.x) * 360f * 1.1f / Screen.width;
//                                DbgUtils.Log(DBGTAG," RtpSLIDE a="+angle+" dmx="+Mathf.Abs(deltaMoveX)+", t0="+deltaMoveT0.x+", t1="+deltaMoveT1.x+" c="+recentSlideCount+" sr="+recentSlides+"/"+recentRotates+", lockr="+m_lockRotate);
                                
                                if(m_recentDrotate.Count == 5)
                                    m_recentDrotate.Dequeue();
                                m_recentDrotate.Enqueue(true);
                                
                                if(m_recentDrotate2.Count == 20)
                                    m_recentDrotate2.Dequeue();
                                m_recentDrotate2.Enqueue(true);
                            }
                            else
                            {
                                    if(cross.z < 0) angle = 360 - angle;
//                                DbgUtils.Log(DBGTAG," ROTAT a="+angle+" dmx="+Mathf.Abs(deltaMoveX)+", t0="+deltaMoveT0.x+", t1="+deltaMoveT1.x+" c="+recentSlideCount+" sr="+recentSlides+"/"+recentRotates+", lockr="+m_lockRotate);
                                
                                if(m_recentDrotate.Count == 5)
                                    m_recentDrotate.Dequeue();
                                m_recentDrotate.Enqueue(false);
                                
                                if(m_recentDrotate2.Count == 20)
                                    m_recentDrotate2.Dequeue();
                                m_recentDrotate2.Enqueue(false);
                            }
                        }
                        m_drotaAngle = angle;

                        m_drotaOldT0Pos.Set(t0.position.x, t0.position.y);
                        m_drotaOldT1Pos.Set(t1.position.x, t1.position.y);
                        m_drotaCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                        m_drotaReturn = true;
                    }
                    else
                    {
                        m_drotating   = false;
                        m_drotaOldT0Pos.Set(-1f, -1f);
                        m_drotaOldT1Pos.Set(-1f, -1f);
                        m_drotaCursorPos.Set(-1f, -1f);
                        m_recentDrotate.Clear();
                        m_recentDrotate2.Clear();
                        m_lockSlide = false;
                        m_lockRotate = false;
                        m_drotaAngle  = 0f;
                        m_drotaReturn = false;
                    }
                }
                else
                {
                    m_drotating   = false;
                    m_drotaOldT0Pos.Set(-1f, -1f);
                    m_drotaOldT1Pos.Set(-1f, -1f);
                    m_drotaCursorPos.Set(-1f, -1f);
                    m_recentDrotate.Clear();
                    m_recentDrotate2.Clear();
                    m_lockSlide = false;
                    m_lockRotate = false;
                    m_drotaAngle  = 0f;
                    m_drotaReturn = false;
                }
            }

            deltaAngle = m_drotaAngle;
            return m_drotaReturn;
        }
        
    #endregion

    #region zoom_n_scroll
    //-----------------------------------------------------
    // Zoom à 2 doigts
    protected bool Zoom2(out float deltaZoom)
    {
        if(m_zoomTime != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_zoomTime = Time.time;

            if(Input.touchCount == 2)
            {
                Touch t0 = Input.touches[0];
                Touch t1 = Input.touches[1];

                if(!m_zooming /*|| t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began*/)
                {
                    m_zooming = true;
                    m_lastZoomDist = Vector2.Distance(t0.position, t1.position);
                    m_zoomReturn = false;
                    m_zoomDelta = 0f;
                }
                else if(m_zooming && (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved))
                {
                    m_zoomDelta    = (Vector2.Distance(t0.position, t1.position) - m_lastZoomDist)/c_zoomAdaptFactor;
                    m_zoomReturn   = (m_zoomDelta != 0f);
                    m_lastZoomDist = Vector2.Distance(t0.position, t1.position);
                }
                else
                {
                    m_zooming      = false;
                    m_lastZoomDist = 0f;
                    m_zoomDelta    = 0f;
                    m_zoomReturn   = false;
                }
            } // 2 touch
            else
            {
                m_zooming      = false;
                m_lastZoomDist = 0f;
                m_zoomDelta    = 0f;
                m_zoomReturn   = false;
            }
        }

        deltaZoom = m_zoomDelta;
        return m_zoomReturn;
    } // Zoom2()

    //----------------------------------------------------
    // Scroll horizontal ou (exclusif) vertical
    protected bool Scroll1H(out float deltaScroll)
    {
        if(m_scrollHTime != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_scrollHTime = Time.time;

            if(Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if(t.phase == TouchPhase.Moved)
                {
                    if(!m_scrollingH && (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved))
                    {
                        m_scrollingH    = true;
                        m_scrollHoldX   = GetCursorPos().x;
                        m_scrollHDelta  = 0f;
                        m_scrollHReturn = false;
                    }
//                    if(Mathf.Abs(t.deltaPosition.x) > Mathf.Abs(t.deltaPosition.y)) // Pour ne pas détecter de scroll
//                    {                                                               // quand le mouvement est vertical.
                    m_scrollHDelta  = GetCursorPos().x - m_scrollHoldX; //t.deltaPosition.x * (Time.deltaTime / t.deltaTime);
                    DbgUtils.Log("H", "deltay="+m_scrollHDelta+" => ("+GetCursorPos().x+" - "+m_scrollHoldX+")");
                    m_scrollHReturn = (m_scrollHDelta != 0);
                    m_scrollHoldX = GetCursorPos().x;
//                    }
//                    else
//                    {
//                        m_scrollHDelta  = 0f;
//                        m_scrollHReturn = false;
//                    }
                }
                else
                {
                    m_scrollHDelta  = 0f;
                    m_scrollHReturn = false;
                }
            }
            else
            {
                m_scrollHDelta  = 0f;
                m_scrollHReturn = false;
            }
        }

        deltaScroll = m_scrollHDelta;
        return m_scrollHReturn;
    }

    //----------------------------------------------------
    // Scroll horizontal ou (exclusif) vertical
    protected bool Scroll1V(out float deltaScroll)
    {
        if(m_scrollVTime != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_scrollVTime = Time.time;

            if(Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if(!m_scrollingV && (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved))
                {
                    m_scrollingV    = true;
                    m_scrollVoldY   = GetCursorPos().y;
                    m_scrollVDelta  = 0f;
                    m_scrollVReturn = false;
                }
                else if(m_scrollingV && t.phase == TouchPhase.Moved)
                {
//                    if(Mathf.Abs(t.deltaPosition.y) > Mathf.Abs(t.deltaPosition.x)) // Pour ne pas détecter de scroll
//                    {                                                               // quand le mouvement est horizontal
                    m_scrollVDelta  = GetCursorPos().y - m_scrollVoldY; //t.deltaPosition.y;
//                    DbgUtils.Log("V", "deltay="+m_scrollVDelta+" => ("+GetCursorPos().y+" - "+m_scrollVoldY+")");
                    m_scrollVReturn = (m_scrollVDelta != 0);
                    m_scrollVoldY = GetCursorPos().y;
//                    }
//                    else
//                    {
//                        m_scrollingV = false;
//                        m_scrollVoldY.Set(0f, 0f);
//                        m_scrollVDelta  = 0f;
//                        m_scrollVReturn = false;
//                    }
                }
                else
                {
                    m_scrollingV = false;
                    m_scrollVoldY = 0f;
                    m_scrollVDelta  = 0f;
                    m_scrollVReturn = false;
                }
            }
            else
            {
                m_scrollingV = false;
                m_scrollVoldY = 0f;
                m_scrollVDelta  = 0f;
                m_scrollVReturn = false;
            }
        }

        deltaScroll = m_scrollVDelta;
        return m_scrollVReturn;
    }

    //----------------------------------------------------
    // Scroll horizontal et vertical à 1 doigt
    protected bool Scroll1HnV(out float deltaScroll)
    {
        if(m_scrollHVtime != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_scrollHVtime = Time.time;

            if(Input.touchCount == 1)
            {
                Touch t = Input.touches[0];

                if(!m_scrollingHV && (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved))
                {
                    m_scrollingHV    = true;
                    m_scrollHVoldPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_scrollHVdelta  = 0f;
                    m_scrollHVreturn = false;
                }
                else if(m_scrollingHV && t.phase == TouchPhase.Moved)
                {
                    Vector2 trace = t.deltaPosition;

                    if(trace.x*trace.x > trace.y*trace.y)
                        m_scrollHVdir = (trace.x > 0);
                    else if(trace.x*trace.x < trace.y*trace.y)
                        m_scrollHVdir = (trace.y > 0);

                    m_scrollHVdelta  = (m_scrollHVdir? 1 : -1) * (GetCursorPos() - m_scrollHVoldPos).magnitude / c_scrollAdaptFactor;
                    m_scrollHVreturn = (m_scrollHVdelta != 0);
                    m_scrollHVoldPos.Set(GetCursorPos().x, GetCursorPos().y);
                }
                else
                {
                    m_scrollingHV    = false;
                    m_scrollHVoldPos.Set(-1f, -1f);
                    m_scrollHVdelta  = 0f;
                    m_scrollHVreturn = false;
                }
            }
            else
            {
                m_scrollingHV    = false;
                m_scrollHVoldPos.Set(-1f, -1f);
                m_scrollHVdelta  = 0f;
                m_scrollHVreturn = false;
            }
        }

        deltaScroll = m_scrollHVdelta;
        return m_scrollHVreturn;
    }

    #endregion

    #region evenements_multiples
    protected bool Zoom_n_drag(out float deltaZoom, out Vector2 deltaMove, out Vector2 cursorPos)
    {
        if(m_zndTime != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_zndTime = Time.time;

            if(Input.touchCount == 1)       // Move only
            {
                Touch t = Input.touches[0];
                if(!m_znding && (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved))
                {
                    m_znding = true;
                    m_zndOldDist = -1f;
                    m_zndDeltaZoom = 0f;
                    m_zndDeltaMove.Set(0f, 0f);
                    m_zndCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_zndReturn = false;
                }
                else if(m_znding && t.phase == TouchPhase.Moved)
                {
                    m_zndOldDist = -1f;
                    m_zndDeltaZoom = 0f;
                    m_zndDeltaMove.Set(GetCursorPos().x - m_zndCursorPos.x, GetCursorPos().y - m_zndCursorPos.y);
                    m_zndCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_zndReturn = true;
                }
                else
                {
                    m_znding = false;
                    m_zndOldDist = -1f;
                    m_zndDeltaZoom = 0f;
                    m_zndDeltaMove.Set(0f, 0f);
                    m_zndCursorPos.Set(-1f, -1f);
                    m_zndReturn = false;
                }
            }
            else if(Input.touchCount == 2)  // Move & zoom
            {
                Touch t0 = Input.touches[0];
                Touch t1 = Input.touches[1];

                if(!m_znding || (t0.phase == TouchPhase.Began && !EndedOrCanceled(t1)) ||
                                (t1.phase == TouchPhase.Began && !EndedOrCanceled(t0)) )
                {
                    m_znding = true;
                    m_zndOldDist = Vector2.Distance(t0.position, t1.position);
                    m_zndDeltaZoom = 0f;
                    m_zndDeltaMove.Set(0f, 0f);
                    m_zndCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_zndReturn = false;
                }
                else if(m_znding && ((t0.phase == TouchPhase.Moved && !EndedOrCanceled(t1)) ||
                                     (t1.phase == TouchPhase.Moved && !EndedOrCanceled(t0)) ))
                {
                    m_zndDeltaZoom = (Vector2.Distance(t0.position, t1.position) - m_zndOldDist)/c_zoomAdaptFactor;
                    m_zndDeltaMove.Set(GetCursorPos().x - m_zndCursorPos.x, GetCursorPos().y - m_zndCursorPos.y);
                    m_zndCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_zndReturn = true;
                    m_zndOldDist = Vector2.Distance(t0.position, t1.position);
                }
                else
                {
                    m_znding = false;
                    m_zndOldDist = -1f;
                    m_zndDeltaZoom = 0f;
                    m_zndDeltaMove.Set(0f, 0f);
                    m_zndCursorPos.Set(-1f, -1f);
                    m_zndReturn = false;
                }
            }
            else // 0 ou >2 touch
            {
                m_znding = false;
                m_zndOldDist = -1f;
                m_zndDeltaZoom = 0f;
                m_zndDeltaMove.Set(0f, 0f);
                m_zndCursorPos.Set(-1f, -1f);
                m_zndReturn = false;
            }
        }

        deltaZoom = m_zndDeltaZoom;
        deltaMove = m_zndDeltaMove;
        cursorPos = m_zndCursorPos;
        return m_zndReturn;
    } // Zoom_n_drag()

    protected bool Zoom_n_rotate(out float deltaZoom, out float deltaAngle, out Vector2 cursorPos)
    {
        if(m_znrTime != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_znrTime = Time.time;

            if(Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);
                if(!m_znring && (t0.phase == TouchPhase.Began && !EndedOrCanceled(t1)) ||
                                (t1.phase == TouchPhase.Began && !EndedOrCanceled(t0)))
                {
                    m_znring = true;
                    m_znrOldT0Pos.Set(t0.position.x, t0.position.y);
                    m_znrOldT1Pos.Set(t1.position.x, t1.position.y);
                    m_znrDeltaZoom = 0f;
                    m_znrDeltaAngle = 0f;
                    m_znrCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_znrReturn = false;
                }
                else if(m_znring && ((t0.phase == TouchPhase.Moved && !EndedOrCanceled(t1)) ||
                                     (t1.phase == TouchPhase.Moved && !EndedOrCanceled(t0)) ))
                {
                    float curDist  = Vector2.Distance(t0.position, t1.position);
    //                float lastDist = Vector2.Distance(s_zrtLastT0Pos, s_zrtLastT1Pos);
    
                    // -- Rotation --
                    Vector2 oldDir = m_znrOldT1Pos - m_znrOldT0Pos;
                    float angle = Vector2.Angle(oldDir,(t1.position - t0.position));
                    Vector3 cross = Vector3.Cross(oldDir,(t1.position - t0.position));
                    if(cross.z < 0) angle = 360 - angle;

                    // -- Zoom --
                    float startDist = Vector2.Distance(m_znrOldT0Pos, m_znrOldT1Pos);
                    m_znrDeltaZoom = (curDist - startDist)/c_zoomAdaptFactor;

                    m_znrDeltaAngle = angle;
                    m_znrOldT0Pos.Set(t0.position.x, t0.position.y);
                    m_znrOldT1Pos.Set(t1.position.x, t1.position.y);
                    m_znrCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_znrReturn = (m_znrDeltaZoom != 0f || m_znrDeltaAngle != 0f);
                }
                else
                {
                    m_znring = false;
                    m_znrOldT0Pos.Set(-1f, -1f);
                    m_znrOldT1Pos.Set(-1f, -1f);
                    m_znrDeltaZoom = 0f;
                    m_znrDeltaAngle = 0f;
                    m_znrCursorPos.Set(-1f, -1f);
                    m_znrReturn = false;
                }
            }
            else
            {
                m_znring = false;
                m_znrOldT0Pos.Set(-1f, -1f);
                m_znrOldT1Pos.Set(-1f, -1f);
                m_znrDeltaZoom = 0f;
                m_znrDeltaAngle = 0f;
                m_znrCursorPos.Set(-1f, -1f);
                m_znrReturn = false;
            }
        }

        deltaZoom = m_znrDeltaZoom;
        deltaAngle = m_znrDeltaAngle;
        cursorPos = m_znrCursorPos;
        return m_znrReturn;
    } // Zoom_n_rotate()

    protected bool Zoom_n_drag_n_rotate(out float deltaZoom, out Vector2 deltaMove,
                                      out float deltaAngle, out Vector2 CursorPos)
    {
        if(m_zdrTime != Time.time)
        {
//            GetCursorPos();  // MAJ de la cursorPos
            m_zdrTime = Time.time;

            if(Input.touchCount == 1)       // Move only
            {
                Touch t = Input.GetTouch(0);
                if(!m_zdring && (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved))
                {
                    m_zdring = true;
                    m_zdrOldT0Pos.Set(-1f, -1f);
                    m_zdrOldT1Pos.Set(-1f, -1f);
                    m_zdrDeltaZoom = 0f;
                    m_zdrDeltaAngle = 0f;
                    m_zdrDeltaMove.Set(0f, 0f);
                    m_zdrCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_zdrReturn = false;
                }
                else if(m_zdring && t.phase == TouchPhase.Moved)
                {
                    m_zdrOldT0Pos.Set(-1f, -1f);
                    m_zdrOldT1Pos.Set(-1f, -1f);
                    m_zdrDeltaZoom = 0f;
                    m_zdrDeltaAngle = 0f;
                    m_zdrDeltaMove.Set(GetCursorPos().x - m_zdrCursorPos.x, GetCursorPos().y - m_zdrCursorPos.y);
                    m_zdrCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_zdrReturn = true;
                }
                else
                {
                    m_zdring = false;
                    m_zdrOldT0Pos.Set(-1f, -1f);
                    m_zdrOldT1Pos.Set(-1f, -1f);
                    m_zdrDeltaZoom = 0f;
                    m_zdrDeltaAngle = 0f;
                    m_zdrDeltaMove.Set(0f, 0f);
                    m_zdrCursorPos.Set(-1f,-1f);
                    m_zdrReturn = false;
                }
            }
            else if(Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                if(!m_zdring || (t0.phase == TouchPhase.Began && !EndedOrCanceled(t1)) ||
                                (t1.phase == TouchPhase.Began && !EndedOrCanceled(t0)) )
                {
                    m_zdring = true;
                    m_zdrOldT0Pos.Set(t0.position.x, t0.position.y);
                    m_zdrOldT1Pos.Set(t1.position.x, t1.position.y);
                    m_zdrDeltaZoom = 0f;
                    m_zdrDeltaAngle = 0f;
                    m_zdrDeltaMove.Set(0f, 0f);
                    m_zdrCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_zdrReturn = false;
                }
                else if(m_zdring && ((t0.phase == TouchPhase.Moved && !EndedOrCanceled(t1)) ||
                                     (t1.phase == TouchPhase.Moved && !EndedOrCanceled(t0)) ))
                {
                    float lastDist = Vector2.Distance(m_zdrOldT0Pos, m_zdrOldT1Pos);

                    // -- Rotation --
                    Vector2 oldDir = m_zdrOldT1Pos - m_zdrOldT0Pos;
                    float angle = Vector2.Angle(oldDir,(t1.position - t0.position));
                    Vector3 cross = Vector3.Cross(oldDir,(t1.position - t0.position));

                    float reduc = (angle > c_zdrRotatThres ? 0.2f : 1f);

                    if(cross.z < 0)
                        angle = 360 - angle;
                    m_zdrDeltaAngle = angle;

                    // -- Move --
                    float x = (GetCursorPos().x - m_zdrCursorPos.x) * reduc;
                    float y = (GetCursorPos().y - m_zdrCursorPos.y) * reduc;
                    m_zdrDeltaMove.Set(x>0.0f ? Mathf.Floor(x+0.5f) : Mathf.Ceil(x+0.5f),
                                       y>0.0f ? Mathf.Floor(y+0.5f) : Mathf.Ceil(y+0.5f));
                    // -- Zoom --
                    m_zdrDeltaZoom = (Vector2.Distance(t0.position, t1.position) - lastDist)/c_zoomAdaptFactor * reduc;

                    m_zdrOldT0Pos.Set(t0.position.x, t0.position.y);
                    m_zdrOldT1Pos.Set(t1.position.x, t1.position.y);

                    m_zdrCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    m_zdrReturn = (m_zdrDeltaAngle != 0f || m_zdrDeltaZoom != 0f ||
                                   !m_zdrDeltaMove.Equals(Vector2.zero));
                }
                else
                {
                    m_zdring = false;
                    m_zdrOldT0Pos.Set(-1f, -1f);
                    m_zdrOldT1Pos.Set(-1f, -1f);
                    m_zdrDeltaZoom = 0f;
                    m_zdrDeltaAngle = 0f;
                    m_zdrDeltaMove.Set(0f, 0f);
                    m_zdrCursorPos.Set(-1f,-1f);
                    m_zdrReturn = false;
                }
            }
            else
            {
                m_zdring = false;
                m_zdrOldT0Pos.Set(-1f, -1f);
                m_zdrOldT1Pos.Set(-1f, -1f);
                m_zdrDeltaZoom = 0f;
                m_zdrDeltaAngle = 0f;
                m_zdrDeltaMove.Set(0f, 0f);
                m_zdrCursorPos.Set(-1f,-1f);
                m_zdrReturn = false;
            }
        }

        deltaZoom = m_zdrDeltaZoom;
        deltaAngle = m_zdrDeltaAngle;
        deltaMove = m_zdrDeltaMove;
        CursorPos = m_zdrCursorPos;
        return m_zdrReturn;

    } // Zoom_n_drag_n_rotate
        
        
    protected bool Zoom_n_dragV_n_rotate(out float deltaZoom, out float deltaMoveV,
                                       out float deltaAngle)
    {
            if(m_zdVrTime != Time.time)
            {
    //            GetCursorPos();  // MAJ de la cursorPos
                m_zdVrTime = Time.time;
    
                if(Input.touchCount == 2)
                {
                    Touch t0 = Input.GetTouch(0);
                    Touch t1 = Input.GetTouch(1);
    
                    if(!m_zdVring || (t0.phase == TouchPhase.Began && !EndedOrCanceled(t1)) ||
                                     (t1.phase == TouchPhase.Began && !EndedOrCanceled(t0)) )
                    {
                        m_zdVring = true;
                        m_zdVrOldT0Pos.Set(t0.position.x, t0.position.y);
                        m_zdVrOldT1Pos.Set(t1.position.x, t1.position.y);
                        m_zdVrDeltaZoom = 0f;
                        m_zdVrDeltaAngle = 0f;
                        m_zdVrDeltaMoveV = 0.0f;
                        m_zdVrCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                        m_zdVrReturn = false;
                        m_recent_zdVr2.Clear();
                        m_lock_zdVr_DragV = false;
                        m_lock_zdVr_Zoom = false;
                        if(m_zdVrRecentAngles.Count > 0)
                            m_zdVrRecentAngles.Clear();
                    }
                    else if(m_zdVring && (!EndedOrCanceled(t1) && !EndedOrCanceled(t0)))
                    {
                        float lastDist = Vector2.Distance(m_zdVrOldT0Pos, m_zdVrOldT1Pos);
                        Vector2 deltaMoveT0 = t0.position - m_zdVrOldT0Pos;
                        Vector2 deltaMoveT1 = t1.position - m_zdVrOldT1Pos;

                        int recentDragV   = 0;
                        int recentRotates = 0;
                        int recentZoom    = 0;
                        int recentCount   = 20;
                        foreach(int b in m_recent_zdVr2)
                        {
                            switch(b)
    						{
                                case 1 : recentZoom++;    break;
                                case 2 : recentDragV++;   break;
                                case 3 : recentRotates++; break;
                                default: break;
                            }
                        }
                        if(recentZoom+recentRotates > recentCount/2+1)
                        {
                            m_lock_zdVr_Zoom   = true;
                            m_lock_zdVr_DragV  = false;
                        }
                        else if(recentDragV > recentCount/2+1)
                        {
                            m_lock_zdVr_Zoom   = false;
                            m_lock_zdVr_DragV  = true;
                        }

                        // -- Init --
                        float minDragV = Mathf.Min(Mathf.Abs(deltaMoveT0.y), Mathf.Abs(deltaMoveT1.y));      // init DragV

                        Vector2 oldDir = m_zdVrOldT1Pos - m_zdVrOldT0Pos;                                 // init Rota
                        float angle = Vector2.Angle(oldDir,(t1.position - t0.position));
                        Vector3 cross = Vector3.Cross(oldDir,(t1.position - t0.position));
                        if(cross.z < 0)
                            angle = -angle;

                        if(m_zdVrRecentAngles.Count == 12)
                            m_zdVrRecentAngles.Dequeue();
                        m_zdVrRecentAngles.Enqueue(angle);
                        float angleSum = 0f;
                        foreach(float a in m_zdVrRecentAngles)
                            angleSum += Mathf.Abs(a);

                        float angleT0 = Vector2.Angle(t0.position, m_zdVrOldT0Pos);
                        float angleT1 = Vector2.Angle(t1.position, m_zdVrOldT1Pos);
                        //Debug.Log("a0 = "+angleT0.ToString("F2")+" / "+angleT1.ToString("F2")+", "+Mathf.Abs(angleT0-angleT1).ToString("F2"));

                        float curDist = Vector2.Distance(t0.position, t1.position);
                        float deltaDist = (curDist - lastDist); // init Zoom
    
                        m_zdVrDeltaMoveV = 0f;
                        m_zdVrDeltaZoom  = 0f;
                        m_zdVrDeltaAngle = 0f;

                        bool dragVing = (minDragV >= 1f) && TouchesGoSameWaysUpAndDown(deltaMoveT0, deltaMoveT1) && (Mathf.Abs(angle) < 2.5f &&
                                        Mathf.Abs(angleSum) <= 25f) && (Mathf.Abs(deltaDist) <= 2f);
                        bool zooming  = !TouchesGoSameWays(deltaMoveT0, deltaMoveT1) && (Mathf.Abs(deltaDist) > 2f) && (Mathf.Abs(angle) < 0.45f) &&
                                          Mathf.Abs(angleSum) < 17f;
                        bool rotating = (Mathf.Abs(angle) > 0.67f) &&
                                        (TouchesGoSameWays(deltaMoveT0, deltaMoveT1) || (Mathf.Abs(deltaDist) < 5f ||
                                             (!dragVing && Mathf.Abs(angleSum) >= 15f)));

                        // -- ROTATION --
                        if((m_lock_zdVr_Zoom || !dragVing) && !m_lock_zdVr_DragV)
                        {
                            m_zdVrDeltaAngle = angle;

                            m_zdVrDeltaZoom = deltaDist / c_zoomAdaptFactor;

                            // réduction de l'action non prioritaire. Idée pour améliorer ça et éviter le petit retard quand on passe d'une
                            // action à l'autre dans le même geste : au lieu de diminuer l'action de 75% si l'autre action est majoritaire à 80%,
                            // réduire l'action d'un coefficient interpolé (ex : 1-pourcentage de majorité de l'autre si celui-ce est supérieur à 50)
                            if(recentZoom > recentRotates || recentDragV > recentRotates)
                            {
                                if(recentZoom > 0.8f*recentCount)
                                    m_zdVrDeltaAngle = 0f;
                                else
                                    m_zdVrDeltaAngle *= 0.2f;
                            }
                            else if(recentRotates > recentZoom+2 || recentDragV > recentZoom)
                            {
                                if(recentRotates > 0.9f*recentCount)
                                    m_zdVrDeltaZoom = 0f;
                                else
                                    m_zdVrDeltaZoom *= 0.2f;
                            }

                            if(m_recent_zdVr2.Count < recentCount) // pour éviter de trop zoomer/ dézoomer à l'amorce d'un rotate
                                m_zdVrDeltaZoom *= 0.7f+0.3f*((float)m_recent_zdVr2.Count / recentCount);

                            if(rotating)
                            {
                                if(m_recent_zdVr2.Count == recentCount)
                                    m_recent_zdVr2.Dequeue();
                                m_recent_zdVr2.Enqueue(3);
                            }
                            if(zooming)
                            {
                                if(m_recent_zdVr2.Count == recentCount)
                                    m_recent_zdVr2.Dequeue();
                                m_recent_zdVr2.Enqueue(1);
                            }

                            m_zdVrReturn = true;
                        }
                        // -- DRAGV --
                        else
                        {
                            float deltaY = (GetCursorPos().y - m_zdVrCursorPos.y);
                            m_zdVrDeltaMoveV = deltaY/10f;

                            if(m_recent_zdVr2.Count == recentCount)
                                m_recent_zdVr2.Dequeue();
                            m_recent_zdVr2.Enqueue(2);

                            m_zdVrReturn = true;
                        }
                        
                        m_zdVrOldT0Pos.Set(t0.position.x, t0.position.y);
                        m_zdVrOldT1Pos.Set(t1.position.x, t1.position.y);
                        m_zdVrCursorPos.Set(GetCursorPos().x, GetCursorPos().y);
                    }
                    else
                    {
                        m_zdVring = false;
                        m_zdVrOldT1Pos.Set(-1f, -1f);
                        m_zdVrOldT1Pos.Set(-1f, -1f);
                        m_zdVrDeltaZoom = 0f;
                        m_zdVrDeltaAngle = 0f;
                        m_zdVrDeltaMoveV = 0.0f;
                        m_zdVrCursorPos.Set(-1f,-1f);
                        m_zdVrReturn = false;
                        m_recent_zdVr2.Clear();
                        m_lock_zdVr_Zoom = false;
                        m_lock_zdVr_DragV = false;
                        if(m_zdVrRecentAngles.Count > 0)
                            m_zdVrRecentAngles.Clear();
                    }
                }
                else
                {
                    m_zdVring = false;
                    m_zdVrOldT1Pos.Set(-1f, -1f);
                    m_zdVrOldT1Pos.Set(-1f, -1f);
                    m_zdVrDeltaZoom = 0f;
                    m_zdVrDeltaAngle = 0f;
                    m_zdVrDeltaMoveV = 0.0f;
                    m_zdVrCursorPos.Set(-1f,-1f);
                    m_zdVrReturn = false;
                    m_recent_zdVr2.Clear();
                    m_lock_zdVr_Zoom = false;
                    m_lock_zdVr_DragV = false;
                    if(m_zdVrRecentAngles.Count > 0)
                        m_zdVrRecentAngles.Clear();
                }
            }
    
            deltaZoom = m_zdVrDeltaZoom;
            deltaAngle = m_zdVrDeltaAngle;
            deltaMoveV = m_zdVrDeltaMoveV;
       //     CursorPos = m_zdVrCursorPos;
            return m_zdVrReturn;
    
        } // Zoom_n_dragV_n_rotate
    #endregion

    #endregion

    #region touch_Util
    protected bool EndedOrCanceled(Touch t)
    {
        return (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled);
    }

    protected bool TouchesGoSameWays(Vector2 deltaT0, Vector2 deltaT1)
    {
        return (deltaT0.x >= 1f && deltaT1.x >= 1f) || (deltaT0.x <= -1f && deltaT1.x <= -1f);
    }
    protected bool TouchesGoSameWaysUpAndDown(Vector2 deltaT0, Vector2 deltaT1)
    {
        return (deltaT0.y >= 1f && deltaT1.y >= 1f) || (deltaT0.y <= -1f && deltaT1.y <= -1f);
    }

    protected bool OneTouchStationary(Vector2 deltaT0, Vector2 deltaT1)
    {
        return (deltaT0.x < 1f && deltaT0.y < 1f && (Mathf.Abs(deltaT1.x) >= 1f || Mathf.Abs(deltaT1.y) >= 1f)) ||
               (deltaT1.x < 1f && deltaT1.y < 1f && (Mathf.Abs(deltaT0.x) >= 1f || Mathf.Abs(deltaT0.y) >= 1f));
    }

    protected bool TouchesAreClose(Vector2 t0Pos, Vector2 t1Pos)
    {
        return (Vector2.Distance(t0Pos, t1Pos) < Screen.width*0.55f);
    }  
    protected bool TouchesAreVeryClose(Vector2 t0Pos, Vector2 t1Pos, float delta)
    {
        return (Vector2.Distance(t0Pos, t1Pos) < delta);
    }

    #endregion

} // class TouchInput

} // namespace Pointcube.Input
