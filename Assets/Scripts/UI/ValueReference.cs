using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Values
{
    NeedToWin,
    PlayerHealth
}

public enum ValueType
{
    Int,
    Float
}


[System.Serializable]
public class ValueReference
{
    public TMP_Text textField;
    public Button addValueButton;
    public Button removeValueButton;
    public Values valueKey;
    public ValueType valueType;
    public float floatStep = 0.5f;

    private float CurrentValue
    {
        get => GameParameter.Instance.GetFloat(valueKey);
        set
        {
            GameParameter.Instance.SetFloat(valueKey, value);
            GameParameter.Instance.ApplySettings();
            UpdateText();
        }
    }

    public void Initialize()
    {
        addValueButton.onClick.AddListener(AddValue);
        removeValueButton.onClick.AddListener(RemoveValue);
        UpdateText();
    }

    private void AddValue()
    {
        if (valueType == ValueType.Int)
            CurrentValue = Mathf.FloorToInt(CurrentValue) + 1;
        else
            CurrentValue += floatStep;
    }

    private void RemoveValue()
    {
        if (valueType == ValueType.Int)
            CurrentValue = Mathf.Max(0, Mathf.FloorToInt(CurrentValue) - 1);
        else
            CurrentValue = Mathf.Max(0, CurrentValue - floatStep);
    }

    private void UpdateText()
    {
        if (valueType == ValueType.Int)
            textField.text = Mathf.FloorToInt(CurrentValue).ToString();
        else
            textField.text = CurrentValue.ToString("0.##");
    }
}


