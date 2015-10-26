using UnityEngine;
using System.Collections;

public class animatedGUI {
	
	private object[] texNormal;
	private object[] texHover;
	private object[] texPressed;
	private object[] use;
	
	private string state;
	
	private Texture2D current;
	
	private float t = 0;
	private float delta;
	
	private int index = 0;
	
	private Rect r;
	
	private bool output = false;
	
	GUIStyle style = new GUIStyle();
	
	public  enum TypeOfGUI {texture,button,repeatButton,toggle };
	
	private TypeOfGUI tog;
	
	public animatedGUI(float speed/*,Rect rc*/,TypeOfGUI t )
	{
		delta = speed;
		state = "normal";
		tog = t;
//		r = rc;
//		tex = new ArrayList();
	}
	
//	public void addTex(Texture2D tx)
//	{
////		tex.Add(tx); // quand tex = arraylist
//		tex[tex.Length] = tx;
//		if(current == null)
//			current = tx;
//	}
	
	public void loadFrom(string folder)
	{
		texNormal = Resources.LoadAll(folder+"/normal");
		texHover = Resources.LoadAll(folder+"/hover");
		texPressed = Resources.LoadAll(folder+"/pressed");
	}
	
	// Update is called once per frame
	public void update (bool pressed) 
	{
//		Vector2 cursor = Input.mousePosition;
//		cursor.y = Screen.height-cursor.y;
		//-----------------------------------------
//		if(r.Contains(cursor)&& !pressed)
//			state = "hover";
//		else if(r.Contains(cursor) && pressed)
//		{
//			state = "pressed";
//		}
//		else if(tog == animatedGUI.TypeOfGUI.toggle && output)
//		{
//			state = "pressed";
//		}
//		else
//			state = "normal";
				
		//-----------------------------------------
		if(Time.time > t+delta)
		{
			t = Time.time;

			switch(state)
			{
				case "normal":
					if(index < texNormal.Length)
					{
						current = (Texture2D) texNormal[index];
						index ++;
					}
					else
						index = 0;
				break;
					
				case "hover":
					if(index < texHover.Length)
					{
						current = (Texture2D) texHover[index];
						index ++;
					}
					else
						index = 0;
				break;
					
				case "pressed":
					if(index < texPressed.Length)
					{
						current = (Texture2D) texPressed[index];
						index ++;
					}
					else
						index = 0;
				break;
			}
		}
	}
	
	public bool getGuiBtn(Rect rc)
	{
		return getGuiBtn(rc,false);
	}
	
	
	public bool getGuiBtn(Rect rc,bool self)
	{
//		return GUI.Button(r,current,style);
//		r = rc;
//		if(isButton)
//			return GUI.RepeatButton(r,current,style);
//		else
//		{
//			GUI.DrawTexture(r,current);
//			return true;
//		}
		r = rc;
		
		switch(tog)
		{
			case TypeOfGUI.button:
				output = GUI.Button(r,current,style);
			break;
			
			case TypeOfGUI.repeatButton:
				output = GUI.RepeatButton(r,current,style);
			break;
			
			case TypeOfGUI.texture:
			GUI.DrawTexture(r,current);
			output = true;
			break;
			
			case TypeOfGUI.toggle:
			output = GUI.Toggle(r,self,current,style);				
			break;
		}
		if(output)
			state = "pressed";
		else
			state = "normal";
		return output;
	}
		
	
}
