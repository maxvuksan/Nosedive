using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A group of sound loops, to be played by the LoopingAudioManager
/// </summary>

[CreateAssetMenu(menuName = "Custom/Looping Sound Group")]
public class LoopingSoundGroup : ScriptableObject
{
    public LoopingSound[] sounds;
}

