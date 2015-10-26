using UnityEngine;
using System.Collections;
using Pointcube.Utils;


public class PluginPhotoManagerGUI : MonoBehaviour
{
	public GameObject testPlane;
	private string imagePath;
	
	private bool _isImport = false;
	
	//Dev test
	Texture2D devTestTexture;
	
	void Start()
	{

		// Listen to image picker event so we can load the image into a texture later
#if UNITY_IOS
		EtceteraManager.imagePickerChoseImageEvent += imagePickerChoseImage;
		EtceteraManager.imagePickerCancelledEvent += imagePickerCanceled;
#endif
		
#if UNITY_ANDROID
		EtceteraAndroidManager.photoChooserSucceededEvent += TakePhotoSuccess;
		EtceteraAndroidManager.albumChooserSucceededEvent += AlbumChoiceSuccess;
		
		EtceteraAndroidManager.photoChooserCancelledEvent += ImageChoiceCancel;
		EtceteraAndroidManager.albumChooserCancelledEvent += ImageChoiceCancel;
#endif
	
	}
	
	
	void OnEnable()
	{
				// Listen to image picker event so we can load the image into a texture later
#if UNITY_IOS
		EtceteraManager.imagePickerChoseImageEvent += imagePickerChoseImage;
		EtceteraManager.imagePickerCancelledEvent += imagePickerCanceled;
#endif
		
#if UNITY_ANDROID
		//EtceteraAndroidManager.photoChooserSucceededEvent += TakePhotoSuccess;
		//EtceteraAndroidManager.albumChooserSucceededEvent += AlbumChoiceSuccess;
		
		EtceteraAndroidManager.photoChooserCancelledEvent += ImageChoiceCancel;
		EtceteraAndroidManager.albumChooserCancelledEvent += ImageChoiceCancel;
#endif
	}
	void OnDisable()
	{
		// Stop listening to the image picker event
#if UNITY_IOS
		EtceteraManager.imagePickerChoseImageEvent -= imagePickerChoseImage;
		EtceteraManager.imagePickerCancelledEvent -= imagePickerCanceled;
#endif
		
#if UNITY_ANDROID
		//EtceteraAndroidManager.photoChooserSucceededEvent -= TakePhotoSuccess;
		//EtceteraAndroidManager.albumChooserSucceededEvent -= AlbumChoiceSuccess;
		
		EtceteraAndroidManager.photoChooserCancelledEvent -= ImageChoiceCancel;
		EtceteraAndroidManager.albumChooserCancelledEvent -= ImageChoiceCancel;
#endif
	}
	
	public void SetMode(bool isImport)
	{
		Debug.Log("MODE SET TO IMPORT OR NOT");
		_isImport = isImport;	
	}
	
	
void OnGUI()
	{
		if(devTestTexture != null)
		{
			GUI.DrawTexture(new Rect(10,10,256,256),devTestTexture);	
		}
	}
	
#if UNITY_IOS
	void imagePickerChoseImage( string imagePath )
	{
		if(GameObject.Find("MainScene").GetComponent<GUIStart>().enabled == false)
			return;
		GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = false;
		GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(false);
		this.imagePath = imagePath;
		if( imagePath == null )
		{
			EtceteraBinding.showAlertWithTitleMessageAndButton( "Load Photo Texture Error", "You have to choose a photo before loading", "OK" );
			return;
		}
		Debug.Log( "UNITY imagePickerChoseImage: " + imagePath );

		//BC
		//EtceteraBinding.etceteraSetMaxSizeKeepingRatioImageAtPath(imagePath, 1920, 1920); // 3508 -> A4 en 300dpi
		//EtceteraBinding.resizeImageAtPath(imagePath, 1920, 1920);
		
		StartCoroutine( EtceteraManager.textureFromFileAtPath( "file://" + imagePath, textureLoaded, textureLoadFailed ) );
	}
	
	void imagePickerCanceled()
	{
		if(GameObject.Find("MainScene").GetComponent<GUIStart>().enabled == false)
			return;
		GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = false;
		GameObject.Find("camPivot").GetComponent<SceneControl>().init ();
	//	GameObject.Find ("MainScene").GetComponent<GUIStart>().backToClients();
	}
	
