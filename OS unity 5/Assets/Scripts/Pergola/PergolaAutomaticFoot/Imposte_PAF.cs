using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Imposte_PAF : MonoBehaviour,IPergolaAutoFeet
{
	
	public GameObject meshRef;					//Objet réference a instancier
	
	private Material	_imposteMaterial;
	
	private float 		_imposteWidth=0.06f;
	private float 		_imposteDemiMontant=0.025f;//taille des demi montants pour les extrémités gauche et droite
	private float 		_decalBorder=0.005f;//décalage pour marquer la limitte entre les poteaux et l'imposte
	// Espacement maximum entre deux montant verticaux
	public float 		_deltaWidth = 1.250f;
	
	//------------------------------------------------------
	
	private FunctionConf_PergolaAutoFeet _paf;		//script parent
	
	private GameObject _impostes;					//GameObject Parent
	
	private bool _uiShow = false;				//Affichage de l UI?
	private bool _canHaveImposteN = true;		//peut avoir des screens coté N? (ex. patio > non)
	private bool _canHaveImposteS = true;		//peut avoir des screens coté S? (ex. patio > non)
	private bool _canHaveImposteE = true;		//peut avoir des screens coté E? (ex. patio > non)
	private bool _canHaveImposteW = true;		//peut avoir des screens coté W? (ex. patio > non)
	private bool _applyToAll = false;
	
	private int _selectedIndex = 0;				//numero module sélectionné
	
	private struct imposteData					//Structure de donnée du screen
	{
		public bool isActive;					//Si ya un Imposte ou pas
	}
	
	private Dictionary<string,imposteData> _impostesData = new Dictionary<string, imposteData>();//Liste des impostes avec leur datas
	private Dictionary<int,Vector4> _nsewTypes = new Dictionary<int, Vector4>(); //type de bloc par numéro de bloc
	
	private const string _sideN = "_N";				//Coté Nord/Front
	private const string _sideS = "_S";				//Coté Sud/Back
	private const string _sideE = "_E";				//Coté Est/Right
	private const string _sideW = "_W";				//Coté West/Left
	
	private bool _uiN = false;						//affichage ui reglage imposte N
	private bool _uiS = false;						//affichage ui reglage imposte S
	private bool _uiE = false;						//affichage ui reglage imposte E
	private bool _uiW = false;						//affichage ui reglage imposte W

	//----------vv Functions vv-----------------------------
	
	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{
		_paf = origin;		
		
		if(transform.FindChild("impostes"))
			_impostes = transform.FindChild("impostes").gameObject;
		else
		{
			_impostes = new GameObject("impostes");
			_impostes.transform.parent = parent;
			_impostes.transform.localPosition = Vector3.zero;
			_impostes.AddComponent<MeshRenderer>();
		}
		
		
		if(parent!=null)
		{
			/*Transform frame = parent.transform.FindChild("frame");
			if(frame!=null)
			{
				_impostes.renderer.material = frame.renderer.material;
			}*/
			Transform imposteObj = parent.transform.FindChild("imposte");
			if(imposteObj!=null)
			{			
				if(imposteObj.GetComponent<Renderer>())
				{
					Transform frame = transform.FindChild("frame");
					if(frame!=null)
					{
						imposteObj.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
					}			
				}
			}
			Transform leftObj = parent.transform.FindChild("left");
			if(leftObj!=null)
			{			
				if(leftObj.GetComponent<Renderer>())
				{
					Transform frame = transform.FindChild("frame");
					if(frame!=null)
					{
						leftObj.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
					}			
				}
			}
			Transform rightObj = parent.transform.FindChild("right");
			if(rightObj!=null)
			{			
				if(rightObj.GetComponent<Renderer>())
				{
					Transform frame = transform.FindChild("frame");
					if(frame!=null)
					{
						rightObj.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
					}			
				}
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
		
		float imposteL = _paf.GetLocalL();
		float imposteW = _paf.GetLocalW();
		float imposteH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
		float halffootSize = footSize/2;
		
		if(nsew.x == 1 && _canHaveImposteN)//Front = North
		{
			BuildSingleImposte(origine,index,_sideN);
		}	
		if(nsew.y == 1 && _canHaveImposteS)//Back = South
		{
			BuildSingleImposte(origine,index,_sideS);
		}
		
		if(nsew.w == 1 && _canHaveImposteW)//Left = West
		{
			BuildSingleImposte(origine,index,_sideW);
		}
		
		if(nsew.z == 1 && _canHaveImposteE)//Right = East
		{
			BuildSingleImposte(origine,index,_sideE);
		}
	}
	
	private void BuildSingleImposte(Vector3 origine,int index,string side)
	{		
		float imposteL = _paf.GetLocalL();
		float imposteW = _paf.GetLocalW();
		float imposteH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
	//	float doubleFootSize = footSize*2;
		float halfFootSize = footSize/8;
		
		//Get Screen data
		imposteData impD;
		string tag = index.ToString()+side;
		
		if(_impostesData.ContainsKey(tag))
		{
			impD = _impostesData[tag];
			if(!impD.isActive)
				return;
		}
		else
		{
			impD.isActive = false;
			
			_impostesData[tag] = impD;
			return;
		}
		
		//Instantiate
		GameObject scr = (GameObject)Instantiate(meshRef);
		scr.transform.parent = _impostes.transform;
		scr.name = index.ToString()+side;
		//_imposteWidth = scr.transform.renderer.bounds.extents.z;
		

		Transform imposteObj = scr.transform.FindChild("imposte");
		if(imposteObj!=null)
		{			
			_imposteWidth = imposteObj.transform.GetComponent<Renderer>().bounds.extents.z;
			if(imposteObj.GetComponent<Renderer>())
			{
				Transform frame = transform.FindChild("frame");
				if(frame!=null)
				{
					imposteObj.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
				}			
			}
		}
		Transform leftObj = scr.transform.FindChild("left");
		if(leftObj!=null)
		{			
			if(leftObj.GetComponent<Renderer>())
			{
				Transform frame = transform.FindChild("frame");
				if(frame!=null)
				{
					leftObj.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
				}			
			}
		}
		Transform rightObj = scr.transform.FindChild("right");
		if(rightObj!=null)
		{			
			if(rightObj.GetComponent<Renderer>())
			{
				Transform frame = transform.FindChild("frame");
				if(frame!=null)
				{
					rightObj.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
				}			
			}
		}
		
		float h = (_paf.GetH() - _paf.GetFrameSizeH());
		float sizeH = 1.0f;//sd.opening * h;
		float posH = h/*-(sizeH/2)*/;
		
		switch (side)
		{
		case _sideN:
			float combineFeet = _nsewTypes[index][2]+_nsewTypes[index][3];
			float decalFeet = (_nsewTypes[index][2]-_nsewTypes[index][3])*footSize/2.0f;
			float width = imposteL-combineFeet*footSize-2*_imposteDemiMontant;
			float decalEmptyFoot = 0;
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lS)
			{
				if(_nsewTypes[index][2]==1)
				{
					width += footSize;
					decalEmptyFoot=-footSize/2;
				}
			}
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.muralWidth)
			{
				if(_nsewTypes[index][3]==1)
				{
					width += footSize;
					decalEmptyFoot=footSize/2;
				}
			}
			if( width>_deltaWidth)
			{
				int NbImposte = Mathf.CeilToInt(width/_deltaWidth);
				float singleWidth  = width/(float)NbImposte;
				Vector3 scaleN = new Vector3(singleWidth,sizeH,1);
				scr.transform.localScale = scaleN;
				List<GameObject> imposteList = new List<GameObject> ();
				imposteList.Add(scr);
				for (int i=0; i<NbImposte-1;i++)
				{
					GameObject duplicateImposte = (GameObject)Instantiate(scr);
					duplicateImposte.transform.parent = _impostes.transform;
					duplicateImposte.transform.localScale = scaleN;
					imposteList.Add(duplicateImposte);
				}
				int imposteNumero=0;
				foreach(GameObject singleImposte in imposteList)
				{
					if(imposteNumero!=imposteList.Count-1)
					{
						Transform left = singleImposte.transform.FindChild("left");
						if(left!=null)
						{
							left.GetComponent<Renderer>().enabled = false;
						}
					}
					if(imposteNumero!=0)
					{						
						Transform left = singleImposte.transform.FindChild("left");
						if(left!=null)
						{
							left.GetComponent<Renderer>().enabled = false;
						}
					}
					singleImposte.transform.localPosition = origine + new Vector3(
					singleWidth*(imposteNumero+0.5f)-width/2.0f-decalFeet-decalEmptyFoot,
						posH,
						imposteW/2.0f-_imposteWidth-_decalBorder);
					singleImposte.transform.Rotate(new Vector3(0,0,0));
					imposteNumero++;
				}
				
			}
			else
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(0,posH,imposteW/2-halfFootSize-_imposteWidth);
				scr.transform.Rotate(new Vector3(0,0,0));
			}				
			break;
		case _sideS:			
			combineFeet = _nsewTypes[index][2]+_nsewTypes[index][3];
			decalFeet = (_nsewTypes[index][2]-_nsewTypes[index][3])*footSize/2.0f;
			width = imposteL-combineFeet*footSize-2*_imposteDemiMontant;
			decalEmptyFoot = 0;
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lN)
			{
				if(_nsewTypes[index][2]==1)
				{
					width += footSize;
					decalEmptyFoot=-footSize/2;
				}
			}
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.muralWidth)
			{
				if(_nsewTypes[index][3]==1)
				{
					width += footSize;
					decalEmptyFoot=footSize/2;
				}
			}
			if( width>_deltaWidth)
			{
				int NbImposte = Mathf.CeilToInt(width/_deltaWidth);
				float singleWidth  = width/(float)NbImposte;
				Vector3 scaleN = new Vector3(singleWidth,sizeH,1);
				scr.transform.localScale = scaleN;
				List<GameObject> imposteList = new List<GameObject> ();
				imposteList.Add(scr);
				for (int i=0; i<NbImposte-1;i++)
				{
					GameObject duplicateImposte = (GameObject)Instantiate(scr);
					duplicateImposte.transform.parent = _impostes.transform;
					duplicateImposte.transform.localScale = scaleN;
					imposteList.Add(duplicateImposte);
				}
				int imposteNumero=0;
				foreach(GameObject singleImposte in imposteList)
				{
					if(imposteNumero!=imposteList.Count-1)
					{
						Transform right = singleImposte.transform.FindChild("right");
						if(right!=null)
						{
							right.GetComponent<Renderer>().enabled = false;
						}
					}
					if(imposteNumero!=0)
					{						
						Transform right = singleImposte.transform.FindChild("right");
						if(right!=null)
						{
							right.GetComponent<Renderer>().enabled = false;
						}
					}
					singleImposte.transform.localPosition = origine + new Vector3(
					singleWidth*(imposteNumero+0.5f)-width/2.0f-decalFeet-decalEmptyFoot,
						posH,
						-(imposteW/2-_imposteWidth-_decalBorder));
					singleImposte.transform.Rotate(new Vector3(0,180,0));
					imposteNumero++;
				}
				
			}
			else
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(0,posH,-(imposteW/2-halfFootSize-_imposteWidth));
				scr.transform.Rotate(new Vector3(0,180,0));
			}				
			
			break;
		case _sideE:					
			combineFeet = _nsewTypes[index][0]+_nsewTypes[index][1];
			decalFeet = (_nsewTypes[index][0]-_nsewTypes[index][1])*footSize/2.0f;
			width = imposteW-combineFeet*footSize-2*_imposteDemiMontant;
			if( width>_deltaWidth)
			{
				int NbImposte = Mathf.CeilToInt(width/_deltaWidth);
				float singleWidth  = width/(float)NbImposte;
				Vector3 scaleN = new Vector3(singleWidth,sizeH,1);
				scr.transform.localScale = scaleN;
				List<GameObject> imposteList = new List<GameObject> ();
				imposteList.Add(scr);
				for (int i=0; i<NbImposte-1;i++)
				{
					GameObject duplicateImposte = (GameObject)Instantiate(scr);
					duplicateImposte.transform.parent = _impostes.transform;
					duplicateImposte.transform.localScale = scaleN;
					imposteList.Add(duplicateImposte);
				}
				int imposteNumero=0;
				foreach(GameObject singleImposte in imposteList)
				{
					if(imposteNumero!=imposteList.Count-1)
					{
						Transform right = singleImposte.transform.FindChild("right");
						if(right!=null)
						{
							right.GetComponent<Renderer>().enabled = false;
						}
					}
					if(imposteNumero!=0)
					{						
						Transform right = singleImposte.transform.FindChild("right");
						if(right!=null)
						{
							right.GetComponent<Renderer>().enabled = false;
						}
					}
					singleImposte.transform.localPosition = origine + new Vector3(
						imposteL/2-_imposteWidth-_decalBorder,
						posH,
						singleWidth*(imposteNumero+0.5f)-width/2.0f-decalFeet);
					singleImposte.transform.Rotate(new Vector3(0,90,0));
					
					imposteNumero++;
				}
				
			}
			else
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(imposteL/2-halfFootSize-_imposteWidth,posH,0);
				scr.transform.Rotate(new Vector3(0,90,0));
			}				
			
			break;
		case _sideW:				
			combineFeet = _nsewTypes[index][0]+_nsewTypes[index][1];
			decalFeet = (_nsewTypes[index][0]-_nsewTypes[index][1])*footSize/2.0f;
			width = imposteW-combineFeet*footSize-2*_imposteDemiMontant;
			decalEmptyFoot = 0;
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lS)
			{
				if(_nsewTypes[index][1]==1)
				{
					width += footSize;
					decalEmptyFoot=footSize/2;
				}
			}
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lN)
			{
				if(_nsewTypes[index][0]==1)
				{
					width += footSize;
					decalEmptyFoot=-footSize/2;
				}
			}
			if( width>_deltaWidth)
			{
				int NbImposte = Mathf.CeilToInt(width/_deltaWidth);
				float singleWidth  = width/(float)NbImposte;
				Vector3 scaleN = new Vector3(singleWidth,sizeH,1);
				scr.transform.localScale = scaleN;
				List<GameObject> imposteList = new List<GameObject> ();
				imposteList.Add(scr);
				for (int i=0; i<NbImposte-1;i++)
				{
					GameObject duplicateImposte = (GameObject)Instantiate(scr);
					duplicateImposte.transform.parent = _impostes.transform;
					duplicateImposte.transform.localScale = scaleN;
					imposteList.Add(duplicateImposte);
				}
				int imposteNumero=0;
				foreach(GameObject singleImposte in imposteList)
				{
					if(imposteNumero!=imposteList.Count-1)
					{
						Transform left = singleImposte.transform.FindChild("left");
						if(left!=null)
						{
							left.GetComponent<Renderer>().enabled = false;
						}
					}
					if(imposteNumero!=0)
					{						
						Transform left = singleImposte.transform.FindChild("left");
						if(left!=null)
						{
							left.GetComponent<Renderer>().enabled = false;
						}
					}
					singleImposte.transform.localPosition = origine + new Vector3(
						-imposteL/2/*+halfFootSize*/+_imposteWidth+_decalBorder,
						posH,
						singleWidth*(imposteNumero+0.5f)-width/2.0f-decalFeet-decalEmptyFoot);
					singleImposte.transform.Rotate(new Vector3(0,270,0));
					
					imposteNumero++;
				}
				
			}
			else
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(-imposteL/2+halfFootSize+_imposteWidth,posH,0);
				scr.transform.Rotate(new Vector3(0,270,0));
			}
			
			break;
		}
				
	}
	
	public void GetUI(/*GUISkin skin*/)
	{		
		bool tmpui = GUILayout.Toggle(_uiShow,TextManager.GetText("Pergola.Imposte"),GUILayout.Height(50),GUILayout.Width(280));
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
			if(_impostesData.ContainsKey(_selectedIndex.ToString()+_sideN) && _nsewTypes[_selectedIndex].x == 1 && _canHaveImposteN)
			{
				_uiN = GUILayout.Toggle(_uiN,"Side N","toggleN",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiN)
				{
					if(!_impostesData[_selectedIndex.ToString()+_sideN].isActive)
					{
						imposteData impD = _impostesData[_selectedIndex.ToString()+_sideN];
						impD.isActive = true;
						_impostesData[_selectedIndex.ToString()+_sideN] = impD;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiE = _uiS = _uiW = false;
				}
			}
			if(_impostesData.ContainsKey(_selectedIndex.ToString()+_sideS) && _nsewTypes[_selectedIndex].y == 1 && _canHaveImposteS)
			{
				_uiS = GUILayout.Toggle(_uiS,"Side S","toggleS",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiS)
				{
					if(!_impostesData[_selectedIndex.ToString()+_sideS].isActive)
					{
						imposteData impD = _impostesData[_selectedIndex.ToString()+_sideS];
						impD.isActive = true;
						_impostesData[_selectedIndex.ToString()+_sideS] = impD;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiE = _uiN = _uiW = false;
				}
			}
			if(_impostesData.ContainsKey(_selectedIndex.ToString()+_sideE) && _nsewTypes[_selectedIndex].z == 1 && _canHaveImposteE)
			{
				_uiE = GUILayout.Toggle(_uiE,"Side E","toggleE",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiE)
				{
					if(!_impostesData[_selectedIndex.ToString()+_sideE].isActive)
					{
						imposteData impD = _impostesData[_selectedIndex.ToString()+_sideE];
						impD.isActive = true;
						_impostesData[_selectedIndex.ToString()+_sideE] = impD;
						PergolaAutoFeetEvents.FireRebuild();
					}
					
					_uiN = _uiS = _uiW = false;
				}
			}
			if(_impostesData.ContainsKey(_selectedIndex.ToString()+_sideW) && _nsewTypes[_selectedIndex].w == 1 && _canHaveImposteW)
			{
				_uiW = GUILayout.Toggle(_uiW,"Side W","toggleW",GUILayout.Height(50),GUILayout.Width(50));
				if(_uiW)
				{
					if(!_impostesData[_selectedIndex.ToString()+_sideW].isActive)
					{
						imposteData impD = _impostesData[_selectedIndex.ToString()+_sideW];
						impD.isActive = true;
						_impostesData[_selectedIndex.ToString()+_sideW] = impD;
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
		foreach(Transform t in _impostes.transform)
			Destroy(t.gameObject);
	}
	
	private void CleanData()
	{
		int max = _paf.GetNbL()*_paf.GetNbW();
		ArrayList deleteList = new ArrayList();
		
		foreach(string s in _impostesData.Keys)
		{
			int index = int.Parse(s.Split('_')[0]);
			if(index>=max)
				deleteList.Add(s);
		}
		
		foreach(string del in deleteList)
		{
			_impostesData.Remove(del);	
		}
		
		_nsewTypes.Clear();
	}
	
	//---------------------------------
	
	private void SingleUI(string tag)
	{
		imposteData impD = _impostesData[_selectedIndex.ToString()+tag];
		
		string uiStr = TextManager.GetText((impD.isActive)? "Cacher":"Afficher" );
		
		bool tmpActiv = GUILayout.Toggle(impD.isActive,uiStr,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
		if(tmpActiv != impD.isActive)
		{
			if(!tmpActiv)
				_uiN = _uiE = _uiS = _uiW = false;
			impD.isActive = tmpActiv;
			_impostesData[_selectedIndex.ToString()+tag] = impD;
			PergolaAutoFeetEvents.FireRebuild();
		}
		
		
		
	}

	//---------------------------------------------------------
	
	private void UpdateSelected(int i) // Mise a jour du bloc sélectionné
	{
		_selectedIndex = i;
		_uiN = _uiS = _uiE = _uiW = false;
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
			_canHaveImposteN = true;
			_canHaveImposteS = true;
			_canHaveImposteE = true;
			_canHaveImposteW = true;
			break;
		case "muralLength":
			_canHaveImposteN = false;
			_canHaveImposteS = true;
			_canHaveImposteE = true;
			_canHaveImposteW = true;
			break;
		case "muralWidth":
			_canHaveImposteN = true;
			_canHaveImposteS = true;
			_canHaveImposteE = true;
			_canHaveImposteW = false;
			break;
		case "lN":
			_canHaveImposteN = false;
			_canHaveImposteS = true;
			_canHaveImposteE = false;
			_canHaveImposteW = true;
			break;
		case "lS":
			_canHaveImposteN = true;
			_canHaveImposteS = false;
			_canHaveImposteE = false;
			_canHaveImposteW = true;
			break;
		case "patio":
			_canHaveImposteN = false;
			_canHaveImposteS = false;
			_canHaveImposteE = false;
			_canHaveImposteW = false;
			break;
		}
	}
	
	public void SaveOption(BinaryWriter buf)
	{		
//		buf.Write(_needExtension);
		buf.Write(_impostesData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_impostesData.ContainsKey(i+_sideN))
			{
				buf.Write((string)(i+_sideN));					//Key
				buf.Write(_impostesData[i+_sideN].isActive);
			}
			
			if(_impostesData.ContainsKey(i+_sideS))
			{
				buf.Write((string)(i+_sideS));
				buf.Write(_impostesData[i+_sideS].isActive);
			}
			
			if(_impostesData.ContainsKey(i+_sideE))
			{
				buf.Write((string)(i+_sideE));
				buf.Write(_impostesData[i+_sideE].isActive);
			}
			
			if(_impostesData.ContainsKey(i+_sideW))
			{
				buf.Write((string)(i+_sideW));
				buf.Write(_impostesData[i+_sideW].isActive);
			}
		}
	}
	
	public void LoadOption(BinaryReader buf)
	{
		int entryNb = buf.ReadInt32();
		_impostesData.Clear();
		
		for(int i=0;i<entryNb;i++)
		{
			string key = buf.ReadString();
			
			imposteData impD;
			
			impD.isActive = buf.ReadBoolean();
			if(!_impostesData.ContainsKey(key)){
				_impostesData.Add(key,impD);
			}
		}
	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_impostesData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_impostesData.ContainsKey(i+_sideN))
			{
				al.Add((string)(i+_sideN));					//Key
				al.Add(_impostesData[i+_sideN].isActive);
			}
			
			if(_impostesData.ContainsKey(i+_sideS))
			{
				al.Add((string)(i+_sideS));
				al.Add(_impostesData[i+_sideS].isActive);
			}
			
			if(_impostesData.ContainsKey(i+_sideE))
			{
				al.Add((string)(i+_sideE));
				al.Add(_impostesData[i+_sideE].isActive);
			}
			
			if(_impostesData.ContainsKey(i+_sideW))
			{
				al.Add((string)(i+_sideW));
				al.Add(_impostesData[i+_sideW].isActive);
			}
		}
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		int entryNb = (int)al[0];
		
		_impostesData.Clear();
		
		int off7 = 1;
		for(int i=0;i<entryNb;i++)
		{
			string key = (string)al[off7];
			off7 ++;
			
			imposteData impD;

			impD.isActive = (bool)al[off7];
			off7 ++;
			
			_impostesData.Add(key,impD);
		}
	}
	
}
