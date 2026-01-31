using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonarWaveController : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float waveInterval = 7f;
    [SerializeField] private float maxRadius = 100f;
    [SerializeField] private float waveSpeed = 20f;
    
    [Header("Visualization")]
    [SerializeField] private int circleSegments = 80;
    [SerializeField] private bool surfaceConformingMode = false;
    [SerializeField] private int raycastCount = 60;
    [SerializeField] private float raycastMaxDistance = 150f;
    
    [Header("Exclusions")]
    [SerializeField] private List<GameObject> excludedObjects = new List<GameObject>();
    [SerializeField] private List<string> excludedTags = new List<string>();
    
    private LineRenderer lineRenderer;
    private SphereCollider waveCollider;
    private HashSet<GameObject> currentWaveHits = new HashSet<GameObject>();
    private Coroutine waveCoroutine;
    private bool isActive = false;
    private bool isPaused = false;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        waveCollider = GetComponent<SphereCollider>();
        
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            ConfigureLineRenderer();
        }
        
        if (waveCollider == null)
        {
            waveCollider = gameObject.AddComponent<SphereCollider>();
            waveCollider.isTrigger = true;
        }
        
        waveCollider.radius = 0f;
        lineRenderer.enabled = false;
    }
    
    private void ConfigureLineRenderer()
    {
        lineRenderer.positionCount = circleSegments;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }
    
    public void EnableSonar()
    {
        if (isActive) return;
        
        isActive = true;
        isPaused = false;
        
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }
        
        waveCoroutine = StartCoroutine(EmitWavesRoutine());
    }
    
    public void DisableSonar()
    {
        isActive = false;
        isPaused = false;
        
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }
        
        lineRenderer.enabled = false;
        waveCollider.radius = 0f;
        currentWaveHits.Clear();
    }
    
    public void PauseSonar()
    {
        isPaused = true;
        lineRenderer.enabled = false;
        waveCollider.radius = 0f;
    }
    
    public void ResumeSonar()
    {
        if (isActive)
        {
            isPaused = false;
        }
    }
    
    public void TriggerManualWave()
    {
        StartCoroutine(EmitSingleWave());
    }
    
    private IEnumerator EmitWavesRoutine()
    {
        while (isActive)
        {
            if (!isPaused)
            {
                yield return StartCoroutine(EmitSingleWave());
            }
            
            yield return new WaitForSeconds(waveInterval);
        }
    }
    
    private IEnumerator EmitSingleWave()
    {
        currentWaveHits.Clear();
        lineRenderer.enabled = true;
        
        Debug.Log($"Sonar wave starting. Position: {transform.position}");
        
        float currentRadius = 0f;
        
        while (currentRadius < maxRadius)
        {
            if (isPaused)
            {
                lineRenderer.enabled = false;
                waveCollider.radius = 0f;
                yield break;
            }
            
            currentRadius += waveSpeed * Time.deltaTime;
            currentRadius = Mathf.Min(currentRadius, maxRadius);
            
            waveCollider.radius = currentRadius;
            
            if (surfaceConformingMode)
            {
                UpdateSurfaceConformingCircle(currentRadius);
            }
            else
            {
                UpdateFlatCircle(currentRadius);
            }
            
            yield return null;
        }
        
        Debug.Log($"Sonar wave complete. Hit {currentWaveHits.Count} objects");
        
        lineRenderer.enabled = false;
        waveCollider.radius = 0f;
        currentWaveHits.Clear();
    }
    
    private void UpdateFlatCircle(float radius)
    {
        Vector3[] positions = new Vector3[circleSegments];
        
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = (float)i / circleSegments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            
            positions[i] = new Vector3(x, y, 0f);
        }
        
        lineRenderer.positionCount = circleSegments;
        lineRenderer.SetPositions(positions);
    }
    
    private void UpdateSurfaceConformingCircle(float radius)
    {
        List<Vector3> positions = new List<Vector3>();
        
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = (float)i / raycastCount * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 origin = transform.position;
            
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, raycastMaxDistance))
            {
                float distanceToHit = Vector3.Distance(origin, hit.point);
                
                if (distanceToHit <= radius + 5f && distanceToHit >= radius - 5f)
                {
                    positions.Add(transform.InverseTransformPoint(hit.point));
                }
                else if (distanceToHit > radius)
                {
                    Vector3 circlePoint = origin + direction * radius;
                    positions.Add(transform.InverseTransformPoint(circlePoint));
                }
            }
            else
            {
                Vector3 circlePoint = origin + direction * radius;
                positions.Add(transform.InverseTransformPoint(circlePoint));
            }
        }
        
        if (positions.Count > 0)
        {
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isActive || isPaused) return;
        
        if (other.CompareTag("Player")) return;
        
        if (IsExcluded(other.gameObject)) return;
        
        if (currentWaveHits.Contains(other.gameObject)) return;
        
        Debug.Log($"Sonar wave hit: {other.gameObject.name}");
        
        SonarObstacleIlluminator illuminator = other.GetComponentInParent<SonarObstacleIlluminator>();
        if (illuminator != null)
        {
            Debug.Log($"  - Found illuminator on {illuminator.gameObject.name}, triggering fade");
            illuminator.OnWaveHit();
            currentWaveHits.Add(other.gameObject);
        }
        else
        {
            Debug.LogWarning($"  - No SonarObstacleIlluminator on {other.gameObject.name} or its parents!");
        }
    }
    
    private bool IsExcluded(GameObject obj)
    {
        if (excludedObjects.Contains(obj)) return true;
        
        foreach (string tag in excludedTags)
        {
            if (obj.CompareTag(tag)) return true;
        }
        
        return false;
    }
}
