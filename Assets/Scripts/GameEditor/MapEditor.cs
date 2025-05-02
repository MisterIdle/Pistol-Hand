using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapEditor : BaseManager
{
    public static MapEditor Instance { get; private set; }

    public BlockDatabase blockDatabase;
    public GameObject map;
    public GameObject grid;
    public GameObject gridblock;

    [SerializeField] private Vector2 buildAreaMin = new(-9, -5);
    [SerializeField] private Vector2 buildAreaMax = new(9, 5);

    public bool mirrorEnabled = true;
    public bool gridEnabled = true;

    private List<PlacedBlock> placedBlocks = new();
    private List<GameObject> gridBlocks = new();
    private GameObject ghostBlock;
    private GameObject ghostMirrorBlock;
    private int currentBlockIndex = 0;

    private Camera cam;
    private Vector3 lastPlacedPos = Vector3.positiveInfinity;
    private Vector3 lastRemovedPos = Vector3.positiveInfinity;

    public bool HasBeenTestedAndValid = false;

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
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(0f, -0.5f, -10), 0f));

        HUDManager.editorButton.gameObject.SetActive(false);

        GameManager.PlayerCount = 0;
        GameManager.PlayerDeath = 0;

        GenerateGrid();
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
            lastRemovedPos = Vector3.positiveInfinity;
        }

        if (Input.GetMouseButton(1) && gridPos != lastRemovedPos)
        {
            RemoveBlock(gridPos);
            lastRemovedPos = gridPos;
            lastPlacedPos = Vector3.positiveInfinity;
        }

        if (gridEnabled)
            GenerateGrid();
        else
            HideGrid();

        UpdateGhostBlock(gridPos);

        SetCratePhysics(false);
    }

    void HandleScrollInput()
    {
        if (Input.mouseScrollDelta.y < 0)
        {
            currentBlockIndex = (currentBlockIndex + 1) % blockDatabase.blockList.Count;
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            currentBlockIndex = (currentBlockIndex - 1 + blockDatabase.blockList.Count) % blockDatabase.blockList.Count;
        }

        HUDEditorManager.HighlightBlockTypeButton(blockDatabase.blockList[currentBlockIndex].type);

        Destroy(ghostBlock);
        Destroy(ghostMirrorBlock);
        CreateGhostBlock();
    }

    public void SetCurrentBlockByType(BlockType type)
    {
        currentBlockIndex = blockDatabase.blockList.FindIndex(b => b.type == type);
        if (currentBlockIndex == -1) return;

        Destroy(ghostBlock);
        Destroy(ghostMirrorBlock);
        CreateGhostBlock();
    }

    void PlaceBlock(Vector3 pos)
    {
        if (!IsInBuildArea(pos) || IsBlockAt(pos)) return;
    
        var blockData = blockDatabase.blockList[currentBlockIndex];

        var spawnPositions = placedBlocks
            .Where(b => b.type == BlockType.Spawn)
            .Select(b => SnapToGrid(b.position))
            .ToList();
        
        if (blockData.type == BlockType.Crate)
        {
            foreach (var spawnPos in spawnPositions)
            {
                float distance = Vector2.Distance(pos, spawnPos);
                bool isAboveSpawn = Mathf.Approximately(pos.x, spawnPos.x) && pos.y > spawnPos.y;
    
                if (distance <= GameManager.GridSize || isAboveSpawn)
                    return;
            }
        }
    
        GameObject obj = Instantiate(blockData.prefab, pos, Quaternion.identity, map.transform);
        placedBlocks.Add(new PlacedBlock { type = blockData.type, instance = obj, position = pos });
    
        if (mirrorEnabled)
        {
            Vector3 mirrorPos = GetMirrorPosition(pos);
            if (IsInBuildArea(mirrorPos) && !IsBlockAt(mirrorPos))
            {
                GameObject mirrorObj = Instantiate(blockData.prefab, mirrorPos, Quaternion.identity, map.transform);
                placedBlocks.Add(new PlacedBlock { type = blockData.type, instance = mirrorObj, position = mirrorPos });
            }
        }

        HasBeenTestedAndValid = false;
    
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

        HasBeenTestedAndValid = false;

        RefreshAllTiles();
    }

    void CreateGhostBlock()
    {
        var prefab = blockDatabase.blockList[currentBlockIndex].prefab;

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

    public MapValidationResult ValidateMap()
    {
        var spawnBlocks = placedBlocks.Where(b => b.type == BlockType.Spawn).ToList();
        if (spawnBlocks.Count < 4)
        {
            return new MapValidationResult
            {
                isValid = false,
                errorMessage = "There must be at least 4 spawn points."
            };
        }

        foreach (var spawn in spawnBlocks)
        {
            Vector3 currentPos = spawn.position + new Vector3(0, -GameManager.GridSize, 0);

            while (IsInBuildArea(currentPos))
            {
                var blockBelow = placedBlocks.FirstOrDefault(b => b.position == currentPos);
                if (blockBelow != null)
                {
                    if (blockBelow.type == BlockType.Platform)
                        break;
                    else
                    {
                        return new MapValidationResult
                        {
                            isValid = false,
                            errorMessage = "Each spawn must be above a platform."
                        };
                    }
                }

                currentPos += new Vector3(0, -GameManager.GridSize, 0);
            }

            if (!IsInBuildArea(currentPos))
            {
                return new MapValidationResult
                {
                    isValid = false,
                    errorMessage = "Each spawn must have a platform below within the build area."
                };
            }
        }

        return new MapValidationResult
        {
            isValid = true,
            errorMessage = string.Empty
        };
    }

    public List<PlacedBlock> GetPlacedBlocks()
    {
        return placedBlocks;
    }

    public void LoadBlocksFromSaveData(List<SaveManager.MapSaveData.BlockData> loadedBlocks)
    {
        ClearMap(); 

        foreach (var data in loadedBlocks)
        {
            var blockData = blockDatabase.blockList.FirstOrDefault(b => b.type == data.type);
            if (blockData != null)
            {
                GameObject obj = Instantiate(blockData.prefab, data.position, Quaternion.identity, map.transform);
                placedBlocks.Add(new PlacedBlock
                {
                    type = data.type,
                    instance = obj,
                    position = data.position
                });
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

    public void ToggleMirror()
    {
        mirrorEnabled = !mirrorEnabled;

        if (mirrorEnabled)
            ghostMirrorBlock.SetActive(true);
        else
            ghostMirrorBlock.SetActive(false);
    }
    
    public void ToggleGrid()
    {
        if (!MapTester.Instance.InTestMode)
        {
            gridEnabled = !gridEnabled;

            if (gridEnabled)
                ShowGrid();
            else
                HideGrid();
        }
    }


    public void GenerateGrid()
    {
        if (gridBlocks.Count > 0) return;

        for (float x = buildAreaMin.x; x <= buildAreaMax.x; x += GameManager.GridSize)
        {
            for (float y = buildAreaMin.y; y <= buildAreaMax.y; y += GameManager.GridSize)
            {
                GameObject gridBlock = Instantiate(gridblock, new Vector3(x, y, 0), Quaternion.identity, grid.transform);
                gridBlocks.Add(gridBlock);
            }
        }
    }

    public void ShowGrid()
    {
        foreach (var block in gridBlocks)
        {
            block.SetActive(true);
        }
    }

    public void HideGrid()
    {
        foreach (var block in gridBlocks)
        {
            block.SetActive(false);
        }
    }

    public void SetCratePhysics(bool enabled)
    {
        foreach (var placedBlock in placedBlocks)
        {
            if (placedBlock.instance != null && placedBlock.type == BlockType.Crate)
            {
                var rb = placedBlock.instance.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.simulated = enabled;
                }
            }
        }
    }

    public void ReplaceCrateAfterTest()
    {
        var cratePositions = placedBlocks
            .Where(b => b.type == BlockType.Crate && b.instance != null)
            .Select(b => b.position)
            .ToList();

        placedBlocks.RemoveAll(b =>
        {
            if (b.type == BlockType.Crate && b.instance != null)
            {
                Destroy(b.instance);
                return true;
            }
            return false;
        });

        var blockData = blockDatabase.blockList.FirstOrDefault(b => b.type == BlockType.Crate);
        if (blockData == null) return;

        foreach (var pos in cratePositions)
        {
            GameObject obj = Instantiate(blockData.prefab, pos, Quaternion.identity, map.transform);
            placedBlocks.Add(new PlacedBlock { type = BlockType.Crate, instance = obj, position = pos });
        }

        RefreshAllTiles();
    }


    public class PlacedBlock
    {
        public BlockType type;
        public GameObject instance;
        public Vector3 position;
    }

    public struct MapValidationResult
    {
        public bool isValid;
        public string errorMessage;
    }

}
