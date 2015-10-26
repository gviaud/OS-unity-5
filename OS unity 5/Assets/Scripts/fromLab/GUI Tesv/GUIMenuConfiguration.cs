using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pointcube.Global;
using System.Linq;
public class GUIMenuConfiguration : MonoBehaviour,GUIInterface 
{

	GUIItemV2 Root;
	public GUIItemV2 activeItem;
	GUIItemV2 specFunctions;

	ArrayList othersMenu;

	public Texture background;

	//UI
	public GUISkin skin;
	bool showMenu = false;
//    private m_justShowedMenu;
	public float m_off7 = Screen.width;       // TODO passer en private
	Vector2 scrollpos = new Vector2(0,0);//vertical scroll menu trop grand
	
	Rect m_menuGroup;
    public Rect m_areaRect;                 // TODO passer en private
	
	int selConfig = -1;
	int selObj = -1;
	int selT = -1;
	
	//chargement textures
	Texture2D[] tmpTex;
	public GameObject obj2config;
	public GameObject picker;
	//Functions spe
	MonoBehaviour[] components;

    public bool ShowHelpPannel=false;
	
	bool _assetBundleIsOpen = false;

	public bool tempHide_Plage = false;

//Fonctions---------------------------

    //----------------------------------------------------
    void Awake()
    {
        UsefullEvents.OnResizeWindowEnd += SetRects;
        UsefullEvents.OnResizingWindow  += SetRects;

//        m_justShowedMenu = false;
    }

