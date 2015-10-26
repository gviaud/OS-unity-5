using UnityEngine;
using System.Collections.Generic;

public class Ev2_Polygon : IEv2_Zone
{
#region attributs
    // -- Polygone --
    private  List<Line>         m_lines;
    private  Line               m_tmpLine;      // Ligne actuellement éditée
    private  bool               m_lineStarted;
    private  bool               m_closed;
    private  Color              m_mainColor;
	private bool 				_invert;

    // -- Ressources affichage --
    private static Texture2D    m_vertexTex			= (Texture2D) Resources.Load("tracage/OS_DOT_ADD");  // TODO supprimer les fichiers de resources
    private static Texture2D    m_vertexTex_Sub		= (Texture2D) Resources.Load("tracage/OS_DOT_SUB");  // TODO supprimer les fichiers de resources
    private static Texture2D    m_lineTex    		= (Texture2D) Resources.Load("tracage/OS_TRACE_WHITE");
    private static Texture2D    m_tmpLineTex 		= (Texture2D) Resources.Load("tracage/OS_TRACE_ADD");
    private static Texture2D    m_tmpLineTex_Sub	= (Texture2D) Resources.Load("tracage/OS_TRACE_SUB");

    // -- static --
    private static readonly int s_lineDrawWidth    = 7;//5;
    private static readonly int s_lineDrawWidthSml = 7;//2;
//    private static readonly int s_addPointThres    = 10; // nb min de pixels autorisé entre 2 points

    private static readonly string DEBUGTAG = "Ev2_Polygon : ";
#endregion

    //-----------------------------------------------------
    public Ev2_Polygon()
    {
        m_lines       	= new List<Line>();
        m_tmpLine     	= null;
        m_lineStarted	= false;
        m_closed      	= false;
		_invert			= false;

        m_mainColor   = Color.clear;
    }

    //-----------------------------------------------------
    public Ev2_Polygon(Ev2_Polygon model)
    {
        m_lines = new List<Line>();
        Line[] lines = model.GetLines();
        for(int i=0; i<lines.Length; i++)
            m_lines.Add(lines[i]);

        m_tmpLine     = null;
        m_lineStarted = true;
        m_closed      = true;

        m_mainColor   = model.GetMainColor();
    }

    //-----------------------------------------------------
    public void AddPoint(Vector2 newPoint)
    {
        if(!m_lineStarted)
        {
            m_tmpLine = new Line();
            m_tmpLine.src = newPoint;
            m_lineStarted = true;
        }
        else
        {
            m_tmpLine.setDst(newPoint, s_lineDrawWidth);

            Vector2 pxSrc = ToPixelCoord(m_tmpLine.src);
            Vector2 pxDst = ToPixelCoord(m_tmpLine.dst);
            float pixelLen = (pxDst - pxSrc).magnitude;

            if(pixelLen > 3/*10*/)
            {
                m_lines.Add(m_tmpLine);
                m_tmpLine = new Line();
                m_tmpLine.src = newPoint;
                m_lineStarted  = true;
            }
        }
    } // AddPoint()

    //-----------------------------------------------------
    public void Close(bool noNewLine = false)
    {
        if(!noNewLine)
        {
            m_tmpLine.setDst(new Vector2(m_lines[0].src.x, m_lines[0].src.y), s_lineDrawWidth);
            m_lines.Add(m_tmpLine);
        }
        m_lineStarted = false;
        m_closed = true;   // polygone verouillé
    }

    //-----------------------------------------------------
    public void Cancel()
    {
        m_lines.Clear();
        m_tmpLine = null;
        m_lineStarted = false;
        m_closed = false;
    }

#region draw

