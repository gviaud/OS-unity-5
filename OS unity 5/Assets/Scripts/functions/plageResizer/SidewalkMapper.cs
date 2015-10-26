using UnityEngine;
using System.Collections;

public class SidewalkMapper : BaseMapper
{
	public SidewalkMapper () : base () {}
	
	public SidewalkMapper (Mesh mesh) : base (mesh) {}
	
	public override void RecalculateTextureCoordinate ()
	{
		Vector2 [] uv = _mesh.uv;
		for (int i = 0; i < _mesh.vertexCount; ++i)
		{
			Vector3 vertex = _mesh.vertices[i];
			uv[i] = new Vector2 (vertex.x, vertex.z);	
		}
		_mesh.uv = uv;
	}
}
