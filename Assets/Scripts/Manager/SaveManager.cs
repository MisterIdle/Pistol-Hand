using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveManager
{
    private static string key = "MapEditorInternalKey";

    private static string GetSaveFolderPath()
    {
        string saveFolder = Path.Combine(Application.persistentDataPath, "Save");
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
            Debug.Log($"Dossier de sauvegarde créé : {saveFolder}");
        }
        return saveFolder;
    }

    public static void SaveMap(string fileName, List<(string id, Vector3 pos, string sprite)> blocks)
    {
        string saveFolderPath = GetSaveFolderPath();

        MapData mapData = new();
        foreach (var block in blocks)
        {
            mapData.blocks.Add(new BlockData
            {
                blockID = block.id,
                position = block.pos,
                spriteName = block.sprite
            });
        }

        string json = JsonUtility.ToJson(mapData);
        byte[] encrypted = Encrypt(json);
        File.WriteAllBytes(Path.Combine(saveFolderPath, fileName + ".map"), encrypted);
    }

    public static List<(string id, Vector3 pos)> LoadMap(string fileName)
    {
        string path = Path.Combine(GetSaveFolderPath(), fileName + ".map");
        if (!File.Exists(path)) return null;

        byte[] data = File.ReadAllBytes(path);
        string json = TryDecrypt(data);
        if (string.IsNullOrEmpty(json)) return null;

        MapData mapData = JsonUtility.FromJson<MapData>(json);
        return mapData.blocks.Select(b => (b.blockID, b.position)).ToList();
    }

    public static List<(string id, Vector3 pos)> LoadRandomMap()
    {
        string saveFolder = GetSaveFolderPath();

        var mapFiles = Directory.GetFiles(saveFolder, "*.map").ToList();

        if (mapFiles.Count == 0)
        {
            Debug.LogError("Aucune carte trouvée.");
            return null;
        }

        string randomMap = mapFiles[Random.Range(0, mapFiles.Count)];
        string mapName = Path.GetFileNameWithoutExtension(randomMap);

        Debug.Log($"Carte aléatoire choisie : {mapName}");

        return LoadMap(mapName);
    }

    private static string TryDecrypt(byte[] data)
    {
        try
        {
            return Decrypt(data);
        }
        catch
        {
            try
            {
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return null;
            }
        }
    }

    private static byte[] Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = GetKey();
        aes.IV = new byte[16];

        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamWriter sw = new(cs, Encoding.UTF8);
        sw.Write(plainText);
        sw.Flush();
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    private static string Decrypt(byte[] cipherData)
    {
        using Aes aes = Aes.Create();
        aes.Key = GetKey();
        aes.IV = new byte[16];

        using MemoryStream ms = new(cipherData);
        using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader sr = new(cs, Encoding.UTF8);
        return sr.ReadToEnd();
    }

    private static byte[] GetKey()
    {
        using SHA256 sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(key));
    }
}
