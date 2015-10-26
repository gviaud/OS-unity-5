Shader "Custom/DevScreenShaderBackFace"{
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_SpcColorPercent ("Spc clr blend", Range (0.01, 1)) = 0.1
	_MainTex ("Base (RGB) TransGloss (A)", 2D) = "white" {}
	_AlphaTex ("Alpha", 2D) = "white" {}
	_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
    _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
}

SubShader
{
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 300
//	Cull off
	CGPROGRAM
	#pragma surface surf BlinnPhong alpha
	
	sampler2D _MainTex;
	sampler2D _AlphaTex;
	fixed4 _Color;
	//fixed4 _SpecColor;
	half _Shininess;
	half _SpcColorPercent;
	float4 _RimColor;
	float _RimPower;
	
	struct Input
	{
		float2 uv_MainTex;
		float2 uv_AlphaTex;
		float3 viewDir;
	};
	
	void surf (Input IN, inout SurfaceOutput o)
	{
		fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
		half4 alpha = tex2D(_AlphaTex, IN.uv_AlphaTex);
	
	//	_SpecColor.rgb = _Color.rgb * _SpcColorPercent;
		_SpecColor.rgb = _Color.rgb ;
	
	//	o.Albedo = tex.rgb * _Color.rgb;
		o.Albedo = tex.rgb * (_Color.rgb * _SpcColorPercent);
		
		o.Gloss = tex.a;
		//o.Alpha = alpha.r * alpha.g * alpha.b * _Color.a;
		o.Specular = _Shininess;
		half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
	   	//o.Emission = _RimColor.rgb * pow (rim, _RimPower);
	   	o.Emission = (_Color.rgb * _SpcColorPercent)* pow (rim, _RimPower);
	   	o.Alpha   = 0.75 + (alpha.r * (1-pow(rim, _RimPower)))/4;
	}
	ENDCG
}

Fallback "Transparent/VertexLit"
}
