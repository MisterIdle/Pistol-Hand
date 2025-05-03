using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-100)]
public class GameManager : BaseManager
{
    public static GameManager Instance { get; private set; }
    public BlockDatabase blockDatabase;
    public GameState CurrentState { get; private set; } = GameState.WaitingForPlayers;

    [Header("Game Settings")]
    public int MinPlayers = 2;
    public int PlayerCount;
    public int PlayerDeath;
    public int NeedToWin = 3;
    public Transform[] _spawnPoints;


    [Header("Scene Settings")]
    public string LobbySceneName = "Lobby";
    public string GameSceneName = "Match";
    public string TrophySceneName = "Trophy";
    public string EditorSceneName = "Editor";
    public float GridSize = 0.5f;

    private void Awake()
    {
        InitializeSingleton();

        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        Time.timeScale = 1f;
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.WaitingForPlayers:
                LobbyManager.InLobby();
                break;
            case GameState.Playing:
                MatchManager.InMatch();
                break;
            case GameState.Editor:
                MapTester.InTestMatch();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
    }

    public bool CheckPlayer() 
    {
        return PlayerDeath == PlayerCount - 1 && PlayerCount >= MinPlayers;
    }

    public bool IsPlayerKilledByAnother()
    {
        PlayersController[] players = GetAllPlayers();
        foreach (PlayersController player in players)
        {
            if (player.LastHitBy != null)
            {
            return true;
            }
        }
        return false;

    }

    public PlayersController[] GetAllPlayers()
    {
        PlayersController[] players = FindObjectsByType<PlayersController>(FindObjectsSortMode.None);
        return players;
    }

    public void ResetAllPlayers()
    {
        PlayersController[] players = GetAllPlayers();
        foreach (PlayersController player in players)
        {
            player.Respawn();
            PlayerDeath = 0;
        }
    }

    public void SetSpawnPoints()
    {
        _spawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawn")
            .Select(go => go.transform)
            .ToArray();
    }

    public void PlaceAllPlayers()
    {
        var players = GameManager.GetAllPlayers();
        var shuffledSpawnPoints = _spawnPoints.OrderBy(x => Random.value).ToArray();
        for (int i = 0; i < players.Length; i++)
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points found! Stopping the game.");
                break;
            }

            var spawnPoint = shuffledSpawnPoints[i % shuffledSpawnPoints.Length];
            players[i].SetPosition(spawnPoint.position);
        }
    }

    public void PlacePlayer(PlayersController player)
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found! Stopping the game.");
            return;
        }

        var availableSpawnPoints = _spawnPoints.Where(sp => !IsSpawnPointOccupied(sp)).ToArray();
        if (availableSpawnPoints.Length == 0)
        {
            Debug.LogError("No available spawn points! Stopping the game.");
            return;
        }

        var randomSpawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Length)];
        player.transform.position = randomSpawnPoint.position;
    }

    private bool IsSpawnPointOccupied(Transform spawnPoint)
    {
        PlayersController[] players = GetAllPlayers();
        foreach (var p in players)
        {
            if (Vector3.Distance(p.transform.position, spawnPoint.position) < 0.1f)
            {
                return true;
            }
        }
        return false;
    }
}