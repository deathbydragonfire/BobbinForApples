using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteMotionBlurController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Material motionBlurMaterial;
    private Material originalMaterial;
    private Vector2 previousPosition;
    private Vector2 velocity;
    
    [Header("Motion Blur Settings")]
    [SerializeField] private Material motionBlurMaterialPrefab;
    [SerializeField] private float velocityScale = 0.5f;
    [SerializeField] private float blurAmount = 0.5f;
    [SerializeField] private int blurSamples = 8;
    
    [Header("Activation")]
    [SerializeField] private bool alwaysActive = true;
    [SerializeField] private float minSpeedThreshold = 0.1f;
    [SerializeField] private float velocitySmoothing = 0.1f;
    
    private static readonly int VelocityProperty = Shader.PropertyToID("_Velocity");
    private static readonly int BlurAmountProperty = Shader.PropertyToID("_BlurAmount");
    private static readonly int BlurSamplesProperty = Shader.PropertyToID("_BlurSamples");
    private static readonly int VelocityScaleProperty = Shader.PropertyToID("_VelocityScale");
    private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    
    private bool isInitialized = false;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.sharedMaterial;
        previousPosition = transform.position;
    }
    
    private void OnEnable()
    {
        if (!isInitialized)
        {
            InitializeMaterial();
        }
        else if (motionBlurMaterial != null)
        {
            spriteRenderer.material = motionBlurMaterial;
        }
    }
    
    private void OnDisable()
    {
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }
    
    private void InitializeMaterial()
    {
        if (motionBlurMaterialPrefab != null)
        {
            motionBlurMaterial = new Material(motionBlurMaterialPrefab);
        }
        else
        {
            Shader motionBlurShader = Shader.Find("Custom/SpriteMotionBlur");
            if (motionBlurShader != null)
            {
                motionBlurMaterial = new Material(motionBlurShader);
                
                if (originalMaterial != null)
                {
                    if (originalMaterial.HasProperty(MainTexProperty))
                    {
                        motionBlurMaterial.SetTexture(MainTexProperty, originalMaterial.GetTexture(MainTexProperty));
                    }
                    if (originalMaterial.HasProperty(ColorProperty))
                    {
                        motionBlurMaterial.SetColor(ColorProperty, originalMaterial.GetColor(ColorProperty));
                    }
                }
            }
            else
            {
                Debug.LogError("SpriteMotionBlur shader not found. Make sure it's compiled correctly.");
                enabled = false;
                return;
            }
        }
        
        spriteRenderer.material = motionBlurMaterial;
        
        motionBlurMaterial.SetFloat(BlurAmountProperty, blurAmount);
        motionBlurMaterial.SetInt(BlurSamplesProperty, blurSamples);
        motionBlurMaterial.SetFloat(VelocityScaleProperty, velocityScale);
        
        isInitialized = true;
    }
    
    private void LateUpdate()
    {
        if (motionBlurMaterial == null) return;
        
        Vector2 currentPosition = transform.position;
        Vector2 frameVelocity = (currentPosition - previousPosition) / Time.deltaTime;
        
        velocity = Vector2.Lerp(velocity, frameVelocity, velocitySmoothing);
        
        if (alwaysActive || velocity.magnitude > minSpeedThreshold)
        {
            motionBlurMaterial.SetVector(VelocityProperty, velocity);
        }
        else
        {
            motionBlurMaterial.SetVector(VelocityProperty, Vector2.zero);
        }
        
        previousPosition = currentPosition;
    }
    
    public void SetBlurIntensity(float intensity)
    {
        blurAmount = Mathf.Clamp01(intensity);
        if (motionBlurMaterial != null)
        {
            motionBlurMaterial.SetFloat(BlurAmountProperty, blurAmount);
        }
    }
    
    public void SetBlurSamples(int samples)
    {
        blurSamples = Mathf.Clamp(samples, 1, 16);
        if (motionBlurMaterial != null)
        {
            motionBlurMaterial.SetInt(BlurSamplesProperty, blurSamples);
        }
    }
    
    public void SetVelocityScale(float scale)
    {
        velocityScale = scale;
        if (motionBlurMaterial != null)
        {
            motionBlurMaterial.SetFloat(VelocityScaleProperty, velocityScale);
        }
    }
    
    public void SetAlwaysActive(bool active)
    {
        alwaysActive = active;
    }
    
    public void SetMinSpeedThreshold(float threshold)
    {
        minSpeedThreshold = threshold;
    }
    
    private void OnDestroy()
    {
        if (motionBlurMaterial != null)
        {
            Destroy(motionBlurMaterial);
        }
    }
}
