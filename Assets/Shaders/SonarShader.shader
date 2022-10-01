Shader "thquinn/SonarShader"
{
    Properties
    {
        _PingLocation ("Ping Location", Vector) = (0, 0, 0, 0)
        _PingDistance ("Ping Distance", Float) = 0
        _PingAge ("Ping Age", Float) = 0
        _MaxPingAge ("Max Ping Age", Float) = 10
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off 
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float3 _PingLocation;
            float _PingDistance, _PingAge, _MaxPingAge;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            static const float _FalloffMult = .45;
            static const float _FalloffPow = 1.5;
            static const float _BackfaceMult = .33;
            static const float EXPIRATION_FADE_SECONDS = 1;
            static const float EXPIRATION_AGE = _MaxPingAge - EXPIRATION_FADE_SECONDS;
            static const float FADE_START_DISTANCE = 50;

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float distance = length(i.worldPos - _PingLocation);
                float delta = distance - _PingDistance;
                float f;
                if (delta < 0) {
                    f = _FalloffMult / pow(-delta, _FalloffPow);
                } else {
                    f = .01 / delta;
                }
                f = clamp(f, 0, 1);
                f *= facing < 0 ? _BackfaceMult : 1;
                float distanceToCamera = length(i.worldPos - _WorldSpaceCameraPos);
                if (distanceToCamera > FADE_START_DISTANCE) {
                    f *= 1 - (distanceToCamera - FADE_START_DISTANCE) * .1;
                    f = max(0, f);
                }
                if (_PingAge > EXPIRATION_AGE) {
                    f *= 1 - (_PingAge - EXPIRATION_AGE) / EXPIRATION_FADE_SECONDS;
                }
                fixed4 col = fixed4(1, 1, 1, f);
                return col;
            }
            ENDCG
        }
    }
}
