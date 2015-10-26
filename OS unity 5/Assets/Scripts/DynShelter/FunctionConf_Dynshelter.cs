using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using Pointcube.Utils;

public class FunctionConf_Dynshelter : MonoBehaviour,Function_OS3D
{

#region public vars
	public int os3dFuncId;
	
	public Texture2D addBigger,addSmaller,addSame,addSameFree,addFacade,addExtrem;
	
	public string modelName;
	
	public GameObject limitObject;
	public GameObject arrowRef;
	
	public enum ModuleType
	{
		bloc,
		multiBloc,
		facade,
		extremite
	}
#endregion

#region private vars
	//Function OS3D -------------
	private GUIMenuConfiguration 	menuConf;
	private GUIMenuInteraction 		menuInteract;
	private ObjInteraction 			interact;
	
	private GUISkin skin;
	
	//Function DynShelter--------	
	private DynShelterModel _dsMdl;
	
	private DynShelterModManager _modManager;
	
	private DynShelterModule _currentModule;
	
	private ArrayList _modules = new ArrayList();
	
	private string[] _tmpStylesNames;
	
	private Texture2D[] _tmpStylesThumbs;
	
	private AssetBundle _assetBundle;
	
	private int _selectedIndex = 0;
	private int m_icounterFacade = 0;
	private int m_icurrentColor = 0;
	
	private bool _nextInsertion = true;
	private float _decalZ = 0.0f;
	private float _decalZRemove = 0.0f;
	private bool _removeModule = true;
	private bool _removeNext = true;
	private bool _isBuilding;
	
	private Quaternion _savedRotation;			//rotation de l'abri
	private Vector3 _savedScale;				//scale de l'abri
	private Vector3 _fwdLimit,_bwdLimit;
	private Vector3 _fwdLimitMax,_bwdLimitMax;
	
	private OSLib 	 _lib;
	
	private bool _hasFacadeInDefaultEnd = false;
	private string _saveStyleEnd="1";
	private	int _saveTailleEnd=1;
	
	private bool _hasFacadeInDefaultBegin = false;
	private string _saveStyleBegin="1";
	private	int _saveTailleBegin=1;
	
	//UI--------------------------------------
	public enum UISelector
	{
		styles,
		colors,
		addNext,
		addPrev,
		limits,
		mods,
		none
	}
	
	private UISelector _currentUI = UISelector.none;
	
	private bool _isLockedNext = false;
	private bool _isLockedPrev = false;

	private Rect _upperArea;
	private Rect _leftArea;

	
	private string _currentType;
	
	private Texture2D _blank;
	
	private int _scrollPos;
	
	private GUISkin _uiSkin;
	
	private GameObject _arrowNxt,_arrowPrv,_limitFwd,_limitBwd;
	
	//CONSTRUCTION----------------------------
	public bool _canAddNxtBig,_canAddNxtSme,_canAddNxtSml,_canAddNxtF,_canAddNxtE;
	public bool _canAddPrvBig,_canAddPrvSme,_canAddPrvSml,_canAddPrvF,_canAddPrvE;
	private bool _canRemoveCurrent;
	
	private int _nextAdds=0,_prevAdds=0;
	
	private const string _matColorTag = "_color";
	private const string _matColorTag2 = "_Color";
	
	private DynShelterUI UI;
	
	private static int btnSize = 64;
	
	private Color32 _currentColor;
	private int _currentColorIndex = 0;
	private int _currentStyle;
	
	//MANIPULATION-----------------------------
	private float _moveSpeed = 1f;
	private static float _maxSpeed = 4f;
	private float _resetSpeedTimer = 0;
	private static float _resetSpeedDelay = 0.2f;
		
	//Shadow
	private bool _firstBuild = true;
	private float _oldDiag;
	
	private string szbrand = "";
	
	
	
#endregion

#region Unity Fcns
	
	// Use this for initialization
	void Start ()
	{
		if(_uiSkin == null)
			_uiSkin = (GUISkin)Resources.Load("skins/DynShelterSkin");
		
		UI = new DynShelterUI(this);
		
		menuConf = GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>();
		menuInteract = GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>();
		interact = Camera.main.GetComponent<ObjInteraction>();
		
		_dsMdl = new DynShelterModel(this);
		
		_nextInsertion = true;
		
		StartCoroutine(LoadConf());
		
		_upperArea = new Rect(Screen.width/2 - 300,225,600,120);
		_leftArea = new Rect(0.0f, 0.0f, Screen.width * 2.0f, Screen.height);
		
		
		InstantiateRessources();
		
		UsefullEvents.mode2DStateUpdated += QuitOverride;
		UsefullEvents.OnResizeWindowEnd += UIRelocate;
		
		CenterDeployAndFeetLimit();
	}
	
    void OnDestroy()
	{
		UsefullEvents.mode2DStateUpdated -= QuitOverride;
		UsefullEvents.OnResizeWindowEnd -= UIRelocate;
	}
	// Update is called once per frame
	void FixedUpdate ()
	{


		UI.UIUpdate();
		
		if(_currentModule != null)
		{
			if(szbrand == "PoolCover")
			{
				_arrowNxt.transform.localPosition = _currentModule.GetExtPosNoOverride(false);
				_arrowPrv.transform.localPosition = _currentModule.GetExtPosNoOverride(true);
				
				if((_selectedIndex == 0 || _selectedIndex == _modules.Count-1)&& _currentModule.GetModuleType() == ModuleType.facade)
				{
					_arrowNxt.transform.localPosition += new Vector3(0,0,-1);
					_arrowPrv.transform.localPosition += new Vector3(0,0,1);
				}
			}
			else
			{
				_arrowNxt.SetActive(false);
				_arrowPrv.SetActive(false);
			}
		}
		
		if(/*_currentUI == UISelector.limits*/_limitBwd.activeSelf && _limitFwd.activeSelf)
		{
			_limitFwd.transform.localPosition = _fwdLimit;
			_limitBwd.transform.localPosition = _bwdLimit;
		}
		
		if(_resetSpeedTimer>=0)
		{
			_resetSpeedTimer += Time.deltaTime;
			if(_resetSpeedTimer>_resetSpeedDelay)
			{
				_resetSpeedTimer = -1;
				_moveSpeed = 1f;
			}
		}
		
	}
	
	void OnGUI()
	{
		if(_isBuilding)
			return;
		
		GUI.skin = _uiSkin;
		
		if(GUI.Button(new Rect(Screen.width/2 - 75,0,150,50),"Validate","Validate"))
			Validate();
			
		if(szbrand == "")
		{
			szbrand = GetComponent<ObjData>().GetObjectModel().GetBrand();
		}
		
		if(szbrand == "PoolCover")
		{
			_upperArea = new Rect(Screen.width/2 - 300,30,600,120);
			
			if(!IsAbrifixe())
			{
				UI.GetGUILeft();	
			}
			
			UI.GetGUIRight();
			
			GUIOldUpperDisplay();
		}
		else
		{
			UI.GetGUIShelterEditor();
			
			GUIUpperDisplay();
		}
		
		UI.GetGUIDesigner ();
		
		//HUD LEFT SECTION > Selection module
//		GUIModuleSelector();
		
		//HUD General a droite
//		GUIMainMenu();

//		GUI3D();		
	}
	
	void OnEnable()
	{
		if(_modManager != null)
			_modManager.enabled = true;
		//if(IsAbrifixe())
		//	return;
		if(_arrowNxt != null)
		{
			if(szbrand == "PoolCover")
			{
				_arrowNxt.GetComponent<Renderer>().enabled = true;
				_arrowPrv.GetComponent<Renderer>().enabled = true;
			}
			else
			{
				_arrowNxt.GetComponent<Renderer>().enabled = false;
				_arrowPrv.GetComponent<Renderer>().enabled = false;
			}
		}
	}
	
	void OnDisable()
	{
		if(szbrand == "PoolCover")
		{
			UpdateVisibility(true);
		}
		
		ActiveLimits(false);
		
		if(szbrand == "PoolCover")
		{
			_arrowNxt.GetComponent<Renderer>().enabled = false;
			_arrowPrv.GetComponent<Renderer>().enabled = false;
		}
			
		_modManager.enabled = false;
	}
	
#endregion
	
#region Fcns DynShelter
	
//--------------GUI FCN's---------------------------------

	public void AddModule(int _ioffsetSize, ModuleType _type)
	{
		StartCoroutine(AddModule(_currentModule.GetSize() + _ioffsetSize,_dsMdl.GetFirstStyle(_type,_currentModule.GetSize()),_type,_currentColor));
	}
	
	public void SetSelectedIndex(int _i)
	{
		_selectedIndex = _i;
	}
	
	public int GetSelectedIndex()
	{
		return _selectedIndex;
	}
	
	public Vector3 GetLimitBwd()
	{
		return _limitBwd.transform.localPosition; 
		//return _bwdLimit;
	}
	
	public Vector3 GetLimitFwd()
	{
		return _limitFwd.transform.localPosition; 
		//return _fwdLimit;
	}
	
	public Vector3 GetMaxLimitBwd()
	{
		return _bwdLimitMax;
	}
	
	public Vector3 GetMaxLimitFwd()
	{
		return _fwdLimitMax;
	}
	
	public DynShelterModule GetModule(int _iid)
	{
		if(_iid < _modules.Count && _iid >= 0)
		{ 
			return ((DynShelterModule)_modules[_iid]);
		}
		
		return null;
	}
	
	public void MoveLimitBwd(float _fz)
	{
		_bwdLimit.z += _fz;
		_bwdLimit.z = Mathf.Clamp(_bwdLimit.z,_bwdLimitMax.z,((DynShelterModule)_modules[_modules.Count-1]).GetExtPos(false).z);
	}
	
	public void MoveLimitFwd(float _fz)
	{
		_fwdLimit.z += _fz;
		_fwdLimit.z = Mathf.Clamp(_fwdLimit.z,((DynShelterModule)_modules[0]).GetExtPos(true).z,_fwdLimitMax.z);
	}
	
	public void ActiveLimits(bool _b)
	{
		_limitFwd.SetActive(_b);
		_limitBwd.SetActive(_b);
	}
	
	public Vector3 GetPositionModule(int _iid)
	{
		if(_iid < _modules.Count && _iid >= 0)
		{
			return ((DynShelterModule)_modules[_iid]).GetPos();
		}

		return Vector3.zero;
	}
	
	public int GetNumberStyle()
	{
		return _tmpStylesThumbs.Length;
	}
	
	public int GetNumberFacade()
	{
		return m_icounterFacade;
	}
	
	public bool CanAddNext()
	{
		return ((_canAddNxtBig || _canAddNxtSme || _canAddNxtSml) && CanAddBloc()) || (_canAddNxtF && CanAddFacade());
	}
	
	public bool CanAddPrev()
	{
		return ((_canAddPrvBig || _canAddPrvSme || _canAddPrvSml) && CanAddBloc()) || (_canAddPrvF && CanAddFacade());
	}
	
	private bool CanAddBloc()
	{
		return _modules.Count < (10 + m_icounterFacade);
	}
	
	private bool CanAddFacade()
	{
		return m_icounterFacade < 2;
	}
	
	public int GetNumberBloc()
	{
		return _modules.Count - m_icounterFacade;
	}
	
