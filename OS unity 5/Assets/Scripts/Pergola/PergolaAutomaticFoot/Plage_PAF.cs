using UnityEngine;
using System.Collections;
using System.IO;

public class Plage_PAF : MonoBehaviour,IPergolaAutoFeet
{
	
	public GameObject meshRef;		//Mesh de réference
	
	public float thickness;			//épaisseur de la plage
	
	public Material plageMat;		//material
	//----------------------------------------------
	private bool _uiShow;			//switch affichage de l'ui
	private bool _hasPlage;			//switch affichage plage 3d
	
	private FunctionConf_PergolaAutoFeet _paf; //script parent
	
	private GameObject _plage;		//objet parent
	
	private float _off7N = 0;			//offset plage coté Nord
	private float _off7S = 0;			//offset plage coté Sud
	private float _off7E = 0;			//offset plage coté Est
	private float _off7W = 0;			//offset plage coté Ouest
	
	private bool _uiN = false;			//toggle affichage
	private bool _uiS = false;			//toggle affichage
	private bool _uiE = false;			//toggle affichage
	private bool _uiW = false;			//toggle affichage
	
	private const float _maxOffset = 10;
	//----------------------------------------------
	
	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{
		_paf = origin;
		
		if(transform.FindChild("PergolaPlage"))
			_plage = transform.FindChild("PergolaPlage").gameObject;
		else
		{
			_plage = new GameObject("PergolaPlage");
			_plage.transform.parent = parent;
			_plage.transform.localPosition = Vector3.zero;
		}
		
		if(!_plage.GetComponent<MeshRenderer>())
			_plage.AddComponent<MeshRenderer>();
		_plage.GetComponent<Renderer>().material = plageMat;
	}
	
	void OnEnable()
	{
		PergolaAutoFeetEvents.toggleUIVisibility += ToggleUI;
	}
	void OnDisable()
	{
		PergolaAutoFeetEvents.toggleUIVisibility -= ToggleUI;
	}
	
	public void Build(Vector3 origine,int index,Vector4 nsew)
	{
		if(index == 0)
		{
			GameObject beach = (GameObject)Instantiate(meshRef);
			beach.name = "beach";
			beach.layer = 0;
			beach.transform.parent = _plage.transform;
			beach.GetComponent<Renderer>().material = _plage.GetComponent<Renderer>().material;
			beach.transform.localScale = new Vector3(_paf.GetL() + _off7E+_off7W,thickness,_paf.GetW() +_off7N+_off7S);
			beach.transform.localPosition = (new Vector3(_off7E-_off7W,-thickness-0.002f,_off7N-_off7S))/2;
//			beach.renderer.material.SetTextureScale("_MainTex",new Vector2(_paf.GetL() + _off7E+_off7W,_paf.GetW() +_off7N+_off7S));
			_plage.GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(_paf.GetL() + _off7E+_off7W,_paf.GetW() +_off7N+_off7S));
			
			beach.GetComponent<Renderer>().enabled = _hasPlage;
		}
	}
	
	public void GetUI(/*GUISkin skin*/)
	{
		//Header
		bool tmpui = GUILayout.Toggle(_uiShow,TextManager.GetText("PergolaPlage"),GUILayout.Height(50),GUILayout.Width(280));
		if(tmpui != _uiShow)
		{
			_uiShow = tmpui;
			if(_uiShow)
				PergolaAutoFeetEvents.FireToggleUIVisibility(GetType().ToString());
			else
				PergolaAutoFeetEvents.FireToggleUIVisibility("close");

		}
		
		//Body ^^
		if(_uiShow)
		{
			bool tmp = GUILayout.Toggle(_hasPlage,TextManager.GetText((_hasPlage)? "Cacher":"Afficher" ),"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			
			if(tmp != _hasPlage)
			{
				_hasPlage = tmp;
				_plage.transform.GetChild(0).GetComponent<Renderer>().enabled = _hasPlage;
				
				if(_hasPlage)
					ResizePlage();
				
			}
			
			if(_hasPlage)
			{
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
					SingleSizeUI(ref _off7N,"N");
				}
				
				else if(_uiS)
				{
					SingleSizeUI(ref _off7S,"S");
				}
				
				else if(_uiE)
				{
					SingleSizeUI(ref _off7E,"E");
				}
				
				else if(_uiW)
				{
					SingleSizeUI(ref _off7W,"W");
				}
			}
			
		}
	}
	
	public void Clear()
	{
		foreach(Transform t in _plage.transform)
			Destroy(t.gameObject);
	}
	
	public void ToggleUI(string s)
	{
		if(s != GetType().ToString())
			_uiShow = false;
	}
	
	//----------------------------------------------
	
	private void SingleSizeUI(ref float off7,string tag)
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		float tmp = GUILayout.HorizontalSlider(off7,0,_maxOffset,GUILayout.Width(180));
		if(tmp != off7)
		{
			off7 = tmp;
			ResizePlage();
		}
//		GUILayout.Label(TextManager.GetText("Pergola.Side")+" "+tag,GUILayout.Height(50));
		GUILayout.Space(20);
		GUILayout.EndHorizontal();	
	}
	
	private void ResizePlage()
	{
		GameObject go = _plage.transform.GetChild(0).gameObject;
		
		go.transform.localScale = new Vector3(_paf.GetL() + _off7E+_off7W,thickness,_paf.GetW() +_off7N+_off7S);
		go.transform.localPosition = new Vector3((_off7E-_off7W)/2,(-thickness-0.002f)/2,(_off7N-_off7S)/2);
		
		_plage.GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(_paf.GetL() + _off7E+_off7W,_paf.GetW() +_off7N+_off7S));
		
	}
	
	public void SaveOption(BinaryWriter buf)
	{
		buf.Write(_hasPlage);
		buf.Write(_off7N);
		buf.Write(_off7S);
		buf.Write(_off7E);
		buf.Write(_off7W);
		
	}
	
	public void LoadOption(BinaryReader buf)
	{
		_hasPlage = buf.ReadBoolean();
		_off7N = buf.ReadSingle();
		_off7S = buf.ReadSingle();
		_off7E = buf.ReadSingle();
		_off7W = buf.ReadSingle();
	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_hasPlage);
		al.Add(_off7N);
		al.Add(_off7S);
		al.Add(_off7E);
		al.Add(_off7W);
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		_hasPlage = (bool)al[0];
		_off7N = (float)al[1];
		_off7S = (float)al[2];
		_off7E = (float)al[3];
		_off7W = (float)al[4];
	}
	
}
