using UnityEngine;
using System.Collections;

using Pointcube.Global;

public enum PoolDesignerMode
{
	None,
	PolygonEdition,
	PlanTransformation,
	BackgroundScale,
	BackgroundTranslation,
	PolygonMoveAndScale
}

public class PoolUIv2 : MonoBehaviour, FunctionUI_OS3D
{
	//-----------------------------------------------
	// -- Références scène --
    public    GameObject _backgroundImg;

	protected static int ZOOM_SIZE = 50;
	protected static Color OUT_OF_BOUND_COLOR = new Color (180, 180, 180, 180);
	
	protected Function_PoolDesigner _function;
	
	protected Polygon _polygon;
	
	protected PolygonDrawer _polyDrawer;
	protected Snapper _snapper;
	protected PlanTransformation _planTransformation;
	
	protected Camera _planCamera;
	protected Camera _renderCamera;
	
	protected int _selectedFunction = 0;
	
	protected bool canDisp = false;
	protected bool canHideMenu = false;
	protected bool isTwoLevelMenu = false;
	protected bool isJustShowing = false;
	
	protected bool _isSliding = false;
	
	protected float leftOff7 = -320;
	protected float rightOff7 = UnityEngine.Screen.width;
	protected float bgImgOff7 = UnityEngine.Screen.width;
	
	Vector2 scrollpos = new Vector2 (0, 0);
	
	protected Rect leftMenuGroup;
	protected Rect rightMenuGroup;
	protected Rect bgImgMenuGroup;
	
	protected Rect leftRect;
	protected Rect rightRect;
	protected Rect bgImgRect;
	
	protected bool showLeftMenu = false;
	protected bool showRightMenu = false;
	protected bool showBgImgMenu = false;
	protected bool bgImgMode = false;
	
	protected string iPadPath = "";
	
	protected PoolDesignerMode _mode = PoolDesignerMode.PolygonEdition;
	
	protected Edge2 _backgroundEdge;
	
	protected Point2 _movingBgPoint;
	protected Vector2 _movingBgPointOffset;
	
	protected Vector2 prevMousePosition;
	
	protected int lastUIValue = -1;
	
	protected GameObject _mainCamera;
	
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
	protected Texture2D _zoomTexture;
#endif
	
	bool selectEditMode = false;
	public GUISkin skin;
	
    // -- DEBUG --
    private static readonly string DEBUGTAG = "PoolDesignerUI : ";   
    private static readonly bool   DEBUG    = true;
	//-----------------------------------------------
	
	private Rect m_headerRect;
	private Rect m_drawRect,m_planRect,m_paramRect;
	private Rect m_tmpDrawRect,m_tmpPlanRect,m_tmpParamRect;
	private Rect m_mainView;
	private Rect m_askRect;
	
	private int m_selM = 0;
	private const int c_drawId = 0;
	private const int c_planId = 1;
	private const int c_paramId = 2;
	
	private float m_btnW = 170f;
	private float m_btnWL = 200f;
	private float m_btnH = 50f;
	private float m_speed = 20f;
	
	private bool m_open = false;
	private bool m_subMenuSelected = true;
	
	private string[] m_menuTxts   = {"PoolDesigner.LeftTitle","PoolDesigner.Image","GUIMenuRight.Parameters"};
	private string[] m_drawMenus  = {"PoolDesigner.Edit","DynShelter.Move","GUIMenuRight.newProject","GUIMenuRight.reset", "GUIMenuInteraction.Refocus"};
	private string[] m_planMenus  = {"PoolDesigner.Image.Load","PoolDesigner.Image.Move",/*"PoolDesigner.Image.Scale",*/"PoolDesigner.Image.Remove"/*,"PoolDesigner.Image.ShowHide"*/};
	private string[] m_paramMenus = {"PoolDesigner.Snap","PoolDesigner.SquareCotation","PoolDesigner.Image.ShowHide"};
	private string[] m_menuStylesOff = {"trace","plan","param"};
	private string[] m_menuStylesOn  = {"traceOn","planOn","paramOn"};
	
	//test
	public float sldVal,sldMax;
	string tmpStr="";
	bool magnet,size;
	public System.Action m_confirmedAction;
	private bool m_askConfirmation = false;
	private string m_askMessage="";
	private int selectedPointIndex;
	bool actionToCloseMenu = false;
	
	bool _justCloseMenu = false;
	
	// Use this for initialization
	void Start ()
	{
		//NEW UI ----------------------------------------------
		m_headerRect = new Rect(0,0,Screen.width,m_btnH);
		
		m_drawRect = new Rect(0,-m_drawMenus.Length* m_btnH,m_btnWL,m_drawMenus.Length* m_btnH);
		m_tmpDrawRect = new Rect(0,m_btnH,m_btnWL,m_btnH);
		
//		m_planRect = new Rect(m_btnW,-m_planMenus.Length* m_btnH,m_btnWL,m_planMenus.Length* m_btnH);
		m_planRect = new Rect(m_btnW,-m_planMenus.Length* m_btnH,m_btnWL,m_btnH);
		m_tmpPlanRect = new Rect(0,m_btnH,m_btnWL,m_btnH);
		
		m_paramRect = new Rect(2*m_btnW,-m_paramMenus.Length* m_btnH,m_btnWL*1.25f,m_paramMenus.Length* m_btnH);
		m_tmpParamRect = new Rect(0,m_btnH,m_btnWL*1.25f,m_btnH);
		
		m_mainView = new Rect(0,m_btnH,Screen.width,Screen.height-(2*m_btnH));
		m_askRect  = new Rect(Screen.width/3f,Screen.height/4f,Screen.width/3f,Screen.height/4f);
		//NEW UI ----------------------------------------------
		
		 if(_backgroundImg == null)  Debug.LogError(DEBUGTAG+"Background Image"+PC.MISSING_REF);

		GameObject cameraGameObject = GameObject.Find ("PlanCamera");
		_planCamera = cameraGameObject.GetComponent<Camera>();
		_mainCamera = GameObject.Find ("mainCam");
		_renderCamera = Camera.main;
		
		_polyDrawer = cameraGameObject.GetComponent <PolygonDrawer> ();
		_snapper = cameraGameObject.GetComponent <Snapper> ();
		_planTransformation = cameraGameObject.GetComponent <PlanTransformation> ();
		
		_planCamera.GetComponent<Camera>().enabled = false;
		_polyDrawer.enabled = false;
		_planTransformation.enabled = false;
		
		enabled = false;
		
		Point2 prevBgPoint = new Point2  (UnityEngine.Screen.width / 2 - 50, UnityEngine.Screen.height / 2);
		Point2 nextBgPoint = new Point2  (UnityEngine.Screen.width / 2 + 50, UnityEngine.Screen.height / 2);
		
		_backgroundEdge = new Edge2 (prevBgPoint, nextBgPoint);
		
		prevBgPoint.SetNextEdge (_backgroundEdge);
		nextBgPoint.SetPrevEdge (_backgroundEdge);
	}

