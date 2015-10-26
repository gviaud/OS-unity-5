using System.Collections;

public class XMLNodeList: ArrayList 
{
	public XMLNode Pop()
	{
		XMLNode item = null;
	int d=0;
		if(this.Count==0)
			 d=1;
		try{
		item = (XMLNode)this[this.Count - 1];
		}
		catch(System.ArgumentOutOfRangeException e)
		{
			return null;
		}
		
		this.Remove(item);
		
		return item;
	}
	
	public int Push(XMLNode item)
	{
		Add(item);
		
		return this.Count;
	}
}