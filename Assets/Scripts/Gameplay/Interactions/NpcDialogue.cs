
using UnityEngine;

/// <summary>
/// Configuration for a specific npc interaction sequence
/// </summary>
[CreateAssetMenu(menuName = "Custom/Npc Dialogue")]
public class NpcDialogue : ScriptableObject{

    public NpcDialogueLine[] Lines;
}

[System.Serializable]
public struct NpcDialogueLine
{
    public string Content;
}