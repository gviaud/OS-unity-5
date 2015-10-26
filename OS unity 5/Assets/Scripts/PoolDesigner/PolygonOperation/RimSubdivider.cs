using UnityEngine;
using System.Collections.Generic;

// Classe qui subdivise un polygone afin de generer une margelle propre
// du point de vue geometrique et du mapping
// les unite de mesure sont exprime en coordonnes ecran (le * 100)
public class RimSubdivider
{
	protected static float RIM_SIZE = 0.45f * 100; // taille d'une margelle
	protected static float MIN_INSIDE_RIM_LENGTH = 0.10f * 100; // taille min d'une margelle du cote liner
	
	protected PolygonRawData _subdividedPolygon = new PolygonRawData ();
	
	// liste des indices de debut et fin de courbe
	// utilise pour le mapping de la margelle dans les courbes
	protected List<int> _curveIndices = new List<int> ();
	
	// dictionnaire associant un indice de debut de courbe à un type d'angle
	// utilisé pour le mapping des margelles
	protected Dictionary <int, AngleType> _indexedAngleType = new Dictionary<int, AngleType> ();
	
	// facteur pour exprimer les longueurs en coordonnees monde
	protected float _polygonScaleFactor = 0.01f;
	
	public RimSubdivider ()
	{}
	
