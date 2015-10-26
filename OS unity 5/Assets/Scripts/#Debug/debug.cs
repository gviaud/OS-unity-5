using UnityEngine;
using System.Collections;
using System.IO;

using Pointcube.Utils;

public class debug : MonoBehaviour {
	
	string All;
	string Textures;
	string Meshes;
	string Materials;
	string GameObjects;
	string Components; 
	string memory;
	GameObject avatar;
	GameObject grid;
	
	public bool activeDebug = false;
	
	private string[] konamiCode = { KeyCode.UpArrow.ToString(),KeyCode.UpArrow.ToString(),
									KeyCode.DownArrow.ToString(),KeyCode.DownArrow.ToString(),
									KeyCode.LeftArrow.ToString(),KeyCode.RightArrow.ToString(),
									KeyCode.LeftArrow.ToString(),KeyCode.RightArrow.ToString(),
									KeyCode.A.ToString(),KeyCode.B.ToString(),};
	private int _kcId = 0;
	
	
	// -- GUI debug --
	private Rect 		m_GUIdbgRect;
	private Vector2		m_scrollVect;
	public  int  		m_GUIdbgWidth	= 750;
	
	#region unity_func
	//-----------------------------------------------------
	void OnEnable()
	{
		UsefullEvents.OnResizingWindow  += ResizeRects;
		UsefullEvents.OnResizeWindowEnd += ResizeRects;	
	}
	
	//-----------------------------------------------------
	void Start ()
	{
//		Debug.Log("azerty ?"+Directory.Exists(Application.dataPath+"/Resources"));
		avatar = GameObject.Find("boy");
		grid = GameObject.Find("grid");
		
		m_GUIdbgRect = new Rect(0f, 0f, m_GUIdbgWidth, 0f);
		m_scrollVect = new Vector2();
		ResizeRects();
	}
	

