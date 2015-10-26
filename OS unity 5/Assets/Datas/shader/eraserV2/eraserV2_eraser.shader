Shader "Pointcube/EraserV2_eraser"
{
    Properties
    {
        _MainColor("Main Color", Color)  =  (1.0,1.0,1.0,1.0)
        _MainTex  ("Base (RGB)", 2D) = "white" {}
        _BgTex    ("Background (RGB)", 2D) = "white" {}
        _AlphaMap ("Alpha (A)", 2D) = "white" {}
        _BgBounds ("minX maxX minY maxY", Vector) = (1.0, 1.0, 1.0, 1.0)
        _OpenGL   ("API: OpenGL (1), D3D (0)", Float) = 0
    }

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off Fog { Mode Off }

            CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #include "UnityCG.cginc"

                half4 _MainColor;
                uniform sampler2D _MainTex;
                uniform sampler2D _BgTex;
                uniform sampler2D _AlphaMap;
                half4 _BgBounds;
                half  _OpenGL;

                fixed4 frag (v2f_img i) : COLOR
                {
                    fixed4 original = tex2D(_MainTex, i.uv);

                    float2 bgUv = i.uv;
                    if(_OpenGL == 0)
                        bgUv.g = 1 - bgUv.g;

                    fixed4 alpha    = tex2D(_AlphaMap, bgUv);

                    bgUv.r = bgUv.r*(_BgBounds.g-_BgBounds.r)+_BgBounds.r;  // Supprimer les bandes
                    bgUv.g = bgUv.g*(_BgBounds.a-_BgBounds.b)+_BgBounds.b;  //   blanches

                    fixed4 bg       = tex2D(_BgTex,   bgUv);

                    fixed4 output = lerp(original, bg, 1-alpha.a);
                    output.a = 1;

                    return output;
                }
            ENDCG

        }
    }
    Fallback off
}
