using System;
using System.Collections;
using UnityEngine;

public class BobbdraManager : MonoBehaviour
{
    public event Action OnBobbdraDefeated;
    
    public enum BossFightState
    {
        Idle,
        Intro,
        Combat,
        Death
    }
    
    [Header("Boss Stats")]
    [SerializeField] private float maxHealth = 400f;
    [SerializeField] private float phase2HealthThreshold = 0.66f;
    [SerializeField] private float phase3HealthThreshold = 0.33f;
    
    [Header("Head References")]
    [SerializeField] private BobbdraHead leftHead;
    [SerializeField] private BobbdraHead centerHead;
    [SerializeField] private BobbdraHead rightHead;
    
    [Header("Attack Patterns")]
    [SerializeField] private float baseAttackCooldown = 5f;
    [SerializeField] private SequentialBitePattern sequentialBitePattern;
    [SerializeField] private ProjectileBarragePattern projectileBarragePattern;
    [SerializeField] private SideAmbushPattern sideAmbushPattern;
    [SerializeField] private DownAttackPattern downAttackPattern;
    
    [Header("Attack Weights - Phase 1")]
    [SerializeField] private float phase1SequentialBiteWeight = 50f;
    [SerializeField] private float phase1ProjectileBarrageWeight = 50f;
    [SerializeField] private float phase1SideAmbushWeight = 10f;
    [SerializeField] private float phase1DownAttackWeight = 10f;
    
    [Header("Attack Weights - Phase 2")]
    [SerializeField] private float phase2SequentialBiteWeight = 40f;
    [SerializeField] private float phase2ProjectileBarrageWeight = 40f;
    [SerializeField] private float phase2SideAmbushWeight = 30f;
    [SerializeField] private float phase2DownAttackWeight = 30f;
    
    [Header("Attack Weights - Phase 3")]
    [SerializeField] private float phase3SequentialBiteWeight = 30f;
    [SerializeField] private float phase3ProjectileBarrageWeight = 30f;
    [SerializeField] private float phase3SideAmbushWeight = 50f;
    [SerializeField] private float phase3DownAttackWeight = 50f;
    
    [Header("Visual Feedback")]
    [SerializeField] private float hitShakeDuration = 0.1f;
    [SerializeField] private float hitShakeMagnitude = 0.15f;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private BossVisualEffects visualEffects;
    
    [Header("Death Sequence")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private BobbdraDeathSequence deathSequence;
    [SerializeField] private AnimationClip outerDeathsClip;
    [SerializeField] private AnimationClip centerDeathClip;
    
    [Header("Intro Animation")]
    [SerializeField] private AnimationClip roarClip;
    [SerializeField] private float introRiseDistance = 10f;
    [SerializeField] private float introRiseDuration = 5f;
    
    [Header("Annoying Head")]
    [SerializeField] private GameObject annoyingHeadPrefab;
    [SerializeField] private float annoyingHeadSpawnDelay = 3f;
    
    private float currentHealth;
    private BossFightState currentState = BossFightState.Idle;
    private float attackTimer;
    private int currentPhaseLevel = 1;
    private BobbdraHead[] heads;
    private BobbdraAttackPattern[] attackPatterns;
    private int lastPatternIndex = -1;
    private bool isExecutingPattern;
    
    private Renderer[] bossRenderers;
    private Material[] originalMaterials;
    private Material[] flashMaterials;
    
    private BossHealthBarUI healthBarUI;
    private bool isInvincible;
    private AnnoyingHeadController spawnedAnnoyingHead;
    
    public BossFightState CurrentState => currentState;
    public float HealthPercentage => currentHealth / maxHealth;
    public float MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        
        heads = new BobbdraHead[3];
        heads[0] = leftHead;
        heads[1] = centerHead;
        heads[2] = rightHead;
        
        for (int i = 0; i < heads.Length; i++)
        {
            if (heads[i] != null)
            {
                heads[i].Initialize(this);
            }
        }
        
        attackPatterns = new BobbdraAttackPattern[4];
        attackPatterns[0] = sequentialBitePattern;
        attackPatterns[1] = projectileBarragePattern;
        attackPatterns[2] = sideAmbushPattern;
        attackPatterns[3] = downAttackPattern;
        
        for (int i = 0; i < attackPatterns.Length; i++)
        {
            if (attackPatterns[i] != null)
            {
                attackPatterns[i].Initialize(this, heads);
            }
        }
        
        if (visualEffects == null)
        {
            visualEffects = GetComponent<BossVisualEffects>();
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
        }
    }
    
    private void Start()
    {
        currentState = BossFightState.Intro;
        Debug.Log("Bobbdra initialized in Intro state - waiting for boss fight to start");
        
        StartCoroutine(RiseFromBelow());
    }
    
    private IEnumerator RiseFromBelow()
    {
        Vector3 spawnPosition = transform.position;
        Vector3 startPosition = spawnPosition + Vector3.down * introRiseDistance;
        transform.position = startPosition;
        
        Debug.Log($"Bobbdra: Starting rise from {startPosition.y} to {spawnPosition.y}");
        
        yield return new WaitForSeconds(1.5f);
        
        float elapsedTime = 0f;
        while (elapsedTime < introRiseDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / introRiseDuration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPosition, spawnPosition, easedT);
            yield return null;
        }
        
