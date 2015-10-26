//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/eraserMask/EraserMask.cs - 02/2012 - KS
// TODO Peaufinage : anti aliasing pour l'eraser ?
// TODO Optimisation : utiliser un HashSet<MaskPix> pour les zones fusionnées, et éviter les duplications de pixel (ajouter la fonction getHashCode ?)
// TODO Amélioration outil : permettre de zoomer, et déplacer l'image (à réfléchir car pb avec la 3D si la caméra bouge...)
// TODO Enlever le liséré blanc visible dans certains cas entre les zones gommées et les zones non gommées.

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

//-----------------------------------------------------------------------------
// EraserMask - attachée de base à EraserImage
//    Met à jour l'image de la gomme, et fournit les fonctions qui permettent
//    de gommer ou rétablir une zone de la scène.
public class EraserMask : MonoBehaviour
{
    //-----------------------------------------------------
    public class MaskedZone
    {
        public List<MaskPix> m_pixels;      // Pixels de la zone
        public bool[] m_images;             // Tableau de bool pour chaque sous-image
                                            // (True = sous-image contenant des pixels de cette zone)
        public MaskedZone(List<MaskPix> pixels, int imgw, int imgh, int xMin, int xMax, int yMin, int yMax)
        {
            int childX, childY, cols, rows;
            rows = cols = (int)Mathf.Sqrt(c_childCount);

            m_pixels = pixels;
            if(xMax != 0 && yMax != 0)
            {
                int childXmin = (int) ((xMin - (xMin % imgw)) / imgw);
                int childYmin = (int) ((yMin - (yMin % imgh)) / imgh);
                int childXmax = (int) ((xMax + imgw-1 - (xMax % imgw)) / imgw);
                int childYmax = (int) ((yMax + imgh-1 - (yMax % imgh)) / imgh);
                m_images = new bool[c_childCount];

//                Debug.Log(xMin+"-"+xMax+", "+yMin+","+yMax+" -> "+childXmin+"-"+childXmax+","+childYmin+"-"+childYmax);

                for(int i=0; i<c_childCount; i++)
                {
                    childX = i % cols;
                    childY = (i-childX)/rows;
                    m_images[i] = (childX >= childXmin && childX <= childXmax &&
                                   childY >= childYmin && childY <= childYmax);
//                    if(m_images[i]) Debug.Log("image "+i+" ("+childX+","+childY+") utilisee");
//                    else            Debug.Log("image "+i+" pas utilisee car !("+childX+">="+childXmin+" && "+childX+" <= "+childXmax+" && "+childY+" >= "+childYmin+" && "+childY+" <= "+childYmax+")");
                }
            }
            else
                m_images = null;
        }
		
		public MaskedZone(List<MaskPix> pixels, bool[] images)
		{
			m_pixels = pixels;
			m_images = images;
		}
    };

    //-----------------------------------------------------
    private const uint      m_maxUndoCount = 33;    // nombre d'undo possibles par défaut

    private GUITexture      m_guiTexRef;            // GUITexture de référence (image de fond actuelle)
    private Texture         m_currentTexture;       // Texture actuellement affichée

    private LinkedList<MaskedZone> m_erasedZones;   // pour stocker (LinkedList pour avoir accès au Top et au Bottom et à l'intérieur, + efficacité)
    private Stack<MaskedZone>      m_undoedZones;   // pour redo (Pile car pas besoin de plus)
    private List<MaskedZone>       m_backupZones;   // pour undo
    
    private  Texture               m_baseTexture;   // Texture de base (créée automatiquement en fonction de la taille de l'écran)

    // -- Outil utilisé --
    public GUIEditTools.EditTool m_tool; // TODO private

    // Nombre de sous-divisions pour la génération des sous-images
    public int              m_subDivCount = 4;
    
    // -- Offset des images --
    private int             m_offsety;             // Offset des sous-images (pour que celles du gazon soient au même endroit que
    private int             m_offsetx;             // celles de la gomme)

    private List<MaskPix>   m_zoneToUpdate;        // Zone à mettre à jour
    private bool            m_added;               // Zone à mettre à jour ajoutée ou non (non si c'est un undo)

    private const int       c_childCount = 16;     // Nombre de sous-images
//    private Texture2D[]     m_childImgs;         // Textures2D des sous-images
	
