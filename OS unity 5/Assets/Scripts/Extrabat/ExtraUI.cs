using UnityEngine;
using System.Collections.Generic;

public class ExtraUI : MonoBehaviour
{
	#region attributs
	private bool isLoginVisible = false;
	private string tmp_login = "";//"testos3d";
	private string tmp_psswrd = "";//"test789";
	
	private bool _shopsVisible = true;
	private string[] _shops_ids;
	private string[] _shops_names;
	private int _shop_selected = -1;
	private Vector2 _shopListPos= Vector2.zero;
	
	private string[] _clients_names;
	private string [] _clients_ids;
	private Vector2 _clientListPos= Vector2.zero;
	private int _clientSelected = -1;
	
	private string _clientSelected_name= "";
	private string _clientSelected_id= "";
	
	private string _client_search="";
	private bool waiting = false;
	
	public GUISkin extraSkin;
	
	//UI
	private Rect ui_lftPanel;
	private Rect ui_rgtPanel; 
	
	#endregion
	
	#region UNITY FCN's
	// Use this for initialization
	void Awake ()
	{
		ExtraEvent.auth += showLogin;
		ExtraEvent.updateShopList += updateShopsList;
		ExtraEvent.setShop += setShp;
		ExtraEvent.updateClientList += updateClientList;
		ExtraEvent.pleaseWait += showPleaseWait;
		ExtraEvent.uiActivation += activation;
		
		//UI
		ui_lftPanel = new Rect((Screen.width-700)/2,(Screen.height-500)/2,700/2,500);
		ui_rgtPanel = new Rect(ui_lftPanel.xMax,ui_lftPanel.y,700/2,500);
		
		enabled = false;
	}
	
