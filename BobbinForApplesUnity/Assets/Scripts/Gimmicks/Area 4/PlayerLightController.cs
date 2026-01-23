using UnityEngine;
using System.Collections;

public class PlayerLightController : MonoBehaviour
{
    [Header("Light Reference")]
    [SerializeField] private Light playerLight;
    
    [Header("Light Positioning")]
    [SerializeField] private float lightZOffset = -2.5f;
    
    private bool isLightActive = false;
    private Coroutine fadeCoroutine;
    private float originalIntensity;
    private bool originalEnabledState;
    private Vector3 originalLocalPosition;
    private bool hasStoredOriginalSettings = false;
    
    private const string LIGHT_OBJECT_NAME = "Player Light";
    private const float FADE_DURATION = 0.5f;

    private void Awake()
    {
        if (playerLight == null)
        {
            FindPlayerLight();
        }
        
        if (playerLight != null && !hasStoredOriginalSettings)
        {
            StoreOriginalLightSettings();
        }
    }

    public void EnableLight(float range, float intensity, Color lightColor)
    {
        if (playerLight == null)
        {
            FindPlayerLight();
        }
        
        if (playerLight == null)
        {
            Debug.LogWarning("PlayerLightController: No player light found!");
            return;
        }
        
        if (!hasStoredOriginalSettings)
        {
            StoreOriginalLightSettings();
        }

        if (isLightActive)
        {
            return;
        }

        ConfigureLight(range, intensity, lightColor);
        PositionLightForward();

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeLightIn(intensity));
    }

    public void DisableLight()
    {
        if (!isLightActive || playerLight == null)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeLightOut());
    }

    private void FindPlayerLight()
    {
        Transform lightTransform = transform.Find(LIGHT_OBJECT_NAME);
        if (lightTransform != null)
        {
            playerLight = lightTransform.GetComponent<Light>();
            if (playerLight != null)
            {
                Debug.Log($"Found existing player light: {LIGHT_OBJECT_NAME}");
            }
        }
        
        if (playerLight == null)
        {
            Light[] childLights = GetComponentsInChildren<Light>(true);
            if (childLights.Length > 0)
            {
                playerLight = childLights[0];
                Debug.Log($"Found player light in children: {playerLight.gameObject.name}");
            }
        }
    }

    private void StoreOriginalLightSettings()
    {
        if (playerLight != null)
        {
            originalIntensity = playerLight.intensity;
            originalEnabledState = playerLight.enabled;
            originalLocalPosition = playerLight.transform.localPosition;
            hasStoredOriginalSettings = true;
            
            Debug.Log($"Stored original light settings - Intensity: {originalIntensity}, Enabled: {originalEnabledState}, Position: {originalLocalPosition}");
        }
    }

    private void PositionLightForward()
    {
        if (playerLight != null)
        {
            Vector3 newPosition = originalLocalPosition;
            newPosition.z = lightZOffset;
            playerLight.transform.localPosition = newPosition;
            
            Debug.Log($"Positioned light forward at local Z: {lightZOffset}");
        }
    }

    private void ConfigureLight(float range, float intensity, Color lightColor)
    {
        if (playerLight != null)
        {
            playerLight.range = range;
            playerLight.color = lightColor;
            playerLight.intensity = 0f;
        }
    }

    private IEnumerator FadeLightIn(float targetIntensity)
    {
        if (playerLight == null)
        {
            yield break;
        }

        playerLight.enabled = true;
        float startIntensity = playerLight.intensity;
        float elapsed = 0f;

        while (elapsed < FADE_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / FADE_DURATION;
            playerLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        playerLight.intensity = targetIntensity;
        isLightActive = true;
        
        Debug.Log($"Player light enabled: Range={playerLight.range}, Intensity={playerLight.intensity}, Position={playerLight.transform.localPosition}");
    }

    private IEnumerator FadeLightOut()
    {
        if (playerLight == null)
        {
            yield break;
        }

        float startIntensity = playerLight.intensity;
        float elapsed = 0f;

        while (elapsed < FADE_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / FADE_DURATION;
            playerLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }

        playerLight.intensity = originalIntensity;
        playerLight.enabled = originalEnabledState;
        playerLight.transform.localPosition = originalLocalPosition;
        isLightActive = false;
        
        Debug.Log("Player light disabled and restored to original settings");
    }

    private void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (playerLight != null && hasStoredOriginalSettings)
        {
            playerLight.intensity = originalIntensity;
            playerLight.enabled = originalEnabledState;
            playerLight.transform.localPosition = originalLocalPosition;
        }
    }
}
