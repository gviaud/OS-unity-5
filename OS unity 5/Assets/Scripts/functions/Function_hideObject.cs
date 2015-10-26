using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class Function_hideObject : MonoBehaviour, Function_OS3D {
	
	public bool _hide = true;
	public string _nameObjectToHide = "";
	public string _strObjectToHide = "";
	public int id;
	
	private List<Transform> _objectToHide = new List<Transform>();
	private bool _lasteState = false;
	private string _functionName = "Cacher";
	private string _altFunctionName = "Afficher";	
	private string _lowWallName="muret";
	private string _plageName="plage";
	//Btn gauche
	//private Rect _btnHideObj;

	// Use this for initialization
	void Start () {
		if(_objectToHide.Count==0)
		{
		//	_objectToHide = new List<Transform>();
		//	foreach (Transform child in transform)
			Transform[] allChildren = GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				if(_nameObjectToHide.CompareTo(_plageName)==0)
				{
					if(child.name.Contains(_nameObjectToHide))
						_objectToHide.Add(child);	
					if(child.name.Contains(_lowWallName))
						_objectToHide.Add(child);						
				}
				else
				{
					if(child.name.Contains(_nameObjectToHide))
						_objectToHide.Add(child);				
				}
			}
			//_objectToHide = transform.Find(_nameObjectToHide);			
			_lasteState = _hide;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(_objectToHide.Count>0)
		{
		/*	Debug.Log("_hide : "+_hide
			          +"_lasteState : "+_lasteState
			          +"_objectToHide.renderer.enabled : "+_objectToHide.renderer.enabled);*/
			
			foreach (Transform child in _objectToHide)
			{
				if(child && child.GetComponent<Renderer>() && _hide ==child.GetComponent<Renderer>().enabled)
				{
					child.GetComponent<Renderer>().enabled = !_hide;
					_lasteState=_hide;
				}
			}
		}
	}
	public void DoAction ()
	{
		if(_objectToHide.Count>0)
		{
			foreach (Transform child in _objectToHide)
			{
				if(child.GetComponent<Renderer>()!=null)
					child.GetComponent<Renderer>().enabled = _lasteState;
			}	
			_lasteState=!_lasteState;
			_hide=_lasteState;
		}	
	}
	public void DoAction2 ()
	{
		if(_objectToHide.Count>0)
		{
			foreach (Transform child in _objectToHide)
			{
				if(child.GetComponent<Renderer>()!=null)
					child.GetComponent<Renderer>().enabled = true;
			}	
			_lasteState=!_lasteState;
			_hide=_lasteState;
		}	
	}

	public string GetFunctionName()
	{
		if (_lasteState)
			return TextManager.GetText(_altFunctionName);
		else
			return TextManager.GetText(_functionName);
	}
	
	public string GetFunctionParameterName()
	{
		string szobjectToHide = _strObjectToHide == "plage" ? TextManager.GetText(_strObjectToHide) + "/" + TextManager.GetText("PoolResizerUI.LowWall") : TextManager.GetText(_strObjectToHide);
		return GetFunctionName()+" "+szobjectToHide;
	}
	
	public int GetFunctionId()
	{
		return id;
	}
	
	public void setUI(FunctionUI_OS3D ui)
	{
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}
	//SAVE/LOAD
	
	public void save(BinaryWriter buf)
	{
//		Debug.Log(!_lasteState + " _ " + _nameObjectToHide + " _ " + _strObjectToHide);
		buf.Write(!_lasteState);
		buf.Write(_nameObjectToHide);
		buf.Write(_strObjectToHide);
	}
	
	public void load(BinaryReader buf)
	{
		_lasteState = buf.ReadBoolean();
		_nameObjectToHide = buf.ReadString();
		_strObjectToHide = buf.ReadString();
		//_objectToHide = transform.Find(_nameObjectToHide);
		_objectToHide = new List<Transform>();
		if(_objectToHide.Count==0)
		{
		//	_objectToHide = new List<Transform>();
		//	foreach (Transform child in transform)
			Transform[] allChildren = GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				if(_nameObjectToHide.CompareTo(_plageName)==0)
				{
					if(child.name.Contains(_nameObjectToHide))
						_objectToHide.Add(child);	
					if(child.name.Contains(_lowWallName))
						_objectToHide.Add(child);						
				}
				else
				{
					if(child.name.Contains(_nameObjectToHide))
						_objectToHide.Add(child);				
				}
			}
			//_objectToHide = transform.Find(_nameObjectToHide);			
		//	_lasteState = _hide;
		}
		DoAction();
	}
	
	public ArrayList getConfig()
	{
		ArrayList list = new ArrayList();
		list.Add(_nameObjectToHide);
		list.Add(_hide);
		return list;
	}
	
	public void setConfig(ArrayList config)
	{
		/*_objectToHide.transform.FindChild((string)config[0]);
		
		if(_objectToHide.Count==0)
		{
			foreach (Transform child in transform)
			{
				if(child.name.StartsWith(_nameObjectToHide))
					_objectToHide.Add(child);
			}
		}*/
		
		_nameObjectToHide = (string)config[0];
		_hide = (bool)config[1];
		_lasteState = _hide;
		//DoAction();
	}
	
	public void SetObjectToHide (List<Transform> objectToHide)
	{
		_objectToHide = objectToHide;	
	}
		
}
