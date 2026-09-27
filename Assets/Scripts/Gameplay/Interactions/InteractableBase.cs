using UnityEngine;

/// <summary>
/// Base interface for all world interactable
/// </summary>
public class InteractableBase : MonoBehaviour
{
    public string InteractionText = "Pickup";
    public bool CanBeInteractedWith { get; protected set; } = true;
    
    /// <summary>
    /// Is invoked on an interaction, this is only called when no InteractionBase is assigned as active
    /// </summary>
    public virtual void OnInteract(WorldInteractionDetector interactionDetector)
    {
        // Implemented by child class...
    }

    /// <summary>
    /// Is invoked when interactions occur to an already active assigned InteractionBase 
    /// </summary>
    public virtual void OnInteractWhenActive(WorldInteractionDetector interactionDetector)
    {
        // Implemented by child class...
    }

    /// <summary>
    /// Is invoked when an active interaction is closed
    /// </summary>
    public virtual void OnCloseActiveInteraction(WorldInteractionDetector interactionDetector)
    {
        // Implemented by child class...
    }
}