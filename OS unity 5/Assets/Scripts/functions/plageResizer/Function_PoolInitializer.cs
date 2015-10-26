using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
public class Function_PoolInitializer : MonoBehaviour, Function_OS3D
{
	//public bool poolSet = false;
	string _nameObjectToHide = "Reglage plage";
	string _strObjectToHide = "ReglagePlage";
	private Transform _objectToHide = null;
	private bool _lasteState = false;
	private string _functionName = "Reglage plage";
	private string _altFunctionName = "Afficher";
	
	private GUIItemV2 guiItem = null;
	
	public string _uiName = "PoolResizerUI";
	
	public int id;
	bool plage = false;
	bool margelle = true;
	protected FunctionUI_OS3D _ui;
	
	private AABBOutlineResizer _resizer;
	
	// object servant de référance pour la boite englobante 
	public string _objectBoxName = "margelle";

	void Awake ()
	{

		try
		{
			Transform sideWalk = transform.Find ("plage");
			
			AABBOutlineResizer aabboutlineresier = sideWalk.gameObject.GetComponent<AABBOutlineResizer> ();
			if (aabboutlineresier == null)
			{
				aabboutlineresier = sideWalk.gameObject.AddComponent<AABBOutlineResizer> ();
				aabboutlineresier._objectBoxName = _objectBoxName;
				aabboutlineresier.InitBox();
			}
			else
			{
				aabboutlineresier.InitBox(true);
			}
			//sideWalk.gameObject.AddComponent<Function_hideObject> ();
			
			Transform currentLowWall = transform.Find ("muret");
			
			if (currentLowWall == null)
			{
				Mesh sideWalkMesh = sideWalk.GetComponent<MeshFilter> ().mesh;			
				
				// generate sidewalk uvs
				SidewalkMapper sm = new SidewalkMapper (sideWalkMesh);
				sm.RecalculateTextureCoordinate ();
				
				MeshRenderer sideWalkRenderer = sideWalk.GetComponent<MeshRenderer> ();
				sideWalkRenderer.material.mainTextureScale = new Vector2 (0.5f, 0.5f);
				
				Bounds b = sideWalkMesh.bounds;
				
				// define and create geometry for the lowWall
				float rightPoint = b.extents.x;
				float leftPoint = -b.extents.x;
				float forwardPoint = b.extents.z;
				float backPoint = -b.extents.z;
				
				Vector3 p0 = new Vector3 (leftPoint, 0, forwardPoint);
				Vector3 p1 = new Vector3 (leftPoint, -1.0f, forwardPoint);
				Vector3 p2 = new Vector3 (rightPoint, 0, forwardPoint);
				Vector3 p3 = new Vector3 (rightPoint, -1.0f, forwardPoint);
				Vector3 p4 = new Vector3 (rightPoint, 0, backPoint);
				Vector3 p5 = new Vector3 (rightPoint, -1.0f, backPoint);
				Vector3 p6 = new Vector3 (leftPoint, 0, backPoint);
				Vector3 p7 = new Vector3 (leftPoint, -1.0f, backPoint);
				
				Mesh lowWallMesh = new Mesh();
				lowWallMesh.vertices = new Vector3 [] {p0, p1, p2, p3,
													   p2, p3, p4, p5,
													   p4, p5, p6, p7,
													   p6, p7, p0, p1};
				
				lowWallMesh.triangles = new int [] {0, 1, 2, 1, 3, 2,
													4, 5, 6, 5, 7, 6,
													8, 9, 10, 9, 11, 10,
													12, 13, 14, 13, 15, 14};
				
				lowWallMesh.RecalculateBounds ();
				lowWallMesh.RecalculateNormals ();
				
				// create and add the new gameObject to the scene as child of this gameobject
				GameObject lowWall = new GameObject ("muret");
				lowWall.transform.parent = transform;
				lowWall.transform.localPosition = Vector3.zero;
				lowWall.transform.localRotation = Quaternion.identity;
				lowWall.transform.localScale = Vector3.one;
				
				float x = sideWalk.transform.localPosition.x + b.center.x;
				float y = sideWalk.transform.localPosition.y + b.center.y - b.extents.y;
				float z = sideWalk.transform.localPosition.z + b.center.z;
				lowWall.transform.localPosition = new Vector3(x, y+0.032f, z);

				// add component for rendering, resizing and remapping
				MeshFilter meshFilter = lowWall.AddComponent<MeshFilter> ();
				meshFilter.mesh = lowWallMesh;
				
				MeshRenderer lowWallRenderer = lowWall.AddComponent<MeshRenderer> ();
				lowWallRenderer.material = Resources.Load ("materials/lowWallMat") as Material;
				
				DynamicMapper dynamicMapper = lowWall.AddComponent<DynamicMapper> ();
				dynamicMapper.SetMapper (new LowWallMapper ());
				
				AABBOutlineResizer resizer = lowWall.AddComponent<AABBOutlineResizer> ();
				resizer._objectBoxName = _objectBoxName; 
				resizer.InitBox();
				resizer.SetDownOffset(-1.0f);
				
				//lowWall.AddComponent<Function_hideObject> ();
			}
			else
			{
				AABBOutlineResizer resizer = currentLowWall.gameObject.GetComponent<AABBOutlineResizer> ();
				if (resizer == null)
				{
					resizer = currentLowWall.gameObject.AddComponent<AABBOutlineResizer> ();
					resizer._objectBoxName = _objectBoxName; 
					resizer.InitBox();
					resizer.SetDownOffset(-1.0f);
				}
				else
				{
					resizer.InitBox(true);
				}
			}
			
			Transform occlusion = transform.Find ("SolOcclu");
			if (occlusion != null)
			{
				AABBOutlineResizer resizerOcclusion = occlusion.gameObject.GetComponent<AABBOutlineResizer> ();
				if (resizerOcclusion == null)
				{
					resizerOcclusion = occlusion.gameObject.AddComponent<AABBOutlineResizer> ();
					resizerOcclusion._objectBoxName = _objectBoxName;
					resizerOcclusion.InitBox();
				}
				else
				{
					resizerOcclusion.InitBox(true);
				}
			}
		}
		catch (ArgumentNullException ex)
		{
			Debug.Log (ex.Message);
		}
		catch (NullReferenceException ex)
		{
			Debug.Log (ex.Message);
		}
		if(_uiName!=null)
		setUI(
			(FunctionUI_OS3D)GameObject.Find("MainScene").GetComponent(_uiName) );	

		Transform plage = transform.Find ("plage");
		Transform margelle = transform.Find ("margelle");
		Transform muret = transform.Find ("muret");
		if(plage)
			InitPreset (plage, plage.GetComponent<Renderer>().material);
		if(margelle)
			InitPreset (margelle, margelle.GetComponent<Renderer>().material);
		if(muret)
			InitPreset (muret, muret.GetComponent<Renderer>().material);

	}

