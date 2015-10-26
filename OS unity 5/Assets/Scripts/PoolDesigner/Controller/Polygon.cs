using UnityEngine;
using System.Collections.Generic;

// Class reprensentant un polygone ouvert ou ferme
public class Polygon
{
	protected bool _closed = false;
	
	protected Vector2 _polygonCenter;
	
	public Point2Data[] _pointsData;
	
	// ensemble de points et segments lié entre eux et ordonnés dans le sens antihoraire
	protected LoopedList <Point2> _points = new LoopedList<Point2> ();
	protected LoopedList <Edge2> _edges = new LoopedList<Edge2> ();
	
	protected Bounds _bounds = new Bounds (Vector3.zero, Vector3.zero);
	
	public void UpdateBounds ()
	{
		float minX = float.MaxValue;
		float maxX = float.MinValue;
		float minY = float.MaxValue;
		float maxY = float.MinValue;
		
		foreach (Point2 pt2 in _points)
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
		
		Vector3 size = new Vector3 (maxX - minX, 0, maxY - minY);
		Vector3 center = new Vector3 ((maxX + minX) / 2, 0, (maxY + minY) / 2);
		_bounds = new Bounds (center, size);
		
		_polygonCenter = new Vector2 (center.x, center.z);
	}
	
	public Polygon ()
	{
		
	}
	
	public Polygon (Point2Data [] pointsData)
	{
		_pointsData = new Point2Data[pointsData.Length];
		
		List<int> curvedIndices = new List<int> ();
		
		for (int pIndex = 0; pIndex < pointsData.Length; ++pIndex)
		{
			Point2Data newPointData 	= new Point2Data();
			newPointData.position.x 	= pointsData[pIndex].position.x;
			newPointData.position.y	 	= pointsData[pIndex].position.y;
			newPointData.radius 		= pointsData[pIndex].radius;
			newPointData.junctionType 	= pointsData[pIndex].junctionType;
			newPointData.bstairway  	= pointsData[pIndex].bstairway;
			_pointsData[pIndex] = newPointData;			
			
			AddPoint (new Point2 (pointsData[pIndex].position));
			
			if (pointsData[pIndex].junctionType == JunctionType.Curved)
			{
				curvedIndices.Add (pIndex);	
			}
		}
		
		if (pointsData.Length > 2)
		{
			Close ();
		}
		
		foreach (int curvedIndex in curvedIndices)
		{
			SetJunctionType (curvedIndex, JunctionType.Curved);
			
			ArchedPoint2 aPoint = _points[curvedIndex] as ArchedPoint2;
			Point2Data pData = pointsData[curvedIndex];
			
			aPoint.SetRadius (pData.radius);
		}
	}
	
	public Polygon (PolygonRawData polygonRawData)
	{
		
		_closed = true;
		
		float area = 0;
		
		for (int i = 0; i < polygonRawData.Count; ++i)
		{
			Vector2 p1 = polygonRawData[i];
			Vector2 p2 = polygonRawData[i + 1];
			
			area += p1.x * p2.y - p2.x * p1.y;
		}
		
		if (area > 0)
		{
			polygonRawData.Reverse ();	
		}
		
		//Debug.Log ("AREA " + area);
		
		Edge2 prevEdge = null;
		Point2 prevPt = null;
		Point2 currentPt = null;
		
		for (int i = 0; i < polygonRawData.Count - 1; ++i)
		{
			Vector2 rawPt = polygonRawData[i];
			
			if (currentPt == null)
			{
				//currentPt = new Point2 (rawPt);
				currentPt = new Point2 ();
				currentPt.Set(
					rawPt.x,
					rawPt.y);
			}
			
			Point2 nextPt = new Point2 (polygonRawData[i + 1]);
			
			if (i == polygonRawData.Count - 2 && polygonRawData.Count > 2)
			{
				nextPt = _points[0].GetPrevEdge ().GetPrevPoint2 ();
				//nextPt = (Point2)_points[0].GetPrevEdge ().GetPrevPoint2 ().Clone();
				//nextPt = new Point2();
				nextPt.Set(
					_points[0].GetPrevEdge ().GetPrevPoint2 ().GetX(),
					_points[0].GetPrevEdge ().GetPrevPoint2 ().GetY());
			}
			
			if (prevEdge == null)
			{
				if (prevPt == null)
				{
					//prevPt = new Point2 (polygonRawData[i - 1]);
					prevPt = new Point2 ();
					prevPt.Set(
						polygonRawData[i - 1].x,
						polygonRawData[i - 1].y);
				}
				
				prevEdge = new Edge2 (prevPt, currentPt);
				prevPt.SetNextEdge (prevEdge);
			}
			
			Edge2 nextEdge = new Edge2 (currentPt, nextPt);
//			if (i == polygonRawData.Count - 1)
//			{
//				nextEdge = _points[0].GetPrevEdge ();
//			}
			
			currentPt.SetEdges (prevEdge, nextEdge);
			
			_points.Add (currentPt);
			_edges.Add (nextEdge);
			
			if (i == polygonRawData.Count - 2 && polygonRawData.Count >= 2)
			{
				nextPt.SetPrevEdge (nextEdge);
				_edges.Add (nextPt.GetNextEdge ());
				_points.Add (nextPt);
			}
			
			prevEdge = nextEdge;
			prevPt = currentPt;
			currentPt = nextPt;
		}
		
		if (_edges.Count == 2 && _points.Count == 2)
		{
			_points[0].SetPrevEdge (null);	
			_points[1].SetNextEdge (null);
			_edges.RemoveAt (1);
		}
		
		UpdateBounds ();
	}
	
