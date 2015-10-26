using UnityEngine;
using System.Collections;
using System.IO;

using Pointcube.Utils;
using Pointcube.Global;

public class Function_ShadowScreenV2 : MonoBehaviour,Function_OS3D
{

#region public vars
	public GameObject 	footRef;			//Prefab de reference pour les pieds
	public GameObject 	attachRef;			//Prefab de reference pour les attaches
	
	public Material		matRef;				//materiaux de réference de la toile
	
	public Texture2D[] dots;				//imgs des dots pour UI
	public Texture2D[] lines;				//imgs des lines pour UI
	
	public Vector3[] defaultSail;			//Points de la voile par defaut
	
	public GUIStyle quadToggle;				//Style button tri/quad
	public GUIStyle showFeetToogle;			//Style button afficher/cacher pied
	public GUIStyle bgButton;				//Style button sélection pied
	
	public float 		footSize;			//Taille du pied (diametre)
	public float		attachDiam;			//Taille de l'attache (diametre)
	public float 		minHeight;			//Hauteur mini des pieds
	public float		maxHeight;			//Hauteur maxi des pieds
	
	public int 			nbIteration;		//nb iteration pour arc de la toile
	public int 			functionId;			//id du script(au cas ou il est en double)
	
	public bool isQuad;						//toile triangulaire ou quadra
	
	public Material feetMaterial;
//	private Material screenUpMat;
//	private Material screenDwnMat;
#endregion

#region private vars
	
	private Vector3[] 	_points;					// positions des poteaux
	private Vector3[]	_screenPoints; 				// points de la toile
	private Vector3[] 	_normals;					// normales des points du triangle de la toile
	private Vector3[] 	_anglesDirs;				// Direction vers l'interieur des points du triangle de la toile
	private Vector3[]	_midPoints;					//points Milieux des coté
	
	private float[]   	_offsets;					// off7 de chaque pied
	
	private bool[]		_shows;						// option affichage de chaquePied
	private bool[] 		_tilted;					// option d'inclinaison ou non du pied
	
	private Vector2 _oldMousePos;
	private Vector2 _uiScroll;					//scroll de l'UI
	
	private float _screenArea;					//Surface totale de la toile en m²
	private float _perimeter;					//Mêtre linéaire ^^
	private float _deltaSpeed = 0.01f;  		//facteur vitesse en fonction du déplacement du touch ou de la mouse
	private float _deltaScale;					//Scale Tactile
	
	private int _index = 0;						//index su pied sélectionné
	private int _scrollPos = 0;					//index de scroll des materiaux
	
	private bool _hasMoved;						//mouvement dans l'ui
    private bool _clickOnUI;                    // clic commencé sur l'UI
	private bool _firstBuild = true;			//premier build (pour éviter de mettre a jour les iosShadows)
	
	private GUISkin _uiSkin;					//Skin de l'UI
	
	private Rect _uiArea;						//Zone ou est l'UI

//	private GameObject _screen;					//GameObject de la toile
	private GameObject _screenUp;
	private GameObject _screenDwn;
	
	private Material _footMat;					//materiau des pieds
	
	private Quaternion _savedRotation;			//rotation du pergola
	private Vector3 _savedScale;				//scale du pergola
	private Vector3 _midPoint;					//point au millieu
	
	private float _oldDiag;						//utilisé pour calculer le scale de l'ios shadow
	
	private const string _screenName = "screenName";//nom du GO de la toile
	
	private float decalage = 0.12f;
	private float _tmr = 0;
	
	private const float _divCount = 10;
	private const float _subDivCount = 9; // = _divCount - 1;
	private float _arcAmp = 0.2f;
	
	private GameObject _iosShadows;	
	
#endregion
	
#region Unity Fcns
	
	// Use this for initialization
	void Start ()
	{		
		_iosShadows = GameObject.Find("iosShadows");
		if(_uiSkin == null)
			_uiSkin = (GUISkin)Resources.Load("skins/ShadeSailSkin");
	
//		feetMaterial = (Material)Resources.Load("materials/Chrome");
//		screenUpMat = (Material)Resources.Load("materials/screenUp");
//		screenDwnMat = (Material)Resources.Load("materials/screenDwn");
		
		if(transform.FindChild("screenUp") != null)
			_screenUp = transform.FindChild("screenUp").gameObject;
//		else
//		{
//			_screenUp = new GameObject("screenUp");
//			_screenUp.layer = 9;
//			_screenUp.transform.parent = transform;
//			_screenUp.transform.localPosition = Vector3.zero;
//			
//			_screenUp.AddComponent<MeshRenderer>();
//			_screenUp.renderer.material = matRef;
//			_screenUp.AddComponent<ApplyShader>();
//		}
		
		if(transform.FindChild("screenDwn") != null)
			_screenDwn = transform.FindChild("screenDwn").gameObject;
//		else
//		{
//			_screenDwn = new GameObject("screenDwn");
//			_screenUp.layer = 9;
//			_screenDwn.transform.parent = transform;
//			_screenDwn.transform.localPosition = Vector3.zero;
//			
//			_screenDwn.AddComponent<MeshRenderer>();
//			_screenDwn.renderer.material = matRef;
//			_screenDwn.AddComponent<ApplyShader>();
//		}
		
		// Function 0S3D
		_uiArea = new Rect(Screen.width-260, 0, 260, Screen.height);
        _clickOnUI = false;

		// build toilke par def
		_points = defaultSail;
		
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
		
		_tilted = new bool[4];
		_tilted[0] = false;
		_tilted[1] = false;
		_tilted[2] = false;
		_tilted[3] = false;	
		
		Rebuild();
		
//		_firstBuild = false;
		_tmr = Time.time + Time.maximumDeltaTime;
//		enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//--vv--Interaction--vv--
		
		if(_firstBuild && Time.time > _tmr)
		{
			_firstBuild = false;
			RecalculateBounds();
			enabled = false;
		}
		
//#if UNITY_STANDALONE
		Vector3 tmppoint = _points[_index];
		Vector2 pos = PC.In.GetCursorPos();
        if(PC.In.Click1Down())
        {
           if(_uiArea.Contains(PC.In.GetCursorPosInvY()))
               _clickOnUI = true;
        }

		if(!_uiArea.Contains(PC.In.GetCursorPosInvY()) || _hasMoved)
		{
            Vector2 deltaDrag;
			if(PC.In.Click1Down())
			{
				_oldMousePos = PC.In.GetCursorPos();
				_hasMoved = false;
                if(!_uiArea.Contains(PC.In.GetCursorPosInvY()))
                    _clickOnUI = false;
			}
			else if(PC.In.Click1Up())
			{
				if(!_hasMoved && !_clickOnUI)
                    Validate();
				_hasMoved = false;
			}
            else if(!_clickOnUI && PC.In.Drag1(out deltaDrag))             //Position
            {
                if(pos != _oldMousePos)
                {
                    MoveFeet(new Vector2(deltaDrag.x, deltaDrag.y));
                    _oldMousePos = pos;
                    _hasMoved = true;
                }
            }
		}

		//Offset
        float deltaScroll;
		if(PC.In.Zoom(out deltaScroll))
			ChangeFeetOffset(deltaScroll*10);
		
//#else
//		//Tactile INTERACTION
//		if(Input.touchCount > 0)
//		{
//			if(Input.touchCount == 1) // déplacement du pied en X/Z
//			{
//				Touch t0 = Input.touches[0];
//				if(!_uiArea.Contains(t0.position) || _hasMove)
//				{
//					if(t0.phase == TouchPhase.Began)
//					{
//						_hasMove = false;
//						_oldMousePos = t0.position;
//					}
//					
//					else if(t0.phase == TouchPhase.Moved)
//					{
//						MoveFeet(new Vector2(t0.position.x - _oldMousePos.x,t0.position.y - _oldMousePos.y));
//						_oldMousePos = t0.position;
//						_hasMove = true;
//					}
//					if(t0.phase == TouchPhase.Ended)
//					{
//						if(!_hasMove)
//							Validate();
//						_hasMove = false;
//					}
//				}
//			}
//			else if(Input.touchCount == 2)//scale du offset
//			{
//				Touch t0 = Input.touches[0];
//				Touch t1 = Input.touches[1];
//				if(!_uiArea.Contains(t0.position) && !_uiArea.Contains(t1.position))
//				{
//					if(t0.phase == TouchPhase.Began)
//						_deltaScale = Vector2.Distance(t0.position,t1.position);
//					else if(t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
//					{
//						float tmpDelta = Vector2.Distance(t0.position,t1.position);
//						ChangeFeetOffset(tmpDelta - _deltaScale);
//						_deltaScale = tmpDelta;
//					}
//				}
//			}
//		}
//#endif
		
//		_tgt.transform.position = new Vector3(_points[_index].x,0,_points[_index].z);	
	}
	
