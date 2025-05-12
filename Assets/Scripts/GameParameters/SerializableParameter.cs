[System.Serializable]
public class SerializableParameter
{
    public GameParameterType key;
    public float value;
    public float minValue;
    public float maxValue;
    public float stepValue;

    public SerializableParameter(ScriptableParameter param)
    {
        key = param.key;
        value = param.value;
        minValue = param.minValue;
        maxValue = param.maxValue;
        stepValue = param.stepValue;
    }
}
