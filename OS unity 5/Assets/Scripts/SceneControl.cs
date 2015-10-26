using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pointcube.Global;

public class SceneControl : MonoBehaviour
{	
	float dist = 0;
	
	Camera cam;

    // -- Références scène --
	GameObject mainNode;
	GameObject mainScene;
	GameObject grid;
	GameObject avatar;
    public GameObject m_grassGround;    // Plans de l'outil gazon
    //public GameObject m_grassSky;       // Plans de l'outil gazon
    
    public float fscaleChange = 0.0f;
	
	Vector3 sceneCenter;
	
	float oldmousePosY = 0;
	float oldmousePosX = 0;
	
//	float oldScale;
	
	float startHeight;
	static public float m_avatar_curscale;
	static float m_lacet = 0;
	static float m_roulis = 0;
	static float m_inclinaison = 0;
	static public float m_hauteur = 0;
	static float m_pers = 60;
	//static float m_roulis = 0;
	
	//	ArrayList save = ne
	float m_bkupLacet = 0.0f;
	float m_bkupRoulis = 0.0f;
	float m_bkupInclinaison = 0.0f;
	float m_bkupHauteur = 0.0f;
	float m_bkupPers = 0.0f;
	float m_bkucurScale = 0.0f;
	
	private float gridControlSpeed = 1.5f;//0.5f;
	
	//Perspective
	float persHeight;
	Vector3 persTarget;

    // Paramètres pour l'échelle de la scène
	private float       m_curScale;           // Echelle courante (= echelle de base modifiée par les facteurs)
	private float       m_baseSclFactor;      // Facteur appliqué à la scène au lancement de l'application (le "8")
	
	private CameraFrustum _cameraFrustrum;
	
	private GameObject _iosShadows;

    private static readonly string DEBUGTAG = "SceneControl : ";

    //-----------------------------------------------------
    void Awake()
    {
		m_baseSclFactor = GameObject.Find("_avatar").transform.localScale.x;
        
		m_curScale = m_baseSclFactor;
    }

    //-----------------------------------------------------
	// Use this for initialization
	void Start()
	{
       // if(m_grassSky == null)      Debug.LogError(DEBUGTAG+"Grass Sky"+PC.MISSING_REF);
        if(m_grassGround == null)   Debug.LogError(DEBUGTAG+"Grass Ground"+PC.MISSING_REF);
		
		cam = Camera.main;
		startHeight = cam.transform.position.y;
		mainNode = GameObject.Find("MainNode");
		mainScene = GameObject.Find("MainScene");
		grid = GameObject.Find("grid");
		avatar = GameObject.Find("_avatar");
		_cameraFrustrum = GameObject.Find("mainCam").GetComponent<CameraFrustum>();
		
		_iosShadows = GameObject.Find("iosShadows");
        
		sceneCenter = grid.transform.position;
//		dist = Mathf.Abs(cam.transform.localPosition.z)/2;

		init();
	}
	
//	public void reinit()
//	{
//		dist = Mathf.Abs(cam.transform.localPosition.z)/2;
//		lacet = 0;
//		inclinaison = 0;
//	 	hauteur = 0;
//		pers = 60;
//		reinitScale();
//	}

    //-----------------------------------------------------
    // Initialiser / réinitialiser les réglages de la scène
	public void init()
	{
		transform.rotation = new Quaternion(0,0,0,0);
		dist = Mathf.Abs(cam.transform.localPosition.z)/2;
		m_lacet = 0;
		m_roulis = 0;
		updateLacet();
		m_inclinaison = 0;
		updateInclinaison();
	 	m_hauteur = 0;
		updateHeight();
		
		//perspective
		m_pers = 60;
		persHeight = calcPersHeight(_cameraFrustrum.GetFov());
		persChange();
		
//		persChange();
		reinitScale();
		saveToModel();
		saveOriginal();

	}
	
