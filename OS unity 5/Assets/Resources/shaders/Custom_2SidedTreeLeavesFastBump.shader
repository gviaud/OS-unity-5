Shader "Custom/2SidedTreeLeavesFastBump" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_TranslucencyColor ("Translucency Color", Color) = (0.73,0.85,0.41,1) // (187,219,106,255)
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
	_TranslucencyViewDependency ("View dependency", Range(0,1)) = 0.7
	_ShadowStrength("Shadow Strength", Range(0,1)) = 1.0
	
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	
	// These are here only to provide default values
	_Scale ("Scale", Vector) = (1,1,1,1)
	_SquashAmount ("Squash", Float) = 1	
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_DecalTex ("Decal", 2D) = "white" {}
	
	//_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	//_BumpMap ("Normalmap", 2D) = "bump" {}
	//_GlossMap ("Gloss (A)", 2D) = "black" {}
	//_TranslucencyMap ("Translucency (A)", 2D) = "white" {}
	//_ShadowOffset ("Shadow Offset (A)", 2D) = "black" {}

}

SubShader { 
	Tags { "IgnoreProjector"="True" "RenderType"="TreeLeaf" }
	Cull Off
	LOD 200
		
CGPROGRAM
#pragma surface surf TreeLeaf alphatest:_Cutoff vertex:TreeVertLeaf addshadow nolightmap
#pragma exclude_renderers molehill
#pragma glsl_no_auto_normalization
#include "Tree.cginc"

sampler2D _MainTex;
sampler2D _DecalTex;
//sampler2D _BumpMap;
//sampler2D _GlossMap;
//sampler2D _TranslucencyMap;
half _Shininess;
sampler2D _BumpMap;

struct Input {
	float2 uv_MainTex;
	fixed4 color : COLOR; // color.a = AO
};

void surf (Input IN, inout LeafSurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 decal = tex2D(_DecalTex, IN.uv_MainTex);
	c*= decal;
	o.Albedo = c.rgb * _Color.rgb * IN.color.a;
	o.Translucency = 0.0f;// tex2D(_TranslucencyMap, IN.uv_MainTex).rgb;
	o.Gloss = 0.0f;//tex2D(_GlossMap, IN.uv_MainTex).a;
	o.Alpha = c.a;
	o.Specular = 0.078125;
	//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
	o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

Dependency "OptimizedShader" = "Hidden/Nature/Tree Creator Leaves Optimized"
FallBack "Diffuse"
}
