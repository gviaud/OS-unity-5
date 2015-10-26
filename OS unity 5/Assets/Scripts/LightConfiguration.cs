using UnityEngine;
using System.Collections;

using Pointcube.Global;

public class LightConfiguration : MonoBehaviour
{
    // -- Références scène --
//    public  GameObject m_iOSshadowsFloor;
//    public  GameObject m_iOSshadows;

	private GameObject sun;
	private GameObject ambient;
	private GameObject refelxObj;
	
	private GameObject backgroundImage;
	private GUITexture _guiTextureBackground;
	private GameObject eraserImages;
	private EraserMask eraserMask;
	
//	private float test = 0.0f;
//	private bool plus = true;
	
	public float m_hourDefault 	    = 45f;
	public float m_azimutDefault    = 243f;
	float m_intensityDefault 		= 0.3f;//0.4375f;
    public float m_blurDefault      = 0;
    public float m_powerDefault     = 0.5f;//0.25f;
    public float m_realShadowPower	= 0;
    public float m_realShadowBlur	= 0.5f;//0.25f;


    private const float c_hourThresold = 12f; // Pour limiter la longueur des ombres et éviter les ombres qui
                                              // dépassent dans le mauvais sens à cause du projecteur+flou (iOS)
    private const float c_hourMax   = 90-c_hourThresold;
    private const float c_hourMin   = -90+c_hourThresold;
    private const float c_intMin    = 0f;
    private const float c_intMax    = 1f;
    private const float c_blurMin   = 0f;
    private const float c_blurMax   = 6f;
	
	private const float c_lightIntensityMax = 0.9f;//0.875f;
	private const float c_lightIntensityMin = 0f;
	
	private float	_lastIntensity	=	0.3f;
	
	//Light
	public float m_hour;
	public float m_azimut;
	private float m_intensity;
	//Shadow
    private float m_blur;
    private float m_power;
	//Image Background
    private float m_imgBackground_r = 128;
    private float m_imgBackground_g = 128;
    private float m_imgBackground_b = 128;

	//Reflexion
	ArrayList r_presets = new ArrayList();
	private float r_slider = 0.5f;
	
	private int r_presetId = -1;
	
	private Color r_start = new Color(0,0,0,0);
	private Color r_end = new Color(0,0,0,0);
	
	
	
	public GUISkin skin;
	public bool m_iosShadowsActive = false;
	private GameObject _iosShadows;
	private GameObject _floorShadows;
	private GameObject _floorLight;
	
	//Compass
	bool m_isCompassActive =false;
	float m_compass = 0;
	private float Offset = -90.0f;
	private float orientationTemp = 0.0f;
	private float m_sceneOrientation = 0.0f;
	
	//Presets
	LightPresets m_presets;
	LightPresets.Preset _currentPreset;

    // -- Debug --
    private static readonly string DEBUGTAG = "LightConfiguration : ";
    private static readonly bool   DEBUG    = true;
	
	//--vv--UI--vv--
	private bool _showOrientationUI = false;
	private Matrix4x4 _uiOrientation = new Matrix4x4();
	
	public Texture2D _orientationBG;
	public Texture2D _orientationArrow;
	public Texture2D _orientationOverlay;
	public GUIStyle _NSEWStyle;
	
	private Rect _orientationUI;
	private Rect _rN = new Rect(-10,-25 -20,20,20);
	private Rect _rS = new Rect(-10,25,20,20);
	private Rect _rE = new Rect(25,-10,20,20);
	private Rect _rW = new Rect(-25-20,-10,20,20);
	
	public GameObject sunSpecMatin;
	public GameObject sunSpecAprem;

