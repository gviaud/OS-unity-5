 Shader "Custom/RotateUVs" {
        Properties {
			_Color ("Main Color", Color) = (1,1,1,1)
            _MainTex ("Base (RGB)", 2D) = "white" {}
            _Rotation ("Rotation Speed", Float) = 0.0
        }
        SubShader {
            Tags { "RenderType"="Opaque" }
            LOD 0
           
            CGPROGRAM
            #pragma surface surf Lambert vertex:vert
     
            sampler2D _MainTex;
			fixed4 _Color;
     
            struct Input {
                float2 uv_MainTex;
            };
     
            float _Rotation;
            void vert (inout appdata_full v) {
                v.texcoord.xy -=0.5;
                float s = sin ( _Rotation );
                float c = cos ( _Rotation );
                float2x2 rotationMatrix = float2x2( c, -s, s, c);
                rotationMatrix *=0.5;
                rotationMatrix +=0.5;
                rotationMatrix = rotationMatrix * 2-1;
                v.texcoord.xy = mul ( v.texcoord.xy, rotationMatrix );
                v.texcoord.xy += 0.5;
            }
     
            void surf (Input IN, inout SurfaceOutput o) {  
                half4 c = tex2D (_MainTex, IN.uv_MainTex);
                o.Albedo = c.rgb * _Color.rgb;
                o.Alpha = c.a;
            }
            ENDCG
        }
        FallBack "Diffuse"
    }