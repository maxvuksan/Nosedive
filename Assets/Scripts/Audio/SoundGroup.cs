using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A group of configured sounds, to be played by the AudioManager
/// </summary>
[CreateAssetMenu(menuName = "Custom/Sound Group")]
public class SoundGroup : ScriptableObject
{
    public Sound[] sounds;
}

