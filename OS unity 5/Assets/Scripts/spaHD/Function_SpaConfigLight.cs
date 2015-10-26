using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;

using Pointcube.Utils;
using Pointcube.Global;

public class Function_SpaConfigLight : MonoBehaviour, Function_OS3D
{
    //-----------------------------------------------------
    // Enums
    enum LightMode {off = 0, manual = 1, autocycle = 2}  // (off par defaut)

    //-----------------------------------------------------
    // -- Paramètres inspecteur --
    public  bool              mLoopLightMode;              // Lumière on/off (true = changement au prochain update)
    public  Color             mLightColor     = new Color(0f, 0.47f, 1f, 1f); // Poignée pour changer la couleur de la lumière
    public  float             mColorH		  = 212;       // Poignée pour changer la teinte de la couleur du spa
    public  float             mColorLoopSpeed = 0.15f;     // Vitesse de loop pour la lumière
	
    // -- Noms des objets du spa dans la scène (à respecter !) --
    public  string sLightSystemRootName    = "coque";
    public  string sWaterParentName        = "water";
    public  string sWaterSpecName          = "waterSpec";
    public  string sLEDmeshName            = "buses";
    public  string sLEDmaterialName        = "mat_lampes";
    public  string sMainSpotlightName      = "mainSpaLight";
    public  string sLEDglowObjectName      = "Glow";
    public  string sLEDspotObjectName      = "Light";

    // -- Réglages des couleurs, quand spa éclairé --
    public  float  sWaterSelfiLowerVal     = 75f;
    public  float  sMainSpotDesat          = 15f;
    public  float  sLEDspotDesat           = 100f;
    public  float  sLEDglowDesat           = 30f;
	
	public  float  sMainSpotLowering       = 7f;      // Règle l'intensité du mainspot en low (= intensité normale / cette variable)
	
	// -- GUI --
	private Rect               mSliderRect; //new Rect(50f, 50f, 10f, 500f);
    private Rect               m_uiRect1;
    private Rect               m_uiRect2;

    private float _uiOff7;                  // Offset pour le "touchez-ici pour valider"
    private GUISkin skin;
    
    private GUIMenuInteraction guiMenuInteraction;
    private GUIMenuConfiguration guiMenuConfiguration;
    private bool showGui = false;

	
    // -- Paramètres récupérés automatiquement sur le spa existant (s'il respecte le format requis) --
    private Transform         mSpa;                     // Spa dont la lumière est pilotée par ce script
    private Transform         mLightSystemRoot;         // Racine de l'arborescence du système de lumières du spa
    private Material          mLEDmat;                  // Matériau des lumières du spa
    private GameObject        mWaterSpec;               // Objet de l'eau du spa (avec un shader de la famille DispNormRimSpec)
    private Light             mMainSpotlight;           // Spot principal du spa (qui éclaire du dessus)
    private List<GameObject>  mLEDglows;                // Liste des glows des LEDs
    private List<Light>       mLEDspots;                // Liste des spots éventuels des LEDs
    private Color             mLEDmatColor;             // Couleur d'origine du matériau du verre des LED
    private Color             mLinerColor;              // Couleur moyenne de la coque quand spa éteint

    // -- Variables fonctionnement général --
    private LightMode         mLightMode;               // Mode de lumière
    private Color             mCurLightCol;             // Couleur de la lumière actuelle
    private float             mCurColorH;               // Teinte actuelle de la couleur de la lumière

    private bool              mMainSpotLow;             // Si le spot principal doit avoir une intensité basse (spa vide)
    private float             mSpotsLitI;               // Intensité à donner aux spots au rallumage des lumières

