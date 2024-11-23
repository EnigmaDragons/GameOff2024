Shader "Hidden/Universal Render Pipeline/IntoxicationEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Float) = 1.0
        _SwayAmount ("Sway Amount", Float) = 1.0
        _SwaySpeed ("Sway Speed", Float) = 1.0
        _ChromaticAberration ("Chromatic Aberration", Float) = 0.2
        _Saturation ("Saturation", Float) = 1.2
        _TimeOffset ("Time Offset", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "IntoxicationEffect"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            
            #pragma vertex Vert
            #pragma fragment Fragment

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _BlurAmount;
            float _SwayAmount;
            float _SwaySpeed;
            float _ChromaticAberration;
            float _Saturation;
            float _TimeOffset;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float2 getSway(float2 uv, float time)
            {
                float2 sway = float2(
                    sin(time * _SwaySpeed + uv.y * 4.0),
                    cos(time * _SwaySpeed * 0.8 + uv.x * 4.0)
                ) * _SwayAmount;
                return sway;
            }

            float4 blur(float2 uv, float blur)
            {
                float4 color = float4(0, 0, 0, 0);
                float2 offset = float2(blur * 0.001, 0);
                
                // 9-tap gaussian blur
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - offset * 4) * 0.05;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - offset * 3) * 0.09;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - offset * 2) * 0.12;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - offset) * 0.15;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * 0.18;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset) * 0.15;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * 2) * 0.12;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * 3) * 0.09;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * 4) * 0.05;
                
                return color;
            }

            float4 Fragment(Varyings input) : SV_Target
            {
                float2 sway = getSway(input.uv, _TimeOffset);
                float2 uv = input.uv + sway;
                
                // Apply chromatic aberration
                float4 r = blur(uv + float2(_ChromaticAberration * 0.02, 0), _BlurAmount);
                float4 g = blur(uv, _BlurAmount);
                float4 b = blur(uv - float2(_ChromaticAberration * 0.02, 0), _BlurAmount);
                
                float4 col = float4(r.r, g.g, b.b, 1.0);
                
                // Adjust saturation
                float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(float3(luminance, luminance, luminance), col.rgb, _Saturation);
                
                return col;
            }
            ENDHLSL
        }
    }
}