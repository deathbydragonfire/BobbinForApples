using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float deadZone = 2f;
    
    [Header("Horizontal Follow")]
    [SerializeField] private bool enableHorizontalFollow = false;
    [SerializeField] private float horizontalDeadZone = 2f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float normalSize = 5f;
    [SerializeField] private float zoomInSize = 3f;
    [SerializeField] private float zoomSpeed = 2f;
    
    private Camera cameraComponent;
    private Vector3 fixedOffset;
    private float targetSize;
    private bool isZooming;
    private float originalDeadZone;
    private float originalHorizontalDeadZone;
    private float currentDeadZone;
    private float currentHorizontalDeadZone;
    private bool horizontalFollowActive;

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
        originalDeadZone = deadZone;
        originalHorizontalDeadZone = horizontalDeadZone;
        currentDeadZone = deadZone;
        currentHorizontalDeadZone = horizontalDeadZone;
        horizontalFollowActive = enableHorizontalFollow;
        
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
        
        if (horizontalFollowActive)
        {
            FollowTargetX();
        }
        
        UpdateZoom();
    }

    private void FollowTargetY()
    {
        float currentCameraY = transform.position.y;
        float targetY = target.position.y;
        float deltaY = targetY - currentCameraY;
        
        if (Mathf.Abs(deltaY) > currentDeadZone)
        {
            float direction = Mathf.Sign(deltaY);
            float desiredY = targetY - (direction * currentDeadZone);
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = new Vector3(currentPosition.x, desiredY, currentPosition.z);
            transform.position = Vector3.Lerp(currentPosition, targetPosition, followSpeed * Time.deltaTime);
        }
    }

    private void FollowTargetX()
    {
        float currentCameraX = transform.position.x;
        float targetX = target.position.x;
        float deltaX = targetX - currentCameraX;
        
        if (Mathf.Abs(deltaX) > currentHorizontalDeadZone)
        {
            float direction = Mathf.Sign(deltaX);
            float desiredX = targetX - (direction * currentHorizontalDeadZone);
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = new Vector3(desiredX, currentPosition.y, currentPosition.z);
            transform.position = Vector3.Lerp(currentPosition, targetPosition, followSpeed * Time.deltaTime);
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

    public void SetDeadZone(float newDeadZone)
    {
        currentDeadZone = newDeadZone;
        currentHorizontalDeadZone = newDeadZone;
    }

    public void ResetDeadZone()
    {
        currentDeadZone = originalDeadZone;
        currentHorizontalDeadZone = originalHorizontalDeadZone;
    }

    public void EnableHorizontalFollow()
    {
        horizontalFollowActive = true;
    }

    public void DisableHorizontalFollow()
    {
        horizontalFollowActive = false;
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
