using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class MatchManager : BaseManager
{
    public static MatchManager Instance { get; private set; }

    [Header("Spawn System")]
    public bool IsLoading = false;
    public bool FirstMatch = true;
    public int countdownStart = 3;
    public int count;

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

    private IEnumerator NewMatch()
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
                    Debug.Log($"Player {SkinManager.Instance.GetPlayerColorName(p.PlayerID)} wins the match!");
                    IsLoading = false;
                    StartCoroutine(TeleportToTrophy());
                    yield break;
                }
            }
        }

        yield return new WaitForSeconds(1f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        yield return new WaitForSeconds(1f);

        LoadRandomMap();

        GameManager.ResetAllPlayers();
        GameManager.SetSpawnPoints();
        GameManager.PlaceAllPlayers();

        yield return CameraManager.MoveCameraTransition(false, 1f);
        IsLoading = false;
        StartCoroutine(StartMatch());
    }

    public IEnumerator StartMatch()
    {
        PlayerController[] players = GameManager.GetAllPlayers();
        count = countdownStart; 
        foreach (var p in players) p.CanMove(false);
        while (count > 0)
        {
            Debug.Log($"{count}");
            count--;
            yield return new WaitForSeconds(1f);
        }
        foreach (var p in players) p.CanMove(true);
        Debug.Log("Go");
    }

    public void LoadRandomMap()
    {
        var mapNames = SaveManager.GetAllMaps();
        if (mapNames.Count == 0)
        {
            Debug.LogWarning("No maps available to load.");
            return;
        }      

        string randomMapName = mapNames[Random.Range(0, mapNames.Count)];
        var loadedData = SaveManager.LoadMap(randomMapName);
        if (loadedData == null) return;    

        foreach (Transform child in blocks.transform)
        {
            Destroy(child.gameObject);
        }      

        var placedBlocks = BlockLoader.LoadBlocks(loadedData, blockDatabase, blocks.transform);
        TileManager.RefreshAllTiles(placedBlocks);

        print($"Loaded map: {randomMapName}");
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
}
