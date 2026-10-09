using NUnit.Framework.Constraints;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{

    public enum PressurePlateType
    {
        TriggeredByPlayer,
        TriggeredByPowerBox,
    }

    [SerializeField] public PressurePlateType _pressurePlateType;
    [SerializeField] public MeshRenderer _mesh;
    [SerializeField] public PressurePlateListener[] _listeners;
    [SerializeField] private Animator _animator;
    [SerializeField] private bool _staysTriggered;

    /// <summary>
    /// The index provided to the listener, allows listeners to tell pressure plates apart
    /// </summary>
    [SerializeField] public int _plateIndex; 
    private bool _onState = false;
    private MaterialOverrides _materialOverrides;
    
    private float _intensityOnState;
    private float _intensityOffState;

    private void Awake()
    {
        if (_pressurePlateType == PressurePlateType.TriggeredByPlayer)
        {
            _intensityOnState = 0.0f;
            _intensityOffState = 1.0f;
        }
        else if (_pressurePlateType == PressurePlateType.TriggeredByPowerBox)
        {
            _intensityOnState = 1.0f;
            _intensityOffState = 0.0f;
        }

        _materialOverrides = GetComponent<MaterialOverrides>();
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
            AudioManager.Singleton.Play("PressurePlate_SwitchDown");
        }
        else
        {
            AudioManager.Singleton.Play("PressurePlate_SwitchUp");
        }

        foreach(var listener in _listeners){
            
            if(listener == null){
                continue;
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
