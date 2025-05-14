using System.Collections;
using UnityEngine;
using System.Linq;

public class TrophyManager : BaseManager
{
    public static TrophyManager Instance { get; private set; }

    private bool _animationStarted = false;

    [Header("Trophy Animation")]
    [SerializeField] private GameObject trophyPrefab;
    [SerializeField] private GameObject fireworkPrefab;

    private float _fireworkTimer = 0f;
    [SerializeField] private float fireworkMin = 1f;
    [SerializeField] private float fireworkMax = 3f;

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

    private void Update()
    {
        _fireworkTimer += Time.deltaTime;
        if (_fireworkTimer >= Random.Range(fireworkMin, fireworkMax))
        {
            GenerateFirework();
            _fireworkTimer = 0f;
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

        Instantiate(trophyPrefab, MapManager.MapTile.transform, MapManager.MapTile.transform);

        var players = GameManager.GetAllPlayers();

        GameManager.ResetAllPlayers();
        MapManager.SetSpawnPoints();

        foreach (var player in players)
        {
            MapManager.PlacePlayer(player);
            player.SetMovementState(true);
        }

        UpdateCrown();

        yield return new WaitForSeconds(5f);

        foreach (var p in GameManager.GetAllPlayers())
        {
            if (p.Wins != GameManager.NeedToWin)
            {
                p.KillPlayer();
                yield return new WaitForSeconds(3f);
            }
        }

        HUDManager.SetTransition(false);
        yield return new WaitForSeconds(2f);

        foreach (var p in GameManager.GetAllPlayers())
            Destroy(p.gameObject);

        GameManager.PlayerCount = 0;
        GameManager.PlayerDeath = 0;

        HUDManager.ClearTitle();
        HUDManager.HideAllPlayerCards();
     
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
                HUDManager.ShowTitle($"TROPHY CEREMONY", $"WINNER: {winner.name}", Color.yellow, SkinManager.GetPlayerColor(winner.PlayerID), 250f, true);
    }

    public void GenerateFirework()
    {
        var firework = Instantiate(fireworkPrefab, MapManager.MapTile.transform);
        
        var screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        firework.transform.position = new Vector3(
            Random.Range(-screenBounds.x, screenBounds.x), 
            Random.Range(-screenBounds.y, screenBounds.y), 
            0f);

        var color = SkinManager.GetPlayerColor(GameManager.GetAllPlayers().OrderByDescending(p => p.Wins).FirstOrDefault().PlayerID);
        firework.GetComponent<Firework>().SetColor(color);
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
}
