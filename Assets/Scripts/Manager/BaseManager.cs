using UnityEngine;
using System;

public abstract class BaseManager : MonoBehaviour
{
    protected GameManager GameManager => GameManager.Instance;
    protected HUDManager HUDManager => HUDManager.Instance;
    protected CameraManager CameraManager => CameraManager.Instance;
    protected LobbyManager LobbyManager => LobbyManager.Instance;
    protected MatchManager MatchManager => MatchManager.Instance;
    protected TrophyManager TrophyManager => TrophyManager.Instance;
    
    protected void InitializeSingleton<T>(T instance, Action<T> setInstance) where T : MonoBehaviour
    {
        if (setInstance == null)
        {
            setInstance(instance);
            DontDestroyOnLoad(instance.gameObject);
        }
        else if (instance != setInstance.Target)
        {
            Destroy(instance.gameObject);
        }
    }
}