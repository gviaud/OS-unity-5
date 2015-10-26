using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using Pointcube.Global;using Pointcube.Utils;
public class Function_Pergola : MonoBehaviour,Function_OS3D
{
	#region Attributs
	
	//Structs, Enum, ...
	public struct PElement 
	{
		public int _w;		//PRIS EN COMPTE QUE SI COUPLAGE L
		public int _subW1;	//PRIS EN COMPTE QUE SI COUPLAGE L
		public int _subW2;	//PRIS EN COMPTE QUE SI COUPLAGE L
		public int _l;		//PRIS EN COMPTE QUE SI COUPLAGE W
		public int _h;
		public PergolaType _pergolaType;
		public string _L1;//vv Screens
		public string _L2;
		public string _W1;
		public string _W2;
		public bool _f1;//vv Pieds
		public bool _f2;
		public bool _f3;
		public bool _f4;
		public bool _dpW1;
		public bool _dpW2;
		public bool _dpL1;
		public bool _dpL2;
	}
	
//	[Serializable ]
//	public class BuildDatas
//	{
//		public int _footSize;// = 0120;
//
//		
//		public int _frameH;// = 0210;
//		public int _frameW;// = 0120;
//		public int _frameL;// = 0120;
//		
//		public int _subFrameW;// = 0094;
//		public int _subFrameL;// = 0094;	
//	}
//	
//	[Serializable ]
//	public class PergolaRule
//	{
//		public int singleMaxSize;
//		public int extremMaxSize;
//		public int middleMaxSize;
//		
//		//QUE POUR LES COUPLAGES L
//		public int limit;
//		public int subEMS;
//		public int subMMS;
//		//------------------------
//		
//		public int maxWL; // Valeur max commune a tout les modules
//		public int maxH;
//		
//		public Function_Pergola.PergolaType type;
//		
//		public void save(BinaryWriter buf)
//		{
//			buf.Write(singleMaxSize);
//			buf.Write(extremMaxSize);
//			buf.Write(middleMaxSize);
//			
//			buf.Write(limit);
//			buf.Write(subEMS);
//			buf.Write(subMMS);
//			
//			buf.Write(maxWL);
//			buf.Write(maxH);
//			
//			buf.Write((int)type);
//		}
//		
//		public void load(BinaryReader buf)
//		{
//			singleMaxSize = buf.ReadInt32();
//			extremMaxSize = buf.ReadInt32();
//			middleMaxSize = buf.ReadInt32();
//			
//			limit = buf.ReadInt32();
//			subEMS = buf.ReadInt32();
//			subMMS = buf.ReadInt32();
//			
//			maxWL = buf.ReadInt32();
//			maxH = buf.ReadInt32();
//			
//			type = (Function_Pergola.PergolaType) buf.ReadInt32();
//			
//		}
//	}
	
	public enum PergolaType
	{
		single,
		CL,
		CW
	}
	
	//UI
	private GUISkin _skin;
	private Rect _uiArea;
	
	private bool _showSizes = true;//Par defaut le
	private bool _showFeets = false;
	
	private string _L = "";
	private string _W = "";
	private string _W1 = "";
	private string _W2 = "";
	private string _H = "";
	
	private Vector2 _scrollpos;
	
	private bool _noUI = false;
	
	//Building
	
	public BuildDatas b_datas;
	public PergolaRule b_rules;
	
	private int _lTot;
	private int _wTot;
	private int _hTot;
	private int _index = 0; // Index du module sélectionné
	
	private float _oldDiag = -1;
	
	private List<PElement> _modules = new List<PElement>();
	private ArrayList _options = new ArrayList();
	
	public Material mat_frame;
//	public Material mat_feet;
	
	private GameObject _frame;
	private GameObject _feet;
	private GameObject _f1;
	private GameObject _f2;
	private GameObject _f3;
	private GameObject _f4;
	
	public GameObject BasePrimitive;
	
	private Quaternion _savedRotation;
	private Vector3 _savedScale;
	
	private PElement _currentModule;
	
	private bool _globalVisibility = true;
	
	//OS3D Function
	public int id;
	
	#endregion

    void OnEnable()
    {
        UsefullEvents.ScaleChange += ScaleChange;
    }

    void OnDisable()
    {
       UsefullEvents.ScaleChange -= ScaleChange;
    }

	// Use this for initialization
	void Start ()
	{
		
		Color c = mat_frame.color;
	//	mat_frame = new Material(Shader.Find("Custom/2Sided"));
	//	mat_frame.color = c;
		
		foreach(Component cp in this.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType() != this.GetType() && cp.GetType().GetInterface("IPergola")!= null)
			{
				_options.Add(cp);
			}
		}
		
		//---------------------------------UI-----------------------------------
		_uiArea = new Rect(Screen.width-280, 0, 300, Screen.height);
		if(_skin == null)
			_skin = (GUISkin)Resources.Load("skins/PergolaSkin");
		
		//--------------------------Set main SubObjects-------------------------
		SaveTransform();
		
		//Set main SubObjects		
		if(!transform.FindChild("frame"))
		{
			_frame = new GameObject("frame");
			_frame.transform.parent = this.transform;
			_frame.transform.localPosition = Vector3.zero;
			
			_frame.AddComponent<MeshRenderer>();
			_frame.GetComponent<Renderer>().material = mat_frame;
		}
		else
		{
			_frame =	transform.FindChild("frame").gameObject;
			_frame.GetComponent<Renderer>().material = mat_frame;
		}
		
		if(!transform.FindChild("feet"))
		{
			_feet = new GameObject("feet");
			_feet.transform.parent = this.transform;
			_feet.transform.localPosition = Vector3.zero;
			
			_feet.AddComponent<MeshRenderer>();
			_feet.GetComponent<Renderer>().material = _frame.GetComponent<Renderer>().material;
		}
		else
		{
			_feet =	transform.FindChild("feet").gameObject;
			_feet.GetComponent<Renderer>().material = _frame.GetComponent<Renderer>().material;
		}
		
		ReapplyTransform();
		
		//---------------------------------------------------------------------------
		CreateDefautElement();
		
