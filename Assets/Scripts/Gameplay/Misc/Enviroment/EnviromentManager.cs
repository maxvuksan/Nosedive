using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

/// <summary>
/// Orchestrates the state of enviromental effects 
/// </summary>
public class EnviromentManager : MonoBehaviour
{


    [SerializeField] private Light _mainDirectionalLight;

    [SerializeField] private FogProfile _fogProfile;
    
    /// <summary>
    /// The ground fog sits at the current player death height, this offset shifts that fog
    /// </summary>
    [SerializeField] private int _fogGroundLayerOffset; 


    [Header("Cavity/Rim Light Settings")]
    [SerializeField] private Material _cavityMaterial;


    [Header("Rain Settings")]

    [SerializeField] private float _rainHeightAbovePlayer;
    [SerializeField] private ParticleSystem _rainParticleSource;
    [SerializeField] private int _rainPerSecond = 1000;

    private LoopingSound _rainSound;
    private LoopingSound _windSound;
    

    [Header("Collectable Sphere Settings")]
    public CollectablePulse CollectableSpherePulse;
    [SerializeField] private CollectablePulseColours[] _collectablePulseConfigurations;
    [HideInInspector] public float CollectableWireBlendT = 0;
    public float CollectableWireBlendIncreaseSpeed = 0.1f;
    [SerializeField] private Material _collectableWireBlendMaterial;
    [SerializeField] private Color _collectableWireOffColour;
    [SerializeField] private float _collectableWireIncreaseRate;
    private Color _collectableWireOverrideColour;
    private bool _collectableWireOverrideIsIncreasing;
    private float _collectableWireOverrideTrackedT = 0;
 
    // The fog override colour fades in (increase rate) then when reaches full intensity begins fading out (decrease rate)
    [SerializeField] private float _overrideFogColourIncreaseRate;
    [SerializeField] private float _overrideFogColourDecreaseRate;
    private float _overrideFogColourTrackedT = 0;
    private bool _overrideFogColourIncreasing = false;
    private Color _overrideFogColour;

    
    
    private float _lerpT;

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

    /// <summary>
    /// Gets the collectable pulse colour data set at a specific collectable index
    /// </summary>
    public CollectablePulseColours GetCollectablePulseColours(int index)
    {
        index %= _collectablePulseConfigurations.Length;
        return _collectablePulseConfigurations[index];
    }

    /// <summary>
    /// Temporarily overrides the colour of the fog, this override colour slowly fades back to the active colour
    /// </summary>
    public void SetTemporarilyFogColourOverride(Color fogOverrideColour)
    {
        _overrideFogColourTrackedT = 0;
        _overrideFogColour = fogOverrideColour;
        _overrideFogColourIncreasing = true;
    }

    /// <summary>
    /// Overrides the colour of the wire powered colour
    /// </summary>
    /// <param name="wireOverrideColour">The colour the wire should be</param>
    public void SetTemporaryCollectableWireColourOveride(Color wireOverrideColour)
    {
        _collectableWireOverrideColour = wireOverrideColour;
        _collectableWireOverrideIsIncreasing = true;
    }


