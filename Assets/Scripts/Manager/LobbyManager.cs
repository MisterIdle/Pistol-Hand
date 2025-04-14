using UnityEngine;
using System.Collections;

public class LobbyManager : BaseManager
{
    public static LobbyManager Instance { get; private set; }

    [Header("Player Settings")]
    public int _playerID = 0;
    private PlayerController playerController;

    public bool LoadNextMap = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void OnPlayerJoin()
    {
        playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.PlayerID = _playerID;
            playerController.name = "Player " + _playerID;

            StartCoroutine(CameraManager.SlowMotion());

            _playerID++;
            GameManager.PlayerCount++;
        }
    }

    public void InLobby() {
        if (GameManager.CheckPlayer()) 
        {
            StartCoroutine(StartNewGame());
        }
    }

    public IEnumerator StartNewGame() 
    {
        if (LoadNextMap) yield break;
        LoadNextMap = true;
        
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(CameraManager.MoveCameraTransition(true, 1f));

        print("Starting new game...");
        yield return new WaitForSeconds(1f);
        LoadNextMap = false;

        yield return SceneLoader.LoadScene(GameManager.GameSceneName);
    }

    public IEnumerator ReturnLobby()
    {
        HUDManager.FadeIn(1f);
        yield return new WaitForSeconds(1.5f);

        yield return SceneLoader.LoadScene(GameManager.LobbySceneName);

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            Destroy(player.gameObject);
        }

        GameManager.PlayerCount = 0;
        GameManager.PlayerDeath = 0;
    }

    public void UpdateLobbyState()
    {
        HUDManager.FadeOut(1f);
    }
}