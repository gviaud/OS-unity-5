using UnityEngine;
using System.Collections;

// represente un polygone issue de la fusion de deux polygones
// dont un est entierement inclus dans l'autre (polygone avec un trou)
public class MergedPolygon
{
	public PolygonRawData polygonRawData;
	
	public int originalIndex0;
	public int originalIndex1;
	public int duplicatedIndex0;
	public int duplicatedIndex1;
}
