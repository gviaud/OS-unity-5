Shader "Custom/alpa2Sided" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AlphaTex ("Alpha", 2D) = "white" {}
     //   _ShadowTex ("ShadowTex (RGB)", 2D) = "white" {}
     //   _NormalTex ("Normal (RGB)", 2D) = "bump" {}
        _Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_Color ("Main Color", Color) = (1,1,1,1)
}

SubShader {     
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	      Cull Off
	LOD 200

CGPROGRAM         
#pragma surface surf BlinnPhong alpha

sampler2D _MainTex;
sampler2D _AlphaTex;
//sampler2D _ShadowTex;
//sampler2D _NormalTex;
half _Shininess;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float2 uv_AlphaTex;
	float2 uv2_ShadowTex;
	float2 uv2_NormalTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	half4 c = tex2D (_MainTex, IN.uv_MainTex);
	half4 alpha = tex2D(_AlphaTex, IN.uv_AlphaTex);
//	half4 shadow = tex2D(_ShadowTex, IN.uv2_ShadowTex);
//	half4 normalMap = tex2D(_NormalTex, IN.uv2_NormalTex);
	
	//c *= (shadow+0.55);	
	
	o.Albedo = c.rgb * _Color.rgb;
//	o.Normal = UnpackNormal (normalMap);
	o.Gloss = c.a;		
	o.Alpha = alpha.r;
	o.Specular = _Shininess;
}
ENDCG
}
FallBack "Transparent/VertexLit"
}