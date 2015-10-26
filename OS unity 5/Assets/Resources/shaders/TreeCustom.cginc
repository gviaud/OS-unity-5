#ifndef TREE_CUSTOM_CG_INCLUDED
#define TREE_CUSTOM_CG_INCLUDED

#include "TerrainEngine.cginc"

fixed4 _Color;

struct LeafSurfaceOutput {
	fixed3 Albedo;
	fixed3 Normal;
	fixed3 Emission;
	half Specular;
	fixed Alpha;
};

inline half4 LightingTreeLeafCustom (LeafSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
{
	half4 c;
	c.rgb = s.Albedo * _LightColor0.rgb;	
	return c;	
	
}

#endif