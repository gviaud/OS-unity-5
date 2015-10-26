using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using Pointcube.Global;
using Pointcube.Utils;

public class DesignerDesjoyaux : MonoBehaviour
{
	public int os3dFuncId;
	
	public GameObject gofiltrationBlock;
	public FunctionConf_PoolDesignerDesjoyaux poolDesigner;
	
	//public GameObject gotemp;
	
	private List<bool> listStairway = new List<bool>();
	
	public float ftimerGenerate = 0.0f;
	public bool blockStairway = false;
	
	private bool _uiSize = false;
	private bool _uiBloc = false;
	private bool _uiStairway = false;
	private bool _showUI = true;
	
	private GUISkin _uiSkin;
	private Rect _uiArea;
	private Vector2 _uiScroll;
	
	void Awake()
	{
		enabled = false;
	}
	
	// Use this for initialization	
	void Start ()
	{
		/*_selectMat = new Material(Shader.Find("Diffuse"));
		_selectMat.color = Color.green;*/
		
		_uiArea = new Rect(Screen.width-280, 0, 300, Screen.height);
		if(_uiSkin == null)
			_uiSkin = (GUISkin)Resources.Load("skins/DesjoyauxSkin");
			
		for(int i = 0; i < 3; i++)
		{
			listStairway.Add(false);
		}
	}
	
	void OnEnable()
	{
	} 
	
	void OnDisable()
	{
	}
	
	void OnDestroy()
	{
	}

