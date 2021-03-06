using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pointcube.Global;
using Pointcube.Utils;

public class HelpPanel : MonoBehaviour
{
    // -- Réf. Scène --
    public  GameObject m_mainCam;

	private Texture2D m_helpTexture;
	private Texture2D _aboutBg;
	private Texture2D _upgradeBg;
	
	public string m_helpPanelname           = "HELP";
    public string m_helpPanelExpressName    = "HELP_EXPRESS";
	bool showHelp = false;
	
	string overrideTxt = "";
	
	Color help2Color = new Color(1,1,1,0);
	
    private Rect r_showHide;
	private Rect r_hlp2;
	private Rect r_hlp4;
    private Rect r_helpImageRect;
	private Rect m_unknownRect;		// Apparemment c'est le rectangle autour du bouton d'aide (en haut à droite)

	public GUIStyle hlpPanel2;
	public GUIStyle hlpBtn;
	public GUIStyle hlpBtn2;
	public GUIStyle hlpText;
	public GUIStyle hlpEmpty;
	public GUIStyle aboutTxt;
	public GUIStyle aboutTxtBold;
	public GUIStyle checkBox;
	public GUIStyle background;
	public GUISkin upgrade;
	
	private bool displayOverride = false;
	public bool _showAbout = false;
	public bool _showUpgrade = true;
	private bool _uiLocked = false;
	//private bool _doNotShowPopUpMaj = false;
	
	Vector2 upgradeScrollpos = Vector2.zero;
	
//	float lowPassWidthInSec = 1.0f;
//	
//	private float AccUpdateInterval = 1.0f/60.0f;
//	private float LowPassFilterFactor; 
//	private Vector3 lowPassValue = Vector3.zero;
//
//	private Vector3 IphoneAcc;
//	private Vector3 IphoneDeltaAcc;
//	
//	private float threshold = 1.25f;
//	private float time = 0f;
//	private float deltaTime = 1f;

	private static readonly string DEBUGTAG = "HelpPanel : ";
	private string m_szurl = "http://www.pointcube.com/activationOneshot/00_configs/maj/maj_";
	private string m_szmaj = "";
	
#region Unity_func
	// Use this for initialization

    //-----------------------------------------------------
    void Awake()
    {
        UsefullEvents.OnResizingWindow += SetRects;
        UsefullEvents.OnResizeWindowEnd += SetRects;
        UsefullEvents.HideGUIforScreenshot += UIOverride;
        UsefullEvents.HideGUIforBeforeAfter += UIOverride;
		UsefullEvents.LockGuiDialogBox  += LockUI;
    }

    //-----------------------------------------------------
	void Start ()
	{
//		LowPassFilterFactor = AccUpdateInterval / lowPassWidthInSec;
        if(m_mainCam == null) Debug.Log(DEBUGTAG+"Main Cam"+PC.MISSING_REF);

        r_hlp2      = new Rect();
        r_hlp4      = new Rect();
        r_helpImageRect = new Rect();
        r_showHide  = new Rect(0f, 0f, hlpBtn.fixedWidth, hlpBtn.fixedHeight);
		m_unknownRect = new Rect(Screen.width-60,10,50,50);
		
		string name;
        if(usefullData._edition == usefullData.OSedition.Lite)
            name = m_helpPanelExpressName;
        else
            name = m_helpPanelname;

		string prefix = "en";
		if(PlayerPrefs.HasKey("language"))
			prefix=PlayerPrefs.GetString("language");
#if UNITY_STANDALONE
		m_helpTexture = (Texture2D)Resources.Load("images_multilangue/"+prefix+"/"+name+"_PC");
#else
 		m_helpTexture = (Texture2D)Resources.Load("images_multilangue/"+prefix+"/"+name+"_TCH");
#endif
		_aboutBg = (Texture2D)Resources.Load("images_multilangue/"+"OS_FAB_BG");
		
		_upgradeBg = Resources.Load<Texture2D>("gui/Maj/MAJ HISTORY");

		SetRects();
	
		if(!PlayerPrefs.HasKey("PopupMaj") || PlayerPrefs.GetInt("PopupMaj")!=usefullData.version_forMaj  )
		{
			//PlayerPrefs.DeleteKey("PopupMaj");
			_showUpgrade = true;
		}
		else
			_showUpgrade = false;
		
		StartCoroutine("initPopupMaj");
	}
	
