using UnityEngine;
using System.Collections.Generic;

public static class TileManager
{
    public static void RefreshAllTiles(List<MapEditor.PlacedBlock> placedBlocks)
    {
        Dictionary<Vector2Int, RuleTiteApply> tileMap = new();

        foreach (var block in placedBlocks)
        {
            if (block.instance == null) continue;
            RuleTiteApply tile = block.instance.GetComponent<RuleTiteApply>();
            if (tile == null) continue;

            Vector2Int gridPos = ToGridPosition(block.instance.transform.position);
            tileMap[gridPos] = tile;
        }

        foreach (var pair in tileMap)
        {
            Vector2Int pos = pair.Key;
            RuleTiteApply tile = pair.Value;

            Dictionary<Vector3, bool> neighbors = new()
            {
                { Vector3.up, tileMap.ContainsKey(pos + Vector2Int.up) },
                { Vector3.down, tileMap.ContainsKey(pos + Vector2Int.down) },
                { Vector3.left, tileMap.ContainsKey(pos + Vector2Int.left) },
                { Vector3.right, tileMap.ContainsKey(pos + Vector2Int.right) }
            };

            tile.UpdateSprite(neighbors);
        }
    }

    private static Vector2Int ToGridPosition(Vector3 pos)
    {
        float size = GameManager.Instance.GridSize;
        int x = Mathf.RoundToInt(pos.x / size);
        int y = Mathf.RoundToInt(pos.y / size);
        return new Vector2Int(x, y);
    }
}
