using UnityEngine;

/// <summary>
/// A base class to inherit, provides a way to perform an action when a pressure plate triggers
/// </summary>
public class PressurePlateListener : MonoBehaviour
{

    /// <summary>
    /// Is called when the pressure plate goes from on->off or off->on
    /// </summary>
    /// <param name="pressurePlateState">The new state of the pressure plate</param>
    public virtual void OnSwitchState(bool pressurePlateState)
    {
        // implementation is within child class
    }

    /// <summary>
    /// Is called when the pressure plate goes from on->off or off->on
    /// </summary>
    /// <param name="pressurePlateState">The new state of the pressure plate</param>
    /// <param name="pressurePlateIndex">The index provided from the pressure plate, allows multiple pressure plates to influence a listener</param>
    public virtual void OnSwitchState(bool pressurePlateState, int pressurePlateIndex)
    {
        // implementation is within child class
    }

}