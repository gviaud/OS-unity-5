using UnityEngine;
using System.Collections;

using Pointcube.Utils;
using Pointcube.Global;

public class PolygonDrawer : MonoBehaviour
{			
	protected static float POINT_RADIUS = 50;
	protected static float DIMENSION_OFFSET = 50; // offset autour du rectangle de dimension
	protected static float COTATION_OFFSET = 25; // offset pour le rectangle permettant le dessin des cotes
	protected static float COTATION_HEIGHT = 70;
	
	protected Vector2 _cursor;
	
	// offset entre le milieu du point et _cursor lors de la selection et deplacement d'un point
	protected Vector2 _pointOffset;
	
	protected Polygon _polygon = new Polygon ();
	
	protected Snapper _snapper;
	
	protected PlanTransformation _planTransformation;
	
	protected Point2 _selectedPoint = null;
	protected int _selectedPointIndex = -1;
	
	protected Edge2 _selectedEdge = null;
	protected int _selectedEdgeIndex = -1;
	
	protected int _selectedPointIndexCandidate = -1;
	protected int _selectedEdgeIndexCandidate = -1;

    protected Touch _firstTouch;

	protected bool _canMove = false;
	
//	protected PoolDesignerUI _gui;
	protected PoolUIv2 _gui;
	
	protected bool _maxDimensionVisible = true;
	protected bool _cotationVisible = true;
	
	protected float	_background_width = 1024;
	protected float	_background_height = 768;
	protected float _offsetWidth = 0;
	protected float _offsetHeight = 0;
	
	protected Rect _rectBackground = new Rect(0, 0, 200, 200);
	protected Rect _rectBackgroundInit = new Rect(0, 0, 200, 200);
	
	public GUISkin skin;
	
	public Texture2D backgroundImage;
	public Texture2D backgroundImageGrid;
	private bool _gridVisible = true;
	public Texture2D labelBackgroundImage;

    protected Touch [] _touches;
	
	private string m_newWidth = "";
	private string m_newHeight = "";
	public bool m_isModifyingW = false;
	public bool m_isModifyingH = false;
	private Rect m_cotationW,m_cotationH;
	private static int  m_steps = 0;
	private const  int	c_maxSteps = 50;
	private const string c_uiCoteHeightName = "uiHeight";
	private const string c_uiCoteWidthName  = "uiWidth";
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_EDITOR
	private TouchScreenKeyboard m_kb;
#endif

