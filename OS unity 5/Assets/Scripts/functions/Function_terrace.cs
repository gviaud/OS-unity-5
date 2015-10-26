using UnityEngine;
using System.Collections;
using Pointcube.Global;
using System.Collections.Generic;
using System.IO;



/* La mesh de plage est g�n�r�e en triangulant le polygone con�u par l'utilisateur
 * La m�thode utilis�e est celle "des oreilles", son algorythme �tant d�cris ici : http://fr.wikipedia.org/wiki/Triangulation_d%27un_polygone */
public class Function_terrace : MonoBehaviour, Function_OS3D
{
    class PVertex
    {

       /* public Vector3 position
        {
            get { return gameObject.transform.position; }
            set { gameObject.transform.position = value; }
        }*/
        public Vector3 Position
        {
            get { return gameObject.transform.position; }
            set { 
				gameObject.transform.position = value; }
        }
        public GameObject gameObject;

        //liste des autres PVertex li�s � celui-ci par des arr�tes
        public List<PVertex> linked=new List<PVertex>();

        //OS3D functions
        public void save(BinaryWriter buf){
            buf.Write((double)Position.x);
            buf.Write((double)Position.y);
            buf.Write((double)Position.z);
        }
        public void save(ArrayList al)
        {
            al.Add(Position.x);
            al.Add(Position.y);
            al.Add(Position.z);
           /* al.Add(gameObject.transform.localPosition.x);
            al.Add(gameObject.transform.localPosition.y);
            al.Add(gameObject.transform.localPosition.z);*/
        }

        /// <summary>
        /// Chargement d'un PVertex depuis un BinaryReader
        /// </summary>
        /// <param name="buf">BinaryReader de la sauvegarde</param>
        /// <param name="fun">terrace</param>
        /// <returns></returns>
        public static PVertex load(BinaryReader buf,Function_terrace fun){
            PVertex v= new PVertex();
           // vgo.transform.position = position;
            float x=(float)buf.ReadDouble();
             float y=(float)buf.ReadDouble();
             float z=(float)buf.ReadDouble();
             Debug.Log("loading vertex");
             Debug.Log("x=" + x);
             Debug.Log("y=" + y);
             Debug.Log("z=" + z);
             v.gameObject = fun.createVertexObject(new Vector3(x, y, z));
            return v;
        }
		
		public static PVertex load(Vector3 vert, Function_terrace fun){
            PVertex v= new PVertex();
           // vgo.transform.position = position;
             Debug.Log("loading vertex");
             Debug.Log("x=" + vert.x);
             Debug.Log("y=" + vert.y);
             Debug.Log("z=" + vert.z);
             v.gameObject = fun.createVertexObject(vert);
            return v;
        }

        /// <summary>
        /// chargement d'un PVertex depuis un arraylist
        /// </summary>
        /// <param name="al"></param>
        /// <param name="index">index du vertex dans l'arraylist</param>
        /// <param name="fun"></param>
        /// <returns></returns>
        public static PVertex load(ArrayList al,int index, Function_terrace fun)
        {
            PVertex v = new PVertex();
            v.gameObject = fun.createVertexObject(new Vector3((float)al[index], (float)al[index+1], (float)al[index+2]));
            return v;
        }

        /// <summary>
        /// retourne l'index du Vertex
        /// </summary>
        /// <param name="m_vertices">liste de vertices dans laquelle chercher l'index</param>
        /// <returns></returns>
        public int getIndex(LinkedList<PVertex> m_vertices){
            int index=0;
            foreach(PVertex vtemp in m_vertices){
                if(vtemp==this){
                    return index;
                }
                index++;
            }
          return -1;
        }

        /// <summary>
        /// retourne le Vertex correspondant � l'index pass� en param�tre
        /// </summary>
        public static PVertex vertexFromIndex(int index,LinkedList<PVertex> m_vertices){
             int i=0;
            foreach(PVertex vtemp in m_vertices){
                if(i==index){
                    return vtemp;
                }
                i++;
            }
            return null;
        }
            
    }


    class PEdge
    {
        public PVertex v1,v2;
        public GameObject gameObject;

        //OS3D functions
        public void save(BinaryWriter buf,LinkedList<PVertex> m_vertices){
            buf.Write(v1.getIndex(m_vertices));
            buf.Write(v2.getIndex(m_vertices));
        }
        public void save(ArrayList al, LinkedList<PVertex> m_vertices)
        {
            al.Add(v1.getIndex(m_vertices));
            al.Add(v2.getIndex(m_vertices));
        }

        public static PVertex[] load(BinaryReader buf,LinkedList<PVertex> m_vertices){
            return new PVertex[]{PVertex.vertexFromIndex(buf.ReadInt32(),m_vertices),PVertex.vertexFromIndex(buf.ReadInt32(),m_vertices)};
        }
        public static PVertex[] load(ArrayList al, LinkedList<PVertex> m_vertices, int index)
        {
            Debug.Log("loading edge at index" + index);
            //int i1 = Mathf.CeilToInt((float)al[index]);
           // int i2 = Mathf.CeilToInt((float)al[index+1]);
            int i1 = (int)al[index];
            int i2 = (int)al[index+1];
            Debug.Log("al[index]=" + al[index+1]);
            return new PVertex[] { PVertex.vertexFromIndex(i1, m_vertices), PVertex.vertexFromIndex(i2, m_vertices) };
        }
    }

    GameObject mainScene;

    /// <summary>Liste de vertices actuelle (modifi�e pendant la g�n�ration du polygone</summary>
    LinkedList<PVertex> m_vertices = new LinkedList<PVertex>();

    /// <summary>Liste de vertices sauvegard�e, restaur�e une fois le polygone g�n�r�</summary>
    LinkedList<PVertex> m_verticesSaved = new LinkedList<PVertex>();

    /// <summary>Liste des arr�tes</summary>
   List<PEdge> m_edges = new List<PEdge>();

    /// <summary>Arr�tes sauvegard�es</summary>
    List<PEdge> m_edgesSaved = new List<PEdge>();

     /// <summary>Liste des triangles du mesh (liste de tripl�s d'index de vertices)</summary>
  List<int> m_trisList=new List<int>();

   /// <summary>Liste des vertices du mesh (liste de positions)</summary>
   LinkedList<Vector3> m_vertexList = new LinkedList<Vector3>();
	
   /// <summary>Liste des vertices Initiale du mesh (liste de positions)</summary>
    Vector3[] m_vertexList_Original = new Vector3[]{
		new Vector3(1,0,1),
		new Vector3(-1,0,1),
		new Vector3(-1,0,-1),
		new Vector3(1,0,-1)};

   /// <summary>contiens le vertex s�lectionn� pour modification</summary>
    private PVertex m_vertexSelected=null;

    /// <summary>contiens l'ar�te s�lectionn�e pour ajout d'un vertex</summary>
    private PEdge m_edgeSelected=null;

    /// <summary>Mode �dition : vrai=>modification vertex, false=>plage g�n�r�e</summary>
    private bool m_editingMode=true;

    private bool m_thicknessMode = false;

    /// <summary>Mati�re de l'edge faisant la jonction entre, le premier et le dernier Vertex. Placez une valeur dans l'editeur pour changer celui par defaut.</summary>
    private Material m_matLastEdge = null; 

    /// <summary>GameObject de la plage</summary>
    private GameObject m_plageMesh=null;

    private int m_needColliderUpdate=0;

    /// <summary>renderer de l'objet plage qui sert � stocker la mati�re s�lectionn�e </summary>
    private Renderer m_curMatRenderer = null;

    /// <summary> tiling de la texture de laplage</summary>
    private const float c_tiling = 0.05f;

    /// <summary>Nombre de raycast par arr�te � la premi�re tentative </summary>
    private const int c_raycastPerEdge = 7;

    /// <summary>Epaisseur de la plage intiale</summary>
    private const float c_thicknessInit = 0.2f;

    /// <summar>valeur augmentation/diminution �paisseur quand on appuie sur les boutons + et -</summar></summary>
    private const float c_thicknessDelta = 0.01f;

    private string m_thicknessString;
     
    /// <summary>�paisseur de la plage</summary>
    private float m_thickness = c_thicknessInit;

