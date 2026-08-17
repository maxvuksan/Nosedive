Shader "Custom/FogAwareBillboardTransparent"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Texture (RGBA)", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags 
        { 
            // In a Deferred path, these tags tell URP to skip the G-Buffer 
            // and render this object during the Transparent Forward overlay step.
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ForwardTransparent"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            #include "Assets/Shaders/HelperCalculations.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD1;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output;

                // 1. Get the world space pivot center of the quad
                float3 objectWorldPos = unity_ObjectToWorld._m03_m13_m23; 

                // 2. Calculate the flat look direction to the camera (locking vertical tilt)
                float3 cameraPosWS = GetCameraPositionWS();
                float3 lookDir = cameraPosWS - objectWorldPos;
                lookDir.y = 0.0f; // Force the look system to stay strictly horizontal
                lookDir = normalize(lookDir);

                // 3. Establish clean upright billboard coordinate vectors
                float3 upWS = float3(0.0f, 1.0f, 0.0f);          // Strict global vertical axis
                float3 rightWS = normalize(cross(upWS, lookDir)); // Horizontal perpendicular vector

                // 4. Extract Inspector scaling values safely from the transformation matrix
                float scaleX = length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20));
                // Note: Since the quad is rotated 90 on X, its local Z axis now drives its height scale!
                float scaleZ = length(float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22));

                // 5. CRITICAL SWAP: Map local X to Right, and local Z to Up!
                // We completely ignore input.positionOS.y because a flat quad has 0 local height.
                float3 rotatedPosWS = objectWorldPos 
                                    + (rightWS * (input.positionOS.x * scaleX)) 
                                    + (upWS    * (input.positionOS.z * scaleZ)); // Z acts as your new upright height!

                // 6. Complete standard projection matrix bindings
                output.positionWS = rotatedPosWS;
                output.positionCS = TransformWorldToHClip(rotatedPosWS);
                output.uv = input.uv;
                

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                input.uv.y = 1.0 - input.uv.y;

                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float3 sceneColour = texColor.rgb;

                float3 worldPos = input.positionWS;
                float3 cameraPosition = GetCameraPositionWS();

                float3 finalColour = CalculateObjectSpaceFoggedColour(sceneColour, worldPos, cameraPosition);

                // Return final color while respecting the source alpha of your map
                return float4(finalColour, texColor.a);
            }
            ENDHLSL
        }
    }
}
