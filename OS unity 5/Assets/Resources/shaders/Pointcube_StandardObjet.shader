// Shader created with Shader Forge v1.17 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.17;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:3,spmd:0,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,rpth:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:0,x:34380,y:32675,varname:node_0,prsc:2|diff-2822-OUT,spec-2561-OUT,gloss-4680-OUT,normal-7750-OUT,emission-1421-OUT;n:type:ShaderForge.SFN_Tex2d,id:123,x:33619,y:32785,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:_Normal,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:bbab0a6f7bae9cf42bf057d8ee2755f6,ntxv:3,isnm:True|UVIN-165-UVOUT,MIP-2165-OUT;n:type:ShaderForge.SFN_Multiply,id:1075,x:34307,y:32378,varname:node_1075,prsc:2|A-2651-OUT,B-8216-OUT;n:type:ShaderForge.SFN_Tex2d,id:4036,x:33799,y:32545,ptovrint:False,ptlb:Diffuse,ptin:_Diffuse,varname:_Normal_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:b66bceaf0cc0ace4e9bdc92f14bba709,ntxv:2,isnm:False|UVIN-165-UVOUT,MIP-2165-OUT;n:type:ShaderForge.SFN_Slider,id:4680,x:32760,y:32781,ptovrint:False,ptlb:gloss,ptin:_gloss,varname:_node_7318_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5986067,max:1;n:type:ShaderForge.SFN_Slider,id:5196,x:32760,y:33194,ptovrint:False,ptlb:UV translation,ptin:_UVtranslation,varname:_Roughness_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:10;n:type:ShaderForge.SFN_Slider,id:2165,x:32760,y:32693,ptovrint:False,ptlb:Blur,ptin:_Blur,varname:_Roughness_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:5;n:type:ShaderForge.SFN_Color,id:7994,x:33831,y:32247,ptovrint:False,ptlb:Lightness color,ptin:_Lightnesscolor,varname:node_7994,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:2651,x:34143,y:32236,varname:node_2651,prsc:2|A-7994-RGB,B-8162-OUT;n:type:ShaderForge.SFN_Slider,id:8162,x:32760,y:32442,ptovrint:False,ptlb:Ligntness,ptin:_Ligntness,varname:node_8162,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:2.217465,max:10;n:type:ShaderForge.SFN_Desaturate,id:5127,x:33980,y:32398,varname:node_5127,prsc:2|COL-9899-OUT,DES-6321-OUT;n:type:ShaderForge.SFN_Slider,id:9256,x:32760,y:32524,ptovrint:False,ptlb:Saturation,ptin:_Saturation,varname:node_9256,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1.140978,max:2;n:type:ShaderForge.SFN_Rotator,id:165,x:33290,y:32677,varname:node_165,prsc:2|UVIN-3020-OUT,ANG-7559-OUT;n:type:ShaderForge.SFN_Slider,id:7495,x:32760,y:33029,ptovrint:False,ptlb:UV Rotation,ptin:_UVRotation,varname:node_7495,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:7;n:type:ShaderForge.SFN_Clamp,id:7559,x:33097,y:32728,varname:node_7559,prsc:2|IN-7495-OUT,MIN-6836-OUT,MAX-8418-OUT;n:type:ShaderForge.SFN_Vector1,id:6836,x:32628,y:33002,varname:node_6836,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:8418,x:32628,y:33051,varname:node_8418,prsc:2,v1:6.28;n:type:ShaderForge.SFN_Subtract,id:6321,x:33980,y:32545,varname:node_6321,prsc:2|A-728-OUT,B-9256-OUT;n:type:ShaderForge.SFN_Vector1,id:728,x:34087,y:32589,varname:node_728,prsc:2,v1:1;n:type:ShaderForge.SFN_Color,id:9453,x:33621,y:32107,ptovrint:False,ptlb:Hue color,ptin:_Huecolor,varname:node_9453,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.1405769,c3:0,c4:1;n:type:ShaderForge.SFN_Add,id:9899,x:33799,y:32398,varname:node_9899,prsc:2|A-8246-OUT,B-4036-RGB;n:type:ShaderForge.SFN_Append,id:6566,x:33109,y:32217,varname:node_6566,prsc:2|A-7048-OUT,B-377-OUT;n:type:ShaderForge.SFN_Append,id:8202,x:33278,y:32217,varname:node_8202,prsc:2|A-6566-OUT,B-7911-OUT;n:type:ShaderForge.SFN_Vector1,id:7048,x:32930,y:32049,cmnt:rouge,varname:node_7048,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:377,x:32930,y:32127,cmnt:vert,varname:node_377,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:7911,x:33278,y:32355,cmnt:Bleu,varname:node_7911,prsc:2,v1:0;n:type:ShaderForge.SFN_Slider,id:1428,x:32760,y:32867,ptovrint:False,ptlb:Specular level,ptin:_Specularlevel,varname:node_1428,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1020299,max:1;n:type:ShaderForge.SFN_Slider,id:8448,x:32760,y:32611,ptovrint:False,ptlb:Hue level,ptin:_Huelevel,varname:node_8448,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:5,max:5;n:type:ShaderForge.SFN_Multiply,id:8246,x:33615,y:32398,varname:node_8246,prsc:2|A-9076-OUT,B-4281-OUT;n:type:ShaderForge.SFN_Add,id:9076,x:33440,y:32217,varname:node_9076,prsc:2|A-8202-OUT,B-8448-OUT;n:type:ShaderForge.SFN_Color,id:7681,x:34020,y:32798,ptovrint:False,ptlb:Emissive color,ptin:_Emissivecolor,varname:node_7681,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_SwitchProperty,id:1421,x:34020,y:32971,ptovrint:False,ptlb:Bulb,ptin:_Bulb,varname:node_1421,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-7681-RGB,B-5184-OUT;n:type:ShaderForge.SFN_Vector1,id:2159,x:33472,y:32971,cmnt:rouge,varname:node_2159,prsc:2,v1:255;n:type:ShaderForge.SFN_Vector1,id:8445,x:33472,y:33040,cmnt:vert,varname:node_8445,prsc:2,v1:255;n:type:ShaderForge.SFN_Append,id:6775,x:33619,y:32971,varname:node_6775,prsc:2|A-2159-OUT,B-8445-OUT;n:type:ShaderForge.SFN_Append,id:5184,x:33799,y:32971,varname:node_5184,prsc:2|A-6775-OUT,B-5974-OUT;n:type:ShaderForge.SFN_Vector1,id:5974,x:33799,y:33108,cmnt:Bleu,varname:node_5974,prsc:2,v1:255;n:type:ShaderForge.SFN_SwitchProperty,id:8216,x:34143,y:32398,ptovrint:False,ptlb:Difuse map,ptin:_Difusemap,varname:node_8216,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-2628-RGB,B-5127-OUT;n:type:ShaderForge.SFN_Color,id:2628,x:33980,y:32247,ptovrint:False,ptlb:Solid color,ptin:_Solidcolor,varname:node_2628,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Slider,id:3342,x:32760,y:33388,ptovrint:False,ptlb:Reflexion intensity,ptin:_Reflexionintensity,varname:node_3342,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:10;n:type:ShaderForge.SFN_Cubemap,id:6765,x:33746,y:33188,ptovrint:False,ptlb:CubeMap,ptin:_CubeMap,varname:node_6765,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,pvfc:0|MIP-8750-OUT;n:type:ShaderForge.SFN_Slider,id:8750,x:32760,y:33296,ptovrint:False,ptlb:Reflexion Blur,ptin:_ReflexionBlur,varname:_Reflexionintensity_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:5;n:type:ShaderForge.SFN_Add,id:34,x:34465,y:32378,varname:node_34,prsc:2|A-1075-OUT,B-2853-OUT;n:type:ShaderForge.SFN_Vector1,id:256,x:33799,y:33364,cmnt:rouge,varname:node_256,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:4807,x:33799,y:33444,cmnt:vert,varname:node_4807,prsc:2,v1:0;n:type:ShaderForge.SFN_Append,id:4824,x:33978,y:33388,varname:node_4824,prsc:2|A-256-OUT,B-4807-OUT;n:type:ShaderForge.SFN_Append,id:8255,x:34147,y:33388,varname:node_8255,prsc:2|A-4824-OUT,B-5692-OUT;n:type:ShaderForge.SFN_Vector1,id:5692,x:34147,y:33526,cmnt:Bleu,varname:node_5692,prsc:2,v1:0;n:type:ShaderForge.SFN_SwitchProperty,id:2853,x:34167,y:33217,ptovrint:False,ptlb:Reflexion,ptin:_Reflexion,varname:node_2853,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-8255-OUT,B-175-OUT;n:type:ShaderForge.SFN_Multiply,id:175,x:33992,y:33153,varname:node_175,prsc:2|A-6765-RGB,B-3342-OUT;n:type:ShaderForge.SFN_Lerp,id:7750,x:33799,y:32785,varname:node_7750,prsc:2|A-7181-OUT,B-8321-OUT,T-735-OUT;n:type:ShaderForge.SFN_Vector4,id:8321,x:33464,y:32643,varname:node_8321,prsc:2,v1:0,v2:0,v3:1,v4:0;n:type:ShaderForge.SFN_Slider,id:4191,x:32760,y:32949,ptovrint:False,ptlb:Normal level,ptin:_Normallevel,varname:node_4191,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:7181,x:33615,y:32607,varname:node_7181,prsc:2|A-123-RGB,B-3093-OUT;n:type:ShaderForge.SFN_Vector1,id:3093,x:33615,y:32557,varname:node_3093,prsc:2,v1:5;n:type:ShaderForge.SFN_Subtract,id:735,x:33188,y:32908,varname:node_735,prsc:2|A-3041-OUT,B-4191-OUT;n:type:ShaderForge.SFN_Vector1,id:3041,x:33290,y:32952,varname:node_3041,prsc:2,v1:1;n:type:ShaderForge.SFN_TexCoord,id:7384,x:33094,y:32083,varname:node_7384,prsc:2,uv:0;n:type:ShaderForge.SFN_Tex2d,id:7127,x:34650,y:32542,ptovrint:False,ptlb:Lightmap,ptin:_Lightmap,varname:node_7127,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-2443-UVOUT;n:type:ShaderForge.SFN_Multiply,id:2822,x:34650,y:32378,varname:node_2822,prsc:2|A-6326-OUT,B-34-OUT;n:type:ShaderForge.SFN_Multiply,id:3020,x:33288,y:32083,varname:node_3020,prsc:2|A-5097-OUT,B-4983-OUT;n:type:ShaderForge.SFN_Slider,id:4983,x:32760,y:32361,ptovrint:False,ptlb:UV Tile,ptin:_UVTile,varname:node_4983,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.1,cur:1,max:3;n:type:ShaderForge.SFN_Add,id:5097,x:33193,y:31919,varname:node_5097,prsc:2|A-7384-UVOUT,B-5196-OUT;n:type:ShaderForge.SFN_TexCoord,id:2443,x:34465,y:32497,varname:node_2443,prsc:2,uv:1;n:type:ShaderForge.SFN_Slider,id:212,x:32760,y:33488,ptovrint:False,ptlb:Light_Map_Intensity,ptin:_Light_Map_Intensity,varname:node_212,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:0,max:1;n:type:ShaderForge.SFN_Add,id:6326,x:34917,y:32488,varname:node_6326,prsc:2|A-7127-RGB,B-212-OUT;n:type:ShaderForge.SFN_Multiply,id:4281,x:33831,y:32089,varname:node_4281,prsc:2|A-7645-OUT,B-9453-RGB;n:type:ShaderForge.SFN_Tex2d,id:1086,x:33621,y:31925,ptovrint:False,ptlb:Hue Mask,ptin:_HueMask,varname:node_1086,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5b950436b9495ba4896a9700b8fdec8e,ntxv:0,isnm:False|UVIN-165-UVOUT;n:type:ShaderForge.SFN_Multiply,id:7645,x:33841,y:31942,varname:node_7645,prsc:2|A-1086-RGB,B-6649-OUT;n:type:ShaderForge.SFN_Slider,id:6649,x:32760,y:32272,ptovrint:False,ptlb:Hue Mask intensity,ptin:_HueMaskintensity,varname:node_6649,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Tex2d,id:2281,x:33621,y:31746,ptovrint:False,ptlb:Spec Mask,ptin:_SpecMask,varname:_HueMask_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-165-UVOUT;n:type:ShaderForge.SFN_Multiply,id:2561,x:33841,y:31800,varname:node_2561,prsc:2|A-2281-RGB,B-1802-OUT;n:type:ShaderForge.SFN_Vector1,id:766,x:33290,y:32568,varname:node_766,prsc:2,v1:1;n:type:ShaderForge.SFN_Subtract,id:1802,x:33188,y:32524,varname:node_1802,prsc:2|A-766-OUT,B-1428-OUT;proporder:2628-8216-4036-123-4191-7127-212-5196-7495-4983-2165-4680-2281-1428-7994-8162-1086-6649-9453-8448-9256-7681-1421-2853-3342-8750-6765;pass:END;sub:END;*/

