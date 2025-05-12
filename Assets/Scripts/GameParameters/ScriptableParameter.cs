using UnityEngine;

[CreateAssetMenu(fileName = "New Game Parameter", menuName = "Game Settings/Game Parameter")]
public class ScriptableParameter : ScriptableObject
{
    public GameParameterType key;
    public float value;
    public float minValue;
    public float maxValue;
    public float stepValue;
}
