Shader "Custom/alphaColor" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_DecalTex ("Decal", 2D) = "white" {}
	_Cutoff ("Base Alpha cutoff", Range (0,.9)) = .5	
	_Scale ("Scale", Vector) = (1,1,1,1)
	_SquashAmount ("Squash", Float) = 1	
}
SubShader {
	Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
	Lighting off	
	// Render both front and back facing polygons.
	Cull Off


	// first pass:
	//   render any pixels that are more than [_Cutoff] opaque
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Cutoff;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			float4 _Color;
			half4 frag (v2f i) : COLOR
			{
				half4 col = _Color * tex2D(_MainTex, i.texcoord);
				clip(col.a - _Cutoff);
				return col;
			}
		ENDCG
	}

	// Second pass:
	//   render the semitransparent details.
	Pass {
		Tags { "RequireOption" = "SoftVegetation" }
		
		// Dont write to the depth buffer
		ZWrite off
		
		// Set up alpha blending
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Cutoff;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			float4 _Color;
			half4 frag (v2f i) : COLOR
			{
				half4 col = _Color * tex2D(_MainTex, i.texcoord);
				clip(-(col.a - _Cutoff));
				return col;
			}
		ENDCG
	}
	Alphatest Greater 0
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGB
	Pass {
		Tags { "LightMode" = "Always" }
		Material {
		//	Diffuse [_Color]
			Ambient [_Color]
		//	Shininess [_Shininess]
		//	Specular [_SpecColor]
			Emission [_Emission]		
		}
		Lighting On
		SeparateSpecular On
		SetTexture [_MainTex] {
			Combine texture * primary DOUBLE, texture * primary
		} 
	}	
}

SubShader {
	Tags { "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
	Lighting off
	
	// Render both front and back facing polygons.
	Cull Off
	
	// first pass:
	//   render any pixels that are more than [_Cutoff] opaque
	Pass {  
		AlphaTest Greater [_Cutoff]
		SetTexture [_MainTex] {
			constantColor [_Color]
			combine texture * constant, texture * constant 
		}
	}

	// Second pass:
	//   render the semitransparent details.
	Pass {
		Tags { "RequireOption" = "SoftVegetation" }
		
		// Dont write to the depth buffer
		ZWrite off
		
		// Only render pixels less or equal to the value
		AlphaTest LEqual [_Cutoff]
		
		// Set up alpha blending
		Blend SrcAlpha OneMinusSrcAlpha
		
		SetTexture [_MainTex] {
			constantColor [_Color]
			Combine texture * constant, texture * constant 
		}
	}
	Alphatest Greater 0
	ZWrite Off
	
//One	The value of one - use this to let either the source or the destination color come through fully.
//Zero	The value zero - use this to remove either the source or the destination values.
//SrcColor	The value of this stage is multiplied by the source color value.
//SrcAlpha	The value of this stage is multiplied by the source alpha value.
//DstColor	The value of this stage is multiplied by frame buffer source color value.
//DstAlpha	The value of this stage is multiplied by frame buffer source alpha value.
//OneMinusSrcColor	The value of this stage is multiplied by (1 - source color).
//OneMinusSrcAlpha	The value of this stage is multiplied by (1 - source alpha).
//OneMinusDstColor	The value of this stage is multiplied by (1 - destination color).
//OneMinusDstAlpha	The value of this stage is multiplied by (1 - destination alpha).
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGB
	Pass {
		Tags { "LightMode" = "Always" }
		Material {
			//Diffuse [_Color]
			Ambient [_Color]
			Shininess [_Shininess]
			Specular [_SpecColor]
			Emission [_Emission]	
		}
		Lighting On
		SeparateSpecular On
		SetTexture [_MainTex] {
			Combine texture * primary DOUBLE, texture * primary
		} 
	}
	
}

}