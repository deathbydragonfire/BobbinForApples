using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Moving,
        Telegraph,
        Attacking,
        Cooldown,
        Transition,
        Defeated
    }
    
    [Header("Boss Stats")]
    [SerializeField] private float maxHealth = 400f;
    [SerializeField] private float phase2HealthThreshold = 0.66f;
    [SerializeField] private float phase3HealthThreshold = 0.33f;
    
    [Header("Attack Timing")]
    [SerializeField] private float phase1AttackInterval = 5f;
    [SerializeField] private float phase2AttackInterval = 4f;
    [SerializeField] private float phase3AttackInterval = 3f;
    [SerializeField] private float cooldownDuration = 1f;
    
    [Header("References")]
    [SerializeField] private BossMovement bossMovement;
    [SerializeField] private BossAttackManager attackManager;
    [SerializeField] private BossHealthBarUI healthBarUI;
    
    [Header("Visual Feedback")]
    [SerializeField] private float hitShakeDuration = 0.1f;
    [SerializeField] private float hitShakeMagnitude = 0.15f;
    [SerializeField] private float cameraShakeDuration = 0.1f;
    [SerializeField] private float cameraShakeMagnitude = 0.05f;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;
    
    private Renderer[] bossRenderers;
    private Material[] originalMaterials;
    private Material[] flashMaterials;
    private CameraShake cameraShake;
    
    private float currentHealth;
    private int currentPhase = 1;
    private BossState currentState = BossState.Idle;
    private float attackTimer;
    
    public BossState CurrentState => currentState;
    public int CurrentPhase => currentPhase;
    public float HealthPercentage => currentHealth / maxHealth;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        
        if (bossMovement == null)
        {
            bossMovement = GetComponent<BossMovement>();
        }
        
        if (attackManager == null)
        {
            attackManager = GetComponent<BossAttackManager>();
        }
        
        bossRenderers = GetComponentsInChildren<Renderer>();
        if (bossRenderers != null && bossRenderers.Length > 0)
        {
            originalMaterials = new Material[bossRenderers.Length];
            flashMaterials = new Material[bossRenderers.Length];
            
            for (int i = 0; i < bossRenderers.Length; i++)
            {
                if (bossRenderers[i] != null)
                {
                    originalMaterials[i] = bossRenderers[i].material;
                    
                    flashMaterials[i] = new Material(originalMaterials[i]);
                    flashMaterials[i].color = hitFlashColor;
                }
            }
            
            Debug.Log($"Boss materials initialized. Found {bossRenderers.Length} renderers. Flash color: {hitFlashColor}");
        }
        else
        {
            Debug.LogWarning("Boss renderers not found!");
        }
        
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
    
    public void StartBossFight()
    {
        ChangeState(BossState.Moving);
        
        if (healthBarUI != null)
        {
            healthBarUI.Initialize(maxHealth);
            healthBarUI.Show();
        }
        
        if (bossMovement != null)
        {
            bossMovement.StartMovement(currentPhase);
        }
    }
    
    private void Update()
    {
        switch (currentState)
        {
            case BossState.Moving:
                UpdateMovingState();
                break;
            case BossState.Cooldown:
                UpdateCooldownState();
                break;
        }
    }
    
    private void UpdateMovingState()
    {
        attackTimer += Time.deltaTime;
        
        float currentInterval = GetCurrentAttackInterval();
        if (attackTimer >= currentInterval)
        {
            ChangeState(BossState.Telegraph);
            StartCoroutine(ExecuteAttackSequence());
        }
    }
    
    private void UpdateCooldownState()
    {
        attackTimer += Time.deltaTime;
        
        if (attackTimer >= cooldownDuration)
        {
            ChangeState(BossState.Moving);
            attackTimer = 0f;
        }
    }
    
    private IEnumerator ExecuteAttackSequence()
    {
        yield return new WaitForSeconds(1f);
        
        ChangeState(BossState.Attacking);
        
        if (attackManager != null)
        {
            attackManager.ExecuteRandomAttack(currentPhase);
        }
        
        yield return new WaitForSeconds(2f);
        
        ChangeState(BossState.Cooldown);
        attackTimer = 0f;
    }
    
    public void TakeDamage(float damage)
    {
        if (currentState == BossState.Defeated)
        {
            return;
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);
        
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth);
        }
        
        StartCoroutine(HitFeedback());
        
        CheckPhaseTransition();
        
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    private IEnumerator HitFeedback()
    {
        if (cameraShake != null)
        {
            cameraShake.Shake(cameraShakeDuration, cameraShakeMagnitude);
        }
        
        StartCoroutine(BossShake());
        StartCoroutine(FlashBoss());
        
        yield return null;
    }
    
    private IEnumerator BossShake()
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < hitShakeDuration)
        {
            float offsetX = Random.Range(-hitShakeMagnitude, hitShakeMagnitude);
            float offsetY = Random.Range(-hitShakeMagnitude, hitShakeMagnitude);
            
            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
    }
    
    private IEnumerator FlashBoss()
    {
        if (bossRenderers == null || bossRenderers.Length == 0)
        {
            Debug.LogWarning("Cannot flash boss - no renderers found!");
            yield break;
        }
        
        for (int i = 0; i < bossRenderers.Length; i++)
        {
            if (bossRenderers[i] != null && flashMaterials[i] != null)
            {
                bossRenderers[i].material = flashMaterials[i];
            }
        }
        
        yield return new WaitForSeconds(hitFlashDuration);
        
        for (int i = 0; i < bossRenderers.Length; i++)
        {
            if (bossRenderers[i] != null && originalMaterials[i] != null)
            {
                bossRenderers[i].material = originalMaterials[i];
            }
        }
    }
    
    private void CheckPhaseTransition()
    {
        float healthPercent = HealthPercentage;
        
        if (currentPhase == 1 && healthPercent <= phase2HealthThreshold)
        {
            TransitionToPhase(2);
        }
        else if (currentPhase == 2 && healthPercent <= phase3HealthThreshold)
        {
            TransitionToPhase(3);
        }
    }
    
    private void TransitionToPhase(int newPhase)
    {
        currentPhase = newPhase;
        ChangeState(BossState.Transition);
        
        Debug.Log($"Boss transitioning to Phase {newPhase}");
        
        if (bossMovement != null)
        {
            bossMovement.StartMovement(currentPhase);
        }
        
        StartCoroutine(TransitionCoroutine());
    }
    
    private IEnumerator TransitionCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        
        ChangeState(BossState.Moving);
        attackTimer = 0f;
    }
    
    private void Die()
    {
        ChangeState(BossState.Defeated);
        Debug.Log("Boss defeated!");
        
        if (healthBarUI != null)
        {
            healthBarUI.Hide();
        }
        
        BossArenaManager arenaManager = FindFirstObjectByType<BossArenaManager>();
        if (arenaManager != null)
        {
            arenaManager.OnBossDefeated();
        }
        
        StartCoroutine(DeathSequence());
    }
    
    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(2f);
        
        Destroy(gameObject);
    }
    
    private void ChangeState(BossState newState)
    {
        currentState = newState;
    }
    
    private float GetCurrentAttackInterval()
    {
        switch (currentPhase)
        {
            case 1: return phase1AttackInterval;
            case 2: return phase2AttackInterval;
            case 3: return phase3AttackInterval;
            default: return phase1AttackInterval;
        }
    }
    
    public void SetHealthBarUI(BossHealthBarUI healthBar)
    {
        healthBarUI = healthBar;
        Debug.Log("Boss health bar UI assigned");
    }
}
