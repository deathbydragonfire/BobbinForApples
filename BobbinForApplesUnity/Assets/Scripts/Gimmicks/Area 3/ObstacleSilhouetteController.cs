using UnityEngine;
using System.Collections;

public class ObstacleSilhouetteController : MonoBehaviour
{
    private Material[] originalMaterials;
    private MeshRenderer meshRenderer;
    private bool isSilhouetteActive = false;
    private Coroutine transitionCoroutine;
    private Material[] currentMaterials;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ApplySilhouette(Material silhouetteMaterial, float transitionDuration)
    {
        if (isSilhouetteActive || meshRenderer == null)
        {
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        originalMaterials = meshRenderer.materials;
        transitionCoroutine = StartCoroutine(TransitionToSilhouette(silhouetteMaterial, transitionDuration));
    }

    public void RestoreOriginalMaterial(float transitionDuration)
    {
        if (!isSilhouetteActive || meshRenderer == null || originalMaterials == null)
        {
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(TransitionToOriginal(transitionDuration));
    }

    private IEnumerator TransitionToSilhouette(Material silhouetteMaterial, float duration)
    {
        Material[] silhouetteMaterials = new Material[originalMaterials.Length];
        Material[] transitionMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            silhouetteMaterials[i] = silhouetteMaterial;
            transitionMaterials[i] = new Material(originalMaterials[i]);
        }

        meshRenderer.materials = transitionMaterials;
        currentMaterials = transitionMaterials;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < transitionMaterials.Length; i++)
            {
                if (transitionMaterials[i].HasProperty("_BaseColor"))
                {
                    Color originalColor = originalMaterials[i].GetColor("_BaseColor");
                    Color targetColor = Color.black;
                    transitionMaterials[i].SetColor("_BaseColor", Color.Lerp(originalColor, targetColor, t));
                }
            }

            yield return null;
        }

        meshRenderer.materials = silhouetteMaterials;
        currentMaterials = silhouetteMaterials;
        isSilhouetteActive = true;
    }

    private IEnumerator TransitionToOriginal(float duration)
    {
        Material[] transitionMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            transitionMaterials[i] = new Material(currentMaterials[i]);
        }

        meshRenderer.materials = transitionMaterials;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < transitionMaterials.Length; i++)
            {
                if (transitionMaterials[i].HasProperty("_BaseColor") && originalMaterials[i].HasProperty("_BaseColor"))
                {
                    Color targetColor = originalMaterials[i].GetColor("_BaseColor");
                    Color currentColor = Color.black;
                    transitionMaterials[i].SetColor("_BaseColor", Color.Lerp(currentColor, targetColor, t));
                }
            }

            yield return null;
        }

        meshRenderer.materials = originalMaterials;
        currentMaterials = null;
        isSilhouetteActive = false;
    }

    private void OnDestroy()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        if (meshRenderer != null && originalMaterials != null && isSilhouetteActive)
        {
            meshRenderer.materials = originalMaterials;
        }
    }
}
