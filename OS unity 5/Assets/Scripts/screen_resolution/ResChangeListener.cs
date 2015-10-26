using UnityEngine;
using System.Collections;

public class ResChangeListener : MonoBehaviour
{
	private     int  mResizingW;
	private		int  mResizingH;

    private     int  mResizeEndW;
    private     int  mResizeEndH;

    private     int  mResizingCount;
    private     bool mSkipNextResizing;

	//-----------------------------------------------------
	void Start()
	{
		mResizingW  = Screen.width;
		mResizingH  = Screen.height;
        mResizeEndW = Screen.width;
        mResizeEndH = Screen.height;

        mResizingCount = 0;
        mSkipNextResizing = false;
	}
	
	//-----------------------------------------------------
    // Appelé aussi pendant les resize de fenêtre (même si
    // on n'a pas relâché le clic)
	void OnGUI()
	{
		if(mResizingW != Screen.width || mResizingH != Screen.height)
		{
            if(!mSkipNextResizing)
            {
//                Debug.Log ("========================== RESIZING, Count = "+mResizingCount);
    		    UsefullEvents.FireResizingWindow(/*Screen.width - mResizingW, Screen.height - mResizingH*/);
                mResizingCount++;
            }
            else
            {
                mSkipNextResizing = false;
//                Debug.Log ("============================= SKIPPING RESIZING");
            }

            mResizingW = Screen.width;
            mResizingH = Screen.height;
		}
	}

    //-----------------------------------------------------
    // Appelé seulement lorsque le clic est relâché après
    // un redimensionnement
    void Update()
    {		
        if(mResizeEndW != Screen.width || mResizeEndH != Screen.height)
        {
            if(mResizingCount == 0)
                mSkipNextResizing = true;
            else
                mResizingCount = 0;

//            Debug.Log ("========================== RESIZED, Count = "+mResizingCount);
            UsefullEvents.FireResizeWindowEnd(/*Screen.width - mResizeEndW, Screen.height - mResizeEndH*/);
            mResizeEndW = Screen.width;
            mResizeEndH = Screen.height;
        }
    }
	
} // ResChangeListener
