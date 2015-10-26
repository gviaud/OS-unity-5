using UnityEngine;
using System.Collections;
using System.IO;

public interface IPergolaAutoFeet
{
	
	void Init(Transform parent,FunctionConf_PergolaAutoFeet origin);
	
	void Build(Vector3 origine,int index,Vector4 nsew);
	
	void GetUI(/*GUISkin skin*/);
	
	void ToggleUI(string s);
	
	void Clear();
	
	void SaveOption(BinaryWriter buf);
	
	void LoadOption(BinaryReader buf);
	
	ArrayList GetConfig();
	
	void SetConfig(ArrayList al);
	
}
