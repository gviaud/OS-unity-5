using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Genere un mesh a partir d'une forme de base et d'un profile
// represente par des polygones
// genere de meme les uvs pour une margelle de taille données
public class PolygonLofter
{
	protected float rimSize = 0.45f; // taille d'une margelle en m
	protected float rimWidth = 0.33f; // largeur d'une margelle en m
		
	protected Polygon _polygon; // forme (souvent une portion de margelle donc polygone ouvert)
	protected Polygon _shape; // profile
	
	protected Mesh _mesh;
	
	// list des indices de debut et fin d'arc de cercle
	// ex : [indiceDebut0, indiceFin0, indiceDebut1, indiceFin1, ...,]
	protected List<int> _curveIndices;
	protected Dictionary <int, AngleType> _indexedAngleType;
	
	protected int _indexOffset;
	protected int _rimCount;
	
	// nb de point du polygone entier
	// et non de _polygon qui correspond a une portion
	// dans la majorite des cas
	protected int _wholePolyCount;
	
	protected bool _isCurved = false;
	
	public PolygonLofter (Polygon polygon, Polygon shape, Mesh mesh)
	{
		_polygon = polygon;
		_shape = shape;
		_mesh = mesh;
	}
	
	protected void SetNormals ()
	{
		_mesh.RecalculateNormals ();
		
		if (_polygon.IsClosed ())
		{
			// fusion les normales de debut et fin de mesh
			// pour eviter une jonction visible			
			Vector3 [] normals = _mesh.normals;
			int shapePointCount = _shape.GetPoints ().Count;
			
			for (int startIndex = 0, endIndex = _mesh.vertexCount - shapePointCount;
				 startIndex < shapePointCount;
				 ++startIndex, ++endIndex)
			{
				Vector3 normal = normals[startIndex] + normals[endIndex];
				normal.Normalize ();
				normals[startIndex] = normal;
				normals[endIndex] = normal;
			}
			
			_mesh.normals = normals;
		}
	}
	