	// fonction de mise a jour du polygone avec la bonne valeur de l'arc de cercle
	// et mise a jour de l'interface (slider et label)
	protected void UpdateCurveSliderValue ()
	{
		if (_polyDrawer.GetSelectedPointIndex () >= 0)
		{
			Point2 selectedPoint = _polyDrawer.GetSelectedPoint ();
			int selectedPointIndex = _polyDrawer.GetSelectedPointIndex ();
			
//			float curveRadius = _curveSlider.GetValue () * 100;
			float curveRadius = sldVal * 100;
			
			if (selectedPoint != null &&  selectedPoint.GetJunction () == JunctionType.Broken)
			{
				if (curveRadius > 0 && selectedPoint.GetPrevEdge () != null && selectedPoint.GetNextEdge () != null)
				{
					_polygon.SetJunctionType (selectedPointIndex, JunctionType.Curved);
					ArchedPoint2 ar = _polygon.GetPoints ()[selectedPointIndex] as ArchedPoint2;
//					_curveSlider.SetMaxValue (ar.GetMaxRadius () / 100);
					sldMax = ar.GetMaxRadius () / 100;
					ar.SetRadius (curveRadius);
					_polygon.UpdateBounds ();
				}
			}
			else if (selectedPoint != null && selectedPoint.GetJunction () == JunctionType.Curved)
			{
				if (curveRadius > 0)
				{
					ArchedPoint2 ar = _polygon.GetPoints ()[selectedPointIndex] as ArchedPoint2;
					float pointRadius = ar.GetMeasuredRadius () / 100;
					float maxRadius = ar.GetMaxRadius () / 100;
					
					if (!Utils.Approximately (pointRadius, curveRadius))
					{
						ar.SetRadius (curveRadius);
						_polygon.UpdateBounds ();
					}
					
					if (!Utils.Approximately (maxRadius,sldMax/100 /*_curveSlider.GetMaxValue () / 100*/))
					{
						//_curveSlider.SetMaxValue (maxRadius);
						sldMax = maxRadius;
					}
				}
				else
				{
					_polygon.SetJunctionType (selectedPointIndex, JunctionType.Broken);
				}
			}	
		}
	}

