using UnityEngine;

public class BossVisualEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BobbdraManager bobbdraManager;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private HitEffectSpawner hitEffectSpawner;
    [SerializeField] private DamageFlash damageFlash;
    
    [Header("Hit Effects")]
    [SerializeField] private float normalHitShakeDuration = 0.15f;
    [SerializeField] private float normalHitShakeMagnitude = 0.2f;
    [SerializeField] private float criticalHitShakeDuration = 0.3f;
    [SerializeField] private float criticalHitShakeMagnitude = 0.4f;
    
    [Header("Phase Transition")]
    [SerializeField] private GameObject phaseTransitionEffectPrefab;
    [SerializeField] private float phaseTransitionShakeDuration = 0.5f;
    [SerializeField] private float phaseTransitionShakeMagnitude = 0.5f;
    
    private void Awake()
    {
        if (bobbdraManager == null)
        {
            bobbdraManager = GetComponent<BobbdraManager>();
        }
        
        if (cameraShake == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraShake = mainCamera.GetComponent<CameraShake>();
                if (cameraShake == null)
                {
                    cameraShake = mainCamera.gameObject.AddComponent<CameraShake>();
                }
            }
        }
        
        if (hitEffectSpawner == null)
        {
            hitEffectSpawner = GetComponent<HitEffectSpawner>();
        }
        
        if (damageFlash == null)
        {
            damageFlash = GetComponent<DamageFlash>();
        }
    }
    
    public void OnBossHit(Vector3 hitPosition, bool isCritical = false)
    {
        if (damageFlash != null)
        {
            damageFlash.Flash();
        }
        
        if (cameraShake != null)
        {
            float duration = isCritical ? criticalHitShakeDuration : normalHitShakeDuration;
            float magnitude = isCritical ? criticalHitShakeMagnitude : normalHitShakeMagnitude;
            cameraShake.Shake(duration, magnitude);
        }
        
        if (hitEffectSpawner != null)
        {
            hitEffectSpawner.SpawnHitEffect(hitPosition, isCritical);
        }
    }
    
    public void OnPhaseTransition(int newPhase)
    {
        if (cameraShake != null)
        {
            cameraShake.Shake(phaseTransitionShakeDuration, phaseTransitionShakeMagnitude);
        }
        
        if (phaseTransitionEffectPrefab != null && bobbdraManager != null)
        {
            GameObject effect = Instantiate(phaseTransitionEffectPrefab, bobbdraManager.transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
    }
}
