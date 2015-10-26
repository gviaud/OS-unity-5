using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Claustra_PAF : MonoBehaviour,IPergolaAutoFeet
{
	
	public GameObject meshRef;					//Objet réference a instancier
		
	private Material	_claustraMaterial;
	
	private float 		_claustraWidth=0.06f;
	private float 		_decalBorder=0.005f;//décalage pour marquer la limitte entre les poteaux et l'imposte
	private float 		_claustraHeight=2.539085f;
	private float 		_montantSize=0.05f;//taille des montants verticaux
	private float 		_deltaMax=0.150f;//espacement maximal entre les poteaux verticaux de la pergola et les poteaux verticaux du claustra
	// Espacement maximum entre deux montant verticaux
	//private float 		_deltaWidth = 1.250f;
	private float 		_deltaWidth = 1.100f;	
	private float 		_defaultHeight = 2.6f;	
	private float 		_minWidth = 1.800f;
	private float 		_secondMinWidth = 2.800f;
	private float 		_maxWidth = 3.700f;
	
	
	private string _uiWidth = "1800";
	
	
	//------------------------------------------------------
	
	private FunctionConf_PergolaAutoFeet _paf;		//script parent
	
	private GameObject _claustras;					//GameObject Parent
	private GameObject _cubeMask;					//Mask qui cache le bas des claustras
	
	private bool _uiShow = false;				//Affichage de l UI?
	private bool _canHaveClaustraN = true;		//peut avoir des screens coté N? (ex. patio > non)
	private bool _canHaveClaustraS = true;		//peut avoir des screens coté S? (ex. patio > non)
	private bool _canHaveClaustraE = true;		//peut avoir des screens coté E? (ex. patio > non)
	private bool _canHaveClaustraW = true;		//peut avoir des screens coté W? (ex. patio > non)
	private bool _applyToAll = false;
	
	private int _selectedIndex = 0;				//numero module sélectionné
	
	private struct claustraData					//Structure de donnée du screen
	{
		public bool		isActive;					//Si ya un Claustra ou pas
		public bool 	partialWidth;				//Si le claustra ne fait pas toute la largeur
		public bool 	fromLeft;					//Si le claustra est partiel en partant de gauche
		public float	width;					//taille du claustra partiel
	}
	
	private Dictionary<string,claustraData> _claustrasData = new Dictionary<string, claustraData>();//Liste des claustras avec leur datas
	private Dictionary<int,Vector4> _nsewTypes = new Dictionary<int, Vector4>(); //type de bloc par numéro de bloc
	private Dictionary<int,Vector3> _pos = new Dictionary<int, Vector3>(); //position par numéro de bloc
	
	private const string _sideN = "_N";				//Coté Nord/Front
	private const string _sideS = "_S";				//Coté Sud/Back
	private const string _sideE = "_E";				//Coté Est/Right
	private const string _sideW = "_W";				//Coté West/Left
	
	private bool _uiN 		= false;			//affichage ui reglage claustra N
	private bool _uiS 		= false;			//affichage ui reglage claustra S
	private bool _uiE 		= false;			//affichage ui reglage claustra E
	private bool _uiW 		= false;			//affichage ui reglage claustra W
	private bool _uiGeneral = true;			
	
	private float _timer;
	private const float _refresh = 0.15f;
	
		
	//----------vv Functions vv-----------------------------
	
	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{
		_paf = origin;		
		
		if(transform.FindChild("claustras"))
			_claustras = transform.FindChild("claustras").gameObject;
		else
		{
			_claustras = new GameObject("claustras");
			_claustras.transform.parent = parent;
			_claustras.transform.localPosition = Vector3.zero;
			_claustras.AddComponent<MeshRenderer>();
		}
		if(parent!=null)
		{
			Transform frame = parent.transform.FindChild("frame");
			if(frame!=null)
			{
				_claustras.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
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
	
	private void UpdateVisibility()
	{
		UpdateType(_paf._type.ToString());
	}
	public void Build(Vector3 origine,int index,Vector4 nsew)
	{
		UpdateVisibility();
		if(index == 0)
		{
			CleanData();//Clear le _nsewTypes
		}
		
		_nsewTypes.Add(index,nsew);
		_pos.Add(index,origine);
		
		float claustraL = _paf.GetLocalL();
		float claustraW = _paf.GetLocalW();
		float claustraH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
		float halffootSize = footSize/2;
		
		if(nsew.x == 1 && _canHaveClaustraN)//Front = North
		{
			BuildSingleClaustra(origine,index,_sideN);
		}	
		if(nsew.y == 1 && _canHaveClaustraS)//Back = South
		{
			BuildSingleClaustra(origine,index,_sideS);
		}
		
		if(nsew.w == 1 && _canHaveClaustraW)//Left = West
		{
			BuildSingleClaustra(origine,index,_sideW);
		}
		
		if(nsew.z == 1 && _canHaveClaustraE)//Right = East
		{
			BuildSingleClaustra(origine,index,_sideE);
		}
	}
	
	private void BuildSingleClaustra(Vector3 origine,int index,string side)
	{		
		float claustraL = _paf.GetLocalL();
		float claustraW = _paf.GetLocalW();
		float claustraH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
		float halfFootSize = footSize/8;
		
		//Get Screen data
		claustraData claustraD;
		string tag = index.ToString()+side;
		
		if(_claustrasData.ContainsKey(tag))
		{
			claustraD = _claustrasData[tag];
			if(!claustraD.isActive)
				return;
		}
		else
		{
			claustraD.isActive = false;
			claustraD.partialWidth = true/*false*/;
			claustraD.fromLeft = true;	
			claustraD.width = _minWidth;			
			_claustrasData[tag] = claustraD;
			return;
		}
		
		//Instantiate
		GameObject scr = (GameObject)Instantiate(meshRef);
		scr.transform.parent = _claustras.transform;
		scr.name = index.ToString()+side;
		
		Transform claustraObj = scr.transform.FindChild("claustra");
		if(claustraObj!=null)
		{
			_claustraWidth = claustraObj.transform.GetComponent<Renderer>().bounds.extents.z;
			_claustraHeight = claustraObj.transform.GetComponent<Renderer>().bounds.extents.y;		
		
			if(claustraObj.GetComponent<Renderer>())
			{
				Transform frame = transform.FindChild("frame");
				if(frame!=null)
				{
					claustraObj.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
				}			
			}
		}
		
		float h = (_paf.GetH() - _paf.GetFrameSizeH());
		float sizeH = h/*/(_claustraHeight*2.0f)*/;
		float posH = h;
		
		switch (side)
		{
		case _sideN:
			if(_minWidth>claustraL)
			{
				Destroy(scr);
				break;
			}
			if(claustraD.width>claustraL)
			{
				claustraD.width=claustraL;
				if((claustraD.width>_minWidth)&&(claustraD.width<_secondMinWidth))
				{
					claustraD.width=_minWidth;
				}
				if(claustraD.width<_minWidth)
				{
					claustraD.width = _minWidth;
				}
				_claustrasData[tag]=claustraD;
			}
			float combineFeet = _nsewTypes[index][2]+_nsewTypes[index][3];
			float decalFeet = (_nsewTypes[index][2]-_nsewTypes[index][3])*footSize/2.0f;
			float width = claustraL-combineFeet*footSize;						
			float decalEmptyFoot = 0;
			float newWidth = claustraD.width-combineFeet*footSize;
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lS)
			{
				if(_nsewTypes[index][2]==1)
				{
					width += footSize;
					newWidth+= footSize;
					decalEmptyFoot=-footSize/2;
				}
			}
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.muralWidth)
			{
				if(_nsewTypes[index][3]==1)
				{
					width += footSize;
					newWidth+= footSize;
					decalEmptyFoot=footSize/2;
				}
			}
			if(claustraD.partialWidth)
			{
				//float newWidth = claustraD.width-combineFeet*footSize;
				if(claustraD.fromLeft)
				{
					decalEmptyFoot-=(width-newWidth)/2.0f;
				}
				else
				{
					decalEmptyFoot+=(width-newWidth)/2.0f;				
				}
				width = newWidth;
			}
			if( width>_deltaWidth)
			{
				float deltaTemp=_deltaWidth;
				if(width<=(_deltaWidth+2*_deltaMax))
				{
					deltaTemp=width-2*_deltaMax-0.01f;
				}
				//int NbMontant = Mathf.FloorToInt((width-2*_deltaMax)/deltaTemp)+1;	
				int NbMontant = 2;
				if(claustraD.width>=_secondMinWidth)
				{
					NbMontant = 3;
				}
				float delta = (width-((NbMontant-1)*deltaTemp))/2.0f;				
				Vector3 scaleN = new Vector3(_montantSize,h,_montantSize);
				for (int i=0; i<NbMontant;i++)
				{
					GameObject montant = GameObject.CreatePrimitive(PrimitiveType.Cube);
					montant.name = index.ToString()+side+"vertMontant";
					Transform frame = transform.FindChild("frame");
					if(frame!=null)
					{
						montant.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
					}			
					montant.transform.parent = _claustras.transform;
					montant.transform.localScale = scaleN;
					Destroy(montant.GetComponent("BoxCollider"));
					montant.transform.localPosition = origine + new Vector3(
					delta +deltaTemp*i-width/2.0f-decalFeet-decalEmptyFoot,
						posH/2,
						claustraW/2.0f-_claustraWidth-_decalBorder);
				}				
			}
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(0-decalFeet-decalEmptyFoot,posH,claustraW/2-halfFootSize-_claustraWidth);
				scr.transform.Rotate(new Vector3(0,0,0));
				/*Transform frame = transform.FindChild("frame");
				if(frame!=null)
				{
					scr.renderer.material = frame.renderer.material;
				}	*/
			}				
			break;
		case _sideS:	
			if(_minWidth>claustraL)
			{
				Destroy(scr);
				break;
			}
			if(claustraD.width>claustraL)
			{
				claustraD.width=claustraL;
				if((claustraD.width>_minWidth)&&(claustraD.width<_secondMinWidth))
				{
					claustraD.width=_minWidth;
				}
				if(claustraD.width<_minWidth)
				{
					claustraD.width = _minWidth;
				}
				_claustrasData[tag]=claustraD;
			}		
			combineFeet = _nsewTypes[index][2]+_nsewTypes[index][3];
			decalFeet = (_nsewTypes[index][2]-_nsewTypes[index][3])*footSize/2.0f;
			width = claustraL-combineFeet*footSize;
			newWidth = claustraD.width-combineFeet*footSize;
			decalEmptyFoot = 0;
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lN)
			{
				if(_nsewTypes[index][2]==1)
				{
					width += footSize;
					newWidth += footSize;
					decalEmptyFoot=-footSize/2;
				}
			}
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.muralWidth)
			{
				if(_nsewTypes[index][3]==1)
				{
					width += footSize;
					newWidth += footSize;
					decalEmptyFoot=footSize/2;
				}
			}
			if(claustraD.partialWidth)
			{
				//float newWidth = claustraD.width-combineFeet*footSize;
				if(claustraD.fromLeft)
				{
					decalEmptyFoot+=(width-newWidth)/2.0f;
				}
				else
				{
					decalEmptyFoot-=(width-newWidth)/2.0f;				
				}
				width = newWidth;
			}
			if( width>_deltaWidth)
			{
				float deltaTemp=_deltaWidth;
				if(width<=(_deltaWidth+2*_deltaMax))
				{
					deltaTemp=width-2*_deltaMax-0.01f;
				}
				//int NbMontant = Mathf.FloorToInt((width-2*_deltaMax)/deltaTemp)+1;	
				int NbMontant = 2;
				if(claustraD.width>=_secondMinWidth)
				{
					NbMontant = 3;
				}
				float delta = (width-((NbMontant-1)*deltaTemp))/2.0f;				
				Vector3 scaleN = new Vector3(_montantSize,h,_montantSize);
				for (int i=0; i<NbMontant;i++)
				{
					GameObject montant = GameObject.CreatePrimitive(PrimitiveType.Cube);
					montant.name = index.ToString()+side+"vertMontant";
					Transform frame = transform.FindChild("frame");
					if(frame!=null)
					{
						montant.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
					}			
					montant.transform.parent = _claustras.transform;
					montant.transform.localScale = scaleN;
					Destroy(montant.GetComponent("BoxCollider"));
					montant.transform.localPosition = origine + new Vector3(
					delta +deltaTemp*i-width/2.0f-decalFeet-decalEmptyFoot,
						posH/2,
						-(claustraW/2.0f-_claustraWidth-_decalBorder));
				}	
				
			}
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(0-decalFeet-decalEmptyFoot,posH,-(claustraW/2-halfFootSize-_claustraWidth));
				scr.transform.Rotate(new Vector3(0,180,0));
			}				
			
			break;
		case _sideE:	
			if(_minWidth>claustraW)
			{
				Destroy(scr);
				break;
			}
			if(claustraD.width>claustraW)
			{
				claustraD.width=claustraW;
				if((claustraD.width>_minWidth)&&(claustraD.width<_secondMinWidth))
				{
					claustraD.width=_minWidth;
				}
				if(claustraD.width<_minWidth)
				{
					claustraD.width = _minWidth;
				}
				_claustrasData[tag]=claustraD;
			}
			combineFeet = _nsewTypes[index][0]+_nsewTypes[index][1];
			decalFeet = (_nsewTypes[index][0]-_nsewTypes[index][1])*footSize/2.0f;
			width = claustraW-combineFeet*footSize;
			newWidth = claustraD.width-combineFeet*footSize;
			decalEmptyFoot = 0;
			if(claustraD.partialWidth)
			{
			//	float newWidth = claustraD.width-combineFeet*footSize;
				if(claustraD.fromLeft)
				{
					decalEmptyFoot+=(width-newWidth)/2.0f;
					
				}
				else
				{
					decalEmptyFoot-=(width-newWidth)/2.0f;				
				}
				width = newWidth;
			}
			if( width>_deltaWidth)
			{				
				float deltaTemp=_deltaWidth;
				if(width<=(_deltaWidth+2*_deltaMax))
				{
					deltaTemp=width-2*_deltaMax-0.01f;
				}
				//int NbMontant = Mathf.FloorToInt((width-2*_deltaMax)/deltaTemp)+1;		
				int NbMontant = 2;
				if(claustraD.width>=_secondMinWidth)
				{
					NbMontant = 3;
				}
				float delta = (width-((NbMontant-1)*deltaTemp))/2.0f;				
				Vector3 scaleN = new Vector3(_montantSize,h,_montantSize);
				for (int i=0; i<NbMontant;i++)
				{
					GameObject montant = GameObject.CreatePrimitive(PrimitiveType.Cube);
					montant.name = index.ToString()+side+"vertMontant";
					Transform frame = transform.FindChild("frame");
					if(frame!=null)
					{
						montant.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
					}			
					montant.transform.parent = _claustras.transform;
					montant.transform.localScale = scaleN;
					Destroy(montant.GetComponent("BoxCollider"));
					montant.transform.localPosition = origine + new Vector3(
						claustraL/2-_claustraWidth-_decalBorder,
						posH/2,
						delta +deltaTemp*i-width/2.0f-decalFeet-decalEmptyFoot);
				}
				
			}
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(claustraL/2-halfFootSize-_claustraWidth,posH,-decalFeet-decalEmptyFoot);
				scr.transform.Rotate(new Vector3(0,90,0));
			}				
			
			break;
		case _sideW:	
			if(_minWidth>claustraW)
			{
				Destroy(scr);
				break;
			}
			if(claustraD.width>claustraW)
			{
				claustraD.width=claustraW;
				if((claustraD.width>_minWidth)&&(claustraD.width<_secondMinWidth))
				{
					claustraD.width=_minWidth;
				}
				if(claustraD.width<_minWidth)
				{
					claustraD.width = _minWidth;
				}
				_claustrasData[tag]=claustraD;
			}			
			combineFeet = _nsewTypes[index][0]+_nsewTypes[index][1];
			decalFeet = (_nsewTypes[index][0]-_nsewTypes[index][1])*footSize/2.0f;
			width = claustraW-combineFeet*footSize;			
			newWidth = claustraD.width-combineFeet*footSize;
			decalEmptyFoot = 0;
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lS)
			{
				if(_nsewTypes[index][1]==1)
				{
					width += footSize;
					newWidth += footSize;
					decalEmptyFoot=footSize/2;
				}
			}
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lN)
			{
				if(_nsewTypes[index][0]==1)
				{
					width += footSize;
					newWidth += footSize;
					decalEmptyFoot=-footSize/2;
				}
			}
			if(claustraD.partialWidth)
			{
			//	float newWidth = claustraD.width-combineFeet*footSize;
				if(claustraD.fromLeft)
				{
					decalEmptyFoot-=(width-newWidth)/2.0f;
				}
				else
				{
					decalEmptyFoot+=(width-newWidth)/2.0f;				
				}
				width = newWidth;
			}
			if( width>_deltaWidth)
			{						
				float deltaTemp=_deltaWidth;
				if(width<=(_deltaWidth+2*_deltaMax))
				{
					deltaTemp=width-2*_deltaMax-0.01f;
				}
				//int NbMontant = Mathf.FloorToInt((width-2*_deltaMax)/deltaTemp)+1;	
				int NbMontant = 2;
				if(claustraD.width>=_secondMinWidth)
				{
					NbMontant = 3;
				}
				float delta = (width-((NbMontant-1)*deltaTemp))/2.0f;				
				Vector3 scaleN = new Vector3(_montantSize,h,_montantSize);
				for (int i=0; i<NbMontant;i++)
				{
					GameObject montant = GameObject.CreatePrimitive(PrimitiveType.Cube);
					montant.name = index.ToString()+side+"vertMontant";
					Transform frame = transform.FindChild("frame");
					if(frame!=null)
					{
						montant.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
					}			
					montant.transform.parent = _claustras.transform;
					montant.transform.localScale = scaleN;
					Destroy(montant.GetComponent("BoxCollider"));
					montant.transform.localPosition = origine + new Vector3(
						-(claustraL/2-_claustraWidth-_decalBorder)
						,
						posH/2,						
						delta +deltaTemp*i-width/2.0f-decalFeet-decalEmptyFoot);
					//montant.transform.Rotate(new Vector3(0,270,0));
				}
				
			}
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(-claustraL/2+halfFootSize+_claustraWidth,posH,-decalFeet-decalEmptyFoot);
				scr.transform.Rotate(new Vector3(0,270,0));
			}
			
			break;
		}		
	}
	
	public void GetUI(/*GUISkin skin*/)
	{		
		if(_uiGeneral)
		{
			bool tmpui = false;
			try{
				tmpui = GUILayout.Toggle(_uiShow,TextManager.GetText("Pergola.Claustra"),GUILayout.Height(50),GUILayout.Width(280));
			}
			catch(System.ArgumentException e)
			{
				Debug.LogError("Claustra_PAF - GetUI "+e.Message);
			}
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
				if(_claustrasData.ContainsKey(_selectedIndex.ToString()+_sideN) && _nsewTypes[_selectedIndex].x == 1 && _canHaveClaustraN)
				{
					bool _uiNTemp = GUILayout.Toggle(_uiN,"Side N","toggleN",GUILayout.Height(50),GUILayout.Width(50));
					if(_uiNTemp!=_uiN)
					{
						_uiN=_uiNTemp;
						if(_uiN)
						{
							claustraData claustraD = _claustrasData[_selectedIndex.ToString()+_sideN];
							int tmp = Mathf.RoundToInt(claustraD.width*1000f);
							_uiWidth = (Mathf.RoundToInt(tmp)).ToString();
							if(!claustraD.isActive)
							{
								//claustraData claustraD = _claustrasData[_selectedIndex.ToString()+_sideN];
								claustraD.isActive = true;
								_claustrasData[_selectedIndex.ToString()+_sideN] = claustraD;
								PergolaAutoFeetEvents.FireRebuild();
							}
							
							_uiE = _uiS = _uiW = false;
						}
					}
				}
				if(_claustrasData.ContainsKey(_selectedIndex.ToString()+_sideS) && _nsewTypes[_selectedIndex].y == 1 && _canHaveClaustraS)
				{
					bool _uiStemp = GUILayout.Toggle(_uiS,"Side S","toggleS",GUILayout.Height(50),GUILayout.Width(50));
					if(_uiStemp!=_uiS)
					{
						_uiS=_uiStemp;
					
						if(_uiS)
						{
							claustraData claustraD = _claustrasData[_selectedIndex.ToString()+_sideS];
							int tmp = Mathf.RoundToInt(claustraD.width*1000f);
							_uiWidth = (Mathf.RoundToInt(tmp)).ToString();
							if(!claustraD.isActive)
							{
								//claustraData claustraD = _claustrasData[_selectedIndex.ToString()+_sideS];
								claustraD.isActive = true;
								_claustrasData[_selectedIndex.ToString()+_sideS] = claustraD;	
								PergolaAutoFeetEvents.FireRebuild();
							}
							
							_uiE = _uiN = _uiW = false;
						}
					}
				}
				if(_claustrasData.ContainsKey(_selectedIndex.ToString()+_sideE) && _nsewTypes[_selectedIndex].z == 1 && _canHaveClaustraE)
				{
					bool _uiETemp = GUILayout.Toggle(_uiE,"Side E","toggleE",GUILayout.Height(50),GUILayout.Width(50));
					
					if(_uiETemp!=_uiE)
					{
						_uiE=_uiETemp;
						if(_uiE)
						{
							claustraData claustraD = _claustrasData[_selectedIndex.ToString()+_sideE];
							int tmp = Mathf.RoundToInt(claustraD.width*1000f);
							_uiWidth = (Mathf.RoundToInt(tmp)).ToString();
							if(!claustraD.isActive)
							{
								//claustraData claustraD = _claustrasData[_selectedIndex.ToString()+_sideE];
								claustraD.isActive = true;
								_claustrasData[_selectedIndex.ToString()+_sideE] = claustraD;	
								PergolaAutoFeetEvents.FireRebuild();
							}
							
							_uiN = _uiS = _uiW = false;
						}
					}
				}
				if(_claustrasData.ContainsKey(_selectedIndex.ToString()+_sideW) && _nsewTypes[_selectedIndex].w == 1 && _canHaveClaustraW)
				{
					bool _uiWTemp = GUILayout.Toggle(_uiW,"Side W","toggleW",GUILayout.Height(50),GUILayout.Width(50));
					
					if(_uiWTemp!=_uiW)
					{
						_uiW=_uiWTemp;
						if(_uiW)
						{
							claustraData claustraD = _claustrasData[_selectedIndex.ToString()+_sideW];
							int tmp = Mathf.RoundToInt(claustraD.width*1000f);
							_uiWidth = (Mathf.RoundToInt(tmp)).ToString();	
							if(!claustraD.isActive)
							{
								//claustraData claustraD = _claustrasData[_selectedIndex.ToString()+_sideW];
								claustraD.isActive = true;
								_claustrasData[_selectedIndex.ToString()+_sideW] = claustraD;
								PergolaAutoFeetEvents.FireRebuild();
							}	
							
							_uiE = _uiS = _uiN = false;
						}
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
	}
	
	public void Clear()
	{
		foreach(Transform t in _claustras.transform)
			Destroy(t.gameObject);
	}
	
	private void CleanData()
	{
		int max = _paf.GetNbL()*_paf.GetNbW();
		ArrayList deleteList = new ArrayList();
		
		foreach(string s in _claustrasData.Keys)
		{
			int index = int.Parse(s.Split('_')[0]);
			if(index>=max)
				deleteList.Add(s);
		}
		
		foreach(string del in deleteList)
		{
			_claustrasData.Remove(del);	
		}
		
		_nsewTypes.Clear();
		_pos.Clear();
	}
	
	//---------------------------------
	
	private void SingleUI(string tag)
	{
		claustraData claustraD = _claustrasData[_selectedIndex.ToString()+tag];
		
		string uiStr = TextManager.GetText((claustraD.isActive)? "Cacher":"Afficher" );
		
		bool tmpActiv = GUILayout.Toggle(claustraD.isActive,uiStr,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
		if(tmpActiv != claustraD.isActive)
		{
			if(!tmpActiv)
				_uiN = _uiE = _uiS = _uiW = false;
			claustraD.isActive = tmpActiv;
			_claustrasData[_selectedIndex.ToString()+tag] = claustraD;
			PergolaAutoFeetEvents.FireRebuild();
		}
		
		if(claustraD.isActive)
		{
			//reglage ouverture partielle -------------------------------------------
			/*GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			string uiStrPartial = TextManager.GetText((claustraD.partialWidth)? "Pergola.full":"Pergola.partial" );
			bool tmpPartial = GUILayout.Toggle(claustraD.partialWidth,uiStrPartial,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			if(tmpPartial != claustraD.partialWidth)
			{
				claustraD.partialWidth = tmpPartial;
				_claustrasData[_selectedIndex.ToString()+tag] = claustraD;
				if(_applyToAll)
				{
					ApplyToAll(claustraD);
				}
				else
				{
					UpdateClaustraPartialWidth(_selectedIndex,tag);
				}
			}
			GUILayout.EndHorizontal();	*/		
			
			if(claustraD.partialWidth)
			{
				//reglage ouvertue gauche droite -------------------------------------------
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				
				string uiFromLeft = TextManager.GetText((claustraD.fromLeft)? "Pergola.right":"Pergola.left" );
				bool tmpFromLeft = GUILayout.Toggle(claustraD.fromLeft,uiFromLeft,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
				if(tmpFromLeft != claustraD.fromLeft)
				{
					claustraD.fromLeft = tmpFromLeft;
					_claustrasData[_selectedIndex.ToString()+tag] = claustraD;
					if(_applyToAll)
					{
						ApplyToAll(claustraD);
					}
					else
						UpdateClaustraPartialWidth(_selectedIndex,tag);
				}
				GUILayout.EndHorizontal();	
				
				//reglage taille d'ouverture -------------------------------------------
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				float maxWidth = Mathf.Min(_paf.GetLocalL(), _maxWidth);
				if(tag.CompareTo(_sideE)==0 ||tag.CompareTo(_sideW)==0)
				{
					maxWidth = Mathf.Min(_paf.GetLocalW(), _maxWidth);
				}
				if(GUILayout.RepeatButton("-","btn-",GUILayout.Height(50),GUILayout.Width(50)) && Time.time > _timer)
				{
					claustraD.width = claustraD.width - 0.1f;
					if((claustraD.width>_minWidth)&&(claustraD.width<_secondMinWidth))
					{
						claustraD.width=_minWidth;
					}
					if(claustraD.width<_minWidth)
					{
						claustraD.width = _minWidth;
					}	
					if(claustraD.width>maxWidth)
					{
						claustraD.width = maxWidth;
					}
					int tmp = Mathf.RoundToInt(claustraD.width*1000f);
					_uiWidth = (Mathf.RoundToInt(tmp)).ToString();					
					_claustrasData[_selectedIndex.ToString()+tag] = claustraD;
					
					UpdateClaustraPartialWidth(_selectedIndex,tag);
					_timer = Time.time + _refresh;
				}
				GUI.SetNextControlName("sizeClaustra");
				_uiWidth = GUILayout.TextField(_uiWidth,GUILayout.Height(40),GUILayout.Width(75));
		#if UNITY_IPHONE || UNITY_ANDROID
				if(_uiWidth != (Mathf.RoundToInt(claustraD.width*1000)).ToString())
				{
					float newvalue = float.Parse(_uiWidth) / 1000;
		
					if(newvalue<_minWidth)
					{
						newvalue= _minWidth;
						int tmp = Mathf.RoundToInt(newvalue*1000f);
						_uiWidth = (Mathf.RoundToInt(tmp)).ToString();	
					}
					if((newvalue>_minWidth)&&(newvalue<_secondMinWidth))
					{
						newvalue= _minWidth;			
						int tmp = Mathf.RoundToInt(newvalue*1000f);
						_uiWidth = (Mathf.RoundToInt(tmp)).ToString();	
					}
					if(newvalue>maxWidth)
					{
						newvalue = maxWidth;
						int tmp = Mathf.RoundToInt(newvalue*1000f);
						_uiWidth = (Mathf.RoundToInt(tmp)).ToString();	
					}
					claustraD.width = newvalue;
					_claustrasData[_selectedIndex.ToString()+tag] = claustraD;
					
					UpdateClaustraPartialWidth(_selectedIndex,tag);
				}
		#else
			//	Debug.Log("GUI.GetNameOfFocusedControl() "+GUI.GetNameOfFocusedControl() +", tag "+ tag);
				if(_uiWidth != (Mathf.RoundToInt(claustraD.width*1000)).ToString() && (GUI.GetNameOfFocusedControl() != "sizeClaustra" || Event.current.keyCode == KeyCode.Return))
				{
					float newvalue = float.Parse(_uiWidth) / 1000;	
					
					if(newvalue<_minWidth)
					{
						newvalue= _minWidth;			
						int tmp = Mathf.RoundToInt(newvalue*1000f);
						_uiWidth = (Mathf.RoundToInt(tmp)).ToString();		
					}
					if((newvalue>_minWidth)&&(newvalue<_secondMinWidth))
					{
						newvalue= _minWidth;			
						int tmp = Mathf.RoundToInt(newvalue*1000f);
						_uiWidth = (Mathf.RoundToInt(tmp)).ToString();	
					}
					if(newvalue>maxWidth)
					{
						newvalue = maxWidth;
						int tmp = Mathf.RoundToInt(newvalue*1000f);
						_uiWidth = (Mathf.RoundToInt(tmp)).ToString();	
					}
					claustraD.width = newvalue;
					_claustrasData[_selectedIndex.ToString()+tag] = claustraD;
					
					UpdateClaustraPartialWidth(_selectedIndex,tag);
				}
		#endif
				if(GUILayout.RepeatButton("+","btn+",GUILayout.Height(50),GUILayout.Width(50)) && Time.time > _timer)
				{
					claustraD.width = claustraD.width + 0.1f;	
					if((claustraD.width>_minWidth)&&(claustraD.width<_secondMinWidth))
					{
						claustraD.width=_secondMinWidth;
					}
					if(claustraD.width<_minWidth)
					{
						claustraD.width = _minWidth;
					}	
					if(claustraD.width>maxWidth)
					{
						claustraD.width = maxWidth;
					}
					int tmp = Mathf.RoundToInt(claustraD.width*1000f);
					_uiWidth = (Mathf.RoundToInt(tmp)).ToString();				
					_claustrasData[_selectedIndex.ToString()+tag] = claustraD;
					
					UpdateClaustraPartialWidth(_selectedIndex,tag);
					_timer = Time.time + _refresh;
				}
				
				
				GUILayout.Space(30);
				GUILayout.EndHorizontal();			
			}
			_applyToAll = GUILayout.Toggle(_applyToAll,TextManager.GetText("Pergola.ApplyToAll"),"toggle2",GUILayout.Height(50),GUILayout.Width(280));
		}
	}
	
	private void UpdateClaustraPartialWidth(int index, string tag)
	{		
		bool partialWidth = true;//_claustrasData[index.ToString()+tag].partialWidth;
		bool fromLeft = _claustrasData[index.ToString()+tag].fromLeft;
		float width = _claustrasData[index.ToString()+tag].width;
		
		/*ArrayList toDelete = new ArrayList();
		foreach (Transform child in _claustras.transform)
		{
			if(child.name.Contains(index.ToString()+tag))
				Destroy(child.gameObject);
				//toDelete.Add(child);
		}
		//foreach(Transform childToDelete in toDelete)
		//	Destroy(childToDelete.gameObject);
		BuildSingleClaustra(_pos[index],index,tag);	*/
		PergolaAutoFeetEvents.FireRebuild();
	}
		
	private void ApplyToAll(claustraData data)
	{
		if(!data.isActive)
			return;
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_nsewTypes[i].x == 1 && _canHaveClaustraN)
			{
				string nme = i+_sideN;
				if(_claustrasData.ContainsKey(nme))
				{
					if(_claustrasData[nme].isActive)
					{
						claustraData dataTemp = _claustrasData[nme];
						dataTemp.partialWidth = true;//data.partialWidth;
						dataTemp.fromLeft = data.fromLeft;
						_claustrasData[nme]=dataTemp;
			//			UpdateClaustraPartialWidth(i, _sideN);
					}
				}
			}
			
			if(_nsewTypes[i].y == 1 && _canHaveClaustraS)
			{
				string nme = i+_sideS;
				if(_claustrasData.ContainsKey(nme))
				{
					if(_claustrasData[nme].isActive)
					{
						claustraData dataTemp = _claustrasData[nme];
						dataTemp.partialWidth = true;//data.partialWidth;
						dataTemp.fromLeft = data.fromLeft;
						_claustrasData[nme]=dataTemp;
				//		UpdateClaustraPartialWidth(i, _sideS);
					}
				}
			}
			
			if(_nsewTypes[i].w == 1 && _canHaveClaustraW)
			{
				string nme = i+_sideW;
				if(_claustrasData.ContainsKey(nme))
				{
					if(_claustrasData[nme].isActive)
					{
						claustraData dataTemp = _claustrasData[nme];
						dataTemp.partialWidth = true;//data.partialWidth;
						dataTemp.fromLeft = data.fromLeft;
						_claustrasData[nme]=dataTemp;
				//		UpdateClaustraPartialWidth(i, _sideW);
					}
				}
			}
			
			if(_nsewTypes[i].z == 1 && _canHaveClaustraE)
			{
				string nme = i+_sideE;
				if(_claustrasData.ContainsKey(nme))
				{
					if(_claustrasData[nme].isActive)
					{
						claustraData dataTemp = _claustrasData[nme];
						dataTemp.partialWidth = true;//data.partialWidth;
						dataTemp.fromLeft = data.fromLeft;
						_claustrasData[nme]=dataTemp;
				//		UpdateClaustraPartialWidth(i, _sideE);
					}
				}
			}			
		}
		PergolaAutoFeetEvents.FireRebuild();
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
		_uiGeneral = true;
		if( _paf.GetH()>(_defaultHeight))
		{
			_canHaveClaustraN = false;
			_canHaveClaustraS = false;
			_canHaveClaustraE = false;
			_canHaveClaustraW = false;
			_uiGeneral = false;
			return;
		}
		switch (s)
		{
		case "ilot":
			_canHaveClaustraN = true;
			_canHaveClaustraS = true;
			_canHaveClaustraE = true;
			_canHaveClaustraW = true;
			break;
		case "muralLength":
			_canHaveClaustraN = false;
			_canHaveClaustraS = true;
			_canHaveClaustraE = true;
			_canHaveClaustraW = true;
			break;
		case "muralWidth":
			_canHaveClaustraN = true;
			_canHaveClaustraS = true;
			_canHaveClaustraE = true;
			_canHaveClaustraW = false;
			break;
		case "lN":
			_canHaveClaustraN = false;
			_canHaveClaustraS = true;
			_canHaveClaustraE = false;
			_canHaveClaustraW = true;
			break;
		case "lS":
			_canHaveClaustraN = true;
			_canHaveClaustraS = false;
			_canHaveClaustraE = false;
			_canHaveClaustraW = true;
			break;
		case "patio":
			_canHaveClaustraN = false;
			_canHaveClaustraS = false;
			_canHaveClaustraE = false;
			_canHaveClaustraW = false;
			break;
		}
	}
	
	public void SaveOption(BinaryWriter buf)
	{		
		buf.Write(_claustrasData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_claustrasData.ContainsKey(i+_sideN))
			{
				buf.Write((string)(i+_sideN));					//Key
				buf.Write(_claustrasData[i+_sideN].isActive);
				buf.Write(_claustrasData[i+_sideN].partialWidth);
				buf.Write(_claustrasData[i+_sideN].fromLeft);
				buf.Write(_claustrasData[i+_sideN].width);
			}
			
			if(_claustrasData.ContainsKey(i+_sideS))
			{
				buf.Write((string)(i+_sideS));
				buf.Write(_claustrasData[i+_sideS].isActive);
				buf.Write(_claustrasData[i+_sideS].partialWidth);
				buf.Write(_claustrasData[i+_sideS].fromLeft);
				buf.Write(_claustrasData[i+_sideS].width);
			}
			
			if(_claustrasData.ContainsKey(i+_sideE))
			{
				buf.Write((string)(i+_sideE));
				buf.Write(_claustrasData[i+_sideE].isActive);
				buf.Write(_claustrasData[i+_sideE].partialWidth);
				buf.Write(_claustrasData[i+_sideE].fromLeft);
				buf.Write(_claustrasData[i+_sideE].width);
			}
			
			if(_claustrasData.ContainsKey(i+_sideW))
			{
				buf.Write((string)(i+_sideW));
				buf.Write(_claustrasData[i+_sideW].isActive);
				buf.Write(_claustrasData[i+_sideW].partialWidth);
				buf.Write(_claustrasData[i+_sideW].fromLeft);
				buf.Write(_claustrasData[i+_sideW].width);
			}
		}
	}
	
	public void LoadOption(BinaryReader buf)
	{
		int entryNb = buf.ReadInt32();
		_claustrasData.Clear();
		
		for(int i=0;i<entryNb;i++)
		{
			string key = buf.ReadString();
			
			claustraData claustraD;
			
			claustraD.isActive = buf.ReadBoolean();
			claustraD.partialWidth = buf.ReadBoolean();
			claustraD.fromLeft = buf.ReadBoolean();
			claustraD.width = buf.ReadSingle();
			
			_claustrasData.Add(key,claustraD);
		}
	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_claustrasData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_claustrasData.ContainsKey(i+_sideN))
			{
				al.Add((string)(i+_sideN));					//Key
				al.Add(_claustrasData[i+_sideN].isActive);
				al.Add(_claustrasData[i+_sideN].partialWidth);
				al.Add(_claustrasData[i+_sideN].fromLeft);
				al.Add(_claustrasData[i+_sideN].width);
			}
			
			if(_claustrasData.ContainsKey(i+_sideS))
			{
				al.Add((string)(i+_sideS));
				al.Add(_claustrasData[i+_sideS].isActive);
				al.Add(_claustrasData[i+_sideS].partialWidth);
				al.Add(_claustrasData[i+_sideS].fromLeft);
				al.Add(_claustrasData[i+_sideS].width);
			}
			
			if(_claustrasData.ContainsKey(i+_sideE))
			{
				al.Add((string)(i+_sideE));
				al.Add(_claustrasData[i+_sideE].isActive);
				al.Add(_claustrasData[i+_sideE].partialWidth);
				al.Add(_claustrasData[i+_sideE].fromLeft);
				al.Add(_claustrasData[i+_sideE].width);
			}
			
			if(_claustrasData.ContainsKey(i+_sideW))
			{
				al.Add((string)(i+_sideW));
				al.Add(_claustrasData[i+_sideW].isActive);
				al.Add(_claustrasData[i+_sideW].partialWidth);
				al.Add(_claustrasData[i+_sideW].fromLeft);
				al.Add(_claustrasData[i+_sideW].width);
			}
		}
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		int entryNb = (int)al[0];
		
		_claustrasData.Clear();
		
		int off7 = 1;
		for(int i=0;i<entryNb;i++)
		{
			string key = (string)al[off7];
			off7 ++;
			
			claustraData claustraD;
			
			claustraD.isActive = (bool)al[off7];
			off7 ++;
			claustraD.partialWidth = (bool)al[off7];
			claustraD.partialWidth = (bool)al[off7];
			off7 ++;
			claustraD.fromLeft = (bool)al[off7];
			off7 ++;
			claustraD.width = (float)al[off7];
			off7++;
			
			_claustrasData.Add(key,claustraD);
		}
	}
	
}