	//-----------------------------------------------------
    // Charge la dernière configuration de la scène enregistrée dans le modèle
	public void loadFromModel()
	{
		SceneModel.Hlips data = Montage.sm.getCamData();
			
		m_lacet = data.m_l;
		updateLacet();
		
		m_inclinaison = data.m_i;
		updateInclinaison();
		
		m_roulis = data.m_t;
		updateRoulis();
		
		m_hauteur = data.m_h;
		updateHeight();


		if(data.m_p != m_pers)
		{
			m_pers = data.m_p;
			persChange();
		}
		
		reinitScale();
		scaleScene((data.m_s/m_baseSclFactor)-1);
	}
	
	//-----------------------------------------------------
    // Undo
	public void UndoCam()
	{
		SceneModel.Hlips data = Montage.sm.UndoCamera();
		
		m_lacet = data.m_l;
		updateLacet();
		
		m_inclinaison = data.m_i;
		updateInclinaison();
		
		m_hauteur = data.m_h;
		updateHeight();
		
		if(data.m_p != m_pers)
		{
			m_pers = data.m_p;
			persChange();
		}
		reinitScale();
//		m_curScale = data.m_s;
		scaleScene((data.m_s/m_baseSclFactor)-1);
	}
	
	public void RedoCam()
	{
		SceneModel.Hlips data = Montage.sm.RedoCamera();
		
		m_lacet = data.m_l;
		updateLacet();
		
		m_inclinaison = data.m_i;
		updateInclinaison();
		
		m_hauteur = data.m_h;
		updateHeight();

		m_curScale = data.m_s;

		if(data.m_p != m_pers)
		{
			m_pers = data.m_p;
			persChange();
		}
		reinitScale();
//		m_curScale = data.m_s;
		scaleScene((data.m_s/m_baseSclFactor)-1);
	}
	
	public void saveBkup()
	{
		//Debug.Log(":"+m_lacet+":"+m_roulis+":"+m_inclinaison+":"+m_hauteur);
		m_bkupLacet = m_lacet;
		m_bkupRoulis = m_roulis;
		m_bkupInclinaison = m_inclinaison;
		m_bkupHauteur = m_hauteur;
		m_bkupPers = m_pers;
		m_bkucurScale = m_curScale;
	}
	
	public void bkup()
	{
		m_lacet = m_bkupLacet;
		m_roulis = m_bkupRoulis;
		m_inclinaison = m_bkupInclinaison;
		m_hauteur = m_bkupHauteur;
		m_pers = m_bkupPers;
		m_curScale = m_bkucurScale;

		updateRoulis();
		updateLacet();
		updateInclinaison();
		updateHeight();
		persChange();
		reinitScale();
	}
	
	//-----------------------------------------------------
    // Enregistre la configuration actuelle de la scène dans le modèle
    // et crée un point d'undo
	public void saveToModel()
	{
		Montage.sm.AddCameraState(m_hauteur,m_lacet,m_inclinaison,m_pers,m_curScale,m_roulis);
	}
	
	public void saveOriginal()
	{
//		Quaternion originalOrientation = transform.rotation;
//		Montage.sm.setOriginalState(originalOrientation,m_hauteur,m_pers,m_curScale);
		Montage.sm.setOriginalState(m_hauteur,m_lacet,m_inclinaison,m_pers,m_curScale,m_roulis);	
	}
	
	//DEBUG PC
	void OnGUI()
	{

		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(fscaleChange > 0.0f)
		{
			fscaleChange -= Time.deltaTime;
		}
		updateHeight ();
		updateLacet ();
		updateRoulis ();
		updateInclinaison ();
	}
	
