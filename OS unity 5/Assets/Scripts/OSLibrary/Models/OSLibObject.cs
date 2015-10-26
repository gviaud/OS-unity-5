using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSLibObject : MultiLanguages
{
	protected int _id = -1;
	
	protected string _type;
	
	protected OSLibCategory _categorie;
	
	protected string _brand;
	
	protected string _thumbnailPath;
	
	protected OSLibModel _model;
	
	protected OSLibModules _modules;
	
	protected OSLib _library;
	
	protected Texture2D _thumbnail;
	
	protected MultiLanguages _description = new MultiLanguages ();
	
	protected bool _mode2d = false;
	
	protected bool _allowscale = false;
	
	private const int c_brandIdLimit = 900;
	private int _ScaleGenerale = -1;

	public OSLibObject (int id, 
						string type, 
						string brand,
						string thumbnail,
						OSLib library,
						OSLibModel model,
						OSLibModules modules,
						OSLibCategory cat,
						bool mode2d,
						bool allowscale,
	                    int ScaleGenerale)
	{
		_id = id;
		_type = type;
		_brand = brand;
		_thumbnailPath = thumbnail;
		_library = library;
		_model = model;
		_modules = modules;
		_categorie = cat;
		_mode2d = mode2d;
		_allowscale = allowscale;
		_ScaleGenerale = ScaleGenerale;
	}
	
	public int GetId ()
	{
		return _id;	
	}
	
	public bool IsMode2D()
	{
		return _mode2d;
	}
	
	public bool IsAllowedToScale()
	{
		return _allowscale;
	}
	
	public OSLib GetLibrary ()
	{
		return _library;	
	}
	
	public void SetObjectType (string type)
	{
		_type = type;
	}
	
	public string GetObjectType ()
	{
		return _type;
	}
	public int GetObjectScaleGenerale()
	{
		return _ScaleGenerale;
	}
	
	public void SetBrand (string brand)
	{
		_brand = brand;
	}
	
	public string GetBrand ()
	{
		return _brand;	
	}
	
	public bool IsBrandObject()
	{
		//Debug.Log("brand id : " + getCategory().GetParentCategory().GetBrandId());
		//Debug.Log("brand id limit : " + c_brandIdLimit);
		return getCategory().GetParentCategory().GetBrandId() <= c_brandIdLimit;
			
	}

	public int GetParendBrandID()
	{
		return getCategory().GetParentCategory().GetBrandId();
	}
	
	public void SetThumbnailPath (string path)
	{
		_thumbnailPath = path;
	}
	
	public string GetThumbnailPath ()
	{
		return _thumbnailPath;	
	}
	
	public void SetModel (OSLibModel model)
	{
		_model = model;
	}
	
	public OSLibModel GetModel ()
	{
		return _model;	
	}
	
	public void SetModules (OSLibModules modules)
	{
		_modules = modules;	
	}
	
	public OSLibModules GetModules ()
	{
		return _modules;	
	}
	
	public OSLibCategory getCategory()
	{
		return _categorie;
	}
	
	public MultiLanguages GetDescription ()
	{
		return _description;
	}
	
	public void SetThumbnail (Texture2D thumbnail)
	{
		_thumbnail = thumbnail;
	}
	
	public Texture2D GetThumbnail ()
	{
		return _thumbnail;	
	}
}