	void Update()
	{
		//Animation des panneaux
		if(m_selM == c_drawId && m_open)
			m_drawRect.y += m_speed;
		else
			m_drawRect.y -= m_speed;
		m_drawRect.y = Mathf.Clamp(m_drawRect.y,-m_drawMenus.Length* m_btnH,0);
		
		if(m_selM == c_planId && m_open)
			m_planRect.y += m_speed;
		else
			m_planRect.y -= m_speed;
		m_planRect.y = Mathf.Clamp(m_planRect.y,-m_planMenus.Length* m_btnH,0);
		
		if(m_selM == c_paramId && m_open)
			m_paramRect.y += m_speed;
		else
			m_paramRect.y -= m_speed;
		m_paramRect.y = Mathf.Clamp(m_paramRect.y,-m_paramMenus.Length* m_btnH,0);
		
		//Clic dans le vide pour fermer les menus
		if(PC.In.Click1Down() && _mode != PoolDesignerMode.BackgroundScale)
		{
			if(PC.In.CursorOnUI(m_mainView))
			{
				Rect r1 = m_drawRect;
				r1.y+=m_btnH;
				Rect r2 = m_planRect;
				r2.y+=m_btnH;
				Rect r3 = m_paramRect;
				r3.y+=m_btnH;
				if(!PC.In.CursorOnUIs(r1,r2,r3))
				{
					m_open = false;
					if(!m_subMenuSelected)
					{
						m_selM = -1;
						_mode = PoolDesignerMode.PolygonEdition;
					}
					return;
				}
			}
		}
		
		UpdateCurveSliderValue ();
		
		// gestion des points de reference permettant la mise à l'echelle
		// de l'image de fond
		if (PC.In.Click1Up())
		{
			_movingBgPoint = null;
			
		}
	
		if (_movingBgPoint != null)
		{
			Vector2 newPosition = PC.In.GetCursorPosInvY() - _movingBgPointOffset;
			_movingBgPoint.Set (newPosition);	
		}
		
		// gestion de la translation de l'image de fond
		if (_mode == PoolDesignerMode.BackgroundTranslation)
		{
			if(PC.In.Click1Down())
			{
				prevMousePosition = PC.In.GetCursorPosInvY();
			}
			
			if(PC.In.Click1Hold())
			{
				float scale = _planTransformation.GetScale ().x;
				
				Vector2 mousePosition = PC.In.GetCursorPosInvY();
				Vector2 translation = mousePosition - prevMousePosition;
				
				Rect backgroundRect = _function.GetBackgroundRect ();
				backgroundRect.x = backgroundRect.x + translation.x / scale;
				backgroundRect.y = backgroundRect.y + translation.y / scale;

				_function.SetBackgroundRect (backgroundRect);
				
				prevMousePosition = mousePosition;
			}
		}
		
		//New déplacement/scale du tracé
		if(_mode == PoolDesignerMode.PolygonMoveAndScale)
		{
			Vector2 deltaMove;
			float deltaZoom;
			
			if(PC.In.Zoom_n_drag1(out deltaZoom,out deltaMove))
			{
				deltaMove.y *=-1;
				_polyDrawer.MovePolygon(deltaMove);
				Rect  backgroundRect = _function.GetBackgroundRect();//, deltaZoom*10f);
				if(Mathf.Abs(deltaZoom)>0)
				{
					float heightBefore = (backgroundRect.height!=0)?backgroundRect.height:UnityEngine.Screen.height;
					float widthBefore =(backgroundRect.width!=0)?backgroundRect.width:UnityEngine.Screen.width;
					float xBefore = backgroundRect.x;
					float yBefore = backgroundRect.y;
					
					
					float heightAfter = heightBefore * (1+deltaZoom);
					float widthAfter = widthBefore * (1+deltaZoom);
					
					Rect newPlanRect = new Rect(
						backgroundRect.x + (widthBefore-widthAfter)/2,
						backgroundRect.y + (heightBefore-heightAfter)/2,
						widthAfter,
						heightAfter
						);
					
					_planTransformation.ZoomToImage(newPlanRect,_polyDrawer.GetOffSet(),(1+deltaZoom), true);
					
					heightAfter = heightBefore / (1+deltaZoom);
					widthAfter = widthBefore / (1+deltaZoom);
					
					Rect newBgRect = new Rect(
						backgroundRect.x + (widthBefore-widthAfter)/2,
						backgroundRect.y + (heightBefore-heightAfter)/2,
						widthAfter,
						heightAfter
						);
						
					_function.SetBackgroundRect (newBgRect);
				}
			}
		}
		
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		// copie la texture de fond pour dessiner la zone au dessus du doigt
		if (_mode == PoolDesignerMode.BackgroundScale && _movingBgPoint != null)
		{
			// calcule taille de l'image de fond selon les dimensions de l'ecran
			Texture2D bgTex = _function.GetBackgroundImage ();

			float wFactor =  (float)bgTex.width / (float)UnityEngine.Screen.width;
			float hFactor =  (float)bgTex.height / (float)UnityEngine.Screen.height;

			float factor = (float)bgTex.width / (float)bgTex.height;

			int w;
			int h;

			if (wFactor > hFactor)
			{
				w = UnityEngine.Screen.width;
				h = Mathf.RoundToInt ((float)w / factor);
			}
			else
			{
				h = UnityEngine.Screen.height;
				w = Mathf.RoundToInt (h * factor);
			}

			// decalage entre les dimensions de l'image et celles de l'ecran
			int wOffset = (UnityEngine.Screen.width - w) / 2;
			int hOffset = (UnityEngine.Screen.height - h) / 2;

			// facteur entre l'image reel et redimensionnée
			float wMult = (float)bgTex.width / (float)w;
			float hMult = (float)bgTex.height / (float)h;
			
			Vector2 movingBgPt = _movingBgPoint;
			movingBgPt.x = movingBgPt.x - (ZOOM_SIZE / 2) / wMult;
			movingBgPt.y = UnityEngine.Screen.height - (movingBgPt.y + (ZOOM_SIZE / 2) / hMult);
			
			int reelX = (int)((movingBgPt.x - wOffset) * wMult);
			int reelY = (int)((movingBgPt.y - hOffset) * hMult);

			// dimension du carre de pixel apres transformation
			int pW = (int)(ZOOM_SIZE * wMult);
			int pH = (int)(ZOOM_SIZE * hMult);
			
			// position du carre de pixel apres transformation
			int x = reelX - (pW - ZOOM_SIZE) / 2;
			int y = reelY - (pH - ZOOM_SIZE) / 2;

			Color [] cols = new Color [pW * pH];

			// copie des pixels
			for (int row = 0; row < pH; ++row)
			{
				for (int col = 0; col < pW; ++col)
				{
					int colIndex = col + row * pH;

					int xI = col + x;
					int yI = row + y;

					if (xI < 0 || yI < 0 || yI > bgTex.height || xI > bgTex.width)
					{
						cols[colIndex] = OUT_OF_BOUND_COLOR;
					}
					else
					{
						cols[colIndex] = bgTex.GetPixel (xI, yI);
					}
				}
			}

			_zoomTexture.SetPixels (cols);
			_zoomTexture.Apply ();
		}
#endif
	}
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR	
	// creation de la texture d'apercu lors de la mise a l'echelle
	// en fonction de l'image chargée
	protected void SetZoomTextureDimension ()
	{
		Texture2D bgTex = _function.GetBackgroundImage ();
			
		float wFactor =  (float)bgTex.width / (float)UnityEngine.Screen.width;
		float hFactor =  (float)bgTex.height / (float)UnityEngine.Screen.height;
		
		float factor = (float)bgTex.width / (float)bgTex.height;
		
		int w;
		int h;
		
		if (wFactor > hFactor)
		{
			w = UnityEngine.Screen.width;
			h = Mathf.RoundToInt ((float)w / factor);
		}
		else
		{
			h = UnityEngine.Screen.height;
			w = Mathf.RoundToInt (h * factor);
		}
		
		float wMult = (float)bgTex.width / (float)w;
		float hMult = (float)bgTex.height / (float)h;
		
		int pW = (int)(ZOOM_SIZE * wMult);
		int pH = (int)(ZOOM_SIZE * hMult);
		
		_zoomTexture = new Texture2D (pW, pH);
	}
#endif
	
	public Polygon GetPolygon()
	{
		return _polygon;
	}
	
