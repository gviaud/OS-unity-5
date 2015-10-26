using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pointcube.Global;

public class M2DlineManager : MouseTool
{
    public  GameObject    m_mainCam;
    public  Mode2D        m_mode2D;
    public  GUIStyle      m_cancelStyle;
    public  GUIStyle      m_textStyle;
    public Texture2D      m_lineTex;
    public  Color         m_textOutlineColor;
    private GUIStyle      m_outlineStyle;

    private BgImgManager  m_bgImgMgr;

    // -- Lignes --
    private  List<Line>   m_lines;
    private  bool         m_lineStarted;
    private  Line         m_tmpLine;      // Ligne actuellement éditée
    private  Line         m_selectedLine;
	private  float 		  m_selectLineThresold = 10.0f;		// tolérance pour la sélection des lignes	
	private  bool		  m_justeDeleted = false; // indique qu'un segment vient d'être effacé

    // -- Loupe --
    private bool          m_loupeActive;                    // Loupe active ou non
    private float         m_loupeClickTime;                 // Temps après le dernier clic non relâché (pour activation de la loupe)
    private int           m_loupeClick_x, m_loupeClick_y;   // Position de la souris au dernier clic non relâché
    private const float   m_loupeDelay = 0.1f;              // Délai d'activation de la loupe après un clic non relâché
    private const float   m_loupeThresold = 10.0f;          // Tolérance de différence de position entre le clic et le délai
    private GUITexture    m_loupeGUI;                       // GUITexture de la loupe
    public Texture2D      m_loupeTex;                       // Image de la loupe
    public RenderTexture  m_loupeRenderTex;                 // RenderTexture de la loupe
    private int           m_stickBd;                        // Marge à partir de laquelle l'aimantation vers les pixels extrêmes de l'image s'active
    private LoupeGUI      _loupeGUI;
	private int			  m_aaBkup;

	private static bool 		m_snapActivated=true;

	public static bool SnapActivated {
		get {
			return m_snapActivated;
		}
		set {
			m_snapActivated = value;
		}
	}

	private int		m_snapTrigger=6; //distance max entre le curseur et l'axe pour activer le snap
	
    // -- Ressources affichage --
    private static Texture2D    m_vertexTex     ;// TODO supprimer les fichiers de resources
    private static Texture2D    m_vertexTex_Sub ;// TODO supprimer les fichiers de resources
    private static Texture2D    m_tmpLineTex    ;
    private static Texture2D    m_tmpLineTex_Sub;

    private const int s_lineDrawWidthSml = 7;//2;

    private const string DTAG = "LineManager (Mode2D) : ";
	
	private bool _activated = false;

