Shader "Add" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_DecalTex ("Decal (RGBA)", 2D) = "black" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 250
	
CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
sampler2D _DecalTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float2 uv_DecalTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	half4 decal = tex2D(_DecalTex, IN.uv_DecalTex);
	//c.rgb = lerp (c.rgb, decal.rgb, decal.a);
	//c *= _Color;
	
	float causticLvl = decal.r;
	const float seuil = 0.5; // sépare les caustiques et les dépressions
	causticLvl -= seuil;
	if(causticLvl > 0.0) 	// caustique
		causticLvl = causticLvl / (1.0-seuil)/1.5; // "/1.5" : pour diminuer
	else 				// dépression
		causticLvl = causticLvl / (seuil*2.0); // "*2" : réglage pour diminuer
		
	//causticLvl += 0.5f;
	causticLvl *= 0.5f;
	// caustiques suivant la profondeur
	causticLvl *= clamp(0.45, 0.0, 1.0);
	
	fixed3 causticLvl3 =  fixed3(causticLvl, causticLvl, causticLvl);
	c += causticLvl;
	c *= _Color;
	
	
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Diffuse"
}
