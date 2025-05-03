using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockDatabase", menuName = "LevelEditor/BlockDatabase")]
public class BlockDatabase : ScriptableObject
{
    public List<BlockData> BlockList;

    private Dictionary<BlockType, BlockData> lookup;

    public void Init()
    {
        lookup = new();
        foreach (var block in BlockList)
        {
            lookup[block.type] = block;
        }
    }

    public BlockData Get(BlockType type)
    {
        if (lookup == null) Init();
        return lookup.TryGetValue(type, out var data) ? data : null;
    }

    public int GetIndex(BlockType type)
    {
        return BlockList.FindIndex(b => b.type == type);
    }
}
