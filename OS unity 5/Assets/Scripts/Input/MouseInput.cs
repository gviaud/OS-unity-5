using UnityEngine;

namespace Pointcube.InputEvents
{

public class MouseInput : CursorInput
{
    #region attributs_&_constructeur
    // Singleton (obligé car une classe statique ne peut pas implémenter une interface)
    private static  MouseInput s_instance = null;

    // -- Double clic --
    private float   m_dClickTime;                      // Temps du dernier appel à la fonction double clic (pour retourner
    private Vector2 m_dClick1stClickPos;               // toujours la même chose pendant la même frame + éviter de re-calculer)
    private Vector2 m_dClickCurPos;
    private float   m_dClickDelay;
    private int     m_dClickReturn;

    private bool    m_doubleClicking;                  // 2e clic d'un double clic en cours
    private bool    m_simpleClicked;                   // clic simple récent ou pas (potentiel double clic)
    private float   m_lastClickTime;                   // temps (s) depuis le dernier clic simple récent (0 si aucun)
    private float   m_clicked_x, m_clicked_y;          // position du curseur au clic simple récent (-1 si aucun)

    private const float c_doubleClickDelay = 0.5f;     //0.20f; // temps entre les deux clics (s)
    private const float c_doubleClickThresold = 50.0f; //30.0f; // tolérance de position entre 1er et 2e clic (en pixels)

    // -- Drag --
    private float   m_drag1Time;
    private Vector2 m_drag1DeltaMove;
    private Vector2 m_drag1CursorPos;
    private int     m_drag1Return;

    private bool    m_drag1;
    private Vector2 m_drag1LastPos;

    private float   m_drag2Time;
    private Vector2 m_drag2DeltaMove;
    private Vector2 m_drag2CursorPos;
    private int     m_drag2Return;

    private bool    m_drag2;
    private Vector2 m_drag2LastPos;
	
	private float   m_dragtwo1Time;
	private Vector2 m_dragtwo1DeltaMove;
	private Vector2 m_dragtwo1CursorPos;
	private int     m_dragtwo1Return;
	
	private bool    m_dragtwo1;
	private Vector2 m_dragtwo1LastPos;
	
    private float   m_drag3Time;
    private Vector2 m_drag3DeltaMove;
    private Vector2 m_drag3CursorPos;
    private int     m_drag3Return;

    private bool    m_drag3;
    private Vector2 m_drag3LastPos;


    // -- --
    private static Vector2 m_trashVect;                // Vecteur poubelle pour supprimer les arguments out
    private static float   m_trashFloat;               // float idem

    private const string DBGTAG = "MouseInput : ";

    //-----------------------------------------------------
    private MouseInput()
    {
        // -- Double clic --
        m_dClickTime        = 0f;
        m_dClick1stClickPos = new Vector2(-1f, -1f);
        m_dClickCurPos      = new Vector2(-1f, -1f);
        m_dClickDelay       = -1f;
        m_dClickReturn      = 0;

        m_doubleClicking   = false;
        m_simpleClicked    = false;
        m_lastClickTime    = 0f;
        m_clicked_x        = -1f;
        m_clicked_y        = -1f;

        // -- Drag --
        m_drag1Time        = 0f;
        m_drag1DeltaMove   = new Vector2(0f, 0f);
        m_drag1CursorPos   = new Vector2(-1f, -1f);
        m_drag1Return      = 0;
        m_drag1            = false;
        m_drag1LastPos     = new Vector2(-1f, -1f);

        m_drag2Time        = 0f;
        m_drag2DeltaMove   = new Vector2(0f, 0f);
        m_drag2CursorPos   = new Vector2(-1f, -1f);
        m_drag2Return      = 0;
        m_drag2            = false;
        m_drag2LastPos     = new Vector2(-1f, -1f);
			
        m_drag3Time        = 0f;
        m_drag3DeltaMove   = new Vector2(0f, 0f);
        m_drag3CursorPos   = new Vector2(-1f, -1f);
        m_drag3Return      = 0;
        m_drag3            = false;
        m_drag3LastPos     = new Vector2(-1f, -1f);

        // -- variables poubelles --
        m_trashVect  = new Vector2(-1f, -1f);
        m_trashFloat = -1f;
    }

    //-----------------------------------------------------
    public static MouseInput GetInstance()
    {
        if(s_instance == null)
            s_instance = new MouseInput();

        return s_instance;
    }
    #endregion attributs_&_constructeur
		
	override public bool TouchSupported(){return false;}
	override public  PcTouch touches(int index){return null;}
	override public  int touchCount{get{return 0;}}
				
