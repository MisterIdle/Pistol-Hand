using UnityEngine;

public enum BlockType
{
    Spawn,
    Platform,
    Spike,
    Spring,
    Crate,
}

[System.Serializable]
public class BlockData
{
    public BlockType type;
    public GameObject prefab;
}