	void InitPreset(Transform target)
	{
		Dictionary<string,float> preset;
		if(target.name == "plage" || target.name == "Plage")
		{
			preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigPlage("01p");
		}
		else if(target.name == "margelle" || target.name == "Margelle")
		{
			preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMargelle("01_mar");
		}
		else if(target.name == "muret" || target.name == "Muret")
		{
			preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMuret("01m");
		}
		else
		{
			preset = null;
		}
		if(preset != null)
		{
			string[] keys = preset.Keys.ToArray();
			float red = 0;
			float green = 0;
			float blue = 0;
			foreach (string key in keys)
			{
				switch(key)
				{
				case "Normallevel ":
					target.GetComponent<Renderer> ().material.SetFloat("_Normallevel",preset[key]);
					break ;
				case "UVtranslation ":
					target.GetComponent<Renderer> ().material.SetFloat("_UVtranslation",preset[key]);
					break ;
				case "UVRotation ":
					target.GetComponent<AABBOutlineResizer>().setOffsetRotationUV(preset[key]);
					//target.GetComponent<Renderer> ().material.SetFloat("_UVRotation",preset[key]);
					break ;
				case "UVTile ":
					target.GetComponent<AABBOutlineResizer>().setOffsetTileUV(preset[key]);
					//target.GetComponent<Renderer> ().material.SetFloat("_UVTile",preset[key]);
					break ;
				case "Blur ":
					target.GetComponent<Renderer> ().material.SetFloat("_Blur",preset[key]);
					break ;
				case "gloss ":
					target.GetComponent<Renderer> ().material.SetFloat("_gloss",preset[key]);
					break ;
				case "Specularlevel ":
					target.GetComponent<Renderer> ().material.SetFloat("_Specularlevel",preset[key]);
					break ;
				case "Lightness ":
					target.GetComponent<Renderer> ().material.SetFloat("_Ligntness",preset[key]);
					break ;
				case "HueMaskIntensity ":
					target.GetComponent<Renderer> ().material.SetFloat("_HueMaskIntensity",preset[key]);
					break ;
				case "HuecolorR ":
					red = preset[key];
					break ;
				case "HuecolorG ":
					green = preset[key];
					break ;
				case "HuecolorB ":
					blue = preset[key];
					break ;
				case "Huelevel ":
					target.GetComponent<AABBOutlineResizer>().setHueLevel(preset[key]);;
					break ;
				case "Saturation ":
					target.GetComponent<AABBOutlineResizer>().setSaturation(preset[key]);
					break ;
				case "Bulb ":
					target.GetComponent<Renderer> ().material.SetFloat("_Bulb",preset[key]);
					break ;
				case "Reflexion ":
					target.GetComponent<Renderer> ().material.SetFloat("_Reflexion",preset[key]);
					break ;
				case "Reflexionintensity ":
					target.GetComponent<Renderer> ().material.SetFloat("_Reflexionintensity",preset[key]);
					break ;
				case "ReflexionBlur ":
					target.GetComponent<Renderer> ().material.SetFloat("_ReflexionBlur",preset[key]);
					break ;
				}					
			}
			Vector3 hue = new Vector3(red,green,blue);
			Color hueColor = new Color(red,green,blue,1.0f);
			if(target.name == "plage" || target.name == "Plage")
			{
				target.GetComponent<AABBOutlineResizer>().setColorPlage(hue);
			}
			else if(target.name == "margelle" || target.name == "Margelle")
			{
				target.GetComponent<AABBOutlineResizer>().setColorMargelle(hue);
			}
			else if(target.name == "muret" || target.name == "Muret")
			{
				target.GetComponent<AABBOutlineResizer>().setColorMuret(hue);
			}
			//picker.GetComponent<HSVPicker>().currentColor = hue;
		}
	}
	void InitPreset(Transform target , Material mat)
	{
		Dictionary<string,float> preset;
		if(target.name == "plage" || target.name == "Plage")
		{
			if(target.GetComponent<Renderer>().material.shader.name.Contains("Pointcube"))
			{
				preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigPlage(mat.GetTexture("_Diffuse").name);
			}
			else
			{
				Texture temp = mat.GetTexture("_MainTex");
				target.GetComponent<Renderer>().material.shader = Resources.Load("shaders/Pointcube_StandardObjet") as Shader;
				mat.SetTexture("_Diffuse", temp);
				preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigPlage(mat.GetTexture("_Diffuse").name);
			}
		}
		else if(target.name == "margelle" || target.name == "Margelle")
		{
			if(target.GetComponent<Renderer>().material.shader.name.Contains("Pointcube"))
			{
				Debug.Log("aaaaaaaaaaaaa  " + mat.GetTexture("_Diffuse").name);
				preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMargelle(mat.GetTexture("_Diffuse").name);
			}
			else
			{
				Texture temp = mat.GetTexture("_MainTex");
				target.GetComponent<Renderer>().material.shader = Resources.Load("shaders/Pointcube_StandardObjet") as Shader;
				mat.SetTexture("_Diffuse", temp);
				preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMargelle(mat.GetTexture("_Diffuse").name);
			}
		}
		else if(target.name == "muret" || target.name == "Muret")
		{
				if(target.GetComponent<Renderer>().material.shader.name.Contains("Pointcube"))
				{
					preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMuret(mat.GetTexture("_Diffuse").name);
				}
				else
				{
					Texture temp = mat.GetTexture("_MainTex");
					target.GetComponent<Renderer>().material.shader = Resources.Load("shaders/Pointcube_StandardObjet") as Shader;
					mat.SetTexture("_Diffuse", temp);
				preset = GameObject.Find ("MainScene").GetComponent<LoadShaderPresets>().getConfigMuret(mat.GetTexture("_Diffuse").name);
				}
		}
		else
		{
			preset = null;
		}
		if(preset != null)
		{
			string[] keys = preset.Keys.ToArray();
			float red = 0;
			float green = 0;
			float blue = 0;
			Debug.Log("ICI  ");
			foreach (string key in keys)
			{
				switch(key)
				{
				case "Normallevel ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 0.5f;
					target.GetComponent<Renderer> ().material.SetFloat("_Normallevel",preset[key]);
					break ;
				case "UVtranslation ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 0.0f;
					target.GetComponent<Renderer> ().material.SetFloat("_UVtranslation",preset[key]);
					break ;
				case "UVRotation ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 0.0f;
					target.GetComponent<AABBOutlineResizer>().setOffsetRotationUV(preset[key]);
					//target.GetComponent<Renderer> ().material.SetFloat("_UVRotation",preset[key]);
					break ;
				case "UVTile ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 0.5f;
					target.GetComponent<AABBOutlineResizer>().setOffsetTileUV(preset[key]);
					//target.GetComponent<Renderer> ().material.SetFloat("_UVTile",preset[key]);
					break ;
				case "Blur ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 0.0f;
					target.GetComponent<Renderer> ().material.SetFloat("_Blur",preset[key]);
					break ;
				case "gloss ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 0.4f;
					target.GetComponent<Renderer> ().material.SetFloat("_gloss",preset[key]);
					break ;
				case "Specularlevel ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 0.5f;
					target.GetComponent<Renderer> ().material.SetFloat("_Specularlevel",preset[key]);
					break ;
				case "Lightness ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 1.0f;
					target.GetComponent<Renderer> ().material.SetFloat("_Ligntness",preset[key]);
					break ;
				case "HueMaskIntensity ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 1.0f;
					target.GetComponent<Renderer> ().material.SetFloat("_HueMaskIntensity",preset[key]);
					break ;
				case "HuecolorR ":
					red = preset[key];
					break ;
				case "HuecolorG ":
					green = preset[key];
					break ;
				case "HuecolorB ":
					blue = preset[key];
					break ;
				case "Huelevel ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 0.0f;
					target.GetComponent<AABBOutlineResizer>().setHueLevel(preset[key]);;
					break ;
				case "Saturation ":
					if(preset[key] !=0 && preset[key]<0.0000001|| preset[key] != 0 && preset[key]>10000)
						preset[key] = 1.0f;
					target.GetComponent<AABBOutlineResizer>().setSaturation(preset[key]);
					break ;
				case "Bulb ":
					if(preset[key] !=0 && preset[key]<0.0000001)
						preset[key] = 0.0f;
					target.GetComponent<Renderer> ().material.SetFloat("_Bulb",preset[key]);
					break ;
				case "Reflexion ":
					if(preset[key] !=0 && preset[key]<0.0000001)
						preset[key] = 0.0f;
					target.GetComponent<Renderer> ().material.SetFloat("_Reflexion",preset[key]);
					if(preset[key] !=0)
					{
						Texture2D temp = (Texture2D)Resources.Load("shader/configs/Chrome 1");
						target.GetComponent<Renderer> ().material.SetTexture("_CubeMap",temp);
					}
					break ;
				case "Reflexionintensity ":
					target.GetComponent<Renderer> ().material.SetFloat("_Reflexionintensity",preset[key]);
					break ;
				case "ReflexionBlur ":
					target.GetComponent<Renderer> ().material.SetFloat("_ReflexionBlur",preset[key]);
					break ;
				}					
			}
			Vector3 hue = new Vector3(red,green,blue);
			Color hueColor = new Color(red,green,blue,1.0f);
			if(target.name == "plage" || target.name == "Plage")
			{
				target.GetComponent<AABBOutlineResizer>().setColorPlage(hue);
			}
			else if(target.name == "margelle" || target.name == "Margelle")
			{
				target.GetComponent<AABBOutlineResizer>().setColorMargelle(hue);
			}
			else if(target.name == "muret" || target.name == "Muret")
			{
				target.GetComponent<AABBOutlineResizer>().setColorMuret(hue);
			}

			//picker.GetComponent<HSVPicker>().currentColor = hue;
			//Texture2D tempMap =
		}
	}
	void Update ()
	{
		if(guiItem != null)
		{

			foreach(Function_hideObject f_ho in transform.GetComponents(typeof(Function_hideObject)))
			{
				if(f_ho._nameObjectToHide == "plage" )
				{
					if(f_ho.GetFunctionName() == "Cacher")
					{
						plage = true;
					}
					else
					{
						plage = false;
						guiItem.SetEnableUI(false);
					}	
				}
				if(f_ho._nameObjectToHide == "margelle" )
				{
					if(f_ho.GetFunctionName() == "Cacher")
					{
						margelle = true;
					}
					else
					{
						margelle = false;
					}	
				}
			
			}
			if(margelle || plage){
				guiItem.SetEnableUI(true);
			}else{
				guiItem.SetEnableUI(false);
			}
		}
	}
	