    // -- Variables pour lerp allumage / extinction des lumières --
    private bool              mAnimOnOff;               // Animation en cours ou non
    private float             mAnimOnOffProg;           // Progression de l'animation si en cours
    private float             mSpotlightsStartI;        // Valeur de départ de l'intensité des spotlights pour l'anim en cours
    private float             mSpotlightsEndI;          // Valeur de fin de l'intensité des spotlights pour l'anim en cours
    private Color             mGlowColorStart;          // Couleur principale de départ du shader des glow
    private Color             mGlowColorEnd;            // Couleur principale de fin du shader des glow
    private Color             mMatLEDcolStart;          // Couleur de départ du matériau des LEDs (mat_lampes)
    private Color             mMatLEDcolEnd;            // Couleur de fin du matériau des LEDs (mat_lampes)
    private Color             mWaterSelfiColStart;      // Couleur de départ de la teinte de l'eau (waterSpec)
    private Color             mWaterSelfiColEnd;        // Couleur de fin de la teinte de l'eau

    // -- Variables allumage / extinction spotlight principal (pour quand on vide le spa)
    private bool              mAnimMainSpotOnOff;       // Animation en cours ou non
    private float             mAnimMainSpotOnOffProg;   // Progression
    private float             mMainSpotIstart;
    private float             mMainSpotIend;

    // -- Variables de MÀJ de la range des spotlights quand l'echelle du spa est modifiée --
    private Vector3           mOldSpaScale;
    private float             mMainSpotlightBaseRange;
    private float[]           mLEDSpotBaseRanges;

    // ----------------------------------------------------
    private static readonly int    sLerpDuration           = 40; // en fixedFrames avec fixedTime = 0.01

    // -- Strings language --
    private static readonly string  _spaLightOff            = "SpaConfigLight.off";
    private static readonly string  _spaLightManual         = "SpaConfigLight.manual";
    private static readonly string  _spaLightAutocycle      = "SpaConfigLight.autocycle";

    // -- Debug --
    private static readonly string DEBUGTAG = "SpaConfig : ";


#region unity_func

    //-----------------------------------------------------
    void Awake()
    {
        // -- GUI --
        _uiOff7 = Screen.width-290;
        mSliderRect = new Rect(0,84,142,600);
        m_uiRect1 = new Rect(0f /*_uiOff7*/, 0, 300, 0f /*Screen.height*/);
        m_uiRect2 = new Rect(0,0,80, 0f /*Screen.height*/);
//        FitToScreen(); exécuté dans le OnEnable après le Awake
    }

    //-----------------------------------------------------
    void OnEnable()
    {
        UsefullEvents.OnResizeWindowEnd += FitToScreen;
        UsefullEvents.OnResizingWindow  += FitToScreen;
        FitToScreen();
    }

