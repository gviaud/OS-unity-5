using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Pointcube.Global;

public class GUIMenuLeft : MonoBehaviour, GUIInterface 
{
	public string m_ctxPanelID; // ctx1_multiSel
	public string m_ctxPanelID_1; // ctx1_upperconfig
	public string m_ctxPanelID_2; // ctx1_upperbrand

	public Texture textureBackGround;
	public Texture textureBackGroundRightButton;

	protected OSLib _library;
	
	GUIItemV2 Root;
	//GUIItemV2 Title;
	GUIItemV2 sceneObjs;
	GUIItemV2 activeItem;
	
	GameObject originals;
	GameObject mainNode;
	
	//int selM = -1;
	int selTyp = -1;
	int selFab = -1;
	int selObj = -1;
	
	//int bkupSelM = -1;
	int	bkupSelTyp = -1;
	int	bkupSelFab = -1;
	int bkupSelObj = -1;
	
	//FastConfig
	[HideInInspector]
	public bool isFastConfig = false;
	
	//Swap
	bool isSwapping = false;
	public bool canSwap = true;
	Vector3 swappingPos;
	GameObject toSwapObj;
	
	//UI
	public GUISkin skin;
	bool showMenu = false;
	bool canDisp = false;
	float off7 = -320;
	Vector2 scrollpos = new Vector2(0,0);//vertical scroll menu trop grand
	Rect menuGroup;
	
	float deltaLateral = 0;
	float dlLimit = 100;
	
	bool canHideMenu = false;
	//bool isTwoLevelMenu = false;
	bool isJustShowing = false;
	int isJustShowingIndex = -1;
	
	bool _isSliding = false;
	
	ArrayList othersMenu;
	
	//private bool ulActive = false;
	private bool _uiLocked = false;
	//private const float ulOff7 = 150;
	
	//indique si le menu top est affiché
	private bool isUpperList = false;
	
	private ObjInteraction _interacteur;
	
	
	protected Dictionary<int, int> _firstCategory = new Dictionary<int, int> ();
	protected Dictionary<int, int> _secondCategory = new Dictionary<int, int> ();
	
	private List<GUIItemV2> listParentCategory = new List<GUIItemV2>();

//Fonctions---------------------------_
	#region unity Fcns.

    //-----------------------------------------------------
    void Awake()
    {
        UsefullEvents.OnResizingWindow  += SetRects;
        UsefullEvents.OnResizeWindowEnd += SetRects;
		UsefullEvents.LockGuiDialogBox  += LockUI;
		UsefullEvents.NewMontage        += ResetUI;
    }

    //-----------------------------------------------------
	void Start ()
	{
		originals = GameObject.Find("Originals");
		mainNode = GameObject.Find("MainNode");
		
		_interacteur = Camera.main.GetComponent<ObjInteraction>();
		
		menuGroup = new Rect();
        SetRects();

		othersMenu = new ArrayList();
		foreach(Component cp in this.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType() != this.GetType() && cp.GetType().GetInterface("GUIInterface")!= null)
			{
				othersMenu.Add(cp);
			}
		}
		
