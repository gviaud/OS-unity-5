using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text; 

public class SaveToolSet{
	
	//Plus utilisé !!! car sauvegarde dans la BDR
	//pour ça voir RegisterRW
	string _FileLocation = Application.dataPath;
  	string _FileName="SaveData.xml";
	string _data; 
	Data myData;
	
	public struct Data
		{
		  	public bool activated;
			public int endDay;
			public int endMonth;
			public int endYear;
		}
	
	public SaveToolSet()
	{
		myData.activated = false;
		myData.endDay = 0;
		myData.endMonth = 0;
		myData.endYear = 0;
	}
	
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	// Méthodes persos -----------------------
	
	public void saveConfig(int d, int m, int y)
	{
	
	myData.activated = true;
	myData.endDay = d;
	myData.endMonth = m;
	myData.endYear = y;
	//TODO le reste
	
	
	// create XML!
	_data = SerializeObject(myData);
	CreateXML(); 
	
	}
	public bool loadConfig()
	{
		LoadXML();
		if(_data.ToString() != "")
		{
			myData = (Data)DeserializeObject(_data);
			//TODO le reste
		} 
		return false;
	}
	
	//------------------------------------------
	
	void CreateXML()
  	{
      StreamWriter writer;
      FileInfo t = new FileInfo(_FileLocation+"\\"+ _FileName);
      if(!t.Exists)
      {
         writer = t.CreateText();
      }
      else
      {
         t.Delete();
         writer = t.CreateText();
      }
      writer.Write(_data);
      writer.Close();
      Debug.Log("File written.");
	}
   
	void LoadXML()
	{
		StreamReader r = File.OpenText(_FileLocation+"\\"+ _FileName);
		string _info = r.ReadToEnd();
		r.Close();
		_data=_info;
		Debug.Log("File Read");
	}
	
	string SerializeObject(object pObject)
	{
		string XmlizedString = null;
		MemoryStream memoryStream = new MemoryStream();
		XmlSerializer xs = new XmlSerializer(typeof(Data));
		XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
		xs.Serialize(xmlTextWriter, pObject);
		memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
		XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
		return XmlizedString;
	}
   
   object DeserializeObject(string pXmlizedString)
	{
		XmlSerializer xs = new XmlSerializer(typeof(Data));
		MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
		XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
		return xs.Deserialize(memoryStream);
	} 
	string UTF8ByteArrayToString(byte[] characters)
	{     
	  UTF8Encoding encoding = new UTF8Encoding();
	  string constructedString = encoding.GetString(characters);
	  return (constructedString);
	}
   
   	byte[] StringToUTF8ByteArray(string pXmlString)
	{
	  UTF8Encoding encoding = new UTF8Encoding();
	  byte[] byteArray = encoding.GetBytes(pXmlString);
	  return byteArray;
	} 
}
