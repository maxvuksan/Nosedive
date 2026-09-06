using NaughtyAttributes;
using UnityEngine;

public enum FootstepLayerTypes
{
    GlassShards,
    Grass,
}

public class AudioZoneFootstepLayer : AudioZone
{

    /// <summary>
    /// Should we apply a mask
    /// </summary>
    public bool UseMask;
    /// <summary>
    /// Will only apply when we are walking on ground of this mask type
    /// </summary>
    [ShowIf("UseMask")]
    public MaterialTypes Mask;
    
    public FootstepLayerTypes Type;
}
