using UnityEngine;
using System.Collections;
using System.IO;

using Pointcube.Global;
using Pointcube.InputEvents;
using Pointcube.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif
//if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//using System.Windows.Forms;
//endif

public class ExplorerV2 : MonoBehaviour
{
	#region Attributs

    string[] m_clients;
	string[] m_projects;
	string m_currentPath;
	string m_rootPath;

	public Texture backGroundTop;
	public GUISkin skin;
	
	GUIDialogBox gui_cb;
	
	GUIArrows arrows;
	
	public static ContextDataModel cdm;
	
	AsyncOperation async;

    private Rect r_group1;

	private Rect r_fullPanelBG;
	private Rect r_fullPanel;
	private Rect r_fullPanelSV;
	
	private Rect r_halfPanelLeft;
	private Rect r_halfPanelLeftSV;
	
	private Rect r_halfPanelRight;

    // -- Progress Bar --
    private Rect r_loadBarFond;
    private Rect r_loadBarGrpMv;
    private Rect r_loadBar;

    // -- Flèches --
	private Rect r_leftPanelArrows;
	private Rect r_fullPanelBGArrows;        // TODO private
    private Rect r_backButton;
    private bool m_useClientSkin;            // true = utiliser le skin "client", false = skin projets

	Vector2 sp_fullPanel = Vector2.zero;
	Vector2 sp_halfPanelLeft = Vector2.zero;
	
	int id_selectedClient = -1;
	int id_selectedProject = -1;
	int id_clientToSuppress = -1;
	
	bool byPassGui = false;
	bool newFilePanel = false;
	bool suppressionMode = false;
	bool tryToCreateClient = false;
	bool tryToDeleteProject = false;
	bool isComments = false;
	bool autoSelect = false;
	bool renaming = false;
	
	private bool m_bexplorerInGame = false;
	private bool bnewSave = false;
	private bool bsave = false;
	private bool bsaveRequest = false;
	
	private string szsaveName = "";
	private string szsaveComment = "";

    public GameObject m_background;
	Texture bkUp;

    private static readonly string DEBUGTAG = "ExplorerV2";
    private static readonly bool   DEBUG    = true;
	
	//Equivalent des anciennes scnènes
	GameObject _explorer;
	GameObject _mainWithLibs; // Tu abuses Filipe

	private static Texture2D _staticRectTexture;
	private static GUIStyle _staticRectStyle;
	
	// Note that this function is only meant to be called from OnGUI() functions.
	public static void GUIDrawRect( Rect position, Color color )
	{
		if( _staticRectTexture == null )
		{
			_staticRectTexture = new Texture2D( 1, 1 );
		}
		
		if( _staticRectStyle == null )
		{
			_staticRectStyle = new GUIStyle();
		}
		
		_staticRectTexture.SetPixel( 0, 0, color );
		_staticRectTexture.Apply();
		
		_staticRectStyle.normal.background = _staticRectTexture;
		
		GUI.Box( position, GUIContent.none, _staticRectStyle );
		
		
	}

	#endregion
	
	#region UNITY's fcns
    //-----------------------------------------------------
    void Awake()
    {
        UsefullEvents.OnResizingWindow  += SetRects;
        UsefullEvents.OnResizeWindowEnd += SetRects;
    }

    //-----------------------------------------------------
	void Start ()
	{
        if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log(DEBUGTAG+"Start");

        if(m_background == null)
            Debug.LogError(DEBUGTAG+"Background not set, please set it in the inspector.");

        // -- Progress bar --
        r_loadBarFond = new Rect(162,UnityEngine.Screen.height*0.865f,UnityEngine.Screen.width-162*2,4); // new Rect(162,664,700,4);
        r_loadBarGrpMv = new Rect(162,UnityEngine.Screen.height*0.865f,0,4);                 // new Rect(162,664,0,4);
        r_loadBar = new Rect(0,0,UnityEngine.Screen.width-162*2f,4);                         // new Rect(0,0,700,4);

		PlayerPrefs.SetString(usefullData.k_toLoadPath,"");
		PlayerPrefs.SetString(usefullData.k_toLoadParams,"");
		PlayerPrefs.SetString(usefullData.k_startBypass,"");
		//PlayerPrefs.SetString(usefullData.k_selectedClient,"");
#if UNITY_IPHONE
		EtceteraManager.mailComposerFinishedEvent += IOutils.EndMail;
#endif
		
		m_rootPath = usefullData.SavePath;
		m_currentPath = m_rootPath;
		m_projects = new string[0];
		gui_cb = GetComponent<GUIDialogBox>();
		
		updateClientList();

		arrows = GetComponent<GUIArrows>();
		arrows.setDepth(GUI.depth);

        // -- Rects GUI --
        r_group1            = new Rect();

        r_fullPanelBG       = new Rect(162, 134, 700, 500);
        r_fullPanel         = new Rect(200,150,624,468);
        r_fullPanelSV       = new Rect(0,0,624,468);

        r_halfPanelLeft     = new Rect(162,134+50,350,500-50);
        r_halfPanelLeftSV   = new Rect(0,0,350,500);
        r_backButton        = new Rect(r_halfPanelLeft.x-30, r_halfPanelLeft.y+(r_halfPanelLeft.height-350)/2,30,350);

        r_halfPanelRight    = new Rect(162+350,134,350,500);

        r_leftPanelArrows   = new Rect();
        r_fullPanelBGArrows = new Rect();
        m_useClientSkin = true;

        SetRects();

		if(PlayerPrefs.HasKey(usefullData.k_selectedClient))
		{
			string cl = PlayerPrefs.GetString(usefullData.k_selectedClient);
			if(cl != "")
			{
				for(int i=0;i<m_clients.Length;i++)
				{
					if(m_clients[i] == cl)
					{
						id_selectedClient = i;
						id_selectedProject = -1;
						cdm = null;
						m_currentPath = m_rootPath + m_clients[id_selectedClient];

						arrows.setArrowsRect(r_leftPanelArrows,"projetUp","projetDwn");
                        m_useClientSkin = false;

						updateProjectList();
						if(m_projects.Length>0)
							autoSelect = true;
					}
				}
			}
		}
		updateClientList();		
	} // Start()
	
