Shader "Pointcube/NormRimSpec"
{
    Properties
    {
        _MainColor   ("Main Color", Color)  =  (1.0,1.0,1.0,1.0)
        _MainTex     ("Main Texture", 2D)   = "white" {}
        _Blend       ("Texture/Color Blend", Range(0, 1)) = 0.41
        _GlobAlpha   ("Global Alpha", Range(0, 1))        = 0.37
        _MinAlpha    ("MinAlpha", Range(0, 1))            = 0.28

        _BumpMap     ("Normalmap", 2D) = "bump" {}                          // Bump + Displacement

        _RimColor    ("Rim Color", Color)          = (0.31,0.31,0.31,1.0)   // Rim Lighting
        _RimPower    ("Rim Power", Range(0.5,8.0)) = 8.0

        _SelfIllu    ("SelfIllu", Color)         = (0.0, 0.0, 0.0, 0.0)     // Self-illu

        _Gloss       ("Glossiness", Range(0, 1)) = 0.6                       // Spec
        _Shininess   ("Shininess", Range(0, 1))  = 0.75
        _SpecColor   ("Specular Color", Color)   = (1, 0.957, 0.694, 1)

        _DispVect1   ("DispVect1", Vector) = (0.2, 0.3, 0.7, 0.2)
        _Amount      ("Extrusion Amount", Float)   = 0.075                    // Displacement

    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }


        CGPROGRAM
        // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
        //#pragma exclude_renderers gles
        #pragma surface surf BlinnPhong vertex:vert alpha
        #pragma target 3.0
        #pragma glsl

        //-------------------------------------------------
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 viewDir;
        };

        //-------------------------------------------------
        sampler2D _MainTex;
        float4    _MainColor;
//        float4    _TintColor;

        float     _Blend;
        float     _GlobAlpha;
        float     _MinAlpha;

        sampler2D _BumpMap;

        half      _Shininess;       // Spec
        float     _Gloss;

        float4    _RimColor;        // Rim Lighting
        float     _RimPower;

        float4    _SelfIllu;

        // Disp vect try
        float4    _DispVect1;
        float     _Amount;

        //-------------------------------------------------
        void vert (inout appdata_full v)
        {
//            float disp = _DispVect1[0];//(int)floor(v.texcoord.x*4)];
//            float disp = v.texcoord.x*100;
//            v.vertex.xyz += v.normal * v.texcoord.x * _Amount;
        }

        //-------------------------------------------------
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo
            fixed4 texCol = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 mixed = lerp(texCol, _MainColor, _Blend);
            o.Albedo = mixed.rgb; // * _TintColor.rgb; //(c.rgb + (1-_MainColor.a)) * _MainColor.rgb;

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
