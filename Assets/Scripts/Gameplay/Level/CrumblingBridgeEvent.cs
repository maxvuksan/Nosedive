using UnityEngine;


/// <summary>
/// Script to drive the crumbling bridge event
/// </summary>
public class CrumblingBridgeEvent : MonoBehaviour
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

    public void ExplodeBridge()
    {
        AudioManager.Singleton.Play("CrumblingBridge_Explode");
        _animator.SetBool("Exploded", true);

        foreach(var entry in _fxObjectSpawns)
        {
            GameObject newObject = Instantiate(entry.PrefabToSpawn, entry.PositionToSpawn); 

            // register to ensure it is destroyed when scene select state is entered...
            TemporaryGameobjectContainer.Register(newObject);
        }
    }


   
}
