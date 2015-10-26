using UnityEngine;
using System.Collections.Generic;

using Pointcube.Global;


public class MainCamManager : MonoBehaviour
{
    // -- RÃ©f. scÃ¨ne --
    public GameObject       m_backgroundImg;
    public GameObject       m_backGrid;
    public GameObject       m_eraserNode;
    public GameObject       m_grassNode;
    public GameObject       m_bgCam;
    public Material         m_bgMat;
    public Material         m_eraserMat;
	public Rect				m_rect;
	public Rect				m_rect2;
	
	
    // -- Rendu de la cam background --
    private RenderTexture   m_bgRenderTex;

    private bool            m_visible;

    private static readonly string DEBUGTAG = "MainCamManager : ";
	
#region unity_func
    //-----------------------------------------------------
    void Awake()
    {
        UsefullEvents.OnResizingWindow  += FitViewportToScreen;
        UsefullEvents.OnResizeWindowEnd += FitViewportToScreen;
    }
    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= FitViewportToScreen;
        UsefullEvents.OnResizeWindowEnd -= FitViewportToScreen;
    }
    //-----------------------------------------------------
	void Start()
    {
        if(m_backgroundImg == null) Debug.LogError(DEBUGTAG+"Background Image"   +PC.MISSING_REF);
        if(m_backGrid == null)      Debug.LogError(DEBUGTAG+"Back grid"          +PC.MISSING_REF);
        if(m_eraserNode == null)    Debug.LogError(DEBUGTAG+"EraserImages"       +PC.MISSING_REF);
        if(m_grassNode == null)     Debug.LogError(DEBUGTAG+"Grass skybox"       +PC.MISSING_REF);
        if(m_bgCam == null)         Debug.LogError(DEBUGTAG+"Background camera"  +PC.MISSING_REF);
        if(m_eraserMat == null)     Debug.LogError(DEBUGTAG+"Eraser material"    +PC.MISSING_REF);
        if(m_bgMat == null)         Debug.LogError(DEBUGTAG+"Background material"+PC.MISSING_REF);

        m_bgRenderTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        m_bgMat.SetTexture("_BgTex", m_bgRenderTex);

        m_eraserMat.SetTexture("_BgTex", m_bgRenderTex);
        m_eraserMat.SetFloat("_OpenGL", usefullData.s_openGL ? 1f : 0f);

        m_visible = false;
    }
    //-----------------------------------------------------
	void Update()
    { 
		m_rect2 = GetComponent<Camera>().pixelRect;
//		camera.pixelRect = m_rect;
	}
    //-----------------------------------------------------
//    void OnGUI()
//    {
//        if(GUI.Button(new Rect(50f, 50f, 50f, 50f), "Gommer"))
//        {
//            m_backgroundImg.AddComponent("PolygonTracer");
//            m_backgroundImg.GetComponent<PolygonTracer>().m_backgroundImg = m_backgroundImg;
//        }
//
//        if(GUI.Button(new Rect(100f, 50f, 50f, 50f), "Gazonner"))
//        {
//            GameObject.Find("backgroundGrass").AddComponent("PolygonTracer");
//            GameObject.Find("backgroundGrass").GetComponent<PolygonTracer>().SetTool(GUIEditTools.EditTool.Grass);
//            GameObject.Find("backgroundGrass").GetComponent<PolygonTracer>().m_backgroundImg = m_backgroundImg;
//            GameObject.Find("backgroundGrass").GetComponent<PolygonTracer>().m_grassBg = m_grassNode;
//        }
//    }
	
	
#if !UNITY_ANDROID
    //-----------------------------------------------------
    void OnPreRender()
    {
//		Debug.Log ("OnRender : "+camera.pixelRect);
#if UNITY_IPHONE
		if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if(UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPad2Gen)
			{
				if(m_visible)
		        {
		            // -- MÃ j renderTexture bgCam -> mainCam
		            UpdateBgCamRenderTex();                // TODO appeler Ã§a que quand nÃ©cessaire pour optimiser (!)
		            Graphics.Blit(m_bgRenderTex, m_bgMat); // application du background
		        }
			}
		}
#else
		if(m_visible)
		{
			// -- MÃ j renderTexture bgCam -> mainCam
			UpdateBgCamRenderTex();                // TODO appeler Ã§a que quand nÃ©cessaire pour optimiser (!)
			Graphics.Blit(m_bgRenderTex, m_bgMat); // application du background
		}		
