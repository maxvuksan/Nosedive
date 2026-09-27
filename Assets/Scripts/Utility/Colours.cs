using UnityEngine;


/// <summary>
/// A structure to encapsulate all the games colour values
/// </summary>
[CreateAssetMenu(menuName = "Custom/Colours Profile")]
public class ColoursProfile : ScriptableObject
{
    [Header("UI")]
    public Color UiIdle;
    public Color UiSelected;
    public Color UiDisabled;
    public Color UiInventoryGridBackingPanel;
    
    [Header("World")]
    public Color WireOff;
    public Color WireOn;
    public Color PressurePadOff;
    public Color PressurePadOn;
}