	IEnumerator initPopupMaj()
	{
		m_szurl += PlayerPrefs.GetString("language") + ".txt";
		//print ("CA CHARGE");
		WWW www = new WWW(m_szurl);
		yield return www;
		
		if(www.error != null)
		{
			Debug.Log(www.error);
			
			if(PlayerPrefs.HasKey("stringPopupMaj"))
			{
				m_szmaj = PlayerPrefs.GetString("stringPopupMaj");
			}
			else
			{
				m_szmaj = "";
			}
		}
		else
		{
			StringReader reader = new StringReader(www.text);
			
			string szmaj = "";
			string szline = "";
			
			do
			{
				szline = reader.ReadLine();
				
				if(szline != null)
				{
					szmaj += "\n" + szline;
				}
				
			}while(szline != null);
			
			if(!PlayerPrefs.HasKey("stringPopupMaj") || (PlayerPrefs.HasKey("stringPopupMaj") && PlayerPrefs.GetString("stringPopupMaj") != szmaj))
			{
				PlayerPrefs.SetString("stringPopupMaj", szmaj);
			}
			m_szmaj = PlayerPrefs.GetString("stringPopupMaj");
		}
	}
	
    //-----------------------------------------------------
	void Update ()
	{
//		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//		if(Time.time > time+deltaTime)
//		{
//			IphoneAcc = iPhoneInput.acceleration;
//			IphoneDeltaAcc = IphoneAcc-LowPassFilter(IphoneAcc);
//	
//			if(Mathf.Abs(IphoneDeltaAcc.magnitude)>=threshold)	
//			{
//				showHelp = !showHelp;
//				lowPassValue = Vector3.zero;
//				time = Time.time;
//			}
//		}
//		#endif

		if(_showUpgrade)
		{
			Rect tempRect = new Rect(Screen.width/2-_upgradeBg.width/2,Screen.height/2-_upgradeBg.height/2,
			                         _upgradeBg.width,_upgradeBg.height);

			if(
			#if UNITY_IPHONE
				Input.touchCount > 0
			#else
				Input.GetMouseButtonDown(0)
			#endif		
				&& !tempRect.Contains(Input.mousePosition))
			{
				_showUpgrade = false;
				
				GetComponent<GUIStart>().showStart(true);
				GetComponent<GUIStart>().setMenuMode(10, -1);
				
				PlayerPrefs.SetInt("PopupMaj", usefullData.version_forMaj);

			}
		}

		if(showHelp || _showAbout) // panneau d'aide principal
		{
			if(PC.In.Click1Up() && !PC.In.CursorOnUI(m_unknownRect))
			{
                if(showHelp /*|| _showAbout*/)
                    switchVisibility();
				if(_showAbout)
				{
					_showAbout = false;
					GetComponent<GUIStart>().enabled = true;
					GetComponent<GUIStart>().setMenuMode(10, -1);
				}
				/*if(_showUpgrade)
					_showUpgrade = false;*/
			}
		}
		
		if(GetComponent<GUIMenuMain>().isOneMenuOpen() || GetComponent<PoolResizerUI>().enabled 
			|| (GetComponent<GUISubTools>().enabled && !GameObject.Find("backgroundImage").GetComponent<PolygonTracer>() 
			&& !GetComponent<GUISubTools>().IsDisplayChoicePanelActivated()) ||
			GetComponent<StairControllerUI> ().enabled || GetComponent<GUIMenuConfiguration>().ShowHelpPannel) // Gestion du "Toucher ici pour valider"
		{
			if(help2Color.a < 1)
				help2Color.a += 2*Time.deltaTime;
		}
		else
		{
			if(help2Color.a > 0)
				help2Color.a -= 2*Time.deltaTime;		
		}
	}

