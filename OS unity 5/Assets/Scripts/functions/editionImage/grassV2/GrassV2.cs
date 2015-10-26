using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Pointcube.Global;
using Pointcube.Utils;


//-----------------------------------------------------
public class GrassV2 : MonoBehaviour
{

#region attributs

    public  int                      m_maskSizeDiv = 1;

    // -- Références scène --
    public  GameObject               m_mainScene;
    public  GameObject               m_backgroundImg;
    public  GameObject               m_grassSkybox;
    public  Texture2D                m_mask;
    public  Texture2D                m_grassBuffer;


    // -- Mécaniques --
    private LinkedList<GrassV2zone>  m_zones;           // Toutes les zones
    private Stack<GrassV2zone>       m_undoedZones;     // pour redo (Pile car pas besoin de plus)
    private List<GrassV2zone>        m_backupZones;     // pour undo
    private LinkedList<IEv2_Zone>    m_zonesVec;        // zones en vectoriel
    private Stack<IEv2_Zone>         m_undoedZoneVec;
    // TODO undo / redo

    // -- Application texture --
    private bool                     m_needMaskUpdate;
    private bool                     m_zoneAdded;
    private GrassV2zone              m_updatedZone;     // Zone récemment ajoutée
    private int                      m_updatingCount;   // nombre d'update en cours sur le mask
    private bool                     m_needMaskApply;   // Besoin ou non de mettre à jour le masque

    private Rect                     m_pixIn;

    // -- Chargement montage --
    private bool                     m_mustAddZones;
    private List<IEv2_Zone>          m_zonesToAdd;
    private int[]                    m_zonesTexIDtoAdd;
    private List<byte[]>             m_zonesTexGenToAdd;

    private static readonly uint     m_maxUndoCount = 33;    // nombre d'undo possibles

    // -- Debug --
    private static readonly string   DEBUGTAG = "GrassV2 : ";
    private static readonly bool     DEBUG    = true;

	bool isNight = false;

	LightConfiguration lightconfig;

#endregion

#region unity_func
    //-----------------------------------------------------
    void OnEnable()
    {
        UsefullEvents.NightMode += SetNightMode;
//        UsefullEvents.OnResizingWindow  += FitGuiTextureToScreen; // Appelé par bgImgManager
//        UsefullEvents.OnResizeWindowEnd += FitGuiTextureToScreen;
    }

	//-----------------------------------------------------
	void Start()
    {
        //if((PC.DEBUG && DEBUG) || PC.DEBUGALL) Debug.Log(DEBUGTAG+"Start");

        if(m_mainScene == null)     Debug.LogError(DEBUGTAG+"Main Scene"     +PC.MISSING_REF);
        if(m_backgroundImg == null) Debug.LogError(DEBUGTAG+"Background Img" +PC.MISSING_REF);
        if(m_grassSkybox == null)   Debug.LogError(DEBUGTAG+"Grass skybox"   +PC.MISSING_REF);

        m_zones          = new LinkedList<GrassV2zone>();
        m_undoedZones    = new Stack<GrassV2zone>();
        m_backupZones    = new List<GrassV2zone>();
        m_zonesVec       = new LinkedList<IEv2_Zone>();
        m_undoedZoneVec  = new Stack<IEv2_Zone>();

        m_mask           = null; // Mask initialisé dans Reset (appelé par d'autres modules OS3D)
        m_zoneAdded      = false;
        m_needMaskUpdate = false;
//        m_newZone        = null;
        m_updatingCount  = 0;
        m_needMaskApply  = false;

        m_pixIn          = new Rect();

        m_mustAddZones     = false;
        m_zonesToAdd       = null;
        m_zonesTexIDtoAdd  = null;
        m_zonesTexGenToAdd = null;
		lightconfig = GameObject.Find ("LightPivot").GetComponent<LightConfiguration>();

	}

