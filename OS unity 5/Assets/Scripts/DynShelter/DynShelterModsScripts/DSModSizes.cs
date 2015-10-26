using UnityEngine;
using System.Collections;

public class DSModSizes: MonoBehaviour,IDSMod
{
	
	//---------vv--Publics--vv-------------
	public string localRefName;
	public OrigamiRuleV2[] displayRules;
	
	public Transform[] UNodes;
	
	public float heightDelta;
	
	public float widthStep = 0.1f;
	public float heightStep = 0.1f;
		
	//vv--IDSMod--vv
	public bool 		isGlobal;					//configuration appliqué a tout les Mod du meme HTag
	public bool			isIntegrated = true;
	public int 			id;
	
	//---------vv--Privates--vv-------------
	
	private Vector3[] _UOrigins;
	
	private Transform _rulesLocalRef;
	
	private float _widthVal,_heightVal;
	private float _tgtWidth,_tgtHeight;
	bool applyRules = false;
	
	private Animation _anim;
	private AnimationState _animState;
	
	private bool _animLock = false;
	
	//vv--IDSMod--vv
	private DynShelterModManager _modsManager;
	
	//---------vv--Functions--vv-------------
	
	void Awake()
	{
		_UOrigins = new Vector3[UNodes.Length];
		
		foreach(Transform t in transform.GetComponentsInChildren<Transform>(true))
		{
			if(t.name == localRefName)
				_rulesLocalRef = t;
		}
		
		int index=0;
		foreach(Transform t in UNodes)
		{
			_UOrigins[index]  = t.localPosition;			
			index ++;
		}
		
		foreach(OrigamiRuleV2 r in displayRules)
		{
			r.ApplyRuleDisplay(Show,transform,_rulesLocalRef);
		}
		
		_widthVal = _heightVal = 0f;
		_tgtWidth = _tgtHeight = 0f;
		
		//Récuperation de lanimation
		_anim = GetComponent<Animation>();
		_animState = _anim.GetComponent<Animation>()["Take 001"];

	}
	
	void Update()
	{
		//Reglage largeur
		if(_tgtWidth != _widthVal)
		{
			_widthVal = _tgtWidth;
			
			_animState.enabled = true;
			_anim.Play();
			_animState.time = _widthVal * _animState.length ;
			
			applyRules = true;
		}
		else
		{
			if(_anim.isPlaying)
			{
				_anim.Stop();
				_animLock = false;
			}
		}
		
		//Reglage hauteur
		if(_tgtHeight != _heightVal)
		{
			_heightVal = _tgtHeight;
			ApplyHeight();
			applyRules = true;
		}
		
		//Application des règles d'affichage
		if(applyRules)
		{
			foreach(OrigamiRuleV2 r in displayRules)
			{
				r.ApplyRuleDisplay(Show,transform,_rulesLocalRef);
			}
		}
	}
	
	/*void OnGUI()
	{
		bool applyRules = false;
		//Slider reglage Largeur
		float tmp = GUI.HorizontalSlider(new Rect(50,50+debugUIDelta,500,30),_widthVal,0f,1f);
		if(tmp != _widthVal)
		{
			_widthVal = tmp;
			
			_anim.animation["Take 001"].enabled = true;
			_anim.Play();
			_anim.animation["Take 001"].time = _widthVal * _anim.animation["Take 001"].length ;
			
			applyRules = true;
		}
		else
		{
			if(_anim.isPlaying)
			{
				_anim.Stop();
			}
		}
		
		//Slider reglage hauteur
		float hTmp = GUI.VerticalSlider(new Rect(50 + debugUIDelta,100,30,500),_heightVal,1f,0f);
		if(hTmp != _heightVal)
		{
			_heightVal = hTmp;
			ApplyHeight();
			applyRules = true;
		}
		
		if(applyRules)
		{
			//Application des règles d'affichage
			foreach(OrigamiRuleV2 r in displayRules)
			{
				r.ApplyRuleDisplay(Show);
			}
		}
	}*/
	
	private void ApplyHeight()
	{
		for(int u=0;u<UNodes.Length;u++)//y+
		{
			Vector3 t = UNodes[u].localPosition;
			t.y = _UOrigins[u].y + _heightVal * heightDelta;
			UNodes[u].localPosition = t;
		}
	}
	
	//Fonction d'affichage utilisé dans les Rules
	public void Show(GameObject g, bool b)
	{
		g.GetComponent<Renderer>().enabled = b;
	}
	
	//Fonction de positionnement utilisé dans les Rules
	public void UpdatePos(GameObject obj,GameObject orig,Vector3 axis)
	{
		Vector3 newPos = obj.transform.localPosition;
		
		if(axis.x != 0)
		{
			newPos.x = orig.transform.localPosition.x;
		}
		if(axis.y != 0)
		{
			newPos.y = orig.transform.localPosition.y;
		}
		if(axis.z != 0)
		{
			newPos.z = orig.transform.localPosition.z;
		}
		
		obj.transform.localPosition = newPos;
	}
	
	public void SetH(float h)
	{
		_tgtHeight = h;	
	}
	
