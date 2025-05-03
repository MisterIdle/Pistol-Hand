    
using System.Collections.Generic;
using UnityEngine;
    
[System.Serializable]
public class MapSaveData
{
    public List<BlockData> placedBlocks;
    
    [System.Serializable]
    public class BlockData
    {
        public BlockType type;
        public Vector3 position;
    }
}