using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pointcube.Global;
using Pointcube.Utils;
using Pointcube.InputEvents;

public class LoadShaderPresets : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Dictionary<string,float> getConfigPlage(string index){
		Dictionary<string, float> conf = new Dictionary<string, float>();
		string fullpath = "shaders/configs/plage/" + index/* + ".txt"*/; // the file is actually ".txt" in the end
		
		TextAsset textAsset = (TextAsset) Resources.Load(fullpath, typeof(TextAsset));
		
		if(textAsset != null){
			StringReader reader = new StringReader(textAsset.text);
			
			
			
			if(reader != null)
			{
				string floatString = "";
				
				do
				{
					floatString = reader.ReadLine();
					
					if(floatString != null)
					{
						string szkey = floatString.Substring(0, floatString.IndexOf("="));
						float fvalue = float.Parse(floatString.Substring(floatString.IndexOf("=") + 1));
						conf.Add(szkey,fvalue);
						
					}
					
				}while(floatString != null);
				
				reader.Close();
			}
			return conf;
		}else{
			return null;
		}
	}
	public Dictionary<string,float> getConfigMargelle(string index){
		Dictionary<string, float> conf = new Dictionary<string, float>();
		string fullpath = "shaders/configs/margelle/" + index/* + ".txt"*/; // the file is actually ".txt" in the end
		
		TextAsset textAsset = (TextAsset) Resources.Load(fullpath, typeof(TextAsset));
		
		if(textAsset != null){
			StringReader reader = new StringReader(textAsset.text);
			
			
			
			if(reader != null)
			{
				string floatString = "";
				
				do
				{
					floatString = reader.ReadLine();
					
					if(floatString != null)
					{
						string szkey = floatString.Substring(0, floatString.IndexOf("="));
						float fvalue = float.Parse(floatString.Substring(floatString.IndexOf("=") + 1));
						conf.Add(szkey,fvalue);
						
					}
					
				}while(floatString != null);
				
				reader.Close();
			}
			return conf;
		}else{
			return null;
		}
	}
	public Dictionary<string,float> getConfigMuret(string index){
		Dictionary<string, float> conf = new Dictionary<string, float>();
		string fullpath = "shaders/configs/muret/" + index/* + ".txt"*/; // the file is actually ".txt" in the end
		
		TextAsset textAsset = (TextAsset) Resources.Load(fullpath, typeof(TextAsset));
		
		if(textAsset != null){
			StringReader reader = new StringReader(textAsset.text);
			
			
			
			if(reader != null)
			{
				string floatString = "";
				
				do
				{
					floatString = reader.ReadLine();
					
					if(floatString != null)
					{
						string szkey = floatString.Substring(0, floatString.IndexOf("="));
						float fvalue = float.Parse(floatString.Substring(floatString.IndexOf("=") + 1));
						conf.Add(szkey,fvalue);
						
					}
					
				}while(floatString != null);
				
				reader.Close();
			}
			return conf;
		}else{
			return null;
		}
	}
}
