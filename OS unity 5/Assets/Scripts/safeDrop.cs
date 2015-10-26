using UnityEngine;
using System.Collections;

public class safeDrop : MonoBehaviour {
	
	Material sdmat;
	public bool canAdd = true;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	void OnTriggerEnter(Collider c)
	{
		if(c.gameObject.layer == 9 || c.gameObject.layer == 15)
		{
			sdmat.color = new Color(1,0,0,0.5f);
			canAdd = false;
		}
		
	}
	
	void OnTriggerExit(Collider c)
	{
		if(c.gameObject.layer == 9 || c.gameObject.layer == 15)
		{
			sdmat.color = new Color(0,1,0,0.5f);
			canAdd = true;
		}
	}

	public void showObject(bool visible)
	{
		canAdd = true;
		GetComponent<BoxCollider>().isTrigger = true;
		sdmat = new Material(Shader.Find("Transparent/Diffuse"));
		sdmat.color = new Color(0,1,0,0.5f);
		
//		Transform tmp = transform.GetChild(0);
		if(transform.GetChildCount() == 0)
		{
			GetComponent<Renderer>().enabled = visible;
		}
		else
		{
			Transform[] transformsChildren = UsefulFunctions.getAllChild(gameObject);
			for(int i=0;i<transformsChildren.Length;i++)
			//for(int i=0;i<transform.GetChildCount();i++)
			{
				//if(transform.GetChild(i).name.Contains("light") == false)
				if(transformsChildren[i].name.Contains("light") == false)
				{
					if(transformsChildren[i].GetComponent<MeshRenderer>())
						transformsChildren[i].GetComponent<Renderer>().enabled = visible;
				}
			}
		}
		//changement de couleur
		if(transform.GetChildCount() == 0)
		{
			int nb = GetComponent<Renderer>().materials.Length;
			Material[] toModel = new Material[nb];
			for(int i=0;i<nb;i++)
			{
				toModel[i]=sdmat;
			}
			GetComponent<Renderer>().materials = toModel;
//			renderer.material = sdmat;
		}
		else
		{
			Transform[] transformsChildren = UsefulFunctions.getAllChild(gameObject);
			for(int i=0;i<transformsChildren.Length;i++)
			//for(int i=0;i<transform.GetChildCount();i++)
			{
				
				if(transformsChildren[i].GetComponent<MeshRenderer>())
				{
					
					//La texture d'eau est mal restituée à la déselection, solution temporaire est de cacher l'eau
					//le temps du déplacement
					if (transformsChildren[i].name.Contains("water"))
					{
						transformsChildren[i].GetComponent<Renderer>().enabled = true;	
					}
					else if (transformsChildren[i].name.EndsWith("nored"))
					{
						transformsChildren[i].GetComponent<Renderer>().enabled = false;	
					}
					else if (transformsChildren[i].GetComponent<Renderer>().enabled) 
					{
						int nb = transformsChildren[i].GetComponent<Renderer>().materials.Length;					
						Material[] _toModel = new Material[nb];
						for (int j=0;j<nb;j++)
						{
							_toModel[j] = sdmat;
						}
						transformsChildren[i].GetComponent<Renderer>().materials = _toModel;
					}
				}
			}
		}
	}
		
}
