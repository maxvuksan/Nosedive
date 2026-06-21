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
    
    /// <summary>
    /// How fast the camera smoothly moves towards the target position
    /// </summary>
    public float PositionSmoothTime = 0.1f;

    /// <summary>
    /// How fast the camera smoothly rotates towards the target rotation
    /// </summary>
    public float RotationSmoothTime = 0.1f;

    private Vector3 _positionVelocity;


    void Update()
    {
        if (Target == null) return;

        transform.position = Vector3.SmoothDamp(transform.position, Target.position + TargetOffset, ref _positionVelocity, PositionSmoothTime);

        Quaternion targetRotation = Target.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-RotationSmoothTime * Time.deltaTime));
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