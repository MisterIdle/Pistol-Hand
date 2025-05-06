using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Values
{
    NeedToWin,
    PlayerHealth
}

[System.Serializable]
public class IntReference
{
    public TMP_Text textField;
    public Button addValueButton;
    public Button removeValueButton;
    public Values valueType;

    private int CurrentValue
    {
        get => GameParameter.Instance.GetValue(valueType);
        set
        {
            GameParameter.Instance.SetValue(valueType, value);
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

    private void AddValue() => CurrentValue++;
    private void RemoveValue() => CurrentValue = Mathf.Max(0, CurrentValue - 1);
    private void UpdateText() => textField.text = CurrentValue.ToString();
}

