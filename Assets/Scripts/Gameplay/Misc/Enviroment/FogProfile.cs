using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// An asset to capture the configuration of the custom fog
/// </summary>
[CreateAssetMenu(menuName = "Custom/Fog Profile")]
public class FogProfile : ScriptableObject
{
    public FogDataStruct Data;
}

/// <summary>
/// The configuration of the custom fog
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct FogDataStruct
{   
    [Header("Main Fog")]
    public Color Colour;
    [Range(0,1)]
    public float Density;

    /// <summary>
    /// To add depth to the fog, we drive the density using perlin noise
    /// </summary>
    [Range(0.0001f, 0.2f)]
    public float BlobNoiseScale;

    /// <summary>
    /// The influence the blobs have on the fog density
    /// </summary>
    [Range(0,1)]
    public float BlobNoiseIntensity;

    /// <summary>
    /// The intensity of random pixel noise blended with the fog
    /// </summary>
    [Range(0,1)]
    public float RandomNoise;

    [Header("Wind")]
    public Vector3 WindDirection;

    public float WindSpeed;

    [Header("Ground Fog")]
    /// <summary>
    /// Enables an extra fog volume on the y axis (for fog below the world)
    /// </summary>
    public float GroundFogStartHeight;
    /// <summary>
    /// From the GroundFogStartHeight, how far the ground fog extends downwards before reaching max density
    /// </summary>
    public float GroundFogDepth;


    [Header("Luminance Cut Through")]
    /// <summary>
    /// The minimum luminance value that can cut through the fog
    /// </summary>
    public float LuminanceThresholdMin;

    /// <summary>
    /// The maximum luminance value that can cut through the fog 
    /// </summary>
    public float LuminanceThresholdMax;

    /// <summary>
    /// How much of the fog is ignored 
    /// </summary>
    [Range(0,1)]
    public float LuminanceCutThroughStrength;

    [HideInInspector] public Vector3  _padding1;

    [Header("Highlight Ring")]

    public Color HighlightRingColour;

    /// <summary>
    /// The position the ring originates from, will expand from this spot
    /// </summary>
    public Vector3 HighlightRingOriginPosition;

    /// <summary>
    /// The radius of the ring, expanded from the ring origin
    /// </summary>
    public float HighlightRingRadius;

    /// <summary>
    /// The thickness of the ring, the ring effect will apply at (Radius - Bandsize) to (Radius + Bandsize)
    /// </summary>
    public float HighlightRingBandSize;

    /// <summary>
    /// How much the edges are smoothly lerped wirth the main fog
    /// </summary>
    [Range(0, 100)]
    public float HighlightRingFeather;

    /// <summary>
    /// The intensity falloff of the ring caused by the distance from the camera to the highlighted fragment
    /// </summary>
    public float HighlightRingDistanceFalloffStart;
    public float HighlightRingDistanceFalloffEnd;
    
}


