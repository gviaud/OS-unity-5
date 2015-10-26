using UnityEngine;
using System.Collections;

public class PoolFrise : MonoBehaviour {
	
	Transform _liner;
	
	bool _link = true;
	
	int _linerHashcode = -1;
	int _friseHashcode = -1;
	
	// Use this for initialization
	void Start ()
	{
		_liner = transform.parent.FindChild("liner");
		if(!_liner)
			_liner = transform.parent.FindChild("coque");
//		Debug.Log("liner "+ _liner);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(_link)
		{	if(!_liner)
			{
				_liner = transform.parent.FindChild("liner");
				if(!_liner)
					_liner = transform.parent.FindChild("coque");
			}
			if(_liner.GetComponent<Renderer>().material.mainTexture!=null)
			{	
				if(_liner.GetComponent<Renderer>().material.mainTexture.GetHashCode() != _linerHashcode)
				{
					GetComponent<Renderer>().material.mainTexture = _liner.GetComponent<Renderer>().material.mainTexture;
					_linerHashcode = _liner.GetComponent<Renderer>().material.mainTexture.GetHashCode();
					_friseHashcode = _linerHashcode;
				}
			}
			else
			{
				GetComponent<Renderer>().material.mainTexture = null;
			}
			if(GetComponent<Renderer>().material.color != _liner.GetComponent<Renderer>().material.color)
			{
				GetComponent<Renderer>().material.color = _liner.GetComponent<Renderer>().material.color;	
			}
			if(_liner.GetComponent<Renderer>().material.mainTexture!=null)
			{	
				if(GetComponent<Renderer>().material.mainTexture.GetHashCode() != _friseHashcode)
				{
					_link = false;
					enabled = false;
				}	
			}
		}
	}
}
