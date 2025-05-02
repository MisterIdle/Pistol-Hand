using UnityEngine;
using System.Collections;

public class MatchManager : BaseManager
{
    public static MatchManager Instance { get; private set; }

    [Header("Spawn System")]
    public bool IsLoading = false;
    public bool FirstMatch = true;

    [Header("Map System")]
    public BlockDatabase blockDatabase;
    public GameObject blocks;
    
    [Header("Start System")]
    public int countdownStart = 3;
    public int count;

    [Header("Draw System")]
    public float drawTime = 0.5f;

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
                StartCoroutine(DrawMatch());
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

        PlayerController[] players = GameManager.GetAllPlayers();
        foreach (var p in players) p.SetMovementState(false);

        yield return CameraManager.MoveCameraTransition(false, 1f);
        IsLoading = false;
        StartCoroutine(StartMatch());
    }

    public IEnumerator StartMatch()
    {
        PlayerController[] players = GameManager.GetAllPlayers();
        count = countdownStart;
        while (count > 0)
        {
            Debug.Log($"{count}");
            count--;
            yield return new WaitForSeconds(1f);
        }
        foreach (var p in players) p.SetMovementState(true);
        Debug.Log("Go");
    }

    public IEnumerator DrawMatch()
    {
        PlayerController[] players = GameManager.GetAllPlayers();
        while (drawTime > 0)
        {
            drawTime -= Time.deltaTime;
            yield return null;
        }

        int playersAlive = 0;
        PlayerController lastAlivePlayer = null;

        foreach (var p in players)
        {
            if (!p.IsDead)
            {
                playersAlive++;
                lastAlivePlayer = p;
            }
        }

        if (playersAlive == 1 && lastAlivePlayer != null)
        {
            lastAlivePlayer.Wins++;
            Debug.Log($"Player {SkinManager.Instance.GetPlayerColorName(lastAlivePlayer.PlayerID)} wins this round!");
        }
        else
        {
            Debug.Log("Draw");
        }
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

        foreach (var block in placedBlocks)
        {
            if (block.type == BlockType.Spawn)
            {
                var spriteRenderer = block.instance.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
            }
        }

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
