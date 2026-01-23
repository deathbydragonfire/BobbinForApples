using UnityEngine;

public class Area3SilhouetteManager : MonoBehaviour
{
    [Header("Silhouette Settings")]
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Color silhouetteColor = Color.black;
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1f;
    
    [Header("Filter Options")]
    [SerializeField] private bool includeInactive = false;

    private const string PLAYER_TAG = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            ApplySilhouetteToPlayer(other.gameObject);
            ApplySilhouetteToObstacles();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            RestorePlayerMaterials(other.gameObject);
            RestoreObstacleMaterials();
        }
    }

    private void ApplySilhouetteToPlayer(GameObject player)
    {
        PlayerSilhouetteController silhouetteController = player.GetComponent<PlayerSilhouetteController>();
        if (silhouetteController == null)
        {
            silhouetteController = player.AddComponent<PlayerSilhouetteController>();
        }

        silhouetteController.ApplySilhouette(silhouetteMaterial, silhouetteColor, transitionDuration);
    }

    private void RestorePlayerMaterials(GameObject player)
    {
        PlayerSilhouetteController silhouetteController = player.GetComponent<PlayerSilhouetteController>();
        if (silhouetteController != null)
        {
            silhouetteController.RestoreOriginalMaterials(transitionDuration);
        }
    }

    private void ApplySilhouetteToObstacles()
    {
        if (silhouetteMaterial == null)
        {
            Debug.LogWarning("Silhouette material not assigned!");
            return;
        }

        int count = 0;
        MeshRenderer[] renderers = includeInactive 
            ? GetComponentsInChildren<MeshRenderer>(true) 
            : GetComponentsInChildren<MeshRenderer>(false);

        foreach (MeshRenderer renderer in renderers)
        {
            ObstacleSilhouetteController silhouetteController = renderer.GetComponent<ObstacleSilhouetteController>();
            if (silhouetteController == null)
            {
                silhouetteController = renderer.gameObject.AddComponent<ObstacleSilhouetteController>();
            }

            silhouetteController.ApplySilhouette(silhouetteMaterial, transitionDuration);
            count++;
        }

        Debug.Log($"Applied silhouette effect to {count} obstacles in Area 3");
    }

    private void RestoreObstacleMaterials()
    {
        int count = 0;
        MeshRenderer[] renderers = includeInactive 
            ? GetComponentsInChildren<MeshRenderer>(true) 
            : GetComponentsInChildren<MeshRenderer>(false);

        foreach (MeshRenderer renderer in renderers)
        {
            ObstacleSilhouetteController silhouetteController = renderer.GetComponent<ObstacleSilhouetteController>();
            if (silhouetteController != null)
            {
                silhouetteController.RestoreOriginalMaterial(transitionDuration);
            }
            count++;
        }

        Debug.Log($"Restored {count} obstacles in Area 3");
    }
}
