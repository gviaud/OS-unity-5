using UnityEngine;
using System.Collections;

public class CopyMaterialFromParent : MonoBehaviour {

	// Use this for initialization
	void Start () {
		foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
		{
			if(transform.GetComponent<Renderer>()!=null)
				renderer.material = transform.GetComponent<Renderer>().material;
		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
