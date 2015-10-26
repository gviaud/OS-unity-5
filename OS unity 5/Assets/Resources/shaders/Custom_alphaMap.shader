Shader "Custom/alphaMap" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AlphaTex ("Alpha", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent"  "RenderType"="Transparent"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alphatest:_Cutoff 

sampler2D _MainTex;
sampler2D _AlphaTex;

struct Input {
	float2 uv_MainTex;
	float2 uv_AlphaTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	half4 c = tex2D (_MainTex, IN.uv_MainTex);
	half4 shadow = tex2D(_AlphaTex, IN.uv_AlphaTex);
	
	o.Albedo = c.rgb;
	o.Alpha = shadow.r;
}
ENDCG
}
Fallback "Transparent/VertexLit"
}