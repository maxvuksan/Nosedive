using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the state of the game at a high level
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        OptionsMenu,
        SelectingLevel,
        Playing,
        LoseBlackScreenWipe,
        WinWhiteScreenWipe,
    }

    /// <summary>
    /// Enables these objects when a specific game state is active...
    /// </summary>

    public GameObject[] enableWhenMainMenu;
    public GameObject[] enableWhenOptionsMenu;
    public GameObject[] enableWhenSelectingLevel;
    public GameObject[] enableWhenPlaying;
    public GameObject[] enableWhenLose;
    public GameObject[] enableWhenWin;


    /// <summary>
    /// Subscrible actions which trigger when a specific game state becomes active
    /// </summary>
    public static Action OnStatePlay;
    public static Action OnStateSelectingLevel;
    public static Action OnStateLoseScreen;
    public static Action OnStateWinScreen;
    
    /// <summary>
    /// The current game state, should be set through SetState()
    /// </summary>
    public static GameState CurrentState { get; private set; }

    public static GameStateManager Singleton;

    void Awake()
    {
        Helpers.CreateSingleton<GameStateManager>(ref Singleton, this);
    }

    void Start()
    {
        SaveManager.Load();
        LevelSelection.Singleton.LoadLevelPreview();
        SetState(GameState.MainMenu);
    }
    
    private void SetActiveArrayExclusive(GameObject[] arrayToEnable)
    {
        var activeSet = new HashSet<GameObject>(arrayToEnable);
        var processed = new HashSet<GameObject>();

        foreach (var array in new[]
        {
            enableWhenMainMenu,
            enableWhenOptionsMenu,
            enableWhenSelectingLevel,
            enableWhenPlaying,
            enableWhenLose,
            enableWhenWin
        })
        {
            foreach (var obj in array)
            {
                if (obj != null && processed.Add(obj)){
                    obj.SetActive(activeSet.Contains(obj));
                }
            }
        }
    }

    /// <summary>
    /// Changes the current game state enabling/disabling the respective object array and invoking the state action
    /// </summary>
    /// <param name="state">The new game state to change to</param>
    public void SetState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
            {
                SetActiveArrayExclusive(enableWhenMainMenu);
                break;
            }
            case GameState.OptionsMenu:
            {
                SetActiveArrayExclusive(enableWhenOptionsMenu);
                break;
            }
            case GameState.SelectingLevel:
            {
                SetActiveArrayExclusive(enableWhenSelectingLevel);
                OnStateSelectingLevel?.Invoke();
                break;
            }
            case GameState.Playing:
            {
                SetActiveArrayExclusive(enableWhenPlaying);
                OnStatePlay?.Invoke();
                break;
            }
            case GameState.LoseBlackScreenWipe:
            {
                SetActiveArrayExclusive(enableWhenLose);
                OnStateLoseScreen?.Invoke();
                break;
            }
            case GameState.WinWhiteScreenWipe:
            {
                SetActiveArrayExclusive(enableWhenWin);
                OnStateWinScreen?.Invoke();
                break;
            }
        
        }

        CurrentState = state;
    }


    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AudioManager.Singleton.Play(Helpers.Singleton.UiBlipSubmitSoundLabel);
            SetState(GameState.MainMenu);
        }
    }
}
