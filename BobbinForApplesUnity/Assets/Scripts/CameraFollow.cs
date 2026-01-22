using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float deadZone = 2f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float normalSize = 5f;
    [SerializeField] private float zoomInSize = 3f;
    [SerializeField] private float zoomSpeed = 2f;
    
    private Camera cameraComponent;
    private Vector3 fixedOffset;
    private float targetSize;
    private bool isZooming;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        
        if (cameraComponent == null)
        {
            Debug.LogError("CameraFollow requires a Camera component!");
        }
        
        if (target == null)
        {
            Debug.LogWarning("CameraFollow has no target assigned!");
        }
        
        targetSize = normalSize;
        if (cameraComponent != null)
        {
            if (cameraComponent.orthographic)
            {
                cameraComponent.orthographicSize = normalSize;
            }
        }
    }

    private void Start()
    {
        if (target != null)
        {
            Vector3 currentPosition = transform.position;
            fixedOffset = new Vector3(currentPosition.x, 0f, currentPosition.z);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        
        FollowTargetY();
        UpdateZoom();
    }

    private void FollowTargetY()
    {
        float currentCameraY = transform.position.y;
        float targetY = target.position.y;
        float deltaY = targetY - currentCameraY;
        
        if (Mathf.Abs(deltaY) > deadZone)
        {
            float direction = Mathf.Sign(deltaY);
            float desiredY = targetY - (direction * deadZone);
            Vector3 targetPosition = new Vector3(fixedOffset.x, desiredY, fixedOffset.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }

    private void UpdateZoom()
    {
        if (!isZooming || cameraComponent == null)
        {
            return;
        }
        
        if (cameraComponent.orthographic)
        {
            cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
            
            if (Mathf.Abs(cameraComponent.orthographicSize - targetSize) < 0.01f)
            {
                cameraComponent.orthographicSize = targetSize;
                isZooming = false;
            }
        }
        else
        {
            float currentFov = cameraComponent.fieldOfView;
            float targetFov = ConvertSizeToFov(targetSize);
            cameraComponent.fieldOfView = Mathf.Lerp(currentFov, targetFov, zoomSpeed * Time.deltaTime);
            
            if (Mathf.Abs(currentFov - targetFov) < 0.1f)
            {
                cameraComponent.fieldOfView = targetFov;
                isZooming = false;
            }
        }
    }

    private float ConvertSizeToFov(float size)
    {
        const float baseFov = 60f;
        const float baseSize = 5f;
        return baseFov * (size / baseSize);
    }

    public void ZoomInAndBack()
    {
        StartCoroutine(ZoomSequence());
    }

    private System.Collections.IEnumerator ZoomSequence()
    {
        targetSize = zoomInSize;
        isZooming = true;
        
        while (isZooming)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        targetSize = normalSize;
        isZooming = true;
    }
}
