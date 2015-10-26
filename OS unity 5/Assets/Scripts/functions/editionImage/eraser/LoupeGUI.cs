//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/eraser/loupeGUI.cs - 02/2012 - KS
// Attaché par défaut à Background/loupe/GUITexLoupe
using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Classe permettant d'afficher l'image faisant office de loupe pour la gomme,
// ainsi que le curseur indiquant l'endroit pointé sur cette image.
// Utilisé par PolygonTracer, pour sa fonctionnalité de loupe.
public class LoupeGUI : MonoBehaviour
{
    private  Texture2D	m_cursor;    		// Texture du curseur indiquant l'endroit pointé
    private  Texture2D	m_prev_point;		// Texture du poiont précédent
    private  Texture2D	m_prev_point_sub;	// Texture du poiont précédent
    private  int		m_cur_x;     		// Position du curseur (-1 = ne pas afficher le curseur)
    private  int		m_cur_y;	
    //private  int		m_last_x;     		// Position du curseur (-1 = ne pas afficher le curseur)
    //private  int		m_last_y;
	private ArrayList	m_lastPoints;		//position des derniers points
	private bool		_invert;
	private int			_lineWidth = 7;
	
	private bool _containsLastPoint = false;
    private	Texture2D	m_lineTex;
	
	private Rect rectLoupe;
	private int _offsetLoupeRect = 37;
    
	//-----------------------------------------------------
	void Start ()
    {
	    m_cursor 			=	(Texture2D) Resources.Load("tracage/OS_LOUPE");
	    m_prev_point 		=	(Texture2D) Resources.Load("tracage/OS_DOT_ADD_INZOOM");
	    m_prev_point_sub	=	(Texture2D) Resources.Load("tracage/OS_DOT_SUB_INZOOM");
		m_lineTex    		= 	(Texture2D) Resources.Load("tracage/OS_TRACE_WHITE");
        m_cur_x  			=	-1;
        m_cur_y  			=	-1;
        //m_last_x  			= 	-1;
        //m_last_y  			= 	-1;
		m_lastPoints 		=	new ArrayList();
		_invert				=	false;
	}
	
	//-----------------------------------------------------
	void Update ()
    {}
    