	public void SetW(float w)
	{
		_tgtWidth = w;	
	}
	
	//---------vv--Functions IDSMod--vv-------------
	public void GetModUI()
	{
//		Debug.Log("ANIMLOCK>"+_animLock);
		//Largeur
		GUILayout.BeginHorizontal("outil",GUILayout.Height(50),GUILayout.Width(260));
		GUILayout.FlexibleSpace();
		if(GUILayout.RepeatButton("+","move+",GUILayout.Height(50),GUILayout.Width(50)) /*&& !_animLock*/)
		{
			_animLock = true;
			_tgtWidth = Mathf.Clamp(_tgtWidth + widthStep,0f,1f);
			SetToAll();
		}
		
		GUILayout.Label(TextManager.GetText("DynShelter.changeWidth")/*+" "+((int)(_widthVal*100)).ToString()+"%"*/,GUILayout.Height(50),GUILayout.Width(100));
		
		if(GUILayout.RepeatButton("-","move-",GUILayout.Height(50),GUILayout.Width(50)) /*&& !_animLock*/)
		{
			_animLock = true;
			_tgtWidth = Mathf.Clamp(_tgtWidth - widthStep,0f,1f);
			SetToAll();
		}
		
		GUILayout.Space(20);
		GUILayout.EndHorizontal();
		
		//Hauteur
		GUILayout.BeginHorizontal("outil",GUILayout.Height(50),GUILayout.Width(260));
		GUILayout.FlexibleSpace();
		if(GUILayout.RepeatButton("+","move+",GUILayout.Height(50),GUILayout.Width(50)))
		{
			_tgtHeight = Mathf.Clamp(_tgtHeight + heightStep,0f,1f);
			SetToAll();
		}
		
		GUILayout.Label(TextManager.GetText("DynShelter.changeHeight")/*+" "+((int)(_heightVal*100)).ToString()+"%"*/,GUILayout.Height(50),GUILayout.Width(100));
		
		if(GUILayout.RepeatButton("-","move-",GUILayout.Height(50),GUILayout.Width(50)))
		{
			_tgtHeight = Mathf.Clamp(_tgtHeight - heightStep,0f,1f);
			SetToAll();
		}
		
		GUILayout.Space(20);
		GUILayout.EndHorizontal();
	}
	
	public void ApplyConf(ArrayList conf,bool reset = false)
	{
		_tgtWidth = (float) conf[0];
		if(reset)
			_widthVal = 0;
		
		_tgtHeight = (float) conf[1];
//		_heightVal = 0;
	}
	
	public void SetToAll(bool reset = false)
	{
		ArrayList conf = new ArrayList();
		conf.Add(_tgtWidth);	//0
		conf.Add(_tgtHeight);	//1
		
		_modsManager.ApplyGlobal(GetHashTag(),conf,reset);
	}
	
	public void SetModManger(DynShelterModManager mgr)
	{
		_modsManager = mgr;	
	}
	
	public string GetHashTag(){return GetType().ToString()+"_"+(isGlobal? "G":"L")+"_"+id.ToString();}// _hashTag;}
	
	public bool IsGlobalMod(){return isGlobal;}
	public bool IsIntegrated(){return isIntegrated;}	
	
	public string SaveConf ()
	{
		string s;
		s = _tgtWidth.ToString()+"#"+_tgtHeight.ToString();
		return s;
	}
	
	public void LoadConf(string conf)
	{
		string[] s = conf.Split('#');
		
		_tgtWidth = float.Parse(s[0]);
		_tgtHeight = float.Parse(s[1]);
	}
	
	public string GetGameObj(){return gameObject.name;}
}

[System.Serializable]
public class OrigamiRuleV2
{
	public bool getTestParentByName = false;
	public string TestParentName;
	public bool getActionParentByName = false;
	public string ActionParentName;
	
	public Transform objTestParent;
	public Transform[] objsTest;

	public Transform objActionParent;
	public Transform[] objsAction;
	
	public bool useLocalRef;
	public bool localPos;
	
	public Vector3 axis;
	public Vector3 values;
		
	public Rule condition;
	
	public delegate void Display(GameObject g,bool b);
	
	public enum Rule
	{
		sup,
		supEq,
		inf,
		infEq,
		eq,
		diff
	}
	
