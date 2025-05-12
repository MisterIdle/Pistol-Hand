using UnityEngine.UI;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ValueModifier
{
    public Button incrementButton;
    public Button decrementButton;
    public TMP_Text valueText;
    public GameParameterType parameterKey;

    private SettingsManager settingsManager;

    public void Initialize()
    {
        settingsManager = SettingsManager.Instance;

        UpdateValueText();

        incrementButton.onClick.AddListener(IncrementValue);
        decrementButton.onClick.AddListener(DecrementValue);
    }

    private void IncrementValue()
    {
        var param = GetParameter();
        if (param != null)
        {
            param.value = Mathf.Clamp(param.value + param.stepValue, param.minValue, param.maxValue);
            settingsManager.SaveGameParameters();
            UpdateValueText();
        }
    }

    private void DecrementValue()
    {
        var param = GetParameter();
        if (param != null)
        {
            param.value = Mathf.Clamp(param.value - param.stepValue, param.minValue, param.maxValue);
            settingsManager.SaveGameParameters();
            UpdateValueText();
        }
    }

    public void UpdateValueText()
    {
        var param = GetParameter();
        if (param != null)
        {
            valueText.text = param.value.ToString();
        }

        SettingsManager.Instance.SaveGameParameters();
    }

    private SerializableParameter GetParameter()
    {
        return settingsManager.GetParameterByKey(parameterKey);
    }
}
