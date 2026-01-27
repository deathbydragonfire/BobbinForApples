using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Image flashImage;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("Auto Setup")]
    [SerializeField] private bool createFlashImageOnAwake = true;
    
    private Canvas flashCanvas;
    private bool isFlashing;
    
    private void Awake()
    {
        if (createFlashImageOnAwake && flashImage == null)
        {
            CreateFlashCanvas();
        }
        
        if (flashImage != null)
        {
            flashImage.enabled = false;
        }
    }
    
    private void CreateFlashCanvas()
    {
        GameObject canvasObject = new GameObject("ScreenFlashCanvas");
        canvasObject.transform.SetParent(transform);
        
        flashCanvas = canvasObject.AddComponent<Canvas>();
        flashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        flashCanvas.sortingOrder = 9999;
        
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObject.AddComponent<GraphicRaycaster>();
        
        GameObject imageObject = new GameObject("FlashImage");
        imageObject.transform.SetParent(canvasObject.transform);
        
        flashImage = imageObject.AddComponent<Image>();
        flashImage.color = flashColor;
        flashImage.enabled = false;
        
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    public void Flash()
    {
        Flash(flashDuration, flashColor);
    }
    
    public void Flash(float duration)
    {
        Flash(duration, flashColor);
    }
    
    public void Flash(float duration, Color color)
    {
        if (flashImage == null)
        {
            Debug.LogWarning("ScreenFlash: Flash image not assigned");
            return;
        }
        
        if (isFlashing)
        {
            StopAllCoroutines();
        }
        
        StartCoroutine(FlashCoroutine(duration, color));
    }
    
    private IEnumerator FlashCoroutine(float duration, Color color)
    {
        isFlashing = true;
        flashImage.enabled = true;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float alpha = flashCurve.Evaluate(t);
            
            Color currentColor = color;
            currentColor.a = alpha;
            flashImage.color = currentColor;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        flashImage.enabled = false;
        isFlashing = false;
    }
    
    private void OnDisable()
    {
        if (isFlashing && flashImage != null)
        {
            flashImage.enabled = false;
            isFlashing = false;
        }
    }
}
