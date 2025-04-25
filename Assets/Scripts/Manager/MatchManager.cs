using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class MatchManager : BaseManager
{
    public static MatchManager Instance { get; private set; }

    [Header("Spawn System")]
    public bool IsLoading = false;
    public bool FirstMatch = true;

    [Header("Map System")]
    public BlockDatabase blockDatabase;
    public GameObject blocks;

    private void Awake()
    {
        InitializeSingleton();
        GameManager.SetGameState(GameState.Playing);
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

    public void InMatch()
    {
        if (GameManager.CheckPlayer())
            StartCoroutine(NewMatch());
    }

    private IEnumerator NewMatch()
    {
        if (IsLoading) yield break;
        IsLoading = true;

        if (FirstMatch)
        {
            foreach (var p in GameManager.GetAllPlayers())
                p.Wins = 0;

            FirstMatch = false;
        }
        else
        {
            foreach (var p in GameManager.GetAllPlayers())
            {
                if (p.IsDead) continue;
                p.Wins++;

                if (p.Wins >= GameManager.NeedToWin)
                {
                    Debug.Log($"Player {p.PlayerID} wins the match!");
                    IsLoading = false;
                    StartCoroutine(TeleportToTrophy());
                    yield break;
                }
            }
        }

        yield return new WaitForSeconds(1f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        yield return StartCoroutine(LoadMapFromSave());

        yield return new WaitForSeconds(1f);

        GameManager.ResetAllPlayers();
        GameManager.SetSpawnPoints();
        GameManager.PlaceAllPlayers();

        yield return CameraManager.MoveCameraTransition(false, 1f);
        IsLoading = false;
    }

    private IEnumerator LoadMapFromSave()
    {
        List<(string id, Vector3 pos)> blocks = SaveManager.LoadRandomMap();
        if (blocks != null)
        {
            foreach (var block in blocks)
            {
                var blockData = blockDatabase.blocks.FirstOrDefault(b => b.id == block.id);
                if (blockData != null)
                {
                    Instantiate(blockData.prefab, block.pos, Quaternion.identity);
                }
                else
                {
                    Debug.LogError($"Block ID {block.id} not found in the database!");
                }
            }
        }
        else
        {
            Debug.LogError("Failed to load map data or map is empty.");
            IsLoading = false;
            yield break;
        }
        yield return null;
    }

    private IEnumerator TeleportToTrophy()
    {
        if (IsLoading) yield break;
        IsLoading = true;

        GameManager.SetGameState(GameState.Trophy);

        yield return new WaitForSeconds(1f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        IsLoading = false;
        yield return SceneLoader.LoadScene(GameManager.TrophySceneName);
    }
}
