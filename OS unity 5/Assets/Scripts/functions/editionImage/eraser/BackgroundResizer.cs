//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/eraser/BackgroundResizer.cs - 02/2012 - KS

using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Classe utilisée pour redimensionner l'image de fond afin de créer une marge
// et donc un mini espace de travail autour.
// Trop compliqué à implémenter pour le redimensionnement des 16 images de la
// gomme (cf. EraserMask > ResizeImages()), donc non utilisé.
public class BackgroundResizer : MonoBehaviour
{
    private float m_backup_x;
    private float m_backup_y;
    private float m_backup_w;
    private float m_backup_h;
    
    //-----------------------------------------------------
	void Start ()
    {
        m_backup_x = -1;
        m_backup_y = -1;
        m_backup_w = -1;
        m_backup_h = -1;
	}
	
	//-----------------------------------------------------
    void Update ()
    {
	}
    
    //-----------------------------------------------------
    public float ResizeBg()
    {
        float  percent;
        Rect pixIn = GetComponent<GUITexture>().pixelInset;
        m_backup_x = pixIn.x;
        m_backup_y = pixIn.y;
        m_backup_w = pixIn.width;
        m_backup_h = pixIn.height;
        
        // Choisir un pourcentage de réduction de l'image de sorte que la dimension la plus grande
        // soit réduite de 32 pixels
        if(pixIn.width >= pixIn.height)
            percent = (32*100)/pixIn.width;
        else
            percent = (32*100)/pixIn.height;
        
//        Debug.Log("iciiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii "+percent);
        // Recentrer l'image
        pixIn.x += pixIn.width*percent/200;
        pixIn.y += pixIn.height*percent/200;
        pixIn.width -= pixIn.width*percent/100;
        pixIn.height -= pixIn.height*percent/100;
        GetComponent<GUITexture>().pixelInset = pixIn;
        
        return percent;
    } // ResizeBg()
    
    //-----------------------------------------------------
    public void ResetBg()
    {
        Rect pixIn = GetComponent<GUITexture>().pixelInset;
        pixIn.x = m_backup_x;
        pixIn.y = m_backup_y;
        pixIn.width = m_backup_w;
        pixIn.height = m_backup_h;
        GetComponent<GUITexture>().pixelInset = pixIn;
        
        m_backup_x = m_backup_y = -1;
        m_backup_h = m_backup_w = -1;
    } // ResetBg()
    
} // BackgroundResizer
