using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VertexGroup : List<int>
{	
	// help to find the translation vector when offset is updated
	private Vector3 _origin = Vector3.zero;
	
	protected Mesh _mesh;
	
	// direction in object space coordinate
	protected Vector3 _direction;
	
	// offset from starting to current position
	protected float _offset = 0;
	
	public VertexGroup (Mesh m) : base ()
	{
		_mesh = m;
	}
	
	public VertexGroup (Mesh m, Vector3 direction) : base ()
	{
		_mesh = m;
		_direction = direction;
		_direction.Normalize();
	}
	
	protected void updateVerticesPosition (float newOffset)
	{
		Vector3 currentPosition = _origin + _direction * _offset;
		Vector3 newPosition = _origin + _direction * newOffset;
		Vector3 translation = newPosition - currentPosition;
		Vector2 uvTranslation = new Vector2 (translation.x, translation.z);
		
		Vector3 [] vertices = _mesh.vertices;
		Vector2 [] uv = _mesh.uv;
		
		foreach (int index in this)
		{		
			if (index < uv.Length)
				uv[index] += uvTranslation;
					
			if (index < vertices.Length)
				vertices[index] += translation;
		}
		
		_mesh.uv = uv;
		_mesh.vertices = vertices;
		
		_offset = newOffset;
	}
	
	public float Offset
	{
		get 
		{
			return _offset;
		}
		
		set 
		{
			updateVerticesPosition (value);
		}
	}
	
	public void SetValueOnly (float offset)
	{
		_offset = offset;	
	}
}
