using UnityEngine;
using System.Collections;
using System.IO;

using Pointcube.Utils;
using Pointcube.Global;

#if UNITY_EDITOR
using UnityEditor;
using System;
#endif
//#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//using System.Windows.Forms;
//#endif

public class Montage : MonoBehaviour
{
#region variables
	string[] savedFiles;
	string[] m_clients;
	string[] files;
	
	GameObject mainNode;
	GameObject mainScene;
	GameObject eraserNode;
	GameObject grassNode;

    public GameObject m_mainCam;
    public Mode2D     m_mode2D;
	
	public static ContextDataModel cdm;
	public static SceneModel sm;
	public static SceneUpgradeModel sum;
	public static ObjectsModel om;
	public static bool s_eraserMasksRdy = false;
	
	bool savePrompt = false;
	bool loadPrompt = false;
	bool ClientPrompt = false;
    bool isLoading = false;

    public bool IsLoading
    {
        get { return isLoading; }
        set { isLoading = value; }
    }
	public enum states {notSaved, saving, saved};
	public states saveState;
	bool preview = true;
	bool back2Start = false;
	bool m_completeLoad = true;
	bool m_saveInGame = false;
	bool m_saveAndQuit=false;
	bool m_saveAndSend=false;
	bool m_saveAndExport=false;
	bool m_saveAndAccueil=false;
	bool b_tryToOverride = false;
	bool m_isSliding = false;// Anti slide and select
    bool m_errorLoading = false;
	
	WWW www;
	
	string saveName = "";
	string _autoSaveName = "";
	string loadName = "";
	string comment = "";
	public string m_client = "";
	string m_newClient = "";
	
	int loadIndex = -1;
	int readyToLoad = -1;
	
	//GUI
	private Rect saveBox;
	private Rect loadBox;
	private Rect filesBox;

	Vector2 scrollpos = new Vector2(0,0);
	
	public GUISkin skin;
	
	//test temporaire
	bool showClient = false;
	int m_selectedClient=-1;
	
	GUIDialogBox _cb;
	
	public static bool assetBundleLock = false;

    public static readonly string DEBUGTAG = "Montage : ";

    private string sceneToLoad;



#endregion
#region unity_func
	//-----------------------------------------------------
	void Awake ()
	{
		cdm = new ContextDataModel ();
		sm = new SceneModel ();
		sum = new SceneUpgradeModel ();
		om = new ObjectsModel (this);

        UsefullEvents.OnResizingWindow  += RelocateUI;
        UsefullEvents.OnResizeWindowEnd += RelocateUI;

#if UNITY_IPHONE
		EtceteraManager.mailComposerFinishedEvent += IOutils.EndMail;
#endif
		
	}

    //-----------------------------------------------------
	void Start ()
	{
        if(m_mainCam == null)  Debug.LogError(DEBUGTAG+"mainCam"+PC.MISSING_REF);
        if(m_mode2D == null)  Debug.LogError(DEBUGTAG+"mode2D"+PC.MISSING_REF);

		getListOfSavedFiles ();
		
		mainScene = GameObject.Find("MainScene");
		mainNode = GameObject.Find ("MainNode");
		eraserNode = GameObject.Find ("eraserImages");
		grassNode = GameObject.Find ("grassImages");

        // -- Rects GUI --
		saveBox  = new Rect (0f, 0f, 300, 600);
		loadBox  = new Rect (0f, 0f, 300, 600);
		filesBox = new Rect (0f, 0f, 300, 600);
        RelocateUI();

		if(PlayerPrefs.HasKey(usefullData.k_toLoadPath) && PlayerPrefs.HasKey(usefullData.k_toLoadParams))
		{
			string path = PlayerPrefs.GetString(usefullData.k_toLoadPath);
			string boolean = PlayerPrefs.GetString(usefullData.k_toLoadParams);
			if(boolean != "")
				m_completeLoad = bool.Parse(boolean);
			
			if(path != "")
			{
				StartCoroutine(load(path,true));
			}
		}
		getListOfClients();
		
		if(PlayerPrefs.HasKey(usefullData.k_selectedClient))
		{
			string cl = PlayerPrefs.GetString(usefullData.k_selectedClient);
			if(cl != "")
			{
				m_client = cl;
				for(int i=0;i<m_clients.Length;i++)
				{
					if(m_clients[i] == m_client)
						m_selectedClient = i;
				}
				getListOfSavedFiles();
			}
			PlayerPrefs.DeleteKey(usefullData.k_selectedClient);
		}
		
		//ConfirmationBox
		_cb = GetComponent<GUIDialogBox>();
		
	}
	
	public void LoadFromExplorer(string name, bool isFullPath)
	{
		m_completeLoad = isFullPath;
		StartCoroutine(load(name,true));
	}



    //-----------------------------------------------------
	void OnGUI ()
	{
		GUI.skin = skin;
		if (isLoading) 
		{ } 
		else 
		{			
			if ((savePrompt || loadPrompt)&& !ClientPrompt) 
			{
//				GetComponent<GUIStart> ().showStart (false);
				fileBoxGUI();
				if(savePrompt)
					saveBoxGUI();
				if(loadPrompt)
					loadBoxGUI();
			}
			else if(ClientPrompt)
			{
				fileBoxGUI();
				clientBoxGUI();
			}
		}
		
	}

    //-----------------------------------------------------
	void Update()
	{
        if (sceneToLoad != null)
        {
            LoadFromExplorer(System.String.Copy(sceneToLoad), true);
            sceneToLoad = null;
        }

        //scroll tactile
        float deltaScroll;
        if(PC.In != null){
			if(PC.In.ScrollViewV(out deltaScroll))
	        {
	            m_isSliding = true;
	            if(PC.In.CursorOnUI(filesBox))
	                scrollpos.y += deltaScroll;
	        }
	        else if(PC.In.Click1Up())
	            m_isSliding = false;
			//Check input pour savoir si modif.  NOTSAVED
			//TODO check si il ya une vrai modification (cr�ation, scale ,etc...) avant de changer le state
			if(Input.touchCount > 0 || Input.anyKey){
				saveState = states.notSaved;
			}
	//		if(Input.touchCount>0)
	//		{
	//			Touch t = Input.touches[0];
	//			if(t.phase == TouchPhase.Moved)
	//			{
	//				m_isSliding = true;
	//				if(filesBox.Contains(t.position) && t.deltaPosition.y!=0)
	//				{
	//					scrollpos.y = scrollpos.y + t.deltaPosition.y;	
	//				}
	//			}
	//			
	//			if(t.phase == TouchPhase.Ended)
	//			{
	//				m_isSliding = false;
	//			}
	//		}
		}
	}