	public void setUI(FunctionUI_OS3D ui)
	{
		_ui = ui;
	}
	
	public void setGUIItem(GUIItemV2 _guiItem)
	{
		guiItem = _guiItem;
	}
	
	public void DoAction ()
	{
	/*	if(_uiName!=null)
		{
			((PoolResizerUI)GameObject.Find("MainScene").GetComponent(_uiName)).SetPoolObject(transform);	
		}*/
		if(_ui!=null)
		{
			_ui.DoActionUI(gameObject);
			/*GameObject.Find("MainScene").GetComponent<PoolResizerUI>().enabled = true;
			GameObject.Find("MainScene").GetComponent<PoolResizerUI>().SetPoolObject(transform);	
			
			GameObject.Find("MainScene").GetComponent<GUIMenuConfiguration>().setVisibility(false);
			
			GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().setVisibility(false);
			GameObject.Find("MainScene").GetComponent<GUIMenuInteraction>().isConfiguring = false;
			
			Camera.mainCamera.GetComponent<ObjInteraction>().setSelected(null,true);
			Camera.mainCamera.GetComponent<ObjInteraction>().setActived(false);*/
		}
	}
	
	public string GetFunctionName()
	{
		if (_lasteState)
			return _altFunctionName;
		else
			return _functionName;
	}
	
