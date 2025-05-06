using UnityEngine;

[DefaultExecutionOrder(-110)]
public class GameParameter : BaseManager
{
    public static GameParameter Instance { get; private set; }

    [Header("Game Settings")]
    public int NeedToWin = 3;
    public int PlayerHealth = 3;

    private void Awake()
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

    public void Update()
    {

        if (GameManager.Instance.CurrentState != GameState.WaitingForPlayers) return;

        GameManager.NeedToWin = NeedToWin;

        var players = GameManager.GetAllPlayers();
        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].IsDead) continue;
                players[i].SetPlayerHealth(PlayerHealth);
            }
        }
    }

    public int GetNeedToWin() => NeedToWin;
    public void SetNeedToWin(int value) => NeedToWin = value;

    public int GetPlayerHealth() => PlayerHealth;
    public void SetPlayerHeal(int value) => PlayerHealth = value;
}
