using UnityEngine;
using System.Collections;

public class CamSwitcher : MonoBehaviour
{
	public Camera sndCam;
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()
	{
		if(GUI.Button(new Rect(Screen.width-160,10,150,30),"switchCam"))
		{
			this.GetComponent<Camera>().enabled = !this.GetComponent<Camera>().enabled;
			sndCam.enabled = !sndCam.enabled;
		}
	}
}
