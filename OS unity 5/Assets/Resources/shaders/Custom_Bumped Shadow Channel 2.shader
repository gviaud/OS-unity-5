// Simplified Bumped shader. Differences from regular Bumped one:
// - no Main Color
// - Normalmap uses Tiling/Offset of the Base texture
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Custom/Bumped Shadow Channel 2" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Bumpmap", 2D) = "bump" {}
	_ShadowTex ("Shadow", 2D) = "white" {}
	_NormalMap ("Normalmap", 2D) = "bump" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 250

CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _NormalMap;
sampler2D _ShadowTex;

struct Input {
	float2 uv_MainTex;
	float2 uv2_ShadowTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 shadow = tex2D(_ShadowTex, IN.uv2_ShadowTex);
	c*= shadow;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
	//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex))+UnpackNormal(tex2D(_NormalMap, IN.uv2_ShadowTex));
	
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex))*0.5;
	o.Normal += UnpackNormal(tex2D(_NormalMap, IN.uv2_ShadowTex))*0.5;
}
ENDCG  
}

FallBack "Mobile/Diffuse"
}
