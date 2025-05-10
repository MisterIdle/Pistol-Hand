using UnityEngine;

public abstract class BaseManager : MonoBehaviour
{
    protected LobbyManager LobbyManager => LobbyManager.Instance;
    protected GameManager GameManager => GameManager.Instance;
    protected HUDManager HUDManager => HUDManager.Instance;
    protected HUDEditorManager HUDEditorManager => HUDEditorManager.Instance;
    protected CameraManager CameraManager => CameraManager.Instance;
    protected MatchManager MatchManager => MatchManager.Instance;
    protected TrophyManager TrophyManager => TrophyManager.Instance;
    protected MapEditor MapEditor => MapEditor.Instance;
    protected MapTester MapTester => MapTester.Instance;
    protected SkinManager SkinManager => SkinManager.Instance;
    protected SettingsManager SettingsManager => SettingsManager.Instance;
    protected MapManager MapManager => MapManager.Instance;
    protected AudioManager AudioManager => AudioManager.Instance;
}