	#region function_unity
	//-----------------------------------------------------
	void Start ()
    {
//        if(m_iOSshadowsFloor == null)      Debug.LogError(DEBUGTAG+"iosShadowsFloor"+PC.MISSING_REF);
//        if(m_iOSshadows      == null)      Debug.LogError(DEBUGTAG+"iosShadows"+MISSING_REF);
		_iosShadows = GameObject.Find("iosShadows");
		_floorShadows = GameObject.Find("floorShadows");
		_floorLight= GameObject.Find("floorLight");
		m_presets = new LightPresets(this);
		backgroundImage = GameObject.Find("backgroundImage");
		if(backgroundImage!=null)
			_guiTextureBackground = backgroundImage.GetComponent<GUITexture>();

		sunSpecMatin = GameObject.Find ("SpecLight (1)");
		sunSpecAprem = GameObject.Find ("SpecLight");
		sunSpecMatin.SetActive (false);

		eraserImages = GameObject.Find("eraserImages");
		if(eraserImages!=null)
			eraserMask = eraserImages.GetComponent<EraserMask>();
		
		sun = GameObject.Find("sunLight");

		ambient = GameObject.Find("LightAmbient");
        refelxObj = GameObject.Find("collisionPlane");
		
		//Reflexion
		//Sol Clair
		r_presets.Add(new Color(1, 1, 1, 0.082f));
		r_presets.Add(new Color(1, 0.941f, 0.843f, 0.251f));
		
		//Sol Moyen
		r_presets.Add(new Color(1, 0.941f, 0.843f, 0.082f));
		r_presets.Add(new Color(0.784f, 0.741f, 0.663f, 0.439f));
		
		//Sol Sombre
		r_presets.Add(new Color(0.824f, 0.824f, 0.824f, 0.027f));
		r_presets.Add(new Color(0.439f, 0.408f, 0.369f, 0.439f));
		
		//NoReflex
		r_presets.Add(new Color(0, 0, 0, 0));

		//-----------------------------------
		
        SetLightConfig(true);
		
		Montage.sm.AddLightState(m_azimut,m_hour,m_intensity,m_blur,m_power,
			m_imgBackground_r, m_imgBackground_g, m_imgBackground_b);
		
		UpdateOrientationUI();		
		
		if(usefullData.lowTechnologie)
		{	
			if(sun!=null)
				sun.GetComponent<Light>().shadows = LightShadows.None;
			if(_floorShadows!=null)
				_floorShadows.SetActive(false);
			if(_floorLight!=null)
				_floorLight.SetActive(false);
			if(refelxObj!=null)
				refelxObj.GetComponent<Renderer>().enabled = false;
		}
		
	} // Start()
	
	//-----------------------------------------------------
	void Update ()
	{	
		if(m_hour<0)
		{
			sunSpecAprem.GetComponent<Light>().cullingMask = 0 << 4 | 1 << 28 | 1 << 29 | 1 << 30;

			if(!sunSpecMatin.activeInHierarchy)
				sunSpecMatin.SetActive(true);
		}
		else
		{
			sunSpecAprem.GetComponent<Light>().cullingMask = 1 << 4  | 1 << 28 | 1 << 29 | 1 << 30;
			
			if(sunSpecMatin.activeInHierarchy)
				sunSpecMatin.SetActive(false);
		}
	}
	
	void OnEnable()
	{
		UsefullEvents.NightMode+=SetNightMode;
		UsefullEvents.UpdateUIState += UIUpdated;
	}
	
	void OnDisable()
	{
		UsefullEvents.NightMode-=SetNightMode;
		UsefullEvents.UpdateUIState -= UIUpdated;
	}
	#endregion
	
	public void ShowOrientation(bool show)
	{
		_showOrientationUI = show;
	}
	public bool IsOrientationShown()
	{
		return _showOrientationUI;
	}
	
	private void UIUpdated(string ui,int[] indexs)
	{
		if(ui == "GUIMenuRight" && indexs.Length > 0)
		{
			if(indexs[0] == 1 && indexs[1] == 0 && indexs[2] == 1)
			{
				_showOrientationUI = true;
			}
			else if(_showOrientationUI)
				_showOrientationUI = false;
		}
	}
	