	void LateUpdate ()
	{		
		if(showRightMenu)//version IPAD
		{
			if(PC.In.Click1Down() && !isOnUI() && _polyDrawer.GetSelectedPointIndex () < 0 ||
				_mode == PoolDesignerMode.PlanTransformation || !_polygon.IsClosed ())
			{
				showRightMenu = false;
				canHideMenu = false;
			}
		}
		
		// on quitte le mode edition de l'image de fond
		if (_mode == PoolDesignerMode.PolygonEdition &&
			bgImgMode &&
			PC.In.Click1Down() &&
			_polyDrawer.GetSelectedPointIndex () >= 0 &&
			! isOnUI ()) 
		{
			bgImgMode = false;
			selectEditMode = false;
//			LeftRoot.setSelected (0);
			showBgImgMenu = false;
		}
			
		if (selectEditMode)
		{
			selectEditMode = false;
//			LeftRoot.setSelected (0);
		}
	}
	
	void OnGUI()
	{
		GUI.depth = -11;
		GUI.skin = skin;

		//CONFIRMATION BOX
		if(m_askConfirmation)
		{
			GUI.BeginGroup(m_askRect,"","bg");
			
			GUI.Label(new Rect(0,0,m_askRect.width,m_askRect.height/2f),m_askMessage,"msgbox");
			if(GUI.Button(new Rect(m_askRect.width/12f,m_askRect.height*2f/3f,m_askRect.width/3f,m_askRect.height/4f),"OK","btn"))
			{
				if(m_confirmedAction!=null)
					m_confirmedAction();
				m_askConfirmation = false;
			}
			if(GUI.Button(new Rect(m_askRect.width*7f/12f,m_askRect.height*2f/3f,m_askRect.width/3f,m_askRect.height/4f),"Cancel","btn"))
			{
				m_confirmedAction = null;
				m_askConfirmation = false;
			}
			
			GUI.EndGroup();
			return;
		}
		
		//SUBMENUS
		GUI.BeginGroup(m_mainView);
		DrawUI();
		PlanUI();
		ParamUI();
		GUI.EndGroup();
		
		//FOOTER SLIDER ARRONDI
		if(_mode == PoolDesignerMode.PolygonEdition)
		{
			selectedPointIndex = _polyDrawer.GetSelectedPointIndex ();
			
			GUI.BeginGroup(new Rect(0,Screen.height-m_btnH,Screen.width,m_btnH),"","bg");
			if (selectedPointIndex >= 0)
			{	
				GUI.Label(new Rect(0,0,100,m_btnH),TextManager.GetText("PoolDesigner.RightTitle")+" : ");
				
				//TXTFIELD
				tmpStr = GUI.TextField(new Rect(100,0,m_btnH,m_btnH),tmpStr);
				GUI.Label(new Rect(130,0,20,m_btnH),"m");
				if(tmpStr != sldVal.ToString("F2") && GUI.Button(new Rect(160f,0,m_btnH,m_btnH),"","rfrsh"))
				{
					sldVal = float.Parse(tmpStr);
					sldVal = Mathf.Clamp(sldVal,0,sldMax);
					tmpStr = sldVal.ToString("F2");
				}
				
				//SLIDER
				float tmpVal = GUI.HorizontalSlider(new Rect(200f,0,Screen.width - 400f,m_btnH),sldVal,0,sldMax);
				if(tmpVal != sldVal)
				{
					sldVal = tmpVal;
					tmpStr = sldVal.ToString("F2");
				}
				
				//Btn suppression de point
				if(GUI.Button(new Rect(Screen.width-m_btnW,0,m_btnW,m_btnH),TextManager.GetText("PoolDesigner.RemovePoint"),"suppr"))
				{
					m_confirmedAction = DeleteSelectedPoint;
					m_askMessage = TextManager.GetText("PoolDesigner.RemovePoint")+"?";
					m_askConfirmation = true;
				}
			}
			else
			{
#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
				GUI.Label(new Rect(0,0,3*m_btnWL,m_btnH),TextManager.GetText("PoolDesigner.Footer"));
#else
				GUI.Label(new Rect(0,0,3*m_btnWL,m_btnH),TextManager.GetText("PoolDesigner.FooterTablette"));
#endif
			}
			GUI.EndGroup();
		}
		
		//MENUBAR
		GUI.BeginGroup(m_headerRect,"","bg");
		for(int i=0;i<3;i++)
		{
			if(GUI.Button(new Rect(i*m_btnW,0,m_btnW,m_btnH),TextManager.GetText(m_menuTxts[i]),(i==m_selM? m_menuStylesOn[i]:m_menuStylesOff[i])))
			{
				if(m_selM == i)
					m_open = !m_open;
				else
				{
					m_selM = i;
					m_open = true;
					m_subMenuSelected = false;
					_mode = PoolDesignerMode.None;
				}
			}
		}
		//---------
		if(GUI.Button(new Rect(Screen.width-74-100,0,100,m_btnH),"","quit"))
		{
			//Close without save
			Reset();
			Validate();
		}
		if(GUI.Button(new Rect(Screen.width-74,0,74,m_btnH),"","valid"))
		{
			// save and close
			Validate();
		}
		GUI.EndGroup();
		
		if (_mode == PoolDesignerMode.BackgroundScale && _function.GetBackgroundImage () != null)
		{		
			// dessin de l'image de fond en mode taille max par rapport a l'ecran
			// pour regler l'echelle
			Rect backgroundRect = new Rect (0, 0, UnityEngine.Screen.width, UnityEngine.Screen.height);
		//	GUI.DrawTexture (backgroundRect, _function.GetBackgroundImage (), ScaleMode.ScaleToFit);
		/*	
			Point2 prevPt = _backgroundEdge.GetPrevPoint2 ();
			Point2 nextPt = _backgroundEdge.GetNextPoint2 ();
			
			Vector2 prevPtV = prevPt;
			Vector2 nextPtV = nextPt;
			
			// dessin du segment
			EdgeDrawer.Draw (_backgroundEdge, Matrix4x4.identity);
			
			// dessin des points de reference pour regler l'echelle
			if (GUI.RepeatButton (new Rect (prevPt.GetX () - 25, prevPt.GetY () - 25, 50, 50), "", "viewfinder"))
			{
				_movingBgPoint = prevPt;
                _movingBgPointOffset = PC.In.GetCursorPosInvY() - prevPtV;
			}
			
			if (GUI.RepeatButton (new Rect (nextPt.GetX () - 25, nextPt.GetY () - 25, 50, 50), "", "viewfinder"))
			{
				_movingBgPoint = nextPt;
                _movingBgPointOffset = PC.In.GetCursorPosInvY() - nextPtV;
			}
			
			float angle = Vector2.Angle (nextPtV - prevPtV, Vector2.right);
					
			if (prevPtV.y > nextPtV.y) 
			{ 
				angle = -angle; 
			}
			
			Matrix4x4 m = GUI.matrix;
			
        	GUIUtility.RotateAroundPivot (angle, prevPtV);
			
			// taille du segment apres transformation matricielle
			float edgeLength = Vector2.Distance (nextPtV, prevPtV);
			
			GUI.Label (new Rect (prevPtV.x + edgeLength / 2 - 20,
				                 prevPtV.y - 30, 40, 20), "1m", "dimension label");
			
			GUI.matrix = m;*/
			ValidateBackground ();	
			/*if (_movingBgPoint == null)
			{				
				Rect validateRect = new Rect (0,150,250,50);
				
				if (GUI.Button (validateRect, TextManager.GetText("PoolDesigner.ValidateScale"), "validate scale"))
				{
					ValidateBackground ();	
				}
			}
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR	
			else
			{
				Vector2 mBgPt = _movingBgPoint;
				GUI.DrawTexture (new Rect (mBgPt.x - ZOOM_SIZE / 2, mBgPt.y - 80, ZOOM_SIZE, ZOOM_SIZE), _zoomTexture, ScaleMode.StretchToFill);
			
				GUI.Box (new Rect (mBgPt.x - ZOOM_SIZE / 2, mBgPt.y - 80, ZOOM_SIZE, ZOOM_SIZE), "", "viewfinder");	
			}
#endif
*/
		}
	}
	
