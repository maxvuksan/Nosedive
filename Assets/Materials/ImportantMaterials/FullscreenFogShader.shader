Shader "Hidden/CustomFog"
{
    Properties
    {
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100 ZWrite Off Cull Off

        Pass
        {
            Name "CustomFogPass"


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            
            #include "HelperCalculations.hlsl"

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 sceneColor = SampleSceneColor(input.uv);
                float depth = SampleSceneDepth(input.uv);

                float3 worldPos = ComputeWorldSpacePosition(input.uv, depth, UNITY_MATRIX_I_VP);
                float3 rayStart = _WorldSpaceCameraPos;
                float3 rayDir = worldPos - rayStart;
                float distance = length(rayDir);
                rayDir = normalize(rayDir);

                // LAYER 1: GROUND FOG
                float volumetricGroundFactor = CalculateGroundFog(rayStart, rayDir, distance);
                float3 layer1Color = lerp(sceneColor, _FogColour.rgb, volumetricGroundFactor);

                // LAYER 2: GLOBAL BLOB FOG 
                float3 fogOutput = CalculateBlobFog(layer1Color, worldPos, input.uv, distance);

                // LUMINANCE HOLE CUT-THROUGH
                float luminance = dot(sceneColor, float3(0.2126, 0.7152, 0.0722));
                float brightMask = smoothstep(_LumThresholdMin, _LumThresholdMax, luminance) * _CutThroughStrength;
                float3 finalColor = lerp(fogOutput.rgb, sceneColor, brightMask);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
