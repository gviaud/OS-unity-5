using UnityEngine;
using System.Collections;
using System.IO;
using System;
public class Neons_PAF : MonoBehaviour,IPergolaAutoFeet
{
	
	public GameObject meshRef;				//Poutre Hztl avec neon
	public GameObject fakeLightRef;			//Objet réference a instancier (plan qui fake l'éclairage)
	public Material fakeLightMat;			//Materiau du plan qui fake l'éclairage des néons
	public Color[] neonColors;				//Couleurs possibles des néons
	public float bladeSizeLimit;			//taille maxi module simple
	
	//---------------------------
	public static ContextDataModel cdm = new ContextDataModel();
	private FunctionConf_PergolaAutoFeet paf;	//script parent
	
	private bool _switchOnOff = false;		//Allumage ou non
	private bool _isLOriented = true;
	private bool _uiShow = false;			//Affichage de l UI
	private bool _isNightMode = false;		//mode nuit
	
	private static Texture2D[] _uiNeonColors;		//Couleurs possibles des néons /!\ faudrait ptet faire un load from resources au lieu du static
	private int _scrollPos;					//position scroll selecteur couleur
		
	private int _colorId = 0;				//ID de la couleur dans neonColors
	
	private float _intensity = 1f;
	
	private Color _lightColor;				//couleur de la lumiere
	
	private GameObject _neons;				//GameObject Parent
	
	private string _uiNightMode;			//txt du btn mode nuit
		
	//----------vv Functions vv-----------------------------
	
	#region Interface's functions
 	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{		
		//Création de la liste des textures
		if(neonColors.Length>0 && _uiNeonColors == null) //Si null on le créé
		{
			_uiNeonColors = new Texture2D[neonColors.Length];
			for(int i=0;i<neonColors.Length;i++)
			{
				Texture2D t = new Texture2D(40,40);
				Color [] tmp = new Color[t.GetPixels().Length];
				for(int c=0;c<t.GetPixels().Length;c++)
				{
					tmp[c] = neonColors[i];
				}
				t.SetPixels(tmp);		
				t.Apply();
				_uiNeonColors[i] = t;
			}
		}
		else if(_uiNeonColors != null) // si pas null mais que pas assez de couleurs /rap au public
		{
			if(_uiNeonColors.Length < neonColors.Length)
			{
				_uiNeonColors = new Texture2D[neonColors.Length];
				for(int i=0;i<neonColors.Length;i++)
				{
					Texture2D t = new Texture2D(40,40);
					Color [] tmp = new Color[t.GetPixels().Length];
					for(int c=0;c<t.GetPixels().Length;c++)
					{
						tmp[c] = neonColors[i];
					}
					t.SetPixels(tmp);		
					t.Apply();
					_uiNeonColors[i] = t;
				}
			}
		}
		
		_lightColor = neonColors[_colorId];
		
		fakeLightMat.color = _lightColor;
		paf = origin;
		
		
		if(transform.FindChild("neons"))
			_neons = transform.FindChild("neons").gameObject;
		else
		{
			_neons = new GameObject("neons");
			_neons.transform.parent = parent;
			_neons.transform.localPosition = Vector3.zero;
		}
	}
	
	void OnEnable()
	{
		PergolaAutoFeetEvents.bladesDirChange += UpdateBladesDir;
		PergolaAutoFeetEvents.toggleUIVisibility += ToggleUI;
		PergolaAutoFeetEvents.nightModeChange += UpdateNightMode;
		
	}
	void OnDisable()
	{
		PergolaAutoFeetEvents.bladesDirChange -= UpdateBladesDir;
		PergolaAutoFeetEvents.toggleUIVisibility -= ToggleUI;
		PergolaAutoFeetEvents.nightModeChange -= UpdateNightMode;
	}
	
