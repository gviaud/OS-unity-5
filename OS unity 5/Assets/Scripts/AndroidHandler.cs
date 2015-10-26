using UnityEngine;
using System.Collections;

#if UNITY_ANDROID
using CodX.Android;
#endif

public class AndroidHandler : MonoBehaviour
{
#if UNITY_ANDROID
    public void TakePictureCallback(string result)
    {
        Debug.Log("TakePictureCallBack result = "+result);
        switch (result)
        {
            case "missing_app":
                AndroidCommons.Toast("No candidate to handle this operation was found.");
                break;
            case "unknown_format":
                AndroidCommons.Toast("Operation canceled.");
                break;
            default:
                StartCoroutine(((PluginPhotoManagerGUI)GameObject.Find("MainScene").GetComponent("PluginPhotoManagerGUI")).TakePhotoSuccess(result));
                AndroidCommons.Toast("Picture available @ " + result);
                break;
        }
    }
#endif
}