    /// <summary>GameObject repr�sentant un vertex</summary>
    public GameObject m_vertexObject = null;

    /// <summary>GameObject repr�sentant une ar�te, doit �tre constitu� au moins d'un collider adapt� pour le raycast</summary>
    public GameObject m_edgeObject = null;
    
    /// <summary>Skin de la gui du configurateur de terrace</summary>
    private GUISkin m_skin;


    private bool m_disableCollidersAfterCreatingMesh=false;

    //ui variables
    private bool helpPanelShown = false;
    private bool m_showGUI = false;
    private float hideGUITimer=0;
    private Rect m_uiArea;
    private List<Rect> m_uiRects = new List<Rect>();
    private bool m_firstOnGUI = true;
    private bool clickedOnUI=false;
	private bool _justeSelected = false; //indique si le clic souris vient juste de s�lectionner un point, au relachement du clic passe � false
	
	
	//couleur de selection des spheres
	private Color SELECTED_COLOR = new Color(0.0f,1.0f,0.0f,1.0f);
	private Color UNSELECTED_COLOR = new Color(26.0f/255.0f,249.0f/255.0f,249.0f/255.0f,1.0f);
    #region Function_OS3D
    private int id = 0;

    public string GetFunctionName()
    {
        return "Sol";
    }
    public string GetFunctionParameterName()
    {
        return "Configurer";
    }

    public int GetFunctionId()
    {
        return id;
    }


    public void DoAction()
    {
        mainScene.GetComponent<GUIMenuConfiguration>().setVisibility(false);
        mainScene.GetComponent<GUIMenuInteraction>().setVisibility(false);
        mainScene.GetComponent<GUIMenuInteraction>().isConfiguring = false;
        
        Camera.main.GetComponent<ObjInteraction>().setSelected(null, true);
        Camera.main.GetComponent<ObjInteraction>().setActived(false);
        m_showGUI = true;
       // removeEdge(m_vertices.First.Value, m_vertices.Last.Value);
        enabled = true;
        enableColliders();
    }

    //  sauvegarde/chargement	
    public void save(BinaryWriter buf)
    {
        buf.Write(m_vertices.Count);
      //  buf.Write(m_edges.Count);
		Debug.Log("m_vertices.Count "+m_vertices.Count);
	//	Debug.Log("m_edges.Count "+m_edges.Count);
        foreach (PVertex v in m_vertices)
        {
            v.save(buf);
        }
    }

    public void load(BinaryReader buf)
    {

        if (m_plageMesh != null)
        {
            Destroy(m_plageMesh);
        }
		Reset(); 
		int vertexCount = buf.ReadInt32();
		m_vertexList_Original = new Vector3[vertexCount];
		
		//Debug.Log("m_vertices.Count "+m_vertices.Count);
		List<PVertex> vertices = new List<PVertex>();
        for (int i = 0; i < vertexCount; i++)
        {
			PVertex v1 = PVertex.load(buf,this);
			m_vertexList_Original[i]=new Vector3(v1.Position.x,0.0f,v1.Position.z);

        } 
		Init();
    }
	
	public void Init()
    {
		transform.localPosition=new Vector3(transform.localPosition.x, 0.0f, transform.localPosition.z);
		//Cr�ation rectangle initial
        //StartCoroutine(createRectangle(20, 8));
		
        if (m_plageMesh != null)
        {
            Destroy(m_plageMesh);
        }
		Reset();
        int vertexCount = m_vertexList_Original.Length;
        int edgeCount = vertexCount;
        for (int i = 0; i < vertexCount; i++)
        {
            m_vertices.AddLast(PVertex.load(m_vertexList_Original[i],this));
        }
        for (int i = 0; i < edgeCount; i++)
        {
            //PVertex[] v2 = PEdge.load(buf, m_vertices);
			int indexDebut=i;
			int indexFin=(i<edgeCount-1)?i+1:0;			
			
			PVertex v1 =	PVertex.vertexFromIndex(indexDebut,m_vertices);
			PVertex v2 =	PVertex.vertexFromIndex(indexFin,m_vertices);
			
			addEdge(v1, v2);
        }
        StartCoroutine(generatePolygons());      
    }


    //Set L'ui si besoin
    public void setUI(FunctionUI_OS3D ui)
    {
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}

    //similaire au Save/Load mais utilis� en interne d'un objet a un autre (swap)
    public ArrayList getConfig()
    {
        ArrayList al = new ArrayList();
        al.Add(m_vertices.Count);
        foreach (PVertex v in m_vertices)
        {
            v.save(al);
        }
        return al;
    }

    public void setConfig(ArrayList config)
    {
		if (m_plageMesh != null)
        {
            Destroy(m_plageMesh);
        }
		Reset();
        int vertexCount = (int)config[0];
        int index=1;
		m_vertexList_Original = new Vector3[vertexCount];
        /*for (int i = 0; i < vertexCount; i++)
        {
            m_vertices.AddLast(PVertex.load(config,index,this));
            index+=3;
        }   */ 
		for (int i = 0; i < vertexCount; i++)
        {
			PVertex v1 = PVertex.load(config,index,this);
			m_vertexList_Original[i]=new Vector3(v1.Position.x,0.0f,v1.Position.z);
            index+=3;
        } 
		Init();
    }
	
	private HelpPanel _helpPanel;




    #endregion	


    /// <summary>
    /// Active ou d�sactive les colliders des Vertex et Edges de la plage. 
    /// </summary>
    public void enableColliders(bool enable=true)
    {
        foreach (PEdge e in m_edges)
        {
            for (int i = 0; i < e.gameObject.transform.childCount; i++)
            {
                try
                {
                    e.gameObject.transform.GetChild(i).GetComponent<Collider>().enabled = enable;
                }
                catch (UnityException ue)
                {
                }
            }
        }
        foreach (PVertex v in m_vertices)
        {
            v.gameObject.GetComponent<Collider>().enabled = enable;
        }
    }

    /// <summary>
    ///Sauvegarde l'�tat des ar�tes et des points.
    ///</summary>
    private void saveEdgesAndVertices()
    {
        m_verticesSaved = new LinkedList<PVertex>();
        m_edgesSaved = new List<PEdge>();
        Debug.Log("Vertices Saved :" + m_vertices.Count);
        foreach (PVertex v in m_vertices)
        {
            m_verticesSaved.AddLast(v);
        }
        foreach (PEdge e in m_edges)
        {
            PEdge newE = new PEdge();
            newE.v1 = e.v1;
            newE.v2 = e.v2;
            m_edgesSaved.Add(newE);
        }
    }


 
    /// <summary>
    /// Restaure l'�tat des Vertices et ar�tes sauvegard�
    /// </summary>
    private void loadEdgesAndVertices(bool rendererEnabled=true)
    {
        m_vertexSelected = null;
       // Debug.Log("loading Edges");

        foreach (PEdge e in m_edges)
        {
            if (e.gameObject != null)
            {
                Destroy(e.gameObject);
            }
        }
        m_edges = new List<PEdge>();
         m_vertices=new LinkedList<PVertex>();
        foreach (PVertex v in m_verticesSaved)
        {
            v.linked = new List<PVertex>();
            m_vertices.AddLast(v);
            if(!rendererEnabled)
                v.gameObject.GetComponent<Renderer>().enabled=false;
        }
        Debug.Log("Vertices Loaded :" + m_vertices.Count);
        foreach (PEdge e in m_edgesSaved)
        {
            if (e.v1 == m_vertices.Last.Value && e.v2 == m_vertices.First.Value || e.v2 == m_vertices.Last.Value && e.v1 == m_vertices.First.Value)
            {
                if (rendererEnabled)
                    addEdge(e.v1, e.v2, true);
                else
                    addEdge(e.v1, e.v2, true, false);
            }
            else
            {
                if (rendererEnabled)
                    addEdge(e.v1, e.v2);
                else
                    addEdge(e.v1, e.v2, false);
            }
        } 
    }


