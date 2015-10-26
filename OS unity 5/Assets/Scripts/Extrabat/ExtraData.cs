using UnityEngine;
using System.Collections.Generic;

public class ExtraData{
	
	#region Auth Data
	private string _privateKey = "";
	private string _extra_appName = "os3d";
	private string _extra_OS3dKey = "Ebh7$dmkNewGzC#xYyTaRWOtAnUqL9vHg4MIrQKPujNJ$obwFsfBE2TlCkG86fUh";//"FOURNIE_PAR_EXTRABAT";
	#endregion
	
	#region Pref keys
	public static string pk_privateKey = "PRIVATEKEY";
	public static string pk_selectedShop = "SHOP";
	#endregion
	
	#region userData
	private Dictionary<string,string> _shops;
	private string _selectedShop;
	#endregion
	
	public ExtraData () //CONSTRUCTEUR
	{
		_shops = new Dictionary<string, string>();
	}
	
	#region accessors/mutators
	public string getAppName()
	{
		return _extra_appName;	
	}
	
	public void setPrivateKey(string k)
	{
		_privateKey = k;
		PlayerPrefs.SetString(pk_privateKey,k);
	}
	public string getPrivateKey()
	{
		return _privateKey;	
	}
	
	public string getOS3DKey()
	{
		return _extra_OS3dKey;	
	}
	
	public void addShop(string id, string name)
	{
		_shops.Add(id,name);
	}
	public Dictionary<string,string> getShops()
	{
		return _shops;	
	}
	public void clearShops()
	{
		_shops.Clear();
	}
	
	public void setSelectedShop(string i)
	{
		_selectedShop = i;
		PlayerPrefs.SetString(pk_selectedShop,i);
	}
	public string getSelectedShop()
	{
		return _selectedShop;
	}
	#endregion
	
	#region save/load PlayerPrefs
	
	#endregion
	
}
