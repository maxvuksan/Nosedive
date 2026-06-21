using System;
using UnityEngine;

[System.Serializable]
public struct UserSettings{
    public float SoundVolume;
    public float EnvironmentVolume;
    public int DisplayMode;
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
        
        bool loadSuccess = DataSerializer.LoadObjectFromFile<UserSaveData>(_saveFileName, ref loadedData);

        if (loadSuccess)
        {
            Data.Settings = loadedData.Settings;
            Data.Progress = loadedData.Progress;
        }
        else 
        {
            // we do not have a save file yet, initalize default values...

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
            };

        }

        OnLoad?.Invoke();
    }
}