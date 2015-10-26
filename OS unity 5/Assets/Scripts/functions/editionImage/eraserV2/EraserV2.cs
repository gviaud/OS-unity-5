using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pointcube.Global;

public class EraserV2 : MonoBehaviour
{
    //-----------------------------------------------------
//    public struct EraserV2zone
//    {
//        public List<MaskPix> m_pixels;
//        public Vector4       m_bounds;
//
//        public EraserV2zone(List<MaskPix> pixels, float minX, float maxX, float minY, float maxY)
//        {
//            m_pixels = pixels;
//            m_bounds = new Vector4(minX, maxX, minY, maxY);
//        }
//
//        public EraserV2zone(List<MaskPix> pixels, Vector4 bounds)
//        {
//            m_pixels = pixels;
//            m_bounds = bounds;
//        }
//    } // struct EraserV2zone

    //-----------------------------------------------------
    public  int        m_maskSizeDiv = 1;

    // -- Références scène --
    public  GameObject m_mainScene;
    public  GameObject m_mainCam;
    public  GameObject m_backgroundImg;
    public  Material   m_eraserV2mat;

    // -- --
    public  Texture2D  m_mask;                         // Texture masque

    // -- Zones gommées --
    private LinkedList<IEv2_Zone>     m_zones;         // Toutes les zones
    private Stack<IEv2_Zone>          m_undoedZones;   // pour redo (Pile car pas besoin de plus)
//    private List<IEv2_Zone>           m_backupZones;   // pour undo


    // -- Application texture --
    private bool                      m_needMaskUpdate;
    private IEv2_Zone                 m_zoneToUpdate;   // Zone récemment ajoutée ou retirée (undo)
    private int                       m_updatingCount;  // nombre d'update en cours sur le mask
    private bool                      m_needMaskApply;  // Besoin ou non de mettre à jour le masque

//    // -- Sauvegarde --
//    private SceneUpgradeModel         m_model;

    private static readonly string    DEBUGTAG = "EraserV2 : ";
    private static readonly bool      DEBUG = true;

    private static readonly uint      m_maxUndoCount = 33;    // nombre d'undo possibles

#region unity_func

    //-----------------------------------------------------
    void Awake()
    {
//        UsefullEvents.OnResizeWindowEnd += ResizeMask;
    }

    //-----------------------------------------------------
	void Start()
    {
        if(m_mainScene == null)     Debug.LogError(DEBUGTAG+"MainScene"      +PC.MISSING_REF);
        if(m_mainCam == null)       Debug.LogError(DEBUGTAG+"MainCam"        +PC.MISSING_REF);
        if(m_backgroundImg == null) Debug.LogError(DEBUGTAG+"Background Img" +PC.MISSING_REF);
        if(m_eraserV2mat == null)   Debug.LogError(DEBUGTAG+"Eraser material"+PC.MISSING_REF);

	    m_zones         = new LinkedList<IEv2_Zone>();
        m_undoedZones   = new Stack<IEv2_Zone>();
//        m_backupZones   = new List<IEv2_Zone>();

        m_mask           = null; // Mask initialisé dans Reset (appelé par d'autres modules OS3D)
        m_needMaskUpdate = false;
        m_updatingCount  = 0;
        m_needMaskApply  = false;
        m_zoneToUpdate   = null;

//        Reset();                // TODO virer ce reset quand dev fini !! DEV ONLY !!
//        m_model = Montage.sum;
	}

    //-----------------------------------------------------
	void Update()
    {
        if(m_needMaskApply && m_updatingCount == 0)
        {
            m_needMaskApply = false;
            StartCoroutine(ApplyMask());
        }

        if(m_needMaskUpdate && m_mainScene.GetComponent<PleaseWaitUI>().IsDisplayingIcon())
        {
            StartCoroutine(UpdateMask(m_zoneToUpdate, true));
            m_needMaskUpdate = false;
        }

	} // Update()

    //-----------------------------------------------------
    void OnDestroy()
    {
//        UsefullEvents.OnResizeWindowEnd -= ResizeMask;
    }

#endregion

#region multi_res
    //-----------------------------------------------------
    public void ResizeMask()
    {
        if(m_mask != null)
        {
            Rect bgRect = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
            if(m_mask.width != bgRect.width || m_mask.height != bgRect.height)
                UpdateAllZones();
        }
    }
#endregion

#region public
    //-----------------------------------------------------
    public void Reset()
    {
        if(m_mask == null)
        {
            m_mask = ResetMask(true);
            m_eraserV2mat.SetTexture("_AlphaMap", m_mask);
        }
        else
            m_mask = ResetMask(false);

        m_zones.Clear();
        m_undoedZones.Clear();
//        m_backupZones.Clear();

        m_needMaskApply = true;
    }

