using UnityEngine;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Pointcube.Utils;

public class EncodeToolSet{
	
	bool debugMode = false;
	
	public EncodeToolSet()
	{
	}
	
	// METHODES PERSOS
	
	// CREATE CODE -------------------------------------
	
#if UNITY_ANDROID && !UNITY_EDITOR
	  AndroidJavaObject mWiFiManager;
#endif
     
     private string ReturnMacAndroidAddress()
       {
          string macAddr = "";
#if UNITY_ANDROID && !UNITY_EDITOR
          if (mWiFiManager == null)
          {
             using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
             {
                mWiFiManager = activity.Call<AndroidJavaObject>("getSystemService","wifi");
             }
          }
          macAddr = mWiFiManager.Call<AndroidJavaObject>("getConnectionInfo").Call<string>("getMacAddress");
		Debug.Log("Mac Adress : "+ macAddr);
#endif
          return macAddr;
      }
	
	
		public static byte[] StringToByteArray(String hex)
		{
		  int NumberChars = hex.Length;
		  byte[] bytes = new byte[NumberChars / 2];
		  for (int i = 0; i < NumberChars; i += 2)
		    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
		  return bytes;
		}
	
	
	private string ReturnMacPlayerRefAddress()
	{
		DbgUtils.Log("MACOSXPB","ReturnMacPlayerRefAddress 1");
		string macAddr = "";
		//string macAddrRef = "";
		if( !PlayerPrefs.HasKey("nifRef") )
		{
			DbgUtils.Log("MACOSXPB","!nifRef 0");
			int nbNetwork=0; 
			try{
				foreach (NetworkInterface nif in NetworkInterface.GetAllNetworkInterfaces())
				//foreach (NetworkInterface nif in MacOsNetworkInterface.ImplGetAllNetworkInterfaces ())
				{
					DbgUtils.Log("MACOSXPB","0 nif adresse "+nbNetwork+" "+nif.GetPhysicalAddress().ToString());
					DbgUtils.Log("MACOSXPB","0 nif type "+nbNetwork+" "+nif.NetworkInterfaceType.ToString());
					if((nif.NetworkInterfaceType==NetworkInterfaceType.Tunnel)
					||(nif.NetworkInterfaceType==NetworkInterfaceType.Loopback)
					||(nif.NetworkInterfaceType==NetworkInterfaceType.Unknown))
						continue;
					
					DbgUtils.Log("MACOSXPB","nif ok"+nbNetwork+" "+nif.GetPhysicalAddress().ToString());
					PlayerPrefs.SetString("nif"+nbNetwork,nif.GetPhysicalAddress().ToString());
					if(nbNetwork==0)
					{
						//macAddrRef=nif.GetPhysicalAddress().ToString();
						//PlayerPrefs.SetString("nifRef",macAddrRef);
						byte[] macAddress = nif.GetPhysicalAddress().GetAddressBytes();
						sbyte[] realMacAddr = new sbyte[macAddress.Length];
						
						for(int i=0;i<macAddress.Length;i++)
						{
							realMacAddr.SetValue((sbyte)macAddress[i],i);
							//Debug.Log("realMacAddr["+i+"] = "+realMacAddr[i]);
							DbgUtils.Log("MACOSXPB","realMacAddr["+i+"] = "+realMacAddr[i]);
						}
						if (nif.GetPhysicalAddress().ToString() != String.Empty)
						{
							macAddr = encodeSBytesUpperCase(realMacAddr);
							Debug.Log("macAdd "+macAddr);
							DbgUtils.Log("MACOSXPB","macAdd "+macAddr);
							PlayerPrefs.SetString("nifRef",macAddr);
						}
					}
					nbNetwork++;
				}
			}
			catch(System.Exception e)
			{
				DbgUtils.Log("MACOSXPB","Exception "+e.Message);
				PlayerPrefs.SetString("nif0","000000000000");
				PlayerPrefs.SetString("nifRef","000000000000");
				macAddr = "000000000000";
			}
			
		DbgUtils.Log("MACOSXPB","!nifRef 1");
		}
		else
		{
			DbgUtils.Log("MACOSXPB","nifRef 0");
			List<string> nifList = new List<string>();
			int iter=0;
			while (PlayerPrefs.HasKey("nif"+iter))
			{
				nifList.Add(PlayerPrefs.GetString("nif"+iter));
				iter++;
			}
			int nbNetwork=0;
			bool founded = false;
			DbgUtils.Log("MACOSXPB","nifRef 1");
			try{
				foreach (NetworkInterface nif in NetworkInterface.GetAllNetworkInterfaces())
				{
					
					DbgUtils.Log("MACOSXPB","1 nif"+nbNetwork+" "+nif.GetPhysicalAddress().ToString());
					DbgUtils.Log("MACOSXPB","1  nif type "+nbNetwork+" "+nif.NetworkInterfaceType.ToString());
					nbNetwork++;
					if((nif.NetworkInterfaceType==NetworkInterfaceType.Tunnel)
					||(nif.NetworkInterfaceType==NetworkInterfaceType.Loopback)
					||(nif.NetworkInterfaceType==NetworkInterfaceType.Unknown))
						continue;
					DbgUtils.Log("MACOSXPB","1 nif ok"+(nbNetwork-1)+" "+nif.GetPhysicalAddress().ToString());
					if(nifList.Contains(nif.GetPhysicalAddress().ToString()))
					{
						macAddr = PlayerPrefs.GetString("nifRef");
						Debug.Log("macAdd Ref "+macAddr);
						DbgUtils.Log("MACOSXPB","macAdd Ref "+macAddr);
						founded = true;
						break;
					}
				}
			}
			catch(System.Exception e)
			{
				DbgUtils.Log("MACOSXPB","Exception "+e.Message);
				macAddr = "000000000000";
			}
			
			DbgUtils.Log("MACOSXPB","nifRef 1");
		}
			DbgUtils.Log("MACOSXPB","ReturnMacPlayerRefAddress 2");
		//PlayerPrefs.DeleteKey("nifRef");
		PlayerPrefs.Save();
		return macAddr;
      }
	
