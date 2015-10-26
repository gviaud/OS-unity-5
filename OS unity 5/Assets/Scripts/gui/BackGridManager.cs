using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Pointcube.Global;

public class BackGridManager : MonoBehaviour
{
    private enum BgridMode {loading, mainmenu, selector, montage};

    // -- Textures --
    private  Texture     mLoadingBgTex;          // Fond au chargement (chargé automatiquement selon la res. de l'écran)
    private  Texture     mMenuBgTex;             // Fond du mainmenu (idem)

    public   Texture     mSelectorBgTex;         // Fond menu de sélection de photo (assigné dans l'inspecteur)
    public   Texture     mMontageBgTex;          // Fond pendant montage, stretchable avec le 9-sliced (idem)
    
    public bool buseStandardBgs = true;
	public List<Texture2D> listBgsStandard = new List<Texture2D> (); 
	public List<Texture2D> listBgs = new List<Texture2D> ();

    // -- Mode d'affichage de la guiTexture --
    private  BgridMode   mMode;

    // -- Variables mode montage --
    public   int         mMontageTexBorders;     // Bordure non stretchable de la texture (9-cut truc)
    public   Color       mMontageTexTint;
    private  bool        mNeedRescale;

    // -- Références objets scène --
    public   Transform   mBackgroundImage;       // Référence vers le GameObject qui affiche la photo
    private  GUITexture  mLogoGUItex;


    // -- Noms des fichiers d'image /!\ A TENIR A JOUR SI RENOMMAGE /!\ --
    public string        sBgMissingFilename      = "missing";
    public string        mLoadingBgFilename      = "OS_SPLASH_2";
    public string        mMainmenuBgFilename     = "OS_SPLASH_4";
//  public             sBg0Name                = "OS_SPLASH_0"; // Splash à mettre à la main dans les buildSettings
    public   string      m_logoFullPath          = "gui/backgrounds/fixed/OS3D_logo_512_glow";
    public   string      m_logoExpressPath       = "gui/backgrounds/fixed/OS3D_EXPRESS_LOGO";
    
	public GUITexture crossFadeGuiTexture;

    // Liste des ratios supportés (utilisée pour splash screens) /!\ CLASSÉE PAR ORDRE CROISSANT /!\
    private static readonly float[]     sSupportedRatios       = {4/3f, 16/10f /*, 16/9f*/};
    private static readonly string[]    sSupportedRatioFolders = {"43", "1610" /*, "169"*/}; // Nom des dossiers des splashs par ratio

    private static readonly string      DEBUGTAG = "BackGridManager : ";
    private static readonly bool        DEBUG    = true;

#region unity_func
    //-----------------------------------------------------
    void Awake()
    {
        mNeedRescale = false;

        UsefullEvents.OnResizingWindow += SetNeedRescale;
        UsefullEvents.OnResizeWindowEnd += SetNeedRescale;
    }

    //-----------------------------------------------------
    void Start()
    {
        // -- GUITexture du logo (ratio fixe) --
        string logoPath = (usefullData._edition == usefullData.OSedition.Lite? m_logoExpressPath: m_logoFullPath);
        Texture logoTex = (Texture2D) Resources.Load(logoPath, typeof(Texture2D));
        if(logoTex == null)
            Debug.LogError(DEBUGTAG+logoPath+" "+PC.MISSING_RES);
        
		if(transform.childCount > 0)
		{
	        mLogoGUItex = transform.GetChild(0).transform.GetComponent<GUITexture>();
	        mLogoGUItex.GetComponent<GUITexture>().texture = logoTex;
        }
        
       /* if(mLogoGUItex == null || mLogoGUItex.guiTexture.texture == null)
        { 
            Debug.LogError(DEBUGTAG+"Logo GUItexture not found or texture not assigned."); this.enabled = false;
        }*/

        // -- Texture de fond pendant montage --
       /* if(mMontageBgTex == null)
            { Debug.LogError(DEBUGTAG+"Texture not found... Deactivating script."); this.enabled = false;}*/

        // -- Textures loading & menu : chargement selon la résolution de l'écran --
        mLoadingBgTex     = null;
        mMenuBgTex        = null;

        string    bgFormat = ChooseBestBgRatio(UnityEngine.Screen.width, UnityEngine.Screen.height);
        Object[]  bgObj    = Resources.LoadAll("gui/backgrounds/"+bgFormat, typeof(Texture));
        Texture   curTex;
        for(int i=0; i<bgObj.Length; i++)
        {
            curTex = (Texture) bgObj[i];
            //Debug.Log(DEBUGTAG+" found splash : "+curTex.name);
            if(curTex.name.Contains(mLoadingBgFilename))
                mLoadingBgTex = curTex;

            if(curTex.name.Contains(mMainmenuBgFilename))
                mMenuBgTex = curTex;

//            else if(PC.DEBUG && DEBUG)
//                Debug.LogWarning(DEBUGTAG+"Found unknown splash texture : "+curTex.name);
        }

        // -- Erreurs si images non trouvées --
       /* if(mLoadingBgTex == null)
        {
            Debug.LogError(DEBUGTAG+"Splash not found : "+mLoadingBgFilename+" for format \""+bgFormat+"\"");
            mLoadingBgTex = (Texture) Resources.Load("gui/backgrounds/"+sBgMissingFilename, typeof(Texture));
        }
        if(mMenuBgTex   == null)
        {
            Debug.LogError(DEBUGTAG+"Splash not found : "+mMainmenuBgFilename+" for format \""+bgFormat+"\"");
            mMenuBgTex = (Texture) Resources.Load("gui/backgrounds/"+sBgMissingFilename, typeof(Texture));
        }*/

        // -- Configuration initiale --
		
		SetLoadingMode();
    }
    
