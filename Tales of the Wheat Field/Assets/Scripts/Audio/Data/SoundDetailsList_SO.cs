using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "SoundDetailsList_SO", menuName = "Sound/SoundDetailsList")]
public class SoundDetailsList_SO : ScriptableObject
{
    public List<SoundDetails> soundDetailsList;

    public SoundDetails GetSoundDetails(SoundName soundName)
    {
        return soundDetailsList.Find(i => i.soundName == soundName);
    }
}
    [System.Serializable]
public class SoundDetails 
{
    public SoundName soundName;

    public AudioClip soundClip;
    [Range(0.1f,1.5f)]
    public float soundPitchMin ;
    [Range(0.1f,1.5f)]
    public float soundPitchMax ;
    [Range(0.1f, 1f)]
    public float soundVolume;

    public float soundPitch()
    {
        return Random.Range(soundPitchMin, soundPitchMax);
    }
}

