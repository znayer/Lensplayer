using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour {


    AudioSource audioSource;

    static SoundEffects instance;

	// Use this for initialization
	void Start () {
        instance = this;
        audioSource = GetComponent<AudioSource>();
	}
	


    static public void PlaySoundEffect(AudioClip clip)
    {
        if (instance.audioSource.isPlaying)
        {
            return;
        }
        instance.audioSource.clip = clip;
        instance.audioSource.Play();
    }


    static public void SetVolume(float vol)
    {
        instance.audioSource.volume = vol;
    }

    static public float GetVolume()
    {
        return instance.audioSource.volume;
    }

}
