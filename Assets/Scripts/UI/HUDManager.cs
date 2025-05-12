using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

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

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
        MessageUIObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SceneLoader.LoadScene(GameManager.EditorSceneName));
    }

    public IEnumerator TransitionToGameScene()
    {
        TogglePause();
        SetTransition(false);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SceneLoader.LoadScene(GameManager.LobbySceneName));
        MessageUIObject.SetActive(true);
    }

    public void ShowTitle(string title, string subtitle, Color titleColor, Color subtitleColor)
    {
        BackgroundImage.gameObject.SetActive(true);
        _titleText.text = title;
        _subtitleText.text = subtitle;
        _titleText.color = titleColor;
        _subtitleText.color = subtitleColor;
    }

    public void ClearTitle()
    {
        BackgroundImage.gameObject.SetActive(false);
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

}
