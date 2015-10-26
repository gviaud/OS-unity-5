Shader "Custom/2SidedSelfIllum" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_EmissionLM ("Emission (Lightmapper)", Float) = 1.0
}

SubShader {     
	Cull Off
	LOD 200

CGPROGRAM         
#pragma surface surf Lambert

sampler2D _MainTex;
fixed4 _Color;
half _EmissionLM;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	half4 c = tex2D (_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb * _Color.rgb;
	//o.Gloss = c.a;
	o.Emission = c.rgb * _EmissionLM;
}
ENDCG
}
}