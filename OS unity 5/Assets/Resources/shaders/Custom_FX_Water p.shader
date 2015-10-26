#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "Custom/FX/Water p" { 
Properties {
	//_Color ("Main Color", Color) = (1,1,1,0.5) 
	ks ("Specular reflectance", Vector) = (19,9,-16,-7) 
	_SpecularColor ("Specular color", COLOR)  = ( .72, .72, .72, 1)
	_Shininess ("Specular shininess", Range (0.1, 600.0)) = 150.0
	//_Shininess ("Specular shininess", Float ) = 150.0
	//_Emission ("Emmisive Color", Color) = (0,0,0,0) 
	 _LightPos ("LightPos", Vector) = (0.0,0.463,0.882,0)//(0,0,0)
	
	_WaveScale ("Wave scale", Float) = 0.1//Range (0.02,0.15)) = 0.063
	_ReflDistort ("Reflection distort", Float) = 0.44 // Range (0,1.5)) = 0.44
	_RefrDistort ("Refraction distort", Float) = 0.4 //Range (0,1.5)) = 0.40
	_RefrColor ("Refraction color", COLOR)  = ( .34, .85, .92, 1)
	_Fresnel ("Fresnel (A) ", 2D) = "gray" {}
	_BumpMap ("Normalmap ", 2D) = "bump" {}
	WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	_ReflectiveColor ("Reflective color (RGB) fresnel (A) ", 2D) = "" {}
	_ReflectiveColorCube ("Reflective color cube (RGB) fresnel (A)", Cube) = "" { TexGen CubeReflect }
	_HorizonColor ("Simple water horizon color", COLOR)  = ( .172, .463, .435, 1)
	_MainTex ("Fallback texture", 2D) = "" {}
	_ReflectionTex ("Internal Reflection", 2D) = "" {}
	_RefractionTex ("Internal Refraction", 2D) = "" {} 
	
	_azimut ("Azimut (don't touch)", Float) = 1.0 //Range (0,1.5)) = 0.40
//	ke ("Emissive reflectance", Vector) = (19,9,-16,-7) 
//	ka ("Ambient reflectance", Vector) = (19,9,-16,-7) 
//	kd ("Diffuse reflectance", Vector) = (19,9,-16,-7) 
	
}


// -----------------------------------------------------------
// Fragment program cards

