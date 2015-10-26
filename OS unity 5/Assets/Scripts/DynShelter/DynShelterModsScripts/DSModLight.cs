using UnityEngine;
using System.Collections;
using Pointcube.Global;
using System.IO;

public class DSModLight : MonoBehaviour,IDSMod
{
	public int 			id;
	
	public bool 		isGlobal;					//configuration appliqué a tout les Mod du meme HTag
	public bool			isIntegrated = false;
	public bool			isProjector;				//projector ou spotLight
	public bool			useRefMesh;					//Utilise le mesh nommé en dessous
	public bool			useCustomSpawnLocation;		//Au lieu de spawner a l'endroit des RefsMesh, spawn a la spawnLocalPos
													//(position local/transform auquel est attaché ce script)
	
	public string[] 	refsName;					//nom des refsMesh
	public string		meshColorPropName = "_MainColor";//"_RimColor" par exemple pour la glowBall si custom shader
	public string		lightColorPropName = "_MainColor";//Nécessaire que si Projector
	public string 		uiRefName;
	
	public Color32[] 		colors; //Liste De couleur >> si 0 juste Switch On/Off
	
	public GameObject	instanceRef;				//GameObject a instancier
	public Vector3		spawnLocalPos = Vector3.zero;				//Position local d'instanciation si useCustomSpawnLocation activé
	
	//-------------------------
	private static readonly string DEBUGTAG = "DSModLight : ";
//	private string 	_hashTag = "nuuull";
	
	private bool 	_isSwitchedOn;
//	private bool	_uiShowColors;
	private bool	_hasColors;
	
	private int _uiscrollPos;
	
	private int 	_idSelectedColor;
	
	private ArrayList _modObjs = new ArrayList();
	
	private Texture2D _blank;
	
	private DynShelterModManager _modsManager;
	
	private float x= 0,z= 0,ratio= 0,size = 0;
	private SceneModel sm;
	
	private string go;
	
	private float _power = 0.5f; // %age
	private float _baseLightIntensity;
	private Color _baseProjIntensity;
	private Color _baseModObjIntensity;

	//-------------------------
	void Awake()
	{
//		_hashTag = GetType().ToString()+"_"+(isGlobal? "G":"L")+"_"+id.ToString();
		
		//init
		_isSwitchedOn   	= false;
//		_uiShowColors		= false;
		_idSelectedColor	= 0;
		_uiscrollPos 		= 0;
		_blank 				= (Texture2D)Resources.Load("gui/blank");
		
		
//		BoxCollider bc = transform.GetComponentInChildren<BoxCollider>();
//		x = bc.size.x;
//		z = bc.size.z;
//		ratio = x/z;
//		size = x;
		sm = Montage.sm;
		
		//Errors
		if(colors.Length == 0)
		{
			_hasColors = false;
			Debug.LogWarning(DEBUGTAG +"No colors set > only Switch On/Off");
		}
		else
		{
			_hasColors = true;	
		}
		
		if(refsName.Length == 0)
			Debug.LogError(DEBUGTAG +"refsName[]"+PC.MISSING_REF);
		else
			InitRefs();
		go = gameObject.name;
		
		UsefullEvents.ScaleChange += UpdateScale;
//		UsefullEvents.NightMode += NigthModeUpdated;
		
		foreach(GameObject g in _modObjs)
		{
			
			if(isProjector)
			{
				foreach(Projector p in g.GetComponentsInChildren<Projector>(true))
				{
					_baseProjIntensity = p.material.color;
					UpdatePower();
					return;
				}
			}
			else
			{
				foreach(Light l in g.GetComponentsInChildren<Light>(true))
				{
					_baseLightIntensity = l.intensity;
					UpdatePower();
					return;
				}
			}
		}
	}
	
