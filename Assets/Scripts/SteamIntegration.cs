using System;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Initalizes and interacts with steamworks
/// </summary>
public class SteamIntegration : MonoBehaviour
{
    private static SteamIntegration Singleton = null;
    private static bool Initalized;
    
    /// <summary>
    /// The id of the steam game
    /// </summary>
    [SerializeField] private uint _appId = 4289250;

    void Start()
    {
        Initalized = false;
        
        if (Singleton != null)
        {
            Destroy(this);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(this);

        try
        {
            Steamworks.SteamClient.Init(_appId);

            Initalized = true;            
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    static public void UnlockAchievement(string achievementId)
    {
        if (!Initalized)
        {
            return;
        }

        var ach = new Steamworks.Data.Achievement(achievementId);
        ach.Trigger();

        Debug.Log($"Achievement {achievementId} triggered");
    }

    static public void ClearAchievementStatus(string achievementId)
    {
        if (!Initalized)
        {
            return;
        }

        var ach = new Steamworks.Data.Achievement(achievementId);
        ach.Clear();
        Debug.Log($"Achievement {achievementId} cleared");
    }

    void Update()
    {
        if (!Initalized)
        {
            return;
        }

        Steamworks.SteamClient.RunCallbacks();
    }

    void OnApplicationQuit()
    {
        if (!Initalized)
        {
            return;
        }

        Steamworks.SteamClient.Shutdown();
    }

    /// <summary>
    /// Editor buttons to unlock and clear achievements
    /// </summary>
    
    [SerializeField] string testAchievementId;

    [Button("Unlock (testAchievementId)")]
    private void UnlockTestAchievement()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Enter Play Mode first."); return; }
        UnlockAchievement(testAchievementId);
    }

    [Button("Clear (testAchievementId)")]
    private void ClearTestAchievement()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Enter Play Mode first."); return; }
        ClearAchievementStatus(testAchievementId);
    }


}
