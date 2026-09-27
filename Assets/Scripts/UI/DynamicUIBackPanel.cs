using UnityEngine;
using UnityEngine.UI;

public enum PanelColour
{
    Black,
    White,
}

/// <summary>
/// Dynamic back panel which can move around the screen to accent different text elements 
/// </summary>
public class DynamicUIBackPanel : MonoBehaviour
{
    private static DynamicUIBackPanel Singleton;
    [SerializeField] private RectTransform _backPanel;
    [SerializeField] private Image _backPanelImage;
    [SerializeField] private float _widthPadding;
    
    void Awake()
    {
        Helpers.CreateSingleton(ref Singleton, this);
    }
    
    public static void SetColour(PanelColour colourEnum)
    {
        switch(colourEnum)
        {
            case PanelColour.Black:
                Singleton._backPanelImage.color = Color.black;
                break;
            case PanelColour.White:
                Singleton._backPanelImage.color = Color.white;
                break;
        }
    }
    
    public static void SetPosition(Vector2 position)
    {
        Singleton._backPanel.position = position;
    }

    public static void SetWidth(float width)
    {
        float newWidth = width + Singleton._widthPadding;
        if (width == 0)
        {
            // Setting width to 0 means we are turning the panel off
            newWidth = 0;
        }
        
        Singleton._backPanel.sizeDelta = new Vector2(newWidth, Singleton._backPanel.sizeDelta.y);
    }

}
