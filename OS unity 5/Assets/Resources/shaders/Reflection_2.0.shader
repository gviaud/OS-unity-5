// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position

Shader "Custom/Reflection.2.0" { 
Properties {
    _IlluminCol("Main Color", Color) = (1,1,1,0.5)
    _ReflectionTex("Texture", 2D) = "white" { }
}
SubShader {
Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
Blend SrcAlpha OneMinusSrcAlpha
    Pass {
ZWrite  Off
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

float4 _IlluminCol;
sampler2D _ReflectionTex;
sampler2D _ReflectionTex2;

struct appdata {
    float4 vertex : POSITION;
    float4 uv : TEXCOORD0;
   };

struct v2f {
    float4 pos : SV_POSITION;
    float4  uv : TEXCOORD0;
    float4  uvscale : TEXCOORD1;
//	float2 uv2 : TEXCOORD1;
    
};
float4 _ReflectionTex_ST;
uniform float4x4 _ProjMatrixReflection;

float4 positionInProjSpace; 

v2f vert (appdata v) {
    v2f o;
    o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
    o.uvscale = mul( UNITY_MATRIX_MVP, v.vertex );
    o.uv = mul(_ProjMatrixReflection, v.vertex); 
  
    
    return o;
}

half4 frag( v2f i ) : COLOR {
	float uvscaleFinal = i.uvscale.w;       
	
   	half4 texcol2 = tex2D(_ReflectionTex,i.uv.xy/uvscaleFinal ); 
   	
   	float offsetAlphaMinus = 0.5;
   	float offsetAlphaMas = 0.5;
   	if(
   		  ((texcol2.x>0.45)&&(texcol2.x<0.55))
   		 && ((texcol2.y>0.45)&&(texcol2.y<0.55))
   		 && ((texcol2.z>0.45)&&(texcol2.z<0.55))
   		)
   	{
		texcol2= half4(0,0,0,0);
   	}
   	else
   	{
   		texcol2 =  texcol2 * _IlluminCol;
	}
	return  texcol2;
}

ENDCG

    }
}

    SubShader {
        Tags {"RenderType"="Opaque" }
		Pass {
            Material {
                Diffuse [_IlluminCol]
                Ambient [_IlluminCol]
                Shininess [_Shininess]
                Specular [_SpecColor]
                Emission [_Emission]
            }
            Lighting On
        }
        }

Fallback "TransparentCutout "
} 