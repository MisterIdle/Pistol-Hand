using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraManager : BaseManager
{
    [Header("Camera Settings")]
    public static CameraManager Instance { get; private set; }
    public float Slowfactor = 0.05f;
    public float SlowDuration = 0.02f;
    public float ShakeTime;
    private bool _isCameraUp;
    private CinemachineCamera _cinemachineCam;

    private void Awake()
    {
        InitializeSingleton();
        _cinemachineCam = GetComponent<CinemachineCamera>();
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

    public void Update()
    {
        if (ShakeTime > 0)
        {
            ShakeTime -= Time.deltaTime;
            if (ShakeTime <= 0)
            {
                var noise = _cinemachineCam.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
                noise.AmplitudeGain = 0f;
            }
        }
    }

    public void ShakeCamera(float intensity, float time)
    {
        var noise = _cinemachineCam.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        noise.AmplitudeGain = intensity;
        ShakeTime = time;
    }

    public IEnumerator SlowMotion()
    {
        Time.timeScale = Slowfactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        yield return new WaitForSeconds(SlowDuration);
        Time.timeScale = 1f;
    }

    public IEnumerator MoveCameraTransition(bool moveUp, float time)
    {
        if ((_isCameraUp && moveUp) || (!_isCameraUp && !moveUp))
        {
            yield break;
        }

        _isCameraUp = moveUp;
        int direction = moveUp ? 1 : -1;
        Vector3 targetPosition = _cinemachineCam.transform.position + new Vector3(0, direction * 50, 0);

        float elapsedTime = 0f;
        Vector3 originalPos = _cinemachineCam.transform.position;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / time);
            _cinemachineCam.transform.position = Vector3.Lerp(originalPos, targetPosition, t);
            yield return null;
        }
    }

    public IEnumerator ChangeCameraLens(float newLens, float duration)
    {
        float originalLens = _cinemachineCam.Lens.OrthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            _cinemachineCam.Lens.OrthographicSize = Mathf.Lerp(originalLens, newLens, t);
            yield return null;
        }

        _cinemachineCam.Lens.OrthographicSize = newLens;
    }

    public IEnumerator SetCameraPosition(Vector3 newPos, float duration)
    {
        Vector3 originalPos = _cinemachineCam.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            _cinemachineCam.transform.position = Vector3.Lerp(originalPos, newPos, t);
            yield return null;
        }

        _cinemachineCam.transform.position = newPos;
    }
}