		//Ajout au Root
		Root = new GUIItemV2(-1,-1,"Root","","",this);
		//Title = new GUIItemV2(-1,-1,"Objets","title","title",this);
	}
	
	public void CreateMenu (OSLib library)
	{
		_library = library;
		Montage.om.setLibrary(library); // < temporaire, mettre cette ligne ailleur
		//Menu ajout objets
		//GUIItemV2 originalsObj = new GUIItemV2(0,0,TextManager.GetText("GUIMenuLeft.Add"),"menuAddOn","menuAddOff",this);
		int x = 0;

		foreach(OSLibCategory catLvl1 in library.GetCategoryList ())
		{
			if (catLvl1.GetCategoryList ().Count > 0)
			{
				GUIItemV2 typToAdd;
				if(catLvl1.GetText (PlayerPrefs.GetString("language")) == "OneShot 3d")
				{
					typToAdd = new GUIItemV2(0,x, catLvl1.GetText (PlayerPrefs.GetString("language")), "menuOSOn", "menuOSOff", this);
				}
				else
				{
					typToAdd = new GUIItemV2(0,x, catLvl1.GetText (PlayerPrefs.GetString("language")), "menuAddOn", "menuAddOff", this);
				}

				_firstCategory[catLvl1.GetBrandId()]=x;
				x++;
				int y = 0;
				foreach(OSLibCategory catLvl2 in catLvl1.GetCategoryList ())
				{
					_secondCategory[catLvl2.GetBrandId()]=y;
					
					GUIUpperList fabToAdd = null;
					string szparentCategory = catLvl2.GetText (PlayerPrefs.GetString("language") + "_parent");
					if(szparentCategory != "")
					{
						foreach(GUIItemV2 guiItemV2 in listParentCategory)
						{
							if(szparentCategory == guiItemV2.m_text)
							{
								fabToAdd = new GUIUpperList(1,/*guiItemV2.getSubItemsCount()*/y, catLvl2.GetText (PlayerPrefs.GetString("language")), "sousMenuOn2", "sousMenuOff2", this);
								guiItemV2.addSubItem(fabToAdd);
								break;
							}
						}
						
						if(fabToAdd == null)
						{
							GUIItemV2 parent = new GUIItemV2(1,listParentCategory.Count, szparentCategory,
							                                 "sousMenuOn", "sousMenuOff", this);
							
							fabToAdd = new GUIUpperList(1,y, catLvl2.GetText (PlayerPrefs.GetString("language")), "sousMenuOn2", "sousMenuOff2", this);
							
							parent.addSubItem(fabToAdd);
							typToAdd.addSubItem(parent);
							
							listParentCategory.Add(parent);
						}
					}
					else
					{
						fabToAdd = new GUIUpperList(1,y, catLvl2.GetText (PlayerPrefs.GetString("language")), "sousMenuOn2", "sousMenuOff2", this);
						typToAdd.addSubItem(fabToAdd);
					}

					y++;
									
					int size = catLvl2.GetObjectList ().Count;
					int numberPerPage = 24;
					//Screen.currentResolution;

					#if UNITY_IPHONE
						numberPerPage = 9;
					#else

					if(Screen.currentResolution.width/Screen.currentResolution.height == 1.25f)
						numberPerPage = 25;
					else if(Screen.currentResolution.width/Screen.currentResolution.height == 1.6f)
						numberPerPage = 24;
					else
						numberPerPage = 24;
					#endif

					if( size % numberPerPage != 0)
					{
						size = (catLvl2.GetObjectList ().Count/numberPerPage)*numberPerPage + numberPerPage;
					}

					Texture2D[] tmpTexs = new Texture2D[size];
					string[] tmpTexts = new string[size];
					bool[] tmpCustomizables = new bool[size];
					bool[] tmpBrands = new bool[size];

					int i = 0;      
					foreach(OSLibObject obj in catLvl2.GetObjectList ())
					{
						// load texture
						Texture2D tmp = obj.GetThumbnail ();//Resources.Load (obj.GetThumbnailPath ());
						if(tmp == null)
							tmp =  Resources.Load ("thumbnails/noThumbs", typeof(Texture2D)) as Texture2D;
						if(obj==null)
							Debug.Log("Obj null");
						if (obj.GetDefaultText ()==null)
							Debug.Log("Obj null");
//						tmp.name = obj.GetDefaultText ();
						tmpTexts[i] = obj.GetText(PlayerPrefs.GetString("language"));
						tmpTexs[i] = tmp;
						tmpCustomizables[i] = obj.GetModules().GetStandardModuleList().Count > 0;
						tmpBrands[i] = obj.IsBrandObject();
						//print (tmpTexts[i]);

						++i;
	
					}
	
					fabToAdd.m_ctxPanelConfig = m_ctxPanelID_1;
					fabToAdd.m_ctxPanelBrand = m_ctxPanelID_2;
					fabToAdd.setImgContent(tmpTexs,tmpTexts, tmpCustomizables, tmpBrands);

				}
				
				Root.addSubItem(typToAdd);
				
			}
			
			if (catLvl1.GetObjectList ().Count > 0)
			{
				GUIUpperList fabToAdd = new GUIUpperList(0,x, catLvl1.GetDefaultText (), "sousMenuOn", "sousMenuOff", this);
				x++;
				
				Texture2D[] tmpTexs = new Texture2D[catLvl1.GetObjectList ().Count];
				int i = 0;
				foreach(OSLibObject obj in catLvl1.GetObjectList ())
				{
					// load texture
					Texture2D tmp = obj.GetThumbnail ();
					if(tmp == null)
						tmp =  Resources.Load ("thumbnails/noThumbs",typeof(Texture2D)) as Texture2D;
					
					tmp.name = obj.GetText(PlayerPrefs.GetString("language"));// obj.GetDefaultText ();
					tmpTexs[i] = (Texture2D)tmp;
			
					++i;
				}
				
				fabToAdd.setImgContent(tmpTexs);
				Root.addSubItem(fabToAdd);
	
			}

		}
		
		//Root.addSubItem(originalsObj);
		//print (x);
		if(usefullData._edition != usefullData.OSedition.Lite)
		{
			//Menu objets present dans la scene (sauf en édition lite)
			sceneObjs = new GUIItemV2(0,++x,TextManager.GetText("GUIMenuLeft.Select"),"menuSceneOn","menuSceneOff",this);
			Root.addSubItem(sceneObjs);
		}
		
		sceneObjs.SetEnableUI(GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().getNumberObjects() > 0);
		
		ResetMenu();
	}
	
	void Update ()
	{
        if(PC.ctxHlp.PanelBlockingInputs())
            return;

		panelAnimation();
		
		if(showMenu)//version IPAD
		{
			antiSlideAndSelect();
//			if(Input.touchCount>0 && !isOnUI() && !GetComponent<GUIMenuLeft>().isOnUI())
//			{
//				switch (Input.touches[0].phase)
//				{
//				case TouchPhase.Began:
//					canHideMenu = true;
//					break;
//				case TouchPhase.Moved:
//					canHideMenu = false;
//					break;
//				case TouchPhase.Ended:
//					if(canHideMenu)
//					{
//						showMenu = false;
//						if(isJustShowing)
//						{
//							isJustShowing = false;
//							_interacteur.setSelected(mainNode.transform.GetChild(isJustShowingIndex).gameObject);
//							isJustShowingIndex = -1;
//							resetSceneMenu();
//						}
//						if(isSwapping)
//						{
//							endSwapping();	
//						}
//						canHideMenu = false;
//						//saveMenuState();
//						//resetMenu();
//						returnToLvlOne();
//						resetSceneMenu();
//					}
//					break;
//				}	
//			}		
//			if(Application.platform != RuntimePlatform.IPhonePlayer)
//			{
			if(PC.In.Click1Down() && !isOnUI() && !isJustShowing)
			{
				closeMenu();
			}
			
			if(PC.In.Click1Up() && !isOnUI() && isJustShowing)
			{
				isJustShowing = false;
				_interacteur.setSelected(mainNode.transform.GetChild(isJustShowingIndex).gameObject);
				isJustShowingIndex = -1;
				resetSceneMenu();
			}
//			}
		}	
		
		if(sceneObjs != null)
		{
			sceneObjs.SetEnableUI(GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().getNumberObjects() > 0);
		}
		
		//Slide menus
        float deltaScroll;
        if(PC.In.CursorOnUI(menuGroup) && PC.In.ScrollViewV(out deltaScroll))
            scrollpos.y += deltaScroll;
	}
	
	void OnGUI()
	{

//        if(GUI.Button(new Rect(50f, 50f, 50f, 50f), "Switch"))
//            PC.In = // TODO

		GUISkin bkup = GUI.skin;
		GUI.skin = skin;

		if(showMenu)
			GUI.DrawTexture(new Rect ( off7 -30, Screen.height - textureBackGround.height,textureBackGround.width-50,textureBackGround.height),textureBackGround);
		//GUI.Box(menuGroup,"MENULEFT");
		
		//MENU
		if(showMenu || off7<Screen.width)
		{
			GUILayout.BeginArea(new Rect(off7-30, 0/*(ulActive? ulOff7:0)*/, 260, Screen.height/*-(ulActive? ulOff7:0)*/));
		    GUILayout.FlexibleSpace();
		
			scrollpos = GUILayout.BeginScrollView(scrollpos,"empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
			
			GUILayout.Box("","bgFadeUp",GUILayout.Width(260),GUILayout.Height(150));//fade en haut
			GUILayout.BeginVertical("bgFadeMid",GUILayout.Width(260));
			
			//Title.getUI(false);
			Root.showSubItms();//Menu
			
			GUILayout.EndVertical();
			GUILayout.Box("","bgFadeDw",GUILayout.Width(260),GUILayout.Height(150));//fade en bas
			
			GUILayout.EndScrollView();
			
		    GUILayout.FlexibleSpace();
		    GUILayout.EndArea();
			
			if(activeItem != null && showMenu)
			{
				if(activeItem.GetType() == typeof(GUIUpperList))
				{
					((GUIUpperList)activeItem).display();	
				}
			}
		}
			
		//MenuToggle
		if(showMenu)
		{
			//BTN SHOW HIDE MENU
//			GUI.Box (new Rect(off7-30,Screen.height/2-(55/2),30,55),"","halfMenuToggle");
//			showMenu = GUI.Toggle(new Rect(off7-30,Screen.height/2-(55/2),30,55),showMenu,"","halfMenuToggle");
			showMenu = GUI.Toggle(new Rect(off7-30,Screen.height/2-350/2,0,350),showMenu,"","halfMenuToggle");
			
			if(showMenu == false)	// Super pouce vert pour cette condition imbriquée
			{
				if(isJustShowing)
				{
					isJustShowing = false;
					_interacteur.setSelected(mainNode.transform.GetChild(isJustShowingIndex).gameObject);
					isJustShowingIndex = -1;
					resetSceneMenu();
				}
				if(isSwapping)
				{
					endSwapping();	
				}
				returnToLvlOne();
				resetSceneMenu();
				
			}
			
//			//BTN '?'
//			if(GUI.Button(new Rect(5,Screen.height/2-150,100,100),"","infos"))
//			{
//				
//			}
//			
//			//BTN NEXT PAGE
//			if(GUI.Button(new Rect(5,Screen.height/2-250,100,100),"","pageNext"))
//			{
//				if(activeItem.GetType() == typeof(GUIGridV2))
//				{
//					((GUIGridV2)activeItem).changePage(1);
//				}
//			}
//			
//			//BTN PREVIOUS PAGE
//			if(GUI.Button(new Rect(5,Screen.height/2-350,100,100),"","pagePrev"))
//			{
//				if(activeItem.GetType() == typeof(GUIGridV2))
//				{
//					((GUIGridV2)activeItem).changePage(-1);
//				}
//			}
		}
		else
		{
			if(canDisp)
			{
				//BTN SHOW HIDE MENU
				GUI.DrawTexture(new Rect (0, 0,textureBackGroundRightButton.width-25, textureBackGroundRightButton.height ), textureBackGroundRightButton);
				bool tmpShw = GUI.Toggle(new Rect(0-45,Screen.height/2-50,100,100),showMenu,"","menuToggle");
				if(showMenu != tmpShw && !_uiLocked)
				{
					showMenu = tmpShw;
					hideOthers();
					_interacteur.setSelected(null);
//					setMenuState();
				}
			}
		}
		
		if(isFastConfig && Camera.main.GetComponent<ObjInteraction>().getSelected())
		{
			if(Camera.main.GetComponent<ObjInteraction>().getSelected().GetComponent<ObjData>().IsCustomizable())
			{
				GetComponent<GUIMenuInteraction>().isConfiguring = true;
				Camera.main.GetComponent<ObjInteraction>().setActived(false);
				
				GetComponent<GUIMenuConfiguration>().enabled = true;
				GetComponent<GUIMenuConfiguration>().setVisibility(true);
				GetComponent<GUIMenuConfiguration>().OpenMaterialTab();
				
				canDisplay(false);
				GetComponent<GUIMenuRight>().canDisplay(false);
			}
			else
			{
				print ("no customizable");
			}
			
			isFastConfig = false;
		}
		
		GUI.skin = bkup;
	}

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= SetRects;
        UsefullEvents.OnResizeWindowEnd -= SetRects;
		UsefullEvents.LockGuiDialogBox  -= LockUI;
		UsefullEvents.NewMontage        -= ResetUI;
    }
    #endregion
