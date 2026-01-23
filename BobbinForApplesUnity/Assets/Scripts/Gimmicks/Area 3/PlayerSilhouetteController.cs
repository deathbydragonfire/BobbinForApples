using UnityEngine;
using System.Collections;
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
    private bool isSilhouetteActive = false;
    private Coroutine transitionCoroutine;

    public void ApplySilhouette(Material silhouetteMaterial, Color silhouetteColor, float transitionDuration)
    {
        if (isSilhouetteActive)
        {
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        StoreOriginalRenderersData();
        transitionCoroutine = StartCoroutine(TransitionToSilhouette(silhouetteMaterial, silhouetteColor, transitionDuration));
    }

    public void RestoreOriginalMaterials(float transitionDuration)
    {
        if (!isSilhouetteActive)
        {
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(TransitionToOriginal(transitionDuration));
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

        Debug.Log($"Stored data for {meshRenderers.Count} mesh renderers and {spriteRenderers.Count} sprite renderers");
    }

    private IEnumerator TransitionToSilhouette(Material silhouetteMaterial, Color silhouetteColor, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            foreach (var data in spriteRenderers)
            {
                if (data.spriteRenderer != null)
                {
                    data.spriteRenderer.color = Color.Lerp(data.originalColor, silhouetteColor, t);
                }
            }

            yield return null;
        }

        foreach (var data in spriteRenderers)
        {
            if (data.spriteRenderer != null)
            {
                data.spriteRenderer.color = silhouetteColor;
            }
        }

        foreach (var data in meshRenderers)
        {
            if (data.meshRenderer != null)
            {
                Material[] silhouetteMaterials = new Material[data.originalMaterials.Length];
                for (int i = 0; i < silhouetteMaterials.Length; i++)
                {
                    silhouetteMaterials[i] = silhouetteMaterial;
                }
                data.meshRenderer.materials = silhouetteMaterials;
            }
        }

        isSilhouetteActive = true;
        Debug.Log($"Silhouette transition complete");
    }

    private IEnumerator TransitionToOriginal(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            foreach (var data in spriteRenderers)
            {
                if (data.spriteRenderer != null)
                {
                    data.spriteRenderer.color = Color.Lerp(data.spriteRenderer.color, data.originalColor, t);
                }
            }

            yield return null;
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
        isSilhouetteActive = false;
        Debug.Log("Original materials restored");
    }

    private void OnDestroy()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
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
    }
}
