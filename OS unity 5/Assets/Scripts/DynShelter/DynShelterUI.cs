using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class DynShelterHangs
{
	public int iid = 0;
	
	public bool bselected = false;
	public bool block = true;
	public bool blimit = false;
	
	public Vector2 v2position = Vector2.zero;
}

public class DynShelterUI
{
	//Public's
	public string[] items;
	
	private float wheelSpeed = 10f;
	private float viewHeight = 100;
	private float margin	 = 125;
	
	public Texture2D fade;
	
	//Private's
	private Rect _uiSvExt;
	private Rect _uiSvInt;
	private Rect _uiSgInt;
	private Rect _uiSelectorPos;
	private Rect _uiSlideDetect;
	
	private Rect _rightArea;
	private Rect _leftArea;
	private Rect _upperArea;
	
	private Rect rectLine;
	public Rect rectGroupDS;
	
	private Vector2 svPos;
	private Vector2 _scrollpos;
	
	private int itmSelected = 0;
	private int targetIndex;
	
	private float targetPos;
	private float lastDelta = 0;
	private float _resetTimer;
	private float _resetLimit = 5;
	private float fsizeXBloc;
	private float m_ftimerUnselect = 1.0f;
	private float m_ftargetOffsetSnap = 0.0f;
	private float m_fXpositionBeforeSnap = 0.0f;
	private float m_fmouseXposition = 0.0f;
	private float m_foffsetMove = 0.0f;
	
	private bool _resetActive = true;
	
	private FunctionConf_Dynshelter _configurator;
	private DynShelterModManager _modManager;
	
	private bool _prevLock,_nextLock;
	private bool _canRemove;
	private bool _transpModules = false;
	private bool _showConceptMenu = true;
	private bool _hasMove = false;
	private bool m_binitShelterEditor = false;
	private bool m_breset = false;
	private bool m_bswitchFacade = false;
	
	private DynShelterHangs m_currentHangs = null;
	private DynShelterHangs m_saveCurrentHangs = null;
	private DynShelterHangs m_snapHangs = null;
	
	private List<DynShelterHangs> m_toggleHangs = new List<DynShelterHangs>();
	private List<DynShelterHangs> m_toggleLimits = new List<DynShelterHangs>();
	
	private FunctionConf_Dynshelter.UISelector _UISelector;
	
	public DynShelterUI(FunctionConf_Dynshelter configurator)
	{
		_configurator = configurator;
		_modManager = configurator.GetModManager();
		
		_UISelector = configurator.GetUISelector();
				
		_uiSelectorPos = new Rect(0,Screen.height/2 - viewHeight/2,200,viewHeight);
		_uiSlideDetect = new Rect(0,Screen.height/2 - (viewHeight+100)/2,200,viewHeight+100);
		_uiSvExt = new Rect(0,0,_uiSelectorPos.width,viewHeight);
		targetPos = margin-(viewHeight/4);
		
		_rightArea = new Rect(Screen.width-260,100,260,Screen.height-100);
		_leftArea = new Rect(0.0f, 0.0f, Screen.width * 2.0f, Screen.height);//new Rect(0,_uiSlideDetect.y-200,260,_uiSelectorPos.height+200+150);
		_upperArea = new Rect(Screen.width/2 - 400,50,800,150);
		
		items = new string[0];
	}
	
	public void Relocate()
	{
		_uiSelectorPos = new Rect(0,Screen.height/2 - viewHeight/2,200,viewHeight);
		_uiSlideDetect = new Rect(0,Screen.height/2 - (viewHeight+100)/2,200,viewHeight+100);
		
		_rightArea = new Rect(Screen.width-260,100,260,Screen.height-100);
		_leftArea = new Rect(0.0f, 0.0f, Screen.width * 2.0f, Screen.height);//new Rect(0,_uiSlideDetect.y-200,260,_uiSelectorPos.height+200+150);
		_upperArea = new Rect(Screen.width/2 - 400,50,800,150);
	}
	
