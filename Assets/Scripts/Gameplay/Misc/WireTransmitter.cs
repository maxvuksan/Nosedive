using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

/// <summary>
/// Script to manage power running through a wire
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MaterialOverrides))]
[RequireComponent(typeof(SplineContainer))]
public class WireTransmitterListener : PressurePlateListener
{
    /// <summary>
    /// Should the base leak effect be removed, set to true if we want the wire to be a uniform colour
    /// </summary>
    [SerializeField] private bool RemoveBaseLeakEffect;

    /// <summary>
    /// Shorthand for calling OnSwitchState for the next wire in the chain,
    /// this could also be done using InvokeOnTransmit or InvokeOnPowerLost
    /// </summary>
    [SerializeField] private WireTransmitterListener[] NextWireInChain;
    
    /// <summary>
    /// Invokes a specific function when the power finishes traveling through the fire
    /// </summary>
    [SerializeField] private UnityEvent InvokeOnTransmit;
    
    /// <summary>
    /// Invokes a specific function when the wire loses power
    /// </summary>
    [SerializeField] private UnityEvent InvokeOnPowerLost;

    /// <summary>
    /// If connected to a WireBox, what light index is this targeting?
    /// </summary>
    public int WireBoxLightIndexTarget = 0;
    
    private MaterialOverrides _materialOverrides;
    private MeshRenderer _wireMesh;
    
    /// <summary>
    /// The result of the incoming pressure plate state, should the wire be filled or turned off?
    /// </summary>
    private bool _wireActive;
    private float _filledAmountTracked;
    private float _wireLengthDivider;
    
    private float _underblendAmountTracked;
    private float _underblendOpacityTracked = 0;

    private void Awake()
    {
        _materialOverrides = GetComponent<MaterialOverrides>();
        _wireMesh = GetComponent<MeshRenderer>();

        // Scale transmit speed by this, to ensure all wire transmissions move same speed relative to the wire length
        _wireLengthDivider = 1 / GetComponent<SplineContainer>().CalculateLength();
        
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
        if (_wireActive && !pressurePlateState)
        {
            SuddenlyLosePower();
        }
        
        // Tell listeners we lost power
        InvokeOnPowerLost?.Invoke();
        
        _wireActive = pressurePlateState;
    }

    public void Update()
    {
        if (_wireActive)
        {
            bool lessThan1 = _filledAmountTracked < 1;
            
            _filledAmountTracked += Time.deltaTime * _wireLengthDivider * Helpers.Singleton.WireTransmitSpeed;

            if (lessThan1 && _filledAmountTracked >= 1.0f)
            {
                // We have just filled the wire, Tell listeners
                UpdateWiresInChain(true);
                InvokeOnTransmit?.Invoke();
            }
            _filledAmountTracked = Mathf.Clamp(_filledAmountTracked, 0f, 1f);
            
        }
        // TODO: I don't think i need this because sudden power loss would set fill to 0
        // else
        // {
        //     _filledAmountTracked -= Time.deltaTime;
        //     _filledAmountTracked = Mathf.Clamp(_filledAmountTracked, 0f, 1f);
        // }

        if (_underblendOpacityTracked > 0)
        {
            _underblendOpacityTracked -= Time.deltaTime * Helpers.Singleton.WireUnderBlendDisapateSpeed;
        }
        
        // Assign the material override to determine how filled the wire is 
        AssignMaterialOverrides();
    }
    
    private void SuddenlyLosePower()
    {
        // Apply current fill as under-blend
        
        _underblendAmountTracked = _filledAmountTracked;
        _underblendOpacityTracked = Helpers.Singleton.WireUnderBlendDisapateInitalOpacity;

        // The current is applied to the under-blend, remove from main fill
        _filledAmountTracked = 0;
        
        // Tell listeners we lost power
        UpdateWiresInChain(false);
        InvokeOnPowerLost?.Invoke();
    }

    private void UpdateWiresInChain(bool state)
    {
        foreach (var wire in NextWireInChain)
        {
            wire.OnSwitchState(state);
        }
    }

    private void AssignMaterialOverrides()
    {
        _materialOverrides.AssignColourOverride("_UnderColour", Color.Lerp(Helpers.Colours.WireOff, Helpers.Colours.WireOn, _underblendOpacityTracked));
        _materialOverrides.AssignFloatOverride("_UnderBlend", _underblendAmountTracked);
        _materialOverrides.AssignFloatOverride("_Blend", _filledAmountTracked);
    }
}
