using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class Function_switchLights : MonoBehaviour, Function_OS3D {
	
	private bool _switchedON = false;

	private List<Light> _lights = new List<Light>();
	
	private string _functionName = "AllumerLampes";
	private string _altFunctionName = "EteindreLampes";

	public int id;
	
	// Use this for initialization
	void Start ()
	{
		foreach(Transform t in transform)
		{
			if(t.GetComponent<Light>())
			{
				t.GetComponent<Light>().enabled = _switchedON;
				_lights.Add(t.GetComponent<Light>());
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{	
	
	}
	public void DoAction ()
	{
		_switchedON = !_switchedON;
		updateLights();
	}

	public string GetFunctionName()
	{
		if (_switchedON)//si allumées
			return TextManager.GetText(_altFunctionName);
		else
			return TextManager.GetText(_functionName);
	}
	
	public string GetFunctionParameterName()
	{
		return GetFunctionName();//+" "+_strObjectToHide;
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
		buf.Write(_switchedON);
	}
	
	public void load(BinaryReader buf)
	{
		_switchedON = buf.ReadBoolean();
		updateLights();
	}
	
	public ArrayList getConfig()
	{
		ArrayList al = new ArrayList();
		al.Add(_switchedON);
		return al;
	}
	
	public void setConfig(ArrayList config)
	{
		_switchedON = (bool)config[0];
		updateLights();
	}
	
	void updateLights ()
	{
		foreach(Light l in _lights)
		{
			l.enabled = _switchedON;
		}	
	}
}
