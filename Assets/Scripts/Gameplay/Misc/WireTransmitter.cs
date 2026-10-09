using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script to manage power running through a wire
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MaterialOverrides))]
public class WireTransmitterListener : PressurePlateListener
{
    
    /// <summary>
    /// Should the base leak effect be removed, set to true if we want the wire to be a uniform colour
    /// </summary>
    [SerializeField] private bool RemoveBaseLeakEffect;
    
    /// <summary>
    /// Invokes a specific function when the power finishes traveling through the fire
    /// </summary>
    [SerializeField] private UnityEvent InvokeOnTransmit;
    
    /// <summary>
    /// Used to change the speed at which 
    /// </summary>
    [SerializeField] private float FillSpeedMultiplier;
    
    private MaterialOverrides _materialOverrides;
    private MeshRenderer _wireMesh;
    
    /// <summary>
    /// The result of the incoming pressure plate state, should the wire be filled or turned off?
    /// </summary>
    private bool _wireActive;
    private float _filledAmountTracked;

    private void Awake()
    {
        _materialOverrides = GetComponent<MaterialOverrides>();
        _wireMesh = GetComponent<MeshRenderer>();
        
        _filledAmountTracked = 0;
        _wireActive = false;    
    }

    private void Start()
    {
        if (RemoveBaseLeakEffect)
        {
            // Set base leak to 0 to essentially hide it
            _materialOverrides.AssignFloatOverride("_BaseLeak", 0);
        }
    }

    public override void OnSwitchState(bool pressurePlateState)
    {
        _wireActive = pressurePlateState;
    }

    public void Update()
    {
        if (_wireActive)
        {
            bool lessThan1 = _filledAmountTracked < 1;
            
            _filledAmountTracked += Time.deltaTime;

            if (lessThan1 && _filledAmountTracked >= 1.0f)
            {
                // We have just filled the wire, invoke the transmit function
                InvokeOnTransmit?.Invoke();
            }
            _filledAmountTracked = Mathf.Clamp(_filledAmountTracked, 0f, 1f);
            
        }
        else
        {
            _filledAmountTracked -= Time.deltaTime;
            _filledAmountTracked = Mathf.Clamp(_filledAmountTracked, 0f, 1f);
        } 
        
        // Assign the material override to determine how filled the wire is 
        _materialOverrides.AssignFloatOverride("_Blend", _filledAmountTracked);
    }
}
