using UnityEngine;
using System.Collections;

public class GUIDialogBox : MonoBehaviour {
		
	private bool _showBox = false;
	private bool _confirm = false;
	private bool _justOkBox = false;
	private bool _3rdChoice=false;
	private bool _canceled = false;
	private bool _isInputBox = false;
	private bool _is3btnBox = false;
	
	private string _confirmTxt="";
	private string _inputString = "";
	
	private string _yesBtn = "oui";
	private string _noBtn = "non";
	private string _otherBtn = "autre";
	
	public GUISkin skin;
	
//	private Rect bg;
//	private Rect innerGroup;
	
	private int _depth = 0;
		
	//-------------------------------
	
	// Use this for initialization
	void Start ()
	{
//		bg = new Rect(Screen.width/2-420/2,Screen.height/2-230/2,420,230);
//		innerGroup = new Rect(35,40,350,150);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void OnGUI()
	{
		GUI.skin = skin;
		GUI.depth = _depth;
		
		if(_showBox)
		{
			if(_is3btnBox || _isInputBox) // grande fenetre
			{
				GUI.Box(new Rect(Screen.width/2-595/2,Screen.height/2-230/2,595,230),"","bg");
				GUI.BeginGroup(new Rect(Screen.width/2-595/2+35,Screen.height/2-230/2+40,525,150));
			}
			else // petite fenetre
			{
				GUI.Box(new Rect(Screen.width/2-420/2,Screen.height/2-230/2,420,230),"","bg");
				GUI.BeginGroup(new Rect(Screen.width/2-420/2+35,Screen.height/2-230/2+40,350,150));
			}
					
			//GRANDE FENETRE
			if(_isInputBox)
			{
				GUI.Label(new Rect(0,0,525,50),_confirmTxt,"txt");

				GUI.SetNextControlName("TextInput");
				_inputString = GUI.TextField(new Rect(0,50,525,50),_inputString,"input");
				if (GUI.GetNameOfFocusedControl() == string.Empty) 
				{
					GUI.FocusControl("TextInput");
				}

				if(GUI.Button(new Rect(0,100,524/2,50),_yesBtn,"btn"))
				_confirm = true;
				if(GUI.Button(new Rect(524/2,100,524/2+1,50),_noBtn,"btn"))
				_canceled = true;
			}
			else if(_is3btnBox)
			{
				GUI.Label(new Rect(0,0,525,100),_confirmTxt,"txt");
				
				if(GUI.Button(new Rect(0,100,525/3,50),_yesBtn,"btn"))
				_confirm = true;
				if(GUI.Button(new Rect(525/3,100,525/3,50),_otherBtn,"btn"))
				_3rdChoice = true;
				if(GUI.Button(new Rect(2*(525/3),100,525/3,50),_noBtn,"btn"))
				_canceled = true;
			}
			
			//PTITE FENETRE
			else if(_justOkBox)
			{
				GUI.Label(new Rect(0,0,350,100),_confirmTxt,"txt");
				if(GUI.Button(new Rect(0,100,350,50),_yesBtn,"btn"))
					_confirm = true;
			}
			else
			{
				GUI.Label(new Rect(0,0,350,100),_confirmTxt,"txt");
				if(GUI.Button(new Rect(0,100,350/2,50),_yesBtn,"btn"))
				_confirm = true;
				if(GUI.Button(new Rect(350/2,100,350/2,50),_noBtn,"btn"))
				_canceled = true;
			}
						
			
			GUI.EndGroup();

		}
	}
	
	//SET'S
	public void setText(string txt)
	{
		_confirmTxt = txt;
	}
	
	public void setBtns(string yes,string no)
	{
		if(yes != "")
			_yesBtn = yes;
		if(no != "")
			_noBtn = no;
	}
	
	public void setBtns(string yes,string no,string other)
	{
		if(yes != "")
			_yesBtn = yes;
		if(no != "")
			_noBtn = no;
		if(other != "")
			_otherBtn = other;
	}
	
	public void showMe(bool isVisible,int depth)
	{
		showMe(isVisible,depth,false);	
	}
	public void showMe(bool isVisible,int depth,bool inputBx) //CACHER LA BOX LA REINITIALISE!
	{
		_showBox = isVisible;
		_isInputBox = inputBx;
		_depth = depth-1;
		
		UsefullEvents.FireLockGuiDialogBox(isVisible);
		
		if(!isVisible)
		{
			resetAll();
		}
	}
	
	public void Show3BtnBox(bool isVisible,int depth)
	{
		_showBox = isVisible;
		_depth = depth-1;
		_is3btnBox = true;
		
		UsefullEvents.FireLockGuiDialogBox(isVisible);
		
		if(!isVisible)
		{
			resetAll();
		}
	}
	
	public void ShowJustOkBox(bool isVisible,int depth)
	{
		_showBox = isVisible;
		_depth = depth-1;
		_justOkBox = true;
		
		UsefullEvents.FireLockGuiDialogBox(isVisible);
		
		if(!isVisible)
		{
			resetAll();
		}	
	}
	
	//GET'S
	public bool getConfirmation()
	{
		return _confirm;
	}
	
	public bool get3rdChoice()
	{
		return _3rdChoice;
	}
	
	public string getInputTxt()
	{
		return _inputString;		
	}
	
	public bool getCancel()
	{
		return _canceled;
	}
	
	public bool isVisible()
	{
		return _showBox;	
	}
	
	public Rect getRect()
	{
		if(_is3btnBox || _isInputBox) // grande fenetre
		{
			return new Rect(Screen.width/2-595/2,Screen.height/2-230/2,595,230);
			
		}
		else // petite fenetre
		{
			return new Rect(Screen.width/2-420/2,Screen.height/2-230/2,420,230);
		}
	}
	
	public void resetBtns()
	{
		_confirm = false;
		_canceled = false;
	}
	
	public void resetAll()
	{
		_showBox = false;
		_confirm = false;
		_justOkBox = false;
		_3rdChoice=false;
		_canceled = false;
		_isInputBox = false;
		_is3btnBox = false;
		
		_confirmTxt="";
		_inputString = "";
		
		_yesBtn = "oui";
		_noBtn = "non";
		_otherBtn = "autre";
	}
	
}
