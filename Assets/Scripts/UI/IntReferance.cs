using UnityEngine;
using System.Reflection;
using TMPro;
using System;

[System.Serializable]
public class IntReference
{
    public TextMeshProUGUI textObject;
    public GameObject targetObject;
    public string componentName;
    public string fieldName;
    public int minValue;
    public int maxValue;

    public int GetValue()
    {
        if (targetObject == null || string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(componentName))
            return 0;

        var component = targetObject.GetComponent(componentName);
        if (component == null)
        {
            Debug.LogWarning($"Component '{componentName}' not found on '{targetObject.name}'");
            return 0;
        }

        var type = component.GetType();
        var field = type.GetField(fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field != null && field.FieldType == typeof(int))
        {
            return (int)field.GetValue(component);
        }

        Debug.LogWarning($"Field '{fieldName}' not found or not int in '{componentName}'");
        return 0;
    }

    public void SetValue(int value)
    {
        if (targetObject == null || string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(componentName))
            return;

        var component = targetObject.GetComponent(componentName);
        if (component == null)
        {
            Debug.LogWarning($"Component '{componentName}' not found on '{targetObject.name}'");
            return;
        }

        var type = component.GetType();
        var field = type.GetField(fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field != null && field.FieldType == typeof(int))
        {
            field.SetValue(component, value);
            textObject.text = value.ToString();
            
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found or not int in '{componentName}'");
        }
    }

    public void Increment() => SetValue(Math.Clamp(GetValue() + 1, minValue, maxValue));
    public void Decrement() => SetValue(Math.Clamp(GetValue() - 1, minValue, maxValue));
}
