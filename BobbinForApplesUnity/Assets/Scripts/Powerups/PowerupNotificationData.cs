using UnityEngine;
using TMPro;

[System.Serializable]
public class PowerupNotificationConfig
{
    public PowerupType powerupType;
    public string title;
    public string subtitle;
}

[CreateAssetMenu(fileName = "PowerupNotificationData", menuName = "Powerups/Notification Data")]
public class PowerupNotificationData : ScriptableObject
{
    [Header("Notification Settings")]
    public PowerupNotificationConfig[] notificationConfigs;
    
    [Header("Text Style")]
    public TMP_FontAsset titleFont;
    public float titleFontSize = 36f;
    public Color titleColor = Color.white;
    
    public TMP_FontAsset subtitleFont;
    public float subtitleFontSize = 24f;
    public Color subtitleColor = Color.gray;
    
    public PowerupNotificationConfig GetConfig(PowerupType type)
    {
        foreach (var config in notificationConfigs)
        {
            if (config.powerupType == type)
            {
                return config;
            }
        }
        return null;
    }
}