	void Awake()
	{
        UsefullEvents.OnResizeWindowEnd       += ResizeRectsToScreen;
	}
	    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizeWindowEnd       -= ResizeRectsToScreen;
    }
	
	
	
	// Use this for initialization
	protected void Start ()
	{
		EdgeDrawer.SOLID_EDGE_TEXTURE = Resources.Load ("PoolDesigner/edge") as Texture2D;
		_snapper = GetComponent<Snapper> ();
		_planTransformation = GetComponent<PlanTransformation> ();
//		_gui = GameObject.Find("MainScene").GetComponent<PoolDesignerUI> ();
		_gui = GameObject.Find("MainScene").GetComponent<PoolUIv2>();
		ResizeRectsToScreen();
		
		m_cotationW = m_cotationH = new Rect(0,0,0,0);
	}
	
	// Update is called once per frame
	protected void Update () 
	{
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        _touches = Input.touches;
#endif
		if (_gui.GetCurrentMode () == PoolDesignerMode.PolygonEdition)
		{
			_cursor = _planTransformation.GetTransformedMousePosition ();
			
			// on cherche le point et l'edge qui sont sous le curseur
			FindIntersectedPoint ();
			FindIntersectedEdge ();

//#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//            if(Input.touchCount == 1)
//            {
//                _firstTouch = _planTransformation.GetTouches()[0];
//                Touch testouch = _touches[0];
//
//            if (!_gui.isOnUI () && _firstTouch.phase == TouchPhase.Began)
//            {
//                Debug.Log("---------> firstTouch "+_firstTouch.deltaTime+", "+_firstTouch.position+", "+_firstTouch.deltaPosition+", "+_firstTouch.phase);
//                Debug.Log("---------> testTouch  "+testouch.deltaTime+", "+testouch.position+", "+testouch.deltaPosition+", "+testouch.phase);
//#else
      //if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR
            if (!_gui.isOnUI () && _planTransformation.GetClickBegan()/*PC.In.Click1Down()*/ && !_gui.IsOpen())
            {
//#endif
				if(_gui.IsJustCloseMenu())
				{
					_gui.setIsJustCloseMenu(false);
					return;
				}
    			if (_selectedPointIndexCandidate >= 0) // selection
    			{
    				_selectedPointIndex = _selectedPointIndexCandidate;
    				_selectedPoint = _polygon.GetPoints ()[_selectedPointIndex];
    
    				if (!_polygon.IsClosed () && _selectedPointIndex == 0 /*&& _polygon.GetPoints ().Count > 2*/)
    				{
    					_polygon.Close ();
    					_selectedPointIndex = _polygon.GetPoints ().IndexOf (_selectedPoint);
    				}
    
    				_canMove = true;
    			}
    			else if (_selectedEdgeIndexCandidate >= 0) // insert point
    			{
    				_selectedEdgeIndex = _selectedEdgeIndexCandidate;
    				_selectedEdge = _polygon.GetEdges ()[_selectedEdgeIndex];
    
    				Point2 newPoint = new Point2 (_cursor);
    				newPoint.Set(newPoint.GetX()-_offsetWidth, newPoint.GetY()-_offsetHeight);
    				_polygon.InsertPoint (_selectedEdgeIndex, newPoint);
    
    				_selectedPointIndex = _polygon.GetPoints ().IndexOf (newPoint);
    				_selectedPoint = newPoint;
    
    				_canMove = true;
    			}
    
    			else if (!_polygon.IsClosed ()) // add point
    			{
    				Point2 newPoint = new Point2 (_cursor);
    				newPoint.Set(newPoint.GetX()-_offsetWidth, newPoint.GetY()-_offsetHeight);
    				_polygon.AddPoint (newPoint);
    
    				_selectedPointIndex = _polygon.GetPoints ().Count - 1;
    				_selectedPoint = newPoint;
    
    				_canMove = true;
    
    			}
    			else // deselection
    			{
    				_selectedPointIndex = -1;
    				_selectedPoint = null;
    
    				_selectedEdgeIndex = -1;
    				_selectedEdge = null;
    			}
    
    			if (_selectedPointIndex >= 0) // update radius slider values
    			{
    				_pointOffset = (Vector2)_selectedPoint - _cursor;
    
    				if (_selectedPoint.GetJunction () == JunctionType.Broken)
    				{
    					_gui.SetSliderValues (0, 1);
    				}
    				else if (_selectedPoint.GetJunction () == JunctionType.Curved)
    				{
    					ArchedPoint2 ar = _selectedPoint as ArchedPoint2;
    					_gui.SetSliderValues (ar.GetMeasuredRadius () / 100, ar.GetMaxRadius () / 100);
    				}
    			}
            }
//#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//            }
//#endif
			if (PC.In.Click1Up()) //Input.GetMouseButtonUp (0))
			{
				_canMove = false;
				
				if (!_polygon.IsClosed () && 
					_polygon.GetPoints ().Count > 2 && 
					_selectedPointIndex == _polygon.GetPoints ().Count - 1) // close polygon
				{
					Vector2 firstPoint = _polygon.GetPoints ()[0];
					float rectSize = 100;
					Rect closeRect = new Rect (
						firstPoint.x - rectSize/2, 
						firstPoint.y - rectSize/2, 
						rectSize, rectSize);
					
					if (closeRect.Contains (_cursor))
					{				
						_polygon.CloseAndFusion ();
						
						_selectedPointIndex = -1;
						_selectedPoint = null;
						
						_selectedEdgeIndex = -1;
						_selectedEdge = null;
					}	
				}
			}
			
			if(PC.In.Click1Hold() && _selectedPointIndex >= 0 && _canMove) // move selected point
			{
				_snapper.SetExclusionPoint (_selectedPoint);
				Vector2 pointPosition = _cursor + _pointOffset;
				
				if (_snapper.IsAngleSnapActivate () && _snapper.IsAngleSnapped ())
				{
					pointPosition = _snapper.GetAngleSnapPosition ();
				}
				
				if (_snapper.IsAlignedPointSnapActivate ())
				{
					if (_snapper.IsHSnapped ())
					{
						pointPosition.y = _snapper.GetHSnapY () /*+ _offsetHeight*/;
					}
					
					if (_snapper.IsVSnapped ())
					{
						pointPosition.x = _snapper.GetVSnapX () /*+ _offsetWidth*/;
					}
				}
				
				if (_snapper.IsPointSnapActivate () && _snapper.IsIntersectedSnapped ())
				{
					pointPosition = _snapper.GetIntersectedSnapPosition ();
			//		pointPosition.Set(pointPosition.x + _offsetWidth, pointPosition.y + _offsetHeight);
				}
				
				_polygon.MovePoint (_selectedPointIndex, pointPosition);
			}
//#endif
		}
		if(m_isModifyingH || m_isModifyingW)
		{
			if (PC.In.Click1Up() && !PC.In.CursorOnUIs(m_cotationH,m_cotationW))
			{
				
				m_isModifyingW = m_isModifyingH = false;
			}
		}
	}
	
	protected void OnGUI ()
	{		
		// Get transformation matrix
		Matrix4x4 matrix = _planTransformation.GetMatrix ();
		
		GUI.depth = -10;
		
		// draw background image
		GUI.DrawTexture (_rectBackground, backgroundImage, ScaleMode.StretchToFill, false);
//		GUI.DrawTexture (_rectBackground, backgroundImage, ScaleMode.StretchToFill, false);
		if(_gridVisible)
		{
			ImgUtils.TileTextureWithOffset(backgroundImageGrid,_rectBackgroundInit,
				_rectBackground,
				ScaleMode.ScaleToFit, 0/*_offsetWidth*/,0/* _offsetHeight*/);
		}
		GUISkin bkup = GUI.skin;
		GUI.skin = skin;
		
		// draw current scale --> ex: 1 x 1
		float scale = Mathf.Round ((1 / _planTransformation.GetScale ().x) * 100) / 100 ;
	//	GUI.DrawTexture (new Rect (850+ _offsetWidth-20, 550+ _offsetHeight-20, 140, 140), labelBackgroundImage, ScaleMode.StretchToFill, true);
	//	GUI.Label (new Rect (850+ _offsetWidth, 590 + _offsetHeight, 100, 20),/* scale.ToString () + " x " + */scale.ToString () + "m", "dimension label Main");
		
		Function_PoolDesigner function = _gui.GetFunction ();
		
		// Dessin de l'image de fond chargé par l'utilisateur
		if (function.GetBackgroundImage () != null && 
			function.IsBackgroundImageVisible () && 
			_gui.GetCurrentMode () != PoolDesignerMode.BackgroundScale)
		{
			Rect bgRect = function.GetBackgroundRect ();
			
			// on applique les transformation du plan a l'image de fond
			Vector2 bgPt = new Vector2 (bgRect.x + _offsetWidth, bgRect.y + _offsetHeight);
			bgPt = matrix.MultiplyPoint (bgPt);
			bgRect.x = bgPt.x;
			bgRect.y = bgPt.y;
			
			bgRect.width = bgRect.width * _planTransformation.GetScale ().x;
			bgRect.height = bgRect.height * _planTransformation.GetScale ().x;
			
			GUI.DrawTexture (bgRect, function.GetBackgroundImage (), ScaleMode.ScaleToFit, false);
		}
		
		if (_maxDimensionVisible && _polygon.GetPoints ().Count > 0 && !(_cotationVisible && _canMove && PC.In.Click1Hold()))
		{
			// draw dimension square with the boundingBox
			Bounds polyBounds = _polygon.bounds;
			Vector2 dimensionPosition = new Vector2 (
				polyBounds.center.x - polyBounds.extents.x + _offsetWidth,
				polyBounds.center.z - polyBounds.extents.z + _offsetHeight);
			
			dimensionPosition = matrix.MultiplyPoint (dimensionPosition);
			
			Vector2 dimensionSize = new Vector2 (polyBounds.size.x, 
				                                 polyBounds.size.z);
			
			float poolWidth = Mathf.Round (polyBounds.size.x) / 100;
			float poolHeight = Mathf.Round (polyBounds.size.z) / 100;
			
			if(m_newWidth != poolWidth.ToString() && !m_isModifyingW)
				m_newWidth = poolWidth.ToString();
			if(m_newHeight != poolHeight.ToString() && !m_isModifyingH)
				m_newHeight = poolHeight.ToString();
			
			dimensionSize = matrix.MultiplyVector (dimensionSize);
			
			Rect dimensionRect = new Rect (dimensionPosition.x - DIMENSION_OFFSET, 
				                           dimensionPosition.y - DIMENSION_OFFSET ,
				                           dimensionSize.x + DIMENSION_OFFSET + DIMENSION_OFFSET / 2, 
				                           dimensionSize.y + DIMENSION_OFFSET + DIMENSION_OFFSET / 2);
			
			GUI.Label (dimensionRect, "", "dimensionSquare"); //Flèches
			
			m_cotationW.Set((dimensionRect.x + dimensionRect.width / 2) - 50,dimensionRect.y - 10,100,30);
			m_cotationH.Set(dimensionRect.x - 80,(dimensionRect.y + dimensionRect.height / 2) - 10,100,30);
			
//			if(m_cotationH.x < 100f) // plus besoin car plus de boutons gauche/droite
//			{
//				if(m_cotationH.yMax < Screen.height/2f && m_cotationH.yMax > Screen.height/2f - 25f)
//					m_cotationH.y =  Screen.height/2f - 25f - 50f;
//				else if(m_cotationH.yMax > Screen.height/2f && m_cotationH.yMax < Screen.height/2f + 25f)
//					m_cotationH.y =  Screen.height/2f + 25f + 20f;
//				else if(m_cotationH.y > Screen.height/2f && m_cotationH.y < Screen.height/2f + 25f)
//					m_cotationH.y =  Screen.height/2f + 25f + 20f;
//				else if(m_cotationH.y < Screen.height/2f && m_cotationH.y > Screen.height/2f -25f)
//					m_cotationH.y =  Screen.height/2f - 25f - 50f;
//			}
			//MODIFICATION DES COTES HEIGHT-----------------------------------------------------------------
			if(!m_isModifyingH)
			{
				GUI.BeginGroup(m_cotationH);
				if(GUI.Button(new Rect(0,0,30,30),"","changeSize") || 
					UsefulFunctions.GUIOutlineButton/* GUI.Button*/(new Rect(30,0,70,30),poolHeight.ToString () + "m","txtout","txtin"))
				{
					m_isModifyingH = true;
					m_isModifyingW = false;
#if UNITY_ANDROID || UNITY_IPHONE
					m_kb = TouchScreenKeyboard.Open(poolHeight.ToString(),TouchScreenKeyboardType.NumbersAndPunctuation);		
#endif
				}
				GUI.EndGroup();
			}
			else
			{
				
				GUI.BeginGroup(m_cotationH);
#if!(UNITY_ANDROID || UNITY_IPHONE)
				if(GUI.Button(new Rect(0,0,30,30),"","applySize"))
				{
					if(ChangeSize())
						m_isModifyingH = false;
					else
						m_newHeight = "";
				}
				GUI.SetNextControlName(c_uiCoteHeightName);
//				m_newHeight = GUI.TextField(new Rect(30,0,70,30),m_newHeight);
				m_newHeight = UsefulFunctions.GUIOutlineTextField(new Rect(30,0,70,30),m_newHeight,"txtout","txtin");
#else
				m_newHeight = m_kb.text;
//				GUI.Label(new Rect(30,0,70,30),m_newHeight,"dimension label");
				UsefulFunctions.GUIOutlineLabel(new Rect(30,0,70,30),m_newHeight,"txtout","txtin");
				if(m_kb.done)
				{
					if(m_kb.wasCanceled)
					{
						m_isModifyingH = false;
						m_newHeight = "";
					}
					else
					{
						if(ChangeSize())
							m_isModifyingH = false;
						else
						{
							m_newHeight = "";
							m_isModifyingH = false;
						}
					}
				}
#endif
				GUI.EndGroup();
				if(GUI.GetNameOfFocusedControl() != c_uiCoteHeightName)
					GUI.FocusControl(c_uiCoteHeightName);
			}
			//MODIFICATION DES COTES Width----------------------------------------------------------------
			if(!m_isModifyingW)
			{
				GUI.BeginGroup(m_cotationW);
				if(GUI.Button(new Rect(0,0,30,30),"","changeSize") || 
					UsefulFunctions.GUIOutlineButton/* GUI.Button*/(new Rect(30,0,70,30),poolWidth.ToString () + "m", "txtout","txtin"))
				{
					m_isModifyingW = true;
					m_isModifyingH = false;
#if UNITY_ANDROID || UNITY_IPHONE
					m_kb = TouchScreenKeyboard.Open(poolWidth.ToString(),TouchScreenKeyboardType.NumbersAndPunctuation);			
#endif
				}
				GUI.EndGroup();
			}
			else
			{
				GUI.BeginGroup(m_cotationW);
#if!(UNITY_ANDROID || UNITY_IPHONE)
				if(GUI.Button(new Rect(0,0,30,30),"","applySize"))
				{
					if(ChangeSize())
						m_isModifyingW = false;
					else
						m_newWidth = "";
				}
				GUI.SetNextControlName(c_uiCoteWidthName);
//				m_newWidth = GUI.TextField(new Rect(30,0,70,30),m_newWidth);
				m_newWidth = UsefulFunctions.GUIOutlineTextField(new Rect(30,0,70,30),m_newWidth,"txtout","txtin");
#else
				m_newWidth = m_kb.text;
//				GUI.Label(new Rect(30,0,70,30),m_newWidth,"dimension label");
				UsefulFunctions.GUIOutlineLabel(new Rect(30,0,70,30),m_newWidth,"txtout","txtin");
				if(m_kb.done)
				{
					if(m_kb.wasCanceled)
					{
						m_isModifyingW = false;
						m_newWidth = "";
					}
					else
					{
						if(ChangeSize())
							m_isModifyingW = false;
						else
						{
							m_newWidth = "";
							m_isModifyingW = false;
						}
					}
				}
#endif
				GUI.EndGroup();
				if(GUI.GetNameOfFocusedControl() != c_uiCoteWidthName)
					GUI.FocusControl(c_uiCoteWidthName);
			}
			//GUI.Label (new Rect (dimensionRect.x + dimensionRect.width + 10, (dimensionRect.y + dimensionRect.height / 2) + 10, 40, 20), "", "dimension label");
			//GUI.Label (new Rect ((dimensionRect.x + dimensionRect.width / 2) - 20, dimensionRect.y + dimensionRect.height + 10, 40, 20), "", "dimension label");
		}
		
		// draw edges
		int edgeCounter = 0;
		foreach (Edge2 edge in _polygon.GetEdges ())
		{
			if (edge.GetPrevPoint2 ().GetJunction () != JunctionType.Curved &&
				edge.GetNextPoint2 ().GetJunction () != JunctionType.Curved)
			{
				EdgeDrawer.Draw (edge, matrix, 10, _offsetWidth, _offsetHeight); // draw simple edges
			}
			else
			{
				Point2 prevPoint = edge.GetPrevPoint2 ();
				Point2 prevEdgePoint = new Point2 (prevPoint);
				if (prevPoint.GetJunction () == JunctionType.Curved)
				{
					ArchedPoint2 aPoint = prevPoint as ArchedPoint2;
					prevEdgePoint.Set (aPoint.GetNextTangentPoint ());
					
					Edge2 curvedEdge = new Edge2 (prevPoint, prevEdgePoint);
					
					Color bckColor = GUI.color;
					GUI.color = new Color (0.9f, 0.9f, 0.9f, 0.8f);
					EdgeDrawer.Draw (curvedEdge, matrix, 6, _offsetWidth, _offsetHeight); // draw edges before the curve
					GUI.color = bckColor;
				}
				
				Point2 nextPoint = edge.GetNextPoint2 ();
				Point2 nextEdgePoint = new Point2 (nextPoint);
				if (nextPoint.GetJunction () == JunctionType.Curved)
				{
					ArchedPoint2 aPoint = nextPoint as ArchedPoint2;
					nextEdgePoint.Set (aPoint.GetPrevTangentPoint ());
					
					Edge2 curvedEdge = new Edge2 (nextEdgePoint, nextPoint);
					
					Color bckColor = GUI.color;
					GUI.color = new Color (0.9f, 0.9f, 0.9f, 0.8f);
					EdgeDrawer.Draw (curvedEdge, matrix, 6, _offsetWidth, _offsetHeight); // draw edges after the curve
					GUI.color = bckColor;
				}
				
				// draw edges between the curve
				Edge2 fillEdge = new Edge2 (prevEdgePoint, nextEdgePoint);
				EdgeDrawer.Draw (fillEdge, matrix, 10, _offsetWidth, _offsetHeight);
			}
			
			++edgeCounter;
		}
		
		// draw point according its current state
		int pointCounter = 0;
		foreach (Point2 point in _polygon.GetPoints ())
		{
			Vector2 position = point;
			position.Set(position.x+ _offsetWidth, position.y+ _offsetHeight);
			position = matrix.MultiplyPoint (position);
			
			if (pointCounter != _selectedPointIndex)
			{
				GUI.Box (new Rect (position.x - POINT_RADIUS / 2  , 
					               position.y - POINT_RADIUS / 2  , 
					               POINT_RADIUS, 
					               POINT_RADIUS), "", "point");
			}
			else
			{
				if (_cotationVisible && _canMove && PC.In.Click1Hold()/*Input.GetMouseButton (0)*/)
				{
					// dessin des cote de chaque segment
					// ici segement precedent le sommet courant (sens anti-horaire)
					Edge2 prevEdge = point.GetPrevEdge ();
					if (prevEdge != null)
					{
						// on retrouve les deux sommets correspondant a chaque segment
						// en prenant en compte les sommets de type arc de cercle
						Point2 prevPt2 = prevEdge.GetPrevPoint2 ();
						
						
						Vector2 prevPt = prevPt2;
						prevPt.x = prevPt.x+_offsetWidth;
						prevPt.y = prevPt.y+_offsetHeight;
						
						if (prevPt2.GetJunction () == JunctionType.Curved)
						{
							ArchedPoint2 ap = prevPt2 as ArchedPoint2;
							prevPt = ap.GetNextTangentPoint ();
							prevPt.x = prevPt.x+_offsetWidth;
							prevPt.y = prevPt.y+_offsetHeight;
						}					
						
						Vector2 nextPt = point;
						nextPt.x = nextPt.x+_offsetWidth;
						nextPt.y = nextPt.y+_offsetHeight;
						
						if (point.GetJunction () == JunctionType.Curved)
						{
							ArchedPoint2 ap = point as ArchedPoint2;
							nextPt = ap.GetPrevTangentPoint ();
							nextPt.x = nextPt.x+_offsetWidth;
							nextPt.y = nextPt.y+_offsetHeight;
						}
						
						// taille reel du segment
						float edgelabel = Mathf.Round (Vector2.Distance (nextPt, prevPt)) / 100;
						
						// on applique la matrice de transformation aux deux sommets
						prevPt = matrix.MultiplyPoint (prevPt);
						nextPt = matrix.MultiplyPoint (nextPt);
						
						// on cherche l'inclinaison a appliqué pour tracer la fleche et le label de dimension
						float angle = Vector2.Angle (nextPt - prevPt, Vector2.right);
						
						if (prevPt.y > nextPt.y) 
						{ 
							angle = -angle; 
						}
						
						Matrix4x4 m = GUI.matrix;
						
			        	GUIUtility.RotateAroundPivot (angle, prevPt);
						
						// taille du segment apres transformation matricielle
						float edgeLength = Vector2.Distance (nextPt, prevPt);
						
						Rect edgeRect = new Rect (prevPt.x - COTATION_OFFSET, 
							                      prevPt.y, 
							                      edgeLength + COTATION_OFFSET + COTATION_OFFSET, 
							                      COTATION_HEIGHT);
						string style = "cotation";
						float minusLabelY = 0;
						
						if (point.GetAngleType () == AngleType.Inside)
						{
							edgeRect = new Rect (prevPt.x - COTATION_OFFSET, 
							                     prevPt.y - COTATION_HEIGHT, 
							                     edgeLength + COTATION_OFFSET + COTATION_OFFSET, 
							                     COTATION_HEIGHT);
							style = "reverse cotation";
							minusLabelY = POINT_RADIUS + POINT_RADIUS;
						}
						
						GUI.Box (edgeRect, "", style);
						
						if (angle > -90 && angle <= 90) // ici le label est dans le bon sens
						{
							UsefulFunctions.GUIOutlineLabel/* GUI.Label */(new Rect ((prevPt.x + edgeLength / 2) - 20, 
							                  	 prevPt.y + POINT_RADIUS - minusLabelY,
							                     40, 
							                     20), edgelabel.ToString () + "m", "txtout","txtin");
						}
						else
						{
							// le label est a l'envers, on applique une rotation
							// par rapport au sommet suivant le sommet courant (sens anti-horaire)
							// et d'angle 180 - angle mesuré
							GUI.matrix = m;
							GUIUtility.RotateAroundPivot (angle - 180, nextPt);
							UsefulFunctions.GUIOutlineLabel/* GUI.Label */(new Rect ((nextPt.x + edgeLength / 2) - 20, 
							                  	 nextPt.y - POINT_RADIUS - 20 + minusLabelY,
							                     40, 
							                     20), edgelabel.ToString () + "m", "txtout","txtin");
						}
						
						GUI.matrix = m;
					}
					
					// meme chose pour le segment suivant
					Edge2 nextEdge = point.GetNextEdge ();
					if (nextEdge != null)
					{
						Vector2 prevPt = point;
						prevPt.x = prevPt.x+_offsetWidth;
						prevPt.y = prevPt.y+_offsetHeight;
						
						if (point.GetJunction () == JunctionType.Curved)
						{
							ArchedPoint2 ap = point as ArchedPoint2;
							prevPt = ap.GetNextTangentPoint ();
							prevPt.x = prevPt.x+_offsetWidth;
							prevPt.y = prevPt.y+_offsetHeight;
						}
						
						Point2 nextPt2 = nextEdge.GetNextPoint2 ();
						Vector2 nextPt = nextPt2;
						nextPt.x = nextPt.x+_offsetWidth;
						nextPt.y = nextPt.y+_offsetHeight;
						
						
						if (nextPt2.GetJunction () == JunctionType.Curved)
						{
							ArchedPoint2 ap = nextPt2 as ArchedPoint2;
							nextPt = ap.GetPrevTangentPoint ();
							nextPt.x = nextPt.x+_offsetWidth;
							nextPt.y = nextPt.y+_offsetHeight;
						}
						
						float edgelabel = Mathf.Round (Vector2.Distance (nextPt, prevPt)) / 100;
						
						prevPt = matrix.MultiplyPoint (prevPt);
						nextPt = matrix.MultiplyPoint (nextPt);
						
						float angle = Vector2.Angle (nextPt - prevPt, Vector2.right);
						
						if (prevPt.y > nextPt.y) 
						{ 
							angle = -angle; 
						}
						
						Matrix4x4 m = GUI.matrix;
						
			        	GUIUtility.RotateAroundPivot (angle, prevPt);
						
						float edgeLength = Vector2.Distance (nextPt, prevPt);
						
						Rect edgeRect = new Rect (prevPt.x - COTATION_OFFSET, 
							                      prevPt.y, 
							                      edgeLength + COTATION_OFFSET + COTATION_OFFSET, 
							                      COTATION_HEIGHT);
						string style = "cotation";
						float minusLabelY = 0;
						
						if (point.GetAngleType () == AngleType.Inside)
						{
							edgeRect = new Rect (prevPt.x - COTATION_OFFSET,
							                     prevPt.y - COTATION_HEIGHT, 
							                     edgeLength + COTATION_OFFSET + COTATION_OFFSET, 
							                     COTATION_HEIGHT);
							style = "reverse cotation";
							minusLabelY = POINT_RADIUS + POINT_RADIUS;
						}
						
						GUI.Box (edgeRect, "", style);
						
						if (angle > -90 && angle <= 90)
						{
							UsefulFunctions.GUIOutlineLabel/* GUI.Label */(new Rect ((prevPt.x + edgeLength / 2) - 20, 
							                  	 prevPt.y + POINT_RADIUS - minusLabelY,
							                     40, 
							                     20), edgelabel.ToString () + "m", "txtout","txtin");
						}
						else
						{
							GUI.matrix = m;
							GUIUtility.RotateAroundPivot (angle - 180, nextPt);
							UsefulFunctions.GUIOutlineLabel/* GUI.Label */(new Rect ((nextPt.x + edgeLength / 2) - 20, 
							                  	 nextPt.y - POINT_RADIUS - 20 + minusLabelY,
							                     40, 
							                     20), edgelabel.ToString () + "m", "txtout","txtin");
						}
						
						GUI.matrix = m;
					}
				}
				
				// dessin du sommet courant
				if (_canMove && PC.In.Click1Hold() /*Input.GetMouseButton (0)*/)
				{
					GUI.Box (new Rect (position.x - POINT_RADIUS *2 / 2, 
						               position.y - POINT_RADIUS *2 / 2, 
						               POINT_RADIUS * 2, 
						               POINT_RADIUS * 2), "", "selected point up");
				}
				else
				{
					GUI.Box (new Rect (position.x - POINT_RADIUS / 2, 
						               position.y - POINT_RADIUS / 2, 
						               POINT_RADIUS, 
						               POINT_RADIUS), "", "selected point");
				}
			}
			
			// si arc de cercle dessin de l'arc de cercle
			if (point.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 ap = point as ArchedPoint2;
				ArcDrawer.Draw (ap, matrix,_offsetWidth, _offsetHeight);
			}
			
			++pointCounter;
		}
		
		GUI.skin = bkup;
		
		/*********************** DEBUG DRAW RECT FOR EDGE AND POINT SELECTION AND TRANSFORMED CURSOR POSITION ***********************/
		/*float s = 1 / _planTransformation.GetScale ().x;
		float pr = POINT_RADIUS * s;
		
		foreach (Point2 point in _polygon.GetPoints ())
		{
			Vector2 p = point;
			Rect pointRect = new Rect (p.x - pr / 2, 
			                           p.y - pr / 2, 
				                       pr, 
				                       pr);
			
			GUI.Box (pointRect, "");
		}
		
		GUI.Button (new Rect (_cursor.x - 4, _cursor.y - 4, 8, 8), "");
		
		foreach (Edge2 edge in _polygon.GetEdges ())
		{
			Vector2 prevPt = edge.GetPrevPoint2 ();
			Vector2 nextPt = edge.GetNextPoint2 ();
			
			float angle = Vector2.Angle (nextPt - prevPt, Vector2.right);
        
			if (prevPt.y > nextPt.y) 
			{ 
				angle = -angle; 
			}
			
			Matrix4x4 m = GUI.matrix;
			
        	GUIUtility.RotateAroundPivot (angle, prevPt);
			Rect edgeRect = new Rect (prevPt.x + (pr / 2), 
				                      prevPt.y - (pr / 2), 
				                      edge.GetLength () - pr, 
				                      pr);
			GUI.Box (edgeRect, "");
			
			GUI.matrix = m;
		}*/
		/********************************************************/
	}

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR
	
