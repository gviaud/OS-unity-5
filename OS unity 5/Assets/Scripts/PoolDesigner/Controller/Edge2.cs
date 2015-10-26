using UnityEngine;
using System.Collections;

// class representant un segment de polygon
// chaque segment reference deux points correspondant aux extremité du segment
public class Edge2
{
	protected Point2 _prevPt;
	protected Point2 _nextPt;
	
	protected Vector2 _normal;
	protected Vector2 _direction;
	
	protected float _length;
	
	public void Update ()
	{
		Vector2 prev = _prevPt;
		Vector2 next = _nextPt;
		
		_direction = next - prev;
		_length = _direction.magnitude;
		
		_direction.Normalize ();
		_normal = new Vector2 (-_direction.y, _direction.x);
	}
	
	public Edge2 (Point2 prevPt, Point2 nextPt)
	{
		_prevPt = prevPt;
		_nextPt = nextPt;
		Update ();
	}
	
	public Point2 GetPrevPoint2 ()
	{
		return _prevPt;
	}
	
	public void SetPrevPoint2 (Point2 prevPoint)
	{
		_prevPt = prevPoint;
	}
	
	public Point2 GetNextPoint2 ()
	{
		return _nextPt;
	}
	
	public void SetNextPoint2 (Point2 nextPoint)
	{
		_nextPt = nextPoint;
	}
	
	public Vector2 GetNormal ()
	{
		return _normal;	
	}
	
	public float GetLength ()
	{
		return _length;	
	}
	
	public Vector2 ToVector2 ()
	{
		return (Vector2)_nextPt	- (Vector2)_prevPt;
	}
	
	public Vector2 ToUnitVector2 ()
	{
		return _direction;	
	}
}
