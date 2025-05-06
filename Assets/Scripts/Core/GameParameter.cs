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
        parameters[Values.Health] = 3f;
        parameters[Values.MaxSpeed] = 8f;
        parameters[Values.JumpForce] = 10f;
        parameters[Values.HitForce] = 1f;
        parameters[Values.CrossbowForce] = 1f;
        parameters[Values.ReloadBullet] = 0.5f;
        parameters[Values.BulletSpeed] = 20f;
        parameters[Values.DashSpeed] = 50f;
        parameters[Values.DashCooldown] = 1f;
        parameters[Values.DashDuration] = 0.1f;
        parameters[Values.StunDuration] = 0.1f;
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
                player.SetHealth(GetInt(Values.Health));
                player.SetMaxSpeed(GetFloat(Values.MaxSpeed));
                player.SetJumpForce(GetFloat(Values.JumpForce));
                player.SetHitForce(GetFloat(Values.HitForce));
                player.SetCrossbowForce(GetFloat(Values.CrossbowForce));
                player.SetReloadBullet(GetFloat(Values.ReloadBullet));
                player.SetBulletSpeed(GetFloat(Values.BulletSpeed));
                player.SetDashSpeed(GetFloat(Values.DashSpeed));
                player.SetDashCooldown(GetFloat(Values.DashCooldown));
                player.SetDashDuration(GetFloat(Values.DashDuration));
                player.SetStunDuration(GetFloat(Values.StunDuration));
            }
        }
    }
}
