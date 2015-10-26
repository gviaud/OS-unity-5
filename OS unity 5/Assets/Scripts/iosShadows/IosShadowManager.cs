using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using Pointcube.Utils;
using Pointcube.Global;

// TODO : quand un objet est remplacé par un autre, supprimer l'ombre et la re-créer

//-----------------------------------------------------------------------------
// Classe qui gère les ombres (camera + projecteur) pour iOS. cf. aussi IosShadow.cs
public class IosShadowManager : MonoBehaviour
{
    // -- Réf. scène --
    public GameObject           m_mainNode;
    public GameObject           m_lightPivot;
    public GameObject           m_camPivot;

    //-----------------------------------------------------
	public float                lightness       = 0.75f;
	public int                  textureSize     = 256;
    public Quaternion           shadows_rotation;       // Utilisé pour modifier les ombres

//	public Material				mCamMat;
//	public Material				mProjMat;
	
    private GameObject          m_added;
    private Dictionary<GameObject, int> m_children;     // Dictionnaire pour accéder au système d'ombrage iOS
                                                            // d'un objet à partir d'une référence vers cet objet
    // Layer internally used for shadow rendering. Should not be used for anything in the game.
    private static int          s_uniqueLayer = 31;     // Doit être le même que celui de IosShadow
    private static string       DEBUGTAG = "IosShadowManager : ";
    private static bool         DEBUG    = true;
	
	private float _power	= 0.0f;
	private float _blur		= 0.0f;
	
    
    //-----------------------------------------------------
	void Start()
	{
        if(PC.DEBUG && DEBUG)
            Debug.Log (DEBUGTAG+"Start()");

        if(m_mainNode == null)      Debug.Log(DEBUGTAG+"Main Node"  +PC.MISSING_REF);
        if(m_lightPivot == null)    Debug.Log(DEBUGTAG+"Light Pivot"+PC.MISSING_REF);
        if(m_camPivot == null)      Debug.Log(DEBUGTAG+"Cam Pivot"  +PC.MISSING_REF);

        m_children = new Dictionary<GameObject, int>();
        m_added = null;
		
		// Rotation
		Quaternion q = m_lightPivot.transform.localRotation;
		
		Vector3 v = q.eulerAngles;
		v.y += 180;
		v.x = 90 - v.x;
		q.eulerAngles = v;
        shadows_rotation = q;
        
        for(int i=0; i<m_mainNode.transform.GetChildCount(); i++)
		    AddShadow(m_mainNode.transform.GetChild(i).gameObject);
        
	} // Start()
	
	void OnEnable()
	{
		UsefullEvents.UpdateIosShadowScale += AddShadowScaleFactor;
		UsefullEvents.ReinitIosShadow += ReinitIosShadow;
	}
	void OnDisable()
	{
		UsefullEvents.UpdateIosShadowScale -= AddShadowScaleFactor;
		UsefullEvents.ReinitIosShadow -= ReinitIosShadow;
	}
    