    void Update()
    {

		if(crossFadeGuiTexture != null)
    	{

			crossFadeGuiTexture.GetComponent<GUITexture>().border = GetComponent<GUITexture>().border;
			crossFadeGuiTexture.GetComponent<GUITexture>().pixelInset = GetComponent<GUITexture>().pixelInset;
			
			if(crossFadeGuiTexture.texture != null)
			{
				Color tempColor = crossFadeGuiTexture.GetComponent<GUITexture>().color;
				tempColor.a -= Time.deltaTime * 0.5f;
				crossFadeGuiTexture.GetComponent<GUITexture>().color = tempColor;

				if(crossFadeGuiTexture.color.a <= 0.01f)
				{
					crossFadeGuiTexture.GetComponent<GUITexture>().texture = null;
				}
			}
		}
    }

    //-----------------------------------------------------
    void OnGUI()
    {
        if(Event.current.type == EventType.Repaint)
        {
            if(mNeedRescale)
            {
                RescaleGuiTexture();
                mNeedRescale = false;
            }
        }
    }

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= SetNeedRescale;
        UsefullEvents.OnResizeWindowEnd -= SetNeedRescale;
    }
#endregion

#region multi_res
    //-----------------------------------------------------
    public void RescaleGuiTexture()
    {
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        if(mMode == BgridMode.montage)        // Scaler la texture avec les border pour entourer la photo
        {
            RectOffset bRect = GetComponent<GUITexture>().border;
            Rect       pRect = GetComponent<GUITexture>().pixelInset;

            bRect.left = bRect.right = bRect.top = bRect.bottom = mMontageTexBorders;

            pRect.width = mBackgroundImage.GetComponent<GUITexture>().pixelInset.width + (mMontageTexBorders*2);
            pRect.height = mBackgroundImage.GetComponent<GUITexture>().pixelInset.height + (mMontageTexBorders*2);
            pRect.x = mBackgroundImage.GetComponent<GUITexture>().pixelInset.x - mMontageTexBorders;
            pRect.y = mBackgroundImage.GetComponent<GUITexture>().pixelInset.y - mMontageTexBorders;

            GetComponent<GUITexture>().border = bRect;
            GetComponent<GUITexture>().pixelInset = pRect;
        } // Mode montage
        else                    // Faire correspondre la guiTexture à l'écran
        {
            Rect rP = GetComponent<GUITexture>().pixelInset;
            rP.x = rP.y = 0;
            rP.width = screenWidth;
            rP.height = screenHeight;
            GetComponent<GUITexture>().pixelInset = rP;

            RectOffset rO = GetComponent<GUITexture>().border;
            rO.bottom = rO.top = rO.left = rO.right = 0;
            GetComponent<GUITexture>().border = rO;

            // -- Redimensionnement du logo si besoin --
//            int minDim = (Screen.width < Screen.height)? Screen.width : Screen.height;
//            if(minDim < 256) minDim = 256;      // Taille minimale du logo
//
//            if(minDim < mLogoGUItex.texture.width)
//            {
//                pRect.width  = minDim;
//                pRect.height = minDim;
//            }

            // -- Replacement du logo --
            
			if(mLogoGUItex != null)
			{
	            Rect pRect = mLogoGUItex.pixelInset;
	            if(mMode == BgridMode.loading)
	            {
	                pRect.x = (Screen.width - pRect.width)/2;   // centré x
	                pRect.y = (Screen.height - pRect.height)/2; // centré y
	            }
	            else if(mMode == BgridMode.mainmenu)
	            {
	                if(Screen.width <= 1024) pRect.x = 0;       // à gauche
	                else                     pRect.x = (Screen.width / 4f) - 256; // 1024/4
	                pRect.y = (Screen.height - pRect.height)/2; // centré y
	            }
	            mLogoGUItex.pixelInset = pRect;
            }
        }
    } // RescaleGUItexture()