    //-----------------------------------------------------
    void OnDelete()
    {
        UsefullEvents.OnResizingWindow  -= RelocateUI;
        UsefullEvents.OnResizeWindowEnd -= RelocateUI;
#if UNITY_IPHONE
			EtceteraManager.mailComposerFinishedEvent -= IOutils.EndMail;
#endif
    }

#endregion
#region privee

    //-----------------------------------------------------
    private void RelocateUI()
    {
        saveBox.x = UnityEngine.Screen.width / 2;
        saveBox.y = (UnityEngine.Screen.height / 2) - 300;

        loadBox.x = UnityEngine.Screen.width / 2;
        loadBox.y = (UnityEngine.Screen.height / 2) - 300;

        filesBox.x = (UnityEngine.Screen.width / 2) - 300+5;
        filesBox.y = (UnityEngine.Screen.height / 2) - 300;
    }

    //-----------------------------------------------------
	private void fileBoxGUI()
	{
		GUI.BeginGroup(filesBox);
		GUI.Box(new Rect(40,0,260,150),"","bgLUp");
		GUI.Box(new Rect(40,150,260,325),"","bgLMid");
        GUI.Box(new Rect(40,475,260,150),"","bgLDwn");

		if(m_clients.Length > 0)
		{
			if(m_selectedClient != -1)//Client s�lectionn� > choix de la sauvegarde
			{
                string clt = m_clients[m_selectedClient].ToString();
                clt = (clt.Length > 12) ? clt.Substring(0, 12)+"..." : clt;

                GUI.Label(new Rect(15,150,260,30),TextManager.GetText("Montage.Client")+" "+clt,"headerL");
				if(files.Length>0)
				{
					scrollpos = GUI.BeginScrollView(new Rect(40,200,270,275),scrollpos,new Rect(new Rect(0,0,250,50*files.Length)));
					int tmpid = GUI.SelectionGrid (new Rect(0,0,250,50*files.Length), loadIndex, files, 1,"btnList");
					if(tmpid != loadIndex && !m_isSliding)
					{
						loadIndex = tmpid;
						if(savePrompt)//savePrompt saveName = celui s�lectionn� dans la liste
						{
							saveName = files [loadIndex];
							StartCoroutine (preload (files [loadIndex]));
						}
						if(loadPrompt)//LoadPrompt + sauvegarde s�lectionn�e
						{
							loadName = files [loadIndex];
							StartCoroutine (preload (files [loadIndex]));
							readyToLoad = loadIndex;
						}
					}
					GUI.EndScrollView();
				}
				else
					GUI.Label(new Rect(40,200,290,275),TextManager.GetText("Montage.NoSave"),"nosaves");

				if(GUI.Button(new Rect(90,510,180,50),TextManager.GetText("Montage.BackClient"),"txtValid"))//Back to clients
				{
					m_selectedClient = -1;
                    loadIndex = -1;
					ClientPrompt = true;
					m_client = "";
				}
				if(files.Length>5)
				{
					if(GUI.RepeatButton(new Rect(100,440,100,100),"","up"))
						scrollpos.y = scrollpos.y - 5;
					if(GUI.RepeatButton(new Rect(200,440,100,100),"","down"))
						scrollpos.y = scrollpos.y + 5;
				}
			}
			else // Pas encore de client s�lectionn�
			{
                GUI.Label(new Rect(15,150,260,30),TextManager.GetText("Montage.ChooseClient"),"headerL");
				scrollpos = GUI.BeginScrollView(new Rect(40,200,270,275),scrollpos,new Rect(new Rect(0,0,250,50*m_clients.Length)));
				int tmp = GUI.SelectionGrid (new Rect(0,0,250,50*m_clients.Length), m_selectedClient, m_clients, 1,"btnList");
				if(tmp != m_selectedClient && !m_isSliding)
				{
					m_selectedClient = tmp;
					m_client = m_clients[m_selectedClient];
					ClientPrompt = false;
					getListOfSavedFiles();
				}
				GUI.EndScrollView();
//				if(savePrompt && !ClientPrompt)
//				{
	//				m_newClient = GUI.TextField(new Rect(100,510,200,30),m_newClient);
	//				if(GUI.Button(new Rect(100,450,100,30),"NewClient") && m_newClient != "")
	//				{
	//					Directory.CreateDirectory(usefullData.SavePath+m_newClient);
	//					m_newClient = "";
	//					getListOfClients();
	//				}
//					if(GUI.Button(new Rect(100,475,100,30),TextManager.GetText("Montage.NewClient")))
//						ClientPrompt = true;
//				}
				if(m_clients.Length>5)
				{
					if(GUI.RepeatButton(new Rect(100,440,100,100),"","up"))
						scrollpos.y = scrollpos.y - 5;
					if(GUI.RepeatButton(new Rect(200,440,100,100),"","down"))
						scrollpos.y = scrollpos.y + 5;
				}
			}
		}
		else
			GUI.Label(new Rect(40,150,290,250),TextManager.GetText("Montage.NoClient"),"nosaves");

		
//		if(savePrompt)//savePrompt saveName = celui s�lectionn� dans la liste
//		{
//			if (loadIndex != -1 && files[loadIndex] != null)
//			{
//				saveName = files [loadIndex];
////				loadIndex = -1;
//			}
//		}
//		if(loadPrompt)//LoadPrompt + sauvegarde s�lectionn�e
//		{
//			if (loadIndex != -1 && files [loadIndex] != null)
//			{
//				loadName = files [loadIndex];
//				StartCoroutine (preload (files [loadIndex]));
//				readyToLoad = loadIndex;
////				loadIndex = -1;
//			}
//		}


		GUI.EndGroup();
	}
	
	public void setComment(string _szcomment)
	{
		cdm.update(_szcomment);
	}
	