	public string GetFunctionParameterName()
	{
		return " "+TextManager.GetText(_strObjectToHide);
	}
	
	public int GetFunctionId()
	{
		return id;
	}
	
	//SAVE/LOAD
	
	public void save(BinaryWriter buf)
	{
		int nb = 0;
		foreach(Transform t in transform)
		{
			if(t.GetComponent<AABBOutlineResizer>())
			{
				nb ++;
			}
		}
		buf.Write(nb);//int
		
		foreach(Transform t in transform)
		{
			if(t.GetComponent<AABBOutlineResizer>())
			{
				_resizer = t.GetComponent<AABBOutlineResizer>();
				buf.Write(t.name);//string
				if(t.gameObject.name != "margelle")
				{
					buf.Write(_resizer.GetDownOffset());// vv Floats vv
					buf.Write(_resizer.GetUpOffset());
					buf.Write(_resizer.GetForwardOffset());
					buf.Write(_resizer.GetBackOffset());
					buf.Write(_resizer.GetRightOffset());
					buf.Write(_resizer.GetLeftOffset());
				}
				buf.Write(_resizer.getOffsetRotationUV());
				buf.Write(_resizer.getOffsetTileUV());
				//Debug.Log("ECRITURE TILE : " + _resizer.getOffsetTileUV());
				if(t.gameObject.name == "plage")
				{
					buf.Write(_resizer.getColorPlage().x);
					buf.Write(_resizer.getColorPlage().y);
					buf.Write(_resizer.getColorPlage().z);
					//Debug.Log("ECRITURE COLORPLAGE  x:" + _resizer.getColorPlage().x + "  y:"+_resizer.getColorPlage().y + "  z:"+_resizer.getColorPlage().z);
				}
				else if(t.gameObject.name == "margelle")
				{
					buf.Write(_resizer.getColorMargelle().x);
					buf.Write(_resizer.getColorMargelle().y);
					buf.Write(_resizer.getColorMargelle().z);
				}
				else if(t.gameObject.name == "muret")
				{
					buf.Write(_resizer.getColorMuret().x);
					buf.Write(_resizer.getColorMuret().y);
					buf.Write(_resizer.getColorMuret().z);
				}
				buf.Write(_resizer.getHueLevel());
				buf.Write(_resizer.getSaturation());
			}
		}
	}
	
