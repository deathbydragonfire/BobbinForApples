using UnityEngine;
using System.Collections.Generic;

public class PlayerSilhouetteController : MonoBehaviour
{
    private class RendererData
    {
        public MeshRenderer meshRenderer;
        public Material[] originalMaterials;
    }

    private class SpriteRendererData
    {
        public SpriteRenderer spriteRenderer;
        public Color originalColor;
    }

    private List<RendererData> meshRenderers = new List<RendererData>();
    private List<SpriteRendererData> spriteRenderers = new List<SpriteRendererData>();
    private Material silhouetteMaterial;
    private Color silhouetteColor;
    private bool isInitialized = false;

    public void Initialize(Material material, Color color)
    {
        if (isInitialized)
        {
            return;
        }

        silhouetteMaterial = material;
        silhouetteColor = color;
        StoreOriginalRenderersData();
        isInitialized = true;
    }

    public void UpdateSilhouetteIntensity(float intensity)
    {
        if (!isInitialized)
        {
            return;
        }

        intensity = Mathf.Clamp01(intensity);
        float adjustedIntensity = Mathf.Pow(intensity, 0.3f);

        foreach (var data in spriteRenderers)
        {
            if (data.spriteRenderer != null)
            {
                data.spriteRenderer.color = Color.Lerp(data.originalColor, silhouetteColor, adjustedIntensity);
            }
        }

        foreach (var data in meshRenderers)
        {
            if (data.meshRenderer != null)
            {
                Material[] blendedMaterials = new Material[data.originalMaterials.Length];
                for (int i = 0; i < data.originalMaterials.Length; i++)
                {
                    blendedMaterials[i] = new Material(data.originalMaterials[i]);
                    
                    Color targetBlack = Color.black;
                    float colorBlend = adjustedIntensity;
                    float propertyBlend = adjustedIntensity;
                    
                    if (intensity > 0.85f)
                    {
                        float finalPhase = (intensity - 0.85f) / 0.15f;
                        colorBlend = Mathf.Lerp(adjustedIntensity, 1f, finalPhase * finalPhase);
                        propertyBlend = colorBlend;
                    }
                    
                    if (blendedMaterials[i].HasProperty("_BaseColor"))
                    {
                        Color originalColor = data.originalMaterials[i].GetColor("_BaseColor");
                        Color blendedColor = Color.Lerp(originalColor, targetBlack, colorBlend);
                        blendedMaterials[i].SetColor("_BaseColor", blendedColor);
                    }
                    
                    if (blendedMaterials[i].HasProperty("_Metallic"))
                    {
                        float originalMetallic = data.originalMaterials[i].GetFloat("_Metallic");
                        blendedMaterials[i].SetFloat("_Metallic", Mathf.Lerp(originalMetallic, 0f, propertyBlend));
                    }
                    
                    if (blendedMaterials[i].HasProperty("_BaseMetallic"))
                    {
                        float originalMetallic = data.originalMaterials[i].GetFloat("_BaseMetallic");
                        blendedMaterials[i].SetFloat("_BaseMetallic", Mathf.Lerp(originalMetallic, 0f, propertyBlend));
                    }
                    
                    if (blendedMaterials[i].HasProperty("_Smoothness"))
                    {
                        float originalSmoothness = data.originalMaterials[i].GetFloat("_Smoothness");
                        blendedMaterials[i].SetFloat("_Smoothness", Mathf.Lerp(originalSmoothness, 0f, propertyBlend));
                    }
                    
                    if (blendedMaterials[i].HasProperty("_BaseSmoothnessRemapMax"))
                    {
                        float originalSmoothness = data.originalMaterials[i].GetFloat("_BaseSmoothnessRemapMax");
                        blendedMaterials[i].SetFloat("_BaseSmoothnessRemapMax", Mathf.Lerp(originalSmoothness, 0f, propertyBlend));
                    }
                    
                    if (blendedMaterials[i].HasProperty("_SpecColor"))
                    {
                        Color originalSpecColor = data.originalMaterials[i].GetColor("_SpecColor");
                        blendedMaterials[i].SetColor("_SpecColor", Color.Lerp(originalSpecColor, targetBlack, propertyBlend));
                    }
                    
                    if (blendedMaterials[i].HasProperty("_SpecularColor"))
                    {
                        Color originalSpecColor = data.originalMaterials[i].GetColor("_SpecularColor");
                        blendedMaterials[i].SetColor("_SpecularColor", Color.Lerp(originalSpecColor, targetBlack, propertyBlend));
                    }
                    
                    if (blendedMaterials[i].HasProperty("_EmissionColor"))
                    {
                        if (propertyBlend > 0.5f)
                        {
                            blendedMaterials[i].EnableKeyword("_EMISSION");
                            float emissionStrength = Mathf.Lerp(0f, 10f, (propertyBlend - 0.5f) * 2f);
                            blendedMaterials[i].SetColor("_EmissionColor", targetBlack * emissionStrength);
                        }
                        else
                        {
                            Color originalEmission = data.originalMaterials[i].GetColor("_EmissionColor");
                            blendedMaterials[i].SetColor("_EmissionColor", Color.Lerp(originalEmission, targetBlack, propertyBlend * 2f));
                        }
                    }
                    
                    if (blendedMaterials[i].HasProperty("_WetSmoothness"))
                    {
                        float originalWetSmoothness = data.originalMaterials[i].GetFloat("_WetSmoothness");
                        blendedMaterials[i].SetFloat("_WetSmoothness", Mathf.Lerp(originalWetSmoothness, 0f, propertyBlend));
                    }
                    
                    if (blendedMaterials[i].HasProperty("_WetColor"))
                    {
                        Color originalWetColor = data.originalMaterials[i].GetColor("_WetColor");
                        blendedMaterials[i].SetColor("_WetColor", Color.Lerp(originalWetColor, targetBlack, propertyBlend));
                    }
                    
                    if (blendedMaterials[i].HasProperty("_BaseAORemapMax"))
                    {
                        float originalAO = data.originalMaterials[i].GetFloat("_BaseAORemapMax");
                        blendedMaterials[i].SetFloat("_BaseAORemapMax", Mathf.Lerp(originalAO, 0f, propertyBlend));
                    }
                    
                    if (blendedMaterials[i].HasProperty("_DetailAlbedoScale"))
                    {
                        float originalDetail = data.originalMaterials[i].GetFloat("_DetailAlbedoScale");
                        blendedMaterials[i].SetFloat("_DetailAlbedoScale", Mathf.Lerp(originalDetail, 0f, propertyBlend));
                    }
                }
                data.meshRenderer.materials = blendedMaterials;
            }
        }
    }

