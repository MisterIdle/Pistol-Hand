using System.Collections;
using UnityEngine;
using System.Linq;

public class TrophyManager : BaseManager
{
    public static TrophyManager Instance { get; private set; }

    private bool _animationStarted = false;

    private void Awake()
    {
        InitializeSingleton();
        StartCoroutine(HandleTrophyAnimation());

        HUDManager.BackgroundImage.enabled = false;
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

    private IEnumerator HandleTrophyAnimation()
    {
        if (_animationStarted) yield break;
        _animationStarted = true;

        StarGenerator.Instance.ClearStars();
        yield return new WaitForSeconds(0.5f);
        StarGenerator.Instance.GenerateStars();

        StartCoroutine(LoadTrophyMap());

        yield return CameraManager.MoveCameraTransition(false, 1f);

        var players = GameManager.GetAllPlayers();

        GameManager.ResetAllPlayers();
        MapManager.SetSpawnPoints();

        foreach (var player in players)
        {
            MapManager.PlacePlayer(player);
            player.SetMovementState(true);
        }

        yield return new WaitForSeconds(3f);

        foreach (var p in GameManager.GetAllPlayers())
        {
            if (p.Wins != GameManager.NeedToWin)
            {
                p.KillPlayer();
                yield return new WaitForSeconds(2f);
            }
        }

        HUDManager.SetTransition(false);
        yield return new WaitForSeconds(2f);

        foreach (var p in GameManager.GetAllPlayers())
            Destroy(p.gameObject);

        GameManager.PlayerCount = 0;
        GameManager.PlayerDeath = 0;

        HUDManager.ClearTitle();
     
        yield return SceneLoader.LoadScene(GameManager.LobbySceneName);
    }

    public IEnumerator LoadTrophyMap()
    {
        var data = SaveManager.LoadMap("Trophy");
        if (data == null)
        {
            Debug.LogWarning("Trophy map not found.");
            yield break;
        }

        foreach (Transform child in MapManager.MapTile.transform)
            Destroy(child.gameObject);

        var placedBlocks = BlockLoader.LoadBlocks(data, MapManager.blockDatabase, MapManager.MapTile.transform);
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

        var winner = GameManager.GetAllPlayers().OrderByDescending(p => p.Wins).FirstOrDefault();
        if (winner != null)
                HUDManager.ShowTitle($"TROPHY CEREMONY", $"WINNER: {winner.name}", Color.yellow, SkinManager.GetPlayerColor(winner.PlayerID));
    }
}
