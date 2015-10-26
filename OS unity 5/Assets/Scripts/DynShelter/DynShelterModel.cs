using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynShelterModel
{

#region public vars
	
#endregion

#region private vars			
	private Dictionary<int,string[]> _blocs   = new Dictionary<int,string[]>();
	private Dictionary<int,string[]> _facades  = new Dictionary<int,string[]>();
	private Dictionary<int,string[]> _extrems = new Dictionary<int,string[]>();
	
	private Dictionary<string,Texture2D> _thumbnails = new Dictionary<string, Texture2D>();
	
	private Color32[] _colorList;
	
	private FunctionConf_Dynshelter _parent;
	
	private ArrayList _startConf = new ArrayList();
	
	private bool _abrisFixe = false;
	
#endregion
	
#region Fcns
	
	//Constructor -----------------------
	public DynShelterModel(FunctionConf_Dynshelter parent)
	{
		_parent = parent;
	}
	
	//Function----------------------------
	public void ParseConfFile(TextAsset confFile)
	{
//		Debug.Log("START MOTHERF*CKING PARSING");
		XMLParser parser = new XMLParser ();
		XMLNode rootNode = parser.Parse (confFile.text).GetNode ("settings>0");
		
		//COLORS----------------------------------------------------------------
		XMLNode colorsNode = rootNode.GetNode("colors>0");
		XMLNodeList colorsList = colorsNode.GetNodeList ("color");
		
		_colorList = new Color32[colorsList.Count];
		int colorIndex = 0;
		foreach (XMLNode colorNode in colorsList)
		{
			byte r;
			byte.TryParse (colorNode.GetValue ("@R"), out r);
			
			byte g;
			byte.TryParse (colorNode.GetValue ("@G"), out g);
			
			byte b;
			byte.TryParse (colorNode.GetValue ("@B"), out b);
			
			byte a = 255;
			if (!colorNode.GetValue ("@A").Equals (""))
				byte.TryParse (colorNode.GetValue ("@A"), out a);
			
			Color32 color = new Color32 (r, g, b, 255);
			_colorList[colorIndex] = color;
			colorIndex++;
		}
		
		//STYLES----------------------------------------------------------------
		XMLNode stylesNode = rootNode.GetNode("styles>0");
		XMLNodeList stylesList = stylesNode.GetNodeList ("style");
		
		foreach(XMLNode styleNode in stylesList)
		{
			string name = styleNode.GetValue("@nom");
			string thumbPath = styleNode.GetValue("@thumb");
			
			_thumbnails.Add(name,_parent.LoadImg(thumbPath));
		}
		
		//SIZES-----------------------------------------------------------------
		XMLNode sizesNode = rootNode.GetNode("sizes>0");
		XMLNodeList sizesList = sizesNode.GetNodeList ("size");
		int tailleUnique=-1;
		foreach(XMLNode styleNode in sizesList)
		{
			string typ = styleNode.GetValue("@type");
			
			int taille;
			int.TryParse(styleNode.GetValue("@t"),out taille);
			if(tailleUnique==-1)
			{
				tailleUnique = taille;	
				_abrisFixe = true;							
			}
			else
			{
				if(tailleUnique != taille)
					_abrisFixe=false;
			}
			
			string styles = styleNode.GetValue("@styles");
			string[] sList = styles.Split(',');
			switch (typ)
			{
			case "bloc":
			case "multiBloc":
				AddStylesToSize(ref _blocs,taille,sList);
				break;
			case "facade":
				AddStylesToSize(ref _facades,taille,sList);
				break;
			case "extremite":
				AddStylesToSize(ref _extrems,taille,sList);
				break;
			}
		}
		
		//DefaultConf----------------------------------------------------------------
		XMLNode defaultConf = rootNode.GetNode("defaultConf>0");
		XMLNodeList defautModules = defaultConf.GetNodeList ("module");
		_startConf.Clear();
		
		foreach (XMLNode module in defautModules)
		{
			string typ = module.GetValue("@type");
			
			int taille;
			int.TryParse(module.GetValue("@t"),out taille);
			
			string style = module.GetValue("@style");
						
			_startConf.Add(typ);
			_startConf.Add(taille);
			_startConf.Add(style);
		}
		
//		Debug.Log("BLOC COUNT>"+_blocs.Count);
//		Debug.Log("FACADE COUNT>"+_facades.Count);
//		Debug.Log("EXTREM COUNT>"+_extrems.Count);
	}
	
	private void  AddStylesToSize(ref Dictionary<int,string[]> dict,int key,string[] list)
	{
		if(dict.ContainsKey(key))
		{
			string[] tmpList = dict[key];
			string[] newList = new string[tmpList.Length + list.Length];
			
			int index = 0;
			foreach(string s in tmpList)
				newList[index++] = s;
			foreach(string s in list)
				newList[index++] = s;
			
			dict[key] = newList;
		}
		else
		{
			dict.Add(key,list);
		}
	}
		
	//HasModule --------------------------
	public bool HasBlocsOfSize(int s)
	{
		return _blocs.ContainsKey(s);	
	}
	public bool HasFacadeOfSize(int s)
	{
		return _facades.ContainsKey(s);	
	}
	public bool HasExtremOfSize(int s)
	{
		return _extrems.ContainsKey(s);	
	}
	
	public string GetFirstStyle(FunctionConf_Dynshelter.ModuleType typ,int taille)
	{
		string output = "";
		switch (typ)
		{
		case FunctionConf_Dynshelter.ModuleType.bloc:
			output = _blocs[taille][0];
			break;
		case FunctionConf_Dynshelter.ModuleType.facade:
			output = _facades[taille][0];
			break;
		case FunctionConf_Dynshelter.ModuleType.extremite:
			output = _extrems[taille][0];
			break;
		}
		return output;
	}
	
	//GetStyles --------------------------
	public string[] GetStylesNameOfSize(int taille,FunctionConf_Dynshelter.ModuleType typ)
	{
		switch(typ)
		{
		case FunctionConf_Dynshelter.ModuleType.bloc:
			if(_blocs.ContainsKey(taille))
				return _blocs[taille];
			else
				return null;
			break;
			
		case FunctionConf_Dynshelter.ModuleType.facade:
			if(_facades.ContainsKey(taille))
				return _facades[taille];
			else
				return null;
			break;
			
		case FunctionConf_Dynshelter.ModuleType.extremite:
			if(_extrems.ContainsKey(taille))
				return _extrems[taille];
			else
				return null;
			break;
		
		default:
			return null;
			break;
		}
	}
	
//	public Texture2D[] GetStylesThumbsOfSize(int taille,FunctionConf_Dynshelter.ModuleType typ)
//	{
//		string[] tmpNames = GetStylesNameOfSize(taille,typ);
//		Texture2D[] outThumbs = new Texture2D[tmpNames.Length];
//		
//		for(int i=0;i<outThumbs.Length;i++)
//		{
//			outThumbs[i] = _thumbnails[tmpNames[i]];	
//		}
//		
//		return outThumbs;
//	}
	
	public Texture2D[] GetStylesThumbsOfSize(string[] list)
	{
		Texture2D[] outThumbs = new Texture2D[list.Length];
		
		for(int i=0;i<outThumbs.Length;i++)
		{
			outThumbs[i] = _thumbnails[list[i]];	
		}
		
		return outThumbs;
	}
	
	public Color32[] GetColorList(){return _colorList;}
	
	//Get --------------------------------
	
	public ArrayList GetdefaultConf()
	{
		if(_startConf!= null)
			return _startConf;
		else return new ArrayList();
	}
	
	public Color32 GetColor(int i)
	{
		if(i < _colorList.Length)
			return _colorList[i];
		else
			return _colorList[0];
	}
	
	public bool IsAbriFixe()
	{
		return _abrisFixe;
	}
	
	
#endregion

}