    /// <summary>
    /// Ajoute un vertex (avec un mesh repr�sentatif) � la position indiqu�e et cr�e une arr�te avec le vertex pr�c�dent
    /// </summary>
    /// <param name="position">position du vertex � ajouter</param>
    private void addVertex(Vector3 position, bool localPosition=false)
    {
        PVertex v = new PVertex();
        v.gameObject = createVertexObject(position);
        if(m_vertices.Count>0)
            addEdge(m_vertices.Last.Value, v);
        m_vertices.AddLast(v);
    }


    /// <summary>
    /// change la position d'un vertex et met � jour les gameObject des edges voisins
    /// </summary>
    /// <param name="v">vertex � bouger</param>
    /// <param name="position">nouvelle position</param>
    private void moveVertex(PVertex v, Vector3 position)
    {
        v.Position = position;
        int counter = 0;
        //v.gameObject.transform.position = position;
        foreach (PEdge e in m_edges)
        {
            if (e.v1 == v || e.v2 == v)
            {
                e.gameObject.transform.position = (e.v1.Position + e.v2.Position) / 2;
                e.gameObject.transform.LookAt(e.v1.Position);
                e.gameObject.transform.parent = null;
                e.gameObject.transform.localScale = new Vector3(e.gameObject.transform.localScale.x, e.gameObject.transform.localScale.y, Vector3.Distance(e.v1.Position, e.v2.Position));
                e.gameObject.transform.parent = this.gameObject.transform;
                counter++;
            }
            if (counter > 1)
            {
                break;
            }
        }
    }


  
   /// <summary>
    /// Ajoute une ar�te entre les vertex v1 et v2.
    /// � l'ar�te est associ�e un gameobject constitu� d'une mesh repr�sentative et d'une mesh tr�s haute et fine servant aux collisions
   /// </summary>
    private void addEdge(PVertex v1, PVertex v2,bool lastEdge=false,bool rendererEnabled=true)
    {
        
        PEdge e = new PEdge();
        e.v1 = v1;
        e.v2 = v2;
        
        v1.linked.Add(v2);
        v2.linked.Add(v1);
        e.gameObject = createEdgeObject();
        /*if (lastEdge)
        {
            e.gameObject.transform.FindChild("line").renderer.material = m_matLastEdge;
        }*/
        e.gameObject.transform.position = (e.v1.Position+ e.v2.Position) / 2;
        e.gameObject.transform.LookAt(v1.Position);
        e.gameObject.transform.parent = null;
        e.gameObject.transform.localScale = new Vector3( e.gameObject.transform.localScale.x, e.gameObject.transform.localScale.y, Vector3.Distance(e.v1.Position, e.v2.Position));
        e.gameObject.transform.parent = this.gameObject.transform;
        m_edges.Add(e);
        if (!rendererEnabled)
        {
            e.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
        }
       
    }

    /// <summary>
    /// Supprime l'ar�te ayant pour extr�mit�s les vertices pass�s en param�tre
    /// </summary>
    private void removeEdge(PVertex v0, PVertex v1)
    {
        deleteEar(v0, v1, null, false, false);
    }


    /// <summary>
    /// Ajoute un Vertex v0 entre v1 et v2 dans la liste et ajoute les ar�tes [v1 v0] et [v2 v0]
    /// v0 sera positionn� selon le param�tre position
    /// </summary>
    void addVertexOnEdge(PVertex v1, PVertex v2,Vector3 position)
    {
        //Si c'est l'ar�tre entre le premier et le dernier vertex ( la rouge), on fait un simple ajout
        if ((v1 == m_vertices.First.Value && v2 == m_vertices.Last.Value) || (v2 == m_vertices.First.Value && v1 == m_vertices.Last.Value))
        {
            removeEdge(m_vertices.First.Value, m_vertices.Last.Value);
            addVertex(position);
            addEdge(m_vertices.First.Value, m_vertices.Last.Value, true);
            m_vertexSelected = null;
            m_edgeSelected = null;
			
			//on remet le nouveau en vert
			PVertex selected = m_vertices.Last.Value;
			selected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",SELECTED_COLOR);
        }
        else
        { //sinon on cr�e un nouveau vertex, ins�r� au bon endroit dans la liste
            PVertex vNew = new PVertex();
            vNew.gameObject = createVertexObject(position);
            LinkedListNode<PVertex> n1 = m_vertices.Find(v1);
            LinkedListNode<PVertex> n2 = m_vertices.Find(v2);
            if (n1.Next == n2)
            {
                m_vertices.AddAfter(n1, vNew);
            }
            else
                m_vertices.AddAfter(n2, vNew);
            addEdge(vNew, v1);
            addEdge(vNew, v2);
            removeEdge(v1, v2);
            m_vertexSelected = vNew;
            m_edgeSelected = null;
			
			//on remet le nouveau en vert
			m_vertexSelected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",SELECTED_COLOR);
        }
		
    }

    void addVertexOnEdge(PEdge e, Vector3 position){
                        PVertex v1 = e.v1;
                        PVertex v2 = e.v2;
                        addVertexOnEdge(v1, v2, position);
    }

    /// <summary>
    /// Suppression d'une oreille (on enl�ve les supprime [v1 v0] et [v2 v0] et le sommet v0)
    /// </summary>
    /// <param name="delMesh">Supprime le gameObject de v0</param>
    /// <param name="fromList">Supprime d�finitivement v0 de m_list></param>
    private void deleteEar(PVertex v0,PVertex v1,PVertex v2,bool delMesh=false,bool fromList=true){
        List<PEdge> toremove = new List<PEdge>();
        foreach(PEdge e in m_edges){
            if((e.v1==v0&&e.v2==v1) ||(e.v1==v1&&e.v2==v0)
                ||(e.v1==v0&&e.v2==v2) ||(e.v1==v2&&e.v2==v0)){
                Destroy(e.gameObject);
                toremove.Add(e);
            }
        }
        foreach (PEdge r in toremove)
        {
            m_edges.Remove(r);
        }
        if (v1 != null)
        {
            v1.linked.Remove(v0);
            v0.linked.Remove(v1);
        }
        if(v2!=null){
             v2.linked.Remove(v0);
            v0.linked.Remove(v1);
        }   
        if (delMesh)
        {
            Destroy(v0.gameObject);
        }
        if (fromList)
            m_vertices.Remove(v0);
    }

    /// <summary>
    /// retourne vrai si le vertex v0 est le sommet d'une oreille
    /// </summary>
    /// <param name="nbRays">Nombre de raycast � faire sur l'arr�te oppos�e � v0</param>
    private bool isEar(PVertex v0,int nbRays)
    {
		if(v0.linked.Count<2)
			return false;
        PVertex v1 = v0.linked[0];
        PVertex v2 = v0.linked[1];
        bool isEar = true;
        RaycastHit[] hits1;
        for (int i = 0; i < nbRays; i++)
        {
            //On fait des raycast le long de l'arr�te pour voir si elle est bien � l'int�rieur du polygone 
            Vector3 mid12 = ((i + 1) * v1.Position + (nbRays - i) * v2.Position) / (nbRays + 1);     
			//Vector3 v1Pos = v1.Position - transform.localPosition;
			//Vector3 v2Pos = v2.Position - transform.localPosition;
            //Vector3 mid12 = ((i + 1) * v1Pos + (nbRays - i) * v2Pos) / (nbRays + 1);     
			//Debug.DrawRay(mid12, Vector3.forward*200,Color.red,5f);
			//Debug.DrawRay(v2.Position, (v2.Position-v1.Position).normalized* -200,UNSELECTED_COLOR,5f);
            hits1 = Physics.RaycastAll(mid12, Vector3.forward, 200, 1 << 24/*LayerMask.NameToLayer("polyplage")*/);
            if ((hits1.Length ) % 2 == 0 || (hits1.Length )==0)
            {
				/*if ((hits1.Length )==0)
				{
					Debug.DrawRay(mid12, Vector3.forward*200*(i+1)/10,Color.yellow,5f);
                	isEar= false;
                	break;
				}*/
			//	listGizmos.Clear();
			//	drawGizmo = true;
			//	Debug.Log ("Alert i = "+i+" hits1.Length "+hits1.Length);
			/*	Debug.DrawRay(mid12, Vector3.forward*200*(i+1)/10,SELECTED_COLOR,5f);
				foreach (RaycastHit raycasthit in hits1)
				{
					listGizmos.Add(raycasthit.point);
					//listColliderCenter.Add(((BoxCollider) raycasthit.collider).bounds.center);
					//listColliderExtents.Add(((BoxCollider) raycasthit.collider).bounds.extents);
					
		            Renderer renderer = raycasthit.collider.renderer;
		            if (renderer) {
		                renderer.material.shader = Shader.Find("Transparent/Diffuse");
						Color color =  renderer.material.color;
						color.a  = 0.3F;
		                renderer.material.color=color;
		            }
					
				}*/
                isEar= false;
		//		Debug.Break();
		//		drawGizmo = false;
                break;
            }   
			/*else if ((hits1.Length )==0)
            {
				Debug.DrawRay(mid12, Vector3.forward*200*(i+1)/10,Color.yellow,5f);
                isEar= false;
                break;
            }*/
			/*else{
				Debug.DrawRay(mid12, Vector3.forward*200*(i+1)/10,Color.red,5f);
			}*/
			/*hits1 = Physics.RaycastAll(v2.Position, (v2.Position-v1.Position).normalized, -200, 1 << LayerMask.NameToLayer("polyplage"));
            if ((hits1.Length ) % 2 == 0 || (hits1.Length )==0)
            {
                isEar= false;
                break;
            }*/
        }
        return isEar;
    }
/*	private bool drawGizmo = false;
	List<Vector3> listGizmos = new List<Vector3>();
	//List<Vector3> listColliderCenter = new List<Vector3>();
	//List<Vector3> listColliderExtents = new List<Vector3>();
	void OnDrawGizmos() {
		Color drawcolor = new Color(0.5f,0.5f,0.5f,0.2f);
		if(drawGizmo)
		{
			//int i=0;
			foreach (Vector3 raycasthit in listGizmos)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(raycasthit, 0.1f);
			//Gizmos.color =drawcolor;
			//	Gizmos.DrawCube(listColliderCenter[i], listColliderExtents[i]);
			//	i++;
			}
			
		}
    }*/
	
