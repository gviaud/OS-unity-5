using UnityEngine;
using System.Collections;
using System.IO;

public interface IDSMod
{	
	void GetModUI();
	
	void ApplyConf(ArrayList conf,bool reset);
	
	void SetToAll(bool reset);
	
	void SetModManger(DynShelterModManager mgr);
	
	string GetHashTag();
	
	bool IsGlobalMod();
	bool IsIntegrated();
	
	string SaveConf ();
	
	void LoadConf(string conf);
	
	string GetGameObj();
	
}
