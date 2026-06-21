void Billboard_float(float4 positionOS, out float4 positionHCS)
{
    // Z-axis reference vector
    float3 zForward = float3(0.0, 0.0, 1.0);

    // Forward = object's Z axis in world space
    // Preserves object rotation around Z
    float3 forward = mul((float3x3)unity_ObjectToWorld, zForward);

    // Object world position
    float3 worldPos = unity_ObjectToWorld._m03_m13_m23;

    // Direction from object to camera
    float3 toCamera = worldPos - _WorldSpaceCameraPos;

    // Right vector
    // Perpendicular to camera direction and forward
    // Preserves object X scale
    float3 right =
        normalize(cross(forward, toCamera))
        * length(unity_ObjectToWorld._m00_m10_m20);

    // Up vector
    // Perpendicular to right and forward
    // Preserves object Y scale
    float3 up =
        normalize(cross(right, forward))
        * length(unity_ObjectToWorld._m01_m11_m21);

    // Billboard transform matrix
    float4x4 mat =
    {
        1,0,0,0,
        0,1,0,0,
        0,0,1,0,
        0,0,0,1
    };

    mat._m00_m10_m20 = right;     // X basis
    mat._m01_m11_m21 = up;        // Y basis
    mat._m02_m12_m22 = forward;   // Z basis
    mat._m03_m13_m23 = worldPos;  // Translation

    // Local vertex position
    float4 vertex = float4(positionOS.xyz, 1.0);

    // Transform to billboarded world position
    vertex = mul(mat, vertex);

    positionHCS = vertex;
}