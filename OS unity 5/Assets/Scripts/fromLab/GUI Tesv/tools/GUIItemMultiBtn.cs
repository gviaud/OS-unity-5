using UnityEngine;
using System.Collections;

public class GUIItemMultiBtn: GUIItemV2
{	
	int itmW = 260;
	int itmH = 50;
	
	int _off7;
		
	ArrayList subItems;
	string m_backGround;
	
	public GUIItemMultiBtn(int d,string bg,string bOn,string bOf,GUIInterface o,int offset)
		:base(d,-1,"",bOn,bOf,o)
	{
		subItems = new ArrayList();
		m_backGround = bg;
		_off7 = offset;
	}	public GUIItemMultiBtn(int d,string bg,string bOn,string bOf,GUIInterface o,int offset, bool dontHideWhenUnselected)
		:base(d,-1,"",bOn,bOf,o,dontHideWhenUnselected)
	{
		subItems = new ArrayList();
		m_backGround = bg;
		_off7 = offset;
	}
	
	//fcns sub items
	
	//ajout d'un set d'item
	public override void setItems(ArrayList inSub)
	{
		subItems = inSub;
		m_subItemsCount = subItems.Count;
	}
	
	//ajout d'un item
	public override void addSubItem(GUIItemV2 itm)
	{
		subItems.Add(itm);
		m_subItemsCount ++;
	}
	
	//retourne la position dans la liste de l'item donné
	/*public int getSubItemIndex(GUIItemV2 itm)
	{
		return subItems.IndexOf(itm);//>=0 si qqch, -1 sinon
	}*/
	
	//UI
	
	//Affichage des sous items
	public override bool getUI(bool b)//showSubItms()
	{		
		GUILayout.BeginHorizontal(m_backGround,GUILayout.Width(itmW),GUILayout.Height(itmH));
		GUILayout.Space(_off7);
		
		foreach(GUIItemV2 itm in subItems)
		{
			itm.getUI(40,40);
//			if(itm.getUI(40,40))
//			{
//				origine.updateGUI(itm,getSubItemIndex(itm),true);
//			}
			GUILayout.FlexibleSpace();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		return false;
	}
}
