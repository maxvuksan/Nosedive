#ifndef HELPER_CALCULATIONS_INCLUDED
#define HELPER_CALCULATIONS_INCLUDED
            
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

CBUFFER_START(FogVariables)
    float4 _FogColour;                  // Row 1
    
    float  _FogDensity;                 // Row 2
    float  _NoiseScale;
    float  _NoiseIntensity;
    float  _RandomNoiseFeather;
    
    float3 _WindDirection;              // Row 3
    float  _WindSpeed;
    
    float  _GroundFogStartHeight;       // Row 4
    float  _GroundFogDepth;
    float  _LumThresholdMin;
    float  _LumThresholdMax;
    
    float  _CutThroughStrength;         // Row 5
    float3 _padRow5;                    // Fills Row 5
    
    float4 _HighlightRingColour;        // Row 6 
    
    float3 _HighlightRingOriginPosition;// Row 7
    float  _HighlightRingRadius;
    
    float  _HighlightRingBandSize;      // Row 8
    float  _HighlightRingFeather;
    float  _HiglightRingFalloffStart;
    float  _HighlightRingFalloffEnd;

    float _CameraPointLightRadius;
    float _CameraPointLightStrength;

CBUFFER_END


// Shared Hash Function
float hash3(float3 p)
{
    p = frac(p * 0.3183099 + float3(0.1, 0.1, 0.1));
    p *= 17.0;
    return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
}

// Shared 3D Noise Layer
float noise3D(float3 x)
{
    float3 p = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);

    #define L3(a,b,c) hash3(p + float3(a,b,c))
    
    return lerp(
        lerp(lerp(L3(0,0,0), L3(1,0,0), f.x), lerp(L3(0,1,0), L3(1,1,0), f.x), f.y),
        lerp(lerp(L3(0,0,1), L3(1,0,1), f.x), lerp(L3(0,1,1), L3(1,1,1), f.x), f.y), f.z
    );
}

// High-frequency screen space grain hash
float ScreenGrain(float2 uv, float time)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233) + time)) * 43758.5453);
}


// Calculate Volumetric Ground Fog Factor
float CalculateGroundFog(float3 rayStart, float3 rayDir, float maxDistance)
{
    float fogStart = _GroundFogStartHeight;
    float fogEnd   = _GroundFogStartHeight - _GroundFogDepth;
    float depthRange = max(_GroundFogDepth, 0.001);

    float3 targetWorldPos = rayStart + rayDir * maxDistance;

    // Hard boundary fallbacks
    if (targetWorldPos.y >= fogStart && rayStart.y >= fogStart)
    {
        return 0.0;
    }
    if (targetWorldPos.y <= fogEnd && rayStart.y <= fogEnd)
    {
        return 1.0;
    }

    float validYStart = clamp(rayStart.y,     fogEnd, fogStart);
    float validYEnd   = clamp(targetWorldPos.y, fogEnd, fogStart);

    float thicknessStart = (fogStart - validYStart) / depthRange;
    float thicknessEnd   = (fogStart - validYEnd)   / depthRange;

    float linearFogFactor = (thicknessStart + thicknessEnd) * 0.5;

    // --- THE FIXED OPAQUE BLOCK MATH ---
    // Instead of scaling by simple vertical delta fractions, calculate the literal 3D metric 
    // distance the ray spent passing through the vertical bounds of the slab container.
    float verticalDist = abs(validYStart - validYEnd);
    float3 slopeScale = rayDir / max(abs(rayDir.y), 0.0001);
    float actualRayLengthThroughSlab = verticalDist * length(slopeScale);

    // Apply the exponential accumulation against your density slider.
    // If your density slider is set high, this will turn 100% pitch opaque instantly.
    float fogIntensity = 1.0 - exp(-actualRayLengthThroughSlab * linearFogFactor);

    // Hard fallback: If the ray hits geometry below the floor, force full opacity block
    if (targetWorldPos.y <= fogEnd)
    {
        fogIntensity = 1.0;
    }

    return saturate(fogIntensity);
}

