using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Is responsible for loading and unloading levels depending on where the player is in the world and which level bounds he overlaps
/// </summary>
public class LevelStreamer : MonoBehaviour
{
    private SimpleWalker _player;

    /// <summary>
    /// Tracks the index of levels the player overlaps 
    /// </summary>
    private HashSet<int> _overlappingLevelIndicies;

    void Awake()
    {
        _overlappingLevelIndicies = new();
        _player = FindFirstObjectByType<SimpleWalker>(FindObjectsInactive.Include);    
    }

    void FixedUpdate() 
    {
        // only perform level streaming in playmode
        if(GameStateManager.CurrentState != GameStateManager.GameState.Playing)
        {
            return;
        }

        _overlappingLevelIndicies.Clear();

        int levelIndex = 0;
        foreach(var level in LevelFullMap.Singleton.Levels)
        {
            // load
            if (level.GeometryBounds.Contains(_player.transform.position))
            {
                _overlappingLevelIndicies.Add(levelIndex);
                LevelFullMap.Singleton.LoadLevel(levelIndex);
            }
            // all levels not present in the HashSet are considered not active

            levelIndex++;
        }

        LevelFullMap.Singleton.LoadLevelHashSet(ref _overlappingLevelIndicies);
    }
}