    //-----------------------------------------------------
	void OnGUI()
	{         
		//GUI.depth = 100;
		if(displayOverride)
			return;
		if(showHelp)
		{
			GUI.Box(new Rect(0,0,Screen.width,Screen.height),"",background);

			GUI.BeginGroup(r_helpImageRect, m_helpTexture);
			//GUI.DrawTexture(r_helpImageRect, m_helpTexture);



			float ffactorTxtH = -0.225f;
			float ffactorTxtH2 = -0.085f;
			float ffactorTxtH3 = -ffactorTxtH2 - 0.0775f;
			float ffactorTxtH4 = -ffactorTxtH - 0.0815f;
			
			float ffactorTxtW = -0.087f;
			float ffactorTxtW2 = 0.285f;



			GUI.Label(new Rect(r_helpImageRect.width * ffactorTxtW,	r_helpImageRect.height * ffactorTxtH,	r_helpImageRect.width,	r_helpImageRect.height),"MOVE" ,hlpText);
			GUI.Label(new Rect(r_helpImageRect.width * ffactorTxtW2,	r_helpImageRect.height * ffactorTxtH,	r_helpImageRect.width,	r_helpImageRect.height),"ROTATE" ,hlpText);
			GUI.Label(new Rect(r_helpImageRect.width * ffactorTxtW,	r_helpImageRect.height * ffactorTxtH3,	r_helpImageRect.width,	r_helpImageRect.height),"SCALE" ,hlpText);
			GUI.Label(new Rect(r_helpImageRect.width * ffactorTxtW2,	r_helpImageRect.height * ffactorTxtH3,	r_helpImageRect.width,	r_helpImageRect.height),"HEIGHT" ,hlpText);
		
			//GUI.Label(new Rect(r_helpImageRect.width * ffactorTxtW,	r_helpImageRect.height * ffactorTxtH2,	r_helpImageRect.width,	r_helpImageRect.height),"LEFT BUTTON" ,hlpText);
			//GUI.Label(new Rect(r_helpImageRect.width * ffactorTxtW,	r_helpImageRect.height * ffactorTxtH4,	r_helpImageRect.width,	r_helpImageRect.height),"CENTER BUTTON" ,hlpText);
			//GUI.Label(new Rect(r_helpImageRect.width * ffactorTxtW2,	r_helpImageRect.height * ffactorTxtH2,	r_helpImageRect.width,	r_helpImageRect.height),"RIGHT BUTTON" ,hlpText);
			//GUI.Label(new Rect(r_helpImageRect.width * ffactorTxtW2,	r_helpImageRect.height * ffactorTxtH4,	r_helpImageRect.width,	r_helpImageRect.height),"LEFT + RIGHT" ,hlpText);
			
			int inumberBtn = 3;
			float ffactorPosW = 0.2f;
			float ffactorSizeW = (1.0f - (ffactorPosW * 2.0f)) / inumberBtn;
			int iidcurrentPosW = 0;
			float fposH = r_helpImageRect.height * 0.7137f;
			float ffactorSizeH = 0.035f;
			          
			if(GUI.Button(new Rect((r_helpImageRect.width * 0.3625f) - (r_helpImageRect.width * ffactorSizeW), fposH, r_helpImageRect.width * ffactorSizeW, r_helpImageRect.height * ffactorSizeH),
			TextManager.GetText("HelpPanel.video"),hlpBtn2))
			{
				Application.OpenURL("https://www.youtube.com/watch?v=-P9-EXoEJtE&list=PLwFAhIpbMJMS_Hb8S04KD_9qvWQ6nJz81");
			}
			if(GUI.Button(new Rect((r_helpImageRect.width * 0.5f) - (r_helpImageRect.width * ffactorSizeW * 0.5f), fposH, r_helpImageRect.width * ffactorSizeW, r_helpImageRect.height * ffactorSizeH),
			TextManager.GetText("HelpPanel.pdf"),hlpBtn2))
			{
				Application.OpenURL("http://www.pointcube.fr/images/00_tutos/os3d_utilisation_2014-02_fr.pdf");
			}
			
			if(GUI.Button(new Rect(r_helpImageRect.width * 0.6375f, fposH, r_helpImageRect.width * ffactorSizeW, r_helpImageRect.height * ffactorSizeH),
			              TextManager.GetText("HelpPanel.contactUs"),hlpBtn2))
			{
#if UNITY_IPHONE 
				EtceteraManager.setPrompt(true);
				EtceteraBinding.showMailComposer("info@pointcube.fr",
				                                 "Licence : " + PlayerPrefs.GetString(usefullData.k_logIn),
				                                 "Veuillez remplir les champs suivant :\nNom :\nNumero de Téléphone :\nNature du problème :", false);
				//Bien penser a sauver en UTF-8 pour garder les accentsd

#else
				Application.OpenURL("http://www.pointcube.fr/contact");
#endif	
				
			}
			
			GUI.EndGroup();
		}
		
		if(!GetComponent<LibraryLoader>() && _showUpgrade && m_szmaj != "")
		{
			GetComponent<GUIStart>().showStart(false);
			
			//Rect tempRect = new Rect(0.0f, 0.0f, 500.0f, 500.0f);
			Rect bgPopupUpgrade = new Rect(Screen.width/2-_upgradeBg.width/2,Screen.height/2-_upgradeBg.height/2,_upgradeBg.width,_upgradeBg.height);
			GUI.DrawTexture(bgPopupUpgrade, _upgradeBg);
			
			/*GUI.Label(new Rect(Screen.width/2-_upgradeBg.width/2+100,Screen.height/2-_upgradeBg.height/2+75,290,90),
			          "Mise à jour " + usefullData.version_ID + " :",aboutTxt); */
			
			//string sztxt = m_szmaj;
			
			GUISkin bkup = GUI.skin;
			GUI.skin = upgrade;
			
			float fwidth = 515.0f;
			float fheight = 414.0f;
			
			GUILayout.BeginArea(new Rect((Screen.width/2-_upgradeBg.width/2)+50, (Screen.height/2-_upgradeBg.height/2)+48, fwidth + 26.0f, fheight));
			upgradeScrollpos = GUILayout.BeginScrollView(upgradeScrollpos, false, true, GUILayout.Width(fwidth), GUILayout.Height(fheight));

			/*upgradeScrollpos = GUI.BeginScrollView(new Rect((Screen.width/2-_upgradeBg.width/2)+50, (Screen.height/2-_upgradeBg.height/2)+48, 515.0f, 414.0f),
			upgradeScrollpos, new Rect(0,0,0.0f,700));*/
			                               
	        GUILayout.Label(/*new Rect(0,0,500.0f,5000),*/
			                m_szmaj);
			
			/*if(GUILayout.Button("OK",GUILayout.Width(50), GUILayout.Height(50)))
			{
				_showUpgrade = false;
				
				GetComponent<GUIStart>().showStart(true);
				
				if(_doNotShowPopUpMaj)
				{
					PlayerPrefs.SetInt("PopupMaj", 1);
				}
			}*/
			
			GUILayout.EndScrollView();
			GUILayout.EndArea();
			 
			GUI.skin = bkup;
			/*_doNotShowPopUpMaj = GUI.Toggle(new Rect(bgPopupUpgrade.x + 75.0f, bgPopupUpgrade.y + bgPopupUpgrade.height - 140.0f, 50.0f, 50.0f),
											 _doNotShowPopUpMaj, "Ne plus afficher", checkBox);
			*/
			/*if(GUI.Button(new Rect(bgPopupUpgrade.x + bgPopupUpgrade.width - 180.0f, bgPopupUpgrade.y + bgPopupUpgrade.height - 140.0f, 100.0f, 50.0f),
			 "OK", hlpBtn2))
			{
				_showUpgrade = false;
				
				GetComponent<GUIStart>().showStart(true);
				
				if(_doNotShowPopUpMaj)
				{
					PlayerPrefs.SetInt("PopupMaj", 1);
				}
			}*/
		}
		
		if(_showAbout)
		{              
			GUI.DrawTexture(new Rect(Screen.width/2-_aboutBg.width/2,Screen.height/2-_aboutBg.height/2,_aboutBg.width,_aboutBg.height),_aboutBg);

            string text;
            if(usefullData._edition == usefullData.OSedition.Lite)
                text = TextManager.GetText("HelpPanel.os3dExpress");
            else
                text = TextManager.GetText("HelpPanel.configurateur3D");

			GUI.Label(new Rect((Screen.width/2) - (290/2) + 235.0f,(Screen.height/2) - (30/2) + 62.0f,290,30), "ONESHOT 3D v" + usefullData.version_ID, aboutTxtBold);
			
			GUI.Label(new Rect((Screen.width/2) - 290/2,(Screen.height/2) - (90/2) + 56.0f,290,90),
				TextManager.GetText("HelpPanel.idclient")+" "+PlayerPrefs.GetString(usefullData.k_logIn),aboutTxt); 
			//TextManager.GetText("HelpPanel.designedPointcube");
		}	

		GUI.depth = -1;

		if(!GetComponent<GUIStart>().enabled && !_showUpgrade && !_showAbout && 
		   !m_mainCam.GetComponent<Mode2D>().IsActive())
		{
			if(GUI.Button(r_showHide ,"Hlp",hlpBtn))
			{
				switchVisibility();
        	}
        }

		GUI.depth = 0;

       /* if(help2Color.a > 0f && !_uiLocked)       // Quick-fix biossun : bouton invisible mais cliquable et gênant
		{
    		GUI.color = help2Color;
    		if(overrideTxt != "")
    			GUI.Label(r_hlp2,overrideTxt,hlpPanel2);
    		else
    		{
    #if UNITY_IPHONE || UNITY_ANDROID
    			GUI.Label(r_hlp2,TextManager.GetText("HelpPanel.TouchHere"),hlpPanel2);
    #else
    			GUI.Label(r_hlp2,TextManager.GetText("HelpPanel.TouchHerePC"),hlpPanel2);
    #endif
    			if(GUI.Button(r_hlp4, "",hlpEmpty) )
    			{
                    if (GetComponent<GUIMenuConfiguration>().ShowHelpPannel)
                    {
                        Debug.Log("show helppannel to false");
						GetComponent<GUIMenuConfiguration>().ShowHelpPannel = false;
                    }
                    else
					{
                        GameObject mainnode = GameObject.Find("MainNode");
                        if (mainnode != null)
                        {
                            ObjBehavGlobal objBehavGlobal = mainnode.GetComponent<ObjBehavGlobal>();
                            if (objBehavGlobal != null)
                            {
                                ObjInteraction objInteraction = Camera.mainCamera.GetComponent<ObjInteraction>();
                                if (objInteraction != null)
                                {
                                    //if(!objInteraction.isDragging())
                                    {
                                        Avatar.Locked = true;
                                        objInteraction.setSelected(null);
                                        //		Debug.Log("Deselection");
                                    }
                                }
                            }
                        }
                    }
    			}
    		}
		    GUI.color = Color.white;
        } // if help2Color pas transparent*/
		
	}

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow -= SetRects;
        UsefullEvents.OnResizeWindowEnd -= SetRects;
        UsefullEvents.HideGUIforScreenshot  -= UIOverride;
        UsefullEvents.HideGUIforBeforeAfter -= UIOverride;
		UsefullEvents.LockGuiDialogBox  -= LockUI;
    }