	private GameObject m_bgImg;

	#region Unity's fcns.
    //-----------------------------------------------------
    void Start()
    {   
		m_bgImg = GameObject.Find("Background/backgroundImage");
        m_guiTexRef = m_bgImg.GetComponent<GUITexture>();

        m_baseTexture = null;
//		if(m_baseTexture == null)
//			m_baseTexture = (Texture)Resources.Load("mask/eraser_baseImg256");
		
        DeleteSubImages();
        CreateSubImages(m_subDivCount);
//        m_childImgs = new Texture2D[m_subDivCount*m_subDivCount];

        m_erasedZones = new LinkedList<MaskedZone>();
        m_undoedZones = new Stack<MaskedZone>();
        m_backupZones = new List<MaskedZone>();

//        m_merged = false;

        m_zoneToUpdate      = null;
        m_added             = false;
        
        m_offsetx = 0;
        m_offsety = 0;
        
        if(m_tool == GUIEditTools.EditTool.None)        // Si outil non initialisé, utiliser la gomme par défaut
            m_tool = GUIEditTools.EditTool.Eraser;

//        m_baseTexture = new Texture2D(1024/m_subDivCount, 768/m_subDivCount);
//        Color pixCol = new Color(0, 0, 0, 0);
//        for(int x=0; x<m_baseTexture.width; x++)
//        {
//            for(int y=0; y<m_baseTexture.height; y++)
//                ((Texture2D)m_baseTexture).SetPixel(x, y, pixCol);
//        }
		
    } // Start()
    
    //-----------------------------------------------------
	public void setText()
	{}

    //-----------------------------------------------------
    void Update()
    {
        // Charger la texture si ce n'est pas déjà fait
        if(m_guiTexRef==null)
            m_guiTexRef = m_bgImg.GetComponent<GUITexture>();
        
        if(m_guiTexRef != null)
        {
//            if(m_guiTexRef.texture != null)
//            {
//				
//					
                // Mettre à jour la texture si besoin
//                Texture refTex = m_guiTexRef.texture;
//                if(!m_currentTexture || m_currentTexture.GetHashCode() != refTex.GetHashCode())
//                {
//					
                    // -- Reset d'image --
                    // Mise à jour de l'image de référence
//					
//                    m_currentTexture = refTex;
//                    for(int i=0; i<transform.GetChildCount(); i++)
//                    {
//                        if(m_baseTexture == null)
//                            Debug.LogError("Texture de base non trouvee, il faut en assigner une au script dans l'inspecteur.");
//                        Texture2D newTex = (Texture2D) Object.Instantiate(m_baseTexture); // Copie de l'image de base (transparente) pour ne pas l'éditer directement
//                        transform.GetChild(i).guiTexture.texture = (Texture) newTex;
//                    }
//                    
//                    // Réinitialisation des zones de gommage
//                    m_backupZones.Clear();
//                    m_erasedZones.Clear();
//
//                        m_currentTexture = refTex;
//						StartCoroutine(resetImage(refTex));
//					
                    // -- Fin reset --
//                } // si la texture a changé
//            } // if texture != null
//
//            GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
           
			if((GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().IsDisplayingIcon()) && m_zoneToUpdate != null)
            {
                System.DateTime begin = System.DateTime.Now;
                Debug.Log("Updating subimages begin...");
    
                StartCoroutine( UpdateEraserImage(m_zoneToUpdate, m_added) );
    
                Debug.Log("Ended updating subimages... time spent="+(System.DateTime.Now-begin));

                m_zoneToUpdate = null;
                //GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
            }
		
		} // if guiTextRef != null
    } // Update()
	
	//-----------------------------------------------------
    void OnGUI()
    {}
	
	#endregion
	
