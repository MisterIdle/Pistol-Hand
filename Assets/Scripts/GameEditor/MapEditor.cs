using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MapEditor : BaseManager
{
    public BlockDatabase blockDatabase;

    // Interdiction de changer ses valeurs!
    private Vector2 buildAreaMin = new Vector2(-8, -5);
    private Vector2 buildAreaMax = new Vector2(8, 5);

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

        foreach (var player in GameManager.GetAllPlayers())
        {
            Destroy(player.gameObject);
        }

        CameraManager.ChangeCameraLens(6.5f);
        CameraManager.SetCameraPosition(new Vector3(0, -1f, -10));

        HUDManager.gameObject.SetActive(false);

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
        if (!IsInBuildArea(pos) || IsBlockAt(pos)) return;

        var blockData = blockDatabase.blocks[currentBlockIndex];
        GameObject obj = Instantiate(blockData.prefab, pos, Quaternion.identity);
        placedBlocks.Add(new PlacedBlock { id = blockData.id, instance = obj });
    }

    private void RemoveBlock(Vector3 pos)
    {
        if (!IsInBuildArea(pos)) return;

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
        {
            ghostBlock.transform.position = pos;
            ghostBlock.SetActive(IsInBuildArea(pos));
        }
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

    private bool IsInBuildArea(Vector3 pos)
    {
        return pos.x >= buildAreaMin.x && pos.x <= buildAreaMax.x &&
               pos.y >= buildAreaMin.y && pos.y <= buildAreaMax.y;
    }

    public void SaveMap(string mapName)
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

        string path = folder + "/map_" + mapName + ".json";
        File.WriteAllText(path, JsonUtility.ToJson(mapData, true));
        Debug.Log("Map saved to: " + path);
    }


    public void LoadMap(string mapName)
    {
        string path = Application.dataPath + "/Save/" + mapName + ".json";
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        MapData mapData = JsonUtility.FromJson<MapData>(json);

        foreach (var block in placedBlocks)
        {
            if (block.instance != null)
                Destroy(block.instance);
        }
        placedBlocks.Clear();

        foreach (var data in mapData.blocks)
        {
            var blockData = blockDatabase.blocks.Find(b => b.id == data.blockID);
            if (blockData != null)
            {
                GameObject obj = Instantiate(blockData.prefab, data.position, Quaternion.identity);
                placedBlocks.Add(new PlacedBlock { id = data.blockID, instance = obj });
            }
        }

        Debug.Log("Map loaded: " + mapName);
    }


    private class PlacedBlock
    {
        public string id;
        public GameObject instance;
    }

    // Draw gizmo for build area
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(buildAreaMin.x, buildAreaMin.y), new Vector3(buildAreaMax.x, buildAreaMin.y));
        Gizmos.DrawLine(new Vector3(buildAreaMax.x, buildAreaMin.y), new Vector3(buildAreaMax.x, buildAreaMax.y));
        Gizmos.DrawLine(new Vector3(buildAreaMax.x, buildAreaMax.y), new Vector3(buildAreaMin.x, buildAreaMax.y));
        Gizmos.DrawLine(new Vector3(buildAreaMin.x, buildAreaMax.y), new Vector3(buildAreaMin.x, buildAreaMin.y));
    }
}
