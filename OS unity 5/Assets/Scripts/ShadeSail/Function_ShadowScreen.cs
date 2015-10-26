using UnityEngine;
using System.Collections;
using System.IO;

public class Function_ShadowScreen : MonoBehaviour,Function_OS3D
{

#region public vars
	public GameObject 	footRef;
	public GameObject 	attachRef;
	public GameObject	targetRef;
	
	public Material		matRef;
	
	public float 		footSize;
	public float		attachDiam;
	public float 		minHeight;
	public float		maxHeight;
	
	public int functionId;
#endregion

#region private vars
	
	private Vector3[] 	_points;// positions des points du triangle de la toile
	private Vector3[] 	_normals;// normales des points du triangle de la toile
	private Vector3[] 	_anglesDirs;// Direction vers l'interieur des points du triangle de la toile
	
	private float[]   	_offsets;// off7 de chaque pied
	
	private bool[]		_shows;// option affichage de chaquePied
	
	private Vector2 _oldMousePos;
	private Vector2 _uiScroll;					//scroll de l'UI
	
	private float _screenArea;	//Surface totale de la toile en m²
	private float _deltaSpeed = 0.1f;  //facteur vitesse en fonction du déplacement du touch ou de la mouse
	private float _deltaScale;
	
	private int _index = 0;		//index su pied sélectionné
	
	private bool _isQuad;
	private bool _isConfiguringFoot = false;
	
	private GUISkin _uiSkin;					//Skin de l'UI
	
	private Rect _uiArea;						//Zone ou est l'UI

	private GameObject _tgt;
	
#endregion
	
#region Unity Fcns
	
	// Use this for initialization
	void Start ()
	{
		// Function 0S3D
		_uiArea = new Rect(Screen.width-300, 0, 300, Screen.height);
		if(_uiSkin == null)
			_uiSkin = (GUISkin)Resources.Load("skins/PergolaSkin");
		
		// build toilke par def
		_points = new Vector3[4];
		_points[0] = new Vector3(-1,1,0);
		_points[1] = new Vector3(0,1,1);
		_points[2] = new Vector3(0,1,-1);
		_points[3] = new Vector3(1,1,0);
		
		_offsets = new float[4];
		_offsets[0] = 0f;
		_offsets[1] = 0f;
		_offsets[2] = 0f;
		_offsets[3] = 0f;
		
		_shows = new bool[4];
		_shows[0] = true;
		_shows[1] = true;
		_shows[2] = true;
		_shows[3] = true;
		
		Rebuild();
		
		//Création de la target
		_tgt = (GameObject) Instantiate(targetRef);
		foreach(Transform t in _tgt.transform)
			t.GetComponent<Renderer>().enabled = false;
		
//		enabled = false;

	}
	