	//-----------------------------------------------------
	void Update()
    {
		if(!isNight)
		{
			float temp =  (1-lightconfig.getIntensity()*1.0f)-0.6f;
			if(temp<0)
				temp = 0;
			GetComponent<GUITexture>().color = new Color (m_backgroundImg.GetComponent<GUITexture>().color.r-temp,m_backgroundImg.GetComponent<GUITexture>().color.g-temp,m_backgroundImg.GetComponent<GUITexture>().color.b-temp,1.0f);
		}
			//if(!isNight)
		//	guiTexture.color;

		if(m_needMaskApply && m_updatingCount == 0)
        {
            m_needMaskApply = false;
            StartCoroutine(ApplyMask());
        }

        if(m_needMaskUpdate && m_mainScene.GetComponent<PleaseWaitUI>().IsDisplayingIcon())
        {
            StartCoroutine(UpdateMask(m_updatedZone, true, m_zoneAdded));
            m_needMaskUpdate = false;
        }

        if(m_mustAddZones && m_mainScene.GetComponent<PleaseWaitUI>().IsDisplayingIcon())
        {
            m_mustAddZones = false;
            AddZones(m_zonesToAdd, m_zonesTexIDtoAdd, m_zonesTexGenToAdd);
        }
        else if(m_mustAddZones && !m_mainScene.GetComponent<PleaseWaitUI>().IsDisplayingIcon())
            m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
	}

    //-----------------------------------------------------
    void OnDisable()
    {
        UsefullEvents.NightMode -= SetNightMode;

//        UsefullEvents.OnResizingWindow  -= FitGuiTextureToScreen;    // Cf BgImgManager
//        UsefullEvents.OnResizeWindowEnd -= FitGuiTextureToScreen;
    }

#endregion

#region multi_res
    //-----------------------------------------------------
    public void FitGuiTextureToScreen()
    {
        m_pixIn = (m_backgroundImg.GetComponent<GUITexture>().pixelInset);
        GetComponent<GUITexture>().pixelInset = m_pixIn;
    }
#endregion

#region internal
    //-----------------------------------------------------
    private IEnumerator UpdateMask(GrassV2zone newZone, bool apply, bool added)
    {
        m_updatingCount++;

        // -- Application des pixels de la zone au masque --
        List<MaskPixRGB> pixels = newZone.m_pixels;

        List<MaskPixRGB> bkPix = (added ? m_backupZones[m_backupZones.Count-1].m_pixels : null);
        for(int i=0; i<pixels.Count; i++)
        {
            int x = (int)(pixels[i].GetX()/(float)m_maskSizeDiv);
            int y = (int)(pixels[i].GetY()/(float)m_maskSizeDiv);

            for(int j=0; j<m_maskSizeDiv; j++)
            {
                for(int k=0; k<m_maskSizeDiv; k++)
                {
                    if(added)
                    {
                        bkPix[i].SetColor(m_mask.GetPixel(x+j, y+k));  // Sauvegarde pour futur undo
//                        Debug.Log (m_mask.GetPixel(x+j, y+k));
                    }
                    m_mask.SetPixel(x+j, y+k, pixels[i].GetColor());   // Application nouveau pixel
                }
            }
        } // foreach pixel
//        Debug.Log(DEBUGTAG+" Updating grass ... "+pixels.Count+" pixels written on mask = "+m_mask.width+", "+m_mask.height);

//        for(int i=0; i<pixels.Count; i++)
//            Debug.Log("yoho "+m_backupZones[m_backupZones.Count-1].m_pixels[i].GetColor().ToString());

        if(apply)
            StartCoroutine(ApplyMask());

        m_updatingCount--;
        yield return null;
    } // UpdateMask()

    //-----------------------------------------------------
    private IEnumerator ApplyMask()
    {
//        System.DateTime begin = System.DateTime.Now;
//        Debug.Log("Updating grass mask image begin ...");
        m_mask.Apply();
//        Debug.Log("Ended apply time spent="+(System.DateTime.Now-begin));
        GetComponent<GUITexture>().texture = m_mask;
        m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);

