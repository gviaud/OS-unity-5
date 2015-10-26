using UnityEngine;
using System.Collections;

using Pointcube.Global;
using Pointcube.InputEvents;

public class GuiTextureClip : MonoBehaviour
{
    private  enum       State { none, halfShow, show, hide, moving };
    private  State      m_state = State.none;

    // -- Références scène --
    public   GameObject m_mainScene;
    public   GameObject m_backGrid;

    // -- Textures & styles --
    public   Texture    m_gradientTex;
    public   int        m_gradientTexLimit;     // Pixel à partir duquel afficher la barre qui descend (hauteur du groupe)

    public   GUIStyle   m_buttonStyle;

    private  Texture    m_photoTexture;         // Récupéré automatiquement à partir de m_backGrid
    private  Texture    m_buttonTex;

    // -- Rect --
    private  Rect       m_groupRect;
    private  Rect       m_photoTextureRect;
    private  Rect       m_gradientRect;
    private  Rect       m_hideButtonRect;

    private  Vector2    m_screenCenter;

    // -- Etat du Avant/Après --
    private  bool       m_objectsHidden;

    // -- transitions animées --
    private bool            m_switchAnim;       // Transition changement Before/After ou After/Before
    private float           m_switchProg;       // Avancement de la transition
    public  float           m_barHeight;        // Hauteur de la barre
    private float           m_barStartHeight;   // Position de départ
    private float           m_barEndHeight;     // d'arrivée


    public bool m_active;
	private bool m_override = true;
	private bool _uiLocked = false;

    private static readonly int    sLerpDuration = 75;

    private static readonly string DEBUGTAG = "GuiTextureClip : ";
    private static readonly bool   DEBUG    = true;

#region unity_func
    // --------------------------------------------
    void Awake()
    {
        // -- Initialisations --
        m_objectsHidden = false;
        m_screenCenter  = new Vector2(Screen.width/2, Screen.height/2);

        m_groupRect         = new Rect(0f,0f,0f,0f);
        m_gradientRect      = new Rect(0f,0f,0f,0f);
        m_photoTextureRect  = new Rect(0f,0f,0f,0f);
        m_hideButtonRect    = new Rect(0f,0f,0f,0f);

        m_switchAnim        = false;
        m_switchProg        = 0f;
        m_barHeight         = 0f;
        m_barStartHeight    = 0f;
        m_barEndHeight      = 0f;

        m_buttonTex = m_buttonStyle.normal.background;

        // -- erreurs si paramètres inspecteur manquants --
        if(m_buttonTex == null)
            Debug.LogError(DEBUGTAG+"No texture assigned to skin.normal. Please assign one in the inspector.");
        if(m_mainScene == null)
            Debug.LogError(DEBUGTAG+"Main Scene not assigned in inspector, please set it.");
        if(m_backGrid == null)
            Debug.LogError(DEBUGTAG+"Back Grid not assigned in inspector, please set it.");

        // -- Evenements --
        UsefullEvents.OnResizingWindow  += SetRects;
        UsefullEvents.OnResizeWindowEnd += SetRects;
		
		UsefullEvents.LockGuiDialogBox  += LockUI;
    }

    // --------------------------------------------
    void Start()
    {
//        m_animSpeed = 10*Time.deltaTime;
//        m_mainScene = GameObject.Find("MainScene");

        SetRects();
    }

    // --------------------------------------------
    void OnGUI()
    {
        if(!m_active)  return;

        GUI.depth = -10;            // Afficher cette GUI devant le reste

        if(m_switchAnim)
        {
            if(m_photoTexture == null)
            {
                Debug.LogError(DEBUGTAG+"No photoTexture found. Please set it using SetTexture().");
                m_objectsHidden = false;
                m_switchAnim = false;
                Camera.main.GetComponent<ObjInteraction>().setActived(true);
            }
            else
            {
                // -- Affichage de la texture qui masque le montage --
                GUI.BeginGroup( m_groupRect );
                GUI.DrawTexture( m_photoTextureRect, m_photoTexture );
                GUI.EndGroup();

                Matrix4x4 bkup = GUI.matrix;

//                if(!m_objectsHidden)
//                    GUIUtility.RotateAroundPivot(180f, m_screenCenter);

                GUI.DrawTexture(m_gradientRect, m_gradientTex, ScaleMode.StretchToFill);

                GUI.matrix = bkup;
            }
        } // Si transition animée en cours
        else
        {
            if(!m_objectsHidden)
            {
                if(GUI.Button(m_hideButtonRect, "", m_buttonStyle) && !_uiLocked)     // Bouton cacher montage
                    LaunchHideAnim();
            }
            else
            {
                // -- Affichage de la texture qui masque le montage --
                GUI.BeginGroup( m_groupRect );
                GUI.DrawTexture( m_photoTextureRect, m_photoTexture );
                GUI.EndGroup();

                Matrix4x4 bkup = GUI.matrix;
                GUIUtility.RotateAroundPivot(180f, m_screenCenter);

                if(GUI.Button(m_hideButtonRect, "", m_buttonStyle))     // -- Bouton ré-afficher montage --
                    LaunchUnhideAnim();

                GUI.matrix = bkup;
            }
        } // Si pas transition animée

    } // OnGUI()