    //-----------------------------------------------------
    public void Draw(float cursorX, float cursorY, bool drawTmpLine)
    {
        Rect screen = new Rect(0f, 0f, (float)Screen.width, (float) Screen.height);
        Draw(cursorX, cursorY, drawTmpLine, screen);
    }
    //-----------------------------------------------------
    public void Draw(float cursorX, float cursorY, bool drawTmpLine, Rect bgImg)
    {
        if(m_lineStarted)
        {
            DrawLines(bgImg);
            if(drawTmpLine) DrawTmpLine(cursorX, cursorY, bgImg);
            DrawVertices(bgImg);
        }
    } // Draw()

    //-----------------------------------------------------
    private void DrawLines(Rect bgImg)
    {
        if(m_lines.Count > 0)
        {
            foreach(Line l in m_lines)
            {
                Vector2 src = ToPixelCoord(l.src, bgImg);
                Vector2 dst = ToPixelCoord(l.dst, bgImg);

                Drawer.DrawLineCentered(src, dst, m_lineTex, l.width);
            } // foreach line
        }
    } // DrawLines()
    
    //-----------------------------------------------------
    private void DrawTmpLine(float cursorX, float cursorY, Rect bgImg)
    {
        if(m_tmpLine != null)
        {
            Vector2 src = ToPixelCoord(m_tmpLine.src, bgImg);
            Vector2 dst = new Vector2(cursorX, cursorY);
			if(_invert==false)
				Drawer.DrawLineCentered(src, dst, m_tmpLineTex, s_lineDrawWidthSml);
			else
				Drawer.DrawLineCentered(src, dst, m_tmpLineTex_Sub, s_lineDrawWidthSml);
            dst.x = (dst.x - bgImg.x)/bgImg.width;
            dst.y = (dst.y - bgImg.y)/bgImg.height;
            m_tmpLine.dst = dst;
        }
    } // DrawTmpLine()
    
    //-----------------------------------------------------
    private void DrawVertices(Rect bgImg)
    {
        if(m_lines.Count == 0 && m_lineStarted)
        {
            Vector2 src = ToPixelCoord(m_tmpLine.src, bgImg);
			if(_invert==false)
          	  Drawer.DrawIcon(src, m_vertexTex, m_vertexTex.width);///2);
			else
          	  Drawer.DrawIcon(src, m_vertexTex_Sub, m_vertexTex_Sub.width);///2);
        }
        else
        {
            for(int i=0; i<m_lines.Count; i++)
            {
                Line l = m_lines[i];
                if(i==0)
                {
                    Vector2 src = ToPixelCoord(l.src, bgImg);
					if(_invert==false)
		          	  Drawer.DrawIcon(src, m_vertexTex, m_vertexTex.width);///2);
					else
		          	  Drawer.DrawIcon(src, m_vertexTex_Sub, m_vertexTex_Sub.width);///2);
                }
                Vector2 dst = ToPixelCoord(l.dst, bgImg);
				if(_invert==false)
					Drawer.DrawIcon(dst, m_vertexTex, m_vertexTex.width);///2);
				else
					Drawer.DrawIcon(dst, m_vertexTex_Sub, m_vertexTex_Sub.width);///2);
            }
        }
    } // DrawVertices()

    //-----------------------------------------------------
    private Vector4 GetBounds(Rect bgRect)
    {
//        Debug.Log ("CREATING BOUNDS, rect = "+bgRect.ToString());
        float maxY_scr = 0, maxX_scr = 0, minY_scr = 1, minX_scr = 1;
        float curY, curX;
        for(int i=0; i<m_lines.Count; i++)
        {
            curY = m_lines[i].dst.y;
            curX = m_lines[i].dst.x;
            if(curY < minY_scr) minY_scr = curY;
            if(curY > maxY_scr) maxY_scr = curY;
            if(curX < minX_scr) minX_scr = curX;
            if(curX > maxX_scr) maxX_scr = curX;
        }

        // -- Butées --
        if(minX_scr < 0f) minX_scr = 0f;
        if(minY_scr < 0f) minY_scr = 0f;
        if(maxX_scr > 1f) maxX_scr = 1f;
        if(maxY_scr > 1f) maxY_scr = 1f;
//        Debug.Log ("BOUNDS = "+minX_scr+", "+maxX_scr+", "+minY_scr+", "+maxY_scr);

        // -- Calcul rectangle englobant --
        Vector4 bounds = new Vector4();
        bounds.x = (maxX_scr * bgRect.width);        // maxX
        bounds.y = (minX_scr * bgRect.width);        // minX
        bounds.z = ((1 - minY_scr) * bgRect.height); // maxY
        bounds.w = ((1 - maxY_scr) * bgRect.height); // minY

//        Debug.Log ("BOUNDS = "+bounds.ToString());
        return bounds;
    }