	void OnGUI()
	{
		if(_showOrientationUI)
		{
			GUI.depth = -10;
			Rect DynRect = new Rect(-_orientationUI.width/2,-_orientationUI.height/2,_orientationUI.width,_orientationUI.height);
			
			if(_orientationBG!=null)
			GUI.DrawTexture(_orientationUI,_orientationBG);
		
			Matrix4x4 bkup = GUI.matrix;
			GUI.matrix = _uiOrientation;
			
			if(_orientationArrow!= null)
				GUI.DrawTexture(DynRect,_orientationArrow);
			GUI.Label(_rN,TextManager.GetText("GUIMenuRight.North"),_NSEWStyle);
			GUI.Label(_rS,TextManager.GetText("GUIMenuRight.South"),_NSEWStyle);
			GUI.Label(_rE,TextManager.GetText("GUIMenuRight.East"),_NSEWStyle);
			GUI.Label(_rW,TextManager.GetText("GUIMenuRight.West"),_NSEWStyle);
			
			GUI.matrix = bkup;
			
			if(_orientationOverlay!=null)
				GUI.DrawTexture(_orientationUI,_orientationOverlay);
		}
	}
	
//	void OnGUI ()
//	{
//		Offset = float.Parse(GUI.TextField(new Rect(200,50,50,50), Offset.ToString()));
//		hourTemp = float.Parse(GUI.TextField(new Rect(250,50,50,50), hourTemp.ToString()));*/
//		GUI.Label(new Rect(130,50,50,20),"Heure");
//		hour = GUI.HorizontalSlider(new Rect(25, 50, 100, 30), hour, -80f, 80f);		
//		setHour(hour);
//		
//		GUI.Label(new Rect(130,90,50,20),"Azimut");
//		azimut = GUI.HorizontalSlider(new Rect(25, 90, 100, 30), azimut, 0.0F, 360f);		
//		setAzimut(azimut);
//		
//		GUI.Label(new Rect(130,130,50,20),"Intensité");
//		intensity = GUI.HorizontalSlider(new Rect(25, 130, 100, 30), intensity, 0.0F, 2f);		
//		setIntensity(intensity);
//	}

    //-----------------------------------------------------
	public void SetLightConfig(bool setToDefault)
	{
		SetLightConfig(setToDefault,true,true);
	}
	
	public void SetLightConfig(bool setToDefault,bool light,bool shadows)
	{
		if(setToDefault)
		{
			if(light)
			{
				m_hour      = m_hourDefault;
				m_azimut    = m_azimutDefault;
				m_intensity = m_intensityDefault;
			}
			
			if(shadows)
			{
				m_blur      = m_blurDefault;
				m_power     = m_powerDefault;
			}

		}
		
		if(light)
		{
			setHour(m_hour);
	        setAzimut(m_azimut);
	        setIntensity(m_intensity);
		}
		
		if(shadows)
		{
//			if(Application.platform !=RuntimePlatform.Android)
//	       		setBlur(m_blur);
	        setPower((/*1 - */m_power/*0.5f*/)*0.43f+0.45f);
			
		}
		if(_guiTextureBackground!=null)
		{
			_guiTextureBackground.color = new Color(
				m_imgBackground_r/255.0f,
				m_imgBackground_g/255.0f,
				m_imgBackground_b/255.0f);
			if(eraserMask!=null)
				eraserMask.UpdateMaskColor(
				_guiTextureBackground.color);
		}
	}

    //-----------------------------------------------------
    public void UndoLight(bool isLight)
    {
//        Debug.Log("UndoLight");
        SceneModel.Ahibp lastState = Montage.sm.UndoLight(isLight);
        m_hour      = lastState.m_h;
        m_azimut    = lastState.m_a;
        m_intensity = lastState.m_i;
        m_blur      = lastState.m_b;
        m_power     = lastState.m_p;

        SetLightConfig(false,isLight,!isLight);
    }
	
	public void RedoLight(bool isLight)
	{
		SceneModel.Ahibp lastState = Montage.sm.RedoLight(isLight);
        m_hour      = lastState.m_h;
        m_azimut    = lastState.m_a;
        m_intensity = lastState.m_i;
        m_blur      = lastState.m_b;
        m_power     = lastState.m_p;

        SetLightConfig(false,isLight,!isLight);	
	}

	//--------------SET's----------------------------------
	
	public void initAzimut(float _azimut)
	{
		m_azimut = _azimut;
		setAzimut(m_azimut);
		UpdateOrientationUI();
	}
	
	public void setAzimut(float f)
	{
        Quaternion q = transform.localRotation;
        
        Vector3 v = q.eulerAngles;
        v.y = Quaternion.AngleAxis(f, Vector3.up).eulerAngles.y;
		q.eulerAngles = v;
		transform.localRotation = q;
		
		v.y += 180;
		v.x = 90-v.x;
        q.eulerAngles = v;
//        if(m_iosShadowsActive) _iosShadows.GetComponent<IosShadowManager>().shadows_rotation = q;
	}
	
	public float getAzimut()
	{
		return m_azimut;
	}
	
