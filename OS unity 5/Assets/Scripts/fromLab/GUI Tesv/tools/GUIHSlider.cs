using UnityEngine;
using System.Collections;

public class GUIHSlider : GUIItemV2
{
	protected float _minValue = 0;
	protected float _maxValue;
	protected float _value;
	
	public GUIHSlider (int depth,int actionId,string bOn,string bOf/*,GUIStyle iOn,GUIStyle iOf*/,GUIInterface o) 
		: base (depth, actionId, "", bOn, bOf, o)
	{}
		
	public override bool getUI(bool activ)
	{
		//string max = (Mathf.Round (_maxValue * 10) / 10).ToString ();
		string val = (Mathf.Round (_value * 100) / 100).ToString () + "m";
			
		GUILayout.BeginHorizontal(bgOff, GUILayout.Width(260), GUILayout.Height(50));
		GUILayout.FlexibleSpace ();
		
		//GUILayout.Label ("0", GUILayout.Width(10), GUILayout.Height(50));
		
		float tmpValue = GUILayout.HorizontalSlider (_value, _minValue, _maxValue, GUILayout.Height(50),GUILayout.Width(180));
		GUILayout.Space (5);
		
		GUILayout.Label (val, GUILayout.Width(50), GUILayout.Height(50));
		GUILayout.Space (5);
		
		GUILayout.EndHorizontal();
		
		if (tmpValue != _value)
		{
			_value = tmpValue;
		}
		
		return false;
	}
	
	public override bool getUI(int w, int h)
	{
		float tmpValue = GUILayout.HorizontalSlider (_value, _minValue, _maxValue, GUILayout.Height(h),GUILayout.Width(w));
		
		if (tmpValue != _value)
		{
			_value = tmpValue;
		}
		return false;
	}
	
	public float GetValue ()
	{
		return _value;	
	}
	
	public void SetValue (float val)
	{
		_value = val;
	}
	
	public float GetMinValue ()
	{
		return _minValue;	
	}
	
	public void SetMinValue (float val)
	{
		_minValue = val;
	}
	
	public float GetMaxValue ()
	{
		return _maxValue;	
	}
	
	public void SetMaxValue (float val)
	{
		_maxValue = val;
	}
}
