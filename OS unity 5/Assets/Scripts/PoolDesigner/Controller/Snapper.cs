using UnityEngine;
using System.Collections.Generic;

// class gerant le magnetisme d'alignement, de point et angulaire
public class Snapper : MonoBehaviour 
{
	public float SNAP_OFFSET = 100;
	public float THICKNESS = 30;
	public int ANGLE_STEP = 10;
	
	protected Point2 _exclusionPoint; // point du polygone a exclure du magnetisme, typiquement le point selectionne
	
	protected Rect _hRect; // rect pour le magnetisme d'aligment horizontal
	protected Rect _vRect; // rect pour le magnetisme d'aligment vertical
	
	protected Vector2 _hSnapPoint; // position du point dans l'alignement horizontal
	protected Vector2 _vSnapPoint; // position du point dans l'alignement vertical
	protected Vector2 _intersectedSnapPoint; // position du point correspondant aux deux alignements
	protected Vector2 _angleSnapPoint; // position du point calculé a partir du magnetisme angulaire
	
	protected Vector2 _cursor;
	
	protected bool _isHSnap; // a t'on trouvé un point pour l'alignement horizontal
	protected bool _isVSnap; // a t'on trouvé un point pour l'alignement vertical
	protected bool _isIntersectedSnap; // a t'on trouvé un point pour l'alignement horizontal et vertical
	protected bool _isAngleSnap; // a t'on trouvé un point pour le magnetisme angulaire
	
	// activation des differents magnetismes
	protected bool _angleSnapActivate = false;
	protected bool _pointSnapActivate = true;
	protected bool _alignedPointSnapActivate = true;
	
	protected Polygon _polygon;
	
	protected PolygonDrawer _polygonDrawer;
	
	protected PlanTransformation _planTransformation;
	
	protected Touch _firstTouch;
	
	protected float _offsetWidth	=	0.0f;
	protected float _offsetHeight	=	0.0f;
	
	// Use this for initialization
	protected void Start () 
	{
		_hRect = new Rect (0, 0, Screen.width, THICKNESS);
		_vRect = new Rect (0, 0, THICKNESS, Screen.height);
		
		_planTransformation = GetComponent<PlanTransformation> ();
		_polygonDrawer = GetComponent<PolygonDrawer> ();
		_polygon = _polygonDrawer.GetPolygon ();
	}
	
