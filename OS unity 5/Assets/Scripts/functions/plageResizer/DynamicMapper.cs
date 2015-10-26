using UnityEngine;
using System;
using System.Collections;

public class DynamicMapper : MonoBehaviour 
{
	protected bool _mappingDirty = false;
	
	protected Mesh _mesh;
	
	protected BaseMapper _mapper;
	
	// Use this for initialization
	void Awake () 
	{
		MeshFilter meshFilter = GetComponent<MeshFilter> ();
		
		if (meshFilter == null)
		{
			throw new ArgumentNullException ("MeshFilter");
		}
		
		_mesh = meshFilter.mesh;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (_mapper == null)
			return;
				
		if (_mappingDirty)
		{
			_mappingDirty = false;
			_mapper.RecalculateTextureCoordinate ();
		}
	}
	
	public void MarkMappingDirty()
	{
		_mappingDirty = true;
	}
	
	public void SetMapper (BaseMapper baseMapper)
	{
		_mapper = baseMapper;
		
		if (_mapper != null)
		{
			_mapper.SetMesh (_mesh);
			_mapper.RecalculateTextureCoordinate ();
		}
	}
}