	private void DrawUI()
	{
		GUI.BeginGroup(m_drawRect,"","bg");
		m_tmpDrawRect.y = 0;		
		if(GUI.Button(m_tmpDrawRect,TextManager.GetText(m_drawMenus[0]),"bg"))//Editer
		{
			_mode = PoolDesignerMode.PolygonEdition;
			m_open = false;
			m_subMenuSelected = true;
		}
		m_tmpDrawRect.y += m_btnH;
		if(GUI.Button(m_tmpDrawRect,TextManager.GetText(m_drawMenus[1]),"bg"))//Placer
		{
//			_mode = PoolDesignerMode.PlanTransformation;
			_mode = PoolDesignerMode.PolygonMoveAndScale;
			m_open = false;
			m_subMenuSelected = true;
		}
		m_tmpDrawRect.y += m_btnH;
		if(GUI.Button(m_tmpDrawRect,TextManager.GetText(m_drawMenus[2]),"bg"))//Nouveau
		{
//			Clear();
			m_confirmedAction = Clear;
			m_askMessage = TextManager.GetText(m_drawMenus[2])+"?";
			m_askConfirmation = true;
			m_open = false;
			m_subMenuSelected = true;
			_justCloseMenu = true;
		}		
		m_tmpDrawRect.y += m_btnH;
		if(GUI.Button(m_tmpDrawRect,TextManager.GetText(m_drawMenus[3]),"bg"))//Reset
		{
//			Reset();
			m_confirmedAction = Reset;
			m_askMessage = TextManager.GetText(m_drawMenus[3])+"?";
			m_askConfirmation = true;
			m_open = false;
			m_subMenuSelected = true;
			_justCloseMenu = true; 
		}	
		m_tmpDrawRect.y += m_btnH;
		if(GUI.Button(m_tmpDrawRect,TextManager.GetText(m_drawMenus[4]),"bg"))//Recentrer
		{
			Vector2 v2direction = new Vector2(Screen.width* 0.5f, Screen.height * 0.5f) - _polygon.GetPolygonCenter();
			float fmagnitude = v2direction.magnitude;
			v2direction.Normalize();
			
			while(fmagnitude > 10.0f)
			{
				_polyDrawer.MovePolygon(v2direction * fmagnitude * 0.5f);
				
				v2direction = new Vector2(Screen.width* 0.5f, Screen.height * 0.5f) - _polygon.GetPolygonCenter();
				fmagnitude = v2direction.magnitude;
				v2direction.Normalize();
			}
			
			_polyDrawer.ChangeSize();
		}
		
		GUI.EndGroup();
	}
	private void PlanUI()
	{
		GUI.BeginGroup(m_planRect,"","bg");
		m_tmpPlanRect.y = 0;		
		if(GUI.Button(m_tmpPlanRect,TextManager.GetText(m_planMenus[0]),"bg"))//Charger
		{
			loadImage ();
			m_subMenuSelected = false;
		}
		m_tmpPlanRect.y += m_btnH;
		if(GUI.Button(m_tmpPlanRect,TextManager.GetText(m_planMenus[1]),"bg"))//PLacer
		{
//			_mode = PoolDesignerMode.BackgroundTranslation;
			_mode = PoolDesignerMode.PlanTransformation;
			m_open = false;
			m_subMenuSelected = true;
		}
		
		if(!_function.HasBackgroundImage())
		{
			if(m_planRect.height != 2*m_btnH)
				m_planRect.height = 2*m_btnH;
			
			GUI.EndGroup();
			return;
		}
		else if(m_planRect.height != m_planMenus.Length* m_btnH)
		{
			m_planRect.height = m_planMenus.Length* m_btnH;
		}
		
		/*m_tmpPlanRect.y += m_btnH;
		if(GUI.Button(m_tmpPlanRect,TextManager.GetText(m_planMenus[2]),"bg"))//Echelle
		{
			_mode = PoolDesignerMode.BackgroundScale;
			m_open = false;
			m_subMenuSelected = false;
		}*/
		m_tmpPlanRect.y += m_btnH;
		if(GUI.Button(m_tmpPlanRect,TextManager.GetText(m_planMenus[2]),"bg"))//Supprimer
		{
//			Texture2D bgImg = _function.GetBackgroundImage ();
//			if (bgImg != null)
//			{
//				_function.SetBackgroundImage (bgImg);
//				Destroy (bgImg);	
//			}
			m_confirmedAction = SuppressPlan;
			m_askMessage = TextManager.GetText(m_planMenus[2])+"?";
			m_askConfirmation = true;
			m_subMenuSelected = false;
		}
		m_tmpPlanRect.y += m_btnH;
		/*bool tmp = GUI.Toggle(m_tmpPlanRect,_function.IsBackgroundImageVisible(),TextManager.GetText(m_planMenus[4]));//Afficher/cacher
		if(tmp != _function.IsBackgroundImageVisible())
		{
			_function.SetBackgroundImageVisible(tmp);
		}*/
		GUI.EndGroup();
	}
	private  void ParamUI()
	{
		GUI.BeginGroup(m_paramRect,"","bg");
		m_tmpParamRect.y = 0;		
		magnet = GUI.Toggle(m_tmpParamRect,_snapper.IsAlignedPointSnapActivate(),TextManager.GetText(m_paramMenus[0]));
		if(magnet != _snapper.IsAlignedPointSnapActivate())
		{
			_snapper.SetAlignedPointSnapActive(magnet);
			_mode = PoolDesignerMode.PolygonEdition;
			m_open = false;
			m_subMenuSelected = true;
		}
		
		m_tmpParamRect.y += m_btnH;
		size = GUI.Toggle(m_tmpParamRect,_polyDrawer.IsMaxDimensionVisible(),TextManager.GetText(m_paramMenus[1]));
		if(size != _polyDrawer.IsMaxDimensionVisible())
		{
			_polyDrawer.SetMaxDimensionVisible(size);
			_mode = PoolDesignerMode.PolygonEdition;
			m_open = false;
			m_subMenuSelected = true;
		}
		
		m_tmpParamRect.y += m_btnH;
		bool tmp = GUI.Toggle(m_tmpParamRect,_polyDrawer.IsGridVisible(),TextManager.GetText(m_paramMenus[2]));//Afficher/cacher
		if(tmp !=_polyDrawer.IsGridVisible())
		{
			_polyDrawer.SetGridVisible(tmp);
			_mode = PoolDesignerMode.PolygonEdition;
			m_open = false;
			m_subMenuSelected = true;
		}
		
		GUI.EndGroup();
	}
	
