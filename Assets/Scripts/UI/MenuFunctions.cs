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
        SaveManager.OnLoad += OnLoad;
    }
    private void OnDestroy()
    {
        SaveManager.OnLoad -= OnLoad;
    }

    private void OnLoad()
    {
        _soundAudioSlider.Value = SaveManager.Data.Settings.SoundVolume;
        _enviromentAudioSlider.Value = SaveManager.Data.Settings.EnvironmentVolume;
        _displayModeCarousel.Index = SaveManager.Data.Settings.DisplayMode;
    }


    /// <summary>
    /// Sets the the game to a specific state through GameStateManager
    /// </summary>

    #region  Game States 

    public void SetState_Play()
    {
        GameStateManager.Singleton.SetState(GameStateManager.GameState.Playing);
    }
    public void SetState_MainMenu()
    {
        GameStateManager.Singleton.SetState(GameStateManager.GameState.MainMenu);
    }
    public void SetState_LevelSelect()
    {
        GameStateManager.Singleton.SetState(GameStateManager.GameState.SelectingLevel);
    }
    public void SetState_Options()
    {
        GameStateManager.Singleton.SetState(GameStateManager.GameState.OptionsMenu);
    }

    #endregion




    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        // save on quit...
        SaveManager.Save();
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

    public void OnDisplayModeChange(int displayModeIndex)
    {
        // 0: Fullscreen
        // 1: Windowed
        // 2: Borderless

        int width = Screen.currentResolution.width;
        int height = Screen.currentResolution.height;

        switch (displayModeIndex)
        {
            case 0:
                Screen.SetResolution(width, height, FullScreenMode.ExclusiveFullScreen);
                break;

            case 1:
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

