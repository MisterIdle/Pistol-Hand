using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : BaseManager
{
    public static AudioManager instance;

    [Header("Audio")]
    public AudioMixer AudioMixer;
    public Slider MasterVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider SFXVolumeSlider;

    [Header("Music")]
    public AudioSource MusicSource;
    public AudioClip[] AudioClips;

    [Header("SFX")]
    public AudioSource SFXSource;
    public AudioClip[] SFXClips;

    private void Awake()
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
    }

    public void PlayMusic(int trackIndex)
    {
        if (trackIndex >= 0 && trackIndex < AudioClips.Length)
        {
            MusicSource.clip = AudioClips[trackIndex];
            MusicSource.Play();
        }
    }

    public void StopMusic()
    {
        MusicSource.Stop();
    }

    public void PlaySFX(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < SFXClips.Length)
        {
            SFXSource.PlayOneShot(SFXClips[clipIndex]);
        }
    }

    public void OnSetSFXVolume(float volume)
    {
        AudioMixer.SetFloat("SFXVolume", volume);
    }

    public void OnSetMusicVolume(float volume)
    {
        AudioMixer.SetFloat("MusicVolume", volume);
    }

    public void OnSetMasterVolume(float volume)
    {
        AudioMixer.SetFloat("MasterVolume", volume);
    }
}
