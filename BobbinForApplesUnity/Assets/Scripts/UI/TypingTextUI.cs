using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class TypingTextUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI textDisplay;
    
    [Header("Typing Effect Settings")]
    [SerializeField] private float charactersPerSecond = 30f;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Audio Settings")]
    [SerializeField] private SoundData typingSound;
    
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (textDisplay == null)
        {
            textDisplay = GetComponent<TextMeshProUGUI>();
        }
        
        canvasGroup.alpha = 0f;
        
        if (textDisplay != null)
        {
            textDisplay.text = "";
        }
    }
    
    public void DisplayText(string text, Action onComplete = null)
    {
        StartCoroutine(DisplayTextSequence(text, onComplete));
    }
    
    private IEnumerator DisplayTextSequence(string text, Action onComplete)
    {
        if (textDisplay == null) yield break;
        
        textDisplay.text = "";
        
        yield return StartCoroutine(FadeIn());
        
        yield return StartCoroutine(TypeText(text));
        
        yield return new WaitForSeconds(displayDuration);
        
        yield return StartCoroutine(FadeOut());
        
        onComplete?.Invoke();
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
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator TypeText(string text)
    {
        if (textDisplay == null) yield break;
        
        textDisplay.text = "";
        float delay = 1f / charactersPerSecond;
        
        AudioSource typingSoundSource = null;
        if (typingSound != null && AudioManager.Instance != null)
        {
            typingSoundSource = AudioManager.Instance.PlaySoundWithReference(typingSound);
        }
        
        foreach (char character in text)
        {
            textDisplay.text += character;
            yield return new WaitForSeconds(delay);
        }
        
        if (typingSoundSource != null && typingSoundSource.isPlaying)
        {
            typingSoundSource.Stop();
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
        
        if (textDisplay != null)
        {
            textDisplay.text = "";
        }
    }
}
