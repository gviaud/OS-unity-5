using UnityEngine;
using System;

// Class permettant de representer un point dans unity
// position, type de jonction (arc de cercle ou angle) et rayon de l'arc de cercle
// La serialization permet de creer un array de Point2Data dans l'editor, afin de creer
// des formes predefinis. De meme cela permet la copy automatique des donnes avec la fonction
// Instantiate.
[Serializable]
public class Point2Data
{
	public Vector2 position;
	
	public float radius;
	
	public JunctionType junctionType;
	
	public bool bstairway = false;
}
