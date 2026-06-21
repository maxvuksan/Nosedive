using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FilterSettings
{
    [Range(0,1)]
    public float Size;
    [Range(0,1)]
    public float Feather;

    [Range(10, 22000)]
    public float MinCutoffFrequency;

    [Range(10, 22000)]
    public float MaxCutoffFrequency;
}

/// <summary>
/// Fades in audio loops 
/// </summary>
public class AudioZoneLooped : AudioZone
{

    /// <summary>
    /// Does this loop go through a lowpass filter?
    /// </summary>
    public bool UseLowPassFilter;
    public FilterSettings LowPassFilterSettings;

    /// <summary>
    /// The looping sounds to fade in when in this zone, the volumeScaler is scaled by the player proximity to the zone
    /// </summary>
    public string[] LoopsToFadeIn;

    void Start()
    {
        foreach(string loop in LoopsToFadeIn)
        {
            if (UseLowPassFilter)
            {
                LoopingAudioManager.Singleton.AttachLowPassFilterToLoop(loop);
            }
        }
    }

    /// <summary>
    /// Gets the amount of filter influence this zone should have at a specific world position
    /// </summary>
    /// <param name="worldPosition">The position to calculate for</param>
    /// <returns>A value from 0-1 indicating the filter strength (0 is none/outer edge, 1 is full/inner core)</returns>
    public float GetFilterInfluenceFactor(Vector3 worldPosition)
    {
        if (!UseLowPassFilter){ 
            return 0f;
        }

        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        Vector3 filterMaxBoxSize = Size * Mathf.Clamp01(LowPassFilterSettings.Size);
        Vector3 halfSize = filterMaxBoxSize * 0.5f;

        if (halfSize.x <= 0f || halfSize.y <= 0f || halfSize.z <= 0f) {
            return 0f;
        }

        float tx = Mathf.Abs(localPos.x) / halfSize.x;
        float ty = Mathf.Abs(localPos.y) / halfSize.y;
        float tz = Mathf.Abs(localPos.z) / halfSize.z;

        float t = Mathf.Max(tx, ty, tz);

        if (t >= 1f) {
            return 0f;
        }

        // t < 1 = inside the filter box, remap feather region to 0-1 factor
        // 0 at the outer edge (1f), 1 at the inner core (1f - Feather)
        return Mathf.Clamp01(Mathf.InverseLerp(1f, 1f - LowPassFilterSettings.Feather, t));
    }


    private void OnDrawGizmosSelected() 
    {
        if (!UseLowPassFilter) return;  

        // Cache matrix and match the parent object's local transformation
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Set gizmo colour to solid red
        Gizmos.color = Color.red;

        // Outer box (matches the filter size percentage relative to the parent Size)
        Vector3 maxBoxSize = Size * Mathf.Clamp01(LowPassFilterSettings.Size);

        // Inner box (scaled down by the feather percentage)
        float innerScale = Mathf.Clamp01(1f - LowPassFilterSettings.Feather);
        Vector3 minBoxSize = maxBoxSize * innerScale;

        // Draw the simple wireframes
        Gizmos.DrawWireCube(Vector3.zero, minBoxSize);
        Gizmos.DrawWireCube(Vector3.zero, maxBoxSize);

        // Restore matrix state
        Gizmos.matrix = oldMatrix;
    }
}
