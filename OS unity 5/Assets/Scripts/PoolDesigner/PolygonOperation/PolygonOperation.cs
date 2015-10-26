using UnityEngine;
using System.Collections;

// Set d'operation applicable aux polygoens
public class PolygonOperation
{		
	// Cherche les vertices qui transformera le polgone troué en polygone concave
	// voir http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
	protected static MutuallyVisibleVertices FindMutuallyVisibleVertices (Polygon inner, Polygon outer)
	{
		LoopedList<Point2> points = inner.GetPoints ();
		
		Vector2 innerMaxXVertex = new Vector2 (float.MinValue, 0);
		
		int innerIndex = 0;
		int outerIndex = 0;
		
		for (int vertexIndice = 0; vertexIndice < points.Count; ++vertexIndice)
		{
			Vector2 vertex = points[vertexIndice];
			
			if (vertex.x > innerMaxXVertex.x)
			{
				innerMaxXVertex = vertex;
				innerIndex = vertexIndice;
			}
		}
		
		// fake algorithm because we know that outer polygon is an axis aligned rectangle
		Vector2 upRight = outer.GetPoints ()[0];
		Vector2 downRight = outer.GetPoints ()[1];
		
		float upDistance = Vector2.Distance (innerMaxXVertex, upRight);
		float downDistance = Vector2.Distance (innerMaxXVertex, downRight);
		Vector2 duplicatedMVVOffset;
		
		if (upDistance < downDistance)
		{
			outerIndex = 0;
			duplicatedMVVOffset = new Vector2 (1.0f, 0); 
		}
		else
		{
			outerIndex = 1;	
			duplicatedMVVOffset = new Vector2 (-1.0f, 0);
		}
		
		MutuallyVisibleVertices mvv = new MutuallyVisibleVertices ();
		mvv.innerIndex = innerIndex;
		mvv.outerIndex = outerIndex;
		mvv.duplicatedMVVOffset = duplicatedMVVOffset;
		
		return mvv;
	}
	
	// fusionne deux polygones. Inner doit etre totalement inclus dans outer
	// permet la triangulation d'un polygonr avec un trou --> plage
	// voir http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
	public static MergedPolygon MergeInnerAndOuter (Polygon inner, Polygon outer)
	{
		MergedPolygon mergedPolygon = new MergedPolygon ();
		PolygonRawData mergedPolyRaw = new PolygonRawData ();
		
		MutuallyVisibleVertices mvv = FindMutuallyVisibleVertices (inner, outer);
		
		LoopedList<Point2> innerPoints = inner.GetPoints ();
		LoopedList<Point2> outerPoints = outer.GetPoints ();
		
		int innerIndex = 0;
		for (; innerIndex <= mvv.innerIndex; ++innerIndex)
		{
			mergedPolyRaw.Add (innerPoints[innerIndex]);	
		}
		
		mergedPolygon.originalIndex0 = mergedPolyRaw.Count - 1;
		mergedPolygon.originalIndex1 = mergedPolyRaw.Count;
		
		for (int counter = 0, outerIndex = mvv.outerIndex; 
			counter < outerPoints.Count; 
			++counter, --outerIndex)
		{
			mergedPolyRaw.Add (outerPoints[outerIndex]);
		}
		
		mergedPolyRaw.Add (outerPoints[mvv.outerIndex] + mvv.duplicatedMVVOffset);
		mergedPolygon.duplicatedIndex1 = mergedPolyRaw.Count - 1;
		
		mergedPolyRaw.Add (innerPoints[mvv.innerIndex] + mvv.duplicatedMVVOffset);
		mergedPolygon.duplicatedIndex0 = mergedPolyRaw.Count - 1;
		
		for (; innerIndex < innerPoints.Count; ++innerIndex)
		{
			mergedPolyRaw.Add (innerPoints[innerIndex]);
		}
		
		mergedPolygon.polygonRawData = mergedPolyRaw;
		
//		Debug.Log (mergedPolygon.originalIndex0 + " " + mergedPolygon.originalIndex1 + " " + mergedPolygon.duplicatedIndex0 + " " + mergedPolygon.duplicatedIndex1);
		
		return mergedPolygon;
	}
	
	// retourne un nouveau polygone correspondant a un contour du polygone polygon
	// calcule a partir de quantity
	public static Polygon GetOutlinedPolygon (Polygon polygon, float quantity)
	{
		Point2Data [] pointsData = new Point2Data[polygon.GetPoints ().Count];
		polygon.CopyPoint2DataTo (pointsData);
		
		for (int pIndex = 0; pIndex < pointsData.Length; ++pIndex)
		{
			Point2 pt2 = polygon.GetPoints ()[pIndex];
			
			Vector2 position = pointsData[pIndex].position;
			position = position + pt2.GetCalculatedNormal () * quantity;
			
			pointsData[pIndex].position = position;
		}
		
		return new Polygon (pointsData);
	}
}