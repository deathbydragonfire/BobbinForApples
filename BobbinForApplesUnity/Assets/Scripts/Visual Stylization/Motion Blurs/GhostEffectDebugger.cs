using UnityEngine;

public class GhostEffectDebugger : MonoBehaviour
{
    private GhostSpriteEffect ghostEffect;
    private MotionBlurController motionBlurController;
    
    private void Start()
    {
        ghostEffect = GetComponent<GhostSpriteEffect>();
        motionBlurController = GetComponent<MotionBlurController>();
        
        if (ghostEffect == null)
        {
            Debug.LogError($"[GhostDebugger] No GhostSpriteEffect found on {gameObject.name}");
            return;
        }
        
        if (motionBlurController == null)
        {
            Debug.LogWarning($"[GhostDebugger] No MotionBlurController found on {gameObject.name}");
        }
        
        LogStatus();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            LogStatus();
        }
    }
    
    private void LogStatus()
    {
        if (ghostEffect == null) return;
        
        Debug.Log($"[GhostDebugger] {gameObject.name} Status:\n" +
                  $"  Component Enabled: {ghostEffect.enabled}\n" +
                  $"  GameObject Active: {gameObject.activeInHierarchy}\n" +
                  $"  Position: {transform.position}\n" +
                  $"  Has SpriteRenderer: {GetComponent<SpriteRenderer>() != null}\n" +
                  (motionBlurController != null ? $"  MotionBlurType: {motionBlurController.GetCurrentType()}\n" : "") +
                  $"  Press 'G' key to check status again");
    }
}