    //-----------------------------------------------------
    // Preset de réglage de la scène pour les photos prises avec l'IPad
	public void SetIpadGridPreset()
	{
		reinitScale();
		//scaleScene((13.6f/m_baseSclFactor)-1);
		//scaleScene((9.8f/m_baseSclFactor)-1);
		scaleScene((12.8f/m_baseSclFactor)-1);
		m_hauteur = 0f;
		/*if(_cameraFrustrum==null)
			Camera.mainCamera.fieldOfView = 34.1f;
		else if (_cameraFrustrum.enabled==false)
			Camera.mainCamera.fieldOfView = 34.1f;
		m_pers = 52.0f;*/
		m_pers = 51.3f;
		persChange();
	}
//	public void getGui()
//	{
//		//Inclinaison lacet
//		GUI.Label(new Rect(0,80,80,20),"Rotation");
//		lacet = GUI.HorizontalSlider(new Rect(85, 85, 180, 30), lacet, -180f, 180f);		
//
//		GUI.Label(new Rect(0,120,80,20),"Inclinaison");
//		inclinaison = GUI.HorizontalSlider(new Rect(85, 125, 180, 30), inclinaison, -15.0F, +90f);
//		//hauteur
//		
//		GUI.Label(new Rect(0,160,80,20),"Hauteur");
//		hauteur = GUI.HorizontalSlider(new Rect(85, 165, 180, 30), hauteur, -10, 50);
//		
//		GUI.Label(new Rect(0,200,80,20),"Perspective");
//		pers = GUI.HorizontalSlider(new Rect(85, 205, 180, 30), pers, 20, 150);
//		
////		if(GUI.RepeatButton(new Rect(0, 145,100,40),"hauteur +"))
////		{
////			transform.Translate(new Vector3(0,-0.1f,0));
////		}
////		if(GUI.RepeatButton(new Rect(110, 145,100,40),"hauteur -"))
////      	{
////			transform.Translate(new Vector3(0,+0.1f,0));
////		}
////		//perspective
////		if(GUI.RepeatButton(new Rect(0, 190,100,40),"pers. +"))
////		{
////			setPrespective(true);
////		}
////		if(GUI.RepeatButton(new Rect(110, 190,100,40),"pers. -"))
////      	{
////			setPrespective(false);
////		}
//
//		setLacet(lacet);
//		setInclinaison(inclinaison);
//		Vector3 tmpH = transform.position;
//		tmpH.y = hauteur;
//		transform.position = tmpH;
////		cam.fieldOfView = pers;
//		persChange();
//	}
	
	/// //////////GUI//////////////////////////
	
//	public void getRotGui(Rect r,GUISkin s)
//	{
//		GUI.skin =s;
//		lacet = GUI.HorizontalSlider(r, lacet, -180f, 180f);
//		setLacet(lacet);
//	}
//	public void getIncGui(Rect r,GUISkin s)
//	{
//		GUI.skin =s;
//		inclinaison = GUI.HorizontalSlider(r, inclinaison, -15.0F, +90f);
//		setInclinaison(inclinaison);
//	}
//	public void getHauGui(Rect r,GUISkin s)
//	{
//		GUI.skin =s;
//		hauteur = GUI.HorizontalSlider(r, hauteur, -10, 50);
//		Vector3 tmpH = transform.position;
//		tmpH.y = hauteur;
//		transform.position = tmpH;
//	}
//	public void getpersGui(Rect r,GUISkin s)
//	{
//		GUI.skin =s;
//		pers = GUI.HorizontalSlider(r, pers, 20, 150);
//		cam.fieldOfView = pers;
//	}
//	public void getScaleGui(Rect r,GUISkin s)
//	{
//		GUI.skin =s;
//		scaleFactor = GUI.HorizontalSlider(r, scaleFactor, 0, 32);
//		setScaleFactor(scaleFactor);
//	}

	/// ////////////////////////////////////
	
////////////////GUI2///////////////////
	#region setLIHPS
	
	public void initL(float _lacet)
	{
		m_lacet = _lacet;
		updateLacet();
	}
	
	public float setL(float sens)
	{
		if(sens>0)
		{
			if(m_lacet<180)
				m_lacet = m_lacet + (sens/2);
			else
				m_lacet = -180;
		}
		else
		{
			if(m_lacet>-180)
				m_lacet = m_lacet + (sens/2);
			else
				m_lacet = 180;
		}	
		
		updateLacet();
		return m_lacet;
	}	
	public float GetL()
	{
		return m_lacet;
	}
	
	public void initR(float _roulis)
	{
		m_roulis = _roulis;
		updateRoulis();
	}
	
	public float setR(float sens)
	{
		if(sens>0)
		{
			if(m_roulis<180)
				m_roulis = m_roulis + (sens/2);
			else
				m_roulis = -180;
		}
		else
		{
			if(m_roulis>-180)
				m_roulis = m_roulis + (sens/2);
			else
				m_roulis = 180;
		}	
		
		updateRoulis();
		return m_roulis;
	}
	public float GetR()
	{
		return m_roulis;
	}
	