	public void CenterDeployAndFeetLimit(bool _blimit = true)
	{
		if(_modules.Count > 0)
		{
			List<DynShelterModule> tempListDsm = new List<DynShelterModule>();
			
			foreach(DynShelterModule dsm in _modules)
			{
				if(dsm.GetModuleType() != ModuleType.facade)
				{
					tempListDsm.Add(dsm);
				}
			}
			
			bool bleftFacade = ((DynShelterModule)_modules[_modules.Count - 1]).GetModuleType() == ModuleType.facade;
			bool brightFacade = ((DynShelterModule)_modules[0]).GetModuleType() == ModuleType.facade;
			
			bool bpair = (tempListDsm.Count - 1) % 2 != 0;
			
			int iid = (int)((tempListDsm.Count) * 0.5f);
			
			if(iid >= 0 && iid <= tempListDsm.Count - 1)
			{
				DynShelterModule centerDsm = tempListDsm[iid];
				
				float fvalue = bpair  ? -centerDsm.GetIntOffSet() : 0.0f;
				float foffsetFacade = 0.0f;
					
				centerDsm.SetPos(false, fvalue);
					
				DynShelterModule nextDsm = centerDsm.GetNextModule();
				while(nextDsm != null)
				{
					if(nextDsm.GetModuleType() != ModuleType.facade)
					{
						fvalue -= (nextDsm.GetIntOffSet() * 2.0f);
					}
					else
					{
						fvalue -= centerDsm.GetIntOffSet();
						
						foffsetFacade += centerDsm.GetIntOffSet();
					}
						
					nextDsm.SetPos(false, fvalue);
					
					nextDsm = nextDsm.GetNextModule();
				}
				
				if(_blimit || _bwdLimit.z > (fvalue - centerDsm.GetIntOffSet() + foffsetFacade))
				{
					_bwdLimit.z = fvalue - centerDsm.GetIntOffSet() + foffsetFacade;
					_limitBwd.transform.localPosition = _bwdLimit;
				}
				
				fvalue = bpair ? -centerDsm.GetIntOffSet() : 0.0f;
				
				DynShelterModule prevDsm = centerDsm.GetPrevModule();	
				while(prevDsm != null)
				{
					if(prevDsm.GetModuleType() != ModuleType.facade)
					{
						fvalue += (prevDsm.GetIntOffSet() * 2.0f);
					}
					else
					{
						fvalue += centerDsm.GetIntOffSet();
						
						foffsetFacade += centerDsm.GetIntOffSet();
					}
					
					prevDsm.SetPos(false, fvalue);
					
					prevDsm = prevDsm.GetPrevModule();
				}
				
				if(bleftFacade)
				{
					foffsetFacade -= centerDsm.GetIntOffSet();
				}
				
				if(_blimit || _fwdLimit.z < (fvalue + centerDsm.GetIntOffSet() - foffsetFacade))
				{
					_fwdLimit.z = fvalue + centerDsm.GetIntOffSet() - foffsetFacade;
					_limitFwd.transform.localPosition = _fwdLimit;
				}
			}
		}
	}
	
