using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.IO;

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
    public TMP_Text globalMapName;
    public bool confirmed = false;

    private string saveDirectory = "Assets/Save";
    private string actionToConfirm = "";
    private string currentMapName = "";

    public List<BlockButtonBinding> blockButtons = new List<BlockButtonBinding>();
    private Color selectedColor = Color.yellow;
    private Color defaultColor = Color.white;

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

        for (int i = 0; i < blockButtons.Count; i++)
        {
            var blockButton = blockButtons[i];
            blockButton.button.onClick.AddListener(() => OnBlockTypeButtonClick(blockButton.blockType));
            var buttonImage = blockButton.button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = defaultColor;
            }
        }
    }

    public void OnBlockTypeButtonClick(BlockType type)
    {
        MapEditor.SetCurrentBlockByType(type);
        HighlightBlockTypeButton(type);
    }

    public void HighlightBlockTypeButton(BlockType type)
    {
        foreach (var blockButton in blockButtons)
        {
            var buttonImage = blockButton.button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = blockButton.blockType == type ? selectedColor : defaultColor;
            }
        }
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
        StartCoroutine(TransitionToEditorScene());
    }

    public IEnumerator TransitionToEditorScene()
    {
        HUDManager.EnableHUD(false);
        yield return new WaitForSeconds(1f);

        StartCoroutine(SceneLoader.LoadScene(GameManager.LobbySceneName));
    }

    public void OnPlayButtonClick()
    {
        var validation = MapEditor.Instance.ValidateMap();
        if (!validation.isValid)
        {
            MessageUI(validation.errorMessage, Color.red, false);
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
        if (!MapEditor.HasBeenTestedAndValid)
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
        if (string.IsNullOrEmpty(currentMapName))
        {
            MessageUI("No map is currently loaded.", Color.red, false);
            return;
        }

        string newMapName = mapNameInputField.text.Trim();

        if (string.IsNullOrEmpty(newMapName))
        {
            MessageUI("New map name cannot be empty.", Color.red, false);
            return;
        }

        if (newMapName == currentMapName)
        {
            MessageUI("New map name is the same as the current one.", Color.red, false);
            return;
        }

        actionToConfirm = "rename";
        MessageUI($"Are you sure you want to rename the map '{currentMapName}' to '{newMapName}'?", Color.green, true);
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

        currentMapName = mapName;
        MapEditor.Instance.LoadBlocksFromSaveData(loadedBlocks);
        MessageUI($"Map '{mapName}' loaded successfully.", Color.green, false);

        globalMapName.text = "CURRENT MAP: " + mapName;
        mapNameInputField.text = mapName;
        
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
        string newMapName = mapNameInputField.text.Trim();

        if (string.IsNullOrEmpty(newMapName))
        {
            MessageUI("New map name cannot be empty.", Color.red, false);
            return;
        }

        string oldFilePath = Path.Combine(saveDirectory, currentMapName + ".map");
        string newFilePath = Path.Combine(saveDirectory, newMapName + ".map");

        if (File.Exists(newFilePath))
        {
            MessageUI($"Map '{newMapName}' already exists.", Color.red, false);
            return;
        }

        if (File.Exists(oldFilePath))
        {
            File.Move(oldFilePath, newFilePath);
            currentMapName = newMapName;
            RefreshMapDropdown();
            MessageUI($"Map renamed to '{newMapName}' successfully.", Color.green, false);

            globalMapName.text = "CURRENT MAP: " + newMapName;
            mapNameInputField.text = newMapName;
        }
        else
        {
            MessageUI($"Map '{currentMapName}' not found.", Color.red, false);
        }
    }

    [System.Serializable]
    public class BlockButtonBinding
    {
        public Button button;
        public BlockType blockType;
    }
}
