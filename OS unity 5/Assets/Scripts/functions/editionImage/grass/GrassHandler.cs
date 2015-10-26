//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/grass/GrassHandler.cs - 05/2012 KS
// Attaché à MainScene/grassSkybox de base
// TODO : mettre à jour la gomme si on a gommé puis changé la matière derrière
// TODO : Sauvegarde des textures générées
using UnityEngine;
using System.Collections.Generic;
using Pointcube.Global;
using Pointcube.Utils;
using System;
public class GrassHandler : MonoBehaviour
{
    // -- Réf. scène --
    public  Camera          m_mainCam;
    public  GameObject      m_backgroundImg;
	public  GameObject      m_grassSky;
	
    public  RenderTexture   m_grassRenderTex;       // Debug

    // -- Textures de gazon --
    private UnityEngine.Object[]        m_defaultTex;
	private UnityEngine.Object[]        m_defaultThumbs;
//    private Texture2D       m_curUsedTex;         // Texture utilisée en ce moment
    private int             m_curUsedTexI;          // Indice
    private int             m_lastUsedTexI;         // Indice de la dernière texture utilisée
//    private int             m_saveTexID;          // ID de texture à sauvegarder

    // -- Textures synthétisées --
	private int 			m_maxSynthTex = 100;		
    private Texture2D[]     m_synthTex;             // Textures synthétisées
    private int             m_curSynthTex;          // Itérateur, pointe sur l'indice de la prochaine texture
    private Texture2D[]     m_synthTexGenPt;        // Points de départ de génération des tex. synth.
//    private List<Vector2>   m_loadHistory;        // Historique chargé d'une sauvegarde
	ContextDataModel cdm ;
	string[] strDate;
    private static readonly string DEBUGTAG = "GrassHandler : ";

#region unity_func
    //-----------------------------------------------------
	void Start()
    {
        if(m_mainCam == null)       Debug.LogError(DEBUGTAG+"MainCam"+PC.MISSING_REF);
        if(m_backgroundImg == null) Debug.LogError(DEBUGTAG+"Background image"+PC.MISSING_REF);
        if(m_grassSky == null)      Debug.LogError(DEBUGTAG+"Grass Sky"+PC.MISSING_REF);

        // -- Textures par défault --
        if(m_defaultTex == null){
            m_defaultTex = Resources.LoadAll("grass/default", typeof(Texture2D));
		}
		if(m_defaultThumbs == null){
			m_defaultThumbs = Resources.LoadAll("grass/thumbs", typeof(Texture2D));
		}
        m_grassRenderTex = null;

//        m_curUsedTex  = null;
        m_curUsedTexI  = 0;
        m_lastUsedTexI = -1;
//        m_saveTexID       = 0;

        m_synthTex = new Texture2D[m_maxSynthTex];
        for(int i=0; i<m_maxSynthTex; i++)
            m_synthTex[i] = null;
        m_curSynthTex = 0;
        m_synthTexGenPt = new Texture2D[m_maxSynthTex];
//        m_loadHistory = null;

	}

    //-----------------------------------------------------
	void Update()
    { }

#endregion

#region internal
    //-----------------------------------------------------
//    private void SetNightMode(bool night)
//    {
//        m_nightMode = night;
//    }

    //-----------------------------------------------------
    private RenderTexture CreateRenderTex(int sizeDiv)
    {
        int resW, resH;
        Rect r = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
        #if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID
            Resolution res = Screen.currentResolution;
            resW = res.width;
            resH = res.height;
        #else
            Resolution[] res = Screen.resolutions;    // TODO iPad limiter (rétina)
            resW = res[res.Length-1].width;
            resH = res[res.Length-1].height;
		    
        #endif
        r = ImgUtils.ResizeImagePreservingRatio(r.width, r.height, resW, resH);
//        Debug.Log ("CREATED RT : "+(r.width/sizeDiv)+", "+(r.height/sizeDiv));
        return new RenderTexture((int)(r.width/sizeDiv), (int)(r.height/sizeDiv), 16);
    }

    //-----------------------------------------------------
    public void HandleZoneTex(int id, Texture2D texGen)
    {

		if(cdm == null || strDate ==null)
		{
			cdm = new ContextDataModel();
			strDate = cdm.m_lastOpenDate.Split('/');
		}
		if(new DateTime(Convert.ToInt32(strDate[2]),Convert.ToInt32(strDate[1]),Convert.ToInt32(strDate[0])) >= (new DateTime(2015,7,11)))
		{
			if(id == -1)
	            return;
			else if(id < m_synthTex.Length)
	            SetUsedTexture(id);
   		 }
	}

#endregion

#region save_load
    //-----------------------------------------------------
//    public int[] GetSaveData()
//    {
//        int[] output = new int[m_history.Count*2];
//        for(int j=0, i=0; i<m_history.Count; i++)
//        {
//            output[j++] = (int)m_history[i].x;
//            output[j++] = (int)m_history[i].y;
//        }
//        return output;
//    }
//
//    //-----------------------------------------------------
//    public void Load(int[] data)
//    {
//        m_loadHistory = new List<Vector2>();
//        for(int i=0; i<data.Length; i+=2)
//            m_loadHistory.Add(new Vector2(data[i], data[i+1]));
//    }

#endregion