    //----------------------------------------------------
	void Start ()
	{
		m_menuGroup = new Rect();
        m_areaRect  = new Rect();
        SetRects();
		
		othersMenu = new ArrayList();
		foreach(Component cp in this.GetComponents<MonoBehaviour>())
		{
			if(cp.GetType() != this.GetType() && cp.GetType().GetInterface("GUIInterface")!= null)
			{
				othersMenu.Add(cp);
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		panelAnimation();
		
		//Slide menus
        float deltaScroll;
        if(PC.In.CursorOnUI(m_menuGroup) && PC.In.ScrollViewV(out deltaScroll))
            scrollpos.y += deltaScroll;

//		if(Input.touchCount>0)
//		{
//			Touch t = Input.touches[0];
//			if(m_menuGroup.Contains(t.position) && t.deltaPosition.y!=0)
//			{
//				scrollpos.y = scrollpos.y + t.deltaPosition.y;	
//			}
//		}
	}
	
	void OnGUI()
	{
		GUISkin bkup = GUI.skin;
		GUI.skin = skin;
		
		//		GUI.Box(menuGroup,"MENUCONFIG");
		
		//MENU
		if(showMenu || m_off7<Screen.width)
		{
			GUI.DrawTexture(new Rect (m_off7+50,0,background.width-50, background.height),background);
			GUILayout.BeginArea(m_areaRect);
			GUILayout.FlexibleSpace();
			
			scrollpos = GUILayout.BeginScrollView(scrollpos,"empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
			
			GUILayout.Box("","bgFadeUp",GUILayout.Width(210),GUILayout.Height(150));//fade en haut
			GUILayout.BeginVertical("bgFadeMid",GUILayout.Width(210));
			
			Root.showSubItms();//Menu

			if(activeItem != null)
			{
				//print (activeItem.m_text);
				
				if(activeItem.m_text != "plage" && activeItem.m_text != "Plage" )
				{
					if( obj2config != null)
						if( obj2config.GetComponent<Function_hideObject> () != null)
						GetComponent<GUIMenuInteraction> ().m_Hide_Plage = obj2config.GetComponent<Function_hideObject> ()._hide;
					//print (GetComponent<GUIMenuInteraction> ().m_Hide_Plage);
				}
			}

			GUILayout.EndVertical();
			GUILayout.Box("","bgFadeDw",GUILayout.Width(210),GUILayout.Height(150));//fade en bas
			
			GUILayout.EndScrollView();
			
			GUILayout.FlexibleSpace();
			GUILayout.EndArea();
			
			if(activeItem != null && showMenu)
			{
				if(activeItem.GetType() == typeof(GUIUpperList))
				{
					((GUIUpperList)activeItem).display();
				}
			}
		}

		GUI.skin = bkup;
	}
   

    //----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizeWindowEnd -= SetRects;
        UsefullEvents.OnResizingWindow  -= SetRects;
    }
//FCN. AUX. PRIVEE--------------------

    private void SetRects()
    {
        m_menuGroup.Set(Screen.width-260,0,260,Screen.height);

        if(showMenu)
            m_off7 = Screen.width - 260; // (cf. panel animation limit)
        else
            m_off7 = Screen.width;

        m_areaRect.Set(m_off7, 0, 300, Screen.height);
    }

	private void panelAnimation()
	{
		if(showMenu)
		{
			float limit = Screen.width-260;
			
			if(m_off7>limit)
			{
				if(m_off7<limit-1)
				{
					m_off7 = limit;
					m_menuGroup.x = m_off7;
				}
				else
				{
					m_off7 = Mathf.Lerp(m_off7,limit,5*Time.deltaTime);
					m_menuGroup.x = m_off7;
				}
			}
		}
		else
		{
			if(m_off7<Screen.width)
			{
				if(m_off7>(Screen.width-1))
				{
					m_off7 = Screen.width;
					m_menuGroup.x = m_off7;
					resetMenu();
				}
				else
				{
					m_off7 = Mathf.Lerp(m_off7,Screen.width,5*Time.deltaTime);
					m_menuGroup.x = m_off7;
				}
			}
		}
        m_areaRect.x = m_off7+50;
	}
	
	private void resetMenu()
	{
		selConfig = -1;
		selObj = -1;
		selT = -1;


		if(Root.getSelectedItem()!= null)//menu
		{
			if(Root.getSelectedItem().getSelectedItem()!=null)//sous menu
			{
				if(Root.getSelectedItem().getSelectedItem().getSelectedItem() != null)//tool
				{
					Root.getSelectedItem().getSelectedItem().resetSelected();
				}
				Root.getSelectedItem().resetSelected();
			}
			Root.resetSelected();
		}
	}
	
	private void ui2fcn()
	{
		/*switch (selConfig)
		{
		case 0: // reglage des maps
			mapsControl();
			break;
		
		case 1: // fcn. Speciales
			specFcnControl();
			break;
		
		}*/


		if(selConfig == 0)
		{
			mapsControl();
		}
		else if(selConfig > 0)
		{
			specFcnControl();
			//resetMenu();
		}
	}
	
	private void mapsControl()
	{
		if(selT != -1)
		{
			//			string subObjName = "";
			int index = 0;
			foreach(Transform t in obj2config.transform)
			{
				if(t.name != activeItem.m_text)
					index++;
				else
				{
					//					subObjName = t.name;
					break;
				}
			}
			//			Debug.Log (selConfig + " " + selObj + " " + selT);
			OSLibObject selectedObjectModel = obj2config.GetComponent<ObjData>().GetObjectModel ();
			OSLibModules modules = selectedObjectModel.GetModules ();
			OSLibModule module = modules.GetStandardModuleList () [selObj];
			Transform objToModif = obj2config.transform.Find (module.GetModuleType());
			Debug.Log("objToModif >"+objToModif);

			Function_hideObject hider = obj2config.GetComponent<Function_hideObject> ();
			if( hider != null )
				tempHide_Plage = hider._hide;
			if (hider != null && hider._hide && objToModif.name == "plage")
			{
				hider.DoAction2 ();
			}

			if (objToModif != null)
			{
				//				int textureCount = module.GetTextureList ().Count;
				int colorCount = module.GetColorList ().Count;
				
				if (selT >= colorCount)
				{
					//Debug.Log ("SET Texture INDEX " + selT);
					OSLibTexture textureData = 	module.GetTextureList () [selT - colorCount];

					objToModif.GetComponent<Renderer>().material.color = new Color(0.78f,0.78f,0.78f,1.0f);
					
					StartCoroutine (ApplyTexture (objToModif, textureData, selectedObjectModel.GetLibrary ()));
					
					if(textureData.GetTextureType2()!=null)
					{
						Transform objToModif2 = obj2config.transform.Find (textureData.GetTextureType2());	
						if (objToModif2 != null)
						{
							objToModif2.GetComponent<Renderer>().material.color = new Color(0.78f,0.78f,0.78f,1.0f);
							StartCoroutine (ApplyTexture2 (objToModif2, textureData, selectedObjectModel.GetLibrary ()));
						}
					}
				}
				else if (selT < colorCount)
				{
					int colorIndex = Mathf.Abs (selT);
					if(objToModif.GetComponent<MeshRenderer>())
					{
						objToModif.GetComponent<Renderer>().material.mainTexture = null;
						if((objToModif.GetComponent<Renderer>().material.HasProperty("_DecalTex")) && (usefullData.lowTechnologie))
						{
							objToModif.GetComponent<Renderer>().material.mainTexture = objToModif.GetComponent<Renderer>().material.GetTexture("_DecalTex");
						}
					}
					OSLibColor osLibColor=module.GetColorList () [colorIndex];
					objToModif.GetComponent<Renderer>().material.color = osLibColor.GetColor ();
					if(osLibColor.GetColorType2()!=null)
					{
						Color color2 = osLibColor.GetColor2();
						Transform objToModif2 = obj2config.transform.Find (
							osLibColor.GetColorType2());
						//	Debug.Log("objToModif2 >"+objToModif2);
						if (objToModif2 != null)
						{
							objToModif2.GetComponent<Renderer>().material.color = color2;
							if(objToModif2.GetComponent<MeshRenderer>())
							{
								objToModif2.GetComponent<Renderer>().material.mainTexture = null;
								if((objToModif2.GetComponent<Renderer>().material.HasProperty("_DecalTex")) && (usefullData.lowTechnologie))
								{
									objToModif2.GetComponent<Renderer>().material.mainTexture = objToModif2.GetComponent<Renderer>().material.GetTexture("_DecalTex");
								}
							}
						}
					}
				}
			}
	
			//obj2config.transform.GetChild(index).renderer.material.mainTexture = ((GUIGridV2)activeItem).getImgContent(selT);
			//			obj2config.GetComponent<ObjData>().setSubId(subObjName,selT);
			obj2config.GetComponent<ObjData>().setSubId(module.GetModuleType(),selT);
		}
		else
		{
			if(selObj != -1)
			{
				OSLibObject selectedObjectModel = obj2config.GetComponent<ObjData>().GetObjectModel ();
				OSLibModules modules = selectedObjectModel.GetModules ();
				OSLibModule module = modules.GetStandardModuleList () [selObj];
				string name = module.GetModuleType();
				if(obj2config.GetComponent<ObjData>().getConfiguration().Contains(name))
				{
					((/*GUIGridV2*/GUIUpperList)activeItem).setUiId((int)obj2config.GetComponent<ObjData>().getConfiguration()[name]);
				}
			}
		}
	}
	
	private IEnumerator ApplyTexture (Transform target, OSLibTexture textureData, OSLib library)
	{
		while(_assetBundleIsOpen)
			yield return new WaitForEndOfFrame();
		_assetBundleIsOpen = true;
		WWW www = WWW.LoadFromCacheOrDownload (library.GetAssetBundlePath (), library.GetVersion ());
		yield return www;
		
		AssetBundle assets = www.assetBundle;

		Texture2D texture = assets.LoadAsset (textureData.GetFilePath (), 
										 typeof (Texture2D)) as Texture2D;
		Texture2D normalMap = null;
		if (textureData.GetNormalPath () != "")
			normalMap = assets.LoadAsset (textureData.GetNormalPath (), 
		                                        typeof (Texture2D)) as Texture2D;

		Texture2D hueMask = null;
		if (textureData.GetHueMaskPath () != "")
			hueMask = assets.LoadAsset (textureData.GetHueMaskPath (), 
			                              typeof (Texture2D)) as Texture2D;

		Texture2D specularMask = null;
		if (textureData.GetSpecularMaskPath () != "")
			specularMask = assets.LoadAsset (textureData.GetSpecularMaskPath (), 
			                            typeof (Texture2D)) as Texture2D;

		assets.Unload (false);
		target.GetComponent<Renderer>().material.mainTexture = texture;
		float scale = textureData.GetScale();
		/*if (target.name.CompareTo("plage")==0)
			scale/=2.0f;*/
		if (target.name.CompareTo("sol")==0)
			scale/=2.0f;
		if(target.GetComponent<Renderer>().material.shader.name == "Pointcube/StandardObjet")
		{
			target.GetComponent<Renderer>().material.SetTextureScale("_Diffuse",new Vector2(scale,scale));
			target.GetComponent<Renderer> ().material.SetTexture ("_Normal", normalMap);
			target.GetComponent<Renderer> ().material.SetTexture ("_Diffuse", texture);
			target.GetComponent<Renderer> ().material.SetTexture ("_HueMask", hueMask);
			target.GetComponent<Renderer> ().material.SetTexture ("_SpecMask", specularMask);

			Dictionary<string,float> preset;
			if(target.name == "plage" || target.name == "Plage")
			{
				preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigPlage(texture.name);
			}
			else if(target.name == "margelle" || target.name == "Margelle")
			{
				preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMargelle(texture.name);
			}
			else if(target.name == "muret" || target.name == "Muret")
			{
				preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMuret(texture.name);
			}
			else
			{
				preset = null;
			}
			if(preset != null)
			{
				string[] keys = preset.Keys.ToArray();
				float red = 0;
				float green = 0;
				float blue = 0;
				foreach (string key in keys)
				{
					switch(key)
					{
						case "Normallevel ":
							target.GetComponent<Renderer> ().material.SetFloat("_Normallevel",preset[key]);
							break ;
						case "UVtranslation ":
							target.GetComponent<Renderer> ().material.SetFloat("_UVtranslation",preset[key]);
							break ;
						case "UVRotation ":
							target.GetComponent<AABBOutlineResizer>().setOffsetRotationUV(preset[key]);
							//target.GetComponent<Renderer> ().material.SetFloat("_UVRotation",preset[key]);
							break ;
						case "UVTile ":
							target.GetComponent<AABBOutlineResizer>().setOffsetTileUV(preset[key]);
							//target.GetComponent<Renderer> ().material.SetFloat("_UVTile",preset[key]);
							break ;
						case "Blur ":
								target.GetComponent<Renderer> ().material.SetFloat("_Blur",preset[key]);
							break ;
						case "gloss ":
								target.GetComponent<Renderer> ().material.SetFloat("_gloss",preset[key]);
							break ;
						case "Specularlevel ":
							target.GetComponent<Renderer> ().material.SetFloat("_Specularlevel",preset[key]);
							break ;
						case "Lightness ":
							target.GetComponent<Renderer> ().material.SetFloat("_Ligntness",preset[key]);
							break ;
						case "HueMaskIntensity ":
							target.GetComponent<Renderer> ().material.SetFloat("_HueMaskIntensity",preset[key]);
							break ;
						case "HuecolorR ":
							red = preset[key];
							break ;
						case "HuecolorG ":
							green = preset[key];
							break ;
						case "HuecolorB ":
							blue = preset[key];
							break ;
						case "Huelevel ":
							target.GetComponent<AABBOutlineResizer>().setHueLevel(preset[key]);;
							break ;
						case "Saturation ":
							target.GetComponent<AABBOutlineResizer>().setSaturation(preset[key]);
							break ;
						case "Bulb ":
							target.GetComponent<Renderer> ().material.SetFloat("_Bulb",preset[key]);
							break ;
						case "Reflexion ":
							target.GetComponent<Renderer> ().material.SetFloat("_Reflexion",preset[key]);
							
							if(preset[key] !=0)
							{
								Cubemap temp = Resources.Load("shaders/configs/Chrome 1") as Cubemap;
								Debug.Log(temp.name  +  "    Reflexion");
								target.GetComponent<Renderer> ().material.SetTexture("_CubeMap",temp);
							}
							break ;
						case "Reflexionintensity ":
							target.GetComponent<Renderer> ().material.SetFloat("_Reflexionintensity",preset[key]);
							break ;
						case "ReflexionBlur ":
							target.GetComponent<Renderer> ().material.SetFloat("_ReflexionBlur",preset[key]);
							break ;
					}					
				}
				Vector3 hue = new Vector3(red,green,blue);
				Color hueColor = new Color(red,green,blue,1.0f);
				if(target.name == "plage" || target.name == "Plage")
				{
					target.GetComponent<AABBOutlineResizer>().setColorPlage(hue);
				}
				else if(target.name == "margelle" || target.name == "Margelle")
				{
					target.GetComponent<AABBOutlineResizer>().setColorMargelle(hue);
				}
				else if(target.name == "muret" || target.name == "Muret")
				{
					target.GetComponent<AABBOutlineResizer>().setColorMuret(hue);
				}

				//target.GetComponent<Renderer> ().material.SetColor("_Huecolor",hue);
				picker.GetComponent<HSVPicker>().currentColor = hueColor;
			}
		}
		else
		{
			target.GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(scale,scale));
			target.GetComponent<Renderer> ().material.SetTexture ("_BumpMap", normalMap);
		}
	
		_assetBundleIsOpen=false;
	}
	
	private IEnumerator ApplyTexture2 (Transform target, OSLibTexture textureData, OSLib library)
	{
		while(_assetBundleIsOpen)
			yield return new WaitForEndOfFrame();
		_assetBundleIsOpen = true;
		WWW www = WWW.LoadFromCacheOrDownload (library.GetAssetBundlePath (), library.GetVersion ());
		yield return www;
		
		AssetBundle assets = www.assetBundle;
		Texture2D texture = assets.LoadAsset (textureData.GetFilePath2 (), 
										 typeof (Texture2D)) as Texture2D;
		assets.Unload (false);
		target.GetComponent<Renderer>().material.mainTexture = texture;
		_assetBundleIsOpen=false;
	}
	
	private void specFcnControl()
	{
		if(selT > 0 && specFunctions != null && specFunctions.getSelectedItem() != null)
		{
			((Function_OS3D)components[selT-1]).DoAction();
			specFunctions.getSelectedItem().m_text = ((Function_OS3D)components[selT-1]).GetFunctionParameterName();
			specFunctions.getSelectedItem().SetToogleActivated (true);
			specFunctions.getSelectedItem().resetSelected();
			specFunctions.resetSelected();
		}
		else if(selConfig>0 && Root.getSelectedItem().m_text != TextManager.GetText("GUIMenuConfiguration.SpecialFunction"))
		{
			((Function_OS3D)components[selConfig-1]).DoAction();
			Root.getSelectedItem().m_text = ((Function_OS3D)components[selConfig-1]).GetFunctionParameterName();
			selConfig = -1;
			Root.getSelectedItem().resetSelected();
		}
	}
	
	private void hideOthers()
	{
		foreach(Component cp in othersMenu)
		{
			((GUIInterface)cp).setVisibility(false);
		}
	}
	
//FCN. AUX. PUBLIC--------------------
	
	public void updateGUI(GUIItemV2 itm,int val,bool reset)
	{
		/*if(activeItem == itm && val == -1)
			activeItem = null;
		else*/ if(reset)
			activeItem = itm;
		if(val == -1)
			activeItem = null;



		switch (itm.getDepth())
		{
			case 0:
				selConfig = val;
				if(reset)
				{
					selObj = -1;
					selT = -1;
					if(itm.getSelectedItem()!=null && itm.getSelectedItem().getSelectedItem()!=null)
						itm.getSelectedItem().resetSelected();
					if(itm.getSelectedItem()!=null)
						itm.resetSelected();
				}
				break;
				
			case 1:
				selObj = val;
				if(reset)
				{
					selT = -1;
					if(itm.getSelectedItem()!=null)
						itm.resetSelected();
				}
				break;
				
			case 2:
				selT = val;
				break;
		
		}		
		ui2fcn();
	}
	
	public void setVisibility(bool b)
	{
		showMenu = b;
//        m_justShowedMenu = true;
//		if(!b)
//		{
//			resetMenu();
//		}	
	}
	
	public void OpenMaterialTab()
	{
		Root.setSelected(0);
		updateGUI((GUIItemV2)Root.getSubItems()[0],0,true);
	}
	
	public void OpenFunctionTab()
	{
		if(Root.getSubItemsCount() > 0)
		{
			Root.setSelected(1);
			updateGUI((GUIItemV2)Root.getSubItems()[1],0,true);
		}
	}
	
	public bool isVisible()
	{
		return showMenu;
	}
	
	public void canDisplay(bool b)
	{
//		canDisp = b;	
	}
	
	public bool isOnUI()
	{
		if(activeItem != null && activeItem.GetType() == typeof(GUIUpperList))
		{
			GUIUpperList guiUL = activeItem as GUIUpperList;
			
			if(guiUL != null)
			{
				return PC.In.CursorOnUIs(m_menuGroup, guiUL.bgThumbRect /*new Rect(0,0,Screen.width,160)*/);
			}
		}
		
		return PC.In.CursorOnUIs(m_menuGroup, new Rect(0,0,Screen.width,160));
	}
	
	public void CreateGui()
	{
		//rien ici	
	}
	
	void OnEnable()
	{
		GUI.skin = skin;
		m_off7 = Screen.width;
		Root = new GUIItemV2(-1,-1,"Root","","",this);
		
		GUIItemV2 mapsConfig = new GUIItemV2(0,0,TextManager.GetText("GUIMenuConfiguration.Materials"),"MaterialOn","MaterialOff",this);
		specFunctions = new GUIItemV2(0,1,TextManager.GetText("GUIMenuConfiguration.SpecialFunction"),"FunctionOn","FunctionOff",this);
		
		//Textures et couleurs
		obj2config = Camera.main.GetComponent<ObjInteraction>().configuringObj(null);// <- récupération de l'objet a configurer

		ObjData objData = obj2config.GetComponent <ObjData> ();
		OSLibObject obj = objData.GetObjectModel (); // get from obj2congfig
		OSLibModules modules = obj.GetModules ();
		int x = 0;
	//Debug.Log("Nb De Mobules>"+modules.GetStandardModuleList ().Count);
		foreach (OSLibModule module in modules.GetStandardModuleList ())
		{			
			int thumbCount = module.GetTextureList ().Count + module.GetColorList ().Count;
			
//			string nameThumb = "";
			if (thumbCount > 0/* || true*/)
			{
				Texture2D [] thumbnails = new Texture2D [thumbCount];
				string [] nameThumbs = new string [thumbCount];
				int textIndex = 0;
				foreach (OSLibColor col in module.GetColorList ()) //Ajout des vignettes des couleurs
				{
					Texture2D thumbnail = col.GetThumbnail ();
					nameThumbs[textIndex] = col.GetText(PlayerPrefs.GetString("language"));
					
					if (thumbnail != null)
						thumbnails[textIndex++] = thumbnail;
					else 
					{
						Color colorThumb = col.GetColor();
						thumbnail = new Texture2D(128,128);
						for (int cy=0; cy<128;cy++)
							for(int cx=0; cx<128; cx++)
								thumbnail.SetPixel(cx,cy,colorThumb);
						thumbnail.Apply();
						thumbnails[textIndex++] = thumbnail;						
					//	thumbnails[textIndex++] = Resources.Load("thumbnails/noThumbs",typeof(Texture2D)) as Texture2D;
					}
				}
				
				foreach (OSLibTexture tex in module.GetTextureList () ) //Ajout des vignettes des textures
				{
					Texture2D thumbnail = tex.GetThumbnail ();
					nameThumbs[textIndex] = tex.GetText(PlayerPrefs.GetString("language"));
					
					if (thumbnail != null)
						thumbnails[textIndex++] = thumbnail;
					else
						thumbnails[textIndex++] = Resources.Load("thumbnails/noThumbs",typeof(Texture2D)) as Texture2D;
				}
				
				
				/*GUIGridV2*/GUIUpperList thumbGrid = new /*GUIGridV2*/GUIUpperList (1,x, TextManager.GetText(module.GetModuleType ()), "outilOn", "outilOff", this);
				x++;
				thumbGrid.setImgContent (thumbnails, nameThumbs, null, null);
				mapsConfig.addSubItem (thumbGrid);
			}
		}
		
		//Ajout au rootNode
		if(modules.GetStandardModuleList().Count > 0)
			Root.addSubItem(mapsConfig);
		
		//Func. speciales

		components =  obj2config.GetComponent<ObjData>().getSpecFcns();
		bool bspecFunction = false;

		for(int i=0; i<components.Length;i++) 
		{
	    	Object comp = components[i];

	    	if (comp.GetType().ToString().StartsWith("Function_"))
	    	{

				Function_OS3D funct = (Function_OS3D)comp;
				/*if(funct.GetFunctionName() == "Reglage plage"){
					GUIItemV2 guiItem = new GUIItemV2(2,i+1,TextManager.GetText(funct.GetFunctionParameterName()),"FunctionOn","FunctionOff",this);
					funct.setGUIItem(guiItem);
					Root.addSubItem(guiItem);
				}else{*/
					GUIItemV2 guiItem = new GUIItemV2(2,i+1,TextManager.GetText(funct.GetFunctionParameterName()),"outilOn","outilOff",this);
					funct.setGUIItem(guiItem);

					specFunctions.addSubItem(guiItem);
					bspecFunction = true;
				//}
	     	}
		    if (comp.GetType().ToString().StartsWith("FunctionAlt1_"))
		    {   
				Function_OS3D funct = (Function_OS3D)comp;
				GUIItemV2 guiItem = new GUIItemV2(2,i+1,TextManager.GetText(funct.GetFunctionParameterName()),"outilOn","outilOff",this);
				funct.setGUIItem(guiItem);
				specFunctions.addSubItem(guiItem);
				bspecFunction = true;
    	 	}
			if (comp.GetType().ToString().StartsWith("FunctionConf_"))
	    	{
				Function_OS3D funct = (Function_OS3D)comp;
				GUIItemV2 guiItem = new GUIItemV2(0,i+1,TextManager.GetText(funct.GetFunctionParameterName()),"menuOn","menuOff",this,true);
				funct.setGUIItem(guiItem);
				
				//specFunctions.addSubItem(guiItem);
				//bspecFunction = true;
				
				Root.addSubItem(guiItem);
	     	}
	   	}
		
		//Ajout au rootNode
	   	if(bspecFunction)
	   	{
			Root.addSubItem(specFunctions);
		}
		
		showMenu = true;
		
	}
	
	void OnDisable()
	{
		m_off7 = Screen.width;
		showMenu = false;
		if(Camera.main)
			Camera.main.GetComponent<ObjInteraction>().configuringObj(obj2config);//<- on remet obj2config dans selectedObj
		obj2config = null;
		activeItem = null;
		Resources.UnloadUnusedAssets();
	}
	
//	void OnApplicationQuit()
//	{
//		Camera.mainCamera.GetComponent<ObjInteraction>().configuringObj(obj2config);
//	}	
		
	public GameObject GetConfiguredObj()
	{
		return obj2config;
	}
}