	private void saveBoxGUI()
	{
		GUI.BeginGroup(saveBox);
		GUI.Box(new Rect(0,0,260,150),"","bgRUp");
		GUI.Box(new Rect(0,150,260,300),"","bgRMid");
		GUI.Box(new Rect(0,450,260,150),"","bgRDwn");
		
		if(m_client != "")
        {}
//			GUI.Label(new Rect(15,150,260,30),TextManager.GetText("Montage.Save")+" ("+m_client+")","headerR");
		else
			GUI.Label(new Rect(15,150,260,30),TextManager.GetText("Montage.Save"),"headerR");
		
		GUI.Label(new Rect(20,180,260,30),TextManager.GetText("Montage.Name"), "txt");
		saveName = GUI.TextField (new Rect (0, 210, 260, 50), saveName,"txtField");
		
		GUI.Label(new Rect(20,420,180,20),TextManager.GetText("Montage.CreatedThe")+" "+cdm.m_birthDate,"txtSmall");
		GUI.Label(new Rect(20,260,260,30),TextManager.GetText("Montage.Comments"),"txt");
		
		comment = GUI.TextArea (new Rect (0, 290, 260, 130), comment,"txtArea");
		
		GUI.Box(new Rect(0,420,260,50),"","bgValid");
		
		if(m_selectedClient != -1 && saveName != "")
		{
			if(b_tryToOverride)
			{
				if(! _cb.isVisible())
				{
					_cb.showMe(true,GUI.depth);
					_cb.setText(TextManager.GetText("Montage.Overwrite")+" "+saveName+" ?");
					_cb.setBtns(TextManager.GetText("Montage.Yes"),TextManager.GetText("Montage.Cancel"));
				}
				else
				{
					if(_cb.getConfirmation())
					{
						_cb.showMe(false,GUI.depth);
						
						// DO
						StartCoroutine (save (saveName));
						savePrompt = false;
						b_tryToOverride = false;
						//----------------------------
					}
					if(_cb.getCancel())
					{
						_cb.showMe(false,GUI.depth);
						
						// DO
						b_tryToOverride = false;
						//----------
					}
				}
			}
			
			if(GUI.Button(new Rect(20,440,85,50),TextManager.GetText("Montage.Save"),"txtValid") && saveName != "")
			{
				//if (comment != "" && comment != null)
				cdm.update (comment);
				
				if(isInSaves(saveName))
				{
					b_tryToOverride = true;
				}
				else
				{
					StartCoroutine (save (saveName));
					savePrompt = false;
					//canShow();
				}
			}
			
			if(GUI.Button(new Rect(115,440,85,50),TextManager.GetText("Montage.Delete"),"txtValid"))
			{
                IOutils.DeleteMontageFile(usefullData.SavePath +m_client+"/"+ saveName);
				
				getListOfSavedFiles();
				resetPreview();
			}
//			GUI.Label(new Rect(98,420,4,50),"|","txtValid");
		}
		
//		GUI.Box(new Rect(0,470,260,50),"","bgValid");
		if(GUI.Button(new Rect(20,510,180,50),TextManager.GetText("Montage.Cancel"),"txtValid"))//new Rect(0,470,200,50)
		{
			savePrompt = false;
			canShow();
			getListOfSavedFiles();
		}
		
		GUI.EndGroup();
	}
	
	private void loadBoxGUI()
	{
		GUI.BeginGroup(loadBox);
		GUI.Box(new Rect(0,0,260,150),"","bgRUp");
		GUI.Box(new Rect(0,150,260,300),"","bgRMid");
		GUI.Box(new Rect(0,450,260,150),"","bgRDwn");
		
		GUI.Label(new Rect(15,150,260,30),"Charger","headerR");
		
		if (readyToLoad != -1)
		{
			if(preview)
				GUI.Box (new Rect (15, 200, 150, 150), cdm.m_thumbnail,"preview");
			else
				GUI.Label (new Rect (0, 200, 260, 170), cdm.m_comments,"txtArea");
			
			preview = GUI.Toggle(new Rect (165, 250, 50, 50),preview,"","previewTg");
			
			GUI.Label(new Rect(15,360,260,30),loadName,"subheader");
			GUI.Label(new Rect(15,390,260,20),TextManager.GetText("Montage.CreatedThe")+" "+cdm.m_birthDate,"txt");
			
//			GUI.Box(new Rect(0,420,260,50),"","bgValid");
			if(GUI.Button(new Rect(20,440,85,50),TextManager.GetText("Montage.Validate"),"txtValid"))
			{
					StartCoroutine (load (files [readyToLoad],false));
					loadPrompt = false;
				
			}
//			GUI.Label(new Rect(98,420,4,50),"|","txtValid");
//			GUI.Box(new Rect(100,420,100,50),"","bgValid");
			if(GUI.Button(new Rect(115,440,85,50),TextManager.GetText("Montage.Cancel"),"txtValid"))
			{
				loadPrompt = false;
				if(back2Start)
					GetComponent<GUIStart>().showStart(true);
				else
					canShow();
			}
			
//			GUI.Box(new Rect(0,470,260,50),"","bgValid");
			if(GUI.Button(new Rect(20,510,180,50),TextManager.GetText("Montage.Delete"),"txtValid"))
			{
                IOutils.DeleteMontageFile(usefullData.SavePath +m_client+"/"+ loadName);
				
				readyToLoad --;
				getListOfSavedFiles();
				resetPreview();
			}
		}
		else
		{
			if(m_selectedClient != -1)
				GUI.Label(new Rect(50,300,260,30),TextManager.GetText("Montage.SelectFile"), "subheader");
			else
				GUI.Label(new Rect(50,300,260,30),TextManager.GetText("Montage.SelectClient"), "subheader");
			
			GUI.Label(new Rect(15,300,30,30), "", "arrow");
			
//			GUI.Box(new Rect(0,420,260,50),"","bgValid");
			if(GUI.Button(new Rect(20, 420, 80, 50), TextManager.GetText("Montage.Cancel"), "txtValid")) // TODO ou est ce bouton ?
			{
				loadPrompt = false;
				if(back2Start)
					GetComponent<GUIStart>().showStart(true);
				else
					canShow();
			}
		}
		GUI.EndGroup();
	}
	
