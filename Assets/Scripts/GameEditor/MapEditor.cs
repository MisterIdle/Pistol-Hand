using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MapEditor : BaseManager
{
    public BlockDatabase blockDatabase;

    private List<PlacedBlock> placedBlocks = new();
    private GameObject ghostBlock;
    private int currentBlockIndex = 0;

    private Camera cam;
    private Vector3 lastPlacedPos = Vector3.positiveInfinity;
    private Vector3 lastRemovedPos = Vector3.positiveInfinity;

    private void Start()
    {
        cam = Camera.main;
        CreateGhostBlock();

        GameManager.SetGameState(GameState.Editor);
    }

    private void Update()
    {
        Vector3 gridPos = GetMouseGridPosition();
        HandleScrollInput();

        if (Input.GetMouseButton(0) && gridPos != lastPlacedPos)
        {
            PlaceBlock(gridPos);
            lastPlacedPos = gridPos;
        }

        if (Input.GetMouseButton(1) && gridPos != lastRemovedPos)
        {
            RemoveBlock(gridPos);
            lastRemovedPos = gridPos;
        }

        UpdateGhostBlock(gridPos);

        if (Input.GetKeyDown(KeyCode.F1))
            SaveMap();
    }

    private void HandleScrollInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            currentBlockIndex = (currentBlockIndex + (int)Mathf.Sign(scroll)) % blockDatabase.blocks.Count;
            if (currentBlockIndex < 0) currentBlockIndex += blockDatabase.blocks.Count;

            Destroy(ghostBlock);
            CreateGhostBlock();
        }
    }

    private void PlaceBlock(Vector3 pos)
    {
        if (IsBlockAt(pos)) return;

        var blockData = blockDatabase.blocks[currentBlockIndex];
        GameObject obj = Instantiate(blockData.prefab, pos, Quaternion.identity);
        placedBlocks.Add(new PlacedBlock { id = blockData.id, instance = obj });
    }

    private void RemoveBlock(Vector3 pos)
    {
        for (int i = placedBlocks.Count - 1; i >= 0; i--)
        {
            if (placedBlocks[i].instance != null && placedBlocks[i].instance.transform.position == pos)
            {
                Destroy(placedBlocks[i].instance);
                placedBlocks.RemoveAt(i);
                return;
            }
        }
    }

    private void CreateGhostBlock()
    {
        var prefab = blockDatabase.blocks[currentBlockIndex].prefab;
        ghostBlock = Instantiate(prefab);
        foreach (var r in ghostBlock.GetComponentsInChildren<SpriteRenderer>())
        {
            r.color = new Color(r.color.r, r.color.g, r.color.b, 0.4f);
        }
        foreach (var c in ghostBlock.GetComponentsInChildren<Collider2D>())
        {
            c.enabled = false;
        }
    }

    private void UpdateGhostBlock(Vector3 pos)
    {
        if (ghostBlock != null)
            ghostBlock.transform.position = pos;
    }

    private Vector3 GetMouseGridPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        return SnapToGrid(worldPos);
    }

    private Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / GameManager.GridSize) * GameManager.GridSize;
        float y = Mathf.Round(pos.y / GameManager.GridSize) * GameManager.GridSize;
        return new Vector3(x, y, 0f);
    }

    private bool IsBlockAt(Vector3 pos)
    {
        foreach (var block in placedBlocks)
        {
            if (block.instance != null && block.instance.transform.position == pos)
                return true;
        }
        return false;
    }

    private void SaveMap()
    {
        var mapData = new MapData();

        foreach (var block in placedBlocks)
        {
            if (block.instance != null)
            {
                mapData.blocks.Add(new BlockData
                {
                    blockID = block.id,
                    position = block.instance.transform.position
                });
            }
        }

        string folder = Application.dataPath + "/Save";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string path = folder + "/map_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
        File.WriteAllText(path, JsonUtility.ToJson(mapData, true));
        Debug.Log("Map saved to: " + path);
    }

    private class PlacedBlock
    {
        public string id;
        public GameObject instance;
    }
}
