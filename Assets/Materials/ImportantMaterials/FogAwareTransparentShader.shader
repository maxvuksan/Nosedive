Shader "Custom/FogAwareTransparentShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "HelperCalculations.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD1;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Object space -> World space
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionWS = positionWS;

                // World space -> Clip space
                OUT.positionCS = TransformWorldToHClip(positionWS);

                // Pass UVs through (apply tiling/offset here, not in frag)
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            float4 frag(Varyings input) : SV_Target
            {
                input.uv.y = 1.0 - input.uv.y;

                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                float alpha = _BaseColor.a * texColor.a;

                float3 worldPos = input.positionWS;
                float3 cameraPosition = GetCameraPositionWS();

                float3 finalColour = CalculateObjectSpaceFoggedColour(texColor.rgb, worldPos, cameraPosition);

                // Return final color while respecting the source alpha of your map
                return float4(finalColour.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
