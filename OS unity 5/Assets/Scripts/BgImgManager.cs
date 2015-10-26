using UnityEngine;
using System.Collections;
using Pointcube.Global;
using Pointcube.Utils;


public class BgImgManager : MonoBehaviour
{
#region attributs
    // -- Références scène --
    public  GameObject m_mainCam;           // Pour gérer le Av/Après
    public  GameObject m_grassNode;             // Pour gérer multi-res du grassV2
    public  GameObject m_PlaneReflect;       // Pour màj du plan de réflexions
    private GameObject m_bgImg;

    // -- Debug --
    private static readonly string DEBUGTAG = "BgImgManager : ";
#endregion

#region unity_func
    //-----------------------------------------------------
    void Awake() // Fonction de multi-res appelée par MainCamManager
    {
//        UsefullEvents.OnResizingWindow  += FitViewportToScreen;
//        UsefullEvents.OnResizeWindowEnd += FitViewportToScreen;
    }

	//-----------------------------------------------------
	void Start () 
	{
        if(m_mainCam == null)     Debug.LogError(DEBUGTAG+"Main Camera"   +PC.MISSING_REF);
        if(m_grassNode == null)   Debug.LogError(DEBUGTAG+"GrassV2 Node"  +PC.MISSING_REF);
        if(m_PlaneReflect == null) Debug.LogError(DEBUGTAG+"Plane reflect" +PC.MISSING_REF);

        m_bgImg = gameObject;
        if(m_bgImg.name != "backgroundImage") Debug.LogError(DEBUGTAG+PC.WRONG_OBJ+"backgroundImage");
	}
	
	//-----------------------------------------------------
	void Update ()
    { }

    //-----------------------------------------------------
	void OnGUI()
    { }

    //-----------------------------------------------------
    void OnDestroy() // Fonction de multi-res appelée par MainCamManager
    {
//        UsefullEvents.OnResizingWindow  -= FitViewportToScreen;
//        UsefullEvents.OnResizeWindowEnd -= FitViewportToScreen;
    }
#endregion

#region multi_res
    //-----------------------------------------------------
    public void FitBgImgToScreen()  // Fonction de multi-res appelée par MainCamManager
    {
        if(m_bgImg.GetComponent<GUITexture>().texture != null)
        {
            // -- Image de fond --
            Rect r = RescaleBgImg();

            // -- Màj Avant/Après --
            m_mainCam.GetComponent<GuiTextureClip>().UpdatePhotoTexRect(r.x,r.y,r.width,r.height);
            
            // -- Màj gazonV2 --
            m_grassNode.GetComponent<GrassV2>().FitGuiTextureToScreen();
        }
    } // FitViewportToScreen()

    //-----------------------------------------------------
    public Rect RescaleBgImg(float fitWidth, float fitHeight)
    {
        Texture tex = m_bgImg.GetComponent<GUITexture>().texture;
        Rect r = ImgUtils.ResizeImagePreservingRatio(tex.width, tex.height, fitWidth, fitHeight);
        m_bgImg.GetComponent<GUITexture>().pixelInset = r;
        return r;
    }
    //-----------------------------------------------------
    public Rect RescaleBgImg()
    {
        return RescaleBgImg(Screen.width, Screen.height);
    }
#endregion

//    //-----------------------------------------------------
//	public bool ratioHD(float W,float H)
//	{		
//		float _width = m_bgImg.guiTexture.texture.width;
//		float _height = m_bgImg.guiTexture.texture.height;
//		
//		float _x = 0;
//		float _y = 0;
//		
//		if (_width>=_height)
//		{
//			float _factor = W/_width;
//			_height *= _factor;
//			_width = W;				
//			_y = (H - _height)/2;
//		}
//		else if (_height>=_width)
//		{
//			float _factor = H/_height;
//			_width *= _factor;
//			_height = H;
//			_x = (W - _width)/2;
//		}
//		m_bgImg.guiTexture.pixelInset = new Rect(_x, _y, _width, _height);
//		
//		//ANTI rendu 3d dans les "Bandes Noires"
////		Camera.mainCamera.pixelRect = new Rect ((W/2)-(_width/2),
////		                                        (H/2)-(_height/2),
////		                                        _width,_height);
//		return true;
//	}

#region get_set

    //-----------------------------------------------------
    public void ReinitBackgroundTex()
    {
        if(m_bgImg.GetComponent<GUITexture>().texture != null)
            SetBackgroundTexture(Montage.sm.getBackground());
    }

    //-----------------------------------------------------
    public void SetBackgroundTexture(Texture2D t)
    {
        if(t != null)
        {
            m_bgImg.GetComponent<GUITexture>().texture = t;
            m_PlaneReflect.GetComponent<Renderer>().material.mainTexture = t;

            m_mainCam.GetComponent<GuiTextureClip>().SetTexture((Texture) t);

            FitBgImgToScreen();
        }
        else
            Debug.LogError(DEBUGTAG+"cannot set null texture !");

        m_mainCam.GetComponent<LineHorizon>().UpdateScreenValues();
//        m_mainCam.GetComponent<MainCamManager>().UpdateBgCamRenderTex();
    } // SetBackgroundTexture()
	
	public float GetYOffset()
	{
		return m_bgImg.GetComponent<GUITexture>().pixelInset.y;
	}

#endregion

} // class BgImgManager
