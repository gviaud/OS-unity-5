using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;

using Pointcube.Utils;

public class Function_SpaConfigWater : MonoBehaviour, Function_OS3D
{
    //-----------------------------------------------------
    // Enums
    enum WaterMode {empty = 0, turbulent = 1, calm = 2}

    //-----------------------------------------------------
    // -- Paramètres inspecteur --
    public  bool              mLoopWaterMode;              // Vide/Eau agitée/Eau calme (true = changement au prochain update)
	public  float             mDecalageEauVide = 0.8f;
	
    // -- Noms des objets du spa dans la scène (à respecter !) --
    public  string  sWaterParentName        = "water";
    public  string  sWaterSpecName          = "waterSpec";
    public  string  sWaterRefractName       = "waterRefract";
//    private static readonly string  sWaterDNRMshaderName    = "Pointcube/NormRimSpec";
//    private static readonly string  sWaterRefshaderName     = "Pointcube/Stained BumpDistort";

    // -- Réglages du shader d'eau (DispNormRimSpec-like) --
    public  float   sWaterMat_M1_TCblend    = 0.52f;      // Mode bouillonnant
    public  float   sWaterMat_M1_Alpha      = 0.385f;
    public  float   sWaterMat_M1_MinAlpha   = 0.26f;
    public  float   sWaterMat_M1_MinCoef    = 4f;
    public  float   sWaterMat_M1_DispAmt    = 0.20f; //0.075f;
//    private static readonly string  sWaterMat_M1_NMapPath   = "images/nmap_eauSpaAgitee";
//    private static readonly string  sWaterMat_M1_RGBMapPath = "images/rgbmap";
    public  float   sWaterRefMat_M1_Refract = 15f;

    public  float   sWaterMat_M2_TCblend    = 1f;         // Mode calme
    public  float   sWaterMat_M2_Alpha      = 0.25f;
    public  float   sWaterMat_M2_MinAlpha   = 0.19f;
	public  float   sWaterMat_M2_MinCoef    = 4f;
    public  float   sWaterMat_M2_DispAmt    = 0.2f; //0.03f;
//    private static readonly string  sWaterMat_M2_NMapPath   = "images/nmap_eauSpaCalme";
    public  float   sWaterRefMat_M2_Refract = 10f;
	
	
    // -- Paramètres récupérés automatiquement sur le spa existant (s'il respecte le format requis) --
    private Transform         mSpa;                     // Spa dont la lumière est pilotée par ce script
    private GameObject        mWater;                   // Objet de l'eau du spa (avec un shader de la famille DispNormRimSpec)
    private GameObject        mWaterRefract;            // 2e objet de l'eau du spa, avec le shader de réfraction

    // -- Variables fonctionnement --
    private WaterMode         mWaterMode;               // Mode d'eau
    private Color             mCurLightCol;             // Couleur de la lumière actuelle
    private float             mCurColorH;               // Teinte actuelle de la couleur de la lumière

    // -- Variables lerp vidange --
    private bool              mEmptyAnim;               // Animation de vidange en cours ou non
    private float             mEmptyAnimProg;           // Progression de l'animation
    private float             mEmptyAnimStart;          // Position de départ de waterSpec
    private float             mEmptyAnimEnd;            // De fin
    private float             mEmptyAnimRefStart;       // Idem pour waterRefract
    private float             mEmptyAnimRefEnd;

    // -- Variables lerp eau agitée -> eau calme
    private bool              mCalmAnim;                // Animation en cours ou non
    private float             mCalmAnimProg;
    private float             mCalmAnimBlendStart;
    private float             mCalmAnimBlendEnd;
    private float             mCalmAnimGlobAlphaStart;
    private float             mCalmAnimGlobAlphaEnd;
    private float             mCalmAnimMinAlphaStart;
    private float             mCalmAnimMinAlphaEnd;
    private float             mCalmAnimMinCoefStart;
    private float             mCalmAnimMinCoefEnd;
    private float             mCalmAnimAmountStart;
    private float             mCalmAnimAmountEnd;
    private float             mCalmAnimBmpAmtStart;
    private float             mCalmAnimBmpAmtEnd;

