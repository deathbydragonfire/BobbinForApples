using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private RectTransform crosshairImage;
    [SerializeField] private Canvas canvas;
    
    private bool isActive;
    
    private void Awake()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        
        if (crosshairImage != null)
        {
            crosshairImage.gameObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (!isActive || crosshairImage == null || Mouse.current == null)
        {
            return;
        }
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        crosshairImage.position = mousePosition;
    }
    
    public void ShowCrosshair()
    {
        if (crosshairImage != null)
        {
            crosshairImage.gameObject.SetActive(true);
            isActive = true;
            Cursor.visible = false;
        }
    }
    
    public void HideCrosshair()
    {
        if (crosshairImage != null)
        {
            crosshairImage.gameObject.SetActive(false);
            isActive = false;
            Cursor.visible = true;
        }
    }
}
