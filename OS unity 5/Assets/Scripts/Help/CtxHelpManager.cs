using UnityEngine;
using System.Collections.Generic;

using Pointcube.Global;
using Pointcube.InputEvents;

public class CtxHelpManager : MonoBehaviour
{
    // -- Public --
    public  string       m_xmlHelpPanelsPath;    // gui/help/ctxHelp/xml/panels
    public  string       m_imgHelpPanelsFolder;  // gui/help/ctxHelp/textures

    public  GUIStyle     m_helpTxtStyleLeft;
    public  GUIStyle     m_helpTxtStyleCenter;
    public  GUIStyle     m_helpTxtStyleRightBot;
    public  GUIStyle     m_helpTxtStyleFooter;

    // -- Panneaux --
    private Dictionary<string, CtxHelpPanel>  m_panels;

    // -- Affichage --
    private CtxHelpPanel m_curHelpPanel;
    private CtxHelpPanel m_nextHelpPanel;
    private Rect         m_helpRect;        // image d'aide
    public bool         m_displayHelp;
    private bool         m_hiding;
    private float        m_helpTimer;

    // -- Transition animée help --
    private bool         m_animHelp;        // En cours ou non
    private float        m_animHelpProg;    // Progression
    private float        m_helpGUIalpha;
    private float        m_helpGUIaStart;
    private float        m_helpGUIaEnd;
    private bool         m_helpCountdown;
    
    //public float m_ftimerButton = 0.1f;

    // -- Countdown --
    private bool         m_useStartCountdown;
	private float		 m_startCountdown; 		// countdown pour le premier panneau d'un montage
//    private const float  c_helpCountdown = 3f;    // en secondes

    // -- Clic capté ou non (pour panneaux semi-modaux) --
    private bool         m_panelBlockingInputs;
	private bool         m_lastFrameClicked;
	
	private bool m_bkupRight = false;
	private bool m_bkupLeft = false;
	
	private int[] m_bkupStateMenu = new int[3];
	
	//public bool bhelp = true;
	public bool blockJustShow = false;
	
	private string m_szpanelID = "";

    // -- Debug --
    private static readonly string DBGTAG = "CtxHelp : ";
    private static readonly bool   DEBUG  = true;

    #region unity_func
    //----------------------------------------------------
    void Awake()
    {
        PC.ctxHlp = this;

        // -- Affichage --
        m_curHelpPanel      = null;
        m_nextHelpPanel     = null;
        m_helpRect          = new Rect();
        m_helpTimer         = 0f;
        m_displayHelp       = false;
        m_hiding            = false;

        m_animHelp          = false;
        m_animHelpProg      = 0f;
        m_helpGUIalpha      = 0f;
        m_helpGUIaStart     = 0f;
        m_helpGUIaEnd       = 0f;
        m_helpCountdown     = false;

        m_useStartCountdown  = false;

        m_panelBlockingInputs    = false;
        m_lastFrameClicked  = false;
        // -- Chargement auto des images d'aide, selon plateforme --
        m_panels = new Dictionary<string, CtxHelpPanel>();


#if !UNITY_ANDROID
        LoadPanels();
#endif
    }

    //-----------------------------------------------------
    void OnEnable()
    {
        UsefullEvents.OnResizeWindowEnd += FitToScreen;
        UsefullEvents.OnResizingWindow  += FitToScreen;
        #if UNITY_ANDROID
             this.enabled = false;
        #endif  
    }

    //-----------------------------------------------------
    void Update()
    {
		/*if(m_curHelpPanel == null !bhelp)
            return;*/

        // -- Countdown --
       /*	if(m_helpCountdown)
        {
            m_helpTimer += Time.deltaTime;
            if(m_helpTimer >= (m_useStartCountdown? m_startCountdown : m_curHelpPanel.GetDelay()))
            {
				//Debug.Log(DBGTAG+" Cooldown Finished : "+m_helpTimer);
                ShowHelp();
                m_helpCountdown = false;
                m_useStartCountdown = false;
            }
            
        } // helpCountdown*/
		
		Rect rscreen = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
		
        // -- Input events -> Hide --
        if(m_lastFrameClicked)
        {
            m_lastFrameClicked = false;
            m_panelBlockingInputs = false;
        }
		
        if(m_helpCountdown && (ClickBegin() || (m_curHelpPanel.FadeOnScroll() && ScrollBegin())))
		{
			//m_curHelpPanel = null;
            HideHelp();
        }
        else if(m_displayHelp && ClickBegin())
		{
			//m_curHelpPanel = null;
            HideHelp();
			m_panelBlockingInputs = PC.In.CursorOnUI(rscreen);
		}
		else if(m_displayHelp && m_curHelpPanel.FadeOnScroll() && ScrollBegin())
		{
			//m_curHelpPanel = null;
            HideHelp();
			m_panelBlockingInputs = PC.In.CursorOnUI(rscreen);
			m_lastFrameClicked = true;
        }
        else if(m_panelBlockingInputs && ClickEnded())
		{
			//m_curHelpPanel = null;
            m_lastFrameClicked = true;
        }
    }

