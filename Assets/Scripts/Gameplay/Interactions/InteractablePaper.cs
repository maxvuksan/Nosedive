using UnityEngine;

/// <summary>
/// The interactable papers found scattered around the world
/// </summary>
public class InteractablePaper : InteractableBase
{
    /// <summary>
    /// The index this paper is unlocking, this corresponds to the index of the PaperVisualData entry
    /// </summary>
    public int UnlockIndex = 0;
    
    [SerializeField] private GameObject _paperSheetMesh;
    
    public void SetCollectedState(bool state)
    {
        _paperSheetMesh.SetActive(!state);
        CanBeInteractedWith = false;
    }

    public override void OnInteract(WorldInteractionDetector interactionDetector)
    {
        AudioManager.Singleton.Play("GrabPaper");
        PaperGridPresenter.Singleton.UnlockPaperAtIndex(UnlockIndex);
        SetCollectedState(true);
    }
}
