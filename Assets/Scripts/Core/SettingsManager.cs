using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-110)]
public class SettingsManager : BaseManager
{
    public static SettingsManager Instance { get; private set; }
    public GameParametersDatabase gameParametersDatabase;
    private List<SerializableParameter> parametersList = new List<SerializableParameter>();

    private AudioParameter audioParameter = new AudioParameter();
    
    private string savePath = "Settings/GameParams.json";
    private string audioPath = "Settings/AudioParams.json";

    private void Awake()
    {
        InitializeSingleton();

        LoadGameParameters();
        LoadAudioParameters();
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

    public void SaveGameParameters()
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

    public void LoadGameParameters()
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

            SaveGameParameters();
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

        SaveGameParameters();
    }
    
    public void SaveAudioParameters()
    {
        string directory = Path.GetDirectoryName(audioPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(audioParameter, true);
        File.WriteAllText(audioPath, json);
    }

    public void LoadAudioParameters()
    {
        if (File.Exists(audioPath))
        {
            string json = File.ReadAllText(audioPath);
            audioParameter = JsonUtility.FromJson<AudioParameter>(json);
        }
        else
        {
            audioParameter = new AudioParameter();
            SaveAudioParameters();
        }

        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        AudioManager.MusicVolume(audioParameter.musicVolume);
        AudioManager.SFXVolume(audioParameter.sfxVolume);
    }

    public AudioParameter GetAudioParameter()
    {
        return audioParameter;
    }

}
