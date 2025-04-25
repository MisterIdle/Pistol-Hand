using System.Collections.Generic;
using System.Linq;
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
            var blockPrefab = database.blocks.FirstOrDefault(b => b.id == blockData.id)?.prefab;
            if (blockPrefab != null)
            {
                var blockInstance = Object.Instantiate(blockPrefab, blockData.position, Quaternion.identity, parent);
                placedBlocks.Add(new MapEditor.PlacedBlock { id = blockData.id, instance = blockInstance });
            }
            else
            {
                Debug.LogWarning($"Prefab for block ID {blockData.id} not found!");
            }
        }

        return placedBlocks;
    }
}
