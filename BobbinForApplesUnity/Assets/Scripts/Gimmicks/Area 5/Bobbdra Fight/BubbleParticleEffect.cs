using UnityEngine;

public class BubbleParticleEffect : MonoBehaviour
{
    [Header("Bubble Particle Settings")]
    [SerializeField] private ParticleSystem bubbleParticles;
    [SerializeField] private Material bubbleMaterial;
    [SerializeField] private float emissionDuration = 0.5f;
    [SerializeField] private SpriteRenderer headSpriteRenderer;
    
    [Header("Bubble Size")]
    [SerializeField] private float minBubbleSize = 0.3f;
    [SerializeField] private float maxBubbleSize = 0.8f;
    
    private void Awake()
    {
        if (bubbleParticles == null)
        {
            bubbleParticles = GetComponent<ParticleSystem>();
        }
        
        if (headSpriteRenderer == null)
        {
            headSpriteRenderer = GetComponentInParent<SpriteRenderer>();
        }
        
        if (bubbleParticles != null)
        {
            ConfigureParticleSystem();
        }
    }
    
    private void ConfigureParticleSystem()
    {
        var main = bubbleParticles.main;
        main.duration = emissionDuration;
        main.loop = false;
        main.startLifetime = 5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(minBubbleSize, maxBubbleSize);
        main.gravityModifier = -0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = bubbleParticles.emission;
        emission.rateOverTime = 50f;
        
        var shape = bubbleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sprite;
        
        if (headSpriteRenderer != null && headSpriteRenderer.sprite != null)
        {
            shape.sprite = headSpriteRenderer.sprite;
        }
        
        shape.spriteRenderer = headSpriteRenderer;
        
        var textureSheetAnimation = bubbleParticles.textureSheetAnimation;
        textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
        
        var renderer = bubbleParticles.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (bubbleMaterial != null)
            {
                renderer.material = bubbleMaterial;
            }
        }
    }
    
    public void PlayBubbleEffect()
    {
        if (bubbleParticles != null)
        {
            bubbleParticles.Play();
        }
    }
}
