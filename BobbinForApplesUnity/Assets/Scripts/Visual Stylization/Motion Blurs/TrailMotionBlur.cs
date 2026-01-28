using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailMotionBlur : MonoBehaviour
{
    private TrailRenderer trailRenderer;
    private Rigidbody2D rb2D;
    
    [Header("Motion Blur Settings")]
    [SerializeField] private float minSpeedForTrail = 2f;
    [SerializeField] private float maxTrailTime = 0.5f;
    [SerializeField] private float trailWidth = 0.5f;
    [SerializeField] private bool alwaysActive = false;
    
    [Header("Transform-Based Velocity")]
    [SerializeField] private bool useTransformVelocity = true;
    [SerializeField] private float velocitySmoothing = 0.1f;
    
    private Vector2 previousPosition;
    private Vector2 smoothedVelocity;
    
    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        rb2D = GetComponent<Rigidbody2D>();
        previousPosition = transform.position;
    }
    
    private void Start()
    {
        if (trailRenderer != null)
        {
            trailRenderer.emitting = alwaysActive;
            trailRenderer.widthMultiplier = trailWidth;
        }
    }
    
    private void Update()
    {
        float speed = GetCurrentSpeed();
        
        if (alwaysActive)
        {
            trailRenderer.emitting = true;
            trailRenderer.time = Mathf.Lerp(0.1f, maxTrailTime, Mathf.Clamp01(speed / 10f));
        }
        else
        {
            if (speed > minSpeedForTrail)
            {
                trailRenderer.emitting = true;
                trailRenderer.time = Mathf.Lerp(0.1f, maxTrailTime, Mathf.Clamp01(speed / 10f));
            }
            else
            {
                trailRenderer.emitting = false;
            }
        }
    }
    
    private float GetCurrentSpeed()
    {
        if (useTransformVelocity || rb2D == null)
        {
            Vector2 currentPosition = transform.position;
            Vector2 frameVelocity = (currentPosition - previousPosition) / Time.deltaTime;
            smoothedVelocity = Vector2.Lerp(smoothedVelocity, frameVelocity, velocitySmoothing);
            previousPosition = currentPosition;
            return smoothedVelocity.magnitude;
        }
        else
        {
            return rb2D.linearVelocity.magnitude;
        }
    }
    
    public void SetActive(bool active)
    {
        if (trailRenderer != null)
        {
            trailRenderer.emitting = active;
            if (!active)
            {
                ClearTrail();
            }
        }
    }
    
    public void ClearTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
    
    public void SetTrailSettings(float width, float time, float minSpeed)
    {
        trailWidth = width;
        maxTrailTime = time;
        minSpeedForTrail = minSpeed;
        
        if (trailRenderer != null)
        {
            trailRenderer.widthMultiplier = trailWidth;
        }
    }
}
