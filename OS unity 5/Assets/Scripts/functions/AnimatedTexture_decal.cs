using UnityEngine;
using System.Collections;

public class AnimatedTexture_decal : MonoBehaviour
{
    //public float fps = 30.0f;
    //public Texture2D[] frames;
	public float speed = 0.3f;
	public float tiling = 1.0f;
	public string nameImgList = "";
	public string nameShader = "";
	public string nameTexture = "";
	public bool sameImage=false;
	//public float speedRepeat = 1;
	ArrayList causticPicts = new ArrayList();
	Texture2D pic = null;
	
    public float frameIndex=0;
    public float nbImg=100.0f;
    //private Projector projector;
	
	void Awake(){

		speed = 0.54f;
		nbImg=100.0f;
		tiling = 0.75f;
		LoadData();

//		Debug.Log(causticPicts.Count);
	}
	public void LoadData()
	{
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
			causticPicts.Clear();
			Object[] _caustics = Resources.LoadAll(nameImgList, typeof(Texture2D));
			for(int i=0;i<_caustics.Length;i++){
				causticPicts.Add(_caustics[i]);
				//causticPicts[i] = (Texture2D) _caustics[i];
			}
			//RUSTINE pour diminuer la taille des caustics sans reprendre les piscines
			if((nameImgList.CompareTo("caustics")==0) && tiling==0.5f)
			{
				tiling*=1.6f;
			}
		}
	}
    void Start()
    {		
	//InvokeRepeating("NextFrame",1/fps , 1/fps);
		if(usefullData.lowTechnologie)
		{
			if(nameShader.CompareTo("Custom/FX/Water p")!=0)
			{
				nameShader = "Transparent/Diffuse";
				nameTexture = "_MainTex";
			}
		}
		tiling = tiling*1.5f;
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
					int col = (int)( row*100+nbImg);
					GetComponent<Renderer>().material.SetTextureOffset(nameTexture,new Vector2(row/nbImg,col/nbImg));
				}
				else
				{
					GetComponent<Renderer>().material.SetTextureScale(nameTexture,new Vector2(tiling, tiling));
			        GetComponent<Renderer>().material.SetTexture(nameTexture,(Texture2D) causticPicts[(int)frameIndex]);
					frameIndex+=speed;
					if (frameIndex>=causticPicts.Count) 
						frameIndex = 0;
				}
			}
		}
    }
}