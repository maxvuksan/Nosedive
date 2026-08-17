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
    /// </summary>
    [Header("Fog Profile Settings")]

    public Color FogColour;

    [Range(0, 0.05f)]
    public float FogDensity;
    
    [Tooltip("The intensity which the fog density fluctuates (this is driven by 3D noise)")]
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