using System.Collections;
using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private ProjectilePool bossProjectilePool;
    [SerializeField] private Transform player;
    
    [Header("Pattern 1: Radial Burst")]
    [SerializeField] private int radialBurstCount = 14;
    [SerializeField] private float radialBurstSpeed = 3f;
    
    [Header("Pattern 2: Spiral Stream")]
    [SerializeField] private float spiralDuration = 3f;
    [SerializeField] private float spiralRotationSpeed = 15f;
    [SerializeField] private float spiralFireInterval = 0.2f;
    [SerializeField] private float spiralSpeed = 4f;
    [SerializeField] private int spiralProjectilesPerBurst = 3;
    
    [Header("Pattern 3: Aimed Triple Shot")]
    [SerializeField] private float aimedSpreadAngle = 10f;
    [SerializeField] private float aimedSpeed = 5f;
    
    [Header("Pattern 4: Wave Barrier")]
    [SerializeField] private int waveProjectileCount = 9;
    [SerializeField] private float waveSpeed = 2f;
    [SerializeField] private float waveSpacing = 1.5f;
    
    [Header("Pattern 5: Laser Sweep")]
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private float laserRotationSpeed = 90f;
    [SerializeField] private float laserDuration = 4f;
    [SerializeField] private float laserRange = 15f;
    
    private Transform bossTransform;
    
    private void Awake()
    {
        bossTransform = transform;
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
    }
    
    public void ExecuteRandomAttack(int phase)
    {
        int attackChoice = 0;
        
        switch (phase)
        {
            case 1:
                attackChoice = Random.Range(0, 2);
                if (attackChoice == 0)
                    RadialBurst();
                else
                    AimedTripleShot();
                break;
                
            case 2:
                attackChoice = Random.Range(0, 4);
                if (attackChoice == 0)
                    RadialBurst();
                else if (attackChoice == 1)
                    StartCoroutine(SpiralStream());
                else if (attackChoice == 2)
                    WaveBarrier();
                else
                    AimedTripleShot();
                break;
                
            case 3:
                attackChoice = Random.Range(0, 5);
                if (attackChoice == 0)
                    RadialBurst();
                else if (attackChoice == 1)
                    StartCoroutine(SpiralStream());
                else if (attackChoice == 2)
                    WaveBarrier();
                else if (attackChoice == 3)
                    AimedTripleShot();
                else
                    StartCoroutine(LaserSweep());
                break;
        }
    }
    
    private void RadialBurst()
    {
        float angleStep = 360f / radialBurstCount;
        
        for (int i = 0; i < radialBurstCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
            
            SpawnProjectile(bossTransform.position, direction, radialBurstSpeed);
        }
        
        Debug.Log("Boss executed: Radial Burst");
    }
    
    private IEnumerator SpiralStream()
    {
        float elapsed = 0f;
        float currentAngle = 0f;
        
        Debug.Log("Boss executed: Spiral Stream");
        
        while (elapsed < spiralDuration)
        {
            for (int i = 0; i < spiralProjectilesPerBurst; i++)
            {
                float offsetAngle = currentAngle + (i * 120f);
                Vector3 direction = Quaternion.Euler(0f, 0f, offsetAngle) * Vector3.right;
                SpawnProjectile(bossTransform.position, direction, spiralSpeed);
            }
            
            currentAngle += spiralRotationSpeed;
            elapsed += spiralFireInterval;
            
            yield return new WaitForSeconds(spiralFireInterval);
        }
    }
    
    private void AimedTripleShot()
    {
        if (player == null) return;
        
        Vector3 toPlayer = (player.position - bossTransform.position).normalized;
        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        
        for (int i = -1; i <= 1; i++)
        {
            float angle = baseAngle + (i * aimedSpreadAngle);
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
            
            SpawnProjectile(bossTransform.position, direction, aimedSpeed);
        }
        
        Debug.Log("Boss executed: Aimed Triple Shot");
    }
    
    private void WaveBarrier()
    {
        Vector3 waveDirection = Random.value > 0.5f ? Vector3.right : Vector3.up;
        Vector3 perpendicular = Vector3.Cross(waveDirection, Vector3.forward).normalized;
        
        int gapIndex = Random.Range(0, waveProjectileCount);
        
        for (int i = 0; i < waveProjectileCount; i++)
        {
            if (i == gapIndex) continue;
            
            float offset = (i - waveProjectileCount * 0.5f) * waveSpacing;
            Vector3 startPosition = bossTransform.position + (perpendicular * offset);
            
            SpawnProjectile(startPosition, waveDirection, waveSpeed);
        }
        
        Debug.Log("Boss executed: Wave Barrier");
    }
    
    private IEnumerator LaserSweep()
    {
        if (laserLine == null)
        {
            Debug.LogWarning("Laser LineRenderer not assigned, skipping laser sweep");
            yield break;
        }
        
        Debug.Log("Boss executed: Laser Sweep");
        
        laserLine.enabled = true;
        float elapsed = 0f;
        float startAngle = 0f;
        
        while (elapsed < laserDuration)
        {
            float currentAngle = startAngle + (laserRotationSpeed * elapsed);
            Vector3 direction = Quaternion.Euler(0f, 0f, currentAngle) * Vector3.right;
            
            Vector3 laserEnd = bossTransform.position + direction * laserRange;
            
            laserLine.SetPosition(0, bossTransform.position);
            laserLine.SetPosition(1, laserEnd);
            
            CheckLaserHit(direction);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        laserLine.enabled = false;
    }
    
    private void CheckLaserHit(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(bossTransform.position, direction, out hit, laserRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Laser hit player!");
            }
        }
    }
    
    private void SpawnProjectile(Vector3 position, Vector3 direction, float speed)
    {
        if (bossProjectilePool == null)
        {
            Debug.LogError("Boss projectile pool not assigned!");
            return;
        }
        
        GameObject projectile = bossProjectilePool.GetObject();
        if (projectile != null)
        {
            projectile.transform.position = position;
            projectile.transform.rotation = Quaternion.identity;
            
            BossProjectile projectileScript = projectile.GetComponent<BossProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, speed, bossProjectilePool);
            }
        }
    }
    
    public void SetProjectilePool(ProjectilePool pool)
    {
        bossProjectilePool = pool;
        Debug.Log("Boss projectile pool assigned");
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        Debug.Log("Boss player target assigned");
    }
}
