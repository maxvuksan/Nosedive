using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A visually interesting way to transmit events with a delay
/// </summary>
public class SignalTransmitter : PressurePlateListener
{
    /// <summary>
    /// Calls OnRecieveSignal on some other transmitter after some delay
    /// </summary>
    [SerializeField] private SignalTransmitter _otherTransmitterTrigger;

    /// <summary>
    /// Used to change the 2nd material slot (tip of the indicator)
    /// </summary>
    [SerializeField] private MeshRenderer[] _meshRenderers;

    /// <summary>
    /// A function to call when a signal is recieved
    /// </summary>
    [SerializeField] private UnityEvent _onSignalRecievedFunction;

    /// <summary>
    /// How long the transmitter waits before transmitting after the OnSwitchState call
    /// </summary>
    [SerializeField] private float _onPressurePlateDelaySeconds;

    /// <summary>
    /// How long the transmitter waits before acting on the OnRecieveSignal call
    /// </summary>
    [SerializeField] private float _onRecieveSignalDelaySeconds;

    private Coroutine onRecieveDelayCoroutine;
    private Coroutine onPressurePlateDelayCoroutine;

    private void Start() {

        Material[] currentMaterials = _meshRenderers[0].materials;

        if (_otherTransmitterTrigger != null)
        {
            currentMaterials[1] = Helpers.Singleton.MaterialEmissiveWhite; 
        }
        else
        {
            currentMaterials[1] = Helpers.Singleton.MaterialEmissiveDim; 
        }
        SetMeshMaterials(currentMaterials);
    }

    private void SetMeshMaterials(Material[] materials)
    {
        foreach(var mr in _meshRenderers)
        {
            mr.materials = materials;
        }
    }

    void OnEnable()
    {
        onRecieveDelayCoroutine = null;
        onPressurePlateDelayCoroutine = null;
    }

    void OnDisable()
    {
        if (onRecieveDelayCoroutine != null)
        {
            StopCoroutine(onRecieveDelayCoroutine);
            onRecieveDelayCoroutine = null; 
        }

        if (onPressurePlateDelayCoroutine != null)
        {
            StopCoroutine(onPressurePlateDelayCoroutine);
            onPressurePlateDelayCoroutine = null; 
        }
    }


    public void OnRecieveSignal()
    {
        onRecieveDelayCoroutine = StartCoroutine(OnRecieveSignal_Delayed());
    }


    public override void OnSwitchState(bool pressurePlateState)
    {
        print("hit plate");
        if (pressurePlateState)
        {
            onPressurePlateDelayCoroutine = StartCoroutine(OnSwitchState_Delayed());
        }
    }

    IEnumerator OnSwitchState_Delayed()
    {

        SetMaterialsAfterUse();

        yield return new WaitForSeconds(_onPressurePlateDelaySeconds);
        
        if(_otherTransmitterTrigger != null)
        {
            AudioManager.Singleton.Play("SignalTransmitter_Blip", transform.position);
            _otherTransmitterTrigger.OnRecieveSignal();
        }
    }

    IEnumerator OnRecieveSignal_Delayed()
    {
        SetMaterialsAfterUse();

        yield return new WaitForSeconds(_onRecieveSignalDelaySeconds);

        _onSignalRecievedFunction?.Invoke();   
    }

    private void SetMaterialsAfterUse()
    {
        Material[] currentMaterials = _meshRenderers[0].materials;

        if (_otherTransmitterTrigger != null)
        {
            currentMaterials[1] = Helpers.Singleton.MaterialEmissiveDim; 
        }
        else
        {
            currentMaterials[1] = Helpers.Singleton.MaterialEmissiveWhite; 
        }
        SetMeshMaterials(currentMaterials);
    }


}
