using UnityEngine;
using System.Collections;
using System.IO;

using Pointcube.Global;
using Pointcube.Utils;

public class FunctionConf_PergolaAutoFeet : MonoBehaviour,Function_OS3D
{
	
	public GameObject frameMesh;				//Objet réference a instancier (cadre par defaut)
	public GameObject feetMesh;					//Objet réference a instancier (pieds)
	public GameObject arrowRef;
	
	public float maxSizeBladesDir;				//taille maxi sens des lames
	public float maxSize;						//taille maxi
	public float widthMin;						//taille largeur mini
	public float lengthMin;						//taille longueur mini
	public float heightMax;						//taille hauteur maxi
	public float heightMin;						//taille hauteur mini
	
	public float frameSizeH;					//taille hauteur poutre cadre(la largeur est la meme que footsize)
	public float footSize;						//taille largeur et longueur d'un pied
	
	public Material frameMaterial;				//Materiau du pergola
	
	public GUIStyle selectorStyle;
	
	public int os3dFuncId;
	
	public PergolaType _type;
	
	public enum PergolaType
	{
		ilot, 	//pas de poteau, pas de screens
		muralLength,
		muralWidth,
		lN,		//
		lS,
		patio	//tout les poteaux, tout les screens
	}
	//---------------------------------------------

	private bool _isLengthOriented = true;		//sens des lames
	private bool _isFeetVisible = false;		//Affichage des pieds intermédiares
	private bool _firstBuild = true;
	
	private bool _uiShowDim = false;			//Affichage UI Dimmensions
	private bool _uiShowFeet = false;			//Affichage UI foot
	private bool _uiShowSelector = true;		//Affichage UI Selecteur modules
	private bool _uiShowType = false;			//Affichage UI Selecteur type pergola
	
	private bool _sideRestrictionN = true;		//peut avoir des screens coté N? (ex. patio > non)
	private bool _sideRestrictionS = true;		//peut avoir des screens coté S? (ex. patio > non)
	private bool _sideRestrictionE = true;		//peut avoir des screens coté E? (ex. patio > non)
	private bool _sideRestrictionW = true;		//peut avoir des screens coté W? (ex. patio > non)
	private bool _arrowVisible = false;			//visibilité des fleches
	private bool _useSideRestriction = false;	//utilise ou non la restriction en fonction de la pos. du module et du type de pergola
	private bool _useSideOrientation = false;	//indique si les flèches montrent les angles ou les cotés (true = angles)
	private bool _showUI = true;				//btn qui affiche/cache les options de configurations
	
	private GameObject _frame;					//GameObject parent cadre
	private GameObject _frameMesh;				//Objet réference a instancier (cadre par defaut)
	private GameObject _arrows;					//GameObject parent des fleches
	
	private float _localWidth;					//taille largeur d'un module
	private float _localLength;					//taille longeur d'un module
	
	private float _widthMax;					//taille largeur maxi
	private float _lengthMax;					//taille longueur maxi
	
	private float _width;						//taille largeur totale
	private float _length;						//taille longueur totale
	private float _height;						//taille hauteur totale
	private float _bladeLengthMax;				//Taille mini avant 2lames
	
	private int _nbLengthModule;				//nb de modules dans le sens de la longueur
	private int _nbWidthModule;					//nb de modules dans le sens de la largeur
	private int _selectedModule = 0;			//numero du module sélectionné
	
	private string[] _uiModules;				//tableau des numéros de modules pour UI
	private string _uiWidth;					//largeur totale pour UI
	private string _uiLength;					//longueur totale pour UI
	private string _uiHeight;					//hauteur totale pour UI
	private const string  _tagSel = "_Selector";
	private const string  _tagDim = "_Dimensions";
	private const string  _tagFet = "_Feet";
	private const string  _tagTyp = "_Type";
	
	private Quaternion _savedRotation;			//rotation du pergola
	private Vector3 _savedScale;				//scale du pergola
	
	private ArrayList _options = new ArrayList();//liste des options(neons,screens,...)
	
	private GUISkin _uiSkin;					//Skin de l'UI
	private Rect _uiArea;						//Zone ou est l'UI
	private Vector2 _uiScroll;					//scroll de l'UI
	
	private Material _selectMat;
	
	private float _oldDiag;
	private ArrayList visu = new ArrayList();
	
	private string _currentOpenedMenu = "";
	
	private float _timer;
	private float _firstTime;
	private const float _refresh = 0.15f;
	
	private GameObject _iosShadows;
	
	private bool _showFeetSwitch = false;
	
	//----------vv Functions vv-----------------------------
	// ---------test for Biossun V2------------------------- 
	void Awake()
	{
		if(gameObject.GetComponent<Imposte_PAF>()==null)
		{
			gameObject.AddComponent<Imposte_PAF>();
			Imposte_PAF imposte_PAF = gameObject.GetComponent<Imposte_PAF>();
			imposte_PAF.meshRef = (GameObject) Resources.Load("Prefabs/imposteRef");
		}
		if(gameObject.GetComponent<Claustra_PAF>()==null)
		{
			gameObject.AddComponent<Claustra_PAF>();
			Claustra_PAF claustra_PAF = gameObject.GetComponent<Claustra_PAF>();
			claustra_PAF.meshRef = (GameObject) Resources.Load("Prefabs/claustraRef");
		}
		if(gameObject.GetComponent<Jardiniere_PAF>()==null)
		{
			gameObject.AddComponent<Jardiniere_PAF>();
			Jardiniere_PAF jardiniere_PAF = gameObject.GetComponent<Jardiniere_PAF>();
			jardiniere_PAF.meshRef = (GameObject) Resources.Load("Prefabs/jardiniereRef");
		}
		if(gameObject.GetComponent<AllGlass_PAF>()==null)
		{
			gameObject.AddComponent<AllGlass_PAF>();
			AllGlass_PAF allGlass_PAF = gameObject.GetComponent<AllGlass_PAF>();
			allGlass_PAF.meshRef = (GameObject) Resources.Load("Prefabs/All-glassRef");
		}
	}
	// --------- Fin test for Biossun V2--------------------
	// Use this for initialization	
	void Start ()
	{		
		_iosShadows = GameObject.Find("iosShadows");
//		bool disable = true;
//		if(transform.GetChildCount() > 0)
//		{
//			disable = false;
//		}
		
		//Var's
		_selectMat = new Material(Shader.Find("Diffuse"));
		_selectMat.color = Color.green;
		
		//Recherche des "options" du pergola
		foreach(Component cp in this.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType() != this.GetType() && cp.GetType().GetInterface("IPergolaAutoFeet")!= null)
			{
				_options.Add(cp);
			}
		}
		
		//Initialisations
		Init();
		foreach(IPergolaAutoFeet paf in _options)
			paf.Init(transform,this);
		
		//pergola par defaut
		_frameMesh = frameMesh;
//		_type = PergolaType.ilot;
		if(_isLengthOriented)
		{
			_length = 3.5f;
			_width = 4.0f;
		}
		else
		{
			_width = 3.5f;
			_length = 4.0f;
		}
		_height = 2.500f;
		
		//set de l'UI
		
		_uiWidth = (Mathf.RoundToInt(_width*1000)).ToString();
		_uiLength = (Mathf.RoundToInt(_length*1000)).ToString();
		_uiHeight = (Mathf.RoundToInt(_height*1000)).ToString();
		
		_uiArea = new Rect(Screen.width-280, 0, 300, Screen.height);
		if(_uiSkin == null)
			_uiSkin = (GUISkin)Resources.Load("skins/PergolaSkin");
		
