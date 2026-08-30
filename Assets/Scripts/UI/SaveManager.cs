using System;
using System.Collections.Generic;

[System.Serializable]
public struct UserSettings{
    public float SoundVolume;
    public float EnvironmentVolume;
    public int DisplayMode;
}

/// <summary>
/// Note: This has been made a class so we can access it through UserGaneProgress.SceneDataList[index]... 
/// and recieve the reference not a copy
/// </summary>
[System.Serializable]
public class SceneData
{
    /// <summary>
    /// Has the collectable sphere in this scene been triggered
    /// </summary>
    public bool CollectableSphereTriggered = false;
}

[System.Serializable]
public struct UserGameProgress
{
    /// <summary>
    /// The current scene/level index we are in, this is different to the unlock scene incase we reply levels 
    /// </summary>
    public int CurrentScene;

    /// <summary>
    /// The furthest scene/level index we have reached
    /// </summary>
    public int UnlockedScene;

    /// <summary>
    /// A list of persistant data for each scene/level
    /// </summary>
    public List<SceneData> SceneDataList;
}

/// <summary>
/// Data which persists multiple play sessions, this includes configured settings, and progress in the game
/// </summary>
[System.Serializable]
public struct UserSaveData
{
    public UserSettings Settings;
    public UserGameProgress Progress;
}

/// <summary>
/// Static class to perform saving operations
/// </summary>
public static class SaveManager
{
    public static UserSaveData Data;

    public static Action OnLoad;

    private static string _saveFileName = "/user.save";



    public static void Save()
    {
        DataSerializer.SaveObjectToFile(Data, _saveFileName);      
    }

    public static void Load()
    {
        UserSaveData loadedData = new();
        
        bool loadSuccess = DataSerializer.LoadObjectFromFile(_saveFileName, ref loadedData);

        // Allocate a SceneData entry for each scene/level
        List<SceneData> sceneDataList = new List<SceneData>(LevelFullMap.Singleton.Levels.Length);
        for (int i = 0; i < LevelFullMap.Singleton.Levels.Length; i++)
        {
            sceneDataList.Add(new SceneData());
        }

        if (loadSuccess)
        {
            Data.Settings = loadedData.Settings;
            Data.Progress = loadedData.Progress;

            Data.Progress.SceneDataList = sceneDataList;

            for (int i = 0; i < LevelFullMap.Singleton.Levels.Length; i++)
            {
                // If the entry exists use that data
                if(loadedData.Progress.SceneDataList.Count > i)
                {
                    Data.Progress.SceneDataList[i] = loadedData.Progress.SceneDataList[i];
                }
            }

        }
        else 
        {
            // We do not have a save file yet, initalize default values...

            Data.Settings = new UserSettings 
            { 
                SoundVolume = 1.0f, 
                EnvironmentVolume = 1.0f, 
                DisplayMode = 0 
            };

            Data.Progress = new UserGameProgress 
            { 
                CurrentScene = 0,
                UnlockedScene = 0, 
                SceneDataList = sceneDataList
            };
        }

        OnLoad?.Invoke();
    }
}