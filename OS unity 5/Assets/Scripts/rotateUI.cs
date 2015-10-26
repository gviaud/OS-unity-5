using UnityEngine;
using System.Collections;

public class rotateUI : MonoBehaviour {
	
	Vector2 center;
	Vector2 baseVector = new Vector2(1,0);
	
	public Texture rotateTex; 
	
	float angle = 0;
		
	// Use this for initialization
	void Start () {
		center.x = Screen.width/2;
		center.y = Screen.height/2;
	}
	
	// Update is called once per frame
	void Update () {
//		angle = angle + 0.1f;
//		Debug.Log("Angle = "+angle);
		if(Input.GetMouseButton(0))
		{
			Vector2 cursor = Input.mousePosition;
			Vector2 v = cursor-center;
			
			if(cursor.y>center.y)
			{
				angle = Vector2.Angle(baseVector,v);
				angle = 180+(180-angle);
			}
			else
			{
				angle = Vector2.Angle(baseVector,v);
			}	
			angle = (float)((int)angle);
		}
	}
	
	void OnGUI ()
	{
		GUI.DrawTexture(new Rect(center.x-40,center.y-40,80,80),rotateTex);
		Matrix4x4 matrixBackup = GUI.matrix;
		{
			GUIUtility.RotateAroundPivot(angle,center);
			GUI.RepeatButton(new Rect(center.x+20,center.y-10,50,20),angle + "°");			
		}
		GUI.matrix = matrixBackup;
	}
}
