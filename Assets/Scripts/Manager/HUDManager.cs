using UnityEngine.UI;

public class HUDManager : BaseManager
{
    public Image transition;

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
}
