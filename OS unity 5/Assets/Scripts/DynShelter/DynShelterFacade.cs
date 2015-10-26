using UnityEngine;
using System.Collections;

public class DynShelterFacade : DynShelterModule
{
	
	public DynShelterFacade(int size,string style,FunctionConf_Dynshelter.ModuleType type,Color32 color,FunctionConf_Dynshelter parent)
		: base (size,style,type,color,parent)
	{
		
	}
	
	public override bool MoveModuleV2 (float delta, DynShelterModule startPoint,bool actOnPrevs=true,bool actOnNexts=true)
	{
//		Debug.Log ("delta : "+delta+", actOnPrevs : "+actOnPrevs+", actOnNexts : "+actOnNexts);
		if(_nextModule != null)
		{
//			Debug.Log ("next module");
			if(startPoint != _nextModule)
			{
//				Debug.Log ("startPoint != _nextModule");
				return _nextModule.MoveModuleV2(delta,null);
			}
			else
			{
//				Debug.Log ("false startPoint != _nextModule");
				SetPos(false,GetMyPos(false));
				return true;
			}
		}
		
		else if(_prevModule != null)
		{
//			Debug.Log ("prev module");
			if(startPoint != _prevModule)
			{
//				Debug.Log ("startPoint != _nextModule");
				return _prevModule.MoveModuleV2(delta,null);
			}
			else
			{
//				Debug.Log ("false startPoint != _nextModule");
				SetPos(false,GetMyPos(true));
				return true;
			}
		}
		else
			return false;
	}
	
	public override void SetGameObject(GameObject g)
	{
//		_moduleGameObj = g;
		
		base.SetGameObject(g);
		
		Transform tmp = _moduleGameObj.transform.FindChild("facade");
		tmp.localPosition = new Vector3(0,tmp.localPosition.y,0);
		if(!tmp.GetComponent<BoxCollider>())
		{
			tmp.gameObject.AddComponent<BoxCollider>();
		}
		
		Bounds intBds = tmp.GetComponent<BoxCollider>().bounds;
		_off7Int = intBds.size.z ;/// 2f; 
		_off7Ext = _off7Int;
		_length = _off7Ext*2;
		_width = intBds.size.x;
		_height = intBds.size.y;
		
		tmp.GetComponent<BoxCollider>().enabled = false;
	}
	
	public override void InsertBetween(DynShelterModule prev, DynShelterModule next)
	{
		if(next == null && prev == null)
			return;
		
		if(next == null) // la facade est le dernier module
		{
//			Debug.Log("dernier module");
			//Retourne la facade
//			Vector3 flip = new Vector3(0,180,0);
//			Quaternion flipQuat = new Quaternion(0,0,0,0);
//			flipQuat.eulerAngles = flip;
//			_moduleGameObj.transform.localRotation = flipQuat;
			foreach(Transform t in _moduleGameObj.transform)
			{
				Vector3 pos = t.localPosition;
				pos.z = -pos.z;
				t.localPosition = pos;
//			Debug.Log("pos : "+pos);
			}
			
			prev.SetNextModule(this);
			this.SetPrevModule(prev);
		}
		/*else*/ //prev = null, la facade est le premier module
		if(prev == null)
		{
//			Debug.Log("premier module");
			next.SetPrevModule(this);
			this.SetNextModule(next);
		}
	}
	
	public override void SetPrevModule(DynShelterModule prev)
	{
		_prevModule = prev;
		if(prev == null)
			return;
		((DynShelterModule) prev).SetIsLastEnd(true);
		SetPos(false,GetMyPos(true));
	}
	public override void SetNextModule(DynShelterModule next)
	{
		_nextModule = next;
		if(next == null)
			return;
		((DynShelterModule) next).SetIsFirstEnd(true);
		SetPos(false,GetMyPos(false));
	}
	
	public override void RemoveModule ()
	{
		if(_prevModule != null)
		{
			_prevModule.SetNextModule(null);			
			(/*(DynShelterBloc)*/ _prevModule).SetIsLastEnd(false);
		}
		
		if(_nextModule != null)
		{
			_nextModule.SetPrevModule(null);
			(/*(DynShelterBloc)*/ _nextModule).SetIsFirstEnd(false);
		}

		MonoBehaviour.Destroy(GetGameObj());
	}
	
	public /*private*/ float GetMyPos(bool prev)
	{
	//	Debug.Log("GetMyPos  prev : "+prev);
		if(prev)
		{
//			Debug.Log("GetExtPos  : "+_prevModule.GetExtPos(AR) );
//			Debug.Log("GetIntPos  : "+_prevModule.GetIntPos(AR) );
			return ((_prevModule.GetExtPos(AR) + _prevModule.GetIntPos(AR))/2).z;
//			return (_prevModule.GetExtPos(AR).z);
		}
		else
		{
//			Debug.Log("GetMyPos  : "+((_nextModule.GetExtPos(AV) + _nextModule.GetIntPos(AV))/2).z);
			return ((_nextModule.GetExtPos(AV) + _nextModule.GetIntPos(AV))/2).z;
//			return (_nextModule.GetExtPos(AV).z);
		}
	}
	
	//--vv--FCN's spécifiques--vv--------
	
	private bool Push(bool prev,float delta,DynShelterModule comp)
	{
		if(prev)
		{
			if(Mathf.Sign((comp.GetExtPos(AR)-(GetExtPos(AV) + new Vector3(0,0,delta))).z)<0)
				return true;
			else
				return false;
		}
		else
		{
			if(Mathf.Sign((comp.GetExtPos(AV) - (GetExtPos(AR) + new Vector3(0,0,delta))).z)>0)
				return true;
			else
				return false;
		}
	}
	
	private bool CheckCollisionPossibilityF(float delta,bool checkNext,DynShelterModule cmp)
	{
		if(checkNext)//Checks with nexts
		{
			if(_size >= cmp.GetSize())//Meme taille
			{
				return Contact(delta,AR,true,AV,true,cmp,checkNext);
			}
			else return false;
		}
		else//Checks with prevs
		{
			if(_size >= cmp.GetSize())//Meme taille
			{
				return Contact(delta,AV,true,AR,true,cmp,checkNext);
			}
			else return false;
		}
	}
}
