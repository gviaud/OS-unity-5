using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class PergolaScreens : MonoBehaviour,IPergola
{
	
	public PergolaScreen[] screensType;
	
	public GameObject _BasePrimitive;
	
	private bool _showScreens = false;
	
	private GameObject screens;
	
	private Function_Pergola _fp;
	
	private float _opening = 0.5f;
		
	private bool _isSelectingColor = false;
	
	private Texture2D _image;
	
	private Color _oldColor = Color.white;
	
	// Use this for initialization
	void Awake ()
	{
		_fp = GetComponent<Function_Pergola>();
		Material screenMat = new Material(Shader.Find("Diffuse"));
		
		_image = (Texture2D)Resources.Load("colormap");
		
		if(!transform.FindChild("screen"))
		{
			screens = new GameObject("screen");
			screens.transform.parent = this.transform;
			screens.transform.localPosition = Vector3.zero;
			
			screens.AddComponent<MeshRenderer>();
			screens.GetComponent<Renderer>().material = screenMat;
//			screens.renderer.material = mat_screens;
		}
		else
		{
			screens =	transform.FindChild("screen").gameObject;
			screens.GetComponent<Renderer>().material = screenMat;
//			screens.renderer.material = mat_screens;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(screens.GetComponent<Renderer>().material.color != _oldColor)
		{
			_oldColor = screens.GetComponent<Renderer>().material.color;
			if(screens == null)
			return;
			foreach(Transform t in screens.transform)
			{
				if(t.GetComponent<Renderer>() != null)
				{
					if(t.GetComponent<Renderer>().material != null)
					{
						t.GetComponent<Renderer>().material.color = _oldColor;
					}	
				}
			}
		}
	}
	
	#region Other Fcn's
	
	bool UISelectScreen(string label,ref string ms)
	{	
		bool hasChange = false;
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("","btn<",GUILayout.Height(50),GUILayout.Width(50)))
		{
			int i = -1;
			for(int ii = 0;ii<screensType.Length;ii++)
				if(ms == screensType[ii].name)
					i = ii;
				
			if(i > 0)
			{
				i--;
				ms = screensType[i].name;
				hasChange = true;
//				_fp.UpdatePergola();
			}
			else
			{
				i = screensType.Length -1;
				ms = screensType[i].name;
				hasChange = true;
			}
		}
		GUILayout.Label(ms,GUILayout.Height(50),GUILayout.Width(75));
		
		if(GUILayout.Button("","btn>",GUILayout.Height(50),GUILayout.Width(50)))
		{
			int i = -1;
			for(int ii = 0;ii<screensType.Length;ii++)
				if(ms == screensType[ii].name)
					i = ii;
			if(i < screensType.Length-1)
			{
				i++;
				ms = screensType[i].name;
				hasChange = true;
//				_fp.UpdatePergola();
			}
			else
			{
				i = 0;
				ms = screensType[i].name;
				hasChange = true;
			}
		}
		GUILayout.Label(label,"txt",GUILayout.Height(50),GUILayout.Width(50));
		GUILayout.Space(20);
		GUILayout.EndHorizontal();
		return hasChange;
	}
	
	void UpdateScreens()
	{
//		foreach(PergolaScreen ps in screensType)
//		{
//			if(ps.mat != null)
//			{
//				if(ps.mat.HasProperty("_AlphaTex"))
//				{
//					Vector2 off7 = ps.mat.GetTextureScale("_AlphaTex");
//					off7.y = _opening;
//					ps.mat.SetTextureScale("_AlphaTex",off7);
//				}
//			}
//		}
		
		if(screens == null)
			return;
			
		foreach(Transform t in screens.transform)
		{
			if(t.GetComponent<Renderer>() != null)
			{
				if(t.GetComponent<Renderer>().material != null)
				{
					if(t.GetComponent<Renderer>().material.HasProperty("_AlphaTex"))
					{
						Vector2 off7 = t.GetComponent<Renderer>().material.GetTextureScale("_AlphaTex");
						off7.y = _opening;
						t.GetComponent<Renderer>().material.SetTextureScale("_AlphaTex",off7);
						t.GetComponent<Renderer>().material.color = _oldColor;
					}
				}	
			}
		}
		
		float h = (_fp.GetHeight() - _fp.b_datas._frameH)/1000f;
		
		float sizeH = _opening * h;
		float posH = h-(sizeH/2);
//		posH = posH *10;
		
		foreach(Transform t in screens.transform)
		{
			Vector3 pos = t.localPosition;
			Vector3 scl = t.localScale;
			
			pos.y = posH;
			scl.z = sizeH;
			
			t.localPosition= pos;
			t.localScale = scl;
		}
	}
	
	void AddPartAtWthMat(Vector3 position, Vector3 scale,GameObject p,int i,Material mat,float angleY)
	{
		Vector3 floatScl = scale/1000f;
		Vector3 floatPos = position/1000f;
		
		GameObject part = (GameObject) Instantiate(_BasePrimitive);
		part.name = i.ToString();
		part.transform.parent = p.transform;
		part.layer = gameObject.layer;
		part.GetComponent<Renderer>().material = mat;//Materiaux/type de screen
//		part.renderer.material = (Material)Instantiate(mat);//Materiaux Uniques/screen
		part.transform.localPosition = floatPos;
		part.transform.localScale = floatScl;
		
		Quaternion q = part.transform.localRotation;
		Vector3 angles = q.eulerAngles;
		angles.x += 90;
		angles.y += angleY;
		q.eulerAngles = angles;
		part.transform.localRotation = q;
		
		part.AddComponent<ApplyShader>();
		
	}
	
	public PergolaScreen GetScreenType(string name)
	{
		PergolaScreen ps = new PergolaScreen();
		for(int i=0;i<screensType.Length;i++)
		{
			if(screensType[i].name == name)
				ps = screensType[i];
		}
		return ps;
	}
	
	public PergolaScreen[] GetScreensType()
	{
		return screensType;	
	}
	
	#endregion
	
	#region Interface Fcn's
	
	public void Build(Function_Pergola.PElement m,BuildDatas bd,PergolaRule pr,int index,int off7)
	{
		float h = m._h - bd._frameH;
		
		float sizeH = _opening * h;
		float posH = h-(sizeH/2);
		
		int sizeL = m._l- bd._subFrameW;
		int sizeW = m._w - bd._subFrameW;
		
		if(m._pergolaType ==  Function_Pergola.PergolaType.single)
		{
			if(m._W1 != screensType[0].name)
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,-m._l/2+bd._footSize/2),new Vector3(sizeW,1,sizeH),screens,index,GetScreenType(m._W1).mat,180);
			if(m._W2 != screensType[0].name)
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,m._l/2-bd._footSize/2),new Vector3(sizeW,1,sizeH),screens,index,GetScreenType(m._W2).mat,0);
			if(m._L2 != screensType[0].name)
				AddPartAtWthMat(new Vector3(off7+bd._footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,GetScreenType(m._L2).mat,-90);
			if(m._L1 != screensType[0].name)
				AddPartAtWthMat(new Vector3(off7+m._w-bd._footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,GetScreenType(m._L1).mat,90);
		}		
		else if(m._pergolaType == Function_Pergola.PergolaType.CL)
		{
			if(m._W1 != screensType[0].name)
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,-m._l/2+bd._footSize/2),new Vector3(sizeW,1,sizeH),screens,index,GetScreenType(m._W1).mat,180);
			if(m._W2 != screensType[0].name)
				AddPartAtWthMat(new Vector3(off7+m._w/2,posH,m._l/2-bd._footSize/2),new Vector3(sizeW,1,sizeH),screens,index,GetScreenType(m._W2).mat,0);
			
			if(m._L2 != screensType[0].name)
				AddPartAtWthMat(new Vector3(off7+bd._footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,GetScreenType(m._L2).mat,-90);
			if(m._L1 != screensType[0].name)
				AddPartAtWthMat(new Vector3(off7+m._w-bd._footSize/2,posH,0),new Vector3(sizeL,1,sizeH),screens,index,GetScreenType(m._L1).mat,90);
		}
		else if(m._pergolaType == Function_Pergola.PergolaType.CW)
		{			
			if(m._W1 != screensType[0].name)
				AddPartAtWthMat(new Vector3(0,posH,off7+bd._footSize/2),new Vector3(sizeW,1,sizeH),screens,index,GetScreenType(m._W1).mat,180);
			if(m._W2 != screensType[0].name)
				AddPartAtWthMat(new Vector3(0,posH,off7+m._l-bd._footSize/2),new Vector3(sizeW,1,sizeH),screens,index,GetScreenType(m._W2).mat,0);
			if(m._L2 != screensType[0].name)
				AddPartAtWthMat(new Vector3(-m._w/2+bd._footSize/2,posH,off7+m._l/2),new Vector3(sizeL,1,sizeH),screens,index,GetScreenType(m._L2).mat,-90);
			if(m._L1 != screensType[0].name)
				AddPartAtWthMat(new Vector3(m._w/2-bd._footSize/2,posH,off7+m._l/2),new Vector3(sizeL,1,sizeH),screens,index,GetScreenType(m._L1).mat,90);
		}
		
		UpdateScreens();
	}
	
	public void GetUI(GUISkin skin)
	{
		_showScreens = GUILayout.Toggle(_showScreens,TextManager.GetText("Pergola.Screens"),GUILayout.Height(50),GUILayout.Width(280));
		if(_showScreens)
		{
			Function_Pergola.PElement currentModule = _fp.GetCurrentModule();
			bool change;
			
			switch (/*rule.type*/_fp.GetCurrentModule()._pergolaType)
			{
			case Function_Pergola.PergolaType.single:
				change = false;
				change |= UISelectScreen("L1",ref currentModule._L1);
				change |=UISelectScreen("L2",ref currentModule._L2);
				change |=UISelectScreen("W1",ref currentModule._W1);
				change |=UISelectScreen("W2",ref currentModule._W2);
				if(change)
				{
					_fp.SetCurrentModule(currentModule);
					_fp.CheckSizes();
					_fp.UpdatePergola();
				}
				break;
				
			case Function_Pergola.PergolaType.CL:
				change = false;
				change |= UISelectScreen("W1",ref currentModule._W1);
				change |= UISelectScreen("W2",ref currentModule._W2);
				if(_fp.GetModuleIndex() == 0)//1er
				{
					change |= UISelectScreen("L2",ref currentModule._L2);
				}
				if(_fp.GetModuleIndex() == _fp.GetModulesCount()-1)//Dernier
				{
					change |= UISelectScreen("L1",ref currentModule._L1);
				}
				if(change)
				{
					_fp.SetCurrentModule(currentModule);
					_fp.CheckSizes();
					_fp.UpdatePergola();
				}
				break;
				
			case Function_Pergola.PergolaType.CW:
				change = false;
				change |= UISelectScreen("L1",ref currentModule._L1);
				change |= UISelectScreen("L2",ref currentModule._L2);
				if(_fp.GetModuleIndex() == 0)//1er
				{
					change |= UISelectScreen("W1",ref currentModule._W1);
				}
				if(_fp.GetModuleIndex() == _fp.GetModulesCount()-1)//Dernier
				{
					change |= UISelectScreen("W2",ref currentModule._W2);
				}
				if(change)
				{
					_fp.SetCurrentModule(currentModule);
					_fp.CheckSizes();
					_fp.UpdatePergola();
				}
				break;			
			}
			
//			if(GUILayout.Button("ChangeScreenColor > "+_isSelectingColor,"bg",GUILayout.Height(50),GUILayout.Width(280)))
//			{
//				_isSelectingColor = !_isSelectingColor;
//			}
			
//			if(_isSelectingColor)
//			{
//				GUI.Window(0,new Rect(Screen.width-230,0,230,200),ColorPicker,"picker");
//			}
			
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			float tmp = GUILayout.HorizontalSlider(_opening,0,1,GUILayout.Width(240));
			if(_opening != tmp)
			{
				_opening = tmp;
				UpdateScreens();
			}
			
			GUILayout.Space(10);
			GUILayout.EndHorizontal();
			
		}
	}
	
//	public void ColorPicker(int id)
//	{
//		if(_isSelectingColor)
//		{
//			if(GUI.Button(new Rect(0,0,230,200),_image,"empty"))
//			{
//				Vector2 pos = Input.mousePosition;
//				pos.y = Screen.height-pos.y;
//				
//				pos.x = pos.x - (Screen.width-230);
//				pos.x = 230-pos.x;
//				pos.y = 200-pos.y;
//				
//				_oldColor = _image.GetPixel((int)pos.x,(int)pos.y);
//				_isSelectingColor = false;
//				
//				UpdateScreens();
//			}
//		}
//	}
	
	public void PSave(BinaryWriter buffer)
	{
		
	}
	
	public void PLoad(BinaryReader buffer)
	{
		
	}
	
	public void Clear()
	{
		if(screens != null)
		{
			foreach(Transform t in screens.transform)
			{
				Destroy(t.gameObject);
			}
		}
	}
	
	#endregion
}

[Serializable ]
public class PergolaScreen
{
	public string name;
	public int maxSize;
	public Material mat;
	
	public PergolaScreen()
	{
		name = "none";
		maxSize = -1;
		mat = null;
	}
}
