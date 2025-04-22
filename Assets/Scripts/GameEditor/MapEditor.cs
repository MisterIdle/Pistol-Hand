using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class MapEditor : BaseManager
{
    public static MapEditor Instance { get; private set; }

    public BlockDatabase blockDatabase;

    [SerializeField] private Vector2 buildAreaMin = new Vector2(-9, -5);
    [SerializeField] private Vector2 buildAreaMax = new Vector2(9, 5);

    private List<PlacedBlock> placedBlocks = new();
    private GameObject ghostBlock;
    private GameObject mirrorGhostBlock;
    private int currentBlockIndex = 0;

    private Camera cam;
    private Vector3 lastPlacedPos = Vector3.positiveInfinity;
    private Vector3 lastRemovedPos = Vector3.positiveInfinity;

    public void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        cam = Camera.main;
        CreateGhostBlock();

        GameManager.SetGameState(GameState.Editor);

        foreach (var player in GameManager.GetAllPlayers())
        {
            Destroy(player.gameObject);
        }

        HUDManager.EnableHUD(true);
        StartCoroutine(CameraManager.ChangeCameraLens(6.5f, 0f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(-1.5f, -1f, -10), 0f));
        HUDManager.Instance.editorButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (MapTester.Instance.InTestMode) return;

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

    public void SetBlockIndex(int index)
    {
        currentBlockIndex = index;
        Destroy(ghostBlock);
        CreateGhostBlock();
    }

    private void PlaceBlock(Vector3 pos)
    {
        if (!IsInBuildArea(pos) || IsBlockAt(pos) || HUDEditorManager.Instance.errorImage.gameObject.activeSelf) return;

        var blockData = blockDatabase.blocks[currentBlockIndex];
        GameObject obj = Instantiate(blockData.prefab, pos, Quaternion.identity);
        placedBlocks.Add(new PlacedBlock { id = blockData.id, instance = obj });

        if (HUDEditorManager.Instance.mirrorModeToggle.isOn)
        {
            Vector3 mirrorPos = new Vector3(-pos.x, pos.y, pos.z);
            if (IsInBuildArea(mirrorPos) && !IsBlockAt(mirrorPos))
            {
                GameObject mirrorObj = Instantiate(blockData.prefab, mirrorPos, Quaternion.identity);
                placedBlocks.Add(new PlacedBlock { id = blockData.id, instance = mirrorObj });
            }
        }
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
                break;
            }
        }

        if (HUDEditorManager.Instance.mirrorModeToggle.isOn)
        {
            Vector3 mirrorPos = new Vector3(-pos.x, pos.y, pos.z);
            for (int i = placedBlocks.Count - 1; i >= 0; i--)
            {
                if (placedBlocks[i].instance != null && placedBlocks[i].instance.transform.position == mirrorPos)
                {
                    Destroy(placedBlocks[i].instance);
                    placedBlocks.RemoveAt(i);
                    break;
                }
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

        if (mirrorGhostBlock != null)
        {
            Destroy(mirrorGhostBlock);
            mirrorGhostBlock = null;
        }
    }

    private void UpdateGhostBlock(Vector3 pos)
    {
        if (ghostBlock != null)
        {
            ghostBlock.transform.position = pos;
            ghostBlock.SetActive(IsInBuildArea(pos) && !HUDEditorManager.Instance.errorImage.gameObject.activeSelf);
        }

        if (HUDEditorManager.Instance.mirrorModeToggle.isOn && ghostBlock != null)
        {
            if (mirrorGhostBlock == null)
            {
                mirrorGhostBlock = Instantiate(blockDatabase.blocks[currentBlockIndex].prefab);
                foreach (var r in mirrorGhostBlock.GetComponentsInChildren<SpriteRenderer>())
                    r.color = new Color(r.color.r, r.color.g, r.color.b, 0.4f);
                foreach (var c in mirrorGhostBlock.GetComponentsInChildren<Collider2D>())
                    c.enabled = false;
            }

            Vector3 mirrorPos = new Vector3(-pos.x, pos.y, pos.z);
            mirrorGhostBlock.transform.position = mirrorPos;
            mirrorGhostBlock.SetActive(IsInBuildArea(mirrorPos));
        }
        else if (mirrorGhostBlock != null)
        {
            Destroy(mirrorGhostBlock);
            mirrorGhostBlock = null;
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
        string requiredBlockID = "4";
        int requiredCount = 4;
        int actualCount = 0;

        var mapData = new MapData();

        foreach (var block in placedBlocks)
        {
            if (block.instance != null)
            {
                if (block.id == requiredBlockID)
                    actualCount++;

                mapData.blocks.Add(new BlockData
                {
                    blockID = block.id,
                    position = block.instance.transform.position
                });
            }
        }

        if (actualCount < requiredCount)
        {
            HUDEditorManager.Instance.ErrorMessage($"Il faut au moins {requiredCount} blocs de type 'Spawn' pour sauvegarder la carte.");
            return;
        }

        string folder = Application.dataPath + "/Save";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string path = folder + "/map_" + mapName + ".json";
        File.WriteAllText(path, JsonUtility.ToJson(mapData, true));
        Debug.Log("Map saved to: " + path);

        HUDEditorManager.Instance.SuccessMessage($"Carte \"{mapName}\" sauvegardée.");
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

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(buildAreaMin.x, buildAreaMin.y), new Vector3(buildAreaMax.x, buildAreaMin.y));
        Gizmos.DrawLine(new Vector3(buildAreaMax.x, buildAreaMin.y), new Vector3(buildAreaMax.x, buildAreaMax.y));
        Gizmos.DrawLine(new Vector3(buildAreaMax.x, buildAreaMax.y), new Vector3(buildAreaMin.x, buildAreaMax.y));
        Gizmos.DrawLine(new Vector3(buildAreaMin.x, buildAreaMax.y), new Vector3(buildAreaMin.x, buildAreaMin.y));
    }
}
