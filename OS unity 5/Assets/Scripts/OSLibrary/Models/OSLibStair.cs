using UnityEngine;
using System.Collections;

public class OSLibStair : MultiLanguages
{
	protected int _id;
	
	protected string _type;
	
	protected string _brand;
	
	protected string _thumbnailPath;
	
	protected Texture2D _thumbnail;
	
	public OSLibStair (int id, 
					   string type, 
					   string brand,
					   string thumbnail)
	{
		_id = id;
		_type = type;
		_brand = brand;
		_thumbnailPath = thumbnail;	
	}
	
	public int GetId ()
	{
		return _id;	
	}
	
	public void SetStairType (string type)
	{
		_type = type;
	}
	
	public string GetStairType ()
	{
		return _type;
	}
	
	public void SetBrand (string brand)
	{
		_brand = brand;
	}
	
	public string GetBrand ()
	{
		return _brand;	
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
}
