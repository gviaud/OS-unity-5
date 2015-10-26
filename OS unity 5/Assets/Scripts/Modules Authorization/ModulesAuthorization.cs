using UnityEngine;
using System.Collections;

public static class ModulesAuthorization
{
	#region listener(s)
	public static event System.Action<string,bool> moduleAuth;
	#endregion
	
	#region fireEvent(s)
	public static void FireAuthorizeModule(string str,bool b)
	{
		// si str == le nom d'un module, celui ci de débloque
		// si str == "lockAll" (par exemple) on bloque tout
//		Debug.Log("Authorisation du module : "+str+" >>> " +b);
		if(moduleAuth != null)
			moduleAuth(str,b);
	}
	#endregion
	
}