    /// <summary>
    /// Retourne la premi�re oreille trouv�e
    /// </summary>
    /// <param name="nbRays">Nombre de raycasts par ar�te</param>
    private PVertex findEar(int nbRays){
        foreach (PVertex v in m_vertices)
        {
            if (isEar(v, nbRays))
            {
                return v;
            }
        }
        return null;
    }
 


    /// <summary>
    /// Ajoute le triangle de l'oreille � la liste de triangles du futur mesh de la plage
    /// </summary>
    private void saveEar(PVertex v0, PVertex v1, PVertex v2)
    {
        if (Vector3.Cross(v1.Position-v0.Position,v2.Position-v0.Position).y>0)
        {
            m_trisList.Add(v0.getIndex(m_verticesSaved));
            m_trisList.Add(v1.getIndex(m_verticesSaved));
            m_trisList.Add(v2.getIndex(m_verticesSaved));
        }
        else
        {
            m_trisList.Add(v0.getIndex(m_verticesSaved));
            m_trisList.Add(v2.getIndex(m_verticesSaved));
            m_trisList.Add(v1.getIndex(m_verticesSaved));
        }
    }



    /// <summary>
    /// Cr�e la mesh � partir de m_vertices et de trisArray
    /// loadEdgesAndVertices() doit �tre appell� avant createMesh()
    /// </summary>
    /// <returns>le GameObject de la mesh</returns>
    private GameObject createMesh(){
        int[] trisArray;
        int nbVert = m_vertices.Count;
        Vector3[] vertexArray = new Vector3[2*nbVert];

        //formation de la liste de vecteurs repr�sentant les vertices
        foreach (PVertex v in m_vertices)
        {
            m_vertexList.AddLast(v.Position);
        }

        int i=0;
        //Cr�ation du tableau final contenant la position des Vertices avec 2 fois plus de vertices pour donner
        //une �paisseur � la terrace. Aussi, ajout des triangles sur les c�t�  de la terrace.
        foreach(Vector3 v in m_vertexList)
        {
           vertexArray[i]=v+m_thickness*Vector3.up;
           vertexArray[nbVert+i] = v;
           if (i > 0)
           {
               m_trisList.Add(i);
               m_trisList.Add(i - 1 + nbVert); ;
               m_trisList.Add(i - 1);
              
               m_trisList.Add(i);
               m_trisList.Add(i + nbVert); ;
               m_trisList.Add(i - 1 + nbVert); ;
               
           }
           i++;
         }
        m_trisList.Add(0);
        m_trisList.Add(2*nbVert-1); ;
        m_trisList.Add(nbVert-1);

        m_trisList.Add(0);
        m_trisList.Add(nbVert); ;
        m_trisList.Add(2 * nbVert - 1); ;


         Vector2[] uvs = new Vector2[vertexArray.Length];
         i = 0;

        //uvs, planar mapping
         while (i < nbVert)
         {
             uvs[i] = new Vector2(vertexArray[i].x*0.1f, vertexArray[i].z*0.1f);
             uvs[i + nbVert] = uvs[i];
             i++;
         }

        //Cr�ation de la mesh en elle m�me
          trisArray = m_trisList.ToArray();
          GameObject plage = new GameObject("plageObject", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
          MeshFilter mf = plage.GetComponent<MeshFilter>();
          Mesh mesh = new Mesh();
          mf.mesh = mesh;
          mesh.vertices = vertexArray;
          mesh.triangles = trisArray;
          mesh.RecalculateNormals();
          mesh.RecalculateBounds();
          mesh.uv = uvs;
          m_trisList = new List<int>();
          m_vertexList = new LinkedList<Vector3>();
          //mis � jour de la texture de la plage si n�c�ssaire

         plage.GetComponent<Renderer>().material = m_curMatRenderer.material;
         m_needColliderUpdate = 1;
          return plage;
    }







    /// <summary>
    /// Triangulation du polygone pour cr�er la plage
    /// </summary>
    private IEnumerator generatePolygons()
    {
		//on remet l'ancien vertex en bleu
		if (m_vertexSelected != null)
		{
			m_vertexSelected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",UNSELECTED_COLOR);
		}
		else if (m_vertices.Count > 0)
		{
			PVertex selected = m_vertices.Last.Value;
			selected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",UNSELECTED_COLOR);
		}
		
		//Vector3 backupPosition = new Vector3(transform.localPosition.x,transform.localPosition.y,transform.localPosition.z);
		//transform.localPosition = new Vector3(0.0f,0.0f,0.0f);
		Quaternion backupRotation = new Quaternion(transform.localRotation.x,transform.localRotation.y,transform.localRotation.z,transform.localRotation.w);
		transform.localRotation = Quaternion.identity;
		yield return new WaitForFixedUpdate();
		//listColliderCenter.Clear();
		//listColliderExtents.Clear();
	//	listGizmos.Clear();
       	mainScene.GetComponent<PleaseWaitUI>().SetLoadingMode(true);
        bool fail = false;
        int nbRays = c_raycastPerEdge;
        //ajoute une ar�te entre le premier et le dernier vertex
        //addEdge(m_vertices.Last.Value, m_vertices.First.Value);
        //Sauvegarde 
        saveEdgesAndVertices();
        hideVertices();
        //La boucle s'arr�te lorsqu'il reste un seul triangle
        while (m_vertices.Count > 3)
        {
            //On attend la mise � jour du moteur physique
            yield return new WaitForFixedUpdate();
            //On trouve une oreille
            PVertex v = findEar(nbRays);
            if (v != null)
            {
                PVertex v1 = v.linked[0];
                PVertex v2 = v.linked[1];
                //sauvegarde oreille dans kes triangles du mesh
                saveEar(v, v1, v2);
                //suppression de l'oreille
                deleteEar(v, v1, v2);
                //ajout d'une ar�te � sa base
                addEdge(v1, v2,false,false);
            }
            else //si on ne trouve pas d'oreille
            {
                //si on a d�pass� 50 raycasts par ar�te : fail
                if (nbRays > 50)
                {
                    Debug.Log("Failed to generate polygons");
                    //On charge la configuration initiale
                    loadEdgesAndVertices();
                    showVertices();
                    //On supprime l'ar�te entre le premier et le dernier vertex
                   // removeEdge(m_vertices.First.Value, m_vertices.Last.Value);
                    fail = true;
                    //Retour au mode �dition
                    if (mainScene.GetComponent<GUIMenuConfiguration>().enabled)
                    {
                        mainScene.GetComponent<GUIMenuConfiguration>().enabled = false;
                        mainScene.GetComponent<GUIMenuConfiguration>().setVisibility(false);
                    }
                    mainScene.GetComponent<GUIMenuInteraction>().isConfiguring = false;
                    Camera.main.GetComponent<ObjInteraction>().setSelected(null);
                    mainScene.GetComponent<GUIMenuLeft>().canDisplay(false);
                    mainScene.GetComponent<GUIMenuRight>().canDisplay(false);
                    mainScene.GetComponent<GUIMenuMain>().enabled = false;
                    //Camera.mainCamera.GetComponent<ObjInteraction>().setActived(false);
//                    mainScene.GetComponent<GUIMenuInteraction>().enabled = false;
                    m_editingMode = true;
                    m_thicknessMode = false;
                    showGUIForTime(2f);
				//	transform.localPosition = backupPosition;
					transform.localRotation = backupRotation;
					
					//on met le dernier point en vert
					if (m_vertexSelected != null)
					{
						//PVertex selected = m_vertexSelected;
						m_vertexSelected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",SELECTED_COLOR);
					}
					else if (m_vertices.Count > 0)
					{
						PVertex selected = m_vertices.Last.Value;
						selected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",SELECTED_COLOR);
					}
					
                    break;
                }
                else
                {
                    m_trisList = new List<int>();
                    //On charge la configuration initiale
                    loadEdgesAndVertices(false);
                    //On augmente le nombre de raycats pour une plus grande pr�cision
                    nbRays += 4;
                    Debug.Log("v=null, loadingEdge and nbRays=" + nbRays);
                }
            }
        }
        //Si la triangulation a r�ussi
        if (!fail)
        {
			try {
	            //Dernier triangle
	            saveEar(m_vertices.First.Value, m_vertices.First.Value.linked[0], m_vertices.First.Value.linked[1]);
			}
			catch (System.NullReferenceException e)
			{
				Debug.LogError("Function_Terrace generatePolygons() :"+e.ToString());
				return true;
			}
			/*if(m_vertices.First!=null)
			{
				saveEar(m_vertices.First.Value, m_vertices.First.Value.linked[0], m_vertices.First.Value.linked[1]);
			}*/
            //On charge la configuration initiale
            loadEdgesAndVertices();
            hideVertices();
            //Cr�ation de la mesh
            Destroy(m_plageMesh);
            m_plageMesh = createMesh();
            m_plageMesh.transform.parent = this.gameObject.transform;
            m_editingMode = false;
            /*
            mainScene.GetComponent<GUIMenuConfiguration>().setVisibility(false);
            mainScene.GetComponent<GUIMenuInteraction>().isConfiguring = false;
             * */
            if (m_disableCollidersAfterCreatingMesh)
            {
                enableColliders(false);
                this.enabled = false;
            }
        }
        m_disableCollidersAfterCreatingMesh = false;
        mainScene.GetComponent<PleaseWaitUI>().SetLoadingMode(false);
		//transform.localPosition = backupPosition;		
		transform.localRotation = backupRotation;
    }

    /// <summary>
    /// Change l'�paisseur de la plage 
    /// </summary>
    /// <param name="thickness">nouvelle �paisseur</param>
    /// <returns>true si l'�paisseur en param�tre>0 et m_plageMesh non null</returns>
    public bool setPlageThickness(float thickness)
    {
        Debug.Log("setting plage thickness");
        if (thickness >= 0 && m_plageMesh != null)
        {
            m_thickness = thickness;
            Mesh mesh = m_plageMesh.transform.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = new Vector3[mesh.vertexCount];
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                if (i < mesh.vertexCount / 2)
                {
                    vertices[i] = new Vector3(mesh.vertices[i].x, m_thickness, mesh.vertices[i].z);
                }
                else
                {
                    vertices[i] = mesh.vertices[i];
                }
            }
            mesh.vertices = vertices;
            return true;
        }
        else
        {
            return false;
        }
    }




