using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// A custom value slider ui element 
/// </summary>
public class CustomTextCarousel : MonoBehaviour
{
    
    /// <summary>
    /// This text object is changed to match the selected option
    /// </summary>
    [SerializeField] private TextMeshProUGUI _text;

    /// <summary>
    /// The possible options to be selected
    /// </summary>
    [SerializeField] private string[] _options;

    /// <summary>
    /// Is invoked when the selected index changes, this index is passed as an argument to the function
    /// </summary>
    [SerializeField] private UnityEvent<int> _onIndexChanged;

    /// <summary>
    /// The current index of the carousel
    /// </summary>
    public int Index {
        get => _index; 
        set 
        { 
            int newIndex = value;
            if(newIndex >= _options.Length)
            {
                newIndex = 0;
            }
            else if(newIndex < 0)
            {
                newIndex = _options.Length - 1;
            }

            _index = newIndex;
            RefreshText();

            _onIndexChanged?.Invoke(_index);
        }
    }

    private int _index;

    private void Awake() {
        Index = 0;
    }

    /// <summary>
    /// Increases the value 
    /// </summary>
    public void NextValue()
    {
        Index = Index + 1;
    }

    /// <summary>
    /// Increases the value 
    /// </summary>
    public void PreviousValue()
    {
        Index = Index - 1;
    }

    /// <summary>
    /// Sets the apprioriate text on the text object
    /// </summary>
    public void RefreshText()
    {
        _text.text = _options[Index];
    }
}
