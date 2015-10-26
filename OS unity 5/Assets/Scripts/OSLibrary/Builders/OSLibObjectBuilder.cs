using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSLibObjectBuilder : BaseLibBuilder<OSLibObject> 
{
	// internal use
	private string _searchedBrand;
	
	protected static List<OSLibModules> _modulesList;
	
	protected OSLib _library;
	
	protected OSLibCategory _categorie;
		
	protected bool IsModulesOfBrand (OSLibModules mod)
	{
		return mod.GetBrand ().Equals (_searchedBrand);
	}
	
	public OSLibObjectBuilder (XMLNode node, OSLib library, OSLibCategory cat) : base (node) 
	{
		_library = library;
		
		_categorie = cat;
	}
	
	public static void SetModuleList (List<OSLibModules> modulesList)
	{
		_modulesList = modulesList;	
	}
		
	override public OSLibObject GetLibModel ()
	{
		int id;// = int.Parse (_node.GetValue ("@id"));
		int scaleGenerale = -1;
		int.TryParse (_node.GetValue ("@id"), out id);
		string type = _node.GetValue ("@type");
		string brand = _node.GetValue ("@marque");
		//string scaleGenerale = _node.GetValue ("@ScaleGenerale");
		int.TryParse (_node.GetValue ("@ScaleGenerale"), out scaleGenerale);
		//Debug.Log(" aaaa a a a a Lecture   " + scaleGenerale);
		string mode2D = _node.GetValue("@mode2d");
		string allowscaleStr = _node.GetValue("@allowscale");
		
		bool isMode2D;
		bool.TryParse (mode2D, out isMode2D);
		bool allowscale;
		bool.TryParse (allowscaleStr, out allowscale);
		//allowscale = true;
		string thumbnail = _node.GetValue("thumbnail>0>@fichier");
		
		OSLibModelBuilder modelBuilder = 
							new OSLibModelBuilder (_node.GetNode ("model>0"));
		
		OSLibModel model = modelBuilder.GetLibModel ();
		
		// create associated module from available ones
		OSLibModules modules = new OSLibModules(brand);
		
		XMLNodeList objModList = _node.GetNodeList ("modules>0>module");
		foreach (XMLNode objModNode in objModList)
		{
			string moduleType = objModNode.GetValue ("@type");
			OSLibModule module = GetModuleFromAvailables (brand, type, moduleType);
				
			modules.AddStandard (module);
		}
//		Debug.Log("isMode2D"+isMode2D);
		
		OSLibObject obj = new OSLibObject (id, type, brand, thumbnail, _library, model, modules,_categorie,isMode2D, allowscale,scaleGenerale);
		OSLibBuilderUtils.FillLanguages (obj, _node);
		
		return obj;
	}
	
	// create a list of texture and color according the object brand and type,
	// and the module type
	protected OSLibModule GetModuleFromAvailables (string brand, 
												   string typeInAvailable, 
												   string typeFromObj)
	{
		OSLibModule objModule = new OSLibModule (typeFromObj, brand);
		
		if (_modulesList == null)
			return objModule;
			
		_searchedBrand = brand;
		OSLibModules modules = _modulesList.Find (IsModulesOfBrand);
		
		if (modules == null)
			return objModule;
		
		OSLibModule module = modules.FindModule (typeInAvailable);
		
		if (module == null)
			return objModule;
		
		List<OSLibTexture> textures = module.FindTextureList (typeFromObj);
		List<OSLibColor> colors = module.FindColorList (typeFromObj);
		
		objModule.SetTextureList (textures);
		objModule.SetColorList (colors);
		
		return objModule;
	}
}