    //-----------------------------------------------------
    public List<MaskPix> Rasterize(Rect bgRect)
    {
        Vector4 bounds = GetBounds(bgRect);

        // -- Remplissage --
        return FillPolygon2(m_mainColor, (int)bgRect.width, (int)bgRect.height, bounds);
    } // Rasterize()

    //-----------------------------------------------------
    public GrassV2zone RasterizeRGB(Rect bgRect)
    {
//        Debug.Log(DEBUGTAG+" Rasterizing RGB "+bgRect.ToString());
        Vector4 bounds = GetBounds(bgRect);

//        Debug.Log(DEBUGTAG+" bounds = "+bounds.ToString());
        // -- Remplissage --
        List<MaskPixRGB> pix = FillPolygon2_RGB(m_mainColor, (int)bgRect.width,
                                                                        (int)bgRect.height, bounds);
//        Debug.Log (DEBUGTAG+" ... "+pix.Count+" pixels in polygon !");
        return new GrassV2zone(pix, bounds.y, bounds.x, bounds.w, bounds.z);

    } // Rasterize()

    //-----------------------------------------------------
//    private void FillPolygon1(int maxX, int maxY, int minX, int minY, List<MaskPix> mask)
//    {
//        int x_scr, y_scr, j, x, y, i;
//        bool insidePoly;
//        float alpha = (m_inverser? 0 : 1);
//        float xOffset = m_backgroundImg.guiTexture.pixelInset.x;
//        float yOffset = m_backgroundImg.guiTexture.pixelInset.y;
//
//        for(x=minX; x<maxX ; x++)       // Remplissage du polygone
//        {
//        //                Debug.Log("X = "+x);
//           for(y=minY; y<maxY ; y++)
//           {
//        //                    Debug.Log("Y = "+y);
//               j = lines.Count - 1;
//               insidePoly = false;
//               for (i = 0; i < lines.Count; i++)
//               {
//                    x_scr = x + (int)xOffset;
//                    y_scr = Screen.height - y - (int)yOffset;
//                    if (((Line)lines[i]).dst.y < y_scr && ((Line)lines[j]).dst.y >= y_scr ||
//                        ((Line)lines[j]).dst.y < y_scr && ((Line)lines[i]).dst.y >= y_scr)
//                    {
//                        if ((((Line)lines[i]).dst.x+ (y_scr - ((Line)lines[i]).dst.y)/(((Line)lines[j]).dst.y
//                                    - ((Line)lines[i]).dst.y)*(((Line)lines[j]).dst.x - ((Line)lines[i]).dst.x)) < x_scr)
//                        {
//                            insidePoly = !insidePoly;
//                        }
//                    }
//                    j = i;
//                } // pour chaque ligne du polygone
//                if(insidePoly)      // construction du masque
//                {
//                    if(m_tool == GUIEditTools.EditTool.Eraser || m_tool == GUIEditTools.EditTool.Inpaint)
//                        mask.Add(new MaskPix(x, y, alpha));
//                    if(m_tool == GUIEditTools.EditTool.Grass)
//                    {
//        //                            Debug.Log("pixel rgb");
//                        mask.Add(new MaskPixRGB(x, y, alpha));
//                    }
//                }
//           } // for y
//        } // for x
//    }