    //-----------------------------------------------------
    void Start ()
    {
		guiMenuConfiguration = GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>();
		guiMenuInteraction = GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>();

		if(skin == null)
			skin = (GUISkin)Resources.Load("skins/mode2D");

        // -- Initialisation variables membres --
        mLoopLightMode      = false;
        mLightMode          = LightMode.off;
        mCurColorH          = 0f;
        mCurLightCol        = new Color();

        mLightSystemRoot    = null;
        mWaterSpec          = null;
        mLEDmat             = null;
        mMainSpotlight      = null;

        mOldSpaScale        = new Vector3();

        // -- Variables transitions animées --
        mAnimOnOff          = false;
        mAnimOnOffProg      = 0f;
        mSpotlightsStartI   = 0f;
        mSpotlightsEndI     = 0f;
        mGlowColorStart     = Color.black;
        mGlowColorEnd       = Color.black;
        mMatLEDcolStart     = Color.black;
        mMatLEDcolEnd       = Color.black;
        mWaterSelfiColStart = Color.black;
        mWaterSelfiColEnd   = Color.black;

        mAnimMainSpotOnOff  = false;
        mAnimMainSpotOnOffProg = 0f;
        mMainSpotIstart     = 0f;
        mMainSpotIend       = 0f;

        // -- Récupérations des objets de la scène dont on a besoin --
        mSpa = transform;
        Transform child, child2;

        for(int i=0; i<mSpa.GetChildCount(); i++)
        {
            child = mSpa.GetChild(i);
            if(child.name == sLightSystemRootName)
                mLightSystemRoot = child;                   // Nœud parent du système de lumières
            else if(child.name == sWaterParentName)
            {
                for(int j=0; j<child.GetChildCount(); j++)
                {
                    child2 = child.GetChild(j);
                    if(child2.name.Equals(sWaterSpecName))
                        mWaterSpec = child2.gameObject;     // Eau (celle avec le shader DispNormRimSpec-like)
                }
            }
            else if(child.name == sLEDmeshName)
            {
                for(int j=0; j<child.GetComponent<Renderer>().materials.Length; j++)
                {
                    if(child.GetComponent<Renderer>().materials[j].name.Contains(sLEDmaterialName))
                        mLEDmat = child.GetComponent<Renderer>().materials[j];      // Matériau du mesh du verre des LEDs
                }
            }
        }

        // -- Si un objet est absent, afficher une erreur et désactiver le script --
        if(mLightSystemRoot == null)
            { DeactivateWithLogError(sLightSystemRootName); return; }
        if(mWaterSpec       == null)
            { DeactivateWithLogError(sWaterSpecName);       return; }
        if(mLEDmat          == null)
            { DeactivateWithLogError(sLEDmaterialName);     return; }

        // -- Récupération des spotlights et des matériaux des glows des LEDs --
        mLEDglows = new List<GameObject>();
        mLEDspots = new List<Light>();

        for(int i=0; i<mLightSystemRoot.GetChildCount(); i++)
        {
            child = mLightSystemRoot.GetChild(i);
            if(child.name == sMainSpotlightName)
                mMainSpotlight = child.GetComponent<Light>();
            else
            {
                for(int j=0; j<child.GetChildCount(); j++)
                {
                    child2 = child.GetChild(j);
                    if(child2.name == sLEDglowObjectName)
                        mLEDglows.Add(child2.gameObject);
                    else if(child2.name == sLEDspotObjectName)
                        mLEDspots.Add(child2.GetComponent<Light>());
                } // foreach child of child
            } // if not main spotlight

        } // For each child

        if(mMainSpotlight == null)
            { DeactivateWithLogError(sMainSpotlightName);   return; }

        // -- Couleur moyenne de la coque (correspond normalement à "lumières éteintes") --
		if (mWaterSpec.GetComponent<Renderer>().material.HasProperty("_MainColor"))
			mLinerColor = mWaterSpec.GetComponent<Renderer>().material.GetColor("_MainColor");

        // -- Couleur du matériau du verre des LED (correspond normalement à "lumières éteintes") --
        mLEDmatColor = mLEDmat.GetColor("_Color");

        // -- Echelle du spa (et range des spotlights) --
        mOldSpaScale = Vector3.one; // Note : Pas mOldSpaScale = mSpa.LocalScale sinon la range des proj n'est pas mise à jour au swap.
        mMainSpotlightBaseRange = mMainSpotlight.range;
        mLEDSpotBaseRanges = new float[mLEDspots.Count];
        for(int i=0; i<mLEDspots.Count; i++)
            mLEDSpotBaseRanges[i] = mLEDspots[i].range;

        // Note : mSpotsLitI est MÀJ par spaConfigWater, qui appelle updateSpotsIntensity à son 1er Update()

    } // Start()

    //-----------------------------------------------------
    private void DeactivateWithLogError(string objectName)
    {
        Debug.LogError(DEBUGTAG+" object \""+objectName+"\" not found. Deactivating Script.");
        this.enabled = false;
    }

