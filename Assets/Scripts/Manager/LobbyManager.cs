using UnityEngine;
using System.Collections;

public class LobbyManager : BaseManager
{
    public static LobbyManager Instance { get; private set; }

    [Header("Player Settings")]
    public int _playerID = 0;
    public bool LoadNextMap = false;

    private void Awake()
    {
        InitializeSingleton();
        GameManager.SetGameState(GameState.WaitingForPlayers);
        HUDManager.FadeOut(1f);

        CameraManager.ChangeCameraLens(5f);
        CameraManager.SetCameraPosition(new Vector3(0, 0, -10));

        HUDManager.gameObject.SetActive(true);
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

    public void OnPlayerJoin()
    {
        var player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        player.PlayerID = _playerID;
        player.name = $"Player {_playerID}";

        StartCoroutine(CameraManager.SlowMotion());

        _playerID++;
        GameManager.PlayerCount++;
    }

    public void InLobby()
    {
        if (GameManager.CheckPlayer())
            StartCoroutine(StartNewGame());
    }

    public IEnumerator StartNewGame()
    {
        if (LoadNextMap) yield break;
        LoadNextMap = true;

        yield return new WaitForSeconds(1.5f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        LoadNextMap = false;

        yield return SceneLoader.LoadScene(GameManager.GameSceneName);
    }
}