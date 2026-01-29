using UnityEngine;
using UnityEngine.Rendering;

public class UnderwaterEffectManager : MonoBehaviour
{
    [Header("Volume Settings")]
    [Tooltip("The Volume component to control (usually on this GameObject)")]
    [SerializeField] private Volume underwaterVolume;
    
    [Header("Transition Settings")]
    [Tooltip("Speed of the fade in/out transition")]
    [SerializeField] private float transitionSpeed = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private UnderwaterEffectVolume underwaterEffectVolume;
    private float targetIntensity = 0f;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (underwaterVolume == null)
        {
            underwaterVolume = GetComponent<Volume>();
        }

        if (underwaterVolume == null)
        {
            Debug.LogError("UnderwaterEffectManager: Volume component not found!");
            return;
        }

        if (underwaterVolume.profile == null)
        {
            Debug.LogError("UnderwaterEffectManager: Volume Profile not assigned to Volume component!");
            return;
        }

        if (!underwaterVolume.profile.TryGet(out underwaterEffectVolume))
        {
            Debug.LogError($"UnderwaterEffectManager: UnderwaterEffectVolume component not found in Volume profile '{underwaterVolume.profile.name}'! " +
                          "Please add the 'Underwater Effect Volume' override to your Volume Profile in the Inspector.");
        }
        else
        {
            underwaterEffectVolume.effectIntensity.value = 0f;
            Debug.Log($"UnderwaterEffectManager: Successfully initialized with Volume Profile '{underwaterVolume.profile.name}'");
        }
    }

    private void Update()
    {
        if (isTransitioning && underwaterEffectVolume != null)
        {
            float currentIntensity = underwaterEffectVolume.effectIntensity.value;
            float newIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, transitionSpeed * Time.deltaTime);
            underwaterEffectVolume.effectIntensity.value = newIntensity;

            if (Mathf.Approximately(newIntensity, targetIntensity))
            {
                isTransitioning = false;
                
                if (showDebugLogs)
                {
                    Debug.Log($"Underwater effect transition complete. Intensity: {newIntensity}");
                }
            }
        }
    }

    public void EnableUnderwaterEffect()
    {
        if (underwaterEffectVolume != null)
        {
            targetIntensity = 1f;
            isTransitioning = true;
            
            if (showDebugLogs)
            {
                Debug.Log("Enabling underwater effect");
            }
        }
    }

    public void DisableUnderwaterEffect()
    {
        if (underwaterEffectVolume != null)
        {
            targetIntensity = 0f;
            isTransitioning = true;
            
            if (showDebugLogs)
            {
                Debug.Log("Disabling underwater effect");
            }
        }
    }

    public void SetEffectIntensity(float intensity)
    {
        if (underwaterEffectVolume != null)
        {
            targetIntensity = Mathf.Clamp01(intensity);
            isTransitioning = true;
            
            if (showDebugLogs)
            {
                Debug.Log($"Setting underwater effect intensity to {targetIntensity}");
            }
        }
    }

    public float GetCurrentIntensity()
    {
        if (underwaterEffectVolume != null)
        {
            return underwaterEffectVolume.effectIntensity.value;
        }
        return 0f;
    }
}
