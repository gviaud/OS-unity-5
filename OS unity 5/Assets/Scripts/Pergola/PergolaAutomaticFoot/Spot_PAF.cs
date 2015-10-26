using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class Spot_PAF : MonoBehaviour,IPergolaAutoFeet
{
	
	public GameObject meshRef;				//Objet réference a instancier
	public static ContextDataModel cdm = new ContextDataModel();
	private bool _hasSpots = false;			//spots ou pas
	private bool _switch = false;
	private bool _uiShow = false;			//Affichage de l UI
	private bool _isNightMode = false;		//mode nuit
	
	private float _intensity = 0.75f;
	
	private GameObject _spots;				//GameObject Parent
	
	private FunctionConf_PergolaAutoFeet paf;	//script parent
	
	private string _uiNightMode;			//txt du btn mode nuit
	
	//----------vv Functions vv-----------------------------
	
	void OnEnable()
	{
		UsefullEvents.ScaleChange += UpdateScale;
		UsefullEvents.NightMode += UpdateNightMode;
		PergolaAutoFeetEvents.toggleUIVisibility += ToggleUI;
		PergolaAutoFeetEvents.nightModeChange += UpdateNightMode;
	}
	void OnDisable()
	{
		UsefullEvents.ScaleChange -= UpdateScale;
		UsefullEvents.NightMode -= UpdateNightMode;
		PergolaAutoFeetEvents.toggleUIVisibility -= ToggleUI;
		PergolaAutoFeetEvents.nightModeChange -= UpdateNightMode;
	}
	
	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{
//		_isNightMode = UsefulFunctions.GetMontage().GetSM().IsNightMode();
		
		paf = origin;
		
		if(transform.FindChild("spots"))
			_spots = transform.FindChild("spots").gameObject;
		else
		{
			_spots = new GameObject("spots");
			_spots.transform.parent = parent;
			_spots.transform.localPosition = Vector3.zero;
		}
		
		
	}
	
	public void Build(Vector3 origine,int prefix,Vector4 nsew)
	{
		if(!_hasSpots)
			return;

		int nbL = Mathf.FloorToInt(paf.GetLocalL()/* - 2*pbe.GetFootSize()*/);
		int nbW = Mathf.FloorToInt(paf.GetLocalW()/* - 2*pbe.GetFootSize()*/);
		
		float startL = -paf.GetLocalL()/2 + (paf.GetLocalL() - (float)(nbL-1))/2f;
		float startW = -paf.GetLocalW()/2 + (paf.GetLocalW() - (float)(nbW-1))/2f;
		float height = paf.GetH() - paf.GetFrameSizeH();
		
		Vector3 off7;
		
		for(int l=0;l<nbL;l++)
		{
			off7 = new Vector3(startL + l,height,(paf.GetLocalW()- paf.GetFootSize())/2);
			GameObject s1 = (GameObject)Instantiate(meshRef);
			s1.name = prefix.ToString() + "_l"+l+"1";
			s1.transform.parent = _spots.transform;
			s1.transform.localPosition = origine + off7;
//			s1.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
			
			off7 = new Vector3(startL + l,height,(paf.GetLocalW()- paf.GetFootSize())/-2);
			GameObject s2 = (GameObject)Instantiate(meshRef);
			s2.name = prefix.ToString() + "_l"+l+"2";
			s2.transform.parent = _spots.transform;
			s2.transform.localPosition = origine + off7;
//			s2.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		}
		
		for(int w=0;w<nbW;w++)
		{
			off7 = new Vector3((paf.GetLocalL()- paf.GetFootSize())/2,height,startW + w);
			GameObject s1 = (GameObject)Instantiate(meshRef);
			s1.name = prefix.ToString() + "_w"+w+"1";
			s1.transform.parent = _spots.transform;
			s1.transform.localPosition = origine + off7;
//			s1.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
			
			off7 = new Vector3((paf.GetLocalL()- paf.GetFootSize())/-2,height,startW + w);
			GameObject s2 = (GameObject)Instantiate(meshRef);
			s2.name = prefix.ToString() + "_w"+w+"2";
			s2.transform.parent = _spots.transform;
			s2.transform.localPosition = origine + off7;
//			s2.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		}
		UpdateScale();
		UpdateLight(true,true);
	}
	
	public void GetUI(/*GUISkin skin*/)
	{
		bool tmpui = GUILayout.Toggle(_uiShow,TextManager.GetText("Pergola.SpotLight"),GUILayout.Height(50),GUILayout.Width(280));
		if(tmpui != _uiShow)
		{
			_uiShow = tmpui;
			if(_uiShow)
				PergolaAutoFeetEvents.FireToggleUIVisibility(GetType().ToString());
		}
		
		if(_uiShow)
		{
			
			bool tmp = GUILayout.Toggle(_hasSpots,TextManager.GetText((_hasSpots)? "Cacher":"Afficher" ),"toggle2"
				,GUILayout.Height(50),GUILayout.Width(280));
			if(tmp != _hasSpots)
			{
				_hasSpots = tmp;
				PergolaAutoFeetEvents.FireNeedScreenExtension(_hasSpots);
				PergolaAutoFeetEvents.FireRebuild();
			}
			
			if(_hasSpots)
			{
				bool tmpOn = GUILayout.Toggle(_switch,TextManager.GetText((_switch)? "Eteindre":"Allumer" ),"toggle2"
				,GUILayout.Height(50),GUILayout.Width(280));
				
				if(tmpOn != _switch)
				{
					_switch = tmpOn;
					PergolaAutoFeetEvents.FireNightModeChange(_switch);
					UsefullEvents.FireNightMode(_switch);
				}	
			}
			
			if(_switch)
			{
				GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
				GUILayout.FlexibleSpace();
				float tmpIntensity = GUILayout.HorizontalSlider(_intensity,0f,1.5f,GUILayout.Width(200));
				if(tmpIntensity != _intensity)
				{
					_intensity = tmpIntensity;
					UpdateLight(false,true);
				}
				GUILayout.Space(20);
				GUILayout.EndHorizontal();
			}
			
//			bool tmpNight = GUILayout.Toggle(_isNightMode,_uiNightMode,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
//			if(tmpNight != _isNightMode)
//			{
////				_isNightMode = tmpNight;
//				PergolaAutoFeetEvents.FireNightModeChange(tmpNight);
//				UsefullEvents.FireNightMode(_isNightMode);
//			}
		}
	}
	
	public void Clear()
	{
		foreach(Transform t in _spots.transform)
			Destroy(t.gameObject);
	}
	
	private void UpdateScale()
	{	
		foreach(Transform spot in _spots.transform)
		{
			spot.GetComponentInChildren<Light>().range = paf.GetH()*1.25f* Montage.sm.getCamData().m_s;//scl.x * 3;
		}
	}
	
	private void UpdateLight(bool switchOnOff, bool intensity)
	{
		foreach(Transform spot in _spots.transform)
		{
			if(switchOnOff)
			{
				spot.GetComponentInChildren<Light>().enabled = _isNightMode;
				Color sw;
				
				if(_isNightMode)
					sw = new Color(0.25f+_intensity/2,0.25f+_intensity/2,0.25f+_intensity/2);
				else 
					sw = new Color(0.25f,0.25f,0.25f);
				
				spot.GetComponent<Renderer>().materials[spot.GetComponent<Renderer>().materials.Length-2].color = sw;
			}
			if(intensity && _isNightMode)
			{
				spot.GetComponent<Renderer>().materials[spot.GetComponent<Renderer>().materials.Length-2].color = new Color(0.25f+_intensity/2,0.25f+_intensity/2,0.25f+_intensity/2);
				spot.GetComponentInChildren<Light>().spotAngle = 140;
				spot.GetComponentInChildren<Light>().intensity = 1+_intensity;
				spot.GetComponentInChildren<Light>().color = new Color(_intensity/2,_intensity/2,_intensity/2);
			}
		}
	}
	
	public void ToggleUI(string s)
	{
		if(s != GetType().ToString())
			_uiShow = false;
	}
	
	private void UpdateNightMode(bool b)
	{
		_isNightMode = b;
		_switch = b;
		UpdateLight(true,true);
		if(b)
			_uiNightMode = TextManager.GetText("LightPresets.DayMode");
		else
			_uiNightMode = TextManager.GetText("LightPresets.NightMode");
	}
	
	public void SaveOption(BinaryWriter buf)
	{
		buf.Write(_hasSpots);
	}
	public void LoadOption(BinaryReader buf)
	{
		_hasSpots = buf.ReadBoolean();

	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_hasSpots);
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		_hasSpots = (bool) al[0];
	}
	
}