    #region implementation_events
    //-- Clics simples --
    override public bool Click1Down()                      { return (HandleMouseClick(out m_trashVect, 0) == 1); }
    override public bool Click1Down(out Vector2 cursorPos) { return (HandleMouseClick(out cursorPos  , 0) == 1); }
    override public bool Click1Hold()                      { return (HandleMouseClick(out m_trashVect, 0) == 2); } // Note : il n'y a jamais Hold et Down à la même frame, contrairement au truc Unity de base
    override public bool Click1Hold(out Vector2 cursorPos) { return (HandleMouseClick(out cursorPos  , 0) == 2); }
    override public bool Click1Up()                        { return (HandleMouseClick(out m_trashVect, 0) == 3); }
    override public bool Click1Up(out Vector2 cursorPos)   { return (HandleMouseClick(out cursorPos  , 0) == 3); }

    override public bool Click2Down()                      { return (HandleMouseClick(out m_trashVect, 1) == 1); }
    override public bool Click2Down(out Vector2 cursorPos) { return (HandleMouseClick(out cursorPos  , 1) == 1); }
    override public bool Click2Hold()                      { return (HandleMouseClick(out m_trashVect, 1) == 2); }
    override public bool Click2Hold(out Vector2 cursorPos) { return (HandleMouseClick(out cursorPos  , 1) == 2); }
    override public bool Click2Up()                        { return (HandleMouseClick(out m_trashVect, 1) == 3); }
    override public bool Click2Up(out Vector2 cursorPos)   { return (HandleMouseClick(out cursorPos  , 1) == 3); }
    override public bool Click3Up()                        { return (HandleMouseClick(out m_trashVect, 2) == 3); }

    // Retourne true si clic gauche up et clic droit non actif, ou clic droit terminé et clic gauche non actif.
    override public bool ClickEnded()
    {
        int c1 = HandleMouseClick(out m_trashVect, 0);
        int c2 = HandleMouseClick(out m_trashVect, 1);
        return (c1 == 3 && c2 != 1 && c2 != 2) || (c2 == 3 && c1 != 1 && c1 != 2);
    }
	
    // -- Doubles clics --
    override public bool DoubleClickDown()
        { return (HandleDoubleClick(out m_trashVect, out m_trashVect, out m_trashFloat) == 1); }

    override public bool DoubleClickDown(out Vector2 currentPos, out Vector2 firstClickPos, out float clickDelay)
        { return (HandleDoubleClick(out currentPos, out firstClickPos, out clickDelay) == 1);  }

    override public bool DoubleClickHold()
        { return (HandleDoubleClick(out m_trashVect, out m_trashVect, out m_trashFloat) == 2); }

    override public bool DoubleClickHold(out Vector2 currentPos, out Vector2 firstClickPos, out float clickDelay)
        { return (HandleDoubleClick(out currentPos, out firstClickPos, out clickDelay) == 2);  }

    override public bool DoubleClickUp()
        { return (HandleDoubleClick(out m_trashVect, out m_trashVect, out m_trashFloat) == 3); }

    override public bool DoubleClickUp(out Vector2 currentPos, out Vector2 firstClickPos, out float clickDelay)
        { return (HandleDoubleClick(out currentPos, out firstClickPos, out clickDelay) == 3);  }

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
    override public bool Rotate()                     { return HandleDrag2(out m_trashVect, out m_trashVect) == 2; }
    override public bool Rotate(out float deltaAngle)
    {
        Vector2 deltaMove;
        bool drag = HandleDrag2(out deltaMove, out m_trashVect) == 2;
        if(drag)
            deltaAngle = deltaMove.x * 360f * 1.1f / Screen.width;
        else
            deltaAngle = 0f;
        return drag;
    }

    override public bool DoubleRotate()
        { return HandleDrag2(out m_trashVect, out m_trashVect) == 2; }

    override public bool DoubleRotate(out float deltaAngle)
    {
        Vector2 deltaMove;
        bool drag = HandleDrag2(out deltaMove, out m_trashVect) == 2;
        if(drag)
            deltaAngle = deltaMove.x * 360f * 1.1f / Screen.width;
        else
            deltaAngle = 0f;
        return drag;
    }