    //-----------------------------------------------------
    // Méthode de remplissage de polygone optimisé tirée de alienryderflex.com/polygon_fill/
    private List<MaskPix> FillPolygon2(Color mainCol, int scr_w, int scr_h, Vector4 bounds)
    {
//        float xOffset = m_backgroundImg.guiTexture.pixelInset.x;
//        float yOffset = m_backgroundImg.guiTexture.pixelInset.y;

        int maxX = (int)bounds.x;
        int minX = (int)bounds.y;
        int maxY = (int)bounds.z;
        int minY = (int)bounds.w;

        // -- Pré-calculer les coordonnées des lignes en pixels --
        for(int i=0; i<m_lines.Count; i++)
            m_lines[i].PreComputeScreenCoords(scr_w, scr_h/*, xOffset, yOffset*/);

        // -- Remplissage --
        List<MaskPix> output = new List<MaskPix>();
        float alpha = mainCol.a;

        int y_scr;
        List<int> intersect = new List<int>();
        float lineY1, lineY2, lineX1, lineX2;
//        int maxX_img = maxX * m_wGUI / width - m_xoffset;
//        int minX_img = minX * m_wGUI / width - m_xoffset;
//        float alpha = (m_inverser ? 0 : 1);
        
        // Remplissage de polygone optimisé, cf. alienryderflex.com/polygon_fill/
        for(int y=minY; y<maxY; y++) // pour chaque ligne
        {
            y_scr = scr_h - y;// - (int)yOffset; // Enlevé Offset
            intersect.Clear();
    //                Debug.Log("Droite : f(x)="+y);
    //                y_img = Screen.height - y * m_hGUI /height - m_yoffset;
            for(int i=0; i<m_lines.Count; i++) // pour chaque point du polygone
            {
                lineY1 = m_lines[i].GetScrSrc().y;
                lineY2 = m_lines[i].GetScrDst().y;
    //                    Debug.Log("Ligne poly : ("+lineX1+","+lineY1+")-("+lineX2+","+lineY2+")");
                if(lineY1 < (double) y_scr && lineY2 >= (double) y_scr ||
                   lineY2 < (double) y_scr && lineY1 >= (double) y_scr)
                {
                    lineX1 = m_lines[i].GetScrSrc().x; //* m_wGUI / width - m_xoffset;
                    lineX2 = m_lines[i].GetScrDst().x; //* m_wGUI / width - m_xoffset;
                    intersect.Add((int) (lineX1+(y_scr - lineY1)/(lineY2 - lineY1) * (lineX2 - lineX1))); // Ajouter le point d'intersection à la liste
    //                        Debug.Log("Y droite ("+y+") est compris entre "+lineY1+" et "+ lineY2+"... calcul X intersection : "+(int) (lineX1+(y - lineY1)/(lineY2 - lineY1) * (lineX2 - lineX1)));
                }
            }
    //                Debug.Log("tri des points trouves...");
            // Tri à bulles sur les points d'intersection trouvés -> ordre croissant
            int ii = 0, swap;
            while(ii<intersect.Count-1)
            {
                if(intersect[ii] > intersect[ii+1])
                {
                    swap = intersect[ii];
                    intersect[ii] = intersect[ii+1];
                    intersect[ii+1] = swap;
                    if(ii != 0) ii--;
                }
                else ii++;
            }

    //                Debug.Log("points d'inter tries ! ");
    //                for(ii=0 ; ii<intersect.Count ; ii++)
    //                    Debug.Log("."+intersect[ii]);
    //                Debug.Log("Remplissage des zones paires");
            // Remplissage des zones "paires" des lignes
         
//         int off7X = Mathf.CeilToInt(xOffset); // Enlevé Offset
            for(int i=0; i<intersect.Count; i=i+2)
            {
//                Debug.Log("i = "+i);
                if(intersect[i  ] >= maxX)//+off7X) // Enlevé Offset
                {
    //                        Debug.Log(intersect[i]+" >= " + (maxX+1) +" donc break");
                    break;
                }
                if(intersect[i+1] >  minX)//+off7X)  // Enlevé Offset
                {
                 //        Debug.Log(intersect[i+1] +" > "+(minX-1)+" donc remplissage...");
                    if(intersect[i  ] < minX)//+off7X) // Enlevé Offset
                    {
    //                            Debug.Log("recalage "+intersect[i]+" a "+(minX-1));
                        intersect[i]   = minX;//+off7X; // Enlevé Offset
                    }
                    if(intersect[i+1] > maxX)//+off7X) // Enlevé Offset
                    {
    //                            Debug.Log("recalage "+intersect[i+1]+" a "+(maxX+1));
                        intersect[i+1] = maxX;//+off7X; // Enlevé Offset
                    }
                    for(int x= (int)intersect[i]; x<intersect[i+1]; x++)
                    {
                        output.Add(new MaskPix(x, y, alpha));
                    } // Pour chaque colonne
                } // Si le x du point d'intersection impair est à droite du bord gauche de la zone
            }
        } // pour chaque ligne

        return output;
    } // FillPolygon2()

