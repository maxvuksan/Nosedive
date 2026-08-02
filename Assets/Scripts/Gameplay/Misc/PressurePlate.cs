using NUnit.Framework.Constraints;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{

    [SerializeField] public Material _offMaterial;
    [SerializeField] public Material _onMaterial;
    [SerializeField] public MeshRenderer _mesh;
    [SerializeField] public PressurePlateListener[] _listeners;
    [SerializeField] private Animator _animator;
    [SerializeField] private bool _staysTriggered;

    /// <summary>
    /// The index provided to the listener, allows listeners to tell pressure plates apart
    /// </summary>
    [SerializeField] public int _plateIndex; 
    private bool _onState = false;
    private bool _shouldTurnOff = false;

    private void OnEnable() {
        
        _mesh.material = _offMaterial;
        _animator.SetBool("Pressed", false);
    }

    private void OnTriggerStay(Collider other) 
    {
        if(other.GetComponent<SimpleWalker>() == null)
        {
            return;
        }


        SetOnState(true);
        _shouldTurnOff = false;
    }

    void FixedUpdate()
    {
        if (_shouldTurnOff)
        {
           SetOnState(false);
        }

        _shouldTurnOff = true;
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

        if (state)
        {
            _mesh.material = _onMaterial;
            _animator.SetBool("Pressed", true);
            AudioManager.Singleton.Play("PressurePlate_Switch");
        }
        else
        {
            _mesh.material = _offMaterial;
        }

        _onState = state;

        foreach(var listener in _listeners){
            listener.OnSwitchState(_onState);
            listener.OnSwitchState(_onState, _plateIndex);
        }
    }

}