	public void UIUpdate()
	{
#if UNITY_STANDALONE || UNITY_EDITOR
		Vector2 cursor = Input.mousePosition;
		cursor.y = Screen.height - cursor.y;
		
		if(Input.GetMouseButtonUp(0))
		{
			if(!_leftArea.Contains(cursor) && !_rightArea.Contains(cursor) && !_upperArea.Contains(cursor))
			{
				_configurator.QuitOverride();
			}	
		}
		
		if(_uiSlideDetect.Contains(cursor))
		{
			if(!_transpModules)
			{
				//_configurator.UpdateVisibility();
				_transpModules = true;
				_resetActive = false;
			}
//			svPos.y += (Input.GetAxis("ScrollWheel")*wheelSpeed);
			if(Input.GetAxis("Mouse ScrollWheel")>0)
			{
				targetIndex --;
				targetIndex = Mathf.Clamp(targetIndex,0,items.Length-1);
				targetPos = margin-(viewHeight/4) + targetIndex * 50;
			}
			if(Input.GetAxis("Mouse ScrollWheel")<0)
			{
				targetIndex ++;
				targetIndex = Mathf.Clamp(targetIndex,0,items.Length-1);
				targetPos = margin-(viewHeight/4) + targetIndex * 50;
			}
		}
		else
		{
			if(_transpModules)
			{
				_resetActive = true;
				_resetTimer = 0;
				_transpModules = false;
			}
		}
		
		if(targetPos != svPos.y)
		{
			svPos.y = Mathf.Lerp(svPos.y,targetPos,Time.deltaTime * wheelSpeed);
			if(Mathf.Abs(targetPos - svPos.y)<0.1f)
			{
				svPos.y = targetPos;
				//itmSelected = targetIndex;
				_configurator.SetSelected(targetIndex);
				//_configurator.UpdateVisibility();
			}
		}
#elif UNITY_ANDROID || UNITY_IPHONE
		if(Input.touchCount>0) //-> si touch
		{
			Touch t = Input.GetTouch(0);
			
			Vector2 cursor = t.position;
			cursor.y = Screen.height - cursor.y;
			
			if(!_leftArea.Contains(cursor) && !_rightArea.Contains(cursor) && !_upperArea.Contains(cursor))
			{
				if(t.phase == TouchPhase.Ended)
					_configurator.QuitOverride();
			}
			
			if(_uiSlideDetect.Contains(cursor)) //-> si dans zone
			{
				if(t.phase == TouchPhase.Moved)
				{
					if(!_transpModules)
					{
						_configurator.UpdateVisibility();
						_transpModules = true;
						_resetActive = false;
					}
					_hasMove = true;
					svPos.y += t.deltaPosition.y;
					lastDelta = t.deltaPosition.y;
				}
				else if(t.phase == TouchPhase.Ended)
				{
					if(_transpModules)
					{
						_resetActive = true;
						_resetTimer = 0;
						_transpModules = false;
					}
					
					if(_hasMove)
					{
						_hasMove = false;
						if(lastDelta > 0)	// ^
						{
							targetIndex = Mathf.RoundToInt((svPos.y+15 - (margin-viewHeight/4))/50);
						}
						else 				// v
						{
							targetIndex = Mathf.RoundToInt((svPos.y-15 - (margin-viewHeight/4))/50);
						}
						targetIndex = Mathf.Clamp(targetIndex,0,items.Length-1);
						
						targetPos = margin-(viewHeight/4) + targetIndex * 50;
						
						_configurator.SetSelected(targetIndex);
						_configurator.UpdateVisibility();
					}
				}
			}
			else // -> pas dans zone
			{
				if(_transpModules)
				{
					_resetActive = true;
					_resetTimer = 0;
					_transpModules = false;
				}
				
				if(_hasMove)
				{
					_hasMove = false;
					if(lastDelta > 0)	// ^
					{
						targetIndex = Mathf.RoundToInt((svPos.y+15 - (margin-viewHeight/4))/50);
					}
					else 				// v
					{
						targetIndex = Mathf.RoundToInt((svPos.y-15 - (margin-viewHeight/4))/50);
					}
					targetIndex = Mathf.Clamp(targetIndex,0,items.Length-1);
					
					targetPos = margin-(viewHeight/4) + targetIndex * 50;
					
					_configurator.SetSelected(targetIndex);
					_configurator.UpdateVisibility();
				}
				if(targetPos != svPos.y)
				{
					svPos.y = Mathf.Lerp(svPos.y,targetPos,Time.deltaTime * wheelSpeed);
					if(Mathf.Abs(targetPos - svPos.y)<0.1f)
					{
						svPos.y = targetPos;
						_configurator.SetSelected(targetIndex);
						_configurator.UpdateVisibility();
					}
				}
			}
		}
		
		if(targetPos != svPos.y && !_hasMove)
		{
			svPos.y = Mathf.Lerp(svPos.y,targetPos,Time.deltaTime * wheelSpeed);
			if(Mathf.Abs(targetPos - svPos.y)<0.1f)
			{
				svPos.y = targetPos;
				_configurator.SetSelected(targetIndex);
				_configurator.UpdateVisibility();
			}
		}
		
#endif
		
		if(_resetActive)
		{
			_resetTimer += Time.deltaTime;
			if(_resetTimer > _resetLimit)
			{
				//Réaffiche tout
				
				_resetActive = false;
				//_configurator.UpdateVisibility(true);
			}
		}
	}
	
	public void SetItemsNb(int nb)
	{
		items = new string[nb];
		for(int i = 0;i<nb;i++)
		{
			items[i] = (i+1).ToString()+"/"+nb.ToString();	
		}
		
		_uiSvInt = new Rect(0,0,_uiSelectorPos.width,items.Length*50 + margin*2);
		_uiSgInt = new Rect(0,margin,_uiSelectorPos.width,items.Length * 50);
	}
	
	public void SetSelectedItem(int i)
	{
		if(items.Length>0)
		{
			targetIndex = Mathf.Clamp(i,0,items.Length-1);
			targetPos = margin-(viewHeight/4) + targetIndex * 50;
		}
	}
	
	public void SetRemovability(bool val)
	{
		_canRemove = val;	
	}
	
	public void SetSelectedLocks(bool prev,bool next)
	{
		_prevLock = prev;
		_nextLock = next;
	}

	public void GetGUIDesigner()
	{
	}

	public void GetGUILeft()
	{
		//Fade
		GUI.Box(new Rect(260,_uiSelectorPos.y-200,-260,150),"","bgFadeUp");
		GUI.Label(new Rect(0,_uiSelectorPos.y-100,150,50),TextManager.GetText("DynShelter.Selecteur"),"Title");
		GUI.Box(new Rect(260,_uiSelectorPos.y-50,-260,_uiSelectorPos.height+50),"","bgFadeMid");
		GUI.Box(new Rect(260,_uiSelectorPos.yMax,-260,150),"","bgFadeDw");
		
		//Selecteur
		GUI.BeginGroup(_uiSelectorPos);
		if(items.Length > 0)
		{
			GUI.BeginScrollView(_uiSvExt,svPos,_uiSvInt,false,false);
			//			GUI.SelectionGrid(uiSgInt,itmSelected,items,1,"Grid");
			
			GUI.Label(new Rect(0,_uiSgInt.y - 50,_uiSgInt.width,50),"","Grid");
			GUI.BeginGroup(_uiSgInt);
			for(int i=0;i<items.Length;i++)
			{
				GUI.Label(new Rect(0,i*50,_uiSgInt.width,50),items[i],"Grid");
			}
			GUI.EndGroup();
			GUI.Label(new Rect(0,_uiSgInt.yMax,_uiSgInt.width,50),"","Grid");
			
			GUI.EndScrollView();
		}
		
		//Fader
		GUI.Label(new Rect(0,0,_uiSelectorPos.width,25),"","Fade");
		GUI.Label(new Rect(0,viewHeight,_uiSelectorPos.width,-25),"","Fade");
		
		GUI.EndGroup();
		
		if(!_configurator.IsAbrifixe())
		{
			//LockBtns
			if(targetPos == svPos.y)
			{
				if(targetIndex > 0)
				{
					GUI.Label(new Rect(_uiSelectorPos.width-75,_uiSelectorPos.y + 10,75,30),"","PrevLockBg");
					bool tmpPrv = GUI.Toggle(new Rect(_uiSelectorPos.width-50,_uiSelectorPos.y /*+ 10*/,50,50),_prevLock,"PRV","Lock");
					if(tmpPrv != _prevLock)
					{
						_prevLock = tmpPrv;
						_configurator.UpdateLocks(tmpPrv,_nextLock);
					}
				}
				
				if(targetIndex < items.Length-1)
				{
					GUI.Label(new Rect(_uiSelectorPos.width-75,_uiSelectorPos.yMax - 40,75,30),"","NextLockBg");
					bool tmpNxt = GUI.Toggle(new Rect(_uiSelectorPos.width-50,_uiSelectorPos.yMax -50,50,50),_nextLock,"NXT","Lock");
					if(tmpNxt != _nextLock)
					{
						_nextLock = tmpNxt;
						_configurator.UpdateLocks(_prevLock,tmpNxt);
					}
				}
				
				//ANCHOR BTN
				if(_configurator.GetCurrentModule().GetModuleType() == FunctionConf_Dynshelter.ModuleType.bloc || 
				   _configurator.GetCurrentModule().GetModuleType() == FunctionConf_Dynshelter.ModuleType.multiBloc)
				{
					bool tmp = GUI.Toggle(new Rect(25,_uiSelectorPos.y + _uiSelectorPos.height/2 - 25,50,50),
					                      _configurator.GetCurrentModule().IsAnchored(),"","rivet");
					if(tmp != _configurator.GetCurrentModule().IsAnchored())
					{
						_configurator.GetCurrentModule().SetAnchor(tmp);	
					}
				}
			}
		}
		
		
		
		//		//BUTTONS UP DWN
		if(GUI.Button(new Rect(_uiSelectorPos.width/2 - 25,_uiSelectorPos.yMin-50,50,50),"^^^","btnUp"))
		{
			targetIndex= Mathf.Clamp(--targetIndex,0,items.Length-1);
			targetPos = margin-(viewHeight/4) + targetIndex * 50;
			_resetActive = true;
			_resetTimer = 0;
		}
		if(GUI.Button(new Rect(_uiSelectorPos.width/2 - 25,_uiSelectorPos.yMax,50,50),"vvv","btnDw"))
		{
			targetIndex= Mathf.Clamp(++targetIndex,0,items.Length-1);
			targetPos = margin-(viewHeight/4) + targetIndex * 50;
			_resetActive = true;
			_resetTimer = 0;
		}
		
		//		//Move
		//		if(GUI.RepeatButton(new Rect(10,uiSelectorPos.y,50,50),"+","move+"))
		//		{
		//			_configurator.MoveSelectedModule(true);
		//		}
		//		if(GUI.RepeatButton(new Rect(10,uiSelectorPos.yMax - 50,50,50),"-","move-"))
		//		{
		//			_configurator.MoveSelectedModule(false);
		//		}
	}
	