    //-----------------------------------------------------
    void Update()
    {	
		showGui=false;
		if(guiMenuInteraction.isConfiguring == true)
		{
			if(guiMenuConfiguration.GetConfiguredObj().GetHashCode() == gameObject.GetHashCode())
			{
				//enabled=true;
				if(mLightMode == LightMode.manual)
				{
					showGui=true;
					guiMenuInteraction.setVisibility(false);
					guiMenuInteraction.SetMenuSubRect(m_uiRect2);

                   // if(PC.In.Click1Up() && !PC.In.CursorOnUIs(m_uiRect1, m_uiRect2))
                   //     Validate();

//                    if((Application.platform == RuntimePlatform.WindowsEditor)
//						||(Application.platform == RuntimePlatform.WindowsPlayer)
//						||(Application.platform == RuntimePlatform.OSXEditor)
//						||(Application.platform == RuntimePlatform.OSXPlayer))
//					{	
//						
//						if (Input.GetMouseButtonDown(0))
//						{
//							Vector3 mousePos = Input.mousePosition;
//							if(!m_uiRect1.Contains(mousePos) && !m_uiRect2.Contains(mousePos))
//								Validate();
//						}
//					}
//					else
//					{
//						if (Input.touchCount <= 0)
//						{ }
//						else
//						{			
//							Touch firstTouch = Input.touches[0];
//							if(!m_uiRect1.Contains(firstTouch.position) && !m_uiRect2.Contains(firstTouch.position))
//							{
//								switch (firstTouch.phase)
//								{
//									case TouchPhase.Began:
//									{
//										_hasMoved = false;
//										break;
//									}		
//									case TouchPhase.Ended:
//									{
//										if(_hasMoved==false)
//											Validate();
//										break;
//									}
//								}
//							}
//						}
//					}
				}
			}
		}

        // -- Allumer / éteindre le spa --
        if(mLoopLightMode)
        {
            mLoopLightMode = false;
            LoopSpaLight();
        }

        // -- Maj de la couleur de la lumière du spa --
        if(!mCurLightCol.Equals(mLightColor))
        {
            UpdateSpaLightColor(mLightColor, mLightMode!=LightMode.off);
            mCurLightCol = mLightColor;
        }

        if(mColorH != mCurColorH)
        {
            if      (mColorH > 359.9f) mColorH = 359.9f;               // Butées 0-255
            else if (mColorH < 0f)     mColorH = 0f;

            mCurColorH = mColorH;
            mCurLightCol = ColorHSVutils.SetHueTo(mCurLightCol, mCurColorH);
            mLightColor = mCurLightCol;
            UpdateSpaLightColor(mLightColor, mLightMode!=LightMode.off);
        }
        
        // -- Maj de la range des spotlight quand la scale du spa est modifiée --
        if(!mOldSpaScale.Equals(mSpa.localScale))
        {
            mMainSpotlight.range = mMainSpotlightBaseRange * mSpa.localScale.x;
            for(int i=0; i<mLEDspots.Count; i++)
                mLEDspots[i].range = mLEDSpotBaseRanges[i] * mSpa.localScale.x;

            mOldSpaScale = mSpa.localScale;
        }

    } // Update()

    //-----------------------------------------------------
    void FixedUpdate()
    {
        if(mLightMode == LightMode.autocycle)
        {
            mColorH = (mColorH + mColorLoopSpeed)%360f;
        }

        if(mAnimOnOff)  // Animation allumage / extinction des lumières
        {
            if(mAnimOnOffProg < sLerpDuration)
            {
                // -- Baisser / augmenter l'intensité des spots (spot principal et spots des LEDs) --
                float lerpIntensity = Mathf.Lerp(mSpotlightsStartI, mSpotlightsEndI,
                                                      ((-Mathf.Cos(mAnimOnOffProg/sLerpDuration*2*Mathf.Acos(0))+1)/2));
                mMainSpotlight.intensity = lerpIntensity;

                for(int i=0; i<mLEDspots.Count; i++)
                    mLEDspots[i].intensity = lerpIntensity;

                // -- Baisser / augmenter l'alpha de _MainCol des glow --
                Color lerpColor = Color.Lerp(mGlowColorStart, mGlowColorEnd,
                                                      ((-Mathf.Cos(mAnimOnOffProg/sLerpDuration*2*Mathf.Acos(0))+1)/2));
                for(int i=0; i<mLEDglows.Count; i++)
                    mLEDglows[i].GetComponent<Renderer>().material.SetColor("_MainColor", lerpColor);

                // -- Baisser / augmenter la luminosité de la couleur de mat_lampes --
                lerpColor = Color.Lerp(mMatLEDcolStart, mMatLEDcolEnd,
                                                      ((-Mathf.Cos(mAnimOnOffProg/sLerpDuration*2*Mathf.Acos(0))+1)/2));
                mLEDmat.SetColor("_Color", lerpColor);

                // -- Lerp la teinte (selfIllu) de l'eau du spa --
                lerpColor = Color.Lerp(mWaterSelfiColStart, mWaterSelfiColEnd,
                                                      ((-Mathf.Cos(mAnimOnOffProg/sLerpDuration*2*Mathf.Acos(0))+1)/2));
                mWaterSpec.GetComponent<Renderer>().material.SetColor("_SelfIllu", lerpColor);

                mAnimOnOffProg++;
            }
            else
            {
                mAnimOnOff = false;
                mAnimOnOffProg = 0f;

                if(mLightMode == LightMode.off)     // Si éteint, désactiver les spots et les glows
                    LitSpa(false);
            }
        } // AnimOnOff

        if(mAnimMainSpotOnOff)
        {
            if(mAnimMainSpotOnOffProg < sLerpDuration)
            {
                // -- Baisser / augmenter l'intensité des spots (spot principal et spots des LEDs) --
                mMainSpotlight.intensity = Mathf.Lerp(mMainSpotIstart, mMainSpotIend,
                                              ((-Mathf.Cos(mAnimMainSpotOnOffProg/sLerpDuration*2*Mathf.Acos(0))+1)/2));

                mAnimMainSpotOnOffProg++;
            }
            else
            {
                mAnimMainSpotOnOff = false;
                mAnimMainSpotOnOffProg = 0f;
            }
        } // AnimMainSpotOnOff

    } // FixedUpdate()

