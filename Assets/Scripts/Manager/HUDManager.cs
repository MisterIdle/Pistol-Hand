using UnityEngine.UI;
using UnityEngine;

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

    public void FadeIn(float time)
    {
        transition.CrossFadeAlpha(1, time, false);
    }

    public void FadeOut(float time)
    {
        transition.CrossFadeAlpha(0, time, false);
    }

    public void OnEditorButtonClick()
    {
        StartCoroutine(SceneLoader.LoadScene(GameManager.EditorSceneName));
        Debug.Log("Change to Editor Scene");
    }
}
