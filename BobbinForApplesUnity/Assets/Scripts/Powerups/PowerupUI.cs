using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PowerupUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Image selectionFrame;
    
    [Header("Selection Frame Animation")]
    [SerializeField] private Sprite[] frameSprites;
    [SerializeField] private float frameAnimationSpeed = 10f;
    
    [Header("Layout Settings")]
    [SerializeField] private float iconSpacing = 10f;
    [SerializeField] private float iconSize = 64f;
    [SerializeField] private Vector2 selectionFrameOffset = Vector2.zero;
    
    [Header("Icon Scale Animation")]
    [SerializeField] private float unselectedScale = 0.8f;
    [SerializeField] private float selectedScale = 1f;
    [SerializeField] private float scaleTransitionDuration = 1f;
    
    [Header("Consume Animation")]
    [SerializeField] private float redFlashDuration = 0.2f;
    [SerializeField] private float scaleAnimationDuration = 0.3f;
    
    private List<Image> iconImages = new List<Image>();
    private List<float> iconScales = new List<float>();
    private List<float> iconScaleVelocities = new List<float>();
    private int currentSelectedIndex = -1;
    private int previousSelectedIndex = -1;
    private bool isAnimatingFrame = false;
    
    private void Start()
    {
        if (selectionFrame != null)
        {
            selectionFrame.gameObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (isAnimatingFrame && frameSprites != null && frameSprites.Length > 0 && selectionFrame != null)
        {
            int frameIndex = Mathf.FloorToInt(Time.time * frameAnimationSpeed) % frameSprites.Length;
            selectionFrame.sprite = frameSprites[frameIndex];
        }
        
        UpdateSelectionFramePosition();
        UpdateIconScales();
    }
    
    private void UpdateSelectionFramePosition()
    {
        if (selectionFrame == null || currentSelectedIndex < 0 || currentSelectedIndex >= iconImages.Count)
        {
            return;
        }
        
        RectTransform frameRect = selectionFrame.GetComponent<RectTransform>();
        if (frameRect != null)
        {
            float xPos = currentSelectedIndex * (iconSize + iconSpacing) + (iconSize * 0.5f);
            frameRect.anchoredPosition = new Vector2(xPos + selectionFrameOffset.x, selectionFrameOffset.y);
        }
    }
    
    private void UpdateIconScales()
    {
        for (int i = 0; i < iconImages.Count; i++)
        {
            if (iconImages[i] == null) continue;
            
            float targetScale = (i == currentSelectedIndex) ? selectedScale : unselectedScale;
            float velocity = iconScaleVelocities[i];
            float newScale = Mathf.SmoothDamp(iconScales[i], targetScale, ref velocity, scaleTransitionDuration);
            
            iconScales[i] = newScale;
            iconScaleVelocities[i] = velocity;
            iconImages[i].transform.localScale = Vector3.one * newScale;
        }
    }
    
    public void UpdateDisplay(List<Sprite> powerupSprites, int selectedIndex)
    {
        ClearIcons();
        
        if (powerupSprites == null || powerupSprites.Count == 0)
        {
            if (selectionFrame != null)
            {
                selectionFrame.gameObject.SetActive(false);
            }
            isAnimatingFrame = false;
            return;
        }
        
        for (int i = 0; i < powerupSprites.Count; i++)
        {
            GameObject iconObj = Instantiate(iconPrefab, iconContainer);
            Image iconImage = iconObj.GetComponent<Image>();
            
            if (iconImage != null)
            {
                iconImage.sprite = powerupSprites[i];
                iconImages.Add(iconImage);
                
                float initialScale = (i == selectedIndex) ? selectedScale : unselectedScale;
                iconScales.Add(initialScale);
                iconScaleVelocities.Add(0f);
                iconImage.transform.localScale = Vector3.one * initialScale;
            }
            
            RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
                rectTransform.anchoredPosition = new Vector2(i * (iconSize + iconSpacing), 0f);
            }
        }
        
        previousSelectedIndex = currentSelectedIndex;
        currentSelectedIndex = selectedIndex;
        UpdateSelectionFrame();
        isAnimatingFrame = true;
    }
    
    private void UpdateSelectionFrame()
    {
        if (selectionFrame == null || currentSelectedIndex < 0 || currentSelectedIndex >= iconImages.Count)
        {
            if (selectionFrame != null)
            {
                selectionFrame.gameObject.SetActive(false);
            }
            isAnimatingFrame = false;
            return;
        }
        
        selectionFrame.gameObject.SetActive(true);
        UpdateSelectionFramePosition();
    }
    
    private void ClearIcons()
    {
        foreach (Image icon in iconImages)
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
        }
        
        iconImages.Clear();
        iconScales.Clear();
        iconScaleVelocities.Clear();
    }
    
    public void PlayConsumeAnimation(int index, System.Action onComplete)
    {
        if (index >= 0 && index < iconImages.Count)
        {
            StartCoroutine(ConsumeAnimationCoroutine(iconImages[index], onComplete));
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    private IEnumerator ConsumeAnimationCoroutine(Image icon, System.Action onComplete)
    {
        Color originalColor = icon.color;
        
        float elapsed = 0f;
        while (elapsed < redFlashDuration)
        {
            elapsed += Time.deltaTime;
            icon.color = Color.Lerp(originalColor, Color.red, elapsed / redFlashDuration);
            yield return null;
        }
        
        icon.color = Color.red;
        
        Vector3 originalScale = icon.transform.localScale;
        elapsed = 0f;
        
        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleAnimationDuration;
            icon.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }
        
        onComplete?.Invoke();
    }
}