    #region unity_func
    //-----------------------------------------------------
	void Awake()
    {
        m_bgImgMgr = m_backgroundImg.GetComponent<BgImgManager>();
        if(m_bgImgMgr == null) Debug.LogError(DTAG+"BgImgManager"+PC.MISSING_REF);
        if(m_mode2D   == null) Debug.LogError(DTAG+"Mode2D"      +PC.MISSING_REF);
        if(m_mainCam  == null) Debug.LogError(DTAG+"MainCam"     +PC.MISSING_REF);
        if(m_lineTex  == null) Debug.LogError(DTAG+"Line texture"+PC.MISSING_REF);

        m_vertexTex      = (Texture2D) Resources.Load("tracage/OS_DOT_ADD");
        m_vertexTex_Sub  = (Texture2D) Resources.Load("tracage/OS_DOT_SUB");
        m_tmpLineTex     = (Texture2D) Resources.Load("tracage/OS_TRACE_ADD");
        m_tmpLineTex_Sub = (Texture2D) Resources.Load("tracage/OS_TRACE_SUB");

        GameObject GUITexLoupe = GameObject.Find("Background/loupe/GUITexLoupe");
        _loupeGUI = GUITexLoupe.GetComponent<LoupeGUI>();

        m_ignoreClick      = true;
        m_opengl           = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
                           
	    m_lines            = new List<Line>();
        m_lineStarted      = false;
        m_tmpLine          = null;
        m_selectedLine     = null;

        m_loupeActive      = false;
        m_loupeClickTime   = 0;
        m_loupeTex         = new Texture2D(64, 64, TextureFormat.RGB24, false);
        m_loupeGUI         = GUITexLoupe.GetComponent<GUITexture>();
        m_loupeRenderTex   = null;
        m_loupeGUI.texture = m_loupeTex;
		m_aaBkup		   = QualitySettings.antiAliasing;
			
        m_outlineStyle = new GUIStyle(m_textStyle);
        m_outlineStyle.normal.textColor = m_textOutlineColor;

        this.enabled       = false;
	}
    //-----------------------------------------------------
    void OnDisable()
    {
        m_ignoreClick = true;

        if(m_lineStarted)
        {
            m_tmpLine = null;
            m_lineStarted = false;
        }
    }
	//-----------------------------------------------------
	void Update()
    {
		if(!_activated)
			return;
        base.UpdateMouseTool();
		//Rectification position curseur si snap Activé (magnétisme)
		if(m_tmpLine!=null&&m_snapActivated){
				 Vector2 src = ToPixelCoord(m_tmpLine.src, m_bgImgMgr.GetComponent<GUITexture>().pixelInset);
					int dx=(int)Mathf.Abs(m_mousePosX-src.x);
					int dy=(int)Mathf.Abs(m_mousePosY-src.y);
					if(dx<m_snapTrigger&&dx<dy){
						m_mousePosX=src.x;
					}
					else if(dy<m_snapTrigger&&dy<dx){
						m_mousePosY=src.y;
					}
			}
		
        float xOffset = m_backgroundImg.GetComponent<GUITexture>().pixelInset.x;
        float yOffset = m_backgroundImg.GetComponent<GUITexture>().pixelInset.y;
        float bgImgW  = m_backgroundImg.GetComponent<GUITexture>().pixelInset.width;
        float bgImgH  = m_backgroundImg.GetComponent<GUITexture>().pixelInset.height;
		if(!m_lineStarted)
		{
			if(m_mode2D.IsHelpDisplayed())
				return;
#if !UNITY_ANDROID
			//if(PC.In.Click1Down() && !m_mode2D.IsCursorOnUI())
	        {
				if(m_loupeActive)
					m_mainCam.GetComponent<MainCamManager>().RenderSingle(m_loupeRenderTex); // Donne une texture noire sous iOS	
	        	StartCoroutine(DisplayLoupe(xOffset, yOffset, bgImgW, bgImgH));	
			//	Debug.Log("loupe");
			}
#endif
	       // if(PC.In.Click1Down() && !m_mode2D.IsCursorOnUI())
	        if(PC.In.Click1Up() && !m_mode2D.IsCursorOnUI())
	        {
				if(m_justeDeleted)
				{
					m_justeDeleted = false;
					return;
				}
				if(m_lines.Count > 0)     // Select line
				{
					Line m_clickedLine = GetHoveredLine(m_mousePosX, m_mousePosY);
					if(m_clickedLine != null)
					{
						m_selectedLine = m_clickedLine;
						return;
					}
	                else
					{
						if(m_selectedLine!=null)
						{
	                    	m_selectedLine = null;
							return;
						}
	                    m_selectedLine = null;
					}
				}
				
				m_tmpLine = new Line();
	            m_tmpLine.src = new Vector2((m_mousePosX-xOffset)/bgImgW, (m_mousePosY-yOffset)/bgImgH);
	            m_lineStarted = true;
				//print ("aaaaaaaaaaaaaaaaaaaaaaaaaAudioLowPassFilter");
	            if(m_loupeActive)
	            {
	                m_loupeClickTime = 0;
	                m_loupeActive = false;
	//                m_loupeGUI.enabled = false;
	                _loupeGUI.SetCurXY(-1, -1);
	                _loupeGUI.SetLastXY(-1,-1,null);
#if UNITY_IPHONE 
					QualitySettings.antiAliasing = m_aaBkup;
#endif
				}

	        }
		}
		else
		{
	        Vector2 deltaDrag = Vector2.zero;
	        if(PC.In.Drag1(out deltaDrag) && deltaDrag != Vector2.zero && m_lineStarted)
	        {
	            if(m_selectedLine != null)
	                m_selectedLine = null;
	        }
	
	        if(PC.In.Click1Up() && m_lineStarted)
	        {
	            if(m_ignoreClick)
	                m_ignoreClick = false;
	            else
	            {
	                bool justEndedLine = false;
	            //    if(m_lineStarted && PC.In.Click1Up() )                           // End line
	                {
	                    m_tmpLine.dst = new Vector2((m_mousePosX-xOffset)/bgImgW, (m_mousePosY-yOffset)/bgImgH);
	
	                    if(m_tmpLine.GetPixelLen(m_bgImgMgr.GetComponent<GUITexture>().pixelInset) > 7f)
	                    {
	                        m_lines.Add(m_tmpLine);
	                        m_mode2D.SaveModel();
	                        justEndedLine = true;
	                    }
	                    m_tmpLine = null;
	                    m_lineStarted = false;
	                }
	
	                if(m_lines.Count > 0 && !justEndedLine)     // Select line
	                {
	                    Line m_clickedLine = GetHoveredLine(m_mousePosX, m_mousePosY);
	                    if(m_clickedLine != null)
	                        m_selectedLine = m_clickedLine;
	                    else
	                        m_selectedLine = null;
	                }
	            }
	        } // click 1 up
	
	 //       if(!m_mode2D.HasBgImg())
	 //           return;
        
#if !UNITY_ANDROID
	        if(m_loupeActive)
	            m_mainCam.GetComponent<MainCamManager>().RenderSingle(m_loupeRenderTex); // Donne une texture noire sous iOS	
	        StartCoroutine(DisplayLoupe(xOffset, yOffset, bgImgW, bgImgH));	
		//	Debug.Log("Display Loupe");
	        if(PC.In.Click1Up())   // Au relâchement du clic, masquer la loupe
	        {
	            if(m_ignoreClick)           // Ignorer un relâchement qui correspond à un clic commencé avant l'activation du PolygonTracer
	                m_ignoreClick = false;
	            else if(m_loupeActive)
	            {
	                m_loupeClickTime = 0;
	                m_loupeActive = false;
	//                m_loupeGUI.enabled = false;
	                _loupeGUI.SetCurXY(-1, -1);
	                _loupeGUI.SetLastXY(-1,-1,null);
#if UNITY_IPHONE 
					QualitySettings.antiAliasing = m_aaBkup;
#endif
	            }
	        } // Clic gauche relâché
#endif
		}
	}
    //-----------------------------------------------------
    void OnGUI()
    {
        DrawLines(m_bgImgMgr.GetComponent<GUITexture>().pixelInset);
        Vector2 cursorPos = PC.In.GetCursorPosInvY();

        Rect bgRect = m_bgImgMgr.GetComponent<GUITexture>().pixelInset;
		if(m_tmpLine != null){

				
         DrawTmpLine(m_mousePosX, m_mousePosY, bgRect);
			
		}

        // -- Delete line button --
        if(m_selectedLine != null)
        {
            Vector2 lineCenter = m_selectedLine.GetPixelCenter(bgRect);
            Rect delBtnRect = new Rect(0f, 0f, 32f, 32f);

            Vector2 rectCenter = new Vector2();
            rectCenter.x = Mathf.Clamp(lineCenter.x, bgRect.x+delBtnRect.width/2f, bgRect.xMax-delBtnRect.width/2f);
            rectCenter.y = Mathf.Clamp(lineCenter.y, bgRect.y+delBtnRect.height/2f, bgRect.yMax-delBtnRect.height/2f);

            delBtnRect.center = rectCenter;

            if(GUI.Button(delBtnRect, "", m_cancelStyle))   // Delete line
            {
                DeleteLine(m_selectedLine);
                m_selectedLine = null;
				m_justeDeleted = true;
                m_mode2D.SaveModel();
            }
        }
    }
    //-----------------------------------------------------
    IEnumerator DisplayLoupe(float xOffset, float yOffset, float bgImgW, float bgImgH)
    {
        yield return new WaitForEndOfFrame(); // Wait to avoid the "not inside drawing frame" error

        // -- Loupe --
        if(PC.In.Click1Hold() && (!m_mode2D.IsCursorOnUI() || m_loupeActive))
        {
            if(m_loupeClickTime == 0)            // Initialisation au début du clic
            {
                m_loupeClick_x = (int) m_mousePosX;
                m_loupeClick_y = (int) m_mousePosY;
            }
            if(m_loupeClickTime < m_loupeDelay)  // Si le délai n'est pas atteint
            {
                if(m_mousePosX > (m_loupeClick_x - m_loupeThresold) && m_mousePosX < (m_loupeClick_x + m_loupeThresold) &&
                   m_mousePosY > (m_loupeClick_y - m_loupeThresold) && m_mousePosY < (m_loupeClick_y + m_loupeThresold))
                {                                // Si le curseur n'a pas bougé, incrémenter le décompte "loupe"
                    m_loupeClickTime += Time.deltaTime;
                }
                else                             // Sinon, réinitialiser le décompte
                    m_loupeClickTime = 0;
            }
            else
            {       							 // Si le délai est atteint, activer la loupe
#if UNITY_IPHONE
				if(!m_loupeActive)
				{
					m_aaBkup = QualitySettings.antiAliasing; // l'AA empêche les ReadPixels de fonctionner sur iPad
					QualitySettings.antiAliasing = 0;
				}
#endif
                m_loupeActive = true;
                int imgOffset = 32;              // Distance entre le curseur et le coin le plus proche de l'image loupe
                int imgSize   = 128;             // Côté de l'image loupe (= côté de l'image source * 2)

                // -- Sélection des pixels de l'image à lire => Délocalisé dans MouseTool
                float[] data = base.GetZoneCornerFromCursor(imgSize/2, m_opengl);
                float x = data[0] + xOffset;
                float y = data[1] + yOffset;
                bool decX = (data[2]==1f);
                bool decY = (data[3]==1f);
				
                // Lecture des pixels de la renderTexture active
                Rect polyDrawRect = new Rect(x, y, imgSize/2, imgSize/2);
                m_loupeTex.ReadPixels(polyDrawRect, 0, 0);
                _loupeGUI.ContainsLastPoint(false);
                ArrayList lastXY = new ArrayList();
                ArrayList prevPoints = new ArrayList();
                ArrayList screenPrevPoints = new ArrayList();
                bool findPoint=false;
                if(m_lines.Count>0)
                {
                    bool first = true;
                    foreach(Line polyLine in m_lines)
                    {
                        if(first)
                        {
                            Vector2 srcPoint = polyLine.src;
                            prevPoints.Add(srcPoint);
                            first = false;
                        }
                        Vector2 prevPoint = polyLine.dst;
                        prevPoints.Add(prevPoint);
                        findPoint=true;
                    }
                }
                else if(m_lineStarted)
                {
                    Line polyLine = m_tmpLine;
                    if(polyLine!=null)
                    {
                        Vector2 prevPoint = polyLine.src;
                        prevPoints.Add(prevPoint);
                        findPoint=true;
                    }
                }
                if(findPoint==true)
                {
#if UNITY_IPHONE
                 foreach(Vector2 prevPoint in prevPoints)
                 {
                     Vector2 screenPrevPoint = new Vector2(
                         prevPoint.x*bgImgW+xOffset,
                         Screen.height-(prevPoint.y*bgImgH+yOffset));
                     screenPrevPoints.Add(screenPrevPoint);
                 }
#else
                 foreach(Vector2 prevPoint in prevPoints)
                 {
                     Vector2 screenPrevPoint = new Vector2(
                         prevPoint.x*bgImgW+xOffset,
                         prevPoint.y*bgImgH+yOffset);
                     screenPrevPoints.Add(screenPrevPoint);
                 }
#endif
                 int itPoint=0;
                 _loupeGUI.ContainsLastPoint(false);
                 foreach(Vector2 screenPrevPoint in screenPrevPoints)
                 {
                     if(prevPoints.Count>itPoint)
                     {
                         Vector2 prevPoint = (Vector2) prevPoints[itPoint];
                         _loupeGUI.ContainsLastPoint(true);
                         float lastx = m_mousePosX - screenPrevPoint.x;
                         float lasty = m_mousePosY - (prevPoint.y*bgImgH+yOffset);//screenPrevPoint.y;
                         lastXY.Add(new Vector2(lastx,lasty));
                     //  Debug.Log("Find point numero : "+itPoint);
                     }
                     itPoint++;
                 }
             }

             m_loupeTex.Apply(false);

             // -- Placement de l'image loupe --
             bool invX = false, invY = false;
             Rect pixInset = m_loupeGUI.pixelInset;
             if(decX)                                                      // Si la souris est proche du bord de l'image
             {
                 if(m_mousePosX > Screen.width/2)
                     x = Screen.width-xOffset-imgSize-imgSize/4-imgOffset; // fixe à droite
                 else
                     x = xOffset - imgSize + imgOffset;                    // fixe à gauche
             }
             else
                 x = m_mousePosX - imgSize - imgOffset;                    // Cas général : au dessus du curseur

             if(decY)                                                      // De même pour y
             {
                 if(m_mousePosY > Screen.height/2)
                     y = Screen.height - yOffset - imgSize - imgSize/4 - imgOffset;
                 else
                     y = yOffset + imgSize/4 + imgOffset;
             }
             else                                                          // Cas général : à gauche du curseur
                 y = m_mousePosY - imgOffset - imgSize;

             // Si l'image de la loupe n'a pas la place d'être affichée de ce côté du curseur, changer de côté
             pixInset.x = (invX = (m_mousePosX < imgSize+imgOffset+xOffset))? x+imgSize+imgOffset*2 : x;
             pixInset.y = ((invY = (m_mousePosY < imgSize+imgOffset+yOffset)) && !decY)? y+imgSize+imgOffset*2 : y;

             m_loupeGUI.pixelInset = pixInset;

             // -- Position du curseur sur l'image de la loupe --
             x = (invX)? (m_mousePosX + imgOffset + pixInset.width/2) : (m_mousePosX - imgSize - imgOffset + pixInset.width/2);
             if(decX) // Si l'image est fixe proche du bord de l'écran
             {
                 if(m_mousePosX > Screen.width - m_stickBd)
                     x = Screen.width - 1 - imgSize/4 - imgOffset;   // Aimantation sur le dernier pixel de l'image quand on est
                 else if(m_mousePosX < m_stickBd)                    // proche du bord de l'écran (pour Ipad)
                     x = 1 + imgSize/4 + imgOffset;
                 else if(m_mousePosX > Screen.width/2)               // Cas général : déplacement du curseur en fonction du curseur de la souris
                     x += m_mousePosX - (Screen.width - xOffset - imgSize/4);
                 else                                                // de même pour la partie gauche de l'écran
                     x -= xOffset + imgSize/4 - m_mousePosX;
             } // Si loupe fixe en X

             y = (invY)? ((m_mousePosY + imgSize + imgOffset - pixInset.height/2)) : (m_mousePosY - imgOffset - pixInset.height/2);
             if(decY)    // De même pour Y
             {
                 if(m_mousePosY > Screen.height-m_stickBd)
                     y = Screen.height - 1 - imgSize/4 - imgOffset;
                 else if(m_mousePosY < m_stickBd)
                     y = 1 + imgSize/4 + imgOffset;
                 else if(m_mousePosY > Screen.height/2)
                     y += m_mousePosY - (Screen.height - yOffset - imgSize/4);
                 else
                     y -= (yOffset + imgSize/4) - m_mousePosY;
             } // Si loupe fixe en Y
               _loupeGUI.SetCurXY((int)x, (int)y);

                // Nettoyer
//                m_loupeRenderTex.Release(); // bug sur Ipad à cause d'une bibliothèque liée à OpenGL

            } // Loupe activée
        }

        // Désactivation de la loupe dans Update();
		
        yield return true;
    } // Display loupe (coroutine)
    #endregion

