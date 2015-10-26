Shader "Custom/Bumped Shadow SpecularColor" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_ShadowTex ("Shadow", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_DecalTex ("Decal", 2D) = "white" {}
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 250
	
CGPROGRAM
#pragma surface surf BlinnPhong


sampler2D _MainTex;
sampler2D _ShadowTex;
sampler2D _DecalTex;
sampler2D _BumpMap;
half _Shininess;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float2 uv2_ShadowTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex)*_Color;
	fixed4 shadow = tex2D(_ShadowTex, IN.uv2_ShadowTex);
	fixed4 decal = tex2D(_DecalTex, IN.uv_MainTex);
	tex*= decal;
	tex*= shadow;
	o.Albedo = tex.rgb;
	o.Gloss = tex.a;
	o.Alpha = tex.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

FallBack "Specular"
}
