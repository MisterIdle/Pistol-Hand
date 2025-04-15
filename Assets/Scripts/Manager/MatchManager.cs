using System.Collections;
using System.Linq;
using UnityEngine;

public class MatchManager : BaseManager
{
    public static MatchManager Instance { get; private set; }
    
    [Header("Spawn System")]
    public bool LoadNextMap = true;
    public bool FirstMatch = true;

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

    public IEnumerator NewMatch()
    {
        if (LoadNextMap) yield break;
        LoadNextMap = true;

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
                    LoadNextMap = false;
                    StartCoroutine(TeleportToTrophy());
                    yield break;
                }
            }
        }

        yield return new WaitForSeconds(1f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        GameManager.ResetAllPlayers();
        GameManager.SetSpawnPoints();
        GameManager.PlaceAllPlayers();

        yield return CameraManager.MoveCameraTransition(false, 1f);
        LoadNextMap = false;
    }


    private IEnumerator TeleportToTrophy()
    {
        if (LoadNextMap) yield break;
        LoadNextMap = true;

        GameManager.SetGameState(GameState.Trophy);

        yield return new WaitForSeconds(1f);
        yield return CameraManager.MoveCameraTransition(true, 1f);

        LoadNextMap = false;
        yield return SceneLoader.LoadScene(GameManager.TrophySceneName);
        
    }
}