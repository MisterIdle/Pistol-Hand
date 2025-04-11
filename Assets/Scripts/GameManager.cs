using System.Collections;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int MaxPlayers = 4;
    public int MinPlayers = 2;
    public int PlayerDeath = 0;
    public int PlayerCount = 0;
    public int winsToWin = 3;

    [Header("Scenes")]
    [SerializeField] private SceneAsset _lobbyScene;
    [SerializeField] private SceneAsset _matchAreaScene;
    [SerializeField] private SceneAsset _trophyScene;

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
    public bool AnimationEnd = false;
    public bool hasLoad = false;
    public GameState CurrentState { get; private set; } = GameState.WaitingForPlayers;

    private Dictionary<PlayerController, int> playerWins = new();
    private List<PlayerController> players = new List<PlayerController>();

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
    }

    private void Load()
    {
        var cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        if (cameras.Length > 0)
        {
            virtualCamera = cameras[0];
            DontDestroyOnLoad(virtualCamera.gameObject);
            print("Cinemachine camera found: " + virtualCamera.name);
        }
        if (Camera.main != null)
        {
            DontDestroyOnLoad(Camera.main.gameObject);
            print("Main camera found: " + Camera.main.name);
        }
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
            case GameState.Trophy:
                InTrophy();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
            {
                var noise = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
                noise.AmplitudeGain = 0f;
            }
        }
    }

    public void ShakeCamera(float intensity, float time)
    {
        var noise = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        noise.AmplitudeGain = intensity;
        shakeTime = time;
    }

    public IEnumerator StunAndSlowMotion()
    {
        Time.timeScale = slowfactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        yield return new WaitForSeconds(slowDuration);
        Time.timeScale = 1f;
    }

    public void FadeIn(float time)
    {
        HUDManager.Instance.transition.CrossFadeAlpha(1, time, false);
    }

    public void FadeOut(float time)
    {
        HUDManager.Instance.transition.CrossFadeAlpha(0, time, false);
    }

    public IEnumerator LoadMatchArea()
    {
        if (LoadNextMap) yield break;
        LoadNextMap = true;

        yield return new WaitForSeconds(1f);
        yield return MoveCameraTransition(true, 1f);

        CheckLastPlayerStanding();

        if (CurrentState == GameState.Trophy) yield break;

        yield return LoadScene(_matchAreaScene.name);
        PrepareMatch();

        yield return MoveCameraTransition(false, 1f);
        LoadNextMap = false;
    }

    public IEnumerator LoadTrophyArea()
    {
        yield return new WaitForSeconds(1f);

        yield return LoadScene(_trophyScene.name);
        PrepareMatch();

        yield return MoveCameraTransition(false, 1f);
        LoadNextMap = false;
    }

    public IEnumerator LoadLobbyArea()
    {
        FadeIn(1f);
        yield return new WaitForSeconds(1.5f);

        yield return LoadScene(_lobbyScene.name);

        players = new List<PlayerController>(FindObjectsByType<PlayerController>(FindObjectsSortMode.None));
        foreach (PlayerController player in players)
        {
            Destroy(player.gameObject);
        }

        Destroy(virtualCamera.gameObject);
        Destroy(Camera.main.gameObject);

        CurrentState = GameState.WaitingForPlayers;
        PlayerCount = 0;
        PlayerDeath = 0;
        hasLoad = false;

        StopAllCoroutines();
    }

    private IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
                asyncLoad.allowSceneActivation = true;

            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
    }

    private void PrepareMatch()
    {
        spawnPoints = null;
        RefreshSpawnPoints();
        ResetAllPlayers();
    }

    private void RefreshSpawnPoints()
    {
        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("PlayerSpawn");
        spawnPoints = new Transform[spawnObjects.Length];
        for (int i = 0; i < spawnObjects.Length; i++)
            spawnPoints[i] = spawnObjects[i].transform;
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

    private void InLobby()
    {
        if (!hasLoad)
        {
            Load();
            FadeOut(1f);
            hasLoad = true;
        }

        if (PlayerDeath == PlayerCount - 1 && PlayerCount >= MinPlayers)
        {
            StartCoroutine(LoadMatchArea());
            CurrentState = GameState.Playing;
        }
    }

    private void InGame()
    {
        if (PlayerDeath == PlayerCount - 1 && PlayerCount >= MinPlayers)
            StartCoroutine(LoadMatchArea());
    }

    private void InTrophy()
    {
        StartCoroutine(EndAnimation());
    }

    private void CheckLastPlayerStanding()
    {
        players = new List<PlayerController>(FindObjectsByType<PlayerController>(FindObjectsSortMode.None));

        foreach (PlayerController player in players)
        {
            if (!player.IsDead)
            {
                player.Wins++;
                if (player.Wins >= winsToWin)
                {
                    CurrentState = GameState.Trophy;
                    StartCoroutine(LoadTrophyArea());
                }
            }
        }
    }

    private IEnumerator EndAnimation()
    {
        if (AnimationEnd) yield break;
        AnimationEnd = true;

        yield return new WaitForSeconds(3f);

        List<PlayerController> nonWinners = new List<PlayerController>(players);
        nonWinners.RemoveAll(p => p.Wins >= winsToWin);

        yield return new WaitForSeconds(2f);

        while (nonWinners.Count > 0)
        {
            int index = Random.Range(0, nonWinners.Count);
            PlayerController playerToExplode = nonWinners[index];

            playerToExplode.KillPlayer();
            nonWinners.RemoveAt(index);

            yield return new WaitForSeconds(0.7f);
        }

        yield return new WaitForSeconds(2f);

        AnimationEnd = false;
        StartCoroutine(LoadLobbyArea());
    }

    public void ResetAllPlayers()
    {
        players = new List<PlayerController>(FindObjectsByType<PlayerController>(FindObjectsSortMode.None));
        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        foreach (PlayerController player in players)
        {
            Transform spawnPoint = availableSpawns.Count > 0 ? availableSpawns[Random.Range(0, availableSpawns.Count)] : spawnPoints[Random.Range(0, spawnPoints.Length)];
            player.Respawn();
            player.transform.position = spawnPoint.position;
            player.gameObject.SetActive(true);
        }

        PlayerDeath = 0;
    }
}
