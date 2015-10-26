Shader "Custom/Multiply" {
    Properties {
    
    
        _Color ("Main Color", Color) = (1,1,1,0)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _ShadowTex ("ShadowTex (RGB)", 2D) = "white" {}
        _DispertionTex ("DispertionTex (RGB)", 2D) = "white" {}
    //    _WaterCol ("Dispersion Color", Color) = (0.424, 0.9511,0.9953,0)
		_DecalTex ("Decal (RGBA)", 2D) = "black" {}
        
    }
   
SubShader {         
	Tags { "RenderType"="Opaque" }
	LOD 250
	
CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
sampler2D _ShadowTex;
sampler2D _DispertionTex;
sampler2D _DecalTex;
fixed4 _Color;


struct Input {
	float2 uv_MainTex;
	float2 uv2_ShadowTex;
	float2 uv2_DispertionTex;
	float2 uv_DecalTex;
};

void surf (Input IN, inout SurfaceOutput o) 
{

	half4 c = tex2D(_MainTex, IN.uv_MainTex);
	
	c *= _Color;	
	
	half4 shadow = tex2D(_ShadowTex, IN.uv2_ShadowTex);
	half4 disp = tex2D(_DispertionTex, IN.uv2_DispertionTex);	
	half4 decal = tex2D(_DecalTex, IN.uv_DecalTex);	
	
	//Shadow texture
	//Shadow Map très claire
	//c *= shadow;	
	//Shadow Map très foncée
//	c *= (shadow+0.55);	
//	c *= (shadow+0.2);	
//	c *= shadow;
	

	
                
	fixed3 absorbtion =  fixed3(0.624, 0.0511,0.0053) ;			
	disp = 1-disp-0.1;
	disp.r /= 25.0;
	c.r *= exp(-absorbtion.r * disp.r * 55.5);
	c.g *= exp(-absorbtion.g * disp.r * 55.5);
	c.b *= exp(-absorbtion.b * disp.r * 55.5);	 

		//Caustic texture
	float causticLvl = decal.r;
	const float seuil = 0.5; // sépare les caustiques et les dépressions
	const float factorCaustics =0.42f;
	
	causticLvl -= seuil;
	if(causticLvl > 0.0) 	// caustique
		causticLvl = causticLvl / (1.0-seuil)/1.5; 
	else 				// dépression
		causticLvl = causticLvl / (seuil*2.0); 
	causticLvl *= factorCaustics;
	
	causticLvl *= clamp(disp.g*2.0, 0.0, 1.0);	

	c.rgb += causticLvl;
	
	c.rgb = clamp(c.rgb, 0.0, 1.0);	
	
	o.Alpha = c.a;	
	
	o.Albedo = c.rgb;
	          
}
ENDCG
}

}