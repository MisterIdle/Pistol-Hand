using System.Collections.Generic;

public class GameParameter : BaseManager
{
    public static GameParameter Instance { get; private set; }

    private Dictionary<Values, int> parameters = new Dictionary<Values, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeDefaults();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDefaults()
    {
        parameters[Values.NeedToWin] = 3;
        parameters[Values.PlayerHealth] = 3;
    }

    public int GetValue(Values key) => parameters.TryGetValue(key, out var value) ? value : 0;
    public void SetValue(Values key, int value) => parameters[key] = value;

    public void ApplySettings()
    {
        GameManager.NeedToWin = GetValue(Values.NeedToWin);

        var players = GameManager.GetAllPlayers();
        if (players != null)
        {
            foreach (var player in players)
            {
                player.SetPlayerHealth(GetValue(Values.PlayerHealth));
            }
        }
    }
}
