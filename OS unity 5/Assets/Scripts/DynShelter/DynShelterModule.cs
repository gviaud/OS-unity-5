using UnityEngine;
using System.Collections;

public class DynShelterModule
{

#region public vars
 	public string midPartName;
	public string fwdPartName;

	public bool bextrem = false;
#endregion

#region private vars
	protected int _size;
	
	protected float _length,_width,_height;
	protected float _off7Int;
	protected float _off7Ext;
	
	protected string _style;
	protected string _tag;
	
	protected bool _canMoveNext; //false si physiquement bloqué
	protected bool _canMovePrev; //false si physiquement bloqué
	protected bool _isFirstEnd,_isLastEnd;
	protected bool _isAnchored;
	
	protected FunctionConf_Dynshelter _parentConf;
	
	protected FunctionConf_Dynshelter.ModuleType _type;
	
	protected DynShelterModule _nextModule, _prevModule;
	
	protected Color32 _color;
	
	protected bool _lockNext, _lockPrev;
	
	protected GameObject _moduleGameObj;
	
	
	protected const bool AV = true;
	protected const bool AR = false;
	protected const float Epsilon = 0.02f;
	
	protected ArrayList _backUpMaterials;
	protected Renderer[] _renderers;
	protected Material _transparent;
#endregion
	
#region Fcns
	
	//CONSTRUCTOR---
	
	public DynShelterModule(int size,string style,FunctionConf_Dynshelter.ModuleType type,Color32 color,FunctionConf_Dynshelter parent)
	{
		_size = size;
		_style = style;
		_type = type;
		_color = color;
		_parentConf = parent;
		
		//--------------------------------------------
		
		_tag = _type.ToString()+"_t"+_size+"_"+_style;
		_canMoveNext = true;
		_canMovePrev = true;
		_lockNext = true;
		_lockPrev = true;
		_isAnchored = false;
		_nextModule = null;
		_prevModule = null;
		
		_transparent = new Material(Shader.Find("Transparent/Diffuse"));
		_transparent.color = new Color(1,1,1,0.25f);
		_backUpMaterials = new ArrayList();
	}
	
	//--vv--CONFIG FCNS--vv--
	public void ChangeStyle(string newStyle,GameObject newGo)
	{
		_style = newStyle;
		_tag = _type.ToString()+"_t"+_size+"_"+_style;
//		_moduleGameObj = newGo;
		SetGameObject(newGo);
	}
	
	public virtual bool MoveModuleV2(float delta, DynShelterModule startPoint,bool actOnPrevs=true,bool actOnNexts=true)
	{
		return true;
	}
	
	public virtual bool CheckTelescopicMove(float delta,bool checkNext)
	{
		return true;
	}
	
	public virtual void InsertBetween(DynShelterModule prev, DynShelterModule next)
	{
		//A implémenter dans les classes filles
	}
	
	public virtual void RemoveModule()
	{
		//A Overrider dans les classes filles si besoin
		
		if(_prevModule != null)
		{
			_prevModule.SetNextModule(_nextModule);
			if(_nextModule != null)
			{
				Vector3 prevDist = GetPos() - _prevModule.GetPos();
				prevDist = prevDist/2;
				_prevModule.MoveModuleV2(prevDist.z,_prevModule);
			}
		}
		else
			if(_nextModule != null)
				_nextModule.SetPrevModule(null);
		
		if(_nextModule != null)
		{
			_nextModule.SetPrevModule(_prevModule);
			if(_prevModule != null)
			{
				Vector3 nextDist = GetPos() - _nextModule.GetPos();
				nextDist = nextDist/2;
				_nextModule.MoveModuleV2(nextDist.z,_nextModule);
			}
		}
		else
			if(_prevModule != null)
				_prevModule.SetNextModule(null);

		MonoBehaviour.Destroy(GetGameObj());
	}
	
	//Accessors/Mutators---
	//--Get's---
	public Vector3 GetPos()
	{
		return _moduleGameObj.transform.localPosition;
	}
	
	public Vector3 GetWorldPos()
	{
		return _moduleGameObj.transform.position;
	}
	
	public GameObject GetGameObj(){return _moduleGameObj;}
	
	public int 		GetSize(){return _size;}
	public float 	GetLength(){return _length;}
	public float 	GetWidth(){return _width;}
	public float 	GetHeight(){return _height;}
	public string 	GetStyle(){return _style;}
	public string 	GetTag(){return _tag;}
	public Color32	GetColor(){return _color;}
	public FunctionConf_Dynshelter GetParent(){return _parentConf;}
	