	// Texture loading delegates
	public void textureLoaded( Texture2D texture )
	{
		Debug.Log( "UNITY textureLoaded "+texture+", "+texture.width+", "+texture.height);
		//testPlane.renderer.material.mainTexture = texture;
//		GetComponent<GUIMenu>().setStarter(texture);
//		Vector3 gyroData = Input.gyro.rotationRate;
		//GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = false;
		/*Montage.sm.updateFond(texture, false,"");
		GetComponent<GUIMenuMain>().setStarter(true, true);
		//GetComponent<GUIImgSelector>().showMe();
		*/
		Montage.sm.updateFond(texture, false,"");
		StartCoroutine (loadImg());
		if(System.IO.File.Exists(imagePath))
		{
			Debug.Log("IMG STILL EXISTS: Deleting ...");
			System.IO.File.Delete(imagePath);
		}
	}

	public void textureLoadFailed( string error )
	{
		GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = false;
		GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(false);
//		EtceteraBinding.showAlertWithTitleMessageAndButton( "Error Loading Texture.  Did you choose a photo first?", error, "OK" );
		Debug.Log( "textureLoadFailed: " + error );
		EtceteraManager.setPrompt(false);
	}
#endif

#if UNITY_ANDROID
private void ResizeTexAtPath(string imagePath)
{
    AndroidJavaObject options = new AndroidJavaObject("android/graphics/BitmapFactory$Options");
    // Debug.Log("options set injustdecodebounds");
    options.Set<bool>("inJustDecodeBounds", true);
    //  Debug.Log("factory class");
    AndroidJavaClass factory = new AndroidJavaClass("android.graphics.BitmapFactory");
    //Debug.Log("factorydecodeFile");
    factory.CallStatic<AndroidJavaObject>("decodeFile", imagePath, options);
    int imgWidth = options.Get<int>("outWidth");
    int imgHeight = options.Get<int>("outHeight");
    Debug.Log("imgWidth=" + imgWidth);
    Debug.Log("imgHeight=" + imgHeight);
    float scaleMax = 1;
    //Debug.Log("javaobjects end");
    if (imgWidth > Screen.width)
    {

        scaleMax = ((float)Screen.width) / imgWidth;
    }
    if (imgHeight > Screen.height)
    {
        scaleMax = Mathf.Min(scaleMax, ((float)Screen.height) / imgHeight);
    }
    //Redimensione la photo si elle est plus grande que l'écran
    if (scaleMax < 1)
    {
        Debug.Log("Photo too big, scaling the photo taken : ratio=" + scaleMax);
#if UNITY_ANDROID
        EtceteraAndroid.scaleImageAtPath(imagePath, scaleMax);
#endif
    }
}
#endif

#if UNITY_ANDROID
public IEnumerator TakePhotoSuccess(string imagePath)
	{
		Debug.Log("### STRING RETURNED >>"+imagePath+"<< ###");
		if(GameObject.Find("MainScene").GetComponent<GUIStart>().enabled == false)
            yield return false;
		GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = false;
		GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(false);


     Debug.Log("Calling native android functions to get the image size");
     AndroidJavaObject options = new AndroidJavaObject("android/graphics/BitmapFactory$Options");
    // Debug.Log("options set injustdecodebounds");
    options.Set<bool>("inJustDecodeBounds", true);
  //  Debug.Log("factory class");
    AndroidJavaClass factory = new AndroidJavaClass("android.graphics.BitmapFactory");
    //Debug.Log("factorydecodeFile");
    factory.CallStatic<AndroidJavaObject>("decodeFile",imagePath, options);
    int imgWidth = options.Get<int>("outWidth");
    int imgHeight = options.Get<int>("outHeight");
    Debug.Log("imgWidth=" + imgWidth);
    Debug.Log("imgHeight=" + imgHeight);
    float scaleMax = 1;
    //Debug.Log("javaobjects end");
    if (imgWidth > Screen.width)
        {

            scaleMax = ((float)Screen.width) / imgWidth;
        }
    if (imgHeight > Screen.height)
    {
        scaleMax = Mathf.Min(scaleMax, ((float)Screen.height) / imgHeight);
    }
    //Redimensione la photo si elle est plus grande que l'écran
     if(scaleMax<1){
            Debug.Log("Photo too big, scaling the photo taken : ratio="+scaleMax);
            EtceteraAndroid.scaleImageAtPath(imagePath,scaleMax);     
     }
     WWW www = new WWW("file://" + imagePath);
     yield return www;
     Texture2D tex = www.texture;
        
      //  AndroidCommons.Toast("Texture loaded !!!!!");

        //EditorUtility.CompressTexture(tex, TextureFormat.RGBA32,TextureCompressionQuality.Fast);
        //tex.Compress(true);
        //ImgUtils.Bilinear(tex, tex.width / 2, tex.height / 2, true);

     
        /*
        int k = 0;
        while ((tex.width > 4096 || tex.height > 4096) && k < 2)
        {
            k++;
            Debug.Log("Texture of the photo ist too big !!!! tex.width= " + tex.width + " tex.height= " + tex.height);
            TextureScale.Bilinear(tex, tex.width / 2, tex.height / 2);
        }
        */
    
		//StartCoroutine(AndroidLoadPhoto(imagePath));
			
		Montage.sm.updateFond(tex, false,"");
		
	//	Montage.sm.updateFond(tex, false,"");
		//GetComponent<GUIMenuMain>().setStarter(true, true);
		
		StartCoroutine (loadImg());
		
//		if(System.IO.File.Exists(imagePath))
//		{
//			Debug.Log("IMG STILL EXISTS: Deleting ...");
//			System.IO.File.Delete(imagePath);
//		}

        Debug.Log("setting background tex");
        GameObject.Find("backgroundImage").GetComponent<BgImgManager>().SetBackgroundTexture(tex);
        GameObject.Find("mainCam").GetComponent<MainCamManager>().FitViewportToScreen();

	}
//	IEnumerator AndroidLoadPhoto(string path)
//	{
//		// vv TEST AVEC PATH SEULEMENT vv
//		Texture2D tex = new Texture2D(1,1);
//		WWW www = new WWW("file://" +path);
//		yield return www;
//		www.LoadImageIntoTexture(tex);
//		
//		Montage.sm.updateFond(tex, false,"");
//		
//		GetComponent<GUIMenuMain>().setStarter(true, true);
//	}
	