    // -- Réglage de l'objet de l'eau --
    private float             mWaterMeshFilledY;             // Position Y de l'eau quand remplie (récupérée sur l'objet)
    private float             mWaterMeshDefaultFilledY;      // Position Y de l'eau quand remplie (récupérée sur l'objet)
    private float             mWaterRefMeshFilledY;          // Position Y de l'eau quand remplie (récupérée sur l'objet)
    private float             mWaterRefMeshDefaultFilledY;   // Position Y de l'eau quand remplie (récupérée sur l'objet)
	private float             mWaterMeshEmptyY;              // Position Y de l'eau quand vide

    // ----------------------------------------------------
    // Constantes

    // -- Strings language --
    private static readonly string  _spaWaterCalm           = "SpaConfigWater.calm";
    private static readonly string  _spaWaterBubbling       = "SpaConfigWater.bubbling";
    private static readonly string  _spaWaterEmpty          = "SpaConfigWater.empty";

    // -- Debug --
    private static readonly string DEBUGTAG = "SpaConfig : ";

    private static readonly int     sLerpDuration          = 85;      // Durée pour lerp en fixedframes (50ff = 1s si fixedtime = 0.02)
    private static readonly int     sShortLerpDuration     = 40;

    //-----------------------------------------------------
	void Start ()
    {
        // -- Initialisation variables membres --
        mLoopWaterMode   = false;
        mWaterMode       = WaterMode.turbulent;

        mWater           = null;
        mWaterRefract    = null;

        // -- Variables Animation --
        mEmptyAnim         = false;
        mEmptyAnimProg     = 0f;   
        mEmptyAnimStart    = 0f;   
        mEmptyAnimEnd      = 0f;
        mEmptyAnimRefStart = 0f;
        mEmptyAnimRefEnd   = 0f;

        mCalmAnim               = false;
        mCalmAnimProg           = 0f;
        mCalmAnimBlendStart     = 0f;
        mCalmAnimBlendEnd       = 0f;
        mCalmAnimGlobAlphaStart = 0f;
        mCalmAnimGlobAlphaEnd   = 0f;
        mCalmAnimMinAlphaStart  = 0f;
        mCalmAnimMinAlphaEnd    = 0f;
        mCalmAnimMinCoefStart   = 0f;
        mCalmAnimMinCoefEnd     = 0f;
        mCalmAnimAmountStart    = 0f;
        mCalmAnimAmountEnd      = 0f;
        mCalmAnimBmpAmtStart    = 0f;
        mCalmAnimBmpAmtEnd      = 0f;

        // -- Récupérations des objets de la scène dont on a besoin --
        mSpa = transform;
        Transform child, child2;
        for(int i=0; i<mSpa.GetChildCount(); i++)
        {
			child = mSpa.GetChild(i);
			if(child.name == sWaterParentName)
			{
				for(int j=0; j<child.GetChildCount(); j++)
                {
					child2 = child.GetChild(j);
                    if(child2.name.Equals(sWaterSpecName))
						mWater = child2.gameObject;         // Eau (celle avec le shader DispNormRimSpec-like)
					else if(child2.name.Equals(sWaterRefractName))
						mWaterRefract = child2.gameObject;   // Eau (avec réfraction)
                }
			}
        } // foreach child

        // -- Si un objet est absent, afficher une erreur et désactiver le script --
        if(mWater           == null)
            { DeactivateWithLogError(sWaterSpecName);       return; }
        if(mWaterRefract    == null)
            { DeactivateWithLogError(sWaterRefractName);    return; }
		
//		mWaterMeshDefaultFilledY = mWater.transform.localPosition.y;
//		mWaterRefMeshDefaultFilledY = mWaterRefract.transform.localPosition.y;
		
		mWaterMeshFilledY = mWater.transform.localPosition.y;
		mWaterRefMeshFilledY = mWaterRefract.transform.localPosition.y;
		mWaterMeshEmptyY = mWaterMeshFilledY-mDecalageEauVide;
		if(usefullData.lowTechnologie)
		{
			Shader shad =Shader.Find("Transparent/Diffuse"); 
			mWater.GetComponent<Renderer>().material.shader = shad;
			mWater.GetComponent<Renderer>().material.SetColor("_Color", new Color (1.0f,1.0f,1.0f,0.5f));
			mWaterRefract.GetComponent<Renderer>().enabled=false;
			//mWaterRefract.renderer.material.shader = shad;
			//mWaterRefract.renderer.material.SetColor("_Color", new Color (1.0f,1.0f,1.0f,0.5f));
		}
		
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
        if(mLoopWaterMode)
        {
            mLoopWaterMode = false;
            LoopWaterMode();
        }
	} // Update()