    //-----------------------------------------------------
    // Create a child camera/projector object
    public GameObject AddShadow(GameObject target)
    {
        // -- Bounding box --
		float scl = m_camPivot.GetComponent<SceneControl>().getScaleFactor();
        scl = (scl>0 ? scl : (scl != 0 ? -scl : 1));
        
        Bounds bbox = _3Dutils.getMeshBounds(target);
        float diag = _3Dutils.GetBoundDiag(bbox);
//        Debug.Log (DEBUGTAG+"SceneScl = "+sceneScl);
        diag /= m_camPivot.GetComponent<SceneControl>().GetSceneScale();
		diag *= 1.5f;                                               // marge de sécurité

//		Debug.Log("IOShadow : diag="+diag);

        GameObject child;
		if(Application.platform != RuntimePlatform.Android)
       	 child = new GameObject (transform.GetChildCount()+"_"+target.name, typeof(Camera),
				typeof(Projector), typeof(BlurEffect), typeof(IosShadow));
		else
        	child = new GameObject (transform.GetChildCount()+"_"+target.name, typeof(Camera),
				typeof(Projector),  typeof(IosShadow));
			
        // -- Camera --
        child.GetComponent<Camera>().clearFlags = CameraClearFlags.Color;
        child.GetComponent<Camera>().backgroundColor = Color.white;
        child.GetComponent<Camera>().cullingMask = (1 << s_uniqueLayer);
        child.GetComponent<Camera>().orthographic = true;
        child.GetComponent<Camera>().orthographicSize = diag;
        child.GetComponent<Camera>().aspect = 1.0f;
        child.GetComponent<Camera>().farClipPlane = diag;
        child.GetComponent<Camera>().nearClipPlane = -diag;
        
        // -- Projecteur --
        Projector proj = (Projector) child.GetComponent(typeof(Projector));
        proj.orthographic = true;
        //proj.ignoreLayers = (1<<m_targetLayer);  
        var layerMask = 1 << 13;
        // This would cast rays only against colliders in layer 13.
        // But instead we want to collide against everything except layer 13. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;
        proj.ignoreLayers = layerMask;
        proj.orthographicSize = diag;
        proj.farClipPlane = diag;
        proj.nearClipPlane = -diag;
        
        // -- Effet flou --
		if(Application.platform !=RuntimePlatform.Android)
        	child.GetComponent<BlurEffect>().iterations = 0;
        
        // -- Transform --        
        child.transform.parent = transform;

//        child.transform.localRotation = Quaternion.AngleAxis(310, Vector3.right);   // angle par défaut pour bien voir les ombres
//        child.transform.RotateAround(Vector3.up, 180);

        // Le pivot des objets n'est pas toujours centré, donc on utilise ce qu'on peut pour le retrouver
        if(target.transform.GetComponent<Collider>())
            child.transform.position = target.transform.GetComponent<Collider>().bounds.center;
        else if(target.transform.GetComponent<Renderer>())
            child.transform.position = target.transform.GetComponent<Renderer>().bounds.center;
        else
            Debug.LogError("IosShadowManager : Impossible de trouver le centre de l'objet");

        // -- Script --
//        ((IosShadow)child.GetComponent<IosShadow>()).shadowMaterial = mCamMat;
//        ((IosShadow)child.GetComponent<IosShadow>()).projectorMaterial = mProjMat;		
        ((IosShadow)child.GetComponent<IosShadow>()).Init(target, textureSize, lightness, diag, child.transform.position);
		((IosShadow)child.GetComponent<IosShadow>()).SetLightness(_power);
        child.GetComponent<IosShadow>().Scale(m_camPivot.GetComponent<SceneControl>().GetSceneScale());
		
		if(Application.platform !=RuntimePlatform.Android)
			((BlurEffect)child.GetComponent<BlurEffect>()).iterations = (int) _blur;
		//        m_uniqueLayer --;
        
		//if(!m_children.ContainsKey(target))
        m_children.Add(target, transform.GetChildCount()-1);
        m_added = target;

        return child;
    } // CreateCamProj
    
    //-----------------------------------------------------
	void LateUpdate()
	{
        if(m_added != null) // Utilisé pour centrer le projecteur sur l'objet après la création
        {                   // (comme le centre de la collision box est utilisé, il faut attendre une frame avant qu'il soit correct)
            UpdateShadowPos(m_added);
            m_added = null;
        }
        
        // Mise à jour de la rotation des projecteurs
        for(int i=0; i<transform.GetChildCount(); i++)
            transform.GetChild(i).transform.localRotation = shadows_rotation;
                
	} // LateUpdate()
 
