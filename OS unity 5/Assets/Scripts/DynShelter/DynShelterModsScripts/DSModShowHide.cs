using UnityEngine;
using System.Collections;
using Pointcube.Global;
using System.IO;

public class DSModShowHide : MonoBehaviour,IDSMod
{
	public bool 		isGlobal;
	public bool			isIntegrated = false;
	public int 			id;
	
	public string[]		hideNames;
	
	public string		uiRefName;
	public string		stateOn,stateOff;
	
	public bool 		DefaultValue;
	
	//-------------------------
	private static readonly string DEBUGTAG = "DSModShowHide : ";
	private string 	_hashTag;
	
	private ArrayList _toHide;
	
	private bool _state;//open>true, close>false
	
	private DynShelterModManager _modsManager;
	
	private string go;
	
	//-------------------------
	
	void Awake()
	{
		_hashTag = GetType().ToString()+"_"+(isGlobal? "G":"L")+"_"+id.ToString();
		
		//Errors
		if(hideNames.Length == 0)
			Debug.LogError(DEBUGTAG +"hideName"+PC.MISSING_REF);
		
		//INIT
		_state = DefaultValue;
		_toHide = new ArrayList();
		foreach(Transform t in GetComponentsInChildren<Transform>())
		{
			string nme = t.name;
			bool addIt = false;
			foreach(string s in hideNames)
			{
				if(s == nme)
					addIt = true;
			}
			
			if(addIt)
				_toHide.Add(t.gameObject);
		}
		
		
		UpdateState();
		go = gameObject.name;
	}
	
	//-------------------------
	
	private void UpdateState()
	{
		foreach(GameObject g in _toHide)
		{
			g.SetActive(_state);	
		}
	}
	
	//-------------------------
	
	#region Interface's fcns.
	public string GetGameObj()
	{
		return gameObject.name;
	}
	
	public void GetModUI()
	{
		//Sélecteur d'options (Droite)
		if(GUILayout.Button((_state? 
			TextManager.GetText(stateOff):TextManager.GetText(stateOn))+" "+TextManager.GetText(uiRefName),"OutilWithoutIcon",GUILayout.Height(50),GUILayout.Width(260)))
		{
			_state = !_state;
			if(isGlobal)
				SetToAll();
			else
				UpdateState();
		}
	}
	
	public void SetToAll(bool reset = false)
	{
		ArrayList conf = new ArrayList();
		conf.Add(_state);	//0
		
		_modsManager.ApplyGlobal(_hashTag,conf,reset);
	}
	
	public void SetModManger(DynShelterModManager mgr)
	{
		_modsManager = mgr;	
	}
	
	public void ApplyConf(ArrayList conf,bool reset = false)
	{
		_state = 	(bool) 	conf[0];
		UpdateState();
	}
	
	public string GetHashTag(){return _hashTag;}
	
	public bool IsGlobalMod(){return isGlobal;}
	public bool IsIntegrated(){return isIntegrated;}
	
	public string SaveConf ()
	{
		string s;
		s = _state.ToString();
		return s;
	}
	
	public void LoadConf(string conf)
	{
		_state = bool.Parse(conf);
		UpdateState();
	}
	#endregion
}