    public void RestoreOriginalMaterials()
    {
        if (!isInitialized)
        {
            return;
        }

        foreach (var data in spriteRenderers)
        {
            if (data.spriteRenderer != null)
            {
                data.spriteRenderer.color = data.originalColor;
            }
        }

        foreach (var data in meshRenderers)
        {
            if (data.meshRenderer != null)
            {
                data.meshRenderer.materials = data.originalMaterials;
            }
        }

        meshRenderers.Clear();
        spriteRenderers.Clear();
        isInitialized = false;
    }

    private void StoreOriginalRenderersData()
    {
        meshRenderers.Clear();
        spriteRenderers.Clear();

        MeshRenderer[] meshRends = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer renderer in meshRends)
        {
            RendererData data = new RendererData
            {
                meshRenderer = renderer,
                originalMaterials = renderer.materials
            };
            meshRenderers.Add(data);
        }

        SpriteRenderer[] spriteRends = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in spriteRends)
        {
            SpriteRendererData data = new SpriteRendererData
            {
                spriteRenderer = renderer,
                originalColor = renderer.color
            };
            spriteRenderers.Add(data);
        }
    }

    private void OnDestroy()
    {
        foreach (var data in spriteRenderers)
        {
            if (data.spriteRenderer != null)
            {
                data.spriteRenderer.color = data.originalColor;
            }
        }

        foreach (var data in meshRenderers)
        {
            if (data.meshRenderer != null)
            {
                data.meshRenderer.materials = data.originalMaterials;
            }
        }
    }
}