	public String createSendCode(bool act,bool renew,int login,string mdp)
	{		
		DbgUtils.Log("MACOSXPB","createSendCode 1");
		bool _activation = act;
		bool _renouvellement = renew;
		
		//Activation
		int activ;
		if(_activation == true)
			activ = 0x2;
		else
			activ = 0x1;
		
		if(_renouvellement == true)
			activ |= 0x1;
		else
			activ |= 0x0;
		char active = (char)(activ+65);
		
		
		//MAC ADDRESS
		String macAdd="";
		String macAddRef="";
#if UNITY_ANDROID && !UNITY_EDITOR
			macAdd = ReturnMacAndroidAddress();
			macAdd = macAdd.Replace(":","");
			byte[] macAddress2 = StringToByteArray(macAdd);
			sbyte[] realMacAddr2 = new sbyte[macAddress2.Length];
			for(int i=0;i<macAddress2.Length;i++)
			{
				realMacAddr2.SetValue((sbyte)macAddress2[i],i);
			}
			if (macAdd != String.Empty)
			{
				macAdd = encodeSBytesUpperCase(realMacAddr2);
			}
#elif UNITY_IPHONE && !UNITY_EDITOR
		macAdd = ReturnMacPlayerRefAddress();
	 	//macAdd = EtceteraBinding.uniqueDeviceIdentifier();
		Debug.Log("UUID : "+macAdd);
		macAdd = macAdd.ToUpper();
		Debug.Log("UUID encodeSBytesUpperCase : "+macAdd);
		if(macAdd.Length>12)
		{
			macAdd = macAdd.Substring(0,12);
		}
		Debug.Log("UUID macAdd : "+macAdd);
		/* uniqueDeviceIdentifier et uniqueGlobalDeviceIdentifier EtceteraManager.mm
		 NSString *udid;
		 udid = [UIDevice currentDevice].identifierForVendor.UUIDString;
		 return [self MD5HashString:udid];
    	*/
#else
		DbgUtils.Log("MACOSXPB","ReturnMacPlayerRefAddress 0");
		macAdd = ReturnMacPlayerRefAddress();
#endif
		//IDCLIENT
		int idC = login;
		//String idclient = encodeBytesUpperCase(convertToBytes(idC));
		String idclient = encodeSBytesUpperCase(convertToSBytes(idC)); 	
		
		//DATE
		byte[] date = convertToBytes(encodeDate());
		sbyte[] realDate = new sbyte[date.Length];
		
		for(int i=0;i<date.Length;i++)
		{
			realDate.SetValue((sbyte)date[i],i);
		}
		
		Char[] date1 = encodeByteUpperCase(realDate[1]);
		Char[] date2 = encodeByteUpperCase(realDate[2]);
		Char[] date3 = encodeByteUpperCase(realDate[3]);
		
		String dateFinal = ""+date1[0]+date1[1]+date2[0]+date2[1]+date3[0]+date3[1];
		
		//ID SOFT
		Char[] idSoft = encodeByteUpperCase(3);
		String ids = ""+idSoft[0]+idSoft[1];
		
		//Version
		Char[] version = encodeByteUpperCase(2);
		String vers = ""+version[0]+version[1];
		
		//RESELLER
		short re = 0;
		String ReSeller = encodeBytesUpperCase(convertShortToBytes(re));
		
		//PASSWORD
		String pwd = mdp;
		String ActivatioCode = active + macAdd + idclient + dateFinal+ids+vers+ReSeller+pwd;
		
		
		if(debugMode)
		{
			Debug.Log("ACTIV = "+active);
			Debug.Log("MAC adresse = "+ macAdd);
			Debug.Log("IDCLIENT = "+ idclient);
			Debug.Log("DATE = "+ dateFinal);
			Debug.Log("IDSOFT = "+ids);
			Debug.Log("VERSION = "+vers);
			Debug.Log("RESELLER = "+ReSeller);
			Debug.Log("PASSWORD = "+pwd);
			Debug.Log("FINAL KEY = "+ ActivatioCode);
		}
		
			DbgUtils.Log("MACOSXPB","ACTIV = "+active);
			DbgUtils.Log("MACOSXPB","MAC adresse = "+ macAdd);
			DbgUtils.Log("MACOSXPB","IDCLIENT = "+ idclient);
			DbgUtils.Log("MACOSXPB","DATE = "+ dateFinal);
			DbgUtils.Log("MACOSXPB","IDSOFT = "+ids);
			DbgUtils.Log("MACOSXPB","VERSION = "+vers);
			DbgUtils.Log("MACOSXPB","RESELLER = "+ReSeller);
			DbgUtils.Log("MACOSXPB","PASSWORD = "+pwd);
			DbgUtils.Log("MACOSXPB","FINAL KEY = "+ ActivatioCode);
		
		
		return ActivatioCode;
		
	}
	