	public void load(BinaryReader buf)
	{
		int nb = buf.ReadInt32();
		for(int i=0;i<nb;i++)
		{
			string nm = buf.ReadString();
			Transform t = transform.FindChild(nm);
			Debug.Log("NAME  " + t.name);
			if(t.gameObject.name == "plage" || t.gameObject.name == "Plage" ||t.gameObject.name == "margelle" ||t.gameObject.name == "Margelle" || t.gameObject.name == "muret"|| t.gameObject.name == "Muret")
			{
				InitPreset(t,t.GetComponent<Renderer>().material);
			}
//			t.gameObject.AddComponent<AABBOutlineResizer>();
			if(t.gameObject.name != "margelle" && (t.gameObject.name == "plage" || t.gameObject.name == "Plage" ||t.gameObject.name == "margelle" ||t.gameObject.name == "Margelle"))
			{
				t.GetComponent<AABBOutlineResizer>().SetDownOffset(buf.ReadSingle());
				t.GetComponent<AABBOutlineResizer>().SetUpOffset(buf.ReadSingle());
				t.GetComponent<AABBOutlineResizer>().SetForwardOffset(buf.ReadSingle());
				t.GetComponent<AABBOutlineResizer>().SetBackOffset(buf.ReadSingle());
				t.GetComponent<AABBOutlineResizer>().SetRightOffset(buf.ReadSingle());
				t.GetComponent<AABBOutlineResizer>().SetLeftOffset(buf.ReadSingle());
				t.GetComponent<AABBOutlineResizer>().applyChanges();
			}
			if(t.gameObject.name == "plage" || t.gameObject.name == "Plage" ||t.gameObject.name == "margelle" ||t.gameObject.name == "Margelle" || t.gameObject.name == "muret"|| t.gameObject.name == "Muret")
			{
				float tempRotationUV = buf.ReadSingle();
				float tempTileUV = buf.ReadSingle();
				if(tempRotationUV != 0 && tempRotationUV<0.000001 || tempRotationUV != 0 && tempRotationUV>10000)
					tempRotationUV = 0.0f;
				if(tempTileUV != 0 && tempTileUV<0.000001 || tempTileUV != 0 && tempTileUV>10000)
					tempTileUV = 0.5f;

				t.GetComponent<AABBOutlineResizer>().setOffsetRotationUV(tempRotationUV);
				t.GetComponent<AABBOutlineResizer>().setOffsetTileUV(tempTileUV);
			}
			if(t.gameObject.name == "plage")
			{
				float r = buf.ReadSingle();
				float g = buf.ReadSingle();
				float b = buf.ReadSingle();
				//Debug.Log("R : " + r + "  G : " + g +"  B : " + b);
				t.GetComponent<AABBOutlineResizer>().setColorPlage(new Vector3(r,g,b));
			}
			else if(t.gameObject.name == "margelle")
			{
				float r = buf.ReadSingle();
				float g = buf.ReadSingle();
				float b = buf.ReadSingle();
				t.GetComponent<AABBOutlineResizer>().setColorMargelle(new Vector3(r,g,b));
			}
			else if(t.gameObject.name == "muret")
			{
				float r = buf.ReadSingle();
				float g = buf.ReadSingle();
				float b = buf.ReadSingle();
				t.GetComponent<AABBOutlineResizer>().setColorMuret(new Vector3(r,g,b));
			}
			if(t.gameObject.name == "plage" || t.gameObject.name == "Plage" ||t.gameObject.name == "margelle" ||t.gameObject.name == "Margelle" || t.gameObject.name == "muret"|| t.gameObject.name == "Muret")
			{
				float tempHUElevel = buf.ReadSingle();
				float tempSaturation = buf.ReadSingle();
				if(tempHUElevel != 0 && tempHUElevel<0.000001 || tempHUElevel != 0 && tempHUElevel>10000)
					tempHUElevel = 0.0f;
				if(tempSaturation != 0 && tempSaturation<0.000001|| tempSaturation != 0 && tempSaturation>10000)
					tempSaturation = 0.5f;
				t.GetComponent<AABBOutlineResizer>().setHueLevel(tempHUElevel);
				t.GetComponent<AABBOutlineResizer>().setSaturation(tempSaturation);
			}
		}
	}
	
