Shader "Custom/URP_Terrain" {
    Properties {
        _BaseTextures("Base Texture Array", 2DArray) = "" {}
        _TestScale("Scale", Float) = 1
        _MinHeight("Min Height", Float) = 0
        _MaxHeight("Max Height", Float) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _MIXED_LIGHTING_SUBTRACTIVE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 lightDir : TEXCOORD2;
            };

            // Propriedades do shader
            TEXTURE2D_ARRAY(_BaseTextures);
            SAMPLER(sampler_BaseTextures);

            float _TestScale;
            float _MinHeight;
            float _MaxHeight;

            Varyings vert(Attributes input) {
                Varyings output;
                float4 positionWS = TransformObjectToHClip(input.positionOS);
                output.positionCS = positionWS;
                output.worldPos = TransformObjectToWorld(input.positionOS);
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                output.lightDir = GetMainLightDirection();
                return output;
            }

            float inverseLerp(float a, float b, float value) {
                return saturate((value - a) / (b - a));
            }

            float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
                float3 scaledWorldPos = worldPos / scale;

                float3 xProjection = SAMPLE_TEXTURE2D_ARRAY(_BaseTextures, sampler_BaseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
                float3 yProjection = SAMPLE_TEXTURE2D_ARRAY(_BaseTextures, sampler_BaseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
                float3 zProjection = SAMPLE_TEXTURE2D_ARRAY(_BaseTextures, sampler_BaseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;

                return xProjection + yProjection + zProjection;
            }

            float3 BlendTerrain(Varyings IN, out float3 albedo) {
                float heightPercent = inverseLerp(_MinHeight, _MaxHeight, IN.worldPos.y);
                float3 blendAxes = abs(IN.worldNormal);
                blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

                float3 finalColor = float3(0, 0, 0);

                // Exemplo: use a textura no índice 0 para testes
                finalColor = triplanar(IN.worldPos, _TestScale, blendAxes, 0);

                albedo = finalColor;
                return finalColor;
            }

            half4 frag(Varyings IN) : SV_Target {
                float3 albedo;
                float3 color = BlendTerrain(IN, albedo);

                // Aplicar iluminação
                float3 normalWS = normalize(IN.worldNormal);
                float3 lightColor = GetMainLightColor();
                float3 lightDir = normalize(IN.lightDir);

                float3 diffuse = Lambert(normalWS, lightDir);
                float3 finalColor = color * diffuse * lightColor;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}