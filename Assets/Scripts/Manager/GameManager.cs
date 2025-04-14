using UnityEngine;

public class GameManager : BaseManager
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.WaitingForPlayers;

    [Header("Game Settings")]
    public int MinPlayers = 2;
    public int PlayerCount;
    public int PlayerDeath;

    [Header("Scene Settings")]
    public string LobbySceneName = "Lobby";
    public string GameSceneName = "Match";
    public string TrophySceneName = "Trophy";

    private void Awake()
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

        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        Time.timeScale = 1f;

        HUDManager.Instance.FadeOut(0.5f);
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
            case GameState.Trophy:
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
}