    //-----------------------------------------------------
    // Retourne la boîte englobante de la hiérarchie donnée
	/*public static Bounds getMeshBounds(GameObject root)
	{
		//Get all the mesh filters in the tree.
		MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter> ();
		
		//Construct an empty bounds object w/o an extant.
		Bounds bounds = new Bounds (Vector3.zero, Vector3.zero);
		bool firstTime = true;
		
		//Debug.Log("filters "+filters.Length);
		
		//For each mesh filter...
		foreach (MeshFilter mf in filters) {
			//Pull its bounds into the overall bounds.  Bounds are given in
			//the local space of the mesh, but we want them in world space,
			//so, tranform the max and min points by the xform of the object
			//containing the mesh.
			Vector3 maxWorld = mf.transform.TransformPoint (mf.mesh.bounds.max);
			Vector3 minWorld = mf.transform.TransformPoint (mf.mesh.bounds.min);
			
			//If no bounds have been set yet...
			if (firstTime) {
				firstTime = false;
				//Set the bounding box to encompass the current mesh, bounds,
				//but in world coordinates.
				//center
				bounds = new Bounds ((maxWorld + minWorld) / 2, maxWorld - minWorld);
				//extent
			//We've started a bounding box.  Make sure it ecapsulates
			} else {
				//the current mesh extrema.
				bounds.Encapsulate (maxWorld);
				bounds.Encapsulate (minWorld);
			}
		}
		//Return the bounds just computed.
		return bounds;
	} // getMeshBounds()*/
    
    //-----------------------------------------------------
    public void UpdateShadowPos(GameObject obj)
    {
		//Debug.Log("UpdateShadowPos");
        int shadowIndex = 0;
        if(m_children.TryGetValue(obj, out shadowIndex))
        {
            // Le pivot des objets n'est pas toujours centré, donc on utilise ce qu'on peut pour le retrouver
            if(obj.transform.GetComponent<Collider>())
            {
                transform.GetChild(shadowIndex).GetComponent<IosShadow>().transform.position = obj.transform.GetComponent<Collider>().bounds.center;
                
//                // LOD : si l'objet est éloigné, on réduit la taille de la texture
//                // TODO faire ça dans une routine pour éviter le petit délai créé par le changement de texture
//                if(obj.transform.collider.bounds.center.z >= 75f && transform.GetChild(shadowIndex).GetComponent<IosShadow>().GetTexSize() > 256)
//                    transform.GetChild(shadowIndex).GetComponent<IosShadow>().ChangeRenderTex(256);
//                else if(obj.transform.collider.bounds.center.z < 75f && transform.GetChild(shadowIndex).GetComponent<IosShadow>().GetTexSize() <= 256)
//                    transform.GetChild(shadowIndex).GetComponent<IosShadow>().ChangeRenderTex(512);
            }
            else if(obj.transform.GetComponent<Renderer>())
                transform.GetChild(shadowIndex).GetComponent<IosShadow>().transform.position = obj.transform.GetComponent<Renderer>().bounds.center;
            else
                Debug.LogError("IosShadowManager : Impossible de trouver le centre de l'objet "+obj.name);
        }
        else
            Debug.LogWarning("IosShadowManager : UpdatePosition failed because no existing IosShadow corresponds to the given GameObject.");
    }
	  //-----------------------------------------------------
    public void ReinitIosShadow(GameObject obj)
    {
		StartCoroutine(DelAndAddShadow(obj));
	}
	
	IEnumerator DelAndAddShadow(GameObject obj)
	{
		while(!m_children.ContainsKey(obj))
		{
			yield return new WaitForEndOfFrame();	
		}
		DeleteShadow(obj, true);
		AddShadow(obj);	
	}

