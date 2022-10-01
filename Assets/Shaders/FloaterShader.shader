Shader "thquinn/FloaterShader"
{
    Properties
    {
        _SpecularVector ("Specular Vector", Vector) = (1, 0, 0, 0)
        [PerRendererData] s_Alpha ("Alpha", Float) = 0.5
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            float3 _SpecularVector;
            float _Alpha;

            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                o.normal = UnityObjectToWorldNormal(normal);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float a = 1;
                a *= (dot(i.normal, _SpecularVector) + 1) * 1;
                a = lerp(a, .66, .5);
                a *= _Alpha;
                fixed4 col = fixed4(1, 1, 1, a);
                return col;
            }
            ENDCG
        }
    }
}