	//GUI
	void OnGUI()
	{			
//		if(GUI.Button(new Rect(Screen.width-200,Screen.height-200,200,200),"print"))
//		{
//			foreach(Vector3 p in _points)
//			{
//				Debug.Log(p);	
//			}
//		}
		
//		float tmpF = GUI.HorizontalSlider(new Rect(Screen.width/2-50,Screen.height-50,100,25),decalage,0,1);
//		if(tmpF != decalage)
//		{
//			decalage = tmpF;
//			Rebuild();
//		}
		
		if(_firstBuild)
			return;
		
		GUI.skin = _uiSkin;
		GUI.Box(new Rect(Screen.width-210.0f,0.0f,Screen.width-280.0f,Screen.height),"","BackGround");
		//UI du tracé
		UI3D();
		
		//UI du configurateur
		if(!_hasMoved)
		{
			//-------vv----FADE Haut-------------vv------------------
			GUILayout.BeginArea(_uiArea);
			GUILayout.FlexibleSpace();
			_uiScroll = GUILayout.BeginScrollView(_uiScroll,"","",GUILayout.Width(260));//scrollView en cas de menu trop grand
			GUILayout.Box("","UP",GUILayout.Width(260),GUILayout.Height(150));//fade en haut
			GUILayout.BeginVertical("MID",GUILayout.Width(260));
			
			//-------vv----UI Configurateur-------vv---------------
			GUILayout.FlexibleSpace();
			GUILayout.Label(TextManager.GetText("DynShelter.Title"),"Title");
			//---------------------------------------------
			GUILayout.BeginHorizontal(GUILayout.Height(50));
			GUILayout.FlexibleSpace();
			//affichage/masquage/incliné pied
			string btnStyle  = "";
			if(_shows[_index] && !_tilted[_index])
			{
				btnStyle = "poto";//tiltpoto
			}
			else if(_shows[_index] && _tilted[_index])
			{
				btnStyle = "tiltpoto";//nopoto
			}
			else
			{
				btnStyle = "nopoto";//poto
			}
			
			if(GUILayout.Button("",btnStyle,GUILayout.Height(50),GUILayout.Width(50)))
			{
				if(_shows[_index] && !_tilted[_index])
				{
					_tilted[_index] = true;
				}
				else if(_shows[_index] && _tilted[_index])
				{
					_shows[_index] = false;
				}
				else
				{
					_shows[_index]  = true;
					_tilted[_index] = false;
				}
				Rebuild();
			}
//			GUILayout.Space(10);
			
			//Double Toile
			bool tmp = GUILayout.Toggle(isQuad,"","triquad",GUILayout.Height(50),GUILayout.Width(50));
			if(tmp != isQuad)
			{
				isQuad = tmp;
				if(!isQuad)
					_index = 0;
				Rebuild();
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
			//---------------------------------------------
			
			//---------------------------------------------
			GUILayout.BeginHorizontal(GUILayout.Height(200));//pieds et hauteur
			GUILayout.FlexibleSpace();
			//		-----------------
			GUILayout.BeginVertical(/*"bg",*/GUILayout.Height(200),GUILayout.Width(50));//pieds
			UISelector();
			GUILayout.EndVertical();
			//		-----------------
//			GUILayout.Space(10);
			//		-----------------
			GUILayout.BeginVertical(/*"bg",*/GUILayout.Height(200),GUILayout.Width(50));//hauteur
			UISetHeight();
			GUILayout.EndVertical();
			//		-----------------
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
			//---------------------------------------------
			
			GUILayout.BeginHorizontal(GUILayout.Height(30));//pieds et hauteur
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Retour","mat",GUILayout.Height(30),GUILayout.Width(100)))
			{
				GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = true;
				Camera.main.GetComponent<ObjInteraction>().setActived(false);
				GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().enabled = true;
				GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(true);
				//GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().OpenMaterialTab();
				Camera.main.GetComponent<GuiTextureClip>().enabled = true;
				
				GameObject.Find("MainScene").GetComponent<GUIMenuLeft>().canDisplay(false);
				GameObject.Find("MainScene").GetComponent<GUIMenuRight>().canDisplay(false);
				
				enabled = false;
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
//			GUILayout.Space(10);
			//---------------------------------------------
//			GUILayout.Label(TextManager.GetText("GUIMenuRight.Hauteur")+" "+TextManager.GetText("ShadeSail.poto")+"\n"+((Vector3) _points[_index]).y.ToString("F") +"m","Title",GUILayout.Height(50));
//			GUILayout.Label(TextManager.GetText("ShadeSail.area")+"\n"+"~"+_screenArea.ToString("F")+TextManager.GetText("ShadeSail.metric"),"Title",GUILayout.Height(50));
//			GUILayout.Label(TextManager.GetText("ShadeSail.lineMetric")+"\n"+"~"+_perimeter.ToString("F")+"m","Title",GUILayout.Height(50));
			
			
			GUILayout.FlexibleSpace();
			//-------vv----FADE BAS-------------vv------------------
			GUILayout.EndVertical();
			GUILayout.Box("","DWN",GUILayout.Width(260),GUILayout.Height(150));//fade en bas
			
			GUILayout.EndScrollView();
			GUILayout.FlexibleSpace();
			
			GUILayout.EndArea();
		}

		
		// UpperDisplay
		GUILayout.BeginArea(new Rect(Screen.width/2-250,0,500,50),"","hudBg");
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(GetSurfaceString(),"hudTxt");
		GUILayout.Space(10);
		GUILayout.Label("-","hudTxt");
		GUILayout.Space(10);
		GUILayout.Label(GetLinearString(),"hudTxt");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		
	}
#endregion
	
#region Fcns
	
	//-----------vv-UI-vv---------------------
	//UI des dots and lines
	private void UI3D()
	{
		int max = isQuad? 4:3; 
		Vector2[] osps = new Vector2[max];
		float scl = transform.localScale.x;
		for(int i=0;i<max;i++)
		{
			
			Vector3 pos = transform.position + scl*_points[i].x*transform.right + scl*_points[i].z*transform.forward;
			osps[i] = Camera.main.WorldToScreenPoint(new Vector3(pos.x,0,pos.z)); // osp onScreenPos
		}
		
		for(int ii=0;ii<max;ii++) //Lines
		{			
			Vector2 p1 = osps[ii];
			p1.y = Screen.height - p1.y;
			Vector2 p2 = osps[ii==max-1? 0:ii+1];
			p2.y = Screen.height - p2.y;
			
			if(!isQuad && ii == max-1)
				Drawer.DrawLine(p1,p2,lines[lines.Length-1],4);
			else
				Drawer.DrawLine(p1,p2,lines[ii],4);
		}
		
		for(int ii=0;ii<max;ii++) //Dots
		{
			GUI.DrawTexture(new Rect(osps[ii].x-15,Screen.height-osps[ii].y-15,30,30),dots[ii]);//Passage en btns
		}
		
		for(int t=0;t<max;t++) //Lines length
		{
			if(t == _index || (t==max-1? 0:t+1) == _index)
			{
				Vector2 midLine = (osps[t] + osps[t==max-1? 0:t+1])/2;
				midLine.y = Screen.height - midLine.y;
				
				Vector3 cur = _points[t];
				cur.y = 0;
				Vector3 nxt = _points[t==max-1? 0:t+1];
				nxt.y = 0;
				
				float dist = Vector3.Distance(cur,nxt);
				GUI.Label(new Rect(midLine.x-30,midLine.y-15,60,30),dist.ToString("F")+"m","midLine");
			}
		}
		
	}
	
	//Choisit le pied a deplacer
	private void UISelector()
	{
		GUILayout.FlexibleSpace();
		for(int ui=0;ui<(isQuad?4:3);ui++)
		{
			if(GUILayout.Toggle(_index ==ui,dots[ui],bgButton,GUILayout.Height(50),GUILayout.Width(50)))
			{
				_index = ui;
			}
		}
		GUILayout.FlexibleSpace();
	}
 
	//règle la hauteur du pied choisi
	private void UISetHeight()
	{
		GUILayout.Space(12.5f);
		GUILayout.Label(((Vector3) _points[_index]).y.ToString("F") +"m","sizer",GUILayout.Height(25),GUILayout.Width(50));
		GUILayout.Space(12.5f);
		float tmpH = GUILayout.VerticalSlider(_points[_index].y,maxHeight,minHeight/*,GUILayout.Height(150)*/,GUILayout.Width(50));
		if(tmpH != _points[_index].y)
			ChangeFeetHeight(tmpH);
	}
	
	//-----------vv-BUILD-vv------------------
	
	//Calcule la surface du screen
	private void CalculateArea(Mesh m)
	{		
		_screenArea = 0;
		for(int t=0;t<m.triangles.Length;t=t+3)
		{
			int ip1 = m.triangles[t];
			int ip2 = m.triangles[t+1];
			int ip3 = m.triangles[t+2];
			
			Vector3 p1 = m.vertices[ip1];
			Vector3 p2 = m.vertices[ip2];
			Vector3 p3 = m.vertices[ip3];
			
			Vector3 cross = Vector3.Cross((p2-p1),(p3-p1));
			
			_screenArea += (cross.magnitude/2);
		}
	}
	
	//Rebuild
	private void Rebuild()
	{
		ClearIt();
		BuildIt();
	}
	
	//efface les GameObjects avant de recréer
	private void ClearIt()
	{
		foreach(Transform t in transform)
		{
//			if(t.name != _screenName)
			if(t.name != "screenUp" && t.name != "screenDwn")
				Destroy(t.gameObject);
//			else
//				foreach(Transform s in t)
//					Destroy(s.gameObject);
		}
		
	}
	
	//Build le voile d'ombrage
	private void BuildIt()
	{
		SaveTransform();
		
		CalculateDatas();
		
		BuildFeet();
		
		if(!isQuad)
		{
			BuildArcScreen(false);
			BuildArcScreen(true);
		}
		else
		{
			BuildQuadScreen2();
		}
		
		CalculateInfos();
		
		RecalculateBounds();
		
		ReapplyTransform();	
		
	//	_iosShadows.GetComponent<IosShadowManager>().UpdateShadowPos(gameObject);	
		
	}
	
	//Build les pieds
	private void BuildFeet()
	{
		_screenPoints = new Vector3[4];
		
		for(int i=0;i<(isQuad? 4:3);i++)
		{		
			GameObject dummyFoot = new GameObject("dummyFoot");
			dummyFoot.transform.parent = transform;
			dummyFoot.transform.localPosition = _points[i];
			Vector3 yRot = _anglesDirs[i];
			yRot.y = 0;
			dummyFoot.transform.localRotation = Quaternion.FromToRotation(dummyFoot.transform.right,yRot);
			
			float size;
			
			if(_tilted[i])
			{
				Vector3 tmp = dummyFoot.transform.localRotation.eulerAngles;
				tmp.z += 15;
				Quaternion q = new Quaternion(0,0,0,0);
				q.eulerAngles = tmp;
				dummyFoot.transform.localRotation = q;
				size = _points[i].y/Mathf.Abs(Mathf.Cos(Mathf.Deg2Rad*15));
			}
			else
				size = _points[i].y;
			
			GameObject foot = (GameObject) Instantiate(footRef);
			foot.name = "foot"+i;
			foot.layer = 9;
			foreach(Transform t in foot.transform)
				t.gameObject.layer = 9;
			foot.transform.parent = dummyFoot.transform;
			foot.transform.localPosition = new Vector3(/*_points[i].x*/0,-size/2,0/*_points[i].z*/);
			foot.transform.localScale = new Vector3(footSize,size,footSize);
			foot.transform.localRotation = new Quaternion(0,0,0,0);
			
			if(foot.GetComponent<Renderer>())
				foot.GetComponent<Renderer>().enabled = _shows[i];
			else
				foot.transform.GetChild(0).GetComponent<Renderer>().enabled = _shows[i];
			
			foot.transform.GetChild(0).GetComponent<Renderer>().material = feetMaterial;
			
			GameObject dummyP = new GameObject();
			dummyP.name = "Dummy"+i;
			dummyP.transform.parent = transform;
			dummyP.transform.localPosition = _points[i];
			dummyP.transform.localRotation = Quaternion.FromToRotation(dummyP.transform.right,_anglesDirs[i]);

			
			
			GameObject attach = (GameObject) Instantiate(attachRef);
			attach.name = "attach"+i;
			attach.layer = 9;
			attach.transform.parent = dummyP.transform;
			attach.transform.localPosition = new Vector3(_offsets[i]/2+footSize/4,0,0);
			attach.transform.localScale = new Vector3(_offsets[i]+footSize/2,attachDiam/2,attachDiam/2);
			attach.transform.localRotation = new Quaternion(0,0,0,0);
			attach.transform.GetChild(0).GetComponent<Renderer>().material = feetMaterial;

			_screenPoints[i] = _points[i]+(_offsets[i]+footSize/2)*dummyP.transform.right;
		}
	}
	
	//Build la toile
	private void BuildArcScreen(bool inverted)
	{
		//Creation Mesh
		Mesh m = new Mesh();
		m.Clear();
		m.name = "side"+ (inverted? "Dwn":"Up");
		
		//Calcul Arc points
		Vector3[]arcPoints = new Vector3[1 + (isQuad? 4:3)*nbIteration];
		Vector3[]norms = new Vector3[1 + (isQuad? 4:3)*nbIteration];
		
		int index =0;
		
		arcPoints[index] = _midPoint ;//- _normals[0]*0.01f;
		norms[index] = _normals[0];//Vector3.zero;
		index++;
		
		for(int po=0;po<(isQuad? 4:3);po++)
		{
			int cur = po;
			int nxt=0;
			Vector3 sens = Vector3.zero;
			
			if(isQuad)
			{
				nxt = (po==3? 0:po+1);
				sens =  _midPoints[po] - _midPoint;
			}
			else
			{
				switch(po)
				{
				case 0:
					nxt = 1;
					sens = _anglesDirs[2];
					break;
				case 1:
					nxt = 2;
					sens = _anglesDirs[0];
					break;
				case 2:
					nxt = 0;
					sens = _anglesDirs[1];
					break;
				}
			}
			
			arcPoints[index] = _screenPoints[cur];
			norms[index] = _normals[po];
			index ++;
			for(int iter=1;iter<nbIteration;iter++)
			{
				float decal = iter-(nbIteration/2);
				decal = decal*decal;
				decal = - decal + ((nbIteration/2)*(nbIteration/2));
				decal *=0.01f;
				
				arcPoints[index] =  _screenPoints[cur] + ((((float)iter/(float)nbIteration)*(_screenPoints[nxt]-_screenPoints[cur]))- sens*decal);
				norms[index] = _normals[po];
				index++;
			}
		}
		
		//Triangles
		int[] triangles = new int[3 * arcPoints.Length];
		index = 0;
		for(int ap=1;ap<arcPoints.Length;ap++)
		{
			if(inverted)
			{
				triangles[index] = 0;
				index++;
				triangles[index] = ap+1 == arcPoints.Length? 1:ap+1;
				index++;
				triangles[index] = ap;
				index++;
			}
			else
			{
				triangles[index] = 0;
				index++;
				triangles[index] = ap;
				index++;
				triangles[index] = ap+1 == arcPoints.Length? 1:ap+1;
				index++;
			}
		}
		
			//UV's 
		Vector2[] uvs = new Vector2[1 + (isQuad? 4:3)*nbIteration];
		for(int uvp=0;uvp<uvs.Length;uvp++)
		{
			uvs[uvp] = new Vector2(arcPoints[uvp].x,arcPoints[uvp].z);
		}
		
		m.vertices = arcPoints;
		m.triangles = triangles;
		m.uv = uvs;
		m.normals = norms;
		
		Color[] colors = new Color[arcPoints.Length];
        for(int i=0;i<arcPoints.Length;i++)
		{
            colors[i] = Color.white;
        }
        m.colors = colors;
		
		m.RecalculateBounds();
		
		m.RecalculateNormals();
		CalculateMeshTangents(m);
		m.Optimize();
		
		CalculateArea(m);
		
//		GameObject screen = new GameObject("screen"+(inverted? "Dwn":"Up"));//screenUp
//		screen.layer = 9;
//		screen.transform.parent = transform;//_screen.transform;
//		screen.transform.localPosition = Vector3.zero;
//		screen.transform.localScale = Vector3.one;
//		screen.AddComponent<MeshFilter>();
//		screen.AddComponent<MeshRenderer>();
//        screen.GetComponent<MeshFilter>().mesh = m;
//		screen.renderer.material = (inverted? screenDwnMat:screenUpMat);
		
		if(inverted)
		{
			_screenDwn.GetComponent<MeshFilter>().mesh = null;
			_screenDwn.GetComponent<MeshFilter>().mesh = m;
		}
		else
		{
			_screenUp.GetComponent<MeshFilter>().mesh = null;
			_screenUp.GetComponent<MeshFilter>().mesh = m;
		}
		
//		if(!inverted)
//		{
//			if(!GetComponent<MeshCollider>())
//			{
//				gameObject.AddComponent<MeshCollider>();
//			}
//			GetComponent<MeshCollider>().sharedMesh = m;
//			GetComponent<MeshCollider>().convex = false;
//		}
	}
	
//	private void BuildQuadScreen()
//	{
//		Vector3 pMid = _screenPoints[0] + _screenPoints[1] + _screenPoints[2] + _screenPoints[3];
//		pMid = pMid/4;
//
//		//CALCUL ABCD --------------------
//		Vector3[] ABCD = new Vector3[4];
//		for(int i=0;i<4;i++)
//		{
//			ABCD[i] = (_screenPoints[i] + _screenPoints[i==3? 0:i+1])/2;
//		}
//		
//		for(int ii=0;ii<4;ii++)
//		{
//			Vector3 dir = ABCD[ii>=2? ii-2:ii+2] - ABCD[ii];
//			ABCD[ii] = ABCD[ii] + (decalage*dir);
//		}
//		
//		//CALCUL Step1 --------------------
//		Vector3[] step1 = new Vector3[4];
//		for(int s1=0;s1<4;s1++)
//		{
//			step1[s1] = (_screenPoints[s1] + pMid + ABCD[s1] + ABCD[ s1==0? 3:(s1-1)])/4;
//		}
//		
//		//CALCUL Step2 --------------------
//		Vector3[] step2 = new Vector3[4];
//		for(int s2=0;s2<4;s2++)
//		{
//			step2[s2] = (ABCD[s2]+pMid+step1[s2]+step1[s2==3? 0:(s2+1)])/4;	
//		}
//		
//		//CALCUL Step3 --------------------
//		Vector3[] step3 = new Vector3[8];
//		int index = 0;
//		for(int s3=0;s3<4;s3++)
//		{
//			step3[index] = _screenPoints[s3]*0.42f + ABCD[s3]*0.42f + step1[s3]*0.16f;
//			index++;
//			step3[index] = _screenPoints[s3==3? 0:(s3+1)]*0.42f + ABCD[s3]*0.42f + step1[s3==3? 0:(s3+1)]*0.16f;
//			index++;
//		}
//		
//		//CALCUL Step4 --------------------
//		Vector3[] step4 = new Vector3[4];
//		for(int s4=0;s4<4;s4++)
//		{
//			step4[s4] = ABCD[s4]*0.73f + (_screenPoints[s4] + pMid + _screenPoints[s4==3? 0:(s4+1)])*0.09f;
//		}
//		
//		//Remplissage des points --------------------
//		Vector3[] verts = new Vector3[25];
//		int rIndex =0;
//		verts[rIndex] = pMid;
//		rIndex ++;
//		
//		foreach(Vector3 v in step1)//1 -> 4
//		{
//			verts[rIndex] = v;
//			rIndex++;
//		}
//		
//		foreach(Vector3 v in step2)//5 -> 8
//		{
//			verts[rIndex] = v;
//			rIndex++;
//		}
//		
//		foreach(Vector3 v in _screenPoints)//9 -> 12
//		{
//			verts[rIndex] = v;
//			rIndex++;
//		}
//		foreach(Vector3 v in ABCD)//13 -> 16
//		{
//			verts[rIndex] = v;
//			rIndex++;
//		}
//		foreach(Vector3 v in step3)//17 -> 25
//		{
//			verts[rIndex] = v;
//			rIndex++;
//		}
//		
//		//Creation des tris
//		int[] tris = new int[96*2];
//		int triIndex = 0;
//		for(int t=1;t<5;t++)
//		{
//			tris[triIndex] = 0;
//			triIndex ++;
//			tris[triIndex] = t;
//			triIndex ++;
//			tris[triIndex] = 4+t;
//			triIndex ++;
//			
//			tris[triIndex] = 0;
//			triIndex ++;
//			tris[triIndex] = 4+t;
//			triIndex ++;
//			tris[triIndex] = (t==4? 1:(t+1));
//			triIndex ++;
//			
//		}
//		
//		
//		for(int tt=0;tt<4;tt++)
//		{
//			tris[triIndex] = 5+tt;
//			triIndex ++;
//			tris[triIndex] = 1+tt;
//			triIndex ++;
//			tris[triIndex] = 17+(tt*2);
//			triIndex ++;
//			tris[triIndex] = 5+tt;
//			triIndex ++;
//			tris[triIndex] = 17+(tt*2);
//			triIndex ++;
//			tris[triIndex] = 13+tt;
//			triIndex ++;
//			tris[triIndex] = 5+tt;
//			triIndex ++;
//			tris[triIndex] = 13+tt;
//			triIndex ++;
//			tris[triIndex] = 18+(tt*2);
//			triIndex ++;
//			tris[triIndex] = 5+tt;
//			triIndex ++;
//			tris[triIndex] = 18+(tt*2);
//			triIndex ++;
//			tris[triIndex] = tt==3? 1:tt+2;
//			triIndex ++;
//		}
//		
//		for(int ttt=0;ttt<4;ttt++)
//		{
//			if(ttt == 0)
//			{
//				tris[triIndex] = ttt+1;
//				triIndex ++;
//				tris[triIndex] = 9+ttt;
//				triIndex ++;
//				tris[triIndex] = 17+ttt;
//				triIndex ++;
//				tris[triIndex] = ttt+1;
//				triIndex ++;
//				tris[triIndex] = 24+ttt;
//				triIndex ++;
//				tris[triIndex] = 9+ttt;
//				triIndex ++;	
//			}
//			else
//			{
//				tris[triIndex] = ttt+1;
//				triIndex ++;
//				tris[triIndex] = (8+ttt)*2;
//				triIndex ++;
//				tris[triIndex] = 9+ttt;
//				triIndex ++;
//				tris[triIndex] = ttt+1;
//				triIndex ++;
//				tris[triIndex] = 9+ttt;
//				triIndex ++;
//				tris[triIndex] = (8+ttt)*2 + 1;
//				triIndex ++;
//			}
//		}
//		
//		//Uvs
//		Vector2[] uvs = new Vector2[verts.Length];
//		for(int u=0;u<uvs.Length;u++)
//		{
//			uvs[u] = new Vector2(verts[u].x,verts[u].z);	
//		}
//		
//		Mesh m = new Mesh();
//		Mesh mback = new Mesh();
//		m.Clear();
//		mback.Clear();
//		
//		m.name = "screenMesh";
//		mback.name = "backFace";
//		
//		m.vertices = verts;
//		mback.vertices = verts;
//		
//		m.triangles = tris;
//		int[] tmptris = tris;
//		for(int i=0;i<tmptris.Length;i=i+3)
//		{
//			int tmp = tmptris[i+1];
//			tmptris[i+1] = tmptris[i+2];
//			tmptris[i+2] = tmp;
//		}
//		mback.triangles = tmptris;
//		
//		m.normals = new Vector3[verts.Length];
//		mback.normals = new Vector3[verts.Length];
//		
//		m.uv = uvs;
//		mback.uv = uvs;
//		
//		m.RecalculateBounds();
//		m.RecalculateNormals();
//		CalculateMeshTangents(m);
//		m.Optimize();
//		
//		CalculateArea(m);
//		
//		mback.RecalculateBounds();
//		mback.RecalculateNormals();
//		CalculateMeshTangents(mback);
//		mback.Optimize();
//		
//		GameObject screen = new GameObject("screenUp");
//		screen.layer = 9;
//		screen.transform.parent = transform;//_screen.transform;
//		screen.transform.localPosition = Vector3.zero;
//		screen.transform.localScale = Vector3.one;
//		screen.AddComponent<MeshFilter>();
//		screen.AddComponent<MeshRenderer>();
//        screen.GetComponent<MeshFilter>().mesh = m;
//		screen.renderer.material = screenUpMat;
////		screen.renderer.material = _screen.renderer.material;
////		screen.AddComponent<ApplyShader>();
//		
//		GameObject screenBack = new GameObject("screenDwn");
//		screenBack.layer = 9;
//		screenBack.transform.parent = transform;//_screen.transform;
//		screenBack.transform.localPosition = Vector3.zero;
//		screenBack.transform.localScale = Vector3.one;
//		screenBack.AddComponent<MeshFilter>();
//		screenBack.AddComponent<MeshRenderer>();
//        screenBack.GetComponent<MeshFilter>().mesh = mback;
//		screenBack.renderer.material = screenDwnMat;
////		screenBack.renderer.material = _screen.renderer.material;
////		screenBack.AddComponent<ApplyShader>();
//		
//		if(!GetComponent<MeshCollider>())
//		{
//			gameObject.AddComponent<MeshCollider>();
//		}
//		GetComponent<MeshCollider>().sharedMesh = m;
//	}
	
	private void BuildQuadScreen2()
	{
		//Création des points
		Vector3[] verts = new Vector3[(int)_divCount * (int)_divCount];
		
		Vector3 A = _screenPoints[0];
		Vector3 B = _screenPoints[1];
		Vector3 C = _screenPoints[2];
		Vector3 D = _screenPoints[3];
		
		Vector3 mABmDC =((D+C)/2) - ((A+B)/2);
		Vector3 mADmBC =((B+C)/2) - ((A+D)/2);
		
		mABmDC = mABmDC.normalized;
		mADmBC = mADmBC.normalized;
		
		float arcX,arcY,invX,invY;
		
		for(int y=0;y<_divCount;y++)
		{
//			if(y == 0 || y == _divCount-1)
//				arcY = 0;
//			else
//				arcY = _arcAmp * Mathf.Sin((y/_divCount)*Mathf.PI); // valeurs de l'arc
				arcY = _arcAmp * ((-(y/(_subDivCount/2)-1)*(y/(_subDivCount/2)-1))+1);
			invY = 1-((2*y)/_divCount);// valeur de l'écrasement/inversion de l'arc
			
			for(int x=0;x<_divCount;x++)
			{
//				if(x == 0 || x == _divCount-1)
//					arcX = 0;
//				else
//					arcX = _arcAmp * Mathf.Sin((x/_divCount)*Mathf.PI);
					arcX = _arcAmp * ((-(x/(_subDivCount/2)-1)*(x/(_subDivCount/2)-1))+1);
				invX = 1-((2*x)/_divCount);
					
				float cf9x = (_subDivCount-x)/_subDivCount;
				float cf9y = (_subDivCount-y)/_subDivCount;
				float cfx = x/_subDivCount;
				float cfy = y/_subDivCount;
				
				verts[y*(int)_divCount+x] = cf9x*cf9y*A + cfx*cf9y*B + cf9x*cfy*D + cfx*cfy*C;
				verts[y*(int)_divCount+x] += invY*arcX*mABmDC + invX*arcY*mADmBC;				
			}
		}
		
		//Création des tris
		int[] tris = new int[(int)_subDivCount*(int)_subDivCount*2*3];
		int idxr=0;
		for(int y=0;y<(int)_subDivCount;y++)
		{
			for(int x=0;x<(int)_subDivCount;x++)
			{
				tris[idxr] = (y*(int)_divCount) + x;
				idxr++;
				tris[idxr] = (y*(int)_divCount) + x + 1;
				idxr++;
				tris[idxr] = (y+1)*(int)_divCount + (x+1);
				idxr++;
				
				tris[idxr] = (y*(int)_divCount) + x;
				idxr++;
				tris[idxr] = (y+1)*(int)_divCount + (x+1);
				idxr++;
				tris[idxr] = (y+1)*(int)_divCount + x;
				idxr++;
			}
		}
		
		//Uvs
		Vector2[] uvs = new Vector2[verts.Length];
		for(int u=0;u<uvs.Length;u++)
		{
			uvs[u] = new Vector2(verts[u].x,verts[u].z);	
		}
		
		Mesh m = new Mesh();
		Mesh mback = new Mesh();
		m.Clear();
		mback.Clear();
		
		m.name = "sideUp";
		mback.name = "sideDwn";
		
		m.vertices = verts;
		mback.vertices = verts;
		
		m.triangles = tris;
		int[] tmptris = tris;
		for(int i=0;i<tmptris.Length;i=i+3)
		{
			int tmp = tmptris[i+1];
			tmptris[i+1] = tmptris[i+2];
			tmptris[i+2] = tmp;
		}
		mback.triangles = tmptris;
		
		m.normals = new Vector3[verts.Length];
		mback.normals = new Vector3[verts.Length];
		
		m.uv = uvs;
		mback.uv = uvs;
		
		m.RecalculateBounds();
		m.RecalculateNormals();
		CalculateMeshTangents(m);
		m.Optimize();
		
		CalculateArea(m);
		
		mback.RecalculateBounds();
		mback.RecalculateNormals();
		CalculateMeshTangents(mback);
		mback.Optimize();
		
//		GameObject screen = new GameObject("screenUp");
//		screen.layer = 9;
//		screen.transform.parent = transform;//_screen.transform;
//		screen.transform.localPosition = Vector3.zero;
//		screen.transform.localScale = Vector3.one;
//		screen.AddComponent<MeshFilter>();
//		screen.AddComponent<MeshRenderer>();
//        screen.GetComponent<MeshFilter>().mesh = m;
//		screen.renderer.material = screenUpMat;
////		screen.renderer.material = _screen.renderer.material;
////		screen.AddComponent<ApplyShader>();
//		
//		GameObject screenBack = new GameObject("screenDwn");
//		screenBack.layer = 9;
//		screenBack.transform.parent = transform;//_screen.transform;
//		screenBack.transform.localPosition = Vector3.zero;
//		screenBack.transform.localScale = Vector3.one;
//		screenBack.AddComponent<MeshFilter>();
//		screenBack.AddComponent<MeshRenderer>();
//        screenBack.GetComponent<MeshFilter>().mesh = mback;
//		screenBack.renderer.material = screenDwnMat;
////		screenBack.renderer.material = _screen.renderer.material;
////		screenBack.AddComponent<ApplyShader>();
		
		_screenUp.GetComponent<MeshFilter>().mesh = null;
		_screenUp.GetComponent<MeshFilter>().mesh = m;
		
		_screenDwn.GetComponent<MeshFilter>().mesh = null;
		_screenDwn.GetComponent<MeshFilter>().mesh = mback;
		
		
//		if(!GetComponent<MeshCollider>())
//		{
//			gameObject.AddComponent<MeshCollider>();
//		}
//		GetComponent<MeshCollider>().sharedMesh = m;
//		GetComponent<MeshCollider>().convex = true;
		
	}
	
	//-----------vv-Calculs-vv----------------
	
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
		//Calcul des normales
		_normals = new Vector3[4];
		if(isQuad)
		{
			_normals[0] = Vector3.Cross(_points[1]-_points[0],_points[3]-_points[0]);
			_normals[1] = Vector3.Cross(_points[2]-_points[1],_points[0]-_points[1]);
			_normals[2] = Vector3.Cross(_points[3]-_points[2],_points[1]-_points[2]);
			_normals[3] = Vector3.Cross(_points[0]-_points[3],_points[2]-_points[3]);
		}
		else
		{
			_normals[0] = Vector3.Cross(_points[1]-_points[0],_points[2]-_points[0]);
			_normals[1] = Vector3.Cross(_points[2]-_points[1],_points[0]-_points[1]);
			_normals[2] = Vector3.Cross(_points[0]-_points[2],_points[1]-_points[2]);
			_normals[3] = Vector3.zero;
		}
		
		//MidPoint
		_midPoint = Vector3.zero;
		for(int mp=0;mp<(isQuad? 4:3);mp++)
		{
			_midPoint += _points[mp];	
		}
		
		_midPoint = _midPoint/(isQuad? 4f:3f);
		
		//midPoints
		_midPoints = new Vector3[4];
		if(isQuad)
		{
			_midPoints[0] = (_points[0]+_points[1])/2;
			_midPoints[1] = (_points[1]+_points[2])/2;
			_midPoints[2] = (_points[2]+_points[3])/2;
			_midPoints[3] = (_points[3]+_points[0])/2;
		}
		else
		{
			_midPoints[0] = (_points[0]+_points[1])/2;
			_midPoints[1] = (_points[1]+_points[2])/2;
			_midPoints[2] = (_points[2]+_points[0])/2;
			_midPoints[3] = Vector3.zero;
		}
		
		//AngleDirs
		_anglesDirs = new Vector3[4];
		
		_anglesDirs[0] = _midPoint - _points[0];
		_anglesDirs[1] = _midPoint - _points[1];
		_anglesDirs[2] = _midPoint - _points[2];
		_anglesDirs[3] = _midPoint - _points[3];

//		for(int i=0;i<4;i++)
//			_anglesDirs[i] = Vector3.Normalize(_anglesDirs[i]);
		
	}
	
	//Calcul des données d'info
	private void CalculateInfos()
	{
		int max = isQuad? 4:3; 
		_perimeter = 0;
		for(int t=0;t<max;t++) //Lines length
		{
			Vector3 cur = _points[t];
			cur.y = 0;
			Vector3 nxt = _points[t==max-1? 0:t+1];
			nxt.y = 0;
			
			_perimeter += Vector3.Distance(cur,nxt);
		}
	}

	//déplace le point du screen sélectionné
	private void MoveFeet(Vector2 delta)
	{
		Vector3 relativeDirection  = new Vector3(_deltaSpeed*delta.x ,0, _deltaSpeed*delta.y);
		
        Vector3 absoluteDirection = Camera.main.transform.rotation * relativeDirection ;
		absoluteDirection.y=0;
		/*
		float oldy = _points[_index].y;
		Vector3 cdmInW = _points[_index];
		cdmInW.y = 0;
		Vector3 cdmOnScreen3 = Camera.mainCamera.WorldToScreenPoint(cdmInW);
		Vector2 cdmOnScreen2 = new Vector2(cdmOnScreen3.x,cdmOnScreen3.y);
		cdmOnScreen2 = cdmOnScreen2-delta;
		RaycastHit newhitm = new RaycastHit ();
		Physics.Raycast (Camera.mainCamera.ScreenPointToRay(cdmOnScreen2), out newhitm, Mathf.Infinity,9);
		if(newhitm.rigidbody)
		{
			_points[_index] = newhitm.point;
			_points[_index].y = oldy;
		}		*/
		_points[_index] += transform.InverseTransformDirection(absoluteDirection);
		Rebuild();
	}
	
	//change la hauteur du point du screen sélectionné
	private void ChangeFeetHeight(float delta)
	{
		if(delta>=0)
		{
			_points[_index].y =delta;
			Rebuild();
		}
	}
	
	//change l'offset du point du screen sélectionné
	private void ChangeFeetOffset(float delta)
	{	
		_offsets[_index] += delta * _deltaSpeed;
		if(_offsets[_index] <0)
			_offsets[_index] = 0;
		Rebuild();
	}
	
	private void SaveTransform()
	{
		_savedRotation = transform.localRotation;
		_savedScale = transform.localScale;
		
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
	
	private void ReapplyTransform()
	{
		transform.localRotation = _savedRotation;
		transform.localScale = _savedScale;
	} 
	
	private void RecalculateBounds()
	{

		if(!GetComponent<BoxCollider>())
			gameObject.AddComponent<BoxCollider>();
		
		gameObject.GetComponent<BoxCollider>().size = Vector3.zero;
		gameObject.GetComponent<BoxCollider>().center = Vector3.zero;
		
		Bounds b =new Bounds(Vector3.zero,Vector3.zero);//= UsefulFunctions.getMeshBounds(gameObject);

		for(int i=0;i<(isQuad? 4:3);i++)
		{
			b.Encapsulate(new Vector3(_points[i].x,_points[i].y+0.2f,_points[i].z));
//			b.Encapsulate(new Vector3(_points[i].x,0,_points[i].z));
		}
		
		float[] vals = new float[(isQuad? 4:3)];
		for(int p=0;p<(isQuad? 4:3);p++)
		{
			vals[p] = _points[p].y;	
		}
		
		float min = Mathf.Min(vals);
		float max = Mathf.Max(vals);
		
		float sizeY = max-min;
		float centerY = min + sizeY/2;
		
//		gameObject.GetComponent<BoxCollider>().size = new Vector3(b.size.x*1.25f,/*b.size.y*/sizeY,b.size.z*1.25f);
////		gameObject.GetComponent<BoxCollider>().center = b.center;
//		gameObject.GetComponent<BoxCollider>().center = new Vector3(b.center.x,centerY,b.center.y);
		
		if(!_firstBuild)
		{
			gameObject.GetComponent<BoxCollider>().size = new Vector3(b.size.x*1.25f,/*b.size.y*/sizeY,b.size.z*1.25f);
			gameObject.GetComponent<BoxCollider>().center = new Vector3(b.center.x,centerY,b.center.z);
			
			UpdateShadows();
		}
		else
		{
			gameObject.GetComponent<BoxCollider>().size = new Vector3(b.size.x*1.25f,b.size.y,b.size.z*1.25f);
			gameObject.GetComponent<BoxCollider>().center = b.center;
			
			RecalcOldDiag();
		}
//		Destroy(gameObject.GetComponent<BoxCollider>());
		
	}
	
	//MAJ de la iosShadow
	private void UpdateShadows()
	{
		float newDiag = _3Dutils.GetBoundDiag(gameObject.GetComponent<BoxCollider>().bounds);
		
		float factor = newDiag / _oldDiag;
				
		UsefullEvents.FireUpdateIosShadowScale(gameObject,factor);
		
//		_oldDiag = newDiag;
		
	}
	
	//utilisé quand on scale la scene
	private void RecalcOldDiag()
	{
		_oldDiag = _3Dutils.GetBoundDiag(gameObject.GetComponent<BoxCollider>().bounds);
	}
	
	private string GetSurfaceString()
	{
		return TextManager.GetText("ShadeSail.area")+" "+_screenArea.ToString("F")+TextManager.GetText("ShadeSail.metric");
	}
	
	private string GetLinearString()
	{
		return TextManager.GetText("ShadeSail.lineMetric")+" "+_perimeter.ToString("F")+"m";
	}
	
	private void CreateBoxCollider()
	{
		if(!GetComponent<BoxCollider>())
		{
			gameObject.AddComponent<BoxCollider>();
		}
		
		
	}
	
#endregion
	
#region Function_OS3D
	public string GetFunctionName(){return TextManager.GetText("DynShelter.Title");}
	public string GetFunctionParameterName(){return TextManager.GetText("DynShelter.Title");}
	
	public int GetFunctionId(){return functionId;}
	
	public void DoAction()
	{
		GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);
			
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;
		Camera.main.GetComponent<GuiTextureClip>().enabled = false;
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,true);
		Camera.main.GetComponent<ObjInteraction>().setActived(false);	
		enabled = true;
		UsefullEvents.ShowHelpPanel += Validate;
	}
	
