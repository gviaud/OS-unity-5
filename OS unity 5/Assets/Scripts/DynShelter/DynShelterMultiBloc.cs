using UnityEngine;
using System.Collections;

public class DynShelterMultiBloc : DynShelterModule
{
	
	private DynShelterModule _first = null;
	private DynShelterModule _last = null;
	
	private GameObject ARSubObj;
	
	private float _AvArSize = 0;
	
//	private bool _isFirstEnd,_isLastEnd;
	
	//-------------------------vv--CONSTRUCTORS--vv----------------------------------------------
	
	public DynShelterMultiBloc(int size,string style,FunctionConf_Dynshelter.ModuleType type,Color32 color,FunctionConf_Dynshelter parent)
		: base (size,style,type,color,parent)
	{
		_isFirstEnd = _isLastEnd = false;
	}
	
	public DynShelterMultiBloc(DynShelterBloc baseBloc)
		: base (baseBloc.GetSize(),baseBloc.GetStyle(),baseBloc.GetModuleType(),baseBloc.GetColor(),baseBloc.GetParent())
	{
//		_moduleGameObj = baseBloc.GetGameObj();
		_parentConf.SaveTransform();
		SetGameObject(baseBloc.GetGameObj());
		_parentConf.ReapplyTransform();
		
		SetBlocConfig(baseBloc.GetBlocConfig());
		
		_prevModule = baseBloc.GetPrevModule();
		_nextModule = baseBloc.GetNextModule();
		
		WhoIs(true);
		WhoIs(false);
		
		_lockNext = baseBloc.GetNextLock();
		_lockPrev = baseBloc.GetPrevLock();
		_isFirstEnd = _isLastEnd = false;
		
		
	}
	
	//-------------------------vv--FCNS--vv------------------------------------------------------
	