    //-----------------------------------------------------
    void OnGUI()
    {
		/*if(PlayerPrefs.GetInt("HelpPopup") == 0)
            return;*/
		
		int ibkupDepth = GUI.depth;
		GUI.depth = 10000;
//        GUI.Button(new Rect(300, 50, 200, 50), "::: "+(m_curHelpPanel != null ?(m_useLongCountdown? m_curHelpPanel.GetLongDelay() : m_curHelpPanel.GetDelay()):0));

        if(m_displayHelp)
        {
            Color bkupCol = GUI.color;
            Color newCol  = GUI.color;
            newCol.a = m_helpGUIalpha;
            GUI.color = newCol;
            
			m_curHelpPanel.Draw(new Rect((Screen.width * 0.5f) - (m_curHelpPanel.GetWidth() * 0.5f), 
			                             (Screen.height * 0.5f) - (m_curHelpPanel.GetHeight() * 0.5f),
			                             m_curHelpPanel.GetWidth(), m_curHelpPanel.GetHeight()));
            GUI.color = bkupCol;
        } // display HelpPanel
		
		GUI.depth = ibkupDepth;
		/*if(!m_displayHelp && bhelp && GUI.Button(new Rect((Screen.width * 0.5f) - 25.0f, Screen.height * 0.1f, 50.0f, 50.0f), "?"))
		{
			bhelp = true;
			blockJustShow = true;
			ShowHelp();
		}*/
		/*else if(ClickEnded() && !bhelp)
		{
			m_curHelpPanel = null;
			HideHelp();
		}*/
    }

    //-----------------------------------------------------
    void FixedUpdate()
    {
        if(m_animHelp)
        {
            if(m_animHelpProg <= PC.T_ANIM_LEN)
            {
                m_helpGUIalpha = Mathf.Lerp(m_helpGUIaStart, m_helpGUIaEnd,
                                  ((-Mathf.Cos(m_animHelpProg/PC.T_ANIM_LEN*2*Mathf.Acos(0))+1)/2));
                m_animHelpProg++;
            }
            else
            {
                m_animHelpProg = 0f;
                m_animHelp = false;

                if(m_helpGUIalpha == 0f)
                {
                    m_displayHelp = false;    // Désactiver si invisible en fin de transition
                    m_hiding = false;
                    //if(m_nextHelpPanel != null) ChangeHelpPanel(m_nextHelpPanel);
                }
            }
        } // anim_Help

    } // FixedUpdate()

    //-----------------------------------------------------
    void OnDisable()
    {
        UsefullEvents.OnResizeWindowEnd -= FitToScreen;
        UsefullEvents.OnResizingWindow  -= FitToScreen;
    }
    #endregion

    #region multi_res
    //-----------------------------------------------------
    private void FitToScreen()
    {
        if(m_curHelpPanel != null)
        {
            m_helpRect.x = (UnityEngine.Screen.width-m_curHelpPanel.GetWidth())/2f;
            m_helpRect.y = (UnityEngine.Screen.height-m_curHelpPanel.GetHeight())/2f;
        }
    }
    #endregion

    #region mecaniques
    //-----------------------------------------------------
    private void LaunchHelpCountdown()
    {
        m_helpCountdown = true;
        m_helpTimer     = 0f;
    }

    //-----------------------------------------------------
    public void ShowHelp()
	{
		/*if(PlayerPrefs.GetInt("HelpPopup") == 0)
			return;*/
			
        if(!m_displayHelp)
		{
			GUIMenuLeft guiML = GameObject.Find("MainScene").GetComponent<GUIMenuLeft>();
			GUIMenuRight guiMR = GameObject.Find("MainScene").GetComponent<GUIMenuRight>();
			
			if(guiML != null && guiML.isVisible())
			{
				m_bkupLeft = true;
				m_bkupStateMenu = guiML.getStateMenu();
				guiML.setVisibility(false);
				guiML.canDisplay(true);
			}
			
			if(guiMR != null && guiMR.isVisible())
			{	
				m_bkupRight = true;
				m_bkupStateMenu = guiMR.getStateMenu();
				print (m_bkupStateMenu[0] + " " + m_bkupStateMenu[1] + " " +m_bkupStateMenu[2]);
				guiMR.setVisibility(false);
				guiMR.canDisplay(true);
				guiMR.avatarForceDisplay(false);
			}
			
			
			//m_panels.Remove(m_szpanelID); //Un message d'aide par session
            m_displayHelp = true;
            m_helpGUIaStart = m_helpGUIalpha;
            m_helpGUIaEnd = 1f;
            m_animHelpProg = 0f;
            m_animHelp = true;
        }
    } // ShowHelp()

