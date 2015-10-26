using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSLibModule 
{
	// interbal use
	private string _searchedType;
	
	protected string _type;
	
	protected string _brand;
	
	protected List<OSLibTexture> _textures = new List<OSLibTexture> ();
	
	protected List<OSLibColor> _colors = new List<OSLibColor> ();
	
	protected bool IsTextureOfType (OSLibTexture tex)
	{
		return tex.GetTextureType ().Equals (_searchedType);
	}
	
	protected bool IsColorOfType (OSLibColor col)
	{
		return col.GetColorType ().Equals (_searchedType);
	}
	
	public OSLibModule (string type, string brand)
	{
		_type = type;
		_brand = brand;
	}
	
	public void SetModuleType (string type)
	{
		_type = type;	
	}
	
	public string GetModuleType ()
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
	
	public void AddTexure (OSLibTexture texture)
	{
		_textures.Add (texture);
	}
	
	public void AddColor (OSLibColor color)
	{
		_colors.Add (color);
	}
	
	public void SetTextureList (List<OSLibTexture> textures)
	{
		_textures = textures;	
	}
	
	public List<OSLibTexture> GetTextureList ()
	{
		return _textures;	
	}
	
	public void SetColorList (List<OSLibColor> colors)
	{
		_colors = colors;	
	}
	
	public List<OSLibColor> GetColorList ()
	{
		return _colors;	
	}
	
	public List<OSLibTexture> FindTextureList (string type)
	{
		_searchedType = type;
		
		List<OSLibTexture> textures = _textures.FindAll (IsTextureOfType);
		
		if (textures == null)
			textures = new List<OSLibTexture> ();
		
		return textures;
	}
	
	public List<OSLibColor> FindColorList (string type)
	{
		_searchedType = type;
		
		List<OSLibColor> colors = /*new List<OSLibColor> ();//= _*/_colors.FindAll (IsColorOfType);
			
//		if (type.Equals ("liner"))
//			colors = _colors;
//		else if(type.Equals ("coque"))
//			colors = _colors;
		
//		if (colors == null)
//			colors = new List<OSLibColor> ();
		
		return colors;
	}
}
