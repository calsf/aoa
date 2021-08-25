Shader "Custom/WallDestroyedShader"
{
    Properties
    {
        _Alpha ("Alpha", Range(0, 1)) = 1
        _MainTex ("Main Texture", 2D) = ""
        _BlendTex1 ("Blend Texture 1", 2D) = ""
        _BlendTex2 ("Blend Texture 2", 2D) = ""
        _BlendTex3 ("Blend Texture 3", 2D) = ""
    }
    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
      
        CGPROGRAM
      
        #pragma surface surf Standard alpha
      
        struct Input 
        {
            float2 uv_MainTex;
        };

        float _Alpha;
        sampler2D _MainTex;
        sampler2D _BlendTex1;
        sampler2D _BlendTex2;
        sampler2D _BlendTex3;
 
        void surf (Input IN, inout SurfaceOutputStandard o) 
        {
            o.Albedo = lerp(tex2D(_MainTex, IN.uv_MainTex).rgb, tex2D(_BlendTex1, IN.uv_MainTex).rgb, 0);
            o.Albedo *= lerp(tex2D(_MainTex, IN.uv_MainTex).rgb, tex2D(_BlendTex2, IN.uv_MainTex).rgb, 0);
            o.Albedo *= lerp(tex2D(_MainTex, IN.uv_MainTex).rgb, tex2D(_BlendTex3, IN.uv_MainTex).rgb, 0);
            o.Alpha = _Alpha;
        }
        ENDCG
    }
    Fallback "Diffuse"
}