	//-----------------------------------------------------
	public void setHour(float f)
	{
        Quaternion q = transform.localRotation;
        
        Vector3 v = q.eulerAngles;
        v.x = Quaternion.AngleAxis(f, Vector3.right).eulerAngles.x;
		q.eulerAngles = v;
        transform.localRotation = q;
		
		v.y += 90;
		v.x = 90-v.x;
        q.eulerAngles = v;
		
//        if(m_iosShadowsActive) _iosShadows.GetComponent<IosShadowManager>().shadows_rotation = q;

	}
	
	//-----------------------------------------------------
	public void setIntensity(float f)
	{
		//sun.light.intensity = f/7;
		//ambient.light.intensity = f; //f/4
		/*
		sun.light.intensity =4*f/5;
		ambient.light.intensity = f/5;*/
	//	_lastIntensity = f;
		sun.GetComponent<Light>().intensity = Mathf.Max(f,0.01f);
		if(ambient!=null)
			ambient.GetComponent<Light>().intensity = Mathf.Max(f/20.0f,0.01f);
		
		float a = -489.74f;
		float b = 680.26f;
		float c = 40.0f;
		float result = a*f*f+b*f+c;
		RenderSettings.ambientLight = new Color(result/255.0f,result/255.0f,result/255.0f);
	}
	
	//-----------------------------------------------------
   /* private void setBlur(float newBlur)
    {
        if(m_iosShadowsActive)
        {	
			_iosShadows.GetComponent<IosShadowManager>().SetBlur(newBlur);
            for(int i=0; i<_iosShadows.transform.GetChildCount(); i++)
                _iosShadows.transform.GetChild(i).GetComponent<BlurEffect>().iterations = (int) newBlur;
        }
    }*/
    
	//-----------------------------------------------------
    private void setPower(float newPower)
    {
  /*      if(m_iosShadowsActive)
        {
			_iosShadows.GetComponent<IosShadowManager>().SetPower(newPower);
            for(int i=0; i<_iosShadows.transform.GetChildCount(); i++)
                _iosShadows.transform.GetChild(i).GetComponent<IosShadow>().SetLightness(newPower);
        }*/
		AdaptShadow();
    }	
	
	private void AdaptShadow()
	{		
		if(_floorShadows!=null)
		{
			Material mat = _floorShadows.transform.GetComponent<Renderer>().material;
			if (mat.HasProperty("_ShadowIntensity"))
			{
				mat.SetFloat("_ShadowIntensity",m_intensity>0.55?0.55f:m_intensity);
				if(sun!=null)
					//sun.light.shadowStrength = m_intensity>0.55?0.55f:m_intensity;
					sun.GetComponent<Light>().shadowStrength = m_intensity>0.1f?Mathf.Min(1.0f,m_intensity+0.2f):0.0f;
			}
		}		
	}
	
	//-----------------------------------------------------
	private void setReflexion(Color c)
	{
		if(refelxObj!=null)
		{
			if (refelxObj.GetComponent<Renderer>().material.HasProperty("_IlluminCol"))
	                refelxObj.GetComponent<Renderer>().material.SetColor("_IlluminCol", c);
			else
				Debug.Log("NO REFLEX SHADER");
		}
	}
	
	public void setReflexionLimits(int i) // 0 > clair, 1 > moyen, 2 > foncé
	{
		if(i == r_presetId)
			return;
		
		if(i>-1 && i < 3)
		{
			r_start = (Color)r_presets[2*i];
			r_end = (Color)r_presets[2*i+1];
			r_presetId = i;
			saveReflexToModel();
		}
		else
		{
			if(i == 6)
			{
				r_start = (Color)r_presets[6];
				r_end = (Color)r_presets[6];
				r_presetId = 6;
				saveReflexToModel();
			}
			else
				Debug.Log("INDEX OUT OF PRESETS LIMITS");
		}
	}
    
	public void setReflex2Zero()
	{
		setReflexionLimits(6);
		setReflexion(new Color(0,0,0,0));
//		setReflexion((Color)r_presets[6]);
//		r_presetId = 6;
		saveReflexToModel();
	}
	
