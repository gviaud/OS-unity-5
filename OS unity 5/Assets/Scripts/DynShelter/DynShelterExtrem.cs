using UnityEngine;
using System.Collections;

public class DynShelterExtrem : DynShelterModule
{
	public DynShelterExtrem(int size,string style,FunctionConf_Dynshelter.ModuleType type,Color32 color,FunctionConf_Dynshelter parent)
		: base (size,style,type,color,parent)
	{
		
	}
	
//	public override bool MoveModule(float delta,bool isSelected,bool actOnPrevs,bool actOnNexts,DynShelterModule stop = null)
//	{
//		//TODO
//		
//		return true;
//	}
	
	public override void SetGameObject(GameObject g)
	{
		_moduleGameObj = g;
	}
	
	public override void InsertBetween(DynShelterModule prev, DynShelterModule next)
	{
		if(next == null) // la facade est le dernier module
		{
			SetPos(false,prev.GetExtPos(AR).z);
			prev.SetNextModule(this);
			this.SetPrevModule(prev);
		}
		else //prev = null, la facade est le premier module
		{
			SetPos(false,next.GetExtPos(AV).z);
			next.SetPrevModule(this);
			this.SetNextModule(next);
		}
	}
	
	//--vv--FCN's spécifiques--vv--------
		
}
