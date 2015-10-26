using UnityEngine;
using System.Collections.Generic;

public class OSLibStairs : MultiLanguages
{
	protected List<OSLibStair> _stairs = new List<OSLibStair> ();
	
	protected int _id;
	
	protected int _dependency;
	
	public OSLibStairs (int id, int dependency)
	{
		_id = id;
		_dependency = dependency;
	}
	
	public void AddStair (OSLibStair stair)
	{
		_stairs.Add (stair);	
	}
	
	public int GetId ()
	{
		return _id;	
	}
	
	public int GetDependency ()
	{
		return _dependency;	
	}
	
	public List<OSLibStair> GetStairList ()
	{
		return _stairs;	
	}
}
