//using UnityEngine;
//using System.Collections;
//
//public class GUItesvFromLab : MonoBehaviour,GUIInterface{
//	
//	public GUISkin skin;
//	
////	GUIItemV2 SelectedMenu;
////	GUIItemV2 SelectedSubMenu;
////	GUIItemV2 SelectedTool;
//	GUIItemV2 Root;
//	
//	GUIItemV2 activeItem;
//	
//	float tgtPos;
//	float tgtHeight;
//	
//	int selM = -1;
//	int selSM = -1;
//	int selT = -1;
//	
//	Rect menuGroup;
//	
//	//UI
//	bool showMenu = false;
//	float off7 = -300;
//	
//	Vector2 scrollpos = new Vector2(0,0);
//	
//	GameObject cube;
//	
//	// Use this for initialization
//	void Start ()
//	{			
//		
//		cube = GameObject.Find("Cube");
//		
//		Root = new GUIItemV2(-1,"Root","bgOn","bgOff",this);
//		
//		for(int i=0;i<4;i++)
//		{
//			Root.addSubItem(new GUIItemV2(0,("Menu "+i),"bgOn","bgOff",this));
//		}
//		
//		int z = 1;
//		foreach(GUIItemV2 menu in Root.getSubItems())
//		{
//			if(z == 1)
//			{
//				GUIGridV2 grid = new GUIGridV2(1,("Sous Menu Grid"),"bgOn2","bgOff2",this);
//				Object[] tmp = Resources.LoadAll("thumbs",typeof(Texture2D));
//				Texture2D[] texs = new Texture2D[tmp.Length];
//				for(int i=0;i<tmp.Length;i++)
//				{
//					texs[i]=(Texture2D)tmp[i];
//				}
//				grid.setImgContent(texs);
//				menu.addSubItem(grid);
//			}
//			else
//			{
//				for(int i=0;i<4;i++)
//				{
//					menu.addSubItem(new GUIItemV2(1,("Sous Menu "+i),"bgOn2","bgOff2",this));
//				}
//				
//				foreach(GUIItemV2 sousMenu in menu.getSubItems())
//				{
//					for(int i=0;i<4;i++)
//					{
//						sousMenu.addSubItem(new GUIItemV2(2,("Outil n° "+i),"bgOn","bgOff",this));
//					}
//				}
//			}
//			z++;
//		}
//		
////		float h = 300+Root.getSubItemsCount()*50;
////		menuGroup = new Rect(0,(Screen.height/2)-(h/2),260,300+Root.getSubItemsCount()*50);
////		
////		tgtPos = (Screen.height/2)-(h/2);
////		tgtHeight = Root.getSubItemsCount()*50;
////		
////		updateGroupPos();
//	}
//	
//	// Update is called once per frame
//	void Update ()
//	{
//////		Debug.Log("MENU "+selM+" - SOUSMENU "+selSM+" - TOOL "+selT);
////		if(tgtPos != menuGroup.y)
////		{
//////			menuGroup.y = Mathf.Lerp(menuGroup.y,tgtPos,5*Time.deltaTime);
////			menuGroup.y = tgtPos;
////		}
////		if(tgtHeight != menuGroup.height)
////		{
//////			menuGroup.height = Mathf.Lerp(menuGroup.height,tgtHeight,5*Time.deltaTime);
////			menuGroup.height = tgtHeight;
////		}
////		if(SelectedTool != null)
////			Debug.Log(SelectedTool.GetType());
//		
//		//animation latérale menu
//		if(showMenu)
//		{
//			if(off7<110)
//			{
//				if(off7>109)
//					off7 = 110;
//				else
//				{
//					off7 = Mathf.Lerp(off7,110,5*Time.deltaTime);
//				}
//			}
//		}
//		else
//		{
//			if(off7>-300)
//			{
//				if(off7<-209)
//				{
//					off7 = -300;
//					resetMenu();
//				}
//				else
//				{
//					off7 = Mathf.Lerp(off7,-300,5*Time.deltaTime);
//				}
//			}
//		}
//		
//		doTheJob();
//		
//	}
//	
//	void OnGUI()
//	{
////		GUI.skin = skin;
////		float pos = 150;
////		
////		GUI.BeginGroup(menuGroup);
////		GUI.Box(new Rect(0,0,260,150),"","up");
////		GUI.Box(new Rect(0,150,260,menuGroup.height-300),"","bg");
////		foreach(GUIItemV2 menu in Root.getSubItems())
////		{
////			if(menu == SelectedMenu)
////			{
////				if(menu.getUI(pos,true))
////				{
////					SelectedMenu = null;
////					selM = -1;
////					SelectedSubMenu = null;
////					selSM = -1;
////					SelectedTool = null;
////					selT = -1;
////					updateGroupPos();
////				}
////				pos = menu.updateY(pos);
////				
////				if(SelectedMenu != null)
////				{
////					foreach(GUIItemV2 subMenu in SelectedMenu.getSubItems())
////					{
////						if(subMenu == SelectedSubMenu)
////						{
////							if(subMenu.getUI(pos,true))
////							{
////								SelectedSubMenu = null;
////								selSM = -1;
////								SelectedTool = null;
////								selT = -1;
////								updateGroupPos();
////							}
////							pos = subMenu.updateY(pos);
////							
////							if(SelectedSubMenu != null)
////								foreach(GUIItemV2 tool in SelectedSubMenu.getSubItems())
////								{
////									if(tool == SelectedTool)
////									{
////										if(tool.getUI(pos,true))
////										{
////											SelectedTool = null;
////											selT = -1;
////										}
////										pos = tool.updateY(pos);
////									}
////									else
////									{
////										if(tool.getUI(pos,false))
////										{
////											SelectedTool = tool;
////											selT = SelectedSubMenu.getSubItemIndex(tool);
////										}
////										pos = tool.updateY(pos);
////									}
////								}
////						}
////						else
////						{
////							if(subMenu.getUI(pos,false))
////							{
////								SelectedSubMenu = subMenu;
////								selSM = SelectedMenu.getSubItemIndex(subMenu);
////								updateGroupPos();
////							}
////							pos = subMenu.updateY(pos);
////						}
////					}
////				}
////				
////			}
////			else
////			{
////				if(menu.getUI(pos,false))
////				{
////					SelectedMenu = menu;
////					selM = Root.getSubItemIndex(menu);
////					updateGroupPos();
////				}
////				pos = menu.updateY(pos);;
////			}
////		}
////		GUI.Box(new Rect(0,pos,260,150),"","dw");
////		GUI.EndGroup();
//		
//		GUI.skin = skin;
//				
//		showMenu = GUI.Toggle(new Rect(5,Screen.height/2-50,100,100),showMenu,"","menuToggle");
//		
//		if(GUI.Button(new Rect(5,Screen.height/2-150,100,100),"","infos"))
//		{
//			
//		}
//		
//		if(GUI.Button(new Rect(5,Screen.height/2-250,100,100),">"))
//		{
//			if(activeItem.GetType() == typeof(GUIGridV2))
//			{
//				((GUIGridV2)activeItem).changePage(1);
//			}
//		}
//		
//		if(GUI.Button(new Rect(5,Screen.height/2-350,100,100),"<"))
//		{
//			if(activeItem.GetType() == typeof(GUIGridV2))
//			{
//				((GUIGridV2)activeItem).changePage(-1);
//			}
//		}
//		
//		if(showMenu || off7 > -290)
//		{
//			GUILayout.BeginArea(new Rect(off7, 0, 300, Screen.height));
//		    GUILayout.FlexibleSpace();
//		
//			scrollpos = GUILayout.BeginScrollView(scrollpos,GUILayout.Width(300));//scrollView en cas de menu trop grand
//			GUILayout.Box("","up",GUILayout.Width(260),GUILayout.Height(150));//fade en haut
//			GUILayout.BeginVertical("bg",GUILayout.Width(260));
//			Root.showSubItms();
//			GUILayout.EndVertical();
//			GUILayout.Box("","dw",GUILayout.Width(260),GUILayout.Height(150));//fade en bas
//			
//			GUILayout.EndScrollView();
//			
//		    GUILayout.FlexibleSpace();
//		    GUILayout.EndArea();
//		}
//	}
//	
//	//AUX FCN
//	
//	public void canDisplay(bool b)
//	{
////		canDisp = b;	
//	}
//	
//	public void updateGUI(GUIItemV2 itm,int val,bool reset)
//	{
//		if(reset)
//			activeItem = itm;
//		
//		switch (itm.getDepth())
//		{
//			case 0:
//				selM = val;
//				if(reset)
//				{
//					selSM = -1;
//					selT = -1;
//					if(itm.getSelectedItem()!=null && itm.getSelectedItem().getSelectedItem()!=null)
//						itm.getSelectedItem().resetSelected();
//					if(itm.getSelectedItem()!=null)
//						itm.resetSelected();
//				}
//				break;
//				
//			case 1:
//				selSM = val;
//				if(reset)
//				{
//					selT = -1;
//					if(itm.getSelectedItem()!=null)
//						itm.resetSelected();
//				}
//				break;
//				
//			case 2:
//				selT = val;
//				break;
//		}
//	}
//	
//	public void setVisibility(bool b)
//	{
//		showMenu = b;	
//	}
//	
//	public bool isVisible()
//	{
//		return showMenu;
//	}
//	
//	public bool isOnUI()
//	{
//		return false;	
//	}
//	
//	private void resetMenu()
//	{
//		selM = -1;
//		selSM = -1;
//		selT = -1;
//		
//		if(Root.getSelectedItem()!= null)//menu
//		{
//			if(Root.getSelectedItem().getSelectedItem()!=null)//sous menu
//			{
//				if(Root.getSelectedItem().getSelectedItem().getSelectedItem() != null)//tool
//				{
//					Root.getSelectedItem().getSelectedItem().resetSelected();
//				}
//				Root.getSelectedItem().resetSelected();
//			}
//			Root.resetSelected();
//		}
//	}
//	
//	private float slideHztl()
//	{
//		return (Input.mousePosition.y-Screen.height/2)/10000;
//	}
//	
//	private float slideVrtcl()
//	{
//		return (Input.mousePosition.x-Screen.width/2)/10000;
//	}
//	
//	void doTheJob()
//	{
//		if(selM == 1)
//		{
//			if(selSM == 0)
//			{
//				Vector3 tmp = cube.transform.position;
//				
//				switch (selT) 
//				{
//					case 0:
//					tmp.x = tmp.x + slideHztl();
//					break;
//					case 1:
//					tmp.y = tmp.y + slideHztl();
//					break;
//					case 2:
//					tmp.z = tmp.z + slideHztl();
//					break;
//				}
//				
//				cube.transform.position = tmp;
//				
//			}
//			else if(selSM == 1)
//			{
//				switch (selT) 
//				{
//					case 0:
//					cube.transform.RotateAround(Camera.mainCamera.transform.up,0.01f);
//					break;
//					case 1:
//					cube.transform.RotateAround(Camera.mainCamera.transform.forward,0.01f);
//					break;
//					case 2:
//					cube.transform.RotateAround(Camera.mainCamera.transform.right,0.01f);
//					break;
//				}
//			}
//		
//		}
//	}
//	
////	void updateGroupPos()
////	{
////		float height = Root.getSubItemsCount();
////		float posy = Screen.height/2;
////		
////		if(SelectedMenu != null)
////		{
////			height = height + SelectedMenu.getSubItemsCount();
////			posy = posy - (selM+1)*50;
////		}
////		else
////		{
////			posy = posy - (Root.getSubItemsCount()/2)*50;
////		}
////		
////		if(SelectedSubMenu != null)
////		{
////			height = height + SelectedSubMenu.getSubItemsCount();
////			posy = posy - (selSM+1)*50-(SelectedSubMenu.getSubItemsCount()/2)*50;
////		}
////		else
////		{
////			if(SelectedMenu != null)
////				posy = posy - (SelectedMenu.getSubItemsCount()/2)*50;
////		}
////		
////		height = height*50;
////		
////		tgtHeight = height+300;
////		tgtPos = posy-150;
//////		menuGroup.height = height;
//////		menuGroup.y = posy;
////	}
//}