	public string reflexSwitch()
	{
		string ui = "";
		if(r_presetId == 6 || r_presetId == -1)
		{
			setReflexionLimits(1);
			ui = TextManager.GetText("GUIMenuRight.ReflectionDeActivate");
			changeReflex(0);
		}
		else if(r_presetId ==1)
		{
			setReflexionLimits(6);
			setReflexion(new Color(0,0,0,0));
			ui = TextManager.GetText("GUIMenuRight.ReflectionActivate");
		}
		saveReflexToModel();
		return ui;
	}
	
	public string getReflexionState()
	{
		string ui = "";
		if(r_presetId == 6 || r_presetId == -1)
		{
			ui = TextManager.GetText("GUIMenuRight.ReflectionDeActivate");
		}
		else if(r_presetId ==1)
		{
			ui = TextManager.GetText("GUIMenuRight.ReflectionActivate");
		}
		saveReflexToModel();
		return ui;	
	}
	
	public bool isReflexActive()
	{
		bool onOff = true;
		if(r_presetId == 6 || r_presetId == -1)
		{
			onOff = false;
		}
		else if(r_presetId ==1)
			onOff = true;
		return onOff;
	}
	
	//------------CHANGE's--------------------------
	public float changeAzimut(float change)
	{
        if(change != 0)
        {
		    m_azimut = (m_azimut + change)%360;
		    setAzimut(m_azimut);
			UpdateOrientationUI();
        }
		return m_azimut;
	}
	
	public void initHour(float _hour)
	{
		m_hour = _hour;
		setHour(m_hour);
	}
	
	//-----------------------------------------------------
	public float changeHour(float change)
	{
        if(change != 0)
        {
    	    float oldHour = m_hour;
//			Debug.Log("TESTHOUR "+change+" "+m_hour+" "+(m_hour+change));
    		if(m_hour + change > c_hourMax)      m_hour = c_hourMax;
    		else if(m_hour + change < c_hourMin) m_hour = c_hourMin;
			else                                 m_hour += change;


    		setHour(m_hour);
        }
		//return ((m_hour+90)/12.5f)+7;
		//return (m_hour+144.0f)*(6.0f/72.0f);
		
		//y = deltaHeure/deltaAngle * angle + 12
		
		if(m_hour <0f)
			return (m_hour*(4.5f/78f))+12f;
		else
			return (m_hour*(9f/78f))+12f;
	}
	
	// remet à jour l'intensité après un passage en mode nuit par exemple
	public void ResetIntensity()
	{
		m_intensity = _lastIntensity;
		setIntensity(m_intensity);
	}
	
	public float getIntensity()
	{
		return  m_intensity;
	}
	
	public void initIntensity(float _intensity)
	{
		m_intensity = _intensity;
		setIntensity(m_intensity);
		AdaptShadow();
		changeIntensity(0.1f);
	}
	
	//-----------------------------------------------------
	public float changeIntensity(float change)	// TODO prendre en compte la valeur pour faire comme les autres sliders, et adapter le undo/redo
	{
        if(change != 0)
        {
            m_intensity += change;
    
            if(m_intensity > c_lightIntensityMax)      m_intensity = c_lightIntensityMax;
            else if(m_intensity < c_lightIntensityMin) m_intensity = c_lightIntensityMin;
    
			_lastIntensity = m_intensity;
    		setIntensity(m_intensity);
//    		m_power = (1-m_intensity)*0.43f+0.45f; // Donne une valeur comprise entre 0.45 et 0.88
//            setPower(m_power);
        }
		float a = 0.6f;
		float b = 0.3f;
		float c = -m_intensity;
		float delta = b*b-4*a*c;
		float result = 0.0f;
		if (delta<0)
		{
			result=-1.0f;
			//Debug.Log("Delta < 0 result "+result);
		}
		else if (delta == 0)
		{
			result = -b/(2*a);
			//Debug.Log("Delta = 0 result "+result);
		}
		else
		{
			//float x1 = (-b-Mathf.Sqrt(delta))/(2*a);
			float x2 = (-b+Mathf.Sqrt(delta))/(2*a);
			//Debug.Log("Delta > 0 x1 "+x1);
			//Debug.Log("Delta > 0 x2 "+x2);
			//result = x1;	
			//if(x1<0)
				result = x2;			
			//Debug.Log("Delta = 0 result "+result);
		}
		
		//return (m_intensity*(1/c_lightIntensityMax))*100;
		
		AdaptShadow();
		return result*100;
	}

