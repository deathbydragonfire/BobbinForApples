using UnityEngine;

public class ArenaOutline : MonoBehaviour
{
    [SerializeField] private BoxCollider arenaCollider;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineWidth = 0.15f;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private Material lineMaterial;
    
    private void Awake()
    {
        if (arenaCollider == null)
        {
            arenaCollider = GetComponent<BoxCollider>();
        }
        
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        
        InitializeLineRenderer();
    }
    
    private void Start()
    {
        UpdateOutline();
    }
    
    private void InitializeLineRenderer()
    {
        if (lineRenderer == null) return;
        
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 4;
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;
        
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        
        lineRenderer.enabled = false;
    }
    
    public void UpdateOutline()
    {
        if (arenaCollider == null || lineRenderer == null) return;
        
        Vector3 center = transform.TransformPoint(arenaCollider.center);
        Vector3 size = arenaCollider.size;
        
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;
        
        Vector3[] corners = new Vector3[4];
        corners[0] = center + new Vector3(-halfWidth, -halfHeight, 0);
        corners[1] = center + new Vector3(halfWidth, -halfHeight, 0);
        corners[2] = center + new Vector3(halfWidth, halfHeight, 0);
        corners[3] = center + new Vector3(-halfWidth, halfHeight, 0);
        
        lineRenderer.SetPositions(corners);
    }
    
    public void ShowOutline()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }
    
    public void HideOutline()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
