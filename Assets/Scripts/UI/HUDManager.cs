using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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
    [SerializeField] private GameObject _stopButton;
    [SerializeField] private GameObject _creditMenu;

    [Header("Game HUD")]
    [SerializeField] private List<Button> _parametersButtons = new List<Button>();

    [Header("Audio Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [SerializeField] public GameObject MessageUIObject;
    [SerializeField] public Image BackgroundImage;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _subtitleText;

    private Dictionary<TMP_Text, bool> _healthAnimationStates = new Dictionary<TMP_Text, bool>();
    
    [SerializeField] private List<PlayerCardData> _playerCardsData = new List<PlayerCardData>();
    
    [Header("Value Modifiers")]
    [SerializeField] private List<ValueModifier> _valueModifiers = new List<ValueModifier>();

    public bool IsPaused = false;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        UpdateEditorGameButton();

        AudioParameter audio = SettingsManager.GetAudioParameter();

        musicVolumeSlider.value = audio.musicVolume;
        sfxVolumeSlider.value = audio.sfxVolume;
        
        foreach (var valueModifier in _valueModifiers)
        {
            valueModifier.Initialize();
        }
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
        bool escapePressed = Input.GetKeyDown(KeyCode.Escape) || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

        if (escapePressed)
        {
            if (_creditMenu.activeSelf)
            {
                CloseCreditMenu();
                return;
            }

            TogglePause();
        }
    }


    public void TogglePause()
    {
        IsPaused = !IsPaused;
        _mainMenu.SetActive(IsPaused);
        UpdateEditorGameButton();
        Time.timeScale = IsPaused ? 0 : 1;
    }

    private void UpdateEditorGameButton()
    {
        if (IsPaused)
        {
            switch (GameManager.CurrentState)
            {
                case GameState.WaitingForPlayers:
                    _gameButton.SetActive(false);
                    _editorButton.SetActive(true);
                    _stopButton.SetActive(false);
                    break;
                case GameState.Editor:
                    _gameButton.SetActive(true);
                    _editorButton.SetActive(false);
                    _stopButton.SetActive(false);
                    break;
                case GameState.Playing:
                    _gameButton.SetActive(false);
                    _editorButton.SetActive(false);
                    _stopButton.SetActive(true);
                    break;
                default:
                    _gameButton.SetActive(false);
                    _editorButton.SetActive(false);
                    _stopButton.SetActive(false);
                    break;
            }
        }
        else
        {
            _gameButton.SetActive(false);
            _editorButton.SetActive(false);
            _stopButton.SetActive(false);
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

    public void OnGameButtonClick()
    {
        StartCoroutine(TransitionToGameScene());
    }

    public void OnEditorButtonClick()
    {
        StartCoroutine(TransitionToEditorScene());
    }

    public void OnStopButtonClick()
    {
        StartCoroutine(TransitionToGameScene());
    }

    public IEnumerator TransitionToEditorScene()
    {
        TogglePause();
        SetTransition(false);
        MessageUIObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        HUDManager.HideAllPlayerCards();
        StartCoroutine(SceneLoader.LoadScene(GameManager.EditorSceneName));
    }

    public IEnumerator TransitionToGameScene(bool togglePause = true)
    {
        if (togglePause)
            TogglePause();

        SetTransition(false);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SceneLoader.LoadScene(GameManager.LobbySceneName));
        HUDManager.HideAllPlayerCards();
        MessageUIObject.SetActive(true);
    }

    public void ShowTitle(string title, string subtitle, Color titleColor, Color subtitleColor, float verticalOffset = 0, bool animateTitle = false)
    {
        BackgroundImage.gameObject.SetActive(true);
        _titleText.text = title;
        _titleText.color = titleColor;
        _subtitleText.color = subtitleColor;

        Vector3 offset = new Vector3(0, verticalOffset, 0);
        _titleText.rectTransform.localPosition += offset;
        _subtitleText.rectTransform.localPosition += offset;
        
        _subtitleText.text = subtitle;

        if (animateTitle)
            StartCoroutine(AnimateTitle());
    }

    public void ClearTitle()
    {
        BackgroundImage.gameObject.SetActive(false);
        _titleText.text = string.Empty;
        _subtitleText.text = string.Empty;

        _titleText.rectTransform.localPosition = Vector3.zero;
        _subtitleText.rectTransform.localPosition = new Vector3(0, -112, 0);
    }

    private IEnumerator AnimateTitle()
    {
        Vector3 originalScale = _titleText.transform.localScale;
        float pulse = 1.05f;
        float speed = 2f;

        float time = 0f;
        while (BackgroundImage.gameObject.activeSelf)
        {
            float scale = 1 + Mathf.Sin(time * speed) * (pulse - 1);
            _titleText.transform.localScale = originalScale * scale;
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        _titleText.transform.localScale = originalScale;
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

    public void UpdatePlayerHealth(int playerID, float health)
    {
        int index = playerID - 1;
        if (index >= 0 && index < _playerCardsData.Count)
        {
            PlayerCardData playerCard = _playerCardsData[index];
    
            if (Mathf.Approximately(playerCard.LastHealth, health))
                return;
    
            playerCard.LastHealth = health;
            playerCard.Health.text = $"{health:F0} HP";
            StartCoroutine(AnimateHealthChange(playerCard.Health));
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

    public void HideAllPlayerCards()
    {
        foreach (var playerCard in _playerCardsData)
        {
            if (playerCard.PlayerCard != null)
            {
                playerCard.PlayerCard.SetActive(false);
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
        if (_healthAnimationStates.TryGetValue(healthText, out bool isAnimating) && isAnimating)
            yield break;

        _healthAnimationStates[healthText] = true;

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

        _healthAnimationStates[healthText] = false;
    }

    public void OnClickReset()
    {
        SettingsManager.Instance.DefaultParameters();

        foreach (var valueModifier in _valueModifiers)
        {
            valueModifier.UpdateValueText();
        }
    }

    public void SetMusicVolume()
    {
        AudioManager.MusicVolume(musicVolumeSlider.value);
        SettingsManager.GetAudioParameter().musicVolume = musicVolumeSlider.value;
        SettingsManager.SaveAudioParameters();
    }

    public void SetSFXVolume()
    {
        AudioManager.SFXVolume(sfxVolumeSlider.value);
        SettingsManager.GetAudioParameter().sfxVolume = sfxVolumeSlider.value;
        SettingsManager.SaveAudioParameters();
    }

    public void EnableParameterButton(bool enable)
    {
        foreach (var button in _parametersButtons)
        {
            button.gameObject.SetActive(enable);
            button.interactable = enable;
        }
    }

    public void OnCreditButtonClick()
    {
        _mainMenu.SetActive(false);
        _creditMenu.SetActive(true);
    }

    public void CloseCreditMenu()
    {
        _creditMenu.SetActive(false);
        _mainMenu.SetActive(true);
    }

    public void OnQuitButtonClick()
    {
        Application.Quit();
    }
}
