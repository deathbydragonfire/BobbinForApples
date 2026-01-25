using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Icons")]
    [SerializeField] private Image[] healthIcons;
    
    [Header("Visual Feedback")]
    [SerializeField] private float hitFlashDuration = 0.2f;
    [SerializeField] private float hitShakeDuration = 0.3f;
    [SerializeField] private float hitScalePulse = 1.3f;
    [SerializeField] private float shakeIntensity = 10f;
    
    [Header("Damage Colors")]
    [SerializeField] private Color fullHealthColor = Color.white;
    [SerializeField] private Color darkColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    private int currentHealth;
    private int maxHealth;
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
    }
    
    public void Show()
    {
        canvasGroup.alpha = 1f;
        Debug.Log("PlayerHealthUI: Showing apple health UI");
    }
    
    public void Hide()
    {
        canvasGroup.alpha = 0f;
        Debug.Log("PlayerHealthUI: Hiding apple health UI");
    }
    
    public void Initialize(int maxHealthValue)
    {
        Debug.Log($"PlayerHealthUI: Initialize called with {maxHealthValue} max health");
        maxHealth = maxHealthValue;
        currentHealth = maxHealth;
        
        if (healthIcons.Length != maxHealth)
        {
            Debug.LogWarning($"Health icons count ({healthIcons.Length}) doesn't match max health ({maxHealth})");
        }
        
        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] != null)
            {
                healthIcons[i].color = fullHealthColor;
            }
        }
        
        Debug.Log($"PlayerHealthUI: Initialized with {currentHealth}/{maxHealth} health (UI will show after title card)");
    }
    
    public void TakeDamage()
    {
        if (currentHealth <= 0)
        {
            return;
        }
        
        currentHealth--;
        Debug.Log($"Apple UI: Lost 1 apple! Health now {currentHealth}/{maxHealth}");
        
        int iconIndex = currentHealth;
        if (iconIndex >= 0 && iconIndex < healthIcons.Length && healthIcons[iconIndex] != null)
        {
            StartCoroutine(HitFeedback(healthIcons[iconIndex]));
        }
    }
    
    private IEnumerator HitFeedback(Image targetIcon)
    {
        Debug.Log("Apple UI: Starting hit feedback - pulse, shake, and darken");
        Vector3 originalScale = targetIcon.transform.localScale;
        Vector3 originalPosition = targetIcon.transform.localPosition;
        
        float elapsed = 0f;
        
        while (elapsed < hitFlashDuration)
        {
            float t = elapsed / hitFlashDuration;
            float scaleFactor = Mathf.Lerp(hitScalePulse, 1f, t);
            targetIcon.transform.localScale = originalScale * scaleFactor;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        targetIcon.transform.localScale = originalScale;
        
        elapsed = 0f;
        while (elapsed < hitShakeDuration)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            
            targetIcon.transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        targetIcon.transform.localPosition = originalPosition;
        
        targetIcon.color = darkColor;
        Debug.Log("Apple UI: Hit feedback complete - apple darkened");
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
