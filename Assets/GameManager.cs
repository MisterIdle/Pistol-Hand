using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int minPlayer = 2;
    public int currentPlayers = 0;
    public int playersDeath = 0;
    public int needToWin = 3;

    public enum GameState { WaitingForPlayers, Playing, GameOver }
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
    }

    private void Update()
    {
        if (CurrentState == GameState.WaitingForPlayers)
        {
            InLobby();
        }
        else if (CurrentState == GameState.Playing && playersDeath >= currentPlayers - 1)
        {
            RoundEndStop();
        }
    }

    private void InLobby()
    {
        if (currentPlayers >= minPlayer && CurrentState == GameState.WaitingForPlayers)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
        }
    }

    private void StartGame()
    {
        if (CurrentState == GameState.Playing) return;

        CurrentState = GameState.Playing;
        playersDeath = 0;
        SceneManager.LoadScene("GameScene");
    }

    private void RoundEndStop()
    {
        if (CurrentState != GameState.Playing) return;

        PlayerController winner = null;
        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (!player.isDead)
            {
                winner = player;
                break;
            }
        }

        if (winner != null)
        {
            winner.wins++;

            Debug.Log("Winner: " + winner.name + " with " + winner.wins + " wins!");

            if (winner.wins >= needToWin)
            {
                StartCoroutine(StartFinishGame(winner));
            }
            else
            {
                StartCoroutine(RestartRound());
            }
        }
    }

    private IEnumerator RestartRound()
    {
        CurrentState = GameState.WaitingForPlayers;
        yield return new WaitForSeconds(1f);

        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            player.Respawn();
        }

        StartGame();
    }

    private IEnumerator StartFinishGame(PlayerController winner)
    {
        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            player.Respawn();
        }
        
        CurrentState = GameState.GameOver;
        yield return new WaitForSeconds(1f);
        
        SceneManager.LoadScene("Lobby");

        Debug.Log("Winner: " + winner.name);

        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            player.wins = 0;
        }

        playersDeath = 0;
        currentPlayers = 0;
        CurrentState = GameState.WaitingForPlayers;
    }
}