	//-------------------------
	private void GUIColors()
	{
		//Sélecteur de couleurs
		
		GUILayout.BeginHorizontal("","Outil",GUILayout.Height(50),GUILayout.Width(260));
		GUILayout.FlexibleSpace();
//		
//		if(GUILayout.Button("","btn<",GUILayout.Width(50),GUILayout.Height(50)))
//		{
//			_uiscrollPos --;
//			if(_uiscrollPos < 0)
//				_uiscrollPos = 0;
//			
//		}
//		//----------
//		for(int col=_uiscrollPos;col<_uiscrollPos+3;col++)
//		{
//			GUI.color = colors[col];
//			if(GUILayout.Toggle(col==_idSelectedColor,"","colorBtn",GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				if(col != _idSelectedColor)
//				{
//					_idSelectedColor = col;
//					if(isGlobal)
//						SetToAll();
//					else
//						SetColors();
//				}
//			}
//		}
//		GUI.color = Color.white;
//		//----------
//		if(GUILayout.Button("","btn>",GUILayout.Width(50),GUILayout.Height(50)))
//		{
//			_uiscrollPos ++;
//			if(_uiscrollPos+3 > colors.Length)
//				_uiscrollPos = colors.Length-3;
//		}
//		
//		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUI.BeginGroup(GUILayoutUtility.GetLastRect());
		
		if(GUI.Button(new Rect(10,0,50,50),"","btn<"))
		{
			_uiscrollPos --;
			if(_uiscrollPos < 0)
				_uiscrollPos = 0;
		}
		
		//----------
		for(int col=_uiscrollPos;col<_uiscrollPos+3;col++)
		{
			Rect pos = new Rect((col-_uiscrollPos)*50+60,0,50,50);
			GUI.color = colors[col];
			GUI.DrawTexture(pos,_blank);
			GUI.color = Color.white;
			
			if(GUI.Toggle(pos,col == _idSelectedColor,"","colorBtn"))
			{
				if(col != _idSelectedColor)
				{
					_idSelectedColor = col;
					if(isGlobal)
						SetToAll();
					else
						SetColors();
				}
			}
			
		}
		
		//----------
		
		if(GUI.Button(new Rect(260 - 50,0,50,50),"","btn>"))
		{
			_uiscrollPos ++;
			if(_uiscrollPos+3 > colors.Length)
				_uiscrollPos = colors.Length-3;
		}
		
		GUI.EndGroup();
	}
	
	private void InitRefs()
	{
		if(useCustomSpawnLocation) 	//Ajout de une instance a l'endroit spécifié
		{
			GameObject g = (GameObject) Instantiate(instanceRef);
			g.transform.parent = transform;
			g.transform.localPosition = spawnLocalPos;
			g.transform.localRotation = Quaternion.identity;

			//Ajout
			_modObjs.Add(g);
		}
		
		foreach(Transform t in GetComponentsInChildren<Transform>(true))
		{
			foreach(string s in refsName)
			{
				if(t.name == s)
				{
					if(!useCustomSpawnLocation)
					{
						GameObject g = (GameObject) Instantiate(instanceRef);	
						g.transform.parent = t;
						g.transform.localPosition = Vector3.zero;
						g.transform.localRotation = Quaternion.identity;
	
						//Ajout
						_modObjs.Add(g);
					}
					
					if(useRefMesh)
					{
						_modObjs.Add(t.gameObject);
					}
					
				}
			}
		}
		
		Switch();
		
		if(_hasColors)
		{
			SetColors();
		}

		UpdateScaleFirst();
	}
	
	private void SetColors()
	{
		Color currentColor = colors[_idSelectedColor];
		currentColor.r = _power * currentColor.r;
		currentColor.g = _power * currentColor.g;
		currentColor.b = _power * currentColor.b;
		
		
		
		foreach(GameObject g in _modObjs)
		{
			foreach(MeshRenderer mr in g.GetComponentsInChildren<MeshRenderer>(true))
			{
				mr.material.SetColor(meshColorPropName,currentColor);//colors[_idSelectedColor]);
			}
			if(isProjector)
			{
				foreach(Projector p in g.GetComponentsInChildren<Projector>(true))
				{
//					p.material.SetColor(lightColorPropName,currentColor);//colors[_idSelectedColor]);
					p.material.color = currentColor;//colors[_idSelectedColor]);
				}
			}
			else
			{
				foreach(Light l in g.GetComponentsInChildren<Light>(true))
				{
					l.color = currentColor;//colors[_idSelectedColor]);
				}
			}
		}
	}
	
	private void Switch()
	{
		foreach(GameObject g in _modObjs)
		{
			g.SetActive(_isSwitchedOn);	
		}
	}
	
	private void UpdateScale()
	{
		foreach(GameObject g in _modObjs)
		{
			if(isProjector)
			{
				if(g==null)
					continue;
				foreach(Projector p in g.GetComponentsInChildren<Projector>(true))
				{
					p.farClipPlane = 1.5f * p.transform.position.y;
				}
			}
			else
			{
				if(g==null)
					continue;
				foreach(Light l in g.GetComponentsInChildren<Light>(true))
				{
					l.range = 2 * l.transform.position.y;
				}
			}
		}
	}
	
	private void UpdateScaleFirst()
	{

		foreach(GameObject g in _modObjs)
		{
			if(isProjector)
			{
				foreach(Projector p in g.GetComponentsInChildren<Projector>(true))
				{
					p.farClipPlane = 1.5f * p.transform.position.y * sm.getCamData().m_s;
				}
			}
			else
			{
				foreach(Light l in g.GetComponentsInChildren<Light>(true))
				{
					l.range = 2 * l.transform.position.y * sm.getCamData().m_s;
				}
			}
		}
	}
	
