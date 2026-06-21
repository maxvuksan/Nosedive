using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A custom value slider ui element 
/// </summary>
public class CustomSlider : MonoBehaviour
{
    
    /// <summary>
    /// This object is scaled on the axis between 0-1 depending on the sliders internal value
    /// </summary>
    [SerializeField] public Transform _scaler;

    /// <summary>
    /// Is invoked when the slider value changes, passes the new value as an argument to the function
    /// </summary>
    [SerializeField] public UnityEvent<float> _onValueChanged;

    /// <summary>
    /// How much the slider changes on increment
    /// </summary>
    public float IncrementFactor;

    /// <summary>
    /// The minium value of the slider (is represented by an empty bar visually)
    /// </summary>
    public float Min;

    /// <summary>
    /// The maximum value of the slider (is represented by a full bar visually)
    /// </summary>
    public float Max;

    /// <summary>
    /// The current value of the slider
    /// </summary>
    public float Value {
        get => _value; 
        set 
        { 
            _value = Mathf.Clamp(value, Min, Max);
            
            RefreshScaleObject();

            _onValueChanged?.Invoke(_value);
        }
    }

    private float _value;

    private void Awake() {
        Value = (Min + Max) / 2.0f;
    }

    /// <summary>
    /// Increases the value by the increment factor
    /// </summary>
    public void IncreaseValue()
    {
        Value = Value + IncrementFactor;
    }

    /// <summary>
    /// Increases the value by the increment factor
    /// </summary>
    public void DecreaseValue()
    {
        Value = Value - IncrementFactor;
    }

    /// <summary>
    /// Rescales the fill transform by the current value
    /// </summary>
    public void RefreshScaleObject()
    {
        _scaler.localScale = new Vector3(Value / Max, _scaler.localScale.y, _scaler.localScale.z);
    }
}