	private void clientBoxGUI()
	{
		GUI.BeginGroup(loadBox);
		GUI.Box(new Rect(0,0,260,150),"","bgRUp");
		GUI.Box(new Rect(0,150,260,300),"","bgRMid");
		GUI.Box(new Rect(0,450,260,150),"","bgRDwn");
		
		GUI.Label(new Rect(15,150,260,30),TextManager.GetText("Montage.CreateClient"),"headerR");
		
		m_newClient = GUI.TextField (new Rect (0, 210, 260, 50),m_newClient,"txtField");
        GUI.Box(new Rect(0,260,200,50),"","bgValid");
        if(GUI.Button(new Rect(20,260,180,50),TextManager.GetText("Montage.AddClient"),"txtValid") && m_newClient != "")
		{
			Directory.CreateDirectory(usefullData.SavePath+m_newClient);
			getListOfClients();
			
			m_client = m_newClient;
			for(int i=0;i<m_clients.Length;i++)
			{
				if(m_clients[i] == m_client)
					m_selectedClient = i;
			}
			ClientPrompt = false;
			m_newClient = "";
			getListOfSavedFiles();
		}
		GUI.Box(new Rect(0,310,200,50),"","bgValid");
		if(GUI.Button(new Rect(20,310,180,50),TextManager.GetText("Montage.Cancel"),"txtValid"))//new Rect(0,470,200,50)
		{
			ClientPrompt = false;
			savePrompt = false;
			loadPrompt = false;
			canShow();
			getListOfSavedFiles();
		}
		
		GUI.EndGroup();
	}
	
	private void canShow()
	{
		GetComponent<GUIMenuLeft>().canDisplay(true);
		GetComponent<GUIMenuRight>().canDisplay(true);
		Camera.main.GetComponent<ObjInteraction>().setActived(true);
	}
	
	#endregion
	
	//FCN. AUX. PUBLIC
	#region fcn Aux.
	public ContextDataModel getcdm ()
	{
		return cdm;	
	}
	
	public SceneModel GetSM ()
	{
		return sm;	
	}
	
	public void getListOfSavedFiles ()
	{
		string path = usefullData.SavePath+m_client;
		
		savedFiles = Directory.GetFiles (path);
		files = new string[savedFiles.Length];
		
		for (int i=0; i<savedFiles.Length; i++) {
			string tmp = savedFiles [i];
			if (tmp.Contains (usefullData.SaveFileExtention)) {
				tmp = tmp.Replace (/*usefullData.SavePath*/path, "");
				tmp = tmp.Replace ("/", "");
				tmp = tmp.Replace ("\\", "");
				tmp = tmp.Replace (usefullData.SaveFileExtention, "");
	
				files [i] = tmp;
			}
			if (tmp.Contains (usefullData.SaveNewFileExtention)) {
				tmp = tmp.Replace (/*usefullData.SavePath*/path, "");
				tmp = tmp.Replace ("/", "");
				tmp = tmp.Replace ("\\", "");
				tmp = tmp.Replace (usefullData.SaveNewFileExtention, "");
	
				files [i] = tmp;
			}
		}
		if(PlayerPrefs.HasKey(usefullData.k_selectedProject))
		{
			string s = PlayerPrefs.GetString(usefullData.k_selectedProject);
			if(s != "")
			{
				saveName = "";
				foreach(string str in files)
				{
					if(str == s)
						saveName = s;
				}
			}
		}
	}
	
	public void getListOfClients()
	{
		m_clients = Directory.GetDirectories(usefullData.SavePath);
		for(int i=0;i<m_clients.Length;i++)
		{
			string s = m_clients[i];
			s = s.Replace(usefullData.SavePath,"");
			m_clients[i] = s;
		}
	}
	
	private void resetPreview()
	{
		cdm.reset();
		loadName = "";
		readyToLoad = -1;
	}
	
	public void showSave ()
	{
		loadPrompt = false;
		savePrompt = true;
		getListOfClients();
		if(m_clients.Length == 0 || m_selectedClient ==-1)
		{
			ClientPrompt = true;
		}
		if(PlayerPrefs.HasKey(usefullData.k_selectedProject))
		{
			string s = PlayerPrefs.GetString(usefullData.k_selectedProject);
			if(s != "")
			{
				saveName = "";
				foreach(string str in files)
				{
					if(str == s)
						saveName = s;
				}
			}
		}	
	}
	
	public void showSaveAndQuit()
	{
//		getListOfClients();
		showSave();
		m_saveAndQuit = true;
	}
	public void showSaveAndAccueil()
	{
//		getListOfClients();
		showSave();
		m_saveAndAccueil = true;
	}
	
	public void showLoad (bool b)
	{
		savePrompt = false;
		loadPrompt = true;
		if(m_selectedClient == -1)
			ClientPrompt = true;
		back2Start = b;
		getListOfClients();

	}
	
	#endregion
	
	//FCN. AUX. PRIVEE
	
	#region Save / Load
	IEnumerator preload (string name)
	{
		// load ContextData only
		string localPath = usefullData.SavePath +m_client+"/"+ name; //+usefullData.SaveFileExtention;
        cdm.load(new BinaryReader(IOutils.LoadMontageFile(localPath)));
		comment = cdm.m_comments;
		yield return Resources.UnloadUnusedAssets();
		yield return true;
	}

