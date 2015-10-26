using UnityEngine;
using System.Collections;

public class GUINotificationLicenceExpiration 
{
	public GUISkin skin;
	
	private float m_falpha = 1.0f;
	private int m_isens = -1;  

	public Texture BgTexture;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void updateGui ()
	{
		GUISkin bkup = GUI.skin;
		GUI.skin = skin;
		
		Rect rect = new Rect(Screen.width -200,50, 150,50);
		Color bkupColor = GUI.color;
		GUI.color = new Color(bkupColor.r, bkupColor.g, bkupColor.b, 0.5f);
		//GUI.Box(new Rect(rect.x + (rect.width * 0.2f), Screen.height * 0.0575f, rect.width * 0.6f, rect.height * 0.35f), "", "blackPX");
		GUI.color = bkupColor;
		
		bkupColor = GUI.color;
		int inumberDays = PlayerPrefs.GetInt("LicenceDateRestDays");
		
		string sztemp = TextManager.GetText("LicenceNotification.Sentence") + " " + inumberDays.ToString() + " " + TextManager.GetText("LicenceNotification.Day");
		string szstyle = "notifLicence";
		
		if(inumberDays > 0)
		{
			if(inumberDays <= 30) //Deux semaines
			{
				szstyle += "Alert";
				
				m_falpha += Time.deltaTime * m_isens * 0.5f;
				
				if(m_isens == 1 && m_falpha >= 1.0f)
				{
					m_falpha = 1.0f;
					m_isens = -1;
				}
				else if(m_isens == -1 && m_falpha <= 0.1f)
				{
					m_falpha = 0.1f;
					m_isens = 1;
				}
				
				if(inumberDays <= 5) //3 Jours et moins
				{
					GUI.color = new Color(Mathf.Cos(Time.time), 0.0f, 0.0f, Mathf.Cos(Time.time));
				}
				else if(inumberDays <= 30)
				{
					GUI.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					GUI.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
				}
			}
		}
		else
		{
			sztemp = TextManager.GetText("LicenceNotification.Infinite");
		}

	//	temp = (Texture)Resources.Load ("gui/Test.png");
		//Debug.Log (temp.name + "  " + temp.width + "  " + temp.height + "  ");
		//GUI.DrawTexture(new Rect(Screen.width - BgTexture.width , Screen.height * 0.025f,BgTexture.width, BgTexture.height), BgTexture);
		GUI.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		GUI.Label(rect, sztemp, szstyle);
		
		GUI.color = bkupColor;

		GUIStyle style;

		if(inumberDays <= 5) //3 Jours et moins
			style= "renewOrange" ;
		else if(inumberDays <= 30)
			style= "renewVert" ;
		else 
			style = "renew";

		if(inumberDays <= 30)
			GUI.color = new Color(1.0f, 1.0f, 1.0f,0.6666f+Mathf.Cos(5f*Time.time)/3.0f);

		if(inumberDays >= 0 && inumberDays <= 30 && GUI.Button (new Rect(Screen.width - 50, 50, 50, 50),
		                                                        ""/*TextManager.GetText("LicenceNotification.Renew")*/, style))
		{
			GUI.color = bkupColor;
			bkupColor = GUI.color;
			GUI.color = new Color(bkupColor.r, bkupColor.g, bkupColor.b, 0.5f);
			GUI.Box(new Rect((Screen.width * 0.5f) - 25.0f, Screen.height * 0.075f, 50.0f, 50.0f), "", "blackPX");
			GUI.color = bkupColor;
			
#if UNITY_IPHONE
			EtceteraManager.setPrompt(true);
			EtceteraBinding.showMailComposer("info@pointcube.fr",
			                                 TextManager.GetText("LicenceNotification.Licence") + " : " + PlayerPrefs.GetString(usefullData.k_logIn),
			                                 TextManager.GetText("LicenceNotification.Mailing"), false);
			
			
#else
			Application.OpenURL("http://www.pointcube.fr/contact");
#endif	
		}
		if( inumberDays > 30)
		{
			GUI.Box(new Rect(Screen.width - BgTexture.width-100, Screen.height * 0.025f, Screen.width * 0.5f, Screen.height * 0.1f), "", "PictoLicOk");
		}
		GUI.color = bkupColor;
		GUI.skin = bkup;
	}
}