	//-----------------------------------------------------
	public float changeShadowIntensity(float change)
	{
		if(change != 0)
        {
            m_power -= change;
    
            if(m_power > c_intMax)      m_power = c_intMax;
            else if(m_power < c_intMin) m_power = c_intMin;
    
//    		m_power = (1-m_intensity)*0.43f+0.45f; // Donne une valeur comprise entre 0.45 et 0.88
//            setPower(m_power*0.43f+0.45f);
			
			setPower(m_power*0.8f+0.2f);
        }
		return (1-m_power)*100;
	}
	
	//-----------------------------------------------------
/*	public float changeBlur(float change)
	{
        if(change != 0)
        {
            m_blur += change;
    
            if(m_blur + change > c_blurMax)      m_blur = c_blurMax;
            else if(m_blur + change < c_blurMin) m_blur = c_blurMin;
    		if(Application.platform !=RuntimePlatform.Android)
    			setBlur(m_blur);
        }
		return (m_blur /6)*100;
	}*/
	
	//-----------------------------------------------------
	public float changeReflex(float change)
	{
		r_slider = r_slider + change;
		if(r_slider<0)
			r_slider = 0;
		if(r_slider>1)
			r_slider = 1;
				
		Color c = new Color(0,0,0,0);        
		c.r = ((1-r_slider)*r_start.r + r_slider*r_end.r);
		c.g = ((1-r_slider)*r_start.g + r_slider*r_end.g);
		c.b = ((1-r_slider)*r_start.b + r_slider*r_end.b);
		c.a = ((1-r_slider)*r_start.a + r_slider*r_end.a);
		
		setReflexion(c);
		
		return r_slider*100;
		
	}

    //-----------------------------------------------------
	public void loadReflex(float rate , int id)
	{
		r_slider = rate;
		setReflexionLimits(id);
		changeReflex(0);
	}
	
	//-----------------------------------------------------
	public void getGui() //obsolete
	{
//		GUISkin bkup = GUI.skin;
//		GUI.skin = skin;
//		GUI.Label(new Rect(200,10,100,50),"Heure");
//		m_hour = GUI.HorizontalSlider(new Rect(10, 10, 180, 50), m_hour, 0f, 80-c_hourThresold);
//		setHour(80 - m_hour);
//		
//		GUI.Label(new Rect(200,70,100,50),"Orientation");
//		m_azimut = GUI.HorizontalSlider(new Rect(10, 70, 180, 50), m_azimut, 0f, 360f);		
//		setAzimut(m_azimut);
//		
//		GUI.Label(new Rect(200,130,100,50),"Intensite");
//		m_intensity = GUI.HorizontalSlider(new Rect(10, 130, 180, 50), m_intensity, 0f, 0.99f); // 1 -> problème avec le floorlight trop éclairé		
//		setIntensity(m_intensity);
//        
//        GUI.Label(new Rect(200,190,100,50),"Flou");
//        m_blur = (int) GUI.HorizontalSlider(new Rect(10, 190, 180, 50), m_blur, 0f, 6f);
//        setBlur(m_blur);
//        
//        GUI.Label(new Rect(200,165,70,20),"Puissance");
//        m_power = GUI.HorizontalSlider(new Rect(10, 170, 180, 30), m_power, 0f, 1f);
//        m_power = (1-m_intensity)*0.43f+0.45f; // Donne une valeur comprise entre 0.45 et 0.88
//        setPower(m_power);
		
//		Montage.sm.AddLightState(m_azimut,m_hour,intensity,m_blur,m_power);
	}//obsolete
	
	//--------------SAVE n LOAD---------------------------------------
	public void LoadFromModel()
	{
		SceneModel.Ahibp ahipb = Montage.sm.getLightData();
		m_azimut = ahipb.m_a;
		m_hour = ahipb.m_h;
		m_intensity = ahipb.m_i;
    	m_blur = ahipb.m_b;
    	m_power = ahipb.m_p;
		m_imgBackground_r = ahipb.m_img_r;
		m_imgBackground_g = ahipb.m_img_g;
		m_imgBackground_b = ahipb.m_img_b;
		
		SetLightConfig(false);
	}

    //-----------------------------------------------------
	public void SaveToModel()
	{
		SaveToModel(true);
	}
	
    public void SaveToModel(bool isLight)
    {
        Montage.sm.AddLightState(isLight,m_azimut, m_hour, m_intensity, m_blur, m_power,
			m_imgBackground_r, m_imgBackground_g, m_imgBackground_b);
    }
	
