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
        get
        {
            return valueType switch
            {
                Values.NeedToWin => GameParameter.Instance.GetNeedToWin(),
                Values.PlayerHealth => GameParameter.Instance.GetPlayerHealth(),
                _ => 0
            };
        }
        set
        {
            switch (valueType)
            {
                case Values.NeedToWin:
                    GameParameter.Instance.SetNeedToWin(value);
                    break;
                case Values.PlayerHealth:
                    GameParameter.Instance.SetPlayerHeal(value);
                    break;
            }
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