	public void Build(Vector3 origine,int prefix,Vector4 nsew)
	{
		//Add Fake Neon Light
		GameObject lite = (GameObject)Instantiate(fakeLightRef);
		lite.name = "lite_"+prefix.ToString();
		lite.transform.parent = _neons.transform;
		lite.transform.localPosition = origine + new Vector3(0,paf.GetH()-(3*paf.GetFrameSizeH())/4,0);
		lite.transform.localScale = new Vector3(paf.GetLocalL()-paf.GetFootSize(),1,paf.GetLocalW()-paf.GetFootSize());
		
		//Tile de la texture si poutre intermédiaire
		if(_isLOriented)
		{
			if(paf.GetLocalL()>bladeSizeLimit)
				fakeLightMat.SetTextureScale("_AlphaTex",new Vector2(2,1));
			else
				fakeLightMat.SetTextureScale("_AlphaTex",new Vector2(1,1));
		}
		else
		{
			if(paf.GetLocalW()>bladeSizeLimit)
				fakeLightMat.SetTextureScale("_AlphaTex",new Vector2(1,2));
			else
				fakeLightMat.SetTextureScale("_AlphaTex",new Vector2(1,1));
		}
		
		if(lite.GetComponent<Renderer>())
			lite.GetComponent<Renderer>().material = fakeLightMat;
		else
			lite.transform.GetChild(0).GetComponent<Renderer>().material = fakeLightMat;
		
		
		
		if(lite.GetComponent<Renderer>())
			lite.AddComponent<ApplyShader>();
		else
			lite.transform.GetChild(0).gameObject.AddComponent<ApplyShader>();
		
		UpdateColors();
		UpdateState();
	}
	
