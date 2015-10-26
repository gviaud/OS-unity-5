using UnityEngine;
using System.Collections;

public class OSLibModuleBuilder : BaseLibBuilder<OSLibModule> 
{
	protected string _brand;
	
	public OSLibModuleBuilder (XMLNode node, string brand) : base (node) 
	{
		_brand = brand;
	}
		
	override public OSLibModule GetLibModel ()
	{
		string type = _node.GetValue ("@type");
		
		OSLibModule module = new OSLibModule (type, _brand);
		
		// add textures
		XMLNodeList textureList = _node.GetNodeList ("textures>0>texture");
		
		foreach (XMLNode textureNode in textureList)
		{
			OSLibTextureBuilder textureBuilder = new OSLibTextureBuilder (textureNode);
			OSLibTexture texture = textureBuilder.GetLibModel ();
			
			module.AddTexure (texture);
		}
		
		// add colors
		XMLNodeList colorList = _node.GetNodeList ("couleurs>0>couleur");
		
		foreach (XMLNode colorNode in colorList)
		{
			OSLibColorBuilder colorBuilder = new OSLibColorBuilder (colorNode);
			OSLibColor color = colorBuilder.GetLibModel ();
			
			module.AddColor (color);
		}
		
		return module;
	}
}