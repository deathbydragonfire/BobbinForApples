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
    
    public void RestoreFullHealth()
    {
        Debug.Log($"PlayerHealthUI: Restoring full health from {currentHealth} to {maxHealth}");
        currentHealth = maxHealth;
        
        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] != null)
            {
                healthIcons[i].color = fullHealthColor;
                healthIcons[i].transform.localScale = Vector3.one;
            }
        }
        
        Debug.Log($"PlayerHealthUI: Health restored to {currentHealth}/{maxHealth}");
    }
    
    public void RestoreHealth(int amount)
    {
        Debug.Log($"PlayerHealthUI: Restoring {amount} health from {currentHealth}/{maxHealth}");
        
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        int healthRestored = currentHealth - previousHealth;
        
        for (int i = 0; i < healthRestored; i++)
        {
            int iconIndex = previousHealth + i;
            if (iconIndex >= 0 && iconIndex < healthIcons.Length && healthIcons[iconIndex] != null)
            {
                StartCoroutine(RestoreHealthAnimation(healthIcons[iconIndex], i * 0.2f));
            }
        }
        
        Debug.Log($"PlayerHealthUI: Health restored to {currentHealth}/{maxHealth}");
    }
    
    private IEnumerator RestoreHealthAnimation(Image targetIcon, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log("Apple UI: Starting health restore animation - gold glow, scale, shake, flash white");
        
        Vector3 originalScale = targetIcon.transform.localScale;
        Vector3 originalPosition = targetIcon.transform.localPosition;
        Color originalColor = fullHealthColor;
        Color goldColor = new Color(1f, 0.84f, 0f, 1f);
        Color whiteColor = Color.white;
        
        float fadeToGoldDuration = 0.3f;
        float scaleDuration = 0.3f;
        float shakeAndFlashDuration = 0.4f;
        float scaleBackDuration = 0.3f;
        
        targetIcon.color = originalColor;
        
        float elapsed = 0f;
        while (elapsed < fadeToGoldDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeToGoldDuration;
            targetIcon.color = Color.Lerp(originalColor, goldColor, t);
            yield return null;
        }
        targetIcon.color = goldColor;
        
        float scaleMultiplier = 1.5f;
        elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDuration;
            float scale = Mathf.Lerp(1f, scaleMultiplier, t);
            targetIcon.transform.localScale = originalScale * scale;
            yield return null;
        }
        targetIcon.transform.localScale = originalScale * scaleMultiplier;
        
        elapsed = 0f;
        float shakeFrequency = 30f;
        float flashFrequency = 10f;
        while (elapsed < shakeAndFlashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shakeAndFlashDuration;
            
            float offsetX = Mathf.Sin(elapsed * shakeFrequency) * shakeIntensity * (1f - t);
            float offsetY = Mathf.Cos(elapsed * shakeFrequency * 1.3f) * shakeIntensity * (1f - t);
            targetIcon.transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            
            float flashT = Mathf.PingPong(elapsed * flashFrequency, 1f);
            targetIcon.color = Color.Lerp(goldColor, whiteColor, flashT);
            
            yield return null;
        }
        
        targetIcon.transform.localPosition = originalPosition;
        
        elapsed = 0f;
        while (elapsed < scaleBackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleBackDuration;
            float scale = Mathf.Lerp(scaleMultiplier, 1f, t);
            targetIcon.transform.localScale = originalScale * scale;
            targetIcon.color = Color.Lerp(whiteColor, originalColor, t);
            yield return null;
        }
        
        targetIcon.transform.localScale = originalScale;
        targetIcon.transform.localPosition = originalPosition;
        targetIcon.color = originalColor;
        
        Debug.Log("Apple UI: Health restore animation complete");
    }
}