#endif
    }
    //-----------------------------------------------------
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {	
#if UNITY_IPHONE
		if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if(UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPad2Gen)
			{
				if(m_visible)
		        {
			    	Graphics.Blit(src, dst, m_eraserMat);       // "gommage"
		        }
			}
		}
#else
		if(m_visible)
		{
			Graphics.Blit(src, dst, m_eraserMat);       // "gommage"
		}
#endif
	}	
#endif

#endregion

#region multi_res
    //-----------------------------------------------------
    // Cette fonction doit appeller toutes les fonctions de multi-res qui utilisent les donnÃ©es
    // de backgroundImage (pour s'assurer qu'elle est bien Ã  jour)
    public void FitViewportToScreen()
    {
//        Debug.Log("Fitting background to screen");
        if(/*!m_visible || */m_backgroundImg.GetComponent<GUITexture>().texture == null) { m_visible = false; return; }

//        Debug.Log("m_backgroundImg.GetComponent<BgImgManager>().FitBgImgToScreen();");
        // -- MÃ€J de l'image de fond --
        m_backgroundImg.GetComponent<BgImgManager>().FitBgImgToScreen();
        m_rect = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
		
        // -- Redimensionner la RenderTexture de fond --
        m_bgRenderTex.Release();
        m_bgRenderTex.width  = Screen.width;
        m_bgRenderTex.height = Screen.height;

        // -- MÃ j mainCam : ANTI rendu 3d dans les "Bandes Noires" --
        GetComponent<Camera>().pixelRect = m_rect;
		m_rect = GetComponent<Camera>().pixelRect;
		
		if(!usefullData.lowTechnologie)
		{
	        // -- MÃ j EraserV2 --
	        m_eraserNode.GetComponent<EraserV2>().ResizeMask();
		}
    }
#endregion

    //----------------------------------------------------
    public void UpdateBgCamRenderTex()
    {
        m_bgCam.GetComponent<Camera>().targetTexture = m_bgRenderTex;
        m_bgCam.GetComponent<Camera>().Render();
        m_bgCam.GetComponent<Camera>().targetTexture = null;

        Rect r = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
        m_bgMat.SetVector("_BgBounds", new Vector4(r.xMin/Screen.width, r.xMax/Screen.width,
                                                       r.yMin/Screen.height, r.yMax/Screen.height));

        m_eraserMat.SetVector("_BgBounds", new Vector4(r.xMin/Screen.width, r.xMax/Screen.width,
                                                       r.yMin/Screen.height, r.yMax/Screen.height));
    }
    //----------------------------------------------------
    public void SetVisible(bool visible)
    {
		if(!usefullData.lowTechnologie)
		{
        	m_visible = visible;
			if(m_visible)
				FitViewportToScreen();
		}
    }
    //-----------------------------------------------------
    public RenderTexture GetBgRenderTex()
    {
        return m_bgRenderTex;
    }
    //-----------------------------------------------------
	public RenderTexture GetBgOnly(RenderTexture rt)
	{
	 	UpdateBgCamRenderTex();                // TODO appeler Ã§a que quand nÃ©cessaire pour optimiser (!)
	 	Graphics.Blit(m_bgRenderTex,rt, m_bgMat);
		
        return rt;
	}
    //-----------------------------------------------------
    public RenderTexture RenderSingle(RenderTexture rt, bool erase = true,
                                                                     int cullingMask = int.MaxValue)
    {
        return RenderSingle(rt, GetComponent<Camera>().pixelRect, erase, cullingMask);
    }
    //-----------------------------------------------------
    public RenderTexture RenderSingle(RenderTexture rt, Rect rect, bool erase = true,
                                                                     int cullingMask = int.MaxValue)
    {
        int  backupMask  = GetComponent<Camera>().cullingMask;
        Rect backupRect  = GetComponent<Camera>().pixelRect;
        if(!erase)
            this.enabled = false;
        if(cullingMask != int.MaxValue)
            GetComponent<Camera>().cullingMask = cullingMask;
        GetComponent<Camera>().rect = rect;
        GetComponent<Camera>().targetTexture = rt;
        GetComponent<Camera>().Render();
        GetComponent<Camera>().targetTexture = null;
        GetComponent<Camera>().pixelRect = backupRect;
        GetComponent<Camera>().cullingMask = backupMask;
        this.enabled = true;
		
        return rt;
    }

} // class MainCamManager
