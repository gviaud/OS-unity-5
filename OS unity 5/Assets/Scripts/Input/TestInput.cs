using UnityEngine;
using System.Collections;

using Pointcube.Utils;
using Pointcube.InputEvents;
using Pointcube.Global;

public class TestInput : MonoBehaviour
{
    public  GameObject  m_cube;
	public  GameObject  m_cubeHeight;
    public  Texture2D   m_texture;

    private CursorInput m_input;
	//private TouchInput m_input;

    private bool   m_click;
    private bool   m_dclick;
    private bool   m_drags;
    private bool   m_rotate;
    private bool   m_drotate;
    private bool   m_zoom;
    private bool   m_scrollH;
    private bool   m_scrollV;
    private bool   m_scrollHV;
    private bool   m_zoomd;
    private bool   m_zoomr;
    private bool   m_zoomrds;
    private bool   m_zoomrdsdragV;

    private float  m_texScale;
    private float  m_texRotate;

    private Rect   m_leftRect;
    private Rect   m_t1Rect;
    private Rect   m_t2Rect;
    private Rect   m_t3Rect;
    private Rect   m_t4Rect;
    private Rect   m_t5Rect;
    private Rect   m_t6Rect;
    private Rect   m_t7Rect;
    private Rect   m_t8Rect;
    private Rect   m_t9Rect;
    private Rect   m_t10Rect;
    private Rect   m_t11Rect;
    private Rect   m_t12Rect;
    private Rect   m_t13Rect;
    private Rect   m_l1Rect;
    private Rect   m_texRect;

    private Rect[] m_uiRects;

    private float dbg1, dbg2, dbg3;
    private const string DBGTAG = "TestInput : ";

    void Awake()
    {
        // OneShotRevolution
        // com.Pointcube.OneShotRevolution
/*#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        m_input = TouchInput.GetInstance();
#else
        m_input = MouseInput.GetInstance();
#endif*/
		
		//test windows 8
		//m_input = WinTouchInput.GetInstance();
		m_input = WinTouchInput.GetInstance();
        m_click     = true;
        m_dclick    = false;
        m_drags     = false;
        m_rotate    = false;
        m_zoom      = false;
        m_scrollH   = false;
        m_scrollHV  = false;
        m_zoomd     = false;
        m_zoomr     = false;
        m_zoomrds   = false;
        m_zoomrdsdragV   = false;

        m_texScale  = 1f;
        m_texRotate = 0f;

        m_leftRect  = new Rect(50f,  20f, 200f, 260f);
        m_t1Rect    = new Rect(50f,   0f, 200f,  20f);
        m_t2Rect    = new Rect(50f,  20f, 200f,  20f);
        m_t3Rect    = new Rect(50f,  40f, 200f,  20f);
        m_t4Rect    = new Rect(50f,  60f, 200f,  20f);
        m_t5Rect    = new Rect(50f,  80f, 200f,  20f);
        m_t6Rect    = new Rect(50f, 100f, 200f,  20f);
        m_t7Rect    = new Rect(50f, 120f, 200f,  20f);
        m_t8Rect    = new Rect(50f, 140f, 200f,  20f);
        m_t9Rect    = new Rect(50f, 160f, 200f,  20f);
        m_t10Rect   = new Rect(50f, 180f, 200f,  20f);
        m_t11Rect   = new Rect(50f, 200f, 200f,  20f);
        m_t12Rect   = new Rect(50f, 220f, 200f,  20f);
        m_t13Rect   = new Rect(50f, 240f, 200f,  20f);
        m_l1Rect    = new Rect(Screen.width - (150f),  20f, 100f, 20f);
        m_texRect   = new Rect((Screen.width-100)/2, (Screen.height-100)/2, 100, 100);

        m_uiRects    = new Rect[2];
        m_uiRects[0] = m_leftRect;
        m_uiRects[1] = m_l1Rect;
    }

