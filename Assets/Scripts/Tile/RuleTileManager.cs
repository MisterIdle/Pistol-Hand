using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class TileManager
{
    public static void RefreshAllTiles(List<PlacedBlock> placedBlocks)
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

    public static string GetSpriteNameAtPosition(Vector3 position, List<PlacedBlock> placedBlocks)
    {
        Vector2Int gridPos = ToGridPosition(position);
        RuleTiteApply tile = placedBlocks
            .Where(block => block.instance != null)
            .Select(block => new { Block = block, Tile = block.instance.GetComponent<RuleTiteApply>() })
            .FirstOrDefault(pair => ToGridPosition(pair.Block.instance.transform.position) == gridPos)?.Tile;

        if (tile != null)
        {
            SpriteRenderer spriteRenderer = tile.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer.sprite.name;
            }
        }
        return null;
    }

    private static Vector2Int ToGridPosition(Vector3 pos)
    {
        float size = MapManager.Instance.GridSize;
        int x = Mathf.RoundToInt(pos.x / size);
        int y = Mathf.RoundToInt(pos.y / size);
        return new Vector2Int(x, y);
    }
}

