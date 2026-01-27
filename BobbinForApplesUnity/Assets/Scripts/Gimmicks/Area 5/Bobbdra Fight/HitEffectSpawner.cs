using UnityEngine;

public class HitEffectSpawner : MonoBehaviour
{
    [Header("Effect Prefabs")]
    [SerializeField] private GameObject hitParticlePrefab;
    [SerializeField] private GameObject criticalHitPrefab;
    
    [Header("Settings")]
    [SerializeField] private float effectLifetime = 2f;
    [SerializeField] private bool useWorldSpace = true;
    
    public void SpawnHitEffect(Vector3 position, bool isCritical = false)
    {
        GameObject prefabToUse = isCritical && criticalHitPrefab != null ? criticalHitPrefab : hitParticlePrefab;
        
        if (prefabToUse == null)
        {
            return;
        }
        
        GameObject effect = Instantiate(prefabToUse, position, Quaternion.identity);
        
        if (useWorldSpace)
        {
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ParticleSystem.MainModule main = ps.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
            }
        }
        
        Destroy(effect, effectLifetime);
    }
    
    public void SpawnHitEffectAtTransform(Transform targetTransform, bool isCritical = false)
    {
        if (targetTransform != null)
        {
            SpawnHitEffect(targetTransform.position, isCritical);
        }
    }
}
