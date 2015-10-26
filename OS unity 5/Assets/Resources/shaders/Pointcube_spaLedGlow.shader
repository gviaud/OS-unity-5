Shader "Pointcube/spaLedGlow"
{
    Properties
    {
        _MainColor   ("Main Color", Color) =  (1.0,1.0,1.0,1.0)
        _MainTex     ("Main Texture (RGB)", 2D)   = "white" {}

        _RimColor    ("Rim Color", Color) = (0.12,0.53,1,0.0)    // Rim Lighting
        _RimPower    ("Rim Power", Float) = 0.35

        _RimAlphaPower    ("Rim Alpha Power", Float) = 0.15         // Rim alpha
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent-1" }

        CGPROGRAM
        #pragma surface surf Unlit alpha

      half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten)
      {
          half4 c;
          c.rgb = s.Albedo;
          c.a = s.Alpha;
          return c;
      }

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        sampler2D _MainTex;
        fixed4    _MainColor;

        float4    _RimColor;        // Rim Lighting
        float     _RimPower;

        float     _RimAlphaPower;

//        void vert (inout appdata_full v)
//        {
//            //#if !defined(SHADER_API_OPENGL)
//            float4 tex = tex2Dlod(_DispTex, float4(v.texcoord.xy,0,0));
//            v.vertex.xyz += v.normal * tex.a * _Amount;
//            //#endif
//        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));

            // Albedo
            fixed4 c  = tex2D(_MainTex, IN.uv_MainTex) * _MainColor;
            o.Albedo  = lerp(c.rgb, _RimColor.rgb, pow(rim, _RimPower));// * (_RimColor.rgb * (1-pow(rim, _RimPower)));

            // Rim lighting
            //o.Emission = (_RimColor.rgb * pow(rim, _RimPower));

            // Alpha
            o.Alpha   = _MainColor.a * (1-pow(rim, _RimAlphaPower));
        }
        ENDCG

    } // SubShader

    FallBack "Diffuse"

} // Shader