	// calcule le perimetre si fermé
	public float GetPerimeter ()
	{
		if (!_closed)
			return float.NaN;
		
		float perimeter = 0.0f;
		
		foreach (Edge2 edge in _edges)
		{
			perimeter += edge.GetLength ();	
		}
			
		return perimeter;
	}
	
	// calcule la longueur du polygone meme si fermé
	public float GetLength ()
	{
		float length = 0;
		foreach (Edge2 edge in _edges)
		{
			length += edge.GetLength ();
		}
		
		return length;
	}
	
	// aire signée du polygon
	public double GetArea ()
	{
		if (!_closed)
			return double.PositiveInfinity;
		
		float area = 0;
		
		for (int i = 0; i < _points.Count; ++i)
		{
			Point2 p1 = _points[i];
			Point2 p2 = _points[i + 1];
			
			area += p1.GetX () * p2.GetY () - p2.GetX () * p1.GetY ();
		}
		
		area /= 2;
		
		return area;
	}
	
	public LoopedList<Point2> GetPoints ()
	{
		return _points;	
	}
	
	public LoopedList<Edge2> GetEdges ()
	{
		return _edges;	
	}
	
	public Vector2 [] ToVector2Array ()
	{
		Vector2 [] array = new Vector2[_points.Count];
		
		for (int i = 0; i < _points.Count; ++i)
		{
			array[i] = _points[i];
		}
		
		return array;
	}
	
	public Bounds bounds
	{
		get
		{
			return _bounds;
		}
	}
	
	public void SetClosed (bool closed)
	{
		_closed = closed;	
	}
	
	public bool IsClosed ()
	{
		return _closed;	
	}
	
	// Donne la position sur le polygone selon une quantité entre 0 et 1
	// 0 correspondant au point 0, 0.5 à la moitié, en terme de perimetre, du polygone.
	public Vector2 GetPositionAt (float quantity)
	{
		if (quantity < 0 || Utils.Approximately (quantity, 0))
		{
			return _points[0];	
		}
		
		if (quantity > 1 || Utils.Approximately (quantity, 1))
		{
			if (_closed)
			{
				return _points[0];
			}
			else
			{
				return _points[_points.Count - 1];
			}
		}
		
		float perimeter = GetPerimeter ();
		float length = quantity * perimeter;
		
		Edge2 currentEdge = _edges[0];
		float lengthAcc = 0;
		
		while (currentEdge.GetLength () + lengthAcc < length)
		{
			lengthAcc += currentEdge.GetLength ();
			currentEdge = currentEdge.GetNextPoint2 ().GetNextEdge ();
		}
		
		Vector2 nextPoint = currentEdge.GetNextPoint2 ();
		Vector2 prevPoint = currentEdge.GetPrevPoint2 ();
		
		Vector2 direction =  (nextPoint -prevPoint).normalized; 
		float translation = length - lengthAcc;
		
		Vector2 position = prevPoint + (direction * translation);
			
		return position;		
	}
	
	public void AddPoint (Point2 point)
	{
		if (_points.Count > 0)
		{
			Point2 lastPoint = _points[_points.Count - 1];
			Edge2 edge = new Edge2 (lastPoint, point);
			_edges.Add (edge);
			
			lastPoint.SetNextEdge (edge);
			point.SetPrevEdge (edge);
		}
		
		_points.Add (point);
		point.Update ();
		
		UpdateBounds ();
	}
	
