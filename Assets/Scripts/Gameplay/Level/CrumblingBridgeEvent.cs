using UnityEngine;


/// <summary>
/// Script to drive the crumbling bridge event
/// </summary>
public class CrumblingBridgeEvent : PressurePlateListener
{
    [System.Serializable]
    protected struct ObjectSpawn{

        public Transform PositionToSpawn;
        public GameObject PrefabToSpawn;
    }

    /// <summary>
    /// Objects to be spawned at specific locations e.g. particle effects
    /// </summary>
    [SerializeField] private ObjectSpawn[] _fxObjectSpawns;
    [SerializeField] private Animator _animator;
    


    /// <summary>
    /// Triggers the bridge destruction
    /// </summary>
    /// <param name="pressurePlateState">Will only explode bridge if this is true</param>
    public override void OnSwitchState(bool pressurePlateState)
    {
        if (pressurePlateState)
        {
            _animator.SetBool("Exploded", true);
            SpawnFxObjects();            
        }
    }

    public void SpawnFxObjects()
    {
        foreach(var entry in _fxObjectSpawns)
        {
            GameObject newObject = Instantiate(entry.PrefabToSpawn, entry.PositionToSpawn); 

            // register to ensure it is destroyed when scene select state is entered...
            TemporaryGameobjectContainer.Register(newObject);
        }
    }


   
}
