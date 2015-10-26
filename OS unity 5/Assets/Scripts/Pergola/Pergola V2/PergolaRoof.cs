using UnityEngine;
using System.Collections;
using System.IO;

public class PergolaRoof : MonoBehaviour,IPergola
{
	
	public GameObject side;
	public GameObject screen;
	public GameObject bar;
	
	private GameObject _roof;
	
	public Material mat_roof;
	
	private float _opening = 1;
	private float _screenSize ;
	
	public int sectionBOx;//Section Barre Oblique (x)
	public int sectionBOy;//Section Barre Oblique (y)
		
	private Function_Pergola _fp;
	
	private bool _showRoof;
	
	// Use this for initialization
	void Awake ()
	{
		_fp = GetComponent<Function_Pergola>();
		
		if(!transform.FindChild("roof"))
		{
			_roof = new GameObject("roof");
			_roof.transform.parent = this.transform;
			_roof.transform.localPosition = Vector3.zero;
			
			_roof.AddComponent<MeshRenderer>();
			_roof.GetComponent<Renderer>().material = mat_roof;
		}
		else
		{
			_roof =	transform.FindChild("roof").gameObject;
			_roof.GetComponent<Renderer>().material = mat_roof;
		}
		_roof.AddComponent<ApplyShader>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	#region Building Fcn's
	void BuildCLRoofAt(Vector3 posStart,float Width,Function_Pergola.PElement m,BuildDatas bd,int index)
	{
		_screenSize = Mathf.Abs((m._l/2 - bd._frameL)/(Mathf.Cos(10* Mathf.Deg2Rad)));	//Longueur du screen
		float triangleSize = m._l - 1200; 												// Taille du triangle
		float pivotAlt = (600)*Mathf.Tan(Mathf.Tan(10* Mathf.Deg2Rad));					//altitude de la barre
		
		Vector3 posEnd = posStart;
		posEnd.x +=Width;
		
		Vector3 triStUpAf = new Vector3(-0.1f,0.2f,1); // décalage anti flick
		Vector3 triEdUpAf = new Vector3(0.1f,0.2f,1);
		
		//Build Des Triangles
		AddPartAt(posStart+triStUpAf,new Vector3(triangleSize,triangleSize,1000),new Vector3(0,-90,0),_roof,index+"_mL",side,_fp.GetMaterial());
		AddPartAt(posEnd+triEdUpAf,new Vector3(triangleSize,triangleSize,1000),new Vector3(0,90,0),_roof,index+"_mR",side,_fp.GetMaterial());
		
		//Build Des Screens
		GameObject screen1 = new GameObject(index+"_screen1");
		
		AddPartAt(new Vector3(-Width/2+sectionBOx/2,0,-(_screenSize * _opening)/2),new Vector3(sectionBOx,sectionBOy,(_screenSize * _opening)),Vector3.zero,screen1,"sl",bar,_fp.GetMaterial());
		AddPartAt(new Vector3(Width/2-sectionBOx/2,0,-(_screenSize * _opening)/2),new Vector3(sectionBOx,sectionBOy,(_screenSize * _opening)),Vector3.zero,screen1,"sr",bar,_fp.GetMaterial());
		AddPartAt(new Vector3(0,0,-(_opening * (_screenSize * _opening))/2),new Vector3(Width-sectionBOx,1000,_opening * (_screenSize * _opening)),Vector3.zero,screen1,"screen",screen,_roof.GetComponent<Renderer>().material);
		
		screen1.transform.parent = _roof.transform;
		screen1.transform.localPosition = (new Vector3(posStart.x+Width/2,m._h-pivotAlt-sectionBOy/2,m._l/2-bd._frameL))/1000f;
		Quaternion q = new Quaternion(0,0,0,0);
		q.eulerAngles = new Vector3(10.0f,0f,0f);
		screen1.transform.localRotation = q;
		
		GameObject screen2 = new GameObject(index+"_screen2");
		
		AddPartAt(new Vector3(-Width/2+sectionBOx/2,0,(_screenSize * _opening)/2),new Vector3(sectionBOx,sectionBOy,(_screenSize * _opening)),Vector3.zero,screen2,"sl",bar,_fp.GetMaterial());
		AddPartAt(new Vector3(Width/2-sectionBOx/2,0,(_screenSize * _opening)/2),new Vector3(sectionBOx,sectionBOy,(_screenSize * _opening)),Vector3.zero,screen2,"sr",bar,_fp.GetMaterial());
		AddPartAt(new Vector3(0,0,(_opening * (_screenSize * _opening))/2),new Vector3(Width-sectionBOx,1000,_opening * _screenSize),Vector3.zero,screen2,"screen",screen,_roof.GetComponent<Renderer>().material);
		
		screen2.transform.parent = _roof.transform;
		screen2.transform.localPosition = (new Vector3(posStart.x+Width/2,m._h-pivotAlt-sectionBOy*1.5f,-m._l/2+bd._frameL))/1000f;
		Quaternion q2 = new Quaternion(0,0,0,0);
		q2.eulerAngles = new Vector3(-10.0f,0f,0f);
		screen2.transform.localRotation = q2;
	}
	
	void BuildCWRoofAt(Vector3 posStart,float Length,Function_Pergola.PElement m,BuildDatas bd,int index)
	{
		_screenSize = Mathf.Abs((Length/2)/(Mathf.Cos(10* Mathf.Deg2Rad)));	//Longueur du screen
		float triangleSize = m._l - 1200; 									// Taille du triangle
		float pivotAlt = (600)*Mathf.Tan(Mathf.Tan(10* Mathf.Deg2Rad));		//altitude de la barre
		
		Vector3 posEnd = posStart;
		posEnd.x +=m._w - 2*bd._frameW;
		
		Vector3 triStUpAf = new Vector3(-0.1f,0.2f,1); // décalage anti flick
		Vector3 triEdUpAf = new Vector3(0.1f,0.2f,1);
		
		//Build Des Triangles
		AddPartAt(posStart+triStUpAf,new Vector3(triangleSize,triangleSize,1000),new Vector3(0,-90,0),_roof,index+"_mL",side,_fp.GetMaterial());
		AddPartAt(posEnd+triEdUpAf,new Vector3(triangleSize,triangleSize,1000),new Vector3(0,90,0),_roof,index+"_mR",side,_fp.GetMaterial());
		
		//Build Des Screens
		GameObject screen1 = new GameObject(index+"_screen1");
		
		AddPartAt(new Vector3(-m._w/2+sectionBOx/2+bd._frameW,0,-(_screenSize * _opening)/2),new Vector3(sectionBOx,sectionBOy,(_screenSize * _opening)),Vector3.zero,screen1,"sl",bar,_fp.GetMaterial());
		AddPartAt(new Vector3(m._w/2-sectionBOx/2-bd._frameW,0,-(_screenSize * _opening)/2),new Vector3(sectionBOx,sectionBOy,(_screenSize * _opening)),Vector3.zero,screen1,"sr",bar,_fp.GetMaterial());
		AddPartAt(new Vector3(0,0,-(_opening * (_screenSize * _opening))/2),new Vector3(m._w-sectionBOx-2*bd._frameW,1000,_opening * (_screenSize * _opening)),Vector3.zero,screen1,"screen",screen,_roof.GetComponent<Renderer>().material);
		
		screen1.transform.parent = _roof.transform;
		screen1.transform.localPosition = (new Vector3(0,m._h-pivotAlt-sectionBOy/2,posStart.z + Length/2))/1000f;
		Quaternion q = new Quaternion(0,0,0,0);
		q.eulerAngles = new Vector3(10.0f,0f,0f);
		screen1.transform.localRotation = q;
		
		GameObject screen2 = new GameObject(index+"_screen2");
		
		AddPartAt(new Vector3(-m._w/2+sectionBOx/2+bd._frameW,0,(_screenSize * _opening)/2),new Vector3(sectionBOx,sectionBOy,(_screenSize * _opening)),Vector3.zero,screen2,"sl",bar,_fp.GetMaterial());
		AddPartAt(new Vector3(m._w/2-sectionBOx/2-bd._frameW,0,(_screenSize * _opening)/2),new Vector3(sectionBOx,sectionBOy,(_screenSize * _opening)),Vector3.zero,screen2,"sr",bar,_fp.GetMaterial());
		AddPartAt(new Vector3(0,0,(_opening * (_screenSize * _opening))/2),new Vector3(m._w-sectionBOx-2*bd._frameW,1000,_opening * _screenSize),Vector3.zero,screen2,"screen",screen,_roof.GetComponent<Renderer>().material);
		
		screen2.transform.parent = _roof.transform;
		screen2.transform.localPosition = (new Vector3(0,m._h-pivotAlt-sectionBOy*1.5f,posStart.z -Length/2))/1000f;
		Quaternion q2 = new Quaternion(0,0,0,0);
		q2.eulerAngles = new Vector3(-10.0f,0f,0f);
		screen2.transform.localRotation = q2;
	}
	
	void AddPartAt(Vector3 position, Vector3 scale,Vector3 rotation,GameObject p,string name,GameObject refObj,Material mat)
	{
		Vector3 floatScl = scale/1000f;
		Vector3 floatPos = position/1000f;
		
		GameObject part = (GameObject) Instantiate(refObj);
		part.name = name;
		part.transform.parent = p.transform;
		part.layer = gameObject.layer;
		if(part.transform.GetChildCount() == 0 && part.GetComponent<Renderer>())
			part.GetComponent<Renderer>().material = mat;
		else
		{
			foreach(Transform t in part.transform)
			{
				if(t.GetComponent<Renderer>())
					t.GetComponent<Renderer>().material = mat;
			}
		}
		part.transform.localPosition = floatPos;
		part.transform.localScale = floatScl;
		
		Quaternion q = part.transform.localRotation;
		q.eulerAngles = rotation;
		part.transform.localRotation = q;
		
		part.AddComponent<ApplyShader>();
	}
	
	void AddPartAt(Vector3 position, Vector3 scale,GameObject p,int i,GameObject refObj,float angleY,Material mat)
	{
		Vector3 floatScl = scale/1000f;
		Vector3 floatPos = position/1000f;
		
		GameObject part = (GameObject) Instantiate(refObj);
		part.name = i.ToString()+"_mdl";
		part.transform.parent = p.transform;
		part.layer = gameObject.layer;
		if(part.transform.GetChildCount() == 0 && part.GetComponent<Renderer>())
			part.GetComponent<Renderer>().material = mat;
		else
		{
			foreach(Transform t in part.transform)
			{
				if(t.GetComponent<Renderer>())
					t.GetComponent<Renderer>().material = mat;
			}
		}
		part.transform.localPosition = floatPos;
		part.transform.localScale = floatScl;
		
		Quaternion q = part.transform.localRotation;
		Vector3 angles = q.eulerAngles;
		angles.y += angleY;
		q.eulerAngles = angles;
		part.transform.localRotation = q;
		
		part.AddComponent<ApplyShader>();
	}
	
	void UpdateScreenOpening()
	{
		if(_fp.GetRule().type == Function_Pergola.PergolaType.CL || _fp.GetRule().type == Function_Pergola.PergolaType.single)
		{
			foreach(Transform node in _roof.transform)
			{
				if(node.name.Contains("screen"))
				{
					Transform scr = node.FindChild("screen");
					float sens = Mathf.Sign(scr.localPosition.z);
					
					Vector3 lclscl = scr.localScale;
					lclscl.z = _opening*(_screenSize/1000);
					scr.localScale = lclscl;
					
					Vector3 lclPos = scr.localPosition;
					lclPos.z = sens * (scr.localScale.z/2);
					scr.localPosition = lclPos;	
				}
			}
		}
		else
		{
			foreach(Transform node in _roof.transform)
			{
				if(node.name.Contains("screen"))
				{
					int index = int.Parse(node.name.Split('_')[0]);
					float size = calcLength(index);
					
					Transform scr = node.FindChild("screen");
					float sens = Mathf.Sign(scr.localPosition.z);
					
					Vector3 lclscl = scr.localScale;
					lclscl.z = _opening*(size/1000);
					scr.localScale = lclscl;
					
					Vector3 lclPos = scr.localPosition;
					lclPos.z = sens * (scr.localScale.z/2);
					scr.localPosition = lclPos;
				}
			}
		}
	}
	
	float calcLength(int index)
	{
		float Length = _fp.GetModule(index)._l;
		float wLength = 0;
		
		
		
		if(_fp.GetModulesCount() == 1) 	// Single
		{
				wLength = Length - 2*_fp.GetBuildDatas()._frameL;
		}
		else 							//Multi
		{
			if(index == 0)// 1er
			{
				wLength = Length - _fp.GetBuildDatas()._frameL - _fp.GetBuildDatas()._subFrameL/2;
			}
			else if(index == _fp.GetModulesCount()-1)//dernier
			{
				wLength = Length - _fp.GetBuildDatas()._frameL - _fp.GetBuildDatas()._subFrameL/2;
			}
			else//les Autres
			{
				wLength = Length - _fp.GetBuildDatas()._subFrameL;
			}
		}
		return Mathf.Abs((wLength/2)/(Mathf.Cos(10* Mathf.Deg2Rad)));
	}
	
	#endregion
	
	#region Interface Fcn's
	
	public void Build(Function_Pergola.PElement m,BuildDatas bd,PergolaRule pr,int index,int off7)
	{
		
				
		if(m._pergolaType == Function_Pergola.PergolaType.CL || m._pergolaType == Function_Pergola.PergolaType.single)
		{
			Vector3 start;
			Vector3 start2;
			float width;
			float width2;
			if(_fp.GetModulesCount() == 1) // Single
			{
				if(m._w <= pr.limit)
				{
					start = new Vector3(off7+bd._frameW,m._h,0);
					width = m._w-2*bd._frameW;
					BuildCLRoofAt(start,width,m,bd,index);
				}
				else
				{
					start = new Vector3(off7+bd._frameW,m._h,0);
					width = m._subW1 - bd._frameW - bd._subFrameW/2;
					BuildCLRoofAt(start,width,m,bd,index);
					//BUILD LA BARRE
					AddPartAt(new Vector3(off7+m._subW1,m._h - bd._frameH/2,0),new Vector3(bd._subFrameW,bd._frameH,m._l-2*bd._frameL),_fp.GetFrame(),index,bar,0,_fp.GetMaterial());
					//-----------
					start2 = new Vector3(off7+m._subW1+bd._subFrameW/2,m._h,0);
					width2 = m._subW2 - bd._frameW - bd._subFrameW/2;
					BuildCLRoofAt(start2,width2,m,bd,index);
					
				}
			}
			else// Multi
			{
				if(index == 0)// 1er
				{
					if(m._w <= pr.limit)
					{
						start = new Vector3(off7+bd._frameW,m._h,0);
						width = m._w-bd._frameW-bd._subFrameW/2;
						BuildCLRoofAt(start,width,m,bd,index);
					}
					else
					{
						start = new Vector3(off7+bd._frameW,m._h,0);
						width = m._subW1 - bd._frameW - bd._subFrameW/2;
						BuildCLRoofAt(start,width,m,bd,index);
						//BUILD LA BARRE
						AddPartAt(new Vector3(off7+m._subW1,m._h - bd._frameH/2,0),new Vector3(bd._subFrameW,bd._frameH,m._l-2*bd._frameL),_fp.GetFrame(),index,bar,0,_fp.GetMaterial());
						//-----------
						start2 = new Vector3(off7+m._subW1+bd._subFrameW/2,m._h,0);
						width2 = m._subW2 - bd._subFrameW;
						BuildCLRoofAt(start2,width2,m,bd,index);
						
					}
				}
				else if(index == _fp.GetModulesCount()-1)//dernier
				{
					if(m._w <= pr.limit)
					{
						start = new Vector3(off7+bd._subFrameW/2,m._h,0);
						width = m._w-bd._frameW-bd._subFrameW/2;
						BuildCLRoofAt(start,width,m,bd,index);
					}
					else
					{
						start = new Vector3(off7+bd._subFrameW/2,m._h,0);
						width = m._subW1 - bd._subFrameW/2 - bd._subFrameW/2;
						BuildCLRoofAt(start,width,m,bd,index);
						//BUILD LA BARRE
						AddPartAt(new Vector3(off7+m._subW1,m._h - bd._frameH/2,0),new Vector3(bd._subFrameW,bd._frameH,m._l-2*bd._frameL),_fp.GetFrame(),index,bar,0,_fp.GetMaterial());
						//-----------
						start2 = new Vector3(off7+m._subW1+bd._subFrameW/2,m._h,0);
						width2 = m._subW2 - bd._frameW - bd._subFrameW/2;
						BuildCLRoofAt(start2,width2,m,bd,index);
						
					}
				}
				else//les Autres
				{
					if(m._w <= pr.limit)
					{
						start = new Vector3(off7+bd._subFrameW/2,m._h,0);
						width = m._w-bd._subFrameW;
						BuildCLRoofAt(start,width,m,bd,index);
					}
					else
					{
						start = new Vector3(off7+bd._subFrameW/2,m._h,0);
						width = m._subW1 - bd._subFrameW;
						BuildCLRoofAt(start,width,m,bd,index);
						//BUILD LA BARRE
						AddPartAt(new Vector3(off7+m._subW1,m._h - bd._frameH/2,0),new Vector3(bd._subFrameW,bd._frameH,m._l-2*bd._frameL),_fp.GetFrame(),index,bar,0,_fp.GetMaterial());
						//-----------
						start2 = new Vector3(off7+m._subW1+bd._subFrameW/2,m._h,0);
						width2 = m._subW2 - bd._subFrameW;
						BuildCLRoofAt(start2,width2,m,bd,index);
						
					}
				}
			}
		}
		else // COUPLAGE W
		{
			Vector3 wStart;
			float 	wLength;
			
			if(_fp.GetModulesCount() == 1) 	// Single
			{
				
					wStart = new Vector3(-m._w/2 + bd._frameW,m._h,off7+m._l/2);
					wLength = m._l - 2*bd._frameL;
					BuildCWRoofAt(wStart,wLength,m,bd,index);
			}
			else 							//Multi
			{
				if(index == 0)// 1er
				{
					wStart = new Vector3(-m._w/2 + bd._frameW,m._h,off7+(m._l+bd._subFrameL)/2);
					wLength = m._l - bd._frameL - bd._subFrameL/2;
					BuildCWRoofAt(wStart,wLength,m,bd,index);
				}
				else if(index == _fp.GetModulesCount()-1)//dernier
				{
					wStart = new Vector3(-m._w/2 + bd._frameW,m._h,off7+(m._l-bd._subFrameL)/2);
					wLength = m._l - bd._frameL - bd._subFrameL/2;
					BuildCWRoofAt(wStart,wLength,m,bd,index);
				}
				else//les Autres
				{
					wStart = new Vector3(-m._w/2 + bd._frameW,m._h,off7+m._l/2);
					wLength = m._l - bd._subFrameL;
					BuildCWRoofAt(wStart,wLength,m,bd,index);
				}
			}
		}
		
		
	}
	
	public void GetUI(GUISkin skin)
	{
		_showRoof = GUILayout.Toggle(_showRoof,TextManager.GetText("Pergola.Roof"),GUILayout.Height(50),GUILayout.Width(280));
		if(_showRoof)
		{
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			float tmp = GUILayout.HorizontalSlider(_opening,0.05f,1,GUILayout.Width(240));
			if(_opening != tmp)
			{
				_opening = tmp;
				UpdateScreenOpening();
			}
			
			GUILayout.Space(10);
			GUILayout.EndHorizontal();
		}
	}
	
	public void PSave(BinaryWriter buffer)
	{
		
	}
	
	public void PLoad(BinaryReader buffer)
	{
		
	}
	
	public void Clear()
	{
		if(_roof != null)
		{
			foreach(Transform t in _roof.transform)
			{
				Destroy(t.gameObject);
			}
		}
	}
	
	#endregion
	
}
