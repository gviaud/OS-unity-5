using UnityEngine;
using System.Collections;
//using Microsoft.Win32;
using System;

public class RegisterRW{

//	// Use this for initialization
//	void Start () {
//	
//	}
//	
//	// Update is called once per frame
//	void Update () {
//	
//	}
	
	//TODO changer les alias quand plus en debug
	
	//private static string RegPath = "Software\\JavaSoft\\Prefs\\OS3DV4_Debug";
	
	public static string serializedDataID = "data";
	public static string loginDataID = "log";
	public static string mdpDataID = "mdp";
	public static string lld = "lld";
	
	/*--------------------------BDR Read/Write ToolSet---------------------------*/
	
	public static string ReadValue(string key)
	{
		//READ Dans la base de registre uniquement pc
//		string returnedValue;
//		RegistryKey Nkey = Registry.LocalMachine;
//		try
//		{
//			RegistryKey valKey = Nkey.OpenSubKey(RegPath, true);
//			if(valKey == null)
//			{
//				returnedValue = null;
//			}
//			else
//			{
//				returnedValue = valKey.GetValue(key).ToString();
//				valKey.Close();
//			}
//		}
//		catch(Exception er)
//		{
////			Debug.Log(er.Message);
//			returnedValue = null;
//		}
//		finally
//		{
//			Nkey.Close();
//		}
//		return returnedValue;
		
		string outStr = "";
		
		if(PlayerPrefs.HasKey(key))
		{
			outStr = PlayerPrefs.GetString(key);
		}
		return outStr;
	}

	public static void WriteValue(string key,object keyValue)
	{
		//Write dans la brd uniquement pc
//		RegistryKey Nkey = Registry.LocalMachine;
//		try
//		{
//			RegistryKey valKey = Nkey.OpenSubKey(RegPath, true);
//			if(valKey == null)
//			{
//				Nkey.CreateSubKey(RegPath);
//			}
//			valKey.SetValue(key, keyValue) ;
//		}
//		catch(Exception er)
//		{
////			Debug.Log(er.Message.ToString());
//		}
//		finally
//		{
//			Nkey.Close();
//		}
		
		PlayerPrefs.SetString(key,(string) keyValue);
	}
	
	
}
