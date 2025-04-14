using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public float slowfactor = 0.05f;
    public float slowDuration = 0.02f;
    public float shakeTime;
    private bool _isCameraUp;
    private CinemachineCamera _cinemachineCam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _cinemachineCam = GetComponent<CinemachineCamera>();
    }

    public void Update()
    {
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
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
        shakeTime = time;
    }

    public IEnumerator SlowMotion()
    {
        Time.timeScale = slowfactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        yield return new WaitForSeconds(slowDuration);
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
}
