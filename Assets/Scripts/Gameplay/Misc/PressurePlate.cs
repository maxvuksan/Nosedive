using System;
using NaughtyAttributes;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Note:
/// </summary>
public class PressurePlate : PressurePlateListener
{
    public enum PressurePlateType
    {
        TriggeredByPlayer,
        TriggeredByPowerBox,
    }

    [Serializable]
    public struct TriggeredByPlayer
    {
        public MeshRenderer LightMesh;
        public BoxCollider PlayerOnStepTrigger;
    }

    [Serializable]
    public struct TriggeredByPowerBox
    {
        public MeshRenderer LightMesh;
        public PowerBoxAttractSpot AttractSpot;
    }

    [SerializeField] public PressurePlateType _pressurePlateType;

    [ShowIf(nameof(_pressurePlateType), PressurePlateType.TriggeredByPlayer)] 
    [SerializeField] private TriggeredByPlayer _triggeredByPlayer;
    
    [ShowIf(nameof(_pressurePlateType), PressurePlateType.TriggeredByPowerBox)] 
    [SerializeField] private TriggeredByPowerBox _triggeredByPowerBox;
    
    private MeshRenderer _mesh;
    
    [SerializeField] public PressurePlateListener[] _listeners;
    [SerializeField] private Animator _animator;
    [SerializeField] private bool _staysTriggered;
    
    /// <summary>
    /// The index provided to the listener, allows listeners to tell pressure plates apart
    /// </summary>
    [SerializeField] public int _plateIndex;
    
    public bool OnState => _onState;
    private bool _onState = false;
    
    private MaterialOverrides _materialOverrides;
    
    private float _intensityOnState;
    private float _intensityOffState;

    private void Awake()
    {
        if (_pressurePlateType == PressurePlateType.TriggeredByPlayer)
        {
            _mesh = _triggeredByPlayer.LightMesh;
             
            _intensityOnState = 0.0f;
            _intensityOffState = 1.0f;
        }
        else if (_pressurePlateType == PressurePlateType.TriggeredByPowerBox)
        {
            _mesh = _triggeredByPowerBox.LightMesh;
            _intensityOnState = 1.0f;
            _intensityOffState = 0.0f;
        }
        
        // Note: I was getting a warning about doing this in OnValidate, so have moved out of ConfigurePlateByType
        _triggeredByPlayer.PlayerOnStepTrigger.enabled = _pressurePlateType == PressurePlateType.TriggeredByPlayer;
        ConfigurePlateByType();
        
        _materialOverrides = GetComponent<MaterialOverrides>();
    }

    private void OnValidate()
    {
        #if UNITY_EDITOR
        EditorApplication.delayCall += () => 
        {
            ConfigurePlateByType();
        };
        #endif
    }

    private void ConfigurePlateByType()
    {
        if (_triggeredByPlayer.LightMesh != null)
        {
            _triggeredByPlayer.LightMesh.gameObject.SetActive(_pressurePlateType ==
                                                              PressurePlateType.TriggeredByPlayer);
        }
        if (_triggeredByPowerBox.LightMesh != null)
        {
            _triggeredByPowerBox.LightMesh.gameObject.SetActive(_pressurePlateType == PressurePlateType.TriggeredByPowerBox);
        }

        if (_triggeredByPowerBox.AttractSpot != null)
        {
            _triggeredByPowerBox.AttractSpot.gameObject.SetActive(_pressurePlateType == PressurePlateType.TriggeredByPowerBox);
        }
    }

    private void OnEnable() {
        
        _onState = false;
        _animator.SetBool("Pressed", false);
        UpdateColours(_onState);
    }

    private void OnTriggerEnter(Collider other) 
    {
        // Only trigger by player on contact
        if (_pressurePlateType != PressurePlateType.TriggeredByPlayer)
        {
            return;
        }
        
        if(other.GetComponent<PlayerController>() == null)
        {
            return;
        }

        SetOnState(true);
    }

    private void OnTriggerExit(Collider other) 
    {
        if (_pressurePlateType != PressurePlateType.TriggeredByPlayer)
        {
            return;
        }
        
        if(other.GetComponent<PlayerController>() == null)
        {
            return;
        }

        SetOnState(false);
    }

    public void SetOnState(bool state)
    {
        if(_onState && _staysTriggered)
        {
            return;
        }

        if(state == _onState)
        {
            return;
        }

        _onState = state;

        UpdateColours(state);
        
        if (state)
        {
            FmodAudioManager.PlayOneShot("PressurePlateDown");
        }
        else
        { 
            FmodAudioManager.PlayOneShot("PressurePlateUp");
        }

        foreach(var listener in _listeners){
            
            if(listener == null){
                continue;
            }

            // Don't allow infinite recursion
            if (listener.gameObject == this.gameObject)
            {
                Debug.LogWarning("A PressurePlate has been assigned as a listener on itself, this is not allowed");
                return;
            }
        
            listener.OnSwitchState(_onState);
            listener.OnSwitchState(_onState, _plateIndex);
        }
    }

    private void UpdateColours(bool state)
    {
        if (state)
        {
            _materialOverrides.AssignFloatOverride("_Intensity", _intensityOnState);

            if (_pressurePlateType == PressurePlateType.TriggeredByPlayer)
            {
                _animator.SetBool("Pressed", true);
            }
        }
        else
        {
            if (_pressurePlateType == PressurePlateType.TriggeredByPlayer)
            {
                _animator.SetBool("Pressed", false);
            }
            
            _materialOverrides.AssignFloatOverride("_Intensity", _intensityOffState);
        }
    }

}
