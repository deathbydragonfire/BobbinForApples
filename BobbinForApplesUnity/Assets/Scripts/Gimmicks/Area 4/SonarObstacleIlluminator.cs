using System.Collections;
using UnityEngine;

public class SonarObstacleIlluminator : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeDuration = 4f;
    
    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private int activeCoroutines = 0;
    
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
    
    private void Awake()
    {
        InitializeRenderers();
        propertyBlock = new MaterialPropertyBlock();
        SetEmissionColor(Color.black);
    }
    
    private void InitializeRenderers()
    {
        Renderer directRenderer = GetComponent<Renderer>();
        
        if (directRenderer != null)
        {
            renderers = new Renderer[] { directRenderer };
        }
        else
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }
    }
    
    public void OnWaveHit()
    {
        if (activeCoroutines > 0)
        {
            StopAllCoroutines();
            activeCoroutines = 0;
        }
        
        activeCoroutines++;
        StartCoroutine(FadeRoutine());
    }
    
    private IEnumerator FadeRoutine()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            Color emissionColor = Color.Lerp(Color.black, Color.white, t);
            SetEmissionColor(emissionColor);
            yield return null;
        }
        
        SetEmissionColor(Color.white);
        
        elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            Color emissionColor = Color.Lerp(Color.white, Color.black, t);
            SetEmissionColor(emissionColor);
            yield return null;
        }
        
        activeCoroutines--;
        
        if (activeCoroutines == 0)
        {
            SetEmissionColor(Color.black);
        }
    }
    
    private void SetEmissionColor(Color emissionColor)
    {
        if (renderers == null || renderers.Length == 0) return;
        
        propertyBlock.SetColor(EmissionColorProperty, emissionColor);
        
        foreach (Renderer rend in renderers)
        {
            if (rend != null)
            {
                rend.SetPropertyBlock(propertyBlock);
                
                if (rend.sharedMaterial != null)
                {
                    if (emissionColor.maxColorComponent > 0.01f)
                    {
                        rend.sharedMaterial.EnableKeyword("_EMISSION");
                    }
                    else
                    {
                        rend.sharedMaterial.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }
}