Shader "Pointcube/StandardObjet" {
    Properties {
        _Solidcolor ("Solid color", Color) = (0.5,0.5,0.5,1)
        [MaterialToggle] _Difusemap ("Difuse map", Float ) = 0.3686275
        _Diffuse ("Diffuse", 2D) = "black" {}
        _Normal ("Normal", 2D) = "bump" {}
        _Normallevel ("Normal level", Range(0, 1)) = 1
        _Lightmap ("Lightmap", 2D) = "white" {}
        _Light_Map_Intensity ("Light_Map_Intensity", Range(-1, 1)) = 0
        _UVtranslation ("UV translation", Range(0, 10)) = 0
        _UVRotation ("UV Rotation", Range(0, 7)) = 0
        _UVTile ("UV Tile", Range(0.1, 3)) = 1
        _Blur ("Blur", Range(0, 5)) = 0
        _gloss ("gloss", Range(0, 1)) = 0.5986067
        _SpecMask ("Spec Mask", 2D) = "white" {}
        _Specularlevel ("Specular level", Range(0, 1)) = 0.1020299
        _Lightnesscolor ("Lightness color", Color) = (1,1,1,1)
        _Ligntness ("Ligntness", Range(0, 10)) = 2.217465
        _HueMask ("Hue Mask", 2D) = "white" {}
        _HueMaskintensity ("Hue Mask intensity", Range(0, 1)) = 1
        _Huecolor ("Hue color", Color) = (1,0.1405769,0,1)
        _Huelevel ("Hue level", Range(0, 5)) = 5
        _Saturation ("Saturation", Range(0, 2)) = 1.140978
        _Emissivecolor ("Emissive color", Color) = (0,0,0,1)
        [MaterialToggle] _Bulb ("Bulb", Float ) = 0
        [MaterialToggle] _Reflexion ("Reflexion", Float ) = 0
        _Reflexionintensity ("Reflexion intensity", Range(0, 10)) = 0
        _ReflexionBlur ("Reflexion Blur", Range(0, 5)) = 0
        _CubeMap ("CubeMap", Cube) = "_Skybox" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers xbox360 ps3 psp2 
            #pragma target 3.0
            #pragma glsl
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float _gloss;
            uniform float _UVtranslation;
            uniform float _Blur;
            uniform float4 _Lightnesscolor;
            uniform float _Ligntness;
            uniform float _Saturation;
            uniform float _UVRotation;
            uniform float4 _Huecolor;
            uniform float _Specularlevel;
            uniform float _Huelevel;
            uniform float4 _Emissivecolor;
            uniform fixed _Bulb;
            uniform fixed _Difusemap;
            uniform float4 _Solidcolor;
            uniform float _Reflexionintensity;
            uniform samplerCUBE _CubeMap;
            uniform float _ReflexionBlur;
            uniform fixed _Reflexion;
            uniform float _Normallevel;
            uniform sampler2D _Lightmap; uniform float4 _Lightmap_ST;
            uniform float _UVTile;
            uniform float _Light_Map_Intensity;
            uniform sampler2D _HueMask; uniform float4 _HueMask_ST;
            uniform float _HueMaskintensity;
            uniform sampler2D _SpecMask; uniform float4 _SpecMask_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
            #endif
            #ifdef DYNAMICLIGHTMAP_ON
                o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
            #endif
            o.normalDir = UnityObjectToWorldNormal(v.normal);
            o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
            o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
            o.posWorld = mul(_Object2World, v.vertex);
            float3 lightColor = _LightColor0.rgb;
            o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
            UNITY_TRANSFER_FOG(o,o.pos);
            TRANSFER_VERTEX_TO_FRAGMENT(o)
            return o;
        }
        float4 frag(VertexOutput i) : COLOR {
            i.normalDir = normalize(i.normalDir);
            float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/// Vectors:
            float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
            float node_165_ang = clamp(_UVRotation,0.0,6.28);
            float node_165_spd = 1.0;
            float node_165_cos = cos(node_165_spd*node_165_ang);
            float node_165_sin = sin(node_165_spd*node_165_ang);
            float2 node_165_piv = float2(0.5,0.5);
            float2 node_165 = (mul(((i.uv0+_UVtranslation)*_UVTile)-node_165_piv,float2x2( node_165_cos, -node_165_sin, node_165_sin, node_165_cos))+node_165_piv);
            float3 _Normal_var = UnpackNormal(tex2Dlod(_Normal,float4(TRANSFORM_TEX(node_165, _Normal),0.0,_Blur)));
            float3 normalLocal = lerp(float4((_Normal_var.rgb*5.0),0.0),float4(0,0,1,0),(1.0-_Normallevel));
            float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
            
            float nSign = sign( dot( viewDirection, i.normalDir ) ); // Reverse normal if this is a backface
            i.normalDir *= nSign;
            normalDirection *= nSign;
            
            float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
            float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
            float3 lightColor = _LightColor0.rgb;
            float3 halfDirection = normalize(viewDirection+lightDirection);
// Lighting:
            float attenuation = LIGHT_ATTENUATION(i);
            float3 attenColor = attenuation * _LightColor0.xyz;
            float Pi = 3.141592654;
            float InvPi = 0.31830988618;
///// Gloss:
            float gloss = _gloss;
            float specPow = exp2( gloss * 10.0+1.0);
/// GI Data:
            UnityLight light;
            #ifdef LIGHTMAP_OFF
                light.color = lightColor;
                light.dir = lightDirection;
                light.ndotl = LambertTerm (normalDirection, light.dir);
            #else
                light.color = half3(0.f, 0.f, 0.f);
                light.ndotl = 0.0f;
                light.dir = half3(0.f, 0.f, 0.f);
            #endif
            UnityGIInput d;
            d.light = light;
            d.worldPos = i.posWorld.xyz;
            d.worldViewDir = viewDirection;
            d.atten = attenuation;
            #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                d.ambient = 0;
                d.lightmapUV = i.ambientOrLightmapUV;
            #else
                d.ambient = i.ambientOrLightmapUV;
            #endif
            d.boxMax[0] = unity_SpecCube0_BoxMax;
            d.boxMin[0] = unity_SpecCube0_BoxMin;
            d.probePosition[0] = unity_SpecCube0_ProbePosition;
            d.probeHDR[0] = unity_SpecCube0_HDR;
            d.boxMax[1] = unity_SpecCube1_BoxMax;
            d.boxMin[1] = unity_SpecCube1_BoxMin;
            d.probePosition[1] = unity_SpecCube1_ProbePosition;
            d.probeHDR[1] = unity_SpecCube1_HDR;
            UnityGI gi = UnityGlobalIllumination (d, 1, gloss, normalDirection);
            lightDirection = gi.light.dir;
            lightColor = gi.light.color;
// Specular:
            float NdotL = max(0, dot( normalDirection, lightDirection ));
            float LdotH = max(0.0,dot(lightDirection, halfDirection));
            float4 _SpecMask_var = tex2D(_SpecMask,TRANSFORM_TEX(node_165, _SpecMask));
            float3 specularColor = (_SpecMask_var.rgb*(1.0-_Specularlevel));
            float specularMonochrome = max( max(specularColor.r, specularColor.g), specularColor.b);
            float NdotV = max(0.0,dot( normalDirection, viewDirection ));
            float NdotH = max(0.0,dot( normalDirection, halfDirection ));
            float VdotH = max(0.0,dot( viewDirection, halfDirection ));
            float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
            float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
            float specularPBL = max(0, (NdotL*visTerm*normTerm) * unity_LightGammaCorrectionConsts_PIDiv4 );
            float3 directSpecular = 1 * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
            half grazingTerm = saturate( gloss + specularMonochrome );
            float3 indirectSpecular = (gi.indirect.specular);
            indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
            float3 specular = (directSpecular + indirectSpecular);
/// Diffuse:
            NdotL = max(0.0,dot( normalDirection, lightDirection ));
            half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
            float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
            float3 indirectDiffuse = float3(0,0,0);
            indirectDiffuse += gi.indirect.diffuse;
            float4 _Lightmap_var = tex2D(_Lightmap,TRANSFORM_TEX(i.uv1, _Lightmap));
            float4 _HueMask_var = tex2D(_HueMask,TRANSFORM_TEX(node_165, _HueMask));
            float4 _Diffuse_var = tex2Dlod(_Diffuse,float4(TRANSFORM_TEX(node_165, _Diffuse),0.0,_Blur));
            float3 diffuseColor = ((_Lightmap_var.rgb+_Light_Map_Intensity)*(((_Lightnesscolor.rgb*_Ligntness)*lerp( _Solidcolor.rgb, lerp((((float3(float2(0.0,0.0),0.0)+_Huelevel)*((_HueMask_var.rgb*_HueMaskintensity)*_Huecolor.rgb))+_Diffuse_var.rgb),dot((((float3(float2(0.0,0.0),0.0)+_Huelevel)*((_HueMask_var.rgb*_HueMaskintensity)*_Huecolor.rgb))+_Diffuse_var.rgb),float3(0.3,0.59,0.11)),(1.0-_Saturation)), _Difusemap ))+lerp( float3(float2(0.0,0.0),0.0), (texCUBElod(_CubeMap,float4(viewReflectDirection,_ReflexionBlur)).rgb*_Reflexionintensity), _Reflexion )));
            diffuseColor *= 1-specularMonochrome;
            float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
// Emissive:
            float3 emissive = lerp( _Emissivecolor.rgb, float3(float2(255.0,255.0),255.0), _Bulb );
// Final Color:
            float3 finalColor = diffuse + specular + emissive;
            fixed4 finalRGBA = fixed4(finalColor,1);
            UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
            return finalRGBA;
        }
        ENDCG
    }
    Pass {
        Name "Meta"
        Tags {
            "LightMode"="Meta"
        }
        Cull Off
        
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #define UNITY_PASS_META 1
        #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
        #define _GLOSSYENV 1
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "UnityPBSLighting.cginc"
        #include "UnityStandardBRDF.cginc"
        #include "UnityMetaPass.cginc"
        #pragma fragmentoption ARB_precision_hint_fastest
        #pragma multi_compile_shadowcaster
        #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
        #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
        #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
        #pragma multi_compile_fog
        #pragma exclude_renderers xbox360 ps3 psp2 
        #pragma target 3.0
        #pragma glsl
        uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
        uniform float _gloss;
        uniform float _UVtranslation;
        uniform float _Blur;
        uniform float4 _Lightnesscolor;
        uniform float _Ligntness;
        uniform float _Saturation;
        uniform float _UVRotation;
        uniform float4 _Huecolor;
        uniform float _Specularlevel;
        uniform float _Huelevel;
        uniform float4 _Emissivecolor;
        uniform fixed _Bulb;
        uniform fixed _Difusemap;
        uniform float4 _Solidcolor;
        uniform float _Reflexionintensity;
        uniform samplerCUBE _CubeMap;
        uniform float _ReflexionBlur;
        uniform fixed _Reflexion;
        uniform sampler2D _Lightmap; uniform float4 _Lightmap_ST;
        uniform float _UVTile;
        uniform float _Light_Map_Intensity;
        uniform sampler2D _HueMask; uniform float4 _HueMask_ST;
        uniform float _HueMaskintensity;
        uniform sampler2D _SpecMask; uniform float4 _SpecMask_ST;
        struct VertexInput {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 texcoord0 : TEXCOORD0;
            float2 texcoord1 : TEXCOORD1;
            float2 texcoord2 : TEXCOORD2;
        };
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;
            float2 uv2 : TEXCOORD2;
            float4 posWorld : TEXCOORD3;
            float3 normalDir : TEXCOORD4;
        };
        VertexOutput vert (VertexInput v) {
            VertexOutput o = (VertexOutput)0;
            o.uv0 = v.texcoord0;
            o.uv1 = v.texcoord1;
            o.uv2 = v.texcoord2;
            o.normalDir = UnityObjectToWorldNormal(v.normal);
            o.posWorld = mul(_Object2World, v.vertex);
            o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
            return o;
        }
        float4 frag(VertexOutput i) : SV_Target {
            i.normalDir = normalize(i.normalDir);
/// Vectors:
            float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
            float3 normalDirection = i.normalDir;
            
            float nSign = sign( dot( viewDirection, i.normalDir ) ); // Reverse normal if this is a backface
            i.normalDir *= nSign;
            normalDirection *= nSign;
            
            float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
            UnityMetaInput o;
            UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
            
            o.Emission = lerp( _Emissivecolor.rgb, float3(float2(255.0,255.0),255.0), _Bulb );
            
            float4 _Lightmap_var = tex2D(_Lightmap,TRANSFORM_TEX(i.uv1, _Lightmap));
            float node_165_ang = clamp(_UVRotation,0.0,6.28);
            float node_165_spd = 1.0;
            float node_165_cos = cos(node_165_spd*node_165_ang);
            float node_165_sin = sin(node_165_spd*node_165_ang);
            float2 node_165_piv = float2(0.5,0.5);
            float2 node_165 = (mul(((i.uv0+_UVtranslation)*_UVTile)-node_165_piv,float2x2( node_165_cos, -node_165_sin, node_165_sin, node_165_cos))+node_165_piv);
            float4 _HueMask_var = tex2D(_HueMask,TRANSFORM_TEX(node_165, _HueMask));
            float4 _Diffuse_var = tex2Dlod(_Diffuse,float4(TRANSFORM_TEX(node_165, _Diffuse),0.0,_Blur));
            float3 diffColor = ((_Lightmap_var.rgb+_Light_Map_Intensity)*(((_Lightnesscolor.rgb*_Ligntness)*lerp( _Solidcolor.rgb, lerp((((float3(float2(0.0,0.0),0.0)+_Huelevel)*((_HueMask_var.rgb*_HueMaskintensity)*_Huecolor.rgb))+_Diffuse_var.rgb),dot((((float3(float2(0.0,0.0),0.0)+_Huelevel)*((_HueMask_var.rgb*_HueMaskintensity)*_Huecolor.rgb))+_Diffuse_var.rgb),float3(0.3,0.59,0.11)),(1.0-_Saturation)), _Difusemap ))+lerp( float3(float2(0.0,0.0),0.0), (texCUBElod(_CubeMap,float4(viewReflectDirection,_ReflexionBlur)).rgb*_Reflexionintensity), _Reflexion )));
            float4 _SpecMask_var = tex2D(_SpecMask,TRANSFORM_TEX(node_165, _SpecMask));
            float3 specColor = (_SpecMask_var.rgb*(1.0-_Specularlevel));
            float specularMonochrome = max(max(specColor.r, specColor.g),specColor.b);
            diffColor *= (1.0-specularMonochrome);
            float roughness = 1.0 - _gloss;
            o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
            
            return UnityMetaFragment( o );
        }
        ENDCG
    }
}
FallBack "Diffuse"
CustomEditor "ShaderForgeMaterialInspector"
}
