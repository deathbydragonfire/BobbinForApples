using System.Collections;
using UnityEngine;

public class AttackTelegraph : MonoBehaviour
{
    [Header("Telegraph Settings")]
    [SerializeField] private SpriteRenderer telegraphRenderer;
    [SerializeField] private float pulseDuration = 1f;
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1f);
    [SerializeField] private Color startColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color endColor = new Color(1f, 0f, 0f, 0.8f);
    
    [Header("Scale Animation")]
    [SerializeField] private bool scaleAnimation = true;
    [SerializeField] private Vector3 startScale = Vector3.one * 0.8f;
    [SerializeField] private Vector3 endScale = Vector3.one * 1.2f;
    
    [Header("Rotation")]
    [SerializeField] private bool rotateOverTime = false;
    [SerializeField] private float rotationSpeed = 30f;
    
    private bool isActive;
    
    private void Awake()
    {
        if (telegraphRenderer == null)
        {
            telegraphRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (telegraphRenderer != null)
        {
            telegraphRenderer.enabled = false;
        }
    }
    
    public void ShowTelegraph(float duration)
    {
        if (telegraphRenderer == null)
        {
            return;
        }
        
        StopAllCoroutines();
        StartCoroutine(TelegraphSequence(duration));
    }
    
    private IEnumerator TelegraphSequence(float duration)
    {
        isActive = true;
        telegraphRenderer.enabled = true;
        
        if (scaleAnimation)
        {
            transform.localScale = startScale;
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float pulseValue = pulseCurve.Evaluate(t);
            
            telegraphRenderer.color = Color.Lerp(startColor, endColor, pulseValue);
            
            if (scaleAnimation)
            {
                transform.localScale = Vector3.Lerp(startScale, endScale, pulseValue);
            }
            
            if (rotateOverTime)
            {
                transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Hide();
    }
    
    public void Hide()
    {
        isActive = false;
        
        if (telegraphRenderer != null)
        {
            telegraphRenderer.enabled = false;
        }
    }
    
    private void OnDisable()
    {
        Hide();
    }
}
