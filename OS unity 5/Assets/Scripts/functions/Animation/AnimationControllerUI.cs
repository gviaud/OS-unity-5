using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationControllerUI : MonoBehaviour, FunctionUI_OS3D {
	
	public const float EPSILON = 0.01f;
		
	float vSliderValue=0.0f;
	
	private AnimationController _controller;
	
	List<AnimationState> _anims = new List<AnimationState>();
	
	private bool _tempHide = false;	
	//indique si le doigts a bouge ou si simple clique
	private bool _hasMoved = false;
	
	private float _uiOff7;
	public GUISkin skinPlage;

	// Use this for initialization
	void Start () 
	{
//		guiZone = new Rect(Screen.width/2-750/2,0,750,50);
		_uiOff7 = Screen.width-290;
		enabled = false;
	}
	
	void OnGUI () {		
		GUISkin bkup = GUI.skin;
		GUI.skin = skinPlage;
		GUI.Box (new Rect(0,0,80,Screen.height),"","BackgroundLarge");
		vSliderValue = GUI.VerticalSlider(new Rect(0,84,142,600),vSliderValue, 0.0F, 1.0F);
		GUI.skin = bkup;
	}	
	
	void Update ()
	{
		
		if((Application.platform == RuntimePlatform.WindowsEditor)
			||(Application.platform == RuntimePlatform.WindowsPlayer)
			||(Application.platform == RuntimePlatform.OSXEditor)
			||(Application.platform == RuntimePlatform.OSXPlayer))
		{	
			
			if (Input.GetMouseButtonDown(0))
			{
				Vector3 mousePos = Input.mousePosition;
				if(!new Rect(_uiOff7, 0, 300, Screen.height).Contains(mousePos) && !new Rect(0,0,80,Screen.height).Contains(mousePos))
				{
					_tempHide = false;
					_controller.Stop();
					Validate();
				}
			}
			else
			{
			//	if (Input.GetMouseButtonDown(0))
				{
					_controller.UpdateValue(vSliderValue);
				}
			}
		}
		else
		{
			if (Input.touchCount <= 0)
			{
				
			}
			else
			{			
				Touch firstTouch = Input.touches[0];
				if(!new Rect(_uiOff7, 0, 300, Screen.height).Contains(firstTouch.position) && !new Rect(0,0,80,Screen.height).Contains(firstTouch.position))
				{
					switch (firstTouch.phase)
					{
						case TouchPhase.Began:
						{
							_tempHide = true;
							_hasMoved = false;
							break;
						}						
		//				case TouchPhase.Canceled:
		//				{
		//					_tempHide = false;
		//					break;
		//				}
						case TouchPhase.Ended:
						{
							_tempHide = false;
							if(_hasMoved==false)
								Validate();
							break;
						}
					}
				}
				else
				{
				//	if (Input.GetMouseButtonDown(0))
					{
						_controller.UpdateValue(vSliderValue);
					}
				}
			}
		}
		//animation Interface
		/*if(_tempHide)
		{
			if(_uiOff7 != Screen.width)
			{
				if(_uiOff7 < Screen.width-1)
				{
					_uiOff7 = Mathf.Lerp(_uiOff7,Screen.width,5*Time.deltaTime);
				}
				else
				{
					_uiOff7 = Screen.width;
				}
			}
		}
		else
		{
			//Screen.width-290
			if(_uiOff7 != Screen.width-290)
			{
				if(_uiOff7 > Screen.width-291)
				{
					_uiOff7 = Mathf.Lerp(_uiOff7,Screen.width-290,5*Time.deltaTime);
				}
				else
				{
					_uiOff7 = Screen.width-290;
				}
			}
		}*/
	}
		
	void Validate()
	{
		Camera.main.GetComponent<ObjInteraction>().configuringObj(null);
		_controller = null;
		GetComponent<GUIMenuInteraction> ().unConfigure();
		GetComponent<GUIMenuInteraction> ().setVisibility (false);
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,false);			
		enabled = false;
	}
	
	public void DoActionUI(GameObject gameobject)
	{
		GameObject.Find("MainScene").GetComponent<AnimationControllerUI>().enabled = true;
		SetPoolObject (gameobject.transform);
		GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);			
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;			
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,true);
		Camera.main.GetComponent<ObjInteraction>().setActived(false);		
	}
	
	public void SetPoolObject (Transform pool)
	{
		_controller = pool.GetComponent<AnimationController>();
		if(_controller!=null)
			vSliderValue = _controller.GetValue();
	}
		
	
}