    //-----------------------------------------------------
    void FixedUpdate()
    {
        if(mEmptyAnim)  // Animation vidange ou remplissage
        {
            if(mEmptyAnimProg < sLerpDuration)
            {
                Vector3 v = mWater.transform.localPosition;
                v.y = Mathf.Lerp(mEmptyAnimStart, mEmptyAnimEnd, ((-Mathf.Cos(mEmptyAnimProg/sLerpDuration*2*Mathf.Acos(0))+1)/2));
                mWater.transform.localPosition = v;

                v = mWaterRefract.transform.localPosition;
                v.y = Mathf.Lerp(mEmptyAnimRefStart, mEmptyAnimRefEnd, ((-Mathf.Cos(mEmptyAnimProg/sLerpDuration*2*Mathf.Acos(0))+1)/2));
                mWaterRefract.transform.localPosition = v;

                if(mWaterMode == WaterMode.turbulent && mEmptyAnimProg == 45) // Lancer le rallumage du spot à la moitié si remplissage
				{
					if(mSpa.GetComponent<Function_SpaConfigLight>()!=null)
                    	mSpa.GetComponent<Function_SpaConfigLight>().IncreaseMainSpotlight();
				}

                mEmptyAnimProg++;
            }
            else
                mEmptyAnim = false;     // fin de l'animation
        }

        if(mCalmAnim)   // Animation eau agitée -> au calme (et inversement si besoin)
        {
            if(mCalmAnimProg < sShortLerpDuration)
            {
                float tmp = Mathf.Lerp(mCalmAnimBlendStart, mCalmAnimBlendEnd,
                                                  ((-Mathf.Cos(mCalmAnimProg/sShortLerpDuration*2*Mathf.Acos(0))+1)/2));
                mWater.GetComponent<Renderer>().material.SetFloat("_Blend", tmp);
                tmp = Mathf.Lerp(mCalmAnimGlobAlphaStart, mCalmAnimGlobAlphaEnd,
                                                  ((-Mathf.Cos(mCalmAnimProg/sShortLerpDuration*2*Mathf.Acos(0))+1)/2));
                mWater.GetComponent<Renderer>().material.SetFloat("_GlobAlpha", tmp);
                tmp = Mathf.Lerp(mCalmAnimMinAlphaStart, mCalmAnimMinAlphaEnd,
                                                  ((-Mathf.Cos(mCalmAnimProg/sShortLerpDuration*2*Mathf.Acos(0))+1)/2));
                mWater.GetComponent<Renderer>().material.SetFloat("_MinAlpha", tmp);
                tmp = Mathf.Lerp(mCalmAnimMinCoefStart, mCalmAnimMinCoefEnd,
                                                  ((-Mathf.Cos(mCalmAnimProg/sShortLerpDuration*2*Mathf.Acos(0))+1)/2));
				if(mWater.GetComponent<Renderer>().material.HasProperty("_MinCoef"))
                	mWater.GetComponent<Renderer>().material.SetFloat("_MinCoef", tmp);
                tmp = Mathf.Lerp(mCalmAnimAmountStart, mCalmAnimAmountEnd,
                                                  ((-Mathf.Cos(mCalmAnimProg/sShortLerpDuration*2*Mathf.Acos(0))+1)/2));
                mWater.GetComponent<Renderer>().material.SetFloat("_Amount", tmp);
                tmp = Mathf.Lerp(mCalmAnimBmpAmtStart, mCalmAnimBmpAmtEnd,
                                                  ((-Mathf.Cos(mCalmAnimProg/sShortLerpDuration*2*Mathf.Acos(0))+1)/2));
                mWaterRefract.GetComponent<Renderer>().material.SetFloat("_BumpAmt", tmp);

                mCalmAnimProg++;
            }
            else
            {
                mCalmAnim = false;

                // -- Changement de texture à la fin de la transition --
                AnimatedTexture_decal_switch[] animTexs;
                animTexs = mWater.GetComponents<AnimatedTexture_decal_switch>();
                for(int i=0; i<animTexs.Length; i++)
                    animTexs[i].SetAlternativeImages(true);

                animTexs = mWaterRefract.GetComponents<AnimatedTexture_decal_switch>();
                for(int i=0; i<animTexs.Length; i++)
                    animTexs[i].SetAlternativeImages(true);
            }
        }

    } // FixedUpdate()

