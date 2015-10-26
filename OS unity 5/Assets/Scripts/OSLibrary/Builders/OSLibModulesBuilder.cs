using UnityEngine;
using System.Collections;

public class OSLibModulesBuilder : BaseLibBuilder<OSLibModules> 
{
	public OSLibModulesBuilder (XMLNode node) : base (node) 
	{}
		
	override public OSLibModules GetLibModel ()
	{
		string brand = _node.GetValue ("@marque");
		OSLibModules modules = new OSLibModules (brand);
		
		XMLNodeList moduleList = _node.GetNodeList ("module");
		
		foreach (XMLNode moduleNode in moduleList)
		{
			OSLibModuleBuilder moduleBuilder = new OSLibModuleBuilder (moduleNode, brand);
			OSLibModule module = moduleBuilder.GetLibModel ();
			
			modules.AddStandard (module);
		}
		
		return modules;
	}
}