	public void GetGUIRight()
	{
		
		GUILayout.BeginArea(new Rect(Screen.width - 260, 0, 260, Screen.height));
		GUILayout.FlexibleSpace();
		
		_scrollpos = GUILayout.BeginScrollView(_scrollpos,GUILayout.Width(260));//scrollView en cas de menu trop grand
		
		GUILayout.Box("","bgFadeUp",GUILayout.Width(260),GUILayout.Height(150));//fade en haut
		GUILayout.BeginVertical("bgFadeMid",GUILayout.Width(260));
		
		//---vv--UI Here--vv---------------
		GUILayout.Label(TextManager.GetText("DynShelter.Title"),"Title",GUILayout.Height(50),GUILayout.Width(260));
		
		//		GUILayout.Label(TextManager.GetText("DynShelter.Conception"),"Menu",GUILayout.Height(50),GUILayout.Width(260));
		bool tmpConcept = GUILayout.Toggle(_showConceptMenu,TextManager.GetText("DynShelter.Conception")
		                                   ,"Menu",GUILayout.Height(50),GUILayout.Width(260));
		if(tmpConcept && !_showConceptMenu)
		{
			_showConceptMenu = tmpConcept;
			_UISelector =FunctionConf_Dynshelter.UISelector.none;
			_configurator.SetUISelector(_UISelector);
			_configurator.ShowArrows(true);
		}
		else if(!tmpConcept && _showConceptMenu)
		{
			_showConceptMenu = tmpConcept;
			_UISelector =FunctionConf_Dynshelter.UISelector.none;
			_configurator.SetUISelector(_UISelector);
		}
		
		
		if(_showConceptMenu)
		{
			//STYLES
			bool tmpStyles = GUILayout.Toggle(_UISelector ==FunctionConf_Dynshelter.UISelector.styles,TextManager.GetText("DynShelter.Styles"),"outil",GUILayout.Height(50),GUILayout.Width(260));
			if(tmpStyles && _UISelector !=FunctionConf_Dynshelter.UISelector.styles)
			{
				_UISelector =FunctionConf_Dynshelter.UISelector.styles;
				_configurator.SetUISelector(_UISelector);
			}
			//COULEURS
			bool tmpColors = GUILayout.Toggle(_UISelector ==FunctionConf_Dynshelter.UISelector.colors,TextManager.GetText("DynShelter.Couleurs"),"outil",GUILayout.Height(50),GUILayout.Width(260));
			if(tmpColors && _UISelector !=FunctionConf_Dynshelter.UISelector.colors)
			{
				
				_UISelector =FunctionConf_Dynshelter.UISelector.colors;
				_configurator.SetUISelector(_UISelector);
			}
			
			if(!_configurator.IsAbrifixe())
			{
				if(_configurator.GetCurrentModule()!=null)
				{
					if(_configurator.GetCurrentModule().GetModuleType() != FunctionConf_Dynshelter.ModuleType.facade)
					{	//ADDNEXT					
						bool tmpNext = GUILayout.Toggle(_UISelector ==FunctionConf_Dynshelter.UISelector.addNext ,TextManager.GetText("DynShelter.AddNext"),"outilNext",GUILayout.Height(50),GUILayout.Width(260));
						if(tmpNext && _UISelector !=FunctionConf_Dynshelter.UISelector.addNext)
						{
							_UISelector =FunctionConf_Dynshelter.UISelector.addNext;
							_configurator.SetUISelector(_UISelector);
							
							_configurator.setNextInsertion(true);
						}				
						
						//ADDPREV
						bool tmpPrev = GUILayout.Toggle(_UISelector ==FunctionConf_Dynshelter.UISelector.addPrev ,TextManager.GetText("DynShelter.AddPrevious"),"outilPrev",GUILayout.Height(50),GUILayout.Width(260));
						if(tmpPrev && _UISelector !=FunctionConf_Dynshelter.UISelector.addPrev)
						{
							_UISelector =FunctionConf_Dynshelter.UISelector.addPrev;
							_configurator.SetUISelector(_UISelector);
							
							_configurator.setNextInsertion(false);
						}
						
					}
				}
				
				//DEPLACEMENT
				GUILayout.BeginHorizontal("outil",GUILayout.Height(50),GUILayout.Width(260));
				GUILayout.FlexibleSpace();
				if(GUILayout.RepeatButton("+","move+",GUILayout.Height(50),GUILayout.Width(50)))
				{
					if(_UISelector != FunctionConf_Dynshelter.UISelector.none)
					{
						_UISelector = FunctionConf_Dynshelter.UISelector.none;
						_configurator.SetUISelector(_UISelector);
					}
					_configurator.MoveSelectedModule(true, Time.deltaTime * 300.0f);
				}
				
				GUILayout.Label(TextManager.GetText("DynShelter.Move"),GUILayout.Height(50),GUILayout.Width(100));
				
				if(GUILayout.RepeatButton("-","move-",GUILayout.Height(50),GUILayout.Width(50)))
				{
					if(_UISelector != FunctionConf_Dynshelter.UISelector.none)
					{
						_UISelector = FunctionConf_Dynshelter.UISelector.none;
						_configurator.SetUISelector(_UISelector);
					}
					_configurator.MoveSelectedModule(false, Time.deltaTime * 300.0f);
				}
				
				GUILayout.Space(20);
				GUILayout.EndHorizontal();
			}
			else
			{
				if(_configurator.GetCurrentModule()!=null)
				{
					if(_configurator.GetCurrentModule().GetModuleType() != FunctionConf_Dynshelter.ModuleType.facade)
					{	
						//ADDNEXT					
						bool tmpNext = GUILayout.Toggle(_UISelector ==FunctionConf_Dynshelter.UISelector.addNext ,TextManager.GetText("DynShelter.AddNext"),"outil",GUILayout.Height(50),GUILayout.Width(260));
						//if(tmpNext && _UISelector !=FunctionConf_Dynshelter.UISelector.addNext)
						if(tmpNext && _UISelector !=FunctionConf_Dynshelter.UISelector.addPrev)
						{
							/*_UISelector =FunctionConf_Dynshelter.UISelector.addNext;
							_configurator.SetUISelector(_UISelector);							
							_configurator.setNextInsertion(true);*/							
							_configurator.setNextInsertion(false);
							_configurator.AddDirectModule();
						}	
						
					}
				}				
			}
			
			//INTEGRATED MOD
			_modManager.GetIntegratedGUI();
			
			if(!_configurator.IsAbrifixe())
			{
				//LIMITS
				bool tmpLimits = GUILayout.Toggle(_UISelector ==FunctionConf_Dynshelter.UISelector.limits,TextManager.GetText("DynShelter.LimitedMove"),"outil",GUILayout.Height(50),GUILayout.Width(260));
				if(tmpLimits && _UISelector !=FunctionConf_Dynshelter.UISelector.limits)
				{
					_UISelector =FunctionConf_Dynshelter.UISelector.limits;
					_configurator.SetUISelector(_UISelector);
				}
			}
			
			//REMOVE
			if(_canRemove)
			{
				if(GUILayout.Button(TextManager.GetText("DynShelter.Delete"),"outil",GUILayout.Height(50),GUILayout.Width(260)))
				{
					_configurator.RemoveCurrent();
				}
			}
		}
		
		//--vv--MODS--vv--
		
		bool tmpMods = GUILayout.Toggle(_UISelector ==FunctionConf_Dynshelter.UISelector.mods,TextManager.GetText("DynShelter.AdvancedOptions")
		                                ,"Menu+",GUILayout.Height(50),GUILayout.Width(260));
		if(tmpMods && _UISelector !=FunctionConf_Dynshelter.UISelector.mods)
		{
			_showConceptMenu = false;
			_UISelector =FunctionConf_Dynshelter.UISelector.mods;
			_configurator.SetUISelector(_UISelector);
			_configurator.ShowArrows(false);
		}
		else if(!tmpMods && _UISelector ==FunctionConf_Dynshelter.UISelector.mods)
		{
			_UISelector =FunctionConf_Dynshelter.UISelector.none;
			_configurator.SetUISelector(_UISelector);
			_configurator.ShowArrows(true);
		}
		
		if(_UISelector ==FunctionConf_Dynshelter.UISelector.mods)
		{
			_modManager.GetUI();
		}
		
		GUILayout.EndVertical();
		GUILayout.Box("","bgFadeDw",GUILayout.Width(260),GUILayout.Height(150));//fade en bas
		
		GUILayout.EndScrollView();
		
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();
	}
	
