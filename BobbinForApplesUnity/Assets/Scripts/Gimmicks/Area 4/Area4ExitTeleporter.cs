using UnityEngine;

public class Area4ExitTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private Vector3 arenaSpawnPosition = new Vector3(0f, -800f, 0f);
    [SerializeField] private BossArenaManager bossArenaManager;
    
    [Header("Exit Detection")]
    [SerializeField] private float exitThreshold = 0.5f;
    
    private BoxCollider areaCollider;
    private bool playerInArea = false;
    private bool hasTriggeredBoss = false;
    
    private void Awake()
    {
        areaCollider = GetComponent<BoxCollider>();
        
        if (areaCollider == null)
        {
            Debug.LogError("Area4ExitTeleporter requires a BoxCollider component!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInArea = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggeredBoss)
        {
            playerInArea = false;
            
            if (IsExitingFromBottom(other.transform.position))
            {
                TeleportPlayerToArena(other.gameObject);
            }
        }
    }
    
    private bool IsExitingFromBottom(Vector3 playerPosition)
    {
        if (areaCollider == null) return false;
        
        Vector3 colliderWorldCenter = transform.position + areaCollider.center;
        float bottomEdge = colliderWorldCenter.y - (areaCollider.size.y / 2f);
        
        float playerBottomEdge = playerPosition.y;
        
        bool isAtBottom = playerBottomEdge <= bottomEdge + exitThreshold;
        
        Debug.Log($"Player exiting Area 4. Position: {playerPosition.y}, Bottom edge: {bottomEdge}, Is at bottom: {isAtBottom}");
        
        return isAtBottom;
    }
    
    private void TeleportPlayerToArena(GameObject player)
    {
        hasTriggeredBoss = true;
        
        Debug.Log($"Teleporting player to arena at {arenaSpawnPosition}");
        
        player.transform.position = arenaSpawnPosition;
        
        Debug.Log($"Player position after teleport: {player.transform.position}");
        
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            Debug.Log("Player velocity reset to zero");
        }
        
        DiverBuoyancy buoyancy = player.GetComponent<DiverBuoyancy>();
        if (buoyancy != null)
        {
            buoyancy.enabled = false;
            Debug.Log("Player buoyancy disabled for boss fight");
        }
        
        if (bossArenaManager != null)
        {
            bossArenaManager.TriggerBossEncounter();
        }
        else
        {
            Debug.LogError("BossArenaManager reference not assigned!");
        }
    }
}
