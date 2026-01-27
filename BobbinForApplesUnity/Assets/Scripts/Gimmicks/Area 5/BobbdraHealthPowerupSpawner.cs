using System.Collections;
using UnityEngine;

public class BobbdraHealthPowerupSpawner : MonoBehaviour
{
    [Header("Powerup Settings")]
    [SerializeField] private GameObject healthPowerupPrefab;
    
    [Header("Arena Settings")]
    [SerializeField] private BoxCollider arenaCollider;
    [SerializeField] private float spawnHeight = -798f;
    [SerializeField] private float minDistanceFromBobbdra = 5f;
    
    [Header("Phase Timing")]
    [SerializeField] private float phase2MinSpawnTime = 5f;
    [SerializeField] private float phase2MaxSpawnTime = 15f;
    [SerializeField] private float phase3MinSpawnTime = 5f;
    [SerializeField] private float phase3MaxSpawnTime = 15f;
    
    private BobbdraManager bobbdraManager;
    private bool phase2PowerupSpawned;
    private bool phase3PowerupSpawned;
    private int lastPhase = 1;
    private bool isInitialized;
    
    private void Start()
    {
        if (healthPowerupPrefab == null)
        {
            Debug.LogError("BobbdraHealthPowerupSpawner: Health Powerup Prefab not assigned!");
            enabled = false;
            return;
        }
        
        if (arenaCollider == null)
        {
            Debug.LogError("BobbdraHealthPowerupSpawner: Arena Collider not assigned!");
            enabled = false;
            return;
        }
        
        Debug.Log("BobbdraHealthPowerupSpawner: Waiting for Bobbdra to spawn...");
    }
    
    private void Update()
    {
        if (!isInitialized)
        {
            bobbdraManager = FindFirstObjectByType<BobbdraManager>();
            if (bobbdraManager != null)
            {
                isInitialized = true;
                float initialHealth = bobbdraManager.HealthPercentage;
                Debug.Log($"BobbdraHealthPowerupSpawner: BobbdraManager found! Initial health: {initialHealth * 100}%. Waiting for combat to start...");
            }
            return;
        }
        
        if (bobbdraManager == null || bobbdraManager.IsDead())
        {
            return;
        }
        
        if (bobbdraManager.CurrentState != BobbdraManager.BossFightState.Combat)
        {
            return;
        }
        
        float healthPercentage = bobbdraManager.HealthPercentage;
        int currentPhase = GetCurrentPhase(healthPercentage);
        
        if (currentPhase != lastPhase)
        {
            OnPhaseChange(currentPhase);
            lastPhase = currentPhase;
        }
    }
    
    private int GetCurrentPhase(float healthPercentage)
    {
        if (healthPercentage <= 0.33f)
        {
            return 3;
        }
        else if (healthPercentage <= 0.66f)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }
    
    private void OnPhaseChange(int newPhase)
    {
        Debug.Log($"BobbdraHealthPowerupSpawner: Phase changed to {newPhase}");
        
        if (newPhase == 2 && !phase2PowerupSpawned)
        {
            float spawnDelay = Random.Range(phase2MinSpawnTime, phase2MaxSpawnTime);
            StartCoroutine(SpawnPowerupAfterDelay(spawnDelay, 2));
            phase2PowerupSpawned = true;
        }
        else if (newPhase == 3 && !phase3PowerupSpawned)
        {
            float spawnDelay = Random.Range(phase3MinSpawnTime, phase3MaxSpawnTime);
            StartCoroutine(SpawnPowerupAfterDelay(spawnDelay, 3));
            phase3PowerupSpawned = true;
        }
    }
    
    private IEnumerator SpawnPowerupAfterDelay(float delay, int phase)
    {
        Debug.Log($"BobbdraHealthPowerupSpawner: Scheduling Phase {phase} powerup spawn in {delay} seconds");
        yield return new WaitForSeconds(delay);
        
        SpawnPowerup(phase);
    }
    
    private void SpawnPowerup(int phase)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        GameObject powerup = Instantiate(healthPowerupPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"BobbdraHealthPowerupSpawner: Phase {phase} health powerup spawned at {spawnPosition}");
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Bounds bounds = arenaCollider.bounds;
        Vector3 spawnPosition;
        int maxAttempts = 20;
        int attempts = 0;
        
        do
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            spawnPosition = new Vector3(randomX, spawnHeight, 0f);
            
            attempts++;
            
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("BobbdraHealthPowerupSpawner: Could not find spawn position away from Bobbdra. Using last attempt.");
                break;
            }
        }
        while (bobbdraManager != null && Vector3.Distance(spawnPosition, bobbdraManager.transform.position) < minDistanceFromBobbdra);
        
        return spawnPosition;
    }
}
