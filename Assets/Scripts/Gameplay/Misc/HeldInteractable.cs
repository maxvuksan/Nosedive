using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Interface for what the player is currently holding
/// </summary>
public class HeldInteractable : MonoBehaviour
{
    /// <summary>
    /// Transform to place the held item into
    /// </summary>
    [SerializeField] private Transform _heldPositioningTransform;

    [SerializeField] private CameraFollow _heldInteractableCameraFollow;
    
    public InteractablePowerBox PowerBox => _powerBox;
    private InteractablePowerBox _powerBox;
    
    private bool _preBoxAttractCalculations = false;
    private float _snappedZDegrees;
    private float _snappedYDegrees;
    private float _snappedXDegrees;
    
    [CanBeNull] public PowerBoxAttractSpot PowerBoxAttractSpot;
    
    /// <summary>
    /// How far we should hold interactable from the player
    /// </summary>
    [SerializeField] private float _distanceFromPlayer;

    public static HeldInteractable Singleton;

    private void Awake()
    {
        Helpers.CreateSingleton(ref Singleton, this);
    }

    public void Start()
    {
        _heldPositioningTransform.localPosition = Vector3.forward * _distanceFromPlayer; 
        _heldInteractableCameraFollow.Target = _heldPositioningTransform;
    }

    public void SetInteractableAsHeld(InteractablePowerBox interactable)
    {
        _powerBox = interactable;
    }

    void Update()
    {
        if (_powerBox == null)
        {
            return;
        }
        
        if (InputManager.InteractInputOnPress())
        {
            if (PowerBoxAttractSpot != null)
            {
                _powerBox.PlaceOnPlate(PowerBoxAttractSpot);
                _powerBox = null;
            }
            
        }
            
    }
    
    void FixedUpdate()
    {
        if (_powerBox == null)
        {
            return;
        }
        
        // Switch between held box being in front of player and attracted to slot
        if (PowerBoxAttractSpot == null)
        {
            _heldInteractableCameraFollow.Source = _powerBox.transform;
            _preBoxAttractCalculations = false;
        }
        else
        {
            if (!_preBoxAttractCalculations)
            {
                _preBoxAttractCalculations = true;
                
                // Find nearest rotation rotational face (90 degree increment, apply this offset to the attract spot)
                // This is done so our cube doesn't do any strange rotations when being attracted to the new spot,
                // Visually it should look like the cube is simply being placed in the most logical way
                _snappedXDegrees = (Mathf.Round(_powerBox.transform.eulerAngles.x / 90.0f) * 90.0f ) % 360;
                _snappedYDegrees = (Mathf.Round(_powerBox.transform.eulerAngles.y / 90.0f) * 90.0f ) % 360;
                _snappedZDegrees = (Mathf.Round(_powerBox.transform.eulerAngles.z / 90.0f) * 90.0f ) % 360;
            }
            
            PowerBoxAttractSpot.CameraFollow.TargetRotationalOffset = Quaternion.Euler(
                -_snappedXDegrees + 90, 
                -_snappedYDegrees, 
                -_snappedZDegrees);
            
            PowerBoxAttractSpot.CameraFollow.Source = _powerBox.transform;
            _heldInteractableCameraFollow.Source = null;
        }
    }
    
    
}
