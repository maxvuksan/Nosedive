using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Orchestrates the state of enviromental effects 
/// </summary>
public class EnviromentManager : MonoBehaviour
{
    [Header("Wind Settings")]

    [SerializeField] private float _fogMinDensity = 0.03f;
    [SerializeField] private float _fogMaxDensity = 0.05f;
    [Range(0, 1)]
    [SerializeField] private float _fogDensityLerpSpeed;
    private LoopingSound _windSound;


    [Header("Rain Settings")]

    [SerializeField] private float _rainHeightAbovePlayer;
    [SerializeField] private ParticleSystem _rainParticleSource;
    [SerializeField] private int _rainPerSecond = 1000;
    private LoopingSound _rainSound;
    
    [SerializeField] private float _lerpT;

    /// <summary>
    /// The level which we are pulling the enviromental state from
    /// </summary>
    private int _activeLevelIndex;

    /// <summary>
    /// The current state of the enviroment, is an interpolation of the state between the nearby levels
    /// </summary>
    public LevelEnviromentSettings EnviromentState { get => _enviromentState;}
    private LevelEnviromentSettings _enviromentState;
    public static EnviromentManager Singleton;
    private CameraFollow _playerCamera;
    private Camera _mainCamera;

    private void Awake()
    {
        Helpers.CreateSingleton(ref Singleton, this);
        
        _mainCamera = Camera.main;
        _playerCamera = FindFirstObjectByType<CameraFollow>(FindObjectsInactive.Include);
    }
    private void Start()
    {
        _enviromentState = LevelFullMap.Singleton.Levels[0].EnviromentSettings;
        ApplyEnviromentState();

    }

    private void OnEnable()
    {
        _windSound = LoopingAudioManager.Singleton.EnableLoop("WeatherWind");
        _rainSound = LoopingAudioManager.Singleton.EnableLoop("WeatherRain");
    }

    private void OnDisable()
    {
        LoopingAudioManager.Singleton.DisableLoop("WeatherWind");
        LoopingAudioManager.Singleton.DisableLoop("WeatherRain");
    }

    private void ApplyEnviromentState()
    {
        float windPercent = Mathf.Clamp01(WindSimulator.Singleton.CurrentWindMagnitude / 10f);
        float windT = 0.5f + (windPercent - 0.5f) / 2.0f * _enviromentState.WindStrength;
        float targetDensity = Mathf.Lerp(_fogMinDensity, _fogMaxDensity, windT);
        
        _windSound.volumeScaler = _enviromentState.WindStrength;
        _rainSound.volumeScaler = _enviromentState.RainStrength;

        RenderSettings.fogDensity = Mathf.Lerp(
            RenderSettings.fogDensity, 
            targetDensity, 
            _fogDensityLerpSpeed
        );

        RenderSettings.fogColor = _enviromentState.FogColour;
        _mainCamera.backgroundColor = _enviromentState.BackgroundColour;

        var emission = _rainParticleSource.emission;
        emission.rateOverTime = Mathf.Lerp(0, _rainPerSecond, _enviromentState.RainStrength);
    }

    private void Update() 
    {
        // update rain position to above camera
        _rainParticleSource.transform.position = _playerCamera.transform.position + new Vector3(0, _rainHeightAbovePlayer, 0);

        SetLerpEnviromentState();
        ApplyEnviromentState();
    }

