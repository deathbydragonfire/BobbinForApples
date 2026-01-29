using System.Collections;
using UnityEngine;

public class ArenaOutlineDrawAnimation : MonoBehaviour
{
    [SerializeField] private BoxCollider arenaCollider;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private AnimationCurve drawCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    private Vector3[] corners;
    private const int SEGMENTS_PER_SIDE = 50;
    
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
        
        CalculateCorners();
    }
    
    private void CalculateCorners()
    {
        if (arenaCollider == null) return;
        
        Vector3 center = transform.TransformPoint(arenaCollider.center);
        Vector3 size = arenaCollider.size;
        
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;
        
        corners = new Vector3[4];
        corners[0] = center + new Vector3(-halfWidth, halfHeight, 0);
        corners[1] = center + new Vector3(halfWidth, halfHeight, 0);
        corners[2] = center + new Vector3(halfWidth, -halfHeight, 0);
        corners[3] = center + new Vector3(-halfWidth, -halfHeight, 0);
    }
    
    public void StartDrawAnimation()
    {
        StartCoroutine(DrawAnimationCoroutine());
    }
    
    private IEnumerator DrawAnimationCoroutine()
    {
        if (lineRenderer == null || corners == null) yield break;
        
        lineRenderer.enabled = true;
        lineRenderer.loop = false;
        
        int totalPositions = SEGMENTS_PER_SIDE * 4 + 1;
        lineRenderer.positionCount = totalPositions;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / animationDuration);
            float curveValue = drawCurve.Evaluate(normalizedTime);
            
            float totalDistance = curveValue * 4f;
            
            int visiblePositions = 1;
            
            for (int i = 0; i < totalPositions; i++)
            {
                float positionIndex = (float)i / SEGMENTS_PER_SIDE;
                
                if (positionIndex <= totalDistance)
                {
                    Vector3 position = GetPositionOnPerimeter(positionIndex);
                    lineRenderer.SetPosition(i, position);
                    visiblePositions = i + 1;
                }
                else
                {
                    break;
                }
            }
            
            lineRenderer.positionCount = visiblePositions;
            
            yield return null;
        }
        
        lineRenderer.positionCount = 5;
        lineRenderer.SetPosition(0, corners[0]);
        lineRenderer.SetPosition(1, corners[1]);
        lineRenderer.SetPosition(2, corners[2]);
        lineRenderer.SetPosition(3, corners[3]);
        lineRenderer.SetPosition(4, corners[0]);
        lineRenderer.loop = true;
    }
    
    private Vector3 GetPositionOnPerimeter(float distance)
    {
        int side = Mathf.FloorToInt(distance);
        float t = distance - side;
        
        switch (side)
        {
            case 0:
                return Vector3.Lerp(corners[0], corners[1], t);
            case 1:
                return Vector3.Lerp(corners[1], corners[2], t);
            case 2:
                return Vector3.Lerp(corners[2], corners[3], t);
            case 3:
                return Vector3.Lerp(corners[3], corners[0], t);
            default:
                return corners[0];
        }
    }
}