        yield return null;
    }

    //----------------------------------------------------
    private Texture2D CreateMask(bool create)
    {
        int resW, resH;
        Rect r = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID
        Resolution res = Screen.currentResolution;
        resW = res.width;
        resH = res.height;
#else
        Resolution[] res = Screen.resolutions;
        resW = res[res.Length-1].width;
        resH = res[res.Length-1].height;
#endif
        r = ImgUtils.ResizeImagePreservingRatio(r.width, r.height, resW, resH);
//        Debug.Log("TEXTURE= "+(r.width/m_maskSizeDiv)+", "+(r.height/m_maskSizeDiv));
        Texture2D mask = null;
        if(create)
        {
            mask = new Texture2D((int)(r.width/m_maskSizeDiv), (int)(r.height/m_maskSizeDiv),
                                                                       TextureFormat.RGBA32, false);
        }
        else
        {
            mask = m_mask;
            mask.Resize((int)(r.width/m_maskSizeDiv), (int)(r.height/m_maskSizeDiv));
        }

        Color empty = Color.clear;
        Color[] pix = new Color[mask.width * mask.height];
        for(int i=0; i<pix.Length; i++)
            pix[i] = empty;
        mask.SetPixels(pix);

        return mask;
    } // CreateMask()

    //----------------------------------------------------
    private void SetNightMode(bool night)
    {
		if (night)
		{
			GetComponent<GUITexture>().color = m_backgroundImg.GetComponent<GUITexture>().color;
			isNight = true;
		}
        else
		{
            GetComponent<GUITexture>().color = Color.gray;
			isNight = false;
		}

    }

    //-----------------------------------------------------
    private void AddZones(List<IEv2_Zone> newZones, int[] zonesTexID, List<byte[]> texsGen)
    {
        for(int i=0, j=0; i<newZones.Count; i++)
        {
            Texture2D texGen = null;
            if(texsGen[i].Length > 0)
            {
                texGen = new Texture2D(25, 25);
                texGen.LoadImage(texsGen[i]);
            }
            m_grassSkybox.GetComponent<GrassHandler>().HandleZoneTex(zonesTexID[j++], texGen);
            AddZone(newZones[i]);
            StartCoroutine(UpdateMask(m_updatedZone, false, true));
        }
        m_needMaskUpdate = false;

        m_needMaskApply = true;
        m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
        // pour chaque zone, GrassHandler.SetCurTexture(zoneID), puis AddZone
    }
#endregion

#region save_load
    //-----------------------------------------------------
    public List<float> GetZonesData()
    {
        List<float> output = new List<float>();

        for(LinkedListNode<IEv2_Zone> it = m_zonesVec.First; it != null; it = it.Next)
        {
            float[] zoneData = it.Value.GetSaveData();
            for(int i=0; i<zoneData.Length; i++)
                output.Add(zoneData[i]);
        }
//        Debug.Log (DEBUGTAG+" DATA TO SAVE = "+output[0]+" = "+output.Count);

        return output;
    } // GetZonesData()

    //-----------------------------------------------------
    public List<int> GetZoneTexID()
    {
        List<int> output = new List<int>();
        for(LinkedListNode<GrassV2zone> it = m_zones.First; it != null; it = it.Next)
            output.Add(it.Value.m_textureID);

        return output;
    }

    //-----------------------------------------------------
    public List<byte[]> GetZoneSynthTexBase()
    {
        List<byte[]> output = new List<byte[]>();
        for(LinkedListNode<GrassV2zone> it = m_zones.First; it != null; it = it.Next)
        {
            if(it.Value.m_texGen != null)
				output.Add(it.Value.m_texGen.EncodeToJPG());
            else
                output.Add(new byte[0]);
//            Debug.Log("SAVING TEX GEN : "+it.Value.m_textureID+" Gen Coord = "+it.Value.m_synthTexGenCoord.ToString());
        }

        return output;
    }

    //-----------------------------------------------------
    public void Load(float[] data, int[] texIds, List<byte[]> texsGen)
    {
//       Debug.Log(DEBUGTAG+"loading !");
        List<IEv2_Zone> loadedZones = new List<IEv2_Zone>();
        IEv2_Zone newZone = null;
        int i=0;
        while(i<data.Length)
        {
            float[] zoneData = new float[(int)data[i]];

            for(int j=0; j<zoneData.Length; j++)
                zoneData[j] = data[i++];

//            Debug.Log("grassV2 Load : num "+i+" = "+data[i-1]);
            if(data[i-1] == 0f)
                newZone = new Ev2_Polygon();
            else
                Debug.LogError(DEBUGTAG+"load : unrecognized IEv2_Zone type ("+data[i-1]+")");

            newZone.LoadFromData(zoneData);
            loadedZones.Add(newZone);
        }

        m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
        m_mustAddZones = true;
        m_zonesToAdd = loadedZones;
        m_zonesTexIDtoAdd = texIds;
        m_zonesTexGenToAdd = texsGen;
    } // Load

#endregion

