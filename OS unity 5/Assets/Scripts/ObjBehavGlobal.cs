//-----------------------------------------------------------------------------
// Assets/Scripts/ObjBehavGlobal.cs - 02/2012 - KS

using UnityEngine;
using System.Collections.Generic;

using Pointcube.Global;
using Pointcube.Utils;

//---------------------------------------------------------
// Script pour les comportements à appliquer à tous les objets
// de la scène (nœuds enfants de MainNode)
public class ObjBehavGlobal : MonoBehaviour
{
    private bool       m_objectsTransp;   // Objets actuellement transparents ou non

//    private bool       m_movingAll;       // Fonction "déplacer tous les objets" activée ou non
    private Vector2    m_dragAllBasePos;  // Position de départ (x, z)
    private Vector3    m_rotateAllCenter; // Centre de rotation
    //public  GameObject m_debugCube;


    //-----------------------------------------------------
	void Start ()
    {
        m_objectsTransp = false;

//        m_movingAll     = false;
        m_rotateAllCenter = Vector3.zero;
//        m_debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
//        m_debugCube.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
//        m_debugCube.renderer.material.SetColor("_MainColor", new Color(1f, 1f, 0f, 0.5f));
    }
	
	//-----------------------------------------------------
	void Update ()
    {
//        if(m_movingAll && m_oldSelectAllPos != m_selectAll.transform.localPosition)
//        {
//            Transform child;
//            for(int i=0; i<transform.GetChildCount(); i++)
//            {
//                child = transform.GetChild(i).transform;
//                if(child.name != "avatar" && child.name != "selectAll_noShadows")
//                {
//                    child.localPosition += (m_selectAll.transform.localPosition - m_oldSelectAllPos);
//                }
//            } // foreach child
//            m_oldSelectAllPos = m_selectAll.transform.localPosition;
//        }
    } // Update()
        
    //-----------------------------------------------------
    // Rendre les objets de la scène translucides, utilisé
    // pour l'outil gomme (cf. GUISubTools et EraserMask)
    public void ToggleSceneObjectsTransparent()
    {
        for(int i=0; i<transform.GetChildCount() ; i++)
        {
            ObjBehav objScript = (ObjBehav) transform.GetChild(i).GetComponent<ObjBehav>();
            if(objScript)
            {
                if(!m_objectsTransp)
                    objScript.SetTransparent(transform.GetChild(i).gameObject);
                else
                    objScript.UnsetTransparent(transform.GetChild(i).gameObject);
            }
        }
		/*Transform[] allChildren = GetComponentsInChildren<Transform>();
		foreach (Transform transformChild in allChildren) 
        {
          ObjBehav objScript = transformChild.GetComponent<ObjBehav>();
          if(objScript)
            {
                if(!m_objectsTransp)
                    objScript.SetTransparent(objScript.gameObject);
                else
                    objScript.UnsetTransparent(objScript.gameObject);
            }
        }*/
        m_objectsTransp = !m_objectsTransp;
    } // ToggleSceneObjectsTransparent()

	public bool isObjectsTransp()
	{
		return m_objectsTransp;
	}
	
//=============================================================================
// Pour le déplacement de tous les objets de la scène, utilisé par ObjInteraction
#region transformAll

    //-----------------------------------------------------
    public void InitDragAllPos()
    {
        m_dragAllBasePos = _3Dutils.RaycastFromScreen(PC.In.GetCursorPos());
    }

    //-----------------------------------------------------
    public void DragAll()
    {
//        Debug.Log("DragAll");
        Vector2 curPoint = _3Dutils.RaycastFromScreen(PC.In.GetCursorPos());
        Vector3 tmpPoint = new Vector3();
        if(curPoint != m_dragAllBasePos)
        {
            Transform child;
            for(int i=0; i<transform.GetChildCount(); i++)
            {
                child = transform.GetChild(i).transform;
                if(child.name != "_avatar")
                {
                    tmpPoint.x = child.localPosition.x+(curPoint.x-m_dragAllBasePos.x);
                    tmpPoint.y = child.localPosition.y;
                    tmpPoint.z = child.localPosition.z+(curPoint.y-m_dragAllBasePos.y);
                    child.localPosition = tmpPoint;
                }
            } // foreach child
//            Debug.Log("basePos = ("+m_dragAllBasePos.x+","+m_dragAllBasePos.y+"curPos=("+curPoint.x+","+curPoint.y+")");
            m_dragAllBasePos = curPoint;
        }
    }

    //-----------------------------------------------------
    // Le centre de rotation ne doit pas bouger pendant
    // une rotation, ce qui est le cas si on prend en temps
    // réel le centre du bounds de la scène.
    public void InitRotateAllCenter()
    {
        m_rotateAllCenter = GetSceneCenter();
    }

    //-----------------------------------------------------
    // Faire tourner les objets de la scène, de angleChange°
    // autour de m_rotateAllCenter.
    public void RotateAll(float angleChange)
    {
        if(angleChange != 0)
        {
            Transform child;
            for(int i=0; i<transform.GetChildCount(); i++)
            {
                child = transform.GetChild(i).transform;
                if(child.name != "_avatar")
                    child.RotateAround(m_rotateAllCenter, Vector3.up, angleChange);
            } // foreach child
        }
    } // RotateAll()

    //-----------------------------------------------------
    // Retourne le centre de la bounding box qui englobe les objets sous MainNode.
    // TODO Retourner le centre de gravité ne serait pas plus adapté ? (attirance des objets éloignée moindre, donc
    // rotation plus centrée sur les endroits comportant plusieurs objets...)
    public Vector3 GetSceneCenter()
    {
		// Moyenne
		Vector3 center = Vector3.zero;
        Transform child;
        for(int i=0; i<transform.GetChildCount(); i++)
        {
            child = transform.GetChild(i);
            if(child.name != "_avatar")
				center += child.position;
        }
		center.x /= transform.GetChildCount() - 1;
		center.y = 0;
		center.z /= transform.GetChildCount() - 1;
		
		return center;
		// Calcul du centre de la BB de la scene
//        Bounds sceneBounds = new Bounds(Vector3.zero, Vector3.one);
//        Transform child;
//        for(int i=0; i<transform.GetChildCount(); i++)
//        {
//            child = transform.GetChild(i);
//            if(child.name != "avatar")
//                sceneBounds.Encapsulate(child.collider.bounds);
//        }

//        m_debugCube.transform.position = sceneBounds.center;
//        Vector3 tmpScale = new Vector3(sceneBounds.extents.x, sceneBounds.extents.y, sceneBounds.extents.z);
//        m_debugCube.transform.localScale = tmpScale*2;

//        return sceneBounds.center;
    } // GetSceneCenter()
    
    public int getNumberObjects()
    {
    	return transform.childCount - 1;
    }

    public void SelectAll()
    {
        // Rendre verts les objets de la scène
        GameObject child;
        for(int i=0; i<transform.GetChildCount(); i++)
        {
            child = transform.GetChild(i).gameObject;
            if(child.name != "_avatar")
                child.GetComponent<ObjBehav>().setAsSelected(child);
        } // foreach child
    }

    public void UnselectAll()
    {
        GameObject child;
        for(int i=0; i<transform.GetChildCount(); i++)
        {
            child = transform.GetChild(i).gameObject;
            if(child.name != "_avatar")
                child.GetComponent<ObjBehav>().setAsUnselected(child);
        } // foreach child
    }

#endregion

} // class ObjBehavGlobal
