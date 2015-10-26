using UnityEngine;
using System.Collections;

// Class generant une forme de base represente par un polygone ferme
public class PolygonExtruder
{
	protected Polygon _polygon;
	
	protected float _quantity = 0; // hauteur de l'extrusion
	
	protected bool _upCap = false; // bouche t'on le haut de l'extrusion
	protected bool _bottomCap = false; // bouche t'on le bas de l'extrusion
	protected bool _inverseNormal = false; // inverse t'on les normals
	protected bool _inverseUpAndDown = false; // utile si _quantity < 0
	
	protected Mesh _mesh;
	
	public PolygonExtruder (Polygon polygon, 
		                    Mesh mesh,
		                    float quantity,
		                    bool upCap,
		                    bool bottomCap,
		                    bool inverseNormal)
	{
		_polygon = polygon;
		_mesh = mesh;
		_quantity = quantity;
		_upCap = upCap;
		_bottomCap = bottomCap;
		_inverseNormal = inverseNormal;
		
		if (_quantity > 0)
		{
			_inverseUpAndDown = true;	
		}
		
//		foreach (Vector3  v in _mesh.vertices)
//		{
//			Debug.Log (v);	
//		}
//		
//		foreach (int i in _mesh.triangles)
//		{
//			Debug.Log (i);
//		}
	}
	
	protected void SetNormals ()
	{
		_mesh.RecalculateNormals ();
		
		if (_polygon.IsClosed ())
		{
			// fusion les normales de debut et fin de mesh
			// pour eviter une jonction visible
			int lastIndex = _mesh.vertexCount - 2;
			Vector3 [] normals = _mesh.normals;
			
			Vector3 topNormal = normals[0] + normals[lastIndex];
			topNormal.Normalize ();
			normals[0] = topNormal;
			normals[lastIndex] = topNormal;
			
			Vector3 bottomNormal = normals[1] + normals[lastIndex + 1];
			bottomNormal.Normalize ();
			normals[1] = bottomNormal;
			normals[lastIndex + 1] = bottomNormal;
			
			_mesh.normals = normals;
		}
	}
	
	protected void SetVertices ()
	{
		LoopedList<Point2> points = _polygon.GetPoints ();
		
		if (_quantity != 0)
		{
			int verticesCount = points.Count * 2;
			
			if (_polygon.IsClosed ())
			{
				verticesCount += 2;
			}
			
			if (_upCap)
			{
				verticesCount += points.Count;
			}
			
			if (_bottomCap)
			{
				verticesCount += points.Count;
			}
			
			Vector3 [] vertices = new Vector3 [verticesCount];
			int vertexIndex = 0;
			
			for (int pointIndex = 0; pointIndex < points.Count; ++pointIndex, vertexIndex += 2)
			{
				Vector2 p0 = points[pointIndex];
				//Vector2 p0 = points[points.Count -1 - pointIndex];
				
				if (!_inverseUpAndDown)
				{
					vertices[vertexIndex] = new Vector3 (p0.x, 0, p0.y);
					vertices[vertexIndex + 1] = new Vector3 (p0.x, _quantity, p0.y);
				}
				else
				{
					vertices[vertexIndex + 1] = new Vector3 (p0.x, 0, p0.y);
					vertices[vertexIndex] = new Vector3 (p0.x, _quantity, p0.y);
				}
			}
			
			if (_polygon.IsClosed ())
			{
				// si le polygone est ferme, on double les points de departs
				Vector2 p0 = points[0];
				
				if (!_inverseUpAndDown)
				{
					vertices[verticesCount - 2] = new Vector3 (p0.x, 0, p0.y);
					vertices[verticesCount - 1] = new Vector3 (p0.x, _quantity, p0.y);
				}
				else
				{
					vertices[verticesCount - 1] = new Vector3 (p0.x, 0, p0.y);
					vertices[verticesCount - 2] = new Vector3 (p0.x, _quantity, p0.y);
				}
			}
			
			
			if (_upCap)
			{
				// duplique les vertices du "haut"
				for (int i = 0; i < points.Count; ++i, ++vertexIndex)
				{
					Vector2 p0 = points[i];
					
					if (!_inverseUpAndDown)
					{
						vertices[vertexIndex] = new Vector3 (p0.x, 0, p0.y);
					}
					else
					{
						vertices[vertexIndex] = new Vector3 (p0.x, _quantity, p0.y);
					}
				}
			}
			
			if (_bottomCap)
			{
				// duplique les vertices du "haut"
				for (int i = 0; i < points.Count; ++i, ++vertexIndex)
				{
					Vector2 p0 = points[i];
					
					if (!_inverseUpAndDown)
					{
						vertices[vertexIndex] = new Vector3 (p0.x, _quantity, p0.y);
					}
					else
					{
						vertices[vertexIndex] = new Vector3 (p0.x, 0, p0.y);
					}
				}
			}
			
			_mesh.vertices = vertices;
		}
		else if (_upCap || _bottomCap)
		{
			// pas d'extrusion, plan de la forme
			Vector3 [] vertices = new Vector3[points.Count];
			
			for (int pointIndex = 0; pointIndex < points.Count; ++pointIndex)
			{
				Vector2 p0 = points[pointIndex];
				vertices[pointIndex] = new Vector3 (p0.x, 0, p0.y);
			}
			
			_mesh.vertices = vertices;
		}
	}
	