#endregion

    //-----------------------------------------------------
    // Choix du meilleur format de splash. Retourne le nom du dossier
    // dans lequel sont stockés les splashs du ratio choisi.
    private string ChooseBestBgRatio(int screenW, int screenH)
    {
        float screenRatio = (float)screenW/(float)screenH;
        for(int i=0; i<sSupportedRatios.Length; i++)
        {
            if(screenRatio == sSupportedRatios[i])
                return sSupportedRatioFolders[i];
            else if(screenRatio < sSupportedRatios[i])
            {
                if(i == 0)
                    return sSupportedRatioFolders[i];
                else
                {
                    float diffUp = sSupportedRatios[i] - screenRatio;
                    float diffDown = screenRatio - sSupportedRatios[i-1];
                    if(diffUp >= diffDown)
                        return sSupportedRatioFolders[i-1];
                    else
                        return sSupportedRatioFolders[i];
                }
            } // si le ratio de l'écran n'est pas égal au ratio
        } // pour chaque ratio supporté

        return sSupportedRatioFolders[sSupportedRatios.Length-1];

    } // ChooseBestBgRatio()


    //-----------------------------------------------------
    // Note : Paramètres pour correspondre à OnResizingWindow
    private void SetNeedRescale()
    {
        mNeedRescale = true;
    }

	private void setRandomBg()
	{
		if(buseStandardBgs && listBgsStandard.Count > 0)
		{
			GetComponent<GUITexture>().texture = listBgsStandard[Random.Range(0, listBgsStandard.Count)];
		}
		else if(listBgs.Count > 0)
		{
			GetComponent<GUITexture>().texture = listBgs[Random.Range(0, listBgs.Count)];
		}
	}
	
	public void allowRandomBg(bool _bcrossFade = false)
	{
		if(_bcrossFade)
		{
			crossFadeGuiTexture.GetComponent<GUITexture>().texture = GetComponent<GUITexture>().texture;
			setRandomBg();
			crossFadeGuiTexture.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		}
		else
		{
			setRandomBg();
		}
	}
	
	private void allowBg(string _szname)
	{
		foreach(Texture2D t2d in listBgs)
		{
			if(t2d.name == _szname)
			{
				GetComponent<GUITexture>().texture = t2d;
				return;
			}
		}
		
		foreach(Texture2D t2d in listBgsStandard)
		{
			if(t2d.name == _szname)
			{
				GetComponent<GUITexture>().texture = t2d;
				return;
			}
		}
		
		allowRandomBg();
	}
	
    //-----------------------------------------------------
    public void SetMenuMode()
    {
        GetComponent<GUITexture>().color = Color.gray;
        GetComponent<GUITexture>().pixelInset.Set(0, 0, Screen.width, Screen.height);
        //guiTexture.texture = mMenuBgTex;
        
		if(PlayerPrefs.HasKey("BG"))
		{
			allowBg(PlayerPrefs.GetString("BG"));
		}
		else
		{
			allowRandomBg(PlayerPrefs.HasKey("BGCross"));
			
			if(PlayerPrefs.HasKey("BGCross"))
			{
				PlayerPrefs.DeleteKey("BGCross");
			}
		}
			
        mLogoGUItex.enabled = true;
        mMode = BgridMode.mainmenu;
		Camera.main.GetComponent<Mode2D>().DestroyObjectCopy ();
        mNeedRescale = true;
        Debug.Log (DEBUGTAG+"MENU MODE");
    }

    //-----------------------------------------------------
    public void SetLoadingMode()
    {
        GetComponent<GUITexture>().color = Color.gray;
		
		if(PlayerPrefs.HasKey("BG"))
		{
			allowBg(PlayerPrefs.GetString("BG"));
		}
		else
		{
			allowRandomBg();
		}
		
		if(mLogoGUItex != null)
		{
        	mLogoGUItex.enabled = true;
        }
		else if(!PlayerPrefs.HasKey("BG"))
		{
		    PlayerPrefs.SetString("BG", GetComponent<GUITexture>().texture.name);
			PlayerPrefs.SetInt("BGCross", 0);
        }
        
        mMode = BgridMode.loading;
        RescaleGuiTexture();
       // Debug.Log (DEBUGTAG+"MENU MODE");
    }

    //-----------------------------------------------------
    public void SetMontageMode()
    {
		if(PlayerPrefs.HasKey("BG"))
		{
			PlayerPrefs.DeleteKey("BG");
		}
		
        GetComponent<GUITexture>().texture = mMontageBgTex;
        GetComponent<GUITexture>().color   = mMontageTexTint;
        mLogoGUItex.enabled = false;
        mMode = BgridMode.montage;
        RescaleGuiTexture();
       // Debug.Log (DEBUGTAG+"MONTAGE MODE");
       
		GameObject.Find("camPivot").GetComponent<SceneControl>().saveBkup();
    }

    //-----------------------------------------------------
    public void SetSelectorMode()
    {
        //guiTexture.texture = mSelectorBgTex;
        mLogoGUItex.enabled = false;
        mMode = BgridMode.selector;
        RescaleGuiTexture();
        //Debug.Log (DEBUGTAG+"SELECTOR MODE");
    }
    
} // BackGrid()