#endregion
    
    //-----------------------------------------------------
    private void SetRects()
    {
        r_showHide.x = Screen.width  - r_showHide.width;

        r_hlp4.Set(Screen.width/2-100,175,200,50);
        r_hlp2.Set(Screen.width/2-200,0,400,400);

        // -- Redimensionnement "Touch Here" si besoin --
        int minDim = (int) ((Screen.width < Screen.height)? Screen.width/1.5f : Screen.height/1.5f);
        if(minDim < 196) minDim = 196;      // Taille minimale
        if(minDim < r_hlp2.height)
        {
            r_hlp2.width  = minDim;
            r_hlp2.height = minDim;
            r_hlp2.x = (Screen.width-r_hlp2.height)/2;
        }

        if(m_helpTexture != null)
        {
			r_helpImageRect = new Rect((Screen.width * 0.5f) - (m_helpTexture.width * 0.5f),
										 (Screen.height * 0.5f) - (m_helpTexture.height * 0.5f),
			                           m_helpTexture.width,
			                           m_helpTexture.height);
			
			
			/*ImgUtils.ResizeImagePreservingRatio(m_helpTexture.width,
			                                                      m_helpTexture.height,
                                                                  Screen.width,
                                                                  Screen.height);*/
        }
    }
	
	private void LockUI(bool isLck)
	{
		_uiLocked = isLck;
	}