	private string SetLocalPath(string newPath)
	{
				Debug.Log("SetHasModel -1");
				Debug.Log("SetHasModel 1");
				Debug.Log("SetHasModel 2");
		cdm.SetHasVersion(true);
				Debug.Log("SetHasModel 3");
				Debug.Log("SetHasModel 4");
				Debug.Log("SetHasModel 5");
				Debug.Log("SetHasModel 6");
				Debug.Log("SetHasModel 7");
				Debug.Log("SetHasModel 8");
		return newPath;
	}
	IEnumerator load (string name, bool isFullPath)
	{
		m_mainCam.GetComponent<ObjInteraction>().enabled = false;
		yield return Resources.UnloadUnusedAssets();
		s_eraserMasksRdy = false;
		mainScene.GetComponent<PleaseWaitUI>().SetLoadingMode(true);
		yield return new WaitForEndOfFrame();
		isLoading = true;
		string localPath;
		if(isFullPath)
		{
			if(name.EndsWith(usefullData.SaveFileExtention))
			{
				name = name.Substring(0,name.LastIndexOf('.'));
			}		
			else if(name.EndsWith(usefullData.SaveNewFileExtention))
			{
				name = name.Substring(0,name.LastIndexOf('.'));
			}
			//localPath = name;
			
			string localPathTemp = name + usefullData.SaveFileExtention;
			//cdm.SetHasVersion(false);
			if(File.Exists(localPathTemp))
			{
				cdm.SetHasVersion(false);
				localPath = localPathTemp;
			}
			else{		
				localPath =  SetLocalPath(name + usefullData.SaveNewFileExtention);				
			}
			
			while (GetComponent<LibraryLoader>() != null)
			{
				yield return new WaitForEndOfFrame();	
			}
			PlayerPrefs.DeleteKey(usefullData.k_toLoadPath);
			PlayerPrefs.DeleteKey(usefullData.k_toLoadParams);
			//yield return new WaitForSeconds(5);//TODO Changer ca en "Wait for libs";
		}
		else
		{
			if(name.EndsWith(usefullData.SaveFileExtention))
			{
				name = name.Substring(0,name.LastIndexOf('.')-1);
			}		
			else if(name.EndsWith(usefullData.SaveNewFileExtention))
			{
				name = name.Substring(0,name.LastIndexOf('.')-1);
			}

			localPath = usefullData.SavePath +m_client+"/"+ name + usefullData.SaveFileExtention;
			if(!File.Exists(localPath))
			{
				localPath = usefullData.SavePath +m_client+"/"+ name + usefullData.SaveNewFileExtention;
				cdm.SetHasVersion(true);
			}
			else
			{
				cdm.SetHasVersion(false);
			}
			PlayerPrefs.SetString(usefullData.k_selectedProject,name);
		}
		
		BinaryReader buf = new BinaryReader(IOutils.LoadMontageFile(localPath));
		
		cdm.load (buf);
		
		sm.load (buf,m_completeLoad, cdm.versionSave);
        Debug.Log("Scemn model loaded");
        m_mainCam.GetComponent<Mode2D>().LoadFromSceneModel();
        Debug.Log("main cam loaded");
		if(m_completeLoad)
		{
			//om.load (buf);
            Debug.Log("begin obj loading");
			yield return StartCoroutine(loadObjs(buf));
            if (m_errorLoading)
            {
                yield return true;
            }
            Debug.Log("obj loaded");
			yield return Resources.UnloadUnusedAssets();


//    			while(!isOk()) 	// Attend que les images de gazon
//    			{				//	et de gomme soient toutes instanci�es
//    				yield return new WaitForEndOfFrame();
//    			}
//
//    			while(!s_eraserMasksRdy)// attend la fin du reset du gazon et de la gomme
//    			{
//    				yield return new WaitForEndOfFrame();
//    			}

                mainScene.GetComponent<PleaseWaitUI>().SetLoadingMode(true);
                if(!mainScene.GetComponent<PleaseWaitUI>().IsDisplayingIcon())
                    yield return new WaitForEndOfFrame();
    			/*yield return*/ sum.load(buf);//Chargement des donn�es de gazon et de gomme

//    			if(sum.gotGomme)//si donn�es de gomme > application de la gomme
//    			{
//    				yield return StartCoroutine(sum.gommeNode.loadMZ(sum.gommeZone));
//    			}
//    			if(sum.gotGazon)//si donn�es de gazon > application du gazon
//    			{
//    				yield return StartCoroutine(sum.gazonNode.loadMZ(sum.gazonZone));
//    			}
//    			if(!sum.gotGomme && !sum.gotGazon)
//    			{
//    				mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
//    			}
		}

        mainScene.GetComponent<PleaseWaitUI>().SetLoadingMode(false);
        buf.Close();
		isLoading = false;
		canShow();
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
		Camera.main.GetComponent<ObjInteraction>().enabled = true;
		mainNode.transform.FindChild("_avatar").GetComponent<Avatar>().SetForceDisplay(false);
        GameObject.Find("mainCam").GetComponent<MainCamManager>().FitViewportToScreen();
		yield return true;
	}
	
	public IEnumerator save (string name)
	{
		mainScene.GetComponent<PleaseWaitUI>().enabled = true;
		saveState = states.saving;
		//		reactive le composant 
		Camera.main.GetComponent<ObjInteraction>().enabled = false;
		yield return Resources.UnloadUnusedAssets();
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		yield return new WaitForEndOfFrame();
		isLoading = true;
		float t = Time.time;
		MemoryStream stream = new MemoryStream();
		BinaryWriter buf = new BinaryWriter (stream);
		
		//yield return StartCoroutine(Camera.mainCamera.GetComponent<Screenshot>().saveThumbnail());
		#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_STANDALONE_OSX
			yield return StartCoroutine(Camera.main.GetComponent<Screenshot>().takeScreenShotPC(true));
		#endif	
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			yield return StartCoroutine(Camera.main.GetComponent<Screenshot>().saveThumbnailETC());
		#endif
		
		yield return new WaitForEndOfFrame();
		// vv sauvegarde dans le buffer vv
		cdm.save (buf);
		
		sm.save (buf);
		
		om.save (buf);
		
		sum.save(buf);
		
		// vv Sauvegarde dans le fichier vv
		
		buf.Close ();

        string path = "";
        if(!name.Equals(""))
        {
			if(name.Contains("/")){
				GameObject.Find("MainScene").GetComponent<GUIMenuMain>().messError = 7.0f;
			}else{
            	path = usefullData.SavePath +m_client+"/"+ name + usefullData.SaveNewFileExtention;
            	IOutils.CreateMontageFile(stream, path);
			}
        }
        else
            path = IOutils.CreateTmpMontageFile(stream);    // Pas de nom donn�, sauvegarde tmp
		
		yield return new WaitForSeconds(0.25f);
		isLoading = false;
		t = Time.time - t;
		Debug.Log ("Saved in " + path + " in " + t + " sec");

		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
		getListOfSavedFiles();
		if(m_saveInGame)
		{
			GetComponent<GUIStart>().quitExplorerInGame();
		}
		if(m_saveAndQuit)
		{
			m_saveAndQuit = false;
			GetComponent<GUIMenuRight>().SetAllowQuit(true);
            InterProcess.Stop();
			#if UNITY_STANDALONE_WIN 
            System.Diagnostics.Process.GetCurrentProcess().Kill(); 
#else 
            UnityEngine.Application.Quit(); 
#endif
		}
		if(m_saveAndAccueil)
		{
//			StartCoroutine(GetComponent<GUIMenuRight>().LoadAccueil());
			GetComponent<GUIMenuRight>().gotoAccueil();
			GetComponent<GUIStart>().quitExplorerInGame();
		}
		if(m_saveAndSend)
		{
            IOutils.EmailMontageFile(path); // Copy -> email
			//montageMailer(path);
			m_saveAndSend = false;
		}
		if(m_saveAndExport)
		{
            IOutils.ExportMontageFile(path); // Copy -> ordi
			m_saveAndExport = false;
		}
		Camera.main.GetComponent<ObjInteraction>().enabled = true;
		PlayerPrefs.SetString(usefullData.k_selectedProject,name);

		yield return true;
		saveState = states.saved;
	}
	#endregion
	
