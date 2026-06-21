using UnityEngine;

public class BirdSpawnpointConfiguration : MonoBehaviour
{
    public Vector3 PreferredFlyDirection;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.orangeRed;

        Gizmos.DrawLine(transform.position, transform.position + PreferredFlyDirection * 10);

    }
}
