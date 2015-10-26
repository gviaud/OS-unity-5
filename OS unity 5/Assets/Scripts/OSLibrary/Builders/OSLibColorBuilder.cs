using UnityEngine;
using System.Collections;

public class OSLibColorBuilder : BaseLibBuilder<OSLibColor>
{
	public OSLibColorBuilder (XMLNode node) : base (node) 
	{}
		
	override public OSLibColor GetLibModel ()
	{
		int id;// = int.Parse (_node.GetValue ("@id"));
		int.TryParse (_node.GetValue ("@id"), out id);
		
		string thumbnail = _node.GetValue ("@thumbnail");
		string type = _node.GetValue ("@type");
		
		byte r;
		byte.TryParse (_node.GetValue ("@R"), out r);
		
		byte g;
		byte.TryParse (_node.GetValue ("@G"), out g);
		
		byte b;
		byte.TryParse (_node.GetValue ("@B"), out b);
		
		byte a = 255;
		if (!_node.GetValue ("@A").Equals (""))
			byte.TryParse (_node.GetValue ("@A"), out a);
		
		Color32 color = new Color32 (r, g, b, a);
		
		OSLibColor libColor = new OSLibColor(id, color, type, thumbnail);		

		if(_node.ContainsKey("@type2"))
		{
			
			Color32 color2 = new Color32 (r, g, b, a);
			string type2 = _node.GetValue ("@type2");
			if(_node.ContainsKey("@R2"))
			{
				byte r2;
				byte.TryParse (_node.GetValue ("@R2"), out r2);
			
				byte g2;
				byte.TryParse (_node.GetValue ("@G2"), out g2);
			
				byte b2;
				byte.TryParse (_node.GetValue ("@B2"), out b2);
				byte a2 = 255;
				if (!_node.GetValue ("@A").Equals (""))
					byte.TryParse (_node.GetValue ("@A"), out a2);
				color2 = new Color32 (r2, g2, b2, a2);
			}
			libColor = new OSLibColor(id, color, color2, type, type2, thumbnail);
		}
		
		OSLibBuilderUtils.FillLanguages (libColor, _node);
		
		return libColor;
	}
}
