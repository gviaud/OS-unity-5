using UnityEngine;
using System.Collections;

using Pointcube.Global;

public class Avatar : MonoBehaviour
{
    // -- Réf. Scène --
	public GameObject m_mainCam;

	private bool _autoDisplay = false;
	private bool _forceDisplay = true;
	private bool _enable = true;
	private static bool _locked=true;

	public static bool Locked {
		get {
			return _locked;
		}
		set {
			_locked = value;
		}
		
	}	
	private GameObject barre;
	private GameObject vignette;
	private GameObject avatar;
	private GameObject _camPivot;
	
	private MonoBehaviour _guistart;
//	private MonoBehaviour _mode2D;

    private static readonly string DEBUGTAG = "Avatar : ";
	
	private ArrayList _children;

	//-----------------------------------------------------
	void Awake()
	{
		UsefullEvents.NewMontage += ReInit;
		UsefullEvents.HideGUIforScreenshot += TempHide;
	}
	
	void Start ()
	{
		_children = new ArrayList();
        if(m_mainCam == null) Debug.LogError(DEBUGTAG+"MainCam "+PC.MISSING_REF);
		//barre = transform.GetChild(0).gameObject;
//		vignette = barre.transform.GetChild(0).gameObject;
		//avatar = transform.GetChild(1).gameObject;
		
		foreach(Transform child in transform)
		{
			if (child.name.CompareTo("barre")==0)
				barre = child.gameObject;
			else if (child.name.CompareTo("boy")==0)
				avatar = child.gameObject;
			else if (child.name.StartsWith("z"))
			{
				foreach(Transform subChild in child)
				{
					_children.Add(subChild.gameObject);
				}
			}
		}

		GameObject gameobjet =  GameObject.Find("MainScene");
		if(gameobjet!=null)
			_guistart = (MonoBehaviour) gameobjet.transform.GetComponent("GUIStart");
		
		_camPivot = GameObject.Find("camPivot");
		
		Vector3 v3rotation = transform.eulerAngles;
		v3rotation.y = _camPivot.transform.eulerAngles.y;
		transform.rotation = Quaternion.Euler(v3rotation);
		
		UpdateState();
	}
	
	void Update()
	{
		float s = 20/transform.localScale.x;

		if(_guistart!=null)
		{
			if(_guistart.enabled)
			{
				if(_enable)
				{
					barre.GetComponent<Renderer>().enabled = false;
	//				vignette.renderer.enabled = false;
					avatar.GetComponent<Renderer>().enabled = false;
					
					_enable = false;
					GetComponent<ObjBehav>().enabled = false;
					GetComponent<BoxCollider>().enabled = false;
					foreach(GameObject child in _children)
					{
						child.GetComponent<Renderer>().enabled = false;
					}
				}
			}
			
			Vector3 v3rotation = transform.eulerAngles;
			v3rotation.y = _camPivot.transform.eulerAngles.y;
			transform.rotation = Quaternion.Euler(v3rotation);
			
			float fscale = _camPivot.GetComponent<SceneControl>().GetS() * 10.0f;
			transform.localScale = new Vector3(fscale, fscale, fscale);
			
			//GameObject.Find("grid").transform.localScale = new Vector3(fscale * 10.0f, fscale * 10.0f, fscale * 10.0f);
			//GameObject.Find ("grid").transform.rotation = transform.rotation;
			//_camPivot.GetComponent<SceneControl>().initL(transform.rotation.eulerAngles.y);
		}

#if UNITY_ANDROID && !UNITY_EDITOR
			barre.renderer.enabled=barre.renderer.enabled;
			avatar.renderer.enabled=avatar.renderer.enabled;
#endif
	}
	
	public void SetAutoDisplay(bool b)
	{
		_autoDisplay = b;
		UpdateState();
	}
	
	public bool SetForceDisplay()
	{
		_forceDisplay = !_forceDisplay;
		UpdateState();
		return _forceDisplay;
	}
	
	public bool SetForceDisplay(bool forcedisplay)
	{
		_forceDisplay = forcedisplay;
		UpdateState();
		return _forceDisplay;
	}
	
	private void UpdateState()
	{
		if((_autoDisplay||_forceDisplay))
		{
			if(!_enable)
			{
				barre.GetComponent<Renderer>().enabled = true;
//				vignette.renderer.enabled = true;

				if(usefullData._edition == usefullData.OSedition.Full)
				{
					avatar.GetComponent<Renderer>().enabled = true;
				}

				_enable = true;
				GetComponent<ObjBehav>().enabled = true;
				
				foreach(BoxCollider bc in GetComponents<BoxCollider>())
				{
					bc.enabled = true;
				}
				
				foreach(GameObject child in _children)
				{
					child.GetComponent<Renderer>().enabled = true;
				}
			}
		}
		else
		{
			if(_enable)
			{
				barre.GetComponent<Renderer>().enabled = false;
//				vignette.renderer.enabled = false;
				avatar.GetComponent<Renderer>().enabled = false;
//				m_mainCam.GetComponent<ObjInteraction>().setSelected(null); // StackOverflowException quand on décommente la ligne
				_enable = false;
				GetComponent<ObjBehav>().enabled = false;
				
				foreach(BoxCollider bc in GetComponents<BoxCollider>())
				{
					bc.enabled = false;
				}
				
				foreach(GameObject child in _children)
				{
					child.GetComponent<Renderer>().enabled = false;
				}
			}
		}
	}
	
	private void ReInit()
	{
		if(transform!=null)
		{
			transform.position = Vector3.zero;
			Quaternion q = new Quaternion(0,0,0,0);
			transform.rotation = q;
			
			SetForceDisplay(true);
		}
	}
	
	void OnDestroy()
	{
		UsefullEvents.NewMontage -= ReInit;
        UsefullEvents.HideGUIforScreenshot -= TempHide;
	}
	
	public bool IsForceDisplayed()
	{
		return _forceDisplay;
	}
	
	private void TempHide(bool hide)
	{
		barre.SetActive(!hide);
		avatar.SetActive(!hide);
		foreach(GameObject child in _children)
		{
			child.SetActive(!hide);
		}
	}
	
	public ArrayList getChildList()
	{
		return _children;
	}
	
}
