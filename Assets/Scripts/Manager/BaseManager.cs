using UnityEngine;

public abstract class BaseManager : MonoBehaviour
{
    protected LobbyManager LobbyManager => LobbyManager.Instance;
    protected GameManager GameManager => GameManager.Instance;
    protected HUDManager HUDManager => HUDManager.Instance;
    protected CameraManager CameraManager => CameraManager.Instance;
    protected MatchManager MatchManager => MatchManager.Instance;
    protected TrophyManager TrophyManager => TrophyManager.Instance;
}