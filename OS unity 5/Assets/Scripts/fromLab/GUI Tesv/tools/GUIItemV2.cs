using UnityEngine;
using System.Collections;

using Pointcube.Global;

public class GUIItemV2
{
	public string m_text;
	
	private Texture2D t2dicon;
	
	int itmW = 260;
	int itmH = 50;
	protected int m_subItemsCount=0;
	protected int m_depth;
	protected int m_actionId;
	
	protected bool _enableUI = true;
	
	protected bool bisSecondButtonClicked = false;
	protected bool bsecondButton = false;
	
	ArrayList subItems;
	
	//	Rect btnRect;
	
	protected string bgOn;
	protected string bgOff;
	
	protected string bgSecondButtonOn = "";
	protected string bgSecondButtonOff = "";
	protected string textSecondOn = ""; 
	protected string textSecondOff = "";
	//	protected Texture2D icoOn;
	//	protected Texture2D icoOff;
	
	protected GUIContent gc;
	
	protected GUIItemV2 selectedItm = null;
	
	protected GUIInterface origine;
	
	protected bool _dontHideWhenSelected = false;
	
	protected bool _isToogleActivated = false;
	public bool isBackground = false;

	public GUIItemV2(int depth,int actionId,string inTxt,string bOn,string bOf,GUIInterface o)
	{	
		m_depth = depth;
		m_actionId = actionId;
		m_text = inTxt;
		subItems = new ArrayList();
		
		bgOn = bOn;
		bgOff = bOf;
		//		icoOn = iOn.normal.background;
		//		icoOff = iOf.normal.background;
		origine = o;
		_dontHideWhenSelected = false;
		
		t2dicon = null;
	}	
	public GUIItemV2(int depth,int actionId,string inTxt,string bOn,string bOf,GUIInterface o, bool dontHideWhenUnselected)
	{
		m_depth = depth;
		m_actionId = actionId;
		m_text = inTxt;
		subItems = new ArrayList();
		
		bgOn = bOn;
		bgOff = bOf;
		//		icoOn = iOn.normal.background;
		//		icoOff = iOf.normal.background;
		origine = o;
		_dontHideWhenSelected = dontHideWhenUnselected;
		t2dicon = null;
	}
	
	//fcns sub items
	//externes
	
	//Ajout de plusieurs SubItems
	public virtual void setItems(ArrayList inSub)
	{
		subItems = inSub;
		m_subItemsCount = subItems.Count;
	}
	
	//Ajout de 1 SubItem
	public virtual void addSubItem(GUIItemV2 itm)
	{
		subItems.Add(itm);
		m_subItemsCount ++;
	}
	public bool DontHideWhenSelected()
	{
		return _dontHideWhenSelected;
	}
	//retourne tout les SubItems
	public ArrayList getSubItems()
	{
		return subItems;
	}
	
	//retourne le nombre de SubItems
	public int getSubItemsCount()
	{
		return m_subItemsCount;	
	}
	
	public int getActionId()
	{
		return m_actionId;
	}
	
	//	//retourne la position dans la liste de l'item donné
	//	public int getSubItemIndex(GUIItemV2 itm)
	//	{
	//		return subItems.IndexOf(itm);//>=0 si qqch, -1 sinon
	//	}
	
	//retourne l'item sélectionné
	public GUIItemV2 getSelectedItem()
	{
		return selectedItm;	
	}
	
	//déselectionne l'item sélectionné
	public void resetSelected()
	{
		selectedItm = null;	
	}
	
	//retourne la profondeur de l'item : root = -1, menu =0, sousmenu = 1, ...
	public int getDepth()
	{
		return m_depth;	
	}
	
	//UI
	
	//changement du nom
	public void chgTxt(string newTxt)
	{
		m_text = newTxt;	
	}
	
