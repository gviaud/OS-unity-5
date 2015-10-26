using UnityEngine;
using System.Collections;
using Pointcube.Global;
using System.IO;

public class DSModOpennings : MonoBehaviour,IDSMod
{
	public bool 		isGlobal;
	public bool			isIntegrated = false;
	public int 			id;
	
	public string		openName;
	public string		closeName;
	
	public string		uiRefName;
	public string		uiOpenRefName;
	public string		uiCloseRefName;
	
	//-------------------------
	private static readonly string DEBUGTAG = "DSModOpennings : ";
	private string 	_hashTag;
	
	private ArrayList _stateOpen;
	private ArrayList _stateClosed;
	
	private bool _state;//open>true, close>false
	
	private DynShelterModManager _modsManager;
	
	private string go;
	
	//-------------------------
	
	void Awake()
	{
		_hashTag = GetType().ToString()+"_"+(isGlobal? "G":"L")+"_"+id.ToString();
		
		//Errors
		if(openName == "")
			Debug.LogError(DEBUGTAG +"openName"+PC.MISSING_REF);
		
		if(closeName == "")
			Debug.LogError(DEBUGTAG +"closeName"+PC.MISSING_REF);
		
		//INIT
		_stateOpen = new ArrayList();
		_stateClosed = new ArrayList();
		_state = false;
		foreach(Transform t in GetComponentsInChildren<Transform>(true))
		{
			if(t.name == openName)
			{
				_stateOpen.Add(t.gameObject);
			}
			
			if(t.name == closeName)
				_stateClosed.Add(t.gameObject);
		}
		
		UpdateState();
		
		go = gameObject.name;
	}
	
	//-------------------------
	
	private void UpdateState()
	{
		
		foreach(GameObject g in _stateOpen)
		{
			g.SetActive(_state);	
		}
		
		foreach(GameObject g in _stateClosed)
		{
			g.SetActive(!_state);	
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
		if(GUILayout.Button((_state? TextManager.GetText(uiCloseRefName):TextManager.GetText(uiOpenRefName)),"OutilWithoutIcon",GUILayout.Height(50),GUILayout.Width(260)))
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
