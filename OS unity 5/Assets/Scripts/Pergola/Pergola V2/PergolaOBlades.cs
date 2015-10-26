using UnityEngine;
using System.Collections;
using System.IO;

public class PergolaOBlades : MonoBehaviour,IPergola
{
	private float _angle = 45;
	
	public int _bladeH;
	public int _bladeW;
	
	public Material mat_blades;
	
	public GameObject _BasePrimitive;
	private GameObject blades;
	
	private bool _showBlades = false;
	
	private Function_Pergola _fp;
	
	// Use this for initialization
	void Awake ()
	{		
		
		_fp = GetComponent<Function_Pergola>();
		
		if(!transform.FindChild("blades"))
		{
			blades = new GameObject("blades");
			blades.transform.parent = this.transform;
			blades.transform.localPosition = Vector3.zero;
			
			blades.AddComponent<MeshRenderer>();
			blades.GetComponent<Renderer>().material = mat_blades;
		}
		else
		{
			blades =	transform.FindChild("blades").gameObject;
			blades.GetComponent<Renderer>().material = mat_blades;
		}
		blades.AddComponent<ApplyShader>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	#region Building Fcn's
	void AddPartAt(Vector3 position, Vector3 scale,GameObject p,int i,GameObject refObj,float angleY)
	{
		Vector3 floatScl = scale/1000f;
		Vector3 floatPos = position/1000f;
		
		GameObject part = (GameObject) Instantiate(refObj);
		part.name = i.ToString();
		part.transform.parent = p.transform;
		part.layer = gameObject.layer;
		if(part.transform.GetChildCount() == 0 && part.GetComponent<Renderer>())
			part.GetComponent<Renderer>().material = p.GetComponent<Renderer>().material;
		else
		{
			foreach(Transform t in part.transform)
			{
				if(t.GetComponent<Renderer>())
					t.GetComponent<Renderer>().material = p.GetComponent<Renderer>().material;
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
	#endregion
	
	#region Other Fcn's
	void UpdateBladesOrientation()
	{
		Quaternion q = new Quaternion(0,0,0,0);
		q.eulerAngles = new Vector3(_angle,0,0);
		foreach(Transform t in blades.transform)
		{
			t.localRotation = q;	
		}
	}
	#endregion
	
	#region Interface Fcn's
	
	public void Build(Function_Pergola.PElement m,BuildDatas bd,PergolaRule pr,int index,int off7)
	{
		if(m._pergolaType == Function_Pergola.PergolaType.CL || m._pergolaType == Function_Pergola.PergolaType.single)
		{
			int len = m._l - 2*bd._footSize;
			int h = m._h - bd._frameH/2;
			int centre = off7+m._w/2;
			int startZ = -len/2+_bladeW/2;
			int nb = Mathf.FloorToInt(len/_bladeW);
			for(int i=0;i<nb;i++)
			{
				AddPartAt(new Vector3(centre,h,startZ+i*_bladeW),new Vector3(m._w-bd._subFrameW,_bladeH,_bladeW),blades,index,_BasePrimitive,0);
			}
			if(m._w > pr.limit)
			{
				AddPartAt(new Vector3(off7+m._subW1,h,0),new Vector3(bd._subFrameW,bd._frameH,len),_fp.GetFrame(),index,_BasePrimitive,0);
			}
			
		}
		else// if(m._pergolaType == PergolaType.CW)
		{
			int startZ = 0;
			int len = 0;
			if(_fp == null)
				_fp = GetComponent<Function_Pergola>();
			
			if(_fp.GetModulesCount() == 1)//1 module
			{
				startZ = off7+bd._footSize;
				len = m._l - 2*bd._footSize;
			}
			else//plusieurs modules
			{
				if(index == 0)//1er
				{
					startZ = off7+bd._footSize;
					len = m._l - bd._footSize - bd._subFrameW;
				}
				else if(index == _fp.GetModulesCount()-1)//dernier
				{
					startZ = off7+bd._subFrameW/2;
					len = m._l - bd._footSize - bd._subFrameW;
				}
				else//intermediares
				{
					startZ = off7+bd._subFrameW/2;
					len = m._l - 2*bd._subFrameW;
				}
			}
			
			startZ+=_bladeW/2;
			
			int nb = Mathf.CeilToInt(len/_bladeW);
			int h = m._h - bd._frameH/2;
			
			for(int i=0;i<nb;i++)
			{
				AddPartAt(new Vector3(0,h,startZ+i*_bladeW),new Vector3(m._w-2*bd._footSize,_bladeH,_bladeW),blades,index,_BasePrimitive,0);
			}
		}
		
		UpdateBladesOrientation();
	}
	
	public void GetUI(GUISkin skin)
	{
		_showBlades = GUILayout.Toggle(_showBlades,TextManager.GetText("Pergola.Blades"),GUILayout.Height(50),GUILayout.Width(280));
		if(_showBlades)
		{
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			float tmp = GUILayout.HorizontalSlider(_angle,10,90,GUILayout.Width(240));
			if(_angle != tmp)
			{
				_angle = tmp;
				UpdateBladesOrientation();
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
		Material mt = blades.GetComponent<Renderer>().material;
		if(blades != null)
		{
			foreach(Transform t in blades.transform)
			{
				Destroy(t.gameObject);
			}
			Destroy(blades);
		}
		Destroy(blades);
		blades = new GameObject("blades");
		blades.transform.parent = this.transform;
		blades.transform.localPosition = Vector3.zero;
		
		blades.AddComponent<MeshRenderer>();
		blades.GetComponent<Renderer>().material = mt;//mat_blades;
		blades.AddComponent<ApplyShader>();
	}
		
	
	#endregion
	
}
