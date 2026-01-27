using UnityEngine;

public class ProjectileSpawnGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private Light glowLight;
    [SerializeField] private Renderer glowRenderer;
    [SerializeField] private Color glowColor = new Color(2f, 0f, 0f, 1f);
    [SerializeField] private float maxIntensity = 5f;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    private Material glowMaterial;
    private float flashTimer;
    private bool isFlashing;
    private Color originalEmission;
    
    private void Awake()
    {
        if (glowLight == null)
        {
            glowLight = GetComponent<Light>();
        }
        
        if (glowRenderer == null)
        {
            glowRenderer = GetComponent<Renderer>();
        }
        
        if (glowRenderer != null)
        {
            glowMaterial = glowRenderer.material;
            originalEmission = glowMaterial.GetColor("_EmissionColor");
        }
        
        if (glowLight != null)
        {
            glowLight.intensity = 0f;
        }
    }
    
    private void Update()
    {
        if (isFlashing)
        {
            flashTimer += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(flashTimer / flashDuration);
            float curveValue = flashCurve.Evaluate(normalizedTime);
            
            UpdateGlowIntensity(curveValue);
            
            if (flashTimer >= flashDuration)
            {
                isFlashing = false;
                UpdateGlowIntensity(0f);
            }
        }
    }
    
    public void TriggerFlash()
    {
        flashTimer = 0f;
        isFlashing = true;
        Debug.Log($"ProjectileSpawnGlow: Flash triggered! Light: {glowLight != null}, Material: {glowMaterial != null}, MaxIntensity: {maxIntensity}, Color: {glowColor}");
    }
    
    private void UpdateGlowIntensity(float intensity)
    {
        if (glowLight != null)
        {
            glowLight.intensity = intensity * maxIntensity;
            glowLight.color = glowColor;
        }
        
        if (glowMaterial != null)
        {
            Color emissionColor = glowColor * intensity;
            glowMaterial.SetColor("_EmissionColor", emissionColor);
        }
    }
    
    public void SetGlowEnabled(bool enabled)
    {
        if (!enabled)
        {
            isFlashing = false;
            UpdateGlowIntensity(0f);
        }
    }
}
