using UnityEngine;
using System.Collections;

/*
 * 	Interface pour tout les plugins externe(style extrabat)
 * 	qui sont rattachés au GameObject "plugins"
 */

public interface PluginsOS3D
{
	
	string GetPluginName();
	
	void StartPlugin();
	
	void AuthorizePlugin(string s,bool b);
	
	bool isAuthorized();
	
}
