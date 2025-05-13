using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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

    [Header("Gamepad Settings")]
    private HashSet<Gamepad> disconnectedGamepads = new();
    private bool isPausedForGamepad = false;

    [Header("Disconnect Timeout Settings")]
    public float DisconnectTimeout = 10f;
    private float disconnectTimer = 0f;
    private bool _timerEnded = false;

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
        HandleGameState();

        if (CurrentState == GameState.Playing)
        {
            HandleDisconnectedGamepads();
        }
    }

    private void HandleGameState()
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

    private void HandleDisconnectedGamepads()
    {
        if (disconnectedGamepads.Count > 0)
        {
            disconnectTimer += Time.unscaledDeltaTime;
            Debug.Log($"Disconnected Gamepads: {disconnectedGamepads.Count}, Disconnect Timer: {disconnectTimer:F1}s");
    
            if (disconnectTimer >= DisconnectTimeout && !_timerEnded)
            {
                _timerEnded = true;

                disconnectedGamepads.Clear();
                isPausedForGamepad = false;

                Time.timeScale = 1f;
                StartCoroutine(HUDManager.TransitionToGameScene(false));
                HUDManager.BackgroundImage.enabled = false;
                HUDManager.ClearTitle();

                disconnectTimer = 0f;

                _timerEnded = false;
                return;
            }
    
            if (!isPausedForGamepad)
            {
                PauseGameOnDisconnect();
            }
            UpdateDisconnectTitle();
        }
        else
        {
            if (isPausedForGamepad)
            {
                disconnectTimer = 0f;
                ResumeGameIfAllConnected();
                HUDManager.BackgroundImage.enabled = true;
            }
        }
    }


    private void UpdateDisconnectTitle()
    {
        HUDManager.BackgroundImage.enabled = false;
        HUDManager.ShowTitle("CONTROLLER DISCONNECTED", $"RECONNECT TO CONTINUE ({(DisconnectTimeout - disconnectTimer):F1}s)", Color.white, Color.red, 0, true);
    }

    private void PauseGameOnDisconnect()
    {
        if (!isPausedForGamepad)
        {
            isPausedForGamepad = true;
            Time.timeScale = 0f;
        }
    }

    private void ResumeGameIfAllConnected()
    {
        if (disconnectedGamepads.Count == 0 && isPausedForGamepad)
        {
            isPausedForGamepad = false;
            Time.timeScale = 1f;
            HUDManager.ClearTitle();
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

    public void ResetGame()
    {
        PlayerCount = 0;
        PlayerDeath = 0;
        SetGameState(GameState.WaitingForPlayers);
        ResetAllPlayers();

        foreach (var player in GetAllPlayers())
        {
            Destroy(player.gameObject);
        }
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Disconnected:
                    disconnectedGamepads.Add(gamepad);
                    break;
                case InputDeviceChange.Reconnected:
                    disconnectedGamepads.Remove(gamepad);
                    break;
                case InputDeviceChange.Added:
                    break;
            }
        }
    }
}
