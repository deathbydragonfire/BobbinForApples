using UnityEngine;

public class BobbdraPuppetMotionBlur : MonoBehaviour
{
    [Header("Puppet References")]
    [SerializeField] private bool findChildrenAutomatically = true;
    
    private MotionBlurController[] motionBlurControllers;
    
    private void Awake()
    {
        if (findChildrenAutomatically)
        {
            motionBlurControllers = GetComponentsInChildren<MotionBlurController>(true);
        }
    }
    
    public void SetMotionBlurType(MotionBlurController.MotionBlurType blurType)
    {
        if (motionBlurControllers == null || motionBlurControllers.Length == 0)
        {
            Debug.LogWarning($"No MotionBlurControllers found on {gameObject.name}");
            return;
        }
        
        foreach (var controller in motionBlurControllers)
        {
            if (controller != null)
            {
                controller.SetMotionBlurType(blurType);
            }
        }
    }
    
    public void SetAlwaysActive(bool active)
    {
        if (motionBlurControllers == null) return;
        
        foreach (var controller in motionBlurControllers)
        {
            if (controller != null)
            {
                controller.SetAlwaysActive(active);
            }
        }
    }
    
    public void SetMinSpeedThreshold(float threshold)
    {
        if (motionBlurControllers == null) return;
        
        foreach (var controller in motionBlurControllers)
        {
            if (controller != null)
            {
                controller.SetMinSpeedThreshold(threshold);
            }
        }
    }
    
    public MotionBlurController.MotionBlurType GetCurrentType()
    {
        if (motionBlurControllers != null && motionBlurControllers.Length > 0 && motionBlurControllers[0] != null)
        {
            return motionBlurControllers[0].GetCurrentType();
        }
        
        return MotionBlurController.MotionBlurType.None;
    }
    
    public int GetControllerCount()
    {
        return motionBlurControllers != null ? motionBlurControllers.Length : 0;
    }
}