	//ENVOI DU CODE -------------------------------------
//	
//	public string HttpPost (string uri, string parameters)
//	{ 
//	   // parameters: name1=value1&name2=value2	
//	   WebRequest webRequest = WebRequest.Create (uri);
//	   //string ProxyString = 
//	   //   System.Configuration.ConfigurationManager.AppSettings
//	   //   [GetConfigKey("proxy")];
//	   //webRequest.Proxy = new WebProxy (ProxyString, true);
//	   //Commenting out above required change to App.Config
//	   webRequest.ContentType = "application/x-www-form-urlencoded";
//	   webRequest.Method = "POST";
//	   byte[] bytes = Encoding.ASCII.GetBytes (parameters);
//	   Stream os = null;
//	   try
//	   { // send the Post
//	      webRequest.ContentLength = bytes.Length;   //Count bytes to send
//	      os = webRequest.GetRequestStream();
//	      os.Write (bytes, 0, bytes.Length);         //Send it
//	   }
//	   catch (WebException ex)
//	   {
//	      Debug.Log( ex.Message + "HttpPost: Request error");
//	   }
//	   finally
//	   {
//	      if (os != null)
//	      {
//	         os.Close();
//	      }
//	   }
//	 
//	   try
//	   { // get the response
//	      WebResponse webResponse = webRequest.GetResponse();
//			Debug.Log(webResponse.ToString());
//	      if (webResponse == null) 
//	         { return null; }
//	      StreamReader sr = new StreamReader (webResponse.GetResponseStream());
//	      return sr.ReadToEnd ().Trim ();
//	   }
//	   catch (WebException ex)
//	   {
//	    	Debug.Log( ex.Message + "HttpPost: Response error");
//			//Activation_old.setErrorMessage("Server connection fail");
//	   }
//	   return null;
//	}
	
