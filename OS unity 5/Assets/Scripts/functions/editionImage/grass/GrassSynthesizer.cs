//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/grass/GrassSynthesizer.cs - 05/2012 KS
// Attaché dynamiquement à Background/backgroundImage par GUISubTools

using UnityEngine;
using System.Collections;
using Pointcube.Global;

//-----------------------------------------------------------------------------
// Classe qui permet de capturer une zone de 25x25 pixels de l'image de fond
// (la photo). Après la capture, elle appelle TextureSynthesizer pour générer
// une texture à partir de l'échantillon capturé.
public class GrassSynthesizer : MouseTool
{
    // -- Références scène --
    public  GameObject        m_mainCam;
//    public  static readonly GameObject m_mainScene = GameObject.Find("MainScene");


    private Texture2D      m_captureCursor = (Texture2D) Resources.Load("grass/curseur_capture"); // Curseur de capture
    // Texture utilisée pour stocker l'échantillon d'image capturé
//    public Texture2D       m_sampleTex;
    private static Texture2D  s_synthTex;         // Texture synthétisée

//    private int            m_camMaskBk;        // Pour sauvegarde du culling mask de la mainCam
    private bool           m_captureLaunched;

    private static readonly int  c_sampleSize = 25;
    private static readonly string DEBUGTAG = "GrassSynthesizer";
    //-----------------------------------------------------
	void Start()
    {
        if(m_mainCam   == null) Debug.LogError(DEBUGTAG+"Main camera"+PC.MISSING_REF);
//        if(m_mainScene == null) Debug.LogError(DEBUGTAG+"Main scene" +PC.MISSING_REF);

        base.StartMouseTool();
        GameObject.Find("MainScene").GetComponent<GUIMenuMain>().SetHideAll(true);

//        m_sampleTex = new Texture2D(c_sampleSize, c_sampleSize, TextureFormat.RGB24, false);

//        m_camMaskBk = GameObject.Find("mainCam").transform.camera.cullingMask;
//        GameObject.Find("mainCam").transform.camera.cullingMask = 0;

        m_captureLaunched = false;
    }

    //-----------------------------------------------------
	void Update()
    {
        base.UpdateMouseTool();

        if(PC.In.Click1Up())
        {
            if(m_ignoreClick)        // Ignorer le relâchement d'un clic commencé avant l'activation
                { m_ignoreClick = false; return; }

            m_captureLaunched = true;
            GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
        }
        if(m_captureLaunched && GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().IsDisplayingIcon())
        {
            //-- Capture de l'échantillon sur l'image de fond --
            float[] data = base.GetZoneCornerFromCursor(c_sampleSize);
            int xOffset = (int) m_backgroundImg.GetComponent<GUITexture>().pixelInset.x;

            Texture2D sampleTex = new Texture2D(c_sampleSize, c_sampleSize, TextureFormat.RGB24, false);

            Color bkCol = m_backgroundImg.GetComponent<GUITexture>().color;     // Utile si mode nuit actif
            m_backgroundImg.GetComponent<GUITexture>().color = Color.gray;
            Camera.main.GetComponent<MainCamManager>().UpdateBgCamRenderTex();
            RenderTexture rt = Camera.main.GetComponent<MainCamManager>().GetBgRenderTex();
            m_backgroundImg.GetComponent<GUITexture>().color = bkCol;
            
            RenderTexture.active = rt;
            sampleTex.ReadPixels(new Rect((int)data[0]+xOffset, (int)data[1], c_sampleSize, c_sampleSize), 0, 0);
            RenderTexture.active = null;

            sampleTex.Apply();
            
			if(GameObject.Find("MainScene").GetComponent<GUISubTools>().isActive())
			{
				GameObject.Find("MainScene").GetComponent<GUISubTools>().setDisplayChoicePanel(false);
			}
			
            Destroy(this);
        }
    } // Update()

    //-----------------------------------------------------
    void OnGUI()
    {
//        if(mx >=)
        Drawer.DrawIcon(new Vector2(m_mousePosX, m_mousePosY), m_captureCursor);
    }
	
	void OnDisable()
	{
        Resources.UnloadUnusedAssets();
    }

    //-----------------------------------------------------
    public static void SynthesizeFromBaseTex(Texture2D baseTex)
    {

        image i = new image(c_sampleSize, c_sampleSize);
        i.setPixels(baseTex);

        // -- Synthétisation de la texture à partir de l'échantillon --
        TextureSynthesizer ts = new TextureSynthesizer(128, 128);
        i = ts.synthesize(i);
        s_synthTex = i.getTextureRGB();
        s_synthTex.Apply();
//            byte[] bytes = m_synthTex.EncodeToPNG();   // Sauvegarde en fichier
//            System.IO.File.WriteAllBytes(Application.dataPath+"/Resources/grass/synth/grassSynth0.png", bytes);

        // -- Enregistrer la texture et l'appliquer aux plans de gazon --
        GameObject.Find("grassSkybox").GetComponent<GrassHandler>().AddSynthTex(s_synthTex,
                                                                                     baseTex, true);
        // -- Modifier l'interface (devrait être fait dans le contrôleur) --
//          GameObject.Find("MainScene").GetComponent<GUIMenuMain>().SetHideAll(false);
//        GameObject.Find("mainCam").transform.camera.cullingMask = m_camMaskBk;
        GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
    }
} // class GrassSynthesizer
