using UnityEngine;
using System.Collections;

public class PhotoConfig : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		//fit de l'image de fond
//		float h = GetComponent<GUITexture>().texture.height;
//		float w = GetComponent<GUITexture>().texture.width;
//		float ratio = w/h;
//		
//		float frameSize = 125;
//		
//		float sizeW = 0;
//		float sizeH = 0;
//		
//		if(w>h)//format paysage
//		{
//			sizeW = Screen.width-frameSize;
//			sizeH = sizeW/ratio;
//			
//			GetComponent<GUITexture>().pixelInset = new Rect(frameSize,(Screen.height-sizeH)/2,sizeW,sizeH);
//			//Anti rendus objets dans les bandes noires
//			Camera.mainCamera.pixelRect = new Rect ((Screen.width/2)-(sizeW/2)+(frameSize/2),
//			                                        (Screen.height/2)-(sizeH/2),
//			                                        sizeW,sizeH);
//		}
//		else   // format portrait
//		{
//			sizeW = Screen.height*ratio;
//			sizeH = Screen.height;
//			
//			GetComponent<GUITexture>().pixelInset = new Rect((Screen.width-sizeW)/2,0,sizeW,sizeH);
//			//Anti rendus objets dans les bandes noires
//			Camera.mainCamera.pixelRect = new Rect ((Screen.width/2)-(sizeW/2),
//			                                        (Screen.height/2)-(sizeH/2),
//			                                        sizeW,sizeH);
//		}
//		
////		//Anti rendus objets dans les bandes noires
//		Camera.mainCamera.pixelRect = new Rect ((Screen.width/2)-(sizeW/2)+(frameSize/2),(Screen.height/2)-(sizeH/2),sizeW,sizeH);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