	// Update is called once per frame
	void Update ()
	{
		if(ftimerGenerate > 0.0f)
		{
			ftimerGenerate -= Time.deltaTime;
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
	
	void designerDesjoyauxUI()
	{
		GUI.skin = _uiSkin;
		GUI.Box(new Rect(Screen.width-280.0f,0.0f,Screen.width-280.0f,Screen.height),"","BackGround");
		//-------vv----FADE Haut-------------vv------------------
		GUILayout.BeginArea(_uiArea);
		GUILayout.FlexibleSpace();
		_uiScroll = GUILayout.BeginScrollView(_uiScroll,"empty","empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
		GUILayout.Box("","UP",GUILayout.Width(280),GUILayout.Height(150));//fade en haut
		GUILayout.BeginVertical("MID");
		
		if(_showUI)
		{
			if(GUILayout.Button(TextManager.GetText("DesignerDesjoyaux.Return"),"Menu",GUILayout.Height(50),GUILayout.Width(280)))
			{
				GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().isConfiguring = true;
				Camera.main.GetComponent<ObjInteraction>().setActived(false);
				GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().enabled = true;
				GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(true);
				Camera.main.GetComponent<GuiTextureClip>().enabled = true;
				
				GameObject.Find("MainScene").GetComponent<GUIMenuLeft>().canDisplay(false);
				GameObject.Find("MainScene").GetComponent<GUIMenuRight>().canDisplay(false);
				enabled = false;
			}
			
			bool tmpui = GUILayout.Toggle(_uiSize, TextManager.GetText("DesignerDesjoyaux.SizePool"),GUILayout.Height(50),GUILayout.Width(280));
			if(tmpui != _uiSize)
			{
				_uiSize = tmpui;
				_uiStairway = false;
				_uiBloc = false;
			}
			
			if(_uiSize)
			{
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				
				string szscale = (8 + (poolDesigner.fscale * 2.0f)) + "m x " + (4 + poolDesigner.fscale) + "m";
				GUILayout.Box(szscale,"TextField",GUILayout.Height(40),GUILayout.Width(200));
				
				GUILayout.Space(20);	
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				
				if(poolDesigner.fscale == -1)
				{
					GUILayout.Box("","empty",GUILayout.Height(50),GUILayout.Width(50));
				}
				else if(GUILayout.Button("-","btn-",GUILayout.Height(50),GUILayout.Width(50)))
				{
					UIchangeSize(-1);
				}
				GUILayout.Label(TextManager.GetText("DesignerDesjoyaux.Size"), "TextField", GUILayout.Height(40),GUILayout.Width(75));
				
				if(poolDesigner.fscale == 2)
				{
					GUILayout.Box("","empty",GUILayout.Height(50),GUILayout.Width(50));
				}
				else if(GUILayout.Button("+","btn+",GUILayout.Height(50),GUILayout.Width(50)) && poolDesigner.fscale < 2)
				{
					UIchangeSize(1);
				}
				
				GUILayout.Space(30);
				GUILayout.EndHorizontal();
			}
			
			//UI TypeStairway
			bool tmpTyp = GUILayout.Toggle(_uiStairway,TextManager.GetText("DesignerDesjoyaux.Stairway"),GUILayout.Height(50),GUILayout.Width(280));
			if(tmpTyp != _uiStairway)
			{
				_uiStairway = tmpTyp;
				_uiSize = false;
				_uiBloc = false;
			}
			
			if(_uiStairway)
			{
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				
				string sztemp = poolDesigner.bstairway ? TextManager.GetText("DesignerDesjoyaux.Hide") : TextManager.GetText("DesignerDesjoyaux.Display");
				
				if(GUILayout.Button(sztemp,"TextField",GUILayout.Height(40),GUILayout.Width(200)))
				{
					if(!blockStairway)
					{
						blockStairway = true;
						
						if(sztemp == TextManager.GetText("DesignerDesjoyaux.Display") )
						{
							if(poolDesigner.fsizeStairway != 0.0f && poolDesigner.fradiusStairway != 0.0f)
							{
								StartCoroutine(poolDesigner.addStairway(poolDesigner.pointsData, true, 1, poolDesigner.fsizeStairway, poolDesigner.fradiusStairway));
							}
							else
							{
								listStairway[1] = true;
								
								poolDesigner.bstairway = false;
								poolDesigner.broman = true;
								StartCoroutine(poolDesigner.addStairway(poolDesigner.pointsData, true, 0, 176.0f));
							}
							
							if(gofiltrationBlock)
							{
								foreach(Transform child in gofiltrationBlock.transform)
								{
									child.GetComponent<Renderer>().enabled = false;
								}
								
								gofiltrationBlock.GetComponent<Renderer>().enabled = false;
							}
						}
						else
						{
							StartCoroutine(poolDesigner.removeStairway());
							
							if(gofiltrationBlock)
							{
								foreach(Transform child in gofiltrationBlock.transform)
								{
									child.GetComponent<Renderer>().enabled = false;
								}
								
								gofiltrationBlock.GetComponent<Renderer>().enabled = false;
							}
						}
					}
				}
				
				GUILayout.Space(20);	
				GUILayout.EndHorizontal();
				
				if(sztemp == TextManager.GetText("DesignerDesjoyaux.Hide"))
				{
					UIchangeStairway("Rect 2,76m", 276.0f, 0.0f, 0);
					
					UIchangeStairway("Roman 1,76m", 176.0f, 84.0f, 1);
					
					UIchangeStairway("Roman 2,76m", 276.0f, 131.0f, 2);
					
					GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
					GUILayout.FlexibleSpace();
					
					bool bbutton = poolDesigner.balwaysCenter ? GUILayout.Button("<","btn<",GUILayout.Height(50),GUILayout.Width(50)) :
						GUILayout.RepeatButton("<","btn<",GUILayout.Height(50),GUILayout.Width(50));
					if(bbutton && !blockStairway)
					{
						blockStairway = true;
						
						StartCoroutine(UImoveStairway(1));
					}
					
					GUILayout.Label(TextManager.GetText("DesignerDesjoyaux.StairwayPosition"), "TextField", GUILayout.Height(40),GUILayout.Width(75));
					
					bbutton = poolDesigner.balwaysCenter ? GUILayout.Button(">","btn>",GUILayout.Height(50),GUILayout.Width(50)) :
						GUILayout.RepeatButton(">","btn>",GUILayout.Height(50),GUILayout.Width(50));
					if(bbutton && !blockStairway)
					{
						blockStairway = true;
						
						StartCoroutine(UImoveStairway(-1));
					}
					
					GUILayout.Space(30);
					GUILayout.EndHorizontal();
				}
			}
			
			//UI Bloc
			bool tmpDim = GUILayout.Toggle(_uiBloc,TextManager.GetText("DesignerDesjoyaux.Bloc"),GUILayout.Height(50),GUILayout.Width(280));
			if(tmpDim != _uiBloc)
			{
				_uiBloc = tmpDim;
				_uiSize = false;
				_uiStairway = false;
			}
			if(_uiBloc && gofiltrationBlock)
			{
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				
				if(poolDesigner.listDoubleV3.Count > 0)
				{
					string sztemp = gofiltrationBlock.GetComponent<Renderer>().enabled ? TextManager.GetText("DesignerDesjoyaux.Hide") : TextManager.GetText("DesignerDesjoyaux.Display");
					
					if(GUILayout.Button(sztemp,"TextField",GUILayout.Height(40),GUILayout.Width(200)))
					{
						gofiltrationBlock.GetComponent<Renderer>().enabled = !gofiltrationBlock.GetComponent<Renderer>().enabled;
						
						foreach(Transform child in gofiltrationBlock.transform)
						{
							if(child.GetComponent<Renderer>())
							{
								child.GetComponent<Renderer>().enabled = gofiltrationBlock.GetComponent<Renderer>().enabled;
							}
						}
						
						poolDesigner.reinitBloc();
						poolDesigner.moveBloc();
					}
				}
				else
				{
					GUILayout.Box(TextManager.GetText("DesignerDesjoyaux.NoSpace"),"TextField",GUILayout.Height(40),GUILayout.Width(200));
				}
				
				GUILayout.Space(20);	
				GUILayout.EndHorizontal();
				
				if(gofiltrationBlock.GetComponent<Renderer>().enabled)
				{
					UIchangeBloc("GRI");
					UIchangeBloc("PFI");
					
					GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
					GUILayout.FlexibleSpace();
					
					float ftmp = GUILayout.HorizontalSlider(poolDesigner.fgoBlocPosition,0.0f,poolDesigner.fmagnitudeRail,GUILayout.Height(50), GUILayout.Width(200));
					if(ftmp != poolDesigner.fgoBlocPosition)
					{
						poolDesigner.fgoBlocPosition = ftmp;
						poolDesigner.moveBloc();
					}
					
					GUILayout.Space(20);
					GUILayout.EndHorizontal();
				}
			}
		}
		
		
		//-------vv----FADE BAS-------------vv------------------
		GUILayout.EndVertical();
		GUILayout.Box("","DWN",GUILayout.Width(280),GUILayout.Height(150));//fade en bas
		
		GUILayout.EndScrollView();
		GUILayout.FlexibleSpace();
		
		GUILayout.EndArea();
	}
	
	void OnGUI()
	{
		designerDesjoyauxUI();
	}
	
	private void UIchangeSize(int _isens)
	{
		if(ftimerGenerate <= 0.0f)
		{
			ftimerGenerate = 0.5f;
			StartCoroutine(poolDesigner.scalePool(_isens));
			
		}
		
		if(gofiltrationBlock)
		{
			foreach(Transform child in gofiltrationBlock.transform)
			{
				child.GetComponent<Renderer>().enabled = false;
			}
			
			gofiltrationBlock.GetComponent<Renderer>().enabled = false;
		}
	}
	
	private void UIchangeBloc(string _szname)
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		string szmesh = _szname + "181";
		string sztextStyle = gofiltrationBlock.name == szmesh ? "TextArea" : "TextField";
		
		if(GUILayout.Button(_szname,sztextStyle,GUILayout.Height(40),GUILayout.Width(200)))
		{
			gofiltrationBlock.GetComponent<Renderer>().enabled = true;
			
			GameObject gotemp = gofiltrationBlock;
			
			gofiltrationBlock = GameObject.Instantiate(Resources.Load(szmesh)) as GameObject;
			gofiltrationBlock.transform.parent = gotemp.transform.parent;
			gofiltrationBlock.transform.position = gotemp.transform.position;
			gofiltrationBlock.transform.rotation = gotemp.transform.rotation;
			gofiltrationBlock.transform.localScale = Vector3.one;
			gofiltrationBlock.name = szmesh;
			
			foreach(Transform child in gofiltrationBlock.transform)
			{
				child.gameObject.layer = 14;  // underwater
			}
			
			poolDesigner.goBloc = gofiltrationBlock;
			
			Destroy(gotemp);
		}
		
		GUILayout.Space(20);	
		GUILayout.EndHorizontal();
	}
	
	private IEnumerator UImoveStairway(int _isens)
	{
		blockStairway = true;
		
		if(gofiltrationBlock)
		{
			foreach(Transform child in gofiltrationBlock.transform)
			{
				child.GetComponent<Renderer>().enabled = false;
			}
			
			gofiltrationBlock.GetComponent<Renderer>().enabled = false;
		}
		
		yield return new WaitForEndOfFrame();
		
		StartCoroutine(poolDesigner.addStairway(poolDesigner.pointsData, true, _isens, poolDesigner.fsizeStairway, poolDesigner.fradiusStairway));
	}
	
	private void UIchangeStairway(string _szname, float _fsize, float _fradius, int _icurrentStairway)
	{
		if(poolDesigner.canAddStairway(poolDesigner.pointsData, _fsize))
		{
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			string sztextStyle = listStairway[_icurrentStairway] ? "TextArea" : "TextField";
			
			if(GUILayout.Button(_szname,sztextStyle,GUILayout.Height(40),GUILayout.Width(200)) && !listStairway[_icurrentStairway])
			{
				if(!blockStairway)
				{
					blockStairway = true;
					
					poolDesigner.bcenter = true;
					
					poolDesigner.broman = _fradius != 0.0f ? true : false;
					
					for(int i = 0; i < listStairway.Count; i++)
					{
						listStairway[i] = i != _icurrentStairway ? false : true;
					}
					
					if(gofiltrationBlock)
					{
						foreach(Transform child in gofiltrationBlock.transform)
						{
							child.GetComponent<Renderer>().enabled = false;
						}
						
						gofiltrationBlock.GetComponent<Renderer>().enabled = false;
					}
					
					StartCoroutine(poolDesigner.addStairway(poolDesigner.pointsData, true, 0, _fsize, _fradius));
				}
			}
			
			GUILayout.Space(20);	
			GUILayout.EndHorizontal();
		}
	}
	
	public string GetFunctionName(){return "DesignerDesjoyaux";}
	
	public string GetFunctionParameterName(){return "Designer";}
	
	public int GetFunctionId(){return os3dFuncId;}
	
	public void DoAction()
	{
		GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);
			
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;
			
		//gotemp = Camera.mainCamera.GetComponent<ObjInteraction>().getSelected();
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,true);
		Camera.main.GetComponent<ObjInteraction>().setActived(false);	
		
		//PergolaAutoFeetEvents.FireNightModeChange(UsefulFunctions.GetMontage().GetSM().IsNightMode());
		
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
		foreach(bool b in listStairway)
		{
			buf.Write((bool)b);
		}
		
		/*buf.Write((bool) _uiBloc);
		buf.Write((bool) _uiSize);
		buf.Write((bool) _uiStairway);*/
	}
	
