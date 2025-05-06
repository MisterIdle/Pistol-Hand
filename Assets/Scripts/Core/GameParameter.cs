using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-110)]
public class GameParameter : BaseManager
{
    public static GameParameter Instance { get; private set; }

    private Dictionary<Values, float> parameters = new Dictionary<Values, float>();

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
        parameters[Values.NeedToWin] = 3f;
        parameters[Values.PlayerHealth] = 3f;
    }

    public float GetFloat(Values key) => parameters.TryGetValue(key, out var value) ? value : 0f;
    public void SetFloat(Values key, float value) => parameters[key] = value;

    public int GetInt(Values key) => Mathf.FloorToInt(GetFloat(key));
    public void SetInt(Values key, int value) => SetFloat(key, value);


    public void ApplySettings()
    {
        GameManager.NeedToWin = GetFloat(Values.NeedToWin);

        var players = GameManager.GetAllPlayers();
        if (players != null)
        {
            foreach (var player in players)
            {
                player.SetPlayerHealth(GetInt(Values.PlayerHealth));
            }
        }
    }
}
