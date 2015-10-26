using UnityEngine;
using System.Collections.Generic;

// genere une margelle a partir d'une forme de base representé par un polygone
// subdivisé par le RimSubdivider afin d'obtenir un mapping correcte et d'accueillir
// des pieces correspondant a un angle specifique (ex : margelle pour les angles droits interieurs)
public class RimGenerator
{
	protected GameObject _rim; //gameobject parent
	
	protected List<Polygon> _rimParts = new List<Polygon> (); // liste de polygone representant une portion
	
	protected List<PolygonLofter> _rimLofters = new List<PolygonLofter> (); // liste de lofter generant chaque portion
	
	protected List<Point2> _corners = new List<Point2> (); // liste des points correspondant a un coin
	
	protected RimSubdivider _rars;
	
	protected int _rimCount;
	
	protected Material _portionMaterial;
	
	public RimGenerator (Transform parent)
	{
		// on verifie que le composant parent de la margelle existe (cas d'une copie)
		// sinon on le cree et configure
		// tout les elements de la margelle (parent et enfant) partage le meme materiaux
		// indispensable pour la configuration des matieres.
		Transform rimTransform = parent.FindChild ("margelle");
		if (rimTransform == null)
		{
			_rim = new GameObject ("margelle");
			
			_rim.transform.parent = parent;
			_rim.transform.localPosition = Vector3.zero;
			_rim.transform.localRotation = Quaternion.identity;
			_rim.transform.localScale = Vector3.one;
			
			MeshRenderer renderer = _rim.AddComponent<MeshRenderer> ();
			renderer.material = new Material (Shader.Find ("Diffuse"));
			_portionMaterial = renderer.material;
		}
		else
		{
			_rim = rimTransform.gameObject;
			_portionMaterial = _rim.GetComponent<Renderer>().material;
			
			foreach (Transform t in _rim.transform)
			{
				if (t.name == "portion")
				{
					t.GetComponent<Renderer>().material = _portionMaterial;
				}
				else
				{
				
					/*if(t.GetChild(0) != null && t.GetChild(0).GetChild(1) != null && 
					t.GetChild(0).GetChild(1).renderer && t.GetChild (0).GetChild (1).renderer.material)
					{
						t.GetChild (0).GetChild (1).renderer.material = _portionMaterial;
					}*/
				}
			}
		}		
		_rars = new RimSubdivider ();
	}
	
