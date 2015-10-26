using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Genere une piscine complete a partir d'une forme de base
// represente par un polygone ferme
public class PoolGenerator
{	
	protected Polygon _transformedPolygon;
	
	protected RimGenerator _rimGenerator;
	
	protected GameObject _thePool;
	
	protected MeshCollider _meshCollider;
	
	protected Transform _liner;
	protected Transform _sidewalk;
	protected Transform _rim;
	protected Transform _water;
	protected Transform _frieze;
	protected Transform _mask;
	protected Transform _occlusion;
	
	protected float _polygonScaleFactor = 0.01f;
	
	protected Material _linerMaterial;
	protected Material _sidewalkMaterial;
	protected Material _rimMaterial;
	protected Material _waterMaterial;
	protected Material _maskMaterial;
	protected Material _occluMaterial;
	
	protected void InitPoolComponent ()
	{
		// Pour chaque composant de la piscine on verifie s'il n'existe pas deja
		// dans le Gameobject (copy par exemple)
		// sinon on le crée puis on le configure (position, orientation, scale, components et materiaux)
		
		// liner
		_liner = _thePool.transform.FindChild ("liner");
		
		if (_liner == null)
		{
			GameObject liner = new GameObject ("liner");
			liner.layer = 14; // underwater
			
			_liner = liner.transform;
			_liner.parent = _thePool.transform;
			_liner.localPosition = Vector3.zero;
			_liner.localRotation = Quaternion.identity;
			_liner.localScale = Vector3.one;
			_liner.Translate (0, -0.10f, 0);
			
			//AnimatedTexture_decal caustics = liner.AddComponent<AnimatedTexture_decal> ();
		
			liner.AddComponent<MeshFilter> ();
			liner.AddComponent<MeshRenderer> ();
			
			_linerMaterial = new Material (Shader.Find ("Custom/Multiply"));
			_linerMaterial.color = new Color32 (183, 183, 183, 255);
			_linerMaterial.SetTexture ("_DispertionTex", Resources.Load ("PoolDesigner/gradient") as Texture2D);
			
		/*	AnimatedTexture_decal animatedDecal = liner.AddComponent<AnimatedTexture_decal> ();		
			animatedDecal.speed=0.1f;	
			animatedDecal.tiling=0.8f;
			animatedDecal.nameImgList="caustics";
			animatedDecal.nameShader="Custom/Multiply";
			animatedDecal.nameTexture="_DecalTex";*/
			
			liner.GetComponent<Renderer>().material = _linerMaterial;
		}
		
		// sidewalk
		_sidewalk = _thePool.transform.FindChild ("plage");
		
		if (_sidewalk == null)
		{
			GameObject sidewalk = new GameObject ("plage");
			
			_sidewalk = sidewalk.transform;
			_sidewalk.parent = _thePool.transform;
			_sidewalk.localPosition = Vector3.zero;
			_sidewalk.localRotation = Quaternion.identity;
			_sidewalk.localScale = Vector3.one;
			
			sidewalk.AddComponent<MeshFilter> ();
			sidewalk.AddComponent<MeshRenderer> ();
			
			sidewalk.GetComponent<Renderer>().material = _sidewalkMaterial;
		}
		
		// rim
		_rimGenerator = new RimGenerator(_thePool.transform);
		_rim = _rimGenerator.GetParentGameObject ().transform;
		
		MeshFilter rimFilter = _rim.gameObject.GetComponent <MeshFilter> ();
		if (rimFilter == null)
		{
			rimFilter = _rim.gameObject.AddComponent <MeshFilter> ();
		}
		//rimFilter.mesh = new Mesh ();
		
		// occlusion
		_occlusion = _thePool.transform.FindChild ("occlusion");
		
		if (_occlusion == null)
		{
			GameObject occlusion = new GameObject ("occlusion");
			
			_occlusion = occlusion.transform;
			_occlusion.parent = _thePool.transform;
			_occlusion.localPosition = Vector3.zero;
			_occlusion.localRotation = Quaternion.identity;
			_occlusion.localScale = Vector3.one;
			_occlusion.Translate (0, 0.001f, 0);
			
			occlusion.AddComponent<MeshFilter> ();
			occlusion.AddComponent<MeshRenderer> ();
			
			_occluMaterial = new Material (Shader.Find ("Transparent/Diffuse"));
			_occluMaterial.mainTexture = Resources.Load ("PoolDesigner/SolOcclu") as Texture2D;
			occlusion.GetComponent<Renderer>().material = _occluMaterial;
		}
		
		// water
		_water = _thePool.transform.FindChild ("water");
		
		if (_water == null)
		{
			GameObject water = new GameObject ("water");
			
			water.layer = 4; //Water
			_water = water.transform;
			
			_water.parent = _thePool.transform;
			_water.localPosition = Vector3.zero;
			_water.localRotation = Quaternion.identity;
			_water.localScale = Vector3.one;
			_water.Translate (0, -0.10f, 0);
			
			water.AddComponent<MeshFilter> ();
			water.AddComponent<MeshRenderer> ();
			
			Water waterScript = water.AddComponent<Water> ();
			int layer1 = 14; //underwater 
			int layer2 = 21; // underWaterCamera
			
			int layerMask1 = 1 << layer1;
			int layerMask2 = 1 << layer2;
			int finalLayer = layerMask1 | layerMask2;
			waterScript.m_RefractLayers = finalLayer;
			
			int layer3 = 9; // Raycaster
			int layerMask3 = 1 << layer3;
			
			int layer4 = 18;
			int layerMask4 = 1 << layer4; // planReflectWater
			waterScript.m_ReflectLayers = layerMask3 | layerMask4;
			
			_waterMaterial = new Material (Shader.Find ("Custom/FX/Water p"));
			_waterMaterial.SetColor ("_SpecularColor", new Color32 (253, 253, 253, 219));
			_waterMaterial.SetFloat ("_Shininess", 432.3896f);
			_waterMaterial.SetFloat ("_WaveScale", 0.02403846f);
			_waterMaterial.SetFloat ("_ReflDistort", 0.0576923f);
			_waterMaterial.SetFloat ("_RefrDistort", 0.735689f);
			_waterMaterial.SetColor ("_RefrColor", new Color32 (215, 250, 255, 146));
			_waterMaterial.SetTexture ("_BumpMap", Resources.Load ("PoolDesigner/Waterbump") as Texture2D);
			_waterMaterial.SetTexture ("_Fresnel", Resources.Load ("PoolDesigner/fresnel_noir_20") as Texture2D);
			
			water.GetComponent<Renderer>().material = _waterMaterial;
		}
		
		// frieze
		_frieze = _thePool.transform.FindChild ("frise");
		
		if (_frieze == null)
		{
			GameObject frieze = new GameObject ("frise");
			
			frieze.layer = 21; // underWaterCamera
			_frieze = frieze.transform;
			_frieze.parent = _thePool.transform;
			_frieze.localPosition = Vector3.zero;
			_frieze.localRotation = Quaternion.identity;
			_frieze.localScale = Vector3.one;
			
			frieze.AddComponent<MeshFilter> ();
			frieze.AddComponent<MeshRenderer> ();
		}
		
		_frieze.GetComponent<Renderer>().material = _liner.GetComponent<Renderer>().material;
		
		// mask
		_mask = _thePool.transform.FindChild ("maskf");
		
		if (_mask == null)
		{
			GameObject mask = new GameObject ("maskf");
		
			_mask = mask.transform;
			_mask.parent = _thePool.transform;
			_mask.localPosition = Vector3.zero;
			_mask.localRotation = Quaternion.identity;
			_mask.localScale = Vector3.one;
			
			mask.AddComponent<MeshFilter> ();
			mask.AddComponent<MeshRenderer> ();
			mask.AddComponent<Mask> ();
			
			_maskMaterial = new Material (Shader.Find("Custom/inverseNormalDiffuse"));
			mask.GetComponent<Renderer>().material = _maskMaterial;
		}
		
		// add physics for click, selection and collision
		Rigidbody rigidBody = _thePool.GetComponent<Rigidbody> ();
		if (rigidBody == null)
		{
			rigidBody = _thePool.AddComponent<Rigidbody> ();	
		}
		
		_meshCollider = _thePool.GetComponent<MeshCollider> ();
		if (_meshCollider == null)
		{
			_meshCollider = _thePool.AddComponent<MeshCollider> ();	
		}
	}
	
