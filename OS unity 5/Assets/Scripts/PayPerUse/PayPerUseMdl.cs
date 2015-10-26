using UnityEngine;
using System.Collections;
using System;

public class PayPerUseMdl
{
	#region --Attributes and constructor--
	private ArrayList m_days;
	
	private bool m_needToSendCode;
	
	private string m_code = "";
	
	private const string	c_payperuseKey = "PPUKEY";
	private const string	c_ppuKWaitSending= "PPUSEND";
	private const string	c_url = "http://127.0.0.1:8080/OneShot_admin/payperuse/payperuse.php";
	
	private EncodeToolSet m_encodeur;
	private DecodeToolSet m_decodeur;
	
	public static System.Action<bool> PayPerUseSendRequest;
	private static PayPerUseMdl m_instance;
	
	//-----------------------------------------------------------
	private PayPerUseMdl()
	{
 		m_encodeur = new EncodeToolSet();
		m_decodeur = new DecodeToolSet();
		m_days = new ArrayList();
		m_code = "";
		m_needToSendCode = false;
		
		if(PlayerPrefs.HasKey(c_payperuseKey))
		{
			m_code = PlayerPrefs.GetString(c_payperuseKey);
			CreateDaysFromCode();
		}
		
		if(PlayerPrefs.HasKey(c_ppuKWaitSending))
		{
			m_needToSendCode = (PlayerPrefs.GetInt(c_ppuKWaitSending) == 0)? false : true;
			if(m_needToSendCode)
				FirePayPerUseSendRequest(true);
		}
	}
	
	public static PayPerUseMdl Instance
    {
        get
        {
            if (m_instance==null)
            {
                m_instance = new PayPerUseMdl();
            }
            return m_instance;
        }
    }
	
	#endregion
	
	#region --publics--
	public void AddToday()
	{
		int today = m_encodeur.encodeDate();
		
		/*//DATE
		byte[] date = m_encodeur.convertToBytes(m_encodeur.encodeDate());
		sbyte[] realDate = new sbyte[date.Length];
		
		for(int i=0;i<date.Length;i++)
		{
			realDate.SetValue((sbyte)date[i],i);
		}
		
		Char[] date1 = m_encodeur.encodeByteUpperCase(realDate[1]);
		Char[] date2 = m_encodeur.encodeByteUpperCase(realDate[2]);
		Char[] date3 = m_encodeur.encodeByteUpperCase(realDate[3]);
		
		String dateFinal = ""+date1[0]+date1[1]+date2[0]+date2[1]+date3[0]+date3[1];*/
		
		Debug.Log("TODAY>"+today);
		if(!m_days.Contains(today))
		{
			m_days.Add(today);
		}
		PrepareCodeSending();
	}
	
	public void AddThisDate(int day, int month, int year)
	{
		int thisDate = m_encodeur.encodeDate(/*day, month, year*/);
		Debug.Log("TODAY>"+thisDate);
		if(!m_days.Contains(thisDate))
		{
			m_days.Add(thisDate);
		}
		PrepareCodeSending();
	}
	
	public void ResetDays()
	{
		m_days.Clear();
		m_code = "";
		m_needToSendCode = false;
		
		UpdatePrefs();
	}
	
	public IEnumerator SendCode()
	{
		Debug.Log("PPU -> CODE SENDED");
		WWWForm form = new WWWForm();
		form.AddField("ppu_info",m_code);
		form.AddField("ppu_id",PlayerPrefs.GetInt(usefullData.k_logIn));
		
		WWW sender = new WWW(c_url,form);
		yield return sender;
		
		if(sender.error != null) // pas de connection
		{
			Debug.Log("CONNECTION Error "+sender.error);
		}
		else
		{
			if(sender.text == "1")
			{
				Debug.Log("PPU -> GOOD CODE RECEIVED");
				FirePayPerUseSendRequest(false);
				ResetDays();
			}
			else
			{
				Debug.Log("PPU -> ERRROR > "+sender.text);
			}
		}
	}
	public string GetCode()
	{
		return m_code;
	}
	
	public ArrayList GetDateFromCode(int daysCode)
	{
		return null;//m_decodeur.decodeDate(daysCode);
	}
	#endregion
	
	#region --privates--
	private void PrepareCodeSending()
	{
		CreateCodeFromDays();
		m_needToSendCode = true;
		FirePayPerUseSendRequest(true);
		UpdatePrefs();
	}
	
	private void CreateDaysFromCode()
	{
	//	Debug.Log("code decryption " +m_code);
		byte[] buffer = m_decodeur.decodeBytesUpperCase(m_code);
		m_days.Clear();		
		for (int i = 4; i < buffer.Length; i=i+4)
		{
			byte[] tmpByte = new byte[4];
			tmpByte[0] = buffer[i];
			tmpByte[1] = buffer[i+1];
			tmpByte[2] = buffer[i+2];
			tmpByte[3] = buffer[i+3];			
			m_days.Add(m_decodeur.convertBytesToInt(tmpByte));
		}
	}
	public ArrayList CreateDaysFromCode(string code)
	{
		
	//	Debug.Log("code perso decryption " +code);
		byte[] buffer = m_decodeur.decodeBytesUpperCase(code);
		ArrayList days =  new ArrayList();
		for (int i = 4; i < buffer.Length; i=i+4)
		{
			byte[] tmpByte = new byte[4];
			tmpByte[0] = buffer[i];
			tmpByte[1] = buffer[i+1];
			tmpByte[2] = buffer[i+2];
			tmpByte[3] = buffer[i+3];	
					
			days.Add(m_decodeur.convertBytesToInt(tmpByte));
		//	days.Add(GetDateFromCode(m_decodeur.convertBytesToInt(tmpByte)).ToString());
		}
		return days;
	}
		
	private void CreateCodeFromDays()
	{
		byte[] buffer = new byte[4*m_days.Count+4];
		int idx = 0;
		
		byte[] tmpSizeDay = m_encodeur.convertToBytes(m_days.Count);
		buffer[idx++] = tmpSizeDay[0];
		buffer[idx++] = tmpSizeDay[1];
		buffer[idx++] = tmpSizeDay[2];
		buffer[idx++] = tmpSizeDay[3];	
		
		foreach(int d in m_days)
		{
			byte[] tmpDay = m_encodeur.convertToBytes(d);
			buffer[idx++] = tmpDay[0];
			buffer[idx++] = tmpDay[1];
			buffer[idx++] = tmpDay[2];
			buffer[idx++] = tmpDay[3];		
		}
		m_code = m_encodeur.encodeBytesUpperCase(buffer);
	//	Debug.Log("code encryption " +m_code);
	}
	
	private void UpdatePrefs()
	{
		PlayerPrefs.SetInt(c_ppuKWaitSending,m_needToSendCode?1:0);
		PlayerPrefs.SetString(c_payperuseKey,m_code);
	}
	
	private void FirePayPerUseSendRequest(bool enable)
	{
		if(PayPerUseSendRequest!=null)
			PayPerUseSendRequest(enable);	
	}
	#endregion
}
