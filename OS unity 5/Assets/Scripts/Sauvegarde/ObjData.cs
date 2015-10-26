using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ObjData : MonoBehaviour
{
	
//	string original = "";
	bool canUpdateMaps = false;
	
//	int _typ;
//	int _fab;
	
	Object[] tmpMaps;
	
	IDictionary subObjects = new SortedList();
	
	OSLibObject _objectModel;
	
	//Functions spe
	List<Component> specFcns = new List<Component>();
	
	private int _nbConfMats = 0;
	
	bool _assetBundleIsOpen = false;
	
	public int selFab = -1;
	public int selTyp = -1;
	public int selObj = -1;

	public string textureName;
	//-----------------------------------------------
	
	// Use this for initialization
	void Start ()
	{
//		// Textures et couleurs configurables
//		if(subObjects.Count==0)
//		{
//			//TODO changer le "foreach subobject" en "foreach module in modules
//			for(int i=0;i<transform.GetChildCount();i++)
//			{
//				subObjects.Add(transform.GetChild(i).name,0);	
//			}
//		} // ^^ fait dans le SetObjectModel() ^^
		
		
//		original=name.Replace("(Clone)","");
		//name = original;
//		canUpdateMaps = true;
		
		//Fcn spé
		foreach(Component c in GetComponents<MonoBehaviour>())
		{
			if(c.GetType().ToString().StartsWith("Function"))
			{
				specFcns.Add(c);
			}				
		}
	}
	
	// Update is called once per frame
//	void Update ()
//	{
//		if(canUpdateMaps)
//		{
//			canUpdateMaps = false;
//			StartCoroutine(updateMaps());
//		}
//	}
	
	//--------------------------------------------------
	
//	public void setTypFab(int typ,int fab)
//	{
//		_typ = typ;
//		_fab = fab;
//	}
//	
//	public int getTyp()
//	{
//		return _typ;
//	}
//	
//	public int getFab()
//	{
//		return _fab;	
//	}
	
	public void setSubId(string name,int index)
	{
		subObjects[name] = index;
//		Debug.Log("n° "+index+ " pour "+name);
	}
	
	public bool IsCustomizable()
	{
		return _nbConfMats>0 || specFcns.Count > 0;	
	}
	
	////////////////////////////////////////////
	//Sauvegarde Data objet
	
	
	public void SaveData(BinaryWriter buf)
	{
//		buf.Write(original);
//		buf.Write(subObjects.Count);
//		
//		buf.Write(transform.position.x);
//		buf.Write(transform.position.y);
//		buf.Write(transform.position.z);
//		
//		buf.Write(transform.rotation.x);
//		buf.Write(transform.rotation.y);
//		buf.Write(transform.rotation.z);
//		buf.Write(transform.rotation.w);
//		
//		//index's de configuration des maps
//		for(int i=0;i<transform.GetChildCount();i++)
//		{
//			buf.Write((int)subObjects[transform.GetChild(i).name]);
//		}
//		
//		//fcn. spé.
//		buf.Write(specFcns.Count);
//		foreach(Component c in specFcns)
//		{
//			buf.Write(c.GetType().ToString());
//			((Function_OS3D)c).save(buf);
//		}
		
		int type,brand,indexObj;
				
		//Sauvergarde Type et marque de l'objet
		if(_objectModel.getCategory().GetParentCategory() == null) // que un type
		{
			brand = -1;
			type = _objectModel.getCategory().GetId();
		}
		else // type plus marque
		{
			brand = _objectModel.getCategory().GetId();
			type = _objectModel.getCategory().GetParentCategory().GetId();
		}
		
		indexObj = -1;
		/*for(int i=0;i<_objectModel.getCategory().GetObjectList().Count;i++)
		{
			if(_objectModel.getCategory().GetObjectList()[i].GetId() == _objectModel.GetId())
			{
				indexObj = i;
			}
		}*/
		indexObj = _objectModel.GetId();
		
		buf.Write(brand);
		buf.Write(type);
		buf.Write(indexObj);

		//Sauvegarde spatiale
		//position
		buf.Write(transform.position.x);
		buf.Write(transform.position.y);
		buf.Write(transform.position.z);
		//rotation
		buf.Write(transform.rotation.x);
		buf.Write(transform.rotation.y);
		buf.Write(transform.rotation.z);
		buf.Write(transform.rotation.w);
		//scale
		buf.Write(transform.localScale.x);
		buf.Write(transform.localScale.y);
		buf.Write(transform.localScale.z);
		
		//LockAbility
		buf.Write(GetComponent<ObjBehav>().amILocked());
		
		//Sauvegarde configuration couleurs
//		buf.Write(_objectModel.GetModules().GetStandardModuleList().Count);
		buf.Write(subObjects.Count);
		
		foreach (OSLibModule mod in _objectModel.GetModules().GetStandardModuleList())
		{
			if(subObjects.Contains(mod.GetModuleType()))
			{
				buf.Write(mod.GetModuleType());
				buf.Write((int)subObjects[mod.GetModuleType()]);
			}
		}
		
		//fcn. spé.
		buf.Write(specFcns.Count);
		foreach(Component c in specFcns)
		{
			buf.Write(c.GetType().ToString());
			buf.Write((int)((Function_OS3D)c).GetFunctionId());
			((Function_OS3D)c).save(buf);
		}
	}
	
//	public void LoadConf(ArrayList conf)
//	{
//		int i=0;
//		foreach(Transform t in transform)
//		{
//			setSubId(t.name,(int)conf[i]);
//			i++;
//		}
//		canUpdateMaps = true;
//
//	}
	
//	IEnumerator updateMaps()//TODO A modifier quand yaura les libs
//	{
//		foreach(Transform t in transform)
//		{
//			//Quand yaura les libs, au lieu de faire un Resources.Load,
//			//faire un truc du genre lib.getMaps(t.name)
//			tmpMaps = Resources.LoadAll("maps/"+t.name);
//			//Debug.Log(tmpMaps.Length+" MAPS FOR "+t.name);
//			if(tmpMaps.Length>0)
//			{
//				int id = (int)subObjects[t.name];
//				t.renderer.material.mainTexture = (Texture2D)tmpMaps[id];	
//			}
//			
//			yield return new WaitForSeconds(Time.deltaTime);
//		}
//		tmpMaps = null;
//		yield return true;
//	} 
	
	public void SetObjectModel (OSLibObject obj)
	{
		_objectModel = obj;
	}
	
	public void SetObjectModel (OSLibObject obj, AssetBundle assets)
	{
		_objectModel = obj;
		_nbConfMats = _objectModel.GetModules().GetStandardModuleList().Count;
		// apply default materials
		foreach (OSLibModule mod in obj.GetModules ().GetStandardModuleList())
		{
			Transform modTarget = transform.Find (mod.GetModuleType ());
			Color colorText = new Color(0.78f,0.78f,0.78f,1.0f);
			if (modTarget != null)
			{
				if (mod.GetColorList ().Count > 0)
				{
					OSLibColor color = OSLibBuilderUtils.
						GetFirstColorOfType(mod, mod.GetModuleType ());
					modTarget.GetComponent<Renderer>().material.color = color.GetColor ();
					if(modTarget.GetComponent<MeshRenderer>())
					{
						modTarget.GetComponent<Renderer>().material.mainTexture = null;
						if((modTarget.GetComponent<Renderer>().material.HasProperty("_DecalTex")) && (usefullData.lowTechnologie))
						{
							modTarget.GetComponent<Renderer>().material.mainTexture = modTarget.GetComponent<Renderer>().material.GetTexture("_DecalTex");
						}
					}
					
					if(color.GetColorType2()!=null)
					{
						Transform modTarget2 = transform.Find (color.GetColorType2());	
						if (modTarget2 != null)
						{
							Color color2 = color.GetColor2();
							modTarget2.GetComponent<Renderer>().material.mainTexture = null;
							modTarget2.GetComponent<Renderer>().material.color = color2;
						}
					}
				}
				else if (mod.GetTextureList ().Count > 0)
				{
					OSLibTexture textData = OSLibBuilderUtils.
												GetFirstTextureOfType (mod, mod.GetModuleType ());
					
					Texture2D text = assets.LoadAsset (textData.GetFilePath (), 
													   typeof (Texture2D)) as Texture2D;
					if(modTarget.GetComponent<Renderer>().material.shader.name.Contains("Pointcube"))
					{
						Debug.Log(text.name + " TEXTURES");
						modTarget.GetComponent<Renderer>().material.SetTexture("_Diffuse",text);
						/*modTarget.GetComponent<Renderer>().material.SetTexture("_Normal",text);	
						modTarget.GetComponent<Renderer>().material.SetTexture("_HueMask",text);	
						modTarget.GetComponent<Renderer>().material.SetTexture("_SpecMask",text);*/
						modTarget.GetComponent<Renderer>().material.color = colorText;
						float scale = textData.GetScale();
						
						if(textData.GetTextureType2()!=null)
						{
							Transform modTarget2 = transform.Find (textData.GetTextureType2());	
							if (modTarget2 != null)
							{
								Texture2D text2 = assets.LoadAsset (textData.GetFilePath2 (), 
								                                    typeof (Texture2D)) as Texture2D;
								modTarget2.GetComponent<Renderer>().material.mainTexture = text2;		
								modTarget2.GetComponent<Renderer>().material.color = colorText;
								modTarget2.GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(scale,scale));
							}
						}
					}
					else{
						modTarget.GetComponent<Renderer>().material.mainTexture = text;		
						modTarget.GetComponent<Renderer>().material.color = colorText;
						float scale = textData.GetScale();
						if (modTarget.name.CompareTo("plage")==0)
							scale/=2.0f;
						modTarget.GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(scale,scale));
						
						if(textData.GetTextureType2()!=null)
						{
							Transform modTarget2 = transform.Find (textData.GetTextureType2());	
							if (modTarget2 != null)
							{
								Texture2D text2 = assets.LoadAsset (textData.GetFilePath2 (), 
														   typeof (Texture2D)) as Texture2D;
								modTarget2.GetComponent<Renderer>().material.mainTexture = text2;		
								modTarget2.GetComponent<Renderer>().material.color = colorText;
								modTarget2.GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(scale,scale));
							}
						}
					}
				}
				subObjects.Add(mod.GetModuleType(),0);
			}
		}
	}
	
	public OSLibObject GetObjectModel ()
	{
		return _objectModel;	
	}
	
