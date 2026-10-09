using UnityEngine;

/// <summary>
/// Configuration for how a spawned bird behaves
/// </summary>
public class BirdSpawnpointConfiguration : MonoBehaviour
{
    public Vector3 PreferredFlyDirection;
    public bool DisableFlyingAway;
    
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.orangeRed;

        Gizmos.DrawLine(transform.position, transform.position + PreferredFlyDirection * 10);

    }
}
