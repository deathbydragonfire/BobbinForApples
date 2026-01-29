using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    public event Action OnFadeInComplete;
    
    [Header("UI References")]
    [SerializeField] private GameObject healthBarContainer;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI healthPercentageText;
    
    [Header("Health Bar Settings")]
    [SerializeField] private string bossName = "The One Who Bites";
    [SerializeField] private Color healthColor = Color.red;
    [SerializeField] private float updateSpeed = 0.3f;
    [SerializeField] private float fadeSpeed = 1f;
    
    private float maxHealth;
    private float currentHealth;
    private float displayedHealth;
    private CanvasGroup canvasGroup;
    private RectTransform healthBarFillRect;
    private float healthBarWidth;
    private Coroutine titleChangeCoroutine;
    private Coroutine healthUpdateCoroutine;
    private Coroutine refillCoroutine;
    
    private void Awake()
    {
        canvasGroup = healthBarContainer?.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null && healthBarContainer != null)
        {
            canvasGroup = healthBarContainer.AddComponent<CanvasGroup>();
        }
        
        if (healthBarFill != null)
        {
            healthBarFill.color = healthColor;
            healthBarFillRect = healthBarFill.GetComponent<RectTransform>();
            
            if (healthBarFillRect != null)
            {
                RectTransform parentRect = healthBarFillRect.parent as RectTransform;
                if (parentRect != null)
                {
                    healthBarWidth = parentRect.rect.width;
                }
                
                healthBarFillRect.anchorMin = new Vector2(0, 0);
                healthBarFillRect.anchorMax = new Vector2(0, 1);
                healthBarFillRect.pivot = new Vector2(0, 0.5f);
                healthBarFillRect.anchoredPosition = Vector2.zero;
                healthBarFillRect.sizeDelta = new Vector2(healthBarWidth, 0);
                
                Debug.Log($"HealthBar initialized: width = {healthBarWidth}");
            }
        }
        
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(true);
        }
    }
    
    public void Initialize(float startingMaxHealth)
    {
        maxHealth = startingMaxHealth;
        currentHealth = startingMaxHealth;
        displayedHealth = startingMaxHealth;
        
        UpdateHealthBar();
        
        Debug.Log($"Boss health bar initialized with {maxHealth} HP (UI will show after title card)");
    }
    
    public void UpdateHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        
        if (healthUpdateCoroutine != null)
        {
            StopCoroutine(healthUpdateCoroutine);
        }
        if (refillCoroutine != null)
        {
            StopCoroutine(refillCoroutine);
            refillCoroutine = null;
        }
        
        healthUpdateCoroutine = StartCoroutine(SmoothUpdateHealth());
    }
    
    public void RefillHealth(float newMaxHealth, float refillDuration = 3f)
    {
        maxHealth = newMaxHealth;
        currentHealth = newMaxHealth;
        
        if (healthUpdateCoroutine != null)
        {
            StopCoroutine(healthUpdateCoroutine);
            healthUpdateCoroutine = null;
        }
        if (refillCoroutine != null)
        {
            StopCoroutine(refillCoroutine);
        }
        
        refillCoroutine = StartCoroutine(RefillHealthBar(refillDuration));
    }
    
    private IEnumerator RefillHealthBar(float duration)
    {
        float startHealth = displayedHealth;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            displayedHealth = Mathf.Lerp(startHealth, maxHealth, t);
            UpdateHealthBar();
            
            yield return null;
        }
        
        displayedHealth = maxHealth;
        UpdateHealthBar();
        refillCoroutine = null;
    }
    
    private IEnumerator SmoothUpdateHealth()
    {
        float startHealth = displayedHealth;
        float elapsed = 0f;
        
        while (elapsed < updateSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / updateSpeed;
            
            displayedHealth = Mathf.Lerp(startHealth, currentHealth, t);
            UpdateHealthBar();
            
            yield return null;
        }
        
        displayedHealth = currentHealth;
        UpdateHealthBar();
        healthUpdateCoroutine = null;
    }
    
    private void UpdateHealthBar()
    {
        float fillAmount = displayedHealth / maxHealth;
        
        if (healthBarFillRect != null)
        {
            float targetWidth = healthBarWidth * fillAmount;
            healthBarFillRect.sizeDelta = new Vector2(targetWidth, 0);
            Debug.Log($"HealthBar Updated: {displayedHealth}/{maxHealth} = {fillAmount:F2} (width = {targetWidth:F1}/{healthBarWidth:F1})");
        }
        
        if (healthPercentageText != null)
        {
            int percentage = Mathf.RoundToInt(fillAmount * 100f);
            healthPercentageText.text = $"{percentage}%";
        }
    }
    
    public void Show()
    {
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(true);
        }
        
        StartCoroutine(FadeIn());
    }
    
    public void Hide()
    {
        StartCoroutine(FadeOut());
    }
    
    public void ChangeTitle(string newTitle, float typingSpeed = 0.05f)
    {
        if (titleChangeCoroutine != null)
        {
            StopCoroutine(titleChangeCoroutine);
        }
        titleChangeCoroutine = StartCoroutine(TypewriterTitleChange(newTitle, typingSpeed));
    }
    
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeSpeed);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        
        OnFadeInComplete?.Invoke();
        Debug.Log("Boss health bar fade-in complete");
    }
    
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeSpeed);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }
    }
    
    private IEnumerator TypewriterTitleChange(string newTitle, float typingSpeed)
    {
        if (bossNameText == null) yield break;
        
        string currentText = bossNameText.text;
        
        while (currentText.Length > 0)
        {
            currentText = currentText.Substring(0, currentText.Length - 1);
            bossNameText.text = currentText;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        yield return new WaitForSeconds(0.2f);
        
        for (int i = 0; i <= newTitle.Length; i++)
        {
            bossNameText.text = newTitle.Substring(0, i);
            yield return new WaitForSeconds(typingSpeed);
        }
        
        bossName = newTitle;
        titleChangeCoroutine = null;
    }
}
