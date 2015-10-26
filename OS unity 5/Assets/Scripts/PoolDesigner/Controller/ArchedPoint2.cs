using UnityEngine;
using System.Collections;

// class point representant un arc de cercle
public class ArchedPoint2 : Point2
{
	protected static float ANGLE_STEP = 5;
	
	// point tangent a l'arc
	protected Vector2 _prevTangentPoint;
	protected Vector2 _nextTangentPoint;
	
	protected Vector2 _anchorPoint; // intersection entre la bissectrice de l'angle et l'arc de cercle
	protected Vector2 _center;
	
	// tangentes de l'arc de cercle
	protected StraightLine _prevTangent;
	protected StraightLine _nextTangent;
	
	protected float _radius = 100; // rayon souhaité
	protected float _measuredRadius; // rayon mesuré
	protected float _maxRadius; // rayon maximum possible
	protected float _arcAngle; // angle entre les deux tangents
	protected float _arcLength; // longueur de l'arc
	
	protected bool _prevTangentPointMerged;
	protected bool _nextTangentPointMerged;
	
	public ArchedPoint2 (Point2 point) : base (point)
	{
		_prevEdge = point.GetPrevEdge ();
		_nextEdge = point.GetNextEdge ();
		_junction = JunctionType.Curved;
		
		_prevEdge.SetNextPoint2 (this);
		_nextEdge.SetPrevPoint2 (this);
		
		Update (false);
	}
	