	//-----------------------------------------------------
	void Update ()
	{
		if(activeDebug)
		{
			All = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)).Length.ToString();
			Textures = Resources.FindObjectsOfTypeAll(typeof(Texture)).Length.ToString();
			Meshes = Resources.FindObjectsOfTypeAll(typeof(Mesh)).Length.ToString();
			Materials = Resources.FindObjectsOfTypeAll(typeof(Material)).Length.ToString();
			GameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Length.ToString();
			Components = Resources.FindObjectsOfTypeAll(typeof(Component)).Length.ToString();
			memory = System.GC.GetTotalMemory(false).ToString();
		}
		if(Input.touchCount == 4)
		{
			Rect rhg = new Rect(0,0,50,50);
			Rect rhd = new Rect(Screen.width-50,0,50,50);
			Rect rbg = new Rect(0,Screen.height-50,50,50);
			Rect rbd = new Rect(Screen.width-50,Screen.height-50,50,50);
			
			bool bhg = false;
			bool bhd = false;
			bool bbg = false;
			bool bbd = false;
			
			foreach(Touch t in Input.touches)
			{
				if(rhg.Contains(t.position))
					bhg = true;
				if(rhd.Contains(t.position))
					bhd = true;
				if(rbg.Contains(t.position))
					bbg = true;
				if(rbd.Contains(t.position))
					bbd = true;
			}
			
			if(bhg&&bhd&&bbd&&bbg)
			{
				activeDebug = !activeDebug;
				bhg = bhd=bbg=bbd=false;
			}
		}
		
		PcUnlock();
	}
	

	//-----------------------------------------------------
	void OnGUI()
	{
		if(activeDebug)
		{
			GUILayout.BeginArea(new Rect(0,/*(Screen.height/2)+*/20,250,Screen.height-20),GUI.skin.FindStyle("Box"));
			GUILayout.Label("All " + All);
			GUILayout.Label("Textures " + Textures);
			GUILayout.Label("Meshes " + Meshes);
			GUILayout.Label("Materials " + Materials);
			GUILayout.Label("GameObjects " + GameObjects);
			GUILayout.Label("Components " + Components);
			GUILayout.Label("MEMORY  " + memory);
			if(avatar!=null)
				GUILayout.Label("avatar  " + avatar.GetComponent<Renderer>().enabled);
			if(grid!=null)
				GUILayout.Label("grid  " + grid.GetComponent<Renderer>().enabled);
			if(PlayerPrefs.HasKey(usefullData.k_logIn))
				GUILayout.Label("ID >"+PlayerPrefs.GetString(usefullData.k_logIn));
			
			if(GUILayout.Button("GC",GUILayout.Width(100),GUILayout.Height(30)))
				System.GC.Collect();
			if(GUILayout.Button("Clear unused",GUILayout.Width(100),GUILayout.Height(30)))
				Resources.UnloadUnusedAssets();
			if(GUILayout.Button("Textures",GUILayout.Width(100),GUILayout.Height(30)))
				texPrint();
			if(GUILayout.Button("CleanCache",GUILayout.Width(100),GUILayout.Height(30)))
				Caching.CleanCache();
			if(GUILayout.Button("CleanPrefs",GUILayout.Width(100),GUILayout.Height(30)))
				PlayerPrefs.DeleteAll();
	
			if(GUILayout.Button("Gyro settings",GUILayout.Width(100),GUILayout.Height(30)))
			{
				GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = !GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled;
				GameObject.Find("camPivot").GetComponent<gyroControl_v2>().SetDebugMode();
			}
            if(GUILayout.Button("GUI Log",GUILayout.Width(100),GUILayout.Height(30)))
                DbgUtils.ToggleGUIdebug();
            if(DbgUtils.GUIdebugEnabled())
                if(GUILayout.Button("Clear GUI Log",GUILayout.Width(100),GUILayout.Height(30)))
                    DbgUtils.ClearLog();
			GUILayout.EndArea();
			
		
			if(DbgUtils.GUIdebugEnabled())
			{
				GUILayout.BeginArea(m_GUIdbgRect);
				m_scrollVect = GUILayout.BeginScrollView(m_scrollVect, GUI.skin.FindStyle("Box"));
				GUILayout.TextArea(DbgUtils.GetLog(0));
				GUILayout.EndScrollView();
				GUILayout.EndArea();
			}
		}
	}
	
	//-----------------------------------------------------
	void OnDisable()
	{
		UsefullEvents.OnResizingWindow  -= ResizeRects;
		UsefullEvents.OnResizeWindowEnd -= ResizeRects;	
	}
	
	#endregion
	
	//-----------------------------------------------------
	private void ResizeRects()
	{
		m_GUIdbgRect.x = Screen.width-m_GUIdbgWidth;
		m_GUIdbgRect.height = Screen.height; 
	}
	
	//-----------------------------------------------------
	void PcUnlock()
	{		
		if(Input.GetKeyUp(KeyCode.UpArrow))
		{
			if(konamiCode[_kcId] == KeyCode.UpArrow.ToString())
				_kcId ++;
			else
				_kcId = 0;
			
			if(_kcId == konamiCode.Length)
			{
				activeDebug = !activeDebug;
				_kcId = 0;
			}
		}
		if(Input.GetKeyUp(KeyCode.DownArrow))
		{
			if(konamiCode[_kcId] == KeyCode.DownArrow.ToString())
				_kcId ++;
			else
				_kcId = 0;
			
			if(_kcId == konamiCode.Length)
			{
				activeDebug = !activeDebug;
				_kcId = 0;
			}
		}
		
		if(Input.GetKeyUp(KeyCode.LeftArrow))
		{
			if(konamiCode[_kcId] == KeyCode.LeftArrow.ToString())
				_kcId ++;
			else
				_kcId = 0;
			
			if(_kcId == konamiCode.Length)
			{
				activeDebug = !activeDebug;
				_kcId = 0;
			}
		}
		if(Input.GetKeyUp(KeyCode.RightArrow))
		{
			if(konamiCode[_kcId] == KeyCode.RightArrow.ToString())
				_kcId ++;
			else
				_kcId = 0;
			
			if(_kcId == konamiCode.Length)
			{
				activeDebug = !activeDebug;
				_kcId = 0;
			}
		}
		
		if(Input.GetKeyUp(KeyCode.A))
		{
			if(konamiCode[_kcId] == KeyCode.A.ToString())
				_kcId ++;
			else
				_kcId = 0;
			
			if(_kcId == konamiCode.Length)
			{
				activeDebug = !activeDebug;
				_kcId = 0;
			}
		}
		if(Input.GetKeyUp(KeyCode.B))
		{
			if(konamiCode[_kcId] == KeyCode.B.ToString())
				_kcId ++;
			else
				_kcId = 0;
			
			if(_kcId == konamiCode.Length)
			{
				activeDebug = !activeDebug;
				_kcId = 0;
			}
		}	
	}
	
	
	//-----------------------------------------------------
	void texPrint()
	{
		foreach(Texture t in Resources.FindObjectsOfTypeAll(typeof(Texture)))
			Debug.Log(t.name);
	}
}
