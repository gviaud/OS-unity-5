using UnityEngine;
using System.Collections;
using System.IO;

public enum PositionSpa{AboveGround = 0, HalfBurried = 1, Burried = 2};

public class FunctionAlt1_positionSpa : MonoBehaviour, Function_OS3D {
	
	public PositionSpa _position = PositionSpa.AboveGround;
	public int id;
	
	private PositionSpa _lastPosition = PositionSpa.AboveGround;
	private ArrayList _transforms = null;
	private ArrayList _zInits = null;
	private float _zUpValue=0.0f;
	private string _maskName="mask";
	private string _plageName="plage";
	private string _skirtName="jupe";
	private string _paroisName="paroi";
	private string _occlusionName="occlusion";
	private string _lowWallName="muret";
	private Vector3 _extents;
	private Vector3 _center;
	private string _functionName = "hors-sol";
	private string _alt1FunctionName = "semi-enterre";
	private string _alt2FunctionName = "enterre";
	private ArrayList _transformsNoPos = null;
	

	// Use this for initialization
	void Start ()
	{
		if(_transforms == null)
		{
			_transforms = new ArrayList();
			_transformsNoPos = new ArrayList();
			_zInits = new ArrayList();
			foreach (Transform child in transform)
			{	
				if(child.name.CompareTo(_maskName)==0)
					continue;
				if(child.name.CompareTo(_plageName)==0)
					continue;
				if(child.name.CompareTo(_occlusionName)==0)
					continue;	
				if(child.name.CompareTo(_lowWallName)==0)
					continue;				
				if(child.tag.CompareTo("noPos")==0)
				{
					_transformsNoPos.Add(child);
					continue;		
				}
				if(child.name.CompareTo(_skirtName)==0)
				{
					Bounds b = child.GetComponent<Renderer>().bounds;//getMeshBounds(child);					
					_zUpValue = b.extents.y*2;
					_extents = b.extents;
				}
				if(child.name.CompareTo(_paroisName)==0)
				{
					Bounds b = child.GetComponent<Renderer>().bounds;//getMeshBounds(child);					
					_zUpValue = b.extents.y*2;
					_extents = b.extents;
				}				
				_transforms.Add(child);
				_zInits.Add(child.localPosition.y);
				//Debug.Log ("child : "+child.name);
			}
			Init ();
		}
	}
	
	/*void OnDrawGizmosSelected() {
			Gizmos.color = Color.yellow;
			if(_transforms.Count>0)
				Gizmos.DrawWireCube(_center, _extents*2);
		}
	*/
	
	public void DoAction ()
	{
		Transform jupe = transform.Find("jupe");
		if (jupe!=null)
		{
			//Bounds b = getMeshBounds(jupe);
			Bounds b = jupe.GetComponent<Renderer>().bounds;//getMeshBounds(child);	
			_zUpValue = b.extents.y*2;
			_extents = b.extents;
			_center = b.center;
		}
		else
		{	
			Transform paroi = transform.Find("paroi");
			if (paroi!=null)
			{
				Bounds b = paroi.GetComponent<Renderer>().bounds;
				_zUpValue = b.extents.y*2;
				_extents = b.extents;
				_center = b.center;
			}
			
		}
		
		Vector3 vectDelta = new Vector3();
		switch (_lastPosition)
		{
			case PositionSpa.AboveGround :
				for (int i=0; i<_transforms.Count;i++) {	
					Transform child = (Transform)_transforms[i];
					vectDelta.y = /*child.localPosition.z*/ - _zUpValue/2;
					child.Translate(vectDelta);
				}
				_lastPosition=PositionSpa.HalfBurried;
			
			for (int i=0; i<_transformsNoPos.Count;i++) {	
				((Transform)_transformsNoPos[i]).gameObject.layer = 2;
			}
			break;			
				case PositionSpa.HalfBurried : 	
					for (int i=0; i<_transforms.Count;i++) {	
						Transform child = (Transform)_transforms[i];
						vectDelta.y = /*child.localPosition.z*/ - _zUpValue/2;
						child.Translate(vectDelta);
					}
					_lastPosition=PositionSpa.Burried;
			for (int i=0; i<_transformsNoPos.Count;i++) {	
				((Transform)_transformsNoPos[i]).gameObject.layer = 2;
			}
				break;
			case PositionSpa.Burried : 	
				for (int i=0; i<_transforms.Count;i++) {	
						Transform child = (Transform)_transforms[i];	
						vectDelta.y = /*child.localPosition.z*/ + _zUpValue;	
						child.Translate(vectDelta);
					}
					_lastPosition=PositionSpa.AboveGround;
			for (int i=0; i<_transformsNoPos.Count;i++) {	
				((Transform)_transformsNoPos[i]).gameObject.layer = 0;
			}
				break;
			default : 
				break;
		}
		_position = _lastPosition;	
	}
	// Update is called once per frame
	void Update ()
	{		

	}
	
