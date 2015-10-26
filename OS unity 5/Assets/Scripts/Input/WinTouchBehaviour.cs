using UnityEngine;
using System.Collections;
using Pointcube.Global;
using Pointcube.InputEvents;

public class WinTouchBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	void OnGUI ()
	{
		
		//GUI.Label (new Rect(300,300,300,300),"using touch :"+((WinTouchInput)PC.In).m_usingTouch);
		
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_STANDALONE_WIN
		if(PC.In!=null && PC.In.TouchSupported()){
			//GUI.Label (new Rect(300,400,300,300),"wintouch updated");
			((WinTouchInput)PC.In).Update();
		}
#endif
	
	}
}