	public void saveReflexToModel()
	{
		Montage.sm.setReflex(r_presetId,r_slider);
	}
	
	private void UpdateOrientationUI()
	{
		_orientationUI = new Rect((Screen.width - 150)/2,40,150,150);
		
		Quaternion rot = new Quaternion(0,0,0,0);
		
		rot.eulerAngles = new Vector3(0,0,m_azimut+90);
		
		Vector3 pos = new Vector3((_orientationUI.xMin + _orientationUI.xMax)/2,(_orientationUI.yMin + _orientationUI.yMax)/2,0);
		
		_uiOrientation.SetTRS(pos,rot,Vector3.one);
	}
	
	#region compass
	
	public void setCompassActive(bool b)
	{
		if(b)
		{
			Input.compass.enabled = b;
			m_isCompassActive = b;
		}
		else
		{
			Debug.Log ("Input.compass.magneticHeading : "+ Input.compass.magneticHeading);
			m_compass = -Input.compass.magneticHeading;
			Input.compass.enabled = b;
			m_isCompassActive = b;
			
			setCompass();
//			m_azimut = (m_compass+Offset)%360;
//			setAzimut(m_azimut);
//			
//			float time = System.DateTime.Now.Hour + (System.DateTime.Now.Minute/60);
//			
//			if(time < 7)
//				time = 7;
//			
//			if(time > 19)
//				time = 19;
//			
////			float timeToAngle = ((time-7)*12.5f)-90;
//			
//		//	time = hourTemp;
//			m_hour = ((time-7)*12.5f)-90;
//			setHour(m_hour);
//			
//			SaveToModel();
//			
//		//	Debug.Log ("orientation set to : "+ m_compass + "time : "+time);
		}
	}
	
	private void setCompass()
	{
		//Compass

		float o = Montage.sm.getCamData().m_l;
		m_azimut = (m_compass+Offset+o)%360;
		
		setAzimut(m_azimut);
		
		float time = System.DateTime.Now.Hour + (System.DateTime.Now.Minute/60.0f);
		
		if(time < 7)
			time = 7;
		
		if(time > 19)
			time = 19;
			
			
		//Time
		//m_hour = ((time-7)*12.5f)-90;
		m_hour = (72f/6f)*time - 144;
		setHour(m_hour);
		
		//_iosShadows.GetComponent<IosShadowManager>().UpdateShadowsPos();
		SaveToModel();
	}
	
	#endregion
	
	#region presets
	
	public ArrayList getPresets()
	{
		if(m_presets == null)
			m_presets = new LightPresets(this);
		return m_presets.getPresetsList();
	}
	
	public void applyPreset(int i)
	{
		_currentPreset = m_presets.getPreset(i);
		
		m_intensity = _currentPreset.l_intensity;
		
		m_blur = _currentPreset.s_blur;
		m_power = _currentPreset.s_power;
		
		m_imgBackground_r = _currentPreset.imgBackground_r;
		m_imgBackground_g = _currentPreset.imgBackground_g;
		m_imgBackground_b = _currentPreset.imgBackground_b;
		
		SetLightConfig(false,true,true);
		
//		setReflexionLimits((int)preset.r_id);
		loadReflex(_currentPreset.r_rate,(int)_currentPreset.r_id );
				
		SaveToModel(true);
		SaveToModel(false);
		saveReflexToModel();
		
		if(i==2)
        {
			Montage.sm.SetNightMode(true);
//            m_iOSshadowsFloor.renderer.enabled = false; // TODO "quick-fix biossun" à améliorer
        }
		else
        {
			Montage.sm.SetNightMode(false);
//            m_iOSshadowsFloor.renderer.enabled = true;  // TODO "quick-fix biossun" à améliorer
        }
	}
	
	public LightPresets.Preset GetCurrentPreset()
	{
		return _currentPreset;
	}
	
	public void SetNightMode(bool night)
	{
		if(night)
			applyPreset(2);
		else
		{
			applyPreset(0);		
			ResetIntensity();
		}
	}

    public Color GetBgTexColor()
    {
        return new Color(m_imgBackground_r, m_imgBackground_g, m_imgBackground_b, 1f);
    }
	
	#endregion
	
} // class LightConfiguration
