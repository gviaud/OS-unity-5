Shader "Custom/alphaSelfIlum" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AlphaTex ("Alpha", 2D) = "white" {}
		_Illum ("Illumin (A)", 2D) = "white" {}
		_EmissionLM ("Emission (Lightmapper)", Float) = 1.0
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha

sampler2D _MainTex;
sampler2D _AlphaTex;
sampler2D _Illum;
fixed4 _Color;
half _EmissionLM;

struct Input {
	float2 uv_MainTex;
	float2 uv_AlphaTex;
	float2 uv_Illum;
};
		void surf (Input IN, inout SurfaceOutput o) {
	half4 c = tex2D (_MainTex, IN.uv_MainTex);
	half4 shadow = tex2D(_AlphaTex, IN.uv_AlphaTex);

	o.Albedo = c.rgb;
	o.Alpha = shadow.r;
	//o.Emission = c.rgb * tex2D(_Illum, IN.uv_Illum);
	o.Emission = c.rgb * _EmissionLM;
	
		}
		ENDCG
	} 
Fallback "Transparent/VertexLit"
}
