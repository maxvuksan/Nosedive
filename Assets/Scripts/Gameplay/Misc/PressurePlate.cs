using UnityEngine;

public class PressurePlate : MonoBehaviour
{

    [SerializeField] public Material _offMaterial;
    [SerializeField] public Material _onMaterial;
    [SerializeField] public MeshRenderer _mesh;
    [SerializeField] public PressurePlateListener[] _listeners;
    /// <summary>
    /// The index provided to the listener, allows listeners to tell pressure plates apart
    /// </summary>
    [SerializeField] public int _plateIndex; 
    private bool _onState;
    private bool _shouldTurnOff = false;

    private void OnTriggerStay(Collider other) 
    {
        if(other.GetComponent<SimpleWalker>() == null)
        {
            return;
        }

        _mesh.material = _onMaterial;

        SetOnState(true);
        _shouldTurnOff = false;
    }

    void FixedUpdate()
    {
        if (_shouldTurnOff)
        {
            _mesh.material = _offMaterial;
           SetOnState(false);
        }

        _shouldTurnOff = true;
    }

    private void SetOnState(bool state)
    {
        if(state == _onState)
        {
            return;
        }

        _onState = state;

        foreach(var listener in _listeners){
            listener.OnSwitchState(_onState);
            listener.OnSwitchState(_onState, _plateIndex);
        }
    }

}