	public void MovePoint (int index, Vector2 position)
	{
		Point2 point = _points[index];
		point.Set (position);
		
		UpdateBounds ();
	}
	
	// ferme le polygon
	public void Close ()
	{
		Point2 firstPoint = _points[0];
		Point2 lastPoint = _points[_points.Count - 1];
		
		Edge2 edge = new Edge2 (lastPoint, firstPoint);
		_edges.Add (edge);
		
		firstPoint.SetPrevEdge (edge);
		lastPoint.SetNextEdge (edge);
		
		_closed = true;
		
		//Debug.Log ("AREA " + GetArea ());
		
		if (GetArea () > 0)
		{
			//Debug.Log ("Reverse Polygon");
			_points.Reverse ();
			_edges.Reverse ();
			
			for (int i = 0; i < _points.Count; ++i)
			{
				Point2 currentPoint = _points[i];
				Point2 nextPoint = _points[i + 1];
				
				Edge2 prevEdge = _edges[i - 1];
				Edge2 currentEdge = _edges[i];
				
				currentEdge.SetPrevPoint2 (currentPoint);
				currentEdge.SetNextPoint2 (nextPoint);
				
				currentPoint.SetPrevEdge (prevEdge);
				currentPoint.SetNextEdge (currentEdge);
			}
		}
		
		UpdateBounds ();
	}
	
	// ferme et fusionne le dernier point avec le premier
	public void CloseAndFusion ()
	{
		_points.RemoveAt (_points.Count - 1);
		
		Point2 firstPoint = _points[0];
		Point2 lastPoint = _points[_points.Count - 1];
		Edge2 lastEdge = _edges[_edges.Count - 1];
		
		firstPoint.SetPrevEdge (lastEdge);		
		lastEdge.SetNextPoint2 (firstPoint);
		
		_closed = true;
		
		if (GetArea () > 0)
		{
			_points.Reverse ();
			_edges.Reverse ();
			
			for (int i = 0; i < _points.Count; ++i)
			{
				Point2 currentPoint = _points[i];
				Point2 nextPoint = _points[i + 1];
				
				Edge2 prevEdge = _edges[i - 1];
				Edge2 currentEdge = _edges[i];
				
				currentEdge.SetPrevPoint2 (currentPoint);
				currentEdge.SetNextPoint2 (nextPoint);
				
				currentPoint.SetPrevEdge (prevEdge);
				currentPoint.SetNextEdge (currentEdge);
			}
		}
		
		UpdateBounds ();
	}
	
	public void InsertPoint (Edge2 edge, Point2 point)
	{
		int edgeAndPointIndex = _edges.IndexOf (edge);
		
		if (edgeAndPointIndex < 0)
			return;
				
		Point2 nextPoint = edge.GetNextPoint2 ();
		Edge2 newEdge = new Edge2 (point, nextPoint);
		edge.SetNextPoint2 (point);
		point.SetEdges (edge, newEdge);
		nextPoint.SetPrevEdge (newEdge);
		
		_edges.Insert (edgeAndPointIndex + 1, newEdge);
		_points.Insert (edgeAndPointIndex + 1, point);
		
		Point2 prevPoint = edge.GetPrevPoint2 ();
		prevPoint.Update (false);	
		point.Update (false);
		nextPoint.Update (false);
		
		UpdateBounds ();
	}
	
	public void InsertPoint (int edgeIndex, Point2 point)
	{
		Edge2 edge = _edges[edgeIndex];
		
		InsertPoint (edge, point);
	}
	
	public void RemovePoint (Point2 point)
	{
		int index = _points.IndexOf (point);
		
		if (index >= 0)
		{
			RemovePoint (index);
		}
	}
	