	public bool getIsLineStarted()
	{
		return m_lineStarted;
	}
	
	public void removeStartedLine()
	{
		m_tmpLine = null;
		m_lineStarted = false;
	}
	
    #region lines_mechanics
    //-----------------------------------------------------
    public void DeleteAllLines()
    {
        m_lines = new List<Line>();
    }

    public void DeleteLine(Line lineToDel)
    {
        m_lines.Remove(lineToDel);
        lineToDel = null;
    }
    //-----------------------------------------------------
    public Line[] GetLines()
    {
        return m_lines.ToArray();
    }
	
	public void SetActivated(bool activated)
	{
		_activated = activated;
	}
    #endregion


    #region line_utils
    //-----------------------------------------------------
    private void DrawLines(Rect bgRect)
    {
    	//bgRect.y *= -1;
        if(m_lines.Count > 0)
		{print ("hallah   " + m_lines.Count);
            foreach(Line l in m_lines)
            {
                Vector2 src = ToPixelCoord(l.src, bgRect);
                Vector2 dst = ToPixelCoord(l.dst, bgRect);
                Texture2D lineTex = m_lineTex;
                if(l == m_selectedLine)
                    lineTex = m_tmpLineTex_Sub;
                Drawer.DrawLineCentered(src, dst, lineTex, s_lineDrawWidthSml);
                if(l != m_selectedLine)
                    DrawLineLength(l, bgRect);
            }
        }
    }
    //-----------------------------------------------------
    private void DrawTmpLine(float cursorX, float cursorY, Rect bgRect)
    {
        if(m_tmpLine != null)
        {
            Vector2 src = ToPixelCoord(m_tmpLine.src, bgRect);
            Vector2 dst = new Vector2(cursorX, cursorY);
			Drawer.DrawIcon(src,m_vertexTex);
            Drawer.DrawLineCentered(src, dst, m_tmpLineTex, s_lineDrawWidthSml);
            dst.x = (dst.x - bgRect.x)/bgRect.width;
            dst.y = (dst.y - bgRect.y)/bgRect.height;
            m_tmpLine.dst = dst;
            if(m_tmpLine.GetPixelLen(bgRect) >= 3)
                DrawLineLength(m_tmpLine, bgRect);
        }
    }
    //-----------------------------------------------------
    private void DrawLineLength(Line line, Rect bgRect)
    {
        Vector2 lineCenter = line.GetPixelCenter(bgRect);

        float linePxLen = line.GetPixelLen(bgRect);

        string labelText = (linePxLen * m_mode2D.GetMetersPerPixels()).ToString("F1")+TextManager.GetText("Mode2D.LineLenUnit");
        float rectWidth = m_textStyle.CalcSize(new GUIContent(labelText)).x;
        Rect txtBtnRect = new Rect(0f, 0f, rectWidth, 32f);
        Vector2 rectCenter = new Vector2();
        rectCenter.x = Mathf.Clamp(lineCenter.x, bgRect.x+txtBtnRect.width/2f, bgRect.xMax-txtBtnRect.width/2f);
        rectCenter.y = Mathf.Clamp(lineCenter.y, bgRect.y+txtBtnRect.height/2f, bgRect.yMax-txtBtnRect.height/2f);
        txtBtnRect.center = rectCenter;

        Rect txtBtnRectL = txtBtnRect;
        Rect txtBtnRectR = txtBtnRect;
        Rect txtBtnRectT = txtBtnRect;
        Rect txtBtnRectB = txtBtnRect;
        txtBtnRectL.x -= 1f;
        txtBtnRectR.x += 1f;
        txtBtnRectB.y -= 1f;
        txtBtnRectT.y += 1f;

        GUI.Label(txtBtnRectL, labelText, m_outlineStyle);   // Draw 5 times the text to create a 1px outline
        GUI.Label(txtBtnRectR, labelText, m_outlineStyle);
        GUI.Label(txtBtnRectB, labelText, m_outlineStyle);
        GUI.Label(txtBtnRectT, labelText, m_outlineStyle);
        GUI.Label(txtBtnRect,  labelText, m_textStyle);
    }
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
    //-----------------------------------------------------
    private Line GetHoveredLine(float mouseX, float mouseY)
    {
        Vector2 mouse      = new Vector2(mouseX, mouseY);
        float   dist       = float.MaxValue;
        int     i          = 0;
        int     chosenLine = -1;
        foreach(Line curLine in m_lines)
        {
            Vector2 src = ToPixelCoord(curLine.src, m_bgImgMgr.GetComponent<GUITexture>().pixelInset);
            Vector2 dst = ToPixelCoord(curLine.dst, m_bgImgMgr.GetComponent<GUITexture>().pixelInset);
            float distSum  = Vector2.Distance(mouse, src) + Vector2.Distance(mouse, dst);
            float distDiff = distSum - Vector2.Distance(src, dst);
            if(distDiff < m_selectLineThresold && distDiff < dist)   // If the sum of the cursor adjacent sides of the triangle is near
            {                                       // the length of the line, the line is considered hovered
                dist = distDiff;
                chosenLine = i;
            }
            i++;
        }
        if(chosenLine != -1)
            return m_lines[chosenLine];
        else
            return null;
    }
    #endregion

