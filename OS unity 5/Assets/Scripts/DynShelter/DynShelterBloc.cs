using UnityEngine;
using System.Collections;

public class DynShelterBloc : DynShelterModule
{	
	
	private float _AvArSize = 0;
	
	//-------------------------vv--FCNS--vv------------------------------------------------------
	
	public DynShelterBloc(int size,string style,FunctionConf_Dynshelter.ModuleType type,Color32 color,FunctionConf_Dynshelter parent)
		: base (size,style,type,color,parent)
	{
		_isFirstEnd = _isLastEnd = false;
	}

	public override bool MoveModuleV2 (float delta, DynShelterModule startPoint,bool actOnPrevs=true,bool actOnNexts=true)
	{
		//test des limites-------------------------------
		bool notBlockedByLimits = CheckLimitMove(delta);
		if(!notBlockedByLimits)
		{
//			Debug.Log(_tag + "Blocked by limits");
			return false;
		}
		
		//test Ancrage du module
		if(_isAnchored)
		{
			return false;	
		}
		
		//test déplacement telescopique-------------------------------
		bool notBlockedByNext = true;
		bool notBlockedByPrev = true;
		
		if(delta > 0)
		{
			//Test PushPrev
			if(_prevModule != null && actOnPrevs && _prevModule != startPoint && _lockPrev)
				if(CheckTelescopicMove(delta,false))
					notBlockedByPrev = _prevModule.MoveModuleV2(delta,this,true,false);//true,true
			//SI pushPrev > test PullNext
			if(notBlockedByPrev)
				if(_nextModule != null && actOnNexts && _nextModule != startPoint && _lockNext)
					if(CheckTelescopicMove(delta,true))
						notBlockedByNext = _nextModule.MoveModuleV2(delta,this,false,true);//true,true
		}
		else //delta <0
		{
			//Test PushNext
			if(_nextModule != null && actOnNexts && _nextModule != startPoint && _lockNext)
				if(CheckTelescopicMove(delta,true))
					notBlockedByNext = _nextModule.MoveModuleV2(delta,this,false,true);//true,true
			//SI PushNext > test PullPrev
			if(notBlockedByNext)
				if(_prevModule != null && actOnPrevs && _prevModule != startPoint && _lockPrev)
					if(CheckTelescopicMove(delta,false))
						notBlockedByPrev = _prevModule.MoveModuleV2(delta,this,true,false);//true,true
		}
		
		if(!notBlockedByNext || !notBlockedByPrev)
		{
			return false;
		}
		
		//test éventuelles collision-------------------------------
		if(delta>0)	//test with prevs
		{
			DynShelterModule prev;
			if(_prevModule != null)
			{
				if(_prevModule.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
					prev = (DynShelterModule)_prevModule;
				else
					prev = null;
			}
			else prev = null;
			
			while(prev != null)
			{
				if(CheckCollisionPossibility(delta,false,prev))
				{
					if(!prev.MoveModuleV2(delta,this))
						return false;
				}
				if(prev.GetPrevModule() != null)
				{
					if(prev.GetPrevModule().GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
						prev = (DynShelterModule)prev.GetPrevModule();
					else
						prev = null;
				}
				else
					prev = null;
			}
		}
		else 		//test with nexts
		{
			DynShelterModule next;
			if(_nextModule != null)
			{
				if(_nextModule.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
					next = (DynShelterModule)_nextModule;
				else
					next = null;
			}
			else next = null;

			while(next != null)
			{
				if(CheckCollisionPossibility(delta,true,next))
				{
					if(!next.MoveModuleV2(delta,this))
						return false;
				}
				if(next.GetNextModule() != null)
				{
					if(next.GetNextModule().GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
						next = (DynShelterModule) next.GetNextModule();
					else
						next = null;
				}
				else
					next = null;
			}
		}
		
		//IF CAN MOVE -----
		SetPos(true,delta);
		
		if(_isFirstEnd)
		{
			_prevModule.MoveModuleV2(delta,this);
		}
		if(_isLastEnd)
		{
			_nextModule.MoveModuleV2(delta,this);
		}
		
		return true;	
	}
	
	public override void SetGameObject(GameObject g)
	{
//		_moduleGameObj = g;
		base.SetGameObject(g);
		
		//----vv--Calcul des off7s--vv---------------
		Transform mid = _moduleGameObj.transform.FindChild("Mid");
		
		if(!mid.GetComponent<BoxCollider>().enabled)
			mid.GetComponent<BoxCollider>().enabled = true;
		
		Bounds intBds = _moduleGameObj.transform.FindChild("Mid").GetComponent<BoxCollider>().bounds;
		_off7Int = intBds.size.z / 2f; 
		_off7Ext = Mathf.Abs(_moduleGameObj.transform.FindChild("AV").transform.localPosition.z);
		
		_AvArSize = _off7Ext - _off7Int;
		
		_length = _off7Ext*2;
		_width = intBds.size.x;
		_height = intBds.size.y;
		
		foreach(BoxCollider bc in _moduleGameObj.GetComponentsInChildren<BoxCollider>())
		{
			bc.enabled = false;
		}
	}
	
	public override void InsertBetween(DynShelterModule prev, DynShelterModule next)
	{		
		int nxtSize = next!=null? next.GetSize() : -1;
		int prvSize = prev!=null? prev.GetSize() : -1;
		
		float newPos = 0.0f;
		
		if(next != null && prev != null) //Cas d'ajout entre deux modules
		{
			next.SetPrevModule(this);
			prev.SetNextModule(this);
			
			SetNextPrevModule(prev,next);
			
			if(_size == nxtSize && next.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
			{
				//Add collé au next				
				next.MoveModuleV2(-_length/2,next);
				prev.MoveModuleV2(_length/2,prev);
				
				newPos = next.GetPos().z + (_length/2+next.GetLength()/2);
			}
			if(_size == prvSize && prev.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
			{
				//Add collé au prev
				if(_size != nxtSize)
				{
					next.MoveModuleV2(-_length/2,next);
					prev.MoveModuleV2(_length/2,prev);
				}
				
				newPos = prev.GetPos().z - (_length/2+prev.GetLength()/2);
			}
			if(_size != nxtSize && _size != prvSize)
			{
				//add a cheval
				float space = Vector3.Distance(prev.GetPos(),next.GetPos());
				if(space > _length)
				{
					newPos = ((prev.GetPos() + next.GetPos()).z)/2;	
				}
				else
				{
					float off7 = (_length - space)/2;
					next.MoveModuleV2(-off7,next);
					prev.MoveModuleV2(off7,prev);
					
					newPos = (prev.GetPos().z + next.GetPos().z)/2;		
				}				
			}
		}
		else //(next == null || prev == null) // Cas d'ajout aux extremites (1er ou dernier)
		{
			if(next !=null)
			{
				next.SetPrevModule(this);
				SetNextModule(next);
				
				if(_size == nxtSize)
				{
					//Add collé au next
					newPos = next.GetPos().z + (_length/2 + next.GetLength()/2);
				}
				else
				{
					//Add collé au next ou a cheval si pas la place
//					newPos = next.GetExtPos(AV).z;
					newPos = /*next.GetPos().z + */next.GetIntPos(AV).z - GetIntPos(AR).z;
				}
			}
			else//(prev != null)
			{
				prev.SetNextModule(this);
				SetPrevModule(prev);
				
				if(_size == prvSize)
				{
					//Add collé au prev
					newPos = prev.GetPos().z - (_length/2 + prev.GetLength()/2);
				}
				else
				{
					//Add collé au prev ou a cheval si pas la place
//					newPos = prev.GetExtPos(AR).z;
					newPos = /*prev.GetPos().z + */prev.GetIntPos(AR).z - GetIntPos(AV).z;
				}
			}
		}
		
		//Applying
		SetPos(false,newPos);
		
	}
	
	//--vv--FCN's spécifiques--vv--------	
	//Configs
	public ArrayList GetBlocConfig()
	{
		ArrayList conf = new ArrayList();
		conf.Add(_off7Int);
		conf.Add(_off7Ext);
		conf.Add(_length);
		conf.Add(_width);
		conf.Add(_height);
		return conf;
	}
	public void SetBlocConfig(ArrayList conf)
	{
		_off7Int = (float) conf[0];
		_off7Ext = (float) conf[1];
		_length = (float) conf[2];
		_width = (float) conf[3];
		_height = (float) conf[4];
	}
	
	//Accessors/mutators
//	public bool IsFirstEnd(){return _isFirstEnd;}
//	public bool IsLastEnd(){return _isLastEnd;}
//	
//	public void SetIsFirstEnd(bool val){_isFirstEnd = val;}
//	public void SetIsLastEnd(bool val){_isLastEnd = val;}
	
	//Déplacement tests
	override public bool CheckTelescopicMove(float delta,bool checkNext)
	{
		bool pushNext = false;
		bool pullNext = false;
		bool pushPrev = false;
		bool pullPrev = false;
		
		if(delta <0)//---------------------------------------------------------------------dep Z-
		{
			if(_nextModule != null && _lockNext && checkNext)
			{
				if(_nextModule.GetSize()!=GetSize())//---Si c'est un bloc
				{
					if(Push(false,delta))
					{
						pushNext = true;	
					}
				}
				else//-------------------------------------------------------------------------Si c'est un bloc de mm taille
				{
					pushNext = true;
				}
			}
			
			if(_prevModule != null && _lockPrev && !checkNext)
			{
				if(_prevModule.GetSize()!=GetSize())//---Si c'est un bloc
				{
					if(Pull(true,delta))
					{
						pullPrev = true;
					}
				}
				else//-------------------------------------------------------------------------Si c'est une facade ou une extremité ou un bloc de mm taille
				{
					pullPrev = true;
					if(Contact(delta,AV,true,AR,true,_prevModule,false))
						pullPrev = false;
				}
			}
		}
		else if(delta >0)//-----------------------------------------------------------------------------dep Z+
		{
			if(_nextModule != null && _lockNext && checkNext)
			{
				if(_nextModule.GetSize()!=GetSize())//---Si c'est un bloc
				{
					if(Pull(false,delta))
					{
						pullNext = true;	
					}
				}
				else//-------------------------------------------------------------------------Si c'est une facade ou une extremité ou un bloc de mm taille
				{
					pullNext = true;
					if(Contact(delta,AR,true,AV,true,_nextModule,true))
						pullNext = false;
				}
			}
			
			if(_prevModule != null && _lockPrev && !checkNext)
			{
				if(_prevModule.GetSize()!=GetSize())//---Si c'est un bloc
				{
					if(Push(true,delta))
					{
						pushPrev = true;
					}
				}
				else//-------------------------------------------------------------------------Si c'est un bloc de mm taille
				{
						pushPrev = true;
				}
			}
		}
	
		if(checkNext)
		{
			if((pushNext||pullNext))
				return true;
			else
				return false;
		}
		else
		{
			if((pushPrev||pullPrev))
				return true;
			else
				return false;
		}
	}
	private bool CheckCollisionPossibility(float delta,bool checkNext,DynShelterModule cmp)
	{
		if(checkNext)//Checks with nexts
		{
			if(cmp == _nextModule && _lockNext)
				return false;
			if(_size == cmp.GetSize())//Meme taille
			{
				return Contact(delta,AR,true,AV,true,cmp,checkNext);
			}
			else if(cmp.GetSize() >= _size && cmp.IsLastEnd() )
			{
				return Contact(delta,AR,true,AR,false,cmp,checkNext);
			}
			else
				return false;
		}
		else//Checks with prevs
		{
			if(cmp == _prevModule && _lockPrev)
				return false;
			if(_size == cmp.GetSize() /*|| (cmp.GetModuleType() != FunctionConf_Dynshelter.ModuleType.bloc && _size <= cmp.GetSize())*/)//Meme taille
			{
				return Contact(delta,AV,true,AR,true,cmp,checkNext);
			}
			else if(cmp.GetSize() >= _size && cmp.IsFirstEnd())
			{
				return Contact(delta,AV,true,AV,false,cmp,checkNext);
			}
			else//--------------------//pas meme taille
			{
				return false;
			}
		}
	}
	
	private bool Push(bool prev,float delta)
	{
		if(prev)
		{
			if(Mathf.Sign((_prevModule.GetExtPos(AR) - (GetIntPos(AR)+new Vector3(0,0,delta))).z) < 0 )
				return true;
			else
				return false;
		}
		else //next
		{
			if(Mathf.Sign((_nextModule.GetExtPos(AV) - (GetIntPos(AV)+new Vector3(0,0,delta))).z) > 0 )
				return true;
			else
				return false;
		}
	}
	private bool Pull(bool prev,float delta)
	{
		if(prev)
		{
			if(Mathf.Sign((GetIntPos(AV)+new Vector3(0,0,delta)-_prevModule.GetIntPos(AR)).z) < 0 )
				return true;
			else
				return false;
		}
		else //next
		{
			if(Mathf.Sign(((GetIntPos(AR)+new Vector3(0,0,delta)) - _nextModule.GetIntPos(AV)).z) > 0 )
				return true;
			else
				return false;
		}
	}

}