    //-----------------------------------------------------
	// Supprime les infos ARGB des images de gommes/gazon
	public IEnumerator resetTest(bool last)
	{
		Debug.Log("Start reset "+m_tool);
		m_guiTexRef = m_bgImg.GetComponent<GUITexture>();
		Texture refTex = m_guiTexRef.texture;
		for(int i=0; i<transform.GetChildCount(); i++)
        {
            if(m_baseTexture == null)
            {
                int subdiv = (int) Mathf.Sqrt(transform.GetChildCount());
                m_baseTexture = CreateBaseTexture(Screen.width/subdiv, Screen.height/subdiv);
            }

            Texture2D newTex = (Texture2D) Object.Instantiate(m_baseTexture); // Copie de l'image de base (transparente) pour ne pas l'éditer directement
            transform.GetChild(i).GetComponent<GUITexture>().texture = (Texture) newTex;
//          m_childImgs[i] = newTex;
			yield return new WaitForEndOfFrame();
        }

        // Réinitialisation des zones de gommage
        m_backupZones.Clear();
        m_erasedZones.Clear();
		yield return new WaitForSeconds(Time.deltaTime);

		if(last)
		{
			Resources.UnloadUnusedAssets();
			//GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
			Montage.s_eraserMasksRdy = true;
		}
		Debug.Log("End reset "+m_tool);

		yield return true;
	}
	
	public IEnumerator resetImage(Texture refTex)
	{
        for(int i=0; i<transform.GetChildCount(); i++)
        {
            if(m_baseTexture == null)
            {
                int subdiv = (int) Mathf.Sqrt(transform.GetChildCount());
                m_baseTexture = CreateBaseTexture(Screen.width/subdiv, Screen.height/subdiv);
            }

            Texture2D newTex = (Texture2D) Object.Instantiate(m_baseTexture); // Copie de l'image de base (transparente) pour ne pas l'éditer directement
            transform.GetChild(i).GetComponent<GUITexture>().texture = (Texture) newTex;
//          m_childImgs[i] = newTex;
			yield return new WaitForEndOfFrame();
        }

        // Réinitialisation des zones de gommage
        m_backupZones.Clear();
        m_erasedZones.Clear();
		yield return new WaitForEndOfFrame();
		//Resources.UnloadUnusedAssets();
		Debug.Log("RESET FINI");
		yield return true;
	}//Obsolete

    //-----------------------------------------------------
    private Texture2D CreateBaseTexture(int width, int height)
    {
        Texture2D newTex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        byte      zero = 0;
        Color32[] pixels = new Color32[width*height];

        for(int i=0; i<pixels.Length; i++)
            pixels[i] = new Color32(zero, zero, zero, zero);

        newTex.SetPixels32(pixels);

        return newTex;
    } // CreateBaseTexture()

    //-----------------------------------------------------
	public void CreateSubImages()
	{
		CreateSubImages(m_subDivCount);
	}
    public void CreateSubImages(int subDivisionCount)
    {
        int subImgCount = subDivisionCount * subDivisionCount;
        int x, y;
        float xSize = Screen.width / subDivisionCount;
        float ySize = Screen.height / subDivisionCount;
        Rect inset = new Rect(0, 0, 0, 0);
        Vector3 transf = new Vector3(0, 0, 1);

        for(int i=0; i<subImgCount; i++)
        {
            GameObject subImg = new GameObject("subImage_"+i, typeof(GUITexture));
            x = i%subDivisionCount;
            y = (i-x)/subDivisionCount;

            inset.width  = xSize;
            inset.height = ySize;
            inset.x = xSize*x;
//			Debug.Log("screen heigth "+(Screen.height - ySize*(y+1)));
			inset.y = Screen.height - ySize*(y+1);
            subImg.GetComponent<GUITexture>().pixelInset = inset;

            // -- Transform --
            subImg.transform.parent = transform;
            if(m_tool == GUIEditTools.EditTool.Grass) // Pour que les images du gazon ne passent pas devant la scène 3D
            {
                subImg.transform.localPosition = transf;
                subImg.layer = LayerMask.NameToLayer("grassUI");
            }
        } // Foreach sous-image à créer
    }