	 void AlbumChoiceSuccess(string imagePath, Texture2D tex)
	{
		Debug.Log("### STRING RETURNED >>"+imagePath+"<< ###");
		if(GameObject.Find("MainScene").GetComponent<GUIStart>().enabled == false)
			return;
		GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = false;
		GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(false);
		
		Montage.sm.updateFond(tex, false,"");
		//GetComponent<GUIMenuMain>().setStarter(true, true);
		
		StartCoroutine (loadImg());
		
	}

    public IEnumerator AlbumChoiceSuccessPathOnly(string imagePath)
    {
        Debug.Log("Album choice success ");
        ResizeTexAtPath(imagePath);
        Debug.Log("texture resized ");
        WWW www = new WWW("file://" + imagePath);
        yield return www;
        Texture2D tex = www.texture;
        Debug.Log("### STRING RETURNED >>" + imagePath + "<< ###");
        /*
        if (GameObject.Find("MainScene").GetComponent<GUIStart>().enabled == false)
            yield return false;
         */
   
        GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = false;
        GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(false);
        Debug.Log("gyro and compass disabled");

        Montage.sm.updateFond(tex, false, "");
        Debug.Log("fond updated");
        //GetComponent<GUIMenuMain>().setStarter(true, true);

        StartCoroutine(loadImg());
        Debug.Log("loadImg started");

        GameObject.Find("backgroundImage").GetComponent<BgImgManager>().SetBackgroundTexture(tex);
        GameObject.Find("mainCam").GetComponent<MainCamManager>().FitViewportToScreen();

    }
	
	

#endif
	
	public void ImageChoiceCancel()
	{
		GameObject.Find("camPivot").GetComponent<gyroControl_v2>().enabled = false;
		GameObject.Find("LightPivot").GetComponent<LightConfiguration>().setCompassActive(false);
		Debug.Log( "error" );
        if(GameObject.Find("MainScene").GetComponent<GUIStart>().enabled ){
		    GameObject.Find("camPivot").GetComponent<SceneControl>().init ();
        }
		//GameObject.Find ("MainScene").GetComponent<GUIStart>().backToClients();
	}
	
	IEnumerator loadImg ()
	{	
		GameObject guiStart = GameObject.Find ("MainScene");
		if(guiStart!=null)
		{		
			GUIStart start = guiStart.GetComponent<GUIStart>();
			if(start!=null)
			{
				while(!start.IsGUIReady())
				{
					yield return new WaitForEndOfFrame();	
				}
			}
		}
		if(_isImport)//photo importé
		{
			Debug.Log("IMPORT PHOTO");
			GetComponent<GUIMenuMain> ().setStarter (true, false, true);
		}
		else // photo de l'appareil photo
		{
			Debug.Log("TAKE PHOTO");
			GetComponent<GUIMenuMain> ().setStarter (false, true);
		}
		yield return true;
	}
	
}
