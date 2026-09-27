using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Functions to drive the functionality of the games menu
/// </summary>
public class MenuFunctions : MonoBehaviour
{
    /// <summary>
    /// When the game is loaded we want to update our ui elements to reflect the saved state
    /// </summary>
    [Header("Settings")]
    [SerializeField] private CustomSlider _soundAudioSlider;
    [SerializeField] private CustomSlider _enviromentAudioSlider;
    [SerializeField] private CustomTextCarousel _displayModeCarousel;


    private void Awake()
    {
        SaveManager.OnLoad += RestoreSavedSettings;
    }
    private void OnDestroy()
    {
        SaveManager.OnLoad -= RestoreSavedSettings;
    }

    private void RestoreSavedSettings()
    {
        _soundAudioSlider.Value = SaveManager.Data.Settings.SoundVolume;
        _enviromentAudioSlider.Value = SaveManager.Data.Settings.EnvironmentVolume;
        _displayModeCarousel.Index = SaveManager.Data.Settings.DisplayMode;
    }


    /// <summary>
    /// Sets the game to a specific state through GameStateManager
    /// </summary>

    #region  Game States 

    public static void SetState_Play()
    {
        GameStateManager.Singleton.SetState(GameStateManager.GameState.Playing);
    }
    public static void SetState_MainMenu()
    {
        GameStateManager.Singleton.SetState(GameStateManager.GameState.MainMenu);
        // Note: We save here because this is the state we reach after leaving the settings (Options -> Menu)
        SaveManager.Save();
    }
    public static void SetState_Inventory()
    {
        GameStateManager.Singleton.SetState(GameStateManager.GameState.Inventory);
    }
    public static void SetState_Options()
    {
        GameStateManager.Singleton.SetState(GameStateManager.GameState.OptionsMenu);
    }

    #endregion

    public void QuitGame()
    {
        // Note: we can't save on OnApplicationQuit because the sliders may be destroyed first
        SaveManager.Save();
        Application.Quit();
    }

    /// <summary>
    /// Applies setting changes to the application state
    /// </summary>

    #region Callbacks

    public void OnSoundVolumeChange(float volume)
    {
        SaveManager.Data.Settings.SoundVolume = volume;
    }

    public void OnEnviromentVolumeChange(float volume)
    {
        SaveManager.Data.Settings.EnvironmentVolume = volume;
    }
    
    public void OnMouseSensitivityChange(float scaler)
    {
        SaveManager.Data.Settings.MouseSensitivityScaler = scaler;
    }

    public void OnDisplayModeChange(int displayModeIndex)
    {
        // displayModeIndex index values map to the following values
        
        // 0: Fullscreen
        // 1: Windowed
        // 2: Borderless
        
        // Note: This was not made an enum because enums can't be assigned to the UnityEvent callbacks
        
        int width = Screen.currentResolution.width;
        int height = Screen.currentResolution.height;

        switch (displayModeIndex)
        {
            case 0:
                Screen.SetResolution(width, height, FullScreenMode.ExclusiveFullScreen);
                break;

            case 1:
                // Scale window by 0.9 so that you windowed mode fits on the screen
                int windowWidth = Mathf.RoundToInt(width * 0.9f);
                int windowHeight = Mathf.RoundToInt(height * 0.9f);
                Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);
                break;

            case 2:
                Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
                break;
        }

        SaveManager.Data.Settings.DisplayMode = displayModeIndex;
    }

    #endregion
}