	public ArrayList getConfig()
	{
		ArrayList config = new ArrayList();
		
		int nb = 0;
		foreach(Transform t in transform)
		{
			if(t.GetComponent<AABBOutlineResizer>())
			{
				nb ++;
			}
		}
		config.Add(nb);//int
		
		foreach(Transform t in transform)
		{
			if(t.GetComponent<AABBOutlineResizer>())
			{
				_resizer = t.GetComponent<AABBOutlineResizer>();
				config.Add(t.name);//string
				if(t.gameObject.name != "margelle")
				{
					config.Add(_resizer.GetDownOffset());// vv Floats vv
					config.Add(_resizer.GetUpOffset());
					config.Add(_resizer.GetForwardOffset());
					config.Add(_resizer.GetBackOffset());
					config.Add(_resizer.GetRightOffset());
					config.Add(_resizer.GetLeftOffset());
				}
				config.Add(_resizer.getOffsetRotationUV());
				config.Add(_resizer.getOffsetTileUV());

				if(t.gameObject.name == "plage")
				{
					config.Add(_resizer.getColorPlage().x);
					config.Add(_resizer.getColorPlage().y);
					config.Add(_resizer.getColorPlage().z);
				}
				else if(t.gameObject.name == "margelle")
				{
					config.Add(_resizer.getColorMargelle().x);
					config.Add(_resizer.getColorMargelle().y);
					config.Add(_resizer.getColorMargelle().z);
				}
				else if(t.gameObject.name == "muret")
				{
					config.Add(_resizer.getColorMuret().x);
					config.Add(_resizer.getColorMuret().y);
					config.Add(_resizer.getColorMuret().z);
				}
				config.Add(_resizer.getHueLevel());
				config.Add(_resizer.getSaturation());
			}
		}
		
		return config;
	}
	
