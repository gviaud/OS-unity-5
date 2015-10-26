using UnityEngine;
using System.Collections;

public class ArcDrawer
{
	protected static float ANGLE_STEP = 10;
	protected static float POINT_RADIUS = 50;
	
	// dessine un arc de cercle defini par les parametres de point auquel on applique une transformation de matrix
	// le pas de l'arc de cercle est defini par ANGLE_STEP en degré
	public static void Draw (ArchedPoint2 point, Matrix4x4 matrix,
		float offsetWidth=0, float offsetHeight=0)
	{	
		Vector2 center = point.GetCenter ();
		float radius = point.GetMeasuredRadius ();
		
		Vector2 prevTangentPoint = point.GetPrevTangentPoint ();
		Vector2 nextTangentPoint = point.GetNextTangentPoint ();
		
		Vector2 firstPoint2Add = prevTangentPoint;
		Vector2 lastPoint2Add = nextTangentPoint;
		
		Vector2 startVector = (prevTangentPoint - center).normalized;
		Vector2 endVector = (nextTangentPoint - center).normalized;
		
		Vector2 angleMeasureReference = Vector2.right;
		
		if (point.GetAngleType () == AngleType.Outside)
		{
			startVector = (nextTangentPoint - center).normalized;
			endVector = (prevTangentPoint - center).normalized;
			
			firstPoint2Add = nextTangentPoint;
			lastPoint2Add = prevTangentPoint;
		}
		
		// On cherche les angles de depart et de fin par rapport au vector (1, 0)
		// pour tracer l'arc dans le sens trigonometrique
		float startAngle;		
		if (Utils.Approximately (Vector3.Cross (startVector, angleMeasureReference).z, 0))
		{
			startAngle = Vector2.Dot (startVector, angleMeasureReference) < 0 ? 180 : 0;
		}
		else
		{
			Quaternion startRotation = Quaternion.FromToRotation (angleMeasureReference, startVector);
			startAngle = startRotation.eulerAngles.z;
		}
		
		float endAngle;
		if (Utils.Approximately (Vector3.Cross (endVector, angleMeasureReference).z, 0))
		{
			endAngle = Vector2.Dot (endVector, angleMeasureReference) < 0 ? 180 : 0;
		}
		else
		{
			Quaternion endRotation = Quaternion.FromToRotation (angleMeasureReference, endVector);
			endAngle = endRotation.eulerAngles.z;
		}
		
		//Debug.Log (startAngle + " " + endAngle);
		
		if (startAngle > endAngle)
		{
			endAngle += 360;
		}
		
		startAngle += ANGLE_STEP;
		float currentAngle = startAngle;
		
		// on cree le polygon ouvert correspondant a l'arc de cercle
		Polygon arc = new Polygon ();
		arc.AddPoint (new Point2 (firstPoint2Add));
		
		// ajout de chaque point du polygone calcule a partir de l'angle courant
		while (currentAngle < endAngle)
		{
			float radAngle = currentAngle * Mathf.Deg2Rad;
			Vector2 arcPoint = new Vector2 (Mathf.Cos (radAngle), Mathf.Sin (radAngle)) * radius + center;
			
			arc.AddPoint (new Point2 (arcPoint));
			
			currentAngle += ANGLE_STEP;
		}
		
		arc.AddPoint (new Point2 (lastPoint2Add));
		
//		arc = new Polygon ();
//		foreach (Vector2 v in point.GetCurve ())
//		{
//			arc.AddPoint (new Point2 (v));
//		}
		
		// on dessine l'ensemble des edges correspondant a l'arc de cercle
		foreach (Edge2 edge in arc.GetEdges ())
		{
			EdgeDrawer.Draw (edge, matrix,10, offsetWidth, offsetHeight );	
		}
		
//		Vector2 anchorPoint = matrix.MultiplyPoint (point.GetAnchorPoint ());
//		GUI.Box (new Rect (anchorPoint.x - POINT_RADIUS / 2, 
//					       anchorPoint.y - POINT_RADIUS / 2, 
//					       POINT_RADIUS, 
//					       POINT_RADIUS), "", "selected point");
		
		// dessine les points tangents
		prevTangentPoint.Set(prevTangentPoint.x+offsetWidth,prevTangentPoint.y + offsetHeight);
		prevTangentPoint = matrix.MultiplyPoint (prevTangentPoint);
		GUI.Box (new Rect (prevTangentPoint.x - POINT_RADIUS / 2,
					       prevTangentPoint.y - POINT_RADIUS / 2, 
					       POINT_RADIUS, 
					       POINT_RADIUS), "", /*"selected point up"*/"arcpoint");
		
		nextTangentPoint.Set(nextTangentPoint.x+offsetWidth,nextTangentPoint.y + offsetHeight);
		nextTangentPoint = matrix.MultiplyPoint (nextTangentPoint);
		GUI.Box (new Rect (nextTangentPoint.x - POINT_RADIUS / 2, 
					       nextTangentPoint.y - POINT_RADIUS / 2, 
					       POINT_RADIUS, 
					       POINT_RADIUS), "", /*"selected point up"*/"arcpoint");
		
//		center = matrix.MultiplyPoint (center);
//		GUI.Box (new Rect (center.x - POINT_RADIUS / 2, 
//					       center.y - POINT_RADIUS / 2, 
//					       POINT_RADIUS, 
//					       POINT_RADIUS), "", "selected point");
	}
}