	// Update is called once per frame
	void Update ()
	{
		
		
		Vector3 tmppoint = _points[_index];
		
		//MOUSE INTERACTION
		if(Input.GetMouseButtonDown(0))
			_oldMousePos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
		
		//Position
		if(Input.GetMouseButton(0))
		{			
			Vector2 pos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
			if(pos != _oldMousePos && !_uiArea.Contains(pos))
			{
				MoveFeet(new Vector2(pos.x - _oldMousePos.x,pos.y - _oldMousePos.y));
				_oldMousePos = pos;
			}	
		}
		
		//Offset
		if(Input.GetAxis("Mouse ScrollWheel") != 0)
			ChangeFeetOffset(Input.GetAxis("Mouse ScrollWheel"));
		
		
		
		
		//Tactile INTERACTION
		if(Input.touchCount > 0)
		{
			if(Input.touchCount == 1) // déplacement du pied en X/Z
			{
				Touch t0 = Input.touches[0];
				if(!_uiArea.Contains(t0.position))
				{
					if(t0.phase == TouchPhase.Began)
						_oldMousePos = t0.position;
					
					else if(t0.phase == TouchPhase.Moved)
					{
						MoveFeet(new Vector2(t0.position.x - _oldMousePos.x,t0.position.y - _oldMousePos.y));
						_oldMousePos = t0.position;	
					}
				}
			}
			else if(Input.touchCount == 2)//scale du offset
			{
				Touch t0 = Input.touches[0];
				Touch t1 = Input.touches[1];
				if(!_uiArea.Contains(t0.position) && !_uiArea.Contains(t1.position))
				{
					if(t0.phase == TouchPhase.Began)
						_deltaScale = Vector2.Distance(t0.position,t1.position);
					else if(t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
					{
						float tmpDelta = Vector2.Distance(t0.position,t1.position);
						ChangeFeetOffset(tmpDelta - _deltaScale);
						_deltaScale = tmpDelta;
					}
				}
			}
		}
		
//		_tgt.transform.position = new Vector3(_points[_index].x,0,_points[_index].z);	
	}
	
	void OnGUI()
	{		
		GUI.skin = _uiSkin;
		
		//-------vv----FADE Haut-------------vv------------------
		GUILayout.BeginArea(_uiArea);
		GUILayout.FlexibleSpace();
		_uiScroll = GUILayout.BeginScrollView(_uiScroll,"empty","empty",GUILayout.Width(300));//scrollView en cas de menu trop grand
		GUILayout.Box("","UP",GUILayout.Width(280),GUILayout.Height(150));//fade en haut
		GUILayout.BeginVertical("MID");
		
		//-------vv----UI Configurateur-------vv---------------
		
		//Sélection du pied a modifier
		UISelector();
		
		//affichage/masquage pied
		bool tmpShow = GUILayout.Toggle(_shows[_index],"Show foot",GUILayout.Height(50),GUILayout.Width(280));
		if(tmpShow != _shows[_index])
		{
			_shows[_index] = tmpShow;
			Rebuild();
		}
		
		//Double Toile
		bool tmp = GUILayout.Toggle(_isQuad,"Dual Mode",GUILayout.Height(50),GUILayout.Width(280));
		if(tmp != _isQuad)
		{
			_isQuad = tmp;
			if(!_isQuad)
				_index = 0;
			Rebuild();
		}
		GUILayout.Label("size >"+_screenArea+"m²");
		UISetHeight();
		
		//-------vv----FADE BAS-------------vv------------------
		GUILayout.EndVertical();
		GUILayout.Box("","DWN",GUILayout.Width(280),GUILayout.Height(150));//fade en bas
		
		GUILayout.EndScrollView();
		GUILayout.FlexibleSpace();
		
		GUILayout.EndArea();
		
		//-----------------------------------------
//		GUILayout.BeginArea(new Rect(10,10,300,500));
//		GUILayout.BeginVertical();
//		
//		UISelector();
//		UISetPos();
//		UISetHeight();
//		
//		GUILayout.Label("size >"+_screenArea+"m²");
//		
//		bool tmp = GUILayout.Toggle(_isQuad,"Dual Mode");
//		if(tmp != _isQuad)
//		{
//			_isQuad = tmp;
//			if(!_isQuad)
//				_index = 0;
//			ClearIt();
//			BuildIt();
//		}
//		
//		bool tmpShow = GUILayout.Toggle(_shows[_index],"Show foot");
//		if(tmpShow != _shows[_index])
//		{
//			_shows[_index] = tmpShow;
//			ClearIt();
//			BuildIt();
//		}
//		
//		bool tmpUI = GUILayout.Toggle(_isConfiguringFoot,"configure");
//		if(tmpUI != _isConfiguringFoot)
//		{
//			_isConfiguringFoot = tmpUI;
//			foreach(Transform t in _tgt.transform)
//				t.renderer.enabled = tmpUI;
//				
//		}
//		
//		GUILayout.EndVertical();
//		GUILayout.EndArea();
		
	}
#endregion
	
#region Fcns
	
	//Choisit le pied a deplacer
	private void UISelector()
	{
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		if(GUILayout.Button("<",GUILayout.Height(50),GUILayout.Width(50)))
		{
			_index --;
			if(_isQuad)
				_index = Mathf.Clamp(_index,0,_points.Length-1);
			else
				_index = Mathf.Clamp(_index,0,_points.Length-2);
		}
		GUILayout.Space(10);
		GUILayout.Label((_index+1)+" / "+(_isQuad? _points.Length :_points.Length-1) ,GUILayout.Height(50));
		GUILayout.Space(10);
		if(GUILayout.Button(">",GUILayout.Height(50),GUILayout.Width(50)))
		{
			
			_index ++;
			if(_isQuad)
				_index = Mathf.Clamp(_index,0,_points.Length-1);
			else
				_index = Mathf.Clamp(_index,0,_points.Length-2);
		}
		GUILayout.Space(20);
		GUILayout.EndHorizontal();	
	}
	
	//règle la hauteur du pied choisi
	private void UISetHeight()
	{
		float tmp = _points[_index].y;
		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
		GUILayout.FlexibleSpace();
		
		GUILayout.BeginVertical();
		GUILayout.Space(20);
		float tmpH = GUILayout.VerticalSlider(_points[_index].y,maxHeight,minHeight,GUILayout.Height(150));
		if(tmpH != _points[_index].y)
			ChangeFeetHeight(tmpH);
		GUILayout.Space(20);
		GUILayout.EndVertical();
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		if(_points[_index].y != tmp)
		{
			Rebuild();
		}
	}
	
	//Calcule la surface du screen
	private void CalculateArea()
	{
		if(_isQuad)
		{
			Vector3 cross = Vector3.Cross((_points[2]-_points[0]),(_points[1]-_points[0]));
			
			Vector3 cross2 = Vector3.Cross((_points[2]-_points[3]),(_points[1]-_points[3]));
			_screenArea = cross.magnitude / 2 + cross2.magnitude / 2 ;
		}
		else
		{
			Vector3 cross = Vector3.Cross((_points[2]-_points[0]),(_points[1]-_points[0]));
			_screenArea = cross.magnitude / 2;
		}
	}
	
	//Rebuild
	private void Rebuild()
	{
		ClearIt();
		BuildIt();
	}
	
	//Build le voile n'ombrage
	private void BuildIt()
	{
		CalculateDatas();
		RecalcOffset();
		BuildFeet();
		
		if(_isQuad)
		{
			BuildScreen(false);
			BuildScreen(true);
		}
		else
			BuildScreen(false);
		
		CalculateArea();
	}
	
	//Test si le point d'attach a une altitude > 0
	private bool PreBuild()
	{
		bool ok = true;
		
		CalculateDatas();
		
		GameObject dummyP = new GameObject();
		dummyP.name = "Dummy"+_index;
		dummyP.transform.parent = transform;
		dummyP.transform.localPosition = _points[_index];
		dummyP.transform.localRotation = Quaternion.FromToRotation(dummyP.transform.right,_anglesDirs[_index]);
		
		GameObject attachDummy = new GameObject();
		attachDummy.name = "AttachDummy"+_index;
		attachDummy.transform.parent = dummyP.transform;
		attachDummy.transform.localPosition = new Vector3(-_offsets[_index],0,0);
		if(attachDummy.transform.position.y <0)
			ok = false;
		
		return ok;
	}
	
	//Build les pieds
	private void BuildFeet()
	{
		for(int i=0;i<(_isQuad? 4:3);i++)
		{
			GameObject dummyP = new GameObject();
			dummyP.name = "Dummy"+i;
			dummyP.transform.parent = transform;
			dummyP.transform.localPosition = _points[i];
//			dummyP.transform.localRotation = Quaternion.FromToRotation(dummyP.transform.up,_normals[i]);
			dummyP.transform.localRotation = Quaternion.FromToRotation(dummyP.transform.right,_anglesDirs[i]);
			
			GameObject attachDummy = new GameObject();
			attachDummy.name = "AttachDummy"+i;
			attachDummy.transform.parent = dummyP.transform;
			attachDummy.transform.localPosition = new Vector3(-_offsets[i],0,0);
			
			GameObject attach = (GameObject) Instantiate(attachRef);
			attach.name = "attach"+i;
			attach.transform.parent = dummyP.transform;
			attach.transform.localPosition = new Vector3(-_offsets[i]/2,0,0);
			attach.transform.localScale = new Vector3(_offsets[i],attachDiam,attachDiam);
			attach.transform.localRotation = new Quaternion(0,0,0,0);
			
			GameObject foot = (GameObject) Instantiate(footRef);
			foot.name = "foot"+i;
			foot.transform.parent = attachDummy.transform;
			foot.transform.localPosition = new Vector3(0,-attachDummy.transform.position.y/2,0);
			foot.transform.localScale = new Vector3(footSize,attachDummy.transform.position.y,footSize);
			if(foot.GetComponent<Renderer>())
				foot.GetComponent<Renderer>().enabled = _shows[i];
			else
				foot.transform.GetChild(0).GetComponent<Renderer>().enabled = _shows[i];
		}
	}
	
	//Build la/les toiles
	private void BuildScreen(bool scnd)
	{
		Mesh m = new Mesh();
		
		m.name = "screen"+ (scnd? "2":"1");
			
		GameObject screen = new GameObject("screen"+ (scnd? "2":"1"));
		screen.transform.parent = transform;
		screen.transform.localPosition = Vector3.zero;
		screen.transform.localScale = Vector3.one;
		screen.AddComponent<MeshFilter>();
		screen.AddComponent<MeshRenderer>();
		
		Vector3[] points = new Vector3[3];
		Vector3[] norms = new Vector3[3];
		
		//Points and Normals
		if(scnd)
		{
			for(int i=0;i<3;i++)
			{
				points[i] = _points[i+1];
				norms[i] = _normals[i+1];
			}
		}
		else
		{
			for(int i=0;i<3;i++)
			{
				points[i] = _points[i];
				norms[i] = _normals[i];
			}
		}
		
		//Triangles
		int[] triangles = new int[3];
		if(scnd)
		{
			triangles[0] = 0;
			triangles[1] = 1;
			triangles[2] = 2;
		}
		else
		{
			triangles[0] = 1;
			triangles[1] = 0;
			triangles[2] = 2;
		}
		
		//UV's
		Vector2[] uvs = new Vector2[3];
//		uvs[0] = new Vector2 (0,0);
//		uvs[1] = new Vector2 (1,0);
//		uvs[2] = new Vector2 (0,1);
		uvs[0] = new Vector2 (points[0].x,points[0].z);
		uvs[1] = new Vector2 (points[1].x,points[1].z);
		uvs[2] = new Vector2 (points[2].x,points[2].z);

		
		
		float dir = 1;
		if(scnd)
			dir = -1;

		
		m.vertices = points;
		m.triangles = triangles;
		m.uv = uvs;
		m.normals = norms;
		m.Optimize();
		
		m.RecalculateBounds();
//		m.RecalculateNormals();
		CalculateMeshTangents(m);
		
        screen.GetComponent<MeshFilter>().mesh = m;
		screen.GetComponent<Renderer>().material = matRef;
	}
	
	//Recalcule les tangentes
	public void CalculateMeshTangents(Mesh mesh)
	{
    //speed up math by copying the mesh arrays
    int[] triangles = mesh.triangles;
    Vector3[] vertices = mesh.vertices;
    Vector2[] uv = mesh.uv;
    Vector3[] normals = mesh.normals;
 
    //variable definitions
    int triangleCount = triangles.Length;
    int vertexCount = vertices.Length;
 
    Vector3[] tan1 = new Vector3[vertexCount];
    Vector3[] tan2 = new Vector3[vertexCount];
 
    Vector4[] tangents = new Vector4[vertexCount];
 
    for (long a = 0; a < triangleCount; a += 3)
    {
	        int i1 = triangles[a + 0];
	        int i2 = triangles[a + 1];
	        int i3 = triangles[a + 2];
	 
	        Vector3 v1 = vertices[i1];
	        Vector3 v2 = vertices[i2];
	        Vector3 v3 = vertices[i3];
	 
	        Vector2 w1 = uv[i1];
	        Vector2 w2 = uv[i2];
	        Vector2 w3 = uv[i3];
	 
	        float x1 = v2.x - v1.x;
	        float x2 = v3.x - v1.x;
	        float y1 = v2.y - v1.y;
	        float y2 = v3.y - v1.y;
	        float z1 = v2.z - v1.z;
	        float z2 = v3.z - v1.z;
	 
	        float s1 = w2.x - w1.x;
	        float s2 = w3.x - w1.x;
	        float t1 = w2.y - w1.y;
	        float t2 = w3.y - w1.y;
	 
	        float r = 1.0f / (s1 * t2 - s2 * t1);
	 
	        Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
	        Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
	 
	        tan1[i1] += sdir;
	        tan1[i2] += sdir;
	        tan1[i3] += sdir;
	 
	        tan2[i1] += tdir;
	        tan2[i2] += tdir;
	        tan2[i3] += tdir;
	    }
	 
	 
	    for (long a = 0; a < vertexCount; ++a)
	    {
	        Vector3 n = normals[a];
	        Vector3 t = tan1[a];
	 
	        //Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
	        //tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
	        Vector3.OrthoNormalize(ref n, ref t);
	        tangents[a].x = t.x;
	        tangents[a].y = t.y;
	        tangents[a].z = t.z;
	 
	        tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
	    }
	 
	    mesh.tangents = tangents;
	}
	
	//Recalcule les normales et les directions vers l'interieur
	private void CalculateDatas()
	{
		_normals = new Vector3[4];
		_normals[0] = Vector3.Cross(_points[1]-_points[0],_points[2]-_points[0]);
		_normals[1] = Vector3.Cross(_points[2]-_points[1],_points[0]-_points[1]);
		_normals[2] = Vector3.Cross(_points[0]-_points[2],_points[1]-_points[2]);
		_normals[3] = Vector3.Cross(_points[2]-_points[3],_points[1]-_points[3]);
		
		_anglesDirs = new Vector3[4];
//		_anglesDirs[0] = ((_points[1]-_points[0]) + (_points[2]-_points[0]))/2;
//		_anglesDirs[1] = ((_points[0]-_points[1]) + (_points[3]-_points[1]))/2;
//		_anglesDirs[2] = ((_points[0]-_points[2]) + (_points[3]-_points[2]))/2;
//		_anglesDirs[3] = ((_points[1]-_points[3]) + (_points[2]-_points[3]))/2;
		if(_isQuad)
		{
			_anglesDirs[0] = ((_points[1]-_points[0]) + (_points[2]-_points[0]))/2;
			_anglesDirs[1] = (_points[2]-_points[1])/2;
			_anglesDirs[2] = (_points[1]-_points[2])/2;
			_anglesDirs[3] = ((_points[1]-_points[3]) + (_points[2]-_points[3]))/2;	
		}
		else
		{
			_anglesDirs[0] = ((_points[1]-_points[0]) + (_points[2]-_points[0]))/2;
			_anglesDirs[1] = ((_points[0]-_points[1]) + (_points[3]-_points[1]))/2;
			_anglesDirs[2] = ((_points[0]-_points[2]) + (_points[3]-_points[2]))/2;
			_anglesDirs[3] = ((_points[1]-_points[3]) + (_points[2]-_points[3]))/2;
		}
		
	}
	
	//déplace le point du screen sélectionné
	private void MoveFeet(Vector2 delta)
	{
		_points[_index].x +=delta.x * _deltaSpeed;
		_points[_index].z += delta.y * _deltaSpeed;
		Rebuild();
	}
	
	//change la hauteur du point du screen sélectionné
	private void ChangeFeetHeight(float delta)
	{
		float old = _points[_index].y;
		_points[_index].y =delta;
		if(!PreBuild())
			_points[_index].y = old;
		Rebuild();
	}
	
	//change l'offset du point du screen sélectionné
	private void ChangeFeetOffset(float delta)
	{
		float bkupOff7 = _offsets[_index];
		
		_offsets[_index] += delta * _deltaSpeed;
		
		if(!PreBuild())
			_offsets[_index] = bkupOff7;
		Rebuild();
	}
	
	//Recalcule l'offset suite a un déplacement
	private void RecalcOffset()
	{
		GameObject dummyP = new GameObject();
		GameObject attachDummy = new GameObject();
		
		for(int i=0;i<4;i++)
		{
			dummyP.transform.parent = transform;
			dummyP.transform.localPosition = _points[i];
			dummyP.transform.localRotation = Quaternion.FromToRotation(dummyP.transform.right,_anglesDirs[i]);
			
			attachDummy.transform.parent = dummyP.transform;
			attachDummy.transform.localPosition = new Vector3(-_offsets[i],0,0);
			
			if(attachDummy.transform.position.y <=0)	
			{
				float alt = dummyP.transform.position.y + Mathf.Abs(attachDummy.transform.position.y);
				float newAlt = dummyP.transform.position.y;
				
				float newOffset = (newAlt/alt)*_offsets[i];
				_offsets[i] = newOffset;
			}
		}
		Destroy(dummyP);
		Destroy(attachDummy);
	}
	
	//efface les GameObjects avant de recréer
	private void ClearIt()
	{
		foreach(Transform t in transform)
		{
			Destroy(t.gameObject);	
		}
	}
	
#endregion
	
#region Function_OS3D
	public string GetFunctionName(){return "Toile d'ombrage";}
	public string GetFunctionParameterName(){return "Toile d'ombrage";}
	
	public int GetFunctionId(){return functionId;}
	
	public void DoAction()
	{
		GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);
			
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;
			
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,true);
		Camera.main.GetComponent<ObjInteraction>().setActived(false);	
				
		enabled = true;
	}
	
	public void Validate()
	{
		//cache la tgt
		_isConfiguringFoot = false;
		foreach(Transform t in _tgt.transform)
			t.GetComponent<Renderer>().enabled = _isConfiguringFoot;
			
		Camera.main.GetComponent<ObjInteraction>().configuringObj(null);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().unConfigure();
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().setVisibility (false);
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,false);		
		enabled = false;	
	}
	
	//  sauvegarde/chargement
	
	public void save(BinaryWriter buf)
	{
		
	}
	
	public void load(BinaryReader buf)
	{
		
	}
	
	//Set L'ui si besoin
	
	public void setUI(FunctionUI_OS3D ui){	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}
	
	//public void setVisible();
	
	//similaire au Save/Load mais utilisé en interne d'un objet a un autre (swap)
	public ArrayList getConfig()
	{
		return new ArrayList();	
	}
	
	public void setConfig(ArrayList config)
	{
		
	}
#endregion

}
