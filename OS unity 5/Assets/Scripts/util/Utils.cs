using UnityEngine;
using System.Collections;

// TODO ajouter le package Pointcube.Utils, et le charger dans ArcDrawer, ArchedPoint2, Point2, Polygon, Snapper,
//    PoolDesignerUI, GeometryTools, RimGenerator, RimSubdivider

public class Utils 
{
	public static bool Approximately(float a, float b) 
	{ 
		return Approximately(a, b, 0.01f) ; 
	}
	public static bool Approximately(float a, float b, float tolerance) 
	{ 
		return Mathf.Abs(a - b) < tolerance; 
	}

    //-----------------------------------------------------
    // Affiche une texture tilée. A appeler d'un OnGUI
    // (la texture sera devant toutes les GUITextures).
    public static void TileGUItexture(Texture texture, Rect tile, Rect areaToFill, ScaleMode scaleMode)
    {
        // Tiles an <areaToFill> with a <texture> which is scaled to the size
        // of a <tile> using a given <scaleMode>. Author: Isaac Manning Dart
        for (float y = areaToFill.y; y < areaToFill.y + areaToFill.height; y = y + tile.height)
        {
            for (float x = areaToFill.x; x < areaToFill.x + areaToFill.width; x = x + tile.width)
            {
                tile.x = x; tile.y = y;
                GUI.DrawTexture(tile, texture, scaleMode);
            }
        }
    } // TileGUItexture()
}
