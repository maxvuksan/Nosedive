using UnityEngine;

/// <summary>
/// Provides functionality for an object to smoothly mimic the another objects transform, the intended use case is a camera following a player
/// </summary>
public class CameraFollow : MonoBehaviour
{
    /// <summary>
    /// The target we wish to follow
    /// </summary>
    public Transform Target;

    /// <summary>
    /// The offset to apply to the target
    /// </summary>
    public Vector3 TargetOffset = new(0,0,0);
    
    [Tooltip("Time in seconds to reach the target position.")]
    public float PositionSmoothTime = 0.025f;

    [Tooltip("Speed multiplier for rotation. Higher = faster tracking.")]
    public float RotationSmoothSpeed = 5.0f;

    private Vector3 _positionVelocity;


    void LateUpdate()
    {
        if (Target == null) return;

        // 1. Position Smoothing (SmoothDamp is excellent, keep this!)
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            Target.position + TargetOffset, 
            ref _positionVelocity, 
            PositionSmoothTime
        );

        // 2. Corrected Rotation Smoothing
        // Using a higher speed factor makes the exponential decay responsive.
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            Target.rotation, 
            1f - Mathf.Exp(-RotationSmoothSpeed * Time.deltaTime)
        );
    }


    /// <summary>
    /// Telports the camera position to Target.position + TargetOffset 
    /// </summary>
    public void SnapToTarget()
    {
        transform.position = Target.transform.position + TargetOffset;
        transform.rotation = Target.transform.rotation;
    }
}