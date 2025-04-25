using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class HUDEditorManager : BaseManager
{
    public static HUDEditorManager Instance { get; private set; }

    public GameObject editorUI;
    public GameObject messageUI;
    public GameObject messageUINormal;
    public GameObject messageUIConfirm;
    public TMP_Text messageUIText;
    public GameObject center;
    public GameObject testerUI;
    public TMP_Dropdown allMapsDropdown;
    public TMP_InputField mapNameInputField;
    public bool confirmed = false;

    private string saveDirectory = "Assets/Save";
    private string actionToConfirm = "";

    private void Awake()
    {
        InitializeSingleton();
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

    private void Start()
    {
        PopulateMapDropdown();
    }

    private void PopulateMapDropdown()
    {
        if (!Directory.Exists(saveDirectory)) return;

        string[] mapFiles = Directory.GetFiles(saveDirectory, "*.map");
        List<string> mapNames = new List<string>();

        foreach (var mapFile in mapFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(mapFile);
            mapNames.Add(fileName);
        }

        allMapsDropdown.ClearOptions();
        allMapsDropdown.AddOptions(mapNames);
    }

    private void RefreshMapDropdown()
    {
        allMapsDropdown.ClearOptions();
        PopulateMapDropdown();
    }

    public void MessageUI(string message, Color color, bool isConfirm = false)
    {
        messageUI.SetActive(true);
        messageUIText.color = color;
        messageUIText.text = message;

        if (isConfirm)
        {
            messageUIConfirm.gameObject.SetActive(true);
            messageUINormal.gameObject.SetActive(false);
        }
        else
        {
            messageUIConfirm.gameObject.SetActive(false);
            messageUINormal.gameObject.SetActive(true);
        }
    }

    public void OnCloseMessageUI()
    {
        messageUI.SetActive(false);
        messageUIText.text = string.Empty;
    }

    public void OnConfirmClick()
    {
        switch (actionToConfirm)
        {
            case "save":
                OnConfirmSave();
                break;
            case "load":
                OnConfirmLoad();
                break;
            case "delete":
                OnConfirmDelete();
                break;
            case "clear":
                OnConfirmClear();
                break;
            case "rename":
                OnConfirmRename();
                break;
            default:
                MessageUI("Unknown action", Color.red, false);
                break;
        }
    }

    public void OnCancelClick()
    {
        actionToConfirm = "";
        confirmed = false;
        messageUI.SetActive(false);
        messageUIText.text = string.Empty;
    }

    public void OnLobbyButtonClick()
    {
        HUDManager.gameObject.SetActive(true);
    }

    public void OnPlayButtonClick()
    {
        if (!MapEditor.CompletMap())
        {
            MessageUI("Please complete the map before testing.", Color.red, false);
            return;
        }

        editorUI.SetActive(false);
        testerUI.SetActive(true);
        center.SetActive(false);
        MapTester.StartTestMatch();
    }

    public void OnBackButtonClick()
    {
        editorUI.SetActive(true);
        testerUI.SetActive(false);
        center.SetActive(true);
        MapTester.StopTestMatch();
    }

    public void OnMirrorToggle()
    {
        MapEditor.ToggleMirror();
    }

    public void OnGridToggle()
    {
        MapEditor.ToggleGrid();
    }

    public void OnSaveButtonClick()
    {
        if (!MapEditor.CompletMap())
        {
            MessageUI("Please complete the map before saving.", Color.red, false);
            return;
        }

        string mapName = mapNameInputField.text.Trim();

        if (string.IsNullOrEmpty(mapName))
        {
            MessageUI("Map name cannot be empty.", Color.red, false);
            return;
        }

        actionToConfirm = "save";
        MessageUI($"Are you sure you want to save the map '{mapName}'?", Color.green, true);
    }

    public void OnLoadButtonClick()
    {
        if (allMapsDropdown.options.Count == 0)
        {
            MessageUI("No maps available to load.", Color.red, false);
            return;
        }

        string mapName = allMapsDropdown.options[allMapsDropdown.value].text.Trim();

        if (string.IsNullOrEmpty(mapName))
        {
            MessageUI("Please select a map to load.", Color.red, false);
            return;
        }

        actionToConfirm = "load";
        MessageUI($"Are you sure you want to load the map '{mapName}'?", Color.green, true);
    }

    public void OnDeleteButtonClick()
    {
        string mapName = allMapsDropdown.options[allMapsDropdown.value].text.Trim();
        actionToConfirm = "delete";
        MessageUI($"Are you sure you want to delete the map '{mapName}'?", Color.red, true);
    }

    public void OnClearButtonClick()
    {
        actionToConfirm = "clear";
        MessageUI("Are you sure you want to clear the map?", Color.red, true);
    }

    public void OnRenameButtonClick()
    {
        string mapName = allMapsDropdown.options[allMapsDropdown.value].text.Trim();
        actionToConfirm = "rename";
        MessageUI($"Are you sure you want to rename the map '{mapName}'?", Color.green, true);
    }

    private void OnConfirmSave()
    {
        string mapName = mapNameInputField.text.Trim();
        SaveManager.SaveMap(mapName, MapEditor.GetPlacedBlocks());
        RefreshMapDropdown();
        MessageUI($"Map '{mapName}' saved successfully.", Color.green, false);
    }

    private void OnConfirmLoad()
    {
        string mapName = allMapsDropdown.options[allMapsDropdown.value].text.Trim();

        List<SaveManager.MapSaveData.BlockData> loadedBlocks = SaveManager.LoadMap(mapName);
        if (loadedBlocks == null || loadedBlocks.Count == 0)
        {
            MessageUI($"Failed to load the map '{mapName}'.", Color.red, false);
            return;
        }

        MapEditor.Instance.LoadBlocksFromSaveData(loadedBlocks);
        MessageUI($"Map '{mapName}' loaded successfully.", Color.green, false);
    }

    private void OnConfirmDelete()
    {
        string mapName = allMapsDropdown.options[allMapsDropdown.value].text.Trim();
        string filePath = Path.Combine(saveDirectory, mapName + ".map");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            RefreshMapDropdown();
            MessageUI($"Map '{mapName}' deleted successfully.", Color.green, false);
        }
        else
        {
            MessageUI($"Map '{mapName}' not found.", Color.red, false);
        }
    }

    private void OnConfirmClear()
    {
        MapEditor.Instance.ClearMap();
        MessageUI("Map cleared successfully.", Color.green, false);
    }

    private void OnConfirmRename()
    {
        string mapName = allMapsDropdown.options[allMapsDropdown.value].text.Trim();
        string newMapName = mapNameInputField.text.Trim();

        if (string.IsNullOrEmpty(newMapName))
        {
            MessageUI("New map name cannot be empty.", Color.red, false);
            return;
        }

        string oldFilePath = Path.Combine(saveDirectory, mapName + ".map");
        string newFilePath = Path.Combine(saveDirectory, newMapName + ".map");

        if (File.Exists(oldFilePath))
        {
            File.Move(oldFilePath, newFilePath);
            RefreshMapDropdown();
            MessageUI($"Map '{mapName}' renamed to '{newMapName}' successfully.", Color.green, false);
        }
        else
        {
            MessageUI($"Map '{mapName}' not found.", Color.red, false);
        }
    }
}

