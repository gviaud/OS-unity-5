using UnityEngine;
using System.Collections;

public class GUICheckbox : GUIItemV2
{
	protected bool _value;
	
	public GUICheckbox (int depth,int actionId,string inTxt,string bOn,string bOf/*,GUIStyle iOn,GUIStyle iOf*/,GUIInterface o) 
		: base (depth, actionId, inTxt, bOn, bOf, o)
	{
		SetToogleActivated(true);
	}
	
	public override bool getUI(bool activ)
	{
		GUILayout.BeginHorizontal(bgOff, GUILayout.Width(260), GUILayout.Height(50));
		bool tmpValue = GUILayout.Toggle (_value, m_text, GUILayout.Height(50),GUILayout.Width(260));
		GUILayout.EndHorizontal();
		
		if (tmpValue != _value)
		{
			_value = tmpValue;
			return true;
		}
		
		return false;
	}
	
	public bool GetValue ()
	{
		return _value;	
	}
	
	public void SetValue (bool val)
	{
		_value = val;	
	}
}
