using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;



/// <summary>
/// Data relating to the pulse animation which propogates through the fog volume
/// </summary>
[System.Serializable]
public class CollectablePulse
{
    public float RadiusInital;
    public float RadiusIncreaseRateInital;
    public float RadiusIncreaseRateMax;
    [HideInInspector] public Color ColourInital;
    [HideInInspector] public Color ColourMax;
    public float BandSizeInital;
    public float BandSizeMax;
    public float FeatherInital;
    public float FeatherMax;
    [HideInInspector] public Vector3 OriginPoint;

    public float Duration;
    [HideInInspector] public float DurationTracked;
    [HideInInspector] public bool PulseCompleted = true;
}

/// <summary>
/// A set of colours to use for the collectable
/// </summary>
[System.Serializable]
public class CollectablePulseColours
{
    public Color ColourInital;
    public Color ColourMax;
    public Color FogColour;
}

/// <summary>
/// The games collectable spheres, increases in power when the connected pressure plate triggers, explodes/collects when full power reached
/// </summary>
public class CollectableSphere : PressurePlateListener
{
    [SerializeField] private int _sphereIndex = 0;
    [SerializeField] private Light[] _lightSources;
    [SerializeField] private float _minLightSourceIntensity;
    [SerializeField] private float _maxLightSourceIntensity;
    private static float s_currentRadius;
    private bool _active;
    private float _wireLerpT;
    private bool _lastPressurePlateState;
    

    private void Awake() 
    {
        _active = true;    
    }

    private void OnEnable()
    {
        if (!_active)
        {
            GetComponent<Animator>().SetTrigger("Off");
        }
    }

    /// <summary>
    /// Should be invoked by the pressure plate the player stands on 
    /// </summary>
    public override void OnSwitchState(bool pressurePlateState)
    {
        if (!_lastPressurePlateState && pressurePlateState)
        {
            _wireLerpT = 0;
        }
        _lastPressurePlateState = pressurePlateState;
    }

    /// <summary>
    /// Sets the intensity of every point light on the sphere
    /// </summary>
    void SetPointLightsIntensity(float intensity)
    {
        for(int i = 0; i < _lightSources.Length; i++)
        {
            _lightSources[i].intensity = intensity;
        }        
    }

    void Update()
    {
        if (!_lastPressurePlateState || !_active)
        {
            return;
        }

        EnviromentManager.Singleton.SetTemporaryCollectableWireColourOveride(Color.white);

        _wireLerpT += Time.deltaTime * EnviromentManager.Singleton.CollectableWireBlendIncreaseSpeed;
        _wireLerpT = Mathf.Clamp01(_wireLerpT);
        EnviromentManager.Singleton.CollectableWireBlendT = _wireLerpT;

        SetPointLightsIntensity(Mathf.Lerp(_minLightSourceIntensity, _maxLightSourceIntensity, _wireLerpT));

        if (_wireLerpT == 1)
        {
            _active = false;
            SetPointLightsIntensity(0);
            GetComponent<Animator>().SetTrigger("Pulse");
        }
    }

    /// <summary>
    /// Increments the state of the fog pulse animation
    /// </summary>
    public static void UpdateCurrentPulse(FogProfile fogProfile, CollectablePulse pulseData)
    {
        if (pulseData.PulseCompleted)
        {
            return;
        }

        if(pulseData.DurationTracked == 0)
        {
            s_currentRadius = pulseData.RadiusInital;
        }

        float lerpT = pulseData.DurationTracked / pulseData.Duration;

        lerpT = Mathf.Clamp01(lerpT);

        fogProfile.Data.HighlightRingOriginPosition = pulseData.OriginPoint;
        fogProfile.Data.HighlightRingColour = Color.Lerp(pulseData.ColourInital, pulseData.ColourMax, Helpers.EaseOutExponential(lerpT));
        fogProfile.Data.HighlightRingRadius = s_currentRadius; 
        fogProfile.Data.HighlightRingBandSize = Mathf.Lerp(pulseData.BandSizeInital, pulseData.BandSizeMax, lerpT);
        fogProfile.Data.HighlightRingFeather = Mathf.Lerp(pulseData.FeatherInital, pulseData.FeatherMax, lerpT);

        pulseData.DurationTracked += Time.deltaTime;

        float radiusIncreaseRate = Mathf.Lerp(pulseData.RadiusIncreaseRateInital, pulseData.RadiusIncreaseRateMax, lerpT);
        s_currentRadius += radiusIncreaseRate * Time.deltaTime;
        
        if(pulseData.DurationTracked >= pulseData.Duration)
        {
            pulseData.PulseCompleted = true;
        }
    }

    /// <summary>
    /// Begins the collection sphere fog pulse effect
    /// </summary>
    public void GeneratePulse()
    {
        EnviromentManager.Singleton.CollectableSpherePulse.OriginPoint = transform.position;
        EnviromentManager.Singleton.CollectableSpherePulse.DurationTracked = 0;
        EnviromentManager.Singleton.CollectableSpherePulse.ColourInital = EnviromentManager.Singleton.GetCollectablePulseColours(_sphereIndex).ColourInital;
        EnviromentManager.Singleton.CollectableSpherePulse.ColourMax = EnviromentManager.Singleton.GetCollectablePulseColours(_sphereIndex).ColourMax;
        EnviromentManager.Singleton.CollectableSpherePulse.PulseCompleted = false;
        EnviromentManager.Singleton.SetTemporarilyFogColourOverride(EnviromentManager.Singleton.GetCollectablePulseColours(_sphereIndex).FogColour);
        AudioManager.Singleton.Play("CollectableSphere_Pulse", transform.position);
    }
}