#endif
		
	protected void FindIntersectedEdge ()
	{
		_selectedEdgeIndexCandidate = -1;
		int edgeCounter = 0;
		
		float scale = 1 / _planTransformation.GetScale ().x;
		float pr = POINT_RADIUS * scale;
		
		// pour chaque edge on crée un rectangle de l'epaisseur POINT_RADIUS * currentScale
		// et de la longueur de l'edge. Puis on applique une transformation au curseur correspondant
		// a l'orientation de l'edge. Et on verifie que le curseur intersecte le rectangle.
		
		foreach (Edge2 edge in _polygon.GetEdges ())
		{			
			Point2 prevPoint = edge.GetPrevPoint2 ();
			Point2 prevEdgePoint = new Point2 (prevPoint);
			if (prevPoint.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 aPoint = prevPoint as ArchedPoint2;
				prevEdgePoint.Set (aPoint.GetNextTangentPoint ());
			}
			
			Point2 nextPoint = edge.GetNextPoint2 ();
			Point2 nextEdgePoint = new Point2 (nextPoint);
			if (nextPoint.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 aPoint = nextPoint as ArchedPoint2;
				nextEdgePoint.Set (aPoint.GetPrevTangentPoint ());
			}
				
			Edge2 transformedEdge = new Edge2 (prevEdgePoint, nextEdgePoint);

			Vector2 prevPt = transformedEdge.GetPrevPoint2 ();
			prevPt.Set(prevPt.x+_offsetWidth, prevPt.y+_offsetHeight);
			Vector2 nextPt = transformedEdge.GetNextPoint2 ();
			nextPt.Set(nextPt.x+_offsetWidth, nextPt.y+_offsetHeight);
			
			Vector3 translation = new Vector3 (prevPt.x, 0, prevPt.y);
			Matrix4x4 translationMatrix = Matrix4x4.TRS (translation, Quaternion.identity, Vector3.one);

			Vector3 edgeVector = new Vector3 (transformedEdge.ToVector2 ().x, 0, transformedEdge.ToVector2 ().y);
			Quaternion rotation = Quaternion.FromToRotation (edgeVector, Vector3.right);
			Matrix4x4 rotationMatrix = Matrix4x4.TRS (Vector3.zero, rotation, Vector3.one);
			
			Matrix4x4 edgeMatrix = translationMatrix * rotationMatrix * translationMatrix.inverse;
			Vector3 transformedCursor3 = edgeMatrix.MultiplyPoint (new Vector3 (_cursor.x, 0, _cursor.y));
			Vector2 transformedCursor = new Vector2 (transformedCursor3.x, transformedCursor3.z);
			
			Rect edgeRect = new Rect (prevPt.x + (pr / 2), 
				                      prevPt.y - (pr / 2), 
				                      transformedEdge.GetLength () - pr, 
				                      pr);
			
			if (edgeRect.Contains (transformedCursor))
			{
				_selectedEdgeIndexCandidate = edgeCounter;
//				Debug.Log ("EDGE " + edgeCounter);
			}
			
			++edgeCounter;
		}
	}
	
	protected void FindIntersectedPoint ()
	{
		_selectedPointIndexCandidate = -1;
		int pointCounter = 0;
		
		float scale = 1 / _planTransformation.GetScale ().x;
		float pr = POINT_RADIUS * scale;
		
		// Pour chaque point on crée un rectangle dimensionné selon le scale courant
		// Puis on teste l'intersection entre le rectangle et le curseur
		foreach (Point2 point in _polygon.GetPoints ())
		{
			Vector2 p = point;
			Rect pointRect = new Rect (p.x - pr / 2 + _offsetWidth,
			                           p.y - pr / 2 + _offsetHeight,
				                       pr, pr);
			
			if (pointRect.Contains (_cursor))
			{
				_selectedPointIndexCandidate = pointCounter;
				//Debug.Log ("POINT " + pointCounter+" "+_cursor.ToString());
			}
			
			++pointCounter;
		}
	}
	
	public Polygon GetPolygon ()
	{
		return _polygon;	
	}
	
	public void SetPolygon (Polygon p)
	{
		_polygon = p;	
	}
	
	public Point2 GetSelectedPoint ()
	{
		if (_selectedPointIndex >= 0 && _selectedPointIndex < _polygon.GetPoints ().Count)
		{
			return _polygon.GetPoints () [_selectedPointIndex];	
		}
		else
		{
			return null;	
		}
	}
	
	public int GetSelectedPointIndex ()
	{
		return _selectedPointIndex;	
	}
	
	public void ResetSelection ()
	{
		_selectedPointIndex = -1;
		_selectedPoint = null;
		
		_selectedEdgeIndex = -1;
		_selectedEdge = null;
	}
	
	public void Clear ()
	{
		_selectedPoint = null;
		_selectedPointIndex = -1;
		_selectedEdge = null;
		_selectedEdgeIndex = -1;
		
		_polygon.Clear ();
	}
	
	public bool IsCotationVisible ()
	{
		return _cotationVisible;
	}
	
	public void SetCotationVisible (bool visible)
	{
		_cotationVisible = visible;	
	}
	
	public bool IsMaxDimensionVisible ()
	{
		return _maxDimensionVisible;
	}
	
	public void SetMaxDimensionVisible (bool visible)
	{
		_maxDimensionVisible = visible;	
	}
	
	/**
	 * Renvoit l'offset lié à la multi résolution
	 **/
	public Vector2 GetOffSet()
	{
		CaluclateOffSet();
		return new Vector2(_offsetWidth, _offsetHeight);
	}
	
	public bool ChangeSize()
	{
		//Calcul des valeurs
		Vector2 center = _polygon.GetPolygonCenter();
		Bounds polyBounds = _polygon.bounds;
		
		int poolWidth  = Mathf.RoundToInt (polyBounds.size.x);
		int poolHeight = Mathf.RoundToInt (polyBounds.size.z);
		
		float maxX = polyBounds.extents.x;
		float maxY = polyBounds.extents.z;
		
		float dx = 0f;
		float dy = 0f;
		float nw = 0f;
		float nh = 0f;
		if(m_newWidth != "")
		{
			m_newWidth = m_newWidth.Replace(',','.');
			if(float.TryParse(m_newWidth,out nw))
			{
				int newSizeW = Mathf.RoundToInt (nw * 100f);
				dx = newSizeW - poolWidth;
				dx = dx/2f;
			}
			else
				return false;
		}
		if(m_newHeight != "")
		{
			m_newHeight = m_newHeight.Replace(',','.');
			if(float.TryParse(m_newHeight,out nh))
			{
				int newSizeH = Mathf.RoundToInt (nh * 100f);
				dy = newSizeH - poolHeight;
				dy = dy/2f;
			}
			else
				return false;
		}
		
		//Application aux points
		for (int i = 0; i < _polygon.GetPoints().Count; i++)
		{	
			Vector2 point = _polygon.GetPoints()[i];
			float rx = Mathf.Round((point.x-center.x) / maxX);
			float ry = Mathf.Round((point.y-center.y) / maxY);
			
//			Debug.Log(_polygon.GetPoints()[i].GetJunction().ToString()+"-RX>"+rx+"|||RY>"+ry);	
			
			Vector2 target = new Vector2(point.x + (rx*dx),point.y + (ry*dy));
			
			_polygon.MovePoint(i,target);
		}
		
		if((Mathf.Abs(_polygon.bounds.size.x-(nw*100f))>0.5f || Mathf.Abs(_polygon.bounds.size.z-(nh*100f))>0.5f) && m_steps<=c_maxSteps)
		{
//			Debug.Log("Recursion !!!"+m_steps);
			m_steps ++;
			ChangeSize();
			
		}
		else
		{
//			Debug.Log("Recursion Steps Count !!!"+m_steps);
			m_steps = 0;
//			Debug.Log("ResetSteps !!!"+m_steps);
		}
//		for (int i = 0; i < _polygon.GetPoints().Count; i++)
//		{	
//			Vector2 point = _polygon.GetPoints()[i];
//			Debug.Log("point:"+i+">>"+point);
//		}
		
		//_planTransformation.ZoomToPolygon (_polygon, new Vector2(0,0));
		//_planTransformation.ZoomToImage(backgroundRect,new Vector2(0,0),1, true);
		
		Function_PoolDesigner function = _gui.GetFunction ();
		if(function.GetBackgroundImage()==null)
			StartCoroutine(ZoomToImageAuto());
		
		return true;
	}
	
	IEnumerator ZoomToImageAuto() {
		_planTransformation.ZoomToPolygon (_polygon, GetOffSet());
		
		Function_PoolDesigner function = _gui.GetFunction ();
		Rect  backgroundRect = function.GetBackgroundRect();
		yield return new WaitForEndOfFrame();
			float heightBefore = (backgroundRect.height!=0)?backgroundRect.height:UnityEngine.Screen.height;
			float widthBefore =(backgroundRect.width!=0)?backgroundRect.width:UnityEngine.Screen.width;
			float xBefore = backgroundRect.x;
			float yBefore = backgroundRect.y;
			float deltaZoom = 0.1f;
			float heightAfter = heightBefore * (1+deltaZoom);
			float widthAfter = widthBefore * (1+deltaZoom);
					
			Rect newPlanRect = new Rect(
						backgroundRect.x + (widthBefore-widthAfter)/2,
						backgroundRect.y + (heightBefore-heightAfter)/2,
						widthAfter,
						heightAfter
						);
			_planTransformation.ZoomToImage(newPlanRect,GetOffSet(),(1+deltaZoom), true);
					
			heightAfter = heightBefore / (1+deltaZoom);
			widthAfter = widthBefore / (1+deltaZoom);
					
			Rect newBgRect = new Rect(
						backgroundRect.x + (widthBefore-widthAfter)/2,
						backgroundRect.y + (heightBefore-heightAfter)/2,
						widthAfter,
						heightAfter
						);
			function.SetBackgroundRect (newBgRect);
		
	}
	public void Scale(float delta)
	{
		//Calcul des valeurs
		Vector2 center = _polygon.GetPolygonCenter();
		Bounds polyBounds = _polygon.bounds;
		
		int poolWidth  = Mathf.RoundToInt (polyBounds.size.x);
		int poolHeight = Mathf.RoundToInt (polyBounds.size.z);
		
		float maxX = polyBounds.extents.x;
		float maxY = polyBounds.extents.z;
		
		float dx = 0f;
		float dy = 0f;
		float nw = 0f;
		float nh = 0f;
		if(m_newWidth != "")
		{
			if(float.TryParse(m_newWidth,out nw))
			{
				int newSizeW = Mathf.RoundToInt (nw * 100f);
				dx = newSizeW - poolWidth;
				dx = dx/2f;
			}
			else
				return;
		}
		if(m_newHeight != "")
		{
			if(float.TryParse(m_newHeight,out nh))
			{
				int newSizeH = Mathf.RoundToInt (nh * 100f);
				dy = newSizeH - poolHeight;
				dy = dy/2f;
			}
			else
				return;
		}
		
		//Application aux points
		for (int i = 0; i < _polygon.GetPoints().Count; i++)
		{	
			Vector2 point = _polygon.GetPoints()[i];
			float rx = Mathf.Round((point.x-center.x) / maxX);
			float ry = Mathf.Round((point.y-center.y) / maxY);
			
			Vector2 target = new Vector2(point.x + (rx*delta),point.y + (ry*delta));
			
			_polygon.MovePoint(i,target);
		}
	}
	
	public void MovePolygon(Vector2 delta)
	{
		for (int i = 0; i < _polygon.GetPoints().Count; i++)
		{	
			Vector2 point = _polygon.GetPoints()[i];
			
			Vector2 target = new Vector2(point.x + (delta.x/_planTransformation.GetScale().x),
										point.y + (delta.y/_planTransformation.GetScale().x));
			
			_polygon.MovePoint(i,target);
		}
	}
	
	public void SetPositionPolygon(Vector2 v2position)
	{
		for (int i = 0; i < _polygon.GetPoints().Count; i++)
		{	
			Vector2 point = _polygon.GetPoints()[i];
			
			/*Vector2 target = new Vector2(point.x + (delta.x/_planTransformation.GetScale().x),
			                             point.y + (delta.y/_planTransformation.GetScale().x));*/
			
			_polygon.MovePoint(i,v2position);
		}
	}
	
	public void SetGridVisible(bool state)
	{
		_gridVisible = state;
	}
	public bool IsGridVisible()
	{
		return _gridVisible;
	}
	#region multi_res
	//-----------------------------------------------------
    // Note : Quand mode2D désactivé, resize fait quand même mais pas directement sur la guiTexture
    //          => sur le rect m_savedBgImgPixIn à la place
    public void ResizeRectsToScreen()
    {
		CaluclateOffSet();
		_rectBackground = new Rect(0,0,
			UnityEngine.Screen.width,
			UnityEngine.Screen.height);
		if(_snapper!=null)
			_snapper.SetOffset(_offsetWidth,_offsetHeight);		

    } // ResizeRectsToScreen()
	private void CaluclateOffSet()
	{
		_offsetWidth	=	 (UnityEngine.Screen.width - _background_width)/2;
		_offsetHeight	=	 (UnityEngine.Screen.height - _background_height)/2;
	}
	#endregion
	
	
}