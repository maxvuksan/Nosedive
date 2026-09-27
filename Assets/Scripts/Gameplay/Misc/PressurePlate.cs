using NUnit.Framework.Constraints;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{

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

    private void Awake()
    {
        _materialOverrides = GetComponent<MaterialOverrides>();
    }

    private void OnEnable() {
        
        _onState = false;
        _animator.SetBool("Pressed", false);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.GetComponent<PlayerController>() == null)
        {
            return;
        }

        SetOnState(true);
    }

    private void OnTriggerExit(Collider other) 
    {
        if(other.GetComponent<PlayerController>() == null)
        {
            return;
        }

        SetOnState(false);
    }

    private void SetOnState(bool state)
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

        if (state)
        {
            _materialOverrides.AssignFloatOverride("_Intensity", 0.0f);
            _animator.SetBool("Pressed", true);
            AudioManager.Singleton.Play("PressurePlate_SwitchDown");
        }
        else
        {
            _materialOverrides.AssignFloatOverride("_Intensity", 0.0f);
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

}