	public void ApplyRuleDisplay(Display action,Transform rootNode,Transform localRef)
	{	
		if(objsAction.Length == 0 || objsTest.Length == 0)
		{
			if(getTestParentByName || getActionParentByName)
			{
				foreach(Transform t in rootNode.GetComponentsInChildren<Transform>(true))
				{
					if(getTestParentByName)
					{
						if(t.name == TestParentName)
							objTestParent = t;
					}
					
					if(getActionParentByName)
					{
						if(t.name == ActionParentName)
							objActionParent = t;
					}
				}
				
				if(objTestParent == null)
					Debug.LogError("DIDNT FOUND objTestParent" + TestParentName);
				
				if(objActionParent == null)
					Debug.LogError("DIDNT FOUND objActionParent" + ActionParentName);
			}
			
			if(objTestParent != null)
			{
				objsTest = new Transform[objTestParent.GetChildCount()];
				int indexer = 0;
				foreach(Transform t in objTestParent.GetComponentsInChildren<Transform>(true))
				{
					if(t != objTestParent && t.parent == objTestParent)
					{
						objsTest[indexer] = t;
						indexer ++;
					}
				}
			}
			if(objActionParent != null)
			{
//				bool tmpB = objActionParent.gameObject.activeSelf;
//				objActionParent.gameObject.SetActive(true);
				
				objsAction = new Transform[objActionParent.GetChildCount()];
				int indexer = 0;
				foreach(Transform t in objActionParent.GetComponentsInChildren<Transform>(true))
				{
					if(t != objActionParent && t.parent == objActionParent)
					{
						objsAction[indexer] = t;
						indexer ++;
					}
				}
//				objActionParent.gameObject.SetActive(tmpB);
			}
		}

		for(int i=0;i<objsTest.Length;i++)
		{
			if(useLocalRef)
				action(objsAction[i].gameObject,RuleTest(objsTest[i],localRef));
			else
				action(objsAction[i].gameObject,RuleTest(objsTest[i],null));
		}
	}
	
	private bool RuleTest(Transform t,Transform localRef)
	{
		bool test = true;
		switch(condition)
		{
		case Rule.sup:
			if(axis.x != 0)
			{
				if(localPos)
					test &= t.localPosition.x > values.x;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).x > values.x;	
				}
				else
					test &= t.position.x > values.x;
			}
			if(axis.y != 0)
			{
				if(localPos)
					test &= t.localPosition.y > values.y;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).y > values.y;	
				}
				else
					test &= t.position.y > values.y;
			}
			if(axis.z != 0)
			{
				if(localPos)
					test &= t.localPosition.z > values.z;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).z > values.z;	
				}
				else
					test &= t.position.z > values.z;
			}
			break;
		case Rule.supEq:
			if(axis.x != 0)
			{
				if(localPos)
					test &= t.localPosition.x >= values.x;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).x >= values.x;	
				}
				else
					test &= t.position.x >= values.x;
			}
			if(axis.y != 0)
			{
				if(localPos)
					test &= t.localPosition.y >= values.y;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).y >= values.y;	
				}
				else
					test &= t.position.y >= values.y;
			}
			if(axis.z != 0)
			{
				if(localPos)
					test &= t.localPosition.z >= values.z;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).z >= values.z;	
				}
				else
					test &= t.position.z >= values.z;
			}
			break;
		case Rule.inf:
			if(axis.x != 0)
			{
				if(localPos)
					test &= t.localPosition.x < values.x;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).x < values.x;	
				}
				else
					test &= t.position.x < values.x;
			}
			if(axis.y != 0)
			{
				if(localPos)
					test &= t.localPosition.y < values.y;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).y < values.y;	
				}
				else
					test &= t.position.y < values.y;
			}
			if(axis.z != 0)
			{
				if(localPos)
					test &= t.localPosition.z < values.z;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).z < values.z;	
				}
				else
					test &= t.position.z < values.z;
			}
			break;
		case Rule.infEq:
			if(axis.x != 0)
			{
				if(localPos)
					test &= t.localPosition.x <= values.x;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).x <= values.x;	
				}
				else
					test &= t.position.x <= values.x;
			}
			if(axis.y != 0)
			{
				if(localPos)
					test &= t.localPosition.y <= values.y;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).y <= values.y;	
				}
				else
					test &= t.position.y <= values.y;
			}
			if(axis.z != 0)
			{
				if(localPos)
					test &= t.localPosition.z <= values.z;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).z <= values.z;	
				}
				else
					test &= t.position.z <= values.z;
			}
			break;
		case Rule.eq:
			if(axis.x != 0)
			{
				if(localPos)
					test &= t.localPosition.x == values.x;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).x == values.x;	
				}
				else
					test &= t.position.x == values.x;
			}
			if(axis.y != 0)
			{
				if(localPos)
					test &= t.localPosition.y == values.y;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).y == values.y;	
				}
				else
					test &= t.position.y == values.y;
			}
			if(axis.z != 0)
			{
				if(localPos)
					test &= t.localPosition.z == values.z;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).z == values.z;	
				}
				else
					test &= t.position.z == values.z;
			}
			break;
		case Rule.diff:
			if(axis.x != 0)
			{
				if(localPos)
					test &= t.localPosition.x != values.x;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).x != values.x;	
				}
				else
					test &= t.position.x != values.x;
			}
			if(axis.y != 0)
			{
				if(localPos)
					test &= t.localPosition.y != values.y;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).y != values.y;	
				}
				else
					test &= t.position.y != values.y;
			}
			if(axis.z != 0)
			{
				if(localPos)
					test &= t.localPosition.z != values.z;
				else if(localRef != null)
				{
					test = localRef.InverseTransformPoint(t.position).z != values.z;	
				}
				else
					test &= t.position.z != values.z;
			}
			break;
		}
		
		return test;
	}
	
}
