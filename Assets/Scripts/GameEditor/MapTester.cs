using UnityEngine;

public class MapTester : BaseManager
{
    public static MapTester Instance { get; private set; }

    public int _playerID = 0;
    public bool InTestMode = false;

    private void Awake()
    {
        InitializeSingleton();
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
        if (!InTestMode) return;

        var players = GameManager.GetAllPlayers();
        if (players == null) return;

        int id = 0;
        foreach (var player in players)
        {
            player.name = $"Player {id}";
            player.PlayerID = id;
            id++;

            GameManager.PlacePlayer(player);
        }

        _playerID = id;
        GameManager.PlayerCount = id;

        StartCoroutine(CameraManager.SlowMotion());
    }

    public void InTestMatch()
    {
        if (GameManager.CheckPlayer())
            StopTestMatch();
    }

    public void StartTestMatch()
    {
        StartCoroutine(CameraManager.ChangeCameraLens(5f, 1f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(0, 0, -10), 1f));

        HUDEditorManager.Instance.editorUI.SetActive(false);
        HUDEditorManager.Instance.testerUI.SetActive(true);

        HUDEditorManager.Instance.mirrorModeImage.SetActive(false);
        HUDEditorManager.Instance.killBoxImage.SetActive(false);

        GameManager.ResetAllPlayers();
        GameManager.SetSpawnPoints();

        InTestMode = true;

    }

    public void StopTestMatch()
    {
        InTestMode = false;
        StartCoroutine(CameraManager.ChangeCameraLens(6.5f, 1f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(-1.5f, -1f, -10), 1f));

        var players = GameManager.GetAllPlayers();
        if (players == null) return;

        foreach (var player in players)
        {
            Destroy(player.gameObject);
        }

        GameManager.PlayerCount = 0;
        GameManager.PlayerDeath = 0;
        
        HUDEditorManager.Instance.editorUI.SetActive(true);
        HUDEditorManager.Instance.testerUI.SetActive(false);

        HUDEditorManager.Instance.mirrorModeImage.SetActive(true);
        HUDEditorManager.Instance.killBoxImage.SetActive(true);

        InTestMode = false;
    }
}
