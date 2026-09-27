using UnityEngine;

/// <summary>
/// Applies a physical material type (defined in MaterialManager) to a specific object
/// </summary>
public class ApplyMaterial : MonoBehaviour
{
    /// <summary>
    /// The texture this surface should sound like (e.g. wood, metal, concrete)
    /// </summary>
    public MaterialTypes Material;

    /// <summary>
    /// If true the player will die instantly touching when landing on this 
    /// </summary>
    public bool KillOnImpact = false;
}
