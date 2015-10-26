using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using Pointcube.Global;

public class Function_PergolaConfigurator : MonoBehaviour,Function_OS3D
{
	#region Attributs
	
	public enum PergolaType
	{
		single,
		CL,
		CW
	}
	
//	enum ModuleStyle 
//	{
//		sans = 0,
//		toileScreen = 1,
//		pvc = 2,
//		pvcFenetre = 3,
//		cristalPlus = 4
//	}
	
	public PergolaScreenOLD[] screensType;
	
	struct Module 
	{
		public int _w;//PRIS EN COMPTE QUE SI COUPLAGE L
		public int _subW1;//PRIS EN COMPTE QUE SI COUPLAGE L
		public int _subW2;//PRIS EN COMPTE QUE SI COUPLAGE L
		public int _l;//PRIS EN COMPTE QUE SI COUPLAGE W
		public int _h;
		public PergolaType _pergolaType;
		public PergolaScreenOLD _L1;//vv Screens
		public PergolaScreenOLD _L2;
		public PergolaScreenOLD _W1;
		public PergolaScreenOLD _W2;
		public bool _f1;//vv Pieds
		public bool _f2;
		public bool _f3;
		public bool _f4;
	}
	
	//UI
	private GUISkin skin;
	private Rect _uiArea;
	
	private string _L = "";
	private string _W = "";
	private string _W1 = "";
	private string _W2 = "";
	private string _H = "";
	
	private bool _showSizes = true;//Par defaut le
	private bool _showFeets = false;
	private bool _showBlades = false;
	private bool _showScreens = false;
	
	//Pergola's sizes
	private int _footSize = 0120;
	private int _mainFrameH = 0210;
	private int _subFrameW = 0094;
	private int _bladeW = 0160;
	private int _bladeH = 0035;
	
	private int _lTot;
	private int _wTot;
	private int _hTot;
	
	//Pergola's building
	public PergolaRuleOLD rule;
//	ArrayList _modules = new ArrayList();
	List<Module> _modules = new List<Module>();
	
	//Pergola's Materials
	Material mat_feet;
	Material mat_frame;
	Material mat_blades;
	Material mat_screens;
	
	//Pergola's SubObjects
	private GameObject feet;
	private GameObject frame;
	private GameObject blades;
	private GameObject screens;
	
	private GameObject f1;
	private GameObject f2;
	private GameObject f3;
	private GameObject f4;
	
	public GameObject _BasePrimitive;
	public GameObject _FoldingBlade;
	
	//...
	private Quaternion savedRotation;
	
	private Vector3 savedScale;
	
	public int id;
	
	private int _index = 0;
	
//	private float _angle = 45;
	private float _angle = 0;
	private float _opening=1;
	
	private Module _currentModule;
	
	#endregion
	
	#region UNITY's fcns
	// Use this for initialization
	void Start ()
	{
		//UI
		_uiArea = new Rect(Screen.width-280,0,280,Screen.height);
		if(skin == null)
			skin = (GUISkin)Resources.Load("skins/PergolaSkin");
		
		//Set Materials
		mat_feet = new Material(Shader.Find("Diffuse"));
		mat_feet.color = Color.white;
		
		mat_frame = new Material(Shader.Find("Diffuse"));
		mat_frame.color = Color.white;
		
//		mat_blades = new Material(Shader.Find("Diffuse"));
		mat_blades = new Material(Shader.Find("Custom/2Sided"));
		mat_blades.color = Color.white;
		
		mat_screens = new Material(Shader.Find("Transparent/Diffuse"));
		mat_screens.color = new Color(1,1,1,0.5f);
		
		SaveTransform();
		
		//Set main SubObjects
		if(!transform.FindChild("feet"))
		{
			feet = new GameObject("feet");
			feet.transform.parent = this.transform;
			feet.transform.localPosition = Vector3.zero;
			
			feet.AddComponent<MeshRenderer>();
			feet.GetComponent<Renderer>().material = mat_feet;
		}
		else
		{
			feet =	transform.FindChild("feet").gameObject;
			feet.GetComponent<Renderer>().material = mat_feet;
		}
		
		if(!transform.FindChild("frame"))
		{
			frame = new GameObject("frame");
			frame.transform.parent = this.transform;
			frame.transform.localPosition = Vector3.zero;
			
			frame.AddComponent<MeshRenderer>();
			frame.GetComponent<Renderer>().material = mat_frame;
		}
		else
		{
			frame =	transform.FindChild("frame").gameObject;
			frame.GetComponent<Renderer>().material = mat_frame;
		}
		
		if(!transform.FindChild("blades"))
		{
			blades = new GameObject("blades");
			blades.transform.parent = this.transform;
			blades.transform.localPosition = Vector3.zero;
			
			blades.AddComponent<MeshRenderer>();
			blades.GetComponent<Renderer>().material = mat_blades;
		}
		else
		{
			blades =	transform.FindChild("blades").gameObject;
			blades.GetComponent<Renderer>().material = mat_blades;
		}
		
		if(!transform.FindChild("screens"))
		{
			screens = new GameObject("screens");
			screens.transform.parent = this.transform;
			screens.transform.localPosition = Vector3.zero;
			
			screens.AddComponent<MeshRenderer>();
			screens.GetComponent<Renderer>().material = mat_screens;
		}
		else
		{
			screens =	transform.FindChild("screens").gameObject;
			screens.GetComponent<Renderer>().material = mat_screens;
		}
		
		ReapplyTransform();
		
		_currentModule._w = 4000;
		_W = _currentModule._w.ToString();
		_currentModule._l = 7000;
		_L = _currentModule._l.ToString();
		_currentModule._subW1 = 0;
		_currentModule._subW2 = 0;
		_currentModule._h = 2500;
		_H = _currentModule._h.ToString();
		_currentModule._pergolaType = rule.type;
		
		_currentModule._L1 = new PergolaScreenOLD();
		_currentModule._L2 = new PergolaScreenOLD();
		_currentModule._W1 = new PergolaScreenOLD();
		_currentModule._W2 = new PergolaScreenOLD();
		
		_currentModule._f1 = true;
		_currentModule._f2 = true;
		_currentModule._f3 = true;
		_currentModule._f4 = true;
		
		_modules.Add(_currentModule);
		
		UpdatePergola();
		
//		enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//--vv-- Update Pergola --vv--
		if(!CompareModules(_currentModule,/*(Module)*/_modules[_index]))
			UpdatePergola();	
		
		//--vv-- INTERACTION --vv--
		if(PC.In.Click1Up() && !PC.In.CursorOnUI(_uiArea))
			Validate();
	}
	
