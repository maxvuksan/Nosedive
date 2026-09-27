using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A helper script to enable fading colour effects on graphic elements (textures, sprites etc...). E.g. the screen fades to from black->transparent when a level loads
/// </summary>
public class ColourFader : MonoBehaviour
{
    
    [SerializeField] private Color _offColour;
    [SerializeField] private Color _onColour;
    [SerializeField] private MaskableGraphic _target;
    [SerializeField] private float _fadeSpeed;
    [SerializeField] private bool _initalOnState;
    [SerializeField] private bool _toggleOnEnable;
    
    private bool _onState;
    private float _fadeTimeTracked;

    void OnEnable()
    {   
        if (_initalOnState)
        {
            _fadeTimeTracked = 1;
            _target.color = _onColour;
        }
        else
        {
            _fadeTimeTracked = 0;
            _target.color = _offColour;
        }

        LerpColour();

        if (_toggleOnEnable)
        {
            SetOnState(!_initalOnState);
        }
    }

    public void Update()
    {
        if (!_onState)
        {
            _fadeTimeTracked -= Time.deltaTime * _fadeSpeed;
        }
        else
        {
            _fadeTimeTracked += Time.deltaTime * _fadeSpeed;
        }

        _fadeTimeTracked = Mathf.Clamp01(_fadeTimeTracked);

        LerpColour();
    }

    /// <summary>
    /// Sets the state of the fader, if it is not the current state, it will begin transitioning to the new state within Update()
    /// </summary>
    /// <param name="onState">The state to set the fader to</param>
    public void SetOnState(bool onState)
    {
        _onState = onState;
    }
    
    /// <summary>
    /// Sets the fade time tracked variable, this represents the on state as a lerp time parameter, 0 is fully offColour, 1 is fully onColour
    /// </summary>
    /// <param name="t"></param>
    public void SetFadeTimeT(float t)
    {
        _fadeTimeTracked = t;
        LerpColour();
    }

    private void LerpColour()
    {
        _target.color = Color.Lerp(_offColour, _onColour, _fadeTimeTracked);
    }

}
