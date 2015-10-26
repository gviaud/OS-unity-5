using UnityEngine;
using System.Collections;

public class Line {
	
	public Vector2 src;
	public Vector2 dst;
	
	public Vector2 drawSrc;
	public Vector2 drawDst;
	
	public Vector2 dir;
	public Vector2 perpDir;
	public float width;

    private Vector2 m_scr_src;
    private Vector2 m_scr_dst;
	
	
	public void setDst(Vector2 v, float w)
	{
		dst = v;
		dir = dst - src;
		dir = dir.normalized;
		width = w;
		
		//Décommenter pour que les lignes se chevauche
		Vector2 nsrc = new Vector2(src.x-1+(dir.y*width/2)/*-(dir.x*(width/2))*/, src.y-(dir.x*width/2)/*-(dir.y*(width/2))*/);
	
		Vector2 ndst = new Vector2(dst.x-1+(dir.y*width/2)/*+(dir.x*(width/2))*/, dst.y-(dir.x*width/2)/*+(dir.y*(width/2))*/);
		
		drawSrc = nsrc;
		drawDst = ndst;
		
	}
	
	public float getLength()
	{
		return (dst-src).magnitude;	
	}
	
	public Vector2 getDir()
	{
		dir = dst - src;
		dir = dir.normalized;
		return dir;
	}
	
	public Vector2 GetSrc()
	{
	    return src;
	}

    public Vector2 GetDst()
    {
        return dst;
    }

    public void PreComputeScreenCoords(int scr_w, int scr_h/*, int xOffset, int yOffset*/)
    {
        m_scr_src = src;
        m_scr_src.x = (m_scr_src.x * scr_w)/* - xOffset*/;
        m_scr_src.y = /*scr_h -*/ (m_scr_src.y * scr_h)/* - yOffset*/;

        m_scr_dst = dst;
        m_scr_dst.x = (m_scr_dst.x * scr_w)/* - xOffset*/;
        m_scr_dst.y = /*scr_h -*/ (m_scr_dst.y * scr_h)/* - yOffset*/;
    }

    public float GetPixelLen(Rect displayRect)
    {
        Vector2 srcPxCoords = src;
        srcPxCoords.x = (srcPxCoords.x * displayRect.width) + displayRect.x;
        srcPxCoords.y = (srcPxCoords.y * displayRect.height) + displayRect.y;

        Vector2 dstPxCoords = dst;
        dstPxCoords.x = (dstPxCoords.x * displayRect.width) + displayRect.x;
        dstPxCoords.y = (dstPxCoords.y * displayRect.height) + displayRect.y;
        return Vector2.Distance(srcPxCoords, dstPxCoords);
    }

    public Vector2 GetCenter()
    {
        return (dst + src)/2f;
    }

    public Vector2 GetPixelCenter(Rect displayRect)
    {
        Vector2 srcPxCoords = src;
        srcPxCoords.x = (srcPxCoords.x * displayRect.width) + displayRect.x;
        srcPxCoords.y = (srcPxCoords.y * displayRect.height) + displayRect.y;

        Vector2 dstPxCoords = dst;
        dstPxCoords.x = (dstPxCoords.x * displayRect.width) + displayRect.x;
        dstPxCoords.y = (dstPxCoords.y * displayRect.height) + displayRect.y;
        return (dstPxCoords + srcPxCoords)/2f;
    }

    public Vector2 GetScrSrc()
    {
        return m_scr_src;
    }

    public Vector2 GetScrDst()
    {
        return m_scr_dst;
    }
	
//	public static bool isOnOrigin(Vector2 currentPos, Vector2 origin, float offset)
//	{
//		if(currentPos.x >= origin.x -offset && currentPos.x <= origin.x+offset && currentPos.y >= origin.y-offset && currentPos.y <= origin.y+offset)
//		{
////			Debug.Log("polygone referme");
//			return true;
//		}
//		else return false;
//	}
}
