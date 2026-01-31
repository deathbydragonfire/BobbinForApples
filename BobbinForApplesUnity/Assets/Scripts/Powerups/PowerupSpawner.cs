using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PowerupSpawner : MonoBehaviour
{
    [Header("Powerup Prefabs")]
    [SerializeField] private GameObject boostPrefab;
    [SerializeField] private GameObject busterPrefab;
    [SerializeField] private GameObject heavyPrefab;
    [SerializeField] private GameObject freezePrefab;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("Area 1 Heavy Spawn")]
    [SerializeField] private Transform area1HeavySpawnPoint;
    
    [Header("Spawn Settings")]
    [SerializeField] [Range(0f, 1f)] private float powerupScale = 1f;
    [SerializeField] private float spawnZDepth = 0f;
    
    private void Start()
    {
        SpawnPowerups();
    }
    
    private void SpawnPowerups()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }
        
        if (area1HeavySpawnPoint != null && heavyPrefab != null)
        {
            Vector3 spawnPos = area1HeavySpawnPoint.position;
            spawnPos.z = spawnZDepth;
            GameObject heavy = Instantiate(heavyPrefab, spawnPos, Quaternion.identity);
            heavy.transform.localScale *= powerupScale;
        }
        
        List<GameObject> randomPowerupPrefabs = new List<GameObject>();
        
        if (boostPrefab != null)
        {
            randomPowerupPrefabs.Add(boostPrefab);
        }
        if (busterPrefab != null)
        {
            randomPowerupPrefabs.Add(busterPrefab);
        }
        if (freezePrefab != null)
        {
            randomPowerupPrefabs.Add(freezePrefab);
        }
        
        List<GameObject> shuffledPowerups = randomPowerupPrefabs.OrderBy(x => Random.value).ToList();
        
        int powerupIndex = 0;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == area1HeavySpawnPoint)
            {
                continue;
            }
            
            if (powerupIndex < shuffledPowerups.Count)
            {
                Vector3 spawnPos = spawnPoint.position;
                spawnPos.z = spawnZDepth;
                GameObject powerup = Instantiate(shuffledPowerups[powerupIndex], spawnPos, Quaternion.identity);
                powerup.transform.localScale *= powerupScale;
                powerupIndex++;
            }
        }
    }
}
