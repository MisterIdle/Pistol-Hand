using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    [System.Serializable]
    public class SkinColor
    {
        public string Name;
        public Color Color;
    }

    public static SkinManager Instance;

    [SerializeField] private List<SkinColor> _availableColors;

    private Dictionary<int, SkinColor> _assignedColors = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool AssignColor(int playerID, int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= _availableColors.Count)
            return false;

        SkinColor selected = _availableColors[colorIndex];

        foreach (var color in _assignedColors.Values)
        {
            if (color == selected)
                return false;
        }

        _assignedColors[playerID] = selected;
        return true;
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

    public void UnassignColor(int playerID)
    {
        if (_assignedColors.ContainsKey(playerID))
            _assignedColors.Remove(playerID);
    }

    public List<SkinColor> GetAvailableColors()
    {
        List<SkinColor> free = new();
        foreach (var skin in _availableColors)
        {
            if (!_assignedColors.ContainsValue(skin))
                free.Add(skin);
        }
        return free;
    }

    public bool ChangeColor(int playerID, int newColorIndex)
    {
        if (newColorIndex < 0 || newColorIndex >= _availableColors.Count)
            return false;

        SkinColor newColor = _availableColors[newColorIndex];

        foreach (var entry in _assignedColors)
        {
            if (entry.Value == newColor && entry.Key != playerID)
                return false;
        }

        _assignedColors[playerID] = newColor;
        return true;
    }
}