    //-----------------------------------------------------
    public Texture2D GetGrassImage(int sizeDiv, Vector4 bounds)
    {
        if(m_grassRenderTex == null)
            m_grassRenderTex = CreateRenderTex(sizeDiv);

        //m_mainCam.enabled = false;

        if(m_grassRenderTex.width  != m_mainCam.pixelRect.width  ||
           m_grassRenderTex.height != m_mainCam.pixelRect.height ||
           m_lastUsedTexI          != m_curUsedTexI)
        {
//            Rect backupRect = m_mainCam.pixelRect;
//            int backupMask = m_mainCam.cullingMask;
//            m_mainCam.GetComponent<MainCamManager>().enabled = false;
//            m_mainCam.cullingMask = 1 << 20; // Sélectionner seulement le calque contenant les plans du gazon
//            m_mainCam.rect = new Rect(0f, 0f, 1f, 1f);
//            m_mainCam.targetTexture = m_grassTex;
//            m_mainCam.Render();
//            m_mainCam.targetTexture = null;
//            m_mainCam.pixelRect = backupRect;
//            m_mainCam.cullingMask = backupMask;
//            m_mainCam.GetComponent<MainCamManager>().enabled = true;
            m_mainCam.GetComponent<MainCamManager>().RenderSingle(m_grassRenderTex,
                                                        new Rect(0f, 0f, 1f, 1f), false, (1 << 20));
            m_lastUsedTexI = m_curUsedTexI;
        }
        RenderTexture.active = m_grassRenderTex;
        int w = (int)(bounds.y - bounds.x);
        int h = (int)(bounds.w - bounds.z);
//        Debug.Log ("BOUNDS BOUNDS UBODUS = "+bounds.ToString());
        Texture2D grassBuffer = new Texture2D(w, h, TextureFormat.RGB24, false);
        float y = usefullData.s_openGL? bounds.z : (m_grassRenderTex.height - bounds.w);
        Rect bRect = new Rect(bounds.x, y, w, h);
//        Debug.Log("bbonuds : "+bRect.ToString());
        grassBuffer.ReadPixels(bRect, 0, 0, false);
        RenderTexture.active = null;
		/*for (int i=0; i<grassBuffer.texelSize.x; i++)
						for (int j=0; i<grassBuffer.texelSize.x; j++)
								if (grassBuffer.GetPixel (i, j).r == 1 && grassBuffer.GetPixel (i, j).g == 1 && grassBuffer.GetPixel (i, j).b == 1)
										grassBuffer.SetPixel (i, j, new Color (1.0f, 0.0f, 0.0f, 0.0f));*/

        return grassBuffer;
    } // GetImage
    
    //-----------------------------------------------------
    public void ReleaseTexture()
    {
        RenderTexture.active = null;
        m_mainCam.targetTexture = null;
//        m_grassTex.Release();
    }

    //-----------------------------------------------------
    public void AddSynthTex(Texture2D newSynthTex, Texture2D baseTex, bool useImmediately = false)
    {
        if(m_synthTex[m_curSynthTex] != null)
            Destroy(m_synthTex[m_curSynthTex]);
		newSynthTex.name = "Capture"+GameObject.Find("MainScene").GetComponent<GUISubTools>().newTextures.Count;
		GameObject.Find("MainScene").GetComponent<GUISubTools>().newTextures.Add(newSynthTex);
        m_synthTex[m_curSynthTex] = newSynthTex;
        m_synthTexGenPt[m_curSynthTex] = baseTex;
//        Debug.Log(DEBUGTAG+"ADDED SYNTH : "+m_curSynthTex);

        if(useImmediately)
            SetUsedTexture(m_curSynthTex);

        m_curSynthTex = (m_curSynthTex + 1) % m_maxSynthTex;

//        m_saveTexID = index < m_defaultTex.Length ? index : m_defaultTex.Length + m_curSynthTex;

        // Mise à jour de l'interface (devrait être fait dans le contrôleur)
        GameObject.Find("MainScene").GetComponent<GUISubTools>().SetSynthesizedGrassTex(newSynthTex);
    }

    //-----------------------------------------------------
    public void SetUsedTexture(int index)
    {
        Texture2D newTex;
		int count = m_defaultTex.Length;
        //Debug.Log(DEBUGTAG+"SET USED TEXTURE id = "+index+", len="+m_defaultTex.Length);
		newTex = (Texture2D) m_defaultTex[index];
		//Debug.Log ("hanlder!: "+newTex.name);
        m_curUsedTexI = index;
        for(int i=0; i<transform.childCount; i++)
            transform.GetChild(i).GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
		
		m_grassSky.GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
    }

    //-----------------------------------------------------
    public UnityEngine.Object[] GetDefaultTexs()
    {
        return m_defaultTex;
    }

	public UnityEngine.Object[] GetDefaultThumbs()
	{
		return m_defaultThumbs;
	}
    //-----------------------------------------------------
    public Texture2D[] GetSynthTex()
    {
        return m_synthTex;
    }

    //-----------------------------------------------------
    public int GetCurTexID()
    {
        return m_curUsedTexI;
    }

    //-----------------------------------------------------
    public Texture2D GetSynthTexGen()
    {
        if(m_curUsedTexI < m_defaultTex.Length)
            return null;
        else
        {
//            int id = m_curUsedTexI - m_defaultTex.Length;
            int lastSynthTexID = m_curUsedTexI - m_defaultTex.Length;
//            Debug.Log("GETTING SYNTH TEX COORD : "+lastSynthTexID);
            return m_synthTexGenPt[lastSynthTexID];
        }
    }

//    //-----------------------------------------------------
//    public Texture2D[] GetSynthTexs()
//    {
//        return m_synthTex;
//    }
    
} // class GrassCapture
