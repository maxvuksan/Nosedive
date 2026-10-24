using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// How many lights are required to complete the WireBox?
/// </summary>
public enum RequiredLightsToIlluminate
{
    Single,
    Double,
    Triple,
    Quad,
}

/// <summary>
/// A breaker style box that wires feed into, acts as a hub for multiple wire signals
/// Passes a success signal out when all the lights are illuminated
/// </summary>
public class WireBox : MonoBehaviour
{
    private class LightData
    {
        public bool On;
    }

    
    /// <summary>
    /// Is invoked when the WireBox has all its lights powered
    /// </summary>
    [Header("Callbacks")]
    [SerializeField] public UnityEvent InvokeOnComplete;
        
    /// <summary>
    /// Is invoked when the WireBox goes from fully powered to partially/not lit
    /// </summary>
    [SerializeField] public UnityEvent InvokeOnPowerLoss;

    /// <summary>
    /// How many lights are required to complete the WireBox?
    /// Changing this will dynamically change the mesh
    /// </summary>
    [Header("Lights")]
    [SerializeField] RequiredLightsToIlluminate _requiredLightsToIlluminate;

    /// <summary>
    /// Meshes to represent different arrangements of lights, this should be in the order of RequiredLightsToIlluminate enum
    /// </summary>
    [SerializeField] private Mesh[] _lightMeshes;
    [SerializeField] private MeshFilter _lightMeshFilter;
    [SerializeField] private MeshRenderer _lightMeshRenderer;

    /// <summary>
    /// Data for each light, keyed by the wire that is providing the power
    /// </summary>
    private List<LightData> _lightData;

    /// <summary>
    /// The previous completion state of the box,
    /// this is true if all the lights are on, false otherwise
    /// </summary>
    private bool _previousCompletionState = false;

    private void Awake()
    {
        ApplyInitialConfiguration();
        
        _previousCompletionState = false;

        // Populate initial state of light data
        _lightData = new();
        for (int i = 0; i <= (int)_requiredLightsToIlluminate; i++)
        {
            _lightData.Add(new LightData { On = false });
        }
    }

    private void ApplyInitialConfiguration()
    {
        _lightMeshFilter.mesh = _lightMeshes[(int)_requiredLightsToIlluminate];

        List<Material> lightMaterials = new();

        for (int i = 0; i <= (int)_requiredLightsToIlluminate; i++)
        {
            lightMaterials.Add(Helpers.Singleton.MaterialEmissiveDim);
        }
        _lightMeshRenderer.materials = lightMaterials.ToArray();
    }
    
    /// <summary>
    /// Evaluates if all the lights are powered,
    /// invokes OnComplete or OnPowerLoss if the completion state changes
    /// </summary>
    public void EvaluateIfComplete()
    {
        var newState = true;

        // Cache materials so we can reapply with changes
        Material[] currentMaterials = _lightMeshRenderer.materials;
        
        var i = 0;
        foreach (var light in _lightData)
        {
            if (!light.On)
            {
                currentMaterials[i] = Helpers.Singleton.MaterialEmissiveDim;
                newState = false;
            }
            else
            {
                currentMaterials[i] = Helpers.Singleton.MaterialEmissiveWhite;
            }
            i++;
        }
        
        // Reapply materials with changes
        _lightMeshRenderer.materials = currentMaterials;
        
        if (newState != _previousCompletionState)
        {
            _previousCompletionState = newState;

            // Switching to complete
            if (newState)
            {
                InvokeOnComplete?.Invoke();                
            }
            // Switching to incomplete 
            else
            {
                InvokeOnPowerLoss?.Invoke();                
            }
        }
    }    
    
    /// <summary>
    /// A specific wire is now providing power to the box
    /// </summary>
    public void ReceivePower(WireTransmitterListener wire)
    {
        // Play sound if light is not already on
        if (!_lightData[wire.WireBoxLightIndexTarget].On)
        {
            FmodAudioManager.PlayOneShot("WireBoxOn");
        }
        
        _lightData[wire.WireBoxLightIndexTarget].On = true;
        EvaluateIfComplete();
    }
    
    /// <summary>
    /// A specific wire no longer has power
    /// </summary>
    public void RemovePower(WireTransmitterListener wire)
    {
        _lightData[wire.WireBoxLightIndexTarget].On = false;
        EvaluateIfComplete();
    }
}
