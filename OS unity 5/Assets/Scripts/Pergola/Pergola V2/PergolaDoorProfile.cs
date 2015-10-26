using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class PergolaDoorProfile : MonoBehaviour,IPergola
{
	
	public GameObject _BasePrimitive;
	
	private GameObject doors;
	
	private Function_Pergola _fp;
	
	private bool _showDoorProfiles;
		
	// Use this for initialization
	void Awake ()
	{
		_fp = GetComponent<Function_Pergola>();
		
		if(!transform.FindChild("doors"))
		{
			doors = new GameObject("doors");
			doors.transform.parent = this.transform;
			doors.transform.localPosition = Vector3.zero;
			
			doors.AddComponent<MeshRenderer>();
//			doors.renderer.material = _fp.GetMaterial();
		}
		else
		{
			doors =	transform.FindChild("doors").gameObject;
//			doors.renderer.material = _fp.GetMaterial();
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	#region Other Fcn's
	
	void AddPartAt(Vector3 position, Vector3 scale,GameObject p,int i)
	{
		Vector3 floatScl = scale/1000f;
		Vector3 floatPos = position/1000f;
		
		GameObject part = (GameObject) Instantiate(_BasePrimitive);
		part.name = i.ToString();
		part.layer = gameObject.layer;
		
		part.GetComponent<Renderer>().material = _fp.GetMaterial();
		
		part.transform.parent = p.transform;
		part.transform.localPosition = floatPos;
		part.transform.localScale = floatScl;
		part.transform.localRotation = Quaternion.identity;
		
	}
	
	bool UISetDoorProfile(string label,ref bool b)
	{	
		bool hasChange = false;
//		GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
//		GUILayout.FlexibleSpace();
		
		if(b)
		{
			if(GUILayout.Button(TextManager.GetText("Pergola.Hide")+"\n"+label,GUILayout.Width(50),GUILayout.Height(50)))
			{
				b = false;
				hasChange = true;
			}
		}
		else
		{
			if(GUILayout.Button(TextManager.GetText("Pergola.Show")+"\n"+label,GUILayout.Width(50),GUILayout.Height(50)))
			{
				b = true;
				hasChange = true;
			}
		}
		
//		GUILayout.Space(20);
//		GUILayout.EndHorizontal();
		return hasChange;
	}
	
	#endregion
	
	#region Interface Fcn's
	
	public void Build(Function_Pergola.PElement m,BuildDatas bd,PergolaRule pr,int index,int off7)
	{		
		float sizeH = m._h - bd._frameH;
		float posH = sizeH/2;
		
		int sizeL = bd._footSize;
		int sizeW = bd._footSize;
		
		Vector3 size = new Vector3(sizeW,sizeH,sizeL);
		
		if(m._pergolaType ==  Function_Pergola.PergolaType.single)
		{
			if(m._dpW1) 
				AddPartAt(new Vector3(off7+m._w/2,posH,-m._l/2+bd._footSize/2),size,doors,index);
			if(m._dpW2) 
				AddPartAt(new Vector3(off7+m._w/2,posH,m._l/2-bd._footSize/2),size,doors,index);
			if(m._dpL2) 
				AddPartAt(new Vector3(off7+bd._footSize/2,posH,0),size,doors,index);
			if(m._dpL1) 
				AddPartAt(new Vector3(off7+m._w-bd._footSize/2,posH,0),size,doors,index);
		}		
		else if(m._pergolaType == Function_Pergola.PergolaType.CL)
		{
			if(m._dpW1) 
				AddPartAt(new Vector3(off7+m._w/2,posH,-m._l/2+bd._footSize/2),size,doors,index);
			if(m._dpW2) 
				AddPartAt(new Vector3(off7+m._w/2,posH,m._l/2-bd._footSize/2),size,doors,index);
			if(m._dpL2) 
				AddPartAt(new Vector3(off7+bd._footSize/2,posH,0),size,doors,index);
			if(m._dpL1) 
				AddPartAt(new Vector3(off7+m._w-bd._footSize/2,posH,0),size,doors,index);
		}
		else if(m._pergolaType == Function_Pergola.PergolaType.CW)
		{			
			if(m._dpW1) 
				AddPartAt(new Vector3(0,posH,off7+bd._footSize/2),size,doors,index);
			if(m._dpW2)
				AddPartAt(new Vector3(0,posH,off7+m._l-bd._footSize/2),size,doors,index);
			if(m._dpL2) 
				AddPartAt(new Vector3(-m._w/2+bd._footSize/2,posH,off7+m._l/2),size,doors,index);
			if(m._dpL1) 
				AddPartAt(new Vector3(m._w/2-bd._footSize/2,posH,off7+m._l/2),size,doors,index);
		}	
	}
	
	public void GetUI(GUISkin skin)
	{
		_showDoorProfiles = GUILayout.Toggle(_showDoorProfiles,TextManager.GetText("Pergola.DoorProfile"),GUILayout.Height(50),GUILayout.Width(280));
		
		if(_showDoorProfiles)
		{
			Function_Pergola.PElement currentModule = _fp.GetCurrentModule();
			bool change;
			GUILayout.BeginHorizontal("","bg",GUILayout.Height(50),GUILayout.Width(280));
			GUILayout.FlexibleSpace();
			
			switch (/*rule.type*/_fp.GetCurrentModule()._pergolaType)
			{
			case Function_Pergola.PergolaType.single:
				change = false;
				change |= UISetDoorProfile("L1",ref currentModule._dpL1);
				change |=UISetDoorProfile("L2",ref currentModule._dpL2);
				change |=UISetDoorProfile("W1",ref currentModule._dpW1);
				change |=UISetDoorProfile("W2",ref currentModule._dpW2);
				if(change)
				{
					_fp.SetCurrentModule(currentModule);
					_fp.UpdatePergola();
				}
				break;
				
			case Function_Pergola.PergolaType.CL:
				change = false;
				change |= UISetDoorProfile("W1",ref currentModule._dpW1);
				change |= UISetDoorProfile("W2",ref currentModule._dpW2);
				if(_fp.GetModuleIndex() == 0)//1er
				{
					change |= UISetDoorProfile("L2",ref currentModule._dpL2);
				}
				if(_fp.GetModuleIndex() == _fp.GetModulesCount()-1)//Dernier
				{
					change |= UISetDoorProfile("L1",ref currentModule._dpL1);
				}
				if(change)
				{
					_fp.SetCurrentModule(currentModule);
					_fp.CheckSizes();
					_fp.UpdatePergola();
				}
				break;
				
			case Function_Pergola.PergolaType.CW:
				change = false;
				change |= UISetDoorProfile("L1",ref currentModule._dpL1);
				change |= UISetDoorProfile("L2",ref currentModule._dpL2);
				if(_fp.GetModuleIndex() == 0)//1er
				{
					change |= UISetDoorProfile("W1",ref currentModule._dpW1);
				}
				if(_fp.GetModuleIndex() == _fp.GetModulesCount()-1)//Dernier
				{
					change |= UISetDoorProfile("W2",ref currentModule._dpW2);
				}
				if(change)
				{
					_fp.SetCurrentModule(currentModule);
					_fp.CheckSizes();
					_fp.UpdatePergola();
				}
				break;			
			}
			
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}
	}
	
	public void PSave(BinaryWriter buffer)
	{
		
	}
	
	public void PLoad(BinaryReader buffer)
	{
		
	}
	
	public void Clear()
	{
		if(doors != null)
		{
			foreach(Transform t in doors.transform)
			{
				Destroy(t.gameObject);
			}
		}
	}
	
	#endregion
}

/*
 * Pour le dev des vrai profil de porte :
 * si ajout d'un profil de porte -> enlève le screen qui est au meme endroit
 * et ajoute au noeud "profile de porte" index(+1)+(1|2)+"p1|p2|s1|s2|sp"   <-- index meme principe que pour les screens (localisation)
 * +pX > pieds | sX screens(non porte) | sp screen porte
 * 3 types : un pied, porte a droite / un pied, porte a gauche / 2pieds, porte au milileu
 * 
 * UI : 
 * 
 * Un selecteur type dselecteur de module pour selectionner le coté ou mettre profiole de porte
 * un autre selecteur du meme type pour sélectionner le type de profil
 * deux blocs de dimmensions (comme pour les dimmensions des modules)
 * 2/3 selecteur pour les types de screens
 * un slider pour le screen de la porte
 * */