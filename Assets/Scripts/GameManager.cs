using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int MaxPlayers = 4;
    public int MinPlayers = 2;
    public int PlayerReadyCount = 0;
    public int PlayerCount = 0;

    [Header("Hit")]
    public float slowfactor = 0.05f;
    public float slowDuration = 0.02f;
    public float shakeTime;

    [Header("Cinemachine Settings")]
    public CinemachineCamera virtualCamera;
    

    public enum GameState { WaitingForPlayers, Playing, Trophy }
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
    }

    public void Update()
    {
        switch (CurrentState)
        {
            case GameState.WaitingForPlayers:
                InLobby();
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

    // GAME STATE MANAGEMENT

    private void InLobby()
    {
        if (PlayerReadyCount == PlayerCount - 1 && PlayerCount >= MinPlayers)
        {
            print("All players are ready! Starting the game...");
        }
    }
}
