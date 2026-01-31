using UnityEngine;

public class Area3SilhouetteManager : MonoBehaviour
{
    [Header("Silhouette Settings")]
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Color silhouetteColor = Color.black;
    
    [Header("Depth-Based Settings")]
    [SerializeField] private float topYPosition = 0f;
    [SerializeField] private float halfwayYPosition = -10f;
    [SerializeField] private float updateInterval = 0.1f;
    
    [Header("Filter Options")]
    [SerializeField] private bool includeInactive = false;

    private const string PLAYER_TAG = "Player";
    private GameObject activePlayer;
    private bool playerInArea = false;

    private void Start()
    {
        if (TryGetComponent<Collider>(out Collider trigger))
        {
            Bounds bounds = trigger.bounds;
            topYPosition = bounds.max.y;
            halfwayYPosition = bounds.min.y + (bounds.size.y * 0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            activePlayer = other.gameObject;
            playerInArea = true;
            InitializeSilhouetteControllers();
            InvokeRepeating(nameof(UpdateSilhouetteIntensity), 0f, updateInterval);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            playerInArea = false;
            CancelInvoke(nameof(UpdateSilhouetteIntensity));
            RestorePlayerMaterials(other.gameObject);
            RestoreObstacleMaterials();
            activePlayer = null;
        }
    }

    private void UpdateSilhouetteIntensity()
    {
        if (!playerInArea || activePlayer == null)
        {
            return;
        }

        float playerY = activePlayer.transform.position.y;
        float intensity = CalculateIntensity(playerY);

        UpdatePlayerSilhouette(intensity);
        UpdateObstacleSilhouettes(intensity);
    }

    private float CalculateIntensity(float yPosition)
    {
        if (yPosition >= topYPosition)
        {
            return 0f;
        }
        else if (yPosition <= halfwayYPosition)
        {
            return 1f;
        }
        
        float range = topYPosition - halfwayYPosition;
        float distance = topYPosition - yPosition;
        return Mathf.Clamp01(distance / range);
    }

    private void InitializeSilhouetteControllers()
    {
        if (activePlayer != null)
        {
            PlayerSilhouetteController silhouetteController = activePlayer.GetComponent<PlayerSilhouetteController>();
            if (silhouetteController == null)
            {
                silhouetteController = activePlayer.AddComponent<PlayerSilhouetteController>();
            }
            silhouetteController.Initialize(silhouetteMaterial, silhouetteColor);
        }

        if (silhouetteMaterial == null)
        {
            Debug.LogWarning("Silhouette material not assigned!");
            return;
        }

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
            silhouetteController.Initialize(silhouetteMaterial);
        }
    }

    private void UpdatePlayerSilhouette(float intensity)
    {
        if (activePlayer == null)
        {
            return;
        }

        PlayerSilhouetteController silhouetteController = activePlayer.GetComponent<PlayerSilhouetteController>();
        if (silhouetteController != null)
        {
            silhouetteController.UpdateSilhouetteIntensity(intensity);
        }
    }

    private void UpdateObstacleSilhouettes(float intensity)
    {
        MeshRenderer[] renderers = includeInactive 
            ? GetComponentsInChildren<MeshRenderer>(true) 
            : GetComponentsInChildren<MeshRenderer>(false);

        foreach (MeshRenderer renderer in renderers)
        {
            ObstacleSilhouetteController silhouetteController = renderer.GetComponent<ObstacleSilhouetteController>();
            if (silhouetteController != null)
            {
                silhouetteController.UpdateSilhouetteIntensity(intensity);
            }
        }
    }

    private void RestorePlayerMaterials(GameObject player)
    {
        PlayerSilhouetteController silhouetteController = player.GetComponent<PlayerSilhouetteController>();
        if (silhouetteController != null)
        {
            silhouetteController.RestoreOriginalMaterials();
        }
    }

    private void RestoreObstacleMaterials()
    {
        MeshRenderer[] renderers = includeInactive 
            ? GetComponentsInChildren<MeshRenderer>(true) 
            : GetComponentsInChildren<MeshRenderer>(false);

        foreach (MeshRenderer renderer in renderers)
        {
            ObstacleSilhouetteController silhouetteController = renderer.GetComponent<ObstacleSilhouetteController>();
            if (silhouetteController != null)
            {
                silhouetteController.RestoreOriginalMaterial();
            }
        }
    }
}