	//-----------------------------------------------------
	void Update ()
	{
		//if(m_bexplorerInGame)
		//{
			Rect tempRect = new Rect(	(Screen.width * 0.5f) - (r_fullPanelBG.width * 0.5f) - 25.0f,
										(Screen.height * 0.5f) - (r_fullPanelBG.height * 0.5f),
										r_fullPanelBG.width + 25.0f, r_fullPanelBG.height + 60.0f);
										
										
			bool bclic = false;
			
			if(
#if UNITY_IPHONE
			Input.touchCount > 0
#else
			Input.GetKey(KeyCode.Mouse0)
#endif		
			&& !tempRect.Contains(Input.mousePosition))
			{
				if(m_bexplorerInGame)
				{
					GameObject mainScene = GameObject.Find ("MainScene");			
					if(mainScene!=null)
					{
						GUIStart guiStart = mainScene.GetComponent<GUIStart>();
						if(guiStart!=null)
						{
							suppressionMode = false;
							guiStart.quitExplorerInGame();
						}
					}
				}
				else
				{
					suppressionMode = false;
					SwitchToMainWithLibs();
				}
			}
		//}
	}
	
	//-----------------------------------------------------
	void OnGUI()
	{

		if(bsaveRequest)
		{
			if(szsaveName == "")
			{
				szsaveName = "Sans nom";
			}
			
			if(bnewSave)
			{
				int i = 0;
				bool bcheck = false;
				string sztemp = szsaveName;
				
				do
				{
					bcheck = true;
					
					foreach(string sz in m_projects)
					{
						if(sz == szsaveName)
						{
							szsaveName = sztemp + " (" + (++i).ToString() + ")";
							bcheck = false;
						}
					}
					
				}while(!bcheck);
			}
			
			GameObject.Find("MainScene").GetComponent<Montage>().m_client = m_clients[id_selectedClient];
			GameObject.Find("MainScene").GetComponent<Montage>().setComment(szsaveComment);
			StartCoroutine(GameObject.Find("MainScene").GetComponent<Montage>().SaveInGame(szsaveName));
			bsaveRequest = false;
		}
		
		if(byPassGui)
			return;
		if(arrows!=null)
			arrows.setDepth(GUI.depth);
		GUI.BeginGroup(r_group1);
		#region GUI GENERALE
		GUI.DrawTexture(new Rect(r_fullPanelBG.x,r_fullPanelBG.y-50,r_fullPanelBG.width,50), backGroundTop);
		GUI.skin = skin;
		
		/*if(m_bexplorerInGame)
		{
			if(GUI.Button(new Rect(r_fullPanelBG.x,r_fullPanelBG.y-50,65,40),TextManager.GetText("ExplorerV2.Mounting"),"noArrow"))
			{
				GameObject mainScene = GameObject.Find ("MainScene");			
				if(mainScene!=null)
				{
					GUIStart guiStart = mainScene.GetComponent<GUIStart>();
					if(guiStart!=null)
					{
						guiStart.quitExplorerInGame();
					}
				}
			}
		}
		else 
		{
			/*if(GUI.Button(new Rect(r_fullPanelBG.x,r_fullPanelBG.y-50,65,40),TextManager.GetText("ExplorerV2.Home"),"noArrow") 
				&& !gui_cb.isVisible()
				&& async == null)
			{
				SwitchToMainWithLibs();
				//StartCoroutine(LoadLevel());
			}*/
		//}
	//	print (id_selectedClient);
		if(id_selectedClient !=-1)
		{
			if(GUI.Button(new Rect(r_fullPanelBG.x,r_fullPanelBG.y-50,50,50),"","arrow") && !gui_cb.isVisible() )
			{
				id_selectedClient = -1;
				arrows.setArrowsRect(r_fullPanelBGArrows,"clientUp","clientDwn");
	            m_useClientSkin = true;
			}
		}
		if(id_selectedClient != -1)
		{
			/*if(GUI.Button(new Rect(r_fullPanelBG.x+120+100,r_fullPanelBG.y-50,120,40),TextManager.GetText("ExplorerV2.Projects"),"arrow") && !gui_cb.isVisible())
			{
			}*/
		}		
		#endregion
		
		#region Panneau clients
		float falphaColor = 0.8f;
		if(id_selectedClient == -1)
		{
			Color bkupColor = GUI.color;
			GUI.color = new Color(bkupColor.a, bkupColor.g, bkupColor.b, falphaColor);
			GUI.Box(r_fullPanelBG,"","clientPanelBg");
			GUI.DrawTexture(new Rect(r_fullPanelBG.x,r_fullPanelBG.y-50,r_fullPanelBG.width,50), backGroundTop);

			GUI.color = bkupColor;
			
			sp_fullPanel = GUI.BeginScrollView(r_fullPanel,sp_fullPanel,r_fullPanelSV);
			string style = "fullPanelSG";
			if(suppressionMode)
				style = "fullPanelSGSuppr";
			int tmpId = GUI.SelectionGrid(r_fullPanelSV,id_selectedClient,m_clients,4,style);
			
			GUI.EndScrollView();
			
			bkupColor = GUI.color;
			GUI.color = new Color(bkupColor.a, bkupColor.g, bkupColor.b, falphaColor);
			GUI.Box(new Rect(r_fullPanelBG.x,r_fullPanelBG.y/*-10*/,r_fullPanelBG.width,30),"","faderTop");
			GUI.Box(new Rect(r_fullPanelBG.x,r_fullPanelBG.yMax-100,r_fullPanelBG.width,100),"","faderBtm");
			GUI.color = bkupColor;

			Rect[] temp = new Rect[1];
			temp[0] = r_fullPanelBG;
			temp[0].y -=50;
			temp[0].height +=50;
			//GUIDrawRect(temp[0],new Color(1.0f,0.0f,1.0f,1.0f));
			/*if(GUI.Button(r_backButton,"","backArrow") && !gui_cb.isVisible() || !PC.In.ClickOnUI(temp) && PC.In.Click1Down())
			{
			
				if(!m_bexplorerInGame)
				{
					SwitchToMainWithLibs();
				}
				else
				{
					GameObject mainScene = GameObject.Find ("MainScene");			
					if(mainScene!=null)
					{
						GUIStart guiStart = mainScene.GetComponent<GUIStart>();
						if(guiStart!=null)
						{
							guiStart.quitExplorerInGame();
						}
					}
				}
			}*/
			
			if(suppressionMode) // Mode Suppression de clients
			{
				if(tmpId != -1)
				{
					id_clientToSuppress = tmpId;
					gui_cb.showMe(true,GUI.depth);
					gui_cb.setText(TextManager.GetText("ExplorerV2.Delete")+" "+m_clients[id_clientToSuppress]+" ?");
				}
				
				if(id_clientToSuppress != -1)
				{
					
					if(gui_cb.getConfirmation())
					{
						Directory.Delete(m_rootPath+m_clients[id_clientToSuppress],true);//Suppression
						m_currentPath = m_rootPath;
						id_clientToSuppress = -1;
						id_selectedProject = -1;
						cdm = null;
						updateClientList();
						updateProjectList();
						gui_cb.showMe(false,GUI.depth);
					}
					if(gui_cb.getCancel())
					{
						id_clientToSuppress = -1;
						gui_cb.showMe(false,GUI.depth);
					}
				}
			}
			else if( !gui_cb.isVisible())// Mode Sélection de clients
			{
				if(tmpId != id_selectedClient)
				{
					id_selectedClient = tmpId;
					id_selectedProject = -1;
					cdm = null;
	//				_tryDeleteFile = false;
	//				gui_cb.showMe(false,GUI.depth);
					m_currentPath = m_rootPath + m_clients[id_selectedClient];
					arrows.setArrowsRect(r_leftPanelArrows,"projetUp","projetDwn");
                    m_useClientSkin = false;
					updateProjectList();
					if(m_projects.Length>0)
						autoSelect = true;
					
					bnewSave = false;
					bsave = false;
					bsaveRequest = false;
					szsaveName = "";
					szsaveComment = "";
				}
			}
			
#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
//			if(GUI.Button(new Rect(r_fullPanelBG.xMax-430,r_fullPanelBG.y-50,160,40),
//				TextManager.GetText("ExplorerV2.ImportProject"),"new"))O
//			{
//				importFile();
//			}
#endif
			if((GUI.Button(new Rect(r_fullPanelBG.xMax-270,r_fullPanelBG.y-50,160,40),TextManager.GetText("ExplorerV2.NewClient"),"new")  && !gui_cb.isVisible())|| tryToCreateClient)
			{
				if(!tryToCreateClient)
				{
					tryToCreateClient = true;
					gui_cb.showMe(true,GUI.depth,true);
					gui_cb.setBtns("Ok","Annuler");
					gui_cb.setText(TextManager.GetText("ExplorerV2.New"));

					suppressionMode = false;
				}
				
				if(gui_cb.getConfirmation())
				{
					if(gui_cb.getInputTxt() != "")
					{
						string newClient = gui_cb.getInputTxt();
						
						Directory.CreateDirectory(m_rootPath+newClient);
						updateClientList();
						
						tryToCreateClient = false;
						gui_cb.showMe(false,GUI.depth);
					}
					else
					{
						gui_cb.setText(TextManager.GetText("ExplorerV2.InvalidName"));
						gui_cb.resetBtns();
					}
				}
				
				if(gui_cb.getCancel())
				{
					tryToCreateClient = false;
					gui_cb.showMe(false,GUI.depth);
				}
			}
			string textDelete = TextManager.GetText("ExplorerV2.Delete");
			if(suppressionMode)
				textDelete = TextManager.GetText("ExplorerV2.OK");
		
			bool tmpSup = GUI.Toggle(new Rect(r_fullPanelBG.xMax-130,r_fullPanelBG.y-50,110,40),
				suppressionMode,textDelete,"suppress");
			if(!gui_cb.isVisible() && suppressionMode != tmpSup)
			{
				suppressionMode = tmpSup;
			}

		}
		#endregion
		
		#region Panneau projets
		else
		{
			//RENAMING
			//if(!m_bexplorerInGame)
			//{
				if((GUI.Button(new Rect(r_fullPanelBG.x+200,r_fullPanelBG.y-50,100,40),TextManager.GetText("ExplorerV2.Rename"),"rename")  && !gui_cb.isVisible())|| renaming)
				{
					if(!renaming)
					{
						renaming = true;
						gui_cb.showMe(true,GUI.depth,true);
						gui_cb.setBtns("Ok","Annuler");
						gui_cb.setText(TextManager.GetText("ExplorerV2.NewName"));
					}
					
					if(gui_cb.getConfirmation())
					{
						string newName = gui_cb.getInputTxt();
						print (newName);
						if(IOutils.RenameDirectory(m_rootPath+m_clients[id_selectedClient],newName))
						{
							updateClientList();
							id_selectedClient = -1;
							for (int i = 0; i < m_clients.Length; i++)
							{
								if(m_clients[i] == newName)
									id_selectedClient = i;
							}
							if(id_selectedClient != -1)
							{
								cdm = null;
								m_currentPath = m_rootPath + m_clients[id_selectedClient];
								arrows.setArrowsRect(r_leftPanelArrows,"projetUp","projetDwn");
			                    m_useClientSkin = false;
								updateProjectList();
								if(m_projects.Length>0)
									autoSelect = true;	
							}						
						}
						renaming = false;
						gui_cb.showMe(false,GUI.depth);
					}
					
					if(gui_cb.getCancel())
					{
						renaming = false;
						gui_cb.showMe(false,GUI.depth);
					}
				}
			//}
			
			//PANNEAU DE GAUCHE
			
			Color bkupColor = GUI.color;
			GUI.color = new Color(bkupColor.a, bkupColor.g, bkupColor.b, falphaColor);
			if(newFilePanel || id_selectedProject ==-1 || m_bexplorerInGame)
			{
				GUI.Box(r_fullPanelBG,"","projPanelBGEmpty");
			}
			else
			{
				GUI.Box(r_fullPanelBG,"","projPanelBG");
			}

			GUI.color = bkupColor;
//			GUI.Box(r_halfPanelLeft,"");
			if(GUI.Button(new Rect(r_halfPanelLeft.x,r_halfPanelLeft.y-50,r_halfPanelLeft.width,50),
				m_clients[id_selectedClient],"projetClientTitle")  && !gui_cb.isVisible()) // btn avec le nom du client qui permet de revenir a la liste des clients
			{
				id_selectedClient = -1;
				arrows.setArrowsRect(r_fullPanelBGArrows,"clientUp","clientDwn");
                m_useClientSkin = true;
			}
		
			if(GUI.Button(r_backButton,"","backArrow") && !gui_cb.isVisible())
			{
				id_selectedClient = -1;
				arrows.setArrowsRect(r_fullPanelBGArrows,"clientUp","clientDwn");
                m_useClientSkin = true;
			}
			
			sp_halfPanelLeft = GUI.BeginScrollView(r_halfPanelLeft,sp_halfPanelLeft,r_halfPanelLeftSV);
			int tmpId = GUI.SelectionGrid(r_halfPanelLeftSV,id_selectedProject,m_projects,1,"halfPanelSG");
			GUI.EndScrollView();
			GUI.Box(new Rect(r_halfPanelLeft.x,r_halfPanelLeft.yMax-100,r_halfPanelLeft.width,100),"FADER","halfFader");
			if(tmpId != id_selectedProject && !PC.In.CursorOnUI(r_leftPanelArrows))//isOnRect(r_leftPanelArrows)) // SELECTION D'UN PROJET
			{
				id_selectedProject = tmpId;
				PlayerPrefs.SetString(usefullData.k_selectedProject,m_projects[id_selectedProject]);
				m_currentPath = m_rootPath + m_clients[id_selectedClient]+"/"+ m_projects[id_selectedProject];
				newFilePanel = false;
				bsave = false;
				StartCoroutine(preload());
			}
			else if(autoSelect)
			{
				id_selectedProject = 0;
				PlayerPrefs.SetString(usefullData.k_selectedProject,m_projects[id_selectedProject]);
				autoSelect = false;
				m_currentPath = m_rootPath + m_clients[id_selectedClient]+"/"+ m_projects[id_selectedProject];
				newFilePanel = false;
				StartCoroutine(preload());
			}
			
			//Fleches défilement liste
//			GUI.Box(r_leftPanelArrows,"");
//			GUI.BeginGroup(r_leftPanelArrows);
//			if(GUI.Button(new Rect(0,0,r_halfPanelLeft.width/2,50),"^"))
//			{
//				Debug.Log("AV"+sp_halfPanelLeft.y);
//				sp_halfPanelLeft.y -= 50;
//				if(sp_halfPanelLeft.y <0)
//					sp_halfPanelLeft.y = 0;
//				Debug.Log("AP"+sp_halfPanelLeft.y);
//			}
//			if(GUI.Button(new Rect(r_halfPanelLeft.width/2,0,r_halfPanelLeft.width/2,50),"v"))
//			{
//				sp_halfPanelLeft.y += 50;
//				if(sp_halfPanelLeft.y > r_halfPanelLeftSV.height)
//					sp_halfPanelLeft.y = r_halfPanelLeftSV.height;
//			}
//			GUI.EndGroup();
			if(m_projects.Length > 0)
			{	
				if(!m_bexplorerInGame)
				{
					if(GUI.Button(new Rect(r_fullPanelBG.xMax-290,r_fullPanelBG.y-50,180,40),TextManager.GetText("ExplorerV2.NewAssembly"),"new")  && !gui_cb.isVisible())
					{
						newFilePanel = true;
						id_selectedProject = -1;
						cdm = null;
					}
				}
					
				if((GUI.Button(new Rect(r_fullPanelBG.xMax-130,r_fullPanelBG.y-50,110,40),TextManager.GetText("ExplorerV2.Delete"),"suppress")  && !gui_cb.isVisible())|| tryToDeleteProject)
				{
					if(!tryToDeleteProject)
					{
						if(id_selectedProject != -1)
						{
							tryToDeleteProject = true;
							gui_cb.showMe(true,GUI.depth);
							gui_cb.setText(TextManager.GetText("ExplorerV2.Delete")+" "+m_projects[id_selectedProject]+" ?");
						}
						else
						{
							tryToDeleteProject = true;
							gui_cb.ShowJustOkBox(true,GUI.depth);
							gui_cb.setBtns(TextManager.GetText("ExplorerV2.OK"),"");
							gui_cb.setText(TextManager.GetText("ExplorerV2.SelectProjectList"));
						}
					}
					if(gui_cb.getConfirmation())
					{
						if(id_selectedProject != -1)
						{
							IOutils.DeleteMontageFile(m_currentPath);
							
							m_currentPath = m_rootPath + m_clients[id_selectedClient];
							id_selectedProject = -1;
							cdm = null;
							updateProjectList();
							
							tryToDeleteProject = false;
							gui_cb.showMe(false,GUI.depth);
						}
						else
						{
							tryToDeleteProject = false;
							gui_cb.ShowJustOkBox(false,GUI.depth);
						}
					}
					if(gui_cb.getCancel())
					{
						if(id_selectedProject != -1)
						{
							tryToDeleteProject = false;
							gui_cb.showMe(false,GUI.depth);
						}
						else
						{
							gui_cb.ShowJustOkBox(false,GUI.depth);
						}
					}
				}
			}
			
			//PANNEAU DE DROITE
//			GUI.Box(r_halfPanelRight,"");
			GUI.BeginGroup(r_halfPanelRight);
			
			if(newFilePanel)//PANNEAU DE NOUVEAU PROJET
			{
				if(m_bexplorerInGame)
				{
					bnewSave = true;
					bsave = true;
					
					szsaveName = "";
					szsaveComment = "";
					
					newFilePanel = false;
				}
				else
				{
					GUI.Label(new Rect(25,10,r_halfPanelRight.width-50,30),TextManager.GetText("ExplorerV2.NewAssemblyBold"),"blueTxtCentre");
					#if UNITY_IPHONE || UNITY_ANDROID
					if(GUI.Button(new Rect(25,50,r_halfPanelRight.width-50,50),TextManager.GetText("ExplorerV2.takePicture"),"rgtPanelBtn")
						&& async == null)
					{
						//PlayerPrefs.SetString(usefullData.k_startBypass,"photo");
						StartCoroutine(TakePicture());
						//StartCoroutine(LoadLevel());
					}
					#endif
					if(GUI.Button(new Rect(0,100,r_halfPanelRight.width-0,50),TextManager.GetText("ExplorerV2.loadPicture"),"rgtPanelBtn")
						&& async == null)
					{
						StartCoroutine(LoadPicture());
						//StartCoroutine(LoadLevel());
					}
					if(GUI.Button(new Rect(0,150,r_halfPanelRight.width-0,50),TextManager.GetText("ExplorerV2.examplePicture"),"rgtPanelBtn")
						&& async == null)
					{
						//PlayerPrefs.SetString(usefullData.k_startBypass,"exemple");
						StartCoroutine(ExamplePicture());
						//StartCoroutine(LoadLevel());
					}				
					#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
					if(GUI.Button(new Rect(0,200,r_halfPanelRight.width-0,50),TextManager.GetText("ExplorerV2.ImportProject"),"rgtPanelBtn")
						&& async == null)
					{
	                    m_currentPath = m_currentPath.Substring(0, m_currentPath.LastIndexOf('/'));
						string importPath = IOutils.ImportMontageFile(m_currentPath);
	                    if(!importPath.Equals(m_currentPath))
	                    {
	                        m_currentPath = importPath;
	                        updateProjectList();
	                        //PlayerPrefs.SetString(usefullData.k_toLoadPath, m_currentPath);
	                        //PlayerPrefs.SetString(usefullData.k_toLoadParams, "True");											
							StartCoroutine(LoadProject(true));	
	                        //StartCoroutine(LoadLevel());
	                    }
					}
	                #endif
	           	}
			}
			else if(cdm != null || m_bexplorerInGame)//PANNEAU DE PREVIEW PROJET
			{
				if(!bsave && cdm != null)
				{
					GUI.DrawTexture(new Rect (25,25,300,225), cdm.m_thumbnail);
					if(m_projects.Length>id_selectedProject)
						GUI.Label(new Rect (25,25,300,225),m_projects[id_selectedProject],"previewFader");
					
	//				GUI.Label(new Rect(15,180,260,20),cdm.m_owner);
					GUI.Label(new Rect(25,270,260,20),TextManager.GetText("ExplorerV2.createdThe")+" : "+cdm.m_birthDate,"blueTxt");
					GUI.Label(new Rect(25,300,300,4),"","separator");
					
					if(!isComments)
						GUI.Box (new Rect (25, 310, 270, 80), TextManager.GetText("ExplorerV2.Comments")+" :\n" + cdm.m_comments,"blueTxt");
					else
						GUI.Box (new Rect (25, 25, 300, 365), TextManager.GetText("ExplorerV2.Comments")+" :\n" + cdm.m_comments,"blueTxt2");
					isComments = GUI.Toggle(new Rect(295,370,20,20),isComments,"","loupe");
				}
				
				if(bsave)
				{
					GUI.Box (new Rect (25, 25, 300, 365), TextManager.GetText("Montage.Name") + " :","blueTxt2");
					
					if(bnewSave)
					{
						szsaveName = GUI.TextField (new Rect (0, 45, 400, 50), szsaveName,"txtField");
					}
					else
					{
						GUI.Label(new Rect (25, 65, 400, 50),szsaveName,"blueTxt2");
					}
					
					GUI.Label(new Rect(25,110,300,4),"","separator");
					GUI.Box (new Rect (25, 130, 300, 365), TextManager.GetText("Montage.Comments") + " :","blueTxt2");
					szsaveComment = GUI.TextArea (new Rect (0, 150, 400, 200), szsaveComment,"txtArea");
					
					if(cdm != null)
					{
						if(GUI.Button(new Rect(0,r_halfPanelRight.yMax-100-r_halfPanelRight.y,350,50),"Retour","rgtPanelBtn")
						   && async == null)
						{
							bsave = false;
						}
					}
					
					if(GUI.Button(new Rect(0,r_halfPanelRight.yMax-50-r_halfPanelRight.y,350,50),"Sauvegarder","rgtPanelBtn")
					   && async == null)
					{

						GameObject mainScene = GameObject.Find ("MainScene");
						mainScene.GetComponent<GUIMenuRight>().SaveSceneControl();

						byPassGui = true;
						bsaveRequest = true;
						arrows.enabled = false;
					}
				}
				else if(!m_bexplorerInGame)
				{
					if(GUI.Button(new Rect(0,r_halfPanelRight.yMax-100-r_halfPanelRight.y,175,50),TextManager.GetText("ExplorerV2.LoadProject"),"rgtPanelBtn")
						&& async == null)
					{
						StartCoroutine(LoadProject(true));	
						//StartCoroutine(LoadLevel());
					}
					
					if(GUI.Button(new Rect(175,r_halfPanelRight.yMax-100-r_halfPanelRight.y,175,50),TextManager.GetText("ExplorerV2.LoadWithoutObjects"),"rgtPanelBtn")
						&& async == null)
					{
						//PlayerPrefs.SetString(usefullData.k_toLoadPath,m_currentPath);
						//PlayerPrefs.SetString(usefullData.k_toLoadParams,"False");
						
						StartCoroutine(LoadProject(false));	
						//StartCoroutine(LoadLevel());
					}
					
					
					#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
					if(GUI.Button(new Rect(0,r_halfPanelRight.yMax-50-r_halfPanelRight.y,350,50),TextManager.GetText("ExplorerV2.SendProject"),"rgtPanelBtn"))
					{
						IOutils.EmailMontageFile(m_currentPath);
					}
	                #else
				
				
	    			if(GUI.Button(new Rect(0,r_halfPanelRight.yMax-50-r_halfPanelRight.y,175,50),TextManager.GetText("ExplorerV2.ImportProject"),"rgtPanelBtn"))
	                {
	                    m_currentPath = m_currentPath.Substring(0, m_currentPath.LastIndexOf('/'));
	                    string importPath = IOutils.ImportMontageFile(m_currentPath);
	                    if(!importPath.Equals(m_currentPath))
	                    {
	                        m_currentPath = importPath;
	                        updateProjectList();
	                        //PlayerPrefs.SetString(usefullData.k_toLoadPath, m_currentPath);
	                        //PlayerPrefs.SetString(usefullData.k_toLoadParams, "True");						
							StartCoroutine(LoadProject(true));	
	                       // StartCoroutine(LoadLevel());
	                    }
	                }
	                else if(GUI.Button(new Rect(175,r_halfPanelRight.yMax-50-r_halfPanelRight.y,175,50),TextManager.GetText("ExplorerV2.ExportProject"),"rgtPanelBtn"))
	                {
	                    IOutils.ExportMontageFile(m_currentPath);
					}
					
					#endif
                }
                else
                {
					if(GUI.Button(new Rect(175,r_halfPanelRight.yMax-50-r_halfPanelRight.y,175,50),"Charger ", "rgtPanelBtn")
					   && async == null)
					{
						StartCoroutine(LoadProject(true));
						
						GameObject mainScene = GameObject.Find ("MainScene");			
						if(mainScene!=null)
						{
							GUIStart guiStart = mainScene.GetComponent<GUIStart>();
							if(guiStart!=null)
							{
								guiStart.quitExplorerInGame();
								
							}
						}
					}
					
					if(GUI.Button(new Rect(0,r_halfPanelRight.yMax-50-r_halfPanelRight.y,175,50),"Ecraser", "rgtPanelBtn")
					   && async == null)
					{
						bnewSave = false;
						bsave = true;
						
						szsaveName = m_projects[id_selectedProject];
						szsaveComment = cdm.m_comments;
					}
					
					if(GUI.Button(new Rect(0,r_halfPanelRight.yMax-100-r_halfPanelRight.y,350,50),"Nouvelle sauvegarde","rgtPanelBtn")
					   && async == null)
					{
						bnewSave = true;
						bsave = true;
						
						szsaveName = "";
						szsaveComment = "";
					}
                }
			}
			GUI.EndGroup();
		}
		#endregion

		
		
		GUI.EndGroup();
		
		
		//Chargement
		if(async != null)
		{
			GUI.Box(r_loadBarFond,"","loadingBg");
			r_loadBarGrpMv.width = async.progress * r_loadBarFond.width;
			
			GUI.BeginGroup(r_loadBarGrpMv);
			GUI.Label(r_loadBar,"","loadingBar");
			GUI.EndGroup();
			GUI.Label(new Rect(r_loadBarFond.xMax-150,r_loadBarFond.yMax,150,30),TextManager.GetText("ExplorerV2.Loading"),"loadtxt");
		}

		/*GUIDrawRect(new Rect(	(Screen.width * 0.5f) - (r_fullPanelBG.width * 0.5f) - 25.0f,
		                     (Screen.height * 0.5f) - (r_fullPanelBG.height * 0.5f),
		                     r_fullPanelBG.width + 25.0f, r_fullPanelBG.height + 60.0f),new Color(1.0f,0.0f,1.0f,1.0f));*/
	}

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= SetRects;
        UsefullEvents.OnResizeWindowEnd -= SetRects;
    }
	void OnEnable()
	{
		updateClientList();
	}
	#endregion
	
	IEnumerator TakePicture()
	{
		ActivateToMainWithLibs();
		//PlayerPrefs.SetString(usefullData.k_startBypass,"extImage");			
		if(id_selectedClient != -1)
		{
			PlayerPrefs.SetString(usefullData.k_selectedClient,m_clients[id_selectedClient]);	
		}
		GameObject mainScene = GameObject.Find ("MainScene");			
		if(mainScene!=null)
		{
			while(!mainScene.activeSelf)
				yield return new WaitForEndOfFrame();
				GUIStart guiStart = mainScene.GetComponent<GUIStart>();
				if(guiStart!=null)
				{
					guiStart.ShowStartAnd(true,false,false);
				}
		}
		DeactivateExplorer();
	}
			
	IEnumerator LoadPicture()
	{
		ActivateToMainWithLibs();
		//PlayerPrefs.SetString(usefullData.k_startBypass,"extImage");			
		if(id_selectedClient != -1)
		{
			PlayerPrefs.SetString(usefullData.k_selectedClient,m_clients[id_selectedClient]);	
		}
		GameObject mainScene = GameObject.Find ("MainScene");			
		if(mainScene!=null)
		{
			while(!mainScene.activeSelf)
				yield return new WaitForEndOfFrame();
				GUIStart guiStart = mainScene.GetComponent<GUIStart>();
				if(guiStart!=null)
				{
					guiStart.ShowStartAnd(false,true,false);
				}
		}
		DeactivateExplorer();
	}
	
	IEnumerator ExamplePicture()
	{
		ActivateToMainWithLibs();
		//PlayerPrefs.SetString(usefullData.k_startBypass,"extImage");			
		if(id_selectedClient != -1)
		{
			PlayerPrefs.SetString(usefullData.k_selectedClient,m_clients[id_selectedClient]);	
		}
		GameObject mainScene = GameObject.Find ("MainScene");			
		if(mainScene!=null)
		{
			while(!mainScene.activeSelf)
				yield return new WaitForEndOfFrame();
				GUIStart guiStart = mainScene.GetComponent<GUIStart>();
				if(guiStart!=null)
				{
					guiStart.ShowStartAnd(false,false,true);
				}
		}
		DeactivateExplorer();
	}
	
	IEnumerator LoadProject(bool withObject)
	{
		ActivateToMainWithLibs();
//		yield return new WaitForEndOfFrame();
	//	PlayerPrefs.SetString(usefullData.k_toLoadPath,m_currentPath);
	//	PlayerPrefs.SetString(usefullData.k_toLoadParams,"True");						
		if(id_selectedClient != -1)
		{
			PlayerPrefs.SetString(usefullData.k_selectedClient,m_clients[id_selectedClient]);	
		}	
		if(m_currentPath != "")
		{
			GameObject mainScene = GameObject.Find ("MainScene");
			if(mainScene!=null)
			{
				while(!mainScene.activeSelf)
					yield return new WaitForEndOfFrame();
				Montage montage = mainScene.GetComponent<Montage>();
				GUIStart guiStart = mainScene.GetComponent<GUIStart>();
				if(guiStart!=null)
				{
					guiStart.showStart(false);
				}
				GUIMenuMain guiMain = mainScene.GetComponent<GUIMenuMain>();
				if(guiMain!=null)
				{
					guiMain.SetHideAll(true);
				}
				if(montage!=null)
					montage.LoadFromExplorer(m_currentPath,withObject);
			}
		}
		DeactivateExplorer();
	}
	public void explorerInGame()
	{
		byPassGui = false;
		m_bexplorerInGame = true;
	}
	public void quitExplorerInGame()
	{
		byPassGui = false;
		m_bexplorerInGame = false;
		
		id_selectedClient = -1;
		id_selectedProject = -1;
		cdm = null;
		
		bnewSave = false;
		bsave = false;
		bsaveRequest = false;
		szsaveName = "";
		szsaveComment = "";
		Camera.main.GetComponent<Mode2D>().DestroyObjectCopy ();
		
	}
	
	private void SwitchToMainWithLibs()
	{
		ActivateToMainWithLibs();
		DeactivateExplorer();
	}	
	private void ActivateToMainWithLibs()
	{		
		transform.GetComponent<Camera>().enabled = false;
		_mainWithLibs  = GameObject.Find("MainWithLibs");
		if(_mainWithLibs !=null)
		{
		//	_mainWithLibs.SetActive(true);
			foreach(Transform tra in _mainWithLibs.transform)
				tra.gameObject.SetActive(true);
		}
	}
	private void DeactivateExplorer()
	{		
		id_selectedClient = -1;
		arrows.setArrowsRect(r_fullPanelBGArrows,"clientUp","clientDwn");
        m_useClientSkin = true;
		
		bnewSave = false;
		bsave = false;
		
		szsaveName = "";
		szsaveComment = "";
		
		_explorer = GameObject.Find("Explorer");		
		if(_explorer!=null)
		{
			//_explorer.SetActive(false);
			foreach(Transform tra in _explorer.transform)
				tra.gameObject.SetActive(false);
		}
	}
    //-----------------------------------------------------
    private void SetRects()
    {
        r_group1.Set((UnityEngine.Screen.width-1024)/2,(UnityEngine.Screen.height-768)/2,1024,768);

        r_leftPanelArrows.Set((UnityEngine.Screen.width-700)/2,(UnityEngine.Screen.height+500)/2-50, r_halfPanelLeft.width, 50);
        r_fullPanelBGArrows.Set(UnityEngine.Screen.width/2-r_halfPanelLeft.width/2, (UnityEngine.Screen.height+500)/2-50, r_halfPanelLeft.width,50);

        if(m_useClientSkin)
            arrows.setArrowsRect(r_fullPanelBGArrows,"clientUp","clientDwn");
        else
            arrows.setArrowsRect(r_leftPanelArrows,"projetUp","projetDwn");

        Rect pixin = m_background.GetComponent<GUITexture>().pixelInset;
        pixin.Set(0, 0, UnityEngine.Screen.width, UnityEngine.Screen.height);
        m_background.GetComponent<GUITexture>().pixelInset = pixin;
    } // SetRects()
	

	#region METHOD's
	
	public GUISkin getSkin()
	{
		return skin;	
	}
	
	public void changePage(int i)
	{
		if(id_selectedClient == -1)
		{
			if(i<0)
			{
				sp_fullPanel.y -= 10;
				if(sp_fullPanel.y <0)
					sp_fullPanel.y = 0;
			}
			else
			{
				sp_fullPanel.y += 10;
				if(sp_fullPanel.y > r_fullPanelSV.height)
					sp_fullPanel.y = r_fullPanelSV.height;
			}
		}
		else
		{
			if(i<0)
			{
				sp_halfPanelLeft.y -= 10;
				if(sp_halfPanelLeft.y <0)
					sp_halfPanelLeft.y = 0;
			}
			else
			{
				sp_halfPanelLeft.y += 10;
				if(sp_halfPanelLeft.y > r_halfPanelLeftSV.height)
					sp_halfPanelLeft.y = r_halfPanelLeftSV.height;
			}
		}
	}
	
	private void updateClientList()
	{
		m_clients = Directory.GetDirectories(usefullData.SavePath);
		for(int i=0;i<m_clients.Length;i++)
		{
			if(m_rootPath != null)
			{
				string s = m_clients[i];
				s = s.Replace(m_rootPath,"");
				m_clients[i] = s;
			}
		}
		
		int nbLines = m_clients.Length/4;
		
		if(m_clients.Length>nbLines*4)
			nbLines ++;
		
		r_fullPanelSV.height = nbLines * 100+50;
		//update du rectangle contenant les icones clients
	}
	
	private void updateProjectList()
	{
		if(m_currentPath != m_rootPath && m_currentPath != "")
		{
			m_projects = Directory.GetFiles(m_currentPath);
			for(int i=0;i<m_projects.Length;i++)
			{
				string s = m_projects[i];
				s = s.Replace(m_currentPath,"");
				s = s.Replace("\\","");
				s = s.Replace("/","");
				
				if(s.Contains(usefullData.SaveFileExtention))
					s = s.Replace(usefullData.SaveFileExtention,"");
				
				if(s.Contains(usefullData.SaveNewFileExtention))
					s = s.Replace(usefullData.SaveNewFileExtention,"");
				
				m_projects[i] = s;
			}
			r_halfPanelLeftSV.height = m_projects.Length*50 + 40;
			if(m_projects.Length ==0)
				newFilePanel = true;
			else
				newFilePanel = false;
		}
		else
			m_projects = new string[0];
	}
	
	IEnumerator preload ()
	{
		cdm = new ContextDataModel();
        cdm.load(new BinaryReader(IOutils.LoadMontageFile(m_currentPath)));

		yield return true;
	}
	
	/*IEnumerator LoadLevel()
	{
		if(id_selectedClient != -1)
		{
			PlayerPrefs.SetString(usefullData.k_selectedClient,m_clients[id_selectedClient]);	
		}
		
		if(usefullData.sc_oneShot == "")
			Debug.Log("NO SCENE SET");
		else
		{
#if UNITY_IPHONE
			EtceteraManager.mailComposerFinished -= IOutils.EndMail;
#endif
			
	        async = UnityEngine.Application.LoadLevelAsync(usefullData.sc_oneShot);
	        yield return async;
	        Debug.Log("Loading Oneshot");
		}
    }*/
	
//	void endMail(string result)
//	{
//		Debug.Log("Mail Sender : "+result);
//#if UNITY_IPHONE
//		EtceteraManager.setPrompt(false);
//#endif
//		m_background.guiTexture.texture = bkUp;
//		byPassGui = false;
//	}

	private bool isOnRect(Rect target)
	{
		bool isOnR = false;
#if UNITY_IPHONE
		if(Input.touchCount > 0)
		{
			foreach(Touch t in Input.touches)
			{
				if(target.Contains(t.position))
				{
					isOnR = true;	
				}
			}
		}
#else
		Vector2 cursor = Input.mousePosition;
		cursor.y = UnityEngine.Screen.height - cursor.y;
		if(target.Contains(cursor))
			isOnR = true;
#endif
		return isOnR;
	}
	
	#endregion
}