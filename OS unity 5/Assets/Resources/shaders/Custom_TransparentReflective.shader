Shader "Custom/TransparentReflective" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 0)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
	//_Cube2 ("Reflection Texture", 2D) =  "white" {}
}

SubShader {
	LOD 300
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	Cull Off
	
CGPROGRAM
#pragma surface surf BlinnPhong alpha

sampler2D _MainTex;
samplerCUBE _Cube;
//sampler2D _Cube2;

fixed4 _Color;
fixed4 _ReflectColor;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	float3 worldRefl;
	float3 viewDir;
	float3 worldNormal;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = tex.a;
	o.Specular = _Shininess;
	
	fixed4 reflcol = texCUBE (_Cube, IN.worldRefl);
	//fixed4 reflcol = tex2D(_Cube2, IN.uv_MainTex * IN.viewDir);
	reflcol *= tex.a;
	o.Emission = reflcol.rgb * _ReflectColor.rgb;

	o.Alpha = reflcol.a * _Color.a;
}
ENDCG
} 

FallBack "Transparent/Specular"
}