    //-----------------------------------------------------
    private List<MaskPixRGB> FillPolygon2_RGB(Color mainCol, int scr_w, int scr_h, Vector4 bounds)
    {
        int maxX = (int)bounds.x;
        int minX = (int)bounds.y;
        int maxY = (int)bounds.z;
        int minY = (int)bounds.w;

        // -- Pré-calculer les coordonnées des lignes en pixels --
        for(int i=0; i<m_lines.Count; i++)
            m_lines[i].PreComputeScreenCoords(scr_w, scr_h/*, xOffset, yOffset*/);

        // -- Remplissage --
        List<MaskPixRGB> output = new List<MaskPixRGB>();

        int y_scr;
        List<int> intersect = new List<int>();
        float lineY1, lineY2, lineX1, lineX2;
        
        // Remplissage de polygone optimisé, cf. alienryderflex.com/polygon_fill/
        for(int y=minY; y<maxY; y++) // pour chaque ligne
        {
            y_scr = scr_h - y;
            intersect.Clear();
            for(int i=0; i<m_lines.Count; i++) // pour chaque point du polygone
            {
                lineY1 = m_lines[i].GetScrSrc().y;
                lineY2 = m_lines[i].GetScrDst().y;
                if(lineY1 < (double) y_scr && lineY2 >= (double) y_scr ||
                   lineY2 < (double) y_scr && lineY1 >= (double) y_scr)
                {
                    lineX1 = m_lines[i].GetScrSrc().x;
                    lineX2 = m_lines[i].GetScrDst().x;
                    intersect.Add((int) (lineX1+(y_scr - lineY1)/(lineY2 - lineY1) * (lineX2 - lineX1))); // Ajouter le point d'intersection à la liste
                }
            }
            // Tri à bulles sur les points d'intersection trouvés -> ordre croissant
            int ii = 0, swap;
            while(ii<intersect.Count-1)
            {
                if(intersect[ii] > intersect[ii+1])
                {
                    swap = intersect[ii];
                    intersect[ii] = intersect[ii+1];
                    intersect[ii+1] = swap;
                    if(ii != 0) ii--;
                }
                else ii++;
            }
            for(int i=0; i<intersect.Count; i=i+2)
            {
                if(intersect[i  ] >= maxX) break;
                if(intersect[i+1] >  minX)
                {
                    if(intersect[i  ] < minX) intersect[i]   = minX;
                    if(intersect[i+1] > maxX) intersect[i+1] = maxX;
                    for(int x= (int)intersect[i]; x<intersect[i+1]; x++)
                    {
                        output.Add(new MaskPixRGB(x, y, mainCol));
//                        Debug.Log (output[output.Count-1].GetColor().ToString());
                    } // Pour chaque colonne
                } // Si le x du point d'intersection impair est à droite du bord gauche de la zone
            }
        } // pour chaque ligne
        return output;
    } // FillPolygon2_RGB()

