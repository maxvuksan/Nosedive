using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Provides functionality for an object to smoothly mimic the another objects transform, the intended use case is a camera following a player
/// </summary>
public class CameraFollow : MonoBehaviour
{
    
    /// <summary>
    /// The source to do the following.
    /// </summary>
    [CanBeNull] public Transform Source;
    
    /// <summary>
    /// The target we wish to follow
    /// </summary>
    [CanBeNull] public Transform Target;

    /// <summary>
    /// The rotational offset the source is from the target
    /// </summary>
    public Quaternion TargetRotationalOffset = Quaternion.identity;
    
    /// <summary>
    /// The positional offset to apply to the target
    /// </summary>
    public Vector3 TargetOffset = new(0,0,0);
    
    [Tooltip("Time in seconds to reach the target position.")]
    public float PositionSmoothTime = 0.025f;

    [Tooltip("Speed multiplier for rotation. Higher = faster tracking.")]
    public float RotationSmoothSpeed = 5.0f;


    public Quaternion DesiredRotation
    {
        get => Target.rotation * TargetRotationalOffset;
    }

    public Vector3 DesiredPosition
    {
        get => Target.position + TargetOffset;
    }
    
    
    private Vector3 _positionVelocity;



    void LateUpdate()
    {
        if (Target == null || Source == null)
        {
            return;
        }

        // Position Smoothing (SmoothDamp is excellent, keep this!)
        Source.position = Vector3.SmoothDamp(
            Source.position, 
            DesiredPosition, 
            ref _positionVelocity, 
            PositionSmoothTime
        );

        // Corrected Rotation Smoothing
        // Using a higher speed factor makes the exponential decay responsive.
        Source.rotation = Quaternion.Slerp(
            Source.rotation, 
            // Note: This is intentionally multiply, this is how we rotate one quaternion by another
            DesiredRotation, 
            1f - Mathf.Exp(-RotationSmoothSpeed * Time.deltaTime)
        );
    }


    /// <summary>
    /// Teleports the camera position to Target.position + TargetOffset 
    /// </summary>
    public void SnapToTarget()
    {
        if (Target == null || Source == null)
        {
            return;
        }
        
        Source.position = Target.position + TargetOffset;
        Source.rotation = Target.rotation;
    }
}