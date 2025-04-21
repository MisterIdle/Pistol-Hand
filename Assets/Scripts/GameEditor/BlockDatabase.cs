using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BlockDatabase", menuName = "MapEditor/BlockDatabase")]
public class BlockDatabase : ScriptableObject
{
    public List<BlockInfo> blocks;
}

[System.Serializable]
public class BlockInfo
{
    public string id;
    public GameObject prefab;
}
