Shader "Pointcube/EraserV2_copyCam"
{
    Properties
    {
         _MainColor ("Main Color", Color)           = (1,1,1,1)
         _MainTex   ("Base (RGB)", 2D)              = "white" {}
         _BgBounds  ("minX maxX minY maxY", Vector) = (1.0, 1.0, 1.0, 1.0)
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
    
                sampler2D _MainTex;
                fixed4    _MainColor;
                half4     _BgBounds;
    
                fixed4 frag (v2f_img i) : COLOR
                {
//                    float2 dxUv = i.uv;
//                    dxUv.g = 1 - dxUv.g;
                    float2 bgUv = i.uv;
                    bgUv.r = bgUv.r*(_BgBounds.g-_BgBounds.r)+_BgBounds.r;  // Supprimer les bandes
                    bgUv.g = bgUv.g*(_BgBounds.a-_BgBounds.b)+_BgBounds.b;  //   blanches
                    fixed4 output = tex2D(_MainTex, bgUv);

                    return output;
                }
            ENDCG
        }
    }
    FallBack off
}
