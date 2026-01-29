using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossTitleCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private Image backgroundVignette;
    
    [Header("Title Settings")]
    [SerializeField] private string bossTitle = "THE BOBBER APPROACHES";
    [SerializeField] private string bossSubtitle = "\"Prepare to be Bobbed\"";
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float scaleInDuration = 0.7f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private float bounceStrength = 1.2f;
    
    private CanvasGroup canvasGroup;
    private BossUIManager uiManager;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (titleText != null)
        {
            titleText.text = bossTitle;
        }
        
        if (subtitleText != null)
        {
            subtitleText.text = bossSubtitle;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        if (titleText != null)
        {
            titleText.transform.localScale = Vector3.zero;
        }
        
        if (subtitleText != null)
        {
            subtitleText.transform.localScale = Vector3.zero;
        }
        
        uiManager = FindFirstObjectByType<BossUIManager>();
    }
    
    public void PlayIntroSequence()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("TitleCard", 2f);
            Debug.Log("Boss music (TitleCard) triggered with title card!");
        }
        
        StartCoroutine(IntroSequence());
    }
    
    private IEnumerator IntroSequence()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        yield return StartCoroutine(FadeIn());
        
        yield return new WaitForSeconds(displayDuration);
        
        yield return StartCoroutine(FadeOut());
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        OnSequenceComplete();
    }
    
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        float maxDuration = Mathf.Max(fadeInDuration, scaleInDuration);
        float elapsed = 0f;
        
        while (elapsed < maxDuration)
        {
            elapsed += Time.deltaTime;
            
            float fadeT = Mathf.Clamp01(elapsed / fadeInDuration);
            float scaletT = Mathf.Clamp01(elapsed / scaleInDuration);
            
            canvasGroup.alpha = fadeT;
            
            float bounceScale = EaseOutBounce(scaletT);
            
            if (titleText != null)
            {
                titleText.transform.localScale = Vector3.one * bounceScale;
            }
            
            if (subtitleText != null)
            {
                subtitleText.transform.localScale = Vector3.one * bounceScale;
            }
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        
        if (titleText != null)
        {
            titleText.transform.localScale = Vector3.one;
        }
        
        if (subtitleText != null)
        {
            subtitleText.transform.localScale = Vector3.one;
        }
    }
    
    private float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        
        float baseValue;
        
        if (t < 1f / d1)
        {
            baseValue = n1 * t * t;
        }
        else if (t < 2f / d1)
        {
            t -= 1.5f / d1;
            baseValue = n1 * t * t + 0.75f;
        }
        else if (t < 2.5f / d1)
        {
            t -= 2.25f / d1;
            baseValue = n1 * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / d1;
            baseValue = n1 * t * t + 0.984375f;
        }
        
        float overshoot = (baseValue - 1f) * (bounceStrength - 1f);
        return baseValue + overshoot;
    }
    
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
    
    private void OnSequenceComplete()
    {
        Debug.Log("Boss title card sequence completed");
        
        ArenaOutlineDrawAnimation arenaDrawAnimation = FindFirstObjectByType<ArenaOutlineDrawAnimation>();
        if (arenaDrawAnimation != null)
        {
            Debug.Log("Starting arena outline draw animation after title card");
            arenaDrawAnimation.StartDrawAnimation();
        }
        
        PlayerHealthUI playerHealthUI = FindFirstObjectByType<PlayerHealthUI>();
        if (playerHealthUI != null)
        {
            playerHealthUI.RestoreFullHealth();
            Debug.Log("Player health restored to full before boss fight");
        }
        
        if (uiManager != null)
        {
            uiManager.ShowHealthUI();
        }
        
        BossController boss = FindFirstObjectByType<BossController>();
        if (boss != null)
        {
            boss.StartBossFight();
        }
    }
}
