using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct LevelSelectionChipData
{
    public LevelSelectionChip Chip;
}

public class LevelSelection : MonoBehaviour
{   
    [SerializeField] private ColourFader _colourFaderLevelSwitch;
    [SerializeField] private GameObject _levelChipPrefab;
    [SerializeField] private Transform _levelChipParent;
    [SerializeField] private RawImage _levelPreviewImage;
    [SerializeField] private UIElementJolt _levelPreviewJolter;

    /// <summary>
    /// Is called when the level preview is cycled between chips
    /// </summary>
    public static Action OnLevelPreviewChange;

    private GameObject _loadedLevel;

    public static LevelSelection Singleton;
    
    private bool _chipsGenerated = false;
    private List<LevelSelectionChipData> _levelChips;
    private int _selectedChip;
    private bool _initalLoaded;

    public void Awake()
    {
        Helpers.CreateSingleton(ref Singleton, this);
    }

    public void Start()
    {
        LoadInital();
    }
    
    public void OnEnable()
    {
        LoadInital();
    }

    private void LoadInital()
    {
        if(_initalLoaded || LevelFullMap.Singleton == null)
        {
            return;
        }
        
        _selectedChip = LevelFullMap.Singleton.LevelToSpawnAtIndex; 
        LevelFullMap.Singleton.LoadLevel(-1);
        LoadLevelPreview();
        UpdateLevelSelectionChipColours();
        StepSelected(0);

        _initalLoaded = true;
    }

    /// <summary>
    /// Generates objects associated with each level (e.g. level chips)
    /// </summary>
    private void GenerateLevelObjects()
    {
        _levelChips = new List<LevelSelectionChipData>();

        for(int i = 0; i < LevelFullMap.Singleton.Levels.Count; i++)
        {
            GameObject newGameobject = Instantiate(_levelChipPrefab, _levelChipParent);
            LevelSelectionChip chip = newGameobject.GetComponent<LevelSelectionChip>();
            chip.SetSelected(false);

            LevelSelectionChipData data;
            data.Chip = chip;
            _levelChips.Add(data);
        }
    }

    private void UpdateLevelSelectionChipColours()
    {
        if(_levelChips == null)
        {
            print("Generating level chips");
            GenerateLevelObjects();
        }

        for(int i = 0; i < LevelFullMap.Singleton.Levels.Count; i++)
        {
            if (i > SaveManager.Data.Progress.UnlockedScene && !Helpers.Singleton.DebugMode)
            {
                _levelChips[i].Chip.SetColour(Helpers.Colours.UiDisabled);
            }
            else
            {
                _levelChips[i].Chip.SetColour(Helpers.Colours.UiSelected);
            }
        }
    }

    
    public void Update()
    {
        if (InputManager.LeftInputOnPress())
        {
            if (StepSelected(-1))
            {
                AudioManager.Singleton.Play(Helpers.Singleton.UiBlipDownSoundLabel);
            }
        }
        else if (InputManager.RightInputOnPress())
        {
            if (StepSelected(1))
            {
                AudioManager.Singleton.Play(Helpers.Singleton.UiBlipUpSoundLabel);
            }
        }
        if(InputManager.JumpInputOnPress() || InputManager.InteractInputOnPress())
        {
            LoadLevel(_selectedChip);
            GameStateManager.Singleton.SetState(GameStateManager.GameState.Playing);
            AudioManager.Singleton.Play(Helpers.Singleton.UiBlipSubmitSoundLabel);
        }
    }

    /// <summary>
    /// Unloads the existing level then loads a level at a given index
    /// </summary>
    /// <param name="levelIndex">The index to load, if an index < 0 only unloading the existing level will happen</param>
    private void LoadLevel(int levelIndex)
    {
        LevelFullMap.Singleton.LevelToSpawnAtIndex = levelIndex;
        LevelFullMap.Singleton.LoadLevel(levelIndex);
    }



    /// <summary>
    /// Moves to a new selected chip, stepping in the specified x direction, returns true if the step was successful
    /// </summary>
    /// <param name="direction">The direction on the x axis to step (-1 for left, 1 for right)</param>
    public bool StepSelected(int direction)
    {
        if(_levelChips == null){
            return false;
        }

        int proposedNewChip = _selectedChip + direction;

        // wrap around index accounting for number of levels
        if(proposedNewChip >= _levelChips.Count)
        {
            proposedNewChip = 0;
        }
        else if(proposedNewChip < 0)
        {
            proposedNewChip = _levelChips.Count - 1;
        }

        // we have not unlocked that level yet
        if(proposedNewChip > SaveManager.Data.Progress.UnlockedScene && !Helpers.Singleton.DebugMode)
        {
            return false;
        }

        _selectedChip = proposedNewChip;

        
        for(int i = 0; i < _levelChips.Count; i++)
        {
            if(i == _selectedChip)
            {
                _levelChips[i].Chip.SetSelected(true);

                if(direction != 0){
                    _levelChips[i].Chip.GetComponentInChildren<UIElementJolt>().Jolt(new Vector2(direction, 0));
                    _levelPreviewJolter.StopJolt();
                    _levelPreviewJolter.Jolt(new Vector2(-direction, 0));
                }
                
                _levelPreviewImage.GetComponent<ColourFader>().SetOnState(true);
                _levelPreviewImage.GetComponent<ColourFader>().SetFadeTimeT(0);

                LoadLevelPreview();
            }
            else
            {
                _levelChips[i].Chip.SetSelected(false);
            }
        }
        return true;
    }

    /// <summary>
    /// Loads the level represented by the selected chip, configures the camera with the correct settings to preview that level
    /// </summary>
    public void LoadLevelPreview()
    {
        LoadLevel(_selectedChip);

        HeadMovement playerHead = FindFirstObjectByType<HeadMovement>(FindObjectsInactive.Include);
        PlayerController playerMovement = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);

        var cameraFollow = Camera.main.GetComponentInParent<CameraFollow>();
        
        Camera.main.fieldOfView = playerMovement.CameraMinFov;

        cameraFollow.Target = LevelFullMap.Singleton.GetActiveLevel().CameraPreviewPosition;
        cameraFollow.TargetOffset = new Vector3(0, playerHead.GetCameraTargetYOffset(), 0);

        _colourFaderLevelSwitch.SetOnState(false);
        _colourFaderLevelSwitch.SetFadeTimeT(1);

        cameraFollow.SnapToTarget();

        OnLevelPreviewChange?.Invoke();
    }
}
