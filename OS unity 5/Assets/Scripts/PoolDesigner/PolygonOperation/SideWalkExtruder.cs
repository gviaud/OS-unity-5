using UnityEngine;
using System.Collections;

// Class permettant de creer une plage a partir d'une forme de base
// represente par un polygone ferme
public class SideWalkExtruder
{
	protected static int BORDER_VERTEX_COUNT = 4;
	
	protected Polygon _polygon;
	
	protected float _quantity = 0;
	protected float _outlineOffset = 10;
	
	protected Mesh _mesh;
	
	protected MergedPolygon _mergedPolygon;
	
	public SideWalkExtruder (Polygon polygon, Mesh mesh, float quantity, float outlineOffset)
	{
		_polygon = polygon;
		_mesh = mesh;
		_quantity = quantity;
		_outlineOffset = outlineOffset;
	}
	
	// fusion de la forme de base (liner, margelle) et rectangle exterieur
	// pour la triangulation
	// voir http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
	protected void MergedInnerAndOuter ()
	{
		Bounds polyBounds = _polygon.bounds;
		Vector3 extents = polyBounds.extents;
		Vector3 center = polyBounds.center;
		
		// Pythagore
		float translation = _outlineOffset * 1.41421356f; // sqrt (2)
			
		Vector2 upRightDir = new Vector2 (1, 1).normalized * translation;
		Vector2 upRight = new Vector2 (center.x + extents.x, center.z + extents.z) + upRightDir;
		
		Vector2 downRightDir = new Vector2 (1, -1).normalized * translation;
		Vector2 downRight = new Vector2 (center.x + extents.x, center.z - extents.z) + downRightDir;
		
		Vector2 downLeftDir = new Vector2 (-1, -1).normalized * translation;
		Vector2 downLeft = new Vector2 (center.x - extents.x, center.z - extents.z) + downLeftDir;
		
		Vector2 upLeftDir = new Vector2 (-1, 1).normalized * translation;
		Vector2 upLeft = new Vector2 (center.x - extents.x, center.z + extents.z) + upLeftDir;
		
		PolygonRawData outerRawPoly = new PolygonRawData ();
		outerRawPoly.Add (upRight);
		outerRawPoly.Add (downRight);
		outerRawPoly.Add (downLeft);
		outerRawPoly.Add (upLeft);
		
		Polygon outerPolygon = new Polygon (outerRawPoly);
		_mergedPolygon = PolygonOperation.MergeInnerAndOuter (_polygon, outerPolygon);
	}
	
	protected void SetVertices ()
	{
		int mergedPolyCount = _mergedPolygon.polygonRawData.Count - 2;
		int verticesCount = mergedPolyCount + BORDER_VERTEX_COUNT * 4;
		Vector3 [] vertices = new Vector3[verticesCount];
		
		for (int pointIndex = 0, verticesIndex = 0; 
			pointIndex < _mergedPolygon.polygonRawData.Count; 
			++pointIndex)
		{
			if (pointIndex != _mergedPolygon.duplicatedIndex0 && 
				pointIndex != _mergedPolygon.duplicatedIndex1)
			{
				Vector2 p0 = _mergedPolygon.polygonRawData[pointIndex];
				vertices[verticesIndex++] = new Vector3 (p0.x, 0, p0.y);
			}
		}
		
		Bounds polyBounds = _polygon.bounds;
		Vector3 extents = polyBounds.extents;
		Vector3 center = polyBounds.center;
		
		// Pythagore
		float translation = _outlineOffset * 1.41421356f; // sqrt (2)
		
		Vector3 upRightDir = new Vector3 (1, 0, 1).normalized * translation;
		Vector3 upRight = new Vector3 (center.x + extents.x, 0, center.z + extents.z) + upRightDir;
		
		Vector3 downRightDir = new Vector3 (1, 0, -1).normalized * translation;
		Vector3 downRight = new Vector3 (center.x + extents.x, 0, center.z - extents.z) + downRightDir;
		
		Vector3 downLeftDir = new Vector3 (-1, 0, -1).normalized * translation;
		Vector3 downLeft = new Vector3 (center.x - extents.x, 0, center.z - extents.z) + downLeftDir;
		
		Vector3 upLeftDir = new Vector3 (-1, 0, 1).normalized * translation;
		Vector3 upLeft = new Vector3 (center.x - extents.x, 0, center.z + extents.z) + upLeftDir;
		
		// add border faces vertices
		int borderIndex = mergedPolyCount;
		Vector3 [] borderVertices = {downRight, downLeft, upLeft};
		
		Vector3 upRightDown = upRight;
		upRightDown.y = _quantity;
		vertices[borderIndex++] = upRight;
		vertices[borderIndex++] = upRightDown;
		
		foreach (Vector3 borderVertex in borderVertices)
		{
			Vector3 borderDownVertex = borderVertex;
			borderDownVertex.y = _quantity;
			
			vertices[borderIndex++] = borderVertex;
			vertices[borderIndex++] = borderDownVertex;
			vertices[borderIndex++] = borderVertex;
			vertices[borderIndex++] = borderDownVertex;
		}
		
		vertices[borderIndex++] = upRight;
		vertices[borderIndex++] = upRightDown;
		
		_mesh.vertices = vertices;
	}
	
	protected void SetIndices ()
	{		
		//----- up face generation -----//
		Triangulator triangulator = new Triangulator (_mergedPolygon.polygonRawData.ToArray ());
		int [] upFaceIndices = triangulator.Triangulate ();
		
		int borderIndexCount = BORDER_VERTEX_COUNT * 6;
		int [] indices = new int [upFaceIndices.Length + borderIndexCount];
		int indicesIndex = 0;
		
		for (; indicesIndex < upFaceIndices.Length; ++indicesIndex)
		{
			int index = upFaceIndices[indicesIndex];
			
			if (index == _mergedPolygon.duplicatedIndex0)
			{
				indices[indicesIndex] = _mergedPolygon.originalIndex0;
			}
			else if (index == _mergedPolygon.duplicatedIndex1)
			{
				indices[indicesIndex] = _mergedPolygon.originalIndex1;
			}
			else if (index > _mergedPolygon.duplicatedIndex0)
			{
				indices[indicesIndex] = index - 2;	
			}
			else
			{
				indices[indicesIndex] = index;
			}
		}
		
		//----- border generation -----//
		int mergedPolyCount = _mergedPolygon.polygonRawData.Count - 2;
		int startIndex = mergedPolyCount;
		int endIndex = startIndex + BORDER_VERTEX_COUNT * 4;
		
		for (int vertexIndex = startIndex; vertexIndex < endIndex; vertexIndex += 4, indicesIndex += 6)
		{
			indices[indicesIndex] = vertexIndex;
			indices[indicesIndex + 1] = vertexIndex + 1;
			indices[indicesIndex + 2] = vertexIndex + 2;
			indices[indicesIndex + 3] = vertexIndex + 1;
			indices[indicesIndex + 4] = vertexIndex + 3;
			indices[indicesIndex + 5] = vertexIndex + 2;
		}
		
		_mesh.triangles = indices;
	}
	
	public Mesh GetMesh ()
	{			
		return _mesh;	
	}
	
	public void Generate ()
	{
		_mesh.Clear ();
		
		MergedInnerAndOuter ();

		SetVertices ();
		SetIndices ();
			
		_mesh.RecalculateNormals ();
		_mesh.RecalculateBounds ();	
	}
}