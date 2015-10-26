using UnityEngine;
using System.Collections;
using Pointcube.Global;
using Pointcube.InputEvents;

public class GUIUpperList : GUIItemV2
{	
	int itmW = 260;
	int itmH = 260;
	
	ArrayList subItems;
	
	public string m_ctxPanelConfig; // ctx1_upperconfig
	public string m_ctxPanelBrand; // ctx1_upperbrand
	
	//GRID
	int selected = -1;
	int uiSelected=-1;
	string[] content;
	Texture2D[] thumbs;
	bool[] customizables;
	bool[] brands;
	int pageIndex=0;
	Vector2 scrollPos = new Vector2(0,0);
	int maxPage=0;
	int m_ipageNumber = 0;
	bool m_thumbsPerso = false;

	Rect r_position;
	Rect r_content;
	
	public Rect bgThumbRect;
	
	float off7 = 0;
	bool _isSliding = false;
	
	public GUIUpperList(int d,int actId,string inTxt,string bOn,string bOf/*,GUIStyle iOn,GUIStyle iOf*/,GUIInterface o)
		:base(d,actId,inTxt,bOn,bOf/*,iOn,iOf*/,o)
	{
		subItems = new ArrayList();
//		mds = new GUIMultiDotSlider(1);
		thumbs = new Texture2D[0];
		r_position = new Rect();
        SetRects();
		r_content = new Rect(0,0,100,100);

        UsefullEvents.OnResizingWindow += SetRects;
        UsefullEvents.OnResizeWindowEnd += SetRects;
	}
	
	//fcns sub items
	
	//ajout d'un set d'item
	public override void setItems(ArrayList inSub)
	{
		subItems = inSub;
		m_subItemsCount = subItems.Count;
		updateContent();
	}
	
	//ajout d'un item
	public override void addSubItem(GUIItemV2 itm)
	{
		subItems.Add(itm);
		m_subItemsCount ++;
		updateContent();
	}

	public void setImgContent(Texture2D[] imgs, Texture2D[] thumbsArray)
	{
		thumbs = thumbsArray;
		
		maxPage = thumbs.Length;
		//		
		//		if(thumbs.Length>maxPage*6)
		//			maxPage ++;
		
		ArrayList tmp = new ArrayList();
		subItems = new ArrayList();
		int i=0;
		foreach(Texture2D t in imgs)
		{
			if(t==null)
				imgs=null;
			addSubItem(new GUIItemV2(m_depth+1,i,t.name,"bgOn","bgOff",origine));
			i++;
		}
		r_content.width = maxPage * 100;
		if(r_content.width <= 600)
		{
			off7 = (/*Screen.width*/650 - r_content.width)/2;
			if(off7<0)
				off7 = 0;
		}
	}
	//set du contenu img de la grille
	public void setImgContent(Texture2D[] imgs)
	{
		thumbs = imgs;
		
		maxPage = thumbs.Length;
//		
//		if(thumbs.Length>maxPage*6)
//			maxPage ++;
				
		ArrayList tmp = new ArrayList();
		subItems = new ArrayList();
		int i=0;
		foreach(Texture2D t in imgs)
		{
			if(t==null)
				imgs=null;
			addSubItem(new GUIItemV2(m_depth+1,i,t.name,"bgOn","bgOff",origine));
			i++;
		}
		r_content.width = maxPage * 100;
		if(r_content.width <= 600)
		{
			off7 = (/*Screen.width*/650 - r_content.width)/2;
			if(off7<0)
				off7 = 0;
		}
//		mds.setNbDot(maxPage);
	}
	
	public void setImgContent(Texture2D[] imgs,string[] names, bool[] _customizables, bool[] _brands)
	{
		thumbs = imgs;
		
		maxPage = thumbs.Length;
		
		customizables = _customizables;
		
		brands = _brands;
				
		ArrayList tmp = new ArrayList();
		subItems = new ArrayList();
		int i=0;

		//Debug.Log ("tabTex : "+imgs.Length);

		foreach(Texture2D t in imgs)
		{
			if(t==null)
				imgs=null;
			addSubItem(new GUIItemV2(m_depth+1,i,names[i],"bgOn","bgOff",origine));

			i++;
		}

		r_content.width = maxPage * 100;
		if(r_content.width <= 600)
		{
			off7 = (/*Screen.width*/650 - r_content.width)/2;
			if(off7<0)
				off7 = 0;
		}

//		mds.setNbDot(maxPage);
	}
	
	public Texture2D getImgContent(int index)
	{
		return thumbs[index];
	}
	
	//Changement de page de la grille
	public void changePage(int i)
	{
//		if(pageIndex + i<maxPage && pageIndex+i >=0)
//			pageIndex=pageIndex+i;
//			
//		scrollPos.x = pageIndex*200;
		
//		mds.setDotPos(pageIndex);
	}
	
