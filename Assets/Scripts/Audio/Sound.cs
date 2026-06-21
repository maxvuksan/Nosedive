using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A sound to be played by the AudioManager, configure with a SoundGroup
/// </summary>
[System.Serializable]
public class Sound
{
    public string name;

    [Header("General")]
    public AudioClip[] clips;
    [Range(0f, 1f)] 
    public float volume;
    [Range(0.1f, 3f)]
    public float pitch;
    [Range(0f,0.4f)]
    public float pitch_variation;
}