	//chargement background depuis src externe
	public bool isOnUI()
	{
		return savePrompt || loadPrompt;
	}
	
	public void loadBgFromOutside (string path)
	{
		StartCoroutine (loadFromOutside (path));	
	}
	
	public IEnumerator loadFromOutside (string path)
	{
		string p = "file://" + path;
		www = new WWW (p);
		yield return www;
		
		Texture2D tmp = new Texture2D ((int)www.texture.width, (int)www.texture.height);
		www.LoadImageIntoTexture (tmp);
		
		sm.background = tmp;
		
		GetComponent<GUIMenuMain> ().setStarter (false);
		yield return true;
	}
	
	public void reset()
	{
		StartCoroutine(resetFond()); //ICI
	}
	
	IEnumerator resetFond()
	{
		if(!usefullData.lowTechnologie)
		{
	        eraserNode.GetComponent<EraserV2>().Reset();
	        grassNode.GetComponent<GrassV2>().Reset();
		}
//		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
//		yield return new WaitForEndOfFrame();
//		yield return StartCoroutine(sum.gommeNode.resetTest(false));
////		yield return new WaitForEndOfFrame();
//		yield return StartCoroutine(sum.gazonNode.resetTest(true));
		yield return true;
	}
	
	public bool isOk()
	{
		bool b = true;
		foreach(Transform t in eraserNode.transform)
			if(t.GetComponent<GUITexture>().texture == null){b = false;}
		foreach(Transform t in grassNode.transform)
			if(t.GetComponent<GUITexture>().texture == null){b = false;}
//		if(eraserNode.GetComponent<EraserMask>().isReseting){b = false;}
//		if(grassNode.GetComponent<EraserMask>().isReseting){b = false;}
		
		return b;
	}
	
	bool isInSaves(string nameToTest)
	{
		bool outVal = false;
		foreach(string s in files)
		{
			if(s == nameToTest)
			{
				outVal = true;
			}	
		}
		return outVal;
	}
		
