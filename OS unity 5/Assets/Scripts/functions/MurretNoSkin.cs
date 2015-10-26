using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pointcube.Global;
using System;
using Pointcube.Utils;

public class MurretNoSkin : MonoBehaviour, Function_OS3D
{

    public GameObject[] xMovePositive = new GameObject[9];
    public GameObject[] xMoveNegative = new GameObject[9];
    public GameObject[] yMovePositive = new GameObject[9];
    public GameObject[] yMoveNegative = new GameObject[9];
    public GameObject[] zMovePositive = new GameObject[9];
    public GameObject[] zMoveNegative = new GameObject[9];
    public GameObject root;
    // Use this for initialization
    public float xScaleInit = 0;
    public float yScaleInit = 0;
    public float zScaleInit = 0;
    private float m_scaleFactor = 0.1f;
    private float xScale = 0;
    private float yScale = 0;
    private float zScale = 0;
    private bool m_configuring = false;
    private GUISkin m_skinInter;
    private GUISkin m_skin;
    private Rect _uiArea;
    GameObject m_selectedBackup;
    string valueStrX;
    string valueStrY;
    string valueStrZ;


    #region Function_OS3D
    private int id = 0;



    public void moveMesh(Mesh m, Vector3 delta){
        Vector3[] vertices = new Vector3[m.vertexCount];
        for (int i = 0; i < m.vertexCount; i++)
        {
            vertices[i].x += m.vertices[i].x +  delta.x;
            vertices[i].y += m.vertices[i].y + delta.y;
            vertices[i].z += m.vertices[i].z + delta.z;
        }
        m.vertices = vertices;
        m.RecalculateBounds();

    }

    public void addSizeToMesh(Mesh m, Vector3 size)
    {
        Vector3 center = m.bounds.center;
        Vector3[] vertices = new Vector3[m.vertexCount];
        for (int i = 0; i < m.vertexCount; i++)
        {
            vertices[i].x += m.vertices[i].x+Mathf.Sign(m.vertices[i].x-center.x) * size.x / 2;
            vertices[i].y += m.vertices[i].y + Mathf.Sign(m.vertices[i].y-center.y) * size.y / 2;
            vertices[i].z += m.vertices[i].z + Mathf.Sign(m.vertices[i].z-center.z) * size.z / 2;
        }
        m.vertices = vertices;
        m.RecalculateBounds();

    }


    public string GetFunctionName()
    {
        return "Changer";
    }
    public string GetFunctionParameterName()
    {
        return "Changer la taille";
    }

    public int GetFunctionId()
    {
        return id;
    }

    public void DoAction()
    {
        GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);

        GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
        GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;
        //m_selectedBackup = Camera.mainCamera.GetComponent<ObjInteraction>().getSelected();
        Camera.main.GetComponent<ObjInteraction>().setSelected(null, true);
        Camera.main.GetComponent<ObjInteraction>().setActived(false);

