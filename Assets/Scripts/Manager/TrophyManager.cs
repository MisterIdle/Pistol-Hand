using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrophyManager : MonoBehaviour
{
    public static TrophyManager Instance { get; private set; }

    [SerializeField] private string _trophySceneName = "Trophy";
    public bool AnimationEnd { get; set; } = false;

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

    public void StartTrophySequence()
    {
        StartCoroutine(TrophySequence());
    }

    private IEnumerator TrophySequence()
    {
        yield return new WaitForSeconds(1f);
        yield return SceneLoader.LoadScene(_trophySceneName);
        yield return StartCoroutine(EndAnimation());
    }

    private IEnumerator EndAnimation()
    {
        if (AnimationEnd) yield break;
        AnimationEnd = true;

        yield return new WaitForSeconds(3f);

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        var nonWinners = new List<PlayerController>(players);
        nonWinners.RemoveAll(p => p.Wins >= MatchManager.Instance.WinsToWin);

        yield return new WaitForSeconds(2f);

        while (nonWinners.Count > 0)
        {
            int index = Random.Range(0, nonWinners.Count);
            PlayerController playerToExplode = nonWinners[index];

            //playerToExplode.KillPlayer();
            nonWinners.RemoveAt(index);

            yield return new WaitForSeconds(0.7f);
        }

        yield return new WaitForSeconds(2f);

        AnimationEnd = false;
        yield return LobbyManager.Instance.ReturnLobby();
    }
}