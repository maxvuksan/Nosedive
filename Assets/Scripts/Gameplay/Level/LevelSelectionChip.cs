using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionChip : MonoBehaviour
{
    private static int s_smallChipSize = 7;
    private static int s_largeChipSize = 20;
    
    [SerializeField] private RectTransform _transformToScale; 
    [SerializeField] private Image _imageToColour;

    public void SetColour(Color colour)
    {
        _imageToColour.color = colour;
    }

    public void SetSelected(bool state)
    {
        if (state)
        {
            _transformToScale.sizeDelta = new Vector2(s_largeChipSize, s_largeChipSize);
        }
        else
        {
            _transformToScale.sizeDelta = new Vector2(s_smallChipSize, s_smallChipSize);
        }
    }

}
