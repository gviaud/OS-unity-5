using UnityEngine;
using System.Collections;

public class OSLibBuilder : BaseLibBuilder<OSLib>
{
	protected string _assetBundlePath;
	protected int _version;
	
	public OSLibBuilder (XMLNode node, string assetBundlePath, int version) : base (node) 
	{
		_assetBundlePath = assetBundlePath;
		_version = version;
	}
		
	override public OSLib GetLibModel ()
	{
		int id;// = int.Parse (_node.GetValue ("@id"));
		int.TryParse (_node.GetValue ("@id"), out id);
		
//		int version;
//		int.TryParse (_node.GetValue ("@version"), out version);
		
		string oneShotVersion = _node.GetValue ("@versionOneShot");
		string name = _node.GetValue ("@nom");
		
		string parent = _node.GetValue("@parent");
		
		OSLib lib = new OSLib (id, _version, oneShotVersion, name, _assetBundlePath);
		
		// add modules
		XMLNodeList modulesList = _node.GetNodeList ("modules");
		
		foreach (XMLNode modulesNode in modulesList)
		{
			OSLibModulesBuilder modulesBuilder = new OSLibModulesBuilder (modulesNode);
			OSLibModules modules = modulesBuilder.GetLibModel ();
			
			lib.AddModule (modules);
		}
		
		OSLibObjectBuilder.SetModuleList (lib.GetModulesList ());
		
		// add categories
		XMLNodeList objectsList = _node.GetNodeList ("objets");
		
		foreach (XMLNode objectNode in objectsList)
		{
			OSLibCategoryBuilder categoryBuilder = new OSLibCategoryBuilder (objectNode, null, lib);
			OSLibCategory category = categoryBuilder.GetLibModel ();
			
			lib.AddCategory (category);
		}
		
		// add stair
		XMLNodeList stairsList = _node.GetNodeList ("accessoires>0>categorie");
		
		foreach (XMLNode stairNode in stairsList)
		{
			OSLibStairsBuilder stairsBuilder = new OSLibStairsBuilder (stairNode);
			OSLibStairs stairs = stairsBuilder.GetLibModel ();
			
			lib.AddStairs (stairs);
		}
		
		//add bg
		XMLNodeList bgList = _node.GetNodeList ("backgrounds>0>texture");
		foreach (XMLNode bgNode in bgList)
		{
			string bgName = bgNode.GetValue ("@fichier");
			OSLibBackground bg = new OSLibBackground(bgName);
			
			lib.AddBg(bg);
		}
		
		return lib;
	}
}