	//Affichage des sous items
	public virtual void showSubItms()
	{
		//bisSecondButton = false;
		
		foreach(GUIItemV2 itm in subItems)
		{
			if(itm != null)
			{
				if(itm.IsEnableUI())
				{
					if(itm == selectedItm)
					{
						if((itm.getSubItemsCount()>0)||(itm.isToogleActivated()))
						{
							if(itm.getUI(true))
							{
								origine.updateGUI(selectedItm,-1,true);
								selectedItm = null;
							}
							itm.showSubItms();
						}
						else
						{
							itm.getUI(true);
						}
					}
					else
					{
						if(!itm.DontHideWhenSelected())
						{
							/*if(selectedItm != null && itm.m_depth == 0)
								GUI.color = new Color(1,1,1,0.75f);*/
						}
						else
						{
							//GUI.color = new Color(1,1,1,1);
						}
						if(itm.getUI(false))
						{
							selectedItm = itm;
							origine.updateGUI(selectedItm,/*getSubItemIndex(selectedItm)*/selectedItm.m_actionId,true);
						}
						//GUI.color = new Color(1,1,1,1);
					}
				}
			}
		}
	}
	
	//Affichage de l'item (this)
	public virtual bool getUI(bool activ)
	{
		bool bcheck = false;
		float fw = 260;
		string szbg = activ ? bgOn : bgOff;
		
		if(bsecondButton || t2dicon != null)
		{
			GUILayout.BeginHorizontal(/*activ ? "emptyRightOn" : "emptyRightOff"*/szbg, GUILayout.Height(50), GUILayout.Width(210));
			
			float foffset = 50.0f;
			
			GUILayout.Box("", "empty", GUILayout.Height(foffset));
			
			if(bsecondButton)
			{
				
				bool btemp  = GUILayout.Button("", activ ?  bgSecondButtonOn : bgSecondButtonOff, GUILayout.Height(50), GUILayout.Width(50));
				
				if(btemp)
				{
					bisSecondButtonClicked = true;
				}
			}
			else if(t2dicon != null)
			{
				GUILayout.Box(t2dicon, GUILayout.Height(50), GUILayout.Width(50));
			}
			
			bcheck = GUILayout.Button(m_text, activ ? textSecondOn : textSecondOff, GUILayout.Height(50), GUILayout.Width(210 - foffset));
		}
		else
		{
			bcheck = GUILayout.Button(m_text/* + m_actionId.ToString()*/,szbg, GUILayout.Height(50),GUILayout.Width(210));
		}
		
		if(bsecondButton || t2dicon != null)
		{
			GUILayout.EndHorizontal();
		}
		
		return bcheck;
	}
	
	public virtual bool getUI(int w,int h)
	{
		return GUILayout.Button(m_text,bgOn,GUILayout.Height(h),GUILayout.Width(w));
	}
	
	public void setSecondButton(string _bgSecondOn, string _bgSecondOff, string _textSecondOn, string _textSecondOff)
	{
		bgSecondButtonOn = _bgSecondOn;
		bgSecondButtonOff = _bgSecondOff;
		textSecondOn = _textSecondOn;
		textSecondOff = _textSecondOff;
		bsecondButton = true;
	}
	
	public void setActiveSecondButton(bool _bactive)
	{
		bsecondButton = _bactive;
	}
	
	public void setIcon(Texture2D _t2dicon, string _textSecondOn, string _textSecondOff)
	{
		t2dicon = _t2dicon;
		textSecondOn = _textSecondOn;
		textSecondOff = _textSecondOff;
	}
	
	public bool isSecondButtonClicked()
	{
		bool breturn = bisSecondButtonClicked;
		bisSecondButtonClicked = false;
		return breturn;
	}
	
	public void setSelected(int index)
	{
		//selectedItm=(GUIItemV2)subItems[index];
		foreach(GUIItemV2 itm in subItems)
		{
			if(itm != null && itm.m_actionId == index)
			{
				selectedItm = itm;
				return;
			}
		}
	}
	
	public void SetEnableUI(bool enableUI)
	{
		_enableUI = enableUI;
	}
	public bool IsEnableUI()
	{
		return _enableUI;
	}
	
	public void SetToogleActivated(bool activated)
	{
		_isToogleActivated = activated;
	}
	public bool isToogleActivated()
	{
		return _isToogleActivated;
	}
}