	public void DoActionUI(GameObject gameobject)
	{		
	//	UnityEngine.Camera.mainCamera.enabled = false;
		GetComponent<GUIMenuConfiguration>().setVisibility(false);			
		GetComponent<GUIMenuInteraction>().setVisibility(false);
		GetComponent<GUIMenuInteraction>().isConfiguring = false;
		GetComponent<GUIMenuLeft> ().setVisibility (false);
		
		GetComponent<HelpPanel>().enabled = false;
		
		_renderCamera.GetComponent<GuiTextureClip>().enabled = false;
		_renderCamera.GetComponent<ObjInteraction>().setSelected(null,true);
		_renderCamera.GetComponent<ObjInteraction>().setActived(false);
		_renderCamera.GetComponent<Mode2D>().enabled = false;		
		//_renderCamera.enabled = false;

		_function = gameobject.GetComponent<Function_PoolDesigner> ();
		_polygon = _function.GetPolygon ();
		
		_polyDrawer.SetPolygon (_polygon);
		_snapper.SetPolygon (_polygon);
		
		_planCamera.enabled = true;
		_snapper.enabled = true;
		_polyDrawer.enabled = true;
		_planTransformation.enabled = true;
		
		canDisp = true;
		enabled = true;
		
		_mode = PoolDesignerMode.PolygonEdition;
		//m_open = false;
		//m_subMenuSelected = true;
		
		_planTransformation.ZoomToPolygon (_polygon,_polyDrawer.GetOffSet());
		//_mode = PoolDesignerMode.PolygonEdition;
		
#if UNITY_IPHONE && !UNITY_EDITOR		
		EtceteraManager.imagePickerChoseImageEvent += OpenFileIpad;
		EtceteraManager.imagePickerCancelledEvent += OpenFileIpadFailed;
#endif			
#if UNITY_ANDROID && !UNITY_EDITOR		
		EtceteraAndroidManager.albumChooserSucceededEvent += OpenFileAndroid;
		EtceteraAndroidManager.albumChooserCancelledEvent += OpenFileAndroidFailed;
#endif			
		
		_mode = PoolDesignerMode.PolygonEdition;
		m_open = false;
		m_subMenuSelected = true;
		
	}
	
	public void canDisplay(bool b)
	{
		canDisp = b;
	}
//	public bool isVisible()
//	{
//		return showLeftMenu;
//	}
	