    //-----------------------------------------------------
    void OnGUI()
    {
    }

    //-----------------------------------------------------
    void LoopWaterMode()
    {
        if(mEmptyAnim) return;

        mWaterMode = (WaterMode) (((int)mWaterMode+1) % (Enum.GetNames(typeof(WaterMode)).Length));

        if(mWaterMode == WaterMode.empty || mWaterMode == WaterMode.turbulent)
        {
            mEmptyAnim = true;
            mEmptyAnimProg = 0f;
            mEmptyAnimStart     = (mWaterMode == WaterMode.empty) ? mWaterMeshFilledY : mWaterMeshEmptyY;
            mEmptyAnimEnd       = (mWaterMode == WaterMode.empty) ? mWaterMeshEmptyY  : mWaterMeshFilledY;
            mEmptyAnimRefStart  = (mWaterMode == WaterMode.empty) ? mWaterRefMeshFilledY : mWaterMeshEmptyY;
            mEmptyAnimRefEnd    = (mWaterMode == WaterMode.empty) ? mWaterMeshEmptyY  : mWaterRefMeshFilledY;

            if(mWaterMode == WaterMode.empty)       // Lancer l'extinction du spot principal si vidange
			{
				if(mSpa.GetComponent<Function_SpaConfigLight>()!=null)
					mSpa.GetComponent<Function_SpaConfigLight>().DecreaseMainSpotlight();
			}
        }

        if(mWaterMode != WaterMode.empty)
            SetWaterMode(mWaterMode == WaterMode.calm);

    } // LoopWaterMode()

