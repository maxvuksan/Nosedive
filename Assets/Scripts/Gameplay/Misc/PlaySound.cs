using UnityEngine;


/// <summary>
/// A single configured sound
/// </summary>
[System.Serializable]
public struct PlaySoundEntry
{
    /// <summary>
    /// The label the AudioManager will search for 
    /// </summary>
    public string Label;
}

/// <summary>
/// Helper script to play sounds via function invocations, this is intended to be triggered as UnityEvents within animations
/// </summary>
public class PlaySound : MonoBehaviour
{
    [SerializeField] private PlaySoundEntry _oneShotA;
    
    public void PlayOneShotA()
    {
        FmodAudioManager.PlayOneShot(_oneShotA.Label, transform.position, 1, 1);        
    }
}
