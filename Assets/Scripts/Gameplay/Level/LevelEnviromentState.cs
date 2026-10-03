using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Settings configuring enviromental state for a given level
/// </summary>
[System.Serializable]
public struct LevelEnviromentSettings
{
    /// <summary>
    /// When the player goes below this height, they die
    /// </summary>
    public float DeathZoneHeight;

    /// <summary>
    /// Scales the number of rain particles, and the volume of the rain sound loop
    /// </summary>
    [Range(0, 1)]
    public float RainStrength;

    /// <summary>
    /// Scales the volume of the wind sound loop
    /// </summary>
    [Range(0, 1)]
    public float WindStrength;

    /// <summary>
    /// Controls the opacity of the cavity lighting effect
    /// </summary>
    [Range(0, 1)]
    public float CavityLightingOpacity;
    
    /// <summary>
    /// Directly controls the assigned FogProfile
    ///
    /// UseInterpolatedFogColour determines if a single fog colour or interpolated fog colour should be used
    /// If true, the fog colour will interpolate based on the distance (FogColour -> FogEndColour)
    /// </summary>
    [Header("Fog Profile Settings")]
    public bool UseInterpolatedFogColour;
    
    public Color FogColour;
    
    /// <summary>
    /// The end of the fog colour, this is only applied if fog interpolation is enabled
    /// </summary>
    [ShowIf("UseInterpolatedFogColour")] [AllowNesting]
    public Color FogEndColour;
    
    /// <summary>
    /// At what distance does the fog colour begin interpolating from start->end
    /// </summary>
    [ShowIf("UseInterpolatedFogColour")] [AllowNesting]
    public float FogInterpolationStart;
    
    /// <summary>
    /// From the start interpolation distance, how much further is the end distance
    /// </summary>
    [ShowIf("UseInterpolatedFogColour")] [AllowNesting]
    public float FogInterpolationDepth;
    
    /// <summary>
    /// The exponential density of the fog 
    /// </summary>
    [Range(0, 0.05f)]
    public float FogDensity;
    
    /// <summary>
    /// The intensity which the fog density fluctuates (this is driven by 3D noise)
    /// </summary>
    [Range(0, 1)]
    public float FogBlobNoiseIntensity;
    
    [Range(0,0.5f)]
    public float DirectionalLightSourceIntensity; 

    [Tooltip("The radius of the light source emitted from the cameras position ")]
    [Range(0,500)]
    public float CameraLightSourceRadius;

    [Tooltip("The intensity of the light emitted from the cameras position")]
    [Range(0,1)]
    public float CameraLightSourceIntensity;


}