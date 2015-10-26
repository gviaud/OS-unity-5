using UnityEngine;
using System.Collections;
using System;

public class Drawer : MonoBehaviour {
    
	public static Texture2D lineTex = null;

    public static void DrawLine(Rect rect) { DrawLine(rect, GUI.contentColor, 1.0f); }
    public static void DrawLine(Rect rect, Color color) { DrawLine(rect, color, 1.0f); }
    public static void DrawLine(Rect rect, float width) { DrawLine(rect, GUI.contentColor, width); }
    public static void DrawLine(Rect rect, Color color, float width) { DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }
    
	public static void DrawLine(Vector2 pointA, Vector2 pointB) { DrawLine(pointA, pointB, GUI.contentColor, 1.0f); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color) { DrawLine(pointA, pointB, color, 1.0f); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, float width) { DrawLine(pointA, pointB, GUI.contentColor, width); }
    
	void Start()
	{

	}
	
	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
        // Save the current GUI matrix, since we're going to make changes to it.
        Matrix4x4 matrix = GUI.matrix;

        // Generate a single pixel texture if it doesn't exist
        if (!lineTex) { lineTex = new Texture2D(1, 1); }

        // Store current GUI color, so we can switch it back later,
//		Color savedColor = GUI.color;
//        GUI.color = color;

		
        // Determine the angle of the line.
        float angle = Vector3.Angle(pointB - pointA, Vector2.right);

        // Vector3.Angle always returns a positive number.
        // If pointB is above pointA, then angle needs to be negative.
        if (pointA.y > pointB.y) { angle = -angle; }

        GUIUtility.RotateAroundPivot(angle, pointA);
		
		GUI.DrawTexture(new Rect(pointA.x,pointA.y,(pointB - pointA).magnitude,width), lineTex);

        GUI.matrix = matrix;
//        GUI.color = savedColor;
    }
	
	public static void DrawLine(Vector2 pointA, Vector2 pointB, Texture2D tex, float width)
    {
        // Save the current GUI matrix, since we're going to make changes to it.
        Matrix4x4 matrix = GUI.matrix;		
        // Determine the angle of the line.
        float angle = Vector3.Angle(pointB - pointA, Vector2.right);

        // Vector3.Angle always returns a positive number.
        // If pointB is above pointA, then angle needs to be negative.
        if (pointA.y > pointB.y) { angle = -angle; }

        GUIUtility.RotateAroundPivot(angle, pointA);
		
		GUI.DrawTexture(new Rect(pointA.x,pointA.y,(pointB - pointA).magnitude,width), tex);//ICI
		
        GUI.matrix = matrix;
    }
	
	public static void DrawLineCentered(Vector2 pointA, Vector2 pointB, Texture2D tex, float width)
    {
        // Save the current GUI matrix, since we're going to make changes to it.
        Matrix4x4 matrix = GUI.matrix;		
        // Determine the angle of the line.
        float angle = Vector3.Angle(pointB - pointA, Vector2.right);

        // Vector3.Angle always returns a positive number.
        // If pointB is above pointA, then angle needs to be negative.
        if (pointA.y > pointB.y) { angle = -angle; }

        GUIUtility.RotateAroundPivot(angle, pointA);
		
		GUI.DrawTexture(new Rect(pointA.x,pointA.y-width/2,(pointB - pointA).magnitude,width), tex);//ICI
		
        GUI.matrix = matrix;
    }
	
	public static void DrawIcon(Vector2 pointA, Texture2D map){DrawIcon(pointA, map,map.width);}
	public static void DrawIcon(Vector2 pointA, Texture2D map,float mapsize)
    {
    	Matrix4x4 matrix = GUI.matrix;
        float size = mapsize;
		float halfSize = size/2;
		GUI.DrawTexture(new Rect(pointA.x-halfSize,pointA.y-halfSize,size,size),map);
		GUI.matrix = matrix;
    }
	public static void DrawIcon(Vector2 pointA,float angle,Texture2D map,float mapsize)
    {
    	Matrix4x4 matrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(angle, pointA);
        float size = mapsize;
		float halfSize = size/2;
		GUI.DrawTexture(new Rect(pointA.x-halfSize,pointA.y-halfSize,size,size),map);
		GUI.matrix = matrix;
    }
	
	public static void DrawTxt(Vector2 pointA,float angle,string txt)
    {
//		Color saved = GUI.color;
//		GUI.color = Color.black;
//    	Matrix4x4 matrix = GUI.matrix;
//		GUIUtility.RotateAroundPivot(angle, pointA);
//        
//		float w = 100;
//		float h = 30;
//		
//		GUI.Label(new Rect(pointA.x-(w/4),pointA.y-(h),w,h),txt);
//		
//		GUI.matrix = matrix;
//		GUI.color = saved;
    }
	
}