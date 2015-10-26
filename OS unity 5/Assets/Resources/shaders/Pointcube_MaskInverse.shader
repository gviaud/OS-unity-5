Shader "Pointcube/MaskInverse" {
Properties {
    _MainTex ("Texture", 2D) = ""
}
 
Category {
    Tags {Queue = Background}
	Cull Front
    ColorMask 0
 
    SubShader {Pass {
        GLSLPROGRAM
        varying lowp vec2 uv;
 
        #ifdef VERTEX
        void main() {
            gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            uv = gl_MultiTexCoord0.xy;
        }
        #endif
 
        #ifdef FRAGMENT
        uniform lowp sampler2D _MainTex;
        void main() {
            if (texture2D(_MainTex, uv).a < .5) discard;
        }
        #endif      
        ENDGLSL
    }}
 
    SubShader {Pass {
        AlphaTest Greater 0.5
        SetTexture[_MainTex]
    }}
}
}
 
