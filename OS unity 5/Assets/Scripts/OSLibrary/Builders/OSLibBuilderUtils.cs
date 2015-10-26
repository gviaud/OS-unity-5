using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSLibBuilderUtils
{
	// interbal use
	private static int _searchedId;
	
	protected static bool IsCategoryWithId (OSLibCategory category)
	{
		if (category.GetBrandId()!=-1)
			return category.GetBrandId () == _searchedId;
			
		return category.GetId () == _searchedId;
	}
	
	protected static bool IsStairsWithId (OSLibStairs stairs)
	{
		return stairs.GetId () == _searchedId;
	}
	
	protected static bool IsStairWithId (OSLibStair stair)
	{
		return stair.GetId () == _searchedId;
	}
	
	// create vector3 from a formated string
	public static Vector3 GetVector (string stringVector)
	{
		Vector3 vector = new Vector3 ();
		string [] coords = stringVector.Split (',');
		
		for (int coordIndex = 0; coordIndex < coords.Length; ++coordIndex)
		{
			float coord;
			float.TryParse (coords [coordIndex], out coord);
			
			vector[coordIndex] = coord;
		}

		return vector;
	}
	
	// create quaternion from a formated string
	public static Quaternion GetQuaternion (string stringQuat)
	{
		Quaternion quat = new Quaternion ();
		string [] coords = stringQuat.Split (',');
		
		for (int coordIndex = 0; coordIndex < coords.Length; ++coordIndex)
		{
			float coord;
			float.TryParse (coords [coordIndex], out coord);
			
			quat[coordIndex] = coord;
		}

		return quat;
	}
	
	// fill a multilanguages object from languages detected from an XMLNode
	public static void FillLanguages (MultiLanguages multiLang, XMLNode node)
	{
		XMLNodeList names = node.GetNodeList ("nom>0>text");
		
		foreach (XMLNode name in names)
		{
			string lang = name.GetValue ("@lang");
			string text = name.GetValue ("_text");
			
			string defaultAtt = name.GetValue ("@defaut");
			bool isDefault;
			bool.TryParse (defaultAtt, out isDefault);			
			
			multiLang.AddText (lang, text, isDefault);
		}
		
		XMLNodeList parent = node.GetNodeList ("parent>0>text");
		
		foreach (XMLNode name in parent)
		{
			string lang = name.GetValue ("@lang");
			string text = name.GetValue ("_text");
			
			string defaultAtt = name.GetValue ("@defaut");
			bool isDefault;
			bool.TryParse (defaultAtt, out isDefault);
			
			//Debug.Log (text);
			multiLang.AddText (lang + "_parent", text, isDefault);
		}
	}
	
	public static OSLibTexture GetFirstTextureOfType (OSLibModule module, string type)
	{
		foreach (OSLibTexture texture in module.GetTextureList ())
		{
			if (texture.GetTextureType () == type)
				return texture;
		}
		
		return null;
	}
	
	public static OSLibColor GetFirstColorOfType (OSLibModule module, string type)
	{
		foreach (OSLibColor color in module.GetColorList ())
		{
			if (color.GetColorType () == type)
				return color;
		}
		
		return null;
	}
	
	// merge multiple librairies into one to fit the interface
	public static OSLib MergeLibraries (ICollection<OSLib> libraries)
	{
		OSLib mergedLibrary = new OSLib (-1, -1, "", "Merge", "");
		
		foreach (OSLib library in libraries)
		{
			foreach (OSLibCategory catLvl1 in library.GetCategoryList ())
			{
				_searchedId = catLvl1.GetBrandId();
				if(_searchedId==-1)
					_searchedId = catLvl1.GetId ();
				OSLibCategory libraryCatLvl1 = mergedLibrary.GetCategoryList ().Find (IsCategoryWithId);
				if (libraryCatLvl1 == null)
				{
					libraryCatLvl1 = new OSLibCategory (catLvl1.GetId (), null, catLvl1, catLvl1.GetBrandId());
					mergedLibrary.AddCategory (libraryCatLvl1);	
				}
				
				foreach (OSLibCategory catLvl2 in catLvl1.GetCategoryList ())
				{
					_searchedId = catLvl2.GetBrandId();
					if(_searchedId==-1)
						_searchedId = catLvl2.GetId ();
					OSLibCategory libraryCatLvl2 = libraryCatLvl1.GetCategoryList ().Find (IsCategoryWithId);
					if (libraryCatLvl2 == null)
					{
						libraryCatLvl2 = new OSLibCategory (catLvl2.GetId (), libraryCatLvl1, catLvl2, catLvl2.GetBrandId());
						libraryCatLvl1.AddChildCategory (libraryCatLvl2);
					}
					
					foreach (OSLibObject obj in catLvl2.GetObjectList ())
					{
						libraryCatLvl2.AddObject (obj);
					}
				}
				
				foreach (OSLibObject obj in catLvl1.GetObjectList ())
				{
					libraryCatLvl1.AddObject (obj);
				}
					
			}
			
			foreach (OSLibStairs stairs in library.GetStairsList ())
			{
				_searchedId = stairs.GetId ();
				OSLibStairs mergedStairs = mergedLibrary.GetStairsList ().Find (IsStairsWithId);
				if (mergedStairs == null)
				{
					mergedStairs = new OSLibStairs (stairs.GetId (), stairs.GetDependency ());
					mergedLibrary.AddStairs (mergedStairs);
				}
				
				foreach (OSLibStair stair in stairs.GetStairList ())
				{
					_searchedId = stair.GetId ();
					OSLibStair mergedStair = mergedStairs.GetStairList ().Find (IsStairWithId);
					if (mergedStair == null)
					{
						mergedStair = new OSLibStair (stair.GetId (), 
							                          stair.GetStairType (), 
							                          stair.GetBrand (), 
							                          stair.GetThumbnailPath ());
						
						mergedStairs.AddStair (mergedStair);
					}
				}
			}
		}
		
		/*
		 //Debug
		 foreach (OSLibCategory catLvl1 in mergedLibrary.GetCategoryList ())
		{
			Debug.Log("Catégorie niveau 1 : "+catLvl1.GetDefaultText()+" id "+catLvl1.GetId()+" brandid "+catLvl1.GetBrandId());
			foreach (OSLibCategory catLvl2 in catLvl1.GetCategoryList ())
			{
				Debug.Log("--------------- Catégorie niveau 2 : "+catLvl2.GetDefaultText()+" id "+catLvl2.GetId()+" brandid "+catLvl2.GetBrandId());	
				foreach (OSLibObject obj in catLvl2.GetObjectList())
				{
					Debug.Log("---------------- -------------- Objets : "+obj.GetDefaultText()+", cat 1 : "+obj.getCategory().GetParentCategory().GetId()+", cat2 : "+obj.getCategory().GetId());	
				}
			}
		}*/
		
	
		OSLib mergedAndOrderedLibrary = new OSLib (-1, -1, "", "mergedAndOrdered", "");
		 //Debug
		foreach (OSLibCategory catLvl1 in mergedLibrary.GetCategoryList ())
		{
			//OSLibCategory catLvl1Ordered = catLvl1;
			OSLibCategory catLvl1Ordered = new OSLibCategory(
				catLvl1.GetId(), 
				catLvl1.GetParentCategory(),
				catLvl1.isPrimaryType(), 
				catLvl1.GetBrandId());
			
			catLvl1Ordered.SetLanguagesDictionary(catLvl1.GetLanguagesDictionary());
			catLvl1Ordered.SetDefaultLanguage(catLvl1.GetDefaultLanguage());

			foreach (OSLibCategory catLvl2 in catLvl1.GetCategoryList ())
			{
				//catLvl1Ordered.AddChildCategory(catLvl2);
				
				if(catLvl1Ordered.GetCategoryList().Count==0)
					catLvl1Ordered.AddChildCategory(catLvl2);	
				else
				{
					bool max = true;
					for (int countCat=0; countCat < catLvl1Ordered.GetCategoryList().Count; countCat++)
					{
						OSLibCategory catLvl2ReOrdered = catLvl1Ordered.GetCategoryList()[countCat];
						if(catLvl2ReOrdered.GetBrandId()>catLvl2.GetBrandId())
						{
							catLvl1Ordered.AddChildCategoryAtPosition(catLvl2,countCat);
							max = false;
							break;
						}
					}
					if(max)
					{
						catLvl1Ordered.AddChildCategory(catLvl2);
					}
				}
			}
			
			
			if(mergedAndOrderedLibrary.GetCategoryList().Count==0)
				mergedAndOrderedLibrary.AddCategory (catLvl1Ordered);	
			else
			{
				bool max = true;
				for (int countCat=0; 
					countCat<mergedAndOrderedLibrary.GetCategoryList().Count;countCat++)
				{
					OSLibCategory catLvl1ReOrdered = mergedAndOrderedLibrary.GetCategoryList()[countCat];
					if(catLvl1ReOrdered.GetBrandId()>catLvl1Ordered.GetBrandId())
					{
						mergedAndOrderedLibrary.AddCategoryAtPosition(catLvl1Ordered,countCat);
						max = false;
						break;
					}
				}
				if(max)
				{
					mergedAndOrderedLibrary.AddCategory(catLvl1Ordered);
				}
			}
		}		
		
		/*
		//Debug
		Debug.Log("----------------------------------------------------------");
		Debug.Log("----------------------------------------------------------");
		foreach (OSLibCategory catLvl1 in mergedAndOrderedLibrary.GetCategoryList ())
		{
			Debug.Log("Catégorie niveau 1 : "+catLvl1.GetDefaultText()+" id "+catLvl1.GetId()+" brandid "+catLvl1.GetBrandId());
			foreach (OSLibCategory catLvl2 in catLvl1.GetCategoryList ())
			{
				Debug.Log("--------------- Catégorie niveau 2 : "+catLvl2.GetDefaultText()+" id "+catLvl2.GetId()+" brandid "+catLvl2.GetBrandId());	
				foreach (OSLibObject obj in catLvl2.GetObjectList())
				{
					Debug.Log("---------------- -------------- Objets : "+obj.GetDefaultText()+"  isMode 2D : "+obj.IsMode2D().ToString()+", cat 1 : "+obj.getCategory().GetParentCategory().GetId()+", cat2 : "+obj.getCategory().GetId());	
				}
			}
		}
		*/
		
		return mergedAndOrderedLibrary;
		
	//	return mergedLibrary;
	}
}