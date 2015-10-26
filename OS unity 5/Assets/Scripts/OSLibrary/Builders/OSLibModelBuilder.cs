using UnityEngine;
using System.Collections;

public class OSLibModelBuilder : BaseLibBuilder<OSLibModel>
{
	public OSLibModelBuilder (XMLNode node) : base (node) 
	{}
		
	override public OSLibModel GetLibModel ()
	{
		string path = _node.GetValue ("@fichier");
		OSLibModel model = new OSLibModel (path);
		
		string position = _node.GetValue ("@position");
		if (!position.Equals (""))
			model.SetPosition (OSLibBuilderUtils.GetVector (position));
		
		string orientation = _node.GetValue ("@orientation");
		if (!orientation.Equals (""))
			model.SetOrientation (OSLibBuilderUtils.GetQuaternion (orientation));
		
		string scale = _node.GetValue ("@scale");
		if (!scale.Equals (""))
			model.SetScale (OSLibBuilderUtils.GetVector (scale));
		
		
		return model;
	}
}
