using System.Collections.Generic;
using UnityEngine;

public static class BlockLoader
{
    public static List<MapEditor.PlacedBlock> LoadBlocks(
        List<SaveManager.MapSaveData.BlockData> loadedBlocks,
        BlockDatabase database,
        Transform parent
    )
    {
        var placedBlocks = new List<MapEditor.PlacedBlock>();

        foreach (var blockData in loadedBlocks)
        {
            var blockInfo = database.Get(blockData.type);
            if (blockInfo != null && blockInfo.prefab != null)
            {
                var blockInstance = Object.Instantiate(blockInfo.prefab, blockData.position, Quaternion.identity, parent);
                placedBlocks.Add(new MapEditor.PlacedBlock
                {
                    type = blockData.type,
                    instance = blockInstance
                });
            }
            else
            {
                Debug.LogWarning($"Prefab for block type {blockData.type} not found!");
            }
        }

        return placedBlocks;
    }
}