	protected void SetIndices ()
	{
		LoopedList<Point2> points = _polygon.GetPoints ();
		
		int indicesCount = points.Count * 6;
		int sideCount = indicesCount;
		
		int [] indices = new int[indicesCount];
		
		if (_upCap || _bottomCap)
		{
			Vector2 [] cap = _polygon.ToVector2Array ();
			
			// indices des bouchons calcculé grace a la triangulation
			Triangulator tr = new Triangulator (cap);
			int [] capIndices = tr.Triangulate ();
			
			int capIndicesIndex = sideCount;
			int upAndBottomCapCount = capIndices.Length;
			
			if (_quantity == 0)
			{
				indicesCount = 0;
				sideCount = 0;
				capIndicesIndex = 0;
			}
			
			if (_upCap)
			{
				indicesCount += upAndBottomCapCount;
			}
			
			if (_bottomCap)
			{
				indicesCount += upAndBottomCapCount;
			}
			
			indices = new int [indicesCount];
			int upCapOffset = 0;
			
			if (_upCap)
			{
				if (_quantity != 0)
				{
					upCapOffset = points.Count * 2;
				}
				
				for (int upCapIndex = 0; 
					capIndicesIndex < sideCount + upAndBottomCapCount - 2; 
					capIndicesIndex += 3, upCapIndex += 3)
				{
					if (!_inverseNormal)
					{
						indices[capIndicesIndex] = capIndices[upCapIndex] + upCapOffset;
						indices[capIndicesIndex + 1] = capIndices[upCapIndex + 1] + upCapOffset;
						indices[capIndicesIndex + 2] = capIndices[upCapIndex + 2] + upCapOffset;
					}
					else
					{
						indices[capIndicesIndex] = capIndices[upCapIndex] + upCapOffset;
						indices[capIndicesIndex + 1] = capIndices[upCapIndex + 2] + upCapOffset;
						indices[capIndicesIndex + 2] = capIndices[upCapIndex + 1] + upCapOffset;
					}
				}
				
			}
			
			if (_bottomCap)
			{
				capIndicesIndex = indicesCount - upAndBottomCapCount;
				int bottomOffset = upCapOffset;
				
				if (_quantity != 0)
				{
					if (bottomOffset == 0)
					{
						bottomOffset = points.Count * 2;
					}
					else
					{
						bottomOffset += points.Count;
					}
				}
					
				for (int bottomCapIndex = 0; 
					capIndicesIndex < indicesCount - 2; 
					capIndicesIndex += 3, bottomCapIndex += 3)
				{
					if (_inverseNormal)
					{
						indices[capIndicesIndex] = capIndices[bottomCapIndex] + bottomOffset;
						indices[capIndicesIndex + 1] = capIndices[bottomCapIndex + 1] + bottomOffset;
						indices[capIndicesIndex + 2] = capIndices[bottomCapIndex + 2] + bottomOffset;
					}
					else
					{
						indices[capIndicesIndex] = capIndices[bottomCapIndex] + bottomOffset;
						indices[capIndicesIndex + 1] = capIndices[bottomCapIndex + 2] + bottomOffset;
						indices[capIndicesIndex + 2] = capIndices[bottomCapIndex + 1] + bottomOffset;
					}
				}
			}
		}
		
		if (_quantity != 0)
		{
			int indicesIndex = 0;
			for (int pointIndex = 0; 
				pointIndex < points.Count * 2 - 2; 
				indicesIndex += 6, pointIndex += 2)
			{
				if (!_inverseNormal)
				{
					indices[indicesIndex] = pointIndex;
					indices[indicesIndex + 1] = pointIndex + 1;
					indices[indicesIndex + 2] = pointIndex + 2;
					indices[indicesIndex + 3] = pointIndex + 1;
					indices[indicesIndex + 4] = pointIndex + 3;
					indices[indicesIndex + 5] = pointIndex + 2;
				}
				else
				{
					indices[indicesIndex] = pointIndex;
					indices[indicesIndex + 1] = pointIndex + 2;
					indices[indicesIndex + 2] = pointIndex + 1;
					indices[indicesIndex + 3] = pointIndex + 1;
					indices[indicesIndex + 4] = pointIndex + 2;
					indices[indicesIndex + 5] = pointIndex + 3;
				}
			}
			
			if (_polygon.IsClosed ())
			{
				if (!_inverseNormal)
				{
					int pointIndex = points.Count * 2 - 2;
					int lastIndex = _mesh.vertexCount;
					
					indices[indicesIndex] = pointIndex;
					indices[indicesIndex + 1] = pointIndex + 1;
					indices[indicesIndex + 2] = lastIndex - 2;
					indices[indicesIndex + 3] = pointIndex + 1;
					indices[indicesIndex + 4] = lastIndex - 1;
					indices[indicesIndex + 5] = lastIndex - 2;
				}
				else
				{
					int pointIndex = points.Count * 2 - 2;
					int lastIndex = _mesh.vertexCount;
					
					indices[indicesIndex] = pointIndex;
					indices[indicesIndex + 1] = lastIndex - 2;
					indices[indicesIndex + 2] = pointIndex + 1;
					indices[indicesIndex + 3] = pointIndex + 1;
					indices[indicesIndex + 4] = lastIndex - 2;
					indices[indicesIndex + 5] = lastIndex - 1;
				}
			}
		}
		
		_mesh.triangles = indices;
	}
	
	public void Generate ()
	{
		_mesh.Clear ();
		
		SetVertices ();
		SetIndices ();
		SetNormals ();
			
		_mesh.RecalculateBounds ();	
	}
	
	public Mesh GetMesh ()
	{			
		return _mesh;	
	}
	
	public void SetQuantity (float quantity)
	{
		_quantity = quantity;
		_inverseUpAndDown = quantity > 0;	

		SetVertices ();
	}
	
	public float GetQuantity ()
	{
		return _quantity;	
	}
	
	public Polygon GetMeshPolygon ()
	{
		return _polygon;	
	}
	
	public bool IsUpCapped ()
	{
		return _upCap;
	}
	
	public bool IsBottomCapped ()
	{
		return _bottomCap;
	}
}
