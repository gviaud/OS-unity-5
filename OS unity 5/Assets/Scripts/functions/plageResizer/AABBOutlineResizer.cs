using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AABBOutlineResizer : MonoBehaviour 
{
	static protected float TOLERANCE = 0.1f;
	
	protected Mesh _mesh;
	
	[SerializeField]
	protected Bounds _initialBounds;
	
	[SerializeField]
	protected int [] _forwardIndices;
	
	[SerializeField]
	protected int [] _rightIndices;
	
	[SerializeField]
	protected int [] _leftIndices;
	
	[SerializeField]
	protected int [] _backIndices;
	
	[SerializeField]
	protected int [] _upIndices;
	
	[SerializeField]
	protected int [] _downIndices;
	
	[SerializeField]
	protected float _forwardOffset;
	
	[SerializeField]
	protected float _rightOffset;
	
	[SerializeField]
	protected float _leftOffset;
	
	[SerializeField]
	protected float _backOffset;
	
	[SerializeField]
	protected float _upOffset;
	
	[SerializeField]
	protected float _downOffset;

	[SerializeField]
	protected float _OffsetRotationUV;

	[SerializeField]
	protected float _OffsetTileUV;

	[SerializeField]
	protected float _R_Plage;
	[SerializeField]
	protected float _G_Plage;
	[SerializeField]
	protected float _B_Plage;
	
	[SerializeField]
	protected float _R_Margelle;
	[SerializeField]
	protected float _G_Margelle;
	[SerializeField]
	protected float _B_Margelle;
	
	[SerializeField]
	protected float _R_Muret;
	[SerializeField]
	protected float _G_Muret;
	[SerializeField]
	protected float _B_Muret;

	[SerializeField]
	protected float _Hue_Level;
	[SerializeField]
	protected float _Saturation;

	// vertex group of each side of the sidewalk
	protected VertexGroup _forwardSide;
	protected VertexGroup _rightSide;
	protected VertexGroup _leftSide;
	protected VertexGroup _backSide;
	protected VertexGroup _upSide;
	protected VertexGroup _downSide;
	
	//Offsets minimums //
	private float m_fwdMinOff7;
	private float m_rgtMinOff7;
	private float m_lftMinOff7;
	private float m_bckMinOff7;
	
	// object servant de référance pour la boite englobante 
	public string _objectBoxName = "margelle";
	
	private bool _isInit = false;
	//---------
	
	// Use this for initialization
	
	public void InitBox(bool fromCopy=false)
  //void Awake () 
	{
		MeshFilter meshFilter = GetComponent<MeshFilter> ();
		
		if (meshFilter == null)
		{
			throw new ArgumentNullException ("MeshFilter");
		}
		
		_mesh = meshFilter.mesh;
		_mesh.RecalculateBounds ();
		
		Bounds meshBounds = _mesh.bounds;		
		//Bounds meshBoundsTransform = transform.renderer.bounds;
		
		if (!fromCopy)
			_initialBounds = meshBounds;
		
		_rightSide = new VertexGroup (_mesh, Vector3.right);
		_leftSide = new VertexGroup (_mesh, Vector3.left);
		_forwardSide = new VertexGroup (_mesh, Vector3.forward);
		_backSide = new VertexGroup (_mesh, Vector3.back);
		_upSide = new VertexGroup (_mesh, Vector3.up);
		_downSide = new VertexGroup (_mesh, Vector3.down);
		
		float rightPoint = _initialBounds.center.x  + _initialBounds.extents.x;
		float leftPoint = _initialBounds.center.x - _initialBounds.extents.x;
		float forwardPoint = _initialBounds.center.z + _initialBounds.extents.z;
		float backPoint = _initialBounds.center.z - _initialBounds.extents.z;
		float upPoint = _initialBounds.center.y /*+ meshBoundsTransform.center.y*/ + _initialBounds.extents.y;
		float downPoint = _initialBounds.center.y /*+ meshBoundsTransform.center.y*/ - _initialBounds.extents.y;
		
		//Set des offset min en fonction de la box collider de la piscine 
		//et du décalage de la plage par rappport a celle-ci
//		Debug.Log("OBJECT = "+transform.name);
		
		//Bounds colliderBounds = transform.parent.collider.bounds;
		Transform margelle = transform.parent.Find(_objectBoxName);
		//Bounds colliderBounds = margelle.renderer.bounds;
		MeshFilter meshFilterMargelle = margelle.GetComponent<MeshFilter> ();
		if (meshFilterMargelle == null)
		{
			throw new ArgumentNullException ("MeshFilter");
		}		
		Mesh _meshMargelle = meshFilterMargelle.mesh;
		_meshMargelle.RecalculateBounds ();		
		Bounds colliderBounds = _meshMargelle.bounds;
		
		float rightColliderPoint = colliderBounds.center.x + colliderBounds.extents.x;
		float leftColliderPoint = colliderBounds.center.x - colliderBounds.extents.x;
		float forwardColliderPoint = colliderBounds.center.z + colliderBounds.extents.z;
		float backColliderPoint = colliderBounds.center.z - colliderBounds.extents.z;
		
		float centersDiffX = transform.localPosition.x/*- meshBoundsTransform.center.x*/;
		float centersDiffZ = transform.localPosition.z/*- meshBoundsTransform.center.z*/;
				
	  	m_fwdMinOff7 = -Mathf.Abs(forwardPoint  - forwardColliderPoint + centersDiffZ);
	  	m_rgtMinOff7 =	-Mathf.Abs(rightPoint  - rightColliderPoint + centersDiffX);
	  	m_lftMinOff7 = -Mathf.Abs(leftPoint - leftColliderPoint + centersDiffX);
	  	m_bckMinOff7 = -Mathf.Abs(backPoint - backColliderPoint + centersDiffZ);

		//---------
		
		if (!fromCopy)
		{
			for (int i = 0; i < _mesh.vertexCount; ++i)
			{
				Vector3 vertex = _mesh.vertices[i];
				
				//Vertices selection
				if (vertex.x <= rightPoint + TOLERANCE && vertex.x >= rightPoint - TOLERANCE)
				{
					_rightSide.Add (i);
				}
				
				if (vertex.x <= leftPoint + TOLERANCE && vertex.x >= leftPoint - TOLERANCE)
				{
					_leftSide.Add (i);
				}
				
				if (vertex.z <= forwardPoint + TOLERANCE && vertex.z >= forwardPoint - TOLERANCE)
				{
					_forwardSide.Add (i);
				}
				
				if (vertex.z <= backPoint + TOLERANCE && vertex.z >= backPoint - TOLERANCE)
				{
					_backSide.Add (i);
				}
				
				if (vertex.y <= upPoint + TOLERANCE && vertex.y >= upPoint - TOLERANCE)
				{
					_upSide.Add (i);
				}
				
				if (vertex.y <= downPoint + TOLERANCE && vertex.y >= downPoint - TOLERANCE)
				{
					_downSide.Add (i);
				}
			}
			
			_forwardIndices = _forwardSide.ToArray ();
			_rightIndices = _rightSide.ToArray ();
			_leftIndices = _leftSide.ToArray ();
			_backIndices = _backSide.ToArray ();
			_upIndices = _upSide.ToArray ();
			_downIndices = _downSide.ToArray ();
			
		}
		else
		{
			_forwardSide.AddRange (_forwardIndices);
			_rightSide.AddRange (_rightIndices);
			_leftSide.AddRange (_leftIndices);
			_backSide.AddRange (_backIndices);
			_upSide.AddRange (_upIndices);
			_downSide.AddRange (_downIndices);
			
			_forwardSide.SetValueOnly (_forwardOffset);
			_rightSide.SetValueOnly (_rightOffset);
			_leftSide.SetValueOnly (_leftOffset);
			_backSide.SetValueOnly (_backOffset);
			_upSide.SetValueOnly (_upOffset);
			_downSide.SetValueOnly (_downOffset);
		}
		
		_isInit=true;
	}
	
	void OnDestroy ()
	{
//		if (_mesh == null)
//			return;
//		
//		_forwardSide.Offset = 0.0f;
//		_rightSide.Offset = 0.0f;
//		_leftSide.Offset = 0.0f;
//		_backSide.Offset = 0.0f;
//		_upSide.Offset = 0.0f;
//		_downSide.Offset = 0.0f;
//		
//		_mesh.RecalculateBounds ();
	}
	
	void OnGUI()
	{
//		if(GUI.Button(new Rect(50,50,50,50),"infos") && transform.name == "plage")
//		{
//			Debug.Log("fwd "+_forwardSide);
//			Debug.Log("bck "+_backSide.Offset);
//			Debug.Log("lft "+_leftSide.Offset);
//			Debug.Log("rgt "+_rightSide.Offset);
//		}
	}
	
	// offsets getter and setter
	public float GetForwardOffset ()
	{
		return _forwardSide.Offset;	
	}
	
	public void SetForwardOffset (float offset)
	{
		if( _forwardSide != null)
		{
			if (offset >= m_fwdMinOff7)
			{
				_forwardOffset = offset;
				_forwardSide.Offset = offset;	
			}
			else
			{
				_forwardOffset = m_fwdMinOff7;
				_forwardSide.Offset = m_fwdMinOff7;
			}
		}
	}
	
	public float GetRightOffset ()
	{
		return _rightSide.Offset;	
	}
	
	public void SetRightOffset (float offset)
	{
		if (_rightSide != null) 
		{
			if (offset >= m_rgtMinOff7) {
				_rightOffset = offset;
				_rightSide.Offset = offset;	
			} else {
				_rightOffset = m_rgtMinOff7;
				_rightSide.Offset = m_rgtMinOff7;
			}
		}
	}
	
	public float GetLeftOffset ()
	{
		return _leftSide.Offset;	
	}
	
	public void SetLeftOffset (float offset)
	{
		if (_leftSide != null) 
		{
			if (offset >= m_lftMinOff7)
			{
				_leftOffset = offset;
				_leftSide.Offset = offset;	
			}
			else
			{
				_leftOffset = m_lftMinOff7;
				_leftSide.Offset = m_lftMinOff7;
			}
		}
	}
	
	public float GetBackOffset ()
	{
		return _backSide.Offset;	
	}
	
	public void SetBackOffset (float offset)
	{
		if (_backSide != null) 
		{
			if (offset >= m_bckMinOff7)
			{
				_backOffset = offset;
				_backSide.Offset = offset;	
			}
			else
			{
				_backOffset = m_bckMinOff7;
				_backSide.Offset = m_bckMinOff7;
			}
		}
	}
	
	public float GetUpOffset ()
	{
		return _upSide.Offset;	
	}
	
	public void SetUpOffset (float offset)
	{
		_upOffset = offset;
		_upSide.Offset = offset;	
	}
	
	public float GetDownOffset ()
	{
		return _downSide.Offset;	
	}
	
	public void SetDownOffset (float offset)
	{
		_downOffset = offset;
		_downSide.Offset = offset;	
	}
	
	public float getFwdMinOff7()
	{
		return m_fwdMinOff7;
	}
	public float getBckMinOff7()
	{
		return m_bckMinOff7;
	}
	public float getRgtMinOff7()
	{
		return m_rgtMinOff7;
	}
	public float getLftMinOff7()
	{
		return m_lftMinOff7;
	}
	
	public Bounds InitialBounds
	{
		get { return _initialBounds; }	
	}

	public void setOffsetRotationUV(float _offset)
	{
		_OffsetRotationUV = _offset;//Debug.Log ("AZERTYHYUIOLCFKNJOOLKDNJ");
		//GetComponent<Renderer> ().material.SetColor("_Color", new Vector4(_OffsetRotationUV,0.2f,0.5f,1.0f));
		GetComponent<Renderer> ().material.SetFloat("_UVRotation", _OffsetRotationUV);
	}
	public float getOffsetRotationUV()
	{
		return _OffsetRotationUV ;
	}

	public float getOffsetTileUV()
	{
		return _OffsetTileUV ;
	}

	public void setOffsetTileUV(float _offset)
	{
		_OffsetTileUV = _offset;//Debug.Log ("AZERTYHYUIOLCFKNJOOLKDNJ");
		//GetComponent<Renderer> ().material.SetColor("_Color", new Vector4(_OffsetRotationUV,0.2f,0.5f,1.0f));
		GetComponent<Renderer> ().material.SetFloat("_UVTile", _OffsetTileUV);
	}
	
	public void applyChanges()
	{
		_mesh.RecalculateBounds ();
	}


	public Vector3 getColorPlage()
	{
		return new Vector3(_R_Plage,_G_Plage,_B_Plage);
	}
	public Vector3 getColorMargelle()
	{
		return new Vector3(_R_Margelle,_G_Margelle,_B_Margelle);
	}
	public Vector3 getColorMuret()
	{
		return new Vector3(_R_Muret,_G_Muret,_B_Muret);
	}
	public void setColorPlage(Vector3 _newColor)
	{
		_R_Plage = _newColor.x;
		_G_Plage = _newColor.y;
		_B_Plage = _newColor.z;

		transform.parent.Find("plage").GetComponent<Renderer> ().material.SetColor("_Huecolor", new Color(_R_Plage,_G_Plage,_B_Plage));
	}

	public void setColorMargelle(Vector3 _newColor)
	{
		_R_Margelle = _newColor.x;
		_G_Margelle = _newColor.y;
		_B_Margelle = _newColor.z;
		
		transform.parent.Find("margelle").GetComponent<Renderer> ().material.SetColor("_Huecolor", new Color(_R_Margelle,_G_Margelle,_B_Margelle));
	}
	public void setColorMuret(Vector3 _newColor)
	{
		_R_Muret = _newColor.x;
		_G_Muret = _newColor.y;
		_B_Muret = _newColor.z;
		
		transform.parent.Find("muret").GetComponent<Renderer> ().material.SetColor("_Huecolor", new Color(_R_Muret,_G_Muret,_B_Muret));
	}

	public float getHueLevel()
	{
		return _Hue_Level;
	}
	public void setHueLevel(float new_HueLevel)
	{
		_Hue_Level = new_HueLevel;
		
		GetComponent<Renderer> ().material.SetFloat("_Huelevel", _Hue_Level);
	}
	public float getSaturation()
	{
		return _Saturation;
	}
	public void setSaturation(float new_Saturation)
	{
		_Saturation = new_Saturation;
		
		GetComponent<Renderer> ().material.SetFloat("_Saturation", _Saturation);
	}

	void InitPreset(Transform target)
	{
		Dictionary<string,float> preset;
		if(target.name == "plage" || target.name == "Plage")
		{
			preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigPlage("01p");
		}
		else if(target.name == "margelle" || target.name == "Margelle")
		{
			preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMargelle("01_mar");
		}
		else if(target.name == "muret" || target.name == "Muret")
		{
			preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMuret("01m");
		}
		else
		{
			preset = null;
		}
		if(preset != null)
		{
			string[] keys = preset.Keys.ToArray();
			float red = 0;
			float green = 0;
			float blue = 0;
			foreach (string key in keys)
			{
				switch(key)
				{
				case "Normallevel ":
					target.GetComponent<Renderer> ().material.SetFloat("_Normallevel",preset[key]);
					break ;
				case "UVtranslation ":
					target.GetComponent<Renderer> ().material.SetFloat("_UVtranslation",preset[key]);
					break ;
				case "UVRotation ":
					target.GetComponent<Renderer> ().material.SetFloat("_UVRotation",preset[key]);
					break ;
				case "UVTile ":
					target.GetComponent<Renderer> ().material.SetFloat("_UVTile",preset[key]);
					break ;
				case "Blur ":
					target.GetComponent<Renderer> ().material.SetFloat("_Blur",preset[key]);
					break ;
				case "gloss ":
					target.GetComponent<Renderer> ().material.SetFloat("_gloss",preset[key]);
					break ;
				case "Specularlevel ":
					target.GetComponent<Renderer> ().material.SetFloat("_Specularlevel",preset[key]);
					break ;
				case "Lightness ":
					target.GetComponent<Renderer> ().material.SetFloat("_Ligntness",preset[key]);
					break ;
				case "HueMaskIntensity ":
					target.GetComponent<Renderer> ().material.SetFloat("_HueMaskIntensity",preset[key]);
					break ;
				case "HuecolorR ":
					red = preset[key];
					break ;
				case "HuecolorG ":
					green = preset[key];
					break ;
				case "HuecolorB ":
					blue = preset[key];
					break ;
				case "Huelevel ":
					target.GetComponent<Renderer> ().material.SetFloat("_Huelevel",preset[key]);
					break ;
				case "Saturation ":
					target.GetComponent<Renderer> ().material.SetFloat("_Saturation",preset[key]);
					break ;
				case "Bulb ":
					target.GetComponent<Renderer> ().material.SetFloat("_Bulb",preset[key]);
					break ;
				case "Reflexion ":
					target.GetComponent<Renderer> ().material.SetFloat("_Reflexion",preset[key]);
					if(preset[key] !=0)
					{
						Texture2D temp = (Texture2D)Resources.Load("shader/configs/Chrome 1");
						target.GetComponent<Renderer> ().material.SetTexture("_CubeMap",temp);
					}
					break ;
				case "Reflexionintensity ":
					target.GetComponent<Renderer> ().material.SetFloat("_Reflexionintensity",preset[key]);
					break ;
				case "ReflexionBlur ":
					target.GetComponent<Renderer> ().material.SetFloat("_ReflexionBlur",preset[key]);
					break ;
				}					
			}
			Color hue = new Color(red,green,blue,1);
			target.GetComponent<Renderer> ().material.SetColor("_Huecolor",hue);
			//picker.GetComponent<HSVPicker>().currentColor = hue;
		}
	}
}