		//enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//--vv-- Update Pergola --vv--
		if(!ComparePElements(_currentModule,/*(PElement)*/_modules[_index]))
		{
			UpdatePergola();	
		}
		
		//--vv-- INTERACTION --vv--
        if(PC.In.Click1Up() && !PC.In.CursorOnUI(_uiArea))
            Validate();

        float deltaScroll;
        if(PC.In.ScrollViewV(out deltaScroll) && PC.In.CursorOnUI(_uiArea))
            _scrollpos.y += deltaScroll;

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
//			if(_uiArea.Contains(t.position) && t.deltaPosition.y!=0)
//			{
//				_scrollpos.y = _scrollpos.y + t.deltaPosition.y;
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
	
	void OnGUI()
	{
		GUI.skin = _skin;
		GUI.Box(new Rect(Screen.width-280.0f,0.0f,Screen.width-280.0f,Screen.height),"","BackGround");
		//-----------FEET MARKS-------------------------------
		if(_showFeets && _f1 != null)
		{
			Vector3 LblPos = Camera.main.WorldToScreenPoint(_f1.transform.position);
			GUI.Box(new Rect(LblPos.x-20,Screen.height - LblPos.y-20,40,40),"1","feetmark");
			LblPos = Camera.main.WorldToScreenPoint(_f2.transform.position);
			GUI.Box(new Rect(LblPos.x-20,Screen.height - LblPos.y-20,40,40),"2","feetmark");
			LblPos = Camera.main.WorldToScreenPoint(_f3.transform.position);
			GUI.Box(new Rect(LblPos.x-20,Screen.height - LblPos.y-20,40,40),"3","feetmark");
			LblPos = Camera.main.WorldToScreenPoint(_f4.transform.position);
			GUI.Box(new Rect(LblPos.x-20,Screen.height - LblPos.y-20,40,40),"4","feetmark");
		}
		
		//--------------Configurateur-------------------------
		GUILayout.BeginArea(_uiArea);
		GUILayout.FlexibleSpace();
		
		_scrollpos = GUILayout.BeginScrollView(_scrollpos,"empty","empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
		
		GUILayout.Box("","UP",GUILayout.Width(280),GUILayout.Height(150));//fade en haut
		GUILayout.BeginVertical("MID");
		
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
		
		//-----------GESTION DES MODULES--------------------
		if(b_rules.type != PergolaType.single)
		{
			UIModulesSelector();
			UIModulesAddRemove();
		}
		//-----------GESTION DES TAILLES--------------------
		_showSizes = GUILayout.Toggle(_showSizes,TextManager.GetText("Pergola.Sizes"),GUILayout.Height(50),GUILayout.Width(280));
		if(_showSizes)
		{			
			UISizesSetting();
		}
		//-----------GESTION DES PIEDS----------------------
		_showFeets = GUILayout.Toggle(_showFeets,TextManager.GetText("Pergola.Feet"),GUILayout.Height(50),GUILayout.Width(280));
		if(_showFeets)
		{
			UIFeetControl();
			if(_f1 == null)
			{
				_f1 = _feet.transform.FindChild(((_index+1)*10+1).ToString()).gameObject;
				_f2 = _feet.transform.FindChild(((_index+1)*10+2).ToString()).gameObject;
				_f3 = _feet.transform.FindChild(((_index)*10+1).ToString()).gameObject;
				_f4 = _feet.transform.FindChild(((_index)*10+2).ToString()).gameObject;	
			}
		}
		else
		{
			if(_f1 != null)
			{
				_f1 = null;
				_f2 = null;
				_f3 = null;
				_f4 = null;	
			}
		}
		
		//-----------Options's UI-------------------------------
		foreach(IPergola ip in _options)
		{
			ip.GetUI(_skin);
		}
		//-----------FADE BAS-------------------------------
		GUILayout.EndVertical();
		GUILayout.Box("","DWN",GUILayout.Width(280),GUILayout.Height(150));//fade en bas
		
		GUILayout.EndScrollView();
		GUILayout.FlexibleSpace();
		
		GUILayout.EndArea();
	}
	
	#region UI Fcn's
	
	void UIFeetControl()
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		int tmpCrc = CalcFeetCrc();
		
		_currentModule._f1 = GUILayout.Toggle(_currentModule._f1,"1","Button",GUILayout.Width(50),GUILayout.Height(50));
		_currentModule._f2 = GUILayout.Toggle(_currentModule._f2,"2","Button",GUILayout.Width(50),GUILayout.Height(50));
		_currentModule._f3 = GUILayout.Toggle(_currentModule._f3,"3","Button",GUILayout.Width(50),GUILayout.Height(50));
		_currentModule._f4 = GUILayout.Toggle(_currentModule._f4,"4","Button",GUILayout.Width(50),GUILayout.Height(50));
		
		if(tmpCrc != CalcFeetCrc())
			UpdateFeet();
		GUILayout.Space(20);
		GUILayout.EndHorizontal();	
	}
	void UIModulesSelector()
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		if(GUILayout.Button("Prv","btn<",GUILayout.Height(50),GUILayout.Width(50)))
		{
			if(_index-1 >=0)
			{
				_index --;
				_currentModule = /*(Module)*/_modules[_index];
				UpdateDisplay();
				_f1 = null;
			}
		}
		GUILayout.Space(10);
		GUILayout.Label(TextManager.GetText("Pergola.Module")+(_index+1)+" / "+_modules.Count,GUILayout.Height(50));
		GUILayout.Space(10);
		if(GUILayout.Button("Nxt","btn>",GUILayout.Height(50),GUILayout.Width(50)))
		{
			if(_modules.Count > _index+1)
			{
				_index ++;
				_currentModule = /*(Module)*/_modules[_index];
				UpdateDisplay();
				_f1 = null;
			}
		}
		