	// Update is called once per frame
	protected void Update () 
	{
		_cursor = _planTransformation.GetTransformedMousePosition ();

#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR		
		if (Input.touchCount != 1)
		{
			return;	
		}
#endif		
		// aligned and point snapping
		float scaleFactor = _planTransformation.GetScale ().x;
		float transformedThickness = THICKNESS * 1;
		
		Bounds polyBound = _polygon.bounds;
		float minX = polyBound.center.x - polyBound.extents.x - SNAP_OFFSET;
		float maxX = polyBound.center.x + polyBound.extents.x;
		float minY = polyBound.center.z - polyBound.extents.z - SNAP_OFFSET;
		float maxY = polyBound.center.z + polyBound.extents.z;
		
		_hRect.x = minX;
		_hRect.y = _cursor.y - transformedThickness / 2;
		_hRect.width = (maxX - minX) + SNAP_OFFSET;
		_hRect.height = transformedThickness;
		
		_vRect.x = _cursor.x - transformedThickness / 2;
		_vRect.y = minY;
		_vRect.height = (maxY - minY) + SNAP_OFFSET;
		_vRect.width = transformedThickness;
		
		_hSnapPoint = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);
		_vSnapPoint = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);
		 _intersectedSnapPoint = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);
		
		_isHSnap = false;
		_isVSnap = false;
		_isIntersectedSnap = false;
		
		// pour le magnetisme d'alignement on teste l'intersection entre les points du polygone et
		// _hRect et _vRect
		foreach (Point2 pt2 in _polygon.GetPoints ())
		{
			if (pt2 != _exclusionPoint)
			{
				Vector2 pt = pt2;
				Vector2 ptTransform = new Vector2(pt.x + _offsetWidth, pt.y + _offsetHeight);
				
				bool internHSnap = false;
				bool internVSnap = false;
				
				if (_hRect.Contains (ptTransform))
				{
					_isHSnap = true;
					internHSnap = true;
					
					float hDistanceToPt = Mathf.Abs ((_hRect.y + THICKNESS / 2) - (pt.y /*+ _offsetHeight*/));
					float hCandidateDistance = Mathf.Abs ((_hRect.y + THICKNESS / 2) - _hSnapPoint.y);
					
					if (hDistanceToPt < hCandidateDistance)
					{
						_hSnapPoint = pt;
					}
				}
				
				if (_vRect.Contains (ptTransform))
				{
					_isVSnap = true;
					internVSnap = true;
					
					float vDistanceToPt = Mathf.Abs ((_vRect.x + transformedThickness / 2) - (pt.x /*+_offsetWidth*/));
					float vCandidateDistance = Mathf.Abs ((_vRect.x + transformedThickness / 2) - _vSnapPoint.x);
					
					if (vDistanceToPt < vCandidateDistance)
					{
						_vSnapPoint = pt;
					}
				}
				
				if (internHSnap && internVSnap)
				{
					float ptDistance = (_cursor - ptTransform).sqrMagnitude;
					float candidateDistance = (_cursor - _intersectedSnapPoint).sqrMagnitude;
					
					if (ptDistance < candidateDistance)
					{
						_isIntersectedSnap = true;
						_intersectedSnapPoint = pt;
					}
				}
			}
		}
		
		//Debug.Log (_hSnapPoint + " " + _vSnapPoint);
		
		// angle snapping
		_isAngleSnap = false;
		Point2 selectedPoint = _polygonDrawer.GetSelectedPoint ();
		
		// Pour le magnetisme angulaire, l'angle formé entre le point selectionné et ses voisin
		// doit etre multiple de ANGLE_STEP
		if (selectedPoint != null)
		{
			Edge2 prevEdge = selectedPoint.GetPrevEdge ();
			
			if (prevEdge != null)
			{
				Point2 prevPoint = prevEdge.GetPrevPoint2 ();
				
				prevPoint.Set(prevPoint.GetX() /*+ _offsetWidth*/, prevPoint.GetY() /*+ _offsetHeight*/);
				
				Edge2 prePrevEdge = prevPoint.GetPrevEdge ();
				Vector2 prePrevPoint = prevPoint + new Vector2 (-1, 0);
				
				if (prePrevEdge != null)
				{
					prePrevPoint = prePrevEdge.GetPrevPoint2 ();
					prevPoint.Set(prevPoint.GetX() /*+ _offsetWidth*/, prevPoint.GetY() /*+ _offsetHeight*/);
				}
				
				// find angle in relation to previous snap (HSnap and VSnap) if active
				
				Vector2 prePrevVector = prePrevPoint - prevPoint;
				Vector2 prevVector = _cursor - prevPoint;
				
				float prevAngle = Vector2.Angle (prevVector, prePrevVector);
				_isAngleSnap = true;
				
				// find closest angle multiple of angle_step
				int roundedAngle = Mathf.RoundToInt (prevAngle);
				int minusModulo = roundedAngle % ANGLE_STEP;
				int plusModulo = ANGLE_STEP - minusModulo;
				
				int substractAngle = minusModulo;
				if (Mathf.Abs (minusModulo) > plusModulo)
				{
					substractAngle = -plusModulo;	
				}
				
				int snapAngle = roundedAngle - substractAngle;
//				Debug.Log (prevAngle + " " + snapAngle);
				
				Vector2 prePrevNormal = new Vector2 (-prePrevVector.y, prePrevVector.x).normalized;
				float normalAndPrevDot = Vector2.Dot (prevVector, prePrevNormal);
				
				if (Utils.Approximately (normalAndPrevDot, 0))
				{
					normalAndPrevDot = 0;	
				}
				
				float angleSign = Mathf.Sign (normalAndPrevDot);
				float angleDifference = Mathf.DeltaAngle (prevAngle, snapAngle) * angleSign;
				
				Quaternion rotation = Quaternion.AngleAxis (angleDifference, Vector3.forward);
				Matrix4x4 rotationMatrix = Matrix4x4.TRS (Vector3.zero, rotation, Vector3.one);
				
				Vector2 realAngleVector = _cursor - prevPoint;
				Vector2 snappedAngleVector = rotationMatrix.MultiplyVector (realAngleVector);
				snappedAngleVector.Normalize ();
				
				Vector2 projectedVector = Vector3.Project (realAngleVector, snappedAngleVector);
				_angleSnapPoint = prevPoint + projectedVector;
			}
		}
	}
	
	protected void OnGUI ()
	{
		GUI.depth = 0;
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//		if (Input.touchCount == 1)
//		{
#endif
//			GUI.Button (_hRect, "");
//			GUI.Button (_vRect, "");
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
//		}
#endif
	}
	
	public Polygon GetPolygon ()
	{
		return _polygon;	
	}
	
	public void SetPolygon (Polygon polygon)
	{
		_polygon = polygon;
	}
	
	public bool IsHSnapped ()
	{
		return _isHSnap;
	}
	
	public bool IsVSnapped ()
	{
		return _isVSnap;
	}
	
	public bool IsIntersectedSnapped ()
	{
		return _isIntersectedSnap;
	}
	
	public bool IsAngleSnapped ()
	{
		return _isAngleSnap;	
	}
	
	public float GetHSnapY ()
	{
		return _hSnapPoint.y;
	}
	
	public float GetVSnapX ()
	{
		return _vSnapPoint.x;
	}
	
	public Vector2 GetIntersectedSnapPosition ()
	{
		return _intersectedSnapPoint;
	}
	
	public Vector2 GetAngleSnapPosition ()
	{
		return _angleSnapPoint;	
	}
	
	public void SetExclusionPoint (Point2 pt)
	{
		_exclusionPoint = pt;
	}
	
	public Point2 GetExclusionPoint ()
	{
		return _exclusionPoint;
	}
	
	public bool IsAngleSnapActivate ()
	{
		return _angleSnapActivate;	
	}
	
	public void SetAngleSnapActive (bool active)
	{
		_angleSnapActivate = active;	
	}
	
	public bool IsPointSnapActivate ()
	{
		return _pointSnapActivate;	
	}
	
	public void SetPointSnapActive (bool active)
	{
		_pointSnapActivate = active;	
	}
	
	public bool IsAlignedPointSnapActivate ()
	{
		return _alignedPointSnapActivate;	
	}
	
	public void SetAlignedPointSnapActive (bool active)
	{
		_alignedPointSnapActivate = active;	
	}
	
	public void SetOffset(float offsetWidth, float offsetHeight)
	{
		_offsetWidth	=	offsetWidth;
		_offsetHeight	=	offsetHeight;
	}
}