using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip[] musicClips;
    public AudioClip[] sfxClips;

    private Dictionary<string, AudioClip> musicDict;
    private Dictionary<string, AudioClip> sfxDict;

    public bool isPlayClearSound;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        musicDict = new Dictionary<string, AudioClip>();
        foreach (var clip in musicClips)
        {
            musicDict[clip.name] = clip;
        }

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var clip in sfxClips)
        {
            sfxDict[clip.name] = clip;
        }
    }

    public void PlayMusic(string clipName)
    {
        if (musicDict.ContainsKey(clipName))
        {
            musicSource.clip = musicDict[clipName];
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Music clip not found: " + clipName);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(string clipName)
    {
        if (sfxDict.ContainsKey(clipName))
        {
            sfxSource.PlayOneShot(sfxDict[clipName]);
        }
        else
        {
            Debug.LogWarning("SFX clip not found: " + clipName);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}