    // -- Zoom & scroll --
    override public bool Zoom()                          { return ScrollWheel(out m_trashFloat); }
    override public bool Zoom(out float deltaZoom)       { return ScrollWheel(out deltaZoom);    }
    override public bool ScrollH()                       { return ScrollWheel(out m_trashFloat); }
    override public bool ScrollH(out float deltaScroll)  { return ScrollWheel(out deltaScroll);  }
    override public bool ScrollV()                       { return ScrollWheel(out m_trashFloat); }
    override public bool ScrollV(out float deltaScroll)  { return ScrollWheel(out deltaScroll);  }
    override public bool ScrollHV()                      { return ScrollWheel(out m_trashFloat); }
    override public bool ScrollHV(out float deltaScroll) { return ScrollWheel(out deltaScroll);  }

    override public bool ScrollViewH()                      { return false; }
    override public bool ScrollViewH(out float deltaScroll) { deltaScroll = m_trashFloat; return false; }
    override public bool ScrollViewV()                      { return false; }
    override public bool ScrollViewV(out float deltaScroll) { deltaScroll = m_trashFloat; return false; }

    //-----------------------------------------------------
    // Événements multiples // TODO unités
    override public bool Zoom_n_drag1(out float deltaZoom, out Vector2 deltaMove)
        { return Zoom_n_drag1(out deltaZoom, out deltaMove, out m_trashVect); }

    override public bool Zoom_n_drag1(out float deltaZoom, out Vector2 deltaMove, out Vector2 cursorPos)
    {
        bool scrollw = ScrollWheel(out deltaZoom);
        bool drag    = HandleDrag1(out deltaMove, out cursorPos) == 2;
        return (scrollw || drag);
    }

    override public bool Zoom_n_rotate2D(out float deltaZoom, out float deltaAngle)
        { return Zoom_n_rotate2D(out deltaZoom, out deltaAngle, out m_trashVect); }

    override public bool Zoom_n_rotate2D(out float deltaZoom, out float deltaAngle, out Vector2 cursorPos)
    {
        Vector2 deltaMove;

        bool scrollw = ScrollWheel(out deltaZoom);
        bool drag    = HandleDrag2(out deltaMove, out cursorPos) == 2;

        deltaAngle = deltaMove.x;
        return (scrollw || drag);
    }

    override public bool Zoom_n_drag1_n_rotate2D(out float deltaZoom, out Vector2 deltaMove, out float angle)
        { return Zoom_n_drag1_n_rotate2D(out deltaZoom, out deltaMove, out angle, out m_trashVect); }

    override public bool Zoom_n_drag1_n_rotate2D(out float deltaZoom, out Vector2 deltaMove,
                                                 out float angle,     out Vector2 cursorPos)
    {
        Vector2 deltaMoveD, deltaMoveR, cursorPosD, cursorPosR;

        bool scrollw = ScrollWheel(out deltaZoom);
        bool drag    = HandleDrag1(out deltaMoveD, out cursorPosD) == 2;
        bool rotate  = HandleDrag2(out deltaMoveR, out cursorPosR) == 2;

        deltaMove = deltaMoveD;
        angle     = deltaMoveR.x;

        if(drag)        cursorPos = cursorPosD;
        else if(rotate) cursorPos = cursorPosR;
        else            cursorPos = m_trashVect;

        return (scrollw || drag || rotate);
    }   
		
	override public bool Zoom_n_drag2V_n_rotate2D(out float deltaZoom, out float deltaMove,
                                                 out float angle)
    {
        Vector2 deltaMoveD, deltaMoveR, cursorPosD, cursorPosR;

        bool scrollw = ScrollWheel(out deltaZoom);
        bool rotate  = HandleDrag2(out deltaMoveR, out cursorPosR) == 2;
        bool drag    = HandleDragTwo1(out deltaMoveD, out cursorPosD) == 2;

        deltaMove = deltaMoveD.y;
        angle     = deltaMoveR.x;

        return (scrollw || drag || rotate);
    }
    
	/*override public bool Zoom_n_drag2V_n_rotate2D(out float deltaZoom, out float deltaMove,
	                                              out float angle)
	{
		Vector2 deltaMoveD, deltaMoveR, cursorPosD, cursorPosR;
		
		bool scrollw = ScrollWheel(out deltaZoom);
		bool rotate  = HandleDrag2(out deltaMoveR, out cursorPosR) == 2;
		bool drag    = HandleDragTwo1(out deltaMoveD, out cursorPosD) == 2;
		
		deltaMove = deltaMoveD.y;
		angle     = deltaMoveR.x;
		
		Debug.Log(angle);
		Debug.Log(deltaMove);
		//      if(drag)        cursorPos = cursorPosD;
		//     else if(rotate) cursorPos = cursorPosR;
		//      else            cursorPos = m_trashVect;
		
		return (scrollw || drag || rotate);
	}*/
	
    //-----------------------------------------------------
    override public Vector2 GetCursorPos()
    {
        return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }

