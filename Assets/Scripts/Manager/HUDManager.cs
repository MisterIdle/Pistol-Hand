using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public Image transition;

    public static HUDManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
