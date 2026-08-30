using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Screen Noise Profile")]
[System.Serializable]
public class ScreenNoiseProfile : IProfile<ScreenNoiseDataStruct>
{
    /// <summary>
    /// Determines the magnitude of the calculated NoiseRandomUvOffset  
    /// Note: This lives outside the data structure because it is not needed in the shader
    /// </summary>
    public float NoiseRandomOffsetScale;

    /// <summary>
    /// The data contained in the shaders constant buffer
    /// </summary>
    public ScreenNoiseDataStruct Data;
    public ScreenNoiseDataStruct GetData() => Data;
}


[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct ScreenNoiseDataStruct
{
    public float NoiseIntensity;

    /// <summary>
    /// The UV offset to apply when sampling the noise texture
    /// Note: This value is set at runtime
    /// </summary>
    [HideInInspector] public Vector2 _NoiseRandomUvOffset;

    /// <summary>
    /// The dimensions of the viewport
    /// </summary>
    public Vector2 _ViewportDimensions;
    private Vector2 _Padding0;
}