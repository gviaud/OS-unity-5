using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class ObjectsModel
{
	/* ObjectsModel
	 * contient les données des objets
	 */
	
	Montage _montage;
	GameObject mainNode;
	private OSLib m_library;
	
	public ObjectsModel(Montage m)
	{
		mainNode = GameObject.Find("MainNode");
		_montage = m;
	}
	
	public void setLibrary(OSLib lib)
	{
		m_library = lib;
	}
	public OSLib getMainLib()
	{
		return m_library;	
	}
	
	//Save/Load
	public void save(BinaryWriter buf)
	{
		try{
			buf.Write((mainNode.transform.GetChildCount()-1)); //< nombre d'obj (en dehors de lavatar)
		}
			catch(UnityException  e){
					Debug.Log ("error saving objects :"+e.ToString());
				}
		foreach(Transform t in mainNode.transform)
		{
			if(t.name != "_avatar")
			{
				try{
					t.GetComponent<ObjData>().SaveData(buf);
					Debug.Log ("transform "+t.name);
				}
				catch(UnityException  e){
					Debug.Log ("error saving objects :"+e.ToString());
				}
			}
		}
		
	}

	//AUX. FCN.
}
