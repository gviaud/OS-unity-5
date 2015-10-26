//-----------------------------------------------------------------------------
// Assets/Scripts/fromLab/NewGui/GUIEditTools.cs - 02/2012 - KS

using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Classe du menu des outils de gestion de la lumière et du menu de retouche d'image (à droite)
public class GUIEditTools : MonoBehaviour
{
    public  enum     MenuMode {LightMode, ImageMode};                   // Modes du menu (Light = menu de gestion de la lumière,
                                                                        //                Image = menu d'édition d'image)
    private string[] m_imgLabels  = {"Gomme", "Matiere", "InPaint"};    // Labels des boutons d'édition d'image
    private string[] m_lgtLabels  = {"Ombrage", "Reflexion"};           // Labels des boutons de gestion de la lumière
    
    public  enum     EditTool {Eraser  = 100, Grass = 110,              // Outils de retouche d'image
                               Inpaint = 120,
                               Shadows = 200, Reflec = 210, None = 0};  // Outils de gestion de la lumière
    
    private MenuMode m_curMenuMode;                                     // Mode actuel du menu (menu d'édition d'image ou de gestion de lumière)
    private EditTool m_curTool;                                         // Outil actif
    
        
    private bool m_displayLightSliders;
	
    //-----------------------------------------------------
    void Start()
    {
        m_curTool     = EditTool.None;
        m_curMenuMode = MenuMode.ImageMode;
		m_displayLightSliders = false;
    }
    
    //-----------------------------------------------------
    // Exécution des actions des boutons sur la scène
    void Update()
    {}
    
    //-----------------------------------------------------
    // Mise à jour de l'interface
    void OnGUI()
    {
        //GUI.skin = skin;
//        switch (curTool) 
//        {
//            case (int) EditTools.Eraser :
//                // Effet de surbrillance    
//            break;
//      
//            case (int) EditTools.Grass :
//                // Effet de surbrillance    
//            break;
//        }
		if(m_displayLightSliders)
            GameObject.Find("LightPivot").GetComponent<LightConfiguration>().getGui();
    }

    //-----------------------------------------------------
    public void DisplayImgEditTools(Rect displayZone)
    {
        //GUI.skin = skin;
        GUI.BeginGroup(displayZone);
        float btnH = 50;        // TODO rendre générique la hauteur de bouton?
        int toolCount = m_imgLabels.Length;
        
        for(int i=0 ; i<toolCount ; i++)
        {
            if(GUI.Button(new Rect(displayZone.width-btnH,(i*btnH),btnH,btnH), m_imgLabels[i]))
            {
                m_curTool = (EditTool) 100+10*i;         // 100 = outil1, 110 = outil2, etc.
                
                if(m_curTool == EditTool.Inpaint) // L'inpainting n'a pas de sous-outil : on l'affiche directement
                {
                    if(GameObject.Find("Background/backgroundImage").GetComponent("PolygonTracer") == null &&
                       GameObject.Find("Background/backgroundImage").GetComponent<GUITexture>().texture != null)
                    {
//                      Debug.Log("GUISubTools : Activation Polygon Inpainting");
                        GameObject.Find("Background/backgroundImage").AddComponent<PolygonTracer>();
                        
                        // Passer le PolygonTracer en "mode Inpainting"
                        GameObject.Find("backgroundImage").GetComponent<PolygonTracer>().SetTool(EditTool.Inpaint);
                        
                        Component script = GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>();
						GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().ToggleSceneObjectsTransparent();
                    }
                    
//                    GetComponent<GUIMain>().SetFoldRgtMenu(true);
                } // if inpainting
                /*else    // Affichage du menu des sous-outils pour les outils qui en ont besoin, avec activation d'un sous-outil par défaut
                {
                    GetComponent<GUISubTools>().SetDefaultInitOn();
                    GetComponent<GUISubTools>().SetCurTool((int) m_curTool);
    //                Debug.Log("GUIEditTools : EditionImage, outil : "+m_curTool);
//                    GetComponent<GUIMain>().SetDisplaySubTools(true);
                }*/
            }
        }

        GUI.EndGroup();
    } // DisplayImgEditTools()
 
    //-----------------------------------------------------
    public void DisplayLightTools(Rect displayZone)
    {
        //GUI.skin = skin;
        
        GUI.BeginGroup(displayZone);
        float btnH = 50;        // TODO rendre générique la hauteur de bouton?
        int toolCount = m_lgtLabels.Length;
        for(int i=0 ; i< toolCount; i++)
        {
            if(GUI.Button(new Rect(displayZone.width-btnH,(i*btnH),btnH,btnH), m_lgtLabels[i]))
            {
                m_curTool = (EditTool) 200+10*i;         // 200 = outil1, 210 = outil2, etc.

				if(m_curTool == EditTool.Shadows)
				{
                    m_displayLightSliders = true; 		 // outil ombrage (pas de sous-outil)
//                    GetComponent<GUIMain>().SetFoldRgtMenu(true);
				}
                else
				{
					//GetComponent<GUISubTools>().SetCurTool((int) m_curTool);
//                	GetComponent<GUIMain>().SetDisplaySubTools(true);
				}
                //GetComponent<GUISubTools>().SetSubTool(i);
            }
        }
        GUI.EndGroup();
    } // DisplayLightTools()
    
	
    //-----------------------------------------------------
    // Annulation d'actions en cours (pour les outils n'ayant pas de sous-outils)
    public void ExitTool()
    {
        if(m_curTool == EditTool.Inpaint)
        {
            // TODO Si polygone pas fini, alors message de confirmation d'annulation de polygone
            Component script = GameObject.Find("Background/backgroundImage").GetComponent("PolygonTracer");
            if(script) ((PolygonTracer) script).abort(true);
//               else Debug.LogError("GUISubTools : Script \"Background/backgroundImage/PolygonTracer\" introuvable");
            
            GameObject.Find("MainNode").GetComponent<ObjBehavGlobal>().ToggleSceneObjectsTransparent();
        }
		else if(m_curTool == EditTool.Shadows)
			m_displayLightSliders = false;
        
        m_curTool = EditTool.None;
    }
    
    //-----------------------------------------------------
    // TODO rendre générique cette fonction pour GUIGridTools, GUIEditTools, GUISubTools ?
    public int GetButtonCount()
    {
        switch((int)m_curMenuMode)
        {
            case (int) MenuMode.ImageMode :
                return m_imgLabels.Length;
            case (int) MenuMode.LightMode :
                return m_lgtLabels.Length;
            default :
                return 0;
        }
    }
	
    //-----------------------------------------------------
    public void SetMenuMode(bool light)
    {
        if(light)
            m_curMenuMode = MenuMode.LightMode;
        else
            m_curMenuMode = MenuMode.ImageMode;
    }
    
    //-----------------------------------------------------
    public int GetCurrentTool()
    {
        return (int) m_curTool;
    }
    
} // class GUIEditTools
