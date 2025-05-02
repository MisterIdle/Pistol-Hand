using UnityEngine;

public class MapTester : BaseManager
{
    public static MapTester Instance { get; private set; }

    public int PlayerID = 0;
    public bool InTestMode = false;
    private bool wasGridEnabledBeforeTest = true;

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
        Debug.Log("InTestMode: " + InTestMode);
        if (!InTestMode)
        {
            var players = GameManager.GetAllPlayers();
            if (players != null)
            {
                foreach (var player in players)
                {
                    Destroy(player.gameObject);
                }
            }
            return;
        }
        
        var activePlayers = GameManager.GetAllPlayers();
        if (activePlayers == null) return;

        foreach (var player in activePlayers)
        {
            GameManager.PlacePlayer(player);
        }

        PlayerID++;
        GameManager.PlayerCount++;
    }

    public void InTestMatch()
    {
        if (InTestMode && GameManager.CheckPlayer())
        {
            StopTestMatch();
            print("Test Match Ended");
            
            if (GameManager.IsPlayerKilledByAnother())
            {
                MapEditor.HasBeenTestedAndValid = true;
                print("Test Match Validated");
            }
        }
    }

    public void StartTestMatch()
    {
        wasGridEnabledBeforeTest = MapEditor.Instance.gridEnabled;

        if (MapEditor.Instance.gridEnabled)
        {
            MapEditor.Instance.ToggleGrid();
        }

        StartCoroutine(CameraManager.ChangeCameraLens(5f, 1f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(0, 0, -10), 1f));

        HUDEditorManager.editorUI.SetActive(false);
        HUDEditorManager.testerUI.SetActive(true);

        MapEditor.SetCratePhysics(true);

        GameManager.ResetAllPlayers();
        GameManager.SetSpawnPoints();

        InTestMode = true;
    }

    public void StopTestMatch()
    {
        InTestMode = false;
        StartCoroutine(CameraManager.ChangeCameraLens(6.5f, 1f));
        StartCoroutine(CameraManager.SetCameraPosition(new Vector3(0f, -0.5f, -10), 1f));

        var players = GameManager.GetAllPlayers();
        if (players == null) return;

        foreach (var player in players)
        {
            Destroy(player.gameObject);
        }

        GameManager.PlayerCount = 0;
        GameManager.PlayerDeath = 0;

        PlayerID = 0;

        HUDEditorManager.editorUI.SetActive(true);
        HUDEditorManager.testerUI.SetActive(false);
        HUDEditorManager.center.SetActive(true);

        MapEditor.ReplaceCrateAfterTest();
    

        if (wasGridEnabledBeforeTest)
        {
            MapEditor.Instance.ToggleGrid();
        }
    }
}
