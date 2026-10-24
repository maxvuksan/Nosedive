using System;
using UnityEngine;

/// <summary>
/// A puzzle element for charging PowerBoxes
/// </summary>
public class PowerBoxCharger : PressurePlateListener
{
 
    private PowerBoxAttractSpot _attractSpot;
    private PressurePlate _pressurePlate;

    private void Awake()
    {
        _attractSpot = GetComponentInChildren<PowerBoxAttractSpot>();
        _pressurePlate = GetComponentInChildren<PressurePlate>();
    }

    public override void OnSwitchState(bool pressurePlateState)
    {
        TransmitPower();
    }
    
    /// <summary>
    /// Attempts to transmit power to the attached PowerBox
    /// Returns true if there is a attached box that can be powered
    /// </summary>
    public void TransmitPower()
    {
        if (_attractSpot.AttachedPowerBox == null)
        {
            return;
        }

        _attractSpot.AttachedPowerBox.Powered = true;
    }
}
