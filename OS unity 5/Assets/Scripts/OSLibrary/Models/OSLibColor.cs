using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSLibColor : MultiLanguages
{
	protected int _id = -1;
	
	protected string _type;
	
	protected string _type2;
	
	protected string _thumbnailPath;
	
	protected Texture2D _thumbnail;
	
	public Color _color = new Color ();
	
	public Color _color2 = new Color ();
	
	public OSLibColor (int id, Color color, string type, string thumbnail)
	{
		_id = id;
		_color = color;
		_type = type;
		_thumbnailPath = thumbnail;
	}	
	
	public OSLibColor (int id, Color color, Color color2, string type, string type2, string thumbnail)
	{
		_id = id;
		_color = color;
		_type = type;
		_color2 = color2;
		_type2 = type2;
		_thumbnailPath = thumbnail;
	}
	
	public int GetId ()
	{
		return _id;	
	}
	
	public void SetColorType (string type)
	{
		_type = type;
	}
	
	public string GetColorType ()
	{
		return _type;
	}
	
	public string GetColorType2 ()
	{
		return _type2;
	}
	
	public void SetColor (Color color)
	{
		_color = color;	
	}
	
	public Color GetColor ()
	{
		return _color;	
	}	
	
	public Color GetColor2 ()
	{
		return _color2;	
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
		if(_thumbnail==null)
		{
			_thumbnail = new Texture2D(1,1);
			_thumbnail.name = GetDefaultText();
			_thumbnail.SetPixel(0,0, _color);
			_thumbnail.Apply();
		}
		return _thumbnail;	
	}
}
