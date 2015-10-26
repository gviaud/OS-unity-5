using UnityEngine;
using System.Collections;

// mapping plannaire prenant en compte la profondeur (axe Y) du mesh et 
// la hauteur du degrade (en m) pour calculé les uv
// application pour le liner de piscine
public class LinerScatteringMapper : PlannarMapper
{
	protected static float GRADIENT_HEIGHT = -3;
	
	public LinerScatteringMapper (Mesh mesh) : base (mesh, Vector3.forward, UVChannel.uv1)
	{}
	
	protected override void SetUvs ()
	{
		_mesh.RecalculateBounds ();
		Bounds meshBounds = _mesh.bounds;	
		
		float maxY = meshBounds.center.y + meshBounds.extents.y;
		float minY = maxY - GRADIENT_HEIGHT;
		
		Vector2 [] uvs = new Vector2[_mesh.vertexCount];
		
		for (int vertexIndex = 0; vertexIndex < _mesh.vertexCount; ++vertexIndex)
		{
			Vector3 vertex = _mesh.vertices[vertexIndex];
			
			float u = vertex.x;
			float v = maxY - vertex.y;
			
			if (v >= minY)
			{
				v = 1;	
			}
			else
			{
				v /= minY;	
			}
			
//			Debug.Log (vertex + " " +  u + " " + v);
			
			Vector2 uv = new Vector2 (u, 1 - v);
			uvs[vertexIndex] = uv;
		}
		
		_mesh.uv2 = uvs;
	}
}
