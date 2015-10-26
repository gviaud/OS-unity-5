using UnityEngine;
using System.Collections;
using Pointcube.Global;

public class RotatingGUITexture : MonoBehaviour
{
    // -- Variables pour la rotation --
    private float           mRotatingAngle;

    public  bool            mIsActive;
    public  Texture         mRotatingTex;
    private Rect            mRotatingTexRect;
    private float           mRotatingSpeed;

    // static
    private static readonly string DEBUGTAG     = "RotatingGUITexture : ";
    private static readonly bool   DEBUG        = false;

	//-----------------------------------------------------
	void Start()
    {
	    mRotatingAngle = 0f;
//        mIsActive = true;
        mRotatingSpeed = 2f;
        mRotatingTexRect = new Rect(100f, 100f, 100f, 100f);

        if(PC.DEBUG && DEBUG) Debug.Log(DEBUGTAG+"Instanciation");

        if(mRotatingTex == null)
        {
            if(PC.DEBUG && DEBUG) Debug.LogError(DEBUGTAG+" No texture assigned. Deactivating script.");
            this.enabled = false;
        }
	}
	
	//-----------------------------------------------------
	void OnGUI()
    {
	    if(mIsActive && mRotatingTex != null)
            DrawRotatingTex();
	} // OnGUI()

    //-----------------------------------------------------
    private void DrawRotatingTex()
    {
        //rotating loading
        Matrix4x4 bkup = GUI.matrix;
        GUIUtility.RotateAroundPivot(mRotatingAngle, mRotatingTexRect.center);

        GUI.DrawTexture(mRotatingTexRect, mRotatingTex);

        GUI.matrix = bkup;
    } // DrawRotatingTex

    //-----------------------------------------------------
    public void SetRect(Rect newRect)
    {
        mRotatingTexRect = newRect;
    }

    //-----------------------------------------------------
    public void SetActive(bool newState)
    {
        mIsActive = newState;
    }

    //-----------------------------------------------------
    public bool IsActive()
    {
        return mIsActive;
    }

    //-----------------------------------------------------
    void FixedUpdate()
    {
        if(mIsActive)
        {
            mRotatingAngle += mRotatingSpeed;
            if(mRotatingAngle >= 360)
                mRotatingAngle -= 360;
        }

    } // FixedUpdate()

} // class StartAnimBg
