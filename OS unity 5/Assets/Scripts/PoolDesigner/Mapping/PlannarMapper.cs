using UnityEngine;
using System.Collections;

// mapping plannaire selon une direction donnee
public class PlannarMapper
{
	protected Mesh _mesh;
	
	protected Vector3 _axis;
	
	protected UVChannel _uvChannel;
	
	protected float _scale = 1.0f;

	public PlannarMapper (Mesh mesh, Vector3 axis, UVChannel uvChannel=UVChannel.uv0, float scale=1.0f)
	{
		_mesh = mesh;
		_axis = axis;
		_axis.Normalize ();
		_uvChannel = uvChannel;
		_scale = scale;
	}
	
	protected virtual void SetUvs ()
	{
		_mesh.RecalculateBounds ();
		Bounds meshBounds = _mesh.bounds;	
		Vector3 center = meshBounds.center;
		Vector3 upVertex = center + Vector3.up;
		Vector3 upVector = upVertex - center;
		upVector.Normalize ();
		
		Quaternion plannarRot = Quaternion.FromToRotation (upVector, _axis);
		Matrix4x4 plannarMatrix = Matrix4x4.TRS (Vector3.zero, plannarRot, Vector3.one);
		
		Vector2 [] uvs = new Vector2[_mesh.vertexCount];
		
		for (int vertexIndex = 0; vertexIndex < _mesh.vertexCount; ++vertexIndex)
		{
			Vector3 vertex = _mesh.vertices[vertexIndex];
			Vector3 transformedVertex = plannarMatrix.MultiplyPoint (vertex);
			
			float u = transformedVertex.x/_scale;
			float v = transformedVertex.z/_scale;
			
			Vector2 uv = new Vector2 (u, v);
			uvs[vertexIndex] = uv;
		}
		
		switch (_uvChannel)
		{
		case UVChannel.uv0:
			_mesh.uv = uvs;
			break;
			
		case UVChannel.uv1:
			_mesh.uv2 = uvs;
			break;
		}
	}
	
	public void Generate ()
	{
		SetUvs ();	
	}
}
