using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("Material Properties")]
    [SerializeField] private string colorPropertyName = "_Color";
    [SerializeField] private string emissionPropertyName = "_EmissionColor";
    [SerializeField] private bool useEmission = false;
    
    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Material[][] flashMaterials;
    private Color[][] originalColors;
    private bool isFlashing;
    
    private void Awake()
    {
        InitializeMaterials();
    }
    
    private void InitializeMaterials()
    {
        renderers = GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            return;
        }
        
        originalMaterials = new Material[renderers.Length][];
        flashMaterials = new Material[renderers.Length][];
        originalColors = new Color[renderers.Length][];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;
            originalMaterials[i] = new Material[materials.Length];
            flashMaterials[i] = new Material[materials.Length];
            originalColors[i] = new Color[materials.Length];
            
            for (int j = 0; j < materials.Length; j++)
            {
                originalMaterials[i][j] = materials[j];
                flashMaterials[i][j] = new Material(materials[j]);
                
                if (materials[j].HasProperty(colorPropertyName))
                {
                    originalColors[i][j] = materials[j].GetColor(colorPropertyName);
                }
            }
        }
    }
    
    public void Flash()
    {
        if (isFlashing)
        {
            StopAllCoroutines();
        }
        
        StartCoroutine(FlashCoroutine());
    }
    
    public void FlashWithDuration(float duration)
    {
        if (isFlashing)
        {
            StopAllCoroutines();
        }
        
        StartCoroutine(FlashCoroutine(duration));
    }
    
    private IEnumerator FlashCoroutine(float duration = -1f)
    {
        isFlashing = true;
        
        if (duration < 0)
        {
            duration = flashDuration;
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = flashCurve.Evaluate(elapsed / duration);
            ApplyFlash(t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        RestoreOriginalMaterials();
        isFlashing = false;
    }
    
    private void ApplyFlash(float intensity)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;
            
            for (int j = 0; j < materials.Length; j++)
            {
                Color targetColor = Color.Lerp(originalColors[i][j], flashColor, intensity);
                
                if (materials[j].HasProperty(colorPropertyName))
                {
                    materials[j].SetColor(colorPropertyName, targetColor);
                }
                
                if (useEmission && materials[j].HasProperty(emissionPropertyName))
                {
                    materials[j].SetColor(emissionPropertyName, flashColor * intensity);
                }
            }
            
            renderers[i].materials = materials;
        }
    }
    
    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;
            
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j].HasProperty(colorPropertyName))
                {
                    materials[j].SetColor(colorPropertyName, originalColors[i][j]);
                }
                
                if (useEmission && materials[j].HasProperty(emissionPropertyName))
                {
                    materials[j].SetColor(emissionPropertyName, Color.black);
                }
            }
            
            renderers[i].materials = materials;
        }
    }
    
    private void OnDisable()
    {
        if (isFlashing)
        {
            StopAllCoroutines();
            RestoreOriginalMaterials();
            isFlashing = false;
        }
    }
}
