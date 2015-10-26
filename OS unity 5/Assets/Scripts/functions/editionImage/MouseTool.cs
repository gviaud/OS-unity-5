//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/MouseTool.cs - 05/2012 - KS
// Classe mère pour tous les outils qui demandent une interaction avec la souris

using UnityEngine;
using System.Collections;
using Pointcube.Global;

public class MouseTool : MonoBehaviour
{
    public GameObject    m_backgroundImg;

    protected float      m_mousePosX;
    protected float      m_mousePosY;

    protected bool       m_ignoreClick;        // Eviter de prendre en compte le clic qui a servi à activer l'outil

    protected bool       m_opengl;             // OpenGL ou DirectX (textures gérées différemment sur Y)

    private static readonly string DEBUGTAG = "MouseTool : ";
    private static readonly bool   DEBUG    = false;
    //-----------------------------------------------------
	protected void StartMouseTool()         // A appeler par Start() des classes filles
    {
        if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log(DEBUGTAG+"StartMouseTool");

        m_mousePosX   = 0;
        m_mousePosY   = 0;

        m_ignoreClick = true;

        m_opengl      = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
    }

    //-----------------------------------------------------
	protected void UpdateMouseTool()        // A appeler par Update() des classes filles
    {
        m_mousePosX = PC.In.GetCursorPosInvY().x;
        m_mousePosY = PC.In.GetCursorPosInvY().y;

        if(PC.In.Click1Down())  // Ne pas ignorer de relâchement de clic postérieur à cet "enfoncement" de clic
            m_ignoreClick = false;
    }
    
    //-----------------------------------------------------
    void OnGUI()
    {
    }

    //-----------------------------------------------------
    // Retourne les coordonées du top-left(TODO CHECK) pixel du
    // carré de côté imgSize et centré sur le curseur de la souris,
    // avec gestion des bordures de l'image de fond.
    // Le tableau de sortie contient 4 float :
    // {x, y, isXNearToImageBorder (1/0), isYNearToImageBorder (1/0)}
    protected float[] GetZoneCornerFromCursor(int imgSize, bool invY = true)
    {
        bool decX = false, decY = false;
        float x, y;
        float xOffset = m_backgroundImg.GetComponent<GUITexture>().pixelInset.x;
        float yOffset = m_backgroundImg.GetComponent<GUITexture>().pixelInset.y;
        float mouseY  = (invY ? m_mousePosY : Screen.height - m_mousePosY);

        if(decX = (m_mousePosX >= Screen.width-xOffset-imgSize/2))  // Si le curseur est proche du bord de l'image,
            x = Screen.width-xOffset*2-imgSize;                       //  ne pas lire les pixels hors de l'image
        else if(decX = (m_mousePosX <= xOffset + imgSize/2))
            x = 0;
        else                                                          // Sinon lire simplement le carré de côté imgSize/2
            x = m_mousePosX-imgSize/2-xOffset;                                // centré sur le curseur

        if(decY = (mouseY >= Screen.height-yOffset-imgSize/2)) // De même pour l'axe des Y
        {
           if(m_opengl) y = 0;   // OpenGL et D3D ont des façons différentes de stocker les images en Y
           else         y = Screen.height-yOffset*2-imgSize;
        }
        else if(decY = (mouseY < yOffset+imgSize/2))
        {
           if(m_opengl) y = Screen.height-yOffset*2-imgSize;
           else         y = 0;
        }
        else
        {
           if(m_opengl) y = Screen.height-mouseY-yOffset-imgSize/2;
           else         y = mouseY-yOffset-imgSize/2;
        }

        return new float[] {x, y, (decX)? 1f : 0f, (decY)? 1f : 0f};
    } // GetZoneCornerFromCursor()

} // class MouseTool
