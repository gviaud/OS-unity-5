using UnityEngine;
using System.Collections;

public class MaskCreator : MonoBehaviour 
{
	
	public RenderTexture rt;
	Texture _text;
	bool _mode2D = false;
	float _factor = 1.0f;
	int size = 1024;
	
#region unity_func
	void Awake()
	{
		size = GetMinSquareValue();
		UsefullEvents.OnResizeWindowEnd += ReCreateMask;
	}
	
	// Use this for initialization
	void Start ()
	{
		
		//rt = new RenderTexture(Screen.width,Screen.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Default);
		rt = new RenderTexture(size,size,0,RenderTextureFormat.Default,RenderTextureReadWrite.Default);
		GetComponent<Camera>().targetTexture = rt;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(!usefullData.lowTechnologie)
		{
			gameObject.SetActive(false);
		}
	}
	
	void OnGUI()
	{	
	}
	
	//-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizeWindowEnd -= ReCreateMask;
    }	
#endregion
	
	public int GetSize()
	{
		return size;
	}
	
	//-----------------------------------------------------
    private void ReCreateMask()
    {
		size = GetMinSquareValue();
		rt = new RenderTexture(size,size,0,RenderTextureFormat.Default,RenderTextureReadWrite.Default);
//		Debug.Log("w "+size);
		GetComponent<Camera>().targetTexture = rt;
    }
	private int GetMinSquareValue()
	{
		/*int squareValue = 16;
		while (squareValue<Screen.width)
		{
			squareValue*=squareValue;
			if(squareValue>=2048)
				return squareValue;
		}
		return squareValue;*/
		return 1024;
		//return Mathf.Max(Screen.width, Screen.height);
	}
	
	public Texture getMask()
	{
		if (!_mode2D)
			return (Texture) rt;	
		else
			return  _text;
	}
	
	public void Set2DMode(bool mode2d)
	{
		_mode2D = mode2d;
		ReCreateMask();
	}
	
	public void SetTexture(Texture text)
	{
		_text = text;
	}
	public Texture GetTexture()
	{
		return _text;
	}
	public bool IsMode2D()
	{
		return _mode2D;
	}
	public float GetFactor()
	{
		return _factor;
	}
	public void SetFactor(float factor)
	{
		_factor = factor;
	}
}
