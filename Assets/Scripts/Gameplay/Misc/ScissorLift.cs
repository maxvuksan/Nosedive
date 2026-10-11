using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A single scissor segment on the lift
/// It is expected segments are structured as
///
/// - Segment
/// -   Beam
/// -   InnerBeam
///
/// In the scene hierarchy 
/// </summary>
public struct ScissorLiftExtensionSegment
{
    public Transform Parent;
    public Transform Beam;
    public Transform InnerBeam;
}

/// <summary>
/// A scissor lift, that retracts and extents
/// TODO: The scissor lift should have better spring behaviour, it should like overshoot and bounce to its positions
/// </summary>
public class ScissorLift : PressurePlateListener
{
    [SerializeField] private Transform _platform;
    [SerializeField] private GameObject[] _extensionSegmentGameObjects;
    
    [SerializeField] private float _maxRotation;
    [SerializeField] private float _maxTranslation;
    [SerializeField] private float _springTime;

    /// <summary>
    /// Remaps the 0-1 extensionAmount range to a min-1 range
    /// </summary>
    [Range(0,1)]
    [SerializeField] private float _extensionMinRemap;
    
    [Range(0,1)]
    [SerializeField] public float _extensionAmount;
    
    private float _currentExtensionAmount;
    private float _springVelocity;

    private List<ScissorLiftExtensionSegment> _extensionSegments;

    private void Start()
    {
        CalculateExtensionSegments();
        RecalculateExtension();
    }


    public override void OnSwitchState(bool pressurePlateState, int pressurePlateIndex)
    {
        if (!pressurePlateState)
        {
            return;
        }
        
        if (pressurePlateIndex == 0)
        {
            _extensionAmount = 1.0f;
        }
        else
        {
            _extensionAmount = 0.0f;
        }
    }


    private void CalculateExtensionSegments()
    {
        _extensionSegments = new();
        
        foreach (var segment in _extensionSegmentGameObjects)
        {
            ScissorLiftExtensionSegment extensionSegment;
            extensionSegment.Parent = segment.transform;
            extensionSegment.Beam = segment.transform.GetChild(0);
            extensionSegment.InnerBeam = segment.transform.GetChild(1);
            
            _extensionSegments.Add(extensionSegment);
        }
    }

    private void Update()
    {
        _currentExtensionAmount = Mathf.SmoothDamp(
            _currentExtensionAmount, 
            _extensionAmount, 
            ref _springVelocity, 
            _springTime
        );
        
        RecalculateExtension();
    }

    private void RecalculateExtension()
    {
        float remappedExtension = Mathf.Lerp(_extensionMinRemap, 1.0f, _currentExtensionAmount);
        
        float rotation = Mathf.Lerp(0, _maxRotation, remappedExtension);
        float translation = Mathf.Lerp(0, _maxTranslation, remappedExtension);
        float offset = translation / 2.0f;
        
        int i;
        for (i = 0; i < _extensionSegments.Count; i++)
        {
            _extensionSegments[i].Beam.transform.localRotation = Quaternion.Euler(0, rotation, 0);
            _extensionSegments[i].InnerBeam.transform.localRotation = Quaternion.Euler(0, -rotation, 0);
            
            _extensionSegments[i].Parent.transform.localPosition = new Vector3(0, 0, translation * i + offset);
        }
        
        _platform.transform.localPosition = new Vector3(0, 0, translation * i - (translation / 2.0f) + offset);
    }

}
