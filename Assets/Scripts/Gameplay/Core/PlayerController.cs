
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class SimpleWalker : MonoBehaviour
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


    [Header("Crouching / Sliding")]
    [SerializeField] private float _crouchingBodyScaleY = 0.2f;
    
    /// <summary>
    /// The time it takes for the body to transition from non-crouching to crouching (vice-versa)
    /// </summary>
    [SerializeField] private float _crouchingBodyScaleSpeed;

    [SerializeField] private PhysicsMaterial _bouncingPhysicsMaterial;
    [SerializeField] private PhysicsMaterial _slidingPhysicsMaterial;
    [SerializeField] private float _slidingInitalDownwardVelocity = 5;
    [SerializeField] private float _slidingInitalForwardBoost;
    [SerializeField] private float _slidingDeceleration = 8f;
    [SerializeField] private float _slidingTurnRateDegrees = 360; // max degrees/second
    [SerializeField] private float _slidingMinSpeed = 2f;
    [SerializeField] private float _slidingGravityAccelMultiplier = 3;
    private bool _sliding;
    private bool _crouching;
    private float _crouchingBodyScaleYTracked;


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


    /// <summary>
    /// Mark this flag as true, if the level win has been reached
    /// </summary>
    public bool ReachedWinFlag = false;

    private CapsuleCollider capsuleCollider;   
    private Rigidbody rb;
    private Vector3 _previousVelocity;
    private LoopingSound soundLoopFallingWind;
    private bool _grounded = false;
    private MaterialTypes _groundedMaterialType;
    private Vector2 _inputMovementVector;
    private Vector3 _groundedNormal;


    void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
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
        _crouching = false;
        _crouchingBodyScaleYTracked = 1;
    }

    void OnDisable()
    {
        LoopingAudioManager.Singleton.DisableLoop("FallingWind");
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, _jumpHeight, rb.linearVelocity.z);
        _timeSinceLastJumpPerformed = 0;
        _crouching = false;
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
        UpdateGround();
        TryToLoseOrWin();
        _timeSinceLastJumpPerformed += Time.fixedDeltaTime;
        _timeSinceLastJumpInput += Time.fixedDeltaTime;


        if (_crouching)
        {
            _crouchingBodyScaleYTracked = Mathf.MoveTowards(_crouchingBodyScaleYTracked, _crouchingBodyScaleY, Time.fixedDeltaTime * _crouchingBodyScaleSpeed);
            capsuleCollider.material = _slidingPhysicsMaterial;
        }
        else
        {
            _crouchingBodyScaleYTracked = Mathf.MoveTowards(_crouchingBodyScaleYTracked, 1, Time.fixedDeltaTime * _crouchingBodyScaleSpeed);
            capsuleCollider.material = _bouncingPhysicsMaterial;
        }

        transform.localScale = new Vector3(1, _crouchingBodyScaleYTracked, 1);

        // add extra gravity force
        rb.AddForce(new Vector3(0, -_extraGravity * Time.fixedDeltaTime, 0));



        if (_crouching && _grounded)
        {
            ApplyPhysicsSliding();
        }
        else
        {
            ApplyPhysicsWalking();
            _sliding = false;
        }

        _previousVelocity = rb.linearVelocity;

    }

    private void ApplyPhysicsSliding()
    {
        if(rb.linearVelocity.magnitude > _slidingMinSpeed)
        {
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            if (!_sliding)
            {
                // Always launch in the direction the player is facing, not leftover walk velocity
                Vector3 startDir = bodyRotation.forward;
                float startSpeed = Mathf.Max(horizontalVel.magnitude, runSpeed) * _slidingInitalForwardBoost;
                Vector3 boosted = startDir * startSpeed;
                rb.linearVelocity = new Vector3(boosted.x, rb.linearVelocity.y, boosted.z);
                horizontalVel = new Vector3(boosted.x, 0, boosted.z);
            }

            _sliding = true;

            float speed = horizontalVel.magnitude;
            Vector3 currentDir = speed > 0.0001f ? horizontalVel.normalized : bodyRotation.forward;

            // Steering: rotate direction towards input (this is the ONLY thing that changes direction)
            if (_inputMovementVector.sqrMagnitude > 0.01f)
            {
                Vector3 steerDir = (bodyRotation.forward * _inputMovementVector.y + bodyRotation.right * _inputMovementVector.x).normalized;
                float maxRadiansDelta = _slidingTurnRateDegrees * Mathf.Deg2Rad * Time.fixedDeltaTime;
                currentDir = Vector3.RotateTowards(currentDir, steerDir, maxRadiansDelta, 0f).normalized;
            }

            // Slope-driven speed change: gravity's pull along the slope, projected onto OUR direction
            // (this is a scalar dot product - it doesn't rotate currentDir, only scales speed)
            Vector3 gravityAlongSlope = Vector3.ProjectOnPlane(Physics.gravity, _groundedNormal);
            float downhillAccel = Vector3.Dot(gravityAlongSlope, currentDir) * _slidingGravityAccelMultiplier;

            speed += downhillAccel * Time.fixedDeltaTime;
            speed -= _slidingDeceleration * Time.fixedDeltaTime;
            speed = Mathf.Clamp(speed, 0f, _maxSpeed);

            Vector3 newHorizontal = currentDir * speed;
            rb.linearVelocity = new Vector3(newHorizontal.x, rb.linearVelocity.y, newHorizontal.z);
        }
        else
        {
            _crouching = false;
            _sliding = false;
        }
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

        // Crouching / Sliding ...

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _crouching = true;
            rb.AddForce(new Vector3(0, _slidingInitalDownwardVelocity, 0), ForceMode.Impulse);
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _crouching = false;
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
        && _timeSinceLastJumpPerformed > _coyteTime)
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
        if (_crouching)
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
        // Get horizontal velocity components (ignore vertical)
        float xVelocity = rb.linearVelocity.x;
        float zVelocity = rb.linearVelocity.z;
        
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

            _groundedNormal = hit.normal;
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
