using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using System.Threading;

public class UsefulFunctions
{
	//-----------------------------------------------------
    // Retourne les tous enfants d'un noeuds
	public static Transform[] getAllChild(GameObject root)
	{

		Transform[] filters = root.GetComponentsInChildren<Transform> ();

		return filters;
	}
	
#region func_slide
    //-----------------------------------------------------
    // TODO voir le slideDetect de DT pour android

//    private static bool     s_lastSens         = true;
//    private static float    s_sldDtct          = 0;
//    private static float    s_sldDtctThreshold = 20;
//
//    public static float SlideDetect(bool exponential, float cap)
//    {
//        if(UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer) //Ipad
//        {
//            if(Input.touchCount >0)
//            {
//                Touch t = Input.touches[0];
//                if(t.phase == TouchPhase.Moved)
//                {
//                    if(s_sldDtct < s_sldDtctThreshold)
//                    {
//                        s_sldDtct += t.deltaPosition.magnitude;
//                        return 0;
//                    }
//                    else
//                    {                        
//                        // Pas de distingo horizontal-vertical
//                        Vector2 trace = t.deltaPosition;
//                        
//                        if(trace.x*trace.x > trace.y*trace.y)
//                            s_lastSens = trace.x>0;
//                        else if(trace.x*trace.x < trace.y*trace.y)
//                            s_lastSens = trace.y>0;
//                        
//                        float touch = (s_lastSens? 1 : -1)*t.deltaPosition.magnitude;
//                        
//                        if(exponential)
//                            touch = (touch<0? -1 : 1)*touch*touch;
//                        
//                        if(touch > cap)       touch = cap;
//                        else if(touch < -cap) touch = -cap;
//                        
//                        return touch;
//                    }
//                }
//                else if(t.phase == TouchPhase.Ended) return 0;
//                else return 0;
//            }
//            else 
//            {
//                s_sldDtct = 0;
//                return 0;
//            }
//        }
//        else //PC et autres
//        {
//            float val = Input.GetAxis("Mouse ScrollWheel");
//            if(exponential)
//                val=val*5;
//            return 200*val;    
//        }
//    } // slideDetect
//

#endregion	
#region func_mouse
    //-----------------------------------------------------
    // Retourne le déplacement du curseur souris en pixels
    // TODO variables membres
//    public static Vector2 SimpleMoveDetectMouse()
//    {
//        // -- Détection déplacement image souris --
//        if(Input.GetMouseButtonDown(0) && !CursorOnUI())
//        {
//            m_clickLastPos = Input.mousePosition;
//            m_leftClicking = true;
//            DisableHelp();
//        }
//
//        if(m_leftClicking)
//        {
//            if(!m_justResizedScr) // ignorer si changement de résolution (sinon ça translate
//            {                     //  quand on clique sur le bouton "plein écran" en éditeur)
//                plusX = Input.mousePosition.x - m_clickLastPos.x;
//                plusY = Input.mousePosition.y - m_clickLastPos.y;
//            }
//            else
//                m_justResizedScr = false;
//
//            m_clickLastPos = Input.mousePosition;
//
//            if(Input.GetMouseButtonUp(0))
//                m_leftClicking = false;
//        }
//    }

#endregion
#region func_touchPad
    // TODO à faire pour android

    //-----------------------------------------------------
    private static Vector2 m_mdTouchLastPos = Vector2.zero;
    public static Vector2 MoveDetectTouch()
    {
        Vector2 result = Vector2.zero;
        if(Input.touchCount == 1)
        {
            Touch t0 = Input.touches[0];
            if(t0.phase == TouchPhase.Began)
            {
                m_mdTouchLastPos = Input.mousePosition;
            }
            else if(t0.phase == TouchPhase.Moved)
            {
                result.x = t0.position.x - m_mdTouchLastPos.x;
                result.y = t0.position.y - m_mdTouchLastPos.y;

                m_mdTouchLastPos = t0.position;
            }
        }
        return result;
    } // MoveDetect

