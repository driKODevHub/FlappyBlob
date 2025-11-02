Shader "Unlit/Wobble2D_URP"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color ("Tint Color", Color) = (1,1,1,1)
        
        [Header(Wobble Settings)]
        _WobbleAmount ("Wobble Amount", Range(0, 0.2)) = 0.02
        _WobbleSpeed ("Wobble Speed", Range(0, 10)) = 5.0
        _WobbleInterval ("Wobble Interval (Steps/Sec)", Range(1, 30)) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Tags { "LightMode"="Universal2D" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 texcoord     : TEXCOORD0;
                half4  color        : COLOR;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 texcoord     : TEXCOORD0;
                half4  color        : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _WobbleAmount;
                float _WobbleSpeed;
                float _WobbleInterval;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float rand(float2 seed)
            {
                return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                // --- ОНОВЛЕНА ЛОГІКА ДЛЯ ДЕФОРМАЦІЇ ---
                float steppedTime = floor(_Time.y * _WobbleInterval) / _WobbleInterval;
                float timeFactor = steppedTime * _WobbleSpeed;

                // Тепер "зерно" для генерації випадкового числа включає позицію самої вершини (v.positionOS.xy).
                // Це гарантує, що кожна вершина отримає унікальне зміщення.
                float randX = rand(float2(timeFactor, v.positionOS.x * 0.1));
                float randY = rand(float2(timeFactor, v.positionOS.y * 0.1));

                float wobbleOffsetX = (randX * 2.0 - 1.0) * _WobbleAmount;
                float wobbleOffsetY = (randY * 2.0 - 1.0) * _WobbleAmount;

                v.positionOS.x += wobbleOffsetX;
                v.positionOS.y += wobbleOffsetY;
                // ---------------------------------------------

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                half4 finalColor = texColor * i.color;
                return finalColor;
            }
            ENDHLSL
        }
    }
}

