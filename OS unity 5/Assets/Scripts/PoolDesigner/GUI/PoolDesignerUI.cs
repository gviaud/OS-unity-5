using UnityEngine;
using System.Collections;
using Pointcube.Global;

//if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//using System.Windows.Forms;
//endif

//public enum PoolDesignerMode
//{
//	PolygonEdition,
//	PlanTransformation,
//	BackgroundScale,
//	BackgroundTranslation
//}

public class PoolDesignerUI : MonoBehaviour, FunctionUI_OS3D, GUIInterface
{
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
	
	protected GUIItemV2 LeftRoot;
	protected GUIItemV2 RightRoot;
	protected GUIItemV2 bgImgRoot;
	protected GUIItemV2 LeftTitle;
	protected GUIItemV2 RightTitle;
	protected GUIItemV2 bgImgTitle;
	protected GUIItemV2 activeItem;
	
	protected GUIHSlider _curveSlider;
	protected GUICheckbox _alignedSnapping;
	protected GUICheckbox _angleSnapping;
	protected GUICheckbox _showHideBackgroundImage;
	protected GUICheckbox _squareCotation; // checkbox cotation globales
	protected GUICheckbox _edgeCotation; // checkbox cotation par segment
	
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
	
	private GameObject _iosShadows;

    // -- DEBUG --
    private static readonly string DEBUGTAG = "PoolDesignerUI : ";   
    private static readonly bool   DEBUG    = true;

