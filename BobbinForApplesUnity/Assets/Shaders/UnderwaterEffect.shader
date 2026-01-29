Shader "Hidden/UnderwaterEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShallowColor ("Shallow Water Color", Color) = (0.2, 0.6, 0.8, 1)
        _DeepColor ("Deep Water Color", Color) = (0.05, 0.3, 0.5, 1)
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.5
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveFrequency ("Wave Frequency", Float) = 20.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.003
        _CausticsSpeed ("Caustics Speed", Float) = 0.5
        _CausticsScale ("Caustics Scale", Float) = 5.0
        _CausticsIntensity ("Caustics Intensity", Range(0, 1)) = 0.3
        _EffectIntensity ("Effect Intensity", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "UnderwaterEffect"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _ShallowColor;
            float4 _DeepColor;
            float _TintStrength;
            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveAmplitude;
            float _CausticsSpeed;
            float _CausticsScale;
            float _CausticsIntensity;
            float _EffectIntensity;

            float GenerateNoise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            float CausticsPattern(float2 uv, float time)
            {
                float2 p = uv * _CausticsScale;
                float2 i = floor(p);
                float2 f = frac(p);
                
                float a = GenerateNoise(i + time * _CausticsSpeed);
                float b = GenerateNoise(i + float2(1.0, 0.0) + time * _CausticsSpeed);
                float c = GenerateNoise(i + float2(0.0, 1.0) + time * _CausticsSpeed);
                float d = GenerateNoise(i + float2(1.0, 1.0) + time * _CausticsSpeed);
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                float noise = lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
                
                float caustics = pow(noise, 3.0);
                caustics += pow(GenerateNoise(p * 0.5 + time * _CausticsSpeed * 0.7), 2.0) * 0.5;
                
                return saturate(caustics);
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float time = _Time.y;
                
                float wave = sin(uv.y * _WaveFrequency + time * _WaveSpeed) * _WaveAmplitude;
                wave += sin(uv.y * _WaveFrequency * 0.5 + time * _WaveSpeed * 1.3) * _WaveAmplitude * 0.5;
                float2 distortedUV = uv + float2(wave, 0);
                
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);
                
                float depthGradient = 1.0 - uv.y;
                float4 waterTint = lerp(_ShallowColor, _DeepColor, depthGradient);
                
                float4 tintedColor = lerp(baseColor, baseColor * waterTint, _TintStrength);
                
                float caustics = CausticsPattern(uv, time);
                float4 causticsColor = tintedColor + caustics * _CausticsIntensity * float4(0.8, 1.0, 1.0, 0);
                
                float4 finalColor = lerp(baseColor, causticsColor, _EffectIntensity);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
