using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AllGlass_PAF : MonoBehaviour,IPergolaAutoFeet
{
	
	public GameObject meshRef;					//Objet réference a instancier

	private Material	_allGlassMaterial;
	
	private float 		_allGlassWidth=0.06f;
	// Espacement maximum entre deux montant verticaux
	public float 		_deltaWidth = 0.770f;
	public float 		_defaultHeight = 2.5f;
	private float		_offset = 0.01f; //sécurité pour éviter le clipping avec les poteaux verticaux
	private float		_offsetOpen = 0.025f; // espace entre les panneaux de verre lorsqu'ils soont en positions ouverts
	private float		_railWidth = 0.015f;//0.03f; // épaisseur du rail
	private float		_spaceBetweenGlass = 0.002f; // espace entre les panneaux de glass
	
	//------------------------------------------------------
	
	private FunctionConf_PergolaAutoFeet _paf;		//script parent
	
	private GameObject _allGlasss;					//GameObject Parent
	
	private bool _uiShow = false;				//Affichage de l UI?
	private bool _canHaveAllGlassN = true;		//peut avoir des screens coté N? (ex. patio > non)
	private bool _canHaveAllGlassS = true;		//peut avoir des screens coté S? (ex. patio > non)
	private bool _canHaveAllGlassE = true;		//peut avoir des screens coté E? (ex. patio > non)
	private bool _canHaveAllGlassW = true;		//peut avoir des screens coté W? (ex. patio > non)
	private bool _applyToAll = false;
	
	private int _selectedIndex = 0;				//numero module sélectionné
	
	private struct allGlassData					//Structure de donnée du screen
	{
		public bool isActive;					//Si ya un AllGlass ou pas
		public bool isOpen;						//Si le AllGlass est ouvert ou pas
		public bool right;						//Si le AllGlass s'ouvre à droite, sinon s'ouvre à gauche
	}
	
	private Dictionary<string,allGlassData> _allGlasssData = new Dictionary<string, allGlassData>();//Liste des allGlasss avec leur datas
	private Dictionary<int,Vector4> _nsewTypes = new Dictionary<int, Vector4>(); //type de bloc par numéro de bloc	
	
	private const string _sideN = "_N";				//Coté Nord/Front
	private const string _sideS = "_S";				//Coté Sud/Back
	private const string _sideE = "_E";				//Coté Est/Right
	private const string _sideW = "_W";				//Coté West/Left
	
	private bool _uiN 		= false;			//affichage ui reglage allGlass N
	private bool _uiS 		= false;			//affichage ui reglage allGlass S
	private bool _uiE 		= false;			//affichage ui reglage allGlass E
	private bool _uiW 		= false;			//affichage ui reglage allGlass W
		
	private bool _uiGeneral = true;				

		
	//----------vv Functions vv-----------------------------
	
	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{
		_paf = origin;		
		
		if(transform.FindChild("All-glassRef"))
			_allGlasss = transform.FindChild("All-glassRef").gameObject;
		else
		{
			_allGlasss = new GameObject("All-glassRef");
			_allGlasss.transform.parent = parent;
			_allGlasss.transform.localPosition = Vector3.zero;
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
		
		float allGlassL = _paf.GetLocalL();
		float allGlassW = _paf.GetLocalW();
		float allGlassH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
		float halffootSize = footSize/2;
		if(nsew.x == 1 && _canHaveAllGlassN)//Front = North
		{
			BuildSingleAllGlass(origine,index,_sideN);
		}	
		if(nsew.y == 1 && _canHaveAllGlassS)//Back = South
		{
			BuildSingleAllGlass(origine,index,_sideS);
		}
		
		if(nsew.w == 1 && _canHaveAllGlassW)//Left = West
		{
			BuildSingleAllGlass(origine,index,_sideW);
		}
		
		if(nsew.z == 1 && _canHaveAllGlassE)//Right = East
		{
			BuildSingleAllGlass(origine,index,_sideE);
		}
	}
	
	private void BuildSingleAllGlass(Vector3 origine,int index,string side)
	{		
		//Debug.Log("BuildSingleAllGlass  origine : "+origine);
		float allGlassL = _paf.GetLocalL();
		float allGlassW = _paf.GetLocalW();
		float allGlassH = _paf.GetH();
		
		float footSize = _paf.GetFootSize();
		//float doubleFootSize = footSize*2;
		float halfFootSize = footSize/8;
		
		//Get Screen data
		allGlassData allGlassD;
		string tag = index.ToString()+side;
		
		if(_allGlasssData.ContainsKey(tag))
		{
			allGlassD = _allGlasssData[tag];
			if(!allGlassD.isActive)
				return;
		}
		else
		{
			allGlassD.isActive = false;
			allGlassD.isOpen = false;
			allGlassD.right = true;

			_allGlasssData[tag] = allGlassD;
			return;
		}
		
		//Instantiate
		GameObject scr = (GameObject)Instantiate(meshRef);
		scr.transform.parent = _allGlasss.transform;
		scr.name = index.ToString()+side;
		if(scr.transform.GetChildCount()>0)
		{
			Transform child = scr.transform.GetChild(0);
			_allGlassWidth = child.GetComponent<Renderer>().bounds.extents.z;			
		}
		Transform top = scr.transform.FindChild("top");		
		if(top!=null)
		{
			if(top.GetComponent<Renderer>())
			{
				Transform frame = transform.FindChild("frame");
				if(frame!=null)
				{
					top.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
				}			
			}
		}
		Transform bottom = scr.transform.FindChild("bottom");		
		if(bottom!=null)
		{
			if(bottom.GetComponent<Renderer>())
			{
				Transform frame = transform.FindChild("frame");
				if(frame!=null)
				{
					bottom.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
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
			float width = allGlassL-combineFeet*footSize;
			float decalEmptyFoot = 0;
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lS)
			{
				if(_nsewTypes[index][2]==1)
				{
					width += footSize;
					decalFeet-=footSize/2;
				}
			}
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.muralWidth)
			{
				if(_nsewTypes[index][3]==1)
				{
					width += footSize;
					decalFeet+=footSize/2;
				}
			}
			if( width>_deltaWidth)
			{
				//construction des glass fermées
				int NbAllGlass = Mathf.CeilToInt(width/_deltaWidth);
				float singleWidth  = width/(float)NbAllGlass;
				Vector3 scaleN = new Vector3(singleWidth-_spaceBetweenGlass,h,1);
				scr.transform.localScale = scaleN;
				scr.gameObject.name=index.ToString()+side+"close";
				List<GameObject> allGlassList = new List<GameObject> ();
				allGlassList.Add(scr);
				for (int i=0; i<NbAllGlass+1;i++)
				{
					GameObject duplicateAllGlass = (GameObject)Instantiate(scr);
					duplicateAllGlass.transform.parent = _allGlasss.transform;
					duplicateAllGlass.transform.localScale = scaleN;					
					allGlassList.Add(duplicateAllGlass);
				}
				int allGlassNumero=0;
				float decalA=Mathf.Cos(Mathf.PI/4.0f)*singleWidth/2;
				float decalB=singleWidth/2-decalA;
				foreach(GameObject singleAllGlass in allGlassList)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
					singleWidth*(allGlassNumero+0.5f)-width/2.0f-decalFeet,
						posH/2,
						allGlassW/2.0f-_allGlassWidth-_offset);
					singleAllGlass.transform.Rotate(new Vector3(0,0,0));
					singleAllGlass.gameObject.SetActive(!allGlassD.isOpen);
					if(allGlassNumero==0)
					{
						singleAllGlass.gameObject.name=index.ToString()+side+"closeright";
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass-1)
					{
						singleAllGlass.gameObject.name=index.ToString()+side+"closeleft";
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && !allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass)
					{
						
						singleAllGlass.transform.Rotate(new Vector3(0,45.0f,0));
						singleAllGlass.transform.localPosition = origine + new Vector3(
							singleWidth*(0+0.5f)-width/2.0f-decalFeet-decalB,
							posH/2,
							allGlassW/2.0f-_allGlassWidth-_offset-decalA);
						singleAllGlass.gameObject.name=index.ToString()+side+"closeleft";
						Transform knobleft = singleAllGlass.transform.FindChild("knob-left");		
						if(knobleft!=null)
						{
							knobleft.GetComponent<Renderer>().enabled=true;
							knobleft.localScale = new Vector3(
								0.05f/singleAllGlass.transform.localScale.x,
								0.02f,								
								0.05f/singleAllGlass.transform.localScale.y);
						}
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && !allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass+1)
					{
						singleAllGlass.transform.Rotate(new Vector3(0,-45.0f,0));
						singleAllGlass.transform.localPosition = origine + new Vector3(
							singleWidth*(NbAllGlass-1+0.5f)-width/2.0f-decalFeet+decalB,
							posH/2,
							allGlassW/2.0f-_allGlassWidth-_offset-decalA);
						singleAllGlass.gameObject.name=index.ToString()+side+"closeright";
						Transform knobright = singleAllGlass.transform.FindChild("knob-right");		
						if(knobright!=null)
						{
							knobright.GetComponent<Renderer>().enabled=true;
							knobright.localScale = new Vector3(
								0.05f/singleAllGlass.transform.localScale.x,
								0.02f,								
								0.05f/singleAllGlass.transform.localScale.y);
						}
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && allGlassD.right);
					}
					allGlassNumero++;
				}
				//construction des glass ouvertes à gauche
				List<GameObject> allGlassListOpenLeft = new List<GameObject> ();
				for (int i=0; i<NbAllGlass;i++)
				{
					GameObject duplicateAllGlassOpenLeft = (GameObject)Instantiate(scr);
					duplicateAllGlassOpenLeft.gameObject.name=index.ToString()+side+"openleft";
					duplicateAllGlassOpenLeft.transform.parent = _allGlasss.transform;
					duplicateAllGlassOpenLeft.transform.localScale = scaleN;					
					allGlassListOpenLeft.Add(duplicateAllGlassOpenLeft);
				}
				int allGlassNumeroOpenLeft=0;
				foreach(GameObject singleAllGlass in allGlassListOpenLeft)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
					_offsetOpen*(allGlassNumeroOpenLeft+1f)-width/2.0f-decalFeet,
						posH/2,
						allGlassW/2.0f-_allGlassWidth-_offset-singleWidth/2);
					singleAllGlass.transform.Rotate(Vector3.up,90.0f);
					allGlassNumeroOpenLeft++;
					singleAllGlass.gameObject.SetActive(allGlassD.isOpen && !allGlassD.right);
				}
				//construction des glass ouvertes à droite
				List<GameObject> allGlassListOpenRight = new List<GameObject> ();
				for (int i=0; i<NbAllGlass;i++)
				{
					GameObject duplicateAllGlassOpenRight = (GameObject)Instantiate(scr);
					duplicateAllGlassOpenRight.gameObject.name=index.ToString()+side+"openright";
					duplicateAllGlassOpenRight.transform.parent = _allGlasss.transform;
					duplicateAllGlassOpenRight.transform.localScale = scaleN;					
					allGlassListOpenRight.Add(duplicateAllGlassOpenRight);
				}
				int allGlassNumeroOpenRight=0;
				foreach(GameObject singleAllGlass in allGlassListOpenRight)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
					-_offsetOpen*(allGlassNumeroOpenRight+1f)+width/2.0f-decalFeet,
						posH/2,
						allGlassW/2.0f-_allGlassWidth-_offset-singleWidth/2);
					singleAllGlass.transform.Rotate(Vector3.up,90.0f);
					allGlassNumeroOpenRight++;
					singleAllGlass.gameObject.SetActive(allGlassD.isOpen && allGlassD.right);
				}					
			}
			else
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(0,posH,allGlassW/2-halfFootSize-_allGlassWidth);
				scr.transform.Rotate(new Vector3(0,0,0));
			}				
			//création du rail inférieur
			GameObject railInf = GameObject.CreatePrimitive(PrimitiveType.Cube);
			railInf.name=index.ToString()+side+"RailInf";
			railInf.transform.parent = _allGlasss.transform;
			railInf.transform.localScale = new Vector3(width,_railWidth,_railWidth);
			railInf.transform.localPosition = origine + new Vector3(
				0.0f-decalFeet,
				_railWidth/2,
				allGlassW/2.0f-_allGlassWidth-_offset);
			
			Transform frame = transform.FindChild("frame");
			if(frame!=null)
			{
				railInf.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
			}		
			//création du rail supérieur
			GameObject railSup = GameObject.CreatePrimitive(PrimitiveType.Cube);
			railSup.name = index.ToString()+side+"RailSup";
			railSup.transform.parent = _allGlasss.transform;
			railSup.transform.localScale = new Vector3(width,_railWidth,_railWidth);
			railSup.transform.localPosition = origine + new Vector3(
				0-decalFeet,
				posH-_railWidth/2,
				allGlassW/2.0f-_allGlassWidth-_offset);
			if(frame!=null)
			{
				railSup.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
			}
			break;
		case _sideS:			
			combineFeet = _nsewTypes[index][2]+_nsewTypes[index][3];
			decalFeet = (_nsewTypes[index][2]-_nsewTypes[index][3])*footSize/2.0f;
			width = allGlassL-combineFeet*footSize;
			decalEmptyFoot = 0;
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lN)
			{
				if(_nsewTypes[index][2]==1)
				{
					width += footSize;
					decalFeet-=footSize/2;
				}
			}
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.muralWidth)
			{
				if(_nsewTypes[index][3]==1)
				{
					width += footSize;
					decalFeet+=footSize/2;
				}
			}
			if( width>_deltaWidth)
			{
				//construction des glass fermées
				int NbAllGlass = Mathf.CeilToInt(width/_deltaWidth);
				float singleWidth  = width/(float)NbAllGlass;
				Vector3 scaleN = new Vector3(singleWidth-_spaceBetweenGlass,h,1);
				scr.transform.localScale = scaleN;
				scr.gameObject.name=index.ToString()+side+"close";
				List<GameObject> allGlassList = new List<GameObject> ();
				allGlassList.Add(scr);
				for (int i=0; i<NbAllGlass+1;i++)
				{
					GameObject duplicateAllGlass = (GameObject)Instantiate(scr);
					duplicateAllGlass.transform.parent = _allGlasss.transform;
					duplicateAllGlass.transform.localScale = scaleN;
					allGlassList.Add(duplicateAllGlass);
				}
				int allGlassNumero=0;
				float decalA=Mathf.Cos(Mathf.PI/4.0f)*singleWidth/2;
				float decalB=singleWidth/2-decalA;
				foreach(GameObject singleAllGlass in allGlassList)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
					singleWidth*(allGlassNumero+0.5f)-width/2.0f-decalFeet,
						posH/2,
						-(allGlassW/2-_allGlassWidth-_offset));
					singleAllGlass.transform.Rotate(new Vector3(0,180,0));
					singleAllGlass.gameObject.SetActive(!allGlassD.isOpen);
					if(allGlassNumero==0)
					{
						singleAllGlass.gameObject.name=index.ToString()+side+"closeleft";
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && !allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass-1)
					{
						singleAllGlass.gameObject.name=index.ToString()+side+"closeright";
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass)
					{
						singleAllGlass.transform.Rotate(new Vector3(0,135.0f,0));
						singleAllGlass.transform.localPosition = origine + new Vector3(
							singleWidth*(0+0.5f)-width/2.0f-decalFeet-decalB,
							posH/2,
							-(allGlassW/2-_allGlassWidth-_offset-decalA));
						singleAllGlass.gameObject.name=index.ToString()+side+"closeright";	
						Transform knobleft = singleAllGlass.transform.FindChild("knob-left");		
						if(knobleft!=null)
						{
							knobleft.GetComponent<Renderer>().enabled=true;
							knobleft.localScale = new Vector3(
								0.05f/singleAllGlass.transform.localScale.x,
								0.02f,								
								0.05f/singleAllGlass.transform.localScale.y);
						}
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass+1)
					{
						singleAllGlass.transform.Rotate(new Vector3(0,225.0f,0));
						singleAllGlass.transform.localPosition = origine + new Vector3(
							singleWidth*(NbAllGlass-1+0.5f)-width/2.0f-decalFeet+decalB,
							posH/2,
							-(allGlassW/2-_allGlassWidth-_offset-decalA));
						singleAllGlass.gameObject.name=index.ToString()+side+"closeleft";
						Transform knobright = singleAllGlass.transform.FindChild("knob-right");		
						if(knobright!=null)
						{
							knobright.GetComponent<Renderer>().enabled=true;
							knobright.localScale = new Vector3(
								0.05f/singleAllGlass.transform.localScale.x,
								0.02f,								
								0.05f/singleAllGlass.transform.localScale.y);
						}
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && !allGlassD.right);
					}
					allGlassNumero++;
				}
				//construction des glass ouvertes à gauche
				List<GameObject> allGlassListOpenLeft = new List<GameObject> ();
				for (int i=0; i<NbAllGlass;i++)
				{
					GameObject duplicateAllGlassOpenLeft = (GameObject)Instantiate(scr);
					duplicateAllGlassOpenLeft.gameObject.name=index.ToString()+side+"openleft";
					duplicateAllGlassOpenLeft.transform.parent = _allGlasss.transform;
					duplicateAllGlassOpenLeft.transform.localScale = scaleN;					
					allGlassListOpenLeft.Add(duplicateAllGlassOpenLeft);
				}
				int allGlassNumeroOpenLeft=0;
				foreach(GameObject singleAllGlass in allGlassListOpenLeft)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
					-_offsetOpen*(allGlassNumeroOpenLeft+1f)+width/2.0f-decalFeet,
						posH/2,
						-(allGlassW/2.0f-_allGlassWidth-_offset-singleWidth/2));
					singleAllGlass.transform.Rotate(Vector3.up,90.0f);
					allGlassNumeroOpenLeft++;
					singleAllGlass.gameObject.SetActive(allGlassD.isOpen && !allGlassD.right);
				}
				//construction des glass ouvertes à droite
				List<GameObject> allGlassListOpenRight = new List<GameObject> ();
				for (int i=0; i<NbAllGlass;i++)
				{
					GameObject duplicateAllGlassOpenRight = (GameObject)Instantiate(scr);
					duplicateAllGlassOpenRight.gameObject.name=index.ToString()+side+"openright";
					duplicateAllGlassOpenRight.transform.parent = _allGlasss.transform;
					duplicateAllGlassOpenRight.transform.localScale = scaleN;					
					allGlassListOpenRight.Add(duplicateAllGlassOpenRight);
				}
				int allGlassNumeroOpenRight=0;
				foreach(GameObject singleAllGlass in allGlassListOpenRight)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
					_offsetOpen*(allGlassNumeroOpenRight+1f)-width/2.0f-decalFeet,
						posH/2,
						-(allGlassW/2.0f-_allGlassWidth-_offset-singleWidth/2));
					singleAllGlass.transform.Rotate(Vector3.up,90.0f);
					allGlassNumeroOpenRight++;
					singleAllGlass.gameObject.SetActive(allGlassD.isOpen && allGlassD.right);
				}		
				
			}
			else
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(0,posH,-(allGlassW/2-halfFootSize-_allGlassWidth));
				scr.transform.Rotate(new Vector3(0,180,0));
			}	
			//création du rail inférieur
			railInf = GameObject.CreatePrimitive(PrimitiveType.Cube);
			railInf.name=index.ToString()+side+"RailInf";
			railInf.transform.parent = _allGlasss.transform;
			railInf.transform.localScale = new Vector3(width,_railWidth,_railWidth);
			railInf.transform.localPosition = origine + new Vector3(
				0.0f-decalFeet,
				_railWidth/2,
				-allGlassW/2.0f+_allGlassWidth+_offset);
			
			frame = transform.FindChild("frame");
			if(frame!=null)
			{
				railInf.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
			}		
			//création du rail supérieur
			railSup = GameObject.CreatePrimitive(PrimitiveType.Cube);
			railSup.name = index.ToString()+side+"RailSup";
			railSup.transform.parent = _allGlasss.transform;
			railSup.transform.localScale = new Vector3(width,_railWidth,_railWidth);
			railSup.transform.localPosition = origine + new Vector3(
				0-decalFeet,
				posH-_railWidth/2,
				-allGlassW/2.0f+_allGlassWidth+_offset);
			if(frame!=null)
			{
				railSup.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
			}			
			
			break;
		case _sideE:			
			combineFeet = _nsewTypes[index][0]+_nsewTypes[index][1];
			decalFeet = (_nsewTypes[index][0]-_nsewTypes[index][1])*footSize/2.0f;
			width = allGlassW-combineFeet*footSize;		
			if( width>_deltaWidth)
			{
				//construction des glass fermées
				int NbAllGlass = Mathf.CeilToInt(width/_deltaWidth);
				float singleWidth  = width/(float)NbAllGlass;
				Vector3 scaleN = new Vector3(singleWidth-_spaceBetweenGlass,h,1);
				scr.transform.localScale = scaleN;
				scr.gameObject.name=index.ToString()+side+"close";
				List<GameObject> allGlassList = new List<GameObject> ();
				allGlassList.Add(scr);
				for (int i=0; i<NbAllGlass+1;i++)
				{
					GameObject duplicateAllGlass = (GameObject)Instantiate(scr);
					duplicateAllGlass.transform.parent = _allGlasss.transform;
					duplicateAllGlass.transform.localScale = scaleN;
					allGlassList.Add(duplicateAllGlass);
				}
				int allGlassNumero=0;
				float decalA=Mathf.Cos(Mathf.PI/4.0f)*singleWidth/2;
				float decalB=singleWidth/2-decalA;
				foreach(GameObject singleAllGlass in allGlassList)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
						allGlassL/2-_allGlassWidth-_offset,
						posH/2,
						singleWidth*(allGlassNumero+0.5f)-width/2.0f-decalFeet);
					singleAllGlass.transform.Rotate(new Vector3(0,90,0));	
					singleAllGlass.gameObject.SetActive(!allGlassD.isOpen);
					if(allGlassNumero==0)
					{
						singleAllGlass.gameObject.name=index.ToString()+side+"closeleft";
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && !allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass-1)
					{
						singleAllGlass.gameObject.name=index.ToString()+side+"closeright";
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass)
					{
						singleAllGlass.transform.Rotate(new Vector3(0,135.0f,0));
						singleAllGlass.transform.localPosition = origine + new Vector3(
						allGlassL/2-_allGlassWidth-_offset-decalA,
						posH/2,
						singleWidth*(0+0.5f)-width/2.0f-decalFeet-decalB);
						singleAllGlass.gameObject.name=index.ToString()+side+"closeright";
						Transform knobleft = singleAllGlass.transform.FindChild("knob-left");		
						if(knobleft!=null)
						{
							knobleft.GetComponent<Renderer>().enabled=true;
							knobleft.localScale = new Vector3(
								0.05f/singleAllGlass.transform.localScale.x,
								0.02f,								
								0.05f/singleAllGlass.transform.localScale.y);
						}
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass+1)
					{
						singleAllGlass.transform.Rotate(new Vector3(0,45.0f,0));
						singleAllGlass.transform.localPosition = origine + new Vector3(
						allGlassL/2-_allGlassWidth-_offset-decalA,
						posH/2,
						singleWidth*(NbAllGlass-1+0.5f)-width/2.0f-decalFeet+decalB);	
						singleAllGlass.gameObject.name=index.ToString()+side+"closeleft";
						Transform knobleft = singleAllGlass.transform.FindChild("knob-left");		
						if(knobleft!=null)
						{
							knobleft.GetComponent<Renderer>().enabled=true;
							knobleft.localScale = new Vector3(
								0.05f/singleAllGlass.transform.localScale.x,
								0.02f,								
								0.05f/singleAllGlass.transform.localScale.y);
						}
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && !allGlassD.right);
					}
					allGlassNumero++;
				}
				//construction des glass ouvertes à gauche
				List<GameObject> allGlassListOpenLeft = new List<GameObject> ();
				for (int i=0; i<NbAllGlass;i++)
				{
					GameObject duplicateAllGlassOpenLeft = (GameObject)Instantiate(scr);
					duplicateAllGlassOpenLeft.gameObject.name=index.ToString()+side+"openleft";
					duplicateAllGlassOpenLeft.transform.parent = _allGlasss.transform;
					duplicateAllGlassOpenLeft.transform.localScale = scaleN;					
					allGlassListOpenLeft.Add(duplicateAllGlassOpenLeft);
				}
				int allGlassNumeroOpenLeft=0;
				foreach(GameObject singleAllGlass in allGlassListOpenLeft)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
						allGlassL/2.0f-_allGlassWidth-_offset-singleWidth/2,
						posH/2,
					-_offsetOpen*(allGlassNumeroOpenLeft+1f)+width/2.0f-decalFeet);
					singleAllGlass.transform.Rotate(Vector3.up,90.0f);
					allGlassNumeroOpenLeft++;
					singleAllGlass.gameObject.SetActive(allGlassD.isOpen && !allGlassD.right);
				}
				//construction des glass ouvertes à droite
				List<GameObject> allGlassListOpenRight = new List<GameObject> ();
				for (int i=0; i<NbAllGlass;i++)
				{
					GameObject duplicateAllGlassOpenRight = (GameObject)Instantiate(scr);
					duplicateAllGlassOpenRight.gameObject.name=index.ToString()+side+"openright";
					duplicateAllGlassOpenRight.transform.parent = _allGlasss.transform;
					duplicateAllGlassOpenRight.transform.localScale = scaleN;					
					allGlassListOpenRight.Add(duplicateAllGlassOpenRight);
				}
				int allGlassNumeroOpenRight=0;
				foreach(GameObject singleAllGlass in allGlassListOpenRight)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
						allGlassL/2.0f-_allGlassWidth-_offset-singleWidth/2,
						posH/2,
					_offsetOpen*(allGlassNumeroOpenRight+1f)-width/2.0f-decalFeet);
					singleAllGlass.transform.Rotate(Vector3.up,90.0f);
					allGlassNumeroOpenRight++;
					singleAllGlass.gameObject.SetActive(allGlassD.isOpen && allGlassD.right);
				}	
				
			}
			else
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(allGlassL/2-halfFootSize-_allGlassWidth,posH,0);
				scr.transform.Rotate(new Vector3(0,90,0));
			}			
			//création du rail inférieur
			railInf = GameObject.CreatePrimitive(PrimitiveType.Cube);
			railInf.transform.Rotate(new Vector3(0,90,0));	
			railInf.name=index.ToString()+side+"RailInf";
			railInf.transform.parent = _allGlasss.transform;
			railInf.transform.localScale = new Vector3(width,_railWidth,_railWidth);
			railInf.transform.localPosition = origine + new Vector3(
				allGlassL/2.0f-_allGlassWidth-_offset,
				_railWidth/2,
				0.0f-decalFeet);
			
			frame = transform.FindChild("frame");
			if(frame!=null)
			{
				railInf.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
			}		
			//création du rail supérieur
			railSup = GameObject.CreatePrimitive(PrimitiveType.Cube);
			railSup.transform.Rotate(new Vector3(0,90,0));	
			railSup.name = index.ToString()+side+"RailSup";
			railSup.transform.parent = _allGlasss.transform;
			railSup.transform.localScale = new Vector3(width,_railWidth,_railWidth);
			railSup.transform.localPosition = origine + new Vector3(
				allGlassL/2.0f-_allGlassWidth-_offset,
				posH-_railWidth/2,
				0.0f-decalFeet);
			if(frame!=null)
			{
				railSup.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
			}			
			
			break;
		case _sideW:				
			combineFeet = _nsewTypes[index][0]+_nsewTypes[index][1];
			decalFeet = (_nsewTypes[index][0]-_nsewTypes[index][1])*footSize/2.0f;
			width = allGlassW-combineFeet*footSize;	
			decalEmptyFoot = 0;
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lS)
			{
				if(_nsewTypes[index][1]==1)
				{
					width += footSize;
					decalFeet+=footSize/2;
				}
			}
			if(_paf._type==FunctionConf_PergolaAutoFeet.PergolaType.lN)
			{
				if(_nsewTypes[index][0]==1)
				{
					width += footSize;
					decalFeet-=footSize/2;
				}
			}
			if( width>_deltaWidth)
			{
				//construction des glass fermées
				int NbAllGlass = Mathf.CeilToInt(width/_deltaWidth);
				float singleWidth  = width/(float)NbAllGlass;
				Vector3 scaleN = new Vector3(singleWidth-_spaceBetweenGlass,h,1);
				scr.transform.localScale = scaleN;
				scr.gameObject.name=index.ToString()+side+"close";
				List<GameObject> allGlassList = new List<GameObject> ();
				allGlassList.Add(scr);
				for (int i=0; i<NbAllGlass+1;i++)
				{
					GameObject duplicateAllGlass = (GameObject)Instantiate(scr);
					duplicateAllGlass.transform.parent = _allGlasss.transform;
					duplicateAllGlass.transform.localScale = scaleN;
					allGlassList.Add(duplicateAllGlass);
				}
				int allGlassNumero=0;
				float decalA=Mathf.Cos(Mathf.PI/4.0f)*singleWidth/2;
				float decalB=singleWidth/2-decalA;
				foreach(GameObject singleAllGlass in allGlassList)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
						-allGlassL/2+_allGlassWidth+_offset,
						posH/2,
						singleWidth*(allGlassNumero+0.5f)-width/2.0f-decalFeet);
					singleAllGlass.transform.Rotate(new Vector3(0,270,0));		
					singleAllGlass.gameObject.SetActive(!allGlassD.isOpen);
					if(allGlassNumero==0)
					{
						singleAllGlass.gameObject.name=index.ToString()+side+"closeright";
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass-1)
					{
						singleAllGlass.gameObject.name=index.ToString()+side+"closeleft";
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && !allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass)
					{
						singleAllGlass.transform.Rotate(new Vector3(0,225.0f,0));
						singleAllGlass.transform.localPosition = origine + new Vector3(
						-allGlassL/2+_allGlassWidth+_offset+decalA,
						posH/2,
						singleWidth*(0+0.5f)-width/2.0f-decalFeet-decalB);
						singleAllGlass.gameObject.name=index.ToString()+side+"closeleft";
						Transform knobright = singleAllGlass.transform.FindChild("knob-right");		
						if(knobright!=null)
						{
							knobright.GetComponent<Renderer>().enabled=true;
							knobright.localScale = new Vector3(
								0.05f/singleAllGlass.transform.localScale.x,
								0.02f,								
								0.05f/singleAllGlass.transform.localScale.y);
						}
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && !allGlassD.right);
					}
					if(allGlassNumero==NbAllGlass+1)
					{
						singleAllGlass.transform.Rotate(new Vector3(0,315.0f,0));
						singleAllGlass.transform.localPosition = origine + new Vector3(
						-allGlassL/2+_allGlassWidth+_offset+decalA,
						posH/2,
						singleWidth*(NbAllGlass-1+0.5f)-width/2.0f-decalFeet+decalB);
						singleAllGlass.gameObject.name=index.ToString()+side+"closeright";	
						Transform knobright = singleAllGlass.transform.FindChild("knob-right");		
						if(knobright!=null)
						{
							knobright.GetComponent<Renderer>().enabled=true;
							knobright.localScale = new Vector3(
								0.05f/singleAllGlass.transform.localScale.x,
								0.02f,								
								0.05f/singleAllGlass.transform.localScale.y);
						}
						singleAllGlass.gameObject.SetActive(!allGlassD.isOpen && allGlassD.right);
					}			
					allGlassNumero++;
				}
				//construction des glass ouvertes à gauche
				List<GameObject> allGlassListOpenLeft = new List<GameObject> ();
				for (int i=0; i<NbAllGlass;i++)
				{
					GameObject duplicateAllGlassOpenLeft = (GameObject)Instantiate(scr);
					duplicateAllGlassOpenLeft.gameObject.name=index.ToString()+side+"openleft";
					duplicateAllGlassOpenLeft.transform.parent = _allGlasss.transform;
					duplicateAllGlassOpenLeft.transform.localScale = scaleN;					
					allGlassListOpenLeft.Add(duplicateAllGlassOpenLeft);
				}
				int allGlassNumeroOpenLeft=0;
				foreach(GameObject singleAllGlass in allGlassListOpenLeft)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
						-allGlassL/2.0f+_allGlassWidth+_offset+singleWidth/2,
						posH/2,
					_offsetOpen*(allGlassNumeroOpenLeft+1f)-width/2.0f-decalFeet);
					singleAllGlass.transform.Rotate(Vector3.up,90.0f);
					allGlassNumeroOpenLeft++;
					singleAllGlass.gameObject.SetActive(allGlassD.isOpen && !allGlassD.right);
				}
				//construction des glass ouvertes à droite
				List<GameObject> allGlassListOpenRight = new List<GameObject> ();
				for (int i=0; i<NbAllGlass;i++)
				{
					GameObject duplicateAllGlassOpenRight = (GameObject)Instantiate(scr);
					duplicateAllGlassOpenRight.gameObject.name=index.ToString()+side+"openright";
					duplicateAllGlassOpenRight.transform.parent = _allGlasss.transform;
					duplicateAllGlassOpenRight.transform.localScale = scaleN;					
					allGlassListOpenRight.Add(duplicateAllGlassOpenRight);
				}
				int allGlassNumeroOpenRight=0;
				foreach(GameObject singleAllGlass in allGlassListOpenRight)
				{
					singleAllGlass.transform.localPosition = origine + new Vector3(
						-allGlassL/2.0f+_allGlassWidth+_offset+singleWidth/2,
						posH/2,
					-_offsetOpen*(allGlassNumeroOpenRight+1f)+width/2.0f-decalFeet);
					singleAllGlass.transform.Rotate(Vector3.up,90.0f);
					allGlassNumeroOpenRight++;
					singleAllGlass.gameObject.SetActive(allGlassD.isOpen && allGlassD.right);
				}		
				
			}
			else
			{
				Vector3 scaleN = new Vector3(width,sizeH,1);
				scr.transform.localScale = scaleN;
				scr.transform.localPosition = origine + new Vector3(-allGlassL/2+halfFootSize+_allGlassWidth,posH,0);
				scr.transform.Rotate(new Vector3(0,270,0));
			}	
			//création du rail inférieur
			railInf = GameObject.CreatePrimitive(PrimitiveType.Cube);
			railInf.transform.Rotate(new Vector3(0,90,0));	
			railInf.name=index.ToString()+side+"RailInf";
			railInf.transform.parent = _allGlasss.transform;
			railInf.transform.localScale = new Vector3(width,_railWidth,_railWidth);
			railInf.transform.localPosition = origine + new Vector3(
				-allGlassL/2+_allGlassWidth+_offset,
				_railWidth/2,
				0.0f-decalFeet);
			
			frame = transform.FindChild("frame");
			if(frame!=null)
			{
				railInf.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
			}		
			//création du rail supérieur
			railSup = GameObject.CreatePrimitive(PrimitiveType.Cube);
			railSup.transform.Rotate(new Vector3(0,90,0));	
			railSup.name = index.ToString()+side+"RailSup";
			railSup.transform.parent = _allGlasss.transform;
			railSup.transform.localScale = new Vector3(width,_railWidth,_railWidth);
			railSup.transform.localPosition = origine + new Vector3(
				-allGlassL/2+_allGlassWidth+_offset,
				posH-_railWidth/2,
				0.0f-decalFeet);
			if(frame!=null)
			{
				railSup.GetComponent<Renderer>().material = frame.GetComponent<Renderer>().material;
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
				tmpui = GUILayout.Toggle(_uiShow,TextManager.GetText("Pergola.AllGlass"),GUILayout.Height(50),GUILayout.Width(280));
			}
			catch(System.ArgumentException e)
			{
				Debug.LogError("AllGlass_PAF - GetUI "+e.Message);
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
				if(_allGlasssData.ContainsKey(_selectedIndex.ToString()+_sideN) && _nsewTypes[_selectedIndex].x == 1 && _canHaveAllGlassN)
				{
					_uiN = GUILayout.Toggle(_uiN,"Side N","toggleN",GUILayout.Height(50),GUILayout.Width(50));
					if(_uiN)
					{
						if(!_allGlasssData[_selectedIndex.ToString()+_sideN].isActive)
						{
							allGlassData allGlassD = _allGlasssData[_selectedIndex.ToString()+_sideN];
							allGlassD.isActive = true;
							_allGlasssData[_selectedIndex.ToString()+_sideN] = allGlassD;
							PergolaAutoFeetEvents.FireRebuild();
						}
						
						_uiE = _uiS = _uiW = false;
					}
				}
				if(_allGlasssData.ContainsKey(_selectedIndex.ToString()+_sideS) && _nsewTypes[_selectedIndex].y == 1 && _canHaveAllGlassS)
				{
					_uiS = GUILayout.Toggle(_uiS,"Side S","toggleS",GUILayout.Height(50),GUILayout.Width(50));
					if(_uiS)
					{
						if(!_allGlasssData[_selectedIndex.ToString()+_sideS].isActive)
						{
							allGlassData allGlassD = _allGlasssData[_selectedIndex.ToString()+_sideS];
							allGlassD.isActive = true;
							_allGlasssData[_selectedIndex.ToString()+_sideS] = allGlassD;
							PergolaAutoFeetEvents.FireRebuild();
						}
						
						_uiE = _uiN = _uiW = false;
					}
				}
				if(_allGlasssData.ContainsKey(_selectedIndex.ToString()+_sideE) && _nsewTypes[_selectedIndex].z == 1 && _canHaveAllGlassE)
				{
					_uiE = GUILayout.Toggle(_uiE,"Side E","toggleE",GUILayout.Height(50),GUILayout.Width(50));
					if(_uiE)
					{
						if(!_allGlasssData[_selectedIndex.ToString()+_sideE].isActive)
						{
							allGlassData allGlassD = _allGlasssData[_selectedIndex.ToString()+_sideE];
							allGlassD.isActive = true;
							_allGlasssData[_selectedIndex.ToString()+_sideE] = allGlassD;
							PergolaAutoFeetEvents.FireRebuild();
						}
						
						_uiN = _uiS = _uiW = false;
					}
				}
				if(_allGlasssData.ContainsKey(_selectedIndex.ToString()+_sideW) && _nsewTypes[_selectedIndex].w == 1 && _canHaveAllGlassW)
				{
					_uiW = GUILayout.Toggle(_uiW,"Side W","toggleW",GUILayout.Height(50),GUILayout.Width(50));
					if(_uiW)
					{
						if(!_allGlasssData[_selectedIndex.ToString()+_sideW].isActive)
						{
							allGlassData allGlassD = _allGlasssData[_selectedIndex.ToString()+_sideW];
							allGlassD.isActive = true;
							_allGlasssData[_selectedIndex.ToString()+_sideW] = allGlassD;
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
	}
	
	public void Clear()
	{
		foreach(Transform t in _allGlasss.transform)
			Destroy(t.gameObject);
	}
	
	private void CleanData()
	{
		int max = _paf.GetNbL()*_paf.GetNbW();
		ArrayList deleteList = new ArrayList();
		
		foreach(string s in _allGlasssData.Keys)
		{
			int index = int.Parse(s.Split('_')[0]);
			if(index>=max)
				deleteList.Add(s);
		}
		
		foreach(string del in deleteList)
		{
			_allGlasssData.Remove(del);	
		}
		
		_nsewTypes.Clear();
	}
	
	//---------------------------------
	
	private void SingleUI(string tag)
	{
		allGlassData allGlassD = _allGlasssData[_selectedIndex.ToString()+tag];
		
		string uiStr = TextManager.GetText((allGlassD.isActive)? "Cacher":"Afficher" );
		
		bool tmpActiv = GUILayout.Toggle(allGlassD.isActive,uiStr,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
		if(tmpActiv != allGlassD.isActive)
		{
			if(!tmpActiv)
				_uiN = _uiE = _uiS = _uiW = false;
			allGlassD.isActive = tmpActiv;
			_allGlasssData[_selectedIndex.ToString()+tag] = allGlassD;
			PergolaAutoFeetEvents.FireRebuild();
		}
		
		if(allGlassD.isActive)
		{
			//reglage ouverture-------------------------------------------
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			string uiStrOpen = TextManager.GetText((allGlassD.isOpen)? "Pergola.close":"Pergola.open" );
			bool tmpOpen = GUILayout.Toggle(allGlassD.isOpen,uiStrOpen,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			if(tmpOpen != allGlassD.isOpen)
			{
				allGlassD.isOpen = tmpOpen;
				_allGlasssData[_selectedIndex.ToString()+tag] = allGlassD;
				if(_applyToAll)
				{
					ApplyToAll(allGlassD);
				}
				else
					UpdateAllGlassOpen(_selectedIndex,tag);
			}
		//	GUILayout.Space(20);
			GUILayout.EndHorizontal();
			
			//reglage matiere-------------------------------------------
			
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			string uiStrRight = TextManager.GetText((allGlassD.right)? "Pergola.right":"Pergola.left" );
			bool tmpRight = GUILayout.Toggle(allGlassD.right,uiStrRight,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			if(tmpRight != allGlassD.right)
			{
				allGlassD.right = tmpRight;
				_allGlasssData[_selectedIndex.ToString()+tag] = allGlassD;
				if(_applyToAll)
				{
					ApplyToAll(allGlassD);
				}
				else
					UpdateAllGlassOpen(_selectedIndex,tag);
			}
		//	GUILayout.Space(20);
			GUILayout.EndHorizontal();				
			//-----Apply to all ------------
			//la partie commenté c'est pour faire un applytoall des qu'on toggle
			_applyToAll = GUILayout.Toggle(_applyToAll,TextManager.GetText("Pergola.ApplyToAll"),"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			
		}
		
	}

	private void UpdateAllGlassOpen(int index, string tag)
	{		
		bool open = _allGlasssData[index.ToString()+tag].isOpen;
		bool right = _allGlasssData[index.ToString()+tag].right;
		//Transform scr = _allGlasss.transform.FindChild(name);
		foreach(Transform child in _allGlasss.transform)
		{
			if(right)
			{
				if(child.name.Contains("left"))
				{
					child.gameObject.SetActive(false);
				}
				else
				{
					if(child.name.Contains(index.ToString()+tag+"open"))
					{
						child.gameObject.SetActive(open);
					}
					if(child.name.Contains(index.ToString()+tag+"close"))
					{
						child.gameObject.SetActive(!open);
					}
				}
			}
			else
			{
				if(child.name.Contains("right"))
				{
					child.gameObject.SetActive(false);
				}
				else
				{
					if(child.name.Contains(index.ToString()+tag+"open"))
					{
						child.gameObject.SetActive(open);
					}
					if(child.name.Contains(index.ToString()+tag+"close"))
					{
						child.gameObject.SetActive(!open);
					}
				}
			}
		}	
	}
		
	private void ApplyToAll(allGlassData data)
	{
		if(!data.isActive)
			return;
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_nsewTypes[i].x == 1 && _canHaveAllGlassN)
			{
				string nme = i+_sideN;
				if(_allGlasssData.ContainsKey(nme))
				{
					if(_allGlasssData[nme].isActive)
					{
						_allGlasssData[nme] = data;
						UpdateAllGlassOpen(i, _sideN);
					}
				}
			}
			
			if(_nsewTypes[i].y == 1 && _canHaveAllGlassS)
			{
				string nme = i+_sideS;
				if(_allGlasssData.ContainsKey(nme))
				{
					if(_allGlasssData[nme].isActive)
					{
						_allGlasssData[nme] = data;
						UpdateAllGlassOpen(i, _sideS);
					}
				}
			}
			
			if(_nsewTypes[i].w == 1 && _canHaveAllGlassW)
			{
				string nme = i+_sideW;
				if(_allGlasssData.ContainsKey(nme))
				{
					if(_allGlasssData[nme].isActive)
					{
						_allGlasssData[nme] = data;
						UpdateAllGlassOpen(i, _sideW);
					}
				}
			}
			
			if(_nsewTypes[i].z == 1 && _canHaveAllGlassE)
			{
				string nme = i+_sideE;
				if(_allGlasssData.ContainsKey(nme))
				{
					if(_allGlasssData[nme].isActive)
					{
						_allGlasssData[nme] = data;
						UpdateAllGlassOpen(i, _sideE);
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
	
	
	public void ToggleUI(string s) // ordre d'affichage (un affiché, les autres caché)
	{
		if(s != GetType().ToString())
		{
			_uiShow = false;
			_uiN = _uiS = _uiE = _uiW = false;
		}
	}
	
	public void UpdateType(string s) //Mise a jour du type (si patio > pas de screens)
	{
		_uiGeneral = true;
		if(_paf.GetH()>(_defaultHeight))
		{
			_canHaveAllGlassN = false;
			_canHaveAllGlassS = false;
			_canHaveAllGlassE = false;
			_canHaveAllGlassW = false;
			_uiGeneral = false;
			return;
		}
		switch (s)
		{
		case "ilot":
			_canHaveAllGlassN = true;
			_canHaveAllGlassS = true;
			_canHaveAllGlassE = true;
			_canHaveAllGlassW = true;
			break;
		case "muralLength":
			_canHaveAllGlassN = false;
			_canHaveAllGlassS = true;
			_canHaveAllGlassE = true;
			_canHaveAllGlassW = true;
			break;
		case "muralWidth":
			_canHaveAllGlassN = true;
			_canHaveAllGlassS = true;
			_canHaveAllGlassE = true;
			_canHaveAllGlassW = false;
			break;
		case "lN":
			_canHaveAllGlassN = false;
			_canHaveAllGlassS = true;
			_canHaveAllGlassE = false;
			_canHaveAllGlassW = true;
			break;
		case "lS":
			_canHaveAllGlassN = true;
			_canHaveAllGlassS = false;
			_canHaveAllGlassE = false;
			_canHaveAllGlassW = true;
			break;
		case "patio":
			_canHaveAllGlassN = false;
			_canHaveAllGlassS = false;
			_canHaveAllGlassE = false;
			_canHaveAllGlassW = false;
			break;
		}
	}
	
	public void SaveOption(BinaryWriter buf)
	{		
		buf.Write(_allGlasssData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_allGlasssData.ContainsKey(i+_sideN))
			{
				buf.Write((string)(i+_sideN));					//Key
				buf.Write(_allGlasssData[i+_sideN].isActive);
				buf.Write(_allGlasssData[i+_sideN].isOpen);
				buf.Write(_allGlasssData[i+_sideN].right);
			}
			
			if(_allGlasssData.ContainsKey(i+_sideS))
			{
				buf.Write((string)(i+_sideS));
				buf.Write(_allGlasssData[i+_sideS].isActive);
				buf.Write(_allGlasssData[i+_sideS].isOpen);
				buf.Write(_allGlasssData[i+_sideS].right);
			}
			
			if(_allGlasssData.ContainsKey(i+_sideE))
			{
				buf.Write((string)(i+_sideE));
				buf.Write(_allGlasssData[i+_sideE].isActive);
				buf.Write(_allGlasssData[i+_sideE].isOpen);
				buf.Write(_allGlasssData[i+_sideE].right);
			}
			
			if(_allGlasssData.ContainsKey(i+_sideW))
			{
				buf.Write((string)(i+_sideW));
				buf.Write(_allGlasssData[i+_sideW].isActive);
				buf.Write(_allGlasssData[i+_sideW].isOpen);
				buf.Write(_allGlasssData[i+_sideW].right);
			}
		}
	}
	
	public void LoadOption(BinaryReader buf)
	{
		int entryNb = buf.ReadInt32();
		_allGlasssData.Clear();
		
		for(int i=0;i<entryNb;i++)
		{
			string key = buf.ReadString();
			
			allGlassData allGlassD;
			allGlassD.isActive = buf.ReadBoolean();
			allGlassD.isOpen = buf.ReadBoolean();
			allGlassD.right = buf.ReadBoolean();
			
			_allGlasssData.Add(key,allGlassD);
		}
	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_allGlasssData.Count);
		
		int nb = _paf.GetNbL() * _paf.GetNbW();
		for(int i=0;i<nb;i++)
		{
			if(_allGlasssData.ContainsKey(i+_sideN))
			{
				al.Add((string)(i+_sideN));					//Key
				al.Add(_allGlasssData[i+_sideN].isActive);
				al.Add(_allGlasssData[i+_sideN].isOpen);
				al.Add(_allGlasssData[i+_sideN].right);
			}
			
			if(_allGlasssData.ContainsKey(i+_sideS))
			{
				al.Add((string)(i+_sideS));
				al.Add(_allGlasssData[i+_sideS].isActive);
				al.Add(_allGlasssData[i+_sideS].isOpen);
				al.Add(_allGlasssData[i+_sideS].right);
			}
			
			if(_allGlasssData.ContainsKey(i+_sideE))
			{
				al.Add((string)(i+_sideE));
				al.Add(_allGlasssData[i+_sideE].isActive);
				al.Add(_allGlasssData[i+_sideE].isOpen);
				al.Add(_allGlasssData[i+_sideE].right);
			}
			
			if(_allGlasssData.ContainsKey(i+_sideW))
			{
				al.Add((string)(i+_sideW));
				al.Add(_allGlasssData[i+_sideW].isActive);
				al.Add(_allGlasssData[i+_sideW].isOpen);
				al.Add(_allGlasssData[i+_sideW].right);
			}
		}
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		int entryNb = (int)al[0];
		
		_allGlasssData.Clear();
		
		int off7 = 1;
		for(int i=0;i<entryNb;i++)
		{
			string key = (string)al[off7];
			off7 ++;
			
			allGlassData allGlassD;

			allGlassD.isActive = (bool)al[off7];
			off7 ++;
			allGlassD.isOpen = (bool)al[off7];
			off7 ++;
			allGlassD.right = (bool)al[off7];
			off7 ++;
			
			_allGlasssData.Add(key,allGlassD);
		}
	}
	
}
