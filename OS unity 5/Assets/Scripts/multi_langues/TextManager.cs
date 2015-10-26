using UnityEngine;
using System.Collections;
using System.IO;
 
// originally found in: http://forum.unity3d.com/threads/35617-TextManager-Localization-Script
 
/// <summary>
/// TextManager
/// 
/// Reads PO files in the Assets\Resources\Languages directory into a Hashtable.
/// Look for "PO editors" as that's a standard for translating software.
/// 
/// Example:
/// 
/// load the language file:
///   TextManager.LoadLanguage("helptext-pt-br");
/// 
/// which has to contain at least two lines as such:
///   msgid "HELLO WORLD"
///   msgstr "OLA MUNDO"
/// 
/// then we can retrieve the text by calling:
///   TextManager.GetText("HELLO WORLD");
/// </summary>
public class TextManager : MonoBehaviour {
 
	private static TextManager instance;
	private static Hashtable textTable;
	private TextManager () {} 
	
	private static string _defaultFile = "strings_en";
 
	private static TextManager Instance 
	{
		get 
		{
			if (instance == null) 
			{
        		// Because the TextManager is a component, we have to create a GameObject to attach it to.
       	 		GameObject notificationObject = new GameObject("Default TextManager");
 
        		// Add the DynamicObjectManager component, and set it as the defaultCenter
       			instance = (TextManager) notificationObject.AddComponent(typeof(TextManager));
    		}
			return instance;
		}
	}
 
	public static TextManager GetInstance ()
	{
		return Instance;
	}	
 
	public static bool LoadLanguage (string filename)
	{
		GetInstance();
 
		if (filename == null)
		{
			//Debug.Log("[TextManager] loading default language.");
			textTable = null; // reset to default
			return false; // this means: call LoadLanguage with null to reset to default
		}
 
		string fullpath = "Languages/" +  filename; // the file is actually ".txt" in the end
 
		TextAsset textAsset = (TextAsset) Resources.Load(fullpath, typeof(TextAsset));
		if (textAsset == null) 
		{
			Debug.LogWarning("[TextManager] "+ fullpath +" file not found.");
			Debug.LogWarning("[TextManager] "+ _defaultFile +" file is use by default.");
			fullpath = "Languages/" +_defaultFile;
		    textAsset = (TextAsset) Resources.Load(fullpath, typeof(TextAsset));
		//	return false;
		}
 
		//Debug.Log("[TextManager] loading: "+ fullpath);
 
		if (textTable == null) 
		{
			textTable = new Hashtable();
		}
 
		textTable.Clear();
 
		StringReader reader = new StringReader(textAsset.text);
		string key = null;
		string val = null;
		string line;
		while ( (line = reader.ReadLine()) != null)
		{
			
			if(line.Contains(":"))
			{
				int pos = line.IndexOf(":");
				key = line.Substring(0,pos);
				while(key.StartsWith(" "))
				{
					key = key.Substring(1);
				}
				while(key.EndsWith(" "))
				{
					key = key.Substring(0, key.Length-1);
				}					
				val = line.Substring(pos+1);
				while(val.StartsWith(" "))
				{
					val = val.Substring(1);
				}
				while(val.EndsWith(" "))
				{
					val = val.Substring(0, val.Length-1);
				}
				textTable.Add(key, val);
			//	Debug.Log("key "+key);
			//	Debug.Log("val "+val);
				key = val = null;
				
			}
			/*if (line.StartsWith("msgid \""))
			{
    			key = line.Substring(7, line.Length - 8);
			}
			else if (line.StartsWith("msgstr \""))
			{
    			val = line.Substring(8, line.Length - 9);
			}
			else
			{
	    		if (key != null && val != null) 
	    		{
	    			// TODO: add error handling here in case of duplicate keys
	    			textTable.Add(key, val);
					key = val = null;
	    		} 
    		}*/
		}
 
		reader.Close();
 
		return true;
	}
 
 
	public static string GetText (string key)
	{
		if (key != null && textTable != null)
		{
			if (textTable.ContainsKey(key))
			{
				string result = (string)textTable[key];
				if (result.Length > 0)
				{
					key = result;
				}
			}
		}
		return key;
	}
}