	void OnDestroy()
	{
		ExtraEvent.auth -= showLogin;
		ExtraEvent.updateShopList -= updateShopsList;
		ExtraEvent.setShop -= setShp;
		ExtraEvent.updateClientList -= updateClientList;
		ExtraEvent.pleaseWait -= showPleaseWait;
		ExtraEvent.uiActivation -= activation;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void OnGUI()
	{
		
		GUI.skin = extraSkin;
		GUI.Box(new Rect(0,0,Screen.width,Screen.height),"","bg");
		GUI.Box(new Rect(Screen.width/2-700/2,Screen.height/2-500/2,700,500),"","inbg");
		
		#region ui left
		GUI.BeginGroup(ui_lftPanel);
		
		if(isLoginVisible) // Panneau de login
		{
			GUI.Label(new Rect(0,0,350,50),"Connexion a ExtraBat","header");
			
			GUI.Label(new Rect(25,100,100,30),"Login");
			tmp_login = GUI.TextField(new Rect(25,130,300,40),tmp_login);
			
			GUI.Label(new Rect(25,200,100,30),"Password");
			tmp_psswrd = GUI.TextField(new Rect(25,230,300,40),tmp_psswrd);
			
			if(GUI.Button(new Rect(25,350,300,50),"ACTIVATE","logBtn"))
			{
				if(tmp_login != "" && tmp_psswrd != "")
				{
					ExtraEvent.fireStartAuth(tmp_login,tmp_psswrd);
				}
//				else
//					GUI.Label(new Rect(25,330,300,30),"LOGIN or MDP VIDE","txt");
			}
			
		}
		else
		{
			if(_shopsVisible && _shops_names!=null) //Panneau de sélection du shop
			{
				GUI.Label(new Rect(0,0,350,50),"Selection Shop","header");
				_shopListPos = GUI.BeginScrollView(new Rect(0,50,350,415),_shopListPos,new Rect(25,50,350,_shops_names.Length*50));
				_shop_selected = GUI.SelectionGrid(new Rect(25,50,350,_shops_names.Length*50),_shop_selected,_shops_names,1,"ShopList");
				GUI.EndScrollView();
				
				if(_shop_selected != -1)
				{
					ExtraEvent.fireSetShop(_shops_ids[_shop_selected]);
				}
			}
			else 	//Panneau de selection du client + envoi
			{
				GUI.Label(new Rect(0,0,350,50),"Selection Client","header");
				
				GUI.Label(new Rect(25,50,150,30),"Recherche Client");
				string tmp = GUI.TextField(new Rect(25,80,300,40),_client_search);
				if(tmp  != _client_search)
				{
					_client_search = tmp;
					ExtraEvent.fireStartSearch(tmp);
				}
				
				if(_clients_names != null)
				{
					_clientListPos = GUI.BeginScrollView(new Rect(0,150,400,330),_clientListPos,new Rect(0,0,350,_clients_names.Length*50));
					_clientSelected = GUI.SelectionGrid(new Rect(0,0,350,_clients_names.Length*50),_clientSelected,_clients_names,1,"ShopList");
					if(_clientSelected != -1)
					{
						_clientSelected_name = _clients_names[_clientSelected];
						_clientSelected_id = _clients_ids[_clientSelected];
						_clientSelected = -1;
					}
					GUI.EndScrollView();
				}
				
				GUI.Label(new Rect(0,450,350,50),"","fader");
				
				if(GUI.RepeatButton(new Rect(0,450,175,50),"","dwarrow"))
				{
					_clientListPos.y += 5;
					Mathf.Clamp(_clientListPos.y,0,_clients_names.Length*50);
				}
				if(GUI.RepeatButton(new Rect(175,450,175,50),"","uparrow"))
				{
					_clientListPos.y -= 5;
					Mathf.Clamp(_clientListPos.y,0,_clients_names.Length*50);
				}
			}	
		}
		
		GUI.EndGroup();
		
		if(!isLoginVisible && !_shopsVisible)
		{
			if(GUI.Button(new Rect(ui_lftPanel.x-30,Screen.height/2-150/2,30,150),"","bckArrow"))
			{
				_shop_selected = -1;
				_shopsVisible = true;
				_clientSelected_name = "";
				_clientSelected_id = "-1";
				_clientSelected = -1;
			}
		}
		
		#endregion
		
		#region ui right
		GUI.BeginGroup(ui_rgtPanel);
		GUI.DrawTexture(new Rect(25,25,300,225),GetComponent<ExtraControl>().getPreview());
		
		GUI.Label(new Rect(25,270,100,30),"Magasin","txtBleu");
		if(_shop_selected >-1)
		{
			if(_shops_names[_shop_selected] != "")
				GUI.Label(new Rect(25,300,100,30),_shops_names[_shop_selected],"txtBleu");
			else
				GUI.Label(new Rect(25,300,100,30),"< Selectionnez un magasin","txtBleu");
		}
		else
				GUI.Label(new Rect(25,300,100,30),"< Selectionnez un magasin","txtBleu");
		
		if(!_shopsVisible)
		{
			GUI.Label(new Rect(25,330,100,30),"Client","txtBleu");
			if(_clientSelected_name != "" && _clientSelected_id != "-1")
				GUI.Label(new Rect(25,360,100,30),_clientSelected_name,"txtBleu");
			else
				GUI.Label(new Rect(25,360,100,30),"< Selectionnez un client","txtBleu");
		}
		
		if(isLoginVisible)
		{
			GUI.Label(new Rect(0,400,175,50),"Envoyer image","deactBtns");//4 btn HG
			
			GUI.Label(new Rect(175,400,175,50),"Changer magasin","deactBtns");//4 btn HD
			
		}
		else
		{
			if(GUI.Button(new Rect(0,400,175,50),"Envoyer image","rgtBtns"))//4 btn HG
			{
				if(_clientSelected_id != "-1")
					ExtraEvent.fireSendImage(_clientSelected_id);
			}
			if(GUI.Button(new Rect(175,400,175,50),"Changer magasin","rgtBtns"))//4 btn HD
			{
				_shop_selected = -1;
				_shopsVisible = true;
				_clientSelected_name = "";
				_clientSelected_id = "-1";
				_clientSelected = -1;
			}
		}
		if(GUI.Button(new Rect(0,450,175,50),"Quitter ExtraBat","rgtBtns"))//4 btn BG
		{
			GetComponent<ExtraControl>().QuitPlugin();
		}
		if(GUI.Button(new Rect(175,450,175,50),"Se deconnecter","rgtBtns"))//4 btn BD
		{
			PlayerPrefs.DeleteKey(ExtraData.pk_privateKey);
			PlayerPrefs.DeleteKey(ExtraData.pk_selectedShop);
			ExtraEvent.fireNeedAuth();
		}
		GUI.EndGroup();
		#endregion
		
//		if(isLoginVisible) // Panneau de login
//		{
//			
//			GUI.Label(new Rect(10,10,100,30),"LOGIN");
//			tmp_login = GUI.TextField(new Rect(120,10,150,30),tmp_login);
//			
//			GUI.Label(new Rect(10,50,100,30),"MDP");
//			tmp_psswrd = GUI.TextField(new Rect(120,50,150,30),tmp_psswrd);
//			
//			if(GUI.Button(new Rect(10,90,150,30),"ACTIVATE"))
//			{
//				if(tmp_login != "" && tmp_psswrd != "")
//				{
//					ExtraEvent.fireStartAuth(tmp_login,tmp_psswrd);
//				}
//				else
//					Debug.LogError("LOGIN or MDP INVALID!");
//			}
//		}
		
//		else
//		{
//			
		
//			if(_shopsVisible) //Panneau de sélection du shop
//			{
//				shop_selected = GUI.SelectionGrid(new Rect(10,10,250,400),_shop_selected,_shops_names,1);
//				if(_shop_selected != -1)
//				{
//					ExtraEvent.fireSetShop(_shops_ids[_shop_selected]);
//				}
//			}
		
//			else 	//Panneau de selection du client + envoi
//			{
		
////			// changement shop
//				if(_shops_names!= null)
//				{
//					if(GUI.Button(new Rect(10,10,250,30),_shops_names[_shop_selected]))
//					{
//						_shop_selected = -1;
//						_shopsVisible = true;
//					}
//				}
		
//				//champ recherche client
//				GUI.Label(new Rect(10,50,100,30),"Search :");
//				string tmp = GUI.TextField(new Rect(120,50,130,30),_client_search);
//				if(tmp  != _client_search)
//				{
//					_client_search = tmp;
//					ExtraEvent.fireStartSearch(tmp);
//				}
//				
//				//envoie image
//				if(_clients_names != null)
//				{
//					for(int i=0;i<_clients_names.Length;i++)
//					{
//						if(GUI.Button(new Rect(10,90+i*40,250,30),_clients_names[i]) && _clients_ids[i] != "-1")
//						{
//							ExtraEvent.fireSendImage(_clients_ids[i]);
//						}
//					}					
//				}
//			}
//		}
		
		if(waiting)
		{
			GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-150,300,300),"Please Wait");	
		}
	}
	#endregion
	
	#region aux fcns
	private void showLogin(bool b)
	{
		isLoginVisible = b;
	}
	
	private void updateShopsList(Dictionary<string,string> dict)
	{
		_shops_ids = new string[dict.Count];
		_shops_names = new string[dict.Count];
		int index = 0;
		foreach(string s in dict.Keys)
		{
			_shops_ids[index] = s;
			_shops_names[index] = s + " - "+dict[s];
			index ++;
		}
		_shopsVisible = true;
	}
	private void setShp(string s)
	{
//		_shop_selected = i;
		for(int j=0;j<_shops_ids.Length;j++)
		{
			if(_shops_ids[j] == s)
				_shop_selected = j;
		}
		_shopsVisible = false;
	}
	
	private void updateClientList(Dictionary<string,string> dict)
	{
		_clients_ids = new string[dict.Count];
		_clients_names = new string[dict.Count];
		int index = 0;
		
		foreach(string s in dict.Keys)
		{
			_clients_ids[index] = s;
			_clients_names[index] = dict[s];
			index ++;
		}
	}
	
	private void showPleaseWait(bool b)
	{
		waiting = b;
	}
	
	private void activation(bool b)
	{
		Debug.Log("ACTIVATION UI RECEIVED");
		enabled = b;
	}
	#endregion
}
