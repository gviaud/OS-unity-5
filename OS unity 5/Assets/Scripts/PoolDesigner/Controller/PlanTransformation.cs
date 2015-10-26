using UnityEngine;
using System.Collections;

using Pointcube.Global;

public class PlanTransformation : MonoBehaviour 
{
	// facteur de division a applique lorsque l'on recentre la vue
	protected static float DIVIDE_FACTOR = 1.8f;
		
	protected Matrix4x4 _transformationMatrix = Matrix4x4.identity;
	protected Matrix4x4 _translationMatrix;
	protected Matrix4x4 _scaleMatrix;
	
	protected Vector2 _transformedMousePosition;
	protected Vector2 _mousePosition;
	protected Vector2 _prevMousePosition;
	protected Vector2 _prevScaleVector;
	
	protected bool    _clickBegan;

    protected Vector2 _planTransla;

	protected float _virtualScaleFactor = 1;
	
	protected Vector3 _animatedScale = new Vector3(1.0f,1.0f,1.0f);
	protected Vector2 _animatedTranslation;
	
//	protected PoolDesignerUI _gui;
	protected PoolUIv2 _gui;
	
	protected bool _IsInitialized = false;
		
	// Use this for initialization
	void Start () 
	{
		_prevMousePosition = PC.In.GetCursorPosInvY(); //new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
//		_gui = GameObject.Find("MainScene").GetComponent<PoolDesignerUI> ();
		_gui = GameObject.Find("MainScene").GetComponent<PoolUIv2> ();
	}
	
	// Update is called once per frame
	void Update () 
	{		
//#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR
		_mousePosition = PC.In.GetCursorPosInvY();
		
		Matrix4x4 transformedMatrixInverse = _transformationMatrix.inverse;
		_transformedMousePosition = transformedMatrixInverse.MultiplyPoint (_mousePosition);
		
//		Vector2 deltaMousePosition = _mousePosition - _prevMousePosition;
		Vector2 translation = Vector2.zero;
		Vector3 scale = Vector3.one;

        if(_gui.GetCurrentMode () == PoolDesignerMode.PlanTransformation ||
           _gui.GetCurrentMode () == PoolDesignerMode.BackgroundTranslation
		/*	||_gui.GetCurrentMode () == PoolDesignerMode.PolygonMoveAndScale*/)    // Quick-fix PC.In
        {
            float   deltaScale;
            Vector2 deltaDrag;
            PC.In.Zoom_n_drag1(out deltaScale, out deltaDrag);
            _planTransla.Set(deltaDrag.x, -deltaDrag.y);

    		if (_gui.GetCurrentMode () == PoolDesignerMode.PlanTransformation) // applique des modifs de la vue uniquement dans le mode associe
    		{
    			if(PC.In.Click1Hold())
    			{
    				Vector3 currentScale = new Vector3 (_transformationMatrix.m00, 
    				                                    _transformationMatrix.m11, 
    				                                    _transformationMatrix.m22);
    			
    				Matrix4x4 currentScaleMatrix = Matrix4x4.Scale (currentScale).inverse;
    				translation = currentScaleMatrix.MultiplyVector (_planTransla);//new Vector2 (deltaMousePosition.x, deltaMousePosition.y));
    			}
    
    			float scaleFactor = _virtualScaleFactor + (deltaScale*0.3f /*(Input.GetAxis("Mouse ScrollWheel"))*/) * 0.5f;

    			if (scaleFactor > 0)
    			{
    				scale = new Vector3 (scaleFactor / _virtualScaleFactor, scaleFactor / _virtualScaleFactor, scaleFactor / _virtualScaleFactor);
    				_virtualScaleFactor = scaleFactor;
    			}
    		}
			if (_gui.GetCurrentMode () == PoolDesignerMode.PolygonMoveAndScale) // applique des modifs de la vue uniquement dans le mode associe
    		{
			//	float scaleFactor = _virtualScaleFactor + (deltaScale*0.3f /*(Input.GetAxis("Mouse ScrollWheel"))*/) * 0.5f;

    		/*	if (scaleFactor > 0)
    			{
    				scale = new Vector3 (scaleFactor / _virtualScaleFactor, scaleFactor / _virtualScaleFactor, scaleFactor / _virtualScaleFactor);
    				_virtualScaleFactor = scaleFactor;
					
					_animatedScale = new Vector3 (scaleFactor, scaleFactor, 1);
					Matrix4x4 scaleMatrix = Matrix4x4.Scale (_animatedScale);
					_transformationMatrix = _transformationMatrix * scaleMatrix;
    			}*/
				

    		/*	if (deltaScale !=0)
    			{
					
    				scale = new Vector3 (deltaScale+_virtualScaleFactor , deltaScale+_virtualScaleFactor , deltaScale+_virtualScaleFactor);
    			}
				*/
			}

        }
//#endif
        _clickBegan = PC.In.Click1Down();

		_translationMatrix = Matrix4x4.TRS (translation, Quaternion.identity, Vector3.one);
		_scaleMatrix = Matrix4x4.Scale (scale);
		
		Matrix4x4 translateToScaleMatrix = Matrix4x4.TRS (_transformedMousePosition,
					                                      Quaternion.identity,
					                                      Vector3.one);
		
		_transformationMatrix = _transformationMatrix *
			                    translateToScaleMatrix * 
			                    _scaleMatrix * 
				                translateToScaleMatrix.inverse *
				                _translationMatrix;
		
		_prevMousePosition = _mousePosition;
	}
	
