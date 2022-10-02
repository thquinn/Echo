Shader "thquinn/PlaygroundShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Noise ("Noise Texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        sampler2D _Noise;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float yzNoise = tex2D(_Noise, IN.worldPos.yz * .15) * abs(IN.worldNormal.x);
            float xyNoise = tex2D(_Noise, IN.worldPos.xy * .15) * abs(IN.worldNormal.z);
            float xzNoise = tex2D(_Noise, IN.worldPos.xz * .15) * abs(IN.worldNormal.y);
            float noise = xyNoise + xzNoise + yzNoise;
            o.Albedo = _Color.rgb + fixed3(1, 1, 1) * max(IN.worldPos.y, 0) * .05 - noise * .1;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
