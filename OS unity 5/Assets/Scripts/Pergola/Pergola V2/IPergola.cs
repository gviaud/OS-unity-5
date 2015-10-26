using UnityEngine;
using System.Collections;
using System.IO;

public interface IPergola
{
	
	void Build(Function_Pergola.PElement m,BuildDatas bd,PergolaRule pr,int index,int off7);
	
	void GetUI(GUISkin skin);
	
	void PSave(BinaryWriter buffer);
	
	void PLoad(BinaryReader buffer);
	
	void Clear();
	
}