	public void GetUI(/*GUISkin skin*/)
	{
		bool tmpui = GUILayout.Toggle(_uiShow,TextManager.GetText("Pergola.NeonLight"),GUILayout.Height(50),GUILayout.Width(280));
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
			bool tmp = GUILayout.Toggle(_switchOnOff,TextManager.GetText((_switchOnOff)? "Eteindre":"Allumer" ),"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			if(tmp != _switchOnOff)
			{
				_switchOnOff = tmp;
				paf.SetFrameMesh(meshRef,!_switchOnOff);
				UpdateState();
			}
			
			bool tmpNight = GUILayout.Toggle(_isNightMode,_uiNightMode,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			if(tmpNight != _isNightMode)
			{
				PergolaAutoFeetEvents.FireNightModeChange(tmpNight);
				UsefullEvents.FireNightMode(_isNightMode);
			}
			
			if(_switchOnOff)
			{
				
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				
				if(GUILayout.Button("","btn<",GUILayout.Width(50),GUILayout.Height(50)))
				{
					_scrollPos --;
					if(_scrollPos < 0)
						_scrollPos = 0;
					
				}
				//----------
				for(int col=_scrollPos;col<_scrollPos+3;col++)
				{
					if(GUILayout.Toggle(col==_colorId,_uiNeonColors[col],"grid",GUILayout.Width(50),GUILayout.Height(50)))
					{
						if(col != _colorId)
						{
							_colorId = col;
							_lightColor = neonColors[_colorId];
							UpdateColors();
						}
					}
				}
		
				//----------
				if(GUILayout.Button("","btn>",GUILayout.Width(50),GUILayout.Height(50)))
				{
					_scrollPos ++;
					if(_scrollPos+3 > _uiNeonColors.Length)
						_scrollPos = _uiNeonColors.Length-3;
				}
				
				GUILayout.Space(20);
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				float tmpIntensity = GUILayout.HorizontalSlider(_intensity,0.2f,1f,GUILayout.Width(200));
				if(tmpIntensity != _intensity)
				{
					_intensity = tmpIntensity;
					UpdateColors();
				}
				GUILayout.Space(20);
				GUILayout.EndHorizontal();
			}
		}
	}
	
	public void Clear()
	{
		foreach(Transform t in _neons.transform)
			Destroy(t.gameObject);
	}
	#endregion
	
	//UpdateColors
	//Mets a jour la couleur des néons(mesh) de chaque poutre hztl
	//et celle du materiau des fakeLight
	private void UpdateColors()
	{
		fakeLightMat.color = _lightColor * (_intensity);
		fakeLightMat.SetFloat("_EmissionLM",_intensity);
			
		foreach(Transform t in paf.GetFrame().transform)
		{
			foreach(Transform child in t)
			{	
				if(child.name == "neonMesh")
				{
					if(child.GetComponent<Renderer>())
						child.GetComponent<Renderer>().material.color = _lightColor * _intensity;
				}
				else
				{
					if(child.GetComponent<Renderer>())
						child.GetComponent<Renderer>().material = paf.GetFrameMat();
				}
			}
		}
	}
	
	//UpdateState
	//Mets a jour l'état affiché/caché
	private void UpdateState()
	{
		foreach(Transform t in _neons.transform)
		{
			if(t.GetComponent<Renderer>())
				t.GetComponent<Renderer>().enabled = _switchOnOff;
			else
				t.GetChild(0).GetComponent<Renderer>().enabled = _switchOnOff;
		}
		
		foreach(Transform t in paf.GetFrame().transform)
		{
			Transform mesh = t.FindChild("neonMesh");
			
			if(mesh)
			{
				if(mesh.GetComponent<Renderer>())
				{
					if(_switchOnOff)
						mesh.GetComponent<Renderer>().material.color = _lightColor;
					else
						mesh.GetComponent<Renderer>().material.color = Color.white;
				}
			}
		}
	}
	
	private void UpdateBladesDir(bool b)
	{
		_isLOriented = b;
	}
	
	public void ToggleUI(string s)
	{
		if(s != GetType().ToString())
			_uiShow = false;
	}
	
	private void UpdateNightMode(bool b)
	{
		_isNightMode = b;
		if(b)
			_uiNightMode = TextManager.GetText("LightPresets.DayMode");
		else
			_uiNightMode = TextManager.GetText("LightPresets.NightMode");
	}
	
	public void SaveOption(BinaryWriter buf)
	{
		buf.Write(_switchOnOff);
		if(_switchOnOff)
		{
			buf.Write(_colorId);
			Debug.Log ("color = "+_colorId);
			buf.Write(_isLOriented);
			buf.Write(_intensity);	//FM
		}
	}
	
	public void LoadOption(BinaryReader buf)
	{
		//Debug.Log (""+cdm.m_lastOpenDate+" opendate");
		_switchOnOff = buf.ReadBoolean();
		if(_switchOnOff)
		{
			_colorId = buf.ReadInt32();

			_isLOriented = buf.ReadBoolean();
			string[] str = cdm.m_lastOpenDate.Split('/');
			if(new DateTime(Convert.ToInt32(str[2]),Convert.ToInt32(str[1]),Convert.ToInt32(str[0])) >= (new DateTime(2015,7,21)))
			{
				_intensity= buf.ReadSingle();	//FM
			}
			Debug.Log("_colorId:"+_colorId);
			_lightColor = neonColors[_colorId];
		}
	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_switchOnOff);
		al.Add(_colorId);
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		_switchOnOff = (bool) al[0];
		_colorId = (int) al[1];
	}
	
//	void AddNeonLights(Vector3 origine,int prefix)
//	{
//		int nbL = Mathf.FloorToInt(paf.GetLocalL()/* - 2*paf.GetFootSize()*/);
//		int nbW = Mathf.FloorToInt(paf.GetLocalW()/* - 2*paf.GetFootSize()*/);
//		
////		if(nbL == 0)
////			nbL = 1;
////		if(nbW == 0)
////			nbW = 1;
//		
//		float startL = -paf.GetLocalL()/2 + (paf.GetLocalL() - (float)(nbL-1))/2f;
//		float startW = -paf.GetLocalW()/2 + (paf.GetLocalW() - (float)(nbW-1))/2f;
//
//		
//		float height = paf.GetH() -0.5f;
//		
//		Vector3 off7;
//		
//		for(int l=0;l<nbL;l++)
//		{
//			off7 = new Vector3(startL + l,height,(paf.GetLocalW()- paf.GetFootSize())/2 - 0.2f);
//			GameObject s1 = (GameObject)Instantiate(refLight);
//			s1.name = prefix.ToString() + "_l"+l+"1";
//			s1.transform.parent = _neons.transform;
//			s1.transform.localPosition = origine + off7;
//			s1.transform.Rotate(new Vector3(320,180,0));
//			
//			
//			off7 = new Vector3(startL + l,height,(paf.GetLocalW()- paf.GetFootSize())/-2 + 0.2f);
//			GameObject s2 = (GameObject)Instantiate(refLight);
//			s2.name = prefix.ToString() + "_l"+l+"2";
//			s2.transform.parent = _neons.transform;
//			s2.transform.localPosition = origine + off7;
//			s2.transform.Rotate(new Vector3(320,0,0));
//			
//		}
//		
//		for(int w=0;w<nbW;w++)
//		{
//			off7 = new Vector3((paf.GetLocalL()- paf.GetFootSize())/2 - 0.2f,height,startW + w);
//			GameObject s1 = (GameObject)Instantiate(refLight);
//			s1.name = prefix.ToString() + "_w"+w+"1";
//			s1.transform.parent = _neons.transform;
//			s1.transform.localPosition = origine + off7;
//			s1.transform.Rotate(new Vector3(320,270,0));
//			
//			
//			off7 = new Vector3((paf.GetLocalL()- paf.GetFootSize())/-2 + 0.2f,height,startW + w);
//			GameObject s2 = (GameObject)Instantiate(refLight);
//			s2.name = prefix.ToString() + "_w"+w+"2";
//			s2.transform.parent = _neons.transform;
//			s2.transform.localPosition = origine + off7;
//			s2.transform.Rotate(new Vector3(320,90,0));
//			
//		}	
//	}
	
//	void AddNeonLightsCorner(Vector3 origine,int prefix)
//	{
//		//Corners
//		float height = paf.GetH() -0.1f;
//		Vector3 off7 = new Vector3(-paf.GetLocalL()/2+paf.GetFootSize()/2,height,paf.GetLocalW()/2-paf.GetFootSize()/2);
	//		GameObject f1 = (GameObject)Instantiate(refLight);
	//		f1.name = prefix.ToString() + "_1";
	//		f1.transform.parent = _neons.transform;
	//		f1.transform.localPosition = origine + off7;
	//		f1.transform.Rotate(new Vector3(0,90+45,0));
//		
//		off7 = new Vector3(paf.GetLocalL()/2-paf.GetFootSize()/2,height,paf.GetLocalW()/2-paf.GetFootSize()/2);
//		GameObject f2 = (GameObject)Instantiate(refLight);
//		f2.name = prefix.ToString() + "_2";
//		f2.transform.parent = _neons.transform;
//		f2.transform.localPosition = origine + off7;
//		f2.transform.Rotate(new Vector3(0,180+45,0));
//		
//		off7 = new Vector3(-paf.GetLocalL()/2+paf.GetFootSize()/2,height,-paf.GetLocalW()/2+paf.GetFootSize()/2);
//		GameObject f3 = (GameObject)Instantiate(refLight);
//		f3.name = prefix.ToString() + "_3";
//		f3.transform.parent = _neons.transform;
//		f3.transform.localPosition = origine + off7;
//		f3.transform.Rotate(new Vector3(0,45,0));
//		
//		off7 = new Vector3(paf.GetLocalL()/2-paf.GetFootSize()/2,height,-paf.GetLocalW()/2+paf.GetFootSize()/2);
//		GameObject f4 = (GameObject)Instantiate(refLight);
//		f4.name = prefix.ToString() + "_4";
//		f4.transform.parent = _neons.transform;
//		f4.transform.localPosition = origine + off7;
//		f4.transform.Rotate(new Vector3(0,-45,0));
//		
//		//sides
//		off7 = new Vector3(-paf.GetLocalL()/2+paf.GetFootSize()/*/2*/,height,0);
//		GameObject f5 = (GameObject)Instantiate(refLight);
//		f5.name = prefix.ToString() + "_5";
//		f5.transform.parent = _neons.transform;
//		f5.transform.localPosition = origine + off7;
//		f5.transform.Rotate(new Vector3(0,90,0));
//		
//		off7 = new Vector3(paf.GetLocalL()/2-paf.GetFootSize()/*/2*/,height,0);
//		GameObject f6 = (GameObject)Instantiate(refLight);
//		f6.name = prefix.ToString() + "_6";
//		f6.transform.parent = _neons.transform;
//		f6.transform.localPosition = origine + off7;
//		f6.transform.Rotate(new Vector3(0,-90,0));
//		
//		off7 = new Vector3(0,height,paf.GetLocalW()/2-paf.GetFootSize()/*/2*/);
//		GameObject f7 = (GameObject)Instantiate(refLight);
//		f7.name = prefix.ToString() + "_7";
//		f7.transform.parent = _neons.transform;
//		f7.transform.localPosition = origine + off7;
//		f7.transform.Rotate(new Vector3(0,180,0));
//		
//		off7 = new Vector3(0,height,-paf.GetLocalW()/2+paf.GetFootSize()/*/2*/);
//		GameObject f8 = (GameObject)Instantiate(refLight);
//		f8.name = prefix.ToString() + "_8";
//		f8.transform.parent = _neons.transform;
//		f8.transform.localPosition = origine + off7;
//		f8.transform.Rotate(new Vector3(0,0,0));
//	}
}