    //-----------------------------------------------------
    public void UpdateShadowsPos()
    {
        GameObject obj, child;
		if(m_children != null)
		{
	        foreach(KeyValuePair<GameObject, int> keyVal in m_children)
	        {
	            obj = keyVal.Key;
	            child = transform.GetChild(keyVal.Value).gameObject;
	
	            // Le pivot des objets n'est pas toujours centré, donc on utilise ce qu'on peut pour le retrouver
	            if(obj.transform.GetComponent<Collider>())
	                child.transform.position = obj.transform.GetComponent<Collider>().bounds.center;
	            else if(obj.transform.GetComponent<Renderer>())
	                child.transform.position = obj.transform.GetComponent<Renderer>().bounds.center;
	            else
	                Debug.LogError("IosShadowManager : Impossible de trouver le centre de l'objet : "+obj.name);
	        }
		}
    }
	
	public void UpdateShadowParameter()
	{	
		/*GameObject obj, child;
		if(m_children != null)
		{
	        foreach(KeyValuePair<GameObject, int> keyVal in m_children)
	        {
	            obj = keyVal.Key;
	            child = transform.GetChild(keyVal.Value).gameObject;
	
	            // Le pivot des objets n'est pas toujours centré, donc on utilise ce qu'on peut pour le retrouver
	            if(obj.transform.collider)
	                child.transform.position = obj.transform.collider.bounds.center;
	            else if(obj.transform.renderer)
	                child.transform.position = obj.transform.renderer.bounds.center;
	            else
	                Debug.LogError("IosShadowManager : Impossible de trouver le centre de l'objet : "+obj.name);
	        }
		}*/
	}

    //-----------------------------------------------------
    public void UpdateShadowsScale(float newScale)
    {
        for(int i=0; i<transform.GetChildCount(); i++)
        {
		//	if(!transform.GetChild(i).name.Contains("_avatar"))
            	transform.GetChild(i).GetComponent<IosShadow>().Scale(newScale);
		/*	else
			{
				ReinitIosShadow(transform.GetChild(i).gameObject);
			}*/
        }
    }
	
    //-----------------------------------------------------
    // Scale une ombre d'objet indépendemment de la scène
    // Note : le facteur donné ici sera conservé lors des
    // 	       changements ultérieurs du facteur global de la scène
    public void AddShadowScaleFactor(GameObject obj, float newScale)
    {
        //Debug.Log(newScale);
		int index;
		if(m_children.TryGetValue(obj, out index))
            transform.GetChild(index).GetComponent<IosShadow>().AddScaleFactor(newScale);
        else
            Debug.LogError(DEBUGTAG+"Object not found in IosShadowManager's dictionary : "+obj.name);

    }

	
    //-----------------------------------------------------
    public void DeleteShadow(GameObject obj, bool immediate)
    { 
        int shadowIndex = 0;
        if(m_children.TryGetValue(obj, out shadowIndex))
        {
            m_children.Remove(obj);
            // DestroyImmediate pour éviter que le script iosShadow n'essaye d'accéder au GameObjet supprimé
            DestroyImmediate(transform.GetChild(shadowIndex).gameObject);
            
            // Mise à jour des indices des GameObjects fils dans le dictionnaire
            if(shadowIndex < transform.GetChildCount())
            {
                List<GameObject> keys = new List<GameObject>();
                foreach (GameObject key in m_children.Keys)
                {
                    if(m_children[key] > shadowIndex)
                        keys.Add(key);
                } // Foreach entry in dictionary
                foreach(GameObject key in keys)
                {
                    m_children[key] = m_children[key] - 1;
                    transform.GetChild(m_children[key]).name = m_children[key]+"_"+key.name;
                }
            } // s'il faut mettre à jour les indices
        }
        else
            Debug.LogWarning("IosShadowManager : DeleteShadow failed because no existing IosShadow corresponds to the given GameObject.");
    }
	
	public void RedoShadow(GameObject obj)
	{
		DeleteShadow(obj,true);
		AddShadow(obj);
	}
	
	public void SetPower(float newPower)
	{
		_power = newPower;
	}
	
	public void SetBlur(float newBlur)
	{
		_blur = newBlur;
	}
	
} // class IosShadows
