using UnityEngine;

public class SimpleParticleEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool destroyAfterPlay = true;
    [SerializeField] private float destroyDelay = 2f;
    
    private void Awake()
    {
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
        }
    }
    
    private void Start()
    {
        if (playOnAwake && particleSystem != null)
        {
            particleSystem.Play();
            
            if (destroyAfterPlay)
            {
                float lifetime = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
                Destroy(gameObject, Mathf.Max(lifetime, destroyDelay));
            }
        }
    }
    
    public void Play()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }
    
    public void Stop()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }
}
