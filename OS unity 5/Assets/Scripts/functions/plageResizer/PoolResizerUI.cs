using UnityEngine;
using System;
using System.Collections;

using Pointcube.Global;

public class PoolResizerUI : MonoBehaviour , FunctionUI_OS3D
{

	public Texture2D texBlue; 
	public Texture2D texRed; 
	public Texture2D texGreen; 
	public Texture2D texYellow;
	public Texture2D texWhite; 

	public Texture2D texBlue_A; 
	public Texture2D texRed_A;  
	public Texture2D texGreen_A; 
	public Texture2D texYellow_A; 
	public Texture2D texWhite_A;  
	
	Texture2D color;
	public Transform camRGB;
	public GameObject picker;	
	int selectionGrid = 0;
	bool m_ChangeColor = false;
	float[] tmpOff7Tab;
	public Color[] _colorTab;

	protected static string FORWARD = "SetForwardOffset";
	protected static string RIGHT = "SetRightOffset";
	protected static string LEFT = "SetLeftOffset";
	protected static string BACK = "SetBackOffset";
	protected static string DOWN = "SetDownOffset";
	
	// distance between arrows and sidewalk along y axis
	protected static float Y_ARROW = 0.3f;
	protected static float OFFSET_SPEED = 0.01f;
	protected static float HEIGHT_SPEED = 0.1f;
	protected /*static*/ float MIN_OFFSET = 0.0f;
	protected static float MAX_OFFSET = 5.0f;
	protected static float MAX_OFFSET_MURET = 5.0f;
	protected static float MIN_HEIGHT = -1.0f;
	protected static float MAX_HEIGHT = 0.0f;
	
	protected static Vector3 SCALE_START;
	protected static Vector3 SCALE_END = new Vector3(2.5f, 0.5f, 2.5f);
	
	protected int _selectedSideIndex = -1;
	protected float _selectedOffset;
	protected string _selectedSide;
	protected string _selectedSideRotationUV;
	protected string [] _commands = {"", "", "", "", ""};

	protected float _forwardOffset = 0.0f;
	protected float _rightOffset = 0.0f;
	protected float _leftOffset = 0.0f;
	protected float _backOffset = 0.0f;
	protected float _lowWallHeight = -1.0f;
	
	protected bool _hasOcclusion = false;
	
	// 3d arrows
	protected Transform _forwardArrow;
	protected Transform _rightArrow;
	protected Transform _leftArrow;
	protected Transform _backArrow;
	protected Transform _selectedArrow;
	
	protected GameObject _poolObject;
	
	protected AABBOutlineResizer _sideWalkResizer;
	protected AABBOutlineResizer _occlusionResizer;
	protected AABBOutlineResizer _lowWallResizer;
	
	protected Vector3 _occlusionInitialPosition;
	
	protected Vector2 _arrowProjDirection = Vector2.zero;
	protected bool _arrowProjDirty = false;
	
	// Prefab
	public Transform Arrow3DModel;
	
	//GUI
	public GUISkin skinPlage;
	public GUISkin skinMenuRight;
	public GUISkin skinMenuInteraction;

	public Texture textureBackGroundLarge;
	public Texture textureBackGroundLight;

//	private Rect guiZone;
	int sideIndex = -1;
	private bool _hasPool = false;
	private bool _tempHide = false;
	private float _uiOff7;

    private Rect  m_uiRect1;
    private Rect  m_uiRect2;

    private bool  m_clickOnUI;

	//indique si le doigts a bouge ou si simple clique
//	private bool _hasMoved = false;
	
	float tmpOff7 = 0;
	float tmpOff7RotationUV = 0;
	float tmpOff7TilePlage = 0;
	float tmpOff7TileMuret = 0;
	float tmpOff7TileMargelle = 0;


	protected static float MAX_OFFSET_ROTATION_UV = 3.15f;
	protected /*static*/ float MIN_OFFSET_ROTATION_UV = 0.0f;
	protected float _selectedOffset_RotationUV;

	protected static float MAX_OFFSET_TILE_PLAGE = 3.0f;
	protected /*static*/ float MIN_OFFSET_TILE_PLAGE = 0.1f;
	protected float _selectedOffset_TilePlage;

	protected static float MAX_OFFSET_TILE_MURET = 3.0f;
	protected /*static*/ float MIN_OFFSET_TILE_MURET = 0.1f;
	protected float _selectedOffset_TileMuret;

	protected static float MAX_OFFSET_TILE_MARGELLE = 3.0f;
	protected /*static*/ float MIN_OFFSET_TILE_MARGELLE = 0.1f;
	protected float _selectedOffset_TileMargelle;

	//-----------------------------------------------------
	private AABBOutlineResizer getResizer (string gameObjectName)
	{
		Transform go = _poolObject.transform.Find (gameObjectName);
			
		if (go == null)
			return null;
		
		AABBOutlineResizer resizer = go.GetComponent<AABBOutlineResizer> ();	

		return resizer;
	}

    //-----------------------------------------------------
    void Awake()
    {
        _uiOff7 = Screen.width;
        m_uiRect1 = new Rect(_uiOff7, 0, 300, 0f /*Screen.height*/);
        m_uiRect2 = new Rect(0,0,80,0f /*Screen.height*/);
        m_clickOnUI = false;
		tmpOff7Tab = new float[5];
		_colorTab = new Color[3];
		
	}
	
	//-----------------------------------------------------
    void OnEnable()
    {
        UsefullEvents.OnResizeWindowEnd += FitToScreen;
        UsefullEvents.OnResizingWindow  += FitToScreen;
        FitToScreen();

        _arrowProjDirty = true;
        GetComponent<GUIMenuMain>().hideShowUD(false);
    }

	//-----------------------------------------------------
	void Start () 
	{
//		guiZone = new Rect(Screen.width/2-750/2,0,750,50);
		enabled = false;
	}
	
