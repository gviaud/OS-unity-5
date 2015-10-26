using UnityEngine;
using System.Collections;
using System;
using Pointcube.Global;

public class GUIMenuInteraction : MonoBehaviour,GUIInterface
{
	public Texture backGround;
	GUIItemV2 Root;
	GUIItemV2 Title;
	
	GUIItemV2 activeItm;
	
	int selM = -1;
	
	ArrayList othersMenu;
	
	//UI
	public GUISkin skin;
	public GUISkin skinIco;
	
	bool showMenu = false;
	float off7 = -300;
	Vector2 scrollpos = new Vector2(0,0);//vertical scroll menu trop grand
	Rect menuGroup;
	Rect menuSubGroup;
	int indexConf;
	int indexLock;
	
	//Interaction
	ObjInteraction interacteur;
	
	public bool isConfiguring = false;
	
	GameObject configuredObj;
	
	Touch t0;
	Touch t1;

	public bool m_Hide_Plage;

	// Fonctions-------------------------------------------
	void Start ()
	{
		
		interacteur = Camera.main.GetComponent<ObjInteraction>();
		othersMenu = new ArrayList();
		foreach(Component cp in this.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType() != this.GetType() && cp.GetType().GetInterface("GUIInterface")!= null)
			{
				othersMenu.Add(cp);
			}
		}
		
		menuGroup  = new Rect(off7,(Screen.height/2)-350/2,260,350);
		menuSubGroup = new Rect(0,0,0,0);
		
