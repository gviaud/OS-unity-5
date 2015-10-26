using UnityEngine;
using System.Collections;
using System.IO;

public class LightPresets/* : MonoBehaviour */{
	
	LightConfiguration lc;
	
//	bool showSaveDialog = false;
	
//	string presetName = "";
	
//	public string destinationFinale = "C:/Documents and Settings/yguille/Bureau/Presets/";
	
	public struct Preset
	{
		public string p_name;
		public float l_intensity;
		public float s_blur;
		public float s_power;
		public float r_id;//savoir si reflex ou pas
		public float r_rate;
		public float imgBackground_r;
		public float imgBackground_g;
		public float imgBackground_b;
		public int id;
		
		public void savePreset(string name, string dest)
		{
			p_name = name;
			
			string tosave = "Preset : "+ p_name+" ### ";
			tosave += "intensité = "+l_intensity+" # ";
			tosave += "shadow blur = "+s_blur+" # ";
			tosave += "shadow power = "+s_power+" # ";
			tosave += "reflex id = "+r_id+ " # ";
			tosave += "reflex rate = "+r_rate+" # ";
			/*tosave += "imgBackground_r = "+imgBackground_r+" # ";
			tosave += "imgBackground_g = "+imgBackground_g+" # ";
			tosave += "imgBackground_b = "+imgBackground_b+" # ";*/
			
			File.WriteAllText(dest+name+".txt",tosave);
		}
		
		public void prePrintIt()
		{
			string tosave = "Preset : "+ p_name+"\n";
			tosave += "intensite = "+l_intensity+" # ";
			tosave += "shadow blur = "+s_blur+" # ";
			tosave += "shadow power = "+s_power+" # ";
			tosave += "reflex id = "+r_id+" # ";
			tosave += "reflex rate = "+r_rate+" # ";
			/*tosave += "imgBackground_r = "+imgBackground_r+" # ";
			tosave += "imgBackground_g = "+imgBackground_g+" # ";
			tosave += "imgBackground_b = "+imgBackground_b+" # ";*/
			
			Debug.Log(tosave);
		}
	}
	
	Preset currentPreset;
	
	ArrayList presetsList = new ArrayList();
	
	public LightPresets(LightConfiguration l)
	{
		lc = l;
		
		/*Preset p1 = new Preset();
		p1.p_name = "LightPresets.Inside";
		p1.l_intensity = 0.2f;
		p1.s_blur = 3f;
		p1.s_power = 0.9f;
		p1.r_id = 1;
		p1.r_rate = 0;
		p1.imgBackground_r = 128;
		p1.imgBackground_g = 128;
		p1.imgBackground_b = 128;
		p1.id = 0;*/
		
		Preset p2 = new Preset();
		p2.p_name = "LightPresets.LightingOutdoor";
		p2.l_intensity = 0.3f;
		p2.s_blur = 2.8f;
		p2.s_power = 0.4f;
		p2.r_id = 6;
		p2.r_rate = 0;
		p2.imgBackground_r = 128;
		p2.imgBackground_g = 128;
		p2.imgBackground_b = 128;
		p2.id = 1;
		
		Preset p3 = new Preset();
		p3.p_name = "LightPresets.CloudyOutdoor";
		p3.l_intensity = 0.2f;
		p3.s_blur = 6f;
		p3.s_power = 0.8f;
		p3.r_id = 6;
		p3.r_rate = 0;
		p3.imgBackground_r = 128;
		p3.imgBackground_g = 128;
		p3.imgBackground_b = 128;
		p3.id = 2;
		
		Preset p4 = new Preset();
		p4.p_name = "LightPresets.Night";
		p4.l_intensity = 0.05f;
		p4.s_blur = 6f;
		p4.s_power = 1.0f;
		p4.r_id = 6;
		p4.r_rate = 0;
		p4.imgBackground_r = 33;
		p4.imgBackground_g = 44;
		p4.imgBackground_b = 80;	
		p4.id = 3;
		
		//presetsList.Add(p1);
		presetsList.Add(p2);
		presetsList.Add(p3);
		presetsList.Add(p4);
		
	}
	
// vv used to create presets vv	
//	
//	// Use this for initialization
//	void Start ()
//	{
//		lc = GameObject.Find("LightPivot").GetComponent<LightConfiguration>();
//	}
//	
//	// Update is called once per frame
//	void Update ()
//	{
//	
//	}
//	
//	void OnGUI()
//	{
//		if(GUI.Button(new Rect(0,Screen.height-100,100,40),"SavePreset"))
//		{
//			if(!showSaveDialog)
//			{
//				showSaveDialog = true;
//				makePreset();
//			}
//			else
//			{
//				showSaveDialog = false;	
//			}
//		}
//		
//		if(showSaveDialog)
//		{
//			presetName = GUI.TextField(new Rect(10,Screen.height-50,150,40),presetName);
//			if(GUI.Button(new Rect(160,Screen.height-50,100,40),"Save") && presetName != "")
//			{
//				currentPreset.savePreset(presetName,destinationFinale);
//				showSaveDialog = false;
//			}
//		}
//	}

	//Private's
//	void makePreset()
//	{
//		currentPreset = new Preset();
//		currentPreset.l_intensity = Montage.sm.getLightData().m_i;
//		currentPreset.s_blur = Montage.sm.getShadowData().m_b;
//		currentPreset.s_power = Montage.sm.getShadowData().m_p;
//		currentPreset.r_id = Montage.sm.getReflexData().m_id;
//		currentPreset.r_rate = Montage.sm.getReflexData().m_rate;
//		
//		currentPreset.prePrintIt();
//	}
	
	//Public's
	
	public Preset getPreset(int i)
	{
		return(Preset)presetsList[i];	
	}

	public ArrayList getPresetsList()
	{
		return presetsList;	
	}
		
}