	public void Init ()
	{
		Init (false);
	}
	public void Init (bool fromload)
	{		
		Vector3 vectDelta = new Vector3 ();
		if (_lastPosition != _position) {
			Transform jupe = transform.Find ("jupe");
			bool copy = false;
			if (jupe != null) {
			//	Bounds b = getMeshBounds (jupe);
				Bounds b = jupe.GetComponent<Renderer>().bounds;//getMeshBounds(child);		
				_zUpValue = b.extents.y * 2 ;// 2;
				_extents = b.extents;
				_center = b.center;
				if(jupe.position.y<-_zUpValue/3.0f)
					copy=true;
			}
			else
			{
				Transform paroi = transform.Find ("paroi");
				if (paroi != null) {
					Bounds b = paroi.GetComponent<Renderer>().bounds;
					_zUpValue = b.extents.y * 2 ;
					_extents = b.extents;
					_center = b.center;
					if(paroi.position.y<-_zUpValue/3.0f)
						copy=true;
				}
			}
				
			
			if((copy==false)|| (fromload==true))
			{
				switch (_position) {
				case PositionSpa.AboveGround:
					switch (_lastPosition) {
					case PositionSpa.HalfBurried:
						for (int i = 0; i < _transforms.Count; i++) {
							Transform child = (Transform)_transforms[i];
							vectDelta.y = +_zUpValue / 2;
							child.Translate (vectDelta);
						}
						_lastPosition = PositionSpa.AboveGround;
						break;
					case PositionSpa.Burried:
						for (int i = 0; i < _transforms.Count; i++) {
							Transform child = (Transform)_transforms[i];
							vectDelta.y = +_zUpValue;
							child.Translate (vectDelta);
						}
						_lastPosition = PositionSpa.AboveGround;
						break;
					default:
						break;
					}
					
				for (int i=0; i<_transformsNoPos.Count;i++) {			
					((Transform)_transformsNoPos[i]).gameObject.layer = 0;
				}
					break;
				case PositionSpa.HalfBurried:
					switch (_lastPosition) {
					case PositionSpa.AboveGround:
						for (int i = 0; i < _transforms.Count; i++) {
							Transform child = (Transform)_transforms[i];
							vectDelta.y = -_zUpValue / 2;
							child.Translate (vectDelta);
						}
						_lastPosition = PositionSpa.HalfBurried;
						break;
					case PositionSpa.Burried:
						for (int i = 0; i < _transforms.Count; i++) {
							Transform child = (Transform)_transforms[i];
							vectDelta.y = +_zUpValue/2;
							child.Translate (vectDelta);
						}
						_lastPosition = PositionSpa.HalfBurried;
						break;
					default:
						break;
					}
				for (int i=0; i<_transformsNoPos.Count;i++) {	
					((Transform)_transformsNoPos[i]).gameObject.layer = 2;
				}
					break;
				case PositionSpa.Burried:
					switch (_lastPosition) 
					{
					case PositionSpa.HalfBurried:
						for (int i = 0; i < _transforms.Count; i++) {
							Transform child = (Transform)_transforms[i];
							vectDelta.y = -_zUpValue / 2;
							child.Translate (vectDelta);
						}
						_lastPosition = PositionSpa.Burried;
						break;
					case PositionSpa.AboveGround:
						for (int i = 0; i < _transforms.Count; i++) {
							Transform child = (Transform)_transforms[i];
							vectDelta.y = -_zUpValue;
							child.Translate (vectDelta);
						}
						_lastPosition = PositionSpa.Burried;
						break;
					default:
						break;
					}
				for (int i=0; i<_transformsNoPos.Count;i++) {	
					((Transform)_transformsNoPos[i]).gameObject.layer = 2;
				}
					break;
				default:
					break;
				}
			}
		}
		_lastPosition = _position;
		
	}
	
	public string GetFunctionName()
	{	
		switch (_lastPosition)
		{
			case PositionSpa.AboveGround :
				return TextManager.GetText(_alt1FunctionName);
			break;
			case PositionSpa.HalfBurried :
				return TextManager.GetText(_alt2FunctionName);
			break;
			case PositionSpa.Burried :
				return TextManager.GetText(_functionName);
			break;
			default : 
				return null;
				break;
		}
	}
	
	public string GetFunctionParameterName()
	{
		return GetFunctionName();
	}
	
	public void setUI(FunctionUI_OS3D ui)
	{
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}
	
	public int GetFunctionId()
	{
		return id;
	}
	
	/*private Bounds getMeshBounds (Transform root)
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
	}
	*/
	//  SAVE/LOAD
	
	public void save(BinaryWriter buf)
	{
		int i = (int)_lastPosition;
	//	Debug.Log("LastPosition "+_lastPosition);
	/*	i--;
		if(i==-1)
			i = 2;*/
		
		buf.Write(i);
	}
	
	public void load(BinaryReader buf)
	{
		//start
		_transforms = new ArrayList();
		_zInits = new ArrayList();
		foreach (Transform child in transform)
		{	
			if(child.name.CompareTo(_maskName)==0)
				continue;
			if(child.name.CompareTo(_plageName)==0)
				continue;
			if(child.name.CompareTo(_occlusionName)==0)
				continue;
			if(child.name.CompareTo(_skirtName)==0)
			{
				//Bounds b = getMeshBounds(child);
				Bounds b = child.GetComponent<Renderer>().bounds;//getMeshBounds(child);		
				_zUpValue = b.extents.y*2;
				_extents = b.extents;
			}
			if(child.name.CompareTo(_paroisName)==0)
			{
				//Bounds b = getMeshBounds(child);
				Bounds b = child.GetComponent<Renderer>().bounds;//getMeshBounds(child);		
				_zUpValue = b.extents.y*2;
				_extents = b.extents;
			}
			_transforms.Add(child);
			_zInits.Add(child.localPosition.y);
			//Debug.Log ("child : "+child.name);
		}
		
		//load
		_lastPosition = PositionSpa.HalfBurried;
		_position = (PositionSpa)buf.ReadInt32();
	//	Debug.Log("LastPosition "+_lastPosition);
		Init (true);
		//DoAction();
	}

	public ArrayList getConfig()
	{
		ArrayList list = new ArrayList();
		list.Add(_position);
		return list;
	}
	
	public void setConfig(ArrayList config)
	{
		_position = (PositionSpa)config[0];
		_lastPosition = PositionSpa.HalfBurried;
		Init (true);
	}
	
}
