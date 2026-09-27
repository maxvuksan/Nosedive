using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains visual data for the collectable papers found around the game
/// </summary>
[CreateAssetMenu(menuName = "Custom/Collectable Paper Visual Data")]
public class PaperVisualData : ScriptableObject
{
    /// <summary>
    /// The size of this list must match the size defined on the InventoryGridPresenter
    /// Size == InventoryGridPresenter (Dimensions.x * Dimensions.y)
    /// </summary>
    public List<PaperVisualDataEntry> Papers;
}

[System.Serializable]
public class PaperVisualDataEntry
{
    /// <summary>
    /// The sprite to show when viewing the paper in the InventoryGridPresenter cells
    /// </summary>
    public Sprite SmallSprite;
}