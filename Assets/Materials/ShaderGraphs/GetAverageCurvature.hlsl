float GetNormalEdge(float2 uv, float3x3 viewMatrix)
{
    float2 pixel = 1.0 / _ScreenParams.xy;

    float3 center = normalize(mul(viewMatrix, SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv)));

    float3 left  = normalize(mul(viewMatrix, SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv + float2(-pixel.x, 0))));
    float3 right = normalize(mul(viewMatrix, SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv + float2( pixel.x, 0))));
    float3 up    = normalize(mul(viewMatrix, SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv + float2(0,  pixel.y))));
    float3 down  = normalize(mul(viewMatrix, SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv + float2(0, -pixel.y))));

    float edge =
        distance(center, left) +
        distance(center, right) +
        distance(center, up) +
        distance(center, down);

    return edge;
}

float SampleLinearDepth(float2 uv)
{
    return LinearEyeDepth(
        SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv),
        _ZBufferParams
    );
}

float GetDepthEdge(float2 uv, float depthThreshold, float depthSoftness)
{
    float2 pixel = 1.0 / _ScreenParams.xy;

    float center = SampleLinearDepth(uv);

    float left  = SampleLinearDepth(uv + float2(-pixel.x, 0));
    float right = SampleLinearDepth(uv + float2( pixel.x, 0));
    float up    = SampleLinearDepth(uv + float2(0,  pixel.y));
    float down  = SampleLinearDepth(uv + float2(0, -pixel.y));

    float maxDiff = 0.0;

    maxDiff = max(maxDiff, abs(center - left));
    maxDiff = max(maxDiff, abs(center - right));
    maxDiff = max(maxDiff, abs(center - up));
    maxDiff = max(maxDiff, abs(center - down));

    // Reject tiny depth changes.
    return smoothstep(
        depthThreshold,
        depthThreshold + depthSoftness,
        maxDiff
    );
}

void GetAverageCurvature_float(
    float2 screenPosition,
    float normalMultiplier,
    float depthMultiplier,
    float depthThreshold,
    float depthSoftness,
    float edgePower,
    out float edgeMask)
{
    float3x3 viewMatrix = (float3x3)UNITY_MATRIX_V;

    float normalEdge =
        GetNormalEdge(screenPosition, viewMatrix) *
        normalMultiplier;

    float depthEdge =
        GetDepthEdge(screenPosition, depthThreshold, depthSoftness) *
        depthMultiplier;

    float edge = normalEdge + depthEdge;

    edgeMask =
        pow(
            saturate(edge),
            edgePower
        );

    // Exponential squared fog attenuation.
    // Matches Unity-style Exp2 fog shape.
    float eyeDepth = SampleLinearDepth(screenPosition);

    // Tune this value to match your scene.
    const float fogDecay = 0.01;

    float fogFactor =
        exp(-pow(eyeDepth * fogDecay, 2.0));

    edgeMask *= fogFactor;
}