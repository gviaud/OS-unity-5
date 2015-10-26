using UnityEngine;
using System.Collections;

using Pointcube.Global;

public class ObjInstanciation : MonoBehaviour
{
    public Mode2D     m_mode2D;

    public string m_ctxPanelID; // ctx1_objAdd
    Transform objToIns;

    GameObject objectNode;
    //GameObject originalsNode;
    GameObject safeDropObj;
    GameObject mainScene;

    GameObject m_obj2rotate;
	
	OSLibObject m_object;
	
	int count = 0;
	int countChild = 0;
	int childIndex = -1;
	int upPosition = Screen.height - 115;
	int downPosition = Screen.height;
	
	public int iobjectNumber = 0;
	
//	public float colA = 1;
	
//	public Texture2D i_cat_n;
//	public Texture2D i_cat_o;
//	public Texture2D i_bg_objets;
	
	public GUISkin skin;
		
	//SafeDrop
	private bool safeDrop = false;
	bool tempLock = true;
	
	RaycastHit safeDropRCH = new RaycastHit();
	
	ArrayList guiStyles = new ArrayList ();
	ArrayList guiStylesChild = new ArrayList ();
	
	string categories = "_parents";
	
	public Color i_textColor;
	Color col = new Color (1, 0, 0, 0.25f);
	
	Vector2 rgtScrlVw = Vector2.zero;
	
	float steps = 2;
	
	Transform sdObj;
	Vector2 pos = new Vector2(0,0);
	Vector2 oldMousePos = new Vector2(-1,-1);
	bool unlockStarted = false;
	Vector3 goodPos;
	
	protected bool isCreatingSDObj = false;
	
	
	int libIndex = 0;
	string[] libs;
	//GUIDualTxtBtn[] libsBtns;
	
	
	private GameObject _iosShadows;
	
	//bool safeDropRotate = false;
	//float safeDropAngle = 0;

	private const string DTAG = "ObjInstanciation : ";
	
	private int selFab = 0;
	private int selTyp = 0;
	private int selObj = 0;
	
	
	//----------------------------------------------------
	void Start ()
	{
        if(m_mode2D == null) Debug.LogError(DTAG+" mode2D "+PC.MISSING_REF);

		mainScene = GameObject.Find("MainScene");
		objectNode = GameObject.Find ("MainNode");
		_iosShadows = GameObject.Find("iosShadows");
		//originalsNode = GameObject.Find ("Originals");
		//safeDropObj = GameObject.Find("safeDrop");
		//guiStyleGeneration();
		count = 0;//originalsNode.transform.GetChildCount ();
		//libs = new string[count];
		//libsBtns = new GUIDualTxtBtn[count];
//		GUIStyle s = skin.FindStyle("blu btn");
//		for(int i = 0;i<count;i++)
//		{
//			
//			GUIDualTxtBtn btn = new GUIDualTxtBtn(new Rect(0,i*40,180,40),12,150,
//			                                      originalsNode.transform.GetChild(i).gameObject.name,
//			                                      originalsNode.transform.GetChild(i).transform.GetChildCount().ToString(),s);
//			libsBtns[i] = btn;
//			libs[i] = originalsNode.transform.GetChild(i).gameObject.name;
//		}
	}

	//Update is called once per frame
	void Update ()
	{
		if(safeDrop)
		{
			safeDrop = false;
			if(m_object != null)
			{
				StartCoroutine(cloneObject(m_object,new Vector3(0,0,0)));
			}
		}
		
		if(m_obj2rotate != null)
		{
			float angle = m_obj2rotate.transform.localRotation.eulerAngles.y;
			if(angle < 175)
			{
				angle = Mathf.Lerp(angle,180,5*Time.deltaTime);
				Quaternion q = new Quaternion(0,0,0,0);
				q.eulerAngles = new Vector3(0,angle,0);
				m_obj2rotate.transform.rotation = q;
			}
			else
			{
				Quaternion q = new Quaternion(0,0,0,0);
				q.eulerAngles = new Vector3(0,180,0);
				m_obj2rotate.transform.rotation = q;
				m_obj2rotate = null;
			}
		}
	}

