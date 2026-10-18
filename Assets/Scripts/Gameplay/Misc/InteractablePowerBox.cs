using System;
using UnityEngine;

public class InteractablePowerBox : InteractableBase
{
    
    /// <summary>
    /// Is the box powered?
    /// </summary>
    public bool Powered
    {
        get => _powered; 
        set => SetPowerState(value);
    }
    
    /// <summary>
    /// Determines whether this box has power initially
    /// </summary>
    [SerializeField] private bool _powered;
    
    private BoxCollider _collider;
    private Rigidbody _rigidbody;
    private PowerBoxAttractSpot _attractSpot;
    private Transform _previousParent;
    private MeshRenderer _renderer;
    
    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
        Release();

        SetPowerState(_powered);
    }

    private void Start()
    {
        _previousParent = transform.parent;
    }

    public override void OnInteract(WorldInteractionDetector interactionDetector)
    {
        HeldInteractable.Singleton.SetInteractableAsHeld(this);
        transform.localRotation = Quaternion.identity;
        
        _collider.enabled = false;
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = false;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
        
        FmodAudioManager.PlayOneShot("PowerBoxPickup");

        if (_attractSpot != null)
        {
            // Remove from slot
            _attractSpot.SetOccupiedByPowerBox(null);
        }
    }

    public void PlaceOnPlate(PowerBoxAttractSpot powerBoxAttractSpot)
    {
        _collider.enabled = true;
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = true;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
        
        transform.parent = powerBoxAttractSpot?.transform;
        powerBoxAttractSpot.SetOccupiedByPowerBox(this);
        _attractSpot = powerBoxAttractSpot;
    }

    public void Release()
    {
        transform.parent = _previousParent;
        _collider.enabled = true;
        _rigidbody.isKinematic = false;
        _rigidbody.detectCollisions = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        
        FmodAudioManager.PlayOneShot("PowerBoxPutdown");
    }

    private void SetPowerState(bool powered)
    {
        _powered = powered; 
        
        Material[] mats = _renderer.materials; 

        // Change the specific index in the copy
        mats[0] = powered ? Helpers.Singleton.MaterialEmissiveWhite : Helpers.Singleton.MaterialEmissiveDim;

        // Assign the modified array back to the renderer
        _renderer.materials = mats; 
    }
  
  
}