    #region save_load
    //-----------------------------------------------------
    public float[] GetSaveData()
    {
        List<float> output = new List<float>();
        output.Add(m_lines.Count*4f);
        foreach(Line line in m_lines)
        {
            output.Add(line.src.x);
            output.Add(line.src.y);
            output.Add(line.dst.x);
            output.Add(line.dst.y);
        }
        return output.ToArray();
    }
    //-----------------------------------------------------
    public void LoadFromData(float[] data)
    {
		m_lines.Clear();
        if(data == null)
            return;
        
        int i=0;
        Line tmpLine = null;
		int count = 0;
		if(data.Length>0)
			count = (int) data[0];
        //foreach(float f in data)
        for(int j=1;j<count+1;j++)
        {
			float f = data[j];
            switch(i)
            {
                case 0 :
                    tmpLine = new Line();
                    tmpLine.src = new Vector2(f, 0f);
                    break;

                case 1 :
                    tmpLine.src.y = f;
                    break;

                case 2 :
                    tmpLine.dst = new Vector2(f, 0f);
                    break;

                case 3 :
                    tmpLine.dst.y = f;
                    m_lines.Add(tmpLine);
                    break;

                default : break;
            }
            i = (i+1)%4;
        } // foreach float
    }
    #endregion
} // class M2DlineManager
