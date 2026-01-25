using UnityEngine;

public class ArenaPlayerBoundary : MonoBehaviour
{
    [SerializeField] private BoxCollider arenaCollider;
    [SerializeField] private Transform player;
    [SerializeField] private float padding = 0.5f;
    
    private bool isActive = false;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    
    private void Awake()
    {
        if (arenaCollider == null)
        {
            arenaCollider = GetComponent<BoxCollider>();
        }
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }
    
    private void Start()
    {
        CalculateBounds();
    }
    
    private void CalculateBounds()
    {
        if (arenaCollider == null) return;
        
        Vector3 center = transform.TransformPoint(arenaCollider.center);
        Vector3 size = arenaCollider.size;
        
        float halfWidth = (size.x / 2f) - padding;
        float halfHeight = (size.y / 2f) - padding;
        
        minBounds = new Vector3(center.x - halfWidth, center.y - halfHeight, 0f);
        maxBounds = new Vector3(center.x + halfWidth, center.y + halfHeight, 0f);
    }
    
    private void LateUpdate()
    {
        if (!isActive || player == null) return;
        
        Vector3 playerPos = player.position;
        
        playerPos.x = Mathf.Clamp(playerPos.x, minBounds.x, maxBounds.x);
        playerPos.y = Mathf.Clamp(playerPos.y, minBounds.y, maxBounds.y);
        playerPos.z = 0f;
        
        player.position = playerPos;
    }
    
    public void EnableBoundary()
    {
        CalculateBounds();
        isActive = true;
        Debug.Log($"Arena boundary enabled. Bounds: Min={minBounds}, Max={maxBounds}");
    }
    
    public void DisableBoundary()
    {
        isActive = false;
        Debug.Log("Arena boundary disabled");
    }
}
