using UnityEngine;
using System.Collections;

public class LobbyManager : BaseManager
{
    public static LobbyManager Instance { get; private set; }

    [Header("Player Settings")]
    public int PlayerID = 0;
    private bool _loadNextMap = false;

    private void Awake()
    {
        InitializeSingleton();
        GameManager.SetGameState(GameState.WaitingForPlayers);

        HUDManager.SetTransition(true);

        SkinManager.ClearAssignedColors();

        StartCoroutine(CameraManager.ChangeCameraLens(5f, 0f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(0, 0, -10), 0f));
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
        PlayerID++;
        GameManager.PlayerCount++;
    }

    public void InLobby()
    {
        if (GameManager.CheckPlayer())
            StartCoroutine(StartNewGame());
    }

    public IEnumerator StartNewGame()
    {
        if (_loadNextMap) yield break;
        _loadNextMap = true;

        yield return new WaitForSeconds(1.5f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        _loadNextMap = false;

        yield return SceneLoader.LoadScene(GameManager.GameSceneName);
    }
}