    /// <summary>
    /// S'il existe une ar�te ayant pour GameObject celui pass� en param�tre alors m_edgeSelected prend sa valeur.
    /// Sinon m_edgeSelected aura pour nouvelle valeur null.
    /// </summary>
    public void selectEdgeFromGameObject(GameObject go)
    {
        m_edgeSelected = null;
        foreach (PEdge e in m_edges)
        {
            if (e.gameObject.transform.FindChild("line").gameObject == go)
            {
                m_edgeSelected = e;
                break;
            }
        }
    }

    /// <summary>
    /// S'il existe un vertex ayant pour GameObject celui pass� en param�tre alors m_vertexSelected prend sa valeur.
    /// </summary>
    public void selectVertexFromGameObject(GameObject go)
    {
        Debug.Log("gameobject selected :" + go.ToString());
        foreach (PVertex v in m_vertices)
        {
            if (v.gameObject == go)
            {
				//on remet l'ancien vertex en bleu
				if (m_vertexSelected != null)
				{
					m_vertexSelected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",UNSELECTED_COLOR);
				}
				else if (m_vertices.Count > 0)
				{
					PVertex selected = m_vertices.Last.Value;
					selected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",UNSELECTED_COLOR);
				}
                m_vertexSelected = v;
				//on met le nouveau en vert
				m_vertexSelected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",SELECTED_COLOR);
                break;
            }
        }
    }

    /// <summary>
    /// Retourne le gameObject d'une nouvelle mesh repr�sentant une ar�te (+collider)
    /// </summary>
    /// <returns></returns>
    private GameObject createEdgeObject(){
		GameObject ego = (GameObject)Instantiate(m_edgeObject);
        ego.transform.localScale *= 3;
        ego.transform.parent = this.gameObject.transform;
		Transform line =ego.transform.FindChild("line");
		if(line!=null)
		{
			BoxCollider box = line.gameObject.GetComponent<BoxCollider>();
			if(box!=null)
			{
				box.extents = new Vector3(box.extents.x*5.0f,box.extents.y*5.0f,box.extents.z);
			}
		}
		ego.SetActive(true);
		return ego;
    }

    /// <summary>
    /// etourne le gameObject d'une nouvelle mesh repr�sentant un vertex
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private GameObject createVertexObject(Vector3 position)
    {
        if (m_vertexObject == null)
        {
            GameObject vgo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vgo.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            vgo.transform.position= position;
            m_vertexObject = vgo;
            vgo.transform.parent = this.gameObject.transform;
            return vgo;
        }
        else
        {
            Debug.Log("creating vertex at :" + position.ToString());
			Vector3 pos = new Vector3(
				position.x/*+this.gameObject.transform.position.x*/,
				0.0f,
				position.z/*+this.gameObject.transform.position.z*/);
            GameObject vgo = (GameObject)Instantiate(m_vertexObject, pos, Quaternion.identity);
           // vgo.transform.position = position;
           // vgo.transform.localScale *= 5;
            vgo.transform.localScale *= (1*this.gameObject.transform.localScale.x);
            vgo.transform.parent = this.gameObject.transform;
            vgo.SetActive(true);
            
            return vgo;
        }
    }