	public void setUiId(int i)
	{
		uiSelected = i;
	}
	
	//mise a jour du contenu txt
	void updateContent()
	{
		content = new string[m_subItemsCount];
		for(int i=0;i<m_subItemsCount;i++)
		{
			content[i] = ((GUIItemV2)subItems[i]).m_text;	
		}
	}
	
	//UI
	
	//Affichage des sous items

	public void display()
	{
		/*int d = GUI.depth;
		GUI.depth = -1000;*/
		
		if(selected != -1)
		{
			selected = -1; // pour agir comme un btn;
			origine.updateGUI(selectedItm,-1,false);
		}
		
		float fsizeThumb = 128.0f;
		float fspaceEnterThumb = fsizeThumb / 8;
		float fsizeThumbAndSpace = fsizeThumb + fspaceEnterThumb;
		int inumberThumbInLine = (int)((Screen.width - (fsizeThumbAndSpace * 3.5f)) / fsizeThumbAndSpace);
		inumberThumbInLine = Mathf.Clamp(inumberThumbInLine, 1, inumberThumbInLine);
		float fstartX = (Screen.width * 0.5f) - (fsizeThumbAndSpace * (inumberThumbInLine * 0.5f)) - (fspaceEnterThumb * 0.5f);
		
		int imaxLine = (int)((Screen.height - (fsizeThumbAndSpace * 1.5f)) / fsizeThumbAndSpace);
		float foffsetStartY = thumbs.Length > (inumberThumbInLine * imaxLine) ? -fspaceEnterThumb * 2.0f : -fspaceEnterThumb * 1.0f;
		float fstartY = ((Screen.height - (fsizeThumbAndSpace * imaxLine) - (imaxLine * fspaceEnterThumb * 2.0f)) * 0.5f) + foffsetStartY;
		
		int iline = 1;
		
		for(int i = (m_ipageNumber * (inumberThumbInLine * imaxLine)), j = 0;
		    j < imaxLine; 
			i++)
		{
			if(i % inumberThumbInLine == inumberThumbInLine - 1)
			{
				j++;
				iline = j;
			}
			
			if(i >= thumbs.Length && j != 0)
			{
				break;
			}
		}
		
		//fix
		if((iline * inumberThumbInLine) < (thumbs.Length - (m_ipageNumber * (inumberThumbInLine * imaxLine))) && iline < imaxLine)
		{
			iline++;
		}
		//fix
		
		int ioffsetArrow = thumbs.Length > (inumberThumbInLine * imaxLine) ? 3 : 1;
		
		bgThumbRect = new Rect(fstartX, fstartY - fspaceEnterThumb, 
								(fsizeThumbAndSpace * inumberThumbInLine) + fspaceEnterThumb,
		                       (fsizeThumbAndSpace * iline) + (fspaceEnterThumb * ioffsetArrow) + (iline * fspaceEnterThumb * 2.0f) + (fspaceEnterThumb * 3.0f));
		       		   
		Color tempColor = GUI.color;
		GUI.color = new Color(0, 0, 0, 0.4f);
		GUI.Box(bgThumbRect,"", "upperListThumb");
		GUI.color = tempColor;
		
		if(thumbs.Length > (inumberThumbInLine * imaxLine))
		{
				GUI.Label(new Rect(bgThumbRect.x, bgThumbRect.y + bgThumbRect.height - (fspaceEnterThumb * 5.5f), bgThumbRect.width, fspaceEnterThumb * 2.0f),
			          (m_ipageNumber + 1).ToString() + "/" + ((thumbs.Length / (inumberThumbInLine * imaxLine)) + 1).ToString(),
						"upperPageTxt");
		}   
		            
		for(int i = (m_ipageNumber * (inumberThumbInLine * imaxLine)), j = 0, k = 0; 
		j < iline; 
		i++, k++)
		{
			Rect thumbRect = new Rect(fstartX + (k * fsizeThumbAndSpace) + (fspaceEnterThumb), fstartY + (j * fsizeThumbAndSpace) + (j * fspaceEnterThumb * 2.0f), fsizeThumb, fsizeThumb);
			
			if(i < thumbs.Length)
			{
				GUI.Box(thumbRect,thumbs[i],"upperListThumb");
				
				//int ioffsetIcon = 0;
				float fsizeIcon = 30.0f;
				
				if(customizables != null && customizables[i] &&
				   GUI.Button(new Rect(thumbRect.x/* + thumbRect.width - fsizeIcon*/, thumbRect.y/* + thumbRect.height - (++ioffsetIcon * fsizeIcon)*/, fsizeIcon, fsizeIcon),"","upperConfig"))
				{
					if(PlayerPrefs.GetString("language") == "fr")
					{
						PC.ctxHlp.ShowCtxHelp("ctx1_upper_fr");
					}
					else
					{
						PC.ctxHlp.ShowCtxHelp("ctx1_upper_en");
					}
						
					PC.ctxHlp.ShowHelp();
				}

				if(brands != null && brands[i] &&
				   GUI.Button(new Rect(thumbRect.x + thumbRect.width - fsizeIcon, thumbRect.y , fsizeIcon, fsizeIcon),"","upperBrand"))
				{
					if(PlayerPrefs.GetString("language") == "fr")
					{
						PC.ctxHlp.ShowCtxHelp("ctx1_upper_fr");
					}
					else
					{
						PC.ctxHlp.ShowCtxHelp("ctx1_upper_en");
					}
					
					PC.ctxHlp.ShowHelp();
				}

				if(GUI.Button(thumbRect,"","upperlistTxt"))
				{

					uiSelected = i;
					selectedItm = (GUIItemV2)subItems[i];
					origine.updateGUI(selectedItm,i,false);

				}

				GUI.Label(new Rect(thumbRect.x + 1.0f, thumbRect.y + thumbRect.height - 1.0f, thumbRect.width - 2.0f, fspaceEnterThumb * 2.0f) ,content[i],"upperTittleThumb");
				//bc

				if(i == uiSelected)
				{

					float fsizeFramework = 3.0f;
					
					GUI.Box(new Rect(thumbRect.x - (fsizeFramework * 0.5f), thumbRect.y - (fsizeFramework * 0.5f), thumbRect.width + fsizeFramework, fsizeFramework), "", "whitePX");
					GUI.Box(new Rect(thumbRect.x - (fsizeFramework * 0.5f), thumbRect.y + thumbRect.height - (fsizeFramework * 0.5f), thumbRect.width + fsizeFramework, fsizeFramework), "", "whitePX");
					
					GUI.Box(new Rect(thumbRect.x - (fsizeFramework * 0.5f), thumbRect.y - (fsizeFramework * 0.5f), fsizeFramework, thumbRect.height), "", "whitePX");
					GUI.Box(new Rect(thumbRect.x + thumbRect.width - (fsizeFramework * 0.5f), thumbRect.y - (fsizeFramework * 0.5f), fsizeFramework, thumbRect.height), "", "whitePX");

				}
			}
			
			if(i % inumberThumbInLine == inumberThumbInLine - 1)
			{
				j++;
				k = -1;
			}
		}
		
		if(thumbs.Length > (inumberThumbInLine * imaxLine))
		{
			if(m_ipageNumber > 0 && GUI.Button(new Rect(bgThumbRect.x + (fsizeThumbAndSpace * 0.5f) - 25.0f, bgThumbRect.y + bgThumbRect.height - 100, 50, 50),"","pagePrev"))
			{
				m_ipageNumber--;
			}
			
			if(((m_ipageNumber + 1) * (inumberThumbInLine * imaxLine)) < thumbs.Length &&
			   GUI.Button(new Rect(bgThumbRect.x + bgThumbRect.width - (fsizeThumbAndSpace * 0.5f) - 25.0f, bgThumbRect.y + bgThumbRect.height - 100, 50, 50),"","pageNext"))
			{
				m_ipageNumber++;
			}
		}
		
		Rect rectBtnOk = new Rect(bgThumbRect.x + fspaceEnterThumb, bgThumbRect.y + bgThumbRect.height - 50, bgThumbRect.width - (fspaceEnterThumb * 2.0f), fspaceEnterThumb * 2.0f);
		if(GUI.Button(rectBtnOk,"OK","upperlistTxt"))
		{
			GUIMenuLeft guiML = origine as GUIMenuLeft;
			if(guiML)
			{

				guiML.closeMenu();
			}
			else
			{
				GUIMenuInteraction guiMI = GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>();
				
				if(guiMI)
				{

					guiMI.closeMenu();
				}
			}

			GUIMatPicker guiMP  =GameObject.Find("MainScene").GetComponent<GUIMatPicker>();
			if(guiMP!=null)
			{
				GameObject.Find("MainScene").GetComponent<GUISubTools>().Validate();
			}
		}

		//Color bkupColor = GUI.color;
		//GUI.color = Color.green;
		//GUI.Label(rectBtnOk, "", "upperTittleThumb");
		//GUI.color = bkupColor;
		//GUI.Label(rectBtnOk, "OK", "upperlistTxt");
		
		//GUI.depth = d;
		
		GameObject.Find("MainScene").GetComponent<GUIMenuMain>().ActivateLineDisplay(false);
	}

    private void SetRects()
    {
        r_position.Set((Screen.width-650)/2,10,650, 160);
    }

    // -- Destructeur --
	~GUIUpperList()
    {
        UsefullEvents.OnResizeWindowEnd -= SetRects;
        UsefullEvents.OnResizingWindow -= SetRects;
    }


}
