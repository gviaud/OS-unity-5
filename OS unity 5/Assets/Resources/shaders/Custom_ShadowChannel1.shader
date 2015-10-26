// Simplified Bumped shader. Differences from regular Bumped one:
// - no Main Color
// - Normalmap uses Tiling/Offset of the Base texture
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Custom/ShadowChannel1" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ShadowTex ("Shadow", 2D) = "white" {}
	_Color ("Main Color", Color) = (1,1,1,1)
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 250

CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
sampler2D _ShadowTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float2 uv_ShadowTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	c *= _Color;
	fixed4 shadow = tex2D(_ShadowTex, IN.uv_ShadowTex);
	c*= shadow;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG  
}

FallBack "Mobile/Diffuse"
}
