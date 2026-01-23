using UnityEngine;

public class Area4DarknessController : MonoBehaviour
{
    [Header("Radial Lighting")]
    [SerializeField] private bool useRadialLighting = true;
    
    [Header("Scene Light Settings")]
    [SerializeField] private float darknessFadeDuration = 1f;
    
    [Header("Optional Scene Lights")]
    [SerializeField] private Light[] sceneLights;
    [SerializeField] private bool autoFindSceneLights = true;
    
    private const string PLAYER_TAG = "Player";
    
    private float[] originalLightIntensities;
    private Color originalAmbientColor;
    private float originalAmbientIntensity;
    private UnityEngine.Rendering.AmbientMode originalAmbientMode;
    private bool originalFogEnabled;
    private Material originalSkybox;
    private float originalReflectionIntensity;
    private UnityEngine.Rendering.DefaultReflectionMode originalReflectionMode;
    private Camera mainCamera;
    private DepthBasedWaterEffect depthWaterEffect;
    private RadialLightController radialLightController;
    private Coroutine darknessTransitionCoroutine;

    private void Awake()
    {
        mainCamera = Camera.main;
        
        if (mainCamera != null)
        {
            depthWaterEffect = mainCamera.GetComponent<DepthBasedWaterEffect>();
        }
        
        if (useRadialLighting)
        {
            radialLightController = GetComponent<RadialLightController>();
            if (radialLightController == null)
            {
                radialLightController = gameObject.AddComponent<RadialLightController>();
            }
        }
        
        if (autoFindSceneLights && (sceneLights == null || sceneLights.Length == 0))
        {
            Light[] foundLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            
            int count = 0;
            foreach (Light light in foundLights)
            {
                if (light.type == LightType.Directional || light.type == LightType.Point)
                {
                    count++;
                }
            }
            
            sceneLights = new Light[count];
            int index = 0;
            foreach (Light light in foundLights)
            {
                if (light.type == LightType.Directional || light.type == LightType.Point)
                {
                    sceneLights[index] = light;
                    index++;
                }
            }
            
            Debug.Log($"Area 4: Auto-found {sceneLights.Length} scene lights");
        }
        
        StoreOriginalLightingSettings();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            DisableSceneLighting();
            
            if (useRadialLighting && radialLightController != null)
            {
                radialLightController.EnableRadialLight();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            RestoreSceneLighting();
            
            if (useRadialLighting && radialLightController != null)
            {
                radialLightController.DisableRadialLight();
            }
        }
    }

    private void StoreOriginalLightingSettings()
    {
        originalAmbientColor = RenderSettings.ambientLight;
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        originalAmbientMode = RenderSettings.ambientMode;
        originalFogEnabled = RenderSettings.fog;
        originalSkybox = RenderSettings.skybox;
        originalReflectionIntensity = RenderSettings.reflectionIntensity;
        originalReflectionMode = RenderSettings.defaultReflectionMode;
        
        if (sceneLights != null && sceneLights.Length > 0)
        {
            originalLightIntensities = new float[sceneLights.Length];
            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (sceneLights[i] != null)
                {
                    originalLightIntensities[i] = sceneLights[i].intensity;
                }
            }
        }
    }

    private void DisableSceneLighting()
    {
        if (depthWaterEffect != null)
        {
            depthWaterEffect.OverrideColors(Color.black, Color.black, 0f);
        }
        
        if (darknessTransitionCoroutine != null)
        {
            StopCoroutine(darknessTransitionCoroutine);
        }
        
        darknessTransitionCoroutine = StartCoroutine(TransitionToDarkness());
    }

    private void RestoreSceneLighting()
    {
        if (darknessTransitionCoroutine != null)
        {
            StopCoroutine(darknessTransitionCoroutine);
        }
        
        darknessTransitionCoroutine = StartCoroutine(TransitionToLight());
        
        if (depthWaterEffect != null)
        {
            depthWaterEffect.ClearOverride();
        }
    }

    private System.Collections.IEnumerator TransitionToDarkness()
    {
        float elapsed = 0f;

        while (elapsed < darknessFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / darknessFadeDuration;

            RenderSettings.ambientLight = Color.Lerp(originalAmbientColor, Color.black, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(originalAmbientIntensity, 0f, t);
            RenderSettings.reflectionIntensity = Mathf.Lerp(originalReflectionIntensity, 0f, t);

            if (sceneLights != null)
            {
                for (int i = 0; i < sceneLights.Length; i++)
                {
                    if (sceneLights[i] != null && originalLightIntensities != null && i < originalLightIntensities.Length)
                    {
                        sceneLights[i].intensity = Mathf.Lerp(originalLightIntensities[i], 0f, t);
                    }
                }
            }

            yield return null;
        }

        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientIntensity = 0f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.fog = false;
        RenderSettings.skybox = null;
        RenderSettings.reflectionIntensity = 0f;
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;

        if (sceneLights != null)
        {
            foreach (Light light in sceneLights)
            {
                if (light != null)
                {
                    light.enabled = false;
                }
            }
        }

        Debug.Log("Area 4: Complete darkness achieved");
    }

    private System.Collections.IEnumerator TransitionToLight()
    {
        if (sceneLights != null)
        {
            foreach (Light light in sceneLights)
            {
                if (light != null)
                {
                    light.enabled = true;
                }
            }
        }

        float elapsed = 0f;

        while (elapsed < darknessFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / darknessFadeDuration;

            RenderSettings.ambientLight = Color.Lerp(Color.black, originalAmbientColor, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(0f, originalAmbientIntensity, t);
            RenderSettings.reflectionIntensity = Mathf.Lerp(0f, originalReflectionIntensity, t);

            if (sceneLights != null)
            {
                for (int i = 0; i < sceneLights.Length; i++)
                {
                    if (sceneLights[i] != null && originalLightIntensities != null && i < originalLightIntensities.Length)
                    {
                        sceneLights[i].intensity = Mathf.Lerp(0f, originalLightIntensities[i], t);
                    }
                }
            }

            yield return null;
        }

        RenderSettings.ambientLight = originalAmbientColor;
        RenderSettings.ambientIntensity = originalAmbientIntensity;
        RenderSettings.ambientMode = originalAmbientMode;
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.skybox = originalSkybox;
        RenderSettings.reflectionIntensity = originalReflectionIntensity;
        RenderSettings.defaultReflectionMode = originalReflectionMode;

        if (sceneLights != null)
        {
            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (sceneLights[i] != null && originalLightIntensities != null && i < originalLightIntensities.Length)
                {
                    sceneLights[i].intensity = originalLightIntensities[i];
                }
            }
        }

        Debug.Log("Area 4: Lighting fully restored");
    }

    private void OnDestroy()
    {
        if (darknessTransitionCoroutine != null)
        {
            StopCoroutine(darknessTransitionCoroutine);
        }

        RenderSettings.ambientLight = originalAmbientColor;
        RenderSettings.ambientIntensity = originalAmbientIntensity;
        RenderSettings.ambientMode = originalAmbientMode;
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.skybox = originalSkybox;
        RenderSettings.reflectionIntensity = originalReflectionIntensity;
        RenderSettings.defaultReflectionMode = originalReflectionMode;

        if (depthWaterEffect != null)
        {
            depthWaterEffect.ClearOverride();
        }

        if (sceneLights != null && originalLightIntensities != null)
        {
            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (sceneLights[i] != null && i < originalLightIntensities.Length)
                {
                    sceneLights[i].enabled = true;
                    sceneLights[i].intensity = originalLightIntensities[i];
                }
            }
        }
    }
}