	public void setConfig(ArrayList config)
	{
		int index = 0;
		
		int nb = (int)config[index];
		
		index++;
		/*
		for(int i=0;i<nb;i++)
		{
			string nm = (string)config[index];
			index++;
			Transform t = transform.FindChild(nm);

			if(t.gameObject.name != "margelle")
			{
				t.GetComponent<AABBOutlineResizer>().SetDownOffset((float)config[index]);
				index++;
				t.GetComponent<AABBOutlineResizer>().SetUpOffset((float)config[index]);
				index++;
				t.GetComponent<AABBOutlineResizer>().SetForwardOffset((float)config[index]);
				index++;
				t.GetComponent<AABBOutlineResizer>().SetBackOffset((float)config[index]);
				index++;
				t.GetComponent<AABBOutlineResizer>().SetRightOffset((float)config[index]);
				index++;
				t.GetComponent<AABBOutlineResizer>().SetLeftOffset((float)config[index]);
				index++;
				t.GetComponent<AABBOutlineResizer>().applyChanges();

			}
			if( t.GetComponent<AABBOutlineResizer>() )
			{
				t.GetComponent<AABBOutlineResizer>().setOffsetRotationUV((float)config[index]);
				index++;
				t.GetComponent<AABBOutlineResizer>().setOffsetTileUV((float)config[index]);
				index++;
			}
			if(t.gameObject.name == "plage")
			{
				float r = (float)config[index];
				index++;
				float g = (float)config[index];
				index++;
				float b = (float)config[index];
				index++;
				t.GetComponent<AABBOutlineResizer>().setColorPlage(new Vector3(r,g,b));
				index++;
			}
			else if(t.gameObject.name == "margelle")
			{
				float r = (float)config[index];
				index++;
				float g = (float)config[index];
				index++;
				float b = (float)config[index];
				index++;
				if( t.GetComponent<AABBOutlineResizer>() )
				{
					t.GetComponent<AABBOutlineResizer>().setColorMargelle(new Vector3(r,g,b));
					index++;
				}
			}
			else if(t.gameObject.name == "muret")
			{
				float r = (float)config[index];
				index++;
				float g = (float)config[index];
				index++;
				float b = (float)config[index];
				index++;
				if( t.GetComponent<AABBOutlineResizer>() )
				{
					t.GetComponent<AABBOutlineResizer>().setColorMuret(new Vector3(r,g,b));
					index++;
				}
			}
			if( t.GetComponent<AABBOutlineResizer>() )
			{
				t.GetComponent<AABBOutlineResizer>().setHueLevel((float)config[index]);
				index++;
			}
			t.GetComponent<AABBOutlineResizer>().setSaturation((float)config[index]);
			index++;
		}*/
	}

}