	public virtual Vector3 	GetIntPos(bool forward)
	{
		if(forward)
			return _moduleGameObj.transform.localPosition + /*_moduleGameObj.transform.forward*_off7Int*/new Vector3(0,0,_off7Int);
		else
			return _moduleGameObj.transform.localPosition - /*_moduleGameObj.transform.forward*_off7Int*/new Vector3(0,0,_off7Int);
	}
	public Vector3 	GetIntPosNoOverride(bool forward)
	{
		if(forward)
			return _moduleGameObj.transform.localPosition + new Vector3(0,0,_off7Int);
		else
			return _moduleGameObj.transform.localPosition - new Vector3(0,0,_off7Int);
	}
	
	public virtual Vector3 	GetExtPos(bool forward)
	{
	//	Debug.Log("_moduleGameObj.transform.localPosition "+_moduleGameObj.transform.localPosition+",_off7Ext : "+_off7Ext); 
		if(forward)
			return _moduleGameObj.transform.localPosition + /*_moduleGameObj.transform.forward*_off7Ext*/ new Vector3(0,0,_off7Ext);
		else
			return _moduleGameObj.transform.localPosition - /*_moduleGameObj.transform.forward*_off7Ext*/ new Vector3(0,0,_off7Ext);
	}
	public Vector3 	GetExtPosNoOverride(bool forward)
	{
		if(_moduleGameObj==null)
			return new Vector3(0,0,_off7Ext);
		if(forward)
			return _moduleGameObj.transform.localPosition + new Vector3(0,0,_off7Ext);
		else
			return _moduleGameObj.transform.localPosition - new Vector3(0,0,_off7Ext);
	}
	
	public bool 	GetPrevLock(){return _lockPrev;}
	public bool 	GetNextLock(){return _lockNext;}
	
	public DynShelterModule GetPrevModule(){return _prevModule;}
	public DynShelterModule GetNextModule(){return _nextModule;}
	
	public FunctionConf_Dynshelter.ModuleType GetModuleType(){return _type;}
	
	public float GetIntOffSet()
	{
		return _off7Int;
	}
	
	public bool IsAnchored(){return _isAnchored;}
	
	//--Set's---
	public void SetPos(bool isOffset,float val)
	{
	//	Debug.Log ("isOffset : "+isOffset+", val : "+val);
		Vector3 pos = _moduleGameObj.transform.localPosition;
		if(isOffset)
			pos.z += val;
		else
			pos.z = val;
		
		_moduleGameObj.transform.localPosition = pos;
	}
	public virtual void SetGameObject(GameObject g)
	{
		_moduleGameObj = g;
		_renderers = _moduleGameObj.GetComponentsInChildren<Renderer>(true);
		_backUpMaterials.Clear();
		foreach(Renderer r in _renderers)
		{
			foreach(Material m in r.materials)
			{
				_backUpMaterials.Add(m);	
			}
		}
	}
	
	public void SetAnchor(bool b){_isAnchored = b;}
	
	
	//Next/Prev Module
	public virtual void SetPrevModule(DynShelterModule prev)
	{
		_prevModule = prev;
	}
	public virtual void SetNextModule(DynShelterModule next)
	{
		_nextModule = next;
	}
	public void SetNextPrevModule(DynShelterModule prev,DynShelterModule next)
	{
		_prevModule = prev;
		_nextModule = next;
	}
	
	//Next/Prev Locks
	public void SetNextPrevLocks(bool prev, bool next)
	{
		_lockPrev = prev;
		_lockNext = next;
	}
	public void SetPrevLocks(bool prev)
	{
		_lockPrev = prev;
	}
	public void SetNextLocks(bool next)
	{
		_lockNext = next;
	}
	
	public void	SetColor(Color32 c){_color = c;}
	
	public virtual bool IsFirstEnd(){return _isFirstEnd;}
	public virtual bool IsLastEnd(){return _isLastEnd;}
	
	public virtual void SetIsFirstEnd(bool val){_isFirstEnd = val;}
	public virtual void SetIsLastEnd(bool val){_isLastEnd = val;}
	
	//-----vv--UTILS FCNS--vv--------------------------------------------------
	
	//Créé un vecteur "test" entre avant/arriere intérieur/exterieur entre le current et une valeur de position (limites virtuelles)
	//renvoie true si avec le déplacement delta il y a contact
	protected bool ContactWthVal(float delta,bool curAvt,bool curExt, Vector3 pos,bool cmpIsNext)
	{
		Vector3 toTest;
		
		toTest = pos;
		
		if(curExt)
		{
			toTest = toTest - (GetExtPos(curAvt) + new Vector3(0,0,delta));
		}
		else//int
		{
			toTest = toTest - (GetIntPos(curAvt) + new Vector3(0,0,delta));
		}
				
		if(cmpIsNext)
		{
			if(toTest.z >= 0)
				return true;
			else
				return false;
		}
		else
		{
			if(toTest.z <= 0)
				return true;
			else
				return false;
		}
		
	}
	