	private bool MoveMulti(float delta, DynShelterModule startPoint,bool actOnPrevs=true,bool actOnNexts=true)
	{
		_canMoveNext = true;
		_canMovePrev = true;
		
		DynShelterModule multiNext = _last.GetNextModule(); //multiNext equivalent du _nextModule
		DynShelterModule multiPrev = _first.GetPrevModule();//multiPrev equivalent du _prevModule
		
		//test des limites-------------------------------
		bool notBlockedByLimits = CheckLimitMove(delta);
		
		if( (!notBlockedByLimits) && !_parentConf.IsAbrifixe())
		{
			Debug.Log(_tag + "Blocked by limits");
			return false;
		}
		
		//test Ancrage du module
		bool test = true;
		DynShelterModule testModule = _first;
		while(testModule != _last && test)
		{
			if(testModule.IsAnchored())
			{
				test = false;
			}
			else
			{
				testModule = testModule.GetNextModule();	
			}
		}		
		if(!test)
		{
			Debug.Log("Anchored");
			return false;
		}
		
		//test déplacement telescopique-------------------------------
		bool notBlockedByNext = true;
		bool notBlockedByPrev = true;
		
		if(delta > 0)
		{
			//Test PushPrev 
			if(multiPrev != null && actOnPrevs && multiPrev != startPoint && _lockPrev)
				if(CheckTelescopicMove(delta,false,multiNext,multiPrev))
					notBlockedByPrev = multiPrev.MoveModuleV2(delta,this,true,false);//true,true
			//SI pushPrev > test PullNext
			if(notBlockedByPrev)
				if(multiNext != null && actOnNexts && multiNext != startPoint && _lockNext)
					if(CheckTelescopicMove(delta,true,multiNext,multiPrev))
						notBlockedByNext = multiNext.MoveModuleV2(delta,this,false,true);//true,true
		}
		else //delta <0
		{
			//Test PushNext
			if(multiNext != null && actOnNexts && multiNext != startPoint && _lockNext)
				if(CheckTelescopicMove(delta,true,multiNext,multiPrev))
					notBlockedByNext = multiNext.MoveModuleV2(delta,this,false,true);//true,true
			//SI PushNext > test PullPrev
			if(notBlockedByNext)
				if(multiPrev != null && actOnPrevs && multiPrev != startPoint && _lockPrev)
					if(CheckTelescopicMove(delta,false,multiNext,multiPrev))
						notBlockedByPrev = multiPrev.MoveModuleV2(delta,this,true,false);//true,true
		}
		
		if(!notBlockedByNext || !notBlockedByPrev)
		{
			Debug.Log("Blockedbynextprev");
			return false;
		}
		
		//test éventuelles collision-------------------------------
		if(delta>0)	//test with prevs
		{
			DynShelterModule prev;
			if(multiPrev != null)
			{
				if(multiPrev.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
					prev = (DynShelterModule)multiPrev;
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
			if(multiNext != null)
			{
				if(multiNext.GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc)
					next = (DynShelterModule)multiNext;
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
			multiPrev.MoveModuleV2(delta,this);
		}
		/*else
		{
			if(GetPrevModule()!=null)
				if(GetPrevModule().GetType()==typeof(DynShelterFacade))
					multiPrev.MoveModuleV2(delta,this);
		}*/
		if(_isLastEnd) 
		{
			multiNext.MoveModuleV2(delta,this);
		}
		/*else
		{
			if(GetNextModule()!=null)
				if(GetNextModule().GetType()==typeof(DynShelterFacade))
					multiPrev.MoveModuleV2(delta,this);
		}*/
		
		return true;
		
	}
	
	public override bool MoveModuleV2(float delta, DynShelterModule startPoint,bool actOnPrevs=true,bool actOnNexts=true)
	{
		bool outPut = true;
		
		outPut = MoveMulti(delta,this,actOnPrevs,actOnNexts); // Test déplacement de tout l'ensemble
		
		if(_nextModule != null && outPut)
		{
			if(_nextModule.GetType() == GetType() && _nextModule.GetSize() == GetSize()) // propagation du mouvement aux nexts
			{
				((DynShelterMultiBloc) _nextModule).MoveModuleLocal(delta,false,false,true);
			}
		}
		
		if(_prevModule != null && outPut)
		{
			if(_prevModule.GetType() == GetType() && _prevModule.GetSize() == GetSize()) // propagation du mouvement aux prevs
			{
				((DynShelterMultiBloc) _prevModule).MoveModuleLocal(delta,false,true,false);
			}
		}
		
		return outPut;
	}
	
	public void MoveModuleLocal(float delta,bool isSelected,bool actOnPrevs,bool actOnNexts)
	{
		SetPos(true,delta);
		if(_nextModule != null)
		{
			if(actOnNexts && _nextModule.GetType() == GetType() && _nextModule.GetSize() == GetSize()) // propagation du mouvement aux nexts
			{
				((DynShelterMultiBloc) _nextModule).MoveModuleLocal(delta,false,false,true);
			}
		}
		
		if(_prevModule != null)
		{
			if(actOnPrevs && _prevModule.GetType() == GetType() && _prevModule.GetSize() == GetSize()) // propagation du mouvement aux prevs
			{
				((DynShelterMultiBloc) _prevModule).MoveModuleLocal(delta,false,true,false);
			}
		}
	}
	
	public override void SetGameObject(GameObject g)
	{
//		_moduleGameObj = g;
		base.SetGameObject(g);
		
		//----vv--Calcul des off7s--vv---------------
		
		if(!_moduleGameObj.transform.FindChild("Mid").GetComponent<BoxCollider>().enabled)
			_moduleGameObj.transform.FindChild("Mid").GetComponent<BoxCollider>().enabled = true;
		
		ARSubObj = _moduleGameObj.transform.FindChild("AR").gameObject;
		Bounds intBds = _moduleGameObj.transform.FindChild("Mid").GetComponent<BoxCollider>().bounds;
		_off7Int = intBds.size.z / 2f; 
		_off7Ext = Mathf.Abs(ARSubObj.transform.localPosition.z);
		//_off7Ext = Mathf.Abs(_moduleGameObj.transform.FindChild("AV").transform.localPosition.z);
		
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
		float off7 = 0.0f;
		if(next != null && prev != null) //Cas d'ajout entre deux modules
		{
			//--vv--configuration des modules--vv--
			if(next.GetModuleType() != FunctionConf_Dynshelter.ModuleType.bloc)
			{
				_last = this;
			}
			next.SetPrevModule(this);
			
			if(prev.GetModuleType() != FunctionConf_Dynshelter.ModuleType.bloc)
			{
				_first = this;
			}
			prev.SetNextModule(this);
				
//			SetNextPrevModule(prev,next);
			SetPrevModule(prev);
			SetNextModule(next);
			
			WhoIs(true);
			WhoIs(false);

			//--vv--Placement--vv--
			if(_size == nxtSize && (_size != prvSize || prev.GetModuleType() != FunctionConf_Dynshelter.ModuleType.bloc)) //Le suivant est de la mm taille mais pas le précedent
			{
				//Add collé au next				
				newPos = next.GetPos().z;
				off7 = -_length;
				if(next.GetType() == GetType())
					off7 += _AvArSize;//TAILLE DU MONTANT
				
				DynShelterModule nxt = next;
				while(nxt != null)
				{
					nxt.SetPos(true,off7);
					nxt = nxt.GetNextModule();
				}
			}
			else if(_size == prvSize && (_size != nxtSize || next.GetModuleType() != FunctionConf_Dynshelter.ModuleType.bloc)) //le précedent est de la meme taille mais pas le suivant
			{
				//Add collé au prev
				newPos = prev.GetPos().z;
				off7 = _length;
				if(prev.GetType() == GetType())
					off7 -= ((DynShelterMultiBloc)prev).GetAvArSize();//TAILLE DU MONTANT
				
				DynShelterModule prv = prev;
				while(prv != null)
				{
					prv.SetPos(true,off7);
					prv = prv.GetPrevModule();
				}
			}
			else //if(_size == nxtSize && _size == prvSize) // le precedent et le suivant ont la meme taille que le courant
			{
				DynShelterModule nxt = next;
				off7 = -_length;
				if(next.GetType() == GetType())
					off7 += _AvArSize;//TAILLE DU MONTANT
				while(nxt != null)
				{
					nxt.SetPos(true,off7/2);
					nxt = nxt.GetNextModule();
				}
				
				off7 = _length;
				if(prev.GetType() == GetType())
					off7 -= ((DynShelterMultiBloc)prev).GetAvArSize();//TAILLE DU MONTANT
				DynShelterModule prv = prev;
				while(prv != null)
				{
					prv.SetPos(true,off7/2);
					prv = prv.GetPrevModule();
				}
				
				newPos = (next.GetPos()+prev.GetPos()).z/2;
			}
//			else // le precedent et le suivant ont une taille différente du courant
//			{
//				DynShelterModule nxt = next;
//				while(nxt != null)
//				{
//					nxt.SetPos(true,-_length/2);
//					nxt = nxt.GetNextModule();
//				}
//				
//				DynShelterModule prv = prev;
//				while(prv != null)
//				{
//					prv.SetPos(true,_length/2);
//					prv = prv.GetPrevModule();
//				}
//				
//				newPos = (next.GetPos()+prev.GetPos()).z/2;
//			}
			
			
		}
		else //(next == null || prev == null) // Cas d'ajout aux extremites (1er ou dernier)
		{
			if(next !=null) // this est le 1er module
			{
				next.SetPrevModule(this);
				SetNextModule(next);
				
				WhoIs(true);
				WhoIs(false);	
				
				if(_size == nxtSize)
				{
					//Add collé au next
					newPos = next.GetPos().z + (_length/2 + next.GetIntOffSet()/*next.GetLength()/2*/);
				}
				else
				{
					//Add collé au next ou a cheval si pas la place
					newPos = next.GetExtPos(AV).z;
				}
			}
			else//(prev != null) // this est le dernier module
			{
				prev.SetNextModule(this);
				SetPrevModule(prev);
				
				WhoIs(true);
				WhoIs(false);
				
				if(_size == prvSize)
				{
					//Add collé au prev
					newPos = prev.GetPos().z - (_length/2 + prev.GetIntOffSet() /*prev.GetLength()/2*/);
				}
				else
				{
					//Add collé au prev ou a cheval si pas la place
					newPos = prev.GetExtPos(AR).z;
				}
			}
		}
		
		//Applying Position
		SetPos(false,newPos);
		
	}
	
	public override Vector3 GetIntPos(bool forward)
	{
		if(forward)
		{	
			if(_first!=null)
				return _first.GetIntPosNoOverride(forward);
			else
				return new Vector3(0.0f,0.0f,0.0f);
		}
		else
		{
			if(_last!=null)
				return _last.GetIntPosNoOverride(forward);
			else
				return new Vector3(0.0f,0.0f,0.0f);				
		}
	}
	public override Vector3 GetExtPos(bool forward)
	{
		if(forward)
		{	
			if(_first!=null)
				return _first.GetExtPosNoOverride(forward);
			else
				return new Vector3(0.0f,0.0f,0.0f);
		}
		else
		{
			if(_last!=null)
				return _last.GetExtPosNoOverride(forward);
			else
				return new Vector3(0.0f,0.0f,0.0f);				
		}
	}
	
	public override void SetNextModule(DynShelterModule next)
	{
		if(next != null && next.GetType() == GetType() && _size == next.GetSize())
		{
			ARSubObj.SetActive(false);
		}
		else
		{
			ARSubObj.SetActive(true);
		}
		_nextModule = next;
	}
	
	public override void RemoveModule ()
	{
		bool next = false;
		bool prev = false;
		
		if(_prevModule != null)
		{
			_prevModule.SetNextModule(_nextModule);
			if(_prevModule.GetType() == GetType() && _prevModule.GetSize() == GetSize())
				prev = true;
		}
		else
			_nextModule.SetPrevModule(null);
		
		if(_nextModule != null)
		{
			_nextModule.SetPrevModule(_prevModule);
			if(_nextModule.GetType() == GetType() && _nextModule.GetSize() == GetSize())
				next = true;
		}
		else
			_prevModule.SetNextModule(null);
		
		if(next && !prev)
		{
			DynShelterModule nxt = _nextModule;
			while(nxt != null)
			{
				nxt.SetPos(true,_length);
				nxt = nxt.GetNextModule();
			}
		}
		else if (prev && !next)
		{
			DynShelterModule prv = _prevModule;
			while(prv != null)
			{
				prv.SetPos(true,-_length);
				prv = prv.GetPrevModule();
			}
		}
		else
		{
			DynShelterModule nxt = _nextModule;
			DynShelterModule prv = _prevModule;
			
			while(nxt != null)
			{
				nxt.SetPos(true,_length/2);
				nxt = nxt.GetNextModule();
			}
			
			while(prv != null)
			{
				prv.SetPos(true,-_length/2);
				prv = prv.GetPrevModule();
			}
			
			
		}
		
		if(next)
		{
			((DynShelterMultiBloc) _nextModule).WhoIs(true);
			((DynShelterMultiBloc) _nextModule).WhoIs(false);
		}
		else if(prev)
		{
			((DynShelterMultiBloc) _prevModule).WhoIs(true);
			((DynShelterMultiBloc) _prevModule).WhoIs(false);
		}
		
	

		MonoBehaviour.Destroy(GetGameObj());
	}
	
	//--vv--FCN's spécifiques--vv--------
	
	public void WhoIs(bool first)
	{
		if(first)
		{
			if(_prevModule != null)
			{
				if(_prevModule.GetType() != GetType() || _prevModule.GetSize()!=GetSize())
				{
					IAm(first,this);//Arret > réponse
				}
				else
				{
					((DynShelterMultiBloc) _prevModule).WhoIs(first);//Propagation question aux prevs
				}
			}
			else
			{
				IAm(first,this);//Arret > réponse	
			}
		}
		else //last
		{
			if(_nextModule != null)
			{
				if(_nextModule.GetType() != GetType() || _nextModule.GetSize()!=GetSize())
				{
					IAm(first,this);//Arret > réponse	
				}
				else
				{
					((DynShelterMultiBloc) _nextModule).WhoIs(first);//Propagation question aux suivants	
				}
			}
			else
			{
				IAm(first,this);//Arret > réponse
			}
		}
	}
	
	public void IAm(bool first,DynShelterMultiBloc bloc)
	{
		if(first)
		{
			_first = bloc;
			if(_nextModule != null)
			{
				if(_nextModule.GetType() == GetType() && _nextModule.GetSize()==GetSize())
				{
					((DynShelterMultiBloc) _nextModule).IAm(first,bloc);//Propagation de la réponse aux suivants	
				}
			}
		}
		else
		{
			_last = bloc;
			if(_prevModule != null)
			{
				if(_prevModule.GetType() == GetType() && _prevModule.GetSize()==GetSize())
				{
					((DynShelterMultiBloc) _prevModule).IAm(first,bloc);//Propagation de la reponse aux précedants
				}
			}
		}
	}
	
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
	
	public float GetAvArSize(){return _AvArSize;}
	
		//Déplacement tests
	private bool CheckTelescopicMove(float delta,bool checkNext,DynShelterModule nxt,DynShelterModule prv)
	{
		bool pushNext = false;
		bool pullNext = false;
		bool pushPrev = false;
		bool pullPrev = false;
		
		if(delta <0)//---------------------------------------------------------------------dep Z-
		{
			if(nxt != null && _lockNext && checkNext)
			{
				if(nxt.GetSize()!=GetSize())//---Si c'est un bloc
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
			
			if(prv != null && _lockPrev && !checkNext)
			{
				if(prv.GetSize()!=GetSize())//---Si c'est un bloc
				{
					if(Pull(true,delta))
					{
						pullPrev = true;
					}
				}
				else//-------------------------------------------------------------------------Si c'est une facade ou une extremité ou un bloc de mm taille
				{
					pullPrev = true;
					if(Contact(delta,AV,true,AR,true,prv,false))
						pullPrev = false;
				}
			}
		}
		else if(delta >0)//-----------------------------------------------------------------------------dep Z+
		{
			if(nxt != null && _lockNext && checkNext)
			{
				if(nxt.GetSize()!=GetSize())//---Si c'est un bloc
				{
					if(Pull(false,delta))
					{
						pullNext = true;	
					}
				}
				else//-------------------------------------------------------------------------Si c'est une facade ou une extremité ou un bloc de mm taille
				{
					pullNext = true;
					if(Contact(delta,AR,true,AV,true,nxt,true))
						pullNext = false;
				}
			}
			
			if(prv != null && _lockPrev && !checkNext)
			{
				if(prv.GetSize()!=GetSize())//---Si c'est un bloc
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
			if(Mathf.Sign((_first.GetPrevModule().GetExtPos(AR) - (GetIntPos(AR)+new Vector3(0,0,delta))).z) < 0 )
				return true;
			else
				return false;
		}
		else //next
		{
			if(Mathf.Sign((_last.GetNextModule().GetExtPos(AV) - (GetIntPos(AV)+new Vector3(0,0,delta))).z) > 0 )
				return true;
			else
				return false;
		}
	}	
	private bool Pull(bool prev,float delta)
	{
		if(prev)
		{
			if(Mathf.Sign((GetIntPos(AV)+new Vector3(0,0,delta)-_first.GetPrevModule().GetIntPos(AR)).z) < 0 )
				return true;
			else
				return false;
		}
		else //next
		{
			if(Mathf.Sign(((GetIntPos(AR)+new Vector3(0,0,delta)) - _last.GetNextModule().GetIntPos(AV)).z) > 0 )
				return true;
			else
				return false;
		}
	}
}