    /// <summary>
    /// Cr�e une plage de forme rectangulaire, g�n�re le polygone � la fin
    /// Les WaitForFixedUpdate()sont importants pour une bonne mise � jour des colliders
    /// </summary>
    /// <param name="width">largeur de la plage</param>
    /// <param name="length">longueur de la plage</param>
    public IEnumerator createRectangle(float width, float length)
    {
        yield return new WaitForFixedUpdate();
        Ray rayHit1 = new Ray(new Vector3(-width / 2, 50, -length / 2), Vector3.down);
        Ray rayHit2 = new Ray(new Vector3(width / 2, 50, -length / 2), Vector3.down);
        Ray rayHit3 = new Ray(new Vector3(width / 2, 50, length / 2), Vector3.down);
        Ray rayHit4 = new Ray(new Vector3(-width / 2, 50, length / 2), Vector3.down);

        RaycastHit result;
        Physics.Raycast(rayHit1, out result, 55, 1 << LayerMask.NameToLayer("grid"));
        addVertex(new Vector3(-width / 2, result.point.y, -length / 2));
        yield return new WaitForFixedUpdate();
        Physics.Raycast(rayHit2, out result, 55, 1 << LayerMask.NameToLayer("grid"));
        addVertex(new Vector3(width / 2, result.point.y, -length / 2));
        yield return new WaitForFixedUpdate();
        Physics.Raycast(rayHit3, out result, 55, 1 << LayerMask.NameToLayer("grid"));
        addVertex(new Vector3(width / 2, result.point.y, length / 2));
        yield return new WaitForFixedUpdate();
        Physics.Raycast(rayHit4, out result, 55, 1 << LayerMask.NameToLayer("grid"));
        addVertex(new Vector3(-width / 2,  result.point.y ,length / 2));
        yield return new WaitForSeconds(0.5f);
        addEdge(m_vertices.Last.Value, m_vertices.First.Value,true);
        m_disableCollidersAfterCreatingMesh = true;
        StartCoroutine(generatePolygons());
    }
	
	private void Reset()
	{
		m_edges.Clear();
		m_vertices.Clear();
		m_verticesSaved.Clear();
		m_edgesSaved.Clear();
		m_trisList.Clear();		
		foreach(Transform transformChild in gameObject.transform)
		{
			if(transformChild.name.Contains("Clone"))
				Destroy(transformChild.gameObject);
		}
	//	transform.
	}

    void Awake(){
        mainScene = GameObject.Find("MainScene");
        GameObject goMaterial= new GameObject("sol", typeof(MeshRenderer), typeof(MeshFilter));
        m_curMatRenderer = goMaterial.GetComponent<Renderer>();
        m_curMatRenderer.material.SetTextureScale("_MainTex", new Vector2(c_tiling, c_tiling));
        goMaterial.transform.parent = this.gameObject.transform;
		_helpPanel = mainScene.GetComponent<HelpPanel>();
    }

        

	// Use this for initialization
	void Start () {
        
        //R�cup�ration texture du dernier c�t�
  //      if(m_matLastEdge==null)
  //          m_matLastEdge = (Material)Resources.Load("materials/red",typeof(Material));
        m_uiArea = new Rect(Screen.width - 280, 0, 280, Screen.height);
        m_skin = (GUISkin)Resources.Load("skins/PergolaSkin");
		Init();
        //Cr�ation rectangle initial
      //  StartCoroutine(createRectangle(20, 8));
        m_editingMode = false;
	}