    //-----------------------------------------------------
    private Vector2 ToPixelCoord(Vector2 v)
    {
        Vector2 output = new Vector2(v.x, v.y);
        output.x *= Screen.width;
        output.y *= Screen.height;
        return output;
    }

    //-----------------------------------------------------
    private Vector2 ToPixelCoord(Vector2 v, Rect bgImg)
    {
        Vector2 output = new Vector2(v.x, v.y);
        output.x = (output.x * bgImg.width) + bgImg.x;
        output.y = (output.y * bgImg.height) + bgImg.y;
        return output;
    }
#endregion

#region save_load
    //-----------------------------------------------------
    public float[] GetSaveData()
    {
        float[] output = new float[(m_lines.Count*4)+3];

        int j = 0;
        output[j++] = (m_lines.Count*4) + 3;      // Taille
        output[j++] = m_mainColor.a;              // Couleur
        for(int i=0; i<m_lines.Count; i++)
        {
            output[j++] = m_lines[i].GetSrc().x;
            output[j++] = m_lines[i].GetSrc().y;
            output[j++] = m_lines[i].GetDst().x;
            output[j++] = m_lines[i].GetDst().y;
        }
        output[j++] = 0f;                         // Type (0 = Ev2_Polygon)
        return output;
    }

    //-----------------------------------------------------
    public void LoadFromData(float[] data)
    {
        if(data[0] != data.Length || data[data.Length-1] != 0f)
            Debug.LogError(DEBUGTAG + "Invalid data to load");

        m_mainColor = new Color(0f, 0f, 0f, data[1]);
        for(int i=2; i<data.Length-1; i+=2)
            AddPoint(new Vector2(data[i], data[i+1]));
        Close(true);
    }

#endregion

#region get_set
    //-----------------------------------------------------
    public int GetLineCount()
    {
        return m_lines.Count;
    }

    //-----------------------------------------------------
    public bool IsClosed()
    {
        return m_closed;
    }

    //-----------------------------------------------------
    public bool IsStarted()
    {
        return m_lineStarted;
    }
	
	public Line getLineTemp()
	{
		return m_tmpLine;
	}

    //-----------------------------------------------------
    public Vector2 GetOrigin()
    {
        Vector2 output = Vector2.zero;
        if(m_lines.Count <= 0)
            Debug.LogError(DEBUGTAG+"Trying to get unexisting origin : polygon has no lines yet !");
        else
            output = m_lines[0].GetSrc();

        return output;
    }

    //-----------------------------------------------------
    public bool IsOnOrigin(float pointX, float pointY, Rect bgImg, float thresold)
    {
        Vector2 origin = GetOrigin();

        return (pointX >= (origin.x*bgImg.width) + bgImg.x - thresold &&
                pointX <= (origin.x*bgImg.width) + bgImg.x + thresold &&
                pointY >= (origin.y*bgImg.height) + bgImg.y - thresold &&
                pointY <= (origin.y*bgImg.height) + bgImg.y + thresold);
    }

    //-----------------------------------------------------
    public Line[] GetLines()
    {
        Line[] output = null;
        if(m_lines.Count <= 0)
            Debug.LogError(DEBUGTAG+"Trying to get unexisting lines : polygon has no lines yet !");
        else
            output = m_lines.ToArray();

        return output;
    }

    //-----------------------------------------------------
    public void SetTmpLineEnd(float x, float y)
    {
        m_tmpLine.dst.Set(x, y);
    }

    //-----------------------------------------------------
    public void SetMainColor(Color newCol)
    {
        m_mainColor = newCol;
    }

    //-----------------------------------------------------
    public Color GetMainColor()
    {
        return m_mainColor;
    }

    //-----------------------------------------------------
    public IEv2_Zone Clone()
    {
        return (IEv2_Zone) new Ev2_Polygon(this);
    }
	
	public void SetInvert(bool invert)
	{
		_invert = invert;
	}
#endregion

} // class Polygon
