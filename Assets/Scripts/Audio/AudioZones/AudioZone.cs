using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// The shape of the audio zone influence region
/// </summary>
public enum AudioZoneShape
{
    Box,
    Sphere,
}


/// <summary>
/// An audio zone defines a zone for localized audio effects to be applied
/// </summary>
public class AudioZone : MonoBehaviour
{

    [Header("Configuration")]

    private bool _isBox { get => Shape == AudioZoneShape.Box;}
    private bool _isSphere { get => Shape == AudioZoneShape.Sphere;}

    /// <summary>
    /// The size of the audio zone if our shape is a sphere
    /// </summary>
    [DisableIf("_isBox")]
    [Range(0,1000)]
    public float Radius = 1;

    /// <summary>
    /// The size of the audio zone if our shape is a box
    /// </summary>
    [DisableIf("_isSphere")]
    public Vector3 Size;

    public AudioZoneShape Shape;

    /// <summary>
    /// How strongly the audio zones influence fades in
    /// </summary>
    [Range(0,1)]
    public float Feathering;

    void OnEnable()
    {
        AudioZoneManager.AddZone(this);    
    }

    void OnDisable()
    {
        AudioZoneManager.RemoveZone(this);    
    }


    /// <summary>
    /// Gets the amount of influence the audio zone should have to a specific world position
    /// </summary>
    /// <param name="worldPosition">The position to calculate for</param>
    /// <returns>A value from 0-1 indicating the influence strength (0 is none, 1 is full)</returns>
    public float GetInfluenceFactor(Vector3 worldPosition)
    {
        if(Shape == AudioZoneShape.Box){

            // Calculate vector offset relative to world position to ignore object rotation/scale
            Vector3 worldOffset = worldPosition - transform.position;
            Vector3 halfSize = Size * 0.5f;

            if (halfSize.x <= 0 || halfSize.y <= 0 || halfSize.z <= 0) return 0f;

            // Check distance along world axes
            float tx = Mathf.Abs(worldOffset.x) / halfSize.x;
            float ty = Mathf.Abs(worldOffset.y) / halfSize.y;
            float tz = Mathf.Abs(worldOffset.z) / halfSize.z; 

            float t = Mathf.Max(tx, ty, tz);

            return Mathf.Clamp01(Mathf.InverseLerp(1f, 1f - Feathering, t));

        }
        else if (Shape == AudioZoneShape.Sphere)
        {
            if (Radius <= 0) {
                // Prevent division by zero if radius is zero
                return 0f;
            }

            float distance = Vector3.Distance(transform.position, worldPosition);

            // Normalize distance relative to the radius (0 = center, 1 = edge)
            float t = distance / Radius;

            // t < 1 = inside sphere, remap feather region to 0-1
            return Mathf.Clamp01(Mathf.InverseLerp(1f, 1f - Feathering, t));
        }

        return 0.0f;
    }

    private void OnDrawGizmos()
    {
        // Cache and setup the transformation matrix to support rotation and scaling
        
        DrawGizmoInfluence(1.0f, Color.green);
        DrawGizmoInfluence(Mathf.Clamp01(1f - Feathering), new Color(1f, 0.8f, 0f, 1f));
    }

    /// <summary>
    /// Draws a wireframe to represent an portion of the influence 
    /// </summary>
    /// <param name="portion">The amount of size to cover (0-1)</param>
    /// <param name="colour">The colour of the wireframe</param>
    protected void DrawGizmoInfluence(float portion, Color colour)
    {
        Gizmos.color = colour;

        if (Shape == AudioZoneShape.Box)
        {
            Gizmos.DrawWireCube(transform.position, Size * portion);
        }
        else if (Shape == AudioZoneShape.Sphere)
        {
            Gizmos.DrawWireSphere(transform.position, Radius * portion);
        }
    }

}