    //-----------------------------------------------------
	void Update ()
	{	
		Rect tempRect = new Rect(Screen.width/2 -300,Screen.height/2-175, 600,350);
		if(
#if UNITY_IPHONE
			Input.touchCount > 0
#else
			Input.GetKey(KeyCode.Mouse0)
#endif		
			&&! tempRect.Contains(Input.mousePosition)
		){
			if (camRGB.gameObject.activeInHierarchy){
				validateColor();
			}
		}			
		if (_arrowProjDirty && _selectedArrow != null)
		{
			_arrowProjDirty = false;
			
			// perform selected arrow orientation in viewport coord
			MeshFilter arrowMeshFilter = _selectedArrow.GetComponent<MeshFilter> ();
			Bounds arrowBounds = arrowMeshFilter.mesh.bounds;
			
			Vector3 top = arrowBounds.center + 
							new Vector3 (arrowBounds.extents.x, 0, 0);
			
			Vector3 bottom = arrowBounds.center - 
								new Vector3 (arrowBounds.extents.x,0, 0);
			
			top = _selectedArrow.localToWorldMatrix.MultiplyPoint (top);
			bottom = _selectedArrow.localToWorldMatrix.MultiplyPoint (bottom);
			
			top = Camera.main.WorldToViewportPoint (top);
			bottom = Camera.main.WorldToViewportPoint (bottom);
			
			Vector2 top2d = new Vector2 (top.x, top.y);
			Vector2 bottom2d = new Vector2 (bottom.x, bottom.y);
			
			_arrowProjDirection = top2d - bottom2d;
			_arrowProjDirection.Normalize ();
		}
		
		// shrink arrows if not selected
		if (_forwardArrow != _selectedArrow)
		{
			_forwardArrow.localScale = Vector3.Lerp (_forwardArrow.localScale, 
														SCALE_START,
														Time.deltaTime * 10.0f);	
		}
		
		if (_rightArrow != _selectedArrow)
		{
			_rightArrow.localScale = Vector3.Lerp (_rightArrow.localScale, 
													SCALE_START,
													Time.deltaTime * 10.0f);	
		}
		
		if (_leftArrow != _selectedArrow)
		{
			_leftArrow.localScale = Vector3.Lerp (_leftArrow.localScale, 
													SCALE_START,
													Time.deltaTime * 10.0f);	
		}
		
		if (_backArrow != _selectedArrow)
		{
			_backArrow.localScale = Vector3.Lerp (_backArrow.localScale, 
													SCALE_START,
													Time.deltaTime * 10.0f);	
		}
		
		// grow the selected Arrow
		if (_selectedArrow != null)
		{
			_selectedArrow.localScale = Vector3.Lerp (_selectedArrow.localScale, 
														SCALE_END,
														Time.deltaTime * 10.0f);
		}
		Rect rect_mode2D = new Rect (0.0f,0.0f,50.0f,50.0f);
		Rect rect_help = new Rect (Screen.width-50.0f,0.0f,50.0f,50.0f);
		if (PC.In.Click1Down() && PC.In.CursorOnUIs(rect_mode2D, rect_help))
		{
			_tempHide = false;
			Validate();
		}
		else if(PC.In.Click1Down() && PC.In.CursorOnUIs(m_uiRect1, m_uiRect2) || camRGB.gameObject.activeInHierarchy)
            m_clickOnUI = true;
        else if(m_clickOnUI && PC.In.Click1Up())
            m_clickOnUI = false;

		else if(PC.In.Click1Up() && !PC.In.CursorOnUIs(m_uiRect1, m_uiRect2))
        {
            _tempHide = false;
            Validate();
        }

//		if((Application.platform == RuntimePlatform.WindowsEditor)
//			||(Application.platform == RuntimePlatform.WindowsPlayer)
//			||(Application.platform == RuntimePlatform.OSXEditor)
//			||(Application.platform == RuntimePlatform.OSXPlayer))
//		{
//		//	if (_selectedSide == null)
//		//		return;
//			//DoTheMickey();	
//			
//			if (Input.GetMouseButtonDown(0))
//			{
//				Vector3 mousePos = Input.mousePosition;
//				if(!m_uiRect1.Contains(mousePos) && !m_uiRect2.Contains(mousePos))
//				{
//					_tempHide = false;
//					Validate();
//				}
//			}
//		}
//		else
//		{
//			if (_selectedSide == null || Input.touchCount <= 0)
//			{ }
//			else
//			{
//				Touch firstTouch = Input.touches[0];
//				if(!m_uiRect1.Contains(firstTouch.position) && !m_uiRect2.Contains(firstTouch.position))
//				{
//					switch (firstTouch.phase)
//					{
//						case TouchPhase.Began:
//						{
//							_tempHide = true;
//						    _hasMoved = false;
//							break;
//						}
//		//				case TouchPhase.Canceled:
//		//				{
//		//					_tempHide = false;
//		//					break;
//		//				}
//						case TouchPhase.Ended:
//						{
//							_tempHide = false;
//						if(_hasMoved==false)
//							Validate();
//							break;
//						}
//					}
//				}
////						case TouchPhase.Moved:
////						{			
////						_hasMoved = true;
////							if (!_selectedSide.Equals (DOWN))
////							{
////								float magnitude = firstTouch.deltaPosition.magnitude;
////								float dot = Vector2.Dot (_arrowProjDirection, 
////								                         firstTouch.deltaPosition);
////							
////								float offset = magnitude * dot * OFFSET_SPEED * Time.deltaTime + _selectedOffset;
////								_selectedOffset = Mathf.Clamp (offset, MIN_OFFSET, MAX_OFFSET);
////								
////								SendMessageOptions opt = SendMessageOptions.DontRequireReceiver;
////								_poolObject.BroadcastMessage (_selectedSide, _selectedOffset, opt);
////								_poolObject.BroadcastMessage ("MarkMappingDirty", opt);
////							}
////							else
////							{
////								float deltaTouch = firstTouch.deltaPosition.y; 
////								float offset = -deltaTouch * HEIGHT_SPEED * Time.deltaTime + _selectedOffset;
////								_selectedOffset = Mathf.Clamp (offset, MIN_HEIGHT, MAX_HEIGHT);
////								
////								SendMessageOptions opt = SendMessageOptions.DontRequireReceiver;
////								_lowWallResizer.SendMessage (_selectedSide, _selectedOffset, opt);
////								_lowWallResizer.SendMessage ("MarkMappingDirty", opt);
////							
////								if (_hasOcclusion)
////								{
////									Vector3 occHeight = _occlusionInitialPosition + 
////														new Vector3(0, _selectedOffset * -1 - 1, 0);
////									
////									_occlusionResizer.transform.localPosition = occHeight;
////								}
////							}
////						
//							switch (_selectedSideIndex)
//							{
//								case 0:
//								{
//									_forwardOffset = _selectedOffset;
//									break;
//								}
//								
//								case 1:
//								{
//									_rightOffset = _selectedOffset;
//									break;
//								}
//									
//								case 2:
//								{
//									_leftOffset = _selectedOffset;
//									break;
//								}
//									
//								case 3:
//								{
//									_backOffset = _selectedOffset;
//									break;
//								}
//								
//								case 4:
//								{
//									_lowWallHeight = _selectedOffset;
//									break;
//								}
//							}
////							break;
//						}
//					}
////				}
////			}
////		}
		
		//animation Interface
		if(_tempHide)
		{
			if(_uiOff7 != Screen.width)
			{
				if(_uiOff7 < Screen.width-1)
				{
					_uiOff7 = Mathf.Lerp(_uiOff7,Screen.width,5*Time.deltaTime);
				}
				else
				{
					_uiOff7 = Screen.width;
				}
			}
		}
		else
		{
			//Screen.width-290
			if(_uiOff7 != Screen.width-290)
			{
				if(_uiOff7 > Screen.width-291)
				{
					_uiOff7 = Mathf.Lerp(_uiOff7,Screen.width-290,5*Time.deltaTime);
				}
				else
				{
					_uiOff7 = Screen.width-290;
				}
			}
		}
	}

