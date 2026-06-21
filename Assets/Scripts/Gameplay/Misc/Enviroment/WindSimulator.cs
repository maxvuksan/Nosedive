using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Creates fake wind direction and magnitude, this can be used to drive other scripts 
/// </summary>

public class WindSimulator : MonoBehaviour
{
    [System.Serializable]
    public class WindWave
    {
        public float Frequency = 1f;    // How fast this wave oscillates
        public float Amplitude = 1f;    // How strong this wave's influence is
        public Vector2 Direction = Vector2.right; // Wind direction for this wave
        [HideInInspector] public float Phase; // Random starting offset
    }

    [Header("Wind Waves")]
    [SerializeField] private WindWave[] _windWaves = new WindWave[]
    {
        new WindWave { Frequency = 0.5f, Amplitude = 1f, Direction = new Vector2(1, 0) },
        new WindWave { Frequency = 1.2f, Amplitude = 0.6f, Direction = new Vector2(0.7f, 0.3f) },
        new WindWave { Frequency = 2.5f, Amplitude = 0.3f, Direction = new Vector2(-0.2f, 0.8f) }
    };

    [Header("Wind Properties")]
    [SerializeField] private Vector2 _baseWindDirection = new Vector2(1, 0);
    [SerializeField] private float _baseWindStrength = 2f;
    [SerializeField] private float _gustStrength = 5f;

    // Current wind state (read by other scripts)
    public static WindSimulator Singleton;
    public Vector3 CurrentWindForce { get; private set; }
    public float CurrentWindMagnitude { get; private set; }

    private float _time;

    void Awake()
    {
        Helpers.CreateSingleton<WindSimulator>(ref Singleton, this);

        // Randomize phase for each wave so they don't all sync up
        foreach (var wave in _windWaves)
        {
            wave.Phase = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void Update()
    {
        _time += Time.deltaTime;
        
        // Start with base wind
        Vector2 wind = _baseWindDirection.normalized * _baseWindStrength;

        // Add each sine wave
        float gustFactor = 0f;
        foreach (var wave in _windWaves)
        {
            float sineValue = Mathf.Sin(_time * wave.Frequency + wave.Phase);
            
            // Accumulate gust strength (0 to 1 range)
            gustFactor += (sineValue + 1f) * 0.5f * wave.Amplitude;
            
            // Add directional component
            wind += wave.Direction.normalized * sineValue * wave.Amplitude;
        }

        // Normalize gust factor
        gustFactor /= _windWaves.Length;

        // Apply gust multiplier to final wind
        Vector2 finalWind = wind * (1f + gustFactor * _gustStrength);

        // Convert to 3D (assuming XZ plane is ground)
        CurrentWindForce = new Vector3(finalWind.x, 0, finalWind.y);
        CurrentWindMagnitude = CurrentWindForce.magnitude;
    }

    // Visualize wind in editor
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position;
        Gizmos.DrawRay(origin, CurrentWindForce);
        Gizmos.DrawWireSphere(origin + CurrentWindForce, 0.2f);
    }
}