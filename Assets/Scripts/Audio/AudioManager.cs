using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-120)]
public class AudioManager : BaseManager
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioSource musicSource;
    public MusicClip[] musicClips;

    [Header("SFX")]
    public AudioSource sfxSource;
    public SFXClipGroup[] sfxClips;

    private Dictionary<MusicType, AudioClip> musicDict;
    private Dictionary<SFXType, AudioClip[]> sfxDict;

    private void Awake()
    {
        InitializeSingleton();
        InitDictionaries();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitDictionaries()
    {
        musicDict = new Dictionary<MusicType, AudioClip>();
        foreach (var music in musicClips)
        {
            musicDict[music.type] = music.clip;
        }

        sfxDict = new Dictionary<SFXType, AudioClip[]>();
        foreach (var sfx in sfxClips)
        {
            sfxDict[sfx.type] = sfx.clips;
        }
    }

    public void PlayMusic(MusicType type)
    {
        if (musicSource != null && musicDict.ContainsKey(type))
        {
            musicSource.clip = musicDict[type];
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(SFXType type)
    {
        if (sfxSource != null && sfxDict.ContainsKey(type))
        {
            var clips = sfxDict[type];
            if (clips.Length > 0)
            {
                var clip = clips[Random.Range(0, clips.Length)];
                sfxSource.PlayOneShot(clip);
            }
        }
    }

    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
