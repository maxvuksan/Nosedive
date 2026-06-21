using UnityEngine;

/// <summary>
/// An audio zone defines a zone for localized audio effects to be applied
/// </summary>
public class AudioZone : MonoBehaviour
{
    [Header("Configuration")]
    
    /// <summary>
    /// The size of the audio zone
    /// </summary>
    [SerializeField] protected Vector3 Size;

    /// <summary>
    /// How strongly the audio zones influence fades in
    /// </summary>
    [Range(0,1)]
    [SerializeField] protected float Feathering;

    void OnEnable()
    {
        AudioZoneManager.Singleton.AddZone(this);    
    }
    
    void OnDisable()
    {
        AudioZoneManager.Singleton.RemoveZone(this);    
    }

    /// <summary>
    /// Gets the amount of influence the audio zone should have to a specific world position
    /// </summary>
    /// <param name="worldPosition">The position to calculate for</param>
    /// <returns>A value from 0-1 indicating the influence strength (0 is none, 1 is full)</returns>
    public float GetInfluenceFactor(Vector3 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        Vector3 halfSize = Size * 0.5f;

        // How close to the edge on each axis (0 = center, 1 = edge)
        float tx = Mathf.Abs(localPos.x) / halfSize.x;
        float ty = Mathf.Abs(localPos.y) / halfSize.y;
        float tz = Mathf.Abs(localPos.z) / halfSize.z;

        // Worst axis determines if we're inside, and how close to the edge
        float t = Mathf.Max(tx, ty, tz);

        // t < 1 = inside box, remap feather region to 0-1
        return Mathf.Clamp01(Mathf.InverseLerp(1f, 1f - Feathering, t));
    }

    private void OnDrawGizmos()
    {
        // Draw using the object's transform
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Outer box (full area including feather)
        Vector3 outerSize = new Vector3(Size.x, Size.y, Size.z);

        // Inner box (100% effect area)
        float innerScale = Mathf.Clamp01(1f - Feathering);
        Vector3 innerSize = new Vector3(
            Size.x * innerScale,
            Size.y * innerScale,
            Size.z * innerScale);

        // Feather region
        Gizmos.color = new Color(1f, 0.8f, 0f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, outerSize);

        // Solid region
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, innerSize);

        Gizmos.matrix = oldMatrix;

        #if UNITY_EDITOR
            // Audio icon in center
            Gizmos.DrawIcon(transform.position, "AudioSource Gizmo", true);
        #endif
    }
}
