using UnityEngine;

/// <summary>
/// The interactable dialogue for NPCs
/// </summary>
public class InteractableNpc : InteractableBase
{
    public NpcDialogue Dialogue;

    private int _npcDialogueLineIndex;

    
    public override void OnInteract(WorldInteractionDetector interactionDetector)
    {
        print("on interact with npc");
        interactionDetector.AssignActiveInteraction(this);
        _npcDialogueLineIndex = 0;
        NpcDialoguePresenter.Singleton.LoadLine(Dialogue.Lines[_npcDialogueLineIndex]);
    }    
    
    public override void OnInteractWhenActive(WorldInteractionDetector interactionDetector)
    {
        AdvanceDialogueLine(interactionDetector);
    }

    public override void OnCloseActiveInteraction(WorldInteractionDetector interactionDetector)
    {
        _npcDialogueLineIndex = 0;
        NpcDialoguePresenter.Singleton.UnloadLine();
    }


    private void AdvanceDialogueLine(WorldInteractionDetector interactionDetector)
    {
        _npcDialogueLineIndex++;

        if (_npcDialogueLineIndex >= Dialogue.Lines.Length)
        {
            NpcDialoguePresenter.Singleton.UnloadLine();
            interactionDetector.ReleaseActiveInteraction();
            return;
        }
        
        _npcDialogueLineIndex %= Dialogue.Lines.Length;
        NpcDialoguePresenter.Singleton.LoadLine(Dialogue.Lines[_npcDialogueLineIndex]);
    }
}
