using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Net;
using System.Text;

public class ExtraControl : MonoBehaviour,PluginsOS3D {
	
	#region Attributs
	private ExtraData _extraData;
	
	private string[] m_separator = new string[1];
	
	private Texture2D m_sendPic;
	
	private bool m_plugAuthorization = false;
	
	private const string c_pluginName = "extrabat";
	#endregion
	
	#region test var's
	private string debugString;
	private float t;
	#endregion
	
	void Awake()
	{
		ExtraEvent.startAuth += startAuth;
		ExtraEvent.startSearch += startSearch;
		ExtraEvent.setShop += setShp;
		ExtraEvent.sendIt += sendFile;
		
		ModulesAuthorization.moduleAuth += AuthorizePlugin;
	}
	
	void OnDestroy()
	{
		ExtraEvent.startAuth -= startAuth;
		ExtraEvent.startSearch -= startSearch;
		ExtraEvent.setShop -= setShp;
		ExtraEvent.sendIt -= sendFile;
		
		ModulesAuthorization.moduleAuth -= AuthorizePlugin;
	}
	
	// Use this for initialization
	void Start ()
	{
		m_separator[0] = "<br/>";
		_extraData = new ExtraData();
		m_sendPic = new Texture2D(1,1);
		enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	#region connection
	
	//Authentification extrabat
	private void startAuth(string login,string mdp)
	{
		StartCoroutine(createAndSendAuth(login,mdp));	
	}
	IEnumerator createAndSendAuth(string login,string mdp)
	{
//		string url = ExtraApi.url_Main + ExtraApi.url_AuthExt;// > envoi vers php de lien oneshot3d
		string url = "http://www.pointcube.com/extrabat/connexion.php";
		
		WWWForm form = new WWWForm();
		form.AddField(ExtraApi.appName,"os3d");
		form.AddField(ExtraApi.thirdPartyKey,_extraData.getOS3DKey());
		form.AddField(ExtraApi.extraLogin,login);
		form.AddField(ExtraApi.extraMdp,mdp);
		
		WWW www = new WWW(url,form);
		yield return www;
		if(www.error != null)
		{
			Debug.LogError("ERROR "+www.error);
		}
		else
		{
			continueAuth(www.text);
		}
		
		yield return null;
	}
	
	//Update liste des shops
	IEnumerator updateShops()
	{
		string url = "http://www.pointcube.com/extrabat/magasins.php";
		
		WWWForm form = new WWWForm();
		form.AddField(ExtraApi.appName,"os3d");
		form.AddField(ExtraApi.thirdPartyKey,_extraData.getOS3DKey());
		form.AddField(ExtraApi.privateKey,_extraData.getPrivateKey());
		
		WWW www = new WWW(url,form);
		yield return www;
		if(www.error != null)
		{
			Debug.LogError("ERROR "+www.error);
		}
		else
		{
			updateShopsList(www.text);
		}
		
		yield return null;
	}
	
	//recherche client
	private void startSearch(string str)
	{
		StartCoroutine(createAndSendSearch(str));
	}
	IEnumerator createAndSendSearch(string s)
	{
		string url = "http://www.pointcube.com/extrabat/search.php";
		
		WWWForm form = new WWWForm();
		form.AddField(ExtraApi.appName,"os3d");
		form.AddField(ExtraApi.thirdPartyKey,_extraData.getOS3DKey());
		form.AddField(ExtraApi.privateKey,_extraData.getPrivateKey());
		form.AddField(ExtraApi.idShop,_extraData.getSelectedShop());
		form.AddField(ExtraApi.like,s);
		
		WWW www = new WWW(url,form);
		yield return www;
		if(www.error != null)
		{
			Debug.LogError("ERROR "+www.error);
		}
		else
		{
			autoComplete(www.text);
		}
		
		yield return null;
	}
	
	//envoie et confirmation ficher sur serveur extrabat
	private void sendFile(string s)// i > id du client
	{
		StartCoroutine(createAndSendFileOnFtp(s));
	}
	IEnumerator createAndSendFileOnFtp(string id)
	{	
		ExtraEvent.firePleaseWait(true);
		t = Time.time;
		debugString = "-started:"+t;
		yield return new WaitForSeconds(1);
		byte[] img = m_sendPic.EncodeToPNG();
		string fileName = Path.GetRandomFileName();
		fileName = fileName.Replace(".","");
//		Debug.Log("Unique fName>" + fileName+"<");
		
//		ALTERNATE VERSION VV DOESN'T WORK MOUAHAHAH ^^
//		FileStream fs =  File.Open(Application.persistentDataPath+"/test.png",FileMode.Open);
//		fs.Flush();
//		fs.Write(img,0,img.Length);
//		fs.Close();
//		
//		string filename = "Bitcherland.png";
//		FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://www.oneshot3d.com/ExtraTest/"+filename);
//		ftpRequest.Method = WebRequestMethods.Ftp.UploadFileWithUniqueName;
//		ftpRequest.Credentials = new NetworkCredential("sys_pointcub","gx8FztlI7kpM");
//		
////		StreamReader sr = new StreamReader(Application.persistentDataPath+"/test.png");
////		byte[] fileContent = Encoding.UTF8.GetBytes(sr.ReadToEnd());
////		sr.Close();
////		
////		ftpRequest.ContentLength = fileContent.Length;
//		
//		Stream requestStream = ftpRequest.GetRequestStream();
//		requestStream.Write(img,0,img.Length);
//		requestStream.Close();
//		
//		FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
//    
////      Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
//    	Debug.Log("UploadFile complete, >"+response.StatusDescription);
//        response.Close();
		
		debugString += "-startwc:"+Time.time;
		WebClient wc = new WebClient();
		
		wc.Credentials = new NetworkCredential("sys_pointcub","gx8FztlI7kpM"); //local
		
		wc.UploadData("ftp://www.pointcube.com/extrabat/img/"+fileName+".png",img);
		wc.Dispose();
		
		debugString += "-endwc:"+Time.time;
		Debug.Log("COROUTINE UPLOAD FINNISHED");
		yield return new WaitForSeconds(1);
		
		StartCoroutine(fileOnServer(id,fileName));
		yield return null;
	}
	
	IEnumerator fileOnServer(string id, string filename)
	{
		WWWForm form = new WWWForm();
		//params extrabat
		form.AddField(ExtraApi.appName,"os3d");
		form.AddField(ExtraApi.thirdPartyKey,_extraData.getOS3DKey());
		form.AddField(ExtraApi.privateKey,_extraData.getPrivateKey());
		form.AddField(ExtraApi.idClient,id);
		form.AddField(ExtraApi.fileName,(filename+".png"));
		form.AddField("magasin_id",_extraData.getSelectedShop());
		
		string url = "http://www.pointcube.com/extrabat/sendFTP.php";
		
		WWW www = new WWW(url,form);
		yield return www;
		debugString = "-returnForm:"+Time.time;
		if(www.error != null)
		{
			Debug.LogError("ERROR "+www.error);
			ExtraEvent.firePleaseWait(false);
		}
		else
		{
			Debug.Log("RETOUR >"+www.text+"<");
			ExtraEvent.firePleaseWait(false);
		}
		debugString += "-totalTime:"+(Time.time - t).ToString();
		Debug.Log(debugString);
		yield return null;
	}
	
	#endregion
	
	#region aux fcns
	private void continueAuth(string result)
	{
		Debug.Log("RESULT : "+result);
		string[] segmented = result.Split(m_separator,System.StringSplitOptions.None);
		
		string tmpKey = "";
		bool authValid = false;
		
		foreach(string str in segmented)
		{
			if(str != "")
			{
				Debug.Log(">"+str+"<");
				if(str.StartsWith(ExtraApi.privateKey))
				{
//					_extraData.setPrivateKey(str.Replace((ExtraApi.privateKey+"="),""));
//					ExtraEvent.fireSuccessAuth();
//					_extraData.clearShops();
					tmpKey = str.Replace((ExtraApi.privateKey+"="),"");
				}
				if(str.StartsWith(ExtraApi.authValid))
				{
					if(str.Replace((ExtraApi.authValid+"="),"") == "1")
						authValid = true;
				}
			}
		}
		
		if(tmpKey != "" && authValid)
		{
			_extraData.setPrivateKey(tmpKey);
			ExtraEvent.fireSuccessAuth();
			_extraData.clearShops();
			
			StartCoroutine(updateShops());
		}
		
//		ExtraEvent.fireShopListChange(_extraData.getShops());
//		
//		if(_extraData.getShops().Count == 1)// autoselection du shop si yen a qu'un
//		{
//			foreach(string s in _extraData.getShops().Keys)
//			{
//				ExtraEvent.fireSetShop(s);	
//			}
//		}
	}
	
	private void autoComplete(string list)
	{
		Debug.Log("RESULT : "+list);
		string[] segmented = list.Split(m_separator,System.StringSplitOptions.None);
		Debug.Log("SEGMENTED>"+segmented.Length+"<");
		Dictionary<string,string> clients = new Dictionary<string, string>();
		if(list != "")
		{
			foreach(string str in segmented)
			{
				if(str != "")
				{
					clients.Add(str.Split('=')[0],str.Split('=')[1]);
				}
			}
		}
		else
		{
			clients.Add("-1","no Results");
		}
		ExtraEvent.fireClientListChange(clients);
	}
	
	private void updateShopsList(string result)
	{
		Debug.Log("SHoPS LIST : "+result);
		string[] segmented = result.Split(m_separator,System.StringSplitOptions.None);
		
		Dictionary<string,string> shops = new Dictionary<string, string>();
		
		foreach(string str in segmented)
		{
			if(str != "")
			{
				shops.Add(str.Split('=')[0],str.Split('=')[1]);
			}
		}
		ExtraEvent.fireShopListChange(shops);
		
		if(/*_extraData.getShops()*/shops.Count == 1)// autoselection du shop si yen a qu'un
		{
			foreach(string s in shops.Keys)
			{
				ExtraEvent.fireSetShop(s);	
			}
		}
		if(PlayerPrefs.HasKey(ExtraData.pk_selectedShop))// autoselection du shop si déjà été logué et shop choisi
		{
			string shp = PlayerPrefs.GetString(ExtraData.pk_selectedShop);
			if(shp != "")
			{
				foreach(string s in shops.Keys)
				{
					if(s == shp)
					{
						ExtraEvent.fireSetShop(s);
					}
				}
			}
		}
	}
	
	private void setShp(string s)
	{
		_extraData.setSelectedShop(s);
		ExtraEvent.fireStartSearch("");
	}
	
	IEnumerator StartExtraBat()
	{
		//Test si Private Key deja enregistrée
		if(PlayerPrefs.HasKey(ExtraData.pk_privateKey))
		{
			if(PlayerPrefs.GetString(ExtraData.pk_privateKey) != "")
			{
				_extraData.setPrivateKey(PlayerPrefs.GetString(ExtraData.pk_privateKey));
				ExtraEvent.fireSuccessAuth();
				yield return StartCoroutine(updateShops());
			}
			else
				ExtraEvent.fireNeedAuth();
		}
		else
			ExtraEvent.fireNeedAuth();
		
		// Screenshot
		Camera.main.GetComponent<Screenshot>().ShowHideGui(false);
		yield return new WaitForEndOfFrame();
		yield return StartCoroutine(Camera.main.GetComponent<Screenshot>().GenericShot(m_sendPic,true));
		yield return new WaitForEndOfFrame();
		
		//Demarrage de la GUI
		ExtraEvent.fireUIActivation(true);
		
		yield return null;
	}
	
	public Texture2D getPreview()
	{
		return m_sendPic;	
	}
	
	public void QuitPlugin()
	{
		ExtraEvent.fireUIActivation(false);
		Camera.main.GetComponent<Screenshot>().ShowHideGui(true);
		enabled = false;
	}
	
	#endregion
	
	#region plugin Interface
	public string GetPluginName()
	{
		return c_pluginName;	
	}
	
	public void StartPlugin()
	{
		enabled = true;
		StartCoroutine(StartExtraBat());	
	}
	
	public void AuthorizePlugin(string name,bool auth)
	{
		if(name == c_pluginName)
			m_plugAuthorization = auth;
	}
	
	public bool isAuthorized()
	{
		return m_plugAuthorization;
	}
	
	#endregion
	
}
