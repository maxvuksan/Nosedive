

using UnityEngine;

/// <summary>
/// A group of configured sounds, to be played by the FmodAudioManager
/// </summary>
[CreateAssetMenu(menuName = "Custom/Fmod Sound Group")]
public class FmodSoundGroup : ScriptableObject
{
    public FmodSound[] FmodSounds;
}

