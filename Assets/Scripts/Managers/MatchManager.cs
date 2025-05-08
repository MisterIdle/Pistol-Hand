using UnityEngine;
using System.Collections;

public class MatchManager : BaseManager
{
    public static MatchManager Instance { get; private set; }

    [Header("Spawn System")]
    public bool IsLoading = false;
    public bool FirstMatch = true;

    [Header("Map System")]
    [SerializeField] private BlockDatabase _blockDatabase => GameManager.blockDatabase;
    [SerializeField] private GameObject _blocks;

    [Header("Start System")]
    [SerializeField] private int _countdownStart = 3;
    private int _count;
    [SerializeField] private float seconds = 0.5f;

    [Header("Draw System")]
    [SerializeField] private float _drawTime = 0.5f;

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
                    string winnerName = "Player: " + p.PlayerID;
                    HUDManager.ShowTitle(winnerName, "Congratulations!", SkinManager.Instance.GetPlayerColor(p.PlayerID), Color.white);
                    IsLoading = false;
                    StartCoroutine(TeleportToTrophy());
                    yield break;
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        yield return CameraManager.MoveCameraTransition(true, 1f);
        yield return new WaitForSeconds(1f);

        StarGenerator.Instance.ClearStars();

        // Wait for the map to be fully loaded before continuing
        yield return StartCoroutine(LoadRandomMapAndPlacePlayers());

        HUDManager.ClearTitle();

        var players = GameManager.GetAllPlayers();

        foreach (var player in players)
        {
            player.KillPlayer();
        }

        GameManager.ResetAllPlayers();
        GameManager.SetSpawnPoints();

        // Place players after the map is loaded
        foreach (var player in players)
        {
            GameManager.PlacePlayer(player);
        }

        UpdateCrown();

        StarGenerator.Instance.GenerateStars();

        foreach (var p in players) p.SetMovementState(false);

        yield return CameraManager.MoveCameraTransition(false, 1f);
        IsLoading = false;
        StartCoroutine(StartMatch());
    }

    public IEnumerator StartMatch()
    {
        PlayersController[] players = GameManager.GetAllPlayers();
        _count = _countdownStart;

        while (_count > 0)
        {
            HUDManager.ShowTitle(_count.ToString() + "...", "", Color.white, Color.clear);
            yield return new WaitForSeconds(seconds);
            _count--;
        }

        foreach (var player in players)
        {
            player.SetMovementState(true);
        }

        HUDManager.ShowTitle("GO!", "", Color.green, Color.clear);
        yield return new WaitForSeconds(0.5f);
        HUDManager.ClearTitle();
    }

    public IEnumerator DrawMatch()
    {
        PlayersController[] players = GameManager.GetAllPlayers();
        while (_drawTime > 0)
        {
            _drawTime -= Time.deltaTime;
            yield return null;
        }

        int playersAlive = 0;
        PlayersController lastAlivePlayer = null;

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
            string winnerName = "PLAYER: " + lastAlivePlayer.PlayerID;
            HUDManager.ShowTitle(winnerName.ToUpper(), $"{lastAlivePlayer.Wins} / {GameManager.NeedToWin} FOR THE TROPHY!", SkinManager.Instance.GetPlayerColor(lastAlivePlayer.PlayerID), Color.white);
        }
        else
        {
            HUDManager.ShowTitle("DRAW!", "", Color.white, Color.clear);
        }

        UpdateCrown();
    }

    private void UpdateCrown()
    {
        PlayersController[] players = GameManager.GetAllPlayers();
        int maxWins = -1;

        foreach (var p in players)
            if (!p.IsDead && p.Wins > maxWins)
                maxWins = p.Wins;

        foreach (var p in players)
            p.CrownSprite.enabled = !p.IsDead && p.Wins == maxWins && maxWins > 0;
    }

    public IEnumerator LoadRandomMapAndPlacePlayers()
    {
        var mapNames = SaveManager.GetAllMaps();
        if (mapNames.Count == 0)
        {
            Debug.LogWarning("No maps available to load.");
            yield break;
        }

        string randomMapName = mapNames[Random.Range(0, mapNames.Count)];
        var loadedData = SaveManager.LoadMap(randomMapName);
        if (loadedData == null) yield break;

        foreach (Transform child in _blocks.transform)
        {
            Destroy(child.gameObject);
        }

        var placedBlocks = BlockLoader.LoadBlocks(loadedData, _blockDatabase, _blocks.transform);
        TileManager.RefreshAllTiles(placedBlocks);

        foreach (var block in placedBlocks)
        {
            if (block.type == BlockType.Spawn)
            {
                var spriteRenderer = block.instance.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
            }
        }

        print($"Loaded map: {randomMapName}");

        yield return null;
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