	private void InitGUIShelterEditor()
	{
		//rectGroupDS = new Rect((Screen.width * 0.5f) - (Screen.width * 0.8f * 0.5f), Screen.height * 0.06f, Screen.width * 0.8f, Screen.height * 0.42f);
		rectGroupDS = new Rect((Screen.width * 0.5f) - (572.5f * 0.755f), Screen.height * 0.06f, 1145.0f * 0.755f, 418.0f * 0.725f);
		
		fsizeXBloc = 48.0f;
		
		float fsizeXlineLimit = ((_configurator.GetMaxLimitBwd() - _configurator.GetMaxLimitFwd()).magnitude * (fsizeXBloc * 0.5f));
		
		fsizeXlineLimit += (rectGroupDS.width - fsizeXlineLimit) * 0.5f;
		
		rectLine = new Rect(((rectGroupDS.width - fsizeXlineLimit) * 0.5f), rectGroupDS.height * 0.425f, fsizeXlineLimit, 1.0f);
		
		for(int i = 0; i < 2; i++)
		{
			m_toggleLimits.Add(new DynShelterHangs());
			
			m_toggleLimits[i].blimit = true;
			
			m_toggleLimits[i].iid = i;
		}
		
		m_currentHangs = m_toggleLimits[0];
		m_currentHangs.bselected = true;
		
		for(int i = 0; i < _configurator.GetModules().Count; i++)
		{
			m_toggleHangs.Add(new DynShelterHangs());
			
			m_toggleHangs[i].iid = i;
		}
		
		_configurator.CenterDeployAndFeetLimit();
		
		m_binitShelterEditor = true;
		m_breset = true;
	}
	