    private void Awake()
    {
        Helpers.CreateSingleton(ref Singleton, this);
        
        _mainCamera = Camera.main;
        _playerCamera = FindFirstObjectByType<CameraFollow>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        _collectableWireBlendMaterial.SetColor("_OffColour", _collectableWireOffColour);

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
        _windSound.volumeScaler = _enviromentState.WindStrength;
        _rainSound.volumeScaler = _enviromentState.RainStrength;

        _mainCamera.backgroundColor = _enviromentState.FogColour;

        var emission = _rainParticleSource.emission;
        emission.rateOverTime = Mathf.Lerp(0, _rainPerSecond, _enviromentState.RainStrength);

        _cavityMaterial.SetFloat("_Opacity", _enviromentState.CavityLightingOpacity);

        // apply custom fog profile

        _fogProfile.Data.Density = _enviromentState.FogDensity;
        _fogProfile.Data.Colour = _enviromentState.FogColour;
        _fogProfile.Data.BlobNoiseIntensity = _enviromentState.FogBlobNoiseIntensity;
        _fogProfile.Data.GroundFogStartHeight = _enviromentState.DeathZoneHeight + _fogGroundLayerOffset;
        _fogProfile.Data.CameraPointLightStrength = _enviromentState.CameraLightSourceIntensity;
        _fogProfile.Data.CameraPointLightRadius = _enviromentState.CameraLightSourceRadius;
        _fogProfile.Data.CameraPointLightStrength = _enviromentState.CameraLightSourceIntensity;
        _mainDirectionalLight.intensity = _enviromentState.DirectionalLightSourceIntensity;
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

        _enviromentState.WindStrength = Mathf.Lerp(stateCurrent.WindStrength, stateNext.WindStrength, _lerpT);
        _enviromentState.RainStrength = Mathf.Lerp(stateCurrent.RainStrength, stateNext.RainStrength, _lerpT);
        _enviromentState.DeathZoneHeight = Mathf.Lerp(stateCurrent.DeathZoneHeight, stateNext.DeathZoneHeight, _lerpT);

        _enviromentState.FogDensity = Mathf.Lerp(stateCurrent.FogDensity, stateNext.FogDensity, _lerpT);
        _enviromentState.FogBlobNoiseIntensity = Mathf.Lerp(stateCurrent.FogBlobNoiseIntensity, stateNext.FogBlobNoiseIntensity, _lerpT);
        _enviromentState.CavityLightingOpacity = Mathf.Lerp(stateCurrent.CavityLightingOpacity, stateNext.CavityLightingOpacity, _lerpT);
        _enviromentState.CameraLightSourceIntensity = Mathf.Lerp(stateCurrent.CameraLightSourceIntensity, stateNext.CameraLightSourceIntensity, _lerpT);
        _enviromentState.CameraLightSourceRadius = Mathf.Lerp(stateCurrent.CameraLightSourceRadius, stateNext.CameraLightSourceRadius, _lerpT);
        _enviromentState.DirectionalLightSourceIntensity = Mathf.Lerp(stateCurrent.DirectionalLightSourceIntensity, stateNext.DirectionalLightSourceIntensity, _lerpT);

        CollectableSphere.UpdateCurrentPulse(_fogProfile, CollectableSpherePulse);

        if (_overrideFogColourIncreasing)
        {
            _overrideFogColourTrackedT += Time.deltaTime * _overrideFogColourIncreaseRate;

            if(_overrideFogColourTrackedT >= 1)
            {
                _overrideFogColourIncreasing = false;
            }
        }
        else
        {
            _overrideFogColourTrackedT -= Time.deltaTime * _overrideFogColourDecreaseRate;
        }
        _overrideFogColourTrackedT = Mathf.Clamp01(_overrideFogColourTrackedT);
        
        Color trueFogColour = Color.Lerp(stateCurrent.FogColour, stateNext.FogColour, _lerpT);
        _enviromentState.FogColour = Color.Lerp(trueFogColour, _overrideFogColour, _overrideFogColourTrackedT);


        if (_collectableWireOverrideIsIncreasing)
        {
            _collectableWireOverrideIsIncreasing = false;
            _collectableWireOverrideTrackedT += Time.deltaTime * _collectableWireIncreaseRate;

        }
        else
        {
            _collectableWireOverrideTrackedT -= Time.deltaTime * _collectableWireIncreaseRate * 2;
        }
        _collectableWireOverrideTrackedT = Mathf.Clamp01(_collectableWireOverrideTrackedT);

        _collectableWireBlendMaterial.SetFloat("_Blend", CollectableWireBlendT);

        _collectableWireBlendMaterial.SetColor("_OnColour", Color.Lerp(_collectableWireOffColour, _collectableWireOverrideColour, _collectableWireOverrideTrackedT));


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
