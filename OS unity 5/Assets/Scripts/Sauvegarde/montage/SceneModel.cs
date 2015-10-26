using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SceneModel
{
	/* SceneModel
	 * contient les données de:
	 * # caméra
	 * # gyroscope (edit:pas besoin, données enregistré dans H-LIPS)
	 * # fond
	 * # eclairage
	 * */

    //-----------------------------------------------------
    public struct M2D   // Sauvegarde mode2D
    {
        public Rect      m_orgBgImgPixIn;
        public Rect      m_savedBgImgPixIn;
        public Vector2   m_oldScreenSize;
        public float     m_zoomObj;
        public Vector2   m_translaObj;
        public float     m_rotaObj;
        public Texture2D m_loadedBg;
        public bool      m_empty;
        public float[]   m_lineData;
        public bool      m_oldSave;

        public M2D(bool empty = true)
        {
            m_orgBgImgPixIn = new Rect();
            m_savedBgImgPixIn = new Rect();
            m_oldScreenSize = new Vector2();
            m_zoomObj    = 0f;
            m_translaObj = new Vector2();
            m_rotaObj    = 0f;
            m_loadedBg   = null;
            m_empty      = true;
            m_lineData   = null;
            m_oldSave    = false;
        }

        public M2D(Rect orgBgImgRect, Rect savedBgImgRect, Vector2 oldScrSize, float zoomObj,
                                  Vector2 translaObj, float rotaObj, float[] lineData, Texture2D plan, bool oldSave = false)
        {
            m_orgBgImgPixIn = new Rect(orgBgImgRect);
            m_savedBgImgPixIn = new Rect(savedBgImgRect);
            m_oldScreenSize = new Vector2(oldScrSize.x, oldScrSize.y);
            m_zoomObj    = zoomObj;
            m_translaObj = translaObj;
            m_rotaObj    = rotaObj;
            m_loadedBg   = plan;
            m_empty      = false;
            m_lineData   = lineData;
            m_oldSave    = oldSave;
        }

        public void Set(Rect orgBgImgRect, Rect savedBgImgRect, Vector2 oldScrSize, float zoomObj,
                                  Vector2 translaObj, float rotaObj, float[] lineData, Texture2D plan, bool oldSave = false)
        {
            m_orgBgImgPixIn.Set(orgBgImgRect.x, orgBgImgRect.y, orgBgImgRect.width, orgBgImgRect.height);
            m_savedBgImgPixIn.Set (savedBgImgRect.x, savedBgImgRect.y, savedBgImgRect.width, savedBgImgRect.height);
            m_oldScreenSize.Set(oldScrSize.x, oldScrSize.y);
            m_zoomObj    = zoomObj;
            m_translaObj = translaObj;
            m_rotaObj    = rotaObj;
            m_loadedBg   = plan;
            m_empty      = false;
            m_lineData   = lineData;
            m_oldSave    = oldSave;
        }

        public void Set(Rect orgBgImgRect, Rect savedBgImgRect, Vector2 oldScrSize, float zoomObj,
                                                  Vector2 translaObj, float[] lineData, float rotaObj, bool oldSave = false)
        {
            Set(orgBgImgRect, savedBgImgRect, oldScrSize, zoomObj, translaObj, rotaObj, lineData, null, oldSave);
        }
    } // M2D

    //-----------------------------------------------------
	public struct Hlips // Structure de stockage des réglages de la grille
	{
		public float m_h, m_l, m_i, m_p, m_s, m_t;
		
		public Hlips(float h, float l, float i, float p, float s)
		{
			m_h=h; m_l=l; m_i=i; m_p=p; m_s=s; m_t=0;
		}
		public Hlips(float h, float l, float i, float p, float s, float t)
		{
			m_h=h; m_l=l; m_i=i; m_p=p; m_s=s; m_t=t;
		}
	};
	
    //-----------------------------------------------------
	public struct Ahibp // Structure de stockage pour les réglages de la lumière
	{
		public float m_a, m_h, m_i, m_b, m_p, m_img_r, m_img_g, m_img_b;
		
		public Ahibp(float a, float h, float i, float b, float p, float img_r, 
			float img_g, float img_b)
		{
			m_a=a; m_h=h; m_i=i; m_b=b; m_p=p; m_img_r=img_r; m_img_g=img_g; m_img_b=img_b;
		}
	};
	
    //-----------------------------------------------------
	public struct Reflex // Structure de stockage pour les réglages de la reflexion du sol
	{
		public int m_id;
		public float m_rate;
		
		public Reflex(int id,float rate)
		{
			m_id = id;
			m_rate = rate;
		}
	}
	
	Hlips m_originalState;
	
	List<Hlips> m_cameraData;   // Liste d'états pour les réglages de la grille (hauteur, perspective, etc.)
	List<Ahibp> m_lightData;	// Liste d'états pour les réglages de la lumière (intensité, heure, etc.)
	List<Ahibp> m_shadowData;   //liste des undo pour la shadow
	
	List<Hlips> m_redoCam;
	List<Ahibp> m_redoLight;
	List<Ahibp> m_redoShadow;
	
	Reflex m_reflex;

    M2D    m_mode2D;

	private const int   c_maxUndoCount  = 30; // TODO Rendre cette variable uniforme pour toutes les fonctionnalités de OS3D

	//background
	public Texture2D background;
	bool isFromPhototheque;
	string backgroundPath;
	
	private bool _nightMode = false;
	
	WWW www;
	
    //-----------------------------------------------------
	public SceneModel()
	{
		m_cameraData = new List<Hlips>();
		m_lightData  = new List<Ahibp>();
		m_shadowData = new List<Ahibp>();
		
		m_redoCam = new List<Hlips>();
		m_redoLight = new List<Ahibp>();
		m_redoShadow = new List<Ahibp>();
		
		m_reflex = new Reflex(6,0);
        m_mode2D = new M2D();
		//m_originalState = new Hlips(0,0,0,0,0);
	}
	
	#region Camera
	//-----------------------------------------------------
	// Ajout d'un état de configuration pour les réglages de la grille
	public void AddCameraState(float h, float l, float i, float p, float s)
	{
		AddCameraState(h, l, i, p, s,0);
	}
	
	public void AddCameraState(float h, float l, float i, float p, float s,float t)
	{
		if(m_cameraData.Count == c_maxUndoCount)
			m_cameraData.RemoveAt(0);
		
		m_cameraData.Add(new Hlips(h, l, i, p, s,t));
		m_redoCam.Clear();
//		Debug.Log("CameraSatte Added : "+m_cameraData.Count+", s="+s);
	}
	
	public void setOriginalState(float h, float l, float i, float p, float s,float t)
	{
		m_originalState = new Hlips(h,l,i,p,s,t);
	}
	
	//-----------------------------------------------------
	// Get du dernier etat du réglage de la grille
	public Hlips getCamData()
	{
	    return m_cameraData[m_cameraData.Count-1];
	}

    public int getCamDataCount()
    {
        return m_cameraData.Count;
    }
	
	//-----------------------------------------------------
	// Retour à l'état précédent pour les réglages de la grille
	public Hlips UndoCamera()
	{
//		Debug.Log("UndoCameraState : "+m_cameraData.Count+", sret="+m_cameraData[m_cameraData.Count-1].m_s);
		
		if(m_cameraData.Count > 1)
		{
			if(m_redoCam.Count < c_maxUndoCount)
				m_redoCam.Add(m_cameraData[m_cameraData.Count-1]);
			else
			{
				m_redoCam.RemoveAt(0);
				m_redoCam.Add(m_cameraData[m_cameraData.Count-1]);
			}
			m_cameraData.RemoveAt(m_cameraData.Count-1);
		}
		
		return m_cameraData[m_cameraData.Count-1];
	}
	
	//-----------------------------------------------------
	// Refait l'état anuulé des réglages de la grille
	public Hlips RedoCamera()
	{
		if(m_redoCam.Count>0)
		{
			m_cameraData.Add(m_redoCam[m_redoCam.Count-1]);
			m_redoCam.RemoveAt(m_redoCam.Count-1);
		}
		return m_cameraData[m_cameraData.Count-1];
	}
	
	//Update's
//	public void updateCamera(float l,float i,float h,float p,int nb)
//	{
//		if(m_cameraData.Count<5)
//		{
//			m_cameraData.Add(l);
//			m_cameraData.Add(i);
//			m_cameraData.Add(h);
//			m_cameraData.Add(p);
//			m_cameraData.Add(nb);
//		}
//		else
//		{
//			m_cameraData[0] = l;
//			m_cameraData[1] = i;
//			m_cameraData[2] = h;
//			m_cameraData[3] = p;
//			m_cameraData[4] = nb;
//		}
//			
//	}
//	
//	public void updateLight(float a,float h,float i,float b,float p)
//	{
//		if(m_lightData.Count<5)
//		{
//			m_lightData.Add(a);
//			m_lightData.Add(h);
//			m_lightData.Add(i);
//			m_lightData.Add(b);
//			m_lightData.Add(p);
//		}
//		else
//		{
//			m_lightData[0] = a;
//			m_lightData[1] = h;
//			m_lightData[2] = i;
//			m_lightData[3] = b;
//			m_lightData[4] = p;
//		}
//	}
	
	#endregion
	
	#region Light
	//-----------------------------------------------------
	// Ajout d'un état de configuration de l'éclairage
	public void AddLightState(float a,float h,float i,float b,float p, float img_r, 
			float img_g, float img_b)
	{
		AddLightState(true,  a, h, i, b, p, img_r, img_g, img_b);
		AddLightState(false,  a, h, i, b, p, img_r, img_g, img_b);
	}
	
	public void AddLightState(bool isLight, float a,float h,float i,float b,float p, float img_r, 
			float img_g, float img_b)
	{

		if(isLight)
		{
			if(m_lightData.Count == c_maxUndoCount)
				m_lightData.RemoveAt(0);
			
			m_lightData.Add(new Ahibp(a, h, i, b, p, img_r, img_g, img_b));
			m_redoLight.Clear();
		}
		else
		{
			if(m_shadowData.Count == c_maxUndoCount)
			m_shadowData.RemoveAt(0);
		
			m_shadowData.Add(new Ahibp(a, h, i, b, p, img_r, img_g, img_b));
			m_redoShadow.Clear();
		}
	}
	
	//-----------------------------------------------------
	// Retour à l'état précédent pour les réglages de l'éclairage
	public Ahibp UndoLight(bool isLight)
	{
		if(isLight)
		{
			if(m_lightData.Count > 1)
			{
				if(m_redoLight.Count < c_maxUndoCount)
					m_redoLight.Add(m_lightData[m_lightData.Count-1]);
				else
				{
					m_redoLight.RemoveAt(0);
					m_redoLight.Add(m_lightData[m_lightData.Count-1]);
				}
				m_lightData.RemoveAt(m_lightData.Count-1);
			}
			return m_lightData[m_lightData.Count-1];
		}
		else
		{
			if(m_shadowData.Count > 1)
			{
				if(m_redoLight.Count < c_maxUndoCount)
					m_redoShadow.Add(m_shadowData[m_shadowData.Count-1]);
				else
				{
					m_redoShadow.RemoveAt(0);
					m_redoShadow.Add(m_shadowData[m_shadowData.Count-1]);
				}
				m_shadowData.RemoveAt(m_shadowData.Count-1);
			}
			return m_shadowData[m_shadowData.Count-1];
		}
	}
	
	public Ahibp RedoLight(bool isLight)
	{
		if(isLight)
		{
			if(m_redoLight.Count>0)
			{
				m_lightData.Add(m_redoLight[m_redoLight.Count-1]);
				m_redoLight.RemoveAt(m_redoLight.Count-1);
			}
			return m_lightData[m_lightData.Count-1];
		}
		else
		{
			if(m_redoShadow.Count>0)
			{
				m_shadowData.Add(m_redoShadow[m_redoShadow.Count-1]);
				m_redoShadow.RemoveAt(m_redoShadow.Count-1);
			}
			return m_shadowData[m_shadowData.Count-1];
		}
	}
	
	//-----------------------------------------------------
	public Ahibp getLightData()
	{
		return m_lightData[m_lightData.Count-1];
	}
	public Ahibp getShadowData()
	{
		return m_shadowData[m_shadowData.Count-1];
	}
	
	public void SetNightMode(bool night)
	{
		_nightMode = night;
	}
	public bool IsNightMode()
	{
		return _nightMode;
	}
	
	#endregion
	
	#region Reflex
	public void setReflex(int id,float rate)
	{
		m_reflex.m_id = id;
		m_reflex.m_rate = rate;
	}
	
	public Reflex getReflexData()
	{
		return m_reflex;	
	}
	#endregion
	
	#region Background
	//-----------------------------------------------------
	public void updateFond(Texture tex, bool isFromPhtq, string path)
	{
//		background = (Texture2D)tex;
//		isFromPhototheque = isFromPhtq;
//		backgroundPath = path;
//		GameObject.Find("MainScene").GetComponent<Montage>().reset();
		updateFond((Texture2D)tex,isFromPhtq,path);
	}
	public void updateFond(Texture2D tex, bool isFromPhtq, string path)
	{
		background = tex;

		isFromPhototheque = isFromPhtq;
		backgroundPath = path;
		/*if((background.width>1920.0f) || (background.height>1920.0f))
		{
			Rect resizeRect = UsefulFunctions.ResizeImagePreservingRatio(background.width,background.height,1920,1920);
			//bool ok = background.Resize((int)resizeRect.width, (int)resizeRect.height);
			//if(ok)
			//	background.Apply();
			//Debug.Log("Resize OK  : " + ok);
			UsefulFunctions.Bilinear (background, (int)resizeRect.width, (int)resizeRect.height);
			
		}*/
		GameObject.Find("MainScene").GetComponent<Montage>().reset();
	}
	
	//-----------------------------------------------------
	public Texture2D getBackground()
	{
		return background;
	}
	
	public void setBackground(Texture2D tex)
	{
		background = null;
		background = tex;
		GameObject.Find("MainScene").GetComponent<Montage>().reset();
	}
	#endregion


    #region Mode2D
    //-----------------------------------------------------
    public void SetMode2D(Rect orgBgImgRect, Rect savedBgImgRect, Vector2 oldScrSize, float zoomObj,
                            Vector3 translaObj, float rotaObj, float[] lineData, Texture2D plan)
    {
        Vector2 tmpTransla = new Vector2(translaObj.x, translaObj.y);
        m_mode2D.Set(orgBgImgRect, savedBgImgRect, oldScrSize, zoomObj, tmpTransla, rotaObj, lineData, plan);
    }

    //-----------------------------------------------------
    public M2D GetMode2D()
    {
        return m_mode2D;
    }

    #endregion

	#region Save/Load
	//Save/Load
	
	private Ahibp mergeLightNShadow()
	{
//		Ahibp tmpLight = new Ahibp(0,0,0,0,0);
//		Ahibp tmpShadow = new Ahibp(0,0,0,0,0);
//		
//		bool isLight = false;
//		bool isShadow = false;
//		
//		if(m_lightData.Count>0)
//		{
//			isLight = true;
//			tmpLight = m_lightData[m_lightData.Count-1];
//		}
//		if(m_shadowData.Count>0)
//		{
//			isShadow = true;
//			tmpShadow = m_shadowData[m_shadowData.Count-1];
//		}
//		
//		if(isLight && isShadow) // y en a au moins un de chaque
//		{
//			if(tmpLight.m_a == tmpShadow.m_a && tmpLight.m_h == tmpShadow.m_h && tmpLight.m_i == tmpShadow.m_i)
//			{
//				return tmpShadow;
//			}
//			else
//				return tmpLight;
//				
//		}
//		else if(isLight || isShadow) // y en a un des deux
//		{
//			if(!isLight)
//				return tmpShadow;
//			else
//				return tmpLight;
//		}
//		else //y en a aucun --> pb
//		{
//			Debug.Log("No Saved Data");
//			return new Ahibp(0,0,0,0,0);;
//		}
		
		Ahibp toReturn = new Ahibp(0,0,0,0,0, 0, 0, 0);
		
		if(m_lightData.Count>0)
		{
			toReturn = m_lightData[m_lightData.Count-1];	
		}
		if(m_shadowData.Count>0)
		{
			toReturn.m_b = m_shadowData[m_shadowData.Count-1].m_b;
			toReturn.m_p = m_shadowData[m_shadowData.Count-1].m_p;	
		}
		return toReturn;
	}


	
	public void save(BinaryWriter buf)
	{
		try{
		//camera
		Hlips tosave = m_cameraData[m_cameraData.Count-1];
		
		buf.Write((float)tosave.m_h);
		buf.Write((float)tosave.m_l); 
		buf.Write((float)tosave.m_i); 
		buf.Write((float)tosave.m_p); 
		buf.Write((float)tosave.m_s);
		buf.Write((float)tosave.m_t);
		
		//Sauvegarde données originales
		buf.Write((float)m_originalState.m_h);
		buf.Write((float)m_originalState.m_l); 
		buf.Write((float)m_originalState.m_i); 
		buf.Write((float)m_originalState.m_p); 
		buf.Write((float)m_originalState.m_s);
		buf.Write((float)m_originalState.m_t);
		
		Debug.Log("SAVING SCENE tosave "+ tosave.m_h + "  " + tosave.m_l + "  " + tosave.m_i + "  " + tosave.m_p + "  " + tosave.m_s + "  " + tosave.m_t);
			Debug.Log("SAVING SCENE originalState "+ m_originalState.m_h + "  " + m_originalState.m_l + "  " + m_originalState.m_i + "  " + m_originalState.m_p + "  " + m_originalState.m_s + "  " + m_originalState.m_t);
			//Debug.Log ("SAVING SCENE "+tosave.m_h+" "+tosave.m_l+" "+tosave.m_i);
		
//		//light
		// Ahibp liteToSave = m_lightData[m_lightData.Count-1];
		Ahibp liteToSave = mergeLightNShadow();
		
		buf.Write((float)liteToSave.m_a); 
		buf.Write((float)liteToSave.m_h); 
		buf.Write((float)liteToSave.m_i); 
		buf.Write((float)liteToSave.m_b); 
		buf.Write((float)liteToSave.m_p);
		
		buf.Write((float)liteToSave.m_img_r);
		buf.Write((float)liteToSave.m_img_g);
		buf.Write((float)liteToSave.m_img_b);
		buf.Write((bool)_nightMode);
//		
		//Reflex
		buf.Write(m_reflex.m_id);
		buf.Write(m_reflex.m_rate);
		
		//background
		buf.Write(GameObject.Find("backgroundImage").transform.localScale.y);
		if(isFromPhototheque)
		{
			buf.Write(isFromPhototheque);
			buf.Write(backgroundPath);
		}
		else
		{
			buf.Write(isFromPhototheque);

			byte[] bytes = background.EncodeToJPG();
			buf.Write(bytes.Length);
			buf.Write(background.width);
			buf.Write(background.height);
			buf.Write(bytes);
			
		}
		
        // -- Mode2D --
        buf.Write(m_mode2D.m_empty);
        if(!m_mode2D.m_empty)
        {
            buf.Write(m_mode2D.m_orgBgImgPixIn.x);
            buf.Write(m_mode2D.m_orgBgImgPixIn.y);
            buf.Write(m_mode2D.m_orgBgImgPixIn.width);
            buf.Write(m_mode2D.m_orgBgImgPixIn.height);
            buf.Write(m_mode2D.m_savedBgImgPixIn.x);
            buf.Write(m_mode2D.m_savedBgImgPixIn.y);
            buf.Write(m_mode2D.m_savedBgImgPixIn.width);
            buf.Write(m_mode2D.m_savedBgImgPixIn.height);
            buf.Write(m_mode2D.m_oldScreenSize.x);
            buf.Write(m_mode2D.m_oldScreenSize.y);
            buf.Write(m_mode2D.m_zoomObj);
            buf.Write(m_mode2D.m_translaObj.x);
            buf.Write(m_mode2D.m_translaObj.y);
            buf.Write(m_mode2D.m_rotaObj);
//            buf.Write(m_mode2D.m_zoomImg);
//DEBUG_SAVE
            
          if(m_mode2D.m_lineData != null)
            {
                foreach(float f in m_mode2D.m_lineData)
                    buf.Write(f);
            }
            

            if(m_mode2D.m_loadedBg != null)
            {
                buf.Write(true);
				byte[] bytes = m_mode2D.m_loadedBg.EncodeToJPG();
                buf.Write(bytes.Length);
                buf.Write(m_mode2D.m_loadedBg.width);
                buf.Write(m_mode2D.m_loadedBg.height);
                buf.Write(bytes);
            }
            else
                buf.Write(false);
        }
		}
		catch(UnityException e){
			Debug.Log ("error saving scene model"+e.ToString());
		}


	} // save()
	
	public void load(BinaryReader buf,bool completeLoad, string version)
	{
		//camera
		//chargement données montage 
		Hlips toload;
		toload.m_h = buf.ReadSingle();
		toload.m_l = buf.ReadSingle();
		toload.m_i = buf.ReadSingle();
		toload.m_p = buf.ReadSingle();
		toload.m_s = buf.ReadSingle();
		toload.m_t = buf.ReadSingle();

		Debug.Log ("Chargement toload   " + toload.m_h + "  " + toload.m_l + "  " + toload.m_i + "  " + toload.m_p + "  " + toload.m_s + "  "+ toload.m_t + "  ");

		//chargement données origine (gyro)
		m_originalState = new Hlips();
		
		m_originalState.m_h = buf.ReadSingle();
		m_originalState.m_l = buf.ReadSingle();
		m_originalState.m_i = buf.ReadSingle();
		m_originalState.m_p = buf.ReadSingle();
		m_originalState.m_s = buf.ReadSingle();
		m_originalState.m_t = buf.ReadSingle();
		
		Debug.Log ("OriginalState   " + m_originalState.m_h + "  " + m_originalState.m_l + "  " + m_originalState.m_i + "  " + m_originalState.m_p + "  " + m_originalState.m_s + "  "+ m_originalState.m_t + "  ");

		m_cameraData.Clear();
		if(completeLoad)
		{
			m_cameraData.Add(toload);//données sauvegardées
			GameObject.Find("camPivot").GetComponent<SceneControl>().loadFromModel();
		}
		else
		{
			m_cameraData.Add(m_originalState);//données sauvegardées
			GameObject.Find("camPivot").GetComponent<SceneControl>().loadFromModel();
//			/*m_cameraData.*/setFromOriginal();//données originales(gyroscope si photo prise de l'ipad)
		}
		
		
		
//		//ce serait mieux de faire un "fireSceneModelChange" a la fin du load mais bon ...
//		//ptet plus tard
		
//		//light
//		LightData.Clear();
		Ahibp liteToLoad = new Ahibp(0,0,0,0,0,128.0f,128.0f,128.0f);
		liteToLoad.m_a = buf.ReadSingle();
		liteToLoad.m_h = buf.ReadSingle();
		liteToLoad.m_i = buf.ReadSingle();
		liteToLoad.m_b = buf.ReadSingle();
		liteToLoad.m_p = buf.ReadSingle();	
		
		SetNightMode(false);
		if(!LibraryLoader.numVersionInferieur(version,"1.2.1"))
		{
			liteToLoad.m_img_r = buf.ReadSingle();	
			liteToLoad.m_img_g = buf.ReadSingle();	
			liteToLoad.m_img_b = buf.ReadSingle();		
			
		}
		if(!LibraryLoader.numVersionInferieur(version,"1.2.2"))
		{
			_nightMode = buf.ReadBoolean();
		}
		
		m_lightData.Clear();
		m_lightData.Add(liteToLoad);
		GameObject.Find("LightPivot").GetComponent<LightConfiguration>().LoadFromModel();
//		LightData.Add(buf.ReadSingle()); //FLOAT
//		LightData.Add(buf.ReadSingle()); //FLOAT
//		LightData.Add(buf.ReadSingle()); //FLOAT
//		LightData.Add(buf.ReadSingle()); //FLOAT
//		LightData.Add(buf.ReadSingle()); //FLOAT
//		
//		GameObject.Find("LightPivot").GetComponent<LightConfiguration>().loadFromModel(LightData);
//		
		//Reflex
		m_reflex.m_id = buf.ReadInt32();
		m_reflex.m_rate = buf.ReadSingle();
		Debug.Log("LOADED ID" + m_reflex.m_id + " taux "+ m_reflex.m_rate);
//		GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setReflexionLimits(m_reflex.m_id);
		GameObject.Find("LightPivot").GetComponent<LightConfiguration>().loadReflex(m_reflex.m_rate,m_reflex.m_id);
		
		
		//background		
		float scaleY = buf.ReadSingle();
		
		isFromPhototheque = buf.ReadBoolean();
		
		
		Vector3 tmp = GameObject.Find("backgroundImage").transform.localScale;
		tmp.y = scaleY;
		GameObject.Find("backgroundImage").transform.localScale = tmp;

		
		if(isFromPhototheque)
		{
			backgroundPath = buf.ReadString();
			background = GameObject.Find("MainScene").GetComponent<GUIStart>().getPicFromPhototheque(int.Parse(backgroundPath));
			
			GameObject.Find("MainScene").GetComponent<Montage>().reset();
			
			GameObject.Find("MainScene").GetComponent<GUIMenuMain>().setStarter(false);
			GameObject.Find("MainScene").GetComponent<GUIMenuMain>().SetHideAll(true);
		}
		else
		{
			int len = buf.ReadInt32();
			int w = buf.ReadInt32();
			int h = buf.ReadInt32();
			byte[] bytes = buf.ReadBytes(len);

			background = new Texture2D(w,h,TextureFormat.RGB24,false);
            background.LoadImage(bytes);
			
			GameObject.Find("MainScene").GetComponent<Montage>().reset();
			
			GameObject.Find("MainScene").GetComponent<GUIMenuMain> ().setStarter (false);
			GameObject.Find("MainScene").GetComponent<GUIMenuMain>().SetHideAll(true);
//			GameObject.Find("MainScene").GetComponent<Montage>().loadBgFromOutside(backgroundPath);
		}


       // -- Mode2D --
        if(!LibraryLoader.numVersionInferieur(version,"1.3.2"))
        {
            bool m2d_empty = buf.ReadBoolean();
            if(!m2d_empty)
            {
                Rect org = new Rect();
                org.x = buf.ReadSingle();
                org.y = buf.ReadSingle();
                org.width = buf.ReadSingle();
                org.height = buf.ReadSingle();
    
                Rect saved = new Rect();
                saved.x = buf.ReadSingle();
                saved.y = buf.ReadSingle();
                saved.width = buf.ReadSingle();
                saved.height = buf.ReadSingle();
    
                Vector2 old = new Vector2();
                old.x = buf.ReadSingle();
                old.y = buf.ReadSingle();
    
                float   zoomO    = buf.ReadSingle();
                Vector2 translaO = new Vector2(buf.ReadSingle(), buf.ReadSingle());
                float   rotaO    = buf.ReadSingle();
                float[] lineData = null;
                bool    oldSave    = false;

                if(LibraryLoader.numVersionInferieur(version,"1.6.4"))
                {
                    oldSave = true; // Tag the save as old so the Mode2D knows it must not load everything
                    buf.ReadSingle(); // Old "ZoomImg" variable
                }
                else
                {
					//DEBUG_SAVE
					int countLigneData=(int)buf.ReadSingle();
                    lineData = new float[countLigneData+1];
				 	lineData[0] = countLigneData;
                    for(int i=1; i<lineData.Length; i++)    // New lineData
                        lineData[i] = buf.ReadSingle();
					
				}

                bool image = buf.ReadBoolean();
                if(image)
                {
                    int len = buf.ReadInt32();
                    int w = buf.ReadInt32();
                    int h = buf.ReadInt32();
                    byte[] bytes = buf.ReadBytes(len);

                    Texture2D img = new Texture2D(w,h);
                    img.LoadImage(bytes);

                    m_mode2D.Set(org, saved, old, zoomO, translaO, rotaO, lineData, img, oldSave);
                }
                else
                    m_mode2D.Set(org, saved, old, zoomO, translaO, rotaO, lineData, null, oldSave);
            }
        }

	} // load()

	#endregion
	
	//AUX. FCN.
	public void printList(ArrayList list)
	{
		foreach(object o in list)
			Debug.Log(o);
	}
	
} // class SceneModel
