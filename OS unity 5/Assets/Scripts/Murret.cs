using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Murret : MonoBehaviour {

    public GameObject[] xMovePositive=new GameObject[9];
    public GameObject[] xMoveNegative = new GameObject[9];
    public GameObject[] yMovePositive = new GameObject[9];
    public GameObject[] yMoveNegative = new GameObject[9];
    public GameObject[] zMovePositive = new GameObject[9];
    public GameObject[] zMoveNegative = new GameObject[9];
    public GameObject root;
	// Use this for initialization
    public float xScaleInit=0;
    public float yScaleInit = 0;
    public float zScaleInit = 0;

	void Start () {
        int xp=0;
        int xn=0;
        int yp=0;
        int yn=0;
        int zp=0;
        int zn=0;
        foreach (Transform child in root.transform)
        {
            foreach (Transform child2 in child)
            {
                string name = child2.name.ToLower();
                if (name.Contains("top"))
                {
                    yMovePositive[yp] = child2.gameObject;
                    yp++;
                }
                if (name.Contains("bot"))
                {
                    yMoveNegative[yn] = child2.gameObject;
                    yn++;
                }
                if (name.Contains("left"))
                {
                    xMovePositive[xp] = child2.gameObject;
                    xp++;
                }
                if (name.Contains("right"))
                {
                    xMoveNegative[xn] = child2.gameObject;
                    xn++;
                }
                if (name.Contains("front"))
                {
                    zMovePositive[zp] = child2.gameObject;
                    zp++;
                }
                if (name.Contains("back"))
                {
                    zMoveNegative[zn] = child2.gameObject;
                    zn++;
                }
            }
        }

        foreach (Transform child in root.transform)
        {
            if (child.name.ToLower().Contains("scaling"))
            {
                string name = child.name.ToLower();
                if (xScaleInit == 0 && name.Contains("x"))
                    foreach (Transform child2 in child)
                    {
                        xScaleInit = child2.localScale.x;
                        break;
                    }
                if (yScaleInit == 0 && name.Contains("y"))
                     foreach (Transform child2 in child)
                     {
                         yScaleInit = child2.localScale.y;
                         break;
                     }
                if (zScaleInit == 0 && name.Contains("z"))
                     foreach (Transform child2 in child)
                     {
                         zScaleInit = child2.localScale.z;
                         break;
                     }
            }
            if (yScaleInit != 0 && xScaleInit != 0 && zScaleInit != 0)
            {
                break;
            }
        }
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
                        child2.gameObject.GetComponent<Renderer>().material.mainTextureScale += new Vector2( x / (xScaleInit), 0);
                        child2.gameObject.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(x / (2 * (xScaleInit)), 0);
                        child2.localScale += new Vector3(x, 0, 0);
                    }
                }
            }
            Vector3 xPlus = new Vector3(x / 2, 0, 0) * root.transform.localScale.x;
            foreach (GameObject go in xMovePositive)
            {
                go.transform.Translate(xPlus);
            }
            foreach (GameObject go in xMoveNegative)
            {
                go.transform.Translate(-xPlus);
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
                        child2.gameObject.GetComponent<Renderer>().material.mainTextureScale += new Vector2(0, y / (yScaleInit));
                        child2.gameObject.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(0, y / (2 * (yScaleInit)));
                        child2.localScale += new Vector3(0, y, 0);
                    }
                }
            }
            Vector3 yPlus = new Vector3(0, y / 2, 0) * root.transform.localScale.y;
            foreach (GameObject go in yMovePositive)
            {
                go.transform.Translate(yPlus );
            }
            foreach (GameObject go in yMoveNegative)
            {
                go.transform.Translate(-yPlus );
            }

        }

        if (z != 0f)
        {
            foreach (Transform child in root.transform)
            {
                string name = child.name.ToLower();
                if (name.Contains("z") && name.Contains("scaling"))
                {
                    if (name.Contains("x")){
                        foreach (Transform child2 in child)
                        {
                            child2.gameObject.GetComponent<Renderer>().material.mainTextureScale += new Vector2(0, z / (zScaleInit));
                            child2.gameObject.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(0, z / (2 * (zScaleInit)));
                            child2.localScale += new Vector3(0, 0, z);
                        }
                    }
                    else
                        foreach (Transform child2 in child)
                        {
                            child2.gameObject.GetComponent<Renderer>().material.mainTextureScale += new Vector2(z / (zScaleInit), 0);
                            child2.gameObject.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(z / (2 * (zScaleInit)), 0);
                            child2.localScale += new Vector3(0, 0, z);
                        }
                }
            }
            Vector3 zPlus = new Vector3(0, 0, z/2) * root.transform.localScale.z;
            foreach (GameObject go in zMovePositive)
            {
                go.transform.Translate(zPlus);
            }
            foreach (GameObject go in zMoveNegative)
            {
                go.transform.Translate(-zPlus);
            }

        }
       
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Scale(0f, 0f, 0.1f);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Scale(0, 0f, -0.1f);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Scale(0f, 0.3f, 0f);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Scale(0f, -0.3f, 0f);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Scale(0.3f, 0, 0f);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Scale(-0.3f, 0, 0f);
        }
	}
}
