using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class HUDEditorManager : BaseManager
{
    public static HUDEditorManager Instance { get; private set; }

    [Header("UI Card")]
    public GameObject EditorUIObject;
    public GameObject MessageUIObject;
    public GameObject MessageUINormal;
    public GameObject MessageUIConfirm;

    [Header("UI Message")]
    public TMP_Text MessageUIText;

    [Header("UI Map")]
    public GameObject Center;
    public GameObject TesterUI;
    public TMP_Dropdown AllMapsDropdown;
    public TMP_InputField MapNameInputField;
    public TMP_Text GlobalMapName;
    public bool _confirmed = false;

    private string _saveDirectory = "Assets/Save";
    private string _actionToConfirm = "";
    private string _currentMapName = "";

    public List<BlockButtonBinding> BlockButtons = new List<BlockButtonBinding>();
    private Color SelectedColor = Color.yellow;
    private Color DefaultColor = Color.white;

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

        for (int i = 0; i < BlockButtons.Count; i++)
        {
            var blockButton = BlockButtons[i];
            blockButton.button.onClick.AddListener(() => OnBlockTypeButtonClick(blockButton.blockType));
            var buttonImage = blockButton.button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = DefaultColor;
            }
        }

        HUDManager.MessageUIObject.SetActive(false);
    }

    public void OnBlockTypeButtonClick(BlockType type)
    {
        MapEditor.SetCurrentBlockByType(type);
        HighlightBlockTypeButton(type);
    }

    public void HighlightBlockTypeButton(BlockType type)
    {
        foreach (var blockButton in BlockButtons)
        {
            var buttonImage = blockButton.button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = blockButton.blockType == type ? SelectedColor : DefaultColor;
            }
        }
    }

    private void PopulateMapDropdown()
    {
        if (!Directory.Exists(_saveDirectory)) return;

        string[] mapFiles = Directory.GetFiles(_saveDirectory, "*.map");
        List<string> mapNames = new List<string>();

        foreach (var mapFile in mapFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(mapFile);
            mapNames.Add(fileName);
        }

        AllMapsDropdown.ClearOptions();
        AllMapsDropdown.AddOptions(mapNames);
    }

    private void RefreshMapDropdown()
    {
        AllMapsDropdown.ClearOptions();
        PopulateMapDropdown();
    }

    public void MessageUI(string message, Color color, bool isConfirm = false)
    {
        MessageUIObject.SetActive(true);
        MessageUIText.color = color;
        MessageUIText.text = message;

        if (isConfirm)
        {
            MessageUIConfirm.gameObject.SetActive(true);
            MessageUINormal.gameObject.SetActive(false);
        }
        else
        {
            MessageUIConfirm.gameObject.SetActive(false);
            MessageUINormal.gameObject.SetActive(true);
        }
    }

    public void OnCloseMessageUI()
    {
        MessageUIObject.SetActive(false);
        MessageUIText.text = string.Empty;
    }

    public void OnConfirmClick()
    {
        switch (_actionToConfirm)
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
        _actionToConfirm = "";
        _confirmed = false;
        MessageUIObject.SetActive(false);
        MessageUIText.text = string.Empty;
    }

    public IEnumerator TransitionToEditorScene()
    {
        HUDManager.SetTransition(false);
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
    
        EditorUIObject.SetActive(false);
        TesterUI.SetActive(true);
        Center.SetActive(false);
        MapTester.StartTestMatch();
    }


    public void OnBackButtonClick()
    {
        EditorUIObject.SetActive(true);
        TesterUI.SetActive(false);
        Center.SetActive(true);
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
        //if (!MapEditor.HasBeenTestedAndValid)
        //{
        //    MessageUI("Please complete the map before saving.", Color.red, false);
        //    return;
        //}

        string mapName = MapNameInputField.text.Trim();

        if (string.IsNullOrEmpty(mapName))
        {
            MessageUI("Map name cannot be empty.", Color.red, false);
            return;
        }

        _actionToConfirm = "save";
        MessageUI($"Are you sure you want to save the map '{mapName}'?", Color.green, true);
    }

    public void OnLoadButtonClick()
    {
        if (AllMapsDropdown.options.Count == 0)
        {
            MessageUI("No maps available to load.", Color.red, false);
            return;
        }

        string mapName = AllMapsDropdown.options[AllMapsDropdown.value].text.Trim();

        if (string.IsNullOrEmpty(mapName))
        {
            MessageUI("Please select a map to load.", Color.red, false);
            return;
        }

        _actionToConfirm = "load";
        MessageUI($"Are you sure you want to load the map '{mapName}'?", Color.green, true);
    }

    public void OnDeleteButtonClick()
    {
        string mapName = AllMapsDropdown.options[AllMapsDropdown.value].text.Trim();
        _actionToConfirm = "delete";
        MessageUI($"Are you sure you want to delete the map '{mapName}'?", Color.red, true);
    }

    public void OnClearButtonClick()
    {
        _actionToConfirm = "clear";
        MessageUI("Are you sure you want to clear the map?", Color.red, true);
    }

    public void OnRenameButtonClick()
    {
        if (string.IsNullOrEmpty(_currentMapName))
        {
            MessageUI("No map is currently loaded.", Color.red, false);
            return;
        }

        string newMapName = MapNameInputField.text.Trim();

        if (string.IsNullOrEmpty(newMapName))
        {
            MessageUI("New map name cannot be empty.", Color.red, false);
            return;
        }

        if (newMapName == _currentMapName)
        {
            MessageUI("New map name is the same as the current one.", Color.red, false);
            return;
        }

        _actionToConfirm = "rename";
        MessageUI($"Are you sure you want to rename the map '{_currentMapName}' to '{newMapName}'?", Color.green, true);
    }

    private void OnConfirmSave()
    {
        string mapName = MapNameInputField.text.Trim();
        SaveManager.SaveMap(mapName, MapEditor.GetPlacedBlocks());
        RefreshMapDropdown();
        MessageUI($"Map '{mapName}' saved successfully.", Color.green, false);
    }

    private void OnConfirmLoad()
    {
        string mapName = AllMapsDropdown.options[AllMapsDropdown.value].text.Trim();

        List<MapSaveData.BlockData> loadedBlocks = SaveManager.LoadMap(mapName);
        if (loadedBlocks == null || loadedBlocks.Count == 0)
        {
            MessageUI($"Failed to load the map '{mapName}'.", Color.red, false);
            return;
        }

        _currentMapName = mapName;
        MapEditor.LoadBlocksFromSaveData(loadedBlocks);
        MessageUI($"Map '{mapName}' loaded successfully.", Color.green, false);

        GlobalMapName.text = "CURRENT MAP: " + mapName;
        MapNameInputField.text = mapName;
        
    }

    private void OnConfirmDelete()
    {
        string mapName = AllMapsDropdown.options[AllMapsDropdown.value].text.Trim();
        string filePath = Path.Combine(_saveDirectory, mapName + ".map");

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
        string newMapName = MapNameInputField.text.Trim();

        if (string.IsNullOrEmpty(newMapName))
        {
            MessageUI("New map name cannot be empty.", Color.red, false);
            return;
        }

        string oldFilePath = Path.Combine(_saveDirectory, _currentMapName + ".map");
        string newFilePath = Path.Combine(_saveDirectory, newMapName + ".map");

        if (File.Exists(newFilePath))
        {
            MessageUI($"Map '{newMapName}' already exists.", Color.red, false);
            return;
        }

        if (File.Exists(oldFilePath))
        {
            File.Move(oldFilePath, newFilePath);
            _currentMapName = newMapName;
            RefreshMapDropdown();
            MessageUI($"Map renamed to '{newMapName}' successfully.", Color.green, false);

            GlobalMapName.text = "CURRENT MAP: " + newMapName;
            MapNameInputField.text = newMapName;
        }
        else
        {
            MessageUI($"Map '{_currentMapName}' not found.", Color.red, false);
        }
    }
}
