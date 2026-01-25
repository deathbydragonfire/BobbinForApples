using UnityEngine;

public class ArenaProjectileBoundary : MonoBehaviour
{
    [SerializeField] private BoxCollider arenaCollider;
    [SerializeField] private ProjectilePool playerProjectilePool;
    [SerializeField] private ProjectilePool bossProjectilePool;
    [SerializeField] private float checkInterval = 0.1f;
    
    private bool isActive = false;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private float checkTimer;
    
    private void Awake()
    {
        if (arenaCollider == null)
        {
            arenaCollider = GetComponent<BoxCollider>();
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
        
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;
        
        minBounds = new Vector3(center.x - halfWidth, center.y - halfHeight, -10f);
        maxBounds = new Vector3(center.x + halfWidth, center.y + halfHeight, 10f);
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckProjectileBounds();
        }
    }
    
    private void CheckProjectileBounds()
    {
        if (playerProjectilePool != null)
        {
            CheckPoolProjectiles(playerProjectilePool);
        }
        
        if (bossProjectilePool != null)
        {
            CheckPoolProjectiles(bossProjectilePool);
        }
    }
    
    private void CheckPoolProjectiles(ProjectilePool pool)
    {
        foreach (Transform child in pool.transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                Vector3 pos = child.position;
                
                if (pos.x < minBounds.x || pos.x > maxBounds.x ||
                    pos.y < minBounds.y || pos.y > maxBounds.y)
                {
                    pool.ReturnObject(child.gameObject);
                }
            }
        }
    }
    
    public void EnableBoundary()
    {
        CalculateBounds();
        isActive = true;
        checkTimer = 0f;
        Debug.Log($"Arena projectile boundary enabled. Bounds: Min={minBounds}, Max={maxBounds}");
    }
    
    public void DisableBoundary()
    {
        isActive = false;
        Debug.Log("Arena projectile boundary disabled");
    }
    
    public void SetProjectilePools(ProjectilePool playerPool, ProjectilePool bossPool)
    {
        playerProjectilePool = playerPool;
        bossProjectilePool = bossPool;
    }
}