	void Update()
    {
		if(PC.In.TouchSupported()){
			WinTouchInput m_input=(WinTouchInput)this.m_input;
			m_input.Update();
		
        if(!m_input.CursorOnUI(m_uiRects))
        {
            if(m_click)
            {
                Vector2 curPos = new Vector2(-1f, -1f);
    
                if(m_input.Click1Down(out curPos))
                {
                   DbgUtils.Log("touchtest", Time.time+" Click 1 down ! "+curPos);
                }
                if(m_input.Click1Hold(out curPos))
                {
                    Debug.Log(DBGTAG+Time.time+" Click 1 hold ! "+curPos);
                   DbgUtils.Log("touchtest", Time.time+" Click 1 hold ! "+curPos);
                }
                if(m_input.Click1Up(out curPos))
                {
                    Debug.Log(DBGTAG+Time.time+" Click 1 up ! "+curPos);
                   DbgUtils.Log("touchtest", Time.time+" Click 1 up ! "+curPos);
                }
                if(m_input.Click2Down(out curPos))
                {
                    Debug.Log(DBGTAG+Time.time+" Click 2 down ! "+curPos);
                   DbgUtils.Log("touchtest", Time.time+" Click 2 down ! "+curPos);
                }
                if(m_input.Click2Hold(out curPos))
                {
                    Debug.Log(DBGTAG+Time.time+" Click 2 hold ! "+curPos);
                   DbgUtils.Log("touchtest", Time.time+" Click 2 hold ! "+curPos);
                }
                if(m_input.Click2Up(out curPos))
                {
                    Debug.Log(DBGTAG+Time.time+" Click 2 up ! "+curPos);
                   DbgUtils.Log("touchtest", Time.time+" Click 2 up ! "+curPos);
                }
            }
    
            if(m_dclick)
            {
                if(m_input.DoubleClickDown())
                {
                    float delay;
                    Vector2 startPos, curPos;
                    m_input.DoubleClickDown(out curPos, out startPos, out delay);
                    Debug.Log(DBGTAG+Time.time+" Double Click down ! ... curPos="+curPos+", startPos="+startPos+", delay="+delay);
                   DbgUtils.Log("touchtest", Time.time+" Double Click down ! ... curPos="+curPos+", startPos="+startPos+", delay="+delay);
                }
                if(m_input.DoubleClickHold())
                {
                    float delay;
                    Vector2 startPos, curPos;
                    m_input.DoubleClickDown(out curPos, out startPos, out delay);
                    Debug.Log(DBGTAG+Time.time+" Double Click hold ! ... curPos="+curPos+", startPos="+startPos+", delay="+delay);
                   DbgUtils.Log("touchtest", Time.time+" Double Click hold ! ... curPos="+curPos+", startPos="+startPos+", delay="+delay);
                }
                if(m_input.DoubleClickUp())
                {
                    float delay;
                    Vector2 startPos, curPos;
                    m_input.DoubleClickDown(out curPos, out startPos, out delay);
                    Debug.Log(DBGTAG+Time.time+" Double Click up ! ... curPos="+curPos+", startPos="+startPos+", delay="+delay);
                   DbgUtils.Log("touchtest", Time.time+" Double Click up ! ... curPos="+curPos+", startPos="+startPos+", delay="+delay);
                }
            }
            if(m_drags)
            {
                Vector2 deltaMove, cursorPos;
                if(m_input.Drag1Start())
                {
                    Debug.Log(DBGTAG+Time.time+" Drag1 Start !");
                   DbgUtils.Log("touchtest", Time.time+" Drag1 Start !");
                }
                if(m_input.Drag1(out deltaMove, out cursorPos))
                {
                    Debug.Log(DBGTAG+Time.time+" Drag1 ! delta="+deltaMove+", pos="+cursorPos+" cubePos="+
                                    m_cube.transform.localPosition);
                   DbgUtils.Log("touchtest", Time.time+" Drag1 ! delta="+deltaMove+", pos="+cursorPos+" cubePos="+
                                    m_cube.transform.localPosition);
                    Vector3 cubePos = m_cube.transform.localPosition;
                    m_cube.transform.localPosition = _3Dutils.ScreenMoveTo3Dmove(deltaMove, cubePos);
						Debug.Log("plap");
                }
                if(m_input.Drag1End())
                {
                    Debug.Log(DBGTAG+Time.time+" Drag1 End !");
                   DbgUtils.Log("touchtest", Time.time+" Drag1 End !");
                }
                if(m_input.Drag2Start())
                {
                    Debug.Log(DBGTAG+Time.time+" Drag2 Start !");
                   DbgUtils.Log("touchtest", Time.time+" Drag2 Start !");
                }
                if(m_input.Drag2(out deltaMove, out cursorPos))
                {
                    Debug.Log(DBGTAG+Time.time+" Drag2 ! delta="+deltaMove+", pos="+cursorPos);
                   DbgUtils.Log("touchtest", Time.time+" Drag2 ! delta="+deltaMove+", pos="+cursorPos);
                    Quaternion cubeRot = m_cube.transform.localRotation;
                    Vector3 cubeRotA = cubeRot.eulerAngles;
                    cubeRotA.y -= deltaMove.x * 360f * 1.25f / Screen.width; // 125% d'1 tour = 100% de l'écran
                    cubeRot.eulerAngles = cubeRotA;
                    m_cube.transform.localRotation = cubeRot;
                }
                if(m_input.Drag2End())
                {
                    Debug.Log(DBGTAG+Time.time+" Drag2 End !");
                   DbgUtils.Log("touchtest", Time.time+" Drag2 End !");
                }
            }
            if(m_rotate)
            {
                float deltaAngle;
                if(m_input.Rotate(out deltaAngle))
                {
                    Debug.Log(DBGTAG+Time.time+" Rotate ! "+deltaAngle);
                   DbgUtils.Log("touchtest", Time.time+" Rotate ! "+deltaAngle);
                }
//                m_texRotate -= deltaAngle; // Rotate guiTexture
                Quaternion cubeRot = m_cube.transform.localRotation;
                Vector3 cubeRotA = cubeRot.eulerAngles;
                cubeRotA.y -= deltaAngle;
                cubeRot.eulerAngles = cubeRotA;
                m_cube.transform.localRotation = cubeRot;
            }
            if(m_drotate)
            {
                float deltaAngle;
                if(m_input.DoubleRotate(out deltaAngle))
                {
                    Debug.Log(DBGTAG+Time.time+" Double Rotate ! "+deltaAngle);
                   DbgUtils.Log("touchtest", Time.time+" Double Rotate ! "+deltaAngle);

                    Quaternion cubeRot = m_cube.transform.localRotation;
                    Vector3 cubeRotA = cubeRot.eulerAngles;
                    cubeRotA.y -= deltaAngle;
                    cubeRot.eulerAngles = cubeRotA;
                    m_cube.transform.localRotation = cubeRot;
                }
            }
            if(m_zoom)
            {
                float deltaZoom;
                if(m_input.Zoom(out deltaZoom))
                {
                    Debug.Log(DBGTAG+Time.time+" Zoom ! "+deltaZoom);
                   DbgUtils.Log("touchtest", Time.time+" Zoom ! "+deltaZoom);
                    m_texScale += (deltaZoom * 12*96 / Screen.width); // Tout l'écran = 1200% de zoom (1/3 de l'écran = 400%)
                }
            }
            if(m_scrollH)
            {
                float deltaScroll;
                if(m_input.ScrollH(out deltaScroll))
                {
                    Debug.Log(DBGTAG+Time.time+" Scroll H ! "+deltaScroll);
                   DbgUtils.Log("touchtest", Time.time+" Scroll H !"+deltaScroll);
                    m_leftRect.x += deltaScroll;
                    m_uiRects[0].x = m_leftRect.x;
                }
            }
            if(m_scrollV)
            {
                float deltaScroll;
                if(m_input.ScrollV(out deltaScroll))
                {
                    Debug.Log(DBGTAG+Time.time+" Scroll V ! "+deltaScroll);
                   DbgUtils.Log("touchtest", Time.time+" Scroll V !"+deltaScroll);
                    m_leftRect.y -= deltaScroll;
                    m_uiRects[0].y = m_leftRect.y;
                }
            }
            if(m_scrollHV)
            {
                float deltaScroll;
                if(m_input.ScrollHV(out deltaScroll))
                {
                    Debug.Log(DBGTAG+Time.time+" Scroll HV ! "+deltaScroll);
                   DbgUtils.Log("touchtest", Time.time+" Scroll HV !"+deltaScroll);
                    m_leftRect.x += deltaScroll;
                    m_uiRects[0].x = m_leftRect.x;
                }
            }
            if(m_zoomd)
            {
                float deltaZoom;
                Vector2 deltaMove, cursorPos;
                if(m_input.Zoom_n_drag1(out deltaZoom, out deltaMove, out cursorPos))
                {
                    if(deltaZoom != 0)
                    {
                        Debug.Log(DBGTAG+Time.time+" Zoom'n'Drag : Zoom ! "+deltaZoom);
                       DbgUtils.Log("touchtest", Time.time+" Zoom'n'Drag : Zoom ! "+deltaZoom);
                        m_texScale += (deltaZoom * 12*96 / Screen.width); // Tout l'écran = 1200% de zoom (1/3 de l'écran = 400%)
                    }
                    if(!deltaMove.Equals(Vector2.zero))
                    {
                        Debug.Log(DBGTAG+Time.time+" Zoom'n'Drag : Drag ! delta="+deltaMove+", pos="+cursorPos);
                       DbgUtils.Log("touchtest", Time.time+" Zoom'n'Drag : Drag ! delta="+deltaMove+", pos="+cursorPos);

                        m_texRect.x += deltaMove.x;
                        m_texRect.y -= deltaMove.y;
                    }
                }
            }
            if(m_zoomr)
            {
                float deltaZoom, deltaAngle;
                Vector2 cursorPos;
                if(m_input.Zoom_n_rotate2D(out deltaZoom, out deltaAngle, out cursorPos))
                {
                    if(deltaZoom != 0)
                    {
                        Debug.Log(DBGTAG+Time.time+" Zoom'n'Rotate2D : Zoom ! "+deltaZoom+", pos="+cursorPos);
                       DbgUtils.Log("touchtest", Time.time+" Zoom'n'Rotate2D : Zoom ! "+deltaZoom+", pos="+cursorPos);
                        m_texScale += (deltaZoom * 12*96 / Screen.width); // Tout l'écran = 1200% de zoom (1/3 de l'écran = 400%)
                    }
                    if(deltaAngle != 0)
                    {
                        Debug.Log(DBGTAG+Time.time+" Zoom'n'Rotate2D : Rotate ! angle="+deltaAngle+", pos="+cursorPos);
                       DbgUtils.Log("touchtest", Time.time+" Zoom'n'Rotate2D : Rotate ! angle="+deltaAngle+", pos="+cursorPos);
                        m_texRotate -= deltaAngle;
                    }
                }
            }
            if(m_zoomrds)
            {
                float deltaZoom, deltaAngle;
                Vector2 deltaMove, cursorPos;
                if(m_input.Zoom_n_drag1_n_rotate2D(out deltaZoom, out deltaMove, out deltaAngle, out cursorPos))
                {
                    if(deltaZoom != 0)
                    {
                        Debug.Log(DBGTAG+Time.time+" Zoom'n'Drag'n'Rotate2D : Zoom ! "+deltaZoom);
                       DbgUtils.Log("touchtest", Time.time+" Zoom'n'Drag'n'Rotate2D : Zoom ! "+deltaZoom);
                        m_texScale += (deltaZoom * 12*96 / Screen.width); // Tout l'écran = 1200% de zoom (1/3 de l'écran = 400%)
                    }
                    if(!deltaMove.Equals(Vector2.zero))
                    {
                        Debug.Log(DBGTAG+Time.time+" Zoom'n'Drag'n'Rotate2D : Drag ! move="+deltaMove+", pos="+cursorPos);
                       DbgUtils.Log("touchtest", Time.time+" Zoom'n'Drag'n'Rotate2D : Drag ! move="+deltaMove+", pos="+cursorPos);

                        m_texRect.x += deltaMove.x;
                        m_texRect.y -= deltaMove.y;
                    }
                    if(deltaAngle != 0)
                    {
                        Debug.Log(DBGTAG+Time.time+" Zoom'n'Drag'n'Rotate2D : Rotate ! angle="+deltaAngle+", pos="+cursorPos);
                       DbgUtils.Log("touchtest", Time.time+" Zoom'n'Drag'n'Rotate2D : Rotate ! angle="+deltaAngle+", pos="+cursorPos);
                        m_texRotate -= deltaAngle;
                    }
                }
            }
            if(m_zoomrdsdragV)
            {
                float deltaZoom, deltaAngle, deltaMoveV;
                if(m_input.Zoom_n_drag2V_n_rotate2D(out deltaZoom, out deltaMoveV, out deltaAngle))
                {
                    if(deltaZoom != 0)
                    {
                  //      Debug.Log(DBGTAG+Time.time+" Zoom'n'Drag'n'Rotate2D : Zoom ! "+deltaZoom);
                   //    DbgUtils.Log("touchtest", Time.time+" Zoom'n'Drag'n'Rotate2D : Zoom ! "+deltaZoom);
                        Vector3 scl = m_cube.transform.localScale;
						scl += new Vector3(deltaZoom*0.45f, deltaZoom*0.45f, deltaZoom*0.45f);
                        scl = new Vector3(Mathf.Clamp(scl.x, 0.1f, 7.5f), Mathf.Clamp(scl.y, 0.1f, 7.5f), Mathf.Clamp(scl.z, 0.1f, 7.5f));
						m_cube.transform.localScale = scl;
					}
                    if(deltaMoveV!= 0)
                    {
                  //      Debug.Log(DBGTAG+Time.time+" Zoom'n'Drag'n'Rotate2D : Drag ! move="+deltaMoveV);
                  //     DbgUtils.Log("touchtest", Time.time+" Zoom'n'Drag'n'Rotate2D : Drag ! move="+deltaMoveV);

                        //m_texRect.x += deltaMove.x;
                        Vector3 pos = m_cube.transform.localPosition;
						pos.y += deltaMoveV/50f;
						m_cube.transform.localPosition = pos;
                    }
                    if(deltaAngle != 0)
                    {
                 //       Debug.Log(DBGTAG+Time.time+" Zoom'n'Drag'n'Rotate2D : Rotate ! angle="+deltaAngle);
                   //    DbgUtils.Log("touchtest", Time.time+" Zoom'n'Drag'n'Rotate2D : Rotate ! angle="+deltaAngle);
						Quaternion cubeRot = m_cube.transform.localRotation;
	                    Vector3 cubeRotA = cubeRot.eulerAngles;
	                    cubeRotA.y -= deltaAngle*1.35f;
	                    cubeRot.eulerAngles = cubeRotA;
	                    m_cube.transform.localRotation = cubeRot;
                    }
                    dbg1 = deltaZoom;
                    dbg2 = deltaMoveV;
                    dbg3 = deltaAngle;
                }
            }
        } // If !cursorOnUI
			
		}
	} // Update

