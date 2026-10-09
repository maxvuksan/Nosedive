using System;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(CameraFollow))]
public class PowerBoxAttractSpot : MonoBehaviour
{
    private PressurePlate _pressurePlate;
    private CameraFollow _cameraFollow;
    private BoxCollider _boxCollider;
    [CanBeNull] private InteractablePowerBox _attachedPowerBox;

    /// <summary>
    /// The offset to apply to the CameraFollow, this changes depending on if the attract spot is filled or not
    /// </summary>
    private float _yPositiomOffset;
    
    [SerializeField] private float _yOffsetSize;
    
    public CameraFollow CameraFollow => _cameraFollow;

    private void Awake()
    {
        WorldInteractionDetector.OnPostInteractionSearch += OnPostInteractionSearch;
        
        _boxCollider = GetComponent<BoxCollider>();
        _pressurePlate = GetComponentInParent<PressurePlate>();

        if (_pressurePlate == null)
        {
            Debug.LogWarning("_pressurePlate is null, this PowerBoxAttractSpot, must be a child of a pressure plate");
            gameObject.SetActive(false);
            return;
        }
        
        _cameraFollow = GetComponent<CameraFollow>();
    }

    private void Start()
    {
        SetOccupiedByPowerBox(null);
    }

    private void OnDestroy()
    {
        WorldInteractionDetector.OnPostInteractionSearch -= OnPostInteractionSearch;
    }

    private void OnPostInteractionSearch()
    {
        if (_attachedPowerBox == null)
        {
            _cameraFollow.Source = null;
        }
    }

    public void SetOccupiedByPowerBox([CanBeNull] InteractablePowerBox powerBox)
    {
        _attachedPowerBox = powerBox;
        CameraFollow.Source = powerBox?.transform;
        
        // Make box shift down when locked in
        if (powerBox != null)
        {
            // Turn off collider if a box is present, this is to prevent the attract spot from block raycasts
            _boxCollider.enabled = false;
            _pressurePlate.SetOnState(true);
            _yPositiomOffset = 0;
        }
        else
        {
            _boxCollider.enabled = true;
            _pressurePlate.SetOnState(false);
            _yPositiomOffset = _yOffsetSize;
        }
        CameraFollow.TargetOffset.y = _yPositiomOffset;
    }
}
