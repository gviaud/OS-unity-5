using UnityEngine;
using System.Collections;
using System.IO;

public class Function_PoolDesigner : MonoBehaviour, Function_OS3D
{
	protected string _functionName = "PoolDesigner.FunctionName";
	
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
	
//	[SerializeField]
//	protected Transform [] _rimObjects;
	
	public string _uiName = "PoolUIv2";
//	public string _uiName = "PoolDesignerUI";
	
	public int id;
	
	public Point2Data[] pointsData;

	void Awake ()
	{
		transform.parent = GameObject.Find ("MainNode").transform;
		
		if(_uiName != null)
		{
			setUI ((FunctionUI_OS3D)GameObject.Find ("MainScene").GetComponent ("PoolUIv2"));
//			setUI ((FunctionUI_OS3D)GameObject.Find ("MainScene").GetComponent ("PoolDesignerUI"));	
		}
			
		transform.localScale = Vector3.one;
		
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
			
			StartCoroutine (WaitForGameobjectReadyToDoAction ());
		}
		
		//gameObject.GetComponent ("ObjData").SendMessage ("updateSpecFcns");
		//transform.localPosition = currentPosition;
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
			DoAction ();
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
	
	public void DoAction () 
	{
		if(_ui != null)
		{
			_ui.DoActionUI(gameObject);
		}
	}
	
	public void setUI(FunctionUI_OS3D ui)
	{
		_ui = ui;
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
		}
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
			
			pointsData[pIndex] = pData;	
			_pointsDataBackup[pIndex] = pData;
		}
		
		_polygon = new Polygon (pointsData);
		_isLoading = true;
		_IsGenerated = true;
		StartCoroutine (Generate (_polygon));
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
			GameObject lowWall = pool.transform.FindChild ("muret").gameObject;
			Destroy (lowWall);
			
			Transform sidewalk = pool.transform.FindChild ("plage");
			Destroy (sidewalk.GetComponent<AABBOutlineResizer> ());
			
			Destroy (initializer);
		}
		
	//	_polygon = polygon.GetMirrorX();
			
		_polygon = polygon;
	
		
		pointsData = new Point2Data[_polygon.GetPoints ().Count];
		
		
		_polygon.CopyPoint2DataTo (pointsData);	
		
		_poolGen.Generate (_polygon);
		_rimCount = _poolGen.GetRimCount ();
		
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
		
//		ObjData data = pool.GetComponent<ObjData> ();
//		if (data != null)
//		{
//			data.updateSpecFcns ();	
//		}
		
		// mise a jour des Functions
		pool.GetComponent ("ObjData").SendMessage ("updateSpecFcns");
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