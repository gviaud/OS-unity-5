using UnityEngine;
using System.Collections;

public class WebcamTest : MonoBehaviour
{
	public string deviceName;
	WebCamTexture wct=null;
	Color32[] pixs;
	Color32[] outPixs;
	Texture2D tex;
	
	// Use this for initialization
//	void Start ()
//	{
//		
//		
//		WebCamDevice[] devices = WebCamTexture.devices;
//		deviceName = devices [0].name;
//		wct = new WebCamTexture (deviceName, 400, 300, 12);
//		//	renderer.material.mainTexture = wct;
//		wct.Play ();
//	}
	
	public void/*IEnumerator*/ StartVideo ()
	{
	//	yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
	//	if (Application.HasUserAuthorization (UserAuthorization.WebCam))
	//		Debug.Log ("Authorized");
		if(wct==null)
		{
			WebCamDevice[] devices = WebCamTexture.devices;
			deviceName = devices [0].name;
			wct = new WebCamTexture(deviceName,Screen.width/2, Screen.height/2);
		}
		transform.GetComponent<GUITexture>().pixelInset = new Rect(0,0,Screen.width, Screen.height);
		transform.GetComponent<GUITexture>().texture = wct;
		transform.localScale = new Vector3(0.0f,-2.0f,1.0f);
		GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = true;
		wct.Play ();
		
	}
	public void StopVideo()
	{
		if(wct!=null)
		{
			GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = false;
			//wct.Stop();
			wct.Pause();
			StartCoroutine(test());
//			Texture2D tex = new Texture2D(wct.width,wct.height);
//			
//			tex.SetPixels32(wct.GetPixels32());
//			
//			tex.Apply(false);
//			Montage.sm.setBackground(tex);
//			
//			Destroy(wct);
//			wct=null;
		}
	}
	
	IEnumerator test()
	{
//		tex = new Texture2D(wct.width,wct.height);
//			
//		pixs = wct.GetPixels32();
//		tex.SetPixels32(pixs);
//		//tex.Resize(Screen.width,Screen.height); //A TESTER -> MARCHE PAS, PLANTE LAMENTABLEMENT
//		tex.Apply(false);
		
		yield return invertPhoto();
		
//		Montage.sm.setBackground(tex);
		Montage.sm.updateFond(tex,false,"");
		transform.localScale = new Vector3(0,0,1);
		
		Destroy(wct);
		wct=null;
	    GameObject.Find("MainScene").GetComponent<GUIMenuMain>().setStarter(true, true);
		yield return true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	
	}
	
	bool invertPhoto()
	{
		int W = wct.width;
		int H = wct.height;
		pixs = wct.GetPixels32();
		outPixs = new Color32[W*H];
		
		tex = new Texture2D(W,H);	
		
		for(int l=0;l<H;l++)
		{
			for(int w=0;w<W;w++)
			{
				outPixs[((H-l-1)*W)+w] = pixs[(l*W)+w];	
			}
		}
		
		tex.SetPixels32(outPixs);
		tex.Apply(false);
		return true;
	}
	
	/*void OnGUI ()
	{
		if (Input.GetMouseButton (0)) {
			transform.guiTexture.texture = wct;
		}
                    
	}*/

}