    //-----------------------------------------------------
    override public Vector2 GetCursorPosInvY()
    {
        return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }

    //-----------------------------------------------------
    override public bool ClickOnUI(Rect[] uiRects)
    {
        return ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && CursorOnUI(uiRects));
    }

    #endregion

    #region fonctions_specifiques

    #region clics_simples_&_doubles
    //-----------------------------------------------------
    private int HandleMouseClick(out Vector2 cursorPos, int buttonID)
    {
        if(Input.GetMouseButtonDown(buttonID))
        {
            cursorPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            return 1;
        }
        else if(Input.GetMouseButton(buttonID))
        {
            cursorPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            return 2;
        }
        else if(Input.GetMouseButtonUp(buttonID))
        {
            cursorPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            return 3;
        }
        else
        {
            cursorPos = m_trashVect;
            return 0;
        }
    }

	private bool HandleClickRightAndLeft()
	{
		return Input.GetMouseButtonDown(0) && Input.GetMouseButtonDown(1);
	}
    //-----------------------------------------------------
    private int HandleDoubleClick(out Vector2 currentPos, out Vector2 firstClickPos, out float delay)
    {
        if(Time.time != m_dClickTime)            // Premier appel de la fonction à cette frame
        {
            m_dClickTime = Time.time;

            if(m_simpleClicked && Time.time-m_lastClickTime > c_doubleClickDelay) // TODO virer les Time.time-m_lastClickTime car inutiles maintenant (cf. 1ere ligne de la fonction)?
            {
                m_simpleClicked = false;
                m_lastClickTime = 0;
            }

            if(Input.GetMouseButtonDown(0))
            {
                float x = Input.mousePosition.x;
                float y = Input.mousePosition.y;

                if(m_simpleClicked && m_lastClickTime != Time.time && Time.time-m_lastClickTime <= c_doubleClickDelay &&
                   x > (m_clicked_x - c_doubleClickThresold) && x < (m_clicked_x + c_doubleClickThresold) &&
                   y > (m_clicked_y - c_doubleClickThresold) && y < (m_clicked_y + c_doubleClickThresold))
                {
                    m_dClickCurPos.Set(x, y);
                    m_dClick1stClickPos.Set(m_clicked_x, m_clicked_y);
                    m_dClickDelay       = Time.time - m_lastClickTime;
                    m_dClickReturn      = 1;   // DoubleClickDown

                    m_simpleClicked  = false;
                    m_lastClickTime  = 0;
                    m_doubleClicking = true;
                }
                else    // simple clic (potentiel 1er clic d'un futur double clic)
                {
                    m_simpleClicked = true;
                    m_lastClickTime = Time.time;
                    m_clicked_x     = x;
                    m_clicked_y     = y;

                    m_dClickCurPos.Set(m_trashVect.x, m_trashVect.y);
                    m_dClick1stClickPos.Set(m_trashVect.x, m_trashVect.y);
                    m_dClickDelay   = m_trashFloat;
                    m_dClickReturn  = 0;  // Pas d'événement
                }
            } // mouseButtonDown
            else if(m_doubleClicking && Input.GetMouseButton(0) && m_lastClickTime != Time.time)
            {
                m_dClickCurPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_dClick1stClickPos.Set(m_clicked_x, m_clicked_y);
                m_dClickDelay       = Time.time - m_lastClickTime;
                m_dClickReturn      = 2;    // DoubleClickHold
            }
            else if(m_doubleClicking && Input.GetMouseButtonUp(0))
            {
                m_dClickCurPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_dClick1stClickPos.Set(m_clicked_x, m_clicked_y);
                m_dClickDelay       = Time.time - m_lastClickTime;
                m_dClickReturn      = 3;    // DoubleClickUp

                // -- Réinitialisation --
                m_doubleClicking = false;
                m_simpleClicked  = false;
                m_lastClickTime  = 0f;
                m_clicked_x      = -1f;
                m_clicked_y      = -1f;
            }
            else
            {
                m_dClickCurPos.Set(m_trashVect.x, m_trashVect.y);
                m_dClick1stClickPos.Set(m_trashVect.x, m_trashVect.y);
                m_dClickDelay    = m_trashFloat;
                m_dClickReturn   = 0;  // Pas d'événement
            }
        }

        currentPos = new Vector2(m_dClickCurPos.x, m_dClickCurPos.y);
        firstClickPos = new Vector2(m_dClick1stClickPos.x, m_dClick1stClickPos.y);
        delay = m_dClickDelay;

        return m_dClickReturn;
    } // DoubleClick()
    #endregion

