using UnityEngine;
using TMPro;

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

    public bool confirmed = false;


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

    public void OnConfirmMessageUI()
    {
        confirmed = true;
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
        if (MapEditor.mirrorEnabled)
        {
            MapEditor.mirrorEnabled = false;
        }
        else
        {
            MapEditor.mirrorEnabled = true;
        }
    }
}
