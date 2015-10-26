using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GUIRandomHelpSentence {

	private int m_imaxSentence = 0;
	
	private float m_ftimer = 10.0f;
	private float m_fcurrentTimer = 0.0f;
	
	private string m_szrandomSentence = "";
	
	public GUISkin skin;
	
	private string m_url = "http://www.pointcube.com/activationOneshot/00_configs/tips/tips_";
	
	// Use this for initialization
	public IEnumerator init () {
		m_url += PlayerPrefs.GetString("language") + ".txt";
		
		WWW www = new WWW(m_url);
		yield return www;
		
		if(www.error != null)
		{
			Debug.Log(www.error);
			
			do
			{
			}while(PlayerPrefs.HasKey("tips" + ++m_imaxSentence));
			
			if(!PlayerPrefs.HasKey("tips1"))
			{
				m_imaxSentence = 0;
			}
		}
		else
		{
			StringReader reader = new StringReader(www.text);
			
			if(reader != null)
			{
				string szline = "";
				
				do
				{
					szline = reader.ReadLine();
					
					if(szline != null)
					{
						if(PlayerPrefs.HasKey("tips" + ++m_imaxSentence))
						{
							if(PlayerPrefs.GetString("tips" + m_imaxSentence) != szline)
							{
								PlayerPrefs.SetString("tips" + m_imaxSentence, szline);
							}
						}
						else
						{
							PlayerPrefs.SetString("tips" + m_imaxSentence, szline);
						}
					}
					
				}while(szline != null);
			}
			
			changeSentence();
		}
	}
	
	// Update is called once per frame
	public void update () {
		
		m_fcurrentTimer -= Time.deltaTime;
		
		if(m_fcurrentTimer <= 0.0f)
		{
			changeSentence();
		}
	}
	
	public void updateGui ()
	{
		GUISkin bkup = GUI.skin;
		GUI.skin = skin;
		int temp = GUI.skin.label.fontSize;

		Rect rect = new Rect(Screen.width * 0.005125f, Screen.height - 90, Screen.width -175, 50);
		Color bkupColor = GUI.color;
		GUI.color = new Color(bkupColor.r, bkupColor.g, bkupColor.b, 0.3f);
		GUI.Box(new Rect(0.0f, rect.y , Screen.width-210, rect.height), "", "blackPX");
		GUI.color = bkupColor;
		
		if(GUI.Button(new Rect(0.0f, rect.y , Screen.width-210, rect.height ), ""))
		{
			changeSentence();
		}
		
		//GUI.Label(rect, m_szrandomSentence);
		skin.box.fontSize = 1;
		GUI.Box (new Rect(30.0f,Screen.height - 90.0f,50,50),m_szrandomSentence,"hlpIcon");
		//GUI.skin.box.fontSize = temp;

		GUI.skin = bkup;
	}
	
	private void changeSentence()
	{
		if(m_imaxSentence != 0)
		{
			m_fcurrentTimer = m_ftimer;
			string szpreviousSentence = m_szrandomSentence;
			m_szrandomSentence = PlayerPrefs.GetString("tips" + Random.Range(1, m_imaxSentence));
			
			if(szpreviousSentence == m_szrandomSentence)
			{
				changeSentence();
			}
		}
	}
}
