using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class HUDEditorManager : BaseManager
{   
    public static HUDEditorManager Instance { get; private set; }

    public TMP_Dropdown mapDropdown;
    public TMP_InputField mapNameInputField;
    public Image errorImage;
    public TMP_Text feedbackText;
    public Toggle mirrorModeToggle;
    public Toggle knowCenter;
    public GameObject mirrorModeImage;
    public GameObject killBoxImage;

    public GameObject editorUI;
    public GameObject testerUI;

    private void Awake()
    {
        RefreshMapList();
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

    public void OnLobbyButtonClick()
    {
        HUDManager.gameObject.SetActive(true);
        StartCoroutine(GoToLobbyScene());
    }

    public IEnumerator GoToLobbyScene()
    {
        HUDManager.EnableHUD(false);
        yield return new WaitForSeconds(1f);

        StartCoroutine(SceneLoader.LoadScene(GameManager.LobbySceneName));
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
        if (mapDropdown.options.Count == 0)
        {
            ErrorMessage("Aucune carte à charger.");
            return;
        }

        string selectedMap = mapDropdown.options[mapDropdown.value].text;
        MapEditor editor = FindAnyObjectByType<MapEditor>();
        if (editor != null)
        {
            editor.LoadMap(selectedMap);
            SuccessMessage($"Carte \"{selectedMap}\" chargée.");
        }
        else
        {
            ErrorMessage("Éditeur introuvable.");
        }
    }

    public void OnSaveMapButtonClick()
    {
        string mapName = mapNameInputField.text.Trim();
        if (string.IsNullOrEmpty(mapName))
        {
            ErrorMessage("Nom de la carte invalide.");
            return;
        }

        MapEditor editor = FindAnyObjectByType<MapEditor>();
        if (editor != null)
        {
            editor.SaveMap(mapName);
            RefreshMapList();
        }
        else
        {
            ErrorMessage("Éditeur introuvable.");
        }
    }

    public void OnRenameMapButtonClick()
    {
        if (mapDropdown.options.Count == 0)
        {
            ErrorMessage("Aucune carte à renommer.");
            return;
        }

        string selectedMap = mapDropdown.options[mapDropdown.value].text;
        string newMapName = mapNameInputField.text.Trim();
        if (string.IsNullOrEmpty(newMapName))
        {
            ErrorMessage("Nom invalide.");
            return;
        }

        if (!newMapName.StartsWith("map_"))
        {
            newMapName = "map_" + newMapName;
        }

        string oldPath = Application.dataPath + "/Save/" + selectedMap + ".json";
        string newPath = Application.dataPath + "/Save/" + newMapName + ".json";

        if (File.Exists(newPath))
        {
            ErrorMessage("Une carte avec ce nom existe déjà.");
            return;
        }

        if (File.Exists(oldPath))
        {
            File.Move(oldPath, newPath);
            RefreshMapList();
            SuccessMessage($"Carte renommée en \"{newMapName}\".");
        }
        else
        {
            ErrorMessage("Carte d'origine introuvable.");
        }
    }

    public void ErrorMessage(string message)
    {
        errorImage.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = Color.red;
    }

    public void SuccessMessage(string message)
    {
        errorImage.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = Color.green;
    }

    public void OnOKButtonClick()
    {
        errorImage.gameObject.SetActive(false);
        feedbackText.text = string.Empty;
    }

    // Blocks Selection

    public void OnBlockButtonClick(int blockIndex)
    {
        MapEditor.Instance.SetBlockIndex(blockIndex);
    }

    // Start/Stop

    public void OnStartButtonClick()
    {
        MapTester.Instance.StartTestMatch();
    }

    public void OnStopButtonClick()
    {
        MapTester.Instance.StopTestMatch();
    }

    public void OnMirrorModeToggleChanged()
    {
        if (knowCenter.isOn)
        {
            mirrorModeImage.SetActive(true);
        }
        else
        {
            mirrorModeImage.SetActive(false);
        }
    }
}