    //-----------------------------------------------------
    void SetWaterMode(bool calmWater)
    {
		if(usefullData.lowTechnologie)
			return;
        // -- Mise à jour du shader de l'eau --
        if(calmWater)
        {
            // -- Shader DispNormRimSpec --
//            mWater.renderer.material.SetFloat("_Blend", sWaterMat_M2_TCblend);      // Blend Texture/Couleur
//            mWater.renderer.material.SetFloat("_GlobAlpha", sWaterMat_M2_Alpha);    // Alpha global
//            mWater.renderer.material.SetFloat("_MinAlpha", sWaterMat_M2_MinAlpha);  // Minimum Alpha
//            mWater.renderer.material.SetFloat("_MinCoef", sWaterMat_M2_MinCoef);    // Min Coef (displacement)
//            mWater.renderer.material.SetFloat("_Amount", sWaterMat_M2_DispAmt);     // Amount (displacement)
            // -- Shader Refractive glass --
//            mWaterRefract.renderer.material.SetFloat("_BumpAmt", sWaterRefMat_M2_Refract);  // Force de la réfraction

            // -- Paramétrage pour l'animation : shader DispNormRimSpec --
            mCalmAnimBlendStart     = mWater.GetComponent<Renderer>().material.GetFloat("_Blend");
            mCalmAnimBlendEnd       = sWaterMat_M2_TCblend;
            mCalmAnimGlobAlphaStart = mWater.GetComponent<Renderer>().material.GetFloat("_GlobAlpha");
            mCalmAnimGlobAlphaEnd   = sWaterMat_M2_Alpha;
            mCalmAnimMinAlphaStart  = mWater.GetComponent<Renderer>().material.GetFloat("_MinAlpha");
            mCalmAnimMinAlphaEnd    = sWaterMat_M2_MinAlpha;
			if(mWater.GetComponent<Renderer>().material.HasProperty("_MinCoef"))
          	  mCalmAnimMinCoefStart   = mWater.GetComponent<Renderer>().material.GetFloat("_MinCoef");
            mCalmAnimMinCoefEnd     = sWaterMat_M2_MinCoef;
            mCalmAnimAmountStart    = mWater.GetComponent<Renderer>().material.GetFloat("_Amount");
            mCalmAnimAmountEnd      = sWaterMat_M2_DispAmt;
            mCalmAnimBmpAmtStart    = mWaterRefract.GetComponent<Renderer>().material.GetFloat("_BumpAmt");
            mCalmAnimBmpAmtEnd      = sWaterRefMat_M2_Refract;
            mCalmAnim = true;
            mCalmAnimProg = 0f;
        } // Mode eau calme
        else
        {
            // -- Shader DispNormRimSpec --
            mWater.GetComponent<Renderer>().material.SetFloat("_Blend", sWaterMat_M1_TCblend);      // Blend Texture/Couleur
            mWater.GetComponent<Renderer>().material.SetFloat("_GlobAlpha", sWaterMat_M1_Alpha);    // Alpha global
            mWater.GetComponent<Renderer>().material.SetFloat("_MinAlpha", sWaterMat_M1_MinAlpha);  // Minimum Alpha
			if(mWater.GetComponent<Renderer>().material.HasProperty("_MinCoef"))
            	mWater.GetComponent<Renderer>().material.SetFloat("_MinCoef", sWaterMat_M1_MinCoef);    // Min Coef (displacement)
            mWater.GetComponent<Renderer>().material.SetFloat("_Amount", sWaterMat_M1_DispAmt);     // Amount (displacement)

            AnimatedTexture_decal_switch[] animTexs;
            animTexs = mWater.GetComponents<AnimatedTexture_decal_switch>();
            for(int i=0; i<animTexs.Length; i++)
            {
                animTexs[i].SetAlternativeImages(false);
            }

            // -- Shader Refractive glass --
            mWaterRefract.GetComponent<Renderer>().material.SetFloat("_BumpAmt", sWaterRefMat_M1_Refract);  // Force de la réfraction

            animTexs = mWaterRefract.GetComponents<AnimatedTexture_decal_switch>();
            for(int i=0; i<animTexs.Length; i++)
            {
                animTexs[i].SetAlternativeImages(false);
            }

        } // Mode eau agitée
		
    } // SetWaterMode()

    //-----------------------------------------------------
    // GUI OS3D (implémentation de Function_OS3D)
    public void DoAction ()             // au clic du bouton
    {
     LoopWaterMode();
    }

