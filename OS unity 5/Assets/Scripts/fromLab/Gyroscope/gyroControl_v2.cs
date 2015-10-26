using UnityEngine;
using System.Collections;
using Pointcube.Global;

public class gyroControl_v2 : MonoBehaviour {
	
	//script à attacher au camPivot
	
	bool _debug = false;
	
	bool gyroActive;
	
	Quaternion rotationFix;
	Quaternion originalRot;
	
	Gyroscope gyro;
	
	GameObject gyroPivot;
	
	public GameObject _avatar;
	
	Transform originalParent;
	
    private static readonly string DEBUGTAG = "gyroControl_v2 : ";
	
	// Use this for initialization
	void Start ()
	{ 		
        if(_avatar == null) Debug.LogError(DEBUGTAG+"BackGrid"+PC.MISSING_REF);
		originalRot = transform.rotation;
		gyroActive = SystemInfo.supportsGyroscope;
		if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
			gyroActive = true;
        Debug.Log(" GYROACTIVE = " + gyroActive);
		initGyro();
	}
	
	// Update is called once per frame
	void Update () 
	{
		gyroActive=true;
		if(gyroActive &&gyro!=null)
		{
			Quaternion gyroRotation = gyro.attitude * rotationFix;
#if UNITY_IPHONE
			if(Screen.orientation == ScreenOrientation.LandscapeRight)
			{
				Vector3 tmp = gyroRotation.eulerAngles;
				Quaternion newRot = new Quaternion(0,0,0,0);
				newRot.eulerAngles = new Vector3(-tmp.x,-tmp.y,tmp.z);
				transform.localRotation = newRot;
			}
			else
			{
				transform.localRotation = gyroRotation;
				Vector3 tmp = gyroRotation.eulerAngles;
			}
#elif UNITY_ANDROID
			if(Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
			{				
				Vector3 tmp = gyroRotation.eulerAngles;
				Quaternion newRot = new Quaternion(0,0,0,0);
				newRot.eulerAngles = new Vector3(tmp.x,tmp.y,tmp.z);
				transform.localRotation = newRot;
				
			}

#endif
		}
	}
	
	void OnEnable()
	{
		transform.rotation = originalRot;
//		if(gyroPivot)
//			gyroPivot.transform.rotation = originalRot;
		
		initGyro();
	}
	
	void OnDisable()
	{
		gyro.enabled = false;
//		originalParent.transform.rotation = gyroPivot.transform.rotation;
		transform.parent = originalParent;
	/*	if ((transform.localRotation.z>90) &&  (transform.localRotation.z<270))
		{
			Quaternion rotaQuat = Quaternion.Euler(0,0,180);
			transform.localRotation = transform.localRotatio * rotaQuat;
		}*/
		if(gyroPivot)
			Destroy(gyroPivot);
		
		GetComponent<SceneControl>().setLIPH(true);
		
		Vector3 tmp = transform.eulerAngles;
		/*Quaternion newRot = new Quaternion(0,0,0,0);
		newRot.eulerAngles = new Vector3(0,tmp.y,0);
		_avatar.transform.localRotation = newRot;*/
	}
	
	void OnGUI()
	{
		if(_debug)
		{
			GUI.Label(new Rect(200,100,300,100),gyroPivot.transform.localRotation.eulerAngles+"\n"+Screen.orientation+"\n"+rotationFix.eulerAngles.ToString());
			
			if(GUI.Button(new Rect(100,0,100,100),"Reset"))
			{
				gyroPivot.transform.localRotation = new Quaternion(0,0,0,0);
				rotationFix = new Quaternion(0,0,0,0);
			}
			
			if(GUI.Button(new Rect(100,200,100,100),"x"))
			{
				Vector3 tmp = gyroPivot.transform.localRotation.eulerAngles;
				tmp.x += 90;
				Quaternion q = new Quaternion(0,0,0,0);
				q.eulerAngles = tmp;
				gyroPivot.transform.localRotation = q;
			}
			if(GUI.Button(new Rect(200,200,100,100),"y"))
			{
				Vector3 tmp = gyroPivot.transform.localRotation.eulerAngles;
				tmp.y += 90;
				Quaternion q = new Quaternion(0,0,0,0);
				q.eulerAngles = tmp;
				gyroPivot.transform.localRotation = q;
			}
			if(GUI.Button(new Rect(300,200,100,100),"z"))
			{
				Vector3 tmp = gyroPivot.transform.localRotation.eulerAngles;
				tmp.z += 90;
				Quaternion q = new Quaternion(0,0,0,0);
				q.eulerAngles = tmp;
				gyroPivot.transform.localRotation = q;
			}
			
			if(GUI.Button(new Rect(100,300,100,100),"RotFixx"))
			{
				Vector3 tmp = rotationFix.eulerAngles;
				tmp.x += 90;
				rotationFix.eulerAngles = tmp;
			}
			if(GUI.Button(new Rect(200,300,100,100),"RotFixy"))
			{
				Vector3 tmp = rotationFix.eulerAngles;
				tmp.y += 90;
				rotationFix.eulerAngles = tmp;
			}
			if(GUI.Button(new Rect(300,300,100,100),"RotFixz"))
			{
				Vector3 tmp = rotationFix.eulerAngles;
				tmp.z += 90;
				rotationFix.eulerAngles = tmp;
			}
		}
	}
	
	public void InitGyroIPhone()
	{
		//PIVOT
		if (Screen.orientation == ScreenOrientation.LandscapeLeft) 
		{
			//gyroPivot.transform.eulerAngles = new Vector3(90,90,0);
			Quaternion q = new Quaternion(0,0,0,0);
			q.eulerAngles = new Vector3(90,90,0);
			gyroPivot.transform.localRotation = q;
		}
		else if (Screen.orientation == ScreenOrientation.LandscapeRight)
		{
			//gyroPivot.transform.eulerAngles = new Vector3(90,0,270);
			Quaternion q = new Quaternion(0,0,0,0);
			q.eulerAngles = new Vector3(90,90,0);
			gyroPivot.transform.localRotation = q;
		}
		//ROTFIX
		if (Screen.orientation == ScreenOrientation.LandscapeLeft) 
		{
			//rotationFix = new Quaternion(0f,0,0.7071f,0.7071f);
			rotationFix = new Quaternion(0,0,0,0);
			rotationFix.eulerAngles = new Vector3(0,0,180);
		}
		else if (Screen.orientation == ScreenOrientation.LandscapeRight)
		{
			//rotationFix = new Quaternion(0,0,0.7071f,0.7071f);
			rotationFix = new Quaternion(0,0,0,0);
			rotationFix.eulerAngles = new Vector3(0,0,0);
		}
	}
	
	public void InitGyroAndroid()
	{
		//PIVOT
		if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight) 
		{
			gyroPivot.transform.eulerAngles = new Vector3(90,90,90);
		}
		
		//ROTFIX
		if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight) 
		{
			Vector3 rfx = new Vector3(0,0,180);
			rotationFix = new Quaternion(0,0,0,0);
		//	rfx.y += 90;
			rotationFix.eulerAngles = rfx;
		}
	}
	
	public void initGyro()
	{
		if(gyroActive)
		{
			
			if(gyroPivot == null)
			{
				originalParent = transform.parent;
				gyroPivot = new GameObject("GyroPivot");
				gyroPivot.transform.position = transform.position;
				transform.parent = gyroPivot.transform;
				gyroPivot.transform.parent = originalParent;
//				GetComponent<SceneControl>().gyroPivot = gyroPivot;
			}
			
			gyro = Input.gyro;
			gyro.enabled = true;
#if UNITY_IPHONE
			InitGyroIPhone();	
#elif UNITY_ANDROID
			InitGyroAndroid();		
#endif
			
		}
	}
	
	public void SetDebugMode()
	{
		_debug = enabled;	
	}
}
