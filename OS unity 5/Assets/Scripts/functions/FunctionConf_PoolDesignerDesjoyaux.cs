using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class FunctionConf_PoolDesignerDesjoyaux : MonoBehaviour, Function_OS3D
{
	protected string _functionName = "Designer";
	
	protected FunctionUI_OS3D _ui;
	
	protected PoolGenerator _poolGen;
	
	protected Polygon _polygon;
	
	protected bool _isLoading = false;
	
	// polygone utilise pour la remise a zero
	[SerializeField]
	protected Point2Data[] _pointsDataBackup;
	
	[SerializeField]
	protected bool _IsGenerated = false; // pour ne pas regenerer la pisince lors de la copie
	
	[SerializeField]
	protected int _rimCount; // nombre de margelle
	
	[SerializeField]
	protected Texture2D _backgroundImage;
	
	[SerializeField]
	protected Rect _backgroundRect;
	
	[SerializeField]
	protected bool _backgroundImageVisible;
	
	public int id;
	
	public Point2Data[] pointsData;
	
	[SerializeField]
	public List<Point2Data[]> listStairwayPointsData;
	
	[HideInInspector]
	public Point2Data[] savedPointsData;
	
	[HideInInspector]
	public List<GameObject> go3DPosition;
	[HideInInspector]
	public GameObject goParent3DPosition;
	[HideInInspector]
	public GameObject goBloc;
	
	[HideInInspector]
	public List<DoubleVector3> listDoubleV3 = new List<DoubleVector3>();
	[HideInInspector]
	private int idDoubleV3 = 0;
	[HideInInspector]
	private Vector3 v3positionBloc = Vector3.zero;
	
	[HideInInspector]
	public float fstairwayPosition = 0.0f;
	private float fcurrentTemp = 25.0f;
	private int icurrentPointStairway = -1;
	[HideInInspector]
	public bool bstairway = false;
	private float frailStairway = 0.0f;
	
	[HideInInspector]
	public bool broman = false;
	[HideInInspector]
	public float fsizeStairway = 0.0f;
	[HideInInspector]
	public float fradiusStairway = 0.0f;
	
	[HideInInspector]
	public float fgoBlocPosition = 0.0f;
	[HideInInspector]
	public float fmagnitudeRail = 0.0f;
	private float fcurrentMagnitudeRail = 0.0f;
	
	[HideInInspector]
	public float fscale = 0.0f;
	
	[HideInInspector]
	public bool bcenter = false;
	[HideInInspector]
	public bool balwaysCenter = false;
	
	private bool bnewStairwayId = false;
	private float _flimitSize = 100.0f;
	
	void Awake ()
	{
		transform.parent = GameObject.Find ("MainNode").transform;
			
		transform.localScale = Vector3.one;
		
		goParent3DPosition = new GameObject();
		//goParent3DPosition.transform.parent = GameObject.Find ("MainNode").transform;
		goParent3DPosition.name = "3Dposition_" + name;
		
		goBloc = GameObject.Instantiate(Resources.Load("GRI181")) as GameObject;
		goBloc.transform.parent = goParent3DPosition.transform;
		goBloc.GetComponent<Renderer>().enabled = false;
		goBloc.name = "GRI181";
		
		foreach(Transform child in goBloc.transform)
		{
			child.gameObject.layer = 14;  // underwater
			
			if(child.GetComponent<Renderer>())
			{
				child.GetComponent<Renderer>().enabled = false;
			}
		}
		
		_poolGen = new PoolGenerator (gameObject);
		
		if (pointsData.Length > 0)
		{
		//	pointsData = MirrorX(pointsData);
			_polygon = new Polygon (pointsData);
			
			if (!_IsGenerated)
			{
				_IsGenerated = true;
				
				_pointsDataBackup = new Point2Data[pointsData.Length];
				_polygon.CopyPoint2DataTo (_pointsDataBackup);
				
				_poolGen.Generate (_polygon);
				_rimCount = _poolGen.GetRimCount ();
				
				DesignerDesjoyaux fb = gameObject.AddComponent<DesignerDesjoyaux> ();
				fb.gofiltrationBlock = goBloc;
				fb.poolDesigner = this;
				
				Function_hideObject sidewalkHider = gameObject.AddComponent<Function_hideObject> ();
				sidewalkHider._nameObjectToHide = "plage";
				sidewalkHider._strObjectToHide = "plage";
				
				Function_hideObject rimHider = gameObject.AddComponent<Function_hideObject> ();
				rimHider.id = 1;
				rimHider._nameObjectToHide = "margelle";
				rimHider._strObjectToHide = "margelle";
				rimHider._hide = false;
				rimHider.SetObjectToHide (_poolGen.GetRimObjects ());
				
				gameObject.AddComponent<Function_PoolInitializer> ();
				//StartCoroutine (Generate ());
			}
			else
			{
				// copie
				_isLoading = true;
				StartCoroutine (Generate (_polygon));
			}
		}
		else
		{
			_IsGenerated = true;
			_polygon = new Polygon ();
			
			DesignerDesjoyaux fb = gameObject.AddComponent<DesignerDesjoyaux> ();
			fb.gofiltrationBlock = goBloc;
			fb.poolDesigner = this;
			
			Function_hideObject sidewalkHide = gameObject.AddComponent<Function_hideObject> ();
			sidewalkHide._nameObjectToHide = "plage";
			sidewalkHide._strObjectToHide = "plage";
			
			Function_hideObject rimHider = gameObject.AddComponent<Function_hideObject> ();
			rimHider.id = 1;
			rimHider._nameObjectToHide = "margelle";
			rimHider._strObjectToHide = "margelle";
			rimHider._hide = false;
			rimHider.SetObjectToHide (_poolGen.GetRimObjects ());
			
			gameObject.AddComponent<Function_PoolInitializer> ();
			
			//StartCoroutine (WaitForGameobjectReadyToDoAction ());
		}
		
		init3DPoints();
		
		nextCurrentStairwayID(pointsData);
		
		savedPointsData = new Point2Data[pointsData.Length];
		for(int j = 0; j < pointsData.Length; j++)
		{
			savedPointsData[j] = pointsData[j];
		}
		
		if(listDoubleV3.Count > 0)
		{
			idDoubleV3 = 0;
			v3positionBloc = listDoubleV3[0].v3start;
			goBloc.transform.position = v3positionBloc;
			
			moveBloc();
		}
		else
		{
			idDoubleV3 = -1;
		}
	}
	
	void OnDestroy()
	{
		Destroy(goParent3DPosition);
	}
	
	// on fait le DoAction une fois que le gameObject et ces donnees soit pret
	private IEnumerator WaitForGameobjectReadyToDoAction ()
	{
		ObjData data = GetComponent<ObjData> ();
		ObjInteraction objI = Camera.main.GetComponent<ObjInteraction>();
		while (data == null || data.GetObjectModel () == null || objI.getSelected () != gameObject)
		{
			yield return new WaitForEndOfFrame ();
			data = GetComponent<ObjData> ();
		}
		
		if (pointsData.Length <= 0)
		{
		//	DoAction ();
		}
		
		yield return true;
	}
		
	// Use this for initialization
	void Start () 
	{	
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		goParent3DPosition.transform.rotation = transform.rotation;
		goParent3DPosition.transform.position = transform.position;
		goParent3DPosition.transform.localScale = transform.localScale * 0.1f;
		
		// on masque le mesh margelle utilise par le PoolInitializer
		Transform rim = _poolGen.GetParentGameObject ().transform.FindChild ("margelle");
		rim.GetComponent<Renderer>().enabled = false;
		
		Transform liner = _poolGen.GetParentGameObject ().transform.FindChild ("liner");
		liner.GetComponent<Renderer>().enabled = true;
		
		Transform frieze = _poolGen.GetParentGameObject ().transform.FindChild ("frise");
		frieze.GetComponent<Renderer>().enabled = true;
		
		Transform water = _poolGen.GetParentGameObject ().transform.FindChild ("water");
		water.GetComponent<Renderer>().enabled = true;
	}
	
	public Vector2 computeCenter()
	{
		float minX = float.MaxValue;
		float maxX = float.MinValue;
		float minY = float.MaxValue;
		float maxY = float.MinValue;
		
		foreach (Point2 pt2 in _polygon.GetPoints())
		{			
			if (pt2.GetJunction () == JunctionType.Broken)
			{
				Vector2 pt = pt2;
				
				if (pt.x > maxX)
				{
					maxX = pt.x;
				}
				
				if (pt.x < minX)
				{
					minX = pt.x;
				}
				
				if (pt.y > maxY)
				{
					maxY = pt.y;
				}
				
				if (pt.y < minY)
				{
					minY = pt.y;
				}
			}
			else if (pt2.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 aPt = pt2 as ArchedPoint2;
				
				foreach (Vector2 pt in aPt.GetCurve ())
				{
					if (pt.x > maxX)
					{
						maxX = pt.x;
					}
					
					if (pt.x < minX)
					{
						minX = pt.x;
					}
					
					if (pt.y > maxY)
					{
						maxY = pt.y;
					}
					
					if (pt.y < minY)
					{
						minY = pt.y;
					}	
				}
			}
		}
		
		return new Vector2 (((maxX + minX) * 0.5f), (maxY + minY) * 0.5f);
	}
	
	public void reinitGo3D()
	{
		foreach(GameObject go in go3DPosition)
		{
			Destroy(go);
		}
		
		go3DPosition.Clear();
		
		foreach(Point2Data p2d in pointsData)
		{
			GameObject go = new GameObject();
			go.transform.parent = goParent3DPosition.transform;
			go3DPosition.Add(go);
		}
	}
	
	public void init3DPoints()
	{
		reinitGo3D();
		
		Vector2 v2center = computeCenter();
		Vector3 v3center = new Vector3 (v2center.x, 0, -v2center.y) * 0.1f;
		
		int i = 0;
		
		goParent3DPosition.transform.position = v3center;
		
		i = 0;
		
		foreach(Point2Data p2d in pointsData)
		{
			Vector3 v3position = new Vector3(p2d.position.x, 0.0f, -p2d.position.y) * 0.1f;
			go3DPosition[i++].transform.localPosition = v3position - v3center;
		}
		
		goParent3DPosition.transform.position = transform.position;
		
		reinitRail(pointsData);
	}
	
	/*void OnDrawGizmos()
	{
		foreach(GameObject go in go3DPosition)
		{
			Gizmos.DrawCube(go.transform.position, Vector3.one * 0.5f);
		}
	}*/
	
	public void nextCurrentStairwayID(Point2Data[] _pointsData)
	{
		bool bcheck = false;
		do
		{
			icurrentPointStairway++;
			
			if(icurrentPointStairway >= _pointsData.Length)
			{
				icurrentPointStairway = 0;
			}
			
			if(icurrentPointStairway + 1 >= _pointsData.Length)
			{
				if(_pointsData[0].junctionType == JunctionType.Broken &&
				   _pointsData[icurrentPointStairway].junctionType == JunctionType.Broken)
				{
					bnewStairwayId = true;
					bcheck = true;
					
					if((_pointsData[icurrentPointStairway].position - _pointsData[0].position).magnitude < fsizeStairway + _flimitSize)
					{
						bcenter = true;
					}
				}
			}
			else if(_pointsData[icurrentPointStairway + 1].junctionType == JunctionType.Broken &&
			        _pointsData[icurrentPointStairway].junctionType == JunctionType.Broken)
			{
				bnewStairwayId = true;
				bcheck = true;
				
				if((_pointsData[icurrentPointStairway].position - _pointsData[icurrentPointStairway + 1].position).magnitude < fsizeStairway + _flimitSize)
				{
					bcenter = true;
				}
			}
			
		}while(!bcheck);
	}
	
	public void previousCurrentStairwayID(Point2Data[] _pointsData)
	{
		bool bcheck = false;
		do
		{
			icurrentPointStairway--;
			
			if(icurrentPointStairway < 0)
			{
				icurrentPointStairway = _pointsData.Length - 1;
			}
			
			if(icurrentPointStairway + 1 >= _pointsData.Length)
			{
				if(_pointsData[0].junctionType == JunctionType.Broken &&
				   _pointsData[icurrentPointStairway].junctionType == JunctionType.Broken)
				{
					bnewStairwayId = true;
					bcheck = true;
					
					if((_pointsData[icurrentPointStairway].position - _pointsData[0].position).magnitude < fsizeStairway + _flimitSize)
					{
						bcenter = true;
					}
				}
			}
			else if(_pointsData[icurrentPointStairway + 1].junctionType == JunctionType.Broken &&
			        _pointsData[icurrentPointStairway].junctionType == JunctionType.Broken)
			{
				bnewStairwayId = true;
				bcheck = true;
				
				if((_pointsData[icurrentPointStairway].position - _pointsData[icurrentPointStairway + 1].position).magnitude < fsizeStairway + _flimitSize)
				{
					bcenter = true;
				}
			}
			
		}while(!bcheck);
	}
	
	public bool canAddStairway(Point2Data[] _p2d, float _fsize = 276.0f)
	{
		List<Point2Data> listPointsData = new List<Point2Data>();
		
		int iid = 0;
		
		bool bcheck = false;
		
		if(savedPointsData != null && savedPointsData.Length > 0)
		{
			foreach(Point2Data p2d in savedPointsData)
			{
				listPointsData.Add(p2d);
			}
		}
		else
		{
			foreach(Point2Data p2d in pointsData)
			{
				listPointsData.Add(p2d);
			}
		}
		
		foreach(Point2Data p2d in listPointsData)
		{
			int iidNextPoint = iid + 1 >= listPointsData.Count ? 0 : iid + 1;
			
			if(listPointsData[iidNextPoint].junctionType == JunctionType.Broken &&
			   listPointsData[iid].junctionType == JunctionType.Broken)
			   {
					float fmagnitude = (listPointsData[iidNextPoint].position - listPointsData[iid].position).magnitude;
					
					if(fmagnitude > _fsize)
					{
						bcheck = true;
					}
				}
			
			iid++;
		}
		
		return bcheck;
	}
	
	public void checkIfAlwaysCenter(Point2Data[] _p2d, float _fsize = 276.0f)
	{
		List<Point2Data> listPointsData = new List<Point2Data>();
		
		int iid = 0;
		
		balwaysCenter = true;
		
		if(savedPointsData != null && savedPointsData.Length > 0)
		{
			foreach(Point2Data p2d in savedPointsData)
			{
				listPointsData.Add(p2d);
			}
		}
		else
		{
			foreach(Point2Data p2d in pointsData)
			{
				listPointsData.Add(p2d);
			}
		}
		
		foreach(Point2Data p2d in listPointsData)
		{
			int iidNextPoint = iid + 1 >= listPointsData.Count ? 0 : iid + 1;
			
			if(listPointsData[iidNextPoint].junctionType == JunctionType.Broken &&
			   listPointsData[iid].junctionType == JunctionType.Broken)
			{
				float fmagnitude = (listPointsData[iidNextPoint].position - listPointsData[iid].position).magnitude;
				
				if(fmagnitude > (_fsize + _flimitSize))
				{
					balwaysCenter = false;
					return;
				}
			}
			
			iid++;
		}
	}
	
	public IEnumerator addStairway(Point2Data[] _p2d, bool _bgenerate, int _isens, float _fsize = 276.0f, float _fradius = 84.0f)
	{
		gameObject.GetComponent<DesignerDesjoyaux>().blockStairway = true;
		
		fsizeStairway = _fsize;
		fradiusStairway = _fradius;
		
		List<Point2Data> listPointsData = new List<Point2Data>();
		
		if(savedPointsData != null && savedPointsData.Length > 0)
		{
			foreach(Point2Data p2d in savedPointsData)
			{
				listPointsData.Add(p2d);
			}
		}
		else
		{
			foreach(Point2Data p2d in pointsData)
			{
				listPointsData.Add(p2d);
			}
			
			savedPointsData = new Point2Data[listPointsData.Count];
			for(int j = 0; j < listPointsData.Count; j++)
			{
				savedPointsData[j] = listPointsData[j];
			}
		}
		
		int iidNextPoint = icurrentPointStairway + 1 >= listPointsData.Count ? 0 : icurrentPointStairway + 1;
		float fmagnitude = (listPointsData[iidNextPoint].position - listPointsData[icurrentPointStairway].position).magnitude;
		Vector2 v2tempEnd = listPointsData[iidNextPoint].position;
		Vector2 v2tempFirst = listPointsData[icurrentPointStairway].position;
		Vector2 v = (listPointsData[iidNextPoint].position - listPointsData[icurrentPointStairway].position);
		v.Normalize();
		Vector2 v2perpendicularDirection = new Vector2(-v.y, v.x) / Mathf.Sqrt((v.x * v.x) + (v.y * v.y));
		
		int i = icurrentPointStairway + 1;
		
		Vector2 v2first = Vector2.zero;
		Vector2 v2end = Vector2.zero;
		
		
		if(!bcenter)
		{
			float fspeed = 600.0f;
			
			if(bnewStairwayId)
			{
				fspeed = 1.0f;
				bnewStairwayId = false;
			}
			
			fcurrentTemp += Time.deltaTime * _isens * fspeed;
			
			float ftps = 10.0f;
			fcurrentTemp = Mathf.Clamp(fcurrentTemp, ftps, fmagnitude - fsizeStairway - ftps);
		}
		else
		{
			checkIfAlwaysCenter(pointsData, fsizeStairway);
			fcurrentTemp = (fmagnitude * 0.5f) - (_fsize * 0.5f);
			bcenter = false;
		}
		
		if(!broman)
		{
			Point2Data newp2d = new Point2Data();
			newp2d.junctionType = JunctionType.Broken;
			newp2d.position = listPointsData[icurrentPointStairway].position + (v * fcurrentTemp);
			newp2d.radius = 0.0f;
			listPointsData.Insert(i++, newp2d);
			
			v2first = newp2d.position;
			
			newp2d = new Point2Data();
			newp2d.bstairway = true;
			newp2d.junctionType = JunctionType.Broken;
			newp2d.position = listPointsData[i - 1].position + (-v2perpendicularDirection * -165.0f);
			newp2d.radius = 0.0f;
			listPointsData.Insert(i++, newp2d);
			
			newp2d = new Point2Data();
			newp2d.bstairway = true;
			newp2d.junctionType = JunctionType.Broken;
			newp2d.position = listPointsData[i - 1].position + (v * _fsize);
			newp2d.radius = 0.0f;
			listPointsData.Insert(i++, newp2d);
			
			newp2d = new Point2Data();
			newp2d.junctionType = JunctionType.Broken;
			newp2d.position = listPointsData[i - 1].position + (v2perpendicularDirection * -165.0f);
			newp2d.radius = 0.0f;
			listPointsData.Insert(i++, newp2d);
			
			v2end = newp2d.position;
		}
		else
		{
			Point2Data newp2d = new Point2Data();
			newp2d.junctionType = JunctionType.Broken;
			newp2d.position = listPointsData[icurrentPointStairway].position + (v * fcurrentTemp);
			newp2d.radius = 0.0f;
			listPointsData.Insert(i++, newp2d);
			
			v2first = newp2d.position;
			
			newp2d = new Point2Data();
			newp2d.bstairway = true;
			newp2d.junctionType = JunctionType.Broken;
			newp2d.position = listPointsData[i - 1].position + (v2perpendicularDirection * 10.0f);
			newp2d.radius = 0.0f;
			listPointsData.Insert(i++, newp2d);
			
			newp2d = new Point2Data();
			newp2d.bstairway = true;
			newp2d.junctionType = JunctionType.Curved;
			newp2d.position = listPointsData[i - 1].position + (v * (_fsize * 0.5f)) - (v2perpendicularDirection * -1000.0f);
			newp2d.radius = _fradius;
			listPointsData.Insert(i++, newp2d);
			
			newp2d = new Point2Data();
			newp2d.bstairway = true;
			newp2d.junctionType = JunctionType.Broken;
			newp2d.position = listPointsData[i - 1].position + (v * (_fsize * 0.5f)) + (v2perpendicularDirection * -1000.0f);
			newp2d.radius = 0.0f;
			listPointsData.Insert(i++, newp2d);
			
			newp2d = new Point2Data();
			newp2d.junctionType = JunctionType.Broken;
			newp2d.position = listPointsData[i - 1].position + (-v2perpendicularDirection * 10.0f);
			newp2d.radius = 0.0f;
			listPointsData.Insert(i++, newp2d);
			
			v2end = newp2d.position;
		}
		
		float foffset = 25.0f;
		float fnewMagnitude = (v2tempFirst - v2tempEnd).magnitude - foffset;
		
		if((_isens == 1 && fnewMagnitude < ((v2tempFirst - v2end).magnitude)) || balwaysCenter)
		{
			nextCurrentStairwayID(savedPointsData);
			fcurrentTemp = foffset;
		}
		else if(_isens == -1 && fnewMagnitude < ((v2tempEnd - v2first).magnitude))
		{
			previousCurrentStairwayID(savedPointsData);
			iidNextPoint = icurrentPointStairway + 1 >= savedPointsData.Length ? 0 : icurrentPointStairway + 1;
			fcurrentTemp = Mathf.Abs((savedPointsData[icurrentPointStairway].position - savedPointsData[iidNextPoint].position).magnitude) - _fsize - foffset;
		}
		
		Point2Data[] p2ds = new Point2Data[listPointsData.Count];
		
		for(int j = 0; j < listPointsData.Count; j++)
		{
			p2ds[j] = listPointsData[j];
		}
		
		_polygon = new Polygon(p2ds);
		
		if(_bgenerate)
		{
			StartCoroutine (Generate (_polygon));
			
			yield return new WaitForEndOfFrame();
			
			init3DPoints();
			
			if(listDoubleV3.Count > 0)
			{
				idDoubleV3 = 0;
				v3positionBloc = listDoubleV3[0].v3start;
				goBloc.transform.localPosition = v3positionBloc;
				
				fgoBlocPosition = 5.0f;
				fcurrentMagnitudeRail = 0.0f;
			}
		}
		
		bstairway = true;
	}
	
	public IEnumerator removeStairway()
	{
		bstairway = false;
		
		Vector2 v2center = computeCenter();
		Vector3 v3center = new Vector3 (v2center.x, 0, -v2center.y) * 0.1f;
		
		int i = 0;
		
		if(savedPointsData != null && savedPointsData.Length > 0)
		{
			_polygon = new Polygon(savedPointsData);
			StartCoroutine (Generate (_polygon));
			
			yield return new WaitForEndOfFrame();
			
			savedPointsData = null;
		}
		
		goParent3DPosition.transform.position = v3center;
		
		i = 0;
		foreach(Point2Data p2d in pointsData)
		{
			Vector3 v3position = new Vector3(p2d.position.x, 0.0f, -p2d.position.y) * 0.1f;
			go3DPosition[i++].transform.localPosition = v3position - v3center;
		}
		
		goParent3DPosition.transform.position = transform.position;
		
		_polygon = new Polygon(pointsData);
		
		StartCoroutine (Generate (_polygon));
		
		yield return new WaitForEndOfFrame();
		
		init3DPoints();
		
		if(listDoubleV3.Count > 0)
		{
			idDoubleV3 = 0;
			v3positionBloc = listDoubleV3[0].v3start;
			goBloc.transform.localPosition = v3positionBloc;
			
			fgoBlocPosition = 5.0f;
			fcurrentMagnitudeRail = 0.0f;
		}
	}
	
	public IEnumerator scalePool(int _isens)
	{
		List<float> listRadiusPercentage = new List<float>();
		
		Vector2 v2center = computeCenter();
		Vector3 v3center = new Vector3 (v2center.x, 0, -v2center.y) * 0.1f;
		
		int i = 0;
		
		if(savedPointsData != null && savedPointsData.Length > 0)
		{
			_polygon = new Polygon(savedPointsData);
			
			StartCoroutine (Generate (_polygon));
			
			yield return new WaitForEndOfFrame();
			
			savedPointsData = null;
		}
		
		foreach(Point2 p2 in _polygon.GetPoints())
		{
			ArchedPoint2 ar = p2 as ArchedPoint2;
			
			if(ar != null)
			{
				listRadiusPercentage.Add((100.0f * ar.GetMeasuredRadius ()) / ar.GetMaxRadius ());
			}
		}
		
		goParent3DPosition.transform.position = v3center;
		
		i = 0;
		fscale += 0.25f * _isens;
		foreach(Point2Data p2d in pointsData)
		{
			Vector2 v2direction = p2d.position - v2center;
			
			float fpowerScale = 0.1f * 0.52f;
			
			if(_isens < 0)
			{
				fpowerScale= 0.09075f * 0.52f;
			}
			p2d.position += v2direction * fpowerScale * _isens;
			
			Vector3 v3position = new Vector3(p2d.position.x, 0.0f, -p2d.position.y) * 0.1f;
			go3DPosition[i++].transform.localPosition = v3position - v3center;
		}
		
		goParent3DPosition.transform.position = transform.position;
		
		_polygon = new Polygon(pointsData);
		
		i = 0;
		foreach(Point2 p2 in _polygon.GetPoints())
		{
			ArchedPoint2 ar = p2 as ArchedPoint2;
			
			if(ar != null)
			{
				float fnewMeasuredRadius = (listRadiusPercentage[i++] * ar.GetMaxRadius ()) * 0.01f;
				
				ar.SetMeasuredRadius(fnewMeasuredRadius);
			}
		}
		
		frailStairway = 0.0f;
		
		StartCoroutine (Generate (_polygon));
		
		yield return new WaitForEndOfFrame();
		
		if(bstairway && canAddStairway(pointsData, fsizeStairway))
		{
			if(canAddStairway(pointsData, fsizeStairway))
			{
				StartCoroutine (addStairway(null, true, 0, fsizeStairway, fradiusStairway));
			}
			else
			{
				StartCoroutine(removeStairway());
			}	
		}
		
		init3DPoints();
		
		if(listDoubleV3.Count > 0)
		{
			idDoubleV3 = 0;
			v3positionBloc = listDoubleV3[0].v3start;
			goBloc.transform.localPosition = v3positionBloc;
			
			fgoBlocPosition = 5.0f;
			fcurrentMagnitudeRail = 0.0f;
		}
	}
	
	public void reinitRail(Point2Data[] _pointsData)
	{
		listDoubleV3.Clear();
		
		fmagnitudeRail = 0.0f;
		
		float fminSize = 2.5f;
		
		for(int l = 1; l < _pointsData.Length || l < go3DPosition.Count; l++)
		{
			tryToAddDoubleVector3(_pointsData, l - 1, l);
		}
		
		tryToAddDoubleVector3(_pointsData, pointsData.Length - 1, 0);
	}
	
	public void tryToAddDoubleVector3(Point2Data[] _pointsData, int _istart, int _iend)
	{
		float fminSize = 2.5f;
		
		if(_istart >= 0 && !_pointsData[_istart].bstairway && !_pointsData[_iend].bstairway &&
		   _pointsData[_istart].junctionType == JunctionType.Broken &&
		   _pointsData[_iend].junctionType == JunctionType.Broken)
		{
			DoubleVector3 dv3 = new DoubleVector3();
			
			dv3.v3start = go3DPosition[_istart].transform.localPosition;
			dv3.v3end = go3DPosition[_iend].transform.localPosition;
			
			float ffirstmagnitude = (dv3.v3start - dv3.v3end).magnitude;
			
			if(ffirstmagnitude > fminSize)
			{
				Vector3 v3direction = dv3.v3end - dv3.v3start;
				v3direction.Normalize();
				
				dv3.v3start += v3direction * 5.0f;
				dv3.v3end -= v3direction * 5.0f;
				
				Vector3 v3direction2 = dv3.v3end - dv3.v3start;
				v3direction2.Normalize();
				if(Vector3.Dot(v3direction2, v3direction) == 1.0f)
				{
					fmagnitudeRail += (dv3.v3end - dv3.v3start).magnitude;
					
					listDoubleV3.Add(dv3);
				}
			}
		}
	}
	
	public void reinitBloc()
	{
		fcurrentMagnitudeRail = 0.0f;
	}
	
	public void moveBloc()
	{
		reinitRail(pointsData);
		
		Vector3 v3direction = listDoubleV3[idDoubleV3].v3end - listDoubleV3[idDoubleV3].v3start;
		v3direction.Normalize();
		goBloc.transform.localPosition = listDoubleV3[idDoubleV3].v3start + (v3direction * (fgoBlocPosition - fcurrentMagnitudeRail));
		goBloc.transform.localPosition = new Vector3(goBloc.transform.localPosition.x, 0.0f, goBloc.transform.localPosition.z);
		
		if(((listDoubleV3[idDoubleV3].v3end - listDoubleV3[idDoubleV3].v3start).magnitude + fcurrentMagnitudeRail) < fgoBlocPosition &&
		   idDoubleV3 < listDoubleV3.Count - 1)
		{
			fcurrentMagnitudeRail += (listDoubleV3[idDoubleV3].v3end - listDoubleV3[idDoubleV3].v3start).magnitude;
			idDoubleV3++;
			goBloc.transform.localPosition = listDoubleV3[idDoubleV3].v3start;
		}
		else if(fgoBlocPosition < fcurrentMagnitudeRail && idDoubleV3 > 0)
		{
			idDoubleV3--;
			fcurrentMagnitudeRail -= (listDoubleV3[idDoubleV3].v3end - listDoubleV3[idDoubleV3].v3start).magnitude;
			goBloc.transform.localPosition = listDoubleV3[idDoubleV3].v3end;
		}
		
		v3direction = listDoubleV3[idDoubleV3].v3end - listDoubleV3[idDoubleV3].v3start;
		rotateBloc(v3direction);
	}
	
	public void rotateBloc(Vector3 _v3direction)
	{
		Vector2 v = new Vector2(_v3direction.x, _v3direction.z);
		v.Normalize();
		Vector2 v2perpendicularDirection = new Vector2(-v.y, v.x) / Mathf.Sqrt((v.x * v.x) + (v.y * v.y));
		goBloc.transform.localRotation = Quaternion.LookRotation(new Vector3(v2perpendicularDirection.x, 0.0f, v2perpendicularDirection.y));
		goBloc.transform.Rotate(270.0f, 90.0f, 0.0f);
	}
	
	public void DoAction () 
	{
		DesignerDesjoyaux fc_dd = GetComponent<DesignerDesjoyaux>();
		
		if(fc_dd)
		{
			fc_dd.DoAction();
		}
	}

	public void setUI(FunctionUI_OS3D ui)
	{
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}
	
	public string GetFunctionName()
	{
		return GetFunctionParameterName();
	}
	
	public string GetFunctionParameterName()
	{
		return " " + TextManager.GetText(_functionName);
	}
	
	public int GetFunctionId()
	{
		return id;
	}
	
	public ArrayList getConfig()
	{
		return new ArrayList ();
	}
	
	public void setConfig(ArrayList config)
	{
		
	}
	
	public void save (BinaryWriter buf)
	{	int pointCount = _polygon.GetPoints ().Count;
		buf.Write (pointCount);
		
		Point2Data [] savedPolygon = new Point2Data[pointCount];
		_polygon.CopyPoint2DataTo (savedPolygon);
		
		foreach (Point2Data pData in savedPolygon)
		{
			buf.Write ((double)pData.position.x);
			buf.Write ((double)pData.position.y);
			buf.Write ((int)pData.junctionType);
			buf.Write ((double)pData.radius);
			buf.Write ((bool)pData.bstairway);
		}
		
		buf.Write ((double)fcurrentMagnitudeRail);
		buf.Write ((double)fcurrentTemp);
		buf.Write ((double)fscale);
		
		buf.Write ((bool)bstairway);
		buf.Write ((bool)broman);
		buf.Write ((double)fsizeStairway);
		buf.Write ((double)fradiusStairway);
		
		//buf.Write ((string) goBloc.name);
		buf.Write ((bool)goBloc.GetComponent<Renderer>().enabled);
		buf.Write ((double)goBloc.transform.position.x);
		buf.Write ((double)goBloc.transform.position.y);
		buf.Write ((double)goBloc.transform.position.z);
		buf.Write ((double)goBloc.transform.localRotation.eulerAngles.x);
		buf.Write ((double)goBloc.transform.localRotation.eulerAngles.y);
		buf.Write ((double)goBloc.transform.localRotation.eulerAngles.z);
	}
	
	public void load (BinaryReader buf)
	{
		int pointCount = buf.ReadInt32 ();
		pointsData = new Point2Data[pointCount];
		_pointsDataBackup = new Point2Data[pointCount];
		
		for (int pIndex = 0; pIndex < pointCount; ++pIndex)
		{
			Point2Data pData = new Point2Data ();
			pData.position = new Vector2 ((float)buf.ReadDouble (), (float)buf.ReadDouble ());
			pData.junctionType = (JunctionType)buf.ReadInt32 ();
			pData.radius = (float)buf.ReadDouble ();
			pData.bstairway = (bool)buf.ReadBoolean ();
			
			pointsData[pIndex] = pData;	
			_pointsDataBackup[pIndex] = pData;
		}
		
		fcurrentMagnitudeRail = (float)buf.ReadDouble();
		fcurrentTemp = (float)buf.ReadDouble();
		fscale = (float)buf.ReadDouble();
		bstairway = (bool)buf.ReadBoolean();
		broman = (bool)buf.ReadBoolean();
		fsizeStairway = (float)buf.ReadDouble();
		fradiusStairway = (float)buf.ReadDouble();
		
		//goBloc = GameObject.Instantiate(Resources.Load((string)buf.ReadString ())) as GameObject;
		bool bblocRenderer = (bool)buf.ReadBoolean ();
		
		goBloc.GetComponent<Renderer>().enabled = bblocRenderer;
		
		foreach(Transform child in goBloc.transform)
		{
			child.GetComponent<Renderer>().enabled = bblocRenderer;
			child.gameObject.layer = 14;  // underwater
		}
		
		goBloc.transform.position = new Vector3((float)buf.ReadDouble(), (float)buf.ReadDouble(), (float)buf.ReadDouble());
		goBloc.transform.localRotation = Quaternion.Euler((float)buf.ReadDouble(), (float)buf.ReadDouble(), (float)buf.ReadDouble());
		
		_polygon = new Polygon (pointsData);
		_isLoading = true;
		_IsGenerated = true;
		StartCoroutine (Generate (_polygon));
		
		scalePool(0);
		
		//init3DPoints();
		
		//moveBloc();
	}
		
	/**
	 * Fait une opération de mirroire selon l'axe X de la liste des points (X(n) = -x(n))
	 */
	private Point2Data[] MirrorX(Point2Data[] previousPoints)
	{
		Point2Data[] newPointsData;
		newPointsData = new Point2Data[previousPoints.Length];
		for (int i=0;i<previousPoints.Length;i++)
		{
			Point2Data pData = new Point2Data ();
			//Point2Data pDataOrigine =previousPoints[previousPoints.Length-1-i];
			Point2Data pDataOrigine =previousPoints[i];
			pData.junctionType = pDataOrigine.junctionType;
						
			/*if (pData.junctionType == JunctionType.Curved)
			{
				ArchedPoint2 aPoint = pDataOrigine as ArchedPoint2;
				pData.radius = aPoint.GetMeasuredRadius ();
			}*/
			pData.radius = pDataOrigine.radius;
			pData.position.Set(
				pDataOrigine.position.x,
				pDataOrigine.position.y); 
			newPointsData[i] = pData;
		}

		return newPointsData;		
	}
	
	public IEnumerator Generate (Polygon polygon)
	{
		GameObject pool = _poolGen.GetParentGameObject ();
		
		if (_isLoading)
		{
			// lors de la copie, on attend le frame suivante
			// pour que la copie soit complete
			_isLoading = false;
			yield return new WaitForEndOfFrame();
		}
		
		// on conserve les donnees des Function_hideObject de la plage et du Function_PoolInitializer
		// avant de les detruire et de les reaffecter
		Function_hideObject [] hiders = pool.GetComponents<Function_hideObject> ();
		
		Function_hideObject hider = hiders[0].id == 0 ? hiders[0] : hiders[1];
		ArrayList hiderConfig = hider.getConfig ();
			
		if (hider != null)
		{
			Destroy (hider);
		}
		
		Function_hideObject rimHider = hiders[0].id == 1 ? hiders[0] : hiders[1];
		ArrayList rimHiderConfig = rimHider.getConfig ();
			
		if (rimHider != null)
		{
			Destroy (rimHider);
		}
		
		Function_PoolInitializer initializer = pool.GetComponent <Function_PoolInitializer> ();
		ArrayList initializerConfig = initializer.getConfig ();
		
		if (initializer != null)
		{
			if(pool.transform.FindChild ("muret"))
			{
				GameObject lowWall = pool.transform.FindChild ("muret").gameObject;
				
				if(lowWall)
				{
					Destroy (lowWall);
				}
			}
			
			Transform sidewalk = pool.transform.FindChild ("plage");
			
			if(sidewalk)
			{
				Destroy (sidewalk.GetComponent<AABBOutlineResizer> ());
			}
			
			Destroy (initializer);
		}
		
	//	_polygon = polygon.GetMirrorX();
		_polygon = polygon;
	
		List<Point2Data> tpsList = new List<Point2Data>();
		
		foreach(Point2Data p2d in _polygon._pointsData)
		{
			tpsList.Add(p2d);
		}
		
		pointsData = new Point2Data[_polygon.GetPoints ().Count];
		
		_polygon.CopyPoint2DataTo (pointsData);	
		
		_poolGen.Generate (_polygon);
		_rimCount = _poolGen.GetRimCount ();
		
		for(int i = 0; i < tpsList.Count; i++)
		{
			if(tpsList[i].bstairway)
			{
				pointsData[i].bstairway = true;
			}
		}
		
		// on attend la frame suivante que les Function soit prete pour les configurer
		// avec les valeurs sauvegardées
		// et que les script soit supprimés
		yield return new WaitForEndOfFrame();
		
		Function_hideObject addedHider = pool.AddComponent<Function_hideObject> ();
		addedHider._nameObjectToHide = "plage";
		addedHider._strObjectToHide = "plage";
		addedHider.setConfig (hiderConfig);
		
		Function_hideObject addedRimHider = gameObject.AddComponent<Function_hideObject> ();
		addedRimHider.id = 1;
		addedRimHider._nameObjectToHide = "margelle";
		addedRimHider._strObjectToHide = "margelle";
		addedRimHider.setConfig (rimHiderConfig);
		
		addedRimHider.SetObjectToHide (_poolGen.GetRimObjects ());
		
		Function_PoolInitializer addedInitializer = pool.AddComponent<Function_PoolInitializer> ();
		addedInitializer.setConfig (initializerConfig);
		
		// mise a jour des Functions
		pool.GetComponent ("ObjData").SendMessage ("updateSpecFcns");
		
		gameObject.GetComponent<DesignerDesjoyaux>().blockStairway = false;
	}
	
	public Polygon GetPolygon ()
	{
		return _polygon;
	}
	
	public Point2Data [] GetPointsDataBackup ()
	{
		return _pointsDataBackup;	
	}
	
	public int GetRimCout ()
	{
		return _rimCount;	
	}
	
	public Texture2D GetBackgroundImage ()
	{
		return _backgroundImage;
	}
	
	public bool HasBackgroundImage()
	{
		return 	(_backgroundImage != null);
	}
	
	public void SetBackgroundImage (Texture2D image)
	{
		_backgroundImage = image;
	}
	
	public Rect GetBackgroundRect ()
	{
		return _backgroundRect;	
	}
	
	public void SetBackgroundRect (Rect rect)
	{
		_backgroundRect = rect;
	}
	
	public bool IsBackgroundImageVisible ()
	{
		return _backgroundImageVisible;
	}
	
	public void SetBackgroundImageVisible (bool visible)
	{
		_backgroundImageVisible = visible;	
	}
}