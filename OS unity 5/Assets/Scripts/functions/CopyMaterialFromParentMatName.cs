using UnityEngine;
using System.Collections;

public class CopyMaterialFromParentMatName : MonoBehaviour {

	// Use this for initialization
	void Start () {
		foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
		{
			//if(transform.renderer!=null)
			//	renderer.material = transform.renderer.material;
			for (int i = 0;i<renderer.materials.Length;i++)
			{
				if (renderer.materials[i].name.StartsWith(transform.GetComponent<Renderer>().material.name))
					renderer.materials[i] = transform.GetComponent<Renderer>().material;
			}
		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