    //-----------------------------------------------------
    void OnDisable()
    {
        UsefullEvents.OnResizeWindowEnd -= FitToScreen;
        UsefullEvents.OnResizingWindow  -= FitToScreen;

        if(_hasPool)
        {
    //        GetComponent<GUIMenuMain>().hideShowUD(true);
            _hasPool = false;
        }

        DestroyArrows ();
        _selectedSideIndex = -1;
        _selectedSide = null;


//       if (_backArrow != null)
//       {
//           SetArrowVisible (false);
//
//           _selectedSideIndex = -1;
//           _selectedSide = null;
//
//           if (_selectedArrow != null)
//           {
//               _selectedArrow.localScale = SCALE_START;
//               _selectedArrow = null;
//           }
//       }
    }

    //-----------------------------------------------------
    private void FitToScreen()
    {
        _uiOff7 = Screen.width-290;
        m_uiRect1.x = _uiOff7;
        m_uiRect1.height = Screen.height;
        m_uiRect2.height = Screen.height;
    }

    //-----------------------------------------------------
	void DoTheMickey()
	{	
		float hotWheel = Input.GetAxis("Mouse ScrollWheel")*100;
		if(hotWheel==0)
		{
			if(_tempHide)
				_tempHide = false;
			return;
		}
		
		_tempHide = true;
		
		if (!_selectedSide.Equals (DOWN))
		{	
			//float offset = hotWheel * OFFSET_SPEED * Time.deltaTime + _selectedOffset;
			float offset = hotWheel * OFFSET_SPEED + _selectedOffset;
			//Debug.Log("offset "+offset+", hotWheel "+hotWheel+", HEIGHT_SPEED "+HEIGHT_SPEED+", _selectedOffset "+_selectedOffset);
	
			_selectedOffset = Mathf.Clamp (offset, MIN_OFFSET, MAX_OFFSET);
			
			SendMessageOptions opt = SendMessageOptions.DontRequireReceiver;
			_poolObject.BroadcastMessage (_selectedSide, _selectedOffset, opt);
			_poolObject.BroadcastMessage ("MarkMappingDirty", opt);
		}
		else
		{
			//float offset = -hotWheel * HEIGHT_SPEED * Time.deltaTime + _selectedOffset;
			float offset = -hotWheel * HEIGHT_SPEED * 0.1f + _selectedOffset;
			_selectedOffset = Mathf.Clamp (offset, MIN_HEIGHT, MAX_HEIGHT);
			
			SendMessageOptions opt = SendMessageOptions.DontRequireReceiver;
			_lowWallResizer.SendMessage (_selectedSide, _selectedOffset, opt);
			_lowWallResizer.SendMessage ("MarkMappingDirty", opt);
		
			if (_hasOcclusion)
			{
				Vector3 occHeight = _occlusionInitialPosition + 
									new Vector3(0, _selectedOffset * -1 - 1, 0);
				
				_occlusionResizer.transform.localPosition = occHeight;
			}
		}
		
		switch (_selectedSideIndex)
		{
			case 0:
			{
				_forwardOffset = _selectedOffset;
				break;
			}
			
			case 1:
			{
				_rightOffset = _selectedOffset;
				break;
			}
				
			case 2:
			{
				_leftOffset = _selectedOffset;
				break;
			}
				
			case 3:
			{
				_backOffset = _selectedOffset;
				break;
			}
			
			case 4:
			{
				_lowWallHeight = _selectedOffset;
				break;
			}
		}
	}
	
