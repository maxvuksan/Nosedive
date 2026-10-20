using NaughtyAttributes;
using UnityEngine;

public enum FootstepLayerTypes
{
    PuddleSplash
}

public class AudioZoneFootstepLayer : AudioZone
{
    /// <summary>
    /// How much volume scaling is applied to the original footstep sound
    /// 0: Full volume original footstep
    /// 1: Mutes original footstep
    /// </summary>
    [Range(0,1)]
    public float IgnoreActualFootstepSound = 0;
    
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
