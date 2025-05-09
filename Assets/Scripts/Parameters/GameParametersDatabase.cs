using UnityEngine;

[CreateAssetMenu(fileName = "Game Parameters Database", menuName = "Game Settings/Game Parameters Database")]
public class GameParametersDatabase : ScriptableObject
{
    public ScriptableParameter[] parameters;
}
