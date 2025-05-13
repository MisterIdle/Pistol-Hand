using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[DefaultExecutionOrder(-150)]
public static class SaveManager
{
    private static string _UserSaveDirectory = "Save";
    private static string _DefaultSaveDirectory = Application.streamingAssetsPath + "/Save";
    private static string _NeededDirectory = Application.streamingAssetsPath + "/Save/Needed";

    public static void SaveMap(string mapName, List<PlacedBlock> placedBlocks)
    {
        if (!Directory.Exists(_UserSaveDirectory))
        {
            Directory.CreateDirectory(_UserSaveDirectory);
        }

        string path = Path.Combine(_UserSaveDirectory, mapName + ".map");

        MapSaveData saveData = new MapSaveData
        {
            placedBlocks = placedBlocks.Select(b => new MapSaveData.BlockData
            {
                type = b.type,
                position = b.instance.transform.position
            }).ToList()
        };

        string json = JsonUtility.ToJson(saveData, true);

        string encryptedData = EncryptDecrypt(json);

        File.WriteAllText(path, encryptedData);
        Debug.Log("Map saved to " + path);
    }

    public static List<MapSaveData.BlockData> LoadMap(string mapName)
    {
        string userPath = Path.Combine(_UserSaveDirectory, mapName + ".map");
        string defaultPath = Path.Combine(_DefaultSaveDirectory, mapName + ".map");
        string neededPath = Path.Combine(_NeededDirectory, mapName + ".map");

        string path = File.Exists(userPath) ? userPath : File.Exists(defaultPath) ? defaultPath : neededPath;

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogWarning("Map not found in user or default directories: " + mapName);
            return null;
        }

        string encryptedData = File.ReadAllText(path);

        if (string.IsNullOrEmpty(encryptedData))
        {
            Debug.LogWarning("The map file is empty: " + path);
            return null;
        }

        string decryptedData = EncryptDecrypt(encryptedData);
        MapSaveData saveData = JsonUtility.FromJson<MapSaveData>(decryptedData);

        Debug.Log("Map loaded from " + path);
        return saveData.placedBlocks;
    }

    public static List<string> GetAllUsersMaps()
    {
        if (!Directory.Exists(_UserSaveDirectory))
        {
            Debug.LogWarning("Save directory does not exist: " + _UserSaveDirectory);
            return new List<string>();
        }

        string[] files = Directory.GetFiles(_UserSaveDirectory, "*.map");
        List<string> mapNames = files.Select(Path.GetFileNameWithoutExtension).ToList();

        Debug.Log("Available maps (User): " + string.Join(", ", mapNames));
        return mapNames;
    }

    public static List<string> GetAllDefaultMaps()
    {
        if (!Directory.Exists(_DefaultSaveDirectory))
        {
            Debug.LogWarning("Default save directory does not exist: " + _DefaultSaveDirectory);
            return new List<string>();
        }

        string[] files = Directory.GetFiles(_DefaultSaveDirectory, "*.map");
        List<string> mapNames = files.Select(Path.GetFileNameWithoutExtension).ToList();

        Debug.Log("Available maps (Default):" + string.Join(", ", mapNames));
        return mapNames;
    }

    public static List<string> GetAllNeededMaps()
    {
        if (!Directory.Exists(_NeededDirectory))
        {
            Debug.LogWarning("Needed save directory does not exist: " + _NeededDirectory);
            return new List<string>();
        }

        string[] files = Directory.GetFiles(_NeededDirectory, "*.map");
        List<string> mapNames = files.Select(Path.GetFileNameWithoutExtension).ToList();

        Debug.Log("Available maps (Needed):" + string.Join(", ", mapNames));
        return mapNames;
    }

    public static void DeleteMap(string mapName)
    {
        string path = Path.Combine(_UserSaveDirectory, mapName + ".map");
        string metaPath = path + ".meta";

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Map deleted: " + path);
        }
        else
        {
            Debug.LogWarning("Map file not found: " + path);
        }

        if (File.Exists(metaPath))
        {
            File.Delete(metaPath);
            Debug.Log("Meta file deleted: " + metaPath);
        }
        else
        {
            Debug.LogWarning("Meta file not found: " + metaPath);
        }
    }

    private static string EncryptDecrypt(string input)
    {
        StringBuilder output = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            output.Append((char)(c ^ 0xAA)); 
        }

        return output.ToString();
    }
}