    //-----------------------------------------------------
    // Ajoute une zone de gommage / rétablissement
    public void AddZone(IEv2_Zone newZone)
    {
        // Fusionner les plus anciennes zones en une seule si la pile est pleine
//        while(m_erasedZones.Count > m_maxUndoCount)
//           MergeZones();

        // Ajouter la zone aux zones enregistrées
        m_zoneToUpdate = newZone;
        m_zones.AddLast(m_zoneToUpdate);
//        m_backupZones.Add(m_zoneToUpdate.Clone());
        m_undoedZones.Clear();               // Supprimer les zones annulées (plus de redo possible)

        m_needMaskUpdate = true;

        m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);

//        SetVisible(true);
    } // AddZone()

    //-----------------------------------------------------
    public void UndoLastZone()
    {
        if(m_zones.Count >= 1 && m_undoedZones.Count < m_maxUndoCount /*&& m_backupZones.Count > 0*/)
        {
            m_undoedZones.Push(m_zones.Last.Value);        // Ajouter la zone aux zones annulées
            m_zones.RemoveLast();                          // Supprimer la zone des zones gommées
            m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
            UpdateAllZones();
//            m_zoneToUpdate = m_backupZones[m_backupZones.Count-1];
//            m_undo = true;
//            m_backupZones.RemoveAt(m_backupZones.Count-1); // Supprimer l'ancienne zone
        }
        else
            Debug.Log(DEBUGTAG+"Impossible d'annuler (limite d'annulations (" + m_maxUndoCount + ") atteinte ?)");
    }

    //-----------------------------------------------------
    public void Redo()
    {
        if(m_undoedZones.Count > 0)
        {
            m_zones.AddLast(m_undoedZones.Pop());
//            m_backupZones.Add(CloneZone(m_erasedZones.Last.Value));

            m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
            m_needMaskUpdate = true;
            m_zoneToUpdate = m_zones.Last.Value;
        }
        else
            Debug.Log(DEBUGTAG+"Aucune action à refaire");
    }
	
	public bool HasBeenUsedOnce()
	{
		return m_zones.Count>0;	
	}

    //-----------------------------------------------------
    public void AddZones(List<IEv2_Zone> newZones)
    {
        for(int i=0; i<newZones.Count; i++)
            m_zones.AddLast(newZones[i]);

        UpdateAllZones();
    }
#endregion

#region save_load
    //-----------------------------------------------------
    public List<float> GetSaveData()
    {
        List<float> output = new List<float>();

        for(LinkedListNode<IEv2_Zone> it = m_zones.First; it != null; it = it.Next)
        {
            float[] zoneData = it.Value.GetSaveData();
            for(int i=0; i<zoneData.Length; i++)
                output.Add(zoneData[i]);
        }
//        Debug.Log (DEBUGTAG+" DATA TO SAVE = "+output[0]+" = "+output.Count);

        return output;
    } // GetSaveData

    //-----------------------------------------------------
    public void Load(float[] data)
    {
//        Debug.Log(DEBUGTAG+"loading !");
        List<IEv2_Zone> newZones = new List<IEv2_Zone>();
        IEv2_Zone newZone = null;
        int i=0;
        while(i<data.Length)
        {
            float[] zoneData = new float[(int)data[i]];

            for(int j=0; j<zoneData.Length; j++)
                zoneData[j] = data[i++];

//            Debug.Log("num "+i+" = "+data[i-1]);
            if(data[i-1] == 0f)
                newZone = new Ev2_Polygon();
            else
                Debug.LogError(DEBUGTAG+"load : unrecognized IEv2_Zone type ("+data[i-1]+")");

            newZone.LoadFromData(zoneData);
            newZones.Add(newZone);
        }
        AddZones(newZones);
    } // Load
#endregion

#region func_internal
    //----------------------------------------------------
    // create => new Texture2D, !create => resize
    private Texture2D ResetMask(bool create)
    {
        Rect bgRect = m_backgroundImg.GetComponent<GUITexture>().pixelInset;

        Texture2D mask;
        if(create)
        {
            mask = new Texture2D((int)(bgRect.width/m_maskSizeDiv),
                                  (int)(bgRect.height/m_maskSizeDiv), TextureFormat.Alpha8, false);
        }
        else // resize
        {
            mask = m_mask;
            mask.Resize((int)(bgRect.width/m_maskSizeDiv), (int)(bgRect.height/m_maskSizeDiv));
        }

        Color empty = Color.black;
        Color[] pix = new Color[mask.width * mask.height];
        for(int i=0; i<pix.Length; i++)
            pix[i] = empty;
        mask.SetPixels(pix);

        return mask;
    } // ResetMask()

    //-----------------------------------------------------
    private void UpdateAllZones()
    {
        ResetMask(false);
        for(LinkedListNode<IEv2_Zone> it = m_zones.First; it != null; it = it.Next)
            StartCoroutine(UpdateMask(it.Value, false));
        m_needMaskApply = true;
    }

    //-----------------------------------------------------
    private IEnumerator UpdateMask(IEv2_Zone newZone, bool apply)
    {
        m_updatingCount++;

        Rect bgRect = m_backgroundImg.GetComponent<GUITexture>().pixelInset;
        List<MaskPix> pixels = newZone.Rasterize(bgRect);

        Color pixCol = Color.clear;
        for(int i=0; i<pixels.Count; i++)
        {
            int x = (int)(pixels[i].GetX()/(float)m_maskSizeDiv);
            int y = (int)(pixels[i].GetY()/(float)m_maskSizeDiv);
            pixCol.a = pixels[0].GetA();
//            m_mask.SetPixel(x, y, pixCol);
            for(int j=0; j<m_maskSizeDiv; j++)
                for(int k=0; k<m_maskSizeDiv; k++)
                    m_mask.SetPixel(x+j, y+k, pixCol);
        } // foreach pixel

        if(apply)
            StartCoroutine(ApplyMask());

        m_zoneToUpdate = null;

        m_updatingCount--;
        yield return null;
    } // UpdateMask()

    //-----------------------------------------------------
    private IEnumerator ApplyMask()
    {
        System.DateTime begin = System.DateTime.Now;
//        Debug.Log("Updating eraser mask image begin ...");
        m_mask.Apply();
//        Debug.Log("Ended apply time spent="+(System.DateTime.Now-begin));
        m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);

        yield return null;
    }

#endregion

#region get_set
    //-----------------------------------------------------
    public void SetVisible(bool visible)
    {
        m_mainCam.GetComponent<MainCamManager>().SetVisible(visible);
    }

#endregion

} // class EraserV2
