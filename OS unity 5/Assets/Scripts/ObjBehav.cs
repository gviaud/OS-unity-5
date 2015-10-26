using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjBehav : MonoBehaviour {
	
	// a cleaner
	public bool objectCollision = false;
	public bool touchDown = false;
	public bool ready = false;
	//----------------
		
	public bool oneObjectSelected = false;     // Utilisé notamment pour savoir s'il faut repasser l'objet en vert ou en normal a la fin d'une collision
	ArrayList materialsList = new ArrayList();
	ArrayList renderEnabledList = new ArrayList();
	Renderer[] rs;
	
	private float yOrigin;
	
	bool YRotLock = true;
	Material matBackup; //material eau
	GameObject Scene;
	
	Material selected;
	Material unvisible;
	
	public Vector2 escapeVector;
	
	ArrayList materials = new ArrayList();
	    
    private ArrayList m_backupMaterialsT; // Liste de sauvegarde des matériaux (pour SetTransparent et UnsetTransparent)
	
	public bool collisionLock = false; // Quand collision avecAutreObj
	
	public bool locked; //unknow
	
	bool isLocked = false;     // verrouillage objet via menu interaction
		
	public bool isActive = false;
	
	private Vector3 _baseScale;
	private Camera _cam;
	
	Color rouge = new Color(1,0,0,0.5f);
	Color vert =  new Color(0,1,0,0.5f);
	Color orange = new Color(1,0.5f,0f,0.5f);
	Color startColor;

	private GameObject _heightObject;
	private float _heightOff7 = 0.0f;
	private float _heightFinal= 0.0f;
	private float _heightHideDelay = -1;
	private bool showHeightObject = false;
	private bool _castShadow = true;
	
	private Transform _AO = null;
	private bool _showOcclu = true;
	//Color selectColor;

//    private bool allSceneSelected;
	
	//Collision
	
	int nbObjColl=0; //nb dobj collisionné
	
	private int _currentHeight = 0;	private bool _noShadow = false;    //-----------------------------------------------------
	void Start ()
	{
		foreach(SkinnedMeshRenderer m in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			if(m.GetComponent<SkinnedMeshRenderer>())
			{
				startColor = m.GetComponent<SkinnedMeshRenderer>().material.color;
			}
		}
		//Scene = GameObject.Find("camPivot");
		selected = new Material(Shader.Find("Transparent/Diffuse"));
        m_backupMaterialsT = new ArrayList();
		_cam = Camera.main;
		Camera.main.GetComponent<ObjInteraction>().setColor(vert);
		
		unvisible = new Material(Shader.Find("Transparent/Diffuse"));
		unvisible.color = new Color(0,0,0,0);	
		/*if(GetAO()!=null)
		{
			GetAO().gameObject.SetActive(false);
		}*/
	}
	
	public void restart()
	{
		//Scene = GameObject.Find("camPivot");
		selected = new Material(Shader.Find("Transparent/Diffuse"));
        m_backupMaterialsT = new ArrayList();
		//Camera.mainCamera.GetComponent<ObjInteraction>().setColor(vert);
		
		unvisible = new Material(Shader.Find("Transparent/Diffuse"));
		unvisible.color = new Color(0,0,0,0);
	}
	
	// Update is called once per frame
	void Update ()
	{
		GetHeigthObject().SetActive(showHeightObject);
		if(showHeightObject)
		{
			GetHeigthObject().transform.position = new Vector3(
				transform.position.x,
				0.0f,
				transform.position.z);
		}
	
		foreach(SkinnedMeshRenderer m in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			if(m.GetComponent<SkinnedMeshRenderer>() && _cam.GetComponent<ObjInteraction>().getSelected() != gameObject)
			{
				print("DEDANS");
				m.GetComponent<SkinnedMeshRenderer>().material.color = startColor;// = new Color(0.0f,0.0f,0.0f,1.0f);
			}
		}
	}
	
	//################
	//## COLLISIONS ##
	//################
	
	/*void OnCollisionEnter(Collision collision)
	{	
		//Debug.Log("COLLISION ENTER");
//		if(isActive)
//		{
//			Debug.Log(" > "+collision.gameObject.name+ " - "+collision.gameObject.layer);
//		}
		if(tag == "noCol" || collision.gameObject.tag == "noCol")
			return;
		if(collision.gameObject.layer == 9 && isActive)
		{
			
			nbObjColl ++;
//			collisionLock = true;
			Camera.mainCamera.GetComponent<ObjInteraction>().setColor(rouge);
			setAsUnselected(gameObject);
			setAsSelected(gameObject);
			//Debug.Log("COLLISION DONE " +nbObjColl);
//			Camera.main.GetComponent<ObjInteraction>().deselect();
		}
				
	}*/
	
	/*void OnCollisionExit(Collision collision)
	{
		//Debug.Log("COLLISION EXIT");
		if(tag == "noCol" || collision.gameObject.tag == "noCol")
			return;
		if(collision.gameObject.layer == 9 && isActive)
		{			
			//Debug.Log("COLLISION DONE EXIT " + nbObjColl);
			nbObjColl --;
			if(nbObjColl<0)
				nbObjColl = 0;
			if(nbObjColl == 0)
			{
				//Debug.Log("COLLISION EXIT ok");
			//	collisionLock = false;
				Camera.mainCamera.GetComponent<ObjInteraction>().setColor(vert);
				setAsUnselected(gameObject);
				if(oneObjectSelected)
					setAsSelected(gameObject);
			}
			
		}
	}*/
	
	/*void OnCollisionStay(Collision collision)
	{
		//Debug.Log("COLLISION stay");
		if(tag == "noCol" || collision.gameObject.tag == "noCol")
			return;
		if(collision.gameObject.layer == 9 && isActive && nbObjColl == 0)
		{
			nbObjColl ++;
			//collisionLock = true;
			Camera.mainCamera.GetComponent<ObjInteraction>().setColor(rouge);
			setAsUnselected(gameObject);
			setAsSelected(gameObject);
		}
	}*/
	
	void OnDisable()
	{
//		if(Camera.mainCamera == null)
//			return;
//		if(!Camera.mainCamera.GetComponent<ObjInteraction>())
//			return;
//		if(Camera.mainCamera.GetComponent<ObjInteraction>().getSelected() == gameObject)
//		{
//			Camera.mainCamera.GetComponent<ObjInteraction>().setSelected(null);	
//		}
//		if(_heightObject!=null)
//			Destroy(_heightObject);
		if(_cam != null)
		{
			if(_cam.GetComponent<ObjInteraction>())
			{
				if(_cam.GetComponent<ObjInteraction>().getSelected() == gameObject)
				{
					_cam.GetComponent<ObjInteraction>().setSelected(null);	
				}
			}
		}
		if(_heightObject!=null)
			Destroy(_heightObject);
		
	}
	
	public float init()
	{
		RigidbodyConstraints c;
		c =	RigidbodyConstraints.FreezeRotation|
			RigidbodyConstraints.FreezePosition;
		GetComponent<Rigidbody>().constraints = c;
		GetComponent<Rigidbody>().useGravity = false;
		float sclFactor = GameObject.Find("camPivot").GetComponent<SceneControl>().getScaleFactor();
		//yOrigin = ((GetComponent<Collider>().bounds.size.y)/2-GetComponent<Collider>().bounds.center.y)*sclFactor;
		if(GetComponent<BoxCollider>()!=null) 
			yOrigin = ((GetComponent<BoxCollider>().size.y)/2-GetComponent<BoxCollider>().center.y)*sclFactor;
		if(transform.position.y == 0)
			yOrigin = 0;
		
//		_heightOff7 =transform.position.y - Mathf.Abs(yOrigin); // ici pour la copie!
		
//		if(GetComponent<MeshCollider>()!=null)
//			yOrigin = ((GetComponent<MeshCollider>().bounds.size.y)/2-GetComponent<MeshCollider>().bounds.center.y)*sclFactor;
		if((Mathf.Abs(transform.localScale.x)<1.5f)&&(Mathf.Abs(transform.localScale.y)<1.5f)&&(Mathf.Abs(transform.localScale.z)<1.5f))
			transform.localScale = new Vector3(sclFactor, sclFactor, sclFactor);
		_baseScale = transform.localScale;
		showObject(true);
		return yOrigin;
	}
	
//	public void updateY()
//	{
//		yOrigin = ((GetComponent<BoxCollider>().size.y)/2-GetComponent<BoxCollider>().center.y)*SceneControl.getScaleFactor();
//		Vector3 tmp = transform.position;
//		tmp.y = yOrigin;
//		transform.position = tmp;
//	}
	
	public Vector3 GetBaseScale()
	{
		return _baseScale;	
	}
	
	public void ModifyBaseScale(float factor)
	{
		_baseScale *= factor;
	}
	
	public bool Lock()
	{
		if(isLocked)// si deja locké
		{
			isLocked = false;
			Camera.main.GetComponent<ObjInteraction>().setColor(vert);
			setAsUnselected(gameObject);
			setAsSelected(gameObject);
		}
		else // si pas locké
		{
			if(!collisionLock)
			{
				isLocked = true;
				Camera.main.GetComponent<ObjInteraction>().setColor(orange);
				setAsUnselected(gameObject);
				setAsSelected(gameObject);
			}
		}
		return isLocked;
	}
	
	public void iAmLocked(bool b)
	{
		isLocked = b;	
	}
	
	public bool amILocked()
	{
		return isLocked;	
	}
	
	public void showObject(bool visible)
	{
		if(transform.GetChildCount() == 0)
		{
			if(GetComponent<Renderer>())
				GetComponent<Renderer>().enabled = visible;
		}
		else
		{
			for(int i=0;i<transform.GetChildCount();i++)
			{
				if(transform.GetChild(i).GetComponent<Renderer>())
				{
					transform.GetChild(i).GetComponent<Renderer>().enabled = visible;
				}
			}
		}
	}

	public void setAsSelected(GameObject g)
	{
	//	Debug.Log ("SET AS SELECTED : "+g.name);
		/*if(!isLocked)
		{
			if(nbObjColl > 0)
				Camera.mainCamera.GetComponent<ObjInteraction>().setColor(rouge);
			else
				Camera.mainCamera.GetComponent<ObjInteraction>().setColor(vert);
		}*/
		/*if(transform.localPosition.y>0.3f)
		{
			showHeightObject = true;
		}
		else if(transform.localPosition.y<0.0f)
		{
			showHeightObject = true;
		}
		else
		{
			showHeightObject = false;
		}*/
		showHeightObject = true;
	}
    
    //-----------------------------------------------------
	//Material previousMat;
	public void setAsUnselected(GameObject g)
	{		
	//	Debug.Log ("SET AS UNSELECTED : "+g.name);			
		showHeightObject = false;
		/*if(GetComponent<Renderer>())
		{
			GetComponent<Renderer>().material.color = new Color(1.0f,0.2f,1.0f,1.0f);
		}
		else if(GetComponent<MeshRenderer>())
		{
			GetComponent<MeshRenderer>().material.color = new Color(1.0f,0.2f,1.0f,1.0f);
		}
		else if(GetComponent<SkinnedMeshRenderer>())
		{
			GetComponent<SkinnedMeshRenderer>().material.color = new Color(1.0f,0.2f,1.0f,1.0f);
		}*/

	} // setAsUnselected()
	
    //-----------------------------------------------------
    public void SetTransparent(GameObject g)
    {
        selected.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        if(g.transform.GetChildCount() == 0)        // Si l'objet n'a pas de noeuds enfants
        {
            int nb = g.GetComponent<Renderer>().materials.Length;   // Nb de matériaux
            Material[] toModel = new Material[nb];  // Création d'une nouvelle liste de matériaux
            for(int i=0;i<nb;i++)                   // Pour chaque matériau
            {
                materials.Add((Material)g.GetComponent<Renderer>().materials[i]); // le sauvegarder dans la liste de backup des matériaux
                toModel[i]=selected;                              // Le remplacer par le matériau voulu
            }
            g.GetComponent<Renderer>().materials = toModel;         // assigner la liste au gameObject
        }
        else
        {
			Transform[] allChildren = GetComponentsInChildren<Transform>();
            for(int i=0; i<allChildren.Length; i++)
            {
                if(allChildren[i].GetComponent<MeshRenderer>())
                {
                    // L'eau est mal restituée au rétablissement, solution temporaire : cacher l'eau
                   	if (allChildren[i].name.Contains("water")){
						// allChildren[i].renderer.enabled = false;
						Material transp = (Material)Resources.Load ("materials/Plastique_transp");
						matBackup = allChildren[i].GetComponent<Renderer>().material;
						allChildren[i].GetComponent<Renderer>().material = transp;
						Debug.Log ("water");
					}   
                    else
                    {
                        int nb = allChildren[i].GetComponent<Renderer>().materials.Length;
                        Material[] _mats = new Material[nb];
                        
                        //Sauvegarde des matériaux pour les rétablir
                        for (int m=0;m<nb;m++)
                            _mats[m] = allChildren[i].GetComponent<Renderer>().materials[m];
                        
                        m_backupMaterialsT.Add(_mats);//Sauvegarde dans une arrayList globale
                                            
                        Material[] _toModel = new Material[nb];
                        //On remplace chaque material par le material rouge transparent
                        for (int j=0;j<nb;j++)
                            _toModel[j] = selected;
                        
                        //Et on le retourne dans l'objet    
                       allChildren[i].GetComponent<Renderer>().materials = _toModel;
                    } // si ce n'est pas de l'eau
                } // si l'enfant a un renderer
            } // Pour chaque objet enfant
        } // Si plusieurs objets enfants
    }// SetTransparent()
    
    //-----------------------------------------------------
    public void UnsetTransparent(GameObject g)
    {
        if(g.transform.GetChildCount() == 0)
        {
            int nb = g.GetComponent<Renderer>().materials.Length;
            Material[] toModel = new Material[nb];
            for(int i=0;i<nb;i++)
                toModel[i] = (Material)materials[i];
            
            g.GetComponent<Renderer>().materials = toModel;
        } // si un seul enfant
        else
        {
            int j = 0;
			Transform[] allChildren = GetComponentsInChildren<Transform>();
            for(int i=0; i<allChildren.Length; i++)
            {
                if(allChildren[i].GetComponent<MeshRenderer>())
                {   
                    if (allChildren[i].name.Contains("water")){
                       // allChildren[i].renderer.enabled = true;
						allChildren[i].GetComponent<Renderer>().material = matBackup;
					}
                    else
                    {
                        allChildren[i].GetComponent<Renderer>().materials = (Material[]) m_backupMaterialsT[j];
                        j++;
                    }
                }
            }
        } // si plusieurs enfants
        m_backupMaterialsT.Clear();
    } // UnsetTransparent()
	
	public void ChangeLocalHeight(float dir)
	{
		showHeightObject = true;
		if(_currentHeight>0)
		{
			_heightOff7 = Mathf.Clamp(_heightOff7 + dir,0,100);
		}
		else if(_currentHeight<0)
		{
			_heightOff7 = Mathf.Clamp(_heightOff7 + dir,-100,0);
		}
		else
		{
			_heightOff7 = Mathf.Clamp(_heightOff7 + dir,-100,100);
			if(dir>0)
				_currentHeight=1;
			else if(dir<0)
				_currentHeight=-1;
		}
		CheckShadowWithOffSet(false);	
		
		Vector3 pos = transform.position;
		pos.y = yOrigin + _heightOff7;
		transform.position = pos;
		
		GetHeigthObject().SetActive(true);
		GetHeigthObject().transform.position = new Vector3(pos.x,0.0f,pos.z);
		GetHeigthObject().transform.localScale = new Vector3(10.0f,_heightOff7*2,10.0f);

		if(_heightHideDelay == -1)
		{
			_heightHideDelay = 1;
			StartCoroutine(DelayHide());
		}
		else
			_heightHideDelay = 1;
	}
	
	private IEnumerator DelayHide()
	{
		while(_heightHideDelay > 0)
		{
			_heightHideDelay -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		_heightHideDelay = -1;
		GetHeigthObject().SetActive(false);
	}
	public void UpdateCurrentHeight()
	{	
		if(_heightOff7>0)
			_currentHeight=1;
		else if(_heightOff7<0)
			_currentHeight=-1;
		else
			_currentHeight=0;
	}
	
	private GameObject GetHeigthObject()
	{
		if(_heightObject == null)
		{
			_heightObject = (GameObject) GameObject.Instantiate((GameObject)Resources.Load("prefabs/HEIGHT_TOOL_0001"));
			_heightObject.transform.localScale = new Vector3(10.0f,0.0f,10.0f);
			_heightObject.transform.position = new Vector3(0,0,0);	
			if(GetScene()!=null)
			{
				Vector3 tmp = GetScene().transform.eulerAngles;
				//Vector3 tmp2 = Camera.mainCamera.transform.eulerAngles;
				//Debug.Log("Cam pivot rotation "+tmp+"Camera rotation "+tmp2);
				_heightObject.transform.eulerAngles = new Vector3(0,tmp.y,0);
			}
		}

		return _heightObject;	
	}
	
	public float GetHeightOff7()
	{
		return _heightOff7;
	}
	
	public void SetHeighOff7(float off7)
	{
		_heightOff7 = off7;
		if(_heightOff7>0)
			_currentHeight=1;
		else if(_heightOff7<0)
			_currentHeight=-1;
		else
			_currentHeight=0;
		GetHeigthObject().transform.localScale = new Vector3(10.0f,_heightOff7*2,10.0f);
		CheckShadowWithOffSet(true);
	}
	
	private Transform GetAO()
	{
		if(_AO==null)
		{
			_AO = transform.FindChild("AO");	
		}
		return _AO;
	}
	
	private GameObject GetScene()
	{
		if(Scene==null)
		{
			Scene = GameObject.Find("camPivot");
		}
		return Scene;
	}
	
	private void CheckShadowWithOffSet(bool force)
	{
		if(usefullData.lowTechnologie)
			return;
		if((_heightOff7>2.0f)||(_heightOff7<0.0f))		{
			if(_castShadow || force)
			{
				foreach(Renderer renderer in GetRenderers())
				{
					renderer.castShadows = false;
				}
				_castShadow=false;
			}
			if(!_showOcclu || force)
			{
				if(GetAO()!=null)
				{
					GetAO().gameObject.SetActive(true);
				}
				_showOcclu=true;
			}
		}
		else
		{
			if(!_castShadow || force)
			{
				foreach(Renderer renderer in GetRenderers())
				{
					renderer.castShadows = true;
				}
				_castShadow=true;
			}
			if(_showOcclu || force)
			{
				if(GetAO()!=null)
				{
					GetAO().gameObject.SetActive(false);
				}
				_showOcclu=false;
			}
		}
	}
	
	private Renderer[] GetRenderers()
	{
		if(rs==null)
		{
			rs = GetComponentsInChildren<Renderer>();
		}
		return rs;
	}
    
}