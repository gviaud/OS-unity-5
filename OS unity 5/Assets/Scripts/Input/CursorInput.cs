using UnityEngine;

namespace Pointcube.InputEvents
{

public abstract class CursorInput
{
    //-----------------------------------------------------
    // Événements uniques

	public  abstract bool TouchSupported();
	public abstract PcTouch touches(int index);
	public abstract int touchCount{get;}
		
    // -- Clics/taps simples --
	public abstract bool Click1Down();                          // Clic gauche / tap 1 doigt
    public abstract bool Click1Down(out Vector2 cursorPos);     // override avec position
    public abstract bool Click1Hold();                          // Clic maintenu (note : si le curseur bouge, cf. drag)
    public abstract bool Click1Hold(out Vector2 cursorPos);
    public abstract bool Click1Up();
    public abstract bool Click1Up(out Vector2 cursorPos);

    public abstract bool Click2Down();                          // Clic droit / tap 2 doigts
    public abstract bool Click2Down(out Vector2 cursorPos);     // override avec position
    public abstract bool Click2Hold();
    public abstract bool Click2Hold(out Vector2 cursorPos);
    public abstract bool Click2Up();
    public abstract bool Click2Up(out Vector2 cursorPos);
    public abstract bool Click3Up();

    public abstract bool ClickEnded();

    // -- Doubles clics/taps --
    public abstract bool DoubleClickDown();                    // Double clic gauche / tap 1 doigt, 2e clic down
    public abstract bool DoubleClickDown(out Vector2 currentPos, out Vector2 firstClick, out float clickDelay); // TODO unités du délai
    public abstract bool DoubleClickHold();
    public abstract bool DoubleClickHold(out Vector2 currentPos, out Vector2 firstClick, out float clickDelay);
    public abstract bool DoubleClickUp();
    public abstract bool DoubleClickUp(out Vector2 currentPos, out Vector2 firstClick, out float clickDelay);
//    bool DoubleClick2Down();                    // Double clic droit / tap 2 doigts, 2e clic down
//    bool DoubleClick2Down(out Vector2 currentPos, out Vector2 click1Pos, out float clicDelay); // TODO unités du délai
//    bool DoubleClick2Hold();
//    bool DoubleClick2Hold(out Vector2 currentPos, out Vector2 click1Pos, out float clickDelay);
//    bool DoubleClick2Up();
//    bool DoubleClick2Up(out Vector2 currentPos, out Vector2 click1Pos, out float clicDelay);

    // -- Drags --
    public abstract bool Drag1Start();
    public abstract bool Drag1Start(out Vector2 deltaMove);
    public abstract bool Drag1Start(out Vector2 deltaMove, out Vector2 cursorPos); // Click gauche'n'drag / slide 1 doigt
    public abstract bool Drag1();
    public abstract bool Drag1(out Vector2 deltaMove);
    public abstract bool Drag1(out Vector2 deltaMove, out Vector2 cursorPos); // Click gauche'n'drag / slide 1 doigt
    public abstract bool Drag1End();
    public abstract bool Drag1End(out Vector2 deltaMove);
    public abstract bool Drag1End(out Vector2 deltaMove, out Vector2 cursorPos); // Click gauche'n'drag / slide 1 doigt

    public abstract bool Drag2Start();
    public abstract bool Drag2Start(out Vector2 deltaMove);
    public abstract bool Drag2Start(out Vector2 deltaMove, out Vector2 cursorPos); // Click droit'n'drag  / slide 2 doigts
    public abstract bool Drag2();
    public abstract bool Drag2(out Vector2 deltaMove);
    public abstract bool Drag2(out Vector2 deltaMove, out Vector2 cursorPos); // Click droit'n'drag  / slide 2 doigts
    public abstract bool DragTwo1();
    public abstract bool DragTwo1(out Vector2 deltaMove);
    public abstract bool DragTwo1(out Vector2 deltaMove, out Vector2 cursorPos); // Click droit & gauche 'n'drag  / slide 2 doigts
    public abstract bool Drag2End();
    public abstract bool Drag2End(out Vector2 deltaMove);
    public abstract bool Drag2End(out Vector2 deltaMove, out Vector2 cursorPos); // Click droit'n'drag  / slide 2 doigts

