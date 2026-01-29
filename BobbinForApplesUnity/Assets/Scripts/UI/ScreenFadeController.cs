using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFadeController : MonoBehaviour
{
    private static ScreenFadeController instance;
    public static ScreenFadeController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ScreenFadeController>();
            }
            return instance;
        }
    }
    
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float defaultFadeDuration = 1f;
    
    private CanvasGroup canvasGroup;
    private bool isFading;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        
        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
        }
    }
    
    public void FadeToBlack(float duration = -1f, Action onComplete = null)
    {
        if (isFading)
        {
            StopAllCoroutines();
        }
        
        float fadeDuration = duration > 0f ? duration : defaultFadeDuration;
        StartCoroutine(FadeCoroutine(0f, 1f, fadeDuration, onComplete));
    }
    
    public void FadeFromBlack(float duration = -1f, Action onComplete = null)
    {
        if (isFading)
        {
            StopAllCoroutines();
        }
        
        float fadeDuration = duration > 0f ? duration : defaultFadeDuration;
        StartCoroutine(FadeCoroutine(1f, 0f, fadeDuration, onComplete));
    }
    
    public void SetBlack()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }
    
    public void SetClear()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
    
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration, Action onComplete)
    {
        isFading = true;
        
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                
                yield return null;
            }
            
            canvasGroup.alpha = endAlpha;
            
            if (endAlpha == 0f)
            {
                canvasGroup.blocksRaycasts = false;
            }
        }
        
        isFading = false;
        onComplete?.Invoke();
    }
}
