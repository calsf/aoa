Shader "Custom/Wall Shader" 
{
    Properties
    {
        _Blend1 ("Blend 1", Range(0, 10)) = 0
        _Blend2 ("Blend 2", Range(0, 10)) = 0
        _Blend3 ("Blend 3", Range(0, 10)) = 0
        _MainTex ("Main Texture", 2D) = ""
        _BlendTex1 ("Blend Texture 1", 2D) = ""
        _BlendTex2 ("Blend Texture 2", 2D) = ""
        _BlendTex3 ("Blend Texture 3", 2D) = ""
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
      
        CGPROGRAM
      
        #pragma surface surf Standard
      
        struct Input 
        {
            float2 uv_MainTex;
        };

        float _Blend1;
        float _Blend2;
        float _Blend3;
        sampler2D _MainTex;
        sampler2D _BlendTex1;
        sampler2D _BlendTex2;
        sampler2D _BlendTex3;
 
        void surf (Input IN, inout SurfaceOutputStandard o) 
        {
            o.Albedo = lerp(tex2D(_MainTex, IN.uv_MainTex).rgb, tex2D(_BlendTex1, IN.uv_MainTex).rgb, _Blend1);
            o.Albedo *= lerp(tex2D(_MainTex, IN.uv_MainTex).rgb, tex2D(_BlendTex2, IN.uv_MainTex).rgb, _Blend2);
            o.Albedo *= lerp(tex2D(_MainTex, IN.uv_MainTex).rgb, tex2D(_BlendTex3, IN.uv_MainTex).rgb, _Blend3);
        }
        ENDCG
    }
    Fallback "Diffuse"
}
