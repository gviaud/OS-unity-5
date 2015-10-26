using UnityEngine;
using System.Collections;
using System.IO;

public class Function_duck : MonoBehaviour, Function_OS3D 
{
	
	bool isVisible;
	
	public int id;
	
	Transform m_duck;
	
	public string p_prefabToLoad = "animDuck";
	private string _functionName = "Cacher";
	private string _altFunctionName = "Afficher";	
	private string _strObjectToHide = "canard";
	
	// Use this for initialization
	void Start ()
	{
		if(transform.FindChild("water"))
		{
			Transform eau = transform.FindChild("water");
			foreach(Transform t in eau.transform)
			{
				if(t.name.StartsWith("animDuck"))
				{
					m_duck = t.GetChild(0);
					isVisible = m_duck.GetComponent<Renderer>().enabled;
					
//					t.GetChild(0).renderer.enabled = !t.GetChild(0).renderer.enabled;
//				    isVisible = t.GetChild(0).renderer.enabled;
				}
			}
			
			if(m_duck == null)
			{
				// This is the prefab
				GameObject prefab = (GameObject)Resources.Load("animDuck"); 
				// Add the instance in the hierarchy
				GameObject obj = (GameObject)Instantiate(prefab);
	
				// Find the instantiate prefab and asign the parent
				obj.transform.localScale = eau.parent.transform.localScale;
				obj.transform.parent = eau;
				obj.transform.localPosition = new Vector3(0,-0.1f,0);
				
				m_duck = obj.transform.GetChild(0);
				isVisible = m_duck.GetComponent<Renderer>().enabled;
				m_duck.parent.GetComponent<AudioSource>().mute = !isVisible;
			}			
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void DoAction ()
	{
//		if(transform.FindChild("water"))
//		{
//			Transform eau = transform.FindChild("water");
//			foreach(Transform t in eau.transform)
//			{
//				if(t.name.StartsWith("animDuck"))
//				{
//					t.GetChild(0).renderer.enabled = !t.GetChild(0).renderer.enabled;
//				    isVisible = t.GetChild(0).renderer.enabled;
//				}
//			}
//			
//		}
		if(m_duck != null)
		{
			m_duck.GetComponent<Renderer>().enabled = !m_duck.GetComponent<Renderer>().enabled;
			isVisible = m_duck.GetComponent<Renderer>().enabled;
		}
	}
	
	// fcn's privés
	
	//  FCNS obligatoires
	
	public string GetFunctionName()
	{
		if (!isVisible)
			return TextManager.GetText(_altFunctionName);
		else
			return TextManager.GetText(_functionName);
	}
	

	public string GetFunctionParameterName()
	{
		return GetFunctionName()+" "+TextManager.GetText(_strObjectToHide);
	}
	public int GetFunctionId()
	{
		return id;
	}
	
	//  sauvegarde/chargement
	
	public void save(BinaryWriter buf)
	{
		buf.Write(isVisible);
	}
	
	public void load(BinaryReader buf)
	{
		isVisible = buf.ReadBoolean();
		if(transform.FindChild("water"))
		{
			Transform eau = transform.FindChild("water");
			foreach(Transform t in eau.transform)
			{
				if(t.name.StartsWith("animDuck"))
				{
					t.GetChild(0).GetComponent<Renderer>().enabled = isVisible;
				}
			}
			
		}
	}
	
	//Set L'ui si besoin
	
	public void setUI(FunctionUI_OS3D ui)
	{
		
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
	}
	
	//public void setVisible();
	
	//similaire au Save/Load mais utilisé en interne d'un objet a un autre (swap)
	public ArrayList getConfig()
	{
		ArrayList list = new ArrayList();
		list.Add(isVisible);
		return list;
	}
	
	public void setConfig(ArrayList config)
	{
		isVisible = (bool)config[0];
		if(transform.FindChild("water"))
		{
			Transform eau = transform.FindChild("water");
			foreach(Transform t in eau.transform)
			{
				if(t.name.StartsWith("animDuck"))
				{
					t.GetChild(0).GetComponent<Renderer>().enabled = isVisible;
				}
			}
			
		}
	}
}
