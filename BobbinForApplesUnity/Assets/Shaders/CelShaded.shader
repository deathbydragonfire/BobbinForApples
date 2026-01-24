Shader "Custom/CelShaded_HadesStyle"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        
        [Header(Texture Detail)]
        _DetailReduction ("Detail Reduction", Range(0, 1)) = 0
        _DetailSmoothing ("Detail Smoothing", Range(1, 32)) = 16
        
        [Header(Cel Shading)]
        _ShadowSteps ("Shadow Steps", Range(1, 5)) = 2
        _ShadowSmoothness ("Shadow Smoothness", Range(0, 0.5)) = 0.05
        _ShadowColor ("Shadow Tint", Color) = (0.7, 0.65, 0.8, 1)
        _AmbientLight ("Ambient Light", Range(0, 1)) = 0.3
        
        [Header(Specular)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 0.8
        
        [Header(Rim Light)]
        _RimColor ("Rim Color", Color) = (1, 0.9, 0.7, 1)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 1)) = 0.4
        
        [Header(Outline)]
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
        _OutlineColor ("Outline Color", Color) = (0.05, 0.02, 0.1, 1)
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _DetailReduction;
                float _DetailSmoothing;
                float _ShadowSteps;
                float _ShadowSmoothness;
                float4 _ShadowColor;
                float _AmbientLight;
                float _Smoothness;
                float4 _SpecularColor;
                float _SpecularIntensity;
                float4 _RimColor;
                float _RimPower;
                float _RimIntensity;
            CBUFFER_END
            
            float3 ReduceTextureDetail(float3 color, float reduction, float steps)
            {
                if (reduction < 0.01) return color;
                
                float3 posterized = floor(color * steps + 0.5) / steps;
                return lerp(color, posterized, reduction);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                albedo.rgb = ReduceTextureDetail(albedo.rgb, _DetailReduction, _DetailSmoothing);
                
                albedo *= _BaseColor;
                
                float3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));
                
                float NdotL = dot(normalWS, lightDir);
                float lightIntensity = NdotL * 0.5 + 0.5;
                
                float stepped = lightIntensity * _ShadowSteps;
                stepped = floor(stepped) + smoothstep(0.5 - _ShadowSmoothness, 0.5 + _ShadowSmoothness, frac(stepped));
                stepped = stepped / _ShadowSteps;
                stepped = saturate(stepped);
                
                float3 shadowedColor = lerp(_ShadowColor.rgb, float3(1, 1, 1), stepped);
                
                float3 halfVector = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normalWS, halfVector));
                float specularPower = exp2(10 * _Smoothness + 1);
                float specular = pow(NdotH, specularPower) * _SpecularIntensity;
                specular *= step(0.5, NdotL);
                
                float rimDot = 1.0 - saturate(dot(viewDir, normalWS));
                float rimIntensity = pow(rimDot, _RimPower);
                rimIntensity = smoothstep(0.5, 1.0, rimIntensity) * _RimIntensity;
                float3 rim = rimIntensity * _RimColor.rgb;
                
                float3 lighting = shadowedColor * mainLight.color + _AmbientLight;
                
                float3 finalColor = albedo.rgb * lighting;
                finalColor += specular * _SpecularColor.rgb;
                finalColor += rim;
                
                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float4 _OutlineColor;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth;
                output.positionCS = TransformObjectToHClip(positionOS);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                Light mainLight = GetMainLight();
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, mainLight.direction));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                
                output.positionCS = positionCS;
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
