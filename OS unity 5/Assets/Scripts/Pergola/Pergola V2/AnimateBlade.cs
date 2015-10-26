using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimateBlade : MonoBehaviour {
	
	#region Attributs Animation
	private const float EPSILON = 0.05f;
	
 	private	List<AnimationState> _anims = new List<AnimationState>();
	
	private float _totalLength;
	private float _tgtTime;
	
	private bool _animeActive = false;
	#endregion
	#region Attributs Alignemet
	public bool alignX;
	public bool alignY;
	public bool alignZ;
	public Transform alignTo;
	#endregion
	
	// Use this for initialization
	void Start ()
	{
		foreach (AnimationState state in GetComponent<Animation>())
		{
			_anims.Add(state);
        }
		_totalLength = GetComponent<Animation>().clip.length;
	}
	
	// Update is called once per frame
	void Update()
	{
	}
	
	void LateUpdate ()
	{
		if(_animeActive)
		{
			foreach (AnimationState state in _anims)
			{
				if(Mathf.Abs(state.normalizedTime - (_tgtTime))>EPSILON)
				{
					GetComponent<Animation>().Play();
					state.enabled = true;
					state.normalizedTime = _tgtTime;
					GetComponent<Animation>().Sample();
					
				}
				else
				{
					state.enabled = false;
					GetComponent<Animation>().Stop();
					_animeActive = false;
				}
	        }
			
			if(alignTo != null)
			{
				Vector3 newPos = transform.position;
				
				if(alignX)
				{
					newPos.x = alignTo.position.x;	
				}
				if(alignY)
				{
					newPos.y = alignTo.position.y;	
				}
				if(alignZ)
				{
					newPos.z = alignTo.position.z;	
				}
				
				transform.position = newPos;
			}
		}
	}
	
	public void AnimTo(float percent)
	{
		_tgtTime = percent;// * _totalLength;
		_animeActive = true;	
	}
}