    //-----------------------------------------------------
    void OnGUI()
    {
		if(showGui)
		{
			GUISkin bkup = GUI.skin;
			GUI.skin = skin;
			if(mLightMode == LightMode.manual)
				mColorH = GUI.VerticalSlider(mSliderRect, mColorH, 0.0F, 255.0F);
			GUI.skin = bkup;
		}
    }

    //-----------------------------------------------------
    void OnDisable()
    {
        UsefullEvents.OnResizeWindowEnd -= FitToScreen;
        UsefullEvents.OnResizingWindow  -= FitToScreen;
    }
#endregion

    //-----------------------------------------------------
    private void FitToScreen()
    {
        _uiOff7 = Screen.width - 290; // 290 = largeur du menu de droite
        m_uiRect1.x = _uiOff7;
        m_uiRect1.height = Screen.height;

        m_uiRect2.height = Screen.height;
    }

    //-----------------------------------------------------
    public void UpdateSpotIntensity(Color newLinerTexAvgCol)
    {
        float v = ColorHSVutils.GetValue(newLinerTexAvgCol)*255f;
        float newI;
        //Debug.Log(DEBUGTAG+" ---------------------------------------- v="+v);
        if(v<128)
            newI = ((128f-v)/17.181f)+0.45f; // ((128f-v)/17.534f)+0.7f;
        else
            newI = 1.55f; // 1.7f;

        //Debug.Log(DEBUGTAG+" ---------------------------------------- newI="+newI);
        mSpotsLitI = newI;
		
		if(mLightMode != LightMode.off)       // Si lumières allumées, application directe des changements
		{
			mMainSpotlight.intensity = mMainSpotLow ? mSpotsLitI/sMainSpotLowering : mSpotsLitI;
			for(int i=0; i<mLEDspots.Count; i++)
				mLEDspots[i].intensity = mSpotsLitI;
		}

    } // UpdateSpotIntensity()
    
    //-----------------------------------------------------
    public void UpdateWaterColor(Color c)
    {
        mWaterSpec.GetComponent<Renderer>().material.SetColor("_MainColor", c);
    }

    //-----------------------------------------------------
    private void LoopSpaLight()
    {
        if(mAnimOnOff) return;

        mLightMode = (LightMode) (((int)mLightMode+1) % (Enum.GetNames(typeof(LightMode)).Length));

        if(mLightMode == LightMode.off)
            SwitchLightsOff();
        else if(mLightMode == LightMode.manual)
            SwitchLightsOn ();
//        LitSpa(mLightMode != LightMode.off);

    } // ToggleSpaLight()

