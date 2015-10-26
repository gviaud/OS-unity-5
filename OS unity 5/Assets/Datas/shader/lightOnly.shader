Shader "Custom/LightOnly"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf LambertMod

        // Lighting model
        half4 LightingLambertMod (SurfaceOutput s, half3 lightDir, half atten)
        {
            half NdotL = dot (s.Normal, lightDir);
            half4 c;
            clip(_LightColor0.a - 1);
            c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2);
            c.a = s.Alpha; //s.Alpha; // This value does not seem to change anything
            return c;
        }
        
        float4    _Color;
        
        struct Input
        {
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
//          o.Alpha = _Color.rgb; // Makes the plane fully transparent when _Color is black
        }
        ENDCG
    }
}