    //-----------------------------------------------------
    public void DeleteSubImages()
    {
        for(int i=transform.childCount-1; i>=0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    //-----------------------------------------------------
    // Ajoute une zone de gommage / rétablissement
    public void AddEraserZone(List<MaskPix> newZone, int xMin, int xMax, int yMin, int yMax)
    {
        int h = (int)transform.GetChild(0).GetComponent<GUITexture>().pixelInset.height;
        int w = (int)transform.GetChild(0).GetComponent<GUITexture>().pixelInset.width;

        MaskedZone newMZone = new MaskedZone(newZone, w, h, xMin, xMax, yMin, yMax);

        // Fusionner les plus anciennes zones en une seule si la pile est pleine
        while(m_erasedZones.Count > m_maxUndoCount)
           MergeZones();

        // Ajouter la zone aux zones enregistrées
        m_erasedZones.AddLast(newMZone);
        m_backupZones.Add(CloneZone(newMZone));
        m_undoedZones.Clear();                  // Supprimer définitivement les zones annulées (plus de redo possible)

        GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
        m_zoneToUpdate = newZone;
        m_added = true;

//        Debug.Log("Zone added ...");
//        for(int i=0; i<newMZone.m_images.Length; i++)
//        {
//            if(newMZone.m_images[i])
//            {
//                int x = i%4;
//                int y = (i-x)/4;
//                Debug.Log("Dans la sous-image "+i+" ("+x+","+y+")");
//            }
//        }

    } // AddEraserZone()

    //-----------------------------------------------------
    // Fusionner les 2 zones les plus anciennes
    private void MergeZones()
    {
        // Debug.Log("EraserMask : FUSIOOOOOOONNNNN");
        MaskedZone oldestZone = m_erasedZones.First.Value;
        m_erasedZones.RemoveFirst();
        MaskedZone oldestZone2 = m_erasedZones.First.Value;
        m_erasedZones.RemoveFirst();
		
		for(int i=0; i<oldestZone.m_images.Length; i++)
            oldestZone.m_images[i] = oldestZone.m_images[i] || oldestZone2.m_images[i];
		
        // TODO : en fusionnant de cette façon, les éventuels pixels dupliqués sont toujours présents,
        // mais les plus récents seront traités en dernier par UpdateEraserImage, donc l'image sera OK.
        // Pour faire propre il faudrait tout de même supprimer les premières occurences des doublons.
        oldestZone.m_pixels.AddRange(oldestZone2.m_pixels);
        m_erasedZones.AddFirst(oldestZone);
        m_backupZones.RemoveAt(0); // Supprimer la plus ancienne zone de sauvegarde
                                   // (qui ne servira plus car on ne peut plus annuler cette zone)
		
        // Ci-dessous : tentative de suppression des doublons, code TREEEES lent
        // En cas de doublons après fusion, ne garder que la dernière occurence (la plus à jour)
//            List<MaskPix> toRemove = new List<MaskPix>();
//
//            for(int i=0; i<m_erasedZones.First.Value.Count; i++)
//            {
//                MaskPix pixel1 = m_erasedZones.First.Value[i];
//                for(int j=0; j<m_erasedZones.First.Value.Count; j++)
//                {
//                    MaskPix pixel2 = m_erasedZones.First.Value[j];
//                    if(i != j && pixel1.m_x == pixel2.m_x && pixel1.m_y == pixel2.m_x)
//                        toRemove.Add(pixel1);
//                } // foreach pixel2
//            } // foreach pixel1
//
//            if(toRemove.Count > 0)
//            {
//                foreach(MaskPix pixel in toRemove)
//                    m_erasedZones.First.Value.Remove(pixel);
//            }
//            m_merged = true;
    }
    
    //-----------------------------------------------------
    public void UndoLastZone()
    {
        // Annuler si la limite d'annulations n'est pas atteinte,
        // ou bien s'il reste une seule zone qui n'a pas été fusionnée au préalable
        if(m_erasedZones.Count >= 1 && m_undoedZones.Count < m_maxUndoCount && m_backupZones.Count > 0/*|| (m_erasedZones.Count == 1 && !m_merged)*/)
        {
            m_undoedZones.Push(m_erasedZones.Last.Value);                // Ajouter la zone annulée aux zones annulées
            m_erasedZones.RemoveLast();                                  // Supprimer la zone annulée des zones d'effacement
            GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
            m_zoneToUpdate = (m_backupZones[m_backupZones.Count-1]).m_pixels;
            m_added = false;                                             // Mettre à jour l'image en rétablissant les pixels de l'ancienne zone
            m_backupZones.RemoveAt(m_backupZones.Count-1);               // Supprimer l'ancienne zone
        }
        else
            Debug.Log("EraserMask : impossible d'annuler (limite d'annulations (" + m_maxUndoCount + ") atteinte ?)");
    }

    //-----------------------------------------------------
    public void Redo()
    {
        if(m_undoedZones.Count > 0)
        {
            m_erasedZones.AddLast(m_undoedZones.Pop());
            m_backupZones.Add(CloneZone(m_erasedZones.Last.Value));

            GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
            m_zoneToUpdate = ((MaskedZone)m_erasedZones.Last.Value).m_pixels;
            m_added = true;
        }
        else
            Debug.Log("EraserMask : aucune action à refaire");
    }
    
    //-----------------------------------------------------
    // Remet l'alpha de tous les pixels d'eraserImage à 0
    public void ResetAlpha(bool apply, bool clearZones)
    {
        Texture2D childTex;
        Color pixCol;
        for(int i=0; i<transform.GetChildCount(); i++)
        {
            childTex = (Texture2D) transform.GetChild(i).GetComponent<GUITexture>().texture;
            for(int x=0 ; x<childTex.width ; x++)
            {   for(int y=0 ; y<childTex.height ; y++)
                {
                    if(childTex.GetPixel(x, y).a != 0)
                    {
                        pixCol = childTex.GetPixel(x, y);
                        pixCol.a = 0;
                        childTex.SetPixel(x, y, pixCol);
                    } // if pixel.a != 0
                } // for y
            } // for x
            
            if(apply)
                childTex.Apply();
        } // for each child
        
        if(clearZones) // Supprimer aussi les zones d'effacement (pour réinitialisation de l'outil gomme)
        {
            m_erasedZones.Clear();
            m_undoedZones.Clear();
			m_backupZones.Clear();
//            m_merged = false;
        }
    } // ResetAlpha

    //-----------------------------------------------------
    public void UpdateChildImage(int childID)
    {
        foreach(MaskedZone zone in m_erasedZones)
        {
            if(zone.m_images[childID])
            {
                StartCoroutine( UpdateEraserImage(zone.m_pixels, false) );
//                Debug.Log("Updating eraser image : "+childID);
            }
        }
    }

    //-----------------------------------------------------
    // Applique les masques à l'image d'effacement, pour rendre effective une modification de m_erasedZones
    // ATTENTION : aucune vérification concernant les pixels : ils doivent être bien situés dans l'image de fond.
    // ATTENTION2 : il faut avoir appelé au moins une fois la coroutine reset avant cette fonction
    //              (sinon les sous-images sont null)
	
//  private void UpdateEraserImage(List<MaskPix> modifiedZone, bool zoneAdded)
	private IEnumerator UpdateEraserImage(List<MaskPix> modifiedZone, bool zoneAdded)
    {
//		Camera.mainCamera.GetComponent<ObjInteraction>().enabled = false;
//        Texture2D refTex2D = (Texture2D) m_bgImg.guiTexture.texture;
//        int subDiv = (int) Mathf.Sqrt(transform.GetChildCount());
////        int childW = transform.GetChild(0).guiTexture.texture.width;
////        int childH = transform.GetChild(0).guiTexture.texture.height;
//		int childW = (int)transform.GetChild(0).guiTexture.pixelInset.width;
//      	int childH = (int)transform.GetChild(0).guiTexture.pixelInset.height;
//		
//        int childX = 0, childY = 0, childIndex = 0;
//        Texture2D childTex;
//        Color pixCol, pixColGrass; // pixCol grass seulement pour l'eraser
//        List<int> modifiedChildren = new List<int>();
//        
//        // -- Pour outil Gazon --
//        Texture2D grassBuffer = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
//        if(m_tool == GUIEditTools.EditTool.Grass)
//        {
////            GameObject.Find("grassSkybox").GetComponent<GrassHandler>().GetGrassImage();
//            int w = (int) m_bgImg.guiTexture.pixelInset.width;
//            int h = (int) m_bgImg.guiTexture.pixelInset.height;
//            // Lecture de la renderTexture sans les bandes blanches (le cas échéant)
//            grassBuffer.ReadPixels(new Rect((Screen.width - w)/2, (Screen.height - h)/2, w, h), 0, 0);
//        } // Outil gazon
//        
//        // Appliquer l'alpha des zones d'effacement stockées
//        int i=0;
//		int pxlLimit = 0;
//
////        System.DateTime d1 = System.DateTime.Now;
////        System.DateTime d2 = System.DateTime.Now;
////        System.DateTime d3 = System.DateTime.Now;
////        System.DateTime d4 = System.DateTime.Now;
//        Transform grassImgs = GameObject.Find("grassImages").transform;
////		Debug.Log(modifiedZone.Count+ " pixels in modified zone");
//        foreach(MaskPix pixel in modifiedZone)
//        {
//            childX = (pixel.m_x / childW);
//            childY = ((Screen.height - pixel.m_y - 1) / childH);    // -1 pour éviter un décalage
//            childIndex = childY * subDiv + childX;                  // Indice de l'image à éditer pour ce pixel
//			//Debug.Log("MODIF SUR IMAGE N°"+childIndex +" / " +transform.GetChildCount()+ " pxl pos "+pixel.m_x);
//			childTex = (Texture2D) transform.GetChild(childIndex).guiTexture.texture;
//
//            if(m_tool == GUIEditTools.EditTool.Eraser)
//            {
//                pixColGrass = ((Texture2D) grassImgs.GetChild(childIndex).guiTexture.texture).GetPixel(pixel.m_x, pixel.m_y);
//                if(pixColGrass.a == 0)
//				{													  // Si l'outil gazon n'a pas été utilisé sur ce pixel
//                    pixCol = refTex2D.GetPixel(pixel.m_x, pixel.m_y); // Pixel correspondant de la texture de référence
//				}
//				else                                                  // TODO gérer l'anti aliasing (avec un dégradé d'alpha)
//                    pixCol = pixColGrass;                             // Sinon, pixel correspondant de l'image "gazon"
//
//                pixCol.a = pixel.m_alpha;
//            } // Outil gomme
//            else/* if(m_tool == GUIEditTools.EditTool.Grass)*/  
//            {
//                if(zoneAdded)
//                {
//                    pixCol = grassBuffer.GetPixel(pixel.m_x, pixel.m_y);
//                    pixCol.a = pixel.m_alpha;
//                    if(((MaskPixRGB) pixel).m_r != -1)  // Si le pixel de la zone à ajouter contient des infos de couleur
//                    {                                   // (ie. dans le cas d'un redo)
//                        pixCol.r = ((MaskPixRGB) pixel).m_r;
//                        pixCol.g = ((MaskPixRGB) pixel).m_g;
//                        pixCol.b = ((MaskPixRGB) pixel).m_b;
//                    }
//                }
//                else
//                {
//                    pixCol.a = pixel.m_alpha;
//                    pixCol.r = ((MaskPixRGB) pixel).m_r;
//                    pixCol.g = ((MaskPixRGB) pixel).m_g;
//                    pixCol.b = ((MaskPixRGB) pixel).m_b;
//                }
//
//            } // Outil gazon
//            
//            if(zoneAdded && m_backupZones.Count>0)  // si ajout de zone, sauvegarder l'état de la zone avant de l'éditer
//            {
//                MaskPix pixTmp = (m_backupZones[m_backupZones.Count-1]).m_pixels[i];
//                Color bkCol = childTex.GetPixel(pixel.m_x, pixel.m_y); // Couleur du pixel actuel
//                pixTmp.m_alpha = (byte)bkCol.a;
//                if(m_tool == GUIEditTools.EditTool.Grass) // Outil gazon
//                {
////                    // -- Sauvegarde des couleurs de l'image actuelle à l'endroit de la nouvelle zone pour un undo --
////                    ((MaskPixRGB) pixTmp).m_r = bkCol.r;
////                    ((MaskPixRGB) pixTmp).m_g = bkCol.g;
////                    ((MaskPixRGB) pixTmp).m_b = bkCol.b;
////                    // -- Sauvegarde des couleurs de la nouvelle zone pour un undo+redo --
////                    ((MaskPixRGB) pixel).m_r = pixCol.r;
////                    ((MaskPixRGB) pixel).m_g = pixCol.g;
////                    ((MaskPixRGB) pixel).m_b = pixCol.b;
//                } // fin outil gazon
//                (m_backupZones[m_backupZones.Count-1]).m_pixels[i] = pixTmp; // Enregistrement de la zone actuelle
//            }
//            childTex.SetPixel(pixel.m_x, pixel.m_y, pixCol);
//            if(!modifiedChildren.Contains(childIndex))              // Noter les images modifiées
//                modifiedChildren.Add(childIndex);
//
//            i++;
//			pxlLimit ++;
//			if(pxlLimit > 5000)
//			{
//				yield return new WaitForEndOfFrame();
//				pxlLimit = 0;
//			}
//        } // foreach pixel
//		
//        if(m_tool == GUIEditTools.EditTool.Grass) // Outil gazon
//            GameObject.Find("grassSkybox").GetComponent<GrassHandler>().ReleaseTexture();
//
//
//        System.DateTime begin = System.DateTime.Now;
//        Debug.Log("Applying begin...");
//
//        foreach(int child in modifiedChildren)
//        {
//            ((Texture2D) transform.GetChild(child).guiTexture.texture).Apply(false); // Appliquer seulement les images modifiées
//            if(m_tool == GUIEditTools.EditTool.Grass)
//                GameObject.Find("eraserImages").GetComponent<EraserMask>().UpdateChildImage(child); // Mettre à jour la gomme si besoin
//			yield return new WaitForEndOfFrame();
//        }
//
//        Debug.Log("Ended applying... time spent="+(System.DateTime.Now-begin).ToString());
//		Resources.UnloadUnusedAssets();
//		System.GC.Collect();
//		GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
//		Camera.mainCamera.GetComponent<ObjInteraction>().enabled = true;
		yield return null;
    } // UpdateEraserImage()
    
    //-----------------------------------------------------
    public MaskedZone CloneZone(MaskedZone mzone)
    {
        List<MaskPix> zone = mzone.m_pixels;
        List<MaskPix> clone = new List<MaskPix>();

        if(m_tool == GUIEditTools.EditTool.Grass)
        {
            for(int i=0; i<zone.Count; i++)
                clone.Add((MaskPix)((MaskPixRGB)(zone[i])).Clone());
        }
        else
        {
            for(int i=0; i<zone.Count; i++)
                clone.Add((MaskPix)zone[i].Clone());
        }
        return new MaskedZone(clone, 0,0,0,0,0,0);
    }
    
    //-----------------------------------------------------
    // redimensionnement afin de créer une marge et donc un
    // mini espace de travail autour.
    // Trop compliqué donc abandonné pour l'instant.
//    public void ResizeImages(float percent)
//    {
//        GUITexture child;
//        Rect       pixIn;
//        float      subSections = Mathf.Sqrt(transform.GetChildCount());
//        
//        for(int i=0; i<transform.GetChildCount(); i++)
//        {
//            child = transform.GetChild(i).guiTexture;
//            pixIn = child.guiTexture.pixelInset;
//            
//            pixIn.x += func(i%subSections);
//            pixIn.y += func2(i-pixIn.x*subSections);
//            pixIn.width -= 8;//pixIn.width*percent/100;
//            pixIn.height -= 6;//pixIn.height*percent/100;
//            
//            child.guiTexture.pixelInset = pixIn;
//        }
//    } // ResizeImages();
//    
//    private int func(float x)
//    {
//        if(x == 0)
//            return 16;
//        else if(x == 1)
//            return 8;
//        else if(x == 2)
//            return 0;
//        else if(x==3)
//            return -8;
//        else
//            return 0;
//    }
//    
//    private int func2(float x)
//    {
//        if(x == 0)
//            return -6;
//        else if(x == 1)
//            return 0;
//        else if(x == 2)
//            return 6;
//        else if(x==3)
//            return 12;
//        else
//            return 0;
//    }
//    
//    //-----------------------------------------------------
//    public void ResetImages()
//    {
//        GUITexture child;
//        Rect       pixIn;
//        float      subSections = Mathf.Sqrt(transform.GetChildCount());
//        float      w = m_guiTexRef.pixelInset.width/transform.GetChildCount();
//        float      h = m_guiTexRef.pixelInset.height/transform.GetChildCount();
//        
//        for(int i=0; i<transform.GetChildCount(); i++)
//        {
//            child = transform.GetChild(i).guiTexture;
//            pixIn = child.guiTexture.pixelInset;
//            
//            pixIn.x = i%subSections * w;
//            pixIn.y = i-i%subSections*subSections * h;
//            pixIn.width = w;
//            pixIn.height = h;
//            
//            child.guiTexture.pixelInset = pixIn;
//        }
//    }
    
    //-----------------------------------------------------
    public void SetTool(GUIEditTools.EditTool newTool)
    {
        m_tool = newTool;
    }

    //-----------------------------------------------------
    // Supprimer toute possibilité d'annulation (utilisé)
    // pour l'outil gazon une fois que la texture a changé
    // (impossible de récupérer simplement la couleur du pixel à rétablir)
    public void ReinitUndo()
    {
        while(m_erasedZones.Count > 1)
            MergeZones();

        m_backupZones.Clear();
        m_undoedZones.Clear();
    }

    //-----------------------------------------------------
    // Utilisé pour les sous-images du gazon, car la caméra 3D n'a pas le même cadre que
    // la caméra background.
    public void SetOffsets(int xoffset, int yoffset)
    {
        Rect inset;
		int newWidth = -1;
		Texture2D tmp = new Texture2D(1,1,TextureFormat.RGBA32,false);
		tmp.SetPixel(0,0,new Color32(0,0,0,0));
		
//        if(m_guiTexRef.pixelInset.width < Screen.width)
//		{
//			Debug.Log("NewTew Used");
//			newWidth = Mathf.CeilToInt(m_guiTexRef.pixelInset.width/ m_subDivCount);
//			tmp.Resize(newWidth,m_baseTexture.height);
//			tmp.Apply();
//			for(int i=0; i<transform.GetChildCount(); i++)
//        	{
//				transform.GetChild(i).guiTexture.texture = (Texture)tmp;
//			}
//		}
		
        for(int i=0; i<transform.GetChildCount(); i++)
        {
            inset = transform.GetChild(i).GetComponent<GUITexture>().pixelInset;
            inset.x += xoffset - m_offsetx;     // On annule l'ancien offset et on applique le nouveau
            inset.y += yoffset - m_offsety;
//			if(newWidth != -1)
//			{
//				inset.x = (i%m_subDivCount)*newWidth + xoffset;
//				inset.width = newWidth;
//				
//			}
//			else
//			{
//				inset.width = 256;
//				transform.GetChild(i).guiTexture.texture = m_baseTexture;
//			}
            transform.GetChild(i).GetComponent<GUITexture>().pixelInset = inset;
        }
        
        m_offsetx = xoffset;
        m_offsety = yoffset;
    }

    //-----------------------------------------------------
	public void setVisible(bool b)
	{
		foreach(Transform t in transform)
		{
			t.GetComponent<GUITexture>().enabled = b;	
		}
	}
	
	//-----------------------------------------------------
	/*
	 *  prépare les MaskedZone pour le save
	 */
	public MaskedZone setUp4Save()
	{
		if(m_erasedZones.Count == 0)
			return null;
		MaskedZone toOut = m_erasedZones.First.Value;
		foreach(MaskedZone mz in m_erasedZones)
		{
			if(!m_erasedZones.First.Value.Equals(mz))
				toOut.m_pixels.AddRange(mz.m_pixels);
			if(m_tool == GUIEditTools.EditTool.Eraser)
			{
				for(int i=0; i<toOut.m_images.Length; i++)
	            	toOut.m_images[i] = toOut.m_images[i] || mz.m_images[i];
			}
		}
		return toOut;
	}
	
	//-----------------------------------------------------
	/*
	 * Chargement du mask
	 */
	public void loadMaskZone(MaskedZone mz)
	{
//		m_erasedZones.Clear();
		m_erasedZones.AddLast(mz);

        GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		m_zoneToUpdate = ((MaskedZone)m_erasedZones.Last.Value).m_pixels;

		m_added = false;
	}
	
	public IEnumerator loadMZ(MaskedZone mz)
	{
		m_zoneToUpdate = null;
		m_erasedZones.AddLast(mz);

        GameObject.Find("MainScene").GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
		//m_zoneToUpdate = mz.m_pixels;//((MaskedZone)m_erasedZones.Last.Value).m_pixels;

		m_added = false;
		
		StartCoroutine( UpdateEraserImage(mz.m_pixels, m_added) );
				
		yield return true;
	}
	
	public void UpdateMaskColor(Color maskColor)
	{
		for (int i=0; i<transform.GetChildCount(); i++)
		{
			Transform child = transform.GetChild(i);
			GUITexture guiTex = child.GetComponent<GUITexture>();
			if(guiTex!=null)
				guiTex.color = maskColor;
		}
	}
	
} // class EraserMask
