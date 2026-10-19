using FMOD.Studio;
using UnityEngine;

/// <summary>
/// Controls the breathing sounds the player makes
/// </summary>
public class PlayerBreathing : MonoBehaviour
{
    [Header("Gasps")]
    [Range(0,1)]
    [SerializeField] private float _minGaspVolume;    
   
    [Range(0,1)]
    [SerializeField] private float _gaspChance;
    [SerializeField] private float _minSecondsBetweenGasps;
    [Header("Breathing")]
    [SerializeField] private float _idleBreathingMinVelocity;
    [SerializeField] private float _idleBreathingMaxVelocity;
    [SerializeField] private float _idleBreathingGreaterThanMaxDecreaseRate;
    [SerializeField] private float _idleBreathingLessThanMinDecreaseRate;
    [SerializeField] private float _idleBreathingIncreaseRate;

    private float _lastGaspTime;
    private EventInstance _idleBreathingSound;
    private Rigidbody _rigidBody;
    private PlayerController _playerController;
    private float _idleBreathingVolume;

    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _rigidBody = GetComponent<Rigidbody>();
        
        _idleBreathingSound = FmodAudioManager.PlayLoop("BreathingIdleLoop");
        _idleBreathingSound.setParameterByName("Intensity", 0, true);
        
        _idleBreathingVolume = 0;
    }

    void OnDestroy()
    {
        FmodAudioManager.StopLoop("BreathingIdleLoop");        
    }

    public void Update()
    {
        float velocityScale = _rigidBody.linearVelocity.magnitude;

        if (_playerController.Grounded)
        {   
            if(velocityScale < _idleBreathingMinVelocity)
            {
                // Decrease because we are moving too slow 
                _idleBreathingVolume -= _idleBreathingLessThanMinDecreaseRate * Time.deltaTime;

            }
            else
            {
                // Increase volume because we are within idle bounds
                _idleBreathingVolume += _idleBreathingIncreaseRate * Time.deltaTime;
            }
        }
        else
        {
            _idleBreathingVolume -= _idleBreathingIncreaseRate * Time.deltaTime;
        }

        float velocityVolumeMask = (Mathf.SmoothStep(_idleBreathingMaxVelocity, _idleBreathingMaxVelocity + _idleBreathingMinVelocity, 
                                   Mathf.InverseLerp(_idleBreathingMaxVelocity, _idleBreathingMaxVelocity + _idleBreathingMinVelocity, velocityScale)) 
                                   - _idleBreathingMaxVelocity) / _idleBreathingMinVelocity;

        velocityVolumeMask = 1.0f - velocityVolumeMask;

        _idleBreathingVolume = Mathf.Clamp01(_idleBreathingVolume);
        float volume = velocityVolumeMask * _idleBreathingVolume;

        _idleBreathingSound.setParameterByName("Intensity", _idleBreathingVolume);
    }



    public void PlaySound_Gasp()
    {
        if(Time.time - _lastGaspTime < _minSecondsBetweenGasps)
        {
            return;
        }

        // Random chance to not gasp
        if(_gaspChance < Random.Range(0,1))
        {
            return;
        }

        float randomVolume = Random.Range(_minGaspVolume, 1);
        FmodAudioManager.PlayOneShot("BreathingGasp", Vector3.zero, randomVolume);
    
        _lastGaspTime = Time.time;
    }
}
