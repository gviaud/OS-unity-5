using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Runtime.InteropServices;
/*
 * 
 * 		TO DO
 * 
 * 		Eliminer ce qui ne sert plus
 * 
 * /*/
 
 

public class DllImport : MonoBehaviour {


	[DllImport ("UnityArLibCustom")]
	public static extern bool initStream ();

	[DllImport ("UnityArLibCustom")]
	public static extern int nrOfDevices ();
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool getDeviceInfo (int deviceInfo, StringBuilder s);

	[DllImport ("UnityArLibCustom")]
	public static extern int nrOfSupportedModes ();
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool getModeInfo (int modeInfo, StringBuilder s);
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool connectToDevice (int deviceNr, [In, Out] int[] resolution);
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool setMode (int modeNr, [In, Out] int[] resolution);
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool grabImage (int camImageType);

	[DllImport ("UnityArLibCustom")]
	public static extern bool startVideoStream(int camImageType);
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool startVideoStreamScreenshot(int camImageType);
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool stopVideoStream();
	
	[DllImport ("UnityArLibCustom")]
	public static extern void getPixelImage (int x, int y, [In, Out] float[] color);
	
	[DllImport ("UnityArLibCustom")]
	public static extern void getPixelFrame (int x, int y, [In, Out] float[] color);
	
	[DllImport ("UnityArLibCustom")]
	public static extern void releaseStream ();

	[DllImport ("UnityArLibCustom")]
	public static extern int nrOfDetectedMarker ();
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool getDetectedTranslationRotation([In, Out] double[] translation, [In, Out] double[] rotation, int markerNr);

	[DllImport ("UnityArLibCustom")]
	public static extern int nrOfLoadedMarker ();
	
	 [DllImport ("UnityArLibCustom")] 
	 public static extern bool loadImage(string file, [In, Out] int[] dim);
	///////////////
//	 [DllImport ("UnityArLibCustom")] 
//	 public static extern bool sendRawValue(string val, int position );
	
	 [DllImport ("UnityArLibCustom")] 
	 public static extern bool sendImageDataParameters(int bytesPerPixels, [In, Out] int[] dim);
	//////////////
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool processImage();
	
    [DllImport ("UnityArLibCustom")]
	public static extern bool isStreamActive();
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool isRawDataAvailable();
	
	[DllImport ("UnityArLibCustom")]
	public static extern int getChannels();
	
	[DllImport ("UnityArLibCustom")]
	public static extern void releaseImage();

	[DllImport ("UnityArLibCustom")]
	public static extern int getNbchannels();
	
	[DllImport ("UnityArLibCustom")]
	public static extern bool initARforImage();
	
	[DllImport ("UnityArLibCustom")]
	public static extern double getConfidenceValue(int marker_num);
	
	[DllImport ("UnityArLibCustom")]
	public static extern void setConfidenceValue(double cfValue);
	
	[DllImport ("UnityArLibCustom")]
	public static extern int getAreaSize(int nr);
	
	[DllImport ("UnityArLibCustom")]
	public static extern int getID(int nr);
	
	
	[DllImport ("UnityArLibCustom")]
	public static extern int getDir(int nr);
	
	[DllImport ("UnityArLibCustom")]
	public static extern int getCenterX(int nr);


	[DllImport ("UnityArLibCustom")]
	public static extern int getCenterY(int nr);
	
	
	[DllImport ("UnityArLibCustom")]
	internal static extern void sendComponent(float component, int position);
	
	[DllImport ("UnityArLibCustom")]
	public static extern byte get(int position);
		
	[DllImport ("UnityArLibCustom")]
	public static extern int getSize();
	

	[DllImport ("UnityArLibCustom")]
	public static extern void sendBuffertoDLL(byte[] data, int widthHD, int heightHD, int BufferSize);
	//-------------------------------------------------------------------------

	
}
