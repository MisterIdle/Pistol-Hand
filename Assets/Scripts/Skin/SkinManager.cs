using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class SkinManager : BaseManager
{
    public static SkinManager Instance;

    public List<SkinColor> AvailableColors;
    private Dictionary<int, SkinColor> _assignedColors = new();
    private readonly object _lock = new();

    private void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
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

    public bool AssignColor(int playerID, int colorIndex)
    {
        lock (_lock)
        {
            if (colorIndex < 0 || colorIndex >= AvailableColors.Count)
                return false;

            SkinColor selected = AvailableColors[colorIndex];

            if (_assignedColors.ContainsValue(selected))
                return false;

            _assignedColors[playerID] = selected;
            return true;
        }
    }

    public Color GetPlayerColor(int playerID)
    {
        if (_assignedColors.ContainsKey(playerID))
            return _assignedColors[playerID].Color;

        return Color.white;
    }

    public string GetPlayerColorName(int playerID)
    {
        if (_assignedColors.ContainsKey(playerID))
            return _assignedColors[playerID].Name;

        return "None";
    }

    public List<SkinColor> GetAvailableColors()
    {
        List<SkinColor> free = new();
        foreach (var skin in AvailableColors)
        {
            if (!_assignedColors.ContainsValue(skin))
                free.Add(skin);
        }
        return free;
    }

    public bool ChangeColor(int playerID, int newColorIndex)
    {
        lock (_lock)
        {
            if (newColorIndex < 0 || newColorIndex >= AvailableColors.Count)
                return false;
    
            SkinColor newColor = AvailableColors[newColorIndex];
    
            foreach (var entry in _assignedColors)
            {
                if (entry.Value == newColor && entry.Key != playerID)
                    return false;
            }
    
            _assignedColors[playerID] = newColor;
            return true;
        }
    }

    public void ClearAssignedColors()
    {
        _assignedColors.Clear();
    }
}
