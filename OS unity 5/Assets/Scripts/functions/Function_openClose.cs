using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class Function_openClose : MonoBehaviour, Function_OS3D {
	
	public bool _open = false;
	private string _nameObjectToOpen = "Open";
	private string _nameObjectToClose = "Close";
	public string _strObjectToHide = "";
	public int id;
	
	private List<Transform> _objectToOpen = new List<Transform>();
	private List<Transform> _objectToClose = new List<Transform>();
	private bool _lasteState = false;
	private string _functionName = "Ouvrir";
	private string _altFunctionName = "Fermer";	

	// Use this for initialization
	void Start () {
		if(_objectToOpen.Count==0)
		{
			Transform[] allChildren = GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				if(child.name.Contains(_nameObjectToOpen))
					_objectToOpen.Add(child);	
			}					
			_lasteState = _open;
		}
		if(_objectToClose.Count==0)
		{
			Transform[] allChildren = GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				if(child.name.Contains(_nameObjectToClose))
					_objectToClose.Add(child);	
			}					
			_lasteState = _open;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(_objectToOpen.Count>0)
		{			
			foreach (Transform child in _objectToOpen)
			{
				if(_open && !child.GetComponent<Renderer>().enabled)
				{
					child.GetComponent<Renderer>().enabled = true;
					_lasteState=_open;
				}
				else if(!_open && child.GetComponent<Renderer>().enabled)
				{
					child.GetComponent<Renderer>().enabled = false;
					_lasteState=_open;
				}
			}
		}
		if(_objectToClose.Count>0)
		{			
			foreach (Transform child in _objectToClose)
			{
				if(_open && child.GetComponent<Renderer>().enabled)
				{
					child.GetComponent<Renderer>().enabled = false;
					_lasteState=_open;
				}
				else if(!_open && !child.GetComponent<Renderer>().enabled)
				{
					child.GetComponent<Renderer>().enabled = true;
					_lasteState=_open;
				}
			}
		}	
	
	}
	public void DoAction ()
	{
		if(_objectToOpen.Count>0)
		{
			foreach (Transform child in _objectToOpen)
			{
				if(child.GetComponent<Renderer>()!=null)
					child.GetComponent<Renderer>().enabled = !_lasteState;
			}
		}
		if(_objectToClose.Count>0)
		{
			foreach (Transform child in _objectToClose)
			{
				if(child.GetComponent<Renderer>()!=null)
					child.GetComponent<Renderer>().enabled = _lasteState;
			}	
		}	
		_lasteState=!_lasteState;
		_open=_lasteState;
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
		return GetFunctionName()+" "+TextManager.GetText(_strObjectToHide);
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
		buf.Write(_nameObjectToOpen);
		buf.Write(_nameObjectToClose);
		buf.Write(_strObjectToHide);
	}
	
	public void load(BinaryReader buf)
	{
		_lasteState = buf.ReadBoolean();
		_nameObjectToOpen = buf.ReadString();
		_nameObjectToClose = buf.ReadString();
		_strObjectToHide = buf.ReadString();
		_objectToClose = new List<Transform>();
		if(_objectToClose.Count==0)
		{
			Transform[] allChildren = GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				if(child.name.Contains(_nameObjectToClose))
					_objectToClose.Add(child);	
			}
		}
		_objectToOpen = new List<Transform>();
		if(_objectToOpen.Count==0)
		{
			Transform[] allChildren = GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				if(child.name.Contains(_nameObjectToOpen))
					_objectToOpen.Add(child);	
			}
		}
		DoAction();
	}
	
	public ArrayList getConfig()
	{
		ArrayList list = new ArrayList();
		list.Add(_nameObjectToOpen);
		list.Add(_nameObjectToClose);
		list.Add(_open);
		return list;
	}
	
	public void setConfig(ArrayList config)
	{	
		_nameObjectToOpen = (string)config[0];
		_nameObjectToClose = (string)config[1];
		_open = (bool)config[2];
		_lasteState = _open;
		//DoAction();
	}
	
	/*public void SetObjectToHide (List<Transform> objectToOpen)
	{
		_objectToOpen = objectToOpen;	
	}	
	public void SetObjectToClose (List<Transform> objectToClose)
	{
		_objectToClose = objectToClose;	
	}*/
		
}