Subshader { 
	Tags { "WaterMode"="Refractive" "RenderType"="Transparent" "LightMode" = "ForwardBase" }
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE WATER_SIMPLE
		//#pragma surface surf BlinnPhong
		
		#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
		#define HAS_REFLECTION 1
		#endif
		#if defined (WATER_REFRACTIVE)
		#define HAS_REFRACTION 1
		#endif		
		
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		
		uniform float4 _WaveScale4;
		uniform float4 _WaveOffset;
		
		#if HAS_REFLECTION
		uniform float _ReflDistort;
		#endif
		#if HAS_REFRACTION
		uniform float _RefrDistort;
		#endif
		float _Shininess;
		float3 _SpecularColor; 
		float4 _LightPos; 
		float _azimut;
		//Float3 Ks;
		//uniform float4 _WorldLightDir;
		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};
		
		struct v2f {
			float4 pos : SV_POSITION;
			#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
				float4 ref : TEXCOORD0;
				float2 bumpuv0 : TEXCOORD1;
				float2 bumpuv1 : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
			#else
				float2 bumpuv0 : TEXCOORD0;
				float2 bumpuv1 : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
			#endif
			float4 V : TEXCOORD4; 
			float4 L : TEXCOORD5; 
			
//			Float3 globalAmbient;
//			Float3 lightColor;
//			Float3 lightPosition;
//			Float3 eyePosition;
//			Float3 Ke;
//			Float3 Ka;
//			Float3 Kd;
			
			//Float shininess;			
		};
	
		v2f vert(appdata v)
		{
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			
			// scroll bump waves
			float4 temp;
			temp.xyzw = v.vertex.xzxz * _WaveScale4 / 1.0 + _WaveOffset;
			o.bumpuv0 = temp.xy;
			o.bumpuv1 = temp.wz;
			
			//o.Specular = _Shininess;
			//object space view direction (will normalize per pixel)
			o.viewDir.xzy = ObjSpaceViewDir(v.vertex);
			
			#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
			o.ref = ComputeScreenPos(o.pos);
			#endif
			//	o.V=mul (UNITY_MATRIX_MVP, v.vertex);//normalize(v.vertex.xzxz ) ; 
		
			o.V.xzy =  ObjSpaceViewDir(v.vertex);
			//	o.L.xzy =  ObjSpaceLightDir(v.vertex);
			float4 temp2 = _WorldSpaceLightPos0 * _azimut;
			float3 objSpaceLightPos = mul(_World2Object, temp2).xyz;		
			//o.L.xzy=  dot(v.vertex.xyz, _LightPos.xyz)/objSpaceLightPos.xyz;	
			o.L.xzy=  objSpaceLightPos.xyz - v.vertex.xyz * temp2.w ;	
			//o.L.x =(dot(_LightPos.xyz, v.normal)*0.5 +0.5) ;// - v.vertex.xyz * _LightPos.w;	
			//o.L.y= (dot(_LightPos.xyz, v.normal)*0.5 +0.5);
			//o.L.z=(dot(_LightPos.xyz, v.normal)*0.5 +0.5) ;
			
			return o;
		}

	
		#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
		sampler2D _ReflectionTex;
		#endif
		#if defined (WATER_REFLECTIVE) || defined (WATER_SIMPLE)
		sampler2D _ReflectiveColor;
		#endif
		#if defined (WATER_REFRACTIVE)
		sampler2D _Fresnel;
		sampler2D _RefractionTex;
		uniform float4 _RefrColor;
		
		#endif
		#if defined (WATER_SIMPLE)
		uniform float4 _HorizonColor;
		#endif
		sampler2D _BumpMap;
		//float3 _LightPos; 
	
		half4 frag( v2f i ) : COLOR
		{
			i.viewDir = normalize(i.viewDir);
			
			// combine two scrolling bumpmaps into one
			half3 bump1 = UnpackNormal(tex2D( _BumpMap, i.bumpuv0 )).rgb;
			half3 bump2 = UnpackNormal(tex2D( _BumpMap, i.bumpuv1 )).rgb;
			half3 bump = (bump1 + bump2) * 2;//0.5;
			
			// fresnel factor
			half fresnelFac = dot( i.viewDir, bump );
			
			// perturb reflection/refraction UVs by bumpmap, and lookup colors
			
			#if HAS_REFLECTION
			float4 uv1 = i.ref; 
			uv1.xy += bump * _ReflDistort;
			half4 refl = tex2Dproj( _ReflectionTex, UNITY_PROJ_COORD(uv1) );
			//half4 refl = (_Shininess,_Shininess,_Shininess,_Shininess);
			#endif
			#if HAS_REFRACTION
			float4 uv2 = i.ref;
			uv2.xy -= bump * _RefrDistort;
			half4 refr = tex2Dproj( _RefractionTex, UNITY_PROJ_COORD(uv2) ) * _RefrColor;
			//half4 refr = (0.1,0.1,0.1,0.1);
			#endif
			
			// final color is between refracted and reflected based on fresnel	
			half4 color;
			//float3 normalFromBumpMap = tex2D(_BumpMap, i.uv.xy).xyz * 2 - 1;
			
			#if defined(WATER_REFRACTIVE)
			half fresnel = tex2D( _Fresnel, float2(fresnelFac,fresnelFac) ).a;
			color = lerp( refr, refl, fresnel );	
			//color = refr;		
			#endif
			
			#if defined(WATER_REFLECTIVE)
			half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
			color.rgb = refl.rgb;		
			//color.rgb = lerp( water.rgb, refl.rgb, water.a );			
			color.a = 0;//refl.a * water.a;
			#endif
			
			#if defined(WATER_SIMPLE)
			half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
			color.rgb = lerp( water.rgb, _HorizonColor.rgb, water.a );
			color.a = _HorizonColor.a ;
			#endif

			float3 V = normalize(i.V*float3(1,1,1)); 	
			
		//	float3 L = normalize(_WorldSpaceLightPos0.xyz*float3(1,1,1)); 
			float3 L = i.L.xyz;
			
		//	color.xyz =i.L.xyz;
		//	return color;	
			
			float3 N = normalize(bump);			
			float3 R =  normalize(2 * dot(N, L)*N-L);					
			float i_spec = pow(max(dot(V, R), 0.0),_Shininess*0.5);  
			float3 specular = 0.5*_SpecularColor*i_spec;			
			color.xyz = color.xyz+specular;
			
			return color;
		}
	
	ENDCG

	}

} 
SubShader {
	Tags { "WaterMode"="Simple" "RenderType"="Opaque" }
    Pass 
    {
    	SetTexture[_MainTex] {Combine texture * constant ConstantColor[_HorizonColor]}
    }
  }
}
