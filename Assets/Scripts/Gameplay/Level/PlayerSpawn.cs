using UnityEngine;

/// <summary>
/// Player spawn should be added to each level spawnpoint, it
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PlayerSpawn : MonoBehaviour
{


    /// <summary>
    /// Updates the current spawnpoint to be this spawnpoint
    /// </summary>
    /// <param name="other">The player</param>
    public void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<SimpleWalker>();

        if(player == null)
        {
            return;
        }

        LevelFullMap.Singleton.LevelToSpawnAtIndex = GetComponentInParent<Level>().LevelIndex;
    }

}
