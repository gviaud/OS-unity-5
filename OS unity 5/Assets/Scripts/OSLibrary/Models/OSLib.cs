using UnityEngine;
using System.Collections.Generic;

public class OSLib
{
	protected int _id = -1;
	
	protected int _version = -1;
	
	protected string _oneShotVersion = "";
	
	protected string _name = "";
	
	protected string _assetBundlePath = "";
	
	protected List<OSLibCategory> _categories = new List<OSLibCategory> ();
	
	protected List<OSLibModules> _modules = new List<OSLibModules> ();
	
	protected List<OSLibStairs> _stairs = new List<OSLibStairs> ();
	
	protected List<OSLibBackground> _bgs = new List<OSLibBackground> ();
	
	public OSLib (int id, int version, string oneShotVersion, string name, string assetBundlePath)
	{
		_id = id;
		_version = version;
		_oneShotVersion = oneShotVersion;
		_name = name;
		_assetBundlePath = assetBundlePath;
	}
	
	public int GetId ()
	{
		return _id;	
	}
	
	public void SetVersion (int version)
	{
		_version = version;
	}
	
	public int GetVersion ()
	{
		return _version;	
	}
	
	public void SetOneShotVersion (string version)
	{
		_oneShotVersion = version;	
	}
	
	public string GetOneShotVersion ()
	{
		return _oneShotVersion;	
	}
	
	public void SetAssetBundlePath (string path)
	{
		_assetBundlePath = path;	
	}
	
	public string GetAssetBundlePath ()
	{
		return _assetBundlePath;	
	}
	
	public void SetName (string name)
	{
		_name = name;	
	}
	
	public string GetName ()
	{
		return _name;	
	}
	
	public void AddModule (OSLibModules modules)
	{
		_modules.Add (modules);
	}
	
	public void AddCategory (OSLibCategory category)
	{
		_categories.Add (category);	
	}
	
	public void AddCategoryAtPosition (OSLibCategory category, int position)
	{
		if(_categories.Count>position)
			_categories.Insert(position,category);
		else
			_categories.Add(category);
	}
	
	public void AddStairs (OSLibStairs stairs)
	{
		_stairs.Add (stairs);	
	}
	
	public void AddBg (OSLibBackground bg)
	{
		_bgs.Add (bg);	
	}
	
	public List<OSLibModules> GetModulesList ()
	{
		return _modules;
	}
	
	public List<OSLibCategory> GetCategoryList ()
	{
		return _categories;	
	}
	
	public List<OSLibStairs> GetStairsList ()
	{
		return _stairs;	
	}
	
	public List<OSLibBackground> GetBgList ()
	{
		return _bgs;	
	}
}