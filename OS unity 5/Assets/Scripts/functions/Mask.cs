using UnityEngine;
using System.Collections;

public class Mask : MonoBehaviour
{

	public Texture textureApply = null;
	private float width = 512;
	private float height = 512;
	private float offsetX = 0;
	private float offsetY = 0;
	private float _factor = 1.0f;
	private GUITexture _guiText = null;
	private Vector3 _oldPos = new Vector3();
	private Quaternion _oldQuat = new Quaternion();
	private Vector3 _oldCamPos = new Vector3();
	private Quaternion _oldCamQuat = new Quaternion();
	private float _oldFieldOfView = 60.0f;
	
	private float _oldFactor=1.0f;
	
	private CameraFrustum _cameraFrustrum;
	
	MaskCreator mc;
	private bool _oldMode2D = false;
	
	void Awake()
	{
	//	UsefullEvents.OnResizeWindowEnd += Update;
        UsefullEvents.OnResizeWindowEnd += UpdateMapping;
	}
	// Use this for initialization

	void Start ()
	{
		if(usefullData.lowTechnologie)
		{
		_cameraFrustrum = GameObject.Find("mainCam").GetComponent<CameraFrustum>();
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		Vector3[] vertices = mesh.vertices;
		Vector2[] uvs = new Vector2[vertices.Length];
		int i = 0;
		while (i < uvs.Length) {
			uvs[i] = new Vector2 (vertices[i].x, vertices[i].z);
			//Debug.Log("mask Start : "+uvs[i]);
			i++;
		}
		mesh.uv = uvs;
		
		if(textureApply!=null)
		{
			textureApply.wrapMode = TextureWrapMode.Repeat;
			GetComponent<Renderer>().material.SetTexture ("_MainTex", textureApply);
			_guiText = GameObject.Find ("backgroundImage").GetComponent<GUITexture>();
		}
		mc = GameObject.Find("maskCam").GetComponent<MaskCreator>();
		}
		else
		{
			string originalShaderName = transform.GetComponent<Renderer>().material.shader.name;
			if (originalShaderName.Contains("inverse"))
			{
				Shader shad = (Shader)Resources.Load("shaders/"+"Pointcube_maskInverse"); 
				transform.GetComponent<Renderer>().material.shader = shad;
			}
			else
			{
				Shader shad = (Shader)Resources.Load("shaders/"+"Pointcube_mask"); 
				transform.GetComponent<Renderer>().material.shader = shad;
			}
		}
		
	}

	void Update ()
	{
		if(usefullData.lowTechnologie)
		{
			if (GetComponent<Renderer>().isVisible) {
				if(_guiText==null)
					_guiText = GameObject.Find ("backgroundImage").GetComponent<GUITexture>();
				if (_guiText != null) {
				//	Debug.Log ("_guiText : " +_guiText.name);
					
					if (_guiText.texture != null && mc != null) 
					{
						
						Texture texture = mc.getMask();// _guiText.texture;
						if ( (textureApply==null) || (texture != null && textureApply.GetHashCode () != texture.GetHashCode ()) 
							|| (GetComponent<Renderer>().material.mainTexture==null) )
						{
							textureApply = texture;
							if(textureApply!=null)
							{
								textureApply.wrapMode = TextureWrapMode.Repeat;
							}
							GetComponent<Renderer>().material.SetTexture ("_MainTex", textureApply);
					//		Debug.Log ("texture changed : "+renderer.material.GetTexture("_MainTex").GetHashCode());
						}
						Rect inset = _guiText.pixelInset;
						if(!mc.IsMode2D())
						{
							width = mc.GetSize();///inset.width;
							height = mc.GetSize();//inset.height;
							offsetX = 0;//nset.x;
							offsetY = 0;//inset.y;	
						}
						else
						{
							_factor = 1.0f-mc.GetFactor()*2;
							width = 1024*_factor;///inset.width;
							height = 768*_factor;//inset.height;
							offsetX = (width-1024)/2;//inset.x;
							offsetY = (height-768)/2;//inset.y;	
							
						}
						
					}
					//}
				}
				if(Camera.main)
				{
					float camFov=Camera.main.fieldOfView;
					if(_cameraFrustrum!=null)
						if(_cameraFrustrum.enabled)
							camFov = _cameraFrustrum.GetFov();
				if(((!_oldPos.Equals(transform.position))||
				   (!_oldQuat.Equals(transform.rotation))||
				   (!Camera.main.transform.rotation.Equals(_oldCamQuat))||
				   (!Camera.main.transform.position.Equals(_oldCamPos))||
				   (!camFov.Equals(_oldFieldOfView)||
					(_oldMode2D != mc.IsMode2D()))
				   )
					||
					(mc.IsMode2D() && (_oldFactor!=_factor)))
				   {	
						UpdateMapping();
					}
					_oldMode2D = mc.IsMode2D();
				}
				//Debug.Log ("textureApply width : " + width + ", height : " + height + ", offsetX : " + offsetX + ", offsetY : " + offsetY);
			}
		}
		
	}
	
	void UpdateMapping()		
	{
		if(!usefullData.lowTechnologie)
			return;
		//Debug.Log ("texture updated");
		applyPlanarMapping (width, height, offsetX, offsetY, Camera.main);
		_oldPos = transform.position;
		_oldQuat = transform.rotation;
		_oldCamQuat = Camera.main.transform.rotation;
		_oldCamPos = Camera.main.transform.position;
		float newCamFov = Camera.main.fieldOfView;
		if(_cameraFrustrum!=null)
			if(_cameraFrustrum.enabled)
				newCamFov = _cameraFrustrum.GetFov();
		_oldFieldOfView = newCamFov;
		_oldFactor=_factor;
	}
	
	//-----------------------------------------------------
    void OnDestroy()
    {
    //   UsefullEvents.OnResizeWindowEnd -= Update;
        UsefullEvents.OnResizeWindowEnd -= UpdateMapping;
    }	
	
	void applyPlanarMapping (float imageWidth, float imageHeight, float _offsetX, float _offsetY, Camera mainCamera)
	{
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		Vector3[] vertices = mesh.vertices;
		Vector2[] uvs = mesh.uv;
		int count = mesh.vertexCount;
		//Debug.Log("mask count : "+count);
		
		if (vertices.Length != 0) {
			for (int i = 0; i < count; i++) {
				Vector3 vertexPos = vertices[i];
				//	Debug.Log("mask applyPlanarMapping count : "+count);
				if (vertexPos == null)
					return;
				//	Debug.Log("mask applyPlanarMapping vertexPos : ");
				if (mainCamera == null)
					return;
				//	Debug.Log("mask applyPlanarMapping mainCamera : ");
				vertexPos = localToWorld (vertexPos, mesh);
				Vector3 screenPos = mainCamera.WorldToScreenPoint (vertexPos);
				
				// calcul des nouvelles coordonées de texture
				float xT = (screenPos.x + _offsetX) / imageWidth;
				float yT = (screenPos.y + _offsetY) / imageHeight;
				uvs[i] = new Vector2 (xT, yT);
			}
		}
		mesh.uv = uvs;
	}

	Vector3 localToWorld (Vector3 vect, Mesh mesh)
	{
		//return transform.localToWorldMatrix.MultiplyVector(vect);
		Quaternion WorldRotation = transform.rotation;
		Vector3 multVector = Vector3.Scale (vect, transform.lossyScale);
		Vector3 WorldTranslation = transform.position;
		return ((WorldRotation * multVector) + WorldTranslation);
	}
}