	// Use this for initialization
	void Start () 
	{
		
		_iosShadows = GameObject.Find("iosShadows");
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
			
			float curveRadius = _curveSlider.GetValue () * 100;
			
			if (selectedPoint != null &&  selectedPoint.GetJunction () == JunctionType.Broken)
			{
				if (curveRadius > 0 && selectedPoint.GetPrevEdge () != null && selectedPoint.GetNextEdge () != null)
				{
					_polygon.SetJunctionType (selectedPointIndex, JunctionType.Curved);
					ArchedPoint2 ar = _polygon.GetPoints ()[selectedPointIndex] as ArchedPoint2;
					_curveSlider.SetMaxValue (ar.GetMaxRadius () / 100);
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
					
					if (!Utils.Approximately (maxRadius, _curveSlider.GetMaxValue () / 100))
					{
						_curveSlider.SetMaxValue (maxRadius);
					}
				}
				else
				{
					_polygon.SetJunctionType (selectedPointIndex, JunctionType.Broken);
				}
			}	
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		panelAnimation ();
		
		if(showLeftMenu)//version IPAD
		{
			antiSlideAndSelect();
//			if(Input.touchCount>0 && !isOnUI())
//			{
//				switch (Input.touches[0].phase)
//				{
//				case TouchPhase.Began:
//					canHideMenu = true;
//					break;
//				case TouchPhase.Moved:
//					canHideMenu = false;
//					break;
//				case TouchPhase.Ended:
//					if(canHideMenu)
//					{
//						showLeftMenu = false;
//
//						if (bgImgMode)
//						{
//							showBgImgMenu = false;
//						}
//
//						if(isJustShowing)
//						{
//							isJustShowing = false;
//						}
//
//						canHideMenu = false;
//					}
//					break;
//				}
//			}
//
//			if(UnityEngine.Application.platform != RuntimePlatform.IPhonePlayer)
//			{
				if(PC.In.Click1Down() && !isOnUI() )
				{
					showLeftMenu = false;
					canHideMenu = false;
				}
//			}
		}
		
		UpdateCurveSliderValue ();

//#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR
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
//#endif
		
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//		if (Input.touchCount == 1)
//		{
//			Touch touch = Input.touches[0];
//			
//			// gestion des points de reference permettant la mise à l'echelle
//			// de l'image de fond
//			if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
//			{
//				_movingBgPoint = null;
//			}
//
//			if (_movingBgPoint != null)
//			{
//				Vector2 newPosition = new Vector2 (touch.position.x, 
//					                               UnityEngine.Screen.height - touch.position.y) - _movingBgPointOffset;
//				_movingBgPoint.Set (newPosition);	
//			}
//			
//			// gestion de la translation de l'image de fond
//			if (_mode == PoolDesignerMode.BackgroundTranslation)
//			{
//				if (touch.phase == TouchPhase.Began)
//				{
//					prevMousePosition = new Vector2 (touch.position.x, UnityEngine.Screen.height - touch.position.y);
//				}
//				
//				if (touch.phase == TouchPhase.Moved)
//				{
//					float scale = _planTransformation.GetScale ().x;
//					
//					Vector2 mousePosition = new Vector2 (touch.position.x, UnityEngine.Screen.height - touch.position.y);
//					Vector2 translation = mousePosition - prevMousePosition;
//					
//					Rect backgroundRect = _function.GetBackgroundRect ();
//					backgroundRect.x = backgroundRect.x + translation.x / scale;
//					backgroundRect.y = backgroundRect.y + translation.y / scale;
//	
//					_function.SetBackgroundRect (backgroundRect);
//					
//					prevMousePosition = mousePosition;
//				}
//			}
//		}

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
	void LateUpdate ()
	{			
		if(showRightMenu)//version IPAD
		{
			antiSlideAndSelect();
//			if(Input.touchCount>0 && !isOnUI())
//			{
//				switch (Input.touches[0].phase)
//				{
//				case TouchPhase.Began:
//					canHideMenu = true;
//					break;
//				case TouchPhase.Moved:
//					canHideMenu = false;
//					break;
//				case TouchPhase.Ended:
//					if(canHideMenu && !isOnUI() && _polyDrawer.GetSelectedPointIndex () < 0 ||
//					   canHideMenu && _mode == PoolDesignerMode.PlanTransformation)
//					{
//						showRightMenu = false;
//						if(isJustShowing)
//						{
//							isJustShowing = false;
//						}
//
//						canHideMenu = false;
//					}
//					break;
//				}
//			}
//
//			if(UnityEngine.Application.platform != RuntimePlatform.IPhonePlayer)
//			{
				if(PC.In.Click1Down() && !isOnUI() && _polyDrawer.GetSelectedPointIndex () < 0 ||
					_mode == PoolDesignerMode.PlanTransformation || !_polygon.IsClosed ())
				{
					showRightMenu = false;
					canHideMenu = false;
				}
//			}
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
			LeftRoot.setSelected (0);
			showBgImgMenu = false;
		}
			
		if (selectEditMode)
		{
			selectEditMode = false;
			LeftRoot.setSelected (0);
		}
	}
	
	protected void panelAnimation ()
	{
		if(showLeftMenu)
		{
			if(leftOff7<30)
			{
				if(leftOff7>29)
				{
					leftOff7 = 30;
					leftMenuGroup.x = leftOff7-30;
					leftRect.x = leftOff7-30;
				}
				else
				{
					leftOff7 = Mathf.Lerp(leftOff7,30,5*Time.deltaTime);
					leftMenuGroup.x = leftOff7-30;
					leftRect.x = leftOff7-30;
				}
			}
		}
		else
		{
			if(leftOff7>-320)
			{
				if(leftOff7<-319)
				{
					leftOff7 = -320;
					leftMenuGroup.x = leftOff7;
					leftRect.x = leftOff7;
					//resetMenu();
				}
				else
				{
					leftOff7 = Mathf.Lerp(leftOff7,-320,5*Time.deltaTime);
					leftMenuGroup.x = leftOff7;
					leftRect.x = leftOff7;
				}
			}
		}
		
		if(showRightMenu)
		{
			float limit = UnityEngine.Screen.width-290;
			
			if(rightOff7>limit)
			{
				if(rightOff7<limit-1)
				{
					rightOff7 = limit;
					rightMenuGroup.x = rightOff7;
					rightRect.x = rightOff7 + 30;
				}
				else
				{
					rightOff7 = Mathf.Lerp(rightOff7,limit,5*Time.deltaTime);
					rightMenuGroup.x = rightOff7;
					rightRect.x = rightOff7 + 30;
				}
			}
		}
		else
		{
			if(rightOff7<UnityEngine.Screen.width)
			{
				if(rightOff7>(UnityEngine.Screen.width-1))
				{
					rightOff7 = UnityEngine.Screen.width;
					rightMenuGroup.x = rightOff7;
					rightRect.x = rightOff7;
				}
				else
				{
					rightOff7 = Mathf.Lerp(rightOff7,UnityEngine.Screen.width,5*Time.deltaTime);
					rightMenuGroup.x = rightOff7;
					rightRect.x = rightOff7;
				}
			}
		}
		
		if(showBgImgMenu)
		{
			float limit = UnityEngine.Screen.width-290;
			
			if(bgImgOff7>limit)
			{
				if(bgImgOff7<limit-1)
				{
					bgImgOff7 = limit;
					bgImgMenuGroup.x = bgImgOff7;
					bgImgRect.x = bgImgOff7 + 10;
				}
				else
				{
					bgImgOff7 = Mathf.Lerp(bgImgOff7,limit,5*Time.deltaTime);
					bgImgMenuGroup.x = bgImgOff7;
					bgImgRect.x = bgImgOff7 + 10;
				}
			}
		}
		else
		{
			if(bgImgOff7<UnityEngine.Screen.width)
			{
				if(bgImgOff7>(UnityEngine.Screen.width-1))
				{
					bgImgOff7 = UnityEngine.Screen.width;
					bgImgMenuGroup.x = bgImgOff7;
					bgImgRect.x = bgImgOff7;
				}
				else
				{
					bgImgOff7 = Mathf.Lerp(bgImgOff7,UnityEngine.Screen.width,5*Time.deltaTime);
					bgImgMenuGroup.x = bgImgOff7;
					bgImgRect.x = bgImgOff7;
				}
			}
		}
	}
	
	protected void ui2fcn ()
	{
		
	}
	
	protected void antiSlideAndSelect()
	{
//		if(Input.touchCount>0)
//		{
//			Touch t = Input.touches[0];
//			if(t.phase == TouchPhase.Moved)
//			{
//				_isSliding = true;
//			}
//			else if(t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended)
//			{
//				_isSliding = false;
//			}
//		}
//		else
        if(_isSliding == true)
		{
			_isSliding = false;
		}
	}
	
	void OnGUI ()
	{
		GUI.depth = -11;
			
		GUISkin bkup = GUI.skin;
		GUI.skin = skin;
		
		if (_polyDrawer.enabled)
		{
			//MENU LEFT
			if((showLeftMenu || leftOff7 < UnityEngine.Screen.width) && canDisp)
			{
				GUILayout.BeginArea(new Rect(leftOff7, 0, 300, UnityEngine.Screen.height));
			    GUILayout.FlexibleSpace();
			
				scrollpos = GUILayout.BeginScrollView(scrollpos,"empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
				
				GUILayout.Box("","leftBgFadeUp",GUILayout.Width(260),GUILayout.Height(150));//fade en haut
				GUILayout.BeginVertical("leftBgFadeMid",GUILayout.Width(260));
				
				LeftTitle.getUI(false);
				LeftRoot.showSubItms();//Menu
				
				GUILayout.EndVertical();
				GUILayout.Box("","leftBgFadeDw",GUILayout.Width(260),GUILayout.Height(150));//fade en bas
				
				GUILayout.EndScrollView();
				
			    GUILayout.FlexibleSpace();
			    GUILayout.EndArea();
			}
			
			//MENU RIGHT
			if((showRightMenu || rightOff7<UnityEngine.Screen.width) && canDisp)
			{
				GUILayout.BeginArea(new Rect(rightOff7, 0, 300, UnityEngine.Screen.height));
			    GUILayout.FlexibleSpace();
			
				scrollpos = GUILayout.BeginScrollView(scrollpos,"empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
				
				GUILayout.Box("","rightBgFadeUp",GUILayout.Width(260),GUILayout.Height(150));//fade en haut
				GUILayout.BeginVertical("rightBgFadeMid",GUILayout.Width(260));
				
				RightTitle.getUI(false);
				RightRoot.showSubItms();//Menu
				
				GUILayout.EndVertical();
				GUILayout.Box("","rightBgFadeDw",GUILayout.Width(260),GUILayout.Height(150));//fade en bas
				
				GUILayout.EndScrollView();
				
			    GUILayout.FlexibleSpace();
			    GUILayout.EndArea();
			}
			
			if((showBgImgMenu || bgImgOff7<UnityEngine.Screen.width) && canDisp)
			{
				GUILayout.BeginArea(new Rect(bgImgOff7, 0, 300, UnityEngine.Screen.height));
			    GUILayout.FlexibleSpace();
			
				scrollpos = GUILayout.BeginScrollView(scrollpos,"empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
				
				GUILayout.Box("","rightBgFadeUp",GUILayout.Width(260),GUILayout.Height(150));//fade en haut
				GUILayout.BeginVertical("rightBgFadeMid",GUILayout.Width(260));
				
				bgImgTitle.getUI(false);
				bgImgRoot.showSubItms();//Menu
				
				GUILayout.EndVertical();
				GUILayout.Box("","rightBgFadeDw",GUILayout.Width(260),GUILayout.Height(150));//fade en bas
				
				GUILayout.EndScrollView();
				
			    GUILayout.FlexibleSpace();
			    GUILayout.EndArea();
			}
			
			//LEFT MenuToggle
			if(showLeftMenu)
			{
				//BTN SHOW HIDE MENU
				showLeftMenu = GUI.Toggle(new Rect(leftOff7-30,UnityEngine.Screen.height/2-350/2,30,350),showLeftMenu,"","leftHalfMenuToggle");
				
				if (bgImgMode)
				{
					showBgImgMenu = showLeftMenu;	
				}
			}
			else
			{
				if(canDisp)
				{
					//BTN SHOW HIDE MENU
					showLeftMenu = GUI.Toggle(new Rect(0,UnityEngine.Screen.height/2-50,100,100),showLeftMenu,"","leftMenuToggle");
					
					if (bgImgMode)
					{
						showBgImgMenu = showLeftMenu;	
					}
				}
			}
			
			//Menu Toggle
			if(showRightMenu)
			{
				//BTN SHOW HIDE MENU
				showRightMenu = GUI.Toggle(new Rect(rightOff7+260,UnityEngine.Screen.height/2-350/2,30,350),showRightMenu,"","rightHalfMenuToggle");
			}
			else
			{
				if(canDisp && _mode != PoolDesignerMode.PlanTransformation && !bgImgMode)
				{
					//BTN SHOW HIDE MENU
					showRightMenu = GUI.Toggle(new Rect(UnityEngine.Screen.width-100,UnityEngine.Screen.height/2-50,100,100),showRightMenu,"","rightMenuToggle");
				}
			}
		}
		
		if (_mode == PoolDesignerMode.BackgroundScale && _function.GetBackgroundImage () != null)
		{		
			// dessin de l'image de fond en mode taille max par rapport a l'ecran
			// pour regler l'echelle
			Rect backgroundRect = new Rect (0, 0, UnityEngine.Screen.width, UnityEngine.Screen.height);
			GUI.DrawTexture (backgroundRect, _function.GetBackgroundImage (), ScaleMode.ScaleToFit);
			
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
//#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR
//				_movingBgPointOffset = new Vector2 (Input.mousePosition.x, UnityEngine.Screen.height - Input.mousePosition.y) - prevPtV;
//#elif (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//				if (Input.touchCount == 1)
//					_movingBgPointOffset = new Vector2 (Input.touches[0].position.x, UnityEngine.Screen.height - Input.touches[0].position.y) - prevPtV;
//#endif
			}
			
			if (GUI.RepeatButton (new Rect (nextPt.GetX () - 25, nextPt.GetY () - 25, 50, 50), "", "viewfinder"))
			{
				_movingBgPoint = nextPt;
                _movingBgPointOffset = PC.In.GetCursorPosInvY() - nextPtV;
//#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR
//				_movingBgPointOffset = new Vector2 (Input.mousePosition.x, UnityEngine.Screen.height - Input.mousePosition.y) - nextPtV;
//#elif (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//				if (Input.touchCount == 1)
//					_movingBgPointOffset = new Vector2 (Input.touches[0].position.x, UnityEngine.Screen.height - Input.touches[0].position.y) - nextPtV;
//#endif
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
			
			GUI.matrix = m;
			
			if (_movingBgPoint == null)
			{		
				// dessin de bouton de validation en fonction de la position des points de references
				// de reglage de l'echelle
//				float buttonOffset = 20;
//				float buttonWidth = 100;
//				float buttonHeight = 50;
//				
//				Vector2 halfPoint = prevPtV + (nextPtV - prevPtV).normalized * edgeLength / 2;
//				
//				float x;
//				float y;
//				
//				if (halfPoint.x > UnityEngine.Screen.width / 2)
//				{
//					x = prevPtV.x < nextPtV.x ? prevPtV.x - buttonWidth - buttonOffset : 
//						                        nextPtV.x - buttonWidth - buttonOffset;
//				}
//				else
//				{
//					x = prevPtV.x > nextPtV.x ? prevPtV.x + buttonOffset : nextPtV.x + buttonOffset;
//				}
//				
//				if (halfPoint.y > UnityEngine.Screen.height / 2)
//				{
//					y = prevPtV.y < nextPtV.y ? prevPtV.y - buttonHeight - buttonOffset : 
//						                        nextPtV.y - buttonHeight - buttonOffset;
//				}
//				else
//				{
//					y = prevPtV.y > nextPtV.y ? prevPtV.y + buttonOffset : nextPtV.y + buttonOffset;
//				}
				
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
		}
		
		GUI.skin = bkup;
		
//		GUI.Button (leftRect, "");
//		GUI.Button (rightRect, "");
//		GUI.Button (bgImgRect, "");
//		GUI.Button (new Rect(0, UnityEngine.Screen.height / 2 - 50, 100, 100), "");
//		GUI.Button (new Rect(UnityEngine.Screen.width - 100, UnityEngine.Screen.height/2 - 50, 100, 100), "");
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
	}
	
	public void updateGUI(GUIItemV2 itm,int val,bool reset)
	{
		switch (itm.getDepth ())
		{
		case 0:	
			
			if (val == 2) // move plan mode
			{
				_mode = PoolDesignerMode.PlanTransformation;
				bgImgMode = false;
			}
			else
			{
				if (val == -1 && lastUIValue != 10 && lastUIValue != 0)
				{
					selectEditMode = true;
				}
				
				_mode = PoolDesignerMode.PolygonEdition;
			}
			
			if (val == 4) // remove point
			{
				int selectedPointIndex = _polyDrawer.GetSelectedPointIndex ();
				
				if (selectedPointIndex >= 0)
				{
					_polyDrawer.ResetSelection ();
					_polygon.RemovePoint (selectedPointIndex);
					
					if (_polygon.GetPoints ().Count < 3)
					{
						_polygon.SetClosed (false);	
					}
				}
				
				RightRoot.resetSelected ();
			}
			
			if (val == 5) // affichage du menu image
			{
				bgImgMode = true;
				showBgImgMenu = true;
				showRightMenu = false;
			}
			else if (val < 7 && (val != -1 || lastUIValue != 10))
			{
				bgImgMode = false;
				showBgImgMenu = false;
				bgImgRoot.resetSelected ();
			}
			
			if (val == 7) // chargement de l'image
			{
				loadImage ();	
			}
			
			if (itm == _showHideBackgroundImage) // afficher/masquer l'image
			{
				GUICheckbox ckbx = itm as GUICheckbox;
				_function.SetBackgroundImageVisible (ckbx.GetValue ());
			}
			
			if (val == 9) // regler l'echelle de l'image
			{
				showLeftMenu = false;
			
				if (bgImgMode)
				{
					showBgImgMenu = false;	
				}
				showRightMenu = false;
				canDisp = false;
				
				_mode = PoolDesignerMode.BackgroundScale;
			}
			
			if (val == 10) // translation de l'image
			{
				_mode = PoolDesignerMode.BackgroundTranslation;
			}
			
			if (val == 11) // suppression de l'image
			{
				Texture2D bgImg = _function.GetBackgroundImage ();
				
				if (bgImg != null)
				{
					_function.SetBackgroundImage (bgImg);
					Destroy (bgImg);	
				}
			}
			
			if (val > 6 && val != 10)
			{
				bgImgRoot.resetSelected ();	
			}

			break;
			
		case 1:
			if (itm == _alignedSnapping) // snap
			{
				GUICheckbox ckbx = itm as GUICheckbox;
				_snapper.SetAlignedPointSnapActive (ckbx.GetValue ());
			}
			
//			if (itm == _angleSnapping)
//			{
//				GUICheckbox ckbx = itm as GUICheckbox;
//			}
			
			/*if (val == 2)
			{
				_planTransformation.ZoomToPolygon (_polygon);
				((GUIItemV2)LeftRoot.getSubItems ()[2]).resetSelected ();

			}*/
			
			if (itm == _squareCotation) // cotation global
			{
				GUICheckbox ckbx = itm as GUICheckbox;
				_polyDrawer.SetMaxDimensionVisible  (ckbx.GetValue ());
				val++;
				
			}
			
			if (itm == _edgeCotation) // cotation par segment
			{
				GUICheckbox ckbx = itm as GUICheckbox;
				_polyDrawer.SetCotationVisible (ckbx.GetValue ());
				val++;
			}
			
			bgImgMode = false;
			showBgImgMenu = false;
			
			break;
		}
		
		lastUIValue = val;
		
		if(!_isSliding)
			ui2fcn ();
	}
	
	public void setVisibility(bool b)
	{
		
	}
	
	public void canDisplay(bool b)
	{
		canDisp = b;
	}
	
	public bool isOnUI() // TODO quick-fixé pour les nouveaux input. Pour bien fixer : tenir à jour le tableau des rects
	{                    // en temps réel (ie. quand on ouvre le menuRight, mettre le rectangle right dans le tableau,
                         // et quand on le ferme l'enlever).
        Rect[] uiRects = new Rect[3];
        uiRects[0] = (showLeftMenu ? leftRect : new Rect(0, UnityEngine.Screen.height / 2 - 50, 100, 100));
        uiRects[1] = (showRightMenu ? rightRect : new Rect(UnityEngine.Screen.width - 100, UnityEngine.Screen.height/2 - 50, 100, 100));
        uiRects[2] = (showBgImgMenu ? bgImgRect : new Rect(UnityEngine.Screen.width - 100, UnityEngine.Screen.height/2 - 50, 100, 100));

        return PC.In.CursorOnUI(uiRects);

//		Vector2 cursor = new Vector2 (-1,-1);
//		
//		if(UnityEngine.Application.platform != RuntimePlatform.IPhonePlayer)
//		{
//			cursor = Input.mousePosition;
//			cursor.y = UnityEngine.Screen.height - cursor.y;
//		}
//		else
//		{
//			if(Input.touchCount > 0)
//			{
//				Touch t = Input.touches[0];
//				cursor = t.position;
//				cursor.y = UnityEngine.Screen.height - cursor.y;
//			}
//			
//		}
//		
//		bool onLeftUI = false;
//		bool onRightUI = false;
//		bool onBgImgUI = false;
//		
//		if(showLeftMenu)
//		{
//			onLeftUI = leftRect.Contains(cursor);
//		}
//		else
//		{
//			Rect al = new Rect(0, UnityEngine.Screen.height / 2 - 50, 100, 100);
//			onLeftUI = al.Contains(cursor);
//		}
//		
//		if(showRightMenu)
//		{
//			onRightUI = rightRect.Contains (cursor);
//		}
//		else
//		{
//			Rect al = new Rect(UnityEngine.Screen.width - 100, UnityEngine.Screen.height/2 - 50, 100, 100);
//			onRightUI = al.Contains (cursor);
//		}
//		
//		if(showBgImgMenu)
//		{
//			onBgImgUI = bgImgRect.Contains (cursor);
//		}
//		else
//		{
//			Rect al = new Rect(UnityEngine.Screen.width - 100, UnityEngine.Screen.height/2 - 50, 100, 100);
//			onBgImgUI = al.Contains (cursor);
//		}
//		
//		return onLeftUI || onRightUI || onBgImgUI;
	}
	
	public bool isVisible()
	{
		return showLeftMenu;
	}
	
	public void CreateGui()
	{
		// menu niveau
		GUIItemV2 edition = new GUIItemV2 (0, 0, TextManager.GetText("PoolDesigner.Edit"), "leftOutilOn", "leftOutilOff", this, true);
		GUIItemV2 snap = new GUIItemV2 (0, 1, TextManager.GetText("PoolDesigner.Snap"), "leftOutilOn", "leftOutilOff", this, true);
		GUIItemV2 plan = new GUIItemV2 (0, 2, TextManager.GetText("PoolDesigner.Plan"), "leftOutilOn", "leftOutilOff", this, true);
		GUIItemV2 img = new GUIItemV2 (0, 5, TextManager.GetText("PoolDesigner.Image"), "leftOutilOn", "leftOutilOff", this, true);
		GUIItemV2 cotation = new GUIItemV2 (0, 6, TextManager.GetText("PoolDesigner.Cotation"), "leftOutilOn", "leftOutilOff", this, true);
		
		// snap
		_alignedSnapping = new GUICheckbox (1, 0, TextManager.GetText("PoolDesigner.AlignedSnap"), "leftOutilOn", "leftOutilOff", this);
		_alignedSnapping.SetValue (true);
		
		_angleSnapping = new GUICheckbox (1, 1, TextManager.GetText("PoolDesigner.AngleSnap"), "leftOutilOn", "leftOutilOff", this);
		
		snap.addSubItem (_alignedSnapping);
		//snap.addSubItem (_angleSnapping);
		
		// cotation
		_squareCotation = new GUICheckbox (1, 2, TextManager.GetText("PoolDesigner.SquareCotation"), "leftOutilOn", "leftOutilOff", this);
		
		_edgeCotation = new GUICheckbox (1, 3, TextManager.GetText("PoolDesigner.EdgeCotation"), "leftOutilOn", "leftOutilOff", this);
		_edgeCotation.SetValue (true);
		
		cotation.addSubItem (_squareCotation);
		cotation.addSubItem (_edgeCotation);
		
//		GUIItemV2 zoomToPoly = new GUIItemV2 (1, 2, TextManager.GetText("PoolDesigner.ZoomToPoly"), "leftOutilOn", "leftOutilOff", this);
		//plan.addSubItem (zoomToPoly);

		GUIItemMultiBtn validCancelReset = new GUIItemMultiBtn (0, "multi_bg", "", "", this, 20, true);
		
		GUIAutoItem clear = new GUIAutoItem (1, "", "clear", "clear", this);
		clear.doEvent += Clear;
		validCancelReset.addSubItem (clear);
		
		GUIAutoItem reset = new GUIAutoItem (1, "", "reset", "reset", this);
		reset.doEvent += Reset;
		validCancelReset.addSubItem (reset);
		
		GUIAutoItem valid = new GUIAutoItem (1, "", "valid", "valid", this);
		valid.doEvent += Validate;
		validCancelReset.addSubItem (valid);
		
		_curveSlider = new GUIHSlider (0, 3, "rightOutilOn", "rightOutilOff", this);
		GUIItemV2 delete = new GUIItemV2 (0, 4, TextManager.GetText("PoolDesigner.RemovePoint"), "rightOutilOn", "rightOutilOff", this);
		
		// menu image niveaux 0
		GUIItemV2 load = new GUIItemV2 (0, 7, TextManager.GetText("PoolDesigner.Image.Load"), "rightOutilOn", "rightOutilOff", this);
		_showHideBackgroundImage = new GUICheckbox (0, 8, TextManager.GetText("PoolDesigner.Image.ShowHide"), "rightOutilOn", "rightOutilOff", this);
		_showHideBackgroundImage.SetValue (true);
		GUIItemV2 scale = new GUIItemV2 (0, 9, TextManager.GetText("PoolDesigner.Image.Scale"), "rightOutilOn", "rightOutilOff", this);
		GUIItemV2 position = new GUIItemV2 (0, 10, TextManager.GetText("PoolDesigner.Image.Move"), "rightOutilOn", "rightOutilOff", this);
		GUIItemV2 removeImg = new GUIItemV2 (0, 11, TextManager.GetText("PoolDesigner.Image.Remove"), "rightOutilOn", "rightOutilOff", this);
			
		LeftRoot = new GUIItemV2 (-1, -1, "Root", "", "", this);
		LeftRoot.addSubItem (edition);
		LeftRoot.addSubItem (snap);
		LeftRoot.addSubItem (plan);
		LeftRoot.addSubItem (img);
		LeftRoot.addSubItem (cotation);
		LeftRoot.addSubItem (validCancelReset);
		
		RightRoot = new GUIItemV2 (-1, -1, "Root", "", "", this);
		RightRoot.addSubItem (_curveSlider);
		RightRoot.addSubItem (delete);
		
		bgImgRoot = new GUIItemV2 (-1, -1, "Root", "", "", this);
		bgImgRoot.addSubItem (load);
		bgImgRoot.addSubItem (_showHideBackgroundImage);
		bgImgRoot.addSubItem (scale);
		bgImgRoot.addSubItem (position);
		bgImgRoot.addSubItem (removeImg);
		
		LeftTitle = new GUIItemV2 (-1, -1, TextManager.GetText("PoolDesigner.LeftTitle"), "leftTitle", "leftTitle", this);
		RightTitle = new GUIItemV2 (-1, -1, TextManager.GetText("PoolDesigner.RightTitle"), "rightTitle", "rightTitle", this);
		bgImgTitle = new GUIItemV2 (-1, -1, TextManager.GetText("PoolDesigner.Image"), "rightTitle", "rightTitle", this);
		
		leftMenuGroup = new Rect(leftOff7, 0, 300, UnityEngine.Screen.height);
		rightMenuGroup = new Rect(rightOff7, 0, 300, UnityEngine.Screen.height);
		bgImgMenuGroup = new Rect(bgImgOff7, 0, 300, UnityEngine.Screen.height);
		
		leftRect = new Rect(leftOff7, UnityEngine.Screen.height / 2 - 200, 260, 420);
		rightRect = new Rect(rightOff7, UnityEngine.Screen.height / 2 - 100, 260, 200);
		bgImgRect = new Rect(bgImgOff7, UnityEngine.Screen.height / 2 - 150, 280, 300);
		
		LeftRoot.setSelected (0);
		
		canDisp = true;
	}
	
	protected void Validate ()
	{		
	//	UnityEngine.Camera.mainCamera.enabled = true;
		//_renderCamera.enabled = true;		
		GetComponent<HelpPanel>().enabled = true;
		_renderCamera.GetComponent<Mode2D>().enabled = true;
		
		if (_polygon.GetPoints ().Count > 0)
		{
			// generation de la piscine
			StartCoroutine (_function.Generate (_polygon));
		}
		else
		{
			// si pas de point, on supprime le gameobject
			
     /*      	if (_iosShadows != null) 
			{
				_iosShadows.GetComponent<IosShadowManager>().DeleteShadow(_function.gameObject, false);
			}*/
			
            _function.transform.parent = null;
			Destroy(_function.gameObject);
			
			//GetComponent<GUIMenuLeft>().updateSceneObj();
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
	
	protected void Reset ()
	{
		_polyDrawer.Clear ();
		_polygon = new Polygon (_function.GetPointsDataBackup ());
		
		_polyDrawer.SetPolygon (_polygon);
		_snapper.SetPolygon (_polygon);
		
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
		
		Rect backgroundRect = new Rect (rectPosition.x + xOffset - width / 2, 
			                            rectPosition.y + yOffset - height / 2, 
			                            width, height);

		_function.SetBackgroundRect (backgroundRect);
		_function.SetBackgroundImageVisible (true);
		_showHideBackgroundImage.SetValue (true);
		
		_mode = PoolDesignerMode.PolygonEdition;
		bgImgMode = true;
		canDisp = true;		
		_planTransformation.ZoomToImage(backgroundRect,_polyDrawer.GetOffSet(),1.0f/scaleFactor);
	}
	
	public void SetSliderValues (float currentValue, float maxValue)
	{		
		_curveSlider.SetValue (currentValue);
		_curveSlider.SetMaxValue (maxValue);
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
//if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//		System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
//		openFileDialog.InitialDirectory = UnityEngine.Application.dataPath;
//		openFileDialog.Filter = "Image Files|*.jpg;*.png;*.jpeg|All Files|*.*";
//		openFileDialog.FilterIndex = 1;
//		openFileDialog.Title = "Open File";
//		
//		if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
//	    {
//			if ((openFileDialog.FileName!=""))
//	            {
//					sOpenPath = openFileDialog.FileName;
//					StartCoroutine (ContinueOpenFileWin(sOpenPath));
//	            }
//			else
//			{
//				//NOTHING
//			}
//	     }
//		else
//		{
//			//NOTHING
//		}
//			
//endif
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