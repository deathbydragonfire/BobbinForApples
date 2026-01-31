using UnityEngine;
using TMPro;
using System.Collections;

public class PowerupNotificationUI : MonoBehaviour
{
    public static PowerupNotificationUI Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private PowerupNotificationData notificationData;
    [SerializeField] private RectTransform notificationPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    
    [Header("Animation Settings")]
    [SerializeField] private float slideDistance = 30f;
    [SerializeField] private float slideInDuration = 0.3f;
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    private Coroutine currentNotificationCoroutine;
    private CanvasGroup canvasGroup;
    private Vector2 targetPosition;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notificationPanel.gameObject.AddComponent<CanvasGroup>();
        }
        
        targetPosition = notificationPanel.anchoredPosition;
        
        canvasGroup.alpha = 0f;
        notificationPanel.gameObject.SetActive(false);
    }
    
    public void ShowNotification(PowerupType powerupType)
    {
        if (notificationData == null)
        {
            Debug.LogWarning("PowerupNotificationData not assigned!");
            return;
        }
        
        PowerupNotificationConfig config = notificationData.GetConfig(powerupType);
        if (config == null)
        {
            Debug.LogWarning($"No notification config found for {powerupType}");
            return;
        }
        
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }
        
        currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine(config));
    }
    
    private IEnumerator ShowNotificationCoroutine(PowerupNotificationConfig config)
    {
        titleText.text = config.title;
        subtitleText.text = config.subtitle;
        
        if (notificationData.titleFont != null)
        {
            titleText.font = notificationData.titleFont;
        }
        titleText.fontSize = notificationData.titleFontSize;
        titleText.color = notificationData.titleColor;
        
        if (notificationData.subtitleFont != null)
        {
            subtitleText.font = notificationData.subtitleFont;
        }
        subtitleText.fontSize = notificationData.subtitleFontSize;
        subtitleText.color = notificationData.subtitleColor;
        
        notificationPanel.gameObject.SetActive(true);
        
        Vector2 startPosition = targetPosition - new Vector2(0f, slideDistance);
        notificationPanel.anchoredPosition = startPosition;
        canvasGroup.alpha = 0f;
        
        float elapsed = 0f;
        
        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideInDuration;
            float curveValue = slideInCurve.Evaluate(t);
            
            notificationPanel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
            canvasGroup.alpha = curveValue;
            
            yield return null;
        }
        
        notificationPanel.anchoredPosition = targetPosition;
        canvasGroup.alpha = 1f;
        
        yield return new WaitForSeconds(displayDuration);
        
        elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            canvasGroup.alpha = 1f - t;
            
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        notificationPanel.gameObject.SetActive(false);
        
        currentNotificationCoroutine = null;
    }
}
