using UnityEngine;
using System.Collections;

public class GridControl: MonoBehaviour {
	
	GameObject camPivot;
	GameObject cam;
	bool isActive = false;
	
	private CameraFrustum _cameraFrustrum;
	
	// Use this for initialization
	void Start () {
		camPivot = GameObject.Find("camPivot");
		_cameraFrustrum = GameObject.Find("mainCam").GetComponent<CameraFrustum>();
	}
	
	// Update is called once per frame
	void Update () 
	{
//		if(Input.GetKey(KeyCode.G))
//		{
//			StartCoroutine("showHideGrid");
//		}
//		if(Input.GetKey(KeyCode.R))
//		{
//			reinitGrid();
//		}
	}
	
	public IEnumerator showHideGrid()
	{
		if(isActive == false)
		{
			isActive = true;
			bool state = GetComponent<Renderer>().enabled;
			GetComponent<Renderer>().enabled = !state;
			yield return new WaitForSeconds(0.2f);
			isActive = false;
			yield return null;
		}
		else
		{
			yield return null;	
		}
	}
	
	public void reinitGrid()
	{
		camPivot.transform.position = new Vector3(0,0,0);
		Camera.main.transform.localPosition = new Vector3(0,10,-40);
		camPivot.GetComponent<SceneControl>().init();
		camPivot.transform.rotation = new Quaternion(0,0,0,0);
		if(_cameraFrustrum!=null)
		{
			if(_cameraFrustrum.enabled)
			{
				_cameraFrustrum.SetFovX(60);
			}
			else
			{
				Camera.main.fieldOfView = 60;		
			}
		}
		else
		{
			Camera.main.fieldOfView = 60;		
		}
	}
}
