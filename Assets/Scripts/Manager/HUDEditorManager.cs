using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class HUDEditorManager : BaseManager
{
    public Button lobbyButton;
    
    public Button loadMapButton;
    public TMP_Dropdown mapDropdown;

    public Button saveMapButton;
    public TMP_InputField mapNameInputField;

    private void Start()
    {
        RefreshMapList();
        loadMapButton.onClick.AddListener(OnLoadMapButtonClick);
    }

    public void OnLobbyButtonClick()
    {
        StartCoroutine(SceneLoader.LoadScene(GameManager.LobbySceneName));
        Debug.Log("Change to Lobby Scene");
    }

    public void RefreshMapList()
    {
        mapDropdown.ClearOptions();
        List<string> mapFiles = new();
        string folder = Application.dataPath + "/Save";

        if (Directory.Exists(folder))
        {
            var files = Directory.GetFiles(folder, "map_*.json");
            foreach (var file in files)
            {
                mapFiles.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        mapDropdown.AddOptions(mapFiles);
    }

    public void OnLoadMapButtonClick()
    {
        string selectedMap = mapDropdown.options[mapDropdown.value].text;
        MapEditor editor = FindAnyObjectByType<MapEditor>();
        if (editor != null)
        {
            editor.LoadMap(selectedMap);
        }
    }

    public void OnSaveMapButtonClick()
    {
        string mapName = mapNameInputField.text.Trim();
        if (string.IsNullOrEmpty(mapName)) return;

        MapEditor editor = FindAnyObjectByType<MapEditor>();
        if (editor != null)
        {
            editor.SaveMap(mapName);
            RefreshMapList();
        }
    }

    public void OnRenameMapButtonClick()
    {
        string selectedMap = mapDropdown.options[mapDropdown.value].text;
        string newMapName = mapNameInputField.text.Trim();
        if (string.IsNullOrEmpty(newMapName)) return;

        if (!newMapName.StartsWith("map_"))
        {
            newMapName = "map_" + newMapName;
        }

        string oldPath = Application.dataPath + "/Save/" + selectedMap + ".json";
        string newPath = Application.dataPath + "/Save/" + newMapName + ".json";

        if (File.Exists(oldPath))
        {
            File.Move(oldPath, newPath);
            RefreshMapList();
        }
    }
}
