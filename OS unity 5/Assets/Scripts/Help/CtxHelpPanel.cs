using UnityEngine;
using System.Collections.Generic;

public class CtxHelpPanel
{
    private string    m_id;
    private Texture2D m_texture;
    private Rect      m_bgRect;
    private bool      m_fadeOnScroll;
    private float     m_delay;
    private float     m_lgDelay;
	private bool      m_firstDisplay;

    private List<MultiLanguages> m_labels;
    private List<Rect>           m_labelRects;
    private List<GUIStyle>       m_labelStyles;

    private const string DBGTAG = "CtxHelpPanel : ";

    public CtxHelpPanel(string id, Texture2D texture, bool fadeOnScroll, float delay, float lgDelay)
    {
        m_id = id;
        m_texture = texture;
        m_bgRect = new Rect(0f, 0f, texture.width, texture.height);
        m_fadeOnScroll = fadeOnScroll;
        m_delay = delay;
        m_lgDelay = lgDelay;
		m_firstDisplay = true;

        m_labels      = new List<MultiLanguages>();
        m_labelRects  = new List<Rect>();
        m_labelStyles = new List<GUIStyle>();
    }

    //-----------------------------------------------------
    public void AddLabel(string[] lang, string[] label, Rect rect, GUIStyle style)
    {
        MultiLanguages ml = new MultiLanguages();
        for(int i=0; i<lang.Length; i++)
            ml.AddText(lang[i], label[i], lang[i].Equals("en"));
        m_labels.Add(ml);
        m_labelRects.Add(rect);
        m_labelStyles.Add(style);
    }

    //-----------------------------------------------------
    public void Draw(Rect r)
    {
        GUI.BeginGroup(r);
		Debug.Log ("YOOOOOOOOOLOOO " + m_bgRect.width + m_bgRect.height);
        GUI.DrawTexture(m_bgRect, m_texture);
       /* for(int i=0; i<m_labels.Count; i++)
            GUI.Label(m_labelRects[i], m_labels[i].GetText(PlayerPrefs.GetString("language")), m_labelStyles[i]);*/
        GUI.EndGroup();
		m_firstDisplay = false;
    }

    //-----------------------------------------------------
    public float GetWidth()
    {
        return m_texture.width;
    }

    //-----------------------------------------------------
    public float GetHeight()
    {
        return m_texture.height;
    }

    //-----------------------------------------------------
    public string GetID()
    {
        return m_id;
    }

    //-----------------------------------------------------
    public bool FadeOnScroll()
    {
        return m_fadeOnScroll;
    }

    //-----------------------------------------------------
    public float GetDelay()
    {
        return m_firstDisplay ? m_delay : m_lgDelay;
    }
	
	//-----------------------------------------------------
	public void Reinit()
	{
		m_firstDisplay = true;
	}

} // class CtxHelpPanel