	public void load(BinaryReader buf)
	{
		for(int i = 0; i < listStairway.Count; i++)
		{
			listStairway[i] = (bool)buf.ReadBoolean();
		}
		
		/*_uiBloc = (bool)buf.ReadBoolean();
		_uiSize = (bool)buf.ReadBoolean();
		_uiStairway = (bool)buf.ReadBoolean();*/
	}
	
	//Set L'ui si besoin
	
	public void setUI(FunctionUI_OS3D ui)
	{
	}
		
	//similaire au Save/Load mais utilisé en interne d'un objet a un autre (swap)
	public ArrayList getConfig()
	{
		ArrayList al = new ArrayList();
		
		/*al.Add(_length);
		al.Add(_width);
		al.Add(_height);
		
		//al.Add(_type);
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
		}*/
		
		
		return al;
	}
	
	public void setConfig(ArrayList config)
	{
		/*_firstBuild = true;
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
		
		//PergolaAutoFeetEvents.FirePergolaTypeChange(_type.ToString());
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
				gameObject.AddComponent(type);
			
			((IPergolaAutoFeet) GetComponent(type)).SetConfig((ArrayList)config[tmp]);
			tmp++;
		}
		
//		CreateArrows();
		
		_firstBuild = false;
		BuildIt();
//		_firstBuild = false;
//		_firstTime = Time.time + 2 * Time.fixedDeltaTime;*/
	
	}
}
