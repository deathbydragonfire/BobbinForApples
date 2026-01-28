using UnityEngine;

public class MotionBlurTester : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private MotionBlurController targetController;
    [SerializeField] private BobbdraPuppetMotionBlur[] puppetControllers;
    [SerializeField] private bool controlAllPuppets = true;
    
    [Header("Test Controls")]
    [SerializeField] private KeyCode toggleBlurKey = KeyCode.B;
    [SerializeField] private KeyCode cycleMethodKey = KeyCode.M;
    [SerializeField] private KeyCode increaseIntensityKey = KeyCode.KeypadPlus;
    [SerializeField] private KeyCode decreaseIntensityKey = KeyCode.KeypadMinus;
    
    [Header("Current State")]
    [SerializeField] private bool blurEnabled = true;
    [SerializeField] private float currentIntensity = 0.5f;
    [SerializeField] private float intensityStep = 0.1f;
    
    private SpriteMotionBlurController shaderController;
    private MotionBlurController.MotionBlurType[] availableTypes;
    private int currentTypeIndex = 0;
    
    private void Start()
    {
        if (targetController == null)
        {
            targetController = GetComponent<MotionBlurController>();
        }
        
        if (targetController != null)
        {
            shaderController = targetController.GetComponent<SpriteMotionBlurController>();
        }
        
        if (controlAllPuppets && puppetControllers.Length == 0)
        {
            puppetControllers = FindObjectsByType<BobbdraPuppetMotionBlur>(FindObjectsSortMode.None);
        }
        
        availableTypes = new MotionBlurController.MotionBlurType[]
        {
            MotionBlurController.MotionBlurType.None,
            MotionBlurController.MotionBlurType.CustomShader,
            MotionBlurController.MotionBlurType.TrailRenderer,
            MotionBlurController.MotionBlurType.GhostSprites
        };
        
        currentTypeIndex = System.Array.IndexOf(availableTypes, targetController.GetCurrentType());
        if (currentTypeIndex < 0) currentTypeIndex = 1;
        
        LogCurrentState();
    }
    
    private void Update()
    {
        if (targetController == null) return;
        
        if (Input.GetKeyDown(toggleBlurKey))
        {
            ToggleBlur();
        }
        
        if (Input.GetKeyDown(cycleMethodKey))
        {
            CycleBlurMethod();
        }
        
        if (Input.GetKeyDown(increaseIntensityKey))
        {
            AdjustIntensity(intensityStep);
        }
        
        if (Input.GetKeyDown(decreaseIntensityKey))
        {
            AdjustIntensity(-intensityStep);
        }
    }
    
    private void ToggleBlur()
    {
        blurEnabled = !blurEnabled;
        
        if (blurEnabled)
        {
            SetBlurType(availableTypes[currentTypeIndex]);
            Debug.Log($"Motion Blur ENABLED - Type: {availableTypes[currentTypeIndex]}");
        }
        else
        {
            SetBlurType(MotionBlurController.MotionBlurType.None);
            Debug.Log("Motion Blur DISABLED");
        }
    }
    
    private void CycleBlurMethod()
    {
        currentTypeIndex = (currentTypeIndex + 1) % availableTypes.Length;
        SetBlurType(availableTypes[currentTypeIndex]);
        blurEnabled = availableTypes[currentTypeIndex] != MotionBlurController.MotionBlurType.None;
        
        LogCurrentState();
    }
    
    private void SetBlurType(MotionBlurController.MotionBlurType blurType)
    {
        if (controlAllPuppets && puppetControllers != null)
        {
            foreach (var puppet in puppetControllers)
            {
                if (puppet != null)
                {
                    puppet.SetMotionBlurType(blurType);
                }
            }
        }
        else if (targetController != null)
        {
            targetController.SetMotionBlurType(blurType);
        }
    }
    
    private void AdjustIntensity(float delta)
    {
        currentIntensity = Mathf.Clamp01(currentIntensity + delta);
        
        if (shaderController != null)
        {
            shaderController.SetBlurIntensity(currentIntensity);
            Debug.Log($"Blur Intensity: {currentIntensity:F2}");
        }
    }
    
    private void LogCurrentState()
    {
        int totalControllers = 0;
        if (controlAllPuppets && puppetControllers != null)
        {
            foreach (var puppet in puppetControllers)
            {
                if (puppet != null)
                {
                    totalControllers += puppet.GetControllerCount();
                }
            }
        }
        
        string state = $"[Motion Blur Tester]\n" +
                      $"Current Type: {availableTypes[currentTypeIndex]}\n" +
                      $"Intensity: {currentIntensity:F2}\n" +
                      (controlAllPuppets ? $"Controlling: {puppetControllers.Length} puppets ({totalControllers} sprites)\n" : "") +
                      $"Controls:\n" +
                      $"  {toggleBlurKey} - Toggle On/Off\n" +
                      $"  {cycleMethodKey} - Cycle Methods\n" +
                      $"  {increaseIntensityKey}/{decreaseIntensityKey} - Adjust Intensity";
        
        Debug.Log(state);
    }
    
    private void OnGUI()
    {
        if (targetController == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 350, 180));
        GUILayout.Box("Motion Blur Tester - All Puppets");
        
        GUILayout.Label($"Status: {(blurEnabled ? "ENABLED" : "DISABLED")}");
        GUILayout.Label($"Type: {availableTypes[currentTypeIndex]}");
        GUILayout.Label($"Intensity: {currentIntensity:F2}");
        
        if (controlAllPuppets && puppetControllers != null)
        {
            int totalSprites = 0;
            foreach (var puppet in puppetControllers)
            {
                if (puppet != null)
                {
                    totalSprites += puppet.GetControllerCount();
                }
            }
            GUILayout.Label($"Puppets: {puppetControllers.Length} ({totalSprites} sprites)");
        }
        
        GUILayout.Label("");
        GUILayout.Label($"[{toggleBlurKey}] Toggle | [{cycleMethodKey}] Cycle Method");
        GUILayout.Label($"[{increaseIntensityKey}]/[{decreaseIntensityKey}] Intensity");
        
        GUILayout.EndArea();
    }
}