#region public
    //-----------------------------------------------------
    public void Reset()
    {
        if(m_mask == null)
            m_mask = CreateMask(true);
        else
            m_mask = CreateMask(false);
//        guiTexture.texture = m_mask;

        FitGuiTextureToScreen();
        m_needMaskApply = true;

        m_zones.Clear();
        m_undoedZones.Clear();
        m_backupZones.Clear();
        m_zonesVec.Clear();
        m_undoedZoneVec.Clear();
    }

    //----------------------------------------------------
    public void AddZone(IEv2_Zone newZone)
    {
        m_zonesVec.AddLast(newZone);

        // -- Construction de la zone --
        GrassV2zone gZone = newZone.RasterizeRGB(new Rect(0f, 0f, m_mask.width, m_mask.height));
        if(newZone.GetMainColor() == Color.clear)
            gZone.m_textureID = -1;
        else
            gZone.m_textureID = m_grassSkybox.GetComponent<GrassHandler>().GetCurTexID();
        gZone.m_texGen = m_grassSkybox.GetComponent<GrassHandler>().GetSynthTexGen();
//        Debug.Log(DEBUGTAG+"Zone ADDED : "+gZone.m_textureID+", "+(gZone.m_texGen != null ? gZone.m_texGen.ToString() : "null"));

        m_grassBuffer = m_grassSkybox.GetComponent<GrassHandler>().GetGrassImage(m_maskSizeDiv,
                                                                                    gZone.m_bounds);
//        m_grassBuffer.Apply(false); // debug : pour voir la texture dans l'inspecteur

        if(newZone.GetMainColor() != Color.clear)
        {
            List<MaskPixRGB> pix = gZone.m_pixels;
            for(int i=0; i<pix.Count; i++)
            {
                int x = pix[i].GetX() - (int)gZone.m_bounds.x;
                int y = pix[i].GetY() - (int)gZone.m_bounds.z;
                pix[i].SetColor(m_grassBuffer.GetPixel(x, y));
            }
        }
//        Debug.Log (DEBUGTAG+"ADDING Zone : "+pix.Count+" pixels");

        m_updatedZone = gZone;
        m_zones.AddLast(m_updatedZone);
        m_backupZones.Add(new GrassV2zone(gZone));
        m_undoedZones.Clear();                 // Supprimer zones annulées (plus de redo possible)

        m_needMaskUpdate = true;
        m_zoneAdded = true;
        m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);

        SetVisible(true);
    }

    //-----------------------------------------------------
    public void UndoLastZone()
    {
        // Annuler si la limite d'annulations n'est pas atteinte,
        // ou bien s'il reste une seule zone qui n'a pas été fusionnée au préalable
        if(m_zones.Count >= 1 && m_undoedZones.Count < m_maxUndoCount && m_backupZones.Count > 0)
        {
            m_undoedZones.Push(m_zones.Last.Value);   // Ajouter la zone annulée aux zones d'undo
            m_zones.RemoveLast();                     // Supprimer la zone annulée des zones gommées
            m_undoedZoneVec.Push(m_zonesVec.Last.Value);
            m_zonesVec.RemoveLast();                  // Idem pour les zones vectorielles

            m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
            m_updatedZone = m_backupZones[m_backupZones.Count-1];
            m_zoneAdded = false;                           // Mettre à jour l'image en rétablissant les pixels de l'ancienne zone
            m_needMaskUpdate = true;
            m_backupZones.RemoveAt(m_backupZones.Count-1); // Supprimer l'ancienne zone
        }
        else
            Debug.Log(DEBUGTAG+"Can't undo. Undo limit (" + m_maxUndoCount + ") reached ?");
    }

    //----------------------------------------------------
    public void Redo()
    {
        if(m_undoedZones.Count > 0)
        {
            m_zones.AddLast(m_undoedZones.Pop());
            m_zonesVec.AddLast(m_undoedZoneVec.Pop());

            m_backupZones.Add(new GrassV2zone(m_zones.Last.Value));
            m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
            m_updatedZone = m_zones.Last.Value;
            m_needMaskUpdate = true;
            m_zoneAdded = true;
        }
        else
            Debug.Log(DEBUGTAG+"Nothing to redo");
    }
	
	public bool HasBeenUsedOnce()
	{
		if(m_zones != null)
			return m_zones.Count>0;
		else
			return false;
	}

#endregion

    //----------------------------------------------------
    public void SetVisible(bool visible)
    {
        GetComponent<GUITexture>().enabled = visible;
    }

} // class GrassV2