    //-----------------------------------------------------
    // Paramétrer l'animation de transition, puis la lancer
    private void SwitchLightsOn()
    {		
		//GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);			
		guiMenuInteraction.setVisibility(false);
		/*GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;			
		Camera.mainCamera.GetComponent<ObjInteraction>().setSelected(null,true);
		Camera.mainCamera.GetComponent<ObjInteraction>().setActived(false);		*/
		
        LitSpa(true);                                     // Activer les objets (renderers, lights) du système de LED
        mSpotlightsStartI = mMainSpotlight.intensity;     // Main spotlight et LEDs spotlights
        mSpotlightsEndI   = mMainSpotLow ? mSpotsLitI/sMainSpotLowering : mSpotsLitI;

        if(mLEDglows.Count > 0)                           // Glows des LED
        {
            Color endCol    = mLEDglows[0].GetComponent<Renderer>().material.GetColor("_MainColor");
            mGlowColorStart = mLEDglows[0].GetComponent<Renderer>().material.GetColor("_MainColor");
            endCol.a        = 1f;
            mGlowColorEnd   = endCol;
        }

        mMatLEDcolStart     = mLEDmat.GetColor("_Color"); // Couleur du matériau des LED
        mMatLEDcolEnd       = Color.white;
		
		if(mWaterSpec.GetComponent<Renderer>().material.HasProperty("_SelfIllu"))
        	mWaterSelfiColStart = mWaterSpec.GetComponent<Renderer>().material.GetColor("_SelfIllu");   // Couleur de selfIllu de l'eau (waterSpec)
        mWaterSelfiColEnd   = ColorHSVutils.LowerValue(mLightColor, sWaterSelfiLowerVal/255f);

        mAnimOnOff = true;
    } // SwitchLightsOn()

    //-----------------------------------------------------
    // Paramétrer l'animation de transition, puis la lancer
    private void SwitchLightsOff()
    {
        mSpotlightsStartI = mMainSpotlight.intensity;     // Main spotlight
        mSpotlightsEndI   = 0f;

        if(mLEDglows.Count > 0)                           // Glows des LED
        {
            Color endCol    = mLEDglows[0].GetComponent<Renderer>().material.GetColor("_MainColor");
            mGlowColorStart = mLEDglows[0].GetComponent<Renderer>().material.GetColor("_MainColor");
            endCol.a        = 0f;
            mGlowColorEnd   = endCol;
        }

        mMatLEDcolStart     = mLEDmat.GetColor("_Color"); // Couleur du matériau des LED
        mMatLEDcolEnd       = mLEDmatColor;

        mWaterSelfiColStart = mWaterSpec.GetComponent<Renderer>().material.GetColor("_SelfIllu");   // Couleur de selfIllu de l'eau (waterSpec)
        mWaterSelfiColEnd   = Color.black;

        mAnimOnOff = true;
    } // SwitchLightsOff()

    //-----------------------------------------------------
    // Paramétrer l'animation de transition, puis la lancer
    public void DecreaseMainSpotlight()
    {
		if(mLightMode != LightMode.off)                                // Si lumières allumées, transition animée
		{
	        mMainSpotIstart    = mMainSpotlight.intensity;
	        mMainSpotIend      = mSpotsLitI/sMainSpotLowering;
			mAnimMainSpotOnOff = true;          // Lancer la transition
		}
		mMainSpotLow = true;                // Booléen pour fixer l'intensité correctement si allumage différé des lumières
		
    } // SwitchLightsOff()

    //-----------------------------------------------------
    // Paramétrer l'animation de transition, puis la lancer
    public void IncreaseMainSpotlight()
    {
		if(mLightMode != LightMode.off)                                // Si lumières allumées, transition animée
		{
	        mMainSpotIstart    = mMainSpotlight.intensity;
	        mMainSpotIend      = mSpotsLitI;
	        mAnimMainSpotOnOff = true;          // Lancer la transition
		}
        mMainSpotLow = false;
		
    } // SwitchLightsOff()