    //-----------------------------------------------------
    private void HideHelp()
    {
        m_helpCountdown = false;

        if(m_displayHelp && !m_hiding)
		{
			if(m_bkupLeft)
			{
				GUIMenuLeft guiML = GameObject.Find("MainScene").GetComponent<GUIMenuLeft>();
				
				if(guiML != null)
				{
					guiML.setVisibility(true);
					guiML.canDisplay(false);
					guiML.setBkupStateMenu(m_bkupStateMenu);
					guiML.setMenuState();
				}
				
				m_bkupLeft = false;
			}
			
			if(m_bkupRight)
			{
				GUIMenuRight guiMR = GameObject.Find("MainScene").GetComponent<GUIMenuRight>();
				
				if(guiMR != null)
				{
					guiMR.setVisibility(true);
					guiMR.setBkupStateMenu(m_bkupStateMenu);
					if(m_bkupStateMenu[1] == 6)
					{
						guiMR.avatarForceDisplay(true);
					}
					guiMR.setMenuState();
				}
				
				m_bkupRight = false;
			}
			
            m_helpGUIaStart = m_helpGUIalpha;
            m_helpGUIaEnd = 0f;
            m_animHelpProg = 0f;
            m_animHelp = true;
            m_hiding = true;
        }
    }
    
    public void quitHelpPanel()
    {
    	m_curHelpPanel = null;
    }

    //-----------------------------------------------------
    private void ChangeHelpPanel(CtxHelpPanel newPanel, string _panelID)
    {
        if(!m_displayHelp)
        {
			m_szpanelID = _panelID;
            m_curHelpPanel    = newPanel;
            FitToScreen();
            m_helpRect.width  = m_curHelpPanel.GetWidth();
            m_helpRect.height = m_curHelpPanel.GetHeight();
        }
        else
            m_nextHelpPanel = newPanel; // si image d'aide visible ne pas changer tout de suite


    } // ChangeHelpPanel()

    #endregion

    #region load
    //-----------------------------------------------------
    private void LoadPanels()
    {
        TextAsset xml = (TextAsset) Resources.Load(m_xmlHelpPanelsPath, typeof (TextAsset));

        if(xml != null)
        {
            XMLParser parser = new XMLParser();

            // -- Récupération des suffixes des noms de fichier (Touch / mouse) --
            XMLNode root        = parser.Parse(xml.text).GetNode("root>0");
            XMLNodeList sfxList = root.GetNodeList("suffixes>0>suffix");
            string filenameSfx = "";
            foreach(XMLNode sfx in sfxList)
            {
                #if UNITY_IPHONE || UNITY_ANDROID
                    if(sfx.GetValue("@inputType") == "touch")
                        filenameSfx = sfx.GetValue("_text");
                #else
                    if(sfx.GetValue("@inputType") == "mouse")
                        filenameSfx = sfx.GetValue("_text");
                #endif
            }

            // -- Récupération des données par défaut --
            string startDelayStr = root.GetNode("defaultValues>0>startDelay>0").GetValue("_text");
            float.TryParse(startDelayStr, out m_startCountdown);
			
            string dftDelayStr = root.GetNode("defaultValues>0>stdDelay>0").GetValue("_text");
            float dftDelay;
            float.TryParse(dftDelayStr, out dftDelay);

            string dftLdelayStr = root.GetNode("defaultValues>0>longDelay>0").GetValue("_text");
            float dftLdelay;
            float.TryParse(dftLdelayStr, out dftLdelay);
			
//            Debug.Log(DBGTAG+" default Delay : "+dftDelay+" cuz "+dftDelayStr+" AND dftLdelay : "+dftLdelay+" cuz "+dftLdelayStr);

            // -- Récupération des données des panneaux d'aide --
            XMLNodeList panels = root.GetNodeList("panels>0>panel");
            string    id, bgImgName, bgImgExt, path, align, fadeOnScroll, delayStr, lgDelayStr;
            string[]  lang, text;
            Texture2D bgImg;
            Rect      rect = new Rect();
            XMLNodeList labels, texts;
            CtxHelpPanel newPanel;
            float     x, y, w, h, delay, lgDelay;
            foreach(XMLNode panel in panels)
            {
                id = panel.GetValue("@id");
                fadeOnScroll = panel.GetValue("@fadeOnScroll");
                delayStr     = panel.GetValue("@stdDelay");
                lgDelayStr   = panel.GetValue("@longDelay");

                if(delayStr.Length > 0)
                    float.TryParse(delayStr, out delay);
                else
                    delay = dftDelay;

                if(lgDelayStr.Length > 0)
                    float.TryParse(lgDelayStr, out lgDelay);
                else
                    lgDelay = dftLdelay;

                bgImgName = panel.GetNode("bgImg>0").GetValue("@filename");
                bgImgExt  = panel.GetNode("bgImg>0").GetValue("@extension");

                path  = m_imgHelpPanelsFolder+"/"+bgImgName+filenameSfx;
                bgImg = (Texture2D) Resources.Load(path, typeof(Texture2D));
                if(bgImg == null)
                    Debug.LogError(DBGTAG+"Can't find resource : "+path);

                newPanel = new CtxHelpPanel(id, bgImg, fadeOnScroll.ToLower().Equals("true"), delay, lgDelay);

                labels = panel.GetNodeList("labels>0>label");
                foreach(XMLNode label in labels)
                {
                    align = label.GetValue("@align");
                    float.TryParse(label.GetValue("@x"), out x);
                    float.TryParse(label.GetValue("@y"), out y);
                    float.TryParse(label.GetValue("@w"), out w);
                    float.TryParse(label.GetValue("@h"), out h);
                    rect.Set(x, y, w, h);

                    texts = label.GetNodeList("text");
                    lang = new string[texts.Count];
                    text = new string[texts.Count];
                    int i=0;
                    foreach(XMLNode textNode in texts)
                    {
                        lang[i]   = textNode.GetValue("@lang");
                        text[i++] = textNode.GetValue("_text");
                    }
                    if(align == "left")
                        newPanel.AddLabel(lang, text, rect, m_helpTxtStyleLeft);
                    else if(align == "center")
                        newPanel.AddLabel(lang, text, rect, m_helpTxtStyleCenter);
                    else if(align == "right-bottom")
                        newPanel.AddLabel(lang, text, rect, m_helpTxtStyleRightBot);
                    else if(align == "footer")
                        newPanel.AddLabel(lang, text, rect, m_helpTxtStyleFooter);
                    else
                        newPanel.AddLabel(lang, text, rect, m_helpTxtStyleLeft);
                } // foreach label

                m_panels.Add(id, newPanel);
            } // foreach panel
        }
        else
            Debug.LogError(DBGTAG+"Can't find XML : \""+m_xmlHelpPanelsPath+"\"");
    }
    #endregion