    //-----------------------------------------------------
    private static float m_lastZoomDist = 0f;
    public static float ZoomDetectTouch()
    {
        float zoom = 0f;
        if(Input.touchCount == 2)
        {
            Touch t0 = Input.touches[0];
            Touch t1 = Input.touches[1];

            if(t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                m_lastZoomDist = Vector2.Distance(t0.position, t1.position);
            if(t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
            {
                zoom = (Vector2.Distance(t0.position, t1.position) - m_lastZoomDist);
                m_lastZoomDist = Vector2.Distance(t0.position, t1.position);
            }
        } // 2 touch

        return zoom;
    } // ZoomDetectTouch()

    //-----------------------------------------------------
    private static Vector2 s_zmtLastT0pos = Vector2.zero;
    private static Vector2 s_zmtLastT1pos = Vector2.zero;
    public static Vector3 ZoomAndMoveTouch()
    {
        Vector3 result = Vector3.zero;
        if(Input.touchCount == 1)       // Move only
        {
            Touch t0 = Input.touches[0];
            if(t0.phase == TouchPhase.Moved)
            {
                result.x = t0.deltaPosition.x;
                result.y = t0.deltaPosition.y;
            }
        }
        else if(Input.touchCount == 2)  // Move & zoom
        {
            Touch t0 = Input.touches[0];
            Touch t1 = Input.touches[1];

            if(t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                s_zmtLastT0pos = t0.position;
                s_zmtLastT1pos = t1.position;
            }
            if(t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
            {
                float lastDist = Vector2.Distance(s_zmtLastT0pos, s_zmtLastT1pos);

                // -- Move --
                result.x = (t0.deltaPosition.x + t1.deltaPosition.x)/2f;
                result.y = (t0.deltaPosition.y + t1.deltaPosition.y)/2f;

                // -- Zoom --
                result.z = (Vector2.Distance(t0.position, t1.position) - lastDist);

                s_zmtLastT0pos = t0.position;
                s_zmtLastT1pos = t1.position;
            }
        } // 2 touch

        return result;
    } // ZoomAndMoveTouch()

    //-----------------------------------------------------
    private static Vector2  s_zrtLastT0Pos      = new Vector2();
    private static Vector2  s_zrtLastT1Pos      = new Vector2();
    // Retourne (angle en degrés, zoom en pixels)
    public static Vector2 ZoomAndRotatTouch()
    {
        Vector2 result  = new Vector2(0f, 0f);
        if(Input.touchCount == 2)
        {
            Touch t0 = Input.touches[0];
            Touch t1 = Input.touches[1];

            if(t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                s_zrtLastT0Pos = t0.position;
                s_zrtLastT1Pos = t1.position;
            }
            else if(t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
            {
                float curDist  = Vector2.Distance(t0.position, t1.position);
//                float lastDist = Vector2.Distance(s_zrtLastT0Pos, s_zrtLastT1Pos);

                // -- Rotation --
                Vector2 oldDir = s_zrtLastT1Pos - s_zrtLastT0Pos;
                float angle = Vector2.Angle(oldDir,(t1.position - t0.position));
                Vector3 cross = Vector3.Cross(oldDir,(t1.position - t0.position));
                if(cross.z < 0) angle = 360 - angle;

                // -- Zoom --
                float startDist = Vector2.Distance(s_zrtLastT0Pos, s_zrtLastT1Pos);

                result.Set (angle, curDist - startDist);
                s_zrtLastT0Pos = t0.position;
                s_zrtLastT1Pos = t1.position;
            }
        } // 2 touch

        return result;
    } // ZoomAndRotatTouch()

    //-----------------------------------------------------
    private static Vector2 s_zmrtLastT0pos = Vector2.zero;
    private static Vector2 s_zmrtLastT1pos = Vector2.zero;
    private static readonly float c_rotatThres = 0.67f; // en degrés
    public static Vector4 ZoomAndMoveAndRotateTouch() // x,y = Move ; z = Zoom ; w = Rotate
    {
        Vector4 result = Vector4.zero;
        if(Input.touchCount == 1)       // Move only
        {
            Touch t0 = Input.touches[0];
            if(t0.phase == TouchPhase.Moved)
            {
                result.x = t0.deltaPosition.x;
                result.y = t0.deltaPosition.y;
            }
        }
        else if(Input.touchCount == 2)  // Move & zoom
        {
            Touch t0 = Input.touches[0];
            Touch t1 = Input.touches[1];

            if(t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                s_zmrtLastT0pos = t0.position;
                s_zmrtLastT1pos = t1.position;
            }
            if(t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
            {
                float lastDist = Vector2.Distance(s_zmrtLastT0pos, s_zmrtLastT1pos);

                // -- Rotation --
                Vector2 oldDir = s_zmrtLastT1pos - s_zmrtLastT0pos;
                float angle = Vector2.Angle(oldDir,(t1.position - t0.position));
                Vector3 cross = Vector3.Cross(oldDir,(t1.position - t0.position));

                float reduc = (angle > c_rotatThres ? 0.2f : 1f);

                if(cross.z < 0) angle = 360 - angle;
                result.w = angle;

                // -- Move --
                result.x = (t0.deltaPosition.x + t1.deltaPosition.x)/2f * reduc;
                result.y = (t0.deltaPosition.y + t1.deltaPosition.y)/2f * reduc;

                // -- Zoom --
                result.z = (Vector2.Distance(t0.position, t1.position) - lastDist) * reduc;

                s_zmrtLastT0pos = t0.position;
                s_zmrtLastT1pos = t1.position;
            }
        } // 2 touch

        return result;
    } // ZoomAndMoveAndRotateTouch()

    //-----------------------------------------------------
    public static float rescaleFloat(float input, float curMin, float curMax,
                                                                   float wantedMin, float wantedMax)
    {
        return (input-curMin)*((wantedMax-wantedMin)/curMax)+wantedMin;
    }
#endregion

#region montage
    //-----------------------------------------------------
    private static Bounds s_montageBounds;
    public static Bounds GetMontageBounds(GameObject mainNode)
    {
        if(s_montageBounds == null)
            s_montageBounds = new Bounds();

        Transform child;
        for(int i=0; i<mainNode.transform.GetChildCount(); i++)
        {
            child = mainNode.transform.GetChild(i);
            if(child.GetComponent<Collider>() != null)
                s_montageBounds.Encapsulate(child.GetComponent<Collider>().bounds);
            else
                Debug.LogError("UsefulFunctions.GetMontageObjBounds : could not find object collider (\""+child.name+"\")");
        }

        return s_montageBounds;
    }

    //-----------------------------------------------------
    private static Vector2 s_screenCenter;
    public static Vector2 ScrCenter()
    {
        if(s_screenCenter == null)
            s_screenCenter = new Vector2(Screen.width/2f, Screen.height/2f);
        else
            s_screenCenter.Set(Screen.width/2f, Screen.height/2f);

        return s_screenCenter;
    }

    //-----------------------------------------------------
	public static Montage GetMontage()
	{
		return GameObject.Find("MainScene").GetComponent<Montage>();
	}
#endregion
	
#region Parametre
	public static void ChangeLanguage(int langueId)
	{
		switch(langueId)
		{
		case 0:
			ConfigurationManager.SetText("language", "fr");
			PlayerPrefs.SetString("language", "fr");
			break;
		case 1: 
			ConfigurationManager.SetText("language", "en");
			PlayerPrefs.SetString("language", "en");
			break;
		case 2: 
			ConfigurationManager.SetText("language", "de");
			PlayerPrefs.SetString("language", "de");
			break;
		}		
		// This will reset to default POT Language
		TextManager.LoadLanguage(null);		 
		// This will load filename-en.po
		TextManager.LoadLanguage("strings_"+ConfigurationManager.GetText("language"));
	}
	
	public static IEnumerator Deactivate(GUIDialogBox cb, bool saveBefore)
	{
		if(Application.internetReachability != NetworkReachability.NotReachable)
		{
			EncodeToolSet encoder = new EncodeToolSet();
			string login = PlayerPrefs.GetString(usefullData.k_logIn);
			string mdp = PlayerPrefs.GetString(usefullData.k_password);
			string code = encoder.createSendCode(false,false,int.Parse(login),mdp);

			WWWForm deactForm = new WWWForm();
			deactForm.AddField("ac", code);
			
			WWW sender = new WWW(usefullData.DeactivationUrl,deactForm);
			
			yield return sender;
			
			if(sender.error != null)
			{
				Debug.Log("ERROR : "+sender.error);
			}
			else
			{
				Debug.Log("Deactivation result "+sender.text);
				usefullData.DeleteLogo();
				PlayerPrefs.DeleteAll();
				//SetAllowQuit(true);
                InterProcess.Stop();
				#if UNITY_STANDALONE_WIN 
                System.Diagnostics.Process.GetCurrentProcess().Kill(); 
#else 
                UnityEngine.Application.Quit(); 
#endif
			}
		}
		else if(saveBefore)
		{
			cb.showMe(true,GUI.depth);
				cb.setBtns(TextManager.GetText("GUIMenuRight.Yes"),
					TextManager.GetText("GUIMenuRight.No"),
					TextManager.GetText("GUIMenuRight.Save"));
			
			cb.setText(TextManager.GetText("GUIMenuRight.ConnectionFailed"));
		}
	}
#endregion
	
	#region GUI
	
	public static void GUIOutlineLabel(Rect r,string txt,string outStyle,string inStyle)
	{
		Rect txtOutR = r;
		
		txtOutR.x = r.x -1f;
        GUI.Label(txtOutR, txt, outStyle);   // Draw 5 times the text to create a 1px outline
		
		txtOutR.x = r.x +1f;
        GUI.Label(txtOutR, txt, outStyle);
		
		txtOutR.x = r.x;
		txtOutR.y = r.y -1f;
        GUI.Label(txtOutR, txt, outStyle);
		
		txtOutR.y = r.y +1f;
        GUI.Label(txtOutR, txt, outStyle);
        
		GUI.Label(r,       txt, inStyle);
	}	
	
	public static bool GUIOutlineButton(Rect r,string txt,string outStyle,string inStyle)
	{
		Rect txtOutR = r;
		
		txtOutR.x = r.x -1f;
        GUI.Label(txtOutR, txt, outStyle);   // Draw 5 times the text to create a 1px outline
		
		txtOutR.x = r.x +1f;
        GUI.Label(txtOutR, txt, outStyle);
		
		txtOutR.x = r.x;
		txtOutR.y = r.y -1f;
        GUI.Label(txtOutR, txt, outStyle);
		
		txtOutR.y = r.y +1f;
        GUI.Label(txtOutR, txt, outStyle);
        
		return GUI.Button(r,txt,inStyle);
	}	
	
	public static string GUIOutlineTextField(Rect r,string txt,string outStyle,string inStyle)
	{
		Rect txtOutR = r;
		
		txtOutR.x = r.x -1f;
        GUI.Label(txtOutR, txt, outStyle);   // Draw 5 times the text to create a 1px outline
		
		txtOutR.x = r.x +1f;
        GUI.Label(txtOutR, txt, outStyle);
		
		txtOutR.x = r.x;
		txtOutR.y = r.y -1f;
        GUI.Label(txtOutR, txt, outStyle);
		
		txtOutR.y = r.y +1f;
        GUI.Label(txtOutR, txt, outStyle);
        
		return GUI.TextField(r,txt,inStyle);
	}
	
	#endregion
}


