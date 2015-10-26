	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;

	using Pointcube.InputEvents;
	using Pointcube.Utils;
	using Pointcube.Global;

	public class ObjInteraction : MonoBehaviour
	{
		/*private int _lblTouchCount = 0;
		private ArrayList _touchPos = new ArrayList();
		private string _lblStateTotation = "";*/

	    public Mode2D m_mode2D;

		GameObject selectedObj = null;
		OSLibObject m_selectedOSLibObj;
		//hash code du selectedObj
		int _oldSelectedID = -1;
		GameObject mainScene;
		GameObject mainNode;
		GameObject camPivot;
		
		Camera mainCamera;

		public GUISkin guiSkin;
		public Texture2D i_backObjMenu;
	    public string  m_ctxPanelID; // ctx1_objAdd
		
		public bool actived = false;
		bool UIActive = false;
		bool objControled = false;
		bool rotationSnap = true;
		bool rotationLock = true;
		bool dragStarted = false;
		
		//UI générale
		Rect UIbackground;
		
		//rotation ui
		Vector2 center;
		Vector2 baseVector = new Vector2 (1, 0);
		
		bool rotate = false;
		bool dragging = false;

		
		Rect rotateBG = new Rect (0, 0, 0, 0);
		
		float UIBtnW = 90;
		float UIBtnH = 90;
		float UIRotate = 200;//150
		
		float m_ffactorScale = 5.0f;

		
		//autres
		animatedGUI rotationbtn;

		private bool updateDrag = false;
		private bool _isTimer = false;
		private bool longClickStarted = false;
		
		private bool _highLightOverride = false;
		
		//swap
		bool isSwaping = false;
		
		//Tactile Rotation
		Vector2 oldDir;
			
		bool firstTouch = true;
		
		float angle = 0;
		
		//drag's
		int raycastLayer = -1;
		Vector3 oldHitm = new Vector3(0,100000,0);
		Vector3 delta;

		Vector2 oldMousePos;
		
		//drag4
		Vector3 old3DPos;
		bool canDeselect = false;
		
		//stackSelection
		float deadzoneLimits = 200;
		Vector2 deadzonePos;
		int stackIndex = 0;
		bool stackReady = false;
		ArrayList stack = new ArrayList();
		bool selectionLock = false;
		float delay = 0.2f;
		float timer = 0;
		
		//TactileInteraction V2
		bool v2CanDeselect = false;
		bool v2CanMove = false;
		bool v2CanSelectNext = false;
		bool v2CanSelect = false;
		bool _clickOnUI = false;
		
		GameObject v2TempObject;
		
		private bool        m_selectAll = false; // Pour déplacement de tous les objets
	    private bool        m_rotateAllInit;
		
		//Selection multiple
		private bool m_selectMultiple = false;
		private bool m_canDeselectMulti = false;
		
		private GameObject m_toDeselectUnique;
		
		private ArrayList m_ObjsSelection = new ArrayList();
		
		private Vector3 m_msCenter;
		
		private float T;
		private float deltaT = 0.25f;
		
		//mouse interaction
		protected Vector2 cursorBegin = new Vector2();
		protected bool initRotate = false;
		protected Vector2 cursorBeginRotate = new Vector2();
		protected float valueBeginRotate = 0.0f;
		protected int countImobile = 0;
		
		protected int frameCounter = 0;
		protected int frameToSkip=0;
		
		private GameObject _iosShadows;
		
		//Selection
		private GameObject _collidedObj;
		
		//Autolock
	//	bool showAutoLock = false;
	//	string autolockTxt = "";
		
		private bool        m_moveStart;         // Pour savoir quand afficher un objet sélectionné en vert ou non
		//distance a la camera
		GUIMenuMain hud;
		BgImgManager _imgMgr;
		
		private Transform _camPivot;
		
		//Table contenant les anciens matériaux pour le passage en vert lors de la selection
		private Hashtable _oldMaterial;
		
		//private MonoBehaviour _mode2D;
		
		//######################################################
		public Material highlight;//ADDED TO TEST
		private Color vert = new Color(0,1,0,0.5f);
		private Color orange = new Color(1,0.5f,0f,0.5f);
	    
	    private const string DTAG = "ObjInteraction : ";
	    
	    public float fechelle = 1.0f;
	    
		void Awake()
		{
	        if(m_mode2D == null) Debug.LogError(DTAG+" mode2D "+PC.MISSING_REF);

			UsefullEvents.ShowHelpPanel	+= Unselect;
			_camPivot = transform.parent;
		}
		
		void OnDestroy()
		{
			UsefullEvents.ShowHelpPanel	-= Unselect;
		}
		
		// Use this for initialization
		void Start ()
		{

			_oldMaterial = new Hashtable();
			mainNode = GameObject.Find("MainNode");
			mainScene = GameObject.Find("MainScene");
			camPivot = GameObject.Find("camPivot");

			rotationbtn = new animatedGUI (0.02f, animatedGUI.TypeOfGUI.repeatButton);
			rotationbtn.loadFrom ("AnimatedBtns/rotationBtn");
			mainCamera = Camera.main;
			rotateBG.width = UIRotate;
			rotateBG.height = UIRotate;
			UIbackground = new Rect (/*Screen.width - (1.1f*UIRotate)*/0, (Screen.height/2)- ((1.1f*UIRotate)/2), (1.1f*UIRotate), (1.1f*UIRotate));
			_iosShadows = GameObject.Find("iosShadows");

			hud = mainScene.GetComponent<GUIMenuMain>();
			_imgMgr = GameObject.Find("backgroundImage").GetComponent<BgImgManager>();

			m_rotateAllInit = false;
			m_moveStart = false;
			
		//	GameObject gameobjet =  GameObject.Find("mainCam");
		//	if(gameobjet!=null)
		//		_mode2D = (MonoBehaviour) gameobjet.transform.GetComponent("mode2D");
		}

		
		// Update is called once per frame
		void Update()//FixedUpdate ()
		{
			/* Mise à jour windows tactile maintenant dans WinTouchBehaviour.cs
			if(PC.In.GetType()==typeof(WinTouchInput)){
				((WinTouchInput)PC.In).Update();
			}*/

			if(frameToSkip>0){
				frameToSkip--;
			}
			else
			//if(_mode2D!=null)
		//		if(! ((mode2D)_mode2D).isMode2DActive())
		        if(!PC.ctxHlp.PanelBlockingInputs())
				{
					/*if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
					{
		                GenericInteraction();
		//				tactileInteraction();
					}	//amelioratedInteraction();//tactile, test isSwapping intégré
					else
					{*/
						if(!isSwaping)
						{
			                   GenericInteraction();
			            }
			//					mouseInteraction();
								//semiWorkingInteraction();//pc
					//}
					
					UpdateLine();
					UpdateSelectedMaterial();
				}
	//		if(selectedObj != null && !rotate && !dragStarted)
	//		{
	//			UIActive = true;
	//		}
	//		else
	//		{
	//			UIActive = false;
	//		}

		}

		public void DeselectByButton()
		{
			if(v2CanDeselect)
			{
				v2CanDeselect = false;
			}
			setSelected(null);		
		}

		public void UpdateSelectedMaterial()
		{
			//print ("TESTTT " + selectedObj);
			if(isSwaping || _highLightOverride || (selectedObj == null))
			{
				foreach( DictionaryEntry de in _oldMaterial )
		        {
					/*if (de.Key!=null)
						((Material) de.Key).color = (Color) de.Value;*/
				}
				_oldMaterial.Clear();
				return;
			}
			if((selectedObj!=null))
			{
				if(selectedObj.GetHashCode()!=_oldSelectedID)
				{
					foreach( DictionaryEntry de in _oldMaterial )
			        {
						/*if (de.Key!=null)
							((Material) de.Key).color = (Color) de.Value;*/
					}
					_oldMaterial.Clear();
					//return;
				}
			}
			{
				if(selectedObj == null)
					return;
	//			_oldMaterial.Clear();
				foreach(SkinnedMeshRenderer m in selectedObj.GetComponentsInChildren<SkinnedMeshRenderer>())
				{
					if((m.tag !="noRed")&&(m.tag !="noPos")  && (m.gameObject.layer!=14) && m.GetComponent<Renderer>().enabled)
					{
					/*	Mesh bm = new Mesh();
						m.BakeMesh(bm);
						Graphics.DrawMeshNow(bm,m.transform.position,m.transform.rotation);*/
						
						if((m.material!=null) && (m.material.color!=null))
						{
							if(!_oldMaterial.ContainsKey(m.material))
							{
								if(m.material.color != vert && m.material.color != orange)
									_oldMaterial.Add(m.material,new Color(m.material.color.r,m.material.color.g,m.material.color.b,m.material.color.a));
								m.material.color = highlight.color;
							}
							else
							{
								if(!m.material.color.Equals(highlight.color))
								{
									Debug.Log("COLOR premiere : "  + m.material.color);
									if(m.material.color != vert && m.material.color != orange)
										_oldMaterial[m.material] = new Color(m.material.color.r,m.material.color.g,m.material.color.b,m.material.color.a);
									m.material.color = highlight.color;
								}
							}
						}
					}
				}
				_oldSelectedID = selectedObj.GetHashCode();
			}		
		}
		public void UpdateLine()
		{
			if(selectedObj != null)
			{
				Vector2 v1 = new Vector2(selectedObj.transform.position.x,selectedObj.transform.position.z);
				Vector2 v2 = new Vector2(transform.position.x,transform.position.z);
				float dist = Vector2.Distance(v1,v2);
				float sclFactor = GameObject.Find("camPivot").GetComponent<SceneControl>().getScaleFactor();
				dist = dist/sclFactor;
				dist = (float)((int)(dist*100))/100;
				
				//Distance -1m #436
				//dist = dist-1;
				if(dist<0)
					dist = 0;
				
				string objName = selectedObj.name.Replace("(Clone)","");
				
				Vector2 uiEnd = mainCamera.WorldToScreenPoint(new Vector3(v1.x,0,v1.y));
				uiEnd.y = Screen.height - uiEnd.y;
				
				Vector3 tgt = _camPivot.forward * (transform.position.z+1)/2;
				Vector2 uiStart = mainCamera.WorldToScreenPoint(tgt);//new Vector3(/*v2.x/2f*/0,0,/*v2.y/2f*/ transform.position.z+1));
				//uiStart.y = Screen.height - uiStart.y;
				uiStart.y = Screen.height + 40.0f;
				
				//Clamp au bas de l'écran
				if(uiStart.y < Screen.height)
					uiStart.y = Screen.height;
				hud.UpdateLineStartEndPoint(uiStart,uiEnd,dist+" m");
			}
		}
	#region interaction générique (tactile + souris)
	    void GenericInteraction()
	    {
	        bool isOnUI = mainScene.GetComponent<GUIMenuMain>().isOnUi()
	                        || mainScene.GetComponent<GUIStart>().isActive()
	                        || Camera.main.GetComponent<GuiTextureClip>().IsOnUI();
	        bool isOnSaveBox = mainScene.GetComponent<Montage>().isOnUI();

	        bool isConfiguringScene = mainScene.GetComponent<GUIMenuRight>().isVisible();
	       /* if(selectedObj != null)
	            isConfiguringScene = isConfiguringScene && selectedObj.name != "_avatar";*/

	        // Debug.Log("activated = "+actived+", isOnui="+isOnUI+", isSwaping = "+isSwaping+", isConfiguratingScene="+isConfiguringScene+", isonsavebox="+isOnSaveBox
	        if(!actived || isOnUI || isSwaping/* || isConfiguringScene*/ || isOnSaveBox)
			{	
	            return;
			}
			///print ("coucou");

	        Vector2 cursor = PC.In.GetCursorPos();
			if(selectedObj == null && !m_selectAll && !m_selectMultiple)  // pas de selected object
			{

				Vector2 ldrag;
				float deltaScale;
	//            Debug.Log("..... pas de selectedObject");
	            if(PC.In.Click1Down())
	            {
	                v2TempObject = isOnObject(cursor);
	                v2CanSelect = true;
	                doTheBegan(cursor);
	                oldMousePos = cursor;
				
	            }
	            else if(PC.In.Click1Up() && !isConfiguringScene)              // END
	            {
					doTheEnded(cursor);
	            }
				else if(PC.In.Zoom_n_drag1(out deltaScale, out ldrag) && !mainScene.GetComponent<GUIMenuRight>().isVisible())                   // END
				{
					if(deltaScale != 0)
					{
						camPivot.GetComponent<SceneControl>().setS(deltaScale/m_ffactorScale);	
					}
	            }
	        }
	        else //if(selectedObj != null)                          // deja un selectedObject ou sélection multiple
	        {
	            Vector2 ldrag;
	            bool dragTrue = PC.In.Drag1(out ldrag);
	//            Debug.Log("..... deja un selectedObject ou selection multiple");
	            float deltaRota;
				float deltaScale;
				float deltaHeight;
				
				Vector2 deltaMoveBg = Vector2.zero;
				
				if(PC.In.Click1Down()) // BEGIN
	            {
					//DbgUtils.Log ("testtouch","CLiCK 1 DOWN ! object !=null");

					_clickOnUI = true;
	//                Debug.Log(".... Begin");
	                if(m_selectMultiple)
	                    doTheBeganMulti(cursor);
	                else
	                    doTheBegan(cursor);
	                oldMousePos = cursor;
	            }
	#if UNITY_IPHONE
	            else if (dragTrue)
	             {
	#else
	            else if (dragTrue )// &&(Mathf.Abs(ldrag.x)>2||Mathf.Abs(ldrag.y)>2) )
	            {
	                frameCounter++;
	            }
				
	            if(dragTrue && frameCounter>3)
				{
	#endif
	            // MOVE
	//                Debug.Log(".... Move");
	                if(Vector2.Distance(cursorBegin, cursor)<=Mathf.Epsilon)
	                {
	                    countImobile++;
	                    if(countImobile>200)
	                    {
	                        if(m_selectMultiple)
	                            doTheEndedMulti(cursor);
	                        else
	                            doTheEnded(cursor);
	                    }
	                }
	                else
	                {
	                    if(!m_selectMultiple)
	                        doTheMoved(cursor);
	                    else
	                        doTheMovedMulti(cursor);
	                }
	            }
	            else if(PC.In.Zoom_n_drag2V_n_rotate2D(out deltaScale,out deltaHeight,out deltaRota))//ROTATE / SCALE / HEIGHT
	            {
					float retFactor = usefullData.retinus ? 0.5f: 1f;
					_clickOnUI = true;
					
					int ichoiceScaleRotate = Mathf.Abs(deltaScale) > Mathf.Abs(deltaRota) ? 1 : 2; //1 == SCALE | 2 == ROTATE
					if(deltaScale != 0 && ichoiceScaleRotate == 1)
					{
						// In case all the objects in the scene are selected
						if (m_selectAll || usefullData._edition == usefullData.OSedition.Lite)
						{
							camPivot.GetComponent<SceneControl>().setS(deltaScale/m_ffactorScale);
						}
						// If there is a selection
						else if (m_selectedOSLibObj != null) 
						{
							// If there only one object selected and it is a brand object
							//if((!m_selectedOSLibObj.IsBrandObject() || m_selectedOSLibObj.IsAllowedToScale())&&
							if(!m_selectMultiple) 
							{
								if( !selectedObj.GetComponent<ObjBehav>().amILocked())
									DoTheScaleMonoObject(deltaScale/**0.75f*/*retFactor);
							}
						}
						
						
	                    NotifyMode2DobjectsChanged();
					}
					
					if(deltaHeight != 0 && m_selectedOSLibObj!=null)
					{
						if(m_selectedOSLibObj.IsBrandObject() || usefullData._edition == usefullData.OSedition.Lite)
						{
							if( !selectedObj.GetComponent<ObjBehav>().amILocked())
								camPivot.GetComponent<SceneControl>().setH(-deltaHeight/15.0f);		
						}
						else if(!m_selectedOSLibObj.IsBrandObject())
						{
							if(deltaHeight!= 0 && !selectedObj.GetComponent<ObjBehav>().amILocked())
							{
	#if UNITY_IPHONE
								deltaHeight *= 7.5f;
	#endif
								DoTheHeightChange((deltaHeight*retFactor)/15.0f);
							}
						}
					}
					//if(!zoomMoreThanRotate)
					//if((deltaRota<358.0f)||(deltaRota>2.0f))
					if(deltaRota != 0 && ichoiceScaleRotate == 2)
					{
						v2CanDeselect = false;
		//                Debug.Log("..... Rotate");
		                if(!m_selectAll && !m_selectMultiple) // Rotation d'un seul objet
		                {
		                    if(selectedObj != null && !selectedObj.GetComponent<ObjBehav>().amILocked() && selectedObj.name != "_avatar")
		                    {
		                        isMovingOrRotating(true);
		                        float angle = selectedObj.transform.eulerAngles.y - deltaRota*1.75f;//(deltaMove.x * 360f * 1.25f / Screen.width);
		                        selectedObj.transform.eulerAngles = new Vector3 (0, angle, 0);
		
		//                       if((selectedObj.name == "_avatar")&&(_iosShadows!=null))
		//                          _iosShadows.GetComponent<IosShadowManager>().ReinitIosShadow(selectedObj);
		                    }
		                }
		                else if(!m_selectMultiple)         // Rotation de tous les objets
		                {
		                    if(!m_rotateAllInit)
		                    {
		                        isMovingOrRotating(true);   // enlever le vert
		                        m_rotateAllInit = true;
		                        valueBeginRotate = 0.0f;
		                        GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().InitRotateAllCenter();
		                    }
		                    else
		                    {
		                     // float realAngle = GameObject.Find("MainNode").transform.eulerAngles.y;
		//                        float newAngle = RotateOject(cursor,valueBeginRotate, true);
		                        float newAngle = deltaRota;//deltaMove.x * 360f * 1.25f / Screen.width;
		                        GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().RotateAll(-newAngle);
		                        valueBeginRotate = valueBeginRotate+newAngle;
		                 //       UpdateIOSShadows();
		                    }
		
		                }
		                else if(!m_selectAll)           //Rotation de la sélection d'objets
		                {
		                    deselectMulti(false); // Enlever les textures vertes
		                    if(!m_rotateAllInit)
		                    {
		                        isMovingOrRotating(true);   // enlever le vert
		                        m_rotateAllInit = true;
		                        valueBeginRotate = 0.0f;
		                        m_msCenter = getMultiSelectionCenter();
		                    }
		                    else
		                    {
		                        float newAngle = deltaRota;//deltaMove.x * 360f * 1.25f / Screen.width;
		                        rotateMultiSelection(-newAngle);
		                        valueBeginRotate = valueBeginRotate+newAngle;
		             //           UpdateIOSShadows();
		                    }
		                }

	                    NotifyMode2DobjectsChanged();
					}
	            }
	            else if(PC.In.Click1Up())  //END
	            {

					if(_clickOnUI)
					{

		//                Debug.Log(".... end1");
	  	              if(m_selectMultiple)
	   	              	doTheEndedMulti(cursor);
	    	          else
	       	          	doTheEnded(cursor);
						_clickOnUI=false;
					}
	            }
	            else if(PC.In.Click2Up())
	            {

					if(_clickOnUI)
					{
							
		                //DbgUtils.Log("testtouch",".... end2");
		                m_rotateAllInit = false;
		                isMovingOrRotating(false);
		//                RotateOject(cursor, 0.0f, false);
		                if(m_selectAll)
		                    GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().SelectAll();
		                else if(m_selectMultiple)
		                    selectMulti();
						else
							selectedObj.GetComponent<ObjBehav>().UpdateCurrentHeight();
						_clickOnUI=false;
						frameToSkip=3;
					}
	            }
				else if(PC.In.Click3Up())
				{

					if(m_selectAll || m_selectMultiple)
							return;
					if(_clickOnUI)
					{
						selectedObj.GetComponent<ObjBehav>().UpdateCurrentHeight();
						_clickOnUI=false;
					}
				}
	//            else
	//                Debug.Log (".... rien de rien");
	        }
	    } // GenericInteraction

	    private void NotifyMode2DobjectsChanged()
	    {
	        // -- Notify Mode2D to display warning if the user opens it back --
	        if(!m_mode2D.IsActive() && m_mode2D.GetObjCount() > 0 &&
	            m_mode2D.IsPlanLoaded() && !m_mode2D.ShowingMovedObjectsWarning())
	        {
	            //m_mode2D.ShowMovedObjectsWarning();
	        }
	    }
	#endregion

		private void DoTheHeightChange(float dir)
		{
			if(m_selectAll || m_selectMultiple)
					return;
			selectedObj.GetComponent<ObjBehav>().ChangeLocalHeight(dir);
		}
			
	#region interaction non tactile	
		
		/*void mouseInteraction()
		{
			bool isOnUI = mainScene.GetComponent<GUIMenuMain>().isOnUi();
			isOnUI = isOnUI || mainScene.GetComponent<GUIStart>().isActive();
			isOnUI = isOnUI || Camera.mainCamera.GetComponent<GuiTextureClip>().IsOnUI();
			bool isOnSaveBox = mainScene.GetComponent<Montage>().isOnUI();
			bool isConfiguringScene = mainScene.GetComponent<GUIMenuRight>().isVisible();
			
			if(selectedObj != null)
				isConfiguringScene = isConfiguringScene && selectedObj.name != "_avatar";
			
			if(!actived || isOnUI || isSwaping || isConfiguringScene || isOnSaveBox)
			{
										print("returnmouseInteraction");
				return;
			}
			
			Vector2 cursor = PC.In.GetCursorPos();//Input.mousePosition;
			//Debug.Log("Debut du start");
			if(selectedObj == null && !m_selectAll && !m_selectMultiple)	// pas de selected object
			{
				//Debug.Log("pas de selectedObj");
				if(PC.In.Click1Down())//Input.GetMouseButtonDown(0))
				{
	//				setSelected(isOnObject(cursor)); // select au begin
					v2TempObject = isOnObject(cursor);
					if(v2TempObject != null)
					{
						print (v2TempObject.name);
					}
					v2CanSelect = true;
					//Debug.Log("set du canselect");
					oldMousePos = cursor;
				}
				if(PC.In.Click1Down() && Time.time>T+deltaT)//BEGIN
				{
					//Debug.Log("set du canselect");
					doTheBegan(cursor);
					m_moveStart = true;
					T = Time.time;
				}
				if(PC.In.Click1Up())//Input.GetMouseButtonUp(0))//END
				{
					//Debug.Log("passe par le end");
					doTheEnded(cursor);
				}
				if(PC.In.Click1Hold())//Input.GetMouseButton(0) )//MOVE
				{
					doTheMoved(cursor);
				}
			}
			else 					// deja un selectedObject ou sélection multiple
			{
				if(PC.In.Click1Down()  && Time.time>T+deltaT)//BEGIN
				{
					if(m_selectMultiple)
						doTheBeganMulti(cursor);
					else
						doTheBegan(cursor);
					T = Time.time;
					oldMousePos = cursor;
				}
				else if(PC.In.Click1Hold())//Input.GetMouseButton(0) )//MOVE
				{
					if(Vector2.Distance(cursorBegin, cursor)<=Mathf.Epsilon)
					{
						countImobile++;
						if(countImobile>200)
						{
							if(m_selectMultiple)
								doTheEndedMulti(cursor);
							else
								doTheEnded(cursor);
						}
					}
					else
					{
						if( (!m_selectMultiple) )
	                        doTheMoved(cursor);
						else
							doTheMovedMulti(cursor);
					}
				}
				else if(PC.In.Click2Hold())//Input.GetMouseButton(1))                               //ROTATE
				{
	                if(!m_selectAll && !m_selectMultiple) // Rotation d'un seul objet
	                {
						if(!selectedObj.GetComponent<ObjBehav>().amILocked())
						{
			                float realAngle = selectedObj.transform.eulerAngles.y;
			            //    realAngle = realAngle + 0.5f;
						//    selectedObj.transform.eulerAngles = new Vector3 (0, realAngle, 0);
							float newAngle = RotateOject(cursor,realAngle, true);
						    selectedObj.transform.eulerAngles = new Vector3 (0, newAngle, 0);
	                 //       UpdateIOSShadows();
					//		if((selectedObj.name == "_avatar")&&(_iosShadows!=null))
					//			_iosShadows.GetComponent<IosShadowManager>().ReinitIosShadow(selectedObj);
						}
	                }
	                else if(!m_selectMultiple)         // Rotation de tous les objets
	                {
	                    GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().UnselectAll(); // Enlever les textures vertes
	                    if(!m_rotateAllInit)
	                    {
	                        m_rotateAllInit = true;
							valueBeginRotate = 0.0f;
	                        GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().InitRotateAllCenter();
	                    }
	                    else
	                    {
							//float realAngle = GameObject.Find("MainNode").transform.eulerAngles.y;
							float newAngle = RotateOject(cursor,valueBeginRotate, true);
						//	Debug.Log("valueBeginRotate " + valueBeginRotate + " newAngle " + newAngle);
	                        GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().RotateAll(newAngle);
							valueBeginRotate = valueBeginRotate-newAngle;
	 //                       UpdateIOSShadows();
	                    }
	                }
					else if(!m_selectAll) 			//Rotation de la sélection d'objets
					{
						deselectMulti(false); // Enlever les textures vertes
	                    if(!m_rotateAllInit)
	                    {
	                        m_rotateAllInit = true;
							valueBeginRotate = 0.0f;
	                        m_msCenter = getMultiSelectionCenter();
	                    }
	                    else
	                    {
							float newAngle = RotateOject(cursor,valueBeginRotate, true);
							//Debug.Log("valueBeginRotate " + valueBeginRotate + " newAngle " + newAngle);
	                        rotateMultiSelection(newAngle);
							valueBeginRotate = valueBeginRotate-newAngle;
	     //                   UpdateIOSShadows();
	                    }
					}
				}
				
				else if(PC.In.Click1Up())//Input.GetMouseButtonUp(0))//END
				{
					if(m_selectMultiple)
						doTheEndedMulti(cursor);
					else
						doTheEnded(cursor);
				}
	            else if(PC.In.Click2Up())//Input.GetMouseButtonUp(1))
	            {
	                m_rotateAllInit = false;
					RotateOject(cursor, 0.0f, false);
	                if(m_selectAll)
	                    GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().SelectAll();
					if(m_selectMultiple)
						selectMulti();
	            }
			}
		}*/
		
	/*	protected float RotateOject(Vector2 cursor, float angle, bool state)
		{
			if(state)//begin	
			{
				isMovingOrRotating(true);
				if(!initRotate) // debut de la rotation
				{
					cursorBeginRotate = cursor;
					valueBeginRotate = angle;
					initRotate = true;
				}
				{
					Vector2 v = cursor - cursorBeginRotate;
					
					angle =  - v.x*0.5f;
					
	//				Debug.Log("angle +" + angle 
	//					+" cursorBeginRotate : "+cursorBeginRotate.x
	//					+" cursor : "+cursor.x
	//					+" v : "+v.x);
					float newAngle = angle+valueBeginRotate;
	//				if((_iosShadows!=null)&& !m_selectAll && !m_selectMultiple)
	//					_iosShadows.GetComponent<IosShadowManager>().UpdateShadowPos(selectedObj);
					return newAngle;
				//	selectedObj.transform.eulerAngles = new Vector3 (0, angle+valueBeginRotate, 0);

				}
			}
			else //fin de la rotation
			{
				isMovingOrRotating(false);
				initRotate = false;
				return angle;
			}

		}*/
	#endregion
		GameObject isOnObject(Vector2 touchPos)//touchPos NON Inversé
		{
			GameObject toReturn = null;	
			
			RaycastHit hitm = new RaycastHit ();
			Physics.Raycast (Camera.main.ScreenPointToRay(touchPos), out hitm, Mathf.Infinity,/*raycastLayer*/~LayerMask.NameToLayer("Raycaster"));
			if(hitm.rigidbody!= null)
			{
				int layer = hitm.rigidbody.gameObject.layer;
				if(layer == 9 && hitm.rigidbody.gameObject.GetComponent<ObjBehav>())
				{
					toReturn = hitm.rigidbody.gameObject;
					old3DPos = hitm.point;
				}
			}
			
			if(toReturn == null)
			{
				if(_collidedObj!= null)
				{
					toReturn = _collidedObj;
				}
			}		
			return toReturn;
		}
		
		
		
		void OnTriggerEnter(Collider other) 
		{
			GameObject g = other.gameObject;
			if(g.layer == 9 && g.GetComponent<ObjBehav>())
			{
				_collidedObj = g;
			}
		}
		void OnTriggerExit(Collider other)
		{
			GameObject g = other.gameObject;
			if(g.layer == 9 && g.GetComponent<ObjBehav>())
			{
				if(g == _collidedObj)
					_collidedObj = null;
			}
		}
		
	    //-----------------------------------------------------------------------------------
	    // Pour ipad
		
	#region interaction tactile	Obsolete
		
		/*void doTheJob(Touch t)
		{
			// do when selectedobj != null
			switch (t.phase)
			{
			case TouchPhase.Began :
				if(Time.time > timer + delay)
				{
					doTheSelection(t);
					timer = Time.time;
				}
				break;
			case TouchPhase.Moved :
				doTheMove();
				break;
			case TouchPhase.Ended:
				doTheUnselect();
				break;
			}	
		}
		
		void doTheSelection(Touch t) //stack selection
		{
			if(tmp == null)
			{
				canDeselect = true;
			}
			if(selectedObj != null)
			{ 
				print ("nonull");
				if(selectedObj.name != "_avatar")
				{
					print (selectedObj.name);
					print ("coucou : " + GameObject.Find ("_avatar").GetComponent<ObjBehav>().enabled);
					
					if(GameObject.Find ("_avatar").GetComponent<ObjBehav>().enabled)
					{
						print ("lock");
						return;
					}
				}
			}
			else if (tmp != selectedObj && Vector2.SqrMagnitude(t.position-deadzonePos)>deadzoneLimits)
			{
				canDeselect = false;
				selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
				selectedObj = null;
				//selectedObj = tmp;
				setSelected(tmp);
				selectedObj.GetComponent<ObjBehav>().setAsSelected(selectedObj);
				oldMousePos = t.position;
				deadzonePos = t.position;
			}
			else // tmp == selectedObj
			{
				RaycastHit[] hits;
				hits = Physics.RaycastAll(Camera.mainCamera.ScreenPointToRay(t.position), Mathf.Infinity,raycastLayer);
				
				stack.Clear();
				foreach(RaycastHit rch in hits)
				{
					if(rch.rigidbody!= null)
					{
						if(rch.rigidbody.gameObject.layer == 9)
							stack.Add(rch);
					}
				}
				
				//Debug.Log("Stack COUNT " + stack.Count+ " stackindex "+stackIndex);
				
				if(stack.Count>1)
				{
					stack.Sort(distanceSorter.sortDistanceAscending());
					
					if(stackIndex+1 < stack.Count)
						stackIndex ++;
					else
						stackIndex = 0;
					
					canDeselect = false;
					selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
					selectedObj = null;
					selectedObj = ((RaycastHit)stack[stackIndex]).rigidbody.gameObject;
					selectedObj.GetComponent<ObjBehav>().setAsSelected(selectedObj);
				}
				oldMousePos = t.position;
				
			}
			
		}
		
		void doTheMove()
		{

		}*/
		
		void doTheUnselect()
		{
			dragStarted = false;
			selectionLock = false;
			
			if(selectedObj != null)
				selectedObj.GetComponent<Rigidbody>().isKinematic = true;
			
			if(canDeselect)
			{
				selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
				//selectedObj = null;
				setSelected(null);
				//UIActive = false;
				canDeselect = false;
				stackIndex = 0;
			}
			else
			{	
				selectedObj.GetComponent<ObjBehav>().setAsSelected(selectedObj);
				UIActive = true;
				isMovingOrRotating(false);
			}
		}
			
		void isMovingOrRotating(bool b)
		{
			_highLightOverride = b;
			if(b)
			{
				mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
				mainScene.GetComponent<GUIMenuRight>().setVisibility(false);
			}
			else
			{
				if(selectedObj!= null)
				{
					if(selectedObj.name != "_avatar")
					{
						mainScene.GetComponent<GUIMenuInteraction>().setVisibility(true);
					}
					else if(selectedObj.name == "_avatar")
					{
						mainScene.GetComponent<GUIMenuRight>().ShowAvatarSettings();
					}
				}
	//			else{
	//				Debug.LogWarning("selected obj null");
	//			}

			}
		}
		
		private class distanceSorter : IComparer
		{
			int IComparer.Compare(object a, object b)
			{
	//		  car c1=(car)a;
	//		  car c2=(car)b;
	//		  if (c1.year > c2.year)
	//		     return 1;
	//		  if (c1.year < c2.year)
	//		     return -1;
	//		  else
	//		     return 0;
				RaycastHit objA = (RaycastHit) a;
				RaycastHit objB = (RaycastHit) b;
				
				if(objA.distance > objB.distance)
					return 1;
				if(objA.distance < objB.distance)
				{
					if(objA.rigidbody.gameObject.GetComponent<ObjBehav>().amILocked())
						return 1;
					else
						return -1;
				}
				else 
					return 0;
			}
			public static IComparer sortDistanceAscending()
			{      
			   return (IComparer) new distanceSorter();
			}
		}
		
		/*void amelioratedInteraction()
		{
	//		bool isOnInteractionUi = mainScene.GetComponent<GUIInteraction>().isOnUi();
			bool isOnUI = mainScene.GetComponent<GUIMenuMain>().isOnUi();
	//		bool isInConfigurationMode = mainScene.GetComponent<GUIMapsHandler>().isConfiguring();
			bool isConfiguringScene = mainScene.GetComponent<GUIMenuRight>().isVisible();
			if(!actived || isOnUI || isSwaping || isConfiguringScene)
				return;
			
			if(Input.touchCount == 1)
			{
				Touch t = Input.touches[0];
				
				if(selectedObj == null) ////////////pas de selectedObj
				{
					if(t.phase == TouchPhase.Began)
					{
						//selectObj
						GameObject tmp = isOnObject(t.position);
						if(tmp)
						{
	//						selectedObj = tmp;
							setSelected(tmp);
							selectedObj.GetComponent<ObjBehav>().setAsSelected(selectedObj);
	//						UIActive = true;
							oldMousePos = t.position;
							//set de la deadzone
							deadzonePos = t.position;
							stackIndex = 0;
						}
					}
				}
				
				else ////////////////////////////////objet sélectionné
				{
					
					doTheJob(t);				
				}
				
			}
			else if(Input.touchCount == 2)
			{
				Touch t0 = Input.touches[0];
				Touch t1 = Input.touches[1];
				
				if(selectedObj != null)
				{
					//can deselect = false;
					if(t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Moved)
					{
						//rotate
						selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
						rotate = true;
						//UIActive = false;
						isMovingOrRotating(true);
						tactileRotation2();
					}
					else if(t0.phase == TouchPhase.Ended && t1.phase == TouchPhase.Ended)
					{
						rotate = false;
						//UIActive = true;
						isMovingOrRotating(false);
						selectedObj.GetComponent<ObjBehav>().setAsSelected(selectedObj);
	                    
	                    // Mise à jour de la position du projecteur d'ombre pour iOS. TODO : ajouter un "if(iOS) ..."
	                  				


					}
				}
			}
			else if(Input.touchCount == 0 && selectedObj != null)
			{
				rotate = false;
				dragStarted = false;
			}
			
		}*/
		
	#endregion
		
	#region tactile Interaction V2
		
		/*void tactileInteraction()
		{
			bool isOnUI = mainScene.GetComponent<GUIMenuMain>().isOnUi();
			isOnUI = isOnUI || mainScene.GetComponent<GUIStart>().isActive();
			isOnUI = isOnUI || Camera.mainCamera.GetComponent<GuiTextureClip>().IsOnUI();
			bool isConfiguringScene = mainScene.GetComponent<GUIMenuRight>().isVisible();
			if(selectedObj != null)
				isConfiguringScene = isConfiguringScene && selectedObj.name != "_avatar";
			bool isOnSaveBox = mainScene.GetComponent<Montage>().isOnUI();
			
			if(!actived || isOnUI || isSwaping || isConfiguringScene || isOnSaveBox)
			{
											print ("returntactileInteraction");
				return;
			}
			
			if(Input.touchCount == 1) // select'n'drag
			{
				Touch t = Input.touches[0];

				if(selectedObj == null && !m_selectAll && !m_selectMultiple)  // pas de selected object
				{
					if(t.phase == TouchPhase.Began)
					{
						//setSelected(isOnObject(t.position));
						v2TempObject = isOnObject(t.position);
						v2CanSelect = true;
						oldMousePos = t.position;
					}
					if(t.phase == TouchPhase.Moved)
					{
						if(!m_selectMultiple)
						{
							doTheMoved(t.position);
						}
					}
					if(t.phase == TouchPhase.Ended)
					{
						doTheEnded(t.position);
					}
				}
				else //if(selectedObj != null)					// deja un selectedObject
				{
					if(!m_selectMultiple)
					{
						switch (t.phase) 
						{
						case TouchPhase.Began:
							doTheBegan(t.position);
	                        oldMousePos = t.position;
							m_moveStart = true;
							break;
						case TouchPhase.Moved:
							doTheMoved(t.position);
							break;
						case TouchPhase.Ended:
							doTheEnded(t.position);
							break;
						}
					}
					else
					{
						switch (t.phase) 
						{
						case TouchPhase.Began:
							doTheBeganMulti(t.position);
							m_moveStart = true;
							break;
						case TouchPhase.Moved:
							doTheMovedMulti(t.position);
							break;
						case TouchPhase.Ended:
							doTheEndedMulti(t.position);
							break;
						}
					}
				}
			}
			else if(Input.touchCount == 2) // rotate
			{
				Touch t0 = Input.touches[0];
				Touch t1 = Input.touches[1];
				v2CanMove = false;

	            if(m_selectAll && !m_selectMultiple)            // Rotation globale
	            {
	                if(t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
	                {
	                    GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().InitRotateAllCenter();
						GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().UnselectAll();   // Désactivation des textures vertes
	                }
	                else if(t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Moved)
	                {
	                    rotate = true;
	                    isMovingOrRotating(true);
						v2CanDeselect = false;
	                    GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().RotateAll(tactileRotation2());
					    
	     //               UpdateIOSShadows();
	                }
					else if(t0.phase == TouchPhase.Ended && t1.phase == TouchPhase.Ended)
					{
						isMovingOrRotating(false);
						GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().SelectAll();    // Activation des textures vertes
					}
	            }
				else if(m_selectMultiple) //Rotation multiple
				{
					if(t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
	                {
	                    m_msCenter = getMultiSelectionCenter();
						deselectMulti(false);   // Désactivation des textures vertes
	                }
	                else if(t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Moved)
	                {
						m_canDeselectMulti = false;
						float slideSpeed = (Mathf.Abs(t0.deltaPosition.x +t1.deltaPosition.x))/5.0f;
						slideSpeed = ((t0.deltaPosition.x>=0 && t1.deltaPosition.x>=0)? -1 : 1)*slideSpeed;
	                    rotate = true;
	                    isMovingOrRotating(true);
						rotateMultiSelection(slideSpeed);
	//                    UpdateIOSShadows();
	                }
					else if(t0.phase == TouchPhase.Ended || t1.phase == TouchPhase.Ended)
					{
						isMovingOrRotating(false);
						selectMulti();    // Activation des textures vertes
					}
				}
				else if(selectedObj != null && !selectedObj.GetComponent<ObjBehav>().amILocked())       // Rotation d'un seul objet
				{
					//can deselect = false;
					if(t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
	                {
						//_lblStateTotation = "Began";
						selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);         // Désactivation de la texture verte
	                }
					if(t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Moved)
					{
						//_lblStateTotation = "Moved";
						//rotate
						rotate = true;
						//UIActive = false;
						isMovingOrRotating(true);
						tactileRotation2();
					}
					else if(t0.phase == TouchPhase.Ended && t1.phase == TouchPhase.Ended)
					{
						//_lblStateTotation = "Ended";
						rotate = false;
						//UIActive = true;
						isMovingOrRotating(false);
						selectedObj.GetComponent<ObjBehav>().setAsSelected(selectedObj);   // Réactivation de la texture verte
	                    
	                    // Mise à jour de la position du projecteur d'ombre pour iOS. TODO : ajouter un "if(iOS) ..."
	                    					
					}
				}
			}
		} // tactileInteraction()*/
	#endregion
		
	#region fonctions interaction monoObj

	    //-----------------------------------------------------
		void doTheBegan(Vector2 t)
		{
			if(mainScene.GetComponent<GUIMenuLeft>().isVisible())
			{
				return;
			}
			if(selectedObj == null && !m_selectAll)
			{
				return;
			}
	//		if(m_selectAll /*|| !selectedObj.GetComponent<ObjBehav>().collisionLock*/)
	//		{
				GameObject tmp = isOnObject(t);
	            if(m_selectAll)
	            {
	//                Debug.Log("..... DoTheBegan : tout deja selectionne ");
	                GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().InitDragAllPos();
	                v2CanMove = true;
	                v2CanDeselect = true;
					oldMousePos = t;
	            }
				else if(tmp == null)
				{
	//                Debug.Log("..... DoTheBegan : personne dessous");
					v2CanDeselect = true;
					v2CanMove = true;
					oldMousePos = t;
	//				selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj); // Désactivation de la texture verte
				}
				else if(tmp == selectedObj || Vector2.SqrMagnitude(t-oldMousePos)<deadzoneLimits)
				{
	//                Debug.Log("..... DoTheBegan : clic sur le meme");
					v2CanSelectNext = true;
					v2CanMove = true;
					oldMousePos = t;
	//				selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj); // Désactivation de la texture verte
				}
				else
				{
	//                Debug.Log("..... DoTheBegan : clic sur un autre !");
					if(selectedObj != null)
					{
	//					if(selectedObj.GetComponent<ObjData>().isPrimaryObject() && 
	//					!selectedObj.GetComponent<ObjBehav>().amILocked())
	//					{
	//						//AUTOLOCK
	//						StartCoroutine(autoLock(selectedObj));
	//						//selectedObj.GetComponent<ObjBehav>().Lock();	
	//					}	
					}
					//setSelected(tmp);//mettre un bool canselect a true ici et un canseleectOBJ = tmp et deplacer le setselected dans le dotheended
					v2TempObject = tmp;
					v2CanSelect = true;
					
					v2CanMove = true;
					oldMousePos = t;
	//				selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj); // Désactivation de la texture verte
				}
	//		}
	//		else if(selectedObj.GetComponent<ObjBehav>().collisionLock)
	//		{
	//			oldMousePos = t;
	//			v2CanMove = true;
	//		}
			cursorBegin = t;
		}
		
		void doTheMoved(Vector2 t)
		{

			//annuler le canSelect si activé
	//		Debug.Log("v2CanMove "+v2CanMove);
			if(v2CanSelect && v2TempObject != null && selectedObj == null)
			{
				setSelected(v2TempObject);
				v2CanSelect = false;
			}
			else
			{
				v2CanSelect = false;
			}
			if(v2CanMove)
			{
				v2CanSelectNext = false;
				v2CanDeselect = false;
				
				dragStarted = true;
				isMovingOrRotating(true);
	//			if(selectedObj.rigidbody.isKinematic)
	//				selectedObj.rigidbody.isKinematic = false;
				
	            if(m_selectAll)             // MAJ des ombres ios de tous les objets
	            {
	                v2CanDeselect = false;
	//                Debug.Log("DoTheMoved -->"+selectedObj+"<--");

					GameObject mainNode = GameObject.Find("MainNode");
					Transform child;
					
					Vector3 before = mainNode.GetComponent<ObjBehavGlobal>().GetSceneCenter();
					Vector3 move = doTheDrag(before) - before;
					move.y = 0;
					
					for(int i=1; i<mainNode.transform.GetChildCount(); i++)     // Déplacement des autres objets
		            {
		                child = mainNode.transform.GetChild(i).transform;
		                if(child.name != "_avatar")
		                {
							child.transform.position += move;
							/*if(_iosShadows)
								_iosShadows.GetComponent<IosShadowManager>().UpdateShadowPos(child.gameObject);
	                        else
								Debug.LogWarning("Objet \"iosShadows\" introuvable. Les ombres pour iOS risquent de ne pas fonctionner.");*/
		                }
		            }
	            }
	            else if(selectedObj != null && !selectedObj.GetComponent<ObjBehav>().amILocked())
	            {
					if(m_moveStart)
					{
						selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj); // Désactivation de la texture verte
						m_moveStart = false;
					}
					
	//				Vector2 deltaMove;
	//				if(PC.In.Drag1(deltaMove))
	//				{
	//            		Vector3 objPos = selectedObj.transform.localPosition;
	//					selectedObj.transform.localPosition = _3Dutils.ScreenMoveTo3Dmove(deltaMove, objPos);
	            		Vector3 objPos = selectedObj.transform.localPosition;
						selectedObj.transform.localPosition = doTheDrag(objPos);
							
		                // Mise à jour de la position du projecteur d'ombre pour iOS. TODO : ajouter un "if(iOS) ..."
		            /*    if(_iosShadows)
		                    _iosShadows.GetComponent<IosShadowManager>().UpdateShadowPos(selectedObj);
		                else Debug.LogWarning("Objet \"iosShadows\" introuvable. Les ombres pour iOS risquent de ne pas fonctionner.");*/
	//				}
	            }
			}
			countImobile=0;
		}
		
		void doTheEnded(Vector2 t)
		{		
										
			if(v2TempObject != null && v2TempObject.name != "_avatar" && 
			GameObject.Find ("_avatar").GetComponent<ObjBehav>().enabled)
			{ 
				return;
			}
			
			//rajouter un if(canSelect) > setselected(canseleectOBJ)
			//Debug.Log("Debut du end");
			if(v2CanSelect && v2TempObject != null && v2CanSelectNext == false)
			{
				//Debug.Log("selection dans le end");
				setSelected(v2TempObject);
				v2CanSelect = false;
				v2CanDeselect = false;
			}
			if(m_selectAll && v2CanDeselect)
	        {
	            UnselectAll();
	            v2CanDeselect = false;
	        }
	        else if(m_selectAll)
	        {
	            GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().SelectAll();   // Réactive les textures vertes
				isMovingOrRotating(false);
	        }
	        else if(!m_selectAll)
			{
	            setSelected(selectedObj);

				if(selectedObj != null)
				{
					if(selectedObj.name == "_avatar")
					{
						/*GameObject.Find("_avatar").GetComponent<ObjBehav>().isActive = false;
						GameObject.Find("_avatar").GetComponent<ObjBehav>().enabled = false;
						mainScene.GetComponent<GUIMenuRight>().ShowAvatarSettings();*/
					}
					
					isMovingOrRotating(false);

	//				if(selectedObj.GetComponent<ObjBehav>().collisionLock)
	//					mainScene.GetComponent<HelpPanel>().set2ndHelpTxt("L'objet ne peut pas se placer à cet endroit");
	//				else
						mainScene.GetComponent<HelpPanel>().set2ndHelpTxt("");
				//}
				 
		    		if(v2CanDeselect /*&& !selectedObj.GetComponent<ObjBehav>().collisionLock*/)
		    		{
		    			v2CanDeselect = false;
		//				if(selectedObj.GetComponent<ObjData>().isPrimaryObject() && 
		//					!selectedObj.GetComponent<ObjBehav>().amILocked())
		//				{
		//					//AUTOLOCK
		//					StartCoroutine(autoLock(selectedObj));
		//					//selectedObj.GetComponent<ObjBehav>().Lock();	
		//				}
							setSelected(null);
						

		    		}
					else if(v2CanSelectNext)
					{
		    			v2CanSelectNext = false;
													
		    			RaycastHit[] hits;
		    			hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(t), Mathf.Infinity,raycastLayer);
		    			
		    			stack.Clear();
		    			foreach(RaycastHit rch in hits)
		    			{
		    				if(rch.rigidbody!= null)
		    				{
		    					if(rch.rigidbody.gameObject.layer == 9)
		    						stack.Add(rch);
		    				}
		    			}
		    			
		    			if(stack.Count>1)
		    			{
		    				stack.Sort(distanceSorter.sortDistanceAscending());
		    				
		    				if(stackIndex+1 < stack.Count)
		    					stackIndex ++;
		    				else
		    					stackIndex = 0;
											
		    				canDeselect = false;
		    				selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
		    				selectedObj = null;
		    				setSelected(((RaycastHit)stack[stackIndex]).rigidbody.gameObject);
		    				selectedObj.GetComponent<ObjBehav>().setAsSelected(selectedObj);
		    			}
		    			oldMousePos = t;
	    			}
				}
	        }
			countImobile=0;
		} // doTheEnded()

		Vector3 doTheDrag(Vector3 currentPos)
		{
			float d = 1;
	//		objToMove.GetComponent<ObjBehav>().setAsUnselected(objToMove);
	//		objToMove.GetComponent<ObjBehav>().locked = false;
			
	//		Vector2 ret = Vector2.zero;
			//Vector2 currentMousePos = Input.mousePosition;
			
			Vector2 delta2;// = currentMousePos-oldMousePos;
			if(!PC.In.Drag1(out delta2)) // quick fix en attendant de remettre genericInteraction au propre
				return currentPos;

			Vector3 newPos = new Vector3(currentPos.x, currentPos.y, currentPos.z);

			if(delta2 != Vector2.zero && !PC.In.Click2Up())
			{
				//DbgUtils.Log ("testtouch","draging, delta ="+delta2.ToString());
				newPos = _3Dutils.ScreenMoveTo3Dmove(delta2, currentPos, 9, true);
				
				
	//			Vector3 cdmInW = currentPos;
	//			cdmInW.y = 0;
	//			Vector3 cdmOnScreen3 = Camera.mainCamera.WorldToScreenPoint(cdmInW);
	//			Vector2 cdmOnScreen2 = new Vector2(cdmOnScreen3.x,cdmOnScreen3.y);
	//			cdmOnScreen2 = cdmOnScreen2+delta2;
	//			
	//			RaycastHit newhitm = new RaycastHit ();
	//			Physics.Raycast (Camera.mainCamera.ScreenPointToRay(cdmOnScreen2), out newhitm, Mathf.Infinity,9);
	//						
	//			if(newhitm.rigidbody)
	//			{
	//				ret.x = newhitm.point.x;
	//				ret.y = newhitm.point.z;
	//				newPos = newhitm.point;
	//				newPos.y = currentPos.y;
	////				Vector2 futureDir = new Vector2( newPos.x-oldPos.x,newPos.z-oldPos.z);
	////				objToMove.transform.position = newPos;
	//			}

	            NotifyMode2DobjectsChanged();
			}
			oldMousePos = PC.In.GetCursorPos();

			return newPos;
		}
	  /*  void OnGUI()
	    {
	        GUI.Button(new Rect(100, 100, 200, 200), oldMousePos.ToString());
	    }*/
			
		private void DoTheScaleMonoObject(float delta)
		{
			bool temp = m_selectedOSLibObj.GetObjectScaleGenerale() == 1 && m_selectedOSLibObj.GetParendBrandID () == 900 || m_selectedOSLibObj.GetParendBrandID () != 900;
			//Debug.Log("SCALE GENERALE  :  " + m_selectedOSLibObj.GetObjectScaleGenerale() + "   PARENT ID  :  " + m_selectedOSLibObj.GetParendBrandID());
			if(m_selectedOSLibObj.IsBrandObject() && temp)
			{
				camPivot.GetComponent<SceneControl>().setS(delta/m_ffactorScale);		
			}
			else
			{	
				//Scale ObjectOnly
				Vector3 scl = selectedObj.transform.localScale;
				scl *= 1+(delta/2.0f);
				scl.x = Mathf.Clamp(scl.x, 1f, 100.0f);
				scl.y = Mathf.Clamp(scl.y, 1f, 100.0f);
				scl.z = Mathf.Clamp(scl.z, 1f, 100.0f);
				
				float ratio = scl.x / selectedObj.GetComponent<ObjBehav>().GetBaseScale().x;
				if(ratio < 0.5f)
					ratio = 0.5f;
				UsefullEvents.FireUpdateIosShadowScale(selectedObj,ratio);
				
				selectedObj.transform.localScale = scl;
			}
		}

	#endregion

	#region interaction multiObjs
		
		public bool isMultiActive()
		{
			return m_selectMultiple;
		}
		
		void doTheBeganMulti(Vector2 pos)
		{
			GameObject tmp = isOnObject(pos);
			deselectMulti(false);
	        isMovingOrRotating(true);
			oldMousePos = pos;
			if(tmp == null)	// si clic dans le vide
			{
				m_canDeselectMulti = true;
			}
			else 			//si clic sur un objet
			{
				if(tmp.name == "_avatar")
				{
					cursorBegin = pos;
					return;
				}
				if(m_ObjsSelection.Contains(tmp)) 	// si deja dans la list > on l'y enleve
				{
					m_toDeselectUnique = tmp;
				}
				else 							// si pas déja dans la liste > on l'y ajoute
				{
					m_ObjsSelection.Add(tmp);
				}
			}
			cursorBegin = pos;
		}
		
		void doTheMovedMulti(Vector2 pos)
		{
			m_rotateAllInit = false;
			m_canDeselectMulti = false;
			if(m_toDeselectUnique)
				m_toDeselectUnique = null;
			doTheDragMulti();
			countImobile=0;
		}
		
		void doTheEndedMulti(Vector2 pos)
		{
	        isMovingOrRotating(false);
			if(m_toDeselectUnique)
			{
				m_ObjsSelection.Remove(m_toDeselectUnique);
				m_toDeselectUnique = null;
			}
			if(m_canDeselectMulti)
			{
				m_canDeselectMulti = false;
				deselectMulti(true);	
				mainScene.GetComponent<GUIMenuLeft>().canDisplay(true);
	       	 	mainScene.GetComponent<GUIMenuRight>().canDisplay(true);
			}
			else
			{
				selectMulti();
			}

			countImobile=0;
		}
		
		void deselectMulti(bool clear) //met en rouge la sélection
		{
			foreach (GameObject g in m_ObjsSelection)
			{
				g.GetComponent<ObjBehav>().setAsUnselected(g);	
			}
			if(clear)
			{
				m_ObjsSelection.Clear();
				m_selectMultiple = false;
				//m_selectAll = false;
				hud.hideShowUD(false);
				hud.setUDText("","");
			}

		}
		
		void selectMulti() //met en vert la séléction
		{
			foreach (GameObject g in m_ObjsSelection)
			{
				g.GetComponent<ObjBehav>().setAsSelected(g);
			}
		}
		
		void doTheDragMulti()
		{
			GameObject mainNode = GameObject.Find("MainNode");
			
			Vector3 before = mainNode.GetComponent<ObjBehavGlobal>().GetSceneCenter();
			Vector3 move = doTheDrag(before) - before;
			move.y = 0;
			
			foreach(GameObject g in m_ObjsSelection)  // Déplacement des autres objets
		    {
				Transform child = g.transform;
				child.transform.position += move;
			/*	if(_iosShadows)
					_iosShadows.GetComponent<IosShadowManager>().UpdateShadowPos(child.gameObject);
		        else
					Debug.LogWarning("Objet \"iosShadows\" introuvable. Les ombres pour iOS risquent de ne pas fonctionner.");*/

		    }	
		}
		
		void rotateMultiSelection(float angleChange)
		{
		    if(angleChange != 0)
		    {
		        Transform child;
		        foreach(GameObject g in m_ObjsSelection)
		        {
		            child = g.transform;
		            child.RotateAround(m_msCenter, Vector3.up, angleChange);
		        }
		    }
		}
		
		Vector3 getMultiSelectionCenter()
		{
			// Moyenne
			Vector3 center = Vector3.zero;
	        Transform child;
	        foreach(GameObject g in m_ObjsSelection)
	        {
	            child = g.transform;
	            if(child.name != "_avatar")
					center += child.position;
	        }
			center.x /= m_ObjsSelection.Count;
			center.y = 0;
			center.z /= m_ObjsSelection.Count;
			
			return center;	
		}
		
		public void SelectMultiple()
		{
			hud.hideShowUD(true);
			hud.setUDText(TextManager.GetText("GUIMenuLeft.MultipleSelect"),
	                                                         TextManager.GetText("GUIMenuLeft.Active"));
			
			 // Masquer les menus
	        mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
	        mainScene.GetComponent<GUIMenuLeft>().canDisplay(false);
	        mainScene.GetComponent<GUIMenuLeft>().setVisibility(false);
	        mainScene.GetComponent<GUIMenuRight>().canDisplay(false);
			
			m_selectMultiple = true;
			m_selectAll = false;
		}
	#endregion
		
	#region drag's and rotate's
		
		void drag(RaycastHit hitm)//drag Centré
		{
			float d = 1;
			selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
			selectedObj.GetComponent<ObjBehav>().collisionLock = false;
			
			Vector3 oldPos = selectedObj.transform.position;
			Vector3 newPos = hitm.point;
			
			Vector2 futureDir = new Vector2( newPos.x-oldPos.x,newPos.z-oldPos.z);

			selectedObj.transform.position = new Vector3(Mathf.Lerp(selectedObj.transform.position.x,hitm.point.x,5*Time.deltaTime),
			selectedObj.transform.position.y,Mathf.Lerp(selectedObj.transform.position.z,hitm.point.z,5*Time.deltaTime)); //celui de base qui marche
			
			if(selectedObj.GetComponent<ObjBehav>().escapeVector != Vector2.zero)
			{
				d = Vector2.Dot(selectedObj.GetComponent<ObjBehav>().escapeVector,futureDir);
			}
				
			if(selectedObj.GetComponent<ObjBehav>().objectCollision && d>0)// && collisioner.collider != null)
			{
				selectedObj.transform.position = oldPos;
			}
			else
			{
				selectedObj.GetComponent<ObjBehav>().objectCollision = false;
			}

		}
		
		void drag2(RaycastHit hitm)//déplacement décentré
		{
			float d = 1;
			selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
			selectedObj.GetComponent<ObjBehav>().collisionLock = false;
			
			Vector3 oldPos = selectedObj.transform.position;
			Vector3 newPos = oldPos;
			if(oldHitm == new Vector3(0,100000,0))
				oldHitm = hitm.point;
			else
			{
				delta = hitm.point - oldHitm;

				newPos = oldPos+delta;
				oldHitm = hitm.point;
			}

			Vector2 futureDir = new Vector2( hitm.point.x-oldPos.x,hitm.point.z-oldPos.z);

			selectedObj.transform.position = new Vector3(newPos.x,selectedObj.transform.position.y,newPos.z);
			//----
			

			if(selectedObj.GetComponent<ObjBehav>().escapeVector != Vector2.zero)
			{
				d = Vector2.Dot(selectedObj.GetComponent<ObjBehav>().escapeVector,futureDir);
			}
				
			if(selectedObj.GetComponent<ObjBehav>().objectCollision && d>0)// && collisioner.collider != null)
			{
				selectedObj.transform.position = oldPos;
			}
			else
			{
				selectedObj.GetComponent<ObjBehav>().objectCollision = false;
			}

		}
		
		void drag3(GameObject objToMove)//déplacement décentré mieux
		{
			float d = 1;

			Vector3 oldPos = objToMove.transform.position;

			
			Vector2 currentMousePos = Input.mousePosition;
			
			Vector2 delta2 = currentMousePos-oldMousePos;
			if(delta2 != Vector2.zero)
			{
				Vector3 cdmInW = oldPos;
				cdmInW.y = 0;
				Vector3 cdmOnScreen3 = Camera.main.WorldToScreenPoint(cdmInW);
				Vector2 cdmOnScreen2 = new Vector2(cdmOnScreen3.x,cdmOnScreen3.y);
				cdmOnScreen2 = cdmOnScreen2+delta2;
				
				RaycastHit newhitm = new RaycastHit ();
				Physics.Raycast (Camera.main.ScreenPointToRay(cdmOnScreen2), out newhitm, Mathf.Infinity,9);
							
				if(newhitm.rigidbody)
				{
					Vector3 newPos = newhitm.point;
					newPos.y = oldPos.y;
					Vector2 futureDir = new Vector2( newPos.x-oldPos.x,newPos.z-oldPos.z);
					
					objToMove.transform.position = newPos;
	//				if(selectedObj.GetComponent<ObjBehav>().escapeVector != Vector2.zero)
	//				{
	//					d = Vector2.Dot(selectedObj.GetComponent<ObjBehav>().escapeVector,futureDir);
	//				}
	//					
	//				if(selectedObj.GetComponent<ObjBehav>().objectCollision && d>0)// && collisioner.collider != null)
	//				{
	//					selectedObj.transform.position = oldPos;
	//				}
	//				else
	//				{
	//					selectedObj.GetComponent<ObjBehav>().objectCollision = false;
	//				}
				}
			}
			oldMousePos = currentMousePos;
		}
		
		void drag4(Vector3 new3DPos)
		{
			float d = 1;
			selectedObj.GetComponent<ObjBehav>().collisionLock = false;
			Vector3 oldPos = selectedObj.transform.position;
			Vector3 delta3D = new3DPos - old3DPos;
			delta3D.y = 0;
			
			
			selectedObj.GetComponent<Rigidbody>().position = selectedObj.GetComponent<Rigidbody>().position + delta3D;
			
			old3DPos = new3DPos;
			
		}
		
		//rotation a deux doigts en tournant
		float tactileRotation()
		{
			Vector3 torque = Vector3.zero;
			if(Input.touches[0].phase == TouchPhase.Began || Input.touches[1].phase == TouchPhase.Began)
			{
				//firstTouch = false;
				oldDir = Input.touches[1].position - Input.touches[0].position;
				angle = 0;
			}
			else
			{
				angle = Vector2.Angle((oldDir),(Input.touches[1].position - Input.touches[0].position));
				
				//sens rotation
				Vector3 f = new Vector3(Input.touches[1].deltaPosition.x,0,Input.touches[1].deltaPosition.y);
				Vector3 ab = new Vector3(oldDir.x,0,oldDir.y);
				
				Vector3 f2 = new Vector3(Input.touches[0].deltaPosition.x,0,Input.touches[0].deltaPosition.y);
				Vector3 ab2 = new Vector3(-oldDir.x,0,-oldDir.y);
				
				torque = Vector3.Cross(ab,f) + Vector3.Cross(ab2,f2);

	//			float realAngle = selectedObj.transform.eulerAngles.y;
	//			realAngle = realAngle + Mathf.Sign(torque.y)*angle;				
	//			selectedObj.transform.eulerAngles = new Vector3 (0, realAngle, 0);
				selectedObj.transform.RotateAroundLocal(selectedObj.transform.up,Mathf.Sign(torque.y)*((angle/5)*(angle/5))*Time.deltaTime);
				oldDir = Input.touches[1].position - Input.touches[0].position;
			}			
			return Mathf.Sign(torque.y)*5;
		}
		
		//rotation a deux doigts en slidant gauche droite
		float tactileRotation2()
		{
			Touch t0 = Input.touches[0];
			Touch t1 = Input.touches[1];
							
		/*	_touchPos.Clear();
			_lblTouchCount = Input.touchCount;
			foreach (Touch touch in Input.touches)
			{
				_touchPos.Add(touch.deltaPosition);
			}
			*/
			float slideSpeed = (Mathf.Abs(t0.deltaPosition.x +t1.deltaPosition.x))/5.0f;
			if(Application.platform == RuntimePlatform.IPhonePlayer)
			{
				slideSpeed = (Mathf.Abs(t0.deltaPosition.x +t1.deltaPosition.x))/5.0f;
			}
			else if (Application.platform == RuntimePlatform.Android)
			{
				if(t0.deltaPosition.x > 0 || t0.deltaPosition.x < 0)
					slideSpeed = (Mathf.Abs(t0.deltaPosition.x)*2)/2.0f;
				else if(t1.deltaPosition.x > 0 || t1.deltaPosition.x < 0)
					slideSpeed = (Mathf.Abs(t1.deltaPosition.x)*2)/2.0f;
			}
				
		//	_lblStateTotation = slideSpeed.ToString();
			if(!m_selectAll)
			{
				if(t0.deltaPosition.x >= 0 && t1.deltaPosition.x >= 0)
				{
					float realAngle = selectedObj.transform.eulerAngles.y;
					realAngle = realAngle - slideSpeed;
					selectedObj.transform.eulerAngles = new Vector3 (0, realAngle, 0);
		
				}
				else if(t0.deltaPosition.x <= 0 && t1.deltaPosition.x <= 0)
				{
					float realAngle = selectedObj.transform.eulerAngles.y;
					realAngle = realAngle + slideSpeed;
					selectedObj.transform.eulerAngles = new Vector3 (0, realAngle, 0);
				}
			}
			/*if(selectedObj!=null)
			{
				if(_iosShadows!=null)
					_iosShadows.GetComponent<IosShadowManager>().UpdateShadowPos(selectedObj);
			}*/
	        return ((t0.deltaPosition.x>=0 && t1.deltaPosition.x>=0)? -1 : 1)*slideSpeed;
		}
		
	#endregion
		
		IEnumerator TimerDrag ()
		{
			if (!_isTimer) {
				_isTimer = true;
				Debug.Log ("StartTimer 0");
				yield return new WaitForSeconds (5.0f);
				Debug.Log ("StartTimer 1");
				updateDrag = false;
				Debug.Log ("StartTimer 2");
				_isTimer = false;
			}

		}
		
	//	IEnumerator longClick (GameObject o)
	//	{
	//		if(!longClickStarted)
	//		{
	//			longClickStarted = true;
	//			yield return new WaitForSeconds (0.2f);
	//			if (Input.GetMouseButton (0)) 
	//			{
	////				selectedObj = o;
	////				UIActive = false;
	//				setSelected(o);
	//				
	//				dragStarted = true;
	//				mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
	//				//drag3
	//				oldMousePos = Input.mousePosition;
	//				RaycastHit hitm = new RaycastHit ();
	//				Physics.Raycast (Camera.mainCamera.ScreenPointToRay(oldMousePos), out hitm, Mathf.Infinity,9);
	//				if(hitm.rigidbody)
	//					old3DPos = hitm.point;
	//				//------
	//				raycastLayer = 9;
	//			}
	//			longClickStarted = false;
	//			yield return true;
	//		}
	//	}
		
	//	IEnumerator autoLock(GameObject o)
	//	{
	//		showAutoLock = true;
	//		autolockTxt = o.name+" verouille";
	//		yield return new WaitForEndOfFrame();
	//		o.GetComponent<ObjBehav>().Lock();
	//		yield return new WaitForSeconds(0.25f);
	//		o.GetComponent<ObjBehav>().Lock();
	//		yield return new WaitForSeconds(0.25f);
	//		o.GetComponent<ObjBehav>().Lock();
	//		yield return new WaitForSeconds(0.25f);
	//		o.GetComponent<ObjBehav>().setAsUnselected(o);
	//		yield return new WaitForSeconds(0.5f);
	//		showAutoLock = false;
	//		
	//		yield return null;
	//	}
		
	//	public void deactivate ()
	//	{
	//		UIActive = false;
	//		selectedObj = null;
	//	}

	//	IEnumerator TempRotationLock ()
	//	{
	//		rotationLock = false;
	//		rotationSnap = false;
	//		yield return new WaitForSeconds (0.5f);
	//		rotationLock = true;
	//		yield return new WaitForSeconds (0.5f);
	//		rotationSnap = true;
	//		
	//	}
		
		#region Public's
		
		public void setActived (bool b)
		{
			actived = b;
			if(selectedObj != null && b)
				setSelected(/*selectedObj*/null);
		}

	//	public bool isInteracting()
	//	{
	//		return UIActive || dragStarted || longClickStarted || rotate;	
	//	}
		
		public bool isDragging()
		{
			return dragStarted;	
		}

		public Vector3 getSelectedPos()
		{
			return selectedObj.transform.position;	
		}
		
		public GameObject getSelected()
		{
			return selectedObj;	
		}
		
		public bool isSelectionsActives()
		{
			return m_selectAll || (m_selectMultiple &&	m_ObjsSelection.Count>0);
		}
		
		private void Unselect()
		{
			setSelected(null);
		}
		
		public void setSelected(GameObject g)
		{
			setSelected(g,false);

		}
		
		public void setSelected(GameObject g,bool justShow)
		{
			if(g == null) // plus d'objets sélectionné
			{
				
				if(selectedObj != null)
				{
					selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
					selectedObj.GetComponent<ObjBehav>().isActive = false;
					
				}
				selectedObj = null;
				if(!justShow)
				{
					mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
					mainScene.GetComponent<GUIMenuLeft>().canDisplay(true);
					mainScene.GetComponent<GUIMenuRight>().setVisibility(false);
					mainScene.GetComponent<GUIMenuRight>().canDisplay(true);
					
	//				hud.hideShowUD(false);
					hud.ActivateLineDisplay(false);
				}
				setColor(vert);//ADDED TO TEST
				v2CanSelectNext = false;
			}
			else 	//nouvel objet sélectionné
			{

				if(selectedObj != null && selectedObj != g)
				{
					selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
					selectedObj.GetComponent<ObjBehav>().isActive = false;
					setColor(vert);//ADDED TO TEST
				}
				
	            selectedObj = g;
				
				if(selectedObj.transform.GetComponent<ObjData>())
					m_selectedOSLibObj = selectedObj.transform.GetComponent<ObjData>().GetObjectModel();
				else
					m_selectedOSLibObj = null;//cas Avatars
					
	            selectedObj.GetComponent<ObjBehav>().isActive = true;
	            selectedObj.GetComponent<ObjBehav>().setAsSelected(selectedObj);
				
				if(!justShow && selectedObj.name!="_avatar")
				{
		            mainScene.GetComponent<GUIMenuInteraction>().setVisibility(true);
					mainScene.GetComponent<GUIMenuInteraction>().setLockability(selectedObj.GetComponent<ObjBehav>().amILocked());
					mainScene.GetComponent<GUIMenuInteraction>().SetCustomizability(selectedObj.GetComponent<ObjData>().IsCustomizable());
					if(selectedObj.GetComponent<ObjBehav>().amILocked())
					{
						setColor(orange);	//ADDED TO TEST
					}
		            mainScene.GetComponent<GUIMenuLeft>().canDisplay(false);
		            mainScene.GetComponent<GUIMenuRight>().canDisplay(false);
	//	            hud.hideShowUD(true);
					hud.ActivateLineDisplay(true,true);
	            				
					if(selectedObj != null)
					{
						if(selectedObj.GetComponent<ObjBehav>().amILocked())
						{
							highlight.SetColor("_Color", orange);
						}
						else
						{
							highlight.SetColor("_Color", vert);
						}
					}
											
				}
				else if(selectedObj.name == "_avatar")
				{
					mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
					mainScene.GetComponent<GUIMenuRight>().canDisplay(true);
					mainScene.GetComponent<GUIMenuRight>().ShowAvatarSettings();
					hud.ActivateLineDisplay(true,true);
	        	}
	    }
	    }

		public void deselect()
		{
			if(selectedObj != null)
			{
				selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
				selectedObj.GetComponent<Rigidbody>().isKinematic = true;

				setSelected(null);
	//			UIActive = false;
				dragStarted = false;
				raycastLayer = -1;
			}

		}
		
		public void deleteObj()
		{
			if (selectedObj != null)
			{
				highlight.SetColor("_Color", vert);
				UIActive = false;
	            // Suppression du système d'ombre pour iOS (TODO ajouter un if(iOS)...)
	            OSLibObject libObj = selectedObj.transform.GetComponent<ObjData>().GetObjectModel();
	            if(libObj.GetObjectType() == "pool" || libObj.IsMode2D())
	                m_mode2D.RemoveObj(selectedObj.transform);
	       //     if(_iosShadows) _iosShadows.GetComponent<IosShadowManager>().DeleteShadow(selectedObj, false);
	            selectedObj.transform.parent = null;
				Destroy(selectedObj);
	//			selectedObj = null;
				setSelected(null);
				Resources.UnloadUnusedAssets();
			}
		}
			
		public void reinitiObj()
		{
			if (selectedObj != null)
			{
				OSLibObject libObj = selectedObj.GetComponent<ObjData>().GetObjectModel();
				ObjData objData = selectedObj.GetComponent<ObjData>();
				
				deleteObj();
				mainNode.GetComponent<ObjInstanciation>().addObj(libObj, objData.selFab, objData.selTyp, objData.selObj);
			}
		}
		
		public void swapObj(GameObject go)//OBSOLETE
		{
			if (selectedObj != null)
			{
				UIActive = false;
	            
	            // Supprimer l'ombre iOS de l'objet remplacé, et créer celle du nouvel objet.  TODO : ajouter un "if(iOS) ..."
	            
	           /* if(_iosShadows)
	            {
	                _iosShadows.GetComponent<IosShadowManager>().DeleteShadow(selectedObj, true);
	                _iosShadows.GetComponent<IosShadowManager>().AddShadow(go);
	            }*/
	            
				Destroy (selectedObj);
				selectedObj = go;
				
				if(selectedObj != null)
				{
					if(selectedObj.GetComponent<ObjBehav>().amILocked())
					{
						highlight.SetColor("_Color", orange);
					}
					else
					{
						highlight.SetColor("_Color", vert);
					}
				}
			}
		} //OBSOLETE
		
		public void swapObj(GameObject go, GameObject old)
		{
			if (selectedObj != null)
			{
				UIActive = false;
			
				Destroy (old);
				selectedObj = go;

				if(selectedObj != null)
				{
					if(selectedObj.GetComponent<ObjBehav>().amILocked())
					{
						highlight.SetColor("_Color", orange);
					}
					else
					{
						highlight.SetColor("_Color", vert);
					}
				}
			}
			else
			{
				Destroy (old);
			}
		}
		
		public void copyObj()
		{
			StartCoroutine(Copy());
		}

	    //  2013-10 la copie passe par là (swap & instanciation normale -> objInstanciation)
		IEnumerator Copy()
		{
			selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
			Quaternion oldLocalRotation = selectedObj.transform.localRotation;
			OSLibObject libObj = selectedObj.GetComponent<ObjData>().GetObjectModel();
			OSLib objLib = libObj.GetLibrary ();
			ArrayList monBehaviourList = new ArrayList();
			IDictionary confGeneric = selectedObj.GetComponent<ObjData>().getConfiguration();
			
			foreach(Component cp in selectedObj.GetComponents<MonoBehaviour>())
			{
				if(cp.GetType().GetInterface("Function_OS3D")!= null)
				{
					monBehaviourList.Add(cp);
				}
			}

			WWW www = WWW.LoadFromCacheOrDownload (objLib.GetAssetBundlePath (), objLib.GetVersion ());
			yield return www;
			AssetBundle assetBundle = www.assetBundle;
			Object original = assetBundle.LoadAsset (libObj.GetModel ().GetPath (), typeof(GameObject));
			
			GameObject copied = (GameObject) Instantiate (original);
				
			copied.name = copied.name + ++mainNode.GetComponent<ObjInstanciation>().iobjectNumber;
			copied.transform.parent = mainNode.transform;
			copied.transform.localRotation = oldLocalRotation;	
			copied.transform.localScale = selectedObj.transform.localScale;


	        // -- Placement nouvel objet --
	        Vector3 pos = selectedObj.transform.localPosition;
	        SceneModel.Hlips camData = Montage.sm.getCamData();
	        if(camData.m_l >= -45f && camData.m_l < 45f)
	        {
	            pos.z += selectedObj.GetComponent<Collider>().bounds.size.z;
	            if(pos.x < 0)
	                pos.x += selectedObj.GetComponent<Collider>().bounds.size.x;
	            else
	                pos.x -= selectedObj.GetComponent<Collider>().bounds.size.x;
	        }
	        else if(camData.m_l >= 45f && camData.m_l < 135f)
	        {
	            pos.x += selectedObj.GetComponent<Collider>().bounds.size.z;
	            if(pos.z < 0)
	                pos.z += selectedObj.GetComponent<Collider>().bounds.size.x;
	            else
	                pos.z -= selectedObj.GetComponent<Collider>().bounds.size.x;
	        }
	        else if((camData.m_l >= 135f && camData.m_l <= 180f) || (camData.m_l >= -180f && camData.m_l < -135f))
	        {
	            pos.z -= selectedObj.GetComponent<Collider>().bounds.size.z;
	            if(pos.x < 0)
	                pos.x += selectedObj.GetComponent<Collider>().bounds.size.x;
	            else
	                pos.x -= selectedObj.GetComponent<Collider>().bounds.size.x;
	        }
	        else if(camData.m_l >= -135 && camData.m_l < -45f)
	        {
	            pos.x -= selectedObj.GetComponent<Collider>().bounds.size.z;
	            if(pos.z < 0)
	                pos.z += selectedObj.GetComponent<Collider>().bounds.size.x;
	            else
	                pos.z -= selectedObj.GetComponent<Collider>().bounds.size.x;
	        }
	        copied.transform.localPosition = pos;  // fin placement nouvel objet

	        copied.gameObject.layer = 9;
			
			copied.gameObject.AddComponent <ObjBehav>();
			copied.gameObject.AddComponent<ApplyShader>();
			
			ObjData data = (ObjData) copied.gameObject.AddComponent <ObjData>();
			yield return new WaitForEndOfFrame(); // attend la fin du start de ObjBehav
			data.SetObjectModel (libObj, assetBundle);
			
			assetBundle.Unload (false);
			float y = copied.gameObject.GetComponent<ObjBehav>().init(); // > met a la bonne position en y	
			
			copied.gameObject.GetComponent<ObjBehav>().SetHeighOff7(pos.y);
				
			string pref = copied.GetComponent<ObjData> ().GetObjectModel ().GetObjectType ();
	       /* if(pref != "piscines" && pref != "spas")        // Pas d'ombres pour les piscines (très moches)
	        {
				
				if(_iosShadows)
					_iosShadows.GetComponent<IosShadowManager>().AddShadow(copied.gameObject);
	            else
					Debug.LogWarning("Objet \"iosShadows\" introuvable. Les ombres pour iOS risquent de ne pas fonctionner.");
				
	        }*/
			int selectedFunc = 0;
			foreach(Component cp in copied.GetComponents<MonoBehaviour>())
			{
				if(cp.GetType().GetInterface("Function_OS3D")!= null)
				{
					//ArrayList conf = ((Function_OS3D) selectedObj.GetComponent(cp.GetType().ToString())).getConfig();
					ArrayList conf = ((Function_OS3D) monBehaviourList[selectedFunc]).getConfig();
					((Function_OS3D) cp).setConfig(conf);
					selectedFunc++;
				}
			}

	        // -- Ajout de l'objet au mode2D si besoin --
	        OSLibObject obj = copied.transform.GetComponent<ObjData>().GetObjectModel();
				if(obj.GetObjectType() == "pool" || obj.GetObjectType() == "dynamicShelter" ||  obj.IsMode2D())
	            m_mode2D.AddObj(copied.transform);

			yield return StartCoroutine(data.loadConfIE(confGeneric));
			
			mainScene.GetComponent<GUIMenuLeft>().updateSceneObj();
			setSelected(copied);
		}
		
		public void validateObj()
		{
			if (selectedObj != null)
			{
				UIActive = false;
				selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
				selectedObj = null;

			}

		}
		
		public GameObject configuringObj(GameObject g) 	//configuring(null) pour activer configuring (retourne l'object courant)
		{												//configuring(obj) pour désactiver configuring (obj = objet courant)
			GameObject output = null;
			if (selectedObj != null)
			{
				selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
				output = selectedObj;
				selectedObj = null;
			}
			else if(g != null)
				{
				setSelected(g);
				selectedObj.GetComponent<ObjBehav>().setAsSelected(selectedObj);
				output = null;
			}
			return output;
		}
		
		public void swap(bool b)
		{
			UIActive = !b;
			isSwaping = b;
				
	//		if(selectedObj==null)
	//			return;
	//		if(b)
	//			selectedObj.GetComponent<ObjBehav>().setAsUnselected(selectedObj);
		}

	    public void SelectAll()
	    {
	        m_selectAll = true;

	        // Masquer les menus
	        mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
	        mainScene.GetComponent<GUIMenuLeft>().canDisplay(false);
	        mainScene.GetComponent<GUIMenuLeft>().setVisibility(false);
	        mainScene.GetComponent<GUIMenuRight>().canDisplay(false);

	        GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().SelectAll();
	    }
		
	    public void UnselectAll()
	    {
	        m_selectAll = false;

	        // Afficher les menus
	        mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
	//        mainScene.GetComponent<GUIMenuLeft>().setVisibility(true);
	        mainScene.GetComponent<GUIMenuLeft>().canDisplay(true);
	        mainScene.GetComponent<GUIMenuRight>().canDisplay(true);		
	        GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().UnselectAll();

	    }
		
		public bool lockIt()
		{
			if(selectedObj != null)
			{
				bool isLock = selectedObj.GetComponent<ObjBehav>().Lock();
				
				if(isLock)
				{
					highlight.SetColor("_Color", orange);
				}
				else
				{
					
					highlight.SetColor("_Color", vert);
				}

				return isLock;
			}
				
			return false;
		}
		
		public bool isLocked()
		{
			/*if(selectedObj)
			{*/
				return selectedObj.GetComponent<ObjBehav>().amILocked();
			/*}
				
			return false;*/
		}
		
		#endregion
		
		public void setColor(Color c)//ADDED TO TEST
		{
			//highlight.color = c;
		}
		
		void OnApplicationQuit()
		{
			highlight.SetColor("_Color", vert);
		}

		void OnPostRender()
		{

			if(isSwaping || _highLightOverride)
				return;
			if(m_selectMultiple && m_ObjsSelection.Count>0)
			{
				highlight.SetPass(0);
				foreach(GameObject g in m_ObjsSelection)
				{
					foreach(MeshFilter m in g.GetComponentsInChildren<MeshFilter>())
					{
						if((m.tag !="noRed")&&(m.tag !="noPos")  && (m.gameObject.layer!=14) && m.GetComponent<Renderer>().enabled)
							Graphics.DrawMeshNow(m.sharedMesh,m.transform.localToWorldMatrix);
					}
				}
			}
			else if(m_selectAll)
			{
				highlight.SetPass(0);
				foreach(Transform t in mainNode.transform)
				{
					foreach(MeshFilter m in t.GetComponentsInChildren<MeshFilter>())
					{
						if((m.tag !="noRed")&&(m.tag !="noPos")  && (m.gameObject.layer!=14) && m.GetComponent<Renderer>().enabled)
							Graphics.DrawMeshNow(m.sharedMesh,m.transform.localToWorldMatrix);
					}
				}
			}
			else
			{
				if(selectedObj == null)
					return;
											
				if(selectedObj.name != "_avatar")	
				{
					highlight.SetPass(0);
				}
					
				foreach(MeshFilter m in selectedObj.GetComponentsInChildren<MeshFilter>())
				{
					if(m.GetComponent<Renderer>() && m.sharedMesh != null)
						if(((m.tag !="noRed")&&(m.tag !="noPos") && (m.gameObject.layer!=14)) && m.GetComponent<Renderer>().enabled)
							Graphics.DrawMeshNow(m.sharedMesh,m.transform.localToWorldMatrix);
				}
				
	#if UNITY_STANDALONE
	/*			foreach(SkinnedMeshRenderer m in selectedObj.GetComponentsInChildren<SkinnedMeshRenderer>())
				{
					if((m.tag !="noRed")&&(m.tag !="noPos")  && (m.gameObject.layer!=14) && m.renderer.enabled)
					{
						Mesh bm = new Mesh();
						m.BakeMesh(bm);
						Graphics.DrawMeshNow(bm,m.transform.position,m.transform.rotation);
					}
				}*/
	#endif
			}
		}
			
	} // class ObjInteraction
