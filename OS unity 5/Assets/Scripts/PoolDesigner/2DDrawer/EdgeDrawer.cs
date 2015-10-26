using UnityEngine;
using System.Collections;

public class EdgeDrawer
{
	// texture de l'edge, chargée dans PolygonDrawer
	public static Texture2D SOLID_EDGE_TEXTURE;	
	
	// dessine une edge d'une epaisseur thickness en appliquant les transformations
	// de planMatrix
	public static void Draw (Edge2 edge, Matrix4x4 planMatrix, int thickness = 10, 
		float offsetWidth=0, float offsetHeight=0)
	{
		Matrix4x4 matrix = GUI.matrix;	
		
		Vector2 nextPt = edge.GetNextPoint2 ();
		nextPt.Set(nextPt.x+offsetWidth, nextPt.y+offsetHeight);
		nextPt = planMatrix.MultiplyPoint (nextPt);
		
		Vector2 prevPt = edge.GetPrevPoint2 ();
		prevPt.Set(prevPt.x+offsetWidth, prevPt.y+offsetHeight);
		prevPt = planMatrix.MultiplyPoint (prevPt);
		
        float angle = Vector2.Angle(nextPt - prevPt, Vector2.right);
        
		if (prevPt.y > nextPt.y) 
		{ 
			angle = -angle; 
		}

        GUIUtility.RotateAroundPivot (angle, prevPt);
		
		GUI.DrawTexture(new Rect (prevPt.x, prevPt.y - thickness / 2 , 
			(nextPt - prevPt).magnitude, thickness), SOLID_EDGE_TEXTURE);
		
        GUI.matrix = matrix;	
	}
}