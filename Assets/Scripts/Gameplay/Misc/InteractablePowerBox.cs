using System;
using UnityEngine;

public class InteractablePowerBox : InteractableBase
{
    private BoxCollider _collider;
    private PowerBoxAttractSpot _attractSpot;
    
    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }


    public override void OnInteract(WorldInteractionDetector interactionDetector)
    {
        HeldInteractable.Singleton.SetInteractableAsHeld(this);
        transform.localRotation = Quaternion.identity;
        _collider.enabled = false;

        if (_attractSpot != null)
        {
            // Remove from slot
            _attractSpot.SetOccupiedByPowerBox(null);
        }
    }

    public void PlaceOnPlate(PowerBoxAttractSpot powerBoxAttractSpot)
    {
        _collider.enabled = true;
        powerBoxAttractSpot.SetOccupiedByPowerBox(this);
        _attractSpot = powerBoxAttractSpot;
    }

  
  
}