using UnityEngine;
using System.Collections;

// struct representant une droite du plan sous forme de point et vecteur direction
public struct StraightLine 
{
	public Vector2 point;
	public Vector2 tangent;
	
	static public StraightLine FromPointAndDirection (Vector2 pt, Vector2 direction)
	{
		StraightLine l = new StraightLine ();
		l.point = pt;
		l.tangent = direction.normalized;
		
		return l;
	}
	
	static public StraightLine From2Point (Vector2 a, Vector2 b)
	{
		StraightLine l = new StraightLine ();
		l.point = a;
		l.tangent = (b - a).normalized;
		
		return l;
	}
}

public enum IntersectionType
{
	NoIntersection,
	UniqueIntersection,
	MultipleIntersections,
	InfinityIntersections	
}

public struct IntersectionResult
{
	public IntersectionType intersectionType;
	public Vector2 [] intersectedPoints;
	public int intersectionCount;
}

public class GeometryTools
{
	// intersction entre deux droite du plan
	static public IntersectionResult FindIntersection (StraightLine l0, StraightLine l1)
	{
		Vector2 delta = l1.point - l0.point;
		Vector3 crossVector = Vector3.Cross (l0.tangent, l1.tangent);
		
		IntersectionResult ir = new IntersectionResult ();
		
		if (!Utils.Approximately (crossVector.z, 0))
		{
			// unique intersection point
			ir.intersectionType = IntersectionType.UniqueIntersection;
			ir.intersectionCount = 1;
			
			Vector3 delta1CrossVector = Vector3.Cross (delta, l1.tangent);
			float l0Factor = delta1CrossVector.z / crossVector.z;
			
			Vector2 intersectedPoint = l0.point + l0Factor * l0.tangent;
			ir.intersectedPoints = new Vector2 [1];
			ir.intersectedPoints[0] = intersectedPoint;
		}
		else 
		{
			Vector3 delta0CrossVector = Vector3.Cross (l0.tangent, delta);
			
			if (Utils.Approximately (delta0CrossVector.z, 0))
			{		
			// infinitely many points
			ir.intersectionType = IntersectionType.InfinityIntersections;
			ir.intersectionCount = -1;
			}
			else
			{
				// lines are parallel, no intersection
				ir.intersectionType = IntersectionType.NoIntersection;
				ir.intersectionCount = 0;
			}
		}
			
		return ir;	
	}
	
	// obtient une droite perpendiculaire a une droite et passant par un point
	static public StraightLine GetPerpendicularLine (StraightLine l, Vector2 p)
	{
		Vector2 normal = new Vector2 (-l.tangent.y, l.tangent.x);
		normal.Normalize ();
		
		StraightLine perpendicularLine = new StraightLine ();
		perpendicularLine.tangent = normal;
		perpendicularLine.point = p;
		
		return perpendicularLine;	
	}
}