	void Validate()
	{
		Camera.main.GetComponent<ObjInteraction>().configuringObj(null);
		_poolObject =null;
		GetComponent<GUIMenuInteraction> ().unConfigure();
		GetComponent<GUIMenuInteraction> ().setVisibility (false);
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,false);			
		enabled = false;
	}
	
	void OnGUI ()
	{
//		GUI.skin = skin;
//		GUI.Box(guiZone,"","bg");
//		GUI.BeginGroup(guiZone);
//		int x=50;
//		
//		GUI.Label(new Rect(x,0,100,50),"Plage : ","txt");
//		x+=100;
//		
//		for(int i=0;i<4;i++)
//		{
//			string active="B";
//			if(sideIndex == i)
//				active = "A";
//			if(GUI.Button(new Rect(x+i*70,0,75,40),"","arrow"+i+active))
//				sideIndex = i;
//		}
//		x+=280;
//		
//		GUI.Label(new Rect(x,0,20,50),"","separator");
//		x+=20;
//		
//		if(sideIndex == 4)
//		{
//			if(GUI.Button(new Rect(x,0,150,50),"Muret","txtActif"))
//				sideIndex = -1;
//		}
//		else
//		{
//			if(GUI.Button(new Rect(x,0,150,50),"Muret","txt"))
//				sideIndex = 4;
//		}
//		
//		x+=150;
//		
//		GUI.Label(new Rect(x,0,20,50),"","separator");
//		x+=20;
//		
//		if(GUI.Button(new Rect(x,0,40,50),"","reset"))
//			Debug.Log("reset");
//		x+=40;
//		if(GUI.Button(new Rect(x,0,40,50),"","valid"))
//		{
////			Camera.mainCamera.GetComponent<ObjInteraction>().setSelected(_poolObject,true);
//			Camera.mainCamera.GetComponent<ObjInteraction>().configuringObj(null);
//			_poolObject =null;
//						
////			GetComponent<GUIMenuInteraction> ().setVisibility (true);
////			GetComponent<GUIMenuConfiguration> ().setVisibility (true);
//			GetComponent<GUIMenuInteraction> ().unConfigure();
//			GetComponent<GUIMenuInteraction> ().setVisibility (false);
//			
//			//GetComponent<GUIMenuConfiguration> ().setVisibility (false);
//			
//			Camera.mainCamera.GetComponent<ObjInteraction>().setSelected(null,false);
//			
////			GetComponent<GUIMenuInteraction> ().isConfiguring = false;//true
//			
//			enabled = false;
//		}

//		GUI.EndGroup();
		Camera.main.GetComponent<GuiTextureClip> ().m_active = false;
		if(GUI.Button(new Rect(Screen.width/2 - 75, 0, 150, 49),"",skinMenuInteraction.GetStyle("bouttonValide")))
		{
			//Camera.main.GetComponent<ObjInteraction>().DeselectByButton();
		}

		float m_off7 = Screen.width;
		GUI.DrawTexture (new Rect ( Screen.width - 210, 0,210,Screen.height),textureBackGroundLarge);

		//----Interface Droite -------------
		GUILayout.BeginArea(new Rect(Screen.width - 210, -100, 210, Screen.height+ 60));
	    GUILayout.FlexibleSpace();
	
		//scrollpos = GUILayout.BeginScrollView(scrollpos,"empty",GUILayout.Width(300));//scrollView en cas de menu trop grand

		GUILayout.Box("",skinMenuRight.GetStyle("bgFadeUp"),GUILayout.Width(260),GUILayout.Height(150));//fade en haut
		GUILayout.BeginVertical(skinMenuRight.GetStyle("bgFadeMid"),GUILayout.Width(260));
		
		GUIStyle returnBtn = skinPlage.GetStyle("return");
		
		if(GUILayout.Button(TextManager.GetText("Retour"),returnBtn,GUILayout.Height(50),GUILayout.Width(260)))//BTN RETOUR
		{
			_poolObject =null;	
			
			GetComponent<GUIMenuInteraction> ().isConfiguring = true;
			Camera.main.GetComponent<ObjInteraction>().setActived(false);
			GetComponent<GUIMenuConfiguration>().enabled = true;
			GetComponent<GUIMenuConfiguration>().setVisibility(true);
			//GetComponent<GUIMenuConfiguration>().OpenFunctionTab();
			Camera.main.GetComponent<GuiTextureClip>().enabled = true;
			
			GetComponent<GUIMenuLeft>().canDisplay(false);
			GetComponent<GUIMenuRight>().canDisplay(false);
			
			enabled = false;
		}
		
		GUIStyle muret = skinPlage.GetStyle("txtActif");
		
		if(sideIndex == 4)
		{
			muret.normal.textColor = new Color(1f,1f,1f,1f);
		}
		else
		{
			muret.normal.textColor = new Color(1f,1f,1f,1f);
		}

// ------------------ Debut Slider Droite  ------------------------
		bool change = false;

		GUISkin bkup = GUI.skin;
		GUI.skin = skinPlage;

		bool noMargelle = false;

		string[] selStrings = new string[] {TextManager.GetText("PoolResizerUI.Plage"), TextManager.GetText("PoolResizerUI.Muret"), TextManager.GetText("PoolResizerUI.Margelle")};
		if( getResizer ("plage").gameObject.GetComponent<Renderer>().material.shader.name != "Pointcube/StandardObjet" )
		{
			selStrings = new string[] {TextManager.GetText("PoolResizerUI.Plage"), TextManager.GetText("PoolResizerUI.Muret")};
		}
		if( getResizer ("margelle") == null)
		{
			selStrings = new string[] {TextManager.GetText("PoolResizerUI.Plage"), TextManager.GetText("PoolResizerUI.Muret")};
			if(selectionGrid == 2)
				selectionGrid = 0;

			noMargelle = true;
		}
		if (getResizer ("muret") == null) 
		{
			selStrings = new string[] {TextManager.GetText("PoolResizerUI.Plage")};
			selectionGrid = 0;

			noMargelle = true;
		}

		//si pas de margelle et plage off
		//	OU
		//si plage off et margelle off
		if( 
		   (noMargelle && getResizer ("plage").gameObject.GetComponent<Renderer>().enabled == false ) ||
		   (!noMargelle && getResizer ("plage").gameObject.GetComponent<Renderer>().enabled == false && getResizer ("margelle").gameObject.GetComponent<Renderer>().enabled == false )
		   )
		{
			_poolObject =null;	
			
			GetComponent<GUIMenuInteraction> ().isConfiguring = true;
			Camera.main.GetComponent<ObjInteraction>().setActived(false);
			GetComponent<GUIMenuConfiguration>().enabled = true;
			GetComponent<GUIMenuConfiguration>().setVisibility(true);
			//GetComponent<GUIMenuConfiguration>().OpenFunctionTab();
			Camera.main.GetComponent<GuiTextureClip>().enabled = true;
			
			GetComponent<GUIMenuLeft>().canDisplay(false);
			GetComponent<GUIMenuRight>().canDisplay(false);
			
			enabled = false;
		}

		int lastSelectionGrid = selectionGrid;

		selectionGrid = GUILayout.SelectionGrid (selectionGrid, selStrings,1,skinPlage.GetStyle("SelectionGrid"));

		if( !noMargelle && getResizer ("plage").gameObject.GetComponent<Renderer>().enabled == false && getResizer ("margelle").gameObject.GetComponent<Renderer>().enabled == true )
		{
			selectionGrid = 2;
		}
		else if( !noMargelle && getResizer ("plage").gameObject.GetComponent<Renderer>().enabled == true && getResizer ("margelle").gameObject.GetComponent<Renderer>().enabled == false )
		{
			
			if(selectionGrid == 2)
			{
				selectionGrid = lastSelectionGrid;
			}
			
		}
		GUILayout.Box("",skinPlage.GetStyle("bg") ,GUILayout.Height(10),GUILayout.Width(260), GUILayout.MaxHeight (25));
		PrintArrow(false );
		//GUILayout.Box("",skinPlage.GetStyle("bg4") ,GUILayout.Height(10),GUILayout.Width(260), GUILayout.MaxHeight (25));

		//PLAGE
		if (selectionGrid == 0) 
		{
			PrintArrow(true );

			//mains sliders plage
			for(int i=0;i<4;i++)
			{
				GUIStyle plage = skinPlage.GetStyle("bg");
				plage.normal.textColor = new Color(2f/255f,37f/255f,110f/255f,1f);
				string active="B";
				if(sideIndex == i)
				{
					active = "A";
					plage.normal.textColor = new Color(10f/255f,145f/255f,245f/255f,1f);
				}
				//			if(GUI.Button(new Rect(x+i*70,0,75,40),"","arrow"+i+active))

				string[] strings = new string[] {TextManager.GetText("PoolResizerUI.FlecheBleue"), TextManager.GetText("PoolResizerUI.FlecheRouge"), TextManager.GetText("PoolResizerUI.FlecheVerte"), TextManager.GetText("PoolResizerUI.FlecheJaune")};
				GUIContent gc = new GUIContent(strings[i],skinPlage.GetStyle("arrow"+i+active).normal.background);
				if (getResizer ("plage").gameObject.GetComponent<Renderer> ().material.shader.name == "Pointcube/StandardObjet") {
					if(i != 0)
						GUILayout.Box("",skinPlage.GetStyle("bg4") ,GUILayout.Height(10),GUILayout.Width(260), GUILayout.MaxHeight (25));
					skinPlage.GetStyle("bg3").fixedHeight = 20;
					GUILayout.Box(gc,skinPlage.GetStyle("bg3"), GUILayout.Height (20));
				}
				else
				{
					skinPlage.GetStyle("bg3").fixedHeight = 30;
					GUILayout.Box(gc,skinPlage.GetStyle("bg3"), GUILayout.Height (30));

				}

				float slidechange = tmpOff7Tab[i];


				GUILayout.Box("",skinPlage.GetStyle("bg5"));
				if(i == 0)
				{
					GUI.skin.GetStyle("horizontalsliderthumb").normal.background = texBlue;
					GUI.skin.GetStyle("horizontalsliderthumb").hover.background = texBlue_A;
					GUI.skin.GetStyle("horizontalsliderthumb").active.background = texBlue;
				}
				else if(i == 1)
				{
					GUI.skin.GetStyle("horizontalsliderthumb").normal.background = texRed;
					GUI.skin.GetStyle("horizontalsliderthumb").hover.background = texRed_A;
					GUI.skin.GetStyle("horizontalsliderthumb").active.background = texRed;
				}
				else if(i == 2)
				{
					GUI.skin.GetStyle("horizontalsliderthumb").normal.background = texGreen;
					GUI.skin.GetStyle("horizontalsliderthumb").hover.background = texGreen_A;
					GUI.skin.GetStyle("horizontalsliderthumb").active.background = texGreen;
				}
				else if(i == 3)
				{
					GUI.skin.GetStyle("horizontalsliderthumb").normal.background = texYellow;
					GUI.skin.GetStyle("horizontalsliderthumb").hover.background = texYellow_A;
					GUI.skin.GetStyle("horizontalsliderthumb").active.background = texYellow;
				}


				tmpOff7Tab[i] = GUILayout.HorizontalSlider (tmpOff7Tab[i],MIN_OFFSET , MAX_OFFSET, GUILayout.MaxHeight (40));
				
				GUI.skin.GetStyle("horizontalsliderthumb").normal.background = texWhite;
				GUI.skin.GetStyle("horizontalsliderthumb").hover.background = texWhite_A;
				GUI.skin.GetStyle("horizontalsliderthumb").active.background = texWhite;

				if( slidechange != tmpOff7Tab[i])
				{
					sideIndex = i;
					_selectedOffset = tmpOff7Tab[i];
					tmpOff7 = _selectedOffset;
					change = true;
					//print ("_selectedOffset : "+_selectedOffset);
				}
			}

			if( getResizer ("plage").gameObject.GetComponent<Renderer>().material.shader.name == "Pointcube/StandardObjet" )
			{
				GUILayout.Box("",skinPlage.GetStyle("bg4") ,GUILayout.Height(10),GUILayout.Width(260), GUILayout.MaxHeight (25));

				GUILayout. Box (TextManager.GetText("PoolResizerUI.Rotation") + " : " + tmpOff7RotationUV.ToString("0.##"),skinPlage.GetStyle("bg3") );
				//ROTATION
				GUILayout.Box("",skinPlage.GetStyle("bg5"));
				tmpOff7RotationUV = GUILayout.HorizontalSlider (tmpOff7RotationUV, MIN_OFFSET_ROTATION_UV, MAX_OFFSET_ROTATION_UV, GUILayout.MaxHeight (50));
				print ("tmpOff7RotationUV : "+ tmpOff7RotationUV);
				if (tmpOff7RotationUV != _selectedOffset_RotationUV) {
					_selectedOffset_RotationUV = tmpOff7RotationUV;

					getResizer ("plage").setOffsetRotationUV(_selectedOffset_RotationUV);
					//_poolObject.BroadcastMessage ("setOffsetRotationUV", _selectedOffset_RotationUV);
				}

				GUILayout.Box (TextManager.GetText("PoolResizerUI.Taille") + " : " + tmpOff7TilePlage.ToString("0.##"),skinPlage.GetStyle("bg3") );
				//TAILLE PLAGE
				GUILayout.Box("",skinPlage.GetStyle("bg5"));
				tmpOff7TilePlage = GUILayout.HorizontalSlider (tmpOff7TilePlage, MIN_OFFSET_TILE_PLAGE, MAX_OFFSET_TILE_PLAGE, GUILayout.MaxHeight (50));
				
				if (tmpOff7TilePlage != _selectedOffset_TilePlage) {
					_selectedOffset_TilePlage = tmpOff7TilePlage;

					getResizer ("plage").setOffsetTileUV(_selectedOffset_TilePlage);
				//_poolObject.BroadcastMessage ("setOffsetTileUV", _selectedOffset_TilePlage);
				}
			}

		} 
		else if (selectionGrid == 1)
		{
			GUILayout.Box (TextManager.GetText ("PoolResizerUI.MuretTaille"), skinPlage.GetStyle ("bg3"));

			float slidechange = tmpOff7Tab[4];
			GUILayout.Box("",skinPlage.GetStyle("bg5"));
			tmpOff7Tab[4] = GUILayout.HorizontalSlider (tmpOff7Tab[4],MIN_OFFSET , MAX_OFFSET_MURET, GUILayout.MaxHeight (40));
			if( slidechange != tmpOff7Tab[4])
			{
				sideIndex = 4;
				_selectedOffset = tmpOff7Tab[4];
				tmpOff7 = _selectedOffset;
				change = true;
				//print ("_selectedOffset : "+_selectedOffset);
			}

			if (getResizer ("plage").gameObject.GetComponent<Renderer> ().material.shader.name == "Pointcube/StandardObjet") {

				//GUILayout.Box ("", skinPlage.GetStyle ("bg4"), GUILayout.Height (10), GUILayout.Width (260), GUILayout.MaxHeight (25));
				GUILayout.Box (TextManager.GetText("PoolResizerUI.Taille") + " : " + tmpOff7TileMuret.ToString("0.##"),skinPlage.GetStyle("bg3") );
				//TAILLE PLAGE
				GUILayout.Box("",skinPlage.GetStyle("bg5"));
				tmpOff7TileMuret = GUILayout.HorizontalSlider (tmpOff7TileMuret, MIN_OFFSET_TILE_MURET, MAX_OFFSET_TILE_MURET, GUILayout.MaxHeight (50));

				if (tmpOff7TileMuret != _selectedOffset_TileMuret) 
				{
					_selectedOffset_TileMuret = tmpOff7TileMuret;
					getResizer ("muret").setOffsetTileUV(_selectedOffset_TileMuret);
				}
			}

		}
		else if (selectionGrid == 2)
		{

			GUILayout.Box (TextManager.GetText("PoolResizerUI.Taille") + " : " + tmpOff7TileMargelle.ToString("0.##"),skinPlage.GetStyle("bg3") );
			//TAILLE PLAGE

			GUILayout.Box("",skinPlage.GetStyle("bg5"));
			tmpOff7TileMargelle = GUILayout.HorizontalSlider (tmpOff7TileMargelle, MIN_OFFSET_TILE_MARGELLE, MAX_OFFSET_TILE_MARGELLE, GUILayout.MaxHeight (50));
			
			if (tmpOff7TileMargelle != _selectedOffset_TileMargelle) 
			{
				_selectedOffset_TileMargelle = tmpOff7TileMargelle;
				getResizer ("margelle").setOffsetTileUV(_selectedOffset_TileMargelle);
			}
			
		}

		Color backUpColor = GUI.color;
		if (getResizer ("plage").gameObject.GetComponent<Renderer> ().material.shader.name == "Pointcube/StandardObjet") 
		{
			//GUILayout.Box ("", skinPlage.GetStyle ("bg4"), GUILayout.Height (10), GUILayout.Width (260), GUILayout.MaxHeight (10));
			GUILayout.Box (TextManager.GetText ("PoolResizerUI.Couleur"), skinPlage.GetStyle ("bg3"));
			
		}

		switch (selectionGrid){
		case 0:
			if(picker.GetComponent<HSVPicker>().currentPlageColor != new Color(1,0,1,0)){
				GUI.color = picker.GetComponent<HSVPicker>().currentPlageColor;
			}else{
				GUI.color =  picker.GetComponent<HSVPicker>().currentColor;
			}
			break;
		case 1:
			if(picker.GetComponent<HSVPicker>().currentMuretColor != new Color(1,0,1,0)){
				GUI.color = picker.GetComponent<HSVPicker>().currentMuretColor;
			}else{
				GUI.color =  picker.GetComponent<HSVPicker>().currentColor;
			}
			break;
		break;
		case 2:
			if(picker.GetComponent<HSVPicker>().currentMargelleColor != new Color(1,0,1,0)){
				GUI.color = picker.GetComponent<HSVPicker>().currentMargelleColor;
			}else{
				GUI.color =  picker.GetComponent<HSVPicker>().currentColor;
			}
			break;
	}

		if(color == null){
			color = new Texture2D(240,30);
		}

		if( getResizer ("plage").gameObject.GetComponent<Renderer>().material.shader.name == "Pointcube/StandardObjet" )
		{
			if (GUILayout.Button (color,skinPlage.GetStyle("bg4")) )
			{

				float hueLevel;

				switch (selectionGrid){
				case 0:
					if(picker.GetComponent<HSVPicker>().currentPlageColor != new Color(1,0,1,0))
					{
						picker.GetComponent<HSVPicker>().AssignColor(picker.GetComponent<HSVPicker>().currentPlageColor);
						getResizer ("plage").setHueLevel(1);
						getResizer ("plage").setSaturation(1);
					}

					break;
				case 1:
					if(picker.GetComponent<HSVPicker>().currentMuretColor != new Color(1,0,1,0))
					{
						picker.GetComponent<HSVPicker>().AssignColor(picker.GetComponent<HSVPicker>().currentMuretColor);
						getResizer ("muret").setHueLevel(1);
						getResizer ("muret").setSaturation(1);
					}
					break;
				break;
				case 2:
					if(picker.GetComponent<HSVPicker>().currentMargelleColor != new Color(1,0,1,0))
					{
						picker.GetComponent<HSVPicker>().AssignColor(picker.GetComponent<HSVPicker>().currentMargelleColor);
						getResizer ("margelle").setHueLevel(1);
						getResizer ("margelle").setSaturation(1);
					}
					break;
				}

				camRGB.gameObject.SetActive(true);
				m_ChangeColor = true;
			}

		}
		GUI.color = backUpColor;
		GUI.skin = bkup;
// ------------------ Fin Slider Droite  ------------------------

		GUILayout.EndVertical();
		GUILayout.Box("",skinMenuRight.GetStyle("bgFadeDw"),GUILayout.Width(260),GUILayout.Height(150));//fade en bas
		
		//GUILayout.EndScrollView();
		
	    GUILayout.FlexibleSpace();
	    GUILayout.EndArea();
		
		//------------------------
		
//		int sideIndex = GUI.SelectionGrid (new Rect (25, 25, 50, 250), 
//											_selectedSideIndex, _commands, 1);




		if( m_ChangeColor )
		{

			_colorTab[selectionGrid] = picker.GetComponent<HSVPicker>().currentColor;
			//print (_colorTab[selectionGrid]);
			//print (_colorTab[selectionGrid]);

			Vector3 vec = new Vector3(_colorTab[selectionGrid].r,_colorTab[selectionGrid].g,_colorTab[selectionGrid].b);
			if( selectionGrid == 0)
			{

					getResizer ("plage").setColorPlage(vec);
				//getResizer ("plage").setHueLevel(1);

			}
			else if( selectionGrid == 1)
			{

				getResizer ("muret").setColorMuret(vec);
				//getResizer ("muret").setHueLevel(1);

			}
			else  if( selectionGrid == 2)
			{

				getResizer ("margelle").setColorMargelle(vec);
				//getResizer ("margelle").setHueLevel(1);
	
			}
				
			if(!camRGB.gameObject.activeInHierarchy)
			{
				picker.GetComponent<HSVPicker>().currentColor = new Color(0,0,0);
				m_ChangeColor = false;
			}

		}

		if (sideIndex != _selectedSideIndex)
		{
			_selectedSideIndex = sideIndex;	
			_arrowProjDirty = true;
			//print ("OKKK");
			switch (_selectedSideIndex)
			{

				case 0:
				{

					_selectedSide = FORWARD;
					_selectedArrow = _forwardArrow;
					_selectedOffset = _forwardOffset;
					tmpOff7 = _selectedOffset;
					MIN_OFFSET = _sideWalkResizer.getFwdMinOff7();
					break;
				}
				
				case 1:
				{
					_selectedSide = RIGHT;
					_selectedArrow = _rightArrow;
					_selectedOffset = _rightOffset;
					tmpOff7 = _selectedOffset;
					MIN_OFFSET = _sideWalkResizer.getRgtMinOff7();
					break;
				}
					
				case 2:
				{
					_selectedSide = LEFT;
					_selectedArrow = _leftArrow;
					_selectedOffset = _leftOffset;
					tmpOff7 = _selectedOffset;
					MIN_OFFSET = _sideWalkResizer.getLftMinOff7();
					break;
				}
					
				case 3:
				{
					_selectedSide = BACK;
					_selectedArrow = _backArrow;
					_selectedOffset = _backOffset;
					tmpOff7 = _selectedOffset;
					MIN_OFFSET = _sideWalkResizer.getBckMinOff7();
					break;
				}
				
				case 4:
				{
					_selectedSide = DOWN;
					_selectedArrow = null;
					_selectedOffset = _lowWallHeight;
					tmpOff7 = _selectedOffset;
					break;
				}
					/*
				default:
				{
				print ("default");
					_selectedSide = null;
					_selectedArrow = null;
					break;
				}	*/
			}
		}
		/*
		else if (GUI.changed)
		{
			_selectedSideIndex = -1;
			_selectedSide = null;
			_selectedArrow = null;
		}
		*/
//		if(GUI.Button(new Rect (10, Screen.height - 30,20,20),"QUIT"))
//		{
//			_poolObject =null;
//			enabled = false;
//			GetComponent<GUIMenuConfiguration> ().setVisibility (true);
//			GetComponent<GUIMenuInteraction> ().setVisibility (true);
//			Camera.mainCamera.GetComponent<ObjInteraction> ().setActived (true);
//			GameObject.Find ("MainScene").GetComponent<GUIMenuInteraction> ().isConfiguring = true;
//		}

		//--------- Slider gauche---------------------
		bkup = GUI.skin;
		GUI.skin = skinPlage;

		bkup = GUI.skin;
		GUI.skin = skinPlage;
		//print ("_selectedSide ::::::: " +_selectedSide);
			if(_selectedSide != null && change)
			{
				//GUI.DrawTexture (new Rect ( 0, 0,textureBackGroundLight.width,Screen.height),textureBackGroundLight);

				if (!_selectedSide.Equals (DOWN))
				{

	//				float magnitude = firstTouch.deltaPosition.magnitude;
	//				float dot = Vector2.Dot (_arrowProjDirection, 
	//				                         firstTouch.deltaPosition);
	//			
	//				float offset = magnitude * dot * OFFSET_SPEED * Time.deltaTime + _selectedOffset;
	//				_selectedOffset = Mathf.Clamp (offset, MIN_OFFSET, MAX_OFFSET);
					
					
					//tmpOff7 = GUI.VerticalSlider(new Rect(-10,84,142,600),tmpOff7,MAX_OFFSET,MIN_OFFSET);
					
					//if(tmpOff7 != _selectedOffset)
					//{
						//Debug.Log("value="+tmpOff7+"Bornes inf : "+MIN_OFFSET+", sup="+MAX_OFFSET);
						_selectedOffset = tmpOff7;
						SendMessageOptions opt = SendMessageOptions.DontRequireReceiver;
						_poolObject.BroadcastMessage (_selectedSide, _selectedOffset, opt);
						_poolObject.BroadcastMessage ("MarkMappingDirty", opt);
						UpdateOffset();
					//}


				}
				else
				{
	//				float deltaTouch = firstTouch.deltaPosition.y; 
	//				float offset = -deltaTouch * HEIGHT_SPEED * Time.deltaTime + _selectedOffset;
	//				_selectedOffset = Mathf.Clamp (offset, MIN_HEIGHT, MAX_HEIGHT);
					
					//tmpOff7 = GUI.VerticalSlider(new Rect(-10,84,142,600),tmpOff7,MIN_HEIGHT,MAX_HEIGHT);
					
					//if(tmpOff7 != _selectedOffset)
					//{
						_selectedOffset = tmpOff7;
						SendMessageOptions opt = SendMessageOptions.DontRequireReceiver;
						_lowWallResizer.SendMessage (_selectedSide, _selectedOffset, opt);
						_lowWallResizer.SendMessage ("MarkMappingDirty", opt);
						UpdateOffset();
						if (_hasOcclusion)
						{
							Vector3 occHeight = _occlusionInitialPosition + 
												new Vector3(0, _selectedOffset * -1 - 1, 0);
							
							_occlusionResizer.transform.localPosition = occHeight;
						}
				//	}

				}

			}
		
		GUI.skin = bkup;
		
		//------------------------
		
	}

	void PrintArrow(bool _bool)
	{
		_forwardArrow.GetComponent<Renderer> ().enabled = _bool;
		_rightArrow.GetComponent<Renderer> ().enabled = _bool;
		_leftArrow.GetComponent<Renderer> ().enabled = _bool;
		_backArrow.GetComponent<Renderer> ().enabled = _bool;
	}

	void UpdateOffset()
	{
		switch (_selectedSideIndex)
		{
			case 0:
			{
				_forwardOffset = _selectedOffset;
				break;
			}
			
			case 1:
			{
				_rightOffset = _selectedOffset;
				break;
			}
				
			case 2:
			{
				_leftOffset = _selectedOffset;
				break;
			}
				
			case 3:
			{
				_backOffset = _selectedOffset;
				break;
			}
			
			case 4:
			{
				_lowWallHeight = _selectedOffset;
				break;
			}
		}
	}

	public void CreateArrows ()
	{
		if (Arrow3DModel == null)
		{
			throw new ArgumentNullException ("Arrow3DModel");
		}
		
		// Instantiate all 3d arrows and set their color
		_forwardArrow = Instantiate(Arrow3DModel, Vector3.zero, 
								Quaternion.identity) as Transform;
		_forwardArrow.GetComponent<Renderer>().material.color = new Color32 (33, 179, 254, 255);
		
		_rightArrow = Instantiate(Arrow3DModel, Vector3.zero, 
									Quaternion.identity) as Transform;
		_rightArrow.GetComponent<Renderer>().material.color = new Color32 (252, 50, 64, 255);
		
		_leftArrow = Instantiate(Arrow3DModel, Vector3.zero, 
									Quaternion.identity) as Transform;
		_leftArrow.GetComponent<Renderer>().material.color = new Color32 (33, 180, 129, 255);
		
		_backArrow = Instantiate(Arrow3DModel, Vector3.zero, 
									Quaternion.identity) as Transform;
		_backArrow.GetComponent<Renderer>().material.color = new Color32 (255, 252, 67, 255);
		
		SCALE_START = Arrow3DModel.localScale;
	}
	
	public void DestroyArrows ()
	{
		if (_forwardArrow != null)
		{
			Destroy (_forwardArrow.gameObject);
			Destroy (_rightArrow.gameObject);
			Destroy (_leftArrow.gameObject);
			Destroy (_backArrow.gameObject);
		}
	}
	
	public GameObject GetPoolObject ()
	{
		return _poolObject;	
	}
	public void DoActionUI(GameObject gameobject)
	{
		GameObject.Find("MainScene").GetComponent<PoolResizerUI>().enabled = true;
		SetPoolObject (gameobject.transform);
		GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);
			
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;
			
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,true);
		Camera.main.GetComponent<ObjInteraction>().setActived(false);
		sideIndex = 0;
		
	}
	public void SetPoolObject (Transform pool)
	{
		SetPoolObject (pool.gameObject);
	}
		
	public void SetPoolObject (GameObject pool)
	{
		if (pool == null)
		{
			DestroyArrows ();
			return;
		}

		_poolObject = pool;
		_hasPool = true;
		
		_sideWalkResizer = getResizer("plage");

		if (_sideWalkResizer == null)
		{
			DestroyArrows ();
			return;
		}
		
		_occlusionResizer = getResizer("SolOcclu");
		_hasOcclusion = _occlusionResizer != null;
		
		if (_hasOcclusion)
			_occlusionInitialPosition = _occlusionResizer.transform.localPosition;
		
		_lowWallResizer = getResizer("muret");
		
		Bounds b = _sideWalkResizer.InitialBounds;
		
		// place arrows in relation to the sidewalk initial bounding box
		if(_forwardArrow == null)
		{
			CreateArrows();
		}
		
//		_forwardArrow.parent = _sideWalkResizer.transform;
//		_forwardArrow.localPosition = new Vector3 (b.center.x, Y_ARROW, b.center.z + b.extents.z);
//		_forwardArrow.localRotation = Quaternion.AngleAxis (-90, Vector3.up);
//		
//		_rightArrow.parent = _sideWalkResizer.transform;
//		_rightArrow.localPosition = new Vector3 (b.center.x + b.extents.x, Y_ARROW, b.center.z);
//		_rightArrow.localRotation = Quaternion.AngleAxis (0, Vector3.up);
//		
//		_leftArrow.parent = _sideWalkResizer.transform;
//		_leftArrow.localPosition = new Vector3 (b.center.x - b.extents.x, Y_ARROW, b.center.z);
//		_leftArrow.localRotation = Quaternion.AngleAxis (180, Vector3.up);
//		
//		_backArrow.parent = _sideWalkResizer.transform;
//		_backArrow.localPosition = new Vector3 (b.center.x, Y_ARROW, b.center.z - b.extents.z);
//		_backArrow.localRotation = Quaternion.AngleAxis (90, Vector3.up);
		
		float arrowLength = _forwardArrow.GetComponent<Renderer>().bounds.extents.z * 4;
		
		_forwardArrow.parent = _sideWalkResizer.transform;
		_forwardArrow.localPosition = new Vector3 (b.center.x, Y_ARROW, b.center.z + arrowLength);
		_forwardArrow.localRotation = Quaternion.AngleAxis (-90, Vector3.up);
		
		_rightArrow.parent = _sideWalkResizer.transform;
		_rightArrow.localPosition = new Vector3 (b.center.x + arrowLength, Y_ARROW, b.center.z);
		_rightArrow.localRotation = Quaternion.AngleAxis (0, Vector3.up);
		
		_leftArrow.parent = _sideWalkResizer.transform;
		_leftArrow.localPosition = new Vector3 (b.center.x - arrowLength, Y_ARROW, b.center.z);
		_leftArrow.localRotation = Quaternion.AngleAxis (180, Vector3.up);
		
		_backArrow.parent = _sideWalkResizer.transform;
		_backArrow.localPosition = new Vector3 (b.center.x, Y_ARROW, b.center.z - arrowLength);
		_backArrow.localRotation = Quaternion.AngleAxis (90, Vector3.up);
		
		// get offsets from resizer
		_forwardOffset = _sideWalkResizer.GetForwardOffset ();
		_rightOffset = _sideWalkResizer.GetRightOffset ();
		_leftOffset = _sideWalkResizer.GetLeftOffset ();
		_backOffset = _sideWalkResizer.GetBackOffset ();
		_lowWallHeight = _lowWallResizer.GetDownOffset ();

		tmpOff7Tab[0] = _sideWalkResizer.GetForwardOffset ();
		tmpOff7Tab[1] = _sideWalkResizer.GetRightOffset ();
		tmpOff7Tab[2] = _sideWalkResizer.GetLeftOffset ();
		tmpOff7Tab[3] = _sideWalkResizer.GetBackOffset ();
		tmpOff7Tab[4] = _lowWallResizer.GetDownOffset ();

		tmpOff7RotationUV = getResizer ("plage").getOffsetRotationUV ();
		tmpOff7TilePlage = getResizer ("plage").getOffsetTileUV ();
		if (tmpOff7TilePlage <= 0.01f)
			tmpOff7TilePlage = 0.4f;

		Vector3 vec = getResizer ("plage").getColorPlage ();
		_colorTab [0] = new Color (vec.x,vec.y,vec.z);

		camRGB.gameObject.SetActive(true);

		Color color;

		if (getResizer ("plage").gameObject.GetComponent<Renderer> ().material.shader.name == "Pointcube/StandardObjet") 
		{
			color = getResizer ("plage").GetComponent<Renderer> ().material.GetColor ("_Huecolor");
			picker.GetComponent<HSVPicker> ().currentPlageColor = color;

		}

		if (getResizer ("plage").gameObject.GetComponent<Renderer> ().material.shader.name == "Pointcube/StandardObjet") {

			if (getResizer ("muret") != null) {
				vec = getResizer ("muret").getColorPlage ();
				_colorTab [1] = new Color (vec.x, vec.y, vec.z);

				color = getResizer ("muret").GetComponent<Renderer> ().material.GetColor ("_Huecolor");
				picker.GetComponent<HSVPicker> ().currentMuretColor = color;
				tmpOff7TileMuret = getResizer ("muret").getOffsetTileUV ();
				if (tmpOff7TileMuret <= 0.01f)
					tmpOff7TileMuret = 0.4f;
			}
		}
			if( getResizer ("plage").gameObject.GetComponent<Renderer>().material.shader.name == "Pointcube/StandardObjet" )
			{
				if( getResizer ("margelle") != null)
				{
					vec = getResizer ("margelle").getColorPlage ();
					_colorTab[2] = new Color (vec.x,vec.y,vec.z);

					color = getResizer ("margelle").GetComponent<Renderer>().material.GetColor("_Huecolor");
					picker.GetComponent<HSVPicker>().currentMargelleColor = color;
					tmpOff7TileMargelle = getResizer ("margelle").getOffsetTileUV ();
					if (tmpOff7TileMargelle <= 0.01f)
					tmpOff7TileMargelle = 0.4f;
				}
			}

		camRGB.gameObject.SetActive(false);


		Function_hideObject hider = pool.GetComponent<Function_hideObject> ();
		if (hider != null && hider._hide)
		{
			//hider.DoAction ();
		}
	}
	public void validateColor(){
		switch (selectionGrid){
			case 0:
				picker.GetComponent<HSVPicker>().currentPlageColor = picker.GetComponent<HSVPicker>().currentColor;
				break;
			case 1:
				picker.GetComponent<HSVPicker>().currentMuretColor = picker.GetComponent<HSVPicker>().currentColor;
				break;
			case 2:
				picker.GetComponent<HSVPicker>().currentMargelleColor = picker.GetComponent<HSVPicker>().currentColor;
				break;
		}
		camRGB.gameObject.SetActive(false);

	}
	public void SetArrowVisible (bool visible)
	{
		_forwardArrow.GetComponent<Renderer>().enabled = visible;
		_rightArrow.GetComponent<Renderer>().enabled = visible;
		_leftArrow.GetComponent<Renderer>().enabled = visible;
		_backArrow.GetComponent<Renderer>().enabled = visible;
	}

}
