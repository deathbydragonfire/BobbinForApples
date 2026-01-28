Shader "Custom/SpriteMotionBlur"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurAmount ("Blur Amount", Range(0, 1)) = 0.5
        _BlurSamples ("Blur Samples", Range(1, 16)) = 8
        _VelocityScale ("Velocity Scale", Float) = 1.0
        
        [Header(Sprite Rendering)]
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaClip ("Alpha Clip", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _RendererColor;
                float _BlurAmount;
                int _BlurSamples;
                float _VelocityScale;
                float2 _Velocity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _RendererColor;
                return output;
            }

            half4 SampleSpriteTexture(float2 uv)
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                #if ETC1_EXTERNAL_ALPHA
                    half4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, uv);
                    color.a = alpha.r;
                #endif
                
                return color;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 velocity = _Velocity * _VelocityScale * _BlurAmount;
                float2 uvOffset = velocity / max(_BlurSamples, 1);
                
                half4 color = half4(0, 0, 0, 0);
                
                for(int i = 0; i < _BlurSamples; i++)
                {
                    float2 sampleUV = input.uv - uvOffset * i;
                    color += SampleSpriteTexture(sampleUV);
                }
                
                color /= max(_BlurSamples, 1);
                color *= input.color;
                
                return color;
            }
            ENDHLSL
        }
        
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _RendererColor;
                float _BlurAmount;
                int _BlurSamples;
                float _VelocityScale;
                float2 _Velocity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _RendererColor;
                return output;
            }

            half4 SampleSpriteTexture(float2 uv)
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                #if ETC1_EXTERNAL_ALPHA
                    half4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, uv);
                    color.a = alpha.r;
                #endif
                
                return color;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 velocity = _Velocity * _VelocityScale * _BlurAmount;
                float2 uvOffset = velocity / max(_BlurSamples, 1);
                
                half4 color = half4(0, 0, 0, 0);
                
                for(int i = 0; i < _BlurSamples; i++)
                {
                    float2 sampleUV = input.uv - uvOffset * i;
                    color += SampleSpriteTexture(sampleUV);
                }
                
                color /= max(_BlurSamples, 1);
                color *= input.color;
                
                return color;
            }
            ENDHLSL
        }
    }
    
    Fallback "Sprites/Default"
}
