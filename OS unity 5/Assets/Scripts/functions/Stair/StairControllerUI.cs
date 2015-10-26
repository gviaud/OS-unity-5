using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Pointcube.Global;

public class StairControllerUI : MonoBehaviour, FunctionUI_OS3D, GUIInterface
{
	StairController _controller;
	
	GUIItemV2 _root;
	GUIItemV2 _stairMenu;
	GUIUpperList _stairModelList;
	
	int _stairIndex = -1;
	int _gizmoIndex = -1;
	
	List<StairGizmo> _virtualGizmo = new List<StairGizmo> ();
	List<Transform> _3DGizmos = new List<Transform> ();
	
	protected Transform _removeGizmo;
	
	private bool _tempHide = false;	
	//indique si le doigts a bouge ou si simple clique
	private bool _hasMoved = false;
	
	Texture2D [] _thumbnails;
	
	bool _modelChanged = false;
	
	GameObject _selectedObject;
	
	public GUISkin skin;
	
	public Transform GizMould;
	public Transform RemoveGizMould;
	
	// Use this for initialization
	void Start () 
	{
		_stairMenu = new GUIItemV2 (0, 0, "Stair", "sousMenuOn", "sousMenuOff", this);
		_stairModelList = new GUIUpperList (1, 0, TextManager.GetText("stair"), "sousMenuOn", "sousMenuOff", this);
			
		_stairMenu.addSubItem (_stairModelList);
		enabled = false;
	}
	
	void OnEnable()
	{		
		_root = new GUIItemV2(-1, -1, "Root", "", "", this);
		_root.addSubItem (_stairMenu);
	}
	
	void Validate()
	{		
		DestroyGizmos ();
		_controller = null;
	
		GetComponent<GUIMenuInteraction> ().unConfigure();
		GetComponent<GUIMenuInteraction> ().setVisibility (false);
		
		Camera.main.GetComponent<ObjInteraction>().configuringObj(null);
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,false);
		Camera.main.GetComponent<GuiTextureClip>().enabled = true;
		
		enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{			
		if (_modelChanged)
		{
			GameObject currentSelected = Camera.main.GetComponent<ObjInteraction>().getSelected ();
			if (currentSelected != null && _selectedObject != currentSelected)
			{
				StairController controller = currentSelected.transform.GetComponent<StairController> ();
				if (controller != null)
				{
					Camera.main.GetComponent<ObjInteraction>().setSelected(null,true);
					Camera.main.GetComponent<ObjInteraction>().setActived(false);
					
					controller.Init ();
					controller.SetGizmoIndex (_gizmoIndex);
					controller.SetStairIndex (_stairIndex);
					
					SetStairController (currentSelected.transform);
					_selectedObject = null;
					_modelChanged = false;
					
					if (_stairIndex >= 0)
					{
						StartCoroutine (InstantiateGizmos ());
					}
				}
			}
		}
		else if (_controller != null)
		{
            if(PC.In.Click1Down())
            {
                Rect rect1 = new Rect(Screen.width/2-200,0,400,400);
                Rect rect2 = new Rect(0,0,Screen.width,160);

                Ray ray = Camera.main.ScreenPointToRay(PC.In.GetCursorPos());

                RaycastHit hit;
			    if (Physics.Raycast (ray, out hit)) 
				{
					bool noHit = true;
					foreach (Transform t in _3DGizmos)
					{
						if (hit.transform == t) 
						{		
							noHit = false;
							int index = int.Parse (hit.transform.gameObject.name);
							if (_gizmoIndex != index)
							{
								_gizmoIndex = index;
								_selectedObject = _controller.gameObject;//Camera.mainCamera.GetComponent<ObjInteraction>().getSelected ();
								DestroyGizmos ();
								_modelChanged = true;
									
								if (_stairIndex < 0)
								{
									_stairIndex = 0;	
								}
									
								StartCoroutine (_controller.SwapPool (_gizmoIndex, _stairIndex));
							}
							
							break;
						}
					}
						
					if (hit.transform == _removeGizmo)
					{
						noHit = false;
						_stairIndex = -1;
						_gizmoIndex = -1;
						_selectedObject = _controller.gameObject;
						DestroyGizmos ();
						_modelChanged = true;
						StartCoroutine (_controller.SwapPool (_gizmoIndex, _stairIndex));
					}
					
					if (noHit)
					{
						if(/*_stairModelList.bgThumbRect.Contains(PC.In.GetCursorPosInvY()) && */!PC.In.CursorOnUI(_stairModelList.bgThumbRect))
                        {
                            _tempHide = false;
                            Validate();
                        }

//#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_EDITOR
//						Vector3 mousePos = new Vector3 (Input.mousePosition.x,
//								                        Screen.height - Input.mousePosition.y,
//								                        Input.mousePosition.z);
//						
//							
//						if(new Rect(Screen.width/2-200,0,400,400).Contains(mousePos) && !new Rect(0,0,Screen.width,160).Contains(mousePos))
//						{
//							_tempHide = false;
//							Validate();
//						}
//#endif

//#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//						if (Input.touchCount == 1)
//						{
//							Touch firstTouch = Input.touches[0];
//							Vector2 touchPos = new Vector2 (firstTouch.position.x, Screen.height - firstTouch.position.y);
//							if(new Rect(Screen.width/2-200,0,400,400).Contains(touchPos) && !new Rect(0,0,Screen.width,160).Contains(touchPos))
//							{
//								switch (firstTouch.phase)
//								{
//									case TouchPhase.Ended:
//									{
//										if (!_hasMoved)
//											Validate();
//										
//										_hasMoved = false;
//										break;
//									}
//								}
//							}
//						}
//#endif
					}
		    	}
				else
				{
					if(/*_stairModelList.bgThumbRect.Contains(PC.In.GetCursorPosInvY()) && */!PC.In.CursorOnUI(_stairModelList.bgThumbRect))
                    {
                        _tempHide = false;
                        Validate();
                    }
//#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_EDITOR
//					Vector3 mousePos = new Vector3 (Input.mousePosition.x,
//							                        Screen.height - Input.mousePosition.y,
//							                        Input.mousePosition.z);
//						
//					if(new Rect(Screen.width/2-200,0,400,400).Contains(mousePos) && !new Rect(0,0,Screen.width,160).Contains(mousePos))
//					{
//						_tempHide = false;
//						Validate();
//					}
//#endif
//
//#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//					if (Input.touchCount == 1)
//					{
//						Touch firstTouch = Input.touches[0];
//						Vector2 touchPos = new Vector2 (firstTouch.position.x, Screen.height - firstTouch.position.y);
//						if(new Rect(Screen.width/2-200,0,400,400).Contains(touchPos) && !new Rect(0,0,Screen.width,160).Contains(touchPos))
//						{
//							switch (firstTouch.phase)
//							{
//								case TouchPhase.Ended:
//								{
//									if (!_hasMoved)
//										Validate();
//										
//									_hasMoved = false;
//									break;
//								}
//							}
//						}
//					}
//#endif
				}
			}
//#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//			else if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Moved)
//			{
//				_hasMoved = true;
//			}
//#endif
		}
	}
		