	void OnGUI()
	{		
		GUIMulti();
		
		if(_showFeets && f1 != null)//Feet Marks
		{
			Vector3 LblPos = Camera.main.WorldToScreenPoint(f1.transform.position);
			GUI.Box(new Rect(LblPos.x-20,Screen.height - LblPos.y-20,40,40),"1","feetmark");
			LblPos = Camera.main.WorldToScreenPoint(f2.transform.position);
			GUI.Box(new Rect(LblPos.x-20,Screen.height - LblPos.y-20,40,40),"2","feetmark");
			LblPos = Camera.main.WorldToScreenPoint(f3.transform.position);
			GUI.Box(new Rect(LblPos.x-20,Screen.height - LblPos.y-20,40,40),"3","feetmark");
			LblPos = Camera.main.WorldToScreenPoint(f4.transform.position);
			GUI.Box(new Rect(LblPos.x-20,Screen.height - LblPos.y-20,40,40),"4","feetmark");
		}
	}
	
	#endregion
	
	#region UI
	
//	void GUISingle()
//	{
//		GUI.skin = skin;
//		GUILayout.BeginArea(_uiArea);
//		
//		GUILayout.FlexibleSpace();
//		
//		GUILayout.Box("","UP",GUILayout.Width(280),GUILayout.Height(150));//fade en haut
//		GUILayout.BeginVertical("MID");
//		
//		//Changemant size
//		GUILayout.Label("Dimensions","bgTitle",GUILayout.Height(50),GUILayout.Width(280));
//		UISizeSetting("Longueur\n(mm)\n"+rule.maxWL+"max",ref _L,ref _currentModule._l,0,rule.maxWL);
//		UISizeSetting("Largeur\n(mm)\n"+rule.singleMaxSize+"max",ref _W,ref _currentModule._w,0,rule.singleMaxSize);
//		if(_currentModule._w > rule.limit)
//		{
//			if(_W1 == "")
//			{
//				_currentModule._subW1 = _currentModule._w/2;
//				_currentModule._subW2 = _currentModule._w/2;
//				_W1 = _currentModule._subW1.ToString();
//				_W2 = _currentModule._subW2.ToString();	
//			}
//			
//			int sub1 = _currentModule._subW1;
//			bool s1 = false;
//			UISizeSetting("W1\n(mm)\n"+rule.subEMS+"max",ref _W1,ref sub1,0,rule.subEMS);
//			if(sub1+_currentModule._subW2 != _currentModule._w)
//			{
//				int tmp = _currentModule._w - sub1;
//				if(tmp <= rule.subEMS)
//				{
//					s1 = true;
//					_currentModule._subW1 = sub1;
//					_currentModule._subW2 = tmp;
//					_W2 = _currentModule._subW2.ToString();
//				}
//			}
//			
//			int sub2 = _currentModule._subW2;
//			UISizeSetting("W2\n(mm)\n"+rule.subEMS+"max",ref _W2,ref sub2,0,rule.subEMS);
//			if(sub2+_currentModule._subW1 != _currentModule._w && !s1)
//			{
//				int tmp = _currentModule._w - sub2;
//				if(tmp <= rule.subEMS)
//				{
//					_currentModule._subW2 = sub2;
//					_currentModule._subW1 = tmp;
//					_W1 = _currentModule._subW1.ToString();
//				}
//			}
//			
//		}
//		UISizeSetting("Hauteur\n(mm)\n"+rule.maxH+"max",ref _H,ref _currentModule._h,0,rule.maxH);
//		
//		//Reglage angle lames
//		GUILayout.Label("Orientation Lames","bgTitle",GUILayout.Height(50),GUILayout.Width(280));
//		UIBladesControl();
//		
//		GUILayout.EndVertical();
//		GUILayout.Box("","DWN",GUILayout.Width(280),GUILayout.Height(150));//fade en bas
//		
//		GUILayout.FlexibleSpace();
//		
//		GUILayout.EndArea();
//	} //OBSOLETE
	
	void GUIMulti()
	{
		GUI.skin = skin;
		GUILayout.BeginArea(_uiArea);
		
		GUILayout.FlexibleSpace();
		
		GUILayout.Box("","UP",GUILayout.Width(280),GUILayout.Height(150));//fade en haut
		GUILayout.BeginVertical("MID");
		
		//-----------GESTION DES MODULES--------------------
		if(rule.type != PergolaType.single)
		{
			UIModulesSelector();
			UIModulesAddRemove();
		}
		//-----------GESTION DES TAILLES--------------------
		_showSizes = GUILayout.Toggle(_showSizes,"Dimmensions",GUILayout.Height(50),GUILayout.Width(280));
		if(_showSizes)
		{			
			UISizesSetting();
		}
		//-----------GESTION DES PIEDS----------------------
		_showFeets = GUILayout.Toggle(_showFeets,"Pieds",GUILayout.Height(50),GUILayout.Width(280));
		if(_showFeets)
		{
			UIFeetControl();
			if(f1 == null)
			{
				f1 = feet.transform.FindChild(((_index+1)*10+1).ToString()).gameObject;
				f2 = feet.transform.FindChild(((_index+1)*10+2).ToString()).gameObject;
				f3 = feet.transform.FindChild(((_index)*10+1).ToString()).gameObject;
				f4 = feet.transform.FindChild(((_index)*10+2).ToString()).gameObject;	
			}
		}
		else
		{
			if(f1 != null)
			{
				f1 = null;
				f2 = null;
				f3 = null;
				f4 = null;	
			}
		}
		//-----------GESTION DES SCREENS--------------------
		_showScreens = GUILayout.Toggle(_showScreens,"Screens",GUILayout.Height(50),GUILayout.Width(280));
		if(_showScreens)
		{
			UIScreensControl();
		}
		//-----------GESTION DES LAMES----------------------
		_showBlades = GUILayout.Toggle(_showBlades,"Blades",GUILayout.Height(50),GUILayout.Width(280));
		if(_showBlades)
		{
//			UIBladesControl();
			UIAnimBladesControl();
		}
		//-----------FADE BAS-------------------------------
		GUILayout.EndVertical();
		GUILayout.Box("","DWN",GUILayout.Width(280),GUILayout.Height(150));//fade en bas
		
		GUILayout.FlexibleSpace();
		
		GUILayout.EndArea();
	}
	
