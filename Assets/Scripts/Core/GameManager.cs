using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-100)]
public class GameManager : BaseManager
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.WaitingForPlayers;

    public int MinPlayers = 2;
    public int PlayerCount;
    public int PlayerDeath;
    public int NeedToWin;

    [Header("Scene Settings")]
    public string LobbySceneName = "Lobby";
    public string GameSceneName = "Match";
    public string TrophySceneName = "Trophy";
    public string EditorSceneName = "Editor";

    private void Awake()
    {
        InitializeSingleton();
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        Time.timeScale = 1f;
        Screen.SetResolution(1920, 1080, true);
        LoadGameSettings();
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

    public void LoadGameSettings()
    {
        var param = SettingsManager.Instance.GetParameterByKey(GameParameterType.NeedToWin);
        if (param != null) NeedToWin = (int)param.value;
        else Debug.LogError("NeedToWin parameter not found!");
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.WaitingForPlayers: LobbyManager.InLobby(); break;
            case GameState.Playing: MatchManager.InMatch(); break;
            case GameState.Editor: MapTester.InTestMatch(); break;
        }
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
        return GetAllPlayers().Any(p => p.LastHitBy != null);
    }

    public PlayersController[] GetAllPlayers()
    {
        return FindObjectsByType<PlayersController>(FindObjectsSortMode.None);
    }

    public void ResetAllPlayers()
    {
        foreach (var player in GetAllPlayers())
        {
            player.Respawn();
        }
        PlayerDeath = 0;
    }
}
