using UnityEngine;
using System.Collections;

public class AnimatedTexture_decal_switch : MonoBehaviour
{
    //public float fps = 30.0f;
    //public Texture2D[] frames;
	public float speed = 0.1f;
	public float tiling = 1.0f;
	public string nameImgList = "";
	public string nameImgList_alternative = "";
	public string nameShader = "";
	public string nameTexture = "";
	public bool sameImage=false;
	//public float speedRepeat = 1;
	ArrayList causticPicts = new ArrayList();
	ArrayList causticPicts_alternative = new ArrayList();
	Texture2D pic = null;
	
    public float frameIndex=0;
    public float nbImg=8.0f;
    //private Projector projector;
	
	public bool _alternativeImages = false;
	
	void Awake(){
		if(sameImage)
		{
			pic = (Texture2D)Resources.Load(nameImgList, typeof(Texture2D));
			if(GetComponent<Renderer>().material.shader.name.CompareTo(nameShader)==0)
			{
				GetComponent<Renderer>().material.SetTextureScale(nameTexture,new Vector2(1.0f/nbImg, 1.0f/nbImg));
				GetComponent<Renderer>().material.SetTexture(nameTexture,pic);
			}
		}
		else
		{
			Object[] _caustics = Resources.LoadAll(nameImgList, typeof(Texture2D));
			for(int i=0;i<_caustics.Length;i++){
				causticPicts.Add(_caustics[i]);
				//causticPicts[i] = (Texture2D) _caustics[i];
			}
			if (nameImgList_alternative.Length>0)
			{
				Object[] _caustics_alternative = Resources.LoadAll(nameImgList_alternative, typeof(Texture2D));
				for(int i=0;i<_caustics_alternative.Length;i++){
					causticPicts_alternative.Add(_caustics_alternative[i]);
					//causticPicts[i] = (Texture2D) _caustics[i];
				}
			}
		}
//		Debug.Log(causticPicts.Count);
	}
	
    void Start()
    {		
	//InvokeRepeating("NextFrame",1/fps , 1/fps);
		if(usefullData.lowTechnologie)
		{
			nameShader = "Transparent/Diffuse";
			nameTexture = "_MainTex";
		
			int nb=0;
			foreach (AnimatedTexture_decal_switch anim in gameObject.GetComponents<AnimatedTexture_decal_switch>())
			{
				if(nb>0)
					anim.enabled = false;
				nb++;
			}
		}
    }
	void FixedUpdate(){
		NextFrame();			
	}
  /*  void NextFrame()
    {	
		Debug.Log("CAUSTIC Update  : "+causticPicts.Count);
		if(GetComponent<ObjBehav>()!= null)
			if(!GetComponent<ObjBehav>().oneObjectSelected)
				renderer.material.SetTexture("_DecalTex",(Texture2D) causticPicts[(int)frameIndex]);
			else
				renderer.material.SetTexture("_DecalTex",(Texture2D) causticPicts[(int)frameIndex]);
		frameIndex+=speed;
		if (frameIndex>causticPicts.Count) frameIndex = 0;
        //frameIndex = (frameIndex + 1) % causticPicts.Count; //Count;
    }*/
	void NextFrame()
    {
		//Debug.Log("CAUSTIC NextFrame");
		if(GetComponent<Renderer>().isVisible)
		{
		//	Debug.Log("CAUSTIC isVisible");
			if(GetComponent<Renderer>().material.shader.name.CompareTo(nameShader)==0)
			{
				if(sameImage)
				{
					int row = (int)frameIndex/(int)nbImg;
					int col = (int)( row*8+nbImg);
					GetComponent<Renderer>().material.SetTextureOffset(nameTexture,new Vector2(row/nbImg,col/nbImg));
                   // Debug.Log ("meme image "+frameIndex);

				}
				else
				{
					GetComponent<Renderer>().material.SetTextureScale(nameTexture,new Vector2(tiling, tiling));
					if(!_alternativeImages)
					{
						GetComponent<Renderer>().material.SetTexture(nameTexture,(Texture2D) causticPicts[(int)frameIndex]);
					}
					else
					{
						if(causticPicts_alternative.Count==0)
						{
							GetComponent<Renderer>().material.SetTexture(nameTexture,null);
						}
						else
						{
							GetComponent<Renderer>().material.SetTexture(nameTexture,(Texture2D) causticPicts_alternative[(int)frameIndex]);
						}
					}


					frameIndex+=speed;
					if (frameIndex>=causticPicts.Count) 
						frameIndex = 0;
				}
			}
		}
    }
	
	public void SetAlternativeImages(bool alt)
	{
		_alternativeImages = alt;
	}
	public bool IsAlternativeImages()
	{
		return _alternativeImages;
	}
}