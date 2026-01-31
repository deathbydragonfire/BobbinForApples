using UnityEngine;
using TMPro;

public class PowerupFloatingText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform selectionFrame;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Bob Animation")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobSeverity = 10f;
    
    [Header("Fade Animation")]
    [SerializeField] private float fadeSpeed = 1.5f;
    
    [Header("Positioning")]
    [SerializeField] private float yOffset = 50f;
    
    private RectTransform rectTransform;
    private float bobTimer = 0f;
    private float fadeTimer = 0f;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
    
    private void Update()
    {
        if (selectionFrame == null || !selectionFrame.gameObject.activeInHierarchy)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            return;
        }
        
        UpdatePosition();
        UpdateBob();
        UpdateFade();
    }
    
    private void UpdatePosition()
    {
        if (rectTransform != null && selectionFrame != null)
        {
            Vector2 basePosition = selectionFrame.anchoredPosition;
            basePosition.y += yOffset;
            
            float bobOffset = Mathf.Sin(bobTimer) * bobSeverity;
            rectTransform.anchoredPosition = new Vector2(basePosition.x, basePosition.y + bobOffset);
        }
    }
    
    private void UpdateBob()
    {
        bobTimer += Time.deltaTime * bobSpeed;
        
        if (bobTimer > Mathf.PI * 2f)
        {
            bobTimer -= Mathf.PI * 2f;
        }
    }
    
    private void UpdateFade()
    {
        if (canvasGroup == null)
        {
            return;
        }
        
        fadeTimer += Time.deltaTime * fadeSpeed;
        
        float alpha = (Mathf.Sin(fadeTimer) + 1f) * 0.5f;
        canvasGroup.alpha = alpha;
        
        if (fadeTimer > Mathf.PI * 2f)
        {
            fadeTimer -= Mathf.PI * 2f;
        }
    }
}