		//Interaction
		configuredObj = null;
	}
	//-----------------------------------------------------

	void Update ()
	{
		//print ("Yolo " +GetComponent<GUIMenuConfiguration> ().tempHide_Plage);
        if(PC.ctxHlp.PanelBlockingInputs())
            return;

		panelAnimation();

		if((PC.In.Click1Down() || PC.In.Click2Down()) && GetComponent<GUIMenuConfiguration> ().activeItem != null)
		{

			if( isConfiguring && (GetComponent<GUIMenuConfiguration> ().activeItem.m_text == "plage" || GetComponent<GUIMenuConfiguration> ().activeItem.m_text == "Plage"))
			{
				Debug.Log ("m_Hide_Plage " + m_Hide_Plage);
				if( m_Hide_Plage )
				{
					Debug.Log ("OKKKK ");
					GetComponent<GUIMenuConfiguration>().tempHide_Plage = false;
					GetComponent<GUIMenuConfiguration>().obj2config.GetComponent<Function_hideObject> ()._hide = true;
				}
				
			}


		}	

		if(PC.In.Click1Down() && !isOnUI() && !GetComponent<GUIMenuConfiguration>().isOnUI())
		{
			closeMenu();
		}	
	}
	
	void OnGUI()
	{
		GUISkin bkup = GUI.skin;
		GUI.skin = skin;
		if(showMenu)
		{
			GUI.skin = skinIco;
			if(GUI.Button(new Rect(Screen.width/2 - 75, 0, 150, 49),"","bouttonValide"))
			{
				Camera.main.GetComponent<ObjInteraction>().DeselectByButton();
			}
			GUI.skin = skin;
		}
//		GUI.Box(menuGroup,"MENUINTERACTION");
		
		if(Root == null)
			return;
		//MENU
		if(showMenu || off7<Screen.width)
		{
			GUI.DrawTexture(new Rect (off7,0,backGround.width-60, backGround.height), backGround);
			GUILayout.BeginArea(new Rect(off7-10, 0, 210, Screen.height));
		    GUILayout.FlexibleSpace();
		
			scrollpos = GUILayout.BeginScrollView(scrollpos,"empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
			
			GUILayout.Box("","bgFadeUp",GUILayout.Width(210),GUILayout.Height(150));//fade en haut
			GUILayout.BeginVertical("bgFadeMid",GUILayout.Width(210));
			
			GUI.skin = skinIco;
			//Title.getUI(false);
			Root.showSubItms();//Menu
			GUI.skin = skin;
			
			GUILayout.EndVertical();
			GUILayout.Box("","bgFadeDw",GUILayout.Width(210),GUILayout.Height(150));//fade en bas
			
			GUILayout.EndScrollView();
			
		    GUILayout.FlexibleSpace();
		    GUILayout.EndArea();
		}
			
			GUI.skin = bkup;
	}
	
	public void closeMenu()
	{
		if(isConfiguring)
		{
			bool tempHide_Plage = GetComponent<GUIMenuConfiguration>().tempHide_Plage;

			if (tempHide_Plage )
			{
				GetComponent<GUIMenuConfiguration>().tempHide_Plage = false;
				GetComponent<GUIMenuConfiguration>().obj2config.GetComponent<Function_hideObject> ()._hide = true;
			}


			unConfigure();
			interacteur.setSelected(null);
			showMenu = false;

		}
	}
	
//FCN. AUX. PRIVEE--------------------
	
	private void panelAnimation()
	{
		if(showMenu)
		{
			if(off7<0)
			{
				if(off7>-1)
				{
					off7 = 0;
					menuGroup.x = off7;
				}
				else
				{
					off7 = Mathf.Lerp(off7,0,5*Time.deltaTime);
					menuGroup.x = off7;
				}
			}
		}
		else
		{
			if(off7>-300)
			{
				if(off7<-209)
				{
					off7 = -300;
					menuGroup.x = off7;
					//resetMenu();
				}
				else
				{
					off7 = Mathf.Lerp(off7,-300,5*Time.deltaTime);
					menuGroup.x = off7;
				}
			}
		}
		
		
	}
	
	public void resetMenu()
	{
		selM = -1;
		if(Root.getSelectedItem()!= null)//menu
		{
			Root.resetSelected();
		}
	}
	
	private void ui2fcn()
	{
		switch (selM)
		{
		case 0: //Nouvel objet
			if(isConfiguring)
			{
				unConfigure();
			}
			ObjData od = interacteur.getSelected()
				.GetComponent<ObjData>();
			
			interacteur.setSelected(null);
			showMenu = false;
			
//			GetComponent<GUIMenuLeft>().setVisibility(true);
			GetComponent<GUIMenuLeft>().setLibToShow(od); // = setvisibility(true) + affichage de la meme lib que l'objet selected
			resetMenu();
			break;
			
		case 1: //verrouiller
			if(interacteur.lockIt())
			{
				skin.FindStyle("outilOffLock").normal.textColor = new Color(1,150f/255f,0,1);
				activeItm.chgTxt(TextManager.GetText("GUIMenuInteraction.UnLock"));
			}
			else
			{
				skin.FindStyle("outilOffLock").normal.textColor = new Color(2f/255f,37f/255f,110f/255f,1);
				activeItm.chgTxt(TextManager.GetText("GUIMenuInteraction.Lock"));
			}
			
		resetMenu();
		break;
			
		case 2: // Configuration
			isConfiguring = true;
			interacteur.setActived(false);
			GetComponent<GUIMenuConfiguration>().enabled = true;
			GetComponent<GUIMenuConfiguration>().OpenMaterialTab();
			
			GetComponent<GUIMenuLeft>().canDisplay(false);
			GetComponent<GUIMenuRight>().canDisplay(false);
			break;
		
		case 3: // swap
			if(isConfiguring)
			{
				unConfigure();
			}

			showMenu = false;
			interacteur.swap(true);
			GameObject.Find("MainScene").GetComponent<GUIMenuLeft>().startSwapping();

			resetMenu();
			break;
		
		case 4: // copy
			if(isConfiguring)
			{
				unConfigure();
			}
			if(usefullData.lowTechnologie)
			{
				if(GameObject.Find("MainNode").transform.GetChildCount() >18)
				{
					StartCoroutine(GetComponent<PleaseWaitUI>().showTempMsg("Limite d'objet atteinte",2));
					resetMenu();
					break;
				}
			}
			
			interacteur.copyObj();
			resetMenu();
			break;
			
		case 6: // delete
			if(isConfiguring)
			{
				unConfigure();
			}
			interacteur.deleteObj();
			GetComponent<GUIMenuLeft>().updateSceneObj();
			resetMenu();
			break;
			
		case 5: // reinit
			if(isConfiguring)
			{
				unConfigure();
			}
			interacteur.reinitiObj();
			resetMenu();
			break;
			
		case -1:
			if(isConfiguring)
			{
				unConfigure();
			}
			break;
		}
	}
	
	public void unConfigure()
	{
		isConfiguring = false;
		GetComponent<GUIMenuLeft>().canDisplay(true);
		GetComponent<GUIMenuRight>().canDisplay(true);
		interacteur.setActived(true);
		GetComponent<GUIMenuConfiguration>().enabled = false;
		resetMenu();
	}
	
	private void hideOthers()
	{
		foreach(Component cp in othersMenu)
		{
			((GUIInterface)cp).setVisibility(false);
		}
	}
	
	// FCN. AUX. PUBLIC------------------------------------
	public void updateGUI(GUIItemV2 itm,int val,bool reset)
	{
        if(PC.ctxHlp.PanelBlockingInputs())
        {
            resetMenu();
            return;
        }


		activeItm = itm;
		switch (itm.getDepth())
		{
			case 0:
				selM = val;
				break;
		}
		ui2fcn();
	}
	//-----------------------------------------------------
	public void setVisibility(bool b)
	{
		showMenu = b;
		//if(!b)
			//resetMenu();
		//else
		if(b)
		{
			if(usefullData._edition != usefullData.OSedition.Lite)
			{
				if(interacteur.isLocked())
				{
					skin.FindStyle("outilOffLock").normal.textColor = new Color(1,150f/255f,0,1);
					((GUIItemV2)Root.getSubItems()[indexLock]).chgTxt(TextManager.GetText("GUIMenuInteraction.UnLock"));
				}
				else
				{
					skin.FindStyle("outilOffLock").normal.textColor = new Color(2f/255f,37f/255f,110f/255f,1);
					((GUIItemV2)Root.getSubItems()[indexLock]).chgTxt(TextManager.GetText("GUIMenuInteraction.Lock"));
				}
			}
			//Title.chgTxt(interacteur.getSelected().GetComponent<ObjData>().GetObjectModel().GetDefaultText()); //Nom de lobjet dans le title
			hideOthers();
		}
	}
	//-----------------------------------------------------
	public bool isVisible()
	{
		return showMenu;
	}
	//-----------------------------------------------------
	public void CreateGui()
	{
		//UI
		Root = new GUIItemV2(-1,-1,"Root","","",this);
		
		//Title = new GUIItemV2(-1,-1,TextManager.GetText("GUIMenuInteraction.Options"),"title","title",this);
		
		Root.addSubItem(new GUIItemV2(0,2,TextManager.GetText("GUIMenuInteraction.Configure"),"configOn","configOff",this));//2
		indexConf = 0;
		Root.addSubItem(new GUIItemV2(0,0,TextManager.GetText("GUIMenuInteraction.NewObject"),"newOn","newOff",this));//0
		Root.addSubItem(new GUIItemV2(0,3,TextManager.GetText("GUIMenuInteraction.Replace"),"swapOn","swapOff",this));//3
		if(usefullData._edition != usefullData.OSedition.Lite)
			Root.addSubItem(new GUIItemV2(0,4,TextManager.GetText("GUIMenuInteraction.Copy"),"copyOn","copyOff",this));//4
		Root.addSubItem(new GUIItemV2(0,5,TextManager.GetText("GUIMenuInteraction.Refocus"),"reinitOn","reinitOff",this));//5
		if(usefullData._edition != usefullData.OSedition.Lite)
		{
			Root.addSubItem(new GUIItemV2(0,1,TextManager.GetText("GUIMenuInteraction.Lock"),"lockOn","lockOff",this));//1
			indexLock = 5;
		}
		Root.addSubItem(new GUIItemV2(0,6,TextManager.GetText("GUIMenuInteraction.Delete"),"suppressOn","suppressOff",this));//6
	}
	//-----------------------------------------------------
	public void setLockability(bool b) // used by ObjInteraction
	{
		if(usefullData._edition != usefullData.OSedition.Lite)
		{
			if(b)
				((GUIItemV2)Root.getSubItems()[indexLock]).chgTxt(TextManager.GetText("GUIMenuInteraction.UnLock"));
			else
				((GUIItemV2)Root.getSubItems()[indexLock]).chgTxt(TextManager.GetText("GUIMenuInteraction.Lock"));		
		}
	}
	//-----------------------------------------------------
	public void SetCustomizability(bool b)
	{
		((GUIItemV2)Root.getSubItems()[indexConf]).SetEnableUI(b);
	}
	//-----------------------------------------------------
	public void canDisplay(bool b)
	{
//		canDisp = b;	
	}
	//-----------------------------------------------------
	public bool isOnUI()
	{
		return PC.In.CursorOnUIs(menuGroup, menuSubGroup);
	}
	//-----------------------------------------------------
	public void SetMenuSubRect(Rect rectSubgroup)
	{
		menuSubGroup = rectSubgroup;
	}
	//-----------------------------------------------------
	public void ResetMenuSubGroup()
	{
		menuSubGroup = new Rect(0,0,0,0);
	}
	
}