//	public void loadConf(IDictionary conf)
//	{
//		subObjects = conf;
//		StartCoroutine(loadConfIE());	
//	}
	
	public IEnumerator loadConfIE(IDictionary conf)
	{
//		subObjects = conf;
		while (Montage.assetBundleLock)
		{
			yield return new WaitForEndOfFrame ();	
		}
		subObjects.Clear();
		foreach(OSLibModule module in _objectModel.GetModules().GetStandardModuleList())
		{
			if(module.GetColorList().Count>0 || module.GetTextureList().Count > 0) //protection anti module specifique (comme le duck !)
			{
				if(conf.Contains(module.GetModuleType()))
				{
					subObjects.Add(module.GetModuleType(),conf[module.GetModuleType()]);
				}
				else
				{
					subObjects.Add(module.GetModuleType(),0);

				}
			}
		}
		
		Montage.assetBundleLock = true;
		print (_objectModel.GetLibrary ().GetAssetBundlePath ());
		
		WWW www = WWW.LoadFromCacheOrDownload (_objectModel.GetLibrary ().GetAssetBundlePath (),
												_objectModel.GetLibrary ().GetVersion ());
		yield return www;
		if (www.error != null) 
		{
			Debug.Log (www.error);
		}
		else
		{

			AssetBundle assets = www.assetBundle;

			foreach(OSLibModule module in _objectModel.GetModules().GetStandardModuleList())
			{
				if(module.GetColorList().Count>0 || module.GetTextureList().Count > 0) //protection anti module specifique (comme le duck !)
				{
					int indexTex= (int)subObjects[module.GetModuleType()];
		
					Transform objToModif = transform.Find (module.GetModuleType());
					
					if (objToModif != null)
					{
						int textureCount = module.GetTextureList ().Count;
						int colorCount = module.GetColorList ().Count;
						
						if (indexTex /*< textureCount*/ >= colorCount)
						{
							int textureIndex = indexTex - colorCount;
							if (textureIndex < textureCount)
							{
								Texture2D texture = new Texture2D(0,0);
								Texture2D textureN= new Texture2D(0,0);
								Texture2D textureS= new Texture2D(0,0);
								Texture2D textureH= new Texture2D(0,0);

								OSLibTexture textureData = 	module.GetTextureList () [textureIndex];
								Debug.Log("aaaa " + textureData.GetFilePath () +"  nnnnnnn");
							//	StartCoroutine (ApplyTexture (objToModif, textureData, _objectModel.GetLibrary ())); 
								texture = assets.LoadAsset (textureData.GetFilePath (),typeof (Texture2D)) as Texture2D;
								if(textureData.GetNormalPath () !="")
									textureN = assets.LoadAsset (textureData.GetNormalPath (),typeof (Texture2D)) as Texture2D;
								if(textureData.GetSpecularMaskPath () !="")
									textureS = assets.LoadAsset (textureData.GetSpecularMaskPath (),typeof (Texture2D)) as Texture2D;
								if(textureData.GetHueMaskPath () !="")
									textureH = assets.LoadAsset (textureData.GetHueMaskPath (),typeof (Texture2D)) as Texture2D;


								// ---------------- DEBUT CHANGEMENT GUIGUI ----------------

							
								
								if(
								   textureData.GetNormalPath () == "" ||
								   textureData.GetSpecularMaskPath () == "" ||
								   textureData.GetHueMaskPath () == ""
								   )
								{
									texture = assets.LoadAsset ("01P",typeof (Texture2D)) as Texture2D;
									textureN = assets.LoadAsset ("01N",typeof (Texture2D)) as Texture2D;
									textureS = assets.LoadAsset ("01S",typeof (Texture2D)) as Texture2D;
									textureH = assets.LoadAsset ("01H",typeof (Texture2D)) as Texture2D;	
								}
								else
								{
									texture = assets.LoadAsset (textureData.GetFilePath (),typeof (Texture2D)) as Texture2D;
									textureN = assets.LoadAsset (textureData.GetNormalPath (),typeof (Texture2D)) as Texture2D;
									textureS = assets.LoadAsset (textureData.GetSpecularMaskPath (),typeof (Texture2D)) as Texture2D;
									textureH = assets.LoadAsset (textureData.GetHueMaskPath (),typeof (Texture2D)) as Texture2D;		
								}

								// ---------------- FIN CHANGEMENT GUIGUI ----------------
								// ---------------- DEBUT CHANGEMENT COM ----------------
								/*
								print ("textureData.GetNormalPath () : "+textureData.GetFilePath ());
								Texture2D texture = assets.LoadAsset (textureData.GetFilePath (),typeof (Texture2D)) as Texture2D;
								Texture2D textureN = assets.LoadAsset (textureData.GetNormalPath (),typeof (Texture2D)) as Texture2D;
								Texture2D textureS = assets.LoadAsset (textureData.GetSpecularMaskPath (),typeof (Texture2D)) as Texture2D;
								Texture2D textureH = assets.LoadAsset (textureData.GetHueMaskPath (),typeof (Texture2D)) as Texture2D;
								*/
								// ---------------- FIN CHANGEMENT COM ----------------

								objToModif.GetComponent<Renderer>().material.color = new Color(0.78f,0.78f,0.78f,1.0f);//TODO Verifier que la couleur soit du blanc

								if(objToModif.GetComponent<Renderer>().material.shader.name.Contains("Pointcube"))
								{
									objToModif.GetComponent<Renderer>().material.SetTexture("_Diffuse", texture);
									objToModif.GetComponent<Renderer>().material.SetTexture("_Normal",textureN);	
									objToModif.GetComponent<Renderer>().material.SetTexture("_HueMask",textureH);	
									objToModif.GetComponent<Renderer>().material.SetTexture("_SpecMask",textureS);

								}
								else
									objToModif.GetComponent<Renderer>().material.mainTexture = texture;
								
								float scale = textureData.GetScale();
								/*if (objToModif.name.CompareTo("plage")==0)
									scale/=2.0f;*/
								objToModif.GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(scale,scale));
								
								if(textureData.GetTextureType2()!=null)
								{
									Transform objToModif2 = transform.Find (textureData.GetTextureType2());	
									if (objToModif2 != null)
									{
										//objToModif2.renderer.material.color = new Color(0.78f,0.78f,0.78f,1.0f);
										//StartCoroutine (ApplyTexture2 (objToModif2, textureData, _objectModel.GetLibrary ()));									
										Texture2D texture2 = assets.LoadAsset (textureData.GetFilePath2 (),typeof (Texture2D)) as Texture2D;
										objToModif2.GetComponent<Renderer>().material.color = new Color(0.78f,0.78f,0.78f,1.0f);//TODO Verifier que la couleur soit du blanc
										objToModif2.GetComponent<Renderer>().material.mainTexture = texture;
										objToModif2.GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(scale,scale));
									}
								}
							}
						}
						else if (indexTex < /*colorCount + textureCount*/ colorCount)
						{
							int colorIndex = Mathf.Abs (indexTex /*- textureCount*/);
							if (colorIndex < colorCount)
							{
								objToModif.GetComponent<Renderer>().material.mainTexture = null;//TODO Mettre une texture nulle pour avoir un fond uni
								if((objToModif.GetComponent<Renderer>().material.HasProperty("_DecalTex")) && (usefullData.lowTechnologie))
								{
									objToModif.GetComponent<Renderer>().material.mainTexture = objToModif.GetComponent<Renderer>().material.GetTexture("_DecalTex");
								}
								objToModif.GetComponent<Renderer>().material.color = module.GetColorList () [colorIndex].GetColor ();
								
								OSLibColor osLibColor=module.GetColorList () [colorIndex];
								if(osLibColor.GetColorType2()!=null)
								{
									Color color2 = osLibColor.GetColor2();
									Transform objToModif2 = transform.Find (osLibColor.GetColorType2());
									if (objToModif2 != null)
									{
										objToModif2.GetComponent<Renderer>().material.mainTexture = null;//TODO Mettre une texture nulle pour avoir un fond uni
										if((objToModif2.GetComponent<Renderer>().material.HasProperty("_DecalTex")) && (usefullData.lowTechnologie))
										{
											objToModif2.GetComponent<Renderer>().material.mainTexture = objToModif2.GetComponent<Renderer>().material.GetTexture("_DecalTex");
										}
										objToModif2.GetComponent<Renderer>().material.color = color2;
									}
								}
							}
						}
					}
				}
			}
			if(assets!=null)
				assets.Unload (false);
		}

		Montage.assetBundleLock = false;

	}
	
	public IDictionary getConfiguration()
	{
		return subObjects;	
	}
	
	public bool isPrimaryObject()
	{
		if(_objectModel.getCategory().GetParentCategory() == null) // que un type
		{
			return _objectModel.getCategory().isPrimaryType();
		}
		else // type plus marque
		{
			return _objectModel.getCategory().GetParentCategory().isPrimaryType();
		}
	}
	
	public void updateSpecFcns()
	{
		specFcns.Clear();
		foreach(Component c in GetComponents<MonoBehaviour>())
		{
			if(c.GetType().ToString().StartsWith("Function"))
			{
				specFcns.Add(c);
			}				
		}
	}
	
	public MonoBehaviour[] getSpecFcns()
	{
		MonoBehaviour[] components = new MonoBehaviour[specFcns.Count];
		for(int i=0;i<specFcns.Count;i++)
		{
			components[i] = (MonoBehaviour)specFcns[i];
		}
		return components;
	}
}
