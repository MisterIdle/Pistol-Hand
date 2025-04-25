using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapEditor : BaseManager
{
    public static MapEditor Instance { get; private set; }

    public BlockDatabase blockDatabase;
    public GameObject map;

    [SerializeField] private Vector2 buildAreaMin = new(-9, -5);
    [SerializeField] private Vector2 buildAreaMax = new(9, 5);

    public bool mirrorEnabled = true;

    private List<PlacedBlock> placedBlocks = new();
    private GameObject ghostBlock;
    private GameObject ghostMirrorBlock;
    private int currentBlockIndex = 0;

    private Camera cam;
    private Vector3 lastPlacedPos = Vector3.positiveInfinity;
    private Vector3 lastRemovedPos = Vector3.positiveInfinity;

    void Awake()
    {
        InitializeSingleton();
        GameManager.SetGameState(GameState.Editor);
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        cam = Camera.main;
        CreateGhostBlock();

        HUDManager.EnableHUD(true);

        StartCoroutine(CameraManager.ChangeCameraLens(6.5f, 0f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(0f, -1f, -10), 0f));

        HUDManager.editorButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (MapTester.InTestMode || HUDEditorManager.messageUI.activeSelf) return;

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

    void HandleScrollInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0) return;

        currentBlockIndex = (currentBlockIndex + (int)Mathf.Sign(scroll)) % blockDatabase.blocks.Count;
        if (currentBlockIndex < 0) currentBlockIndex += blockDatabase.blocks.Count;

        Destroy(ghostBlock);
        Destroy(ghostMirrorBlock);
        CreateGhostBlock();
    }

    void PlaceBlock(Vector3 pos)
    {
        if (!IsInBuildArea(pos) || IsBlockAt(pos)) return;

        var blockData = blockDatabase.blocks[currentBlockIndex];
        GameObject obj = Instantiate(blockData.prefab, pos, Quaternion.identity, map.transform);
        placedBlocks.Add(new PlacedBlock { id = blockData.id, instance = obj });

        if (mirrorEnabled)
        {
            Vector3 mirrorPos = GetMirrorPosition(pos);
            if (IsInBuildArea(mirrorPos) && !IsBlockAt(mirrorPos))
            {
                GameObject mirrorObj = Instantiate(blockData.prefab, mirrorPos, Quaternion.identity, map.transform);
                placedBlocks.Add(new PlacedBlock { id = blockData.id, instance = mirrorObj });
            }
        }

        RefreshAllTiles();
    }

    void RemoveBlock(Vector3 pos)
    {
        Vector3 mirrorPos = mirrorEnabled ? GetMirrorPosition(pos) : Vector3.positiveInfinity;

        placedBlocks.RemoveAll(b =>
        {
            if (b.instance == null) return false;

            Vector3 bPos = b.instance.transform.position;
            if (bPos == pos || (mirrorEnabled && bPos == mirrorPos))
            {
                Destroy(b.instance);
                return true;
            }
            return false;
        });

        RefreshAllTiles();
    }

    void CreateGhostBlock()
    {
        var prefab = blockDatabase.blocks[currentBlockIndex].prefab;

        ghostBlock = Instantiate(prefab);
        ghostMirrorBlock = Instantiate(prefab);

        foreach (var r in ghostBlock.GetComponentsInChildren<SpriteRenderer>())
            r.color = new Color(r.color.r, r.color.g, r.color.b, 0.4f);
        foreach (var c in ghostBlock.GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        foreach (var r in ghostMirrorBlock.GetComponentsInChildren<SpriteRenderer>())
            r.color = new Color(r.color.r, r.color.g, r.color.b, 0.4f);
        foreach (var c in ghostMirrorBlock.GetComponentsInChildren<Collider2D>())
            c.enabled = false;
    }

    void UpdateGhostBlock(Vector3 pos)
    {
        if (ghostBlock == null || ghostMirrorBlock == null) return;

        ghostBlock.transform.position = pos;
        ghostBlock.SetActive(IsInBuildArea(pos));

        if (mirrorEnabled)
        {
            Vector3 mirrorPos = GetMirrorPosition(pos);
            ghostMirrorBlock.transform.position = mirrorPos;
            ghostMirrorBlock.SetActive(IsInBuildArea(mirrorPos));
        }
        else
        {
            ghostMirrorBlock.SetActive(false);
        }
    }

    Vector3 GetMirrorPosition(Vector3 pos)
    {
        float centerX = (buildAreaMin.x + buildAreaMax.x) / 2f;
        float distanceFromCenter = pos.x - centerX;
        float mirroredX = centerX - distanceFromCenter;
        return new Vector3(mirroredX, pos.y, 0f);
    }

    Vector3 GetMouseGridPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        return SnapToGrid(cam.ScreenToWorldPoint(mousePos));
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        float size = GameManager.GridSize;
        float x = Mathf.Round(pos.x / size) * size;
        float y = Mathf.Round(pos.y / size) * size;
        return new Vector3(x, y, 0f);
    }

    bool IsBlockAt(Vector3 pos) => placedBlocks.Any(b => b.instance != null && b.instance.transform.position == pos);

    bool IsInBuildArea(Vector3 pos)
    {
        return pos.x >= buildAreaMin.x && pos.x <= buildAreaMax.x &&
               pos.y >= buildAreaMin.y && pos.y <= buildAreaMax.y;
    }

    public void RefreshAllTiles()
    {
        TileManager.RefreshAllTiles(placedBlocks);
    }

    public bool CompletMap()
    {
        return placedBlocks.Any(b => b.id == "4");
    }

    public List<PlacedBlock> GetPlacedBlocks()
    {
        return placedBlocks;
    }

    public void LoadBlocksFromSaveData(List<SaveManager.MapSaveData.BlockData> loadedBlocks)
    {
        foreach (var placedBlock in placedBlocks)
        {
            if (placedBlock.instance != null)
            {
                Destroy(placedBlock.instance);
            }
        }
        placedBlocks.Clear();

        foreach (var blockData in loadedBlocks)
        {
            var blockPrefab = blockDatabase.blocks.FirstOrDefault(b => b.id == blockData.id)?.prefab;
            if (blockPrefab != null)
            {
                GameObject blockInstance = Instantiate(blockPrefab, blockData.position, Quaternion.identity, map.transform);
                placedBlocks.Add(new PlacedBlock { id = blockData.id, instance = blockInstance });
            }
            else
            {
                Debug.LogWarning($"Prefab for block ID {blockData.id} not found!");
            }
        }

        RefreshAllTiles();
    }

    public void ClearMap()
    {
        foreach (var placedBlock in placedBlocks)
        {
            if (placedBlock.instance != null)
            {
                Destroy(placedBlock.instance);
            }
        }
        placedBlocks.Clear();
        RefreshAllTiles();
    }


    public class PlacedBlock
    {
        public string id;
        public GameObject instance;
    }
}
