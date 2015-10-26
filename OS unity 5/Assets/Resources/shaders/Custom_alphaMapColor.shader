Shader "Custom/alphaMapColor" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AlphaTex ("Alpha", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
}

SubShader {
	Tags {"Queue"="Transparent"  "RenderType"="Transparent"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
sampler2D _AlphaTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float2 uv_AlphaTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	half4 c = tex2D (_MainTex, IN.uv_MainTex);
	half4 shadow = tex2D(_AlphaTex, IN.uv_AlphaTex);
	
	o.Albedo = c.rgb * _Color.rgb;
	o.Alpha = shadow.r;
}
ENDCG
}
Fallback "Transparent/VertexLit"
}