using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UIElements.Experimental;
using UnityEngine.Events;

[DefaultExecutionOrder(-50)]

public class HUDManager : BaseManager
{
    public static HUDManager Instance { get; private set; }

    [Header("HUD Elements")]
    [SerializeField] private Image _transition;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _configMenu;
    [SerializeField] private GameObject _audioMenu;
    [SerializeField] private GameObject _gameButton;
    [SerializeField] private GameObject _editorButton;

    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _subtitleText;
    
    [SerializeField] private List<PlayerCardData> _playerCardsData = new List<PlayerCardData>();

    [SerializeField] private List<IntReference> _values;

    private bool _isPaused = false;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        UpdateEditorGameButton();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        _mainMenu.SetActive(_isPaused);
        UpdateEditorGameButton();
        Time.timeScale = _isPaused ? 0 : 1;
    }

    private void UpdateEditorGameButton()
    {
        if (_isPaused)
        {
            bool inEditor = GameManager.CurrentState == GameState.Editor;
            _gameButton.SetActive(inEditor);
            _editorButton.SetActive(!inEditor);
        }
        else
        {
            _gameButton.SetActive(false);
            _editorButton.SetActive(false);
        }
    }

    public void SetTransition(bool enable, float time = 1f)
    {
        if (enable)
        {
            StartCoroutine(FadeOut(time));
        }
        else
        {
            StartCoroutine(FadeIn(time));
        }
    }

    public IEnumerator FadeIn(float time)
    {
        _transition.gameObject.SetActive(true);
        _transition.canvasRenderer.SetAlpha(0);
        _transition.CrossFadeAlpha(1, time, false);
        yield return new WaitForSeconds(time);
    }

    public IEnumerator FadeOut(float time)
    {
        _transition.canvasRenderer.SetAlpha(1);
        _transition.CrossFadeAlpha(0, time, false);
        yield return new WaitForSeconds(time);
        _transition.gameObject.SetActive(false);
    }

    public void OnConfigButtonClick()
    {
        _configMenu.SetActive(!_configMenu.activeSelf);
    }

    public void OnConfigQuitButtonClick()
    {
        _configMenu.SetActive(false);
    }

    public void OnAudioButtonClick()
    {
        _audioMenu.SetActive(!_audioMenu.activeSelf);
    }

    public void OnAudioQuitButtonClick()
    {
        _audioMenu.SetActive(false);
    }

    public void OnResumeButtonClick()
    {
        TogglePause();
    }

    public void OnGameButtonClick()
    {
        StartCoroutine(TransitionToGameScene());
    }

    public void OnEditorButtonClick()
    {
        StartCoroutine(TransitionToEditorScene());
    }

    public IEnumerator TransitionToEditorScene()
    {
        TogglePause();
        SetTransition(false);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SceneLoader.LoadScene(GameManager.EditorSceneName));
    }

    public IEnumerator TransitionToGameScene()
    {
        TogglePause();
        SetTransition(false);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SceneLoader.LoadScene(GameManager.LobbySceneName));
    }

    public void ShowTitle(string title, string subtitle, Color titleColor, Color subtitleColor)
    {
        _titleText.text = title;
        _subtitleText.text = subtitle;
        _titleText.color = titleColor;
        _subtitleText.color = subtitleColor;
    }

    public void ClearTitle()
    {
        _titleText.text = string.Empty;
        _subtitleText.text = string.Empty;
    }


    public void DisplayPlayerCards(int playerID)
    {
        int index = playerID - 1;
        if (index < 0 || index >= _playerCardsData.Count)
        {
            Debug.LogWarning($"Invalid PlayerID: {playerID}");
            return;
        }

        PlayerCardData cardData = _playerCardsData[index];

        if (cardData.PlayerCard != null)
        {
            cardData.PlayerCard.SetActive(true);
            Color playerColor = SkinManager.Instance.GetPlayerColor(playerID);
            cardData.PlayerImage.color = playerColor;
        }
    }

    public void UpdatePlayerHealth(int playerID, float healthPercentage)
    {
        int index = playerID - 1;
        if (index >= 0 && index < _playerCardsData.Count)
        {
            PlayerCardData playerCard = _playerCardsData[index];
    
            if (Mathf.Approximately(playerCard.LastHealthPercentage, healthPercentage))
                return;
    
            playerCard.LastHealthPercentage = healthPercentage;
            playerCard.HealthPourcent.text = $"{Mathf.FloorToInt(healthPercentage)}%";
            StartCoroutine(AnimateHealthChange(playerCard.HealthPourcent));
        }
    }

    public void UpdateColorPlayerCard(int playerID, Color color)
    {
        int index = playerID - 1;
        if (index >= 0 && index < _playerCardsData.Count)
        {
            PlayerCardData playerCard = _playerCardsData[index];
            if (playerCard.PlayerImage != null)
            {
                playerCard.PlayerImage.color = color;
            }
        }
    }

    public void DeleteAllPlayerCards()
    {
        foreach (var playerCard in _playerCardsData)
        {
            if (playerCard.PlayerCard != null)
            {
                playerCard.PlayerCard.SetActive(false);
            }
        }
    }


    private IEnumerator AnimateHealthChange(TMP_Text healthText)
    {
        Vector3 originalScale = healthText.transform.localScale;
        Color originalColor = healthText.color;

        float duration = 0.15f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(1f, 1.2f, time / duration);
            healthText.transform.localScale = originalScale * scale;

            Color highlightColor = Color.red;
            healthText.color = Color.Lerp(originalColor, highlightColor, time / duration);

            yield return null;
        }

        healthText.transform.localScale = originalScale;
        healthText.color = originalColor;
    }


    public void OnClickUpValue(int index)
    {
        if (index >= 0 && index < _values.Count)
        {
            _values[index].Increment();
            print($"Value at index {index} incremented to: {_values[index].GetValue()}");
        }
        else
        {
            Debug.LogWarning($"Index {index} est invalide dans la liste _values (taille = {_values.Count})");
        }
    }


    public void OnClickDownValue(int index)
    {
        if (index >= 0 && index < _values.Count)
        {
            _values[index].Decrement();
            print($"Value at index {index} decremented to: {_values[index].GetValue()}");
        }
        else
        {
            Debug.LogWarning($"Index {index} est invalide dans la liste _values (taille = {_values.Count})");
        }
    }

    public int GetValue(int index)
    {
        return (index >= 0 && index < _values.Count) ? _values[index].GetValue() : 0;
    }

}