	//Créé un vecteur "test" entre avant/arriere intérieur/exterieur entre le current et le cmp(comparé) 
	//renvoie true si avec le déplacement delta il y a contact
	protected bool Contact(float delta,bool curAvt,bool curExt, bool cmpAvt,bool cmpExt,DynShelterModule cmp,bool cmpIsNext)
	{
		Vector3 toTest;
		if(cmpExt)
		{
			toTest = cmp.GetExtPos(cmpAvt);
		}
		else//int
		{
			toTest = cmp.GetIntPos(cmpAvt);
		}
		
		if(curExt)
		{
			toTest = toTest - (GetExtPos(curAvt)+ new Vector3(0,0,delta));
		}
		else//int
		{
			toTest = toTest - (GetIntPos(curAvt)+ new Vector3(0,0,delta));
		}
		
		if(cmpIsNext)
		{
			if(toTest.z >= 0) //>=
				return true;
			else
				return false;
		}
		else
		{
			if(toTest.z <= 0)//<=
				return true;
			else
				return false;
		}
		
	}
	
	//Test si le module courant déplacé de delta arrive au niveau d'une limite virtuelle
	protected bool CheckLimitMove(float delta)
	{
		if(delta < 0)
		{
			return !ContactWthVal(delta,AR,true,_parentConf.GetLimitPos(AR),true);	
		}
		else
		{
			return !ContactWthVal(delta,AV,true,_parentConf.GetLimitPos(AV),false);
		}
	}
	