		GUILayout.Space(20);
		GUILayout.EndHorizontal();	
	}
	void UIModulesAddRemove()
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		GUILayout.Space(25);
		
		if(GUILayout.Button(TextManager.GetText("Pergola.Add"),GUILayout.Height(50),GUILayout.Width(75)))
		{
			PElement m = new PElement();//_currentModule;
			
			m._l = _currentModule._l;
			m._w = _currentModule._w;
			m._subW1 = _currentModule._subW1;
			m._subW2 = _currentModule._subW2;
			m._h = _currentModule._h;
			
			m._pergolaType = b_rules.type;
			
			m._L1 = "none";
			m._L2 = "none";
			m._W1 = "none";
			m._W2 = "none";
			
			m._f1 = true;
			m._f2 = true;
			m._f3 = true;
			m._f4 = true;
			
			m._dpW1 = false;
			m._dpW2 = false;
			m._dpL1 = false;
			m._dpL2 = false;
			
//			if(_modules.Count == 1)
//				CheckScreens();
			if(GetComponent<PergolaScreens>())
			{
				PElement me;
				switch (b_rules.type)
				{
				case PergolaType.CL:
					if(_modules[0]._L1 != GetComponent<PergolaScreens>().GetScreensType()[0].name)
					{
						me = _modules[0];
						me._L1 = GetComponent<PergolaScreens>().GetScreensType()[0].name;
						_modules[0]=me;
					}
					break;
					
				case PergolaType.CW:
					if(_modules[0]._W2 != GetComponent<PergolaScreens>().GetScreensType()[0].name)
					{
						me = _modules[0];
						me._W2 = GetComponent<PergolaScreens>().GetScreensType()[0].name;
						_modules[0]=me;
					}
					break;			
				}	
			}
			
			if(GetComponent<PergolaDoorProfile>())
			{
				PElement me;
				switch (b_rules.type)
				{
				case PergolaType.CL:
					if(_modules[0]._dpL1)
					{
						me = _modules[0];
						me._dpL1 = false;
						_modules[0]=me;
					}
					break;
					
				case PergolaType.CW:
					if(_modules[0]._dpW2)
					{
						me = _modules[0];
						me._dpW2 = false;
						_modules[0]=me;
					}
					break;			
				}	
			}
			
			_modules.Add(m);
			_index = _modules.Count-1;
			_currentModule = _modules[_index];
			UpdatePergola();
			UpdateDisplay();
			
		}
		
		GUILayout.Space(50);
		
		if(GUILayout.Button(TextManager.GetText("Pergola.Remove"),GUILayout.Height(50),GUILayout.Width(75)))
		{
			if(_index != 0)
			{
				_modules.RemoveAt(_index);
				_index = 0;
				_currentModule = _modules[_index];
				UpdateDisplay();
				UpdatePergola();
			}
			
		}
		