    #region drag
    //-----------------------------------------------------
    // -- Note : un drag clic gauche est stoppé par un clic droit
    private int HandleDrag1(out Vector2 deltaMove, out Vector2 cursorPos)
    {
        if(m_drag1Time != Time.time)
        {
            m_drag1Time = Time.time;

            if(!m_drag1 && Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                m_drag1DeltaMove.Set(0f, 0f);
                m_drag1CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_drag1Return = 1;

                m_drag1       = true;
                m_drag1LastPos.Set(Input.mousePosition.x, Input.mousePosition.y);
            }
            else if(m_drag1 && Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                m_drag1DeltaMove.Set(Input.mousePosition.x - m_drag1LastPos.x,
                                     Input.mousePosition.y - m_drag1LastPos.y);
                m_drag1CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_drag1Return = /*(m_drag1DeltaMove.x != 0f || m_drag1DeltaMove.y != 0f)?*/ 2;

                m_drag1LastPos.Set(Input.mousePosition.x, Input.mousePosition.y);
            }
            else
            {
                m_drag1DeltaMove.Set(0f, 0f);
                m_drag1CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_drag1Return = (m_drag1 ? 3 : 0);

                m_drag1 = false;
                m_drag1LastPos.Set(-1f, -1f);
            }
        }

        deltaMove = m_drag1DeltaMove;
        cursorPos = m_drag1CursorPos;
        return m_drag1Return;
    } 
		
	private int HandleDrag3(out Vector2 deltaMove, out Vector2 cursorPos)
    {
        if(m_drag3Time != Time.time)
        {
            m_drag3Time = Time.time;

            if(!m_drag3 && Input.GetMouseButton(2) )
            {
                m_drag3DeltaMove.Set(0f, 0f);
                m_drag3CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_drag3Return = 1;

                m_drag3       = true;
                m_drag3LastPos.Set(Input.mousePosition.x, Input.mousePosition.y);
            }
            else if(m_drag3 && Input.GetMouseButton(2))
            {
                m_drag3DeltaMove.Set(Input.mousePosition.x - m_drag3LastPos.x,
                                     Input.mousePosition.y - m_drag3LastPos.y);
                m_drag3CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_drag3Return = /*(m_drag1DeltaMove.x != 0f || m_drag1DeltaMove.y != 0f)?*/ 2;

                m_drag3LastPos.Set(Input.mousePosition.x, Input.mousePosition.y);
            }
            else
            {
                m_drag3DeltaMove.Set(0f, 0f);
                m_drag3CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_drag3Return = (m_drag3 ? 3 : 0);

                m_drag3 = false;
                m_drag3LastPos.Set(-1f, -1f);
            }
        }

        deltaMove = m_drag3DeltaMove;
        cursorPos = m_drag3CursorPos;
        return m_drag3Return;
    }

    //-----------------------------------------------------
    // -- Note : un drag clic droit est stoppé par un clic gauche
    private int HandleDrag2(out Vector2 deltaMove, out Vector2 cursorPos)
    {
        if(m_drag2Time != Time.time)
        {
            m_drag2Time = Time.time;

            if(!m_drag2 && Input.GetMouseButton(1) && !Input.GetMouseButton(0))
            {
                m_drag2DeltaMove.Set(0f, 0f);
                m_drag2CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_drag2Return = 1;

                m_drag2       = true;
                m_drag2LastPos.Set(Input.mousePosition.x, Input.mousePosition.y);
            }
            else if(m_drag2 && Input.GetMouseButton(1) && !Input.GetMouseButton(0))
            {
                m_drag2DeltaMove.Set(Input.mousePosition.x - m_drag2LastPos.x,
                                     Input.mousePosition.y - m_drag2LastPos.y);
                m_drag2CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_drag2Return = /*(m_drag2DeltaMove.x != 0f || m_drag2DeltaMove.y != 0f) ?*/ 2;

                m_drag2LastPos.Set(Input.mousePosition.x, Input.mousePosition.y);
            }
            else
            {
                m_drag2DeltaMove.Set(0f, 0f);
                m_drag2CursorPos.Set(Input.mousePosition.x, Input.mousePosition.y);
                m_drag2Return = (m_drag2 ? 3 : 0);

                m_drag2 = false;
                m_drag2LastPos.Set(-1f, -1f);
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

    private bool ScrollWheel(out float deltaScroll)
    {
        deltaScroll = Input.GetAxis("Mouse ScrollWheel");
        return (deltaScroll != 0);
    }

    #endregion
} // static class MouseInput

} // namespace Pointcube.Input