	public bool isOnUI() // TODO quick-fixé pour les nouveaux input. Pour bien fixer : tenir à jour le tableau des rects
	{                    // en temps réel (ie. quand on ouvre le menuRight, mettre le rectangle right dans le tableau,
                         // et quand on le ferme l'enlever).
		bool OnUI = false;
		Rect tmpRect;
		if(m_selM == c_drawId && m_open)
		{
			tmpRect = m_drawRect;
			tmpRect.y += m_btnH;
			OnUI |= PC.In.CursorOnUI(tmpRect);
		}
		if(m_selM == c_planId && m_open)
		{
			tmpRect = m_planRect;
			tmpRect.y += m_btnH;
			OnUI |= PC.In.CursorOnUI(tmpRect);
		}
		if(m_selM == c_paramId && m_open)
		{
			tmpRect = m_paramRect;
			tmpRect.y += m_btnH;
			OnUI |= PC.In.CursorOnUI(tmpRect);
		}
        return PC.In.CursorOnUIs(m_headerRect,new Rect(0,Screen.height-m_btnH,Screen.width,m_btnH)) || OnUI || m_askConfirmation;
	}
	
	protected void Validate ()
	{		
		GetComponent<HelpPanel>().enabled = true;
		_renderCamera.GetComponent<Mode2D>().enabled = true;
		
		if (_polygon.GetPoints ().Count > 0)
		{
			// generation de la piscine
			StartCoroutine (_function.Generate (_polygon));
		}
		else
		{
            _function.transform.parent = null;
			Destroy(_function.gameObject);
		}
		
		GetComponent<GUIMenuLeft> ().enabled = true;
		GetComponent<GUIMenuLeft> ().setVisibility (false);
		
		GetComponent<GUIMenuInteraction> ().unConfigure();
		GetComponent<GUIMenuInteraction> ().setVisibility (false);
		
		_renderCamera.GetComponent<ObjInteraction>().configuringObj(null);
		_renderCamera.GetComponent<ObjInteraction>().setSelected(null,false);
		_renderCamera.GetComponent<GuiTextureClip>().enabled = true;
		
		enabled = false;
		
		_planCamera.enabled = false;
		_snapper.enabled = false;
		_polyDrawer.enabled = false;
		_planTransformation.enabled = false;
		
		leftOff7 = -320;
		leftRect.x = leftOff7;
		showLeftMenu = false;
		
		if (bgImgMode)
		{
			showBgImgMenu = false;	
		}

        // -- Rétablir photo --
		_backgroundImg.GetComponent<BgImgManager>().ReinitBackgroundTex();

#if UNITY_IPHONE && !UNITY_EDITOR
		EtceteraManager.imagePickerChoseImageEvent -= OpenFileIpad;
		EtceteraManager.imagePickerCancelledEvent -= OpenFileIpadFailed;
#endif	
#if UNITY_ANDROID && !UNITY_EDITOR		
		EtceteraAndroidManager.albumChooserSucceededEvent -= OpenFileAndroid;
		EtceteraAndroidManager.albumChooserCancelledEvent -= OpenFileAndroidFailed;
#endif	
	}	
	
	private void DeleteSelectedPoint()
	{
		_polyDrawer.ResetSelection ();
		_polygon.RemovePoint (selectedPointIndex);
		
		if (_polygon.GetPoints ().Count < 3)
		{
			_polygon.SetClosed (false);
		}
	}
	
	private void SuppressPlan()
	{
		Texture2D bgImg = _function.GetBackgroundImage ();
		if (bgImg != null)
		{
			_function.SetBackgroundImage (bgImg);
			Destroy (bgImg);	
		}	
	}
	
	protected void Reset ()
	{
		_polyDrawer.Clear ();
		_polygon = new Polygon (_function.GetPointsDataBackup ());
		
		_polyDrawer.SetPolygon (_polygon);
		_snapper.SetPolygon (_polygon);
		
		_mode = PoolDesignerMode.PolygonEdition;
		m_open = false;
		m_subMenuSelected = true;
		
		//_planTransformation.ZoomToPolygon (_polygon);
	}
	
	protected void Clear ()
	{
		_polyDrawer.Clear ();
	}
	
	protected void ValidateBackground ()
	{
		float scaleFactor = 100 / _backgroundEdge.GetLength ();

		float width = UnityEngine.Screen.width * scaleFactor;
		float height = UnityEngine.Screen.height * scaleFactor;
		
		float x = (UnityEngine.Screen.width / 2 - width / 2);
		float y = (UnityEngine.Screen.height / 2 - height / 2);
		
		// on place l'image au milieu de la zone de travail
		// en fonction de la transformation du plan courante
		Matrix4x4 m = _planTransformation.GetMatrix ().inverse;
		
		Vector2 upRightCorner = m.MultiplyPoint (new Vector2 (x + width, y));
//		Vector2 upLeftCorner = m.MultiplyPoint (new Vector2 (x - width, y));
		Vector2 bottomLeftCorner = m.MultiplyPoint (new Vector2 (x, y + height));
		Vector2 rectPosition = m.MultiplyPoint (new Vector2 (x, y));
		
		float xOffset = (upRightCorner.x - rectPosition.x) / 2;
		float yOffset = (bottomLeftCorner.y - rectPosition.y) / 2;
		
		/*Rect backgroundRect = new Rect (rectPosition.x + xOffset - width / 2, 
			                            rectPosition.y + yOffset - height / 2, 
			                            width, height);*/
		
		Rect backgroundRect = new Rect (0, 
			                            0, 
			                            width, height);

		_function.SetBackgroundRect (backgroundRect);
		_function.SetBackgroundImageVisible (true);
//		_showHideBackgroundImage.SetValue (true);
		
		_mode = PoolDesignerMode.PolygonEdition;
		
		m_selM = 0;
		m_subMenuSelected = true;
		m_open = false;
		
		bgImgMode = true;
		canDisp = true;		
		_planTransformation.ZoomToImage(backgroundRect,_polyDrawer.GetOffSet(),1.0f/scaleFactor);
	}
	
	public void SetSliderValues (float currentValue, float maxValue)
	{		
//		_curveSlider.SetValue (currentValue);
//		_curveSlider.SetMaxValue (maxValue);
		sldVal = currentValue;
		sldMax = maxValue;
		tmpStr = sldVal.ToString("F2");
	}
	