		CreateArrows();
		

		//Building
		PergolaAutoFeetEvents.FirePergolaTypeChange(_type.ToString());
		PergolaAutoFeetEvents.FireRebuild();
		
//		_firstBuild = false;
		foreach(Transform t in transform)
		{
			Quaternion q = new Quaternion(0,0,0,0);
			q.eulerAngles = new Vector3(0,180,0);
			t.localRotation = q;
		}
		
//		if(disable)
//			enabled = false;
		if(_firstBuild)
			_firstTime = Time.time + 2 * Time.fixedDeltaTime;
		UsefullEvents.ScaleChange += RecalcOldDiag;
	}
	
	void OnEnable()
	{
		//Ajout aux listeners
		PergolaAutoFeetEvents.rebuild += Rebuild;
		PergolaAutoFeetEvents.bladesDirChange += UpdateBladeDir;
		PergolaAutoFeetEvents.toggleUIVisibility += ToggleUI;
		PergolaAutoFeetEvents.pergolaTypeChange += UpdateType;
	} 
	
	void OnDisable()
	{
		PergolaAutoFeetEvents.FireToggleUIVisibility("close");
		
		PergolaAutoFeetEvents.rebuild -= Rebuild;
		PergolaAutoFeetEvents.bladesDirChange -= UpdateBladeDir;
		PergolaAutoFeetEvents.toggleUIVisibility -= ToggleUI;
		PergolaAutoFeetEvents.pergolaTypeChange -= UpdateType;
		
	}
	
	void OnDestroy()
	{
		UsefullEvents.ScaleChange -= RecalcOldDiag;
	}

	// Update is called once per frame
	void Update ()
	{
		if((_isLengthOriented && _length <= maxSize &&_length >= _bladeLengthMax) ||
				(!_isLengthOriented && _width <= maxSize &&_width >= _bladeLengthMax))
			_showFeetSwitch = true;
		else
			_showFeetSwitch = false;
		
		if(_firstBuild)
		{
			if(Time.time > _firstTime)
			{
				//_firstBuild = false;
				SaveTransform();
				CreateBounds();
				ReapplyTransform();
				enabled = false;
				_firstBuild = false;
			}
		}

        //--vv-- INTERACTION --vv--
        if(PC.In.Click1Up() && !PC.In.CursorOnUI(_uiArea))
		    Validate();

        //Slide menus
        float deltaScroll;
        if(PC.In.ScrollViewV(out deltaScroll) && PC.In.CursorOnUI(_uiArea))
            _uiScroll.y = _uiScroll.y + deltaScroll;

		//--vv-- INTERACTION --vv--
//#if UNITY_IPHONE && !UNITY_EDITOR
//		if(Input.touchCount != 0)
//		{
//			Touch firstTouch = Input.touches[0];
//			if(!_uiArea.Contains(firstTouch.position))
//			{
//				if(firstTouch.phase == TouchPhase.Ended)
//				{
//					Validate();
//				}
//			}
//		}
//
//		//Slide menus
//		if(Input.touchCount>0)
//		{
//			Touch t = Input.touches[0];
//			if(_uiArea.Contains(t.position) && Mathf.Abs(t.deltaPosition.y)>5)
//			{
//				_uiScroll.y = _uiScroll.y + t.deltaPosition.y;
//			}
//		}
//#else
//		if (Input.GetMouseButtonDown(0))
//		{
//			Vector3 mousePos = Input.mousePosition;
//			if(!_uiArea.Contains(mousePos))
//			{
//				Validate();
//			}
//		}
//#endif
	}
	
	void Init()
	{
		if(transform.FindChild("frame"))
			_frame = transform.FindChild("frame").gameObject;
		else
		{
			_frame = new GameObject("frame");
			_frame.transform.parent = transform;
			_frame.transform.localPosition = Vector3.zero;
			if(!_frame.GetComponent<MeshRenderer>())
				_frame.AddComponent<MeshRenderer>();
			_frame.GetComponent<Renderer>().material = frameMaterial;
		}
		
		if(_isLengthOriented)
		{
			_lengthMax = maxSizeBladesDir;
			_widthMax = maxSize;
		}
		else
		{
			_widthMax = maxSizeBladesDir;
			_lengthMax = maxSize;
		}
		
	}
	
	#region UI
	
	void OnGUI()
	{
		GUI.skin = _uiSkin;
		//-------vv----FADE Haut-------------vv------------------
		GUI.Box(new Rect(Screen.width-280.0f,0.0f,Screen.width-280.0f,Screen.height),"","BackGround");
		GUILayout.BeginArea(_uiArea);
		GUILayout.FlexibleSpace();
		_uiScroll = GUILayout.BeginScrollView(_uiScroll,"empty","empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
		GUILayout.Box("","UP",GUILayout.Width(280),GUILayout.Height(150));//fade en haut
		GUILayout.BeginVertical("MID");
		
		//-------vv----UI Configurateur-------vv---------------
		
//		//UI Selector
//		bool tmpSel = GUILayout.Toggle(_uiShowSelector,TextManager.GetText("Pergola.Module"),GUILayout.Height(50),GUILayout.Width(280));
//		if(tmpSel != _uiShowSelector)
//		{
//			_uiShowSelector = tmpSel;
//			if(_uiShowSelector)
//				PergolaAutoFeetEvents.FireToggleUIVisibility(_tagSel);
//			else
//				ShowSelected();
//		}
//		
//		if(_uiShowSelector)
//		{
//			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
//			GUILayout.FlexibleSpace();
//			
//			if(_nbLengthModule > 0 && _nbWidthModule > 0)
//			{
//				int tmp = GUILayout.SelectionGrid(_selectedModule,_uiModules,_nbLengthModule,selectorStyle);
//				if(tmp != _selectedModule)
//				{
//					_selectedModule = tmp;
//					ShowSelected();
//					PergolaAutoFeetEvents.FireSelectedModuleChanged(_selectedModule);
//					UpdateArrowsPos();
//				}
//			}
//			GUILayout.Space(20);
//			GUILayout.EndHorizontal();
//		}
		
		if(GUILayout.Button(TextManager.GetText("GUIMenuConfiguration.Materials"),"Menu",GUILayout.Height(50),GUILayout.Width(280)))
		{
			
			GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = true;
			Camera.main.GetComponent<ObjInteraction>().setActived(false);
//			Camera.mainCamera.GetComponent<ObjInteraction>().setSelected(gameObject);
			GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().enabled = true;
			GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(true);
			GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().OpenMaterialTab();
			
			GameObject.Find("MainScene").GetComponent<GUIMenuLeft>().canDisplay(false);
			GameObject.Find("MainScene").GetComponent<GUIMenuRight>().canDisplay(false);
			
			enabled = false;
			
		}
		
		_showUI = GUILayout.Toggle(_showUI,"Designer","Menu",GUILayout.Height(50),GUILayout.Width(280));
		
		if(_showUI)
		{
			//UI Dimmensions
			bool tmpDim = GUILayout.Toggle(_uiShowDim,TextManager.GetText("Pergola.Sizes"),GUILayout.Height(50),GUILayout.Width(280));
			if(tmpDim != _uiShowDim)
			{
				_uiShowDim = tmpDim;
				if(_uiShowDim)
					PergolaAutoFeetEvents.FireToggleUIVisibility(_tagDim);
				else
					PergolaAutoFeetEvents.FireToggleUIVisibility("close");
			}
			
			if(_uiShowDim)
			{
				SingleSizeUI(ref _width,ref _uiWidth,widthMin,"Width");
				
				SingleSizeUI(ref _length,ref _uiLength,lengthMin,"Avance");
				
				SingleSizeUI(ref _height,ref _uiHeight,heightMin,"Height");
			}
			
			//UI Type
			bool tmpTyp = GUILayout.Toggle(_uiShowType,TextManager.GetText("Pergola.Type"),GUILayout.Height(50),GUILayout.Width(280));
			if(tmpTyp != _uiShowType)
			{
				_uiShowType = tmpTyp;
				if(_uiShowType)
					PergolaAutoFeetEvents.FireToggleUIVisibility(_tagTyp);
				else
					PergolaAutoFeetEvents.FireToggleUIVisibility("close");
			}
			
			if(_uiShowType)
			{
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				PergolaType tmpType = _type;
				
				if(GUILayout.Button("<","btn<",GUILayout.Height(50),GUILayout.Width(50)))
				{
					int typ = (int)_type;
					typ ++;
					if(typ >= 6)
						typ = 0;
					if(typ == 1 /*|| typ == 2*/)
						typ = 2;
					_type = (PergolaType)typ;
				}
				
				GUILayout.Label(GetTypeToString(),"TextField",GUILayout.Height(40),GUILayout.Width(125));
				
				if(GUILayout.Button(">","btn>",GUILayout.Height(50),GUILayout.Width(50)))
				{
					int typ = (int)_type;
					typ --;
					if(typ < 0)
						typ = 5;
					if(typ == 1 /*|| typ == 2*/)
						typ = 0;
					_type = (PergolaType)typ;
				}
				
				if(tmpType != _type)
				{
	//				_type = tmpType;
					PergolaAutoFeetEvents.FirePergolaTypeChange(_type.ToString());
					PergolaAutoFeetEvents.FireRebuild();
				}
				GUILayout.Space(20);	
				GUILayout.EndHorizontal();
			}
			
			//UI Foot
			if(_showFeetSwitch)
			{
				bool tmpFet = GUILayout.Toggle(_uiShowFeet,TextManager.GetText("Pergola.Feet"),GUILayout.Height(50),GUILayout.Width(280));
				if(tmpFet != _uiShowFeet)
				{
					_uiShowFeet = tmpFet;
					if(_uiShowFeet)
						PergolaAutoFeetEvents.FireToggleUIVisibility(_tagFet);
					else
						PergolaAutoFeetEvents.FireToggleUIVisibility("close");
				}
				
				if(_uiShowFeet)
				{
					GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
					GUILayout.Space(40);
					GUILayout.FlexibleSpace();
					string txt="";
					if(_isFeetVisible)
						txt = TextManager.GetText("Pergola.Hide");
					else
						txt = TextManager.GetText("Pergola.Show");
					
					bool tmpFeet = GUILayout.Toggle(_isFeetVisible,txt,"txt",GUILayout.Height(50));
					if(tmpFeet != _isFeetVisible)
					{
						_isFeetVisible = tmpFeet;
						UpdateFeetVisibility();
					}
					GUILayout.FlexibleSpace();
					GUILayout.Space(20);
					GUILayout.EndHorizontal();
				}
			}
			else
			{
				if(!_isFeetVisible)
				{
					_isFeetVisible = true;
					UpdateFeetVisibility();
				}
			}
			//UI Options
			foreach(IPergolaAutoFeet paf in _options)
				paf.GetUI();
		}
		//-------vv----FADE BAS-------------vv------------------
		GUILayout.EndVertical();
		GUILayout.Box("","DWN",GUILayout.Width(280),GUILayout.Height(150));//fade en bas
		
		GUILayout.EndScrollView();
		GUILayout.FlexibleSpace();
		
		GUILayout.EndArea();
	}
	
	void SingleSizeUI(ref float val,ref string ui,float minVal,string tag)
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		if(GUILayout.RepeatButton("-","btn-",GUILayout.Height(50),GUILayout.Width(50)) && Time.time > _timer)
		{
			val = val - 0.100f;
			if(val<minVal)
			{
				val = minVal;
			}
			ui = (Mathf.RoundToInt(val*1000)).ToString();
			BuildIt();
			_timer = Time.time + _refresh;
		}
		GUI.SetNextControlName(tag);
		ui = GUILayout.TextField(ui,GUILayout.Height(40),GUILayout.Width(75));
#if UNITY_IPHONE || UNITY_ANDROID
		if(ui != (Mathf.RoundToInt(val*1000)).ToString())
		{

				val = float.Parse(ui) / 1000;
				BuildIt();
		}
#else
		if(ui != (Mathf.RoundToInt(val*1000)).ToString() && (GUI.GetNameOfFocusedControl() != tag || Event.current.keyCode == KeyCode.Return))
		{

				val = float.Parse(ui) / 1000;
				BuildIt();
		}
#endif
		if(GUILayout.RepeatButton("+","btn+",GUILayout.Height(50),GUILayout.Width(50)) && Time.time > _timer)
		{
			val = val + 0.1f;
			int tmp = Mathf.RoundToInt(val*1000f);
			ui = (Mathf.RoundToInt(tmp)).ToString();
			BuildIt();
			_timer = Time.time + _refresh;
		}
		
		GUILayout.Label(TextManager.GetText("Pergola."+tag)+"\n(mm)",GUILayout.Width(60));
		GUILayout.Space(30);
		GUILayout.EndHorizontal();
	}
	
	public void GetModuleSelectorUI()
	{
		//UI Selector
		bool tmpSel = GUILayout.Toggle(_uiShowSelector,TextManager.GetText("Pergola.Module"),"toggle2",GUILayout.Height(50),GUILayout.Width(280));
		if(tmpSel != _uiShowSelector)
		{
			_uiShowSelector = tmpSel;
//			if(_uiShowSelector)
//				PergolaAutoFeetEvents.FireToggleUIVisibility(_tagSel);
//			else
				ShowSelected();
		}
		
		if(_uiShowSelector)
		{
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			if(_nbLengthModule > 0 && _nbWidthModule > 0)
			{
				int tmp = GUILayout.SelectionGrid(_selectedModule,_uiModules,_nbLengthModule,selectorStyle);
				if(tmp != _selectedModule)
				{
					_selectedModule = tmp;
					ShowSelected();
					PergolaAutoFeetEvents.FireSelectedModuleChanged(_selectedModule);
					UpdateArrowsPos();
					UpdateArrowsView(_arrowVisible);
				}
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}	
	}
	
	#endregion
	
	#region BUILDING
	
	void BuildIt()
	{	
		//Clear Pergola
		ClearIt();
		
		if(_length<lengthMin)
		{
			_length = lengthMin;
			_uiLength = ((int)(_length*1000)).ToString();
		}
		
		if(_width<widthMin)
		{
			_width = widthMin;
			_uiWidth = ((int)(_width*1000)).ToString();
		}
		
		if(_height < heightMin)
		{
			_height = heightMin;
			_uiHeight = ((int)(_height*1000)).ToString();
		}
		if(_height > heightMax)
		{
			_height = heightMax;
			_uiHeight = ((int)(_height*1000)).ToString();
		}
		
		//Check and calculate local values
		CheckAndCalculateValues();
		
		int nameIndex = 0;
		Vector3 origine = new Vector3(-_length/2+_localLength/2,0,_width/2-_localWidth/2);
		Vector3 off7 = origine;
	
		_uiModules = new string[_nbLengthModule*_nbWidthModule];
		
		// --vv-- calcul taille des blocs dans le selecteur (cas des tailles dynamiques)
		int fixedWidth = 250/_nbLengthModule;
		int  fixedHeight = 250/_nbWidthModule;
		
		if(fixedWidth > 50)
			fixedWidth = 50;
		if(fixedHeight > 50)
			fixedHeight = 50;
		
		selectorStyle.fixedHeight = Mathf.Min(fixedWidth,fixedHeight);
		selectorStyle.fixedWidth = Mathf.Min(fixedWidth,fixedHeight);
		
		//Construction-----vv--------------
		SaveTransform();
		
		for(int z=0;z<_nbWidthModule;z++)
		{
			off7.z = origine.z - z*_localWidth;
			for(int x=0;x<_nbLengthModule;x++)
			{		
				off7.x = origine.x + x*_localLength;
				Vector4 nsewTyp = GetBlocType(nameIndex);
				BuildFeet(off7,_frame.transform,nameIndex.ToString(),nsewTyp);
				BuildFrame(off7,_frame.transform,nameIndex.ToString(),nsewTyp);
				
				foreach(IPergolaAutoFeet paf in _options)
					paf.Build(off7,nameIndex,nsewTyp);
				
//				AddBloc(off7,nameIndex.ToString());
				_uiModules[nameIndex] = nameIndex.ToString();
				nameIndex ++;
			}
		}
		
		if(_selectedModule > (_nbLengthModule*_nbWidthModule))
			_selectedModule = 0;
		
		UpdateArrowsPos();
		
		UpdateFeetVisibility();
		
		CreateBounds();
		
		ReapplyTransform();

		if(UsefulFunctions.GetMontage().GetSM().IsNightMode())
			UsefullEvents.FireNightMode(true);
		
//		CreateBounds();
	}
	
	void CheckAndCalculateValues()
	{
		float MaxValueL = 0;
		float MaxValueW = 0;
//		Old Style
		if(_isLengthOriented)
		{
			if(_length > maxSize)
				MaxValueL = _lengthMax;
			else
				MaxValueL = maxSize;
			
			MaxValueW = _widthMax;
		}
		else
		{
			if(_width > maxSize)
				MaxValueW = _widthMax;
			else
				MaxValueW = maxSize;
			
			MaxValueL = _lengthMax;
		}
		//-----------------------------------
		
		if(_length <= _lengthMax && _width <= _widthMax)
		{
			_localWidth = _width;
			_localLength = _length;
			_nbWidthModule = 1;
			_nbLengthModule = 1;
		}
		else if(_length > MaxValueL && _width <= MaxValueW)
		{
			_localWidth = _width;
			_nbWidthModule = 1;
			
			_nbLengthModule = Mathf.CeilToInt(_length/MaxValueL);
			_localLength = _length/_nbLengthModule;
			
		}
		else if(_width > MaxValueW && _length <= MaxValueL)
		{
			_localLength = _length;
			_nbLengthModule = 1;
			
			_nbWidthModule = Mathf.CeilToInt(_width/MaxValueW);
			_localWidth = _width/_nbWidthModule;
		}
		else
		{
			_nbLengthModule = Mathf.CeilToInt(_length/MaxValueL);
			_localLength = _length/_nbLengthModule;
			
			_nbWidthModule = Mathf.CeilToInt(_width/MaxValueW);
			_localWidth = _width/_nbWidthModule;
		}
		
	}
	
	void BuildFeet(Vector3 origine,Transform parent,string prefix,Vector4 nsew)
	{
		
		float sclN = nsew.x;
		float sclS = nsew.y;
		float sclE = nsew.z;
		float sclW = nsew.w;
		
		float posN = 1-nsew.x;
		float posS = 1-nsew.y;
		float posE = 1-nsew.z;
		float posW = 1-nsew.w;
		
		float halfFs = footSize /2;
		
		//---------------NORTH WEST------------------
		Vector3 off7 = new Vector3(-_localLength/2+footSize/2,_height/2,_localWidth/2-footSize/2);
		Vector3 nsewOff7 = new Vector3(-posW*halfFs,0,posN*halfFs);
		GameObject f1 = (GameObject)Instantiate(feetMesh);
		f1.name = prefix + "_feet1"; // NW
		f1.transform.parent = parent;
		if(f1.GetComponent<Renderer>())
		{
			f1.GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		else
		{
			f1.transform.GetChild(0).GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		f1.transform.localPosition = origine + off7 + nsewOff7;
		f1.transform.localScale = new Vector3(footSize*sclW,_height,footSize*sclN);
		
		//---------------NORTH EAST------------------
		off7 = new Vector3(_localLength/2-footSize/2,_height/2,_localWidth/2-footSize/2);
		nsewOff7 = new Vector3(posE*halfFs,0,posN*halfFs);
		GameObject f2 = (GameObject)Instantiate(feetMesh);
		f2.name = prefix + "_feet2"; //NE
		f2.transform.parent = parent;
		if(f2.GetComponent<Renderer>())
		{
			f2.GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		else
		{
			f2.transform.GetChild(0).GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		f2.transform.localPosition = origine + off7 + nsewOff7;
		f2.transform.localScale = new Vector3(footSize*sclE,_height,footSize*sclN);
		
		
		//---------------SOUTH WEST------------------
		off7 = new Vector3(-_localLength/2+footSize/2,_height/2,-_localWidth/2+footSize/2);
		nsewOff7 = new Vector3(-posW*halfFs,0,-posS*halfFs);
		GameObject f3 = (GameObject)Instantiate(feetMesh);
		f3.name = prefix + "_feet3"; //SW
		f3.transform.parent = parent;
		if(f3.GetComponent<Renderer>())
		{
			f3.GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		else
		{
			f3.transform.GetChild(0).GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		f3.transform.localPosition = origine + off7 + nsewOff7;
		f3.transform.localScale = new Vector3(footSize*sclW,_height,footSize*sclS);
		
		//---------------SOUTH EAST------------------
		off7 = new Vector3(_localLength/2-footSize/2,_height/2,-_localWidth/2+footSize/2);
		nsewOff7 = new Vector3(posE*halfFs,0,-posS*halfFs);
		GameObject f4 = (GameObject)Instantiate(feetMesh);
		f4.name = prefix + "_feet4"; //SE
		f4.transform.parent = parent;
		if(f4.GetComponent<Renderer>())
		{
			f4.GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		else
		{
			f4.transform.GetChild(0).GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		f4.transform.localPosition = origine + off7 + nsewOff7;
		f4.transform.localScale = new Vector3(footSize*sclE,_height,footSize*sclS);
	}
	
	void BuildFrame(Vector3 origine,Transform parent,string prefix,Vector4 nsew)
	{
		Vector3 scaleW = new Vector3(_localWidth-2*footSize,frameSizeH,footSize);
		Vector3 scaleL = new Vector3(_localLength-2*footSize,frameSizeH,footSize);
		
		float posN = 1-nsew.x; //0 || 0.5
		float posS = 1-nsew.y;
		float posE = 1-nsew.z;
		float posW = 1-nsew.w;
		
		float halfFs = footSize /2;
		
		//----------WEST------------------------
		Vector3 off7 = new Vector3(-_localLength/2+footSize/2,_height-frameSizeH/2,0);
		Vector3 nsewOff7 = new Vector3(0,0,posN*halfFs - posS*halfFs);
		Vector3 nsewScl = new Vector3(posN*footSize+posS*footSize,0,0);
		
		GameObject f1 = (GameObject)Instantiate(_frameMesh);
		f1.name = prefix + "_frame4"; // W
		f1.transform.parent = parent;
		if(f1.GetComponent<Renderer>())
		{
			f1.GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		else
		{
			f1.transform.GetChild(0).GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		f1.transform.localPosition = origine + off7 + nsewOff7;
		f1.transform.localScale = scaleW + nsewScl;
		f1.transform.Rotate(new Vector3(0,-90,0));
		
		//----------EAST------------------------
		off7 = new Vector3(_localLength/2-footSize/2,_height-frameSizeH/2,0);
		nsewOff7 = new Vector3(0,0,posN*halfFs - posS*halfFs);
		nsewScl = new Vector3(posN*footSize+posS*footSize,0,0);
		
		GameObject f2 = (GameObject)Instantiate(_frameMesh);
		f2.name = prefix + "_frame3";//East
		f2.transform.parent = parent;
		if(f2.GetComponent<Renderer>())
		{
			f2.GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		else
		{
			f2.transform.GetChild(0).GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		f2.transform.localPosition = origine + off7 + nsewOff7;
		f2.transform.localScale = scaleW + nsewScl;
		f2.transform.Rotate(new Vector3(0,90,0));
		
		//----------SOUTH------------------------
		off7 = new Vector3(0,_height-frameSizeH/2,-_localWidth/2+footSize/2);
		nsewOff7 = new Vector3(posE*halfFs - posW*halfFs,0,0);
		nsewScl = new Vector3(posE*footSize+posW*footSize,0,0);
		
		GameObject f3 = (GameObject)Instantiate(_frameMesh);
		f3.name = prefix + "_frame2";//South
		f3.transform.parent = parent;
		if(f3.GetComponent<Renderer>())
		{
			f3.GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		else
		{
			f3.transform.GetChild(0).GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		f3.transform.localPosition = origine + off7 + nsewOff7;
		f3.transform.localScale = scaleL + nsewScl;
		f3.transform.Rotate(new Vector3(0,180,0));
		
		//----------NORTH------------------------
		off7 = new Vector3(0,_height-frameSizeH/2,_localWidth/2-footSize/2);
		nsewOff7 = new Vector3(posE*halfFs - posW*halfFs,0,0);
		nsewScl = new Vector3(posE*footSize+posW*footSize,0,0);
		
		GameObject f4 = (GameObject)Instantiate(_frameMesh);
		f4.name = prefix + "_frame1";//North
		f4.transform.parent = parent;
		if(f4.GetComponent<Renderer>())
		{
			f4.GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		else
		{
			f4.transform.GetChild(0).GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		}
		f4.transform.localPosition = origine + off7 + nsewOff7;
		f4.transform.localScale = scaleL + nsewScl;
		f4.transform.Rotate(new Vector3(0,0,0));
	}
	
	void ClearIt()
	{
		foreach(Transform t in _frame.transform)
			Destroy(t.gameObject);
		
		foreach(Transform t in _frame.transform)
			Destroy(t.gameObject);
		
		foreach(IPergolaAutoFeet paf in _options)
					paf.Clear();
		
	}
	
	private void Rebuild()
	{
		ClearIt();
		foreach(IPergolaAutoFeet paf in _options)
			paf.Clear();
			
		BuildIt();	
	}
	
	void CreateBounds()
	{
		if(!GetComponent<BoxCollider>())
		{
			gameObject.AddComponent<BoxCollider>();
		}
		
//		float oldMagnitude = Vector3.SqrMagnitude(gameObject.collider.bounds.size);
//		float newMagnitude = Vector3.SqrMagnitude(new Vector3(_length,_height,_width));
//		
//		float sclFactor = newMagnitude/oldMagnitude;
//		Debug.Log("NewMagn>"+newMagnitude+"<OldMagn>"+oldMagnitude+"<SIZE FACTOR>"+sclFactor);
		
		gameObject.GetComponent<BoxCollider>().center = new Vector3(0,_height/2,0);
		gameObject.GetComponent<BoxCollider>().size = new Vector3(_length,_height,_width);

//        _iosShadows.GetComponent<IosShadowManager>().UpdateShadowPos(gameObject);
        if(!_firstBuild)
        {
            gameObject.GetComponent<BoxCollider>().center = new Vector3(0,_height - frameSizeH/2,0);
            gameObject.GetComponent<BoxCollider>().size = new Vector3(_length,frameSizeH,_width);
        }

		if(!_firstBuild)
		{
			ScaleIosShadows();
		}
		else
		{
			BoxCollider box = gameObject.GetComponent<BoxCollider>();
			Bounds bounds = new Bounds (
				box.center, box.extents);
            _oldDiag = _3Dutils.GetBoundDiag(bounds);
            _oldDiag *= Montage.sm.getCamData().m_s;
		}
	}
	
	//Définition du type de bloc
	
	private Vector4 GetBlocType(int index)
	{
		Vector4 offset = new Vector4(0.5f,0.5f,0.5f,0.5f);// x,y,z,w > n,s,e,w avec soit 1 soit 0.5
//		Debug.Log("bloc "+index+" ----");
		if(IsNorthBloc(index))
		{
			offset.x = 1;
//			Debug.Log("North");
		}
		if(IsSouthBloc(index))
		{
			offset.y = 1;
//			Debug.Log("South");
		}

		if(IsEastBloc(index))
		{
			offset.z = 1;
//			Debug.Log("East");
		}
		if(IsWestBloc(index))
		{
			offset.w = 1;
//			Debug.Log("West");
		}
		return offset;
	}
	
	private bool IsWestBloc(int index)
	{
		if(index == 0)
		{
			return true;
		}
		else if(index == 1)
		{
			if(GetNbL()==1)
				return true;
			else
				return false;
		}
		else
		{
			if(index%GetNbL() == 0)
				return true;
			else
				return false;
		}
	}
	private bool IsEastBloc(int index)
	{
		if(index == 0)
		{
			if(GetNbL()==1)
				return true;
			else
				return false;
		}
		else if(index == 1)
		{
			if(GetNbL() <= 2)
				return true;
			else
				return false;
		}
		else
		{
			if((index+1)%GetNbL() == 0)
				return true;
			else
				return false;
		}
	}
	private bool IsNorthBloc(int index)
	{
		if(index < GetNbL())
			return true;
		else
			return false;
	}
	private bool IsSouthBloc(int index)
	{
		int max = GetNbL()*GetNbW();
		if(index >= max - GetNbL())
			return true;
		else
			return false;
	}
	
	private void SaveTransform()
	{
		_savedRotation = transform.localRotation;
		_savedScale = transform.localScale;
		
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
	
	private void ReapplyTransform()
	{
		transform.localRotation = _savedRotation;
		transform.localScale = _savedScale;
		
		foreach(Transform t in transform)
		{
			if(t.name != "arrows")
			{
				t.localPosition = Vector3.zero;
//				t.localRotation = Quaternion.identity;
				Quaternion q = new Quaternion(0,0,0,0);
				q.eulerAngles = new Vector3(0,180,0);
				t.localRotation = q;
				t.localScale = Vector3.one;
			}
		}
	}
	
	private void CreateArrows()
	{
		if(_arrows != null)
		{
			foreach(Transform t in _arrows.transform)
				Destroy(t.gameObject);
		}
		else
		{
			_arrows = new GameObject("arrows");
			_arrows.transform.parent = transform;
			_arrows.transform.localPosition = Vector3.zero;
		}
		
		_arrows.transform.localScale = Vector3.one;
		_arrows.transform.localRotation = Quaternion.identity;
		
		GameObject N = (GameObject)Instantiate(arrowRef);
		N.name = "n";
		N.transform.parent = _arrows.transform;
		N.transform.localRotation = Quaternion.identity;
		N.transform.Rotate(new Vector3(0,-90,0));
		N.transform.localScale = Vector3.one;
		N.GetComponent<Renderer>().material = _selectMat;
		N.GetComponent<Renderer>().material.color = new Color(252f/255f,50f/255f,64f/255f);
		
		GameObject S = (GameObject)Instantiate(arrowRef);
		S.name = "s";
		S.transform.parent = _arrows.transform;
		S.transform.localRotation = Quaternion.identity;
		S.transform.Rotate(new Vector3(0,90,0));
		S.transform.localScale = Vector3.one;
		S.GetComponent<Renderer>().material = _selectMat;
		S.GetComponent<Renderer>().material.color = new Color(33f/255f,179f/255f,254f/255f);
		
		GameObject E = (GameObject)Instantiate(arrowRef);
		E.name = "e";
		E.transform.parent = _arrows.transform;
		E.transform.localRotation = Quaternion.identity;
		E.transform.Rotate(new Vector3(0,0,0));
		E.transform.localScale = Vector3.one;
		E.GetComponent<Renderer>().material = _selectMat;
		E.GetComponent<Renderer>().material.color = new Color(255f/255f,252f/255f,67f/255f);
		
		GameObject W = (GameObject)Instantiate(arrowRef);
		W.name = "w";
		W.transform.parent = _arrows.transform;
		W.transform.localRotation = Quaternion.identity;
		W.transform.Rotate(new Vector3(0,180,0));
		W.transform.localScale = Vector3.one;
		W.GetComponent<Renderer>().material = _selectMat;
		W.GetComponent<Renderer>().material.color = new Color(33f/255f,180f/255f,129f/255f);
		
		UpdateArrowsView(false);
	}
	
	#endregion
	
	private void UpdateBladeDir(bool b)
	{
		_isLengthOriented = b;
		if(_isLengthOriented)
		{
			_lengthMax = maxSizeBladesDir;
			_widthMax = maxSize;
		}
		else
		{
			_widthMax = maxSizeBladesDir;
			_lengthMax = maxSize;
		}
		
//		PergolaAutoFeetEvents.FireRebuild();
	}
	
	private void UpdateFeetVisibility()
	{

		foreach(Transform p in _frame.transform)
		{
			if(p.name.Contains("addfeet"))
			{
				ShowHide(p,_isFeetVisible);
			}
			
			switch (_type)
			{
//			case PergolaType.ilot:
//				ya tout
//				break;
			case PergolaType.muralLength:
				if(p.localPosition.z > _width/2-footSize)
				{
					if(p.name.Contains("addfeet"))
						ShowHide(p,false);
					else
					{
						Vector3 tmpPos = p.localPosition;
						tmpPos.y = _height-frameSizeH/2;
						p.localPosition = tmpPos;
						
						Vector3 tmpScl = p.localScale;
						tmpScl.y = frameSizeH;
						p.localScale = tmpScl;
						
					}
				}
				break;
			case PergolaType.muralWidth:
				if(p.localPosition.x < -_length/2+footSize)
				{
					if(p.name.Contains("addfeet"))
						ShowHide(p,false);
					else
					{
						Vector3 tmpPos = p.localPosition;
						tmpPos.y = _height-frameSizeH/2;
						p.localPosition = tmpPos;
						
						Vector3 tmpScl = p.localScale;
						tmpScl.y = frameSizeH;
						p.localScale = tmpScl;
						
					}
				}
				break;
			case PergolaType.lN:
				if(p.localPosition.z > _width/2-footSize || p.localPosition.x > _length/2-footSize)
				{
					if(p.name.Contains("addfeet"))
						ShowHide(p,false);
					else
					{
						Vector3 tmpPos = p.localPosition;
						tmpPos.y = _height-frameSizeH/2;
						p.localPosition = tmpPos;
						
						Vector3 tmpScl = p.localScale;
						tmpScl.y = frameSizeH;
						p.localScale = tmpScl;
						
					}
				}
				break;
			case PergolaType.lS:
				if(p.localPosition.z < -_width/2+footSize || p.localPosition.x > _length/2-footSize)
				{
					if(p.name.Contains("addfeet"))
						ShowHide(p,false);
					else
					{
						Vector3 tmpPos = p.localPosition;
						tmpPos.y = _height-frameSizeH/2;
						p.localPosition = tmpPos;
						
						Vector3 tmpScl = p.localScale;
						tmpScl.y = frameSizeH;
						p.localScale = tmpScl;
						
					}
				}
				break;
			case PergolaType.patio:
				if(Mathf.Abs(p.localPosition.z) > _width/2-footSize || Mathf.Abs(p.localPosition.x) > _length/2-footSize)
				{
					if(p.name.Contains("addfeet"))
						ShowHide(p,false);
					else
					{
						Vector3 tmpPos = p.localPosition;
						tmpPos.y = _height-frameSizeH/2;
						p.localPosition = tmpPos;
						
						Vector3 tmpScl = p.localScale;
						tmpScl.y = frameSizeH;
						p.localScale = tmpScl;
						
					}
				}
				break;
			}
		}
	}
	
	private void ShowHide(Transform t,bool b)
	{
		if(t.GetComponent<Renderer>())
		{
			t.GetComponent<Renderer>().enabled = b;
		}
		else
		{
			t.GetChild(0).GetComponent<Renderer>().enabled = b;	
		}
	}
	
	//Mets en vert le module sélectionné
	private void ShowSelected()
	{
		foreach(Transform t in _frame.transform)
		{
			int id = int.Parse(t.name.Split('_')[0]);
			if(t.GetComponent<Renderer>())
			{
				t.GetComponent<Renderer>().material = (_uiShowSelector && id == _selectedModule)? _selectMat : _frame.GetComponent<Renderer>().material;
			}
			else
			{
				t.transform.GetChild(0).GetComponent<Renderer>().material = (_uiShowSelector && id == _selectedModule)? _selectMat : _frame.GetComponent<Renderer>().material;
			}
		}
	}
	
	public void ToggleUI(string s)
	{
		_currentOpenedMenu = s;
		switch (_currentOpenedMenu)
		{
		case _tagSel:
			_uiShowDim = false;
			_uiShowFeet = false;
			_uiShowType = false;
			break;
		case _tagDim:
//			_uiShowSelector = false;
			_uiShowFeet = false;
			_uiShowType = false;
			break;
		case _tagFet:
			_uiShowDim = false;
//			_uiShowSelector = false;
			_uiShowType = false;
			break;
		case _tagTyp:
			_uiShowDim = false;
//			_uiShowSelector = false;
			_uiShowFeet = false;
			_uiShowType = true;
			break;
		default:
			_uiShowDim = false;
//			_uiShowSelector = false;
			_uiShowFeet = false;
			_uiShowType = false;
			_uiScroll.y += 500;
			break;
		}
		
		if(s == typeof(Screens_PAF).ToString() ||
			s== typeof(SideAccessories_PAF).ToString() ||
			s == typeof(Plage_PAF).ToString()||
			s == typeof(Imposte_PAF).ToString()||
			s == typeof(Claustra_PAF).ToString()||
			s == typeof(AllGlass_PAF).ToString()||
			s == typeof(Jardiniere_PAF).ToString())
		{
			
			_useSideOrientation = false;
			if(s != typeof(Plage_PAF).ToString())
			{
				
				if (s == typeof(Jardiniere_PAF).ToString())
				{	
					// pour les jardinière on montre les poteaux et pas les cotés			
					_useSideOrientation = true;
				}
				if(s == typeof(Screens_PAF).ToString()||
					s == typeof(Imposte_PAF).ToString()||
					s == typeof(Claustra_PAF).ToString()||
					s == typeof(AllGlass_PAF).ToString()/*||
					s == typeof(Jardiniere_PAF).ToString()*/)
				{
					_useSideRestriction = true;
				}
				else
					_useSideRestriction = false;
				
				UpdateArrowsPos();	
				UpdateArrowsView(true);
				_uiShowSelector = true;
			}
			else
			{
				_useSideRestriction = false;
				UpdateArrowsView(true,false);
				UpdateArrowsPos(true);	
			}
		}
		else
		{
			UpdateArrowsView(false);
			_useSideRestriction = false;
			if(s != typeof(Plage_PAF).ToString())
			{
				_uiShowSelector = false;
			}
		}
		
		CheckAndRebuild();
		
		ShowSelected();
	}
	
	private void UpdateArrowsPos()
	{
		UpdateArrowsPos(false);	
	}
	private void UpdateArrowsPos(bool center)
	{	
		Vector3 origine = new Vector3(+_length/2-_localLength/2,0,-_width/2+_localWidth/2);
		float w=0;
		float l=0;
		// --vv-- détermination centre du module en fonction de son index --vv-- 
		if(!center)
		{
			int z = 0;
			int x = 0;
			if(_nbLengthModule > 0)
			{
				z = _selectedModule / _nbLengthModule;
				x = _selectedModule % _nbLengthModule;
			}
					
			origine += new Vector3(-x*_localLength,0,+z*_localWidth);
			_arrows.transform.localPosition = origine;
			w = _localWidth;
			l = _localLength;
		}
		else
		{
			w = _width;
			l = _length;
			_arrows.transform.localPosition = Vector3.zero;
		}
	
		Quaternion rotation = new Quaternion(0.0f,0.0f,0.0f,1.0f);
		rotation.eulerAngles = new Vector3(0.0f,180.0f,0.0f);
		if(_useSideOrientation)
		{
			rotation.eulerAngles = new Vector3(0.0f,205.0f,0.0f);
		}
		_arrows.transform.localRotation = rotation;
		//  --vv-- placement des arrows --vv-- 
		foreach(Transform t in _arrows.transform)
		{			
			switch(t.name)
			{
			case "n":
		//		t.localRotation = rotation;
				t.localPosition = new Vector3(0,0.2f,w/2);
				break;
			case "s":
		//		t.localRotation = rotation;
				t.localPosition = new Vector3(0,0.2f,-w/2);
				break;
			case "e":
		//		t.localRotation = rotation;
				t.localPosition = new Vector3(l/2,0.2f,0);
				break;
			case "w":
		//		t.localRotation = rotation;
				t.localPosition = new Vector3(-l/2,0.2f,0);
				break;
			}
		}
	} 
	
	public void UpdateArrowsView(bool newVal)
	{
		UpdateArrowsView(newVal,true);
	}
	public void UpdateArrowsView(bool newVal,bool selectiveDisp)
	{ 
		Debug.Log("Update Arrows >"+newVal);
		_arrowVisible = newVal;
		if(selectiveDisp)
		{
			foreach(Transform t in _arrows.transform)
			{
				switch(t.name)
				{
				case "n":
					t.GetComponent<Renderer>().enabled = _arrowVisible  && (_useSideRestriction? _sideRestrictionN && IsNorthBloc(_selectedModule): true);
					break;
				case "s":
					t.GetComponent<Renderer>().enabled = _arrowVisible  && (_useSideRestriction? _sideRestrictionS && IsSouthBloc(_selectedModule) : true); 
					break;
				case "e":
					t.GetComponent<Renderer>().enabled = _arrowVisible  && (_useSideRestriction? _sideRestrictionE && IsEastBloc(_selectedModule) : true);
					break;
				case "w":
					t.GetComponent<Renderer>().enabled = _arrowVisible && (_useSideRestriction? _sideRestrictionW && IsWestBloc(_selectedModule) : true);
					break;
				}
	//			t.renderer.enabled = newVal;
			}
		}
		else
		{
			foreach(Transform t in _arrows.transform)
			{
				switch(t.name)
				{
				case "n":
					t.GetComponent<Renderer>().enabled = _arrowVisible ;
					break;
				case "s":
					t.GetComponent<Renderer>().enabled = _arrowVisible ;
					break;
				case "e":
					t.GetComponent<Renderer>().enabled = _arrowVisible ;
					break;
				case "w":
					t.GetComponent<Renderer>().enabled = _arrowVisible ;
					break;
				}
	//			t.renderer.enabled = newVal;
			}	
		}
	}
	
	private void ScaleIosShadows()
	{
		BoxCollider box = gameObject.GetComponent<BoxCollider>();
		Bounds bounds = new Bounds (
			box.center, box.extents);
		float newDiag = _3Dutils.GetBoundDiag(bounds);
        newDiag *= Montage.sm.getCamData().m_s;
		float factor = newDiag / _oldDiag;
		UsefullEvents.FireUpdateIosShadowScale(gameObject,factor);
	//	_oldDiag = newDiag;

	}
	
	private void RecalcOldDiag()
	{
		BoxCollider box = gameObject.GetComponent<BoxCollider>();
		Bounds bounds = new Bounds (
			box.center, box.extents);
		_oldDiag = _3Dutils.GetBoundDiag(bounds);
		_oldDiag *= Montage.sm.getCamData().m_s;
	}
	
	#region Accessors/Mutators
	
	public float GetLocalW(){return _localWidth;}		//taille d'un module, pieds compris
	public float GetLocalL(){return _localLength;}
	
	public float GetH(){return _height;}
	public float GetW(){return _width;}
	public float GetL(){return _length;}
	
	public float GetFootSize(){return footSize;}
	public float GetFrameSizeH(){return frameSizeH;}
	
	public int GetNbL(){return _nbLengthModule;}
	public int GetNbW(){return _nbWidthModule;}
	
	public GameObject GetFrame(){return _frame;}
	public GameObject GetFrameMesh(){return _frameMesh;}	
	public void SetFrameMesh(GameObject newRef,bool setToDefault)
	{
		if(setToDefault)
			_frameMesh = frameMesh;
		else if(newRef != null)
			_frameMesh = newRef;
		PergolaAutoFeetEvents.FireRebuild();
	}
	public Material GetFrameMat(){return _frame.GetComponent<Renderer>().material;}
	
	public Vector3 GetSavedScale(){return _savedScale;}
	
//	public GameObject GetFeet(){return _feet;}
	public GameObject GetFeetMesh(){return feetMesh;}
	
	public bool GetAddFeetVisibility(){return _isFeetVisible;}
	
	private string GetTypeToString()
	{
		string outStr = "";
		switch (_type)
		{
		case PergolaType.ilot:
			outStr = TextManager.GetText("Pergola.Ilot");
			break;
		case PergolaType.muralLength:
			outStr = TextManager.GetText("Pergola.WallSide")+"\n("+TextManager.GetText("Pergola.Length")+")";
			break;
		case PergolaType.muralWidth:
			outStr = TextManager.GetText("Pergola.WallSide")+"\n("+TextManager.GetText("Pergola.Width")+")";
			break;
		case PergolaType.lN:
			outStr = TextManager.GetText("Pergola.L")+" 2";
			break;
		case PergolaType.lS:
			outStr = TextManager.GetText("Pergola.L")+" 1";
			break;
		case PergolaType.patio:
			outStr = TextManager.GetText("Pergola.Patio");
			break;
		}
		return outStr;
	}
	
	public void SetBladeMinLength(float f){_bladeLengthMax = f;}
	
	private void UpdateType(string s) //Mise a jour du type (si patio > pas de screens)
	{
		switch (s)
		{
		case "ilot":
			_sideRestrictionN = true;
			_sideRestrictionS = true;
			_sideRestrictionE = true;
			_sideRestrictionW = true;
			break;
		case "muralLength":
			_sideRestrictionN = false;
			_sideRestrictionS = true;
			_sideRestrictionE = _useSideOrientation?false:true;
			_sideRestrictionW = true;
			break;
		case "muralWidth":
			_sideRestrictionN = true;
			_sideRestrictionS = _useSideOrientation?false:true;
			_sideRestrictionE = true;
			_sideRestrictionW = false;
			break;
		case "lN":
			_sideRestrictionN = false;
			_sideRestrictionS = true;
			_sideRestrictionE = false;
			_sideRestrictionW = _useSideOrientation?false:true;
			break;
		case "lS":
			_sideRestrictionN = _useSideOrientation?false:true;
			_sideRestrictionS = false;
			_sideRestrictionE = false;
			_sideRestrictionW = true;
			break;
		case "patio":
			_sideRestrictionN = false;
			_sideRestrictionS = false;
			_sideRestrictionE = false;
			_sideRestrictionW = false;
			break;
		}
	}
	
	private void CheckAndRebuild()
	{
		bool rebuild = false;
		if(_uiWidth != (Mathf.RoundToInt(_width*1000)).ToString())
		{
			_width = float.Parse(_uiWidth) / 1000;
			rebuild |= true;
		}
		
		if(_uiLength != (Mathf.RoundToInt(_length*1000)).ToString())
		{
			_length = float.Parse(_uiLength) / 1000;
			rebuild |= true;
		}
		
		if(_uiHeight != (Mathf.RoundToInt(_height*1000)).ToString())
		{
			_height = float.Parse(_uiHeight) / 1000;
			rebuild |= true;
		}
		if(rebuild)
			BuildIt();
	}
	
	#endregion
	
	#region Interface OS3D_Func. Fcns
	
	public string GetFunctionName(){return "PergolaBuilder";}
	
	public string GetFunctionParameterName(){return "Designer";}
	
	public int GetFunctionId(){return os3dFuncId;}
	
	public void DoAction()
	{
		GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);
			
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;
			
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,true);
		Camera.main.GetComponent<ObjInteraction>().setActived(false);	
		
		PergolaAutoFeetEvents.FireNightModeChange(UsefulFunctions.GetMontage().GetSM().IsNightMode());
		
		enabled = true;
		_showUI = true;
		
		UsefullEvents.ShowHelpPanel += Validate;
	}
	
	void Validate()
	{
		Camera.main.GetComponent<ObjInteraction>().configuringObj(null);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().unConfigure();
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().setVisibility (false);
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,false);
		UsefullEvents.ShowHelpPanel -= Validate;
		enabled = false;
	}
	
	//  sauvegarde/chargement
	
	public void save(BinaryWriter buf)
	{
		buf.Write(_length);
		buf.Write(_width);
		buf.Write(_height);
		
		buf.Write((int)_type);
		buf.Write(_isLengthOriented);
	  	buf.Write(_isFeetVisible);
		
		//-----------------OPTIONS-----------------
		buf.Write(_options.Count);
		
		foreach(IPergolaAutoFeet paf in _options)
		{
			buf.Write(paf.GetType().ToString());
		}
		
		foreach(IPergolaAutoFeet paf in _options)
		{
			paf.SaveOption(buf);
		}
	}
	
	public void load(BinaryReader buf)
	{
		_length = buf.ReadSingle();
		_width = buf.ReadSingle();
		_height = buf.ReadSingle();
		
		_uiLength = (Mathf.RoundToInt(_length*1000)).ToString();
		_uiWidth = (Mathf.RoundToInt(_width*1000)).ToString();
		_uiHeight = (Mathf.RoundToInt(_height*1000)).ToString();
		
		_type = (PergolaType) buf.ReadInt32();
		_isLengthOriented = buf.ReadBoolean();
		UpdateBladeDir(_isLengthOriented);
		_isFeetVisible = buf.ReadBoolean();
		
		
		PergolaAutoFeetEvents.FirePergolaTypeChange(_type.ToString());
		
		string version = Montage.cdm.versionSave;
		if(LibraryLoader.numVersionInferieur(version,"1.2.95"))
		{
			PergolaAutoFeetEvents.FireBladesDirChange(_isLengthOriented);
		}
		
		//-----------------OPTIONS-----------------
		int nb = buf.ReadInt32();
		string[] opts = new string[nb];
		for(int i=0;i<nb;i++)
		{
			opts[i] = buf.ReadString();	
		}		
		
		foreach(string type in opts)
		{
			if(!GetComponent(type))
			{
				gameObject.AddComponent<FunctionConf_PergolaAutoFeet>(); 
				//UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(gameObject, "Assets/Scripts/Pergola/PergolaAutomaticFoot/FunctionConf_PergolaAutoFeet.cs (1793,5)", type);
			}



			//GameObject.AddComponent<T>()
			((IPergolaAutoFeet) GetComponent(type)).LoadOption(buf);
		}
		
		BuildIt();
	}
	
	//Set L'ui si besoin
	
	public void setUI(FunctionUI_OS3D ui)
	{
		
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}
		
	//similaire au Save/Load mais utilisé en interne d'un objet a un autre (swap)
	public ArrayList getConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_length);
		al.Add(_width);
		al.Add(_height);
		
		al.Add(_type);
		al.Add(_isLengthOriented);
		al.Add(_isFeetVisible);
		
		al.Add(_oldDiag);
		//-----------------OPTIONS-----------------
		al.Add(_options.Count);
		
		foreach(IPergolaAutoFeet paf in _options)
		{
			al.Add(paf.GetType().ToString());
		}
		
		foreach(IPergolaAutoFeet paf in _options)
		{
			al.Add(paf.GetConfig());
		}
		
		
		return al;
	}
	
	public void setConfig(ArrayList config)
	{
		_firstBuild = true;
//		enabled = true;
		_length = (float)config[0];
		_width = (float)config[1];
		_height = (float)config[2];
		
		_uiLength = (Mathf.RoundToInt(_length*1000)).ToString();
		_uiWidth = (Mathf.RoundToInt(_width*1000)).ToString();
		_uiHeight = (Mathf.RoundToInt(_height*1000)).ToString();

		//_type = (PergolaType) config[3];
		_isLengthOriented = (bool) config[4];//4
		_isFeetVisible = (bool) config[5];//5
		_oldDiag = (float)config[6];
		
		PergolaAutoFeetEvents.FirePergolaTypeChange(_type.ToString());
		PergolaAutoFeetEvents.FireBladesDirChange(_isLengthOriented);
		
		//-----------------OPTIONS-----------------
		int nb = (int) config[7];//6
		string[] opts = new string[nb];
		for(int i=0;i<nb;i++)
		{
			opts[i] = (string) config[8+i];	//7+i
		}		
		
		int tmp = 8+nb;//7+nb
		foreach(string type in opts)
		{
			if(!GetComponent(type))
			{
				gameObject.AddComponent<FunctionConf_PergolaAutoFeet>();
				//UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(gameObject, "Assets/Scripts/Pergola/PergolaAutomaticFoot/FunctionConf_PergolaAutoFeet.cs (1875,5)", type);
			}

			((IPergolaAutoFeet) GetComponent(type)).SetConfig((ArrayList)config[tmp]);
			tmp++;
		}
		
//		CreateArrows();
		
		_firstBuild = false;
		BuildIt();
//		_firstBuild = false;
//		_firstTime = Time.time + 2 * Time.fixedDeltaTime;
	
	}
	#endregion
}
