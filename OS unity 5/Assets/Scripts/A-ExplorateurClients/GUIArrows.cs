using UnityEngine;
using System.Collections;

public class GUIArrows : MonoBehaviour
{
	ExplorerV2 explorer;
	
	private Rect r_arrowsRect;
    private Rect r_button1;
    private Rect r_button2;
	
	int uiDepth;
	string m_styleUP;
	string m_styleDwn;
	GUISkin skin;

//    //-----------------------------------------------------
//    void Awake()
//    {
//        UsefullEvents.OnResizingWindow  += SetRects;
//        UsefullEvents.OnResizeWindowEnd += SetRects;
//    }

    //-----------------------------------------------------
	void Start()
	{
		explorer = GetComponent<ExplorerV2>();
		skin = explorer.getSkin();

        r_button1 = new Rect();
        r_button2 = new Rect();
	}

    //-----------------------------------------------------
	void OnGUI()
	{
        r_button1.Set(0,0,r_arrowsRect.width/2,50);
        r_button2.Set(r_arrowsRect.width/2,0,r_arrowsRect.width/2,50);

		GUI.skin = skin;
		GUI.depth = uiDepth;
		GUI.BeginGroup(r_arrowsRect);
		if(GUI.RepeatButton(r_button1,"",m_styleDwn))
		{
			explorer.changePage(+1);
		}
		if(GUI.RepeatButton(r_button2,"",m_styleUP))
		{
			explorer.changePage(-1);
		}
		GUI.EndGroup();
	}

//    //-----------------------------------------------------
//    void OnDestroy()
//    {
//        UsefullEvents.OnResizingWindow  -= SetRects;
//        UsefullEvents.OnResizeWindowEnd -= SetRects;
//    }

    //-----------------------------------------------------
    public void setArrowsRect(Rect r,string styleUp,string styleDwn)
	{
		r_arrowsRect = r;
		m_styleUP = styleUp;
		m_styleDwn = styleDwn;
	}

    //-----------------------------------------------------
	public void setDepth(int i)
	{
		uiDepth = i-2;
	}
}