	private void GUIUpperDisplay()
	{
		//HUD UPPER SECTION > addings,styles,colors,limits
		if(_selectedIndex != -1)
		{
			//---vv--INSIDE GROUP--vv------------
			if(_currentUI == UISelector.styles)
			{
				GUI.BeginGroup(UI.rectGroupDS,"","");
				
				for(int i = 0; i < 4; i++)
				{
					string sz = "style" + i.ToString();
					
					if(i < _tmpStylesThumbs.Length && i != _currentStyle)
					{
						
						if(GUI.Button(new Rect(UI.rectGroupDS.width * (0.525f + (0.1f * i)), UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", sz))
						{
							DynShelterModule centerDsm = ((DynShelterModule)_modules[(int)((_modules.Count - m_icounterFacade) * 0.5f)]);

							StartCoroutine(ChangeModuleStyle(i, centerDsm.GetPos().z > _currentModule.GetPos().z));
							_currentStyle = i;
							UI.ResetSelectionUI();
						}
					}
					else if(i == _currentStyle)
					{
						sz += "On";
						GUI.Box(new Rect(UI.rectGroupDS.width * (0.525f + (0.1f * i)), UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", sz);
					}
					else
					{
						sz += "Off";
						GUI.Box(new Rect(UI.rectGroupDS.width * (0.525f + (0.1f * i)), UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", sz);
					}
				}
				
				GUI.EndGroup();
			}
			
			else if(_currentUI ==  UISelector.addNext) 
			{
				GUINextAdd();
			}
			
			else if(_currentUI ==  UISelector.addPrev)
			{
				GUIPrevAdd();
			}
			
			else if(_currentUI == UISelector.colors)
			{
				GUIColors();	
			}
			else if(_currentUI == UISelector.none)
			{
				GUIDefault();	
			}
		}
	}
	
	private void GUIOldUpperDisplay()
	{
		//HUD UPPER SECTION > addings,styles,colors,limits
		if(_selectedIndex != -1)
		{
			if(_currentUI != UISelector.mods && _currentUI != UISelector.none)
			{
				GUI.Box(_upperArea,"","hudBg");
			}
			GUI.BeginGroup(_upperArea);
			//---vv--INSIDE GROUP--vv------------
			if(_currentUI == UISelector.styles)
			{
				int startPos = (int)(_upperArea.width - _tmpStylesThumbs.Length*btnSize)/2;
				for(int i=0;i<_tmpStylesThumbs.Length;i++)
				{
					//					if(GUI.Button(new Rect(startPos+i*btnSize,btnSize/2,btnSize,btnSize),_tmpStylesThumbs[i],"btnStyle"))
					//					{
					//						StartCoroutine(ChangeModuleStyle(i));
					//					}
					bool tmp = GUI.Toggle(new Rect(startPos+i*btnSize,btnSize/2,btnSize,btnSize),(i == _currentStyle),_tmpStylesThumbs[i],"btnStyle");
					if(tmp != (i == _currentStyle))
					{
						StartCoroutine(ChangeModuleStyle(i));
						_currentStyle = i;
					}
				}
			}
			
			else if(_currentUI ==  UISelector.addNext) 
			{
				GUIOldNextAdd();
			}
			
			else if(_currentUI ==  UISelector.addPrev)
			{
				GUIOldPrevAdd();
			}
			
			else if(_currentUI == UISelector.colors)
			{
				GUIOldColors();	
			}
			
			else if(_currentUI == UISelector.limits)
			{
				GUIOldLimitsSelector();	
			}
			//---^^--INSIDE GROUP--^^------------
			GUI.EndGroup();
		}
	}
	
	private void GUIDefault()
	{
		GUI.BeginGroup(UI.rectGroupDS,"","");
		
		GUI.Box(new Rect(UI.rectGroupDS.width * 0.625f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSmallerOff");

		GUI.Box(new Rect(UI.rectGroupDS.width * 0.725f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSameOff");
	
		GUI.Box(new Rect(UI.rectGroupDS.width * 0.525f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addBiggerOff");
	
		GUI.Box(new Rect(UI.rectGroupDS.width * 0.825f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addFacadeOff");
		
		GUI.EndGroup();
	}
	private void GUINextAdd()
	{
		GUI.BeginGroup(UI.rectGroupDS,"","");
		
		if(_canAddNxtSml && _modules.Count < (10 + m_icounterFacade))
		{
			if(GUI.Button(new Rect(UI.rectGroupDS.width * 0.625f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSmaller"))
			{
				//Faire un check du style car si facade sélectionné > marche pas
				StartCoroutine(AddModule(_currentModule.GetSize()-1,_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
			}
		}
		else
		{
			GUI.Box(new Rect(UI.rectGroupDS.width * 0.625f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSmallerOff");
		}
		
		if(_canAddNxtSme && _modules.Count < (10 + m_icounterFacade))
		{
			if(GUI.Button(new Rect(UI.rectGroupDS.width * 0.725f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSame"))
			{
				//Faire un check du style car si facade sélectionné > marche pas
				StartCoroutine(AddModule(_currentModule.GetSize(),_currentModule.GetStyle(),ModuleType.bloc/*multiBloc*/,_currentColor));
			}
		}
		else
		{
			GUI.Box(new Rect(UI.rectGroupDS.width * 0.725f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSameOff");
		}
		
		if(_canAddNxtBig && _modules.Count < (10 + m_icounterFacade))
		{
			if(GUI.Button(new Rect(UI.rectGroupDS.width * 0.525f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addBigger"))
			{
				//Faire un check du style car si facade sélectionné > marche pas
				StartCoroutine(AddModule(_currentModule.GetSize()+1,_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
			}
		}
		else
		{
			GUI.Box(new Rect(UI.rectGroupDS.width * 0.525f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addBiggerOff");
		}
		
		if(_canAddNxtF && m_icounterFacade < 2)
		{
			if(GUI.Button(new Rect(UI.rectGroupDS.width * 0.825f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addFacade"))
			{
				StartCoroutine(AddModule(_currentModule.GetSize(),_dsMdl.GetFirstStyle(ModuleType.facade,_currentModule.GetSize()),ModuleType.facade,_currentColor,false,true));
			}
		}
		else
		{
			GUI.Box(new Rect(UI.rectGroupDS.width * 0.825f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addFacadeOff");
		}
		
		GUI.EndGroup();
	}
	private void GUIPrevAdd()
	{
		GUI.BeginGroup(UI.rectGroupDS,"","");
		
		if(_canAddPrvSml && _modules.Count < (10 + m_icounterFacade))
		{
			if(GUI.Button(new Rect(UI.rectGroupDS.width * 0.625f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSmaller"))
			{
				StartCoroutine(AddModule(_currentModule.GetSize()-1,_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
			}
		}
		else
		{
			GUI.Box(new Rect(UI.rectGroupDS.width * 0.625f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSmallerOff");
		}
		
		if(_canAddPrvSme && _modules.Count < (10 + m_icounterFacade))
		{
			if(GUI.Button(new Rect(UI.rectGroupDS.width * 0.725f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSame"))
			{
				//Faire un check du style car si facade sélectionné > marche pas
				StartCoroutine(AddModule(_currentModule.GetSize(),_currentModule.GetStyle(),ModuleType.bloc/*multiBloc*/,_currentColor));
			}
		}
		else
		{
			GUI.Box(new Rect(UI.rectGroupDS.width * 0.725f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addSameOff");
		}
			
		if(_canAddPrvBig && _modules.Count < (10 + m_icounterFacade))
		{
			if(GUI.Button(new Rect(UI.rectGroupDS.width * 0.525f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addBigger"))
			{
				StartCoroutine(AddModule(_currentModule.GetSize()+1,_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
			}
		}
		else
		{
			GUI.Box(new Rect(UI.rectGroupDS.width * 0.525f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addBiggerOff");
		}
		
		if(_canAddPrvF && m_icounterFacade < 2)
		{
			if(GUI.Button(new Rect(UI.rectGroupDS.width * 0.825f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addFacade"))
			{
				StartCoroutine(AddModule(_currentModule.GetSize(),_dsMdl.GetFirstStyle(ModuleType.facade,_currentModule.GetSize()),ModuleType.facade,_currentColor));
			}
		}
		else
		{
			GUI.Box(new Rect(UI.rectGroupDS.width * 0.825f, UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "addFacadeOff");
		}
		
		GUI.EndGroup();
	}
	private void GUIColors()
	{
		GUI.BeginGroup(UI.rectGroupDS,"","");
		
		int i = 0;
		for(int col = m_icurrentColor; col < m_icurrentColor + 4; col++)
		{
			Rect pos = new Rect(UI.rectGroupDS.width * (0.525f + (0.1f * i)), UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f);
			GUI.color = _dsMdl.GetColorList()[col];
			GUI.DrawTexture(pos,_blank);
			GUI.color = Color.white;
			
			if(GUI.Toggle(new Rect(UI.rectGroupDS.width * (0.525f + (0.1f * i)), UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.225f), 48.0f, 48.0f),col==_currentColorIndex, "", "colorBtn"))
			{
				if(col!=_currentColorIndex)
				{
					_currentColorIndex = col;
					
					UI.ResetSelectionUI();
					ChangeModuleColor(_dsMdl.GetColorList()[_currentColorIndex]);
				}
			}
			
			i++;
		}
		
		if(m_icurrentColor > 0 &&
		   GUI.Button(new Rect((UI.rectGroupDS.width * 0.45f) + (23.25f * 0.5f), UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.2f), 23.25f, 32.25f), "", "prevArrow"))
		{
			m_icurrentColor -= 4;
			
			if(m_icurrentColor < 0)
			{
				m_icurrentColor = 0;
			}
		}
		else
		{
			GUI.Box(new Rect((UI.rectGroupDS.width * 0.45f) + (23.25f * 0.5f), UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.2f), 23.25f, 32.25f), "", "prevArrowOff");
		}
		
		if(m_icurrentColor < _dsMdl.GetColorList().Length - 4 &&
		   GUI.Button(new Rect((UI.rectGroupDS.width * 0.95f) - (23.25f), UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.2f), 23.25f, 32.25f), "", "nextArrow"))
		{
			m_icurrentColor += 4;
			
			if(m_icurrentColor > _dsMdl.GetColorList().Length - 4)
			{
				m_icurrentColor = _dsMdl.GetColorList().Length - 4;
			}
		}
		else
		{
			GUI.Box(new Rect((UI.rectGroupDS.width * 0.95f) - (23.25f), UI.rectGroupDS.height - (UI.rectGroupDS.height * 0.2f), 23.25f, 32.25f), "", "nextArrowOff");
		}
		
		GUI.EndGroup();
		
	}
	private void GUILimitsSelector()
	{
//		float startPos = Screen.width/2;
		float startPos = _upperArea.width/2f;
		
		//----vv--Limit AV--vv----------
		if(GUI.RepeatButton(new Rect(startPos-25-(3*btnSize),btnSize/2,btnSize,btnSize),"+","btn+"))
		{
			_fwdLimit.z +=0.1f;
			_fwdLimit.z = Mathf.Clamp(_fwdLimit.z,((DynShelterModule)_modules[0]).GetExtPos(true).z,_fwdLimitMax.z);
		}
		
		GUI.Label(new Rect(startPos-25-(2*btnSize),btnSize/2,btnSize,btnSize),/*TextManager.GetText("DynShelter.LimitFront")*/"","move+");
		
		if(GUI.RepeatButton(new Rect(startPos-25-(1*btnSize),btnSize/2,btnSize,btnSize),"-","btn-"))
		{
			_fwdLimit.z -=0.1f;
			_fwdLimit.z = Mathf.Clamp(_fwdLimit.z,((DynShelterModule)_modules[0]).GetExtPos(true).z,_fwdLimitMax.z);
		}
		
		//----vv--Limit AR--vv----------
		if(GUI.RepeatButton(new Rect(startPos+25,btnSize/2,btnSize,btnSize),"-","btn-"))
		{
			_bwdLimit.z +=0.1f;
			_bwdLimit.z = Mathf.Clamp(_bwdLimit.z,_bwdLimitMax.z,((DynShelterModule)_modules[_modules.Count-1]).GetExtPos(false).z);
		}
		
		GUI.Label(new Rect(startPos+25+(1*btnSize),btnSize/2,btnSize,btnSize),/*TextManager.GetText("DynShelter.LimitBack")*/"","move-");
		
		if(GUI.RepeatButton(new Rect(startPos+25+(2*btnSize),btnSize/2,btnSize,btnSize),"+","btn+"))
		{
			_bwdLimit.z -=0.1f;
			_bwdLimit.z = Mathf.Clamp(_bwdLimit.z,_bwdLimitMax.z,((DynShelterModule)_modules[_modules.Count-1]).GetExtPos(false).z);
		}
		
	}
	private void GUIOldNextAdd()
	{
		float startPos = (float)(_upperArea.width - _nextAdds*btnSize)/2f;
		if(_canAddNxtSml)
		{
			GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
			GUI.DrawTexture(new Rect(startPos,btnSize/2,btnSize,btnSize),addSmaller);
			if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
			{
				//Faire un check du style car si facade sélectionné > marche pas
				StartCoroutine(AddModule(_currentModule.GetSize()-1,_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
			}
			startPos +=btnSize;
		}
		if(_canAddNxtSme)
		{
			if(!IsAbrifixe())
			{
				GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
				GUI.DrawTexture(new Rect(startPos,btnSize/2,btnSize,btnSize),addSameFree);
				if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
				{
					//Faire un check du style car si facade sélectionné > marche pas
					StartCoroutine(AddModule(_currentModule.GetSize(),_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
				}
				startPos +=btnSize;
			}
			
			//	if(!_currentModule.GetStyle().StartsWith("f"))
			{
				GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
				GUI.DrawTexture(new Rect(startPos,btnSize/2,btnSize,btnSize),addSame);
				if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
				{
					//Faire un check du style car si facade sélectionné > marche pas
					StartCoroutine(AddModule(_currentModule.GetSize(),_currentModule.GetStyle(),ModuleType.multiBloc,_currentColor));
				}
				startPos +=btnSize;
			}
		}
		if(_canAddNxtBig)
		{
			GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
			GUI.DrawTexture(new Rect(startPos,btnSize/2,btnSize,btnSize),addBigger);
			if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
			{
				//Faire un check du style car si facade sélectionné > marche pas
				StartCoroutine(AddModule(_currentModule.GetSize()+1,_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
			}
			startPos +=btnSize;
		}
		//	if(!IsAbrifixe())
		{
			if(_canAddNxtF)
			{
				GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
				GUI.DrawTexture(new Rect(startPos,btnSize/2,btnSize,btnSize),addFacade);
				if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
				{
					StartCoroutine(AddModule(_currentModule.GetSize(),_dsMdl.GetFirstStyle(ModuleType.facade,_currentModule.GetSize()),ModuleType.facade,_currentColor));
				}
				startPos +=btnSize;
			}
			if(_canAddNxtE)
			{
				GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
				GUI.DrawTexture(new Rect(startPos,btnSize/2,btnSize,btnSize),addExtrem);
				if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
				{
					StartCoroutine(AddModule(_currentModule.GetSize(),_dsMdl.GetFirstStyle(ModuleType.extremite,_currentModule.GetSize()),ModuleType.extremite,_currentColor));
				}
				startPos +=btnSize;
			}
		}
	}
	private void GUIOldPrevAdd()
	{
		float startPos =(float) (_upperArea.width - _prevAdds*btnSize)/2f;
		if(_canAddPrvSml)
		{
			GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
			GUI.DrawTexture(new Rect(startPos+btnSize,btnSize/2,-btnSize,btnSize),addSmaller);
			if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
			{
				StartCoroutine(AddModule(_currentModule.GetSize()-1,_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
			}
			startPos +=btnSize;
		}
		if(_canAddPrvSme)
		{
			if(!IsAbrifixe())
			{
				GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
				GUI.DrawTexture(new Rect(startPos+btnSize,btnSize/2,-btnSize,btnSize),addSameFree);
				if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
				{
					
					StartCoroutine(AddModule(_currentModule.GetSize(),_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
				}
				startPos +=btnSize;
			}
			
			GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
			GUI.DrawTexture(new Rect(startPos,btnSize/2,btnSize,btnSize),addSame);
			if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
			{
				//Faire un check du style car si facade sélectionné > marche pas
				StartCoroutine(AddModule(_currentModule.GetSize(),_currentModule.GetStyle(),ModuleType.multiBloc,_currentColor));
			}
			startPos +=btnSize;
		}
		if(_canAddPrvBig)
		{
			GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
			GUI.DrawTexture(new Rect(startPos+btnSize,btnSize/2,-btnSize,btnSize),addBigger);
			if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
			{
				StartCoroutine(AddModule(_currentModule.GetSize()+1,_currentModule.GetStyle(),ModuleType.bloc,_currentColor));
			}
			startPos +=btnSize;
		}
		//if(!IsAbrifixe())
		{
			if(_canAddPrvF)
			{
				GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
				GUI.DrawTexture(new Rect(startPos+btnSize,btnSize/2,-btnSize,btnSize),addFacade);
				if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
				{
					StartCoroutine(AddModule(_currentModule.GetSize(),_dsMdl.GetFirstStyle(ModuleType.facade,_currentModule.GetSize()),ModuleType.facade,_currentColor));
				}
				startPos +=btnSize;
			}
			if(_canAddPrvE)
			{
				GUI.Label(new Rect(startPos,btnSize/2,btnSize,btnSize),"","udButton");
				GUI.DrawTexture(new Rect(startPos+btnSize,btnSize/2,-btnSize,btnSize),addExtrem);
				if(GUI.Button(new Rect(startPos,btnSize/2,btnSize,btnSize),"","empty"))
				{
					StartCoroutine(AddModule(_currentModule.GetSize(),_dsMdl.GetFirstStyle(ModuleType.extremite,_currentModule.GetSize()),ModuleType.extremite,_currentColor));
				}
				startPos +=btnSize;
			}
		}
	}
	private void GUIOldColors()
	{
		//		GUILayout.BeginArea(new Rect(Screen.width/2-150,0,300,btnSize));
		//		GUILayout.BeginArea(new Rect(0,0,_upperArea.width,_upperArea.height));
		//		GUILayout.FlexibleSpace();
		//		
		//		_scrollPos = GUILayout.BeginScrollView(_scrollPos);
		//		GUILayout.BeginHorizontal();
		//		
		//		foreach(Color32 c in _dsMdl.GetColorList())
		//		{
		//			GUI.color = c;
		//			if(GUILayout.Button(_blank,GUILayout.Height(btnSize),GUILayout.Width(btnSize)))
		//				ChangeModuleColor(c);
		//		}
		//		GUI.color = Color.white;
		//		
		//		GUILayout.EndHorizontal();
		//		GUILayout.EndScrollView();
		//		
		//		GUILayout.FlexibleSpace();
		//		GUILayout.EndArea();
		
		//		GUILayout.BeginHorizontal("",GUILayout.Height(btnSize),GUILayout.Width(_upperArea.width));
		//		GUILayout.FlexibleSpace();
		//		
		//		if(GUILayout.Button("","btn<",GUILayout.Width(btnSize),GUILayout.Height(btnSize)))
		//		{
		//			_scrollPos --;
		//			if(_scrollPos < 0)
		//				_scrollPos = 0;
		//			
		//		}
		//		//----------
		//		for(int col=_scrollPos;col<_scrollPos+4;col++)
		//		{
		//			GUI.color = _dsMdl.GetColorList()[col];
		//			if(GUILayout.Button(_blank,"colorBtn",GUILayout.Height(btnSize),GUILayout.Width(btnSize)))
		//			{
		//				UI.ResetSelectionUI();
		//				UpdateVisibility(true);
		//				ChangeModuleColor(_dsMdl.GetColorList()[col]);
		//			}
		//			
		//		}
		//		GUI.color = Color.white;
		//		//----------
		//		if(GUILayout.Button("","btn>",GUILayout.Width(btnSize),GUILayout.Height(btnSize)))
		//		{
		//			_scrollPos ++;
		//			if(_scrollPos+4 > _dsMdl.GetColorList().Length)
		//				_scrollPos = _dsMdl.GetColorList().Length-4;
		//		}
		//		
		//		GUILayout.FlexibleSpace();
		//		GUILayout.EndHorizontal();
		
		if(GUI.Button(new Rect(btnSize,btnSize/2,btnSize,btnSize),"","btn<"))
		{
			_scrollPos --;
			if(_scrollPos < 0)
				_scrollPos = 0;
		}
		
		//----------
		for(int col=_scrollPos;col<_scrollPos+6;col++)
		{
			Rect pos = new Rect((2+col-_scrollPos)*btnSize,btnSize/2,btnSize,btnSize);
			GUI.color = _dsMdl.GetColorList()[col];
			GUI.DrawTexture(pos,_blank);
			GUI.color = Color.white;
			
			if(GUI.Toggle(pos,col==_currentColorIndex,"","colorBtn"))
			{
				if(col!=_currentColorIndex)
				{
					_currentColorIndex = col;
					
					UI.ResetSelectionUI();
					UpdateVisibility(true);
					ChangeModuleColor(_dsMdl.GetColorList()[_currentColorIndex]);
				}
			}
			
		}
		
		//----------
		
		if(GUI.Button(new Rect(_upperArea.width - 2*btnSize,btnSize/2,btnSize,btnSize),"","btn>"))
		{
			_scrollPos ++;
			if(_scrollPos+6 > _dsMdl.GetColorList().Length)
				_scrollPos = _dsMdl.GetColorList().Length-6;
		}
		
	}
	private void GUIOldLimitsSelector()
	{
		//		float startPos = Screen.width/2;
		float startPos = _upperArea.width/2f;
		
		//----vv--Limit AV--vv----------
		if(GUI.RepeatButton(new Rect(startPos-25-(3*btnSize),btnSize/2,btnSize,btnSize),"+","btn+"))
		{
			_fwdLimit.z +=0.1f;
			_fwdLimit.z = Mathf.Clamp(_fwdLimit.z,((DynShelterModule)_modules[0]).GetExtPos(true).z,_fwdLimitMax.z);
		}
		
		GUI.Label(new Rect(startPos-25-(2*btnSize),btnSize/2,btnSize,btnSize),/*TextManager.GetText("DynShelter.LimitFront")*/"","move+");
		
		if(GUI.RepeatButton(new Rect(startPos-25-(1*btnSize),btnSize/2,btnSize,btnSize),"-","btn-"))
		{
			_fwdLimit.z -=0.1f;
			_fwdLimit.z = Mathf.Clamp(_fwdLimit.z,((DynShelterModule)_modules[0]).GetExtPos(true).z,_fwdLimitMax.z);
		}
		
		//----vv--Limit AR--vv----------
		if(GUI.RepeatButton(new Rect(startPos+25,btnSize/2,btnSize,btnSize),"-","btn-"))
		{
			_bwdLimit.z +=0.1f;
			_bwdLimit.z = Mathf.Clamp(_bwdLimit.z,_bwdLimitMax.z,((DynShelterModule)_modules[_modules.Count-1]).GetExtPos(false).z);
		}
		
		GUI.Label(new Rect(startPos+25+(1*btnSize),btnSize/2,btnSize,btnSize),/*TextManager.GetText("DynShelter.LimitBack")*/"","move-");
		
		if(GUI.RepeatButton(new Rect(startPos+25+(2*btnSize),btnSize/2,btnSize,btnSize),"+","btn+"))
		{
			_bwdLimit.z -=0.1f;
			_bwdLimit.z = Mathf.Clamp(_bwdLimit.z,_bwdLimitMax.z,((DynShelterModule)_modules[_modules.Count-1]).GetExtPos(false).z);
		}
		
	}
	
//--------------UPDATES-----------------------------------
	
	//Mets a jour la box collider generale
	private void UpdateBoxCollider()
	{
		float length = Vector3.Distance(((DynShelterModule)_modules[0]).GetExtPos(true),((DynShelterModule)_modules[_modules.Count-1]).GetExtPos(false));
		float w=0,h=0;
		BoxCollider bc = GetComponent<BoxCollider>();
		//--XX--XX--XX--XX--XX--XX--XX--XX--XX--XX--
		foreach(DynShelterModule m in _modules)
		{
			if(m.GetWidth() > w)
				w = m.GetWidth();
			if(m.GetHeight() > h)
				h = m.GetHeight();
			
			Transform obj = m.GetGameObj().transform;
			foreach(Transform t in obj)
			{
				if(Mathf.Abs(t.localPosition.x) > w/2f)
				{
					w = Mathf.Abs(t.localPosition.x) * 2;	
				}
				
				if(Mathf.Abs(t.localPosition.y) > h)
				{
					h = Mathf.Abs(t.localPosition.y) ;	
				}
			}
		}
		bc.size = new Vector3(w,(h*1.5f),length);
		Vector3 center = bc.center;
		center.y = (h*1.5f)/2;
		center.z = ((((DynShelterModule)_modules[0]).GetExtPos(true)+((DynShelterModule)_modules[_modules.Count-1]).GetExtPos(false))/2).z;
		bc.center = center;
		
		//--XX--XX--XX--XX--XX--XX--XX--XX--XX--XX--
		
		/*foreach (Transform trans in transform)
		{
			if(trans.gameObject.GetComponent<BoxCollider>()==null)
				trans.gameObject.AddComponent<BoxCollider>();
			BoxCollider collider = trans.gameObject.GetComponent<BoxCollider>();
			Bounds b = getMeshBounds(trans);
			collider.center = new Vector3(
				0, 
				b.extents.y/transform.localScale.y/2, 
				0);
			collider.extents = new Vector3(
				b.extents.x/transform.localScale.x, 
				b.extents.y/transform.localScale.y, 
				b.extents.z/transform.localScale.z);
			collider.enabled = true;
		}
		*/
		
		foreach (Transform trans in transform)
		{
			if(trans.gameObject.GetComponent<BoxCollider>()!=null)
				Destroy(trans.gameObject.GetComponent<BoxCollider>());
		}	
		
		//if(_firstBuild)
		{
			RecalcOldDiag();	
			UsefullEvents.FireReinitIosShadow(gameObject);
			_firstBuild = false;
		}
		/*else
		{	
			ScaleIosShadows();			
		}*/
	}	
	
	private void ScaleIosShadows()
	{
		float newDiag = _3Dutils.GetBoundDiag(gameObject.GetComponent<BoxCollider>().bounds);
	//	newDiag*=gameObject.transform.localScale.x;
		float factor = newDiag / _oldDiag;
		UsefullEvents.FireUpdateIosShadowScale(gameObject,factor);
//		_oldDiag = newDiag;
		
	}
	
	private void RecalcOldDiag()
	{
		_oldDiag = _3Dutils.GetBoundDiag(gameObject.GetComponent<BoxCollider>().bounds);
	}
	
	//Mets a jour le current module et tout ce qui en dépend (UI,règles ajout,...)
	public void SetSelected(int i)
	{
		if(!IsAbrifixe() || _isBuilding)
		{
			_selectedIndex = Mathf.Clamp(i,0,_modules.Count-1);
		}
		else
		{
			_selectedIndex = Mathf.Clamp(i,1,_modules.Count-2);
		}
		UpdateCurrentModule();
	}
	//Mets a jours le module courant
	public void UpdateCurrentModule()
	{
		//Debug.Log("selectedIndex : "+_selectedIndex+" count : "+_modules.Count );
		if(_modules.Count > 0)
		{
			/*if(_selectedIndex==-1)
				_selectedIndex = 0;
			if(_selectedIndex>=_modules.Count)
				_selectedIndex =_modules.Count-1;*/
			if(_selectedIndex==-1)
				return;
			if(_selectedIndex>=_modules.Count)
				return;
			_currentModule = (DynShelterModule) _modules[_selectedIndex];
			_tmpStylesNames = _dsMdl.GetStylesNameOfSize(_currentModule.GetSize(),_currentModule.GetModuleType());
			_tmpStylesThumbs = _dsMdl.GetStylesThumbsOfSize(_tmpStylesNames);
			
			_currentType = _currentModule.GetType().ToString();//_currentModule.GetModuleType().ToString();
			for(int i=0;i<_tmpStylesNames.Length;i++)
			{
				if(_tmpStylesNames[i] == _currentModule.GetStyle())
				{
					_currentStyle = i;
				}
			}
			UpdateRules();
			
			//Locks -------------------------
			_isLockedNext = _currentModule.GetNextLock();
			_isLockedPrev = _currentModule.GetPrevLock();
			
			//Mods
			_modManager.UpdateLocalMods(_currentModule.GetGameObj());
		}
		else
		{
			//CAS OU PLUS DE MODULES
			_currentModule = null;
		}
		UI.SetSelectedItem(_selectedIndex);
		UI.SetSelectedLocks(_isLockedPrev,_isLockedNext);
		
		
		if(szbrand == "PoolCover")
		{
			UpdateVisibility();
		}
	}
	
	//Mise jour des locks
	public void UpdateLocks(bool prev, bool next)
	{
		_isLockedPrev = prev;
		_isLockedNext = next;
		
		_currentModule.SetPrevLocks(_isLockedPrev);
		if(_currentModule.GetPrevModule() != null)
			_currentModule.GetPrevModule().SetNextLocks(_isLockedPrev);
		
		_currentModule.SetNextLocks(_isLockedNext);
		if(_currentModule.GetNextModule() != null)
			_currentModule.GetNextModule().SetPrevLocks(_isLockedNext);
	}
	
	//Déplacement du module sélectionné
	public bool MoveSelectedModule(bool fwd, float fspeed)
	{
		bool bmoved = false;
		
		if(_currentModule == null)
		{
			return bmoved;	
		}
		
		if(fwd)
		{
			bmoved = _currentModule.MoveModuleV2(/*_moveSpeed**/Time.deltaTime * fspeed,_currentModule);
			_resetSpeedTimer = 0;
		}
		else
		{
			bmoved = _currentModule.MoveModuleV2(-/*_moveSpeed**/Time.deltaTime * fspeed,_currentModule);
			_resetSpeedTimer = 0;
		}
		
		_moveSpeed = Mathf.Clamp(_moveSpeed + 2*Time.deltaTime,1,_maxSpeed);
		
		return bmoved;
	}
	
	//Mise a jour des limites si on Ajoute/Enlève un bloc		
	private void UpdateLimits(bool fromInit)
	{
		float countBloc = 0;
		float averageSize=0;
		float perfectSize=0;
		float decalZValue = 0.0f;
		
		for(int i=0;i<_modules.Count;i++)
		{
			DynShelterModule dm = (DynShelterModule) _modules[i];
			if(i==0)
			{
				_limitFwd.transform.FindChild("R").transform.localScale = new Vector3(dm.GetWidth()+1.0f,1,1);
				_limitFwd.transform.FindChild("T1").transform.localPosition = new Vector3((dm.GetWidth()+1.0f)/2f,0,0);
				_limitFwd.transform.FindChild("T2").transform.localPosition = new Vector3(-(dm.GetWidth()+1.0f)/2f,0,0);
				perfectSize+=dm.GetIntOffSet()*2.0f;
			}
			else if(i==_modules.Count-1)
			{
				_limitBwd.transform.FindChild("R").transform.localScale = new Vector3(dm.GetWidth()+1.0f,1,1);
				_limitBwd.transform.FindChild("T1").transform.localPosition = new Vector3((dm.GetWidth()+1.0f)/2f,0,0);
				_limitBwd.transform.FindChild("T2").transform.localPosition = new Vector3(-(dm.GetWidth()+1.0f)/2f,0,0);
			}
			
			if(dm.GetModuleType() != ModuleType.facade)
			{
				countBloc ++;
				averageSize += dm.GetLength();
			}
			//Calcul de la taille exact
			if(i>0)
			{
				DynShelterModule dmMoinsUn = (DynShelterModule) _modules[i-1];
				if(dmMoinsUn.GetSize()!=dm.GetSize())					
				{
					decalZValue = dm.GetIntOffSet()*2.0f;
					perfectSize+=decalZValue;
				}
				else
				{	
					if(dm.GetType()==typeof(DynShelterMultiBloc))
					{
						decalZValue = dm.GetIntOffSet()*2.0f;
						perfectSize+=decalZValue;
					}
					else
					{
						decalZValue = dm.GetLength();
						perfectSize+=decalZValue;	
					}
				}
								
			}
		}
		if(!fromInit)
		{		
			if(!_removeModule)
			{
				if(_nextInsertion)
				{
					_decalZ-=decalZValue/2;
				}
				else
				{
					_decalZ+=decalZValue/2;
				}
			}
			else
			{
				if(_removeNext)
				{
					_decalZ+=decalZValue/2;
				}
				else
				{
					_decalZ-=decalZValue/2;
				}
			}
		}
		if(countBloc > 0)
		{
			averageSize /= countBloc;
			
		/*	_fwdLimit = new Vector3(0,0,((countBloc/2f))*averageSize);
			_fwdLimitMax = new Vector3(0,0,((countBloc/2f)+3)*averageSize);
	
			_bwdLimit = new Vector3(0,0,((countBloc/2f))*-averageSize);
			_bwdLimitMax = new Vector3(0,0,((countBloc/2f)+3)*-averageSize);*/
			
			_fwdLimit = new Vector3(0,0,perfectSize/2.0f+0.1f+_decalZ);
			_fwdLimitMax = new Vector3(0,0,11.0f/*perfectSize*2/2.0f*1.3f+_decalZ*/);
	
			_bwdLimit = new Vector3(0,0,-perfectSize/2.0f-0.1f+_decalZ);
			_bwdLimitMax = new Vector3(0,0,-11.0f/*perfectSize*2/2.0f*1.3f+_decalZ*/);
		}

	}
	
	//Recentrage de l'abri
	private void UpdateModules() //Not Used Anymore or maybe?
	{
		DynShelterModule mod;
		Vector3 center = (((DynShelterModule) _modules[0]).GetPos() + ((DynShelterModule) _modules[_modules.Count-1]).GetPos())/2;
		float off7 = -center.z;

		
		for(int idx=0;idx<_modules.Count;idx++)
		{
			mod =(DynShelterModule) _modules[idx];
			if(mod.GetModuleType() == ModuleType.bloc)
			{
				mod.SetPos(true,off7);
//				mod.MoveModuleV2(off7,mod,false,false);
			}
		}
	}
	
	//Mise a jour des règles pour savoir si on peut ajouter un Next/Prev, de quelle taille, si on peut le supprimer ...
	private void UpdateRules()
	{
		DynShelterModule prev = _currentModule.GetPrevModule();
		DynShelterModule next = _currentModule.GetNextModule();
		
		//check si on peut le supprimer
		if(prev == null || next == null)
		{
			_canRemoveCurrent = true;
			UI.SetRemovability(true);
		}
		else
		{
			if(Mathf.Abs(prev.GetSize() - next.GetSize()) <=1)
			{	
				if(next.GetModuleType() == ModuleType.bloc && prev.GetModuleType() == ModuleType.bloc)
				{
					_canRemoveCurrent = true;
					UI.SetRemovability(true);
				}
				else if(next.GetModuleType() == ModuleType.facade && prev.GetModuleType() == ModuleType.bloc)
				{
					_canRemoveCurrent = true;
					UI.SetRemovability(true);
				}
				else if(next.GetModuleType() == ModuleType.bloc && prev.GetModuleType() == ModuleType.facade)
				{
					_canRemoveCurrent = true;
					UI.SetRemovability(true);
				}
				else{
					_canRemoveCurrent = false;
					UI.SetRemovability(false);
				}
			}
			else
			{
				_canRemoveCurrent = false;
				UI.SetRemovability(false);
			}
		}
		
		int nbBlocs = 0;
		foreach(DynShelterModule dsm in _modules)
		{
			if(dsm.GetModuleType() == ModuleType.bloc || dsm.GetModuleType() == ModuleType.multiBloc)
				nbBlocs ++;
		}
		if(_currentModule.GetModuleType() == ModuleType.bloc || _currentModule.GetModuleType() == ModuleType.multiBloc)
		{
			if(nbBlocs <=1)
			{
				_canRemoveCurrent = false;
				UI.SetRemovability(false);
			}
		}
		
		if(_currentModule.GetModuleType() == ModuleType.bloc)//SI current est un bloc
		{
			//CAS DU MODULE PRECEDENT-----------------------
			if(prev == null)//Si pas de module précedent
			{
				_canAddPrvBig = _canAddPrvSme = _canAddPrvSml = true;
				_canAddPrvF=_canAddPrvE = true;
			}
			else //si module précedent
			{
				_canAddPrvF = _canAddPrvE = false;
				if(prev.GetModuleType() == ModuleType.bloc) // si module précedent est un bloc
				{
					if(prev.GetSize() == _currentModule.GetSize())
					{
						_canAddPrvBig = _canAddPrvSme = _canAddPrvSml = true;
					}
					else if(prev.GetSize() > _currentModule.GetSize())
					{
						_canAddPrvBig = _canAddPrvSme = true;
						_canAddPrvSml = false;
					}
					else if(prev.GetSize() < _currentModule.GetSize())
					{
						_canAddPrvSml = _canAddPrvSme = true;
						_canAddPrvBig = false;
					}
				}
				else //Si module précedent facade ou extrem
				{
					//_canAddPrvSme = true;
					_canAddPrvSme = _canAddPrvBig = _canAddPrvSml = false;
				}
			}
			//CAS DU MODULE SUIVANT-------------------------
			if(next == null)//Si pas de module Suivant
			{
				_canAddNxtBig = _canAddNxtSme = _canAddNxtSml = true;
				_canAddNxtF = _canAddNxtE = true;
			}
			else //si module suivant
			{
				_canAddNxtF= _canAddNxtE = false;
				if(next.GetModuleType() == ModuleType.bloc) // si module suivant est un bloc
				{
					if(next.GetSize() == _currentModule.GetSize())
					{
						_canAddNxtBig = _canAddNxtSme = _canAddNxtSml = true;
					}
					else if(next.GetSize() > _currentModule.GetSize())
					{
						_canAddNxtBig = _canAddNxtSme = true;
						_canAddNxtSml = false;
					}
					else if(next.GetSize() < _currentModule.GetSize())
					{
						_canAddNxtSml = _canAddNxtSme = true;
						_canAddNxtBig = false;
					}
				}
				else //Si module suivant facade ou extrem
				{
					//_canAddNxtSme = true;
					_canAddNxtSme = _canAddNxtBig = _canAddNxtSml = false;
				}
			}
		}
		else // si current est une facade ou extrem
		{
			if(next == null && prev != null)
			{
				_canAddPrvSme = true;
				_canAddNxtSme = false;
			}
			else if(prev == null && next != null)
			{
				_canAddNxtSme = true;
				_canAddPrvSme = false;
			}
			_canAddNxtBig = _canAddNxtF =_canAddNxtE = _canAddNxtSml = false;
			_canAddPrvBig = _canAddPrvF = _canAddPrvE = _canAddPrvSml = false;
		}
		
		//Check Sizes
		if(!_dsMdl.HasBlocsOfSize(_currentModule.GetSize()+1))
		{
			_canAddNxtBig = false;
			_canAddPrvBig = false;
		}
		
		if(!_dsMdl.HasBlocsOfSize(_currentModule.GetSize()-1))
		{
			_canAddNxtSml = false;
			_canAddPrvSml = false;
		}
		
		if(!_dsMdl.HasFacadeOfSize(_currentModule.GetSize()))
		{
			_canAddNxtF = false;
			_canAddPrvF = false;
		}
		
		if(!_dsMdl.HasExtremOfSize(_currentModule.GetSize()))
		{
			_canAddNxtE = false;
			_canAddPrvE = false;
		}
		
		//nombre d'icones
		_nextAdds = (_canAddNxtBig?1:0) + (_canAddNxtSme?2:0) + (_canAddNxtSml?1:0) + (_canAddNxtF?1:0) + (_canAddNxtE?1:0);
		_prevAdds = (_canAddPrvBig?1:0) + (_canAddPrvSme?2:0) + (_canAddPrvSml?1:0) + (_canAddPrvF?1:0) + (_canAddPrvE?1:0);
		
	}
	
	public void UpdateVisibility(bool reset =false) // false || rien > Cache ceux qui sont pas sélectionné, true > affiche tout;
	{
		if(szbrand == "PoolCover")
		{
			if(IsAbrifixe())
				return;
			for(int i=0;i<_modules.Count;i++)
			{
				if(i!= _selectedIndex && !reset)
					((DynShelterModule)_modules[i]).SetTransparent(true);
				else
					((DynShelterModule)_modules[i]).SetTransparent(false);
			}
		}
	}
	
	
//--------------COROUTINES-----------------------------------
	
	/* LoadConf()
	 * Chargement de la configuration d'abri par défaut
	 **/
	private IEnumerator LoadConf()
	{
		_isBuilding = true;
		Montage.assetBundleLock = true;
		//Attend que le objdata soit configuré
		while(gameObject.GetComponent<ObjData>()==null)
		{
				yield return new WaitForEndOfFrame();	
		}
			while(gameObject.GetComponent<ObjData>().GetObjectModel() == null)
			{
				yield return new WaitForEndOfFrame();	
			}
		_lib = GetComponent<ObjData>().GetObjectModel().GetLibrary();
		//Récupération de l'assetbundle
//		OSLib libObj = GetComponent<ObjData>().GetObjectModel().GetLibrary();
		WWW www = WWW.LoadFromCacheOrDownload (_lib.GetAssetBundlePath (), _lib.GetVersion ());
		yield return www;
		
		if (www.error != null)
		{
			Debug.Log ("AssetBundle ERROR" + www.error);
		}
		else
		{
			_assetBundle = www.assetBundle;
			if (_assetBundle != null)
			{
				//Chargement de la config
				TextAsset confXml = _assetBundle.LoadAsset (modelName+"_conf", typeof (TextAsset)) as TextAsset;
				if (confXml != null)
				{
					_dsMdl.ParseConfFile(confXml);
				}
				
			}
			_assetBundle.Unload (false);
		}
		www.Dispose();
		
		//Création de l'abri par default
		ArrayList stConf= _dsMdl.GetdefaultConf();
		_currentColor = _dsMdl.GetColor(_currentColorIndex);

		if(stConf.Count > 0)
		{
//			SaveTransform();
			_hasFacadeInDefaultBegin = false;
			_hasFacadeInDefaultEnd = false;
			for(int i=0;i<stConf.Count;i=i+3)
			{
				ModuleType typ = ModuleType.bloc;
				switch ((string)stConf[i])
				{
				case "facade":
					typ = ModuleType.facade;
					break;
				case "bloc":
					typ = ModuleType.bloc;
					if(IsAbrifixe())
					{
						typ = ModuleType.multiBloc;
					}
					break;
				case "extremite":
					typ = ModuleType.extremite;
					break;
				case "multibloc":
					typ = ModuleType.multiBloc;
					break;
				}
				
				if((typ == ModuleType.facade) && (i==0))
				{
					_hasFacadeInDefaultBegin = true;
					_saveTailleBegin = (int) stConf[i+1];
					_saveStyleBegin = (string) stConf[i+2];
					continue;
				}
				if((typ == ModuleType.facade) && (i>0))
				{
					_hasFacadeInDefaultEnd = true;
					_saveTailleEnd = (int) stConf[i+1];
					_saveStyleEnd = (string) stConf[i+2];
					continue;
				}
				
				int t = (int) stConf[i+1];
				
				string styl = (string) stConf[i+2];
				
				_nextInsertion = true;
				_decalZ = 0.0f;
				yield return StartCoroutine(AddModule(t,styl,typ,/*_dsMdl.GetColor(0)*/_currentColor,true));
				
			}
			
			_isBuilding = false;
		}
		if(!_hasFacadeInDefaultEnd)
		{
			_selectedIndex = 0;
			if(IsAbrifixe())
				_selectedIndex = Mathf.Clamp(_selectedIndex,1,_modules.Count-2);
		}
		UpdateCurrentModule();
		UpdateModules();
		UpdateBoxCollider();
		ChangeModuleColor(_currentColor);
		
	//	www.Dispose();
		Montage.assetBundleLock = false;
		enabled = false;	
		
		if(_hasFacadeInDefaultEnd)
		{
			yield return StartCoroutine(AddModule(_saveTailleEnd,_saveStyleEnd,ModuleType.facade,_currentColor,true));
			_hasFacadeInDefaultEnd = false;
			_selectedIndex = 0;
			if(IsAbrifixe())
				_selectedIndex = Mathf.Clamp(_selectedIndex,1,_modules.Count-2);
			
		}
		
		if(_hasFacadeInDefaultBegin)
		{
			_selectedIndex = 0;
			UpdateCurrentModule();
			_nextInsertion = false;
			yield return StartCoroutine(AddModule(_saveTailleBegin,_saveStyleBegin,ModuleType.facade,_currentColor,true));
			_hasFacadeInDefaultBegin = false;			
			if(IsAbrifixe())
				_selectedIndex = Mathf.Clamp(_selectedIndex,1,_modules.Count-2);
			
			UpdateCurrentModule();
			UpdateModules();
			UpdateBoxCollider();
		}
		
		//CenterDeployAndFeetLimit();
		
		yield return null;
	}
	
	/* LoadShelter()
	 * Chargement de la configuration d'abri lors du chargement de montage
	 **/
	private IEnumerator LoadShelter(ArrayList CustomConf = null,ArrayList PosConf = null)
	{
		while(_isBuilding)
			yield return null;

		foreach(DynShelterModule m in _modules)
		{
			m.RemoveModule();	
		}
		_modules.Clear();
		_modManager.ClearManager();
		_currentModule = null;
		_selectedIndex = 0;
		
		yield return null;
		
		_isBuilding = true;
		//Attend que le objdata soit configuré
		while(gameObject.GetComponent<ObjData>().GetObjectModel() == null)
		{
			yield return new WaitForEndOfFrame();	
		}
		
		//Création de l'abri par default
		ArrayList stConf = CustomConf;
		
		
		if(stConf.Count > 0)
		{
			Color32 c = new Color32(255,255,255,255);
//			SaveTransform();
			for(int i=0;i<stConf.Count-2;i=i+8)
			{
				ModuleType typ = ModuleType.bloc;
				switch ((string)stConf[i]) //TYP >0
				{
				case "facade":
					typ = ModuleType.facade;
					break;
				case "bloc":
					typ = ModuleType.bloc;
					break;
				case "extremite":
					typ = ModuleType.extremite;
					break;
				}
				
				int t = (int) stConf[i+1]; //TAILLE >1
				
				
				string styl = (string) stConf[i+2]; //STYLE >2
				
				_nextInsertion = true;
				_decalZ = 0.0f;
				yield return StartCoroutine(AddModule(t,styl,typ,_currentColor));
				
				c = (Color32)stConf[i+3]; //color > 3
				_currentModule.SetColor(c);
				
				_currentModule.SetNextLocks((bool)stConf[i+4]);//NLock> 4
				_currentModule.SetPrevLocks((bool)stConf[i+5]);//PLock> 5
				_currentModule.SetAnchor((bool)stConf[i+6]); //Anchor > 6
				
				yield return new WaitForEndOfFrame();
				
				_modManager.UpdateLocalMods(_currentModule.GetGameObj(),true);
				_modManager.ApplyLoadConf((string[])stConf[i+7]); //ModConf>7
			}
			ChangeModuleColor(c);
			_isBuilding = false;
		}
		
		
		_selectedIndex = 0;
		UpdateCurrentModule();

		for(int i=0;i<_modules.Count;i++)
		{
			((DynShelterModule)_modules[i]).SetPos(false,(float)PosConf[i]);
		}
		
		UpdateBoxCollider();
				
		enabled = false;
		ShowArrows(false);
		CenterDeployAndFeetLimit();
		yield return null;
	}
	
	/* AddModule(int size,string style,ModuleType typ,Color32 color,bool fromInit=false)
	 * Ajoute un module de taille, style, type et couleur rentre en params
	 **/
	public void AddDirectModule()
	{
		StartCoroutine(AddModule(_currentModule.GetSize(),_currentModule.GetStyle(),ModuleType.multiBloc,_currentColor));
	}
	public IEnumerator AddModule(int size,string style,ModuleType typ,Color32 color,bool fromInit=false, bool bsensfacade=false)
	{
		
		//Debug.Log("t : "+size+", styl : "+style+",ModuleType : "+ typ.ToString()+", bool "+fromInit);
		
		bool noNeedLimitToBeRecalculate = fromInit;
		_isBuilding = true;
		//-------vv-Récupération du mesh-vv------------
		DynShelterModule newModule = null;
		string newStyle= string.Copy(style);
		string tag = CheckAndCreateTag(size,style,typ,out newStyle);
		style = string.Copy(newStyle);
		
		GameObject go = null;
		bool bextrem = false;
		
		//--vv--Si il na pas deja été créé, ben tant pis on ouvre--vv--
		if(go == null)
		{
//			OSLib libObj = GetComponent<ObjData>().GetObjectModel().GetLibrary();
			WWW www = WWW.LoadFromCacheOrDownload (_lib.GetAssetBundlePath (), _lib.GetVersion ());
			yield return www;
			if (www.error != null)
			{
				Debug.Log ("AssetBundle ERROR" + www.error);
			}
			else
			{
				_assetBundle = www.assetBundle;
				if (_assetBundle != null)
				{			
					string sz = _assetBundle.name;

					if(sz.Contains("extrem"))
					{
						bextrem = true;
					}

				//	Debug.Log("assetBundle.load  : "+tag);
					Object original = _assetBundle.LoadAsset (tag, typeof(GameObject));
					if(original!=null){
						go = (GameObject) Instantiate (original);				
					}
					/*else
					{
						
						_assetBundle.Unload (false);
						www.Dispose();	
					}*/
						

				}
				_assetBundle.Unload (false);
			}
			www.Dispose();
		}
		
		//-------vv-Création du module-vv---------------
		switch(typ)
		{
		case ModuleType.bloc:
			newModule = new DynShelterBloc(size,style,typ,color,this);
			break;
		case ModuleType.facade:
			noNeedLimitToBeRecalculate=true;
			newModule = new DynShelterFacade(size,style,typ,color,this);
			m_icounterFacade++;
			break;
		case ModuleType.extremite:
			newModule = new DynShelterExtrem(size,style,typ,color,this);
			break;
		case ModuleType.multiBloc:
			newModule = new DynShelterMultiBloc(size,style,ModuleType.bloc,color,this);
			if(_currentModule==null)
			{
				break;
			}
			if(_currentModule.GetType() == typeof(DynShelterBloc))
			{
				DynShelterMultiBloc newCurrent = new DynShelterMultiBloc((DynShelterBloc)_currentModule);
				foreach(DynShelterModule dm in _modules)
				{
					if(dm.GetPrevModule() == _currentModule)
						dm.SetPrevModule(newCurrent);
					if(dm.GetNextModule() == _currentModule)
						dm.SetNextModule(newCurrent);
				}		
				
				_currentModule = newCurrent;
				_modules[_selectedIndex] = _currentModule;
			}
			else
			{
				if(_currentModule.GetPrevModule() != null)
				{
					if(_currentModule.GetPrevModule().GetSize() == size && _currentModule.GetPrevModule().GetType() == typeof(DynShelterBloc))
					{
						Debug.Log("Modifying Prev");
						DynShelterMultiBloc newPrev = new DynShelterMultiBloc((DynShelterBloc)_currentModule.GetPrevModule());
						int idx = _modules.IndexOf(_currentModule.GetPrevModule());
						_modules[idx] = newPrev;
						_currentModule.SetNextModule(newPrev);
					}
				}
				
				if(_currentModule.GetNextModule() != null)
				{
					Debug.Log("Modifying Next");
					if(_currentModule.GetNextModule().GetSize() == size && _currentModule.GetNextModule().GetType() == typeof(DynShelterBloc))
					{
						DynShelterMultiBloc newNext = new DynShelterMultiBloc((DynShelterBloc)_currentModule.GetNextModule());
						int idx = _modules.IndexOf(_currentModule.GetNextModule());
						_modules[idx] = newNext;
						_currentModule.SetNextModule(newNext);
					}
				}
			}
			break;	
		}
		
		SaveTransform();
		go.transform.parent = transform;
		go.transform.localPosition = Vector3.zero;

		if(_currentModule != null)
		{
			if(bsensfacade && bextrem)
			{
				_currentModule.bextrem = true;
				go.transform.Rotate(0.0f, 180.0f, 0.0f);
			}
			else
			{
				_currentModule.bextrem = false;
			}
		}

		newModule.SetGameObject(go);
		_modManager.TestModuleForGlobalMods(go);

		//-------vv-Placement-vv-----------------------
		if(_currentModule != null)
		{
//			Debug.Log(" _currentModule not null");
			if(_nextInsertion)
			{
//				Debug.Log(" _nextInsertion");
				newModule.InsertBetween(_currentModule,_currentModule.GetNextModule());
				print ("_selectedIndex :"+ _selectedIndex );
				if( _selectedIndex+1 <= _modules.Count)
				{
					_modules.Insert(_selectedIndex+1,newModule);
					_selectedIndex++;
				}
			}
			else
			{
//				Debug.Log("Not _nextInsertion");
			/*	if(_currentModule.GetType() == typeof(DynShelterMultiBloc)
					&& newModule.GetType() == typeof(DynShelterMultiBloc)
					&& _currentModule.GetPrevModule().GetType() == typeof(DynShelterFacade))
				{
					newModule.InsertBetween(_currentModule,_currentModule.GetNextModule());
					_modules.Insert(_selectedIndex+1,newModule);
				}
				else*/
				{
					newModule.InsertBetween(_currentModule.GetPrevModule(),_currentModule);
					_modules.Insert(_selectedIndex,newModule);
				}
			}
			
			//Déplacement Immobile ^^ QUICKFIX placement des facades a l'ajout de celles-ci
			_currentModule.MoveModuleV2(0,_currentModule,true,true);
		}
		else
		{
//			Debug.Log(" _currentModule  null");
			newModule.SetPos(false,0);
			newModule.SetNextPrevModule(null,null);
			newModule.SetNextPrevLocks(true,true);
			_modules.Add(newModule);
			//_selectedIndex = 0;
		}
		UI.SetItemsNb(_modules.Count);
		UpdateCurrentModule();
		_removeModule = false;
		ChangeModuleColor(_currentColor);
		
		UpdateLimits(noNeedLimitToBeRecalculate);
		
		ReapplyTransform();
		
		if(szbrand == "PoolCover")
		{
			UpdateBoxCollider();
			UpdateVisibility();
		}
				
		//CenterDeployAndFeetLimit();
		
		yield return new WaitForEndOfFrame();
		_isBuilding = false;
	}
	
	/* ChangeModuleStyle(int idx)
	 * Changement de style d'un module par le idx ième
	 * */
	public IEnumerator ChangeModuleStyle(int idx, bool bsensfacade=false)
	{
		string newStyle = _tmpStylesNames[idx];
		string newTag = _currentModule.GetModuleType().ToString()+"_t"+_currentModule.GetSize()+"_"+newStyle;
		if(newStyle != _currentModule.GetStyle())
		{
			//Récupére le GameObject du nouveau style
			GameObject go = null;
			bool bextrem = false;
			//--vv--Copie un module éxistant pour éviter de réouvrir l'assetBundle--vv--
//			foreach(DynShelterModule dsMod in _modules)
//			{
//				if(dsMod.GetTag() == newTag)
//				{
//					go = (GameObject)Instantiate(dsMod.GetGameObj());
//					break;
//				}
//			}
			
			//--vv--Si il na pas deja été créé, ben tant pis on ouvre--vv--
//			if(go == null)
//			{
//				OSLib libObj = GetComponent<ObjData>().GetObjectModel().GetLibrary();
				WWW www = WWW.LoadFromCacheOrDownload (_lib.GetAssetBundlePath (), _lib.GetVersion ());
				yield return www;
				if (www.error != null)
				{
					Debug.Log ("AssetBundle ERROR" + www.error);
				}
				else
				{
					_assetBundle = www.assetBundle;
					if (_assetBundle != null)
				    {

					string sz = newTag;
						if(sz.Contains("extrem"))
						{
							bextrem = true;
						}

						Debug.Log("assetBundle.load  : "+newTag);
						Object original = _assetBundle.LoadAsset (newTag, typeof(GameObject));
			
						go = (GameObject) Instantiate (original);					
					}
					_assetBundle.Unload (false);
				}
				www.Dispose();
//			}
			SaveTransform();
			go.transform.parent = transform;
			go.transform.localRotation = _currentModule.GetGameObj().transform.localRotation;//Quaternion.identity;
			go.transform.localScale = Vector3.one;

			if(bextrem)
			{
				if(bsensfacade)
				{
					go.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
					//go.transform.Rotate(0.0f, 180.0f, 0.0f);
				}

				_currentModule.bextrem = true;
			}
			else
			{
				_currentModule.bextrem = false;
			}

			if(!IsAbrifixe())
			{
				go.transform.localPosition = _currentModule.GetPos();			
				_modManager.TestModuleForGlobalMods(go);
				_modManager.RemoveModFrom(_currentModule.GetGameObj());			
				Destroy(_currentModule.GetGameObj());
				_currentModule.ChangeStyle(newStyle,go);			
				_modules[_selectedIndex] = _currentModule;			
				//ChangeModuleColor(_currentColor);
			}
			else
			{
				int curMod=0;
				//foreach(DynShelterModule m in _modules)
				for(int modulIndex=0;modulIndex<_modules.Count;modulIndex++)
				{
					DynShelterModule m = (DynShelterModule) _modules[modulIndex];
					if(m.GetModuleType()==_currentModule.GetModuleType())
					{
						GameObject goCopy = (GameObject)Instantiate(go);
						goCopy.transform.parent = transform;
						goCopy.transform.localPosition = m.GetPos();				
						_modManager.TestModuleForGlobalMods(goCopy);			
						_modManager.RemoveModFrom(m.GetGameObj());			
						Destroy(m.GetGameObj());
						m.ChangeStyle(newStyle,goCopy);			
						_modules[modulIndex] = m;	
					}
					curMod++;
				}			
				Destroy(go);	
			}
					
			ChangeModuleColor(_currentColor);			
			ReapplyTransform();
		}
		yield return null;
	}
	
//--------------3rd PARTY FCN'S---------------------------
//--------------PUBLICS-----------------------------------
	
	/* ChangeModuleColor(Color32 c)
	 * Change la couleur de TOUT les modules par celle passée en parametres
	 * */
	private void ChangeModuleColor(Color32 c)
	{
//		Debug.Log("APPLYING COLOR "+c+"TO CURRENT MODULE");
//		Transform[] objs = _currentModule.GetGameObj().GetComponentsInChildren<Transform>(true);
//		bool[] states = new bool[objs.Length];
//		for(int i=0;i<objs.Length;i++)
//		{
//			Debug.Log("NAME>>"+objs[i].gameObject.name);
//			states[i] = objs[i].gameObject.activeSelf;
//			objs[i].gameObject.SetActive(true);
//		}
		_currentColor = c;
		
		foreach(DynShelterModule m in _modules)
			m.SetColor(c);
		
		foreach(MeshRenderer mr in /*_currentModule.GetGameObj().*/GetComponentsInChildren<MeshRenderer>(true))
		{
			if(mr.transform.name != "MidMesh")
			{
				foreach(Material m in mr.materials)
				{
					if(m.name.Contains(_matColorTag) || m.name.Contains(_matColorTag2))
					{
						m.color = c;
					}
				}
			}
		}
		
		foreach(SkinnedMeshRenderer mr in /*_currentModule.GetGameObj().*/GetComponentsInChildren<SkinnedMeshRenderer>(true))
		{
			if(mr.transform.name != "MidMesh")
			{
				foreach(Material m in mr.materials)
				{
					if(m.name.Contains(_matColorTag))
					{
						m.color = c;
					}
				}
			}
		}
//		for(int i=0;i<objs.Length;i++)
//		{
//			objs[i].gameObject.SetActive(states[i]);
//		}
	}
	
	/* CheckAndCreateTag(int size,string style,ModuleType typ)
	 * Créé un tag pour le module en fonction de sa taille, son style, et son type
	 * vérifie aussi si pour une taille X, le style S existe, renvoie le 1er style sinon
	 * */
	private string CheckAndCreateTag(int size,string style,ModuleType typ, out string outStyle)
	{
		string tag = "";
		string outType = typ.ToString();
		/*string*/ outStyle = style;
		
		if(typ == ModuleType.multiBloc)
			outType = "bloc";
		
		if(_currentModule != null)
		{
			if(
				(_currentModule.GetModuleType() == ModuleType.facade 
				|| _currentModule.GetModuleType() == ModuleType.extremite
				//|| _currentModule.GetModuleType() == ModuleType.multiBloc
				//|| _currentModule.GetModuleType() == ModuleType.bloc
				)
				&&
				(typ == ModuleType.bloc || typ == ModuleType.multiBloc))
			{
				outStyle = _dsMdl.GetFirstStyle(ModuleType.bloc,size);	
			}
		}
		
		tag = outType + "_t" + size + "_" + outStyle;
		
		return tag;
	}
	
	/* InstantiateRessources()
	 * Créé (instancie les GameObjects) des fleches et des limites (barres avec demi triangle)
	 * */
	private void InstantiateRessources()
	{
		if(_blank == null)
			_blank = (Texture2D)Resources.Load("gui/blank");
		_fwdLimit = new Vector3(0,0,5);
		_bwdLimit = new Vector3(0,0,-5);
		
		
		/*if(szbrand == "PoolCover")
		{*/
			if(_arrowNxt == null)
			{
				_arrowNxt = (GameObject)Instantiate(arrowRef);
				_arrowNxt.transform.parent = transform;
				_arrowNxt.transform.localRotation = Quaternion.Euler(new Vector3(0,90f,0));
				_arrowNxt.GetComponent<Renderer>().material.color = new Color(244f/255f,118f/255f,86f/255f);
				_arrowNxt.GetComponent<Renderer>().enabled = false;
			}
			
			if(_arrowPrv == null)
			{
				_arrowPrv = (GameObject)Instantiate(arrowRef);
				_arrowPrv.transform.parent = transform;
				_arrowPrv.transform.localRotation = Quaternion.Euler(new Vector3(0,-90f,0));
				_arrowPrv.GetComponent<Renderer>().material.color = new Color(61f/255f,198f/255f,188f/255f);
				_arrowPrv.GetComponent<Renderer>().enabled = false;
			}
		//}
		
		if(_limitFwd == null)
		{
			_limitFwd = (GameObject)Instantiate(limitObject);
			_limitFwd.transform.parent = transform;
			_limitFwd.transform.localRotation = Quaternion.Euler(new Vector3(0,180,0));
			foreach(Renderer r in _limitFwd.GetComponentsInChildren<Renderer>())
			{
				r.material.shader = Shader.Find("Transparent/Diffuse");
				r.material.color = new Color(61f/255f,198f/255f,188f/255f,0.75f);
			}
			_limitFwd.SetActive(false);
		}
		
		if(_limitBwd == null)
		{
			_limitBwd = (GameObject)Instantiate(limitObject);
			_limitBwd.transform.parent = transform;
			_limitBwd.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
			foreach(Renderer r in _limitBwd.GetComponentsInChildren<Renderer>())
			{
				//r.material.color = Color.red;
				r.material.shader = Shader.Find("Transparent/Diffuse");
				r.material.color = new Color(244f/255f,118f/255f,86f/255f,0.75f);
			}
			_limitBwd.SetActive(false);
		}
	}
	
	/* UIRelocate()
	 * Relocalise la GUI si changement de résolution
	 * */
	private void UIRelocate()
	{
		_upperArea = new Rect(Screen.width/2 - 300,30,600,120);
		_leftArea = new Rect(0.0f, 0.0f, Screen.width * 2.0f, Screen.height);
		UI.Relocate();
	}
	
	public void QuitOverride(bool b = true)
	{
		if(b)
		{
			Validate();	
		}
	}
	
//--------------PUBLICS-----------------------------------
	
	/* SaveTransform()
	 * Sauvegarde le transform du GameObject et le réinitialise
	 * -> utilisé pour la construction de l'abri
	 * */
	public void SaveTransform()
	{
		_savedRotation = transform.localRotation;
		_savedScale = transform.localScale;
		
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
	
	/* ReapplyTransform()
	 * Réapplique le transform sauvegardé
	 * */
	public void ReapplyTransform()
	{
		transform.localRotation = _savedRotation;
		transform.localScale = _savedScale;	
	}
	
	/* RemoveCurrent()
	 * Supprime le module courant
	 * */
	public void RemoveCurrent()
	{
		bool NoNeedToBeRecalculate = false;
		if(_currentModule.GetModuleType() != ModuleType.facade)
		{
			if(_selectedIndex>_modules.Count/2.0f)
				_removeNext=true;
			else
			{
				_removeNext=false;
			}
			_removeModule = true;
		}
		else
		{
			NoNeedToBeRecalculate = true;
		}
			
		_currentModule.RemoveModule();		
		_modManager.RemoveModFrom(_currentModule.GetGameObj());
		
		_modules.RemoveAt(_selectedIndex);
		UI.SetItemsNb(_modules.Count);
		
		_selectedIndex --;
		if(IsAbrifixe())
			_selectedIndex = Mathf.Clamp(_selectedIndex,1,_modules.Count-2);
		else
			_selectedIndex = Mathf.Clamp(_selectedIndex,0,_modules.Count-1);
		Debug.Log("Remove module, _selectedIndex "+_selectedIndex);
		
		UpdateCurrentModule();
		UpdateLimits(NoNeedToBeRecalculate);
			
		
		if(szbrand == "PoolCover")
		{
			UpdateVisibility();
			UpdateBoxCollider();
		}
		
		CenterDeployAndFeetLimit();
	}
	
	public void RemoveModule(int _iid)
	{
		if(GetModule(_iid).GetModuleType() == ModuleType.facade)
		{
			m_icounterFacade--;
		}
		
		GetModule(_iid).RemoveModule();		
		_modManager.RemoveModFrom(GetModule(_iid).GetGameObj());
		
		_selectedIndex = _iid;
		_modules.RemoveAt(_selectedIndex);
		UI.SetItemsNb(_modules.Count);
		
		_selectedIndex --;
		if(IsAbrifixe())
			_selectedIndex = Mathf.Clamp(_selectedIndex,1,_modules.Count-2);
		else
			_selectedIndex = Mathf.Clamp(_selectedIndex,0,_modules.Count-1);
		Debug.Log("Remove module, _selectedIndex "+_selectedIndex);
		
		UpdateCurrentModule();
		
		if(szbrand == "PoolCover")
		{
			UpdateVisibility();
		}
		
		CenterDeployAndFeetLimit();
	}
	
	/* ShowArrows(bool val)
	 * Affiche/Cache les fleches de déplacement
	 * */
	public void ShowArrows(bool val)
	{
		if(_arrowNxt && _arrowPrv)
		{
			if(IsAbrifixe())
				val=false;
			_arrowNxt.GetComponent<Renderer>().enabled = val;
			_arrowPrv.GetComponent<Renderer>().enabled = val;
		}
	}
	
	public void DestroyArrows()
	{
		/*if(_arrowNxt && _arrowPrv)
		{*/
			Destroy(_arrowNxt);
			Destroy(_arrowPrv);
		//}
	}
	//--vv--Get / Set--vv--
	
	/* LoadImg(string name)
	 * Fcn de chargement d'image depuis l'asset bundle
	 * */
	public Texture2D LoadImg(string name)
	{
		return (Texture2D)_assetBundle.LoadAsset(name,typeof(Texture2D));	
	}
	
	/* GetModules()
	 * retourne la liste des modules
	 * */
	public ArrayList GetModules()
	{
		return _modules;
	}
	
	/* SetModManager(DynShelterModManager modMgr)
	 * Affecte le ModManager(qui gere les lumieres,afficher/cacher,...) au configurateur
	 * */
	public void SetModManager(DynShelterModManager modMgr)
	{
		_modManager = modMgr;	
	}
	
	/* GetModManager()
	 * retourne le ModManager du configurateur
	 * */
	public DynShelterModManager GetModManager()
	{
		return _modManager;	
	}
	
	/* GetUISelector()
	 * Retourne l'etat de l'interface (styles,ajout,limites,...)
	 * */
	public UISelector GetUISelector()
	{
		return _currentUI;	
	}
	
	/* SetUISelector(UISelector ui)
	 * Change l'etat de l'interface
	 * */
	public void SetUISelector(UISelector ui)
	{
		_currentUI = ui;
		
		
		if(szbrand == "PoolCover")
		{
			//ARROWS NEXT PREV MODULE
			if(_currentUI == UISelector.addNext || _currentUI == UISelector.addPrev)
			{
				
				if(_currentUI == UISelector.addNext)
				{
					_arrowNxt.transform.localScale = new Vector3(1.2f,1.2f,1.2f);
					_arrowPrv.transform.localScale = new Vector3(1,1,1);
				}
				if(_currentUI == UISelector.addPrev)
				{
					_arrowNxt.transform.localScale = new Vector3(1,1,1);
					_arrowPrv.transform.localScale = new Vector3(1.2f,1.2f,1.2f);
				}
			}
			else
			{
				if((_arrowPrv!=null)&&(_arrowNxt!=null))
				{
					_arrowPrv.transform.localScale = new Vector3(1,1,1);
					_arrowNxt.transform.localScale = new Vector3(1,1,1);
				}
			}
			
			//LIMITS
			if(_currentUI == UISelector.limits)
			{
				_limitBwd.SetActive(true);
				_limitFwd.SetActive(true);
			}
			else
			{
				_limitBwd.SetActive(false);
				_limitFwd.SetActive(false);
			}
		}
	}
	
	/* setNextInsertion(bool b)
	 * Change le mode d'ajout
	 * si a true  > le prochain module ajouté l'est APRES le module courant
	 * si a false > le prochain module ajouté l'est AVANT le module courant
	 * */
	public void setNextInsertion(bool b)
	{
		_nextInsertion = b;
	}
	
	public DynShelterModule GetCurrentModule(){return _currentModule;}
	
	
#endregion
	
#region Interface Fcns

	public string GetFunctionName(){return "DynShelter";}
	
	public string GetFunctionParameterName(){return TextManager.GetText("DynShelter.Title");}
	
	public int GetFunctionId(){return os3dFuncId;}
	
	public void DoAction()
	{
		Camera.main.GetComponent<GuiTextureClip>().SetOverride(false);
		
		menuConf.setVisibility(false);
			
		menuInteract.setVisibility(false);
		menuInteract.isConfiguring = false;
			
		interact.setSelected(null,true);
		interact.setActived(false);	
		
		enabled = true;
		UsefullEvents.ShowHelpPanel += Validate;
	}
	
	void Validate()
	{	
				
		Camera.main.GetComponent<GuiTextureClip>().SetOverride(true);
		
		SetUISelector(UISelector.none);
		UI.ResetUI();
		UpdateBoxCollider();
		
		interact.configuringObj(null);
		
		menuInteract.unConfigure();
		menuInteract.setVisibility (false);
		
		interact.setSelected(null,false);
		UsefullEvents.ShowHelpPanel -= Validate;	 
		enabled = false;	
	}
	
	//  sauvegarde/chargement
	public void save(BinaryWriter buf)
	{
		buf.Write(_modules.Count); // 0 > nombres de modules
		foreach(DynShelterModule mod in _modules)
		{
			buf.Write(mod.GetModuleType().ToString()); 	// x0 > type
			buf.Write(mod.GetSize());					//x1 > taille
			buf.Write(mod.GetStyle());					//x2 > style
			buf.Write(mod.GetPos().z);					//x3 > pos(Z)
			
			buf.Write(mod.GetColor().r);				//x4 > color.r
			buf.Write(mod.GetColor().g);				//x5 > color.g
			buf.Write(mod.GetColor().b);				//x6 > color.b
			
			buf.Write(mod.GetNextLock());				//x7 > nextLock
			buf.Write(mod.GetPrevLock());			//x8 > prevLock
			buf.Write(mod.IsAnchored());			//x9 >Anchor
			
			_modManager.UpdateLocalMods(mod.GetGameObj(),true);
			_modManager.SaveMods(buf);//x10
		}
		
		buf.Write(_fwdLimit.z);//fwd
		buf.Write(_bwdLimit.z);//bwd
	} 
	
	public void load(BinaryReader buf)
	{
		foreach(DynShelterModule m in _modules)
		{
			Destroy(m.GetGameObj());
		}
		_modules.Clear();
		
		int nb = buf.ReadInt32();
		
		ArrayList conf = new ArrayList();
		ArrayList pos = new ArrayList();
		
		Color32 c;
		
		for(int i=0;i<nb;i++)
		{
			conf.Add(buf.ReadString());// x0 > type
			conf.Add(buf.ReadInt32());//x1 > taille
			conf.Add(buf.ReadString());//x2 > style
			
			pos.Add(buf.ReadSingle());//x3 > pos(Z)
			
			c = new Color32(buf.ReadByte(),buf.ReadByte(),buf.ReadByte(),255);
			conf.Add(c);//x4,5,6 > color
			
			conf.Add(buf.ReadBoolean());//x7 > nextLock
			conf.Add(buf.ReadBoolean());//x8 > prevLock			
			conf.Add(buf.ReadBoolean());//x9 Anchor
			conf.Add(_modManager.LoadMods(buf));//x10 string[]
			//--> 9 item/objects dans la conf au lieu de 11
		}
		
		conf.Add(new Vector3(0,0, buf.ReadSingle()));//fwd
		conf.Add(new Vector3(0,0, buf.ReadSingle()));//bwd
		
		StartCoroutine(LoadShelter(conf,pos));
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
		//-------------
		al.Add(_modules.Count); // 0 > nombres de modules
		foreach(DynShelterModule mod in _modules)
		{
			al.Add(mod.GetModuleType().ToString()); 	// x0 > type
			al.Add(mod.GetSize());					//x1 > taille
			al.Add(mod.GetStyle());					//x2 > style
			al.Add(mod.GetPos().z);					//x3 > pos(Z)
			
			al.Add(mod.GetColor().r);				//x4 > color.r
			al.Add(mod.GetColor().g);				//x5 > color.g
			al.Add(mod.GetColor().b);				//x6 > color.b
			
			al.Add(mod.GetNextLock());				//x7 > nextLock
			al.Add(mod.GetPrevLock());			//x8 > prevLock
			al.Add(mod.IsAnchored());			//x9 > Anchor
			
//			_modManager.UpdateLocalMods(mod.GetGameObj(),true);
//			_modManager.SaveMods(buf);
		}
		
		al.Add(_fwdLimit.z);//fwd
		al.Add(_bwdLimit.z);//bwd
		
		CenterDeployAndFeetLimit();
		//-------------
		return al;
	}
	
	public void setConfig(ArrayList config)
	{
		foreach(DynShelterModule m in _modules)
		{
			Destroy(m.GetGameObj());
		}
		_modules.Clear();
		
		int nb = (int)config[0];
		
		ArrayList conf = new ArrayList();
		ArrayList pos = new ArrayList();
		
		Color32 c;
		int indexer = 1;
		for(int i=0;i<nb;i++)
		{
			string typ = (string)config[indexer ++];
			conf.Add(typ);
			
			int sze = (int) config[indexer ++];
			conf.Add(sze);
			
			string styl = "";
			ModuleType mdTyp = ModuleType.bloc;
			switch(typ)
			{
			case "bloc":
				mdTyp = ModuleType.bloc;
				break;
			case "facade":
				mdTyp = ModuleType.facade;
				break;
			case "extremite":
				mdTyp = ModuleType.extremite;
				break;
			case "multiBloc":
				mdTyp = ModuleType.multiBloc;
				break;
					
			}
			styl = _dsMdl.GetFirstStyle(mdTyp,sze);
			indexer ++;
			
			conf.Add(styl);
			pos.Add(config[indexer ++]);
			
			c = new Color32((byte)config[indexer ++],(byte)config[indexer ++],(byte)config[indexer ++],255);
			conf.Add(c);
			
			conf.Add(config[indexer ++]);//x7 > nextLock
			conf.Add(config[indexer ++]);//x8 > prevLock		
			conf.Add(config[indexer ++]);//x9 >Anchor
			conf.Add(new string[0]);//x10 string[] NO CONF
			//--> 7 item/objects dans la conf
		}
		
		conf.Add(new Vector3(0,0, (float)config[indexer ++]));//fwd
		conf.Add(new Vector3(0,0, (float)config[indexer ++]));//bwd
		
		StartCoroutine(LoadShelter(conf,pos));
	}
	
	public Vector3 GetLimitPos(bool forward)
	{
		if(forward)
			return _fwdLimit;
		else
			return _bwdLimit;
	}
	
	public bool IsAbrifixe()
	{
		bool _isFixe = _dsMdl.IsAbriFixe();
		
		if((_arrowNxt != null) && (_isFixe) && szbrand == "PoolCover")
		{
			_arrowNxt.GetComponent<Renderer>().enabled = false;
			_arrowPrv.GetComponent<Renderer>().enabled = false;
		}
		//return false;
		return _isFixe;
	}
#endregion
}