	public void initI(float _inclinaison)
	{
		m_inclinaison = _inclinaison;
		updateInclinaison();
	}
	
	public float setI(float sens)
	{
		if(sens>0)
		{
			if(m_inclinaison<85)
				m_inclinaison = m_inclinaison + (sens/2);
		}
		else
		{
			if(m_inclinaison>-15)
				m_inclinaison = m_inclinaison + (sens/2);
		}	
		
		updateInclinaison();
		return m_inclinaison;
	}
	public float GetI()
	{
		return m_inclinaison;
	}
	
	public void initH(float _hauteur)
	{
		m_hauteur = _hauteur;
		updateHeight();
	}
	
	public float setH(float sens)
	{
		if(sens>0)
		{
			if(m_hauteur<30)
				m_hauteur = m_hauteur + (sens/2);
		}
		else
		{
			if(m_hauteur>-10)
				m_hauteur = m_hauteur + (sens/2);
		}	
		updateHeight();
		return 1.6f-((m_hauteur+startHeight)/m_baseSclFactor);
	}
	public float GetH()
	{
		return 1.6f-((m_hauteur+startHeight)/m_baseSclFactor);
	}
	
	public void initP(float _pers)
	{
		m_pers = _pers;
		persChange();
	}
	
	public float setP(float sens)
	{
		if(sens>0)
		{
			if(m_pers<119.0f)
			{
				m_pers =m_pers+(sens/2);
				
				if(m_pers > 119.0f)
				{
					m_pers = 119.0f;
				}
			}
		}
		else if(sens<0)
		{
			if(m_pers>15)
				m_pers = m_pers+(sens/2);
		}
		//cam.fieldOfView = pers;
		persChange();
		return m_pers;
	}
	public float GetP()
	{
		return m_pers;
	}
	
	public void initS(float _curScale)
	{
		m_curScale = _curScale;
		scaleScene(_curScale);
	}
	
	public float setS(float scaleChange)
	{
		if(scaleChange != 0)
		{
			scaleScene(scaleChange);
			
			fscaleChange = 0.3f;
//        Debug.Log(DEBUGTAG+"scaling scene : "+m_curScale);
		}
		
		return m_curScale/m_baseSclFactor;
	}
	public float GetS()
	{
		return m_curScale/m_baseSclFactor;
	}
	
	#endregion
	/// ////////////////////////////////////
	#region update LIHPS 
	
	public void updateLacet()
	{
		Quaternion q = transform.localRotation;
		Vector3 v = q.eulerAngles;
		v.y = m_lacet;
		q.eulerAngles = v;
		transform.localRotation = q;
	}	
	
	public void updateInclinaison()
	{
		Quaternion q = transform.localRotation;
		Vector3 v = q.eulerAngles;
		v.x = m_inclinaison;
		q.eulerAngles = v;
		transform.localRotation = q;
	}
	
	public void updateRoulis()
	{
		Quaternion q = transform.localRotation;
		Vector3 v = q.eulerAngles;
		v.z = m_roulis;
		q.eulerAngles = v;
		transform.localRotation = q;
	}
		
	public void updateHeight()
	{
		Vector3 tmpH = transform.position;
		tmpH.y = m_hauteur;
		transform.position = tmpH;
	}

	public void persChange()
	{
//		Vector3 center = new Vector3(0,0,0);
//		Vector3 before = Camera.mainCamera.WorldToScreenPoint(center);
//		Camera.mainCamera.fieldOfView = pers;
//		
//		RaycastHit hitm = new RaycastHit ();
//		Physics.Raycast(Camera.mainCamera.ScreenPointToRay(new Vector2(before.x,before.y)), out hitm, Mathf.Infinity);
//		Vector3 delta = hitm.point;
//		
//		Vector3 pos = Camera.mainCamera.transform.position;
//		pos = pos - delta;
//		Camera.mainCamera.transform.position = pos;
		if(_cameraFrustrum!=null)
		{
			if(_cameraFrustrum.enabled)
			{
				_cameraFrustrum.SetFovX(m_pers);
				Vector3 tmp = cam.transform.localPosition;
				tmp.z = -calcPersDist(m_pers);
				cam.transform.localPosition = tmp;
			}
			else
			{
				Camera.main.fieldOfView = m_pers;
				Vector3 tmp = cam.transform.localPosition;
				tmp.z = -calcPersDist();
				cam.transform.localPosition = tmp;
			}
		}
		else
		{
			Camera.main.fieldOfView = m_pers;
			Vector3 tmp = cam.transform.localPosition;
			tmp.z = -calcPersDist();
			cam.transform.localPosition = tmp;
		}
		
	}
	
