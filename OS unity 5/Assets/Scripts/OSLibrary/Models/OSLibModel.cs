using UnityEngine;
using System.Collections;

public class OSLibModel
{
	protected string _path;
	
	protected string _basePath;
	
	protected Vector3 _scale = Vector3.one;
	
	protected Vector3 _position = Vector3.zero;
	
	protected Quaternion _orientation = Quaternion.identity;
	
	public OSLibModel (string path)
	{
		_path = path;
		_basePath = path;
	}
	
	public OSLibModel (string path, string basePath)
	{
		_path = path;
		_basePath = basePath;
	}
	
	public void SetPath (string path)
	{
		_path = path;
	}
	
	public string GetPath ()
	{
		return _path;
	}
	
	public string GetBasePath ()
	{
		return _basePath;	
	}
	
	public void SetScale (Vector3 scale)
	{
		_scale = scale;
	}
	
	public Vector3 GetScale ()
	{
		return _scale;
	}
	
	public void SetPosition (Vector3 position)
	{
		_position = position;
	}
	
	public Vector3 GetPosition ()
	{
		return _position;
	}
	
	public void SetOrientation (Quaternion orientation)
	{
		_orientation = orientation;
	}
	
	public Quaternion GetOrientation ()
	{
		return _orientation;
	}
}