	protected IEnumerator InstantiateGizmos ()
	{	
		List<Transform> availableTransform = new List<Transform> ();
		yield return StartCoroutine (_controller.FindAvailableGizmos ());
		
		int gizmoCounter = 0;
		foreach (StairGizmo originalGizmo in _controller.GetAvailableGizmos ())
		{
//			string indexString = originalGizmo.name[5].ToString ();
//			int currentIndex = (int.Parse (indexString) - 1);
				
			if (originalGizmo.index != _gizmoIndex || _stairIndex < 0)
			{
				Transform gizmo = Instantiate(GizMould, 
						                      Vector3.zero, 
								              Quaternion.identity) as Transform;
					
				//GameObject gizmo = GameObject.CreatePrimitive (PrimitiveType.Cube);
				gizmo.parent = _controller.transform;
				gizmo.localPosition = originalGizmo.position;
				gizmo.localRotation = originalGizmo.rotation;
				//gizmo.transform.localScale = Vector3.one * 1;
				gizmo.gameObject.layer = 9;
					
//				Rigidbody rb = gizmo.AddComponent <Rigidbody> ();
//				rb.useGravity = false;
//				rb.isKinematic = true;
				
				gizmo.gameObject.name = originalGizmo.index.ToString ();
				_3DGizmos.Add (gizmo.transform);
			}
			else
			{
				_removeGizmo = Instantiate(RemoveGizMould, 
						                   Vector3.zero, 
								           Quaternion.identity) as Transform;
					
				//GameObject gizmo = GameObject.CreatePrimitive (PrimitiveType.Cube);
				_removeGizmo.parent = _controller.transform;
				_removeGizmo.localPosition = originalGizmo.position;
				_removeGizmo.localRotation = originalGizmo.rotation;
//				_removeGizmo.RotateAround (Vector3.up, 90);
				//gizmo.transform.localScale = Vector3.one * 1;
				_removeGizmo.gameObject.layer = 9;
			}
				
			++gizmoCounter;
		}
	}
		