	void UISizesSetting()
	{
		if(_modules.Count == 1)
		{
			switch (rule.type)
			{
			case PergolaType.single:
				UI3SizesBloc(rule.maxWL,rule.singleMaxSize,rule.subEMS,rule.subEMS,rule.maxH);
				break;
			case PergolaType.CL:
				UI3SizesBloc(rule.maxWL,rule.singleMaxSize,rule.subEMS,rule.subEMS,rule.maxH);
				break;
			case PergolaType.CW:
				UI3SizesBloc(rule.singleMaxSize,rule.maxWL,rule.subEMS,rule.subEMS,rule.maxH);
				break;
			}
		}
		else if(_modules.Count>1)
		{
			if(_index == 0)//1er
			{
				if(rule.type == PergolaType.CL)
				{
					UI3SizesBloc(rule.maxWL,rule.extremMaxSize,rule.subEMS,rule.subMMS,rule.maxH);
				}
				else //PergolaType.CW
				{
					UI3SizesBloc(rule.extremMaxSize,rule.maxWL,0,0,rule.maxH);
				}
			}
			else if(_index == _modules.Count-1)//dernier
			{
				if(rule.type == PergolaType.CL)
				{
					UI3SizesBloc(rule.maxWL,rule.extremMaxSize,rule.subMMS,rule.subEMS,rule.maxH);
				}
				else //PergolaType.CW
				{
					UI3SizesBloc(rule.extremMaxSize,rule.maxWL,0,0,rule.maxH);
				}
			}
			else//milieux
			{
				if(rule.type == PergolaType.CL)
				{
					UI3SizesBloc(rule.maxWL,rule.middleMaxSize,rule.subMMS,rule.subMMS,rule.maxH);
				}
				else //PergolaType.CW
				{
					UI3SizesBloc(rule.middleMaxSize,rule.maxWL,0,0,rule.maxH);
				}
			}
		}
	}
	void UI3SizesBloc(int lMax,int wMax,int w1Max,int w2Max,int hMax)
	{
		UISizeSetting("Longueur\n(mm)\n"+lMax+"max",ref _L,ref _currentModule._l,0,lMax);
		UISizeSetting("Largeur\n(mm)\n"+wMax+"max",ref _W,ref _currentModule._w,0,wMax);
		if(_currentModule._w > rule.limit && rule.type != PergolaType.CW)
		{
			if(/*_W1 == ""*/ _currentModule._subW1 == 0)
			{
				_currentModule._subW1 = _currentModule._w/2;
				_currentModule._subW2 = _currentModule._w/2;
				_W1 =_currentModule._subW1.ToString();
				_W2 =_currentModule._subW2.ToString();	
			}
			
			int sub1 = _currentModule._subW1;
			bool s1 = false;
			UISizeSetting("W1\n(mm)\n"+w1Max+"max",ref _W1,ref sub1,0,w1Max);
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
			UISizeSetting("W2\n(mm)\n"+w2Max+"max",ref _W2,ref sub2,0,w2Max);
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
		UISizeSetting("Hauteur\n(mm)\n"+hMax+"max",ref _H,ref _currentModule._h,0,hMax);
	}
	void UISizeSetting(string txt,ref string valueStr,ref int val,int min,int max)
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
	
	void UIBladesControl()
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		float tmp = GUILayout.HorizontalSlider(_angle,10,90,GUILayout.Width(240));
		if(_angle != tmp)
		{
			_angle = tmp;
			UpdateBladesOrientation();
		}
		
		GUILayout.Space(10);
		GUILayout.EndHorizontal();	
	}
	
	void UIAnimBladesControl()
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		float tmp = GUILayout.HorizontalSlider(_angle,0,1,GUILayout.Width(240));
		if(_angle != tmp)
		{
			_angle = tmp;
			UpdateBladesAnimation();
		}
		
