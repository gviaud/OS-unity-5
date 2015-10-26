using UnityEngine;
using System.Collections;

public class OSLibStairBuilder : BaseLibBuilder<OSLibStair>
{
	public OSLibStairBuilder (XMLNode node) : base (node) 
	{}
	
	override public OSLibStair GetLibModel ()
	{
		int id;
		int.TryParse (_node.GetValue ("@id"), out id);
		
		string type = _node.GetValue ("@type");
		string brand = _node.GetValue ("@marque");
		
		string thumbnail = _node.GetValue("thumbnail>0>@fichier");
		
		OSLibStair stair = new OSLibStair (id, type, brand, thumbnail);
		
		OSLibBuilderUtils.FillLanguages (stair, _node);
		
		return stair;
	}
}
