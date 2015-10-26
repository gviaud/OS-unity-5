using UnityEngine;
using System.Collections;
using System.IO;

public class Lames_PAF : MonoBehaviour,IPergolaAutoFeet
{

	public GameObject refMesh;				//Objet réference a instancier
	public Material refMat;					//Materiau de reference
	
	public float bladeHeight;				//hauteur de la lame
	public float bladeWidth;				//largeur de la lame
	public float bladeSizeLimit;
	
	public float angleOpenMin;				//angle ouverture mini
	public float angleOpenMax;				//angle ouverture maxi
	
	//-------------------------------------------------------------
	
	private GameObject _blades;				//GameObject Parent
	
	private FunctionConf_PergolaAutoFeet _paf;	//script parent
	
	private bool _isLOriented = true;		//sens des lames(sens de la longeur ou de la largeur)
	private bool _uiShow = false;			//Affichage de l UI
	
	private float _angle;					//angle des lames
	
	//----------vv Functions vv-----------------------------
	
	public void Init(Transform parent,FunctionConf_PergolaAutoFeet origin)
	{
		_paf = origin;
		_paf.SetBladeMinLength(bladeSizeLimit);
		if(transform.FindChild("blades"))
			_blades = transform.FindChild("blades").gameObject;
		else
		{
			_blades = new GameObject("blades");
			_blades.transform.parent = parent;
			_blades.transform.localPosition = Vector3.zero;
			if(!_blades.GetComponent<MeshRenderer>())
				_blades.AddComponent<MeshRenderer>();
			_blades.GetComponent<Renderer>().material = refMat;
		}
		
		_angle = angleOpenMin;
	}
	
	void OnEnable()
	{
		PergolaAutoFeetEvents.toggleUIVisibility += ToggleUI;
		PergolaAutoFeetEvents.bladesDirChange += UpdateBladesDir;
	}
	
	void OnDisable()
	{
		PergolaAutoFeetEvents.toggleUIVisibility -= ToggleUI;
		PergolaAutoFeetEvents.bladesDirChange -= UpdateBladesDir;
	}
	
