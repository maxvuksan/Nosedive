using UnityEngine;

/// <summary>
/// Settings configuring enviromental state for a given level
/// </summary>
[System.Serializable]
public struct LevelEnviromentSettings
{
    /// <summary>
    /// Scales the number of rain particles, and the volume of the rain sound loop
    /// </summary>
    [Range(0, 1)]
    public float RainStrength;
    /// <summary>
    /// Scales the strength of the wind, and the volume of the wind sound loop
    /// </summary>
    [Range(0, 1)]
    public float WindStrength;

    /// <summary>
    /// Controls the colour of the camera background
    /// </summary>
    public Color BackgroundColour;

    /// <summary>
    /// Controls the colour of the fog albedo
    /// </summary>
    public Color FogColour;

    /// <summary>
    /// When the player goes below this height, they die
    /// </summary>
    public float DeathZoneHeight;
}