//		GUILayout.Space(20);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();	
	}
	void UISizesSetting()
	{
		if(_modules.Count == 1)
		{
			switch (b_rules.type)
			{
			case PergolaType.single:
				UI3SizesBloc(b_rules.maxWL,b_rules.singleMaxSize,b_rules.subEMS,b_rules.subEMS,b_rules.maxH);
				break;
			case PergolaType.CL:
				UI3SizesBloc(b_rules.maxWL,b_rules.singleMaxSize,b_rules.subEMS,b_rules.subEMS,b_rules.maxH);
				break;
			case PergolaType.CW:
				UI3SizesBloc(b_rules.singleMaxSize,b_rules.maxWL,b_rules.subEMS,b_rules.subEMS,b_rules.maxH);
				break;
			}
		}
		else if(_modules.Count>1)
		{
			if(_index == 0)//1er
			{
				if(b_rules.type == PergolaType.CL)
				{
					UI3SizesBloc(b_rules.maxWL,b_rules.extremMaxSize,b_rules.subEMS,b_rules.subMMS,b_rules.maxH);
				}
				else //PergolaType.CW
				{
					UI3SizesBloc(b_rules.extremMaxSize,b_rules.maxWL,0,0,b_rules.maxH);
				}
			}
			else if(_index == _modules.Count-1)//dernier
			{
				if(b_rules.type == PergolaType.CL)
				{
					UI3SizesBloc(b_rules.maxWL,b_rules.extremMaxSize,b_rules.subMMS,b_rules.subEMS,b_rules.maxH);
				}
				else //PergolaType.CW
				{
					UI3SizesBloc(b_rules.extremMaxSize,b_rules.maxWL,0,0,b_rules.maxH);
				}
			}
			else//milieux
			{
				if(b_rules.type == PergolaType.CL)
				{
					UI3SizesBloc(b_rules.maxWL,b_rules.middleMaxSize,b_rules.subMMS,b_rules.subMMS,b_rules.maxH);
				}
				else //PergolaType.CW
				{
					UI3SizesBloc(b_rules.middleMaxSize,b_rules.maxWL,0,0,b_rules.maxH);
				}
			}
		}
	}
	void UI3SizesBloc(int lMax,int wMax,int w1Max,int w2Max,int hMax)
	{
		int localLMax = lMax;
		int localWMax = wMax;
		
		if(GetComponent<PergolaScreens>())
		{
			int screenW1;
			int screenW2;
			screenW1 = GetComponent<PergolaScreens>().GetScreenType(_currentModule._W1).maxSize;
			if(screenW1 == 0)
				screenW1 = wMax;
			screenW2 = GetComponent<PergolaScreens>().GetScreenType(_currentModule._W2).maxSize;
			if(screenW2 == 0)
				screenW2 = wMax;
			
			int screenW = Mathf.Min(screenW1,screenW2);
			localWMax = Mathf.Min(screenW,wMax);
			
			int screenL1;
			int screenL2;
			screenL1 = GetComponent<PergolaScreens>().GetScreenType(_currentModule._L1).maxSize;
			if(screenL1 == 0)
				screenL1 = lMax;
			screenL2 = GetComponent<PergolaScreens>().GetScreenType(_currentModule._L2).maxSize;
			if(screenL2 == 0)
				screenL2 = lMax;
			int screenL = Mathf.Min(screenL1,screenL2);
			localLMax = Mathf.Min(screenL,lMax);
		}
		
		UISizeSetting(TextManager.GetText("Pergola.Length")+"\n(mm)\n"+localLMax+"max",ref _L,ref _currentModule._l,b_rules.minL,localLMax);
		UISizeSetting(TextManager.GetText("Pergola.Width")+"\n(mm)\n"+localWMax+"max",ref _W,ref _currentModule._w,b_rules.minW,localWMax);
		
		if(_currentModule._w > b_rules.limit && b_rules.type != PergolaType.CW)
		{
			if(_currentModule._subW1 == 0 || (_currentModule._subW1 + _currentModule._subW2) != _currentModule._w)
			{
				_currentModule._subW1 = _currentModule._w/2;
				_currentModule._subW2 = _currentModule._w/2;
				_W1 =_currentModule._subW1.ToString();
				_W2 =_currentModule._subW2.ToString();	
			}
			
			int sub1 = _currentModule._subW1;
			bool s1 = false;
			UISizeSetting(TextManager.GetText("Pergola.Width")+"1\n(mm)\n"+w1Max+"max",ref _W1,ref sub1,0,w1Max);
			if(sub1+_currentModule._subW2 != _currentModule._w)
			{
				int tmp = _currentModule._w - sub1;
				if(tmp <= w2Max)
				{
					s1 = true;
					_currentModule._subW1 = sub1;
					_currentModule._subW2 = tmp;
					_W2 =_currentModule._subW2.ToString();
				}
			}
			
			int sub2 = _currentModule._subW2;
			UISizeSetting(TextManager.GetText("Pergola.Width")+"2\n(mm)\n"+w2Max+"max",ref _W2,ref sub2,0,w2Max);
			if(sub2+_currentModule._subW1 != _currentModule._w && !s1)
			{
				int tmp = _currentModule._w - sub2;
				if(tmp <= w1Max)
				{
					_currentModule._subW2 = sub2;
					_currentModule._subW1 = tmp;
					_W1 =_currentModule._subW1.ToString();
				}
			}
			
		}
		UISizeSetting(TextManager.GetText("Pergola.Height")+"\n(mm)\n"+hMax+"max",ref _H,ref _currentModule._h,0,hMax);
	}
	void UISizeSetting(string txt,ref string valueStr,ref int val,int min,int max)
	{
		if(_noUI)
		{
			if(val.ToString() != valueStr)
			{
				if(int.TryParse(valueStr,out val))
				{
					val = val;
				}
				else
					val = 0;
				
				val = Mathf.Clamp(val,min,max);
				valueStr = val.ToString();
				
	//			valueStr = tmp;
				//DoSomething
			}
		}
		else
		{
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("+","btn+",GUILayout.Height(50),GUILayout.Width(50)))
			{
				val += 100;
				val = Mathf.Clamp(val,min,max);
				valueStr = val.ToString();
				//DoSomething
			}
			
	//		string tmp = GUILayout.TextField(valueStr,GUILayout.Height(50),GUILayout.Width(50));
	//		if(tmp != valueStr)
	//		{
	//			if(int.TryParse(tmp,out val))
	//			{
	//				val = val;
	//			}
	//			else
	//				val = 0;
	//			val = Mathf.Clamp(val,min,max);
	//			valueStr = val.ToString();
	//			
	////			valueStr = tmp;
	//			//DoSomething
	//		}
	#if UNITY_IPHONE || UNITY_ANDROID
			string tmp = GUILayout.TextField(valueStr,GUILayout.Height(50),GUILayout.Width(50));
			if(tmp != valueStr)
			{
				if(int.TryParse(tmp,out val))
				{
					val = val;
				}
				else
					val = 0;
				val = Mathf.Clamp(val,min,max);
				valueStr = val.ToString();
				
	//			valueStr = tmp;
				//DoSomething
			}
#else
	
			//XX--XX--XX--XX--XX--XX
			GUI.SetNextControlName(txt);
			valueStr = GUILayout.TextField(valueStr,GUILayout.Height(50),GUILayout.Width(50));
			if((GUI.GetNameOfFocusedControl() != txt || Event.current.keyCode == KeyCode.Return) && val.ToString() != valueStr)
			{
				if(int.TryParse(valueStr,out val))
				{
					val = val;
				}
				else
					val = 0;
				
				val = Mathf.Clamp(val,min,max);
				valueStr = val.ToString();
				
	//			valueStr = tmp;
				//DoSomething
			}
			//XX--XX--XX--XX--XX--XX
#endif	
			
			if(GUILayout.Button("-","btn-",GUILayout.Height(50),GUILayout.Width(50)))
			{
				val -= 100;
				val = Mathf.Clamp(val,min,max);
				valueStr =val.ToString();
				//DoSomething
			}
			GUILayout.Label(txt,GUILayout.Width(60));
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}
	}
	
	void UpdateDisplay()
	{
		_W = _currentModule._w.ToString();
		_W1= _currentModule._subW1.ToString();
		_W2= _currentModule._subW2.ToString();
		_L = _currentModule._l.ToString();
		_H = _currentModule._h.ToString();				
	}
	
	#endregion
	
	#region other Fcn's
	
	void Validate()
	{
		UpdateAndCheckSizes();
		
		Camera.main.GetComponent<ObjInteraction>().configuringObj(null);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().unConfigure();
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().setVisibility (false);
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,false);		
		enabled = false;	
	}
	
	public int GetModulesCount()
	{
		return _modules.Count;	
	}
	
	public GameObject GetFrame()
	{
		return _frame;	
	}
	
	public PElement GetCurrentModule()
	{
		return _currentModule;	
	}
	
	public PElement GetModule(int index)
	{
		return _modules[index];	
	}
	
	public void SetCurrentModule(PElement e)
	{
		_currentModule = e;
	}
	
	public int GetHeight()
	{
		return _modules[0]._h;	
	}
	
	public int GetModuleIndex()
	{
		return _index;	
	}
	
	public PergolaRule GetRule()
	{
		return b_rules;	
	}
	
	public BuildDatas GetBuildDatas()
	{
		return b_datas;	
	}
	
	public Material GetMaterial()
	{
		return _frame.GetComponent<Renderer>().material;
	}
	
	#endregion
	
	#region Building Fcn's
	
	public void UpdatePergola()
	{
		
		//--vv-- UPDATE VALEURS GLOBALES --vv--
		if(b_rules.type == PergolaType.CL)
		{
			if(_currentModule._l != (_modules[_index])._l)
			{
				int size = _currentModule._l;
				
				for(int i=0;i<_modules.Count;i++)
				{
					PElement m = _modules[i];
					m._l = size;
					_modules[i] = m;
				}
			}
		}
		else if(b_rules.type == PergolaType.CW)
		{
			if(_currentModule._w != (_modules[_index])._w)
			{
				int size = _currentModule._w;
				
				for(int i=0;i<_modules.Count;i++)
				{
					PElement m = _modules[i];
					m._w = size;
					_modules[i] = m;
				}
			}
		}
		
		if(_currentModule._h != (_modules[_index])._h)
		{
			int size = _currentModule._h;
			
			for(int i=0;i<_modules.Count;i++)
			{
				PElement m = _modules[i];
				m._h = size;
				_modules[i] = m;
			}
		}
		
		//--vv-- MAJ CURRENT MODULE --vv--
		_modules[_index] = _currentModule;
		
		ClearPergola();
		
		SaveTransform();
		
		//--vv-- CALCUL OFFSET --vv--
		int off7=0;
		for(int i=0;i<_modules.Count;i++)
		{
			PElement m = /*(Module)*/_modules[i];
			if(m._pergolaType == PergolaType.CL || m._pergolaType == PergolaType.single)
			{
				off7 += m._w;
				_wTot = off7;
				_lTot = m._l;
				_hTot = m._h;
			}
			else// if(m._pergolaType == PergolaType.CW)
			{
				off7 += m._l;
				_wTot = m._w;
				_lTot = off7;
				_hTot = m._h;
			}
		}
		
		off7 = -off7/2;
		
		//--vv-- BUILDING --vv--
		for(int i=0;i<_modules.Count;i++)
		{
			PElement m = /*(Module)*/_modules[i];
			BuildFeet(m,i,off7);
			BuildFrame(m,i,off7);
			
			foreach(IPergola ip in _options)
			{
				ip.Build(m,b_datas,b_rules,i,off7);
			}
			
			if(m._pergolaType == PergolaType.CL)
				off7 += m._w;
			else if(m._pergolaType == PergolaType.CW)
				off7 += m._l;
		}
//		UpdateBladesOrientation();
		ReapplyTransform();
		
		CreateBounds();
	}
	
	void BuildFeet(PElement m,int index,int off7)
	{
		if(m._pergolaType == PergolaType.CL || m._pergolaType == PergolaType.single)
		{
			if(index == 0)//1er
			{
				_globalVisibility = _modules[index]._f3;
				AddPartAt(new Vector3(off7+b_datas._footSize/2,m._h/2-b_datas._frameH/2, m._l/2-b_datas._footSize/2),
							new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._footSize),_feet,index+1);
				_globalVisibility = _modules[index]._f4;
				AddPartAt(new Vector3(off7+b_datas._footSize/2,m._h/2-b_datas._frameH/2, -m._l/2+b_datas._footSize/2),
							new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._footSize),_feet,index+2);
			}
			if(!(index == 0))//milieux
			{
				_globalVisibility = _modules[index]._f3;
				AddPartAt(new Vector3(off7,m._h/2-b_datas._frameH/2, m._l/2-b_datas._footSize/2),
						new Vector3(b_datas._subFrameW,m._h-b_datas._frameH,b_datas._footSize),_feet,index*10+1);
				_globalVisibility = _modules[index]._f4;
				AddPartAt(new Vector3(off7,m._h/2-b_datas._frameH/2, -m._l/2+b_datas._footSize/2),
						new Vector3(b_datas._subFrameW,m._h-b_datas._frameH,b_datas._footSize),_feet,index*10+2);
			}
			if(index == _modules.Count-1)//dernier
			{
				_globalVisibility = _modules[index]._f1;
				AddPartAt(new Vector3(off7+m._w-b_datas._footSize/2,m._h/2-b_datas._frameH/2, m._l/2-b_datas._footSize/2),
					new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._footSize),_feet,(index+1)*10+1);
				_globalVisibility = _modules[index]._f2;
				AddPartAt(new Vector3(off7+m._w-b_datas._footSize/2,m._h/2-b_datas._frameH/2, -m._l/2+b_datas._footSize/2),
					new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._footSize),_feet,(index+1)*10+2);
			}
		}
		else// if(m._pergolaType == PergolaType.CW)
		{
			if(index == 0)//1er
			{
				_globalVisibility = _modules[index]._f3;
				AddPartAt(new Vector3(m._w/2-b_datas._footSize/2,m._h/2-b_datas._frameH/2,off7+b_datas._footSize/2),
							new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._footSize),_feet,index+1);
				_globalVisibility = _modules[index]._f4;
				AddPartAt(new Vector3(-m._w/2+b_datas._footSize/2,m._h/2-b_datas._frameH/2,off7+b_datas._footSize/2),
							new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._footSize),_feet,index+2);
			}
			if(!(index == 0))//milieux
			{
				_globalVisibility = _modules[index]._f3;
				AddPartAt(new Vector3(m._w/2-b_datas._footSize/2,m._h/2-b_datas._frameH/2, off7),
						new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._subFrameW),_feet,index*10+1);
				_globalVisibility = _modules[index]._f4;
				AddPartAt(new Vector3(-m._w/2+b_datas._footSize/2,m._h/2-b_datas._frameH/2, off7),
						new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._subFrameW),_feet,index*10+2);
			}
			if(index == _modules.Count-1)//dernier
			{
				_globalVisibility = _modules[index]._f1;
				AddPartAt(new Vector3(m._w/2-b_datas._footSize/2,m._h/2-b_datas._frameH/2, off7+m._l-b_datas._footSize/2),
					new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._footSize),_feet,(index+1)*10+1);
				_globalVisibility = _modules[index]._f2;
				AddPartAt(new Vector3(-m._w/2+b_datas._footSize/2,m._h/2-b_datas._frameH/2, off7+m._l-b_datas._footSize/2),
					new Vector3(b_datas._footSize,m._h-b_datas._frameH,b_datas._footSize),_feet,(index+1)*10+2);
			}
		}
		_globalVisibility = true;
	}
	void BuildFrame(PElement m,int index,int off7)
	{
		int h = m._h - b_datas._frameH/2;
		if(m._pergolaType == PergolaType.CL || m._pergolaType == PergolaType.single)
		{
			int sizeL = m._l - 2*b_datas._footSize;
			
			if(index == 0)//1er
			{
				AddPartAt(new Vector3(off7+b_datas._footSize/2,h,0),new Vector3(b_datas._footSize,b_datas._frameH,sizeL),_frame,index);
			}
			if(!(index == _modules.Count-1))//milieux
			{
				AddPartAt(new Vector3(off7+m._w/2,h,m._l/2-b_datas._footSize/2),new Vector3(m._w,b_datas._frameH,b_datas._footSize),_frame,index);
				AddPartAt(new Vector3(off7+m._w/2,h,-m._l/2+b_datas._footSize/2),new Vector3(m._w,b_datas._frameH,b_datas._footSize),_frame,index);
				
				AddPartAt(new Vector3(off7+m._w,h,0),new Vector3(b_datas._subFrameW,b_datas._frameH,sizeL),_frame,index);
			}
			if(index == _modules.Count-1)//dernier
			{
				AddPartAt(new Vector3(off7+m._w/2,h,m._l/2-b_datas._footSize/2),new Vector3(m._w,b_datas._frameH,b_datas._footSize),_frame,index);
				AddPartAt(new Vector3(off7+m._w/2,h,-m._l/2+b_datas._footSize/2),new Vector3(m._w,b_datas._frameH,b_datas._footSize),_frame,index);
				
				AddPartAt(new Vector3(off7+m._w-b_datas._footSize/2,h,0),new Vector3(b_datas._footSize,b_datas._frameH,sizeL),_frame,index);
			}
		}
		else// if(m._pergolaType == PergolaType.CW)
		{
			int sizeW = m._w - 2*b_datas._footSize;
			
			if(index == 0)//1er
			{
				AddPartAt(new Vector3(0,h,off7+b_datas._footSize/2),new Vector3(sizeW,b_datas._frameH,b_datas._footSize),_frame,index);
			}
			if(!(index == _modules.Count-1))//milieux
			{
				AddPartAt(new Vector3(m._w/2-b_datas._footSize/2,h,off7+m._l/2),new Vector3(b_datas._footSize,b_datas._frameH,m._l),_frame,index);
				AddPartAt(new Vector3(-m._w/2+b_datas._footSize/2,h,off7+m._l/2),new Vector3(b_datas._footSize,b_datas._frameH,m._l),_frame,index);
				
				AddPartAt(new Vector3(0,h,off7+m._l),new Vector3(sizeW,b_datas._frameH,b_datas._subFrameW),_frame,index);
			}
			if(index == _modules.Count-1)//dernier
			{
				AddPartAt(new Vector3(m._w/2-b_datas._footSize/2,h,off7+m._l/2),new Vector3(b_datas._footSize,b_datas._frameH,m._l),_frame,index);
				AddPartAt(new Vector3(-m._w/2+b_datas._footSize/2,h,off7+m._l/2),new Vector3(b_datas._footSize,b_datas._frameH,m._l),_frame,index);
				
				AddPartAt(new Vector3(0,h,off7+m._l-b_datas._footSize/2),new Vector3(sizeW,b_datas._frameH,b_datas._footSize),_frame,index);
			}
		}
	}
	
	void AddPartAt(Vector3 position, Vector3 scale,GameObject p,int i)
	{
		AddPartAt(position, scale,p,i,BasePrimitive,0);
	}
	void AddPartAt(Vector3 position, Vector3 scale,GameObject p,int i,GameObject refObj,float angleY)
	{
		Vector3 floatScl = scale/1000f;
		Vector3 floatPos = position/1000f;
		
		GameObject part = (GameObject) Instantiate(refObj);
		part.name = i.ToString();
		part.transform.parent = p.transform;
		part.layer = gameObject.layer;
		if(part.transform.GetChildCount() == 0 && part.GetComponent<Renderer>())
			part.GetComponent<Renderer>().material = _frame.GetComponent<Renderer>().material;//p.renderer.material;
		else
		{
			foreach(Transform t in part.transform)
			{
				if(t.GetComponent<Renderer>())
					t.GetComponent<Renderer>().material = _frame.GetComponent<Renderer>().material;//p.renderer.material;
			}
		}
		part.transform.localPosition = floatPos;
		part.transform.localScale = floatScl;
		
		Quaternion q = part.transform.localRotation;
		Vector3 angles = q.eulerAngles;
		angles.y += angleY;
		q.eulerAngles = angles;
		part.transform.localRotation = q;
		
		part.GetComponent<Renderer>().enabled = _globalVisibility;
	}
	
	void ClearPergola()
	{
		foreach(Transform t in _feet.transform)
		{
			Destroy(t.gameObject);
		}
		foreach(Transform t in _frame.transform)
		{
			Destroy(t.gameObject);
		}
		foreach(IPergola ip in _options)
		{
			ip.Clear();
		}
	}
	
	void UpdateFeet()
	{	
		
		_f1.GetComponent<Renderer>().enabled = _currentModule._f1;
		_f2.GetComponent<Renderer>().enabled = _currentModule._f2;
		_f3.GetComponent<Renderer>().enabled = _currentModule._f3;
		_f4.GetComponent<Renderer>().enabled = _currentModule._f4;
		
		_modules[_index] = _currentModule;
		
		if(_index != 0)
		{
			PElement pm = _modules[_index-1];
			pm._f1 = _currentModule._f3;
			pm._f2 = _currentModule._f4;
			_modules[_index-1] = pm;
		}
		if(_index != _modules.Count-1)
		{
			PElement nm = _modules[_index+1];
			nm._f3 = _currentModule._f1;
			nm._f4 = _currentModule._f2;
			_modules[_index+1] = nm;
		}
		
	}
	
	void CreateDefautElement()
	{		
		_currentModule._w = 2500;
		_W = _currentModule._w.ToString();
		_currentModule._l = 4000;
		_L = _currentModule._l.ToString();
		_currentModule._subW1 = 0;
		_currentModule._subW2 = 0;
		_currentModule._h = 2500;
		_H = _currentModule._h.ToString();
		_currentModule._pergolaType = b_rules.type;
		
		_currentModule._f1 = true;
		_currentModule._f2 = true;
		_currentModule._f3 = true;
		_currentModule._f4 = true;
		
		_currentModule._dpW1 = false;
		_currentModule._dpW2 = false;
		_currentModule._dpL1 = false;
		_currentModule._dpL2 = false;
		
		_currentModule._L1 = "none";
		_currentModule._L2 = "none";
		_currentModule._W1 = "none";
		_currentModule._W2 = "none";
		
		_modules.Add(_currentModule);
		
		UpdatePergola();
		
		enabled = false;
	}
	
	void SaveTransform()
	{
		_savedRotation = transform.localRotation;
		_savedScale = transform.localScale;
		
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
	
	void ReapplyTransform()
	{
		transform.localRotation = _savedRotation;
		transform.localScale = _savedScale;
		
		foreach(Transform t in transform)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
		}
	}
	
	bool ComparePElements(PElement m1, PElement m2)
	{
		bool b = true;
		if(m1._w != m2._w)
			b=false;
		if(m1._subW1 != m2._subW1)
			b=false;
		if(m1._subW2 != m2._subW2)
			b=false;
		if(m1._l != m2._l)
			b=false;
		if(m1._h != m2._h)
			b=false;
		return b;
	}
	
	void CreateBounds()
	{
		if(!GetComponent<BoxCollider>())
			gameObject.AddComponent<BoxCollider>();
		
		gameObject.GetComponent<BoxCollider>().center = new Vector3(0,_hTot/2,0)/1000f;
		gameObject.GetComponent<BoxCollider>().size = new Vector3(_wTot,_hTot,_lTot)/1000f;
		
		if(_oldDiag == -1)
		{
             _oldDiag = _3Dutils.GetBoundDiag(gameObject.GetComponent<BoxCollider>().bounds);
             _oldDiag *= Montage.sm.getCamData().m_s;
		}
		else
		{
			ScaleIosShadows();
		}
	}
	
	private void ScaleIosShadows()
	{
		float newDiag = _3Dutils.GetBoundDiag(gameObject.GetComponent<BoxCollider>().bounds);
		float factor = newDiag / _oldDiag;
		UsefullEvents.FireUpdateIosShadowScale(gameObject,factor);		
	}

    private void ScaleChange()  // Appelé par l'événement UsefulEvent.ScaleChange
    {
         _oldDiag = _3Dutils.GetBoundDiag(gameObject.GetComponent<BoxCollider>().bounds);
    }


	int CalcFeetCrc()
	{
		int i = 0;
		i = (_currentModule._f1 ? 1:0);
		i += (_currentModule._f2 ? 1:0);
		i += (_currentModule._f3 ? 1:0);
		i += (_currentModule._f4 ? 1:0);
		return i;
	}
	
	public void CheckSizes()
	{
		if(_modules.Count == 1)
		{
			switch (b_rules.type)
			{
			case PergolaType.single:
				CheckSize(b_rules.maxWL,b_rules.singleMaxSize);
				break;
			case PergolaType.CL:
				CheckSize(b_rules.maxWL,b_rules.singleMaxSize);
				break;
			case PergolaType.CW:
				CheckSize(b_rules.singleMaxSize,b_rules.maxWL);
				break;
			}
		}
		else if(_modules.Count>1)
		{
			if(_index == 0)//1er
			{
				if(b_rules.type == PergolaType.CL)
				{
					CheckSize(b_rules.maxWL,b_rules.extremMaxSize);
				}
				else //PergolaType.CW
				{
					CheckSize(b_rules.extremMaxSize,b_rules.maxWL);
				}
			}
			else if(_index == _modules.Count-1)//dernier
			{
				if(b_rules.type == PergolaType.CL)
				{
					CheckSize(b_rules.maxWL,b_rules.extremMaxSize);
				}
				else //PergolaType.CW
				{
					CheckSize(b_rules.extremMaxSize,b_rules.maxWL);
				}
			}
			else//milieux
			{
				if(b_rules.type == PergolaType.CL)
				{
					CheckSize(b_rules.maxWL,b_rules.middleMaxSize);
				}
				else //PergolaType.CW
				{
					CheckSize(b_rules.middleMaxSize,b_rules.maxWL);
				}
			}
		}
	}
	void CheckSize(int lMax,int wMax)
	{
		int localLMax = lMax;
		int localWMax = wMax;
		
		if(GetComponent<PergolaScreens>())
		{
			int screenW1;
			int screenW2;
			screenW1 = GetComponent<PergolaScreens>().GetScreenType(_currentModule._W1).maxSize;
			if(screenW1 == 0)
				screenW1 = wMax;
			screenW2 = GetComponent<PergolaScreens>().GetScreenType(_currentModule._W2).maxSize;
			if(screenW2 == 0)
				screenW2 = wMax;
			
			int screenW = Mathf.Min(screenW1,screenW2);
			localWMax = Mathf.Min(screenW,wMax);
			
			int screenL1;
			int screenL2;
			screenL1 = GetComponent<PergolaScreens>().GetScreenType(_currentModule._L1).maxSize;
			if(screenL1 == 0)
				screenL1 = lMax;
			screenL2 = GetComponent<PergolaScreens>().GetScreenType(_currentModule._L2).maxSize;
			if(screenL2 == 0)
				screenL2 = lMax;
			int screenL = Mathf.Min(screenL1,screenL2);
			localLMax = Mathf.Min(screenL,lMax);
		}
		
		_currentModule._l = Mathf.Clamp(_currentModule._l,0,localLMax);
		_currentModule._w = Mathf.Clamp(_currentModule._w,0,localWMax);
		UpdateDisplay();
		UpdatePergola();
	}
	
	private void UpdateAndCheckSizes()
	{
		_noUI = true;
		UISizesSetting();
		_noUI = false;
		
		UpdatePergola();
	}
	
	#endregion
	
	#region Interface OS3D_Func. Fcns
	
	public string GetFunctionName(){return "PergolaBuilder";}
	
	public string GetFunctionParameterName(){return "Configuration";}
	
	public int GetFunctionId(){return id;}
	
	public void DoAction()
	{
		GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);
			
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;
			
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,true);
		Camera.main.GetComponent<ObjInteraction>().setActived(false);	
		
		
		enabled = true;
	} 
	
	//  sauvegarde/chargement
	
	public void save(BinaryWriter buf)
	{
		buf.Write(_modules.Count);
		
		foreach(PElement m in _modules)
		{
			buf.Write(m._w);
			buf.Write(m._subW1);
			buf.Write(m._subW2);
			buf.Write(m._l);
			buf.Write(m._h);
			
			buf.Write(m._f1);
			buf.Write(m._f2);
			buf.Write(m._f3);
			buf.Write(m._f4);
			
			buf.Write(m._L1);
			buf.Write(m._L2);
			buf.Write(m._W1);
			buf.Write(m._W2);
			
			buf.Write(m._dpL1);
			buf.Write(m._dpL2);
			buf.Write(m._dpW1);
			buf.Write(m._dpW2);
		}
	}
	
	public void load(BinaryReader buf)
	{
		enabled = true;
		int nbPElement = buf.ReadInt32();
		Debug.Log(nbPElement+ "Elements chargés");
		
		_modules.Clear();
		ClearPergola();
		
		for(int i=0;i<nbPElement;i++)
		{
			PElement m = new PElement();
			
			m._w = buf.ReadInt32();
			m._subW1 = buf.ReadInt32();
			m._subW2 = buf.ReadInt32();
			m._l = buf.ReadInt32();
			m._h = buf.ReadInt32();
			
			m._f1 = buf.ReadBoolean();
			m._f2 = buf.ReadBoolean();
			m._f3 = buf.ReadBoolean();
			m._f4 = buf.ReadBoolean();
			
			m._L1 = buf.ReadString();
			m._L2 = buf.ReadString();
			m._W1 = buf.ReadString();
			m._W2 = buf.ReadString();
			
			m._dpL1 = buf.ReadBoolean();
			m._dpL2 = buf.ReadBoolean();
			m._dpW1 = buf.ReadBoolean();
			m._dpW2 = buf.ReadBoolean();
			
			m._pergolaType = b_rules.type;
			
			_modules.Add(m);
			_index = _modules.Count-1;
			_currentModule = _modules[_index];
		}		
		
		UpdatePergola();
		UpdateDisplay();
		
		enabled = false;
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
		
		al.Add(_modules.Count);
		
		foreach(PElement m in _modules)
		{
			al.Add(m._w);
			al.Add(m._subW1);
			al.Add(m._subW2);
			al.Add(m._l);
			al.Add(m._h);
			
			al.Add(m._f1);
			al.Add(m._f2);
			al.Add(m._f3);
			al.Add(m._f4);
			
			al.Add(m._L1);
			al.Add(m._L2);
			al.Add(m._W1);
			al.Add(m._W2);
		}
		
		return al;
	}
	
	public void setConfig(ArrayList config)
	{
		
		int nbPElement = (int)config[0];
		
		int index = 1;
		
		for(int i=0;i<nbPElement;i++)
		{
			PElement m = new PElement();
			
			m._w = (int)config[index];
			index++;
			m._subW1 = (int)config[index];
			index++;
			m._subW2 = (int)config[index];
			index++;
			m._l = (int)config[index];
			index++;
			m._h = (int)config[index];
			index++;
			
			m._f1 = (bool)config[index];
			index++;
			m._f2 = (bool)config[index];
			index++;
			m._f3 = (bool)config[index];
			index++;
			m._f4 = (bool)config[index];
			index++;
			
			m._L1 = (string) config[index];
			index++;
			m._L2 = (string) config[index];
			index++;
			m._W1 = (string) config[index];
			index++;
			m._W2 = (string) config[index];
			index++;
			
			
		}
		UpdatePergola();
	}
	
	#endregion
	
}

	[Serializable ]
	public class BuildDatas
	{
		public int _footSize;// = 0120;

		
		public int _frameH;// = 0210;
		public int _frameW;// = 0120;
		public int _frameL;// = 0120;
		
		public int _subFrameW;// = 0094;
		public int _subFrameL;// = 0094;	
	}
	
	[Serializable ]
	public class PergolaRule
	{
		
		public int singleMaxSize;
		public int extremMaxSize;
		public int middleMaxSize;
		
		//QUE POUR LES COUPLAGES L
		public int limit;
		public int subEMS;
		public int subMMS;
		//------------------------
		
		public int maxWL; // Valeur max commune a tout les modules
		public int maxH;
	
		public int minL;
		public int minW;
		
		public Function_Pergola.PergolaType type;
		
//		public void save(BinaryWriter buf)
//		{
//			buf.Write(singleMaxSize);
//			buf.Write(extremMaxSize);
//			buf.Write(middleMaxSize);
//			
//			buf.Write(limit);
//			buf.Write(subEMS);
//			buf.Write(subMMS);
//			
//			buf.Write(maxWL);
//			buf.Write(maxH);
//			
//			buf.Write((int)type);
//		}
//		
//		public void load(BinaryReader buf)
//		{
//			singleMaxSize = buf.ReadInt32();
//			extremMaxSize = buf.ReadInt32();
//			middleMaxSize = buf.ReadInt32();
//			
//			limit = buf.ReadInt32();
//			subEMS = buf.ReadInt32();
//			subMMS = buf.ReadInt32();
//			
//			maxWL = buf.ReadInt32();
//			maxH = buf.ReadInt32();
//			
//			type = (Function_Pergola.PergolaType) buf.ReadInt32();
//			
//		}
	}