        transform.position = spawnPosition;
        Debug.Log("Bobbdra: Rise complete, waiting for fight to start");
    }
    
    public void StartBossFight()
    {
        if (currentState == BossFightState.Intro)
        {
            StartCoroutine(IntroSequence());
        }
    }
    
    private IEnumerator IntroSequence()
    {
        Debug.Log("Bobbdra: Starting intro sequence");
        
        if (roarClip != null)
        {
            Debug.Log("Bobbdra: Playing roar animation on all heads");
            yield return StartCoroutine(PlayRoarOnAllHeads());
        }
        else
        {
            Debug.LogWarning("Bobbdra: Roar animation clip not assigned");
        }
        
        Debug.Log("Bobbdra: Entering combat state");
        currentState = BossFightState.Combat;
        attackTimer = 0f;
        
        if (annoyingHeadPrefab != null)
        {
            StartCoroutine(SpawnAnnoyingHead());
        }
    }
    
    private IEnumerator PlayRoarOnAllHeads()
    {
        for (int i = 0; i < heads.Length; i++)
        {
            if (heads[i] != null)
            {
                heads[i].PrepareForAttack();
            }
        }
        
        for (int i = 0; i < heads.Length; i++)
        {
            if (heads[i] != null)
            {
                heads[i].PlayRoarAnimation(roarClip);
            }
        }
        
        float roarDuration = roarClip.length;
        Debug.Log($"Bobbdra: Waiting for roar animation to complete ({roarDuration} seconds)");
        
        yield return new WaitForSeconds(roarDuration);
        
        for (int i = 0; i < heads.Length; i++)
        {
            if (heads[i] != null)
            {
                heads[i].OnRoarComplete();
            }
        }
        
        Debug.Log("Bobbdra: Roar animation complete");
    }
    
    private void Update()
    {
        if (currentState == BossFightState.Combat && !isExecutingPattern)
        {
            attackTimer += Time.deltaTime;
            
            float currentCooldown = GetCurrentAttackCooldown();
            if (attackTimer >= currentCooldown)
            {
                SelectNextAttackPattern();
                attackTimer = 0f;
            }
        }
    }
    
    private void SelectNextAttackPattern()
    {
        if (isExecutingPattern || attackPatterns == null || attackPatterns.Length == 0)
        {
            return;
        }
        
        int patternIndex = GetNextPatternIndex();
        BobbdraAttackPattern selectedPattern = attackPatterns[patternIndex];
        
        if (selectedPattern != null)
        {
            Debug.Log($"Bobbdra: Executing pattern {patternIndex} ({selectedPattern.GetType().Name})");
            isExecutingPattern = true;
            StartCoroutine(ExecutePatternWithCompletion(selectedPattern));
        }
    }
    
    private int GetNextPatternIndex()
    {
        float[] weights = GetCurrentPhaseWeights();
        
        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
        }
        
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        
        for (int i = 0; i < weights.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue < cumulativeWeight)
            {
                if (i != lastPatternIndex || attackPatterns.Length == 1)
                {
                    lastPatternIndex = i;
                    return i;
                }
            }
        }
        
        int selectedIndex = UnityEngine.Random.Range(0, attackPatterns.Length);
        while (selectedIndex == lastPatternIndex && attackPatterns.Length > 1)
        {
            selectedIndex = UnityEngine.Random.Range(0, attackPatterns.Length);
        }
        lastPatternIndex = selectedIndex;
        return selectedIndex;
    }
    
    private float[] GetCurrentPhaseWeights()
    {
        float[] weights = new float[4];
        
        if (currentPhaseLevel >= 3)
        {
            weights[0] = phase3SequentialBiteWeight;
            weights[1] = phase3ProjectileBarrageWeight;
            weights[2] = phase3SideAmbushWeight;
            weights[3] = phase3DownAttackWeight;
        }
        else if (currentPhaseLevel >= 2)
        {
            weights[0] = phase2SequentialBiteWeight;
            weights[1] = phase2ProjectileBarrageWeight;
            weights[2] = phase2SideAmbushWeight;
            weights[3] = phase2DownAttackWeight;
        }
        else
        {
            weights[0] = phase1SequentialBiteWeight;
            weights[1] = phase1ProjectileBarrageWeight;
            weights[2] = phase1SideAmbushWeight;
            weights[3] = phase1DownAttackWeight;
        }
        
        return weights;
    }
    
    private IEnumerator ExecutePatternWithCompletion(BobbdraAttackPattern pattern)
    {
        pattern.ExecutePattern();
        
        yield return new WaitUntil(() => pattern.IsPatternComplete());
        
        isExecutingPattern = false;
    }
    
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, transform.position);
    }
    
    public void TakeDamage(float damage, Vector3 hitPosition)
    {
        TakeDamage(damage, hitPosition, false);
    }
    
    public void TakeDamage(float damage, Vector3 hitPosition, bool bypassInvincibility)
    {
        if (currentState == BossFightState.Death)
        {
            return;
        }
        
        if (isInvincible && !bypassInvincibility)
        {
            Debug.Log("Bobbdra is invincible - damage ignored");
            return;
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);
        
        Debug.Log($"Bobbdra took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioEventType.BossDamage, hitPosition);
        }
        
        StartCoroutine(HitFeedback());
        
        if (visualEffects != null)
        {
            visualEffects.OnBossHit(hitPosition, false);
        }
        
        CheckPhaseTransition();
        
        if (currentHealth <= 0f)
        {
            StartDeathSequence();
        }
    }
    
    public void SetHealthBarUI(BossHealthBarUI ui)
    {
        healthBarUI = ui;
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth);
        }
    }
    
    private IEnumerator HitFeedback()
    {
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
            float offsetX = UnityEngine.Random.Range(-hitShakeMagnitude, hitShakeMagnitude);
            float offsetY = UnityEngine.Random.Range(-hitShakeMagnitude, hitShakeMagnitude);
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
        
        if (currentPhaseLevel == 1 && healthPercent <= phase2HealthThreshold)
        {
            TransitionToPhase(2);
        }
        else if (currentPhaseLevel == 2 && healthPercent <= phase3HealthThreshold)
        {
            TransitionToPhase(3);
        }
    }
    
    private void TransitionToPhase(int newPhase)
    {
        currentPhaseLevel = newPhase;
        Debug.Log($"Bobbdra transitioning to Phase {newPhase}");
        
        if (visualEffects != null)
        {
            visualEffects.OnPhaseTransition(newPhase);
        }
    }
    
    private void StartDeathSequence()
    {
        currentState = BossFightState.Death;
        Debug.Log("Bobbdra defeated!");
        
        StopAllAttacks();
        
        StartCoroutine(DeathSequence());
    }
    
    private void StopAllAttacks()
    {
        Debug.Log("Stopping all active attack patterns");
        
        for (int i = 0; i < attackPatterns.Length; i++)
        {
            if (attackPatterns[i] != null)
            {
                attackPatterns[i].StopPattern();
            }
        }
        
        for (int i = 0; i < heads.Length; i++)
        {
            if (heads[i] != null)
            {
                heads[i].StopAllAttacks();
            }
        }
        
        isExecutingPattern = false;
    }
    
    private IEnumerator DeathSequence()
    {
        if (spawnedAnnoyingHead != null)
        {
            spawnedAnnoyingHead.Die();
        }
        
        if (deathSequence != null)
        {
            deathSequence.PlayDeathSequence();
            
            float outerDeathTime = outerDeathsClip != null ? outerDeathsClip.length : 1.5f;
            float centerDeathTime = centerDeathClip != null ? centerDeathClip.length : 2f;
            float sinkTime = 25f / 10f;
            
            float totalAnimationTime = outerDeathTime + centerDeathTime + sinkTime;
            
            yield return new WaitForSeconds(totalAnimationTime);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }
        
        if (centerHead != null)
        {
            SpawnHeart();
        }
        
        yield return new WaitForSeconds(1.5f);
        
        OnBobbdraDefeated?.Invoke();
        
        Destroy(gameObject);
    }
    
    private void SpawnHeart()
    {
        if (heartPrefab != null && centerHead != null)
        {
            Vector3 spawnPosition = centerHead.transform.position;
            GameObject heart = Instantiate(heartPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("Bobbdra: Heart spawned");
        }
        else
        {
            Debug.LogWarning("Bobbdra: Heart prefab not assigned");
        }
    }
    
    private float GetCurrentAttackCooldown()
    {
        float cooldownModifier = 1f;
        
        if (currentPhaseLevel == 2)
        {
            cooldownModifier = 0.75f;
        }
        else if (currentPhaseLevel == 3)
        {
            cooldownModifier = 0.5f;
        }
        
        return baseAttackCooldown * cooldownModifier;
    }
    
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        Debug.Log($"Bobbdra invincibility set to: {invincible}");
    }
    
    private IEnumerator SpawnAnnoyingHead()
    {
        yield return new WaitForSeconds(annoyingHeadSpawnDelay);
        
        if (annoyingHeadPrefab != null && centerHead != null)
        {
            Vector3 spawnPosition = centerHead.transform.position;
            spawnPosition.y = transform.position.y;
            
            GameObject annoyingHeadObj = Instantiate(annoyingHeadPrefab, spawnPosition, Quaternion.identity);
            spawnedAnnoyingHead = annoyingHeadObj.GetComponent<AnnoyingHeadController>();
            
            if (spawnedAnnoyingHead != null)
            {
                spawnedAnnoyingHead.Initialize(spawnPosition, this);
            }
            
            Debug.Log("AnnoyingHead spawned at bottom of arena");
        }
        else
        {
            Debug.LogWarning("Cannot spawn AnnoyingHead: prefab or centerHead not assigned");
        }
    }
    
    public bool IsDead()
    {
        return currentState == BossFightState.Death;
    }
}