	public void Build(Vector3 origine,int prefix,Vector4 nsew)
	{
		float innerW = _paf.GetLocalW() - 2*_paf.GetFootSize();
		float innerL = _paf.GetLocalL() - 2*_paf.GetFootSize();
		float innerH = _paf.GetH()-_paf.GetFrameSizeH()/2;
		int nb;
		float startOff7;
		
		if(_isLOriented)
		{
//			nb = Mathf.FloorToInt(innerW/bladeWidth);
			nb = Mathf.CeilToInt(innerW/bladeWidth);
			startOff7 = (nb*bladeWidth)/2 - bladeWidth/2;
						
			if(_paf.GetLocalL() < bladeSizeLimit) //Une Lame
			{
				for(int i=0;i<nb;i++)
				{
					GameObject blade = (GameObject) Instantiate(refMesh);
					blade.name = prefix.ToString() +"_"+i.ToString();
					blade.transform.parent = _blades.transform;
					blade.GetComponent<Renderer>().material = _blades.GetComponent<Renderer>().material;
					blade.transform.localPosition = origine + new Vector3(0,innerH,startOff7 - i*bladeWidth);
					blade.transform.localScale = new Vector3(innerL,1,1);
					blade.transform.Rotate(new Vector3(_angle,0,0));
				}
			}	
			else 						//Deux Lames
			{
				float off7 = innerL/4+_paf.GetFootSize()/2;
				float halfsize = innerL/2-_paf.GetFootSize();
				for(int i=0;i<nb;i++)
				{
					GameObject blade = (GameObject) Instantiate(refMesh);
					blade.name = prefix.ToString() +"_"+i.ToString()+"a";
					blade.transform.parent = _blades.transform;
					blade.GetComponent<Renderer>().material = _blades.GetComponent<Renderer>().material;
					blade.transform.localPosition = origine + new Vector3(-off7,innerH,startOff7 - i*bladeWidth);
					blade.transform.localScale = new Vector3(halfsize,1,1);
					blade.transform.Rotate(new Vector3(_angle,0,0));
					
					GameObject blade2 = (GameObject) Instantiate(refMesh);
					blade2.name = prefix.ToString() +"_"+i.ToString()+"b";
					blade2.transform.parent = _blades.transform;
					blade2.GetComponent<Renderer>().material = _blades.GetComponent<Renderer>().material;
					blade2.transform.localPosition = origine + new Vector3(off7,innerH,startOff7 - i*bladeWidth);
					blade2.transform.localScale = new Vector3(halfsize,1,1);
					blade2.transform.Rotate(new Vector3(_angle,0,0));
				}
				
				//subframe
				Vector3 scaleW = new Vector3(innerW,_paf.GetFrameSizeH(),_paf.GetFootSize());
				GameObject f1 = (GameObject)Instantiate(_paf.GetFrameMesh());
				f1.name = prefix + "_addframe1";
				f1.transform.parent = _paf.GetFrame().transform;
				if(f1.GetComponent<Renderer>())
				{
					f1.GetComponent<Renderer>().material = _paf.GetFrameMat();
				}
				else
				{
					f1.transform.GetChild(0).GetComponent<Renderer>().material = _paf.GetFrameMat();
				}
				f1.transform.localPosition = origine + new Vector3(_paf.GetFootSize()/2,innerH,0);
				f1.transform.localScale = scaleW;
				f1.transform.Rotate(new Vector3(0,90,0));
				
				GameObject f2 = (GameObject)Instantiate(_paf.GetFrameMesh());
				f2.name = prefix + "_addframe2";
				f2.transform.parent = _paf.GetFrame().transform;
				if(f2.GetComponent<Renderer>())
				{
					f2.GetComponent<Renderer>().material = _paf.GetFrameMat();
				}
				else
				{
					f2.transform.GetChild(0).GetComponent<Renderer>().material = _paf.GetFrameMat();
				}
				f2.transform.localPosition = origine + new Vector3(-_paf.GetFootSize()/2,innerH,0);;
				f2.transform.localScale = scaleW;
				f2.transform.Rotate(new Vector3(0,270,0));
				
				//subfoot
				AdditionalFoot(origine,prefix,nsew);
				
			}
			
		}
		else
		{
//			nb = Mathf.FloorToInt(innerL/bladeWidth);
			nb = Mathf.CeilToInt(innerL/bladeWidth);
			startOff7 = (nb*bladeWidth)/2 - bladeWidth/2;
			
			if(_paf.GetLocalW() < bladeSizeLimit) //Une Lame
			{				
				for(int i=0;i<nb;i++)
				{
					GameObject blade = (GameObject) Instantiate(refMesh);
					blade.name = prefix.ToString() +"_"+i.ToString();
					blade.transform.parent = _blades.transform;
					blade.GetComponent<Renderer>().material = _blades.GetComponent<Renderer>().material;
					blade.transform.localPosition = origine + new Vector3(-startOff7 + i*bladeWidth,innerH,0);
//					blade.transform.localScale = new Vector3(bladeWidth,bladeHeight,innerW);
					blade.transform.localScale = new Vector3(innerW,1,1);
//					blade.transform.Rotate(new Vector3(0,0,angle));
					blade.transform.Rotate(new Vector3(_angle,90,0));
				}
			}
			else 						//Deux Lames
			{
				float off7 = innerW/4+_paf.GetFootSize()/2;
				float halfsize = innerW/2-_paf.GetFootSize();
				for(int i=0;i<nb;i++)
				{
					GameObject blade = (GameObject) Instantiate(refMesh);
					blade.name = prefix.ToString() +"_"+i.ToString()+"a";
					blade.transform.parent = _blades.transform;
					blade.GetComponent<Renderer>().material = _blades.GetComponent<Renderer>().material;
					blade.transform.localPosition = origine + new Vector3(-startOff7 + i*bladeWidth,innerH,off7);
//					blade.transform.localScale = new Vector3(bladeWidth,bladeHeight,halfsize);
					blade.transform.localScale = new Vector3(halfsize,1,1);
//					blade.transform.Rotate(new Vector3(0,0,angle));
					blade.transform.Rotate(new Vector3(_angle,90,0));
					
					GameObject blade2 = (GameObject) Instantiate(refMesh);
					blade2.name = prefix.ToString() +"_"+i.ToString()+"b";
					blade2.transform.parent = _blades.transform;
					blade2.GetComponent<Renderer>().material = _blades.GetComponent<Renderer>().material;
					blade2.transform.localPosition = origine + new Vector3(-startOff7 + i*bladeWidth,innerH,-off7);
//					blade2.transform.localScale = new Vector3(bladeWidth,bladeHeight,halfsize);
					blade2.transform.localScale = new Vector3(halfsize,1,1);
//					blade2.transform.Rotate(new Vector3(0,0,angle));
					blade2.transform.Rotate(new Vector3(_angle,90,0));
				}
				
				//subframe
				Vector3 scaleL = new Vector3(innerL,_paf.GetFrameSizeH(),_paf.GetFootSize());
				GameObject f3 = (GameObject)Instantiate(_paf.GetFrameMesh());
				f3.name = prefix + "_addframe1";
				f3.transform.parent = _paf.GetFrame().transform;
				if(f3.GetComponent<Renderer>())
				{
					f3.GetComponent<Renderer>().material = _paf.GetFrameMat();
				}
				else
				{
					f3.transform.GetChild(0).GetComponent<Renderer>().material = _paf.GetFrameMat();
				}
				f3.transform.localPosition = origine + new Vector3(0,innerH,_paf.GetFootSize()/2);
				f3.transform.localScale = scaleL;
				f3.transform.Rotate(new Vector3(0,0,0));
				
				GameObject f4 = (GameObject)Instantiate(_paf.GetFrameMesh());
				f4.name = prefix + "_addframe2";
				f4.transform.parent = _paf.GetFrame().transform;
				if(f4.GetComponent<Renderer>())
				{
					f4.GetComponent<Renderer>().material = _paf.GetFrameMat();
				}
				else
				{
					f4.transform.GetChild(0).GetComponent<Renderer>().material = _paf.GetFrameMat();
				}
				f4.transform.localPosition = origine + new Vector3(0,innerH,-_paf.GetFootSize()/2);
				f4.transform.localScale = scaleL;
				f4.transform.Rotate(new Vector3(0,180,0));
				
				//subfoot
				AdditionalFoot(origine,prefix,nsew);
			}
		}
	}
	
