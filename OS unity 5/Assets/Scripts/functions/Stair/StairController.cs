using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StairController : MonoBehaviour 
{
	protected List<Transform> _transformGizmos = new List<Transform> ();
	
	protected List<StairGizmo> _gizmos = new List<StairGizmo> ();
	
	protected List<StairGizmo> _availableGizmos;
	
	[SerializeField]
	protected int _selectedGizmoIndex = -1;
	
	[SerializeField]
	protected int _selectedStairIndex = -1;
	
	protected OSLibStairs _stairList;
		
	// Use this for initialization
	void Start ()
	{
		//find gizmos
		int gizmoIndex = 1;
		bool gizmoIsNotNull = true;
		
		do
		{
			Transform gizmo = transform.Find ("Gizmo" + gizmoIndex.ToString ());
			
			if (gizmo != null)
			{		
				Destroy (gizmo.GetComponent<MeshFilter> ());
				Destroy (gizmo.GetComponent<Renderer>());
			}
			else
			{
				gizmoIsNotNull = false;
			}
			
			++gizmoIndex;
		}
		while (gizmoIsNotNull);
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	
	public void Init ()
	{		
		//check stair module
		ObjData objData = GetComponent<ObjData> ();
		if (objData != null)
		{
			//OSLibModules modules = objData.GetObjectModel ().GetModules ();
			_stairList = objData.GetObjectModel ().GetLibrary ().GetStairsList ()[0];
			
			if (_selectedStairIndex >= _stairList.GetStairList ().Count)
			{
				_selectedStairIndex = -1;
			}
			//OSLibModule stairModule = modules.FindModule ("stair");

			if (/*stairModule != null &&*/_gizmos.Count <= 0)
			{
				//find gizmos
				int gizmoIndex = 1;
				bool gizmoIsNotNull = true;
				
				do
				{
					Transform gizmo = transform.Find ("Gizmo" + gizmoIndex.ToString ());
					_transformGizmos.Add (gizmo);
					
					if (gizmo != null)
					{		
						StairGizmo g = new StairGizmo ();
						g.index = gizmoIndex - 1;
						g.position = gizmo.localPosition;
						g.rotation = gizmo.localRotation;
						
						_gizmos.Add (g);
					}
					else
					{
						gizmoIsNotNull = false;
					}
					
					++gizmoIndex;
				}
				while (gizmoIsNotNull);
			}
		}
	}
	
	public IEnumerator SwapPool (int gizmoIndex, int stairIndex)
	{		
		if (gizmoIndex >= _gizmos.Count)
		{
			yield return null;	
		}
			
		ObjData objData = GetComponent<ObjData> ();
		OSLibObject objModel = objData.GetObjectModel ();
		
		Montage.assetBundleLock = true;
		
		OSLib objLib = objModel.GetLibrary ();
		WWW www = WWW.LoadFromCacheOrDownload (objLib.GetAssetBundlePath (), objLib.GetVersion ());
		yield return www;

		AssetBundle assetBundle = www.assetBundle;
		
		int newStairIndex = stairIndex;
		string stairName = "";
		string path = "";
		
		int newGizmoIndex = gizmoIndex;
		
		if (stairIndex < 0)
		{
			newStairIndex = -1;
			path = objModel.GetModel ().GetBasePath ();
		}
		else
		{
			stairName = _stairList.GetStairList ()[stairIndex].GetStairType ();
			
			string baseModelPath = objModel.GetModel ().GetBasePath ();
			path = baseModelPath + "_" + stairName + "_" + (gizmoIndex + 1);
			bool exist = assetBundle.Contains (path);
			
			if (!exist)
			{
				newGizmoIndex = FindFirstGizmoAvailable (assetBundle, stairName);
				
				if (newGizmoIndex < 0)
				{
					path = baseModelPath;
				}
				else
				{
					path = baseModelPath + "_" + stairName + "_" + (newGizmoIndex + 1);	
				}
			}			
		}
		
		assetBundle.Unload (false);
		Montage.assetBundleLock = false;
		
		_selectedGizmoIndex = newGizmoIndex;
		_selectedStairIndex = newStairIndex;
	
		OSLibModel poolStairModel = new OSLibModel (path, objModel.GetModel ().GetBasePath ());
		OSLibObject poolStairObject = new OSLibObject (objModel.GetId (), 
			                                           objModel.GetObjectType (),
			                                           objModel.GetBrand (),
			                                           objModel.GetThumbnailPath (),
			                                           objModel.GetLibrary (),
			                                           poolStairModel,
			                                           objModel.GetModules (),
			                                           objModel.getCategory (),
			                                           objModel.IsMode2D (),
		                                               objModel.IsAllowedToScale (),
		                                               objModel.GetObjectScaleGenerale());
		poolStairObject.SetLanguages (objModel);
		
		Camera.main.GetComponent<ObjInteraction>().setSelected (gameObject, true);
		ObjInstanciation instantiator = GameObject.Find("MainNode").GetComponent<ObjInstanciation> ();	  
		yield return StartCoroutine (instantiator.swap (transform.position, poolStairObject, gameObject, -1, -1, -1));
		
	}
	
	public void SetGizmoIndex (int index)
	{
		_selectedGizmoIndex = index;
	}
	
	public int GetGizmoIndex ()
	{
		return _selectedGizmoIndex;
	}
	
	public void SetStairIndex (int index)
	{
		_selectedStairIndex = index;
	}
	
	public int GetStairIndex ()
	{
		return _selectedStairIndex;
	}
	
	public List<StairGizmo> GetGizmoList ()
	{
		return _gizmos;	
	}
	
	public void SetGizmoList (List<StairGizmo> list)
	{
		_gizmos = list;	
	}
	
	public int FindFirstGizmoAvailable (AssetBundle assetBundle, string stairName)
	{
		ObjData objData = GetComponent<ObjData> ();
		OSLibObject objModel = objData.GetObjectModel ();
		string baseModelPath = objModel.GetModel ().GetPath ();
		
		for (int gizmoIndex = 0; gizmoIndex < _gizmos.Count; ++gizmoIndex)
		{
			string path = baseModelPath + "_" + stairName + "_" + (gizmoIndex + 1);
			
			if (assetBundle.Contains (path))
			{
				return gizmoIndex;	
			}
		}
		
		return -1;
	}
	
	public OSLibStairs GetStairList ()
	{
		/*if (_stairList == null)
		{
			ObjData objData = GetComponent<ObjData> ();
			if (objData != null)
			{
				OSLibModules modules = objData.GetObjectModel ().GetModules ();
				_stairList = objData.GetObjectModel ().GetLibrary ().GetStairsList ()[0];
			}
		}*/
		
		return _stairList;
	}	
	
	public IEnumerator FindAvailableGizmos ()
	{
		if (_selectedStairIndex < 0)
		{
			_availableGizmos = _gizmos;
		}
		else
		{
			_availableGizmos = new List<StairGizmo> ();
			
			ObjData objData = GetComponent<ObjData> ();
			OSLibObject objModel = objData.GetObjectModel ();
	
			OSLib objLib = objModel.GetLibrary ();
			
			Montage.assetBundleLock = true;
			WWW www = WWW.LoadFromCacheOrDownload (objLib.GetAssetBundlePath (), objLib.GetVersion ());
			yield return www;
	
			AssetBundle assetBundle = www.assetBundle;
			
			string stairName = _stairList.GetStairList ()[_selectedStairIndex].GetStairType ();
			
			int gizmoIndex = 1;
			foreach (StairGizmo gizmo in _gizmos)
			{
				string baseModelPath = objModel.GetModel ().GetBasePath ();
				string path = baseModelPath + "_" + stairName + "_" + (gizmoIndex++);	
				
				if (assetBundle.Contains (path))
				{
					_availableGizmos.Add (gizmo);
				}
			}
			
			assetBundle.Unload (false);
			Montage.assetBundleLock = false;
		}
	}
	
	public List<StairGizmo> GetAvailableGizmos ()
	{
		return _availableGizmos;
	}
}