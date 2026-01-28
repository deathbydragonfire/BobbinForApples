using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MotionBlurController : MonoBehaviour
{
    public enum MotionBlurType
    {
        None,
        CustomShader,
        TrailRenderer,
        GhostSprites
    }
    
    [Header("Motion Blur Type")]
    [SerializeField] private MotionBlurType motionBlurType = MotionBlurType.CustomShader;
    
    [Header("Activation Settings")]
    [SerializeField] private bool alwaysActive = true;
    [SerializeField] private float minSpeedThreshold = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private MotionBlurType currentType;
    
    private SpriteMotionBlurController shaderController;
    private TrailMotionBlur trailController;
    private GhostSpriteEffect ghostController;
    
    private Material originalMaterial;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.sharedMaterial;
        
        InitializeControllers();
    }
    
    private void Start()
    {
        SetMotionBlurType(motionBlurType);
    }
    
    private void InitializeControllers()
    {
        shaderController = GetComponent<SpriteMotionBlurController>();
        if (shaderController == null)
        {
            shaderController = gameObject.AddComponent<SpriteMotionBlurController>();
        }
        shaderController.enabled = false;
        
        trailController = GetComponent<TrailMotionBlur>();
        ghostController = GetComponent<GhostSpriteEffect>();
    }
    
    public void SetMotionBlurType(MotionBlurType newType)
    {
        if (currentType == newType) return;
        
        DisableCurrentType();
        
        currentType = newType;
        motionBlurType = newType;
        
        EnableNewType();
    }
    
    private void DisableCurrentType()
    {
        switch (currentType)
        {
            case MotionBlurType.CustomShader:
                if (shaderController != null)
                {
                    shaderController.enabled = false;
                    spriteRenderer.material = originalMaterial;
                }
                break;
                
            case MotionBlurType.TrailRenderer:
                if (trailController != null)
                {
                    trailController.enabled = false;
                    TrailRenderer trail = GetComponent<TrailRenderer>();
                    if (trail != null)
                    {
                        trail.enabled = false;
                    }
                }
                break;
                
            case MotionBlurType.GhostSprites:
                if (ghostController != null)
                {
                    ghostController.enabled = false;
                    ghostController.ClearAllGhosts();
                }
                break;
        }
    }
    
    private void EnableNewType()
    {
        switch (currentType)
        {
            case MotionBlurType.None:
                spriteRenderer.material = originalMaterial;
                break;
                
            case MotionBlurType.CustomShader:
                if (shaderController != null)
                {
                    shaderController.enabled = true;
                }
                break;
                
            case MotionBlurType.TrailRenderer:
                if (trailController == null)
                {
                    Debug.LogWarning($"TrailMotionBlur component not found on {gameObject.name}. Please add it manually.");
                }
                else
                {
                    trailController.enabled = true;
                    TrailRenderer trail = GetComponent<TrailRenderer>();
                    if (trail != null)
                    {
                        trail.enabled = true;
                    }
                }
                break;
                
            case MotionBlurType.GhostSprites:
                if (ghostController == null)
                {
                    Debug.LogWarning($"GhostSpriteEffect component not found on {gameObject.name}. Please add it manually.");
                }
                else
                {
                    ghostController.enabled = true;
                }
                break;
        }
    }
    
    public void SetAlwaysActive(bool active)
    {
        alwaysActive = active;
        
        if (shaderController != null)
        {
            shaderController.SetAlwaysActive(active);
        }
    }
    
    public void SetMinSpeedThreshold(float threshold)
    {
        minSpeedThreshold = threshold;
        
        if (shaderController != null)
        {
            shaderController.SetMinSpeedThreshold(threshold);
        }
    }
    
    public MotionBlurType GetCurrentType()
    {
        return currentType;
    }
}