	private void AdditionalFoot(Vector3 origine,int prefix,Vector4 nsewTyp)
	{		
		float footSize = _paf.GetFootSize();
		float height = _paf.GetH()-_paf.GetFrameSizeH();
		
		float _localWidth = _paf.GetLocalW();
		float _localLength = _paf.GetLocalL();
		
		Vector3 nsewPos = Vector3.zero;
		Vector3 nsewScl = Vector3.zero;
		
		bool show = _paf.GetAddFeetVisibility();
		
		if(_isLOriented)
		{
			if(nsewTyp.x == 1)
			{
				nsewPos = new Vector3(0,0,-footSize/4);
				nsewScl = new Vector3(0,0,+footSize/2);
			}
			
			//NORTH-------------------------
			Vector3 off7 = new Vector3(footSize/4,height/2,_localWidth/2-footSize/4);
			GameObject f1 = (GameObject)Instantiate(_paf.GetFeetMesh());
			f1.name = prefix + "_addfeet1";
			f1.transform.parent = _paf.GetFrame().transform;
			f1.GetComponent<Renderer>().material = _paf.GetFrameMat();
			f1.GetComponent<Renderer>().enabled = show;
			f1.transform.localPosition = origine + off7 + nsewPos;
			f1.transform.localScale = new Vector3(footSize/2,height,footSize/2) + nsewScl;
			
			off7 = new Vector3(-footSize/4,height/2,_localWidth/2-footSize/4);
			GameObject f2 = (GameObject)Instantiate(_paf.GetFeetMesh());
			f2.name = prefix + "_addfeet2";
			f2.transform.parent = _paf.GetFrame().transform;
			f2.GetComponent<Renderer>().material = _paf.GetFrameMat();
			f2.GetComponent<Renderer>().enabled = show;
			f2.transform.localPosition = origine + off7 + nsewPos;
			f2.transform.localScale = new Vector3(footSize/2,height,footSize/2) + nsewScl;
			
			if(nsewTyp.y == 1)
			{
				nsewPos = new Vector3(0,0,+footSize/4);
				nsewScl = new Vector3(0,0,+footSize/2);
			}
			else
			{
				nsewPos = Vector3.zero;
				nsewScl = Vector3.zero;
			}
			//SOUTH-------------------------
			off7 = new Vector3(footSize/4,height/2,-_localWidth/2+footSize/4);
			GameObject f3 = (GameObject)Instantiate(_paf.GetFeetMesh());
			f3.name = prefix + "_addfeet3";
			f3.transform.parent = _paf.GetFrame().transform;
			f3.GetComponent<Renderer>().material = _paf.GetFrameMat();
			f3.GetComponent<Renderer>().enabled = show;
			f3.transform.localPosition = origine + off7 + nsewPos;
			f3.transform.localScale = new Vector3(footSize/2,height,footSize/2) + nsewScl;
			
			off7 = new Vector3(-footSize/4,height/2,-_localWidth/2+footSize/4);
			GameObject f4 = (GameObject)Instantiate(_paf.GetFeetMesh());
			f4.name = prefix + "_addfeet4";
			f4.transform.parent = _paf.GetFrame().transform;
			f4.GetComponent<Renderer>().material = _paf.GetFrameMat();
			f4.GetComponent<Renderer>().enabled = show;
			f4.transform.localPosition = origine + off7 + nsewPos;
			f4.transform.localScale = new Vector3(footSize/2,height,footSize/2) + nsewScl;
		}
		else
		{
			if(nsewTyp.z == 1)
			{
				nsewPos = new Vector3(-footSize/4,0,0);
				nsewScl = new Vector3(+footSize/2,0,0);
			}
			//EAST-------------------------
			Vector3 off7 = new Vector3(_localLength/2-footSize/4,height/2,footSize/4);
			GameObject f1 = (GameObject)Instantiate(_paf.GetFeetMesh());
			f1.name = prefix + "_addfeet1";
			f1.transform.parent = _paf.GetFrame().transform;
			f1.GetComponent<Renderer>().material = _paf.GetFrameMat();
			f1.GetComponent<Renderer>().enabled = show;
			f1.transform.localPosition = origine + off7 + nsewPos;
			f1.transform.localScale = new Vector3(footSize/2,height,footSize/2) + nsewScl;
			
			off7 = new Vector3(_localLength/2-footSize/4,height/2,-footSize/4);
			GameObject f2 = (GameObject)Instantiate(_paf.GetFeetMesh());
			f2.name = prefix + "_addfeet2";
			f2.transform.parent = _paf.GetFrame().transform;
			f2.GetComponent<Renderer>().material = _paf.GetFrameMat();
			f2.GetComponent<Renderer>().enabled = show;
			f2.transform.localPosition = origine + off7 + nsewPos;
			f2.transform.localScale = new Vector3(footSize/2,height,footSize/2) + nsewScl;
			
			if(nsewTyp.w == 1)
			{
				nsewPos = new Vector3(+footSize/4,0,0);
				nsewScl = new Vector3(+footSize/2,0,0);
			}
			else
			{
				nsewPos = Vector3.zero;
				nsewScl = Vector3.zero;
			}
			//WEST-------------------------
			off7 = new Vector3(-_localLength/2+footSize/4,height/2,footSize/4);
			GameObject f3 = (GameObject)Instantiate(_paf.GetFeetMesh());
			f3.name = prefix + "_addfeet3";
			f3.transform.parent = _paf.GetFrame().transform;
			f3.GetComponent<Renderer>().material = _paf.GetFrameMat();
			f3.GetComponent<Renderer>().enabled = show;
			f3.transform.localPosition = origine + off7 + nsewPos;
			f3.transform.localScale = new Vector3(footSize/2,height,footSize/2) + nsewScl;
			
			off7 = new Vector3(-_localLength/2+footSize/4,height/2,-footSize/4);
			GameObject f4 = (GameObject)Instantiate(_paf.GetFeetMesh());
			f4.name = prefix + "_addfeet4";
			f4.transform.parent = _paf.GetFrame().transform;
			f4.GetComponent<Renderer>().material = _paf.GetFrameMat();
			f4.GetComponent<Renderer>().enabled = show;
			f4.transform.localPosition = origine + off7 + nsewPos;
			f4.transform.localScale = new Vector3(footSize/2,height,footSize/2) + nsewScl;
		}
			
	}
	
