
using System.Collections.Generic;
using UnityEngine;

public struct AudioZoneFootstepLayerInfluence
{
    /// <summary>
    /// Reference to the audio zone
    /// </summary>
    public AudioZoneFootstepLayer Zone;
    /// <summary>
    /// The computed influence to apply
    /// </summary>
    public float Influence;
}

public struct AudioZoneLoopedInfluence
{
    /// <summary>
    /// Reference to the audio zone
    /// </summary>
    public AudioZoneLooped Zone;
    /// <summary>
    /// The computed influence to apply
    /// </summary>
    public float Influence;

    /// <summary>
    /// The lerp value for the low pass filter, to lerp between min and max cutoff
    /// </summary>
    public float LowPassFilterInfluence;
}

/// <summary>
/// Manages the influence of audio zones
/// </summary>
public class AudioZoneManager : MonoBehaviour {

    public static AudioZoneManager Singleton;

    /// <summary>
    /// All the audio zones currently within the active levels
    /// </summary>
    private List<AudioZone> _zonePool = new();

    /// <summary>
    /// Footstep layers to apply
    /// </summary>
    private List<AudioZoneFootstepLayerInfluence> _inRangeZoneFootsteps = new(); 

    /// <summary>
    /// Looping audio to play
    /// </summary>
    private List<AudioZoneLoopedInfluence> _inRangeZoneLooped = new(); 

    


    /// <summary>
    /// This array should match the structure of the FootstepLayerTypes enum
    /// </summary>
    public string[] FootstepLayerSounds;

    private SimpleWalker _player;

    public void Awake()
    {
        _player = FindFirstObjectByType<SimpleWalker>(FindObjectsInactive.Include);
        Helpers.CreateSingleton(ref Singleton, this);
    }

    /// <summary>
    /// Adds an audio zone to the active pool
    /// </summary>
    /// <param name="zone">The zone to add</param>
    public void AddZone(AudioZone zone)
    {
        _zonePool.Add(zone);
    }

    /// <summary>
    /// Removes an audio zone from the active pool
    /// </summary>
    /// <param name="zone">The zone to remove</param>
    public void RemoveZone(AudioZone zone)
    {
        _zonePool.Remove(zone);
    }

    void FixedUpdate() 
    {
        // only perform level streaming in playmode
        if(GameStateManager.CurrentState != GameStateManager.GameState.Playing)
        {
            return;
        }

        _inRangeZoneFootsteps.Clear();

        foreach(var zone in _inRangeZoneLooped)
        {
            foreach(string label in zone.Zone.LoopsToFadeIn)
            {
                // turn off volume of loop incase
                LoopingAudioManager.Singleton.GetLoop(label).volumeScaler = 0.0f;
                LoopingAudioManager.Singleton.GetLoop(label).fadeIn = true;
            }
        }
        _inRangeZoneLooped.Clear();
        

        foreach(var zone in _zonePool)
        {
            float influence = zone.GetInfluenceFactor(_player.transform.position);

            // we are in range of audio zone
            if (influence > 0)
            {
                if(zone is AudioZoneFootstepLayer)
                {
                    AudioZoneFootstepLayerInfluence data = new();
                    data.Zone = (AudioZoneFootstepLayer)zone;
                    data.Influence = influence;

                    _inRangeZoneFootsteps.Add(data);
                }

                if(zone is AudioZoneLooped)
                {
                    AudioZoneLoopedInfluence data = new();
                    data.Zone = (AudioZoneLooped)zone;
                    data.Influence = influence;
                    data.LowPassFilterInfluence = data.Zone.GetFilterInfluenceFactor(_player.transform.position);

                    _inRangeZoneLooped.Add(data);
                }
            }
        }

        // apply in range zones
        foreach(var zone in _inRangeZoneLooped)
        {
            foreach(string label in zone.Zone.LoopsToFadeIn)
            {
                LoopingSound sound = LoopingAudioManager.Singleton.GetLoop(label);

                sound.volumeScaler = zone.Influence;
                sound.fadeIn = true;

                sound.LowPassFilter.cutoffFrequency = Mathf.Lerp(
                    zone.Zone.LowPassFilterSettings.MinCutoffFrequency, 
                    zone.Zone.LowPassFilterSettings.MaxCutoffFrequency, 
                    zone.LowPassFilterInfluence);
            }
        }
    }


    /// <summary>
    /// Plays the footstep layer sounds using the computed influences as volume scalers
    /// </summary>
    public void PlayFootstepLayerSounds()
    {
        foreach(var zone in _inRangeZoneFootsteps)
        {
            AudioManager.Singleton.Play(FootstepLayerSounds[(int)zone.Zone.Type], _player.transform.position, zone.Influence);
        }        
    }

}