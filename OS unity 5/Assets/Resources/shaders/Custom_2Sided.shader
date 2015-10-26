Shader "Custom/2Sided" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
}

SubShader {   
	Tags { "RenderType"="Opaque" }  
	Cull Off
	LOD 200

CGPROGRAM         
#pragma surface surf Lambert

sampler2D _MainTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	half4 c = tex2D (_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb * _Color.rgb;
	o.Gloss = c.a;		
}
ENDCG
}
Fallback "VertexLit"
}