	//ENCODE TOOLSET -------------------------------------
	
	public String encodeBytesUpperCase(byte[] buffer) {
		StringBuilder sb = new StringBuilder(buffer.Length*2);
		for (int i = 0; i < buffer.Length; i++) {
			sb.Append(encodeByteUpperCase(buffer[i]));
		}
		return sb.ToString();
	}
	
	public String encodeSBytesUpperCase(sbyte[] buffer) {
		StringBuilder sb = new StringBuilder(buffer.Length*2);
		for (int i = 0; i < buffer.Length; i++) {
			sb.Append(encodeByteUpperCase(buffer[i]));
		}
		return sb.ToString();
	}
	
	public Char[] encodeByteUpperCase(Byte value)
	{
		char[] result = new char[2];
		int val = value;
		val = 128 + val;
		
		result.SetValue(getMultipleOfLetter(((int)val)/26),0 );
		result.SetValue((char)(val%26 + 65),1);
		return result;
	}
	
	public Char[] encodeByteUpperCase(sbyte value)
	{
		char[] result = new char[2];
		int val = value;
		val = 128 + val;
		
		result.SetValue(getMultipleOfLetter(((int)val)/26),0 );
		result.SetValue((char)(val%26 + 65),1);
		return result;
	}
	
	public char getMultipleOfLetter(int val) 
	{
		int mult = (RandomNumber(0,2)); // valeur entre 0 et 2
		val += mult*10;
		if(val >= 26) // pas de lettre après 26. cas si val > 6 et mult == 2
			val -= 10;
		
		return (char)(val + 65);
	}
	
	public int RandomNumber(int min, int max)
	{
		System.Random random = new System.Random();
		return random.Next(min, max);
	}
	
	public byte[] convertToBytes(int integer) 
	{
        byte[] byteArray = new byte[4];

        byteArray[0] = (byte) (integer >> 24);
        byteArray[1] = (byte) (integer >> 16);
        byteArray[2] = (byte) (integer >> 8);
        byteArray[3] = (byte) integer;
        return byteArray;
    }
	
	public sbyte[] convertToSBytes(int integer) 
	{
        sbyte[] byteArray = new sbyte[4];

        byteArray[0] = (sbyte) (integer >> 24);
        byteArray[1] = (sbyte) (integer >> 16);
        byteArray[2] = (sbyte) (integer >> 8);
        byteArray[3] = (sbyte) integer;
        return byteArray;
    }
	
	public byte[] convertShortToBytes(short value) {
        byte[] byteArray = new byte[2];

        byteArray[0] = (byte) (value >> 8);
        byteArray[1] = (byte) value;
        return byteArray;
    }
	
	public int encodeDate() {
		// date encodée = jour|mois|année|checkSum sur 24 bits

		// jour sur 5 bits (commence à 0)
		int day = System.DateTime.Now.Day-1;
		day = (day << 19) & 0xF80000;
		// mois sur 4 bits (commence à 0)
		int month = System.DateTime.Now.Month-1;
		month = (month << 15) & 0x78000;
		// année sur 10 bits : nombre d'années écoulées depuis 1970
		int year = System.DateTime.Now.Year-1970;
		year = (year << 5) & 0x7FE0;
		
		int encodedDate = day | month | year;
		// check sum en dernier sur 5 bits
		int checkSum = count_bits(encodedDate);
		encodedDate |= (checkSum & 0x1F);
		
		return encodedDate;
	}
	
	public int count_bits(int data)
	{   
		int count = 0;
	
		while (data != 0)
		{    data = data & (data - 1);
		count++;
		}
	
		return count;
	}	
}
