using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSLibTexture : MultiLanguages
{
	protected int _id = -1;
	
	protected string _type;
	
	protected string _type2;
	
	protected string _file;
	
	protected string _file2;

	protected string _thumbnailPath;

	protected string _normal;
	protected string _HueMask;
	protected string _SpecularMask;

	protected Texture2D _thumbnail;
	
	protected float _scale = 1.0f;
	
	public OSLibTexture (int id, string type, string file, string thumbnail,string normal, string hueMask, string specularMask)
	{
		_id = id;
		_type = type;
		_file = file;
		_thumbnailPath = thumbnail;
		_normal = normal;
		_HueMask = hueMask;
		_SpecularMask = specularMask;
	}	
	
	public OSLibTexture (int id, string type, string type2, string file, string file2, string thumbnail)
	{
		_id = id;
		_type = type;
		_file = file;
		_file2 = file2;
		_type2 = type2;
		_thumbnailPath = thumbnail;
	}
	
	public int GetId ()
	{
		return _id;	
	}
	
	public void SetTextureType (string type)
	{
		_type = type;
	}
	
	public string GetTextureType2 ()
	{
		return _type2;
	}
	
	public string GetTextureType ()
	{
		return _type;
	}
	
	public void SetThumbnailPath (string path)
	{
		_thumbnailPath = path;	
	}
	
	public string GetThumbnailPath ()
	{
		return _thumbnailPath;	
	}
	
	public void SetThumbnail(Texture2D thumbnail)
	{
		_thumbnail = thumbnail;	
	}
	
	public Texture2D GetThumbnail ()
	{
		return _thumbnail;	
	}
	
	public void SetFilePath (string path)
	{
		_file = path;	
	}
	
	public string GetFilePath ()
	{
		return _file;	
	}
	
	public string GetFilePath2 ()
	{
		return _file2;	
	}

	public string GetNormalPath ()
	{
		return _normal;	
	}
	public string GetHueMaskPath ()
	{
		return _HueMask;	
	}
	public string GetSpecularMaskPath ()
	{
		return _SpecularMask;	
	}

	public void SetScale (float scale)
	{
		_scale = scale;
	}
	
	public float GetScale ()
	{
		return _scale;	
	}
}
