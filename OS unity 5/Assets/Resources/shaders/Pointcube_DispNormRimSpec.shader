Shader "Pointcube/DispNormRimSpec"
{
    Properties
    {
        _MainColor   ("Main Color", Color)  =  (1.0,1.0,1.0,1.0)
        _MainTex     ("Main Texture", 2D)   = "white" {}
        _Blend       ("Texture/Color Blend", Range(0, 1)) = 0.41 // 0.52  // Eau calme : 1.0  // ancienne eau agitée : 0.346
        _GlobAlpha   ("Global Alpha", Range(0, 1))        = 0.37 // 0.385 // Eau calme : 0.25 // ancienne eau agitée : 0.365
        _MinAlpha    ("MinAlpha", Range(0, 1))            = 0.28 // 0.26  // Eau calme : 0.19 // ancienne eau agitée : 0

        _BumpMap     ("Normalmap", 2D) = "bump" {}                          // Bump + Displacement
        _Amount      ("Extrusion Amount", Range(0, 1))   = 0.075            // Displacement

        _RimColor    ("Rim Color", Color)          = (0.31,0.31,0.31,1.0)   // Rim Lighting
        _RimPower    ("Rim Power", Range(0.5,8.0)) = 8.0

        _SelfIllu    ("SelfIllu", Color)         = (0.0, 0.0, 0.0, 0.0)     // Self-illu

        _Gloss       ("Glossiness", Range(0, 1)) = 0.645                    // Spec
        _Shininess   ("Shininess", Range(0, 1))  = 0.58
        _SpecColor   ("Specular Color", Color)   = (1, 0.957, 0.694, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" }

        CGPROGRAM
        #pragma surface surf BlinnPhong vertex:vert alpha
        #pragma target 3.0
        #pragma glsl

        //-------------------------------------------------
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_DispTex;
            float2 uv_BumpMap;
            float3 viewDir;
        };

        //-------------------------------------------------
        sampler2D _MainTex;
        float4    _MainColor;

        float     _Blend;
        float     _GlobAlpha;
        float     _MinAlpha;

        sampler2D _DispTex;
        float     _Amount;
        sampler2D _BumpMap;

        half      _Shininess;       // Spec
        float     _Gloss;

        float4    _RimColor;        // Rim Lighting
        float     _RimPower;

        float4    _SelfIllu;

        //-------------------------------------------------
        void vert (inout appdata_full v)
        {
            //#if !defined(SHADER_API_OPENGL)
            float4 tex = tex2Dlod(_BumpMap, float4(v.texcoord.xy,0,0));
            v.vertex.xyz += v.normal * tex.rgb * _Amount;
            //#endif
        }

        //-------------------------------------------------
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo
            fixed4 texCol = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 mixed = lerp(texCol, _MainColor, _Blend);
            o.Albedo = mixed.rgb;

            // Alpha
            o.Alpha = (texCol.a + _MinAlpha) * _GlobAlpha;

            // Bump
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

            // Spec
            o.Gloss    = _Gloss;
            o.Specular = _Shininess;

            // Rim lighting
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
            o.Emission = _RimColor.rgb * pow (rim, _RimPower) + _SelfIllu.rgb;
        }
        ENDCG

    } // SubShader

    FallBack "Diffuse"

} // Shader
