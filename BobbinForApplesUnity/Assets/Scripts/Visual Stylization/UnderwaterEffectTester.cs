using UnityEngine;

public class UnderwaterEffectTester : MonoBehaviour
{
    private UnderwaterEffectManager manager;

    private void Start()
    {
        manager = FindFirstObjectByType<UnderwaterEffectManager>();
        
        if (manager == null)
        {
            Debug.LogError("UnderwaterEffectManager not found in scene!");
        }
        else
        {
            Debug.Log("UnderwaterEffectManager found successfully");
        }
    }

    private void Update()
    {
        if (manager != null)
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("Manually enabling underwater effect (U key)");
                manager.EnableUnderwaterEffect();
            }
            
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("Manually disabling underwater effect (I key)");
                manager.DisableUnderwaterEffect();
            }
            
            if (Input.GetKeyDown(KeyCode.O))
            {
                float intensity = manager.GetCurrentIntensity();
                Debug.Log($"Current underwater effect intensity: {intensity}");
            }
        }
    }
}