    public string GetFunctionParameterName()     // Label du bouton
    {
        if(mWaterMode == WaterMode.calm)
        {
            return TextManager.GetText(_spaWaterEmpty);
        }
        else if(mWaterMode == WaterMode.empty)
        {
            return TextManager.GetText(_spaWaterBubbling);
        }
	     else // if(mWaterMode == WaterMode.turbulent), else
	     {
	         return TextManager.GetText(_spaWaterCalm);
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
	
//    //-----------------------------------------------------
//    void SetWaterMode(bool calmWater)
//    {
//        AnimatedTexture_decal[] animTexs;
//
//        // -- Mise à jour du shader de l'eau --
//        if(calmWater)
//        {
//            // -- Shader DispNormRimSpec --
//            mWater.renderer.material.SetFloat("_Blend", sWaterMat_M2_TCblend);      // Blend Texture/Couleur
//            mWater.renderer.material.SetFloat("_GlobAlpha", sWaterMat_M2_Alpha);    // Alpha global
//            mWater.renderer.material.SetFloat("_MinAlpha", sWaterMat_M2_MinAlpha);  // Minimum Alpha
//
//            mWater.AddComponent(typeof(AnimatedTexture_decal));             // Création nouveau script
//            animTexs = mWater.GetComponents<AnimatedTexture_decal>();
//            for(int i=0; i<animTexs.Length; i++)
//            {
//                if(animTexs[i].nameImgList == "")
//                {
//                    animTexs[i].nameImgList = sWaterMat_M2_NMapPath;        // Configuration nouveau script
//                    animTexs[i].nameShader  = sWaterDNRMshaderName;
//                    animTexs[i].nameTexture = "_BumpMap";
//                }
//                else
//                {
//                    animTexs[i].enabled = false;
//                    Destroy(animTexs[i]);                                   // Destruction anciens scripts
//                }
//            }
//
//            // -- Shader Refractive glass --
//            mWaterRefract.renderer.material.SetFloat("_BumpAmt", sWaterRefMat_M2_Refract);  // Force de la réfraction
//
//            mWaterRefract.AddComponent<AnimatedTexture_decal>();            // Création nouveau script
//            animTexs = mWaterRefract.GetComponents<AnimatedTexture_decal>();
//            for(int i=0; i<animTexs.Length; i++)
//            {
//                if(animTexs[i].nameImgList == "")
//                {
//                    animTexs[i].nameImgList = sWaterMat_M2_NMapPath;        // Configuration nouveau script
//                    animTexs[i].nameShader  = sWaterRefshaderName;
//                    animTexs[i].nameTexture = "_BumpMap";
//                }
//                else
//                {
//                    animTexs[i].enabled = false;
//                    Destroy(animTexs[i]);                                   // Destruction anciens scripts
//                }
//            }
//
//        } // Mode eau calme
//        else
//        {
//            // -- Shader DispNormRimSpec --
//            mWater.renderer.material.SetFloat("_Blend", sWaterMat_M1_TCblend);      // Blend Texture/Couleur
//            mWater.renderer.material.SetFloat("_GlobAlpha", sWaterMat_M1_Alpha);    // Alpha global
//            mWater.renderer.material.SetFloat("_MinAlpha", sWaterMat_M1_MinAlpha);  // Minimum Alpha
//
//
//            mWater.AddComponent<AnimatedTexture_decal>();                   // Création nouveau script 1 (Bump)
//            mWater.AddComponent<AnimatedTexture_decal>();                   // Création nouveau script 2 (RGB)
//            animTexs = mWater.GetComponents<AnimatedTexture_decal>();
//            int j=0;
//            for(int i=0; i<animTexs.Length; i++)
//            {
//                if(animTexs[i].nameImgList == "")
//                {
//                    if(j==0)
//                    {
//                        animTexs[i].nameImgList = sWaterMat_M1_NMapPath;    // Configuration script 1 (Bump)
//                        animTexs[i].nameShader  = sWaterDNRMshaderName;
//                        animTexs[i].nameTexture = "_BumpMap";
//                    }
//                    else
//                    {
//                        animTexs[i].nameImgList = sWaterMat_M1_RGBMapPath;   // Configuration script 2 (RGB)
//                        animTexs[i].nameShader  = sWaterDNRMshaderName;
//                        animTexs[i].nameTexture = "_MainTex";
//                    }
//                    j++;
//                }
//                else
//                {
//                    animTexs[i].enabled = false;                            // Destruction anciens scripts
//                    Destroy(animTexs[i]);
//                }
//            }
//
//            // -- Shader Refractive glass --
//            mWaterRefract.renderer.material.SetFloat("_BumpAmt", sWaterRefMat_M1_Refract);  // Force de la réfraction
//
//            mWaterRefract.AddComponent<AnimatedTexture_decal>();            // Création nouveau script
//            animTexs = mWaterRefract.GetComponents<AnimatedTexture_decal>();
//            for(int i=0; i<animTexs.Length; i++)
//            {
//                if(animTexs[i].nameImgList == "")
//                {
//                    animTexs[i].nameImgList = sWaterMat_M1_NMapPath;        // Configuration nouveau script
//                    animTexs[i].nameShader  = sWaterRefshaderName;
//                    animTexs[i].nameTexture = "_BumpMap";
//                }
//                else
//                {
//                    animTexs[i].enabled = false;
//                    Destroy(animTexs[i]);                                   // Destruction anciens scripts
//                }
//            }
//
//
//        } // Mode eau agitée
//
//    } // SetWaterMode


} // class spaConfig
