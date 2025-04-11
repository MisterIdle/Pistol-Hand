using System.Collections;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int MaxPlayers = 4;
    public int MinPlayers = 2;
    public int PlayerReadyCount = 0;
    public int PlayerCount = 0;

    [Header("Scenes")]
    [SerializeField] private SceneAsset _lobbyScene;
    [SerializeField] private SceneAsset _matchAreaScene;

    [Header("Hit")]
    public float slowfactor = 0.05f;
    public float slowDuration = 0.02f;
    public float shakeTime;

    [Header("Cinemachine Settings")]
    public CinemachineCamera virtualCamera;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    
    public enum GameState { WaitingForPlayers, Playing, Trophy }
    public bool LoadNextMap = false;
    public GameState CurrentState { get; private set; } = GameState.WaitingForPlayers;

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

        // Anti-lag settings
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        CurrentState = GameState.WaitingForPlayers;
        virtualCamera = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None)[0];

        DontDestroyOnLoad(virtualCamera.gameObject);
        DontDestroyOnLoad(Camera.main.gameObject);
    }

    public void Update()
    {
        switch (CurrentState)
        {
            case GameState.WaitingForPlayers:
                InLobby();
                break;
            case GameState.Playing:
                InGame();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
            {
                CinemachineBasicMultiChannelPerlin noise = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
                noise.AmplitudeGain = 0f;
            }
        }
    }


    // CAMERA EFFECTS

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin noise = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        noise.AmplitudeGain = intensity;
        shakeTime = time;

        print("Camera shake started!");
    }

    public IEnumerator StunAndSlowMotion()
    {
        Time.timeScale = slowfactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        yield return new WaitForSeconds(slowDuration);
        Time.timeScale = 1f;

        print("Slow motion ended!");
    }

    public IEnumerator LoadMatchArea()
    {
        yield return new WaitForSeconds(1f);

        LoadNextMap = true;

        yield return MoveCameraTransition(true, 1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_matchAreaScene.name, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("PlayerSpawn");
        spawnPoints = new Transform[spawnObjects.Length];
        for (int i = 0; i < spawnObjects.Length; i++)
        {
            spawnPoints[i] = spawnObjects[i].transform;
        }

        ResetAllPlayers();

        yield return MoveCameraTransition(false, 1f);

        LoadNextMap = false;
    }


    private IEnumerator MoveCameraTransition(bool moveUp, float time)
    {
        int direction = moveUp ? 1 : -1;
        Vector3 targetPosition = virtualCamera.transform.position + new Vector3(0, direction * 50, 0);

        float elapsedTime = 0f;
        Vector3 originalPos = virtualCamera.transform.position;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / time);
            virtualCamera.transform.position = Vector3.Lerp(originalPos, targetPosition, t);

            yield return null;
        }
    }

    // GAME STATE MANAGEMENT

    private void InLobby()
    {
        if (PlayerReadyCount == PlayerCount - 1 && PlayerCount >= MinPlayers)
        {
            StartCoroutine(LoadMatchArea());
            CurrentState = GameState.Playing;
        }
    }

    private void InGame()
    {
    }

    public void ResetAllPlayers()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            player.Respawn();
            player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            player.gameObject.SetActive(true);
        }
    }
}