	public void GetUI(/*GUISkin skin*/)
	{
		bool tmpui = GUILayout.Toggle(_uiShow,TextManager.GetText("Pergola.Blades"),GUILayout.Height(50),GUILayout.Width(280));
		if(tmpui != _uiShow)
		{
			_uiShow = tmpui;
			if(_uiShow)
				PergolaAutoFeetEvents.FireToggleUIVisibility(GetType().ToString());
			else
				PergolaAutoFeetEvents.FireToggleUIVisibility("close");
		}
		
		if(_uiShow)
		{
			
			string txt = TextManager.GetText("Pergola.BladesChangeDir");
			bool tmp = GUILayout.Toggle(_isLOriented,txt,"toggle2",GUILayout.Height(50),GUILayout.Width(280));
			if(tmp != _isLOriented)
			{
				_isLOriented = tmp;
				PergolaAutoFeetEvents.FireBladesDirChange(_isLOriented);
				PergolaAutoFeetEvents.FireRebuild();
			}
			
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			float tmpAngle = GUILayout.HorizontalSlider(_angle,angleOpenMin,angleOpenMax,GUILayout.Width(200));
			if(tmpAngle != _angle)
			{
				_angle = tmpAngle;
				UpdateBladesOrientation();
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}
	}
	
	public void Clear()
	{
		foreach(Transform t in _blades.transform)
			Destroy(t.gameObject);
	}
	
	private void UpdateBladesOrientation()
	{
		Vector3 newAngle;
		Quaternion finalAngle = new Quaternion(0,0,0,0);
		
		if(_isLOriented)
		{
			newAngle = new Vector3(_angle,0,0);
			
		}
		else
		{
			newAngle = new Vector3(_angle,90,0);
		}
		
		finalAngle.eulerAngles = newAngle;
		
		foreach(Transform l in _blades.transform)
		{
			l.localRotation = finalAngle;
		}
	}
	
	public void ToggleUI(string s)
	{
		if(s != GetType().ToString())
			_uiShow = false;
	}
	
	private void UpdateBladesDir(bool b)
	{
		_isLOriented = b;
	}
	
	public void SaveOption(BinaryWriter buf)
	{
		buf.Write(_angle);
		buf.Write(_isLOriented);
	}
	
	public void LoadOption(BinaryReader buf)
	{
		_angle = buf.ReadSingle();
		_isLOriented = buf.ReadBoolean();
	}
	
	public ArrayList GetConfig()
	{
		ArrayList al = new ArrayList();
		
		al.Add(_angle);
		
		return al;
	}
	
	public void SetConfig(ArrayList al)
	{
		_angle = (float) al[0];
	}
}
