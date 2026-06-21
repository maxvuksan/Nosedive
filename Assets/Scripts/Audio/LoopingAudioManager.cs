using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

/*
    manages constant looping audio tracks, e.g. ambient sound
*/
public class LoopingAudioManager : MonoBehaviour
{
    public static LoopingAudioManager Singleton = null;
    public AudioMixerGroup AudioMix;
    public float DefaultFadeInTime = 10;

    [SerializeField] private LoopingSoundGroup[] _soundGroups;

    /// <summary>
    /// Generate a dictionary for each sound, using the label as key for fast lookup
    /// </summary>
    private Dictionary<string, LoopingSound> _soundDictionary;

    private List<AudioSource> _loopingSourcePool;

    void Awake()
    {
        Helpers.CreateSingleton<LoopingAudioManager>(ref Singleton, this);

        _soundDictionary = new();
        _loopingSourcePool = new();

        // construct sound dictionary...
        foreach(LoopingSoundGroup soundGroup in _soundGroups)
        {
            foreach(LoopingSound sound in soundGroup.sounds)
            {
                AddPoolObject(sound);
                _soundDictionary.Add(sound.label, sound);
            }
        }

    }
    
    void AddPoolObject(LoopingSound sound)
    {
        GameObject sound_obj = new GameObject();
        sound_obj.name = "[LOOP] " + sound.label;
        sound_obj.transform.parent = transform;

        AudioSource new_source = sound_obj.AddComponent<AudioSource>();

        new_source.loop = true;
        new_source.volume = 0;
        new_source.clip = sound.clip;
        new_source.outputAudioMixerGroup = AudioMix;

        sound.fadeIn = false;
        sound.fadeTimeTracked = 0;
        sound.fadeTime = 0;
        sound.volumeScaler = 1;
        sound.PoolIndex = _loopingSourcePool.Count;
        _loopingSourcePool.Add(new_source);
    }


    void Update()
    {
        ManageFading();
    }

    void ManageFading() {

        foreach(var sound in _soundDictionary) {
            
            float volumeScaler = sound.Value.volumeScaler;
            float volume = sound.Value.volume;

            if (volume == 0 && !sound.Value.fadeIn) {

                // stop if playing, only stop non synced loops
                if (_loopingSourcePool[sound.Value.PoolIndex].isPlaying && !sound.Value.scheduledForSyncing) {
                    _loopingSourcePool[sound.Value.PoolIndex].Stop(); 
                }
            }
            else {
                // start if not playing
                if (!_loopingSourcePool[sound.Value.PoolIndex].isPlaying) {
                    _loopingSourcePool[sound.Value.PoolIndex].Play();
                }

                if (sound.Value.fadeTimeTracked < sound.Value.fadeTime)
                {
                    sound.Value.fadeTimeTracked += Time.deltaTime;

                    if (sound.Value.fadeIn)
                    {
                        volume *= sound.Value.fadeTimeTracked / (float)sound.Value.fadeTime;
                    }
                    else
                    {
                        volume *= 1.0f - sound.Value.fadeTimeTracked / (float)sound.Value.fadeTime;
                    }
                }
                else
                {
                    if (!sound.Value.fadeIn)
                    {
                        volume = 0;
                    }

                    sound.Value.fadeTimeTracked = sound.Value.fadeTime;
                }

                
            }

            // modulate the volume
            float t = (Mathf.Sin(Time.time * sound.Value.volume * sound.Value.volumeModulationRate) + 1.0f) * 0.5f;
            float volumeMod = sound.Value.volume * Mathf.Lerp(sound.Value.minVolumeScale, sound.Value.maxVolumeScale, t);

            _loopingSourcePool[sound.Value.PoolIndex].volume = volume * volumeMod * volumeScaler * SaveManager.Data.Settings.EnvironmentVolume;

        }
    }

    public LoopingSound EnableLoop(string loopLabel, float loopFadeIn = -1f, float volumeScaler = 1) {

        return FadeLoop(loopLabel, loopFadeIn, true, volumeScaler);
    }

    public LoopingSound DisableLoop(string loopLabel, float loopFadeIn = -1f) {
        return FadeLoop(loopLabel, loopFadeIn, false, -1);
    }

    public LoopingSound GetLoop(string loopLabel)
    {
        return _soundDictionary[loopLabel];
    }

    /// <summary>
    /// Attaches a low pass filter to the audio source associated with a specific loop
    /// </summary>
    public void AttachLowPassFilterToLoop(string loopLabel)
    {
        var filter = _loopingSourcePool[_soundDictionary[loopLabel].PoolIndex].AddComponent<AudioLowPassFilter>();
        _soundDictionary[loopLabel].LowPassFilter = filter;
    }

    public void DisableEntireCategory(LoopCategory category, float loopFadeIn = -1f) {

        // for (int i = 0; i < _soundGroup.sounds.Length; i++)
        // {
        //     if (_soundGroup.sounds[i].loopCategory == category)
        //     {
        //         FadeLoop(i, loopFadeIn, false, -1);
        //     }
        // }
    }

    private LoopingSound FadeLoop(string loopLabel, float loopFadeTime, bool fadeIn, float volumeScaler)
    {
        if (volumeScaler != -1)
        {
            _soundDictionary[loopLabel].volumeScaler = volumeScaler;
        }

        // we are already fading in this direction
        if (_soundDictionary[loopLabel].fadeIn == fadeIn)
        {
            return _soundDictionary[loopLabel];
        }

        // use default fade time
        if (loopFadeTime < 0)
        {
            loopFadeTime = DefaultFadeInTime;
        }

        _soundDictionary[loopLabel].fadeTime = loopFadeTime;
        _soundDictionary[loopLabel].fadeIn = fadeIn;
        _soundDictionary[loopLabel].fadeTimeTracked = 0.0f;

        return _soundDictionary[loopLabel];
    }




}
 