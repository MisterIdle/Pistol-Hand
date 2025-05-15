using UnityEngine;

public class InputHUD : MonoBehaviour
{
    public static InputHUD Instance { get; private set; }

    private Canvas _canvas;
    public Camera _camera;

    private void Start()
    {
        _camera = MainCamera.Instance.GetComponent<Camera>();

        _canvas = GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = _camera;
    }

    private void Awake()
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
}
