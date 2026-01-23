using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float normalSize = 5f;
    [SerializeField] private float zoomedSize = 3f;
    [SerializeField] private float zoomSpeed = 2f;

    private Camera cameraComponent;
    private CameraFollow cameraFollow;
    private float targetSize;
    private bool isZooming;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        cameraFollow = GetComponent<CameraFollow>();

        if (cameraComponent == null)
        {
            Debug.LogError("CameraZoomController requires a Camera component!");
        }

        targetSize = normalSize;
    }

    private void LateUpdate()
    {
        if (!isZooming || cameraComponent == null)
        {
            return;
        }

        UpdateZoom();
    }

    private void UpdateZoom()
    {
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

    public void ZoomIn()
    {
        targetSize = zoomedSize;
        isZooming = true;
    }

    public void ZoomOut()
    {
        targetSize = normalSize;
        isZooming = true;
    }

    public void SetZoomLevel(float size)
    {
        targetSize = size;
        isZooming = true;
    }
}