    void OnGUI()
    {
        GUI.Box(m_leftRect, "");
        GUI.BeginGroup(m_leftRect);
        m_click			= GUI.Toggle(m_t1Rect,  m_click,    "Clicks");
        m_dclick		= GUI.Toggle(m_t2Rect,  m_dclick,   "Double clicks");
        m_drags 		= GUI.Toggle(m_t3Rect,  m_drags,    "Drags");
        m_rotate  		= GUI.Toggle(m_t4Rect,  m_rotate,   "Rotate");
        m_drotate 		= GUI.Toggle(m_t5Rect,  m_drotate,  "Double Rotate");
        m_zoom    		= GUI.Toggle(m_t6Rect,  m_zoom,     "Zoom");
        m_scrollH 		= GUI.Toggle(m_t7Rect,  m_scrollH,  "ScrollH");
        m_scrollV 		= GUI.Toggle(m_t8Rect,  m_scrollV,  "ScrollV");
        m_scrollHV 		= GUI.Toggle(m_t9Rect,  m_scrollHV, "Scroll HV");
        m_zoomd  		= GUI.Toggle(m_t10Rect, m_zoomd,    "Zoom'n'drag");
        m_zoomr    		= GUI.Toggle(m_t11Rect, m_zoomr,    "Zoom'n'rotate2D");
        m_zoomrds		= GUI.Toggle(m_t12Rect, m_zoomrds,  "Zoom'n'drag'n'rotate2D");
        m_zoomrdsdragV 	= GUI.Toggle(m_t13Rect, m_zoomrdsdragV,  "Zoom'n'drag2V'n'rotate2D");
        GUI.EndGroup();
        GUI.Label(m_l1Rect, m_input.GetCursorPos().ToString());
        //GUI.Box(new Rect(50f, Screen.height-150f, 400f, 100f), "z="+dbg1.ToString("F2")+"\nd="+dbg2.ToString("F2")+"\na="+dbg3);

        Matrix4x4 bk = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(m_texScale, m_texScale), m_texRect.center);
        GUIUtility.RotateAroundPivot(m_texRotate, m_texRect.center);
        GUI.DrawTexture(m_texRect, m_texture);
        GUI.matrix = bk;
		//m_input.drawTouches();
		//GUI.Label(new Rect(300,300,400,20),"touch count : "+WinTouchInput.GetTouchPointCount());
    }

    void OnDisable()
    {
        UsefullEvents.OnResizingWindow  -= FitRectToScreen;
        UsefullEvents.OnResizeWindowEnd -= FitRectToScreen;
    }

    private void FitRectToScreen()
    {
        m_l1Rect.x = Screen.width - (m_l1Rect.width + 50f);
        m_uiRects[1].x = m_l1Rect.x;
    }
}
