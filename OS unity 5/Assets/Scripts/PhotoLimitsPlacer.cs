using UnityEngine;
using System.Collections;

public class PhotoLimitsPlacer : MonoBehaviour {
	
	RaycastHit rch = new RaycastHit();
	
	GameObject point;
	GameObject points;
	GameObject colPlanes;
	GameObject mainScene;
	
	GameObject target;
	
//	public Material m;
	public string btnLabel;
	
	public bool activ = true;
	public bool ready = true;
	public bool activColor = false;
	
	float hauteur = 20;
	float thickness =0.5f; 
	
	
	Rect button = new Rect(Screen.width-160, 20, 150, 20);
	Rect button_m = new Rect(Screen.width-190, 20, 20, 20);
	Rect button_p = new Rect(Screen.width-215, 20, 20, 20);
	
	public Material wallMat;
	
	ArrayList allPoints = new ArrayList();
	
//	Color visible = new Color(0.5f,0.5f,0.5f,0.5f);
//	Color invisible = new Color(0.5f,0.5f,0.5f,0.0f);
	
	
	// Use this for initialization
	void Start ()
	{
		mainScene = GameObject.Find("MainScene");
		point = GameObject.Find("point");
		points = GameObject.Find("points");
		colPlanes = GameObject.Find("colPlanes");
		target = GameObject.Find("target");
		btnLabel = "switch to walls";
//		m.color = visible;
//		this.gameObject.GetComponent<ObjInteraction>().setActived(true);
		activ = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(activColor || activ)
		{
			wallMat.color = new Color(0,1,0,0.2f);
			target.GetComponent<Renderer>().enabled = true;
			for(int i=0;i<colPlanes.transform.GetChildCount();i++)
				{
					colPlanes.transform.GetChild(i).gameObject.layer = 0;	
				}
		}
		else
		{
			wallMat.color = new Color(1,1,1,0.1f);
			target.GetComponent<Renderer>().enabled = false;
			target.transform.position = new Vector3(0,0,-500);
			for(int i=0;i<colPlanes.transform.GetChildCount();i++)
				{
					colPlanes.transform.GetChild(i).gameObject.layer = 9;	
				}
		}
		
		if(!mainScene.GetComponent<GUIMenuMain>().isOnUi())
		{
			Vector2 cursor = Input.mousePosition;
			cursor.y = Screen.height-cursor.y;
			if(Input.GetKey(KeyCode.A))
			{
				for(int i=0;i<colPlanes.transform.GetChildCount();i++)
				{
					Destroy((Object)colPlanes.transform.GetChild(i).gameObject);	
				}
			}
			
			if(activ && !button.Contains(cursor) && !button_m.Contains(cursor) && !button_p.Contains(cursor))
			{
				
				Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rch, 100000,9);
				Vector3 pos = rch.point;
				pos.y = 5;
				target.transform.position = pos;
				pos.y = 0;
				if(Input.GetMouseButton(0))
				{
					StartCoroutine("addPoint",pos);
				}
	//			if(Input.GetMouseButton(1))
	//			{
	//				active = false;
	//				this.gameObject.GetComponent<ObjInteraction>().actived = true;
	//			}
			}
		}
	}
	
	void OnGUI()
	{
//		if(GUI.Button(button,btnLabel))
//		{
//			if(active)
//			{
//				btnLabel = "switch to walls";
////				m.color = invisible;
//				setVisible(true);
//			}
//			else
//			{
//				btnLabel = "switch to positioning";
////				m.color = visible;
//				this.gameObject.GetComponent<ObjInteraction>().deactivate();
//				setVisible(false);
//			}
//			this.gameObject.GetComponent<ObjInteraction>().actived = active;
//			active = !active;
//		}
//		if(active)
//		{
//			if(GUI.Button(button_m,"-"))
//			{
//				wallSizeChanged(false);
//			}
//			if(GUI.Button(button_p,"+"))
//			{
//				wallSizeChanged(true);
//			}
//		}
	}
	
	IEnumerator addPoint(Vector3 p)
	{
		if(ready)
		{
			ready = false;
			
			allPoints.Add(p);
			
			if(allPoints.Count > 1)
			{
				Vector3 a = (Vector3)allPoints[allPoints.Count-2];
				Vector3 b = a;
				b.y += hauteur;
				Vector3 c = (Vector3)allPoints[allPoints.Count-1];
				Vector3 d = c;
				d.y += hauteur;
				//###############################################
				doTheJob(a,c);
				//###############################################
//				GameObject g = new GameObject();
//				g.layer = 0;//15
//				g.AddComponent("MeshFilter");
//				g.AddComponent("MeshRenderer");
//				g.AddComponent("MeshCollider");
//				g.AddComponent("Rigidbody");
//				g.GetComponent<Rigidbody>().isKinematic = true;
////				g.AddComponent("ObjBehav");
//				
//				Mesh mesh = new Mesh();
//				mesh.Clear();
//				mesh.vertices = new Vector3[] {a,b,d,c};
//				mesh.uv = new Vector2[] {new Vector2(a.x, a.z), new Vector2(b.x, b.z),
//						new Vector2(c.x, c.z),new Vector2(d.x, d.z)};
//				mesh.triangles = new int[] {0, 1, 2 ,2,3,0};
//				mesh.RecalculateNormals();
//				
//				g.GetComponent<MeshFilter>().mesh = mesh;
//				g.GetComponent<MeshCollider>().sharedMesh = mesh;
//				g.transform.parent = colPlanes.transform;
////				g.renderer.material = m;
//				g.renderer.material = wallMat;
			}
				                                   
			yield return new WaitForSeconds(0.5f);
			ready = true;
		}
		yield return null;
	}
	public void setVisible(bool b)//showHide objects
	{
		GameObject mainNode = GameObject.Find("MainNode");
		for(int i=0;i<mainNode.transform.GetChildCount();i++)
		{
			mainNode.transform.GetChild(i).GetComponent<ObjBehav>().showObject(b);				
		}
	}
	
	public void wallSizeChanged(bool positif)
	{
		if(positif)
			hauteur = hauteur + 2;
		else
			hauteur = hauteur - 2;
		
		for(int i=0;i<colPlanes.transform.GetChildCount();i++)
			{			
//				Mesh mesh = new Mesh();
//				mesh.Clear();
//				
//				Vector3[] verts = colPlanes.transform.GetChild(i).GetComponent<MeshFilter>().mesh.vertices;
//				verts[1].y = hauteur;
//				verts[2].y = hauteur;
//			
//				mesh.vertices = verts;
//				mesh.uv = colPlanes.transform.GetChild(i).GetComponent<MeshFilter>().mesh.uv;
//				mesh.triangles = colPlanes.transform.GetChild(i).GetComponent<MeshFilter>().mesh.triangles;
//				mesh.RecalculateNormals();
//				colPlanes.transform.GetChild(i).GetComponent<MeshFilter>().mesh = null;
////				colPlanes.transform.GetChild(i).GetComponent<MeshCollider>().sharedMesh = null;
//				
//				colPlanes.transform.GetChild(i).GetComponent<MeshFilter>().mesh = mesh;
////				colPlanes.transform.GetChild(i).GetComponent<MeshCollider>().sharedMesh = mesh;
			Vector3 tmpScale = colPlanes.transform.GetChild(i).localScale;
			Vector3 tmpPosition = colPlanes.transform.GetChild(i).position;
			
			tmpScale.y = hauteur;
			tmpPosition.y = hauteur/2;
			
			colPlanes.transform.GetChild(i).localScale = tmpScale;
			colPlanes.transform.GetChild(i).position = tmpPosition;
				
			}		
	}
	
	private void doTheJob(Vector3 src, Vector3 dst)
	{
		Vector3 dir;
		Vector3 centre;
		float angle;
		float longueur;
		
		dir = dst-src;
		centre = (src+dst)/2;
		centre.y = hauteur/2;
		longueur = Vector3.Magnitude(dir);
		angle = Vector3.Angle(Vector3.right,dir);
		if(dir.z <0)
			angle = -angle;
		
		
		GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		wall.AddComponent<Rigidbody>();
		wall.GetComponent<Rigidbody>().isKinematic = true;
		wall.GetComponent<Rigidbody>().mass = 100;
		wall.transform.localScale = new Vector3(longueur,hauteur,thickness);
		wall.transform.position = centre;
		wall.transform.rotation = Quaternion.Euler(new Vector3(0,-angle,0));
		wall.transform.parent = colPlanes.transform;
		wall.GetComponent<Renderer>().material = wallMat;
		wall.gameObject.layer = 9;
		
	}
}