	public void setLIPH()
	{
		setLIPH(false);
	}
	public void setLIPH(bool fromGyro)
	{
		m_lacet = transform.localRotation.eulerAngles.y;
		m_inclinaison = transform.localRotation.eulerAngles.x;
		m_roulis = transform.localRotation.eulerAngles.z;
		if ((fromGyro==true) &&(m_roulis>90) &&  (m_roulis<270))
		{
			Quaternion rotaQuat = Quaternion.Euler(0,0,180);
			transform.localRotation = transform.localRotation * rotaQuat;
			m_lacet = transform.localRotation.eulerAngles.y;
			m_inclinaison = transform.localRotation.eulerAngles.x;
			m_roulis = transform.localRotation.eulerAngles.z;
		}
		m_pers = Camera.main.fieldOfView;
		if(_cameraFrustrum!=null)
			if(_cameraFrustrum.enabled)
				m_pers = _cameraFrustrum.GetFov();
		persChange();
		m_hauteur = transform.position.y;
		saveToModel();
		saveOriginal();
		saveBkup();
	}
	
	public void setliphS(float newScale)
	{
		reinitScale();
		scaleScene((newScale/m_baseSclFactor)-1);
	}
	
	#endregion
	//------------------------------------------------
	float calcPersDist()
	{
		float d =  persHeight/(Mathf.Tan((cam.fov/2)*Mathf.Deg2Rad));
		return d;
	}
	//------------------------------------------------
	float calcPersDist(float fov)
	{
		float d =  persHeight/(Mathf.Tan((fov/2)*Mathf.Deg2Rad));
		return d;
	}
	
	float calcPersHeight()
	{
		float d = Mathf.Abs(cam.transform.position.z);
		float fov = cam.fieldOfView;
		return d*Mathf.Tan((fov/2)*Mathf.Deg2Rad);
	}
	float calcPersHeight(float newFov)
	{
		float d = Mathf.Abs(cam.transform.position.z);
		float fov = newFov;
		return d*Mathf.Tan((fov/2)*Mathf.Deg2Rad);
	}
	
	//-----------------------------------------------------
//	private void setPrespective(bool plus)
//	{
//		if(plus)
//		{
//			if(cam.fieldOfView < 150)
//				{
//					float fov = cam.fieldOfView;
//					Vector3 newPos = cam.transform.localPosition;
//					newPos.z = newPos.z + dist*(((fov+1)/fov)-1);
//					dist = dist*((fov+1)/fov);
//					
//					cam.transform.localPosition = newPos;
//					cam.fieldOfView += 1;
//				}
//		}
//		else
//		{
//			if(cam.fieldOfView >20)
//				{
//					float fov = cam.fieldOfView;
//					Vector3 newPos = cam.transform.localPosition;
//					newPos.z = newPos.z + dist*(((fov-1)/fov)-1);
//					dist = dist*((fov-1)/fov);
//					
//					cam.transform.localPosition = newPos;
//					cam.fieldOfView -= 1;
//				}
//		}
//	}
	
