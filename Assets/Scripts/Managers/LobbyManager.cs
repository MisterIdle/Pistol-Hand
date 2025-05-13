using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-40)]
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

        HUDManager.ShowTitle("PRESS ANY KEY TO JOIN", "KILL TO BEGIN", Color.white, Color.red, 150f, true);
        HUDManager.BackgroundImage.enabled = false;

        HUDManager.EnableParameterButton(true);

        GameManager.ResetGame();

        LoadLobbyMap();

        AudioManager.Instance.PlayMusic(MusicType.MainMenu);

        StarGenerator.Instance.GenerateStars();

        print("LobbyManager Awake");
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

    public void LoadLobbyMap() {
        foreach (Transform child in MapManager.MapTile.transform)
            Destroy(child.gameObject);
            
        var data = SaveManager.LoadMap("Lobby");
        var blocks = BlockLoader.LoadBlocks(data, MapManager.blockDatabase, MapManager.MapTile.transform);
        TileManager.RefreshAllTiles(blocks);
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

        HUDManager.ClearTitle();

        yield return SceneLoader.LoadScene(GameManager.GameSceneName);
    }
}