    public void OnGUI()
    {
        if (m_showGUI)
        {
            GUISkin backup = GUI.skin;
            GUI.skin = m_skin;

            GUILayout.BeginArea(m_uiArea);
            GUILayout.FlexibleSpace();
            GUILayout.Box("", "UP", GUILayout.Width(280), GUILayout.Height(150));
            GUILayout.BeginVertical("MID");

            //m_uiRects contiens les rectangles des �l�ments du menu afin de savoir si la souris est dessus ou pas
            m_uiRects = new List<Rect>();

            //Bouton Material
            GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(TextManager.GetText("GUIMenuConfiguration.Materials"), "Menu", GUILayout.Height(50), GUILayout.Width(280)))
            {
                //Parfois la plage deviens vert s�lectionn�e quand on change sons material, ce serait mieux pas...
                mainScene.GetComponent<GUIMenuInteraction>().isConfiguring = true;
                Camera.main.GetComponent<ObjInteraction>().setSelected(gameObject);
                Camera.main.GetComponent<ObjInteraction>().setActived(false);

                mainScene.GetComponent<GUIMenuConfiguration>().enabled = true;
                mainScene.GetComponent<GUIMenuConfiguration>().setVisibility(true);
                mainScene.GetComponent<GUIMenuConfiguration>().OpenMaterialTab();

                mainScene.GetComponent<GUIMenuLeft>().canDisplay(false);
                mainScene.GetComponent<GUIMenuRight>().canDisplay(false);
                hideGUI();
                m_editingMode = false;
                hideVertices();

                //G�n�ration de la mesh si ce n'a pas �t� fait
                if (m_plageMesh == null)
                {
                    //Les colliders sont d�sactiver apr�s la cr�ation de la mesh pour pas interf�rer avec le fonctionnement de oneshot
                    m_disableCollidersAfterCreatingMesh = true;
                    StartCoroutine(generatePolygons());
                }
                m_edgeSelected = null;
            }
            //Ajout � la liste de rectangle celui correspondant au bouton
            m_uiRects.Add(GUILayoutUtility.GetLastRect());
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            //Bouton configurer => quand on clique dessus, �a masque la gui pour revenir au menu pr�c�dent
            GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
            GUILayout.FlexibleSpace();
            if (!GUILayout.Toggle(true, TextManager.GetText("Terrasse.Configurer"), "Menu", GUILayout.Height(50), GUILayout.Width(280)))
            {
                if (m_plageMesh == null)
                {
                    m_disableCollidersAfterCreatingMesh = true;
                    StartCoroutine(generatePolygons());
                }
                exitConfigurator(true);
                hideGUI();
            }
            m_uiRects.Add(GUILayoutUtility.GetLastRect());
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            //editing_mode est activ� quand l'utilisateur �dite le contours
            if (m_editingMode)
            {
                //Bouton pour quitter l'editing mode et g�n�rer la mesh
                GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
                GUILayout.FlexibleSpace();
                if (!GUILayout.Toggle(true, TextManager.GetText("Terrasse.Edit"), "Menu", GUILayout.Height(50), GUILayout.Width(280)))
                {
                    m_editingMode = false;
                    hideVertices();
                    if(m_plageMesh==null)
                        StartCoroutine(generatePolygons());
                    m_edgeSelected = null;
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                m_uiRects.Add(GUILayoutUtility.GetLastRect());

                //Bouton pour supprimer un Vertex, uniquement disponible s'il y a plus de 3 Vertices
                if (m_vertices.Count > 3)
                {
                    GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(TextManager.GetText("Terrasse.Delete"), "Menu", GUILayout.Height(50), GUILayout.Width(280)))
                    {
                        Debug.Log("selected : " + m_vertexSelected);
                        PVertex todel = null;

                        //Si un Vertex est s�lection�e, on le suprime
                        if (m_vertexSelected != null)
                        {
                            todel = m_vertexSelected;
                        }
                         //Sinon on suprime le dernier vertex de la liste
                        else if (m_vertices.Count > 0)
                        {
                            todel = m_vertices.Last.Value;
                        }
                        Debug.Log("Suppression points :" + todel.ToString());
                        if (todel != null)
                        {
                            if (todel.linked.Count > 1)
                            {                         
                                if (todel == m_vertices.First.Value || todel == m_vertices.Last.Value)
                                    addEdge(todel.linked[0], todel.linked[1], true);
                                else
                                    addEdge(todel.linked[0], todel.linked[1]);
                                deleteEar(todel, todel.linked[0], todel.linked[1], true);
                            }
                            else if (todel.linked.Count > 0)
                                deleteEar(todel, todel.linked[0], null, true);
                            else
                                deleteEar(todel, null, null, true);
                        }
                        m_vertexSelected = null;
						//on remet le premier en vert
						if (m_vertices.Count > 0)
						{
							PVertex selected = m_vertices.Last.Value;
							selected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",SELECTED_COLOR);
						}
                    }
                    m_uiRects.Add(GUILayoutUtility.GetLastRect());
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(TextManager.GetText("Terrasse.Generate"), "Menu", GUILayout.Height(50), GUILayout.Width(280)))
                {
                    Debug.Log("genrere terasse clicked");
                    m_editingMode = false;
                    hideVertices();
                    StartCoroutine(generatePolygons());
                    m_edgeSelected = null;
                }
                m_uiRects.Add(GUILayoutUtility.GetLastRect());
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

            }
            else
            {

                if (m_thicknessString == null)
                {
                    m_thicknessString = (Mathf.Ceil(m_thickness * 100) / 100).ToString();
                }

                GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(false, TextManager.GetText("Terrasse.Edit"), "Menu", GUILayout.Height(50), GUILayout.Width(280)))
                {
                    Destroy(m_plageMesh);
                    m_editingMode = true;
                    hideGUITimer = Time.time;
                    m_thicknessMode = false;
                    showVertices();
					
					//on met le dernier point en vert
					if (m_vertexSelected != null)
					{
						//PVertex selected = m_vertexSelected;
						m_vertexSelected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",SELECTED_COLOR);
					}
					else if (m_vertices.Count > 0)
					{
						PVertex selected = m_vertices.Last.Value;
						selected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",SELECTED_COLOR);
					}
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

            }

//Epaisseur
          /*  bool oldthickMode = m_thicknessMode;
            GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
            GUILayout.FlexibleSpace();
            m_thicknessMode = GUILayout.Toggle(m_thicknessMode, "R�glage �paisseur", "Menu", GUILayout.Height(50), GUILayout.Width(280));
            m_uiRects.Add(GUILayoutUtility.GetLastRect());
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (m_thicknessMode && !oldthickMode)
            {
                m_editingMode = false;
                hideVertices();
                if(m_plageMesh==null)
                   StartCoroutine(generatePolygons());
                m_edgeSelected = null;
                // Camera.mainCamera.GetComponent<ObjInteraction>().setActived(true);
            }
            if (m_thicknessMode)
            {

                GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
                GUILayout.FlexibleSpace();
                if (GUILayout.RepeatButton("+", "btn+", GUILayout.Height(50), GUILayout.Width(50)))
                {
                    Debug.Log("Xplus pressed");
                    setPlageThickness(m_thickness + c_thicknessDelta);
                    m_thicknessString = (Mathf.Ceil(m_thickness * 100) / 100).ToString();
                }
                string tempText = GUILayout.TextField(m_thicknessString, GUILayout.Height(50), GUILayout.Width(50));
                bool minusPressed = false;
                if (GUILayout.RepeatButton("-", "btn-", GUILayout.Height(50), GUILayout.Width(50)))
                {
                    Debug.Log("Xminus pressed");
                    minusPressed = true;
                    if (m_thickness - c_thicknessDelta >= 0)
                    {
                        Debug.Log(">0");
                        setPlageThickness(m_thickness - c_thicknessDelta);
                        Debug.Log("Xminus pressed");
                        m_thicknessString = (Mathf.Ceil(m_thickness * 100) / 100).ToString();
                    }
                    else
                    {
                        setPlageThickness(0);
                        m_thicknessString = "0";
                    }

                }
                if (!tempText.Equals(m_thicknessString) && !minusPressed)
                {

                    try
                    {
                        Debug.Log("parsing newScalex");
                        float newThickness = float.Parse(tempText.Replace(',', '.'));
                        if (newThickness > 0)
                        {
                            Debug.Log("newscaleX>0");
                            setPlageThickness(newThickness);
                            m_thicknessString = tempText;
                        }
                        else
                        {

                            setPlageThickness(0);
                            if (newThickness == 0)
                            {
                                Debug.Log("newscaleX=0");
                                m_thicknessString = tempText;
                            }
                            else
                            {
                                Debug.Log("newscaleX=0");
                                m_thicknessString = "0";
                            }
                        }
                    }
                    catch (UnityException e)
                    {
                        Debug.Log("exception : " + e.ToString());
                        if (tempText.Equals(""))
                        {
                            Debug.Log("temptext=''");
                            m_thicknessString = "0";
                            setPlageThickness(0);
                        }
                        else
                        {
                            m_thicknessString = (Mathf.Ceil(m_thickness * 100) / 100).ToString();
                        }
                    }

                }
                //GUILayout.Label("Epaisseur", GUILayout.Width(60));
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }*/


            GUILayout.EndVertical();
            GUILayout.Box("", "DWN", GUILayout.Width(280), GUILayout.Height(150));//fade en bas

            GUILayout.FlexibleSpace();

            GUILayout.EndArea();


            GUI.skin = backup;



        }


    }

    void hideVertices()
    {
        foreach (PVertex v in m_vertices)
        {
            v.gameObject.GetComponent<Renderer>().enabled = false;
        }
        foreach (PEdge e in m_edges)
        {
            e.gameObject.transform.FindChild("line").gameObject.GetComponent<MeshRenderer>().enabled = false;
            ((MeshRenderer)(e.gameObject.GetComponentInChildren(typeof(MeshRenderer)))).enabled = false;
        }
    }

    void showVertices()
    {
        foreach (PVertex v in m_vertices)
        {
            v.gameObject.GetComponent<Renderer>().enabled = true;
        }
        foreach (PEdge e in m_edges)
        {
            ((MeshRenderer)(e.gameObject.GetComponentInChildren(typeof(MeshRenderer)))).enabled = true;
        }
    }


    void showGUIForTime(float time){
        Debug.Log("showhui!!!");
        m_showGUI = true;
        hideGUITimer = Time.time;
        UsefullEvents.ShowHelpPanel -= showGUI;
    }

    void showGUI()
    {
        Debug.Log("showGUI");
        m_showGUI = true;
        hideGUITimer = 0;
        UsefullEvents.ShowHelpPanel -= showGUI;
    }

    void hideGUI()
    {
        if(m_showGUI){
            //Debug.Log("hideGUI");
            //UsefullEvents.ShowHelpPanel += showGUI;
            mainScene.GetComponent<GUIMenuConfiguration>().ShowHelpPannel = true;
            UsefullEvents.ShowHelpPanel += showGUI;
            m_showGUI = false;
            m_uiRects = new List<Rect>();
            hideGUITimer = 0;
        }
    }


    /// <summary>
    /// m_clickedOnUI mis � true si la souris est sur le menu
    /// </summary>
    private void updateOnUI(){
        if (m_showGUI)
        {
            clickedOnUI = false;
            for (int i = 0; i < m_uiRects.Count; i++)
            {
                try
                {
                    if (i>1&& m_uiRects[i].width == 1)
                    {
                        m_uiRects[i] = new Rect(m_uiRects[i].x, m_uiRects[i - 1].y + m_uiRects[i - 1].height, m_uiRects[i - 1].width, m_uiRects[i - 1].height);

                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log("error weird :" + e.ToString());
                    Debug.Log("i=" + i);
                }
                Rect r = m_uiRects[i];
                Rect r2 = new Rect(r.x + m_uiArea.x, r.y + m_uiArea.y, r.width, r.height);
                if (PC.In.CursorOnUI(r2))//r2.Contains(PC.In.GetCursorPos()))
                {
                    clickedOnUI = true;
                    //   Debug.Log("1 onui=true !!!");
                    break;
                }
				
                /*if (_helpPanel.IsOnGUI())//r2.Contains(PC.In.GetCursorPos()))
                {					
                    clickedOnUI = true;
                    //   Debug.Log("1 onui=true !!!");
                    break;
                }*/
            }
        }
        else
        {
            clickedOnUI = false;
        }
        }


    public void exitConfigurator(bool stillConfiguring)
    {
        //Camera.mainCamera.GetComponent<ObjInteraction>().setActived(true);
        GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(true);
        GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = true;
        m_showGUI = false;
        enableColliders(false);
        this.enabled = false;



        if(!stillConfiguring){
            mainScene.GetComponent<GUIMenuConfiguration>().setVisibility(false);
             mainScene.GetComponent<GUIMenuInteraction>().resetMenu();
            //mainScene.AddComponent<GUIMenuConfiguration>();
            Camera.main.GetComponent<ObjInteraction>().setActived(true);
            mainScene.GetComponent<GUIMenuLeft>().canDisplay(true);
            mainScene.GetComponent<GUIMenuRight>().canDisplay(true);
            mainScene.GetComponent<GUIMenuMain>().enabled = true;
            Camera.main.GetComponent<ObjInteraction>().setActived(true);
        }

    }

    void FixedUpdate()
    {
        if (m_needColliderUpdate==1)
        {
            gameObject.GetComponent<BoxCollider>().size = m_plageMesh.GetComponent<MeshFilter>().mesh.bounds.size * m_plageMesh.transform.localScale.x;
            gameObject.GetComponent<BoxCollider>().center = m_plageMesh.GetComponent<MeshFilter>().mesh.bounds.center *  m_plageMesh.transform.localScale.x;
			gameObject.GetComponent<BoxCollider>().center*=-0.5f;
            m_needColliderUpdate=0;;
        }
    }



	// Update is called once per frame
    void Update()
    {
        //Mode �dition
        if (m_editingMode)
        {
            bool oldShowGui = m_showGUI;

            //met � jour clickedOnUI pour voir si la souris est sur le menu
            updateOnUI();

            m_showGUI = true;

            if (PC.In.Click1Up())
            {				
				_justeSelected=false;
                //Si relachement click et souris pas sur le menu, on affiche le menu
                if (clickedOnUI || PC.In.GetCursorPos().x <= Screen.width - 280)
                {
                    //Debug.Log("showing gui because click one up and not cursor on the ui position");
                    hideGUITimer = Time.time;//remettre hideGUITimer � Time.time permet d'afficher le menu pour quelque secondes
                    mainScene.GetComponent<GUIMenuConfiguration>().ShowHelpPannel = false;
                }
                else
                {
                    Debug.Log("hiding GUI because click1Up and mouse not on ui");
                    hideGUI();
                }

            }

            //Si 2 secondes se sont pass� depuis la derni�re mis � jour de hideGUITimer, on cache le menu
            if (Time.time - hideGUITimer > 2f)
            {
                hideGUI();
            }

            if (!clickedOnUI)
            {
				                //click pas sur le menu
                if (PC.In.Click1Down())
                {
                    //raycast
                    RaycastHit vHit = new RaycastHit();
                    RaycastHit vHitEdge = new RaycastHit();
                    Ray vRay = Camera.main.ScreenPointToRay(PC.In.GetCursorPos());

                    //Si on clicke pas sur un vertex
                    if (!Physics.Raycast(vRay, out vHit, 1000, 1 << LayerMask.NameToLayer("vertexplage")))
                    {
                        //Ar�te s�lectionn�e
                        if (Physics.Raycast(vRay, out vHitEdge, 1000, 1 << LayerMask.NameToLayer("edgeplage")))
                        {
                          //  Debug.Log("edge selected");
							//on remet l'ancien vertex en bleu
							if (m_vertexSelected != null)
							{
								m_vertexSelected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",UNSELECTED_COLOR);
							}
							else if (m_vertices.Count > 0)
							{
								PVertex selected = m_vertices.Last.Value;
								selected.gameObject.GetComponent<Renderer>().material.SetColor("_Color",UNSELECTED_COLOR);
							}
                            m_vertexSelected = null;
                            selectEdgeFromGameObject(vHitEdge.collider.gameObject);
                            //ajout d'un vertex sur l'ar�te s�lectionn�e
                            addVertexOnEdge(m_edgeSelected, vHitEdge.point);
							_justeSelected = true;
                        }
                        //Rien de s�lectionn�e -> on enleve le fonction ajout d'un point dans le vide
                        /*else if (Physics.Raycast(vRay, out vHit, 1000, 1 << LayerMask.NameToLayer("grid")))
                        {
                            //Ajout d'un Vertex, entre le premier et le dernier
                            addVertexOnEdge(m_vertices.First.Value, m_vertices.Last.Value, vHit.point);
                            m_vertexSelected = null;
                            m_edgeSelected = null;

                        }*/

                    }
                    else
                    {
                        //S�lection d'un vertex
                        Debug.Log("select a vertex");
                        GameObject vgo = vHit.collider.gameObject;
                        if (vgo != null)
                        {
                            selectVertexFromGameObject(vgo);
                            //showGUI(2f);
                        }
                        m_edgeSelected = null;
						_justeSelected = true;
                    }

                }
                //Changement de position du vertex s�lectionn�e quand on garde le bouton de souris appliqu�
                else if (PC.In.Click1Hold())
                {
                    RaycastHit vHit = new RaycastHit();
                    Vector2 v2;

                    if (m_vertexSelected != null && _justeSelected)
                    {
                        if (PC.In.Drag1(out v2))
                        {
                            Ray vRay = Camera.main.ScreenPointToRay(PC.In.GetCursorPos());
                            if (Physics.Raycast(vRay, out vHit, 1000, 1 << LayerMask.NameToLayer("grid")))
                            {
                                moveVertex(m_vertexSelected, vHit.point);

                                //On cache le menu lors du d�placement du Vertex
                                if (Time.time - hideGUITimer > 0.2f)
                                {
                                    hideGUI();
                                    Debug.Log("hiding Gui after dragging");
                                    m_uiRects = new List<Rect>();
                                }

                            }

                        }
                    }

                }

            }
            else
            {
                //Si la souris est sur le menu, on le maintien affich�
                if (Time.time - hideGUITimer <= 2f)
                {
                    hideGUITimer = Time.time;
                }
            }
        }
        //Si le mode edition n'est pas activ�, on quitte le configurateur si l'utilisateur clique en dehors du menu.
        else
        {
            if (PC.In.Click1Down())
            {
                if (PC.In.GetCursorPos().x <Screen.width-300)
                {
                    exitConfigurator(false);
                }
            }
        }
		/*if(m_vertexSelected!=null)
		{
			m_vertexSelected.gameObject.renderer.material.SetColor("_Color",SELECTED_COLOR);
		}*/		
		/*if (m_vertexSelected != null)
		{
			//PVertex selected = m_vertexSelected;
			m_vertexSelected.gameObject.renderer.material.SetColor("_Color",SELECTED_COLOR);
		}
		else if (m_vertices.Count > 0)
		{
			PVertex selected = m_vertices.Last.Value;
			selected.gameObject.renderer.material.SetColor("_Color",SELECTED_COLOR);
		}*/

    }

    #region old

    /// <summary>
    /// Useless and not working
    /// </summary>
    private void checkVerticesInList()
    {
        LinkedList<PVertex[]> vv = new LinkedList<PVertex[]>();
        foreach (PEdge e in m_edges)
        {
            if (!m_vertices.Contains(e.v1))
            {
                Debug.Log("Vertex alone !!!");
                vv.AddLast(new PVertex[] { e.v1, e.v2 });
            }
            if (!m_vertices.Contains(e.v2))
            {
                Debug.Log("Vertex alone !!!");
                vv.AddLast(new PVertex[] { e.v2, e.v1 });
            }
        }
        foreach (PVertex[] e1 in vv)
        {
            PVertex[] voisins = new PVertex[2];
            int counte1 = 0;
            foreach (PVertex[] e2 in vv)
            {
                if (e2[0] == e1[0])
                {
                    voisins[counte1] = e2[1];
                    counte1++;
                }
            }
            LinkedListNode<PVertex> v0 = m_vertices.Find(voisins[0]);
            LinkedListNode<PVertex> v1 = m_vertices.Find(voisins[1]);
            if (v0.Previous == v1)
            {
                m_vertices.AddBefore(v0, e1[0]);
            }
            else
            {
                m_vertices.AddAfter(v0, e1[0]);
            }


            for (int j = 0; j < v0.Value.linked.Count; j++)
            {
                if (v0.Value.linked[j] == v1.Value)
                {
                    v0.Value.linked.RemoveAt(j);
                    v0.Value.linked.Add(e1[0]);
                }
            }
            for (int j = 0; j < v1.Value.linked.Count; j++)
            {
                if (v1.Value.linked[j] == v1.Value)
                {
                    v1.Value.linked.RemoveAt(j);
                    v1.Value.linked.Add(e1[0]);
                }
            }
            e1[0].linked = new List<PVertex>();
            e1[0].linked.Add(v0.Value);
            e1[0].linked.Add(v1.Value);
        }
    }

    public float getAngle(Vector3 v1, Vector3 v2)
    {
        float dx = v2.x - v1.x;
        float dz = v2.z - v1.z;

        float d = Mathf.Sqrt(dx * dx + dz * dz);
        return Mathf.Acos(dx / d);

        // return Mathf.Atan(dz / dx);
    }


    bool triangleInList(List<int[]> list, int i0, int i1, int i2)
    {
        foreach (int[] value in list)
        {
            if (i0 == value[0] && i1 == value[1] && i2 == value[2])
            {
                return true;
            }
        }
        return false;
    }

    #endregion

}
