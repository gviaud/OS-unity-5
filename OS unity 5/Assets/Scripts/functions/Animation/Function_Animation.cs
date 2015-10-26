using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Function_Animation : MonoBehaviour, Function_OS3D {
	
	string _functionName = "ReglageOuverture";
	
	public string _uiName;	
	protected FunctionUI_OS3D _ui;
	
	float vValue=0.0f;
	
	public int id;
	
	private AnimationController _controller;
	
	// Use this for initialization
	void Start () {
		/*foreach (AnimationState state in animation) {
			_anims.Add(state);
        }*/	
	}
	void Awake ()
	{
		_controller = transform.gameObject.AddComponent<AnimationController> ();
		_controller.SetPoolObject(transform);
		//if(_controller!=null)
		//	_controller.UpdateValue(vValue);
		if(_uiName!=null)
		setUI(
			(FunctionUI_OS3D)GameObject.Find("MainScene").GetComponent(_uiName) );	
		
	}
	
	public void DoAction () {
		if(_ui!=null)
		{
			_ui.DoActionUI(gameObject);
		}
        //hSliderValue = GUI.HorizontalSlider(new Rect(25, 25, 500, 30), hSliderValue, 0.0F, 1.0F);
	}	
	
	public void setUI(FunctionUI_OS3D ui)
	{
		_ui = ui;
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}
	
	void Update ()
	{
		
	}
		
	public string GetFunctionName()
	{
		return GetFunctionParameterName();
	}
	
	public string GetFunctionParameterName()
	{
		return " "+TextManager.GetText(_functionName);
	}
	
	public int GetFunctionId()
	{
		return id;
	}
		
	//  sauvegarde/chargement
	
	public void save(BinaryWriter buf)
	{	
		float valueTemp = 0.0f;
		if(_controller!=null)
			valueTemp=_controller.GetValue();
		buf.Write((double)valueTemp);
	}
	
	public void load(BinaryReader buf)
	{
		float valueTemp = (float) buf.ReadDouble();
		if(_controller!=null)
			_controller.UpdateValue(valueTemp);
	}
	
	
	public ArrayList getConfig()
	{
		float valueTemp = 0.0f;
		if(_controller!=null)
			valueTemp=_controller.GetValue();
		ArrayList list = new ArrayList();
		list.Add(valueTemp);
		return list;
	}
		
	public void setConfig(ArrayList config)
	{
		float valueTemp = (float)config[0];
		if(_controller!=null)
			_controller.UpdateValue(valueTemp);
	}
	
}
