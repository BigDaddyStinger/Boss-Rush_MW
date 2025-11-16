using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundEffectsManager : MonoBehaviour
{
    public static SoundEffectsManager instance;
    [SerializeField] AudioMixerGroup sfxMixer;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    public void PlayAudioClip(AudioClip clip, bool randomizePitch = false)
    {
        GameObject tempAudio = new GameObject();
        tempAudio.AddComponent<AudioSource>();
        AudioSource source = tempAudio.GetComponent<AudioSource>();
        if(randomizePitch)
            source.pitch = Random.Range(0.85f, 1.15f);
        source.clip = clip;
        source.outputAudioMixerGroup = sfxMixer;
        source.Play();
        Destroy(tempAudio, source.clip.length);
    }

    public void PlayRandomClip(AudioClip[] clips, bool randomizePitch = false)
    {
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayAudioClip(clip, randomizePitch);
    }
}
