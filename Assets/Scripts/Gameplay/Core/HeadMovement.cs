using UnityEngine;

/// <summary>
/// The head movement of the player, rotates a child head transform on the vertical axis, and the attached GameObjects transform (the body) on the horizontal axis
/// </summary>
public class HeadMovement : MonoBehaviour
{
    [SerializeField] private Transform _head;             

    /// <summary>
    /// How much the head rotates relative to the mouse movements
    /// </summary>
    [SerializeField] public float _mouseSensitivity = 100f;
    
    /// <summary>
    /// The maximum degrees the player can look up/down
    /// </summary>
    [SerializeField] public float _verticalClamp = 85f;  

    private float _xRotation = 0f;     
    private float _yRotation = 0f;     


    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameStateManager.OnStatePlay += OnStatePlay;
    }

    void OnDestroy()
    {
        GameStateManager.OnStatePlay -= OnStatePlay;
    }

    /// <summary>
    /// The distance on the y axis from the players origin to the camera target 
    /// </summary>
    public float GetCameraTargetYOffset()
    {
        return _head.localPosition.y;        
    }

    private void OnStatePlay()
    {
        // setting the body and head rotation to inital states when level loads
        // we must convert world space rotation to local euler angles 

        Level loadedLevel = LevelFullMap.Singleton.GetLevelToSpawnAt();

        Vector3 dir = loadedLevel.PlayerSpawn.transform.forward;

        // yaw
        Vector3 flat = dir;
        flat.y = 0f;

        if (flat.sqrMagnitude > 0.0001f)
            _yRotation = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;

        // pitch
        Vector3 localDir = Quaternion.Euler(0f, -_yRotation, 0f) * dir;

        float horizontal = new Vector2(localDir.x, localDir.z).magnitude;
        _xRotation = -Mathf.Atan2(localDir.y, horizontal) * Mathf.Rad2Deg;

        _xRotation = Mathf.Clamp(_xRotation, -_verticalClamp, _verticalClamp);

        ReflectRotation();

        FindFirstObjectByType<CameraFollow>().SnapToTarget();

    }


    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        _yRotation += mouseX;
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -_verticalClamp, _verticalClamp);

        ReflectRotation();

    }

    /// <summary>
    /// Applies the internal head rotation values to the head and body transforms
    /// </summary>
    void ReflectRotation()
    {
        // Rotate the body 
        transform.rotation = Quaternion.Euler(0f, _yRotation, 0f);

        // Rotate the head 
        if (_head != null)
        {
            _head.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }
    }
}