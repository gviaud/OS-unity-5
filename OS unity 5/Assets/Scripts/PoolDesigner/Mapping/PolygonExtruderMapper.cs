using UnityEngine;
using System.Collections;

// mapping pour les meshs extruder a l'aide du polygone extruder
// proche du mapping cylindrique : paroi et mapping plannaire sur les surface bouchants les parois
public class PolygonExtruderMapper
{
	protected PolygonExtruder _polygonExtruder;
	
	public PolygonExtruderMapper (PolygonExtruder polygonExtruder)
	{
		_polygonExtruder = polygonExtruder;
	}
	
	protected void SetUvs ()
	{
		Mesh mesh = _polygonExtruder.GetMesh ();
		Polygon poly = _polygonExtruder.GetMeshPolygon ();
		
		LoopedList<Point2> points = poly.GetPoints ();
		LoopedList<Edge2> edges = poly.GetEdges ();
		
		float u = 0;
		float v = Mathf.Abs (_polygonExtruder.GetQuantity ());
		
		int vertexIndex = 0;
		Vector2 [] uvs = new Vector2[mesh.vertexCount];
		
		if (mesh.vertexCount < points.Count)
		{
			return;
		}	
		
		for (int pointIndex = 0; pointIndex < points.Count; ++pointIndex)
		{
//			Vector3 vertex = mesh.vertices[vertexIndex];
			
			uvs[vertexIndex++] = new Vector2 (u, v);
			uvs[vertexIndex++] = new Vector2 (u, 0);
			
			u += edges[pointIndex].GetLength ();
		}
		
		if (poly.IsClosed ())
		{ 
			uvs[mesh.vertexCount - 2] = new Vector2 (u, v);
			uvs[mesh.vertexCount - 1] = new Vector2 (u, 0);
		}
		
		if (_polygonExtruder.IsUpCapped ())
		{
			for (int pointIndex = 0; pointIndex < points.Count; ++pointIndex)
			{
				Vector3 vertex = mesh.vertices[vertexIndex];
				uvs[vertexIndex++] = new Vector2 (vertex.x, vertex.z);
			}
		}
		
		if (_polygonExtruder.IsBottomCapped ())
		{
			for (int pointIndex = 0; pointIndex < points.Count; ++pointIndex)
			{
				Vector3 vertex = mesh.vertices[vertexIndex];
				uvs[vertexIndex++] = new Vector2 (vertex.x, vertex.z);
			}
		}
		
		mesh.uv = uvs;
	}
	
	public void Generate ()
	{
		SetUvs ();	
	}
}
