//using UnityEngine;
//using System.Collections;
//
//public class GUIDisplay :MonoBehaviour{
//	
//	static string _title = "no title";
//	static string _message = "no message";
//	static int posX = 0;
//	static int posY = 0;
//	static int width = 0;
//	static int height = 0;
//	static string GuiSwitch = "none";
//	static string _btn1 = "btn1";
//	static string _btn2 = "btn2";
//	static string _btn3 = "btn3";
//	
//	// Use this for initialization
//	void Start () {
//	
//	}
//	
//	// Update is called once per frame
//	void Update () {
//	}
//	
//	void OnGUI() 
//	{
//		switch(GuiSwitch)       
//	      {         
//			case "none":
//			    //No GUI
//			    break; 
//			
//			case "msgBox": 
//			    GUI.Box(new Rect(posX,posY,width,height),_title);
//				GUI.Label(new Rect(posX+40,posY+20,width-50,height-30),_message);
//				if(GUI.Button(new Rect(posX+width-100, posY+height+10, 100, 20), "ok"))
//				{
//					//action
//					GuiSwitch = "none";
//				}
//			    break;
//			
//			case "activationAsk":
//				GUI.Box(new Rect(posX,posY,width,height),_title);
//				GUI.Label(new Rect(posX+40,posY+30,width-50,height-30),_message);
//				if(GUI.Button(new Rect(posX+width-(1*100), posY+height+10, 100, 20), _btn1))
//				{
//					MainClass_old.activation();
//					GuiSwitch = "none";
//				}
//				if(GUI.Button(new Rect(posX, posY+height+10, 100, 20), _btn2))
//				{
//					GuiSwitch = "none";
//				}
//				break;
//	       }
//	}
//	
//	public void displayMessage(string title, string msg)
//	{
//		_message = msg;
//		_title = title;
//		width = 250;
//		height = 100;
//		setCenterPosition();
//		GuiSwitch = "msgBox";
//	}
//	
//	
//	public void twoBtnsMsgBox(string title, string msg, string btn1, string btn2)
//	{
//		bool result = false;
//		_btn1 = btn1;
//		_btn2 = btn2;
//		_message = msg;
//		_title = title;
//		width = 250;
//		height = 100;
//		setCenterPosition();
//		GuiSwitch = "activationAsk";
//	}
//	
//	/**
//	 * centre la message box, ATTENTION : définir width et height avant !
//	 * */
//	private void setCenterPosition()
//	{
//		posX = ((Screen.width)/2)-(width/2);
//		posY = ((Screen.height)/2)-(height/2);
//	}
//}
