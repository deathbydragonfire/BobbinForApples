using UnityEngine;
using UnityEngine.Rendering;

public class DepthBasedWaterEffect : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera mainCamera;

    [Header("Depth Settings")]
    [SerializeField] private float topDepth = 10f;
    [SerializeField] private float bottomDepth = -50f;

    [Header("Color Settings")]
    [SerializeField] private Color shallowWaterColor = new Color(0.4f, 0.7f, 0.9f, 1f);
    [SerializeField] private Color deepWaterColor = new Color(0.05f, 0.1f, 0.2f, 1f);

    [Header("Lighting Settings")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Color shallowLightColor = Color.white;
    [SerializeField] private Color deepLightColor = new Color(0.3f, 0.4f, 0.5f, 1f);
    [SerializeField] private float shallowLightIntensity = 1f;
    [SerializeField] private float deepLightIntensity = 0.3f;

    [Header("Transition Settings")]
    [SerializeField] private AnimationCurve depthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float smoothSpeed = 2f;

    private Color targetBackgroundColor;
    private Color targetLightColor;
    private float targetLightIntensity;
    
    private bool isOverridden = false;
    private Color overrideBackgroundColor;
    private Color overrideLightColor;
    private float overrideLightIntensity;

    private void Awake()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (directionalLight == null)
        {
            directionalLight = FindFirstObjectByType<Light>();
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (isOverridden)
        {
            targetBackgroundColor = overrideBackgroundColor;
            targetLightColor = overrideLightColor;
            targetLightIntensity = overrideLightIntensity;
        }
        else
        {
            float playerDepth = playerTransform.position.y;
            float normalizedDepth = Mathf.InverseLerp(topDepth, bottomDepth, playerDepth);
            normalizedDepth = depthCurve.Evaluate(normalizedDepth);

            targetBackgroundColor = Color.Lerp(shallowWaterColor, deepWaterColor, normalizedDepth);
            targetLightColor = Color.Lerp(shallowLightColor, deepLightColor, normalizedDepth);
            targetLightIntensity = Mathf.Lerp(shallowLightIntensity, deepLightIntensity, normalizedDepth);
        }

        if (mainCamera != null)
        {
            if (smoothTransition)
            {
                mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, targetBackgroundColor, Time.deltaTime * smoothSpeed);
            }
            else
            {
                mainCamera.backgroundColor = targetBackgroundColor;
            }
        }

        if (directionalLight != null)
        {
            if (smoothTransition)
            {
                directionalLight.color = Color.Lerp(directionalLight.color, targetLightColor, Time.deltaTime * smoothSpeed);
                directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, targetLightIntensity, Time.deltaTime * smoothSpeed);
            }
            else
            {
                directionalLight.color = targetLightColor;
                directionalLight.intensity = targetLightIntensity;
            }
        }
    }

    public void OverrideColors(Color backgroundColor, Color lightColor, float lightIntensity)
    {
        isOverridden = true;
        overrideBackgroundColor = backgroundColor;
        overrideLightColor = lightColor;
        overrideLightIntensity = lightIntensity;
    }

    public void ClearOverride()
    {
        isOverridden = false;
    }

    private void OnValidate()
    {
        if (topDepth < bottomDepth)
        {
            bottomDepth = topDepth - 1f;
        }
    }
}
