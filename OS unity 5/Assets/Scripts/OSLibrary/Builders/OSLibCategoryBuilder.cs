using UnityEngine;
using System.Collections;

public class OSLibCategoryBuilder : BaseLibBuilder<OSLibCategory> 
{
	protected OSLibCategory _parentCategory;
	protected OSLib _library;
	
	public OSLibCategoryBuilder (XMLNode node, OSLibCategory parent, OSLib library) : base (node) 
	{
		_parentCategory = parent;
		_library = library;
	}
		
	override public OSLibCategory GetLibModel ()
	{
		int id;// = int.Parse (_node.GetValue ("@id"));
		int.TryParse (_node.GetValue ("@id"), out id);
		
		int brand_id;// = int.Parse (_node.GetValue ("@id"));
		int.TryParse (_node.GetValue ("@brand_id"), out brand_id);
		
//		Debug.Log("BRAND_ID : " + brand_id);
		bool isPrimary;
		bool.TryParse(_node.GetValue("@primary"),out isPrimary);
				
		OSLibCategory category = new OSLibCategory (id, _parentCategory,isPrimary, brand_id);
		OSLibBuilderUtils.FillLanguages (category, _node);
		
		// add subCategorie
		XMLNodeList catList = _node.GetNodeList ("categorie");

		foreach (XMLNode catNode in catList)
		{
			OSLibCategoryBuilder catBuilder = 
							new OSLibCategoryBuilder (catNode, category, _library);
			
			OSLibCategory cat = catBuilder.GetLibModel ();
			
			category.AddChildCategory (cat);
		}

		// add object
		XMLNodeList objectList = _node.GetNodeList ("objet");
		
		if (objectList != null)
		{
			foreach (XMLNode objectNode in objectList)
			{
				OSLibObjectBuilder objBuilder = new OSLibObjectBuilder (objectNode, _library, category);
				OSLibObject obj = objBuilder.GetLibModel ();
				category.AddObject (obj);
			}
		}
		
		return category;
	}
}
