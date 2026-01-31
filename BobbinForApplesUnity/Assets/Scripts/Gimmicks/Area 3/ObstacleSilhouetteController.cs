using UnityEngine;

public class ObstacleSilhouetteController : MonoBehaviour
{
    private Material[] originalMaterials;
    private MeshRenderer meshRenderer;
    private Material silhouetteMaterial;
    private bool isInitialized = false;
    private Material[] currentBlendedMaterials;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Initialize(Material material)
    {
        if (isInitialized || meshRenderer == null)
        {
            return;
        }

        silhouetteMaterial = material;
        originalMaterials = meshRenderer.materials;
        isInitialized = true;
    }

    public void UpdateSilhouetteIntensity(float intensity)
    {
        if (!isInitialized || meshRenderer == null || originalMaterials == null)
        {
            return;
        }

        intensity = Mathf.Clamp01(intensity);

        if (currentBlendedMaterials == null || currentBlendedMaterials.Length != originalMaterials.Length)
        {
            CleanupBlendedMaterials();
            currentBlendedMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                currentBlendedMaterials[i] = new Material(originalMaterials[i]);
            }
            meshRenderer.materials = currentBlendedMaterials;
        }

        float adjustedIntensity = Mathf.Pow(intensity, 0.3f);

        for (int i = 0; i < currentBlendedMaterials.Length; i++)
        {
            Color targetBlack = Color.black;
            float colorBlend = adjustedIntensity;
            float propertyBlend = adjustedIntensity;
            
            if (intensity > 0.85f)
            {
                float finalPhase = (intensity - 0.85f) / 0.15f;
                colorBlend = Mathf.Lerp(adjustedIntensity, 1f, finalPhase * finalPhase);
                propertyBlend = colorBlend;
            }
            
            if (currentBlendedMaterials[i].HasProperty("_BaseColor"))
            {
                Color originalColor = originalMaterials[i].GetColor("_BaseColor");
                Color blendedColor = Color.Lerp(originalColor, targetBlack, colorBlend);
                currentBlendedMaterials[i].SetColor("_BaseColor", blendedColor);
            }
            
            if (currentBlendedMaterials[i].HasProperty("_Metallic"))
            {
                float originalMetallic = originalMaterials[i].GetFloat("_Metallic");
                currentBlendedMaterials[i].SetFloat("_Metallic", Mathf.Lerp(originalMetallic, 0f, propertyBlend));
            }
            
            if (currentBlendedMaterials[i].HasProperty("_BaseMetallic"))
            {
                float originalMetallic = originalMaterials[i].GetFloat("_BaseMetallic");
                currentBlendedMaterials[i].SetFloat("_BaseMetallic", Mathf.Lerp(originalMetallic, 0f, propertyBlend));
            }
            
            if (currentBlendedMaterials[i].HasProperty("_Smoothness"))
            {
                float originalSmoothness = originalMaterials[i].GetFloat("_Smoothness");
                currentBlendedMaterials[i].SetFloat("_Smoothness", Mathf.Lerp(originalSmoothness, 0f, propertyBlend));
            }
            
            if (currentBlendedMaterials[i].HasProperty("_BaseSmoothnessRemapMax"))
            {
                float originalSmoothness = originalMaterials[i].GetFloat("_BaseSmoothnessRemapMax");
                currentBlendedMaterials[i].SetFloat("_BaseSmoothnessRemapMax", Mathf.Lerp(originalSmoothness, 0f, propertyBlend));
            }
            
            if (currentBlendedMaterials[i].HasProperty("_SpecColor"))
            {
                Color originalSpecColor = originalMaterials[i].GetColor("_SpecColor");
                currentBlendedMaterials[i].SetColor("_SpecColor", Color.Lerp(originalSpecColor, targetBlack, propertyBlend));
            }
            
            if (currentBlendedMaterials[i].HasProperty("_SpecularColor"))
            {
                Color originalSpecColor = originalMaterials[i].GetColor("_SpecularColor");
                currentBlendedMaterials[i].SetColor("_SpecularColor", Color.Lerp(originalSpecColor, targetBlack, propertyBlend));
            }
            
            if (currentBlendedMaterials[i].HasProperty("_EmissionColor"))
            {
                if (propertyBlend > 0.5f)
                {
                    currentBlendedMaterials[i].EnableKeyword("_EMISSION");
                    float emissionStrength = Mathf.Lerp(0f, 10f, (propertyBlend - 0.5f) * 2f);
                    currentBlendedMaterials[i].SetColor("_EmissionColor", targetBlack * emissionStrength);
                }
                else
                {
                    Color originalEmission = originalMaterials[i].GetColor("_EmissionColor");
                    currentBlendedMaterials[i].SetColor("_EmissionColor", Color.Lerp(originalEmission, targetBlack, propertyBlend * 2f));
                }
            }
            
            if (currentBlendedMaterials[i].HasProperty("_WetSmoothness"))
            {
                float originalWetSmoothness = originalMaterials[i].GetFloat("_WetSmoothness");
                currentBlendedMaterials[i].SetFloat("_WetSmoothness", Mathf.Lerp(originalWetSmoothness, 0f, propertyBlend));
            }
            
            if (currentBlendedMaterials[i].HasProperty("_WetColor"))
            {
                Color originalWetColor = originalMaterials[i].GetColor("_WetColor");
                currentBlendedMaterials[i].SetColor("_WetColor", Color.Lerp(originalWetColor, targetBlack, propertyBlend));
            }
            
            if (currentBlendedMaterials[i].HasProperty("_BaseAORemapMax"))
            {
                float originalAO = originalMaterials[i].GetFloat("_BaseAORemapMax");
                currentBlendedMaterials[i].SetFloat("_BaseAORemapMax", Mathf.Lerp(originalAO, 0f, propertyBlend));
            }
            
            if (currentBlendedMaterials[i].HasProperty("_DetailAlbedoScale"))
            {
                float originalDetail = originalMaterials[i].GetFloat("_DetailAlbedoScale");
                currentBlendedMaterials[i].SetFloat("_DetailAlbedoScale", Mathf.Lerp(originalDetail, 0f, propertyBlend));
            }
        }
    }

    public void RestoreOriginalMaterial()
    {
        if (!isInitialized || meshRenderer == null || originalMaterials == null)
        {
            return;
        }

        meshRenderer.materials = originalMaterials;
        CleanupBlendedMaterials();
        isInitialized = false;
    }

    private void CleanupBlendedMaterials()
    {
        if (currentBlendedMaterials != null)
        {
            foreach (Material mat in currentBlendedMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
            currentBlendedMaterials = null;
        }
    }

    private void OnDestroy()
    {
        CleanupBlendedMaterials();
        
        if (meshRenderer != null && originalMaterials != null && isInitialized)
        {
            meshRenderer.materials = originalMaterials;
        }
    }
}
