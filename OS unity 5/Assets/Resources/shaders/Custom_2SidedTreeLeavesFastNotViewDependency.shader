Shader "Custom/2SidedTreeLeavesFastNotViewDependency" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
	
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_DecalTex ("Decal", 2D) = "white" {}
	_ShadowTex ("Shadow", 2D) = "white" {}
	
	_Scale ("Scale", Vector) = (1,1,1,1)
	_SquashAmount ("Squash", Float) = 1	
	

}

SubShader { 
	Tags { "IgnoreProjector"="True" "RenderType"="TreeLeafCustom" }
	Cull Off
	LOD 200
		
CGPROGRAM
#pragma surface surf TreeLeafCustom alphatest:_Cutoff 
#pragma exclude_renderers molehill
#pragma glsl_no_auto_normalization
#include "TreeCustom.cginc"

sampler2D _MainTex;
sampler2D _DecalTex;
sampler2D _ShadowTex;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	float2 uv2_ShadowTex;
	fixed4 color : COLOR;
};


void surf (Input IN, inout LeafSurfaceOutput o) 
{
	fixed4 c = fixed4(0.0,0.0,0.0,0.0);
	c= tex2D(_MainTex, IN.uv_MainTex);
	
	fixed4 shadow = tex2D(_ShadowTex, IN.uv2_ShadowTex);
	fixed4 decal = tex2D(_DecalTex, IN.uv_MainTex);
	c*= decal;
	c*= shadow;
	o.Albedo = c.rgb * _Color.rgb * IN.color.a;
	o.Alpha = c.a;
}
ENDCG
}

Dependency "OptimizedShader" = "Hidden/Nature/Tree Creator Leaves Optimized"
FallBack "Diffuse"
}
