using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private float muzzleOffset = 0.5f;
    
    [Header("References")]
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;
    
    private float nextFireTime;
    private bool shootingEnabled;
    private Plane aimPlane;
    
    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (player == null)
        {
            player = transform;
        }
        
        aimPlane = new Plane(Vector3.forward, player.position);
    }
    
    private void Update()
    {
        if (!shootingEnabled) return;
        
        aimPlane.SetNormalAndPosition(Vector3.forward, player.position);
        
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (Time.time >= nextFireTime)
            {
                FireProjectile();
                nextFireTime = Time.time + fireRate;
            }
        }
    }
    
    private void FireProjectile()
    {
        Vector3 aimDirection = CalculateAimDirection();
        
        if (aimDirection == Vector3.zero)
        {
            return;
        }
        
        Vector3 spawnPosition = player.position + aimDirection * muzzleOffset;
        spawnPosition.z = 0f;
        
        GameObject projectile = projectilePool.GetObject();
        if (projectile != null)
        {
            projectile.transform.position = spawnPosition;
            projectile.transform.rotation = Quaternion.identity;
            
            PlayerProjectile projectileScript = projectile.GetComponent<PlayerProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(aimDirection, projectileSpeed, projectilePool);
            }
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(AudioEventType.PlayerShoot, spawnPosition);
            }
            
            Debug.Log($"Projectile fired at position {spawnPosition}");
        }
    }
    
    private Vector3 CalculateAimDirection()
    {
        if (Mouse.current == null || mainCamera == null)
        {
            return Vector3.zero;
        }
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        
        float enter = 0f;
        if (aimPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = (hitPoint - player.position).normalized;
            return direction;
        }
        
        return Vector3.zero;
    }
    
    public void EnableShooting()
    {
        shootingEnabled = true;
        Debug.Log("Player shooting enabled");
    }
    
    public void DisableShooting()
    {
        shootingEnabled = false;
        Debug.Log("Player shooting disabled");
    }
}