float CalculateHighlightRingMask(float3 worldPos, float distanceToCamera)
{
    // Horizontal Shape Math
    float distanceToRingOrigin = distance(_HighlightRingOriginPosition, worldPos);
    float distanceFromCenterOfBand = abs(distanceToRingOrigin - _HighlightRingRadius);

    float halfBand = _HighlightRingBandSize * 0.5;
    float outerEdge = halfBand + max(_HighlightRingFeather, 0.001);
    
    // Core ring base mask shape
    float baseRingMask = smoothstep(outerEdge, halfBand, distanceFromCenterOfBand);

    // Using smoothstep provides a smooth gradient curve (0.0 at start, 1.0 at end)
    float falloffT = smoothstep(_HiglightRingFalloffStart, _HighlightRingFalloffEnd, distanceToCamera);

    // Blend the Falloff Factor
    // Inverts the factor so the ring is 1.0 (fully visible) up close and fades to 0.0 (invisible) far away
    float visibilityFactor = 1.0 - falloffT; 

    return baseRingMask * visibilityFactor;
}

/*
    Fake point light originating from player position
*/
float3 CalculateCameraSourcedLight(float sceneColour, float distanceToCamera){

    // Soften radius boundaries using our safe parameters
    //float maxRadius = max(_CameraPointLightRadius, 0.001f);
    
    // Normalize camera distance into a clean 0 to 1 value (1.0 at camera, 0.0 at max radius)
    float distance01 = saturate(1.0f - (distanceToCamera / _CameraPointLightRadius));
    
    // Generate an aggressive exponential fog light mask around the camera player core
    float lightMask = pow(distance01, 3.0f) * saturate(_CameraPointLightStrength);
    
    // Blend the scene colour towards the local fog base colour using the light sphere factor
    return lerp(sceneColour, _FogColour.rgb, lightMask);
    

}

/*
    Calculates the fog colour and intensity (alpha channel of colour)
*/
float3 CalculateBlobFog(float3 sceneColour, float3 worldPos, float2 screenUV, float distanceToCamera, float screenSpaceNoiseStrength = 1.0)
{
    
    float3 litSceneColour = CalculateCameraSourcedLight(sceneColour, distanceToCamera);
    
    
    float3 windDirNormalized = normalize(_WindDirection);
    float3 noiseSamplePos = (worldPos + (windDirNormalized * _WindSpeed * _Time.y)) * _NoiseScale;

    float noiseVal = noise3D(noiseSamplePos);

    float edgeMask = saturate(1.0 - abs(noiseVal - 0.5) * 2.0);

    float grain = ScreenGrain(screenUV, frac(_Time.y));
    float grainErosion = (grain - 0.5) * edgeMask * _RandomNoiseFeather * screenSpaceNoiseStrength;

    noiseVal = saturate(noiseVal + grainErosion);
    noiseVal = smoothstep(0.2, 0.8, noiseVal);

    float modulatedDensity = _FogDensity * lerp(1.0 - _NoiseIntensity, 1.0 + _NoiseIntensity, noiseVal);
    float fogFactor = modulatedDensity * distanceToCamera;

    float fogLerpT = saturate(exp2(-(fogFactor * fogFactor) * 1.442695));
    float3 finalColour = lerp(_FogColour.rgb, litSceneColour, fogLerpT);

    // Calculate our smooth ring structure
    float ringMask = CalculateHighlightRingMask(worldPos, distanceToCamera);

    float3 ringColour = lerp(finalColour.rgb, _HighlightRingColour.rgb, _HighlightRingColour.a);
    finalColour = lerp(finalColour.rgb, ringColour.rgb, ringMask);

    return finalColour;
}



/*
    Fog calculation to apply for 
*/
float3 CalculateObjectSpaceFoggedColour(float3 sceneColour, float3 worldPos, float3 cameraPosition){

    float distanceToCamera = distance(worldPos, cameraPosition);

    float3 finalColour = CalculateBlobFog(sceneColour, worldPos, float2(0,0), distanceToCamera, 0.0);

    return finalColour;
}


#endif // HELPER_CALCULATIONS_INCLUDED
