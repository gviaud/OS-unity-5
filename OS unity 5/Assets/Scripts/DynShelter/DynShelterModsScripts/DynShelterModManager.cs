using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DynShelterModManager : MonoBehaviour
{
	private FunctionConf_Dynshelter _parent;
	
	private Dictionary<string,ArrayList> _globalMods = new Dictionary<string, ArrayList>();
	private ArrayList _localMods  = new ArrayList();
	
	//---------------------------------
	
	void Awake()
	{
		_parent = GetComponent<FunctionConf_Dynshelter>();
		_parent.SetModManager(this);
	}
	
	void OnEnable()
	{
		foreach(string key in _globalMods.Keys)
		{
			foreach(MonoBehaviour mod in _globalMods[key])
				mod.enabled = true;
		}
	}
	
	void OnDisable()
	{
		foreach(string key in _globalMods.Keys)
		{
			foreach(MonoBehaviour mod in _globalMods[key])
				mod.enabled = false;
		}
	}
	
	//---------------------------------
	
	public void GetUI()
	{
		foreach(string key in _globalMods.Keys)
		{
			if(_globalMods[key].Count > 0)
			{
				if(!((IDSMod)_globalMods[key][0]).IsIntegrated())
				{
					((IDSMod)_globalMods[key][0]).GetModUI();
				}
			}
		}
		
		foreach(IDSMod mod in _localMods)
		{
			if(!mod.IsIntegrated())
			{
				mod.GetModUI();
			}
		}
	}
	
	public void GetIntegratedGUI()
	{
		foreach(string key in _globalMods.Keys)
		{
			if(_globalMods[key].Count > 0)
			{
				if(((IDSMod)_globalMods[key][0]).IsIntegrated())
				{
					((IDSMod)_globalMods[key][0]).GetModUI();
				}
			}
		}
		
		foreach(IDSMod mod in _localMods)
		{
			if(mod.IsIntegrated())
			{
				mod.GetModUI();
			}
		}
	}
	
	public void TestModuleForGlobalMods(GameObject g)
	{
		foreach(Component cp in g.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType().GetInterface("IDSMod")!= null)
			{
				string Htag = ((IDSMod)cp).GetHashTag();
				bool isGlobal = ((IDSMod)cp).IsGlobalMod();
				if(isGlobal)
				{
					if(_globalMods.ContainsKey(Htag))
					{
						_globalMods[Htag].Add(cp);
						((IDSMod)cp).SetModManger(this);
						
						((IDSMod)_globalMods[Htag][0]).SetToAll(true);
					}
					else
					{
						_globalMods.Add(Htag,new ArrayList());
						_globalMods[Htag].Add(cp);
						((IDSMod)cp).SetModManger(this);
					}
				}
				
			}
		}
	}
	
	public void RemoveModFrom(GameObject g)
	{
		foreach(Component cp in g.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType().GetInterface("IDSMod")!= null)
			{
				string Htag = ((IDSMod)cp).GetHashTag();
				bool isGlobal = ((IDSMod)cp).IsGlobalMod();
				
				if(isGlobal)
				{
					if(_globalMods.ContainsKey(Htag))
					{
						if(_globalMods[Htag].Contains(cp))
							_globalMods[Htag].Remove(cp);
					}
				}
			}
		}
	}
	
	public void UpdateLocalMods(GameObject g,bool getAllMods = false)
	{
		_localMods.Clear();
		
		foreach(Component cp in g.GetComponentsInChildren<MonoBehaviour>())
		{
			if(cp.GetType().GetInterface("IDSMod")!= null)
			{
				string Htag = ((IDSMod)cp).GetHashTag();
				bool isGlobal = ((IDSMod)cp).IsGlobalMod();
				
				if(!isGlobal || getAllMods)
				{
						_localMods.Add(cp);
						((IDSMod)cp).SetModManger(this);
				}
				
			}
		}
	}
	
	public void ClearManager()
	{
		_globalMods = new Dictionary<string, ArrayList>();
		_localMods  = new ArrayList();
	}
	
	//---------------------------------
	
	public void ApplyGlobal(string HTag,ArrayList conf,bool reset)
	{
		if(_globalMods.ContainsKey(HTag))
		{
			foreach(IDSMod mod in _globalMods[HTag])
			{
				mod.ApplyConf(conf,reset);	
			}
		}
		else
		{
			Debug.LogWarning(HTag + "Mod no longer exist");	
		}
	}
	
	public void SaveMods(BinaryWriter buf)
	{
		buf.Write(_localMods.Count);		
		foreach(IDSMod mod in _localMods)
		{
			buf.Write(mod.GetHashTag());
			buf.Write(mod.SaveConf());
			buf.Write(mod.GetGameObj());
		}
	}
	
	public string[] LoadMods(BinaryReader buf)
	{
		int nb = buf.ReadInt32();
		string[] cf = new  string[3*nb];
		for(int i=0;i<nb;i++)
		{		
			cf[3*i] = buf.ReadString();
			cf[3*i+1] = buf.ReadString();
			cf[3*i+2] = buf.ReadString();
		}
		
		return cf;
	}
	
	public void ApplyLoadConf(string [] cf)
	{
		int nb = cf.Length;
		
		for(int i=0;i<nb;i=i+3)
		{
			string htag = cf[i];
			string conf = cf[i+1];
			string obj = cf[i+2];
			
			foreach(IDSMod m in _localMods)
			{
				if(m.GetHashTag() == htag && m.GetGameObj() == obj)
				{
					m.LoadConf(conf);
				}
			}
		}
	}
	
//	public void SaveLocalMods(BinaryWriter buf)
//	{
//		buf.Write(_localMods.Count); // nombre de mods
//		foreach(IDSMod mod in _localMods)
//		{
//			buf.Write(mod.GetHashTag());//HTag du mod
//			mod.SaveConf(buf);			//Conf
//		}
//	}
//	
//	public ArrayList LoadLocalMods(BinaryReader buf) // doit d'abord faire un updateLocalMods
//	{
//		ArrayList tempConf = new ArrayList();
//		int nb = buf.ReadInt32();//NB de mods
//		
//		if(nb != _localMods.Count)
//			Debug.Log(GetType().ToString()+" Error : nombre de mods différent du nombre de configs sauvé");
//		
//		for(int i=0;i<nb;i++)
//		{
//			string HTag = buf.ReadString();//HTAg du mod
//			foreach(IDSMod m in _localMods)
//			{
//				if(m.GetHashTag() == HTag)
//				{
//					
//					
//					m.LoadConf(buf);	//conf
//				}
//			}
//		}
//		
//		return tempConf();
//	}
//	
//	public void SaveGlobalMods(BinaryWriter buf)
//	{
//		buf.Write(_globalMods.Keys.Count); // nombre de mods
//		foreach(string key in _globalMods.Keys)
//		{
//			buf.Write(key);//HTag du mod
//			
//			((IDSMod)_globalMods[key][0]).SaveConf(buf);	//Conf
//		}
//	}
//	
//	public void LoadGlobalMods(BinaryReader buf)
//	{
//		int nb = buf.ReadInt32();//NB de mods
//		
//		if(nb != _globalMods.Keys.Count)
//			Debug.Log(GetType().ToString()+" Error : nombre de mods différent du nombre de configs sauvé");
//		
//		for(int i=0;i<nb;i++)
//		{
//			string key = buf.ReadString();
//			((IDSMod)_globalMods[key][0]).LoadConf(buf);
//			((IDSMod)_globalMods[key][0]).SetToAll();
//		}
//	}
}