	public void SetTransparent(bool transp)
	{
		int index = 0;
		if(_backUpMaterials.Count > 0)
		{
			foreach(Renderer r in _renderers)
			{
				Material[] mts = r.materials;
				for(int i=0;i<mts.Length;i++)
				{
					if(transp)
					{
						mts[i] = _transparent;
					}
					else
					{
						mts[i] =  (Material)_backUpMaterials[index];
						index ++;
					}
				}
				r.materials = mts;
			}
		}		
	}
	
#endregion
	
//	public DynShelterModule(int size,string style,FunctionConf_Dynshelter.ModuleType typ,Color32 color,
//		DynShelterModule next,DynShelterModule prev,bool lockNxt,bool lockPrv,GameObject go,FunctionConf_Dynshelter parent)
//	{
//		_size = size;
//		_style = style;
//		_type = typ;
//		_nextModule = next;
//		_prevModule = prev;
//		_color = color;
//		_lockNext = lockNxt;
//		_lockPrev = lockPrv;
//		_moduleGameObj = go;
//		_parentConf = parent;
//		
//		_tag = _type.ToString()+"_t"+_size+"_"+_style;
//		_canMoveNext = true;
//		_canMovePrev = true;
//		
//		//------vv--a Spécifier dans les classes filles au setGameObject--vv----
//		if(_moduleGameObj!= null && typ == FunctionConf_Dynshelter.ModuleType.bloc)
//		{
//			if(!_moduleGameObj.transform.FindChild("Mid").GetComponent<BoxCollider>().enabled)
//				_moduleGameObj.transform.FindChild("Mid").GetComponent<BoxCollider>().enabled = true;
//			
//			Bounds intBds = _moduleGameObj.transform.FindChild("Mid").GetComponent<BoxCollider>().bounds;
//			_off7Int = intBds.size.z / 2f; 
//			_off7Ext = _moduleGameObj.transform.FindChild("AV").transform.localPosition.z;
//			_length = _off7Ext*2;
//			_width = intBds.size.x;
//			_height = intBds.size.y;
//			
//			foreach(BoxCollider bc in _moduleGameObj.GetComponentsInChildren<BoxCollider>())
//			{
//				bc.enabled = false;
//			}	
//		}
//		
//	}
	
//	public bool MoveModule(float delta,bool actOnPrevs,bool actOnNexts)
//	{
//		if(_type == FunctionConf_Dynshelter.ModuleType.facade)
//		{
//			if(_prevModule!=null)
//				_prevModule.MoveModule(delta,true,true);
//			if(_nextModule!= null)
//				_nextModule.MoveModule(delta,true,true);
//			return true;
//		}
//		
//		bool pushNext = false;
//		bool pullNext = false;
//		bool pushPrev = false;
//		bool pullPrev = false;
////		_canMoveNext = true;
////		_canMovePrev = true;
//		
//		//Déplacement limites 
//		if(delta < 0)
//		{
//			if(Vector3.Distance(GetExtPos(AR),_parentConf.GetLimitPos(AR))<Epsilon)
//			{
//				_canMoveNext = false;
//			}
//			else
//				_canMoveNext = true;
//		}
//		else
//		{
//			if(Vector3.Distance(GetExtPos(AV),_parentConf.GetLimitPos(AV))<Epsilon)
//			{
//				_canMovePrev = false;
//			}
//			else
//				_canMovePrev = true;
//		}
//		
//		//Déplacement Télescopique
//		if(delta <0 && _canMoveNext)//---------------------------------------------------------------------dep Z-
//		{
//			if(_nextModule != null && actOnNexts)
//			{
//				if(_nextModule.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc && _nextModule.GetSize()!=GetSize())//---Si c'est un bloc
//				{
//					if(Vector3.Distance(GetExtPos(AR),_nextModule.GetIntPos(AR))<Epsilon)
//					{
//						pushNext = true;	
//					}
//				}
//				else//-------------------------------------------------------------------------Si c'est une facade ou une extremité ou un bloc de mm taille
//				{
//					pushNext = true;
//				}
//			}
//			
//			if(_prevModule != null && actOnPrevs)
//			{
//				if(_prevModule.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc && _prevModule.GetSize()!=GetSize())//---Si c'est un bloc
//				{
//					if(Vector3.Distance(GetIntPos(AV),_prevModule.GetIntPos(AR))<Epsilon)
//					{
//						pullPrev = true;
//					}
//				}
//				else//-------------------------------------------------------------------------Si c'est une facade ou une extremité ou un bloc de mm taille
//				{
//						pullPrev = true;
//				}
//			}
//		}
//		else if(delta >0 && _canMovePrev)//-----------------------------------------------------------------------------dep Z+
//		{
//			if(_nextModule != null && actOnNexts)
//			{
//				if(_nextModule.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc && _nextModule.GetSize()!=GetSize())//---Si c'est un bloc
//				{
//					if(Vector3.Distance(_nextModule.GetIntPos(AV),GetIntPos(AR))<Epsilon)
//					{
//						pullNext = true;	
//					}
//				}
//				else//-------------------------------------------------------------------------Si c'est une facade ou une extremité ou un bloc de mm taille
//				{
//						pullNext = true;
//				}
//			}
//			
//			if(_prevModule != null && actOnPrevs)
//			{
//				if(_prevModule.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc && _prevModule.GetSize()!=GetSize())//---Si c'est un bloc
//				{
//					if(Vector3.Distance(_prevModule.GetIntPos(AV),GetExtPos(AV))<Epsilon)
//					{
//						pushPrev = true;
//					}
//				}
//				else//-------------------------------------------------------------------------Si c'est une facade ou une extremité ou un bloc de mm taille
//				{
//						pushPrev = true;
//				}
//			}
//		}
//		
//		if((pushNext||pullNext) && _lockNext)
//		{
//			if(_nextModule.GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade)
//				_nextModule.SetPos(false,GetExtPos(AR).z+delta);
//			else
//				_canMoveNext = _nextModule.MoveModule(delta,false,true);
//		}
//		if((pushPrev||pullPrev) && _lockPrev)
//		{
//			if(_prevModule.GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade)
//				_prevModule.SetPos(false,GetExtPos(AV).z+delta);
//			else
//				_canMovePrev = _prevModule.MoveModule(delta,true,false);
//		}
//		
//		//Déplacement Collision
//		if(_type == FunctionConf_Dynshelter.ModuleType.bloc)
//		{
//			if(_nextSameSize != null && _nextSameSize != _nextModule && delta < 0 && _nextSameSize.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
//			{
//				if(Vector3.Distance(_nextSameSize.GetExtPos(AV),GetExtPos(AR))<Epsilon)
//				{
//					_canMoveNext &= _nextSameSize.MoveModule(delta,true,true);
//				}
//			}
//			if(_prevSameSize != null && _prevSameSize != _prevModule && delta > 0 && _prevSameSize.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
//			{
//				if(Vector3.Distance(_prevSameSize.GetExtPos(AR),GetExtPos(AV))<Epsilon)
//				{
//					_canMovePrev &= _prevSameSize.MoveModule(delta,true,true);
//				}
//			}
//		}
//		
//		//--------------------------
//		if(_canMoveNext && delta < 0)
//			SetPos(true,delta);
//		else if(_canMovePrev && delta > 0)
//			SetPos(true,delta);
//		
//		if(delta < 0)
//			return _canMoveNext;
//		else //(delta > 0)
//			return _canMovePrev;
//	} 
// << A Overrider dans dynshelterbloc/facade/extrem...
}