	//-----------------------------------------------------
    // Applique un changement d'échelle à l'ensemble de la scène
	public float scaleScene(float sclFactorChange)
	{
		float sclFactor = 1+sclFactorChange;
		if(m_curScale * sclFactor < 0.5 || m_curScale * sclFactor > 32) // On bloque les modifications trop importantes
		{
			return m_curScale/m_baseSclFactor;
		}
		
		m_curScale *= sclFactor;
		
		// -- Unlock les objets de la scène -- TODO voir si c'est encore utile
		/*for(int i = 0;i<mainNode.transform.GetChildCount();i++)
		{
			Transform t = mainNode.transform.GetChild(i);
			
			if(t.GetComponent<ObjBehav>()!=null)
				t.GetComponent<ObjBehav>().locked = false;
		}*/
		
		/*for(int i = 0;i<mainNode.transform.GetChildCount();i++)
		{
			Transform t = mainNode.transform.GetChild(i);
			Vector3 dir = t.position - sceneCenter;
			
			if(t.GetComponent<ObjBehav>()!=null)
			{
				Vector3 pos = t.position;
				dir.x *= sclFactor;
				dir.y *= sclFactor;
				dir.z *= sclFactor;
				pos.y *= sclFactor;
				
				t.position = pos;
			}
		}*/
		
        Vector3 scl = m_grassGround.transform.localScale;
		scl.x *= sclFactor;
		scl.y *= sclFactor;
		scl.z *= sclFactor;
		m_grassGround.transform.localScale = scl;       // scale grass ground

        //scl = m_grassSky.transform.localScale;
        scl.x *= sclFactor;
        scl.y *= sclFactor;
        scl.z *= sclFactor;
        //m_grassSky.transform.localScale = scl;          // scale grass sky

		// -- relock des objets -- TODO voir si c'est encore utile
		/*for(int i = 0;i<mainNode.transform.GetChildCount();i++)
		{
			Transform t = mainNode.transform.GetChild(i);
			
			if(t.GetComponent<ObjBehav>()!=null)
				t.GetComponent<ObjBehav>().locked = true;
		}*/

        // Mettre à jour les transformations des projecteurs pour les iosShadows
//        Debug.Log (DEBUGTAG+"BUG IOSSHADOWS : curScale = "+m_curScale+", baseSclFactor = "+m_baseSclFactor);
//        _iosShadows.GetComponent<IosShadowManager>().UpdateShadowsScale(m_curScale/m_baseSclFactor);
//        _iosShadows.GetComponent<IosShadowManager>().UpdateShadowsPos();
		
		// RESCALE DES OBJETS
		/*for(int i = 0;i<mainNode.transform.GetChildCount();i++)
		{
			Transform t = mainNode.transform.GetChild(i);
			if(t.GetComponent<ObjBehav>()!=null)
			{
				Vector3 newLocalScale = new Vector3(t.localScale.x*sclFactor, t.localScale.y*sclFactor, t.localScale.z*sclFactor);
				t.localScale = newLocalScale;
//				t.gameObject.GetComponent<ObjBehav>().updateY(); //TODO à réactiver ou pas besoin ?
			}
		}*/
		
		List<Transform> listTransform = new List<Transform>();
		Vector3 v3center = Vector3.zero;
		int i = 0;
		foreach(Transform t in mainNode.transform)
		{
			if(t.name != "_avatar")
			{
				v3center += t.position;
				listTransform.Add(t);
				i++;
			}
		}
		
		if(i != 0)
		{
			v3center /= i;
			
			GameObject tempParent = new GameObject();
			tempParent.transform.position = v3center;
			
			foreach(Transform t in listTransform)
			{
				t.parent = tempParent.transform;
			}
			
			Vector3 v3scale = tempParent.transform.localScale;
			
			v3scale.x += sclFactorChange;
			v3scale.y += sclFactorChange;
			v3scale.z += sclFactorChange;
			
			tempParent.transform.localScale = v3scale;
			
			foreach(Transform t in listTransform)
			{
				t.parent = mainNode.transform;
			}
			
			Destroy(tempParent);
		}
		
		UsefullEvents.FireScaleChange();
		return m_curScale/m_baseSclFactor;
	} // scaleScene()
	
	//-----------------------------------------------------
	public float getScaleFactor()
	{
		return m_curScale;
	}

    //-----------------------------------------------------
    public float GetSceneScale()
    {
//        Debug.Log (DEBUGTAG+"GETTING SCENE SCALE : "+m_curScale+"/"+m_baseSclFactor);
        return m_curScale/m_baseSclFactor;
    }

	//-----------------------------------------------------
	public void reinitScale()
	{
		scaleScene(m_baseSclFactor/m_curScale-1);
	}
	
	public float GetPers()
	{
		return m_pers;
	}
	
} // class SceneControl
