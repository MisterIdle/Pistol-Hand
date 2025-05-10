using UnityEngine;

public class MapTester : BaseManager
{
    public static MapTester Instance { get; private set; }

    public int PlayerID = 0;
    public bool InTestMode = false;

    private bool _wasGridEnabledBeforeTest = true;

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
        var players = GameManager.GetAllPlayers();

        if (!InTestMode)
        {
            if (players != null)
            {
                foreach (var player in players)
                {
                    Destroy(player.gameObject);
                }
            }
            return;
        }

        if (players == null) return;

        foreach (var player in players)
        {
            MapManager.PlacePlayer(player);
        }

        PlayerID++;
        GameManager.PlayerCount++;
    }

    public void InTestMatch()
    {
        if (InTestMode && GameManager.CheckPlayer())
        {
            StopTestMatch();

            if (GameManager.IsPlayerKilledByAnother())
            {
                //MapEditor.HasBeenTestedAndValid = true;
            }
        }
    }

    public void StartTestMatch()
    {
        _wasGridEnabledBeforeTest = MapEditor.GridEnabled;

        if (MapEditor.GridEnabled)
        {
            MapEditor.ToggleGrid();
        }

        StartCoroutine(CameraManager.ChangeCameraLens(5f, 1f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(0, 0, -10), 1f));

        HUDEditorManager.EditorUIObject.SetActive(false);
        HUDEditorManager.TesterUI.SetActive(true);

        MapEditor.SetCratePhysics(true);
        GameManager.ResetAllPlayers();
        MapManager.SetSpawnPoints();

        InTestMode = true;
    }

    public void StopTestMatch()
    {
        InTestMode = false;

        StartCoroutine(CameraManager.ChangeCameraLens(6.5f, 1f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(0f, -0.5f, -10), 1f));

        var players = GameManager.GetAllPlayers();
        if (players != null)
        {
            foreach (var player in players)
            {
                Destroy(player.gameObject);
            }
        }

        GameManager.PlayerCount = 0;
        GameManager.PlayerDeath = 0;
        PlayerID = 0;

        HUDEditorManager.EditorUIObject.SetActive(true);
        HUDEditorManager.TesterUI.SetActive(false);
        HUDEditorManager.Center.SetActive(true);

        MapEditor.ReplaceCrateAfterTest();

        HUDManager.DeleteAllPlayerCards();

        if (_wasGridEnabledBeforeTest)
        {
            MapEditor.ToggleGrid();
        }
    }
}
