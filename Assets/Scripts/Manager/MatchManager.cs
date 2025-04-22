using System.Collections;
using UnityEngine;
using System.IO;


public class MatchManager : BaseManager
{
    public static MatchManager Instance { get; private set; }
    
    [Header("Spawn System")]
    public bool IsLoading = false;
    public bool FirstMatch = true;

    [Header("Map System")]
    public BlockDatabase blockDatabase;
    public GameObject blocks;


    private void Awake()
    {
        InitializeSingleton();
        GameManager.SetGameState(GameState.Playing);
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

    public void InMatch()
    {
        if (GameManager.CheckPlayer())
            StartCoroutine(NewMatch());
    }

    public IEnumerator NewMatch()
    {
        if (IsLoading) yield break;
        IsLoading = true;

        if (FirstMatch)
        {
            foreach (var p in GameManager.GetAllPlayers())
                p.Wins = 0;

            FirstMatch = false;
        }
        else
        {
            foreach (var p in GameManager.GetAllPlayers())
            {
                if (p.IsDead) continue;
                p.Wins++;

                if (p.Wins >= GameManager.NeedToWin)
                {
                    Debug.Log($"Player {p.PlayerID} wins the match!");
                    IsLoading = false;
                    StartCoroutine(TeleportToTrophy());
                    yield break;
                }
            }
        }

        yield return new WaitForSeconds(1f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        LoadRandomMap();

        yield return new WaitForSeconds(1f);

        GameManager.ResetAllPlayers();
        GameManager.SetSpawnPoints();
        GameManager.PlaceAllPlayers();

        yield return CameraManager.MoveCameraTransition(false, 1f);
        IsLoading = false;
    }

    private IEnumerator TeleportToTrophy()
    {
        if (IsLoading) yield break;
        IsLoading = true;

        GameManager.SetGameState(GameState.Trophy);

        yield return new WaitForSeconds(1f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        IsLoading = false;
        yield return SceneLoader.LoadScene(GameManager.TrophySceneName);
        
    }

    private void LoadRandomMap()
    {
        string directoryPath = Application.dataPath + "/Save/";
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError("Save directory not found: " + directoryPath);
            return;
        }   

        string[] files = Directory.GetFiles(directoryPath, "*.json");
        if (files.Length == 0)
        {
            Debug.LogError("No map files found in directory: " + directoryPath);
            return;
        }   

        foreach (Transform child in blocks.transform)
            Destroy(child.gameObject);  

        string randomFile = files[Random.Range(0, files.Length)];
        Debug.Log("Loading random map: " + randomFile); 

        string json = File.ReadAllText(randomFile);
        MapData mapData = JsonUtility.FromJson<MapData>(json);  

        foreach (var data in mapData.blocks)
        {
            var blockData = blockDatabase.blocks.Find(b => b.id == data.blockID);
            if (blockData != null)
            {
                Vector3 alignedPos = SnapToGrid(data.position);
                Instantiate(blockData.prefab, alignedPos, Quaternion.identity, blocks.transform);
            }
        }
    }


    private Vector3 SnapToGrid(Vector3 pos)
    {
        float size = GameManager.GridSize;
        float x = Mathf.Round(pos.x / size) * size;
        float y = Mathf.Round(pos.y / size) * size;
        return new Vector3(x, y, 0f);
    }

}