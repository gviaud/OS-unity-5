using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationController : MonoBehaviour {
	
	public const float EPSILON = 0.01f;
		
	float vValue=0.0f;
	
	string _animName = "";
	
	bool allAnim = true;
	
	List<AnimationState> _anims = new List<AnimationState>();
	
	// Use this for initialization
	void Start () {

	}
	
	void Update ()
	{
		UpdateAnim();
	}
	
	public float GetValue()
	{
		return vValue;
	}
	
	public void SetValue(float valueF)
	{
		vValue = valueF;
		ForceValue();
	}
	
	public void Stop()
	{		
		foreach (AnimationState state in _anims) {
			GetComponent<Animation>().Stop();
        }
		
	}
	private void ForceValue()
	{
		foreach (AnimationState state in _anims) {
			GetComponent<Animation>().Play();
			state.enabled = true;
			state.time = vValue*state.length+EPSILON;
			GetComponent<Animation>().Stop();
			state.enabled = false;
        }
	}
	
	private void UpdateAnim()
	{
		foreach (AnimationState state in _anims) {
			if(Mathf.Abs(state.time - (vValue*state.length))>EPSILON)
			{
				GetComponent<Animation>().Play();
				state.enabled = true;
				state.time = vValue*state.length;
			}
			else
			{
				GetComponent<Animation>().Stop();
				state.enabled = false;
			}
        }
	}
	
	public void UpdateValue(float vSliderValue)
	{
		vValue = vSliderValue;
		UpdateAnim();
	}
	
	public void SetPoolObject (Transform pool)
	{
		foreach (AnimationState state in pool.GetComponent<Animation>()) {
			_anims.Add(state);
		}
	}
	
	public void SetPoolObject (Transform pool, string animName)
	{		
		if(animName.Length<1)
		{
			foreach (AnimationState state in pool.GetComponent<Animation>()) {
				_anims.Add(state);
	        }
		}
		else
		{
			SetAnimName(animName);
			foreach (AnimationState state in pool.GetComponent<Animation>()) {
				if (state.name.CompareTo(_animName)==0)
					_anims.Add(state);
	        }
			
		}
	}
	
	public void SetAnimName(string animName)
	{
		_animName = animName;
		if(_animName.Length<1)
		{
			allAnim=false;
		}
				
	}

	
}