	private void CheckAddAndRemoveToggleShelterEditor()
	{
		if(_configurator.GetModules().Count > m_toggleHangs.Count)
		{
			DynShelterHangs tempDSH = new DynShelterHangs();
			
			m_toggleHangs.Add(tempDSH);
			
			_configurator.SetUISelector(FunctionConf_Dynshelter.UISelector.none);
			_UISelector = FunctionConf_Dynshelter.UISelector.none;
			
			for(int i = 0; i < m_toggleHangs.Count; i++)
			{
				m_toggleHangs[i].iid = i;
			}
			
			m_currentHangs = m_toggleHangs[_configurator.GetSelectedIndex()];
			m_currentHangs.bselected = true;
			
			UnselectOtherToggleShelterEditor();
			
			m_breset = true;

			_configurator.CenterDeployAndFeetLimit();
		}
		else if(_configurator.GetModules().Count < m_toggleHangs.Count)
		{
			m_toggleHangs.Remove(m_toggleHangs[m_toggleHangs.Count - 1]);
			
			_configurator.SetUISelector(FunctionConf_Dynshelter.UISelector.none);
			_UISelector = FunctionConf_Dynshelter.UISelector.none;
			
			for(int i = 0; i < m_toggleHangs.Count; i++)
			{
				m_toggleHangs[i].iid = i;
			}
			
			m_currentHangs = null;
			
			LockAll();
			UnselectAll();
			
			m_breset = true;
		}
	}
	
	private void MoveToggleShetlterEditor()
	{
		Vector2 cursor = Vector2.zero;

#if UNITY_STANDALONE || UNITY_EDITOR

		cursor = Input.mousePosition;

#elif UNITY_ANDROID || UNITY_IPHONE
		
		if(Input.touchCount>0)
		{
			Touch t = Input.GetTouch(0);
			cursor = t.position;
		}

#endif

		cursor.y = Screen.height - cursor.y;

		Rect rectContainer = rectGroupDS;
		rectContainer.height = rectLine.y + (rectGroupDS.height * 0.125f);
		
		if(rectContainer.Contains(cursor))
		{
			float ftemp = 0.0f;
				
#if UNITY_STANDALONE || UNITY_EDITOR

			if(cursor.x != m_fmouseXposition && Input.GetKey(KeyCode.Mouse0))
			{
				ftemp = cursor.x > m_fmouseXposition ? 1.0f : -1.0f;
			}
			else
			{
				ftemp = 0.0f;
			}
			
			m_fmouseXposition = cursor.x;
			
#elif UNITY_ANDROID || UNITY_IPHONE
			
			ftemp = GameObject.Find("MainScene").GetComponent<GUIMenuRight>().slidedetect(false, Time.deltaTime * 100.0f);
#endif
			//Debug.Log("temp : " + ftemp + " <0 && " + cursor.x + "<" + m_currentHangs.v2position.x);
			if(m_currentHangs != null && ftemp != 0.0f)
			{
				bool bmoved = false;
				if(m_currentHangs.blimit && ((ftemp > 0 && cursor.x > m_currentHangs.v2position.x) || (ftemp < 0 && cursor.x < m_currentHangs.v2position.x)))
				{
					#if UNITY_STANDALONE || UNITY_EDITOR		
					float fspeed = Time.deltaTime * Mathf.Abs(cursor.x - m_currentHangs.v2position.x) * 1.0f;
					#elif UNITY_ANDROID || UNITY_IPHONE
					float fspeed = Time.deltaTime * Mathf.Abs(cursor.x - m_currentHangs.v2position.x) * 0.1f;
					#endif	
					if(m_currentHangs.iid == 0)
					{
						_configurator.MoveLimitBwd(ftemp * fspeed);
					}
					else
					{
						_configurator.MoveLimitFwd(ftemp * fspeed);
					}
					
					m_breset = false;
				}
				else if((ftemp > 0 && cursor.x > m_currentHangs.v2position.x) || (ftemp < 0 && cursor.x < m_currentHangs.v2position.x))
				{
					m_breset = false;
#if UNITY_STANDALONE || UNITY_EDITOR
					float fspeed = Mathf.Abs(cursor.x - m_currentHangs.v2position.x) * 1.0f;
					if(ftemp > 0 && cursor.x > m_toggleLimits[1].v2position.x)
					{
						fspeed = Mathf.Abs(m_toggleLimits[1].v2position.x - m_currentHangs.v2position.x) * 0.25f;
					}
					else if(ftemp < 0 && cursor.x < m_toggleLimits[0].v2position.x)
					{
						fspeed = Mathf.Abs(m_toggleLimits[0].v2position.x - m_currentHangs.v2position.x) * 0.25f;
					}
					
					bmoved = _configurator.MoveSelectedModule(ftemp > 0, fspeed);
#elif UNITY_ANDROID || UNITY_IPHONE
					_configurator.MoveSelectedModule(ftemp > 0, Mathf.Abs(cursor.x - m_currentHangs.v2position.x) * 0.1f);

#endif
				}
				//Debug.Log(bmoved);
			}
		}
	}
	
	private void UnselectOtherToggleShelterEditor()
	{
		foreach(DynShelterHangs dsh in m_toggleHangs)
		{
			if(dsh != m_currentHangs)
			{
				dsh.bselected = false;
			}
		}
		
		foreach(DynShelterHangs dsh in m_toggleLimits)
		{
			if(dsh != m_currentHangs)
			{
				dsh.bselected = false;
			}
		}
	}
	
	private void UnselectAll()
	{
		foreach(DynShelterHangs dsh in m_toggleHangs)
		{
			dsh.bselected = false;
		}
		
		foreach(DynShelterHangs dsh in m_toggleLimits)
		{
			dsh.bselected = false;
		}
		
		_configurator.ActiveLimits(false);
		m_currentHangs = null;
		_configurator.SetUISelector(FunctionConf_Dynshelter.UISelector.none);
		_UISelector = FunctionConf_Dynshelter.UISelector.none;
	}
	
