using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchManager : BaseManager
{
    public static MatchManager Instance { get; private set; }

    [Header("Spawn System")]
    public bool IsLoading = false;
    public bool FirstMatch = true;
    private string _lastMapName = null;

    [Header("Draw System")]
    [SerializeField] private float _drawTime = 0.5f;

    private void Awake()
    {
        InitializeSingleton();
        GameManager.SetGameState(GameState.Playing);

        HUDManager.BackgroundImage.enabled = true;
        HUDManager.ShowTitle("LOADING...", "", Color.white, Color.clear);

        AudioManager.Instance.PlaySFX(SFXType.One);

        AudioManager.Instance.PlayMusic(MusicType.InGame);

        HUDManager.EnableParameterButton(false);
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
                    HUDManager.ShowTitle(winnerName.ToUpper(), "CONGRATULATIONS!", SkinManager.Instance.GetPlayerColor(p.PlayerID), Color.white);
                    AudioManager.Instance.PlaySFX(SFXType.Firework);

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

        yield return StartCoroutine(LoadRandomMap());

        HUDManager.ClearTitle();

        var players = GameManager.GetAllPlayers();

        foreach (var player in players)
        {
            player.KillPlayer();
        }

        GameManager.ResetAllPlayers();
        MapManager.SetSpawnPoints();

        foreach (var player in players)
        {
            MapManager.PlacePlayer(player);
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

        HUDManager.ShowTitle("READY?", "", Color.white, Color.clear);
        AudioManager.Instance.PlaySFX(SFXType.Two);

        yield return new WaitForSeconds(0.5f);

        foreach (var player in players)
        {
            player.SetMovementState(true);
        }

        HUDManager.ShowTitle("GO!", "", Color.green, Color.clear);
        AudioManager.Instance.PlaySFX(SFXType.Three);

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
            AudioManager.Instance.PlaySFX(SFXType.Firework);

        }
        else
        {
            HUDManager.ShowTitle("DRAW!", "", Color.white, Color.clear);
            AudioManager.Instance.PlaySFX(SFXType.Death);
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

    public IEnumerator LoadRandomMap()
    {
        var allMaps = new List<string>();
        allMaps.AddRange(SaveManager.GetAllDefaultMaps());
        allMaps.AddRange(SaveManager.GetAllUsersMaps());

        if (allMaps.Count == 0)
        {
            Debug.LogWarning("No maps available to load.");
            yield break;
        }

        if (allMaps.Count > 1 && _lastMapName != null)
        {
            allMaps.Remove(_lastMapName);
        }

        string randomMapName = allMaps[Random.Range(0, allMaps.Count)];
        _lastMapName = randomMapName;

        var loadedData = SaveManager.LoadMap(randomMapName);
        if (loadedData == null) yield break;

        foreach (Transform child in MapManager.MapTile.transform)
            Destroy(child.gameObject);

        var placedBlocks = BlockLoader.LoadBlocks(loadedData, MapManager.blockDatabase, MapManager.MapTile.transform);
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

        HUDManager.ShowTitle("LOADING...", randomMapName, Color.white, Color.white);

        yield return new WaitForSeconds(1f);

        yield return null;
    }

    private IEnumerator TeleportToTrophy()
    {
        if (IsLoading) yield break;
        IsLoading = true;

        GameManager.SetGameState(GameState.Trophy);

        yield return new WaitForSeconds(1f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        var players = GameManager.GetAllPlayers();

        foreach (var player in players)
        {
            player.KillPlayer();
            player.SetMovementState(false);
        }

        IsLoading = false;
        yield return SceneLoader.LoadScene(GameManager.TrophySceneName);
    }
}
