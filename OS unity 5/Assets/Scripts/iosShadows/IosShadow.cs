using UnityEngine;

//[AddComponentMenu("")]
// Classe de gestion d'un système d'ombre projetée (caméra + projecteur) pour iOS.
public class IosShadow : MonoBehaviour
{
    private GameObject      m_shadedObject;
//    private RenderTexture   m_shadowMap;
    private int             m_targetLayer;
    private int             m_oldTargetLayer;
    private int             m_texSize;

    private int             savedPixelLightCount;
    private float           m_lightness;
    public Texture          m_fadeoutTexture;

    private float           m_projSize;
    private float           m_lastScaleFactor;      // Dernier facteur d'échelle appliqué (!= indScaleFactor !)
	private float			m_indScaleFactor;       // Facteur d'échelle additionnel, pour chaque ombre de manière indépendante
    private static int      s_uniqueLayer = 31;     // Doit être le même que celui de IosShadowManager
	
	
    //-----------------------------------------------------
    // Shader du matériau de la texture d'ombre
    private static string shadowMatString = @"Shader ""Hidden/ShadowMat"" {
    Properties {
        _Color (""Color"", Color) = (0,0,0,0)
    }
    SubShader {
        Pass {
            ZTest Greater Cull Off ZWrite Off
            Color [_Color]
            SetTexture [_Dummy] { combine primary }
        }
    }
    Fallback off
}";
 
//	public Material shadowMaterial;
    // Matériau de la texture d'ombre
    private Material m_ShadowMaterial = null;
    private Material shadowMaterial {
        get {
            if (m_ShadowMaterial == null) {
                m_ShadowMaterial = new Material (shadowMatString);
                m_ShadowMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
                m_ShadowMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_ShadowMaterial;
        }
    }
    
        // Shader du matériau du projecteur
    private static string projectorMatString = @"   Shader ""Hidden/ShadowProjectorMultiply"" { 
    Properties {
        _ShadowTex (""Cookie"", 2D) = ""white"" { TexGen ObjectLinear }
        _FalloffTex (""FallOff"", 2D) = ""white"" { TexGen ObjectLinear }
    }
    Subshader {
        Pass {
            ZWrite off
            Offset -1, -1
            Fog { Color (1, 1, 1) }
            AlphaTest Greater 0
            ColorMask RGB
            Blend DstColor Zero
            SetTexture [_ShadowTex] {
                combine texture, ONE - texture
                Matrix [_Projector]
            }
            SetTexture [_FalloffTex] {
                constantColor (1,1,1,0)
                combine previous lerp (texture) constant
                Matrix [_ProjectorClip]
            }
        }
    }
}";

//	public Material projectorMaterial;
    // Matériau du projecteur
    private Material m_ProjectorMaterial = null;
    private Material projectorMaterial {
        get {
            if (m_ProjectorMaterial == null) {
                m_ProjectorMaterial = new Material (projectorMatString);
                m_ProjectorMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
                m_ProjectorMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_ProjectorMaterial;
        }
    }
    
    //-----------------------------------------------------
    public void Init(GameObject shadedObject, int texSize, float lightness, float projSize, Vector3 projPos)
    {
        m_shadedObject = shadedObject;
        m_lightness    = lightness;
        m_texSize      = texSize;
        m_projSize     = projSize;
		m_indScaleFactor  = 1f;
		m_lastScaleFactor = 1f;
		
        // -- Projecteur & textures --
        RenderTexture shmap = new RenderTexture(texSize, texSize, 16);
        shmap.isPowerOfTwo = true;
//        m_shadowMap = shmap;
     
        Projector proj = transform.GetComponent<Projector>();
        proj.material = projectorMaterial;
        proj.material.SetTexture("_FalloffTex", m_fadeoutTexture);
        proj.material.SetTexture("_ShadowTex", shmap);
        
        transform.GetComponent<Camera>().targetTexture = shmap;
    } // Init()
    
    //-----------------------------------------------------
    public void ChangeRenderTex(int newSize)
    {
        m_texSize = newSize;
        transform.GetComponent<Camera>().targetTexture.Release();
        RenderTexture shmap = new RenderTexture(newSize, newSize, 16);
        shmap.isPowerOfTwo = true;
     
        Projector proj = transform.GetComponent<Projector>();
        proj.material.SetTexture("_ShadowTex", shmap);
        
        transform.GetComponent<Camera>().targetTexture = shmap;
    }
    
    //-----------------------------------------------------
	void OnPreCull()
	{
        savedPixelLightCount = QualitySettings.pixelLightCount;
        m_oldTargetLayer = m_shadedObject.layer;
        SetLayerRecursive (m_shadedObject.transform, s_uniqueLayer, m_oldTargetLayer);
	}
    
    //-----------------------------------------------------
	void OnPostRender()
	{
        SetLayerRecursive(m_shadedObject.transform, m_oldTargetLayer, s_uniqueLayer);
        
        //RenderTexture oldRT = RenderTexture.active;
        //RenderTexture.active = shadowmap;
        shadowMaterial.color = new Color (m_lightness, m_lightness, m_lightness, m_lightness);
        
        GL.PushMatrix ();
        GL.LoadOrtho ();
        // LoadOrtho loads -1..100 depth range in Unity's
        // conventions. We invert it for OpenGL
        // and put the depth at the very far end.
        const float depth = -99.99f;
        
        for (int i = 0; i < shadowMaterial.passCount; i++)
        {
            shadowMaterial.SetPass (i);
            GL.Begin (GL.QUADS);
            GL.TexCoord2 (0, 0);
            GL.Vertex3 (0, 0, depth);
            GL.TexCoord2 (1, 0);
            GL.Vertex3 (1, 0, depth);
            GL.TexCoord2 (1, 1);
            GL.Vertex3 (1, 1, depth);
            GL.TexCoord2 (0, 1);
            GL.Vertex3 (0, 1, depth);
            GL.End ();
        }
        GL.PopMatrix ();

        QualitySettings.pixelLightCount = savedPixelLightCount;
	} // OnPostRender()
    
    //-----------------------------------------------------
    private static void SetLayerRecursive(Transform tr, int layer, int whereLayer)
    {
        GameObject go = tr.gameObject;
        if(go.layer == whereLayer)
            go.layer = layer;
        
        foreach(Transform child in tr)
            SetLayerRecursive(child, layer, whereLayer);
    }
    
    //-----------------------------------------------------
    public void SetLightness(float lightness)
    {
        m_lightness = lightness;
    }
 
    //-----------------------------------------------------
    public int GetTexSize()
    {
        return m_texSize;
    }

    //-----------------------------------------------------
    public void Scale(float scaleFactor)
    {
//        Debug.Log ("IOS SHadow scale : "+m_shadedObject.name+" projSize = "+m_projSize);
        m_lastScaleFactor = scaleFactor;

		scaleFactor *= m_indScaleFactor;
        transform.GetComponent<Camera>().farClipPlane     = m_projSize*scaleFactor;
        transform.GetComponent<Camera>().nearClipPlane    = -m_projSize*scaleFactor;
        transform.GetComponent<Camera>().orthographicSize = m_projSize*scaleFactor;
        transform.GetComponent<Projector>().farClipPlane     = m_projSize*scaleFactor;
        transform.GetComponent<Projector>().nearClipPlane    = -m_projSize*scaleFactor;
        transform.GetComponent<Projector>().orthographicSize = m_projSize*scaleFactor;
    }
	
    //-----------------------------------------------------
    public void AddScaleFactor(float scaleFactor)
    {
        m_indScaleFactor = scaleFactor;
        Scale(m_lastScaleFactor);
    }
} // class IosShadow