    // ----------------------------------------------------
    void FixedUpdate()
    {
        m_active =!m_mainScene.GetComponent<GUIStart>().isActive()&&
                !m_mainScene.GetComponent<GUIMenuLeft>().isVisible()&&
                !m_mainScene.GetComponent<GUIMenuRight>().isVisible()&&
                !m_mainScene.GetComponent<GUISubTools>().isActive()&&
                !m_mainScene.GetComponent<GUIMenuInteraction>().isConfiguring&&
                !m_mainScene.GetComponent<GUIStart>().enabled &&
                !m_mainScene.GetComponent<HelpPanel>().isHelpVisible() &&
                !Camera.main.GetComponent<ObjInteraction>().isSelectionsActives()&&
                Camera.main.GetComponent<ObjInteraction>().getSelected() == null&&
				m_override;

        if(!m_active) return;

        if(m_switchAnim)
        {
            if(m_switchProg <= sLerpDuration)
            {
                m_barHeight = Mathf.Lerp(m_barStartHeight, m_barEndHeight,
                                                      ((-Mathf.Cos(m_switchProg/sLerpDuration*2*Mathf.Acos(0))+1)/2));
                m_switchProg++;
            }
            else
            {
                m_switchAnim = false;
                m_switchProg = 0;

                if(!m_objectsHidden)
                {
                    Camera.main.GetComponent<ObjInteraction>().setActived(true); // Réactiver interactions objets
                    UsefullEvents.FireHideGUIforBeforeAfter(false);   // Afficher la GUI en fin de transition
                }
            }
            SetBarHeight();
        }

    } // FixedUpdate()

    //-----------------------------------------------------
    void OnDestroy()
    {
        // -- Evenements --
        UsefullEvents.OnResizingWindow  -= SetRects;
        UsefullEvents.OnResizeWindowEnd -= SetRects;
		UsefullEvents.LockGuiDialogBox  -= LockUI;
    }

#endregion

    //-----------------------------------------------------
    private void LaunchHideAnim()
    {
        m_objectsHidden = true;
        Camera.main.GetComponent<ObjInteraction>().setActived(false);  // Désactiver interactions objets
        UsefullEvents.FireHideGUIforBeforeAfter(true); // Cacher la GUI

        // -- lancer animation de masquage du montage --
        m_barStartHeight = 0f;
        m_barEndHeight = 1f;
        m_switchAnim = true;
    }

    //-----------------------------------------------------
    private void LaunchUnhideAnim()
    {
        m_objectsHidden = false;
//        ObjInteraction.setActived(true);      // Activer interactions obj. <== fait en fin de transition
//        UsefullEvents.FireHideGUIforBeforeAfter(false); // Afficher la GUI <== idem

        // -- lancer animation de ré-affichage --
        m_barStartHeight = 1f;
        m_barEndHeight = 0f;
        m_switchAnim = true;
    }

    //-----------------------------------------------------
    public void SetRects()
    {
        m_groupRect.Set(0, 0, Screen.width, 0 /*cf. SetBarHeight()*/);
        m_hideButtonRect.Set((Screen.width - m_buttonTex.width)/2, 0, m_buttonTex.width, m_buttonTex.height);
        m_gradientRect.Set(0, 0 /*cf. SetBarHeight()*/, Screen.width, m_gradientTex.height);

        SetBarHeight();

        m_screenCenter.Set(Screen.width/2, Screen.height/2);
    }

    //-----------------------------------------------------
    private void SetBarHeight()
    {
        float tmp = (m_barHeight * (Screen.height+m_gradientTex.height) - m_gradientTexLimit);
        m_groupRect.height = (tmp < 0f ? 0f : (tmp > Screen.height ? Screen.height : tmp));
        m_gradientRect.y = tmp - (m_gradientTex.height - m_gradientTexLimit);
    }

    //-----------------------------------------------------
    public void UpdatePhotoTexRect(float x, float y, float width, float height)
    {
        m_photoTextureRect.Set(x, y, width, height);
    }

    //-----------------------------------------------------
    public void SetTexture(Texture newTex)
    {
        m_photoTexture = newTex;
    }
	
	public bool IsOnUI()
	{
	    return PC.In.CursorOnUI(m_hideButtonRect);
	}
	
	public void SetOverride(bool b)
	{
		m_override = b;	
	}
	
	private void LockUI(bool isLck)
	{
		_uiLocked = isLck;
	}

} // class GuiTextureClip