	public void Subdivide (Polygon _polygon)
	{
		//_polygon = _polygon.GetMirrorX();
		_subdividedPolygon.Clear ();
		_curveIndices.Clear ();
		_indexedAngleType.Clear ();
		
		// boundingBox de l'element de margelle correspondant a l'angle 90°
		//Bounds rightAngleRimBounds = new Bounds (new Vector3 (-0.085f, 0.025f, -0.085f) * 100, new Vector3 (0.45f, 0.05f, 0.45f) * 100);
		//Bounds rightAngleRimBounds = new Bounds (new Vector3 (-0.15f, 0.00005f, -0.15f)*100, new Vector3 (0.3f, 0.0001f, 0.3f)*100);
		Bounds rightAngleRimBounds = new Bounds (new Vector3 (-0.13f, 1.0f, -0.13f) * 100, new Vector3 (0.30f, 1.0f, 0.30f)*100);
		//Bounds rightAngleRimBounds = new Bounds (new Vector3 (-0.075f, 0.00005f, -0.075f) * 100, new Vector3 (0.15f, 0.05f, 0.15f)*100);
		
		// longueur des coupures a realiser avant et apres les points d'angle 90°
		float xCutOffset = rightAngleRimBounds.center.x + rightAngleRimBounds.extents.x;
		float zCutOffset = rightAngleRimBounds.center.z + rightAngleRimBounds.extents.z;
		
		int ptIndices = 0;
		
		// pour chaque point
		//_polygon.UpdateBounds();
		//Bounds polyBounds = _polygon.bounds;
		foreach (Point2 currentPoint in _polygon.GetPoints ())
		{
			//Point2 currentPoint = (Point2) currentPoint2.Clone();
			//Point2 currentPoint = currentPoint2.;
			//currentPoint.SetX(currentPoint2.GetX()*1);
			//currentPoint.SetX(2*polyBounds.center.x + currentPoint.GetX());
			if (currentPoint.GetJunction () == JunctionType.Broken)
			{ 
				// si c'est un angle de 90°
				if (Utils.Approximately (currentPoint.GetAngle (), 90) && currentPoint.GetAngleType () == AngleType.Outside)
				{	
					float xCut = xCutOffset;
					float zCut = zCutOffset;
					//Debug.Log("xCutOffset : "+xCutOffset);
					//Debug.Log("zCutOffset : "+zCutOffset);
					
					AngleType angleType = currentPoint.GetAngleType ();
					
	//				if (angleType == AngleType.Inside)
	//				{
	//					xCut = rightAngleRimBounds.size.x;
	//					zCut = rightAngleRimBounds.size.z;
	//				}
					
					// si angle exterieur
					if (angleType == AngleType.Inside)
					{
						ptIndices++;
						_subdividedPolygon.Add (currentPoint);
					}
					
					// si angle interieur
					if (angleType == AngleType.Outside)
					{
						// On coupe avant et apres le point sur les segments precedent et suivant
						// de longueur xCut et zCut respectivement
						Vector2 prevDirection = currentPoint.GetPrevEdge ().ToVector2 ().normalized * -1;
						Vector2 prevPoint = currentPoint + prevDirection * xCut;
						
						Vector2 nextDirection = currentPoint.GetNextEdge ().ToVector2 ().normalized;
						Vector2 nextPoint = currentPoint + nextDirection * zCut;
						
						ptIndices++;
						_subdividedPolygon.Add (prevPoint);
						ptIndices++;
						_subdividedPolygon.Add (currentPoint);
						ptIndices++;
						_subdividedPolygon.Add (nextPoint);
					}
					
				}
				else
				{
					// pour les angle non predefini
					AngleType angleType = currentPoint.GetAngleType ();
					
					float rimWidth = 0.31f * 100; // largeur de la margelle
					float sectionLength = rimWidth * currentPoint.GetNormalScaleFactor ();
					float outsideLength = Mathf.Sqrt (sectionLength * sectionLength - rimWidth * rimWidth);
					float subdividedLimit = RIM_SIZE;// + RIM_SIZE / 2;
					
					// si angle exterieur
					if (angleType == AngleType.Inside)
					{
						Edge2 prevEdge = currentPoint.GetPrevEdge ();
						
						if (prevEdge != null && _subdividedPolygon.Count > 0)
						{
							// verification si le point tangent et le point correspondant a la futur coupure sont a fusionner
							// si oui pas de coupure
							bool merged = false;
							ArchedPoint2 prevAPoint = prevEdge.GetPrevPoint2 () as ArchedPoint2;
							
							if (prevAPoint != null && prevAPoint.IsNextTangentPointMerged ())
							{
								merged = true;
							}
							
							if (!merged) 
							{
								// on cree la coupure sur le segment precedent le point courant en fonction de la longueur 
								// de la margelle coté plage (exterieur car angle interieur)
								Vector2 prevSubdVector = _subdividedPolygon[_subdividedPolygon.Count - 1];
								float prevEdgeLength = Vector2.Distance (currentPoint, prevSubdVector);
								
								if (prevEdgeLength > subdividedLimit)
								{
									if (outsideLength >= RIM_SIZE - MIN_INSIDE_RIM_LENGTH)
									{
										Vector2 prevDirection = prevEdge.ToVector2 ().normalized * -1;
										Vector2 prevPoint = currentPoint + prevDirection * (outsideLength + MIN_INSIDE_RIM_LENGTH);
										
										ptIndices++;
										_subdividedPolygon.Add (prevPoint);
									}
									else
									{
										Vector2 prevDirection = prevEdge.ToVector2 ().normalized * -1;
										Vector2 prevPoint = currentPoint + prevDirection * RIM_SIZE;
										
										ptIndices++;
										_subdividedPolygon.Add (prevPoint);
									}
								}
							}
						}
						
						ptIndices++;
						_subdividedPolygon.Add (currentPoint);	// ajout du point courant
						
						Edge2 nextEdge = currentPoint.GetNextEdge ();
						
						// meme procedure pour la coupure sur le segment suivant le point courant
						if (nextEdge != null && nextEdge.GetLength () > subdividedLimit)
						{
							bool merged = false;
							ArchedPoint2 nextAPoint = nextEdge.GetNextPoint2 () as ArchedPoint2;
							
							if (nextAPoint != null && nextAPoint.IsPrevTangentPointMerged ())
							{
								merged = true;
							}
							
							if (!merged)
							{
								if (outsideLength >= RIM_SIZE - MIN_INSIDE_RIM_LENGTH)
								{
									Vector2 nextDirection = nextEdge.ToVector2 ().normalized;
							//		nextDirection.y*=-1;
									Vector2 nextPoint = currentPoint + nextDirection * (outsideLength + MIN_INSIDE_RIM_LENGTH);
									
									ptIndices++;
									_subdividedPolygon.Add (nextPoint);
								}
								else
								{
									Vector2 nextDirection = nextEdge.ToVector2 ().normalized;
							//		nextDirection.y*=-1;
									Vector2 nextPoint = currentPoint + nextDirection * RIM_SIZE;
									
									ptIndices++;
									_subdividedPolygon.Add (nextPoint);
								}
							}
						}
					}
					
					// si angle interieur
					// meme procedure que precedemment mais en prenant comme reference la longueur de margelle
					// cote liner (longueur interieur car angle exterieur)
					if (angleType == AngleType.Outside)
					{
						Edge2 prevEdge = currentPoint.GetPrevEdge ();
						
						if (prevEdge != null && _subdividedPolygon.Count > 0)
						{
							bool merged = false;
							ArchedPoint2 prevAPoint = prevEdge.GetPrevPoint2 () as ArchedPoint2;
							
							if (prevAPoint != null && prevAPoint.IsNextTangentPointMerged ())
							{
								merged = true;
							}
							
							if (!merged)
							{
								Vector2 prevSubdVector = _subdividedPolygon[_subdividedPolygon.Count - 1];
								float prevEdgeLength = Vector2.Distance (currentPoint, prevSubdVector);
								
								if (prevEdgeLength > subdividedLimit)
								{
									if (outsideLength <= RIM_SIZE - MIN_INSIDE_RIM_LENGTH)
									{
										float length = RIM_SIZE - outsideLength;
										Vector2 prevDirection = prevEdge.ToVector2 ().normalized * -1;
										Vector2 prevPoint = currentPoint + prevDirection * length;
										
										ptIndices++;
										_subdividedPolygon.Add (prevPoint);
									}
									else
									{
										Vector2 prevDirection = prevEdge.ToVector2 ().normalized * -1;
									//	prevDirection.y*=-1;
										Vector2 prevPoint = currentPoint + prevDirection * MIN_INSIDE_RIM_LENGTH;
										
										ptIndices++;
										_subdividedPolygon.Add (prevPoint);
									}
								}
							}
						}
						
						ptIndices++;
						_subdividedPolygon.Add (currentPoint);
						
						Edge2 nextEdge = currentPoint.GetNextEdge ();
						
						if (nextEdge != null && nextEdge.GetLength () > subdividedLimit)
						{
							bool merged = false;
							ArchedPoint2 nextAPoint = nextEdge.GetNextPoint2 () as ArchedPoint2;
							
							if (nextAPoint != null && nextAPoint.IsPrevTangentPointMerged ())
							{
								merged = true;
							}
							
							if (!merged)
							{
								if (outsideLength <= RIM_SIZE - MIN_INSIDE_RIM_LENGTH)
								{
									float length = RIM_SIZE - outsideLength;
									Vector2 nextDirection = nextEdge.ToVector2 ().normalized;
									Vector2 nextPoint = currentPoint + nextDirection * length;
									
									ptIndices++;
									_subdividedPolygon.Add (nextPoint);
								}
								else
								{
									Vector2 nextDirection = nextEdge.ToVector2 ().normalized;
									
							//		nextDirection.y*=-1;
									Vector2 nextPoint = currentPoint + nextDirection * MIN_INSIDE_RIM_LENGTH;
									
									ptIndices++;
									_subdividedPolygon.Add (nextPoint);
								}
							}
						}
					}
				}
			}
			else if (currentPoint.GetJunction () == JunctionType.Curved)
			{	
				// si on est dans un arc de cercle pas de subdivision a effectuer
				// on ajoute chaque point de l'arc de cercle
				ArchedPoint2 ar = currentPoint as ArchedPoint2;
				
				int startCurveIndex = _subdividedPolygon.Count;
				
				// ajout de l'indice du premier point de l'arc de cercle
				// pour le mapping dans PolygonLofter
				_curveIndices.Add (startCurveIndex);
				
				// on associe l'indice de debut de courbe au type d'angle courant
				_indexedAngleType.Add (startCurveIndex, currentPoint.GetAngleType ());
				
				foreach (Vector2 p in ar.GetCurve ())
				{
					ptIndices++;
					_subdividedPolygon.Add (p);	
				}
				
				int endCurveIndex = _subdividedPolygon.Count - 1;

				// ajout de l'indice du dernier point de l'arc de cercle
				// pour le mapping dans PolygonLofter
				_curveIndices.Add (endCurveIndex);
			}
		}
		
		// decoupage au niveau du point 0 et du segment precedent le point 0
		// meme procedure que precedemment
		Point2 firstPoint = _polygon.GetPoints ()[0];
		Edge2 firstPrevEdge = firstPoint.GetPrevEdge ();
		
		if (firstPrevEdge != null && firstPoint.GetJunction () == JunctionType.Broken)
		{
			float rimWidth = 0.31f * 100;
			float sectionLength = rimWidth * firstPoint.GetNormalScaleFactor ();
			float outsideLength = Mathf.Sqrt (sectionLength * sectionLength - rimWidth * rimWidth);
			float subdividedLimit = RIM_SIZE; //+ RIM_SIZE / 2;
			
			if (firstPoint.GetAngleType () == AngleType.Inside)
			{
				Vector2 prevSubdVector = _subdividedPolygon[_subdividedPolygon.Count - 1];
				float prevEdgeLength = Vector2.Distance (firstPoint, prevSubdVector);
				
				if (prevEdgeLength > subdividedLimit)
				{
					if (outsideLength >= RIM_SIZE - MIN_INSIDE_RIM_LENGTH)
					{
						Vector2 prevDirection = firstPrevEdge.ToVector2 ().normalized * -1;
						Vector2 prevPoint = firstPoint + prevDirection * (outsideLength + MIN_INSIDE_RIM_LENGTH);
						
						ptIndices++;
						_subdividedPolygon.Add (prevPoint);
					}
					else
					{
						Vector2 prevDirection = firstPrevEdge.ToVector2 ().normalized * -1;
						Vector2 prevPoint = firstPoint + prevDirection * RIM_SIZE;
						
						ptIndices++;
						_subdividedPolygon.Add (prevPoint);
					}
				}
			}
			
			if (firstPoint.GetAngleType () == AngleType.Outside && !Utils.Approximately (firstPoint.GetAngle (), 90))
			{
				Vector2 prevSubdVector = _subdividedPolygon[_subdividedPolygon.Count - 1];
				float prevEdgeLength = Vector2.Distance (firstPoint, prevSubdVector);
				
				if (prevEdgeLength > subdividedLimit)
				{
					if (outsideLength <= RIM_SIZE - MIN_INSIDE_RIM_LENGTH)
					{
						float length = RIM_SIZE - outsideLength;
						Vector2 prevDirection = firstPrevEdge.ToVector2 ().normalized * -1;
						Vector2 prevPoint = firstPoint + prevDirection * length;
						
						ptIndices++;
						_subdividedPolygon.Add (prevPoint);
					}
					else
					{
						Vector2 prevDirection = firstPrevEdge.ToVector2 ().normalized * -1;
						Vector2 prevPoint = firstPoint + prevDirection * MIN_INSIDE_RIM_LENGTH;
						
						ptIndices++;
						_subdividedPolygon.Add (prevPoint);
					}
				}
			}
		}
		
		// on exprime le polygone subdivisé en coordonnées monde, avec son repere centré sur son centre
		Vector2 translation = _polygon.GetPolygonCenter () * -1;
		
		for (int ptIndex = 0; ptIndex < _subdividedPolygon.Count; ++ptIndex)
		{
			_subdividedPolygon[ptIndex] = (_subdividedPolygon[ptIndex] + translation) * _polygonScaleFactor;
		}
	}
	
	public PolygonRawData GetSubdividedPolygon ()
	{
		return _subdividedPolygon;	
	}
	
	public List<int> GetCurveIndices ()
	{
		return _curveIndices;	
	}
	
	public Dictionary <int, AngleType> GetIndexedAngleType ()
	{
		return _indexedAngleType;	
	}
}