    // 2013-10 : l'instanciation d'objet passe par ici (pas swap ni copy)
	public IEnumerator cloneObject(OSLibObject oslObj, Vector3 position)
	{
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		yield return new WaitForEndOfFrame();
		Transform newObj;
		position.y = 100;
		OSLib objLib = oslObj.GetLibrary ();
		//Debug.Log("avant www");
		
		Montage.assetBundleLock = true;
		WWW www = WWW.LoadFromCacheOrDownload (objLib.GetAssetBundlePath (), objLib.GetVersion ());
		yield return www;
		
		AssetBundle assetBundle = www.assetBundle;
		//Debug.Log("Apres www");
		
		Object original = assetBundle.LoadAsset (oslObj.GetModel ().GetPath (), typeof(GameObject));

		Vector3 wordPos = new Vector3();
		Ray ray=Camera.main.ScreenPointToRay(new Vector3(Screen.width/2,Screen.height-180.0f,1.0f));
		RaycastHit hit;
		if(Physics.Raycast(ray,out hit,1000f))
			wordPos = hit.point;

		Vector3 temp = Camera.main.ScreenToWorldPoint (new Vector3(Screen.width / 2, Screen.height - 100.0f, 200.0f));
		//Debug.DrawLine (ray.origin, hit.point, Color.cyan,2000.0f);
		float temp_hauteur = SceneControl.m_hauteur;
		wordPos = Camera.main.transform.position + Camera.main.transform.forward * 100.0f*(0.0000005f*temp_hauteur+1.0f);
		position += Camera.main.transform.forward * (1f + SceneControl.m_hauteur * 0.2f)*10f;
		GameObject go = (GameObject) Instantiate (original, position, new Quaternion (0,0,0,0));

		newObj = go.transform;
		
		newObj.name = newObj.name + ++iobjectNumber;
		
		newObj.parent = objectNode.transform;
		newObj.gameObject.layer = 9;
		
		newObj.gameObject.AddComponent <ObjBehav>();
		newObj.gameObject.AddComponent<ApplyShader>();
		
		ObjData data = (ObjData) newObj.gameObject.AddComponent <ObjData>();
		yield return new WaitForEndOfFrame(); // attend la fin du start de ObjBehav
		data.SetObjectModel (oslObj, assetBundle);
		
		data.selFab = selFab;
		data.selTyp = selTyp;
		data.selObj = selObj;
		
		assetBundle.Unload (false);
		Montage.assetBundleLock = false;
		//yield return new WaitForEndOfFrame(); // attend la fin du start de ObjBehav
		
//      newObj.name = prefix+"_"+o.name;
		float y = newObj.gameObject.GetComponent<ObjBehav>().init(); // > met a la bonne position en y
		newObj.transform.position = new Vector3(newObj.transform.position.x,
		                                        y,
		                                        newObj.transform.position.z);

        // -- Si c'est un gros objet, le déplacer un peu vers le fond --
        float s = Montage.sm.getCamData().m_s;
        if(newObj.GetComponent<Collider>().bounds.size.z > 1.4f*s)
        {
            Matrix4x4 m = Camera.main.cameraToWorldMatrix;
            float distance = (newObj.GetComponent<Collider>().bounds.size.z < 4*s ? 4*s : newObj.GetComponent<Collider>().bounds.size.z)*-1.25f;
            Vector3 p = m.MultiplyPoint(new Vector3(0f, 0f, distance));
            Vector3 pos = newObj.transform.position;
            pos.Set(p.x, pos.y, p.z);
            newObj.transform.position = pos;
        }
		if(oslObj.GetModules().FindModule("sol")!=null)
		{		
			position = Camera.main.transform.position + Camera.main.transform.forward * 5.0f * Mathf.Max(10,newObj.transform.localScale.x);
			position.y = y;
			newObj.transform.position = position;	
			
		}
		 
//		newObj.transform.Rotate (new Vector3 (0, 180, 0));
		
        // TODO Ajouter un "si iOS"
		string pref = newObj.GetComponent<ObjData> ().GetObjectModel ().GetObjectType ();
		//string pref = originalsNode.transform.GetChild(newObj.GetComponent<ObjData>().getTyp()).name;
       /* if(pref != "piscines" && pref != "spas")        // Pas d'ombres pour les piscines (très moches)
        {
            if(_iosShadows)
			{
				_iosShadows.GetComponent<IosShadowManager>().AddShadow(newObj.gameObject);
				
			}
            else
				Debug.LogWarning("Objet \"iosShadows\" introuvable. Les ombres pour iOS risquent de ne pas fonctionner.");
			
        }
		else
		{
			//newObj.gameObject.AddComponent ("PoolInitializer");
		}*/
        // -- Ajout de l'objet au mode2D si besoin --

        OSLibObject obj = newObj.transform.GetComponent<ObjData>().GetObjectModel();
		if (obj.GetObjectType () == "pool" || obj.GetObjectType () == "dynamicShelter" || obj.IsMode2D ()) 
		{

			m_mode2D.AddObj (newObj.transform);

		}
		if (newObj.GetComponent<ObjData> ().GetObjectModel ().GetModules().FindModule("duck")!=null)//TODO Temporaire a enlever plus tard
		{
//					
//			// This is the parent node
//			Transform parent = (Transform) newObj.transform.Find("water");
//			// This is the prefab
//			GameObject prefab = (GameObject)Resources.Load("animDuck"); 
//			// Add the instance in the hierarchy
//			GameObject obj = (GameObject)Instantiate(prefab);
//
//			// Find the instantiate prefab and asign the parent
//			
//			obj.transform.localScale = go.transform.localScale;//new Vector3 (8.0f,8.0f,8.0f);//TODO < remplacer 8.0f par le scale de la scene
//			obj.transform.parent = parent;
//			obj.transform.position = new Vector3(0,-0.1f,0);
			newObj.gameObject.AddComponent<Function_duck>();
			data.updateSpecFcns();
			
			if(go.GetComponent<MeshCollider>())// <- ca aussi ca doit etre mis dans les libs
			{
				go.GetComponent<MeshCollider>().convex = true;
			}
			
		}
//		if (newObj.GetComponent<ObjData> ().GetObjectModel ().GetModules().FindModule("liner")!=null)//TODO Temporaire a enlever plus tard
//		{
//			newObj.FindChild("frise").gameObject.AddComponent<PoolFrise>();
//		}
		
		yield return new WaitForEndOfFrame();
		mainScene.GetComponent<GUIMenuLeft>().updateSceneObj();
		Camera.main.GetComponent<ObjInteraction>().setSelected(go);
		yield return new WaitForEndOfFrame();
		m_obj2rotate = go;
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
        PC.ctxHlp.ShowCtxHelp(m_ctxPanelID);
		yield return true;
	}
	
	
	Vector3 doTheDrag(Vector3 currentPos) // OBSOLETE
	{
		float d = 1;	
		Vector2 ret = Vector2.zero;
		Vector2 currentMousePos = Input.mousePosition;
		Vector2 delta2 = currentMousePos-oldMousePos;
		Vector3 newPos = new Vector3(currentPos.x, currentPos.y, currentPos.z);

		if(delta2 != Vector2.zero)
		{
			Vector3 cdmInW = currentPos;
			cdmInW.y = 0;
			Vector3 cdmOnScreen3 = Camera.main.WorldToScreenPoint(cdmInW);
			Vector2 cdmOnScreen2 = new Vector2(cdmOnScreen3.x,cdmOnScreen3.y);
			cdmOnScreen2 = cdmOnScreen2+delta2;
			
			RaycastHit newhitm = new RaycastHit ();
			Physics.Raycast (Camera.main.ScreenPointToRay(cdmOnScreen2), out newhitm, Mathf.Infinity,9);
						
			if(newhitm.rigidbody)
			{
				ret.x = newhitm.point.x;
				ret.y = newhitm.point.z;
				newPos = newhitm.point;
				newPos.y = currentPos.y;
			}
		}
		oldMousePos = currentMousePos;
		return newPos;
	}
	
//	IEnumerator Unlock()
//	{
//		unlockStarted = true;
//		//yield return new WaitForSeconds(0.5f);
//		safeDropRotate = true;
//		yield return(!safeDropRotate);
//		if(Application.platform == RuntimePlatform.IPhonePlayer)
//		{
//			while (Input.touchCount <1) //tactile : wait for touch
//			{
//	      		yield return null;
//	    	}
//		}
//		//TODO pour le tactile mettre apres le waitforseconds, un yield return Touches.count >0 (détecté un touch)
//		tempLock = false;
//		unlockStarted = false;
//		yield return true;
//	}
	
//	//Fonction de Fit de l'echelle au premier objet ajouté
//	public void scaleFit(Vector3 bboxSize) 
//	{
//		float[] values = {bboxSize.x,bboxSize.y,bboxSize.z};
//		float max = Mathf.Max(values);
//		GameObject.Find("camPivot").GetComponent<SceneControl>().setScaleFactor(-max+13);
//		          
//	}
	