	public PoolGenerator ()
	{
		
	}
	
	public PoolGenerator (GameObject parentGameObject)
	{
		_thePool = parentGameObject;		
		InitPoolComponent ();
	}
	
	public void Generate (Polygon _polygon)
	{
		//_polygon = new Polygon(_polygon.GenerateWithCurveInverse());
		// On genere les meshs des composants de la piscine
		
		if (_polygon.GetPoints ().Count < 3)
			return;
		
		if (!_polygon.IsClosed ())
		{
			_polygon.Close ();
		}
		
		_polygon = _polygon.GetMirrorX();
		
		_polygon.UpdateBounds ();
		Vector2 translation = _polygon.GetPolygonCenter () * -1;
		Vector2 mirror = new Vector2(1,-1);
		// exprime le polygone en coordonnées monde, avec son repere centré sur son centre
		PolygonRawData polyRaw = new PolygonRawData ();
		foreach (Vector2 pt in _polygon.GenerateWithCurve ())
		//foreach (Vector2 pt in _polygon.GenerateWithCurveInverse ())
		//PolygonRawData polydata = _polygon.GenerateWithCurveInverse ();
		//polydata.Reverse();
		//foreach (Vector2 pt in polydata)
		{
			Vector2 transformedPt = (pt + translation) * _polygonScaleFactor;
			/*Vector2 newpt = new Vector2();
			newpt.x=pt.x;
			newpt.y=pt.y;
			
			newpt.y = 2*_polygon.bounds.center.z - newpt.y;
			Vector2 transformedPt = (newpt + translation) * _polygonScaleFactor;*/
			polyRaw.Add (transformedPt);
		}
		
		_transformedPolygon = new Polygon (polyRaw);
		
		// generate liner
		Mesh linerMesh = _liner.GetComponent<MeshFilter> ().mesh;
		PolygonExtruder linerExtrusion = new PolygonExtruder (_transformedPolygon, linerMesh, -1.5f, false, true, true);
		linerExtrusion.Generate ();
		
		LinerScatteringMapper lsm = new LinerScatteringMapper (linerMesh);
		lsm.Generate ();
		
		PolygonExtruderMapper pem = new PolygonExtruderMapper (linerExtrusion);
		pem.Generate ();
		
		// generate sidewalk
		Mesh sidewalkMesh = _sidewalk.GetComponent<MeshFilter> ().mesh;
		SideWalkExtruder se = new SideWalkExtruder (_transformedPolygon, sidewalkMesh, -0.02f, 2);
		se.Generate ();
		
		PlannarMapper pm = new PlannarMapper (sidewalkMesh, Vector3.up);
		pm.Generate ();
		
		// generate rim
		_rimGenerator.Generate (_polygon);
		
		// generate occlusion
		PolygonRawData occluShape = new PolygonRawData ();
		occluShape.Add (new Vector2 (0.28f, 0));
		occluShape.Add (new Vector2 (0.33f, 0));
		
		Polygon occluProfile = new Polygon (occluShape);
		occluProfile.SetClosed (false);
		
		Mesh occluMesh = _occlusion.GetComponent<MeshFilter> ().mesh;
		PolygonLofter occluLofter = new PolygonLofter (_transformedPolygon, occluProfile, occluMesh);
		_occlusion.GetComponent<Renderer>().enabled = false;
		occluLofter.Generate ();
		
		// generate water
		Mesh waterMesh = _water.GetComponent<MeshFilter> ().mesh;
		PolygonExtruder waterPlan = new PolygonExtruder (_transformedPolygon, waterMesh, 0, true, false, false);
		waterPlan.Generate ();
		
		PlannarMapper waterMapper = new PlannarMapper (waterMesh, Vector3.down, UVChannel.uv0, 5);
		waterMapper.Generate ();
		
		// generate frieze
		Mesh friezeMesh = _frieze.GetComponent<MeshFilter> ().mesh;
		PolygonExtruder friezeExtrusion = new PolygonExtruder (_transformedPolygon, friezeMesh, -0.10f, false, false, true);
		friezeExtrusion.Generate ();
		
		PolygonExtruderMapper friezeMapper = new PolygonExtruderMapper (friezeExtrusion);
		friezeMapper.Generate ();
		
		LinerScatteringMapper lsmFrieze = new LinerScatteringMapper (friezeMesh);
		lsmFrieze.Generate ();
		
		// generate mask
		Mesh maskMesh = _mask.GetComponent<MeshFilter> ().mesh;
		PolygonExtruder maskExtrusion = new PolygonExtruder (_transformedPolygon, maskMesh, -0.10f, false, false, true);
		maskExtrusion.Generate ();
		
		PolygonExtruderMapper maskMapper = new PolygonExtruderMapper (maskExtrusion);
		maskMapper.Generate ();
		
		// generate collision mesh
		Mesh collisionMesh = new Mesh ();
		collisionMesh.name = "collision";
		
		Polygon collisionPoly = PolygonOperation.GetOutlinedPolygon (_transformedPolygon, 0.33f);
		PolygonExtruder collisionExtruder = new PolygonExtruder(collisionPoly, collisionMesh, 0.07f, true, false, false);
		collisionExtruder.Generate ();
		
		_meshCollider.sharedMesh = collisionMesh;
		_meshCollider.convex = true;
		collisionMesh.RecalculateBounds ();
		
		// on ajoute le mesh de collision au renderer parent de la margelle
		// pour le calcul de la boundingBox lié a la fonction de redimensionnement
		// de la plage.
		_rim.gameObject.GetComponent<MeshFilter> ().mesh = collisionMesh;
	}
	
	public GameObject GetParentGameObject ()
	{
		return _thePool;	
	}
	/*
	public void SetParentGameObject (GameObject go)
	{
		_thePool = go;
	}
	
	public Polygon GetPolygon ()
	{
		return _polygon;	
	}
	
	public void SetPolygon (Polygon polygon)
	{
		_polygon = polygon;	
	}*/
	
	public int GetRimCount ()
	{
		return _rimGenerator.GetRimCount ();	
	}
	
	// obtiens la liste des transform de la margelle contenant les renderers (masquer la margelle)
	public List<Transform> GetRimObjects ()
	{
		List<Transform> rimObjects = new List<Transform> ();
		
		foreach (Transform t in _rim)
		{
			if (t.name == "portion")	
			{
				rimObjects.Add (t);
			}
			else if (t.name == "RimCorner(Clone)")
			{
				rimObjects.Add (t.GetChild (0).GetChild (0));
			}
		}
		
		rimObjects.Add (_occlusion);
		
		return rimObjects;
	}
}