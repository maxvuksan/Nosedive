using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Manager to control playing FMOD sounds
/// </summary>
public class FmodAudioManager : MonoBehaviour
{
    public static FmodAudioManager Singleton;
    
    /// <summary>
    /// A list of sound groups
    /// </summary>
    [SerializeField] private FmodSoundGroup[] _soundGroups;
    
    private static Dictionary<string, FmodSound> SoundDictionary;

    /// <summary>
    /// The sounds which have been started but not stopped
    /// Key: Name, Value: Fmod EventInstance
    /// </summary>
    private static Dictionary<string, EventInstance> RunningInstances;
    
    private void Awake()
    {
        Helpers.CreateSingleton(ref Singleton, this, true);
        
        RunningInstances = new();
        ConstructDictionaryFromSoundGroups();
    }

    private void ConstructDictionaryFromSoundGroups()
    {
        SoundDictionary = new();

        foreach(var soundGroup in _soundGroups)
        {
            foreach(var sound in soundGroup.FmodSounds)
            {
                SoundDictionary.Add(sound.Name, sound);
            }   
        }
    }

    /// <summary>
    /// Trys to get a sound from the generated sound dictionary, will return true on success, false otherwise
    /// </summary>
    public static bool TryGetSound(string name, out FmodSound sound)
    {
        sound = null;

        if (Singleton == null || SoundDictionary == null)
        {
            Debug.LogWarning("FmodAudioManager is not initialised yet.");
            return false;
        }

        if (!SoundDictionary.TryGetValue(name, out sound))
        {
            Debug.LogWarning($"FMOD sound '{name}' could not be found.");
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// Plays a sound as a one shot
    /// </summary>
    public static void PlayOneShot(string name, float volume = 1.0f, float pitch = 1.0f)
    {
        // If we can't find the sound, return early
        if(!TryGetSound(name, out FmodSound sound))
        {
            return;
        }

        var instance = CreateEventInstance(sound.EventReference, false, Vector3.zero, volume, pitch);
        
        // Play the sound, then free the instance
        instance.start();
        instance.release();
    }

    /// <summary>
    /// Starts an EventInstance
    /// </summary>
    public static EventInstance PlayLoop(string name)
    {
        if(!TryGetSound(name, out FmodSound sound))
        {
            Debug.LogError($"The sound with name: {name} could not be found");
            throw new ArgumentException();
        }
        
        var instance = CreateEventInstance(sound.EventReference, false, Vector3.zero);

        // Play the sound, then free the instance
        instance.start();
        
        RunningInstances.Add(name, instance);
        return RunningInstances[name];
    }

    /// <summary>
    /// Turns off a running EventInstance
    /// </summary>
    public static void StopLoop(string name)
    {
        if (!RunningInstances.TryGetValue(name, out EventInstance runningSound))
        {
            Debug.LogWarning("Trying to disabling a sound that is not in the RunningInstance dictionary, ignoring");
            return;
        }

        // Stop the sound
        runningSound.release();
        RunningInstances.Remove(name);
    }
    
    /// <summary>
    /// Plays a sound as a one shot, assigning its position in 3D space
    /// </summary>
    public static void PlayOneShot(string name, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        // If we can't find the sound, return early
        if(!TryGetSound(name, out FmodSound sound))
        {
            return;
        }
        
        var instance = CreateEventInstance(sound.EventReference, false, Vector3.zero, volume, pitch);

        // Play the sound, then free the instance
        instance.start();
        instance.release();
    }
    
    /// <summary>
    /// Creates and returns an instance of an event, ready to be played 
    /// </summary>
    private static EventInstance CreateEventInstance(EventReference eventReference, bool is3D, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        EventInstance soundInstance = RuntimeManager.CreateInstance(eventReference.Path);
     
        if (is3D)
        {
            // Note: Within Fmod we may need to attach a spatializer to the corresponding sound to make it act as 3D
            soundInstance.set3DAttributes(position.To3DAttributes());
        }
        soundInstance.setVolume(volume);
        soundInstance.setPitch(pitch);
        
        return soundInstance;
    }
    
}