	public void Generate (Polygon _polygon)
	{
		_rimParts.Clear ();
		_rimLofters.Clear ();
		_corners.Clear ();
		
		foreach (Transform child in _rim.transform)
		{
			GameObject.Destroy (child.gameObject);	
		}
		
		_rars.Subdivide (_polygon);
		Polygon subdividedPolygon = new Polygon (_rars.GetSubdividedPolygon ());
		subdividedPolygon.SetClosed (_polygon.IsClosed ());
		
		//******************* OBTENTION DU PROFILE A EXTERNALISER ********************//
		PolygonRawData shapeRaw = new PolygonRawData ();
		
		/*shapeRaw.Add (new Vector2 (0, 0));
		shapeRaw.Add (new Vector2 (0, 0.01f));
		shapeRaw.Add (new Vector2 (-0.015f, 0.026f));
		shapeRaw.Add (new Vector2 (-0.015f, 0.042f));
		shapeRaw.Add (new Vector2 (-0.006f, 0.06f));
		shapeRaw.Add (new Vector2 (0.01f, 0.069f));
		shapeRaw.Add (new Vector2 (0.03f, 0.071f));
		shapeRaw.Add (new Vector2 (0.049f, 0.066f));
		shapeRaw.Add (new Vector2 (0.071f, 0.06f));
		
		shapeRaw.Add (new Vector2 (0.1f, 0.052f));
		shapeRaw.Add (new Vector2 (0.169f, 0.044f));
		shapeRaw.Add (new Vector2 (0.249f, 0.041f));
		shapeRaw.Add (new Vector2 (0.29f, 0.04f));
		shapeRaw.Add (new Vector2 (0.298f, 0.038f));
		shapeRaw.Add (new Vector2 (0.305f, 0.033f));
		shapeRaw.Add (new Vector2 (0.309f, 0.024f));
		shapeRaw.Add (new Vector2 (0.31f, 0.014f));
		shapeRaw.Add (new Vector2 (0.31f, 0));*/
				
		shapeRaw.Add (new Vector2 (-0.02f, 0.0f));
		shapeRaw.Add (new Vector2 (0.28f, 0.0f));
		shapeRaw.Add (new Vector2 (0.28f, 0.001f));
		shapeRaw.Add (new Vector2 (-0.02f, 0.001f));
		
		Polygon profile = new Polygon (shapeRaw);
		profile.SetClosed (false);
		//************************************************************************//
		
		Polygon currentPortion = null;
		bool inPortion = false;
		bool inCurve = false;
		
		int pointCounter = 0;
		int indexOffset = 0;
		
		// pour chaque point du polygon subdivisé
		// si c'est un angle prédéfini on charge le mesh margelle associé
		// sinon on crée un section avec l'outil loft jusqu'au prochain angle prédéfini
		foreach (Point2 currentPoint in subdividedPolygon.GetPoints ())
		{
			float angle = currentPoint.GetAngle ();
			AngleType angleType = currentPoint.GetAngleType ();
			
			if (inPortion || inCurve)
			{
				Point2 pointToAdd = new Point2 (currentPoint);
				currentPortion.AddPoint (pointToAdd);	
			}
			
			Edge2 nextEdge = currentPoint.GetNextEdge ();
			Edge2 prevEdge = currentPoint.GetPrevEdge ();
			Point2 nextPoint = null;			
			Point2 prevPoint = null;			
			
			if (nextEdge != null) 
			{
				nextPoint = nextEdge.GetNextPoint2 ();
				prevPoint = prevEdge.GetPrevPoint2 ();
				
				float nextAngle = nextPoint.GetAngle ();
				AngleType nextAngleType = nextPoint.GetAngleType ();
				// stop case for onr portion
				if (Utils.Approximately (nextAngle, 90) && inPortion && nextAngleType == AngleType.Outside)
				{
					inPortion = false;
					currentPortion.SetClosed (false);
					
					GameObject portionGO = new GameObject ("portion");
					portionGO.transform.parent = _rim.transform;
					portionGO.transform.localPosition = Vector3.zero;
					portionGO.transform.localRotation = Quaternion.identity;
					portionGO.transform.localScale = Vector3.one;
					
					portionGO.AddComponent<MeshRenderer> ();
					MeshFilter meshFilter = portionGO.AddComponent<MeshFilter> ();
					
					PolygonLofter polyLofter = new PolygonLofter (currentPortion, profile, meshFilter.mesh);
					polyLofter.SetCurveIndices (_rars.GetCurveIndices ());
					polyLofter.SetIndexedAngleType (_rars.GetIndexedAngleType ());

					polyLofter.SetIndexOffset (indexOffset + 1, subdividedPolygon.GetPoints ().Count);
					polyLofter.Generate ();
					
					portionGO.GetComponent<Renderer>().material = _portionMaterial;
					
					_rimParts.Add (currentPortion);
					_rimLofters.Add (polyLofter);
				}
				
				// angle prédéfini (90°) --> piece a charger
				if (Utils.Approximately (angle, 90) && angleType == AngleType.Outside)
				{
					_corners.Add (currentPoint);
					
					Vector3 angleRimPosition = new Vector3 (currentPoint.GetX (), 
						                                    0, 
						                                    currentPoint.GetY ());
					
					Vector2 nextEdgeDirection = currentPoint.GetNextEdge ().ToVector2 ();
					Vector3 toDirection = new Vector3 (nextEdgeDirection.x, 0, nextEdgeDirection.y);
					Quaternion angleRimOrientation = Quaternion.LookRotation (toDirection);
					
					GameObject rimCorner = Object.Instantiate (Resources.Load ("PoolDesigner/rimCorner")) as GameObject;
					
					rimCorner.transform.parent = _rim.transform;
					rimCorner.transform.localPosition = angleRimPosition;
					rimCorner.transform.localRotation = angleRimOrientation;
					rimCorner.transform.localScale = Vector3.one;

					//rimCorner.transform.GetChild (0).GetChild (1).renderer.material = _portionMaterial;
					rimCorner.transform.GetChild (0).GetChild (0).GetComponent<Renderer>().material = _portionMaterial;
					
					if (!inPortion)
					{
						inPortion = true;
						indexOffset = pointCounter;
						currentPortion = new Polygon ();
					}
				}
				
			/*	if (Utils.Approximately (nextAngle, 90) && (currentPoint.GetX()==nextPoint.GetX()) && (currentPoint.GetY()==nextPoint.GetY())
					&& (currentPoint.GetX()==prevPoint.GetX()) && (currentPoint.GetY()==prevPoint.GetY()))
				{
					_corners.Add (currentPoint);
					
					Vector3 angleRimPosition = new Vector3 (currentPoint.GetX (), 
						                                    0, 
						                                    currentPoint.GetY ());
					
					Vector2 nextEdgeDirection = nextPoint.GetNextEdge ().ToVector2();
					Vector3 toDirection = new Vector3 (nextEdgeDirection.x, 0, nextEdgeDirection.y);
					Quaternion angleRimOrientation = Quaternion.LookRotation (toDirection);
					
					GameObject rimCorner = Object.Instantiate (Resources.Load ("PoolDesigner/rimCorner")) as GameObject;
					
					rimCorner.transform.parent = _rim.transform;
					rimCorner.transform.localPosition = angleRimPosition;
					rimCorner.transform.localRotation = angleRimOrientation;
					rimCorner.transform.localScale = Vector3.one;

					//rimCorner.transform.GetChild (0).GetChild (1).renderer.material = _portionMaterial;
					rimCorner.transform.GetChild (0).GetChild (0).renderer.material = _portionMaterial;
					
					if (!inPortion)
					{
						inPortion = true;
						indexOffset = pointCounter;
						currentPortion = new Polygon ();
					}
					
				}*/
				
			}
			
			++pointCounter;
		}
		
		if (inPortion) // si on etait entrain d'ajouter des points a une portion
		{
			if (_polygon.IsClosed ()) // on ajoute les points restants
			{
				inPortion = false;
				
				Point2 currentPoint = subdividedPolygon.GetPoints ()[0];
				float angle = currentPoint.GetAngle ();
				AngleType angleType = currentPoint.GetAngleType ();
				
				while (!Utils.Approximately (angle, 90) || angleType == AngleType.Inside)
				{
					Point2 pointToAdd = new Point2 (currentPoint);
					currentPortion.AddPoint (pointToAdd);
					
					currentPoint = currentPoint.GetNextEdge ().GetNextPoint2 ();
					angle = currentPoint.GetAngle ();
					angleType = currentPoint.GetAngleType ();
				}
			}
			
			GameObject portionGO = new GameObject ("portion");
			portionGO.transform.parent = _rim.transform;
			portionGO.transform.localPosition = Vector3.zero;
			portionGO.transform.localRotation = Quaternion.identity;
			portionGO.transform.localScale = Vector3.one;
			
			portionGO.AddComponent<MeshRenderer> ();
			MeshFilter meshFilter = portionGO.AddComponent<MeshFilter> ();
			
			PolygonLofter polyLofter = new PolygonLofter (currentPortion, profile, meshFilter.mesh);
			
			polyLofter.SetIndexOffset (indexOffset + 1, subdividedPolygon.GetPoints ().Count);
			polyLofter.SetCurveIndices (_rars.GetCurveIndices ());
			polyLofter.SetIndexedAngleType (_rars.GetIndexedAngleType ());
			polyLofter.Generate ();
			
			portionGO.GetComponent<Renderer>().material = _portionMaterial;
			
			_rimParts.Add (currentPortion);
			_rimLofters.Add (polyLofter);
		}
		
		// Cas ou il n'y a pas de d'angle predefini
		// une seule portion qui correspond au polygone subdivise
		if (_corners.Count == 0)
		{
			GameObject portionGO = new GameObject ("portion");
			portionGO.transform.parent = _rim.transform;
			portionGO.transform.localPosition = Vector3.zero;
			portionGO.transform.localRotation = Quaternion.identity;
			portionGO.transform.localScale = Vector3.one;
			
			portionGO.AddComponent<MeshRenderer> ();
			MeshFilter meshFilter = portionGO.AddComponent<MeshFilter> ();
			
			PolygonLofter polyLofter = new PolygonLofter (subdividedPolygon, profile, meshFilter.mesh);
			polyLofter.SetIndexOffset (0, subdividedPolygon.GetPoints ().Count);
			polyLofter.SetCurveIndices (_rars.GetCurveIndices ());
			polyLofter.SetIndexedAngleType (_rars.GetIndexedAngleType ());
			polyLofter.Generate ();		
			
			portionGO.GetComponent<Renderer>().material = _portionMaterial;
			
			_rimParts.Add (_polygon);
			_rimLofters.Add (polyLofter);
		}
		
		_rimCount = 0;
		foreach (PolygonLofter lofter in _rimLofters)
		{
			_rimCount += lofter.GetRimCount ();	
		}
		
		_rimCount += _corners.Count;
	}
	
	public GameObject GetParentGameObject ()
	{
		return _rim;	
	}
	
	public int GetRimCount ()
	{
		return _rimCount;	
	}
}