	public void RemovePoint (int pointIndex)
	{
		Point2 pointToRemove = _points[pointIndex];
		Point2 nextPoint = _points[pointIndex + 1];
		
		Edge2 prevEdge = pointToRemove.GetPrevEdge ();
		Edge2 nextEdge = pointToRemove.GetNextEdge ();
		
		if (prevEdge != null)
		{
			if (nextEdge != null)
			{
				Point2 prevPoint = prevEdge.GetPrevPoint2 ();
				
				if (prevPoint.GetPrevEdge () != nextPoint.GetNextEdge ())
				{
					prevEdge.SetNextPoint2 (nextPoint);
					prevEdge.GetNextPoint2 ().Update (false);
					prevEdge.GetPrevPoint2 ().Update (false);
					nextPoint.Update (false);
				}
				else
				{
					prevPoint.SetNextEdge (null);
					
					if (prevPoint.GetJunction () == JunctionType.Curved)
					{
						SetJunctionType (pointIndex - 1, JunctionType.Broken);
					}
					
					nextPoint.SetPrevEdge (null);
					
					if (nextPoint.GetJunction () == JunctionType.Curved)
					{
						SetJunctionType (pointIndex + 1, JunctionType.Broken);
					}
					
					_edges.Remove (prevEdge);
				}
			}
			else
			{
				Point2 prevPoint = prevEdge.GetPrevPoint2 ();
				prevPoint.SetNextEdge (null);
				
				if (nextPoint.GetJunction () == JunctionType.Curved)
				{
					SetJunctionType (pointIndex + 1, JunctionType.Broken);
				}
				
				prevPoint.Update ();
				_edges.Remove (prevEdge);
			}
			
			nextPoint.SetPrevEdge (prevEdge);
		}
		else
		{
			nextPoint.SetPrevEdge (null);
			
			if (nextPoint.GetJunction () == JunctionType.Curved)
			{
				SetJunctionType (pointIndex + 1, JunctionType.Broken);
			}
			
			nextPoint.Update (false);	
		}
		
		if (nextEdge != null)
		{
			_edges.Remove (nextEdge);
		}
		
		pointToRemove.SetEdges (null, null);
		
		_points.RemoveAt (pointIndex);
		
		UpdateBounds ();
	}
	
	public void SetJunctionType (int index, JunctionType junction)
	{
		Point2 selectedPoint = _points[index];
		
		if (selectedPoint.GetJunction () != junction)
		{
			if (junction == JunctionType.Curved)
			{
				ArchedPoint2 ap = new ArchedPoint2 (selectedPoint);
				_points[index] = ap;
			}
			else if (junction == JunctionType.Broken)
			{
				Point2 newPoint = new Point2 (selectedPoint);
				newPoint.SetPrevEdge (selectedPoint.GetPrevEdge ());
				newPoint.SetNextEdge (selectedPoint.GetNextEdge ());
				
				Edge2 prevEdge = newPoint.GetPrevEdge ();
				if (prevEdge != null)
				{
					prevEdge.SetNextPoint2 (newPoint);
				}
				
				Edge2 nextEdge = newPoint.GetNextEdge ();
				if (nextEdge != null)
				{
					nextEdge.SetPrevPoint2 (newPoint);
				}
				
				_points[index] = newPoint;
			}
		}
	}
	
	public void Clear ()
	{
		_points.Clear ();
		_edges.Clear ();
		_closed = false;
		_bounds = new Bounds (Vector3.zero, Vector3.zero);
	}
	
	public Polygon GetMirrorX()
	{
		Polygon newPolygon = new Polygon();
		Point2Data[] newPointsData = new Point2Data[_points.Count];
		CopyPoint2DataToInverse(newPointsData);
		newPolygon = new Polygon(newPointsData);
	
	// ensemble de points et segments lié entre eux et ordonnés dans le sens antihoraire
//	protected LoopedList <Point2> _points = new LoopedList<Point2> ();
//	protected LoopedList <Edge2> _edges = new LoopedList<Edge2> ();
		return newPolygon;
	}
	
	// genere un ensemble de points correspondant au polygone avec ses avec arc de cercle
	public PolygonRawData GenerateWithCurveInverse ()
	{
		PolygonRawData curvedPolygon = new PolygonRawData ();
		float minX = 0.0f;
		float maxX = 0.0f;
		float median=0.0f;
		foreach (Point2 point in _points)
		{
			if(point.GetX()>maxX)
				maxX=point.GetX();
			if(point.GetX()<minX)
				minX=point.GetX();			
		}
		median = (maxX+minX)/2.0f;
			
		foreach (Point2 point in _points)
		//for (int i=_points.Count-1; i>=0; i--)
		{
		//	Point2 point = _points[i];
			float decalage = median - point.GetX();
			float newX = median + decalage;
			point.Set(
				newX,
				point.GetY()
				);
			/*point.Set(
				point.GetX(),
				point.GetY()
				);*/
			if (point.GetJunction () == JunctionType.Broken)
			{
				curvedPolygon.Add (point);	
			}
			else if (point.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 ar = point as ArchedPoint2;
				
				foreach (Vector2 v in ar.GetCurve ())
				{
					curvedPolygon.Add (v);	
				}
			}
		}	
		
		return curvedPolygon;
	}
	// genere un ensemble de points correspondant au polygone avec ses avec arc de cercle
	public PolygonRawData GenerateWithCurve ()
	{
		PolygonRawData curvedPolygon = new PolygonRawData ();
		float minX = 0.0f;
		float maxX = 0.0f;
		float median=0.0f;
		foreach (Point2 point in _points)
		{
			if(point.GetX()>maxX)
				maxX=point.GetX();
			if(point.GetX()<minX)
				minX=point.GetX();			
		}
		median = (maxX+minX)/2.0f;
			
		foreach (Point2 point in _points)
		//for (int i=_points.Count-1; i>=0; i--)
		{
		//	Point2 point = _points[i];
			float decalage = median - point.GetX();
			float newX = median + decalage;
			/*point.Set(
				newX,
				point.GetY()
				);*/
			point.Set(
				point.GetX(),
				point.GetY()
				);
			if (point.GetJunction () == JunctionType.Broken)
			{
				curvedPolygon.Add (point);	
			}
			else if (point.GetJunction () == JunctionType.Curved)
			{
				ArchedPoint2 ar = point as ArchedPoint2;
				
				foreach (Vector2 v in ar.GetCurve ())
				{
					curvedPolygon.Add (v);	
				}
			}
		}	
		
		return curvedPolygon;
	}
	
