using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapEditor : BaseManager
{
    public static MapEditor Instance { get; private set; }

    [Header("References")]
    public BlockDatabase BlockDatabase => MapManager.Instance.blockDatabase;
    public GameObject Map;
    public GameObject Grid;
    public GameObject Gridblock;

    private Vector2 _buildAreaMin = new(-9, -5);
    private Vector2 _buildAreaMax = new(9, 5);

    public bool MirrorEnabled = true;
    public bool GridEnabled = true;

    public bool HasBeenTestedAndValid = false;

    private List<PlacedBlock> _placedBlocks = new();
    private List<GameObject> _gridBlocks = new();

    private GameObject _ghostBlock;
    private GameObject _ghostMirrorBlock;

    private int _currentBlockIndex = 0;

    private Camera _cam;
    private Vector3 _lastPlacedPos = Vector3.positiveInfinity;
    private Vector3 _lastRemovedPos = Vector3.positiveInfinity;

    void Awake()
    {
        InitializeSingleton();
        GameManager.SetGameState(GameState.Editor);

        AudioManager.PlayMusic(MusicType.Editor);

        foreach (Transform child in MapManager.MapTile.transform)
            Destroy(child.gameObject);

        StarGenerator.Instance.ClearStars();

        if (MapManager.MapTile != null)
        {
            Map = MapManager.MapTile.gameObject;
        }
        else
        {
            Debug.LogError("MapTile is not assigned in MapManager.");
        }
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
        _cam = Camera.main;

        CreateGhostBlock();
        HUDManager.SetTransition(true);

        StartCoroutine(CameraManager.ChangeCameraLens(6.5f, 0f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(0f, -0.5f, -10), 0f));

        GameManager.PlayerCount = 0;
        GameManager.PlayerDeath = 0;

        HUDManager.DeleteAllPlayerCards();
        SkinManager.ClearAssignedColors();

        GenerateGrid();
    }

    void Update()
    {
        if (MapTester.InTestMode || HUDEditorManager.MessageUIObject.activeSelf) return;

        if (HUDManager.IsPaused) return;

        Vector3 gridPos = GetMouseGridPosition();

        HandleScrollInput();

        if (Input.GetMouseButton(0) && gridPos != _lastPlacedPos)
        {
            PlaceBlock(gridPos);
            _lastPlacedPos = gridPos;
            _lastRemovedPos = Vector3.positiveInfinity;
        }

        if (Input.GetMouseButton(1) && gridPos != _lastRemovedPos)
        {
            RemoveBlock(gridPos);
            _lastRemovedPos = gridPos;
            _lastPlacedPos = Vector3.positiveInfinity;
        }

        if(Input.GetMouseButton(2))
        {
            GetBlockAt(gridPos);
        }

        if (GridEnabled)
            GenerateGrid();
        else
            HideGrid();

        UpdateGhostBlock(gridPos);
        SetCratePhysics(false);
    }

    void PlaceBlock(Vector3 pos)
    {
        if (!IsInBuildArea(pos) || IsBlockAt(pos)) return;

        var blockData = BlockDatabase.BlockList[_currentBlockIndex];

        var spawnPositions = _placedBlocks
            .Where(b => b.type == BlockType.Spawn)
            .Select(b => SnapToGrid(b.position))
            .ToList();

        if (blockData.type == BlockType.Crate)
        {
            foreach (var spawnPos in spawnPositions)
            {
                float distance = Vector2.Distance(pos, spawnPos);
                bool isAboveSpawn = Mathf.Approximately(pos.x, spawnPos.x) && pos.y > spawnPos.y;

                if (distance <= MapManager.GridSize || isAboveSpawn)
                    return;
            }
        }

        GameObject obj = Instantiate(blockData.prefab, pos, Quaternion.identity, Map.transform);
        _placedBlocks.Add(new PlacedBlock { type = blockData.type, instance = obj, position = pos });

        if (MirrorEnabled)
        {
            Vector3 mirrorPos = GetMirrorPosition(pos);
            if (IsInBuildArea(mirrorPos) && !IsBlockAt(mirrorPos))
            {
                GameObject mirrorObj = Instantiate(blockData.prefab, mirrorPos, Quaternion.identity, Map.transform);
                _placedBlocks.Add(new PlacedBlock { type = blockData.type, instance = mirrorObj, position = mirrorPos });
            }
        }

        HasBeenTestedAndValid = false;
        RefreshAllTiles();
    }

    void RemoveBlock(Vector3 pos)
    {
        Vector3 mirrorPos = MirrorEnabled ? GetMirrorPosition(pos) : Vector3.positiveInfinity;

        _placedBlocks.RemoveAll(b =>
        {
            if (b.instance == null) return false;

            Vector3 bPos = b.instance.transform.position;
            if (bPos == pos || (MirrorEnabled && bPos == mirrorPos))
            {
                Destroy(b.instance);
                return true;
            }
            return false;
        });

        HasBeenTestedAndValid = false;
        RefreshAllTiles();
    }

    PlacedBlock GetBlockAt(Vector2 pos)
    {
        var block = _placedBlocks.FirstOrDefault(b => b.instance != null && (Vector2)b.instance.transform.position == pos);
        if (block != null)
        {
            SetCurrentBlockByType(block.type);
        }

        return block;
    }

    void CreateGhostBlock()
    {
        var prefab = BlockDatabase.BlockList[_currentBlockIndex].prefab;

        _ghostBlock = Instantiate(prefab);
        _ghostMirrorBlock = Instantiate(prefab);

        SetGhostVisuals(_ghostBlock);
        SetGhostVisuals(_ghostMirrorBlock);
    }

    void SetGhostVisuals(GameObject ghost)
    {
        foreach (var r in ghost.GetComponentsInChildren<SpriteRenderer>())
            r.color = new Color(r.color.r, r.color.g, r.color.b, 0.4f);
        foreach (var c in ghost.GetComponentsInChildren<Collider2D>())
            c.enabled = false;
    }

    void UpdateGhostBlock(Vector3 pos)
    {
        if (_ghostBlock == null || _ghostMirrorBlock == null) return;

        _ghostBlock.transform.position = pos;
        _ghostBlock.SetActive(IsInBuildArea(pos));

        if (MirrorEnabled)
        {
            Vector3 mirrorPos = GetMirrorPosition(pos);
            _ghostMirrorBlock.transform.position = mirrorPos;
            _ghostMirrorBlock.SetActive(IsInBuildArea(mirrorPos));
        }
        else
        {
            _ghostMirrorBlock.SetActive(false);
        }
    }

    void HandleScrollInput()
    {
        int blockCount = BlockDatabase.BlockList.Count;

        if (Input.mouseScrollDelta.y < 0)
        {
            _currentBlockIndex = (_currentBlockIndex + 1) % blockCount;
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            _currentBlockIndex = (_currentBlockIndex - 1 + blockCount) % blockCount;
        }

        HUDEditorManager.HighlightBlockTypeButton(BlockDatabase.BlockList[_currentBlockIndex].type);

        Destroy(_ghostBlock);
        Destroy(_ghostMirrorBlock);
        CreateGhostBlock();
    }

    public void SetCurrentBlockByType(BlockType type)
    {
        _currentBlockIndex = BlockDatabase.BlockList.FindIndex(b => b.type == type);
        if (_currentBlockIndex == -1) return;

        Destroy(_ghostBlock);
        Destroy(_ghostMirrorBlock);
        CreateGhostBlock();
    }

    Vector3 GetMouseGridPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        return SnapToGrid(_cam.ScreenToWorldPoint(mousePos));
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        float size = MapManager.GridSize;
        float x = Mathf.Round(pos.x / size) * size;
        float y = Mathf.Round(pos.y / size) * size;
        return new Vector3(x, y, 0f);
    }

    public void GenerateGrid()
    {
        if (_gridBlocks.Count > 0) return;

        for (float x = _buildAreaMin.x; x <= _buildAreaMax.x; x += MapManager.GridSize)
        {
            for (float y = _buildAreaMin.y; y <= _buildAreaMax.y; y += MapManager.GridSize)
            {
                GameObject gridBlock = Instantiate(Gridblock, new Vector3(x, y, 0), Quaternion.identity, Grid.transform);
                _gridBlocks.Add(gridBlock);
            }
        }
    }

    public void ShowGrid() => _gridBlocks.ForEach(g => g.SetActive(true));
    public void HideGrid() => _gridBlocks.ForEach(g => g.SetActive(false));


    public MapValidationResult ValidateMap()
    {
        var spawnBlocks = _placedBlocks.Where(b => b.type == BlockType.Spawn).ToList();
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
            Vector3 currentPos = spawn.position + Vector3.down * MapManager.GridSize;

            while (IsInBuildArea(currentPos))
            {
                var blockBelow = _placedBlocks.FirstOrDefault(b => b.position == currentPos);
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

                currentPos += Vector3.down * MapManager.GridSize;
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

        return new MapValidationResult { isValid = true, errorMessage = string.Empty };
    }

    public void LoadBlocksFromSaveData(List<MapSaveData.BlockData> loadedBlocks)
    {
        ClearMap();

        foreach (var data in loadedBlocks)
        {
            var blockData = BlockDatabase.BlockList.FirstOrDefault(b => b.type == data.type);
            if (blockData != null)
            {
                GameObject obj = Instantiate(blockData.prefab, data.position, Quaternion.identity, Map.transform);
                _placedBlocks.Add(new PlacedBlock { type = data.type, instance = obj, position = data.position });
            }
        }

        SetCratePhysics(false);

        RefreshAllTiles();
    }

    public void ClearMap()
    {
        foreach (var placedBlock in _placedBlocks)
        {
            if (placedBlock.instance != null)
            {
                Destroy(placedBlock.instance);
            }
        }
        _placedBlocks.Clear();
        RefreshAllTiles();
    }

    public void RefreshAllTiles()
    {
        TileManager.RefreshAllTiles(_placedBlocks);
    }

    public void ToggleMirror()
    {
        MirrorEnabled = !MirrorEnabled;
        _ghostMirrorBlock.SetActive(MirrorEnabled);
    }

    public void ToggleGrid()
    {
        if (!MapTester.InTestMode)
        {
            GridEnabled = !GridEnabled;

            if (GridEnabled)
                ShowGrid();
            else
                HideGrid();
        }
    }

    public void SetCratePhysics(bool enabled)
    {
        foreach (var placedBlock in _placedBlocks)
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
        var cratePositions = _placedBlocks
            .Where(b => b.type == BlockType.Crate && b.instance != null)
            .Select(b => b.position)
            .ToList();

        _placedBlocks.RemoveAll(b =>
        {
            if (b.type == BlockType.Crate && b.instance != null)
            {
                Destroy(b.instance);
                return true;
            }
            return false;
        });

        var blockData = BlockDatabase.BlockList.FirstOrDefault(b => b.type == BlockType.Crate);
        if (blockData == null) return;

        foreach (var pos in cratePositions)
        {
            GameObject obj = Instantiate(blockData.prefab, pos, Quaternion.identity, Map.transform);
            _placedBlocks.Add(new PlacedBlock { type = BlockType.Crate, instance = obj, position = pos });
        }

        RefreshAllTiles();
    }

    Vector3 GetMirrorPosition(Vector3 pos)
    {
        float centerX = (_buildAreaMin.x + _buildAreaMax.x) / 2f;
        float distanceFromCenter = pos.x - centerX;
        float mirroredX = centerX - distanceFromCenter;
        return new Vector3(mirroredX, pos.y, 0f);
    }

    bool IsBlockAt(Vector3 pos)
    {
        return _placedBlocks.Any(b => b.instance != null && b.instance.transform.position == pos);
    }

    bool IsInBuildArea(Vector3 pos)
    {
        return pos.x >= _buildAreaMin.x && pos.x <= _buildAreaMax.x &&
               pos.y >= _buildAreaMin.y && pos.y <= _buildAreaMax.y;
    }

    public List<PlacedBlock> GetPlacedBlocks() => _placedBlocks;
}
