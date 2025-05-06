using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource; // Drag to the component to the script is way faster than get it in Awake() method

    public void SetSound(SoundDetails soundDetails)
    {
        audioSource.clip = soundDetails.soundClip;
        audioSource.volume = soundDetails.soundVolume;
        audioSource.pitch = Random.Range(soundDetails.soundPitchMin, soundDetails.soundPitchMax);
    }
}
