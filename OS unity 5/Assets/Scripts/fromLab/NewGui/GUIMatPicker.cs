using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pointcube.Global;

public class GUIMatPicker : MonoBehaviour, GUIInterface{
	GUIItemV2 			_root;
	GUIItemV2 			_matTexMenu;
	GUIUpperList 		_matTexList;
	Vector2 			scrollpos = new Vector2(0,0);
	Rect 				m_menuGroup;

	public GameObject 	m_mainCam;
	public GameObject 	m_mainScene;
	public GameObject 	m_backgroundImg;
	public GUISkin		skin;
	public GameObject 	m_grassSkybox;

	private bool 		canDisp = true;
	private bool 		visibility = false;
	private Texture2D[] m_texArray;
	private int         m_synthTexButtonCount;     // Nombre de textures synthétisées à afficher
	private Object[]    m_grassTex;                // Textures du dossier Resources/grass (chargées si besoin)
	private Object[]	m_grassThumbs;
	private Texture2D   m_grassCurTex;             // Texture actuelle

	GUIItemV2 _newGazonMat;

	// Use this for initialization
	void Start () {
		m_menuGroup = new Rect();
		m_menuGroup.Set(Screen.width-260,0,260,Screen.height);

		m_grassCurTex = (Texture2D) GameObject.Find("grassGround").GetComponent<Renderer>().material.GetTexture("_MainTex");
		_matTexMenu = new GUIItemV2 (0, 0, "Material", "sousMenuOn", "sousMenuOff", this);
		_matTexList = new GUIUpperList (1, 0, TextManager.GetText("material"), "sousMenuOn", "sousMenuOff", this);
		
		_matTexMenu.addSubItem (_matTexList);
		enabled = false;
	}
	void OnEnable()
	{		
		_root = new GUIItemV2(-1, -1, "Root", "", "", this);
		_root.addSubItem (_matTexMenu);
		setContent();
	}
	// Update is called once per frame
	void Update () {
		float deltaScroll;
		if(PC.In.CursorOnUI(m_menuGroup) && PC.In.ScrollViewV(out deltaScroll))
			scrollpos.y += deltaScroll;
	
		if(isVisible())
		{
			if(_matTexList.getSelectedItem() != null)
			{
				//{
					Texture2D selectedTex = m_grassCurTex;
					int i = 0;
					for(;i<m_texArray.Length;i++)
					{
						if(m_texArray[i].name == _matTexList.getSelectedItem().m_text){
							selectedTex =(Texture2D) m_texArray[i];
							break;
						}
					}
						_matTexList.getSelectedItem().SetToogleActivated(true);
						m_grassCurTex = selectedTex;
						//Debug.Log(""+m_grassCurTex.name);
						GetComponent<GUISubTools>().setCurrentMaterial(m_grassCurTex);
						m_grassSkybox.GetComponent<GrassHandler>().SetUsedTexture(i);
						m_mainScene.GetComponent<GUISubTools>().SetSynthesizedGrassTex(m_grassCurTex);
						m_mainScene.GetComponent<GUISubTools>().Validate ();
						
				//}
			}	

		}
	}
	protected void OnGUI ()
	{
		// stair model UI
		if (_matTexList != null)
		{
			GUISkin bkup = GUI.skin;
			GUI.skin = skin;
			
			_matTexList.display ();
			/*if(GUI.Button(new Rect(_matTexList.bgThumbRect.yMax+50, _matTexList.bgThumbRect.xMax, 50, 50), "","Add"))
			{
				m_backgroundImg.AddComponent("GrassSynthesizer");
				m_backgroundImg.GetComponent<GrassSynthesizer>().m_backgroundImg = m_backgroundImg;
				m_backgroundImg.GetComponent<GrassSynthesizer>().m_mainCam = m_mainCam;
				hideUI();
			}*/ // test selecteur texture
			GUI.skin = bkup;

		}

	}
	public void setVisibility (bool b)
	{

	}
	
	public void canDisplay (bool b)
	{
		canDisp = b;
	}
	public bool getDisplay(){
		return canDisp;
	}
	public bool isOnUI ()
	{
		return false;
	}
	
	public bool isVisible()
	{
		return visibility;
	}
	
	public void CreateGui ()
	{
		
	}
	public void updateGUI (GUIItemV2 itm,int val,bool reset)
	{

	}
	public void setContent()
	{
		if(m_grassTex == null)
			m_grassTex = m_grassSkybox.GetComponent<GrassHandler>().GetDefaultTexs();

		if(m_grassThumbs == null){
			m_grassThumbs = m_grassSkybox.GetComponent<GrassHandler>().GetDefaultThumbs();
		}
		int i=0;
		if(m_grassThumbs.Length == m_grassTex.Length){
			int count = m_grassTex.Length;
			m_texArray = new Texture2D[count];
			Texture2D[] thumbsArray = new Texture2D[count];
			for(;i<count;i++){
				m_texArray[i] =(Texture2D) m_grassTex[i];
				thumbsArray[i] =(Texture2D) m_grassThumbs[i];
			}
		
			Texture2D grassTex;
			i=0;
			_matTexList = new GUIUpperList (1, 0, TextManager.GetText("Material"), "sousMenuOn", "sousMenuOff", this);
			_matTexList.setImgContent(m_texArray, thumbsArray);
			//_matTexList.display();
			visibility = true;
		}
	}
	public void hideUI()
	{
		this.enabled=false;
	}

}
