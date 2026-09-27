using UnityEngine;

/// <summary>
/// Enables ui elements to visually jolt in a direction 
/// </summary>
public class UIElementJolt : MonoBehaviour
{
    public float JoltStrength { get => _joltStrength; set => _joltStrength = value; }
    public float JoltSpeed { get => _joltSpeed; set => _joltSpeed = value; }
    
    [SerializeField] private float _joltStrength = 20f;
    [SerializeField] private float _joltSpeed = 10f;
    private static AnimationCurve _joltCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private RectTransform _rectTransform;
    private Vector2 _originalPosition;
    private Vector2 _targetPosition;
    private Vector2 _currentVelocity;
    private bool _isJolting;
    private float _joltTimer;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originalPosition = _rectTransform.anchoredPosition;
    }

    /// <summary>
    /// Called by UIJoltManager. Returns false when jolt is complete. We are making UIJoltManager update us to avoid having redudant Update() calls for idle elements
    /// </summary>
    public bool UpdateJolt()
    {
        if (!_isJolting) return false;

        _joltTimer += Time.deltaTime * _joltSpeed;

        if (_joltTimer >= 1f)
        {
            // Return to original position
            _rectTransform.anchoredPosition = Vector2.SmoothDamp(
                _rectTransform.anchoredPosition,
                _originalPosition,
                ref _currentVelocity,
                0.1f
            );

            // Check if we're close enough to stop
            if (Vector2.Distance(_rectTransform.anchoredPosition, _originalPosition) < 0.1f)
            {
                _rectTransform.anchoredPosition = _originalPosition;
                _isJolting = false;
                _joltTimer = 0f;
                return false; // Jolt complete
            }
        }
        else
        {
            // Jolt away from original position
            float curveValue = _joltCurve.Evaluate(_joltTimer);
            _rectTransform.anchoredPosition = Vector2.Lerp(
                _originalPosition,
                _targetPosition,
                curveValue
            );
        }

        return true; // Still jolting
    }

    /// <summary>
    /// Jolts the UI element in the specified direction
    /// </summary>
    /// <param name="direction">Normalized direction vector to jolt towards</param>
    public void Jolt(Vector2 direction)
    {
        // Store the original position if we're starting fresh
        if (!_isJolting)
        {
            _originalPosition = _rectTransform.anchoredPosition;
        }

        // Calculate target position based on direction and strength
        _targetPosition = _originalPosition + (direction.normalized * _joltStrength);
        
        // Reset timer and start jolting
        _joltTimer = 0f;
        _isJolting = true;
        _currentVelocity = Vector2.zero;

        // Register with manager
        UIJoltManager.Instance.RegisterJolt(this);
    }

    /// <summary>
    /// Stop the current jolt and snap back to original position
    /// </summary>
    public void StopJolt()
    {
        _isJolting = false;
        _rectTransform.anchoredPosition = _originalPosition;
        _joltTimer = 0f;
        UIJoltManager.Instance.UnregisterJolt(this);
    }

    private void OnDisable()
    {
        if (_isJolting)
        {
            UIJoltManager.Instance.UnregisterJolt(this);
        }
    }
}