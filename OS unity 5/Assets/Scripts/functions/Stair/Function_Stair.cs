using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Function_Stair : MonoBehaviour, Function_OS3D
{
	protected string _functionName = "PositionEscalier";
	
	protected FunctionUI_OS3D _ui;
	
	protected StairController _controller;
	
	public string _uiName;
	
	public int id;
	
	void Awake ()
	{
		_controller = transform.gameObject.GetComponent<StairController> ();
		if (_controller == null)
		{
			_controller = transform.gameObject.AddComponent<StairController> ();
		}

		if(_uiName != null)
		{
			setUI((FunctionUI_OS3D)GameObject.Find("MainScene").GetComponent(_uiName) );	
		}
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	public void DoAction () 
	{
		if(_ui != null)
		{
			_controller.Init ();
			_ui.DoActionUI(gameObject);
		}
	}
	
	public void setUI(FunctionUI_OS3D ui)
	{
		_ui = ui;
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}
	
	public string GetFunctionName()
	{
		return GetFunctionParameterName();
	}
	
	public string GetFunctionParameterName()
	{
		return " "+TextManager.GetText(_functionName);
	}
	
	public int GetFunctionId()
	{
		return id;
	}
	
	public ArrayList getConfig()
	{
		int stairIndex = -1;
		int gizmoIndex = -1;
		
		if(_controller != null)
		{
			stairIndex = _controller.GetStairIndex ();
			gizmoIndex = _controller.GetGizmoIndex ();
		}
		
		ArrayList list = new ArrayList();
		list.Add(stairIndex);
		list.Add(gizmoIndex);
		
		Transform mainNode = GameObject.Find ("MainNode").transform;
		
		foreach (StairGizmo g in _controller.GetGizmoList ())
		{
			list.Add (g);
		}
		
		return list;
	}
		
	public void setConfig(ArrayList config)
	{
		int stairIndex = (int)config[0];
		int gizmoIndex = (int)config[1];
		
		if (_controller != null)
		{
			_controller.SetStairIndex (stairIndex);
			_controller.SetGizmoIndex (gizmoIndex);
			
			Transform gizmo1 = _controller.transform.Find ("Gizmo1");
			
			if (/*_controller.GetGizmoList ().Count <= 0*/gizmo1 == null) 
			{
				List<StairGizmo> gizmos = new List<StairGizmo> ();
				
				for (int gizIndex = 2; gizIndex < config.Count; ++gizIndex)
				{
					StairGizmo g = (StairGizmo)config[gizIndex];
					gizmos.Add (g);
					
					GameObject gizmoT = GameObject.CreatePrimitive (PrimitiveType.Cube);
					Destroy (gizmoT.GetComponent<Collider>());
					Destroy (gizmoT.GetComponent<Rigidbody>());
					
					gizmoT.transform.parent = _controller.transform;
					gizmoT.transform.localPosition = g.position;
					gizmoT.transform.localRotation = g.rotation;
					
					gizmoT.name = "Gizmo" + (gizIndex - 1).ToString ();
					gizmoT.GetComponent<Renderer>().enabled = false;
				}
				_controller.SetGizmoList (gizmos);
			}
			else
			{
				_controller.SetStairIndex (-1);
				_controller.SetGizmoIndex (-1);
			}
		}
	}
	
	public void save(BinaryWriter buf)
	{	
		int stairIndex = -1;
		int gizmoIndex = -1;
		
		if(_controller != null)
		{
			stairIndex = _controller.GetStairIndex ();
			gizmoIndex = _controller.GetGizmoIndex ();
		}
		
		buf.Write (stairIndex);
		buf.Write (gizmoIndex);
		buf.Write (_controller.GetGizmoList ().Count);
		
		Transform gizmo1 = _controller.transform.Find ("Gizmo1");
		
		if (/*_controller.GetGizmoList ().Count <= 0*/gizmo1 == null) 
		{
			foreach (StairGizmo g in _controller.GetGizmoList ())
			{
				buf.Write ((double)g.position.x);	
				buf.Write ((double)g.position.y);	
				buf.Write ((double)g.position.z);	
				
				buf.Write ((double)g.rotation.x);	
				buf.Write ((double)g.rotation.y);	
				buf.Write ((double)g.rotation.z);
				buf.Write ((double)g.rotation.w);	
			}
		}
	}
	
	public void load(BinaryReader buf)
	{
		int stairIndex = buf.ReadInt32 ();
		int gizmoIndex = buf.ReadInt32 ();
		int gizmoCount = buf.ReadInt32 ();
		
		if (_controller != null)
		{
			_controller.SetStairIndex (stairIndex);
			_controller.SetGizmoIndex (gizmoIndex);
			
			Transform gizmo1 = _controller.transform.Find ("Gizmo1");
			
			if (/*_controller.GetGizmoList ().Count <= 0*/gizmo1 == null) 
			{
				List<StairGizmo> gizmos = new List<StairGizmo> ();
			
				for (int gizIndex = 0; gizIndex < gizmoCount; ++gizIndex)
				{				
					Vector3 position = new Vector3 ((float)buf.ReadDouble (), 
						                            (float)buf.ReadDouble (), 
						                            (float)buf.ReadDouble ());
					
					Quaternion rotation = new Quaternion ((float)buf.ReadDouble (), 
						                                  (float)buf.ReadDouble (), 
						                                  (float)buf.ReadDouble (), 
						                                  (float)buf.ReadDouble ());
					
					StairGizmo g = new StairGizmo ();
					g.index = gizIndex;
					g.position = position;
					g.rotation = rotation;
					
					gizmos.Add (g);
					
					GameObject gizmoT = GameObject.CreatePrimitive (PrimitiveType.Cube);
					Destroy (gizmoT.GetComponent<Collider>());
					Destroy (gizmoT.GetComponent<Rigidbody>());
					
					gizmoT.transform.parent = _controller.transform;
					gizmoT.transform.localPosition = g.position;
					gizmoT.transform.localRotation = g.rotation;
					
					gizmoT.name = "Gizmo" + (gizIndex + 1).ToString ();
					gizmoT.GetComponent<Renderer>().enabled = false;
				}
				_controller.SetGizmoList (gizmos);
			}
			
			_controller.Init ();
			if (stairIndex >= 0 && gizmoIndex >= 0)
			{
				StartCoroutine (_controller.SwapPool (gizmoIndex, stairIndex));	
			}
		}
	}
}