	protected void DestroyGizmos ()
	{
		foreach (Transform t in _3DGizmos)
		{
			t.parent = null;
			Destroy (t.gameObject);
		}
			
		if (_removeGizmo != null)
		{
			Destroy (_removeGizmo.gameObject);
		}
			
		_3DGizmos.Clear ();
	}
	
	protected void OnGUI ()
	{
		// stair model UI
		if (_stairModelList != null)
		{
			GUISkin bkup = GUI.skin;
			GUI.skin = skin;
				
			_stairModelList.display ();
				
			GUI.skin = bkup;
		}
	}
		
	protected void ui2fcn ()
	{	
		DestroyGizmos ();
		
		if (_gizmoIndex < 0)
		{
			if (_stairIndex >= 0)
			{
				StartCoroutine (InstantiateGizmos ());
			}
		}
		else
		{
			_selectedObject = _controller.gameObject;	
			_modelChanged = true;
			StartCoroutine (_controller.SwapPool (_gizmoIndex, _stairIndex));
		}
	}
	
	public void DoActionUI(GameObject gameobject)
	{
		GameObject.Find("MainScene").GetComponent<HelpPanel>().set2ndHelpTxt ("");
			
		GameObject.Find("MainScene").GetComponent<StairControllerUI>().enabled = true;
		GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);			
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;
		Camera.main.GetComponent<GuiTextureClip>().enabled = false;
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,true);
		Camera.main.GetComponent<ObjInteraction>().setActived(false);
			
		SetStairController (gameobject.transform);
		
		if (_stairIndex >= 0)
		{
			StartCoroutine (InstantiateGizmos ());
		}
							
		ObjData obj = gameobject.transform.GetComponent<ObjData> ();
		OSLibStairs stairs = obj.GetObjectModel ().GetLibrary ().GetStairsList ()[0];
	}
		
	protected void SetStairController (Transform pool)
	{
		_controller = pool.GetComponent<StairController>();
			
		if (_controller != null)
		{		
			_stairIndex = _controller.GetStairIndex ();
			_gizmoIndex = _controller.GetGizmoIndex ();
				
			if (_controller.GetGizmoList ().Count > 0)
			{
				_virtualGizmo = _controller.GetGizmoList ();
			}
				
			//if (_thumbnails == null)
				SetThumbnails (_controller.GetStairList ());

				
//			if (_stairIndex < 0)
//			{
//				_stairModelList.setUiId (_controller.GetStairList ().GetStairList ().Count);
//			}
//			else
//			{
				_stairModelList.setUiId (_stairIndex);
//			}
		}
	}
		
	public void updateGUI (GUIItemV2 itm,int val,bool reset)
	{
		switch (itm.getDepth())
		{
			case 0:
				if(reset)
				{
					_stairIndex = -1;
					if(itm.getSelectedItem()!=null && itm.getSelectedItem().getSelectedItem()!=null)
						itm.getSelectedItem().resetSelected();
					if(itm.getSelectedItem()!=null)
						itm.resetSelected();
				}
				break;
				
			case 1:
				if(reset)
				{
					_stairIndex = -1;
					if(itm.getSelectedItem()!=null)
						itm.resetSelected();
				}
				break;
				
			case 2:
				_stairIndex = val;
//				if (_stairIndex >= _thumbnails.Length - 1)
//				{
//					_stairIndex = -1;	
//				}
				
				_controller.SetStairIndex (_stairIndex);
				break;
		}
			
		ui2fcn ();
	}
	
	public void setVisibility (bool b)
	{
		
	}
	
	public void canDisplay (bool b)
	{
		
	}
	
	public bool isOnUI ()
	{
		return false;
	}
	
	public bool isVisible()
	{
		return false;
	}
	
	public void CreateGui ()
	{
		
	}
		
	public StairController GetController ()
	{
		return _controller;
	}
		
	protected void SetThumbnails (OSLibStairs stairs)	
	{
		_thumbnails = new Texture2D [stairs.GetStairList ().Count/* + 1*/];
		int textureCounter = 0;
			
		foreach (OSLibStair stair in stairs.GetStairList ())
		{
			Texture2D texture = stair.GetThumbnail ();
			texture.name = stair.GetDefaultText ();
			_thumbnails[textureCounter++] = texture;
		}
		
//		Texture2D noStair = Resources.Load("thumbnails/noThumbs",typeof(Texture2D)) as Texture2D;
//		noStair.name = "";
//		_thumbnails[stairs.GetStairList ().Count] = noStair;
		_stairModelList = new GUIUpperList (1, 0, TextManager.GetText("stair"), "sousMenuOn", "sousMenuOff", this);
		_stairModelList.setImgContent (_thumbnails);
	}
}