    //-----------------------------------------------------
    // Vieille méthode pour allumer / éteindre les spas (sans transition animée)
    // Toujours appelée pour désactiver les objets invisibles et les réactiver quand besoin.
    private void LitSpa(bool lightsOn)
    {
        // -- Mise à jour du système de leds du spa --
        mMainSpotlight.enabled = lightsOn;                          // Activer / désactiver la lumière globale du spa

        for(int i=0; i<mLEDglows.Count; i++)
            mLEDglows[i].GetComponent<Renderer>().enabled = lightsOn;               // Activer / désactiver les glows des leds

        for(int i=0; i<mLEDspots.Count; i++)
            mLEDspots[i].enabled       = lightsOn;                  // Activer / désactiver les lumières des leds

        // -- Mise à jour du matériau des LEDs (mesh) du spa --
//        mLEDmat.SetColor("_Color", (lightsOn ? Color.white : mLEDmatColor));     // TODO constante au lieu de white (récupérée dans le start)

        // -- Mise à jour de la teinte (selfIllu) de l'eau du spa --
//        mWaterSpec.renderer.material.SetColor("_SelfIllu",
//                            (lightsOn ? ColorHSVutils.LowerValue(mLightColor, sWaterSelfiLowerVal/255f) : Color.black)); // TODO black idem

    } // LitSpa()

    //-----------------------------------------------------
    private void UpdateSpaLightColor(Color newCol, bool lightsOn)
    {
        // -- Mise à jour de la couleur des Glows et des leds --
        mMainSpotlight.color = ColorHSVutils.Desaturate(newCol, sMainSpotDesat/255f);     // couleur de la lumière principale du spa

        for(int i=0; i<mLEDglows.Count; i++)
        {
            mLEDglows[i].GetComponent<Renderer>().material.SetColor("_RimColor",            // couleurs des glows des LEDs
                         ColorHSVutils.SetValueTo(ColorHSVutils.Desaturate(newCol, sLEDglowDesat/255f), ColorHSVutils.GetValue(mLinerColor)));
        }

        for(int i=0; i<mLEDspots.Count; i++)
            mLEDspots[i].color = ColorHSVutils.Desaturate(newCol, sLEDspotDesat/255f);

        // -- Mise à jour de la tinte de l'eau du spa --
        mLightColor = newCol;

        if(lightsOn)
        {
            //mWater.renderer.material.SetColor("_MainColor", mLightColor);
            mWaterSpec.GetComponent<Renderer>().material.SetColor("_SelfIllu", ColorHSVutils.LowerValue(mLightColor, sWaterSelfiLowerVal/255f));
        }
//        Desaturate(newCol, 128/255)

    } // UpdateSpaLightColor()

    //-----------------------------------------------------
    // GUI OS3D (implémentation de Function_OS3D)
    public void DoAction ()                      // Action au clic du bouton du menu configuration
    {
	//	enabled = true;
        LoopSpaLight();
    }
			
	void Validate()
	{
		Camera.main.GetComponent<ObjInteraction>().configuringObj(null);
	//	GetComponent<GUIMenuInteraction> ().unConfigure();
	//	GetComponent<GUIMenuInteraction> ().setVisibility (false);
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,false);				
	//	enabled = false;
	}

    public string GetFunctionParameterName()     // Label du bouton
    {
        if (mLightMode == LightMode.manual)
        {
            return TextManager.GetText(_spaLightAutocycle);
        }
        else if(mLightMode == LightMode.autocycle)
        {
            return TextManager.GetText(_spaLightOff);
        }
        else //(mLightMode == LightMode.manual), else
        {
            return TextManager.GetText(_spaLightManual);
        }

    } // GetFunctionName()

    public string GetFunctionName()
    {
        return "";
    }
    
    public int GetFunctionId()
    {
        return 0;
    }
    
    //  sauvegarde/chargement
    public void save(BinaryWriter buf)
    {
        return;
    }
    
    public void load(BinaryReader buf)
    {
        return;
    }
    
    //Set L'ui si besoin
    public void setUI(FunctionUI_OS3D ui)
    {
        return;
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}
    
    //similaire au Save/Load mais utilisé en interne d'un objet a un autre (swap)
    public ArrayList getConfig()
    {
        return null;
    }
    
    public void setConfig(ArrayList config)
    {
        return;
    }

} // class spaConfigLight