		GUILayout.Space(10);
		GUILayout.EndHorizontal();		
	}
	
	void UIScreensControl()
	{
		switch (rule.type)
		{
		case PergolaType.single:
			UISelectScreen("L1",ref _currentModule._L1);
			UISelectScreen("L2",ref _currentModule._L2);
			UISelectScreen("W1",ref _currentModule._W1);
			UISelectScreen("W2",ref _currentModule._W2);
			break;
			
		case PergolaType.CL:
			UISelectScreen("W1",ref _currentModule._W1);
			UISelectScreen("W2",ref _currentModule._W2);
			if(_index == 0)//1er
			{
				UISelectScreen("L2",ref _currentModule._L2);
			}
			if(_index == _modules.Count-1)//Dernier
			{
				UISelectScreen("L1",ref _currentModule._L1);
			}
			break;
			
		case PergolaType.CW:
			UISelectScreen("L1",ref _currentModule._L1);
			UISelectScreen("L2",ref _currentModule._L2);
			if(_index == 0)//1er
			{
				UISelectScreen("W1",ref _currentModule._W1);
			}
			if(_index == _modules.Count-1)//Dernier
			{
				UISelectScreen("W2",ref _currentModule._W2);
			}
			break;			
		}
		
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		float tmp = GUILayout.HorizontalSlider(_opening,0,1,GUILayout.Width(240));
		if(_opening != tmp)
		{
			_opening = tmp;
			UpdateScreens();
		}
		
		GUILayout.Space(10);
		GUILayout.EndHorizontal();	
	}
	void UISelectScreen(string label,ref PergolaScreenOLD ms)
	{	
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("","btn<",GUILayout.Height(50),GUILayout.Width(50)))
		{
//			int i = (int)ms;
			int i = -1;
			for(int ii = 0;ii<screensType.Length;ii++)
				if(ms.name == screensType[ii].name)
					i = ii;
				
			if(i > 0)
			{
				i--;
				ms = screensType[i];
				UpdatePergola();
			}
		}
		GUILayout.Label(ms.name,GUILayout.Height(50),GUILayout.Width(75));
		
		if(GUILayout.Button("","btn>",GUILayout.Height(50),GUILayout.Width(50)))
		{
//			int i = (int)ms;
			int i = -1;
			for(int ii = 0;ii<screensType.Length;ii++)
				if(ms.name == screensType[ii].name)
					i = ii;
			
			if(i < screensType.Length-1)
			{
				i++;
				ms = screensType[i];
				UpdatePergola();
			}
		}
		GUILayout.Label(label,"txt",GUILayout.Height(50),GUILayout.Width(50));
		GUILayout.Space(20);
		GUILayout.EndHorizontal();
	}
	
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
		GUILayout.Space(10);
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
				f1 = null;
			}
		}
		GUILayout.Space(10);
		GUILayout.Label("Module "+(_index+1)+" / "+_modules.Count,GUILayout.Height(50));
		GUILayout.Space(10);
		if(GUILayout.Button("Nxt","btn>",GUILayout.Height(50),GUILayout.Width(50)))
		{
			if(_modules.Count > _index+1)
			{
				_index ++;
				_currentModule = /*(Module)*/_modules[_index];
				UpdateDisplay();
				f1 = null;
			}
		}
		
		GUILayout.Space(20);
		GUILayout.EndHorizontal();	
	}
	
	void UIModulesAddRemove()
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		if(GUILayout.Button("Add"/*,"prev"*/,GUILayout.Height(50),GUILayout.Width(50)))
		{
			Module m = new Module();//_currentModule;
			
			m._L1 = new PergolaScreenOLD();
			m._L2 = new PergolaScreenOLD();
			m._W1 = new PergolaScreenOLD();
			m._W2 = new PergolaScreenOLD();
			
			m._l = _currentModule._l;
			m._w = _currentModule._w;
			m._subW1 = _currentModule._subW1;
			m._subW2 = _currentModule._subW2;
			m._h = _currentModule._h;
			
			
			m._pergolaType = rule.type;
			
			m._f1 = true;
			m._f2 = true;
			m._f3 = true;
			m._f4 = true;
			
			if(_modules.Count == 1)
				CheckScreens();
			
			_modules.Add(m);
			_index = _modules.Count-1;
			_currentModule = _modules[_index];
			UpdatePergola();
			UpdateDisplay();
			
		}
		
		if(GUILayout.Button("Rem"/*,"next"*/,GUILayout.Height(50),GUILayout.Width(50)))
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
		
		GUILayout.Space(20);
		GUILayout.EndHorizontal();	
	}
	
	#endregion
	
	#region Aux fcns
	
	void CheckScreens()
	{
		switch (rule.type)
		{
		case PergolaType.CL:
			if(_modules[0]._L1 != screensType[0])
			{
				Module m = _modules[0];
				m._L1 = screensType[0];
				_modules[0]=m;
			}

//			if(_index == _modules.Count-1)//Dernier
//			{
//				if(_currentModule._L2 != screensType[0])
//					_currentModule._L2 = screensType[0];
//			}
//			if(_index != 0 && _index != _modules.Count-1)
//			{
//				if(_currentModule._L1 != screensType[0])
//					_currentModule._L1 = screensType[0];
//				if(_currentModule._L2 != screensType[0])
//					_currentModule._L2 = screensType[0];
//			}
			break;
			
		case PergolaType.CW:
			if(_modules[0]._W2 != screensType[0])
			{
				Module m = _modules[0];
				m._W2 = screensType[0];
				_modules[0]=m;
			}

//			if(_index == _modules.Count-1)//Dernier
//			{
//				if(_currentModule._W1 != screensType[0])
//					_currentModule._W1 = screensType[0];
//			}
//			if(_index != 0 && _index != _modules.Count-1)
//			{
//				if(_currentModule._W1 != screensType[0])
//					_currentModule._W1 = screensType[0];
//				if(_currentModule._W2 != screensType[0])
//					_currentModule._W2 = screensType[0];
//			}
			break;			
		}	
	}
	
	void Validate()
	{
		Camera.main.GetComponent<ObjInteraction>().configuringObj(null);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().unConfigure();
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().setVisibility (false);
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,false);		
		enabled = false;	
	}
	
	void ClearPergola()
	{
		foreach(Transform t in feet.transform)
		{
			Destroy(t.gameObject);
		}
		foreach(Transform t in frame.transform)
		{
			Destroy(t.gameObject);
		}
		foreach(Transform t in blades.transform)
		{
			Destroy(t.gameObject);
		}
		Destroy(blades);
		blades = new GameObject("blades");
		blades.transform.parent = this.transform;
		blades.transform.localPosition = Vector3.zero;
		
		blades.AddComponent<MeshRenderer>();
		blades.GetComponent<Renderer>().material = mat_blades;
		
		foreach(Transform t in screens.transform)
		{
			Destroy(t.gameObject);
		}
	}
	
	void SaveTransform()
	{
		savedRotation = transform.rotation;
		savedScale = transform.localScale;
		
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
	
	void ReapplyTransform()
	{
		transform.rotation = savedRotation;
		transform.localScale = savedScale;
	}
	
	void UpdatePergola()
	{
		
		//--vv-- UPDATE VALEURS GLOBALES --vv--
		if(rule.type == PergolaType.CL)
		{
			if(_currentModule._l != (/*(Module)*/_modules[_index])._l)
			{
				int size = _currentModule._l;
				
				for(int i=0;i<_modules.Count;i++)
				{
					Module m = /*(Module)*/_modules[i];
					m._l = size;
					_modules[i] = m;
				}
			}
		}
		else if(rule.type == PergolaType.CW)
		{
			if(_currentModule._w != (/*(Module)*/_modules[_index])._w)
			{
				int size = _currentModule._w;
				
				for(int i=0;i<_modules.Count;i++)
				{
					Module m = /*(Module)*/_modules[i];
					m._w = size;
					_modules[i] = m;
				}
			}
		}
		
		if(_currentModule._h != (/*(Module)*/_modules[_index])._h)
		{
			int size = _currentModule._h;
			
			for(int i=0;i<_modules.Count;i++)
			{
				Module m = /*(Module)*/_modules[i];
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
			Module m = /*(Module)*/_modules[i];
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
			Module m = /*(Module)*/_modules[i];
			BuildFeet(m,i,off7);
			BuildFrame(m,i,off7);
//			BuildBlades(m,i,off7);
			BuildAnimatedBlades(m,i,off7);
			BuildScreens(m,i,off7);
			
			if(m._pergolaType == PergolaType.CL)
				off7 += m._w;
			else if(m._pergolaType == PergolaType.CW)
				off7 += m._l;
		}
//		UpdateBladesOrientation();
		
		ReapplyTransform();
		
		CreateBounds();
	}
	
	void UpdateDisplay()
	{
		_W = _currentModule._w.ToString();
		_W1= _currentModule._subW1.ToString();
		_W2= _currentModule._subW2.ToString();
		_L = _currentModule._l.ToString();
		_H = _currentModule._h.ToString();				
	}
	
	void UpdateBladesOrientation()
	{
		Quaternion q = new Quaternion(0,0,0,0);
		q.eulerAngles = new Vector3(_angle,0,0);
		foreach(Transform t in blades.transform)
		{
			t.localRotation = q;	
		}
	}
	
	void UpdateBladesAnimation()
	{
		foreach(Transform t in blades.transform)
		{
			if(t.GetComponent<AnimateBlade>())
			{
				t.GetComponent<AnimateBlade>().AnimTo(_angle);
			}
		}
	}
	
	void UpdateFeet()
	{
//		f1 = feet.transform.FindChild(((_index+1)*10+1).ToString()).gameObject;
//		f2 = feet.transform.FindChild(((_index+1)*10+2).ToString()).gameObject;
//		fp1 = feet.transform.FindChild(((_index)*10+1).ToString()).gameObject;
//		fp2 = feet.transform.FindChild(((_index)*10+2).ToString()).gameObject;
		
		f1.GetComponent<Renderer>().enabled = _currentModule._f1;
		f2.GetComponent<Renderer>().enabled = _currentModule._f2;
		f3.GetComponent<Renderer>().enabled = _currentModule._f3;
		f4.GetComponent<Renderer>().enabled = _currentModule._f4;
		
		_modules[_index] = _currentModule;
		
		if(_index != 0)
		{
			Module pm = _modules[_index-1];
			pm._f1 = _currentModule._f3;
			pm._f2 = _currentModule._f4;
			_modules[_index-1] = pm;
		}
		if(_index != _modules.Count-1)
		{
			Module nm = _modules[_index+1];
			nm._f3 = _currentModule._f1;
			nm._f4 = _currentModule._f2;
			_modules[_index+1] = nm;
		}
		
	}
	
	void UpdateScreens()
	{
		foreach(PergolaScreenOLD ps in screensType)
		{
			if(ps.mat != null)
			{
				if(ps.mat.HasProperty("_AlphaTex"))
				{
					Vector2 off7 = ps.mat.GetTextureScale("_AlphaTex");
					off7.y = _opening;
					ps.mat.SetTextureScale("_AlphaTex",off7);
				}
			}
		}
		float h = (_modules[0]._h - _mainFrameH)/10000f;
		
		float sizeH = _opening * h;
		float posH = h-(sizeH/2);
		posH = posH *10;
		
		foreach(Transform t in screens.transform)
		{
			Vector3 pos = t.localPosition;
			Vector3 scl = t.localScale;
			
			pos.y = posH;
			scl.z = sizeH;
			
			t.localPosition= pos;
			t.localScale = scl;
		}
	}
	
	bool CompareModules(Module m1, Module m2)
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
	
	#endregion
	
	#region Building Fcns
	
	void BuildFeet(Module m,int index,int off7)
	{
		if(m._pergolaType == PergolaType.CL || m._pergolaType == PergolaType.single)
		{
			if(index == 0)//1er
			{
				AddPartAt(new Vector3(off7+_footSize/2,m._h/2-_mainFrameH/2, m._l/2-_footSize/2),
							new Vector3(_footSize,m._h-_mainFrameH,_footSize),feet,index+1);
				AddPartAt(new Vector3(off7+_footSize/2,m._h/2-_mainFrameH/2, -m._l/2+_footSize/2),
							new Vector3(_footSize,m._h-_mainFrameH,_footSize),feet,index+2);
			}
			if(!(index == 0))//milieux
			{
				AddPartAt(new Vector3(off7,m._h/2-_mainFrameH/2, m._l/2-_footSize/2),
						new Vector3(_subFrameW,m._h-_mainFrameH,_footSize),feet,index*10+1);
				AddPartAt(new Vector3(off7,m._h/2-_mainFrameH/2, -m._l/2+_footSize/2),
						new Vector3(_subFrameW,m._h-_mainFrameH,_footSize),feet,index*10+2);
			}
			if(index == _modules.Count-1)//dernier
			{
				AddPartAt(new Vector3(off7+m._w-_footSize/2,m._h/2-_mainFrameH/2, m._l/2-_footSize/2),
					new Vector3(_footSize,m._h-_mainFrameH,_footSize),feet,(index+1)*10+1);
				AddPartAt(new Vector3(off7+m._w-_footSize/2,m._h/2-_mainFrameH/2, -m._l/2+_footSize/2),
					new Vector3(_footSize,m._h-_mainFrameH,_footSize),feet,(index+1)*10+2);
			}
		}
		else// if(m._pergolaType == PergolaType.CW)
		{
			if(index == 0)//1er
			{
				AddPartAt(new Vector3(m._w/2-_footSize/2,m._h/2-_mainFrameH/2,off7+_footSize/2),
							new Vector3(_footSize,m._h-_mainFrameH,_footSize),feet,index+1);
				AddPartAt(new Vector3(-m._w/2+_footSize/2,m._h/2-_mainFrameH/2,off7+_footSize/2),
							new Vector3(_footSize,m._h-_mainFrameH,_footSize),feet,index+2);
			}
			if(!(index == 0))//milieux
			{
				AddPartAt(new Vector3(m._w/2-_footSize/2,m._h/2-_mainFrameH/2, off7),
						new Vector3(_footSize,m._h-_mainFrameH,_subFrameW),feet,index*10+1);
				AddPartAt(new Vector3(-m._w/2+_footSize/2,m._h/2-_mainFrameH/2, off7),
						new Vector3(_footSize,m._h-_mainFrameH,_subFrameW),feet,index*10+2);
			}
			if(index == _modules.Count-1)//dernier
			{
				AddPartAt(new Vector3(m._w/2-_footSize/2,m._h/2-_mainFrameH/2, off7+m._l-_footSize/2),
					new Vector3(_footSize,m._h-_mainFrameH,_footSize),feet,(index+1)*10+1);
				AddPartAt(new Vector3(-m._w/2+_footSize/2,m._h/2-_mainFrameH/2, off7+m._l-_footSize/2),
					new Vector3(_footSize,m._h-_mainFrameH,_footSize),feet,(index+1)*10+2);
			}
		}
	}
	void BuildFrame(Module m,int index,int off7)
	{
		int h = m._h - _mainFrameH/2;
		if(m._pergolaType == PergolaType.CL || m._pergolaType == PergolaType.single)
		{
			int sizeL = m._l - 2*_footSize;
			
			if(index == 0)//1er
			{
				AddPartAt(new Vector3(off7+_footSize/2,h,0),new Vector3(_footSize,_mainFrameH,sizeL),frame,index);
			}
			if(!(index == _modules.Count-1))//milieux
			{
				AddPartAt(new Vector3(off7+m._w/2,h,m._l/2-_footSize/2),new Vector3(m._w,_mainFrameH,_footSize),frame,index);
				AddPartAt(new Vector3(off7+m._w/2,h,-m._l/2+_footSize/2),new Vector3(m._w,_mainFrameH,_footSize),frame,index);
				
				AddPartAt(new Vector3(off7+m._w,h,0),new Vector3(_subFrameW,_mainFrameH,sizeL),frame,index);
			}
			if(index == _modules.Count-1)//dernier
			{
				AddPartAt(new Vector3(off7+m._w/2,h,m._l/2-_footSize/2),new Vector3(m._w,_mainFrameH,_footSize),frame,index);
				AddPartAt(new Vector3(off7+m._w/2,h,-m._l/2+_footSize/2),new Vector3(m._w,_mainFrameH,_footSize),frame,index);
				
				AddPartAt(new Vector3(off7+m._w-_footSize/2,h,0),new Vector3(_footSize,_mainFrameH,sizeL),frame,index);
			}
		}
		else// if(m._pergolaType == PergolaType.CW)
		{
			int sizeW = m._w - 2*_footSize;
			
			if(index == 0)//1er
			{
				AddPartAt(new Vector3(0,h,off7+_footSize/2),new Vector3(sizeW,_mainFrameH,_footSize),frame,index);
			}
			if(!(index == _modules.Count-1))//milieux
			{
				AddPartAt(new Vector3(m._w/2-_footSize/2,h,off7+m._l/2),new Vector3(_footSize,_mainFrameH,m._l),frame,index);
				AddPartAt(new Vector3(-m._w/2+_footSize/2,h,off7+m._l/2),new Vector3(_footSize,_mainFrameH,m._l),frame,index);
				
				AddPartAt(new Vector3(0,h,off7+m._l),new Vector3(sizeW,_mainFrameH,_subFrameW),frame,index);
			}
			if(index == _modules.Count-1)//dernier
			{
				AddPartAt(new Vector3(m._w/2-_footSize/2,h,off7+m._l/2),new Vector3(_footSize,_mainFrameH,m._l),frame,index);
				AddPartAt(new Vector3(-m._w/2+_footSize/2,h,off7+m._l/2),new Vector3(_footSize,_mainFrameH,m._l),frame,index);
				
				AddPartAt(new Vector3(0,h,off7+m._l-_footSize/2),new Vector3(sizeW,_mainFrameH,_footSize),frame,index);
			}
		}
	}
	void BuildBlades(Module m,int index,int off7)
	{
		if(m._pergolaType == PergolaType.CL || m._pergolaType == PergolaType.single)
		{
			int len = m._l - 2*_footSize;
			int h = m._h - _mainFrameH/2;
			int centre = off7+m._w/2;
			int startZ = -len/2+_bladeW/2;
			int nb = Mathf.FloorToInt(len/_bladeW);
			for(int i=0;i<nb;i++)
			{
				AddPartAt(new Vector3(centre,h,startZ+i*_bladeW),new Vector3(m._w-_subFrameW,_bladeH,_bladeW),blades,index);
			}
			if(m._w > rule.limit)
			{
				AddPartAt(new Vector3(off7+m._subW1,h,0),new Vector3(_subFrameW,_mainFrameH,len),frame,index);
			}
			
		}
		else// if(m._pergolaType == PergolaType.CW)
		{
			int startZ = 0;
			int len = 0;
			if(_modules.Count == 1)//1 module
			{
				startZ = off7+_footSize;
				len = m._l - 2*_footSize;
			}
			else//plusieurs modules
			{
				if(index == 0)//1er
				{
					startZ = off7+_footSize;
					len = m._l - _footSize - _subFrameW;
				}
				else if(index == _modules.Count-1)//deriner
				{
					startZ = off7+_subFrameW/2;
					len = m._l - _footSize - _subFrameW;
				}
				else//intermediares
				{
					startZ = off7+_subFrameW/2;
					len = m._l - 2*_subFrameW;
				}
			}
			
			startZ+=_bladeW/2;
			
			int nb = Mathf.CeilToInt(len/_bladeW);
			int h = m._h - _mainFrameH/2;
			
			for(int i=0;i<nb;i++)
			{
				AddPartAt(new Vector3(0,h,startZ+i*_bladeW),new Vector3(m._w-2*_footSize,_bladeH,_bladeW),blades,index);
			}
		}
	}
	void BuildAnimatedBlades(Module m,int index,int off7)
	{
		if(m._pergolaType == PergolaType.single)
		{
			int len = m._l - 2*_footSize;
			int h = m._h - _mainFrameH/2;
			int centre = off7+m._w/2;
			int startZ = -len/2/*+250/2*/;
			int nb = Mathf.FloorToInt(len/250);
			for(int i=0;i<nb;i++)
			{
				AddPartAt(new Vector3(centre,h,startZ+i*250),new Vector3(1000,1000,m._w-_subFrameW),blades,index*100+i,_FoldingBlade,90);
			}			
		}
		for(int i=blades.transform.GetChildCount()-1;i>0;i--)
		{
			if(blades.transform.GetChild(i).GetComponent<AnimateBlade>())
			{
				Debug.Log("What is that>"+blades.transform.GetChild(i).GetComponent<AnimateBlade>().alignTo);
				blades.transform.GetChild(i).GetComponent<AnimateBlade>().alignTo = blades.transform.GetChild(i-1).FindChild("1").transform;
			}
		}
	}
	void BuildScreens(Module m,int index,int off7)
	{
		float h = m._h - _mainFrameH;
		
		float sizeH = _opening * h;
		float posH = h-(sizeH/2);
		
		if(m._pergolaType == PergolaType.single)
		{
			int sizeL = m._l- _subFrameW;// - 2*_footSize;
			int sizeW = m._w - _subFrameW;// - 2*_footSize;
			
			if(m._W1.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,-m._l/2+_footSize/2),new Vector3(sizeW,sizeH,/*_footSize/2*/0),screens,index,m._W1.mat,true);
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,-m._l/2+_footSize/2),new Vector3(sizeW,1,sizeH),screens,index,m._W1.mat,180);
			if(m._W2.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,m._l/2-_footSize/2),new Vector3(sizeW,sizeH,/*_footSize/2*/0),screens,index,m._W2.mat,false);
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,m._l/2-_footSize/2),new Vector3(sizeW,1,sizeH),screens,index,m._W2.mat,0);
			if(m._L2.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(off7+_footSize/2,posH,0),new Vector3(/*_footSize/2*/0,sizeH,sizeL),screens,index,m._L2.mat);
				AddPartAtWthMat(new Vector3(off7+_footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,m._L2.mat,-90);
			if(m._L1.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(off7+m._w-_footSize/2,posH,0),new Vector3(/*_footSize/2*/0,sizeH,sizeL),screens,index,m._L1.mat);
				AddPartAtWthMat(new Vector3(off7+m._w-_footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,m._L1.mat,90);
		}		
		else if(m._pergolaType == PergolaType.CL)
		{
			int sizeL = m._l- _subFrameW;// - 2*_footSize;
			int sizeW = m._w - _subFrameW;// - 2*_footSize;
			
			if(m._W1.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,-m._l/2+_footSize/2),new Vector3(sizeW,sizeH,/*_footSize/2*/0),screens,index,m._W1.mat,true);
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,-m._l/2+_footSize/2),new Vector3(sizeW,1,sizeH),screens,index,m._W1.mat,180);
			if(m._W2.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,m._l/2-_footSize/2),new Vector3(sizeW,sizeH,/*_footSize/2*/0),screens,index,m._W2.mat,false);
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,m._l/2-_footSize/2),new Vector3(sizeW,1,sizeH),screens,index,m._W2.mat,0);
			
			if(m._L2.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(off7+_footSize/2,posH,0),new Vector3(/*_footSize/2*/0,sizeH,sizeL),screens,index,m._L2.mat);
				AddPartAtWthMat(new Vector3(off7+_footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,m._L2.mat,-90);
			if(m._L1.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(off7+m._w-_footSize/2,posH,0),new Vector3(/*_footSize/2*/0,sizeH,sizeL),screens,index,m._L1.mat);
				AddPartAtWthMat(new Vector3(off7+m._w-_footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,m._L1.mat,90);
		}
		else if(m._pergolaType == PergolaType.CW)
		{
			int sizeL = m._l- _subFrameW;
			int sizeW = m._w - _subFrameW;
			
			if(m._W1.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(0,posH,off7+_footSize/2),new Vector3(sizeW,sizeH,/*_footSize/2*/0),screens,index,m._W1.mat,true);
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,-m._l/2+_footSize/2),new Vector3(sizeW,1,sizeH),screens,index,m._W1.mat,180);
			if(m._W2.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(0,posH,off7+m._l-_footSize/2),new Vector3(sizeW,sizeH,/*_footSize/2*/0),screens,index,m._W2.mat,true);
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,m._l/2-_footSize/2),new Vector3(sizeW,1,sizeH),screens,index,m._W2.mat,0);
			if(m._L2.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(-m._w/2+_footSize/2,posH,off7+m._l/2),new Vector3(/*_footSize/2*/0,sizeH,sizeL),screens,index,m._L2.mat);
				AddPartAtWthMat(new Vector3(off7+_footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,m._L2.mat,-90);
			if(m._L1.name != screensType[0].name)
//				AddPartAtWthMat(new Vector3(m._w/2-_footSize/2,posH,off7+m._l/2),new Vector3(/*_footSize/2*/0,sizeH,sizeL),screens,index,m._L1.mat);
				AddPartAtWthMat(new Vector3(off7+m._w-_footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,m._L1.mat,90);
		}

	}
	
	void AddPartAt(Vector3 position, Vector3 scale,GameObject p,int i)
	{
		AddPartAt(position, scale,p,i,_BasePrimitive,0);
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
			part.GetComponent<Renderer>().material = p.GetComponent<Renderer>().material;
		else
		{
			foreach(Transform t in part.transform)
			{
				if(t.GetComponent<Renderer>())
					t.GetComponent<Renderer>().material = p.GetComponent<Renderer>().material;
			}
		}
		part.transform.localPosition = floatPos;
		part.transform.localScale = floatScl;
		
		Quaternion q = part.transform.localRotation;
		Vector3 angles = q.eulerAngles;
		angles.y += angleY;
		q.eulerAngles = angles;
		part.transform.localRotation = q;
	}
	
	void AddPartAtWthMat(Vector3 position, Vector3 scale,GameObject p,int i,Material mat,float angleY)
	{
		Vector3 floatScl = scale/10000f;
		Vector3 floatPos = position/1000f;
		
		GameObject part = GameObject.CreatePrimitive(PrimitiveType.Plane);//(GameObject) Instantiate(_BasePrimitive);
		part.name = i.ToString();
		part.transform.parent = p.transform;
		part.layer = gameObject.layer;
		part.GetComponent<Renderer>().material = mat;	part.transform.localPosition = floatPos;
		part.transform.localScale = floatScl;
		
		Quaternion q = part.transform.localRotation;
		Vector3 angles = q.eulerAngles;
		angles.x += 90;
		angles.y += angleY;
		q.eulerAngles = angles;
		part.transform.localRotation = q;
	}
	
	#endregion
	
	#region Interface Fcns
	
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
		rule.save(buf);
		
		buf.Write(_modules.Count);
		buf.Write(_angle);
		buf.Write(_opening);
		
		foreach(Module m in _modules)
		{
			buf.Write(m._w);
			buf.Write(m._subW1);
			buf.Write(m._subW2);
			buf.Write(m._l);
			buf.Write(m._h);
			
//			buf.Write((int) m._L1);
//			buf.Write((int) m._L2);
//			buf.Write((int) m._W1);
//			buf.Write((int) m._W2);
			
			buf.Write(m._f1);
			buf.Write(m._f2);
			buf.Write(m._f3);
			buf.Write(m._f4);
		}
	}
	
	public void load(BinaryReader buf)
	{
		rule = new PergolaRuleOLD();
		rule.load(buf);
		
		int nbModule = buf.ReadInt32();
		_angle = buf.ReadSingle();
		_opening = buf.ReadSingle();
		
		for(int i=0;i<nbModule;i++)
		{
			Module m = new Module();
			
			m._w = buf.ReadInt32();
			m._subW1 = buf.ReadInt32();
			m._subW2 = buf.ReadInt32();
			m._l = buf.ReadInt32();
			m._h = buf.ReadInt32();
			
			m._L1 = new PergolaScreenOLD();//(ModuleStyle)buf.ReadInt32();
			m._L2 = new PergolaScreenOLD();//(ModuleStyle)buf.ReadInt32();
			m._W1 = new PergolaScreenOLD();//(ModuleStyle)buf.ReadInt32();
			m._W2 = new PergolaScreenOLD();//(ModuleStyle)buf.ReadInt32();
			
			m._f1 = buf.ReadBoolean();
			m._f2 = buf.ReadBoolean();
			m._f3 = buf.ReadBoolean();
			m._f4 = buf.ReadBoolean();
			
			m._pergolaType = rule.type;
		}
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
		al.Add(_angle);
		al.Add(_opening);
		
		foreach(Module m in _modules)
		{
			al.Add(m._w);
			al.Add(m._subW1);
			al.Add(m._subW2);
			al.Add(m._l);
			al.Add(m._h);
			
//			buf.Write((int) m._L1);
//			buf.Write((int) m._L2);
//			buf.Write((int) m._W1);
//			buf.Write((int) m._W2);
			
			al.Add(m._f1);
			al.Add(m._f2);
			al.Add(m._f3);
			al.Add(m._f4);
		}
		
		return al;
	}
	
	public void setConfig(ArrayList config)
	{
		
		int nbModule = (int)config[0];
		_angle = (float)config[1];
		_opening = (float)config[2];
		
		int index = 3;
		
		for(int i=0;i<nbModule;i++)
		{
			Module m = new Module();
			
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
			
			m._L1 = new PergolaScreenOLD();//(ModuleStyle)buf.ReadInt32();
			m._L2 = new PergolaScreenOLD();//(ModuleStyle)buf.ReadInt32();
			m._W1 = new PergolaScreenOLD();//(ModuleStyle)buf.ReadInt32();
			m._W2 = new PergolaScreenOLD();//(ModuleStyle)buf.ReadInt32();
			
			m._f1 = (bool)config[index];
			index++;
			m._f2 = (bool)config[index];
			index++;
			m._f3 = (bool)config[index];
			index++;
			m._f4 = (bool)config[index];
			index++;
			
			m._pergolaType = rule.type;
		}
	}
	
	#endregion
}

[Serializable ]
public class PergolaRuleOLD
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
	
	public Function_PergolaConfigurator.PergolaType type;
	
	public void save(BinaryWriter buf)
	{
		buf.Write(singleMaxSize);
		buf.Write(extremMaxSize);
		buf.Write(middleMaxSize);
		
		buf.Write(limit);
		buf.Write(subEMS);
		buf.Write(subMMS);
		
		buf.Write(maxWL);
		buf.Write(maxH);
		
		buf.Write((int)type);
	}
	
	public void load(BinaryReader buf)
	{
		singleMaxSize = buf.ReadInt32();
		extremMaxSize = buf.ReadInt32();
		middleMaxSize = buf.ReadInt32();
		
		limit = buf.ReadInt32();
		subEMS = buf.ReadInt32();
		subMMS = buf.ReadInt32();
		
		maxWL = buf.ReadInt32();
		maxH = buf.ReadInt32();
		
		type = (Function_PergolaConfigurator.PergolaType) buf.ReadInt32();
		
	}
}

[Serializable ]
public class PergolaScreenOLD
{
	public string name;
	public int maxSize;
	public Material mat;
	
	public PergolaScreenOLD()
	{
		name = "none";
		maxSize = -1;
		mat = null;
	}
}
