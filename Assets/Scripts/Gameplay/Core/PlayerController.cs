
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerBreathing))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform bodyRotation;
    [SerializeField] Transform _headRotation;
    public Camera Camera;
    public Transform CameraTiltRotator;

    /// <summary>
    /// The graphic that fades in when the player's y position is lower than the enviroment death height
    /// </summary>
    [SerializeField] MaskableGraphic _deathHeightFadeToBlack; 

    [Header("Movement")]
    [SerializeField] private float _extraGravity = 1;
    public float runSpeed = 7f;
    public float maxHorizontalVelocity;
    public float acceleration = 80f;
    public float deceleration = 50f;
    public float airControlMultiplier = 0.3f; // Reduced control in air
    public float airAcceleration = 40f;
    [SerializeField] private float airDecceleration = 5;
    [SerializeField] private string _footstepWetnessLayerSoundLabel;
    [SerializeField] float _maxSpeed;

    /// <summary>
    /// The time it takes for the body to transition from non-crouching to crouching (vice-versa)
    /// </summary>
    [SerializeField] private float _crouchingBodyScaleSpeed;

    private bool _freeCam;
    private float _flyVerticalInput;
    [SerializeField] private float _flySpeed = 10;
    


    [Header("Jumping")]
    
    [SerializeField] private float _jumpHeight;
    /// <summary>
    /// How long we allow the player to be considered grounded after leaving the ground (enables jumping right after grounded)
    /// </summary>
    [SerializeField] private float _coyteTime; 
    /// <summary>
    /// How long we consider the jump input to be pressed after it was pressed (enables jumping right before grounded)
    /// </summary>
    [SerializeField] private float _inputCacheTime; // how long we hold onto jump inputs

    private float _timeSinceLastJumpPerformed = 0.0f;
    private float _timeSinceLastJumpInput = 0.0f;
    private float _timeSinceLastGrounded = 0.0f;
    private float _timeBelowDeathHeight = 0;


    public float slopeLimit = 55f;

    /// <summary>
    /// The magnitude of the sudden change in velocity that would kill the player
    /// </summary>
    [SerializeField] private float _yVelocityDeathThreshold = 12;

    [Header("Camera")]
    public float CameraMinFov = 50;
    public float CameraMaxFov = 100;
    public float cameraSpeedToReachMaxFov;
    public float cameraFovLerpSpeed = 10;
    public float cameraSpeedToReachMaxTilt;
    public float cameraTiltFactor;
    public float cameraTiltLerpSpeed = 10;

    [Header("FallingWind")]
    public float fallingWindLerpSpeed = 10;
    public float fallingWindSpeedToReachFullVolume = 100;

    /// <summary>
    /// How long it takes for the player to die under the death height threshold
    /// </summary>
    [SerializeField] private float _deathHeightTimeToDie;

    [Header("Grounding")]
    public float groundRayLength = 1.25f;
    public float FootstepDelayBetweenStepSounds = 5;
    [Range(0, 1)]
    public float FootstepVolumeInfluence = 0;
    public float FootstepMaxVolumeSpeed = 15; // where footstep volume reaches peak
    private float _footstepTimeTracked = 0;
    private bool _footstepLeftRightFlipFlop = false;
    public bool Grounded => _grounded;

    /// <summary>
    /// Mark this flag as true, if the level win has been reached
    /// </summary>
    public bool ReachedWinFlag = false;

    private CapsuleCollider capsuleCollider;   
    private Rigidbody rb;
    private PlayerBreathing _playerBreathing;
    private Vector3 _previousVelocity;
    private LoopingSound soundLoopFallingWind;
    private bool _grounded = false;
    private MaterialTypes _groundedMaterialType;
    private Vector2 _inputMovementVector;


    void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        _playerBreathing = GetComponent<PlayerBreathing>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        GameStateManager.OnStatePlay += OnStatePlay;
        GameStateManager.OnStateSelectingLevel += OnStateSelectingLevel;

    }

    void OnDestroy()
    {
        GameStateManager.OnStatePlay -= OnStatePlay;
        GameStateManager.OnStateSelectingLevel -= OnStateSelectingLevel;
    }

    /// <summary>
    /// Given a player spawn poisiton, shoots a ray to find the where it hits the ground, then shifts up up by the players collider
    /// </summary>
    /// <param name="spawnpoint">The spawnpoint to cast from</param>
    /// <returns>The spawnpoint after levelling</returns>
    public Vector3 ShiftSpawnpointToLevelWithGround(Vector3 spawnpoint)
    {
        capsuleCollider = GetComponent<CapsuleCollider>();

        if (Physics.Raycast(
            spawnpoint,
            Vector3.down,
            out RaycastHit hit,
            500.0f, 
            Helpers.Singleton.GroundLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            // Position player on the ground at the hit point
            // Add half the box collider height to place the bottom of the collider on the surface
            float heightOffset = capsuleCollider.height * 0.5f;
            spawnpoint = hit.point + Vector3.up * heightOffset;
        }

        return spawnpoint;
    }


    private void OnStateSelectingLevel()
    {
        // remove any existing tilt
        CameraTiltRotator.localEulerAngles = new Vector3(0, CameraTiltRotator.localEulerAngles.y, 0);

    }

    private void OnStatePlay()
    {
        // disable overflow wind from previous play session
        soundLoopFallingWind.volumeScaler = 0;
        
        ReachedWinFlag = false;

        Level loadedLevel = LevelFullMap.Singleton.GetLevelToSpawnAt();

        Camera.fieldOfView = CameraMinFov;
        CameraTiltRotator.localEulerAngles = new Vector3(0, 0, 0);

        transform.position = loadedLevel.PlayerSpawn.position;
        
        // Reset velocity so player doesn't carry over any physics state
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        _previousVelocity = Vector3.zero;

        rb.position = transform.position;   

        // make camera follow player 
        CameraFollow cameraFollow = Camera.main.GetComponentInParent<CameraFollow>();
        cameraFollow.Target = _headRotation;
        cameraFollow.TargetOffset = Vector3.zero;
        cameraFollow.SnapToTarget();
    }

    void OnEnable()
    {
        soundLoopFallingWind = LoopingAudioManager.Singleton.EnableLoop("FallingWind");
    }

    void OnDisable()
    {
        LoopingAudioManager.Singleton.DisableLoop("FallingWind");
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, _jumpHeight, rb.linearVelocity.z);
        _playerBreathing.PlaySound_Gasp();
        _timeSinceLastJumpPerformed = 0;
    }

    private void DetectIfBelowDeathHeight(float deltaTime)
    {
        if(transform.position.y < EnviromentManager.Singleton.EnviromentState.DeathZoneHeight)
        {
            _timeBelowDeathHeight += deltaTime;

            _deathHeightFadeToBlack.color = new Color(0,0,0, _timeBelowDeathHeight / _deathHeightTimeToDie);

            if(_timeBelowDeathHeight > _deathHeightTimeToDie)
            {
                GameStateManager.Singleton.SetState(GameStateManager.GameState.LoseBlackScreenWipe);   
            }
        }
        else
        {
            _deathHeightFadeToBlack.color = new Color(0,0,0,0);
            _timeBelowDeathHeight = 0;
        }
    }

    void FixedUpdate()
    {
        if (_freeCam)
        {   
            float x = _inputMovementVector.x, y = _inputMovementVector.y;
            Vector3 moveDirection = bodyRotation.transform.forward * y + bodyRotation.transform.right * x;
            moveDirection.Normalize();
            
            moveDirection.y = _flyVerticalInput;
            
            rb.linearVelocity = moveDirection * _flySpeed;
            return;
        }


        UpdateGround();
        TryToLoseOrWin();
        _timeSinceLastJumpPerformed += Time.fixedDeltaTime;
        _timeSinceLastJumpInput += Time.fixedDeltaTime;

        // add extra gravity force
        rb.AddForce(new Vector3(0, -_extraGravity * Time.fixedDeltaTime, 0));

        ApplyPhysicsWalking();

        _previousVelocity = rb.linearVelocity;

    }

    private void ApplyPhysicsWalking()
    {
        float x = _inputMovementVector.x, y = _inputMovementVector.y;
        bool hasInput = (_inputMovementVector.x != 0 || _inputMovementVector.y != 0);
        GetComponent<Animator>().SetBool("Moving", hasInput);

        if (hasInput)
        {
            // Player is giving input - use acceleration
            
            Vector3 moveDirection = bodyRotation.transform.forward * y + bodyRotation.transform.right * x;
            moveDirection.Normalize();

            if (_grounded)
            {
                rb.AddForce(moveDirection * runSpeed * acceleration, ForceMode.Acceleration);
            }
            else
            {
                // Reduced control mid air
                rb.AddForce(moveDirection * runSpeed * airAcceleration * airControlMultiplier, ForceMode.Acceleration);
            }
        }
        else
        {
            // No input - apply deceleration (damping)
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 dampingForce = -horizontalVelocity * deceleration;
            rb.AddForce(dampingForce, ForceMode.Acceleration);
        }

        // Limit horizontal velocity
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        
        if (horizontalVel.magnitude > _maxSpeed)
        {
            horizontalVel = horizontalVel.normalized * _maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVel.x, velocity.y, horizontalVel.z);
        }
    }

    private void CalculateInputs()
    {
        // Debug

        if(Helpers.Singleton.DebugMode){

            if(Input.GetKeyDown(KeyCode.F))
            {   
                _freeCam = !_freeCam;

                if (_freeCam)
                {
                    rb.useGravity = false;
                }
                else
                {
                    rb.useGravity = true;
                }
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                _flyVerticalInput = -1;
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                _flyVerticalInput = 1;
            }
            else
            {
                _flyVerticalInput = 0;
            }
        }

        // Movement ...

        if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            _inputMovementVector.x = -1;
        }
        else if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            _inputMovementVector.x = 1;
        }
        else
        {
            _inputMovementVector.x = 0;
        }

        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            _inputMovementVector.y = 1;
        }
        else if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            _inputMovementVector.y = -1;
        }
        else
        {
            _inputMovementVector.y = 0;
        }

        // Jumping ...

        if(Input.GetKeyDown(KeyCode.Space)){
            _timeSinceLastJumpInput = 0;
        }
    }

    void Update()
    {
        CalculateInputs();

        if (_timeSinceLastJumpInput < _inputCacheTime 
        && _timeSinceLastGrounded < _coyteTime
        && _timeSinceLastJumpPerformed > _coyteTime * 2)
        {
            Jump();
        }

        UpdateCameraFov();
        UpdateFallingWindVolume();
        UpdateCameraMovementTilt();
        UpdateGroundedFootstepSounds();

        DetectIfBelowDeathHeight(Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        
        float impactSpeed = collision.relativeVelocity.magnitude;

        float volume = Mathf.Lerp(1, Mathf.Clamp01(impactSpeed / FootstepMaxVolumeSpeed), FootstepVolumeInfluence);

        UpdateGround();
        PerformFootstepSound(volume);

    }

    void UpdateGroundedFootstepSounds()
    {
        _footstepTimeTracked += Time.deltaTime * rb.linearVelocity.magnitude;

        if (_grounded)
        {
            float volume = Mathf.Lerp(1, Mathf.Clamp01(rb.linearVelocity.magnitude / FootstepMaxVolumeSpeed), FootstepVolumeInfluence);
            PerformFootstepSound(volume);            
        }
    }

    private void PerformFootstepSound(float volume)
    {
        if (_freeCam)
        {
            return;
        }

        if(_footstepTimeTracked > FootstepDelayBetweenStepSounds)
        {
            var materialProperties = MaterialManager.Singleton.Properties[(int)_groundedMaterialType];

            if (_footstepLeftRightFlipFlop)
            {
                AudioManager.Singleton.Play(materialProperties.PlayerFootstepSoundLeft, Vector3.zero, volume);
            }
            else
            {
                AudioManager.Singleton.Play(materialProperties.PlayerFootstepSoundRight, Vector3.zero, volume);
            }

            AudioZoneManager.Singleton.PlayFootstepLayerSounds();
            AudioManager.Singleton.Play(_footstepWetnessLayerSoundLabel, Vector3.zero, EnviromentManager.Singleton.EnviromentState.RainStrength);

            _footstepLeftRightFlipFlop = !_footstepLeftRightFlipFlop;
            _footstepTimeTracked = 0;
        }
    }

    void UpdateCameraFov()
    {
        Vector3 velocity = rb.linearVelocity;
        
        float speedPercent = Mathf.Clamp01(velocity.magnitude / cameraSpeedToReachMaxFov);
        float targetFov = Mathf.Lerp(CameraMinFov, CameraMaxFov, speedPercent);
        
        // Frame-rate independent smoothing
        Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, targetFov, 1f - Mathf.Exp(-cameraFovLerpSpeed * Time.deltaTime));
    }

    void UpdateCameraMovementTilt()
    {
        Vector3 localVelocity = bodyRotation.InverseTransformDirection(rb.linearVelocity);

        // Get horizontal velocity components (ignore vertical)
        float xVelocity = localVelocity.x;
        float zVelocity = localVelocity.z;
        
        // Calculate tilt percentages based on velocity
        float xPercent = Mathf.Clamp(xVelocity / cameraSpeedToReachMaxTilt, -1f, 1f);
        float zPercent = Mathf.Clamp(zVelocity / cameraSpeedToReachMaxTilt, -1f, 1f);
        
        // Calculate target tilt angles
        // X velocity creates Z-axis tilt (rolling left/right)
        // Z velocity creates X-axis tilt (pitching forward/back)
        float targetZTilt = xPercent * cameraTiltFactor;  // Positive X velocity = roll right
        float targetXTilt = -zPercent * cameraTiltFactor; // Positive Z velocity = pitch down
        
        // Get current rotation
        Vector3 currentRotation = CameraTiltRotator.localEulerAngles;
        
        // Convert to -180 to 180 range for smooth interpolation
        float currentX = currentRotation.x > 180 ? currentRotation.x - 360 : currentRotation.x;
        float currentZ = currentRotation.z > 180 ? currentRotation.z - 360 : currentRotation.z;
        
        // Frame-rate independent smoothing
        float newXTilt = Mathf.Lerp(currentX, targetXTilt, 1f - Mathf.Exp(-cameraTiltLerpSpeed * Time.deltaTime));
        float newZTilt = Mathf.Lerp(currentZ, targetZTilt, 1f - Mathf.Exp(-cameraTiltLerpSpeed * Time.deltaTime));
        
        // Apply the tilted rotation
        CameraTiltRotator.localEulerAngles = new Vector3(newXTilt, currentRotation.y, newZTilt);
    }

    void UpdateFallingWindVolume()
    {

        // only introduce wind sound when we could die from the fall
        float deadlyVelocity = Mathf.Abs(rb.linearVelocity.y) - _yVelocityDeathThreshold * 0.2f;

        float speedPercent = Mathf.Clamp01(deadlyVelocity / fallingWindSpeedToReachFullVolume);
        float targetVolumeScaler = Mathf.Lerp(0, 1, speedPercent);

        if (_freeCam)
        {
            targetVolumeScaler = 0;
        }

        // Frame-rate independent smoothing
        soundLoopFallingWind.volumeScaler = Mathf.Lerp(soundLoopFallingWind.volumeScaler, targetVolumeScaler, 1f - Mathf.Exp(-fallingWindLerpSpeed * Time.deltaTime));
    }

    void UpdateGround()
    {
        Vector3 origin = transform.position;
        float radius = capsuleCollider.radius;

        if (Physics.SphereCast(origin,radius, Vector3.down,out RaycastHit hit, groundRayLength, Helpers.Singleton.GroundLayerMask, QueryTriggerInteraction.Ignore) &&
            Vector3.Angle(hit.normal, Vector3.up) <= slopeLimit)
        {


            ApplyMaterial applyMat = hit.collider.GetComponent<ApplyMaterial>();

            if(applyMat != null)
            {
                _groundedMaterialType = hit.collider.GetComponent<ApplyMaterial>().Material;
            }
            else
            {   
                // default to material type at index 0
                _groundedMaterialType = (MaterialTypes)0;
            }

            _timeSinceLastGrounded = 0;
            _grounded = true;
        
        }
        else
        {
            _timeSinceLastGrounded += Time.fixedDeltaTime;
            _grounded = false;
        }
    }

    /// <summary>
    /// Checks if we have rapidly slowed down on the y axis (we assume this is due to an impact), if so trigger the lose state
    /// </summary>
    private void TryToLoseOrWin()
    {
        if (_freeCam)
        {
            return;
        }

        if(Mathf.Abs(_previousVelocity.y - rb.linearVelocity.y) > _yVelocityDeathThreshold)
        {
            if (ReachedWinFlag)
            {
                GameStateManager.Singleton.SetState(GameStateManager.GameState.WinWhiteScreenWipe);   
            }
            else  
            {
                AudioManager.Singleton.Play("DeathImpact");
                AudioManager.Singleton.Play("DeathImpactBones");
                GameStateManager.Singleton.SetState(GameStateManager.GameState.LoseBlackScreenWipe);   
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Vector3 end = origin + Vector3.down * groundRayLength;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawSphere(end, 0.03f);
    }
}