    //-----------------------------------------------------
    // Tracé de la texture de la loupe. La GUITexture est désactivée dans le projet,
    // et l'image est tracée avec DrawTexture au lieu de son renderer par défaut
    // pour qu'elle passe au dessus des traits des polygones.
    void OnGUI()
    {
        if(m_cur_x != -1  &&  m_cur_y != -1)
        {
			rectLoupe = new Rect(m_cur_x+_offsetLoupeRect, m_cur_y+_offsetLoupeRect,
				m_cursor.width-_offsetLoupeRect*2, 
				m_cursor.height-_offsetLoupeRect*2);
            GUI.DrawTexture(GetComponent<GUITexture>().pixelInset, GetComponent<GUITexture>().texture);
            GUI.DrawTexture(new Rect(m_cur_x, m_cur_y, m_cursor.width, m_cursor.height), m_cursor);
        }
		if(_containsLastPoint)
		{       
			if(rectLoupe==null)
				return;
			if(m_lastPoints.Count>0)
	        {			
				bool hasLine = false;
				Vector2 first = new Vector2();
				Vector2 src = new Vector2();
				Vector2 dst = new Vector2();
				
				foreach(Vector2 lastPoint in m_lastPoints)
				{
					int m_last_x = (int) lastPoint.x;
					int m_last_y = (int) lastPoint.y;
					dst = new Vector2(
						m_last_x+m_prev_point.width/2,
						m_last_y+m_prev_point.height/2);	
					if(rectLoupe.Contains(dst))
					{
						if(_invert==false)
				            GUI.DrawTexture(new Rect(m_last_x, m_last_y, m_prev_point.width, m_prev_point.height), m_prev_point);
						else
							GUI.DrawTexture(new Rect(m_last_x, m_last_y, m_prev_point_sub.width, m_prev_point_sub.height), m_prev_point_sub);
					}
					
					if(hasLine)
					{
						if( rectLoupe.Contains(src) && rectLoupe.Contains(dst))
						{
							Drawer.DrawLineCentered(src, dst, m_lineTex, _lineWidth);
						}
						else
						{	
							if(!rectLoupe.Contains(dst))
							{
								Vector2 PointA = new Vector2(dst.x, dst.y);
								if( (rectLoupe.x) > PointA.x)
								{
									float originalLenghtX = src.x - PointA.x;
									float newLenghtX = rectLoupe.x - PointA.x;
									float ratio = newLenghtX / originalLenghtX;
									PointA.x += ratio*originalLenghtX;
									float originalLenghtY = src.y - PointA.y;
									PointA.y += ratio*originalLenghtY;
								}
								if( (rectLoupe.x+rectLoupe.width) < PointA.x)
								{
									float originalLenghtX = src.x - PointA.x;
									float newLenghtX = (rectLoupe.x+rectLoupe.width) - PointA.x ;
									float ratio = newLenghtX / originalLenghtX;
									PointA.x += ratio*originalLenghtX;
									float originalLenghtY = src.y - PointA.y;
									PointA.y += ratio*originalLenghtY;
								}
								if( (rectLoupe.y) > PointA.y)
								{						
									float originalLenghtY = src.y - PointA.y;
									float newLenghtY = rectLoupe.y - PointA.y;
									float ratio = newLenghtY / originalLenghtY;
									PointA.y += ratio*originalLenghtY;
									float originalLenghtX = src.x - PointA.x;
									PointA.x += ratio*originalLenghtX;
								}
								if( (rectLoupe.y+rectLoupe.height) < PointA.y)
								{							
									float originalLenghtY = src.y - PointA.y;
									float newLenghtY = (rectLoupe.y+rectLoupe.height) - PointA.y;
									float ratio = newLenghtY / originalLenghtY;
									PointA.y += ratio*originalLenghtY;
									float originalLenghtX = src.x - PointA.x;
									PointA.x += ratio*originalLenghtX;	
								}
								if(rectLoupe.Contains(src))
									Drawer.DrawLineCentered(src, PointA, m_lineTex, _lineWidth);
								else
								{
									Vector2 PointB = new Vector2(src.x, src.y);
									if( (rectLoupe.x) > PointB.x)
									{
										float originalLenghtX = PointA.x - PointB.x;
										float newLenghtX = rectLoupe.x - PointB.x;
										float ratio = newLenghtX / originalLenghtX;
										PointB.x += ratio*originalLenghtX;
										float originalLenghtY = PointA.y - PointB.y;
										PointB.y += ratio*originalLenghtY;
									}
									if( (rectLoupe.x+rectLoupe.width) < PointB.x)
									{
										float originalLenghtX = PointA.x - PointB.x;
										float newLenghtX = (rectLoupe.x+rectLoupe.width) - PointB.x ;
										float ratio = newLenghtX / originalLenghtX;
										PointB.x += ratio*originalLenghtX;
										float originalLenghtY = PointA.y - PointB.y;
										PointB.y += ratio*originalLenghtY;
									}
									if( (rectLoupe.y) > PointB.y)
									{						
										float originalLenghtY = PointA.y - PointB.y;
										float newLenghtY = rectLoupe.y - PointB.y;
										float ratio = newLenghtY / originalLenghtY;
										PointB.y += ratio*originalLenghtY;
										float originalLenghtX = PointA.x - PointB.x;
										PointB.x += ratio*originalLenghtX;
									}
									if( (rectLoupe.y+rectLoupe.height) < PointB.y)
									{							
										float originalLenghtY = PointA.y - PointB.y;
										float newLenghtY = (rectLoupe.y+rectLoupe.height) - PointB.y;
										float ratio = newLenghtY / originalLenghtY;
										PointB.y += ratio*originalLenghtY;
										float originalLenghtX = PointA.x - PointB.x;
										PointB.x += ratio*originalLenghtX;	
									}
									Drawer.DrawLineCentered(PointB, PointA, m_lineTex, _lineWidth);
								}
							}
							else if( !rectLoupe.Contains(src))
							{
								Vector2 PointA = new Vector2(src.x, src.y);
								if( (rectLoupe.x) > PointA.x)
								{
									float originalLenghtX = dst.x - PointA.x;
									float newLenghtX = rectLoupe.x - PointA.x;
									float ratio = newLenghtX / originalLenghtX;
									PointA.x += ratio*originalLenghtX;
									float originalLenghtY = dst.y - PointA.y;
									PointA.y += ratio*originalLenghtY;
								}
								if( (rectLoupe.x+rectLoupe.width) < PointA.x)
								{
									float originalLenghtX = dst.x - PointA.x;
									float newLenghtX = (rectLoupe.x+rectLoupe.width) - PointA.x ;
									float ratio = newLenghtX / originalLenghtX;
									PointA.x += ratio*originalLenghtX;
									float originalLenghtY = dst.y - PointA.y;
									PointA.y += ratio*originalLenghtY;
								}
								if( (rectLoupe.y) > PointA.y)
								{						
									float originalLenghtY = dst.y - PointA.y;
									float newLenghtY = rectLoupe.y - PointA.y;
									float ratio = newLenghtY / originalLenghtY;
									PointA.y += ratio*originalLenghtY;
									float originalLenghtX = dst.x - PointA.x;
									PointA.x += ratio*originalLenghtX;
								}
								if( (rectLoupe.y+rectLoupe.height) < PointA.y)
								{							
									float originalLenghtY = dst.y - PointA.y;
									float newLenghtY = (rectLoupe.y+rectLoupe.height) - PointA.y;
									float ratio = newLenghtY / originalLenghtY;
									PointA.y += ratio*originalLenghtY;
									float originalLenghtX = dst.x - PointA.x;
									PointA.x += ratio*originalLenghtX;	
								}
								Drawer.DrawLineCentered(PointA, dst, m_lineTex, _lineWidth);
							}
						}
					}
					else
					{
						first = new Vector2(
							m_last_x+m_prev_point.width/2,
							m_last_y+m_prev_point.height/2);
						
					}
					src = new Vector2(
						m_last_x+m_prev_point.width/2,
						m_last_y+m_prev_point.height/2);
					
					
					hasLine = true;
					
				}
				//dernier point avec curseur
				{
					src = new Vector2(
						m_cur_x+m_cursor.width/2,
						m_cur_y+m_cursor.height/2);
					if( !rectLoupe.Contains(dst))
					{
						if( (rectLoupe.x) > dst.x)
						{
							float originalLenghtX = src.x - dst.x;
							float newLenghtX = rectLoupe.x - dst.x;
							float ratio = newLenghtX / originalLenghtX;
							dst.x += ratio*originalLenghtX;
							float originalLenghtY = src.y - dst.y;
							dst.y += ratio*originalLenghtY;
						}
						if( (rectLoupe.x+rectLoupe.width) < dst.x)
						{
							float originalLenghtX = src.x - dst.x;
							float newLenghtX = (rectLoupe.x+rectLoupe.width) - dst.x ;
							float ratio = newLenghtX / originalLenghtX;
							dst.x += ratio*originalLenghtX;
							float originalLenghtY = src.y - dst.y;
							dst.y += ratio*originalLenghtY;
						}
						if( (rectLoupe.y) > dst.y)
						{						
							float originalLenghtY = src.y - dst.y;
							float newLenghtY = rectLoupe.y - dst.y;
							float ratio = newLenghtY / originalLenghtY;
							dst.y += ratio*originalLenghtY;
							float originalLenghtX = src.x - dst.x;
							dst.x += ratio*originalLenghtX;
						}
						if( (rectLoupe.y+rectLoupe.height) < dst.y)
						{							
							float originalLenghtY = src.y - dst.y;
							float newLenghtY = (rectLoupe.y+rectLoupe.height) - dst.y;
							float ratio = newLenghtY / originalLenghtY;
							dst.y += ratio*originalLenghtY;
							float originalLenghtX = src.x - dst.x;
							dst.x += ratio*originalLenghtX;	
						}
					}
					Drawer.DrawLineCentered(dst, src, m_lineTex,_lineWidth);
				}
				//premier point avec curseur, seulement si plus d'un point déjà tracé, sinon doublon avec le bloc d'avant.
				/*if(m_lastPoints.Count>1)
				{
					if( !rectLoupe.Contains(first))
					{
						if( (rectLoupe.x) > first.x)
						{
							float originalLenghtX = src.x - first.x;
							float newLenghtX = rectLoupe.x - first.x;
							float ratio = newLenghtX / originalLenghtX;
							first.x += ratio*originalLenghtX;
							float originalLenghtY = src.y - first.y;
							first.y += ratio*originalLenghtY;
						}
						if( (rectLoupe.x+rectLoupe.width) < first.x)
						{
							float originalLenghtX = src.x - first.x;
							float newLenghtX = (rectLoupe.x+rectLoupe.width) - first.x ;
							float ratio = newLenghtX / originalLenghtX;
							first.x += ratio*originalLenghtX;
							float originalLenghtY = src.y - first.y;
							first.y += ratio*originalLenghtY;
						}
						if( (rectLoupe.y) > first.y)
						{						
							float originalLenghtY = src.y - first.y;
							float newLenghtY = rectLoupe.y - first.y;
							float ratio = newLenghtY / originalLenghtY;
							first.y += ratio*originalLenghtY;
							float originalLenghtX = src.x - first.x;
							first.x += ratio*originalLenghtX;
						}
						if( (rectLoupe.y+rectLoupe.height) < first.y)
						{							
							float originalLenghtY = src.y - first.y;
							float newLenghtY = (rectLoupe.y+rectLoupe.height) - first.y;
							float ratio = newLenghtY / originalLenghtY;
							first.y += ratio*originalLenghtY;
							float originalLenghtX = src.x - first.x;
							first.x += ratio*originalLenghtX;	
						}				
					}
					Drawer.DrawLineCentered(first, src, m_lineTex,_lineWidth);
				}*/
	        }
		}
    } // OnGUI
    
    //-----------------------------------------------------
    public void SetCurXY(int x, int y)
    {
        m_cur_x = (x == -1)? x : x-m_cursor.width/2;
        m_cur_y = (y == -1)? y : y-m_cursor.height/2;
    } 
	//-----------------------------------------------------
    public void SetLastXY(int x, int y, ArrayList lastPoints)
    {
		m_lastPoints.Clear();
		if(lastPoints!=null)
		{
			foreach(Vector2 point in lastPoints)
			{
	       		int m_last_x = (x-(int)point.x*2)-m_prev_point.width/2;
	        	int m_last_y = (y-(int)point.y*2)-m_prev_point.height/2;
				m_lastPoints.Add(new Vector2(m_last_x, m_last_y));
			}
		}
    }
	/**
	 * Indique si la loupe contient le dernier point
	 **/
	public void ContainsLastPoint(bool containLastPoint)
	{
		_containsLastPoint = containLastPoint;
	}
	
	public void SetInvert(bool invert)
	{
		_invert = invert;
	}
    
} // class LoupeGUI
