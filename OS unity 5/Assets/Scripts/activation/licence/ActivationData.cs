using UnityEngine;
using System.Collections;
using System.Text;
using System;

public class ActivationData {
	
	sbyte _random;
	sbyte _checkSum;
	int _endDate;
	sbyte[] _macAddress = new sbyte[6];
	sbyte _idsoft;
	sbyte _version;
	short _revendeur;
	sbyte _nbLibs;
	int[] _clearEndDate = new int[3];
	
	int _login;
	string _mdp;
	
	bool debugMode = true;
	
	
//	// Use this for initialization
//	void Start () {
//	
//	}
//	
//	// Update is called once per frame
//	void Update () {
//	
//	}
	
	/*
	 * fonction qui renseigne les données d'activation a partir d'un buffer
	 * (sbyte[])
	 * */
	public void setInfo(sbyte[] buffer,int login,string mdp)
	{
		_login = login;
		_mdp = mdp;
		int off = 0;
		_random = buffer[off]; 						off += 1;
		_checkSum = buffer[off]; 					off += 1;
		// copie les 3 octets de la date
		_endDate = 0x0;
		_endDate |= (buffer[off] << 16) & 0xFF0000;	off += 1;
		_endDate |= (buffer[off] << 8)  & 0x00FF00;	off += 1;
		_endDate |=  buffer[off]	    & 0x0000FF;	off += 1;
		
		_clearEndDate = setDate(_endDate);
		
		// copie l'adresse MAC
		for (int i = 0; i < _macAddress.Length; i++) 
		{
			_macAddress[i] = buffer[off]; 			off += 1;
		}
		
		_idsoft = buffer[off]; 					off += 1;
		_revendeur = getShort(buffer, off);	off += 2;
		_version = buffer[off];		  		off += 1;
		_nbLibs = buffer[off];		  		  //off += 1;
		
		int count = countByte(buffer); //pour tester avec le checksum
		
		/*if(debugMode)
		{
			Debug.Log("What is in Activation data");
			Debug.Log("RANDOM = "+_random);
			Debug.Log("CheckSum = "+ ((byte)_checkSum) + " - check = "+count);
			Debug.Log("_endDate = "+ _endDate);
			Debug.Log("CLEAR END DATE = "+_clearEndDate[0]+"/"+_clearEndDate[1]+"/"+_clearEndDate[2]);
			Debug.Log("_macAddress = "+macToString(_macAddress));
			Debug.Log("idsoft = "+_idsoft);
			Debug.Log("version = "+_version);
			Debug.Log("revendeur = "+_revendeur);
			Debug.Log("nbLibs = "+_nbLibs);
		}*/
	}
	
	/*
	 * fonction qui sérialise les donnés d'activations et les
	 * enregistre dans la base de registre
	 * */
	
	public string serializeInfos(EncodeToolSet encoder)
	{
		sbyte[] buffer = new sbyte[16];
		int off7 = 0;
		
		buffer[off7] = _random; 				off7++;//0
		buffer[off7] = _checkSum; 				off7++;//1
		buffer[off7] = (sbyte)(_endDate >> 16); off7++;//2
		buffer[off7] = (sbyte)(_endDate >> 8); 	off7++;//3
		buffer[off7] = (sbyte)(_endDate); 		off7++;//4
		
		for(int i=0 ; i<_macAddress.Length ; i++)
		{
			buffer[off7] = _macAddress[i];
			off7++;
		}
		
		buffer[off7] = _idsoft;					off7++;
		putShort(buffer,off7,_revendeur);		off7=off7 + 2;
		buffer[off7] = _version;					off7++;
		buffer[off7] = _nbLibs;

		return encoder.encodeSBytesUpperCase(buffer);
	}
	
	public void deSerializeInfos(DecodeToolSet decoder,string infos)
	{
		sbyte[]buffer = decoder.decodeSBytesUpperCase(infos);
		if(buffer.Length>0)
		{
			int _endDate = 0x0;
			_endDate |= (buffer[2] << 16)& 0xFF0000;
			_endDate |= (buffer[3] << 8)& 0x00FF00;
			_endDate |=  (buffer[4])& 0x0000FF;
			if(_clearEndDate[0] ==0 && _clearEndDate[1] ==0 && _clearEndDate[2] ==0)
			{
				_clearEndDate = setDate(_endDate);
			}
		}
	}
	
/*--------------------------Data toolset-----------------------*/
	
	private string macToString(sbyte[] mac)
	{
		StringBuilder output = new StringBuilder();
		int i = 0;
		foreach(sbyte b in mac)
		{
			int b1 = (b >> 4)&0x0F;
			int b2 = b & 0x0F;
			if((int)b1 <10)
			{
				output.Insert(i,b1);
				i++;
			}
			else
			{
				output.Insert(i,(char)(b1-10+65));
				i++;
			}
			if((int)b2 <10)
			{
				output.Insert(i,b2);
				i++;
			}
			else
			{
				output.Insert(i,(char)(b2-10+65));
				i++;
			}
		}
		return output.ToString();
	}
	
	private short getShort(sbyte[] b,int off)
	{
		return (short) (((b[off + 1] & 0xFF) << 0) + ((b[off + 0]) << 8));	
	}
	
	private void putShort(sbyte[] b, int off, short val)
	{
		b[off + 1] = (sbyte) (val >> 0);
		b[off + 0] = (sbyte) (val >> 8);
	}
	
	public void setClearEndDate(int[] _date)
	{
		for(int i = 0; i < 3; i++)
		{
			_clearEndDate[i] = _date[i];
		}
	}
	
	private int[] setDate(int endDate) {
	
		int[] outdate = new int[3];
		// jour sur 5 bits (commence à 0)
		int day = ((endDate >> 19) & 0x1F)+1;
		// mois sur 4 bits (commence à 0)
		int month = ((endDate >> 15) & 0xF)+1;
		// année sur 10 bits : nombre d'années écoulées depuis 1970
		int year = ((endDate >> 5) & 0x3FF) + 1970;
		// check sum en dernier sur 5 bits
		int checkSum = endDate & 0x1F;
		//if(checkSum != EncodeToolSet.count_bits(endDate >> 5))
		//	Debug.Log("BAD VALUES ERROR !!!!!!!!!!!!!!" + EncodeToolSet.count_bits(endDate >> 5) +" != "+checkSum);
		
		outdate.SetValue(day,0);
		outdate.SetValue(month,1);
		outdate.SetValue(year,2);
		return outdate;
	}
		
	public int getLogin()
	{
		return _login;
	}
	
	public string getMdp()
	{
		return _mdp;
	}
	
	public void setLogin(int i)
	{
		_login = i;
	}
	
	public void setMdp(string str)
 	{
		_mdp = str;
  	}
	
	public int countByte(sbyte[] sbytes)
	{
		byte[] bytes = new byte[sbytes.Length];
		
		for(int i=0;i<sbytes.Length;i++)
		{
			if(i!=1)
			{
				byte b = (byte)(Math.Abs((int)sbytes[i]));
				bytes.SetValue(b,i);
			}
		}
		
		int c = 0;
		foreach(byte ab in bytes)
		{
			for(int i=0;i<8;i++)
			{
				c = c + (int)(ab>>i & 0x1);
			}
		}
		return c;
	}
	
	public int[] getEndDate()
	{
		return _clearEndDate;
	}
		
}
