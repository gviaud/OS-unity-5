using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSLibCategory : MultiLanguages
{
	protected int _id = -1;
	
	protected int _brand_id = -1;
	
	protected OSLibCategory _parent;
	
	protected List<OSLibObject> _objects = new List<OSLibObject> ();
	
	protected List<OSLibCategory> _categories = new List<OSLibCategory> ();
	
	protected bool _isPrimary = false;
	
	public OSLibCategory (int id, OSLibCategory parent)
	{
		_id = id;
		_parent = parent;
	}
	
	public OSLibCategory (int id, OSLibCategory parent, bool prime, int brand_id)
	{
		_id = id;
		_parent = parent;
		_isPrimary = prime;
		_brand_id = (brand_id==0?-1:brand_id);
	}
	
	public OSLibCategory (int id, 
						  OSLibCategory parent, 
						  MultiLanguages languages, int brand_id) : base (languages)
	{
		_id = id;
		_parent = parent;
		_brand_id = (brand_id==0?-1:brand_id);
	}
	
	public int GetId ()
	{
		return _id;	
	}
	
	public int GetBrandId ()
	{
		return _brand_id;	
	}
	
	public OSLibCategory GetParentCategory ()
	{
		return _parent;	
	}
	
	public void AddObject (OSLibObject obj)
	{
		_objects.Add (obj);	
	}
	
	public void AddChildCategory (OSLibCategory cat)
	{
		_categories.Add (cat);
	}
	
	public void AddChildCategoryAtPosition (OSLibCategory category, int position)
	{
		if(_categories.Count>position)
			_categories.Insert(position,category);
		else
			_categories.Add(category);
	}
	
	public List<OSLibCategory> GetCategoryList ()
	{
		return _categories;	
	}
	
	public List<OSLibObject> GetObjectList ()
	{
		return _objects;	
	}
	
	public bool isPrimaryType()
	{
		return _isPrimary;
	}
	 
}