	public void showLibList(Rect r,GUIStyle s)
	{
////		libIndex = GUI.SelectionGrid (r,libIndex,libs ,1,s);
//		GUI.skin = skin;
//		GUI.BeginGroup(r);
//		GUI.Label(new Rect(0,0,r.width,30),"Catalogue","header");
////		libIndex = GUI.SelectionGrid (new Rect(0,30,r.width,r.height-(2*30)),libIndex,libs ,1,s);
//		rgtScrlVw = GUI.BeginScrollView(new Rect(0,30,r.width,r.height-(2*30)),rgtScrlVw,new Rect(0,0,r.width,libsBtns.Length*40));
//		for(int i = 0;i<libsBtns.Length;i++)
//		{
//			if(libsBtns[i].getGui())
//				libIndex = i;
//		}
//		GUI.EndScrollView();
//		GUI.EndGroup();
	}
	
	public void showObjLib(Rect rt)
	{
//		GUI.skin = skin;
//		if(libIndex != -1)
//		{
////			mainScene.GetComponent<GUIMenu>().btmPanelHeader = libs[libIndex];
//			float btnSize = rt.height-40;
//			GUI.BeginGroup(rt);
//			GameObject g = originalsNode.transform.GetChild(libIndex).gameObject;
//			GUISkin vignetteSkin = Camera.mainCamera.GetComponent<ObjInteraction>().guiSkin;//TODO a changer quand le loader sera en place
//			int c = g.transform.GetChildCount();
//			
//			pos = GUI.BeginScrollView (new Rect(40,0,rt.width-(2*40),rt.height-20),pos,new Rect (0, 0, c*(btnSize), rt.height-50));
//			for (int i = 0; i < c ; i++) 
//			{
//				if (GUI.Button (new Rect((btnSize* i), 0, btnSize,btnSize), "", vignetteSkin.FindStyle(g.transform.GetChild(i).name)))
//				{
//					//obj = (Object)g.transform.GetChild(i);
//					safeDrop = true;
////					mainScene.GetComponent<GUIMenuMain>().lpVisible = false;
//				}
//			}
//			GUI.EndScrollView ();
//			
//			if(GUI.Button(new Rect(0,0,100,120),"","btn lib left"))
//			{
//				pos.x = pos.x - (btnSize/2);
//			}
//			if(GUI.Button(new Rect(rt.width-100,0,100,120),"","btn lib right"))
//			{
//				pos.x = pos.x + (btnSize/2);
//			}
//			GUI.EndGroup();
//		}	
	}
	
