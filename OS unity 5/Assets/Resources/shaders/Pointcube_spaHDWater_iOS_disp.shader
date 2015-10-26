Shader "Pointcube/spaHDWater_iOS_disp"
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

        _Gloss       ("Glossiness", Range(0, 1)) = 0.6                      // Spec
        _Shininess   ("Shininess", Range(0, 1))  = 0.75
        _SpecColor   ("Specular Color", Color)   = (1, 0.957, 0.694, 1)

        _Amount      ("Extrusion Amount", Float)   = 0.20                  // Displacement
		_MinCoef	 ("Min Coef", Float)		   = 4

        _DispVect00  ("DispVect00", Vector) = (0.0, 0.0, 0.0, 0.0)          // 1ere ligne, partie 1
        _DispVect10  ("DispVect10", Vector) = (0.0, 0.0, 0.0, 0.0)          // 1ere ligne, partie 2
        _DispVect01  ("DispVect01", Vector) = (0.0, 0.0, 0.0, 0.0)          // 2e ligne, partie 1
        _DispVect11  ("DispVect11", Vector) = (0.0, 0.0, 0.0, 0.0)          // ...
        _DispVect02  ("DispVect02", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect12  ("DispVect12", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect03  ("DispVect03", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect13  ("DispVect13", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect04  ("DispVect04", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect14  ("DispVect14", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect05  ("DispVect05", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect15  ("DispVect15", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect06  ("DispVect06", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect16  ("DispVect16", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect07  ("DispVect07", Vector) = (0.0, 0.0, 0.0, 0.0)
        _DispVect17  ("DispVect17", Vector) = (0.0, 0.0, 0.0, 0.0)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        CGPROGRAM
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
        float     _Amount;
        float     _MinCoef;
        
        float4    _DispVect00;
        float4    _DispVect10;
        float4    _DispVect01;
        float4    _DispVect11;
        float4    _DispVect02;
        float4    _DispVect12;
        float4    _DispVect03;
        float4    _DispVect13;
        float4    _DispVect04;
        float4    _DispVect14;
        float4    _DispVect05;
        float4    _DispVect15;
        float4    _DispVect06;
        float4    _DispVect16;
        float4    _DispVect07;
        float4    _DispVect17;

        //-------------------------------------------------
        float getDisp(float4 dispVect, int index)
        {
            if(index == 0)       return dispVect[0];
            else if(index == 1)  return dispVect[1];
            else if(index == 2)  return dispVect[2];
            else if(index >= 3)  return dispVect[3];
        }

        //-------------------------------------------------
        void vert (inout appdata_full v)
        {
            int dispVecX = v.texcoord.y < 0.5 ? 0 : 1;                    // Indice X du vecteur (0 ou 4)
            int dispVecY = (int)floor(v.texcoord.x*8);                    // Indice Y du vecteur (0 à 7)
            int vecIndex = (int)floor(v.texcoord.y*8) - (dispVecX*4);     // Indice du nombre à récupérer, dans le vecteur (0 à 4)

            float disp = 0;
            if(dispVecX == 0 && dispVecY == 0)          disp = getDisp(_DispVect00, vecIndex);
            else if(dispVecX == 1 && dispVecY == 0)     disp = getDisp(_DispVect01, vecIndex);
            else if(dispVecX == 0 && dispVecY == 1)     disp = getDisp(_DispVect10, vecIndex);
            else if(dispVecX == 1 && dispVecY == 1)     disp = getDisp(_DispVect11, vecIndex);
            else if(dispVecX == 0 && dispVecY == 2)     disp = getDisp(_DispVect02, vecIndex);
            else if(dispVecX == 1 && dispVecY == 2)     disp = getDisp(_DispVect12, vecIndex);
            else if(dispVecX == 0 && dispVecY == 3)     disp = getDisp(_DispVect03, vecIndex);
            else if(dispVecX == 1 && dispVecY == 3)     disp = getDisp(_DispVect13, vecIndex);
            else if(dispVecX == 0 && dispVecY == 4)     disp = getDisp(_DispVect04, vecIndex);
            else if(dispVecX == 1 && dispVecY == 4)     disp = getDisp(_DispVect14, vecIndex);
            else if(dispVecX == 0 && dispVecY == 5)     disp = getDisp(_DispVect05, vecIndex);
                 if(dispVecX == 1 && dispVecY == 5)     disp = getDisp(_DispVect15, vecIndex); // Note : pas de else ici pour éviter un parser overflow
            else if(dispVecX == 0 && dispVecY == 6)     disp = getDisp(_DispVect06, vecIndex);
            else if(dispVecX == 1 && dispVecY == 6)     disp = getDisp(_DispVect16, vecIndex);
            else if(dispVecX == 0 && dispVecY >= 7)     disp = getDisp(_DispVect07, vecIndex);
            else if(dispVecX == 1 && dispVecY >= 7)     disp = getDisp(_DispVect17, vecIndex);

//          fonction : (1-(cos(x*4*acos(0))+1)/2)
            float coef = (1f-(cos((( v.texcoord.y+v.texcoord.x)*4f)*4f*acos(0f))+1f)/_MinCoef); // Note : on ne prend pas la peine de faire en sorte que y soit
                                                                                          //      entre 0 et 1 puisque la fonction a une période de 1
            v.vertex.xyz += v.normal * disp * coef * _Amount;

        } // Vert


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