	public void Validate()
	{

        Camera.main.GetComponent<ObjInteraction>().configuringObj(null);
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().unConfigure();
		GameObject.Find("MainScene").GetComponent<GUIMenuInteraction> ().setVisibility (false);
		Camera.main.GetComponent<ObjInteraction>().setSelected(null,false);
		Camera.main.GetComponent<GuiTextureClip>().enabled = true;
		UsefullEvents.ShowHelpPanel -= Validate;
		enabled = false;	
	}
	
	//  sauvegarde/chargement
	
	public void save(BinaryWriter buf)
	{
		for(int i=0;i<4;i++)
		{
			buf.Write(_points[i].x);
			buf.Write(_points[i].y);
			buf.Write(_points[i].z);
			
			buf.Write(_offsets[i]);
			buf.Write(_shows[i]);
			buf.Write(_tilted[i]);
		}
		
		buf.Write(isQuad);
	}
	
	public void load(BinaryReader buf)
	{
		_points = new Vector3[4];
		_offsets = new float[4];
		_shows = new bool[4];
		
		for(int i=0;i<4;i++)
		{
			Vector3 tmp = new Vector3(0,0,0);
			tmp.x = buf.ReadSingle();
			tmp.y = buf.ReadSingle();
			tmp.z = buf.ReadSingle();
			
			_points[i] = tmp;
			_offsets[i] = buf.ReadSingle();
			_shows[i] = buf.ReadBoolean();
			if(!LibraryLoader.numVersionInferieur(Montage.cdm.versionSave,"1.4.3"))
			{
				_tilted[i] = buf.ReadBoolean();
			}
		}

		if(!LibraryLoader.numVersionInferieur(Montage.cdm.versionSave,"1.3.3"))
		{
			isQuad = buf.ReadBoolean();
		}
		
		Rebuild();
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
		ArrayList ar = new ArrayList();
		
		ar.Add(_points);
		
		return new ArrayList();	
	}
	
	public void setConfig(ArrayList config)
	{
		if(config.Count>0)
			_points = (Vector3[])config[0];
		Rebuild();
	}
	
#endregion

}