	//Fonction de chargement des objets-------------------------------------------
	//Elle est ici car du fait des librairies onlines, la fonctions de chargement
	//doit etre une IEnumerator (s�quen�age et chargement d'assetBundle oblige)
	//(pas possible de la laisser dans ObjectsModel car pas Monobehaviour)
	//	Attention, fonction longue et imbuvable ^^
	//----------------------------------------------------------------------------
	IEnumerator loadObjs(BinaryReader buf)
	{
        mainScene.GetComponent<PleaseWaitUI>().SetLoadingMode(true);
        mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
        if(!mainScene.GetComponent<PleaseWaitUI>().IsDisplayingIcon())
            yield return new WaitForEndOfFrame ();



		if (buf == null)
			Debug.Log ("BUFFER NULL");
		else
			Debug.Log ("BUFFER PLEIN");
        bool cont = true;
        int nb=0;
        try
        {

            nb = buf.ReadInt32();// Nombre d'objet sauvegard�
            Debug.Log("nombre objet charg�  " + nb);
			mainScene.GetComponent<GUIMenuMain>().flushObjects();// Enleve tout les objets de la scene courante
        }
        catch
		{ Debug.Log("BIZARRE ");
            cont = false;
        }

        if (!cont)
        {
            yield return null;
        }
		for(int i=0;i<nb;i++)
		{
            int typeId, brandId, indexObj;
            Vector3 tmpPos;
            Quaternion tmpRot;
            Vector3 tmpScale;
            IDictionary conf;
            OSLibObject oslibObj;
            OSLibCategory catLvl1;
            bool lockability;
			#region chargement parametres objs 
			//Chargement type/marque/objet
			brandId = -1;
			try{
				brandId = buf.ReadInt32();
			}
			catch
			{
				Debug.Log("ALERTE");
			}
				
			
		



			typeId = buf.ReadInt32();
			indexObj = buf.ReadInt32();
			//correction du bug de la biblioth�que designer de piscine � laquelle on a retir� 2 mod�les
			if(brandId==4016 && indexObj>1)
				indexObj=0;	
			/*if(indexObj>1)
			{
				indexObj = 0;
			}*/
			Debug.Log("brand "+brandId+", type "+typeId+", and index "+indexObj+" loaded");
			oslibObj = new OSLibObject(-1, "", "", "", null, null, null, null, false, false,-1);
			catLvl1 = null;// = om.getMainLib().GetCategoryList ()[typeId];

			if(brandId != -1)
			{
				OSLibCategory catLvl2 = null;
			//	int indexObjSearch=0;
				bool founded = false;
		 		foreach (OSLibCategory catLvl1NewSearch in om.getMainLib().GetCategoryList ())
				{		
					if(founded)
						break;
//					Debug.Log("--------------- Cat�gorie niveau 1 : "+catLvl1NewSearch.GetDefaultText());	
					foreach (OSLibCategory catLvl2NewSearch in catLvl1NewSearch.GetCategoryList ())
					{	
						if(founded)
							break;
//						Debug.Log("--------------- Cat�gorie niveau 2 : "+catLvl2NewSearch.GetDefaultText());	
						foreach (OSLibObject obj in catLvl2NewSearch.GetObjectList())
						{	
							if(founded)
								break;
//							Debug.Log("---------------- -------------- Objets : "+obj.GetDefaultText()+", cat 1 : "+obj.getCategory().GetParentCategory().GetId()+", cat2 : "+obj.getCategory().GetId());	
							if((obj.getCategory().GetParentCategory().GetId()==typeId) && (obj.getCategory().GetId()==brandId))
							{									
								//if (indexObjSearch==indexObj)
								if(indexObj == obj.GetId())
								{
									catLvl1= catLvl1NewSearch;
									catLvl2= catLvl2NewSearch;										
									oslibObj = obj;
									
									founded = true;
									break;
								}
							/*	else
								{
									indexObjSearch++;
								}	*/
							}
						}	
					}
				}
                    Debug.Log("osLib loaded founded "+founded);
				/*
				//if(catLvl1!=null)
					for(int lvl2 = 0;lvl2<catLvl1.GetCategoryList ().Count;lvl2++)
					{
						if(catLvl1.GetCategoryList ()[lvl2].GetId() == brandId)
							catLvl2 = catLvl1.GetCategoryList ()[lvl2];
					}
				//if(catLvl2!=null)
				//	if(catLvl2.GetObjectList().Count>=indexObj)
						oslibObj = catLvl2.GetObjectList () [indexObj];
						*/
			}
			else
			{
		//		if(catLvl1!=null)
		//			if(catLvl1.GetObjectList().Count>=indexObj)
				Debug.Log("ELSE");
						oslibObj = catLvl1.GetObjectList () [indexObj];
			}
					
			//chargement du spatial
			//position
                tmpPos = new Vector3(0, 0, 0);
			tmpPos.x = buf.ReadSingle();
			tmpPos.y = buf.ReadSingle();
			tmpPos.z = buf.ReadSingle();
			
			//rotation
			tmpRot = new Quaternion(0, 0, 0, 0);
			tmpRot.x = buf.ReadSingle();
			tmpRot.y = buf.ReadSingle();
			tmpRot.z = buf.ReadSingle();
			tmpRot.w = buf.ReadSingle();

            tmpScale = new Vector3(1, 1, 1);
			if(!LibraryLoader.numVersionInferieur(cdm.versionSave,"1.6.5"))
			{
				//scale	
				tmpScale.x = buf.ReadSingle();
				tmpScale.y = buf.ReadSingle();
				tmpScale.z = buf.ReadSingle();
			}
                Debug.Log("pos and rot laded");
			//LockAbility
                lockability = buf.ReadBoolean();
                Debug.Log("lockability loaded");
			
			//Chargement Configuration textures/couleurs
			int confCount = buf.ReadInt32();
                Debug.Log("confCount loaded");
                conf = new SortedList();
			for(int a=0;a<confCount;a++)
			{	
                    try
                    {
					string objTx = buf.ReadString();
					int txIdx = buf.ReadInt32();
					if(!conf.Contains(objTx))
						conf.Add(objTx,txIdx);
				}
				catch (EndOfStreamException e)
				{
                        Debug.LogWarning("endofstream exception : "+e.Message);
                        m_errorLoading = true;
                        mainScene.GetComponent<PleaseWaitUI>().SetLoadingMode(false);
                        buf.Close();
                        isLoading = false;
                        canShow();
                        //		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
                        Camera.main.GetComponent<ObjInteraction>().enabled = true;
                        mainNode.transform.FindChild("_avatar").GetComponent<Avatar>().SetForceDisplay(false);
                        GameObject.Find("mainCam").GetComponent<MainCamManager>().FitViewportToScreen();
                        mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
                        break;
				}
			}
                if (m_errorLoading)
                {
                    yield return true;
                }
                Debug.Log("config tex/color loaded");
			#endregion
           // }
            /*
            catch (EndOfStreamException ue)
            {
                Debug.Log("error chargement param objs :" + ue.ToString());
                break;
            }*/
			#region instanciation Obj
			// ----------- d�but de cr�ation de l'objet ------------------

			//Ouverture assetBundle
			//if(oslibObj!=null)
			{
				while (assetBundleLock)
				{
					yield return new WaitForEndOfFrame ();	
				}
				OSLib objLib = oslibObj.GetLibrary ();
				if(objLib != null)
				{
				Montage.assetBundleLock = true;
				WWW www = WWW.LoadFromCacheOrDownload (objLib.GetAssetBundlePath (), objLib.GetVersion ());
				yield return www;

                GameObject go;
                Transform newObj;
                AssetBundle assetBundle;
                ObjData data;
                try
                {
                    assetBundle = www.assetBundle;
				//Chargement de l'objet(GameObject)
				UnityEngine.Object original = assetBundle.LoadAsset (oslibObj.GetModel ().GetPath (),typeof(GameObject));
				//instanciation
				//Debug.Log(typeId+ " "+brandId+" "+indexObj+" "+original);
					
                    go = (GameObject)Instantiate(original, tmpPos, tmpRot);
					go.transform.localScale = tmpScale;
				if(go.GetComponent<Rigidbody>())
					go.GetComponent<Rigidbody>().isKinematic = true;
                   newObj = go.transform;
				//Configuraton de lobjet
				newObj.parent = mainNode.transform;
				newObj.name = newObj.name + i;
				newObj.gameObject.AddComponent <ObjBehav>();
				newObj.gameObject.AddComponent <ApplyShader>();
				newObj.gameObject.layer = 9;
				
                    data = (ObjData)newObj.gameObject.AddComponent<ObjData>();
                }
                catch (UnityException ue)
                {
                    Debug.Log("instanciation objs :" + ue.ToString());
                    break;
                }
				yield return new WaitForEndOfFrame(); // attend la fin du start du objData
				data.SetObjectModel (oslibObj, assetBundle);
				assetBundle.Unload (false);
				Montage.assetBundleLock = false;
				yield return new WaitForEndOfFrame(); // attend la fin du start de ObjBehav
	//			data.loadConf(conf);
				yield return StartCoroutine(data.loadConfIE(conf));
                try
                {
				float y = newObj.gameObject.GetComponent<ObjBehav>().init(); // > met a la bonne position en y
				newObj.gameObject.GetComponent<ObjBehav>().iAmLocked(lockability);
				newObj.transform.position = new Vector3(newObj.transform.position.x,
				                                        newObj.transform.position.y+y,
				                                        newObj.transform.position.z);
		
				string pref = newObj.GetComponent<ObjData> ().GetObjectModel ().GetObjectType ();
				//string pref = originalsNode.transform.GetChild(newObj.GetComponent<ObjData>().getTyp()).name;
				if(go.GetComponent<MeshCollider>())// <- ca aussi ca doit etre mis dans les libs
				{
					go.GetComponent<MeshCollider>().convex = true;
					go.transform.localRotation = tmpRot; // Correction du bug qui fait que la piscinne avec le canard est charg�e de travers
				}
				/*if (newObj.GetComponent<ObjData> ().GetObjectModel ().GetModules().FindModule("liner")!=null)//TODO Temporaire a enlever plus tard
				{
					newObj.FindChild("frise").gameObject.AddComponent<PoolFrise>();
				}*/
				mainScene.GetComponent<GUIMenuLeft>().updateSceneObj();
				
				newObj.GetComponent<ObjBehav> ().SetHeighOff7(tmpPos.y);

                // -- Ajout du nouvel objet au mode2D si besoin --
                OSLibObject obj = newObj.transform.GetComponent<ObjData>().GetObjectModel();
					if(obj.GetObjectType() == "pool" || obj.GetObjectType() == "dynamicShelter" ||  obj.IsMode2D())
                    m_mode2D.AddObj(newObj.transform);

				// ----------- fin de cr�ation de l'objet ------------------
				#endregion
                    Debug.Log("instanciation obj loaded");
                }
                catch (EndOfStreamException ue)
                {
                    Debug.Log("instanciation objs :" + ue.ToString());
                    break;
                }
				#region chargement SpecFcn			
				//Chargement des Fonctions Sp�cifiques
                try
                {
						int spcFcnNb = buf.ReadInt32();
				//GameObject tmp = new GameObject();
				for(int s=0;s<spcFcnNb;s++)
				{
					string type = buf.ReadString();
					if(type == "Function_PergolaAutoFeet" && LibraryLoader.numVersionInferieur(cdm.versionSave,"1.2.95"))
					{
						type = "FunctionConf_PergolaAutoFeet";	
					}
					int id = buf.ReadInt32();
					foreach(Component c in go.GetComponents<MonoBehaviour>())
					{
						if(c.GetType().ToString().StartsWith("Function"))
						{
							if(((Function_OS3D)c).GetFunctionId() == id && c.GetType().ToString() == type)
							{
								((Function_OS3D)c).load(buf);	
							}
						}				
					}
					/*
					if(go.GetComponent(type))
					{
						((Function_OS3D) go.GetComponent(type)).load(buf);
					}
					else
					{
						go.AddComponent(type);
						data.updateSpecFcns();
						((Function_OS3D) go.GetComponent(type)).load(buf);
					}
					*/
				}
                    Debug.Log("spcfnc loaded");
                }
                catch (EndOfStreamException ue)
                {
                    Debug.Log("spec fcn:" + ue.ToString());
                    break;
                }
				//-----------------------------------------------------
				#endregion
				mainScene.GetComponent<PleaseWaitUI>().setAdditionalLoadingTxt(TextManager.GetText("Montage.LoadingObject")+" "+(i+1)+"/"+nb);
			}
			}
		}

        mainScene.GetComponent<PleaseWaitUI>().SetLoadingMode(false);
        mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
//		yield return new WaitForEndOfFrame();
//		yield return Resources.UnloadUnusedAssets();
		yield return null;
	}
	