	void OnGUI ()
	{
	}
	
	public void addObj(OSLibObject o, int _selFab, int _selTyp, int _selObj)
	{
		m_object = o;		
		safeDrop = true;
		selFab = _selFab;
		selTyp = _selTyp;
		selObj = _selObj;
//		mainScene.GetComponent<GUIMenuMain>().lpVisible = false;
	}
	
	public void swap(Vector3 p, Object o)//OBSOLETE
	{
//		Transform newObj;
//		newObj = Instantiate (o, p, new Quaternion ()) as Transform;
//		newObj.parent = objectNode.transform;
//		newObj.gameObject.AddComponent ("ObjBehav");
//		newObj.gameObject.layer = 9;
//
//		//newObj.gameObject.AddComponent ("ApplyShader");
//		//yield return new WaitForEndOfFrame(); // attend la fin du start de ObjBehav
//
//		newObj.gameObject.GetComponent<ObjBehav> ().init ();
//		newObj.transform.Rotate (new Vector3 (0, 180, 0));
//		Camera.mainCamera.GetComponent<ObjInteraction>().swapObj(newObj.gameObject); // cest ici quon fait le delete
//		
//		
////		mainScene.GetComponent<GUIMenuLeft>().updateSceneObj();
	}

    // 2013-10 : le swap d'objet passe par ici
	public IEnumerator swap(Vector3 p, OSLibObject newO, GameObject oldO, int _selFab, int _selTyp, int _selObj)
	{
		mainScene.GetComponent<GUIMenuLeft>().canSwap = false;
		selFab = _selFab;
		selTyp = _selTyp;
		selObj = _selObj;
		
		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		yield return new WaitForEndOfFrame();
		
		Quaternion tmpRotate = new Quaternion(0,0,0,0);
		tmpRotate.eulerAngles = oldO.transform.localRotation.eulerAngles + new Vector3(0,180,0);

		Transform newObj;
		//newObj = Instantiate (objToIns, p, tmpRotate) as Transform;
		OSLib objLib = newO.GetLibrary ();
		while (Montage.assetBundleLock)
		{
			yield return new WaitForEndOfFrame ();	
		}
		Montage.assetBundleLock = true;
		WWW www = WWW.LoadFromCacheOrDownload (objLib.GetAssetBundlePath (), objLib.GetVersion ());
		yield return www;
		
		yield return new WaitForEndOfFrame(); // attend la fin du start
		
		AssetBundle assetBundle = www.assetBundle;
	
		Object original = assetBundle.LoadAsset (newO.GetModel ().GetPath (), typeof(GameObject));
			
		GameObject go = Instantiate (original/*, p, tmpRotate*/) as GameObject;
		
		yield return new WaitForEndOfFrame(); // attend la fin du start
		//TODO: to remove

		
		newObj = go.transform;
		newObj.name = newObj.name + ++iobjectNumber;
		newObj.position = p;
		newObj.rotation = tmpRotate;
		newObj.parent = objectNode.transform;
		if(!newO.IsBrandObject())
			newObj.localScale = oldO.transform.localScale;
		
		
		newObj.gameObject.AddComponent <ObjBehav>();
		newObj.GetComponent<ObjBehav>().iAmLocked(oldO.GetComponent<ObjBehav>().amILocked());
		
		newObj.gameObject.layer = 9;
		float y = newObj.gameObject.GetComponent<ObjBehav> ().init ();
		newObj.Rotate (new Vector3 (0, 180, 0));
		
		newObj.gameObject.AddComponent <ApplyShader>();
		ObjData data = (ObjData) newObj.gameObject.AddComponent <ObjData>();
		yield return new WaitForEndOfFrame(); // attend la fin du start de ObjBehav
		
		data.selFab = selFab;
		data.selTyp = selTyp;
		data.selObj = selObj;
		
		data.SetObjectModel (newO, assetBundle);		
		assetBundle.Unload (false);
		Montage.assetBundleLock = false;
		yield return new WaitForEndOfFrame(); // attend la fin du start de ObjBehav
		yield return StartCoroutine(data.loadConfIE(oldO.GetComponent<ObjData>().getConfiguration()));
		
//		float y = newObj.gameObject.GetComponent<ObjBehav>().init();
		newObj.position = new Vector3(newObj.position.x,
		                              newObj.position.y+y,
		                              newObj.position.z);
		
		newObj.GetComponent<ObjBehav>().SetHeighOff7(p.y);

        // -- Suppression de l'ancien objet si besoin --
        OSLibObject oldObj = oldO.transform.GetComponent<ObjData>().GetObjectModel();
        if(oldObj.GetObjectType() == "pool" || oldObj.IsMode2D())
            m_mode2D.RemoveObj(oldO.transform);

        // -- Ajout du nouvel objet au mode2D si besoin --
        OSLibObject obj = newObj.transform.GetComponent<ObjData>().GetObjectModel();
		if(obj.GetObjectType() == "pool" || obj.GetObjectType() == "dynamicShelter" ||  obj.IsMode2D())
            m_mode2D.AddObj(newObj.transform);

//		OSLibObject oldOModel = oldO.GetComponent<ObjData> ().GetObjectModel ();
//		foreach (OSLibModule mod in oldOModel.GetModules ().GetStandardModuleList ())
//		{
//			Transform modTargetOld = oldO.transform.Find (mod.GetModuleType ());
//			Transform modTargetNew = newObj.transform.Find (mod.GetModuleType ());
//			
//			if (modTargetNew != null && modTargetOld != null)
//			{
//				modTargetNew.renderer.material.color = modTargetOld.renderer.material.color;
//				modTargetNew.renderer.material.mainTexture = modTargetOld.renderer.material.mainTexture;
//			}
//		}
		
		//Chargement de la configuration

		foreach(MonoBehaviour func in newObj.GetComponents<MonoBehaviour>())
		{
//			Debug.Log("Passe la "+func.GetType());
			if(func.GetType().GetInterface("Function_OS3D") != null)
			{
			/*	if((Function_OS3D)oldO.GetComponent(func.GetType().ToString()) != null)
				{
					ArrayList conf = ((Function_OS3D)oldO.GetComponent(func.GetType().ToString())).getConfig();
					((Function_OS3D)func).setConfig(conf);
				}
				*/
				foreach (MonoBehaviour function in oldO.GetComponents<MonoBehaviour>())
				{
				//	MonoBehaviour function = (MonoBehaviour)function2;
					if(function.GetType().ToString().CompareTo(func.GetType().ToString())==0) 
					{
						if(
						((Function_OS3D)function).GetFunctionId()== 
						((Function_OS3D)func).GetFunctionId() 
						)		
						{
							ArrayList conf = ((Function_OS3D)function).getConfig();
							((Function_OS3D)func).setConfig(conf);
						}
					}
				}
			}
		}
		
	
		
		//------------------------------
		
		Camera.main.GetComponent<ObjInteraction>().swapObj(newObj.gameObject,oldO); // cest ici quon fait le delete
		
		if (newObj.GetComponent<ObjData> ().GetObjectModel ().GetModules().FindModule("duck")!=null) //TODO Deplacer ca ailleur
		{		
//			// This is the parent node
//			Transform parent = (Transform) newObj.transform.Find("water");
//			// This is the prefab
//			GameObject prefab = (GameObject)Resources.Load("animDuck"); 
//			// Add the instance in the hierarchy
//			GameObject obj = (GameObject)Instantiate(prefab);
//
//			// Find the instantiate prefab and asign the parent
//			obj.transform.localScale = newObj.transform.localScale;//new Vector3 (8.0f,8.0f,8.0f);//TODO < remplacer 8.0f par le scale de la scene
//			obj.transform.parent = parent;
//			obj.transform.position = new Vector3(0,-0.1f,0);
			newObj.gameObject.AddComponent<Function_duck>();
			data.updateSpecFcns();
			if(go.GetComponent<MeshCollider>())// <- ca aussi ca doit etre mis dans les libs
			{
				go.GetComponent<MeshCollider>().convex = true;
			}
		}
		
//		mainScene.GetComponent<GUIMenuLeft>().updateSceneObj();

		mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
		yield return new WaitForEndOfFrame();
		mainScene.GetComponent<GUIMenuLeft>().canSwap = true;
		yield return new WaitForEndOfFrame();
	}

}
