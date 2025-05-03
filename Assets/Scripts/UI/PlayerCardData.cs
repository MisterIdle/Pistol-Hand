using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class PlayerCardData
{
    public GameObject PlayerCard;
    public Image PlayerImage;
    public TMP_Text HealthPourcent;
    public float LastHealthPercentage = -1f;
}
