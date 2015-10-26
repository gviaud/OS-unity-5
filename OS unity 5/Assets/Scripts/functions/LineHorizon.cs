using UnityEngine;
using System.Collections;
using Pointcube.Global;

public class LineHorizon : MonoBehaviour
{
    public Material mat;
	
	public Transform targetGrid;
	
	public GameObject backgroundImage;
	
	private GameObject _helper;
	private GameObject _helperTemp;
	
    private Vector3 startVertex;
	private Vector3 realBeginPoint;
	private Vector3 realEndPoint;
	
    private Quaternion rotationCamera;

    private bool      mShowLine;        // La ligne est masquée pendant un redimensionnement de l'écran
                                        // (sinon elle est mal placée)
	private float sizeY=0,off7Y=0;
	
	private const string DEBUGTAG = "LineHorizon : ";
	
	private float factorReal=2.0f;
	
#region unity_func
    //-----------------------------------------------------
    void Awake()
    {
        mShowLine = true;

        UsefullEvents.OnResizingWindow  += HideLine;
        UsefullEvents.OnResizeWindowEnd += ShowLine;
		UsefullEvents.OnResizeWindowEnd += UpdateScreenValues;
    }
	
    //-----------------------------------------------------
	void Start ()
    {
		if(backgroundImage == null)
			Debug.LogError(DEBUGTAG+"backgroundImage "+PC.MISSING_REF);
		
		if(mat == null)
			Debug.LogError(DEBUGTAG+"mat "+PC.MISSING_REF);
		
	/*	if(targetGrid == null)
			Debug.LogError(DEBUGTAG+"targetGrid "+PC.MISSING_REF);
		else
		{
			_helper = GameObject.Find("lineHorizonHelper");
		}*/
		if(targetGrid == null)
			Debug.LogError(DEBUGTAG+"targetGrid "+PC.MISSING_REF);
		_helper = new GameObject("lineHorizonHelperFixe");
		_helperTemp = new GameObject("lineHorizonHelperMobile");
		
		if(_helper.transform == null)
		{
			_helper.AddComponent<Transform>();
		}
		if(_helperTemp.transform == null)
		{
			_helperTemp.AddComponent<Transform>();
		}
			
		_helper.transform.parent = transform;
		_helperTemp.transform.parent = transform;
		_helper.transform.localPosition = new Vector3(0.0f, -12.8f, 540.0f);
		_helperTemp.transform.localPosition = new Vector3(0.0f, -12.8f, 540.0f);
	}
	
    //-----------------------------------------------------
	void Update ()
	{
		if(mShowLine && targetGrid!=null && _helper!=null)
		{
			if(targetGrid.GetComponent<Renderer>().enabled)
			{				
				Vector3 tmp = Vector3.zero;
				startVertex = Vector3.zero;

				rotationCamera = transform.rotation;				
				
				Vector3 v = rotationCamera.eulerAngles;
				v.y = 0.0f;				
				_helper.transform.localPosition = new Vector3 (
					_helper.transform.localPosition.x,
					-transform.position.y,
					_helper.transform.localPosition.z);
				tmp =Quaternion.AngleAxis(v.x, Vector3.left)*_helper.transform.localPosition;				
				_helperTemp.transform.localPosition = tmp;				
							
				tmp =  Camera.main.WorldToScreenPoint(_helperTemp.transform.position);					
				
				rotationCamera = transform.rotation;
				v = rotationCamera.eulerAngles;
				v.x = 0.0f;
				v.y = 0.0f;				
				startVertex.y = tmp.y;	
			
				float yOrigin = (startVertex.y - off7Y) / sizeY;			
				
				factorReal = backgroundImage.GetComponent<GUITexture>().pixelInset.width / backgroundImage.GetComponent<GUITexture>().pixelInset.height; 
				realEndPoint.Set(1.0f,yOrigin-0.5f*factorReal*Mathf.Tan(Mathf.Deg2Rad * v.z),0.0f);				
				realBeginPoint.Set(0.0f,yOrigin+0.5f*factorReal*Mathf.Tan(Mathf.Deg2Rad * v.z),0.0f);
			
			}
		}
	}
	
    //-----------------------------------------------------
	void OnPreRender()
	{
		if(!mShowLine || targetGrid == null || !targetGrid.GetComponent<Renderer>().enabled || !mat) return;

        GL.PushMatrix();
        mat.SetPass(0);
       	GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(Color.yellow);
        GL.Vertex(realBeginPoint);
        GL.Vertex(realEndPoint);
        GL.End();
       /* GL.Color(Color.red);
        GL.Vertex(realBeginPoint0);
        GL.Vertex(realEndPoint0);
        GL.End();*/
        GL.PopMatrix();
    } 

    //-----------------------------------------------------
    void OnDestroy()
    {
        UsefullEvents.OnResizingWindow  -= HideLine;
        UsefullEvents.OnResizeWindowEnd -= ShowLine;
		UsefullEvents.OnResizeWindowEnd -= UpdateScreenValues;
    }
#endregion

    //-----------------------------------------------------
    private void HideLine()
    {
//        Debug.Log ("============================ Hide");
        mShowLine = false;
    }

    //-----------------------------------------------------
    private void ShowLine()
    {
//        Debug.Log ("=============================== Show");
        mShowLine = true;
    }
	
	//-----------------------------------------------------
	public void UpdateScreenValues()
	{
		sizeY = backgroundImage.GetComponent<GUITexture>().pixelInset.height;
		off7Y = backgroundImage.GetComponent<GUITexture>().pixelInset.y;
	}

} // class LineHorizon