	public PoolDesignerMode GetCurrentMode ()
	{
		return _mode;	
	}
	
	public void SetCurrentMode (PoolDesignerMode mode)
	{
		_mode = mode;	
	}
	
	public Function_PoolDesigner GetFunction ()
	{
		return _function;	
	}
	
	public bool IsOpen()
	{
		return (m_drawRect.y > -m_drawMenus.Length* m_btnH);
	}
	
	public bool IsJustCloseMenu()
	{
		return _justCloseMenu;
	}
	public void setIsJustCloseMenu(bool state)
	{
		_justCloseMenu = state;
	}
	#region bg loading
	private void loadImage()
	{
		#if UNITY_IPHONE
			EtceteraManager.setPrompt(true);
			EtceteraBinding.promptForPhoto( 0.5f, PhotoPromptType.Album );
		#elif  UNITY_ANDROID
            OpenFileAndroidDevice();
        #else
			OpenFileWin();
		#endif
	}
	
	private void OpenFileWin ()
	{
		string sOpenPath = "";
#if UNITY_EDITOR
		Debug.Log("UNITY_EDITOR : ");
    	sOpenPath = UnityEditor.EditorUtility.OpenFilePanel("Open Session","","png");
		Debug.Log("Path0 : "+sOpenPath);
		if(sOpenPath != "")
			StartCoroutine (ContinueOpenFileWin(sOpenPath));
#endif
#if (UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN) && !UNITY_EDITOR
		string[] extensions = new string[] {"jpeg", "jpg", "png"};
        sOpenPath = NativePanels.OpenFileDialog(extensions);
    
		if(sOpenPath.Length < 4){
            if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log("open process has ben canceled or has failed");
        }else{
            if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log("Path0 : "+sOpenPath);
            if(sOpenPath != "")
                StartCoroutine (ContinueOpenFileWin(sOpenPath));
        }
#endif
	}
	IEnumerator ContinueOpenFileWin(string s)
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
			Texture2D backgroundLoaded = new Texture2D(1,1);
			www.LoadImageIntoTexture (backgroundLoaded);
			www.Dispose ();
			
			_function.SetBackgroundImage (backgroundLoaded);
			
			showLeftMenu = false;
			
			if (bgImgMode)
			{
				showBgImgMenu = false;	
			}
			showRightMenu = false;
			canDisp = false;
			
			_mode = PoolDesignerMode.BackgroundScale;
			
			_backgroundEdge.GetPrevPoint2 ().Set (UnityEngine.Screen.width / 2 - 50, UnityEngine.Screen.height / 2);
			_backgroundEdge.GetNextPoint2 ().Set (UnityEngine.Screen.width / 2 + 50, UnityEngine.Screen.height / 2);
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			SetZoomTextureDimension ();
#endif
			yield return true;
		}
	}
	
	private void OpenFileIpad(string path)
	{
#if UNITY_IPHONE
		if(GameObject.Find("MainScene").GetComponent<GUIStart>().enabled == true)
			return;
		if( path == null )
		{
			OpenFileIpadFailed("Path null");
			return;
		}
		iPadPath = path;
		StartCoroutine(EtceteraManager.textureFromFileAtPath( "file://" + path, ContinueOpenFileiPad, OpenFileIpadFailed ) );
#endif
	}
	private void ContinueOpenFileiPad(Texture2D texture)
	{
#if UNITY_IPHONE && !UNITY_EDITOR	
		_function.SetBackgroundImage (texture);
			
		showLeftMenu = false;
			
		if (bgImgMode)
		{
			showBgImgMenu = false;	
		}
		showRightMenu = false;
		canDisp = false;
		
		_mode = PoolDesignerMode.BackgroundScale;
		
		_backgroundEdge.GetPrevPoint2 ().Set (UnityEngine.Screen.width / 2 - 50, UnityEngine.Screen.height / 2);
		_backgroundEdge.GetNextPoint2 ().Set (UnityEngine.Screen.width / 2 + 50, UnityEngine.Screen.height / 2);
//#if UNITY_IPHONE && !UNITY_EDITOR		
		SetZoomTextureDimension ();
#endif
		
		if(System.IO.File.Exists(iPadPath))
		{
			Debug.Log("IMG STILL EXISTS: Deleting ...");
			System.IO.File.Delete(iPadPath);
		}
	}
	
	private void OpenFileIpadFailed()
	{
		Debug.Log("OPEN FILE FAILED");
	}
	private void OpenFileIpadFailed(string error)
	{
		Debug.Log("OPEN FILE FAILED >"+error);
	}
		
    //-----------------------------------------------------
    private void OpenFileAndroidDevice()
    {
#if UNITY_ANDROID
		EtceteraAndroid.promptForPictureFromAlbum(UnityEngine.Screen.width,UnityEngine.Screen.height,name);
#endif
    }	
    //-----------------------------------------------------
    private void OpenFileAndroid(string path, Texture2D texture)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GameObject.Find("MainScene").GetComponent<GUIStart>().enabled == true)
			return;
		if( path == null )
		{
            OpenFileAndroidFailed("Path null");
			return;
		}
		_function.SetBackgroundImage (texture);			
		showLeftMenu = false;			
		if (bgImgMode)
		{
			showBgImgMenu = false;	
		}
		showRightMenu = false;
		canDisp = false;		
		_mode = PoolDesignerMode.BackgroundScale;
		
		_backgroundEdge.GetPrevPoint2 ().Set (UnityEngine.Screen.width / 2 - 50, UnityEngine.Screen.height / 2);
		_backgroundEdge.GetNextPoint2 ().Set (UnityEngine.Screen.width / 2 + 50, UnityEngine.Screen.height / 2);	
		SetZoomTextureDimension ();		
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
}
