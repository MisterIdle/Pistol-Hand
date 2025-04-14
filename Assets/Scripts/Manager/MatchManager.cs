using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MatchManager : BaseManager
{
    public static MatchManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private int _winsToWin = 3;
    public int WinsToWin => _winsToWin;
    
    [Header("Spawn System")]
    private Transform[] _spawnPoints;
    public bool LoadNextMap = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        GameManager.SetGameState(GameState.Playing);
        StartCoroutine(NewMatch());

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            player.Wins = 0;
        }
        
        InitializeSpawnSystem();
    }

    public void InMatch()
    {
        if (GameManager.CheckPlayer())
        {
            StartCoroutine(NewMatch());
        }
    }

    private void InitializeSpawnSystem()
    {
        RefreshSpawnPoints();
    }

    public IEnumerator NewMatch()
    {
        if (LoadNextMap) yield break;
        LoadNextMap = true;

        yield return new WaitForSeconds(1f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (!player.IsDead) 
            {
                player.Wins++;
                if (player.Wins >= _winsToWin)
                {
                    GameManager.SetGameState(GameState.Trophy);
                    yield return SceneLoader.LoadScene(GameManager.TrophySceneName);
                    yield break;
                }
            }
            
        }
        
        RefreshSpawnPoints();
        ResetAllPlayers();

        yield return CameraManager.MoveCameraTransition(false, 1f);
        LoadNextMap = false;
    }

    private void RefreshSpawnPoints()
    {
        var spawnObjects = GameObject.FindGameObjectsWithTag("PlayerSpawn");
        _spawnPoints = new Transform[spawnObjects.Length];
        for (int i = 0; i < spawnObjects.Length; i++)
            _spawnPoints[i] = spawnObjects[i].transform;
    }

    public void ResetAllPlayers()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        var availableSpawns = new List<Transform>(_spawnPoints);

        foreach (var player in players)
        {
            var spawnPoint = availableSpawns.Count > 0 ? 
                availableSpawns[Random.Range(0, availableSpawns.Count)] : 
                _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            
            player.Respawn();
            player.SetPosition(spawnPoint.position);
        }

        GameManager.PlayerDeath = 0;
    }
}