    //-----------------------------------------------------
    // TODO à remplacer par PC.In.Click1Down() || PC.In.Click2Down
    private bool ClickBegin()
    {
        #if UNITY_IPHONE || UNITY_ANDROID
             return (Input.touchCount>0 && Input.GetTouch(0).phase == TouchPhase.Began);
        #else
             return (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1));
        #endif
    }
    //-----------------------------------------------------
    // TODO à remplacer par PC.In.Click1Up() || PC.In.Click2Up
    private bool ClickEnded()
    {
        #if UNITY_IPHONE || UNITY_ANDROID
             return (Input.touchCount>0 && (Input.GetTouch(0).phase == TouchPhase.Ended ||
                                            Input.GetTouch(0).phase == TouchPhase.Canceled));
        #else
             return (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1));
        #endif
    }

    //-----------------------------------------------------
    // TODO à remplacer par PC.In.Click1Up() || PC.In.Click2Up
    private bool ScrollBegin()
    {
        #if UNITY_IPHONE || UNITY_ANDROID
             return (Input.touchCount>0 && Input.GetTouch(0).phase == TouchPhase.Began);
        #else
             return (Input.GetAxis("Mouse ScrollWheel") != 0);
        #endif
    }

    #region public
    //-----------------------------------------------------
    public void ShowCtxHelp(string panelID)
    {
       /* if(PlayerPrefs.GetInt("HelpPopup") == 0)
            return;*/

        CtxHelpPanel newPanel;
        if(m_panels.TryGetValue(panelID, out newPanel))
        {
            ChangeHelpPanel(newPanel, panelID);
            //LaunchHelpCountdown();
        }
        /*else
        {
            Debug.Log(DBGTAG+"Unknown or removed help panel ID : \""+panelID+"\"");
        }*/
		
		//bhelp = true;
    }

    //-----------------------------------------------------
    public string[] GetPanelIDs()
    {
        string[] ids = new string[m_panels.Keys.Count];
        int i=0;
        foreach(string key in m_panels.Keys)
            ids[i++] = key;

        return ids;
    }

    //-----------------------------------------------------
    public bool PanelBlockingInputs()
    {
        return m_panelBlockingInputs;
    }

    //-----------------------------------------------------
    public void UseStartCountdown()
    {
        m_useStartCountdown = true;
    }
	
	//-----------------------------------------------------
	public void ReinitPanelsCountdown()
	{
		foreach(KeyValuePair<string, CtxHelpPanel> panel in m_panels)
			panel.Value.Reinit();	
	}
    #endregion

} // class CtxHelpManager
