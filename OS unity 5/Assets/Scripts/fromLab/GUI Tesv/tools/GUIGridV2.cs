using UnityEngine;
using System.Collections;

public class GUIGridV2 : GUIItemV2
{	
	int itmW = 260;
	int itmH = 260;
	
	ArrayList subItems;
	
	//GRID
	int selected = -1;
	int uiSelected=-1;
	string[] content;
	Texture2D[] thumbs;
	int pageIndex=0;
	Vector2 scrollPos = new Vector2(0,0);
	int maxPage=0;
	
	//MDS
//	GUIMultiDotSlider mds;
	
	public GUIGridV2(int d,int actId,string inTxt,string bOn,string bOf/*,GUIStyle iOn,GUIStyle iOf*/,GUIInterface o)
		:base(d,actId,inTxt,bOn,bOf/*,iOn,iOf*/,o)
	{
		subItems = new ArrayList();
//		mds = new GUIMultiDotSlider(1);
		thumbs = new Texture2D[0];
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
	
	//set du contenu img de la grille
	public void setImgContent(Texture2D[] imgs)
	{
		thumbs = imgs;
		
		maxPage = thumbs.Length/6;
		
		if(thumbs.Length>maxPage*6)
			maxPage ++;
				
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
		
//		mds.setNbDot(maxPage);
	}
	
	public Texture2D getImgContent(int index)
	{
		return thumbs[index];
	}
	
	//Changement de page de la grille
	public void changePage(int i)
	{
		if(pageIndex + i<maxPage && pageIndex+i >=0)
			pageIndex=pageIndex+i;
			
		scrollPos.x = pageIndex*200;
		
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
	public override void showSubItms()
	{
		if(selected != -1)
		{
			selected = -1; // pour agir comme un btn;
			origine.updateGUI(selectedItm,-1,false);
		}
		
		GUILayout.BeginHorizontal("gridbg",GUILayout.Width(itmW),GUILayout.Height(itmH));
		GUILayout.Space(10);
		GUILayout.Space(20);
		
		if(thumbs.Length == 0)
			selected = GUILayout.SelectionGrid(selected,content,2,"grid",GUILayout.Width(itmW),GUILayout.Height(itmH));
		else
		{
			displayObjs();
			GUILayout.BeginVertical(GUILayout.Height(325+50));
			GUILayout.FlexibleSpace();
			
//			float x = itmW/2-(mds.getGrp().width/2) - 10;
//			float y = GUILayoutUtility.GetLastRect().y+itmH+50;
//			mds.setPos(new Vector2(x,y));
//			mds.ui();
//			GUI.BeginGroup(new Rect(itmW/2-60,GUILayoutUtility.GetLastRect().y+itmH+42,100,30));
//			if(GUI.Button(new Rect(0,0,30,30)," ","pagePrev"))
//				changePage(-1);
//			GUI.Label(new Rect(30,0,40,30),pageIndex+1+"/"+maxPage,"pageTxt");
//			if(GUI.Button(new Rect(70,0,30,30)," ","pageNext"))
//				changePage(1);
//			GUI.EndGroup();
			
			GUILayout.EndVertical();
		}
//		selected = GUILayout.SelectionGrid(selected,thumbs,2,"grid",GUILayout.Width(itmW),GUILayout.Height(itmH));
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		
		
		
		
	}
	
	//affichage de la grille avec les pages
	void displayObjs()
	{
		float x = GUILayoutUtility.GetLastRect().x+20;
		float y = GUILayoutUtility.GetLastRect().y;
		
		GUIStyle s = GUI.skin.FindStyle("empty");
		
		GUI.BeginGroup(new Rect(x+45,y,100,30));
		if(GUI.Button(new Rect(0,0,30,30)," ","pagePrev"))
			changePage(-1);
		GUI.Label(new Rect(30,0,40,30),pageIndex+1+"/"+maxPage,"pageTxt");
		if(GUI.Button(new Rect(70,0,30,30)," ","pageNext"))
			changePage(1);
		GUI.EndGroup();
		
		GUI.BeginScrollView(new Rect(x,y+50,200,325),scrollPos,new Rect(0,0,(maxPage)*200,300),s,s);
		int j = 0;
		int a = 0;
		for(int i=0;i<thumbs.Length;i++)
		{
			GUI.Box(new Rect(((i/6)*200)+((a/3)*110)+2,j*105+2,86,86),thumbs[i],"empty");
			if(GUI.Button(new Rect(((i/6)*200)+((a/3)*110),j*105,90,90),"","grid"))//ici remplacer objs par thumbnails
			{
//				objSelected = i;
				selected = i;
				uiSelected = i;
				selectedItm = (GUIItemV2)subItems[selected];
				origine.updateGUI(selectedItm,selected,false);
			}
			if(i == uiSelected)
			{
				GUI.Box(new Rect(((i/6)*200)+((a/3)*110)+2,j*105+2,86,86),"","gridOverLap");
			}
			j++;
			a++;
			if(a>5)
				a = 0;
			if(j>2)
				j = 0;
		}
		GUI.EndScrollView();
	}

}