//FCN. AUX. PRIVEE--------------------

    private void SetRects()
    {
        menuGroup.Set(off7, 0, 320, Screen.height);
    }

	private void antiSlideAndSelect()
	{
        if(PC.In.Drag1())
            _isSliding = true;
        else
            _isSliding = false;

//		if(Input.touchCount>0)
//		{
//			Touch t = Input.touches[0];
//			if(t.phase == TouchPhase.Moved)
//			{
//				_isSliding = true;
//			}
//			else if(t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended)
//			{
//				_isSliding = false;
//			}		
//		}
//		else if(_isSliding == true)
//		{
//			_isSliding = false;
//		}
	}
	
	public void closeMenu()
	{
		showMenu = false;
		if(isSwapping)
			endSwapping();
		
		canHideMenu = false;
		returnToLvlOne();
		resetSceneMenu();
	}
	
	private void panelAnimation()
	{
		if(showMenu)
		{
			if(off7<30)
			{
				if(off7>29)
				{
					off7 = 30;
					menuGroup.x = off7-30;
				}
				else
				{
					off7 = Mathf.Lerp(off7,30,5*Time.deltaTime);
					menuGroup.x = off7-30;
				}
			}
		}
		else
		{
			if(off7>-320)
			{
				if(off7<-319)
				{
					off7 = -320;
					menuGroup.x = off7;
					//resetMenu();
				}
				else
				{
					off7 = Mathf.Lerp(off7,-320,5*Time.deltaTime);
					menuGroup.x = off7;
				}
			}
		}
		

	}
	
	public void setBkupStateMenu(int[] _stateMenu)
	{
		bkupSelFab = _stateMenu[0];
		bkupSelTyp = _stateMenu[1];
		bkupSelObj = _stateMenu[2];
	}
	
	public void setMenuState()
	{
		selFab = bkupSelFab;
		selTyp = bkupSelTyp;
		selObj = bkupSelObj;
		
		if(Root.getSubItemsCount()>0 && selFab != -1)
		{
			Root.setSelected(selFab);
			if(Root.getSelectedItem() != null && 
			Root.getSelectedItem().getSubItemsCount()>0 && selTyp != -1)
			{
				Root.getSelectedItem().setSelected(selTyp);
				
				if(Root.getSelectedItem().getSelectedItem() != null)
				{
					if(Root.getSelectedItem().getSelectedItem().GetType() == typeof(GUIUpperList))
					{
						activeItem = Root.getSelectedItem().getSelectedItem();
					}
					else
					{
						Root.getSelectedItem().getSelectedItem().setSelected(selObj);

						if(Root.getSelectedItem().getSelectedItem().getSelectedItem() != null &&
						   Root.getSelectedItem().getSelectedItem().getSelectedItem().GetType() == typeof(GUIUpperList))
						{
							activeItem = Root.getSelectedItem().getSelectedItem().getSelectedItem();
						
						}
					}
				}
			}
		}
	}
	
	private void hideOthers()
	{
		foreach(Component cp in othersMenu)
		{
			((GUIInterface)cp).setVisibility(false);
		}
	}
	
	private void ui2Fcns()
	{
		if(selFab != -1 && selFab <= (Root.getSubItemsCount() - 1))
		{
			OSLibCategory catLvl1 = _library.GetCategoryList ()[selFab];

			OSLibObject obj;
			if(selTyp != -1)
			{
				//t = originals.transform.GetChild(selTyp).GetChild(selFab).GetChild(selObj);
				OSLibCategory catLvl2 = catLvl1.GetCategoryList () [selTyp];
				
				if(selObj != -1 && canSwap)
				{
					//t = originals.transform.GetChild(selTyp).GetChild(selObj);
					if( selObj < catLvl2.GetObjectList ().Count)
					{

						obj = catLvl2.GetObjectList () [selObj];

						if(isSwapping)
						{
							setToSwapObj();
							if(canSwap){
								canSwap = false;
								StartCoroutine (mainNode.GetComponent<ObjInstanciation>().swap(swappingPos, obj, toSwapObj, selFab, selTyp, selObj));
								resetSceneMenu();
								ResetMenu();
							}
						}
						else
						{
							showMenu = false;
							
							if(CanAddObj(obj))
							{
								mainNode.GetComponent<ObjInstanciation>().addObj(obj, selFab, selTyp, selObj);
								resetSceneMenu();
							}
							ResetMenu();
						}

					}
					selObj = -1;
				}
			}
		}
		else if(selFab == Root.getSubItemsCount())		// Selection d'un obj deja dans la scene
		{
			if(isSwapping)
			{
                endSwapping();
			}
			
			if(selTyp != -1)
            {
                if(selTyp == 0) // Bouton "Selectionner tout"
				{
                    _interacteur.SelectAll();
					resetSceneMenu();
				}
				else if(selTyp == 1) // bouton selection multiple
				{
                    PC.ctxHlp.ShowCtxHelp(m_ctxPanelID);
					_interacteur.SelectMultiple();
					resetSceneMenu();
				}
				else if(selTyp == 2 && selObj != -1)// if(selTyp < mainNode.transform.GetChildCount()) // On ne selectionne pas l'indice 0 car c'est l'avatar
				{
					isJustShowing = true;
					isJustShowingIndex = selObj+1;//selTyp-1;
					_interacteur.setSelected(mainNode.transform.GetChild(selObj+1).gameObject,true);
					selObj = -1;
				}
            }
			else
			{
				isJustShowing = false;
				isJustShowingIndex = -1;
				_interacteur.setSelected(null);	
			}
		}
	}
	
	private void resetSceneMenu()
	{
		if(selFab != 0)
		{
			selTyp = -1;
			selObj = -1;
			selFab = -1;
			if(Root.getSelectedItem()!= null)//menu
			{
				if(Root.getSelectedItem().getSelectedItem()!=null)//sous menu
				{
					Root.getSelectedItem().resetSelected();
				}
				Root.resetSelected();
			}
		}
		//ulActive = false;
	}
	
	private void LockUI(bool isLck)
	{
		_uiLocked = isLck;
	}
	
	private void ResetUI()
	{
		if(usefullData._edition != usefullData.OSedition.Lite)
		{
			sceneObjs.setSelected(2);
			if(sceneObjs.getSelectedItem() != null)
				sceneObjs.getSelectedItem().setItems(new ArrayList());
		}
	}
	
