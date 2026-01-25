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
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.8f;
    
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
        
        uiManager = FindFirstObjectByType<BossUIManager>();
    }
    
    public void PlayIntroSequence()
    {
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
        
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            
            if (titleText != null)
            {
                titleText.transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);
            }
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        
        if (titleText != null)
        {
            titleText.transform.localScale = Vector3.one;
        }
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