	private void UpdatePower()
	{
		if(_hasColors)
		{
			SetColors();
		}
		
		foreach(GameObject g in _modObjs)
		{
			if(meshColorPropName != "_MainColor")
			{
				foreach(MeshRenderer mr in g.GetComponentsInChildren<MeshRenderer>(true))
				{
					mr.material.SetColor("_MainColor",new Color(_power,_power,_power));//colors[_idSelectedColor]);
				}	
			}
				
				
			if(isProjector)
			{
				if(!_hasColors)
				{
					foreach(Projector p in g.GetComponentsInChildren<Projector>(true))
					{
						Color c = _baseProjIntensity;
						c.r *= _power;
						c.g *= _power;
						c.b *= _power;
						
						p.material.color = c;
					}
				}
			}
			else
			{
				foreach(Light l in g.GetComponentsInChildren<Light>(true))
				{
					l.intensity = _power * _baseLightIntensity;
				}
			}
		}
		
	}
	
	private void NigthModeUpdated(bool b)
	{
		_isSwitchedOn = b;
		
		if(isGlobal)
		{
			SetToAll();
		}
		else
			Switch();	
	}
	
	//-------------------------
	#region Interface's fcns.
	public string GetGameObj()
	{
		return gameObject.name;
	}
	
	public void GetModUI()
	{
		//UI Switch OnOff
		if(GUILayout.Button((_isSwitchedOn? 
			TextManager.GetText("DynShelter.Off")
			:TextManager.GetText("DynShelter.On"))
			+ " " +TextManager.GetText(uiRefName) ,"OutilWithoutIcon",GUILayout.Height(50),GUILayout.Width(260)))
		{
			_isSwitchedOn = !_isSwitchedOn;
			
//			if(_isSwitchedOn)
			UsefullEvents.FireNightMode(_isSwitchedOn);
			
			if(isGlobal)
			{
				SetToAll();
			}
			else
				Switch();
		}
		if(_isSwitchedOn)
		{
			GUILayout.BeginHorizontal("OutilWithoutIcon",GUILayout.Height(50),GUILayout.Width(260));
			
			GUILayout.Space(25+20);
			
			float tmpPower = GUILayout.HorizontalSlider(_power,0.1f,1f,GUILayout.Height(45));
			if(_power != tmpPower)
			{
				_power = tmpPower;
				if(isGlobal)
				{
					UpdatePower();
					SetToAll();
				}
				else
				{
					UpdatePower();
				}
			}	
			
			GUILayout.Space(25);
			
			GUILayout.EndHorizontal();
		}
		
		//UI Sélecteur de couleurs
		if(_hasColors && _isSwitchedOn)
		{
//			bool tmpcols = GUILayout.Toggle(_uiShowColors,"Colors","outil",GUILayout.Height(50),GUILayout.Width(260));
//			if(tmpcols != _uiShowColors)
//			{
//				_uiShowColors = tmpcols;
//			}
//			
//			if(_uiShowColors)
				GUIColors();
		}
	}
	
	public void SetToAll(bool reset = false)
	{
		ArrayList conf = new ArrayList();
		conf.Add(_idSelectedColor);	//0
		conf.Add(_isSwitchedOn);	//1
		conf.Add(_power);			//2
		
		_modsManager.ApplyGlobal(/*_hashTag*/GetHashTag(),conf,reset);
	}
	
	public void SetModManger(DynShelterModManager mgr)
	{
		_modsManager = mgr;	
	}
	
	public void ApplyConf(ArrayList conf,bool reset = false)
	{
		_idSelectedColor = 	(int) 	conf[0];
		_isSwitchedOn = 	(bool) 	conf[1];
		float tmpPower  = 	(float) conf[2];
		
		if(_power != tmpPower)
		{
			_power = tmpPower;
			UpdatePower();
		}
		else
		{
			if(_hasColors)
			{
				SetColors();
			}
		}
		Switch();
	}
	
	public string GetHashTag(){return GetType().ToString()+"_"+(isGlobal? "G":"L")+"_"+id.ToString();}// _hashTag;}
	
	public bool IsGlobalMod(){return isGlobal;}
	public bool IsIntegrated(){return isIntegrated;}
	
//	public void SaveConf (BinaryWriter buf)
//	{
//		buf.Write(_idSelectedColor);
//		buf.Write(_isSwitchedOn);
//	}
	
	public string SaveConf ()
	{
		string s;
		s = _idSelectedColor.ToString() +"#"+_isSwitchedOn.ToString()+"#"+_power.ToString();
		return s;
	}
	
	public void LoadConf(string conf)
	{
		string[] s = conf.Split('#');
		
		_idSelectedColor = int.Parse(s[0]);
		_isSwitchedOn = bool.Parse(s[1]);
		float tmpPower;
		if(s.Length == 3)
		{
			tmpPower  = float.Parse(s[2]);
		}
		else
			tmpPower = 1f;
		
		Switch();
		
		if(_power != tmpPower)
		{
			_power = tmpPower;
			UpdatePower();
		}
		else
		{
			if(_hasColors)
			{
				SetColors();
			}
		}
		
//		Switch();
//		if(_hasColors)
//			SetColors();
	}
	#endregion
}
