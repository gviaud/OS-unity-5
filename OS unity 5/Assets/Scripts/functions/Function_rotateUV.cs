using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Function_rotateUV : MonoBehaviour, Function_OS3D {
	
	public float m_fstep = 0.0f;
	
	public string _nameObjectToRotateUV = "";
	
	private string _functionName = "Tourner plage de 45°";
	public int id;
	
	private List<Transform> _objectToRotateUV = new List<Transform>();
	
	private GUIItemV2 guiItem = null;
	
	// Use this for initialization
	void Start () {
	
		if(_objectToRotateUV.Count == 0)
		{
			Transform[] allChildren = GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				if(child.name.Contains(_nameObjectToRotateUV))
				{
					_objectToRotateUV.Add(child);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		if(guiItem != null)
		{
			foreach(Function_hideObject f_ho in transform.GetComponents(typeof(Function_hideObject)))
			{
				if(f_ho._nameObjectToHide == "plage")
				{
					if(f_ho.GetFunctionName() == "Cacher")
					{
						guiItem.SetEnableUI(true);
					}
					else
					{
						guiItem.SetEnableUI(false);
					}	
				}
			}
		}
	}
	
	public void DoAction ()
	{
		if(_objectToRotateUV.Count > 0)
		{
			m_fstep += 0.25f;
			
			if(m_fstep > 0.75f)
			{
				m_fstep = 0.0f;
			}
			
			foreach (Transform child in _objectToRotateUV)
			{
				child.GetComponent<Renderer>().material.SetFloat("_Rotation", Mathf.PI * m_fstep);
			}
		}	
	}
	
	public string GetFunctionName()
	{
		return TextManager.GetText(_functionName);
	}
	
	public string GetFunctionParameterName()
	{
		return GetFunctionName();
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
		guiItem = _guiItem;
	}
	
	public void save(BinaryWriter buf)
	{
		buf.Write((double)m_fstep);
	}
	
	public void load(BinaryReader buf)
	{
		m_fstep = (float)buf.ReadDouble();
		
		if(_objectToRotateUV.Count == 0)
		{
			Transform[] allChildren = GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				if(child.name.Contains(_nameObjectToRotateUV))
				{
					_objectToRotateUV.Add(child);
					child.GetComponent<Renderer>().material.SetFloat("_Rotation", Mathf.PI * m_fstep);
				}
			}
		}
	}
	
	public ArrayList getConfig()
	{
		ArrayList list = new ArrayList();
		return list;
	}
	
	public void setConfig(ArrayList config)
	{
	}
}
