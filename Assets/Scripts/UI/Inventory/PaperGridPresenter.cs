

using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages the paper sprites on the inventory grid
/// </summary>
public class PaperGridPresenter : InventoryGridPresenter
{
    /// <summary>
    /// The scriptable object containing metadata for each paper 
    /// </summary>
    public PaperVisualData VisualData;

    public static PaperGridPresenter Singleton;
    
    public new void Awake()
    {
        base.Awake();
        Helpers.CreateSingleton(ref Singleton, this);
    }

    public void UnlockPaperAtIndex(int index)
    {
        AssignSpriteToCell(index, VisualData.Papers[index].SmallSprite);
    }



}
