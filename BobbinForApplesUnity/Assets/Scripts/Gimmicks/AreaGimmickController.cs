using UnityEngine;

public class AreaGimmickController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool enableCameraZoom = true;
    [SerializeField] private float zoomedDeadZone = 1f;
    [SerializeField] private bool enableHorizontalFollow = true;

    private CameraZoomController cameraZoomController;
    private CameraFollow cameraFollow;

    private void Awake()
    {
        if (enableCameraZoom)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraZoomController = mainCamera.GetComponent<CameraZoomController>();
                cameraFollow = mainCamera.GetComponent<CameraFollow>();
                
                if (cameraZoomController == null)
                {
                    Debug.LogWarning("AreaGimmickController: No CameraZoomController component found on Main Camera!");
                }
                
                if (cameraFollow == null)
                {
                    Debug.LogWarning("AreaGimmickController: No CameraFollow component found on Main Camera!");
                }
            }
        }
    }

    public void OnPlayerEnterArea()
    {
        if (enableCameraZoom)
        {
            if (cameraZoomController != null)
            {
                cameraZoomController.ZoomIn();
            }
            
            if (cameraFollow != null)
            {
                cameraFollow.SetDeadZone(zoomedDeadZone);
                
                if (enableHorizontalFollow)
                {
                    cameraFollow.EnableHorizontalFollow();
                }
            }
        }
    }

    public void OnPlayerExitArea()
    {
        if (enableCameraZoom)
        {
            if (cameraZoomController != null)
            {
                cameraZoomController.ZoomOut();
            }
            
            if (cameraFollow != null)
            {
                cameraFollow.ResetDeadZone();
                
                if (enableHorizontalFollow)
                {
                    cameraFollow.DisableHorizontalFollow();
                }
            }
        }
    }
}
