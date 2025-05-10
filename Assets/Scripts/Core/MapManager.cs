using UnityEngine;
using System.Linq;

public class MapManager : BaseManager
{
    public static MapManager Instance { get; private set; }

    public BlockDatabase blockDatabase;
    public GameObject MapTile;
    public float GridSize = 0.5f;

    private Transform[] _spawnPoints;

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
    }

    public void SetSpawnPoints()
    {
        _spawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawn")
            .Select(go => go.transform)
            .ToArray();
    }

    public void PlacePlayer(PlayersController player)
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found!");
            return;
        }

        var available = _spawnPoints.Where(sp => !IsSpawnPointOccupied(sp)).ToArray();
        if (available.Length == 0 && GameManager.Instance.CurrentState != GameState.Trophy)
        {
            Debug.LogError("No available spawn points!");
            return;
        }

        var random = available[Random.Range(0, available.Length)];
        player.transform.position = random.position;
    }

    private bool IsSpawnPointOccupied(Transform spawnPoint)
    {
        var players = GameManager.Instance.GetAllPlayers();
        return players.Any(p => Vector3.Distance(p.transform.position, spawnPoint.position) < 0.1f);
    }
}