	private void GetGUICadenas(Rect _rectBloc, int _iid, float _fpreviousX, float _foffset)
	{
		GUI.Box(new Rect(_rectBloc.x + _rectBloc.width, rectLine.y, 1.0f, (fsizeXBloc * 0.25f) + _foffset + 4.0f), "", "w_p");
		
		bool bcheck = true;
		
		string sznameStyle = "cadena";
		
		if(!m_toggleHangs[_iid].block && (_rectBloc.x + fsizeXBloc) <= _fpreviousX - 15.0f)
		{
			bcheck = false;
			sznameStyle = "badCadena";
		}
		
		float fsizeCadena = 24.0f;
		bool btemp = GUI.Toggle (new Rect(_rectBloc.x + (_rectBloc.width - (fsizeCadena * 0.5f)), _rectBloc.y + _rectBloc.height + (fsizeXBloc * 0.25f) + _foffset, fsizeCadena, fsizeCadena), 
		                         m_toggleHangs[_iid].block, "", sznameStyle);
		
		if(bcheck && btemp != m_toggleHangs[_iid].block)
		{
			m_breset = false;
			m_toggleHangs[_iid].block = btemp;
			
			if(_configurator.GetModule(_iid) != null)
			{
				_configurator.GetModule(_iid).SetPrevLocks(m_toggleHangs[_iid].block);
				if(_configurator.GetModule(_iid).GetPrevModule() != null)
				{
					_configurator.GetModule(_iid).GetPrevModule().SetNextLocks(m_toggleHangs[_iid].block);
				}
				
				if(btemp && (_rectBloc.x + fsizeXBloc) < _fpreviousX)
				{
					if(_configurator.GetModule(_iid).GetSize() == _configurator.GetModule(_iid).GetPrevModule().GetSize())
					{
						_configurator.CenterDeployAndFeetLimit();

						m_breset = true;

						LockAll();
					}
					else
					{
						m_snapHangs = m_toggleHangs[_iid];
						m_fXpositionBeforeSnap = m_toggleHangs[_iid].v2position.x + _fpreviousX - (_rectBloc.x + fsizeXBloc);
						
						if(_foffset == 0.0f)
						{
							_configurator.SetSelectedIndex(_iid);
							_configurator.UpdateCurrentModule();
						}
					}
				}
			}
			
			_configurator.SetUISelector(FunctionConf_Dynshelter.UISelector.none);
			_UISelector = FunctionConf_Dynshelter.UISelector.none;
		}
	}
	
	public bool GetGUILimitStrip(Vector3 _v3maxLimit, Vector3 _v3limit, bool _bselected, int _isens)
	{
		float foffsetBlocX = 0.0f;//(_configurator.GetNumberFacade() > 0 && ((_configurator.GetModules().Count - 1) % 2 == 0)) || (_configurator.GetModules().Count - 1) % 2 != 0 ? (fsizeXBloc * 0.5f) : 0.0f;
		Rect rectLimitStrip = new Rect(((_v3maxLimit - _v3limit).magnitude * (fsizeXBloc * 0.5f)) + rectLine.x + ((rectGroupDS.width - rectLine.width) * 0.5f) - foffsetBlocX, rectLine.y, 1.0f, -(fsizeXBloc * 0.575f));
		
		GUI.Box(rectLimitStrip, "", "w_p");
		GUI.Box(new Rect(rectLimitStrip.x, rectLimitStrip.y + rectLimitStrip.height, fsizeXBloc * 0.5f * _isens, 1.0f), "", "w_p");
		GUI.Box(new Rect(rectLimitStrip.x + (fsizeXBloc * 0.5f * _isens), (rectLimitStrip.y + rectLimitStrip.height), 1.0f, -fsizeXBloc * 1.2f), "", "w_p");

		DynShelterModule module = _isens < 0 ? _configurator.GetModule(m_toggleHangs.Count - 1) : _configurator.GetModule(0);
		
		if(module.GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade)
		{
			module = _isens < 0 ? _configurator.GetModule(m_toggleHangs.Count - 2) : _configurator.GetModule(1);
		}
		
		//Vector3 v3module = module.GetPos();
		//float flength = (_v3limit - v3module).magnitude - 1.0f;
		//float fdecimal = flength - (int)flength;
		//float finteger = flength - fdecimal;
		//float foffset = module.GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade ? 100.0f : 0.0f;
		//fdecimal = Mathf.Clamp((int)((fdecimal * 100.0f)), 0.0f, float.MaxValue);
		//string sz = (finteger) + "." + fdecimal + " m";
		//GUI.Label(new Rect(rectLimitStrip.x + (_isens < 0 ? (sz.Length * 0.5f * 20.0f * _isens) : 10.0f), rectLimitStrip.y - 25.0f, rectGroupDS.width, rectGroupDS.height), sz);

		float fsizeLimit = 32.0f;
		bool btemp = GUI.RepeatButton(new Rect(rectLimitStrip.x - (fsizeLimit * 0.5f) + (fsizeXBloc * 0.5f * _isens), rectLimitStrip.y - (fsizeXBloc * 1.75f) - fsizeLimit, fsizeLimit, fsizeLimit), "", "limitStrip");
		
		if(_isens < 0)
		{
			m_toggleLimits[0].v2position.x = rectLimitStrip.x - (fsizeLimit * 0.5f) + (fsizeXBloc * 0.5f * _isens) + rectGroupDS.x;
		}
		else
		{
			m_toggleLimits[1].v2position.x = rectLimitStrip.x - (fsizeLimit * 0.5f) + (fsizeXBloc * 0.5f * _isens) + rectGroupDS.x;
		}
		
		if(btemp)
		{
			_configurator.SetUISelector(FunctionConf_Dynshelter.UISelector.none);
			_UISelector = FunctionConf_Dynshelter.UISelector.none;
		}
		
		return btemp;
	}
	