//	Vector3 LowPassFilter(Vector3 newSample)
//	{
//        lowPassValue = Vector3.Lerp(lowPassValue, newSample, LowPassFilterFactor);
//        return lowPassValue;
//
//	}
	
	public void showUpgrade()
	{
		_showUpgrade = true;
		upgradeScrollpos = Vector2.zero;
	}
	
	public void showOverride()
	{
		showHelp = true;
	}
	
	public void switchVisibility()
	{
		/*if(_uiLocked)
			return;*/
		showHelp = !showHelp;
		
		if(showHelp)
		{
			_showAbout = false;
			GetComponent<GUIMenuMain>().SetHideAll(true);
            m_mainCam.GetComponent<Mode2D>().HideUI(true);
			UsefullEvents.FireShowHelpPanel();
		}
		else
		{
			GetComponent<GUIMenuMain>().SetHideAll(false);
            m_mainCam.GetComponent<Mode2D>().HideUI(false);
		}
	}
	
	public void switchAbout()
	{
		_showAbout = !_showAbout;
		if(_showAbout)
		{
			GetComponent<GUIMenuMain>().SetHideAll(true);
			GetComponent<GUIStart>().enabled = false;
		}
		else
		{
			GetComponent<GUIMenuMain>().SetHideAll(false);
		}
	}
	
	public bool isHelpVisible()
	{
		return showHelp;	
	}
	
	public void set2ndHelpTxt(string txt)
	{
		overrideTxt = txt;
	}
	
	void UIOverride(bool show)
	{
		displayOverride = show;	
	}
	
	public bool IsOnGUI()
	{
		if(help2Color.a > 0f && !_uiLocked)
			return PC.In.CursorOnUI(r_hlp4);
		else
			return false;
	}
} // class HelpPanel
