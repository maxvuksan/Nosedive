using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the spawning and reuse of bird GameObjects
/// </summary>
public class BirdManager : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField] int _birdSpawnPercent;
    [SerializeField] private GameObject _birdPrefab;
    private static Stack<GameObject> _inactiveBirds = new(); // bird objects to reuse

    void OnEnable()
    {
        LevelFullMap.OnLevelLoad += OnLevelLoad;    
        LevelFullMap.OnLevelUnload += OnLevelUnload;    
    }

    void OnDisable()
    {
        LevelFullMap.OnLevelLoad -= OnLevelLoad;    
        LevelFullMap.OnLevelUnload -= OnLevelUnload;    
    }


    /// <summary>
    /// When a level loads we spawn birds at the configured bird spawn positions
    /// </summary>
    public void OnLevelLoad(Level level)
    {
        // no birds for this level...
        if(level?.BirdSpawnPositionsParent == null)
        {
            return;
        }

        for(int i = 0; i < level.BirdSpawnPositionsParent.childCount; i++){
            
            float chance = Random.Range(0, 100);
            if(chance < _birdSpawnPercent)
            {
                SpawnBirdInLevel(level, level.BirdSpawnPositionsParent.GetChild(i));
            }
        }
    }

    public void OnLevelUnload(Level level)
    {
        BirdAI[] birds = level.GetComponentsInChildren<BirdAI>();

        //the birds are children of this object
        for(int i = 0; i < birds.Length; i++)
        {
            if (birds[i].gameObject.activeSelf)
            {
                MarkBirdForReuse(birds[i]);
            }
        }
    }

    /// <summary>
    /// Plays a sound for when a bird flys away
    /// </summary>
    /// <param name="birdFlyingAway">The bird which is flying away</param>
    /// <param name="propogateLayer">How directly is the bird scared, 1 for player scared...</param>
    public static void TriggerBirdFlyAwaySound(BirdAI birdFlyingAway, int propogateLayer)
    {
        if(propogateLayer == 0)
        {
            return;
        }

        float volume = 1.0f / propogateLayer;

        // not enough volume
        if(volume < 0.15f)
        {
            return;
        }

        AudioManager.Singleton.Play("Bird_FlyAway", birdFlyingAway.transform.position, volume);
    }

    /// <summary>
    /// Spawns a bird at a specific position in the world
    /// </summary>
    private void SpawnBirdInLevel(Level level, Transform birdSpawnPosition)
    {
        GameObject newBird;

        if(_inactiveBirds.Count > 0)
        {
            newBird = _inactiveBirds.Pop();
            newBird.transform.parent = level.transform;
            newBird.SetActive(true);
        }
        else
        {
            newBird = Instantiate(_birdPrefab, level.transform);
        }

        newBird.transform.position = birdSpawnPosition.transform.position;

        var config = birdSpawnPosition.GetComponent<BirdSpawnpointConfiguration>();
        if(config == null)
        {
            newBird.GetComponent<BirdAI>().PreferredFlyDirection = Vector3.zero;
        }
        else
        {
            newBird.GetComponent<BirdAI>().PreferredFlyDirection = config.PreferredFlyDirection;
        }
    }

    /// <summary>
    /// Marks a bird to be reused by future SpawnBirdAtPosition calls
    /// </summary>
    public static void MarkBirdForReuse(BirdAI bird)
    {
        _inactiveBirds.Push(bird.gameObject);
        bird.gameObject.SetActive(false);
    }
}
