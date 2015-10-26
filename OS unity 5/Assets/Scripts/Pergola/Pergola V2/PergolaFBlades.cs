using UnityEngine;
using System.Collections;
using System.IO;

public class PergolaFBlades : MonoBehaviour,IPergola
{		
	public Material mat_blades;
	
	public GameObject basePrimitive;
	public GameObject frame;
	private GameObject _blades;
	private GameObject _prevBlade;
	
	private float _anim = 0;
	
	private bool _showBlades = false;
	
	private Function_Pergola _fp;
	
	public int bladeSize;
	
	public bool useDoubleFrame;
	public int uDFWidthLimit;
	public int uDFLengthLimit;
	
	
	// Use this for initialization
	void Awake ()
	{
		_fp = GetComponent<Function_Pergola>();
		
		if(!transform.FindChild("blades"))
		{
			_blades = new GameObject("blades");
			_blades.transform.parent = this.transform;
			_blades.transform.localPosition = Vector3.zero;
			
			_blades.AddComponent<MeshRenderer>();
			_blades.GetComponent<Renderer>().material = mat_blades;
		}
		else
		{
			_blades =	transform.FindChild("blades").gameObject;
			_blades.GetComponent<Renderer>().material = mat_blades;
		}
		_blades.AddComponent<ApplyShader>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	#region Build Fcn's
	
	private GameObject AddPartAt(Vector3 position, Vector3 scale,GameObject p,int i,GameObject refObj,float angleY)
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
				{
					t.GetComponent<Renderer>().material = p.GetComponent<Renderer>().material;
				}
			}
		}
		part.transform.localPosition = floatPos;
		part.transform.localScale = floatScl;
		
		Quaternion q = Quaternion.identity;//part.transform.localRotation;
		Vector3 angles = q.eulerAngles;
		angles.y += angleY;
		q.eulerAngles = angles;
		part.transform.localRotation = q;
		
		part.AddComponent<ApplyShader>();
		
		return part;
	}
	
	#endregion
	
	#region Other Fcn's
	void UpdateBladesAnimation()
	{
		foreach(Transform t in _blades.transform)
		{
			if(t.GetComponent<AnimateBlade>())
			{
				t.GetComponent<AnimateBlade>().AnimTo(_anim);
			}
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
//			int nb = Mathf.FloorToInt(len/bladeSize);
//			
//			if((len-(nb*bladeSize))>(bladeSize/2))
//				nb++;
			int nb = Mathf.FloorToInt(len/bladeSize);
			if((len-(nb*bladeSize))>(bladeSize/4))
				nb++;
			float alpha = 1;
			if((nb*bladeSize)>len)
			{
				alpha = 1-(((float)(nb*bladeSize)-len)/((float)len));
			}
			else
			{
				alpha = 1-(((float)(nb*bladeSize)-len)/((float)len));
			}
			
			int startZ = -m._l/2+bd._footSize;//(-(nb*bladeSize)/2)+10;
	
			for(int i=0;i<nb;i++)
			{
				GameObject tmp = AddPartAt(new Vector3(centre,h,startZ+i*bladeSize),new Vector3(1000*alpha,1000,m._w-(2*bd._subFrameW)),_blades,index*100+i,basePrimitive,90);
				if(_prevBlade == null)
					_prevBlade = tmp;
				else
				{
					tmp.GetComponent<AnimateBlade>().alignTo = _prevBlade.transform.FindChild("1").transform;
					_prevBlade = tmp;
				}
			}
			_prevBlade = null;
			
			if(useDoubleFrame)
			{
				if(m._w > uDFWidthLimit && m._l <= uDFLengthLimit) // 1barre
				{
					AddPartAt(new Vector3(off7+m._subW1,h,0),new Vector3(bd._subFrameW,bd._frameH,len),_fp.GetFrame(),index,frame,0);
				}
				else if(m._w > uDFWidthLimit && m._l > uDFLengthLimit) // 2 barres
				{
					AddPartAt(new Vector3(off7+m._subW1-bd._subFrameW/2,h,0),new Vector3(bd._subFrameW,bd._frameH,len),_fp.GetFrame(),index,frame,0);
					AddPartAt(new Vector3(off7+m._subW1+bd._subFrameW/2,h,0),new Vector3(bd._subFrameW,bd._frameH,len),_fp.GetFrame(),index,frame,0);
				}
			}
			else
			{
				if(m._w > pr.limit)
				{
					
					AddPartAt(new Vector3(off7+m._subW1,h,0),new Vector3(bd._subFrameW,bd._frameH,len),_fp.GetFrame(),index,frame,0);
				}
			}
		}
		else
		{
			int len = m._l - 2*bd._footSize;
			int h = m._h - bd._frameH/2;
			int centre = off7+m._l/2;
//			int nb = Mathf.FloorToInt(len/bladeSize);
//			
//			if((len-(nb*bladeSize))>(bladeSize/2))
//				nb++;
			int nb = Mathf.FloorToInt(len/bladeSize);
			if((len-(nb*bladeSize))>(bladeSize/4))
				nb++;
			float alpha = 1;
			if((nb*bladeSize)>len)
			{
				alpha = 1-(((float)(nb*bladeSize)-len)/((float)len));
			}
			else
			{
				alpha = 1-(((float)(nb*bladeSize)-len)/((float)len));
			}
			
			int startZ = -m._l/2+bd._footSize;//(-(nb*bladeSize)/2)+10;
			
			for(int i=0;i<nb;i++)
			{
				GameObject tmp = AddPartAt(new Vector3(0,h,centre+startZ+i*bladeSize),new Vector3(1000*alpha,1000,m._w-(2*bd._subFrameW)),_blades,index*100+i,basePrimitive,90);
				if(_prevBlade == null)
					_prevBlade = tmp;
				else
				{
					tmp.GetComponent<AnimateBlade>().alignTo = _prevBlade.transform.FindChild("1").transform;
					_prevBlade = tmp;
				}
			}
			_prevBlade = null;
		}
		UpdateBladesAnimation();	
	}
	
	public void GetUI(GUISkin skin)
	{
		_showBlades = GUILayout.Toggle(_showBlades,TextManager.GetText("Pergola.Blades"),GUILayout.Height(50),GUILayout.Width(280));
		if(_showBlades)
		{
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			float tmp = GUILayout.HorizontalSlider(_anim,0,1.1f,GUILayout.Width(240));
			if(_anim != tmp)
			{
				_anim = tmp;
				UpdateBladesAnimation();
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
		Material mt = _blades.GetComponent<Renderer>().material;
		if(_blades != null)
		{
			foreach(Transform t in _blades.transform)
			{
				Destroy(t.gameObject);
			}
			Destroy(_blades);
		}
		_blades = new GameObject("blades");
		_blades.transform.parent = this.transform;
		_blades.transform.localPosition = Vector3.zero;
		
		_blades.AddComponent<MeshRenderer>();
		_blades.GetComponent<Renderer>().material = mt;//mat_blades;
		_blades.AddComponent<ApplyShader>();
	}
	
	#endregion
	
}