    private void SetLerpEnviromentState()
    {
        // Dynamically find the correct segment based on world position
        UpdateActiveLevelIndexByZAxis();

        _lerpT = CalculateLerpTFromProjectedPlayerPosition();
        int nextIndex = _activeLevelIndex + 1;

        // Handle end of the map safely
        if (nextIndex >= LevelFullMap.Singleton.Levels.Length)
        {
            _enviromentState = LevelFullMap.Singleton.Levels[_activeLevelIndex].EnviromentSettings;
            return;
        }

        LevelEnviromentSettings stateCurrent = LevelFullMap.Singleton.Levels[_activeLevelIndex].EnviromentSettings;
        LevelEnviromentSettings stateNext = LevelFullMap.Singleton.Levels[nextIndex].EnviromentSettings;

        // TODO: Cache these or use a temporary struct instead of overwriting 
        _enviromentState.BackgroundColour = Color.Lerp(stateCurrent.BackgroundColour, stateNext.BackgroundColour, _lerpT);
        _enviromentState.FogColour = Color.Lerp(stateCurrent.FogColour, stateNext.FogColour, _lerpT);
        _enviromentState.WindStrength = Mathf.Lerp(stateCurrent.WindStrength, stateNext.WindStrength, _lerpT);
        _enviromentState.RainStrength = Mathf.Lerp(stateCurrent.RainStrength, stateNext.RainStrength, _lerpT);
        _enviromentState.DeathZoneHeight = Mathf.Lerp(stateCurrent.DeathZoneHeight, stateNext.DeathZoneHeight, _lerpT);
    }


    /// <summary>
    /// Projects the players position on the axis between the curret and next spawn point, this allows us to calculate the lerp T value between these points
    /// </summary>
    private float CalculateLerpTFromProjectedPlayerPosition()
    {
        int nextIndex = _activeLevelIndex + 1;
        if(nextIndex >= LevelFullMap.Singleton.Levels.Length)
        {
            return 0;
        }

        Vector3 pCurrent = LevelFullMap.Singleton.Levels[_activeLevelIndex].PlayerSpawn.position;        
        Vector3 pNext = LevelFullMap.Singleton.Levels[nextIndex].PlayerSpawn.position;        

        float lerpT = Mathf.InverseLerp(pCurrent.z, pNext.z, _playerCamera.transform.position.z);

        return lerpT;
    }

    private void UpdateActiveLevelIndexByZAxis()
    {
        int totalLevels = LevelFullMap.Singleton.Levels.Length;

        if (totalLevels < 2) {
            return;
        }

        Vector3 playerPos = _playerCamera.transform.position;
        

        // We check the current segment, and the next segment to see if the player has transitioned
        // This allows seamless backward and forward movement across triggers
        for (int i = 0; i < totalLevels; i++)
        {
            if (!LevelFullMap.Singleton.Levels[i].enabled)
            {
                continue;
            }

            Vector3 pCurrent = LevelFullMap.Singleton.Levels[i].PlayerSpawn.position;       
             
            // Last level: no next level exists
            if (i == totalLevels - 1)
            {
                if (playerPos.z < pCurrent.z)
                {
                    _activeLevelIndex = i;
                    break;
                }

                continue;
            }

            Vector3 pNext = LevelFullMap.Singleton.Levels[i + 1].PlayerSpawn.position;

            if (playerPos.z <= pCurrent.z && playerPos.z > pNext.z)
            {
                _activeLevelIndex = i;
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        LevelFullMap levelFullMap = FindFirstObjectByType<LevelFullMap>();

        Gizmos.color = Color.yellow;
        for(int i = 0; i < levelFullMap.Levels.Length - 1; i++)
        {
            Gizmos.DrawLine(levelFullMap.Levels[i].PlayerSpawn.transform.position, levelFullMap.Levels[i + 1].PlayerSpawn.transform.position);
        }
        
        if(LevelFullMap.Singleton == null)
        {
            return;            
        }

        int nextIndex = _activeLevelIndex + 1;
        if(nextIndex >= LevelFullMap.Singleton.Levels.Length)
        {
            return;
        }

        Vector3 pCurrent = LevelFullMap.Singleton.Levels[_activeLevelIndex].PlayerSpawn.position;        
        Vector3 pNext = LevelFullMap.Singleton.Levels[nextIndex].PlayerSpawn.position;        

        Gizmos.color = Color.magenta;   
        Gizmos.DrawWireSphere(Vector3.Lerp(pCurrent, pNext, _lerpT), 10.0f);

        // player death zone level...
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 deathBounds = new Vector3(5000, 1, 5000);
        Vector3 deathOrigin = new Vector3(transform.position.x, _enviromentState.DeathZoneHeight, transform.position.z);
        Gizmos.DrawCube(deathOrigin, deathBounds);
    }
}
