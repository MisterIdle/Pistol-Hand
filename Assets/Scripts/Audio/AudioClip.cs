using UnityEngine;

[System.Serializable]
public struct MusicClip
{
    public MusicType type;
    public AudioClip clip;
}

[System.Serializable]
public struct SFXClipGroup
{
    public SFXType type;
    public AudioClip[] clips;
}