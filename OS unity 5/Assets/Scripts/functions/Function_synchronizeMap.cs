using UnityEngine;
using System.Collections;

public class Function_synchronizeMap : MonoBehaviour
{
	
	private GameObject model;

	public string sznameMap = "";

	void Awake(){

		model = GameObject.Find("liner");

		Texture texture = model.GetComponent<Renderer>().material.GetTexture(sznameMap);
		
		/*if(texture)
		{*/
			GetComponent<Renderer>().material.SetTexture(sznameMap, texture);
		//}
	}
	// Use this for initialization
	void Start () {
		
		model = GameObject.Find("liner");
		
		Texture texture = model.GetComponent<Renderer>().material.GetTexture(sznameMap);
		
		/*if(texture)
		{*/
		GetComponent<Renderer>().material.SetTexture(sznameMap, texture);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		/*if(model && model.renderer && model.renderer.material && model.renderer.material.GetTexture(sznameMap))
		//if(texture)
		{*/
			Texture texture = model.GetComponent<Renderer>().material.GetTexture(sznameMap);
			GetComponent<Renderer>().material.SetTexture(sznameMap, texture);
		//}
	}
}