	protected void SetVertices ()
	{
		LoopedList<Point2> polyPoints = _polygon.GetPoints ();
		LoopedList<Point2> shapePoints = _shape.GetPoints ();
		
		// calcul du nombre de vertices à generer selon la forme de base et le profile
		int verticesCount = polyPoints.Count * shapePoints.Count;
		if (_polygon.IsClosed ())
		{
			verticesCount += shapePoints.Count;	
		}
		
		Vector3 [] vertices = new Vector3[verticesCount];
		int verticesIndex = 0;
		
		Vector2 [] uvs = new Vector2[verticesCount];
		float u = 0;
		
		float shapeLength = _shape.GetLength () + 0.01f;
		bool inCurve = false;
		
		// for mapping uses
		int edgesCount = 0;
		float curveLength = 0;
		float curveScaleFactor = 1;
		
		// pour chaque point de la forme de base
		for (int pointsIndex = 0; pointsIndex < polyPoints.Count; ++pointsIndex)
		{
			Point2 currentPoint = polyPoints[pointsIndex];
			
			// calcul de la matrice de transformation pour generer les points du profile associe
			// au point de la forme courant
			Vector3 currentVertex = new Vector3 (currentPoint.GetX (), 0, currentPoint.GetY ());
			Vector3 currentNormal = new Vector3 (currentPoint.GetNormal ().x, 0, currentPoint.GetNormal ().y);
			Quaternion currentRot = Quaternion.FromToRotation (Vector3.right, currentNormal);			
			Vector3 currentScale = new Vector3(currentPoint.GetNormalScaleFactor (), 1, 1);

			Matrix4x4 currentMatrix = Matrix4x4.TRS (currentVertex, currentRot, currentScale);
			
			float v = 0;
			
			// pour chaque point du profile
			for (int shapeIndex = 0; shapeIndex < shapePoints.Count; ++shapeIndex)
			{
				Point2 shapePoint = shapePoints[shapeIndex];
				
				Vector2 shapeVector = shapePoint;
				Vector3 vertex = new Vector3 (shapeVector.x, shapeVector.y, 0);
				
				// calcul de la position du point du profil selon la transformation currentMatrix
				Vector3 transCurrentVertex = currentMatrix.MultiplyPoint (vertex);				
				vertices[verticesIndex] = transCurrentVertex;
				uvs[verticesIndex++] = new Vector2 (v, u);
				
				// mise a jour de la coordonnees v de mapping selon la longueur du segment suivant
				Edge2 nextEdgeShape = shapePoint.GetNextEdge ();
				if (nextEdgeShape != null)
				{
					float nextV = nextEdgeShape.GetLength () / shapeLength;
					v += nextV;
				}
			}
			
			Edge2 nextEdge = currentPoint.GetNextEdge ();
			if (nextEdge != null)
			{
				if (_isCurved && _curveIndices.Count > 0)
				{				
					// si notre polygon a des arc de cercle 
					// on verifie si on est dans entrain lofter un arc de cercle
					
					// on calcul l'indice dans le polygone complet a partir de l'indice
					// du point courant (indice dans la portion) et de l'offset
					int searchedIndex = pointsIndex + _indexOffset;
					
					if (searchedIndex >= _wholePolyCount)
					{
						searchedIndex = (pointsIndex + _indexOffset) - _wholePolyCount;
					}
					
					int listIndex = _curveIndices.IndexOf (searchedIndex);
					
					// si cet indice est dans la liste
					// soit on entre dans un arc
					// soit on termine un arc
					if (listIndex >= 0)
					{		
//						if (!inCurve)
//							Debug.Log ("START CURVE AT " + searchedIndex);
//						else
//							Debug.Log ("STOP CURVE AT " + searchedIndex);
							
						_curveIndices.RemoveAt (listIndex);
						
						if (!inCurve)
						{
							inCurve = true;
							
							int ptCount = _curveIndices[listIndex] - searchedIndex;
							curveLength = 0;
							
							// on retrouve le type d'angle associé a l'arc de cercle
							AngleType angleType = _indexedAngleType[searchedIndex];
							
							if (angleType == AngleType.Outside)
							{
								// pour les angle interieur c'est la longueur de l'arc exterieur de la margelle
								// que l'on utilise comme reference pour calculer le nombre de margelle
								Polygon outlinedPolygon = PolygonOperation.GetOutlinedPolygon (_polygon, rimWidth);
								LoopedList <Point2> outlinedPolyPoints = outlinedPolygon.GetPoints ();
								
								for (int curvePtIndex = pointsIndex; curvePtIndex < pointsIndex + ptCount; ++curvePtIndex)
								{
									curveLength += outlinedPolyPoints[curvePtIndex].GetNextEdge ().GetLength ();
								}
								
								float insideCurveLength = 0;
								for (int curvePtIndex = pointsIndex; curvePtIndex < pointsIndex + ptCount; ++curvePtIndex)
								{
									insideCurveLength += polyPoints[curvePtIndex].GetNextEdge ().GetLength ();
								}
								
								// facteur de reduction entre les longueurs de l'arc exterieur et interieur
								// utilisé pour pour calculer la longueur des segments exterieur
								curveScaleFactor = curveLength / insideCurveLength;
								
								edgesCount = Mathf.RoundToInt (curveLength / rimSize);

							}
							else
							{
								// pour les angle exterieur c'est la longueur de l'arc interieur de la margelle
								// que l'on utilise comme reference pour calculer le nombre de margelle
								curveScaleFactor = 1;
								for (int curvePtIndex = pointsIndex; curvePtIndex < pointsIndex + ptCount; ++curvePtIndex)
								{
									curveLength += polyPoints[curvePtIndex].GetNextEdge ().GetLength ();
								}
								
								edgesCount = Mathf.RoundToInt (curveLength / rimSize);
							}	
							
							_rimCount += edgesCount;
						}
						else
						{
							inCurve = false;
						}
					}
				}
				
				if (inCurve)
				{
					// si on est dans un arc on calcul la coordonnee u en fonction de la longueur de l'arc et
					// de la longueur d'une margelle
					u += ((nextEdge.GetLength () * curveScaleFactor) / curveLength) * edgesCount;
				}
				else
				{
					// sinon on calcul la coordonnee u en fonction de la longueur du segment suivant
					// et de la longueur d'une margelle
					int rimCount = Mathf.RoundToInt (nextEdge.GetLength () / rimSize);
					
					if (rimCount <= 0)
					{
						rimCount = 1;	
					}
					
					_rimCount += rimCount;
					
					u += rimCount;
				}
			}	
		}
		
		if (_polygon.IsClosed ())
		{
			// si le polygone est ferme, on double les points de departs
			float v = 0;
			
			for (int shapeIndex = 0, endIndex = verticesCount - shapePoints.Count;
				 shapeIndex < shapePoints.Count;
				 ++shapeIndex, ++endIndex)
			{
				Point2 shapePoint = shapePoints[shapeIndex];
				
				vertices[endIndex] = vertices[shapeIndex];
				uvs[endIndex] = new Vector2 (v, u);
				
				Edge2 nextEdgeShape = shapePoint.GetNextEdge ();
				if (nextEdgeShape != null)
				{
					float nextV = nextEdgeShape.GetLength () / shapeLength;
					v += nextV;	
				}
			}
		}
		
		_mesh.vertices = vertices;
		_mesh.uv = uvs;
	}
	
