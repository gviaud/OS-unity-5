using UnityEngine;
using System.Collections.Generic;

public class OSLibModules
{
	// internal use
	private string _searchedType;
	
	protected bool IsModuleOfType (OSLibModule module)
	{
		return module.GetModuleType ().Equals (_searchedType);
	}
	
	protected string _brand;
	
	protected List<OSLibModule> _standard = new List<OSLibModule> ();
	
	public OSLibModules (string brand)
	{
		_brand = brand;	
	}
	
	public void SetBrand (string brand)
	{
		_brand = brand;
	}
	
	public string GetBrand ()
	{
		return _brand;	
	}
	
	public void AddStandard (OSLibModule module)
	{
		_standard.Add (module);	
	}
	
	public OSLibModule FindModule (string type)
	{
		_searchedType = type;
		return _standard.Find (IsModuleOfType);	
	}
	
	public List<OSLibModule> GetStandardModuleList ()
	{
		return _standard;	
	}
}
