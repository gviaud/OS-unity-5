using UnityEngine;
using System.Collections;

public class OSLibTextureBuilder : BaseLibBuilder<OSLibTexture> 
{
	public OSLibTextureBuilder (XMLNode node) : base (node) 
	{}
		
	override public OSLibTexture GetLibModel ()
	{
		int id;// = int.Parse (_node.GetValue ("@id"));
		int.TryParse (_node.GetValue ("@id"), out id);
		
		string file = _node.GetValue ("@fichier");
		string thumbnail = _node.GetValue ("@thumbnail");
		string type = _node.GetValue ("@type");
		string normalmap = _node.GetValue ("@normal");
		string hueMask = _node.GetValue ("@maskHue");
		string specularMask = _node.GetValue ("@maskSpecular");

		OSLibTexture texture = new OSLibTexture (id, type, file, thumbnail,normalmap,hueMask,specularMask);

		
		if(_node.ContainsKey("@type2"))
		{
			
			string file2 = _node.GetValue ("@fichier");
			string type2 = _node.GetValue ("@type2");
			if(_node.ContainsKey("@fichier2"))
			{
				file2 = _node.GetValue ("@fichier2");
			}
			texture = new OSLibTexture (id, type, type2, file, file2, thumbnail);
		}
		
		OSLibBuilderUtils.FillLanguages (texture, _node);
		
		string scaleAttr = _node.GetValue ("@scale");
		
		if (!scaleAttr.Equals (""))
		{
			float scale;
			float.TryParse (scaleAttr, out scale);
			
			texture.SetScale (scale);
		}
		
		return texture;
	}
}
