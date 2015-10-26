Shader "Custom/2SidedSelfIllumTransparent" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	//_Illum ("Illumin (A)", 2D) = "white" {}
	//_EmissionLM ("Emission (Lightmapper)", Float) = 0
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	Cull Off
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
fixed4 _Color;
//sampler2D _Illum;

struct Input {
	float2 uv_MainTex;
	//float2 uv_Illum;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
	o.Emission = c.rgb;
}
ENDCG
}

Fallback "Transparent/VertexLit"
}