//FCN. AUX. PUBLIC--------------------

	public void updateGUI(GUIItemV2 itm,int val,bool reset)
	{
		if(PC.ctxHlp.PanelBlockingInputs())
			return;
			
		if(reset)
			activeItem = itm;
		if(val == -1)
			activeItem = null;
		
		switch (itm.getDepth())
		{
			case 0:
			
				selFab = val;
				
				if(reset)
				{
					selTyp = -1;
					selObj = -1;
					if(itm.getSelectedItem()!=null && itm.getSelectedItem().getSelectedItem()!=null)
						itm.getSelectedItem().resetSelected();
					if(itm.getSelectedItem()!=null)
						itm.resetSelected();
				}
				break;
				
		case 1:
		
				foreach(GUIItemV2 guiItemV2 in listParentCategory)
				{
					if(guiItemV2.m_text == itm.m_text)
					{
						break; //Skip parent 
					}
				}
				
				selTyp = val;
				
				if(reset)
				{
					selObj = -1;
					if(itm.getSelectedItem()!=null)
						itm.resetSelected();
				}
				
				break;
				
			case 2:
				
				selObj = val;
				
				break;
		}
		
//		if(Input.touchCount>0)//anti slide and select a reefaire
//			if(Input.touches[0].phase == TouchPhase.Moved)
//				return;
		if(!PC.In.Drag1())
			ui2Fcns();
		
		/*ulActive = false;
		if(activeItem != null)
		{
			if(activeItem.GetType() == typeof(GUIUpperList))
			{
				ulActive = true;			
				if(isUpperList==false)	
					scrollpos.y+=ulOff7;
		
				isUpperList = true;
			//	ulOff7 = 0.0f;
			}
			else
			{
					isUpperList = false;
			//		ulOff7 = 150.0f;
			}
		}*/
		
		//int[] indexs = new int[4];
		//indexs[0] = selM;
		//indexs[1] = selFab;
		//indexs[2] = selTyp;
		//indexs[3] = selObj;
		//UsefullEvents.FireUpdateUIState(GetType().ToString(),indexs);
		//Debug.Log("m>"+selM+"<fab>"+selFab+"<typ>"+selTyp+"<obj>"+selObj);
	}
	
	public void setVisibility(bool b)
	{
		showMenu = b;
		if(!b)
		{
			//saveMenuState();
			resetSceneMenu();
			if(isSwapping)
			{
				endSwapping();
			}
			//resetMenu();
			returnToLvlOne();
		}
//		else
		//			setMenuState();
		
		ResetMenu();
	}
	
	public void returnToLvlOne()
	{
		/*if(selM == 0)//Retour au niveau 1
		{*/
			if(Root.getSelectedItem()!= null)//menu
			{
				if(Root.getSelectedItem().getSelectedItem()!=null)//sous menu
				{
					if(Root.getSelectedItem().getSelectedItem().getSelectedItem()!=null)
					{
						Root.getSelectedItem().getSelectedItem().getSelectedItem().resetSelected();
						activeItem = null;
					}
					
					Root.getSelectedItem().getSelectedItem().resetSelected();
				}
			}
		//}	
		
		if(activeItem != null && activeItem.GetType() == typeof(GUIUpperList))
		{
			activeItem = null;
		}
	}
	
	public bool isVisible()
	{
		return showMenu;
	}
	
	public void canDisplay(bool b)
	{
		canDisp = b;	
	}
	
	public bool isDisplay()
	{
		return canDisp;
	}
	
	public void CreateGui()
	{
		//rien ici	
	}
	
	public bool isOnUI()
	{
		if(showMenu)
        {
            if(activeItem != null && activeItem.GetType() == typeof(GUIUpperList))
            {
				GUIUpperList guiUL = activeItem as GUIUpperList;
				
				if(guiUL != null)
				{
					return PC.In.CursorOnUIs(menuGroup, guiUL.bgThumbRect);
				}
			}
            else
			{
			    return PC.In.CursorOnUIs(menuGroup);
			}
        }
		
		return PC.In.CursorOnUI(new Rect(0,Screen.height/2-50,100,100));
	}
	
	//-----------------------------------------------------
	public void updateSceneObj()
	{
		if(usefullData._edition == usefullData.OSedition.Lite)	// Do nothing in lite edition (no "scene objects" menu)
			return;
			
		ArrayList tmpList = new ArrayList();
        string s;
		GUIItemV2 uniques = new GUIItemV2(1,2, TextManager.GetText("GUIMenuLeft.Unique"), "sousMenuOn", "sousMenuOff", this); //onglet regroupant la liste des objets
		
        if(mainNode.transform.GetChildCount() > 0)
		{
            tmpList.Add(new GUIItemV2(1,0, TextManager.GetText("GUIMenuLeft.All"), "outilOn", "outilOff", this)); // Pour deplacement de toute la scene
			tmpList.Add(new GUIItemV2(1,1, TextManager.GetText("GUIMenuLeft.Several"), "outilOn", "outilOff", this)); // Pour activer la selection par group
			tmpList.Add(uniques);
		}
		int i=0;
		foreach(Transform t in mainNode.transform)
		{
			if(t.name != "_avatar")
			{
//				s = t.name.Replace("(Clone)","");
				if(t.GetComponent<ObjData>().GetObjectModel() != null)
				{
					s = t.GetComponent<ObjData>().GetObjectModel().getCategory().GetText(PlayerPrefs.GetString("language"));
					s = s +" - "+ t.GetComponent<ObjData>().GetObjectModel().GetText(PlayerPrefs.GetString("language"));
					/*tmpList*/uniques.addSubItem(new GUIItemV2(/*1*/2,i,s,"outilOn","outilOff",this));
					i++;
				}
				else
				{
					print ("error !!!");
				}
			}
		}
		
		sceneObjs.setItems(tmpList);
	}
	//-----------------------------------------------------
	public void setToSwapObj()
	{
		toSwapObj = _interacteur.getSelected();
	}
	
	public int[] getStateMenu()
	{
		int[] stateMenu = new int[3];
		stateMenu[0] = selFab;
		stateMenu[1] = selTyp;
		stateMenu[2] = selObj;
		
		return stateMenu;
	}
	
	//-----------------------------------------------------
	public void startSwapping()
	{
		_interacteur.setActived(false);
		toSwapObj = _interacteur.getSelected();
		
		//Determination de la lib a afficher
		//int type=0,brand=0;
		
		ObjData _objectData = toSwapObj.GetComponent<ObjData>();
		
		bkupSelTyp = _objectData.selTyp;
		bkupSelFab = _objectData.selFab;
		bkupSelObj = _objectData.selObj;
		
		/*OSLibObject _objectModel= toSwapObj.GetComponent<ObjData>().GetObjectModel();
		//Sauvergarde Type et marque de l'objet
		if(_objectModel.getCategory().GetParentCategory() == null) // que un type
		{
			brand = -1;
			int typeID = _objectModel.getCategory().GetId();
			foreach(OSLibCategory cat in _library.GetCategoryList())
			{
				if(cat.GetId() == typeID)
					break;
				else
					type ++;
			}
			
			//bkupSelM = 0;
			bkupSelTyp = type;
			bkupSelFab = -1;
			
			print ("no parent category");
		}
		else // type plus marque
		{
			int brandID = _objectModel.getCategory().GetId();
			int newbrandID = _objectModel.getCategory().GetBrandId();
			int typeID = _objectModel.getCategory().GetParentCategory().GetId();
			int newtypeID = _objectModel.getCategory().GetParentCategory().GetBrandId();
			
		//	if(newbrandID==-1)
		//		newbrandID=brandID;
			
		//	if(newtypeID==-1)
		//		newtypeID=typeID;
			
			foreach(OSLibCategory cat in _library.GetCategoryList())
			{
				if(newtypeID!=-1)
				{
					if(cat.GetBrandId() == newtypeID)
					{
						if(_library.GetCategoryList().Count>type)
						{
							foreach(OSLibCategory cat2 in _library.GetCategoryList()[type].GetCategoryList())
							{
								if(newbrandID!=-1)
								{
									print ("newbrandID1");
									if(cat2.GetBrandId() == newbrandID)
										break;
									else
										brand ++;
								}
								else
								{
									print ("no newbrandID1");
									if(cat2.GetId() == brandID)
										break;
									else
										brand ++;									
								}
							}
							break;
						}
					}
					else
						type ++;
				}
				else
				{
					if(cat.GetId() == typeID)
					{
						if(_library.GetCategoryList().Count>type)
						{
							foreach(OSLibCategory cat2 in _library.GetCategoryList()[type].GetCategoryList())
							{
								if(newbrandID!=-1)
								{
									print ("cat2 newbrandID");
									if(cat2.GetBrandId() == newbrandID)
										break;
									else
										brand ++;
								}
								else
								{
									print ("cat2 no newbrandID");
									if(cat2.GetId() == brandID)
										break;
									else
										brand ++;									
								}
							}
							break;
						}
					}
					else
						type ++;
				}
			}			

			//bkupSelM = 0;
			bkupSelTyp = brand;
			bkupSelFab = type;
			//isTwoLevelMenu = true;
			print ("parent category");
			
		}*/
		setMenuState();
		
		
		isSwapping = true;
		if(toSwapObj!=null)
			swappingPos = toSwapObj.transform.position;
		showMenu = true;		
	}
	//-----------------------------------------------------
	public void endSwapping()
	{
		updateSceneObj();
		isSwapping = false;
		_interacteur.swap(false);
		_interacteur.setActived(true);
	}
	//-----------------------------------------------------
	public void ResetMenu()
	{
		//selM = -1;
		selTyp = -1;
		selFab = -1;
		selObj = -1;
		
		if(Root.getSelectedItem()!= null)//menu
		{
			if(Root.getSelectedItem().getSelectedItem()!=null)//sous menu
			{
				if(Root.getSelectedItem().getSelectedItem().getSelectedItem() != null)//tool
				{
					if(Root.getSelectedItem().getSelectedItem().getSelectedItem().getSelectedItem() != null)//tool
					{
						Root.getSelectedItem().getSelectedItem().getSelectedItem().resetSelected();
					}
					Root.getSelectedItem().getSelectedItem().resetSelected();
				}
				Root.getSelectedItem().resetSelected();
			}
			Root.resetSelected();
		}
		
		// -- Menu objets toujours ouvert en édition lite --
		if(usefullData._edition == usefullData.OSedition.Lite)
		{
			selFab = 0;
			Root.setSelected(0);
		}
		
		activeItem = null;
	}
	//-----------------------------------------------------
	public void setLibToShow(ObjData od)
	{
		//Determination de la lib a afficher
		//int type=0,brand=0;
		
		//ObjData _objectData = interacteur.getSelected().GetComponent<ObjData>();
		
		bkupSelTyp = od.selTyp;
		bkupSelFab = od.selFab;
		bkupSelObj = od.selObj;
		
		//Sauvergarde Type et marque de l'objet
		/*if(oslo.getCategory().GetParentCategory() == null) // que un type
		{
			brand = -1;
			int typeID = oslo.getCategory().GetId();
			foreach(OSLibCategory cat in _library.GetCategoryList())
			{
				if(cat.GetId() == typeID)
					break;
				else
					type ++;
			}
			
			//bkupSelM = 0;
			bkupSelTyp = type;
			bkupSelFab = -1;
		}
		else // type plus marque
		{
			int brandID = oslo.getCategory().GetId();
			int typeID = oslo.getCategory().GetParentCategory().GetId();
			
			bkupSelTyp = _secondCategory[oslo.getCategory().GetBrandId()];
			bkupSelFab = _firstCategory[oslo.getCategory().GetParentCategory().GetBrandId()];
			//bkupSelM = 0;
		//	bkupSelTyp = type;
		//	bkupSelFab = brand;
			
		}*/
		setMenuState();
		showMenu = true;
		//ulActive = true;
	}
	//-----------------------------------------------------
	private bool CanAddObj(OSLibObject oslo)
	{
		bool b = true;
		
		if(usefullData.lowTechnologie)
		{
			if(mainNode.transform.GetChildCount() >18)
			{
				StartCoroutine(GetComponent<PleaseWaitUI>().showTempMsg(TextManager.GetText("GUIMenuLeft.WarningiPhone2Objects"),2));
				b = false;
			}
		}
		
		return b;
	}
	//-----------------------------------------------------
	/*public void SetState(int selection)
	{
		ResetMenu();
		selM = selection;
		Root.setSelected(selM);
		selObj = -1;
		updateGUI(Root.getSelectedItem(),selM,false);
	}*/

} // class GUIMenuLeft
