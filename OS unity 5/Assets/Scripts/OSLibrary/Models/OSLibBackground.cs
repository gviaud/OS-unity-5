using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSLibBackground : MultiLanguages
{
	protected string name;
	
	public OSLibBackground (string _name)
	{
		name = _name;
	}
	
	public string GetBackgroundName ()
	{
		return name;	
	}
}
