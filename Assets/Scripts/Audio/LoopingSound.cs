using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public enum LoopCategory
{
    LOOP_MUSIC,
    LOOP_AMBIENCE,
    LOOP_IS_PART_OF_TRACK
}


/// <summary>
/// Represents a group of looping tracks, to be enabled/disabled together
/// </summary>
[System.Serializable]
public class LoopingProfile
{
    public string Name;
    public string[] LoopsToEnable;    
}


/// <summary>
/// A looping sound to be played by the LoopingAudioManager, configure with a LoopingSoundGroup
/// </summary>
[System.Serializable]
public class LoopingSound
{
    public string label;
    public bool scheduledForSyncing = false;    // if true, the loop will always be playing, even when volume is == 0, this is to keep it in sync with other loops. Use this for music

    public AudioClip clip;
    public LoopCategory loopCategory;

    [HideInInspector]
    public float volumeScaler = 1; // set via code
    [Range(0, 1)]
    public float volume = 1; // scales the volume by this factor
    [Range(0, 1)]
    public float minVolumeScale = 0.6f; // lowest volume can dip
    [Range(0, 1)]
    public float maxVolumeScale = 1.0f; // highest volume can reach
    [Range(0, 3)]
    public float volumeModulationRate; // lower modulation rate, the slower it changes volume

    [HideInInspector]
    public float fadeTimeTracked;
    [HideInInspector]
    public float fadeTime;
    [HideInInspector]
    public bool fadeIn = false;

    /// <summary>
    /// The associated audio source in the pool list
    /// </summary>
    [HideInInspector]
    public int PoolIndex;

    /// <summary>
    /// Some loops have a low pass filter assigned programatically 
    /// </summary>
    [HideInInspector] 
    public AudioLowPassFilter LowPassFilter;

}
