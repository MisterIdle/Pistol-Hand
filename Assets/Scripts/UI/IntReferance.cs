using UnityEngine;
using System.Reflection;
using TMPro;
using System;

[System.Serializable]
public class IntReference
{
    private GameManager gameManager => GameManager.Instance;
    public TextMeshProUGUI textObject;
    public MonoBehaviour target;
    public string fieldName;
    public float minValue;
    public float maxValue;
    public float defaultValue;
    public bool isInfinite;

    public void Initialize()
    {
        SetValue(defaultValue);
    }

    public float GetValue()
    {
        if (target == null || string.IsNullOrEmpty(fieldName))
            return 0;

        var type = target.GetType();
        var field = type.GetField(fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (target.name == "Player")
        {
            var players = gameManager.GetAllPlayers();
            if (players.Length == 0) return 0;
            var playerField = players[0].GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (playerField.FieldType == typeof(int))
            {
                return (int)playerField.GetValue(players[0]);
            }
            else if (playerField.FieldType == typeof(float))
            {
                return (int)(float)playerField.GetValue(players[0]);
            }
        }
        if (field != null && field.FieldType == typeof(int))
        {
            return (int)field.GetValue(target);
        } else if (field != null && field.FieldType == typeof(float))
        {
            return (int)(float)field.GetValue(target);
        }

        Debug.LogWarning($"Field '{fieldName}' not found or not int in '{target.name}'");
        return 0;
    }

    public void SetValue(float value)
    {
        if (target == null || string.IsNullOrEmpty(fieldName))
            return;

        var type = target.GetType();
        var field = type.GetField(fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field != null && (field.FieldType == typeof(int) || field.FieldType == typeof(float)))
        {
            if (target.name == "Player")
            {
                var players = gameManager.GetAllPlayers();
                if (players.Length == 0) return;
                foreach (var player in players)
                {
                    var playerField = player.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field.FieldType == typeof(int)){
                        playerField.SetValue(player, (int)value);
                    }
                    else if (field.FieldType == typeof(float)){
                        playerField.SetValue(player, value);
                    }
                }

            }
            else if (field.FieldType == typeof(int)){
                field.SetValue(target, (int)value);
            }
            else if (field.FieldType == typeof(float)){
                field.SetValue(target, value);
            }
            
            if (value == 10000)
            {
                textObject.text = "∞";
                return;
            }
            textObject.text = value.ToString();
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found or not int in '{target.name}'");
        }
    }

    public void Increment()
    {
        float newValue = GetValue() + 1;
        if (isInfinite && newValue > maxValue)
        {
            SetValue(10000);
            return;
        }
        SetValue(newValue > maxValue ? minValue : newValue);
    }

    public void Decrement()
    {
        if (GetValue() == 10000)
        {
            SetValue(maxValue);
            return;
        }
        float newValue = GetValue() - 1;
        SetValue(newValue < minValue ? maxValue : newValue);
    }
}