	public string getClientStr()
	{
		return m_client;
	}
	
	public IEnumerator SaveInGame(string _szname)
	{
		//		_autoSaveName = "AutoSave"+Time.time;
		m_saveInGame = true;
		yield return StartCoroutine(save(_szname));
	}
	public IEnumerator SaveAndSend()
	{
//		_autoSaveName = "AutoSave"+Time.time;
		m_saveAndSend = true;
		yield return StartCoroutine(save(""));
	}
	public IEnumerator SaveAndExport()
	{
//		_autoSaveName = "AutoSave"+Time.time;
		m_saveAndExport = true;
		yield return StartCoroutine(save(""));
	}

    public void AskToLoadScene(string scene)
    {
        sceneToLoad = scene;
    }

	
//	void endMail(string result)
//	{
//		Debug.Log("Mail Sender : "+result);
//#if UNITY_IPHONE
//		EtceteraManager.setPrompt(false);
//#endif
//	}


    public bool SaveWithName(string name)
    {
		try{
        Debug.Log("Enregistrement de la sc�ne");
        mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
        Camera.main.GetComponent<ObjInteraction>().enabled = false;
        // Resources.UnloadUnusedAssets();
        isLoading = true;
        float t = Time.time;
        MemoryStream stream = new MemoryStream();
        BinaryWriter buf = new BinaryWriter(stream);
        cdm.save(buf);

        sm.save(buf);

        om.save(buf);

        sum.save(buf);
        Debug.Log("5");
        // vv Sauvegarde dans le fichier vv

        buf.Close();
        string path = "";
        if (!name.Equals(""))
        {
            path = usefullData.SavePath + name + usefullData.SaveNewFileExtention;
            Debug.Log("sc�ne enregistr�e ? path : " + path);
            IOutils.CreateMontageFile(stream, path);
        }
        else
        {
            Debug.Log("name = ''");
            path = IOutils.CreateTmpMontageFile(stream);    // Pas de nom donn�, sauvegarde tmp

        }
        isLoading = false;
        Camera.main.GetComponent<ObjInteraction>().enabled = true;
        mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
			return true;
    }
		catch(UnityException e){
			return false;
		}
    }



    public IEnumerator tutoSave(string name)
    {
        Debug.Log("Enregistrement de la sc�ne");
        mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
        Camera.main.GetComponent<ObjInteraction>().enabled = false;
        yield return Resources.UnloadUnusedAssets();
        yield return new WaitForEndOfFrame();
        isLoading = true;
        float t = Time.time;
        MemoryStream stream = new MemoryStream();
        BinaryWriter buf = new BinaryWriter(stream);
        //yield return StartCoroutine(Camera.mainCamera.GetComponent<Screenshot>().saveThumbnail());
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_STANDALONE_OSX
        yield return StartCoroutine(Camera.main.GetComponent<Screenshot>().takeScreenShotPC(true));
#endif
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			                yield return StartCoroutine(Camera.main.GetComponent<Screenshot>().saveThumbnailETC());
#endif
        //yield return new WaitForEndOfFrame();
        // vv sauvegarde dans le buffer vv
        cdm.save(buf);

        sm.save(buf);

        om.save(buf);

        sum.save(buf);
        // vv Sauvegarde dans le fichier vv

        buf.Close();

        string path = "";
        if (!name.Equals(""))
        {
            path = usefullData.SavePath + name + usefullData.SaveNewFileExtention;
            Debug.Log("sc�ne enregistr�e ? path : " + path);
            IOutils.CreateMontageFile(stream, path);
        }
        else
        {
            Debug.Log("name = ''");
            path = IOutils.CreateTmpMontageFile(stream);    // Pas de nom donn�, sauvegarde tmp

        }
        Camera.main.GetComponent<ObjInteraction>().enabled = true;
        mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
    }
	
}
