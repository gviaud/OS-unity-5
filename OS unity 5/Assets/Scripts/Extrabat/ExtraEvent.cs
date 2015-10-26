using UnityEngine;
using System.Collections.Generic;
using System.Net;


public static class ExtraEvent
{
	public static event System.Action<bool> auth;	
	public static event System.Action<string,string> startAuth;
	public static event System.Action<string> startSearch;
	public static event System.Action<Dictionary<string,string>> updateShopList;
	public static event System.Action<Dictionary<string,string>> updateClientList;
	public static event System.Action<string> setShop;
	public static event System.Action<string> sendIt;
	public static event System.Action<bool> pleaseWait;
	public static event System.Action<bool> uiActivation;

	
	#region fireEvents
	public static void fireNeedAuth()
	{
		Debug.Log("Authentification Need Fired");
		if(auth != null)
			auth(true);
	}
	public static void fireSuccessAuth()
	{
		Debug.Log("Authentification successfull Fired");
		if(auth != null)
			auth(false);
	}
	public static void fireStartAuth(string log,string mdp)
	{
		Debug.Log("Authentification Start Fired");
		if(startAuth != null)
			startAuth(log,mdp);
	}
	
	public static void fireShopListChange(Dictionary<string,string> dict)
	{
		Debug.Log("ShopListUpdate Fired");
		if(updateShopList != null)
			updateShopList(dict);
	}
	public static void fireSetShop(string s)
	{
		Debug.Log("Select shop Fired");
		if(setShop != null)
			setShop(s);
		
	}
	
	public static void fireClientListChange(Dictionary<string,string> dict)
	{
		Debug.Log("ClientListChange Fired");
		if(updateClientList != null)
			updateClientList(dict);
	}
	
	public static void fireStartSearch(string str)
	{
		Debug.Log("StartSearch Fired");
		if(startSearch != null)
			startSearch(str);
	}
	
	public static void fireSendImage(string id)
	{
		Debug.Log("SendImage Fired");
		if(sendIt != null)
			sendIt(id);
	}
	
	public static void firePleaseWait(bool b)
	{
		Debug.Log("PleaseWait Fired");
		if(pleaseWait != null)
			pleaseWait(b);
	}
	
	public static void fireUIActivation(bool b)
	{
		Debug.Log("UIActivation Fired");
		if(uiActivation != null)
			uiActivation(b);
	}
	#endregion

}
