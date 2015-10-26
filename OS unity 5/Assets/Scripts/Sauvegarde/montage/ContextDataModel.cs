using UnityEngine;
using System.Collections;
using System.IO;

public class ContextDataModel
{
	/* ContextDataModel
	 * contient les données de:
	 * # thumbnail du montage
	 * # dates
	 * # nb objets
	 * # commentaire
	 * #...
	 * */
	
	public string m_birthDate;
	public string m_lastOpenDate;
	public string m_comments;
	public string m_owner;
	public string versionSave="0.0";
	private bool _hasVersion = false;
	
	int m_nbObjets;
	
	public Texture2D m_thumbnail;
	
	public ContextDataModel()
	{
		m_birthDate = getTodayDate();
		m_lastOpenDate = getTodayDate();
		m_owner = SystemInfo.deviceName;
		m_comments = "";
		m_nbObjets = 0;
		m_thumbnail = new Texture2D(256,256);
//		m_thumbnail = new Texture2D(Screen.width,Screen.height);
		
		//debug
//		if(Application.platform == RuntimePlatform.WindowsEditor ||
//			Application.platform == RuntimePlatform.OSXEditor)
//		{
//	    	Debug.Log("birthDate "+ m_birthDate);
//			Debug.Log("lastOpenDate "+ m_lastOpenDate);
//			Debug.Log("owner "+ m_owner);
//		}
	}
	
	public void SetHasVersion(bool stateVersion)
	{
		_hasVersion = stateVersion;
		Debug.Log("SetHasVersion value "+_hasVersion);
		if(_hasVersion==false)
			versionSave="0.0";
	}
	
	public bool HasVersion()
	{
		return _hasVersion;
	}
	//Update's
	public void update(string com)
	{

		//if(com != "")
		//{
			m_comments = com;	
		//}
	}
	public void update(int nbobj)
	{
		if(nbobj>0)
		{
			m_nbObjets = nbobj;	
		}
	}
	public void updateThumb(Texture2D thumb)
	{
		if(thumb != null)
		{
//			m_thumbnail.SetPixels(thumb.GetPixels());
//			m_thumbnail.Apply();
//			thumb.Resize(Screen.width/4,Screen.height/4);
//			thumb.Apply();
			m_thumbnail = new Texture2D(256,256);
			m_thumbnail = thumb;
		}
	}
	
	//Save/Load
	public void save(BinaryWriter buf)
	{
		try{
			buf.Write(m_birthDate); 	//STRING
	//		buf.Write(lastOpenDate);//STRING
			buf.Write(getTodayDate());//STRING
			buf.Write(m_owner);		//STRING
			buf.Write(m_comments);	//STRING
			buf.Write(m_nbObjets);	//INT
			if(m_thumbnail != null)
			{
				byte[] img = m_thumbnail.EncodeToJPG();
				buf.Write(img.Length);	//INT
				buf.Write(img);			//BYTE[]
			}
			else
			{
				buf.Write(-1);	
			}
			//if(_hasVersion)
			{
				buf.Write(usefullData.version_ID);
			}
		}
		catch(UnityException e){
			Debug.Log ("error saving contextdata models :"+e.ToString());
		}
	}
	
	public void load(BinaryReader buf)
	{
		reset();
		
		m_birthDate = buf.ReadString();
		m_lastOpenDate = buf.ReadString();
		m_owner = buf.ReadString();
		m_comments = buf.ReadString();
		m_nbObjets = buf.ReadInt32();
		
		//load thumbnail
		int length = buf.ReadInt32();
		if(length != -1)
		{
			m_thumbnail.LoadImage(buf.ReadBytes(length));
		}
		if(_hasVersion)
		{
			versionSave =  buf.ReadString();
		}
	}
	
	//AUX. FCN.
	private string getTodayDate()
	{
		string s = 	System.DateTime.Now.Day + "/" +
					System.DateTime.Now.Month + "/" +
					System.DateTime.Now.Year;
		return s;
	}
	
	public void reset()
	{
		m_birthDate = "";
		m_comments = "";
		m_lastOpenDate = "";
		m_owner = "";
		m_thumbnail = new Texture2D(256,256);
//		m_thumbnail = new Texture2D(Screen.width,Screen.height);
	}
}
