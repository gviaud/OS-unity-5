using UnityEngine;
using System.Collections;

public class GUIAutoItem : GUIItemV2
{
//	public string text;
	
	int itmW = 260;
	int itmH = 50;
	
	//protected string bgOn;
	//protected string bgOff;
	
	//protected GUIContent gc;
	
	//protected GUIInterface origine;
	
	public event System.Action doEvent;
	
	// CONSTRUCTOR
	public GUIAutoItem(int d,string inTxt,string bOn,string bOf/*,GUIStyle iOn,GUIStyle iOf*/,GUIInterface o)
		:base(d,-1,inTxt,bOn,bOf,o)
	{
		m_depth = d;
		m_text = inTxt;
		
		bgOn = bOn;
		bgOff = bOf;
//		icoOn = iOn.normal.background;
//		icoOff = iOf.normal.background;
		origine = o;
	}
	
	//fcns sub items
	//externes

	//retourne la profondeur de l'item : root = -1, menu =0, sousmenu = 1, ...
	/*public int getDepth()
	{
		return depth;	
	}*/
	
	//UI
	void doAction()
	{
		if(doEvent != null)
			doEvent();
	}
	//Affichage de l'item (this)
	public override bool getUI(bool activ)
	{
		if(GUILayout.Button(m_text,bgOn,GUILayout.Height(50),GUILayout.Width(260)))
		{
			doAction();
		}
		return false;
	}
	
	public override bool getUI(int w,int h)
	{
		if(GUILayout.Button(m_text,bgOn,GUILayout.Height(h),GUILayout.Width(w)))
		{
			doAction();
		}
		return false;
	}
}
