Shader "Custom/CustomScreenNoise"
{
    Properties
    {
        // Exposed in material editor so you can easily reference your noise texture asset
        [NoScaleOffset] _GrainTexture("Noise Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100 ZWrite Off Cull Off

        Pass
        {
            Name "CustomScreenNoisePass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // Required for rendering full-screen blit textures smoothly
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Textures and Samplers must sit OUTSIDE the Constant Buffer
            Texture2D _GrainTexture;
            SamplerState sampler_GrainTexture;

            // Unity automatically populates this when naming convention matches [TextureName]_TexelSize
            // x = 1/width, y = 1/height, z = width, w = height
            float4 _GrainTexture_TexelSize; 

            // Data Struct Layout. Must match ScreenNoiseDataStruct in C#
            CBUFFER_START(NoiseVariables)
                float _NoiseIntensity;
                float2 _NoiseRandomUvOffset;
                float2 _ViewportDimensions;
                float2 _Padding0;
            CBUFFER_END

            // Remaps a blue noise value to a triangluar PDF [-0.5;1.5]
            float remap_tri(float v){
                float orig = v*2.0 - 1.0;
                v = max(-1.0, orig / sqrt ( abs(orig)));
                return v - sign(orig);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Generates optimal fullscreen hardware triangle coordinates
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texcoord  = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Sample the original scene screen colour
                float3 sceneColour = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord.xy).rgb;

                float2 screenPixels = input.texcoord.xy * _BlitTexture_TexelSize.zw;
                float2 staticNoiseUV = screenPixels / _GrainTexture_TexelSize.zw;
                
                float2 noiseUV = staticNoiseUV + _NoiseRandomUvOffset;

                float noiseSample = _GrainTexture.Sample(sampler_GrainTexture, noiseUV).r;
                
                float3 finalColour = sceneColour + remap_tri(noiseSample) * _NoiseIntensity;
                finalColour = clamp(finalColour, 0.0, 1.0);

                return float4(finalColour, 1.0);
            }
            ENDHLSL
        }
    }
}
