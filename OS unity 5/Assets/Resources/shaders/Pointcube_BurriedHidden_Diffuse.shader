Shader "Pointcube/BurriedHidden_diffuse"
{
	Properties {
		_MainCol ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue" = "Transparent-1"}
//		Cull Off
		CGPROGRAM
    	#pragma surface surf Lambert

		fixed4    _MainCol;
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
          	float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
		   	clip (IN.worldPos.y);
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb*_MainCol.rgb;
			o.Alpha = c.a*_MainCol.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
