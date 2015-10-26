using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Screens_PAF : MonoBehaviour,IPergolaAutoFeet
{
	
	public GameObject meshRef;					//Objet réference a instancier
	public GameObject extensionRef;
	
	//obsolete vv
//	public Material screenMat;					// Materiau du screen
	
	public string[]		screenMatsName;			// Nom des materiaux
	public Material[] 	screenMats;				// Materiaux de screen
	public Color[] 		screenColors;			// Couleurs de screen
	
	//------------------------------------------------------
	
	private FunctionConf_PergolaAutoFeet _paf;		//script parent
	
	private GameObject _screens;				//GameObject Parent
	
	private bool _needExtension = false;		//besoin extension screens?
	private bool _uiShow = false;				//Affichage de l UI?
	private bool _canHaveScreenN = true;		//peut avoir des screens coté N? (ex. patio > non)
	private bool _canHaveScreenS = true;		//peut avoir des screens coté S? (ex. patio > non)
	private bool _canHaveScreenE = true;		//peut avoir des screens coté E? (ex. patio > non)
	private bool _canHaveScreenW = true;		//peut avoir des screens coté W? (ex. patio > non)
	private bool _applyToAll = false;
	
	//obsolete vv
//	private Dictionary<string,float> _openings = new Dictionary<string, float>();//Liste des screens avec leur %age d'ouverture

	private int _selectedIndex = 0;				//numero module sélectionné
	
	private struct screenData					//Structure de donnée du screen
	{
		public bool isActive;					//Si ya un screen ou pas
		public float opening;					//% ouverture (0 > 1)
		public int indexColor;					//index de couleur
		public int indexMat;					//Index de materiau
	}
	
	private Dictionary<string,screenData> _screensData = new Dictionary<string, screenData>();//Liste des screens avec leur datas
	private Dictionary<int,Vector4> _nsewTypes = new Dictionary<int, Vector4>(); //type de bloc par numéro de bloc
	
	private const string _sideN = "_N";				//Coté Nord/Front
	private const string _sideS = "_S";				//Coté Sud/Back
	private const string _sideE = "_E";				//Coté Est/Right
	private const string _sideW = "_W";				//Coté West/Left
	
	private bool _uiN = false;						//affichage ui reglage screen N
	private bool _uiS = false;						//affichage ui reglage screen S
	private bool _uiE = false;						//affichage ui reglage screen E
	private bool _uiW = false;						//affichage ui reglage screen W
	
	private static Texture2D[] _uiScreenColors;		//Couleurs possibles des screens  /!\ faudrait ptet faire un load from resources au lieu du static
	private int _scrollPosMat;				//position scroll selecteur couleur
	private int _scrollPosCol;				//position scroll selecteur couleur
		
	//----------vv Functions vv-----------------------------
	
	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{
		_paf = origin;
		
		if(transform.FindChild("screens"))
			_screens = transform.FindChild("screens").gameObject;
		else
		{
			_screens = new GameObject("screens");
			_screens.transform.parent = parent;
			_screens.transform.localPosition = Vector3.zero;
		}
		
		//Init ui
		if(screenColors.Length>0 && _uiScreenColors == null) //Si null on le créé
		{
			_uiScreenColors = new Texture2D[screenColors.Length];
			for(int i=0;i<screenColors.Length;i++)
			{
				Texture2D t = new Texture2D(40,40);//40*40
				Color [] tmp = new Color[t.GetPixels().Length];
				for(int c=0;c<t.GetPixels().Length;c++)
				{
					tmp[c] = screenColors[i];
				}
				t.SetPixels(tmp);		
				t.Apply();
				_uiScreenColors[i] = t;
			}
		}
		else if(_uiScreenColors != null) // si pas null mais que pas assez de couleurs /rap au public
		{
			if(_uiScreenColors.Length < screenColors.Length)
			{
				_uiScreenColors = new Texture2D[screenColors.Length];
				for(int i=0;i<screenColors.Length;i++)
				{
					Texture2D t = new Texture2D(40,40);//40*40
					Color [] tmp = new Color[t.GetPixels().Length];
					for(int c=0;c<t.GetPixels().Length;c++)
					{
						tmp[c] = screenColors[i];
					}
					t.SetPixels(tmp);		
					t.Apply();
					_uiScreenColors[i] = t;
				}
			}
		}
	}
	
	void OnEnable()
	{
		PergolaAutoFeetEvents.selectedModuleChange += UpdateSelected;
		PergolaAutoFeetEvents.needScreenExtension += UpdateExtension;
		PergolaAutoFeetEvents.toggleUIVisibility += ToggleUI;
		PergolaAutoFeetEvents.pergolaTypeChange += UpdateType;
	}
	void OnDisable()
	{
		PergolaAutoFeetEvents.selectedModuleChange -= UpdateSelected;
		PergolaAutoFeetEvents.needScreenExtension -= UpdateExtension;
		PergolaAutoFeetEvents.toggleUIVisibility -= ToggleUI;
		PergolaAutoFeetEvents.pergolaTypeChange -= UpdateType;
	}
	
	public void Build(Vector3 origine,int index,Vector4 nsew)
	{
		
		if(index == 0)
		{
			CleanData();//Clear le _nsewTypes
		}
		
		_nsewTypes.Add(index,nsew);
		
		float screenL = _paf.GetLocalL();
		float screenW = _paf.GetLocalW();
		float screenH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
		float halffootSize = footSize/2;
		
		if(nsew.x == 1 && _canHaveScreenN)//Front = North
		{
			BuildSingleScreen(origine,index,_sideN);
			if(_needExtension)
				BuildSingleExtension(origine,index,_sideN);
		}	
		if(nsew.y == 1 && _canHaveScreenS)//Back = South
		{
			BuildSingleScreen(origine,index,_sideS);
			if(_needExtension)
				BuildSingleExtension(origine,index,_sideS);
		}
		
		if(nsew.w == 1 && _canHaveScreenW)//Left = West
		{
			BuildSingleScreen(origine,index,_sideW);
			if(_needExtension)
				BuildSingleExtension(origine,index,_sideW);
		}
		
		if(nsew.z == 1 && _canHaveScreenE)//Right = East
		{
			BuildSingleScreen(origine,index,_sideE);
			if(_needExtension)
				BuildSingleExtension(origine,index,_sideE);
		}
	}
	
	private void BuildSingleScreen(Vector3 origine,int index,string side)
	{
		float screenL = _paf.GetLocalL();
		float screenW = _paf.GetLocalW();
		float screenH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
		float halffootSize = footSize/8;
		
		//Get Screen data
		screenData sd;
		string tag = index.ToString()+side;
		
		if(_screensData.ContainsKey(tag))
		{
			sd = _screensData[tag];
			if(!sd.isActive)
				return;
		}
		else
		{
			sd.isActive = false;
			sd.opening = 0.5f;
			sd.indexMat = 0;
			sd.indexColor = 0;
			
			_screensData[tag] = sd;
			return;
		}
		
		//Instantiate
		GameObject scr = (GameObject)Instantiate(meshRef);
		scr.transform.parent = _screens.transform;
		scr.name = index.ToString()+side;
		
		if(scr.GetComponent<Renderer>())
		{
			scr.GetComponent<Renderer>().material = screenMats[sd.indexMat];
			scr.GetComponent<Renderer>().material.color = screenColors[sd.indexColor];
		}
		else
		{
			scr.transform.GetChild(0).GetComponent<Renderer>().material = screenMats[sd.indexMat];
			scr.transform.GetChild(0).GetComponent<Renderer>().material.color = screenColors[sd.indexColor];
		}
		
		float h = (_paf.GetH() - _paf.GetFootSize());
		float sizeH = sd.opening * h;
		float posH = h-(sizeH/2);
		
		switch (side)
		{
		case _sideN:
			scr.transform.localPosition = origine + new Vector3(0,posH,screenW/2-halffootSize);
			scr.transform.localScale = new Vector3(screenL/*-2*/-1*footSize,sizeH,1);
			scr.transform.Rotate(new Vector3(0,0,0));
			break;
		case _sideS:
			scr.transform.localPosition = origine + new Vector3(0,posH,-(screenW/2-halffootSize));
			scr.transform.localScale = new Vector3(screenL/*-2*/-1*footSize,sizeH,1);
			scr.transform.Rotate(new Vector3(0,180,0));
			break;
		case _sideE:
			scr.transform.localPosition = origine + new Vector3(screenL/2-halffootSize,posH,0);
			scr.transform.localScale = new Vector3(screenW/*-2*/-1*footSize,sizeH,1);
			scr.transform.Rotate(new Vector3(0,90,0));
			break;
		case _sideW:
			scr.transform.localPosition = origine + new Vector3(-screenL/2+halffootSize,posH,0);
			scr.transform.localScale = new Vector3(screenW/*-2*/-1*footSize,sizeH,1);
			scr.transform.Rotate(new Vector3(0,270,0));
			break;
		}
		
		if(scr.GetComponent<Renderer>())
			scr.AddComponent<ApplyShader>();
		else
			scr.transform.GetChild(0).gameObject.AddComponent<ApplyShader>();
		
	}
	
	private void BuildSingleExtension(Vector3 origine,int index,string side)
	{	
		//Get Screen data
		screenData sd;
		string tag = index.ToString()+side;
		
		if(_screensData.ContainsKey(tag))
		{
			sd = _screensData[tag];
			if(!sd.isActive)
				return;
		}
		else
			return;
		
		float screenL = _paf.GetLocalL();
		float screenW = _paf.GetLocalW();
		float screenH = _paf.GetH() - _paf.GetFrameSizeH()/2;
		
		float footSize = _paf.GetFootSize();
		float halffootSize = footSize/2;
		
		//Instantiate
		GameObject scr = (GameObject)Instantiate(extensionRef);
		scr.transform.parent = /*_screens.transform*/_paf.GetFrame().transform;
		scr.name = index.ToString()+side;
		
		if(scr.GetComponent<Renderer>())
		{
			scr.GetComponent<Renderer>().material = _paf.GetFrameMat();
		}
		else
		{
			scr.transform.GetChild(0).GetComponent<Renderer>().material = _paf.GetFrameMat();
		}
		
		switch (side)
		{
		case _sideN:
			scr.transform.localPosition = origine + new Vector3(0,screenH,screenW/2+halffootSize);
			scr.transform.localScale = new Vector3(screenL,_paf.GetFrameSizeH(),footSize);
			scr.transform.Rotate(new Vector3(0,0,0));
			break;
		case _sideS:
			scr.transform.localPosition = origine + new Vector3(0,screenH,-(screenW/2+halffootSize));
			scr.transform.localScale = new Vector3(screenL,_paf.GetFrameSizeH(),footSize);
			scr.transform.Rotate(new Vector3(0,180,0));
			break;
		case _sideE:
			scr.transform.localPosition = origine + new Vector3(screenL/2+halffootSize,screenH,0);
			scr.transform.localScale = new Vector3(screenW,_paf.GetFrameSizeH(),footSize);
			scr.transform.Rotate(new Vector3(0,90,0));
			break;
		case _sideW:
			scr.transform.localPosition = origine + new Vector3(-screenL/2-halffootSize,screenH,0);
			scr.transform.localScale = new Vector3(screenW,_paf.GetFrameSizeH(),footSize);
			scr.transform.Rotate(new Vector3(0,270,0));
			break;
		}	
		
	}
	
	public void GetUI(/*GUISkin skin*/)
	{		
		bool tmpui = GUILayout.Toggle(_uiShow,TextManager.GetText("Pergola.Screens"),GUILayout.Height(50),GUILayout.Width(280));
		if(tmpui != _uiShow)
		{
			_uiShow = tmpui;
			if(_uiShow)
				PergolaAutoFeetEvents.FireToggleUIVisibility(GetType().ToString());
			else
			{
				PergolaAutoFeetEvents.FireToggleUIVisibility("close");
			}
		}
		
		if(_uiShow)
		{
			_paf.GetModuleSelectorUI();
			
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			if(_screensData.ContainsKey(_selectedIndex.ToString()+_sideN) && _nsewTypes[_selectedIndex].x == 1 && _canHaveScreenN)
			{
				_uiN = GUILayout.Toggle(_uiN,"Side N","toggleN",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiN)
				{
					if(!_screensData[_selectedIndex.ToString()+_sideN].isActive)
					{
						screenData sd = _screensData[_selectedIndex.ToString()+_sideN];
						sd.isActive = true;
						_screensData[_selectedIndex.ToString()+_sideN] = sd;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiE = _uiS = _uiW = false;
				}
			}
			if(_screensData.ContainsKey(_selectedIndex.ToString()+_sideS) && _nsewTypes[_selectedIndex].y == 1 && _canHaveScreenS)
			{
				_uiS = GUILayout.Toggle(_uiS,"Side S","toggleS",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiS)
				{
					if(!_screensData[_selectedIndex.ToString()+_sideS].isActive)
					{
						screenData sd = _screensData[_selectedIndex.ToString()+_sideS];
						sd.isActive = true;
						_screensData[_selectedIndex.ToString()+_sideS] = sd;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiE = _uiN = _uiW = false;
				}
			}
			if(_screensData.ContainsKey(_selectedIndex.ToString()+_sideE) && _nsewTypes[_selectedIndex].z == 1 && _canHaveScreenE)
			{
				_uiE = GUILayout.Toggle(_uiE,"Side E","toggleE",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiE)
				{
					if(!_screensData[_selectedIndex.ToString()+_sideE].isActive)
					{
						screenData sd = _screensData[_selectedIndex.ToString()+_sideE];
						sd.isActive = true;
						_screensData[_selectedIndex.ToString()+_sideE] = sd;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiN = _uiS = _uiW = false;
				}
			}
			if(_screensData.ContainsKey(_selectedIndex.ToString()+_sideW) && _nsewTypes[_selectedIndex].w == 1 && _canHaveScreenW)
			{
				_uiW = GUILayout.Toggle(_uiW,"Side W","toggleW",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiW)
				{
					if(!_screensData[_selectedIndex.ToString()+_sideW].isActive)
					{
						screenData sd = _screensData[_selectedIndex.ToString()+_sideW];
						sd.isActive = true;
						_screensData[_selectedIndex.ToString()+_sideW] = sd;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiE = _uiS = _uiN = false;
				}
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
			
			if(_uiN)
			{
				SingleUI(_sideN);//Front
			}
			
			else if(_uiS)
			{
				SingleUI(_sideS);//Back
			}
			
			else if(_uiE)
			{
				SingleUI(_sideE);//Right
			}
			else if(_uiW)
			{
				SingleUI(_sideW);//Left
			}
			
		}
	}
	
	public void Clear()
	{
		foreach(Transform t in _screens.transform)
			Destroy(t.gameObject);
//		if(nbl*nbw > paf.GetNbL()*paf.GetNbW())
//			_openings.Clear();
	}
	
	private void CleanData()
	{
		int max = _paf.GetNbL()*_paf.GetNbW();
		ArrayList deleteList = new ArrayList();
		
		foreach(string s in _screensData.Keys)
		{
			int index = int.Parse(s.Split('_')[0]);
			if(index>=max)
				deleteList.Add(s);
		}
		
		foreach(string del in deleteList)
		{
			_screensData.Remove(del);	
		}
		
		_nsewTypes.Clear();
	}
	
	//---------------------------------
	
	private void SingleUI(string tag)
	{
		screenData sd = _screensData[_selectedIndex.ToString()+tag];
		
		string uiStr = TextManager.GetText((sd.isActive)? "Cacher":"Afficher" );
		
		bool tmpActiv = GUILayout.Toggle(sd.isActive,uiStr,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
		if(tmpActiv != sd.isActive)
		{
			if(!tmpActiv)
				_uiN = _uiE = _uiS = _uiW = false;
			sd.isActive = tmpActiv;
			_screensData[_selectedIndex.ToString()+tag] = sd;
			PergolaAutoFeetEvents.FireRebuild();
		}
		
		if(sd.isActive)
		{
			//reglage ouverture-------------------------------------------
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			float tmpf = GUILayout.HorizontalSlider(sd.opening,0,1,GUILayout.Width(200));
			if(tmpf != sd.opening)
			{
				sd.opening = tmpf;
				_screensData[_selectedIndex.ToString()+tag] = sd;
				if(_applyToAll)
				{
					ApplyToAll(sd);
				}
				else
					UpdateScreenOpenning(_selectedIndex.ToString()+tag);
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
			
			//reglage matiere-------------------------------------------
			
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			if(GUILayout.Button("","btn<",GUILayout.Width(50),GUILayout.Height(50)))
			{
				_scrollPosMat --;
				if(_scrollPosMat < 0)
					_scrollPosMat = 0;
				
			}
			//----------
			for(int mat=_scrollPosMat;mat<_scrollPosMat+3;mat++)
			{
				if(GUILayout.Toggle(mat == sd.indexMat ,screenMatsName[mat],"grid",GUILayout.Width(50),GUILayout.Height(50)))
				{
					if(mat != sd.indexMat)
					{
						sd.indexMat = mat;
						_screensData[_selectedIndex.ToString()+tag] = sd;
						if(_applyToAll)
						{
							ApplyToAll(sd);
						}
						else
							UpdateScreenMat(_selectedIndex.ToString()+tag);
					}
				}
			}
	
			//----------
			if(GUILayout.Button("","btn>",GUILayout.Width(50),GUILayout.Height(50)))
			{
				_scrollPosMat ++;
				if(_scrollPosMat+3 > screenMatsName.Length)
					_scrollPosMat = screenMatsName.Length-3;
			}
			
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
			
			//reglage Couleur-------------------------------------------
				
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			if(GUILayout.Button("","btn<",GUILayout.Width(50),GUILayout.Height(50)))
			{
				_scrollPosCol --;
				if(_scrollPosCol < 0)
					_scrollPosCol = 0;
				
			}
			//----------
			for(int col=_scrollPosCol;col<_scrollPosCol+3;col++)
			{
				if(GUILayout.Toggle(col == sd.indexColor,_uiScreenColors[col],"grid",GUILayout.Width(50),GUILayout.Height(50)))
				{
					if(col != sd.indexColor)
					{
						sd.indexColor = col;
						_screensData[_selectedIndex.ToString()+tag] = sd;
						if(_applyToAll)
						{
							ApplyToAll(sd);
						}
						else
							UpdateScreenMat(_selectedIndex.ToString()+tag);
					}
				}
			}
	
			//----------
			if(GUILayout.Button("","btn>",GUILayout.Width(50),GUILayout.Height(50)))
			{
				_scrollPosCol ++;
				if(_scrollPosCol+3 > _uiScreenColors.Length)
					_scrollPosCol = _uiScreenColors.Length-3;
			}
			
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
			
			//-----Apply to all ------------
			//la partie commenté c'est pour faire un applytoall des qu'on toggle
			_applyToAll = GUILayout.Toggle(_applyToAll,TextManager.GetText("Pergola.ApplyToAll"),"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			
			/*bool tmpApply = GUILayout.Toggle(_applyToAll,TextManager.GetText("Pergola.ApplyToAll"),"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			if(tmpApply != _applyToAll)
			{
				_applyToAll = tmpApply;
				if(_applyToAll)
					ApplyToAll(sd);
			}*/
		}
		
	}

	private void UpdateScreenOpenning(string name)
	{		
		float open = _screensData[name].opening;
		Transform scr = _screens.transform.FindChild(name);
				
		float h = (_paf.GetH() - _paf.GetFootSize());
		float sizeH = open * h;
		float posH = h-(sizeH/2);
		
		
		Vector3 tmpPos = scr.transform.localPosition;
		Vector3 tmpScl = scr.transform.localScale ;
		
		tmpPos.y = posH;
		tmpScl.y = sizeH;
		
		scr.transform.localPosition = tmpPos;
		scr.transform.localScale = tmpScl;
		
		UpdateMapOffset(scr,open);
	}
	
	private void UpdateScreenMat(string name)
	{		
		int matId = _screensData[name].indexMat;
		int colId = _screensData[name].indexColor;
		
		Transform scr = _screens.transform.FindChild(name);
		
		if(scr.GetComponent<Renderer>())
		{
			scr.GetComponent<Renderer>().material = screenMats[matId];
			scr.GetComponent<Renderer>().material.color = screenColors[colId];
			if(usefullData.lowTechnologie)
			{
				if(scr.GetComponent<Renderer>().material.shader.name.Contains("2Sided"))
				{
					Shader shad = (Shader)Resources.Load("shaders/Custom_alphaColor"); 
					
					Texture alpha = new Texture2D(1,1);
					bool hastextureAlpha=false;
					if(scr.GetComponent<Renderer>().material.HasProperty("_AlphaTex"))
					{
						alpha =scr.GetComponent<Renderer>().material.GetTexture("_AlphaTex");
						hastextureAlpha = true;
					}			
					
					scr.GetComponent<Renderer>().material.shader = shad;
					
					if((hastextureAlpha) && (alpha!=null))
						scr.GetComponent<Renderer>().material.mainTexture = alpha;
				}
			}
		}
		else
		{
			scr.transform.GetChild(0).GetComponent<Renderer>().material = screenMats[matId];
			scr.transform.GetChild(0).GetComponent<Renderer>().material.color = screenColors[colId];
			if(usefullData.lowTechnologie)
			{
				if(scr.transform.GetChild(0).GetComponent<Renderer>().material.shader.name.Contains("2Sided"))
				{
					Shader shad = (Shader)Resources.Load("shaders/Custom_alphaColor"); 
					
					Texture alpha = new Texture2D(1,1);
					bool hastextureAlpha=false;
					if(scr.transform.GetChild(0).GetComponent<Renderer>().material.HasProperty("_AlphaTex"))
					{
						alpha =scr.transform.GetChild(0).GetComponent<Renderer>().material.GetTexture("_AlphaTex");
						hastextureAlpha = true;
					}
					
					scr.transform.GetChild(0).GetComponent<Renderer>().material.shader = shad;
					
					if((hastextureAlpha) && (alpha!=null))
						scr.transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = alpha;
					
				}
			}
		}
		UpdateScreenOpenning(name);
	}
	
	private void UpdateMapOffset(Transform s,float opn)
	{
		if(s.GetComponent<Renderer>())
		{
			if(s.GetComponent<Renderer>().material.HasProperty("_AlphaTex"))
			{
				Vector2 off7 = s.GetComponent<Renderer>().material.GetTextureScale("_AlphaTex");
				off7.y = opn;
				s.GetComponent<Renderer>().material.SetTextureScale("_AlphaTex",off7);
			}
			else if ((s.GetComponent<Renderer>().material.HasProperty("_MainTex")) && (usefullData.lowTechnologie))
			{
				Vector2 off7 = s.GetComponent<Renderer>().material.GetTextureScale("_MainTex");
				off7.y = opn;
				s.GetComponent<Renderer>().material.SetTextureScale("_MainTex",off7);
			}
		}
		else
		{
			if(s.transform.GetChild(0).GetComponent<Renderer>().material.HasProperty("_AlphaTex"))
			{
				Vector2 off7 = s.transform.GetChild(0).GetComponent<Renderer>().material.GetTextureScale("_AlphaTex");
				off7.y = opn;
				s.transform.GetChild(0).GetComponent<Renderer>().material.SetTextureScale("_AlphaTex",off7);
			}
			else if ((s.transform.GetChild(0).GetComponent<Renderer>().material.HasProperty("_MainTex")) && (usefullData.lowTechnologie))
			{
				Vector2 off7 = s.transform.GetChild(0).GetComponent<Renderer>().material.GetTextureScale("_MainTex");
				off7.y = opn;
				s.transform.GetChild(0).GetComponent<Renderer>().material.SetTextureScale("_MainTex",off7);
			}
		}
	}
	
	private void ApplyToAll(screenData data)
	{
		if(!data.isActive)
			return;
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_nsewTypes[i].x == 1 && _canHaveScreenN)
			{
				string nme = i+_sideN;
				if(_screensData.ContainsKey(nme))
				{
					if(_screensData[nme].isActive)
					{
						_screensData[nme] = data;
						UpdateScreenOpenning(nme);
						UpdateScreenMat(nme);
					}
				}
			}
			
			if(_nsewTypes[i].y == 1 && _canHaveScreenS)
			{
				string nme = i+_sideS;
				if(_screensData.ContainsKey(nme))
				{
					if(_screensData[nme].isActive)
					{
						_screensData[nme] = data;
						UpdateScreenOpenning(nme);
						UpdateScreenMat(nme);
					}
				}
			}
			
			if(_nsewTypes[i].w == 1 && _canHaveScreenW)
			{
				string nme = i+_sideW;
				if(_screensData.ContainsKey(nme))
				{
					if(_screensData[nme].isActive)
					{
						_screensData[nme] = data;
						UpdateScreenOpenning(nme);
						UpdateScreenMat(nme);
					}
				}
			}
			
			if(_nsewTypes[i].z == 1 && _canHaveScreenE)
			{
				string nme = i+_sideE;
				if(_screensData.ContainsKey(nme))
				{
					if(_screensData[nme].isActive)
					{
						_screensData[nme] = data;
						UpdateScreenOpenning(nme);
						UpdateScreenMat(nme);
					}
				}
			}
		}
	}
	
	//---------------------------------------------------------
	
	private void UpdateSelected(int i) // Mise a jour du bloc sélectionné
	{
		_selectedIndex = i;
		_uiN = _uiS = _uiE = _uiW = false;
	}
	
	private void UpdateExtension(bool b) // Mise a jour du besoin ou non de l'extension pour les screens(si spots par ex.)
	{
		_needExtension = b;	
	}
	
	public void ToggleUI(string s) // ordre d'affichage (un affiché, les autres caché)
	{
		if(s != GetType().ToString())
		{
			_uiShow = false;
			_uiN = _uiS = _uiE = _uiW = false;
		}
	}
	
	private void UpdateType(string s) //Mise a jour du type (si patio > pas de screens)
	{
		switch (s)
		{
		case "ilot":
			_canHaveScreenN = true;
			_canHaveScreenS = true;
			_canHaveScreenE = true;
			_canHaveScreenW = true;
			break;
		case "muralLength":
			_canHaveScreenN = false;
			_canHaveScreenS = true;
			_canHaveScreenE = true;
			_canHaveScreenW = true;
			break;
		case "muralWidth":
			_canHaveScreenN = true;
			_canHaveScreenS = true;
			_canHaveScreenE = true;
			_canHaveScreenW = false;
			break;
		case "lN":
			_canHaveScreenN = false;
			_canHaveScreenS = true;
			_canHaveScreenE = false;
			_canHaveScreenW = true;
			break;
		case "lS":
			_canHaveScreenN = true;
			_canHaveScreenS = false;
			_canHaveScreenE = false;
			_canHaveScreenW = true;
			break;
		case "patio":
			_canHaveScreenN = false;
			_canHaveScreenS = false;
			_canHaveScreenE = false;
			_canHaveScreenW = false;
			break;
		}
	}
	
	public void SaveOption(BinaryWriter buf)
	{		
		buf.Write(_needExtension);
		buf.Write(_screensData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_screensData.ContainsKey(i+_sideN))
			{
				buf.Write((string)(i+_sideN));					//Key
				buf.Write(_screensData[i+_sideN].indexColor);	//Value
				buf.Write(_screensData[i+_sideN].indexMat);		//Value
				buf.Write(_screensData[i+_sideN].opening);		//Value
				buf.Write(_screensData[i+_sideN].isActive);
			}
			
			if(_screensData.ContainsKey(i+_sideS))
			{
				buf.Write((string)(i+_sideS));
				buf.Write(_screensData[i+_sideS].indexColor);
				buf.Write(_screensData[i+_sideS].indexMat);
				buf.Write(_screensData[i+_sideS].opening);
				buf.Write(_screensData[i+_sideS].isActive);
			}
			
			if(_screensData.ContainsKey(i+_sideE))
			{
				buf.Write((string)(i+_sideE));
				buf.Write(_screensData[i+_sideE].indexColor);
				buf.Write(_screensData[i+_sideE].indexMat);
				buf.Write(_screensData[i+_sideE].opening);
				buf.Write(_screensData[i+_sideE].isActive);
			}
			
			if(_screensData.ContainsKey(i+_sideW))
			{
				buf.Write((string)(i+_sideW));
				buf.Write(_screensData[i+_sideW].indexColor);
				buf.Write(_screensData[i+_sideW].indexMat);
				buf.Write(_screensData[i+_sideW].opening);
				buf.Write(_screensData[i+_sideW].isActive);
			}
		}
	}
	
	public void LoadOption(BinaryReader buf)
	{
		_needExtension = buf.ReadBoolean();
		int entryNb = buf.ReadInt32();
		_screensData.Clear();
		
		for(int i=0;i<entryNb;i++)
		{
			string key = buf.ReadString();
			
			screenData sd;
			
			sd.indexColor = buf.ReadInt32();
			sd.indexMat = buf.ReadInt32();
			sd.opening = buf.ReadSingle();
			sd.isActive = buf.ReadBoolean();
			
			_screensData.Add(key,sd);
		}
	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_needExtension);
		al.Add(_screensData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_screensData.ContainsKey(i+_sideN))
			{
				al.Add((string)(i+_sideN));					//Key
				al.Add(_screensData[i+_sideN].indexColor);	//Value
				al.Add(_screensData[i+_sideN].indexMat);		//Value
				al.Add(_screensData[i+_sideN].opening);		//Value
				al.Add(_screensData[i+_sideN].isActive);
			}
			
			if(_screensData.ContainsKey(i+_sideS))
			{
				al.Add((string)(i+_sideS));
				al.Add(_screensData[i+_sideS].indexColor);
				al.Add(_screensData[i+_sideS].indexMat);
				al.Add(_screensData[i+_sideS].opening);
				al.Add(_screensData[i+_sideS].isActive);
			}
			
			if(_screensData.ContainsKey(i+_sideE))
			{
				al.Add((string)(i+_sideE));
				al.Add(_screensData[i+_sideE].indexColor);
				al.Add(_screensData[i+_sideE].indexMat);
				al.Add(_screensData[i+_sideE].opening);
				al.Add(_screensData[i+_sideE].isActive);
			}
			
			if(_screensData.ContainsKey(i+_sideW))
			{
				al.Add((string)(i+_sideW));
				al.Add(_screensData[i+_sideW].indexColor);
				al.Add(_screensData[i+_sideW].indexMat);
				al.Add(_screensData[i+_sideW].opening);
				al.Add(_screensData[i+_sideW].isActive);
			}
		}
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		_needExtension = (bool)al[0];
		int entryNb = (int)al[1];
		
		_screensData.Clear();
		
		int off7 = 2;
		for(int i=0;i<entryNb;i++)
		{
			string key = (string)al[off7];
			off7 ++;
			
			screenData sd;
			
			sd.indexColor = (int)al[off7];
			off7 ++;
			sd.indexMat = (int)al[off7];
			off7 ++;
			sd.opening = (float)al[off7];
			off7 ++;
			sd.isActive = (bool)al[off7];
			off7 ++;
			
			_screensData.Add(key,sd);
		}
	}
	
}
