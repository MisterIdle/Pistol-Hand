using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio")]
    public AudioMixer audioMixer;
    [Range(0f, 1f)] private float volume = 1.0f;
    public float volumeStep = 0.05f;
    public float minVolume = -40f;
    public float maxVolume = 10f;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip[] audioClips;
    private int currentClipIndex = 0;

    private Vector2 dpadInput;
    
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

    private void Start()
    {
        PlayMusic(currentClipIndex);
    }

    private void Update()
    {
        HandleVolumeInput();
    }

    private void HandleVolumeInput()
    {
        if (dpadInput.x > 0 && volume < maxVolume)
        {
            volume += volumeStep;
            SetVolume(volume);
        }
        else if (dpadInput.x < 0 && volume > minVolume)
        {
            volume -= volumeStep;
            SetVolume(volume);
        }
    }

    private void SetVolume(float vol)
    {
        audioMixer.SetFloat("MasterVolume", vol);
    }

    public void OnAdjustVolumeFromPlayer(Vector2 dpad)
    {
        dpadInput.x = dpad.x;
    }

    public void PlayMusic(int clipIndex)
    {
        if (audioClips.Length == 0 || clipIndex < 0 || clipIndex >= audioClips.Length) return;

        currentClipIndex = clipIndex;
        musicSource.clip = audioClips[clipIndex];
        musicSource.loop = true;
        musicSource.Play();
    }
}