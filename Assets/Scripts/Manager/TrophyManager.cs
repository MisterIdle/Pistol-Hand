using System.Collections;
using UnityEngine;

public class TrophyManager : BaseManager
{
    public static TrophyManager Instance { get; private set; }
    public bool AnimationEnd = false;

    private void Awake()
    {
        InitializeSingleton();
        StartCoroutine(EndAnimation());
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

    private IEnumerator EndAnimation()
    {
        if (AnimationEnd) yield break;
        AnimationEnd = true;

        yield return CameraManager.MoveCameraTransition(false, 1f);
        
        GameManager.ResetAllPlayers();
        GameManager.SetSpawnPoints();
        GameManager.PlaceAllPlayers();

        yield return new WaitForSeconds(3f);

        foreach (var player in GameManager.GetAllPlayers()) {
            if (player.Wins != GameManager.NeedToWin) {
                player.KillPlayer();
                yield return new WaitForSeconds(2f);
            }
        }

        HUDManager.EnableHUD(false);
        yield return new WaitForSeconds(2f);

        foreach (var player in GameManager.GetAllPlayers()) {
            Destroy(player.gameObject);
        }

        GameManager.PlayerCount = 0;
        GameManager.PlayerDeath = 0;

        yield return SceneLoader.LoadScene(GameManager.LobbySceneName);
    }
}