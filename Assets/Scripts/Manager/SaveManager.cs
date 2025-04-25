using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class SaveManager
{
    private static string saveDirectory = "Assets/Save";

    public static void SaveMap(string mapName, List<MapEditor.PlacedBlock> placedBlocks)
    {
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        string path = Path.Combine(saveDirectory, mapName + ".map");

        MapSaveData saveData = new MapSaveData
        {
            placedBlocks = placedBlocks.Select(b => new MapSaveData.BlockData
            {
                id = b.id,
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
        string path = Path.Combine(saveDirectory, mapName + ".map");

        if (!File.Exists(path))
        {
            Debug.LogWarning("Map file not found: " + path);
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


    public static void DeleteMap(string mapName)
    {
        string path = Path.Combine(saveDirectory, mapName + ".map");

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Map deleted: " + path);
        }
        else
        {
            Debug.LogWarning("Map file not found: " + path);
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


    [System.Serializable]
    public class MapSaveData
    {
        public List<BlockData> placedBlocks;

        [System.Serializable]
        public class BlockData
        {
            public string id;
            public Vector3 position;
        }
    }
}
