using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Jardiniere_PAF : MonoBehaviour,IPergolaAutoFeet
{
	
	public GameObject meshRef;					//Objet réference a instancier
	
	private Material	_jardiniereMaterial;
	
	private float 		_jardiniereWidth=0.84f;
	// espacement entre le poteaux et la jardinière
	public float 		_decalJardiniere = -0.005f;
	
	//------------------------------------------------------
	
	private FunctionConf_PergolaAutoFeet _paf;		//script parent
	
	private GameObject _jardinieres;					//GameObject Parent
	private Transform _pot;								//Transform du pot
	private Transform _flower;							//Transform des fleurs
	private Transform _luminaire;						//Transform de la boule lumineuse
	
	private bool _uiShow = false;				//Affichage de l UI?
	private bool _canHaveJardiniereN = true;		//peut avoir des screens coté N? (ex. patio > non)
	private bool _canHaveJardiniereS = true;		//peut avoir des screens coté S? (ex. patio > non)
	private bool _canHaveJardiniereE = true;		//peut avoir des screens coté E? (ex. patio > non)
	private bool _canHaveJardiniereW = true;		//peut avoir des screens coté W? (ex. patio > non)
	private bool _applyToAll = false;
	
	private int _selectedIndex = 0;				//numero module sélectionné
	
	private struct jardiniereData					//Structure de donnée du screen
	{
		public bool isActive;					//Si ya un Jardiniere ou pas
		public bool showFlower;					//Indique si on aficche les fleur, sinon on aficche une boule lumineuse
	}
	
	private Dictionary<string,jardiniereData> _jardinieresData = new Dictionary<string, jardiniereData>();//Liste des jardinieres avec leur datas
	private Dictionary<int,Vector4> _nsewTypes = new Dictionary<int, Vector4>(); //type de bloc par numéro de bloc
	
	private const string _sideN = "_N";				//Coté Nord/Front
	private const string _sideS = "_S";				//Coté Sud/Back
	private const string _sideE = "_E";				//Coté Est/Right
	private const string _sideW = "_W";				//Coté West/Left
	
	private bool _uiN = false;						//affichage ui reglage jardiniere N
	private bool _uiS = false;						//affichage ui reglage jardiniere S
	private bool _uiE = false;						//affichage ui reglage jardiniere E
	private bool _uiW = false;						//affichage ui reglage jardiniere W
	
	//----------vv Functions vv-----------------------------
	
	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{
		_paf = origin;		
		
		if(transform.FindChild("jardinieres"))
			_jardinieres = transform.FindChild("jardinieres").gameObject;
		else
		{
			_jardinieres = new GameObject("jardinieres");
			_jardinieres.transform.parent = parent;
			_jardinieres.transform.localPosition = Vector3.zero;
			_jardinieres.AddComponent<MeshRenderer>();
		}
		
		if(parent!=null)
		{
			Transform frame = parent.transform.FindChild("frame");
			if(frame!=null)
			{
				_pot = _jardinieres.transform.FindChild("pot");
				_flower = _jardinieres.transform.FindChild("flower");
				_luminaire = _jardinieres.transform.FindChild("luminaire");
				if(_pot!=null)
					_pot.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
			}
		}
			
	}
	
	void OnEnable()
	{
		PergolaAutoFeetEvents.selectedModuleChange += UpdateSelected;
		PergolaAutoFeetEvents.toggleUIVisibility += ToggleUI;
		PergolaAutoFeetEvents.pergolaTypeChange += UpdateType;
	}
	void OnDisable()
	{
		PergolaAutoFeetEvents.selectedModuleChange -= UpdateSelected;
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
		
		float jardiniereL = _paf.GetLocalL();
		float jardiniereW = _paf.GetLocalW();
		float jardiniereH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
		float halffootSize = footSize/2;
		
		if(/*nsew.x == 1 && */_canHaveJardiniereN)//Front = North
		{
			BuildSingleJardiniere(origine,index,_sideN);
		}	
		if(/*nsew.y == 1 &&*/ _canHaveJardiniereS)//Back = South
		{
			BuildSingleJardiniere(origine,index,_sideS);
		}
		
		if(/*nsew.w == 1 &&*/ _canHaveJardiniereW)//Left = West
		{
			BuildSingleJardiniere(origine,index,_sideW);
		}
		
		if(/*nsew.z == 1 &&*/ _canHaveJardiniereE)//Right = East
		{
			BuildSingleJardiniere(origine,index,_sideE);
		}
	}
	
	private void BuildSingleJardiniere(Vector3 origine,int index,string side)
	{		
		float jardiniereL = _paf.GetLocalL();
		float jardiniereW = _paf.GetLocalW();
		float jardiniereH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
		//float doubleFootSize = footSize/**2*/;
		//float halfFootSize = footSize/2;
		
		//Get Screen data
		jardiniereData jardiniereD;
		string tag = index.ToString()+side;
		
		if(_jardinieresData.ContainsKey(tag))
		{
			jardiniereD = _jardinieresData[tag];
			if(!jardiniereD.isActive)
				return;
		}
		else
		{
			jardiniereD.isActive 	= false;
			jardiniereD.showFlower 	= true;
			
			_jardinieresData[tag] = jardiniereD;
			return;
		}
		
		//Instantiate
		GameObject scr = (GameObject)Instantiate(meshRef);
		scr.transform.parent = _jardinieres.transform;
		scr.name = index.ToString()+side;
		Transform pot = scr.transform.FindChild("pot");
		if(pot==null)
			return;
		_jardiniereWidth = pot.GetComponent<Renderer>().bounds.extents.z;
		
		if(pot.GetComponent<Renderer>())
		{
			Transform frame = transform.FindChild("frame");
			if(frame!=null)
			{
				pot.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
			}			
		}
		
		/*float combineFeetL = _nsewTypes[index][2]+_nsewTypes[index][3];
		float decalFeetL = (_nsewTypes[index][2]-_nsewTypes[index][3])*footSize/2.0f;			
		float combineFeetW = _nsewTypes[index][0]+_nsewTypes[index][1];
		float decalFeetW = (_nsewTypes[index][0]-_nsewTypes[index][1])*footSize/2.0f;*/
		
		switch (side)
		{
		case _sideN:			
			scr.transform.localPosition = origine + new Vector3(
				jardiniereL/2-footSize+_jardiniereWidth+_decalJardiniere,
				0,
				jardiniereW/2-footSize+_jardiniereWidth+_decalJardiniere);
			scr.transform.Rotate(new Vector3(0,0,0));	
			break;
		case _sideS:
			scr.transform.localPosition = origine + new Vector3(
				-jardiniereL/2+footSize-_jardiniereWidth-_decalJardiniere,
				0,
				-jardiniereW/2+footSize-_jardiniereWidth-_decalJardiniere);
			scr.transform.Rotate(new Vector3(0,0,0));					
			break;
		case _sideE:	
			scr.transform.localPosition = origine + new Vector3(jardiniereL/2-footSize+_jardiniereWidth+_decalJardiniere,0,-jardiniereW/2+footSize-_jardiniereWidth-_decalJardiniere);
			scr.transform.Rotate(new Vector3(0,0,0));	
			break;
		case _sideW:
			scr.transform.localPosition = origine + new Vector3(-jardiniereL/2+footSize-_jardiniereWidth-_decalJardiniere,0,jardiniereW/2-footSize+_jardiniereWidth+_decalJardiniere);
			scr.transform.Rotate(new Vector3(0,0,0));	
			break;
		}
		scr.transform.FindChild("flower").gameObject.SetActive(jardiniereD.showFlower);
		scr.transform.FindChild("luminaire").gameObject.SetActive(!jardiniereD.showFlower);
	}
	
	public void GetUI(/*GUISkin skin*/)
	{		
		bool tmpui = GUILayout.Toggle(_uiShow,TextManager.GetText("Pergola.Jardiniere"),GUILayout.Height(50),GUILayout.Width(280));
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
			if(_jardinieresData.ContainsKey(_selectedIndex.ToString()+_sideN) /*&& _nsewTypes[_selectedIndex].x == 1*/ && _canHaveJardiniereN)
			{
				_uiN = GUILayout.Toggle(_uiN,"Side N","toggleN",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiN)
				{
					if(!_jardinieresData[_selectedIndex.ToString()+_sideN].isActive)
					{
						jardiniereData jardiniereD = _jardinieresData[_selectedIndex.ToString()+_sideN];
						jardiniereD.isActive = true;
						_jardinieresData[_selectedIndex.ToString()+_sideN] = jardiniereD;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiE = _uiS = _uiW = false;
				}
			}
			if(_jardinieresData.ContainsKey(_selectedIndex.ToString()+_sideS) /*&& _nsewTypes[_selectedIndex].y == 1 */&& _canHaveJardiniereS)
			{
				_uiS = GUILayout.Toggle(_uiS,"Side S","toggleS",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiS)
				{
					if(!_jardinieresData[_selectedIndex.ToString()+_sideS].isActive)
					{
						jardiniereData jardiniereD = _jardinieresData[_selectedIndex.ToString()+_sideS];
						jardiniereD.isActive = true;
						_jardinieresData[_selectedIndex.ToString()+_sideS] = jardiniereD;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiE = _uiN = _uiW = false;
				}
			}
			if(_jardinieresData.ContainsKey(_selectedIndex.ToString()+_sideE) /*&& _nsewTypes[_selectedIndex].z == 1*/ && _canHaveJardiniereE)
			{
				_uiE = GUILayout.Toggle(_uiE,"Side E","toggleE",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiE)
				{
					if(!_jardinieresData[_selectedIndex.ToString()+_sideE].isActive)
					{
						jardiniereData jardiniereD = _jardinieresData[_selectedIndex.ToString()+_sideE];
						jardiniereD.isActive = true;
						_jardinieresData[_selectedIndex.ToString()+_sideE] = jardiniereD;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiN = _uiS = _uiW = false;
				}
			}
			if(_jardinieresData.ContainsKey(_selectedIndex.ToString()+_sideW) /*&& _nsewTypes[_selectedIndex].w == 1*/ && _canHaveJardiniereW)
			{
				_uiW = GUILayout.Toggle(_uiW,"Side W","toggleW",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiW)
				{
					if(!_jardinieresData[_selectedIndex.ToString()+_sideW].isActive)
					{
						jardiniereData jardiniereD = _jardinieresData[_selectedIndex.ToString()+_sideW];
						jardiniereD.isActive = true;
						_jardinieresData[_selectedIndex.ToString()+_sideW] = jardiniereD;
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
		foreach(Transform t in _jardinieres.transform)
			Destroy(t.gameObject);
	}
	
	private void CleanData()
	{
		int max = _paf.GetNbL()*_paf.GetNbW();
		ArrayList deleteList = new ArrayList();
		
		foreach(string s in _jardinieresData.Keys)
		{
			int index = int.Parse(s.Split('_')[0]);
			if(index>=max)
				deleteList.Add(s);
		}
		
		foreach(string del in deleteList)
		{
			_jardinieresData.Remove(del);	
		}
		
		_nsewTypes.Clear();
	}
	
	//---------------------------------
	
	private void SingleUI(string tag)
	{
		jardiniereData jardiniereD = _jardinieresData[_selectedIndex.ToString()+tag];
		
		string uiStr = TextManager.GetText((jardiniereD.isActive)? "Cacher":"Afficher" );
		
		bool tmpActiv = GUILayout.Toggle(jardiniereD.isActive,uiStr,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
		if(tmpActiv != jardiniereD.isActive)
		{
			if(!tmpActiv)
				_uiN = _uiE = _uiS = _uiW = false;
			jardiniereD.isActive = tmpActiv;
			_jardinieresData[_selectedIndex.ToString()+tag] = jardiniereD;
			PergolaAutoFeetEvents.FireRebuild();
		}
		
		if(jardiniereD.isActive)
		{
			//reglage ouverture-------------------------------------------
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			string uiStrType = TextManager.GetText((jardiniereD.showFlower)? "Pergola.luminaire":"Pergola.flower");
			bool tmpType = GUILayout.Toggle(jardiniereD.showFlower,uiStrType,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			if(tmpType != jardiniereD.showFlower)
			{
				jardiniereD.showFlower = tmpType;
				_jardinieresData[_selectedIndex.ToString()+tag] = jardiniereD;
				if(_applyToAll)
				{
					ApplyToAll(jardiniereD);
				}
				else
					UpdateJardiniereType(_selectedIndex,tag);
			}
		//	GUILayout.Space(20);
			GUILayout.EndHorizontal();					
			//-----Apply to all ------------
			//la partie commenté c'est pour faire un applytoall des qu'on toggle
			_applyToAll = GUILayout.Toggle(_applyToAll,TextManager.GetText("Pergola.ApplyToAll"),"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			
		}
	}
	
	private void UpdateJardiniereType(int index, string tag)
	{		
		bool showFlower = _jardinieresData[index.ToString()+tag].showFlower;
		foreach(Transform child in _jardinieres.transform)
		{
			if(child.name.Contains(index.ToString()+tag))
			{
				child.transform.FindChild("flower").gameObject.SetActive(showFlower);
				child.transform.FindChild("luminaire").gameObject.SetActive(!showFlower);
			}		
		}	
	}
	private void ApplyToAll(jardiniereData data)
	{
		if(!data.isActive)
			return;
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_canHaveJardiniereN)
			{
				string nme = i+_sideN;
				if(_jardinieresData.ContainsKey(nme))
				{
					if(_jardinieresData[nme].isActive)
					{
						_jardinieresData[nme] = data;
						UpdateJardiniereType(i, _sideN);
					}
				}
			}
			
			if(_canHaveJardiniereS)
			{
				string nme = i+_sideS;
				if(_jardinieresData.ContainsKey(nme))
				{
					if(_jardinieresData[nme].isActive)
					{
						_jardinieresData[nme] = data;
						UpdateJardiniereType(i, _sideS);
					}
				}
			}
			
			if(_canHaveJardiniereW)
			{
				string nme = i+_sideW;
				if(_jardinieresData.ContainsKey(nme))
				{
					if(_jardinieresData[nme].isActive)
					{
						_jardinieresData[nme] = data;
						UpdateJardiniereType(i, _sideW);
					}
				}
			}
			
			if(_canHaveJardiniereE)
			{
				string nme = i+_sideE;
				if(_jardinieresData.ContainsKey(nme))
				{
					if(_jardinieresData[nme].isActive)
					{
						_jardinieresData[nme] = data;
						UpdateJardiniereType(i, _sideE);
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
	
	/*private void UpdateExtension(bool b) // Mise a jour du besoin ou non de l'extension pour les screens(si spots par ex.)
	{
		_needExtension = b;	
	}*/
	
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
			_canHaveJardiniereN = true;
			_canHaveJardiniereS = true;
			_canHaveJardiniereE = true;
			_canHaveJardiniereW = true;
			break;
		case "muralLength":
			_canHaveJardiniereN = false;
			_canHaveJardiniereS = true;
			_canHaveJardiniereE = false;
			_canHaveJardiniereW = true;
			break;
		case "muralWidth":
			_canHaveJardiniereN = true;
			_canHaveJardiniereS = false;
			_canHaveJardiniereE = true;
			_canHaveJardiniereW = false;
			break;
		case "lN":
			_canHaveJardiniereN = false;
			_canHaveJardiniereS = true;
			_canHaveJardiniereE = false;
			_canHaveJardiniereW = false;
			break;
		case "lS":
			_canHaveJardiniereN = false;
			_canHaveJardiniereS = false;
			_canHaveJardiniereE = false;
			_canHaveJardiniereW = true;
			break;
		case "patio":
			_canHaveJardiniereN = false;
			_canHaveJardiniereS = false;
			_canHaveJardiniereE = false;
			_canHaveJardiniereW = false;
			break;
		}
	}
	
	public void SaveOption(BinaryWriter buf)
	{		
		buf.Write(_jardinieresData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_jardinieresData.ContainsKey(i+_sideN))
			{
				buf.Write((string)(i+_sideN));					//Key
				buf.Write(_jardinieresData[i+_sideN].isActive);
				buf.Write(_jardinieresData[i+_sideN].showFlower);
			}
			
			if(_jardinieresData.ContainsKey(i+_sideS))
			{
				buf.Write((string)(i+_sideS));
				buf.Write(_jardinieresData[i+_sideS].isActive);
				buf.Write(_jardinieresData[i+_sideS].showFlower);
			}
			
			if(_jardinieresData.ContainsKey(i+_sideE))
			{
				buf.Write((string)(i+_sideE));
				buf.Write(_jardinieresData[i+_sideE].isActive);
				buf.Write(_jardinieresData[i+_sideE].showFlower);
			}
			
			if(_jardinieresData.ContainsKey(i+_sideW))
			{
				buf.Write((string)(i+_sideW));
				buf.Write(_jardinieresData[i+_sideW].isActive);
				buf.Write(_jardinieresData[i+_sideW].showFlower);
			}
		}
	}
	
	public void LoadOption(BinaryReader buf)
	{
		int entryNb = buf.ReadInt32();
		_jardinieresData.Clear();
		
		for(int i=0;i<entryNb;i++)
		{
			string key = buf.ReadString();
			
			jardiniereData jardiniereD;
			
			jardiniereD.isActive = buf.ReadBoolean();
			jardiniereD.showFlower = buf.ReadBoolean();
			
			_jardinieresData.Add(key,jardiniereD);
		}
	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_jardinieresData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_jardinieresData.ContainsKey(i+_sideN))
			{
				al.Add((string)(i+_sideN));					//Key
				al.Add(_jardinieresData[i+_sideN].isActive);
			}
			
			if(_jardinieresData.ContainsKey(i+_sideS))
			{
				al.Add((string)(i+_sideS));
				al.Add(_jardinieresData[i+_sideS].isActive);
			}
			
			if(_jardinieresData.ContainsKey(i+_sideE))
			{
				al.Add((string)(i+_sideE));
				al.Add(_jardinieresData[i+_sideE].isActive);
			}
			
			if(_jardinieresData.ContainsKey(i+_sideW))
			{
				al.Add((string)(i+_sideW));
				al.Add(_jardinieresData[i+_sideW].isActive);
			}
		}
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		int entryNb = (int)al[0];
		
		_jardinieresData.Clear();
		
		int off7 = 1;
		for(int i=0;i<entryNb;i++)
		{
			string key = (string)al[off7];
			off7 ++;
			
			jardiniereData jardiniereD;

			jardiniereD.isActive = (bool)al[off7];
			//off7 ++;
			jardiniereD.showFlower = (bool)al[off7];
			off7 ++;
			
			_jardinieresData.Add(key,jardiniereD);
		}
	}
	
}
