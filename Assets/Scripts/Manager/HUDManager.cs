using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class HUDManager : BaseManager
{
    public Image transition;
    public Button editorButton;

    public static HUDManager Instance { get; private set; }

    private void Awake()
    {
        InitializeSingleton();
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

    public void OnEditorButtonClick()
    {
        StartCoroutine(TransitionToEditorScene());
    }

    public IEnumerator TransitionToEditorScene()
    {
        EnableHUD(false);
        yield return new WaitForSeconds(1f);

        StartCoroutine(SceneLoader.LoadScene(GameManager.EditorSceneName));
    }
}
