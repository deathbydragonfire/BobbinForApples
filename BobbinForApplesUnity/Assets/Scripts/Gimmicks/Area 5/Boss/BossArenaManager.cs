using System.Collections;
using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [Header("Boss Setup")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Vector3 arenaCenter = Vector3.zero;
    
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float bossArenaCameraSize = 9f;
    [SerializeField] private float normalCameraSize = 5f;
    [SerializeField] private float bossIntroZoomSize = 4f;
    [SerializeField] private bool fixCameraToArena = true;
    [SerializeField] private float cameraTransitionSpeed = 2f;
    [SerializeField] private float introZoomSpeed = 1.5f;
    
    [Header("Player References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerShootingController playerShooting;
    [SerializeField] private PlayerHealthUI playerHealthUI;
    
    [Header("UI References")]
    [SerializeField] private BossUIManager uiManager;
    
    [Header("Exit Settings")]
    [SerializeField] private Collider arenaExitCollider;
    
    [Header("Arena Boundary")]
    [SerializeField] private ArenaPlayerBoundary playerBoundary;
    [SerializeField] private ArenaOutline arenaOutline;
    [SerializeField] private ArenaProjectileBoundary projectileBoundary;
    
    [Header("Projectile Pools")]
    [SerializeField] private ProjectilePool playerProjectilePool;
    [SerializeField] private ProjectilePool bossProjectilePool;
    
    private const string PLAYER_TAG = "Player";
    
    private GameObject spawnedBoss;
    private bool bossSpawned;
    private bool fightActive;
    private CameraFollow cameraFollow;
    private Vector3 originalCameraPosition;
    private bool cameraTransitioning;
    private Vector3 targetCameraPosition;
    
    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            cameraFollow = mainCamera.GetComponent<CameraFollow>();
            originalCameraPosition = mainCamera.transform.position;
        }
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        if (playerShooting == null && player != null)
        {
            playerShooting = player.GetComponent<PlayerShootingController>();
            if (playerShooting == null)
            {
                playerShooting = player.gameObject.AddComponent<PlayerShootingController>();
            }
        }
        
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<BossUIManager>();
        }
        
        if (playerHealthUI == null)
        {
            playerHealthUI = FindFirstObjectByType<PlayerHealthUI>();
        }
        
        if (arenaExitCollider != null)
        {
            arenaExitCollider.enabled = false;
        }
        
        if (playerBoundary == null)
        {
            playerBoundary = FindFirstObjectByType<ArenaPlayerBoundary>();
        }
        
        if (arenaOutline == null)
        {
            arenaOutline = FindFirstObjectByType<ArenaOutline>();
        }
        
        if (projectileBoundary == null)
        {
            projectileBoundary = FindFirstObjectByType<ArenaProjectileBoundary>();
        }
        
        if (projectileBoundary != null)
        {
            projectileBoundary.SetProjectilePools(playerProjectilePool, bossProjectilePool);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG) && !bossSpawned)
        {
            TriggerBossEncounter();
        }
    }
    
    private void Update()
    {
        if (cameraTransitioning)
        {
            UpdateCameraTransition();
        }
    }
    
    public void TriggerBossEncounter()
    {
        Debug.Log("Boss encounter started!");
        
        bossSpawned = true;
        fightActive = true;
        
        StartCoroutine(BossEncounterSequence());
    }
    
    private IEnumerator BossEncounterSequence()
    {
        SpawnBoss();
        
        if (playerHealthUI != null)
        {
            playerHealthUI.Initialize(3);
        }
        
        if (playerBoundary != null)
        {
            playerBoundary.EnableBoundary();
        }
        
        if (arenaOutline != null)
        {
            arenaOutline.ShowOutline();
        }
        
        yield return new WaitForSeconds(0.3f);
        
        yield return StartCoroutine(CameraIntroSequence());
        
        if (playerShooting != null)
        {
            playerShooting.EnableShooting();
        }
        
        if (uiManager != null)
        {
            uiManager.StartBossEncounter();
        }
    }
    
    private IEnumerator CameraIntroSequence()
    {
        if (mainCamera == null || spawnedBoss == null) yield break;
        
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }
        
        Vector3 bossPosition = spawnedBoss.transform.position;
        Vector3 bossZoomPosition = new Vector3(bossPosition.x, bossPosition.y, mainCamera.transform.position.z);
        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        
        Debug.Log("Camera intro: Zooming to boss");
        float elapsed = 0f;
        float zoomToBossDuration = 1.5f;
        while (elapsed < zoomToBossDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomToBossDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, bossZoomPosition, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, bossIntroZoomSize, smoothT);
            
            yield return null;
        }
        
        mainCamera.transform.position = bossZoomPosition;
        mainCamera.orthographicSize = bossIntroZoomSize;
        
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("Camera intro: Zooming back to arena view");
        Vector3 arenaPosition = new Vector3(arenaCenter.x, arenaCenter.y, mainCamera.transform.position.z);
        elapsed = 0f;
        float zoomOutDuration = 2f;
        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomOutDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            mainCamera.transform.position = Vector3.Lerp(bossZoomPosition, arenaPosition, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(bossIntroZoomSize, bossArenaCameraSize, smoothT);
            
            yield return null;
        }
        
        mainCamera.transform.position = arenaPosition;
        mainCamera.orthographicSize = bossArenaCameraSize;
        
        targetCameraPosition = arenaPosition;
        cameraTransitioning = false;
        
        Debug.Log("Camera intro sequence complete");
    }
    
    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss prefab not assigned!");
            return;
        }
        
        Vector3 spawnPosition = bossSpawnPoint != null ? bossSpawnPoint.position : arenaCenter;
        Debug.Log($"Boss spawn calculation - bossSpawnPoint: {(bossSpawnPoint != null ? bossSpawnPoint.position.ToString() : "null")}, arenaCenter: {arenaCenter}, final spawnPosition before Z: {spawnPosition}");
        spawnPosition.z = 0f;
        
        Debug.Log($"Final boss spawn position: {spawnPosition}");
        spawnedBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        
        BossMovement bossMovement = spawnedBoss.GetComponent<BossMovement>();
        if (bossMovement != null)
        {
            bossMovement.SetArenaCenter(arenaCenter);
        }
        
        BossController bossController = spawnedBoss.GetComponent<BossController>();
        BossAttackManager attackManager = spawnedBoss.GetComponent<BossAttackManager>();
        
        if (attackManager != null)
        {
            attackManager.SetProjectilePool(bossProjectilePool);
            attackManager.SetPlayer(player);
        }
        
        if (uiManager != null)
        {
            BossHealthBarUI healthBar = uiManager.GetHealthBarUI();
            if (healthBar != null && bossController != null)
            {
                bossController.SetHealthBarUI(healthBar);
                Debug.Log("Boss health bar UI connected");
            }
            
            uiManager.InitializeBossHealth(400f);
        }
        
        Debug.Log($"Boss spawned at: {spawnPosition} with tag: {spawnedBoss.tag}");
    }
    
    private void TransitionCameraToArena()
    {
        if (mainCamera == null) return;
        
        if (fixCameraToArena)
        {
            targetCameraPosition = new Vector3(arenaCenter.x, arenaCenter.y, mainCamera.transform.position.z);
            cameraTransitioning = true;
            
            if (cameraFollow != null)
            {
                cameraFollow.enabled = false;
            }
        }
        
        StartCoroutine(ZoomCamera(bossArenaCameraSize));
    }
    
    private void UpdateCameraTransition()
    {
        if (mainCamera == null || !cameraTransitioning) return;
        
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetCameraPosition,
            cameraTransitionSpeed * Time.deltaTime
        );
        
        if (Vector3.Distance(mainCamera.transform.position, targetCameraPosition) < 0.01f)
        {
            mainCamera.transform.position = targetCameraPosition;
            cameraTransitioning = false;
        }
    }
    
    private IEnumerator ZoomCamera(float targetSize)
    {
        if (mainCamera == null) yield break;
        
        float startSize = mainCamera.orthographic ? mainCamera.orthographicSize : 5f;
        float elapsed = 0f;
        float duration = 1f / cameraTransitionSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            }
            else
            {
                float startFov = ConvertSizeToFov(startSize);
                float targetFov = ConvertSizeToFov(targetSize);
                mainCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, t);
            }
            
            yield return null;
        }
        
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = targetSize;
        }
        else
        {
            mainCamera.fieldOfView = ConvertSizeToFov(targetSize);
        }
    }
    
    private float ConvertSizeToFov(float size)
    {
        const float baseFov = 60f;
        const float baseSize = 5f;
        return baseFov * (size / baseSize);
    }
    
    public void OnBossDefeated()
    {
        Debug.Log("Boss defeated! Victory!");
        
        fightActive = false;
        
        StartCoroutine(VictorySequence());
    }
    
    private IEnumerator VictorySequence()
    {
        ClearAllProjectiles();
        
        if (uiManager != null)
        {
            uiManager.EndBossEncounter();
        }
        
        if (playerBoundary != null)
        {
            playerBoundary.DisableBoundary();
        }
        
        if (arenaOutline != null)
        {
            arenaOutline.HideOutline();
        }
        
        yield return new WaitForSeconds(1f);
        
        OpenExit();
        
        RestoreCamera();
        
        if (playerShooting != null)
        {
            playerShooting.DisableShooting();
        }
    }
    
    private void ClearAllProjectiles()
    {
        if (playerProjectilePool != null)
        {
            playerProjectilePool.ReturnAllObjects();
        }
        
        if (bossProjectilePool != null)
        {
            bossProjectilePool.ReturnAllObjects();
        }
        
        Debug.Log("All projectiles cleared from arena");
    }
    
    private void OpenExit()
    {
        if (arenaExitCollider != null)
        {
            arenaExitCollider.enabled = true;
            Debug.Log("Arena exit opened");
        }
    }
    
    private void RestoreCamera()
    {
        if (mainCamera == null) return;
        
        if (fixCameraToArena && cameraFollow != null)
        {
            cameraFollow.enabled = true;
        }
        
        StartCoroutine(ZoomCamera(normalCameraSize));
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(arenaCenter, 0.5f);
        
        if (bossSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bossSpawnPoint.position, 1f);
        }
    }
}
