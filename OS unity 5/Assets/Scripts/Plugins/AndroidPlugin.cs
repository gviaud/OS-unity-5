using UnityEngine;
using System.Collections;
using System.Collections.Generic;




	public static class AndroidPlugin
	{
#if UNITY_ANDROID
        public static void browseGallery()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.Call("openGallery");
        }

        public static void takePicture()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.Call("takePhoto");
        }
#endif

	}