	protected void SetIndices ()
	{
		LoopedList<Point2> polyPoints = _polygon.GetPoints ();
		LoopedList<Point2> shapePoints = _shape.GetPoints ();
		
		int polyCount = polyPoints.Count - 1;
		if (_polygon.IsClosed ())
		{
			++polyCount	;
		}
		
		int shapeCount = shapePoints.Count - 1;
		int shapeOffset = shapePoints.Count;
		
		if (_shape.IsClosed ())
		{
			++shapeCount;
		}
		
		int indicesCount = polyCount * shapeCount * 6;
		int indicesIndex = 0;
		int [] indices = new int[indicesCount];
		
		for (int pointsIndex = 0; pointsIndex < polyPoints.Count - 1; ++pointsIndex)
		{
			for (int shapeIndex = 0; shapeIndex < shapePoints.Count - 1; ++shapeIndex, indicesIndex += 6)
			{
				int baseIndex = pointsIndex * shapePoints.Count + shapeIndex;
					
				indices[indicesIndex] = baseIndex;
				indices[indicesIndex + 1] = baseIndex + 1;
				indices[indicesIndex + 2] = baseIndex + shapeOffset;
				indices[indicesIndex + 3] = baseIndex + 1;
				indices[indicesIndex + 4] = baseIndex + shapeOffset + 1;
				indices[indicesIndex + 5] = baseIndex + shapeOffset;
			}
			
			if (_shape.IsClosed ())
			{
				int firstIndex = pointsIndex * shapePoints.Count;
				int lastIndex = firstIndex + (shapePoints.Count - 1);
				
				indices[indicesIndex] = lastIndex;
				indices[indicesIndex + 1] = firstIndex;
				indices[indicesIndex + 2] = lastIndex + shapeOffset;
				indices[indicesIndex + 3] = firstIndex;
				indices[indicesIndex + 4] = firstIndex + shapeOffset;
				indices[indicesIndex + 5] = lastIndex + shapeOffset;
				
				indicesIndex += 6;
			}
		}
		
		if (_polygon.IsClosed ())
		{
			int endIndex = polyPoints.Count * shapePoints.Count;
			
			for (int shapeIndex = 0; shapeIndex < shapePoints.Count - 1; ++shapeIndex, indicesIndex += 6, ++endIndex)
			{
				int baseIndex = (polyPoints.Count - 1) * shapePoints.Count + shapeIndex;
					
				indices[indicesIndex] = baseIndex;
				indices[indicesIndex + 1] = baseIndex + 1;
				indices[indicesIndex + 2] = endIndex; //shapeIndex;
				indices[indicesIndex + 3] = baseIndex + 1;
				indices[indicesIndex + 4] = endIndex + 1; //shapeIndex + 1;
				indices[indicesIndex + 5] = endIndex; //shapeIndex;
			}
			
			if (_shape.IsClosed ())
			{
				int firstIndex = (polyPoints.Count - 1) * shapePoints.Count;
				int lastIndex = firstIndex + (shapePoints.Count - 1);
				
				indices[indicesIndex] = lastIndex;
				indices[indicesIndex + 1] = firstIndex;
				indices[indicesIndex + 2] = 0;
				indices[indicesIndex + 3] = firstIndex;
				indices[indicesIndex + 4] = 0;
				indices[indicesIndex + 5] = 1;
			}
		}
		
		_mesh.triangles = indices;
	}
	
	public Mesh GetMesh ()
	{
		return _mesh;	
	}
	
	public void Generate ()
	{
		_mesh.Clear ();
			
		SetVertices ();
		SetIndices ();
		SetNormals ();
			
		_mesh.RecalculateBounds ();	
	}
	
	public void SetCurveIndices (List<int> curveIndices)
	{
		_curveIndices = curveIndices;
		_isCurved = _curveIndices.Count > 0;
	}
	
	public void SetIndexedAngleType (Dictionary <int, AngleType> indexedAngleType)
	{
		_indexedAngleType = indexedAngleType;
	}
	
	public void SetIndexOffset (int index, int wholePolyCount)
	{
		_indexOffset = index;	
		_wholePolyCount = wholePolyCount;
	}
	
	public int GetRimCount ()
	{
		return _rimCount;	
	}
}