	// Copy le polygon sous forme d'un ensemble de points uniquement avec les données de points
	// (arc de cercle geres sous forme de donnees et pas de points)
	public void CopyPoint2DataTo (Point2Data[] pointArray)
	{
		
		for (int pIndex = 0;  pIndex < _points.Count; pIndex++)
		{
			Point2 currentPoint = _points[pIndex];
			Point2Data pData = new Point2Data ();			
			
			pData.position = currentPoint;
			pData.junctionType = currentPoint.GetJunction ();
			
			if (pData.junctionType == JunctionType.Curved)
			{
				ArchedPoint2 aPoint = currentPoint as ArchedPoint2;
				pData.radius = aPoint.GetMeasuredRadius ();
			}
			
			pointArray[pIndex] = pData;
		}
	}
		
	// Copy le polygon sous forme d'un ensemble de points uniquement avec les données de points
	// (arc de cercle geres sous forme de donnees et pas de points)
	// mais en inversant en mode miroire X
	public void CopyPoint2DataToInverse (Point2Data[] pointArray)
	{
		Point2Data[] pointArrayTemp = new Point2Data[_points.Count];
		for (int pIndex = 0;  pIndex < _points.Count; pIndex++)
		{
			Point2 currentPoint = _points[pIndex];
			Point2Data pData = new Point2Data ();			
			
			pData.position = currentPoint;
			pData.position.y = 2*this.bounds.center.x - currentPoint.GetY();
			pData.junctionType = currentPoint.GetJunction ();
			
			if (pData.junctionType == JunctionType.Curved)
			{
				ArchedPoint2 aPoint = currentPoint as ArchedPoint2;
				pData.radius = aPoint.GetMeasuredRadius ();
			}			
			pointArrayTemp[pIndex] = pData;
		//	pointArray[pIndex] = pData;
		}
		
		/*for (int pIndex = 0;  pIndex < pointArrayTemp.Length; pIndex++)
		{
			if (pIndex>0)
			{
				pointArray[pIndex].junctionType = pointArrayTemp[pIndex-1].junctionType;
				pointArray[pIndex].radius = pointArrayTemp[pIndex-1].radius;
			}
			else
			{
				pointArray[pIndex].junctionType = pointArrayTemp[pointArray.Length-1].junctionType;
				pointArray[pIndex].radius = pointArrayTemp[pointArray.Length-1].radius;
			}
		}*/
		for (int pIndex = 0;  pIndex < pointArrayTemp.Length; pIndex++)
		{
			pointArray[pIndex] = pointArrayTemp[pointArrayTemp.Length - pIndex -1];
		}
	}
	
	public Vector2 GetPolygonCenter ()
	{
		return _polygonCenter;	
	}
	
	/*public void DebugLoop ()
	{
		Point2 firstPoint = _points[0];
		Point2 lastPoint = _points[_points.Count - 1];
		Point2 currentPoint = firstPoint;
		Edge2 nextEdge = currentPoint.GetNextEdge ();
		int count = 0;
		
		
		while (currentPoint != null && nextEdge != null)
		{
			Debug.Log (count++ + " " + (Vector2)currentPoint);
			currentPoint = nextEdge.GetNextPoint2 ();
			nextEdge = currentPoint.GetNextEdge ();
			
			if (currentPoint == firstPoint)
			{
				break;	
			}
		}
	}*/
}