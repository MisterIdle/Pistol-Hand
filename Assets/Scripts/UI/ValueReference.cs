using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Values
{
    NeedToWin,
    Health,
    MaxSpeed,
    JumpForce,
    HitForce,
    CrossbowForce,
    ReloadBullet,
    BulletSpeed,
    DashSpeed,
    DashCooldown,
    DashDuration,
    StunDuration
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
    public float min;
    public float max;

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
            CurrentValue = Mathf.Clamp(Mathf.FloorToInt(CurrentValue) + 1, min, max);
        else
            CurrentValue = Mathf.Clamp(CurrentValue + floatStep, min, max);
    }

    private void RemoveValue()
    {
        if (valueType == ValueType.Int)
            CurrentValue = Mathf.Clamp(Mathf.FloorToInt(CurrentValue) - 1, min, max);
        else
            CurrentValue = Mathf.Clamp(CurrentValue - floatStep, min, max);
    }

    private void UpdateText()
    {
        if (valueType == ValueType.Int)
            textField.text = Mathf.FloorToInt(CurrentValue).ToString();
        else
            textField.text = CurrentValue.ToString("0.##");
    }
}


