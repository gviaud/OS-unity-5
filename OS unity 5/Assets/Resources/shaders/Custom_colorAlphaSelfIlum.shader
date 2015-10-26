Shader "Custom/colorAlphaSelfIlum" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_AlphaTex ("Alpha", 2D) = "white" {}
		_Illum ("Illumin (A)", 2D) = "white" {}
		_EmissionLM ("Emission (Lightmapper)", Float) = 1.0
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha

sampler2D _AlphaTex;
sampler2D _Illum;
fixed4 _Color;
half _EmissionLM;

struct Input
{
	float2 uv_AlphaTex;
	float2 uv_Illum;
};
	void surf (Input IN, inout SurfaceOutput o) {
	half4 shadow = tex2D(_AlphaTex, IN.uv_AlphaTex);

	o.Albedo = _Color;
	o.Alpha = shadow.r * _Color.a;
	//o.Emission = c.rgb * tex2D(_Illum, IN.uv_Illum);
	o.Emission = _Color * _EmissionLM;
	
		}
		ENDCG
	} 
Fallback "Transparent/VertexLit"
}
