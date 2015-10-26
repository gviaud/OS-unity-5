using UnityEngine;
using System.Collections;

// represente les indices des sommets mutuellement visible du polygone
// http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
public struct MutuallyVisibleVertices
{
	public int innerIndex;
	public int outerIndex;
	public Vector2 duplicatedMVVOffset; // offset pour la fusion apres triangulation
}
