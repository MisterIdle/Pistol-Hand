using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-110)]
public class SettingsManager : BaseManager
{
    public static SettingsManager Instance { get; private set; }
    public GameParametersDatabase gameParametersDatabase;
    private string savePath = "Settings/GameParams.json";
    private List<SerializableParameter> parametersList = new List<SerializableParameter>();

    private void Awake()
    {
        InitializeSingleton();
        LoadParameters();
    }

    private void InitializeSingleton()
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

    public void SaveParameters()
    {
        string directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(new SerializableParameterList { parameters = parametersList }, true);
        File.WriteAllText(savePath, json);

        GameManager.LoadGameSettings();

        var players = GameManager.GetAllPlayers();
        foreach (var player in players)
        {
            player.LoadPlayerSettings();
        }
    }

    public void LoadParameters()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SerializableParameterList loadedParameters = JsonUtility.FromJson<SerializableParameterList>(json);
            parametersList = loadedParameters.parameters;
        }
        else
        {
            parametersList = new List<SerializableParameter>();
            foreach (var param in gameParametersDatabase.parameters)
            {
                parametersList.Add(new SerializableParameter(param));
            }

            SaveParameters();
        }

        Debug.Log("Game parameters loaded from " + savePath);
    }

    public SerializableParameter GetParameterByKey(GameParameterType key)
    {
        return parametersList.FirstOrDefault(p => p.key == key);
    }

    public void DefaultParameters()
    {
        parametersList = new List<SerializableParameter>();
        foreach (var param in gameParametersDatabase.parameters)
        {
            parametersList.Add(new SerializableParameter(param));
        }

        SaveParameters();
    }
}
