using System.Collections.Generic;
using UnityEngine;

public static class BlockLoader
{
    public static List<PlacedBlock> LoadBlocks(
        List<MapSaveData.BlockData> loadedBlocks,
        BlockDatabase database,
        Transform parent
    )
    {
        var placedBlocks = new List<PlacedBlock>(loadedBlocks.Count);
        var blockPrefabs = new Dictionary<BlockType, GameObject>();

        foreach (var blockData in loadedBlocks)
        {
            if (!blockPrefabs.ContainsKey(blockData.type))
            {
                var blockInfo = database.Get(blockData.type);
                if (blockInfo != null && blockInfo.prefab != null)
                {
                    blockPrefabs[blockData.type] = blockInfo.prefab;
                }
                else
                {
                    Debug.LogWarning($"Prefab for block type {blockData.type} not found!");
                    blockPrefabs[blockData.type] = null;
                }
            }

            var prefab = blockPrefabs[blockData.type];
            if (prefab != null)
            {
                var blockInstance = Object.Instantiate(prefab, blockData.position, Quaternion.identity, parent);
                placedBlocks.Add(new PlacedBlock
                {
                    type = blockData.type,
                    instance = blockInstance
                });
            }
        }

        return placedBlocks;
    }
}