	private void GetGUIOpenAll()
	{
		if(m_breset)
		{
			GUI.Box(new Rect(rectGroupDS.width * 0.075f, rectGroupDS.height - (rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "resetOff");
		}
		else if(GUI.Button(new Rect(rectGroupDS.width * 0.075f, rectGroupDS.height - (rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "reset"))
		{
			_configurator.CenterDeployAndFeetLimit(false);
			
			m_breset = true;
		}
	}
	
	private void LockAll()
	{
		for(int i = 0; i < m_toggleHangs.Count; i++)
		{
			m_toggleHangs[i].block = true;
								
			if(_configurator.GetModule(i) != null)
			{
				_configurator.GetModule(i).SetPrevLocks(m_toggleHangs[i].block);
				if(_configurator.GetModule(i).GetPrevModule() != null)
				{
					_configurator.GetModule(i).GetPrevModule().SetNextLocks(m_toggleHangs[i].block);
				}
			}
		}
	}
	
	private void GetGUIColor()
	{
		if(_UISelector == FunctionConf_Dynshelter.UISelector.colors)
		{
			if(GUI.Button(new Rect(rectGroupDS.width * 0.225f, rectGroupDS.height - (rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "colorsOn"))
			{
				_UISelector =FunctionConf_Dynshelter.UISelector.none;
				_configurator.SetUISelector(_UISelector);
			}
		}
		else if(_UISelector != FunctionConf_Dynshelter.UISelector.styles)
		{
			if(GUI.Button(new Rect(rectGroupDS.width * 0.225f, rectGroupDS.height - (rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "colors"))
			{
				UnselectAll();
				
				_UISelector =FunctionConf_Dynshelter.UISelector.colors;
				_configurator.SetUISelector(_UISelector);
			}
		}
		else
		{
			GUI.Box(new Rect(rectGroupDS.width * 0.225f, rectGroupDS.height - (rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "colorsOff");
		}
	}
	
	private void GetGUIStyle()
	{
		if(_UISelector == FunctionConf_Dynshelter.UISelector.styles)
		{
					if(GUI.Button(new Rect(rectGroupDS.width * 0.325f, rectGroupDS.height - (rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "stylesOn"))
					{
						_UISelector =FunctionConf_Dynshelter.UISelector.none;
						_configurator.SetUISelector(_UISelector);
					}
		}
		else if(m_currentHangs != null &&
		 _UISelector != FunctionConf_Dynshelter.UISelector.colors &&
		 _configurator.GetNumberStyle() > 1)
		{
			if(GUI.Button(new Rect(rectGroupDS.width * 0.325f, rectGroupDS.height - (rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "styles"))
			{
				_UISelector = FunctionConf_Dynshelter.UISelector.styles;
				_configurator.SetUISelector(_UISelector);
			}
		}
		else
		{
			GUI.Box(new Rect(rectGroupDS.width * 0.325f, rectGroupDS.height - (rectGroupDS.height * 0.225f), 48.0f, 48.0f), "", "stylesOff");
		}
	}
	
	public void GetGUIShelterEditor()
	{
		if(!m_binitShelterEditor)
		{
			InitGUIShelterEditor();
		}
		
		if(m_snapHangs != null)
		{
			_configurator.MoveSelectedModule(true, 3.0f);
			
			if(m_fXpositionBeforeSnap < m_snapHangs.v2position.x)
			{
				m_snapHangs = null;
				_configurator.SetSelectedIndex(m_currentHangs.iid);
				_configurator.UpdateCurrentModule();
			}
		}
		
		/*Vector2 cursor = Vector2.zero;

#if UNITY_STANDALONE || UNITY_EDITOR
		
		cursor = Input.mousePosition;

#elif UNITY_ANDROID || UNITY_IPHONE
		
		if(Input.touchCount>0 && Input.GetTouch(0).phase == TouchPhase.Ended)
		{
			Touch t = Input.GetTouch(0);
			cursor = t.position;
		}
		
#endif
		
		cursor.y = Screen.height - cursor.y;*/
		
		CheckAddAndRemoveToggleShelterEditor();
		
		GUI.BeginGroup(rectGroupDS,"","backgroundDS");
		
		GUI.Box(rectLine, "", "w_p");
		
		GetGUIOpenAll();
		GetGUIColor();
		GetGUIStyle();
		
		GUI.Label(new Rect(0.0f, 10.0f, rectGroupDS.width, rectGroupDS.height), _configurator.modelName, "textNameModel");
		
		for(int i = 0; i < m_toggleLimits.Count; i++)
		{
			if(i == 0)
			{
				m_toggleLimits[i].bselected = GetGUILimitStrip(_configurator.GetMaxLimitBwd(), _configurator.GetLimitBwd(),
				 m_toggleLimits[i].bselected, -1);
			}
			else
			{
				m_toggleLimits[i].bselected = GetGUILimitStrip(_configurator.GetMaxLimitBwd(), _configurator.GetLimitFwd(),
				 m_toggleLimits[i].bselected, 1);
			}
			
			if(m_toggleLimits[i].bselected && m_snapHangs == null)
			{
				m_currentHangs = m_toggleLimits[i];
				
				_configurator.ActiveLimits(true);
				
				UnselectOtherToggleShelterEditor();
			}
		}
		
		float fpreviousPositionXBloc = 0.0f;
		
		for(int i = 0; i < m_toggleHangs.Count; i++)
		{
			if(_configurator.GetModule(i) != null)
			{
				float fsizeHeight = (fsizeXBloc) + ((_configurator.GetModule(i).GetSize() - 5) * 5.0f);
				
				Rect rectBloc = new Rect(
					Mathf.Abs((_configurator.GetMaxLimitBwd() - _configurator.GetPositionModule(i)).magnitude * (fsizeXBloc * 0.5f)) + ((rectGroupDS.width - rectLine.width) * 0.5f) + (fsizeXBloc * 1.25f), 
					rectLine.y - fsizeHeight, fsizeXBloc * _configurator.GetModule(i).GetIntOffSet(), fsizeHeight);
				
				m_toggleHangs[i].v2position = new Vector2(rectBloc.x + (rectBloc.width * 0.5f) + rectGroupDS.x, rectBloc.y + (rectBloc.height * 0.5f) + rectGroupDS.y);
				
				bool btemp = m_toggleHangs[i].bselected;
				
				float fsizeXFacade = 16.0f;
				if(_configurator.GetModule(i).GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade)
				{
					if(i < 2)
					{
						if(!_configurator.GetModule(i).bextrem)
						{
							btemp = GUI.Toggle(new Rect((rectBloc.x - rectBloc.width) + (fsizeXFacade) - 5.0f, rectBloc.y, fsizeXFacade, fsizeHeight), m_toggleHangs[i].bselected, "", "facade");
							fsizeXFacade = (rectBloc.x - rectBloc.width) + (fsizeXFacade) - 5.0f - 4.0f;
						}
						else
						{
							btemp = GUI.Toggle(new Rect(rectBloc.x + 8.0f, rectBloc.y, fsizeXFacade, fsizeHeight), m_toggleHangs[i].bselected, "", "facade");
							fsizeXFacade = rectBloc.x + 4.0f;
						}
					}
					else
					{
						btemp = GUI.Toggle(new Rect(rectBloc.x + (fsizeXBloc * 0.5f) + 2.0f, rectBloc.y, fsizeXFacade, fsizeHeight), m_toggleHangs[i].bselected, "", "facade");
						fsizeXFacade = rectBloc.x + (fsizeXBloc * 0.5f) + 2.0f - 4.0f;
					}
					
				}
				else if(i == 1 && _configurator.GetModule(0).GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade)
				{
					btemp = (GUI.Toggle(new Rect(rectBloc.x, rectBloc.y, rectBloc.width - fsizeXFacade, rectBloc.height), m_toggleHangs[i].bselected, "", "bloc"));
				}
				else if(i == m_toggleHangs.Count - 2 && _configurator.GetModule(m_toggleHangs.Count - 1) != null && _configurator.GetModule(m_toggleHangs.Count - 1).GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade)
				{
					btemp = (GUI.Toggle(new Rect(rectBloc.x + fsizeXFacade, rectBloc.y, rectBloc.width - fsizeXFacade, rectBloc.height), m_toggleHangs[i].bselected, "", "bloc"));
				}
			    else
				{
					btemp = (GUI.Toggle(rectBloc, m_toggleHangs[i].bselected, "", "bloc"));
				}
				
				if (btemp != m_toggleHangs[i].bselected && m_snapHangs == null && m_toggleHangs[i] != m_currentHangs)
				{
					m_toggleHangs[i].bselected = btemp;
					_UISelector = FunctionConf_Dynshelter.UISelector.none;
					_configurator.SetUISelector(FunctionConf_Dynshelter.UISelector.none);
					
					_configurator.SetSelectedIndex(i);
					_configurator.UpdateCurrentModule();
				}
				
				if(i != 0 && m_toggleHangs[i - 1].bselected &&
				 _configurator.GetModule(i - 1).GetModuleType() != FunctionConf_Dynshelter.ModuleType.facade &&
				   _configurator.GetModule(i).GetModuleType() != FunctionConf_Dynshelter.ModuleType.facade)
				{
					GetGUICadenas(rectBloc, i, fpreviousPositionXBloc, 0.0f);
				}
				
				if(m_toggleHangs[i].bselected && m_snapHangs == null)
				{                                 
					if(i != 0 && _configurator.GetModule(i - 1).GetModuleType() != FunctionConf_Dynshelter.ModuleType.facade &&
					   _configurator.GetModule(i).GetModuleType() != FunctionConf_Dynshelter.ModuleType.facade)
					{
						GetGUICadenas(rectBloc, i, fpreviousPositionXBloc, fsizeXBloc * 0.6f);
					}
					
					m_currentHangs = m_toggleHangs[i];
					
					_configurator.ActiveLimits(false);
					
					float fsizeAdd = 48.0f;
					
					if(_configurator.GetModule(i).GetModuleType() != FunctionConf_Dynshelter.ModuleType.facade)
					{
						if(_configurator.CanAddNext())
						{
							if(_UISelector != FunctionConf_Dynshelter.UISelector.addNext)
							{
								if(GUI.Button (new Rect((rectBloc.x - fsizeAdd) - (rectBloc.width * 0.25f), rectBloc.y + rectBloc.height, fsizeAdd, fsizeAdd), "", "addDS"))
								{
									_UISelector = FunctionConf_Dynshelter.UISelector.addNext;
									_configurator.SetUISelector(_UISelector);
									
									_configurator.setNextInsertion(true);
								}
							}
							else if(_UISelector == FunctionConf_Dynshelter.UISelector.addNext)
							{
								GUI.Box (new Rect((rectBloc.x - fsizeAdd) - (rectBloc.width * 0.25f), rectBloc.y + rectBloc.height, fsizeAdd, fsizeAdd), "", "addDSOn");
							}
						}
						
						if(_configurator.CanAddPrev())
						{
							if(_UISelector != FunctionConf_Dynshelter.UISelector.addPrev)
							{
								if(GUI.Button (new Rect(rectBloc.x + rectBloc.width + (rectBloc.width * 0.25f), rectBloc.y + rectBloc.height, fsizeAdd, fsizeAdd), "", "addDS"))
								{
									_UISelector = FunctionConf_Dynshelter.UISelector.addPrev;
									_configurator.SetUISelector(_UISelector);
									
									_configurator.setNextInsertion(false);
								}
							}
							else if(_UISelector == FunctionConf_Dynshelter.UISelector.addPrev)
							{
								GUI.Box (new Rect(rectBloc.x + rectBloc.width + (rectBloc.width * 0.25f), rectBloc.y + rectBloc.height, fsizeAdd, fsizeAdd), "", "addDSOn");
							}
						}
					}
					
					if(_configurator.GetNumberBloc() > 2 || _configurator.GetCurrentModule().GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade)
					{
						if(i == 0 || i == m_toggleHangs.Count - 1 ||
						   (_configurator.GetModule(i - 1) != null && _configurator.GetModule(i - 1).GetSize() == _configurator.GetModule(i).GetSize() && _configurator.GetModule(i - 1).GetModuleType() != FunctionConf_Dynshelter.ModuleType.facade) ||
						   (_configurator.GetModule(i + 1) != null && _configurator.GetModule(i + 1).GetSize() == _configurator.GetModule(i).GetSize() && _configurator.GetModule(i + 1).GetModuleType() != FunctionConf_Dynshelter.ModuleType.facade))
						{
							float fsizeDelete = 24.0f;
							float fXposition = 0.0f;//_configurator.GetModule(i).GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade ? fsizeXFacade : 0.0f;
							
							if(_configurator.GetModule(i).GetModuleType() == FunctionConf_Dynshelter.ModuleType.facade)
							{
								fXposition = fsizeXFacade;
							}
							else
							{
								fXposition = rectBloc.x + (rectBloc.width * 0.5f) - (fsizeDelete * 0.5f);
							}
							
							if(GUI.Button (new Rect(fXposition,
							                        rectLine.y - ((fsizeXBloc * 0.5f) + (_configurator.GetModule(m_toggleHangs.Count - 1).GetSize() * 5.0f) + fsizeDelete),
							                        fsizeDelete, fsizeDelete), "", "deleteBloc"))
							{
								_configurator.RemoveModule(i);	
							}
						}
					}
					
					UnselectOtherToggleShelterEditor();
				}
				
				fpreviousPositionXBloc = rectBloc.x;
			}
		}
		
		MoveToggleShetlterEditor();
		
		GUI.EndGroup();
	}
		
	public void ResetSelectionUI()
	{
		_transpModules = false;
		_resetActive = false;
	}
	
	public void ResetUI()
	{
		_UISelector = FunctionConf_Dynshelter.UISelector.none;	
	}
}
