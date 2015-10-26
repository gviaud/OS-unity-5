using UnityEngine;
using System.Collections;

public class AnimatedProjector : MonoBehaviour
{
    //public float fps = 30.0f;
    //public Texture2D[] frames;
	public float speed = 1;
	//public float speedRepeat = 1;
	ArrayList causticPicts = new ArrayList();
    private float frameIndex=0;
    private Projector projector;
	
	void Awake(){
		Object[] _caustics = Resources.LoadAll("caustics", typeof(Texture2D));
		for(int i=0;i<_caustics.Length;i++){
			causticPicts.Add(_caustics[i]);
			//causticPicts[i] = (Texture2D) _caustics[i];
		}
		//Debug.Log(causticPicts.Count);
	}
	
    void Start()
    {		
        projector = GetComponent<Projector>();
        
        //InvokeRepeating("NextFrame",1/fps , 1/fps);
    }
	void FixedUpdate(){
		NextFrame();	
		
	}
    void NextFrame()
    {
//		
//		if((int)frameIndex<0)
//			frameIndex = 0;
//		if (frameIndex>causticPicts.Count)
//			frameIndex = causticPicts.Count;
//        
		projector.material.SetTexture("_ShadowTex",(Texture2D) causticPicts[(int)frameIndex]);
		frameIndex+=speed;
		
		if (frameIndex>=causticPicts.Count)
			frameIndex = 0;
        //frameIndex = (frameIndex + 1) % causticPicts.Count; //Count;
    }
}