    // -- Rotate --
    public abstract bool Rotate();
    public abstract bool Rotate(out float deltaAngle);
	public abstract bool DoubleRotate();
	public abstract bool DoubleRotate(out float deltaAngle);

    // -- Zoom & scroll --
    public abstract bool Zoom();
    public abstract bool Zoom(out float deltaZoom);                           // Molette / zoom à 2 doigts

    public abstract bool ScrollH();
    public abstract bool ScrollH(out float deltaScroll);                      // Molette / slide +-
    public abstract bool ScrollV();
    public abstract bool ScrollV(out float deltaScroll);                      // Molette / slide +-
    public abstract bool ScrollHV();
    public abstract bool ScrollHV(out float deltaScroll);                     // Molette / slide +-

    public abstract bool ScrollViewH();                                       // Scroll touch, rien molette
    public abstract bool ScrollViewH(out float deltaScroll);                  //  (c'est automatique avec les scrollview)
    public abstract bool ScrollViewV();
    public abstract bool ScrollViewV(out float deltaScroll);
        
    //-----------------------------------------------------
    // Événements multiples
    public abstract bool Zoom_n_drag1(out float deltaZoom, out Vector2 deltaMove);
    public abstract bool Zoom_n_drag1(out float deltaZoom, out Vector2 deltaMove, out Vector2 cursorPos);

    public abstract bool Zoom_n_rotate2D(out float deltaZoom, out float deltaAngle); // Tip pour une rotation cool : (deltaAngle/UnityEngine.Screen.width)*360f;
    public abstract bool Zoom_n_rotate2D(out float deltaZoom, out float deltaAngle, out Vector2 cursorPos);

    public abstract bool Zoom_n_drag1_n_rotate2D(out float deltaZoom, out Vector2 deltaMove, out float angle);
    public abstract bool Zoom_n_drag1_n_rotate2D(out float deltaZoom, out Vector2 deltaMove, out float angle, out Vector2 cursorPos);
    public abstract bool Zoom_n_drag2V_n_rotate2D(out float deltaZoom, out float deltaMoveV, out float angle);

    //-----------------------------------------------------
    // Misc
    public abstract Vector2 GetCursorPos();                	                 // Position du curseur (moyenne des positions si plusieurs touches)
    public abstract Vector2 GetCursorPosInvY();                              // idem Inversée sur Y 

    //-----------------------------------------------------
    // note : stocker les rects de l'UI courante dans un UI manager
        
    public bool CursorOnUIs(params Rect[] uiRect)
    {
        return CursorOnUI(uiRect);
    }
        
        
    public bool CursorOnUI(Rect uiRect)
    {
        Rect[] r = new Rect[1];
        r[0] = uiRect;
        return CursorOnUI(r);
    }

    public bool CursorOnUI(Rect[] uiRects)
    {
        Vector2 mousePos = Input.mousePosition; // TODO tester sur tablette pour voir si la mousePosition marche avec le touch ?
        mousePos.y = Screen.height-mousePos.y;

        foreach(Rect rect in uiRects)
        {
            if(rect.Contains(mousePos))
                return true;
        }
        return false;
    }

    public abstract bool ClickOnUI(Rect[] uiRects);

    //-----------------------------------------------------
    // Prendre en compte l'accélération d'un mouvement, pour
    // une valeur non linéairement dépendante de la position d'un drag (par exemple)
    public float Accel(float val, float cap)
    {
        val *= (val<0? -1 : 1) * val;

        if(val > cap)       val = cap;
        else if(val < -cap) val = -cap;

        return val;
    }

} // interface CursorInput

} // namespace Pointcube.Input
