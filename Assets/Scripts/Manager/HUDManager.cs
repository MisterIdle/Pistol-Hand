using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]

[System.Serializable]
public class PlayerCardData
{
    public GameObject playerCard;
    public Image playerImage;
    public TMP_Text healthPourcent;
    public float lastHealthPercentage = -1f;
}

public class HUDManager : BaseManager
{
    public Image transition;
    public GameObject mainMenuButton;
    public GameObject gameButton;
    public GameObject editorButton;

    public TMP_Text titleText;
    public TMP_Text subtitleText;

     public List<PlayerCardData> playerCardsData = new List<PlayerCardData>();

    public static HUDManager Instance { get; private set; }

    private bool isPaused = false;

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
        isPaused = !isPaused;
        mainMenuButton.SetActive(isPaused);
        UpdateEditorGameButton();
        Time.timeScale = isPaused ? 0 : 1;
    }

    private void UpdateEditorGameButton()
    {
        if (isPaused)
        {
            bool inEditor = GameManager.Instance.CurrentState == GameState.Editor;
            gameButton.SetActive(inEditor);
            editorButton.SetActive(!inEditor);
        }
        else
        {
            gameButton.SetActive(false);
            editorButton.SetActive(false);
        }
    }

    public void EnableHUD(bool enable, float time = 1f)
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
        transition.gameObject.SetActive(true);
        transition.canvasRenderer.SetAlpha(0);
        transition.CrossFadeAlpha(1, time, false);
        yield return new WaitForSeconds(time);
    }

    public IEnumerator FadeOut(float time)
    {
        transition.canvasRenderer.SetAlpha(1);
        transition.CrossFadeAlpha(0, time, false);
        yield return new WaitForSeconds(time);
        transition.gameObject.SetActive(false);
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
        EnableHUD(false);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SceneLoader.LoadScene(GameManager.EditorSceneName));
    }

    public IEnumerator TransitionToGameScene()
    {
        TogglePause();
        EnableHUD(false);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SceneLoader.LoadScene(GameManager.LobbySceneName));
    }

    public void ShowTitle(string title, string subtitle, Color titleColor, Color subtitleColor)
    {
        titleText.text = title;
        subtitleText.text = subtitle;
        titleText.color = titleColor;
        subtitleText.color = subtitleColor;
    }

    public void ClearTitle()
    {
        titleText.text = string.Empty;
        subtitleText.text = string.Empty;
    }


    public void DisplayPlayerCards(int playerID)
    {
        int index = playerID - 1;
        if (index < 0 || index >= playerCardsData.Count)
        {
            Debug.LogWarning($"Invalid PlayerID: {playerID}");
            return;
        }

        PlayerCardData cardData = playerCardsData[index];

        if (cardData.playerCard != null)
        {
            cardData.playerCard.SetActive(true);
            Color playerColor = SkinManager.Instance.GetPlayerColor(playerID);
            cardData.playerImage.color = playerColor;
        }
    }

    public void UpdatePlayerHealth(int playerID, float healthPercentage)
    {
        int index = playerID - 1;
        if (index >= 0 && index < playerCardsData.Count)
        {
            PlayerCardData playerCard = playerCardsData[index];
    
            if (Mathf.Approximately(playerCard.lastHealthPercentage, healthPercentage))
                return;
    
            playerCard.lastHealthPercentage = healthPercentage;
            playerCard.healthPourcent.text = $"{Mathf.FloorToInt(healthPercentage)}%";
            StartCoroutine(AnimateHealthChange(playerCard.healthPourcent));
        }
    }

    public void UpdateColorPlayerCard(int playerID, Color color)
    {
        int index = playerID - 1;
        if (index >= 0 && index < playerCardsData.Count)
        {
            PlayerCardData playerCard = playerCardsData[index];
            if (playerCard.playerImage != null)
            {
                playerCard.playerImage.color = color;
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
}
