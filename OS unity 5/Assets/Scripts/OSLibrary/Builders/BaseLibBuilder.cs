using UnityEngine;
using System.Collections;

public abstract class BaseLibBuilder<T> : ILibBuilder<T>
{
	protected XMLNode _node;
	
	public BaseLibBuilder (XMLNode node)
	{
		_node = node;	
	}
	
	public void SetNode (XMLNode node)
	{
		_node = node;
	}
	
	public XMLNode GetNode ()
	{
		return _node;	
	}
	
	//create model of type T from an XMLNode
	public abstract T GetLibModel ();
}