	public Matrix4x4 GetMatrix ()
	{
		return _transformationMatrix;	
	}
	
	public Vector2 GetTransformedMousePosition ()
	{
		return _transformedMousePosition;
	}
	
	public Vector3 GetScale ()
	{
		return new Vector3 (_transformationMatrix.m00, 
				            _transformationMatrix.m11, 
				            _transformationMatrix.m22);
	}
	
	public Vector3 GetTranslation ()
	{
		return new Vector3 (_transformationMatrix.m03, 
				            _transformationMatrix.m13, 
				            _transformationMatrix.m23);	
	}

	public bool GetClickBegan()
	{
		return _clickBegan;
	}
	
	// modifie la matrice de transformation afin de centrer le polygon et de le voir dans son ensemble
	public void ZoomToPolygon (Polygon polygon, Vector2 offSet)
	{
		Bounds polyBound = polygon.bounds;
		Vector2 center = new Vector2 (polyBound.center.x + offSet.x, polyBound.center.z+offSet.y);
		Vector2 upLeftTranslation = new Vector2 (-polyBound.extents.x, -polyBound.extents.z);
		Vector2 upLeftPoint = center + upLeftTranslation;
		
		float width = polyBound.size.x;
		float height = polyBound.size.z;
		
		float scaleFactor = 1;
		
		if (width > height)
		{
			if (width == 0)
			{
				_transformationMatrix = Matrix4x4.identity;
				_virtualScaleFactor = 1;
				return;
			}
			
			scaleFactor = Screen.width / (width * DIVIDE_FACTOR);
		}
		else
		{
			scaleFactor = Screen.height / (height * DIVIDE_FACTOR);
			
			if (height == 0)
			{
				_transformationMatrix = Matrix4x4.identity;	
				_virtualScaleFactor = 1;
				return;
			}
		}
		
		_animatedScale = new Vector3 (scaleFactor, scaleFactor, 1);
		
		Matrix4x4 scaleMatrix = Matrix4x4.Scale (_animatedScale);
		
		_animatedTranslation = _transformationMatrix.MultiplyVector (-upLeftPoint);
		Matrix4x4 translationMatrix = Matrix4x4.TRS (_animatedTranslation, Quaternion.identity, Vector3.one);
		
		Matrix4x4 translateAndScaleMatrix = translationMatrix * scaleMatrix;
		
		Vector2 transformedScreenCenter = new Vector2 (Screen.width / 2, Screen.height / 2);
		Vector2 transformedCenter = translateAndScaleMatrix.MultiplyPoint (center);
		Vector2 centerTranslation = translateAndScaleMatrix.inverse.MultiplyVector (transformedScreenCenter - transformedCenter);

		Matrix4x4 centerMatrix = Matrix4x4.TRS (centerTranslation,
			                                    Quaternion.identity,
			                                    Vector3.one);
		if(!_IsInitialized)
		{
			_transformationMatrix = translateAndScaleMatrix * centerMatrix;
			_IsInitialized = true;
		}
	}
	
	// modifie la matrice de transformation afin de centrer l'image et de la voir dans son ensemble
	public void ZoomToImage (Rect imageBound, Vector2 offSet, float scaleImageFactor)
	{ 
		ZoomToImage (imageBound, offSet, scaleImageFactor, false);
	}
	public void ZoomToImage (Rect imageBound, Vector2 offSet, float scaleImageFactor, bool deltaScale)
	{
		//Bounds polyBound = polygon.bounds;
		Vector2 center = new Vector2 (imageBound.center.x + offSet.x, imageBound.center.y+offSet.y);
		Vector2 upLeftTranslation = new Vector2 (-imageBound.width*2 , -imageBound.height*2);
		Vector2 upLeftPoint = center + upLeftTranslation;		
		if(deltaScale)
		{
			scaleImageFactor*=_animatedScale.x;
		}
			
		_animatedScale = new Vector3 (scaleImageFactor, scaleImageFactor, 1);
		
		Matrix4x4 scaleMatrix = Matrix4x4.Scale (_animatedScale);
		
		_animatedTranslation = _transformationMatrix.MultiplyVector (-upLeftPoint);
		Matrix4x4 translationMatrix = Matrix4x4.TRS (_animatedTranslation, Quaternion.identity, Vector3.one);
		
		Matrix4x4 translateAndScaleMatrix = translationMatrix * scaleMatrix;
		
		Vector2 transformedScreenCenter = new Vector2 (Screen.width / 2, Screen.height / 2);
		Vector2 transformedCenter = translateAndScaleMatrix.MultiplyPoint (center);
		Vector2 centerTranslation = translateAndScaleMatrix.inverse.MultiplyVector (transformedScreenCenter - transformedCenter);

		Matrix4x4 centerMatrix = Matrix4x4.TRS (centerTranslation,
			                                    Quaternion.identity,
			                                    Vector3.one);
//		if(!_IsInitialized)
		{
			_transformationMatrix = translateAndScaleMatrix * centerMatrix;
			_IsInitialized = true;
		}
	}
}