	/*public override void Update (bool recursive=true)
	{
		base.Update (recursive);
		
		if (_prevEdge != null && _nextEdge != null)
		{
			_prevTangent = Line.FromPointAndDirection (this, _prevEdge.ToVector2 () * -1);
			_nextTangent = Line.FromPointAndDirection (this, _nextEdge.ToVector2 ());
			
			// find the farthest center position
			float prevLength = _prevEdge.GetLength ();
			Point2 prevPoint = _prevEdge.GetPrevPoint2 ();
			Vector2 prevPointNextTangentPoint = prevPoint;
			if (prevPoint.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 aPrevPoint = _prevEdge.GetPrevPoint2 () as ArchedPoint2;
				prevPointNextTangentPoint = aPrevPoint.GetNextTangentPoint ();
				prevLength = Vector2.Distance (prevPointNextTangentPoint, this);
			}
			
			float nextLength = _nextEdge.GetLength ();
			Point2 nextPoint = _nextEdge.GetNextPoint2 ();
			Vector2 nextPointPrevTangentPoint = nextPoint;
			if (nextPoint.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 aNextPoint = _nextEdge.GetPrevPoint2 () as ArchedPoint2;
				nextPointPrevTangentPoint = aNextPoint.GetPrevTangentPoint ();
				nextLength = Vector2.Distance (nextPointPrevTangentPoint, this);
			}
			
			Line tangentPerp;
			if (prevLength < nextLength)
			{
				tangentPerp = GeometryTools.GetPerpendicularLine (_prevTangent, prevPointNextTangentPoint);
			}
			else
			{
				tangentPerp = GeometryTools.GetPerpendicularLine (_nextTangent, nextPointPrevTangentPoint);
			}
			
			IntersectionResult maxCenterIntersection = GeometryTools.FindIntersection (_angularBisector, tangentPerp);
			
			if (maxCenterIntersection.intersectionType == IntersectionType.UniqueIntersection)
			{
				_junction = JunctionType.Curved;
				
				// find maxCenter, maxRadius and the anchorPoint in relation to both neighbooring point
				Vector2 thisVector2 = this;
				Vector2 maxCenter = maxCenterIntersection.intersectedPoints[0];
				float maxThis2Center = Vector2.Distance (thisVector2, maxCenter);
				float maxRadius = Vector2.Distance (maxCenter, tangentPerp.point);
				
				float lengthArcFactor = (maxThis2Center - maxRadius) * _size;
				_anchorPoint = thisVector2 + lengthArcFactor * _angularBisector.tangent.normalized;
				
				if (Utils.Approximately (_size, 1))
				{
					_center = maxCenter;
					_radius = maxRadius;
					
					if (prevLength < nextLength)
					{
						_prevTangentPoint = prevPointNextTangentPoint;
						
						Line tangentPerpLine = GeometryTools.GetPerpendicularLine (_nextTangent, _center);
						IntersectionResult tangentPointIntersection = 
							GeometryTools.FindIntersection (_nextTangent, tangentPerpLine);
						
						if (tangentPointIntersection.intersectionType == IntersectionType.UniqueIntersection)
						{
							_nextTangentPoint = tangentPointIntersection.intersectedPoints[0];
						}
						else
						{
							Debug.Log ("NO NEXT TANGENT POINT");
						}
					}
					else
					{
						_nextTangentPoint =  nextPointPrevTangentPoint;
						
						Line tangentPerpLine = GeometryTools.GetPerpendicularLine (_prevTangent, _center);
						IntersectionResult tangentPointIntersection = 
							GeometryTools.FindIntersection (_prevTangent, tangentPerpLine);
						
						if (tangentPointIntersection.intersectionType == IntersectionType.UniqueIntersection)
						{
							_prevTangentPoint = tangentPointIntersection.intersectedPoints[0];
						}
						else
						{
							Debug.Log ("NO PREV TANGENT POINT");
						}
					}

				}
				else
				{
					// find both intersection point between both tangent 
					// and the angular bisector's perpendicular line passing by the anchor point
					Line bisectorPerpLine = GeometryTools.GetPerpendicularLine (_angularBisector, _anchorPoint);
					
					IntersectionResult prevTangentIntersection = 
						GeometryTools.FindIntersection (bisectorPerpLine, _prevTangent);
					
					IntersectionResult nextTangentIntersection = 
						GeometryTools.FindIntersection (bisectorPerpLine, _nextTangent);
					
					if (prevTangentIntersection.intersectionType == IntersectionType.UniqueIntersection &&
						nextTangentIntersection.intersectionType == IntersectionType.UniqueIntersection)
					{
						Vector2 prevTangentOffsetingPoint = prevTangentIntersection.intersectedPoints[0];
						Vector2 nextTangentOffsetingPoint = nextTangentIntersection.intersectedPoints[0];
						
						// find tangent points
						float bisector2tangent = Vector2.Distance (prevTangentOffsetingPoint, _anchorPoint);
						
						_prevTangentPoint = prevTangentOffsetingPoint + _prevTangent.tangent * bisector2tangent;
						_nextTangentPoint = nextTangentOffsetingPoint + _nextTangent.tangent * bisector2tangent;
						
						// find center with tangent perpendicular and angular bisector
						Line tangentPerpLine = GeometryTools.GetPerpendicularLine (_prevTangent, _prevTangentPoint);
						
						IntersectionResult centerIntersection = 
							GeometryTools.FindIntersection (_angularBisector, tangentPerpLine);
						
						if (centerIntersection.intersectionType == IntersectionType.UniqueIntersection)
						{
							_center = centerIntersection.intersectedPoints[0];
							_radius = Vector2.Distance (_center, _anchorPoint);
							
						}
						else	
						{
							Debug.Log ("NO CENTER FOUND");
						}
					}
					else
					{
						Debug.Log ("NO TANGENT POINT FOUND");
					}
				}
			}
			else
			{
				Debug.Log ("NO MAX CENTER FOUND");
				_anchorPoint = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);
				_center = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);
				_radius = float.PositiveInfinity;
				_prevTangentPoint = this;
				_nextTangentPoint = this;
				_junction = JunctionType.Broken;
			}
		}
	}*/
	// calcule le centre de l'arc de cercle, le rayon maximum, la position des points tangents
	// la longueur de l'arc, l'angle entre les points tantgents et le centre. Tout cela en fonction
	// de la position du point et des points voisins.
	// voir http://debart.pagesperso-orange.fr/seconde/contruc_cercle.html#ch5
	public override void Update (bool recursive=true)
	{
		base.Update (recursive);

		if (_prevEdge != null && _nextEdge != null)
		{
			_prevTangent = StraightLine.FromPointAndDirection (this, _prevEdge.ToUnitVector2 () * -1);
			_nextTangent = StraightLine.FromPointAndDirection (this, _nextEdge.ToUnitVector2 ());
			
			_prevTangentPointMerged = false;
			_nextTangentPointMerged = false;
			
			// find the farthest center position
			float prevLength = _prevEdge.GetLength ();
			Point2 prevPoint = _prevEdge.GetPrevPoint2 ();
			Vector2 prevPointNextTangentPoint = prevPoint;
			if (prevPoint.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 aPrevPoint = prevPoint as ArchedPoint2;
				prevPointNextTangentPoint = aPrevPoint.GetNextTangentPoint ();
				prevLength = Vector2.Distance (prevPointNextTangentPoint, this);
			}
			
			float nextLength = _nextEdge.GetLength ();
			Point2 nextPoint = _nextEdge.GetNextPoint2 ();
			Vector2 nextPointPrevTangentPoint = nextPoint;
			if (nextPoint.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 aNextPoint = nextPoint as ArchedPoint2;
				nextPointPrevTangentPoint = aNextPoint.GetPrevTangentPoint ();
				nextLength = Vector2.Distance (nextPointPrevTangentPoint, this);
			}
			
			prevPointNextTangentPoint = prevPointNextTangentPoint + _prevTangent.tangent * -40;
			nextPointPrevTangentPoint = nextPointPrevTangentPoint + _nextTangent.tangent * -40;
			
			StraightLine tangentPerp;
			if (prevLength < nextLength)
			{
				tangentPerp = GeometryTools.GetPerpendicularLine (_prevTangent, prevPointNextTangentPoint);
			}
			else
			{
				tangentPerp = GeometryTools.GetPerpendicularLine (_nextTangent, nextPointPrevTangentPoint);
			}
			
			IntersectionResult maxCenterIntersection = GeometryTools.FindIntersection (_angularBisector, tangentPerp);
			
			if (maxCenterIntersection.intersectionType == IntersectionType.UniqueIntersection)
			{
				_junction = JunctionType.Curved;
				
				// find maxCenter, maxRadius and the anchorPoint in relation to both neighbooring point
				Vector2 thisVector2 = this;
				Vector2 maxCenter = maxCenterIntersection.intersectedPoints[0];
				
				_maxRadius = Vector2.Distance (maxCenter, tangentPerp.point);
				float radHalfAngle = Mathf.Deg2Rad * (_angle / 2);
				
				if (_radius > _maxRadius)
				{
					_measuredRadius = _maxRadius;
					_center = maxCenter;
					
					/*if (prevLength < nextLength)
					{
						if (prevPoint.GetJunction () == JunctionType.Curved)
						{
							ArchedPoint2 aPrevPoint = prevPoint as ArchedPoint2;
							
							if(!aPrevPoint._nextTangentPointMerged)
							{
								_prevTangentPointMerged = true;
							}
						}
						else
						{
							_prevTangentPointMerged = true;
						}
					}
					else
					{
						if (nextPoint.GetJunction () == JunctionType.Curved)
						{
							ArchedPoint2 aNextPoint = nextPoint as ArchedPoint2;
							
							if(!aNextPoint._prevTangentPointMerged)
							{
								_nextTangentPointMerged = true;
							}
						}
						else
						{
							_nextTangentPointMerged = true;
						}
					}*/
				}
				else
				{
					_measuredRadius = _radius;
					
					float this2CenterTranslationFactor = _measuredRadius / Mathf.Sin (radHalfAngle);
					_center = thisVector2 + _angularBisector.tangent * this2CenterTranslationFactor;
				}
				
				_anchorPoint = _center + _angularBisector.tangent * -_measuredRadius;
				
				_arcAngle = 180 - ((_angle / 2) + 90);
				_arcLength = _arcAngle * Mathf.Deg2Rad * _measuredRadius;
				
				float tangentPointTranslationFactor = _measuredRadius / Mathf.Tan (radHalfAngle);
				_prevTangentPoint = thisVector2 + _prevTangent.tangent * tangentPointTranslationFactor;
				_nextTangentPoint = thisVector2 + _nextTangent.tangent * tangentPointTranslationFactor;
			}
			else
			{
				Debug.Log ("NO MAX CENTER FOUND");
				_anchorPoint = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);
				_center = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);
				_measuredRadius = float.PositiveInfinity;
				_arcAngle = 0;
				_arcLength = 0;
				_prevTangentPoint = this;
				_nextTangentPoint = this;
				_junction = JunctionType.Broken;
			}
		}
		else
		{
			Debug.Log ("EDGE NULL");	
		}
	}
	
	public Vector2 GetPrevTangentPoint ()
	{
		return _prevTangentPoint;	
	}
	
	public Vector2 GetNextTangentPoint ()
	{
		return _nextTangentPoint;	
	}
	
	public Vector2 GetAnchorPoint ()
	{
		return _anchorPoint;	
	}
	
	public Vector2 GetCenter ()
	{
		return _center;	
	}
	
	public float GetRadius ()
	{
		return _radius;	
	}
	
	public void SetRadius (float radius)
	{
		if (radius < 0)
		{
			_radius = 0;
		}
		else
		{
			_radius = radius;
		}
		
		Update (false);
	}
	
	public void SetMeasuredRadius (float _fmeasuredRadius)
	{
		_measuredRadius = _fmeasuredRadius;	
	}
	
	public float GetMeasuredRadius ()
	{
		return _measuredRadius;	
	}
	
	public float GetArcAngle ()
	{
		return _arcAngle;	
	}
	
	public float GetArcLength ()
	{
		return _arcLength;	
	}
	
	public float GetMaxRadius ()
	{
		return _maxRadius;	
	}
	
	public bool IsPrevTangentPointMerged ()
	{
		return _prevTangentPointMerged;	
	}
	
	public bool IsNextTangentPointMerged ()
	{
		return _nextTangentPointMerged;	
	}
	
	// retourne l'arc de cercle sous forme d'une liste de point
	// voir ArcDrawer pour l'algo
	public PolygonRawData GetCurve ()
	{
		PolygonRawData rawCurve = new PolygonRawData ();
		
		Vector2 firstPoint2Add = _prevTangentPoint;
		Vector2 lastPoint2Add = _nextTangentPoint;
		
		Vector2 startVector = (_prevTangentPoint - _center).normalized;
		Vector2 endVector = (_nextTangentPoint - _center).normalized;
		
		Vector2 angleMeasureReference = Vector2.right;
		
		if (_angleType == AngleType.Outside)
		{
			startVector = (_nextTangentPoint - _center).normalized;
			endVector = (_prevTangentPoint - _center).normalized;
			
			firstPoint2Add = _nextTangentPoint;
			lastPoint2Add = _prevTangentPoint;
		}
		
		float startAngle;		
		if (Utils.Approximately (Vector3.Cross (startVector, angleMeasureReference).z, 0))
		{
			startAngle = Vector2.Dot (startVector, angleMeasureReference) < 0 ? 180 : 0;
		}
		else
		{
			Quaternion startRotation = Quaternion.FromToRotation (angleMeasureReference, startVector);
			startAngle = startRotation.eulerAngles.z;
		}
		
		float endAngle;
		if (Utils.Approximately (Vector3.Cross (endVector, angleMeasureReference).z, 0))
		{
			endAngle = Vector2.Dot (endVector, angleMeasureReference) < 0 ? 180 : 0;
		}
		else
		{
			Quaternion endRotation = Quaternion.FromToRotation (angleMeasureReference, endVector);
			endAngle = endRotation.eulerAngles.z;
		}
		
		// round angle
		startAngle = Mathf.Round(startAngle * 100) / 100;
		endAngle = Mathf.Round(endAngle * 100) / 100;
		
		//Debug.Log (startAngle + " " + endAngle);
		
		if (startAngle > endAngle)
		{
			endAngle += 360;
		}
		
		startAngle += ANGLE_STEP;
		float currentAngle = startAngle;
		
		rawCurve.Add (firstPoint2Add);
		
		while (currentAngle < endAngle)
		{
			float radAngle = currentAngle * Mathf.Deg2Rad;
			Vector2 arcPoint = new Vector2 (Mathf.Cos (radAngle), Mathf.Sin (radAngle)) * _measuredRadius + _center;
			
			if (_angleType == AngleType.Outside)
			{
				rawCurve.Insert (0, arcPoint);
			}
			else
			{
				rawCurve.Add (arcPoint);
			}
			
			currentAngle += ANGLE_STEP;
		}
		
		if (_angleType == AngleType.Outside)
		{
			rawCurve.Insert (0, lastPoint2Add);
		}
		else
		{
			rawCurve.Add (lastPoint2Add);
		}
		
//		if (_prevTangentPointMerged)
//		{
//			rawCurve.RemoveAt (0);
//		}
//		
//		if (_nextTangentPointMerged)
//		{
//			rawCurve.RemoveAt (rawCurve.Count - 1);
//		}
		
		return rawCurve;
	}
}