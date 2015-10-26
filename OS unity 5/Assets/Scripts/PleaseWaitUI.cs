//---------------------------------------------------------
// PleaseWaitUI.cs - 05/2012 KS

using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
public class PleaseWaitUI : MonoBehaviour
{
    // -- Image "patientez svp" --
    private Rect            m_iconRect;                  // Zone d'affichage de l'image
	private Rect            m_txtRect;  				 // Zone d'affichage du txt additionel
    private bool            m_displayWaitIcon;           // Pour savoir s'il faut afficher l'image
    private bool            m_displayingIcon;            // Pour savoir si l'image est affichée (et donc que le traitement long peut commencer)
    private Texture2D       m_pleaseWaitTexture;          // Image à afficher
	public 	string			m_pleaseWaitTextureName;
 	private string 			m_additionalTxt;			 //TXT additionel
    private bool            m_loadingMode;               // Mode chargement

	private bool 			m_tmpDisplay;
	private string			m_tmpMsg = "";
	private Rect 			m_tmpRect;
	
	public GUIStyle txtStyle;
	
	public GUIStyle tmpMsgStyle;

#region unity_func
    //-----------------------------------------------------
    void Awake()
    {
        string prefix = "en";
        if(PlayerPrefs.HasKey("language"))
            prefix=PlayerPrefs.GetString("language");
        m_pleaseWaitTexture = (Texture2D)Resources.Load("images_multilangue/"+prefix+"/"+m_pleaseWaitTextureName);
    
           UsefullEvents.OnResizingWindow  += RelocateUI;
           UsefullEvents.OnResizeWindowEnd += RelocateUI;
    }

    //-----------------------------------------------------
    void Start()
    {
        // -- Message "PleaseWait" --
       // m_pleaseWaitTexture = (Texture2D) Resources.Load("AnimatedBtns/pleaseWait/pleaseWait1", typeof(Texture2D));
        int iconSizeH = m_pleaseWaitTexture.height;
		int iconSizeW = m_pleaseWaitTexture.width;

        // -- Rects GUI --
        m_iconRect          = new Rect(0f, 0f, iconSizeW, iconSizeH);
		m_txtRect           = new Rect(0f, 0f, 4*iconSizeW, iconSizeH);
		m_tmpRect			= new Rect(0f, 0f, 300, 80);
        RelocateUI();

        m_displayWaitIcon   = false;
        m_displayingIcon    = false;
		m_additionalTxt = "";
	  }

    //-----------------------------------------------------
    void Update ()
    {}

    //-----------------------------------------------------
    void OnGUI()
    {
        if(m_displayWaitIcon || m_loadingMode)
        {
			Rect m_txtRect = new Rect(0, 0, 0, 0);
			if(m_loadingMode)
				m_txtRect = new Rect(0, Screen.height*0.6f, Screen.width, 30f);

            GUI.Label(m_iconRect, m_pleaseWaitTexture);
			GUI.Label(m_txtRect,m_additionalTxt,txtStyle);
            m_displayingIcon = true;
        }
		
		if(m_tmpDisplay)
        {
            GUI.Box(m_tmpRect,m_tmpMsg,tmpMsgStyle);
        }
    }

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= RelocateUI;
        UsefullEvents.OnResizeWindowEnd -= RelocateUI;
    }
#endregion

    //-----------------------------------------------------
    private void RelocateUI()
    {
        int iconSizeH = m_pleaseWaitTexture.height;
        int iconSizeW = m_pleaseWaitTexture.width;

        m_iconRect.x = (Screen.width - iconSizeW)/2;
        m_iconRect.y = (Screen.height - iconSizeH)/2;
        m_txtRect.x  = Screen.width/2 - (2*iconSizeW);
        m_txtRect.y  = (Screen.height + iconSizeH)/2;
        m_tmpRect.x  = Screen.width/2 - 150;
        m_tmpRect.y  = Screen.height/2 - 40;
    }

    //-----------------------------------------------------
    public void SetDisplayIcon(bool newState)
    {
//        Debug.Log ("PLEASE WAIT : displaying ="+newState);
        m_displayWaitIcon = newState;
        m_displayingIcon  = (newState == true && m_displayingIcon == true);
		if(!newState)
			m_additionalTxt = "";
    }
	
	public void setAdditionalLoadingTxt(string s)
	{
		m_additionalTxt = s;
	}
    
    //-----------------------------------------------------
    public bool IsDisplayingIcon()
    {
        return m_displayingIcon;
    }

    //-----------------------------------------------------
    public void SetLoadingMode(bool loadingMode)
    {
        m_loadingMode = loadingMode;
    }

    //-----------------------------------------------------
	public IEnumerator showTempMsg(string msg, float tps)
	{
		m_tmpMsg = msg;
		m_tmpDisplay = true;
		yield return new WaitForSeconds(tps);
		m_tmpDisplay = false;
		m_tmpMsg = "";
		yield return null;
	}
	
} // class PleaseWaitUI
