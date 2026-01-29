using System.Collections;
using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [Header("Boss Setup")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform arenaCenter;
    
    [Header("Phase 1: Bobbdra")]
    [SerializeField] private GameObject bobbdraPrefab;
    [SerializeField] private Transform bobbdraSpawnPoint;
    [SerializeField] private bool enableTwoPhaseMode = true;
    
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
    [SerializeField] private ArenaOutlineDrawAnimation arenaDrawAnimation;
    
    [Header("Projectile Pools")]
    [SerializeField] private ProjectilePool playerProjectilePool;
    [SerializeField] private ProjectilePool bossProjectilePool;
    
    [Header("Arena Effects")]
    [SerializeField] private BubbleParticleSetup bubbleParticles;
    
    private const string PLAYER_TAG = "Player";
    
    private GameObject spawnedBoss;
    private GameObject spawnedBobbdra;
    private bool bossSpawned;
    private bool fightActive;
    private CameraFollow cameraFollow;
    private Vector3 originalCameraPosition;
    private bool cameraTransitioning;
    private Vector3 targetCameraPosition;
    private bool disableAutoTrigger;
    private bool skipCameraIntro;
    private bool skipOutlineShow;
    
    private enum BossFightPhase { Bobbdra, BulletHell }
    private BossFightPhase currentPhase = BossFightPhase.Bobbdra;
    
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
        
        if (uiManager != null)
        {
            uiManager.OnHealthBarReady += OnHealthBarReady;
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
    
    private void OnDestroy()
    {
        if (uiManager != null)
        {
            uiManager.OnHealthBarReady -= OnHealthBarReady;
        }
    }
    
    private void OnHealthBarReady()
    {
        Debug.Log("Health bar ready - enabling player shooting and boss attacks");
        
        if (playerShooting != null)
        {
            playerShooting.EnableShooting();
        }
        
        if (currentPhase == BossFightPhase.Bobbdra && spawnedBobbdra != null)
        {
            BobbdraManager bobbdraManager = spawnedBobbdra.GetComponent<BobbdraManager>();
            if (bobbdraManager != null)
            {
                bobbdraManager.StartBossFight();
            }
            
            if (bubbleParticles != null)
            {
                bubbleParticles.StartEmission();
            }
        }
        else if (currentPhase == BossFightPhase.BulletHell && spawnedBoss != null)
        {
            BossController bossController = spawnedBoss.GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.StartCombat();
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG) && !bossSpawned && !disableAutoTrigger)
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
    
    public void DisableAutoTrigger()
    {
        disableAutoTrigger = true;
        Debug.Log("Boss auto-trigger disabled (using transition system)");
    }
    
    public void StartCameraZoomDuringFade(float fadeDuration)
    {
        skipCameraIntro = true;
        skipOutlineShow = true;
        StartCoroutine(CameraZoomDuringFade(fadeDuration));
    }
    
    private IEnumerator CameraZoomDuringFade(float fadeDuration)
    {
        if (mainCamera == null) yield break;
        
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }
        
        Vector3 arenaCenterPos = arenaCenter != null ? arenaCenter.position : Vector3.zero;
        Vector3 arenaPosition = new Vector3(arenaCenterPos.x, arenaCenterPos.y, mainCamera.transform.position.z);
        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        
        Debug.Log($"Camera zoom during fade: Moving from {startPosition} to {arenaPosition}, size {startSize} to {bossArenaCameraSize}");
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, arenaPosition, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, bossArenaCameraSize, smoothT);
            
            yield return null;
        }
        
        mainCamera.transform.position = arenaPosition;
        mainCamera.orthographicSize = bossArenaCameraSize;
        targetCameraPosition = arenaPosition;
        
        Debug.Log("Camera zoom during fade complete");
    }
    
    private IEnumerator BossEncounterSequence()
    {
        if (enableTwoPhaseMode && bobbdraPrefab != null)
        {
            SpawnBobbdra();
        }
        else
        {
            SpawnBoss();
        }
        
        if (playerHealthUI != null)
        {
            playerHealthUI.Initialize(3);
        }
        
        if (playerBoundary != null)
        {
            playerBoundary.EnableBoundary();
        }
        
        if (arenaOutline != null && !skipOutlineShow)
        {
            arenaOutline.ShowOutline();
        }
        
        yield return new WaitForSeconds(0.3f);
        
        if (!skipCameraIntro)
        {
            yield return StartCoroutine(CameraIntroSequence());
        }
        else
        {
            Debug.Log("Skipping camera intro sequence (already done during fade)");
        }
        
        if (uiManager != null)
        {
            uiManager.StartBossEncounter();
        }
    }
    
    private IEnumerator CameraIntroSequence()
    {
        if (mainCamera == null) yield break;
        
        GameObject targetBoss = currentPhase == BossFightPhase.Bobbdra ? spawnedBobbdra : spawnedBoss;
        if (targetBoss == null) yield break;
        
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }
        
        Vector3 arenaCenterPos = arenaCenter != null ? arenaCenter.position : Vector3.zero;
        Vector3 arenaZoomPosition = new Vector3(arenaCenterPos.x, arenaCenterPos.y, mainCamera.transform.position.z);
        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        
        Debug.Log("Camera intro: Zooming to arena center");
        float elapsed = 0f;
        float zoomToBossDuration = 1.5f;
        while (elapsed < zoomToBossDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomToBossDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, arenaZoomPosition, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, bossIntroZoomSize, smoothT);
            
            yield return null;
        }
        
        mainCamera.transform.position = arenaZoomPosition;
        mainCamera.orthographicSize = bossIntroZoomSize;
        
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("Camera intro: Zooming back to arena view");
        Vector3 arenaPosition = new Vector3(arenaCenterPos.x, arenaCenterPos.y, mainCamera.transform.position.z);
        elapsed = 0f;
        float zoomOutDuration = 2f;
        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomOutDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            mainCamera.transform.position = Vector3.Lerp(arenaZoomPosition, arenaPosition, smoothT);
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
        Debug.Log($"=== SpawnBoss() called. Current spawnedBoss: {(spawnedBoss != null ? spawnedBoss.name : "null")} ===");
        
        if (spawnedBoss != null)
        {
            Debug.LogError("WARNING: SpawnBoss called but spawnedBoss already exists!");
        }
        
        if (bossPrefab == null)
        {
            Debug.LogError("Boss prefab not assigned!");
            return;
        }
        
        Vector3 arenaCenterPos = arenaCenter != null ? arenaCenter.position : Vector3.zero;
        Vector3 spawnPosition = bossSpawnPoint != null ? bossSpawnPoint.position : arenaCenterPos;
        Debug.Log($"Boss spawn calculation - bossSpawnPoint: {(bossSpawnPoint != null ? bossSpawnPoint.position.ToString() : "null")}, arenaCenter: {(arenaCenter != null ? arenaCenter.position.ToString() : "null")}, final spawnPosition before Z: {spawnPosition}");
        spawnPosition.z = 0f;
        
        Debug.Log($"Final boss spawn position: {spawnPosition}");
        spawnedBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        
        BossMovement bossMovement = spawnedBoss.GetComponent<BossMovement>();
        if (bossMovement != null && arenaCenter != null)
        {
            bossMovement.SetArenaCenter(arenaCenter.position);
            
            if (arenaOutline != null)
            {
                BoxCollider arenaCollider = arenaOutline.GetComponent<BoxCollider>();
                if (arenaCollider == null && arenaDrawAnimation != null)
                {
                    arenaCollider = arenaDrawAnimation.GetComponent<BoxCollider>();
                }
                
                if (arenaCollider != null)
                {
                    bossMovement.SetArenaCollider(arenaCollider);
                    Debug.Log("Arena collider assigned to boss movement");
                }
                else
                {
                    Debug.LogWarning("Arena collider not found!");
                }
            }
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
        
        Debug.Log($"Boss spawned at: {spawnPosition} with tag: {spawnedBoss.tag}, instance: {spawnedBoss.GetInstanceID()}");
    }
    
    private void SpawnBobbdra()
    {
        if (bobbdraPrefab == null)
        {
            Debug.LogError("Bobbdra prefab not assigned! Falling back to regular boss.");
            SpawnBoss();
            return;
        }
        
        Vector3 arenaCenterPos = arenaCenter != null ? arenaCenter.position : Vector3.zero;
        Vector3 spawnPosition = bobbdraSpawnPoint != null ? bobbdraSpawnPoint.position : new Vector3(arenaCenterPos.x, arenaCenterPos.y - 4f, 0f);
        spawnPosition.z = 0f;
        
        Debug.Log($"Bobbdra spawn position: {spawnPosition}");
        spawnedBobbdra = Instantiate(bobbdraPrefab, spawnPosition, Quaternion.identity);
        
        BobbdraManager bobbdraManager = spawnedBobbdra.GetComponent<BobbdraManager>();
        if (bobbdraManager != null)
        {
            bobbdraManager.OnBobbdraDefeated += OnBobbdraPhaseComplete;
        }
        
        if (uiManager != null)
        {
            BossHealthBarUI healthBar = uiManager.GetHealthBarUI();
            if (healthBar != null && bobbdraManager != null)
            {
                bobbdraManager.SetHealthBarUI(healthBar);
                Debug.Log("Bobbdra health bar UI connected");
            }
            
            uiManager.InitializeBossHealth(bobbdraManager != null ? bobbdraManager.MaxHealth : 400f);
        }
        
        Debug.Log($"Bobbdra spawned at: {spawnPosition} with tag: {spawnedBobbdra.tag}");
    }
    
    private void OnBobbdraPhaseComplete()
    {
        Debug.Log("Bobbdra defeated! Starting Phase 2 transition...");
        StartCoroutine(TransitionToPhase2());
    }
    
    private IEnumerator TransitionToPhase2()
    {
        Debug.Log("=== PHASE 2 TRANSITION START ===");
        currentPhase = BossFightPhase.BulletHell;
        
        if (MusicManager.Instance != null)
        {
            Debug.Log("Starting music transition: TitleCard -> Bullet Hell (3 second fade)");
            MusicManager.Instance.PlayMusic("Bullet Hell", 3f);
        }
        
        if (playerShooting != null)
        {
            playerShooting.DisableShooting();
        }
        
        ClearAllProjectiles();
        
        yield return new WaitForSeconds(1.5f);
        
        if (spawnedBobbdra != null)
        {
            Debug.Log("Destroying Bobbdra instance...");
            Destroy(spawnedBobbdra);
            spawnedBobbdra = null;
            yield return new WaitForSeconds(0.2f);
        }
        
        if (spawnedBoss != null)
        {
            Debug.LogError("WARNING: Boss already exists! Destroying old boss before spawning new one.");
            Destroy(spawnedBoss);
            spawnedBoss = null;
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log("=== PHASE 2 TRANSITION COMPLETE - Boss will spawn via cinematic ===");
    }
    
    public void OnBossCinematicComplete(GameObject boss)
    {
        Debug.Log("Boss cinematic complete, finalizing boss setup");
        
        spawnedBoss = boss;
        
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
                StartCoroutine(RefillHealthBarThenChangeTitle(healthBar));
            }
        }
        
        if (playerShooting != null)
        {
            playerShooting.EnableShooting();
        }
        
        if (bossController != null)
        {
            bossController.StartCombat();
        }
        
        Debug.Log("Boss combat started!");
    }
    
    private IEnumerator RefillHealthBarThenChangeTitle(BossHealthBarUI healthBar)
    {
        healthBar.Initialize(0f);
        healthBar.Show();
        
        yield return new WaitForSeconds(1f);
        
        healthBar.RefillHealth(400f, 3f);
        Debug.Log("Boss health bar refilling");
        
        yield return new WaitForSeconds(3f);
        
        healthBar.ChangeTitle("The Heart of the Beast", 0.05f);
        Debug.Log("Boss title changing to 'The Heart of the Beast'");
    }
    
    private void TransitionCameraToArena()
    {
        if (mainCamera == null) return;
        
        if (fixCameraToArena)
        {
            Vector3 arenaCenterPos = arenaCenter != null ? arenaCenter.position : Vector3.zero;
            targetCameraPosition = new Vector3(arenaCenterPos.x, arenaCenterPos.y, mainCamera.transform.position.z);
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
        if (arenaCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(arenaCenter.position, 0.5f);
        }
        
        if (bossSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bossSpawnPoint.position, 1f);
        }
    }
}
