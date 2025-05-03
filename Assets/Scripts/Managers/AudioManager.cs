using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : BaseManager
{
    public static AudioManager instance;

    [Header("Audio")]
    public AudioMixer AudioMixer;
    [Range(0f, 1f)] private float _volume = 1.0f;
    public float VolumeStep = 0.05f;
    public float MinVolume = -40f;
    public float MaxVolume = 10f;

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

    public void SetVolume(float volume)
    {
        AudioMixer.SetFloat("MasterVolume", Mathf.Lerp(MinVolume, MaxVolume, volume));
    }

    public void IncreaseVolume()
    {
        _volume = Mathf.Min(_volume + VolumeStep, 1f);
        SetVolume(_volume);
    }

    public void DecreaseVolume()
    {
        _volume = Mathf.Max(_volume - VolumeStep, 0f);
        SetVolume(_volume);
    }
}
