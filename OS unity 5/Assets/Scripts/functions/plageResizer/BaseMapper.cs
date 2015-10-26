using UnityEngine;
using System.Collections;

public abstract class BaseMapper
{
	protected Mesh _mesh;
	
	public void SetMesh (Mesh mesh)
	{
		_mesh = mesh;
		
	}
	
	public Mesh GetMesh ()
	{
		return _mesh;	
	}
	
	public BaseMapper (Mesh mesh)
	{
		_mesh = mesh;	
	}
	
	public BaseMapper ()
	{
		_mesh = null;
	}
	
	public abstract void RecalculateTextureCoordinate();
}
