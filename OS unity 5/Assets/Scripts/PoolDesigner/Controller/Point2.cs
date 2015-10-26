using UnityEngine;
using System;
using System.Collections;

// permet de definir si l'angle forme par deux segement est "convexe" --> Outside
// ou "concave" --> Inside au sein du polygone
public enum AngleType
{
	None,
	Inside,
	Outside
}

// enum representant le type de jonction : arc de cercle ou angle
public enum JunctionType
{
	Broken,
	Curved
}

// class representant un point du plan dans un polygone
// chaque point reference deux segments
public class Point2 : ICloneable
{	
	protected Edge2 _nextEdge;
	protected Edge2 _prevEdge;
	
	protected StraightLine _angularBisector; // bissectrice de l'angle formé par les deux segments
	
	protected Vector2 _normal; // normal unitaire au point
	protected Vector2 _calculatedNormal; // normal au point calculé afin de garder un epaisseur constante lors d'un loft
	
	protected float _normalScaleFactor; // scale entre _normal et _calculatedNormal
	
	protected float _angle; // angle formé par les deux segments
	
	protected float _x;
	protected float _y;
	
	protected AngleType _angleType = AngleType.None;
	
	protected JunctionType _junction = JunctionType.Broken;
	
	public Point2 () 
	{
		_x = 0;
		_y = 0;
	}
	
	public Point2 (float xCoord, float yCoord)
	{
		_x = xCoord;
		_y = yCoord;
	}
	
	public Point2 (Vector2 pt2)
	{
		_x = pt2.x;
		_y = pt2.y;
	}
	
//	public static implicit operator Point2 (Vector2 vector)
//    {
//        return new Point2 (vector.x, vector.y);
//    }
	
	public static implicit operator Vector2 (Point2 pt2)
    {
        return new Vector2 (pt2._x, pt2._y);
    }
	
	public object Clone ()
	{
		Point2 clone = (Point2)this.MemberwiseClone ();
		clone.SetEdges (null, null);
		return clone;	
	}
	
	// calcul des donnees : angle, bissectrice, normales en fonction de sa position et de la position des points voisins
	public virtual void Update (bool recursive=true)
	{
		if (_prevEdge != null && _nextEdge != null)
		{
			_prevEdge.Update ();
			_nextEdge.Update ();
			
			Vector2 prevNormal = _prevEdge.GetNormal ();
			Vector2 nextNormal = _nextEdge.GetNormal ();
			Vector2 nextEdgeDirection = _nextEdge.ToVector2 ();
			
			_normal = (prevNormal + nextNormal).normalized;
			_angle = 180 - Vector2.Angle (prevNormal, nextNormal);
			
			Vector2 prevDirection = _prevEdge.ToVector2 ().normalized * -1;
			Vector2 nextDirection = _nextEdge.ToVector2 ().normalized;
			Vector2 abp = (Vector2)this + (prevDirection + nextDirection);
			_angularBisector = StraightLine.From2Point (this, abp);
			
			float prevNormalNextEdgeDot = Vector2.Dot (prevNormal, nextEdgeDirection);
			
			if (Utils.Approximately (prevNormalNextEdgeDot, 0))
			{
				_angleType = AngleType.None;
			}
			else if (prevNormalNextEdgeDot > 0)
			{
				_angleType = AngleType.Inside;
			}
			else if (prevNormalNextEdgeDot < 0)
			{
				_angleType = AngleType.Outside;
			}
			else
			{
				_angleType = AngleType.None;
			}
			
			if (recursive)
			{
				_prevEdge.GetPrevPoint2 ().Update (false);
				_nextEdge.GetNextPoint2 ().Update (false);
			}
		}
		else if (_prevEdge != null && _nextEdge == null)
		{
			_prevEdge.Update ();
			
			_angularBisector = StraightLine.FromPointAndDirection (this, Vector2.zero);
			
			_normal = _prevEdge.GetNormal ();
			_angle = float.NaN;
			
			_angleType = AngleType.None;
			
			if (recursive)
			{
				_prevEdge.GetPrevPoint2 ().Update (false);	
			}
		}
		else if (_prevEdge == null && _nextEdge != null)
		{
			_nextEdge.Update ();
			
			_angularBisector = StraightLine.FromPointAndDirection (this, Vector2.zero);
			
			_normal = _nextEdge.GetNormal ();
			_angle = float.NaN;
			
			_angleType = AngleType.None;
			
			if (recursive)
			{
				_nextEdge.GetNextPoint2 ().Update (false);	
			}
		}
		else
		{
			_angularBisector = StraightLine.FromPointAndDirection (this, Vector2.zero);
			_normal = Vector2.zero;
			_angle = float.NaN;
			_angleType = AngleType.None;
		}
		
		_calculatedNormal = _normal;
		_normalScaleFactor = 1;
		
		if (!float.IsNaN (_angle))
		{
			float radAngle = _angle * Mathf.Deg2Rad;
			float sinus = Mathf.Sin (radAngle / 2);
			
			if (sinus != 0)
			{
				_calculatedNormal = (1 / sinus) * _normal;
				_normalScaleFactor = 1 + (_calculatedNormal.magnitude - 1);
				
				//Debug.Log (_angle + " " + sinus + " " + _normalScaleFactor);
			}
			else
			{
				_calculatedNormal = _normal;
				_normalScaleFactor = 1;
			}
		}
	}
	
	public float GetX ()
	{
		return _x;	
	}
	
	public float GetY ()
	{
		return _y;
	}
	
	public StraightLine GetAngularBisector ()
	{
		return _angularBisector;	
	}
	
	public Vector2 GetNormal ()
	{
		return _normal;	
	}
	
	public Vector2 GetCalculatedNormal ()
	{
		return _calculatedNormal;	
	}
	
	public float GetNormalScaleFactor ()
	{
		return _normalScaleFactor;	
	}
	
	public float GetAngle ()
	{
		return _angle;	
	}
	
	public AngleType GetAngleType ()
	{
		return _angleType;	
	}
	
	public JunctionType GetJunction ()
	{
		return _junction;	
	}
	
	public void SetX (float x)
	{
		_x = x;
		Update ();
	}
	
	public void SetY (float y)
	{
		_y = y;
		Update ();		
	}
	
	public void Set (float x, float y)
	{
		_x = x;
		_y = y;
		Update ();
	}
	
	public void Set (Vector2 pt2)
	{
		_x = pt2.x;
		_y = pt2.y;
		Update ();
	}
	
	public Edge2 GetPrevEdge ()
	{
		return _prevEdge;
	}
	
	public Edge2 GetNextEdge ()
	{
		return _nextEdge;
	}
	
	public void SetPrevEdge (Edge2 prevEdge)
	{
		_prevEdge = prevEdge;
		Update ();
	}
	
	public void SetNextEdge (Edge2 nextEdge)
	{
		_nextEdge = nextEdge;
		Update ();
	}
	
	public void SetEdges (Edge2 prevEdge, Edge2 nextEdge)
	{
		_prevEdge = prevEdge;
		_nextEdge = nextEdge;
		Update ();
	}
}