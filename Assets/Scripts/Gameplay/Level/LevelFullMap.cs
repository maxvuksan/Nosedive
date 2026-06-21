using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// The entire game world, broken up into levels, enables setting levels as active, and loading neighbouring levels
/// </summary>
public class LevelFullMap : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public Level[] Levels { get => _levels; }

    [SerializeField] private Level[] _levels;
    
    public static LevelFullMap Singleton;

    public static Action<Level> OnLevelLoad;
    public static Action<Level> OnLevelUnload;

    /// <summary>
    /// Padding to apply to each levels precomputed bounds (to extend said bounds on each axis)
    /// </summary>
    [SerializeField] private Vector3 _levelGeometryBoundsPadding;
    
    public int LoadedLevelIndex { get => _loadedLevelIndex;}
    private int _loadedLevelIndex = -1;
    
    /// <summary>
    /// The level the player should spawn at
    /// </summary>
    public int LevelToSpawnAtIndex { 
        get 
        {
            return _levelToSpawnAtIndex; 
        }
        set 
        {
            if(_levelToSpawnAtIndex == value)
            {
                return;
            }

            _levelToSpawnAtIndex = value; 

            SaveManager.Data.Progress.CurrentScene = _levelToSpawnAtIndex;
            SaveManager.Data.Progress.UnlockedScene = Mathf.Max(_levelToSpawnAtIndex, SaveManager.Data.Progress.UnlockedScene);
            SaveManager.Save();
        }
    }

    private int _levelToSpawnAtIndex;
 
    void Awake()
    {   
        Helpers.CreateSingleton(ref Singleton, this);

        // tell each level its own index
        int i = 0;
        foreach(var level in _levels)
        {
            level.LevelIndex = i;
            i++;
        }
    }

    void Start()
    {
        LevelToSpawnAtIndex = SaveManager.Data.Progress.CurrentScene;
    }

    /// <summary>
    /// Preprocesses things to prevent runtime computation, 
    /// </summary>
    [ContextMenu("Preprocess data")]
    public void PreprocessData()
    {
        foreach(var level in _levels)
        {
            PreprocessLevel_RaycastBirdSpawnpoints(level);
            PreprocessLevel_RaycastPlayerSpawnpoint(level);
            PreprocessLevel_ComputeBounds(level);
        }
    }

    /// <summary>
    /// Computes the bounding box of all the levels meshes and colliders
    /// </summary>
    /// <param name="level">The level to compute for</param>
    private void PreprocessLevel_ComputeBounds(Level level)
    {
        bool initialized = false;
        Bounds bounds = new Bounds();

        foreach (var r in level.GetComponentsInChildren<MeshRenderer>())
        {
            if (!initialized) { bounds = r.bounds; initialized = true; }
            else bounds.Encapsulate(r.bounds);
        }

        foreach (var c in level.GetComponentsInChildren<Collider>())
        {
            if (!initialized) { bounds = c.bounds; initialized = true; }
            else bounds.Encapsulate(c.bounds);
        }

        bounds.Expand(_levelGeometryBoundsPadding);

        level.GeometryBounds = bounds;
    }

    /// <summary>
    /// Updates the spawnpoint to level with the ground so when the player spawns they do not fall at all
    /// </summary>
    /// <param name="level">The level to compute for</param>
    private void PreprocessLevel_RaycastPlayerSpawnpoint(Level level)
    {
        level.PlayerSpawn.position = FindFirstObjectByType<SimpleWalker>(FindObjectsInactive.Include).ShiftSpawnpointToLevelWithGround(level.PlayerSpawn.position);
    }

    /// <summary>
    /// Updates each bird spawnpoint to be level with the ground so that when birds spawn they are touching the ground
    /// </summary>
    /// <param name="level">The level to compute for</param>
    private void PreprocessLevel_RaycastBirdSpawnpoints(Level level)
    {
        // no birds in this level...
        if(level.BirdSpawnPositionsParent == null)
        {
            return;
        }

        for(int i = 0; i < level.BirdSpawnPositionsParent.childCount; i++)
        {
            if (Physics.Raycast(level.BirdSpawnPositionsParent.GetChild(i).position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, Mathf.Infinity, Helpers.Singleton.GroundLayerMask))
            {
                // Move the spawnpoint to the hit position
                level.BirdSpawnPositionsParent.GetChild(i).position = hit.point;
            }

            // if we do not hit a position, do nothing
        }
    }

    /// <summary>
    /// Unloads the existing level then loads a level at a given index
    /// </summary>
    /// <param name="levelIndex">The index to load, if an index < 0 only unloading the existing level will happen</param>
    public void LoadLevel(int levelIndex)
    {
        if(levelIndex < 0)
        {
            Helpers.SetActiveMonoBehaviourArray(_levels, false);
        }
        
        // ensure new index is within valid range
        _loadedLevelIndex = Mathf.Clamp(levelIndex, 0, _levels.Length - 1);

        var levelIndexHashSet = new HashSet<int> { levelIndex };

        EnableGameObjectsDependingOnLoadedLevels(ref levelIndexHashSet);
    }

    /// <summary>
    /// Loads multiple level indicies, enabling the unison of the loaded indicies neighbours
    /// </summary>
    /// <param name="activeLevelIndicies">A set of level indicies to set as active</param>
    public void LoadLevelHashSet(ref HashSet<int> activeLevelIndicies)
    {
        EnableGameObjectsDependingOnLoadedLevels(ref activeLevelIndicies);
    }

    /// <summary>
    /// Increments the loadedLevelIndex variable, and then loads the next index
    /// </summary>
    public void LoadNextLevel()
    {
        LoadLevel(_loadedLevelIndex + 1);
    }

    /// <summary>
    /// Gets the Level component of the currently active level
    /// </summary>
    /// <returns></returns>
    public Level GetActiveLevel()
    {
        if(_loadedLevelIndex == -1)
        {
            return null;
        }

        return _levels[_loadedLevelIndex];
    } 

    public Level GetLevelToSpawnAt()
    {
        return _levels[LevelToSpawnAtIndex];
    }

    /// <summary>
    /// Loads a set of active levels and the unison of their neighbours
    /// </summary>
    /// <param name="activeLevelIndicies">A list of levels that are considered active</param>
    private void EnableGameObjectsDependingOnLoadedLevels(ref HashSet<int> activeLevelIndicies)
    {
        HashSet<int> shouldBeVisible = new HashSet<int>();
        
        foreach (int i in activeLevelIndicies)
        {
            shouldBeVisible.Add(i - 1);
            shouldBeVisible.Add(i);
            shouldBeVisible.Add(i + 1);
        }

        for (int i = 0; i < _levels.Length; i++)
        {
            if (!shouldBeVisible.Contains(i))
            {
                if (_levels[i].gameObject.activeSelf)
                {
                    OnLevelUnload?.Invoke(_levels[i]);
                }

                _levels[i].gameObject.SetActive(false);
                _levels[i].SetIsActiveLevel(activeLevelIndicies.Contains(i));
                continue;
            }

            if (!_levels[i].gameObject.activeSelf)
            {
                OnLevelLoad?.Invoke(_levels[i]);
            }

            _levels[i].gameObject.SetActive(true);
            _levels[i].SetIsActiveLevel(activeLevelIndicies.Contains(i));
        }
            
    }
}
