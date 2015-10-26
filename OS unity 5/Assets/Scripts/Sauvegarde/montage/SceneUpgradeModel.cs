using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SceneUpgradeModel
{
	/* SceneUpgradeModel
	 * contient les données de:
	 * # inpainting
	 * # gomme
	 * # gazon
	 * */
	
//	byte[] inPainting;
//	byte[] gomme;
//	byte[] gazon;
//	
//	public Texture2D ip;
//	public Texture2D go;
//	public Texture2D ga;
	
//	public EraserMask.MaskedZone gommeZone;
//	public EraserMask.MaskedZone gazonZone;

    public GameObject m_mainScene;
	public GameObject gommeNode;
	public GameObject gazonNode;
    public GameObject m_grassSynthNode;
	
//	public bool gotGomme;
//	public bool gotGazon;
	
	public SceneUpgradeModel()
	{
        m_mainScene = GameObject.Find("MainScene");
		gommeNode = GameObject.Find("eraserImages");
		gazonNode = GameObject.Find("grassImages");
        m_grassSynthNode = GameObject.Find("grassSkybox");
	}
	
	//Update's
	public void update(Texture2D t)
	{
//		inPainting = t.EncodeToPNG();
//		gomme =  t.EncodeToPNG();
//		gazon =  t.EncodeToPNG();
//		
//		Debug.Log(inPainting.Length+ " " +gomme.Length+ " " +gazon.Length);
	}
	
	//Save/Load
	public void save(BinaryWriter buf)
	{
		try{
//		buf.Write(inPainting.Length);
//		buf.Write(inPainting);
//		
//		buf.Write(gomme.Length);
//		buf.Write(gomme);
//		
//		buf.Write(gazon.Length);
//		buf.Write(gazon);

        // -- EraserV2 --
        List<float> e_data = gommeNode.GetComponent<EraserV2>().GetSaveData();

        buf.Write(e_data.Count);            // Taille du bloc EraserV2
        for(int i=0; i<e_data.Count; i++)
            buf.Write(e_data[i]);           // Données
		//tentative save texture
		/*foreach(Texture2D tex in GetComponent<GrassHandler>().GetSynthTex())
		{
				byte[] bytes = tex.EncodeToPNG();
				buf.Write(bytes);
		}*/
      //   -- GrassV2 : Textures synthétisées --
//        int[] synthTexPoints = m_grassSynthNode.GetComponent<GrassHandler>().GetSaveData();
//        buf.Write(synthTexPoints.Length);
//        for(int i=0; i<synthTexPoints.Length; i++)
//            buf.Write(synthTexPoints[i]);

        // -- GrassV2 : zones gazonnées --
        List<float> g_data = gazonNode.GetComponent<GrassV2>().GetZonesData();
        buf.Write(g_data.Count);            // Taille du bloc GrassV2
        for(int i=0; i<g_data.Count; i++)
            buf.Write(g_data[i]);           // Données
        Debug.Log ("written "+g_data.Count+" grass data !");

        List<int> g_texIDs = gazonNode.GetComponent<GrassV2>().GetZoneTexID();
        buf.Write(g_texIDs.Count);          // Taille du bloc "IDs textures GrassV2"
        for(int i=0; i<g_texIDs.Count; i++)
            buf.Write(g_texIDs[i]);
        Debug.Log ("written "+g_texIDs.Count+" grass tex id !");

        List<byte[]> g_synthTexGen = gazonNode.GetComponent<GrassV2>().GetZoneSynthTexBase();
        buf.Write(g_synthTexGen.Count);          // Taille du bloc "IDs textures GrassV2"
        for(int i=0; i<g_texIDs.Count; i++)
        {
            byte[] curTex = g_synthTexGen[i];
            buf.Write(curTex.Length);
            for(int j=0; j<curTex.Length; j++)
                buf.Write(curTex[j]);
        }
        Debug.Log ("written "+g_synthTexGen.Count+" grass tex gen !");

//        {
//    		gommeZone = gommeNode.setUp4Save();
//    		gazonZone = gazonNode.setUp4Save();
//
//    		if(gommeZone == null)
//    			buf.Write(false);
//    		else
//    		{
//    			buf.Write(true);
//    			saveMaskZone(gommeZone,buf,true);
//    		}
//
//    		if(gazonZone == null)
//    			buf.Write(false);
//    		else
//    		{
//    			buf.Write(true);
//    			saveMaskZone(gazonZone,buf,false);
//    		}
//        }
		}
		catch(UnityException ue){
			Debug.Log ("error saving sceneupgrademodel ;"+ue.ToString());
		}

	}
	
	public bool load(BinaryReader buf)
	{
		Debug.Log ("load textures");
//		int ipLength = buf.ReadInt32();
//		inPainting = buf.ReadBytes(ipLength);
//		ip = new Texture2D(1024,768);
//		ip.LoadImage(inPainting);
//		
//		int gomLength = buf.ReadInt32();
//		gomme = buf.ReadBytes(gomLength);
//		go = new Texture2D(1024,768);
//		go.LoadImage(gomme);
//		
//		int gazLength = buf.ReadInt32();
//		gazon = buf.ReadBytes(gazLength);
//		ga = new Texture2D(1024,768);
//		ga.LoadImage(gazon);

        /*if(Montage.cdm.HasVersion() &&
                               !LibraryLoader.numVersionInferieur(Montage.cdm.versionSave, "1.3.3"))
        {*/
            bool noGrassNoEraser = true;

            // -- EraserV2 --
            int count = buf.ReadInt32();
            if(count > 0)
            {
                noGrassNoEraser = false;
                float[] data = new float[count];
//                Debug.Log (" =======> "+count+" data to load");
                for(int i=0; i<count; i++)
                    data[i] = buf.ReadSingle();

                gommeNode.GetComponent<EraserV2>().Load(data);
            }

            m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(true);
	
		
			// -- GrassV2 : Textures synthétisées --
 //           count = buf.ReadInt32();
  //          if(count > 0)
  //          {
 //               int[] dataTex = new int[count];
 //               for(int i=0; i<dataTex.Length; i++)
 //                   dataTex[i] = buf.ReadInt32();

  //              m_grassSynthNode.GetComponent<GrassHandler>().Load(dataTex);
//            }

            // -- GrassV2 --
            count = buf.ReadInt32();
            if(count > 0)
            {
                noGrassNoEraser = false;
                float[] data = new float[count];
//                Debug.Log (" =======> "+count+" grassV2 data to load");
                for(int i=0; i<count; i++)
                    data[i] = buf.ReadSingle();

                count = buf.ReadInt32();
                if(count > 0)
                {
                    int[] texIDs = new int[count];
//                    Debug.Log ("=======> "+count+" grassV2 texIds to load");
                    for(int i=0; i<count; i++)
                        texIDs[i] = buf.ReadInt32();

                    count = buf.ReadInt32();
                    if(count > 0)
                    {
                        List<byte[]> texsGen = new List<byte[]>();
                        for(int i=0; i<count; i++)
                        {
                            int count2 = buf.ReadInt32();
                            byte[] tex = new byte[count2];
                            for(int j=0; j<count2; j++)
                                tex[j] = buf.ReadByte();
                            texsGen.Add(tex);
                        }
//                        Debug.Log ("=======> "+count+" grassV2 texsGen to load");
    
                        gazonNode.GetComponent<GrassV2>().Load(data, texIDs, texsGen);
                    }
                    // TODO else loading error : invalid file
                }
                // TODO else loading error : invalid file
            }

            if(noGrassNoEraser)
                m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
        //}
        /*else // EraserV1 (données lues mais supprimées :/ )
        {
    		bool gotGomme = buf.ReadBoolean();
    		if(gotGomme)
    		{
    			/*gommeZone = LoadMask(buf,true);
    //			gommeNode.loadMaskZone(gommeZone);
    		}

    		bool gotGazon = buf.ReadBoolean();
    		if(gotGazon)
    	/	{
    			/*gazonZone = LoadMask(buf,false);
    //			gazonNode.loadMaskZone(gazonZone);
    		}

            m_mainScene.GetComponent<PleaseWaitUI>().SetDisplayIcon(false);
        }*/
		return true;
//		gommeNode;
	}
	
	
	//AUX. FCN.
	
//	public void saveMaskZone(EraserMask.MaskedZone mz,BinaryWriter buf,bool isGomme)
//	{
//		buf.Write(mz.m_pixels.Count);
//		
//		if(isGomme)
//		{
//			buf.Write(mz.m_images.Length);
//			foreach(bool b in mz.m_images)
//			{
//				buf.Write(b);	
//			}
//			
//			foreach(MaskPix mp in mz.m_pixels)
//			{
//				buf.Write(mp.m_x);
//				buf.Write(mp.m_y);
//				buf.Write(mp.m_alpha);
//			}
//		}
//		else
//		{
//			foreach(MaskPixRGB mp in mz.m_pixels)
//			{
//				buf.Write(mp.m_x);
//				buf.Write(mp.m_y);
//				buf.Write(mp.m_r);
//				buf.Write(mp.m_g);
//				buf.Write(mp.m_b);
//				buf.Write(mp.m_alpha);
//			}
//		}
//	}
	
	public void LoadMask(BinaryReader buf,bool isGomme)
	{
		bool[] images;
		List<MaskPix> pixs = new List<MaskPix>();
		
		int pixCount = buf.ReadInt32();

		if(isGomme)
		{
			int imgCount = buf.ReadInt32();
			images = new bool[imgCount];
			
			for(int i=0;i<imgCount;i++)
			{
				images[i] = buf.ReadBoolean();	
			}

			for(int p=0;p<pixCount;p++)
			{
				MaskPix mp = new MaskPix(buf.ReadInt32(),buf.ReadInt32(),buf.ReadSingle());
				pixs.Add(mp);
			}

		}
		else
		{
			images = null;
			for(int p=0;p<pixCount;p++)
			{
				MaskPixRGB mp = new MaskPixRGB(buf.ReadInt32(),buf.ReadInt32(),
					buf.ReadSingle(),buf.ReadSingle(),buf.ReadSingle(),buf.ReadSingle());
				pixs.Add(mp);
			}
		}
		
//		EraserMask.MaskedZone mz = new EraserMask.MaskedZone(pixs,images);
//		return mz;
	}
}