        enabled = true;
    }

    //  sauvegarde/chargement	
    public void save(BinaryWriter buf)
    {
        buf.Write((double)xScale);
        buf.Write((double)yScale);
        buf.Write((double)zScale);
    }

    public void load(BinaryReader buf)
    {
        xScale = (float)buf.ReadDouble();
        yScale = (float)buf.ReadDouble();
        zScale = (float)buf.ReadDouble();
        //Debug.log("xScale="+xScale);
        //Debug.log("yScale=" + yScale);
        //Debug.log("zScale=" + zScale);
        if (xScaleInit == 0)
        {
            //Debug.log("xscaleinit=0");
            Start();
        }
        else
        {
            //Debug.log("xscaleinit!=0");
            Scale(xScale - xScaleInit, yScale - yScaleInit, zScale - zScaleInit);
        }
    }

    //Set L'ui si besoin
    public void setUI(FunctionUI_OS3D ui)
    {
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}

    //similaire au Save/Load mais utilisé en interne d'un objet a un autre (swap)
    public ArrayList getConfig()
    {
        ArrayList al = new ArrayList();
        al.Add(xScale);
        al.Add(yScale);
        al.Add(zScale);
        return al;
    }

    public void setConfig(ArrayList config)
    {
        xScale = (float)config[0];
        yScale = (float)config[1];
        zScale = (float)config[2];
        if (xScaleInit == 0)
            Start();
        else
            Scale(xScale - xScaleInit, yScale - yScaleInit, zScale - zScaleInit);
    }





    public void OnGUI()
    {/*
        GUILayout.BeginArea(_uiArea);
        GUILayout.FlexibleSpace();
        GUILayout.Box("", "UP", GUILayout.Width(280), GUILayout.Height(150));
        GUILayout.BeginVertical("MID");
        if (GUILayout.Button(TextManager.GetText("Muret.Resize"), "Menu", GUILayout.Height(50), GUILayout.Width(280)))
        {
            GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(true);
            GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = true;
            enabled = false;

        }
        if (valueStrX == null)
        {
            Debug.Log("valuestrx=null");
            valueStrX = (Mathf.Ceil(xScale * 10) / 10).ToString();
            valueStrY = (Mathf.Ceil(yScale * 10) / 10).ToString();
            valueStrZ = (Mathf.Ceil(zScale * 10) / 10).ToString();
        }

        GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
        GUILayout.FlexibleSpace();
        if (GUILayout.RepeatButton("+", "btn+", GUILayout.Height(50), GUILayout.Width(50)))
        {
            Debug.Log("Xplus pressed");
            Scale(m_scaleFactor, 0, 0);
            xScale += m_scaleFactor;
            valueStrX = (Mathf.Ceil(xScale * 10) / 10).ToString();
        }
        //Debug.Log("valueStrX: "+valueStrX);
        string tempText = GUILayout.TextField(valueStrX, GUILayout.Height(50), GUILayout.Width(50));

        bool minusPressed = false;
        if (GUILayout.RepeatButton("-", "btn-", GUILayout.Height(50), GUILayout.Width(50)))
        {
            Debug.Log("Xminus pressed");
            minusPressed = true;
            if (xScale - m_scaleFactor > 0)
            {
                Debug.Log(">0");
                Scale(-m_scaleFactor, 0, 0);
                xScale -= m_scaleFactor;
                Debug.Log("Xminus pressed");
                Debug.Log("valueStrX before" + valueStrX);
                valueStrX = (Mathf.Ceil(xScale * 10) / 10).ToString();
                Debug.Log("minus => valueStrX=" + valueStrX);
            }
            else
            {
                Scale(-xScale, 0, 0);
                xScale = 0;
                valueStrX = "0";
            }

        }
        if (!tempText.Equals(valueStrX) && !minusPressed)
        {

            try
            {
                Debug.Log("parsing newScalex");
                float newScaleX = float.Parse(tempText.Replace(',', '.'));
                if (newScaleX > 0)
                {
                    Debug.Log("newscaleX>0");
                    Scale(newScaleX - xScale, 0, 0);
                    xScale = newScaleX;
                    valueStrX = tempText;
                }
                else
                {

                    Scale(-xScale, 0, 0);
                    xScale = 0;
                    if (newScaleX == 0)
                    {
                        Debug.Log("newscaleX=0");
                        valueStrX = tempText;
                    }
                    else
                    {
                        Debug.Log("newscaleX=0");
                        valueStrX = "0";
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("exception : " + e.ToString());
                if (tempText.Equals(""))
                {
                    Debug.Log("temptext=''");
                    valueStrX = "0";
                    Scale(-xScale, 0, 0);
                    xScale = 0;
                }
                else
                {
                    valueStrX = (Mathf.Ceil(xScale * 10) / 10).ToString();
                }
            }
        }

        GUILayout.Label(TextManager.GetText("Muret.Width"), GUILayout.Width(60));
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
        GUILayout.FlexibleSpace();
        if (GUILayout.RepeatButton("+", "btn+", GUILayout.Height(50), GUILayout.Width(50)))
        {
            Scale(0, 0, m_scaleFactor);
            zScale += m_scaleFactor;
            valueStrZ = (Mathf.Ceil(zScale * 10) / 10).ToString();
        }
        minusPressed = false;
        tempText = GUILayout.TextField(valueStrZ, GUILayout.Height(50), GUILayout.Width(50));
        if (GUILayout.RepeatButton("-", "btn-", GUILayout.Height(50), GUILayout.Width(50)))
        {
            minusPressed = true;
            if (zScale - m_scaleFactor > 0)
            {
                Scale(0, 0, -m_scaleFactor);
                zScale -= m_scaleFactor;
                valueStrZ = (Mathf.Ceil(zScale * 10) / 10).ToString();
            }
            else
            {
                Scale(0, 0, -zScale);
                zScale = 0;
                valueStrZ = "0";
            }
        }
        if (!tempText.Equals(valueStrZ) && !minusPressed)
        {
            try
            {
                float newScaleZ = float.Parse(tempText.Replace(',', '.'));
                if (newScaleZ > 0)
                {
                    Scale(0, 0, newScaleZ - zScale);
                    zScale = newScaleZ;
                    valueStrZ = tempText;
                }
                else
                {
                    //Debug.log("zscale="+zScale);
                    Scale(0, 0, -zScale);
                    zScale = 0;
                    if (newScaleZ == 0)
                    {
                        //Debug.log("newScaleZ=0");
                        valueStrZ = tempText;
                    }
                    else
                        valueStrZ = "0";
                }

            }
            catch (Exception e)
            {
                //Debug.log("Exception");
                if (tempText.Equals(""))
                {
                    //Debug.log("temptxt=''");
                    valueStrZ = "0";
                    Scale(0, 0, -zScale);
                    zScale = 0;
                }
                else
                    valueStrZ = (Mathf.Ceil(zScale * 10) / 10).ToString();
            }
        }
        GUILayout.Label(TextManager.GetText("Muret.Length"), GUILayout.Width(60));
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("", "bg", GUILayout.Height(50), GUILayout.Width(280));
        GUILayout.FlexibleSpace();
        if (GUILayout.RepeatButton("+", "btn+", GUILayout.Height(50), GUILayout.Width(50)))
        {
            Scale(0, m_scaleFactor, 0);
            yScale += m_scaleFactor;
            valueStrY = (Mathf.Ceil(yScale * 10) / 10).ToString();
            //DoSomething
        }
        minusPressed = false;
        tempText = GUILayout.TextField(valueStrY, GUILayout.Height(50), GUILayout.Width(50));
        if (GUILayout.RepeatButton("-", "btn-", GUILayout.Height(50), GUILayout.Width(50)))
        {
            minusPressed = true;
            if (yScale - m_scaleFactor > 0)
            {
                Scale(0, -m_scaleFactor, 0);
                yScale -= m_scaleFactor;
                valueStrY = (Mathf.Ceil(yScale * 10) / 10).ToString();
            }
            else
            {
                Scale(0, -yScale, 0);
                yScale = 0;
                valueStrY = "0";
            }

        }
        if (!tempText.Equals(valueStrY) && !minusPressed)
        {
            try
            {
                float newScaleY = float.Parse(tempText.Replace(',', '.'));
                if (newScaleY > 0)
                {
                    Scale(0, newScaleY - yScale, 0);
                    yScale = newScaleY;
                    valueStrY = tempText;
                }
                else
                {
                    Scale(0, -yScale, 0);
                    yScale = 0;
                    if (newScaleY == 0)
                        valueStrY = tempText;
                    else
                        valueStrY = "0";
                }
            }
            catch (Exception e)
            {

                if (tempText.Equals(""))
                {
                    valueStrY = "0";
                    Scale(0, -yScale, 0);
                    yScale = 0;
                }
                else
                    valueStrY = (Mathf.Ceil(yScale * 10) / 10).ToString();
            }
        }
        GUILayout.Label(TextManager.GetText("Muret.Height"), GUILayout.Width(60));
        GUILayout.Space(20);
        GUILayout.EndHorizontal();




        GUILayout.EndVertical();
        GUILayout.Box("", "DWN", GUILayout.Width(280), GUILayout.Height(150));//fade en bas

        GUILayout.FlexibleSpace();

        GUILayout.EndArea();



        GUI.skin = backup;
      * */
    }

    #endregion

    void Start()
    {
        if (root == null)
        {
            root = this.gameObject;
        }
        //Debug.log("start");
        _uiArea = new Rect(Screen.width - 280, 0, 280, Screen.height);
        int xp = 0;
        int xn = 0;
        int yp = 0;
        int yn = 0;
        int zp = 0;
        int zn = 0;


       Hashtable lchildren = new Hashtable();
        foreach (Transform child2 in root.transform)
        {

               LinkedList<char> parent=new LinkedList<char>();
                string name = child2.name.ToLower();
                if (name.Contains("top"))
                {
                    yMovePositive[yp] = child2.gameObject;
                    yp++;
                    parent.AddLast('y');
                }
                if (name.Contains("bot"))
                {
                    yMoveNegative[yn] = child2.gameObject;
                    yn++;
                    parent.AddLast('y');
                }
                if (name.Contains("left"))
                {
                    xMovePositive[xp] = child2.gameObject;
                    xp++;
                    parent.AddLast('x');
                }
                if (name.Contains("right"))
                {
                    xMoveNegative[xn] = child2.gameObject;
                    xn++;
                    parent.AddLast('x');
                }
                if (name.Contains("front"))
                {
                    zMovePositive[zp] = child2.gameObject;
                    zp++;
                    parent.AddLast('z');
                }
                if (name.Contains("back"))
                {
                    zMoveNegative[zn] = child2.gameObject;
                    zn++;
                    parent.AddLast('z');
                }
            lchildren.Add(child2,parent);
        }


                    new GameObject("xscaling").transform.parent = root.transform;
        new GameObject("yscaling").transform.parent = root.transform;
        new GameObject("zscaling").transform.parent = root.transform;
        new GameObject("xyscaling").transform.parent = root.transform;
        new GameObject("xzscaling").transform.parent = root.transform;
        new GameObject("zyscaling").transform.parent = root.transform;
        new GameObject("moving").transform.parent = root.transform;

        foreach (Transform child in lchildren.Keys)
        {
           LinkedList<char> parents=(LinkedList<char>)lchildren[child];
           if(parents.Count>2){
               child.parent=root.transform.FindChild("moving");
           }
           else if(parents.Contains('x')&&parents.Contains('y')){
                child.parent=root.transform.FindChild("zscaling");
           }
           else if(parents.Contains('x')&&parents.Contains('z')){
                child.parent=root.transform.FindChild("yscaling");
           }
           else if(parents.Contains('y')&&parents.Contains('z')){
                child.parent=root.transform.FindChild("xscaling");
           }
           else if(parents.Contains('x')){
                 child.parent=root.transform.FindChild("zyscaling");
           }
           else if(parents.Contains('y')){
                 child.parent=root.transform.FindChild("xzscaling");
           }
           else if(parents.Contains('z')){
                 child.parent=root.transform.FindChild("xyscaling");
           }
        }

        foreach (Transform child in root.transform)
        {
            //Debug.log("browse child :" + child.name);
            if (child.name.ToLower().Contains("scaling"))
            {
                string name = child.name.ToLower();
                if (/*xScaleInit == 0 && */name.Contains("x"))
                    foreach (Transform child2 in child)
                    {
                        //Debug.log("xScaleInit: size gotten from " + child2.name);
                        float temp= child2.GetComponent<MeshFilter>().mesh.bounds.size.x;
                        if(temp>xScaleInit){
                            xScaleInit=temp;
                        }
                       // break;
                    }
                if (/*yScaleInit == 0 && */name.Contains("y"))
                    foreach (Transform child2 in child)
                    {
                        //Debug.log("yScaleInit: size gotten from " + child2.name);
                        //yScaleInit = child2.lossyScale.y;
                        float temp= child2.GetComponent<MeshFilter>().mesh.bounds.size.y;
                        if (temp > yScaleInit)
                        {
                            yScaleInit = temp;
                        }
                       // break;
                    }
                if (/*zScaleInit == 0 && */name.Contains("z"))
                    foreach (Transform child2 in child)
                    {
                        //Debug.log("zScaleInit: size gotten from " + child2.name);
                       // zScaleInit = child2.lossyScale.z;
                        float temp = child2.GetComponent<MeshFilter>().mesh.bounds.size.z;
                        if (temp > zScaleInit)
                        {
                            zScaleInit = temp;
                        }
                       // break;
                    }
            }
            /*
            if (yScaleInit != 0 && xScaleInit != 0 && zScaleInit != 0)
            {
                break;
            }
             * */
        }
        if (xScale != 0 && yScale != 0 && zScale != 0)
        {
            Scale(xScale - xScaleInit, yScale - yScaleInit, zScale - zScaleInit);
        }
        else
        {
            xScale = xScaleInit;
            yScale = yScaleInit;
            zScale = zScaleInit;
        }
       // enabled = false;
    }

    void Scale(float x, float y, float z)
    {
        if (x != 0f)
        {
            foreach (Transform child in root.transform)
            {
                string name = child.name.ToLower();
                if (name.Contains("x") && name.Contains("scaling"))
                {
                    foreach (Transform child2 in child)
                    {
                       // child2.parent = null;
                        child2.gameObject.GetComponent<Renderer>().material.mainTextureScale += new Vector2(x / (xScaleInit), 0);
                        child2.gameObject.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(x / (2 * (xScaleInit)), 0);
                       // child2.localScale += new Vector3(x, 0, 0);
                        addSizeToMesh(child2.GetComponent<MeshFilter>().mesh, new Vector3(x, 0, 0));
                        //child2.parent = child;
                    }
                }
            }
            Vector3 xPlus = new Vector3(x / 2, 0, 0);
            foreach (GameObject go in xMovePositive)
            {
                moveMesh(go.transform.GetComponent<MeshFilter>().mesh, xPlus);
            }
            foreach (GameObject go in xMoveNegative)
            {
                moveMesh(go.transform.GetComponent<MeshFilter>().mesh, -xPlus);
            }

        }

        if (y != 0f)
        {
            foreach (Transform child in root.transform)
            {
                string name = child.name.ToLower();
                if (name.Contains("y") && name.Contains("scaling"))
                {
                    foreach (Transform child2 in child)
                    {
                       // child2.parent = null;
                        child2.gameObject.GetComponent<Renderer>().material.mainTextureScale += new Vector2(0, y / (yScaleInit));
                        child2.gameObject.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(0, y / (2 * (yScaleInit)));
                       // child2.localScale += new Vector3(0, y, 0);
                        addSizeToMesh(child2.GetComponent<MeshFilter>().mesh, new Vector3(0, y, 0));
                        // child2.parent = child;
                    }
                }
            }
            Vector3 yPlus = new Vector3(0, y / 2, 0);
            foreach (GameObject go in yMovePositive)
            {
                moveMesh(go.transform.GetComponent<MeshFilter>().mesh, yPlus);
            }
            foreach (GameObject go in yMoveNegative)
            {
                moveMesh(go.transform.GetComponent<MeshFilter>().mesh, -yPlus);
            }

        }

        if (z != 0f)
        {
            foreach (Transform child in root.transform)
            {
                string name = child.name.ToLower();
                if (name.Contains("z") && name.Contains("scaling"))
                {
                    if (name.Contains("x"))
                    {
                        foreach (Transform child2 in child)
                        {
                            //child2.parent = null;
                            child2.gameObject.GetComponent<Renderer>().material.mainTextureScale += new Vector2(0, z / (zScaleInit));
                            child2.gameObject.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(0, z / (2 * (zScaleInit)));
                            //child2.localScale += new Vector3(0, 0, z);
                            addSizeToMesh(child2.GetComponent<MeshFilter>().mesh, new Vector3(0, 0, z));
                            //child2.parent = child;
                        }
                    }
                    else
                        foreach (Transform child2 in child)
                        {
                            //child2.parent = null;
                            child2.gameObject.GetComponent<Renderer>().material.mainTextureScale += new Vector2(z / (zScaleInit), 0);
                            child2.gameObject.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(z / (2 * (zScaleInit)), 0);
                            //child2.localScale += new Vector3(0, 0, z);
                            addSizeToMesh(child2.GetComponent<MeshFilter>().mesh, new Vector3(0, 0, z));
                            //child2.parent = child;
                        }
                }
            }
            Vector3 zPlus = new Vector3(0, 0, z / 2);
            foreach (GameObject go in zMovePositive)
            {
                moveMesh(go.transform.GetComponent<MeshFilter>().mesh, zPlus); 
            }
            foreach (GameObject go in zMoveNegative)
            {
                moveMesh(go.transform.GetComponent<MeshFilter>().mesh, -zPlus);
            }

        }

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Scale(0f, 0f, 1f);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Scale(0, 0f, -1f);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Scale(0f, 3f, 0f);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Scale(0f, -3f, 0f);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Scale(3f, 0, 0f);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Scale(-3f, 0, 0f);
        }

        if (PC.In.Click1Up() && !PC.In.CursorOnUI(_uiArea))
        {
            //Debug.log("murret validated");
            Validate();
        }
    }


    void Validate()
    {
        Camera.main.GetComponent<ObjInteraction>().configuringObj(null);

        GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().unConfigure();
        GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);

        Camera.main.GetComponent<ObjInteraction>().setSelected(null, false);
        enabled = false;
        m_configuring = false;
    }
}
