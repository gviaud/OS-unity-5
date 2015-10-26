using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SideAccessories_PAF : MonoBehaviour,IPergolaAutoFeet
{
	public GameObject meshRef;	//Objet réference a instancier
	
	public int nbMax;			//nb d'accessoires max sur un coté 
	
	public float AccLSize;		//taille en largeur de l'accessoire
	public float hOff7;			//off7 de réglage en hauteur
	public float lOff7;			//off7 de marge entre deux accessoires
	public bool lOff7Auto;		//off7 de marge auto > meme distance avant, entre, et apres les accs
	
	public string accName;		//nom de l'accessoire
	//-------------------------
	
	private int _selectedIndex; //numero module sélectionné
	
	private FunctionConf_PergolaAutoFeet _paf;	//script parent
	
	private GameObject _accessories;		//GameObject Parent
	
	private struct AccPosNb					//Strutcture accessoire, nombre / positions(NSEW)
	{
		public int N;
		public int S;
		public int E;
		public int W;
	}
	
	private Dictionary<int,AccPosNb> _accList = new Dictionary<int, AccPosNb>(); //Liste de acc / num. de module
	
	private AccPosNb _uiAPNb;					//AccPosNb pour l'UI
	
	private bool _uiShow = false;			//Affichage de l UI
	private bool _uiN = false;			//toggle affichage
	private bool _uiS = false;			//toggle affichage
	private bool _uiE = false;			//toggle affichage
	private bool _uiW = false;			//toggle affichage
	
	//----------vv Functions vv-----------------------------
	
	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{
		_paf = origin;
		
		if(transform.FindChild("Acc_"+accName))
			_accessories = transform.FindChild("Acc_"+accName).gameObject;
		else
		{
			_accessories = new GameObject("Acc_"+accName);
			_accessories.transform.parent = parent;
			_accessories.transform.localPosition = Vector3.zero;
		}
	}
	
	void OnEnable()
	{
		PergolaAutoFeetEvents.selectedModuleChange += UpdateSelected;
		PergolaAutoFeetEvents.toggleUIVisibility += ToggleUI;
	}
	void OnDisable()
	{
		PergolaAutoFeetEvents.selectedModuleChange -= UpdateSelected;
		PergolaAutoFeetEvents.toggleUIVisibility -= ToggleUI;
	}
	
	public void Build(Vector3 origine,int index,Vector4 nsew)
	{
		if(_accList.ContainsKey(index))
		{			
			AccPosNb tmp = _accList[index];
			
			if(tmp.N >0 && tmp.N <= nbMax)
			{
				AddAccs(origine,index,ref tmp.N,"N");
			}
			if(tmp.S >0 && tmp.S <= nbMax)
			{
				AddAccs(origine,index,ref tmp.S,"S");
			}
			if(tmp.E >0 && tmp.E <= nbMax)
			{
				AddAccs(origine,index,ref tmp.E,"E");
			}
			if(tmp.W >0 && tmp.W <= nbMax)
			{
				AddAccs(origine,index,ref tmp.W,"W");
			}
			
			if(index == _selectedIndex)
				_uiAPNb = tmp;
			
		}
	}
	
	private void AddAccs(Vector3 Origine,int index,ref int nb,string pos)
	{
		float innerW = _paf.GetLocalW() - _paf.GetFootSize();
		float innerL = _paf.GetLocalL() - _paf.GetFootSize();
		float innerH = _paf.GetH()-_paf.GetFrameSizeH()-hOff7;
		
		if(lOff7Auto)
			lOff7 = 0;
		
		if((nb*AccLSize + (nb-1)*lOff7)>innerW && (pos == "W" || pos == "E"))
		{
//			nb = 0;
			while((nb*AccLSize + (nb-1)*lOff7)>innerW)
			{
				nb--;	
			}
		}
		
		if((nb*AccLSize + (nb-1)*lOff7)>innerL && (pos == "N" || pos == "S"))
		{
//			nb = 0;
			while((nb*AccLSize + (nb-1)*lOff7)>innerL)
			{
				nb--;	
			}
		}
		
		float startOff7 = 0;
		
		if(lOff7Auto)
		{
			if(pos == "W" || pos == "E")
			{
				lOff7 = (innerW - (nb*AccLSize))/(nb+1);
				startOff7 = -innerW/2 + lOff7 + AccLSize/2;
			}
			if(pos == "N" || pos == "S")
			{
				lOff7 = (innerL - (nb*AccLSize))/(nb+1);
				startOff7 = -innerL/2 + lOff7 + AccLSize/2;
			}
		}
		else
		{
			startOff7 = ((nb-1)*(AccLSize+lOff7))/-2;
		}
		
		for(int i=0;i<nb;i++)
		{
			GameObject acc = (GameObject)Instantiate(meshRef);
			acc.name = index.ToString() + "_"+pos+"_"+i;
			acc.transform.parent = _accessories.transform;
			
			float off7 = startOff7 + (i*(AccLSize+lOff7));
			
			Vector3 finalOff7 = new Vector3(0,0,0);
			Vector3 rotation = new Vector3(0,0,0);
			Quaternion finalRotation = new Quaternion(0,0,0,0);
			switch (pos)
			{
			case "N":
				finalOff7 = new Vector3(off7,innerH,innerW/2);
				rotation = new Vector3(0,180,0);
				break;
			case "S":
				finalOff7 = new Vector3(off7,innerH,-innerW/2);
				rotation = new Vector3(0,0,0);
				break;
			case "E":
				finalOff7 = new Vector3(innerL/2,innerH,off7);
				rotation = new Vector3(0,-90,0);
				break;
			case"W":
				finalOff7 = new Vector3(-innerL/2,innerH,off7);
				rotation = new Vector3(0,90,0);
				break;
			}
			finalRotation.eulerAngles = rotation;
			acc.transform.localPosition = Origine + finalOff7;
			acc.transform.localRotation = finalRotation;
		}
		
	}
	
	public void GetUI(/*GUISkin skin*/)
	{		
		bool tmpui = GUILayout.Toggle(_uiShow,accName,GUILayout.Height(50),GUILayout.Width(280));
		if(tmpui != _uiShow)
		{
			_uiShow = tmpui;
			if(_uiShow)
				PergolaAutoFeetEvents.FireToggleUIVisibility(GetType().ToString());
			else
				PergolaAutoFeetEvents.FireToggleUIVisibility("close");
		}
		
		if(_uiShow)
		{
			_paf.GetModuleSelectorUI();
			
			bool hasChange = false;
			
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			_uiN = GUILayout.Toggle(_uiN,"Side N","toggleN",GUILayout.Height(50),GUILayout.Width(50));
			if(_uiN)
			{
				_uiS = _uiE = _uiW = false;
			}
			_uiS = GUILayout.Toggle(_uiS,"Side S","toggleS",GUILayout.Height(50),GUILayout.Width(50));
			if(_uiS)
			{
				_uiN = _uiE = _uiW = false;
			}
			_uiE = GUILayout.Toggle(_uiE,"Side E","toggleE",GUILayout.Height(50),GUILayout.Width(50));
			if(_uiE)
			{
				_uiN = _uiS = _uiW = false;
			}
			_uiW = GUILayout.Toggle(_uiW,"Side W","toggleW",GUILayout.Height(50),GUILayout.Width(50));
			if(_uiW)
			{
				_uiN = _uiS = _uiE = false;
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
			
			if(_uiN)
			{
				hasChange |= SingleUI(ref _uiAPNb.N,"N");
			}
			
			else if(_uiS)
			{
				hasChange |= SingleUI(ref _uiAPNb.S,"S");
			}
			
			else if(_uiE)
			{
				hasChange |= SingleUI(ref _uiAPNb.E,"E");
			}
			
			else if(_uiW)
			{
				hasChange |= SingleUI(ref _uiAPNb.W,"W");
			}
			
			if(hasChange)
			{
				if(_accList.ContainsKey(_selectedIndex))
					_accList[_selectedIndex] = _uiAPNb;
				else
					_accList.Add(_selectedIndex,_uiAPNb);
				
				PergolaAutoFeetEvents.FireRebuild();
			}
		}		
	}
	
	private bool SingleUI(ref int val,string tag)
	{
		bool b = false;
		
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		if(GUILayout.Button("","btn-",GUILayout.Width(50),GUILayout.Height(50)))
		{
			val --;
			val = Mathf.Clamp(val,0,nbMax);
			b = true;
		}
		
		GUILayout.Label(val.ToString(),GUILayout.Width(50),GUILayout.Height(50));
		
		if(GUILayout.Button("","btn+",GUILayout.Width(50),GUILayout.Height(50)))
		{
			val ++;
			val = Mathf.Clamp(val,0,nbMax);
			
			float innerW = _paf.GetLocalW() - _paf.GetFootSize();
			float innerL = _paf.GetLocalL() - _paf.GetFootSize();
			
			if(lOff7Auto)
				lOff7 = 0;
			
			if((val*AccLSize + (val-1)*lOff7)>innerW && (tag == "W" || tag == "E"))
			{
				val --;
			}
			if((val*AccLSize + (val-1)*lOff7)>innerL && (tag == "N" || tag == "S"))
			{
				val --;
			}
			
			b = true;
		}
		
//		GUILayout.Label(TextManager.GetText("Pergola.Side")+"\n"+tag,GUILayout.Width(60),GUILayout.Height(50));
		
		GUILayout.Space(30+10);
		GUILayout.EndHorizontal();
		
		return b;
	}
	
	public void Clear()
	{
		foreach(Transform t in _accessories.transform)
			Destroy(t.gameObject);
	}
	
	private void UpdateSelected(int i)
	{
		_selectedIndex = i;
		if(_accList.ContainsKey(_selectedIndex))
			_uiAPNb = _accList[_selectedIndex];
		else
			_uiAPNb = new AccPosNb();
	}
	
	public void ToggleUI(string s)
	{
		if(s != GetType().ToString())
			_uiShow = false;
	}
	
	public void SaveOption(BinaryWriter buf)
	{
		buf.Write(_accList.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_accList.ContainsKey(i))
			{
				buf.Write(i);
				buf.Write(_accList[i].N);
				buf.Write(_accList[i].S);
				buf.Write(_accList[i].E);
				buf.Write(_accList[i].W);
			}
		}
	}
	
	public void LoadOption(BinaryReader buf)
	{
		_accList.Clear();
		
		int nbEntries = buf.ReadInt32();
		
		for(int i=0;i<nbEntries;i++)
		{
			int key = buf.ReadInt32();
			
			AccPosNb tmpVal;
			
			tmpVal.N = buf.ReadInt32();
			tmpVal.S = buf.ReadInt32();
			tmpVal.E = buf.ReadInt32();
			tmpVal.W = buf.ReadInt32();
			if(!_accList.ContainsKey(key))
			{
				_accList.Add(key,tmpVal);
			}
		}
	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_accList.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_accList.ContainsKey(i))
			{
				al.Add(i);
				al.Add(_accList[i].N);
				al.Add(_accList[i].S);
				al.Add(_accList[i].E);
				al.Add(_accList[i].W);
			}
		}
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		_accList.Clear();
		
		int nbEntries = (int)al[0];
		
		int off7 = 1;
		
		for(int i=0;i<nbEntries;i++)
		{
			int key = (int)al[off7];
			off7++;
			
			AccPosNb tmpVal;
			
			tmpVal.N = (int)al[off7];
			off7++;
			tmpVal.S = (int)al[off7];
			off7++;
			tmpVal.E = (int)al[off7];
			off7++;
			tmpVal.W = (int)al[off7];
			off7++;
			
			_accList.Add(key,tmpVal);
		}
	}
	
}
