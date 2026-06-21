using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; 
using System;

/// <summary>
/// A entry within NavigatableMenuList
/// </summary>
public class NavigatableMenuItem : MonoBehaviour
{
    public UnityEvent OnMainInput; 
    public UnityEvent OnLeftInput;
    public UnityEvent OnRightInput;

    /// <summary>
    /// The object which the horizontal jolting is applied to
    /// </summary>
    public GameObject HorizontalJoltTarget;
    public MaskableGraphic[] GraphicTargets;

    /// <summary>
    /// Sets the colour of all the graphic targets
    /// </summary>
    /// <param name="colour">The new colour for the graphic</param>
    public void SetColour(Color colour)
    {
        for(int i = 0; i < GraphicTargets.Length; i++)
        {
            GraphicTargets[i].color = colour;
        }        
    }


    /// <summary>
    /// This argumentless method exists so i can assign it as a Unity action in the inspector
    /// </summary>
    public void JoltRight()
    {
        HorizontalJoltTarget.GetComponent<UIElementJolt>().Jolt(new Vector2(1, 0));
    }
}

