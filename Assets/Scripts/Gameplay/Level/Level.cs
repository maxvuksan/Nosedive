using UnityEngine;


/// <summary>
/// A sub section of the game map
/// </summary>
public class Level : MonoBehaviour
{   
    /// <summary>
    /// GameObjects to enable when the player is in the range of this lkevl
    /// </summary>
    public GameObject[] ActiveDetails;

    /// <summary>
    /// Where the player spawns when the level loads, this position is leveled to ground height during the LevelFullMap pre-processing pass
    /// </summary>
    public Transform PlayerSpawn; 

    /// <summary>
    /// The transform the camera should match when previewing the level in the level select screen
    /// </summary>
    public Transform CameraPreviewPosition; 

    public Transform BirdSpawnPositionsParent; // where birds could spawn for this level, all spawnpoints should be placed under a parent transform

    public LevelEnviromentSettings EnviromentSettings;

    public bool IsActiveLevel { get; private set;}


    /// <summary>
    /// Is set by LevelFullMap, this is the the index of the level in relation to all the levels
    /// </summary>
    public int LevelIndex { get; set;}

    /// <summary>
    /// The bounding box of all the children meshes and colliders, this is set during the level pre-processing stage
    /// </summary>
    public Bounds GeometryBounds 
    { 
        get => _geometryBounds; 
        set => _geometryBounds = value; 
    }
    [HideInInspector] [SerializeField] private Bounds _geometryBounds;
    private Collider[] _allColliders;
 

    private void Awake()
    {
        _allColliders = GetComponentsInChildren<Collider>(true);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;

        Gizmos.DrawLine(PlayerSpawn.position, PlayerSpawn.position + PlayerSpawn.forward * 10);
        Gizmos.DrawLine(CameraPreviewPosition.position, CameraPreviewPosition.position + CameraPreviewPosition.forward * 10);

        if(BirdSpawnPositionsParent == null)
        {
            return;
        }

        Gizmos.color = Color.burlywood;
        for(int i = 0; i < BirdSpawnPositionsParent.childCount; i++)
        {
            Gizmos.DrawSphere(BirdSpawnPositionsParent.GetChild(i).transform.position, 2.0f);
        }
    }


    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(GeometryBounds.center, GeometryBounds.size);

        // player death zone...
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 deathBounds = new Vector3(GeometryBounds.size.x, 1, GeometryBounds.size.z);
        Vector3 deathOrigin = new Vector3(GeometryBounds.center.x, EnviromentSettings.DeathZoneHeight, GeometryBounds.center.z);
        Gizmos.DrawCube(deathOrigin, deathBounds);
        
    }

    /// <summary>
    /// Is this level the loaded level? (if this level is enabled and the neighbour of the loaded level, it is not considered the active level)
    /// </summary>
    /// <param name="isActiveLevel">Are we the active level</param>
    public void SetIsActiveLevel(bool isActiveLevel)
    {
        // state is unchanged
        if(isActiveLevel == IsActiveLevel)
        {
            return;
        }

        IsActiveLevel = isActiveLevel;


        // activate very specific details here...
        
        Helpers.SetActiveGameObjectArray(ActiveDetails, isActiveLevel);

        // Set colliders to be 
        if (_allColliders != null)
        {
            for (int i = 0; i < _allColliders.Length; i++)
            {
                if (_allColliders[i] != null)
                {
                    _allColliders[i].enabled = isActiveLevel;
                }
            }
        }
    }

}
