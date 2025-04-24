using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

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
        
        RefreshAllTiles();
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

        RefreshAllTiles();
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
    
                var spriteRenderer = block.instance.GetComponentInChildren<SpriteRenderer>();
                string spriteName = spriteRenderer?.sprite?.name ?? "";
    
                mapData.blocks.Add(new BlockData
                {
                    blockID = block.id,
                    position = block.instance.transform.position,
                    spriteName = spriteName
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
    
        string path = folder + "/map_" + mapName + ".dat";
    
        string json = JsonUtility.ToJson(mapData, true);
        byte[] compressed = Compress(json);
        byte[] encrypted = Encrypt(compressed, "my_super_secret_key_123");
    
        File.WriteAllBytes(path, encrypted);
    
        HUDEditorManager.Instance.SuccessMessage($"Carte \"{mapName}\" sauvegardée.");
    }
    
    private byte[] Compress(string data)
    {
        byte[] raw = Encoding.UTF8.GetBytes(data);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress)) 
            gzip.Write(raw, 0, raw.Length);
        return output.ToArray();
    }
    
    private byte[] Encrypt(byte[] data, string key)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16];
    
        using var encryptor = aes.CreateEncryptor();
        using var output = new MemoryStream();
        using (var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
            cryptoStream.Write(data, 0, data.Length);
        return output.ToArray();
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

    public void RefreshAllTiles()
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

    private Vector2Int ToGridPosition(Vector3 pos)
    {
        float size = GameManager.GridSize;
        int x = Mathf.RoundToInt(pos.x / size);
        int y = Mathf.RoundToInt(pos.y / size);
        return new Vector2Int(x, y);
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
