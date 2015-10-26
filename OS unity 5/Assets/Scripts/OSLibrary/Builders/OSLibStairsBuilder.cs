using UnityEngine;
using System.Collections;

public class OSLibStairsBuilder : BaseLibBuilder<OSLibStairs> 
{
	public OSLibStairsBuilder (XMLNode node) : base (node) 
	{}
	
	override public OSLibStairs GetLibModel ()
	{
		int id;
		int.TryParse (_node.GetValue ("@id"), out id);
		
		int dependency;
		int.TryParse (_node.GetValue ("@dependance"), out dependency);
		
		OSLibStairs stairs = new OSLibStairs (id, dependency);
		
		OSLibBuilderUtils.FillLanguages (stairs, _node);
		
		XMLNodeList stairCatList = _node.GetNodeList ("objet");
		foreach (XMLNode stairNode in stairCatList)
		{
			OSLibStairBuilder stairBuilder = new OSLibStairBuilder (stairNode);
			OSLibStair stair = stairBuilder.GetLibModel ();
			
			stairs.AddStair (stair);
		}
		
		return stairs;
	}
}
