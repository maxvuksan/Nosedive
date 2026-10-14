


using FMOD.Studio;
using FMODUnity;

[System.Serializable]
public class FmodSound
{
    /// <summary>
    /// The name of the sound, this is how it will be stored in the FmodAudioManager
    /// </summary>
    public string Name;
    
    /// <summary>
    /// The reference to the actual Fmod event instance, this is what sound will play
    /// </summary>
    public EventReference EventReference;
}
